Imports System.Web.Services
Imports System.Web.Services.Protocols
Imports System.ComponentModel
Imports System.Data.SqlClient
Imports System.IO


' To allow this Web Service to be called from script, using ASP.NET AJAX, uncomment the following line.
' <System.Web.Script.Services.ScriptService()> _
<System.Web.Services.WebService(Namespace:="http://tempuri.org/")> _
<System.Web.Services.WebServiceBinding(ConformsTo:=WsiProfiles.BasicProfile1_1)> _
<ToolboxItem(False)> _
Public Class Service1
    Inherits System.Web.Services.WebService


    <WebMethod()> _
    Public Function HelloWorld() As String
       Return "Hello World"
    End Function


    Private myConnectionString As String = ConfigurationManager.ConnectionStrings("DBConStr").ConnectionString

#Region "Old ERP Data Collector"


    <WebMethod()> _
    Public Function PullHeader(ByVal recid As String, ByVal UserName As String) As Headertems
        Dim HeaderItems As Headertems = New Headertems

        Dim conn As New SqlConnection(myConnectionString)

        Dim SP_DecNo As String = ""
        Dim SP_Fashion As String = ""
        Dim SP_Expiry As String = ""

        Try
            GetSysParam(SP_DecNo, SP_Fashion, SP_Expiry)
            conn.Open()
            Dim sql As String = ""


            sql = "select case Pch_whouse when '1' then 'wh1' else 'wh2' end Warehouse,"
            sql &= " convert(varchar(10),Pch_DateCount,103) Pch_DateCount,"
            sql &= " isnull(Pch_MachUser1,'') Pch_MachUser1,isnull(Pch_MachUser2,'') Pch_MachUser2,"
            sql &= " isnull(Pch_MachUser3,'') Pch_MachUser3,isnull(Pch_MachUser4,'') Pch_MachUser4,"
            sql &= " Pch_Status,Pch_Updated,Pch_NbrItems from tbl_PhysicalCountHeader where Pch_RecId="
            sql &= recid
            sql &= " and Pch_Status <> 'C' and Pch_Updated <> 'Y' and ("
            sql &= "Pch_MachUser1 = '" & UserName & "' OR "
            sql &= "Pch_MachUser2 = '" & UserName & "' OR "
            sql &= "Pch_MachUser3 = '" & UserName & "' OR "
            sql &= "Pch_MachUser4 = '" & UserName & "')"

            Dim cmd As New SqlCommand(sql, conn)
            cmd.CommandType = CommandType.Text

            Dim dr As SqlDataReader = cmd.ExecuteReader() ' Visual Basic



            While dr.Read
                If HeaderItems.mitemheader Is Nothing Then
                    ReDim HeaderItems.mitemheader(0)
                Else
                    ReDim Preserve HeaderItems.mitemheader(HeaderItems.mitemheader.Length)
                End If
                HeaderItems.mitemheader(HeaderItems.mitemheader.Length - 1) = New itemheader

                With HeaderItems.mitemheader(HeaderItems.mitemheader.Length - 1)

                    .SP_DecNo = SP_DecNo
                    .SP_Fashion = SP_Fashion
                    .SP_Expiry = SP_Expiry
                    .Warehouse = IIf(dr.Item("Warehouse") Is DBNull.Value, " ", dr.Item("Warehouse").ToString.Replace("'", ""))
                    .Pch_DateCount = IIf(dr.Item("Pch_DateCount") Is DBNull.Value, " ", dr.Item("Pch_DateCount").ToString.Replace("'", ""))
                    .Pch_MachUser1 = IIf(dr.Item("Pch_MachUser1") Is DBNull.Value, " ", dr.Item("Pch_MachUser1").ToString.Replace("'", ""))
                    .Pch_MachUser2 = IIf(dr.Item("Pch_MachUser2") Is DBNull.Value, " ", dr.Item("Pch_MachUser2").ToString.Replace("'", ""))
                    .Pch_MachUser3 = IIf(dr.Item("Pch_MachUser3") Is DBNull.Value, " ", dr.Item("Pch_MachUser3").ToString.Replace("'", ""))
                    .Pch_MachUser4 = IIf(dr.Item("Pch_MachUser4") Is DBNull.Value, " ", dr.Item("Pch_MachUser4").ToString.Replace("'", ""))
                    .Pch_Status = IIf(dr.Item("Pch_Status") Is DBNull.Value, " ", dr.Item("Pch_Status").ToString.Replace("'", ""))
                    .Pch_Updated = IIf(dr.Item("Pch_Updated") Is DBNull.Value, " ", dr.Item("Pch_Updated").ToString.Replace("'", ""))
                    .Pch_NbrItems = IIf(dr.Item("Pch_NbrItems") Is DBNull.Value, " ", dr.Item("Pch_NbrItems").ToString.Replace("'", ""))

                    .UserToUpdate = ""
                    If .Pch_MachUser1.Trim.ToUpper = UserName.Trim.ToUpper Then
                        .UserToUpdate = "1"
                    End If
                    If .Pch_MachUser2.Trim.ToUpper = UserName.Trim.ToUpper Then
                        .UserToUpdate = "2"
                    End If
                    If .Pch_MachUser3.Trim.ToUpper = UserName.Trim.ToUpper Then
                        .UserToUpdate = "3"
                    End If
                    If .Pch_MachUser4.Trim.ToUpper = UserName.Trim.ToUpper Then
                        .UserToUpdate = "4"
                    End If
                End With

            End While

            conn.Close()
            dr.Dispose()
            cmd.Dispose()
            conn.Dispose()

        Catch ex As Exception

        End Try
        Return HeaderItems

    End Function

    Public Class Headertems
        Public mitemheader() As itemheader
    End Class


    Public Class itemheader
        Public Warehouse, Pch_DateCount, Pch_MachUser1, Pch_MachUser2, Pch_MachUser3, Pch_MachUser4, Pch_Status, Pch_Updated, Pch_NbrItems, UserToUpdate, SP_DecNo, SP_Fashion, SP_Expiry As String
    End Class

    <WebMethod()> _
    Public Function PullDetails(ByVal recid As String, ByVal usertocount As String) As DetailItems
        Dim sql As String = ""
        Dim detItems As DetailItems = New DetailItems

        Dim conn As New SqlConnection(myConnectionString)

        Dim locType As String = ""
        Try
            locType = GetlocType(recid)

            conn.Open()

            sql = "select Pcd_RecId,Pcd_Loc,Pcd_stkId,STK_ref as [Refernce],"
            sql &= " stk_Desc,Pcd_Grid, Pcd_DetGrid, Pcd_Color,Color_Desc ,  "
            sql &= " CASE WHEN SUBSTRING(Pcd_ExpDate, 0, 3) = '00' THEN SUBSTRING(Pcd_ExpDate, 4, 8)  ELSE Pcd_ExpDate "
            sql &= " END AS [exp] ,"
            sql &= " Pcd_Batch, Pcd_DateCount, Pcd_Qty, Pcd_QtyCounted1, Pcd_Type1, Pcd_QtyCounted2,"
            sql &= " Pcd_Type2, Pcd_QtyCounted3, Pcd_Type3, Pcd_QtyCounted4, Pcd_Type4"
            '--,'N','N','N','N',
            '--case when Pcd_QtyCounted1 is null then 'Y' else 'N' end,
            '--case when Pcd_QtyCounted2 is null then 'Y' else 'N' end,
            '--case when Pcd_QtyCounted3 is null then 'Y' else 'N' end,
            '--case when Pcd_QtyCounted4 is null then 'Y' else 'N' end,
            '--isnull(Pcd_QtyCounted1,0) + isnull(Pcd_QtyCounted2,0)+
            '-- isnull(Pcd_QtyCounted3,0) + isnull(Pcd_QtyCounted4,0) 
            sql &= " from tbl_PhysicalCountDetail Inner join tbl_PhysicalCountHeader "
            sql &= " on Pcd_HRecId=Pch_RecId and Pcd_loc=Pch_Loc"

            If locType.Trim.ToUpper = "S" Then
                sql &= " inner join pos_stkfil on Pcd_stkid=stk_recid"
            Else
                sql &= " inner join STK04_stkfil on Pcd_stkid=stk_recid"
            End If


            sql &= " inner join stk04_itemtype on stk_itmtype = IT_Code"
            sql &= " inner join stk04_supplier on stk_supplier = Sup_Code"
            sql &= " left JOIN dbo.Stk04_COLOR ON Color_Code = Pcd_Color"
            sql &= " LEFT JOIN dbo.Stk04_Brand ON br_Code = STK_Brand "
            sql &= " where Pch_RecId = " & recid
            sql &= " Order By STK_ref  "

            Dim cmd As New SqlCommand(sql, conn)
            cmd.CommandType = CommandType.Text

            Dim dr As SqlDataReader = cmd.ExecuteReader() ' Visual Basic



            While dr.Read
                If detItems.mitemdetail Is Nothing Then
                    ReDim detItems.mitemdetail(0)
                Else
                    ReDim Preserve detItems.mitemdetail(detItems.mitemdetail.Length)
                End If
                detItems.mitemdetail(detItems.mitemdetail.Length - 1) = New itemDetail

                With detItems.mitemdetail(detItems.mitemdetail.Length - 1)
                    .Pcd_RecId = IIf(dr.Item("Pcd_RecId") Is DBNull.Value, " ", dr.Item("Pcd_RecId").ToString.Replace("'", ""))

                    .Pcd_Loc = IIf(dr.Item("Pcd_Loc") Is DBNull.Value, " ", dr.Item("Pcd_Loc").ToString.Replace("'", ""))
                    .Pcd_stkId = IIf(dr.Item("Pcd_stkId") Is DBNull.Value, " ", dr.Item("Pcd_stkId").ToString.Replace("'", ""))
                    .Refernce = IIf(dr.Item("Refernce") Is DBNull.Value, " ", dr.Item("Refernce").ToString.Replace("'", ""))
                    .stk_Desc = IIf(dr.Item("stk_Desc") Is DBNull.Value, " ", dr.Item("stk_Desc").ToString.Replace("'", ""))
                    .Pcd_Grid = IIf(dr.Item("Pcd_Grid") Is DBNull.Value, " ", dr.Item("Pcd_Grid").ToString.Replace("'", ""))
                    .Pcd_DetGrid = IIf(dr.Item("Pcd_DetGrid") Is DBNull.Value, " ", dr.Item("Pcd_DetGrid").ToString.Replace("'", ""))
                    .Pcd_Color = IIf(dr.Item("Pcd_Color") Is DBNull.Value, " ", dr.Item("Pcd_Color").ToString.Replace("'", ""))
                    .Color_Desc = IIf(dr.Item("Color_Desc") Is DBNull.Value, " ", dr.Item("Color_Desc").ToString.Replace("'", ""))
                    .exp = IIf(dr.Item("exp") Is DBNull.Value, " ", dr.Item("exp").ToString.Replace("'", ""))
                    .Pcd_Batch = IIf(dr.Item("Pcd_Batch") Is DBNull.Value, " ", dr.Item("Pcd_Batch").ToString.Replace("'", ""))
                    .Pcd_DateCount = IIf(dr.Item("Pcd_DateCount") Is DBNull.Value, " ", dr.Item("Pcd_DateCount").ToString.Replace("'", ""))
                    .Pcd_Qty = IIf(dr.Item("Pcd_Qty") Is DBNull.Value, " ", dr.Item("Pcd_Qty").ToString.Replace("'", ""))
                    .Pcd_QtyCounted1 = IIf(dr.Item("Pcd_QtyCounted1") Is DBNull.Value, " ", dr.Item("Pcd_QtyCounted1").ToString.Replace("'", ""))
                    .Pcd_Type1 = IIf(dr.Item("Pcd_Type1") Is DBNull.Value, " ", dr.Item("Pcd_Type1").ToString.Replace("'", ""))
                    .Pcd_QtyCounted2 = IIf(dr.Item("Pcd_QtyCounted2") Is DBNull.Value, " ", dr.Item("Pcd_QtyCounted2").ToString.Replace("'", ""))
                    .Pcd_Type2 = IIf(dr.Item("Pcd_Type2") Is DBNull.Value, " ", dr.Item("Pcd_Type2").ToString.Replace("'", ""))
                    .Pcd_QtyCounted3 = IIf(dr.Item("Pcd_QtyCounted3") Is DBNull.Value, " ", dr.Item("Pcd_QtyCounted3").ToString.Replace("'", ""))
                    .Pcd_Type3 = IIf(dr.Item("Pcd_Type3") Is DBNull.Value, " ", dr.Item("Pcd_Type3").ToString.Replace("'", ""))
                    .Pcd_QtyCounted4 = IIf(dr.Item("Pcd_QtyCounted4") Is DBNull.Value, " ", dr.Item("Pcd_QtyCounted4").ToString.Replace("'", ""))
                    .Pcd_Type4 = IIf(dr.Item("Pcd_Type4") Is DBNull.Value, " ", dr.Item("Pcd_Type4").ToString.Replace("'", ""))

                    Select Case usertocount
                        Case "1"
                            .usercounted = CInt(Val(.Pcd_QtyCounted1))
                        Case "2"
                            .usercounted = CInt(Val(.Pcd_QtyCounted2))
                        Case "3"
                            .usercounted = CInt(Val(.Pcd_QtyCounted3))
                        Case "4"
                            .usercounted = CInt(Val(.Pcd_QtyCounted4))
                    End Select
                End With
            End While



            conn.Close()
            dr.Dispose()
            cmd.Dispose()
            conn.Dispose()
        Catch ex As Exception

        End Try
        Return detItems
    End Function

    Public Function GetSysParam(ByRef SP_DecNo As String, ByRef SP_Fashion As String, ByRef SP_Expiry As String) As String
        Dim sql As String = ""
        Dim conn As New SqlConnection(myConnectionString)
        Dim retResult As String = " "
        Try
            conn.Open()

            sql = "select * from stk04_SysParam"

            Dim cmd As New SqlCommand(sql, conn)
            cmd.CommandType = CommandType.Text

            Dim dr As SqlDataReader = cmd.ExecuteReader() ' Visual Basic



            While dr.Read
                SP_DecNo = IIf(dr.Item("SP_DecNo") Is DBNull.Value, "0", dr.Item("SP_DecNo").ToString.Replace("'", ""))
                SP_Fashion = IIf(dr.Item("SP_Fashion") Is DBNull.Value, 0, dr.Item("SP_Fashion").ToString.Replace("'", ""))
                SP_Expiry = IIf(dr.Item("SP_Expiry") Is DBNull.Value, 0, dr.Item("SP_Expiry").ToString.Replace("'", ""))
            End While

            conn.Close()
            dr.Dispose()
            cmd.Dispose()
            conn.Dispose()
        Catch ex As Exception

        End Try
        Return retResult
    End Function

    Public Function GetlocType(ByVal recid As String) As String
        Dim sql As String = ""
        Dim conn As New SqlConnection(myConnectionString)
        Dim retResult As String = " "
        Try
            conn.Open()

            sql = "select * from STK04_Location where Loc_Code = "
            sql &= " (select Pch_loc from tbl_PhysicalCountHeader where Pch_Recid  = " & recid & ") "

            Dim cmd As New SqlCommand(sql, conn)
            cmd.CommandType = CommandType.Text

            Dim dr As SqlDataReader = cmd.ExecuteReader() ' Visual Basic



            While dr.Read
                retResult = IIf(dr.Item("loc_Type3") Is DBNull.Value, " ", dr.Item("loc_Type3").ToString.Replace("'", ""))
            End While

            conn.Close()
            dr.Dispose()
            cmd.Dispose()
            conn.Dispose()
        Catch ex As Exception

        End Try
        Return retResult
    End Function

    Public Class DetailItems
        Public mitemdetail() As itemDetail
    End Class

    Public Class itemDetail
        Public Pcd_RecId, Pcd_Loc, Pcd_stkId, Refernce, stk_Desc, Pcd_Grid, Pcd_DetGrid, Pcd_Color, Color_Desc, exp, Pcd_Batch, Pcd_DateCount, Pcd_Qty, Pcd_QtyCounted1, Pcd_Type1, Pcd_QtyCounted2, Pcd_Type2, Pcd_QtyCounted3, Pcd_Type3, Pcd_QtyCounted4, Pcd_Type4, usercounted As String
    End Class

    <WebMethod()> _
    Public Function PullPhyBarcodes(ByVal recid As String) As Barcodes
        Dim sql As String = ""
        Dim barItems As Barcodes = New Barcodes

        Dim conn As New SqlConnection(myConnectionString)
        Try
            conn.Open()

            sql = "SELECT PcBarc_RecId,PcBarc_DRecId,PcBarc_Barcode FROM tbl_PhysicalCountDetBarcode"
            sql &= " where PcBarc_DRecId in (select pcd_recid from tbl_PhysicalCountDetail where Pcd_HRecId = " & recid & ") "
            Dim cmd As New SqlCommand(sql, conn)
            cmd.CommandType = CommandType.Text

            Dim dr As SqlDataReader = cmd.ExecuteReader() ' Visual Basic



            While dr.Read
                If barItems.mitemBarcode Is Nothing Then
                    ReDim barItems.mitemBarcode(0)
                Else
                    ReDim Preserve barItems.mitemBarcode(barItems.mitemBarcode.Length)
                End If
                barItems.mitemBarcode(barItems.mitemBarcode.Length - 1) = New itemBarcode

                With barItems.mitemBarcode(barItems.mitemBarcode.Length - 1)
                    .PcBarc_RecId = IIf(dr.Item("PcBarc_RecId") Is DBNull.Value, " ", dr.Item("PcBarc_RecId").ToString.Replace("'", ""))
                    .PcBarc_DRecId = IIf(dr.Item("PcBarc_DRecId") Is DBNull.Value, " ", dr.Item("PcBarc_DRecId").ToString.Replace("'", ""))
                    .PcBarc_Barcode = IIf(dr.Item("PcBarc_Barcode") Is DBNull.Value, " ", dr.Item("PcBarc_Barcode").ToString.Replace("'", ""))
                End With
            End While

            conn.Close()
            dr.Dispose()
            cmd.Dispose()
            conn.Dispose()
        Catch ex As Exception

        End Try
        Return barItems
    End Function

    Public Class Barcodes
        Public mitemBarcode() As itemBarcode
    End Class

    Public Class itemBarcode
        Public PcBarc_RecId, PcBarc_DRecId, PcBarc_Barcode As String
    End Class

    <WebMethod()>
    Public Function InsertDetail(ByVal transCount As String, ByVal SelectedLocation As String, ByVal uInvID As String, ByVal id As String, ByVal Pcd_RecId As String, ByVal usercounted As String, ByVal uUserToUpdate As String) As String
        Dim str As String = "failed"
        Dim UpdateSQL As String = ""
        Dim shouldUpdate As Boolean = True
        Try
            If transCount = "1" Then

                Dim Sql As String = "select Pch_Status,Pch_Updated"
                Sql &= "  from tbl_PhysicalCountHeader "
                Sql &= " where Pch_RecId = " & uInvID
                Sql &= " and Pch_Loc = '" & SelectedLocation & "'"

                Dim tempTable As New DataTable
                tempTable = GetData(Sql)
                If tempTable.Rows.Count > 0 Then
                    Dim Pch_Status As String = IIf(tempTable.Rows(0).Item("Pch_Status") Is DBNull.Value, " ", tempTable.Rows(0).Item("Pch_Status").ToString.Replace("'", ""))
                    Dim Pch_Updated As String = IIf(tempTable.Rows(0).Item("Pch_Updated") Is DBNull.Value, " ", tempTable.Rows(0).Item("Pch_Updated").ToString.Replace("'", ""))
                    If Pch_Status.Trim = "C" Then
                        shouldUpdate = False
                        str = "This Physical Count Was Cancelled"
                    End If
                    If Pch_Updated.Trim.ToUpper = "Y" Then
                        shouldUpdate = False
                        str = "This Physical Count Was Already Updated To Stock"
                    End If
                Else
                    shouldUpdate = False
                    str = "This Physical Count Does Not Exist"
                End If
            End If

            If shouldUpdate Then
                UpdateSQL = " Update tbl_PhysicalCountDetail "
                UpdateSQL &= " set Pcd_QtyCounted" & uUserToUpdate & " = " & usercounted
                UpdateSQL &= " , Pcd_Type" & uUserToUpdate & " = 10, Pcd_DateCount=getdate()"
                UpdateSQL &= " where Pcd_RecId='" & Pcd_RecId & "'"
                UpdateSQL &= " and Pcd_loc = '" & SelectedLocation & "'"

                ExecuteQuery(UpdateSQL)

                str = "ok"
            End If

        Catch ex As Exception
            str = UpdateSQL & ":" & ex.Message

        End Try
        Return str

    End Function

