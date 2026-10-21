Option Explicit Off
Imports System.IO
Imports C1.Win.C1Input
Imports C1.Win.C1TrueDBGrid
Imports System.Data.SqlClient
Imports System.Reflection
Imports DevComponents.DotNetBar
Imports DevComponents.DotNetBar.Controls
Imports System.Windows.Forms.VisualStyles.VisualStyleElement.TaskbarClock
Imports DevComponents.Editors.DateTimeAdv
Imports Microsoft.Office.Interop
Imports System.Threading
Imports DLL_StockSearch

Module ModVar

    Public colSplitter As String = "¿"
    Public rowSplitter As String = "ᴥ"


    Public StkSearchTempTable As String = ""

    Public ConnectionString As String
    Public gbRetSearch(), gbMRetSearch(,) As String
    Public gbServer, gbConType, gbSQLuser, gbSQLpswd, _
           gbPCName, gbDBname, gbUser, gbUserName, gbRepPath, gbHosName, _
           gbApp, gbComp, gbCompName, gbYear, gbLocalIP, gbStepIpAdd As String
    Private gbuserprint As String
    Public ImagesPath As String
    Public ZipPath As String
    Public PicPath As String
    Public gbCurr1, gbCurr2, gbAmt0, gbDetail As String
    Public gbStDate, gbEndDate, gbLstDatUpd As DateTime
    Public VATper As Single
    Public sVATreg As String
    Public vStrategie As String
    'Current accounting year
    Public ACYear As String
    Public CAudit As Boolean
    Public gbQCCode As String
    Public Conv_Err As Boolean
    Public RM_TEMP As String
    Public sCurLoc As String
    Public sCurCodeLoc As String
    Public CurrItmKey() As String
    Public CurrItmKeyPend() As String
    Public CurrItmKeyDesc() As String
    Public gbmaxreflengh As String
    Public CurrItemKeyComp As Boolean   'True is company is part of the key
    Public threadfillingdtcontact As Thread
    Public gbdtContacts As DataTable 'String
    Public gbDBCompYear As String 'Current DB Name + Company + Year
    Public gbExePath As String = Path.GetDirectoryName(Application.ExecutablePath)
    'Public gbLC, gbLCname, gbcurr2, gbCCname As String
    Public i As Integer
    Public strsql As String
    Public ComputerName As String
    Public gbPrivAdd, gbPrivModify, gbPrivDelete, _
           gbPrivView, gbPrivPrint As Boolean

    Public gbDecNo As Integer  'Used decimal points

    Public HideSilent As Boolean

    'Variable Used for Assigned the source control of Richtextbox
    Public gbRichTextSource As RichTextBox
    Public VatLCRound As Integer
    Public ChangePassValue As String

    'Filled When Calling Form For Login Form (Password)
    Public gbFormLoad As Boolean

    'Module Prefix
    Public gbPrxpur As String
    'variable for stock tag  ch @ 25/10/2016
    Public gbStockTag As String
    'variable for stock tag  ch @ 25/10/2016
    Public GBHOCntrlApproved As Boolean
    'Variables For Daily Rates + Operation
    Public Wrate1 As Double, Wrate2 As Double
    Public Wop1 As String, Wop2 As String
    Public WrateNo As Byte 'Rate Used By The Application

    Public gbPrivNegations() As String

    'Variables for item classification
    Public gbTrading, gbFashion, gbExpiry, gbBook As Boolean

    Public nbNotApproved As Integer 'added by GS on 11/05/16
    'Optm TY 05/05/2017--------------------------------
    Public RefreshTimeInMin As Integer
    Public RefreshIsRequested As Boolean
    Public GlobalTempTable As String
    Public GlobalDatatable As DataTable
    Public DV_JurnaOfPI, DV_JurnaOfPO, DV_JurnaOfStat As DataView
    Public GlobalDa As SqlDataAdapter
    Public GlobalDtIsCompleted As Boolean
    Public ThreadFillGlobalTemp As Thread
    Public vatdate As String
    '------------------------------------------------
    ' Public gbtemppath As String = System.IO.Path.GetTempPath & "SavedDocs\"
    Public TempPath As String = Path.GetTempPath & "Po_AD\"
    Private Const CntTitle As String = "Purchase Module"
    Public gbIsOffDb As Boolean
    Public MaxAdjustmentCC As Double
    Public PurchaseSerNo As Boolean
    Public ShowBrandPOPI As Boolean

    Public gbFundDisbursementModuleIsActive As Boolean
    Public gbFreshDollarCurrency, gbAccRateNo As String
    Public gbAccountingLinkedServerForFunds As String = ""

    Public gbPharmacyModuleActive As Boolean

    Public DayIsOpenedBO As Boolean

    Public Sub Execute_WOXACT(ByVal ParamArray SqlStatments() As String)
        Try

            Using Conn As New SqlConnection(ConnectionString)
                Conn.Open()

                Using Com As New SqlCommand("", Conn)

                    Com.CommandTimeout = 900
                    For Idx As Integer = 0 To SqlStatments.Length - 1
                        Com.CommandText = SqlStatments(Idx)
                        Com.ExecuteNonQuery()
                    Next

                End Using

                Conn.Close()
            End Using

        Catch e As Exception
            Throw e
        End Try
    End Sub

    Public Sub FocusStyle(ByRef _ctrl As Control, ByRef _Status As String)
        Try
            If _Status = "Enter" Then
                If _ctrl.GetType Is GetType(TextBox) Then
                    CType(_ctrl, TextBox).SelectAll()
                ElseIf _ctrl.GetType Is GetType(RichTextBox) Then
                    CType(_ctrl, RichTextBox).SelectAll()
                ElseIf _ctrl.GetType Is GetType(C1DateEdit) Then
                    CType(_ctrl, C1DateEdit).SelectAll()
                End If

                _ctrl.BackColor = Color.BlanchedAlmond 'Color.FromArgb(215, 248, 255)  '255,224,192

            ElseIf _ctrl.GetType Is GetType(RadioButton) OrElse _ctrl.GetType Is GetType(CheckBox) Then
                _ctrl.BackColor = Color.Transparent
            Else
                _ctrl.BackColor = Color.White
            End If
        Catch ex As Exception
            Throw ex
        End Try
    End Sub

    Public Sub GridFilterChange(ByRef _GridName As C1TrueDBGrid, Optional ByRef _AddFilter As String = Nothing)
        Dim i As Integer
        Dim dv As DataView
        Dim sColName As String
        Dim strfil, s, F, E As String

        Try

            If _GridName.SelectedRows.Count > 0 Then _GridName.SelectedRows.Clear()

            If _GridName.DataSource.GetType Is GetType(DataView) Then
                dv = CType(_GridName.DataSource, DataView)
            ElseIf _GridName.DataSource.GetType Is GetType(DataTable) Then
                dv = CType(_GridName.DataSource, DataTable).DefaultView
            Else
                Exit Try
            End If

            For i = 0 To _GridName.Splits(0).DisplayColumns.Count - 1

                With _GridName.Splits(0).DisplayColumns(i)

                    '.Style.BackColor = SystemColors.Window

                    If .DataColumn.FilterText <> "" Then

                        sColName = .DataColumn.DataField

                        If dv.Table.Columns(sColName).DataType Is System.Type.GetType("System.DateTime") Then
                            If .DataColumn.FilterText.Trim = "" Then
                                '.Style.BackColor = System.Drawing.Color.Gainsboro
                                strfil &= "IsNull([" & .DataColumn.DataField & "],'01/01/1900')='01/01/1900' and "
                            ElseIf IsDate(.DataColumn.FilterText.Trim) = True Then
                                If .DataColumn.FilterText.Length = 10 AndAlso _
                                   .DataColumn.FilterText.IndexOf(":") < 0 Then
                                    strfil &= " [" & .DataColumn.DataField & "] >= #" & _
                                     CDate(.DataColumn.FilterText.Trim & " 00:00:00").ToString("MM/dd/yyyy HH:mm:ss") & "# and " & _
                                     " [" & .DataColumn.DataField & "] <= #" & _
                                     CDate(.DataColumn.FilterText.Trim & " 23:59:59").ToString("MM/dd/yyyy HH:mm:ss") & "# and "
                                ElseIf .DataColumn.FilterText.Length = 19 AndAlso .DataColumn.FilterText.IndexOf(":") = 13 Then
                                    strfil &= " [" & .DataColumn.DataField & "] = #" & _
                                    CDate(.DataColumn.FilterText.Trim).ToString("MM/dd/yyyy hh:mm:ss") & "# and "
                                End If
                            End If
                        ElseIf dv.Table.Columns(sColName).DataType Is System.Type.GetType("System.String") Then
                            If .DataColumn.FilterText.Trim = "" Then
                                '.Style.BackColor = System.Drawing.Color.Gainsboro
                                strfil &= "(IsNull([" & .DataColumn.DataField & "],'') = '' or " & _
                                "Trim([" & .DataColumn.DataField & "]) = '') and "

                            ElseIf .DataColumn.FilterText.Trim = "!" Then
                                .Style.BackColor = System.Drawing.Color.Goldenrod
                                strfil &= "(IsNull([" & .DataColumn.DataField & "],'') <> '' Or " & _
                                "Trim([" & .DataColumn.DataField & "]) <> '') and "

                            ElseIf .DataColumn.FilterText.Length > 1 AndAlso .DataColumn.FilterText.Substring(0, 1) = "!" Then
                                strfil &= " [" & .DataColumn.DataField & "] not like '" & .DataColumn.FilterText.Substring(1) & "' and "
                                'strfil = Replace(Replace(strfil, "*", "[*]"), "%", "[%]")
                            Else

                                s = .DataColumn.FilterText.Replace("'", "''")

                                s = s.Replace("[", "#L_ACO#").Replace("]", "#R_ACO#")

                                If s.Replace("*", "").Replace("%", "").Trim <> "" Then
                                    If s.StartsWith("%") OrElse s.StartsWith("*") AndAlso s.Length > 1 Then
                                        F = s.Substring(0, 1)
                                    Else
                                        F = Nothing
                                    End If

                                    If s.EndsWith("%") OrElse s.EndsWith("*") AndAlso s.Length > 1 Then
                                        E = s.Substring(s.Length - 1)
                                    Else
                                        E = Nothing
                                    End If
                                End If

                                If Not F Is Nothing OrElse Not E Is Nothing Then

                                    If Not F Is Nothing Then s = s.Substring(1, s.Length - 1)

                                    If Not E Is Nothing AndAlso s.Trim <> "" Then
                                        s = s.Substring(0, s.Length - 1)
                                    End If

                                    s = Replace(Replace(s, "*", "[*]"), "%", "[%]")

                                    If Not F Is Nothing Then s = F & s
                                    If Not E Is Nothing Then s = s & E

                                ElseIf s.IndexOf("%") >= 0 OrElse s.IndexOf("*") >= 0 Then
                                    s = Replace(Replace(s, "*", "[*]"), "%", "[%]")
                                End If

                                s = s.Replace("#L_ACO#", "[[]").Replace("#R_ACO#", "[]]")



                                'Modified By Haitham Timani on 19.June.2013
                                '.'.'.'.'.'.'.'.'.'.'.'.'.'.'.'.'.'.'.'.'.'.'.'.'.'.'.'.'.'.'.'.'.'.'.'.'.'.'.

                                'Modified Because if we filter a check box in the column, it gets the fiter as ([CHK] 'True')
                                ' while it must be ([CHK] = 'True')

                                'strfil &= " [" & .DataColumn.DataField & "] " & _
                                '.DataColumn.FilterOperator & " '" & s & "' and "

                                strfil &= " [" & .DataColumn.DataField & "] " & _
                                IIf(.DataColumn.FilterOperator = "", " = ", .DataColumn.FilterOperator).ToString & " '" & s & "' and "
                                '.'.'.'.'.'.'.'.'.'.'.'.'.'.'.'.'.'.'.'.'.'.'.'.'.'.'.'.'.'.'.'.'.'.'.'.'.'.'.





                            End If
                        ElseIf dv.Table.Columns(sColName).DataType Is System.Type.GetType("System.Boolean") Then
                            strfil &= " [" & .DataColumn.DataField & "] = '" & .DataColumn.FilterText & "' and "

                        Else
                            If .DataColumn.FilterText.Trim = "" Then
                                '.Style.BackColor = System.Drawing.Color.Gainsboro
                                strfil &= "IsNull([" & .DataColumn.DataField & "],-0.00001)=-0.00001 and "
                            ElseIf IsNumeric(.DataColumn.FilterText.Trim) Then
                                'modified by GS on 09/03/16
                                'strfil &= " [" & .DataColumn.DataField & "] " & .DataColumn.FilterOperator & " " & _
                                'CDbl(.DataColumn.FilterText.Trim) & " and "
                                strfil &= " [" & .DataColumn.DataField & "] " & IIf(.DataColumn.FilterOperator = "", "=", .DataColumn.FilterOperator).ToString & " " & _
                                CDbl(.DataColumn.FilterText.Trim) & " and "
                                'GS on 09/03/16
                            End If
                        End If

                    End If

                End With

            Next i

            If Not strfil Is Nothing Then
                strfil = strfil.Substring(0, strfil.Length - 4)
                If Not _AddFilter Is Nothing Then strfil = _AddFilter & " And " & strfil
            ElseIf Not _AddFilter Is Nothing Then
                strfil = _AddFilter
            Else
                strfil = ""
            End If

            dv.RowFilter = strfil

        Catch ex As Exception
            Throw ex
        Finally
            dv = Nothing
        End Try
    End Sub

    Public Function RecordsExists(ByRef _strsql As String, Optional ByVal _cmSql As SqlCommand = Nothing) As Boolean
        Dim cnSql As SqlConnection
        Dim cmSql As SqlCommand
        Try
            If _cmSql Is Nothing Then
                cnSql = New SqlConnection

                cnSql = New SqlConnection(ConnectionString)
                cnSql.Open()

                cmSql = New SqlCommand(_strsql, cnSql)

                If Not cmSql.ExecuteScalar Is Nothing Then
                    RecordsExists = True
                Else
                    RecordsExists = False
                End If
            Else
                _cmSql.CommandText = _strsql
                If Not _cmSql.ExecuteScalar Is Nothing Then
                    RecordsExists = True
                Else
                    RecordsExists = False
                End If
            End If

        Catch ex As Exception
            Throw ex
        Finally
            cmSql = Nothing
            If Not cnSql Is Nothing Then
                If cnSql.State = ConnectionState.Open Then cnSql.Close()
                cnSql = Nothing
            End If
        End Try
    End Function


    Public Function DecPassw(ByVal spwd1 As String) As String
        Dim lp As Integer
        Try
            lp = Len(spwd1)
            For i = 1 To lp
                DecPassw = DecPassw + Chr(Asc(Mid(spwd1, i, 1)) - 2)
            Next i
        Catch ex As Exception
            Throw ex
        End Try
    End Function
    Public Function IncPassw(ByVal spwd1 As String) As String
        Dim lp As Integer
        Try
            lp = Len(spwd1)
            For i = 1 To lp
                IncPassw = IncPassw + Chr(Asc(Mid(spwd1, i, 1)) + 2)
            Next i
        Catch ex As Exception
            Throw ex
        End Try
    End Function
    Public Function Decpassword(ByVal spwd1 As String) As String
        Dim lp As Integer
        Try
            lp = Len(spwd1)
            For i = 1 To lp
                Decpassword = REDACTED + System.Text.Encoding.GetEncoding("windows-1256").GetString(System.Text.Encoding.GetEncoding("windows-1252").GetBytes(Chr(Asc(Right(spwd1, i)) - lp)))
            Next i
        Catch ex As Exception
            Throw ex
        End Try
    End Function
    Public Function Encpassword(ByVal spwd1 As String) As String
        Dim lp As Integer
        Try
            lp = Len(spwd1)
            For i = 1 To lp
                Encpassword = REDACTED + System.Text.Encoding.GetEncoding("windows-1256").GetString(System.Text.Encoding.GetEncoding("windows-1252").GetBytes(Chr(Asc(Right(spwd1, i)) + lp)))
            Next i
        Catch ex As Exception
            Throw ex
        End Try
    End Function

    Public Sub TestPriv(ByRef _frm As Form, ByVal Wstep As String)

        Dim mnu As MenuItem

        Dim i As Int32
        Dim strsql, sName As String

        Dim drsql As SqlDataReader

        Dim cmSql As SqlCommand
        Dim cnSql As SqlConnection

        Try

            cnSql = New SqlConnection(ConnectionString)
            cnSql.Open()

            cmSql = New SqlCommand

            With cmSql
                .CommandType = CommandType.Text
                .Connection = cnSql
            End With

            If _frm.Name = "FrmMain" Then

                For Each mnu In _frm.Menu.MenuItems
                    If mnu.Text <> frmMain.MnuWindows.Text Then
                        EnabledMenu(gbApp, mnu, cmSql, drsql)
                    End If
                Next mnu

                For Each mnu In _frm.Menu.MenuItems
                    If mnu.IsParent AndAlso mnu.Text <> frmMain.MnuWindows.Text Then
                        mnu.Enabled = EnabledParent(mnu)
                    End If
                Next mnu

            Else

                sName = Wstep.Replace("'", "''")

                strsql = "SELECT Com_UserPriv.Priv_Add as Priv_Add,Com_UserPriv.Priv_Modify as Priv_Modify,"
                strsql = strsql & "Com_UserPriv.Priv_Delete as Priv_Delete,Com_UserPriv.Priv_View as Priv_View,"
                strsql = strsql & "Com_UserPriv.Priv_Print as Priv_Print "
                strsql = strsql & "FROM Com_Steps INNER JOIN Com_UserPriv ON "
                strsql = strsql & "Com_Steps.Step_App = Com_UserPriv.Priv_AppCode And "
                strsql = strsql & "Com_Steps.Step_Code = Com_UserPriv.Priv_StepCode "
                strsql = strsql & "WHERE Com_UserPriv.Priv_UserCode = '" & gbUser & "' AND "
                strsql = strsql & "Com_UserPriv.Priv_AppCode = '" & gbApp & "' AND "
                strsql = strsql & "Com_UserPriv.Priv_ModCode = '" & gbApp & "' AND "
                strsql = strsql & "Com_Steps.Step_Code = '" & sName & "' And "
                strsql = strsql & " Com_UserPriv.Priv_Comp='" & gbComp & "' And "
                strsql = strsql & "Com_UserPriv.Priv_Till >= convert(varchar(10),getdate(),101)"

                cmSql.CommandText = strsql

                drsql = cmSql.ExecuteReader()

                If drsql.HasRows Then
                    drsql.Read()

                    If drsql.Item("Priv_Add") Is DBNull.Value Then
                        gbPrivAdd = False
                    Else
                        gbPrivAdd = CType(IIf(drsql.Item("Priv_Add").ToString = "1", True, False), Boolean)
                    End If

                    If drsql.Item("Priv_Modify") Is DBNull.Value Then
                        gbPrivModify = False
                    Else
                        gbPrivModify = CType(IIf(drsql.Item("Priv_Modify").ToString = "1", True, False), Boolean)
                    End If

                    If drsql.Item("Priv_Delete") Is DBNull.Value Then
                        gbPrivDelete = False
                    Else
                        gbPrivDelete = CType(IIf(drsql.Item("Priv_Delete").ToString = "1", True, False), Boolean)
                    End If

                    If drsql.Item("Priv_View") Is DBNull.Value Then
                        gbPrivView = False
                    Else
                        gbPrivView = CType(IIf(drsql.Item("Priv_View").ToString = "1", True, False), Boolean)
                    End If

                    If drsql.Item("Priv_Print") Is DBNull.Value Then
                        gbPrivPrint = False
                    Else
                        gbPrivPrint = CType(IIf(drsql.Item("Priv_Print").ToString = "1", True, False), Boolean)
                    End If
                Else
                    gbPrivAdd = False : gbPrivModify = False
                    gbPrivDelete = False : gbPrivView = False : gbPrivPrint = False
                End If

                drsql.Close() : gbPrivNegations = Nothing
                'Purchase module is open for the user even in official server
                'If gbIsOffDb = True AndAlso gbUser.ToUpper <> "SA" Then
                '    gbPrivAdd = False
                '    If Not _frm Is FrmPInv Then
                '        gbPrivModify = False : gbPrivDelete = False

                '    End If
                'End If
                ''''''''''''''''''''''''''''''''''''''''''''''''''''''''''''''''''''''''''



                'strsql = "Select Nbu_Button From Com_NegationButton Where "
                'strsql &= "Nbu_User = '" & gbUser & "' And Nbu_Appl = '" & gbApp & "' "
                'strsql &= "And Nbu_Module = '" & _Module & "' And Nbu_Step = '" & sName & "' "
                'strsql &= "And Not Nbu_Button is NULL And Ltrim(Nbu_Button) <> ''"

                'With cmSql
                '    .CommandText = strsql : drsql = .ExecuteReader
                'End With

                'If drsql.HasRows Then

                '    Do While drsql.Read
                '        If gbPrivNegations Is Nothing Then
                '            i = 0
                '        Else
                '            i = gbPrivNegations.Length
                '        End If : ReDim Preserve gbPrivNegations(i)
                '        gbPrivNegations(i) = drsql.Item("Nbu_Button").ToString.Trim
                '    Loop

                'End If

            End If

        Catch ex As Exception
            Throw ex
        Finally

            mnu = Nothing

            If Not drsql Is Nothing Then
                If drsql.IsClosed = False Then drsql.Close()
                drsql = Nothing
            End If

            cmSql = Nothing

            If Not cnSql Is Nothing Then
                If cnSql.State = ConnectionState.Open Then cnSql.Close()
                cnSql = Nothing
            End If

        End Try


        'gbPrivAdd = True
        'gbPrivModify = True
        'gbPrivDelete = True
        'gbPrivView = True
        'gbPrivPrint = True

    End Sub

    Function GetFieldDesc(ByVal Table As String, ByVal Criteria As String, ByVal ReturnDesc As String, Optional ByVal _cmSql As SqlCommand = Nothing) As String
        'Function to look up Description In A given Table
        Dim strsql As String
        Dim cnSql As SqlConnection
        Dim cmSql As SqlCommand
        Dim drSql As SqlDataReader

        Try
            If Criteria = "" Then GetFieldDesc = "" : Exit Function

            strsql = "select " & ReturnDesc & " from " & Table & " where " & Criteria

            If _cmSql Is Nothing Then
                cnSql = New SqlConnection(ConnectionString)
                cnSql.Open()

                cmSql = New SqlCommand(strsql, cnSql)

                drSql = cmSql.ExecuteReader
            Else
                _cmSql.CommandText = strsql
                drSql = _cmSql.ExecuteReader
            End If

            If drSql.Read Then
                GetFieldDesc = drSql.Item(0).ToString
            Else
                GetFieldDesc = ""
            End If
        Catch ex As Exception
            Throw ex
        Finally
            If Not drSql Is Nothing Then
                If drSql.IsClosed = False Then drSql.Close()
                drSql = Nothing
            End If

            cmSql = Nothing

            If Not cnSql Is Nothing Then
                If cnSql.State = ConnectionState.Open Then cnSql.Close()
                cnSql = Nothing
            End If
        End Try
    End Function

    Public Function ConvertToByte(ByVal _MediaPath As String) As Byte()
        Dim Bytes As Byte()
        Try
            Dim Fs As FileStream = New FileStream(_MediaPath, FileMode.Open, FileAccess.Read)
            Dim BR As BinaryReader = New BinaryReader(Fs)
            Bytes = BR.ReadBytes(System.Convert.ToInt32(Fs.Length))
            BR.Close() : BR.Dispose() : Fs.Close() : Fs.Dispose()
            Return Bytes
        Catch ex As Exception
            Throw ex
        End Try
    End Function

    Public Sub ReadSys(ByVal Wapp As String, ByVal Wcomp As String, ByVal WYear As String, Optional accServername As String = "")
        Dim strsql As String
        Dim cnSql As SqlConnection
        Dim cmSql As SqlCommand
        Dim drSql As SqlDataReader

        Try
            strsql = "Select * from " & accServername & "Com_AppComp Where Ac_Code='" & Wapp & "'"
            strsql &= " And Ac_Comp='" & Wcomp & "'"
            strsql &= " And Ac_Year='" & WYear & "'"


            cnSql = New SqlConnection(ConnectionString)
            cnSql.Open()

            cmSql = New SqlCommand(strsql, cnSql)

            drSql = cmSql.ExecuteReader

            If drSql.Read Then
                gbCurr1 = drSql.Item("Ac_curr1").ToString()
                gbCurr2 = drSql.Item("Ac_curr2").ToString()
                HosDLL.Others.Curr1 = gbCurr1
                HosDLL.Others.Curr2 = gbCurr2
                gbStDate = CDate(drSql.Item("Ac_stdate").ToString())
                gbEndDate = CDate(drSql.Item("Ac_EndDate").ToString())
                gbLstDatUpd = CDate(drSql.Item("Ac_LPDate").ToString())
                gbAmt0 = drSql.Item("Ac_Amt0").ToString()
                gbDetail = drSql.Item("Ac_Detail").ToString()
            Else
                MsgBox("Application " + Wapp + ", Company " + Wcomp + ", Year " + WYear + " Not Defined In Appl-Company File !", vbCritical, "No Processing")
                End
            End If
        Catch ex As Exception
            Throw ex
        Finally
            If Not drSql Is Nothing Then
                If drSql.IsClosed = False Then drSql.Close()
                drSql = Nothing
            End If

            cmSql = Nothing

            If Not cnSql Is Nothing Then
                If cnSql.State = ConnectionState.Open Then cnSql.Close()
                cnSql = Nothing
            End If
        End Try

    End Sub
    Public Sub EnabledControls(ByRef _Control As Control, ByRef _Value As Boolean, ByVal ParamArray _Array() As Control)
        Dim sCtrl As Control
        Dim sValue As Boolean

        Try
            For Each sCtrl In _Control.Controls

                If sCtrl.Controls.Count > 0 Then

                    If Array.IndexOf(_Array, sCtrl) >= 0 Then
                        sValue = Not _Value
                    Else
                        sValue = _Value
                    End If

                    If sCtrl.GetType Is GetType(C1TrueDBGrid) Then
                        If sValue = False Then
                            With CType(sCtrl, C1TrueDBGrid)
                                .Enabled = False : .HeadingStyle.ForeColor = Color.Gray
                            End With
                        Else
                            With CType(sCtrl, C1TrueDBGrid)
                                .Enabled = True : .HeadingStyle.ForeColor = Color.Maroon
                            End With
                        End If
                    Else
                        EnabledControls(sCtrl, sValue, _Array)
                    End If

                ElseIf Not sCtrl.GetType Is GetType(Button) AndAlso _
                    (sCtrl.Tag Is Nothing OrElse sCtrl.Tag.ToString.Trim <> "Negation") Then

                    If Array.IndexOf(_Array, sCtrl) >= 0 Then
                        sValue = Not _Value
                    Else
                        sValue = _Value
                    End If

                    sCtrl.Enabled = sValue

                    If sCtrl.GetType Is GetType(TextBox) OrElse sCtrl.GetType Is GetType(C1DateEdit) OrElse _
                      (sCtrl.GetType Is GetType(Label) AndAlso sCtrl.Tag Is "TextBox") Then
                        If sValue = True Then
                            sCtrl.BackColor = Color.White
                        Else
                            sCtrl.BackColor = Color.AliceBlue
                        End If
                        If sCtrl.GetType Is GetType(C1DateEdit) AndAlso sCtrl.Name.IndexOf("Time") < 0 Then
                            CType(sCtrl, C1DateEdit).ShowDropDownButton = sValue
                        End If
                    End If

                End If

            Next sCtrl

        Catch ex As Exception
            Throw ex
        Finally
            sCtrl = Nothing
        End Try

    End Sub
    Public Sub EnabledButtons(ByRef _Control As Control, ByRef _Value As Boolean, ByVal ParamArray _Array() As Control)
        Dim sCtrl As Control : Dim sValue As Boolean

        Try
            For Each sCtrl In _Control.Controls
                If sCtrl.GetType Is GetType(Button) Then
                    If Not sCtrl.Tag Is Nothing AndAlso sCtrl.Tag.ToString.Trim = "Negation" Then
                        If gbUser <> "SU" Then sCtrl.Enabled = False
                    Else
                        If Array.IndexOf(_Array, sCtrl) >= 0 Then
                            sValue = Not _Value
                        Else
                            sValue = _Value
                        End If
                        sCtrl.Enabled = sValue
                    End If
                ElseIf sCtrl.Controls.Count > 0 Then
                    EnabledButtons(sCtrl, _Value, _Array)
                End If
            Next sCtrl

        Catch ex As Exception
            Throw ex
        Finally
            sCtrl = Nothing
        End Try
    End Sub
    Public Sub AddStepsLogging(ByVal Wstep As String)
        Dim strsql As String
        Dim cnSql As SqlConnection
        Dim cmSql As SqlCommand

        Try
            If gbUser = "SU" Then Exit Try
            If Wstep = "" Then Exit Sub
            strsql = "Insert Into Com_STEPSLOG "
            strsql &= "(Log_Date ,Log_Time ,Log_User ,Log_App,Log_Module,Log_Step ,Log_TimeIn ,Log_TimeOut,"
            strsql &= " Log_Ustmp,Log_Dstmp,Log_IPAdd,Log_PCName)"
            strsql &= "Values ('" & CType(Now, Date).ToString("MM/dd/yyyy") & "'"
            strsql &= ",'" & CType(Now, Date).ToString("hh:mm:ss") & "'"
            strsql &= ",'" & gbUser & "'"
            strsql &= ",'" & gbApp & "'"
            strsql &= ",'" & gbApp & "'"
            strsql &= ",'" & Wstep & "'"
            strsql &= ",'" & CType(Now, Date).ToString("hh:mm:ss") & "'"
            strsql &= ",'00:00:00','" & gbUser & "',getdate(),'" & gbStepIpAdd & "' , '" & gbPCName & "')"

            cnSql = New SqlConnection(ConnectionString)
            cnSql.Open()

            cmSql = New SqlCommand(strsql, cnSql)

            cmSql.ExecuteNonQuery()

        Catch ex As Exception
            If ex.Message.Contains("The INSERT statement conflicted with the FOREIGN KEY constraint ""FK_Com_StepsLog_Com_Steps""") Then
            Else
                MessageBox.Show(ex.Message, "Add Steps Logging", MessageBoxButtons.OK, MessageBoxIcon.Error)
            End If



        End Try
    End Sub
    Public Sub UpdateStepsLogging(ByVal Wstep As String)
        Dim strsql As String
        Dim cnSql As SqlConnection
        Dim cmSql As SqlCommand

        Try
            If Wstep = "" Then Exit Sub
            strsql = "Update Com_STEPSLOG "
            strsql &= "Set Log_TimeOut ='" & CType(Now, Date).ToString("hh:mm:ss") & "',"
            strsql &= " Log_Ustmp='" & gbUser & "',"
            strsql &= " Log_Dstmp=getdate()"
            strsql &= " WHERE Log_date ='" & CType(Now, Date).ToString("MM/dd/yyyy") & "'"
            strsql &= "AND Log_User= '" & gbUser & "'"
            strsql &= "AND Log_Step='" & Wstep & "'"
            strsql &= "AND Log_TimeOut='00:00:00'"

            cnSql = New SqlConnection(ConnectionString)
            cnSql.Open()

            cmSql = New SqlCommand(strsql, cnSql)
            cmSql.ExecuteNonQuery()
        Catch ex As Exception
            MessageBox.Show(ex.Message, "Update Steps Logging", MessageBoxButtons.OK, MessageBoxIcon.Error)
        End Try

    End Sub
    'Created By Haitham Timani on 06.September.2012
    Enum StringType
        Numeric
        DateTime
        Time12
        Time24
        Other
    End Enum
    'Modified By Haitham Timani on 06.September.2012
    Public Function ValidGet(ByVal _ctr As Control, ByRef _KeyChar As Char, ByRef _Type As StringType, _
        Optional ByRef _precision As Int16 = 0, Optional ByRef _iscale As Int16 = 0, _
        Optional ByRef _quote As Boolean = False, _
        Optional ByRef _Sign As Boolean = False, Optional ByVal UppCase As Boolean = False) As Boolean

        Dim sText As String
        Dim iKey, iPoint, ibfPoint, iafPoint, iSelLen As Int32
        Dim StartPoint As Integer
        Try

            '_Type = Char,Numeric,DateTime Or Time12,Time24

            If UppCase = True Then
                _KeyChar = CChar(_KeyChar.ToString.ToUpper)
            End If

            If _ctr.GetType Is GetType(RichTextBox) Then
                iSelLen = CType(_ctr, RichTextBox).SelectionLength
                StartPoint = CType(_ctr, RichTextBox).SelectionStart
            Else
                iSelLen = CType(_ctr, TextBox).SelectionLength
                StartPoint = CType(_ctr, TextBox).SelectionStart
            End If

            sText = _ctr.Text

            'If sText <> "" And _Type = StringType.Numeric Then
            '    sText = mathround(CDbl(sText), 3).ToString
            'End If
            iKey = Asc(_KeyChar)

            If iKey = 8 Then 'BackSpace
                ValidGet = False
            Else
                Select Case _Type
                    Case StringType.Numeric
                        If _precision > 15 Then _precision = 15
                        If iKey = 43 OrElse iKey = 45 Then 'equal to + or -
                            If Len(sText) - iSelLen > 0 OrElse _Sign = False Then ValidGet = True
                        ElseIf iKey = 46 Then 'equal to .
                            If Len(_ctr.Text) - iSelLen = 0 OrElse _iscale = 0 OrElse _
                               InStr(1, sText, ".", CompareMethod.Text) > 0 Then
                                ValidGet = True
                            End If
                        ElseIf iKey < 48 OrElse iKey > 57 Then 'not between 0 and 9
                            ValidGet = True
                        ElseIf iSelLen < Len(sText) Then
                            iPoint = InStr(1, sText, ".", CompareMethod.Text)

                            If iPoint > 0 And iPoint <= StartPoint Then
                                iafPoint = Len(Mid(sText, iPoint + 1, Len(sText)))
                                If iafPoint = _iscale Then ValidGet = True
                            Else
                                ibfPoint = Len(sText) - iSelLen
                                If ibfPoint = (_precision - _iscale) Then ValidGet = True
                            End If
                        End If
                    Case StringType.DateTime
                        '32 = Space,47=/, 48=1, 57=9
                        If Len(sText) - iSelLen >= 10 OrElse (iKey <> 32 AndAlso (iKey < 47 OrElse iKey > 57)) Then
                            ValidGet = True
                        End If
                    Case StringType.Time12
                        '32 = Space,47=/, 48=1, 57=9, 58=:
                        If Len(sText) - iSelLen >= 8 OrElse (iKey <> 32 AndAlso (iKey < 47 OrElse iKey > 58)) Then
                            ValidGet = True
                        End If
                    Case StringType.Time24
                        '32 = Space,47=/, 48=1, 57=9, 58=:
                        If Len(sText) - iSelLen >= 5 OrElse (iKey <> 32 AndAlso (iKey < 47 OrElse iKey > 58)) Then
                            ValidGet = True
                        End If
                    Case Else
                        If Len(sText) - iSelLen >= _precision Then
                            ValidGet = True
                        ElseIf _quote = False AndAlso iKey = 39 Then
                            ValidGet = True
                        End If
                End Select
            End If

        Catch ex As Exception
            Throw ex
        Finally
            _ctr = Nothing
        End Try

    End Function

    Public Function DateStamp() As String
        Try
            DateStamp = Format(Now, "dd/MM/yyyy") + "-" + Format(Now, "hh:mm tt") + "," + gbUser + ";" + gbPCName + "_" + gbLocalIP

        Catch ex As Exception
            Throw ex
        End Try

    End Function
    Public Function GetDateStamp(ByVal ModStamp As String) As String
        GetDateStamp = Mid(ModStamp, 1, InStr(ModStamp, "-", ) - 1)
    End Function
    Public Function GetTimeStamp(ByVal ModStamp As String) As String
        GetTimeStamp = Mid(ModStamp, InStr(ModStamp, "-") + 1, InStr(ModStamp, ",") - InStr(ModStamp, "-") - 1)
    End Function
    Public Function GetUserStamp(ByVal ModStamp As String) As String
        GetUserStamp = Mid(ModStamp, InStr(ModStamp, ",") + 1, InStr(ModStamp, ";") - InStr(ModStamp, ",") - 1)
    End Function
    Public Function GetPCNameStamp(ByVal ModStamp As String) As String
        GetPCNameStamp = Mid(ModStamp, InStr(ModStamp, ";") + 1, InStr(ModStamp, "_") - InStr(ModStamp, ";") - 1)
    End Function
    Public Function GetIPStamp(ByVal ModStamp As String) As String
        GetIPStamp = Mid(ModStamp, InStr(ModStamp, "_") + 1, Len(ModStamp) - InStr(ModStamp, ",") - 1)
    End Function
    Public Function ValidateEmpNo(ByVal EmpNo As String, ByVal EmpSubNo As String, Optional ByVal Active As String = "All") As Boolean
        Dim strsql As String
        Try
            strsql = "SELECT * FROM [WK_Employee] WHERE [E_Comp]='" & gbComp & "' "
            strsql &= " AND [E_EmpNo]='" & EmpNo & "' AND [E_EmpSubNo]='" & EmpSubNo & "' "
            If Active = "A" Then
                strsql &= " AND [E_Active]='A'"
            ElseIf Active = "I" Then
                strsql &= " AND [E_Active]='I'"
            End If
            ValidateEmpNo = RecordsExists(strsql)
        Catch ex As Exception
            Throw ex
        End Try
    End Function
    Public Function ValidateProjOff(ByVal ProjOffId As String) As Boolean
        Dim strsql As String
        Try
            strsql = "SELECT CPO_RecId FROM [WK_ClientProjOff] WHERE CPO_RecId='" & ProjOffId & "' "
            strsql &= " AND CPO_Comp='" & gbComp & "' AND [CPO_Acive]='Y' AND [CPO_CancelDT] IS null"
            ValidateProjOff = RecordsExists(strsql)
        Catch ex As Exception
            Throw ex
        End Try
    End Function
    Public Function ValidateTaskType(ByVal TaskCode As String) As Boolean
        Dim strsql As String
        Try
            strsql = "Select TT_Code From WK_TaskType Where TT_Code = '" & TaskCode & "'"
            strsql &= " and TT_Comp = '" & gbComp & "'"
            ValidateTaskType = RecordsExists(strsql)
        Catch ex As Exception
            Throw ex
        End Try
    End Function
    Public Function ValidateClientNo(ByVal ClientNo As String, ByVal ClientSubNo As String, Optional ByVal Active As String = "All") As Boolean
        Dim strsql As String
        Try
            strsql = "SELECT * FROM [WK_Client] WHERE [CL_Comp]='" & gbComp & "' "
            strsql &= " AND [CL_CLNo]='" & ClientNo & "' AND [CL_CLSubNo]='" & ClientSubNo & "' "
            If Active = "A" Then
                strsql &= " AND [CL_Priority]='A'"
            ElseIf Active = "I" Then
                strsql &= " AND [CL_Priority]='N'"
            End If
            ValidateClientNo = RecordsExists(strsql)
        Catch ex As Exception
            Throw ex
        End Try
    End Function
    Public Function ValidateFolio(ByVal FolioNo As String, ByVal FolioSubNo As String) As Boolean
        Dim strsql As String
        Try
            strsql = "SELECT * FROM Com_Folio WHERE [FO_Serno]='" & FolioNo & "' "
            strsql &= " AND [FO_SubNo]='" & FolioSubNo & "'"
            ValidateFolio = RecordsExists(strsql)
        Catch ex As Exception
            Throw ex
        End Try
    End Function
    Public Sub ChkUnchFnct(ByVal Stat As String, ByVal LstGrid As C1.Win.C1TrueDBGrid.C1TrueDBGrid, _
                           ByVal daSql As SqlDataAdapter, ByVal dsSql As DataSet, ByVal TmpTable As String, ByVal IndexTB As String, _
                           Optional ByVal order As String = "", Optional ByVal Condition As String = "", _
                           Optional ByVal Condition2 As String = "", Optional ByVal _cmsql As SqlCommand = Nothing)
        Dim i As Integer
        Dim tdb As New C1.Win.C1TrueDBGrid.C1TrueDBGrid
        Dim tbName, strsql, Index As String
        Dim cmsql As SqlCommand
        Dim cnsql As SqlConnection
        Dim da As New SqlDataAdapter
        Try
            Cursor.Current = Cursors.WaitCursor

            tdb = LstGrid
            da = daSql
            Index = IndexTB

            If tdb.Splits(0).Rows.Count > 0 Then
                tdb.UpdateData()
                da.Update(dsSql.Tables(Index))

                'Condition2 is used only in update. Not in select

                If Stat = "Check" Then
                    strsql = "update " & TmpTable & " set [chk] = 'True' where [chk] = 'False' " & Condition & Condition2
                ElseIf Stat = "Uncheck" Then
                    strsql = "update " & TmpTable & " set [chk] = 'False' where [chk] = 'True' " & Condition & Condition2
                End If

                With tdb
                    For i = 0 To .Columns.Count - 1
                        'Abir T. @ 06.09.2013
                        If .Columns(i).FilterText <> "" Then
                            If .Columns(i).ValueItems.Presentation = PresentationEnum.CheckBox Then
                                strsql &= "AND [" & .Columns(i).DataField & "] = '" & .Columns(i).FilterText & "' "
                            ElseIf .Columns(i).ValueItems.Presentation <> PresentationEnum.CheckBox Then
                                'strsql &= "and [" & .Columns(i).DataField & "] " & .Columns(i).FilterOperator & " "

                                'Abir T. @ 16.02.2013
                                If dsSql.Tables(Index).Columns(i).DataType Is System.Type.GetType("System.DateTime") Then
                                    If .Columns(i).FilterText.Trim = "" Then
                                        .Style.BackColor = System.Drawing.Color.Gainsboro
                                        strsql &= "and [" & .Columns(i).DataField & "] " & .Columns(i).FilterOperator & " "
                                        strsql &= "IsNull([" & .Columns(i).DataField & "],'01/01/1900')='01/01/1900' "
                                    ElseIf IsDate(.Columns(i).FilterText.Trim) = True AndAlso .Columns(i).FilterText.Length = 10 Then
                                        strsql &= "and [" & .Columns(i).DataField & "] " & .Columns(i).FilterOperator & " "
                                        strsql &= "'" & CDate(.Columns(i).FilterText.Trim).ToString("MM/dd/yyyy") & "' "
                                    End If
                                ElseIf dsSql.Tables(Index).Columns(i).DataType Is System.Type.GetType("System.Decimal") Then
                                    If IsNumeric(.Columns(i).FilterText.Trim) Then 'added by GS on 19/03/16
                                        strsql &= "and [" & .Columns(i).DataField & "] " & .Columns(i).FilterOperator & " "
                                        strsql &= .Columns(i).FilterText.Replace("'", "''").Replace("*", "%").Trim & " "
                                    End If 'added by GS on 19/03/16
                                Else
                                    strsql &= "and [" & .Columns(i).DataField & "] " & .Columns(i).FilterOperator & " "
                                    strsql &= "'" & .Columns(i).FilterText.Replace("'", "''").Replace("*", "%").Trim & "' "
                                End If
                            End If
                        End If
                    Next
                End With
                If _cmsql Is Nothing Then
                    cnsql = New SqlConnection(ConnectionString)
                    cnsql.Open()
                    cmsql = New SqlCommand(strsql, cnsql)
                    cmsql.ExecuteNonQuery()
                Else
                    _cmsql.CommandText = strsql
                    _cmsql.ExecuteNonQuery()
                End If


                'Abir T. @ 18.02.2013
                strsql = "SELECT * FROM " & TmpTable & " where 1=1 " & Condition  'order by chk desc"
                If order <> "" Then strsql &= " Order By " & order

                dsSql.Tables(Index).Clear()
                da.SelectCommand.CommandText = strsql
                da.Fill(dsSql, Index)
                tdb.Focus()
                tdb.Row = 0
            End If

        Catch ex As Exception
            Throw ex
        Finally
            tdb = Nothing : da = Nothing
            cmsql = Nothing
            If Not cnsql Is Nothing Then
                If cnsql.State = ConnectionState.Open Then cnsql.Close()
                cnsql = Nothing
            End If
            Cursor.Current = Cursors.Default
        End Try
    End Sub

    Public Sub DropSqlObject(ByVal ObjName As String, ByVal ObjType As String)
        Dim strsql As String
        Dim cnsql As SqlConnection
        Dim cmsql As SqlCommand
        Try
            cnsql = New SqlConnection(ConnectionString)
            cnsql.Open()
            cmsql = New SqlCommand(strsql, cnsql)

            If Trim(ObjName) = "" Then Exit Sub
            strsql = "IF EXISTS (SELECT * FROM SYSOBJECTS WHERE ID = OBJECT_ID(N'" + ObjName + "') "
            If ObjType = "VIEW" Then
                strsql &= " AND OBJECTPROPERTY(ID, N'IsView') = 1)"
                strsql &= " DROP view [dbo].[" + ObjName + "]"
                cmsql.CommandText = strsql
                cmsql.ExecuteNonQuery()
            ElseIf ObjType = "TABLE" Then
                strsql &= " AND OBJECTPROPERTY(ID, N'IsUserTable') = 1)"
                strsql &= " DROP Table [dbo].[" + ObjName + "]"
                cmsql.CommandText = strsql
                cmsql.ExecuteNonQuery()
            End If
        Catch ex As Exception
            Throw ex
        Finally
            cmsql = Nothing

            If Not cnsql Is Nothing Then
                If cnsql.State = ConnectionState.Open Then cnsql.Close()
                cnsql = Nothing
            End If
        End Try
    End Sub
    Public Sub DropSqlObjectWithTrans(ByVal ObjName As String, ByVal ObjType As String, ByVal cmSql As SqlCommand)
        Dim strsql As String
        Dim cnsql As SqlConnection
        Try
            If Trim(ObjName) = "" Then Exit Sub

            strsql = "IF EXISTS (SELECT * FROM SYSOBJECTS WHERE ID = OBJECT_ID(N'" + ObjName + "') "

            If ObjType = "VIEW" Then
                strsql &= " AND OBJECTPROPERTY(ID, N'IsView') = 1)"
                strsql &= " DROP view [dbo].[" + ObjName + "]"

                cmSql.CommandText = strsql
                cmSql.ExecuteNonQuery()

            ElseIf ObjType = "TABLE" Then
                strsql &= " AND OBJECTPROPERTY(ID, N'IsUserTable') = 1)"
                strsql &= " DROP Table [dbo].[" + ObjName + "]"

                cmSql.CommandText = strsql
                cmSql.ExecuteNonQuery()
            End If
        Catch ex As Exception
            Throw ex
        End Try
    End Sub
    Public Function GetMaxRecId(ByVal TableName As String, ByVal RecIdName As String) As String
        Dim strsql As String
        Dim cnSql As SqlConnection
        Dim cmSql As SqlCommand
        Dim drSql As SqlDataReader

        Try
            strsql = "SELECT isnull(MAX(" & RecIdName & "),0)+1 FROM [" & TableName & "] "

            cnSql = New SqlConnection(ConnectionString)
            cnSql.Open()

            cmSql = New SqlCommand(strsql, cnSql)

            drSql = cmSql.ExecuteReader

            If drSql.Read Then
                If Not drSql.Item(0).ToString = "" Then
                    GetMaxRecId = drSql.Item(0).ToString
                Else
                    GetMaxRecId = "1"
                End If
            Else
                GetMaxRecId = "1"
            End If

        Catch ex As Exception
            Throw ex
        Finally
            If Not drSql Is Nothing Then
                If drSql.IsClosed = False Then drSql.Close()
                drSql = Nothing
            End If

            cmSql = Nothing

            If Not cnSql Is Nothing Then
                If cnSql.State = ConnectionState.Open Then cnSql.Close()
                cnSql = Nothing
            End If
        End Try
    End Function
    Public Function CreateDataTable(ByRef _tblName As String, ByRef _codeName As String, _
      ByRef _descName As String, ByRef _tblRow(,) As String, _
      Optional ByRef _Keys As Boolean = True) As DataTable

        Dim myDataTable As DataTable

        Dim myDataRow As DataRow
        Dim myDataColumn As DataColumn
        Dim PrimaryKeyColumns(0) As DataColumn

        Dim i As Int32

        Try

            myDataTable = New DataTable(_tblName)

            ' Create First Column
            myDataColumn = New DataColumn

            With myDataColumn
                .DataType = System.Type.GetType("System.String")
                .ColumnName = _codeName
                .ReadOnly = True
                .Unique = _Keys
            End With

            ' Add the Column to the DataColumnCollection.
            myDataTable.Columns.Add(myDataColumn)

            myDataColumn = Nothing

            ' Create Second Column.

            If Not _descName Is Nothing Then
                myDataColumn = New DataColumn

                With myDataColumn
                    .DataType = System.Type.GetType("System.String")
                    .ColumnName = _descName
                    .ReadOnly = True
                    .Unique = False
                End With

                ' Add the Column to the DataColumnCollection.
                myDataTable.Columns.Add(myDataColumn)

                myDataColumn = Nothing

            End If

            ' Make the ID column the primary key column.
            If _Keys = True Then
                PrimaryKeyColumns(0) = myDataTable.Columns(_codeName)
                myDataTable.PrimaryKey = PrimaryKeyColumns
            End If

            For i = 0 To _tblRow.GetUpperBound(0)
                myDataRow = myDataTable.NewRow()
                myDataRow(_codeName) = _tblRow(i, 0)
                If Not _descName Is Nothing Then myDataRow(_descName) = _tblRow(i, 1)
                myDataTable.Rows.Add(myDataRow)
            Next i

            Return (myDataTable)

        Catch exc As Exception
            Throw exc
        Finally
            myDataColumn = Nothing
            myDataRow = Nothing
            myDataTable = Nothing
        End Try

    End Function
    Public Function GetEmpRecIdByUserCode() As String
        Try
            GetEmpRecIdByUserCode = GetFieldDesc("V_Pwd", "user_code ='" & gbUser & "' and Priv_Comp='" & gbComp & "'", "E_RecId")
        Catch ex As Exception
            Throw ex
        End Try
    End Function
    Public Function GetLinkedEmpNameConc(ByVal Table As String) As String
        Dim strsql As String
        Dim cnSql As SqlConnection
        Dim cmSql As SqlCommand
        Try
            cnSql = New SqlConnection(ConnectionString)
            cnSql.Open()
            cmSql = New SqlCommand(strsql, cnSql)

            strsql = "Select TOP 1(Select REPLACE(SUBSTRING(St1.E_Name,1,CHARINDEX(' ',St1.E_Name)+1 ),' ','')+ '/' "
            strsql &= " From " & Table & " ST1"
            strsql &= " Where ST1.Chk = St2.Chk ORDER BY ST1.E_EmpNo"
            strsql &= " For XML PATH ('')) AS splitName FROM " & Table & "] St2 "
            strsql &= " WHERE chk='True'"

            'CloseRec(RS)
            'RS.Open(strsql, DeMain.ConWK, adOpenStatic, adLockReadOnly)

            'If RS.RecordCount <> 0 Then
            '    GetTeamName = Mid(Mid(RS.Fields(0), 1, Len(RS.Fields(0)) - 1), 1, 100)
            'Else
            '    GetTeamName = ""
            'End If

            'strsql = "Update Tmp_Emp_" & UserCode & " set chk='-1' where chk='1' "
            'DeMain.ConWK.Execute(strsql)

        Catch ex As Exception
            Throw ex
        End Try
    End Function

    Public Sub CheckUncheck(ByRef _Table As DataTable, ByRef _ColName As String, _
   Optional ByRef _Filter As String = Nothing, Optional ByRef _Cols() As String = Nothing, _
   Optional ByRef _ColVal() As String = Nothing, Optional ByRef value As String = Nothing, _
   Optional ByVal _RowOrderCol As String = "")

        Dim i As Int32
        Dim bVal As Boolean
        Dim sfltr As String

        Dim drs(), dr As DataRow
        Dim RowOrderNum As Integer
        Try
            Cursor.Current = Cursors.WaitCursor

            With _Table

                sfltr = .DefaultView.RowFilter

                If Not _Filter Is Nothing AndAlso _Filter.Trim <> "" Then
                    If sfltr <> "" Then
                        sfltr &= " And " & _Filter
                    Else
                        sfltr &= _Filter
                    End If
                End If

                drs = .Select(sfltr)

                If sfltr.Trim = "" Then
                    sfltr = "[" & _ColName & "] = 'False'"
                Else
                    sfltr = "[" & _ColName & "] = 'False' And " & sfltr
                End If

                If .Select(sfltr).Length > 0 Then bVal = True

                If _RowOrderCol <> "" Then
                    RowOrderNum = CInt(.Compute(" MAX(" & _RowOrderCol & ") +1 ", "").ToString)
                End If
                For Each dr In drs

                    If _RowOrderCol <> "" AndAlso dr(_ColName).ToString <> "True" Then

                        dr(_RowOrderCol) = CStr(RowOrderNum)
                        RowOrderNum += 1
                    End If

                    If Not value Is Nothing Then dr(_ColName) = value Else dr(_ColName) = bVal

                    If Not _Cols Is Nothing AndAlso Not _ColVal Is Nothing _
                       AndAlso _Cols.GetUpperBound(0) = _ColVal.GetUpperBound(0) Then
                        For i = 0 To _Cols.GetUpperBound(0)
                            dr(_Cols(i)) = _ColVal(i)
                        Next i
                    End If
                Next dr

            End With

        Catch ex As Exception
            Throw ex
        Finally
            drs = Nothing : dr = Nothing
        End Try
    End Sub
    Public Sub RefreshFocus(ByRef _Me As Form)
        Try
            _Me.ActiveControl.Focus() : Application.DoEvents()
            SendKeys.Send("+{TAB}") : Application.DoEvents()
            SendKeys.Send("{TAB}") : Application.DoEvents()
        Catch ex As Exception
        Finally
            _Me = Nothing
        End Try
    End Sub
    Public Sub EnabledEdit(ByRef _Parent As Control, ByRef _Enabled As Boolean, _
    ByRef _ReadOnly As Boolean, ByVal ParamArray _Exceptions() As String)

        Dim ctrl As Control

        Try
            For Each ctrl In _Parent.Controls
                With ctrl
                    If .GetType Is GetType(TextBox) OrElse .GetType Is GetType(C1DateEdit) OrElse _
                       .GetType Is GetType(ListBox) OrElse .GetType Is GetType(CheckedListBox) OrElse _
                       .GetType Is GetType(RichTextBox) OrElse (.GetType Is GetType(Label) AndAlso _
                       Not .Tag Is Nothing AndAlso .Tag.ToString = "TextBox") Then

                        If _ReadOnly = True Then
                            .Enabled = True
                        ElseIf Array.IndexOf(_Exceptions, .Name) >= 0 Then
                            .Enabled = Not _Enabled
                        Else
                            .Enabled = _Enabled
                        End If

                        .ForeColor = Color.Black

                        If .Enabled = True Then
                            Select Case .GetType.ToString
                                Case GetType(TextBox).ToString

                                    If Array.IndexOf(_Exceptions, .Name) >= 0 Then
                                        CType(ctrl, TextBox).ReadOnly = Not _ReadOnly
                                    Else
                                        CType(ctrl, TextBox).ReadOnly = _ReadOnly
                                    End If

                                Case GetType(C1DateEdit).ToString

                                    If Array.IndexOf(_Exceptions, .Name) >= 0 Then
                                        CType(ctrl, C1DateEdit).ReadOnly = Not _ReadOnly
                                    Else
                                        CType(ctrl, C1DateEdit).ReadOnly = _ReadOnly
                                    End If

                                    With CType(ctrl, C1DateEdit)
                                        .ShowDropDownButton = Not .ReadOnly
                                        .ShowUpDownButtons = Not .ReadOnly
                                    End With

                                Case GetType(RichTextBox).ToString
                                    If Array.IndexOf(_Exceptions, .Name) >= 0 Then
                                        CType(ctrl, RichTextBox).ReadOnly = Not _ReadOnly
                                    Else
                                        CType(ctrl, RichTextBox).ReadOnly = _ReadOnly
                                    End If
                            End Select

                            .BackColor = Color.White
                        Else
                            .BackColor = SystemColors.Control
                        End If

                    ElseIf (.GetType Is GetType(Button) AndAlso _
                        Not .Tag Is Nothing AndAlso .Tag.ToString = "DropDown") _
                        OrElse .GetType Is GetType(RadioButton) Then

                        If .GetType Is GetType(Button) Then
                            If _ReadOnly = True AndAlso Array.IndexOf(_Exceptions, .Name) >= 0 Then
                                .Visible = False
                            Else
                                .Visible = True
                            End If
                        Else
                            .Enabled = _Enabled
                        End If
                    ElseIf .GetType Is GetType(CheckBox) Then
                        .Enabled = _Enabled
                    ElseIf .Controls.Count > 0 Then
                        EnabledEdit(ctrl, _Enabled, _ReadOnly, _Exceptions)
                    End If
                End With

            Next ctrl

        Catch ex As Exception
            Throw ex
        Finally
            ctrl = Nothing
        End Try

    End Sub
    Public Sub Logging(ByRef _Module As String, ByRef _Steps As String, ByRef _Direction As String, ByRef _strsql As String, _
        ByRef _LogDate As String, ByRef _LogTime As String, ByRef _Log As Boolean, Optional ByRef _Error As Boolean = False)

        Dim cnSql As SqlConnection
        Dim cmSql As SqlCommand

        Dim sqlVal As String

        Try

            cnSql = New SqlConnection(ConnectionString)
            cnSql.Open()

            cmSql = New SqlCommand(_strsql, cnSql)

            sqlVal = CType(cmSql.ExecuteScalar, String)

            If Not sqlVal Is Nothing AndAlso Not sqlVal Is DBNull.Value Then
                If _Direction = "Load" Then
                    _Log = LogSteps("Add", _Module, _Steps, _LogDate, _LogTime)
                ElseIf _Log = True Then
                    LogSteps(IIf(_Error = True, "Delete", "Modify").ToString, _Module, _Steps, _LogDate, _LogTime)
                End If
            End If
        Catch ex As Exception
            Throw ex
        Finally
            If Not cmSql Is Nothing Then
                cmSql.Dispose() : cmSql = Nothing
            End If : If Not cnSql Is Nothing Then
                If cnSql.State = ConnectionState.Open Then cnSql.Close()
                cnSql.Dispose() : cnSql = Nothing
            End If

            Try
                'Classes.LockRecord.ReleaseUserStepsLock(ConnectionString, gbChildApp, _Steps, gbUser)
            Catch ex As Exception
            End Try

        End Try
    End Sub
    Public Function LogSteps(ByRef _Mode As String, ByRef _Module As String, ByRef _Steps As String, _
        ByRef _logDate As String, ByRef _logTime As String) As Boolean

        Dim cmSql As SqlCommand
        Dim cnSql As SqlConnection

        Try
            cnSql = New SqlConnection(ConnectionString)
            cnSql.Open()

            cmSql = New SqlCommand

            With cmSql
                .CommandText = "stp_LogStepManagement"
                .CommandType = CommandType.StoredProcedure
                .Connection = cnSql

                .Parameters.Add("@Mode", _Mode)
                .Parameters.Add("@User", gbUser) : .Parameters.Add("@App", gbApp)
                .Parameters.Add("@Module", _Module) : .Parameters.Add("@Steps", _Steps)

                If _Mode = "Add" Then
                    .Parameters.Add("@logDate", SqlDbType.VarChar, 10).Direction = ParameterDirection.Output
                    .Parameters.Add("@logTime", SqlDbType.VarChar, 8).Direction = ParameterDirection.Output
                Else
                    .Parameters.Add("@logDate", _logDate)
                    .Parameters.Add("@logTime", _logTime)
                End If

                .ExecuteNonQuery()

                If _Mode = "Add" Then
                    _logDate = .Parameters("@logDate").Value.ToString
                    _logTime = .Parameters("@logTime").Value.ToString
                End If

                LogSteps = True

            End With
        Catch ex As Exception
            LogSteps = False : Throw ex
        Finally
            cmSql = Nothing
            If Not cnSql Is Nothing Then
                If cnSql.State = ConnectionState.Open Then cnSql.Close()
                cnSql = Nothing
            End If
        End Try

    End Function
    Public Sub PriviligeMenu(ByRef _Module As String, ByRef _frm As Form, Optional ByRef _Name As String = "")
        Dim mnu As MenuItem

        Dim i As Int32
        Dim strsql, sName As String

        Dim drsql As SqlDataReader

        Dim cmSql As SqlCommand
        Dim cnSql As SqlConnection

        Try

            cnSql = New SqlConnection(ConnectionString)
            cnSql.Open()

            cmSql = New SqlCommand

            With cmSql
                .CommandType = CommandType.Text
                .Connection = cnSql
            End With

            If _frm.Name = "frmMainMenu" Then

                For Each mnu In _frm.Menu.MenuItems
                    If mnu.Text <> frmMain.MnuWindows.Text Then
                        EnabledMenu(_Module, mnu, cmSql, drsql)
                    End If
                Next mnu

                For Each mnu In _frm.Menu.MenuItems
                    If mnu.IsParent AndAlso mnu.Text <> frmMain.MnuWindows.Text Then
                        mnu.Enabled = EnabledParent(mnu)
                    End If
                Next mnu

            Else

                sName = _Name.Replace("'", "''")

                strsql = "SELECT Com_UserPriv.Priv_Add as Priv_Add,Com_UserPriv.Priv_Modify as Priv_Modify,"
                strsql = strsql & "Com_UserPriv.Priv_Delete as Priv_Delete,Com_UserPriv.Priv_View as Priv_View,"
                strsql = strsql & "Com_UserPriv.Priv_Print as Priv_Print "
                strsql = strsql & "FROM Com_Steps INNER JOIN Com_UserPriv ON "
                strsql = strsql & "Com_Steps.Step_App = Com_UserPriv.Priv_AppCode And "
                strsql = strsql & "Com_Steps.Step_Code = Com_UserPriv.Priv_StepCode "
                strsql = strsql & "WHERE Com_UserPriv.Priv_UserCode = '" & gbUser & "' AND "
                strsql = strsql & "Com_UserPriv.Priv_AppCode = '" & gbApp & "' AND "
                strsql = strsql & "Com_UserPriv.Priv_ModCode = '" & _Module & "' AND "
                strsql = strsql & "Com_Steps.Step_Code = '" & sName & "' And "
                strsql = strsql & "Com_UserPriv.Priv_Till >= convert(varchar(10),getdate(),101)"

                cmSql.CommandText = strsql

                drsql = cmSql.ExecuteReader()

                If drsql.HasRows Then
                    drsql.Read()

                    If drsql.Item("Priv_Add") Is DBNull.Value Then
                        gbPrivAdd = False
                    Else
                        gbPrivAdd = CType(IIf(drsql.Item("Priv_Add").ToString = "1", True, False), Boolean)
                    End If

                    If drsql.Item("Priv_Modify") Is DBNull.Value Then
                        gbPrivModify = False
                    Else
                        gbPrivModify = CType(IIf(drsql.Item("Priv_Modify").ToString = "1", True, False), Boolean)
                    End If

                    If drsql.Item("Priv_Delete") Is DBNull.Value Then
                        gbPrivDelete = False
                    Else
                        gbPrivDelete = CType(IIf(drsql.Item("Priv_Delete").ToString = "1", True, False), Boolean)
                    End If

                    If drsql.Item("Priv_View") Is DBNull.Value Then
                        gbPrivView = False
                    Else
                        gbPrivView = CType(IIf(drsql.Item("Priv_View").ToString = "1", True, False), Boolean)
                    End If

                    If drsql.Item("Priv_Print") Is DBNull.Value Then
                        gbPrivPrint = False
                    Else
                        gbPrivPrint = CType(IIf(drsql.Item("Priv_Print").ToString = "1", True, False), Boolean)
                    End If
                Else
                    gbPrivAdd = False : gbPrivModify = False
                    gbPrivDelete = False : gbPrivView = False : gbPrivPrint = False
                End If

                drsql.Close() : gbPrivNegations = Nothing

                strsql = "Select Nbu_Button From Com_NegationButton Where "
                strsql &= "Nbu_User = '" & gbUser & "' And Nbu_Appl = '" & gbApp & "' "
                strsql &= "And Nbu_Module = '" & _Module & "' And Nbu_Step = '" & sName & "' "
                strsql &= "And Not Nbu_Button is NULL And Ltrim(Nbu_Button) <> ''"

                With cmSql
                    .CommandText = strsql : drsql = .ExecuteReader
                End With

                If drsql.HasRows Then

                    Do While drsql.Read
                        If gbPrivNegations Is Nothing Then
                            i = 0
                        Else
                            i = gbPrivNegations.Length
                        End If : ReDim Preserve gbPrivNegations(i)
                        gbPrivNegations(i) = drsql.Item("Nbu_Button").ToString.Trim
                    Loop

                End If

            End If

        Catch ex As Exception
            Throw ex
        Finally

            mnu = Nothing

            If Not drsql Is Nothing Then
                If drsql.IsClosed = False Then drsql.Close()
                drsql = Nothing
            End If

            cmSql = Nothing

            If Not cnSql Is Nothing Then
                If cnSql.State = ConnectionState.Open Then cnSql.Close()
                cnSql = Nothing
            End If

        End Try
    End Sub

    Public Sub ctMenuCopy(ByRef _frm As Form, ByRef _ContextMenu As ContextMenu)
        Dim txt As TextBox
        Dim rtxt As RichTextBox

        Try
            If _frm.ActiveControl.GetType Is GetType(RichTextBox) Then
                rtxt = CType(_frm.ActiveControl, RichTextBox)
                Clipboard.SetDataObject(rtxt.SelectedText)
            Else
                txt = CType(_frm.ActiveControl, TextBox)
                Clipboard.SetDataObject(txt.SelectedText)
            End If

            _ContextMenu.MenuItems(2).Visible = True

        Catch ex As Exception
            Throw ex
        Finally
            txt = Nothing
        End Try

    End Sub
    Public Sub ctMenuCut(ByRef _frm As Form, ByRef _ContextMenu As ContextMenu)
        Dim txt As TextBox
        Dim rtxt As RichTextBox

        Try

            If _frm.ActiveControl.GetType Is GetType(RichTextBox) Then
                rtxt = CType(_frm.ActiveControl, RichTextBox)
                Clipboard.SetDataObject(rtxt.SelectedText)
                rtxt.SelectedText = ""
            Else
                txt = CType(_frm.ActiveControl, TextBox)
                Clipboard.SetDataObject(txt.SelectedText)
                txt.SelectedText = ""
            End If

            _ContextMenu.MenuItems(2).Visible = True

        Catch ex As Exception
            Throw ex
        Finally
            txt = Nothing
        End Try
    End Sub
    Public Sub ctMenuPast(ByRef _frm As Form, Optional ByRef _Type As String = "Char", Optional ByRef _Quote As Boolean = True, Optional ByVal x As Integer = 0, Optional ByVal y As Integer = 0, Optional _Sign As Boolean = True)
        Dim sValue As String

        Dim txt As TextBox
        Dim rtxt As RichTextBox

        'Char = String
        'Long = Integer
        'Numeric = Decimal 

        Try
            If _Quote = False AndAlso CType(Clipboard.GetDataObject.GetData(DataFormats.Text), String).IndexOf("'") >= 0 Then Exit Try 'added by GS on 02/06/14

            If _frm.ActiveControl.GetType Is GetType(RichTextBox) Then
                rtxt = CType(_frm.ActiveControl, RichTextBox)
                If Clipboard.GetDataObject.GetDataPresent(DataFormats.Text) Then
                    If rtxt.Enabled = True AndAlso rtxt.ReadOnly = False Then
                        sValue = CType(Clipboard.GetDataObject.GetData(DataFormats.Text), String)
                        If rtxt.Text <> "" AndAlso rtxt.MaxLength >= rtxt.Text.Length + sValue.Length Then
                            rtxt.SelectedText = CType(Clipboard.GetDataObject.GetData(DataFormats.Text), String)
                        End If
                    End If
                End If
            Else
                If _frm.ActiveControl.GetType Is GetType(C1TrueDBGrid) Then Exit Sub 'added by GS on 12/08/14
                txt = CType(_frm.ActiveControl, TextBox)
                If Clipboard.GetDataObject.GetDataPresent(DataFormats.Text) Then
                    If txt.Enabled = True AndAlso txt.ReadOnly = False Then
                        sValue = CType(Clipboard.GetDataObject.GetData(DataFormats.Text), String)
                        'edited by GS on 03/03/14'''''''''
                        'If (_Type <> "Char" AndAlso Not IsNumeric(sValue)) OrElse _
                        '   (_Type = "Long" AndAlso sValue.IndexOf(".") > -1) Then
                        '    Exit Try
                        'End If
                        If (_Type <> "Char" AndAlso (Not IsNumeric(sValue) Or sValue.IndexOf(".") >= 0 And txt.Text.IndexOf(".") >= 0 And txt.SelectedText.IndexOf(".") < 0)) OrElse _
                          (_Type = "Long" AndAlso sValue.IndexOf(".") >= 0) Then
                            Exit Try
                        End If
                        ''''''''''''''''''''''''''''''''''

                        If _Sign = False AndAlso _Type <> "Char" AndAlso Val(sValue) < 0 Then Exit Try

                        If txt.MaxLength >= (txt.Text.Length - txt.SelectionLength) + sValue.Length Or txt.MaxLength = 0 Then
                            'Added by GS on 02/06/14
                            Dim TempTextBox As New TextBox
                            TempTextBox.Text = txt.Text
                            TempTextBox.Text = TempTextBox.Text.Remove(txt.SelectionStart, txt.SelectionLength)
                            TempTextBox.Text = TempTextBox.Text.Insert(txt.SelectionStart, CType(Clipboard.GetDataObject.GetData(DataFormats.Text), String))
                            If x > 0 And y > 0 Then
                                'Dim TempTextBox As New TextBox
                                'TempTextBox.Text = txt.Text
                                'TempTextBox.Text = TempTextBox.Text.Remove(txt.SelectionStart, txt.SelectionLength)
                                'TempTextBox.Text = TempTextBox.Text.Insert(txt.SelectionStart, CType(Clipboard.GetDataObject.GetData(DataFormats.Text), String))
                                If Not DecimalValid(x, y, TempTextBox) Then Exit Try
                            Else
                                If _Type = "Long" AndAlso Not IsNumeric(TempTextBox.Text) Then Exit Try 'added by GS on 03/03/16
                            End If
                            'GS on 02/06/14
                            txt.SelectedText = CType(Clipboard.GetDataObject.GetData(DataFormats.Text), String)
                        End If
                    End If
                End If
            End If

        Catch ex As Exception
            Throw ex
        Finally
            txt = Nothing : rtxt = Nothing
        End Try
    End Sub




    Public Sub FilterChange(ByRef _GridName As C1TrueDBGrid, Optional ByRef _AddFilter As String = Nothing)
        Dim i As Integer
        Dim strfil As String

        Dim dv As DataView

        Try

            If _GridName.DataSource.GetType Is GetType(DataView) Then
                dv = CType(_GridName.DataSource, DataView)
            Else
                dv = CType(_GridName.DataSource, DataTable).DefaultView
            End If

            For i = 0 To _GridName.Splits(0).DisplayColumns.Count - 1

                With _GridName.Splits(0).DisplayColumns(i)

                    .Style.BackColor = SystemColors.Window

                    If .DataColumn.FilterText <> "" Then

                        If dv.Table.Columns(i).DataType Is System.Type.GetType("System.DateTime") Then
                            If .DataColumn.FilterText.Trim = "" Then
                                .Style.BackColor = System.Drawing.Color.Gainsboro
                                strfil &= "IsNull([" & .DataColumn.DataField & "],'01/01/1900')='01/01/1900' and "
                            ElseIf IsDate(.DataColumn.FilterText.Trim) = True AndAlso .DataColumn.FilterText.Length >= 8 Then
                                If .DataColumn.FilterOperator = "=" Then
                                    strfil &= " [" & .DataColumn.DataField & "] >= #" & _
                                    CDate(.DataColumn.FilterText.Trim & " 00:00:00").ToString("MM/dd/yyyy HH:mm:ss") & "# and " & _
                                    " [" & .DataColumn.DataField & "] <= #" & _
                                    CDate(.DataColumn.FilterText.Trim & " 23:59:59").ToString("MM/dd/yyyy HH:mm:ss") & "# and "
                                Else
                                    strfil &= " [" & .DataColumn.DataField & "] " & .DataColumn.FilterOperator & " #" & _
                                    CDate(.DataColumn.FilterText.Trim).ToString("MM/dd/yyyy HH:mm:ss") & "# and "
                                End If
                            End If
                        ElseIf dv.Table.Columns(i).DataType Is System.Type.GetType("System.String") Then
                            If .DataColumn.FilterText.Trim = "" Then
                                .Style.BackColor = System.Drawing.Color.Gainsboro
                                strfil &= "(IsNull([" & .DataColumn.DataField & "],'') = '' or " & _
                                "Trim([" & .DataColumn.DataField & "]) = '') and "
                            Else
                                strfil &= " [" & .DataColumn.DataField & "] " & .DataColumn.FilterOperator & " '" & _
                                .DataColumn.FilterText.Replace("'", "''") & "' and "
                            End If
                        Else
                            If .DataColumn.FilterText.Trim = "" Then
                                .Style.BackColor = System.Drawing.Color.Gainsboro
                                strfil &= "IsNull([" & .DataColumn.DataField & "],-0.00001)=-0.00001 and "
                            ElseIf IsNumeric(.DataColumn.FilterText.Trim) = True Then
                                strfil &= " [" & .DataColumn.DataField & "] " & .DataColumn.FilterOperator & " " & _
                                .DataColumn.FilterText.Trim & " and "
                            End If
                        End If

                    End If

                End With

            Next i

            If Not strfil Is Nothing Then
                strfil = strfil.Substring(0, strfil.Length - 4)
                If Not _AddFilter Is Nothing Then strfil = _AddFilter & " And " & strfil
            ElseIf Not _AddFilter Is Nothing Then
                strfil = _AddFilter
            Else
                strfil = ""
            End If

            dv.RowFilter = strfil

        Catch ex As Exception
            Throw ex
        Finally
            dv = Nothing
        End Try
    End Sub
    Private Sub EnabledMenu(ByRef _Module As String, ByRef _ParMenu As MenuItem, _
                            ByRef _cm As SqlCommand, ByRef _dr As SqlDataReader)
        Dim Mnu As MenuItem

        Dim strsql, sMenuName As String

        Try
            For Each Mnu In _ParMenu.MenuItems

                If Mnu.Text <> frmMain.GhcCloseMenu.Text AndAlso Mnu.Text <> frmMain.MnuWindows.Text AndAlso _
                   Mnu.Text <> frmMain.hlpAbout.Text Then

                    If Mnu.IsParent = False Then

                        sMenuName = Mnu.Text.Trim.Replace("'", "''")

                        strsql = "SELECT Com_Steps.Step_Code "
                        strsql = strsql & "FROM Com_Steps INNER JOIN Com_UserPriv ON "
                        strsql = strsql & "Com_Steps.Step_App = Com_UserPriv.Priv_AppCode And "
                        strsql = strsql & "Com_Steps.Step_Code = Com_UserPriv.Priv_StepCode "
                        strsql = strsql & "WHERE (Com_UserPriv.Priv_UserCode = '" & gbUser & "' AND "
                        strsql = strsql & "Com_UserPriv.Priv_AppCode = '" & gbApp & "' AND "
                        strsql = strsql & "Com_UserPriv.Priv_ModCode = '" & _Module & "' AND "
                        strsql = strsql & "Com_Steps.Step_Code = '" & sMenuName & "' And "
                        strsql = strsql & "Com_UserPriv.Priv_Till >= convert(varchar(10),getdate(),101) "
                        strsql = strsql & "AND (abs(Com_UserPriv.Priv_Add) = '1' OR  abs(Com_UserPriv.Priv_Modify) = '1' OR "
                        strsql = strsql & "abs(Com_UserPriv.Priv_Delete) = '1' OR abs(Com_UserPriv.Priv_View) = '1' OR "
                        strsql = strsql & "abs(Com_UserPriv.Priv_Print) = '1'))"

                        _cm.CommandText = strsql

                        _dr = _cm.ExecuteReader()

                        If _dr.Read Then
                            Mnu.Enabled = True
                        Else
                            Mnu.Enabled = False
                        End If

                        _dr.Close()
                    Else
                        EnabledMenu(_Module, Mnu, _cm, _dr)
                    End If
                End If

            Next Mnu


        Catch ex As Exception
            Throw ex
        Finally
            Mnu = Nothing
        End Try

    End Sub
    Private Function EnabledParent(ByRef _mnu As MenuItem) As Boolean
        Dim bValue As Boolean
        Dim mnu1 As MenuItem

        Try
            For Each mnu1 In _mnu.MenuItems
                If mnu1.Text <> "-" Then
                    If mnu1.IsParent = True Then mnu1.Enabled = EnabledParent(mnu1)
                    If mnu1.Enabled = True Then bValue = True
                End If
            Next mnu1

            Return bValue

        Catch ex As Exception
            Throw ex
        Finally
            mnu1 = Nothing
        End Try

    End Function
    Function GetAcYear(ByVal Wapp As String, ByVal Wcomp As String, ByVal WYear As String) As String
        Dim cnSql As SqlConnection
        Dim cmSql As SqlCommand
        Dim drsql As SqlDataReader
        Dim strsql As String
        Try
            cnSql = New SqlConnection(ConnectionString)
            cnSql.Open()
            cmSql = New SqlCommand(strsql, cnSql)

            strsql = "Select ac_accyear from Com_AppComp Where Ac_Code='" & Wapp & "'"
            strsql &= " And Ac_Comp='" & Wcomp & "'"
            strsql &= " And Ac_Year='" & WYear & "'"

            cmSql.CommandText = strsql
            drsql = cmSql.ExecuteReader()

            If drsql.Read Then
                ACYear = drsql.Item(0).ToString
            Else
                MessageBox.Show("Application " + Wapp + ", Company " + gbComp + ", Year " + WYear + " Not Defined In Appl-Company File !", CntTitle, MessageBoxButtons.OK, MessageBoxIcon.Exclamation)
            End If

        Catch ex As Exception
            Throw ex
        Finally
            If Not drsql Is Nothing Then
                If drsql.IsClosed = False Then drsql.Close()
                drsql = Nothing
            End If

            cmSql = Nothing

            If Not cnSql Is Nothing Then
                If cnSql.State = ConnectionState.Open Then cnSql.Close()
                cnSql = Nothing
            End If
        End Try

    End Function
    Public Sub ClearControls(ByRef _Control As Control, ByRef _ErrorProvider As ErrorProvider, _
       ByVal ParamArray _Array() As Control)

        Dim sTag As String

        Dim sCtrl As Control

        Try
            For Each sCtrl In _Control.Controls

                If Array.IndexOf(_Array, sCtrl) < 0 AndAlso (sCtrl.Tag Is Nothing _
                   OrElse sCtrl.Tag.ToString.Trim <> "Negation") Then

                    If (sCtrl.GetType Is GetType(TextBox) OrElse sCtrl.GetType Is GetType(RichTextBox) OrElse sCtrl.GetType Is GetType(TextBoxX) OrElse _
                       (sCtrl.GetType Is GetType(Label) AndAlso Not sCtrl.Tag Is Nothing)) Then

                        If sCtrl.GetType Is GetType(Label) Then
                            If Not sCtrl.Tag Is Nothing Then
                                If sCtrl.Tag Is "TextBox" Then
                                    sCtrl.Text = ""
                                ElseIf sCtrl.Tag.ToString.StartsWith("|") = False Then
                                    sCtrl.Text = sCtrl.Tag.ToString
                                Else
                                    sCtrl.Tag = Nothing
                                End If
                            End If
                        Else
                            sCtrl.Text = ""
                            If Not sCtrl.Tag Is Nothing AndAlso sCtrl.Tag.ToString <> "String" AndAlso _
                            sCtrl.Tag.ToString <> "Integer" AndAlso _
                            sCtrl.Tag.ToString <> "Decimal" Then
                                sCtrl.Tag = Nothing
                            End If
                        End If

                        If Not _ErrorProvider Is Nothing Then _ErrorProvider.SetError(sCtrl, "")

                    ElseIf sCtrl.GetType Is GetType(Button) AndAlso (sCtrl.Tag Is Nothing _
                        OrElse sCtrl.Tag.ToString.Trim <> "Visible") Then
                        sCtrl.Tag = Nothing

                    ElseIf sCtrl.GetType Is GetType(CheckBox) Then
                        With CType(sCtrl, CheckBox)
                            .Tag = Nothing : .Checked = False
                        End With

                    ElseIf sCtrl.GetType Is GetType(RadioButton) Then
                        With CType(sCtrl, RadioButton)
                            .Checked = False : .Tag = Nothing
                        End With

                    ElseIf sCtrl.GetType Is GetType(C1DateEdit) AndAlso sCtrl.Enabled = True Then

                        CType(sCtrl, C1DateEdit).Value = DBNull.Value
                        If Not _ErrorProvider Is Nothing Then _ErrorProvider.SetError(sCtrl, "")
                    ElseIf sCtrl.GetType Is GetType(DateTimeInput) AndAlso sCtrl.Enabled = True Then

                        CType(sCtrl, DateTimeInput).Text = ""
                        If Not _ErrorProvider Is Nothing Then _ErrorProvider.SetError(sCtrl, "")

                    ElseIf sCtrl.Controls.Count > 0 AndAlso _
                    Not sCtrl.GetType Is GetType(C1TrueDBGrid) Then
                        ClearControls(sCtrl, _ErrorProvider, _Array)
                    End If
                End If

            Next sCtrl

        Catch ex As Exception
            Throw ex
        Finally
            sCtrl = Nothing
        End Try
    End Sub
    Public Sub GetParam(ByVal Criteria As String)
        Dim cnSql As SqlConnection
        Dim cmSql As SqlCommand
        Dim strsql As String
        Dim drSql As SqlDataReader
        Try
            cnSql = New SqlConnection(ConnectionString)
            cnSql.Open()
            cmSql = New SqlCommand(strsql, cnSql)

            strsql = "SELECT [App_Param2],[App_Param3],[App_Param4] FROM Com_Application "
            strsql &= " WHERE App_Code='" & Criteria & "'"

            cmSql.CommandText = strsql
            drSql = cmSql.ExecuteReader

            With drSql
                If .HasRows Then
                    .Read()
                    Param2 = .Item("App_Param2").ToString
                    Param3 = .Item("App_Param3").ToString
                    If .Item("App_Param4").ToString = "Y" Then
                        CAudit = True
                    Else
                        CAudit = False
                    End If
                End If
            End With
        Catch ex As Exception
            Throw ex
        Finally
            If Not drSql Is Nothing Then
                If drSql.IsClosed = False Then drSql.Close()
                drSql = Nothing
            End If

            cmSql = Nothing

            If Not cnSql Is Nothing Then
                If cnSql.State = ConnectionState.Open Then cnSql.Close()
                cnSql = Nothing
            End If
        End Try
    End Sub
    Public Sub GetCurrentLoc()
        Dim cnSql As SqlConnection
        Dim cmSql As SqlCommand
        Dim strsql As String
        Dim drSql As SqlDataReader
        Try
            cnSql = New SqlConnection(ConnectionString)
            cnSql.Open()
            cmSql = New SqlCommand(strsql, cnSql)

            strsql = "SELECT Loc_Code,Loc_Desc FROM STK04_LOCATION WHERE Loc_Type1 = 'H'"

            cmSql.CommandText = strsql
            drSql = cmSql.ExecuteReader

            With drSql
                If .HasRows Then
                    .Read()
                    sCurLoc = .Item("Loc_Desc").ToString
                    sCurCodeLoc = .Item("Loc_Code").ToString
                End If
            End With
        Catch ex As Exception
            Throw ex
        Finally
            If Not drSql Is Nothing Then
                If drSql.IsClosed = False Then drSql.Close()
                drSql = Nothing
            End If

            cmSql = Nothing

            If Not cnSql Is Nothing Then
                If cnSql.State = ConnectionState.Open Then cnSql.Close()
                cnSql = Nothing
            End If
        End Try
    End Sub
    'Added by Abir T. @ 09.08.2012
    Public Function InsertRemotely(ByVal stSQL As String, ByVal CurConn As SqlConnection) As Boolean
        'this function is used to insert the same record inserted in the
        'current db into dbs of all companies

        Dim sSQL As String
        Dim MyConn As SqlConnection
        Dim cmSQL2 As SqlCommand
        Dim drSQL2 As SqlDataReader
        Dim cmSQL3 As SqlCommand

        'Foreign Databse parameters
        Dim sServer As String   'remote server
        Dim sAuth As String     'remote authentication S/N
        Dim sDB As String       'remote DB
        Dim sUser As String     'sql login user
        Dim sPwd As String      'sql password

        InsertRemotely = False
        Try
            Cursor.Current = Cursors.WaitCursor
            sSQL = "SELECT Comp_Code FROM Com_Company "
            sSQL = sSQL + " WHERE Comp_Code <> '" + gbComp + "'"
            Dim dt As New DataTable
            Dim da As New SqlDataAdapter(sSQL, CurConn)
            da.Fill(dt)
            If dt.Rows.Count > 0 Then
                For RowIdx As Integer = 0 To dt.Rows.Count - 1
                    sSQL = "SELECT SSS_StkServName, SSS_StkWinAuth, SSS_StkDBName, "
                    sSQL = sSQL + " SSS_StkUser, SSS_StkPwd FROM RLM01_ServSyncSys"
                    sSQL = sSQL + " WHERE SSS_CoCode = '" & dt.Rows(RowIdx)("Comp_Code").ToString & "'"
                    cmSQL2 = New SqlCommand(sSQL, CurConn)
                    drSQL2 = cmSQL2.ExecuteReader
                    With drSQL2
                        If Not .HasRows Then
                            InsertRemotely = False
                        Else
                            .Read()
                            If Trim(.Item("SSS_StkServName").ToString) <> gbServer Or .Item("SSS_StkDBName").ToString <> gbDBname Then
                                sServer = .Item("SSS_StkServName").ToString
                                sAuth = .Item("SSS_StkWinAuth").ToString
                                sDB = .Item("SSS_StkDBName").ToString
                                sUser = .Item("SSS_StkUser").ToString
                                sPwd = REDACTED"SSS_StkPwd").ToString

                                MyConn = New SqlConnection()
                                If sAuth = "N" Then
                                    'MyConn.ConnectionString = "Provider=SQLOLEDB.1;" + _
                                    '"Persist Security Info=False;User ID=" + sUser + ";pwd=" & sPwd & ";" + _
                                    '"Initial Catalog=" + sDB + ";Data Source=" + sServer + ";" + _
                                    '"Connect Timeout=10;Current Languge=English;" + _
                                    '"Packet Size=8192;Application Name=Accounting Package"
                                    MyConn.ConnectionString = "Data Source=" + sServer + ";Initial Catalog=" + sDB + ";User ID=" + sUser + ";Password=" + sPwd
                                Else
                                    'MyConn.ConnectionString = "Provider=SQLOLEDB.1;" + _
                                    '"Integrated Security=SSPI;" + _
                                    '"Persist Security Info=False;Initial Catalog=" + sDB + ";" + _
                                    '"Data Source=" + sServer
                                    MyConn.ConnectionString = "Data Source=" + sServer + ";Initial Catalog=" + sDB + ";Integrated Security=True"
                                End If
                                MyConn.Open()
                                cmSQL3 = New SqlCommand(stSQL, MyConn)
                                cmSQL3.CommandTimeout = 1000
                                cmSQL3.ExecuteNonQuery()

                                cmSQL3 = Nothing
                                If Not MyConn Is Nothing Then
                                    If MyConn.State = ConnectionState.Open Then MyConn.Close()
                                    MyConn.Dispose()
                                    MyConn = Nothing
                                End If
                            End If
                            If Not drSQL2 Is Nothing Then
                                If drSQL2.IsClosed = False Then drSQL2.Close()
                                drSQL2 = Nothing
                            End If
                            cmSQL2 = Nothing
                        End If
                    End With
                Next
ContinueInsert:
                InsertRemotely = True
            Else
                InsertRemotely = False
            End If
        Catch ex As Exception
            If Err.Number = -2147217873 Then
                GoTo ContinueInsert
            ElseIf Err.Number <> -2147217865 Then  'case if table doesnot exist ,no error msgbox
                MessageBox.Show("Error Occured, Description: " & ex.Message, CntTitle, _
                MessageBoxButtons.OK, MessageBoxIcon.Error)
            End If
            InsertRemotely = False
        Finally
            If Not drSQL2 Is Nothing Then
                If drSQL2.IsClosed = False Then drSQL2.Close()
                drSQL2 = Nothing
            End If
            cmSQL2 = Nothing
            cmSQL3 = Nothing
            If Not MyConn Is Nothing Then
                If MyConn.State = ConnectionState.Open Then MyConn.Close()
                MyConn.Dispose()
                MyConn = Nothing
            End If
        End Try
        Exit Function
    End Function
    'Added by Abir T. @ 10.08.2012
    Public Function GetRecId(ByVal Table1 As String, ByVal Field As String, Optional ByVal StrCond As String = "") As Double
        Dim strs As String
        Dim cnSQL As SqlConnection
        Dim cmSQL As SqlCommand
        Dim drSQL As SqlDataReader
        Try
            strs = "Select isnull(Max(" & Field & "),0) + 1  from " & Table1
            If StrCond > "" Then strs = strs + " " + StrCond

            cnSQL = New SqlConnection(ConnectionString)
            cnSQL.Open()
            cmSQL = New SqlCommand(strs, cnSQL)
            drSQL = cmSQL.ExecuteReader
            drSQL.Read()

            GetRecId = CDbl(drSQL.Item(0))
        Catch exp As Exception
            MessageBox.Show("Error : " & exp.Message, CntTitle, _
            MessageBoxButtons.OK, MessageBoxIcon.Error)
        Finally
            If Not drSQL Is Nothing Then
                If drSQL.IsClosed = False Then drSQL.Close()
                drSQL = Nothing
            End If
            cmSQL = Nothing
            If Not cnSQL Is Nothing Then
                If cnSQL.State = ConnectionState.Open Then cnSQL.Close()
                cnSQL = Nothing
            End If
        End Try
    End Function
    Public Sub RefreshBackGrd(ByRef _frm As Form, ByRef _Prx As String, ByRef _strsql As String)
        Dim sBackPath As String = Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData) & "\TEMP"

        Dim cnSql As SqlConnection
        Dim cmSql As SqlCommand

        Dim drSql As SqlDataReader

        Dim ms As MemoryStream

        Dim arrPicture() As Byte

        Try

            If File.Exists(sBackPath) Then File.Delete(sBackPath)

            If Directory.Exists(sBackPath) = False Then
                Directory.CreateDirectory(sBackPath)
            End If : sBackPath &= "\BackgroundImage"

            If Directory.Exists(sBackPath) = False Then Directory.CreateDirectory(sBackPath)

            cnSql = New SqlConnection(ConnectionString)
            cnSql.Open()

            cmSql = New SqlCommand(_strsql, cnSql)

            drSql = cmSql.ExecuteReader

            With drSql
                If .HasRows Then
                    .Read()

                    _Prx = .Item("Prx").ToString.Trim

                    If Not .Item("Img") Is Nothing Then
                        If .Item("Img") Is DBNull.Value Then
                            _frm.BackgroundImage = Nothing
                        Else

                            arrPicture = CType(drSql.Item("Img"), Byte())

                            ms = New MemoryStream(arrPicture)

                            sBackPath &= "\" & _Prx '& ".BMP"

                            '  Image.FromStream(ms).Save(sBackPath)

                            ' _frm.BackgroundImage = Image.FromFile(sBackPath)

                            _frm.BackgroundImageLayout = ImageLayout.Stretch

                            _frm.BackgroundImage = New Bitmap(Image.FromStream(ms))

                            ms.Close() : ms = Nothing

                        End If
                    Else
                        _frm.BackgroundImage = Nothing
                    End If
                End If
                .Close()
            End With

            cnSql.Close()

        Catch ex As Exception
            Throw ex
        Finally
            If Not ms Is Nothing Then
                ms.Close()
                ms = Nothing
            End If
            Disposecontrols(cnSql, cmSql, drSql)
        End Try
    End Sub

    'By Haitham Timani on 05.September.2012
    Public Function ReadFromDBToDataTable(ByVal StrSql As String) As DataTable
        Dim Conn As New SqlConnection(ConnectionString)
        Dim DA As SqlDataAdapter
        Dim DT As New DataTable
        Try
            Conn.Open()
            DA = New SqlDataAdapter(StrSql, Conn)
            DA.Fill(DT)
            ReadFromDBToDataTable = DT
        Catch
            MessageBox.Show(Err.Description, "Load", MessageBoxButtons.OK, MessageBoxIcon.Error)
            ReadFromDB = Nothing
        Finally
            Disposecontrols(Conn, , , , , DA)
        End Try
    End Function
    'By Haitham Timani on 05.September.2012
    Public Sub ReadFromDBToDataSet(ByVal StrSql As String, ByVal TableName As String, ByRef DS As DataSet)
        Dim Conn As New SqlConnection(ConnectionString)
        Dim DA As SqlDataAdapter
        Try
            Conn.Open()
            DA = New SqlDataAdapter(StrSql, Conn)
            DS = New DataSet
            DA.Fill(DS, TableName)
        Catch
            MessageBox.Show(Err.Description, "Load", MessageBoxButtons.OK, MessageBoxIcon.Error)
        Finally
            Disposecontrols(Conn, , , , , DA)
        End Try
    End Sub

    'Added By toufic Since This Priview Function Is used in a transaction and TimeOut Exeption Occ 
    Public Sub ReadFromDBToDataReaderWithconnection(ByVal StrSql As String, ByRef DR As System.Data.SqlClient.SqlDataReader, Optional cmsql As SqlCommand = Nothing)
        Dim Conn As New SqlConnection(ConnectionString)
        Dim CM As System.Data.SqlClient.SqlCommand
        Try
            Conn.Open()
            If cmsql Is Nothing Then
                CM = New System.Data.SqlClient.SqlCommand(StrSql, Conn)
            Else
                CM = cmsql
                CM.CommandText = StrSql
            End If
            DR = CM.ExecuteReader()
        Catch
            MessageBox.Show(Err.Description, "Load", MessageBoxButtons.OK, MessageBoxIcon.Error)
        Finally
            'Conn.Close()
        End Try
    End Sub



    'By Haitham Timani on 06.September.2012
    Public Sub ReadFromDBToDataReader(ByVal StrSql As String, ByRef DR As System.Data.SqlClient.SqlDataReader)
        Dim Conn As New SqlConnection(ConnectionString)
        Dim CM As System.Data.SqlClient.SqlCommand
        Try
            Conn.Open()
            CM = New System.Data.SqlClient.SqlCommand(StrSql, Conn)
            DR = CM.ExecuteReader()
            Conn.ClearAllPools() 'added by GS on 09/03/16
        Catch
            MessageBox.Show(Err.Description, "Load", MessageBoxButtons.OK, MessageBoxIcon.Error)
        Finally

        End Try
    End Sub

    'By Haitham Timani on .September.2012
    Public Sub Execute(ByVal ParamArray SqlStatments() As String)
        Dim Conn As New SqlConnection(ConnectionString)
        Dim Com As SqlCommand
        Dim Transaction As SqlTransaction
        Try
            Conn.Open()
            Transaction = Conn.BeginTransaction()
            Com = New SqlCommand(strsql, Conn, Transaction)
            Com.CommandTimeout = 1000
            For Idx As Integer = 0 To SqlStatments.Count - 1
                Com.CommandText = SqlStatments(Idx)
                Com.ExecuteNonQuery()
            Next
            Transaction.Commit()
        Catch e As Exception
            Transaction.Rollback()
            Throw e
            'MessageBox.Show(Err.Description, "Execute", MessageBoxButtons.OK, MessageBoxIcon.Error)
        Finally
            If Conn.State = ConnectionState.Open Then Conn.Close()
            Conn.ClearAllPools()
            Disposecontrols(Conn, Com, , Transaction)
        End Try
    End Sub
    Public Function ExecuteScalar_WOXACT(ByVal SqlStatment As String) As String
        Try

            Using Conn As New SqlConnection(ConnectionString)
                Conn.Open()

                Using Com As New SqlCommand(SqlStatment, Conn)
                    ExecuteScalar_WOXACT = IIf(Com.ExecuteScalar() Is Nothing, "", Com.ExecuteScalar()).ToString
                End Using

                Conn.Close()
            End Using

        Catch e As Exception
            Throw e
        End Try
    End Function
    Public Function ExecuteScalar(ByVal SqlStatments As String, Optional ByVal _cmSql As SqlCommand = Nothing) As String
        Dim Conn As New SqlConnection(ConnectionString)
        Dim Com As SqlCommand
        Dim Transaction As SqlTransaction
        Dim oObj As Object
        Try
            'Conn.Open()
            'Transaction = Conn.BeginTransaction()
            'Com = New SqlCommand(strsql, Conn, Transaction)
            If Not _cmSql Is Nothing Then
                Com = _cmSql
            Else
                Conn.Open()
                Transaction = Conn.BeginTransaction()
                Com = New SqlCommand(SqlStatments, Conn, Transaction)
            End If
            Com.CommandText = SqlStatments
            'edited by GS on 05/04/16
            'ExecuteScalar = Com.ExecuteScalar().ToString
            oObj = Com.ExecuteScalar()
            If Not oObj Is Nothing AndAlso Not oObj Is DBNull.Value Then
                ExecuteScalar = oObj.ToString
            Else
                ExecuteScalar = ""
            End If
            'GS on 05/04/16
            If _cmSql Is Nothing Then Transaction.Commit()
        Catch e As Exception
            Transaction.Rollback()
            Throw e
            'MessageBox.Show(Err.Description, "Execute", MessageBoxButtons.OK, MessageBoxIcon.Error)
        Finally
            Disposecontrols(Conn, Com, , Transaction)
        End Try
    End Function
    Public Property UserPrint As String
        Get
            UserPrint = gbuserprint
        End Get
        Set(ByVal value As String)
            gbuserprint = value
        End Set
    End Property
    Public Function DecimalValid(ByVal x As Integer, ByVal y As Integer, ByRef txtbox As TextBox) As Boolean
        Dim i, j As Integer
        Try
            DecimalValid = True
            If txtbox.Text.Trim = "" Then Exit Function

            If CDbl(txtbox.Text.Trim).ToString.IndexOf("E") <> -1 Then
                DecimalValid = False
                Exit Function
            End If

            If CDbl(txtbox.Text.Trim).ToString.IndexOf(".") <> -1 Then
                i = CDbl(txtbox.Text.Trim).ToString.IndexOf(".")
                j = CDbl(txtbox.Text.Trim).ToString.Length - CDbl(txtbox.Text.Trim).ToString.IndexOf(".") - 1
            Else
                i = Len(CDbl(txtbox.Text.Trim).ToString)
                j = 0
            End If
            'Test if number of decimal is greater than the specified
            If j > y Then
                DecimalValid = False
                Exit Function
            End If
            If i <> 0 Then
                If i > (x - y) Then
                    DecimalValid = False
                End If
            End If
        Catch ex As Exception
            Throw ex
        End Try
    End Function

    Public Function DiffTest(ByVal FormName As String, ByVal OBJText1 As Object, ByVal OBJText2 As Object, ByVal Typ As String, Optional ByVal Mess As String = "", Optional ByVal Op As String = "", Optional ByVal Index As Integer = 0) As Boolean
        Dim txt1 As TextBox
        Dim txt2 As TextBox
        txt1 = CType(OBJText1, TextBox)
        txt2 = CType(OBJText2, TextBox)
        Try
            If Typ = "D" And IsDate(txt1.Text) And IsDate(txt2.Text) Then
                If Op = "" Then If CType(txt1.Text, Date) > CType(txt2.Text, Date) Then DiffTest = True
                If Op = "=" Then If CType(txt1.Text, Date) <> CType(txt2.Text, Date) Then DiffTest = True
            ElseIf Typ = "TI" And IsDate(txt1.Text) And IsDate(txt2.Text) Then
                If CDate(Format(txt1.Text, "HH:MM")) > CDate(Format(txt2.Text, "HH:MM")) Then DiffTest = True
            ElseIf Typ = "DB" Then
                If CDbl(txt1.Text) > CDbl(txt2.Text) Then DiffTest = True
            Else
                If Typ <> "D" And txt1.Text > txt2.Text Then DiffTest = True
            End If

            If DiffTest = True Then
                If Mess = "" Then Mess = "Incorrect Criteria"
                MessageBox.Show(Mess, FormName, MessageBoxButtons.OK, MessageBoxIcon.Warning)
                If Index = 1 Then txt2.Focus() Else txt1.Focus()
            End If
            Exit Function

        Catch ex As Exception
            Throw ex
        End Try
    End Function

    Function GetSPRates(ByVal SP As String, ByVal WDate As Date, Optional ByVal WCurr As String = "") As Boolean
        Dim drSQL As SqlDataReader
        Dim strsql10 As String
        Try
            Wrate1 = 1 : Wrate2 = 1
            Wop1 = "*" : Wop2 = "*"

            'Get The Rates From DailyRate
            strsql10 = "Select * From Com_DailyRate Where "
            'strsql10 &= " convert(datetime,DR_Date,101)='" & Format(WDate, "Short Date") & "'"
            strsql10 &= " convert(datetime,DR_Date,101)='" & Format(WDate, "MM/dd/yyyy") & "'"
            If WCurr <> "" Then
                strsql10 = strsql10 + " And DR_CurCode='" & WCurr & "'"
            End If
            ReadFromDBToDataReader(strsql10, drSQL)
            With drSQL
                If .HasRows Then
                    .Read()
                    Wrate1 = CDbl(.Item("DR_Rate1LC"))
                    Wrate2 = CDbl(.Item("DR_Rate1CC"))
                    Wop1 = .Item("DR_Op1LC").ToString
                    Wop2 = .Item("DR_Op1CC").ToString
                    GetSPRates = True
                Else
                    GetSPRates = False
                End If
            End With
            If Wrate1 = 0 Then Wrate1 = 1
            If Wrate2 = 0 Then Wrate2 = 1
        Catch ex As Exception
            Throw ex
        Finally
            If Not drSQL Is Nothing Then
                If drSQL.IsClosed = False Then drSQL.Close()
                drSQL = Nothing
            End If
        End Try
    End Function
    'Public Function convertSP(ByVal SP As String, ByVal mnt As Double, ByVal curfrom As String, ByVal curto As String, ByVal sDate As Date) As Double

    '    If curfrom = curto Then
    '        convertSP = mnt
    '        Exit Function
    '    End If

    '    If curfrom = gbCurr1 Then
    '        If GetSPRates(SP, CDate(Format(sDate, "dd/MM/yyyy")), curto) = False Then
    '            MsgBox(curto + " Currency Rates Not Defined For The Date Of " + Format(sDate, "dd/MM/yyyy"), vbExclamation, "No Processing")
    '            Exit Function
    '        Else
    '            convertSP = CDbl(IIf(Wop1 = "*", mnt / Wrate1, mnt * Wrate1))
    '        End If

    '    ElseIf curfrom = gbCurr2 Then
    '        If curto = gbCurr1 Then
    '            If GetSPRates(SP, CDate(Format(sDate, "dd/mm/yyyy")), curfrom) = False Then
    '                MsgBox(curfrom + " Currency Rates Not Defined For The Date Of " + Format(sDate, "dd/mm/yyyy"), vbExclamation, "No Processing")
    '                RSTAT = True
    '                Exit Function
    '            Else
    '                convertSP = CDbl(IIf(Wop1 = "*", mnt * Wrate1, mnt / Wrate1))
    '            End If
    '        Else
    '            If GetSPRates(SP, CDate(Format(sDate, "dd/mm/yyyy")), curto) = False Then
    '                MsgBox(curto + " Currency Rates Not Defined For The Date Of " + Format(sDate, "dd/mm/yyyy"), vbExclamation, "No Processing")
    '                RSTAT = True
    '                Exit Function
    '            Else
    '                convertSP = CDbl(IIf(Wop2 = "*", mnt / Wrate2, mnt * Wrate2))
    '            End If
    '        End If
    '    Else
    '        If GetSPRates(SP, CDate(Format(sDate, "dd/mm/yyyy")), curfrom) = False Then
    '            MsgBox(curfrom + " Currency Rates Not Defined For The Date Of " + Format(sDate, "dd/mm/yyyy"), vbExclamation, "No Processing")
    '            RSTAT = True
    '            Exit Function
    '        Else
    '            If curto = gbCurr1 Then
    '                convertSP = CDbl(IIf(Wop1 = "*", mnt * Wrate1, mnt / Wrate1))
    '            Else
    '                convertSP = CDbl(IIf(Wop2 = "*", mnt * Wrate2, mnt / Wrate2))
    '                If curto <> gbCurr2 Then
    '                    If GetSPRates(SP, CDate(Format(sDate, "dd/mm/yyyy")), curto) = False Then
    '                        MsgBox(curto + " Currency Rates Not Defined For The Date Of " + Format(sDate, "dd/mm/yyyy"), vbExclamation, "No Processing")
    '                        RSTAT = True
    '                        Exit Function
    '                    Else
    '                        convertSP = convertSP(SP, convertSP(SP, mnt, curfrom, gbCurr2, CDate(Format(Now, "mm/dd/yyyy"))), gbCurr2, curto, CDate(Format(Now, "mm/dd/yyyy")))
    '                    End If
    '                End If
    '            End If
    '        End If
    '    End If
    'End Function
    Public Function convertSP(ByVal SP As String, ByVal mnt As Double, ByVal curfrom As String, ByVal curto As String, ByVal sDate As Date, ByVal FormName As String) As Double
        Try
            If curfrom = curto Then
                convertSP = mnt
                Exit Function
            End If

            If curfrom = gbCurr1 Then
                If GetSPRates(SP, CDate(CType(sDate, Date).ToString("dd/MM/yyyy")), curto) = False Then
                    MessageBox.Show(curto + " Currency Rates Not Defined " + CType(sDate, Date).ToString("dd/MM/yyyy"), FormName, MessageBoxButtons.OK, MessageBoxIcon.Exclamation)
                    convertSP = 0
                    Exit Function
                Else
                    convertSP = CDbl(IIf(Wop1 = "*", mnt / Wrate1, mnt * Wrate1))
                End If

            ElseIf curfrom = gbCurr2 Then
                If curto = gbCurr1 Then
                    If GetSPRates(SP, CDate(CType(sDate, Date).ToString("dd/MM/yyyy")), curfrom) = False Then
                        MessageBox.Show(curfrom + " Currency Rates Not Defined " + CType(sDate, Date).ToString("dd/MM/yyyy"), FormName, MessageBoxButtons.OK, MessageBoxIcon.Exclamation)
                        convertSP = 0
                        Exit Function
                    Else
                        convertSP = CDbl(IIf(Wop1 = "*", mnt * Wrate1, mnt / Wrate1))
                    End If
                Else
                    If GetSPRates(SP, CDate(CType(sDate, Date).ToString("dd/MM/yyyy")), curto) = False Then
                        MessageBox.Show(curto + " Currency Rates Not Defined " + CType(sDate, Date).ToString("dd/MM/yyyy"), FormName, MessageBoxButtons.OK, MessageBoxIcon.Exclamation)
                        convertSP = 0
                        Exit Function
                    Else
                        'Rachel A. 01.03.2016 
                        convertSP = CDbl(IIf(Wop2 = "*", mnt / CDbl(IIf(Wrate2 = 0, 1, Wrate2)), mnt * Wrate2))
                        ' convertSP = CDbl(IIf(Wop2 = "*", mnt / Wrate2, mnt * Wrate2))
                    End If
                End If

            Else
                If GetSPRates(SP, CDate(CType(sDate, Date).ToString("dd/MM/yyyy")), curfrom) = False Then
                    MessageBox.Show(curfrom + " Currency Rates Not Defined " + CType(sDate, Date).ToString("dd/MM/yyyy"), FormName, MessageBoxButtons.OK, MessageBoxIcon.Exclamation)
                    convertSP = 0
                    Exit Function
                Else
                    If curto = gbCurr1 Then
                        convertSP = CDbl(IIf(Wop1 = "*", mnt * Wrate1, mnt / Wrate1))
                    Else
                        convertSP = CDbl(IIf(Wop2 = "*", mnt * Wrate2, mnt / Wrate2))
                        If curto <> gbCurr2 Then
                            If GetSPRates(SP, CDate(CType(sDate, Date).ToString("dd/MM/yyyy")), curto) = False Then
                                MessageBox.Show(curto + " Currency Rates Not Defined " + CType(sDate, Date).ToString("dd/MM/yyyy"), CntTitle, MessageBoxButtons.OK, MessageBoxIcon.Exclamation)
                                convertSP = 0
                                Exit Function
                            Else
                                convertSP = convertSP(SP, convertSP(SP, mnt, curfrom, gbCurr2, CDate(CType(Now, Date).ToString("dd/MM/yyyy")), FormName), gbCurr2, curto, CDate(CType(Now, Date).ToString("dd/MM/yyyy")), FormName)
                            End If
                        End If
                    End If
                End If
            End If
        Catch ex As Exception
            Throw ex
        End Try
    End Function

    Public Function convertWithRateOpr(ByVal mnt As Double, ByVal curfrom As String, ByVal curto As String, ByVal sdate As Date, rate1 As Double, rate2 As Double) As Double
        Try
            If curfrom = curto Then
                convertWithRateOpr = mnt
                Exit Function
            End If
            If curfrom = gbCurr1 Then
                If GetRates(CDate(CType(sdate, Date).ToString("dd/MM/yyyy")), curto) = False Then
                    MsgBox(curto + " Currency Rates Not Defined For The Date Of " + CType(sdate, Date).ToString("dd/MM/yyyy"), vbExclamation, "No Processing")
                    convertWithRateOpr = 0
                    Exit Function
                Else
                    convertWithRateOpr = MathRound(CDbl(IIf(Wop1 = "*", mnt / rate1, mnt * rate1)), 3)
                End If
            ElseIf curfrom = gbCurr2 Then
                If curto = gbCurr1 Then
                    If GetRates(CDate(CType(sdate, Date).ToString("dd/MM/yyyy")), curfrom) = False Then
                        MsgBox(curfrom + " Currency Rates Not Defined For The Date Of " + CType(sdate, Date).ToString("dd/MM/yyyy"), vbExclamation, "No Processing")
                        convertWithRateOpr = 0
                        Exit Function
                    Else
                        convertWithRateOpr = MathRound(CDbl(IIf(Wop1 = "*", mnt * rate1, mnt / rate1)), 3)
                    End If
                Else
                    If GetRates(CDate(CType(sdate, Date).ToString("dd/MM/yyyy")), curto) = False Then
                        MsgBox(curto + " Currency Rates Not Defined For The Date Of " + CType(sdate, Date).ToString("dd/MM/yyyy"), vbExclamation, "No Processing")
                        convertWithRateOpr = 0
                        Exit Function
                    Else
                        convertWithRateOpr = MathRound(CDbl(IIf(Wop2 = "*", mnt / rate2, mnt * rate2)), 3)
                    End If
                End If
            Else
                If GetRates(CDate(CType(sdate, Date).ToString("dd/MM/yyyy")), curfrom) = False Then
                    MsgBox(curfrom + " Currency Rates Not Defined For The Date Of " + CType(sdate, Date).ToString("dd/MM/yyyy"), vbExclamation, "No Processing")
                    convertWithRateOpr = 0
                    Exit Function
                Else
                    If curto = gbCurr1 Then
                        convertWithRateOpr = MathRound(CDbl(IIf(Wop1 = "*", mnt * rate1, mnt / rate1)), 3)
                    Else
                        convertWithRateOpr = MathRound(CDbl(IIf(Wop2 = "*", mnt * rate2, mnt / rate2)), 3)
                        If curto <> gbCurr2 Then
                            If GetRates(CDate(CType(sdate, Date).ToString("dd/MM/yyyy")), curto) = False Then
                                MsgBox(curto + " Currency Rates Not Defined For The Date Of " + CType(sdate, Date).ToString("dd/MM/yyyy"), vbExclamation, "No Processing")
                                convertWithRateOpr = 0
                                Exit Function
                            Else
                                convertWithRateOpr = MathRound(CDbl(convert5(convert5(mnt, curfrom, gbCurr2, CDate(Format(Now, "dd/mm/yyyy"))), gbCurr2, curto, CDate(Format(Now, "dd/mm/yyyy")))), 3)
                            End If
                        End If
                    End If
                End If
            End If
        Catch ex As Exception
            Throw ex
        End Try
    End Function
    'Public Function PORD2_GETPROD(ByVal RMS_SUPREF As String, ByVal RMS_SUPrECID As String, _
    '            ByVal RMS_Comp As String, ByVal RMS_Recid As String) As Boolean
    '    Dim drSQL As SqlDataReader
    '    Try
    '        '--..continue
    '        strsql = "EXEC P_PORD2_DuplSupItm"
    '        strsql &= " '" & RMS_SUPREF.Trim.Replace("'", "''") & "' ,'" & RMS_SUPrECID.Trim.Replace("'", "''") & "',"
    '        strsql &= " '" & RMS_Comp.Trim.Replace("'", "''") & "' ,'" & Val(RMS_Recid.Trim.Replace("'", "''")) & "'"
    '        ReadFromDBToDataReader(strsql, drSQL)
    '        drSQL.Read()
    '        If CDbl(drSQL.Item(0)) > 0 Then
    '            MessageBox.Show("Duplicate Supplier Invoice Ref.", CntTitle, MessageBoxButtons.OK, MessageBoxIcon.Information)
    '            PORD2_GETPROD = True
    '        End If
    '    Catch ex As Exception
    '        Throw ex
    '    Finally
    '        If Not drSQL Is Nothing Then
    '            If drSQL.IsClosed = False Then drSQL.Close()
    '            drSQL = Nothing
    '        End If
    '    End Try
    'End Function
    Public Sub GetItmClassification()
        Dim strsql As String
        Try
            strsql = "SELECT SP_Trading,SP_Fashion,SP_Expiry,SP_Book,SP_DefaultStockTag,SP_POApproval, ISNULL(SP_MaxAdjustmentCC,0) SP_MaxAdjustmentCC,"
            strsql &= " ISNULL(SP_2PurchaseSerNo,'N') SP_2PurchaseSerNo , isnull(SP_ShowBrandPOPOI,'N')   SP_ShowBrandPOPOI FROM dbo.stk04_SysParam"

            Dim dt As DataTable = ReadFromDBToDataTable(strsql)

            If dt.Rows.Count > 0 Then
                gbTrading = CBool(IIf(dt(0)("SP_Trading").ToString = "Y", True, False))
                gbFashion = CBool(IIf(dt(0)("SP_Fashion").ToString = "Y", True, False))
                gbExpiry = CBool(IIf(dt(0)("SP_Expiry").ToString = "Y", True, False))
                gbBook = CBool(IIf(dt(0)("SP_Book").ToString = "Y", True, False))
                gbStockTag = dt(0)("SP_DefaultStockTag").ToString
                GBHOCntrlApproved = CBool(IIf(dt(0)("SP_POApproval").ToString = "3", True, False))
                MaxAdjustmentCC = CDbl(dt(0)("SP_MaxAdjustmentCC").ToString)
                PurchaseSerNo = dt(0)("SP_2PurchaseSerNo").ToString.ToUpper = "Y"
                ShowBrandPOPI = dt(0)("SP_ShowBrandPOPOI").ToString.ToUpper = "Y"
            End If
        Catch ex As Exception
            Throw ex
        End Try
    End Sub
    Public Sub InsertMissingRecQty(ByVal cmSql As SqlCommand, ByVal lcItemClass As String, ByVal cLoc As String, _
                   ByVal StkRecId As Long, ByVal Grid As String, ByVal Size As String, ByVal Color As String, ByVal ExpDate As String, ByVal BatchNo As String, _
                   Optional ProductionDate As String = "")
        Try

            If UCase(lcItemClass) = "F" And (Grid = "" Or Size = "" Or Color = "") Then Exit Sub
            If UCase(lcItemClass) = "E" And (ExpDate = "" Or BatchNo = "") Then Exit Sub

            strsql = " if NOT exists (SELECT STQ_STKRECID FROM dbo.stk04_STKQTY WHERE "
            strsql &= " STQ_STKRECID='" & StkRecId & "' AND STQ_LOCCODE='" & cLoc & "'"
            If UCase(lcItemClass) = "T" Or UCase(lcItemClass) = "B" Then
                strsql &= " ) BEGIN"
                strsql &= " Declare @RecId as NUMERIC"
                strsql &= " select @RecId = ISNULL(MAX(STQ_RecId), 0) + 1 FROM stk04_STKQTY"
                strsql &= " INSERT INTO dbo.stk04_STKQTY VALUES ('" & StkRecId & "','" & cLoc & "',0,GETDATE(),'O',0,0,GETDATE(),"
                strsql &= " '" & gbUser & "',@RecId,"
                strsql &= " Null,Null,Null,Null,Null,0,0"
                'Rita M @ 21.11.2016. Allow return sales
                strsql &= " ,null"
                'Rita M @ 21.11.2016. Processing in Production
                strsql &= " ,0"
                strsql &= "  , NULL  /* ProductionDt */  ) END"
            ElseIf UCase(lcItemClass) = "F" Then
                strsql &= " AND STQ_Color='" & Color & "' AND STQ_Grid='" & Grid & "' AND STQ_DetGrid='" & Size & "' "
                strsql &= " ) BEGIN"
                strsql &= " Declare @RecId as NUMERIC"
                strsql &= " select @RecId = ISNULL(MAX(STQ_RecId), 0) + 1 FROM stk04_STKQTY"
                strsql &= " INSERT INTO dbo.stk04_STKQTY VALUES ('" & StkRecId & "','" & cLoc & "',0,GETDATE(),'O',0,0,GETDATE(),"
                strsql &= " '" & gbUser & "',@RecId,"
                strsql &= " '" & Color & "','" & Grid & "','" & Size & "',Null,Null,0,0"
                'Rita M @ 21.11.2016. Allow return sales
                strsql &= " ,null"
                'Rita M @ 21.11.2016. Processing in Production
                strsql &= " ,0"
                strsql &= "  , NULL  /* ProductionDt */  ) END"
            ElseIf UCase(lcItemClass) = "E" Then
                strsql &= " AND STQ_ExpiryDt = '" & ExpDate & "' AND STQ_BatchInfo = '" & BatchNo & "'"
                strsql &= " ) BEGIN"
                strsql &= " Declare @RecId as NUMERIC"
                strsql &= " select @RecId = ISNULL(MAX(STQ_RecId), 0) + 1 FROM stk04_STKQTY"
                strsql &= " INSERT INTO dbo.stk04_STKQTY VALUES ('" & StkRecId & "','" & cLoc & "',0,GETDATE(),'O',0,0,GETDATE(),"
                strsql &= " '" & gbUser & "',@RecId,"
                strsql &= " Null,Null,Null,'" & ExpDate & "','" & BatchNo & "',0,0"
                'Rita M @ 21.11.2016. Allow return sales
                If cLoc = "00" Then
                    strsql &= " ,(SELECT STK_ReturnSale FROM stk04_stkfil WHERE stk_recid=@RecId) "
                Else
                    strsql &= " ,null"
                End If
                'Rita M @ 21.11.2016. Processing in Production
                strsql &= " ,0"
                If ProductionDate <> "" Then
                    strsql &= "  , '" & ProductionDate & "'  /* ProductionDt */ "
                Else
                    strsql &= "  , NULL  /* ProductionDt */ "
                End If
                strsql &= "  ) END"
            End If

            cmSql.CommandText = strsql
            cmSql.ExecuteNonQuery()

        Catch ex As Exception
            Throw ex
        End Try
    End Sub

    'Add new HS Code From Other Forms (Source)
    'Added by Abir T. @ 22.04.2013
    Public Sub InsertHSCode(ByVal cmSql As SqlCommand, ByVal HSCode As String, ByVal Source As String)
        Try
            strsql = " DECLARE @RecID AS NUMERIC"
            strsql &= " SELECT @RecID= ISNULL(MAX(HSC_Recid),0) + 1 FROM STK04_HSCode"
            strsql &= " INSERT INTO STK04_HSCode ( HSC_Recid ,HSC_HSChapter ,HSC_HSCode,"
            strsql &= " HSC_Country, HSC_CustomType ,HSC_CustomPerc ,HSC_CustomValLC ,"
            strsql &= " HSC_Source, HSC_CrDStamp ,HSC_CrUStamp ,HSC_DStamp ,HSC_UStamp)"
            'GetMaxRecId("STK04_HSCode", "HSC_Recid") 
            strsql &= " VALUES (@RecID, '" & Mid(HSCode, 1, 2) & "',"
            strsql &= " '" & HSCode & "', '' , 'V' , NULL , NULL ,"
            strsql &= " '" & Source & "', GETDATE() , '" & gbUser & "' , GETDATE() , '" & gbUser & "')"
            cmSql.CommandText = strsql
            cmSql.ExecuteNonQuery()
        Catch ex As Exception
            Throw ex
        End Try
    End Sub
    Public Function InsertPendItm(ByVal TempPending As String, ByVal Recid As String) As String
        Dim trnsql As SqlTransaction
        Dim cnSql As SqlConnection
        Dim cmSql As SqlCommand
        Dim Str As String
        Try
            InsertPendItm = ""
            cnSql = New SqlConnection(ConnectionString)
            cnSql.Open()
            trnsql = cnSql.BeginTransaction

            Str = "Select ISNULL(MAX(SFP_Recid),0) +1 FROM " & TempPending & ""
            cmSql = New SqlCommand(Str, cnSql, trnsql)
            InsertPendItm = CStr(cmSql.ExecuteScalar())

            Str = " IF NOT EXISTS (SELECT SFP_Recid FROM " & TempPending & " WHERE SFP_OldRecid = " & Val(Recid) & ")"
            Str &= " INSERT INTO " & TempPending
            Str &= " SELECT " & InsertPendItm & ", SFP_Comp,SFP_SUPPLIER,SFP_CATEGORY,SFP_Serie,SFP_Usage,SFP_NEWUSED,SFP_Season,SFP_Brand,"
            Str &= " SFP_Grid, SFP_Gender, SFP_Style, SFP_SingleRef1, SFP_SingleRef2, SFP_ItmClass, SFP_LOCSRC, SFP_InfDate, SFP_OpSource,"
            Str &= " SFP_Desc, SFP_ImpLoc, SFP_ItmIdent, SFP_SerReg, SFP_ItmType, SFP_UNIT, SFP_UNITPACK, SFP_SUBUNIT, SFP_FCCode, SFP_VATType,"
            Str &= " SFP_VATPER, SFP_StartTaxDate, SFP_CrUStamp, SFP_CrDStamp, SFP_UStamp, SFP_DStamp, SFP_HSCode ,SFP_VATSTAT,SFP_TechSheet"
            Str &= " , SFP_operator " 'ch @ 28/03/2017
            Str &= " , 'O', SFP_Recid  FROM STK04_StkFilPending"
            Str &= " WHERE SFP_Recid = '" & Val(Recid) & "'"
            cmSql = New SqlCommand(Str, cnSql, trnsql)
            cmSql.ExecuteNonQuery()

            trnsql.Commit()
        Catch ex As Exception
            Throw ex
            If Not trnsql Is Nothing Then trnsql.Rollback()
        Finally
            If Not trnsql Is Nothing Then trnsql = Nothing
        End Try
    End Function

#Region "Context Menu Procedures"
    Public Sub CreateContextMenu(ByRef _Control As Control)
        Dim sCtrl As Control
        Dim sConMenu As ContextMenu

        Try
            sConMenu = New ContextMenu

            With sConMenu.MenuItems
                .Add("&Copy", New System.EventHandler(AddressOf CopyMenu_Click))
                .Add("Cu&t", New System.EventHandler(AddressOf CutMenu_Click))
                .Add("&Paste", New System.EventHandler(AddressOf PastMenu_Click))
                .Add("-") : .Add("&Font", New System.EventHandler(AddressOf FontMenu_Click))
            End With

            For Each sCtrl In _Control.Controls
                If sCtrl.GetType Is GetType(TextBox) OrElse _
                sCtrl.GetType Is GetType(RichTextBox) OrElse _
                sCtrl.GetType Is GetType(C1DateEdit) OrElse _
                sCtrl.GetType Is GetType(TextBoxX) OrElse _
                sCtrl.GetType Is GetType(RichTextBoxEx) OrElse sCtrl.GetType Is GetType(DateTimeInput) Then
                    sCtrl.ContextMenu = sConMenu
                ElseIf sCtrl.Controls.Count > 0 Then
                    CreateContextMenu(sCtrl)
                End If
            Next sCtrl

        Catch ex As Exception
            Throw ex
        Finally
            sCtrl = Nothing : sConMenu = Nothing
        End Try
    End Sub

    Public Function SetContextMenu() As ContextMenu
        Dim sConMenu As ContextMenu

        Try
            sConMenu = New ContextMenu

            With sConMenu.MenuItems
                .Add("&Copy", New System.EventHandler(AddressOf CopyMenu_Click))
                .Add("Cu&t", New System.EventHandler(AddressOf CutMenu_Click))
                .Add("&Paste", New System.EventHandler(AddressOf PastMenu_Click))
                .Add("-") : .Add("&Font", New System.EventHandler(AddressOf FontMenu_Click))
            End With

            Return sConMenu

        Catch ex As Exception
            Throw ex
        Finally
            sConMenu = Nothing
        End Try
    End Function

    Private Sub CopyMenu_Click(ByVal sender As Object, ByVal e As System.EventArgs)
        Dim sCtrl As Control
        Try
            If gbRichTextSource Is Nothing Then
                sCtrl = CType(sender, MenuItem).GetContextMenu.SourceControl
                ctxMenuCopyCut(sCtrl, sCtrl.ContextMenu)
            Else
                ctxMenuCopyCut(CType(gbRichTextSource, Control), gbRichTextSource.ContextMenu)
            End If
        Catch ex As Exception
            MessageBox.Show(ex.Message, CntTitle & "- Error", MessageBoxButtons.OK, MessageBoxIcon.Error)
        Finally
            sCtrl = Nothing
        End Try
    End Sub

    Private Sub CutMenu_Click(ByVal sender As Object, ByVal e As System.EventArgs)
        Dim sCtrl As Control
        Try
            If gbRichTextSource Is Nothing Then
                sCtrl = CType(sender, MenuItem).GetContextMenu.SourceControl
                ctxMenuCopyCut(sCtrl, sCtrl.ContextMenu, True)
            Else
                ctxMenuCopyCut(CType(gbRichTextSource, Control), gbRichTextSource.ContextMenu, True)
            End If
        Catch ex As Exception
            MessageBox.Show(ex.Message, CntTitle & "- Error", MessageBoxButtons.OK, MessageBoxIcon.Error)
        Finally
            sCtrl = Nothing
        End Try
    End Sub

    Private Sub PastMenu_Click(ByVal sender As Object, ByVal e As System.EventArgs)
        Dim sCtrl As Control
        Dim grd As C1TrueDBGrid : Dim oObj As Object

        Try
            If gbRichTextSource Is Nothing Then
                sCtrl = CType(sender, MenuItem).GetContextMenu.SourceControl
                If sCtrl.GetType Is GetType(C1DateEdit) Then
                    ctxMenuPast(sCtrl, "Datetime")
                ElseIf sCtrl.GetType Is GetType(DateTimeInput) Then
                    ctxMenuPast(sCtrl, "Datetime")
                ElseIf sCtrl.GetType.ToString = "C1.Win.C1TrueDBGrid.GridEditor" Then
                    grd = CType(sCtrl.Parent, C1TrueDBGrid)
                    oObj = grd.Splits(0).DisplayColumns(grd.Col).DataColumn.Tag
                    ctxMenuPast(sCtrl, IIf(oObj Is Nothing, "String", oObj).ToString)
                Else
                    ctxMenuPast(sCtrl, IIf(sCtrl.Tag Is Nothing, "String", sCtrl.Tag).ToString)
                End If
            Else
                ctxMenuPast(CType(gbRichTextSource, Control), _
                IIf(gbRichTextSource.Tag Is Nothing, "String", gbRichTextSource.Tag).ToString)
            End If
        Catch ex As Exception
            MessageBox.Show(ex.Message, CntTitle & "- Error", MessageBoxButtons.OK, MessageBoxIcon.Error)
        Finally
            sCtrl = Nothing : grd = Nothing : oObj = Nothing
        End Try
    End Sub

    Private Sub FontMenu_Click(ByVal sender As Object, ByVal e As System.EventArgs)
        Dim sCtrl As Control

        Try

            If gbRichTextSource Is Nothing Then
                sCtrl = CType(sender, MenuItem).GetContextMenu.SourceControl

                If sCtrl.GetType Is GetType(C1TrueDBGrid) Then
                    ctxMenuFont(CType(sCtrl, C1TrueDBGrid))
                ElseIf sCtrl.GetType.ToString = "C1.Win.C1TrueDBGrid.GridEditor" Then
                    ctxMenuFont(CType(sCtrl.Parent, C1TrueDBGrid))
                Else
                    ctxMenuFont(CType(sCtrl, TextBox))
                End If

            Else
                ctxMenuFont(gbRichTextSource)
            End If

        Catch ex As Exception
            MessageBox.Show(ex.Message, CntTitle & "- Error", MessageBoxButtons.OK, MessageBoxIcon.Error)
        Finally
            sCtrl = Nothing
        End Try
    End Sub

    Public Sub ctxMenuFont(ByRef _Control As TextBox)
        Dim FontDialog1 As New FontDialog

        Try

            With FontDialog1
                .ShowColor = True

                .Font = _Control.Font
                .Color = _Control.ForeColor

                If .ShowDialog = DialogResult.OK Then
                    _Control.Font = .Font : _Control.ForeColor = .Color
                End If

            End With

        Catch ex As Exception
            Throw ex
        Finally
            FontDialog1.Dispose() : FontDialog1 = Nothing
        End Try
    End Sub

    Public Sub ctxMenuFont(ByRef _Control As C1TrueDBGrid)
        Dim FontDialog1 As New FontDialog

        Try

            With FontDialog1
                .ShowColor = True

                .Font = _Control.Splits(0).DisplayColumns(_Control.Col).Style.Font
                .Color = _Control.Splits(0).DisplayColumns(_Control.Col).Style.ForeColor

                If .ShowDialog = DialogResult.OK Then

                    _Control.Controls(2).Font = .Font
                    _Control.Controls(2).ForeColor = .Color

                    _Control.Splits(0).DisplayColumns(_Control.Col).Style.Font = .Font
                    _Control.Splits(0).DisplayColumns(_Control.Col).Style.ForeColor = .Color

                End If

            End With

        Catch ex As Exception
            Throw ex
        Finally
            FontDialog1.Dispose() : FontDialog1 = Nothing
        End Try
    End Sub

    Public Sub ctxMenuFont(ByRef _Control As RichTextBox)
        Dim FontDialog1 As New FontDialog

        Try
            With FontDialog1
                .ShowColor = True

                'If _Control.SelectionLength > 0 Then
                .Font = _Control.SelectionFont
                .Color = _Control.SelectionColor
                'Else
                '    .Font = _Control.Font
                '    .Color = _Control.ForeColor
                'End If

                If .ShowDialog = DialogResult.OK Then
                    'If _Control.SelectionLength > 0 Then
                    _Control.SelectionFont = .Font
                    _Control.SelectionColor = .Color
                    'Else
                    '    _Control.Font = .Font
                    '    _Control.ForeColor = .Color
                    'End If
                End If
            End With
        Catch ex As Exception
            Throw ex
        Finally
            FontDialog1.Dispose() : FontDialog1 = Nothing
        End Try
    End Sub

    Public Sub ctxMenuCopyCut(ByRef _Control As Control, ByRef _ContextMenu As ContextMenu, _
    Optional ByRef _Cut As Boolean = False)

        Dim grd As C1TrueDBGrid
        Dim sSelectedText As String

        Try

            If _Control.GetType Is GetType(RichTextBox) Then
                With CType(_Control, RichTextBox)
                    sSelectedText = .SelectedText
                    If _Cut Then .SelectedText = ""
                End With
            ElseIf _Control.GetType Is GetType(C1DateEdit) Then
                With CType(_Control, C1DateEdit)
                    sSelectedText = .SelectedText
                    If _Cut Then .SelectedText = ""
                End With
            ElseIf _Control.GetType Is GetType(DateTimeInput) Then
                With CType(_Control, DateTimeInput)
                    sSelectedText = .Text
                    If _Cut Then .Text = ""
                End With
            ElseIf _Control.GetType Is GetType(C1TrueDBGrid) OrElse _
                _Control.GetType.ToString = "C1.Win.C1TrueDBGrid.GridEditor" Then

                If _Control.GetType Is GetType(C1TrueDBGrid) Then
                    grd = CType(_Control, C1TrueDBGrid)
                Else
                    grd = CType(_Control.Parent, C1TrueDBGrid)
                End If

                With grd
                    sSelectedText = .SelectedText
                    If _Cut Then .SelectedText = ""
                End With

            Else
                With CType(_Control, TextBox)
                    sSelectedText = .SelectedText
                    If _Cut Then .SelectedText = ""
                End With
            End If

            Clipboard.SetDataObject(sSelectedText) : _ContextMenu.MenuItems(2).Visible = True

        Catch ex As Exception
            Throw ex
        End Try

    End Sub

    Public Sub ctxMenuPast(ByRef _Control As Control, Optional ByRef _Type As String = "String", Optional ByRef _Quote As Boolean = True)

        Dim grd As C1TrueDBGrid
        Dim sMaxLength, sLength As Int32
        Dim sEnabled, sReadOnly As Boolean
        Dim sClipboard, sSelectedText As String

        'String  : Letter & Degit
        'Integer : Degit Without Point
        'Decimal : Degit With Point

        Try

            sClipboard = CType(Clipboard.GetDataObject.GetData(DataFormats.UnicodeText), String)

            If sClipboard Is Nothing Then Exit Try

            If (_Quote = True OrElse sClipboard.IndexOf("'") < 0) AndAlso sClipboard <> "" AndAlso _
               (_Type = "String" OrElse IsNumeric(sClipboard) OrElse _
               (_Type = "DateTime" AndAlso (IsNumeric(sClipboard) OrElse IsDate(sClipboard)))) AndAlso _
               (_Type <> "Integer" OrElse sClipboard.IndexOf(".") < 0) Then

                If _Control.GetType Is GetType(RichTextBox) Then
                    With CType(_Control, RichTextBox)
                        sEnabled = .Enabled
                        sReadOnly = .ReadOnly
                        sLength = .Text.Length
                        sMaxLength = .MaxLength
                        sSelectedText = .SelectedText
                    End With
                ElseIf _Control.GetType Is GetType(C1DateEdit) OrElse _Type = "DateTime" Then
                    If _Control.GetType Is GetType(C1DateEdit) Then
                        With CType(_Control, C1DateEdit)
                            sEnabled = .Enabled
                            sReadOnly = .ReadOnly
                            sLength = .Text.Length
                            sMaxLength = .MaxLength
                            sSelectedText = .SelectedText
                        End With
                    ElseIf _Control.GetType Is GetType(DateTimeInput) Then
                        With CType(_Control, DateTimeInput)
                            sEnabled = .Enabled
                            sReadOnly = .IsInputReadOnly
                            sLength = .Text.Length
                            sSelectedText = .Text
                        End With
                    End If

                ElseIf _Control.GetType Is GetType(C1TrueDBGrid) OrElse _
                    _Control.GetType.ToString = "C1.Win.C1TrueDBGrid.GridEditor" Then

                    If _Control.GetType Is GetType(C1TrueDBGrid) Then
                        grd = CType(_Control, C1TrueDBGrid)
                    Else
                        grd = CType(_Control.Parent, C1TrueDBGrid)
                    End If

                    With grd
                        sEnabled = .Enabled
                        sReadOnly = .Splits(0).DisplayColumns(.Col).Locked
                        sLength = .Splits(0).DisplayColumns(.Col).DataColumn.Text.Length
                        sMaxLength = .Splits(0).DisplayColumns(.Col).DataColumn.DataWidth
                        sSelectedText = .SelectedText
                    End With

                Else
                    With CType(_Control, TextBox)
                        sEnabled = .Enabled
                        sReadOnly = .ReadOnly
                        sLength = .Text.Length
                        sMaxLength = .MaxLength
                        sSelectedText = .SelectedText
                    End With
                End If

                'added by GS on 03/03/2014
                If (_Type = "Decimal" And sClipboard.IndexOf(".") >= 0 And _Control.Text.IndexOf(".") >= 0 And sSelectedText.IndexOf(".") < 0) Then Exit Try

                If sEnabled = True AndAlso sReadOnly = False Then
                    If sMaxLength >= (sLength - sSelectedText.Length) + sClipboard.Length Then
                        If _Control.GetType Is GetType(RichTextBox) Then
                            CType(_Control, RichTextBox).SelectedText = sClipboard
                        ElseIf _Control.GetType Is GetType(C1TrueDBGrid) OrElse _
                            _Control.GetType.ToString = "C1.Win.C1TrueDBGrid.GridEditor" Then
                            grd.SelectedText = sClipboard
                        Else
                            CType(_Control, TextBox).SelectedText = sClipboard

                        End If
                    Else
                        MessageBox.Show("Invalid Number Of Characters", CntTitle, MessageBoxButtons.OK, MessageBoxIcon.Information)
                    End If
                End If

            End If

        Catch ex As Exception
            Throw ex
        End Try
    End Sub

    Public Sub ShowContextMenu(ByRef _sender As Object, Optional ByVal _MenuItem As Boolean = True, _
   Optional ByRef _Basic As Boolean = True, Optional frmtitle As String = "")

        Dim i As Int32
        Dim sNoMenu As Boolean
        Dim sSelectedText As String

        Dim sConMenu As ContextMenu

        Try
            CType(_sender, Control).Focus()

            Cursor.Current = Cursors.WaitCursor : gbRichTextSource = Nothing

            If _sender.GetType Is GetType(RichTextBox) Then
                gbRichTextSource = CType(_sender, RichTextBox)
                With gbRichTextSource
                    If .Enabled = False OrElse .ReadOnly = True OrElse _MenuItem = False Then sNoMenu = True
                    sConMenu = .ContextMenu
                    sSelectedText = .SelectedText
                End With
            ElseIf _sender.GetType Is GetType(RichTextBoxEx) Then
                gbRichTextSource = CType(_sender, RichTextBox)
                With gbRichTextSource
                    If .Enabled = False OrElse .ReadOnly = True OrElse _MenuItem = False Then sNoMenu = True
                    sConMenu = .ContextMenu
                    sSelectedText = .SelectedText
                End With
            ElseIf _sender.GetType Is GetType(C1DateEdit) Then
                With CType(_sender, C1DateEdit)
                    If .Enabled = False OrElse .ReadOnly = True OrElse _MenuItem = False Then sNoMenu = True
                    sConMenu = .ContextMenu
                    sSelectedText = .SelectedText
                End With
            ElseIf _sender.GetType Is GetType(DateTimeInput) Then
                With CType(_sender, DateTimeInput)
                    If .Enabled = False OrElse .IsInputReadOnly = True OrElse _MenuItem = False Then sNoMenu = True
                    sConMenu = .ContextMenu
                    sSelectedText = .Text
                End With
            ElseIf _sender.GetType Is GetType(TextBox) Then
                With CType(_sender, TextBox)
                    If .Enabled = False OrElse .ReadOnly = True OrElse _MenuItem = False Then sNoMenu = True
                    sConMenu = .ContextMenu
                    sSelectedText = .SelectedText
                End With
            ElseIf _sender.GetType Is GetType(TextBoxX) Then
                With CType(_sender, TextBoxX)
                    If .Enabled = False OrElse .ReadOnly = True OrElse _MenuItem = False Then sNoMenu = True
                    sConMenu = .ContextMenu
                    sSelectedText = .SelectedText
                End With
            ElseIf _sender.GetType Is GetType(C1TrueDBGrid) Then
                With CType(_sender, C1TrueDBGrid)
                    If .Enabled = False OrElse .Splits(0).DisplayColumns(.Col).Locked = True _
                    OrElse _MenuItem = False Then sNoMenu = True
                    sConMenu = .Controls(2).ContextMenu
                    sSelectedText = .SelectedText
                End With
            Else
                sConMenu = CType(_sender, Control).ContextMenu : sNoMenu = True
            End If

            With sConMenu

                If sNoMenu = False Then
                    If sSelectedText <> "" Then
                        For i = 0 To 1
                            .MenuItems(i).Visible = True
                        Next i
                    Else
                        For i = 0 To 1
                            .MenuItems(i).Visible = False
                        Next i
                    End If

                    For i = 2 To .MenuItems.Count - 1
                        If i = 2 Then
                            If Clipboard.GetDataObject.GetDataPresent(DataFormats.UnicodeText) Then
                                .MenuItems(2).Visible = True
                            Else
                                .MenuItems(2).Visible = False
                            End If
                        ElseIf _Basic = True Then
                            .MenuItems(i).Visible = False
                        Else
                            .MenuItems(i).Visible = True
                        End If
                    Next i

                    'Font, Check if there are any menu above has visiblitity = true
                    If _Basic = False Then

                        For i = 0 To .MenuItems.Count - 2
                            If .MenuItems(i).Visible = True Then
                                Exit For
                            End If
                        Next i

                        If i = .MenuItems.Count - 1 Then
                            .MenuItems(.MenuItems.Count - 2).Visible = False
                        End If

                    End If


                Else
                    For i = 0 To .MenuItems.Count - 1
                        .MenuItems(i).Visible = False
                    Next i
                End If

            End With

        Catch ex As Exception
            Throw ex
        Finally
            sConMenu = Nothing : Cursor.Current = Cursors.Default
        End Try

    End Sub


    Public Sub ReadOnlyCtr(ByVal _Control As Control, ByRef _Value As Boolean, ByRef _TabStop As Boolean, _
    ByVal ParamArray _Array() As Control)
        Dim sCtrl As Control
        Dim sValue As Boolean

        Try
            For Each sCtrl In _Control.Controls

                If (sCtrl.GetType Is GetType(TextBox) OrElse sCtrl.GetType Is GetType(RichTextBox) OrElse _
                   sCtrl.GetType Is GetType(C1DateEdit) OrElse sCtrl.GetType Is GetType(RadioButton) _
                   OrElse sCtrl.GetType Is GetType(CheckBox) OrElse sCtrl.GetType Is GetType(TextBoxX)) _
                   AndAlso (sCtrl.Tag Is Nothing OrElse sCtrl.Tag.ToString.Trim <> "Negation") Then
                    If Array.IndexOf(_Array, sCtrl) >= 0 Then
                        sValue = Not _Value
                    Else
                        sValue = _Value
                    End If

                    If sCtrl.GetType Is GetType(RadioButton) Then
                        CType(sCtrl, RadioButton).Enabled = Not sValue
                    ElseIf sCtrl.GetType Is GetType(TextBox) And sCtrl.Name <> "txtCode" Then
                        CType(sCtrl, TextBox).ReadOnly = sValue
                    ElseIf sCtrl.GetType Is GetType(TextBoxX) And sCtrl.Name <> "txtCode" Then
                        CType(sCtrl, TextBoxX).ReadOnly = sValue
                    ElseIf sCtrl.GetType Is GetType(RichTextBox) Then
                        CType(sCtrl, RichTextBox).ReadOnly = sValue
                    ElseIf sCtrl.GetType Is GetType(C1DateEdit) Then
                        CType(sCtrl, C1DateEdit).ReadOnly = sValue
                    ElseIf sCtrl.GetType Is GetType(CheckBox) Then
                        CType(sCtrl, CheckBox).AutoCheck = Not sValue
                    End If

                    If Not sCtrl.GetType Is GetType(RadioButton) Then
                        If sValue = True Then
                            sCtrl.TabStop = _TabStop
                        Else
                            sCtrl.TabStop = True
                        End If
                    End If

                    If sCtrl.GetType Is GetType(C1DateEdit) AndAlso sCtrl.Name.IndexOf("Time") < 0 Then
                        CType(sCtrl, C1DateEdit).ShowDropDownButton = Not sValue
                    End If

                ElseIf sCtrl.Controls.Count > 0 Then
                    ReadOnlyCtr(sCtrl, _Value, _TabStop, _Array)
                End If
            Next sCtrl
        Catch ex As Exception
            Throw ex
        Finally
            sCtrl = Nothing
        End Try
    End Sub

    'Added by Abir T. @ 29.08.2012
    Public Function convert5(ByVal mnt As Double, ByVal curfrom As String, ByVal curto As String, ByVal sdate As Date) As Double
        Conv_Err = False
        If curfrom = curto Then
            convert5 = mnt
            Exit Function
        End If
        If curfrom = gbCurr1 Then
            'If GetRates(CDate(CType(sdate, Date).ToString("dd/MM/yyyy")), curto) = False Then
            If GetRates(CDate(CType(sdate, Date).ToString("dd/MM/yyyy")), curto) = False Then
                MsgBox(curto + " Currency Rates Not Defined For The Date Of " + CType(sdate, Date).ToString("dd/MM/yyyy"), vbExclamation, "No Processing")
                convert5 = 0
                Conv_Err = True
                Exit Function
            Else
                convert5 = MathRound(CDbl(IIf(Wop1 = "*", mnt / Wrate1, mnt * Wrate1)), 3)
            End If
        ElseIf curfrom = gbCurr2 Then
            If curto = gbCurr1 Then
                If GetRates(CDate(CType(sdate, Date).ToString("dd/MM/yyyy")), curfrom) = False Then
                    MsgBox(curfrom + " Currency Rates Not Defined For The Date Of " + CType(sdate, Date).ToString("dd/MM/yyyy"), vbExclamation, "No Processing")
                    convert5 = 0
                    Conv_Err = True
                    Exit Function
                Else
                    convert5 = MathRound(CDbl(IIf(Wop1 = "*", mnt * Wrate1, mnt / Wrate1)), 3)
                End If
            Else
                If GetRates(CDate(CType(sdate, Date).ToString("dd/MM/yyyy")), curto) = False Then
                    MsgBox(curto + " Currency Rates Not Defined For The Date Of " + CType(sdate, Date).ToString("dd/MM/yyyy"), vbExclamation, "No Processing")
                    convert5 = 0
                    Conv_Err = True
                    Exit Function
                Else
                    convert5 = MathRound(CDbl(IIf(Wop2 = "*", mnt / Wrate2, mnt * Wrate2)), 3)
                End If
            End If
        Else
            If GetRates(CDate(CType(sdate, Date).ToString("dd/MM/yyyy")), curfrom) = False Then
                MsgBox(curfrom + " Currency Rates Not Defined For The Date Of " + CType(sdate, Date).ToString("dd/MM/yyyy"), vbExclamation, "No Processing")
                convert5 = 0
                Conv_Err = True
                Exit Function
            Else
                If curto = gbCurr1 Then
                    convert5 = MathRound(CDbl(IIf(Wop1 = "*", mnt * Wrate1, mnt / Wrate1)), 3)
                Else
                    convert5 = MathRound(CDbl(IIf(Wop2 = "*", mnt * Wrate2, mnt / Wrate2)), 3)
                    If curto <> gbCurr2 Then
                        If GetRates(CDate(CType(sdate, Date).ToString("dd/MM/yyyy")), curto) = False Then
                            MsgBox(curto + " Currency Rates Not Defined For The Date Of " + CType(sdate, Date).ToString("dd/MM/yyyy"), vbExclamation, "No Processing")
                            convert5 = 0
                            Conv_Err = True
                            Exit Function
                        Else
                            convert5 = MathRound(CDbl(convert5(convert5(mnt, curfrom, gbCurr2, CDate(Format(Now, "dd/mm/yyyy"))), gbCurr2, curto, CDate(Format(Now, "dd/mm/yyyy")))), 3)
                        End If
                    End If
                End If
            End If
        End If
    End Function

    'Added by Abir T. @ 29.08.2012
    Function GetRates(ByVal Wdate As Date, Optional ByVal WCurr As String = "") As Boolean
        Dim strsql5 As String
        Dim cnSql As SqlConnection
        Dim cmSql As SqlCommand
        Dim drSql As SqlDataReader

        Try
            Wrate1 = 1 : Wrate2 = 1
            Wop1 = "*" : Wop2 = "*"

            'Get The Rate No. Used by the application
            strsql5 = "Select * From Com_Application Where App_Code='" & gbApp & "'"
            cnSql = New SqlConnection(ConnectionString)
            cnSql.Open()
            cmSql = New SqlCommand(strsql5, cnSql)
            drSql = cmSql.ExecuteReader
            With drSql
                If Not .HasRows Then
                    GetRates = False
                    Exit Function
                End If
                .Read()
                WrateNo = CByte(.Item("App_Rate").ToString)
            End With
            If Not drSql Is Nothing Then
                If drSql.IsClosed = False Then drSql.Close()
                drSql = Nothing
            End If
            cmSql = Nothing
            If Not cnSql Is Nothing Then
                If cnSql.State = ConnectionState.Open Then cnSql.Close()
                cnSql = Nothing
            End If

            'Get The Rates From DailyRate
            strsql5 = "Select * From Com_DailyRate Where "
            strsql5 = strsql5 + " convert(datetime,DR_Date,101) >='" & Format(Wdate, "MM/dd/yyyy 00:00:00.000") & "'"
            strsql5 = strsql5 + " and convert(datetime,DR_Date,101) <='" & Format(Wdate, "MM/dd/yyyy 23:59:59.000") & "'"
            If WCurr <> "" Then
                strsql5 = strsql5 + " And DR_CurCode='" & CStr(IIf(WCurr = gbCurr1, gbCurr2, WCurr)) & "'"
            End If
            cnSql = New SqlConnection(ConnectionString)
            cnSql.Open()
            cmSql = New SqlCommand(strsql5, cnSql)
            drSql = cmSql.ExecuteReader
            With drSql
                If .HasRows Then
                    .Read()
                    Wrate1 = Val(.Item("DR_Rate" + Trim(Str(WrateNo)) + "LC").ToString)
                    Wrate2 = Val(.Item("DR_Rate" + Trim(Str(WrateNo)) + "CC").ToString)
                    Wop1 = .Item("DR_Op" + Trim(Str(WrateNo)) + "LC").ToString
                    Wop2 = .Item("DR_Op" + Trim(Str(WrateNo)) + "CC").ToString
                    GetRates = True
                Else
                    GetRates = False
                End If
                If Wrate1 = 0 Then Wrate1 = 1
                If Wrate2 = 0 Then Wrate2 = 1
            End With
        Catch ex As Exception
            Throw ex

        Finally
            If Not drSql Is Nothing Then
                If drSql.IsClosed = False Then drSql.Close()
                drSql = Nothing
            End If
            cmSql = Nothing
            If Not cnSql Is Nothing Then
                If cnSql.State = ConnectionState.Open Then cnSql.Close()
                cnSql = Nothing
            End If
        End Try
    End Function
    Public Sub VisibleBtn(ByVal _Control As Control, ByRef _Value As Boolean, _
    ByVal ParamArray _Array() As Control)
        Dim sCtrl As Control
        Dim sValue As Boolean

        Try
            For Each sCtrl In _Control.Controls

                If sCtrl.GetType Is GetType(Button) Then

                    If Not sCtrl.Tag Is Nothing AndAlso sCtrl.Tag.ToString.Trim = "Visible" Then
                        If Array.IndexOf(_Array, sCtrl) >= 0 Then
                            sValue = Not _Value
                        Else
                            sValue = _Value
                        End If

                        If sCtrl.GetType Is GetType(Button) Then
                            CType(sCtrl, Button).Visible = sValue
                        End If
                    End If
                ElseIf sCtrl.Controls.Count > 0 Then
                    VisibleBtn(sCtrl, _Value, _Array)
                End If
            Next sCtrl
        Catch ex As Exception
            Throw ex
        Finally
            sCtrl = Nothing
        End Try
    End Sub
    Public Sub DropLinkServer(ByVal _LinkServer As String)

        Dim strsql As String
        Dim cn As New SqlConnection
        Dim cm As New SqlCommand
        Try


            cn = New SqlConnection(ConnectionString)
            cn.Open()

            cm = New SqlCommand

            With cm
                .Connection = cn
                .CommandType = CommandType.Text
            End With
            strsql = "select srvname from master.dbo.sysservers where srvname = '" & _LinkServer & "'"
            cm.CommandText = strsql
            If Not cm.ExecuteScalar Is Nothing Then
                strsql = "EXEC sp_droplinkedsrvlogin '" & _LinkServer & "', NULL "
                strsql = strsql & "EXEC sp_dropserver  '" & _LinkServer & "', NULL"

                cm.CommandText = strsql
                cm.ExecuteNonQuery()
            End If

        Catch ex As Exception
            Throw ex
        Finally
            Disposecontrols(cn, cm)
        End Try
    End Sub
    Public Sub Disposecontrols(Optional ByRef _cnsql As SqlConnection = Nothing, Optional ByRef _cmsql As SqlCommand = Nothing, Optional ByRef _drsql As SqlDataReader = Nothing, Optional ByRef _trnsql As SqlTransaction = Nothing, Optional ByRef _cbsql As SqlCommandBuilder = Nothing, Optional ByRef _daSQL As SqlDataAdapter = Nothing, Optional ByRef _linkserver As String = Nothing)
        Try

            If Not _drsql Is Nothing Then
                If Not _drsql.IsClosed Then _drsql.Close()
                _drsql = Nothing
            End If

            If Not _daSQL Is Nothing Then _daSQL.Dispose()

            If Not _trnsql Is Nothing Then
                _trnsql.Dispose()
                _trnsql = Nothing
            End If

            If Not _cmsql Is Nothing Then
                _cmsql.Dispose()
                _cmsql = Nothing
            End If

            If Not _cnsql Is Nothing Then
                If _cnsql.State = ConnectionState.Open Then _cnsql.Close()
                _cnsql.Dispose()
                _cnsql = Nothing
            End If
            If Not _cbsql Is Nothing Then
                _cbsql = Nothing
            End If
            If Not _linkserver Is Nothing AndAlso Not _linkserver = "" Then DropLinkServer(_linkserver)
        Catch ex As Exception
            Throw ex
        End Try
    End Sub
    'Added by Abir T. @ 01.08.2013
    Public Sub DisposecontrolsArrays(Optional ByRef _cnsql As SqlConnection = Nothing, Optional ByRef _cmsql As SqlCommand = Nothing, Optional ByRef _trnsql As SqlTransaction = Nothing, Optional ByRef _cbsql As SqlCommandBuilder = Nothing, Optional ByRef _daSQL As SqlDataAdapter = Nothing, Optional ByRef _linkserver As String = Nothing, Optional ByRef _drsql() As SqlDataReader = Nothing)
        Try
            Dim drsql As SqlDataReader
            For Each drsql In _drsql
                If Not drsql Is Nothing Then
                    If Not drsql.IsClosed Then drsql.Close()
                    _drsql = Nothing
                End If
            Next

            If Not _daSQL Is Nothing Then _daSQL.Dispose()

            If Not _trnsql Is Nothing Then
                _trnsql.Dispose()
                _trnsql = Nothing
            End If

            If Not _cmsql Is Nothing Then
                _cmsql.Dispose()
                _cmsql = Nothing
            End If

            If Not _cnsql Is Nothing Then
                If _cnsql.State = ConnectionState.Open Then _cnsql.Close()
                _cnsql.Dispose()
                _cnsql = Nothing
            End If
            If Not _cbsql Is Nothing Then
                _cbsql = Nothing
            End If
            If Not _linkserver Is Nothing AndAlso Not _linkserver = "" Then DropLinkServer(_linkserver)
        Catch ex As Exception
            Throw ex
        End Try
    End Sub
    Public Sub GetStockItemKey()
        Dim cnSql As SqlConnection
        Dim cmSql As SqlCommand
        Dim strsql As String
        Dim drSql As SqlDataReader
        Dim WholeKey, WholeKeyPend, WholeKeyDesc As String
        Dim StartKey As Boolean
        Dim NbrOfKeys As Integer
        Try
            cnSql = New SqlConnection(ConnectionString)
            cnSql.Open()

            strsql = "SET TRAN ISOLATION LEVEL READ UNCOMMITTED  ;  SELECT definition,value, c.max_length as [Maxlght]  FROM sys.columns c LEFT OUTER JOIN sys.extended_properties ex"
            strsql &= " ON ex.major_id=c.object_id AND ex.minor_id=c.column_id AND ex.name='ms_description'"
            strsql &= " INNER JOIN sys.computed_columns compcol ON compcol.column_id=c.column_id"
            strsql &= " INNER JOIN sys.tables  Tbls ON Tbls.object_id = c.object_id"
            strsql &= " WHERE c.name='stk_ref'   AND ISNULL(Tbls.name,'') IN ( 'POS_STKFIL' ,'Stk04_STKFIL')"

            cmSql = New SqlCommand(strsql, cnSql)
            drSql = cmSql.ExecuteReader

            With drSql
                If .HasRows Then
                    .Read()
                    WholeKey = .Item(0).ToString
                    WholeKeyPend = .Item(0).ToString
                    WholeKeyDesc = .Item(1).ToString
                    gbmaxreflengh = .Item("Maxlght").ToString
                End If
            End With

            WholeKeyPend = UCase(WholeKeyPend).Replace("STK_", "SFP_")


            StartKey = False
            NbrOfKeys = 0
            For i = 1 To Len(WholeKey) - 1
                If WholeKey.Substring(i, 1) = "]" Then
                    StartKey = False
                End If

                If StartKey = True Then
                    CurrItmKeyPend(NbrOfKeys - 1) = CurrItmKeyPend(NbrOfKeys - 1) & WholeKeyPend.Substring(i, 1)
                    CurrItmKey(NbrOfKeys - 1) = CurrItmKey(NbrOfKeys - 1) & WholeKey.Substring(i, 1)

                End If
                'added By Ch @ 20/09/2016 Since A Error Appear convert(varchar Is Saved As convert ([Varchar] in DB And it will Consider as Field 
                If Len(WholeKey) - i < 8 Then i = i
                If WholeKey.Substring(i, 1) = "[" AndAlso UCase(WholeKey.Substring(i, 9)) <> "[VARCHAR]" Then
                    StartKey = True
                    ReDim Preserve CurrItmKey(NbrOfKeys)
                    ReDim Preserve CurrItmKeyPend(NbrOfKeys)
                    NbrOfKeys = NbrOfKeys + 1
                End If
                'Else
                '    If WholeKey.Substring(6i, 1) = "[" Then
                '        StartKey = True
                '        ReDim Preserve CurrItmKey(NbrOfKeys)
                '        ReDim Preserve CurrItmKeyPend(NbrOfKeys)
                '        NbrOfKeys = NbrOfKeys + 1
                '    End If
                'End If
            Next

            StartKey = False
            NbrOfKeys = 0
            For i = 1 To Len(WholeKeyDesc) - 1
                If WholeKeyDesc.Substring(i, 1) = "]" Then
                    StartKey = False
                End If

                If StartKey = True Then
                    CurrItmKeyDesc(NbrOfKeys - 1) = CurrItmKeyDesc(NbrOfKeys - 1) & WholeKeyDesc.Substring(i, 1)
                End If

                ' If Len(WholeKeyDesc) - i > 8 Then**
                If WholeKeyDesc.Substring(i, 1) = "[" AndAlso (WholeKeyDesc.Substring(i, CInt(IIf(WholeKeyDesc.Length - i > 9, 9, 0)))) <> "[VARCHAR]" Then
                    StartKey = True
                    ReDim Preserve CurrItmKeyDesc(NbrOfKeys)
                    NbrOfKeys = NbrOfKeys + 1
                End If
                'Else
                '    If WholeKeyDesc.Substring(i, 1) = "[" Then
                '        StartKey = True
                '        ReDim Preserve CurrItmKeyDesc(NbrOfKeys)
                '        NbrOfKeys = NbrOfKeys + 1
                '    End If
                'End If
            Next

            'Abir T. @ 19.06.2014
            If CurrItmKey.Contains("STK_Comp", StringComparer.OrdinalIgnoreCase) Then
                CurrItemKeyComp = True
            Else
                CurrItemKeyComp = False
            End If

        Catch ex As Exception
            Throw ex
        Finally
            Disposecontrols(cnSql, cmSql, drSql)
        End Try
    End Sub
    Public Function GetSupFolio(ByVal SupCode As String) As Double
        Dim drSQL As SqlDataReader
        '..folio Pay Days
        strsql = "select isnull(Fo_PayDays,0) from V_PORD2_SupFolioPay"
        strsql &= " where sup_code='" & SupCode & "'"
        ReadFromDBToDataReader(strsql, drSQL)
        If drSQL.HasRows Then
            drSQL.Read()
            GetSupFolio = CDbl(drSQL.Item(0))
        Else
            GetSupFolio = 0
        End If
    End Function

    Public Function KeySizeGrid(ByVal CurrItem As String) As Double
        Try

            Select Case UCase(CurrItem)
                Case "STK_COMP" : KeySizeGrid = 40
                Case "SFP_COMP" : KeySizeGrid = 40
                Case "STK_SUPPLIER" : KeySizeGrid = 40
                Case "SFP_SUPPLIER" : KeySizeGrid = 40
                Case "STK_CATEGORY" : KeySizeGrid = 40
                Case "SFP_CATEGORY" : KeySizeGrid = 40
                Case "STK_SERIE" : KeySizeGrid = 40
                Case "SFP_SERIE" : KeySizeGrid = 40
                Case "STK_SINGLEREF1" : KeySizeGrid = 80
                Case "SFP_SINGLEREF1" : KeySizeGrid = 80
                Case "STK_SINGLEREF2" : KeySizeGrid = 80
                Case "SFP_SINGLEREF2" : KeySizeGrid = 80
                Case "STK_USAGE" : KeySizeGrid = 40
                Case "SFP_USAGE" : KeySizeGrid = 40
                Case "STK_NEWUSED" : KeySizeGrid = 40
                Case "SFP_NEWUSED" : KeySizeGrid = 40
                Case "STK_SEASON" : KeySizeGrid = 40
                Case "SFP_SEASON" : KeySizeGrid = 40
                Case "STK_BRAND" : KeySizeGrid = 40
                Case "SFP_BRAND" : KeySizeGrid = 40
                Case "STK_GRID" : KeySizeGrid = 45
                Case "SFP_GRID" : KeySizeGrid = 45
                Case "STK_GENDER" : KeySizeGrid = 40
                Case "SFP_GENDER" : KeySizeGrid = 40
                Case "STK_STYLE" : KeySizeGrid = 40
                Case "SFP_STYLE" : KeySizeGrid = 40
            End Select

        Catch ex As Exception
            Throw ex
        End Try
    End Function
    'Created By Rita Merheb on 29.04.2013
    Enum BUAccess
        Purchase
        Sale
        Reporting
        Cost
        Franchise
        Pricing
        Other
    End Enum
    'Public Function AppStkCond(ByVal FldName As String, Optional ByVal tablename As String = "", Optional ByVal Brand As Boolean = False, Optional ByVal Supplier As Boolean = False, Optional ByVal BUAccess As BUAccess = BUAccess.Other, Optional ByVal PendingItem As Boolean = False) As String
    '    'PendingItem is added to use the columns SFP_NewUsed instead of STK_NEWUSED in case of item from pending stock file
    '    Dim Strs1 As String
    '    Try
    '        If UCase(gbUser) = "SA" Or UCase(gbUser) = "SU" Then
    '            AppStkCond = " "
    '        Else

    '            Strs1 = " INNER JOIN  (SELECT ASP_STKTAG FROM STK04_APPSTKPRIV WHERE ASP_USER='" & gbUser & "' AND ASP_APPcODE ='" & gbApp & "') AS STK04_APPSTKPRIV "
    '            Strs1 = Strs1 + " On ASP_StkTag= "
    '            If tablename <> "" Then
    '                Strs1 = Strs1 & tablename & "."
    '            End If
    '            If PendingItem = True Then
    '                'Abir T. @ 12.09.2013
    '                'Strs1 = Strs1 + "coalesce(stk_newused,SFP_NEWUSED) "
    '                Strs1 = Strs1 + " SFP_NEWUSED "
    '            Else
    '                Strs1 = Strs1 + "STK_NEWUSED "
    '            End If

    '            'Business Unit Manager Condition
    '            '1 - Purchase Access
    '            '2 - Sale Access
    '            '3 - Reporting Access
    '            '4 - Cost Access
    '            '5 - Franchise Access
    '            '6 - Pricing Access
    '            If Brand = True Then
    '                Strs1 &= " INNER JOIN ( SELECT BUMP_BrandCode FROM dbo.Stk04_BusUnitManagPriv"
    '                Strs1 &= " WHERE  BUMP_UserCode = '" & gbUser & "' "
    '                Select Case BUAccess
    '                    Case BUAccess.Purchase
    '                        Strs1 &= " AND BUMP_Access = '1'"
    '                    Case BUAccess.Sale
    '                        Strs1 &= " AND BUMP_Access = '2'"
    '                    Case BUAccess.Reporting
    '                        Strs1 &= " AND BUMP_Access = '3'"
    '                    Case BUAccess.Cost
    '                        Strs1 &= " AND BUMP_Access = '4'"
    '                    Case BUAccess.Franchise
    '                        Strs1 &= " AND BUMP_Access = '5'"
    '                    Case BUAccess.Pricing
    '                        Strs1 &= " AND BUMP_Access = '6'"
    '                    Case Else
    '                        Strs1 &= " AND BUMP_Access = ''"
    '                End Select
    '                Strs1 &= " AND BUMP_BrandCode IS NOT NULL AND BUMP_SuppCode IS NULL ) AS BusUnitManagPrivBrand "
    '                Strs1 &= " ON BUMP_BrandCode = "
    '                If tablename <> "" Then
    '                    Strs1 = Strs1 & tablename & "."
    '                End If

    '                If PendingItem = True Then
    '                    'Abir T. @ 12.09.2013
    '                    'Strs1 &= " ON BUMP_BrandCode = coalesce(Stk_Brand,SFP_Brand)"
    '                    Strs1 &= "SFP_Brand"
    '                Else
    '                    Strs1 &= "STK_Brand"
    '                End If
    '            End If
    '            If Supplier = True Then
    '                Strs1 &= " INNER JOIN ( SELECT BUMP_SuppCode FROM dbo.Stk04_BusUnitManagPriv"
    '                Strs1 &= " WHERE  BUMP_UserCode = '" & gbUser & "' "
    '                Select Case BUAccess
    '                    Case BUAccess.Purchase
    '                        Strs1 &= " AND BUMP_Access = '1'"
    '                    Case BUAccess.Sale
    '                        Strs1 &= " AND BUMP_Access = '2'"
    '                    Case BUAccess.Reporting
    '                        Strs1 &= " AND BUMP_Access = '3'"
    '                    Case BUAccess.Cost
    '                        Strs1 &= " AND BUMP_Access = '4'"
    '                    Case BUAccess.Franchise
    '                        Strs1 &= " AND BUMP_Access = '5'"
    '                    Case BUAccess.Pricing
    '                        Strs1 &= " AND BUMP_Access = '6'"
    '                    Case Else
    '                        Strs1 &= " AND BUMP_Access = ''"
    '                End Select
    '                Strs1 &= " AND BUMP_BrandCode IS NULL AND BUMP_SuppCode IS NOT NULL ) AS BusUnitManagPrivSupplier "
    '                Strs1 &= " ON BUMP_SuppCode = "
    '                If tablename <> "" Then
    '                    Strs1 = Strs1 & tablename & "."
    '                End If
    '                If PendingItem = True Then
    '                    Strs1 &= "SFP_SUPPLIER"
    '                Else
    '                    Strs1 &= "STK_SUPPLIER"
    '                End If
    '            End If

    '            AppStkCond = Strs1
    '        End If

    '    Catch ex As Exception
    '        Throw ex
    '    End Try
    'End Function
    Public Function AppStkCond(ByVal FldName As String, Optional ByVal tablename As String = "", Optional ByVal Brand As Boolean = False, Optional ByVal Supplier As Boolean = False, Optional ByVal BUAccess As BUAccess = BUAccess.Other, Optional ByVal PendingItem As Boolean = False, Optional ByVal Index As Integer = 0, Optional ByVal StkPending As Boolean = False, Optional ByVal Item As Boolean = True) As String
        'PendingItem is added to use the columns SFP_NewUsed instead of STK_NEWUSED in case of item from pending stock file
        Dim Strs1 As String
        Try
            If UCase(gbUser) = "SA" Or UCase(gbUser) = "SU" Then
                AppStkCond = " "
            Else

                If Item = True Then
                    Strs1 = " INNER JOIN  (SELECT ASP_STKTAG FROM STK04_APPSTKPRIV WHERE ASP_USER='" & gbUser & "' AND ASP_APPcODE ='" & gbApp & "')"
                    'Abir T. @ 09.11.2013
                    If PendingItem = True Then
                        Strs1 &= " AS STK04_APPSTKPRIVPend" & Index & " ON STK04_APPSTKPRIVPend" & Index & ".ASP_StkTag ="
                        If StkPending = True Then Strs1 &= " COALESCE("
                    Else
                        Strs1 &= " AS STK04_APPSTKPRIVSTK" & Index & " ON STK04_APPSTKPRIVSTK" & Index & ".ASP_StkTag ="
                    End If
                    ' If tablename <> "" Then Strs1 = Strs1 & tablename & "."
                    If PendingItem = True Then
                        If StkPending = True Then
                            If tablename <> "" Then Strs1 = Strs1 & tablename & "."
                            Strs1 &= "STK_NEWUSED"
                            Strs1 &= ","
                            If tablename <> "" Then Strs1 = Strs1 & tablename & "."
                            Strs1 &= "SFP_NEWUSED"
                        Else
                            If tablename <> "" Then Strs1 = Strs1 & tablename & "."
                            Strs1 &= "SFP_NEWUSED"
                        End If
                        If StkPending = True Then Strs1 &= ")"
                    Else
                        If tablename <> "" Then Strs1 = Strs1 & tablename & "."
                        Strs1 &= "STK_NEWUSED "
                    End If
                End If

                'Business Unit Manager Condition
                '1 - Purchase Access
                '2 - Sale Access
                '3 - Reporting Access
                '4 - Cost Access
                '5 - Franchise Access
                '6 - Pricing Access
                If Brand = True Then
                    Strs1 &= " INNER JOIN ( SELECT BUMP_BrandCode FROM dbo.Stk04_BusUnitManagPriv"
                    Strs1 &= " WHERE  BUMP_UserCode = '" & gbUser & "' "
                    Select Case BUAccess
                        Case BUAccess.Purchase
                            Strs1 &= " AND BUMP_Access = '1'"
                        Case BUAccess.Sale
                            Strs1 &= " AND BUMP_Access = '2'"
                        Case BUAccess.Reporting
                            Strs1 &= " AND BUMP_Access = '3'"
                        Case BUAccess.Cost
                            Strs1 &= " AND BUMP_Access = '4'"
                        Case BUAccess.Franchise
                            Strs1 &= " AND BUMP_Access = '5'"
                        Case BUAccess.Pricing
                            Strs1 &= " AND BUMP_Access = '6'"
                        Case Else
                            Strs1 &= " AND BUMP_Access = ''"
                    End Select
                    Strs1 &= " AND isnull(BUMP_BrandCode,'')<> '' AND Isnull(BUMP_SuppCode,'')=''  )"
                    'Abir T. @ 09.11.2013
                    If PendingItem = True Then
                        Strs1 &= " AS BusUnitManagPrivBrandPend" & Index & " ON BusUnitManagPrivBrandPend" & Index & ".BUMP_BrandCode = "
                        If StkPending = True Then
                            Strs1 &= " COALESCE("
                            If tablename <> "" Then Strs1 = Strs1 & tablename & "."
                            Strs1 &= "STK_Brand"
                            Strs1 &= ","
                            If tablename <> "" Then Strs1 = Strs1 & tablename & "."
                            Strs1 &= "SFP_Brand"
                        Else
                            If tablename <> "" Then Strs1 = Strs1 & tablename & "."
                            Strs1 &= "SFP_Brand"
                        End If
                        If StkPending = True Then Strs1 &= ")"
                    Else
                        Strs1 &= " AS BusUnitManagPrivBrandSTK" & Index & " ON BusUnitManagPrivBrandSTK" & Index & ".BUMP_BrandCode = "
                        If tablename <> "" Then Strs1 = Strs1 & tablename & "."
                        Strs1 &= "STK_Brand"
                    End If
                End If
                If Supplier = True Then
                    Strs1 &= " INNER JOIN ( SELECT BUMP_SuppCode FROM dbo.Stk04_BusUnitManagPriv"
                    Strs1 &= " WHERE  BUMP_UserCode = '" & gbUser & "' "
                    Select Case BUAccess
                        Case BUAccess.Purchase
                            Strs1 &= " AND BUMP_Access = '1'"
                        Case BUAccess.Sale
                            Strs1 &= " AND BUMP_Access = '2'"
                        Case BUAccess.Reporting
                            Strs1 &= " AND BUMP_Access = '3'"
                        Case BUAccess.Cost
                            Strs1 &= " AND BUMP_Access = '4'"
                        Case BUAccess.Franchise
                            Strs1 &= " AND BUMP_Access = '5'"
                        Case BUAccess.Pricing
                            Strs1 &= " AND BUMP_Access = '6'"
                        Case Else
                            Strs1 &= " AND BUMP_Access = ''"
                    End Select
                    Strs1 &= " AND BUMP_BrandCode IS NULL AND BUMP_SuppCode IS NOT NULL )"
                    'Abir T. @ 09.11.2013
                    If PendingItem = True Then
                        Strs1 &= " AS BusUnitManagPrivSupplierPend" & Index & " ON BusUnitManagPrivSupplierPend" & Index & ".BUMP_SuppCode = "
                        If StkPending = True Then
                            Strs1 &= " COALESCE("
                            If tablename <> "" Then Strs1 = Strs1 & tablename & "."
                            Strs1 &= "STK_SUPPLIER"
                            Strs1 &= ","
                            If tablename <> "" Then Strs1 = Strs1 & tablename & "."
                            Strs1 &= "SFP_SUPPLIER"
                        End If
                        If tablename <> "" Then Strs1 = Strs1 & tablename & "."
                        Strs1 &= "SFP_SUPPLIER"
                        If StkPending = True Then Strs1 &= ")"

                    Else
                        Strs1 &= " AS BusUnitManagPrivSupplierSTK" & Index & " ON BusUnitManagPrivSupplierSTK" & Index & ".BUMP_SuppCode = "
                        If tablename <> "" Then Strs1 = Strs1 & tablename & "."
                        Strs1 &= "STK_SUPPLIER"
                    End If
                End If

                AppStkCond = Strs1
            End If

        Catch ex As Exception
            Throw ex
        End Try
    End Function

#End Region
    'ch @ 20/10/2016 Wf : 4518
    'Public Function CheckEAN13(ByVal Barcode As Int64) As Boolean ((error : value Either too large or too small for int64 ))
    Public Function CheckEAN13(ByVal Barcode As String) As Boolean
        Try
            If Barcode.ToString.Length <> 13 Then
                Return True
            End If
            Dim LastDigit, Nums(12) As Int64
            Dim i As Integer
            i = 13
            Do While i > 0
                Nums(i - 1) = CType(Mid(Barcode.ToString, i, 1), Integer)
                i -= 1
            Loop
            LastDigit = (Nums(11) * 3) '12
            LastDigit += (Nums(10) * 1) '11
            LastDigit += (Nums(9) * 3) '10
            LastDigit += (Nums(8) * 1) '9
            LastDigit += (Nums(7) * 3) '8
            LastDigit += (Nums(6) * 1) '7
            LastDigit += (Nums(5) * 3) '6
            LastDigit += (Nums(4) * 1) '5
            LastDigit += (Nums(3) * 3) '4
            LastDigit += (Nums(2) * 1) '3
            LastDigit += (Nums(1) * 3) '2
            LastDigit += (Nums(0) * 1) '1st left digit
            LastDigit = LastDigit Mod 10
            If LastDigit <> 0 Then LastDigit = 10 - LastDigit
            If LastDigit = Nums(12) Then
                Return True
            Else
                Return False
            End If
        Catch ex As Exception
            Throw ex
        End Try
    End Function

    Public Function CheckUPCA(ByVal Barcode As String) As Boolean
        Try
            If Barcode.ToString.Length <> 12 Then
                Return True
            End If
            Dim LastDigit, Nums(11) As Int64
            Dim i As Integer
            i = 12
            Do While i > 0
                Nums(i - 1) = CType(Mid(Barcode.ToString, i, 1), Integer)
                i -= 1
            Loop
            LastDigit += (Nums(10) * 3) '11
            LastDigit += (Nums(9) * 1) '10
            LastDigit += (Nums(8) * 3) '9
            LastDigit += (Nums(7) * 1) '8
            LastDigit += (Nums(6) * 3) '7
            LastDigit += (Nums(5) * 1) '6
            LastDigit += (Nums(4) * 3) '5
            LastDigit += (Nums(3) * 1) '4
            LastDigit += (Nums(2) * 3) '3
            LastDigit += (Nums(1) * 1) '2
            LastDigit += (Nums(0) * 3) '1st left digit
            LastDigit = LastDigit Mod 10
            If LastDigit <> 0 Then LastDigit = 10 - LastDigit
            If LastDigit = Nums(11) Then
                Return True
            Else
                Return False
            End If
        Catch ex As Exception
            Throw ex
        End Try
    End Function

    Public Function convert(ByVal mnt As Double, ByVal curfrom As String, ByVal curto As String, ByVal sdate As Date) As Double
        'R.A. 17/10/2014
        'Getrate to give the dev an option if he want to send the rates then Getrate=false 
        Try
            If curfrom = curto Then
                convert = mnt
                Exit Function
            End If
            If curfrom = gbCurr1 Then

                If GetRates(sdate, curto) = False Then
                    MsgBox(curto + " Currency Rates Not Defined For The Date Of " + CStr(sdate), vbExclamation, "No Processing")
                    Exit Function
                Else
                    convert = CDbl(IIf(Wop1 = "*", mnt / Wrate1, mnt * Wrate1))
                End If
            ElseIf curfrom = gbCurr2 Then
                If curto = gbCurr1 Then
                    If GetRates(sdate, curfrom) = False Then
                        MsgBox(curfrom + " Currency Rates Not Defined For The Date Of " + CStr(sdate), vbExclamation, "No Processing")
                        Exit Function
                    Else
                        convert = CDbl(IIf(Wop1 = "*", mnt * Wrate1, mnt / Wrate1))
                    End If
                Else
                    If GetRates(sdate, curto) = False Then
                        MsgBox(curto + " Currency Rates Not Defined For The Date Of " + CStr(sdate), vbExclamation, "No Processing")
                        Exit Function
                    Else
                        convert = CDbl(IIf(Wop2 = "*", mnt / Wrate2, mnt * Wrate2))
                    End If
                End If
            Else
                If GetRates(sdate, curfrom) = False Then
                    MsgBox(curfrom + " Currency Rates Not Defined For The Date Of " + CStr(sdate), vbExclamation, "No Processing")
                    Exit Function
                Else
                    If curto = gbCurr1 Then
                        convert = CDbl(IIf(Wop1 = "*", mnt * Wrate1, mnt / Wrate1))
                    Else
                        convert = MathRound(CDbl((IIf(Wop2 = "*", mnt * Wrate2, mnt / Wrate2))), gbDecNo)
                        If curto <> gbCurr2 Then
                            If GetRates(sdate, curto) = False Then
                                MsgBox(curto + " Currency Rates Not Defined For The Date Of " + CStr(sdate), vbExclamation, "No Processing")
                                Exit Function
                            Else
                                convert = convert(convert(mnt, curfrom, gbCurr2, CDate(Format(Now, "dd/mm/yyyy"))), gbCurr2, curto, CDate(Format(Now, "dd/mm/yyyy")))
                            End If
                        End If
                    End If
                End If
            End If

        Catch ex As Exception
            Throw ex
        End Try
    End Function

    Public Sub FillIfOnlyOneRecordExists(ByVal _Query As String, ByVal textCode As TextBox, ByVal LblDesc As Label)
        Dim optQ As Boolean
        Dim strsql As String
        Dim da As SqlDataAdapter
        Dim dt As DataTable

        Try
            'If _Query Is Nothing OrElse _Query.Trim = "" Then
            '    strsql = "Select distinct " & _Code & " from " & _TableName & " where 1=1 "
            '    If Not QueryCond Is Nothing AndAlso QueryCond.Trim <> "" Then strsql &= " And  " & QueryCond
            'Else
            ' ch @ 16/04/2018 query fill all data in tables to test if exist one record fill data table take time 
            If Not _Query.ToUpper.Contains("SELECT DISTINCT") AndAlso _Query.ToUpper.Contains("SELECT") Then
                _Query = _Query.Replace("select", "Select top 2 ").Replace("SELECT", "SELECT top 2 ")
            End If

            strsql = _Query
            '  End If

            da = New SqlDataAdapter(strsql, ConnectionString)
            dt = New DataTable
            da.Fill(dt)

            If dt.Rows.Count = 1 Then
                textCode.Text = dt.Rows(0)(0).ToString
                LblDesc.Text = dt.Rows(0)(1).ToString
            End If

        Catch ex As Exception
            Throw ex
        Finally
            If Not da Is Nothing Then da.Dispose() : da = Nothing
            dt = Nothing
        End Try
    End Sub
    'Rachel A. 02.03.2016 -- check if doesnt exist in price type(9-n)
    Public Function CheckPriceTypeExist(Optional ByVal values As String = "") As Boolean
        Try

            If RecordsExists(" SELECT PRC_CODE FROM stk04_PRICETYPE WHERE PRC_CODE IN (" & IIf(values <> "", values, "'9','N'").ToString & ")") = True Then
                CheckPriceTypeExist = True
            Else
                MessageBox.Show("Corresponding Price Type Doesn't Exist, Please Add it.", "Price Type", MessageBoxButtons.OK, MessageBoxIcon.Information)
                CheckPriceTypeExist = False
            End If

        Catch ex As Exception
            Throw ex
        End Try
    End Function

    ''added by GS on 12/04/16
    'Public Function Val(str As Object) As Double
    '    Try
    '        Return Microsoft.VisualBasic.Conversion.Val(str.ToString.Replace(",", ""))
    '    Catch ex As Exception
    '        Throw ex
    '    End Try
    'End Function

    'added by GS on 23/06/15
#Region "Linked Server"
    Public Sub droplinkedsrvlogin(server As String)
        Dim cnSql As SqlConnection
        Dim cmSql As SqlCommand
        Dim strsql As String
        Try
            ''...  Call DropLoginUser(server)
            ''...  rmtuser = UserCode & Ccomp & CYear
            If UCase(server) = UCase(gbServer) Then Exit Sub '..If Linked Server =local server Exit
            strsql = "if exists(select * from master.dbo.sysservers where srvname='" & server & "')"
            strsql = strsql + "EXEC sp_droplinkedsrvlogin '" & server & "' ,null"

            cnSql = New SqlConnection(ConnectionString)
            cnSql.Open()

            cmSql = New SqlCommand(strsql, cnSql)

            cmSql.ExecuteNonQuery()
        Catch ex As Exception
            Throw ex
        Finally
            Disposecontrols(cnSql, cmSql)
        End Try
    End Sub
    Public Sub DropServer(server As String)
        Dim cnSql As SqlConnection
        Dim cmSql As SqlCommand
        Dim strsql As String
        Try
            '..If Linked Server =local server Exit
            If UCase(server) = UCase(gbServer) Then Exit Sub
            strsql = "if exists(select * from master.dbo.sysservers where srvname='" & server & "')"
            strsql = strsql + "EXEC sp_dropserver '" & server & "'"

            cnSql = New SqlConnection(ConnectionString)
            cnSql.Open()

            cmSql = New SqlCommand(strsql, cnSql)

            cmSql.ExecuteNonQuery()

        Catch ex As Exception
            Throw ex
        Finally
            Disposecontrols(cnSql, cmSql)
        End Try
    End Sub
    Public Sub AddLinkedServer(server As String)
        Try
            '..If Linked Server =local server Exit
            If UCase(server) = UCase(gbServer) Then Exit Sub
            strsql = " if not exists(select * from master.dbo.sysservers where srvname='" & server & "')"
            strsql = strsql + " EXEC SP_addlinkedserver [" & server & "]"
            Using cnSql = New SqlConnection(ConnectionString)
                cnSql.Open()
                Using cmSql = New SqlCommand(strsql, cnSql)
                    cmSql.ExecuteNonQuery()
                End Using
            End Using
        Catch ex As Exception
            Throw ex
        End Try
    End Sub
    Public Sub AddLinkedSrvLogin(server As String, Auth As String, rmtuser As String, ByRef rmtpassword As String, Optional Decrypt As String = "")
        Try
            ''...Call CreateLoginUser(server)
            ''...rmtuser = UserCode & Ccomp & CYear
            If server = gbServer Then Exit Sub '..If Linked Server =local server Exit
            strsql = "if not exists(SELECT  t2.[srvname] FROM    master.dbo.syslogins T1"
            strsql &= " INNER JOIN master.dbo.sysservers T2 ON T1.sid = T2.srvid where srvname='" & server & "' and T1.[Name]='" & rmtuser & "')"
            If Auth = "Y" Then '...window authentication
                strsql = strsql + " EXEC sp_addlinkedsrvlogin [" & server & "],'false',NULL"
            Else '...sa
                strsql = strsql + " EXEC sp_addlinkedsrvlogin [" & server & "],'false',NULL,'" & rmtuser & "',"
                If Decrypt = "" And rmtpassword <> "" Then
                    strsql = strsql + " '" & Decpassword(rmtpassword) & "'"
                Else
                    strsql = strsql + " '" & rmtpassword & "'"
                End If
            End If
            Using cnSql = New SqlConnection(ConnectionString)
                cnSql.Open()
                Using cmSql = New SqlCommand(strsql, cnSql)
                    cmSql.ExecuteNonQuery()
                End Using
            End Using
        Catch ex As Exception
            Throw ex
        End Try
    End Sub
    Public Function LinkServer(server As String, _
                         rmtuser As String, rmtpassword As String, catalog As String, Auth As String, Index As Integer, Optional Decrypt As String = "", _
                         Optional ByRef linkServ As String = "") As Boolean
        Try
            ' ...create Link Server
            Call AddLinkedServer(server)
            '... Log in To Server
            Call AddLinkedSrvLogin(server, Auth, rmtuser, rmtpassword, Decrypt)
            '...verify If DB is valid
            If UCase(server) <> UCase(gbServer) Then
                strsql = "select  * from [" + server + "].[master].dbo.sysdatabases Where name ='" & catalog & "'"
            Else
                strsql = "select  * from [master].dbo.sysdatabases Where name ='" & catalog & "'"
                If CheckConnection(server, catalog, CBool(IIf(Auth = "Y", True, False)), rmtuser, rmtpassword) = False Then
                    MsgBox("Invalid Connection Infos, check communication data", vbInformation)
                    Exit Function
                End If
            End If
            Using Cnsql As New SqlConnection(ConnectionString)
                Cnsql.Open()
                Using Dasql As New SqlDataAdapter(strsql, Cnsql)
                    Using dt As New DataTable
                        Dasql.Fill(dt)
                        If dt.Rows.Count = 0 Then
                            MsgBox("Invalid database name", vbInformation)
                            '..drop log in user
                            droplinkedsrvlogin(server)
                            '...drop linked server
                            DropServer(server)
                        Else
                            LinkServer = True

                            If UCase(server) <> UCase(gbServer) Then
                                linkServ = "[" & server & "].[" & catalog & "].dbo."
                            Else
                                linkServ = "[" & catalog & "].dbo."
                            End If

                        End If
                    End Using
                End Using
            End Using
        Catch ex As Exception
            '..drop log in user
            droplinkedsrvlogin(server)
            '...drop linked server
            DropServer(server)
            MessageBox.Show("Invalid Connection Infos, check communication data", "Open Connection With Accounting DB", MessageBoxButtons.OK, MessageBoxIcon.Information)
        End Try

    End Function
    Public Function CheckConnection(ByVal SrverName As String, ByVal DBName As String, ByVal WinAuth As Boolean, Optional ByVal UserName As String = "", Optional ByVal PWD As String = "") As Boolean
        Dim ConnStr As String

        If WinAuth = False Then
            ConnStr = "Data Source=" + Replace(SrverName, "'", "''") + ";Initial Catalog=" + Replace(DBName, "'", "''") + ";User ID=" + Replace(UserName, "'", "''") + ";Password=" + Replace(PWD, "'", "''")
        Else
            ConnStr = "Data Source=" + Replace(SrverName, "'", "''") + ";Initial Catalog=" + Replace(DBName, "'", "''") + ";Integrated Security=True" + ";User ID=" + Replace(UserName, "'", "''")
        End If

        Dim Conn As New SqlConnection(ConnStr)
        Dim CM As System.Data.SqlClient.SqlCommand
        Try
            Conn.Open()
            CM = New System.Data.SqlClient.SqlCommand()
            CheckConnection = CBool(CM.Connection.State.Open)
        Catch Ex As System.Exception
            CheckConnection = False
        Finally
            Disposecontrols(Conn, CM)
        End Try
    End Function

    Public Sub LinkServerGlobalVariables(ByRef ServerLink As String, ByRef DBnameLink As String, _
                                   ByRef SQLpswdLink As String, ByRef SQLuserLink As String, ByRef SQLAuth As String, Optional comp As String = "")
        Try
            If comp = "" Then comp = gbComp

            strsql = " SELECT AG_SQLServerName,AG_DBName,AG_USerName,AG_Password,AG_WindowsAuth FROM dbo.STK04_AcGen WHERE AG_Comp='" & comp & "'"
            Using Cnsql As New SqlConnection(ConnectionString)
                Cnsql.Open()
                Using Dasql As New SqlDataAdapter(strsql, Cnsql)
                    Using dt As New DataTable
                        Dasql.Fill(dt)
                        If dt.Rows.Count > 0 Then
                            For Each DR As DataRow In dt.Rows
                                ServerLink = DR("AG_SQLServerName").ToString
                                DBnameLink = DR("AG_DBName").ToString
                                SQLpswdLink = Decpassword(DR("AG_Password").ToString)
                                SQLuserLink = DR("AG_USerName").ToString
                                SQLAuth = DR("AG_WindowsAuth").ToString
                            Next
                        End If
                    End Using
                End Using
            End Using
        Catch ex As Exception
            Throw ex
        End Try
    End Sub
#End Region


    ' add by ty on 18/07/2016
    Public Function NegationPriv(ByVal _stepname As String, ByVal _button As String, Optional _Server As String = "") As Boolean
        Dim strsql As String
        Dim value As Boolean
        Try
            strsql = " SELECT Nbu_hasNeg FROM " & _Server & "Com_NegationButton"
            strsql &= " WHERE Nbu_User ='" & gbUser & "' AND Nbu_Comp='" & gbComp & "'"
            strsql &= " AND Nbu_Appl='" & gbApp & "' AND Nbu_Module='" & gbApp & "' AND Nbu_Step='" & _stepname & "'"
            strsql &= " AND Nbu_Button='" & _button & "' AND Nbu_Exception=1"

            If RecordsExists(strsql) = True Then
                If ExecuteScalar(strsql).ToString = "1" Then
                    value = True
                Else
                    value = False
                End If
            Else
                strsql = " SELECT * FROM " & _Server & "Com_GroupNegButton INNER JOIN Com_UserInfoGroup ON Nbg_Group=UIL_GrpCode"
                strsql &= " WHERE UIL_UserCode ='" & gbUser & "' AND Nbg_Comp='" & gbComp & "'"
                strsql &= " AND Nbg_Appl='" & gbApp & "' AND Nbg_Module='" & gbApp & "' AND Nbg_Step='" & _stepname & "'"
                strsql &= " AND Nbg_Button='" & _button & "'"
                If RecordsExists(strsql) = True Then
                    value = True
                Else
                    value = False
                End If
            End If
            Return value
        Catch ex As Exception
            Throw ex
        End Try

    End Function


    Public Sub RefocusEmptyColonFilterbarClick(ByRef Grid As C1TrueDBGrid, ByVal col() As String, ByRef fixedcol As String, Optional ByVal cond As String = "")
        Try
            Dim a As Integer
            a = Grid.Splits(0).Rows.Count - 2
            If Grid.FilterActive = True Then
                For Each colun As String In col
                    If Grid(a, colun).ToString.Trim = "" AndAlso Grid(a, fixedcol).ToString.Trim <> "" Then
                        Grid.FilterActive = False
                        Grid.Row = a
                        Grid.Col = Grid.Columns.IndexOf(Grid.Columns(colun))
                        Exit Try
                    End If
                Next
                If cond.Trim <> "" AndAlso Grid(a, cond).ToString.Trim = "" AndAlso Grid(a, fixedcol).ToString.Trim <> "" Then
                    Grid.FilterActive = False
                    Grid.Row = a
                    Grid.Col = Grid.Columns.IndexOf(Grid.Columns(col(col.Length - 1)))
                    Exit Try
                End If
            End If

        Catch ex As Exception
            Throw ex
        End Try
    End Sub

    Public Sub getimageLogo()
        Dim myimage As Byte()
        Dim cnsql As SqlConnection
        Dim cmsql As SqlCommand
        Dim strsql As String
        Dim sqlms As New MemoryStream
        Dim Path, tmppath As String
        Dim chks1, chks2 As String
        Try

#If DEBUG Then
            ImagesPath = gbExePath & "\" & gbComp & "Logo.jpg"
#Else
            ImagesPath = gbExePath & "\" & gbComp & "Logo.jpg"
#End If
            '  Dim FI As New System.IO.FileInfo()
            tmppath = Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData) & "\" & RandomString(1, 10) & ".jpg"
            'chks1 = GetChecksum(tmppath)
            'chks2 = GetChecksum(tmppath)
            'If chks1 <> chks2 Then
            'End If
            cnsql = New SqlConnection(ConnectionString)
            cnsql.Open()
            strsql = "SELECT L_Img FROM COm_Logo WHERE L_Comp ='" & gbComp & "'"
            cmsql = New SqlCommand(strsql, cnsql)
            myimage = DirectCast(cmsql.ExecuteScalar(), Byte())
            If myimage Is Nothing Then
                ImagesPath = ""
                Exit Try
            End If
            Using fs1 As FileStream = New FileStream(tmppath, FileMode.Create)
                fs1.Write(myimage, 0, myimage.Length)
            End Using
            While FileIsInUse(ImagesPath)
            End While
            File.Delete(ImagesPath)
            Rename(tmppath, ImagesPath)
            File.Delete(tmppath)
            'sqlms = New MemoryStream(myimage, 0, myimage.Length)
            'sqlms.Write(myimage, 0, myimage.Length)
            'Dim writer As BinaryWriter = New BinaryWriter(File.OpenWrite(Path))
            'writer.Write(myimage)
            'writer.Flush()
            'writer.Close()
        Catch ex As Exception
            MessageBox.Show(ex.Message, "Load", MessageBoxButtons.OK, MessageBoxIcon.Error)
        End Try
    End Sub
    Private Function RandomString(ByVal minCharacters As Integer, ByVal maxCharacters As Integer) As String
        Dim s As String = System.Guid.NewGuid.ToString().Replace("-", "")
        Static r As New Random
        Dim chactersInString As Integer = r.Next(minCharacters, maxCharacters)
        Dim sb As New System.Text.StringBuilder
        Try
            For i As Integer = 1 To maxCharacters
                Dim idx As Integer = r.Next(0, s.Length)
                sb.Append(s.Substring(idx, 1))
            Next
            Return sb.ToString()
        Catch ex As Exception
            Throw ex
        End Try
    End Function
    Private Function GetChecksum(fname As String) As String
        Try
            Using stream As FileStream = File.OpenRead(fname)
                Dim sha = New System.Security.Cryptography.SHA256Managed()
                Dim checksum As Byte() = sha.ComputeHash(stream)
                Return BitConverter.ToString(checksum).Replace("-", [String].Empty)
            End Using
        Catch ex As Exception
            Throw ex
        End Try
    End Function
    Private Function FileIsInUse(ByVal path As String) As Boolean
        Try
            Using fs As FileStream = New FileStream(path, FileMode.Open)
                Return False
            End Using
        Catch ex As Exception
            Return True
        End Try
    End Function
    Public Sub GetContactsFromOutlook()
        Try

            gbdtContacts = HosDLL.HOSDLLModvar.OutlookInitialAddressList

        Catch ex As Exception

        End Try
    End Sub

    Public Sub ShowNotification(_Owner As System.Windows.Forms.Form,
                                _Message As String,
                                Optional _Duration As Integer = 3000,
                                Optional _FontSize As Integer = 12,
                                Optional _Position As DevComponents.DotNetBar.eToastPosition = eToastPosition.MiddleCenter)
        '...Jad JE
        Try
            ToastNotification.DefaultToastGlowColor = eToastGlowColor.Blue
            ToastNotification.ToastBackColor = Color.DarkBlue
            ToastNotification.ToastForeColor = Color.White
            ToastNotification.DefaultTimeoutInterval = _Duration
            ToastNotification.ToastFont = New Font("Arial", _FontSize)
            ToastNotification.Show(_Owner, _Message, _Position)
        Catch ex As Exception
            MessageBox.Show(ex.Message, "Show Notification", MessageBoxButtons.OK, MessageBoxIcon.Error)
        End Try
    End Sub

    Public Sub ShowBalloon(_Owner As System.Windows.Forms.Form, _
                _Title As String, _Message As String, _AutoCloseTimeOut As Integer, _FontSize As Integer)
        '...Jad JE...08 02 2017
        Try
            Dim b As DevComponents.DotNetBar.Balloon = New DevComponents.DotNetBar.Balloon()
            Dim r As Rectangle = Screen.GetWorkingArea(_Owner)
            With b
                .Icon = SystemIcons.Information
                .Style = eBallonStyle.Office2007Alert
                .TipPosition = eTipPosition.Top
                .AlertAnimation = eAlertAnimation.BottomToTop
                .Font = New Font("Arial", _FontSize)
                .CaptionText = _Title
                .Text = _Message
                .Width = 450
                .Height = 150
                .Enabled = True
                .AutoClose = True
                .ShowCloseButton = True
                .AutoCloseTimeOut = _AutoCloseTimeOut
                .Name = "Notification_Balloon"
                .Owner = _Owner
                .Location = New Point(CInt((r.Width / 2) - (.Width / 2)), CInt((r.Height / 2) - (.Height / 2)))
                .Show()
            End With
        Catch ex As Exception
            MessageBox.Show(ex.Message, "Show Balloon", MessageBoxButtons.OK, MessageBoxIcon.Error)
        End Try
    End Sub

    Public Sub GlobalFillIngData()
        Dim strsql As String
        Dim cnsql As SqlConnection
        Dim cbsql As SqlCommandBuilder
        Try
            GlobalDtIsCompleted = False
            DropSqlObject(GlobalTempTable, "TABLE")

            strsql = "CREATE TABLE [dbo]." & GlobalTempTable
            strsql &= " ([ChkPO]    [varchar](5) NULL ,"
            strsql &= " [ChkPI]    [varchar](5) NULL ,"
            'ch @ 03/07/2017
            strsql &= " [ChkStat]    [varchar](5) NULL ,"
            strsql &= " [Recid]   [numeric](18, 0) NOT NULL,"
            'Stock Item Key
            For i = 0 To CurrItmKey.Length - 1
                strsql &= CurrItmKey(i) & "[varchar] (20) NULL,"
            Next
            strsql &= " [Description]     [nvarchar](100) NULL," ' wadih @ 29/02/2016
            strsql &= " [Barcode]         [varchar](40) NULL,"
            strsql &= " [Classification]  [varchar](7) NULL,"
            strsql &= " [Archive]         [varchar](7) NULL,"
            strsql &= " [Active]	      [varchar](5) NULL,"
            strsql &= " [Item Type]       [varchar](3) NULL,"
            strsql &= " [STK_BookId]      [numeric](18, 0) NULL,"
            strsql &= " [New]	          [varchar](5) NOT NULL,"
            strsql &= " [BrandCode]	      [varchar](3) NULL"
            strsql &= ",[Season]	      [varchar](2) NULL" 'added by GS on 01/08/14
            strsql &= ",[Style]	      [varchar](3) NULL"
            strsql &= ",[Gender]	      [varchar](2) NULL"
            If Not CurrItmKey.Contains("STK_Category", StringComparer.OrdinalIgnoreCase) Then
                strsql &= ",[STK_Category]	      [varchar](4) NULL"
            End If
            strsql &= ",[ItmIdent]        [numeric](18, 0) NOT NULL"
            strsql &= " CONSTRAINT  PK_" & GlobalTempTable
            strsql &= " PRIMARY KEY clustered ([Recid], [New]) "
            strsql &= ") ON [PRIMARY]"
            strsql &= "       "
            Execute_WOXACT(strsql)
            strsql = "INSERT INTO " & GlobalTempTable
            strsql &= " select  'False' ChkPO, 'False' ChkPI,'False' ChkStat, STK_RECID,"
            For i = 0 To CurrItmKey.Length - 1
                strsql &= CurrItmKey(i) & ","
            Next
            strsql &= " STK_DESC,"
            strsql &= " (SELECT TOP 1 dbar_barcode FROM Stk04_DetBarcode"
            strsql &= " WHERE STK_RECID = dbar_stkid) AS Barcode,"
            strsql &= " CASE STK_ItmClass WHEN 'F' THEN 'Fashion' WHEN 'B' THEN 'Book'"
            strsql &= " WHEN 'T' THEN 'Trading' WHEN 'E' THEN 'Expiry' END AS Classification,"
            strsql &= " CASE WHEN STK_CurrentArchive = 'A' THEN 'True' ELSE 'False' END AS Archive,"
            strsql &= " CASE WHEN STK_ACTIVE = 'Y' THEN 'True' ELSE 'False' END AS Active,"
            strsql &= " STK_ITMTYPE, STK_BookId, 'False', STK_Brand AS BrandCode"
            strsql &= ",STK_Season ,stk_style ,stk_gender " 'added by GS on 01/08/14
            If Not CurrItmKey.Contains("STK_Category", StringComparer.OrdinalIgnoreCase) Then
                strsql &= ", stk_Category "
            End If
            strsql &= ", STK_ITMIDENT "
            strsql &= " FROM STK04_STKFIL   ;    "

            strsql &= "INSERT INTO " & GlobalTempTable
            strsql &= " SELECT 'False' ChkPO, 'False' ChkPI,'False' ChkStat, SFP_RECID,"
            For i = 0 To CurrItmKey.Length - 1
                strsql &= CurrItmKeyPend(i) & ","
            Next
            strsql &= " SFP_DESC, NULL,"
            strsql &= " CASE SFP_ItmClass WHEN 'F' THEN 'Fashion' WHEN 'B' THEN 'Book'"
            strsql &= " WHEN 'T' THEN 'Trading' WHEN 'E' THEN 'Expiry' END AS Classification,"
            strsql &= " 'False' AS [Archive], 'True' AS Active, SFP_ItmType,"
            strsql &= " NULL, 'True', SFP_Brand AS BrandCode"
            strsql &= ",SFP_Season ,sfp_style ,sfp_gender " 'added by GS on 01/08/14
            If Not CurrItmKey.Contains("STK_Category", StringComparer.OrdinalIgnoreCase) Then
                strsql &= ", sfp_Category "
            End If
            strsql &= ", SFP_ItmIdent"
            strsql &= " FROM STK04_STKFILPending      "
            Execute_WOXACT(strsql)
            strsql = " select * from " & GlobalTempTable & " "
            cnsql = New SqlConnection(ConnectionString)
            cnsql.Open()
            GlobalDa = New SqlDataAdapter(strsql, cnsql)
            If GlobalDatatable Is Nothing Then
                GlobalDatatable = New DataTable
                GlobalDatatable.TableName = "Items"
                GlobalDa.Fill(GlobalDatatable)
                cbsql = New SqlCommandBuilder(GlobalDa)
                GlobalDa.UpdateCommand = cbsql.GetUpdateCommand
                DV_JurnaOfPI = New DataView
                DV_JurnaOfPI.Table = GlobalDatatable
                DV_JurnaOfPO = New DataView
                DV_JurnaOfPO.Table = GlobalDatatable
                DV_JurnaOfStat = New DataView
                DV_JurnaOfStat.Table = GlobalDatatable
            Else
                GlobalDatatable.Rows.Clear()
                GlobalDa.Fill(GlobalDatatable)
                cbsql = New SqlCommandBuilder(GlobalDa)
                GlobalDa.UpdateCommand = cbsql.GetUpdateCommand
            End If
            GlobalDtIsCompleted = True
        Catch ex As Exception
            If ex.Message.Contains("Invalid object name 'PurGlobalTemp") = False Then
                MessageBox.Show(ex.Message, CntTitle, MessageBoxButtons.OK, MessageBoxIcon.Error)
            End If

        Finally
            GlobalDtIsCompleted = True
        End Try
    End Sub
    Public Sub itemValidation(ByVal Refernce As String, Optional ByVal TempTable As String = "", Optional ByRef Dt As DataTable = Nothing, Optional ByVal StkFillCondition As String = "" _
                             , Optional ByRef STK_ACTIVE As String = "Y", Optional ByVal STK_CurrentArchive As String = "C" _
                             , Optional ByVal suppRecId As String = "")
        Dim strsql As String
        Dim PrepTempTable As String
        Try
            If TempTable.Trim = "" AndAlso Dt Is Nothing Then
                MessageBox.Show("Missing TempTable Or DataTable Name", CntTitle, MessageBoxButtons.OK, MessageBoxIcon.Information)
                Exit Try
            End If
            If TempTable.Trim <> "" AndAlso Not Dt Is Nothing Then
                MessageBox.Show("You Can Not Use TempTable And DataTable At Once", CntTitle, MessageBoxButtons.OK, MessageBoxIcon.Information)
                Exit Try
            End If
            PrepTempTable = "PrepTempTable_" & TempTable & "_" & gbUser
            If TempTable.Trim <> "" Then
                DropSqlObject(TempTable, "TABLE")
            End If
            DropSqlObject(PrepTempTable, "TABLE")
            strsql = " DECLARE @tempVariabl VARCHAR(MAX)"
            strsql &= " SET @tempVariabl = '" & Refernce.Trim.Replace("'", "''") & "'"
            strsql &= " SELECT  STK_RECID RECID ,"
            strsql &= " STQ_Color Color ,"
            strsql &= " STQ_DetGrid DetGrid ,"
            strsql &= " STQ_Grid Grid ,"
            strsql &= " STQ_ExpiryDt ExpiryDt ,"
            strsql &= " STQ_BatchInfo BatchInfo"
            strsql &= " INTO    " & PrepTempTable & ""
            strsql &= " FROM    ( SELECT    STK_RECID ,"
            strsql &= " STK_SingleRef1 SingleRef1 ,"
            strsql &= " STK_SingleRef2 SingleRef2 ,"
            strsql &= " STK_REF REF"
            strsql &= " FROM      dbo.STK04_STKFIL"
            strsql &= " WHERE     STK_ACTIVE = '" & STK_ACTIVE & "'"
            strsql &= " AND STK_CurrentArchive = '" & STK_CurrentArchive & "'"
            strsql &= " AND ( STK_REF = @tempVariabl"
            strsql &= " OR STK_SingleRef1 = @tempVariabl"
            strsql &= " OR STK_SingleRef2 = @tempVariabl"
            strsql &= " )"
            strsql &= " ) STK04_STKFIL"
            strsql &= " left JOIN ( SELECT STQ_STKRECID ,"
            strsql &= " STQ_Color ,"
            strsql &= " STQ_DetGrid ,"
            strsql &= " STQ_Grid ,"
            strsql &= " STQ_ExpiryDt ,"
            strsql &= " STQ_BatchInfo"
            strsql &= " FROM   stk04_stkqty"
            strsql &= " WHERE  STQ_LOCCODE = '00'"
            strsql &= " ) stk04_stkqty ON stk04_stkqty.STQ_STKRECID = STK_RECID"

            If gbBook = True Then
                strsql &= " union"
                strsql &= " SELECT  STK_RECID RECID ,"
                strsql &= " NULL Color ,"
                strsql &= " NULL DetGrid ,"
                strsql &= " NULL Grid ,"
                strsql &= " NULL ExpiryDt ,"
                strsql &= " NULL BatchInfo"
                strsql &= " "
                strsql &= " FROM    ( SELECT    STK_RECID ,"
                strsql &= " STK_SingleRef1 SingleRef1 ,"
                strsql &= " STK_SingleRef2 SingleRef2 ,"
                strsql &= " STK_REF REF,STK_BOOKID"
                strsql &= " FROM      dbo.STK04_STKFIL"
                strsql &= " WHERE     STK_ACTIVE = '" & STK_ACTIVE & "'"
                strsql &= " AND STK_CurrentArchive = '" & STK_CurrentArchive & "'"
                strsql &= " "
                strsql &= " ) STK04_STKFIL"
                strsql &= " LEFT OUTER JOIN ( SELECT    BO_ISBN ,"
                strsql &= " BO_Recid ,"
                strsql &= " BO_type"
                strsql &= " FROM      Bibl1_Book WHERE BO_ISBN = @tempVariabl "
                strsql &= " ) AS Bibl1_Book ON STK04_STKFIL.STK_BOOKID = Bibl1_Book.BO_Recid"
                strsql &= " WHERE BO_ISBN = @tempVariabl"
            End If



            strsql &= " UNION"
            strsql &= " SELECT  STK_RECID RECID ,"
            strsql &= " dbar_color Color ,"
            strsql &= " dbar_Detgrid DetGrid ,"
            strsql &= " dbar_grid Grid ,"
            strsql &= " dbar_expdate ExpiryDt ,"
            strsql &= " dbar_batch BatchInfo"
            strsql &= " FROM    ( SELECT    STK_RECID ,"
            strsql &= " STK_SingleRef1 SingleRef1 ,"
            strsql &= " STK_SingleRef2 SingleRef2 ,"
            strsql &= " STK_REF REF"
            strsql &= " FROM      dbo.STK04_STKFIL"
            strsql &= " WHERE     STK_ACTIVE = '" & STK_ACTIVE & "'"
            strsql &= " AND STK_CurrentArchive = '" & STK_CurrentArchive & "'"
            strsql &= " ) STK04_STKFIL"
            strsql &= " INNER JOIN ( SELECT *"
            strsql &= " FROM   STK04_DetBarcode"
            strsql &= " WHERE  dbar_barcode = @tempVariabl"
            strsql &= " ) STK04_DetBarcode ON STK04_DetBarcode.dbar_stkid = STK_RECID"


            strsql &= " UNION"
            strsql &= " SELECT  STK_RECID RECID ,"
            strsql &= " STQ_Color Color ,"
            strsql &= " STQ_DetGrid DetGrid ,"
            strsql &= " STQ_Grid Grid ,"
            strsql &= " STQ_ExpiryDt ExpiryDt ,"
            strsql &= " STQ_BatchInfo BatchInfo"
            strsql &= " FROM    ( SELECT    STK_RECID ,"
            strsql &= " STK_SingleRef1 SingleRef1 ,"
            strsql &= " STK_SingleRef2 SingleRef2 ,"
            strsql &= " STK_REF REF"
            strsql &= " FROM      dbo.STK04_STKFIL"
            strsql &= " WHERE     STK_ACTIVE = '" & STK_ACTIVE & "'"
            strsql &= " AND STK_CurrentArchive = '" & STK_CurrentArchive & "'"
            strsql &= " ) STK04_STKFIL"
            strsql &= " left JOIN ( SELECT STQ_STKRECID ,"
            strsql &= " STQ_Color ,"
            strsql &= " STQ_DetGrid ,"
            strsql &= " STQ_Grid ,"
            strsql &= " STQ_ExpiryDt ,"
            strsql &= " STQ_BatchInfo"
            strsql &= " FROM   stk04_stkqty"
            strsql &= " WHERE  STQ_LOCCODE = '00'"
            strsql &= " ) stk04_stkqty ON stk04_stkqty.STQ_STKRECID = STK_RECID"
            strsql &= " INNER JOIN ( SELECT SRef_StkRecId"
            strsql &= " FROM   STK04_StkSupRef"
            strsql &= " WHERE  SRef_SupRef = @tempVariabl  "
            If suppRecId.Trim <> "" Then
                strsql &= " and  SRef_SupRecId = '" & suppRecId & "'"
            End If
            strsql &= " GROUP BY SRef_StkRecId"
            strsql &= " ) STK04_StkSupRef ON STK04_StkSupRef.SRef_StkRecId = STK04_STKFIL.STK_RECID;"
            If TempTable.Trim = "" Then
                Execute_WOXACT(strsql)
                strsql = " DECLARE @tempVariabl VARCHAR(MAX)"
                strsql &= " SET @tempVariabl = '" & Refernce.Trim.Replace("'", "''") & "'"
            End If

            strsql &= " SELECT DISTINCT"
            strsql &= " STK_RECID ,"
            strsql &= " STK_REF ,"
            strsql &= " STK_DESC ,"
            strsql &= " STQ_Color ,"
            strsql &= " STQ_DetGrid ,"
            strsql &= " STQ_Grid ,"
            strsql &= " STQ_ExpiryDt ,"
            strsql &= " STQ_BatchInfo ,STK_SingleRef1,"
            strsql &= " ISNULL(SRef_SupRef,SRef_SupRefDouble) Supref"
            strsql &= " , ISNULL(SRef_SupColor,SRef_SupColorDouble) SuprefColor"
            If TempTable.Trim <> "" Then
                strsql &= "  into " & TempTable & " "
            End If


            strsql &= " FROM    ( SELECT    STK_RECID ,"
            strsql &= " STK_REF ,"
            strsql &= " STK_DESC,STK_SingleRef1"


            strsql &= " FROM      dbo.STK04_STKFIL"
            strsql &= AppStkCond("STK_Recid", , True, False, BUAccess.Purchase)

            If StkFillCondition.Trim <> "" AndAlso StkFillCondition.IndexOf("SRef_SupRef") > 0 Then
                strsql &= "  LEFT JOIN ( SELECT  SRef_StkRecId ,"
                strsql &= "                      SRef_SupRef"
                strsql &= "              FROM dbo.STK04_StkSupRef"
                strsql &= "            ) STK04_StkSupRef ON STK04_StkSupRef.SRef_StkRecId = STK04_STKFIL.STK_RECID"
            End If

            strsql &= " WHERE     STK_ACTIVE = '" & STK_ACTIVE & "'"
            strsql &= " AND STK_CurrentArchive = '" & STK_CurrentArchive & "'"
            If StkFillCondition.Trim <> "" Then
                strsql &= " And " & StkFillCondition & " "
            End If
            strsql &= " ) STK04_STKFIL"

            '  If gbFashion = True OrElse gbExpiry = True Then
            strsql &= " left JOIN ( SELECT STQ_STKRECID ,"
            If gbFashion = True Then
                strsql &= " STQ_Color ,"
                strsql &= " STQ_DetGrid ,"
                strsql &= " STQ_Grid ,"
            Else
                strsql &= " NULL STQ_Color ,"
                strsql &= " NULL STQ_DetGrid ,"
                strsql &= " NULL STQ_Grid ,"
            End If

            If gbExpiry = True Then
                strsql &= " STQ_ExpiryDt ,"
                strsql &= " STQ_BatchInfo"
            Else
                strsql &= " NULL STQ_ExpiryDt ,"
                strsql &= " NULL STQ_BatchInfo "
            End If


            strsql &= " FROM   stk04_stkqty"
            strsql &= " WHERE  STQ_LOCCODE = '00'"
            strsql &= " ) stk04_stkqty ON stk04_stkqty.STQ_STKRECID = STK04_STKFIL.STK_RECID"
            ' End If

            strsql &= " INNER JOIN " & PrepTempTable & " ON STK_RECID = RECID"
            ' If gbFashion = True OrElse gbExpiry = True Then
            '  If gbFashion = True Then
            strsql &= " AND ISNULL(STQ_Color,'')=ISNULL(Color,'')"
            strsql &= " AND ISNULL(STQ_DetGrid,'')=ISNULL(STQ_DetGrid,'')"
            strsql &= " AND ISNULL(Grid,'')=ISNULL(Grid,'')"
            ' End If

            ' If gbExpiry = True Then
            strsql &= " AND ISNULL(STQ_ExpiryDt,'')=ISNULL(ExpiryDt,'')"
            strsql &= " AND ISNULL(STQ_BatchInfo,'')=ISNULL(BatchInfo,'')"
            ' End If

            '   End If

            strsql &= " LEFT JOIN ( SELECT  SRef_StkRecId ,"
            strsql &= " SRef_SupRef  , SRef_SupColor , SRef_ItmColor "
            strsql &= " FROM    STK04_StkSupRef"
            strsql &= " WHERE   SRef_SupRef = @tempVariabl"
            If suppRecId.Trim <> "" Then
                strsql &= " and  SRef_SupRecId = '" & suppRecId & "'"
            End If
            strsql &= " GROUP BY SRef_StkRecId ,"
            strsql &= " SRef_SupRef , SRef_SupColor , SRef_ItmColor"
            strsql &= " ) STK04_StkSupRef ON STK04_StkSupRef.SRef_StkRecId = STK04_STKFIL.STK_RECID  AND  COALESCE(SRef_ItmColor,'') = COALESCE(STQ_Color,'')   "
            strsql &= " LEFT JOIN ( SELECT  SRef_StkRecId SRef_StkRecIdDouble ,"
            strsql &= " SRef_SupRef SRef_SupRefDouble , SRef_SupColor SRef_SupColorDouble  "
            strsql &= " FROM    STK04_StkSupRef"
            If suppRecId.Trim <> "" Then
                strsql &= " WHERE  SRef_SupRecId = '" & suppRecId & "'"
            End If
            strsql &= " GROUP BY SRef_StkRecId ,"
            strsql &= " SRef_SupRef , SRef_SupColor"
            strsql &= " ) STK04_StkSupRefDouble ON STK04_StkSupRefDouble.SRef_StkRecIdDouble = STK04_STKFIL.STK_RECID AND  COALESCE(SRef_SupColorDouble,'') = COALESCE(STQ_Color,'')   "
            If TempTable.Trim <> "" Then
                Execute_WOXACT(strsql)
            Else
                Dt = ReadFromDBToDataTable(strsql)
            End If

        Catch ex As Exception
            Throw ex
        Finally
            'DropSqlObject(PrepTempTable, "TABLE")
        End Try
    End Sub

    Public Function DuplicateKey(exp As Exception, MessageTitle As String) As Boolean
        Try
            If (exp.Message.ToLower.IndexOf("violation of primary key constraint") > -1 OrElse _
                exp.Message.ToLower.IndexOf("violation of unique key constraint") > -1 AndAlso _
                           exp.Message.ToLower.IndexOf("cannot insert duplicate key in object") > -1) OrElse
                      exp.Message.ToLower.IndexOf("cannot insert duplicate key row in object") > -1 Then
                MessageBox.Show("This Record Is Already Added By Another User", MessageTitle, MessageBoxButtons.OK, MessageBoxIcon.Exclamation)
                Return True
            ElseIf (exp.Message.ToLower.IndexOf("deadlock victim") > -1 OrElse exp.Message.ToLower.IndexOf("deadlocked") > -1) Then
                MessageBox.Show("Saving Failed. Please Try Again.", MessageTitle, MessageBoxButtons.OK, MessageBoxIcon.Exclamation)
                Return True
            End If
        Catch ex As Exception
            MessageBox.Show("Error : " & exp.Message, "ModVar", _
                MessageBoxButtons.OK, MessageBoxIcon.Error)
        End Try
    End Function

#Region "Open Day BO"

    Public Sub OpenDayCashBO(_Cloc As String, ByRef _WorkingDay As String, ByVal cnSqlTrans As SqlConnection, ByVal cmSqlTrans As SqlCommand, ByVal trnSql As SqlTransaction)
        Dim txttime As String, txtOper As String, TxtSettype As String, TxtCurr As String
        Dim OpenValue As Double
        Dim cnSql As SqlConnection
        Dim cmSql As SqlCommand
        Dim drSql As SqlDataReader
        Dim cmSql2 As SqlCommand
        Dim drSql2 As SqlDataReader
        Dim cnSql3 As SqlConnection
        Dim cmSql3 As SqlCommand
        Dim drSql3 As SqlDataReader
        Dim cashRegister As String

        Dim DtCashRegO As DataTable
        Dim strCashRegO As String

        Try

            txttime = Trim(Str(Hour(Date.Now))) + ":" + Trim(Str(Minute(Date.Now)))

            strCashRegO = "SELECT CR_CODE"
            strCashRegO &= " FROM POS_CASHREG Where CR_loc = '" & _Cloc & "'"
            strCashRegO &= " and CR_active = 'Y' "

            DtCashRegO = ReadFromDBToDataTable(strCashRegO)

            For Each CR In DtCashRegO.Select()

                cashRegister = CR.Item("CR_CODE").ToString.Trim.Replace("'", "''")

                cnSql = New SqlConnection(ConnectionString)
                cnSql.Open()
                cmSql = New SqlCommand(strsql, cnSql)

                strsql = " Select * from stk04_SETTLEMENTTYPE where ST_LOC = '" & _Cloc & "' and st_code <> 'NR1'"
                strsql &= " And ST_Active = 'Y'"

                cmSql.CommandText = strsql
                drSql = cmSql.ExecuteReader

                With drSql
                    If .HasRows Then
                        cmSql2 = New SqlCommand(strsql, cnSqlTrans, trnSql)

                        While .Read
                            TxtCurr = gbCurr1

                            strsql = "select CT_AMOUNT,CT_opdate From POS_CASHTRANSACTION Where "
                            strsql &= " CT_OPDATE in (Select Max(CT_OPDATE) from POS_CASHTRANSACTION "
                            strsql &= " Where CT_LOCATION = '" & _Cloc & "'"
                            strsql &= " and CT_CREGISTER = '" & cashRegister & "'"
                            strsql &= " and CT_SETTLTYPE = '" & .Item("ST_code").ToString & "'"
                            strsql &= " and CT_RECID = 'Z'"
                            strsql &= " and CT_CURRCODE = '" & TxtCurr & "'"
                            strsql &= " and CT_OPDATE < '" & CType(_WorkingDay, Date).ToString("MM/dd/yyyy") & "')"

                            strsql &= " and CT_LOCATION = '" & _Cloc & "'"
                            strsql &= " and CT_CREGISTER = '" & cashRegister & "'"
                            strsql &= " and CT_SETTLTYPE = '" & .Item("ST_code").ToString & "'"
                            strsql &= " and CT_RECID = 'Z'"
                            strsql &= " and CT_CURRCODE = '" & TxtCurr & "'"
                            strsql &= " and CT_OPDATE < '" & CType(_WorkingDay, Date).ToString("MM/dd/yyyy") & "'"

                            cmSql2.CommandText = strsql
                            drSql2 = cmSql2.ExecuteReader

                            With drSql2
                                If .HasRows Then
                                    .Read()
                                    OpenValue = CDbl(.Item("CT_AMOUNT").ToString)
                                Else
                                    OpenValue = 0
                                End If
                            End With

                            drSql2.Close()

                            If OpenValue < 0 Then
                                txtOper = "-"
                                OpenValue = Math.Abs(OpenValue)
                            Else
                                txtOper = "+"
                            End If

                            CVValue = CcRate

                            TxtSettype = .Item("ST_code").ToString

                            InsertCashTrans(_Cloc, 0, cashRegister, _WorkingDay,
                                  "O", txttime, "", txtOper, TxtSettype, TxtCurr, Val(OpenValue), Val(CVValue), "", 0, "",
                                   0, "", 0, "", "", "", "", "", 0, 0, cmSqlTrans)

                            TxtCurr = gbCurr2

                            strsql = "select CT_AMOUNT,CT_opdate From POS_CASHTRANSACTION Where "
                            strsql &= " CT_OPDATE in (Select Max(CT_OPDATE) from POS_CASHTRANSACTION "
                            strsql &= " Where CT_LOCATION = '" & _Cloc & "'"
                            strsql &= " and CT_CREGISTER = '" & cashRegister & "'"
                            strsql &= " and CT_SETTLTYPE = '" & .Item("ST_code").ToString & "'"
                            strsql &= " and CT_RECID = 'Z'"
                            strsql &= " and CT_CURRCODE = '" & TxtCurr & "'"
                            strsql &= " and CT_OPDATE < '" & CType(_WorkingDay, Date).ToString("MM/dd/yyyy") & "')"

                            strsql &= " and CT_LOCATION = '" & _Cloc & "'"
                            strsql &= " and CT_CREGISTER = '" & cashRegister & "'"
                            strsql &= " and CT_SETTLTYPE = '" & .Item("ST_code").ToString & "'"
                            strsql &= " and CT_RECID = 'Z'"
                            strsql &= " and CT_CURRCODE = '" & TxtCurr & "'"
                            strsql &= " and CT_OPDATE < '" & CType(_WorkingDay, Date).ToString("MM/dd/yyyy") & "'"

                            cmSql2.CommandText = strsql
                            drSql2 = cmSql2.ExecuteReader

                            With drSql2
                                If .HasRows Then
                                    .Read()
                                    OpenValue = CDbl(.Item("CT_AMOUNT").ToString)
                                Else
                                    OpenValue = 0
                                End If
                            End With

                            drSql2.Close()

                            If OpenValue < 0 Then
                                txtOper = "-"
                                OpenValue = Math.Abs(OpenValue)
                            Else
                                txtOper = "+"
                            End If
                            CVValue = CcRate

                            TxtSettype = .Item("ST_code").ToString

                            InsertCashTrans(_Cloc, 0, cashRegister, _WorkingDay,
                               "O", txttime, "", txtOper, TxtSettype, TxtCurr, Val(OpenValue), Val(CVValue), "", 0, "",
                                0, "", 0, "", "", "", "", "", 0, 0, cmSqlTrans)


                            If Val(Mid(TxtSettype, 1, 1)) < 3 Then
                                cnSql3 = New SqlConnection(ConnectionString)
                                cnSql3.Open()
                                cmSql3 = New SqlCommand(strsql, cnSql3)

                                strsql = "Select * from Com_DailyRate Where DR_CurCode <> '" & gbCurr1 & "'"
                                strsql &= " and DR_CurCode <> '" & gbCurr2 & "' and DR_Date = '" & CType(_WorkingDay, Date).ToString("MM/dd/yyyy") & "'"

                                cmSql3.CommandText = strsql
                                drSql3 = cmSql3.ExecuteReader()

                                With drSql3
                                    If .HasRows Then
                                        While .Read
                                            TxtCurr = .Item("DR_CurCode").ToString

                                            strsql = "select CT_AMOUNT,CT_opdate From POS_CASHTRANSACTION Where "
                                            strsql &= " CT_OPDATE in (Select Max(CT_OPDATE) from POS_CASHTRANSACTION "
                                            strsql &= " Where CT_LOCATION = '" & _Cloc & "'"
                                            strsql &= " and CT_CREGISTER = '" & cashRegister & "'"
                                            strsql &= " and CT_SETTLTYPE = '" & drSql.Item("ST_code").ToString & "'"
                                            strsql &= " and CT_RECID = 'Z'"
                                            strsql &= " and CT_CURRCODE = '" & TxtCurr & "'"
                                            strsql &= " and CT_OPDATE < '" & CType(_WorkingDay, Date).ToString("MM/dd/yyyy") & "')"

                                            strsql &= " and CT_LOCATION = '" & _Cloc & "'"
                                            strsql &= " and CT_CREGISTER = '" & cashRegister & "'"
                                            strsql &= " and CT_SETTLTYPE = '" & drSql.Item("ST_code").ToString & "'"
                                            strsql &= " and CT_RECID = 'Z'"
                                            strsql &= " and CT_CURRCODE = '" & TxtCurr & "'"
                                            strsql &= " and CT_OPDATE < '" & CType(_WorkingDay, Date).ToString("MM/dd/yyyy") & "'"

                                            cmSql2.CommandText = strsql
                                            drSql2 = cmSql2.ExecuteReader

                                            With drSql2
                                                If .HasRows Then
                                                    .Read()
                                                    OpenValue = CDbl(.Item("CT_AMOUNT").ToString)
                                                Else
                                                    OpenValue = 0
                                                End If
                                            End With
                                            If OpenValue < 0 Then
                                                txtOper = "-"
                                                OpenValue = Math.Abs(OpenValue)
                                            Else
                                                txtOper = "+"
                                            End If

                                            drSql2.Close()

                                            CVValue = CcRate
                                            TxtSettype = drSql.Item("ST_code").ToString

                                            InsertCashTrans(_Cloc, 0, cashRegister, _WorkingDay,
                                                  "O", txttime, "", txtOper, TxtSettype, TxtCurr, Val(OpenValue), Val(CVValue), "", 0, "",
                                                    0, "", 0, "", "", "", "", "", 0, 0, cmSqlTrans)

                                        End While
                                    End If
                                End With
                            End If
                        End While
                    Else
                        'MessageBox.Show("No Settlement Type Defined", CntTitle, MessageBoxButtons.OK, MessageBoxIcon.Exclamation)  Req By Jessy Email : FW: New messages when we open the sales at Back office module 
                    End If
                End With

            Next

        Catch ex As Exception

            Throw ex
        Finally
            If Not drSql Is Nothing Then
                If drSql.IsClosed = False Then drSql.Close()
                drSql = Nothing
            End If

            cmSql = Nothing

            If Not cnSql Is Nothing Then
                If cnSql.State = ConnectionState.Open Then cnSql.Close()
                cnSql = Nothing
            End If

            If Not drSql2 Is Nothing Then
                If drSql2.IsClosed = False Then drSql2.Close()
                drSql2 = Nothing
            End If

            cmSql2 = Nothing

            If Not drSql3 Is Nothing Then
                If drSql3.IsClosed = False Then drSql3.Close()
                drSql3 = Nothing
            End If

            cmSql3 = Nothing

            If Not cnSql3 Is Nothing Then
                If cnSql3.State = ConnectionState.Open Then cnSql3.Close()
                cnSql3 = Nothing
            End If

            If Not DtCashRegO Is Nothing Then
                DtCashRegO.Dispose() : DtCashRegO = Nothing
            End If

        End Try
    End Sub

    Public Sub CheckOpeningAndClosingAtBO(_Cloc As String, _WorkingDay As String, cmsql As SqlCommand)
        Dim LastWD As String
        Dim Msg As String
        Dim LastWDFromTrans As String
        Try

            DayIsOpenedBO = False

            If RecordsExists("SELECT TOP 1 CT_LOCATION  FROM dbo.POS_CASHTRANSACTION WHERE CT_LOCATION='" & _Cloc & "' AND CT_RECID='O' AND CT_OPDATE='" & CDate(_WorkingDay).ToString("MM/dd/yyyy") & "'") = True Then

                If RecordsExists("SELECT TOP 1 CT_LOCATION  FROM dbo.POS_CASHTRANSACTION WHERE CT_LOCATION='" & _Cloc & "' AND CT_RECID='Z' AND CT_OPDATE='" & CDate(_WorkingDay).ToString("MM/dd/yyyy") & "'") = True Then

                    MessageBox.Show("Day Is Already Closed", CntTitle, MessageBoxButtons.OK, MessageBoxIcon.Exclamation)
                    DayIsOpenedBO = False
                    Exit Try

                Else

                    DayIsOpenedBO = True

                End If

            Else

                If RecordsExists(" SELECT TOP 1 1 FROM POS_CASHTRANSACTION WHERE CT_LOCATION = '" & _Cloc & "' AND CT_OPDATE='" & CDate(_WorkingDay).ToString("MM/dd/yyyy") & "'") Then
                    DayIsOpenedBO = True
                    Exit Try
                End If

                If RecordsExists("Select top 1 1 from stk04_SETTLEMENTTYPE where ST_LOC = '" & _Cloc & "' and st_code <> 'NR1' And ST_Active = 'Y'") = False Then

                    LastWDFromTrans = ExecuteScalar("select [Date] from F_GetMaxDate('" & _Cloc & "')")

                    If LastWDFromTrans = "" Then
                        DayIsOpenedBO = True
                        Exit Try
                    End If

                    LastWDFromTrans = CDate(LastWDFromTrans).ToString("dd/MM/yyyy")

                    If CDate(_WorkingDay) < CDate(LastWDFromTrans) Then
                        MessageBox.Show("Working Date ( " & _WorkingDay.ToString & " ) Is Less Than Last Transaction Date ( " & CDate(LastWDFromTrans).ToString("dd/MM/yyyy") & " )", CntTitle, MessageBoxButtons.OK, MessageBoxIcon.Exclamation)
                        DayIsOpenedBO = False
                        Exit Try
                    ElseIf CDate(_WorkingDay) > CDate(LastWDFromTrans) Then
                        DayIsOpenedBO = Not ExistWorkingUser(_Cloc)
                        Exit Try
                    Else
                        DayIsOpenedBO = True
                        Exit Try
                    End If

                End If

                LastWD = GetFieldDesc("POS_CASHTRANSACTION", "CT_LOCATION='" & _Cloc & "' AND CT_RECID='O'", "MAX(CT_OPDATE)")

                'Msg = ""
                'If LastWD.Trim <> "" Then
                '    Msg &= "Last Working Day ( "
                '    Msg &= CDate(LastWD).ToString("dd/MM/yyyy")
                '    Msg &= " ) Will Be Closed, "
                'End If
                'Msg &= "New Working Day ( "
                'Msg &= CDate(_WorkingDay).ToString("dd/MM/yyyy")
                'Msg &= " ) Will Be Opened, Continue ?"

                'If MessageBox.Show(Msg, CntTitle, MessageBoxButtons.YesNo, MessageBoxIcon.Question) = DialogResult.Yes Then

                If ExistWorkingUser(_Cloc) = True Then
                    DayIsOpenedBO = False
                    Exit Try
                End If

                If LastWD.Trim <> "" Then
                    cmsql.CommandText = $"EXECUTE dbo.SP_OpenCloseWorkingDayBackOffice 
                                                       '{_Cloc}', 
                                                       'Z' , 
                                                       '{CDate(LastWD).ToString("MM/dd/yyyy")}' , 
                                                       '{gbUser}'"
                    cmsql.ExecuteNonQuery()
                End If

                cmsql.CommandText = $"EXECUTE dbo.SP_OpenCloseWorkingDayBackOffice 
                                                       '{_Cloc}', 
                                                       'O' , 
                                                       '{CDate(_WorkingDay).ToString("MM/dd/yyyy")}' , 
                                                       '{gbUser}'"
                cmsql.ExecuteNonQuery()

                DayIsOpenedBO = True

                'Else

                '    DayIsOpenedBO = False

                'End If


            End If

        Catch ex As Exception
            MessageBox.Show("Error: " & ex.Message, CntTitle, MessageBoxButtons.OK, MessageBoxIcon.Error)
        End Try

    End Sub

    Public Function ExistWorkingUser(_Cloc As String) As Boolean
        'check if Exist users working on the system
        Dim strU, UsersList As String
        Dim DtU As DataTable

        Try

            strU = " SET TRANSACTION ISOLATION LEVEL READ UNCOMMITTED "
            strU &= " Select [User_Name] UserName From ComUserLogDB.dbo.com_userloggingPerLoc "
            strU &= " INNER JOIN dbo.Com_UserInfo ON User_Code COLLATE SQL_Latin1_General_CP1256_CI_AS = LLog_User COLLATE SQL_Latin1_General_CP1256_CI_AS"
            strU &= " Where LLog_Database = '" & gbDBname & "' and LLog_Company = '" & gbComp & "' and LLog_year = '" & gbYear & "' and LLog_Location = '" & _Cloc & "'  "

            DtU = ReadFromDBToDataTable(strU)

            If DtU.Rows.Count > 0 Then

                For Each rw In DtU.Select()
                    UsersList &= ", " & rw.Item("UserName").ToString
                Next

                UsersList = Mid(UsersList, 2)

                MessageBox.Show("Cannot Open A New Day Since The Following Users Are Working On the System :" _
                                & UsersList & ". They Need to Logout Of The System to Open a New Day",
                                "POS", MessageBoxButtons.OK, MessageBoxIcon.Warning)

                Return True

            Else

                Return False

            End If

        Catch ex As Exception
            Throw ex
        Finally

            If Not DtU Is Nothing Then
                DtU.Dispose()
                DtU = Nothing
            End If

        End Try

    End Function

    Public Sub InsertCashTrans(ByVal cLoc As String, ByVal InvHeadNo As Long, ByVal CshReg As String, _
             ByVal WorkDate As String, ByVal opType As String, ByVal tTime As String, ByVal InvNo As String, _
             ByVal opSign As String, ByVal SetType As String, ByVal Curr As String, ByVal Amount As Double, _
             ByVal CVAmount As Double, ByVal rvType As String, ByVal rvNo As Long, ByVal ChkNo As String, _
             ByVal Bank As Long, ByVal ValueDate As String, ByVal NRID As Long, ByVal NRRef As String, _
             ByVal DueDate As String, ByVal SignDate As String, ByVal GuarName As String, ByVal AuthoStr As String, _
             ByVal ContHeadid As Long, ByVal BonAchatSer As Long, Optional ByVal cmSqlTrans As SqlCommand = Nothing)

        Dim cnSql As SqlConnection
        Dim cmSql As SqlCommand
        Dim strsql As String
        Try

            strsql = " Declare @CashNo as numeric "
            strsql &= " select @CashNo = isnull(Max(CT_OPNBR),0) + 1 from POS_CASHTRANSACTION WHERE CT_LOCATION='" & cLoc & "'"
            strsql &= " Insert Into POS_CASHTRANSACTION "
            strsql &= " Values ('" & cLoc & "'" 'location
            strsql &= ", " & InvHeadNo 'invoice rec
            strsql &= ",'" & CshReg & "'" 'cast reg
            strsql &= ", @CashNo"  'cashserno
            strsql &= ",'" & CType(WorkDate, Date).ToString("MM/dd/yyyy") & "'" 'opening date
            strsql &= ",'" & opType & "'"
            strsql &= ",'" & tTime & "'" 'txttime
            strsql &= " , '" & InvNo & "'"
            strsql &= ",'" & opSign & "'"  'operation sign
            strsql &= ",'" & SetType & "'" 'settlement type
            strsql &= ",'" & Curr & "'" 'txtcurr
            strsql &= "," & Amount & "" 'open value
            strsql &= "," & CVAmount & "" 'value in lc
            strsql &= ",'" & rvType & "'" 'rv type
            If rvNo = 0 Then
                strsql &= ",Null"  'rv #
            Else
                strsql &= ",'" & rvNo & "'"  'rv #
            End If
            strsql &= ",'" & ChkNo & "'"
            If Bank = 0 Then
                strsql &= ",Null"
            Else
                strsql &= ",'" & Bank & "'"
            End If
            If ValueDate = "" Then
                strsql &= ",Null"
            Else
                strsql &= ",'" & CType(ValueDate, Date).ToString("MM/dd/yyyy") & "'"
            End If
            If NRID = 0 Then
                strsql &= ",null"
            Else
                strsql &= ",'" & NRID & "'"
            End If
            strsql &= ",'" & NRRef & "'"
            If DueDate = "" Then
                strsql &= ",null"
            Else
                strsql &= ",'" & CType(DueDate, Date).ToString("MM/dd/yyyy") & "'"
            End If
            If SignDate = "" Then
                strsql &= ",null"
            Else
                strsql &= " , '" & CType(SignDate, Date).ToString("MM/dd/yyyy") & "'"
            End If
            strsql &= " , '" & GuarName & "'"
            strsql &= " , '" & AuthoStr & "'"
            If ContHeadid = 0 Then
                strsql &= ",null"
            Else
                strsql &= " , '" & ContHeadid & "'"
            End If
            If BonAchatSer = 0 Then
                strsql &= ",null"
            Else
                strsql &= " , '" & BonAchatSer & "'"
            End If
            strsql &= " ,getdate()"
            strsql &= " , '" & gbUser & "' , NULL )"

            If cmSqlTrans Is Nothing Then
                cnSql = New SqlConnection(ConnectionString)
                cnSql.Open()
                cmSql = New SqlCommand(strsql, cnSql)
                cmSql.ExecuteNonQuery()
            Else
                cmSqlTrans.CommandText = strsql
                cmSqlTrans.ExecuteNonQuery()
            End If

        Catch ex As Exception
            Throw ex
        Finally
            cmSql = Nothing

            If Not cnSql Is Nothing Then
                If cnSql.State = ConnectionState.Open Then cnSql.Close()
                cnSql = Nothing
            End If
        End Try
    End Sub

#End Region
    Public Function GetItemPrive(ByVal _itemRecID As String, ByVal _PendingItem As Boolean) As Boolean
        Dim Strsql, Str_StkOrSfp As String
        Try
            If _PendingItem = True Then
                Str_StkOrSfp = "SFP"
            Else
                Str_StkOrSfp = "STK"
            End If

            Strsql = " SELECT TOP 1"
            Strsql &= " 1"
            Strsql &= " FROM    ( SELECT    " & Str_StkOrSfp & "_NEWUSED STK_NEWUSED , " & Str_StkOrSfp & "_SUPPLIER  STK_SUPPLIER , " & Str_StkOrSfp & "_Brand STK_Brand"
            If _PendingItem = True Then
                Strsql &= " FROM      STK04_StkFilPending"
            Else
                Strsql &= " FROM      STK04_STKFIL"
            End If
            Strsql &= " WHERE     " & Str_StkOrSfp & "_RECID = " & _itemRecID & ""
            Strsql &= " ) Stock"
            Strsql &= " INNER JOIN ( SELECT ASP_STKTag"
            Strsql &= " FROM   STK04_APPSTKPRIV"
            Strsql &= " WHERE  ASP_User = '" & gbUser & "'"
            Strsql &= " AND ASP_AppCode = '" & gbApp & "'"
            Strsql &= " ) Tag ON STK_NEWUSED = ASP_STKTag"
            Strsql &= " INNER JOIN ("
            Strsql &= " SELECT  BUMP_SuppCode"
            Strsql &= " FROM    Stk04_BusUnitManagPriv"
            Strsql &= " WHERE   BUMP_Access = '1'"
            Strsql &= " AND BUMP_UserCode = '" & gbUser & "'"
            Strsql &= " ) Supp ON BUMP_SuppCode = STK_SUPPLIER"
            Strsql &= " INNER JOIN  ("
            Strsql &= " "
            Strsql &= " SELECT  BUMP_BrandCode"
            Strsql &= " FROM    Stk04_BusUnitManagPriv"
            Strsql &= " WHERE   BUMP_Access = '1'"
            Strsql &= " AND BUMP_UserCode = '" & gbUser & "'"
            Strsql &= " )  Brand ON BUMP_BrandCode = STK_Brand"
            Return RecordsExists(Strsql)


        Catch ex As Exception
            Throw ex
        End Try


    End Function
    Public Sub creatConNorView()
        Dim strsql As String
        Try

            strsql = " IF EXISTS ( SELECT  *"
            strsql &= " FROM    SYSOBJECTS"
            strsql &= " WHERE   ID = OBJECT_ID(N'V_StockNormalCons')"
            strsql &= " AND OBJECTPROPERTY(ID, N'IsView') = 1 )"
            strsql &= " SELECT  1"
            If RecordsExists(strsql) = True Then Exit Try

            strsql = " CREATE VIEW V_StockNormalCons as  "
            strsql &= "SELECT NORMAL.STK_RECID NSTK_RECID,"
            For i = 0 To CurrItmKey.Length - 1
                If UCase(CurrItmKey(i)) <> "STK_NEWUSED" Then
                    strsql &= " NORMAL." & CurrItmKey(i) & " as [N" & CurrItmKey(i) & "] , "
                End If
            Next
            strsql &= " NORMAL.STK_NEWUSED NSTK_NEWUSED, NORMAL.STK_DESC NSTK_DESC,NORMAL.STK_DESCABV NSTK_DESCABV,"
            strsql &= " Cons.STK_RECID CSTK_RECID,"
            For i = 0 To CurrItmKey.Length - 1
                If UCase(CurrItmKey(i)) <> "STK_NEWUSED" Then
                    strsql &= " Cons." & CurrItmKey(i) & " as [C" & CurrItmKey(i) & "]     ,"
                End If
            Next
            strsql &= " Cons.STK_NEWUSED CSTK_NEWUSED, Cons.STK_DESC CSTK_DESC,Cons.STK_DESCABV CSTK_DESCABV "
            strsql &= " from stk04_STKFIL NORMAL LEFT JOIN stk04_STKFIL Cons on  "
            For i = 0 To CurrItmKey.Length - 1
                If UCase(CurrItmKey(i)) <> "STK_NEWUSED" Then
                    strsql &= " " & IIf(i <> 0, "And", "").ToString & " Cons." & CurrItmKey(i) & " =  NORMAL." & CurrItmKey(i) & "    "
                End If
            Next
            strsql &= "  And Cons.[STK_NEWUSED] = 'C'"
            strsql &= " WHERE NORMAL.[STK_NEWUSED] <> 'C'  AND cons.STK_RECID IS NOT NULL AND Cons.STK_ACTIVE='Y' AND NORMAL.STK_ACTIVE='Y'   AND ISNULL(NORMAL.STK_SendToPOS, 'N') <> 'N' AND ISNULL(Cons.STK_SendToPOS, 'N') <> 'N'"
            Execute_WOXACT(strsql)
        Catch ex As Exception
            MessageBox.Show(ex.Message, "Load", MessageBoxButtons.OK, MessageBoxIcon.Error)
        End Try
    End Sub

    Public Sub TY_SendToXls(ByVal dt As DataTable, ByVal titel As String, ByRef Package As OfficeOpenXml.ExcelPackage, Optional FormatingDictionary As Dictionary(Of String, String) = Nothing)
        Try
            Dim WS As OfficeOpenXml.ExcelWorksheet
            Dim MyrichTextcoloction As OfficeOpenXml.Style.ExcelRichTextCollection
            Dim MyrichText As OfficeOpenXml.Style.ExcelRichText
            WS = Package.Workbook.Worksheets.Add(titel.Trim)
            WS.Cells(6, 1).LoadFromDataTable(dt, True)
            ' Company Name Head
            MyrichTextcoloction = WS.Cells(1, 1).RichText
            MyrichText = MyrichTextcoloction.Add("Company Name - " & gbCompName & "")
            MyrichText.Bold = True
            WS.Cells(1, 1).Style.HorizontalAlignment = OfficeOpenXml.Style.ExcelHorizontalAlignment.Left
            WS.Cells(1, 1).Style.VerticalAlignment = OfficeOpenXml.Style.ExcelVerticalAlignment.Center
            WS.Cells(1, 1).Style.Font.Size = 18
            WS.Row(1).Height = 23.25
            WS.Cells(1, 1, 1, dt.Columns.Count + 10).Merge = True
            WS.Cells(1, 1, 1, dt.Columns.Count + 10).Style.HorizontalAlignment = OfficeOpenXml.Style.ExcelHorizontalAlignment.Left
            WS.Cells(1, 1, 1, dt.Columns.Count + 10).Style.VerticalAlignment = OfficeOpenXml.Style.ExcelVerticalAlignment.Center
            ' Report Name
            MyrichTextcoloction = WS.Cells(2, 1).RichText
            MyrichText = MyrichTextcoloction.Add(titel.Trim)
            MyrichText.UnderLine = True
            WS.Cells(2, 1, 2, dt.Columns.Count).Merge = True
            WS.Cells(2, 1, 2, dt.Columns.Count).Style.HorizontalAlignment = OfficeOpenXml.Style.ExcelHorizontalAlignment.Center
            WS.Cells(2, 1, 2, dt.Columns.Count).Style.VerticalAlignment = OfficeOpenXml.Style.ExcelVerticalAlignment.Center

            'Printing Date
            MyrichTextcoloction = WS.Cells(3, 1).RichText
            MyrichText = MyrichTextcoloction.Add("Printed On :" & CDate(Now).ToString("dd/MM/yyyy") & "")
            WS.Cells(3, 1, 3, dt.Columns.Count).Merge = True
            WS.Cells(3, 1, 3, dt.Columns.Count).Style.HorizontalAlignment = OfficeOpenXml.Style.ExcelHorizontalAlignment.Left
            WS.Cells(3, 1, 3, dt.Columns.Count).Style.VerticalAlignment = OfficeOpenXml.Style.ExcelVerticalAlignment.Center


            'Printing Date
            MyrichTextcoloction = WS.Cells(4, 1).RichText
            MyrichText = MyrichTextcoloction.Add("Printed By :" & gbUser & "")
            WS.Cells(4, 1, 4, dt.Columns.Count).Merge = True
            WS.Cells(4, 1, 4, dt.Columns.Count).Style.HorizontalAlignment = OfficeOpenXml.Style.ExcelHorizontalAlignment.Left
            WS.Cells(4, 1, 4, dt.Columns.Count).Style.VerticalAlignment = OfficeOpenXml.Style.ExcelVerticalAlignment.Center

            WS.Cells(6, 1, 6, dt.Columns.Count).Style.Fill.PatternType = OfficeOpenXml.Style.ExcelFillStyle.Solid
            WS.Cells(6, 1, 6, dt.Columns.Count).Style.Fill.BackgroundColor.SetColor(Color.FromArgb(0, 88, 176))
            WS.Cells(6, 1, 6, dt.Columns.Count).Style.Font.Bold = True
            WS.Cells(6, 1, 6, dt.Columns.Count).Style.Font.Color.SetColor(Color.White)
            WS.Cells(6, 1, 6, dt.Columns.Count).Style.HorizontalAlignment = OfficeOpenXml.Style.ExcelHorizontalAlignment.Left
            WS.Cells(6, 1, WS.Dimension.Rows, dt.Columns.Count).AutoFitColumns()
            WS.Cells(6, 1, WS.Dimension.Rows, dt.Columns.Count).AutoFilter = True
            WS.Cells(6, 1, WS.Dimension.Rows, dt.Columns.Count).Style.Border.Top.Style = OfficeOpenXml.Style.ExcelBorderStyle.Thin
            WS.Cells(6, 1, WS.Dimension.Rows, dt.Columns.Count).Style.Border.Top.Color.SetColor(Color.White)
            WS.Cells(6, 1, WS.Dimension.Rows, dt.Columns.Count).Style.Border.Bottom.Style = OfficeOpenXml.Style.ExcelBorderStyle.Thin
            WS.Cells(6, 1, WS.Dimension.Rows, dt.Columns.Count).Style.Border.Bottom.Color.SetColor(Color.White)
            WS.Cells(6, 1, WS.Dimension.Rows, dt.Columns.Count).Style.Border.Right.Style = OfficeOpenXml.Style.ExcelBorderStyle.Thin
            WS.Cells(6, 1, WS.Dimension.Rows, dt.Columns.Count).Style.Border.Right.Color.SetColor(Color.White)
            WS.Cells(6, 1, WS.Dimension.Rows, dt.Columns.Count).Style.Border.Left.Style = OfficeOpenXml.Style.ExcelBorderStyle.Thin
            WS.Cells(6, 1, WS.Dimension.Rows, dt.Columns.Count).Style.Border.Left.Color.SetColor(Color.White)
            WS.Row(6).Height = 15.75
            WS.Cells(7, 1, WS.Dimension.Rows, dt.Columns.Count).Style.Fill.PatternType = OfficeOpenXml.Style.ExcelFillStyle.Solid
            WS.Cells(7, 1, WS.Dimension.Rows, dt.Columns.Count).Style.Fill.BackgroundColor.SetColor(Color.FromArgb(221, 235, 247))
            For Each Mycol As DataColumn In dt.Columns
                If Mycol.DataType.ToString = "System.DateTime" Then
                    WS.Cells(7, dt.Columns.IndexOf(Mycol.ColumnName) + 1, WS.Dimension.Rows, dt.Columns.IndexOf(Mycol.ColumnName) + 1).Style.Numberformat.Format = "dd/MM/yyyy"
                    WS.Column(dt.Columns.IndexOf(Mycol.ColumnName) + 1).Width = 11
                End If
            Next
            For Each Mycol As DataColumn In dt.Columns
                Select Case Mycol.DataType.ToString
                    Case "System.DateTime"
                        WS.Cells(7, dt.Columns.IndexOf(Mycol.ColumnName) + 1, WS.Dimension.Rows, dt.Columns.IndexOf(Mycol.ColumnName) + 1).Style.Numberformat.Format = "dd/MM/yyyy"
                        WS.Column(dt.Columns.IndexOf(Mycol.ColumnName) + 1).Width = 11
                    Case "System.Decimal", "System.Double"
                        If FormatingDictionary Is Nothing Then
                            WS.Cells(7, dt.Columns.IndexOf(Mycol.ColumnName) + 1, WS.Dimension.Rows, dt.Columns.IndexOf(Mycol.ColumnName) + 1).Style.Numberformat.Format = "###,###,###,##0.###"
                            WS.Column(dt.Columns.IndexOf(Mycol.ColumnName) + 1).Width = 11
                        Else
                            If FormatingDictionary.Item(Mycol.ColumnName) = "N3" Then
                                WS.Cells(7, dt.Columns.IndexOf(Mycol.ColumnName) + 1, WS.Dimension.Rows, dt.Columns.IndexOf(Mycol.ColumnName) + 1).Style.Numberformat.Format = "###,###,###,##0.000"
                            ElseIf FormatingDictionary.Item(Mycol.ColumnName) = "N2" Then
                                WS.Cells(7, dt.Columns.IndexOf(Mycol.ColumnName) + 1, WS.Dimension.Rows, dt.Columns.IndexOf(Mycol.ColumnName) + 1).Style.Numberformat.Format = "###,###,###,##0.00"
                            ElseIf FormatingDictionary.Item(Mycol.ColumnName) = "N0" Then
                                WS.Cells(7, dt.Columns.IndexOf(Mycol.ColumnName) + 1, WS.Dimension.Rows, dt.Columns.IndexOf(Mycol.ColumnName) + 1).Style.Numberformat.Format = "###,###,###,##0"
                            Else
                                WS.Cells(7, dt.Columns.IndexOf(Mycol.ColumnName) + 1, WS.Dimension.Rows, dt.Columns.IndexOf(Mycol.ColumnName) + 1).Style.Numberformat.Format = FormatingDictionary.Item(Mycol.ColumnName)
                            End If

                            WS.Column(dt.Columns.IndexOf(Mycol.ColumnName) + 1).Width += 11
                        End If

                End Select
            Next

            dt.Clear()
            dt.Dispose()
            GC.Collect()
            WS.PrinterSettings.PaperSize = OfficeOpenXml.ePaperSize.A4
            WS.PrinterSettings.Orientation = OfficeOpenXml.eOrientation.Landscape
        Catch ex As Exception
            Throw ex
        End Try
    End Sub


    Public Sub GetFranchiseModuleVars()
        Try

            ConnectionString = HosDLL.HOSDLLModvar.FR_ConnectionString

            gbServer = HosDLL.HOSDLLModvar.FR_gbServer
            gbDBname = HosDLL.HOSDLLModvar.FR_gbDbName
            gbSQLpswd = HosDLL.HOSDLLModvar.FR_gbSQLpswd

            gbUser = HosDLL.HOSDLLModvar.FR_gbUser
            gbUserName = HosDLL.HOSDLLModvar.FR_gbUserName
            gbApp = HosDLL.HOSDLLModvar.FR_gbApp
            gbComp = HosDLL.HOSDLLModvar.FR_gbComp
            gbCompName = HosDLL.HOSDLLModvar.FR_gbCompName
            gbYear = HosDLL.HOSDLLModvar.FR_gbYear

            gbCurr1 = HosDLL.HOSDLLModvar.FR_gbcurr1
            gbCurr2 = HosDLL.HOSDLLModvar.FR_gbcurr2

            gbTrading = HosDLL.HOSDLLModvar.FR_gbTrading
            gbFashion = HosDLL.HOSDLLModvar.FR_gbFashion
            gbExpiry = HosDLL.HOSDLLModvar.FR_gbExpiry
            gbBook = HosDLL.HOSDLLModvar.FR_gbBook
            gbExePath = HosDLL.HOSDLLModvar.FR_ExePath
            gbPCName = SystemInformation.ComputerName

            CurrItmKey = HosDLL.HOSDLLModvar.FR_CurrItmKey
            CurrItmKeyPend = HosDLL.HOSDLLModvar.FR_CurrItmKeyPend
            CurrItmKeyDesc = HosDLL.HOSDLLModvar.FR_CurrItmKeyDesc

            HosDLL.HOSDLLModvar.FR_OpenedFromFranchise = False

        Catch ex As Exception

        End Try
    End Sub
#Region " Working With Xls Add By Toufic 09/03/2018"
    Public Sub GetExcelSheetsName(ByVal ExlPath As String, FilledCombo As ComboBox)
        Dim xlPATH As String
        Dim xl As Excel.Application
        Dim sht As Excel.Worksheet
        Dim xlsSheets As Excel.Sheets
        Dim xlwbook As Excel.Workbook
        Dim xlwbooks As Excel.Workbooks
        Try
            xlPATH = ExlPath
            xl = New Excel.Application
            xlwbooks = xl.Workbooks
            xlwbook = xlwbooks.Open(xlPATH)
            xlsSheets = xlwbook.Worksheets
            With FilledCombo
                .Items.Clear()
                For i As Integer = 1 To xlsSheets.Count
                    sht = CType(xlsSheets.Item(i), Excel.Worksheet)
                    .Items.Add(CStr(sht.Name))
                    ReleaseComObject(sht)
                Next
            End With
            xlwbook.Close(False)
            xl.Quit()
        Catch ex As Exception
            MessageBox.Show("Error: " & ex.Message, "Exls", MessageBoxButtons.OK, MessageBoxIcon.Error)
        Finally
            ReleaseComObject(xlsSheets)
            ReleaseComObject(xlwbook)
            ReleaseComObject(xlwbooks)
            ReleaseComObject(xl)
        End Try

    End Sub
    Public Sub ReleaseComObject(ByVal obj As Object)
        Try
            System.Runtime.InteropServices.Marshal.ReleaseComObject(obj)
            obj = Nothing
        Catch ex As Exception
            obj = Nothing
        End Try
    End Sub
#End Region

 

    'ch @ 18/12/2018 function math.round has the default midpointrounding is TOEven and if midpointrounding is toeven the round(0.2685,3)= 0.268  will not work normally
    Public Function MathRound(ByVal val As Decimal, Optional precision As Integer = 0) As Double
        Return Math.Round(val, precision, System.MidpointRounding.AwayFromZero)
    End Function

    Public Function MathRound(ByVal val As Double, Optional ByVal precision As Integer = 0) As Double


        Return Math.Round(val, precision, System.MidpointRounding.AwayFromZero)
    End Function

    Public Sub InitStockSearch()

        Try

            StkSearchTempTable = "TempStockSearch_" & gbUser & "_" & gbApp & "_" & gbComp

            clsStockSearch.ssConnectionString = ConnectionString

            clsStockSearch.ssUser = gbUser
            clsStockSearch.ssUserName = gbUserName
            clsStockSearch.ssApp = gbApp
            clsStockSearch.ssComp = gbComp
            clsStockSearch.ssYear = gbYear
            clsStockSearch.ssCurr1 = gbCurr1
            clsStockSearch.ssCurr2 = gbCurr2
            clsStockSearch.ssTempTableName = StkSearchTempTable
            clsStockSearch.ssClocHO = True
            clsStockSearch.ssExpiry = gbExpiry
            clsStockSearch.ssFashion = gbFashion
            clsStockSearch.ssTrading = gbTrading
            clsStockSearch.ssBooks = gbBook
            clsStockSearch.ssCompName = gbCompName
            clsStockSearch.ssDecimalNumQty = gbDecNo
            clsStockSearch.ssDecimalNumAmt = VatLCRound

            clsStockSearch.PrepareStockSearch()

        Catch ex As Exception

        End Try

    End Sub

    Public Sub FillFundDisbursementModuleVarialbles()
        Dim AccLinkedServer As String = ""
        Dim lcServerLink, lcDBnameLink, lcSQLpswdLink, lcSQLuserLink, lcSQLAuth As String
        Try

            If String.IsNullOrEmpty(ACYear) Then Exit Sub

            If RecordsExists("SELECT App_Code FROM dbo.Com_Application WITH (NOLOCK) WHERE App_Code IN ('ACC01', 'MAG')") Then

                LinkServerGlobalVariables(lcServerLink, lcDBnameLink, lcSQLpswdLink, lcSQLuserLink, lcSQLAuth, gbComp)
                LinkServer(lcServerLink, lcSQLuserLink, lcSQLpswdLink, lcDBnameLink, lcSQLAuth, 0, "1", AccLinkedServer)
                gbAccountingLinkedServerForFunds = AccLinkedServer

                If AccLinkedServer.Trim = "" Then gbAccountingLinkedServerForFunds = "" : Exit Sub

                Using dtAccVars As New DataTable

                    Using daAccVars As New SqlDataAdapter($" SELECT ISNULL(SYS_ChkFundsDisbModule, 'N') SYS_ChkFundsDisbModule , 
                                                                    ISNULL(SYS_FreshDollarCurrency, '') SYS_FreshDollarCurrency
                                                             FROM   {AccLinkedServer}ACC01_SysTable WITH ( NOLOCK )
                                                             WHERE SYS_Comp = '{gbComp}'
					                                           AND SYS_Year = '{ACYear}' ",
                                                          ConnectionString)
                        daAccVars.Fill(dtAccVars)
                    End Using

                    If dtAccVars.Rows.Count = 1 Then

                        gbFundDisbursementModuleIsActive = (dtAccVars(0)("SYS_ChkFundsDisbModule").ToString.ToUpper = "Y")
                        gbFreshDollarCurrency = dtAccVars(0)("SYS_FreshDollarCurrency").ToString

                    End If

                End Using

                gbAccRateNo = ExecuteScalar_WOXACT($" SELECT App_Rate
                                                      FROM   {AccLinkedServer}Com_Application WITH ( NOLOCK )
                                                      WHERE	App_Code = 'ACC01' ")

            End If

        Catch ex As Exception
            'Module will be not activated
        End Try
    End Sub

    Public Function GetItemProductGroupRecID(_ItemRecID As String) As String
        Try

            If String.IsNullOrEmpty(_ItemRecID) Then Return ""

            Return ExecuteScalar_WOXACT($" SELECT  IT_RecID
                                           FROM    STK04_STKFIL
                                                   INNER JOIN ( SELECT IT_RecID ,
                                                                       IT_Code
                                                                FROM   STK04_ItemType
                                                              ) STK04_ItemType ON STK04_ItemType.IT_Code = STK04_STKFIL.STK_ITMTYPE
                                           WHERE   STK_RECID = '{_ItemRecID}' ")

        Catch ex As Exception
            Return ""
        End Try
    End Function

    Public Sub EnableRPC(ByVal _LinkedServer As String)
        Dim cnsql As SqlConnection
        Dim cmsql As New SqlCommand
        Try

            cnsql = New SqlConnection(ConnectionString)
            cnsql.Open()

            cmsql = New SqlCommand("", cnsql)

            cmsql.CommandText = " EXEC sp_serveroption '" & _LinkedServer & "', 'rpc', true "
            cmsql.ExecuteNonQuery()

            cmsql.CommandText = " EXEC sp_serveroption '" & _LinkedServer & "', 'rpc out', true "
            cmsql.ExecuteNonQuery()

        Catch ex As Exception
            'Throw
        Finally
            If Not cnsql Is Nothing Then
                If cnsql.State = ConnectionState.Open Then cnsql.Close()
                cnsql.Dispose()
                cnsql = Nothing
            End If
            cmsql = Nothing
        End Try
    End Sub

    Public Class FundsException
        Inherits Exception
        Public Sub New(_message As String)
            MyBase.New(_message)
        End Sub
    End Class

End Module
