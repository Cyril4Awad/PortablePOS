Imports System.Drawing
Imports System.IO
Imports Seagull.BarTender.Print

'Prints the BarTender label designs (.btw), same named data sources as ClsBartender of the Windows modules
'The design reads its values from the named data sources Barcode, Reference... the ones missing in a design are skipped
Public Class ClsBartender

    Public Class LabelItem
        Public Property Barcode As String
        Public Property Reference As String
        Public Property Unit As String
        Public Property Description As String
        Public Property PriceLC As String
        Public Property CurrencyLC As String
        Public Property PriceCC As String
        Public Property CurrencyCC As String
        Public Property Volume As String
        Public Property Weight As String
        'Link of the item (LabelQRDefaultPath/record id), for the QR of the design bound to QR
        Public Property QR As String
    End Class

    'One BarTender engine at a time : two devices printing together wait for each other
    Private Shared ReadOnly PrintLock As New Object

    'Waits for the spooler to report the job printed, the device keeps waiting until then
    Private Const PrintWaitMilliseconds As Integer = 10000

    'Width of the CPCL page (80 mm, 8 dots per mm), the labels and the test ruler alike : a mobile printer centers a page narrower
    'than its print head, the page is always this wide so the label lands where the ruler shows (offsetX = edge of the sticker on the ruler)
    Public Const PageWidthDots As Integer = 80 * 8

    'printerName : empty prints on the printer saved in the design
    Public Shared Sub PrintLabel(documentPath As String, labelItem As LabelItem, printerName As String, qtyToPrint As Integer)

        UseFilledLabel(documentPath, labelItem,
            Sub(labelFormatDocument)

                If Not String.IsNullOrEmpty(printerName) Then
                    labelFormatDocument.PrintSetup.PrinterName = printerName
                End If
                labelFormatDocument.PrintSetup.NumberOfSerializedLabels = qtyToPrint

                Dim messages As Messages = Nothing
                Dim result As Result = labelFormatDocument.Print("Label Design", PrintWaitMilliseconds, messages)

                'The Windows class threw when the print succeeded (Result.Success = 0)
                'Timeout : the job is in the spooler but the driver did not report it done in time (many drivers never do), not an error
                If result = Result.Failure Then
                    Throw New Exception("Print Failed : " & PrintMessages(messages))
                End If

            End Sub)

    End Sub

    'Image (PNG) of one label filled like the print, nothing is sent to a printer
    Public Shared Function PreviewLabel(documentPath As String, labelItem As LabelItem) As Byte()

        Return LabelImage(documentPath, labelItem, 200)

    End Function

    'CPCL commands of the label for a printer of the device (Bluetooth) : the label made by BarTender, as a black and white image,
    'qty copies, each one starting on the next sticker (FORM : label mode)
    'The size is the size of the label in the design (BarTender Page Setup), at the resolution of the printers (8 dots per mm, 203 dpi) :
    'each design is made at the size of the stickers it is printed on
    'offsetX : moves the image to the right when the print head starts before the sticker
    Public Shared Function CpclLabel(documentPath As String, labelItem As LabelItem, qty As Integer, offsetXDots As Integer) As Byte()

        Dim png As Byte() = LabelImage(documentPath, labelItem, 203)

        Using designImage As New Bitmap(New MemoryStream(png))
            Return CpclImage(designImage, qty, offsetXDots)
        End Using

    End Function

    'Test print to set up a printer : a ruler with a mark every mm (long every 5, numbered every 10) from the first dot of the
    'print head, wider than the stickers. Where the edges of the sticker fall on the ruler gives LabelOffsetXMM
    Public Shared Function CpclRuler(widthDots As Integer, heightDots As Integer) As Byte()

        Using ruler As New Bitmap(widthDots, heightDots, Imaging.PixelFormat.Format32bppArgb)

            Using graphic As Graphics = Graphics.FromImage(ruler),
                  font As New Font("Arial", 18, FontStyle.Bold, GraphicsUnit.Pixel)

                graphic.Clear(Color.White)
                graphic.TextRenderingHint = Text.TextRenderingHint.SingleBitPerPixelGridFit

                For mm As Integer = 0 To widthDots \ 8
                    Dim x As Integer = mm * 8
                    Dim length As Integer = If(mm Mod 10 = 0, 80, If(mm Mod 5 = 0, 55, 30))
                    graphic.FillRectangle(Brushes.Black, x, 0, 2, length)
                    If mm Mod 10 = 0 Then graphic.DrawString(mm.ToString, font, Brushes.Black, x + 3, 82)
                Next

                graphic.FillRectangle(Brushes.Black, 0, heightDots \ 2, widthDots, 2)
                graphic.DrawString("mm from the first dot of the print head", font, Brushes.Black, 20, heightDots \ 2 + 12)

            End Using

            Return CpclImage(ruler, 1, 0)

        End Using

    End Function

    'The image as a black and white CPCL graphic (dot for dot, 8 dots per mm), qty copies, each one on the next sticker (FORM)
    Private Shared Function CpclImage(designImage As Bitmap, qty As Integer, offsetXDots As Integer) As Byte()

        Dim widthDots As Integer = designImage.Width
        Dim heightDots As Integer = designImage.Height
        Dim rowBytes As Integer = (widthDots + 7) \ 8
        Dim bits(rowBytes * heightDots - 1) As Byte

        'Drawn on white : the parts of the design without background are blank on the sticker
        Using stickerImage As New Bitmap(widthDots, heightDots, Imaging.PixelFormat.Format32bppArgb)

            'Dot for dot : DrawImageUnscaled would resize by the resolutions (203 dpi of BarTender, 96 dpi of the bitmap)
            Using graphic As Graphics = Graphics.FromImage(stickerImage)
                graphic.Clear(Color.White)
                graphic.DrawImage(designImage, New Rectangle(0, 0, widthDots, heightDots), New Rectangle(0, 0, widthDots, heightDots), GraphicsUnit.Pixel)
            End Using

            Dim data As Imaging.BitmapData = stickerImage.LockBits(New Rectangle(0, 0, widthDots, heightDots), Imaging.ImageLockMode.ReadOnly, Imaging.PixelFormat.Format32bppArgb)
            Try
                Dim pixels(data.Stride * heightDots - 1) As Byte
                Runtime.InteropServices.Marshal.Copy(data.Scan0, pixels, 0, pixels.Length)

                For y As Integer = 0 To heightDots - 1
                    For x As Integer = 0 To widthDots - 1
                        Dim p As Integer = y * data.Stride + x * 4 'B G R A
                        Dim luminance As Integer = (pixels(p + 2) * 299 + pixels(p + 1) * 587 + pixels(p) * 114) \ 1000
                        If luminance < 128 Then
                            'CPCL : 1 = black dot, the first dot is the high bit
                            bits(y * rowBytes + x \ 8) = bits(y * rowBytes + x \ 8) Or CByte(&H80 >> (x Mod 8))
                        End If
                    Next
                Next
            Finally
                stickerImage.UnlockBits(data)
            End Try

        End Using

        'CG : the image in binary, half the size of EG (hex text), the Bluetooth sending is the slow part of the print
        Using cpcl As New MemoryStream

            Dim WriteText = Sub(text As String)
                                Dim textBytes As Byte() = System.Text.Encoding.ASCII.GetBytes(text)
                                cpcl.Write(textBytes, 0, textBytes.Length)
                            End Sub

            WriteText($"! 0 200 200 {heightDots} {qty}" & vbCrLf)
            WriteText($"PAGE-WIDTH {Math.Max(PageWidthDots, widthDots + offsetXDots)}" & vbCrLf)
            WriteText($"CG {rowBytes} {heightDots} {offsetXDots} 0 ")
            cpcl.Write(bits, 0, bits.Length)
            WriteText(vbCrLf)
            WriteText("FORM" & vbCrLf)
            WriteText("PRINT" & vbCrLf)

            Return cpcl.ToArray

        End Using

    End Function

    Private Shared Function LabelImage(documentPath As String, labelItem As LabelItem, dpi As Integer) As Byte()

        Dim imagePath As String = Path.Combine(Path.GetTempPath(), "Label_" & Guid.NewGuid().ToString("N") & ".png")

        Try

            UseFilledLabel(documentPath, labelItem,
                Sub(labelFormatDocument)
                    labelFormatDocument.ExportImageToFile(imagePath, ImageType.PNG, ColorDepth.ColorDepth24bit, New Resolution(dpi), OverwriteOptions.Overwrite)
                End Sub)

            Return File.ReadAllBytes(imagePath)

        Finally
            If File.Exists(imagePath) Then File.Delete(imagePath)
        End Try

    End Function

    'Opens a copy of the design : the design itself can stay open in BarTender Designer while the devices print
    '(opened directly, the hidden engine waits on a "file in use" question nobody can answer, and every print waits behind it)
    Private Shared Sub UseFilledLabel(documentPath As String, labelItem As LabelItem, action As Action(Of LabelFormatDocument))

        If Not File.Exists(documentPath) Then
            Throw New Exception($"Invalid or Inaccessible Document ({documentPath})")
        End If

        Dim copyPath As String = Path.Combine(Path.GetTempPath(), "Label_" & Guid.NewGuid().ToString("N") & ".btw")

        Try

            'Read even while Designer has the file open
            Using source As New FileStream(documentPath, FileMode.Open, FileAccess.Read, FileShare.ReadWrite Or FileShare.Delete),
                  target As New FileStream(copyPath, FileMode.CreateNew, FileAccess.Write)
                source.CopyTo(target)
            End Using

            SyncLock PrintLock

                Using btEngine As New Engine(True)

                    Dim labelFormatDocument As LabelFormatDocument = btEngine.Documents.Open(copyPath)

                    Try
                        FillLabel(labelFormatDocument, labelItem)
                        action(labelFormatDocument)
                    Finally
                        labelFormatDocument.Close(SaveOptions.DoNotSaveChanges)
                    End Try

                End Using

            End SyncLock

        Finally
            If File.Exists(copyPath) Then File.Delete(copyPath)
        End Try

    End Sub

    Private Shared Sub FillLabel(labelFormatDocument As LabelFormatDocument, labelItem As LabelItem)

        SetNamedDataSourceValue(labelFormatDocument, "Barcode", labelItem.Barcode)
        SetNamedDataSourceValue(labelFormatDocument, "BarcodeType", GetBarcodeType(labelItem.Barcode))
        SetNamedDataSourceValue(labelFormatDocument, "Reference", labelItem.Reference)
        SetNamedDataSourceValue(labelFormatDocument, "Unit", labelItem.Unit)
        SetNamedDataSourceValue(labelFormatDocument, "Description", labelItem.Description)
        SetNamedDataSourceValue(labelFormatDocument, "PriceLC", labelItem.PriceLC)
        SetNamedDataSourceValue(labelFormatDocument, "CurrencyLC", labelItem.CurrencyLC)
        SetNamedDataSourceValue(labelFormatDocument, "PriceCC", labelItem.PriceCC)
        SetNamedDataSourceValue(labelFormatDocument, "CurrencyCC", labelItem.CurrencyCC)
        SetNamedDataSourceValue(labelFormatDocument, "Volume", labelItem.Volume)
        SetNamedDataSourceValue(labelFormatDocument, "Weight", labelItem.Weight)
        SetNamedDataSourceValue(labelFormatDocument, "QR", labelItem.QR)

    End Sub

    Private Shared Function PrintMessages(messages As Messages) As String

        If messages Is Nothing Then Return ""

        Dim texts As New List(Of String)
        For Each message As Message In messages
            texts.Add(message.Text)
        Next

        Return String.Join(" | ", texts)

    End Function

    Private Shared Sub SetNamedDataSourceValue(labelFormatDocument As LabelFormatDocument, name As String, value As String)

        If labelFormatDocument.SubStrings.Cast(Of SubString)().Any(Function(s) s.Name.Equals(name, StringComparison.OrdinalIgnoreCase)) Then
            labelFormatDocument.SubStrings(name).Value = If(value, "")
        End If

    End Sub

    Private Shared Function GetBarcodeType(barcode As String) As String

        If String.IsNullOrWhiteSpace(barcode) Then Return "UNKNOWN"

        barcode = barcode.Trim

        Dim isNumeric As Boolean = barcode.All(Function(c) Char.IsDigit(c))

        'UPC-A : 12 digits
        If isNumeric AndAlso barcode.Length = 12 AndAlso IsUPCA(barcode) Then Return "UPCA"

        'EAN-13 : 13 digits
        If isNumeric AndAlso barcode.Length = 13 Then
            If IsEAN13(barcode) Then
                Return "EAN13"
            ElseIf IsUPCA(barcode) Then
                Return "UPCA"
            End If
        End If

        'EAN-8 : 8 digits
        If isNumeric AndAlso barcode.Length = 8 Then Return "EAN8"

        If barcode.Any(Function(c) AscW(c) > 127) Then Return "INVALID"

        'Code 39 : digits, uppercase letters, some symbols (- . $ / + % space)
        If barcode.All(Function(c) Char.IsLetterOrDigit(c) OrElse "-.$/+% ".Contains(c)) AndAlso barcode.Length <= 43 Then Return "CODE39"

        Return "CODE128"

    End Function

    Public Shared Function IsEAN13(rawString As String) As Boolean

        Try

            If rawString.Length <> 13 OrElse Not rawString.All(Function(c) Char.IsDigit(c)) Then Return False

            Dim checkSum As Integer = 0

            For i As Integer = 1 To 11 Step 2
                checkSum += CInt(rawString(i).ToString)
            Next

            checkSum *= 3

            For i As Integer = 0 To 11 Step 2
                checkSum += CInt(rawString(i).ToString)
            Next

            Dim calculatedCheckDigit As Integer = 10 - (checkSum Mod 10)
            If calculatedCheckDigit = 10 Then calculatedCheckDigit = 0

            Return calculatedCheckDigit = CInt(rawString(12).ToString)

        Catch ex As Exception
            Return False
        End Try

    End Function

    'Same as the Windows class : a barcode that is not 12 long is accepted
    Public Shared Function IsUPCA(barcode As String) As Boolean

        Try

            If barcode.Length <> 12 Then Return True

            Dim nums(11) As Long
            For i As Integer = 11 To 0 Step -1
                nums(i) = Convert.ToInt64(barcode.Substring(i, 1))
            Next

            'Odd positions (from the left, 1 based) * 3, even positions * 1
            Dim lastDigit As Long = 0
            For i As Integer = 0 To 10
                lastDigit += nums(i) * If(i Mod 2 = 0, 3, 1)
            Next

            lastDigit = lastDigit Mod 10
            If lastDigit <> 0 Then lastDigit = 10 - lastDigit

            Return lastDigit = nums(11)

        Catch ex As Exception
            Return False
        End Try

    End Function

End Class
