Imports System.Threading
Imports System.Data.SqlClient
Imports System.Web.Script.Serialization
Imports System.Net
Imports System.Text
Imports System.Security.Cryptography
Imports System.IO
Imports PortablePOS_WS
Imports Newtonsoft.Json.Linq
Imports Newtonsoft.Json.Serialization

Module modControls

    Private LocCode As String = ConfigurationManager.AppSettings("LocCode").ToString
    Private ClocHO As Boolean = False

#Region "POS Controls"

    Public Function BeforeCashCount(_UserCode As String, _DeviceName As String, _CashRegister As String, _WorkingDay As String, _LocalCurrency As String) As String
        Try

            If CheckOpenedSession(_DeviceName) = False And ClocHO = False Then
                Return "Cashier Session Should Be Opened Before Cash Count"
            End If

            'Test if Cash count is done at this time, then invoice can't be opened
            If CheckCashCountTime(_CashRegister, _WorkingDay) = True Then
                Return "Cash Count is defined at this time. You need to wait 1 min before opening Cash Count"
            End If

            If CashierMaxCashCount(_CashRegister, _WorkingDay, _LocalCurrency) = True AndAlso ClocHO = False Then
                Return "Cash Count Already Defined No Process"
            End If

            If GetDailyCurr() = "NULL" Then
                Return "Exist not defined rate(s)"
            End If

            If CheckCashierandCashReg(_UserCode, _DeviceName) = False Then
                Return "Cashier Not Active or Not Found"
            End If

            Return ""

        Catch ex As Exception
            Return ex.Message
        End Try
    End Function

    Public Function CheckOpenedSession(_DeviceName As String) As Boolean
        Dim strsql As String
        Try
            strsql = " select * from pos_cashreg where cr_compname = '" & _DeviceName.Trim.Replace("'", "''") & "' and cr_active = 'Y'"
            strsql &= " and cr_sessact = 'Y'  AND CR_Loc='" & LocCode & "'"

            If RecordsExists(strsql) = True Then
                CheckOpenedSession = True   'Session opened
            Else
                CheckOpenedSession = False   'Session not opened
            End If
        Catch ex As Exception
            Throw ex
        End Try
    End Function

    Public Function CheckCashCountTime(_CashRegister As String, _WorkingDay As String) As Boolean
        Dim cnSql As SqlConnection
        Dim cmSql As SqlCommand
        Dim strsql As String
        Dim drSql As SqlDataReader
        Dim times As String
        Try
            cnSql = New SqlConnection(DecryptString(ServerSent.lcConSql, DecPS("REDACTED")))
            cnSql.Open()

            cmSql = New SqlCommand(strsql, cnSql)

            strsql = "SELECT MAX(RIGHT('00' + ccnt_time,5)) ccnt_Time FROM pos_cashcounted WHERE [CCNT_DATE]= CONVERT(DATETIME,'" & _WorkingDay & "',103)  "
            strsql &= " AND [CCNT_CRCODE]='" & _CashRegister & "' "

            cmSql.CommandText = strsql
            drSql = cmSql.ExecuteReader

            With drSql
                If .HasRows Then
                    .Read()
                    If .Item("ccnt_time").ToString.Trim <> "" Then
                        If CInt(.Item("ccnt_Time").Substring(0, 2).ToString) < 24 Then
                            times = .Item("ccnt_time").ToString
                        Else
                            times = IIf(Len(LTrim(Str(CInt(.Item("ccnt_Time").Substring(0, 2).ToString) - 24))) < 2, "0" + CStr(LTrim(Str(CInt(.Item("ccnt_Time").Substring(0, 2).ToString) - 24))), CStr(CInt(.Item("ccnt_Time").Substring(0, 2).ToString) - 24)).ToString & .Item("ccnt_Time").Substring(2, 3).ToString
                        End If
                    End If

                    If times = CType(Now, Date).ToString("HH:mm") Then
                        CheckCashCountTime = True
                    Else
                        CheckCashCountTime = False
                    End If
                Else
                    CheckCashCountTime = False
                End If
            End With

        Catch ex As Exception
            Throw ex
        Finally
            Disposecontrols(cnSql, cmSql, drSql)
        End Try
    End Function

    Public Function CashierMaxCashCount(_CashRegister As String, ByVal WorkingDate As String, LocalCurrency As String, Optional return0CashCount As Boolean = False) As Boolean
        Dim cnSql As SqlConnection
        Dim cmSql As SqlCommand
        Dim drSql As SqlDataReader
        Dim strsql As String
        Dim MaxCashCnt As Integer
        Dim ActualCashCount As Integer
        Dim strMaxCC As String
        Try

            'Actual Count
            'strsql = "select * from pos_cashcounted where ccnt_date = '" & CType(Now, Date).ToString("MM/dd/yyyy") & "' "
            'strsql &= " and ccnt_loc = '" & Cloc & "' and ccnt_settype = '010'"
            'strsql &= " and ccnt_curr = '" & gbCurr1 & "' and ccnt_crcode = '" & CashRegister & "'"
            'ch @ 15/11/2017 test should be based on working date 
            ' ActualCashCount = CInt(GetFieldDesc("pos_cashcounted", "ccnt_date = '" & CType(Now, Date).ToString("MM/dd/yyyy") & "' and ccnt_loc = '" & Cloc & "' and ccnt_settype = '010'  and ccnt_curr = '" & gbCurr1 & "' and ccnt_crcode = '" & CashRegister & "'", "count(*)"))
            ActualCashCount = CInt(GetFieldDesc("pos_cashcounted", "ccnt_date = '" & CType(WorkingDate, Date).ToString("MM/dd/yyyy") & "' and ccnt_loc = '" & LocCode & "' and ccnt_settype = '010'  and ccnt_curr = '" & LocalCurrency & "' and ccnt_crcode = '" & _CashRegister & "'", "count(*)"))

            'Get maximum cash count for this cash register
            'christina @ 07/08/2015

            strMaxCC = GetFieldDesc("pos_cashreg", "CR_Code = '" & _CashRegister & "' AND CR_Loc='" & LocCode & "'", "cr_NoCshCnt")
            MaxCashCnt = CInt(IIf(strMaxCC = "", 0, strMaxCC))

            'return0CashCount: if true then no control on max cash count
            If (MaxCashCnt <> 0 And ActualCashCount >= MaxCashCnt) Or (return0CashCount = False And MaxCashCnt = 0) Then
                CashierMaxCashCount = True
                Exit Function
            End If

            CashierMaxCashCount = False
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

    Public Function GetDailyCurr() As String
        Dim cnSql As SqlConnection
        Dim cmSql As SqlCommand
        Dim strsql As String
        Dim drSql As SqlDataReader
        'Check Rates
        Try

            If 1 = 1 Then Exit Function
            'If Copercurr = "FC" Then Exit Function

            'cnSql = New SqlConnection(DecryptString(ServerSent.lcConSql, DecPS("REDACTED")))
            'cnSql.Open()
            'cmSql = New SqlCommand(strsql, cnSql)

            'If ClocHO = False Then
            '    strsql = "SELECT * From POS_DailyCurrency Where DC_CURR = '" & CStr(IIf(Copercurr = gbCurr1, gbCurr2, Copercurr)) & "'"
            'Else
            '    strsql = " SELECT  ( CASE WHEN App_Rate = 1 THEN DR_Rate1LC ELSE ( CASE WHEN App_Rate = 2 THEN DR_Rate2LC ELSE DR_Rate3LC END ) END ) DC_RateLC ,"
            '    strsql &= " ( CASE WHEN App_Rate = 1 THEN DR_Rate1CC ELSE ( CASE WHEN App_Rate = 2 THEN DR_Rate2CC ELSE DR_Rate3CC END ) END ) DC_RateCC ,"
            '    strsql &= " ( CASE WHEN App_Rate = 1 THEN DR_Op1LC ELSE ( CASE WHEN App_Rate = 2 THEN DR_Op2LC ELSE DR_Op3LC END ) END ) DC_OpLC ,"
            '    strsql &= " DR_RATE1LC DC_AccRate"
            '    strsql &= " FROM dbo.Com_DailyRate INNER JOIN Com_Application ON App_Code = '" & gbApp & "'"
            '    strsql &= " WHERE   DR_Date = '" & CDate(WorkingDay).ToString("MM/dd/yyyy") & "'"
            '    strsql &= " AND DR_CurCode = '" & CStr(IIf(Copercurr = gbCurr1, gbCurr2, Copercurr)) & "'"

            'End If

            'cmSql.CommandText = strsql
            'drSql = cmSql.ExecuteReader

            'With drSql
            '    If .HasRows Then
            '        .Read()
            '        If .Item("DC_RATELC").ToString = "" Or .Item("DC_RATELC").ToString = "0" Then
            '            MessageBox.Show("Currency Rates Are Not Defined.", CntTitle, MessageBoxButtons.OK, MessageBoxIcon.Exclamation)
            '            GetDailyCurr = "NULL"
            '            Exit Function
            '        ElseIf .Item("DC_RATECC").ToString = "" Or .Item("DC_RATECC").ToString = "0" Then
            '            MessageBox.Show("Currency Rates Are Not Defined.", CntTitle, MessageBoxButtons.OK, MessageBoxIcon.Exclamation)
            '            GetDailyCurr = "NULL"
            '            Exit Function
            '        End If
            '    Else
            '        MessageBox.Show("Currency Rates Are Not Defined", CntTitle, MessageBoxButtons.OK, MessageBoxIcon.Exclamation)
            '        GetDailyCurr = "NULL"
            '    End If
            'End With

        Catch ex As Exception
            GetDailyCurr = "NULL"
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

    Public Function CheckCashierandCashReg(_UserCode As String, _DeviceName As String) As Boolean
        Dim strsql As String
        Try
            CheckCashierandCashReg = True

            strsql = "SELECT E_CODE from " & CStr(IIf(ClocHO = False, "POS_Employee", "STK04_Employee")) & " where E_CODE = '" & _UserCode & "' and E_Activ='Y'"
            If RecordsExists(strsql) = False Then
                'MessageBox.Show("Cashier Not Active or Not Found", CntTitle, MessageBoxButtons.OK, MessageBoxIcon.Exclamation)
                CheckCashierandCashReg = False
                Exit Function
            End If

            strsql = "Select CR_Code from POS_CashReg where CR_compNAME = '" & _DeviceName.Replace("'", "''") & "' AND CR_Loc='" & LocCode & "' AND CR_Active='y'"
            If RecordsExists(strsql) = False Then
                'MessageBox.Show("No Corresponding Cash Register # For This Computer", CntTitle, MessageBoxButtons.OK, MessageBoxIcon.Exclamation)
                CheckCashierandCashReg = False
                Exit Function
            End If
        Catch ex As Exception
            Throw ex
        End Try
    End Function

#End Region

End Module
