using System;
using System.Collections.Generic;
using System.Data;
using System.Data.SqlClient;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Seagull.BarTender.Print;
using Seagull.BarTender.Print.Database;

namespace WinApplications
{

    // Password : Ct$5125

    public static class ClsBartender
    {

        public class LabelItem
        {
            public string Barcode { get; set; }
            public string Reference { get; set; }
            public string Unit { get; set; }
            public string Description { get; set; }
            public string PriceLC { get; set; }
            public string CurrencyLC { get; set; }
            public string PriceCC { get; set; }
            public string CurrencyCC { get; set; }
            public string Volume { get; set; }
            public string Weight { get; set; }
        }

        public static void PrintLabel(string documentPath, LabelItem labelItem, string printerName, int qtyToPrint)
        {

            if (!File.Exists(documentPath))
            {
                throw new Exception($@"Invalid or Inaccessible Document ({documentPath})");
            }

            using (Engine btEngine = new Engine(true))
            {

                LabelFormatDocument labelFormatDocument = btEngine.Documents.Open(documentPath);

                SetNamedDataSourceValue(labelFormatDocument, "Barcode", labelItem.Barcode);
                SetNamedDataSourceValue(labelFormatDocument, "BarcodeType", GetBarcodeType(labelItem.Barcode));
                SetNamedDataSourceValue(labelFormatDocument, "Reference", labelItem.Reference);
                SetNamedDataSourceValue(labelFormatDocument, "Unit", labelItem.Unit);
                SetNamedDataSourceValue(labelFormatDocument, "Description", labelItem.Description);
                SetNamedDataSourceValue(labelFormatDocument, "PriceLC", labelItem.PriceLC);
                SetNamedDataSourceValue(labelFormatDocument, "CurrencyLC", labelItem.CurrencyLC);
                SetNamedDataSourceValue(labelFormatDocument, "PriceCC", labelItem.PriceCC);
                SetNamedDataSourceValue(labelFormatDocument, "CurrencyCC", labelItem.CurrencyCC);
                SetNamedDataSourceValue(labelFormatDocument, "Volume", labelItem.Volume);
                SetNamedDataSourceValue(labelFormatDocument, "Weight", labelItem.Weight);

                _ = !string.IsNullOrEmpty(printerName) ? labelFormatDocument.PrintSetup.PrinterName = printerName : string.Empty;
                labelFormatDocument.PrintSetup.NumberOfSerializedLabels = qtyToPrint;

                Messages messages;

                int result = (int)labelFormatDocument.Print("Label Design", 1, out messages); // 1 copy

                labelFormatDocument.Close(SaveOptions.DoNotSaveChanges);

                if (!(result != 0))
                {
                    throw new Exception("Print failed. Error: " + messages.ToString());
                }
            }
        }

        private static void SetNamedDataSourceValue(LabelFormatDocument labelFormatDocument, string name, string value)
        {
            if (labelFormatDocument.SubStrings
                         .Cast<SubString>()
                         .Any(s => s.Name.Equals(name, StringComparison.OrdinalIgnoreCase)))
            {
                labelFormatDocument.SubStrings[name].Value = value;
            }
        }

        private static string GetBarcodeType(string barcode)
        {
            if (string.IsNullOrWhiteSpace(barcode))
                return "UNKNOWN";

            // Remove whitespace
            barcode = barcode.Trim();

            // Check if it's all digits
            bool isNumeric = barcode.All(char.IsDigit);

            // UPC-A: 12 digits
            if (isNumeric && barcode.Length == 12 && IsUPCA(barcode))
            {
                return "UPCA";
            }

            // EAN-13: 13 digits
            if (isNumeric && barcode.Length == 13)
                if (IsEAN13(barcode))
                {
                    return "EAN13";
                }
                else
                {
                    if (IsUPCA(barcode))
                    {
                        return "UPCA";
                    }
                }

            // EAN-8: 8 digits
            if (isNumeric && barcode.Length == 8)
            {
                return "EAN8";
            }

            // Code 128: Any length, allows full ASCII (fallback option)
            if (barcode.Any(c => c > 127))
            {
                return "INVALID";
            }

            // Code 39: Digits, uppercase letters, some symbols (- . $ / + % space)
            if (barcode.All(c => char.IsLetterOrDigit(c) || "-.$/+% ".Contains(c)) && barcode.Length <= 43)
            {
                return "CODE39";
            }

            // Default fallback
            return "CODE128";

        }

        public static bool IsEAN13(string rawString)
        {
            try
            {
                if (rawString.Length != 13 || !rawString.All(char.IsDigit))
                    return false;

                int checkSum = 0;

                for (int i = 1; i < 12; i += 2)
                {
                    checkSum += int.Parse(rawString[i].ToString());
                }

                checkSum *= 3;

                for (int i = 0; i < 12; i += 2)
                {
                    checkSum += int.Parse(rawString[i].ToString());
                }

                int calculatedCheckDigit = 10 - (checkSum % 10);
                if (calculatedCheckDigit == 10)
                    calculatedCheckDigit = 0;

                int actualCheckDigit = int.Parse(rawString[12].ToString());

                return calculatedCheckDigit == actualCheckDigit;
            }
            catch (Exception)
            {
                return false;
            }
        }

        public static bool IsUPCA(string barcode)
        {
            try
            {
                if (barcode.Length != 12)
                    return true;

                long[] nums = new long[12];
                for (int i = 11; i >= 0; i--)
                {
                    nums[i] = Convert.ToInt64(barcode.Substring(i, 1));
                }

                long lastDigit = 0;
                lastDigit += (nums[10] * 3); // position 11
                lastDigit += (nums[9] * 1);  // position 10
                lastDigit += (nums[8] * 3);  // position 9
                lastDigit += (nums[7] * 1);  // position 8
                lastDigit += (nums[6] * 3);  // position 7
                lastDigit += (nums[5] * 1);  // position 6
                lastDigit += (nums[4] * 3);  // position 5
                lastDigit += (nums[3] * 1);  // position 4
                lastDigit += (nums[2] * 3);  // position 3
                lastDigit += (nums[1] * 1);  // position 2
                lastDigit += (nums[0] * 3);  // position 1 (leftmost digit)

                lastDigit %= 10;
                if (lastDigit != 0)
                    lastDigit = 10 - lastDigit;

                return lastDigit == nums[11];

            }
            catch (Exception ex)
            {
                return false;
            }
        }

    }
}
