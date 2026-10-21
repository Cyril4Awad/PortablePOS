Option Explicit Off
Imports CrystalDecisions.CrystalReports.Engine
Imports System.Data.SqlClient
Imports C1.Win.C1TrueDBGrid
Imports System.Text
Imports HosDLL
Imports System.IO
Imports CrystalDecisions.Shared
Imports C1.Win.C1Input
Imports System.Reflection

Module ModPrintRep

    Private pTempSale As String
    Private pTempSale1 As String
    Private PTempSaleD As String
    Private PTempCsh As String
    Private PTempStk As String
    Private ClinetBalnceSumtable As String

    Dim txtpaym(15) As String
    Private isConnected As Boolean
    Private linkServer As String
    Private lcServerLink, lcDBnameLink, lcSQLpswdLink, lcSQLuserLink, lcSQLAuth As String '...Jad-J...WF:4862
    Private ServerName As String 'Name of the linked server used for accounting table acc01 '...Jad-J...WF:4862

    Public ClientBalanceCnxTest As Boolean = False

    Public CurrentInvoiceClAcc As String = ""

    Private DTMails, DtpDf As DataTable

    Public frmSubTransferStatistics As HosDLL.frmTransferStatistics

    Public SernoTemp As String = ""
    '    Public Sub PreviewProcedure(ByVal str As String, ByVal strfil As String, ByVal ReportName As String, ByVal Mee As Form, Optional ByVal rptParam(,) As Object = Nothing, Optional ByVal NbPrint As Integer = 1)

    '        Dim frm As frmPreviewPrint
    '        Dim rptPrev As New ReportDocument
    '        '----------------------
    '        Dim crtableLogoninfos As New TableLogOnInfos
    '        Dim crtableLogoninfo As New TableLogOnInfo
    '        Dim crConnectionInfo As New ConnectionInfo
    '        Dim CrTables As Tables
    '        Dim CrTable As Table
    '        '-------------------------
    '        Dim i As Integer
    '        Dim ExePath As String

    '        ' Objects used to set the parameters in the report
    '        Dim pvCollection As New CrystalDecisions.Shared.ParameterValues
    '        Dim pdvParam As New CrystalDecisions.Shared.ParameterDiscreteValue

    '        ' Objects used to set the proper database connection information
    '        Dim tbCurrent As CrystalDecisions.CrystalReports.Engine.Table
    '        Dim tliCurrent As CrystalDecisions.Shared.TableLogOnInfo
    '        Try
    '            Cursor.Current = Cursors.WaitCursor
    '            ExePath = gbExePath

    '            ' Load the report
    '            If ExePath = gbExePath Then
    '#If DEBUG Then
    '                rptPrev.Load(ExePath & "\..\..\POSOReports\" & ReportName)
    '#Else
    '            rptPrev.Load(gbRepPath & ReportName)
    '#End If
    '            Else
    '                rptPrev.Load(gbRepPath & ReportName)
    '            End If

    '            With crConnectionInfo
    '                .ServerName = gbServer
    '                .DatabaseName = gbDBname
    '                .UserID = "SA"
    '                .Password = REDACTED
    '            End With
    '            CrTables = rptPrev.Database.Tables

    '            For Each CrTable In CrTables
    '                crtableLogoninfo = CrTable.LogOnInfo
    '                crtableLogoninfo.ConnectionInfo = crConnectionInfo
    '                CrTable.ApplyLogOnInfo(crtableLogoninfo)
    '            Next

    '            pdvParam.Value = str
    '            pvCollection.Add(pdvParam)
    '            rptPrev.DataDefinition.ParameterFields("@strsql").ApplyCurrentValues(pvCollection)
    '            pvCollection.Clear()

    '            With rptPrev.DataDefinition
    '                .FormulaFields("fomCompName").Text = "'" & gbCompName.Replace("'","''") & "'"
    '                '.FormulaFields("fomFilter").Text = "'" & strfil & "'"
    '                .FormulaFields("fomuser").Text = "'" & CStr(IIf(UserPrint = "Code", gbUser, gbUserName)) & "'"

    '                If Not rptParam Is Nothing Then
    '                    For i = 0 To rptParam.GetUpperBound(0)
    '                        If Not rptParam(i, 0) Is Nothing AndAlso rptParam(i, 0).ToString <> "" AndAlso Not rptParam(i, 1) Is Nothing Then
    '                            .FormulaFields(rptParam(i, 0).ToString).Text = rptParam(i, 1).ToString
    '                        End If
    '                    Next
    '                End If
    '            End With

    '            'Load Preview form  
    '            frm = New frmPreviewPrint
    '            With frm
    '                .Owner = Mee
    '                .Show(rptPrev, Mee.Text, NbPrint, , True, , True, , , , , , , , False, 100)
    '            End With
    '        Catch Exp As LoadSaveReportException
    '            MessageBox.Show("Incorrect path for loading report.", _
    '                     Mee.Text & " : Load Report Error", MessageBoxButtons.OK, MessageBoxIcon.Error)
    '        Catch Exp As Exception
    '            MessageBox.Show(Exp.Message, Mee.Text & " : General Error", MessageBoxButtons.OK, MessageBoxIcon.Error)
    '        Finally
    '            frm = Nothing
    '            pdvParam = Nothing
    '            tbCurrent = Nothing
    '            tliCurrent = Nothing
    '            pvCollection = Nothing
    '            rptPrev = Nothing
    '        End Try
    '    End Sub
    Public Sub GridPreview(ByRef grdName As C1TrueDBGrid, ByRef tblName As String, ByRef sCaption As String, _
                      Optional ByVal sVatReg As String = "", Optional ByRef sFilter As String = "", _
                      Optional ByRef bLandscape As Boolean = False, Optional ByRef bShowOptionsDialog As Boolean = False)

        Dim i As Integer
        Dim fltStr As String = ""

        Dim sDate, sTime, sAddr, sUser As String

        Dim fntFont As Font

        Dim ds As DataSet

        Try

            With grdName

                If grdName.DataSource.GetType Is GetType(DataView) Then
                    ds = CType(grdName.DataSource, DataView).Table.DataSet
                ElseIf grdName.DataSource.GetType Is GetType(DataSet) Then
                    ds = CType(grdName.DataSource, DataSet)
                End If

                If sFilter <> "" Then fltStr = sFilter

                For i = 0 To ds.Tables(tblName).Columns.Count - 1
                    If .Columns(i).FilterText <> "" Then
                        If fltStr <> "" Then fltStr = fltStr & ","
                        If ds.Tables(tblName).Columns(i).DataType Is GetType(Int16) OrElse _
                           ds.Tables(tblName).Columns(i).DataType Is GetType(Int32) OrElse _
                           ds.Tables(tblName).Columns(i).DataType Is GetType(Int64) Then
                            fltStr = fltStr & .Columns(i).Caption & " >= " & .Columns(i).FilterText + " "
                        Else
                            fltStr = fltStr & .Columns(i).Caption & " = " & .Columns(i).FilterText + " "
                        End If
                    End If
                Next i

                .PreviewInfo.Caption = sCaption & "   /Grid Preview..."
                .PreviewInfo.NavigationPaneDockingStyle = DockStyle.None

            End With

            fntFont = New Font("Courier New", 10, FontStyle.Bold)

            With grdName.PrintInfo
                .WrapText = PrintInfo.WrapTextEnum.Wrap
                .VarRowHeight = PrintInfo.RowHeightEnum.StretchToFit
                .PageFooterHeight = 100

                .PageHeaderStyle.Font = fntFont

                .PageFooterStyle.Font = fntFont

                .PageFooterStyle.ForeColor = System.Drawing.Color.Black

                .RepeatColumnFooters = True
                .RepeatColumnHeaders = True
                .RepeatGridHeader = True

                .ShowProgressForm = True

                .UseGridColors = True

                .ShowOptionsDialog = bShowOptionsDialog



                sDate = "Printing Date: " & Format(Now, "dd/MM/yyyy")
                sTime = "Printing Time: " & Format(Now, "hh:mm tt")
                sUser = "Printing User: " & gbUser

                'sAddr = gbPCName

                If sVatReg <> "" Then sVatReg = "VAT Reg.: " & sVatReg

                '---------------------HEADER---------------

                If bLandscape = True Then
                    'line 1
                    '.PageHeader = vbCLocName & Space(90 - vbCLocName.Length) & sDate & Space(30 - sDate.Length) & Chr(160) & vbCrLf
                    .PageHeader = sDate & Space(30 - sDate.Length) & Chr(160) & vbCrLf

                    'line 2
                    '.PageHeader &= sAddr & Space(90 - sAddr.Length) & sTime & Space(30 - sTime.Length) & Chr(160) & vbCrLf

                    'line 3 ()
                    '.PageHeader &= sVatReg & Space(90 - sVatReg.Length) & sUser & Space(30 - sUser.Length) & Chr(160)
                    '-------------------------------------------
                Else
                    'line 1
                    '.PageHeader = vbCLocName & Space(60 - vbCLocName.Length) & sDate & Space(30 - sDate.Length) & Chr(160) & vbCrLf
                    .PageHeader = sDate & Space(30 - sDate.Length) & Chr(160) & vbCrLf

                    'line 2
                    '.PageHeader &= sAddr & Space(60 - sAddr.Length) & sTime & Space(30 - sTime.Length) & Chr(160) & vbCrLf

                    'line 3 ()
                    '.PageHeader &= sVatReg & Space(60 - sVatReg.Length) & sUser & Space(30 - sUser.Length) & Chr(160)
                    '-------------------------------------------
                End If

                .PageHeaderStyle.WrapText = True : .PageHeaderHeight = 100
                .PageHeaderStyle.ForeColor = System.Drawing.Color.Navy

                .PageHeaderStyle.VerticalAlignment = C1.Win.C1TrueDBGrid.AlignVertEnum.Top
                .PageHeaderStyle.HorizontalAlignment = C1.Win.C1TrueDBGrid.AlignHorzEnum.Near

                .RepeatColumnFooters = True

                .RepeatGridHeader = True
                .RepeatColumnHeaders = True
                .RepeatSplitHeaders = True
                .RepeatGridHeader = True

                .PageSettings.Margins.Top = 20
                .PageSettings.Margins.Left = 20
                .PageSettings.Margins.Bottom = 20
                .PageSettings.Margins.Right = 20

                If fltStr = "" Then
                    .PageFooter = "\p - \P"
                Else
                    .PageFooter = "Filtration :         " & fltStr & vbCrLf & "\p - \P"
                End If

                .PageFooterStyle.WrapText = True

                .PageFooterStyle.VerticalAlignment = C1.Win.C1TrueDBGrid.AlignVertEnum.Top
                .PageFooterStyle.HorizontalAlignment = C1.Win.C1TrueDBGrid.AlignHorzEnum.Near

                .PageSettings.Landscape = bLandscape

                .PageSettings.PaperSize = New System.Drawing.Printing.PaperSize("A4", 827, 1169)

                .PrintPreview()

            End With

        Catch exc As Exception
            Throw exc
        Finally
            ds = Nothing
            fntFont = Nothing
        End Try

    End Sub

    Public Sub PrintCashPOS(ByVal WSerno() As Long, ByVal sOper As String, ByVal txttime As String, ByVal _me As Form, Optional ByVal TempTable As String = "", Optional DocPrefix As String = "", Optional _PrintBeforeClose As Boolean = False, Optional PrintDirect As Boolean = False)
        Dim i As Integer
        'Print Invoice
        Dim frm As frmPreviewGrid
        Dim strsql, strsql1 As String
        Dim StempSales As String
        Dim StempCsh, StempPayMode As String
        Dim reportN As String
        Dim wSernoPrint As String
        Dim strPrev(,) As String
        Try
            Cursor.Current = Cursors.WaitCursor
            frm = New frmPreviewGrid

            StempSales = "Temp_Salesinvh" & gbUser
            StempCsh = "Temp_Csh" & gbUser
            StempPayMode = "Temp_PayMode" & gbUser

            DropSqlObject(StempSales, "TABLE")
            DropSqlObject(StempCsh, "TABLE")

            i = 0
            wSernoPrint = ""
            For i = 0 To WSerno.Length - 1
                wSernoPrint &= "'" & WSerno(i) & "',"
            Next

            wSernoPrint = wSernoPrint.Substring(0, wSernoPrint.Length - 1)

            'fill StempSales From  POS_saleinvhead with RecIDs sent from form
            strsql = "Select * into " & StempSales & " From  POS_saleinvhead "
            strsql &= " where  POS_saleinvhead.invH_LOC = '" & Cloc & "'"
            strsql &= " and POS_saleinvhead.invH_recID in (" & wSernoPrint & ")"
            Execute(strsql)

            'fill StempCsh From  pos_cashtransaction with RecIDs sent from form
            strsql = "Select pos_cashtransaction.* "
            strsql &= ",INVH_CURR" & CStr(IIf(Copercurr = "fc", "fc", "")) & ", INVH_RATE , INVH_RATEFC " 'added by GS on 13/05/16
            strsql &= " into " & StempCsh & " From  pos_cashtransaction "
            strsql &= " INNER JOIN POS_SALEINVHEAD ON CT_LOCATION = INVH_LOC AND CT_INVHEADER = INVH_RECID" 'added by GS on 13/05/16
            strsql &= " where ct_location ='" & Cloc & "' and ct_invheader in (" & wSernoPrint & " )"
            Execute(strsql)


            ' Call FillPayMode(wSernoPrint, StempCsh, StempPayMode)

            'Report Query
            strsql = " SELECT"
            strsql &= " POS_SALEINVHEAD.INVH_LOC, POS_SALEINVHEAD.INVH_RECID, POS_SALEINVHEAD.INVH_DOCTYP1, POS_SALEINVHEAD.INVH_DOCPREFIX, POS_SALEINVHEAD.INVH_SERNO, POS_SALEINVHEAD.INVH_DATE, POS_SALEINVHEAD.INVH_RKS, ISNULL(INVH_CURRFC, INVH_CURR) INVH_CURR, ABS(case when INVH_CURRFC is null then INVH_NET else INVH_NETFC end) INVH_NET,"
            strsql &= " POS_STOCKTRANSACTION.STT_FOOTER  ,POS_STOCKTRANSACTION.STT_PRINTED1 ,POS_STOCKTRANSACTION.STT_PRINTED2  "
            strsql &= " From " & StempSales & "  as POS_SALEINVHEAD inner join "
            strsql &= " POS_STOCKTRANSACTION POS_STOCKTRANSACTION"
            strsql &= " On "
            strsql &= " POS_SALEINVHEAD.INVH_LOC = POS_STOCKTRANSACTION.STT_LOCATION AND"
            strsql &= " POS_SALEINVHEAD.INVH_DOCTYP1 = POS_STOCKTRANSACTION.STT_DOCTYPE1 AND"
            strsql &= " POS_SALEINVHEAD.INVH_DOCPREFIX = POS_STOCKTRANSACTION.STT_PREFIX"
            strsql &= " and POS_saleinvhead.invH_LOC = '" & Cloc & "'"
            strsql &= " and POS_saleinvhead.invH_recID in (" & wSernoPrint & ")"


            Dim NbrCopies As String
            Dim strsqlnbcopy As String
            strsqlnbcopy = " SELECT top 1 STT_NBRCOPY FROM pos_stocktransaction WHERE STT_LOCATION='" & Cloc & "' "
            strsqlnbcopy &= " AND STT_DOCTYPE1='" & sOper & "' AND STT_Active='Y' and STT_PREFIX = '" & DocPrefix.Trim & "'"


            Dim rptParam(11, 1) As Object


            If sOper = "CO" Then
                rptParam(0, 0) = "Title1"
                rptParam(0, 1) = " 'Cash Out'"
            ElseIf sOper = "CI" Then
                rptParam(0, 0) = "Title1"
                rptParam(0, 1) = " 'Cash In'"
            End If

            rptParam(1, 0) = "Time"
            rptParam(1, 1) = " '" & txttime & "'"

            rptParam(2, 0) = "Company"
            rptParam(2, 1) = " '" & gbCompName & "'"

            'rptParam(3, 0) = "PMode1"
            'rptParam(3, 1) = " '" & txtpaym(1) & "'"

            'rptParam(4, 0) = "PMode2"
            'rptParam(4, 1) = " '" & txtpaym(2) & "'"

            'rptParam(5, 0) = "PMode3"
            'rptParam(5, 1) = " '" & txtpaym(3) & "'"

            'rptParam(6, 0) = "PMode4"
            'rptParam(6, 1) = " '" & txtpaym(4) & "'"

            'rptParam(7, 0) = "PMode5"
            'rptParam(7, 1) = " '" & txtpaym(5) & "'"

            'rptParam(8, 0) = "PMode6"
            'rptParam(8, 1) = " '" & txtpaym(6) & "'"

            rptParam(9, 0) = "Curr1"
            rptParam(9, 1) = " '" & gbCurr1 & "'"

            rptParam(11, 0) = "Location"
            rptParam(11, 1) = "'" & Cloc.Replace("'", "''") & "- " & CLocName.Replace("'", "''") & "'"


            Select Case _me.Name

                Case frmCashOutIn.Name
                    strPrev = Nothing
                    Dim sbr As ModPrint.SubReport
                    Dim str As String
                    str = "SELECT * From " & StempPayMode
                    sbr = New ModPrint.SubReport("PayTerm")
                    sbr.AddECParameter("@PayTermsStr", str)
                    If Copercurr = gbCurr1 Then
                        sbr.AddFOParameter("Pround", "" & VatLCRound & "")
                    Else
                        sbr.AddFOParameter("Pround", "2")
                    End If
                    sbr.AddFOParameter("Curr1", "'" & gbCurr1 & "'")
                    ModPrint.InitSubReport(sbr)

                    Dim StrNbCopie As String
                    StrNbCopie = " SELECT top 1 STT_NBRCOPY"
                    StrNbCopie &= " FROM    POS_STOCKTRANSACTION"
                    StrNbCopie &= " WHERE   STT_LOCATION = '" & Cloc & "'"
                    StrNbCopie &= "         AND STT_DOCTYPE1 = '" & sOper & "'"
                    StrNbCopie &= "         AND STT_Active = 'Y'"
                    NbrCopies = ExecuteScalar(StrNbCopie)
                    If NbrCopies = Nothing OrElse NbrCopies.Trim = "" OrElse NbrCopies.Trim = "0" Then
                        NbrCopies = "1"
                    End If

                    PrintPODirectRep(_me, strsql, "rptCashSlip.rpt", CInt(NbrCopies), rptParam, , , , , , , , , , , _PrintBeforeClose)

                Case frmReprintSlips.Name
                    'Preview Query (fields should be included in report query having same names to apply filtration)
                    If PrintDirect Then
                        strPrev = Nothing
                        Dim sbr As ModPrint.SubReport
                        Dim str As String
                        str = "SELECT * From " & StempPayMode
                        sbr = New ModPrint.SubReport("PayTerm")
                        sbr.AddECParameter("@PayTermsStr", str)
                        If Copercurr = gbCurr1 Then
                            sbr.AddFOParameter("Pround", "" & VatLCRound & "")
                        Else
                            sbr.AddFOParameter("Pround", "2")
                        End If
                        sbr.AddFOParameter("Curr1", "'" & gbCurr1 & "'")
                        ModPrint.InitSubReport(sbr)

                        Dim StrNbCopie As String
                        StrNbCopie = " SELECT top 1 STT_NBRCOPY"
                        StrNbCopie &= " FROM    POS_STOCKTRANSACTION"
                        StrNbCopie &= " WHERE   STT_LOCATION = '" & Cloc & "'"
                        StrNbCopie &= "         AND STT_DOCTYPE1 = '" & sOper & "'"
                        StrNbCopie &= "         AND STT_Active = 'Y'"
                        NbrCopies = ExecuteScalar(StrNbCopie)
                        If NbrCopies = Nothing OrElse NbrCopies.Trim = "" OrElse NbrCopies.Trim = "0" Then
                            NbrCopies = "1"
                        End If

                        PrintPODirectRep(_me, strsql, "rptCashSlip.rpt", CInt(NbrCopies), rptParam, , , , , , , , , , , _PrintBeforeClose)

                    Else
                        strsql1 = "Select "
                        strsql1 &= "DocType INVH_DOCTYP1,Prefix INVH_DOCPREFIX,Serial INVH_SERNO,Date INVH_DATE,ABS(TotalAmount) INVH_NET,Currency INVH_CURR,Remarks INVH_RKS "
                        strsql1 &= "from " & TempTable & " "
                        strsql1 &= "Where [Chk] = 'True' "

                        With frm
                            frm.BuildSql(strsql1, , , , , strsql.ToString)

                            .ColDesign("INVH_DOCTYP1", -1, "Doc.Type", , AlignVertEnum.Top, , , , , True)
                            .ColDesign("INVH_DOCPREFIX", -1, "Prefix", , AlignVertEnum.Top, , , , , True)
                            .ColDesign("INVH_SERNO", -1, "Serial", , , , , , , , , frmPreviewGrid.Orders.Descending)
                            .ColDesign("INVH_DATE", -1, "Date", , , , , , , , , frmPreviewGrid.Orders.Descending, , , , "dd/MM/yyyy")
                            .ColDesign("INVH_NET", -1, "Total Amount", AlignHorzEnum.Far, , , , , , , , , , , , "N3")
                            .ColDesign("INVH_CURR", -1, "Currency", AlignHorzEnum.Center, , , , , , , , , , , , "N3")
                            .ColDesign("INVH_RKS", 150, "Remarks", AlignHorzEnum.Near)

                            strPrev = Nothing
                            Dim sbr As frmPreviewGrid.SubReport
                            Dim str As String
                            str = "SELECT * From " & StempPayMode
                            sbr = New frmPreviewGrid.SubReport("PayTerm")
                            sbr.AddECParameter("@PayTermsStr", str)
                            If Copercurr = gbCurr1 Then
                                sbr.AddFOParameter("Pround", "" & VatLCRound & "")
                            Else
                                sbr.AddFOParameter("Pround", "2")
                            End If
                            sbr.AddFOParameter("Curr1", "'" & gbCurr1 & "'")
                            frm.InitSubReport(sbr)

                            .ShowDialog(strPrev, frmReprintSlips.Name, "rptCashSlip.rpt", rptParam)
                        End With
                    End If


            End Select

        Catch ex As Exception
            Throw ex
        Finally
            If Not frm Is Nothing Then frm = Nothing
            DropSqlObject(StempSales, "TABLE")
            DropSqlObject(StempCsh, "TABLE")
            Cursor.Current = Cursors.Default
        End Try
    End Sub
    'Commented by Rita M @ 20.06.2014. Function exists in ModPrint
    'Public Sub FillPayMode(ByVal WSerno As String, ByVal StempCsh As String, ByVal StempPayMode As String, Optional ByVal ReprintSlipTempTable As String = "")
    '    Dim i As Integer
    '    Dim cnSql As SqlConnection
    '    Dim cmSql As SqlCommand
    '    Dim strsql As String
    '    'Dim drSql As SqlDataReader
    '    Try
    '        'For i = 1 To 15
    '        '    txtpaym(i) = ""
    '        'Next

    '        DropSqlObject(StempPayMode, "TABLE")

    '        strsql = "Create table " & StempPayMode
    '        strsql &= "(INV_Loc varchar(2),"
    '        strsql &= " Inv_recID numeric(18, 0),"
    '        strsql &= " PaymentDesc varchar(30),"
    '        strsql &= " Amount numeric(18,3),"
    '        strsql &= " Curr varchar(3),"
    '        strsql &= " AmountCV numeric(18,3))"
    '        Execute(strsql)


    '        cnSql = New SqlConnection(ConnectionString)
    '        cnSql.Open()
    '        cmSql = New SqlCommand(strsql, cnSql)

    '        strsql = " Insert into " & StempPayMode
    '        'strsql &= " select CT_Location,CT_INVHeader,"
    '        'strsql &= " ST_DESC + ': '+ CONVERT(VARCHAR,CONVERT(MONEY,rtrim(ltrim(str(Ct_amount * case when ct_opsign ='+' then 1 else  -1 end ,18,3))),2),1) "
    '        'strsql &= " +' '+ CT_currcode   "
    '        'strsql &= " + ', C/V' +  "
    '        'strsql &= " CONVERT(VARCHAR,CONVERT(MONEY,case when CT_currcode <> '" & Copercurr & "' then CT_amtOPCURR else  0 end ,2),1)"
    '        strsql &= "  SELECT  CT_Location ,CT_INVHeader ,ST_DESC,RTRIM(LTRIM(STR(Ct_amount * CASE WHEN ct_opsign = '+' THEN 1"
    '        strsql &= " ELSE -1 END, 18, 3))), CT_currcode , CASE WHEN CT_currcode <> 'LL' THEN CT_amtOPCURR ELSE 0 End"
    '        strsql &= " from " & StempCsh & " inner join stk04_SETTLEMENTTYPE On ct_location = st_loc and "
    '        strsql &= " CT_Settltype =  ST_CODE "
    '        'Conditions = current location, serial number
    '        If ReprintSlipTempTable = "" Then
    '            strsql &= " Where CT_Location = '" & Cloc & "' "
    '            strsql &= " and CT_INVHeader in (" & WSerno & ")"
    '        End If

    '        cmSql.CommandText = strsql
    '        cmSql.ExecuteNonQuery()

    '        'cmSql.CommandText = strsql
    '        'drSql = cmSql.ExecuteReader

    '        'With drSql
    '        '    If .HasRows Then
    '        '        i = 1
    '        '        While .Read()
    '        '            txtpaym(i) = .Item(0).ToString & CStr(IIf(CDbl(.Item(1).ToString) = 0, "", ", C/V " & Format(.Item(1).ToString, "#,##0.0##")))
    '        '            i = i + 1
    '        '        End While
    '        '    End If
    '        'End With

    '    Catch ex As Exception
    '        Throw ex
    '    End Try
    'End Sub
    Public Sub PrintSalesInvoiceSlipLaser(ByVal Slip As Boolean,
                                          ByVal WSerno As Double,
                                          ByVal sOper As String,
                                          ByVal txttime As String,
                                          ByVal ClNameFromClientFile As Boolean,
                                          ByVal PrintTechContent As Boolean,
                                          ByVal _me As Form,
                                          Optional ByVal ReprintSlipTempTable As String = "",
                                          Optional ByVal copyBonAchat As Boolean = False,
                                          Optional ByVal SupressTotal As Boolean = False,
                                          Optional ByVal ShowBundleComm As Boolean = False,
                                          Optional ByVal curr As String = "",
                                          Optional ByVal clientCode As String = "",
                                          Optional ByVal ClientNO As String = "",
                                          Optional ByVal PrintLinkBTWInvAndCN As Boolean = True,
                                          Optional ByVal PrintExpLot As Boolean = False,
                                          Optional ByVal rbSingleRef2 As Boolean = False,
                                          Optional ByVal ItemReference As String = "B",
                                          Optional ByVal PrintClBalance As Boolean = True,
                                          Optional ByVal Lang As String = "",
                                          Optional ByVal _RepName As String = "",
                                          Optional ByVal _Prefix As String = "",
                                          Optional ByVal _ordering As String = "",
                                          Optional ByRef withPriviewPrint As Boolean = False,
                                          Optional PrintCopy As String = "Y",
                                          Optional ItmCetificate As Boolean = False,
                                          Optional _PrintBeforeClose As Boolean = False,
                                          Optional directprint As Boolean = False,
                                          Optional DoneByInDet As Boolean = False,
                                          Optional PrintFundDisbursementItem As Boolean = False,
                                          Optional FundQuery As String = "",
                                          Optional PrintFundDisbursementPay As Boolean = False,
                                          Optional PrintCompanyLogo As Boolean = False)
        Dim i As Integer
        Dim TxtCln As Double
        'Print Invoice
        Dim frm As frmPreviewGrid
        Dim strsql, strsql1, strsqlBonAchat As String
        Dim StempCsh, StempPayMode, StempCompensation, StempPaymentDueDays As String
        Dim strPrev(,) As String
        Dim PrintBonAchat As Boolean
        Dim RptInvoice As String
        Dim TransTotalVisible As Boolean

        Dim InvalidBalanceMsg As String = ""

        Dim strs As String
        Dim dr0 As DataRow
        Dim DRS0 As SqlDataReader
        Dim maxAttLengh As Integer

        'Variables used to be sent to the preview print function to update the number of printed invoices
        Dim invRecidToUpdCtr As String

        Try
            Cursor.Current = Cursors.WaitCursor

            frm = New frmPreviewGrid

            If Slip = True Then
                PrintClBalance = False
            Else
                If ClocHO = False Then
                    TestConnection()
                End If
            End If

            If WSerno <> 0 Then
                invRecidToUpdCtr = WSerno.ToString
            Else
                Using cnSql As New SqlConnection(ConnectionString), cmSql As New SqlCommand("", cnSql)
                    cnSql.Open()
                    cmSql.CommandText = "SELECT SUBSTRING((SELECT ', ''' + CONVERT(varchar,invhead) +'''' FROM " +
                        ReprintSlipTempTable + " WHERE chk = 'True' FOR XML PATH('')), 2, 8000)"

                    Dim obj As Object
                    obj = cmSql.ExecuteScalar()
                    If Not obj Is Nothing Then
                        invRecidToUpdCtr = obj.ToString
                    Else
                        invRecidToUpdCtr = ""
                    End If
                End Using
            End If
            'SernoTemp = CStr(IIf(WSerno = 0, ReprintSlipTempTable, WSerno))

            RptInvoice = GetFieldDesc("pos_splocation", "SL_Loc='" & Cloc & "'", "SL_RptInvoice")
            If Slip = True Then
                If ItmCetificate Then
                    RptInvoice = "rptInvPosSlipItemCertificate.rpt"
                Else
                    RptInvoice = "rptInvPosSlip.rpt"
                End If
            Else
                If ItmCetificate Then
                    RptInvoice = "rptInvPosLaserItemCertificate.rpt"
                ElseIf SupressTotal = False Then
                    If RptInvoice = "" Then RptInvoice = "rptInvPosLaser.rpt"
                Else
                    If RptInvoice = "" Then
                        RptInvoice = "rptInvPosLaserNoTot.rpt"
                    Else
                        RptInvoice = Mid(RptInvoice, 1, InStr(RptInvoice, ".") - 1) & "NoTot.rpt"
                    End If
                End If
            End If
            'No bundle and comment lines are shown in slip report
            '    If Slip = True Then ShowBundleComm = False
            If PrintClBalance = True AndAlso RptInvoice = "rptInvPosLaserOPP.rpt" Then
                TemClientBalnc(_me, ReprintSlipTempTable, clientCode, ClientNO, IIf(curr = "", Copercurr, curr).ToString)
            End If
            StempCsh = "Temp_SalesInvSlip" & gbUser
            StempPayMode = "Temp_PayModeSalesInvSlip" & gbUser
            StempCompensation = "Temp_PrivCardCompensation" & gbUser
            StempPaymentDueDays = "Temp_PaymentDueDays" & gbUser

            DropSqlObject(StempCsh, "TABLE")
            DropSqlObject(StempCompensation, "TABLE")
            DropSqlObject(StempPaymentDueDays, "TABLE")

            'fill StempCsh From  pos_cashtransaction with RecIDs sent from form
            strsql = "Select pos_cashtransaction.* "
            strsql &= ",INVH_CURR" & CStr(IIf(Copercurr = "fc", "fc", "")) & ", INVH_RATE , INVH_RATEFC " 'added by GS on 13/05/16    '...Jad JE - Invoicing in FC Currency
            strsql &= " into " & StempCsh & " From  pos_cashtransaction "
            strsql &= " INNER JOIN POS_SALEINVHEAD ON CT_LOCATION = INVH_LOC AND CT_INVHEADER = INVH_RECID" 'added by GS on 13/05/16
            'Conditions = current location, serial number
            Select Case _me.Name
                Case FrmSalesInvoice.Name, FrmModRemInvoice.Name, "FrmPurOfUsedBook", frmClientStatement.Name
                    strsql &= " where ct_location ='" & Cloc & "' and ct_invheader = '" & WSerno & "'"
                    'Join with TempTable sent from form, conditions = current location, checked records
                Case frmReprintSlips.Name, frmRevSalesInv.Name
                    strsql &= " Left Join " & ReprintSlipTempTable & " on InvHead = CT_INVHEADER"
                    strsql &= " where CT_LOCATION = '" & Cloc & "'"
                    strsql &= " And [Chk] = 'True' "
            End Select

            Execute(strsql)

            'fill StempCompensation for privilege card compensation
            strsql = " SELECT  INVH_LOC,INVH_RECID,Trn_Points,Trn_Remarks"
            strsql &= " into " & StempCompensation & " FROM dbo.POS_SALEINVHEAD"
            strsql &= " LEFT JOIN tbl_Priv_Transaction ON INVH_LOC = Trn_Loc and SUBSTRING(Trn_InvoiceNo, 1, 2) = INVH_DOCTYP1"
            strsql &= " AND SUBSTRING(Trn_InvoiceNo, 3, 1) = INVH_DOCPREFIX"
            strsql &= " AND SUBSTRING(Trn_InvoiceNo, 5, LEN(Trn_InvoiceNo) - 3) = INVH_SERNO"
            Select Case _me.Name
                Case FrmSalesInvoice.Name, FrmModRemInvoice.Name, "FrmPurOfUsedBook", frmClientStatement.Name
                    strsql &= " where INVH_LOC ='" & Cloc & "' and INVH_RECID = '" & WSerno & "'"
                    'Join with TempTable sent from form, conditions = current location, checked records
                Case frmReprintSlips.Name, frmRevSalesInv.Name
                    strsql &= " Left Join " & ReprintSlipTempTable & " on InvHead = INVH_RECID"
                    strsql &= " where INVH_LOC = '" & Cloc & "'"
                    strsql &= " And [Chk] = 'True' "
            End Select
            strsql &= " AND Trn_Type='S'"

            Execute(strsql)

            'Rita M @ 24.08.2017
            'fill StempPaymentDueDays for payment due days
            strsql = " SELECT DDA_INVHeader,DDA_Counter,DDA_Perc,"
            'strsql &= " DDA_Value,"
            If _me.Name = "frmRevSalesInv" Then
                strsql &= " DDA_Value DDA_Value,"
            Else
                If curr = "" Then 'added by GS on 10/05/16
                    strsql &= CStr(IIf(Copercurr = "FC", " DDA_Value ", "")) & " DDA_Value, "
                    'added by GS on 10/05/16
                Else
                    If curr = gbCurr1 Then
                        strsql &= " dbo.convert5FC('" & gbApp & "','" & gbComp & "','" & gbYear & "', DDA_Value,ISNULL(INVH_CURRFC,INVH_CURR),'" & gbCurr1 & "',INVH_RATE,invh_date) DDA_Value,"
                    ElseIf curr = gbCurr2 Then
                        'If Copercurr = "FC" Then
                        '    strsql &= " DDA_Value,"

                        'Else
                        strsql &= " dbo.convert5FC('" & gbApp & "','" & gbComp & "','" & gbYear & "', DDA_Value,ISNULL(INVH_CURRFC,INVH_CURR),'" & gbCurr2 & "',INVH_RATE" & CStr(IIf(Copercurr = "FC", "FC", "")) & ",invh_date) DDA_Value,"
                        'End If
                    Else
                        strsql &= " DDA_Value DDA_Value,"
                    End If
                End If
            End If

            strsql &= " CT_CURRCODE ,"
            strsql &= " DDA_ValueDate,DDA_Remark"
            strsql &= " into " & StempPaymentDueDays & " FROM dbo.POS_DueDaysOnAC"
            Select Case _me.Name
                Case FrmSalesInvoice.Name, FrmModRemInvoice.Name, "FrmPurOfUsedBook", frmClientStatement.Name
                    strsql &= " INNER JOIN POS_SALEINVHEAD ON INVH_LOC='" & Cloc & "' AND INVH_RECID=DDA_INVHeader"
                    strsql &= " 	  INNER JOIN ( SELECT CT_LOCATION ,
                                                          CT_INVHEADER ,
                                                          CT_CREGISTER ,
                                                          CT_OPNBR,
							                              CT_CURRCODE
                                                   FROM   POS_CASHTRANSACTION
					                               WHERE CT_LOCATION = '" & Cloc & "'
					                               AND CT_INVHEADER = '" & WSerno & "'
                                                 ) POS_CASHTRANSACTION ON CT_LOCATION = DDA_Location
                                                                          AND CT_INVHEADER = DDA_INVHeader
                                                                          AND CT_CREGISTER = DDA_CRegister
                                                                          AND CT_OPNBR = DDA_OPNBR "
                    strsql &= " where DDA_Location ='" & Cloc & "' and DDA_INVHeader = '" & WSerno & "'"
                    If RptInvoice = "rptInvPosLaserMaliks.rpt" OrElse RptInvoice = "rptInvPosLaserDoculand.rpt" _
                    OrElse RptInvoice = "rptInvPosLaserKhayyatKanaan.rpt" OrElse RptInvoice = "rptInvPosLaserNovamart.rpt" Then
                        strsql &= " union all SELECT INVH_RECID , 1   ,100 ,"
                        If _me.Name = "frmRevSalesInv" Then
                            strsql &= " BH_VALUE BH_VALUE,"
                        Else
                            If curr = "" Then
                                strsql &= CStr(IIf(Copercurr = "FC", " BH_VALUE ", "")) & " BH_VALUE, "
                            Else
                                If curr = gbCurr1 Then
                                    strsql &= " dbo.convert5FC('" & gbApp & "','" & gbComp & "','" & gbYear & "', BH_VALUE,ISNULL(INVH_CURRFC,INVH_CURR),'" & gbCurr1 & "',INVH_RATE,invh_date) BH_VALUE,"
                                ElseIf curr = gbCurr2 Then
                                    strsql &= " dbo.convert5FC('" & gbApp & "','" & gbComp & "','" & gbYear & "', BH_VALUE,ISNULL(INVH_CURRFC,INVH_CURR),'" & gbCurr2 & "',INVH_RATE" & CStr(IIf(Copercurr = "FC", "FC", "")) & ",invh_date) BH_VALUE,"
                                Else
                                    strsql &= " BH_VALUE BH_VALUE,"
                                End If
                            End If
                        End If
                        strsql &= "  BH_CURR,  BH_VALDATE  ,BH_REMARKS FROM  dbo.POS_BONACHAT "
                        strsql &= " INNER JOIN POS_SALEINVHEAD ON INVH_LOC = '" & Cloc & "'"
                        strsql &= " and bh_invnbr = invh_doctyp1+invh_docprefix+' '+convert(varchar,invh_serno)"
                        strsql &= " AND CONVERT(DATE,invh_date) = CONVERT(DATE,BH_ISDATE) "
                        strsql &= "  WHERE bh_loc='" & Cloc & "' AND bh_type='R' and  INVH_RECID ='" & WSerno & "'"
                    End If
                    'Join with TempTable sent from form, conditions = current location, checked records
                Case frmReprintSlips.Name, frmRevSalesInv.Name
                    strsql &= " INNER JOIN " & ReprintSlipTempTable & " on DDA_Location='" & Cloc & "' AND DDA_INVHeader=InvHead "
                    strsql &= " INNER JOIN POS_SALEINVHEAD ON INVH_LOC='" & Cloc & "' AND INVH_RECID=DDA_INVHeader"
                    strsql &= " INNER JOIN ( SELECT CT_LOCATION ,
                                                    CT_INVHEADER ,
                                                    CT_CREGISTER ,
                                                    CT_OPNBR,
							                        CT_CURRCODE
                                             FROM   POS_CASHTRANSACTION
					                         WHERE CT_LOCATION = '" & Cloc & "'
                                           ) POS_CASHTRANSACTION ON CT_LOCATION = DDA_Location
                                                                    AND CT_INVHEADER = DDA_INVHeader
                                                                    AND CT_CREGISTER = DDA_CRegister
                                                                    AND CT_OPNBR = DDA_OPNBR "
                    strsql &= " WHERE chk='True'"
                    If RptInvoice = "rptInvPosLaserMaliks.rpt" OrElse RptInvoice = "rptInvPosLaserDoculand.rpt" _
                        OrElse RptInvoice = "rptInvPosLaserKhayyatKanaan.rpt" OrElse RptInvoice = "rptInvPosLaserNovamart.rpt" Then
                        strsql &= " union all SELECT InvHead , 1   ,100 , "
                        If _me.Name = "frmRevSalesInv" Then
                            strsql &= " BH_VALUE BH_VALUE,"
                        Else
                            If curr = "" Then
                                strsql &= CStr(IIf(Copercurr = "FC", " BH_VALUE ", "")) & " BH_VALUE, "
                            Else
                                If curr = gbCurr1 Then
                                    strsql &= " dbo.convert5FC('" & gbApp & "','" & gbComp & "','" & gbYear & "', BH_VALUE,INVH_OriginalCURR,'" & gbCurr1 & "',INVH_RATE,date) BH_VALUE,"
                                ElseIf curr = gbCurr2 Then
                                    strsql &= " dbo.convert5FC('" & gbApp & "','" & gbComp & "','" & gbYear & "', BH_VALUE,INVH_OriginalCURR,'" & gbCurr2 & "',INVH_RATE" & CStr(IIf(Copercurr = "FC", "FC", "")) & ",date) BH_VALUE,"
                                Else
                                    strsql &= " BH_VALUE BH_VALUE,"
                                End If
                            End If
                        End If
                        strsql &= "  BH_CURR,  BH_VALDATE  ,BH_REMARKS FROM  dbo.POS_BONACHAT "
                        strsql &= " INNER JOIN POS_SALEINVHEAD ON INVH_LOC = bh_loc"
                        strsql &= " and bh_invnbr = invh_doctyp1+invh_docprefix+' '+convert(varchar,invh_serno)"
                        strsql &= " AND CONVERT(DATE,invh_date) = CONVERT(DATE,BH_ISDATE) "
                        strsql &= " INNER JOIN " & ReprintSlipTempTable & " ON    INVH_RECID = InvHead"
                        strsql &= "  WHERE chk='True' AND bh_loc='" & Cloc & "' AND bh_type='R' "
                    End If

            End Select

            Execute(strsql)


            Call FillPayMode(CStr(WSerno), StempCsh, StempPayMode, ReprintSlipTempTable, curr,,,, (PrintFundDisbursementPay Or PrintFundDisbursementItem), PrintFundDisbursementPay)



            If RptInvoice = "RptFaircoReportPrint.rpt" Or RptInvoice = "RptFaircoReportView.rpt" _
               Or RptInvoice = "rptInvPosLaserOPP.rpt" Or RptInvoice = "rptInvPosLaserOPPtoprint.rpt" _
               Or RptInvoice = "rptInvPosLaser.rpt" Or RptInvoice = "rptInvPosLaserSansirab.rpt" Or RptInvoice = "rptInvPosLaserItemCertificate.rpt" _
               Or RptInvoice = "rptInvPosLaserMaliks.rpt" Or RptInvoice = "rptInvPosLaserDoculand.rpt" _
               OrElse RptInvoice = "rptInvPosLaserKhayyatKanaan.rpt" OrElse RptInvoice = "rptInvPosLaserNovamart.rpt" OrElse
               RptInvoice = "rptInvPosLaserISM.rpt" OrElse RptInvoice = "rptInvPosLaserMedix.rpt" OrElse RptInvoice = "hintrag01.rpt" OrElse
               RptInvoice = "rptInvPosLaserMountainSteel.rpt" OrElse RptInvoice.ToUpper = "rptInvPosLaserEmileAoun.rpt".ToUpper Then

                strsql = "WITH"

                If RptInvoice = "RptFaircoReportPrint.rpt" Or RptInvoice = "RptFaircoReportView.rpt" _
                       Or RptInvoice = "rptInvPosLaserOPP.rpt" Or RptInvoice = "rptInvPosLaserOPPtoprint.rpt" Then

                    strsql &= " DoConTable as (SELECT doh.INVH_RECID,doh.INVH_LOC,INVDO_DORecId"
                    strsql &= " FROM    dbo.POS_SALEINVhead doh"
                    strsql &= " INNER JOIN POS_SALEINVDET dod ON doh.INVH_RECID = dod.INVD_HEADID"
                    strsql &= " AND doh.INVH_LOC = dod.INVD_LOC"
                    strsql &= " LEFT JOIN dbo.POS_SALEINVDO ON POS_SALEINVDO.INVDO_InvLoc = dod.INVD_LOC"
                    strsql &= " AND POS_SALEINVDO.INVDO_InvHeadID = dod.INVD_HEADID"
                    strsql &= " AND POS_SALEINVDO.INVDO_InvCounter = dod.INVD_Counter"
                    strsql &= " ) "
                End If

                If RptInvoice = "rptInvPosLaser.rpt" Or RptInvoice = "rptInvPosLaserItemCertificate.rpt" Or RptInvoice = "rptInvPosLaserSansirab.rpt" _
                         Or RptInvoice = "rptInvPosLaserOPP.rpt" Or RptInvoice = "rptInvPosLaserOPPtoprint.rpt" _
                    Or RptInvoice = "RptFaircoReportPrint.rpt" Or RptInvoice = "RptFaircoReportView.rpt" Or RptInvoice = "rptInvPosLaserMaliks.rpt" Or RptInvoice = "rptInvPosLaserDoculand.rpt" _
                         OrElse RptInvoice = "rptInvPosLaserKhayyatKanaan.rpt" OrElse RptInvoice = "rptInvPosLaserNovamart.rpt" OrElse RptInvoice = "rptInvPosLaserISM.rpt" OrElse
                         RptInvoice = "rptInvPosLaserMedix.rpt" OrElse RptInvoice = "hintrag01.rpt" OrElse RptInvoice = "rptInvPosLaserMountainSteel.rpt" OrElse RptInvoice.ToUpper = "rptInvPosLaserEmileAoun.rpt".ToUpper Then

                    strsql &= CStr(IIf(strsql = "WITH", "", " , "))
                    strsql &= " SdConTable as (SELECT soh.INVH_RECID,soh.INVH_LOC,HOFR_RECID,HOFR_SeqNo FROM    dbo.POS_SALEINVhead soh"
                    strsql &= " INNER JOIN POS_SALEINVDET sod ON soh.INVH_RECID = sod.INVD_HEADID"
                    strsql &= " AND soh.INVH_LOC = sod.INVD_LOC"
                    strsql &= " INNER JOIN dbo.POS_SALEINVCO ON POS_SALEINVCO.INVCO_InvLoc = sod.INVD_LOC"
                    strsql &= " AND POS_SALEINVCO.INVCO_InvHeadID = sod.INVD_HEADID"
                    strsql &= " AND POS_SALEINVCO.INVCO_InvCounter = sod.INVD_Counter"
                    strsql &= " INNER JOIN dbo.STK04_OfferHeader ON HOFR_HeadRecId = INVCO_OfferRecId"
                    strsql &= " AND hofr_loc = INVCO_OfferLoc"
                    strsql &= " )  "
                End If

            Else

                strsql = ""

            End If


            If PrintExpLot Then
                strsql &= " SELECT INVD_Counter,INVH_LOC, INVH_RECID, INVH_DATE,"
            Else
                strsql &= " SELECT MAX(INVD_Counter) INVD_Counter,INVH_LOC, INVH_RECID, INVH_DATE,"
            End If
            If RptInvoice = "RptFaircoReportView.rpt" Then
                strsql &= " INVH_EMP, " & IIf(ItmCetificate, " '' ", " cl_AcExec ").ToString & " INVD_Salesman, INVH_CSHREG,"
            Else
                strsql &= " INVH_EMP," & IIf(ItmCetificate, " '' ", " INVD_Salesman ").ToString & "   INVD_Salesman, INVH_CSHREG,"
            End If

            strsql &= " ClientFolio.fo_Email ClientEmail, ClientFolio.fo_mof ClientMOF, INVH_CLNAME,"
            strsql &= " ClientFolio.fo_vatreg fo_MOF,ClientFolio.fo_phone1 ClientPhone1,ClientFolio.fo_fax ClientFax,"
            'ch @28.05.2019 mail(ERP_INVOICE_LAYOUT_SHOWING_WRONG_ACCOUNT_CURRENCY)
            ' If RptInvoice = "RPTINVPOSLASERHINTRAG.RPT" Then
            '   strsql &= " SUBSTRING(ca_clac, 1, 11) + " & IIf(curr.Trim <> "" And curr.Trim <> "FC", "'" & curr & "'", " CASE WHEN INVH_CURRFC IS NULL OR INVH_CURRFC = '' THEN INVH_CURR ELSE INVH_CURRFC END").ToString & " ca_clac,"
            strsql &= " SUBSTRING(ca_clac, 1, 11) + INVH_CURR" & CStr(IIf(Copercurr = "FC", "FC", "")) & " ca_clac,"
            '   Else
            '  If Copercurr = "FC" Then
            'strsql &= " SUBSTRING(ca_clac, 1, 11) + invh_curr ca_clac," '...Jad JE - Invoicing in FC Currency
            '   Else
            '  strsql &= " SUBSTRING(ca_clac, 1, 11) + CONVERT(VARCHAR(3),'" & Copercurr & "') ca_clac,"
            '   End If
            '   End If



            strsql &= " CL_Code,CL_No, CL_Name, cl_invdays,CL_PriceCat, "
            If curr = "" Then 'added by GS on 10/05/16
                strsql &= "INVH_CURR" & CStr(IIf(Copercurr = "FC", "FC", "")) & " INVH_CURR,"
                'added by GS on 10/05/16
            Else
                If curr = "FC" Then
                    strsql &= "INVH_CURRFC INVH_CURR,"
                Else
                    strsql &= "'" & curr & "' INVH_CURR,"
                End If

            End If
            'GS on 10/05/16
            strsql &= "INVH_DOCTYP1, INVH_DOCPREFIX, INVH_Serno, Loc_Vat, "
            strsql &= " INVH_CLADDTEL, INVH_DSTAMP, POS_employee_2.e_name as CashierName , INVH_USTAMP ,INVH_VATTYPE,"
            strsql &= " INVH_RKS, "
            strsql &= " invh_rate,invh_ratefc"
            strsql &= " ,STK_NEWUSED, " & IIf(ItmCetificate, " '' ", " INVD_PROCARD ").ToString & " INVD_PROCARD, "

            'if allow change description = true ( pos_splocation ) item is considered as service item to show desciption from sales invoice instead of stock file
            strsql &= " " & IIf(ChangeDesc, "'S'", "").ToString & " STK_SERVICE, "

            If Lang = "" Then ' TY 
                strsql &= " INVD_ITMDESC STK_DESC,"
            Else
                strsql &= " isnull(STKL_ItemDesc,STK_DESC )  STK_DESC ,"
            End If

            strsql &= "STK_VATPER,"

            If curr = "" Then 'added by GS on 10/05/16
                strsql &= CStr(IIf(Copercurr = "FC", " INVH_GROSSFC ", "")) & " INVH_GROSS, " '...Jad JE - Invoicing in FC Currency
                'added by GS on 10/05/16
            Else
                If curr = gbCurr1 Then
                    'strsql &= " dbo.convert5FC('" & gbApp & "','" & gbComp & "','" & gbYear & "', ISNULL(NULLIF(invh_grossfc,0),invh_gross)  ,ISNULL(NULLIF(invh_CURRfc,''),invh_CURR),'" & gbCurr1 & "',ISNULL(NULLIF(INVH_RATEfc,0),INVH_RATE),INVH_Date) INVH_GROSS,"
                    If Copercurr = "FC" Then
                        strsql &= " case when isnull(invh_currfc,'') = '" & gbCurr1 & "'  then  invh_grossfc else "
                        strsql &= " dbo.convert5FC('" & gbApp & "','" & gbComp & "','" & gbYear & "', CONVERT (NUMERIC(18,3) ,dbo.convert5FC('" & gbApp & "','" & gbComp & "','" & gbYear & "',invh_grossFC  ,invh_currFC  ,'" & gbCurr2 & "',INVH_RATEFC,INVH_Date)),'" & gbCurr2 & "'  ,'" & gbCurr1 & "',INVH_RATE,INVH_Date) end  INVH_GROSS,"
                    Else
                        strsql &= " dbo.convert5FC('" & gbApp & "','" & gbComp & "','" & gbYear & "',  invh_gross  ,invh_curr  ,'" & gbCurr1 & "',INVH_RATE,INVH_Date) INVH_GROSS,"
                    End If
                ElseIf curr = gbCurr2 Then
                    If Copercurr = "FC" Then
                        strsql &= " INVH_GROSS,"
                    Else
                        strsql &= " dbo.convert5FC('" & gbApp & "','" & gbComp & "','" & gbYear & "', invh_gross,invh_curr,'" & gbCurr2 & "',INVH_RATE,INVH_Date) INVH_GROSS,"
                    End If
                Else
                    strsql &= " INVH_GROSSFC INVH_GROSS,"
                End If
            End If
            'GS on 10/05/16


            If curr = "FC" Then 'added by GS on 10/05/16
                strsql &= CStr(IIf(Copercurr = "FC", " isnull(nullif(INVH_DISCFC,0),INVH_DISC) ", "")) & " INVH_DISC, "
                'added by GS on 10/05/16
            Else
                If curr = gbCurr1 Then
                    'strsql &= " dbo.convert5FC('" & gbApp & "','" & gbComp & "','" & gbYear & "', ISNULL(NULLIF(INVH_DISCfc,0),INVH_DISC)  ,ISNULL(NULLIF(invh_CURRfc,''),invh_CURR),'" & gbCurr1 & "',ISNULL(NULLIF(INVH_RATEfc,0),INVH_RATE),INVH_Date) INVH_DISC,"
                    If Copercurr = "FC" Then
                        strsql &= " case when isnull(invh_currfc,'') = '" & gbCurr1 & "'  then  INVH_DISCfc else "
                        strsql &= "  dbo.convert5FC('" & gbApp & "','" & gbComp & "','" & gbYear & "',CONVERT (NUMERIC(18,3),dbo.convert5FC('" & gbApp & "','" & gbComp & "','" & gbYear & "',INVH_DISCFC  ,invh_currFC  ,'" & gbCurr2 & "',INVH_RATEFC,INVH_Date)),'" & gbCurr2 & "'  ,'" & gbCurr1 & "',INVH_RATE,INVH_Date)  end  INVH_DISC,"
                    Else
                        strsql &= " dbo.convert5FC('" & gbApp & "','" & gbComp & "','" & gbYear & "',  INVH_DISC  ,invh_curr  ,'" & gbCurr1 & "',INVH_RATE,INVH_Date) INVH_DISC,"
                    End If
                ElseIf curr = gbCurr2 Then
                    strsql &= " dbo.convert5FC('" & gbApp & "','" & gbComp & "','" & gbYear & "', INVH_DISC ,invh_curr ,'" & gbCurr2 & "',INVH_RATE,INVH_Date) INVH_DISC,"
                Else
                    strsql &= " INVH_DISC, "
                End If
            End If

            strsql &= " INVH_AddDiscCaption ,"

            '  strsql &= "INVH_DISC, "
            If curr = "" Then 'added by GS on 10/05/16
                strsql &= CStr(IIf(Copercurr = "FC", " INVH_ADDDISCFC ", "")) & " INVH_ADDDISC, "
                'added by GS on 10/05/16
            Else
                If curr = gbCurr1 Then
                    '   strsql &= " dbo.convert5FC('" & gbApp & "','" & gbComp & "','" & gbYear & "', ISNULL(NULLIF(INVH_ADDDISCfc,0),INVH_ADDDISC) ,ISNULL(NULLIF(invh_currfc,''),invh_curr),'" & gbCurr1 & "',ISNULL(NULLIF(INVH_RATEfc,0),INVH_RATE),INVH_Date) INVH_ADDDISC,"
                    If Copercurr = "FC" Then
                        strsql &= " case when isnull(invh_currfc,'') = '" & gbCurr1 & "'  then  INVH_ADDDISCfc else "
                        strsql &= " dbo.convert5FC('" & gbApp & "','" & gbComp & "','" & gbYear & "', CONVERT (NUMERIC(18,3) ,dbo.convert5FC('" & gbApp & "','" & gbComp & "','" & gbYear & "',INVH_ADDDISCfc  ,invh_currfc  ,'" & gbCurr2 & "',INVH_RATEfc,INVH_Date)),'" & gbCurr2 & "'  ,'" & gbCurr1 & "',INVH_RATE,INVH_Date)   end  INVH_ADDDISC,"
                    Else
                        strsql &= " dbo.convert5FC('" & gbApp & "','" & gbComp & "','" & gbYear & "',  INVH_ADDDISC  ,invh_curr  ,'" & gbCurr1 & "',INVH_RATE,INVH_Date) INVH_ADDDISC,"
                    End If
                ElseIf curr = gbCurr2 Then
                    If Copercurr = "FC" Then
                        strsql &= " INVH_ADDDISC,"
                    Else
                        strsql &= " dbo.convert5FC('" & gbApp & "','" & gbComp & "','" & gbYear & "', INVH_ADDDISC,invh_curr,'" & gbCurr2 & "',INVH_RATE,INVH_Date) INVH_ADDDISC,"
                    End If
                Else
                    strsql &= " INVH_ADDDISCFC INVH_ADDDISC,"
                End If
            End If

            'Additional Discount Percentage added to hintrag report only ( instead of calculating it in the report ) -Jad
            If RptInvoice.Trim.ToUpper = "RPTINVPOSLASERHINTRAG.RPT" Then
                strsql &= " INVH_AddDiscPerc , "
                'Rita M @ 19.05.2021. Payment mode is added only to hintrag report. If exist at lease one payment mode that is not On A/C or additional discount or dead balance, then the invoice
                'is considered as cash
                strsql &= " CASE WHEN ISNULL(Cash,0) > 0 THEN 'Y' ELSE 'N' END PayCash,"
            End If

            'GS on 10/05/16
            If curr = "" Then 'added by GS on 10/05/16
                strsql &= "" & CStr(IIf(Copercurr = "FC", "INVH_VATFCNEW", "")) & " INVH_VATFC, "
                'added by GS on 10/05/16
            Else
                If curr = gbCurr1 Then
                    '   strsql &= " dbo.convert5FC('" & gbApp & "','" & gbComp & "','" & gbYear & "', INVH_VATFC" & CStr(IIf(Copercurr = "FC", "NEW", "")) & ",invh_curr,'" & gbCurr1 & "',INVH_RATE,INVH_Date) INVH_VATFC,"
                    strsql &= " INVH_VATLC INVH_VATFC,"
                ElseIf curr = gbCurr2 Then
                    If Copercurr = "FC" Then
                        strsql &= " INVH_VATFC,"
                    Else
                        strsql &= " dbo.convert5FC('" & gbApp & "','" & gbComp & "','" & gbYear & "', INVH_VATFC" & CStr(IIf(Copercurr = "FC", "NEW", "")) & ",invh_curr,'" & gbCurr2 & "',INVH_RATE,INVH_Date) INVH_VATFC,"
                    End If
                Else
                    strsql &= " INVH_VATFCNEW INVH_VATFC,"
                End If
            End If

            '...JAD - Vat Hintrag
            'strsql &= " dbo.convert5FC('" & gbApp & "','" & gbComp & "','" & gbYear & "',INVH_VATLC,'" & gbCurr1 & "','" & gbCurr2 & "',INVH_VATLCRate,INVH_DATE) INVH_VATCC, "
            strsql &= " CASE WHEN OP_OPCurr = 'FC' or invh_curr = '" & gbCurr2 & "' THEN INVH_VATFC else "
            strsql &= " dbo.convert5FC('" & gbApp & "','" & gbComp & "','" & gbYear & "', INVH_VATLC, '" & gbCurr1 & "','" & gbCurr2 & "',INVH_VATLCRate, INVH_DATE) END INVH_VATCC, "
            '...

            'GS on 10/05/16
            If ItmCetificate Then
                strsql &= " 0  INVD_VATVAL , "
            Else
                If curr = "" Then 'added by GS on 10/05/16
                    strsql &= " isnull(" & CStr(IIf(PrintExpLot, "INVD_VATVAL", "sum(INVD_VATVAL)")) & ",0) INVD_VATVAL , "
                    'added by GS on 10/05/16
                Else
                    If curr = gbCurr1 Then
                        '   strsql &= " dbo.convert5FC('" & gbApp & "','" & gbComp & "','" & gbYear & "', isnull(" & CStr(IIf(PrintExpLot, "ISNULL(NULLIF(INVD_VATVALfc,0),INVD_VATVAL)", "sum(ISNULL(NULLIF(INVD_VATVALfc,0),INVD_VATVAL))")) & ",0),ISNULL(NULLIF(invh_currfc,''),invh_curr),'" & gbCurr1 & "',ISNULL(NULLIF(INVH_RATEfc,0),INVH_RATE),INVH_Date) INVD_VATVAL,"
                        'If Copercurr = "FC" Then
                        'strsql &= " case when isnull(invh_currfc,'') = '" & gbCurr1 & "'  then " & IIf(PrintExpLot, "INVD_VATVALfc", "Sum(INVD_VATVALfc)").ToString & "  else "
                        'strsql &= "  dbo.convert5FC('" & gbApp & "','" & gbComp & "','" & gbYear & "',CONVERT (NUMERIC(18,3), dbo.convert5FC('" & gbApp & "','" & gbComp & "','" & gbYear & "'," & IIf(PrintExpLot, "INVD_VATVALfc", "Sum(INVD_VATVALfc)").ToString & "   ,invh_currfc  ,'" & gbCurr2 & "',INVH_RATEfc,INVH_Date) ),'" & gbCurr2 & "'  ,'" & gbCurr1 & "',INVH_VATLCRate,INVH_Date)  end "
                        'Else
                        '    strsql &= " dbo.convert5FC('" & gbApp & "','" & gbComp & "','" & gbYear & "',  " & IIf(PrintExpLot, "INVD_VATVAL", "Sum(INVD_VATVAL)").ToString & "      ,invh_curr  ,'" & gbCurr1 & "',INVH_RATE,INVH_Date) INVD_VATVAL,"
                        'End If
                        strsql &= " isnull(" & CStr(IIf(PrintExpLot, "INVD_VATVALLC", "sum(INVD_VATVALLC)")) & ",0) INVD_VATVAL,"
                    ElseIf curr = gbCurr2 Then
                        If Copercurr = "FC" Then
                            strsql &= "  isnull(" & CStr(IIf(PrintExpLot, "INVD_VATVAL", "sum(INVD_VATVAL)")) & ",0) INVD_VATVAL,"
                        Else
                            strsql &= " dbo.convert5FC('" & gbApp & "','" & gbComp & "','" & gbYear & "', isnull(" & CStr(IIf(PrintExpLot, "INVD_VATVAL", "sum(INVD_VATVAL)")) & ",0),invh_curr,'" & gbCurr2 & "',INVH_VATLCRate,INVH_Date) INVD_VATVAL,"
                        End If
                    Else
                        strsql &= "  isnull(" & CStr(IIf(PrintExpLot, "INVD_VATVALFC", "sum(INVD_VATVALFC)")) & ",0) INVD_VATVAL, "
                    End If
                End If
            End If

            'GS on 10/05/16
            If ItmCetificate Then
                strsql &= " 0  INVH_NET ,0 INVH_VATLC ,  "
            Else

                If curr = "" Then 'added by GS on 10/05/16
                    strsql &= CStr(IIf(Copercurr = "FC", " INVH_NETFC ", "")) & " INVH_NET, "
                    'added by GS on 10/05/16
                Else
                    If curr = gbCurr1 Then
                        'strsql &= " dbo.convert5FC('" & gbApp & "','" & gbComp & "','" & gbYear & "', ISNULL(NULLIF(INVH_NETfc,0),INVH_NET)  ,ISNULL(NULLIF(invh_currfc,''),invh_curr)  ,'" & gbCurr1 & "',ISNULL(NULLIF(INVH_RATEfc,0),INVH_RATE)   ,INVH_Date) INVH_NET,"
                        If Copercurr = "FC" Then
                            'strsql &= " case when isnull(invh_currfc,'') = '" & gbCurr1 & "'  then  INVH_NETfc else "
                            'strsql &= "  dbo.convert5FC('" & gbApp & "','" & gbComp & "','" & gbYear & "',CONVERT (NUMERIC(18,3),dbo.convert5FC('" & gbApp & "','" & gbComp & "','" & gbYear & "',INVH_NETfc  ,invh_currfc ,'" & gbCurr2 & "',INVH_RATEfc,INVH_Date)),'" & gbCurr2 & "'  ,'" & gbCurr1 & "',INVH_RATE,INVH_Date)   end  INVH_NET,"

                            strsql &= " case when isnull(invh_currfc,'') = '" & gbCurr1 & "'  then INVH_NETfc else "
                            strsql &= " CASE WHEN INVH_VATTYPE <> 'A' then "
                            strsql &= "  dbo.convert5FC('" & gbApp & "','" & gbComp & "','" & gbYear & "',CONVERT (NUMERIC(18,3) ,dbo.convert5FC('" & gbApp & "','" & gbComp & "','" & gbYear & "', INVH_NETfc ,invh_currfc  ,'" & gbCurr2 & "',INVH_RATEfc,INVH_Date) ),'" & gbCurr2 & "'  ,'" & gbCurr1 & "',INVH_RATE,INVH_Date)  "
                            strsql &= " ELSE /*Since VAT rate can be different than invoice rate, we have to recalculate the net */  "
                            strsql &= "  dbo.convert5FC('" & gbApp & "','" & gbComp & "','" & gbYear & "',CONVERT (NUMERIC(18,3) ,dbo.convert5FC('" & gbApp & "','" & gbComp & "','" & gbYear & "', INVH_NETfc - INVH_VATFCNEW ,invh_currfc  ,'" & gbCurr2 & "',INVH_RATEfc,INVH_Date) ),'" & gbCurr2 & "'  ,'" & gbCurr1 & "',INVH_RATE,INVH_Date)  "
                            strsql &= "  + INVH_VATLC end "
                            strsql &= " end  INVH_NET,"
                        Else
                            'strsql &= " dbo.convert5FC('" & gbApp & "','" & gbComp & "','" & gbYear & "',  INVH_NET  ,invh_curr  ,'" & gbCurr1 & "',INVH_RATE,INVH_Date) INVH_NET,"

                            strsql &= " dbo.convert5FC('" & gbApp & "','" & gbComp & "','" & gbYear & "',  INVH_NET - INVH_VATFC ,invh_curr  ,'" & gbCurr1 & "',INVH_RATE,INVH_Date) + INVH_VATLC INVH_NET,"
                        End If
                    ElseIf curr = gbCurr2 Then
                        If Copercurr = "FC" Then
                            strsql &= " INVH_NET,"
                        Else
                            strsql &= " dbo.convert5FC('" & gbApp & "','" & gbComp & "','" & gbYear & "', INVH_NET,invh_curr,'" & gbCurr2 & "',INVH_RATE,INVH_Date) INVH_NET,"
                        End If
                    Else
                        strsql &= " INVH_NETFC INVH_NET,"
                    End If
                End If
                strsql &= "INVH_VATLC,"
            End If

            'GS on 10/05/16

            If ShowBundleComm = True Then
                strsql &= " CASE WHEN rectype = 'b' AND " & IIf(PrintExpLot, "INVD_NET", "sUM(INVD_NET)").ToString & " < 0 THEN 'X' else INVD_STATUS END INVD_STATUS,"
            Else
                strsql &= " INVD_STATUS , "
            End If
            strsql &= " INVD_ITMSTATUS, CAST(INVD_ITMDESC AS NVARCHAR(max)) INVD_ITMDESC, "

            If PrintExpLot Then
                strsql &= " INVD_QTY, "
            Else
                strsql &= " sUM(INVD_QTY) INVD_QTY ,"
            End If

            'GS on 10/05/16
            If ItmCetificate Then
                strsql &= " 0  INVD_UPRICE ,0  INVD_DISC , 0 invd_discval ,0 INVD_NET , "
            Else
                If curr = "" Then 'added by GS on 10/05/16
                    strsql &= CStr(IIf(Copercurr = "FC", " INVD_UPRICEFC ", "")) & " INVD_UPRICE, "
                    'added by GS on 10/05/16
                Else
                    If curr = gbCurr1 Then
                        'strsql &= " dbo.convert5FC('" & gbApp & "','" & gbComp & "','" & gbYear & "',ISNULL(NULLIF(INVD_UPRICEfc,0),INVD_UPRICE) ,ISNULL(NULLIF(invh_currfc,''),invh_curr) ,'" & gbCurr1 & "',ISNULL(NULLIF(INVH_RATEfc,0),INVH_RATE),INVH_Date) INVD_UPRICE,"
                        If Copercurr = "FC" Then
                            strsql &= " case when isnull(invh_currfc,'') = '" & gbCurr1 & "'  then  INVD_UPRICEfc else "
                            strsql &= "  dbo.convert5FC('" & gbApp & "','" & gbComp & "','" & gbYear & "',CONVERT (NUMERIC(18,3) ,dbo.convert5FC('" & gbApp & "','" & gbComp & "','" & gbYear & "',INVD_UPRICEfc  ,invh_currfc  ,'" & gbCurr2 & "',INVH_RATEfc,INVH_Date)),'" & gbCurr2 & "'  ,'" & gbCurr1 & "',INVH_RATE,INVH_Date)   end  INVD_UPRICE,"
                        Else
                            strsql &= " dbo.convert5FC('" & gbApp & "','" & gbComp & "','" & gbYear & "',  INVD_UPRICE  ,invh_curr  ,'" & gbCurr1 & "',INVH_RATE,INVH_Date) INVD_UPRICE,"
                        End If
                    ElseIf curr = gbCurr2 Then
                        If Copercurr = "FC" Then
                            strsql &= " INVD_UPRICE,"
                        Else
                            strsql &= " dbo.convert5FC('" & gbApp & "','" & gbComp & "','" & gbYear & "', INVD_UPRICE,invh_curr,'" & gbCurr2 & "',INVH_RATE,INVH_Date) INVD_UPRICE,"
                        End If
                    Else
                        strsql &= " INVD_UPRICEFC INVD_UPRICE,"
                    End If
                End If
                strsql &= " INVD_DISC,"
                If curr = "" Then 'added by GS on 10/05/16
                    strsql &= CStr(IIf(Copercurr = "FC", " " & IIf(PrintExpLot, "invd_discvalfc", "max(invd_discvalfc)").ToString & " ", IIf(PrintExpLot, "invd_discval", "max(invd_discval)").ToString)) & " invd_discval  , "
                    'added by GS on 10/05/16
                Else
                    If curr = gbCurr1 Then
                        'strsql &= " dbo.convert5FC('" & gbApp & "','" & gbComp & "','" & gbYear & "', ISNULL(NULLIF(" & IIf(PrintExpLot, "invd_discvalfc", "max(invd_discvalfc)").ToString & ",0)," & IIf(PrintExpLot, "invd_discval", "max(invd_discval)").ToString & ") ,ISNULL(NULLIF(invh_currfc,''),invh_curr)  ,'" & gbCurr1 & "',ISNULL(NULLIF(INVH_RATEfc,0),INVH_RATE) ,INVH_Date) invd_discval,"
                        If Copercurr = "FC" Then
                            strsql &= " case when isnull(invh_currfc,'') = '" & gbCurr1 & "'  then  " & IIf(PrintExpLot, "invd_discvalfc", "max(invd_discvalfc)").ToString & " else "
                            strsql &= "  dbo.convert5FC('" & gbApp & "','" & gbComp & "','" & gbYear & "', CONVERT (NUMERIC(18,3) ,dbo.convert5FC('" & gbApp & "','" & gbComp & "','" & gbYear & "'," & IIf(PrintExpLot, "invd_discvalfc", "max(invd_discvalfc)").ToString & "  ,invh_currfc  ,'" & gbCurr2 & "',INVH_RATEfc,INVH_Date)),'" & gbCurr2 & "'  ,'" & gbCurr1 & "',INVH_RATE,INVH_Date)   end  invd_discval,"
                        Else
                            strsql &= " dbo.convert5FC('" & gbApp & "','" & gbComp & "','" & gbYear & "',  " & IIf(PrintExpLot, "invd_discval", "max(invd_discval)").ToString & "  ,invh_curr  ,'" & gbCurr1 & "',INVH_RATE,INVH_Date) invd_discval,"
                        End If
                    ElseIf curr = gbCurr2 Then
                        If Copercurr = "FC" Then
                            strsql &= IIf(PrintExpLot, "invd_discval", "max(invd_discval)").ToString & " invd_discval,"
                        Else
                            strsql &= " dbo.convert5FC('" & gbApp & "','" & gbComp & "','" & gbYear & "', " & IIf(PrintExpLot, "invd_discval", "max(invd_discval)").ToString & " ,invh_curr,'" & gbCurr2 & "',INVH_RATE,INVH_Date) invd_discval,"
                        End If
                    Else
                        strsql &= " " & IIf(PrintExpLot, "invd_discvalfc", "max(invd_discvalfc)").ToString & " invd_discval,"
                    End If
                End If
                If curr = "" Then 'added by GS on 10/05/16
                    If PrintExpLot Then
                        strsql &= CStr(IIf(Copercurr = "FC", " INVD_NETFC ", "")) & " INVD_NET, "
                    Else
                        strsql &= " sUM(" & CStr(IIf(Copercurr = "FC", "INVD_NETFC", "INVD_NET")) & ") INVD_NET , "
                    End If

                    'added by GS on 10/05/16
                Else
                    If curr = gbCurr1 Then
                        'If PrintExpLot Then
                        '    strsql &= " dbo.convert5FC('" & gbApp & "','" & gbComp & "','" & gbYear & "', ISNULL(NULLIF(INVD_NETfc,0),INVD_NET) ,ISNULL(NULLIF(invh_currfc,''),invh_curr),'" & gbCurr1 & "',ISNULL(NULLIF(INVH_RATEfc,0),INVH_RATE),INVH_Date) INVD_NET,"

                        'Else
                        '    strsql &= " dbo.convert5FC('" & gbApp & "','" & gbComp & "','" & gbYear & "', sUM(ISNULL(NULLIF(INVD_NETfc,0),INVD_NET)),ISNULL(NULLIF(invh_currfc,''),invh_curr),'" & gbCurr1 & "',ISNULL(NULLIF(INVH_RATEfc,0),INVH_RATE),INVH_Date) INVD_NET,"
                        'End If
                        If Copercurr = "FC" Then
                            strsql &= " case when isnull(invh_currfc,'') = '" & gbCurr1 & "'  then  " & IIf(PrintExpLot, "INVD_NETfc", "sUM(INVD_NETfc)").ToString & " else "
                            strsql &= "  dbo.convert5FC('" & gbApp & "','" & gbComp & "','" & gbYear & "',CONVERT (NUMERIC(18,3) ,dbo.convert5FC('" & gbApp & "','" & gbComp & "','" & gbYear & "'," & IIf(PrintExpLot, "INVD_NETfc", "SUM(INVD_NETfc)").ToString & "  ,invh_currfc  ,'" & gbCurr2 & "',INVH_RATEfc,INVH_Date) ),'" & gbCurr2 & "'  ,'" & gbCurr1 & "',INVH_RATE,INVH_Date)  end  INVD_NET,"
                        Else
                            strsql &= " dbo.convert5FC('" & gbApp & "','" & gbComp & "','" & gbYear & "',  " & IIf(PrintExpLot, "INVD_NET", "SUM(INVD_NET)").ToString & "  ,invh_curr  ,'" & gbCurr1 & "',INVH_RATE,INVH_Date) INVD_NET,"
                        End If
                    ElseIf curr = gbCurr2 Then
                        If Copercurr = "FC" Then
                            If PrintExpLot Then
                                strsql &= " INVD_NET,"
                            Else
                                strsql &= " sUM(INVD_NET) INVD_NET,"
                            End If
                        Else
                            If PrintExpLot Then
                                strsql &= " dbo.convert5FC('" & gbApp & "','" & gbComp & "','" & gbYear & "', INVD_NET,invh_curr,'" & gbCurr2 & "',INVH_RATE,INVH_Date) INVD_NET,"
                            Else
                                strsql &= " dbo.convert5FC('" & gbApp & "','" & gbComp & "','" & gbYear & "', sUM(INVD_NET),invh_curr,'" & gbCurr2 & "',INVH_RATE,INVH_Date) INVD_NET,"
                            End If
                        End If
                    Else
                        If PrintExpLot Then
                            strsql &= " INVD_NETFC INVD_NET,"
                        Else
                            strsql &= " sUM(INVD_NETFC) INVD_NET,"
                        End If
                    End If
                End If
            End If


            'GS on 10/05/16

            'GS on 10/05/16
            strsql &= " INVD_GIFT, POS_Employee_1.E_Name SalesManName, STK04_Location.Loc_Desc, POS_EMPLOYEE.E_Name,"
            strsql &= " Comp_Addr Fo_addres, Comp_Phone1 Fo_phone1, "

            If RptInvoice.ToUpper = "rptInvPosLaserEmileAoun.rpt".ToUpper Then strsql &= "Comp_Phone2 Fo_phone2,"

            'CH @ 27/02/2017
            If rbSingleRef2 = True Then
                strsql &= " STK_SingleRef2 STK_SingleRef1, "
            Else
                strsql &= " STK_SingleRef1 , "
            End If
            If PrintExpLot Then
                strsql &= " case when isnull(INVD_BARCODE,'')='' then " & CStr(IIf(RptInvoice.Trim = "RPTINVPOSLASERHINTRAG.RPT", "STK_SingleRef2", "stk_ref")) & " else INVD_BARCODE end INVD_BARCODE, "
            Else
                strsql &= " case when isnull(max(INVD_BARCODE),'')='' then " & CStr(IIf(RptInvoice.Trim = "RPTINVPOSLASERHINTRAG.RPT", "STK_SingleRef2", "stk_ref")) & " else max(INVD_BARCODE) end INVD_BARCODE, "
            End If
            strsql &= " stk_ref, " '...Jad-J...WF:4862
            strsql &= " POS_STKFIL.STK_VATTYPE, INVD_DetGrid,INVD_Color,Color_Reference color_desc, "
            If PrintExpLot Then
                strsql &= " CASE WHEN SUBSTRING(INVD_ExpDate, 1, 3) = '00.' "
                strsql &= "  THEN SUBSTRING(INVD_ExpDate, 4, LEN(INVD_ExpDate))   ELSE INVD_ExpDate  END as INVD_ExpDate ,INVD_Batch, "
            Else
                strsql &= " CASE WHEN SUBSTRING(max(INVD_ExpDate), 1, 3) = '00.' "
                strsql &= "  THEN SUBSTRING(max(INVD_ExpDate), 4, LEN(max(INVD_ExpDate)))   ELSE max(INVD_ExpDate)  END as INVD_ExpDate ,MAX(INVD_Batch) INVD_Batch, "
            End If
            'strsql &= " POS_STKFIL.STK_SUPREF,"
            strsql &= " Bibl1_Book.BO_ISBN, Invh_Head1 stt_printed1, Invh_Head2 stt_printed2, Invh_Footer stt_footer,"
            If PrintExpLot Then
                strsql &= " isnull((INVD_TechSheetDesc),'') DSTech_Meas,isnull(dbo.FromRTFtoText(INVD_TechSheetDesc),'') DSTech_Measrtf,STK_FourthParty, "
            Else
                strsql &= " isnull(MAX(INVD_TechSheetDesc),'') DSTech_Meas,isnull(max(dbo.FromRTFtoText(INVD_TechSheetDesc)),'') DSTech_Measrtf,STK_FourthParty, "
            End If
            strsql &= " Trn_CardNo, isnull(Clt_FirstName,'') + ' ' + isnull(Clt_FamilyName,'') PrivCardName, Card_Balance, Trn_Points,"

            'Rachel A. 24.03.2016   
            If PrintExpLot Then
                strsql &= " dbo.FromRTFtoText(INVD_Remarks) INVD_Remarks , INVD_Remarks  INVD_RemarksRTF ," 'Adding remark in report 
            Else
                strsql &= " MAX(dbo.FromRTFtoText(INVD_Remarks)) INVD_Remarks ,  MAX(INVD_Remarks) INVD_RemarksRTF , " 'Adding remark in report 
            End If
            'added by GS on 14/07/16
            If RptInvoice = "RptFaircoReportPrint.rpt" Or RptInvoice = "RptFaircoReportView.rpt" Then
                strsql &= " meas_desc  AS  [PACKUNIT] ,ISNULL(fo_vatreg,'') as [fo_vatreg] ,   "
            Else
                strsql &= " STK_UNIt  AS  [PACKUNIT] ,ISNULL(fo_vatreg,'') as [fo_vatreg] ,   "
            End If
            If ItmCetificate Then
                strsql &= "   0 [withoutdiisc] ,"
            Else
                If curr = "" Then
                    If PrintExpLot Then
                        strsql &= "   CONVERT (NUMERIC(18,3),INVD_UPRICE" & CStr(IIf(Copercurr = "FC", "FC", "")) & " * INVD_QTY) as[withoutdiisc],  "
                    Else
                        strsql &= "   CONVERT (NUMERIC(18,3),INVD_UPRICE" & CStr(IIf(Copercurr = "FC", "FC", "")) & " * MAX(INVD_QTY)) as[withoutdiisc],  "
                    End If
                Else
                    If curr = gbCurr1 Then
                        'If PrintExpLot Then
                        '    strsql &= " dbo.convert5FC('" & gbApp & "','" & gbComp & "','" & gbYear & "', CONVERT (NUMERIC(18,2),ISNULL(NULLIF(INVD_UPRICEfc,0),INVD_UPRICE) * INVD_QTY),ISNULL(NULLIF(invh_currfc,''),invh_curr),'" & gbCurr1 & "',ISNULL(NULLIF(INVH_RATEfc,0),INVH_RATE),INVH_Date) [withoutdiisc],"
                        'Else
                        '    strsql &= " dbo.convert5FC('" & gbApp & "','" & gbComp & "','" & gbYear & "', CONVERT (NUMERIC(18,2),ISNULL(NULLIF(INVD_UPRICEfc,0),INVD_UPRICE) * MAX(INVD_QTY)),ISNULL(NULLIF(invh_currfc,''),invh_curr),'" & gbCurr1 & "',ISNULL(NULLIF(INVH_RATEfc,0),INVH_RATE),INVH_Date) [withoutdiisc],"
                        'End If
                        If Copercurr = "FC" Then
                            strsql &= " case when isnull(invh_currfc,'') = '" & gbCurr1 & "'  then CONVERT (NUMERIC(18,3), " & IIf(PrintExpLot, "CONVERT (NUMERIC(18,3), INVD_UPRICEfc  * INVD_QTY)", "CONVERT (NUMERIC(18,3), INVD_UPRICEfc  * max(INVD_QTY))").ToString & " ) else "
                            strsql &= " dbo.convert5FC('" & gbApp & "','" & gbComp & "','" & gbYear & "', CONVERT (NUMERIC(18,3) ,dbo.convert5FC('" & gbApp & "','" & gbComp & "','" & gbYear & "'," & IIf(PrintExpLot, "CONVERT (NUMERIC(18,3), INVD_UPRICEfc  * INVD_QTY)", "CONVERT (NUMERIC(18,3), INVD_UPRICEfc  * max(INVD_QTY))").ToString & " ,invh_currfc  ,'" & gbCurr2 & "',INVH_RATEfc,INVH_Date)) ,'" & gbCurr2 & "'  ,'" & gbCurr1 & "',INVH_RATE,INVH_Date)  end  [withoutdiisc],"
                        Else
                            strsql &= " dbo.convert5FC('" & gbApp & "','" & gbComp & "','" & gbYear & "', " & IIf(PrintExpLot, "CONVERT (NUMERIC(18,3), INVD_UPRICE  * INVD_QTY)", "CONVERT (NUMERIC(18,3), INVD_UPRICE  * max(INVD_QTY))").ToString & "  ,invh_curr  ,'" & gbCurr1 & "',INVH_RATE,INVH_Date) [withoutdiisc],"
                        End If
                    ElseIf curr = gbCurr2 Then
                        If Copercurr = "FC" Then
                            If PrintExpLot Then
                                strsql &= "   CONVERT (NUMERIC(18,3),INVD_UPRICE * INVD_QTY) as[withoutdiisc],  "
                            Else
                                strsql &= "   CONVERT (NUMERIC(18,3),INVD_UPRICE * MAX(INVD_QTY)) as[withoutdiisc],  "
                            End If
                        Else
                            If PrintExpLot Then
                                strsql &= " dbo.convert5FC('" & gbApp & "','" & gbComp & "','" & gbYear & "', CONVERT (NUMERIC(18,3),INVD_UPRICE * INVD_QTY),invh_curr,'" & gbCurr2 & "',INVH_RATE,INVH_Date) [withoutdiisc],"
                            Else
                                strsql &= " dbo.convert5FC('" & gbApp & "','" & gbComp & "','" & gbYear & "', CONVERT (NUMERIC(18,3),INVD_UPRICE * MAX(INVD_QTY)),invh_curr,'" & gbCurr2 & "',INVH_RATE,INVH_Date) [withoutdiisc],"
                            End If
                        End If

                    Else
                        If PrintExpLot Then
                            strsql &= "  CONVERT (NUMERIC(18,3),INVD_UPRICEFC * INVD_QTY) as[withoutdiisc],  "
                        Else
                            strsql &= "  CONVERT (NUMERIC(18,3),INVD_UPRICEFC * MAX(INVD_QTY)) as[withoutdiisc],  "
                        End If
                    End If
                End If
            End If



            If RptInvoice = "RptFaircoReportPrint.rpt" Or RptInvoice = "RptFaircoReportView.rpt" _
                Or RptInvoice = "rptInvPosLaserOPP.rpt" Or RptInvoice = "rptInvPosLaserOPPtoprint.rpt" Then
                strsql &= " Docon ,"
            Else
                strsql &= " '' as Docon, "
            End If

            If RptInvoice = "rptInvPosLaser.rpt" Or RptInvoice = "rptInvPosLaserItemCertificate.rpt" Or RptInvoice = "rptInvPosLaserSansirab.rpt" _
                Or RptInvoice = "rptInvPosLaserOPP.rpt" Or RptInvoice = "rptInvPosLaserOPPtoprint.rpt" OrElse
                RptInvoice = "RptFaircoReportPrint.rpt" Or RptInvoice = "RptFaircoReportView.rpt" Or RptInvoice = "rptInvPosLaserMaliks.rpt" Or RptInvoice = "rptInvPosLaserDoculand.rpt" _
                OrElse RptInvoice = "rptInvPosLaserKhayyatKanaan.rpt" OrElse RptInvoice = "rptInvPosLaserNovamart.rpt" OrElse
                RptInvoice = "rptInvPosLaserISM.rpt" OrElse RptInvoice = "rptInvPosLaserMedix.rpt" OrElse
                RptInvoice = "hintrag01.rpt" OrElse RptInvoice = "rptInvPosLaserMountainSteel.rpt" OrElse RptInvoice.ToUpper = "rptInvPosLaserEmileAoun.rpt".ToUpper Then

                strsql &= " SDCon ,"
            Else
                strsql &= " '' as SDcon , "
            End If

            'If RptInvoice = "rptInvPosLaserHintrag.rpt" Or RptInvoice = "rptInvPosLaserOPP.rpt" Or RptInvoice = "rptInvPosLaserOPPtoprint.rpt" _
            '    OrElse PrintClBalance = True Then
            If PrintClBalance = True Then
                If RptInvoice = "rptInvPosLaserOPP.rpt" Then
                    If gbCurr1 = IIf(curr = "", Copercurr, curr).ToString Then
                        strsql &= "  isnull(ClinetBalnceSumtable.BalancelC,0) Currentbalance, '' AS ClLastTransaction, 0 ClBalanceDue , "
                    ElseIf IIf(curr = "", Copercurr, curr).ToString = gbCurr2 Then
                        strsql &= "  isnull(ClinetBalnceSumtable.BalanceCC,0) Currentbalance, '' AS ClLastTransaction, 0 ClBalanceDue , "
                    Else
                        strsql &= " dbo.convert5('" & gbApp & "','" & gbComp & "','" & gbYear & "',  isnull(ClinetBalnceSumtable.BalanceCC,0),'" & gbCurr2 & "',invh_currfc,'" & Format(CDate(WorkingDay), "MM/dd/yyyy") & "') "
                        strsql &= "  Currentbalance, '' AS ClLastTransaction, 0 ClBalanceDue , "
                    End If



                Else

                    '...Jad-J...WF:4862  WF:5001 --- Get Current Balance / last transaction Balance Due from Accounting Database
                    If RecordsExists("SELECT App_Code FROM dbo.Com_Application WHERE App_Code IN ('ACC01', 'MAG')") AndAlso PrintClBalance = True Then

                        ModVar.LinkServerGlobalVariables(lcServerLink, lcDBnameLink, lcSQLpswdLink, lcSQLuserLink, lcSQLAuth, gbComp)
                        If CheckConnection(lcServerLink, lcDBnameLink, CBool(IIf(lcSQLAuth = "Y", True, False)), lcSQLuserLink, lcSQLpswdLink) = True Then

                            ModVar.LinkServer(lcServerLink, lcSQLuserLink, lcSQLpswdLink, lcDBnameLink, lcSQLAuth, 0, "1", ServerName)
                            GetAcYear(gbApp, gbComp, gbYear)

                            lcDBnameLink = "[" & lcDBnameLink & "]"

                            If Not (curr = "" Or curr = "FC") Then strsql &= " CASE WHEN invh_curr" & CStr(IIf(Copercurr = "FC", "FC", "")) & " = '" & curr & "' THEN "
                            strsql &= " CASE WHEN INVH_CURR = '" & gbCurr1 & "' THEN Currentbalance.Trn_amtlc"
                            strsql &= " WHEN INVH_CURR = '" & gbCurr2 & "' THEN Currentbalance.Trn_amtcc"
                            strsql &= " ELSE Currentbalance.Trn_amtfc END"
                            If Not (curr = "" Or curr = "FC") Then strsql &= " else 0 end "
                            strsql &= " AS Currentbalance ,"

                            If Not (curr = "" Or curr = "FC") Then strsql &= " CASE WHEN invh_curr" & CStr(IIf(Copercurr = "FC", "FC", "")) & " = '" & curr & "' THEN "
                            strsql &= " Currentbalance.maxtransDate "
                            If Not (curr = "" Or curr = "FC") Then strsql &= " else '' end "
                            strsql &= " AS ClLastTransaction ,"

                            If Not (curr = "" Or curr = "FC") Then strsql &= " CASE WHEN invh_curr" & CStr(IIf(Copercurr = "FC", "FC", "")) & " = '" & curr & "' THEN "
                            strsql &= " CASE WHEN INVH_CURR = '" & gbCurr1 & "' THEN   ClBalanceDue.BALdueAmtLC "
                            strsql &= " WHEN INVH_CURR = '" & gbCurr2 & "' THEN   ClBalanceDue.BALdueAmtCC "
                            strsql &= " ELSE   ClBalanceDue.BALdueAmtFC END"
                            If Not (curr = "" Or curr = "FC") Then strsql &= " else 0 end "
                            strsql &= " AS ClBalanceDue  ,"

                        Else
                            MessageBox.Show("No Connection With Accounting Server, Client Balance And Last Transaction Will Not Be Printed", "Connecting To Accounting DB", MessageBoxButtons.OK, MessageBoxIcon.Information)
                            strsql &= "  0 Currentbalance, '' AS ClLastTransaction, 0 ClBalanceDue , "

                            InvalidBalanceMsg = "Invalid Data"

                        End If

                    Else
                        strsql &= "  0 Currentbalance, '' AS ClLastTransaction, 0 ClBalanceDue , " '...Jad-J...WF:4862  // WF:5001
                        InvalidBalanceMsg = "Invalid Data"
                    End If
                End If


            Else
                strsql &= "  0 Currentbalance, '' AS ClLastTransaction, 0 ClBalanceDue , "
            End If

            'ch @ 05/10/2018 mail : ERP_Stock/POS Operations_Printing Malek Invoice: Add Due Date and SO# 
            If PrintLinkBTWInvAndCN = False Then
                strsql &= "    (SELECT INVH_LOC +'-' + INVH_DOCTYP1+' '+INVH_DOCPREFIX +' '+ convert(VARCHAR(18), INVH_SERNO )FROM dbo.POS_SALEINVhead WHERE INVH_LOC=INVCN_InvLoc AND INVH_RECID=INVCN_InvHeadID) importedFrom , "
            Else
                strsql &= " COALESCE ( (SELECT INVH_LOC +'-' + INVH_DOCTYP1+' '+INVH_DOCPREFIX +' '+ convert(VARCHAR(18), INVH_SERNO )FROM dbo.POS_SALEINVhead WHERE INVH_LOC=INVCN_InvLoc AND INVH_RECID=INVCN_InvHeadID) "
                strsql &= " , (SELECT    HOFR_LOC + ' ' +  HOFR_Prefix + '/' + CONVERT(VARCHAR(18), HOFR_recid)  + '-' + CONVERT(VARCHAR(18), HOFR_SeqNo)FROM      stk04_offerheader  WHERE     HOFR_HeadRecId = INVCO_OfferRecId  AND INVCO_OfferLoc = HOFR_LOC    ) "
                strsql &= " , (SELECT    DOH_Loc + ' ' +  DOH_DocTyp1 + '/' + DOH_DocPrefix  + '-' + CONVERT(VARCHAR(18), DOH_Serno) FROM      STK04_DelOrdH   WHERE     DOH_RecId = INVDO_DORecId  AND DOH_Loc = INVDO_DOLoc   )"
                strsql &= "  ) importedFrom , "
            End If


            If RptInvoice = "rptInvPosLaserOPP.rpt" Or RptInvoice = "rptInvPosLaserOPPtoprint.rpt" Then
                strsql &= " STK_SUBUNIT,STK_UNITPACK,ISNULL(STK_OPERATOR,'*') AS STK_OPERATOR, "

                strsql &= " 			 ( SELECT DISTINCT"
                strsql &= "                         SubHEad.INVH_LOC + '-' + SubHEad.INVH_DOCTYP1 + ' '"
                strsql &= "                         + SubHEad.INVH_DOCPREFIX + ' '"
                strsql &= "                         + CONVERT(VARCHAR(18), SubHEad.INVH_SERNO) + '' + CHAR(10) "
                strsql &= "               FROM      dbo.POS_SALEINVhead SubHEad"
                strsql &= "                         INNER JOIN dbo.POS_SALEINVLinkCNItem cnSub ON cnSub.INVCN_InvLoc = SubHEad.INVH_LOC"
                strsql &= "                                                               AND cnSub.INVCN_InvHeadID = SubHEad.INVH_RECID"
                strsql &= "               WHERE     cnSub.INVCN_CNLoc = " & CStr(IIf(ShowBundleComm, "V_Stk04_InvoiceComment", "POS_SALEINVhead")) & ".INVH_LOC"
                strsql &= "                         AND cnSub.INVCN_CNRecId = " & CStr(IIf(ShowBundleComm, "V_Stk04_InvoiceComment", "POS_SALEINVhead")) & ".INVH_RECID"
                strsql &= "             FOR"
                strsql &= "               XML PATH('')"
                strsql &= "             ) importedFromConcat ,"

            End If

            strsql &= " INVH_DOCPRINTEDP , "
            strsql &= " INVH_VATLCRate , "
            'ch @ 01/08/2019 mail(PetMart_Sales invoice/Purchase invoice: Hide company information)
            If RptInvoice.ToUpper = "RPTINVPOSLASERPETMART.RPT" OrElse RptInvoice.ToUpper = "RPTINVPOSLASERPETMARTWORate.RPT" OrElse
                RptInvoice.ToUpper = "RPTINVPOSLASERALLINTRADE.RPT" OrElse RptInvoice.ToUpper = "RPTINVPOSLASERALLINTRADEWORate.RPT" Then
                strsql &= " case when STT_ACCTRANS='Y' then 'True' else 'False' end  STT_ACCTRANS  , "
            End If
            If RptInvoice = "rptInvPosSlip.rpt" Then
                strsql &= "   CASE WHEN OP_OPCurr = '" & gbCurr1 & "'
                                      THEN dbo.convert5FC('" & gbApp & "', '" & gbComp & "', '" & gbYear & "', INVH_NET, '" & gbCurr1 & "', '" & gbCurr2 & "',
                                                          INVH_RATE, INVH_DATE)
                                      ELSE INVH_NET
                                 END INVHNET ,
                                 CASE WHEN OP_OPCurr = '" & gbCurr1 & "'
                                      THEN dbo.convert5FC('" & gbApp & "', '" & gbComp & "', '" & gbYear & "', " & IIf(PrintExpLot, "INVD_NET", "sum(INVD_NET)").ToString & ", '" & gbCurr1 & "',
                                                              '" & gbCurr2 & "', INVH_RATE, INVH_DATE)
                                      ELSE " & IIf(PrintExpLot, "INVD_NET", "sum(INVD_NET)").ToString & "
                                 END INVD_NETFC, "
            End If

            If RptInvoice = "rptInvPosLaserMedix.rpt" Then
                strsql &= " CASE WHEN SISF_RealDiscountPerc IS NOT NULL
                                 THEN CONCAT('-S', CONVERT(VARCHAR, CAST(SISF_RealDiscountPerc AS INT)))
                                 ELSE ''
                            END serviceRef, 
                            STK_ReturnSale ,"
            End If

            If RptInvoice = "rptInvPosLaserISM.rpt" Then
                strsql &= " CONVERT(VARCHAR(max),SUBSTRING(( SELECT  DISTINCT
                                                +', ' + VEH_Vin + ' ' AS 'data()'
                                        FROM    STK04_VehiclesLinkToSales
                                                INNER JOIN ( SELECT VEH_RecID ,
                                                                    VEH_Vin
                                                             FROM   dbo.STK04_VehiclesDefinition
                                                           ) STK04_VehiclesDefinition ON VEH_RecID = VLS_VEHRecID
                                        WHERE   VLS_INVHRecID = INVH_RECID
                                                AND VLS_INVHLoc = '" & Cloc & "'
                                      FOR
                                        XML PATH('')
                                      ), 2, 8000)) AS [VEH_Vin] , "
            End If

            If RptInvoice.ToUpper = "RPTINVPOSLASERHINTRAG.RPT" Then
                strsql &= $"   CASE WHEN {IIf(PrintExpLot, "INVD_QTY", "SUM(INVD_QTY)")} >= STK_UNITPACK
                                   THEN CASE WHEN STK_OPERATOR = '/'
                                           THEN FLOOR({IIf(PrintExpLot, "INVD_QTY", "SUM(INVD_QTY)")} / STK_UNITPACK)
                                           ELSE FLOOR({IIf(PrintExpLot, "INVD_QTY", "SUM(INVD_QTY)")} * STK_UNITPACK)
                                       END
                                   WHEN {IIf(PrintExpLot, "INVD_QTY", "SUM(INVD_QTY)")} < STK_UNITPACK
                                       AND STK_UNIT = 'Box' THEN FLOOR({IIf(PrintExpLot, "INVD_QTY", "SUM(INVD_QTY)")})
                                   ELSE 0
                           END BOX ,
                           CASE WHEN {IIf(PrintExpLot, "INVD_QTY", "SUM(INVD_QTY)")} > STK_UNITPACK
                                   THEN CASE WHEN STK_OPERATOR = '/'
                                           THEN FLOOR({IIf(PrintExpLot, "INVD_QTY", "SUM(INVD_QTY)")} - ( ( FLOOR({IIf(PrintExpLot, "INVD_QTY", "SUM(INVD_QTY)")}
                                                                               / STK_UNITPACK) )
                                                                       * STK_UNITPACK ))
                                           ELSE FLOOR({IIf(PrintExpLot, "INVD_QTY", "SUM(INVD_QTY)")} - ( ( FLOOR({IIf(PrintExpLot, "INVD_QTY", "SUM(INVD_QTY)")}
                                                                               * STK_UNITPACK) )
                                                                       * STK_UNITPACK ))
                                       END
                                   WHEN {IIf(PrintExpLot, "INVD_QTY", "SUM(INVD_QTY)")} < STK_UNITPACK
                                       AND STK_UNIT = 'pcs' THEN FLOOR({IIf(PrintExpLot, "INVD_QTY", "SUM(INVD_QTY)")})
                                   ELSE 0
                           END UNIT   , "
            End If

            If RptInvoice = "rptInvPosSlip.rpt" Then strsql &= " CR_CashRegisterSerial , "

            If ShowBundleComm = False Then
                strsql &= " COALESCE(Invh_VATPer,(SELECT TOP 1 cv_vatper FROM dbo.com_vatdate WHERE cv_comp = '" & gbComp & "' AND cv_stdate <= POS_SALEINVhead.INVH_DATE ORDER BY cv_stdate DESC)) cv_vatper , "
                strsql &= " 'N' rectype,null BundleCounter "
                If RptInvoice = "rptInvPosLaserSansirabView.rpt" Then strsql &= " ,Meas_Desc "
                strsql &= "  From dbo.POS_SALEINVhead POS_SALEINVhead"
                strsql &= " inner join POS_SALEINVDET on INVH_RECID = INVD_HEADID AND INVH_LOC=INVD_LOC"
                'strsql &= " LEFT JOIN dbo.POS_SALEINVCO ON POS_SALEINVCO.INVCO_InvLoc = POS_SALEINVDET.INVD_LOC AND POS_SALEINVCO.INVCO_InvHeadID = POS_SALEINVDET.INVD_HEADID AND POS_SALEINVCO.INVCO_InvCounter = POS_SALEINVDET.INVD_Counter"
                'strsql &= " LEFT JOIN dbo.POS_SALEINVDO ON POS_SALEINVDO.INVDO_InvLoc = POS_SALEINVDET.INVD_LOC AND POS_SALEINVDO.INVDO_InvHeadID = POS_SALEINVDET.INVD_HEADID AND POS_SALEINVDO.INVDO_InvCounter = POS_SALEINVDET.INVD_Counter"

            Else
                strsql &= " COALESCE(Invh_VATPer,(SELECT TOP 1 cv_vatper FROM dbo.com_vatdate WHERE cv_comp = '" & gbComp & "' AND cv_stdate <= V_Stk04_InvoiceComment.INVH_DATE ORDER BY cv_stdate DESC)) cv_vatper , "
                strsql &= " rectype,BundleCounter "
                If RptInvoice = "rptInvPosLaserSansirabView.rpt" Then strsql &= " ,Meas_Desc "
                strsql &= " From V_Stk04_InvoiceComment"
            End If
            strsql &= " LEFT JOIN dbo.STK04_OPParam ON INVH_LOC = OP_Loc"
            strsql &= " left JOIN " & CStr(IIf(ClocHO = False, "POS_Employee", "STK04_Employee")) & " POS_employee_2 ON [E_Code] = [INVH_UStamp]"
            If RptInvoice = "rptInvPosSlip.rpt" Then strsql &= "INNER JOIN dbo.POS_CashReg ON POS_CashReg.CR_Loc = INVH_LOC AND POS_CashReg.CR_Code = INVH_CSHREG "
            strsql &= " INNER JOIN [Stk04_Clients] ON [INVH_CLIENTSUP] = [CL_RecID]"
            strsql &= " LEFT JOIN [com_folio] ClientFolio ON [CL_FolioNo] = ClientFolio.[Fo_serno]"
            strsql &= " AND [CL_FolioSubNo] = ClientFolio.[Fo_subno]"
            strsql &= CStr(IIf(ShowBundleComm = False, " inner ", " Left ")) & " join " & CStr(IIf(ClocHO = False, "pos_stkfil", "STK04_stkfil")) & " pos_stkfil on invd_stkid = stk_recid"
            If RptInvoice = "rptInvPosLaserSansirabView.rpt" Or RptInvoice = "RptFaircoReportPrint.rpt" Or RptInvoice = "RptFaircoReportView.rpt" Then strsql &= " LEFT JOIN dbo.STK04_Measurements ON STK_UNIt = Meas_Unit "
            If Lang.Trim <> "" Then
                strsql &= " LEFT JOIN (SELECT STKL_StkId, NULLIF(STKL_ItemDesc,'')  STKL_ItemDesc FROM BIBL1_Language"
                strsql &= " INNER JOIN STK04_StkItemDescLang ON STK04_StkItemDescLang.STKL_LangRecId = BIBL1_Language.L_Recid"
                strsql &= " WHERE L_Code = '" & Lang & "'"
                strsql &= " )  STK04_StkItemDescLang ON pos_stkfil.STK_RECID = STK04_StkItemDescLang.STKL_StkId"


            End If
            strsql &= " left join bibl1_book on Bibl1_Book.BO_Recid = POS_STKFIL.STK_BOOKID"
            strsql &= " LEFT join " & CStr(IIf(ClocHO = False, "POS_Employee", "STK04_Employee")) & " POS_Employee_1 on INVD_Salesman = POS_Employee_1.E_Code"
            strsql &= " inner join dbo.STK04_Location on INVH_LOC = STK04_Location.Loc_Code"
            strsql &= " LEFT join " & CStr(IIf(ClocHO = False, "POS_Employee", "STK04_Employee")) & " POS_EMPLOYEE on INVH_EMP = POS_EMPLOYEE.E_Code"
            strsql &= " LEFT JOIN stk04_color on INVD_Color = color_code"
            'Modified by Rita M @ 04/04/2016. Get the info from the company and not the folio of the location
            'strsql &= " left join com_folio on STK04_Location.Loc_Folio = Com_Folio.Fo_serno AND"
            'strsql &= " STK04_Location.Loc_FolioSub = Com_Folio.Fo_subno"
            strsql &= " LEFT JOIN dbo.Com_Company ON dbo.Com_Company.Comp_Code = dbo.STK04_Location.Loc_CoCode"
            strsql &= " LEFT JOIN pos_stocktransaction ON stt_doctype1=INVH_DOCTYP1 AND stt_prefix=INVH_DOCPREFIX"
            strsql &= " AND stt_location=INVH_LOC"
            strsql &= " LEFT JOIN stk04_claccounts ON dbo.Stk04_Clients.CL_RecID = dbo.STk04_ClAccounts.CA_CLRecid"
            strsql &= " and ca_comp ='" & gbComp & "'"

            'strsql &= " LEFT JOIN dbo.STK04_DSTechnical ON STK_RECID=DSTech_RecId"

            'ch @ 15/10/2016 
            strsql &= " LEFT JOIN POS_SALEINVLinkCNItem ON POS_SALEINVLinkCNItem.INVCN_CNLoc =  INVD_LOC AND POS_SALEINVLinkCNItem.INVCN_CNRecId =  INVD_HEADID AND POS_SALEINVLinkCNItem.INVCN_CNCounter =  INVD_Counter"
            If PrintLinkBTWInvAndCN OrElse Not ShowBundleComm Then strsql &= " LEFT JOIN POS_SALEINVCO SALEINVCO  ON SALEINVCO.INVCO_InvLoc = INVD_LOC  AND SALEINVCO.INVCO_InvHeadID = INVD_HEADID    AND SALEINVCO.INVCO_InvCounter = INVD_Counter "
            If PrintLinkBTWInvAndCN OrElse Not ShowBundleComm Then strsql &= " LEFT JOIN POS_SALEINVdo SALEINVdo ON SALEINVdo.INVDO_InvLoc = INVD_LOC  AND SALEINVdo.INVDO_InvHeadID = INVD_HEADID  AND SALEINVdo.INVDO_InvCounter = INVD_Counter "
            strsql &= " LEFT JOIN (select Trn_Loc,Trn_InvoiceNo,Trn_CardNo,SUM(Trn_Points) Trn_Points "
            strsql &= " from tbl_Priv_Transaction WHERE Trn_Type='A' GROUP BY Trn_Loc,Trn_InvoiceNo,Trn_CardNo) "
            strsql &= " tbl_Priv_Transaction ON SUBSTRING(Trn_InvoiceNo,1,2)=INVH_DOCTYP1"
            strsql &= " AND SUBSTRING(Trn_InvoiceNo,3,1)=INVH_DOCPREFIX "
            strsql &= " AND SUBSTRING(Trn_InvoiceNo,5,LEN(Trn_InvoiceNo)-3)=INVH_SERNO AND trn_loc=INVH_LOC"
            strsql &= " LEFT JOIN dbo.tbl_Priv_Cards ON Card_No=Trn_CardNo"
            strsql &= " LEFT JOIN dbo.tbl_Priv_Clients "
            strsql &= " ON dbo.tbl_Priv_Clients.Clt_Recid = dbo.tbl_Priv_Cards.Card_Client	"
            strsql &= " AND tbl_Priv_Clients.Clt_Loc = tbl_Priv_Cards.Card_ClientLoc "

            If RptInvoice = "RptFaircoReportPrint.rpt" Or RptInvoice = "RptFaircoReportView.rpt" _
                Or RptInvoice = "rptInvPosLaserOPP.rpt" Or RptInvoice = "rptInvPosLaserOPPtoprint.rpt" Then

                strsql &= " LEFT JOIN ( "
                strsql &= " SELECT  DoconT2.INVH_LOC loc ,"
                strsql &= " DoconT2.INVH_RECID RECID ,"
                strsql &= " STUFF(( SELECT DISTINCT"
                strsql &= " ';' + CONVERT (VARCHAR, INVDO_DORecId) + ''"
                strsql &= " FROM    DoConTable DoconT1"
                strsql &= " WHERE   DoconT2.INVH_LOC = DoconT1.invh_loc"
                strsql &= " AND DoconT2.INVH_RECID = DoconT1.INVH_RECID"
                strsql &= " FOR"
                strsql &= " XML PATH('')"
                strsql &= " ), 1, 1, '') AS DoCon"
                strsql &= " FROM    DoConTable DoconT2"
                strsql &= " GROUP BY DoconT2.INVH_LOC ,"
                strsql &= " DoconT2.INVH_RECID"
                strsql &= " ) doh ON     doh.RECID = " & IIf(ShowBundleComm, "V_Stk04_InvoiceComment", "POS_SALEINVhead").ToString & ".INVH_RECID"
                strsql &= " AND doh.loc = " & IIf(ShowBundleComm, "V_Stk04_InvoiceComment", "POS_SALEINVhead").ToString & ".INVH_LOC"

            End If

            If RptInvoice = "rptInvPosLaser.rpt" Or RptInvoice = "rptInvPosLaserItemCertificate.rpt" Or RptInvoice = "rptInvPosLaserSansirab.rpt" _
                    Or RptInvoice = "rptInvPosLaserOPP.rpt" Or RptInvoice = "rptInvPosLaserOPPtoprint.rpt" _
                Or RptInvoice = "RptFaircoReportPrint.rpt" Or RptInvoice = "RptFaircoReportView.rpt" Or RptInvoice = "rptInvPosLaserMaliks.rpt" Or RptInvoice = "rptInvPosLaserDoculand.rpt" _
                    OrElse RptInvoice = "rptInvPosLaserKhayyatKanaan.rpt" OrElse RptInvoice = "rptInvPosLaserNovamart.rpt" OrElse
                    RptInvoice = "rptInvPosLaserISM.rpt" OrElse RptInvoice = "rptInvPosLaserMedix.rpt" OrElse
                    RptInvoice = "hintrag01.rpt" OrElse RptInvoice = "rptInvPosLaserMountainSteel.rpt" OrElse RptInvoice.ToUpper = "rptInvPosLaserEmileAoun.rpt".ToUpper Then

                strsql &= " LEFT JOIN ( "
                strsql &= " SELECT  SdconT2.INVH_LOC loc ,"
                strsql &= " SdconT2.INVH_RECID RECID ,"
                strsql &= " STUFF(( SELECT DISTINCT"
                strsql &= " ';' + CONVERT (VARCHAR, HOFR_RECID) + '/'"
                strsql &= " + CONVERT(VARCHAR, HOFR_SeqNo) + ''"
                strsql &= " FROM    SDConTable SdconT1"
                strsql &= " WHERE   SdconT2.INVH_LOC = SdconT1.invh_loc"
                strsql &= " AND SdconT2.INVH_RECID = SdconT1.INVH_RECID"
                strsql &= " FOR"
                strsql &= " XML PATH('')"
                strsql &= " ), 1, 1, '') AS SDCon"
                strsql &= " FROM    SDConTable SdconT2"
                strsql &= " GROUP BY SdconT2.INVH_LOC ,"
                strsql &= " SdconT2.INVH_RECID"
                strsql &= " ) soh ON    soh.RECID = " & IIf(ShowBundleComm, "V_Stk04_InvoiceComment", "POS_SALEINVhead").ToString & ".INVH_RECID"
                strsql &= " AND soh.LOC = " & IIf(ShowBundleComm, "V_Stk04_InvoiceComment", "POS_SALEINVhead").ToString & ".INVH_LOC"

            End If

            Select Case _me.Name
                Case frmReprintSlips.Name, frmRevSalesInv.Name
                    strsql &= " Inner Join " & ReprintSlipTempTable & " on InvHead = INVH_RECID"
            End Select

            '    If RptInvoice = "rptInvPosLaserHintrag.rpt" Or RptInvoice = "rptInvPosLaserOPP.rpt" Or RptInvoice = "rptInvPosLaserOPPtoprint.rpt" Then
            If PrintClBalance = True Then
                If RptInvoice = "rptInvPosLaserOPP.rpt" Then
                    strsql &= "   LEFT JOIN (SELECT Distinct BalanceLC,BalanceCC,Client  FROM " & ClinetBalnceSumtable & ") ClinetBalnceSumtable ON CL_RecID = ClinetBalnceSumtable.Client "
                Else
                    If Not (InvalidBalanceMsg = "Invalid Data") Then

                        strsql &= " LEFT join  ( SELECT   Trn_accno1,Trn_accno2,Trn_subno,Trn_curr ,  CONVERT(VARCHAR, ( MAX(accTbl.Trn_date) ), 103) MaxTransDate , "
                        strsql &= " SUM(accTbl.Trn_amtlc * ( CASE WHEN accTbl.Trn_DC = 'D' THEN 1 ELSE -1 END )) Trn_amtlc,"
                        strsql &= " SUM(accTbl.Trn_amtcc * ( CASE WHEN accTbl.Trn_DC = 'D' THEN 1 ELSE -1 END )) Trn_amtcc,"
                        strsql &= " SUM(accTbl.Trn_amtfc * ( CASE WHEN accTbl.Trn_DC = 'D' THEN 1 ELSE -1 END )) Trn_amtfc"
                        strsql &= "  FROM "

                        If Not lcServerLink = gbServer Then
                            strsql &= " [" & lcServerLink & "]."
                        End If
                        strsql &= lcDBnameLink & ".dbo.Acc01_AccTransaction accTbl "

                        strsql &= " WHERE    accTbl.trn_comp = '" & gbComp & "' AND accTbl.Trn_Year = '" & ACYear & "'"
                        strsql &= " GROUP BY Trn_accno1,Trn_accno2,Trn_subno,Trn_curr"
                        strsql &= " ) AS Currentbalance "
                        strsql &= " ON  Trn_accno1 COLLATE SQL_Latin1_General_CP1256_CI_AS = SUBSTRING(ca_clac, 1, 6) COLLATE SQL_Latin1_General_CP1256_CI_AS"
                        strsql &= " AND Trn_accno2 COLLATE SQL_Latin1_General_CP1256_CI_AS = SUBSTRING(ca_clac, 7, 4) COLLATE SQL_Latin1_General_CP1256_CI_AS"
                        strsql &= " AND Trn_subno COLLATE SQL_Latin1_General_CP1256_CI_AS = SUBSTRING(ca_clac,11, 1) COLLATE SQL_Latin1_General_CP1256_CI_AS"
                        strsql &= " AND Trn_curr COLLATE SQL_Latin1_General_CP1256_CI_AS = INVH_CURR" & CStr(IIf(Copercurr = "FC", "FC", "")) & " COLLATE SQL_Latin1_General_CP1256_CI_AS"

                        strsql &= " LEFT JOIN ( SELECT  Du_AccNo ,"
                        If Not (_me.Name = FrmSalesInvoice.Name OrElse _me.Name = FrmModRemInvoice.Name OrElse _me.Name = "FrmPurOfUsedBook" OrElse _me.Name = frmClientStatement.Name) Then
                            strsql &= " bdClRecID,"
                        End If
                        strsql &= " SUM(dueAmtLC) BALdueAmtLC ,"
                        strsql &= " SUM(dueAmtCC) BALdueAmtCC ,"
                        strsql &= " SUM(dueAmtFC) BALdueAmtFC"
                        strsql &= " FROM    ( 												"
                        strsql &= " SELECT"

                        If Not (_me.Name = FrmSalesInvoice.Name OrElse _me.Name = FrmModRemInvoice.Name OrElse _me.Name = "FrmPurOfUsedBook" OrElse _me.Name = frmClientStatement.Name) Then
                            strsql &= " ClientRecID  [bdClRecID],"
                        End If
                        strsql &= " Du_trnno ,Du_AccNo ,"
                        strsql &= " ISNULL(Du_AmtLC, 0) - ISNULL(sumDs_AmtLC, 0) dueAmtLC ,"
                        strsql &= " ISNULL(Du_AmtCC, 0) - ISNULL(sumDs_AmtCC, 0) dueAmtCC ,"
                        strsql &= " ISNULL(Du_AmtFC, 0) - ISNULL(sumDs_AmtFC, 0) dueAmtFC "
                        strsql &= " FROM   "

                        If Not lcServerLink = gbServer Then
                            strsql &= " [" & lcServerLink & "]."
                        End If
                        strsql &= lcDBnameLink & ".DBO.ACC01_Due"

                        If Not (_me.Name = FrmSalesInvoice.Name OrElse _me.Name = FrmModRemInvoice.Name OrElse _me.Name = "FrmPurOfUsedBook" OrElse _me.Name = frmClientStatement.Name) Then
                            strsql &= " INNER JOIN (SELECT DISTINCT SUBSTRING(ca_clac, 1, 11) + "
                            strsql &= " INVH_OriginalCURR clac,  ClientRecID "
                            strsql &= " FROM " & ReprintSlipTempTable & " "
                            strsql &= " INNER JOIN dbo.STk04_ClAccounts ON ClientRecID = dbo.STk04_ClAccounts.CA_CLRecid"
                            strsql &= " AND ca_comp = '" & gbComp & "' WHERE chk='True') ClAccounts ON Du_AccNo=clac"
                        End If

                        strsql &= " LEFT JOIN ( "
                        strsql &= " SELECT"
                        strsql &= " Ds_Dutrnno ,Ds_Duseqno,"
                        strsql &= " ISNULL(SUM(Ds_AmtLC),0) sumDs_AmtLC ,"
                        strsql &= " ISNULL(SUM(Ds_AmtCC),0) sumDs_AmtCC ,"
                        strsql &= " ISNULL(SUM(Ds_AmtFC),0) sumDs_AmtFC"
                        strsql &= " FROM   "

                        If Not lcServerLink = gbServer Then
                            strsql &= " [" & lcServerLink & "]."
                        End If
                        strsql &= lcDBnameLink & ".DBO.ACC01_Due"

                        If Not (_me.Name = FrmSalesInvoice.Name OrElse _me.Name = FrmModRemInvoice.Name OrElse _me.Name = "FrmPurOfUsedBook" OrElse _me.Name = frmClientStatement.Name) Then
                            strsql &= " INNER JOIN (SELECT DISTINCT SUBSTRING(ca_clac, 1, 11) + INVH_OriginalCURR clac"
                            strsql &= " FROM " & ReprintSlipTempTable & " "
                            strsql &= " INNER JOIN dbo.STk04_ClAccounts ON ClientRecID = dbo.STk04_ClAccounts.CA_CLRecid"
                            strsql &= " AND ca_comp = '" & gbComp & "' WHERE chk='True') ClAccounts ON Du_AccNo=clac"
                        End If

                        strsql &= " INNER join "
                        If Not lcServerLink = gbServer Then
                            strsql &= " [" & lcServerLink & "]."
                        End If
                        strsql &= lcDBnameLink & ".DBO.ACC01_LinkDueSett"
                        strsql &= " ON ACC01_LinkDueSett.Ds_Dutrnno = ACC01_Due.Du_trnno "

                        strsql &= " AND ACC01_LinkDueSett.Ds_Duseqno = ACC01_Due.Du_seqno"
                        strsql &= " WHERE  ISNULL(Du_CostC,'')=''"
                        strsql &= " GROUP BY Ds_Dutrnno, Ds_Duseqno"
                        strsql &= " ) T1 ON Du_trnno = T1.Ds_Dutrnno AND Du_seqno=t1.Ds_Duseqno"
                        strsql &= " WHERE     Du_Comp = '" & gbComp & "'  AND ISNULL(Du_CostC,'')='' AND du_duedate<=CONVERT(date,GETDATE()) "

                        If _me.Name = FrmSalesInvoice.Name OrElse _me.Name = FrmModRemInvoice.Name OrElse _me.Name = "FrmPurOfUsedBook" OrElse _me.Name = frmClientStatement.Name Then
                            strsql &= " AND ACC01_Due.Du_AccNo = '" & CurrentInvoiceClAcc & "' "
                        End If

                        strsql &= "   ) tbl"
                        strsql &= " GROUP BY Du_AccNo "

                        If Not (_me.Name = FrmSalesInvoice.Name OrElse _me.Name = FrmModRemInvoice.Name OrElse _me.Name = "FrmPurOfUsedBook" OrElse _me.Name = frmClientStatement.Name) Then
                            strsql &= "  ,bdClRecID "
                        End If

                        strsql &= " ) "
                        strsql &= " AS ClBalanceDue ON "

                        If Not (_me.Name = FrmSalesInvoice.Name OrElse _me.Name = FrmModRemInvoice.Name OrElse _me.Name = "FrmPurOfUsedBook" OrElse _me.Name = frmClientStatement.Name) Then
                            strsql &= " clbalancedue.bdClRecID = ClientRecID"
                        Else
                            strsql &= " clbalancedue.du_accno = '" & CurrentInvoiceClAcc & "' "
                        End If

                    End If
                End If

            End If

            'Rita M @ 19.05.2021. Payment mode is added only to hintrag report. If exist at lease one payment mode that is not On A/C or additional discount or dead balance, then the invoice
            'is considered as cash
            If RptInvoice.Trim.ToUpper = "RPTINVPOSLASERHINTRAG.RPT" Then
                strsql &= " LEFT JOIN (SELECT CT_LOCATION,CT_INVHEADER ,COUNT(*) Cash "
                strsql &= " 	FROM dbo.POS_CASHTRANSACTION "
                strsql &= " 	WHERE CT_LOCATION = '" & Cloc & "' AND CT_SETTLTYPE NOT IN ('318','350','500','APY','RAC')	"
                strsql &= " 	GROUP BY CT_LOCATION,CT_INVHEADER				 "
                strsql &= " 	) POS_CASHTRANSACTION ON INVH_LOC = CT_LOCATION AND INVH_RECID = CT_INVHEADER "
            End If

            If RptInvoice = "rptInvPosLaserMedix.rpt" Then
                strsql &= " LEFT JOIN STK04_StockItemServiceFees ON SISF_InitStkRecID = STK_RECID "
            End If

            'If RptInvoice = "rptInvPosLaserISM.rpt" Then
            '    strsql &= " LEFT JOIN ( SELECT  VEH_Vin ,
            '                                    VLS_INVHRecID
            '                            FROM    STK04_VehiclesLinkToSales
            '                                    INNER JOIN ( SELECT VEH_RecID ,
            '                                                        VEH_Vin
            '                                                 FROM   dbo.STK04_VehiclesDefinition
            '                                               ) STK04_VehiclesDefinition ON VEH_RecID = VLS_VEHRecID
            '                            WHERE   VLS_INVHLoc = '" & Cloc & "'
            '                          ) STK04_VehiclesLinkToSales ON VLS_INVHRecID = INVH_RECID "
            'End If

            'Conditions = current location, serial number
            Select Case _me.Name
                Case FrmSalesInvoice.Name, FrmModRemInvoice.Name, "FrmPurOfUsedBook", frmClientStatement.Name
                    strsql &= " where INVH_LOC = '" & Cloc & "'"
                    strsql &= " and INVH_RECID = '" & WSerno & "'"
                    'Join with TempTable sent from form, conditions = current location, checked records
                Case frmReprintSlips.Name, frmRevSalesInv.Name
                    'strsql &= " Inner Join " & ReprintSlipTempTable & " on InvHead = INVH_RECID"
                    strsql &= " where INVH_LOC = '" & Cloc & "'"
                    strsql &= " And [Chk] = 'True'"
            End Select
            strsql &= " AND isnull(Card_Active,'Y')='Y' "
            'CH @ 08/02/2017 WF :4983
            If PrintExpLot = False Then
                strsql &= "  GROUP BY INVH_LOC ,INVH_RATE  ,INVH_RECID , INVH_USTAMP , "
                strsql &= "    INVH_DATE, INVH_EMP,"
                strsql &= " INVH_CSHREG, fo_Email, ClientFolio.fo_mof, INVH_CLNAME,"
                strsql &= " ClientFolio.fo_MOF, ClientFolio.fo_phone1, ClientFolio.fo_fax,"
                strsql &= " ca_clac, CL_Code, CL_No, CL_Name, cl_invdays, CL_PriceCat,"
                strsql &= " INVH_CURR, INVH_DOCTYP1, INVH_DOCPREFIX, INVH_Serno, Loc_Vat,"
                strsql &= " INVH_CLADDTEL, INVH_DSTAMP, POS_Employee_2.e_name, INVH_VATTYPE,"
                strsql &= " INVH_RKS, STK_NEWUSED, " & IIf(ChangeDesc, "", " STK_SERVICE, ").ToString & " STK_DESC,"
                strsql &= " STK_VATPER, INVH_GROSS, INVH_DISC, INVH_ADDDISC, INVH_VATFC,"
                If RptInvoice.Trim.ToUpper = "RPTINVPOSLASERHINTRAG.RPT" Then
                    strsql &= " INVH_AddDiscPerc , Cash,"
                End If
                strsql &= " INVH_NET, INVH_VATLC, INVD_STATUS, INVD_ITMSTATUS,"
                strsql &= " CAST(INVD_ITMDESC AS NVARCHAR(max)), INVD_GIFT,"
                strsql &= " POS_EMPLOYEE.E_Name, POS_Employee_1.E_Name, loc_desc,"
                strsql &= " comp_addr, Comp_Phone1,  stk_ref, invh_rate,invh_ratefc,INVH_USTAMP, "
                strsql &= " INVH_AddDiscCaption ,cl_AcExec ,Invh_VATPer,"

                If RptInvoice.ToUpper = "rptInvPosLaserEmileAoun.rpt".ToUpper Then strsql &= " Comp_Phone2 , "

                If RptInvoice.ToUpper = "RPTINVPOSLASERHINTRAG.RPT" Then strsql &= " STK_UNITPACK , STK_OPERATOR, "

                strsql &= " INVH_DOCPRINTEDP , "
                'ch @ 01/08/2019 mail(PetMart_Sales invoice/Purchase invoice: Hide company information)
                If RptInvoice.ToUpper = "RPTINVPOSLASERPETMART.RPT" OrElse RptInvoice.ToUpper = "RPTINVPOSLASERPETMARTWORate.RPT" OrElse
                    RptInvoice.ToUpper = "RPTINVPOSLASERALLINTRADE.RPT" OrElse RptInvoice.ToUpper = "RPTINVPOSLASERALLINTRADEWORate.RPT" Then
                    strsql &= "   STT_ACCTRANS  , "
                End If
                If ItmCetificate = False Then
                    strsql &= " INVD_Salesman ,INVD_PROCARD, INVD_UPRICE ,INVD_DISC , "
                End If

                If Lang.Trim <> "" Then
                    strsql &= " STK04_StkItemDescLang.STKL_ItemDesc ,"
                End If
                If Copercurr = "FC" OrElse Not (curr = gbCurr2) Then
                    strsql &= " INVH_GROSSFC, INVH_ADDDISCFC , INVH_NETFC,  invh_discfc , "
                    If ItmCetificate = False Then
                        strsql &= " INVD_UPRICEFC,  "
                    End If
                End If
                strsql &= " Invh_VATFCNEW , INVH_CURRFC ,"
                'CH @ 27/02/2017
                If rbSingleRef2 = True Then
                    strsql &= " STK_SingleRef2, "
                Else
                    strsql &= " STK_SingleRef1, "
                End If
                If RptInvoice = "RPTINVPOSLASERHINTRAG.RPT" Then
                    strsql &= " STK_SingleRef2 ,"
                End If
                strsql &= " STK_VATTYPE, INVD_DetGrid, invd_color, Color_Reference, bO_ISBN,"
                strsql &= "  Invh_Head1, stt_printed1, Invh_Head2 ,stt_printed2, Invh_Footer ,stt_footer, STK_FourthParty,"
                strsql &= " Trn_CardNo, Clt_FirstName, Clt_FamilyName, Card_Balance, trn_points,"
                If RptInvoice = "RptFaircoReportPrint.rpt" Or RptInvoice = "RptFaircoReportView.rpt" Then
                    strsql &= " meas_desc, fo_vatreg, INVCN_InvLoc, INVCN_InvHeadID"
                Else
                    strsql &= " STK_UNIt, fo_vatreg, INVCN_InvLoc, INVCN_InvHeadID"
                End If
                If PrintLinkBTWInvAndCN Then
                    strsql &= " , INVCO_OfferRecId,INVCO_OfferLoc,INVDO_DORecId,INVDO_DOLoc  "
                End If
                If RptInvoice = "rptInvPosLaserOPP.rpt" Or RptInvoice = "rptInvPosLaserOPPtoprint.rpt" Then strsql &= " ,STK_SUBUNIT,STK_UNITPACK,STK_OPERATOR "

                If ShowBundleComm = True Then strsql &= " , recType, BundleCounter "

                If RptInvoice = "RptFaircoReportPrint.rpt" Or RptInvoice = "RptFaircoReportView.rpt" _
                       Or RptInvoice = "rptInvPosLaserOPP.rpt" Or RptInvoice = "rptInvPosLaserOPPtoprint.rpt" Then
                    strsql &= " ,docon "
                End If

                If RptInvoice = "rptInvPosLaser.rpt" Or RptInvoice = "rptInvPosLaserItemCertificate.rpt" Or RptInvoice = "rptInvPosLaserSansirab.rpt" _
                         Or RptInvoice = "rptInvPosLaserOPP.rpt" Or RptInvoice = "rptInvPosLaserOPPtoprint.rpt" _
                         Or RptInvoice = "RptFaircoReportPrint.rpt" Or RptInvoice = "RptFaircoReportView.rpt" Or RptInvoice = "rptInvPosLaserMaliks.rpt" Or RptInvoice = "rptInvPosLaserDoculand.rpt" _
                         OrElse RptInvoice = "rptInvPosLaserKhayyatKanaan.rpt" OrElse RptInvoice = "rptInvPosLaserNovamart.rpt" OrElse
                         RptInvoice = "rptInvPosLaserISM.rpt" OrElse RptInvoice = "rptInvPosLaserMedix.rpt" OrElse
                         RptInvoice = "hintrag01.rpt" OrElse RptInvoice = "rptInvPosLaserMountainSteel.rpt" OrElse RptInvoice.ToUpper = "rptInvPosLaserEmileAoun.rpt".ToUpper Then
                    strsql &= " ,sdcon "
                End If

                '  If (RptInvoice = "rptInvPosLaserHintrag.rpt" Or RptInvoice = "rptInvPosLaserOPP.rpt" Or RptInvoice = "rptInvPosLaserOPPtoprint.rpt") _
                '      AndAlso Not (InvalidBalanceMsg = "Invalid Data") Then
                If PrintClBalance = True AndAlso Not (InvalidBalanceMsg = "Invalid Data") AndAlso RptInvoice <> "rptInvPosLaserOPP.rpt" Then
                    strsql &= " , "
                    If Not (curr = "" Or curr = "FC") Then strsql &= " CASE WHEN invh_curr" & CStr(IIf(Copercurr = "FC", "FC", "")) & " = '" & curr & "' THEN "
                    strsql &= " CASE WHEN INVH_CURR = '" & gbCurr1 & "' THEN Currentbalance.Trn_amtlc"
                    strsql &= " WHEN INVH_CURR = '" & gbCurr2 & "' THEN Currentbalance.Trn_amtcc"
                    strsql &= " ELSE Currentbalance.Trn_amtfc END"
                    If Not (curr = "" Or curr = "FC") Then strsql &= " else 0 end "

                    strsql &= " ,Currentbalance.maxtransDate, "

                    If Not (curr = "" Or curr = "FC") Then strsql &= " CASE WHEN invh_curr" & CStr(IIf(Copercurr = "FC", "FC", "")) & " = '" & curr & "' THEN "
                    strsql &= " CASE WHEN INVH_CURR = '" & gbCurr1 & "' THEN   ClBalanceDue.BALdueAmtLC "
                    strsql &= " WHEN INVH_CURR = '" & gbCurr2 & "' THEN   ClBalanceDue.BALdueAmtCC "
                    strsql &= " ELSE   ClBalanceDue.BALdueAmtFC END"
                    If Not (curr = "" Or curr = "FC") Then strsql &= " else 0 end "

                End If

                If PrintClBalance = True AndAlso RptInvoice = "RPTINVPOSLASEROPP.RPT" Then
                    strsql &= " , "
                    If gbCurr1 = IIf(curr = "", Copercurr, curr).ToString Then
                        strsql &= "  ClinetBalnceSumtable.BalancelC "
                    ElseIf IIf(curr = "", Copercurr, curr).ToString = gbCurr2 Then
                        strsql &= "  ClinetBalnceSumtable.BalanceCC "
                    Else
                        strsql &= " ClinetBalnceSumtable.BalanceCC"
                    End If
                End If
                If RptInvoice = "rptInvPosLaserSansirabView.rpt" Then strsql &= " ,Meas_Desc "

                strsql &= " , INVH_VATLCRate,OP_OPCurr"

                If RptInvoice = "rptInvPosLaserMedix.rpt" Then strsql &= " , SISF_RealDiscountPerc, STK_ReturnSale "
                'If RptInvoice = "rptInvPosLaserISM.rpt" Then strsql &= " , VEH_Vin "

                If RptInvoice = "rptInvPosSlip.rpt" Then strsql &= ", CR_CashRegisterSerial  "
            End If
            'strsql &= " order by POS_SALEINVDET.INVD_BARCODE Asc"
            'christina   @ 01/04/2016
            ' strsql &= "       ORDER BY BundleCounter ,case when (RecType='B' OR ISNULL(BundleCounter,0)=0 ) then 0 else 1 end,INVD_Counter "
            Dim NbrCopies As String
            'added by GS on 16/03/16
            strsql1 = " SELECT top 1 STT_NBRCOPY FROM pos_stocktransaction WHERE STT_LOCATION='" & Cloc & "' "
            If sOper = "PU" OrElse sOper = "PX" Then
                strsql1 &= " AND STT_DOCTYPE1='" & sOper & "' AND STT_Active='Y'"
            Else
                strsql1 &= " AND STT_DOCTYPE1='" & CStr(IIf(sOper = "S", "IN", "CN")) & "' AND STT_Active='Y'"
            End If
            strsql1 &= " AND STT_PREFIX = '" & _Prefix & "' "
            NbrCopies = ExecuteScalar(strsql1)
            'GS on 16/03/16
            Dim rptParam(29, 1) As Object


            rptParam(1, 0) = "Time"
            rptParam(1, 1) = " '" & txttime & "'"

            'Jad JE Req By Rita
            rptParam(2, 0) = "ProundLC"
            rptParam(2, 1) = " " & VatLCRound & ""

            rptParam(4, 0) = "MOFReg"
            rptParam(4, 1) = " '" & MofReg & "'"

            rptParam(5, 0) = "VATReg"
            rptParam(5, 1) = " '" & sVATreg & "'"

            If RptInvoice = "rptInvPosLaserKhayyatKanaan.rpt" Then
                rptParam(23, 0) = "COMReg"
                rptParam(23, 1) = " '" & COMReg & "'"
            End If

            rptParam(6, 0) = "LocVatRetT"
            rptParam(6, 1) = " '" & LocVatRetT & "'"

            rptParam(7, 0) = "DVAT"
            rptParam(7, 1) = " '" & DVAT & "'"

            rptParam(8, 0) = "vVatper"
            rptParam(8, 1) = " '" & VATper & "'"

            rptParam(9, 0) = "LocVatWhT"
            rptParam(9, 1) = " '" & LocVatWhT & "'"

            rptParam(9, 0) = "LocVatWhT"
            rptParam(9, 1) = " '" & LocVatWhT & "'"

            rptParam(10, 0) = "Curr1"
            rptParam(10, 1) = " '" & gbCurr1 & "'"

            rptParam(11, 0) = "Curr2"
            rptParam(11, 1) = " '" & gbCurr2 & "'"

            rptParam(12, 0) = "DecNo"
            rptParam(12, 1) = " " & gbDecNo & ""

            rptParam(13, 0) = "Curr"
            rptParam(13, 1) = IIf(curr = "", " '" & Copercurr & "'", " '" & curr & "'").ToString 'edited by GS on 10/05/16 ' " '" & Copercurr & "'"

            rptParam(14, 0) = "ClNameFromClientFile"
            If ClNameFromClientFile = True Then
                rptParam(14, 1) = "'TRUE'"
            Else
                rptParam(14, 1) = "'False'"
            End If

            rptParam(15, 0) = "PrintTechContent"
            rptParam(15, 1) = " '" & PrintTechContent & "'"

            rptParam(16, 0) = "gbFashion"
            rptParam(16, 1) = " '" & gbFashion & "'"

            rptParam(17, 0) = "PrintLinkBtwInvAndCN"
            rptParam(17, 1) = " '" & PrintLinkBTWInvAndCN & "'"

            rptParam(18, 0) = "PrintExpLot"
            'rptParam(18, 1) = " '" & PrintExpLot & "'"
            rptParam(18, 1) = IIf(gbExpiry, "'" & PrintExpLot & "'", "'False'").ToString

            rptParam(19, 0) = "ItemReference"
            rptParam(19, 1) = " '" & ItemReference & "'"

            rptParam(20, 0) = "InvalidBalanceMsg"
            If PrintClBalance = False Then
                rptParam(20, 1) = " 'Suppres'"
            Else
                rptParam(20, 1) = " '" & InvalidBalanceMsg & "'"
            End If


            If _ordering = "Def" Then
                Dim allowOrderByRef As Boolean = True, allowOrderByDesc As Boolean = True
                Dim strsql2 As String

                strsql2 = " SELECT TOP 1 1"
                strsql2 &= " FROM    POS_SaleInvBundle"
                Select Case _me.Name
                    Case FrmSalesInvoice.Name, FrmModRemInvoice.Name, frmClientStatement.Name
                        strsql2 &= " where INVB_Loc ='" & Cloc & "' and INVB_HeadId = '" & WSerno & "'"
                    Case frmReprintSlips.Name, frmRevSalesInv.Name
                        strsql2 &= " Left Join " & ReprintSlipTempTable & " on InvHead = INVB_HeadId"
                        strsql2 &= " where INVB_Loc = '" & Cloc & "'"
                        strsql2 &= " And [Chk] = 'True' "
                End Select

                If RecordsExists(strsql2) Then
                    allowOrderByRef = False : allowOrderByDesc = False
                End If

                If RptInvoice = "rptInvPosLaserSansirabView.rpt" Then
                    allowOrderByRef = False
                End If

                If DefaultInvPrintOrder = "R" Then
                    If allowOrderByRef Then _ordering = "REF" Else _ordering = ""
                ElseIf DefaultInvPrintOrder = "D" Then
                    If allowOrderByDesc Then _ordering = "DESC" Else _ordering = ""
                Else
                    _ordering = ""
                End If
            End If

            rptParam(21, 0) = "FoRepOrder"
            rptParam(21, 1) = " '" & _ordering & "'"

            rptParam(22, 0) = "DoneByCaption"
            rptParam(22, 1) = " '" & CStr(IIf(SwitchDoneByAndAE, "A\E", "Done By")) & "'"

            If Slip AndAlso Not ItmCetificate Then

                rptParam(24, 0) = "QRText"
                rptParam(25, 0) = "QRImagePath"

                HosDLL.Others.ReportsQR(ConnectionString, Cloc, rptParam(24, 1), rptParam(25, 1))

            End If

            If RptInvoice = "rptInvPosSlip.rpt" Then rptParam(26, 0) = "LcBasedOnCC" : rptParam(26, 1) = " '" & CStr(IIf(CInvPriceLCBasedOnCC, "Y", "N")) & "'"
            'If VatStDate <> Nothing Then
            '    rptParam(21, 0) = "VatPerValue" : rptParam(21, 1) = "'" & IIf(CDbl(VatPerValue) = CInt(VatPerValue), CInt(VatPerValue), VatPerValue).ToString & "'"
            '    rptParam(22, 0) = "VATStDate" : rptParam(22, 1) = "'" & VatStDate & " 00:00:00'"
            'Else
            '    rptParam(21, 0) = "VatPerValue" : rptParam(21, 1) = "'" & VATper & "'"
            'End If

            If RptInvoice = "rptInvPosLaserISM.rpt" Then rptParam(27, 0) = "IsOffDb" : rptParam(27, 1) = " '" & CStr(IIf(gbIsOffDb, "Y", "N")) & "'"

            If RptInvoice = "rptInvPosLaserMountainSteel.rpt" Then rptParam(28, 0) = "PrintCompanyLogo" : rptParam(28, 1) = " '" & CStr(IIf(PrintCompanyLogo, "Y", "N")) & "'"

            If RptInvoice.ToUpper = "rptInvPosLaserEmileAoun.rpt".ToUpper Then rptParam(29, 0) = "CompAddr" : rptParam(29, 1) = " '" & GetFieldDesc("Com_Company", "Comp_Code = '" & gbComp & "'", "Replace(Replace(Comp_Addr,CHAR(10),''),CHAR(13),' ')") & "'"


            If RptInvoice.ToUpper.Contains("rptInvPosLaserApp".ToUpper) Then

                maxAttLengh = CStr(Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData) & "\TEMP\MailFiles\10").Length

                DTMails = New DataTable
                With DTMails
                    .Columns.Add("Check", GetType(System.Boolean)).DefaultValue = True
                    .Columns.Add("A/C", GetType(System.String))
                    .Columns.Add("Name", GetType(System.String))
                    .Columns.Add("Mail", GetType(System.String))
                    .Columns.Add("Filter", GetType(System.String))
                    .Columns.Add("Status", GetType(System.String))
                    .Columns.Add("Locked", GetType(System.String)).DefaultValue = "N"
                    .Columns.Add("Records", GetType(System.Int64)).AutoIncrement = True
                    .Columns.Add("PdfFilterToLinkDTMails", GetType(System.String)) ' Specific Filtration for The Link Between DTMails and DtpDf 
                    .Columns("Records").AutoIncrementSeed = 1
                    .PrimaryKey = New DataColumn() { .Columns("Records")}
                End With
                DtpDf = New DataTable
                With DtpDf
                    .Columns.Add("Name", GetType(System.String)) 'Client name  
                    .Columns.Add("RecID", GetType(System.String)) 'offer  head recid 
                    .Columns.Add("PdfFilter", GetType(System.String)) ' Spesific Filtration For The Pdf File 
                    .Columns.Add("AttachName", GetType(System.String)) ' Pdf File name 
                    .PrimaryKey = New DataColumn() { .Columns("RecID")}
                End With
                'Case of Crystal Mail xx ------
                DTMails.Clear()
                strs = "  SELECT * ,
                                 ROW_NUMBER() OVER ( ORDER BY Mail ) [Records]
                          FROM   ( SELECT DISTINCT
                                             'True' [Check] ,
                                             '' [A/C] ,
                                             INVH_CLNAME Name ,
                                             CONCAT(ISNULL(Fo_Email, ''),
                                                    CASE WHEN ISNULL(Fo_Email, '') = '' THEN ''
                                                         ELSE ' ; '
                                                    END, CASE WHEN cci_email = Fo_Email THEN ''
                                                              ELSE ISNULL(cci_email, '')
                                                         END) Mail ,
                                             '' Status
                                   FROM      dbo.POS_SALEINVHEAD 
                                   LEFT JOIN ( SELECT  Loc_Code ,
                                                          cci_email ,
                                                          cci_RecID
                                                  FROM    dbo.STK04_Location
                                                          LEFT JOIN ( SELECT  cci_LocCode ,
                                                                              cci_email ,
                                                                              cci_RecID
                                                                      FROM    STK04_CashCLIndex
                                                                      WHERE   cci_RecID = 1
                                                                              AND cci_email IS NOT NULL
                                                                    ) STK04_CashCLIndex ON cci_LocCode = Loc_Code
                                                  WHERE   Loc_CoCode = '" & gbComp & "'
                                                ) STK04_Location ON Loc_Code = INVH_LOC
                                      INNER JOIN dbo.Stk04_Clients ON CL_RecID = INVH_CLIENTSUP
                                      LEFT JOIN dbo.STk04_ClAccounts ON CA_CLRecid = CL_RecID
                                                                        AND CA_Comp = '" & gbComp & "'
                                      LEFT JOIN Com_Folio ON CL_FolioNo = Fo_serno
                                                             AND CL_FolioSubNo = Fo_subno "
                Select Case _me.Name
                    Case frmReprintSlips.Name, frmRevSalesInv.Name
                        strs &= "INNER JOIN " & ReprintSlipTempTable & " ON INVH_RECID = InvHead
                                 WHERE chk='True' "
                    Case FrmSalesInvoice.Name, FrmModRemInvoice.Name, "FrmPurOfUsedBook", frmClientStatement.Name
                        strs &= " where INVH_LOC ='" & Cloc & "' and INVH_RECID = '" & WSerno & "' "
                End Select
                strs &= " GROUP BY  Fo_Email ,
                                      cci_email ,
                                      INVH_CLNAME
                          ) T"

                ReadFromDBToDataReader(strs, DRS0)
                If DRS0.HasRows Then
                    While DRS0.Read
                        dr0 = DTMails.NewRow
                        dr0("Check") = True
                        dr0("Name") = DRS0.Item("Name")
                        dr0("Mail") = DRS0.Item("Mail")
                        dr0("Filter") = "INVH_CLNAME= '" & Replace(DRS0("Name").ToString, "'", "''") & "'"
                        dr0("Locked") = "Y"
                        dr0("PdfFilterToLinkDTMails") = "name= '" & Replace(DRS0.Item("Name").ToString, "'", "''") & "'"
                        dr0("Records") = DRS0("Records").ToString
                        DTMails.Rows.Add(dr0)
                    End While
                End If
                DRS0.Close()
                DtpDf.Clear()
                strs = " SELECT INVH_DOCTYP1 ,
                                INVH_DOCPREFIX ,
                                INVH_SERNO ,
                                INVH_RECID RecID ,
                                INVH_CLNAME Name
                         FROM   dbo.POS_SALEINVHEAD "
                Select Case _me.Name
                    Case frmReprintSlips.Name, frmRevSalesInv.Name
                        strs &= "INNER JOIN " & ReprintSlipTempTable & " ON INVH_RECID = InvHead
                                WHERE chk='True' and INVH_LOC ='" & Cloc & "' "
                    Case FrmSalesInvoice.Name, FrmModRemInvoice.Name, "FrmPurOfUsedBook", frmClientStatement.Name
                        strs &= " where INVH_LOC ='" & Cloc & "' and INVH_RECID = '" & WSerno & "' "
                End Select
                strs &= "       GROUP BY INVH_DOCTYP1 ,
                                INVH_DOCPREFIX ,
                                INVH_SERNO ,
                                INVH_RECID ,
                                INVH_CLNAME
                         ORDER BY INVH_RECID ,
                                INVH_CLNAME "

                ReadFromDBToDataReader(strs, DRS0)
                If DRS0.HasRows Then
                    While DRS0.Read
                        dr0 = DtpDf.NewRow
                        dr0("Name") = DRS0.Item("Name")
                        dr0("RecID") = DRS0.Item("RecID")
                        dr0("PdfFilter") = "({Command.INVH_RECID}=" & DRS0("Recid").ToString & ")"
                        dr0("AttachName") = DRS0.Item("INVH_DOCTYP1").ToString & " " & DRS0.Item("INVH_DOCPREFIX").ToString & " " & DRS0.Item("INVH_SERNO").ToString & " - " & DRS0.Item("Name").ToString
                        DtpDf.Rows.Add(dr0)
                    End While
                End If
                DRS0.Close()

            End If


            'Print Bon D'achat if exist*********************************************
            Dim OtherRepRptParam(4, 1) As Object
            'If Slip = True Then
            strsqlBonAchat = "SELECT  BH_NBR,BH_TYPE,BH_CASHREG,BH_ISDATE,BH_VALDATE,BH_VALUE,BH_CURR,BH_INVNBR,"
            strsqlBonAchat &= " BH_REMARKS, CR_Desc,loc_desc,Loc_Vat,Fo_addres,Fo_phone1 FROM " & StempCsh
            strsqlBonAchat &= " INNER JOIN POS_BONACHAT ON CT_INVNBR = BH_INVNBR AND ct_location=BH_LOC"
            strsqlBonAchat &= " LEFT JOIN dbo.POS_CashReg ON BH_CASHREG = CR_Code AND ct_location=CR_Loc AND CR_Loc='" & Cloc & "'"
            strsqlBonAchat &= " LEFT JOIN dbo.STK04_Location ON Loc_Code=CR_Loc"
            strsqlBonAchat &= " left join com_folio on STK04_Location.Loc_Folio = Com_Folio.Fo_serno AND"
            strsqlBonAchat &= " STK04_Location.Loc_FolioSub = Com_Folio.Fo_subno"
            strsqlBonAchat &= " WHERE BH_TYPE = 'B' AND BH_TOFR = 'T'"
            If RecordsExists(strsqlBonAchat) = True Then
                PrintBonAchat = True
                OtherRepRptParam(1, 0) = "copyBonAchat"
                OtherRepRptParam(1, 1) = " '" & copyBonAchat & "'"

                OtherRepRptParam(2, 0) = "VATReg"
                OtherRepRptParam(2, 1) = " '" & sVATreg & "'"

                OtherRepRptParam(3, 0) = "ProundLC"
                OtherRepRptParam(3, 1) = " '" & VatLCRound & "'"

                OtherRepRptParam(4, 0) = "Curr1"
                OtherRepRptParam(4, 1) = " '" & gbCurr1 & "'"
            End If


            '*************************************************************************
            If Not withPriviewPrint AndAlso _me.Name = frmReprintSlips.Name Then GoTo BackOfficeDirectprint
            Select Case _me.Name

                Case FrmSalesInvoice.Name, FrmModRemInvoice.Name, "FrmPurOfUsedBook", frmClientStatement.Name
BackOfficeDirectprint:
                    strPrev = Nothing
                    Dim sbr As ModPrint.SubReport
                    Dim str As String

                    If RptInvoice <> "rptInvPosLaserOPP.rpt" And RptInvoice <> "RptFaircoReportView.rpt" AndAlso RptInvoice <> "rptInvPosLaserSansirabView.rpt" AndAlso Not RptInvoice.Contains("ItemCertificate.rpt") Then
                        str = "SELECT * From " & StempPayMode
                        sbr = New ModPrint.SubReport("PayTerm")
                        sbr.AddECParameter("@PayTermsStr", str)

                        '...Jad JE... Req By Rita
                        sbr.AddFOParameter("ProundLC", "" & VatLCRound & "")
                        sbr.AddFOParameter("Curr1", "'" & gbCurr1 & "'")

                        ModPrint.InitSubReport(sbr)

                        str = "SELECT * From " & StempCompensation
                        sbr = New ModPrint.SubReport("PrivCardCompensation")
                        sbr.AddECParameter("@PrivCardCompensationStr", str)
                        ModPrint.InitSubReport(sbr)
                    End If

                    'Rita M @ 23.08.2017. Add subreport for payment due days
                    If RptInvoice = "rptInvPosLaserPetMart.rpt" OrElse RptInvoice.ToUpper = "RPTINVPOSLASERPETMARTWORate.RPT" Or RptInvoice = "rptInvPosLaserOPP.rpt" Or RptInvoice = "rptInvPosLaser.rpt" Or RptInvoice = "rptInvPosLaserMaliks.rpt" Or RptInvoice = "rptInvPosLaserDoculand.rpt" OrElse RptInvoice = "rptInvPosLaserKhayyatKanaan.rpt" OrElse RptInvoice = "rptInvPosLaserNovamart.rpt" OrElse
                        RptInvoice = "rptInvPosLaserApp.rpt" AndAlso Not RptInvoice.Contains("ItemCertificate.rpt") Or RptInvoice = "rptInvPosLaserDDT.rpt" Or
                         RptInvoice = "rptInvPosLaserAllinTrade.RPT" OrElse RptInvoice = "rptInvPosLaserAllinTradeWORate.RPT" OrElse
                         RptInvoice = "rptInvPosLaserISM.RPT" OrElse RptInvoice = "rptInvPosLaserMedix.rpt" OrElse
                         RptInvoice = "hintrag01.rpt" OrElse RptInvoice = "rptInvPosLaserMountainSteel.rpt" OrElse RptInvoice.ToUpper = "rptInvPosLaserEmileAoun.rpt".ToUpper Then
                        str = "SELECT DDA_INVHeader,DDA_Counter,DDA_Perc,DDA_Value,DDA_ValueDate,DDA_Remark,CT_CURRCODE FROM " & StempPaymentDueDays
                        sbr = New ModPrint.SubReport("PaymentDueDays")
                        sbr.AddECParameter("@PaymentDueDays", str)
                        sbr.AddFOParameter("ProundLC", "" & VatLCRound & "")
                        sbr.AddFOParameter("Curr1", "'" & gbCurr1 & "'")

                        ModPrint.InitSubReport(sbr)
                    End If


                    Dim PrinterName As String = ""

                    If Not ClocHO AndAlso (_me.Name = "FrmSalesInvoice" OrElse _me.Name = "FrmPurOfUsedBook") AndAlso Not withPriviewPrint AndAlso DefaultLayout.ToUpper.Trim = "S" AndAlso Not Slip And gbLaserPrinterName = "" Then
                        Using prtDialog As New PrintDialog
                            prtDialog.PrinterSettings.Copies = CType(CInt(CStr(IIf(NbrCopies.Trim = "", "1", NbrCopies))), Short)
                            If prtDialog.ShowDialog = Windows.Forms.DialogResult.OK Then
                                PrinterName = prtDialog.PrinterSettings.PrinterName
                                If IsNumeric(prtDialog.PrinterSettings.Copies) AndAlso CInt(CStr(IIf(NbrCopies.Trim = "", "1", NbrCopies))) < prtDialog.PrinterSettings.Copies Then
                                    NbrCopies = CStr(prtDialog.PrinterSettings.Copies)
                                End If
                            End If
                        End Using
                    End If
                    If DoneByInDet Then
                        RptInvoice = RptInvoice.ToLower.Replace(".rpt", "DoneBy.rpt")
                    ElseIf SupressTotal = False Then
                        If RptInvoice = "" Then
                            RptInvoice = IIf(_RepName = "", "rptInvPosLaser.rpt", _RepName & ".rpt").ToString
                        Else
                            RptInvoice = IIf(_RepName = "", RptInvoice, _RepName & RptInvoice.ToLower.Replace("rptinvposlaser", "").Replace(".rpt", "") & ".rpt").ToString
                        End If
                    End If

                    If (PrintFundDisbursementItem Or PrintFundDisbursementPay) And RptInvoice.ToUpper.Contains("rptInvPosLaser".ToUpper) Then
                        RptInvoice = RptInvoice.ToLower.Replace(".rpt", "_WithFunds.rpt")
                    End If

                    If RptInvoice.Contains("_WithFunds") Then
                        sbr = New ModPrint.SubReport("Fund")
                        sbr.AddECParameter("@Fund", FundQuery)
                        sbr.AddECParameter("Curr1", gbCurr1)
                        sbr.AddECParameter("Curr2", gbCurr2)
                        sbr.AddECParameter("deci", IIf(gbDecNo = 2, 2, 3).ToString)
                        ModPrint.InitSubReport(sbr)
                    End If

                    If withPriviewPrint = False Then
                        PrintPODirectRep(_me, strsql, RptInvoice, CInt(CStr(IIf(NbrCopies.Trim = "", "1", NbrCopies))), rptParam,
                                         " ORDER BY invh_recid ,CASE WHEN RecType = 'B' THEN 0 ELSE 1 END ,INVD_Counter ",
                                         CType(IIf(RptInvoice.ToUpper.Contains("rptInvPosLaserApp".ToUpper), DTMails, Nothing), DataTable),, CType(IIf(RptInvoice.ToUpper.Contains("rptInvPosLaserApp".ToUpper), DtpDf, Nothing), DataTable),
                                         , IIf(RptInvoice.ToUpper.Contains("rptInvPosLaserApp".ToUpper), " and XXFiltrationXX ", "").ToString, True, PrinterName,
                                         CBool(IIf(_me.Name = FrmModRemInvoice.Name Or _me.Name = frmClientStatement.Name, True, False)),
                                         PrintCopy, _PrintBeforeClose, invRecidToUpdCtr) '...Jad JE...Company Logo Command
                    Else
                        PrintPODirectRep(_me, strsql, RptInvoice, CInt(NbrCopies), rptParam,
                                         " ORDER BY invh_recid , CASE WHEN RecType = 'B' THEN 0 ELSE 1 END ,INVD_Counter ", CType(IIf(RptInvoice.Contains("rptInvPosLaserApp"), DTMails, Nothing), DataTable),
                                         , CType(IIf(RptInvoice.Contains("rptInvPosLaserApp"), DtpDf, Nothing), DataTable), , IIf(RptInvoice.Contains("rptInvPosLaserApp"), " and XXFiltrationXX ", "").ToString,
                                         True, , withPriviewPrint, PrintCopy, _PrintBeforeClose, invRecidToUpdCtr) '...Jad JE...Company Logo Command
                    End If
                    SernoTemp = ""
                    If PrintBonAchat = True AndAlso Not copyBonAchat Then
                        PrintPODirectRep(_me, strsqlBonAchat, "rptInBonAchat.rpt", 1, OtherRepRptParam, , , , , , , , PrinterName, ,
                                         PrintCopy, _PrintBeforeClose)
                        'If copyBonAchat = False Then
                        '    OtherRepRptParam(1, 1) = " '" & True & "'"
                        '    PrintPODirectRep(_me, strsqlBonAchat, "rptInBonAchat.rpt", 1, OtherRepRptParam, , , , , , , , , , PrintCopy, _PrintBeforeClose)
                        'End If
                    End If

                    '   Case frmReprintSlips.Name  frmRevSalesInv.Name
                Case frmReprintSlips.Name, frmRevSalesInv.Name
                    'Reprint Slip Negation : Allow View Trans Total

                    TransTotalVisible = CBool(IIf(gbUser = "SU", True, NegationPriv("MnuSubRepSlips", "btnAllowToViewTransTotal")))

                    'Preview Query (fields should be included in report query having same names to apply filtration)
                    strsql1 = "Select "
                    strsql1 &= "INVH_DOCTYP1,INVH_DOCPREFIX,INVH_SERNO,INVH_DATE, CL_Code, "
                    strsql1 &= " CL_NO, INVH_CLNAME, CL_NAME, INVH_CLADDTEL,"
                    If curr = "" Then 'added by GS on 10/05/16
                        strsql1 &= CStr(IIf(Copercurr = "FC", " INVH_NETFC ", "")) & " INVH_NET, "
                        'added by GS on 10/05/16
                    Else
                        If curr = gbCurr1 Then
                            '    strsql1 &= " dbo.convert5FC('" & gbApp & "','" & gbComp & "','" & gbYear & "', ISNULL(NULLIF(INVH_NETfc,0),INVH_NET), ISNULL(NULLIF(invh_currfc,''),invh_curr),'" & gbCurr1 & "',ISNULL(NULLIF(INVH_RATEfc,0),INVH_RATE),INVH_Date) INVH_NET,"
                            If Copercurr = "FC" Then
                                strsql1 &= " case when isnull(invh_currfc,'') = '" & gbCurr1 & "'  then  INVH_NETfc else "
                                strsql1 &= "  dbo.convert5FC('" & gbApp & "','" & gbComp & "','" & gbYear & "', CONVERT (NUMERIC(18,3) ,dbo.convert5FC('" & gbApp & "','" & gbComp & "','" & gbYear & "',INVH_NETfc  ,invh_currfc  ,'" & gbCurr2 & "',INVH_RATEfc,INVH_Date)) ,'" & gbCurr2 & "'  ,'" & gbCurr1 & "',INVH_RATE,INVH_Date)  end  INVH_NET,"
                            Else
                                strsql1 &= " dbo.convert5FC('" & gbApp & "','" & gbComp & "','" & gbYear & "',  INVH_NET  ,invh_curr  ,'" & gbCurr1 & "',INVH_RATE,INVH_Date) INVH_NET,"
                            End If
                        ElseIf curr = gbCurr2 Then
                            If Copercurr = "FC" Then
                                strsql1 &= " INVH_NET ,"
                            Else
                                strsql1 &= " dbo.convert5FC('" & gbApp & "','" & gbComp & "','" & gbYear & "', INVH_NET,invh_curr,'" & gbCurr2 & "',INVH_RATE,INVH_Date) INVH_NET,"
                            End If
                        Else
                            strsql1 &= " INVH_NETFC INVH_NET ,"
                        End If
                    End If

                    strsql1 &= " " & IIf(curr.Trim <> "" And curr.Trim <> "FC", "'" & curr & "'", " CASE WHEN INVH_CURRFC IS NULL OR INVH_CURRFC = '' THEN INVH_CURR ELSE INVH_CURRFC END").ToString & " INVH_CURR , "
                    'strsql1 &= " INVH_CURR" & CStr(IIf(Copercurr = "FC", "FC", "")) & " INVH_CURR , "
                    'GS on 10/05/16
                    strsql1 &= " INVH_RKS "
                    strsql1 &= " ,'' RecType , 0 INVD_Counter "
                    strsql1 &= "from POS_SALEINVHEAD INNER JOIN dbo.Stk04_Clients "
                    strsql1 &= " ON INVH_CLIENTSUP = dbo.Stk04_Clients.CL_RecID"
                    strsql1 &= " Left Join " & ReprintSlipTempTable & " on InvHead = INVH_RECID"
                    strsql1 &= " where INVH_LOC = '" & Cloc & "'"
                    strsql1 &= " And [Chk] = 'True' "

                    With frm
                        frm.BuildSql(strsql1, , , , , strsql.ToString, , , , CStr(IIf(PrintBonAchat = True, strsqlBonAchat, Nothing)))
                        .FixSort = " ORDER BY invh_recid , CASE WHEN RecType = 'B' THEN 0 ELSE 1 END ,INVD_Counter "
                        .PrintCopy = PrintCopy
                        .ColDesign("INVH_DOCTYP1", -1, "Doc.Type", , AlignVertEnum.Top, , , , , True)
                        .ColDesign("INVH_DOCPREFIX", -1, "Prefix", , AlignVertEnum.Top, , , , , True)
                        .ColDesign("INVH_SERNO", -1, "Serial", , , , , , , , , frmPreviewGrid.Orders.Descending)
                        .ColDesign("INVH_DATE", -1, "Date", , , , , , , , , frmPreviewGrid.Orders.Descending, , , , "dd/MM/yyyy")
                        .ColDesign("CL_Code", -1, "Client No")
                        .ColDesign("CL_NO", 130, "Client Sub No")
                        .ColDesign("INVH_CLNAME", CInt(IIf(ClNameFromClientFile = False, 160, 0)), "Client Name", AlignHorzEnum.Near, , CBool(IIf(ClNameFromClientFile = False, True, False)), , , , , CBool(IIf(ClNameFromClientFile = False, True, False)))
                        .ColDesign("CL_NAME", CInt(IIf(ClNameFromClientFile = False, 0, 160)), "Client Name", AlignHorzEnum.Near, , CBool(IIf(ClNameFromClientFile = False, False, True)), , , , , CBool(IIf(ClNameFromClientFile = False, False, True)))
                        .ColDesign("INVH_CLADDTEL", 150, "Address", AlignHorzEnum.Near)
                        .ColDesign("INVH_NET", 100, "Total Amount", AlignHorzEnum.Far, , TransTotalVisible, , , , , TransTotalVisible, , , , , CStr(IIf(curr = "", IIf(Copercurr = gbCurr1, VatLCFormat, "#,##0.000"), IIf(curr = gbCurr1, VatLCFormat, "#,##0.000")))) 'CStr(IIf(Copercurr = "LL", "N0", "N2"))) 'edited by GS on 13/05/16
                        .ColDesign("INVH_RKS", 250, "Remarks", AlignHorzEnum.Near)
                        .ColDesign("INVH_CURR", -1, "Curr.", AlignHorzEnum.Center)
                        .ColDesign("RecType", 0, "", AlignHorzEnum.Near, , False, , , , , False, frmPreviewGrid.Orders.Ascending)
                        .ColDesign("INVD_Counter", 0, "", AlignHorzEnum.Center, , False, , , , , False, frmPreviewGrid.Orders.Ascending)
                        RptInvoice = GetFieldDesc("pos_splocation", "SL_Loc='" & Cloc & "'", "SL_RptInvoice")
                        If Slip = True Then
                            If ItmCetificate Then
                                RptInvoice = "rptInvPosSlipItemCertificate.rpt"
                            Else
                                RptInvoice = "rptInvPosSlip.rpt"
                            End If
                        Else
                            If ItmCetificate Then
                                RptInvoice = "rptInvPosLaserItemCertificate.rpt"
                            ElseIf SupressTotal = False Then
                                If RptInvoice = "" Then
                                    RptInvoice = IIf(_RepName = "", "rptInvPosLaser.rpt", _RepName & ".rpt").ToString
                                Else
                                    RptInvoice = IIf(_RepName = "", RptInvoice, _RepName & RptInvoice.ToLower.Replace("rptinvposlaser", "").Replace(".rpt", "") & ".rpt").ToString
                                End If

                            Else
                                If RptInvoice = "" Then
                                    RptInvoice = "rptInvPosLaserNoTot.rpt"
                                Else
                                    RptInvoice = Mid(RptInvoice, 1, InStr(RptInvoice, ".") - 1) & "NoTot.rpt"
                                End If
                            End If
                        End If

                        strPrev = Nothing
                        Dim sbr As frmPreviewGrid.SubReport
                        Dim str As String

                        If RptInvoice <> "rptInvPosLaserOPP.rpt" AndAlso RptInvoice <> "rptInvPosLaserOPPtoprint.rpt" AndAlso RptInvoice <> "RptFaircoReportView.rpt" AndAlso RptInvoice <> "rptInvPosLaserSansirabView.rpt" AndAlso Not RptInvoice.Contains("ItemCertificate.rpt") Then
                            str = "SELECT * From " & StempPayMode
                            sbr = New frmPreviewGrid.SubReport("PayTerm")
                            sbr.AddECParameter("@PayTermsStr", str)

                            '...Jad JE... Req By Rita
                            sbr.AddFOParameter("ProundLC", "" & VatLCRound & "")
                            sbr.AddFOParameter("Curr1", "'" & gbCurr1 & "'")

                            frm.InitSubReport(sbr)

                            str = "SELECT * From " & StempCompensation
                            sbr = New frmPreviewGrid.SubReport("PrivCardCompensation")
                            sbr.AddECParameter("@PrivCardCompensationStr", str)
                            frm.InitSubReport(sbr)
                        End If

                        'Rita M @ 23.08.2017. Add subreport for payment due days
                        If RptInvoice = "rptInvPosLaserPetMart.rpt" OrElse RptInvoice.ToUpper = "RPTINVPOSLASERPETMARTWORate.RPT" Or RptInvoice = "rptInvPosLaserOPP.rpt" OrElse RptInvoice = "rptInvPosLaserOPPtoprint.rpt" OrElse RptInvoice = "rptInvPosLaser.rpt" OrElse RptInvoice.StartsWith("rptInvPosLaserNoPriceAndDisc") Or RptInvoice = "rptInvPosLaserMaliks.rpt" Or RptInvoice = "rptInvPosLaserDoculand.rpt" OrElse RptInvoice = "rptInvPosLaserKhayyatKanaan.rpt" OrElse RptInvoice = "rptInvPosLaserNovamart.rpt" OrElse
                            RptInvoice = "rptInvPosLaserApp.rpt" Or RptInvoice = "rptInvPosLaserDDT.rpt" Or RptInvoice = "rptInvPosLaserAllinTrade.RPT" OrElse RptInvoice = "rptInvPosLaserAllinTradeWORate.RPT" OrElse RptInvoice = "rptInvPosLaserMedix.rpt" OrElse
                            RptInvoice = "rptInvPosLaserISM.rpt" OrElse RptInvoice = "hintrag01.rpt" OrElse RptInvoice = "rptInvPosLaserMountainSteel.rpt" orelse RptInvoice.ToUpper = "rptInvPosLaserEmileAoun.rpt".ToUpper Then
                            str = "SELECT DDA_INVHeader,DDA_Counter,DDA_Perc,DDA_Value,DDA_ValueDate,DDA_Remark,CT_CURRCODE FROM " & StempPaymentDueDays
                            sbr = New frmPreviewGrid.SubReport("PaymentDueDays")
                            sbr.AddECParameter("@PaymentDueDays", str)

                            sbr.AddFOParameter("ProundLC", "" & VatLCRound & "")
                            sbr.AddFOParameter("Curr1", "'" & gbCurr1 & "'")

                            frm.InitSubReport(sbr)
                        End If

                        'If RptInvoice = "rptInvPosLaserMedix.rpt" Then
                        '    str = " SELECT  SUM(INVD_NETFC) netService ,
                        '                    INVH_RECID
                        '            FROM    POS_SALEINVHEAD
                        '                    INNER JOIN ( SELECT INVD_NETFC ,
                        '                                        INVD_LOC ,
                        '                                        INVD_HEADID ,
                        '                                        INVD_STKID
                        '                                 FROM   POS_SALEINVDET
                        '                               ) POS_SALEINVDET ON INVD_LOC = INVH_LOC
                        '                                                   AND INVD_HEADID = INVH_RECID
                        '                    INNER JOIN ( SELECT STK_SERVICE ,
                        '                                        STK_RECID
                        '                                 FROM   " & CStr(IIf(ClocHO = False, "POS_STKFIL", "STK04_STKFIL")) & "
                        '                               ) STK04_STKFIL ON STK_RECID = INVD_STKID
                        '                    INNER JOIN TempReprintSlips_sa ON InvHead = INVH_RECID
                        '                                                      AND Serial = INVH_SERNO
                        '                                                      AND Chk = 'true'
                        '            WHERE   INVH_LOC = '" & Cloc & "'
                        '                    AND STK_SERVICE = 'S'
                        '            GROUP BY INVH_RECID "

                        '    sbr = New frmPreviewGrid.SubReport("TotService")
                        '    sbr.AddECParameter("@strsql", str)
                        '    sbr.AddFOParameter("deci", IIf(gbDecNo = 2, 2, 3).ToString)
                        '    frm.InitSubReport(sbr)
                        'End If

                        If _me.Name Is frmRevSalesInv.Name Then
                            If NbrCopies.Trim <> "" AndAlso IsNumeric(NbrCopies) Then .NBOfCop = CInt(NbrCopies)
                        End If
                        If DoneByInDet Then
                            RptInvoice = RptInvoice.ToLower.Replace(".rpt", "DoneBy.rpt")
                        End If


                        If (PrintFundDisbursementItem Or PrintFundDisbursementPay) And RptInvoice.ToUpper.Contains("rptInvPosLaser".ToUpper) Then
                            RptInvoice = RptInvoice.ToLower.Replace(".rpt", "_WithFunds.rpt")
                        End If

                        If RptInvoice.Contains("_WithFunds") Then
                            sbr = New frmPreviewGrid.SubReport("Fund")
                            sbr.AddECParameter("@Fund", FundQuery)
                            sbr.AddECParameter("Curr1", gbCurr1)
                            sbr.AddECParameter("Curr2", gbCurr2)
                            sbr.AddECParameter("deci", IIf(gbDecNo = 2, 2, 3).ToString)
                            frm.InitSubReport(sbr)
                        End If

                        .Maildt = CType(IIf(RptInvoice.ToUpper.Contains("rptInvPosLaserApp".ToUpper), DTMails, Nothing), DataTable)
                        .PdfDt = CType(IIf(RptInvoice.ToUpper.Contains("rptInvPosLaserApp".ToUpper), DtpDf, Nothing), DataTable)
                        .MailStr = IIf(RptInvoice.ToUpper.Contains("rptInvPosLaserApp".ToUpper), " and XXFiltrationXX ", "").ToString

                        '.ShowDialog(strPrev, frmReprintSlips.Name, CStr(IIf(Slip = True, "rptInvPosSlip.rpt", CStr(IIf(SupressTotal = False, "rptInvPosLaser.rpt", "rptInvPosLaserNoTot.rpt")))), rptParam, , , , , , , , , , , CStr(IIf(PrintBonAchat = True, "rptInBonAchat.rpt", "")), OtherRepRptParam)
                        .ShowDialog(strPrev, frmReprintSlips.Name, RptInvoice, rptParam, , , , , , , , , , ,
                                    CStr(IIf(PrintBonAchat = True AndAlso Not copyBonAchat, "rptInBonAchat.rpt", "")), OtherRepRptParam, , , ,
                                    True, _PrintBeforeClose, directprint, invRecidToUpdCtr)
                    End With

            End Select

        Catch ex As Exception
            Throw ex
        Finally
            SernoTemp = ""
            If Not frm Is Nothing Then frm = Nothing
            Cursor.Current = Cursors.Default
            DropSqlObject(ClinetBalnceSumtable, "TABLE")

            DropSqlObject(StempCsh, "TABLE")
            DropSqlObject(StempPayMode, "TABLE")
            DropSqlObject(StempCompensation, "TABLE")
            DropSqlObject(StempPaymentDueDays, "TABLE")

        End Try
    End Sub
    Private Sub FillPaymentMode(ByVal WSerno As Long)
        Dim cnSql As SqlConnection
        Dim cmSql As SqlCommand
        Dim drSql As SqlDataReader
        Dim strsql As String
        Dim Desc As String
        Dim i As Integer
        Try

            For i = 1 To 15
                txtpaym(i) = ""
            Next

            cnSql = New SqlConnection(ConnectionString)
            cnSql.Open()

            strsql = "select * from Pos_cashTransaction INNER JOIN pos_saleinvhead "
            strsql &= " ON invh_recid = CT_Invheader Where CT_INVHeader = " & WSerno
            strsql &= " And CT_Location = '" & Cloc & "'"

            cmSql = New SqlCommand(strsql, cnSql)
            drSql = cmSql.ExecuteReader

            i = 1

            With drSql
                If .HasRows Then
                    While .Read
                        If Val(.Item("CT_Amount").ToString) <> 0 Then
                            Desc = GetFieldDesc("stk04_SETTLEMENTTYPE", "CT_Code='" & .Item("CT_SETTLTYPE").ToString & "'", "ST_DESC")

                            txtpaym(i) = UCase(Desc)

                            'If .Item("CT_currcode").ToString = gbCurr1 Then
                            'txtpaym(i) = txtpaym(i) + ":" + " " + IIf(.Item("CT_OPSIGN").Value = "-", "-", "") + " " + Format(.Fields("CT_Amount").to, VatLCFormat)
                            'Else
                            txtpaym(i) = txtpaym(i) + ":" + " " + CStr(IIf(.Item("CT_OPSIGN").ToString = "-", "-", "")) + " " + .Item("CT_Amount").ToString
                            'End If

                            txtpaym(i) = txtpaym(i) + .Item("CT_currcode").ToString

                            If Not .Item("CT_Amount").ToString Is DBNull.Value And .Item("CT_currcode").ToString <> Copercurr Then
                                If .Item("CT_opsign").ToString = "-" Then

                                    If Copercurr = gbCurr2 Then
                                        txtpaym(i) = txtpaym(i) + " , C/V " + Format(-CDbl(.Item("CT_amtopcurr").ToString), "######,##0.00") + Copercurr
                                    ElseIf Copercurr = "FC" Then
                                        txtpaym(i) = txtpaym(i) + " , C/V " + Format(-CDbl(.Item("CT_amtopcurr").ToString), "######,##0.00") + CStr(IIf(.Item("INVH_CURRFC").ToString Is DBNull.Value, "", .Item("INVH_CURRFC").ToString))
                                    Else
                                        txtpaym(i) = txtpaym(i) + " , C/V " + Format(-CDbl(.Item("CT_amtOPCURr").ToString), VatLCFormat) + Copercurr
                                    End If

                                Else
                                    If Copercurr = gbCurr2 Then
                                        txtpaym(i) = txtpaym(i) + " , C/V " + Format(.Item("CT_amtopcurr").ToString, "######,##0.00") + Copercurr
                                    ElseIf Copercurr = "FC" Then
                                        txtpaym(i) = txtpaym(i) + " , C/V " + Format(.Item("CT_amtopcurr").ToString, "######,##0.00") + " " + CStr(IIf(.Item("invh_currfc").ToString Is DBNull.Value, "", .Item("invh_currfc").ToString))
                                    Else
                                        txtpaym(i) = txtpaym(i) + " , C/V " + Format(.Item("CT_amtOPCURr").ToString, VatLCFormat) + Copercurr
                                    End If
                                End If
                            End If
                            If .Item("CT_SETTLTYPE").ToString = "315" Then
                                txtpaym(i) = txtpaym(i) + " ,Bon Rest # : " & .Item("ct_bonachatser").ToString
                            End If
                            If .Item("CT_SETTLTYPE").ToString = "314" Then
                                txtpaym(i) = txtpaym(i) + " ,Bon Achat # : " & .Item("ct_bonachatser").ToString
                            End If

                            i = i + 1
                        End If
                    End While
                End If
            End With

        Catch ex As Exception
            Throw ex
        End Try
    End Sub

    Public Sub PrintAdjust(ByVal Cloc As String, ByVal WSerno As Double, ByVal Wprint As Boolean, ByVal SelType As String, ByVal PrtSupRef As Boolean, ByVal DocPrefix As String, ByVal frmMe As Form, Optional ByVal TempTable As String = "", Optional ByVal Laser As Boolean = False, Optional _PrintBeforeClose As Boolean = False, Optional PrintDirect As Boolean = False) 'edited by GS on 12/07/16 (add param Laser)

        Dim i As Integer
        'Print Invoice
        Dim frm As frmPreviewGrid
        Dim strsql As String
        Dim reportN As String
        Dim strPrev(,) As String
        Try
            Cursor.Current = Cursors.WaitCursor
            frm = New frmPreviewGrid

            DropSqlObject("Temp_POS_SALEINVHEAD" & gbUser, "TABLE")
            DropSqlObject("Temp_POS_SALEINVDET" & gbUser, "TABLE")

            'fill Temp_POS_SALEINVHEAD 
            strsql = "  Select * into  Temp_POS_SALEINVHEAD" & gbUser & " From Pos_saleinvhead "
            Select Case frmMe.Name

                'Conditions = current location, serial number
                Case frmStkAdjust.Name
                    strsql &= " where POS_SALEINVHEAD.INVH_LOC = '" & Cloc & "'"
                    strsql &= " and POS_SALEINVHEAD.INVH_RECID = '" & WSerno & "'"

                    'Join with TempTable sent from form, conditions = current location, Type A1/A2, checked records
                Case frmReprintSlips.Name
                    strsql &= "Left Join " & TempTable & " on [Doctype] = INVH_DOCTYP1 and [Prefix] = INVH_DOCPREFIX and [Serial] = INVH_SERNO "
                    strsql &= "where POS_SALEINVHEAD.INVH_LOC = '" & Cloc & "'"
                    strsql &= "And INVH_DOCTYP1 = '" & SelType & "' "
                    strsql &= "And [Chk] = 'True' "
            End Select

            Execute(strsql)

            'fill Temp_POS_SALEINVDET 
            strsql = "  Select pos_saleinvdet.* into  Temp_POS_SALEINVDET" & gbUser & " From temp_Pos_saleinvhead" & gbUser & " "
            strsql &= " inner join pos_saleinvdet ON "
            strsql &= " INVH_RECID = POS_SALEINVDET.INVD_HEADID AND"
            strsql &= " INVH_LOC = INVD_LOC"
            Execute(strsql)

            'Report Query
            strsql = " SELECT"
            strsql &= " POS_SALEINVHEAD.INVH_LOC, POS_SALEINVHEAD.INVH_RECID, POS_SALEINVHEAD.INVH_DOCTYP1, "
            strsql &= " POS_SALEINVHEAD.INVH_DOCPREFIX, POS_SALEINVHEAD.INVH_SERNO, POS_SALEINVHEAD.INVH_DATE, "
            strsql &= " POS_SALEINVHEAD.INVH_EMP,  POS_SALEINVHEAD.INVH_CSHREG, "
            strsql &= " POS_SALEINVHEAD.INVH_CLNAME, POS_SALEINVHEAD.INVH_CLADDTEL, POS_SALEINVHEAD.INVH_RKS, "
            strsql &= " POS_SALEINVHEAD.INVH_GROSS, POS_SALEINVHEAD.INVH_DISC, POS_SALEINVHEAD.INVH_ADDDISC, "
            strsql &= " POS_SALEINVHEAD.INVH_NET, POS_SALEINVHEAD.INVH_VATLC, POS_SALEINVHEAD.INVH_DSTAMP,"
            strsql &= " POS_SALEINVDET.INVD_STATUS, POS_SALEINVDET.INVD_ITMSTATUS, POS_SALEINVDET.INVD_ITMDESC, "
            strsql &= " POS_SALEINVDET.INVD_QTY, POS_SALEINVDET.INVD_UPRICE, POS_SALEINVDET.INVD_DISC, "
            strsql &= " POS_SALEINVDET.INVD_NET, POS_SALEINVDET.INVD_GIFT, POS_SALEINVDET.INVD_Remarks,"
            strsql &= " STK04_Location.Loc_Desc,POS_EMPLOYEE.E_Name,ISNULL(Com_Folio.Fo_addres,'') as [Fo_addres] ,ISNULL(Com_Folio.Fo_phone1,'') as [Fo_phone1] ,"
            strsql &= " POS_SALEINVDET.INVD_Barcode, POS_STKFIL.STK_VATTYPE,POS_STKFIL.STK_ItmClass ItmClass, POS_STKFIL.STK_VATPER,POS_STKFIL.STK_Ref,POS_STKFIL.stk_singleref1,POS_STKFIL.stk_singleref2 "
            'INVD_Color,INVD_DetGrid,INVD_ExpDate,INVD_Batch
            strsql &= " ,IsNull(COALESCE(POS_SALEINVDET.INVD_DetGrid,POS_SALEINVDET.INVD_ExpDate),'') ExpirySize , isnull(COALESCE(Stk04_COLOR.Color_Reference COLLATE SQL_Latin1_General_CP1256_CI_AS,POS_SALEINVDET.INVD_Batch COLLATE SQL_Latin1_General_CP1256_CI_AS),'') LotColor, "
            strsql &= " CL_Code,CL_NO,CL_NAME,(Select top 1 SRef_SupRef from STK04_StkSupRef where SRef_StkRecId = STK_RECID order by SRef_DStamp desc) [SupRef], INVD_Salesman,STT_PRINTED1,STT_PRINTED2,STT_FOOTER "
            If Laser Then strsql &= " , Meas_desc "
            strsql &= " From "
            strsql &= " Temp_POS_SALEINVHEAD" & gbUser & " POS_SALEINVHEAD Inner join "
            strsql &= " Temp_POS_SALEINVDET" & gbUser & " POS_SALEINVDET On  "
            strsql &= " POS_SALEINVHEAD.INVH_RECID = POS_SALEINVDET.INVD_HEADID AND"
            strsql &= " POS_SALEINVHEAD.INVH_Loc = POS_SALEINVDET.INVD_Loc "
            strsql &= " inner join STK04_Location STK04_Location On POS_SALEINVHEAD.INVH_LOC = STK04_Location.Loc_Code "
            strsql &= " left join " & CStr(IIf(ClocHO = False, "POS_Employee", "STK04_Employee")) & " POS_EMPLOYEE On POS_SALEINVHEAD.INVH_EMP = POS_EMPLOYEE.E_Code "
            strsql &= " left join Com_Folio Com_Folio On STK04_Location.Loc_Folio = Com_Folio.Fo_serno "
            strsql &= " and  STK04_Location.Loc_FolioSub = Com_Folio.Fo_subno "
            strsql &= " left join " & CStr(IIf(ClocHO = False, "pos_stkfil", "STK04_stkfil")) & " POS_STKFIL On POS_SALEINVDET.INVD_STKID = POS_STKFIL.STK_RECID "
            strsql &= " LEFT JOIN Stk04_COLOR ON Stk04_COLOR.Color_Code=POS_SALEINVDET.INVD_Color "
            strsql &= " Left join STK04_Clients on CL_RECID = INVH_CLIENTSUP "
            strsql &= " Left Join pos_stocktransaction on STT_LOCATION = POS_SALEINVHEAD.INVH_LOC and STT_DOCTYPE1 = POS_SALEINVHEAD.INVH_DOCTYP1 and STT_PREFIX = POS_SALEINVHEAD.INVH_DOCPREFIX "
            If Laser Then strsql &= " left join STK04_Measurements on stk_unit = Meas_Unit "

            Dim NbrCopies As String
            'added by GS on 19/03/16
            Dim strsqlnbcopy As String
            Select Case frmMe.Name
                Case frmStkAdjust.Name
                    strsqlnbcopy = " SELECT top 1 STT_NBRCOPY FROM pos_stocktransaction WHERE STT_LOCATION='" & Cloc & "' "
                    strsqlnbcopy &= " AND STT_DOCTYPE1='" & SelType & "' AND STT_Active='Y' and STT_PREFIX = '" & DocPrefix.Trim & "'"
                    NbrCopies = ExecuteScalar(strsqlnbcopy)
            End Select
            'GS on 19/03/16
            Dim rptParam(5, 1) As Object


            If SelType = "A1" Then
                rptParam(0, 0) = "Title1"
                rptParam(0, 1) = IIf(Laser, " 'Adjustment IN'", " 'Adjustment IN Slip'").ToString 'edited by GS on 20/07/16
            ElseIf SelType = "A2" Then
                rptParam(0, 0) = "Title1"
                rptParam(0, 1) = IIf(Laser, " 'Adjustment OUT'", " 'Adjustment OUT Slip'").ToString 'edited by GS on 20/07/16
            End If

            '.Formulas(5) = "VatReg='" & vVATReg & "'"
            rptParam(1, 0) = "DecNo"
            rptParam(1, 1) = gbDecNo

            If PrtSupRef = True Then
                rptParam(2, 0) = "PrtSupRef"
                rptParam(2, 1) = " '1'"
                rptParam(3, 0) = "BarcCap"
                rptParam(3, 1) = " 'Sup. Ref'"
            Else
                rptParam(2, 0) = "PrtSupRef"
                rptParam(2, 1) = " '0'"
                rptParam(3, 0) = "BarcCap"
                rptParam(3, 1) = "'" & CStr(IIf(CRetVal = "1", "Barcode", "Reference")) & "'"
            End If

            rptParam(4, 0) = "VATReg"
            rptParam(4, 1) = "'" & sVATreg & "'"

            rptParam(5, 0) = "CRetVal"
            rptParam(5, 1) = "'" & CRetVal & "'"

            Select Case frmMe.Name
                Case frmStkAdjust.Name
                    PrintPODirectRep(frmMe, strsql, IIf(Laser, "rptInvAdjLaser.rpt", "rptInvAdj.rpt").ToString, CInt(NbrCopies), rptParam, , , , , , , , , , , _PrintBeforeClose) 'edited by GS on 12/07/16 (add iif)
                Case frmReprintSlips.Name
                    'Preview Query (fields should be included in report query having same names to apply filtration)
                    If PrintDirect Then
                        PrintPODirectRep(frmMe, strsql, IIf(Laser, "rptInvAdjLaser.rpt", "rptInvAdj.rpt").ToString, CInt(NbrCopies), rptParam, , , , , , , , , , , _PrintBeforeClose)
                    Else
                        Dim strsql1 As String
                        strsql1 = "Select "
                        strsql1 &= "DocType INVH_DOCTYP1,Prefix INVH_DOCPREFIX,Serial INVH_SERNO,Date INVH_DATE, ClientNo CL_Code, ClientSubNo CL_NO, ClientName CL_NAME, Address INVH_CLADDTEL, Remarks INVH_RKS "
                        strsql1 &= "from " & TempTable & " "
                        strsql1 &= "Where [Chk] = 'True' "

                        With frm
                            frm.BuildSql(strsql1, , , , , strsql.ToString)

                            .ColDesign("INVH_DOCTYP1", -1, "Doc.Type", , AlignVertEnum.Top, , , , , True)
                            .ColDesign("INVH_DOCPREFIX", -1, "Prefix", , AlignVertEnum.Top, , , , , True)
                            .ColDesign("INVH_SERNO", -1, "Serial", , , , , , , , , frmPreviewGrid.Orders.Descending)
                            .ColDesign("INVH_DATE", -1, "Date", , , , , , , , , frmPreviewGrid.Orders.Descending, , , , "dd/MM/yyyy")
                            .ColDesign("CL_Code", 0, "Client No", , , False, , , , , False, , , , , , , , , , , False)
                            .ColDesign("CL_NO", 0, "Client Sub No", , , False, , , , , False, , , , , , , , , , , False)
                            .ColDesign("CL_NAME", 0, "Client Name", AlignHorzEnum.Near, , False, , , , , False, , , , , , , , , , , False)
                            .ColDesign("INVH_CLADDTEL", 0, "Address", AlignHorzEnum.Near, , False, , , , , False, , , , , , , , , , , False)
                            .ColDesign("INVH_RKS", 200, "Remarks", AlignHorzEnum.Near)
                            .ShowDialog(strPrev, frmReprintSlips.Name, IIf(Laser, "rptInvAdjLaser.rpt", "rptInvAdj.rpt").ToString, rptParam) 'edited by GS on 12/07/16 (add iif)
                        End With
                    End If

            End Select

        Catch ex As Exception
            Throw ex
        Finally
            If Not frm Is Nothing Then frm = Nothing
            DropSqlObject("Temp_POS_SALEINVHEAD" & gbUser, "TABLE")
            DropSqlObject("Temp_POS_SALEINVDET" & gbUser, "TABLE")
            Cursor.Current = Cursors.Default
        End Try
    End Sub
    Public Sub printRepairSlip(ByVal Cloc As String, ByVal WSerno As Double, ByVal Wprint As Boolean, ByVal PrtSupRef As Boolean, ByVal DocPrefix As String, ByVal RepairNbrCopies As Integer, ByVal frmMe As Form, Optional ByVal TempTable As String = "", Optional ByVal Laser As Boolean = False, Optional _PrintBeforeClose As Boolean = False, Optional PrintDirect As Boolean = False) 'edited by GS on 12/07/16 (add param Laser)
        Dim i As Integer
        'Print Invoice
        Dim frm As frmPreviewGrid
        Dim strsql As StringBuilder
        Dim strsql1 As String
        Dim reportN As String
        Dim drsql As SqlDataReader
        Dim strPrev(,) As String
        Try
            Cursor.Current = Cursors.WaitCursor
            frm = New frmPreviewGrid

            strsql = New StringBuilder

            'Report Query
            With strsql
                .Clear()
                .Append("SELECT  Convert(Varchar,INVH_SERNO) RepH_InvRecID,RepH_Loc,loc_desc,RepH_RecId,RepH_DOCTYP1,RepH_DOCPREFIX,RepH_SerNo, ")
                .Append(" RepH_Date, RepH_USTMP,")

                If CRetVal = "1" Then
                    .Append(" isnull((SELECT TOP 1 case when ltrim(dbar_barcode) = '' then null else dbar_barcode end dbar_barcode FROM dbo.STK04_DetBarcode WHERE dbar_stkid = Stk_Recid")
                    .Append(" AND ISNULL(dbar_grid,'')=ISNULL(RepD_Grid,'')")
                    .Append(" AND ISNULL(dbar_Detgrid,'') =ISNULL(RepD_DetGrid,'')")
                    .Append(" AND ISNULL(dbar_color,'') =ISNULL(RepD_Color,'')")
                    .Append(" AND ISNULL(dbar_expdate,'') = ISNULL(RepD_ExpDate,'')")
                    .Append(" AND ISNULL(dbar_batch,'')=ISNULL(RepD_Batch,'')),STK_REF) ")
                ElseIf CRetVal = "2" Then
                    .Append(" STK_REF ")
                ElseIf CRetVal = "3" Then
                    .Append(" ISNULL(STK_SingleRef1,STK_REF) ")
                ElseIf CRetVal = "4" Then
                    .Append(" ISNULL(STK_SingleRef2,STK_REF) ")
                End If

                .Append(" STK_REF,STK_ItmClass, STK_desc, RepD_DetGrid, Color_Reference Color_Desc, ")
                .Append(" replace(RepD_ExpDate,'00.','') RepD_ExpDate, RepD_Batch, RepD_Qty, RepD_Remark, RepD_ExpDelDt, ")

                .Append(" CONVERT(VARCHAR,CL_Code) + ' / ' +  CONVERT(VARCHAR,CL_No) + ' - ' + RepH_CltName AS [Client] ,")
                .Append(" RepD_Status [Status] , RepD_ActDelDt ")
                .Append(" ,RepH_CltAddress,RepH_DelAddress,RepH_Remark,RepH_SpecialInstructions , STT_PRINTED1,STT_PRINTED2,STT_FOOTER,STT_DESC ")

                .Append(" FROM dbo.POS_REPAIRITEMSHead INNER JOIN dbo.POS_REPAIRITEMSDet ")
                .Append(" ON dbo.POS_REPAIRITEMSHead.RepH_Loc = dbo.POS_REPAIRITEMSDet.RepD_Loc ")
                .Append(" AND dbo.POS_REPAIRITEMSHead.RepH_RecId = dbo.POS_REPAIRITEMSDet.RepD_HRecId ")
                .Append(" LEFT JOIN POS_SALEINVHEAD ON INVH_RECID =RepH_InvRecID")
                .Append(" INNER JOIN " & CStr(IIf(ClocHO = False, "pos_stkfil", "STK04_stkfil")) & " POS_STKFIL ON dbo.POS_REPAIRITEMSDet.RepD_StkId = POS_STKFIL.STK_RECID ")
                .Append(" INNER JOIN dbo.stk04_location ON dbo.POS_REPAIRITEMSHead.RepH_Loc = dbo.stk04_location.loc_code ")
                .Append(" INNER JOIN pos_stocktransaction ON  stt_doctype1 = RepH_DOCTYP1 AND STT_PREFIX= RepH_DOCPREFIX AND  RepH_Loc=STT_LOCATION  ")
                .Append(" LEFT JOIN stk04_color ON RepD_Color=color_code ")
                .Append(" LEFT JOIN Stk04_Clients ON CL_RecID=RepH_ClRecID ")
                Select Case frmMe.Name
                    Case FrmSalesInvoice.Name, frmRepairSlip.Name, FrmModRemInvoice.Name, frmClientStatement.Name
                        .Append(" WHERE RepH_Loc='" & Cloc & "' AND RepH_RecId='" & WSerno & "'")
                    Case frmReprintSlips.Name
                        .Append("Left join " & TempTable & " on RepH_DOCTYP1 = [DocType] and RepH_DOCPREFIX = [Prefix] and RepH_SerNo = [Serial] ")
                        .Append(" WHERE RepH_Loc='" & Cloc & "' ")
                        .Append(" and [Chk] = 'True' ")
                End Select

            End With

            Dim rptParam(4, 1) As Object

            rptParam(0, 0) = "VATReg"
            rptParam(0, 1) = "'VAT#: " & sVATreg & "'" 'modified by GS on 18/07/16 (add VAT#:)

            rptParam(1, 0) = "DecNo"
            rptParam(1, 1) = gbDecNo

            rptParam(2, 0) = "FoReference" : rptParam(2, 1) = "'" & CStr(IIf(CRetVal = "1", "Barcode", CStr(IIf(Laser, "Item Reference", "Itm.Ref")))) & "'"
            'will be sended in query 
            ''..Print
            'strsql1 = "select stt_printed1,stt_printed2,stt_footer,stt_nbrcopy from pos_stocktransaction "
            'strsql1 &= " where stt_doctype1 = 'UR' and stt_prefix = '" & DocPrefix
            'strsql1 &= "' and stt_location = '" & Cloc & "'"

            'ReadFromDBToDataReader(strsql1, drsql)

            'If drsql.HasRows Then
            '    drsql.Read()
            '    rptParam(2, 0) = "Header1"
            '    rptParam(2, 1) = "'" & drsql.Item("stt_printed1").ToString & "'"
            '    rptParam(3, 0) = "Header2"
            '    rptParam(3, 1) = "'" & drsql.Item("stt_printed2").ToString & "'"
            '    rptParam(4, 0) = "Footer"
            '    rptParam(4, 1) = "'" & drsql.Item("stt_footer").ToString & "'"
            'End If

            Select Case frmMe.Name

                Case FrmSalesInvoice.Name, frmRepairSlip.Name, FrmModRemInvoice.Name, frmClientStatement.Name
                    PrintPODirectRep(frmMe, strsql.ToString, IIf(Laser, "rptRepairLaser.rpt", "rptRepairSlip.rpt").ToString, RepairNbrCopies, rptParam, , , , , , , , , , , _PrintBeforeClose) 'edited by GS on 12/07/16 (add iif)

                Case frmReprintSlips.Name
                    'Preview Query (fields should be included in report query having same names to apply filtration)
                    If PrintDirect Then
                        PrintPODirectRep(frmMe, strsql.ToString, IIf(Laser, "rptRepairLaser.rpt", "rptRepairSlip.rpt").ToString, RepairNbrCopies, rptParam, , , , , , , , , , , _PrintBeforeClose)
                    Else
                        strsql1 = "Select "
                        strsql1 &= "DocType RepH_DOCTYP1,Prefix RepH_DOCPREFIX,Serial RepH_SerNo,Date RepH_Date "
                        strsql1 &= " ,CONVERT(VARCHAR,ClientNo) + ' - ' +  CONVERT(VARCHAR,ClientSubNo) + ' - ' + ClientName AS [Client] "
                        strsql1 &= "from " & TempTable & " "
                        strsql1 &= "Where [Chk] = 'True' "

                        With frm
                            frm.BuildSql(strsql1, , , , , strsql.ToString)

                            .ColDesign("RepH_DOCTYP1", 75, "Doc.Type", , AlignVertEnum.Top, , , , , True)
                            .ColDesign("RepH_DOCPREFIX", 70, "Prefix", , AlignVertEnum.Top, , , , , True)
                            .ColDesign("RepH_SerNo", 70, "Serial", , , , , , , , , frmPreviewGrid.Orders.Descending)
                            .ColDesign("RepH_Date", 90, "Date", , , , , , , , , frmPreviewGrid.Orders.Descending, , , , "dd/MM/yyyy")
                            .ColDesign("Client", -1, "Client", AlignHorzEnum.Near, AlignVertEnum.Top, , , , , True)
                            .ShowDialog(strPrev, frmReprintSlips.Name, IIf(Laser, "rptRepairLaser.rpt", "rptRepairSlip.rpt").ToString, rptParam) 'edited by GS on 12/07/16 (add iif)
                        End With
                    End If
            End Select

        Catch ex As Exception
            Throw ex
        Finally
            If Not frm Is Nothing Then frm = Nothing
            Cursor.Current = Cursors.Default
        End Try
    End Sub

    Public Sub PrintDisposal(ByVal Cloc As String, ByVal WSerno As Double, ByVal Wprint As Boolean, ByVal SelType As String, ByVal PrtSupRef As Boolean, ByVal DocPrefix As String, ByVal DisposalStatus As String, ByVal frmForm As Form, Optional ByVal TempTable As String = "", Optional ByVal Laser As Boolean = False, Optional printdirect As Boolean = False) 'edited by GS on 12/07/16

        Dim i As Integer
        Dim frm As frmPreviewGrid
        Dim strsql, strsql1, dispUser As String
        Dim reportN As String
        Dim cnsql As SqlConnection
        Dim cmsql As SqlCommand
        Dim strPrev(,) As String
        Try
            Cursor.Current = Cursors.WaitCursor
            frm = New frmPreviewGrid

            DropSqlObject("Temp_POS_SALEDISPHEAD" & gbUser, "TABLE")
            DropSqlObject("Temp_POS_SALEDISPDET" & gbUser, "TABLE")

            'fill Temp_POS_SALEDISPHEAD from POS_DISPOSALHEADER
            strsql = "  Select * into  Temp_POS_SALEDISPHEAD" & gbUser & " From POS_DISPOSALHEADER  "

            Select Case frmForm.Name

                Case "frmStkDisposal"
                    strsql &= " where POS_DISPOSALHEADER.DISPH_RECID = '" & WSerno & "' "
                    Select Case DisposalStatus
                        Case "N"
                            strsql &= "and POS_DISPOSALHEADER.DISPH_STATUS = 'N' "
                        Case "R"
                            strsql &= "and POS_DISPOSALHEADER.DISPH_STATUS = 'R' "
                    End Select

                Case "frmReprintSlips"
                    strsql &= "Left Join " & TempTable & " on DISPH_RECID = [Serial] "
                    strsql &= "inner join POS_DISPOSALDETAILS ON "
                    strsql &= "DISPH_RECID = POS_DISPOSALDETAILS.DISPD_HRECID And "
                    strsql &= "DISPH_LOCATION = DISPD_LOCATION And DISPD_STATUS = DISPH_STATUS "
                    strsql &= "where [Chk] = 'True' "
                    strsql &= "and POS_DISPOSALHEADER.DISPH_STATUS = [Status] "
            End Select

            strsql &= " and POS_DISPOSALHEADER.DISPH_LOCATION = '" & Cloc & "'"

            Execute(strsql)

            strsql = "  Select POS_DISPOSALDETAILS.* into  Temp_POS_SALEDISPDET" & gbUser & " From Temp_POS_SALEDISPHEAD" & gbUser & " "
            strsql &= " inner join POS_DISPOSALDETAILS ON "
            strsql &= " DISPH_RECID = POS_DISPOSALDETAILS.DISPD_HRECID AND"
            strsql &= " DISPH_LOCATION = POS_DISPOSALDETAILS.DISPD_LOCATION AND POS_DISPOSALDETAILS.DISPD_STATUS = DISPH_STATUS "
            Execute(strsql)

            strsql = "Select SP_UserCN from STK04_SysParam "

            cnsql = New SqlConnection(ConnectionString)
            cnsql.Open()
            cmsql = New SqlCommand(strsql.ToString, cnsql)

            If Not cmsql.ExecuteScalar Is Nothing AndAlso cmsql.ExecuteScalar.ToString = "C" Then
                dispUser = "DISPH_USTAMP "
            Else
                dispUser = "User_Name"
            End If

            'Report Query
            strsql = "SELECT distinct "

            If CRetVal = "1" Then
                strsql &= " isnull((SELECT TOP 1 case when ltrim(dbar_barcode) = '' then null else dbar_barcode end dbar_barcode FROM dbo.STK04_DetBarcode WHERE dbar_stkid = Stk_Recid"
                strsql &= " AND ISNULL(dbar_grid,'')=ISNULL(POS_DISPOSALDETAILS.DISPD_GRIDCODE,'')"
                strsql &= " AND ISNULL(dbar_Detgrid,'') =ISNULL(POS_DISPOSALDETAILS.DISPD_GRIDSIZE,'')"
                strsql &= " AND ISNULL(dbar_color,'') =ISNULL(POS_DISPOSALDETAILS.DISPD_COLORCODE,'')"
                strsql &= " AND ISNULL(dbar_expdate,'') = ISNULL(POS_DISPOSALDETAILS.DISPD_EXPIRYDATE,'')"
                strsql &= " AND ISNULL(dbar_batch,'')=ISNULL(POS_DISPOSALDETAILS.DISPD_BATCHINFO,'')),STK_REF) "
            ElseIf CRetVal = "2" Then
                strsql &= " STK_REF "
            ElseIf CRetVal = "3" Then
                strsql &= " ISNULL(STK_SingleRef1,STK_REF) "
            ElseIf CRetVal = "4" Then
                strsql &= " ISNULL(STK_SingleRef2,STK_REF) "
            End If
            strsql &= " [Barcode],  "
            'strsql &= "Case when POS_DISPOSALDETAILS.DISPD_Barcode is null OR POS_DISPOSALDETAILS.DISPD_Barcode = '' then STK_Ref else POS_DISPOSALDETAILS.DISPD_Barcode end [Barcode], "
            strsql &= "DISPH_RECID,DISPH_LOCATION, DISPH_REMARKS,DISPH_Date , DISPH_DSTAMP, "

            strsql &= dispUser & " [DISPH_USTAMP], "
            strsql &= "POS_DISPOSALDETAILS.DISPD_STATUS, POS_DISPOSALDETAILS.DISPD_QTY, POS_DISPOSALDETAILS.DISPD_REMARKS, LOC_DESC, STK_DESC, Color_Reference color_desc,POS_DISPOSALDETAILS.DISPD_GRIDSIZE,CASE WHEN  SUBSTRING(POS_DISPOSALDETAILS.DISPD_EXPIRYDATE,1,2) = '00' THEN SUBSTRING(POS_DISPOSALDETAILS.DISPD_EXPIRYDATE,4,8)  ELSE POS_DISPOSALDETAILS.DISPD_EXPIRYDATE end DISPD_EXPIRYDATE ,POS_DISPOSALDETAILS.DISPD_BatchInfo,stk_itmclass " 'christina
            strsql &= " , meas_desc   "
            strsql &= "from Temp_POS_SALEDISPHEAD" & gbUser & " POS_DISPOSALHEADER "
            strsql &= "Inner join  Temp_POS_SALEDISPDET" & gbUser & " POS_DISPOSALDETAILS On   POS_DISPOSALHEADER.DISPH_RECID = POS_DISPOSALDETAILS.DISPD_HRECID AND POS_DISPOSALHEADER.DISPH_LOCATION = POS_DISPOSALDETAILS.DISPD_LOCATION "
            strsql &= "AND POS_DISPOSALHEADER.DISPH_STATUS = POS_DISPOSALDETAILS.DISPD_STATUS "
            strsql &= "inner join STK04_Location STK04_Location On POS_DISPOSALHEADER.DISPH_LOCATION = STK04_Location.Loc_Code  "
            strsql &= "left join Com_Folio Com_Folio On STK04_Location.Loc_Folio = Com_Folio.Fo_serno  and  STK04_Location.Loc_FolioSub = Com_Folio.Fo_subno  "
            strsql &= "left join " & CStr(IIf(ClocHO = False, "pos_stkfil", "STK04_stkfil")) & " POS_STKFIL On POS_DISPOSALDETAILS.DISPD_STKRECID = POS_STKFIL.STK_RECID "
            strsql &= "  left join STK04_Measurements on stk_unit =Meas_Unit  "
            strsql &= "left join Com_UserInfo on DISPH_USTAMP = User_Code "
            strsql &= "LEFT JOIN stk04_color ON POS_DISPOSALDETAILS.DISPD_ColorCODE=color_code "

            Dim NbrCopies As String = "1"
            Dim rptParam(11, 1) As Object

            rptParam(0, 0) = "Title1"
            Select Case DisposalStatus
                Case "N", ""
                    rptParam(0, 1) = "'Disposal Transaction Slip'"
                Case "R"
                    rptParam(0, 1) = "'Reverse Disposal Slip'"
            End Select

            rptParam(3, 0) = "DecNo"
            rptParam(3, 1) = gbDecNo

            rptParam(1, 0) = "VATReg"
            rptParam(1, 1) = "'" & sVATreg & "'"
            rptParam(2, 0) = "FoReference" : rptParam(2, 1) = "'" & CStr(IIf(CRetVal = "1", "Barcode", "Reference")) & "'"
            Select Case frmForm.Name

                Case "frmStkDisposal"
                    PrintPODirectRep(frmForm, strsql, IIf(Laser, "rptStkDispLaser.rpt", "rptStkDisp.rpt").ToString, CInt(NbrCopies), rptParam) 'edited by GS on 12/07/16 (add iif)

                Case "frmReprintSlips"
                    If printdirect Then
                        PrintPODirectRep(frmForm, strsql, IIf(Laser, "rptStkDispLaser.rpt", "rptStkDisp.rpt").ToString, CInt(NbrCopies), rptParam, " ORDER BY DISPH_RECID, DISPD_STATUS") 'edited by GS on 12/07/16 (add iif)
                    Else
                        'Preview Query (fields should be included in report query having same names to apply filtration)
                        strsql1 = "Select distinct DISPH_RECID, DISPH_STATUS DISPD_STATUS,DISPH_Date,DISPH_DSTAMP,DISPH_REMARKS from Temp_POS_SALEDISPHEAD" & gbUser & " "
                        strsql1 &= "left join Com_UserInfo on DISPH_USTAMP = User_Code "

                        With frm
                            frm.BuildSql(strsql1, , , , , strsql.ToString)

                            .ColDesign("DISPH_RECID", 60, "Serial", , , , , , , , , frmPreviewGrid.Orders.Descending)
                            .ColDesign("DISPD_STATUS", 70, "Status", AlignHorzEnum.Center)
                            .ColDesign("DISPH_Date", 120, "Transaction Date", , , , , , , , , frmPreviewGrid.Orders.Descending, , , , "dd/MM/yyyy")
                            .ColDesign("DISPH_DSTAMP", 90, "Date Stamp", , , , , , , , , frmPreviewGrid.Orders.Descending, , , , "dd/MM/yyyy")
                            .ColDesign("DISPH_REMARKS", 430, "Remarks", AlignHorzEnum.Near)

                            .ShowDialog(strPrev, frmReprintSlips.Name, IIf(Laser, "rptStkDispLaser.rpt", "rptStkDisp.rpt").ToString, rptParam) 'edited by GS on 12/07/16 (add iif)
                        End With
                    End If
            End Select
        Catch ex As Exception
            MessageBox.Show("General Error : " & ex.Message, frmStkDisposal.Text, MessageBoxButtons.OK, MessageBoxIcon.Warning)
        Finally
            If Not frm Is Nothing Then frm = Nothing
            DropSqlObject("Temp_POS_SALEDISPHEAD" & gbUser, "TABLE")
            DropSqlObject("Temp_POS_SALEDISPDET" & gbUser, "TABLE")
            Cursor.Current = Cursors.Default
            Disposecontrols(cnsql, cmsql)
        End Try
    End Sub
    Public Sub PrintDeliveryOrder(OriginalForm As System.Windows.Forms.Form, Optional ResOrderRecID As Long = -1, Optional Temptable As String = "")

        Try
            Using FRM As New frmPrintOffer
                With FRM
                    .TempRecID = Temptable
                    .Print = False
                    .from = "Delivery"
                    .ShowDialog()
                End With
            End Using

        Catch ex As Exception
            Throw ex
        End Try
    End Sub
    Public Sub PrintReservationTransaction(OriginalForm As System.Windows.Forms.Form, Optional ResOrderRecID As Long = -1, Optional Temptable As String = "", Optional _UseNbrOfCopies As Boolean = False, Optional _PrintBeforeClose As Boolean = False)
        Dim sqlstat, strsqlRep As String
        Dim strsql As String
        Dim cmsql As SqlCommand
        Dim cnsql As SqlConnection
        Dim rptParam(2, 1) As Object
        Dim strPrev(,) As String
        Cursor.Current = Cursors.WaitCursor

        strPrev = Nothing

        strsql = "exec P_RO_InvRep '" & Cloc & "','" & CStr(IIf(ResOrderRecID = -1, "", ResOrderRecID)) & "','" & gbUser & "','" & gbApp & "','" & gbYear & "','" & gbCurr1 & "','" & gbCurr2 & "','" & Temptable & "','" & CRetVal & "'"
        Execute(strsql)

        sqlstat = " SELECT * "
        sqlstat = sqlstat + " From"
        sqlstat = sqlstat + " Temp_RO_Head" & Cloc & CStr(IIf(ResOrderRecID = -1, "0", ResOrderRecID)) & gbUser & " Temp_RO_Head inner join "
        sqlstat = sqlstat + " Temp_RO_Det" & Cloc & CStr(IIf(ResOrderRecID = -1, "0", ResOrderRecID)) & gbUser & " Temp_RO_Det On"
        sqlstat = sqlstat + " Temp_RO_Det.ROD_LOC = Temp_RO_Head.ROH_Loc AND"
        sqlstat = sqlstat + " Temp_RO_Det.ROD_hrecid = Temp_RO_Head.ROH_Recid"
        'christina @ 29/12/2015
        sqlstat = sqlstat + " LEFT JOIN dbo.Stk04_COLOR ON COALESCE(ROD_Color,'')=COALESCE(Color_Code,'')"
        'sqlstat = sqlstat + " Order By Temp_RO_Head.ROH_Recid Asc"

        Dim ViewName As String
        ViewName = "VPP_ResOrder" & "_" & gbUser


        strsqlRep = "if exists (select * FROM sysobjects where id = object_id(N'" & ViewName & "') "
        strsqlRep &= " and OBJECTPROPERTY(id, N'Isview') = 1) drop view " & ViewName
        cnsql = New SqlConnection(ConnectionString)
        cnsql.Open()
        cmsql = New SqlCommand(strsqlRep.ToString, cnsql)

        cmsql.ExecuteScalar()

        'create view
        strsqlRep = " Create View " & ViewName & " AS " & sqlstat
        cmsql.CommandText = strsqlRep
        cmsql.ExecuteScalar()

        strsqlRep = "SELECT * FROM " & ViewName
        ''   strsqlRep &= " order by stk_ref" 'added by GS on 20/11/14

        'added by GS on 18/03/16
        Dim strsql1 As String
        Dim NbrCopies As String
        If _UseNbrOfCopies Then

            '1 active prefix by location for reservation
            strsql1 = " Select STT_NBRCOPY"
            strsql1 &= " FROM pos_stocktransaction "
            strsql1 &= " Where STT_DOCTYPE1 = 'WR' "
            strsql1 &= " AND STT_LOCATION='" & Cloc & "'"
            strsql1 &= " AND STT_ACTIVE = 'Y' "
            NbrCopies = ExecuteScalar(strsql1)

        End If
        'GS on 18/03/16

        rptParam(0, 0) = "Decno" : rptParam(0, 1) = gbDecNo
        rptParam(1, 0) = "FoReference" : rptParam(1, 1) = "'" & CStr(IIf(CRetVal = "1", "Barcode", "Reference")) & "'"
        'added by GS on 19/03/16
        If _UseNbrOfCopies Then
            PreviewProcedure(strsqlRep, "", "rptResOrdOffer.rpt", OriginalForm, rptParam, CInt(NbrCopies), , , , , , , , , , _PrintBeforeClose)
        Else
            'GS on 19/03/16
            PreviewProcedure(strsqlRep, "", "rptResOrdOffer.rpt", OriginalForm, rptParam, , , , , , , , , , , _PrintBeforeClose)
        End If

    End Sub
    Public Sub PrintSlipTransAss(ByVal Recid As String, ByVal TransAssTran As String, ByVal ItmClass As String, ByVal Username As String, ByVal frmMe As Form, Optional ByVal TempTable As String = "", Optional ByVal Laser As Boolean = False, Optional ByVal production As Boolean = False, Optional ByVal PrintCost As Boolean = True, Optional Printdirect As Boolean = False) 'edited by GS on 12/07/16
        Dim sqlstat, strsql1 As String
        Dim frm As frmPreviewGrid
        Dim strPrev(,) As String
        Try
            Cursor.Current = Cursors.WaitCursor
            frm = New frmPreviewGrid

            'Report Query
            sqlstat = " SELECT DISTINCT "
            sqlstat &= " POS_TRANSPOSITIONDETAILS.TRSPD_STATUS as [D.Status],TRSPH_Date,POS_TRANSPOSITIONHEADER.TRSPH_RECID [H.ID],"
            sqlstat &= " POS_TRANSPOSITIONHEADER.TRSPH_STATUS as [H.Status], RIGHT(POS_TRANSPOSITIONHEADER.TRSPH_REMARKS,"
            sqlstat &= " LEN(POS_TRANSPOSITIONHEADER.TRSPH_REMARKS)-1) as [H.Remarks],"
            sqlstat &= " trspd_ExpiryDt,trspd_DetGrid ,"
            sqlstat &= " trspd_BatchInfo, Stk04_COLOR.Color_Reference Color_Desc ,stk_itmclass ItmClass ,"

            sqlstat &= " CASE WHEN POS_TRANSPOSITIONDETAILS.TRSPD_BARCODE='' OR POS_TRANSPOSITIONDETAILS.TRSPD_BARCODE IS NULL  THEN "
            sqlstat &= " POS_STKFIL.STK_Ref ELSE POS_TRANSPOSITIONDETAILS.TRSPD_BARCODE END as [Item.Ref] , "

            If CRetVal = "1" Then
                sqlstat &= " isnull((SELECT TOP 1 case when ltrim(dbar_barcode) = '' then null else dbar_barcode end dbar_barcode FROM dbo.STK04_DetBarcode WHERE dbar_stkid =POS_STKFIL.Stk_Recid"
                sqlstat &= " AND ISNULL(dbar_grid,'')=ISNULL(trspd_Grid,'')"
                sqlstat &= " AND ISNULL(dbar_Detgrid,'') =ISNULL(trspd_DetGrid,'')"
                sqlstat &= " AND ISNULL(dbar_color,'') =ISNULL(trspd_Color,'')"
                sqlstat &= " AND ISNULL(dbar_expdate,'') = ISNULL(trspd_ExpiryDt,'')"
                sqlstat &= " AND ISNULL(dbar_batch,'')=ISNULL(trspd_BatchInfo,'')),POS_STKFIL.STK_REF) "
            ElseIf CRetVal = "2" Then
                sqlstat &= " POS_STKFIL.STK_REF "
            ElseIf CRetVal = "3" Then
                sqlstat &= " ISNULL(POS_STKFIL.STK_SingleRef1,POS_STKFIL.STK_REF)  "
            ElseIf CRetVal = "4" Then
                sqlstat &= " ISNULL(POS_STKFIL.STK_SingleRef2,POS_STKFIL.STK_REF) "
            End If

            sqlstat &= " STK_Ref, "

            sqlstat &= " POS_STKFIL.STK_DESC as [Description],"
            sqlstat &= "    POS_STKFIL.STK_UNIT as Unit , " & CStr(IIf(Laser, "Meas_Desc [Unit Desc],", " ")) & " STK04_Location.Loc_Code as Loc, STK04_Location.Loc_Desc as Location , "
            sqlstat &= "  POS_TRANSPOSITIONDETAILS.TRSPD_QTY AS Qty , POS_TRANSPOSITIONDETAILS.TRSPD_Dstamp as Date "

            sqlstat &= " ,TRSPH_USTAMP, E_Name, TRSPH_DSTAMP" 'added by GS on 20/07/16

            'If Laser = True Then
            sqlstat &= " , TRSPD_UCostLC , TRSPD_UCostCC "
            'End If

            sqlstat &= " From     POS_TRANSPOSITIONDETAILS    "
            sqlstat &= " inner join POS_TRANSPOSITIONHEADER   ON   POS_TRANSPOSITIONDETAILS.TRSPD_LOCATION = POS_TRANSPOSITIONHEADER.TRSPH_LOCATION and POS_TRANSPOSITIONDETAILS.TRSPD_HRECID = POS_TRANSPOSITIONHEADER.TRSPH_RECID and POS_TRANSPOSITIONDETAILS.TRSPD_trsstatus = POS_TRANSPOSITIONHEADER.TRSPh_status "
            sqlstat &= " INNER JOIN  " & CStr(IIf(ClocHO = False, "pos_stkfil", "STK04_stkfil")) & " POS_STKFIL  on POS_TRANSPOSITIONDETAILS.TRSPD_STKRECID = POS_STKFIL.STK_RECID"
            If Laser Then sqlstat &= " INNER JOIN    STK04_Measurements  on Meas_Unit = POS_STKFIL.STK_UNIT "
            sqlstat &= " INNER JOIN  STK04_Location    on POS_TRANSPOSITIONHEADER.TRSPH_LOCATION = STK04_Location.Loc_Code"
            sqlstat &= " LEFT JOIN Stk04_COLOR on Color_Code = trspd_Color "

            'added by GS on 20/07/16
            sqlstat &= " LEFT join " & CStr(IIf(ClocHO = False, "POS_Employee", "STK04_Employee")) & " POS_EMPLOYEE on TRSPH_USTAMP = POS_EMPLOYEE.E_Code "
            'GS on 20/07/16

            Select Case frmMe.Name
                Case frmTransAssmTrs.Name
                    sqlstat &= " Where  POS_TRANSPOSITIONHEADER.TRSPH_RECID = " & Recid
                    If (TransAssTran = "Reverse Assembly" Or TransAssTran = "Reverse Transposition" Or production = True) Then
                        sqlstat &= " AND POS_TRANSPOSITIONHEADER.TRSPH_STATUS = 'R' AND POS_TRANSPOSITIONDETAILS.TRSPD_TRSSTATUS = 'R' "
                    Else
                        sqlstat &= " AND POS_TRANSPOSITIONHEADER.TRSPH_STATUS = 'N' AND POS_TRANSPOSITIONDETAILS.TRSPD_TRSSTATUS = 'N' "
                    End If
                    sqlstat &= " And POS_TRANSPOSITIONHEADER.TRSPH_LOCATION = '" & Cloc & "' "
                Case frmTransferStock.Name
                    sqlstat &= " Where  POS_TRANSPOSITIONHEADER.TRSPH_RECID = " & Recid
                    If (TransAssTran = "Reverse Assembly" Or TransAssTran = "Reverse Transposition" Or production = True) Then
                        sqlstat &= " AND POS_TRANSPOSITIONHEADER.TRSPH_STATUS = 'R' AND POS_TRANSPOSITIONDETAILS.TRSPD_TRSSTATUS = 'R' "
                    Else
                        sqlstat &= " AND POS_TRANSPOSITIONHEADER.TRSPH_STATUS = 'N' AND POS_TRANSPOSITIONDETAILS.TRSPD_TRSSTATUS = 'N' "
                    End If
                    sqlstat &= " And POS_TRANSPOSITIONHEADER.TRSPH_LOCATION = '" & Cloc & "' "
                    'Join with TempTable sent from form, conditions = current location,  checked records
                Case frmReprintSlips.Name
                    sqlstat &= "Left Join " & TempTable & " on POS_TRANSPOSITIONHEADER.TRSPH_RECID = [Serial] and POS_TRANSPOSITIONHEADER.TRSPH_STATUS = [Status] "
                    sqlstat &= "Where POS_TRANSPOSITIONHEADER.TRSPH_LOCATION = '" & Cloc & "' "
                    sqlstat &= "And [Chk] = 'True' "
            End Select

            Dim rptParam(10, 1) As Object
            rptParam(1, 0) = "Header"
            rptParam(1, 1) = "'" & TransAssTran.Replace("Production", "Production (Transposition Multiple In & Out)") & "'"
            'Select Case TransAssTran
            '    Case "Assembly"
            '        rptParam(1, 1) = "'Assembly Slip'"
            '    Case "Reverse Ass"
            '        rptParam(1, 1) = "'Reverse Assembly Slip'"
            '    Case "Transposition"
            '        rptParam(1, 1) = "'Transposition Slip'"
            '    Case "Reverse Trans"
            '        rptParam(1, 1) = "'Reverse Transposition Slip'"
            'End Select

            rptParam(3, 0) = "decno"
            rptParam(3, 1) = gbDecNo

            rptParam(4, 0) = "PrintedBy"
            rptParam(4, 1) = "'" & Username & "'"

            rptParam(5, 0) = "VATReg" : rptParam(5, 1) = "'VAT#: " & sVATreg & "'" 'added by GS on 20/07/16
            'If Laser = True Then
            rptParam(2, 0) = "PRound" : rptParam(2, 1) = "" & VatLCRound & ""
            rptParam(6, 0) = "ShowCost" : rptParam(6, 1) = "'" & IIf(PrintCost = True, "Y", "N").ToString & "'"
            rptParam(7, 0) = "gbcur1" : rptParam(7, 1) = "'" & gbCurr1 & "'"
            rptParam(8, 0) = "gbcu2" : rptParam(8, 1) = "'" & gbCurr2 & "'"
            'End If
            rptParam(9, 0) = "FoReference" : rptParam(9, 1) = "'" & CStr(IIf(CRetVal = "1", "Barcode", "Item Ref.")) & "'"
            rptParam(9, 0) = "Updated" : rptParam(9, 1) = "'Y'"

            Select Case frmMe.Name
                Case frmTransAssmTrs.Name
                    PrintPODirectRep(frmMe, sqlstat, IIf(Laser, "rptAssemblyRepLaser.rpt", "rptAssemblyRep.rpt").ToString, 1, rptParam) 'edited by GS on 12/07/16 (add iif)
                Case frmTransferStock.Name
                    PrintPODirectRep(frmMe, sqlstat, IIf(Laser, "rptAssemblyRepLaser.rpt", "rptAssemblyRep.rpt").ToString, 1, rptParam)
                Case frmReprintSlips.Name
                    If Printdirect Then
                        PrintPODirectRep(frmMe, sqlstat, IIf(Laser, "rptAssemblyRepLaser.rpt", "rptAssemblyRep.rpt").ToString, 1, rptParam)
                    Else
                        'Preview Query (fields should be included in report query having same names to apply filtration)
                        strsql1 = "Select Serial [H.ID], Date [TRSPH_Date], Status [H.Status], Remarks [H.Remarks] "
                        strsql1 &= "from " & TempTable & " "
                        strsql1 &= "Where [Chk] = 'True' "

                        With frm

                            frm.BuildSql(strsql1, , , , , sqlstat)

                            '.ColDesign("D.Status", -1, "D.Status", C1.Win.C1TrueDBGrid.AlignHorzEnum.Center, C1.Win.C1TrueDBGrid.AlignVertEnum.Top, , , , , True, , frmPreviewGrid.Orders.Ascending)
                            .ColDesign("H.ID", -1, "Serial No", C1.Win.C1TrueDBGrid.AlignHorzEnum.Center, C1.Win.C1TrueDBGrid.AlignVertEnum.Top, , , , , , , frmPreviewGrid.Orders.Descending)
                            .ColDesign("H.Status", -1, "H.Status", C1.Win.C1TrueDBGrid.AlignHorzEnum.Center, C1.Win.C1TrueDBGrid.AlignVertEnum.Top, , , , , , , , , , , , , , , , , False)
                            .ColDesign("H.Remarks", 200, "H.Remarks", C1.Win.C1TrueDBGrid.AlignHorzEnum.Near, C1.Win.C1TrueDBGrid.AlignVertEnum.Top, , , , , True)
                            .ColDesign("TRSPH_Date", 100, "Date", , , , , , , , , frmPreviewGrid.Orders.Descending, , , , "dd/MM/yyyy")
                            '.ColDesign("Item.Ref", -1, "Item.Ref", C1.Win.C1TrueDBGrid.AlignHorzEnum.Near, C1.Win.C1TrueDBGrid.AlignVertEnum.Top, , , , , True, , , , , , , , , , , , False)
                            '.ColDesign("Description", -1, "Description", C1.Win.C1TrueDBGrid.AlignHorzEnum.Near, C1.Win.C1TrueDBGrid.AlignVertEnum.Top, , , , , True)
                            '.ColDesign("Unit", -1, "Unit", C1.Win.C1TrueDBGrid.AlignHorzEnum.Center, C1.Win.C1TrueDBGrid.AlignVertEnum.Top, , , , , True, , , , , , , , , , , , False)
                            '.ColDesign("Loc", -1, "Loc", C1.Win.C1TrueDBGrid.AlignHorzEnum.Center, C1.Win.C1TrueDBGrid.AlignVertEnum.Top, , , , , True, , , , , , , , , , , , False)
                            '.ColDesign("Location", -1, "Location", C1.Win.C1TrueDBGrid.AlignHorzEnum.Near, C1.Win.C1TrueDBGrid.AlignVertEnum.Top, , , , , True, , , , , , , , , , , , False)
                            ' .ColDesign("D.Status", -1, "D.Status", C1.Win.C1TrueDBGrid.AlignHorzEnum.Center, C1.Win.C1TrueDBGrid.AlignVertEnum.Top, , , , , True, , , , , , , , , , , , False)
                            '.ColDesign("Qty", -1, "Qty", C1.Win.C1TrueDBGrid.AlignHorzEnum.Center, C1.Win.C1TrueDBGrid.AlignVertEnum.Top, , , , , True, , , , , , , , , , , , False)
                            'If UCase(ItmClass) = "E" Then
                            '    .ColDesign("ExpirySize", -1, "ExpiryDate", C1.Win.C1TrueDBGrid.AlignHorzEnum.Center, C1.Win.C1TrueDBGrid.AlignVertEnum.Top, , , , , True, , , , , , , , , , , , , "ExpirySize as [ExpiryDate]")
                            '    .ColDesign("LotColor", -1, "LotNo", C1.Win.C1TrueDBGrid.AlignHorzEnum.Center, C1.Win.C1TrueDBGrid.AlignVertEnum.Top, , , , , True, , , , , , , , , , , , , "LotColor as [LotNo]")
                            'ElseIf UCase(ItmClass) = "F" Then
                            '    .ColDesign("ExpirySize", -1, "Size", C1.Win.C1TrueDBGrid.AlignHorzEnum.Center, C1.Win.C1TrueDBGrid.AlignVertEnum.Top, , , , , True, , , , , , , , , , , , , "ExpirySize as [Size]")
                            '    .ColDesign("LotColor", -1, "Color", C1.Win.C1TrueDBGrid.AlignHorzEnum.Center, C1.Win.C1TrueDBGrid.AlignVertEnum.Top, , , , , True, , , , , , , , , , , , , "LotColor as [Color]")
                            'Else
                            '    .ColDesign("ExpirySize", -1, "Size", C1.Win.C1TrueDBGrid.AlignHorzEnum.Center, C1.Win.C1TrueDBGrid.AlignVertEnum.Top, , , , , True, False, , , , , , , , , , , False, "ExpirySize as [Size]")
                            '    .ColDesign("LotColor", -1, "Color", C1.Win.C1TrueDBGrid.AlignHorzEnum.Center, C1.Win.C1TrueDBGrid.AlignVertEnum.Top, , , , , True, False, , , , , , , , , , , False, "LotColor as [Color]")
                            'End If
                            '.ColDesign("ItmClass", -1, "ItmClass", C1.Win.C1TrueDBGrid.AlignHorzEnum.Center, C1.Win.C1TrueDBGrid.AlignVertEnum.Top, , , , , True, False, , , , , , , , , , , False, "LotColor as [Color]")

                            .ShowDialog(strPrev, frmMe.Name, IIf(Laser, "rptAssemblyRepLaser.rpt", "rptAssemblyRep.rpt").ToString, rptParam) 'edited by GS on 12/07/16 (add iif)

                        End With
                    End If
            End Select

        Catch ex As Exception
            MessageBox.Show("General Error : " & ex.Message, frmTransAssmTrs.Text, MessageBoxButtons.OK, MessageBoxIcon.Warning)
        Finally
            If Not frm Is Nothing Then frm = Nothing

            Cursor.Current = Cursors.Default
        End Try
    End Sub


    Private Function TestConnection() As Boolean
        Dim strsql As String
        Dim cnsql As SqlConnection
        Dim cmsql As SqlCommand
        Dim trnsql As SqlTransaction
        Dim RollBack As Boolean
        Try
            isConnected = True

            'remote location of the head office 
            GetRemoteLoc("00")
            cnsql = New SqlConnection(ConnectionString)
            cnsql.Open()
            trnsql = cnsql.BeginTransaction
            RollBack = True
            cmsql = New SqlCommand(strsql, cnsql, trnsql) : cmsql.CommandTimeout = 30
            'test if we are trying to connect to the same server 
            If Not dserv = gbServer Then linkServer = "getcurbalce" & gbUser & "" Else linkServer = ""
            strsql = "SET XACT_ABORT ON "
            cmsql.CommandText = strsql
            cmsql.ExecuteNonQuery()
            'Creating link server
            If Not dserv = gbServer Then CreateLinkServer(linkServer, dserv, "sa", OthSqlPwd)
            Try
                If linkServer <> "" Then
                    isConnected = ConnectedToServer("[" + linkServer + "].[" + OthDb + "]")
                Else

                    isConnected = ConnectedToServer("[" + OthDb + "]")
                End If

            Catch ex As Exception
                If RollBack Then trnsql.Rollback()
                RollBack = False
                MessageBox.Show("")
                Return False
                Exit Function
            End Try

            trnsql.Commit()
            RollBack = False
            Return isConnected
        Catch ex As Exception
            If RollBack Then trnsql.Rollback()
            Throw ex
        End Try
    End Function

    Public Sub TemClientBalnc(ByVal _me As Form, ByVal ReprintSlipTempTable As String, ByVal ClientNo As String, ByVal ClientSub As String,
                             ByVal curr As String, Optional Summ As Boolean = True, Optional ClientName As String = "")
        Dim strsql As String
        Dim frm As frmPreviewGrid
        Dim strPrev(,) As String
        Dim drsql As SqlDataReader
        Dim LinkServerAndLinkDB As String
        Dim TempAllTransactionCC As String
        Dim LocOrgCurr As String

        Try

            If ClocHO = False AndAlso TestConnection() = False Then
                '     MessageBox.Show("No Connection With HO ", _me.Text, MessageBoxButtons.OK, MessageBoxIcon.Information)
                ClientBalanceCnxTest = False
                ShowNotification(_me, "No Connection With HO ", 3000, 14, DevComponents.DotNetBar.eToastPosition.MiddleCenter)
                Exit Try
            End If

            ClientBalanceCnxTest = True

            If ClocHO = False Then
                If linkServer.Trim = "" Then
                    LinkServerAndLinkDB = CStr(IIf(ClocHO = False, "[" & OthDb & "].dbo.", ""))
                Else
                    LinkServerAndLinkDB = CStr(IIf(ClocHO = False, "[" & linkServer & "].[" & OthDb & "].dbo.", ""))
                End If
            Else
                LinkServerAndLinkDB = ""
            End If

            If Copercurr = gbCurr1 Then
                LocOrgCurr = gbCurr1
            Else
                LocOrgCurr = gbCurr2
            End If

            TempAllTransactionCC = "TempAllTransactionCC_" & gbUser
            pTempSale = "pTempSale_" & gbUser
            pTempSale1 = "pTempSale1_" & gbUser
            PTempSaleD = "PTempSaleD_" & gbUser
            PTempCsh = "PTempCsh_" & gbUser
            PTempStk = "PTempStk_" & gbUser
            ClinetBalnceSumtable = "ClinetBalnceSumtable_" & gbUser

            DropSqlObject(pTempSale, "Table")
            strsql = " SELECT  INVH_DATE ,"
            strsql &= " INVH_LOC ,INVH_RKS  ,"
            strsql &= " ISNULL( NULLIF(INVH_NETFC,0),INVH_NET) OrgNet,"
            strsql &= " ISNULL( NULLIF(INVH_CURRFC,''),INVH_CURR) OrgCurr,"
            strsql &= " dbo.convert5FC('" & gbApp & "', '" & gbComp & "',  " & gbYear & ", INVH_NET, INVH_CURR, '" & LocOrgCurr & "',"
            strsql &= " INVH_RATE,CONVERT(DATETIME, INVH_DATE)   ) invh_net,  INVH_CURR ,"
            strsql &= " INVH_RATE ,ISNULL( NULLIF(INVH_RATEFC,0),INVH_RATE) RateUSed , "
            strsql &= " INVH_DOCTYP1 ,"
            strsql &= " INVH_DOCPREFIX ,"
            strsql &= " INVH_SERNO ,"
            strsql &= " INVH_RECID ,"
            strsql &= " INVH_CLIENTSUP"
            strsql &= " INTO " & pTempSale & ""
            strsql &= " FROM    " & LinkServerAndLinkDB & "POS_SALEINVHEAD"
            strsql &= " inner join " & LinkServerAndLinkDB & "Stk04_Clients  on  INVH_CLIENTSUP = CL_RecID "

            If ReprintSlipTempTable.Trim = "" Then
                strsql &= "  where CL_Code = '" & ClientNo & "' and  CL_No = '" & ClientSub & "' "
            Else
                If _me.Name <> "frmRevSalesInv" Then
                    strsql &= " inner join " & ReprintSlipTempTable & "  on  CL_Code = ClientNo and  CL_No = ClientSubNo  	  AND Chk ='true'"
                Else
                    strsql &= " inner join " & ReprintSlipTempTable & "  on  CL_Code = clCode and  CL_No = Clno  	  AND Chk ='true'"
                End If
                strsql &= "  where 1 = 1  "
            End If

            strsql &= " and INVH_DOCTYP1 IN( 'IN' ,'CN','CV','RC','DV')"
            Execute(strsql)

            DropSqlObject(TempAllTransactionCC, "Table")
            strsql = " SELECT INVH_CLIENTSUP,INVH_RKS, INVH_DATE,INVH_LOC,INVH_DOCTYP1,INVH_DOCPREFIX,INVH_SERNO,"
            strsql &= " OrgNet ,OrgCurr ,"
            strsql &= " invh_net"
            strsql &= " , SUM(CASE WHEN CT_OPSIGN = '+' then 1 else -1 end  * CASE WHEN CT_SETTLTYPE <>'500' AND CT_SETTLTYPE <> '315' THEN"
            strsql &= " dbo.convert5FC('" & gbApp & "', '" & gbComp & "',  " & gbYear & ", CT_AmtOpCurr, OrgCurr, '" & LocOrgCurr & "',"
            strsql &= " RateUSed,CONVERT(DATETIME, INVH_DATE)   )"
            strsql &= " ELSE 0 END ) Paid ,"
            strsql &= " "
            strsql &= " invh_net - SUM(CASE WHEN CT_OPSIGN = '+' then 1 else -1 end  *  CASE WHEN CT_SETTLTYPE <>'500' AND CT_SETTLTYPE <> '315' THEN"
            strsql &= " dbo.convert5FC('" & gbApp & "', '" & gbComp & "',  " & gbYear & ", CT_AmtOpCurr, OrgCurr, '" & LocOrgCurr & "',"
            strsql &= " RateUSed,CONVERT(DATETIME, INVH_DATE)   )"
            strsql &= " ELSE 0 END ) Remain ,"

            strsql &= " invh_net - SUM(CASE WHEN CT_OPSIGN = '+' then 1 else -1 end  *  CASE WHEN CT_SETTLTYPE <>'500' AND CT_SETTLTYPE <> '315' AND CT_SETTLTYPE <> 'APY' THEN"
            strsql &= " dbo.convert5FC('" & gbApp & "', '" & gbComp & "',  " & gbYear & ", CT_AmtOpCurr, OrgCurr, '" & LocOrgCurr & "',"
            strsql &= " RateUSed,CONVERT(DATETIME, INVH_DATE)   )"
            strsql &= " ELSE 0 END ) Bal"

            strsql &= " INTO " & TempAllTransactionCC & " FROM " & pTempSale & ""
            strsql &= " INNER JOIN " & LinkServerAndLinkDB & "POS_CASHTRANSACTION ON CT_LOCATION =  invh_loc"
            strsql &= " AND CT_INVHEADER = INVH_RECID AND INVH_DOCTYP1 IN( 'IN' ,'CN')"
            strsql &= " GROUP BY INVH_CLIENTSUP,  INVH_DATE,INVH_LOC,INVH_DOCTYP1,INVH_DOCPREFIX,INVH_SERNO,OrgNet ,OrgCurr, invh_net,INVH_RKS"
            strsql &= " UNION all"
            strsql &= " SELECT INVH_CLIENTSUP,INVH_RKS, INVH_DATE,INVH_LOC,INVH_DOCTYP1,INVH_DOCPREFIX,INVH_SERNO,"
            strsql &= " OrgNet ,OrgCurr ,"
            strsql &= " invh_net"
            strsql &= " , 0 Paid ,"
            strsql &= " "
            strsql &= " CASE WHEN INVH_DOCTYP1 in ('CV','DV') then 1 else -1 end  *  invh_net  Remain,"
            strsql &= " CASE WHEN INVH_DOCTYP1 in ('CV','DV') then 1 else -1 end  *  invh_net  Bal"
            strsql &= " FROM " & pTempSale & ""
            strsql &= " INNER JOIN " & LinkServerAndLinkDB & "POS_CASHTRANSACTION ON CT_LOCATION =  invh_loc"
            strsql &= " AND CT_INVHEADER = INVH_RECID AND INVH_DOCTYP1 IN( 'RC' ,'CV' , 'DV' )"
            strsql &= " GROUP BY INVH_CLIENTSUP,  INVH_DATE,INVH_LOC,INVH_DOCTYP1,INVH_DOCPREFIX,INVH_SERNO,OrgNet ,OrgCurr, invh_net,INVH_RKS"
            strsql &= " UNION all"

            strsql &= " SELECT CL_RecID,'BBF', CL_BBFDate ,NULL ,NULL ,NULL ,NULL ,NULL ,NULL,NULL,NULL,"
            strsql &= "  dbo.convert5('" & gbApp & "', '" & gbComp & "',  " & gbYear & ", isnull(CL_BBFSaleCC,0) - isnull(CL_BBFCreditCC,0), '" & gbCurr2 & "', '" & LocOrgCurr & "',"
            strsql &= " CONVERT(DATETIME, CL_BBFDate))"
            strsql &= "  + dbo.convert5('" & gbApp & "', '" & gbComp & "',  " & gbYear & ", isnull(CL_BBFSaleLC,0) - isnull(CL_BBFCreditLC,0), '" & gbCurr1 & "', '" & LocOrgCurr & "',"
            strsql &= " CONVERT(DATETIME, CL_BBFDate)),"

            strsql &= "  dbo.convert5('" & gbApp & "', '" & gbComp & "',  " & gbYear & ", isnull(CL_BBFSaleCC,0) - isnull(CL_BBFCreditCC,0), '" & gbCurr2 & "', '" & LocOrgCurr & "',"
            strsql &= " CONVERT(DATETIME, CL_BBFDate))"
            strsql &= "  + dbo.convert5('" & gbApp & "', '" & gbComp & "',  " & gbYear & ", isnull(CL_BBFSaleLC,0) - isnull(CL_BBFCreditLC,0), '" & gbCurr1 & "', '" & LocOrgCurr & "',"
            strsql &= " CONVERT(DATETIME, CL_BBFDate))"
            strsql &= " FROM   dbo.Stk04_Clients"
            If ReprintSlipTempTable.Trim = "" Then
                strsql &= "  where CL_Code = '" & ClientNo & "' and  CL_No = '" & ClientSub & "' "
            Else
                If _me.Name <> "frmRevSalesInv" Then
                    strsql &= " inner join " & ReprintSlipTempTable & "  on  CL_Code = ClientNo and  CL_No = ClientSubNo  	  AND Chk ='true'"
                Else
                    strsql &= " inner join " & ReprintSlipTempTable & "  on  CL_Code = clCode and  CL_No = Clno  	  AND Chk ='true'"
                End If
                strsql &= " where 1=1 "
            End If
            strsql &= "  and  CL_BBFDate is not NULL "
            Execute(strsql)


            If Summ = True Then
                DropSqlObject(ClinetBalnceSumtable, "Table")
                strsql = " SELECT  INVH_CLIENTSUP Client , dbo.convert5('" & gbApp & "', '" & gbComp & "',  " & gbYear & ",SUM(Bal),'" & LocOrgCurr & "','" & gbCurr2 & "','" & Format(CDate(WorkingDay), "MM/dd/yyyy") & "')  BalanceCC"
                strsql &= " , dbo.convert5('" & gbApp & "', '" & gbComp & "',  " & gbYear & ",SUM(Bal),'" & LocOrgCurr & "','" & gbCurr1 & "','" & Format(CDate(WorkingDay), "MM/dd/yyyy") & "') BalanceLC"
                strsql &= "  into " & ClinetBalnceSumtable & " FROM  " & TempAllTransactionCC & ""
                strsql &= " GROUP BY INVH_CLIENTSUP"

                Execute(strsql)

            Else
                strsql = " SELECT  INVH_DATE [Date] ,"
                strsql &= " INVH_LOC [Loc.Code] ,"
                strsql &= " Loc_Desc [Loc.Name] ,"
                strsql &= " INVH_DOCTYP1 [Doc.Type] ,"
                strsql &= " INVH_DOCPREFIX [Doc.Prefix] ,"
                strsql &= " INVH_SERNO [Serno] ,"
                'Rita M @ 16.01.2019
                strsql &= " stt_desc [Doc. Description],"
                strsql &= " INVH_RKS  [Remarks],"
                strsql &= " OrgNet [Org.Net] ,"
                strsql &= " OrgCurr [Org.Curr] ,"
                If Copercurr = gbCurr1 Then

                    strsql &= " Round((dbo.convert5('" & gbApp & "', '" & gbComp & "',  " & gbYear & ",( invh_net ), '" & LocOrgCurr & "', '" & IIf(Copercurr = "FC", gbCurr2, Copercurr).ToString & "',"
                    strsql &= " CONVERT(DATE , GETDATE()))),0)" ',1)"
                    strsql &= "  [Net],"
                    strsql &= "   "
                    strsql &= " Round((dbo.convert5('" & gbApp & "', '" & gbComp & "',  " & gbYear & ",( Paid ), '" & LocOrgCurr & "', '" & IIf(Copercurr = "FC", gbCurr2, Copercurr).ToString & "',"
                    strsql &= " CONVERT(DATE , GETDATE()))),0)" ',1)"
                    strsql &= " [Paid],"
                    strsql &= "   "
                    strsql &= " Round((dbo.convert5('" & gbApp & "', '" & gbComp & "',  " & gbYear & ",( Remain ), '" & LocOrgCurr & "', '" & IIf(Copercurr = "FC", gbCurr2, Copercurr).ToString & "',"
                    strsql &= " CONVERT(DATE , GETDATE()))),0)" ',1)"
                    strsql &= " [Remain],"
                    strsql &= " SUM(Round(dbo.convert5('" & gbApp & "', '" & gbComp & "',  " & gbYear & ",( Bal ), '" & LocOrgCurr & "', '" & IIf(Copercurr = "FC", gbCurr2, Copercurr).ToString & "',"
                    strsql &= " CONVERT(DATE , GETDATE())),0)) OVER ( ORDER BY INVH_DATE , INVH_LOC , INVH_DOCTYP1 , INVH_DOCPREFIX , INVH_SERNO ) Balance"
                Else
                    strsql &= " (dbo.convert5('" & gbApp & "', '" & gbComp & "',  " & gbYear & ",( invh_net ), '" & LocOrgCurr & "', '" & IIf(Copercurr = "FC", gbCurr2, Copercurr).ToString & "',"
                    strsql &= " CONVERT(DATE , GETDATE())))"
                    strsql &= "  [Net],"
                    strsql &= "   "
                    strsql &= " (dbo.convert5('" & gbApp & "', '" & gbComp & "',  " & gbYear & ",( Paid ), '" & LocOrgCurr & "', '" & IIf(Copercurr = "FC", gbCurr2, Copercurr).ToString & "',"
                    strsql &= " CONVERT(DATE , GETDATE())))"
                    strsql &= " [Paid],"
                    strsql &= "   "
                    strsql &= " (dbo.convert5('" & gbApp & "', '" & gbComp & "',  " & gbYear & ",( Remain ), '" & LocOrgCurr & "', '" & IIf(Copercurr = "FC", gbCurr2, Copercurr).ToString & "',"
                    strsql &= " CONVERT(DATE , GETDATE())))"
                    strsql &= " [Remain],"
                    strsql &= " SUM(dbo.convert5('" & gbApp & "', '" & gbComp & "',  " & gbYear & ",( Bal ), '" & LocOrgCurr & "', '" & IIf(Copercurr = "FC", gbCurr2, Copercurr).ToString & "',"
                    strsql &= " CONVERT(DATE , GETDATE()))) OVER ( ORDER BY INVH_DATE , INVH_LOC , INVH_DOCTYP1 , INVH_DOCPREFIX , INVH_SERNO ) Balance"
                End If

                strsql &= " , case when INVH_DOCTYP1 in ('IN', 'CN') " & IIf(ClocHO, "", " and INVH_LOC = '" & Cloc & "' ").ToString & " then 'View' else '' End [Details] "

                strsql &= " FROM    " & TempAllTransactionCC & ""
                strsql &= " LEFT JOIN dbo.STK04_Location ON Loc_Code = INVH_LOC "
                'Rita M @ 16.01.2019
                strsql &= " LEFT JOIN pos_stocktransaction ON INVH_LOC = STT_LOCATION AND INVH_DOCTYP1 = STT_DOCTYPE1 AND INVH_DOCPREFIX = STT_PREFIX"
                'strsql &= " ORDER BY INVH_DATE ,"
                'strsql &= " INVH_LOC ,"
                'strsql &= " INVH_DOCTYP1 ,"
                'strsql &= " INVH_DOCPREFIX ,"
                'strsql &= " INVH_SERNO"

                Using frmStat As New frmClientStatement

                    With frmStat
                        .lblClient.Text = ClientNo & " / " & ClientSub & " - " & ClientName
                        .lcClCode = ClientNo
                        .lcClNo = ClientSub
                        .strQuery = strsql
                        .stLocOrgCurr = LocOrgCurr
                        .ShowDialog()
                    End With

                End Using

                '   Using Myfrm As New frmPreviewGrid
                '       With Myfrm
                '           .BuildSql(strsql, , , , )
                '           If ClocHO = False Then
                '
                '               .lblPsNote.Text = "N.B: Report Includes Only Data Synchronized with HO."
                '           End If
                '
                '           .ColDesign("Date", -1, "Inv. Date", AlignHorzEnum.Center, , , , , , , , frmPreviewGrid.Orders.Ascending, , , , "dd/MM/yyyy", , , , , , , )
                '           .ColDesign("Doc.Type", -1, "Doc Type", C1.Win.C1TrueDBGrid.AlignHorzEnum.Center, , , , , , , , , , , , , , , , , , , )
                '           .ColDesign("Doc.Prefix", -1, "Doc Pref", C1.Win.C1TrueDBGrid.AlignHorzEnum.Center, , , , , , , , , , , , , , , , , , , )
                '           .ColDesign("Serno", -1, "SerNo", C1.Win.C1TrueDBGrid.AlignHorzEnum.Center, , , , , , , , , , , , , , , , , , , )
                '           .ColDesign("Doc. Description", 250, "Doc Description", C1.Win.C1TrueDBGrid.AlignHorzEnum.Near, , , , , , , , , , , , , , , , , , , )
                '           .ColDesign("Loc.Code", -1, "Loc", C1.Win.C1TrueDBGrid.AlignHorzEnum.Center, , , , , , , , , , , , , , , , , , , )
                '           .ColDesign("Remaks", 250, "Remark", C1.Win.C1TrueDBGrid.AlignHorzEnum.Near, , , , , , , , , , , , , , , , , , , )
                '           .ColDesign("Org.Net", -1, "Org.Net", C1.Win.C1TrueDBGrid.AlignHorzEnum.General, , , , , , , , , , , , "#,##0.000", , , , , , , )
                '           .ColDesign("Org.Curr", -1, "Org.Curr", C1.Win.C1TrueDBGrid.AlignHorzEnum.Near, , , , , , , , , , , , , , , , , , , )
                '
                '           .ColDesign("Net", -1, "Net " & IIf(Copercurr = "FC", gbCurr2, Copercurr).ToString & "", C1.Win.C1TrueDBGrid.AlignHorzEnum.General, , , , , , , , , , , , CurrFormat(LocOrgCurr), , , , , , , )
                '           .ColDesign("Paid", -1, "Paid " & IIf(Copercurr = "FC", gbCurr2, Copercurr).ToString & "", C1.Win.C1TrueDBGrid.AlignHorzEnum.General, , , , , , , , , , , , CurrFormat(LocOrgCurr), , , , , , , )
                '           .ColDesign("Remain", -1, "Remain " & IIf(Copercurr = "FC", gbCurr2, Copercurr).ToString & "", C1.Win.C1TrueDBGrid.AlignHorzEnum.General, , , , , , , , , , , , CurrFormat(LocOrgCurr), , , , , , , )
                '           .ColDesign("Balance", -1, "Balance " & IIf(Copercurr = "FC", gbCurr2, Copercurr).ToString & "", C1.Win.C1TrueDBGrid.AlignHorzEnum.General, , , , , , , , , , , , CurrFormat(LocOrgCurr), , , , , , , )
                '
                '
                '           .ShowDialog(strPrev, _me.Name, "", Nothing, , , , , , , , , , False, , , )
                '       End With
                '
                '   End Using

            End If

            Exit Try


            DropSqlObject(ClinetBalnceSumtable, "TABLE")
            DropSqlObject(pTempSale, "TABLE")
            DropSqlObject(pTempSale1, "TABLE")
            DropSqlObject(PTempSaleD, "TABLE")
            DropSqlObject(PTempCsh, "TABLE")
            DropSqlObject(PTempStk, "TABLE")
            DropSqlObject("ClnStatAcc" & gbUser, "TABLE")
            DropSqlObject("CreditInv" & gbUser, "TABLE")
            DropSqlObject("ClnStat" & gbUser, "TABLE")
            DropSqlObject("ClientStatUnion" & gbUser, "TABLE")
            DropSqlObject("ClnStatLC" & gbUser, "TABLE")

            strsql = " CREATE Table ClnStatAcc" & gbUser & " ("
            ' strsql &= " [Chk] [varchar](5) NULL ,"
            strsql &= " Client numeric(18,0) NOT NULL," 'ID
            strsql &= " Clntype varchar(3) NOT NULL,"
            strsql &= " [Cl. Type Name] [nvarchar] (30) NOT NULL,"
            strsql &= " Code [numeric] (6, 0) NULL,"
            strsql &= " Code1 [smallint] NOT NULL,"

            strsql = strsql + "BBFDate datetime"
            strsql = strsql + ",BBFValuelc Numeric(28,3)"
            strsql = strsql + ",BBFValuecc Numeric(28,3)"
            strsql = strsql + ",CFWLC Numeric(28,3)"
            strsql = strsql + ",CFWCC Numeric(28,3),"
            strsql = strsql + " Address nvarchar(250) collate SQL_Latin1_General_CP1256_CI_AS,"
            strsql = strsql + " totalpurchCC DECIMAL(28,3),totpaidCC DECIMAL (28,3),totalCRCC DECIMAL (28,3),"
            strsql = strsql + " totalpurchLC DECIMAL(28,3),totpaidLC DECIMAL (28,3),totalCRLC DECIMAL (28,3)"
            strsql &= " ,CONSTRAINT  PK_ClnStatAcc" & gbUser
            strsql &= " PRIMARY KEY clustered (Client) "
            strsql &= ") ON [PRIMARY]"
            'strsql = strsql + " )"

            Execute(strsql)


            strsql = " insert into ClnStatAcc" & gbUser
            strsql &= " Select distinct Cl_recid, CL_Type, ClTp_Desc, cl_code, cl_no,"
            strsql &= " CL_BBFDate, "
            strsql = strsql + " Isnull(CL_BBFsaleLC,0) as CL_BBFsaleLC,Isnull(CL_BBFsaleCC,0) as CL_BBFsaleCC, "
            strsql = strsql + " 0,0,fo_addres,0,0,0,0,0,0 "
            strsql = strsql & " from stk04_clients"
            strsql &= " LEFT JOIN " & LinkServerAndLinkDB & "Stk04_ClientType ON CL_Type = ClTp_Code"
            strsql &= " inner join " & LinkServerAndLinkDB & "com_folio on cl_foliono= fo_serno and cl_foliosubno = fo_subno "
            strsql &= " left JOIN " & LinkServerAndLinkDB & "STk04_ClAccounts ON CL_RecID = CA_CLRecid AND CA_Comp = '" & gbComp & "'"
            If ReprintSlipTempTable.Trim = "" Then
                strsql &= "  where CL_Code = '" & ClientNo & "' and  CL_No = '" & ClientSub & "' "
                strsql &= $" AND NOT EXISTS ( SELECT 1
                                                FROM   STK04_ClientExcludedLocations
                                                WHERE  CEL_ClientRecid = CL_RecID
                                                       AND CEL_LocationCode = '{Cloc}' )"
                If ShowCltLinkedToAcc Then strsql &= " and ( CA_cLaC IS NOT NULL OR (CL_Code = " & DefCCcode & " AND ISNULL(CA_Comp,'" & gbComp & "') = '" & gbComp & "') ) "
            Else
                If _me.Name <> "frmRevSalesInv" Then
                    strsql &= " inner join " & ReprintSlipTempTable & "  on  CL_Code = ClientNo and  CL_No = ClientSubNo  	  AND Chk ='true'"
                Else
                    strsql &= " inner join " & ReprintSlipTempTable & "  on  CL_Code = clCode and  CL_No = Clno  	  AND Chk ='true'"
                End If

                 strsql &= $" where NOT EXISTS ( SELECT 1
                                                FROM   STK04_ClientExcludedLocations
                                                WHERE  CEL_ClientRecid = CL_RecID
                                                       AND CEL_LocationCode = '{Cloc}' )"
                If ShowCltLinkedToAcc Then strsql &= " AND ( CA_cLaC IS NOT NULL OR (CL_Code = " & DefCCcode & " AND ISNULL(CA_Comp,'" & gbComp & "') = '" & gbComp & "') ) "
            End If

            Execute(strsql)



            strsql = "Select  t1.* Into " & pTempSale & " From  " & LinkServerAndLinkDB & "Pos_saleinvhead as t1 "
            strsql = strsql + " Inner Join ClnStatAcc" & gbUser & " On Client = invh_clientsup"
            strsql = strsql + " Where invh_Doctyp1 in ('IN','CN','RC','CV','DV')"
            strsql = strsql + " AND invh_date  >= COALESCE(BBFDate,'01/01/1900')"
            strsql = strsql + " AND invh_date  <= '" & Format(CDate(WorkingDay), "MM/dd/yyyy") & "'"
            Execute(strsql)

            '  ..Sales inv. header 'RCN
            strsql = "select  * into " & pTempSale1 & "  From  " & pTempSale & " where invh_Doctyp1 ='RC' and Invh_Docprefix='N'"
            Execute(strsql)

            strsql = "Delete From  " & pTempSale & " where invh_Doctyp1 ='RC' and Invh_Docprefix='N'"
            Execute(strsql)


            '..Sales inv. Details
            strsql = "Select  t1.* Into " & PTempSaleD & " From " & pTempSale & " inner join  Pos_saleinvDet as t1"
            strsql = strsql + " On Invh_loc = invd_loc and invh_recid  = invd_headid "
            Execute(strsql)

            '..Cash Transaction
            strsql = "Select  t1.*,Stk04_SETTLEMENTTYPE.ST_Desc "
            strsql = strsql + " ,INVH_CURR " 'added by GS on 26/02/14 to compare it (before report) with CT_CURRCODE
            strsql = strsql + " Into " & PTempCsh & " From " & pTempSale & " inner join  " & LinkServerAndLinkDB & "Pos_CashTransaction as t1"
            strsql = strsql + " On Invh_loc = ct_location and invh_recid  = ct_invheader "
            strsql = strsql + " inner join Stk04_SETTLEMENTTYPE Stk04_SETTLEMENTTYPE On "
            strsql = strsql + " CT_LOCATION = Stk04_SETTLEMENTTYPE.ST_Loc AND"
            strsql = strsql + " CT_SETTLTYPE = Stk04_SETTLEMENTTYPE.ST_CODE "
            Execute(strsql)

            '..Stock  File
            strsql = " Select distinct Pos_stkfil.* into  " & PTempStk & " FRom "
            If ClocHO = True Then
                strsql = strsql + " stk04_stkFil "
            Else
                strsql = strsql + " pos_stkFil "
            End If

            strsql = strsql + " Pos_stkFil Inner join " & PTempSaleD & " On Stk_recid  =  Invd_stkid  "
            Execute(strsql)


            strsql = "CreditInv" + gbUser
            Execute("exec P_DropTempTable " & strsql)

            'Create table      
            strsql = "CREATE Table CreditInv" + gbUser + " (INVH_CLIENTSUP numeric(18, 0), CT_LOCATION VARCHAR(2),"
            strsql &= " CT_INVHEADER NUMERIC(28,0),CT_INVNBR varchar(11),"
            strsql &= "TOTCREDIT NUMERIC(30,3),TOTALPAID Numeric(30,3)) "
            Execute(strsql)


            strsql = "INSERT INTO CreditInv" + gbUser
            strsql &= " Select INVH_CLIENTSUP, CT_LOCATION , CT_INVHEADER,CT_INVNBR ,"
            strsql &= " isnull(sum(CASE WHEN CT_OPSIGN = '+' then 1 else -1 end * "
            strsql &= " case when (CT_SETTLTYPE = '500' or CT_SETTLTYPE='315' "
            strsql &= " or CT_SETTLTYPE = (case when INVH_DocTyp1='IN' and INVH_DocPrefix='V' then 'APY' else '' end ) )  "
            strsql &= " then  1 else 0 end *  CT_amtOPCURr),0) as TotCredit, "
            strsql &= " isnull(sum(CASE WHEN CT_OPSIGN = '+' then 1 else -1 end  * "
            strsql &= " case when (CT_SETTLTYPE <> '500' and CT_SETTLTYPE <> '315' "
            strsql &= " and CT_SETTLTYPE<>(case when INVH_DocTyp1='IN' and INVH_DocPrefix='V' then 'APY' else '' end ) ) "
            strsql &= " then  1 else 0 end *  CT_amtOPCURr ),0) "
            strsql &= " FROM " & PTempCsh & " inner join  " & pTempSale & " on   "   ' TempSales" + gbUser + " on "
            strsql &= " invh_loc = ct_location And invh_recid = ct_invheader "
            strsql &= " WHERE CT_Recid = 'T' and substring(CT_INVNBR,1,3) <>'RCN' "

            strsql &= " AND invh_date  <= '" + Format(CDate(WorkingDay), "MM/dd/yyyy") + "'"
            strsql &= " GROUP BY INVH_CLIENTSUP, CT_LOCATION, CT_INVHEADER, CT_INVNBR "
            Execute(strsql)

            'create view creditinfo      
            strsql = "if exists (select * from sysobjects where id = object_id('CreditInfo" + gbUser + "') and OBJECTPROPERTY(id, 'IsView') = 1)   drop view CreditInfo" + gbUser
            Execute(strsql)

            strsql = "CREATE VIEW dbo.CreditInfo" + gbUser
            strsql &= " AS SELECT POS_SALEINVHEAD.*,CreditInv.TOTCREDIT AS totcredit,CreditInv.TOTALPAID AS totalPaid "
            strsql &= " FROM  CreditInv" + gbUser + " CreditInv INNER JOIN "
            strsql &= " " & pTempSale & "  POS_SALEINVHEAD ON CreditInv.CT_LOCATION = POS_SALEINVHEAD.INVH_LOC AND "

            strsql &= " CreditInv.INVH_CLIENTSUP = POS_SALEINVHEAD.INVH_CLIENTSUP AND " '''

            strsql &= " CreditInv.CT_INVHEADER = POS_SALEINVHEAD.INVH_RECID where 1= 1 "
            strsql &= " AND invh_date  <= '" + Format(CDate(WorkingDay), "MM/dd/yyyy") + "'"

            Execute(strsql)


            Execute("update creditinv" & gbUser & " set totcredit = 0 where substring(ct_invnbr,1,2) in ('DV','CV')")



            strsql = "IF EXISTS (SELECT name FROM sysobjects WHERE type = 'U' AND name = 'ClientStatUnion" + gbUser + "') "
            strsql &= "DROP TABLE ClientStatUnion" + gbUser
            Execute(strsql)

            strsql = "IF EXISTS (SELECT name FROM sysobjects WHERE type = 'U' AND name = 'ClientStat" + gbUser + "') "
            strsql &= "DROP TABLE ClientStat" + gbUser
            Execute(strsql)

            strsql = "IF EXISTS (SELECT name FROM sysobjects WHERE type = 'U' AND name = 'ClnStatLC" + gbUser + "') "
            strsql &= " DROP TABLE ClnStatLC" + gbUser
            Execute(strsql)

            'Create Table    
            strsql = "CREATE TABLE ClientStatUnion" + gbUser + " (CLIENT INTEGER ,CURR VARCHAR (4),cocode varchar(2) )"
            Execute(strsql)


            strsql = "INSERT INTO ClientStatUnion" + gbUser + " SELECT distinct invh_ClientSup,invH_CURR,'" + gbComp + "' FROM "
            strsql &= " " & pTempSale & ""

            Execute(strsql)


            'Create the statement in CC    
            strsql = "Update ClnstatAcc" + gbUser
            strsql &= " set  totalpurchCC = TOt1, "
            strsql &= " totpaidCC = TOt2,"
            strsql &= " TotalCRCC = TOt3"




            strsql &= " From ("
            strsql &= " SELECT isnull(CLIENT,0) as CLIENT,Isnull(sum(invh_Net * case when invh_Doctyp1 ='IN' Or INVH_Doctyp1='CN' then 1 else 0 end ),0) as Tot1 , "
            strsql &= " isnull(sum(TotalPaid),0) as Tot2,isnull(sum(Totcredit),0)  as Tot3 "
            strsql &= " FROM ClientStatUnion" + gbUser + " LEFT OUTER JOIN  "


            strsql &= " (SELECT  dbo.convert5FC('" & gbApp & "','" & gbComp & "'," & gbYear & ",invh_Net ,invH_CURR,'" & gbCurr2 & "',INVH_RATE,'" & Format(CDate(WorkingDay), "MM/dd/yyyy") & "') invh_Net,"
            strsql &= " dbo.convert5FC('" & gbApp & "','" & gbComp & " '," & gbYear & ",TotalPaid ,invH_CURR,'" & gbCurr2 & "',INVH_RATE,'" & Format(CDate(WorkingDay), "MM/dd/yyyy") & "') TotalPaid   ,"
            strsql &= " dbo.convert5FC('" & gbApp & "','" & gbComp & " '," & gbYear & ",Totcredit ,invH_CURR,'" & gbCurr2 & "',INVH_RATE,'" & Format(CDate(WorkingDay), "MM/dd/yyyy") & "') Totcredit   ,"
            strsql &= " invH_CLIENTsup ,"
            strsql &= " invh_Doctyp1 ,"
            strsql &= " invH_CURR"
            strsql &= " FROM     CreditInfo" + gbUser + " )  as CreditInfo"


            ' strsql &= " (select  * from CreditInfo" + gbUser + " /*where invH_CURR='" + gbCurr2 + "'*/ ) as CreditInfo "



            strsql &= " ON  CreditInfo.invH_CURR = CURR AND  CreditInfo.invH_CLIENTsup = CLIENT "
            strsql &= " Group by isnull(CLIENT,0) ) as t1 "

            strsql &= " INNER JOIN ClnstatAcc" + gbUser + " ON t1.CLIENT = ClnStatAcc" + gbUser + ".Client" '''
            ' strsql &= " Where ClnstatAcc" + gbUser + ".Chk = 'True'"

            Execute(strsql)

            'Create the statement in LC    
            strsql = "Update ClnstatAcc" + gbUser
            strsql &= " set  totalpurchLC = TOt1, "
            strsql &= " totpaidLC = TOt2, "
            strsql &= " TotalCRLC = TOt3 "
            strsql &= " From ("
            strsql &= " SELECT isnull(CLIENT,0) as CLIENT,Isnull(sum(invh_Net * case when invh_Doctyp1 ='IN' Or INVH_Doctyp1='CN' then 1 else 0 end ),0) as Tot1 , "
            strsql &= " isnull(sum(TotalPaid),0) as Tot2,isnull(sum(Totcredit),0)  as Tot3 "


            strsql &= " FROM ClientStatUnion" + gbUser + " LEFT OUTER JOIN "


            'strsql &= " (select  * from CreditInfo" + gbUser + " where invH_CURR='" + gbCurr1 + "' ) "
            strsql &= " (SELECT  dbo.convert5FC('" & gbApp & "','" & gbComp & "'," & gbYear & ",invh_Net ,invH_CURR,'" & gbCurr1 & "',INVH_RATE,'" & Format(CDate(WorkingDay), "MM/dd/yyyy") & "') invh_Net,"
            strsql &= " dbo.convert5FC('" & gbApp & "','" & gbComp & " '," & gbYear & ",TotalPaid ,invH_CURR,'" & gbCurr1 & "',INVH_RATE,'" & Format(CDate(WorkingDay), "MM/dd/yyyy") & "') TotalPaid   ,"
            strsql &= " dbo.convert5FC('" & gbApp & "','" & gbComp & " '," & gbYear & ",Totcredit ,invH_CURR,'" & gbCurr1 & "',INVH_RATE,'" & Format(CDate(WorkingDay), "MM/dd/yyyy") & "') Totcredit   ,"
            strsql &= " invH_CLIENTsup ,"
            strsql &= " invh_Doctyp1 ,"
            strsql &= " invH_CURR"
            strsql &= " FROM     CreditInfo" + gbUser + "  )  as CreditInfo "



            strsql &= " ON  CreditInfo.invH_CURR = CURR AND  CreditInfo.invH_CLIENTsup = CLIENT "
            strsql &= " Group by isnull(CLIENT,0) ) as t1 "

            strsql &= " INNER JOIN ClnstatAcc" + gbUser + " ON t1.CLIENT = ClnStatAcc" + gbUser + ".Client" '''
            '  strsql &= " Where ClnstatAcc" + gbUser + ".Chk = 'True'"

            Execute(strsql)
            DropSqlObject("ClientStatAcc" & gbUser, "TABLE")



            strsql = " SELECT loc_code,Loc_cocode, Loc_Desc,"
            strsql = strsql + " CreditInfo.INVH_LOC, CreditInfo.INVH_RECID, CreditInfo.INVH_DOCTYP1, CreditInfo.INVH_DOCPREFIX,"
            strsql = strsql + " CreditInfo.INVH_SERNO, CreditInfo.INVH_DATE as INVH_DATE, CreditInfo.INVH_CLIENTSUP,"
            strsql = strsql + " isnull(CreditInfo.INVH_RKS,'') as invh_rks, CreditInfo.INVH_CURR, CreditInfo.INVH_NET,CreditInfo.totcredit,"
            strsql = strsql + " POS_saleinvdet.invD_QTY AS invd_QTY,"
            strsql = strsql + " POS_saleinvdet.invd_Uprice AS invD_Uprice,"
            strsql = strsql + " POS_saleinvdet.invd_DISC AS invD_DISC,"
            strsql = strsql + " POS_saleinvdet.invD_NET AS invD_NET,"
            strsql &= " POS_saleinvdet.INVD_Color AS INVD_Color, POS_saleinvdet.INVD_Grid AS INVD_Grid, POS_saleinvdet.INVD_DetGrid AS INVD_DetGrid,"
            strsql &= " POS_saleinvdet.INVD_ExpDate AS INVD_ExpDate, POS_saleinvdet.INVD_Batch AS INVD_Batch,"
            strsql &= " POS_saleinvdet.INVD_STATUS AS INVD_STATUS," 'added by GS on 27/02/14
            strsql &= " pos_STKfil.STK_ItmClass AS STK_ItmClass,"

            strsql &= " pos_STKfil.STK_REF AS Stk_REFerence, "
            strsql &= " pos_STKfil.Stk_DESC AS Stk_DESC, pos_STKfil.STK_Brand AS S_BRAND"
            'GS on 26/02/14
            strsql = strsql + " Into ClientStatAcc" & gbUser
            strsql = strsql + " FROM "
            strsql = strsql + " CreditInfo" & gbUser & " as CreditInfo"
            strsql = strsql + " Left join " & PTempSaleD & " POS_saleinvdet ON POS_saleinvdet.invD_LOC = CreditInfo.invh_LOC AND"
            strsql = strsql + " POS_saleinvdet.invD_HEADid = CreditInfo.invH_recID LEFT JOIN "
            strsql = strsql + " " & PTempStk & " pos_STKfil ON pos_STKfil.Stk_recid = POS_saleinvdet.invD_STKID "
            strsql = strsql + " inner join stk04_location ON stk04_location.loc_code = creditinfo.invh_loc"
            Execute(strsql)


            If Summ = True Then
                strsql = " SELECT  ClnStatAcc.Code ,"
                strsql &= " ClnStatAcc.Code1 ,"
                strsql &= " ClnStatAcc.Client ,"
                strsql &= " ClnStatAcc.Clntype ,"
                strsql &= " ClnStatAcc.[Cl. Type Name] ,"
                strsql &= " ClnStatAcc.BBFDate AS BBFDate ,"
                strsql &= " ClnStatAcc.BBFValuelc ,"
                strsql &= " ClnStatAcc.BBFValuecc ,"
                strsql &= " ClnStatAcc.CFWLC ,"
                strsql &= " ClnStatAcc.CFWCC ,"
                strsql &= " ClnStatAcc.totalpurchLC ,"
                strsql &= " ClnStatAcc.totalpurchCC ,"
                strsql &= " "
                strsql &= " MAX(CASE WHEN ClientStatAcc.INVH_CURR = '" & gbCurr1 & "' THEN ISNULL(DiffRC, 0) + ISNULL(dIFF, 0)"
                strsql &= " ELSE 0"
                strsql &= " END) AS TotalPaidLC ,"
                strsql &= " MAX(CASE WHEN ClientStatAcc.INVH_CURR = '" & gbCurr2 & "' THEN ISNULL(DiffRC, 0) + ISNULL(dIFF, 0)"
                strsql &= " ELSE 0"
                strsql &= " END) AS TotalPaidCC ,"
                strsql &= " MAX(CASE WHEN ClientStatAcc.INVH_CURR = '" & gbCurr1 & "' THEN dIFF"
                strsql &= " ELSE 0"
                strsql &= " END) AS TotalSettledLC ,"
                strsql &= " MAX(CASE WHEN ClientStatAcc.INVH_CURR = '" & gbCurr2 & "' THEN dIFF"
                strsql &= " ELSE 0"
                strsql &= " END) AS TotalSettledCC ,"
                strsql &= " ClnStatAcc.BBFValuelc + ClnStatAcc.CFWLC"
                strsql &= " + ISNULL(ClnStatAcc.totalpurchLC, 0) -"
                strsql &= " SUM(CASE WHEN ClientStatAcc.INVH_CURR = '" & gbCurr1 & "'"
                strsql &= " THEN  ( ISNULL(DiffRC, 0)"
                strsql &= " + ISNULL(dIFF, 0) )"
                strsql &= " ELSE 0"
                strsql &= " END) AS BalanceLC ,"
                strsql &= " ClnStatAcc.BBFValueCC + ClnStatAcc.CFWCC"
                strsql &= " + ISNULL(ClnStatAcc.totalpurchCC, 0) -"
                strsql &= " SUM(CASE WHEN ClientStatAcc.INVH_CURR = '" & gbCurr2 & "'"
                strsql &= " THEN ( ISNULL(DiffRC, 0)"
                strsql &= " + ISNULL(dIFF, 0) )"
                strsql &= " ELSE 0"
                strsql &= " END) AS BalanceCC"

                strsql &= " into " & ClinetBalnceSumtable & "  From ClnStatAcc" & gbUser & " ClnStatAcc"


                strsql &= " LEFT JOIN dbo.Stk04_Clients ON ClnStatAcc.Code = CL_Code AND ClnStatAcc.Code1 = CL_No "
                strsql &= " LEFT JOIN STk04_ClAccounts ON STk04_ClAccounts.CA_CLRecid = Stk04_Clients.CL_RecID  and CA_Comp ='" & gbComp & "'"
                strsql &= " Left join (SELECT    MAX(INVH_DATE) INVH_DATE ,SUM(ISNULL(INVH_NET,0) - ISNULL(totcredit,0)) Diff , "
                strsql &= " INVH_CLIENTSUP,INVH_CURR FROM ( select distinct INVH_DATE ,INVH_NET, totcredit,INVH_CLIENTSUP  ,INVH_CURR,INVH_DOCTYP1 from ClientStatAcc" & gbUser & " WHERE INVH_DOCTYP1 <> 'RC'  )ClientStatAcc" & gbUser & "  GROUP BY INVH_CLIENTSUP,INVH_CURR  ) as ClientStatAcc"
                strsql &= " on ClnstatAcc.Client = ClientStatAcc.INVH_CLIENTSUP"

                strsql &= " Left join (SELECT    MAX(INVH_DATE) INVH_DATE  , SUM(ISNULL(INVH_NET,0) - ISNULL(totcredit,0))  DiffRC  , "
                strsql &= " INVH_CLIENTSUP,INVH_CURR FROM ClientStatAcc" & gbUser & " WHERE INVH_DOCTYP1 = 'RC'  GROUP BY INVH_CLIENTSUP,INVH_CURR,INVH_DOCTYP1 ) as ClientStatAccRC   "
                strsql &= " on ClnstatAcc.Client = ClientStatAccRC.INVH_CLIENTSUP AND ClientStatAccRC.INVH_CURR = ClientStatAcc.INVH_CURR "

                strsql &= " GROUP BY"
                strsql &= " ClnStatAcc.Code ,"
                strsql &= " ClnStatAcc.Code1 ,"
                strsql &= " ClnStatAcc.Client ,"
                strsql &= " ClnStatAcc.Clntype ,"
                strsql &= " ClnStatAcc.[Cl. Type Name] ,"
                strsql &= " ClnStatAcc.BBFDate  ,"
                strsql &= " ClnStatAcc.BBFValuelc ,"
                strsql &= " ClnStatAcc.BBFValuecc ,"
                strsql &= " ClnStatAcc.CFWLC ,"
                strsql &= " ClnStatAcc.CFWCC ,"
                strsql &= " ClnStatAcc.totalpurchLC ,"
                strsql &= " ClnStatAcc.totalpurchCC"

                '  strsql &= "  Where ClientStatAcc.INVH_CURR ='" & curr & "' "
                Execute(strsql)

            Else
                '''''''''''''''''''''''''''''''''''''''''''''''''''''''''''''''''''''''''''''''''''''''''''''''''''''''''''''''''''''''''''''''''
                DropSqlObject("TotalPerClt" & gbUser, "TABLE")

                strsql = " Select ClnStatAcc.Client ,"
                'strsql &= " SUM(ISNULL(ClientStatAcc.INVH_NET, 0) - ISNULL(ClientStatAcc.totcredit,0)) AS TotalPaid ,"





                'strsql &= "  SUM(ISNULL(ClientStatAcc.INVH_NET, 0) - ISNULL(ClientStatAcc.totcredit, 0))"
                'strsql &= " AS TotalSettledLC ,"


                'strsql &= "  SUM(ISNULL(ClientStatAcc.INVH_NET, 0) - ISNULL(ClientStatAcc.totcredit, 0))"
                'strsql &= "  TotalSettledCC ,"

                'strsql &= "  CASE WHEN ClientStatAcc.INVH_CURR = '" & gbCurr1 & "'"
                'strsql &= " THEN ClnStatAcc.BBFValuelc + ClnStatAcc.CFWLC + ISNULL(ClnStatAcc.totalpurchLC, 0) - SUM(ISNULL(ClientStatAcc.INVH_NET, 0) - ISNULL(ClientStatAcc.totcredit, 0))"
                'strsql &= " Else ClnStatAcc.BBFValuelc + ClnStatAcc.CFWLC + ISNULL(ClnStatAcc.totalpurchLC, 0) END AS BalanceLC ,"

                'strsql &= " CASE WHEN ClientStatAcc.INVH_CURR = '" & gbCurr2 & "'"
                'strsql &= " THEN ClnStatAcc.BBFValueCC + ClnStatAcc.CFWCC + ISNULL(ClnStatAcc.totalpurchCC, 0) - SUM(ISNULL(ClientStatAcc.INVH_NET, 0) - ISNULL(ClientStatAcc.totcredit, 0))"
                'strsql &= " Else ClnStatAcc.BBFValueCC + ClnStatAcc.CFWCC + ISNULL(ClnStatAcc.totalpurchCC, 0) END AS BalanceCC "



                strsql &= "   ClnStatAcc.BBFValuelc + ClnStatAcc.CFWLC + ISNULL(ClnStatAcc.totalpurchLC, 0) " ' SUM(ISNULL(ClientStatAcc.INVH_NET, 0) - ISNULL(ClientStatAcc.totcredit, 0))"

                strsql &= " - Sum( "
                strsql &= "isnull( dbo.convert5('" & gbApp & "','" & gbComp & "'," & gbYear & ","
                strsql &= " ClientStatAcc.INVH_NET,INVH_CURR,'" & gbCurr1 & "','" & Format(CDate(WorkingDay), "MM/dd/yyyy") & "'),0)"
                strsql &= "  - isnull( dbo.convert5('" & gbApp & "','" & gbComp & "'," & gbYear & ","
                strsql &= " ClientStatAcc.totcredit,INVH_CURR,'" & gbCurr1 & "','" & Format(CDate(WorkingDay), "MM/dd/yyyy") & "'),0) )"
                ' strsql &= "  dbo.convert5('" & gbApp & "','" & gbComp & "'," & gbYear & ",- SUM(ISNULL(ClientStatAcc.INVH_NET, 0)"
                ' strsql &= " - ISNULL(ClientStatAcc.totcredit, 0)),INVH_CURR,'" & gbCurr1 & "','" & Format(CDate(WorkingDay), "MM/dd/yyyy") & "')"
                strsql &= "   BalanceLC ,"


                strsql &= "   ClnStatAcc.BBFValueCC + ClnStatAcc.CFWCC + ISNULL(ClnStatAcc.totalpurchCC, 0)  " ' SUM(ISNULL(ClientStatAcc.INVH_NET, 0) - ISNULL(ClientStatAcc.totcredit, 0))"

                strsql &= " - Sum( "
                strsql &= "isnull( dbo.convert5('" & gbApp & "','" & gbComp & "'," & gbYear & ","
                strsql &= " ClientStatAcc.INVH_NET,INVH_CURR,'" & gbCurr2 & "','" & Format(CDate(WorkingDay), "MM/dd/yyyy") & "'),0)"
                strsql &= "  - isnull( dbo.convert5('" & gbApp & "','" & gbComp & "'," & gbYear & ","
                strsql &= " ClientStatAcc.totcredit,INVH_CURR,'" & gbCurr2 & "','" & Format(CDate(WorkingDay), "MM/dd/yyyy") & "'),0) )"


                'strsql &= "  dbo.convert5('" & gbApp & "','" & gbComp & "'," & gbYear & ",- SUM(ISNULL(ClientStatAcc.INVH_NET, 0)"
                'strsql &= " - ISNULL(ClientStatAcc.totcredit, 0)),INVH_CURR,'" & gbCurr2 & "','" & Format(CDate(WorkingDay), "MM/dd/yyyy") & "')"
                strsql &= "  BalanceCC "


                strsql &= " Into TotalPerClt" & gbUser

                strsql &= " From ClnStatAcc" & gbUser & " ClnStatAcc"
                strsql &= " Left join (SELECT DISTINCT INVH_RECID,INVH_DATE,INVH_NET,totcredit, INVH_CLIENTSUP,INVH_CURR FROM ClientStatAcc" & gbUser & ") as ClientStatAcc"
                strsql &= " on ClnstatAcc.Client = ClientStatAcc.INVH_CLIENTSUP"
                '   strsql &= " Where ClnstatAcc.Chk = 'True'"

                strsql &= " Group By ClnStatAcc.Client, ClnStatAcc.BBFValuelc, "
                strsql &= " ClnStatAcc.BBFValuecc, ClnStatAcc.CFWLC, ClnStatAcc.CFWCC, "
                strsql &= " ClnStatAcc.totalpurchCC, ClnStatAcc.totalpurchLC "
                '  strsql &= " , ClientStatAcc.INVH_CURR"

                Execute(strsql)
                ''''''''''''''''''''''''''''''''''''''''''''''''''''''''''''''''''''''''''''''''''''''''''''''''''''''''''''''''''''''''''''''''''

                'Calculate Opening Balance Till A Date
                strsql = "SELECT Distinct"

                strsql = strsql + " ClientStatAcc.INVH_DATE as INVH_DATE, ClientStatAcc.loc_code , ClientStatAcc.Loc_cocode, ClientStatAcc.INVH_LOC, "
                strsql = strsql + " ClientStatAcc.INVH_DOCTYP1, ClientStatAcc.INVH_DOCPREFIX, ClientStatAcc.INVH_SERNO,"
                strsql = strsql + " ClientStatAcc.INVH_NET, ClientStatAcc.INVH_NET - totcredit  Paid,  ClientStatAcc.INVH_CURR, " ' ClientStatAcc.totcredit,"
                strsql = strsql + " ClientStatAcc.INVH_RECID, ClientStatAcc.INVH_CLIENTSUP,  ClientStatAcc.INVH_RKS , "

                strsql = strsql + " ClnStatAcc.Code, ClnStatAcc.Code1, "
                strsql = strsql + " ClnStatAcc.Client,      CL_Name , ClnStatAcc.BBFDate as BBFDate,  ClnStatAcc.BBFValuelc, "
                strsql = strsql + " ClnStatAcc.BBFValuecc,  "
                strsql = strsql + " ClnStatAcc.totalpurchLC, ClnStatAcc.totalpurchCC,"

                strsql &= "   TotalPerClt.BalanceLC, TotalPerClt.BalanceCC "




                strsql = strsql + " into " & ClinetBalnceSumtable & " From ClnStatAcc" & gbUser & " ClnStatAcc"
                strsql = strsql + " Left join ClientStatAcc" & gbUser & " ClientStatAcc"
                strsql = strsql + " on ClnstatAcc.Client = ClientStatAcc.INVH_CLIENTSUP"

                strsql &= " Left Join TotalPerClt" & gbUser & " TotalPerClt ON ClnStatAcc.client = TotalPerClt.client INNER JOIN dbo.Stk04_Clients ON ClnStatAcc.Client = Stk04_Clients.CL_RecID"
                Execute(strsql)
                '  strsql &= " Where ClnstatAcc.Chk = 'True'"
                'strsql = strsql & " Order By ClnStatAcc.CLIENT ,ClientStatAcc.invH_DATE ,ClientStatAcc.invH_DOCTYP1 ,ClientStatAcc.invH_DOCPREFIX, ClientStatAcc.invH_SERNO "

                Dim LblVal As String
                strsql = " SELECT TOP 1 'BBF Date :'+ CONVERT(VARCHAR,ISNULL(BBFDate,''))"
                strsql &= " + ' BBF Value  '+ CONVERT(VARCHAR,Format(ISNULL(BBFValuecc,0),'###,###,###,###,###.###'))"
                strsql &= " + ' " & gbCurr2 & " ;'+ CONVERT(VARCHAR,Format(ISNULL(BBFValueLc,0),'###,###,###,###,###.###'))"
                strsql &= " + ' " & gbCurr1 & "  Balance : '+ CONVERT(VARCHAR,Format(ISNULL(BalanceCC,0),'###,###,###,###,###.###'))"
                strsql &= " + ' " & gbCurr2 & " ;'+ CONVERT(VARCHAR,Format(ISNULL(BalanceLC,0),'###,###,###,###,###.###')) + ' " & gbCurr1 & "'"

                strsql &= " FROM " & ClinetBalnceSumtable & ""
                LblVal = ExecuteScalar(strsql)




                strsql = " SELECT  INVH_DATE , loc_code ,"
                strsql &= " Loc_cocode ,   INVH_LOC ,  INVH_DOCTYP1 ,"
                strsql &= " INVH_DOCPREFIX ,  INVH_SERNO ,  INVH_NET ,paid,INVH_NET-paid pendingbal,"

                strsql &= " INVH_CURR , "
                If Copercurr <> gbCurr1 AndAlso Copercurr <> gbCurr2 Then
                    strsql &= " sum (dbo.convert5FC('" & gbApp & "','" & gbComp & "'," & gbYear & ",CASE INVH_DOCTYP1 WHEN 'RC' THEN 0  WHEN 'CV' THEN 0 WHEN 'DV' THEN 0 ELSE 1  END *  INVH_NET-paid * CASE INVH_DOCTYP1   WHEN 'CV' THEN -1 WHEN 'DV' THEN -1 ELSE 1 END ,INVH_CURR,'" & gbCurr2 & "',INVH_RATE,INVH_DATE) ) OVER (ORDER BY INVH_DATE,RECID ASC) [TotalNet]  , "
                Else
                    strsql &= " sum (dbo.convert5FC('" & gbApp & "','" & gbComp & "'," & gbYear & ",CASE INVH_DOCTYP1 WHEN 'RC' THEN 0  WHEN 'CV' THEN 0 WHEN 'DV' THEN 0 ELSE 1  END *  INVH_NET-paid * CASE INVH_DOCTYP1   WHEN 'CV' THEN -1 WHEN 'DV' THEN -1 ELSE 1 END ,INVH_CURR,'" & Copercurr & "',INVH_RATE,INVH_DATE) ) OVER (ORDER BY INVH_DATE,RECID ASC)  [TotalNet]  , "
                End If


                strsql &= " INVH_RECID , INVH_CLIENTSUP ,  INVH_RKS"

                strsql &= "  From " & ClinetBalnceSumtable & ""

                strsql &= " LEFT JOIN (SELECT INVH_RECID RECID ,INVH_LOC LOC ,INVH_RATE FROM " & LinkServerAndLinkDB & "POS_SALEINVHEAD )"
                strsql &= " POS_SALEINVHEAD ON LOC = INVH_LOC AND RECID = INVH_RECID"


                Dim rptParam(10, 1) As String

                frm = New frmPreviewGrid
                strPrev = Nothing
                With frm

                    frm.BuildSql(strsql, , , , )
                    If ClocHO = False Then

                        .lblPsNote.Text = "N.B: Report Includes Only Data Synchronized with HO."
                    End If

                    '.ColDesign("Code", 0, "Cl. No", , , False, , , , , False, )
                    '.ColDesign("Code1", 0, "Cl. Sub", , , False, , , , , False, )
                    '' .ColDesign("TotalPaid", 0, "TotalPaid", C1.Win.C1TrueDBGrid.AlignHorzEnum.Near, , False, , , , , False)
                    .ColDesign("loc_code", 0, "loc_code", C1.Win.C1TrueDBGrid.AlignHorzEnum.Near, , False, , , , , False)
                    .ColDesign("Loc_cocode", 0, "Loc_cocode", C1.Win.C1TrueDBGrid.AlignHorzEnum.Near, , False, , , , , False)
                    .ColDesign("INVH_RECID", 0, "INVH_RECID", C1.Win.C1TrueDBGrid.AlignHorzEnum.Near, , False, , , , , False)
                    .ColDesign("INVH_CLIENTSUP", 0, "INVH_CLIENTSUP", C1.Win.C1TrueDBGrid.AlignHorzEnum.Near, , False, , , , , False)

                    '.ColDesign("Client", 0, "CLt #", , , False, , , , , False, )
                    '.ColDesign("CL_Name", 0, "Client Name", , , False, , , , , False, )
                    '.ColDesign("BBFDate", -1, "BBF Date", AlignHorzEnum.Center, C1.Win.C1TrueDBGrid.AlignVertEnum.Top, , , , , , , , , , , "dd/MM/yyyy")

                    'If gbCurr1 = "LL" Then
                    '    .ColDesign("BBFValuelc", -1, "BBF Values " & gbCurr1, C1.Win.C1TrueDBGrid.AlignHorzEnum.General, C1.Win.C1TrueDBGrid.AlignVertEnum.Top, , , , , , , , , , , "#,##0.000") '"#,##0")
                    'Else
                    '    .ColDesign("BBFValuelc", -1, "BBF Values " & gbCurr1, C1.Win.C1TrueDBGrid.AlignHorzEnum.General, C1.Win.C1TrueDBGrid.AlignVertEnum.Top, , , , , , , , , , , "#,##0.000")
                    'End If
                    '.ColDesign("BBFValueCc", -1, "BBF Values " & gbCurr2, C1.Win.C1TrueDBGrid.AlignHorzEnum.General, C1.Win.C1TrueDBGrid.AlignVertEnum.Top, , , , , , , , , , , "#,##0.000")

                    ''  .ColDesign("CFWLC", 0, "C/F Values " & gbCurr1, , , False, , , , , False, )
                    ''  .ColDesign("CFWCC", 0, "C/F Values " & gbCurr2, , , False, , , , , False, )
                    'If gbCurr1 = "LL" Then
                    '    .ColDesign("totalpurchLC", -1, "Total Invoices " & gbCurr1, C1.Win.C1TrueDBGrid.AlignHorzEnum.General, C1.Win.C1TrueDBGrid.AlignVertEnum.Top, , , , , , , , , , , "#,##0.000") '"#,##0")
                    'Else
                    '    .ColDesign("totalpurchLC", -1, "Total Invoices " & gbCurr1, C1.Win.C1TrueDBGrid.AlignHorzEnum.General, C1.Win.C1TrueDBGrid.AlignVertEnum.Top, , , , , , , , , , , "#,##0.000")
                    'End If
                    '.ColDesign("totalpurchCC", -1, "Total Invoices " & gbCurr2, C1.Win.C1TrueDBGrid.AlignHorzEnum.General, C1.Win.C1TrueDBGrid.AlignVertEnum.Top, , , , , , , , , , , "#,##0.000")
                    ''If gbCurr1 = "LL" Then
                    ''    .ColDesign("TotalSettledLC", -1, "Total Settled " & gbCurr1, C1.Win.C1TrueDBGrid.AlignHorzEnum.General, C1.Win.C1TrueDBGrid.AlignVertEnum.Top, , , , , , , , , , , "#,##0.000") '"#,##0")
                    ''Else
                    ''    .ColDesign("TotalSettledLC", -1, "Total Settled " & gbCurr1, C1.Win.C1TrueDBGrid.AlignHorzEnum.General, C1.Win.C1TrueDBGrid.AlignVertEnum.Top, , , , , , , , , , , "#,##0.000")
                    ''End If
                    ''.ColDesign("TotalSettledCC", -1, "TotalSettled " & gbCurr2, C1.Win.C1TrueDBGrid.AlignHorzEnum.General, C1.Win.C1TrueDBGrid.AlignVertEnum.Top, , , , , , , , , , , "#,##0.000")
                    'If gbCurr1 = "LL" Then
                    '    .ColDesign("BalanceLC", -1, "Balance " & gbCurr1, C1.Win.C1TrueDBGrid.AlignHorzEnum.General, C1.Win.C1TrueDBGrid.AlignVertEnum.Top, , , , , , , , , , , "#,##0.000") '"#,##0")
                    'Else
                    '    .ColDesign("BalanceLC", -1, "Balance " & gbCurr1, C1.Win.C1TrueDBGrid.AlignHorzEnum.General, C1.Win.C1TrueDBGrid.AlignVertEnum.Top, , , , , , , , , , , "#,##0.000")
                    'End If
                    '.ColDesign("BalanceCC", -1, "Balance " & gbCurr2, C1.Win.C1TrueDBGrid.AlignHorzEnum.General, C1.Win.C1TrueDBGrid.AlignVertEnum.Top, , , , , , , , , , , "#,##0.000")

                    .ColDesign("INVH_DATE", -1, "Inv. Date", AlignHorzEnum.Center, , , , , , , , frmPreviewGrid.Orders.Ascending, , , , "dd/MM/yyyy", , , , , , , )
                    .ColDesign("INVH_DOCTYP1", -1, "Doc Type", C1.Win.C1TrueDBGrid.AlignHorzEnum.Center, , , , , , , , , , , , , , , , , , , )
                    .ColDesign("INVH_DOCPREFIX", -1, "Doc Pref", C1.Win.C1TrueDBGrid.AlignHorzEnum.Center, , , , , , , , , , , , , , , , , , , )
                    .ColDesign("INVH_SERNO", -1, "SerNo", C1.Win.C1TrueDBGrid.AlignHorzEnum.Center, , , , , , , , , , , , , , , , , , , )
                    .ColDesign("INVH_LOC", -1, "Loc", C1.Win.C1TrueDBGrid.AlignHorzEnum.Center, , , , , , , , , , , , , , , , , , , )
                    .ColDesign("INVH_RKS", 250, "Remark", C1.Win.C1TrueDBGrid.AlignHorzEnum.Near, , , , , , , , , , , , , , , , , , , )
                    .ColDesign("INVH_NET", -1, "Net", C1.Win.C1TrueDBGrid.AlignHorzEnum.General, , , , , , , , , , , , "#,##0.000", , , , , , , )

                    .ColDesign("paid", -1, "Settled", C1.Win.C1TrueDBGrid.AlignHorzEnum.General, , , , , , , , , , , , "#,##0.000", , , , , , , )
                    .ColDesign("pendingbal", -1, "Pending", C1.Win.C1TrueDBGrid.AlignHorzEnum.General, , , , , , , , , , , , "#,##0.000", , , , , , , )


                    .ColDesign("INVH_CURR", -1, "Curr", C1.Win.C1TrueDBGrid.AlignHorzEnum.Near, , , , , , , , , , , , , , , , , , , )
                    If Copercurr <> gbCurr1 AndAlso Copercurr <> gbCurr2 Then
                        .ColDesign("TotalNet", -1, "Balance in " & gbCurr2 & "", C1.Win.C1TrueDBGrid.AlignHorzEnum.General, , , , , , , , , , , , "#,##0.000", , , , , , , )
                    Else
                        .ColDesign("TotalNet", -1, "Balance in " & Copercurr & "", C1.Win.C1TrueDBGrid.AlignHorzEnum.General, , , , , , , , , , , , "#,##0.000", , , , , , , )
                    End If

                    '    .ColDesign("totcredit", 0, "Bal", C1.Win.C1TrueDBGrid.AlignHorzEnum.General, , , , , , , , , , , , "#,##0.000", , , , , , , )

                    ReportName = "RptCltStatInv.rpt"
                    frm.ShowDialog(strPrev, _me.Name, "", rptParam, , , , , , , , , , False, , , LblVal)
                End With
                DropSqlObject(ClinetBalnceSumtable, "TABLE")
            End If



            DropSqlObject(pTempSale, "TABLE")
            DropSqlObject(pTempSale1, "TABLE")
            DropSqlObject(PTempSaleD, "TABLE")
            DropSqlObject(PTempCsh, "TABLE")
            DropSqlObject(PTempStk, "TABLE")
            DropSqlObject("ClnStatAcc" & gbUser, "TABLE")
            DropSqlObject("CreditInv" & gbUser, "TABLE")
            DropSqlObject("ClnStat" & gbUser, "TABLE")
            DropSqlObject("ClientStatUnion" & gbUser, "TABLE")
            DropSqlObject("ClnStatLC" & gbUser, "TABLE")
        Catch ex As Exception
            Throw ex
        End Try
    End Sub
    Public Function GetClientCurrBalnce(ByVal Cl_recID As String, ClBalCurr As String) As Double
        Dim LinkServerAndLinkDB As String
        Dim strsql As String
        Try

            If ClocHO = False AndAlso TestConnection() = False Then
                '     MessageBox.Show("No Connection With HO ", _me.Text, MessageBoxButtons.OK, MessageBoxIcon.Information)
                ClientBalanceCnxTest = False
                Return 0
                Exit Try
            End If

            ClientBalanceCnxTest = True

            If ClocHO = False Then
                If linkServer.Trim = "" Then
                    LinkServerAndLinkDB = CStr(IIf(ClocHO = False, "[" & OthDb & "].dbo.", ""))
                Else
                    LinkServerAndLinkDB = CStr(IIf(ClocHO = False, "[" & linkServer & "].[" & OthDb & "].dbo.", ""))
                End If
            Else
                LinkServerAndLinkDB = ""
            End If
            strsql = " Select  "
            strsql &= " isnull(dbo.convert5('" & gbApp & "','" & gbComp & "','" & gbYear & "',ISNULL((CBL_AmountCC),0),'" & gbCurr2 & "','" & CStr(IIf(ClBalCurr = gbCurr1, gbCurr1, gbCurr2)) & "','" & CDate(WorkingDay).ToString("MM/dd/yyyy") & "'),0) CLBalance "
            strsql &= "   from " & LinkServerAndLinkDB & "Stk04_ClientBalCC where CLB_ClReciD = " & Cl_recID & "  "

            Using dtBal As New DataTable

                Using daBal As New SqlDataAdapter(strsql, ConnectionString)
                    daBal.SelectCommand.CommandTimeout = 120
                    daBal.Fill(dtBal)
                End Using

                If dtBal.Rows.Count = 0 Then
                    Return 0
                Else
                    Return CDbl(dtBal(0)("CLBalance").ToString)
                End If

            End Using

        Catch ex As Exception
            ClientBalanceCnxTest = False
            Return 0
        Finally
        End Try

    End Function
    Public Function GetClientBalnceFromACc(ByVal Cl_recID As String, ClBalCurr As String) As DataRow
        Dim UsedConnectionString As String
        Dim connectedToHO As Boolean
        Dim linkserverHoACC, ServerLink, DBnameLink, SQLpswdLink, SQLuserLink, SQLAuth As String
        Dim strsql As String
        Dim GetfromFolio As Boolean
        Try
            If ClocHO = True Then
                UsedConnectionString = ConnectionString
            Else
                UsedConnectionString = gbHOConnectionstring
            End If
            strsql = " SELECT AG_SQLServerName,AG_DBName,AG_USerName,AG_Password,AG_WindowsAuth FROM dbo.STK04_AcGen WHERE AG_Comp='" & gbComp & "'"
            Using Cnsql As New SqlConnection(UsedConnectionString)
                Try
                    Cnsql.Open()
                    connectedToHO = True
                Catch ex As Exception
                    connectedToHO = False
                End Try
                If connectedToHO = False Then
                    ClientBalanceCnxTest = False
                    Return Nothing
                End If
                Using Dt_AccConn As New DataTable()
                    Using Da_AccCon As New SqlDataAdapter(strsql, Cnsql)
                        Da_AccCon.Fill(Dt_AccConn)
                        If Dt_AccConn.Rows.Count <> 0 Then
                            ServerLink = Dt_AccConn(0)("AG_SQLServerName").ToString
                            DBnameLink = Dt_AccConn(0)("AG_DBName").ToString
                            SQLpswdLink = Decpassword(Dt_AccConn(0)("AG_Password").ToString)
                            SQLuserLink = Dt_AccConn(0)("AG_USerName").ToString
                            SQLAuth = Dt_AccConn(0)("AG_WindowsAuth").ToString
                        Else
                            ClientBalanceCnxTest = False
                            Return Nothing
                        End If
                    End Using
                End Using
                Using Cmsql As New SqlCommand(strsql, Cnsql)
                    If UCase(Cnsql.DataSource) <> UCase(ServerLink) Then
                        strsql = " if not exists(select * from master.dbo.sysservers where srvname='" & ServerLink & "')"
                        strsql = strsql + " EXEC SP_addlinkedserver [" & ServerLink & "]"
                        Cmsql.CommandText = strsql
                        Cmsql.ExecuteNonQuery()
                        strsql = "if not exists(SELECT  t2.[srvname] FROM    master.dbo.syslogins T1"
                        strsql &= " INNER JOIN master.dbo.sysservers T2 ON T1.sid = T2.srvid where srvname='" & ServerLink & "' and T1.[Name]='sa')"
                        strsql = strsql + " EXEC sp_addlinkedsrvlogin [" & ServerLink & "],'false',NULL,'sa',"
                        strsql = strsql + " '" & SQLpswdLink & "'"
                        Cmsql.CommandText = strsql
                        Cmsql.ExecuteNonQuery()
                        linkserverHoACC = " [" & ServerLink & "].[" & DBnameLink & "].dbo."
                    ElseIf UCase(Cnsql.Database) <> UCase(DBnameLink) Then
                        linkserverHoACC = " [" & DBnameLink & "].dbo."
                    Else
                        linkserverHoACC = " "
                    End If
                    'strsql = "Select ac_accyear from Com_AppComp Where Ac_Code='" & gbApp & "'"
                    'strsql &= " And Ac_Comp='" & gbComp & "'"
                    'strsql &= " And Ac_Year='" & gbYear & "'"
                    'Cmsql.CommandText = strsql
                    'ACYear = Cmsql.ExecuteScalar.ToString
                    ACYear = (CDate(WorkingDay).Year).ToString
                End Using

                strsql = " SELECT TOP 1 1 FROM Stk04_Clients WHERE ISNULL(CL_FolioNo,0) <> 0"
                strsql &= " AND ISNULL(CL_FolioSubNo,0) =0  AND cl_recid =" & Cl_recID & ""
                Using Cmql As New SqlCommand(strsql, Cnsql)
                    Dim obj As Object
                    If Not (obj Is Nothing OrElse obj Is DBNull.Value OrElse obj.ToString = "") Then
                        GetfromFolio = False
                    Else
                        GetfromFolio = True
                    End If
                End Using

                If GetfromFolio = False Then

                    strsql = "  SELECT   Trn_accno1,Trn_accno2,Trn_subno,Trn_curr ,  CONVERT(VARCHAR, ( MAX(accTbl.Trn_date) ), 103) MaxTransDate , "
                    If ClBalCurr = gbCurr1 Then strsql &= " SUM(accTbl.Trn_amtlc * ( CASE WHEN accTbl.Trn_DC = 'D' THEN 1 ELSE -1 END )) amount"
                    If ClBalCurr = gbCurr2 Then strsql &= " SUM(accTbl.Trn_amtcc * ( CASE WHEN accTbl.Trn_DC = 'D' THEN 1 ELSE -1 END )) amount"
                    If ClBalCurr <> gbCurr1 AndAlso ClBalCurr <> gbCurr2 Then strsql &= " SUM(accTbl.Trn_amtfc * ( CASE WHEN accTbl.Trn_DC = 'D' THEN 1 ELSE -1 END )) amount"
                    strsql &= "  FROM "


                    strsql &= linkserverHoACC & "Acc01_AccTransaction accTbl "

                    strsql &= "  Inner join  ( select  *  from STk04_ClAccounts where  CA_Comp ='" & gbComp & "' and CA_CLRecid = " & Cl_recID & "  ) STk04_ClAccounts "
                    strsql &= " ON  Trn_accno1 COLLATE SQL_Latin1_General_CP1256_CI_AS = SUBSTRING(ca_clac, 1, 6) COLLATE SQL_Latin1_General_CP1256_CI_AS"
                    strsql &= " AND Trn_accno2 COLLATE SQL_Latin1_General_CP1256_CI_AS = SUBSTRING(ca_clac, 7, 4) COLLATE SQL_Latin1_General_CP1256_CI_AS"
                    strsql &= " AND Trn_subno COLLATE SQL_Latin1_General_CP1256_CI_AS = SUBSTRING(ca_clac,11, 1) COLLATE SQL_Latin1_General_CP1256_CI_AS"
                    strsql &= " AND Trn_curr COLLATE SQL_Latin1_General_CP1256_CI_AS =   '" & ClBalCurr & "' COLLATE SQL_Latin1_General_CP1256_CI_AS"
                    strsql &= " WHERE    accTbl.trn_comp = '" & gbComp & "' AND accTbl.Trn_Year = '" & ACYear & "'"

                Else
                    strsql = "  SELECT      isnull(MAX(accTbl.Trn_date),getdate())  MaxTransDate , "
                    If ClBalCurr = gbCurr1 Then strsql &= " isnull(SUM(accTbl.Trn_amtlc * ( CASE WHEN accTbl.Trn_DC = 'D' THEN 1 ELSE -1 END )),0) amount"
                    If ClBalCurr = gbCurr2 Then strsql &= " isnull(SUM(accTbl.Trn_amtcc * ( CASE WHEN accTbl.Trn_DC = 'D' THEN 1 ELSE -1 END )),0) amount"
                    If ClBalCurr <> gbCurr1 AndAlso ClBalCurr <> gbCurr2 Then strsql &= "  isnull(SUM(accTbl.Trn_amtcc * ( CASE WHEN accTbl.Trn_DC = 'D' THEN 1 ELSE -1 END )),0) amount"
                    'If ClBalCurr <> gbCurr1 AndAlso ClBalCurr <> gbCurr2 Then strsql &= " SUM(accTbl.Trn_amtfc * ( CASE WHEN accTbl.Trn_DC = 'D' THEN 1 ELSE -1 END )) amount"
                    strsql &= " FROM    dbo.Stk04_Clients"
                    strsql &= " INNER JOIN dbo.Com_Folio ON Com_Folio.Fo_serno = Stk04_Clients.CL_FolioNo"
                    strsql &= " AND Com_Folio.Fo_subno = Stk04_Clients.CL_FolioSubNo"
                    strsql &= " INNER JOIN dbo.ACC01_Auxiliary ON ACC01_Auxiliary.Aux_foliono = Com_Folio.Fo_serno"
                    strsql &= " AND ACC01_Auxiliary.Aux_subfolio = Com_Folio.Fo_subno"
                    strsql &= " INNER JOIN  " & linkserverHoACC & "ACC01_AccTransaction  accTbl ON Trn_Comp = ACC01_Auxiliary.Aux_Comp"
                    strsql &= " AND Trn_Year = ACC01_Auxiliary.Aux_Year"
                    strsql &= " AND Trn_accno1 = ACC01_Auxiliary.Aux_accno1"
                    strsql &= " AND Trn_accno2 = ACC01_Auxiliary.Aux_accno2"
                    strsql &= " AND Trn_subno = ACC01_Auxiliary.Aux_subno"
                    strsql &= " AND Trn_curr = ACC01_Auxiliary.Aux_curr"
                    strsql &= " WHERE cl_recid =" & Cl_recID & ""
                    'No control on company should be done
                    strsql &= " AND Trn_Year ='" & ACYear & "'  "
                    strsql &= " and (Trn_accno1  like '411%' or Trn_accno1  like '401%' )"

                End If

                Using Dt_Clbal As New DataTable
                    Using Da_Clbal As New SqlDataAdapter(strsql, Cnsql)
                        Da_Clbal.Fill(Dt_Clbal)
                        If Dt_Clbal.Rows.Count = 0 Then
                            ClientBalanceCnxTest = True
                            Return Nothing
                        Else
                            ClientBalanceCnxTest = True
                            If ClBalCurr <> gbCurr1 AndAlso ClBalCurr <> gbCurr2 Then
                                Dt_Clbal(0)("amount") = convert5(CDbl(Dt_Clbal(0)("amount")), gbCurr2, ClBalCurr, CDate(WorkingDay))
                            End If
                            Return Dt_Clbal(0)
                        End If
                    End Using
                End Using






                Cnsql.Close()
            End Using
        Catch ex As Exception
            ClientBalanceCnxTest = False
            Return Nothing
        End Try
    End Function
    Public Function FundsQuery(_ReportTempTable As String, _WithData As Boolean, Optional _recid As Double = 0) As String
        Dim strsql As String
        Try
            strsql = $"        SELECT  FTR_RecID ,
                                       FTR_TrnRecID ,
                                       ROUND(( ABS(FTR_AmountToDisburse)
                                               / ( ABS({IIf(Copercurr = "FC", " INVD_NETFC ", "INVD_NET")})
                                                   - CASE WHEN ReportTempTable.INVH_VATTYPE = 'T'
                                                          THEN ABS(ISNULL( {IIf(Copercurr = "FC", " INVD_VATVALFC ", " INVD_VATVAL ")},
                                                                          0))
                                                          ELSE 0
                                                     END ) ) * 100, 2, 1) [FTR_Percentage],
                                       FDT_Code ,
                                       FDT_Description ,
                                       FTR_AmountToDisburse ,
                                       FTR_Currency ,
                                       FTR_OperLC ,
                                       FTR_RateToLC ,
                                       FTR_CounterValueLC ,
                                       FTR_OperFreshDollar ,
                                       FTR_RateFreshDollar ,
                                       FTR_CounterValueFreshDollar ,
                                       FTR_Remarks,
                                       FTR_INVDCounter
                               FROM    POS_FundTransactionSIDet fndTrn
                                       INNER JOIN ( SELECT FDT_Code ,
                                                           FDT_Description ,
                                                           FDT_RecID
                                                    FROM   {gbAccountingLinkedServerForFunds}ACC01_FundTypes
                                                  ) ACC01_FundTypes ON FDT_RecID = fndTrn.FTR_FDTRecID
                                       INNER JOIN ( SELECT INVD_LOC ,
                                                           INVD_HEADID ,
                                                           INVD_Counter,
                                                           INVD_NET,
                                                           INVD_NETFC,
                                                           INVH_VATTYPE ,
                                                           INVD_VATVAL ,
                                                           INVD_VATVALFC
                                                    FROM   V_Stk04_InvoiceComment"
            If _ReportTempTable = "" Then
                strsql &= "                              INNER JOIN ( SELECT INVH_LOC ,
                                                                             INVH_RECID
                                                                        FROM   dbo.POS_SALEINVHEAD
												                        WHERE INVH_LOC = '" & Cloc & "'
												                        AND INVH_RECID =  '" & _recid & "'
                                                                    ) POS_SALEINVHEAD ON POS_SALEINVHEAD.INVH_RECID = V_Stk04_InvoiceComment.INVH_RECID"
            Else
                strsql &= $"                             INNER JOIN {_ReportTempTable} ON InvHead = INVH_RECID
                                                                                            AND Chk = 'True'"
            End If

            strsql &= "                            ) ReportTempTable ON INVD_LOC = FTR_LocCode
                                                                       AND INVD_HEADID = fndTrn.FTR_TrnRecID
                                                                       AND INVD_Counter = FTR_INVDCounter 

                               WHERE 1=1"

            If gbFundDisbursementModuleIsActive And _WithData Then
                strsql &= " "
            Else

                strsql &= " AND 1=50 "

            End If

            Return strsql.Replace(vbCrLf, " ")

        Catch ex As Exception
            Throw
        End Try
    End Function

    Public Sub PrintCollectedMoney(_me As Form, LocationCode As String, RecID As Double)
        Dim strsql As String
        Try

            Cursor.Current = Cursors.WaitCursor

            Dim rptParam(11, 1) As Object

            rptParam(2, 0) = "Company"
            rptParam(2, 1) = " '" & gbCompName & "'"

            rptParam(3, 0) = "LcRound"
            rptParam(3, 1) = "" & VatLCRound & ""

            rptParam(9, 0) = "Curr1"
            rptParam(9, 1) = " '" & gbCurr1 & "'"

            rptParam(11, 0) = "Location"
            rptParam(11, 1) = "'" & Cloc.Replace("'", "''") & "- " & CLocName.Replace("'", "''") & "'"

            strsql = $" SELECT  ECH_LocCode ,
                                ECH_RecID ,
                                CT_INVHEADER ,
                                INVH_DATE ,
                                ECH_EmployeeCode ,
                                emp.E_Name EmployeeName ,
                                ECH_CashierCode ,
                                csh.E_Name CashierName ,
                                ST_CODE,
		                        ST_DESC,
                                CT_CURRCODE ,
                                CT_AMOUNT ,
                                INVH_RKS
                        FROM    dbo.POS_EmployeesCollectionDetails
                                INNER JOIN dbo.POS_EmployeesCollectionHeader ON ECH_LocCode = ECD_LocCode
                                                                                AND ECH_RecID = ECD_ECHRecID
                                INNER JOIN dbo.STK04_Employee emp ON emp.E_Code = ECH_EmployeeCode
                                INNER JOIN dbo.STK04_Employee csh ON csh.E_Code = ECH_CashierCode
                                INNER JOIN dbo.POS_SALEINVHEAD ON INVH_LOC = ECD_INVHLoc
                                                                  AND INVH_RECID = ECD_INVHRecID
                                INNER JOIN dbo.POS_CASHTRANSACTION ON CT_LOCATION = INVH_LOC
                                                                  AND CT_INVHEADER = INVH_RECID
                                INNER JOIN dbo.Stk04_SETTLEMENTTYPE ON Stk04_SETTLEMENTTYPE.ST_Loc = POS_CASHTRANSACTION.CT_LOCATION 
                                                                  AND Stk04_SETTLEMENTTYPE.ST_CODE = POS_CASHTRANSACTION.CT_SETTLTYPE
                        WHERE   ECH_LocCode = '{LocationCode}'
                                AND ECH_RecID = {RecID}
                                AND INVH_DOCTYP1 = 'CI'; "

            PrintPODirectRep(_me, strsql, "rptCollectedMoney.rpt", 1, rptParam, , , , , , , , , , , )

        Catch ex As Exception
            Throw
        Finally
            Cursor.Current = Cursors.Default
        End Try
    End Sub

End Module
