package com.portablepos;

import android.bluetooth.BluetoothAdapter;
import android.bluetooth.BluetoothDevice;
import android.bluetooth.BluetoothSocket;
import android.util.Base64;

import androidx.annotation.NonNull;

import com.facebook.react.bridge.Arguments;
import com.facebook.react.bridge.Promise;
import com.facebook.react.bridge.ReactApplicationContext;
import com.facebook.react.bridge.ReactContextBaseJavaModule;
import com.facebook.react.bridge.ReactMethod;
import com.facebook.react.bridge.WritableArray;
import com.facebook.react.bridge.WritableMap;

import java.io.OutputStream;
import java.util.Set;
import java.util.UUID;

// Sends the label printer commands (CPCL made by the web service) as they are to a paired Bluetooth printer.
// The receipt library of the app (react-native-thermal-printer) only speaks ESC/POS, the label printers need their own language.
public class LabelPrinterModule extends ReactContextBaseJavaModule {

    // Serial port profile : the Bluetooth printers receive their commands on it
    private static final UUID SPP_UUID = UUID.fromString("00001101-0000-1000-8000-00805F9B34FB");

    // The small printers lose data when it comes faster than they print it
    private static final int CHUNK_SIZE = 512;

    LabelPrinterModule(ReactApplicationContext context) {
        super(context);
    }

    @NonNull
    @Override
    public String getName() {
        return "LabelPrinter";
    }

    // Resolves with [{ name, address }] of the devices paired in the Bluetooth settings of Android
    @ReactMethod
    public void getPairedDevices(Promise promise) {
        try {
            BluetoothAdapter adapter = BluetoothAdapter.getDefaultAdapter();
            if (adapter == null) {
                promise.reject("Bluetooth", "This Device Has No Bluetooth");
                return;
            }
            if (!adapter.isEnabled()) {
                promise.reject("Bluetooth", "Bluetooth Is Off");
                return;
            }

            WritableArray devices = Arguments.createArray();
            Set<BluetoothDevice> bondedDevices = adapter.getBondedDevices();
            for (BluetoothDevice device : bondedDevices) {
                WritableMap map = Arguments.createMap();
                map.putString("name", device.getName() == null ? device.getAddress() : device.getName());
                map.putString("address", device.getAddress());
                devices.pushMap(map);
            }
            promise.resolve(devices);
        } catch (Exception e) {
            promise.reject("Bluetooth", e.getMessage());
        }
    }

    // data : the printer commands in base 64
    @ReactMethod
    public void print(final String address, final String data, final Promise promise) {

        new Thread(() -> {

            BluetoothSocket socket = null;

            try {
                BluetoothAdapter adapter = BluetoothAdapter.getDefaultAdapter();
                if (adapter == null || !adapter.isEnabled()) {
                    promise.reject("Bluetooth", "Bluetooth Is Off");
                    return;
                }

                byte[] bytes = Base64.decode(data, Base64.DEFAULT);

                BluetoothDevice device = adapter.getRemoteDevice(address);
                adapter.cancelDiscovery();

                // Connect : the printer is off, too far or connected to another device (the app tells it in plain words)
                try {
                    try {
                        socket = device.createRfcommSocketToServiceRecord(SPP_UUID);
                        socket.connect();
                    } catch (Exception secureError) {
                        // Some printers only accept the connection without encryption
                        closeQuietly(socket);
                        socket = device.createInsecureRfcommSocketToServiceRecord(SPP_UUID);
                        socket.connect();
                    }
                } catch (Exception connectError) {
                    promise.reject("Connect", "Cannot connect to the printer : " + connectError.getMessage());
                    return;
                }

                // Send : the connection was lost while the label was sent
                try {
                    OutputStream output = socket.getOutputStream();
                    for (int start = 0; start < bytes.length; start += CHUNK_SIZE) {
                        output.write(bytes, start, Math.min(CHUNK_SIZE, bytes.length - start));
                        output.flush();
                        Thread.sleep(5);
                    }

                    // Closing at once can cut the end of the data still on its way to the printer
                    Thread.sleep(Math.min(2000, 400 + bytes.length / 40));
                } catch (Exception sendError) {
                    promise.reject("Send", "The connection to the printer was lost : " + sendError.getMessage());
                    return;
                }

                promise.resolve(true);

            } catch (Exception e) {
                promise.reject("Bluetooth", "Cannot Print On The Printer : " + e.getMessage());
            } finally {
                closeQuietly(socket);
            }

        }).start();

    }

    private static void closeQuietly(BluetoothSocket socket) {
        try {
            if (socket != null) socket.close();
        } catch (Exception ignored) {
        }
    }
}