#End Region


#Region "New Methods"

#Region "New ERP DataCollector"

#Region "Download To Machine"

    Public Function InitERPPhysicalCountTables(ByVal ApplicationName As String, ByVal SelectedRecId As String, ByVal SelectedLocation As String, ByVal MachineName As String) As TableInfos
        Dim retval As String = "ok"
        Dim sql As String = ""
        Dim TInfos As TableInfos = New TableInfos
        Dim SP_DecNo As String = ""
        Dim SP_Fashion As String = ""
        Dim SP_Expiry As String = ""

        Dim locType As String = ""

        Dim ShowAvlbQty As Boolean = True

        Try
            locType = GetlocType(SelectedRecId)
            GetSysParam(SP_DecNo, SP_Fashion, SP_Expiry)
            ClearErpDataCollectorTables(ApplicationName, SelectedLocation, MachineName)


            sql = "SELECT DISTINCT ( Priv_UserCode ) FROM dbo.Com_UserPriv"
            sql &= " WHERE  Priv_AppCode = 'COUNT' AND Priv_StepCode = 'mnuOperCountManModify'"
            sql &= " AND Priv_UserCode NOT IN ("
            sql &= " SELECT  Nbu_User FROM dbo.Com_NegationButton"
            sql &= " WHERE Nbu_Appl = 'COUNT'"
            sql &= " AND Nbu_Module = 'COUNT'"
            sql &= " AND Nbu_Step = 'mnuOperCountManModify'"
            sql &= " AND Nbu_Button = 'btnShowAvailableQuantity'"
            sql &= " AND Nbu_Exception = 1 )"

            Dim checkPrivTable As New DataTable
            checkPrivTable = GetData(sql)
            If checkPrivTable.Rows.Count > 0 Then
                ShowAvlbQty = False
            Else
                ShowAvlbQty = True
            End If



            Dim tempTableName As String = "_" & ApplicationName & "_tbl_PhysicalCountHeader_"
            tempTableName &= SelectedLocation & MachineName

            'update order by
            sql = "SELECT ROW_NUMBER() OVER( ORDER BY [Pch_Recid] ASC ) AS RowNumber ,"
            'initial query

            sql &= "Pch_Recid ,"
            sql &= " case Pch_whouse when '1' then 'wh1' else 'wh2' end Warehouse,"
            sql &= " convert(varchar(10),Pch_DateCount,103) Pch_DateCount,"
            sql &= " isnull(Pch_MachUser1,'') Pch_MachUser1,isnull(Pch_MachUser2,'') Pch_MachUser2,"
            sql &= " isnull(Pch_MachUser3,'') Pch_MachUser3,isnull(Pch_MachUser4,'') Pch_MachUser4,"
            sql &= " isnull(Pch_MachUser5,'') Pch_MachUser5,isnull(Pch_MachUser6,'') Pch_MachUser6,"
            sql &= " Pch_Status,Pch_Updated,Pch_NbrItems,"
            sql &= " case Pch_MachUser1 when '" & MachineName & "' then '1' else "
            sql &= " case Pch_MachUser2 when '" & MachineName & "' then '2' else  "
            sql &= " case Pch_MachUser3 when '" & MachineName & "' then '3' else"
            sql &= " case Pch_MachUser4 when '" & MachineName & "' then '4' else"
            sql &= " case Pch_MachUser5 when '" & MachineName & "' then '5' else"
            sql &= " case Pch_MachUser6 when '" & MachineName & "' then '6' else '0'"
            sql &= " end end end end end end UserToUpdate"
            sql &= ",'" & SP_DecNo & "' as SP_DecNo"
            sql &= ",'" & SP_Fashion & "' as SP_Fashion"
            sql &= ",'" & SP_Expiry & "' as SP_Expiry"

            sql &= ", '0' as updated"
            sql &= ", '" & SelectedRecId & "' as InvID"
            'into 
            sql &= " Into " & tempTableName & "xyz"
            sql &= "  from tbl_PhysicalCountHeader where Pch_Updated = 'N' and Pch_RecId="
            sql &= SelectedRecId
            sql &= " and Pch_Loc = '" & SelectedLocation & "'"
            sql &= " and Pch_Status <> 'C' and Pch_Updated <> 'Y' and ("
            sql &= "Pch_MachUser1 = '" & MachineName & "' OR "
            sql &= "Pch_MachUser2 = '" & MachineName & "' OR "
            sql &= "Pch_MachUser3 = '" & MachineName & "' OR "
            sql &= "Pch_MachUser4 = '" & MachineName & "' OR "
            sql &= "Pch_MachUser5 = '" & MachineName & "' OR "
            sql &= "Pch_MachUser6 = '" & MachineName & "')"


            ExecuteQuery(sql)

            CommonInitTablesProc(TInfos, tempTableName, "phyHeader", "Synchronizing Physical Count Header", SelectedLocation, MachineName)
            ''''''''''''''''''''''''

            ''''''''''''''''
            Dim usertocount As String = "0"
            Dim tempTable As New DataTable
            sql = "select UserToUpdate from " & tempTableName & "xyz"

            tempTable = GetData(sql)
            If tempTable.Rows.Count > 0 Then
                usertocount = IIf(tempTable.Rows(0).Item("UserToUpdate") Is DBNull.Value, " ", tempTable.Rows(0).Item("UserToUpdate").ToString.Replace("'", ""))


                tempTableName = "_" & ApplicationName & "_tbl_PhysicalCountDetail_"
                tempTableName &= SelectedLocation & MachineName

                sql = "SELECT ROW_NUMBER() OVER( ORDER BY [Pcd_RecId] ASC ) AS RowNumber ,"
                sql &= " Pcd_RecId,Pcd_Loc,Pcd_stkId,STK_ref as [Refernce],"
                sql &= " stk_Desc,Pcd_Grid, Pcd_DetGrid, Pcd_Color,Color_Desc ,  "
                sql &= " CASE WHEN SUBSTRING(Pcd_ExpDate, 0, 3) = '00' THEN SUBSTRING(Pcd_ExpDate, 4, 8)  ELSE Pcd_ExpDate "
                sql &= " END AS [exp] ,"
                sql &= " Pcd_Batch, Pcd_DateCount"

                If ShowAvlbQty = True Then
                    sql &= " , Pcd_Qty"
                Else
                    sql &= " , 0 as Pcd_Qty"
                End If
                sql &= " , Pcd_QtyCounted1, Pcd_Type1, Pcd_QtyCounted2,"

                sql &= " Pcd_Type2, Pcd_QtyCounted3, Pcd_Type3, Pcd_QtyCounted4, Pcd_Type4, Pcd_QtyCounted5, Pcd_Type5, Pcd_QtyCounted6, Pcd_Type6"

                Select Case usertocount
                    Case "1"
                        sql &= ", case When [Pcd_QtyCounted1] is null then 0 else [Pcd_QtyCounted1] end  as usercounted"
                    Case "2"
                        sql &= ", case When [Pcd_QtyCounted2] is null then 0 else [Pcd_QtyCounted2] end  as usercounted"
                    Case "3"
                        sql &= ", case When [Pcd_QtyCounted3] is null then 0 else [Pcd_QtyCounted3] end  as usercounted"
                    Case "4"
                        sql &= ", case When [Pcd_QtyCounted4] is null then 0 else [Pcd_QtyCounted4] end  as usercounted"

                    Case "5"
                        sql &= ", case When [Pcd_QtyCounted5] is null then 0 else [Pcd_QtyCounted5] end  as usercounted"
                    Case "6"
                        sql &= ", case When [Pcd_QtyCounted6] is null then 0 else [Pcd_QtyCounted6] end  as usercounted"

                    Case "0"
                        sql &= ", 0  as usercounted"
                End Select
                sql &= ", '0' as updated"

                'into 
                sql &= " Into " & tempTableName & "xyz"

                sql &= " from tbl_PhysicalCountDetail Inner join tbl_PhysicalCountHeader "
                sql &= " on Pcd_HRecId=Pch_RecId and Pcd_loc=Pch_Loc"

                If locType.Trim.ToUpper = "S" Then
                    sql &= " inner join pos_stkfil on Pcd_stkid=stk_recid"
                Else
                    sql &= " inner join STK04_stkfil on Pcd_stkid=stk_recid"
                End If


                sql &= " inner join stk04_itemtype on stk_itmtype = IT_Code"
                sql &= " inner join stk04_supplier on stk_supplier = Sup_Code"
                sql &= " left JOIN dbo.Stk04_COLOR ON Color_Code = Pcd_Color"
                sql &= " LEFT JOIN dbo.Stk04_Brand ON br_Code = STK_Brand "
                sql &= " where Pch_RecId = " & SelectedRecId
                sql &= " and Pcd_loc = '" & SelectedLocation & "'"
                'sql &= " Order By STK_ref  "


                ExecuteQuery(sql)

                CommonInitTablesProc(TInfos, tempTableName, "phyDetails", "Synchronizing Details", SelectedLocation, MachineName)
                ''''''''''''''''''''''''

                tempTableName = "_" & ApplicationName & "_tbl_PhysicalCountDetBarcode_"
                tempTableName &= SelectedLocation & MachineName

                sql = "SELECT  ROW_NUMBER() OVER( ORDER BY [PcBarc_RecId] ASC ) AS RowNumber ,"
                sql &= "PcBarc_RecId,PcBarc_DRecId,PcBarc_Barcode"
                sql &= " ,'0' as updated"
                'into 
                sql &= " Into " & tempTableName & "xyz"
                sql &= " FROM tbl_PhysicalCountDetBarcode"
                sql &= " where PcBarc_DRecId in (select pcd_recid from tbl_PhysicalCountDetail where Pcd_HRecId = " & SelectedRecId & " AND pcd_loc='" & SelectedLocation & "') "
                'msm Jad request 25/11/2019
                sql &= " and PcBarc_LOC='" & SelectedLocation & "'"

                ExecuteQuery(sql)

                CommonInitTablesProc(TInfos, tempTableName, "phyBarcodes", "Synchronizing Barcodes", SelectedLocation, MachineName)
                ''''''''''''''''''''''''

            Else

            End If
            '''''''''''''''''''''''''

            


        Catch ex As Exception

        End Try
        Return TInfos
    End Function

#End Region

#Region "Clear Tables"

    Public Function ClearErpDataCollectorTables(ByVal ApplicationName As String, ByVal SelectedLocation As String, ByVal MachineName As String) As String
        Dim retval As String = "ok"
        Try
            Dim tempTableName As String = "_" & ApplicationName & "_tbl_PhysicalCountHeader_"
            tempTableName &= SelectedLocation & MachineName '& SelectedRecId
            DropTempTable(tempTableName)


            tempTableName = "_" & ApplicationName & "_tbl_PhysicalCountDetail_"
            tempTableName &= SelectedLocation & MachineName
            DropTempTable(tempTableName)

            tempTableName = "_" & ApplicationName & "_tbl_PhysicalCountDetBarcode_"
            tempTableName &= SelectedLocation & MachineName
            DropTempTable(tempTableName)

        Catch ex As Exception
            retval = ex.Message
        End Try
        Return retval
    End Function

#End Region

#End Region



#Region "New ERP Scan Barcodes"

#Region "Download To Machine"

    Public Function InitERPScanTables(ByVal ApplicationName As String, ByVal SelectedLocation As String, ByVal MachineName As String) As TableInfos
        Dim retval As String = "ok"
        Dim sql As String = ""
        Dim TInfos As TableInfos = New TableInfos
        Dim SP_DecNo As String = ""
        Dim SP_Fashion As String = ""
        Dim SP_Expiry As String = ""

        Dim locType As String = ""
        Try
            locType = GetlocationType(SelectedLocation)
            GetSysParam(SP_DecNo, SP_Fashion, SP_Expiry)
            ClearERPScanTables(ApplicationName, SelectedLocation, MachineName)

            Dim tempTableName As String = "_" & ApplicationName & "_stk04_SysParam_"
            tempTableName &= SelectedLocation & MachineName

            'update order by
            sql = "SELECT ROW_NUMBER() OVER( ORDER BY [SP_DecNo] ASC ) AS RowNumber ,"
            'initial query
            sql &= "* "
            'into 
            sql &= " Into " & tempTableName & "xyz"
            sql &= "  from stk04_SysParam"


            ExecuteQuery(sql)

            CommonInitTablesProc(TInfos, tempTableName, "tblHeader", "Synchronizing  Header", SelectedLocation, MachineName)
            ''''''''''''''''''''''''

            tempTableName = "_" & ApplicationName & "_tbl_Stock_"
            tempTableName &= SelectedLocation & MachineName

            sql = "SELECT ROW_NUMBER() OVER( ORDER BY [stk_recid] ASC ) AS RowNumber ,"
            sql &= " stk_recid,STK_ref, stk_Desc,'' as barcode, '0' as usercounted,  '0' as updated "
            'into 
            sql &= " Into " & tempTableName & "xyz"

            sql &= " from "

            If locType.Trim.ToUpper = "S" Then
                sql &= " pos_stkfil "
            Else
                sql &= " STK04_stkfil "
            End If


            sql &= " inner join stk04_itemtype on stk_itmtype = IT_Code inner join stk04_supplier on stk_supplier = Sup_Code  LEFT JOIN dbo.Stk04_Brand ON br_Code = STK_Brand "



            ExecuteQuery(sql)

            CommonInitTablesProc(TInfos, tempTableName, "tblstock", "Synchronizing Details", SelectedLocation, MachineName)
            ''''''''''''''''''''''''

            tempTableName = "_" & ApplicationName & "_tbl_DetBarcode_"
            tempTableName &= SelectedLocation & MachineName

            sql = "select ROW_NUMBER() OVER( ORDER BY [dbar_stkid] ASC ) AS RowNumber , dbar_stkid,dbar_barcode"
            sql &= " ,'0' as updated"
            'into 
            sql &= " Into " & tempTableName & "xyz"
            sql &= " FROM Stk04_DetBarcode"

            ExecuteQuery(sql)

            CommonInitTablesProc(TInfos, tempTableName, "tblBarcodes", "Synchronizing Barcodes", SelectedLocation, MachineName)
            ''''''''''''''''''''''''




        Catch ex As Exception

        End Try
        Return TInfos
    End Function

    Public Function GetlocationType(ByVal Location As String) As String
        Dim sql As String = ""
        Dim conn As New SqlConnection(myConnectionString)
        Dim retResult As String = " "
        Try
            conn.Open()

            sql = "select * from STK04_Location where Loc_Code = '" & Location & "'"

            Dim cmd As New SqlCommand(sql, conn)
            cmd.CommandType = CommandType.Text

            Dim dr As SqlDataReader = cmd.ExecuteReader() ' Visual Basic



            While dr.Read
                retResult = IIf(dr.Item("loc_Type3") Is DBNull.Value, " ", dr.Item("loc_Type3").ToString.Replace("'", ""))
            End While

            conn.Close()
            dr.Dispose()
            cmd.Dispose()
            conn.Dispose()
        Catch ex As Exception

        End Try
        Return retResult
    End Function

#End Region

#Region "Clear Tables"

    Public Function ClearERPScanTables(ByVal ApplicationName As String, ByVal SelectedLocation As String, ByVal MachineName As String) As String
        Dim retval As String = "ok"
        Try
            Dim tempTableName As String = "_" & ApplicationName & "_stk04_SysParam_"
            tempTableName &= SelectedLocation & MachineName '& SelectedRecId
            DropTempTable(tempTableName)


            tempTableName = "_" & ApplicationName & "_tbl_Stock_"
            tempTableName &= SelectedLocation & MachineName
            DropTempTable(tempTableName)

            tempTableName = "_" & ApplicationName & "_tbl_DetBarcode_"
            tempTableName &= SelectedLocation & MachineName
            DropTempTable(tempTableName)

        Catch ex As Exception
            retval = ex.Message
        End Try
        Return retval
    End Function

#End Region

#Region "Upload"


    <WebMethod()> _
    Public Function InsertScannedItem(ByVal SelectedLocation As String, ByVal MachineName As String, ByVal id As String, ByVal stk_recid As String, ByVal STK_ref As String, ByVal stk_Desc As String, ByVal barcode As String, ByVal usercounted As String, ByVal fname As String) As String
        Dim str As String = "failed"
        Dim UpdateSQL As String = ""
        Dim lndata As String = ""
        Try
            Dim RootDir As String = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "bin") & "\"
            RootDir &= "ERPScanItems\"

            'Dim rootWebConfig1 As System.Configuration.Configuration
            'rootWebConfig1 = System.Web.Configuration.WebConfigurationManager.OpenWebConfiguration(Nothing)
            'If (rootWebConfig1.AppSettings.Settings.Count > 0) Then
            '    Dim customSetting As System.Configuration.KeyValueConfigurationElement
            '    customSetting = rootWebConfig1.AppSettings.Settings("ERPScanFolder")


            '    If Not (customSetting.Value = Nothing) Then
            '        RootDir = customSetting.Value
            '    Else
            '        '"No Setting"
            '    End If
            'End If
            RootDir = ConfigurationManager.AppSettings("ERPScanFolder")

            If fname.Trim = "" Then
                fname = SelectedLocation
            End If


            Dim FileName As String = RootDir & "\" & fname & ".csv"
            CreateNewDirectory(RootDir)

            If File.Exists(FileName) Then
                lndata = id & ","
                lndata &= STK_ref & ","
                lndata &= " " & barcode & ","
                'lndata &= stk_Desc & ","
                lndata &= usercounted '& vbCrLf
                AppendLineToFile(FileName, lndata)
            Else
                lndata = "Line No,Reference,Barcode,Quantity" '& vbCrLf

                AppendLineToFile(FileName, lndata)
                lndata = id & ","
                lndata &= STK_ref & ","
                lndata &= " " & barcode & ","
                'lndata &= stk_Desc & ","
                lndata &= usercounted '& vbCrLf
                AppendLineToFile(FileName, lndata)
            End If
            'UpdateSQL = " Update tbl_PhysicalCountDetail "
            'UpdateSQL &= " set Pcd_QtyCounted" & uUserToUpdate & " = " & usercounted
            'UpdateSQL &= " , Pcd_Type" & uUserToUpdate & " = 10"
            'UpdateSQL &= " where Pcd_RecId='" & Pcd_RecId & "'"


            'ExecuteQuery(UpdateSQL)

            str = "ok"
        Catch ex As Exception
            str = UpdateSQL & ":" & ex.Message

        End Try
        Return str

    End Function


#End Region

#End Region


#Region "Common Pull Methods"


    Private Function GetInsertValuesSqlQuery(ByRef CreateTable As String, ByRef insertinto As String, ByVal keyval As String, ByVal SqlQuery As String) As String
        Dim cn As New SqlConnection()
        Dim cmd As New SqlCommand()
        Dim schemaTable As DataTable
        Dim myReader As SqlDataReader
        Dim myField As DataRow
        Dim GeneratedQuery As String = ""
        Try
            CreateTable = "_id INTEGER PRIMARY KEY,"
            SqlQuery &= " order by " & keyval

            'Open a connection to the SQL Server Northwind database.
            cn.ConnectionString = myConnectionString
            cn.Open()

            'Retrieve records from the Employees table into a DataReader.
            cmd.Connection = cn
            cmd.CommandText = SqlQuery
            myReader = cmd.ExecuteReader(CommandBehavior.KeyInfo)

            'Retrieve column schema into a DataTable.
            schemaTable = myReader.GetSchemaTable()

            GeneratedQuery = "SELECT " & keyval & ","
            GeneratedQuery &= vbCrLf


            'For each field in the table...
            For Each myField In schemaTable.Rows
                If myField(schemaTable.Columns("isHidden")).ToString() = "False" Then

                    Dim mField As String = ""
                    Dim isnum As Boolean = False
                    Select Case myField(schemaTable.Columns("DataType")).ToString()
                        Case "System.Int64", "System.Int32", "System.Decimal", "System.Double", "System.Boolean"
                            isnum = True
                        Case Else
                            isnum = False
                    End Select

                    If isnum Then
                        '''' + isnull(cast([stkPrice] as nvarchar(18)),'') + ''',' + 
                        '''' + isnull(ltrim(str([stkPrice])),'') + ''',' +

                        'mField &= "'''' + isnull(ltrim(str(["
                        'mField &= myField(schemaTable.Columns("ColumnName")).ToString()
                        'mField &= "])),'') + ''',' + "

                        mField &= "'''' + isnull(cast(["
                        mField &= myField(schemaTable.Columns("ColumnName")).ToString()
                        mField &= "] as nvarchar(18)),'') + ''',' + "
                    Else
                        mField &= "'''' + replace(isnull(["
                        mField &= myField(schemaTable.Columns("ColumnName")).ToString()
                        mField &= "],''),'''','') + ''',' + "
                    End If

                    GeneratedQuery &= mField & vbCrLf
                    insertinto &= myField(schemaTable.Columns("ColumnName")).ToString() & ","

                    If myField(schemaTable.Columns("ColumnName")).ToString() = "RowNumber" Then
                        CreateTable &= myField(schemaTable.Columns("ColumnName")).ToString() & " INTEGER,"
                    Else
                        CreateTable &= myField(schemaTable.Columns("ColumnName")).ToString() & "  TEXT,"
                    End If

                    'myField(schemaTable.Columns("DataType")).ToString()
                End If
            Next
            CreateTable = Mid(CreateTable, 1, CreateTable.Length - 1)
            insertinto = Mid(insertinto, 1, insertinto.Length - 1)
            GeneratedQuery = Mid(GeneratedQuery, 1, GeneratedQuery.Length - 7)
            GeneratedQuery &= "'"

            'Always close the DataReader and Connection objects.
            myReader.Close()
            cn.Close()

            Return GeneratedQuery
        Catch ex As Exception
            Throw ex
        End Try
    End Function



    Public Function CommonInitTablesProc(ByRef TInfos As TableInfos, ByVal tempTableName As String, ByVal OriginalTableName As String, ByVal PullDisplayHeader As String, ByVal SelectedLocation As String, ByVal MachineName As String) As Boolean
        Dim retval As Boolean = True
        Try
            Dim myInsertInto As String = ""
            Dim myCreateTable As String = ""
            Dim newQuery As String = GetInsertValuesSqlQuery(myCreateTable, myInsertInto, "RowNumber", "select * from " & tempTableName & "xyz")
            newQuery &= " as valuesquery into " & tempTableName
            newQuery &= " From " & tempTableName & "xyz"

            ExecuteQuery(newQuery)

            If TInfos.mTableInfo Is Nothing Then
                ReDim TInfos.mTableInfo(0)
            Else
                ReDim Preserve TInfos.mTableInfo(TInfos.mTableInfo.Length)
            End If
            TInfos.mTableInfo(TInfos.mTableInfo.Length - 1) = New TableInfo
            With TInfos.mTableInfo(TInfos.mTableInfo.Length - 1)
                .OriginalTableName = OriginalTableName
                .InsertInto = "Insert Into " & .OriginalTableName & "(" & myInsertInto & ") Values"
                .TableName = tempTableName
                .RowsCount = GetTableRowsCount(tempTableName)
                .LastImportedRow = "0"
                .PullDisplayHeader = PullDisplayHeader
                .PullFinished = "0"
                .PullPreparedDT = Now.ToString("dd/MM/yyyy HH:mm:ss")
                .SelectedLocation = SelectedLocation
                .MachineName = MachineName
                .CreateTable = "CREATE TABLE IF NOT EXISTS " & .OriginalTableName & "(" & myCreateTable & ")"
                .Notes = ""
            End With
        Catch ex As Exception
            Throw ex
        End Try
        Return retval
    End Function


    <WebMethod()>
    Public Function InitPullTables(ByVal ApplicationName As String, ByVal SelectedLocation As String, ByVal MachineName As String, ByVal Extras As String, ByVal DeviceInfo As String) As TableInfos
        Dim TInfos As TableInfos = New TableInfos

        Try

            Select Case ApplicationName.ToUpper
                Case "ERPDATACOLLECTOR"
                    'If CheckDevice(DeviceInfo) Then
                    TInfos = InitERPPhysicalCountTables(ApplicationName, Extras, SelectedLocation, MachineName)
                    'End If

                Case "ERPSCAN"
                    TInfos = InitERPScanTables(ApplicationName, SelectedLocation, MachineName)
                    'Case "FREERECEIVING"
                    '    TInfos = InitFreeReceivingTables(ApplicationName, SelectedLocation, MachineName)
                    'Case "RECEIVING"
                    '    TInfos = InitReceivingTables(ApplicationName, SelectedLocation, MachineName, Extras)
                    'Case "SHOWROOM"
                    '    TInfos = InitShowRoomTables(ApplicationName, SelectedLocation, MachineName)
            End Select

        Catch ex As Exception

        End Try
        Return TInfos
    End Function

    Private Function CheckDevice(ByVal DeviceInfo As String) As Boolean
        Dim retval As Boolean = False
        Try


            Dim strsql As String = ""

