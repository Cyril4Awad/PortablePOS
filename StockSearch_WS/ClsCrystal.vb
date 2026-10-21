Imports System.Data.SqlClient
Imports System.IO
Imports CrystalDecisions.CrystalReports.Engine
Imports CrystalDecisions.Shared

'Fills the Crystal Reports of the ERP (.rpt of POSOReports) and exports them to PDF (the device prints it with Android print)
'or prints them on a printer of the server
'Same filling as PreviewProcedure / LoadSubReports of the ERP (POSOperations ModPrint) : database logon on every table,
'@strsql = the query of the report, the rptParam values in the formula fields, the subreports their own parameters and formulas
Public Class ClsCrystal

    Public Class SubReportValues
        Public Property Name As String
        'Parameter name (@PayTermsStr...) -> value (a query)
        Public Property Parameters As New Dictionary(Of String, String)
        'Formula name -> formula text ('text' with its quotes, numbers as they are)
        Public Property Formulas As New Dictionary(Of String, String)
    End Class

    'reportPath : full path of the .rpt
    'reportSql : the query given to @strsql (select * from the view of the report)
    'formulas : formula name -> formula text, the ones missing in the report are skipped (layouts of the clients can have fewer)
    'companyLogoSql : the query of @strCompanyLogo for the reports that show the logo, "" when the report has none
    Public Shared Function ExportPdf(reportPath As String, connectionString As String, reportSql As String,
                                     formulas As Dictionary(Of String, String), subReports As List(Of SubReportValues),
                                     Optional companyLogoSql As String = "") As Byte()

        Dim pdfBytes As Byte() = Nothing

        UseFilledReport(reportPath, connectionString, reportSql, formulas, subReports, companyLogoSql,
                        Sub(rpt)
                            Using pdf As Stream = rpt.ExportToStream(ExportFormatType.PortableDocFormat)
                                Using memory As New MemoryStream
                                    pdf.CopyTo(memory)
                                    pdfBytes = memory.ToArray
                                End Using
                            End Using
                        End Sub)

        Return pdfBytes

    End Function

    'Same report as ExportPdf, printed on a printer of the server (PrintToPrinter of the ERP direct print)
    'printerName : a printer installed for the Windows user of the service (a shared printer : \\PC\Printer)
    Public Shared Sub PrintReport(reportPath As String, connectionString As String, reportSql As String,
                                  formulas As Dictionary(Of String, String), subReports As List(Of SubReportValues),
                                  printerName As String, copies As Integer, Optional companyLogoSql As String = "")

        UseFilledReport(reportPath, connectionString, reportSql, formulas, subReports, companyLogoSql,
                        Sub(rpt)
                            Dim printerSettings As New Drawing.Printing.PrinterSettings With {.PrinterName = printerName, .Copies = CShort(copies)}
                            If Not printerSettings.IsValid Then Throw New Exception("Printer Not Found On The Server : " & printerName)
                            rpt.PrintToPrinter(printerSettings, New Drawing.Printing.PageSettings(printerSettings), False)
                        End Sub)

    End Sub

    'Loads the report, fills it (same as PreviewProcedure / LoadSubReports of the ERP) and gives it to action
    Private Shared Sub UseFilledReport(reportPath As String, connectionString As String, reportSql As String,
                                       formulas As Dictionary(Of String, String), subReports As List(Of SubReportValues),
                                       companyLogoSql As String, action As Action(Of ReportDocument))

        If Not File.Exists(reportPath) Then Throw New Exception("Report Not Found : " & reportPath)

        Using rpt As New ReportDocument

            Try

                rpt.Load(reportPath)

                Dim connectionInfo As ConnectionInfo = ReportConnectionInfo(connectionString)

                ApplyLogOn(rpt.Database.Tables, connectionInfo)

                If subReports IsNot Nothing Then
                    For Each subReport As SubReportValues In subReports
                        Dim subDocument As ReportDocument = rpt.Subreports(subReport.Name)
                        ApplyLogOn(subDocument.Database.Tables, connectionInfo)
                        For Each parameter In subReport.Parameters
                            rpt.SetParameterValue(parameter.Key, parameter.Value, subReport.Name)
                        Next
                        SetFormulas(subDocument, subReport.Formulas)
                    Next
                End If

                rpt.SetParameterValue("@strsql", reportSql)

                If companyLogoSql <> "" AndAlso HasParameter(rpt, "@strCompanyLogo") Then
                    rpt.SetParameterValue("@strCompanyLogo", companyLogoSql)
                End If

                SetFormulas(rpt, formulas)

                action(rpt)

            Finally
                'Crystal keeps the loaded reports until closed (75 at most per process)
                rpt.Close()
            End Try

        End Using

    End Sub

    'Server, database and login of the report tables : the ones of the web service connection string (SA login or Windows login)
    Private Shared Function ReportConnectionInfo(connectionString As String) As ConnectionInfo

        Dim builder As New SqlConnectionStringBuilder(connectionString)
        Dim connectionInfo As New ConnectionInfo

        With connectionInfo
            .ServerName = builder.DataSource
            .DatabaseName = builder.InitialCatalog
            If builder.IntegratedSecurity Then
                .IntegratedSecurity = True
            Else
                .UserID = builder.UserID
                .Password = REDACTED
            End If
        End With

        Return connectionInfo

    End Function

    Private Shared Sub ApplyLogOn(tables As Tables, connectionInfo As ConnectionInfo)

        For Each table As Table In tables
            Dim logOnInfo As TableLogOnInfo = table.LogOnInfo
            logOnInfo.ConnectionInfo = connectionInfo
            table.ApplyLogOnInfo(logOnInfo)
        Next

    End Sub

    Private Shared Sub SetFormulas(report As ReportDocument, formulas As Dictionary(Of String, String))

        If formulas Is Nothing Then Exit Sub

        For Each formula In formulas
            For Each field As FormulaFieldDefinition In report.DataDefinition.FormulaFields
                If field.FormulaName.Trim("{"c, "}"c, "@"c).Equals(formula.Key, StringComparison.OrdinalIgnoreCase) Then
                    field.Text = formula.Value
                    Exit For
                End If
            Next
        Next

    End Sub

    Private Shared Function HasParameter(report As ReportDocument, name As String) As Boolean

        For Each field As ParameterFieldDefinition In report.DataDefinition.ParameterFields
            If field.Name.Equals(name, StringComparison.OrdinalIgnoreCase) AndAlso field.ReportName = "" Then Return True
        Next

        Return False

    End Function

    'Formula text of a value : 'text' (quotes doubled), the way the ERP fills its rptParam
    Public Shared Function FormulaText(value As String) As String
        Return "'" & If(value, "").Replace("'", "''") & "'"
    End Function

End Class
