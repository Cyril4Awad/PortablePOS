package com.example.erpdatacollectorn;

import android.content.Context;
import android.telephony.TelephonyManager;
import android.os.Build;

public class DeviceInformation {
    public static String getDeviceInformation(Context context) {
        TelephonyManager telephonyManager = (TelephonyManager) context.getSystemService(Context.TELEPHONY_SERVICE);

        String deviceInfo = "Device Model: " + Build.MODEL + "\n";
        deviceInfo += "Device Manufacturer: " + Build.MANUFACTURER + "\n";
        deviceInfo += "Device Brand: " + Build.BRAND + "\n";
        // You can add more information as needed.

        return deviceInfo;
    }
}