#Region "Create Header"
            strsql = "select * from sysobjects where id = object_id(N'_AndroidDevices') "
            strsql &= " and OBJECTPROPERTY(id, N'IsUserTable') = 1"

            Dim tableExistTbl As New DataTable
            tableExistTbl = GetData(strsql)

            If tableExistTbl.Rows.Count <= 0 Then
                strsql = " CREATE TABLE [dbo].[_AndroidDevices]("
                strsql &= " [RecID] [int] IDENTITY(1,1) NOT NULL,"
                strsql &= " [DeviceInfo] [nvarchar](max) NULL,"
                strsql &= " [Allowed] [int]  NULL,"
                strsql &= " CONSTRAINT [PK__AndroidDevices] PRIMARY KEY CLUSTERED"
                strsql &= " ("
                strsql &= " [RecID] Asc"
                strsql &= " )WITH (IGNORE_DUP_KEY = OFF) ON [PRIMARY]"
                strsql &= " ) ON [PRIMARY];"

                ExecuteQuery(strsql)
            End If
#End Region

            If tableExistTbl IsNot Nothing Then
                tableExistTbl.Dispose() : tableExistTbl = Nothing
            End If
            tableExistTbl = New DataTable

            strsql = "select * from [dbo].[_AndroidDevices] where DeviceInfo = '" & DeviceInfo.Trim.Replace("'", "''") & "'"
            tableExistTbl = GetData(strsql)
            If tableExistTbl.Rows.Count <= 0 Then
                strsql = "insert into [dbo].[_AndroidDevices]([DeviceInfo],[Allowed]) Values("
                strsql &= "'" & DeviceInfo.Trim.Replace("'", "''") & "',0)"
                ExecuteQuery(strsql)

                Throw New Exception("Device Not Allowed")
            Else
                Dim Allowed As Integer = 0

                Allowed = IIf(tableExistTbl.Rows(0).Item("Allowed") Is DBNull.Value, 0, tableExistTbl.Rows(0).Item("Allowed"))
                If Allowed = 1 Then
                    retval = True
                Else
                    Throw New Exception("Device Not Allowed")
                End If
            End If

        Catch ex As Exception

        End Try
        Return retval
    End Function


    <WebMethod()> _
    Public Function pullRecords(ByVal TableName As String, ByVal PIndex As String, ByVal PSize As String, ByVal StartFromRow As String) As Records
        Dim Items As Records = New Records
        Dim sql As String = ""
        Dim conn As New SqlConnection(myConnectionString)
        Try
            conn.Open()

            Dim PageSize As Long = Val(PSize)
            Dim PageIndex As Long = Val(PIndex)

            Dim tempTableName As String = TableName

            sql &= " SELECT [valuesquery] FROM " & tempTableName '& "x"
            If StartFromRow.Trim = "" Then
                sql &= " WHERE RowNumber BETWEEN(" & PageIndex & " -1) * " & PageSize & " + 1 AND(((" & PageIndex & " -1) * " & PageSize & " + 1) + " & PageSize & ") - 1"
            Else
                Dim toRecord As Long = Val(StartFromRow)
                toRecord += PageSize - 1
                sql &= " WHERE RowNumber BETWEEN " & Val(StartFromRow) & " AND " & toRecord
            End If



            Dim cmd As New SqlCommand(sql, conn)
            cmd.CommandType = CommandType.Text
            Dim dr As SqlDataReader = cmd.ExecuteReader()
            While dr.Read
                If Items.mRecords Is Nothing Then
                    ReDim Items.mRecords(0)
                Else
                    ReDim Preserve Items.mRecords(Items.mRecords.Length)
                End If
                Items.mRecords(Items.mRecords.Length - 1) = New Record
                With Items.mRecords(Items.mRecords.Length - 1)
                    .valuesquery = IIf(dr.Item("valuesquery") Is DBNull.Value, " ", dr.Item("valuesquery").ToString)
                End With
            End While
            conn.Close()
            dr.Dispose()
            cmd.Dispose()
            conn.Dispose()
            DropTable(tempTableName & "x")
        Catch ex As Exception
        End Try
        Return Items
    End Function

    Public Class Records
        Public mRecords() As Record
    End Class
    Public Class Record
        Public valuesquery As String
    End Class

    <WebMethod()> _
    Public Function ClearPullTables(ByVal ApplicationName As String, ByVal SelectedLocation As String, ByVal MachineName As String) As String
        Dim retval As String = "ok"
        Try
            Select Case ApplicationName.ToUpper
                Case "ERPDATACOLLECTOR"
                    retval = ClearErpDataCollectorTables(ApplicationName, SelectedLocation, MachineName)
                Case "ERPSCAN"
                    retval = ClearERPScanTables(ApplicationName, SelectedLocation, MachineName)
                    'Case "FREERECEIVING"
                    '    retval = ClearFreeReceivingTables(ApplicationName, SelectedLocation, MachineName)
                    'Case "RECEIVING"
                    '    retval = ClearReceivingTables(ApplicationName, SelectedLocation, MachineName)
                    'Case "SHOWROOM"
                    '    retval = ClearShowRoomTables(ApplicationName, SelectedLocation, MachineName)
                Case ""
                    retval = ClearErpDataCollectorTables("ERPDataCollector", SelectedLocation, MachineName)
                    retval = ClearERPScanTables(ApplicationName, SelectedLocation, MachineName)
                    'retval = ClearFreeReceivingTables("FREERECEIVING", SelectedLocation, MachineName)
                    'retval = ClearReceivingTables("RECEIVING", SelectedLocation, MachineName)
                    'retval = ClearShowRoomTables("SHOWROOM", SelectedLocation, MachineName)
            End Select
        Catch ex As Exception
            retval = ex.Message
        End Try
        Return retval
    End Function

