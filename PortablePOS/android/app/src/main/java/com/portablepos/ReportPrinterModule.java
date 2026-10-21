package com.portablepos;

import android.app.Activity;
import android.content.Context;
import android.os.Bundle;
import android.os.CancellationSignal;
import android.os.ParcelFileDescriptor;
import android.print.PageRange;
import android.print.PrintAttributes;
import android.print.PrintDocumentAdapter;
import android.print.PrintDocumentInfo;
import android.print.PrintManager;
import android.util.Base64;

import androidx.annotation.NonNull;

import com.facebook.react.bridge.Promise;
import com.facebook.react.bridge.ReactApplicationContext;
import com.facebook.react.bridge.ReactContextBaseJavaModule;
import com.facebook.react.bridge.ReactMethod;

import java.io.File;
import java.io.FileInputStream;
import java.io.FileOutputStream;
import java.io.InputStream;
import java.io.OutputStream;

// Prints the reports made by the web service (Crystal Reports exported to PDF) with the print service of Android :
// the print dialog of the system opens, the user picks any printer the device can reach (Wi-Fi, Bluetooth through the
// plugin of the printer, Mopria), A4 by default
public class ReportPrinterModule extends ReactContextBaseJavaModule {

    ReportPrinterModule(ReactApplicationContext context) {
        super(context);
    }

    @NonNull
    @Override
    public String getName() {
        return "ReportPrinter";
    }

    // pdfBase64 : the PDF of the report, jobName : the name of the print job (shown in the dialog and the print queue)
    // Resolves once the print dialog is open, the printing itself goes on in the print service
    @ReactMethod
    public void printPdf(String pdfBase64, String jobName, Promise promise) {
        final Activity activity = getCurrentActivity();
        if (activity == null) {
            promise.reject("Print", "The App Is Not Open");
            return;
        }

        final File pdfFile;
        try {
            pdfFile = new File(getReactApplicationContext().getCacheDir(), "report_print.pdf");
            try (OutputStream output = new FileOutputStream(pdfFile)) {
                output.write(Base64.decode(pdfBase64, Base64.DEFAULT));
            }
        } catch (Exception e) {
            promise.reject("Print", "Cannot Save The Report : " + e.getMessage());
            return;
        }

        activity.runOnUiThread(() -> {
            try {
                PrintManager printManager = (PrintManager) activity.getSystemService(Context.PRINT_SERVICE);
                PrintAttributes attributes = new PrintAttributes.Builder()
                        .setMediaSize(PrintAttributes.MediaSize.ISO_A4)
                        .build();
                printManager.print(jobName, new PdfDocumentAdapter(pdfFile, jobName), attributes);
                promise.resolve(true);
            } catch (Exception e) {
                promise.reject("Print", e.getMessage());
            }
        });
    }

    // Gives the PDF as it is to the print service (it is already laid out by the report)
    private static class PdfDocumentAdapter extends PrintDocumentAdapter {

        private final File pdfFile;
        private final String name;

        PdfDocumentAdapter(File pdfFile, String name) {
            this.pdfFile = pdfFile;
            this.name = name;
        }

        @Override
        public void onLayout(PrintAttributes oldAttributes, PrintAttributes newAttributes, CancellationSignal cancellationSignal,
                             LayoutResultCallback callback, Bundle extras) {
            if (cancellationSignal.isCanceled()) {
                callback.onLayoutCancelled();
                return;
            }
            PrintDocumentInfo info = new PrintDocumentInfo.Builder(name + ".pdf")
                    .setContentType(PrintDocumentInfo.CONTENT_TYPE_DOCUMENT)
                    .setPageCount(PrintDocumentInfo.PAGE_COUNT_UNKNOWN)
                    .build();
            callback.onLayoutFinished(info, true);
        }

        @Override
        public void onWrite(PageRange[] pages, ParcelFileDescriptor destination, CancellationSignal cancellationSignal,
                            WriteResultCallback callback) {
            try (InputStream input = new FileInputStream(pdfFile);
                 OutputStream output = new FileOutputStream(destination.getFileDescriptor())) {
                byte[] buffer = new byte[16384];
                int read;
                while ((read = input.read(buffer)) > 0) {
                    if (cancellationSignal.isCanceled()) {
                        callback.onWriteCancelled();
                        return;
                    }
                    output.write(buffer, 0, read);
                }
                callback.onWriteFinished(new PageRange[]{PageRange.ALL_PAGES});
            } catch (Exception e) {
                callback.onWriteFailed(e.getMessage());
            }
        }
    }
}