#End Region

#Region "COmmon Methods"

    Public Sub CreateNewDirectory(ByVal DirectoryPath As String)
        'this function creates a new directory if it's not found
        Try
            If System.IO.Directory.Exists(DirectoryPath) = False Then
                System.IO.Directory.CreateDirectory(DirectoryPath)
            End If
        Catch ex As Exception
            Throw ex
        End Try
    End Sub

    Public Sub AppendLineToFile(ByVal FilePath As String, ByVal strLine As String)
        Dim Canal As Long
        Try
            Canal = FreeFile()
            FileOpen(Canal, FilePath, OpenMode.Append)
            PrintLine(Canal, strLine)
            FileClose(Canal)
        Catch ex As Exception
            Throw ex
        End Try
    End Sub

    'for init tables when paged download requested
    Public Class TableInfos
        Public mTableInfo() As TableInfo
    End Class
    Public Class TableInfo
        Public TableName, RowsCount, OriginalTableName, LastImportedRow, PullDisplayHeader, InsertInto, PullFinished, PullPreparedDT, SelectedLocation, MachineName, CreateTable, Notes As String
    End Class
    'end for init tables when paged download requested

    Public Sub DropTempTable(ByVal tempTableName As String)
        Try
            Dim Sql As String = "if exists (select * from sysobjects where id = object_id(N'" & tempTableName & "') "
            Sql &= "and OBJECTPROPERTY(id, N'IsUserTable') = 1) drop table " & tempTableName & ";"
            Sql &= " if exists (select * from sysobjects where id = object_id(N'" & tempTableName & "x') "
            Sql &= "and OBJECTPROPERTY(id, N'IsUserTable') = 1) drop table " & tempTableName & "x;"
            Sql &= " if exists (select * from sysobjects where id = object_id(N'" & tempTableName & "xyz') "
            Sql &= "and OBJECTPROPERTY(id, N'IsUserTable') = 1) drop table " & tempTableName & "xyz;"
            ExecuteQuery(Sql)
        Catch ex As Exception
            Throw ex
        End Try
    End Sub

    Private Sub DropTable(ByVal tempTableName As String)
        Try
            Dim Sql As String = "if exists (select * from sysobjects where id = object_id(N'" & tempTableName & "') "
            Sql &= "and OBJECTPROPERTY(id, N'IsUserTable') = 1) drop table " & tempTableName & ""
            ExecuteQuery(Sql)
        Catch ex As Exception
            Throw ex
        End Try
    End Sub

    Public Function GetTableRowsCount(ByVal TableName As String) As Long
        Dim retResult As Long = 0
        Dim tempTable As New DataTable
        Try
            tempTable = GetData("select count(*) as cnt from " & TableName)
            retResult = tempTable.Rows(0).Item(0)
        Catch ex As Exception
            retResult = 0
        Finally
            If tempTable IsNot Nothing Then
                tempTable.Dispose()
                tempTable = Nothing
            End If
        End Try
        Return retResult
    End Function
    Public Function GetData(ByVal SqlQuery As String) As DataTable
        Dim dt As New DataTable
        Dim con As New SqlConnection(myConnectionString)
        Dim sda As New SqlDataAdapter
        Dim cmd As SqlCommand = New SqlCommand(SqlQuery, con)
        'cmd.CommandType = CommandType.Text
        'cmd.Connection = con
        Try

            con.Open()
            sda.SelectCommand = cmd
            sda.Fill(dt)
            Return dt
        Catch ex As Exception
            Return Nothing
        Finally
            con.Close()
            sda.Dispose()
            con.Dispose()
        End Try
    End Function

    Private Sub ExecuteQuery(ByVal strQuery As String)

        Dim con As New SqlConnection(myConnectionString)
        Dim cmd As SqlCommand = Nothing

        Try
            cmd = New SqlCommand(strQuery, con)
            con.Open()
            cmd.ExecuteNonQuery()
        Catch ex As Exception
            Throw ex
        Finally
            If con IsNot Nothing Then
                con.Close()
                con.Dispose()
            End If
            If cmd IsNot Nothing Then
                cmd.Dispose() : cmd = Nothing
            End If
        End Try
    End Sub


#End Region

#End Region


End Class