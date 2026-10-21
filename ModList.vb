Option Explicit Off
Imports CrystalDecisions.CrystalReports.Engine
Imports System.IO
Imports C1.Win.C1Input
Imports C1.Win.C1TrueDBGrid
Imports System.Data.SqlClient
Imports System.Reflection
Imports DevComponents.DotNetBar

Module ModList
    Public Function ItemColorStrsql(ByVal RecID As String, Optional ByVal Cond As String = "") As String
        Try
            ItemColorStrsql = "SELECT Color_Code AS Code, Color_Desc AS Description,"
            ItemColorStrsql &= " CASE WHEN Color_Active = 'Y' THEN 'True' ELSE 'False' END Active"
            ItemColorStrsql &= " FROM STK04_Color"
            ItemColorStrsql &= " LEFT JOIN ( SELECT  DISTINCT STQ_color FROM STK04_STKQTY"
            ItemColorStrsql &= " WHERE STQ_STKRECID = '" & RecID & "')"
            ItemColorStrsql &= " Colors ON stq_color = color_code"
            ItemColorStrsql &= " WHERE (Colors.STQ_color IS NOT NULL OR Color_Active = 'Y')"
            If Cond <> "" Then ItemColorStrsql &= " AND " & Cond
        Catch ex As Exception
            Throw ex
        End Try
    End Function
    Public Function HSCodeStrsql(Optional ByVal Cond As String = "") As String
        Try
            HSCodeStrsql = "SELECT HSC_HSChapter AS [Chapter],HSCh_Desc [Chapter Description],"
            HSCodeStrsql &= " HSC_HSCode [HS Code] FROM dbo.STK04_HSCode "
            HSCodeStrsql &= " INNER JOIN dbo.STK04_HSChapter ON HSC_HSChapter=HSCh_Code"
            If Cond <> "" Then
                HSCodeStrsql &= " And " & Cond
            End If
        Catch ex As Exception
            Throw ex
        End Try
    End Function
    Public Function SupplierStrsql(Optional ByVal Cond As String = "", Optional ByVal Active As Boolean = True,
                                   Optional DISTINCT As Boolean = False, Optional SupplierType As Boolean = False) As String
        Try
            SupplierStrsql = "SELECT  " & IIf(DISTINCT, "DISTINCT", "").ToString & " SUP_Recid AS Recid, SUP_Code AS Code, SUP_Name AS Name,"
            SupplierStrsql &= " sup_Country AS Country , sup_curr AS Curr, SUP_CshInvDisc AS Disc,"
            SupplierStrsql &= " CASE WHEN SUP_Active = 'Y' THEN 'True' ELSE 'False' END AS [Active],"
            SupplierStrsql &= " CASE WHEN Sup_LocFor = 'L' THEN 'True' ELSE 'False' END AS [Local],"
            SupplierStrsql &= " CASE WHEN Sup_VATType = 'T' THEN 'Taxable' ELSE 'Non Taxable' END AS [VAT Type]"
            If SupplierType Then SupplierStrsql &= " ,STUFF((SELECT  ','+ SUPT_Desc   AS 'data()'  FROM  stk04_SuppTypeLink "
            If SupplierType Then SupplierStrsql &= " inner join  stk04_SuppType  on SUPTL_TypeCode  = SUPT_Code  WHERE SUPTL_SupRecid"
            If SupplierType Then SupplierStrsql &= " = Sup_RecId FOR XML PATH('') ),1,1,'')  [Linked Type]  "
            SupplierStrsql &= " FROM VSuppliers   WHERE 1=1 "
            If Active Then SupplierStrsql &= " AND SUP_Active = 'Y'"
            If Cond <> "" Then SupplierStrsql &= " And " & Cond
        Catch ex As Exception
            Throw ex
        End Try
    End Function
    Public Function ColorsStrsql(Optional ByVal Cond As String = "", Optional ByVal Active As Boolean = True) As String
        Try
            ColorsStrsql = "SELECT Color_Code Code,Color_Desc Description,"
            ColorsStrsql &= " CASE WHEN Color_Active='y' THEN 'True' ELSE 'False' END AS [Active]"
            ColorsStrsql &= " FROM dbo.Stk04_COLOR"
            ColorsStrsql &= " WHERE 1=1"
            If Active Then ColorsStrsql &= " AND Color_Active = 'Y'"
            If Cond <> "" Then ColorsStrsql &= " And " & Cond
        Catch ex As Exception
            Throw ex
        End Try
    End Function

    Public Function CategoryStrsql(Optional ByVal Cond As String = "", Optional ByVal Active As Boolean = True) As String
        Try
            CategoryStrsql = "select cat_Code AS Code, cat_desc AS Description,"
            CategoryStrsql &= " case when Cat_Active='y' THEN 'True' ELSE 'False' END AS [Active]"
            CategoryStrsql &= " FROM Stk04_Category "
            CategoryStrsql &= " where 1=1 "
            If Active Then CategoryStrsql &= " AND Cat_Active = 'Y'"
            If Cond <> "" Then CategoryStrsql &= " And " & Cond
        Catch ex As Exception
            Throw ex
        End Try
    End Function

    Public Function PublishersStrsql(Optional ByVal Cond As String = "") As String
        Try
            PublishersStrsql = " select P_Code Code,P_name Description FROM dbo.BIBL1_Publishers"
            If Cond <> "" Then PublishersStrsql &= " And " & Cond
        Catch ex As Exception
            Throw ex
        End Try
    End Function
    Public Function SeasonStrsql(Optional ByVal Cond As String = "", Optional ByVal Active As Boolean = True) As String
        Try
            SeasonStrsql = "SELECT Seas_Code AS Code,Seas_Desc AS Description,convert(varchar,Seas_Year) AS Year,"
            SeasonStrsql &= " CASE WHEN Seas_Season ='W' THEN 'Winter' ELSE CASE WHEN Seas_Season='R' THEN 'Spring' ELSE "
            SeasonStrsql &= " CASE WHEN Seas_Season='S' THEN 'Summer' ELSE CASE WHEN Seas_Season='A' THEN 'All' ELSE "
            SeasonStrsql &= " 'Autumn' END END END END AS Type ,case when seas_active='y' THEN 'True' ELSE 'False' END AS [Active]"
            SeasonStrsql &= " FROM dbo.Stk04_Season WHERE 1=1"
            If Active Then SeasonStrsql &= " AND seas_active = 'Y'"
            If Cond <> "" Then SeasonStrsql &= " AND " & Cond
        Catch ex As Exception
            Throw ex
        End Try
    End Function
    Public Function BrandStrsql(Optional ByVal Cond As String = "", Optional ByVal Active As Boolean = True) As String
        Try
            BrandStrsql = "SELECT br_Code AS Code ,br_Name AS Description ,case when br_Active='y' THEN 'True' ELSE 'False' END AS [Active] "
            BrandStrsql &= " FROM dbo.Stk04_Brand WHERE 1=1"
            If Active Then BrandStrsql &= " AND br_Active = 'Y'"
            If Cond <> "" Then BrandStrsql &= " and " & Cond
        Catch ex As Exception
            Throw ex
        End Try
    End Function
    Public Function ColorStrsql(Optional ByVal Cond As String = "", Optional ByVal Active As Boolean = True) As String
        Try
            ColorStrsql = "SELECT Color_Code AS Code ,Color_Desc AS Description,"
            ColorStrsql &= " case when Color_Active='y' THEN 'True' ELSE 'False' END AS [Active]"
            ColorStrsql &= " FROM Stk04_COLOR WHERE  1=1"
            If Active Then ColorStrsql &= " AND Color_Active = 'Y'"
            If Cond <> "" Then ColorStrsql &= " and " & Cond
        Catch ex As Exception
            Throw ex
        End Try
    End Function
    Public Function DepartmentStrsql(Optional ByVal Cond As String = "", Optional ByVal Active As Boolean = True) As String
        Try
            DepartmentStrsql = "SELECT Grp_Code Code,Grp_DESC Description ,"
            DepartmentStrsql &= " case when Grp_Active='y' THEN 'True' ELSE 'False' END AS [Active]"
            DepartmentStrsql &= " FROM dbo.Stk04_Group WHERE 1=1 "
            If Active Then DepartmentStrsql &= " AND Grp_Active = 'Y'"
            If Cond <> "" Then DepartmentStrsql &= " AND " & Cond
        Catch ex As Exception
            Throw ex
        End Try
    End Function
    'Added by Abir T. @ 24.04.2013
    Public Function GenderStrsql(Optional ByVal Cond As String = "", Optional ByVal Active As Boolean = True, Optional ByVal Gender As String = "") As String
        Try
            GenderStrsql = "SELECT G_Code Code, G_DESC Description ,"
            GenderStrsql &= " CASE WHEN G_Active='Y' THEN 'True' ELSE 'False' END AS [Active],"
            'Abir T. @ 20.08.2013
            GenderStrsql &= " CASE WHEN G_Code = '" & Gender & "' THEN 'True'"
            GenderStrsql &= " WHEN G_Desc = '" & Gender & "' THEN 'True'"
            GenderStrsql &= " WHEN G_Desc LIKE '%" & Gender & "%' THEN 'True'"
            GenderStrsql &= " ELSE 'False' END AS [Match]"
            GenderStrsql &= " FROM Stk04_GENDer WHERE 1=1 "
            If Active Then GenderStrsql &= " AND G_Active = 'Y'"

            If Cond <> "" Then GenderStrsql &= " AND " & Cond
        Catch ex As Exception
            Throw ex
        End Try
    End Function
    Public Sub CreateTmpSN(ByVal TempTable As String)
        Try
            DropSqlObject(TempTable, "TABLE")
            strsql = "Create table " & TempTable & " ( "
            strsql &= " [SN_RecID] varchar(40) not null ,"
            strsql &= " [SN_InvDID] numeric(18,3) , "
            strsql &= " [SN_Color] varchar(10) , "
            strsql &= " [SN_Grid] varchar(10) , "
            strsql &= " [SN_Size] varchar(10) , "
            strsql &= " [SN_Remarks] varchar(200) , "
            strsql &= " [SN_StrDate] Datetime , "
            strsql &= " [SN_EndDate] Datetime , "
            strsql &= " [SN_Status] [varchar] (1) , "
            strsql &= " CONSTRAINT PK_" & TempTable & " PRIMARY KEY  (SN_RecID,SN_Status) "
            strsql &= " ) ON [PRIMARY]"
            Execute(strsql)

            strsql = " Insert Into " & TempTable & " "
            strsql &= " Select StkS_ItemSN,StkS_PurInv,StkS_Color,StkS_Grid,StkS_Size,StkS_Remark,StkS_StWarSup,StkS_EdWarSup,'O'  From DLO_StockSerial "
            strsql &= " Where  StkS_PurInv is not Null and StkS_PurInv <>0 "
            Execute(strsql)
        Catch ex As Exception
            Throw ex
        End Try
    End Sub
    Public Function StyleStrsql(Optional ByVal Cond As String = "", Optional ByVal Active As Boolean = True) As String
        Try
            StyleStrsql = "SELECT ST_CODE AS Code,ST_DESC AS Description,"
            StyleStrsql &= " case when ST_ACTIVE='y' THEN 'True' ELSE 'False' END AS [Active]"
            StyleStrsql &= " FROM dbo.stk04_STYLE WHERE 1=1 "
            If Active Then StyleStrsql &= " AND ST_Active = 'Y'"
            If Cond <> "" Then StyleStrsql &= " AND " & Cond
            StyleStrsql &= " ORDER BY ST_CODE"
        Catch ex As Exception
            Throw ex
        End Try
    End Function
    Public Function StyleProdGrpDepStrsql(Optional ByVal Cond As String = "", Optional ByVal Active As Boolean = True) As String
        Try
            StyleProdGrpDepStrsql = "SELECT  ST_CODE AS [Code], ST_DESC AS [Description],"
            StyleProdGrpDepStrsql &= " IT_Code AS [Product Grp. Code], IT_DESC [Product Group Description],"
            StyleProdGrpDepStrsql &= " IT_Group AS [Department Code], Grp_DESC [Department Description],"
            StyleProdGrpDepStrsql &= " CASE WHEN ST_Active='Y' THEN 'True' ELSE 'False' END AS [Active]"
            StyleProdGrpDepStrsql &= " FROM stk04_STYLE"
            StyleProdGrpDepStrsql &= " LEFT JOIN Stk04_StyleLinkItmType ON ST_CODE = SLIT_StyleCode"
            StyleProdGrpDepStrsql &= " LEFT JOIN STK04_ItemType ON SLIT_ItRecId = IT_RecID"
            StyleProdGrpDepStrsql &= " LEFT JOIN Stk04_Group ON IT_Group = Grp_Code "
            StyleProdGrpDepStrsql &= " WHERE ISNULL(IT_ACTIVE,'Y')='Y' AND ISNULL(Grp_Active,'Y')='Y'"
            If Active Then StyleProdGrpDepStrsql &= " AND ST_Active = 'Y'"
            If Cond <> "" Then StyleProdGrpDepStrsql &= " AND " & Cond
            StyleProdGrpDepStrsql &= " ORDER BY ST_CODE"
        Catch ex As Exception
            Throw ex
        End Try
    End Function
    Public Function ItmIdentStrsql(Optional ByVal Cond As String = "") As String
        Try
            ItmIdentStrsql = "SELECT ID_CODE AS Code, ID_DESC AS Description FROM stk04_itmidentif where  isnull(id_active ,'Y') ='Y' "
            If Cond <> "" Then ItmIdentStrsql &= " And " & Cond
        Catch ex As Exception
            Throw ex
        End Try
    End Function
    Public Function ItmTypeStrsql(Optional ByVal Cond As String = "", Optional ByVal Active As Boolean = True) As String
        Try
            ItmTypeStrsql = "SELECT IT_Code Code,IT_DESC Description,"
            ItmTypeStrsql &= " case when IT_ACTIVE='y' THEN 'True' ELSE 'False' END AS [Active]"
            ItmTypeStrsql &= " FROM stk04_itemtype WHERE 1=1"
            If Active Then ItmTypeStrsql &= " AND IT_ACTIVE = 'Y'"
            If Cond <> "" Then ItmTypeStrsql &= " And " & Cond
        Catch ex As Exception
            Throw ex
        End Try
    End Function
    Public Function ItmTypeGroupStrsql(Optional ByVal cond As String = "") As String
        Try
            ItmTypeGroupStrsql = "SELECT IT_Code Code,IT_DESC Description, IT_Group AS [Dep. Code], Grp_Desc AS [Department Desc.] "
            ItmTypeGroupStrsql &= " FROM stk04_itemtype left join stk04_group on IT_Group = Grp_Code "
            ItmTypeGroupStrsql &= " WHERE IT_ACTIVE='Y' AND ISNULL(Grp_Active,'Y')='Y'"
            If cond <> "" Then ItmTypeGroupStrsql &= " And " & cond
            ItmTypeGroupStrsql &= " order by IT_Code"
        Catch ex As Exception
            Throw ex
        End Try
    End Function
    Public Function SuppPrefStrsql(Optional ByVal Cond As String = "", Optional ByVal Active As Boolean = True) As String
        Try
            SuppPrefStrsql = " select Sup_cODE AS [Code],Sup_name AS [Supplier Name]"
            SuppPrefStrsql &= " FROM Stk04_Supplier where 1 = 1"
            If Active Then SuppPrefStrsql &= " AND  Sup_Active='Y'"
            If Cond <> "" Then SuppPrefStrsql &= " And " & Cond
        Catch ex As Exception
            Throw ex
        End Try
    End Function

    Public Function UnitStrsql(Optional ByVal Cond As String = "") As String
        Try
            UnitStrsql = "SELECT meas_unit Code, meas_type Type, meas_desc Description FROM STK04_Measurements where 1=1 "
            If Cond <> "" Then UnitStrsql &= " And " & Cond
        Catch ex As Exception
            Throw ex
        End Try
    End Function

    Public Function BookSubjStrsql(Optional ByVal Cond As String = "") As String
        Try
            BookSubjStrsql = " select S_Code Code,S_Desc Description FROM BIBL1_Subjects"
            If Cond <> "" Then BookSubjStrsql &= " And " & Cond
        Catch ex As Exception
            Throw ex
        End Try
    End Function
    Public Function SuppCategoryStrsql(Optional ByVal Cond As String = "") As String
        Try
            SuppCategoryStrsql = "  select C_Code AS [Code],C_desc AS [Description] FROM dbo.BIBL1_Category"
            If Cond <> "" Then SuppCategoryStrsql &= " And " & Cond
        Catch ex As Exception
            Throw ex
        End Try
    End Function
    Public Function CurrencyStrsql(Optional ByVal Cond As String = "") As String
        Try
            CurrencyStrsql = "SELECT Cur_Code Code,Cur_Desc Description FROM dbo.Com_Currency where 1=1 "
            If Cond <> "" Then CurrencyStrsql &= " And " & Cond
        Catch ex As Exception
            Throw ex
        End Try
    End Function
    Public Function LocationStrsql(Optional ByVal cond As String = "", Optional ByVal Active As Boolean = True) As String
        Try
            LocationStrsql = "SELECT Loc_code AS Code, Loc_desc AS Description,"
            LocationStrsql &= " CASE WHEN Loc_Active='Y' THEN 'True' ELSE 'False' END AS [Active]"
            LocationStrsql &= " FROM STK04_Location WHERE 1= 1"
            If Active = True Then LocationStrsql &= " AND Loc_Active = 'Y' "
            If cond <> "" Then LocationStrsql &= " And " & cond
        Catch ex As Exception
            Throw ex
        End Try
    End Function

    Public Function BooksTypeStrSql(Optional ByVal Cond As String = "") As String
        Try
            BooksTypeStrSql = " select T_Code AS [Code],T_desc AS [Description] FROM dbo.BiBL1_Type"
            If Cond <> "" Then BooksTypeStrSql &= " And " & Cond
        Catch ex As Exception
            Throw ex
        End Try
    End Function

    Public Function BooksLangStrsql(Optional ByVal Cond As String = "") As String
        Try
            BooksLangStrsql = " select L_Code AS [Code],L_Desc AS [Description] FROM BIBL1_Language "
            If Cond <> "" Then BooksLangStrsql &= " And " & Cond
        Catch ex As Exception
            Throw ex
        End Try
    End Function
    Public Sub VATList(ByVal TxtVatPerc As TextBox, ByVal FrmOwner As Form)
        Dim cnSql As SqlConnection
        Dim dsSql As DataSet
        Dim daSql As SqlDataAdapter
        Dim frm As frmSearch
        Dim strsql As String
        Dim Sizefield() As Int16 = {205}
        Dim Alnfield() As String = {"C"}
        Dim Retfield() As String = {"VAT %"}
        Try
            Cursor.Current = Cursors.WaitCursor

            cnSql = New SqlConnection(ConnectionString)
            cnSql.Open()

            strsql = "Select VP_VAT [VAT %] FROM STK04_VATPerc"

            daSql = New SqlDataAdapter(strsql, cnSql)

            dsSql = New DataSet

            daSql.Fill(dsSql, "tbcode")

            cnSql.Close()

            frm = New frmSearch

            With frm
                .Owner = FrmOwner
                If .ShowDialog("Return", dsSql.Tables("tbcode"), dsSql.Tables("tbcode").Rows.Count,
                "VAT % List", Sizefield, Alnfield, Retfield, 0, Nothing, False, 250, 300, , True) = DialogResult.OK Then
                    If gbRetSearch(0) <> "" Then
                        TxtVatPerc.Text = gbRetSearch(0)
                    End If
                    TxtVatPerc.Focus()
                    SendKeys.Send("{TAB}")
                Else
                    TxtVatPerc.Focus()
                End If
            End With

        Catch exp As Exception
            Throw exp
        Finally
            gbRetSearch = Nothing
            dsSql = Nothing : daSql = Nothing
            If Not cnSql Is Nothing Then
                If cnSql.State = ConnectionState.Open Then cnSql.Close()
                cnSql = Nothing
            End If
            frm = Nothing
            Cursor.Current = Cursors.Default
        End Try
    End Sub

    Public Sub Publishers(ByVal txtpubl As TextBox, ByVal lblpubl As Label, ByVal FrmOwner As Form, ByVal TITLE As String)
        Dim strsql As String
        Dim cnSql As SqlConnection
        Dim dsSql As DataSet
        Dim daSql As SqlDataAdapter
        Dim frm As frmSearch
        Dim Sizefield() As Int16 = {40, 160}
        Dim Alnfield() As String = {"C", "N"}
        Dim Retfield() As String = {"Code", "Description"}

        Try

            Cursor.Current = Cursors.WaitCursor


            cnSql = New SqlConnection(ConnectionString)
            cnSql.Open()
            daSql = New SqlDataAdapter(PublishersStrsql, cnSql)
            dsSql = New DataSet
            daSql.Fill(dsSql, "tbcode3")
            cnSql.Close()
            frm = New frmSearch

            With frm
                .Owner = FrmOwner
                If .ShowDialog("Return", dsSql.Tables("tbcode3"), dsSql.Tables("tbcode3").Rows.Count,
                TITLE, Sizefield, Alnfield, Retfield, 0, Nothing, False, 400, 500, , True) = DialogResult.OK Then
                    If gbRetSearch(0) = "" Then
                        txtpubl.Focus()
                    Else
                        If gbRetSearch(0) <> "" Then
                            txtpubl.Text = gbRetSearch(0)
                            lblpubl.Text = gbRetSearch(1)
                            txtpubl.Focus()
                            SendKeys.Send("{TAB}")
                        Else
                            txtpubl.Focus()
                        End If
                    End If
                End If
            End With

        Catch exp As Exception
            Throw exp
        Finally
            gbRetSearch = Nothing
            dsSql = Nothing : daSql = Nothing
            If Not cnSql Is Nothing Then
                If cnSql.State = ConnectionState.Open Then cnSql.Close()
                cnSql = Nothing
            End If
            frm = Nothing
            Cursor.Current = Cursors.Default
        End Try
    End Sub
    Public Sub BooksLang(ByVal txtLang As TextBox, ByVal lblLang As Label, ByVal FrmOwner As Form)
        Dim strsql As String
        Dim cnSql As SqlConnection
        Dim dsSql As DataSet
        Dim daSql As SqlDataAdapter
        Dim frm As frmSearch
        Dim Sizefield() As Int16 = {40, 160}
        Dim Alnfield() As String = {"C", "N"}
        Dim Retfield() As String = {"Code", "Description"}

        Try

            Cursor.Current = Cursors.WaitCursor


            cnSql = New SqlConnection(ConnectionString)
            cnSql.Open()
            daSql = New SqlDataAdapter(BooksLangStrsql, cnSql)
            dsSql = New DataSet
            daSql.Fill(dsSql, "tbcode3")
            cnSql.Close()
            frm = New frmSearch

            With frm
                .Owner = FrmOwner
                If .ShowDialog("Return", dsSql.Tables("tbcode3"), dsSql.Tables("tbcode3").Rows.Count,
                "Languages", Sizefield, Alnfield, Retfield, 0, Nothing, False, 400, 500, , True) = DialogResult.OK Then
                    If gbRetSearch(0) = "" Then
                        txtLang.Focus()
                    Else
                        If gbRetSearch(0) <> "" Then
                            txtLang.Text = gbRetSearch(0)
                            lblLang.Text = gbRetSearch(1)
                            txtLang.Focus()
                            SendKeys.Send("{TAB}")
                        Else
                            txtLang.Focus()
                        End If
                    End If
                End If
            End With

        Catch exp As Exception
            Throw exp
        Finally
            gbRetSearch = Nothing
            dsSql = Nothing : daSql = Nothing
            If Not cnSql Is Nothing Then
                If cnSql.State = ConnectionState.Open Then cnSql.Close()
                cnSql = Nothing
            End If
            frm = Nothing
            Cursor.Current = Cursors.Default
        End Try
    End Sub
    Public Sub BooksSubj(ByVal txtSubj As TextBox, ByVal lblSubj As Label, ByVal FrmOwner As Form)
        Dim strsql As String
        Dim cnSql As SqlConnection
        Dim dsSql As DataSet
        Dim daSql As SqlDataAdapter
        Dim frm As frmSearch
        Dim Sizefield() As Int16 = {40, 160}
        Dim Alnfield() As String = {"C", "N"}
        Dim Retfield() As String = {"Code", "Description"}

        Try

            Cursor.Current = Cursors.WaitCursor


            cnSql = New SqlConnection(ConnectionString)
            cnSql.Open()
            daSql = New SqlDataAdapter(BookSubjStrsql, cnSql)
            dsSql = New DataSet
            daSql.Fill(dsSql, "tbcode3")
            cnSql.Close()
            frm = New frmSearch

            With frm
                .Owner = FrmOwner
                If .ShowDialog("Return", dsSql.Tables("tbcode3"), dsSql.Tables("tbcode3").Rows.Count,
                "Subjects", Sizefield, Alnfield, Retfield, 0, Nothing, False, 300, 500, , True) = DialogResult.OK Then
                    If gbRetSearch(0) = "" Then
                        txtSubj.Focus()
                    Else
                        If gbRetSearch(0) <> "" Then
                            txtSubj.Text = gbRetSearch(0)
                            lblSubj.Text = gbRetSearch(1)
                            txtSubj.Focus()
                            SendKeys.Send("{TAB}")
                        Else
                            txtSubj.Focus()
                        End If
                    End If
                End If
            End With

        Catch exp As Exception
            Throw exp
        Finally
            gbRetSearch = Nothing
            dsSql = Nothing : daSql = Nothing
            If Not cnSql Is Nothing Then
                If cnSql.State = ConnectionState.Open Then cnSql.Close()
                cnSql = Nothing
            End If
            frm = Nothing
            Cursor.Current = Cursors.Default
        End Try
    End Sub
    Public Sub RefSupplier(ByVal txtsupp As TextBox, ByVal lblsupp As Label, ByVal FrmOwner As Form, ByVal TITLE As String)
        Dim strsql As String
        Dim cnSql As SqlConnection
        Dim dsSql As DataSet
        Dim daSql As SqlDataAdapter
        Dim frm As frmSearch
        Dim Sizefield() As Int16 = {40, 300}
        Dim Alnfield() As String = {"C", "N"}
        Dim Retfield() As String = {"Code", "Supplier Name"}

        Try

            Cursor.Current = Cursors.WaitCursor


            cnSql = New SqlConnection(ConnectionString)
            cnSql.Open()
            daSql = New SqlDataAdapter(SuppPrefStrsql, cnSql)
            dsSql = New DataSet
            daSql.Fill(dsSql, "tbcode3")
            cnSql.Close()
            frm = New frmSearch

            With frm
                .Owner = FrmOwner
                If .ShowDialog("Return", dsSql.Tables("tbcode3"), dsSql.Tables("tbcode3").Rows.Count,
                TITLE, Sizefield, Alnfield, Retfield, 0, Nothing, False, 400, 500, , True) = DialogResult.OK Then
                    If gbRetSearch(0) = "" Then
                        txtsupp.Focus()
                    Else
                        If gbRetSearch(0) <> "" Then
                            txtsupp.Text = gbRetSearch(0)
                            lblsupp.Text = gbRetSearch(1)
                            txtsupp.Focus()
                            SendKeys.Send("{TAB}")
                        Else
                            txtsupp.Focus()
                        End If
                    End If
                End If
            End With

        Catch exp As Exception
            Throw exp
        Finally
            gbRetSearch = Nothing
            dsSql = Nothing : daSql = Nothing
            If Not cnSql Is Nothing Then
                If cnSql.State = ConnectionState.Open Then cnSql.Close()
                cnSql = Nothing
            End If
            frm = Nothing
            Cursor.Current = Cursors.Default
        End Try
    End Sub
    Public Sub BooksType(ByVal TxtType As TextBox, ByVal lblType As Label, ByVal FrmOwner As Form)

        Dim cnSql As SqlConnection
        Dim dsSql As DataSet
        Dim daSql As SqlDataAdapter
        Dim frm As frmSearch
        Dim Sizefield() As Int16 = {40, 160}
        Dim Alnfield() As String = {"C", "N"}
        Dim Retfield() As String = {"Code", "Description"}

        Try

            Cursor.Current = Cursors.WaitCursor


            cnSql = New SqlConnection(ConnectionString)
            cnSql.Open()

            daSql = New SqlDataAdapter(BooksTypeStrSql, cnSql)
            dsSql = New DataSet
            daSql.Fill(dsSql, "tbcode3")
            cnSql.Close()
            frm = New frmSearch

            With frm
                .Owner = FrmOwner
                If .ShowDialog("Return", dsSql.Tables("tbcode3"), dsSql.Tables("tbcode3").Rows.Count,
                "Title/DVDs Type", Sizefield, Alnfield, Retfield, 0, Nothing, False, 500, 500, , True) = DialogResult.OK Then
                    If gbRetSearch(0) = "" Then
                        TxtType.Focus()
                    Else
                        If gbRetSearch(0) <> "" Then
                            TxtType.Text = gbRetSearch(0)
                            lblType.Text = gbRetSearch(1)
                            TxtType.Focus()
                            SendKeys.Send("{TAB}")
                        Else
                            TxtType.Focus()
                        End If
                    End If
                End If
            End With

        Catch exp As Exception
            Throw exp
        Finally
            gbRetSearch = Nothing
            dsSql = Nothing : daSql = Nothing
            If Not cnSql Is Nothing Then
                If cnSql.State = ConnectionState.Open Then cnSql.Close()
                cnSql = Nothing
            End If
            frm = Nothing
            Cursor.Current = Cursors.Default
        End Try
    End Sub
    Public Sub HSCodeList(ByVal TxtHSCode As TextBox, ByVal FrmOwner As Form, Optional ByVal Cond As String = "")
        Dim cnSql As SqlConnection
        Dim dsSql As DataSet
        Dim daSql As SqlDataAdapter
        Dim frm As frmSearch
        Dim Sizefield() As Int16 = {60, 250, 250}
        Dim Alnfield() As String = {"C", "N", "N"}
        Dim Retfield() As String = {"Chapter", "Chapter Description", "HS Code"}

        Try
            Cursor.Current = Cursors.WaitCursor
            cnSql = New SqlConnection(ConnectionString)
            cnSql.Open()
            daSql = New SqlDataAdapter(HSCodeStrsql(Cond), cnSql)
            dsSql = New DataSet
            daSql.Fill(dsSql, "tbcode")
            cnSql.Close()
            frm = New frmSearch
            With frm
                .Owner = FrmOwner
                If .ShowDialog("Return", dsSql.Tables("tbcode"), dsSql.Tables("tbcode").Rows.Count,
                "HS Code List", Sizefield, Alnfield, Retfield, 0, Nothing, False, 610, 500, , True) = DialogResult.OK Then
                    If gbRetSearch(0) <> "" Then
                        TxtHSCode.Text = gbRetSearch(2)
                    End If
                    TxtHSCode.Focus()
                    SendKeys.Send("{TAB}")
                Else
                    TxtHSCode.Focus()
                End If
            End With
        Catch exp As Exception
            Throw exp
        Finally
            gbRetSearch = Nothing
            dsSql = Nothing : daSql = Nothing
            If Not cnSql Is Nothing Then
                If cnSql.State = ConnectionState.Open Then cnSql.Close()
                cnSql = Nothing
            End If
            frm = Nothing
            Cursor.Current = Cursors.Default
        End Try
    End Sub
    Public Sub SupplierList(ByVal TxtSupp As TextBox, ByVal TxtSuppDesc As Label, ByVal FrmOwner As Form,
            Optional ByVal Cond As String = "", Optional ByVal TAB As Boolean = True, Optional ByVal Active As Boolean = True,
            Optional ByVal LocForg As Boolean = False, Optional ByVal VATType As Boolean = False, Optional disctict As Boolean = False, Optional SupplierType As Boolean = False)
        'Modified by Abir T. @ 01.08.2014 : Add a parametric field ( VAT Type )

        Dim cnSql As SqlConnection
        Dim dsSql As DataSet
        Dim daSql As SqlDataAdapter
        Dim frm As frmSearch

        Dim FilterInfo(0, 1) As String
        Dim Sizefield() As Int16
        Dim Alnfield() As String
        Dim Retfield() As String
        Dim ColumnPresentation(,) As Object = {{"Active", PresentationEnum.CheckBox}, {"Local", PresentationEnum.CheckBox}}
        FilterInfo(0, 0) = "Active" : FilterInfo(0, 1) = "True"


        Try
            Cursor.Current = Cursors.WaitCursor
            cnSql = New SqlConnection(ConnectionString)
            cnSql.Open()
            daSql = New SqlDataAdapter(SupplierStrsql(Cond, Active, disctict, SupplierType), cnSql)
            dsSql = New DataSet
            daSql.Fill(dsSql, "tbcode")
            cnSql.Close()
            If SupplierType Then
                Sizefield = {0, 60, 150, 60, 40, 50, 40, 40, 100, -1}
                Alnfield = {"C", "C", "N", "N", "C", "F", "C", "C", "N", "N"}
                Retfield = {"recid", "Code", "Name", "Country", "Curr", "Disc", "Active", "Local", "VAT Type", "Linked Type"}
            Else
                Sizefield = {0, 60, 150, 60, 40, 50, 40, 40, 100}
                Alnfield = {"C", "C", "N", "N", "C", "F", "C", "C", "N"}
                Retfield = {"recid", "Code", "Name", "Country", "Curr", "Disc", "Active", "Local", "VAT Type"}
            End If
            If Active = True Then Sizefield(1) = 100 : Sizefield(6) = 0
            If LocForg = False Then Sizefield(7) = 0
            If VATType = False Then Sizefield(2) = 250 : Sizefield(8) = 0
            frm = New frmSearch
            With frm
                .Owner = FrmOwner
                If .ShowDialog("Return", dsSql.Tables("tbcode"), dsSql.Tables("tbcode").Rows.Count,
                    "Supplier List", Sizefield, Alnfield, Retfield, 0, Nothing, False, CShort(IIf(SupplierType, 700, 600)), 500, False, True,
                    , , , , , FilterInfo, , , , , , , , , , , , , , , , , , , ColumnPresentation) = DialogResult.OK Then
                    If gbRetSearch(0) <> "" Then
                        TxtSupp.Text = gbRetSearch(1)
                        TxtSuppDesc.Text = gbRetSearch(2)
                    End If
                    TxtSupp.Focus()
                    If TAB = True Then SendKeys.Send("{TAB}")
                Else
                    TxtSupp.Focus()
                End If
            End With
        Catch exp As Exception
            Throw exp
        Finally
            gbRetSearch = Nothing
            dsSql = Nothing : daSql = Nothing
            If Not cnSql Is Nothing Then
                If cnSql.State = ConnectionState.Open Then cnSql.Close()
                cnSql = Nothing
            End If
            frm = Nothing
            Cursor.Current = Cursors.Default
        End Try
    End Sub
    Public Sub ColorsList(ByVal TxtColor As TextBox, ByVal TxtColorDesc As Label, ByVal FrmOwner As Form, Optional ByVal Cond As String = "", Optional ByVal Active As Boolean = True)
        Dim cnSql As SqlConnection
        Dim dsSql As DataSet
        Dim daSql As SqlDataAdapter
        Dim frm As frmSearch

        Dim FilterInfo(0, 1) As String
        Dim Sizefield() As Int16 = {60, 300, 40}
        Dim Alnfield() As String = {"C", "N", "C"}
        Dim Retfield() As String = {"Code", "description", "Active"}
        Dim ColumnPresentation(,) As Object = {{"Active", PresentationEnum.CheckBox}}
        FilterInfo(0, 0) = "Active" : FilterInfo(0, 1) = "True"
        If Active = True Then Sizefield(0) = 100 : Sizefield(6) = 0

        Try
            Cursor.Current = Cursors.WaitCursor
            cnSql = New SqlConnection(ConnectionString)
            cnSql.Open()

            daSql = New SqlDataAdapter(ColorsStrsql(Cond, Active), cnSql)
            dsSql = New DataSet
            daSql.Fill(dsSql, "tbcode")
            cnSql.Close()

            frm = New frmSearch

            With frm
                .Owner = FrmOwner
                If .ShowDialog("Return", dsSql.Tables("tbcode"), dsSql.Tables("tbcode").Rows.Count,
                "Colors List", Sizefield, Alnfield, Retfield, 0, Nothing, False, 450, 500, , True,
                    , , , , , FilterInfo, , , , , , , , , , , , , , , , , , , ColumnPresentation) = DialogResult.OK Then
                    If gbRetSearch(0) <> "" Then
                        TxtColor.Text = gbRetSearch(0)
                        TxtColorDesc.Text = gbRetSearch(1)
                    End If
                    TxtColor.Focus()
                    SendKeys.Send("{TAB}")
                Else
                    TxtColor.Focus()
                End If
            End With

        Catch exp As Exception
            Throw exp
        Finally
            gbRetSearch = Nothing
            dsSql = Nothing : daSql = Nothing
            If Not cnSql Is Nothing Then
                If cnSql.State = ConnectionState.Open Then cnSql.Close()
                cnSql = Nothing
            End If
            frm = Nothing
            Cursor.Current = Cursors.Default
        End Try
    End Sub

    Public Sub SuppCategoryList(ByVal TxtCateg As TextBox, ByVal TxtCategDesc As Label, ByVal FrmOwner As Form, ByVal TITLE As String)
        Dim cnSql As SqlConnection
        Dim dsSql As DataSet
        Dim daSql As SqlDataAdapter
        Dim frm As frmSearch
        Dim Sizefield() As Int16 = {60, 300}
        Dim Alnfield() As String = {"C", "N"}
        Dim Retfield() As String = {"Code", "Description"}


        Try

            Cursor.Current = Cursors.WaitCursor

            cnSql = New SqlConnection(ConnectionString)
            cnSql.Open()

            daSql = New SqlDataAdapter(SuppCategoryStrsql, cnSql)

            dsSql = New DataSet

            daSql.Fill(dsSql, "tbcode")

            cnSql.Close()

            frm = New frmSearch

            With frm
                .Owner = FrmOwner
                If .ShowDialog("Return", dsSql.Tables("tbcode"), dsSql.Tables("tbcode").Rows.Count,
                TITLE, Sizefield, Alnfield, Retfield, 0, Nothing, False, 410, 500, , True) = DialogResult.OK Then
                    If gbRetSearch(0) <> "" Then
                        TxtCateg.Text = gbRetSearch(0)
                        TxtCategDesc.Text = gbRetSearch(1)
                    End If
                    TxtCateg.Focus()
                    SendKeys.Send("{TAB}")
                Else
                    TxtCateg.Focus()
                End If
            End With

        Catch exp As Exception
            Throw exp
        Finally
            gbRetSearch = Nothing
            dsSql = Nothing : daSql = Nothing
            If Not cnSql Is Nothing Then
                If cnSql.State = ConnectionState.Open Then cnSql.Close()
                cnSql = Nothing
            End If
            frm = Nothing
            Cursor.Current = Cursors.Default
        End Try
    End Sub
    Public Sub CategoryList(ByVal TxtCateg As TextBox, ByVal TxtCategDesc As Label, ByVal FrmOwner As Form, Optional ByVal Cond As String = "", Optional ByVal TAB As Boolean = True, Optional ByVal Active As Boolean = True)
        Dim cnSql As SqlConnection
        Dim dsSql As DataSet
        Dim daSql As SqlDataAdapter
        Dim frm As frmSearch

        Dim FilterInfo(0, 1) As String
        Dim Sizefield() As Int16 = {60, 300, 40}
        Dim Alnfield() As String = {"C", "N", "C"}
        Dim Retfield() As String = {"Code", "Description", "Active"}
        Dim ColumnPresentation(,) As Object = {{"Active", PresentationEnum.CheckBox}}
        FilterInfo(0, 0) = "Active" : FilterInfo(0, 1) = "True"
        If Active = True Then Sizefield(0) = 100 : Sizefield(2) = 0

        Try
            Cursor.Current = Cursors.WaitCursor
            cnSql = New SqlConnection(ConnectionString)
            cnSql.Open()
            daSql = New SqlDataAdapter(CategoryStrsql(Cond, Active), cnSql)
            dsSql = New DataSet
            daSql.Fill(dsSql, "tbcode")
            cnSql.Close()
            frm = New frmSearch
            With frm
                .Owner = FrmOwner
                If .ShowDialog("Return", dsSql.Tables("tbcode"), dsSql.Tables("tbcode").Rows.Count,
                "Category List", Sizefield, Alnfield, Retfield, 0, Nothing, False, 450, 500, False, True,
                , , , , , FilterInfo, , , , , , , , , , , , , , , , , , , ColumnPresentation) = DialogResult.OK Then
                    If gbRetSearch(0) <> "" Then
                        TxtCateg.Text = gbRetSearch(0)
                        TxtCategDesc.Text = gbRetSearch(1)
                    End If
                    TxtCateg.Focus()
                    If TAB = True Then SendKeys.Send("{TAB}")
                Else
                    TxtCateg.Focus()
                End If
            End With

        Catch exp As Exception
            Throw exp
        Finally
            gbRetSearch = Nothing
            dsSql = Nothing : daSql = Nothing
            If Not cnSql Is Nothing Then
                If cnSql.State = ConnectionState.Open Then cnSql.Close()
                cnSql = Nothing
            End If
            frm = Nothing
            Cursor.Current = Cursors.Default
        End Try
    End Sub
    Public Sub SeasonsList(ByVal TxtSeason As TextBox, ByVal TxtSeasonDesc As Label, ByVal FrmOwner As Form, Optional ByVal Cond As String = "", Optional ByVal TAB As Boolean = True, Optional ByVal Active As Boolean = True)
        Dim cnSql As SqlConnection
        Dim dsSql As DataSet
        Dim daSql As SqlDataAdapter
        Dim frm As frmSearch

        Dim FilterInfo(0, 1) As String
        Dim Sizefield() As Int16 = {60, 300, 40, 90, 40}
        Dim Alnfield() As String = {"C", "N", "N", "N", "C"}
        Dim Retfield() As String = {"Code", "description", "Year", "Type", "Active"}
        Dim ColumnPresentation(,) As Object = {{"Active", PresentationEnum.CheckBox}}
        FilterInfo(0, 0) = "Active" : FilterInfo(0, 1) = "True"
        If Active = True Then Sizefield(0) = 100 : Sizefield(4) = 0
        Try
            Cursor.Current = Cursors.WaitCursor

            cnSql = New SqlConnection(ConnectionString)
            cnSql.Open()
            daSql = New SqlDataAdapter(SeasonStrsql(Cond, Active), cnSql)
            dsSql = New DataSet
            daSql.Fill(dsSql, "tbcode")
            cnSql.Close()
            frm = New frmSearch
            With frm
                .Owner = FrmOwner
                If .ShowDialog("Return", dsSql.Tables("tbcode"), dsSql.Tables("tbcode").Rows.Count,
                "Seasons List", Sizefield, Alnfield, Retfield, 0, Nothing, False, 580, 500, False, True,
                , , , , , FilterInfo, , , , , , , , , , , , , , , , , , , ColumnPresentation) = DialogResult.OK Then
                    If gbRetSearch(0) <> "" Then
                        TxtSeason.Text = gbRetSearch(0)
                        TxtSeasonDesc.Text = gbRetSearch(1)
                    End If
                    TxtSeason.Focus()
                    If TAB = True Then SendKeys.Send("{TAB}")
                Else
                    TxtSeason.Focus()
                End If
            End With
        Catch exp As Exception
            Throw exp
        Finally
            gbRetSearch = Nothing
            dsSql = Nothing : daSql = Nothing
            If Not cnSql Is Nothing Then
                If cnSql.State = ConnectionState.Open Then cnSql.Close()
                cnSql = Nothing
            End If
            frm = Nothing
            Cursor.Current = Cursors.Default
        End Try
    End Sub
    Public Sub BrandList(ByVal TxtBrand As TextBox, ByVal TxtBrandDesc As Label, ByVal FrmOwner As Form, Optional ByVal Cond As String = "", Optional ByVal TAB As Boolean = True, Optional ByVal Active As Boolean = True)
        Dim cnSql As SqlConnection
        Dim dsSql As DataSet
        Dim daSql As SqlDataAdapter
        Dim frm As frmSearch

        Dim FilterInfo(0, 1) As String
        Dim Sizefield() As Int16 = {60, 300, 40}
        Dim Alnfield() As String = {"C", "N", "C"}
        Dim Retfield() As String = {"Code", "description", "Active"}
        Dim ColumnPresentation(,) As Object = {{"Active", PresentationEnum.CheckBox}}
        FilterInfo(0, 0) = "Active" : FilterInfo(0, 1) = "True"
        If Active = True Then Sizefield(0) = 100 : Sizefield(2) = 0

        Try
            Cursor.Current = Cursors.WaitCursor
            cnSql = New SqlConnection(ConnectionString)
            cnSql.Open()
            daSql = New SqlDataAdapter(BrandStrsql(Cond, Active), cnSql)
            dsSql = New DataSet
            daSql.Fill(dsSql, "tbcode")
            cnSql.Close()
            frm = New frmSearch
            With frm
                .Owner = FrmOwner
                If .ShowDialog("Return", dsSql.Tables("tbcode"), dsSql.Tables("tbcode").Rows.Count,
                "Brands List", Sizefield, Alnfield, Retfield, 0, Nothing, False, 450, 500, False, True,
                , , , , , FilterInfo, , , , , , , , , , , , , , , , , , , ColumnPresentation) = DialogResult.OK Then
                    If gbRetSearch(0) <> "" Then
                        TxtBrand.Text = gbRetSearch(0)
                        TxtBrandDesc.Text = gbRetSearch(1)
                    End If
                    TxtBrand.Focus()
                    If TAB = True Then SendKeys.Send("{TAB}")
                Else
                    TxtBrand.Focus()
                End If
            End With

        Catch exp As Exception
            Throw exp
        Finally
            gbRetSearch = Nothing
            dsSql = Nothing : daSql = Nothing
            If Not cnSql Is Nothing Then
                If cnSql.State = ConnectionState.Open Then cnSql.Close()
                cnSql = Nothing
            End If
            frm = Nothing
            Cursor.Current = Cursors.Default
        End Try
    End Sub
    Public Sub ColorList(ByVal txtColor As TextBox, ByVal txtColorDesc As Label, ByVal FrmOwner As Form, Optional ByVal Cond As String = "", Optional ByVal TAB As Boolean = True, Optional ByVal Active As Boolean = True)
        Dim cnSql As SqlConnection
        Dim dsSql As DataSet
        Dim daSql As SqlDataAdapter
        Dim frm As frmSearch

        Dim FilterInfo(0, 1) As String
        Dim Sizefield() As Int16 = {60, 300, 40}
        Dim Alnfield() As String = {"C", "N", "C"}
        Dim Retfield() As String = {"Code", "description", "Active"}
        Dim ColumnPresentation(,) As Object = {{"Active", PresentationEnum.CheckBox}}
        FilterInfo(0, 0) = "Active" : FilterInfo(0, 1) = "True"
        If Active = True Then Sizefield(0) = 100 : Sizefield(2) = 0

        Try
            Cursor.Current = Cursors.WaitCursor
            cnSql = New SqlConnection(ConnectionString)
            cnSql.Open()
            daSql = New SqlDataAdapter(ColorStrsql(Cond, Active), cnSql)
            dsSql = New DataSet
            daSql.Fill(dsSql, "tbcode")
            cnSql.Close()
            frm = New frmSearch
            With frm
                .Owner = FrmOwner
                If .ShowDialog("Return", dsSql.Tables("tbcode"), dsSql.Tables("tbcode").Rows.Count,
                "Color List", Sizefield, Alnfield, Retfield, 0, Nothing, False, 450, 500, False, True,
                , , , , , FilterInfo, , , , , , , , , , , , , , , , , , , ColumnPresentation) = DialogResult.OK Then
                    If gbRetSearch(0) <> "" Then
                        txtColor.Text = gbRetSearch(0)
                        txtColorDesc.Text = gbRetSearch(1)
                    End If
                    txtColor.Focus()
                    If TAB = True Then SendKeys.Send("{TAB}")
                Else
                    txtColor.Focus()
                End If
            End With
        Catch exp As Exception
            Throw exp
        Finally
            gbRetSearch = Nothing
            dsSql = Nothing : daSql = Nothing
            If Not cnSql Is Nothing Then
                If cnSql.State = ConnectionState.Open Then cnSql.Close()
                cnSql = Nothing
            End If
            frm = Nothing
            Cursor.Current = Cursors.Default
        End Try
    End Sub
    Public Sub LocationList(ByVal txtLoc As TextBox, ByVal TxtLocDesc As Label, ByVal FrmOwner As Form, Optional ByVal cond As String = "", Optional ByVal Active As Boolean = True, Optional ByVal TAB As Boolean = True)
        Dim cnSql As SqlConnection
        Dim dsSql As DataSet
        Dim daSql As SqlDataAdapter
        Dim frm As frmSearch

        Dim FilterInfo(0, 1) As String
        Dim Sizefield() As Int16 = {60, 300, 40}
        Dim Alnfield() As String = {"C", "N", "C"}
        Dim Retfield() As String = {"Code", "description", "Active"}
        Dim ColumnPresentation(,) As Object = {{"Active", PresentationEnum.CheckBox}}
        FilterInfo(0, 0) = "Active" : FilterInfo(0, 1) = "True"
        If Active = True Then Sizefield(0) = 100 : Sizefield(2) = 0

        Try
            Cursor.Current = Cursors.WaitCursor
            cnSql = New SqlConnection(ConnectionString)
            cnSql.Open()
            daSql = New SqlDataAdapter(LocationStrsql(cond, Active), cnSql)
            dsSql = New DataSet
            daSql.Fill(dsSql, "tbcode")
            cnSql.Close()
            frm = New frmSearch
            With frm
                .Owner = FrmOwner
                If .ShowDialog("Return", dsSql.Tables("tbcode"), dsSql.Tables("tbcode").Rows.Count,
                "Location List", Sizefield, Alnfield, Retfield, 0, Nothing, False, 450, 500, , True,
                    , , , , , FilterInfo, , , , , , , , , , , , , , , , , , , ColumnPresentation) = DialogResult.OK Then
                    If gbRetSearch(0) <> "" Then
                        txtLoc.Text = gbRetSearch(0)
                        TxtLocDesc.Text = gbRetSearch(1)
                    End If
                    txtLoc.Focus()
                    If TAB = True Then SendKeys.Send("{TAB}")
                Else
                    txtLoc.Focus()
                End If
            End With

        Catch exp As Exception
            Throw exp
        Finally
            gbRetSearch = Nothing
            dsSql = Nothing : daSql = Nothing
            If Not cnSql Is Nothing Then
                If cnSql.State = ConnectionState.Open Then cnSql.Close()
                cnSql = Nothing
            End If
            frm = Nothing
            Cursor.Current = Cursors.Default
        End Try
    End Sub
    Public Sub DepartmentList(ByVal TxtDepartment As TextBox, ByVal TxtDepartmentDesc As Label, ByVal FrmOwner As Form, Optional ByVal Cond As String = "", Optional ByVal TAB As Boolean = True, Optional ByVal Active As Boolean = True)
        Dim cnSql As SqlConnection
        Dim dsSql As DataSet
        Dim daSql As SqlDataAdapter
        Dim frm As frmSearch

        Dim FilterInfo(0, 1) As String
        Dim Sizefield() As Int16 = {60, 300, 40}
        Dim Alnfield() As String = {"C", "N", "C"}
        Dim Retfield() As String = {"Code", "description", "Active"}
        Dim ColumnPresentation(,) As Object = {{"Active", PresentationEnum.CheckBox}}
        FilterInfo(0, 0) = "Active" : FilterInfo(0, 1) = "True"
        If Active = True Then Sizefield(0) = 100 : Sizefield(2) = 0

        Try
            Cursor.Current = Cursors.WaitCursor
            cnSql = New SqlConnection(ConnectionString)
            cnSql.Open()
            daSql = New SqlDataAdapter(DepartmentStrsql(Cond, Active), cnSql)
            dsSql = New DataSet
            daSql.Fill(dsSql, "tbcode")
            cnSql.Close()
            frm = New frmSearch
            With frm
                .Owner = FrmOwner
                If .ShowDialog("Return", dsSql.Tables("tbcode"), dsSql.Tables("tbcode").Rows.Count,
                "Department List", Sizefield, Alnfield, Retfield, 0, Nothing, False, 450, 500, False, True,
                , , , , , FilterInfo, , , , , , , , , , , , , , , , , , , ColumnPresentation) = DialogResult.OK Then
                    If gbRetSearch(0) <> "" Then
                        TxtDepartment.Text = gbRetSearch(0)
                        TxtDepartmentDesc.Text = gbRetSearch(1)
                    End If
                    TxtDepartment.Focus()
                    If TAB = True Then SendKeys.Send("{TAB}")
                Else
                    TxtDepartment.Focus()
                End If
            End With
        Catch exp As Exception
            Throw exp
        Finally
            gbRetSearch = Nothing
            dsSql = Nothing : daSql = Nothing
            If Not cnSql Is Nothing Then
                If cnSql.State = ConnectionState.Open Then cnSql.Close()
                cnSql = Nothing
            End If
            frm = Nothing
            Cursor.Current = Cursors.Default
        End Try
    End Sub
    'Public Sub ItmGenList(ByVal TxtField AS TextBox, ByVal TxtFieldDesc AS Label, ByVal FrmOwner AS Form, ByVal Field AS String, Optional ByVal Cond AS String = "", Optional ByVal TAB AS Boolean = True, Optional ByVal Active AS Boolean = True)
    '    '''Field = "G" (GENDer) OR "IT" (Item Type)
    '    Dim cnSql AS SqlConnection
    '    Dim dsSql AS DataSet
    '    Dim daSql AS SqlDataAdapter
    '    Dim frm AS frmSearch

    '    Dim FilterInfo(0, 1) AS String
    '    Dim Sizefield() AS Int16 = {60, 300, 40}
    '    Dim Alnfield() AS String = {"C", "N", "C"}
    '    Dim Retfield() AS String = {"Code", "description", "Active"}
    '    Dim ColumnPresentation(,) AS Object = {{"Active", PresentationEnum.CheckBox}}
    '    FilterInfo(0, 0) = "Active" : FilterInfo(0, 1) = "True"
    '    If Active = True THEN Sizefield(0) = 100 : Sizefield(2) = 0

    '    Try
    '        Cursor.Current = Cursors.WaitCursor
    '        cnSql = New SqlConnection(ConnectionString)
    '        cnSql.Open()
    '        daSql = New SqlDataAdapter(ItmGenStrsql(Field, Cond, Active), cnSql)
    '        dsSql = New DataSet
    '        daSql.Fill(dsSql, "tbcode")
    '        cnSql.Close()
    '        frm = New frmSearch
    '        With frm
    '            .Owner = FrmOwner
    '            If .ShowDialog("Return", dsSql.Tables("tbcode"), dsSql.Tables("tbcode").Rows.Count, _
    '                IIf(Field = "G", "GENDer List", "Item Type List").ToString, Sizefield, Alnfield, _
    '                Retfield, 0, Nothing, False, 450, 500, False, True, , , , , , FilterInfo, , _
    '                , , , , , , , , , , , , , , , , , ColumnPresentation) = DialogResult.OK THEN
    '                If gbRetSearch(0) <> "" THEN
    '                    TxtField.Text = gbRetSearch(0)
    '                    TxtFieldDesc.Text = gbRetSearch(1)
    '                END If
    '                TxtField.Focus()
    '                If TAB = True THEN SENDKeys.SEND("{TAB}")
    '            ELSE
    '                TxtField.Focus()
    '            END If
    '        END With
    '    Catch exp AS Exception
    '        Throw exp
    '    Finally
    '        gbRetSearch = Nothing
    '        dsSql = Nothing : daSql = Nothing
    '        If Not cnSql Is Nothing THEN
    '            If cnSql.State = ConnectionState.Open THEN cnSql.Close()
    '            cnSql = Nothing
    '        END If
    '        frm = Nothing
    '        Cursor.Current = Cursors.Default
    '    END Try
    'END Sub
    Public Sub GenderList(ByVal TxtGender As TextBox, ByVal TxtGenderDesc As Label, ByVal FrmOwner As Form, Optional ByVal Cond As String = "", Optional ByVal TAB As Boolean = True, Optional ByVal Active As Boolean = True, Optional ByVal Match As Boolean = False, Optional ByVal Gender As String = "")
        Dim cnSql As SqlConnection
        Dim dsSql As DataSet
        Dim daSql As SqlDataAdapter
        Dim frm As frmSearch

        Dim FilterInfo(1, 1) As String
        Dim Sizefield() As Int16 = {60, 300, 40, 0}
        Dim Alnfield() As String = {"C", "N", "C", "C"}
        Dim Retfield() As String = {"Code", "description", "Active", "Match"}
        Dim ColumnPresentation(,) As Object = {{"Active", PresentationEnum.CheckBox}, {"Match", PresentationEnum.CheckBox}}
        FilterInfo(0, 0) = "Active" : FilterInfo(0, 1) = "True"
        FilterInfo(1, 0) = "Match" : FilterInfo(1, 1) = "True"
        If Active = True Then Sizefield(0) = 100 : Sizefield(2) = 0
        If Match = True Then Sizefield(3) = 40

        Try
            Cursor.Current = Cursors.WaitCursor
            cnSql = New SqlConnection(ConnectionString)
            cnSql.Open()
            daSql = New SqlDataAdapter(GenderStrsql(Cond, Active, Gender), cnSql)
            dsSql = New DataSet
            daSql.Fill(dsSql, "tbcode")
            cnSql.Close()
            frm = New frmSearch
            With frm
                .Owner = FrmOwner
                If .ShowDialog("Return", dsSql.Tables("tbcode"), dsSql.Tables("tbcode").Rows.Count,
                "Gender List", Sizefield, Alnfield, Retfield, 0, Nothing, False, 500, 500, False, True,
                , , , , , FilterInfo, , , , , , , , , , , , , , , , , , , ColumnPresentation) = DialogResult.OK Then
                    If gbRetSearch(0) <> "" Then
                        TxtGender.Text = gbRetSearch(0)
                        TxtGenderDesc.Text = gbRetSearch(1)
                    End If
                    TxtGender.Focus()
                    If TAB = True Then SendKeys.Send("{TAB}")
                Else
                    TxtGender.Focus()
                End If
            End With
        Catch exp As Exception
            Throw exp
        Finally
            gbRetSearch = Nothing
            dsSql = Nothing : daSql = Nothing
            If Not cnSql Is Nothing Then
                If cnSql.State = ConnectionState.Open Then cnSql.Close()
                cnSql = Nothing
            End If
            frm = Nothing
            Cursor.Current = Cursors.Default
        End Try
    End Sub

    Public Sub CurrencyList(ByVal TxtCurr As TextBox, ByVal TxtCurrDesc As Label, ByVal FrmOwner As Form, Optional ByVal Cond As String = "", Optional ByVal TAB As Boolean = True)
        Dim cnSql As SqlConnection
        Dim dsSql As DataSet
        Dim daSql As SqlDataAdapter
        Dim frm As frmSearch
        Dim Sizefield() As Int16 = {60, 300}
        Dim Alnfield() As String = {"C", "N"}
        Dim Retfield() As String = {"Code", "description"}

        Try
            Cursor.Current = Cursors.WaitCursor

            cnSql = New SqlConnection(ConnectionString)
            cnSql.Open()

            daSql = New SqlDataAdapter(CurrencyStrsql(Cond), cnSql)

            dsSql = New DataSet

            daSql.Fill(dsSql, "tbcode")

            cnSql.Close()

            frm = New frmSearch

            With frm
                .Owner = FrmOwner
                If .ShowDialog("Return", dsSql.Tables("tbcode"), dsSql.Tables("tbcode").Rows.Count,
                "Currency List", Sizefield, Alnfield, Retfield, 0, Nothing, False, 410, 500, , True) = DialogResult.OK Then
                    If gbRetSearch(0) <> "" Then
                        TxtCurr.Text = gbRetSearch(0)
                        TxtCurrDesc.Text = gbRetSearch(1)
                    End If
                    TxtCurr.Focus()
                    If TAB = True Then SendKeys.Send("{TAB}")
                Else
                    TxtCurr.Focus()
                End If
            End With

        Catch exp As Exception
            Throw exp
        Finally
            gbRetSearch = Nothing
            dsSql = Nothing : daSql = Nothing
            If Not cnSql Is Nothing Then
                If cnSql.State = ConnectionState.Open Then cnSql.Close()
                cnSql = Nothing
            End If
            frm = Nothing
            Cursor.Current = Cursors.Default
        End Try
    End Sub
    Public Sub OverheadList(ByVal TxtOverhead As TextBox, ByVal TxtOverheadDesc As Label, ByVal FrmOwner As Form, Optional ByVal Cond As String = "", Optional ByVal TAB As Boolean = True)
        Dim cnSql As SqlConnection
        Dim dsSql As DataSet
        Dim daSql As SqlDataAdapter
        Dim frm As frmSearch
        Dim Sizefield() As Int16 = {60, 300}
        Dim Alnfield() As String = {"C", "N"}
        Dim Retfield() As String = {"Code", "description"}

        Try
            Cursor.Current = Cursors.WaitCursor
            cnSql = New SqlConnection(ConnectionString)
            cnSql.Open()
            daSql = New SqlDataAdapter(OverheadStrsql(Cond), cnSql)
            dsSql = New DataSet
            daSql.Fill(dsSql, "tbcode")
            cnSql.Close()
            frm = New frmSearch
            With frm
                .Owner = FrmOwner
                If .ShowDialog("Return", dsSql.Tables("tbcode"), dsSql.Tables("tbcode").Rows.Count, "Overhead List",
                    Sizefield, Alnfield, Retfield, 0, Nothing, False, 410, 500, , True) = DialogResult.OK Then
                    If gbRetSearch(0) <> "" Then
                        TxtOverhead.Text = gbRetSearch(0)
                        TxtOverheadDesc.Text = gbRetSearch(1)
                    End If
                    TxtOverhead.Focus()
                    If TAB = True Then SendKeys.Send("{TAB}")
                Else
                    TxtOverhead.Focus()
                End If
            End With
        Catch exp As Exception
            Throw exp
        Finally
            gbRetSearch = Nothing
            dsSql = Nothing : daSql = Nothing
            If Not cnSql Is Nothing Then
                If cnSql.State = ConnectionState.Open Then cnSql.Close()
                cnSql = Nothing
            End If
            frm = Nothing
            Cursor.Current = Cursors.Default
        End Try
    End Sub
    Public Sub UnitList(ByVal TxtUnit As TextBox, ByVal TxtUnitDesc As Label, ByVal FrmOwner As Form)
        Dim cnSql As SqlConnection
        Dim dsSql As DataSet
        Dim daSql As SqlDataAdapter
        Dim frm As frmSearch
        Dim Sizefield() As Int16 = {80, 50, 250}
        Dim Alnfield() As String = {"C", "N", "N"}
        Dim Retfield() As String = {"Code", "Type", "description"}

        Try
            Cursor.Current = Cursors.WaitCursor

            cnSql = New SqlConnection(ConnectionString)
            cnSql.Open()

            daSql = New SqlDataAdapter(UnitStrsql, cnSql)

            dsSql = New DataSet

            daSql.Fill(dsSql, "tbcode")

            cnSql.Close()

            frm = New frmSearch

            With frm
                .Owner = FrmOwner
                If .ShowDialog("Return", dsSql.Tables("tbcode"), dsSql.Tables("tbcode").Rows.Count,
                "Units List", Sizefield, Alnfield, Retfield, 0, Nothing, False, 450, 500, , True) = DialogResult.OK Then
                    If gbRetSearch(0) <> "" Then
                        TxtUnit.Text = gbRetSearch(0)
                        TxtUnitDesc.Text = gbRetSearch(1)
                    End If
                    TxtUnit.Focus()
                    SendKeys.Send("{TAB}")
                Else
                    TxtUnit.Focus()
                End If
            End With

        Catch exp As Exception
            Throw exp
        Finally
            gbRetSearch = Nothing
            dsSql = Nothing : daSql = Nothing
            If Not cnSql Is Nothing Then
                If cnSql.State = ConnectionState.Open Then cnSql.Close()
                cnSql = Nothing
            End If
            frm = Nothing
            Cursor.Current = Cursors.Default
        End Try
    End Sub

    Public Sub ItmTypeList(ByVal TxtItmType As TextBox, ByVal TxtItmTypeDesc As Label, ByVal FrmOwner As Form, Optional ByVal Cond As String = "", Optional ByVal Active As Boolean = True)
        Dim cnSql As SqlConnection
        Dim dsSql As DataSet
        Dim daSql As SqlDataAdapter
        Dim frm As frmSearch

        Dim FilterInfo(0, 1) As String
        Dim Sizefield() As Int16 = {60, 300, 40}
        Dim Alnfield() As String = {"C", "N", "C"}
        Dim Retfield() As String = {"Code", "description", "Active"}
        Dim ColumnPresentation(,) As Object = {{"Active", PresentationEnum.CheckBox}}
        FilterInfo(0, 0) = "Active" : FilterInfo(0, 1) = "True"
        If Active = True Then Sizefield(0) = 100 : Sizefield(2) = 0

        Try
            Cursor.Current = Cursors.WaitCursor
            cnSql = New SqlConnection(ConnectionString)
            cnSql.Open()
            daSql = New SqlDataAdapter(ItmTypeStrsql(Cond, Active), cnSql)
            dsSql = New DataSet
            daSql.Fill(dsSql, "tbcode")
            cnSql.Close()
            frm = New frmSearch
            With frm
                .Owner = FrmOwner
                If .ShowDialog("Return", dsSql.Tables("tbcode"), dsSql.Tables("tbcode").Rows.Count,
                "Product Group List", Sizefield, Alnfield, Retfield, 0, Nothing, False, 410, 500, , True,
                , , , , , FilterInfo, , , , , , , , , , , , , , , , , , , ColumnPresentation) = DialogResult.OK Then
                    If gbRetSearch(0) <> "" Then
                        TxtItmType.Text = gbRetSearch(0)
                        TxtItmTypeDesc.Text = gbRetSearch(1)
                    End If
                    TxtItmType.Focus()
                    SendKeys.Send("{TAB}")
                Else
                    TxtItmType.Focus()
                End If
            End With

        Catch exp As Exception
            Throw exp
        Finally
            gbRetSearch = Nothing
            dsSql = Nothing : daSql = Nothing
            If Not cnSql Is Nothing Then
                If cnSql.State = ConnectionState.Open Then cnSql.Close()
                cnSql = Nothing
            End If
            frm = Nothing
            Cursor.Current = Cursors.Default
        End Try
    End Sub
    'Added by Abir T. @ 10.06.2013
    Public Sub ItmTypeGroupList(ByVal TxtItmType As TextBox, ByVal TxtItmTypeDesc As Label, ByVal FrmOwner As Form, ByVal TxtGroup As TextBox,
                            ByVal TxtGroupDesc As Label, Optional ByVal cond As String = "")
        Dim cnSql As SqlConnection
        Dim dsSql As DataSet
        Dim daSql As SqlDataAdapter
        Dim frm As frmSearch
        Dim Sizefield() As Int16 = {75, 200, 75, 200}
        Dim Alnfield() As String = {"C", "N", "C", "N"}
        Dim Retfield() As String = {"Code", "description", "Dep. Code", "Department Desc."}

        Try
            Cursor.Current = Cursors.WaitCursor
            cnSql = New SqlConnection(ConnectionString)
            cnSql.Open()
            daSql = New SqlDataAdapter(ItmTypeGroupStrsql(cond), cnSql)
            dsSql = New DataSet
            daSql.Fill(dsSql, "tbcode")
            cnSql.Close()
            frm = New frmSearch
            With frm
                .Owner = FrmOwner
                If .ShowDialog("Return", dsSql.Tables("tbcode"), dsSql.Tables("tbcode").Rows.Count,
                "Product Group List", Sizefield, Alnfield, Retfield, 0, Nothing, False, 600, 500, , True) = DialogResult.OK Then
                    If gbRetSearch(0) <> "" Then
                        TxtItmType.Text = gbRetSearch(0)
                        TxtItmTypeDesc.Text = gbRetSearch(1)
                        TxtGroup.Text = gbRetSearch(2)
                        TxtGroupDesc.Text = gbRetSearch(3)
                    End If
                    TxtItmType.Focus()
                    SendKeys.Send("{TAB}")
                Else
                    TxtItmType.Focus()
                End If
            End With
        Catch exp As Exception
            Throw exp
        Finally
            gbRetSearch = Nothing
            dsSql = Nothing : daSql = Nothing
            If Not cnSql Is Nothing Then
                If cnSql.State = ConnectionState.Open Then cnSql.Close()
                cnSql = Nothing
            End If
            frm = Nothing
            Cursor.Current = Cursors.Default
        End Try
    End Sub

    Public Sub StyleList(ByVal TxtStyle As TextBox, ByVal TxtStyleDesc As Label, ByVal FrmOwner As Form, Optional ByVal Cond As String = "", Optional ByVal TAB As Boolean = True, Optional ByVal Active As Boolean = True)
        Dim cnSql As SqlConnection
        Dim dsSql As DataSet
        Dim daSql As SqlDataAdapter
        Dim frm As frmSearch

        Dim FilterInfo(0, 1) As String
        Dim Sizefield() As Int16 = {60, 300, 40}
        Dim Alnfield() As String = {"C", "N", "C"}
        Dim Retfield() As String = {"Code", "description", "Active"}
        Dim ColumnPresentation(,) As Object = {{"Active", PresentationEnum.CheckBox}}
        FilterInfo(0, 0) = "Active" : FilterInfo(0, 1) = "True"
        If Active = True Then Sizefield(0) = 100 : Sizefield(2) = 0

        Try
            Cursor.Current = Cursors.WaitCursor
            cnSql = New SqlConnection(ConnectionString)
            cnSql.Open()
            daSql = New SqlDataAdapter(StyleStrsql(Cond, Active), cnSql)
            dsSql = New DataSet
            daSql.Fill(dsSql, "tbcode")
            cnSql.Close()
            frm = New frmSearch
            With frm
                .Owner = FrmOwner
                If .ShowDialog("Return", dsSql.Tables("tbcode"), dsSql.Tables("tbcode").Rows.Count,
                "Style List", Sizefield, Alnfield, Retfield, 0, Nothing, False, 450, 500, False, True,
                , , , , , FilterInfo, , , , , , , , , , , , , , , , , , , ColumnPresentation) = DialogResult.OK Then
                    If gbRetSearch(0) <> "" Then
                        TxtStyle.Text = gbRetSearch(0)
                        TxtStyleDesc.Text = gbRetSearch(1)
                    End If
                    TxtStyle.Focus()
                    If TAB = True Then SendKeys.Send("{TAB}")
                Else
                    TxtStyle.Focus()
                End If
            End With
        Catch exp As Exception
            Throw exp
        Finally
            gbRetSearch = Nothing
            dsSql = Nothing : daSql = Nothing
            If Not cnSql Is Nothing Then
                If cnSql.State = ConnectionState.Open Then cnSql.Close()
                cnSql = Nothing
            End If
            frm = Nothing
            Cursor.Current = Cursors.Default
        End Try
    End Sub
    'Added by Abir T. @ 10.06.2013
    Public Sub StyleProdGrpDepList(ByVal TxtStyle As TextBox, ByVal TxtStyleDesc As Label, ByVal FrmOwner As Form, ByVal TxtItmType As TextBox,
            ByVal TxtItmTypeDesc As Label, ByVal TxtGroup As TextBox, ByVal TxtGroupDesc As Label, Optional ByVal Cond As String = "",
            Optional ByVal TAB As Boolean = True, Optional ByVal Active As Boolean = True)
        Dim cnSql As SqlConnection
        Dim dsSql As DataSet
        Dim daSql As SqlDataAdapter
        Dim frm As frmSearch

        Dim FilterInfo(0, 1) As String
        Dim Sizefield() As Int16 = {60, 200, 60, 200, 0, 0, 40}
        Dim Alnfield() As String = {"C", "N", "C", "N", "C", "N", "C"}
        Dim Retfield() As String = {"Code", "description", "Product Grp. Code", "Product Group Description",
                                    "Department Code", "Department Description", "Active"}
        Dim ColumnPresentation(,) As Object = {{"Active", PresentationEnum.CheckBox}}
        FilterInfo(0, 0) = "Active" : FilterInfo(0, 1) = "True"
        If Active = True Then Sizefield(0) = 100 : Sizefield(6) = 0

        Try
            Cursor.Current = Cursors.WaitCursor
            cnSql = New SqlConnection(ConnectionString)
            cnSql.Open()
            daSql = New SqlDataAdapter(StyleProdGrpDepStrsql(Cond, Active), cnSql)
            dsSql = New DataSet
            daSql.Fill(dsSql, "tbcode")
            cnSql.Close()
            frm = New frmSearch
            With frm
                .Owner = FrmOwner
                If .ShowDialog("Return", dsSql.Tables("tbcode"), dsSql.Tables("tbcode").Rows.Count,
                    "Style List", Sizefield, Alnfield, Retfield, 0, Nothing, False, 610, 500, False, True,
                    , , , , , FilterInfo, , , , , , , , , , , , , , , , , , , ColumnPresentation) = DialogResult.OK Then
                    If gbRetSearch(0) <> "" Then
                        TxtStyle.Text = gbRetSearch(0)
                        TxtStyleDesc.Text = gbRetSearch(1)
                        TxtItmType.Text = gbRetSearch(2)
                        TxtItmTypeDesc.Text = gbRetSearch(3)
                        TxtGroup.Text = gbRetSearch(4)
                        TxtGroupDesc.Text = gbRetSearch(5)
                    End If
                    TxtStyle.Focus()
                    If TAB = True Then SendKeys.Send("{TAB}")
                Else
                    TxtStyle.Focus()
                End If
            End With

        Catch exp As Exception
            Throw exp
        Finally
            gbRetSearch = Nothing
            dsSql = Nothing : daSql = Nothing
            If Not cnSql Is Nothing Then
                If cnSql.State = ConnectionState.Open Then cnSql.Close()
                cnSql = Nothing
            End If
            frm = Nothing
            Cursor.Current = Cursors.Default
        End Try
    End Sub

    Public Sub ItmIdentList(ByVal TxtIdent As TextBox, ByVal TxtIdentDesc As Label, ByVal FrmOwner As Form)
        Dim cnSql As SqlConnection
        Dim dsSql As DataSet
        Dim daSql As SqlDataAdapter
        Dim frm As frmSearch
        Dim Sizefield() As Int16 = {60, 300}
        Dim Alnfield() As String = {"C", "N"}
        Dim Retfield() As String = {"Code", "description"}

        Try
            Cursor.Current = Cursors.WaitCursor
            cnSql = New SqlConnection(ConnectionString)
            cnSql.Open()
            daSql = New SqlDataAdapter(ItmIdentStrsql, cnSql)
            dsSql = New DataSet
            daSql.Fill(dsSql, "tbcode")
            cnSql.Close()
            frm = New frmSearch
            With frm
                .Owner = FrmOwner
                If .ShowDialog("Return", dsSql.Tables("tbcode"), dsSql.Tables("tbcode").Rows.Count,
                "Item Identification List", Sizefield, Alnfield, Retfield, 0, Nothing, False, 410, 500, , True) = DialogResult.OK Then
                    If gbRetSearch(0) <> "" Then
                        TxtIdent.Text = gbRetSearch(0)
                        TxtIdentDesc.Text = gbRetSearch(1)
                    End If
                    TxtIdent.Focus()
                    SendKeys.Send("{TAB}")
                Else
                    TxtIdent.Focus()
                End If
            End With

        Catch exp As Exception
            Throw exp
        Finally
            gbRetSearch = Nothing
            dsSql = Nothing : daSql = Nothing
            If Not cnSql Is Nothing Then
                If cnSql.State = ConnectionState.Open Then cnSql.Close()
                cnSql = Nothing
            End If
            frm = Nothing
            Cursor.Current = Cursors.Default
        End Try
    End Sub

    Public Sub GridSizeList(ByVal TxtGrid As TextBox, ByVal TxtGridDesc As Label, ByVal FrmOwner As Form, Optional ByVal Cond As String = "", Optional ByVal TAB As Boolean = True, Optional ByVal Active As Boolean = True, Optional ByVal Match As Boolean = False, Optional ByVal Size As String = "")
        Dim strsql As String
        Dim MaxCnt, i As Integer
        Dim drSql As SqlDataReader
        Dim cmSql As SqlCommand

        Dim cnSql As SqlConnection
        Dim dsSql As DataSet
        Dim daSql As SqlDataAdapter
        Dim frm As frmSearch

        Dim FilterInfo(1, 1) As String
        Dim Sizefield() As Int16 = {}
        Dim Alnfield() As String = {}
        Dim Retfield() As String = {}

        Try
            Cursor.Current = Cursors.WaitCursor

            cnSql = New SqlConnection(ConnectionString)
            cnSql.Open()

            strsql = "SELECT ISNULL(MAX(GD_Order), 0) FROM STK04_DetGRID INNER JOIN dbo.STK04_GRID ON G_Code=GD_Code"
            If Active = True Then strsql &= " WHERE G_Active='Y'"
            cnSql = New SqlConnection(ConnectionString)
            cnSql.Open()
            cmSql = New SqlCommand(strsql, cnSql)
            drSql = cmSql.ExecuteReader
            drSql.Read()
            MaxCnt = CInt(drSql.Item(0))
            If Not drSql Is Nothing Then
                If drSql.IsClosed = False Then drSql.Close()
                drSql = Nothing
            End If
            cmSql = Nothing

            strsql = "Exec P_GetSizeGrid 'ModList" & FrmOwner.Name & "','" & gbUser & "'"
            cmSql = New SqlCommand(strsql, cnSql)
            cmSql.ExecuteNonQuery()
            cmSql = Nothing

            i = 0
            LoopI = 0

            strsql = "SELECT DISTINCT G_Code AS Code, G_Desc AS Description, G_Active AS Active,"
            'Abir T. @ 20.08.2013
            strsql &= " CASE WHEN GD_SizeDesc = '" & Size & "' THEN 'True'"
            strsql &= " WHEN GD_SizeDesc LIKE '%" & Size & "%' THEN 'True'"
            strsql &= " ELSE 'False' END AS [Match]"
            For i = 1 To MaxCnt
                strsql &= " ,Size" & i
            Next
            strsql &= " FROM TempGridSizeSP_ModList" & FrmOwner.Name & gbUser
            strsql &= " INNER JOIN dbo.stk04_DetGRID ON G_Code = GD_Code"
            strsql &= " WHERE 1 = 1"
            If Active = True Then strsql &= " AND G_Active='True'"
            If Cond <> "" Then strsql &= " AND " & Cond
            i = 0

            ReDim Preserve Sizefield(MaxCnt + 3)
            ReDim Preserve Alnfield(MaxCnt + 3)
            ReDim Preserve Retfield(MaxCnt + 3)

            Sizefield(0) = 50 : Sizefield(1) = 190   'Code & Description 
            Alnfield(0) = "C" : Alnfield(1) = "N"
            Retfield(0) = "Code" : Retfield(1) = "Description"

            'Active
            Dim ColumnPresentation(,) As Object = {{"Active", PresentationEnum.CheckBox}, {"Match", PresentationEnum.CheckBox}}
            FilterInfo(0, 0) = "Active" : FilterInfo(0, 1) = "True"
            FilterInfo(1, 0) = "Match" : FilterInfo(1, 1) = "True"
            If Active = True Then
                Sizefield(2) = 0 : Sizefield(1) = 190
            Else
                Sizefield(2) = 40 : Sizefield(1) = 150
            End If
            Alnfield(2) = "C"
            Retfield(2) = "Active"

            If Match Then Sizefield(3) = 40 Else Sizefield(3) = 0
            Alnfield(3) = "C"
            Retfield(3) = "Match"

            For i = 4 To MaxCnt + 3   'Size
                Sizefield(i) = 50
                Alnfield(i) = "C"
                Retfield(i) = "Size" & i - 3
            Next

            daSql = New SqlDataAdapter(strsql, cnSql)
            dsSql = New DataSet
            daSql.Fill(dsSql, "tbcode")
            cnSql.Close()
            frm = New frmSearch
            With frm
                .Owner = FrmOwner
                If .ShowDialog("Return", dsSql.Tables("tbcode"), dsSql.Tables("tbcode").Rows.Count,
                "Grid Size List", Sizefield, Alnfield, Retfield, 0, Nothing, False, -1, 500, , True,
                    , , , , , FilterInfo, , , , , , , , , , , , , , , , , , , ColumnPresentation) = DialogResult.OK Then
                    If gbRetSearch(0) <> "" Then
                        TxtGrid.Text = gbRetSearch(0)
                        TxtGridDesc.Text = gbRetSearch(1)
                    End If
                    TxtGrid.Focus()
                    If TAB = True Then SendKeys.Send("{TAB}")
                Else
                    TxtGrid.Focus()
                End If
            End With
        Catch exp As Exception
            Throw exp
        Finally
            gbRetSearch = Nothing
            dsSql = Nothing : daSql = Nothing : cmSql = Nothing
            If Not cnSql Is Nothing Then
                If cnSql.State = ConnectionState.Open Then cnSql.Close()
                cnSql = Nothing
            End If
            frm = Nothing
            Cursor.Current = Cursors.Default
        End Try
    End Sub

    Public Function OverheadStrsql(Optional ByVal Cond As String = "") As String
        Try
            OverheadStrsql = "SELECT OH_CODE AS Code, OH_DESC AS Description FROM STK04_Overheads WHERE 1=1"
            If Cond <> "" Then OverheadStrsql &= " And " & Cond
        Catch ex As Exception
            Throw ex
        End Try
    End Function
    Public Function PriceTypeStrsql(Optional ByVal Cond As String = "") As String
        Try
            PriceTypeStrsql = "SELECT PRC_CODE AS Code, PRC_DESC AS Description FROM STK04_PriceType where 1=1 "
            If Cond <> "" Then PriceTypeStrsql &= " And " & Cond
        Catch ex As Exception
            Throw ex
        End Try
    End Function
    Public Function SalePriceTypeStrsql(Optional ByVal Cond As String = "", Optional ByVal Active As Boolean = True) As String
        Try
            SalePriceTypeStrsql = "SELECT Price_CODE AS Code, Price_DESC AS Description,"
            SalePriceTypeStrsql &= " CASE WHEN Price_Active ='Y' THEN 'True' ELSE 'False' END [Active]"
            SalePriceTypeStrsql &= " FROM STK04_SalPriceType where 1=1 "
            If Active = True Then SalePriceTypeStrsql &= " AND Price_Active = 'Y'"
            If Cond <> "" Then SalePriceTypeStrsql &= " And " & Cond
        Catch ex As Exception
            Throw ex
        End Try
    End Function

    Public Function PayModeStrsql(Optional ByVal Cond As String = "", Optional ByVal Active As Boolean = False) As String
        Try
            PayModeStrsql = "SELECT PAY_CODE AS Code, PAY_DESC AS Description ,Pay_SettNo AS [Sett. No.],"
            PayModeStrsql &= " Pay_Percentage AS Percentage, Pay_NoDays AS [No. Days],"
            PayModeStrsql &= " case when PAY_Active ='Y' THEN 'True' ELSE 'False' END  [Active]"
            PayModeStrsql &= " FROM V_PORD2_PayTerm WHERE 1=1 "
            If Active = True Then PayModeStrsql &= " AND PAY_Active = 'Y'"
            If Cond <> "" Then PayModeStrsql &= " And " & Cond
        Catch ex As Exception
            Throw ex
        End Try
    End Function
    Public Function LinkCOStrsql(Optional ByVal Cond As String = "") As String
        Try
            LinkCOStrsql = "SELECT HOFR_LOC AS [Loc.], Loc_Desc AS [Loc. Description], HOFR_RECID AS ID,"
            LinkCOStrsql &= " HOFR_SeqNo AS [Seq.No.], HOFR_ManualNo AS [Manual No.], HOFR_DATE AS Date,"
            LinkCOStrsql &= " CL_Code AS [Client No.], CL_No AS [Sub.No.], HOFR_CLTNAME AS [Client Name],"
            LinkCOStrsql &= " HOFR_CLTADR AS [Client Address], HOFR_CURR AS Curr, HOFR_NET AS [Total Net]"
            LinkCOStrsql &= " FROM dbo.STK04_OfferHeader"
            LinkCOStrsql &= " INNER JOIN dbo.Stk04_Clients ON HOFR_CLTCODE=CL_RecID"
            LinkCOStrsql &= " INNER JOIN dbo.STK04_Location ON Loc_Code=HOFR_LOC"
            LinkCOStrsql &= " WHERE HOFR_CONFDATE IS NOT NULL AND HOFR_Active='Y' and HOFR_INVCLT='N'"
            If Cond <> "" Then LinkCOStrsql &= " And " & Cond
        Catch ex As Exception
            Throw ex
        End Try
    End Function

    Public Function AuxAccStrsql(Optional ByVal Cond As String = "", Optional ByVal ServerName As String = "") As String
        Try
            AuxAccStrsql = "SELECT Aux_accno1 AS [G/L], Aux_accno2 AS Auxiliary,"
            AuxAccStrsql &= " Aux_subno AS Subsidiary,Aux_curr AS Currency,Aux_Name AS [A/C Name]"
            AuxAccStrsql &= " FROM " & ServerName & "Acc01_Auxiliary"
            AuxAccStrsql &= " Where Aux_Comp='" & gbComp & "' And Aux_Year='" & gbYear & "'"
            If Cond <> "" Then AuxAccStrsql &= " And " & Cond
            AuxAccStrsql &= " ORDER BY Aux_accno1,Aux_accno2,Aux_subno,Aux_curr,Aux_Name"
        Catch ex As Exception
            Throw ex
        End Try
    End Function

    Public Sub PriceTypeList(ByVal txtPriceType As TextBox, ByVal lblPriceType As Label, ByVal FrmOwner As Form, Optional ByVal Cond As String = "")
        Dim dsSql As DataSet
        Dim frm As frmSearch
        Dim Sizefield() As Int16 = {60, 300}
        Dim Alnfield() As String = {"C", "N"}
        Dim Retfield() As String = {"Code", "Description"}
        Try
            Cursor.Current = Cursors.WaitCursor
            ReadFromDBToDataSet(PriceTypeStrsql(Cond), "tbcode", dsSql)
            frm = New frmSearch
            With frm
                .Owner = FrmOwner
                If .ShowDialog("Return", dsSql.Tables("tbcode"), dsSql.Tables("tbcode").Rows.Count,
                "Pricing Type List", Sizefield, Alnfield, Retfield, 0, Nothing, False, 410, 500, , True) = DialogResult.OK Then
                    If gbRetSearch(0) <> "" Then
                        txtPriceType.Text = gbRetSearch(0)
                        lblPriceType.Text = gbRetSearch(1)
                    End If
                    txtPriceType.Focus()
                    SendKeys.Send("{TAB}")
                Else
                    txtPriceType.Focus()
                End If
            End With
        Catch exp As Exception
            Throw exp
        Finally
            gbRetSearch = Nothing
            dsSql = Nothing
            frm = Nothing
            Cursor.Current = Cursors.Default
        End Try
    End Sub
    Public Sub PayModeList(ByVal txtPayMode As TextBox, ByVal lblPayMode As Label, ByVal FrmOwner As Form, Optional ByVal Cond As String = "",
                            Optional ByVal Active As Boolean = True, Optional ByVal Title As String = "Payment Mode List")
        Dim dsSql As DataSet
        Dim frm As frmSearch

        Dim FilterInfo(0, 1) As String
        Dim Sizefield() As Int16 = {60, 200, 80, 80, 80, 40}
        Dim Alnfield() As String = {"C", "N", "C", "C", "C", "C"}
        Dim Retfield() As String = {"Code", "Description", "Sett. No.", "Percentage", "No. Days", "Active"}
        Dim MergeCol() As String = {"Code", "Description"}

        Dim ColumnPresentation(,) As Object = {{"Active", PresentationEnum.CheckBox}}
        FilterInfo(0, 0) = "Active" : FilterInfo(0, 1) = "True"
        If Active = True Then Sizefield(1) = 240 : Sizefield(5) = 0

        Try
            Cursor.Current = Cursors.WaitCursor
            ReadFromDBToDataSet(PayModeStrsql(Cond, Active), "tbcode", dsSql)
            frm = New frmSearch
            With frm
                .Owner = FrmOwner
                If .ShowDialog("Return", dsSql.Tables("tbcode"), dsSql.Tables("tbcode").Rows.Count,
                    Title, Sizefield, Alnfield, Retfield, 0, MergeCol, False, 590, 500, , True,
                    , , , , , FilterInfo, , , , , , , , , , , , , , , , , , , ColumnPresentation) = DialogResult.OK Then
                    If gbRetSearch(0) <> "" Then
                        txtPayMode.Text = gbRetSearch(0)
                        lblPayMode.Text = gbRetSearch(1)
                    End If
                    txtPayMode.Focus()
                    SendKeys.Send("{TAB}")
                Else
                    txtPayMode.Focus()
                End If
            End With
        Catch exp As Exception
            Throw exp
        Finally
            gbRetSearch = Nothing
            dsSql = Nothing
            frm = Nothing
            Cursor.Current = Cursors.Default
        End Try
    End Sub

    Public Sub NewUsedList(ByVal TxtNewUsed As TextBox, ByVal TxtNewUsedDesc As Label, ByVal FrmOwner As Form, Optional ByVal _
            TAB As Boolean = True, Optional ByVal Consignment As Boolean = True)

        Dim frm As frmSearch
        Dim drSql As SqlDataReader
        Dim RowNumb, i As Integer
        Dim Cond As String

        Dim Sizefield() As Int16 = {80, 200}
        Dim Alnfield() As String = {"C", "N"}
        Dim Retfield() As String = {"Code", "Description"}
        Try
            Cursor.Current = Cursors.WaitCursor
            If UCase(gbUser) = "SA" Or UCase(gbUser) = "SU" Then
                RowNumb = 8
                If Consignment = False Then RowNumb = RowNumb - 1
            Else
                Cond = "ASP_USER='" & gbUser & "' AND ASP_APPcODE ='" & gbApp & "'"
                If Consignment = False Then Cond &= " AND ASP_STKTAG <> 'C'"
                RowNumb = CInt(GetFieldDesc("STK04_APPSTKPRIV", Cond, "COUNT(ISNULL(ASP_STKTAG,0))"))
                If Consignment = True Then
                    RowNumb = 1
                End If
            End If
            Dim tblRow(RowNumb - 1, 1) As String

            If UCase(gbUser) = "SA" Or UCase(gbUser) = "SU" Then
                If Consignment = True Then
                    tblRow(7, 0) = "C" : tblRow(0, 1) = "Consignment Item"
                Else
                    tblRow(0, 0) = "N" : tblRow(0, 1) = "New"
                    tblRow(1, 0) = "U" : tblRow(1, 1) = "Used"
                    tblRow(2, 0) = "F" : tblRow(2, 1) = "Finished Product" '...Jad-J...(24/10/2016) P-Production replaced by F-Finished Product
                    tblRow(3, 0) = "R" : tblRow(3, 1) = "Raw Material"
                    tblRow(4, 0) = "S" : tblRow(4, 1) = "Semi-Finished Product" '...Jad-J...(24/10/2016) Selling Item replaced by Semi-Finished Product
                    tblRow(5, 0) = "T" : tblRow(5, 1) = "Title"
                    tblRow(6, 0) = "V" : tblRow(6, 1) = "Title For Rent"

                End If

            Else
                If Consignment = True Then
                    strsql = "SELECT ASP_STKTAG ,  "
                    strsql &= " CASE WHEN ASP_STKTAG='C' THEN 'Consignment Item' ELSE 'New & Consignment' END "
                    strsql &= "   FROM STK04_APPSTKPRIV "
                    strsql &= " WHERE ASP_USER='" & gbUser & "' AND ASP_APPcODE ='" & gbApp & "' and ASP_STKTAG = 'C'"
                Else
                    strsql = "SELECT ASP_STKTAG , CASE WHEN ASP_STKTAG='N' THEN 'New' ELSE "
                    strsql &= " CASE WHEN ASP_STKTAG='U' THEN 'Used' ELSE"
                    strsql &= " CASE WHEN ASP_STKTAG='F' THEN 'Finished Product' ELSE " '...Jad-J...(24/10/2016) P-Production replaced by F-Finished Product
                    strsql &= " CASE WHEN ASP_STKTAG='R' THEN 'Raw Material' ELSE	"
                    strsql &= " CASE WHEN ASP_STKTAG='S' THEN 'Semi-Finished Product' ELSE" '...Jad-J...(24/10/2016) Selling Item replaced by Semi-Finished Product
                    strsql &= " CASE WHEN ASP_STKTAG='T' THEN 'Title' ELSE"
                    strsql &= " CASE WHEN ASP_STKTAG='V' THEN 'Title For Rent' ELSE"
                    strsql &= " CASE WHEN ASP_STKTAG='C' THEN 'Consignment Item' ELSE 'New & Consignment' END "
                    strsql &= " END END END END	END END END  FROM STK04_APPSTKPRIV "
                    strsql &= " WHERE ASP_USER='" & gbUser & "' AND ASP_APPcODE ='" & gbApp & "'"
                    If Consignment = False Then strsql &= " and ASP_STKTAG <> 'C'"

                End If
                ReadFromDBToDataReader(strsql, drSql)
                i = 0
                If drSql.HasRows Then
                    While drSql.Read
                        tblRow(i, 0) = drSql.Item(0).ToString
                        tblRow(i, 1) = drSql.Item(1).ToString
                        i = i + 1
                    End While
                End If
            End If
            frm = New frmSearch
            With frm
                .Owner = FrmOwner
                If .ShowDialog("Return", CreateDataTable("tblStatus", "Code", "Description", tblRow), CLng(RowNumb),
                    "Item Status", Sizefield, Alnfield, Retfield, 0, Nothing, False, 310, 300, , True, 25) = DialogResult.OK Then
                    If gbRetSearch(0) <> "" Then
                        TxtNewUsed.Text = gbRetSearch(0)
                        TxtNewUsedDesc.Text = gbRetSearch(1)
                        TxtNewUsed.Focus()
                        If TAB = True Then SendKeys.Send("{TAB}")
                    End If
                Else
                    TxtNewUsed.Focus()
                End If
            End With
        Catch exp As Exception
            Throw exp
        Finally
            gbRetSearch = Nothing
            frm = Nothing
            drSql = Nothing
            Cursor.Current = Cursors.Default

            Retfield = Nothing
            Alnfield = Nothing
            Sizefield = Nothing
        End Try
    End Sub
    Public Sub StatusListDO(ByVal TxtStatus As TextBox, ByVal TxtStatusDesc As Label, ByVal FrmOwner As Form, Optional ByVal TAB As Boolean = True)
        Dim frm As frmSearch
        Dim drSql As SqlDataReader

        Dim tblRow(2, 1) As String

        Dim Sizefield() As Int16 = {80, 200}
        Dim Alnfield() As String = {"C", "N"}
        Dim Retfield() As String = {"Code", "Description"}
        Try
            tblRow(0, 0) = "N" : tblRow(0, 1) = "Normal"
            tblRow(1, 0) = "F" : tblRow(1, 1) = "Free"
            tblRow(2, 0) = "R" : tblRow(2, 1) = "Return"

            frm = New frmSearch
            With frm
                .Owner = FrmOwner
                If .ShowDialog("Return", CreateDataTable("tblStatus", "Code", "Description", tblRow), CLng(RowNumb),
                    "Status", Sizefield, Alnfield, Retfield, 0, Nothing, False, 310, 170, , True, 25) = DialogResult.OK Then
                    If gbRetSearch(0) <> "" Then
                        TxtStatus.Text = gbRetSearch(0)
                        TxtStatusDesc.Text = gbRetSearch(1)
                        TxtStatus.Focus()
                        If TAB = True Then SendKeys.Send("{TAB}")
                    End If
                Else
                    TxtStatus.Focus()
                End If
            End With
        Catch exp As Exception
            Throw exp
        Finally
            gbRetSearch = Nothing
            frm = Nothing
            Retfield = Nothing
            Alnfield = Nothing
            Sizefield = Nothing
            Cursor.Current = Cursors.Default
        End Try
    End Sub
    Public Sub StatusList(ByVal TxtStatus As TextBox, ByVal TxtStatusDesc As Label, ByVal FrmOwner As Form, Optional ByVal TAB As Boolean = True)
        Dim frm As frmSearch
        Dim drSql As SqlDataReader

        Dim tblRow(1, 1) As String

        Dim Sizefield() As Int16 = {80, 200}
        Dim Alnfield() As String = {"C", "N"}
        Dim Retfield() As String = {"Code", "Description"}
        Try
            tblRow(0, 0) = "N" : tblRow(0, 1) = "Normal"
            tblRow(1, 0) = "F" : tblRow(1, 1) = "Free"

            frm = New frmSearch
            With frm
                .Owner = FrmOwner
                If .ShowDialog("Return", CreateDataTable("tblStatus", "Code", "Description", tblRow), CLng(RowNumb),
                    "Status", Sizefield, Alnfield, Retfield, 0, Nothing, False, 310, 170, , True, 25) = DialogResult.OK Then
                    If gbRetSearch(0) <> "" Then
                        TxtStatus.Text = gbRetSearch(0)
                        TxtStatusDesc.Text = gbRetSearch(1)
                        TxtStatus.Focus()
                        If TAB = True Then SendKeys.Send("{TAB}")
                    End If
                Else
                    TxtStatus.Focus()
                End If
            End With
        Catch exp As Exception
            Throw exp
        Finally
            gbRetSearch = Nothing
            frm = Nothing
            Retfield = Nothing
            Alnfield = Nothing
            Sizefield = Nothing
            Cursor.Current = Cursors.Default
        End Try
    End Sub
    Public Sub UsageList(ByVal TxtUsage As TextBox, ByVal TxtUsageDesc As Label, ByVal FrmOwner As Form, Optional ByVal TAB As Boolean = True)
        Dim frm As frmSearch
        Dim drSql As SqlDataReader
        Dim tblRow(1, 1) As String

        Dim Sizefield() As Int16 = {80, 200}
        Dim Alnfield() As String = {"C", "N"}
        Dim Retfield() As String = {"Code", "Description"}
        Try
            tblRow(0, 0) = "N" : tblRow(0, 1) = "Normal Use"
            tblRow(1, 0) = "I" : tblRow(1, 1) = "Internal Use"
            frm = New frmSearch
            With frm
                .Owner = FrmOwner
                If .ShowDialog("Return", CreateDataTable("tblUsage", "Code", "Description", tblRow), CLng(RowNumb),
                    "Usage", Sizefield, Alnfield, Retfield, 0, Nothing, False, 310, 170, , True, 25) = DialogResult.OK Then
                    If gbRetSearch(0) <> "" Then
                        TxtUsage.Text = gbRetSearch(0)
                        TxtUsageDesc.Text = gbRetSearch(1)
                        TxtUsage.Focus()
                        If TAB = True Then SendKeys.Send("{TAB}")
                    End If
                Else
                    TxtUsage.Focus()
                End If
            End With
        Catch exp As Exception
            Throw exp
        Finally
            gbRetSearch = Nothing
            frm = Nothing
            Retfield = Nothing
            Alnfield = Nothing
            Sizefield = Nothing
            Cursor.Current = Cursors.Default
        End Try
    End Sub
    Public Sub AuxAccList(ByVal txtGL As TextBox, ByVal txtAux As TextBox, ByVal txtSub As TextBox, ByVal txtCurr As TextBox, ByVal lblAccName As Label, ByVal FrmOwner As Form, Optional ByVal Cond As String = "", Optional ByVal ServerName As String = "")
        Dim dsSql As DataSet
        Dim frm As frmSearch
        Dim Sizefield() As Int16 = {80, 80, 60, 60, 200}
        Dim Alnfield() As String = {"F", "F", "F", "F", "N"}
        Dim Retfield() As String = {"G/L", "Auxiliary", "Subsidiary", "Currency", "A/C Name"}
        Dim MergeCol() As String = {"G/L", "Auxiliary"}
        Try
            Cursor.Current = Cursors.WaitCursor
            ReadFromDBToDataSet(AuxAccStrsql(Cond, ServerName), "tbcode", dsSql)
            If dsSql.Tables("tbcode").Rows.Count = 0 Then
                MessageBox.Show("No Records Found.", FrmOwner.Name, MessageBoxButtons.OK, MessageBoxIcon.Information)
                Exit Sub
            End If

            frm = New frmSearch
            With frm
                .Owner = FrmOwner
                If .ShowDialog("Return", dsSql.Tables("tbcode"), dsSql.Tables("tbcode").Rows.Count, "List Of Bank Accounts Numbers.",
                    Sizefield, Alnfield, Retfield, 0, MergeCol, False, 550, 500, , True) = DialogResult.OK Then
                    If gbRetSearch(0) <> "" Then
                        txtGL.Text = gbRetSearch(0)
                        txtAux.Text = gbRetSearch(1)
                        txtSub.Text = gbRetSearch(2)
                        txtCurr.Text = gbRetSearch(3)
                        lblAccName.Text = gbRetSearch(4)
                    End If
                    txtCurr.Focus()
                    SendKeys.Send("{TAB}")
                Else
                    txtGL.Focus()
                End If
            End With
        Catch exp As Exception
            Throw exp
        Finally
            gbRetSearch = Nothing
            dsSql = Nothing
            frm = Nothing
            Cursor.Current = Cursors.Default
        End Try
    End Sub
    'Public Sub ItemColorList(ByVal txtColor As TextBox, ByVal txtColorDesc As Label, ByVal RecID As String, ByVal FrmOwner As Form)
    '    Dim Str As String
    '    Dim dsSQL As DataSet
    '    Dim frm As frmSearch

    '    Dim Sizefield() As Int16 = {60, 300, 40}
    '    Dim Alnfield() As String = {"C", "N", "C"}
    '    Dim Retfield() As String = {"Code", "Description", "Active"}
    '    Dim ColumnPresentation(,) As Object = {{"Active", PresentationEnum.CheckBox}}

    '    Try
    '        'Stock Item : Show active colors or inactive colors that are already linked to this item
    '        Str = ItemColorStrsql(RecID, "")
    '        ReadFromDBToDataSet(Str, "tbColors", dsSQL)
    '        If dsSQL.Tables("tbColors").Rows.Count > 0 Then
    '            frm = New frmSearch
    '            With frm
    '                .Owner = FrmOwner
    '                If .ShowDialog("Return", dsSQL.Tables("tbColors"), dsSQL.Tables("tbColors").Rows.Count,
    '                "Colors List", Sizefield, Alnfield, Retfield, 0, Nothing, False, 450, 500, False, True,
    '                , , , , , , , , , , , , , , , , , , , , , , , , ColumnPresentation) = DialogResult.OK Then
    '                    If gbRetSearch(0) <> "" Then
    '                        txtColor.Text = gbRetSearch(0)
    '                        txtColorDesc.Text = gbRetSearch(1)
    '                    End If
    '                    txtColor.Focus()
    '                Else
    '                    txtColor.Focus()
    '                End If
    '            End With
    '        End If
    '    Catch ex As Exception
    '        Throw ex
    '    End Try
    'End Sub

#Region "Purchase Order"
    'This view is used in the printing of the PO
    'Modified  By Abir T. @ 03.06.2013
    Public Function CreateView_V_PORD2_ManOrdDetRep(ByVal Fashion As Boolean) As String
        Dim Str As String
        Dim KeyRef As String

        Dim KeySupp As Boolean = False
        Dim KeyCatg As Boolean = False
        Dim KeySerie As Boolean = False
        Dim KeyUsage As Boolean = False
        Dim KeyNewUsed As Boolean = False

        Dim KeyStyle As Boolean = False
        Dim KeyGender As Boolean = False
        Dim KeyGrid As Boolean = False
        Dim KeySeason As Boolean = False
        Dim KeyBrand As Boolean = False
        Try
            KeyRef = ""
            For k = 0 To CurrItmKey.Length - 1
                If UCase(CurrItmKey(k)) = "STK_SERIE" Then
                    KeyRef &= "cast([" & CurrItmKeyPend(k) + "] as varchar)+'/'+"
                Else
                    KeyRef &= "[" & CurrItmKeyPend(k) + "]+'/'+"
                End If
                If UCase(CurrItmKey(k)) = "STK_SUPPLIER" Then KeySupp = True
                If UCase(CurrItmKey(k)) = "STK_CATEGORY" Then KeyCatg = True
                If UCase(CurrItmKey(k)) = "STK_SERIE" Then KeySerie = True
                If UCase(CurrItmKey(k)) = "STK_USAGE" Then KeyUsage = True
                If UCase(CurrItmKey(k)) = "STK_NEWUSED" Then KeyNewUsed = True

                If UCase(CurrItmKey(k)) = "STK_SEASON" Then KeySeason = True
                If UCase(CurrItmKey(k)) = "STK_BRAND" Then KeyBrand = True
                If UCase(CurrItmKey(k)) = "STK_STYLE" Then KeyStyle = True
                If UCase(CurrItmKey(k)) = "STK_GENDER" Then KeyGender = True
            Next
            KeyRef = KeyRef.Substring(0, KeyRef.Length - 5)
            DropSqlObject("V_PORD2_ManOrdDetRep_" & gbUser, "VIEW")

            Str = " CREATE  VIEW V_PORD2_ManOrdDetRep_" & gbUser & " AS"
            Str &= " SELECT Mod_Hrecid, mod_supitmref, mod_stat, mod_Count, mod_RMRecid, MOD_PENDRecid,"
            For k = 0 To CurrItmKey.Length - 1
                Str &= " ISNULL(" & CurrItmKey(k) + ", " & CurrItmKeyPend(k) + ") AS " & CurrItmKey(k) + ","
            Next
            If KeySupp = False Then Str &= " ISNULL(Stk_supplier, SFP_SUPPLIER) AS Stk_supplier,"
            If KeyCatg = False Then Str &= " ISNULL(Stk_Category, Sfp_Category) AS Stk_Category,"
            If KeySerie = False Then Str &= " ISNULL(Stk_Serie, SFP_Serie) AS Stk_Serie,"
            If KeyUsage = False Then Str &= " ISNULL(Stk_usage, SFP_Usage) AS Stk_usage,"
            If KeyNewUsed = False Then Str &= " ISNULL(Stk_newused, SFP_newused) AS Stk_newused,"

            'Str &= " CASE WHEN ISNULL(STK_CATEGORY, SFP_CATEGORY)='TMP' THEN"
            'Str &= " rtrim(substring(MOD_Rem COLLATE SQL_Latin1_General_CP1256_CI_AS,1,40)) +' '+"
            'Str &= " ltrim(substring(MOD_Rem COLLATE SQL_Latin1_General_CP1256_CI_AS ,41,80) )"
            ' Str &= " ELSE ISNULL(Stk_Desc, SFP_Desc) END AS Stk_Desc,"
            Str &= "   ISNULL(Stk_Desc, SFP_Desc)   AS Stk_Desc,"
            Str &= " ISNULL(Stk_Descabv, '') AS Stk_Descabv,"
            Str &= " ISNULL(STK_REF, " & KeyRef & ") AS STK_REF,"
            Str &= " mod_ORDQty, Meas_Desc AS Stk_SubUNIT, mod_ConsOrdQty,  "
            Str &= " mod_UnitCost, mod_ItmDisc, mod_CashDisc, mod_NETTOT,"
            ' Str &= " CASE WHEN ISNULL(STK_CATEGORY, SFP_CATEGORY)='TMP'THEN '' ELSE MOD_Rem END  AS MOD_Rem,"
            Str &= "   MOD_Rem    AS MOD_Rem,"
            If KeySeason = False Then Str &= " ISNULL(STK_Season, SFP_Season) AS STK_Season ,"
            Str &= " (SELECT Seas_Desc FROM Stk04_Season "
            Str &= " WHERE ISNULL(STK_Season, SFP_Season) = Seas_Code) AS Seas_Desc ,"

            If KeyBrand = False Then Str &= " ISNULL(STK_Brand, SFP_Brand) AS STK_Brand ,"
            Str &= " (SELECT br_Name FROM Stk04_Brand "
            Str &= " WHERE ISNULL(STK_Brand, SFP_Brand) = br_Code) AS br_Name ,"

            If KeyGender = False Then Str &= " ISNULL(STK_Gender, SFP_Gender) AS STK_Gender ,"
            Str &= " (SELECT G_Desc FROM STK04_Gender "
            Str &= " WHERE ISNULL(STK_Gender, SFP_Gender) = G_Code) AS G_Desc ,"

            Str &= " ISNULL(STK_ITMTYPE, SFP_ITMTYPE) AS STK_ITMTYPE ,"
            Str &= " (SELECT IT_DESC FROM STK04_ItemType "
            Str &= " WHERE ISNULL(STK_ITMTYPE, SFP_ITMTYPE) = IT_Code) AS IT_DESC ,"

            If KeyStyle = False Then Str &= " ISNULL(STK_Style, SFP_Style) AS STK_Style,"
            Str &= " MOD_SuppColor, MOD_Color, Color_Desc, MOD_GridCode,MOD_DelDate,mod_lineno"
            Str &= " ,FLQTY, NLQTY, FWQTY, NWQTY, FPQTY, NPQTY "
            Str &= " FROM STK04_morddET"
            Str &= " LEFT JOIN Stk04_stkfil on MOD_RMRecid =Stk_recid"
            Str &= " LEFT JOIN dbo.STK04_StkFilPENDing P1 ON MOD_PENDRecid = SFP_Recid"
            Str &= " LEFT JOIN STK04_Measurements On Meas_unit=MOD_ORDUNIT"
            Str &= " LEFT JOIN Stk04_COLOR ON Color_Code = MOD_Color"

            Str &= " LEFT JOIN (SELECT mod_hrecid hrecid,"
            Str &= " ISNULL(SUM(CASE WHEN MOD_Stat='F' and Meas_Type='L' THEN round(Qty," & gbDecNo & ") ELSE 0 END),0) AS FLQTY," '..Free-Lenght
            Str &= " ISNULL(SUM(CASE WHEN MOD_Stat='N' and Meas_Type='L' THEN round(Qty," & gbDecNo & ") ELSE 0 END),0) AS NLQTY," '...Normal-lenght
            Str &= " ISNULL(SUM(CASE WHEN MOD_Stat='F' and Meas_Type='W' THEN round(Qty," & gbDecNo & ") ELSE 0 END),0) AS FWQTY," '...free- weight
            Str &= " ISNULL(SUM(CASE WHEN MOD_Stat='N' and Meas_Type='W' THEN round(Qty," & gbDecNo & ") ELSE 0 END),0) AS NWQTY," '...normal- weight
            Str &= " ISNULL(SUM(CASE WHEN MOD_Stat='F' and Meas_Type='P' THEN round(Qty," & gbDecNo & ") ELSE 0 END),0) AS FPQTY," '...free-piece
            Str &= " ISNULL(SUM(CASE WHEN MOD_Stat='N' and Meas_Type='P' THEN round(Qty," & gbDecNo & ") ELSE 0 END),0) AS NPQTY" '...normal piece
            Str &= " FROM ("
            Str &= "  SELECT  MOD_HRecid AS mod_hrecid ,"
            Str &= "         MOD_Stat AS MOD_Stat ,"
            Str &= "         ISNULL(Meas1.Meas_Type, Meas2.Meas_Type) AS Meas_Type ,"
            Str &= "         ISNULL(STK_ItmClass, SFP_ItmClass) AS STK_ItmClass ,"
            Str &= "         ISNULL(SUM(( CASE WHEN ISNULL(Meas1.MEAS_SIGN, Meas2.MEAS_SIGN) = '*'"
            Str &= "                                 THEN  CASE WHEN COALESCE(STK_OPERATOR,sfp_operator)='*' THEN  ISNULL(Meas1.MEAS_VALUE, Meas2.MEAS_VALUE) *   ISNULL(STK_UNITPACK,SFP_UNITPACK) * MOD_OrdQty"
            Str &= " 								ELSE  (MOD_OrdQty /ISNULL(STK_UNITPACK,SFP_UNITPACK)) * ISNULL(Meas1.MEAS_VALUE, Meas2.MEAS_VALUE) END         "
            Str &= " 								   ELSE CASE WHEN COALESCE(STK_OPERATOR,sfp_operator)='*' THEN   (  ISNULL(STK_UNITPACK,SFP_UNITPACK) * MOD_OrdQty ) / ISNULL(Meas1.MEAS_VALUE,  Meas2.MEAS_VALUE)"
            Str &= " 								   ELSE   ( MOD_OrdQty / ISNULL(STK_UNITPACK,SFP_UNITPACK)) / ISNULL(Meas1.MEAS_VALUE,  Meas2.MEAS_VALUE) END "
            Str &= "                            END )), 0) AS Qty ,"
            Str &= "         ISNULL(SUM(MOD_UnitCost * MOD_OrdQty * ( 1 - MOD_ItmDisc / 100.0000 )),"
            Str &= "                0) AS tot ,"
            Str &= "         ISNULL(SUM(MOD_UnitCost * MOD_OrdQty * ( 1 - MOD_ItmDisc / 100.0000 )"
            Str &= "                    * ( MOD_CashDisc / 100.0000 )), 0) AS cashdisc ,"
            Str &= "         ISNULL(SUM(MOD_NetTot), 0) AS nettot"
            Str &= "  FROM    Stk04_MOrdDet"
            Str &= "         LEFT JOIN stk04_stkfil ON stk_recid = MOD_RMRecid"
            Str &= "         LEFT JOIN dbo.STK04_StkFilPending ON SFP_Recid = MOD_PendRecid"
            Str &= "         LEFT JOIN stk04_Measurements Meas1 ON Meas1.Meas_unit = stk_subunit"
            Str &= "         LEFT JOIN stk04_Measurements Meas2 ON Meas2.Meas_unit = SFP_SUBUNIT"
            Str &= "  GROUP BY MOD_HRecid ,"
            Str &= "         MOD_Stat ,"
            Str &= "         ISNULL(Meas1.Meas_Type, Meas2.Meas_Type) ,"
            Str &= "         ISNULL(STK_ItmClass, SFP_ItmClass)"
            Str &= "  )tDetTot GROUP BY mod_hrecid) AS TempQty"
            Str &= "  ON TempQty.hrecid =Mod_Hrecid"

            Execute(Str)
        Catch ex As Exception
            Throw ex
        End Try
    End Function
    'Print Sub Report Payment Terms in the PO
    'Added By Abir T. @ 04.06.2013
    Public Sub CreatePOTempSubRep(ByVal TempPOSubRep As String, ByVal POCode As String, ByVal Curr As String, Optional ByVal TempPOSel As String = "", Optional PrintFundDisbursement As Boolean = False) 'edited by GS on 23/08/16 (add TempPOSel) 
        Dim Str As String
        Try
            Str = " IF EXISTS (SELECT name FROM sysobjects WHERE type = 'U' AND name = '" & TempPOSubRep & "')"
            Str &= " DROP TABLE " & TempPOSubRep
            Execute(Str)

            Str = " CREATE TABLE " & TempPOSubRep & "("
            Str &= " [PONo.] [Numeric](18, 0) NOT NULL,"
            'PAY_DESC,Pay_Percentage
            Str &= " [Desc]  NVARCHAR(Max) NULL,"
            Str &= " [Perc] [Numeric](6, 3) NULL,"
            Str &= " [Sett No.] [Numeric](18, 0) NOT NULL,"
            Str &= " [Amt]  [Numeric](18, 3) NULL,"
            Str &= " [Curr]  VARCHAR(3) NULL,"
            Str &= " [Date] DATETIME NULL"

            If gbFundDisbursementModuleIsActive And PrintFundDisbursement Then
                Str &= ",[Fund Code]  VARCHAR(4) NULL,
                         [Fund Desc] NVARCHAR(Max) NULL,
                         [AmtToDisburse]  [Numeric](18, 3) NULL,
                         [Fund Curr]  VARCHAR(3) NULL,
                         [Oper LC]  VARCHAR(1) NULL,
                         [RateToLC] [Numeric](18, 3) NULL,
                         [CounterValLC] [Numeric](18, 3) NULL,
                         [Oper CC]  VARCHAR(1) NULL,
                         [RateToCC] [Numeric](18, 3) NULL,
                         [CounterValCC] [Numeric](18, 3) NULL,
                         [Remarks]  NVARCHAR(Max) NULL"
            End If

            Str &= "         )"
            Execute(Str)

            Str = " INSERT INTO " & TempPOSubRep
            Str &= " SELECT RMP_Hrecid, PAY_DESC,Pay_Percentage,RMP_PaymentNo, RMP_Amt, " & IIf(TempPOSel = "", "'" & Curr & "'", "RMO_cur").ToString & ", RMP_DueDate"  ''" & Curr & "', RMP_DueDate" 'edited by GS on 23/08/16
            If gbFundDisbursementModuleIsActive And PrintFundDisbursement Then
                Str &= " , FDT_Code ,
                           FDT_Description ,
                           FTR_AmountToDisburse ,
                           FTR_Currency ,
                           FTR_OperLC ,
                           FTR_RateToLC ,
                           FTR_CounterValueLC ,
                           FTR_OperFreshDollar ,
                           FTR_RateFreshDollar ,
                           FTR_CounterValueFreshDollar ,
                           FTR_Remarks "
            End If

            Str &= " FROM Stk04_MOPay"

            If TempPOSel <> "" Then Str &= " INNER JOIN " & TempPOSel & " ON RMO_Recid = RMP_Hrecid and Chk = 'True' " 'added by GS on 23/08/16

            'Rachel A. 22.02.2016 
            Str &= " LEFT JOIN dbo.STK04_PayTerm ON STK04_PayTerm.PAY_CODE = Stk04_MOPay.RMP_PayMode AND STK04_PayTerm.Pay_SettNo = Stk04_MOPay.RMP_PaymentNo"

            If gbFundDisbursementModuleIsActive And PrintFundDisbursement Then
                Str &= " LEFT JOIN ( SELECT  FTR_RecID ,
                                             FTR_TrnRecID ,
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
                                             FTR_Remarks ,
                                             FTR_RMPPayMode ,
                                             FTR_RMPPaymentNo
                                     FROM    STK04_FundTransactionPOSett
                                             INNER JOIN ( SELECT FDT_RecID ,
                                                                 FDT_Code ,
                                                                 FDT_Description
                                                          FROM   " & gbAccountingLinkedServerForFunds & "ACC01_FundTypes
                                                        ) ACC01_FundTypes ON FDT_RecID = FTR_FDTRecID
                                   ) STK04_FundTransactionPOSett ON FTR_TrnRecID = RMP_Hrecid
                                                                    AND FTR_RMPPayMode = RMP_PayMode
                                                                    AND FTR_RMPPaymentNo = RMP_PaymentNo "
            End If

            If TempPOSel = "" Then Str &= " WHERE RMP_Hrecid= '" & POCode & "'" 'edited by GS on 23/08/16 (add If TempPOSel = "")

            Execute(Str)
        Catch ex As Exception
            Throw ex
        End Try
    End Sub


    Public Sub PrintPO(ByVal frm As Form, ByVal TempTable As String, ByVal TempPOSubRep As String, ByVal Cond As String,
                       ByVal Cost As Integer, ByVal Mode As Integer, ByVal CurrFC As String, ByVal Fashion As Boolean,
                       Optional ByVal ShipInfo As Boolean = False, Optional ByVal PrintDirect As Boolean = False,
                       Optional ByVal NbPrint As Integer = 1, Optional ByVal TempPOSel As String = "", Optional ByVal locationCode As String = "", Optional ByVal servername As String = "" _
                       , Optional ByVal printTechcontent As Boolean = False, Optional PrintSig As Boolean = True, Optional ShowLineNumber As Boolean = False, Optional PrintFundDisbursement As Boolean = False) 'edited by GS on 23/08/16 (add TempPOSel) edited by toufic on 05/09/2016 (locationCode)
        '..If Mode = 0 THEN Detailed '..Simple ELSE Regular
        Dim strsql, strs As String
        Dim SQLstat As String
        Dim drSql As SqlDataReader
        Dim ReportName As String
        Dim strShip As String
        Dim strFashion As String
        Dim MaxCnt As Integer
        Dim hasCostcenter As Boolean
        Dim costcenterName, costcenterNumber As String
        Dim DTMails, DtpDf As DataTable
        Dim defaultReport As Boolean 'added by GS on 03/08/16
        Dim DRS0 As SqlDataReader
        Dim dr0 As DataRow
        Dim PrintFootRemark As Boolean
        Dim SeperateVATLCRate As Boolean
        Try

            Fashion = False
            'ch @ 26/09/2016 
            If (Not locationCode Is Nothing) AndAlso locationCode.Trim <> "" Then
                strsql = "SELECT DL_CostCenter FROM  dbo.STK04_LocAcDir WHERE DL_Loc ='" & locationCode.Trim & "'"
                If RecordsExists(strsql) Then
                    If ExecuteScalar(strsql).ToString.Trim <> "" Then
                        costcenterNumber = ExecuteScalar(strsql).ToString.Trim
                        strsql = " SELECT Aux_accno2+'-'+Aux_Name from " & servername & "ACC01_Auxiliary"
                        strsql &= " WHERE Aux_accno1 + right(space(4) + Aux_accno2,4) +Aux_subno ='" & costcenterNumber & "'AND Aux_Comp='" & gbComp & "'"
                        strsql &= " AND Aux_Year ='" & ACYear & "'"
                        costcenterName = ExecuteScalar(strsql).ToString
                        hasCostcenter = True
                    Else
                        hasCostcenter = False
                    End If
                Else
                    hasCostcenter = False
                End If
            Else
                hasCostcenter = False
            End If

            CreateView_V_PORD2_ManOrdDetRep(Fashion)

            strsql = "SELECT ISNULL(MAX(GD_Order), 0) FROM STK04_DetGRID"
            ReadFromDBToDataReader(strsql, drSql)
            drSql.Read()
            MaxCnt = CInt(drSql.Item(0))
            If MaxCnt > 30 Then MaxCnt = 30

            'If ShipInfo Then 
            strShip = GetShipInfoFields("TempPOShipInfo_" & gbUser, servername) 'modified by GS on 07/09/16 (add serverlink)
            If Fashion Then strFashion = GetFashionFields("PO", MaxCnt)

            SQLstat = "SELECT" & IIf(Fashion, " DISTINCT", "").ToString
            SQLstat &= " 0 + ROW_NUMBER() OVER ( ORDER BY mod_lineno, v.MOD_supitmref ASC, v.MOD_stat DESC, v.MOD_Count ASC ) [order] , PORD2_PORep.RMO_recid    AS [PO No.], CONVERT(DATETIME, CONVERT(VARCHAR, PORD2_PORep.RMO_date), 101) AS [Date], "
            SQLstat &= " CASE WHEN PORD2_PORep.RMO_POStatus ='U' THEN 'Under Process' WHEN PORD2_PORep.RMO_POStatus ='C' THEN 'Canceled'"
            SQLstat &= " WHEN PORD2_PORep.RMO_POStatus = 'F' THEN 'Finished' END AS [POStatus],"
            SQLstat &= " PORD2_PORep.sup_name AS [Sup. Name], PORD2_PORep.RMO_Cur AS [Curr],"
            SQLstat &= " PORD2_PORep.RMO_Cur + ' - ' + CUR_DESC AS [Currency],"
            SQLstat &= " convert(varchar(205),Rmo_IssLoc + ' - ' + LOC_DESC) AS [Issuing Loc],"
            If hasCostcenter = True Then
                SQLstat &= " convert(varchar(205),'" & costcenterName & "') AS [Cost Center],"
            ElseIf Cond = "" Then
                SQLstat &= "  case when isnull(DL_CostCenter,'') <>  '' then  convert(varchar(205), (SELECT top 1 Aux_accno2 COLLATE SQL_Latin1_General_CP1256_CI_AS +'-'+Aux_Name from " & servername & "ACC01_Auxiliary "
                SQLstat &= " WHERE Aux_accno1 + right(space(4) + Aux_accno2,4) COLLATE SQL_Latin1_General_CP1256_CI_AS +Aux_subno = DL_CostCenter     "
                SQLstat &= " AND Aux_Comp='" & gbComp & "' AND Aux_Year ='" & ACYear & "')) else convert(varchar(205),'') end  AS [Cost Center],"
            Else
                SQLstat &= " convert(varchar(205),'') AS [Cost Center],"
            End If


            SQLstat &= " Rmo_Pricetype + ' - ' + PRC_Desc AS [Price Type],"
            If Fashion Then SQLstat &= " V.Seas_Desc AS [Season], V.br_Name AS [Brand],"
            SQLstat &= " PORD2_PORep.Rmo_Disc AS [Itm Disc], PORD2_PORep.RMO_CashDisc AS [Inv. Disc],"
            'Abir T. @ 26.05.2014
            SQLstat &= " PORD2_PORep.RMO_VATFC AS VATFC, PORD2_PORep.RMO_VATLC AS VATLC,"
            If Cost = 1 Then SQLstat &= " PORD2_PORep.RMO_TotOrdval AS [Total Amount],"
            SQLstat &= " V.mod_supitmref AS [Supp. Ref.], V.STK_REF AS [Ref],"
            SQLstat &= " V.stk_supplier AS [Supplier], V.Stk_Category AS [Category],"
            SQLstat &= " V.Stk_Serie AS [Serie], V.Stk_newused AS [NewUsed], V.Stk_usage AS [Usage],"
            If Fashion Then SQLstat &= strFashion
            SQLstat &= " V.Stk_Desc AS [Description],"
            SQLstat &= " CASE WHEN V.MOD_stat = 'N' THEN 'NORMAL' WHEN V.MOD_Stat = 'F' THEN 'FREE' END AS [Status],"
            SQLstat &= " V.mod_ORDQty AS [Ord. Qty], V.Stk_SubUNIT AS [Unit],"
            If Cost = 1 Then
                If TempPOSel = "" Then 'added by GS on 23/08/16
                    '    If CurrFC = gbCurr1 Then
                    'SQLstat &= " CAST(V.mod_UnitCost AS DECIMAL(18, 0)) AS [Unit Cost In FC],"
                    'Else
                    SQLstat &= " ROUND(V.mod_UnitCost, 3) AS [Unit Cost In FC],"
                    ' End If
                    'added by GS on 23/08/16
                Else
                    SQLstat &= " CASE PORD2_PORep.RMO_cur WHEN '" & gbCurr1 & "' THEN CAST(V.mod_UnitCost AS DECIMAL(18, 0)) ELSE ROUND(V.mod_UnitCost, 3) END AS [Unit Cost In FC],"
                End If
                'GS on 23/08/16

                If Fashion = True Then
                    If gbCurr1 = gbCurr1 Then
                        SQLstat &= " CAST(dbo.convert5('" & gbApp & "', '" & gbComp & "', '" & gbYear & "', V.MOD_UnitCost , PORD2_PORep.RMO_Cur, '" & gbCurr1 & "', PORD2_PORep.RMO_date) AS DECIMAL(18, 0)) AS [Unit Cost LC],"
                    Else
                        SQLstat &= " ROUND((dbo.convert5('" & gbApp & "', '" & gbComp & "', '" & gbYear & "', V.MOD_UnitCost , PORD2_PORep.RMO_Cur, '" & gbCurr1 & "', PORD2_PORep.RMO_date), 3) AS [Unit Cost LC],"
                    End If
                    SQLstat &= " dbo.convert5('" & gbApp & "', '" & gbComp & "', '" & gbYear & "', V.MOD_UnitCost , PORD2_PORep.RMO_Cur, '" & gbCurr2 & "', PORD2_PORep.RMO_date) AS [Unit Cost CC],"
                End If
                SQLstat &= " V.mod_ItmDisc AS [Item Disc], "
                If TempPOSel = "" Or Fashion Then SQLstat &= " /*T.Total_Net*/ V.mod_NETTOT AS [Net Total], " Else SQLstat &= " /*T.MOD_NetTot*/ V.mod_NETTOT AS [Net Total], " 'SQLstat &= " T.Total_Net AS [Net Total], " 'edited by GS on 24/08/16
            End If
            SQLstat &= " MOD_Rem AS [Det. Remarks], RMO_app AS [App. No], RMO_ManualNo AS [Manual No],"
            SQLstat &= " PORD2_PORep.FO_addres AS [Address], PORD2_PORep.FO_Phone1 AS [Phone1], PORD2_PORep.FO_Phone2 AS [Phone2],"
            SQLstat &= " PORD2_PORep.FO_Phone3 AS [Phone3], PORD2_PORep.FO_Fax AS Fax, PORD2_PORep.FO_name AS [Folio Name],"
            SQLstat &= " RMO_remark AS [Remarks], User_Name AS [User Name],"
            SQLstat &= " CONVERT(DATETIME, CONVERT(VARCHAR, V.MOD_DelDate), 101) AS [Del. Date],"
            SQLstat &= " CONVERT(DATETIME, CONVERT(VARCHAR, RMO_ShipDate), 101) AS [Ship. Date]"

            If Fashion = True And Cost = 1 Then
                SQLstat &= " ,'" & gbCurr1 & "' AS LC, '" & gbCurr2 & "' AS CC"
            End If
            SQLstat &= strShip

            'Rachel A. 01.03.2016  adding company logo to report
            'modified by GS on 28/05/16
            'SQLstat &= " ,( SELECT L_Img FROM dbo.COm_Logo WHERE L_Comp='" & gbComp & "' ) [Logo] "
            'SQLstat &= " ,CONVERT(VARBINARY(MAX),( SELECT L_Img FROM dbo.COm_Logo WHERE L_Comp='" & gbComp & "' )) [Logo] " 'removed by GS on 15/09/16 (img should be imported from report)
            'GS on 28/05/16
            SQLstat &= " ,Fo_attention ,RMO_NeedCEOApproval ,RMO_CEOApproved " 'added by GS on 03/08/16
            'ch @ 28/10/2016 
            SQLstat &= " , RMO_HOApproved,RMO_ControlApproved  "

            SQLstat &= " , RMO_AppUstamp AppUstamp ,( select User_Name from Com_UserInfo  where RMO_AppUstamp=User_Code) RMO_AppUstamp ,RMO_AppDstamp " 'added by GS on 12/08/16
            SQLstat &= " ,        COALESCE(SFP_TechSheet,DSTech_Meas,'') [TechContent] " 'CH @ 17/01/2017 WF: 4867
            SQLstat &= " ,          COALESCE(case when stk_vattype='E' then 0.00 else stk_vatper end ,case when sfp_vattype='E' then 0.00 else  SFP_vatper end , 0)  [VatPer] " 'CH @ 11/04/2017
            SQLstat &= " ,    RMO_AcceptDate,RMO_AcceptInfo " 'CH @ 13/04/2017
            SQLstat &= " ,PORD2_PORep.Fo_Email " 'ch @ 24/04/2017
            'ch @ 06/11/2017 wf : 6052
            SQLstat &= "   ,RMO_ControlApprovalUSer ControlApprovalUSer,( select User_Name from Com_UserInfo  where RMO_ControlApprovalUSer=User_Code)RMO_ControlApprovalUSer   , RMO_ControlApprovalDate , "
            SQLstat &= "  RMO_HeadApprovalUser HeadApprovalUser,  ( select User_Name from Com_UserInfo  where RMO_HeadApprovalUser=User_Code)   RMO_HeadApprovalUser,  RMO_HeadApprovalDate "
            SQLstat &= "   , RMO_CEOApprovalUser CEOApprovalUser ,( select User_Name from Com_UserInfo  where RMO_CEOApprovalUser=User_Code)   RMO_CEOApprovalUser,  RMO_CEOApprovalDate "
            SQLstat &= " ,FLQTY, NLQTY, FWQTY, NWQTY, FPQTY, NPQTY "
            If gbFundDisbursementModuleIsActive And PrintFundDisbursement Then
                SQLstat &= ", V.MOD_Hrecid "
            End If
            SQLstat &= " FROM V_PORD2_ManOrdDetRep_" & gbUser & " V"
            If TempPOSel = "" Then 'added by GS on 23/08/16
                SQLstat &= " left JOIN " & TempTable & " T ON V.MOD_Stat = T.MOD_stat"
                'added by GS on 23/08/16
            Else
                If Fashion = False Then
                    SQLstat &= " INNER JOIN (Select MOD_Hrecid, mod_ORDQty, Stk_SubUNIT, mod_UnitCost,mod_deldate, mod_ItmDisc, MOD_stat, MOD_supitmref, MOD_Count,MOD_NetTot from V_PORD2_MODETGRID inner join " & TempPOSel & " TempPOSel on MOD_HRecid = RMO_Recid and Chk = 'True') AS T ON V.MOD_Stat = T.MOD_stat" 'edited by GS on 08/09/16 (add MOD_Hrecid)
                Else
                    SQLstat &= " left JOIN " & TempTable & " T ON V.MOD_Stat = T.MOD_stat"
                End If
            End If
            'GS on 23/08/16
            SQLstat &= " AND V.MOD_SupItmRef = T.MOD_supitmref"
            If Fashion Then
                SQLstat &= " AND V.MOD_GridCode = T.GridCode"
                SQLstat &= " AND V.MOD_Color = T.Color"
                For K = 0 To CurrItmKey.Length - 1
                    SQLstat &= " AND V." & CurrItmKey(K) & " = T." & CurrItmKey(K)
                Next
            Else
                SQLstat &= " AND V.MOD_Count = T.MOD_Count"
            End If
            If TempPOSel <> "" Then SQLstat &= " AND V.mod_Hrecid = T.MOD_Hrecid " 'added by GS on 08/09/16
            'SQLstat &= " INNER JOIN V_PORD2_ManOrdHRep on V.mod_Hrecid = Rmo_Recid"
            SQLstat &= " INNER JOIN PORD2_PORep ON V.mod_Hrecid = Rmo_Recid"
            SQLstat &= " INNER JOIN stk04_PriceType PriceType On Rmo_Pricetype = PriceType.PRC_Code"
            SQLstat &= " LEFT JOIN TempPOShipInfo_" & gbUser & " ShipInfo on RMO_recid = POSI_PohRecID"
            If TempPOSel = "" Then 'added by GS on 02/09/16
                If Fashion Then SQLstat &= " LEFT JOIN TempGridSizeSP_PurchaseOrder" & IIf(Fashion, "Fashion", "").ToString & gbUser & " GS ON GS.G_Code = V.MOD_GridCode"
                'added by GS on 02/09/16
            Else
                If Fashion Then SQLstat &= " LEFT JOIN TempGridSizeSP_RepPurchaseOrder" & IIf(Fashion, "Fashion", "").ToString & gbUser & " GS ON GS.G_Code = V.MOD_GridCode"
            End If
            'GS on 02/09/16
            SQLstat &= " Left Join Com_Folio on Loc_Folio = Fo_serno and Loc_FolioSub = Fo_subno" 'added by GS on 03/08/16
            SQLstat &= "  LEFT JOIN dbo.STK04_STKFIL ON STK_RECID=V.mod_RMRecid " 'CH @ 17/01/2017 WF: 4867
            SQLstat &= "  LEFT JOIN dbo.STK04_DSTechnical ON STK04_DSTechnical.DSTech_RecId = STK04_STKFIL.STK_RECID" 'CH @ 17/01/2017 WF: 4867
            SQLstat &= "  LEFT JOIN dbo.STK04_StkFilPending ON V.MOD_PENDRecid=SFP_Recid" 'CH @ 17/01/2017 WF: 4867
            If TempPOSel = "" Then 'added by GS on 23/08/16
                If Cond <> "" Then SQLstat &= " WHERE " & Cond
                'added by GS on 23/08/16
            Else
                SQLstat &= " INNER JOIN " & TempPOSel & " TempPOSel on TempPOSel.RMO_Recid = PORD2_PORep.RMO_Recid and Chk = 'True' "
            End If
            If hasCostcenter = False And Cond = "" Then
                SQLstat &= " left join dbo.STK04_LocAcDir on  DL_Loc = RMO_IssLoc "
            End If
            'GS on 23/08/16
            ReadFromDBToDataReader(SQLstat, drSql)
            If Not drSql.HasRows Then
                MessageBox.Show("No Records Found.", frm.Text, MessageBoxButtons.OK, MessageBoxIcon.Information)
                Exit Sub
            End If
            'export purchase by mail
            Dim strsqlnbprt As String
            Dim NbrCopies As String
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
                .Columns.Add("RecID", GetType(System.String)) 'Purchase recid 
                .Columns.Add("PdfFilter", GetType(System.String)) ' Spesific Filtration For The Pdf File 
                .Columns.Add("AttachName", GetType(System.String)) ' Pdf File name 
                .PrimaryKey = New DataColumn() { .Columns("RecID")}
            End With
            'Case of Crystal Mail xx ------
            DTMails.Clear()
            strs = " SELECT *,    ROW_NUMBER() OVER ( ORDER BY Mail ) [Records] FROM  ( SELECT distinct * FROM  ( "
            strs &= " SELECT distinct  'True'  [Check],'' [A/C],PORD2_PORep.sup_name Name,isnull(Fo_Email,'') Mail,"
            strs &= " '' Status  "
            strs &= " FROM    V_PORD2_ManOrdDetRep_" & gbUser & " V"
            If TempPOSel = "" Then
                strs &= " left JOIN " & TempTable & " T ON V.MOD_Stat = T.MOD_stat"
            Else
                If Fashion = False Then
                    strs &= " INNER JOIN (Select MOD_Hrecid, mod_ORDQty, Stk_SubUNIT, mod_UnitCost,mod_deldate, mod_ItmDisc, MOD_stat, MOD_supitmref, MOD_Count,MOD_NetTot from V_PORD2_MODETGRID inner join " & TempPOSel & " TempPOSel on MOD_HRecid = RMO_Recid and Chk = 'True' AND STK_ItmClass <> 'F') AS T ON V.MOD_Stat = T.MOD_stat" 'edited by GS on 08/09/16 (add MOD_Hrecid)
                Else
                    strs &= " Left JOIN " & TempTable & " T ON V.MOD_Stat = T.MOD_stat"
                End If
            End If
            strs &= " AND V.MOD_SupItmRef = T.MOD_supitmref"
            If Fashion Then
                strs &= " AND V.MOD_GridCode = T.GridCode"
                strs &= " AND V.MOD_Color = T.Color"
                For K = 0 To CurrItmKey.Length - 1
                    strs &= " AND V." & CurrItmKey(K) & " = T." & CurrItmKey(K)
                Next
            Else
                strs &= " AND V.MOD_Count = T.MOD_Count"
            End If
            If TempPOSel <> "" Then strs &= " AND V.mod_Hrecid = T.MOD_Hrecid "
            strs &= " INNER JOIN PORD2_PORep ON V.mod_Hrecid = Rmo_Recid"
            If TempPOSel = "" Then
                If Cond <> "" Then strs &= " WHERE " & Cond
            Else
                strs &= " INNER JOIN " & TempPOSel & " TempPOSel on TempPOSel.RMO_Recid = PORD2_PORep.RMO_Recid and Chk = 'True' "
            End If
            strs &= " GROUP BY Fo_Email ,  PORD2_PORep.sup_name"
            strs &= " union all"
            strs &= " SELECT distinct  'True'  [Check],'' [A/C],PORD2_PORep.sup_name Name,isnull(FOM_Email,'') Mail,"
            strs &= " '' Status "
            strs &= "   FROM    V_PORD2_ManOrdDetRep_" & gbUser & " V"
            If TempPOSel = "" Then
                strs &= " left JOIN " & TempTable & " T ON V.MOD_Stat = T.MOD_stat"
            Else
                If Fashion = False Then
                    strs &= " INNER JOIN (Select MOD_Hrecid, mod_ORDQty, Stk_SubUNIT, mod_UnitCost,mod_deldate, mod_ItmDisc, MOD_stat, MOD_supitmref, MOD_Count,MOD_NetTot from V_PORD2_MODETGRID inner join " & TempPOSel & " TempPOSel on MOD_HRecid = RMO_Recid and Chk = 'True' ) AS T ON V.MOD_Stat = T.MOD_stat" 'edited by GS on 08/09/16 (add MOD_Hrecid)
                Else
                    strs &= " Left JOIN " & TempTable & " T ON V.MOD_Stat = T.MOD_stat"
                End If
            End If
            strs &= " AND V.MOD_SupItmRef = T.MOD_supitmref"
            If Fashion Then
                strs &= " AND V.MOD_GridCode = T.GridCode"
                strs &= " AND V.MOD_Color = T.Color"
                For K = 0 To CurrItmKey.Length - 1
                    strs &= " AND V." & CurrItmKey(K) & " = T." & CurrItmKey(K)
                Next
            Else
                strs &= " AND V.MOD_Count = T.MOD_Count"
            End If
            If TempPOSel <> "" Then strs &= " AND V.mod_Hrecid = T.MOD_Hrecid "
            strs &= "  INNER JOIN PORD2_PORep ON V.mod_Hrecid = Rmo_Recid"
            strs &= "  LEFT JOIN dbo.Com_FolioMail ON   Fo_RecId = FOM_RecId "
            If TempPOSel = "" Then
                If Cond <> "" Then strs &= " WHERE " & Cond
                If Cond <> "" Then strs &= " and     isnull(FOM_Email,'')<>''  "
            Else
                strs &= " INNER JOIN " & TempPOSel & " TempPOSel on TempPOSel.RMO_Recid = PORD2_PORep.RMO_Recid and Chk = 'True' "
                strs &= " where      isnull(FOM_Email,'')<>''  "
            End If

            strs &= "       GROUP BY FOM_Email , FOM_RecId , PORD2_PORep.sup_name ) T )T1"
            ReadFromDBToDataReader(strs, DRS0)
            If DRS0.HasRows Then
                While DRS0.Read
                    dr0 = DTMails.NewRow
                    dr0("Check") = True
                    dr0("Name") = DRS0.Item("Name")
                    dr0("Mail") = DRS0.Item("Mail")
                    dr0("Filter") = "[Sup. Name]= '" & Replace(DRS0("Name").ToString, "'", "''") & "'"
                    dr0("Locked") = "Y"
                    dr0("PdfFilterToLinkDTMails") = "name= '" & Replace(DRS0.Item("Name").ToString, "'", "''") & "'"
                    dr0("Records") = DRS0("Records").ToString
                    DTMails.Rows.Add(dr0)
                End While
            End If
            DRS0.Close()
            DtpDf.Clear()
            strs = " SELECT distinct  PORD2_PORep.RMO_Recid Recid ,  PORD2_PORep.sup_name Name,PORD2_PORep.RMO_date Date  "
            strs &= "  FROM    V_PORD2_ManOrdDetRep_" & gbUser & " V"
            If TempPOSel = "" Then
                strs &= " left JOIN " & TempTable & " T ON V.MOD_Stat = T.MOD_stat"
            Else
                If Fashion = False Then
                    strs &= " INNER JOIN (Select MOD_Hrecid, mod_ORDQty, Stk_SubUNIT, mod_UnitCost,mod_deldate, mod_ItmDisc, MOD_stat, MOD_supitmref, MOD_Count,MOD_NetTot from V_PORD2_MODETGRID inner join " & TempPOSel & " TempPOSel on MOD_HRecid = RMO_Recid and Chk = 'True' AND STK_ItmClass <> 'F') AS T ON V.MOD_Stat = T.MOD_stat" 'edited by GS on 08/09/16 (add MOD_Hrecid)
                Else
                    strs &= " Left JOIN " & TempTable & " T ON V.MOD_Stat = T.MOD_stat"
                End If
            End If
            strs &= " AND V.MOD_SupItmRef = T.MOD_supitmref"
            If Fashion Then
                strs &= " AND V.MOD_GridCode = T.GridCode"
                strs &= " AND V.MOD_Color = T.Color"
                For K = 0 To CurrItmKey.Length - 1
                    strs &= " AND V." & CurrItmKey(K) & " = T." & CurrItmKey(K)
                Next
            Else
                strs &= " AND V.MOD_Count = T.MOD_Count"
            End If
            If TempPOSel <> "" Then strs &= " AND V.mod_Hrecid = T.MOD_Hrecid "
            strs &= " INNER JOIN PORD2_PORep ON V.mod_Hrecid = Rmo_Recid"

            If TempPOSel = "" Then
                If Cond <> "" Then strs &= " WHERE " & Cond
            Else
                strs &= " INNER JOIN " & TempPOSel & " TempPOSel on TempPOSel.RMO_Recid = PORD2_PORep.RMO_Recid and Chk = 'True' "
            End If

            SeperateVATLCRate = (ExecuteScalar_WOXACT("SELECT isnull(SP_SeperateVATLCRate,'N') FROM stk04_SysParam").ToString = "Y")

            ReadFromDBToDataReader(strs, DRS0)
            If DRS0.HasRows Then
                While DRS0.Read
                    dr0 = DtpDf.NewRow
                    dr0("Name") = DRS0.Item("Name")
                    dr0("RecID") = DRS0.Item("RecID")
                    dr0("PdfFilter") = "({Command.PO No.}=" & DRS0("Recid").ToString & ")"
                    dr0("AttachName") = "PO " & DRS0.Item("Recid").ToString & "-" & DRS0.Item("Name").ToString & "-" & CStr(CDate(DRS0.Item("Date").ToString).ToString("dd-MM-yyyy"))
                    DtpDf.Rows.Add(dr0)
                End While
            End If
            DRS0.Close()
            If Fashion = True Then GoTo defaultReportName
            ReportName = ExecuteScalar("select SP_POReport from stk04_SysParam")
            If Cost <> 1 And ReportName.Length >= 4 Then ReportName = ReportName.Substring(0, ReportName.Length - 4) & "_QTY.rpt"
            defaultReport = False 'added by GS on 03/08/16
            'ch @ 23/01/2018 Add Flag To Report OPP do not Print Footer Remark in Report Used Only To Hide This Section and print the default report mail (FW: ERP_Purchase_Print PI: Special Layout for OPP)
            If ReportName = "PORd2_ManOrdOPP_QTY.rpt" OrElse ReportName = "PORd2_ManOrdOPP.rpt" Then
                PrintFootRemark = False
            Else
                PrintFootRemark = True
            End If
            GoTo testReportName
            'GS on 03/08/16

defaultReportName:

            'Modified by Abir T. @ 28.06.2017
            If Cost = 1 Then ReportName = "PORd2_ManOrd.rpt" Else ReportName = "PORd2_ManOrd_QTY.rpt"

            If PrintFundDisbursement And Cost <> 0 Then
                ReportName = "PORd2_ManOrd_WithFunds.rpt"
            End If

            If Fashion = True Then
                ReportName = ReportName.Substring(0, ReportName.Length - 4)
                ReportName = ReportName + "Fashion.rpt"
            End If
            defaultReport = True 'added by GS on 03/08/16

testReportName:

            '''if in system file, report name is filled
#If DEBUG Then
            If Not File.Exists(gbExePath & "\..\..\PurReports\" & ReportName) Then GoTo defaultReportName
#Else
            If Not File.Exists(gbExePath & "\PurReports\" & ReportName) Then GoTo defaultReportName
#End If

            If PrintDirect = True Then
                PrintPODirectRep(frm, SQLstat, ReportName, TempPOSubRep, Cost, Mode, ShipInfo, Fashion, NbPrint, defaultReport, printTechcontent, DTMails, True, DtpDf, False, "   and XXFiltrationXX  ", PrintSig, ShowLineNumber, PrintFootRemark,
                                 FundsQuery(TempPOSel, PrintFundDisbursement, "PO", Cond), SeperateVATLCRate) 'edited by GS on 03/08/16 (add defaultReport)
            Else
                PrintPOWithPrevGrid(frm, SQLstat, ReportName, TempPOSubRep, Cost, Mode, CurrFC, ShipInfo, MaxCnt, Fashion, NbPrint, defaultReport, printTechcontent, DTMails, True, DtpDf, False, " and XXFiltrationXX ", hasCostcenter, PrintSig, ShowLineNumber, PrintFootRemark,
                                    FundsQuery(TempPOSel, PrintFundDisbursement, "PO", Cond), PrintFundDisbursement, SeperateVATLCRate) 'edited by GS on 01/04/16 (add NbPrint) 'edited by GS on 03/08/16 (add defaultReport)
            End If
        Catch exp As Exception
            Throw exp
        Finally
            If Not frm Is Nothing Then frm = Nothing

            'Cursor.Current = Cursors.Default
            If Not drSql Is Nothing Then
                If drSql.IsClosed = False Then drSql.Close()
                drSql = Nothing
            End If

            '  DropSqlObject("TempPOShipInfo_" & gbUser, "TABLE")
            Cursor.Current = Cursors.Default
        End Try
    End Sub
    'Print Report With Preview Grid
    Public Sub PrintPOWithPrevGrid(ByVal frmPO As Form, ByVal SQLstat As String, ByVal ReportName As String, ByVal TempPOSubRep As String, ByVal Cost As Integer, ByVal Mode As Integer, ByVal CurrFC As String, ByVal ShipInfo As Boolean, ByVal MaxCnt As Integer, ByVal Fashion As Boolean, Optional ByVal NbPrint As Integer = 1, Optional ByVal defaultReport As Boolean = True _
                                   , Optional ByVal techContent As Boolean = False, Optional ByVal DTMails As DataTable = Nothing,
                                    Optional Separated As Boolean = False, Optional ByVal DTpdf As DataTable = Nothing,
                                    Optional AllowAddNewRow As Boolean = False, Optional condSendMail As String = "", Optional hasCostcenter As Boolean = False, Optional PrintSig As Boolean = True, Optional ShowLineNumber As Boolean = False, Optional PrintFootRemark As Boolean = True,
                                   Optional FundQuery As String = "", Optional PrintFundDisbursement As Boolean = False, Optional SeperateVATLCRate As Boolean = False)  'edited by GS on 01/04/16 (add nbprint param) 'edited by GS on 03/08/16 (add defaultReport)
        Dim frm As frmPreviewGrid
        Dim strsql As String
        Dim drSql As SqlDataReader
        Try
            Cursor.Current = Cursors.WaitCursor
            frm = New frmPreviewGrid
            With frm
                .withLogo = True 'added by GS on 15/09/16
                .Maildt = DTMails
                .AllowAddNewRow = AllowAddNewRow
                .DTpdf = DTpdf
                .Separated = True
                frm.BuildSql(SQLstat)
                .ColDesign("FLQTY", 0, "Free Length QTY", AlignHorzEnum.Far, AlignVertEnum.Top, False, , , , , False,, , , , , , , , , , , "Free Length QTY")
                .ColDesign("NLQTY", 0, "Normal Length QTY", AlignHorzEnum.Far, AlignVertEnum.Top, False, , , , False, False, , , , , , , , , , , , "Normal Length QTY")
                .ColDesign("FWQTY", 0, "Free Weight QTY", AlignHorzEnum.Far, AlignVertEnum.Top, False, , , , False, False, , , , , , , , , , , , "Free Weight QTY")
                .ColDesign("NWQTY", 0, "Normal Weight QTY", AlignHorzEnum.Far, AlignVertEnum.Top, False, , , , False, False, , , , , , , , , , , , "Normal Weight QTY")
                .ColDesign("FPQTY", 0, "Free Piece QTY", AlignHorzEnum.Far, AlignVertEnum.Top, False, , , , False, False, , , , , , , , , , , , "Free Piece QTY")
                .ColDesign("NPQTY", 0, "Normal Piece QTY", AlignHorzEnum.Far, AlignVertEnum.Top, False, , , , False, False, , , , , , , , , , , , "Normal Piece QTY")

                .ColDesign("Fo_attention", 0, "Fo_attention", AlignHorzEnum.Near, AlignVertEnum.Top, False, , , , , False, , , , , , , , , , , False) 'added by GS on 03/08/16
                .ColDesign("RMO_NeedCEOApproval", 0, "RMO_NeedCEOApproval", AlignHorzEnum.Near, AlignVertEnum.Top, False, , , , , False, , , , , , , , , , , False) 'added by GS on 03/08/16
                .ColDesign("RMO_CEOApproved", 0, "RMO_CEOApproved", AlignHorzEnum.Near, AlignVertEnum.Top, False, , , , , False, , , , , , , , , , , False) 'added by GS on 03/08/16
                .ColDesign("RMO_AppDstamp", 0, "RMO_AppDstamp", AlignHorzEnum.Near, AlignVertEnum.Top, False, , , , , False, , , , , , , , , , , False) 'added by GS on 12/08/16
                .ColDesign("RMO_AppUstamp", 0, "RMO_AppUstamp", AlignHorzEnum.Near, AlignVertEnum.Top, False, , , , , False, , , , , , , , , , , False) 'added by GS on 12/08/16
                .ColDesign("PO No.", -1, "PO No.", AlignHorzEnum.Center, AlignVertEnum.Top, , , , , True, , frmPreviewGrid.Orders.Ascending)
                .ColDesign("Date", -1, "Date", AlignHorzEnum.Near, AlignVertEnum.Top, , , , , True, , frmPreviewGrid.Orders.Ascending, , , , "dd/MM/yyyy")
                .ColDesign("POStatus", -1, "POStatus", AlignHorzEnum.Near, AlignVertEnum.Top, , , , , True)
                .ColDesign("Sup. Name", -1, "Sup. Name", AlignHorzEnum.Near, AlignVertEnum.Top, , , , , True)
                .ColDesign("Curr", 0, "Curr", AlignHorzEnum.Near, AlignVertEnum.Top, False, , , , , False, , , , , , , , , , , False)
                .ColDesign("RMO_AppUstamp", 0, "RMO_AppUstamp", AlignHorzEnum.Near, AlignVertEnum.Top, False, , , , , False, , , , , , , , , , , False)
                .ColDesign("Currency", -1, "Currency", AlignHorzEnum.Center, AlignVertEnum.Top, , , , , True)
                .ColDesign("Issuing Loc", -1, "Ordering Loc", AlignHorzEnum.Center, AlignVertEnum.Top, , , , , True)
                If hasCostcenter = True Then
                    .ColDesign("Cost Center", -1, "CC/project", AlignHorzEnum.Center, AlignVertEnum.Top, , , , , True)
                Else
                    .ColDesign("Cost Center", 0, "CC/project", AlignHorzEnum.Center, AlignVertEnum.Top, , , , , True, False)
                End If
                .ColDesign("Price Type", -1, "Price Type", AlignHorzEnum.Center, AlignVertEnum.Top, , , , , True)
                'End If
                If Fashion Then
                    .ColDesign("Season", -1, "Season", AlignHorzEnum.Near, AlignVertEnum.Top, , , , , True)
                    .ColDesign("Brand", -1, "Brand", AlignHorzEnum.Near, AlignVertEnum.Top, , , , , True)
                End If
                '.ColDesign("Disc", 80, "Disc", AlignHorzEnum.Far, , , , , , , CBool(IIf(Mode = 1, False, True)), , , , , , , , , , , CBool(IIf(Mode = 1, False, True)))
                .ColDesign("Itm Disc", 0, "Itm Disc", AlignHorzEnum.Far, AlignVertEnum.Top, False, , , , True, False, , , , , , , , , , , False)
                .ColDesign("Inv. Disc", 0, "Inv. Disc", AlignHorzEnum.Far, AlignVertEnum.Top, False, , , , True, False, , , , , , , , , , , False)
                If Cost = 1 Then .ColDesign("Total Amount", 80, "Total Amount", AlignHorzEnum.Far, AlignVertEnum.Top, , , , , True)
                .ColDesign("Supp. Ref.", -1, "Supp. Ref.", , , , , , , , , )
                .ColDesign("Ref", -1, "Ref", AlignHorzEnum.Near)
                'If Mode <> 1 Then
                .ColDesign("Supplier", 0, "Supplier", AlignHorzEnum.Near, , False, , , , , False, , , , , , , , , , , False)
                .ColDesign("Category", 0, "Category", AlignHorzEnum.Near, , False, , , , , False, , , , , , , , , , , False)
                .ColDesign("Serie", 0, "Serial", AlignHorzEnum.Near, , False, , , , , False, , , , , , , , , , , False)
                .ColDesign("NewUsed", 0, "NewUsed", AlignHorzEnum.Near, , False, , , , , False, , , , , , , , , , , False)
                .ColDesign("Usage", 0, "Usage", AlignHorzEnum.Near, , False, , , , , False, , , , , , , , , , , False)
                'End If
                If Fashion Then
                    .ColDesign("Supp. Color", 80, "Supp. Color", AlignHorzEnum.Near)
                    .ColDesign("Gender", 100, "Gender", AlignHorzEnum.Near)
                    .ColDesign("Product Group", 100, "Product Group", AlignHorzEnum.Near)
                    .ColDesign("Style", 60, "Style", AlignHorzEnum.Near)
                    .ColDesign("Color", 60, "Color", AlignHorzEnum.Near)
                    .ColDesign("Grid", 60, "Grid", AlignHorzEnum.Near)

                    For i As Integer = 1 To MaxCnt
                        .ColDesign("Size" & i, 60, "Size" & i, AlignHorzEnum.Center, , , , , , , False)
                    Next
                    If MaxCnt < 30 Then
                        For i As Integer = (MaxCnt + 1) To 30
                            .ColDesign("Size" & i, 60, "Size" & i, AlignHorzEnum.Center, , , , , , , False, , , , , , , , , , , False)
                        Next
                    End If
                    For j As Integer = 1 To MaxCnt
                        .ColDesign("Qty" & j, 60, "Qty" & j, AlignHorzEnum.Center)
                    Next
                    If MaxCnt < 30 Then
                        For j As Integer = (MaxCnt + 1) To 30
                            .ColDesign("Qty" & j, 60, "Qty" & j, AlignHorzEnum.Center, , , , , , , False, , , , , , , , , , , False)
                        Next
                    End If
                End If
                .ColDesign("Description", 120, "Description", AlignHorzEnum.Near)
                .ColDesign("Status", 60, "Status", , , , , , , , , )
                .ColDesign("Ord. Qty", 80, "Ord. Qty", AlignHorzEnum.Far)
                .ColDesign("Unit", 50, "Unit", AlignHorzEnum.Center)
                If Cost = 1 Then
                    .ColDesign("Unit Cost In FC", 100, "Unit Cost In " & CurrFC & "", AlignHorzEnum.Far, , , , , , , , , , , , , , , , , , , "Unit Cost In " & CurrFC)
                    .ColDesign("Item Disc", 80, "Item Disc", AlignHorzEnum.Far)
                    .ColDesign("Net Total", 80, "Net Total", AlignHorzEnum.Far)
                End If
                '.ColDesign("Det. Remarks", 120, "Det. Remarks", AlignHorzEnum.Near)
                '.ColDesign("App. No", 60, "App. No", AlignHorzEnum.Near, AlignVertEnum.Top, , , , , True)
                '' If Mode = 0 Then
                '.ColDesign("Manual No", 100, "Manual No", AlignHorzEnum.Near, AlignVertEnum.Top, , , , , True)
                '' Else
                '.ColDesign("Address", 120, "Address", AlignHorzEnum.Near, AlignVertEnum.Top, , , , , True)
                '.ColDesign("Phone1", 60, "Phone1", AlignHorzEnum.Near, AlignVertEnum.Top, , , , , True)
                '.ColDesign("Phone2", 60, "Phone2", AlignHorzEnum.Near, AlignVertEnum.Top, , , , , True)
                '.ColDesign("Phone3", 60, "Phone3", AlignHorzEnum.Near, AlignVertEnum.Top, , , , , True)
                '.ColDesign("Fax", 120, "Fax", AlignHorzEnum.Near, AlignVertEnum.Top, , , , , True)
                '.ColDesign("Folio Name", 100, "Folio Name", AlignHorzEnum.Near, , , , , , True)
                '' End If
                '.ColDesign("Remarks", 120, "Remarks", AlignHorzEnum.Near, AlignVertEnum.Top, , , , , True)
                '.ColDesign("User Name", 60, "User Name", AlignHorzEnum.Near, AlignVertEnum.Top, , , , , True)
                '.ColDesign("Del. Date", 80, "Del. Date", AlignHorzEnum.Near, AlignVertEnum.Top, , , , , True, , , , , , "dd/MM/yyyy")
                '.ColDesign("Ship. Date", 80, "Ship. Date", AlignHorzEnum.Near, AlignVertEnum.Top, , , , , True, , , , , , "dd/MM/yyyy")
                .ColDesign("Det. Remarks", 0, "Det. Remarks", AlignHorzEnum.Near, , False, , , , False, False, , , , , , , , , , , False)
                .ColDesign("App. No", 0, "App. No", AlignHorzEnum.Near, AlignVertEnum.Top, False, , , , False, False, , , , , , , , , , , False)
                .ColDesign("Manual No", 0, "Manual No", AlignHorzEnum.Near, AlignVertEnum.Top, False, , , , False, False, , , , , , , , , , , False)
                .ColDesign("Address", 0, "Address", AlignHorzEnum.Near, AlignVertEnum.Top, False, , , , False, False, , , , , , , , , , , False)
                .ColDesign("Phone1", 0, "Phone1", AlignHorzEnum.Near, AlignVertEnum.Top, False, , , , False, False, , , , , , , , , , , False)
                .ColDesign("Phone2", 0, "Phone2", AlignHorzEnum.Near, AlignVertEnum.Top, False, , , , False, False, , , , , , , , , , , False)
                .ColDesign("Phone3", 0, "Phone3", AlignHorzEnum.Near, AlignVertEnum.Top, False, , , , False, False, , , , , , , , , , , False)
                .ColDesign("Fax", 0, "Fax", AlignHorzEnum.Near, AlignVertEnum.Top, False, , , , False, False, , , , , , , , , , , False)
                .ColDesign("Folio Name", 0, "Folio Name", AlignHorzEnum.Near, , False, , , , False, False, , , , , , , , , , , False)
                .ColDesign("Remarks", 0, "Remarks", AlignHorzEnum.Near, AlignVertEnum.Top, False, , , , False, False, , , , , , , , , , , False)
                .ColDesign("User Name", 0, "User Name", AlignHorzEnum.Near, AlignVertEnum.Top, False, , , , False, False, , , , , , , , , , , False)
                .ColDesign("Del. Date", 0, "Del. Date", AlignHorzEnum.Near, AlignVertEnum.Top, False, , , , False, False, , , , , , , , , , , False)
                .ColDesign("Ship. Date", 0, "Ship. Date", AlignHorzEnum.Near, AlignVertEnum.Top, False, , , , False, False, , , , , , , , , , , False)
                If Fashion = True And Cost = 1 Then
                    .ColDesign("Unit Cost LC", 100, "Unit Cost In " & gbCurr1, AlignHorzEnum.Far, AlignVertEnum.Top, , , , , , CBool(IIf(gbCurr1 = CurrFC, False, True)), , , , , "N" & VatLCRound, , , , , , , "Unit Cost In " & gbCurr1)
                    .ColDesign("Unit Cost CC", 100, "Unit Cost In " & gbCurr2, AlignHorzEnum.Far, AlignVertEnum.Top, , , , , , CBool(IIf(gbCurr2 = CurrFC, False, True)), , , , , "#,##0.000", , , , , , , "Unit Cost In " & gbCurr2)
                    .ColDesign("LC", 80, "LC", AlignHorzEnum.Near, AlignVertEnum.Top, False, , , , , False)
                    .ColDesign("CC", 80, "CC", AlignHorzEnum.Near, AlignVertEnum.Top, False, , , , , False)
                End If

                'Abir T. @ 27.05.2014
                .ColDesign("VATLC", 0, "VAT " & gbCurr1, AlignHorzEnum.Far, AlignVertEnum.Top, False, , , , True, False, , , , , "#,##0.000", , , , , , False, "VAT " & gbCurr1)
                .ColDesign("VATFC", 0, "VAT " & CurrFC, AlignHorzEnum.Far, AlignVertEnum.Top, False, , , , True, False, , , , , "#,##0.000", , , , , , False, "VAT " & CurrFC)

                'ShipInfo Columns
                .ColDesign("Ship Mode", 60, "Ship Mode", AlignHorzEnum.Near, AlignVertEnum.Top, ShipInfo, , , , True, ShipInfo)
                .ColDesign("Truck Name", 80, "Truck Name", AlignHorzEnum.Near, AlignVertEnum.Top, ShipInfo, , , , True, ShipInfo)
                .ColDesign("Consignee", 80, "Consignee", AlignHorzEnum.Near, AlignVertEnum.Top, ShipInfo, , , , True, ShipInfo)
                .ColDesign("Notify", 80, "Notify", AlignHorzEnum.Near, AlignVertEnum.Top, ShipInfo, , , , True, ShipInfo)
                .ColDesign("Shipper", 80, "Shipper", AlignHorzEnum.Near, AlignVertEnum.Top, ShipInfo, , , , True, ShipInfo)
                .ColDesign("Shipped To", 80, "Shipped To", AlignHorzEnum.Near, AlignVertEnum.Top, ShipInfo, , , , True, ShipInfo)
                .ColDesign("Sold To", 80, "Sold To", AlignHorzEnum.Near, AlignVertEnum.Top, ShipInfo, , , , True, ShipInfo)
                .ColDesign("Payment To", 80, "Payment To", AlignHorzEnum.Near, AlignVertEnum.Top, ShipInfo, , , , True, ShipInfo)
                .ColDesign("Port OfLoad", 80, "Port OfLoad", AlignHorzEnum.Near, AlignVertEnum.Top, ShipInfo, , , , True, ShipInfo)
                .ColDesign("Master BL", 80, "Master BL", AlignHorzEnum.Near, AlignVertEnum.Top, ShipInfo, , , , True, ShipInfo)
                .ColDesign("House BL", 80, "House BL", AlignHorzEnum.Near, AlignVertEnum.Top, ShipInfo, , , , True, ShipInfo)
                .ColDesign("Delivery", 80, "Delivery", AlignHorzEnum.Near, AlignVertEnum.Top, ShipInfo, , , , True, ShipInfo)
                .ColDesign("Validity", 80, "Validity", AlignHorzEnum.Near, AlignVertEnum.Top, ShipInfo, , , , True, ShipInfo)
                .ColDesign("Bank Details", 80, "Bank Details", AlignHorzEnum.Near, AlignVertEnum.Top, ShipInfo, , , , True, ShipInfo)
                .ColDesign("Aux. Acc.", 80, "Aux. Acc.", AlignHorzEnum.Near, AlignVertEnum.Top, ShipInfo, , , , True, ShipInfo)
                .ColDesign("Acc. Name", 80, "Acc. Name", AlignHorzEnum.Near, AlignVertEnum.Top, ShipInfo, , , , True, ShipInfo)
                .ColDesign("No Containers", 80, "No Containers", AlignHorzEnum.Center, AlignVertEnum.Top, ShipInfo, , , , True, ShipInfo)
                .ColDesign("Ex. Ship. Date", 80, "Ex. Ship. Date", AlignHorzEnum.Near, AlignVertEnum.Top, ShipInfo, , , , True, ShipInfo, , , , , "dd/MM/yyyy")
                .ColDesign("Act. Ship. Date", 80, "Act. Ship. Date", AlignHorzEnum.Near, AlignVertEnum.Top, ShipInfo, , , , True, ShipInfo, , , , , "dd/MM/yyyy")
                .ColDesign("Ex. Arvl. Date", 80, "Ex. Arvl. Date", AlignHorzEnum.Near, AlignVertEnum.Top, ShipInfo, , , , True, ShipInfo, , , , , "dd/MM/yyyy")
                .ColDesign("Ex. Arvl. Rks", 120, "Ex. Arvl. Rks", AlignHorzEnum.Near, AlignVertEnum.Top, ShipInfo, , , , True, ShipInfo)
                .ColDesign("Act. Arvl. Date", 80, "Act. Arvl. Date", AlignHorzEnum.Near, AlignVertEnum.Top, ShipInfo, , , , True, ShipInfo, , , , , "dd/MM/yyyy")
                .ColDesign("Act. Arvl. Rks", 120, "Act. Arvl. Rks", AlignHorzEnum.Near, AlignVertEnum.Top, ShipInfo, , , , True, ShipInfo)
                .ColDesign("RMO_HOApproved", 0, "", AlignHorzEnum.Near, AlignVertEnum.Top, False, , , , True, False, , , , , , , , , , , False)
                .ColDesign("RMO_ControlApproved", 0, "", AlignHorzEnum.Near, AlignVertEnum.Top, False, , , , True, False, , , , , , , , , , , False)
                .ColDesign("vATpER", 0, "", AlignHorzEnum.Near, AlignVertEnum.Top, False, , , , True, False, , , , , , , , , , , False)
                .ColDesign("Container Reference", 80, "Container Reference", AlignHorzEnum.Near, AlignVertEnum.Top, ShipInfo, , , , True, ShipInfo) 'added by GS on 06/09/16
                .ColDesign("TechContent", 110, "Technical Content", AlignHorzEnum.Near, AlignVertEnum.Top, techContent, , , , True, techContent) 'ch @ 24/01/2017 
                .ColDesign("TechContent", 110, "Technical Content", AlignHorzEnum.Near, AlignVertEnum.Top, techContent, , , , True, techContent) 'ch @ 24/01/2017 
                .ColDesign("RMO_AcceptDate", 0, "", AlignHorzEnum.Near, AlignVertEnum.Top, False, , , , True, False, , , , , , , , , , , False) 'ch @ 24/01/2017 
                .ColDesign("RMO_AcceptInfo", 0, "", AlignHorzEnum.Near, AlignVertEnum.Top, False, , , , True, False, , , , , , , , , , , False) 'ch @ 24/01/2017 
                .ColDesign("RMO_ControlApprovalUSer", 0, "Control Approval User", AlignHorzEnum.Near, AlignVertEnum.Top, False, , , , , False, , , , , , , , , , , False) 'ch @ 07/11/2017  wf : 6052 
                .ColDesign("RMO_ControlApprovalDate", 0, "Control Approval Date", AlignHorzEnum.Near, AlignVertEnum.Top, False, , , , , False, , , , , , , , , , , False) 'ch @ 07/11/2017  wf : 6052
                .ColDesign("RMO_HeadApprovalUser", 0, "Head Office Approval User", AlignHorzEnum.Near, AlignVertEnum.Top, False, , , , , False, , , , , , , , , , , False) 'ch @ 07/11/2017  wf : 6052
                .ColDesign("RMO_HeadApprovalDate", 0, "Head Office Approval Date", AlignHorzEnum.Near, AlignVertEnum.Top, False, , , , , False, , , , , , , , , , , False) 'ch @ 07/11/2017  wf : 6052
                .ColDesign("RMO_CEOApprovalUser", 0, "CEO Approval User", AlignHorzEnum.Near, AlignVertEnum.Top, False, , , , , False, , , , , , , , , , , False) 'ch @ 07/11/2017  wf : 6052
                .ColDesign("RMO_CEOApprovalDate", 0, "CEO Approval Date", AlignHorzEnum.Near, AlignVertEnum.Top, False, , , , , False, , , , , , , , , , , False) 'ch @ 07/11/2017  wf : 6052
                .ColDesign("Fo_email", 0, "", AlignHorzEnum.Near, AlignVertEnum.Top, False, , , , , False, , , , , , , , , , , False) 'ch @ 07/11/2017  wf : 6052
                .ColDesign("order", 0, "", AlignHorzEnum.Near, AlignVertEnum.Top, False, , , , , False, , , , , , , , , , , False) 'ch @ 08/11/2017  
                .ColDesign("ControlApprovalUSer", 0, "", AlignHorzEnum.Near, AlignVertEnum.Top, False, , , , , False, , , , , , , , , , , False)
                .ColDesign("HeadApprovalUser", 0, "", AlignHorzEnum.Near, AlignVertEnum.Top, False, , , , , False, , , , , , , , , , , False)
                .ColDesign("CEOApprovalUser", 0, "", AlignHorzEnum.Near, AlignVertEnum.Top, False, , , , , False, , , , , , , , , , , False)
                .ColDesign("AppUstamp", 0, "", AlignHorzEnum.Near, AlignVertEnum.Top, False, , , , , False, , , , , , , , , , , False)
                If gbFundDisbursementModuleIsActive And PrintFundDisbursement Then
                    .ColDesign("MOD_Hrecid", 0, "", AlignHorzEnum.Near, AlignVertEnum.Top, False, , , , , False, , , , , , , , , , , False)
                End If

                Dim rptParam(9, 1) As Object
                rptParam(0, 0) = "ShipInfo" : rptParam(0, 1) = ShipInfo
                rptParam(1, 0) = "gbDecNo" : rptParam(1, 1) = IIf(Fashion = True, 0, gbDecNo)
                rptParam(2, 0) = "PrintMode" : rptParam(2, 1) = Mode
                rptParam(3, 0) = "TechContent" : rptParam(3, 1) = techContent
                rptParam(4, 0) = "gbcurr1" : rptParam(4, 1) = "'" & gbCurr1 & "'"
                rptParam(5, 0) = "PrintSig" : rptParam(5, 1) = PrintSig
                rptParam(6, 0) = "ShowLineNUmber" : rptParam(6, 1) = IIf(ShowLineNumber = True, "'Y'", "'N'")
                rptParam(7, 0) = "VATLCROUND" : rptParam(7, 1) = VatLCRound
                rptParam(8, 0) = "ShowNotification" : rptParam(8, 1) = PrintFootRemark
                rptParam(9, 0) = "SeperateVatLCRate" : rptParam(9, 1) = "'" & SeperateVATLCRate & "'"
                '   If Mode = 1 Then Call RefLogoSubRep(ReportName, "L_img", gbComp)
                'Added By Abir T. @ 24.06.2013
                If defaultReport Then 'added by GS on 03/08/16
                    'If Mode = 0 Then  
                    Dim sbr1 As frmPreviewGrid.SubReport
                    Dim str1 As String

                    str1 = "SELECT * FROM " & TempPOSubRep
                    sbr1 = New frmPreviewGrid.SubReport("PayTerm")

                    sbr1.AddECParameter("@PayTermsStr", str1)
                    frm.InitSubReport(sbr1)
                    'End If
                Else
                    Dim sbr2, sbr3, sbr4, sbr5 As frmPreviewGrid.SubReport
                    Dim str2, str3, str4, str5 As String
                    str2 = "SELECT UIF_UserCode,   UIF_UserSignature FROM Com_UserInfoFolio"
                    sbr2 = New frmPreviewGrid.SubReport("CeoApproval")
                    sbr2.AddECParameter("@CeoStr", str2)
                    frm.InitSubReport(sbr2)
                    str3 = "SELECT UIF_UserCode,   UIF_UserSignature FROM Com_UserInfoFolio"
                    sbr3 = New frmPreviewGrid.SubReport("HeadApproval")
                    sbr3.AddECParameter("@HeadStrsql", str3)
                    frm.InitSubReport(sbr3)
                    str4 = "SELECT UIF_UserCode,   UIF_UserSignature FROM Com_UserInfoFolio"
                    sbr4 = New frmPreviewGrid.SubReport("ControlApproval")
                    sbr4.AddECParameter("@ControlStrsql", str4)
                    frm.InitSubReport(sbr4)
                    str5 = "SELECT UIF_UserCode,   UIF_UserSignature FROM Com_UserInfoFolio"
                    sbr5 = New frmPreviewGrid.SubReport("SuppApproval")
                    sbr5.AddECParameter("@SuppStrsql", str5)
                    frm.InitSubReport(sbr5)
                End If 'added by GS on 03/08/16

                If ReportName = "PORd2_ManOrd_WithFunds.rpt" Then
                    Dim sbr6 As frmPreviewGrid.SubReport
                    sbr6 = New frmPreviewGrid.SubReport("Fund")
                    sbr6.AddECParameter("@Fund", FundQuery)
                    sbr6.AddECParameter("Curr1", gbCurr1)
                    sbr6.AddECParameter("Curr2", gbCurr2)
                    sbr6.AddECParameter("deci", IIf(gbDecNo = 2, 2, 3).ToString)
                    frm.InitSubReport(sbr6)
                End If

                frm.ShowDialog(CType(strPrev, String(,)), frmPO.Name, ReportName, rptParam, , , , , , , , , , , NbPrint) 'edited by GS on 01/04/16 (add nbprint param)

            End With
        Catch exp As Exception
            Throw exp
        Finally
            If Not frm Is Nothing Then frm = Nothing
            Cursor.Current = Cursors.Default
            If Not drSql Is Nothing Then
                If drSql.IsClosed = False Then drSql.Close()
                drSql = Nothing
            End If
        End Try
    End Sub
    'Print Report direclty , without Preview Grid
    Public Sub PrintPODirectRep(ByVal frm As Form, ByVal SQLstat As String, ByVal ReportName As String, ByVal TempPOSubRep As String, ByVal Cost As Integer, ByVal Mode As Integer, ByVal ShipInfo As Boolean, ByVal Fashion As Boolean, Optional ByVal NbPrint As Integer = 1, Optional ByVal defaultReport As Boolean = True _
                                 , Optional ByVal techContent As Boolean = False, Optional ByVal DTMails As DataTable = Nothing,
                                    Optional Separated As Boolean = False, Optional ByVal DTpdf As DataTable = Nothing,
                                    Optional AllowAddNewRow As Boolean = True, Optional condSendMail As String = "", Optional PrintSig As Boolean = True, Optional ShowLineNumber As Boolean = False, Optional PrintFootRemark As Boolean = True, Optional FundQuery As String = "", Optional SeperateVATLCRate As Boolean = False) 'edited by GS on 03/08/16 (add defaultReport)
        Dim cnSql As SqlConnection
        Dim cmSql As SqlCommand
        Dim trnSql As SqlTransaction
        Dim strsql As String
        Dim TotNoUser As String
        Dim SelectVch As String
        Dim ViewName As String
        Try
            Cursor.Current = Cursors.WaitCursor
            ViewName = "VPP_" & frm.Name & "_" & gbUser

            cnSql = New SqlConnection(ConnectionString)
            cnSql.Open()

            trnSql = cnSql.BeginTransaction
            cmSql = New SqlCommand(strsql, cnSql, trnSql)

            strsql = "if exists (select * FROM sysobjects where id = object_id(N'" & ViewName & "') "
            strsql &= " and OBJECTPROPERTY(id, N'Isview') = 1) drop view " & ViewName
            cmSql.CommandText = strsql
            cmSql.ExecuteNonQuery()

            'create view
            strsql = " Create View " & ViewName & " AS " & SQLstat
            cmSql.CommandText = strsql
            cmSql.ExecuteNonQuery()
            trnSql.Commit()


            'strsql = "select * from " & ViewName
            strsql = "select * from " & ViewName + " WHERE 1=1"

            condSendMail = strsql & condSendMail


            If defaultReport Then 'added by GS on 03/08/16
                If TempPOSubRep <> "" Then
                    strPrev = Nothing
                    Dim sbr As ModPrint.SubReport
                    Dim str As String
                    str = "SELECT * FROM " & TempPOSubRep
                    sbr = New ModPrint.SubReport("PayTerm")
                    sbr.AddECParameter("@PayTermsStr", str)
                    ModPrint.InitSubReport(sbr)
                End If
            Else
                Dim sbr2, sbr3, sbr4, sbr5 As ModPrint.SubReport
                Dim str2, str3, str4, str5 As String
                str2 = "SELECT UIF_UserCode,   UIF_UserSignature FROM Com_UserInfoFolio"
                sbr2 = New ModPrint.SubReport("CeoApproval")
                sbr2.AddECParameter("@CeoStr", str2)
                ModPrint.InitSubReport(sbr2)
                str3 = "SELECT UIF_UserCode,   UIF_UserSignature FROM Com_UserInfoFolio"
                sbr3 = New ModPrint.SubReport("HeadApproval")
                sbr3.AddECParameter("@HeadStrsql", str3)
                ModPrint.InitSubReport(sbr3)
                str4 = "SELECT UIF_UserCode,   UIF_UserSignature FROM Com_UserInfoFolio"
                sbr4 = New ModPrint.SubReport("ControlApproval")
                sbr4.AddECParameter("@ControlStrsql", str4)
                ModPrint.InitSubReport(sbr4)
                str5 = "SELECT UIF_UserCode,   UIF_UserSignature FROM Com_UserInfoFolio"
                sbr5 = New ModPrint.SubReport("SuppApproval")
                sbr5.AddECParameter("@SuppStrsql", str5)
                ModPrint.InitSubReport(sbr5)
            End If 'added by GS on 03/08/16

            If ReportName = "PORd2_ManOrd_WithFunds.rpt" Then
                Dim sbr6 As ModPrint.SubReport
                sbr6 = New ModPrint.SubReport("Fund")
                sbr6.AddECParameter("@Fund", FundQuery)
                sbr6.AddECParameter("Curr1", gbCurr1)
                sbr6.AddECParameter("Curr2", gbCurr2)
                sbr6.AddECParameter("deci", IIf(gbDecNo = 2, 2, 3).ToString)
                ModPrint.InitSubReport(sbr6)
            End If

            Dim rptParam(9, 1) As Object
            rptParam(0, 0) = "ShipInfo" : rptParam(0, 1) = ShipInfo
            rptParam(1, 0) = "gbDecNo" : rptParam(1, 1) = IIf(Fashion = True, 0, gbDecNo)
            rptParam(2, 0) = "PrintMode" : rptParam(2, 1) = Mode
            rptParam(3, 0) = "TechContent" : rptParam(3, 1) = techContent
            rptParam(4, 0) = "gbcurr1" : rptParam(4, 1) = "'" & gbCurr1 & "'"
            rptParam(5, 0) = "PrintSig" : rptParam(5, 1) = PrintSig
            rptParam(6, 0) = "ShowLineNUmber" : rptParam(6, 1) = IIf(ShowLineNumber = True, "'Y'", "'N'").ToString
            rptParam(7, 0) = "VATLCROUND" : rptParam(7, 1) = VatLCRound
            rptParam(8, 0) = "ShowNotification" : rptParam(8, 1) = PrintFootRemark
            rptParam(9, 0) = "SeperateVatLCRate" : rptParam(9, 1) = "'" & SeperateVATLCRate & "'"
            '   If Mode = 1 Then Call RefLogoSubRep(ReportName, "L_img", gbComp)
            If DTMails Is Nothing Then
                PreviewProcedure(strsql, "", ReportName, frm, rptParam, NbPrint, True) 'edited by GS on 15/09/16 (add withLogo = True)
            Else
                PreviewProcedure(strsql, "", ReportName, frm, rptParam, NbPrint, True, DTMails, Separated, DTpdf, AllowAddNewRow, condSendMail) 'edited by GS on 15/09/16 (add withLogo = True
            End If


        Catch ex As Exception
            Throw ex
        Finally
            Cursor.Current = Cursors.Default
            cmSql = Nothing
            If Not cnSql Is Nothing Then
                If cnSql.State = ConnectionState.Open Then cnSql.Close()
                cnSql = Nothing
            End If
        End Try
    End Sub
#End Region

    'Added By Abir T. @ 13.11.2013
#Region "Purchase Invoice"
    Public Sub CreatePITable(ByVal TempTable As String, ByVal Fashion As Boolean, ByVal KeyGender As Boolean,
                                ByVal KeyStyle As Boolean, ByVal MaxSizes As Integer)
        Dim strsql As String
        Dim j As Integer
        Try
            DropSqlObject(TempTable, "TABLE")
            strsql = " CREATE TABLE " & TempTable & " ("
            strsql &= " Cnt INTEGER not null,"
            strsql &= " supitmref VARCHAR(30) not null ,"
            If Fashion = True Then strsql &= " SuppColor VARCHAR(30),"
            'Stock Item Key
            For i = 0 To CurrItmKey.Length - 1
                strsql &= CurrItmKey(i) & "[varchar] (20) NULL,"
            Next
            If Fashion = True Then
                If KeyGender = False Then strsql &= " Gender NVARCHAR(40),"
                strsql &= " ItemType NVARCHAR(30),"
                If KeyStyle = False Then strsql &= " Style NVARCHAR(20),"
            End If
            strsql &= " Counter INT  not null,"
            strsql &= " StkRecid NUMERIC(18, 0)  ,"
            strsql &= " PendStkRecid numeric(18,0),"
            strsql &= " Recid numeric(18,0) not null,"
            strsql &= " stat VARCHAR(30) NOT NULL ,"
            strsql &= " description NVARCHAR(100) ," 'wadih @07/03/2016
            If Fashion = True Then
                strsql &= " Color VARCHAR(2),"
                strsql &= " GridCode VARCHAR(4),"
                j = 0
                For j = 1 To MaxSizes
                    strsql &= " Size" & j & " numeric(15," & gbDecNo & "),"
                Next
            End If
            strsql &= " Qty NUMERIC(18, 3) ,"
            strsql &= " Subunit VARCHAR(3) ,"
            strsql &= " ConsOrdQty DECIMAL(18, 3) ,"
            strsql &= " UnitCost DECIMAL(18, 3) ,"
            strsql &= " TotalCost DECIMAL(18, 3) ,"
            strsql &= " ItmDisc DECIMAL(6, 3) ,"
            strsql &= " TotalAmount DECIMAL(18, 3) ,"
            strsql &= " InvDisc DECIMAL(6, 3) ,"
            strsql &= " TotalNet DECIMAL(18, 3) ,"
            strsql &= " VAT VARCHAR(100) ,"
            strsql &= " location VARCHAR(2) ,"
            strsql &= " Coef NUMERIC(18, 2) ,"
            strsql &= " retLc NUMERIC(18, 3) ,"
            strsql &= " retCc NUMERIC(18, 3) ,"
            strsql &= " retFc NUMERIC(18, 3) ,"
            strsql &= " Stkfccode VARCHAR(3),"
            strsql &= " PORecId numeric(18,0),"
            strsql &= " PIDRecId numeric(18,0),"
            strsql &= " ItmClass VARCHAR(1),"
            strsql &= " INVD_HRECID numeric (18, 0))" 'added by GS on 08/09/16
            Execute(strsql)

            strsql = "ALTER TABLE " & TempTable & " ADD CONSTRAINT [PK_" & TempTable & "] PRIMARY KEY  CLUSTERED "
            strsql &= " (Cnt) ON [PRIMARY] "
            Execute(strsql)
        Catch ex As Exception
            Throw ex
        End Try
    End Sub
    'Abir T. @ 07.01.2014
    Public Sub FillPITable(ByVal TempTable As String, ByVal Recid As String, ByVal Fashion As Boolean, ByVal FormName As String,
                            ByVal KeyGender As Boolean, ByVal KeyStyle As Boolean, ByVal MaxSizes As Integer, Optional ByVal TempPISel As String = "") 'edited by GS on 07/09/16 (add TempPISel)
        Dim drSQL As SqlDataReader
        Dim drSizes As SqlDataReader
        Dim GridSizeArr() As String
        Dim strGrid As String
        Dim strsql As String
        Dim str As String
        Dim strCnt As String
        Dim k As Integer
        Dim CASHDISC As String
        Dim ITMDISC As String

        Dim TempExpiry As String
        Dim TmpGridBarcode As String
        Try
            If Fashion = False Then
                DropSqlObject("Temp_" & FormName & IIf(Fashion, "Fashion", "").ToString & "_" & gbUser, "TABLE")
                TempExpiry = "TempExpiryPI_" & FormName & IIf(Fashion, "Fashion", "").ToString & "_" & gbUser
                TmpGridBarcode = "TmpGridBarcodePI_" & FormName & IIf(Fashion, "Fashion", "").ToString & "_" & gbUser

                strsql = "SELECT DISTINCT invd_supref,"
                For i = 0 To CurrItmKey.Length - 1
                    strsql &= CurrItmKey(i) & ","
                Next
                strsql &= " invD_Counter, ISNULL(invd_stkrecid, 0) invd_stkrecid, ISNULL(INVD_PendRecId, 0) INVD_PendRecId,"
                strsql &= " COALESCE(invd_stkrecid, INVD_PendRecId, 0) Recid, invD_status,"
                strsql &= " Stk_Desc, invd_PurQty, STK_SUBUNIT,invd_PurConsQty,round(invD_UCost,3) invD_UCost,"
                strsql &= " ROUND((ROUND(invD_UCost, 3) * ROUND(invD_purQty, 3)), 3) AS Total_Cost,"

                CASHDISC = "INVD_CshDisc" : ITMDISC = "INVD_ItmDisc"
                CASHDISC = "CASE WHEN INVD_STATUS='N' THEN " & CASHDISC & " ELSE 100 END"

                strsql &= " CASE WHEN invD_status='N' THEN " & ITMDISC & " ELSE 0 END INV_ITEMDISC,"
                strsql &= " Round(ROUND(invD_purQty, 3) * ROUND(invD_UCost, 3) * ROUND((1-(" & ITMDISC & "/100.000)), 3),3) as TotalAmount,"
                strsql &= " " & CASHDISC & " INV_CASHDISC, ROUND(ROUND(invD_purQty, 3) * ROUND(invD_UCost, 3) * ROUND((1-(" & ITMDISC & " /100.000)), 3) *"
                strsql &= " ROUND((1-(" & CASHDISC & " /100.000)), 3),3) as invD_NetTot,"
                strsql &= " case STK_VATTYPE when 'T' then 'Taxable' when 'E' then 'Embeded' else 'Not Taxable' end VAT,"
                strsql &= " invd_location, INVD_COEF, INVD_RETLC,INVD_RETCC,INVD_RETFC,"
                strsql &= " STK_FCCODE, INVD_PORecId, INVD_RECID AS PIDRecId, STK_ItmClass AS Classification"
                strsql &= " ,INVD_HRECID " 'added by GS on 08/09/16
                strsql &= " INTO Temp_" & FormName & IIf(Fashion, "Fashion", "").ToString & "_" & gbUser
                strsql &= " FROM V_PORD2_PIDETGRID"
                strsql &= " LEFT JOIN STK04_InvDBE ON INVD_RECID = INVB_DrecId"
                If Recid <> "" Then 'added by GS on 07/09/16
                    strsql &= " WHERE invD_Hrecid = " & Val(Recid) & " AND STK_ItmClass <> 'F'"
                    'added by GS on 07/09/16
                Else
                    strsql &= " INNER JOIN " & TempPISel & " TempPISel on TempPISel.INV_RECID = invD_Hrecid and Chk = 'True' AND STK_ItmClass <> 'F' "
                End If
                'GS on 07/09/16
                strsql &= " ORDER BY INVD_HRECID, INVD_RECID" 'edited by GS on 08/09/16 (add INVD_HRECID)

                strsql &= " INSERT INTO " & TempTable
                strsql &= " SELECT ROW_NUMBER() OVER ( ORDER BY PIDRecId ASC), *"
                strsql &= " FROM Temp_" & FormName & IIf(Fashion, "Fashion", "").ToString & "_" & gbUser
                Execute(strsql)

                ''insering into Expiry Grid
                DropSqlObject(TempExpiry, "TABLE")
                strsql = "SELECT ROW_NUMBER() OVER (ORDER BY INVQ_BatchInfo) AS Recid,temp.*"
                strsql &= " INTO " & TempExpiry & " FROM"
                strsql &= " (SELECT DISTINCT Cnt, INVQ_BatchInfo, INVQ_ExpiryDate, INVQ_Recqty,'O' stat"
                strsql &= " FROM " & TempTable & " Grid"
                strsql &= " LEFT JOIN STK04_InvDQty ON INVQ_Drecid = GRID.PIDRECID"
                strsql &= " WHERE INVQ_BatchInfo <> '' AND INVQ_ExpiryDate <> '') temp"
                Execute(strsql)

                ''insering into Barcode Grid
                DropSqlObject(TmpGridBarcode, "TABLE")
                strsql = "SELECT ROW_NUMBER() OVER ( ORDER BY RECID ) AS rec,tempo.* INTO " & TmpGridBarcode
                strsql &= " FROM ( SELECT INVB_RECID, PITemp.Cnt AS RECID, temp.RECID AS E_Recid, detbarc.INVB_Barcode, 'O' stat"
                strsql &= " FROM STK04_InvDET det"
                strsql &= " INNER JOIN " & TempTable & " PITemp ON PIDRecid= det.INVD_Recid"
                strsql &= " INNER JOIN STK04_STKFIL ON STK_RECID = INVD_StkRecid"
                strsql &= " LEFT JOIN STK04_INVDBE detbarc ON INVD_Recid = INVB_DrecId"
                strsql &= " INNER JOIN " & TempExpiry & " temp ON temp.INVQ_BatchInfo = detbarc.IVB_BatchInfo"
                strsql &= " AND temp.INVQ_ExpiryDate = detbarc.INVB_ExpiryDate"
                If Recid <> "" Then 'added by GS on 07/09/16
                    strsql &= " WHERE det.INVD_HRecid = " & Recid & " AND STK_ItmClass='E' AND detbarc.INVB_Barcode <> ''"
                    'added by GS on 07/09/16
                Else
                    strsql &= " INNER JOIN " & TempPISel & " TempPISel on TempPISel.INV_RECID = det.INVD_HRecid and Chk = 'True' AND STK_ItmClass='E' AND detbarc.INVB_Barcode <> '' "
                End If
                'GS on 07/09/16
                strsql &= " UNION"
                strsql &= " SELECT INVB_RECID, Cnt AS RECID, NULL AS E_Recid, detbarc.INVB_Barcode, 'O' stat"
                strsql &= " FROM STK04_InvDET det INNER JOIN dbo.STK04_STKFIL ON STK_RECID=INVD_StkRecid"
                strsql &= " LEFT JOIN STK04_INVDBE detbarc ON INVD_Recid = INVB_Drecid"
                strsql &= " INNER JOIN " & TempTable & " temp ON temp.PIDRecid= det.INVD_Recid"
                If Recid <> "" Then 'added by GS on 07/09/16
                    strsql &= " WHERE det.INVD_HRecid = " & Recid & " AND detbarc.INVB_Barcode <> ''"
                    'added by GS on 07/09/16
                Else
                    strsql &= " INNER JOIN " & TempPISel & " TempPISel on TempPISel.INV_RECID = det.INVD_HRecid and Chk = 'True' AND detbarc.INVB_Barcode <> '' "
                End If
                'GS on 07/09/16
                strsql &= " AND (STK04_STKFIL.STK_ItmClass='T' OR STK04_STKFIL.STK_ItmClass='B')) tempo"
                Execute(strsql)

            Else
                TmpGridBarcode = "TmpGridBarcodePI_" & FormName & "Fashion_" & gbUser
                DropSqlObject(TmpGridBarcode, "TABLE")

                ''CREATING BARCODES
                strsql = "SELECT 0 AS Tmp_Recid, Grid.RECid, grid.Color,"
                For k = 1 To MaxSizes
                    strsql &= " INVB_Barcode AS [Barcode" & k & "] ,'O' AS [Stat" & k & "] ,"
                Next
                strsql &= " 'O' AS Status INTO " & TmpGridBarcode & " FROM  " & TempTable & " Grid"
                strsql &= " INNER JOIN (SELECT 0 counter,* FROM STK04_INVDBE) STK04_INVDBE ON INVB_Drecid = Grid.PIDRECid"
                strsql &= " WHERE 2 = 1"
                Execute(strsql)

                str = "  SELECT distinct INVD_RECID, INVD_HRECID,INVD_SUPREF, INVD_STATUS,"
                str &= " INVD_STKRECID, INVD_PendRecId, INVQ_Color, INVQ_GridCode"
                str &= " FROM STK04_InvDET"
                str &= " LEFT JOIN STK04_InvDQty ON INVD_RECID = INVQ_Drecid"
                If Recid <> "" Then 'added by GS on 07/09/16
                    str &= " WHERE INVD_hRECID = " & Val(Recid)
                    'added by GS on 07/09/16
                Else
                    str &= " INNER JOIN " & TempPISel & " TempPISel on TempPISel.INV_RECID = INVD_hRECID and Chk = 'True' "
                End If
                'GS on 07/09/16
                ReadFromDBToDataReader(str, drSQL)
                k = 0
                With drSQL
                    If .HasRows Then
                        While .Read
                            strGrid = "SELECT GD_SizeDesc FROM dbo.stk04_DetGRID"
                            strGrid &= " WHERE GD_Code='" & .Item("INVQ_GridCode").ToString & "'"
                            strGrid &= " ORDER BY GD_Order"
                            ReadFromDBToDataReader(strGrid, drSizes)
                            i = 0
                            If drSizes.HasRows Then
                                While drSizes.Read
                                    ReDim Preserve GridSizeArr(i)
                                    GridSizeArr(i) = drSizes.Item("GD_SizeDesc").ToString
                                    i = i + 1
                                End While
                            End If
                            If Not drSizes Is Nothing Then
                                If drSizes.IsClosed = False Then drSizes.Close()
                                drSizes = Nothing
                            End If

                            strsql = "INSERT INTO  " & TempTable
                            strsql &= " SELECT (SELECT ISNULL(MAX(Cnt),0) FROM " & TempTable & " )"
                            strsql &= " + ROW_NUMBER() OVER (ORDER BY INVD_RECID) cnt, "
                            strsql &= "  invd_supref, INVQ_SuppColor,"
                            For i = 0 To CurrItmKey.Length - 1
                                strsql &= CurrItmKey(i) & ","
                            Next

                            If KeyGender = False Then strsql &= " (SELECT G_DESC FROM STK04_GENDER WHERE G_CODE = STK_Gender) AS Gender,"
                            strsql &= " (SELECT IT_DESC FROM STK04_ItemType WHERE IT_CODE = STK_ITMTYPE) AS ItemType,"
                            If KeyStyle = False Then strsql &= " (SELECT ST_DESC FROM stk04_STYLE WHERE ST_CODE = STK_Style) AS Style,"

                            strsql &= " (SELECT ISNULL(MAX(Counter),0) + 1 FROM " & TempTable
                            strsql &= " WHERE supitmref = '" & .Item("invd_supref").ToString.Replace("'", "''") & "'" 'edited by GS on 15/09/16 (add replace)
                            strsql &= " AND stat = '" & .Item("INVD_STATUS").ToString & "') AS Counter,"
                            strsql &= " ISNULL(INVD_STKRECID, 0) StkRecid, ISNULL(INVD_PendRecId, 0) PendStkRecid,"
                            strsql &= " COALESCE(INVD_STKRECID, INVD_PendRecId, 0) Recid,"
                            strsql &= " invd_status, Stk_Desc,"
                            strsql &= " INVQ_Color, "

                            strsql &= " STK_Grid AS Grid,"
                            For k = 1 To MaxSizes
                                If k <= GridSizeArr.Length Then
                                    strsql &= " (SELECT INVQ_Recqty FROM STK04_InvDQty "
                                    strsql &= " WHERE INVQ_Drecid = INVD_RECID "
                                    strsql &= " AND INVQ_GridSize = '" & GridSizeArr(k - 1) & "') AS QTY" & k & ","
                                Else
                                    strsql &= " NULL AS Size" & k & ","
                                End If
                            Next
                            strsql &= " invd_PurQty ,"
                            strsql &= " invd_purunit, INVD_PURCONSQTY, round(INVD_UCOST,3) INVD_UCOST,  "
                            strsql &= " ROUND(ROUND(INVD_UCOST, 3) * ROUND(invD_purQty, 3),3) AS Total_Cost, "

                            CASHDISC = "INVD_CshDisc" : ITMDISC = "INVD_ItmDisc"
                            CASHDISC = "CASE WHEN INVD_STATUS='N' THEN " & CASHDISC & " ELSE 100 END"

                            strsql &= " CASE WHEN INVD_STATUS='N' THEN " & ITMDISC & " ELSE 0 END AS invd_itmDisc,"
                            strsql &= " Round(ROUND(invD_purQty, 3) * ROUND(INVD_UCOST, 3)"
                            strsql &= " * ROUND(1-(" & ITMDISC & "/100.000),3),3) as Total_Amount,"
                            strsql &= " " & CASHDISC & " AS MOD_CashDisc,"
                            strsql &= " ROUND(ROUND(invD_purQty, 3)"
                            strsql &= " * ROUND(INVD_UCOST, 3) * ROUND(1-(" & ITMDISC & " /100.000), 3) *"

                            strsql &= " ROUND(1-(" & CASHDISC & " /100.000), 3),3) AS invD_NetTot,"
                            strsql &= " case STK_VATTYPE when 'T' then 'Taxable' when 'E' "
                            strsql &= " then 'Embeded' else 'Not Taxable' end VAT, invd_location, "
                            strsql &= " INVD_COEF, INVD_RETLC,INVD_RETCC,INVD_RETFC, STK_FCCODE,"
                            strsql &= " INVD_PORecId, INVD_RECID AS PIDRecId, STK_ItmClass"
                            strsql &= " ,INVD_HRECID " 'added by GS on 08/09/16
                            strsql &= " FROM V_PORD2_PIDETGRID" 'INVD_HRECID
                            strsql &= " WHERE INVD_RECID = " & .Item("INVD_RECID").ToString & " AND STK_ItmClass = 'F'"
                            strsql &= " AND INVD_HRECID = " & .Item("INVD_HRECID").ToString 'added by GS on 08/09/16
                            Execute(strsql)
                            strsql = ""

                            ''INSERING BARCODES
                            strsql = " INSERT INTO  " & TmpGridBarcode
                            strsql &= " SELECT ROW_NUMBER() OVER ( ORDER BY Grid.RECid  )+(SELECT ISNULL(MAX(Tmp_Recid),0)"
                            strsql &= " FROM " & TmpGridBarcode & "), Grid.RECid, grid.Color,"
                            For k = 1 To MaxSizes
                                If k <= GridSizeArr.Length Then
                                    strsql &= " MIN(CASE WHEN gd_order =  " & (k) & " THEN INVB_Barcode"
                                    strsql &= " Else  Convert(varchar(20), NULL) END)  SIZE" & k & ",'O',"
                                Else
                                    strsql &= "Convert(varchar(20), NULL) ,'O',"
                                End If
                            Next
                            strsql &= " 'O' FROM  " & TempTable & " Grid "
                            strsql &= " INNER JOIN ( SELECT    ROW_NUMBER() OVER ( PARTITION BY INVB_GridSize "
                            strsql &= " ORDER BY INVB_GridSize ) counter ,*"
                            strsql &= " FROM STK04_INVDBE "
                            strsql &= " WHERE INVB_Drecid='" & .Item("INVD_RECID").ToString & "')"
                            strsql &= " STK04_INVDBE ON STK04_INVDBE.INVB_Drecid =  Grid.PIDRECid "
                            strsql &= " INNER JOIN stk04_DetGRID ON INVB_GridCode = GD_Code AND INVB_GridSize = GD_SizeDesc"
                            strsql &= " GROUP BY Grid.RECID , Grid.Color"
                            Execute(strsql)
                            strsql = ""
                        End While
                    End If
                End With
            End If
        Catch ex As Exception
            Throw ex
        End Try
    End Sub
    Public Sub CreatePITempSubRep(ByVal TempPISubRep As String, ByVal Index As Integer, ByVal PICode As String, ByVal Curr As String, Optional ByVal TempPISel As String = "", Optional PrintFundDisbursement As Boolean = False) 'edited by GS on 07/09/16 (add TempPISel)
        'Index = 1-->> Print Sub Report Payment Terms in the PI
        'Index = 2-->> Print Sub Report ExpiryDate & Lot.No. Qty in the PI

        Dim Str As String
        Try
            Str = " IF EXISTS (SELECT name FROM sysobjects WHERE type = 'U' AND name = '" & TempPISubRep & "')"
            Str &= " DROP TABLE " & TempPISubRep
            Execute(Str)
            Select Case Index
                Case 1
                    Str = " CREATE TABLE " & TempPISubRep & "("
                    Str &= " [PINo.] [Numeric](18, 0) NOT NULL,"

                    'Rachel A. 01.03.2016
                    Str &= " [Desc]  NVARCHAR(Max) NULL,"
                    Str &= " [Perc] [Numeric](6, 3) NULL,"

                    Str &= " [Sett No.] [Numeric](18, 0) NOT NULL,"
                    Str &= " [Amt]  [Numeric](18, 3) NULL,"
                    Str &= " [Curr]  VARCHAR(3) NULL,"
                    Str &= " [Date] DATETIME NULL"
                    If gbFundDisbursementModuleIsActive And PrintFundDisbursement Then
                        Str &= ",[Fund Code]  VARCHAR(4) NULL,
                             [Fund Desc] NVARCHAR(Max) NULL,
                             [AmtToDisburse]  [Numeric](18, 3) NULL,
                             [Fund Curr]  VARCHAR(3) NULL,
                             [Oper LC]  VARCHAR(1) NULL,
                             [RateToLC] [Numeric](18, 3) NULL,
                             [CounterValLC] [Numeric](18, 3) NULL,
                             [Oper CC]  VARCHAR(1) NULL,
                             [RateToCC] [Numeric](18, 3) NULL,
                             [CounterValCC] [Numeric](18, 3) NULL,
                             [Remarks]  NVARCHAR(Max) NULL"
                    End If
                    Str &= "        )"
                    Execute(Str)

                    Str = " INSERT INTO " & TempPISubRep
                    Str &= " SELECT IPAY_IRECID ,PAY_DESC, Pay_Percentage,IPAY_PayNo ,IPAY_AMNT, "
                    If PICode <> "" Then
                        Str &= " '" & Curr & "'"
                    Else
                        Str &= " Inv_curr  "
                    End If
                    Str &= " , IPAY_DATE "
                    If gbFundDisbursementModuleIsActive And PrintFundDisbursement Then
                        Str &= " , FDT_Code ,
                               FDT_Description ,
                               FTR_AmountToDisburse ,
                               FTR_Currency ,
                               FTR_OperLC ,
                               FTR_RateToLC ,
                               FTR_CounterValueLC ,
                               FTR_OperFreshDollar ,
                               FTR_RateFreshDollar ,
                               FTR_CounterValueFreshDollar ,
                               FTR_Remarks "
                    End If
                    Str &= " FROM STK04_InvPayTerm"
                    'Rachel A. 01.03.2016 
                    Str &= " LEFT JOIN dbo.STK04_PayTerm ON IPAY_PAYMODE=PAY_CODE    AND IPAY_PayNo = Pay_SettNo"
                    If gbFundDisbursementModuleIsActive And PrintFundDisbursement Then
                        Str &= " LEFT JOIN ( SELECT  FTR_RecID ,
                                                     FTR_TrnRecID ,
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
                                                     FTR_Remarks ,
                                                     FTR_IPayPayMode ,
                                                     FTR_IPayPayNo
                                             FROM    STK04_FundTransactionPISett
                                                     INNER JOIN ( SELECT FDT_RecID ,
                                                                         FDT_Code ,
                                                                         FDT_Description
                                                                  FROM   " & gbAccountingLinkedServerForFunds & "ACC01_FundTypes
                                                                ) ACC01_FundTypes ON FDT_RecID = FTR_FDTRecID
                                           ) STK04_FundTransactionPISett ON FTR_TrnRecID = IPAY_IRECID
                                                                            AND FTR_IPayPayMode = IPAY_PAYMODE
                                                                            AND FTR_IPayPayNo = IPAY_PayNo "
                    End If

                    If PICode <> "" Then 'added by GS on 07/09/16
                        Str &= " WHERE IPAY_IRECID= '" & PICode & "'"
                        'added by GS on 07/09/16
                    Else
                        Str &= " INNER JOIN " & TempPISel & " TempPISel on TempPISel.INV_RECID = IPAY_IRECID and Chk = 'True' "
                    End If

                    'GS on 07/09/16
                    Execute(Str)

                Case 2
                    Str = " CREATE TABLE " & TempPISubRep & "("
                    Str &= " [PIDRecid] [Numeric](18, 0) NOT NULL,"
                    Str &= " [Batch No.] VARCHAR(15) NULL,"
                    Str &= " [ExpiryDate]  VARCHAR(10) NULL,"
                    Str &= " [Qty]  [Numeric](18, 3) NULL,"
                    Str &= " [ProductionDate]  VARCHAR(10) NULL,"
                    Str &= " [Barcode] VARCHAR(20) NULL)"
                    Execute(Str)

                    Str = " INSERT INTO " & TempPISubRep
                    Str &= " SELECT INVQ_Drecid, INVQ_BatchInfo, INVQ_ExpiryDate, INVQ_Recqty,INVQ_ProductionDt," ' INVBBarcode "

                    Str &= " ISNULL(INVBBarcode,"
                    Str &= "        ( ( SELECT TOP 1"
                    Str &= "                     dbar_barcode"
                    Str &= "            FROM     dbo.Stk04_DetBarcode"
                    Str &= "            WHERE    dbar_stkid = INVD_STKRECID"
                    Str &= "                     AND ISNULL(INVQ_ExpiryDate, '') = ISNULL(dbar_expdate,'')"
                    Str &= "                     AND ISNULL(INVQ_BatchInfo, '') = ISNULL(dbar_batch,'')"
                    Str &= "            ORDER BY dbar_DSTMP DESC )"
                    Str &= "        )) INVBBarcode"

                    Str &= " FROM STK04_InvDQty"
                    Str &= " INNER JOIN STK04_InvDET ON INVD_RECID = INVQ_Drecid"

                    Str &= "         LEFT JOIN ( SELECT  tb1.* ,"
                    Str &= "                             INVB_Barcode INVBBarcode"
                    Str &= "                     FROM    ( SELECT    MAX(INVB_RecId) INVBRecId ,"
                    Str &= "                                         INVB_DrecId INVBDrecId ,"
                    Str &= "                                         INVB_GridCode INVBGridCode ,"
                    Str &= "                                         INVB_GridSize INVBGridSize ,"
                    Str &= "                                         INVB_ExpiryDate INVBExpiryDate ,"
                    Str &= "                                         IVB_BatchInfo IVBBatchInfo ,"
                    Str &= "                                         INVB_Color INVBColor "
                    Str &= "                               FROM      dbo.STK04_InvDBE"
                    Str &= "                               GROUP BY  INVB_DrecId ,"
                    Str &= "                                         INVB_GridCode ,"
                    Str &= "                                         INVB_GridSize ,"
                    Str &= "                                         INVB_ExpiryDate ,"
                    Str &= "                                         IVB_BatchInfo ,"
                    Str &= "                                         INVB_Color "
                    Str &= "                             ) tb1"
                    Str &= "                             INNER JOIN STK04_InvDBE tb2 ON tb1.INVBRecId = tb2.INVB_RecId"
                    Str &= "                                                            AND ISNULL(tb1.INVBGridCode,"
                    Str &= "                                                               '') = ISNULL(tb2.INVB_GridCode,"
                    Str &= "                                                               '')"
                    Str &= "                                                            AND ISNULL(tb1.INVBGridSize,"
                    Str &= "                                                               '') = ISNULL(tb2.INVB_GridSize,"
                    Str &= "                                                               '')"
                    Str &= "                                                            AND ISNULL(tb1.INVBExpiryDate,"
                    Str &= "                                                               '') = ISNULL(tb2.INVB_ExpiryDate,"
                    Str &= "                                                               '')"
                    Str &= "                                                            AND ISNULL(tb1.IVBBatchInfo,"
                    Str &= "                                                               '') = ISNULL(tb2.IVB_BatchInfo,"
                    Str &= "                                                               '')"
                    Str &= "                                                            AND ISNULL(tb1.INVBColor,"
                    Str &= "                                                               '') = ISNULL(tb2.INVB_Color,"
                    Str &= "                                                               '')"
                    Str &= "                   ) tb ON INVD_RECID = INVBDrecId"
                    Str &= "                           AND INVQ_BatchInfo = IVBBatchInfo"
                    Str &= "                           AND INVQ_ExpiryDate = INVBExpiryDate"

                    If PICode <> "" Then 'added by GS on 07/09/16
                        Str &= " WHERE INVd_HRECID= '" & PICode & "'"
                        'added by GS on 07/09/16
                    Else
                        Str &= " INNER JOIN " & TempPISel & " TempPISel on TempPISel.INV_RECID = INVd_HRECID and Chk = 'True' "
                    End If
                    'GS on 07/09/16
                    Execute(Str)
            End Select
        Catch ex As Exception
            Throw ex
        End Try
    End Sub
    Public Sub CreatePIHeaderTable(ByVal FormName As String, ByVal Fashion As Boolean, ByVal Mode As Integer, ByVal Recid As String, Optional ByVal TempPISel As String = "", Optional ReportName As String = "") 'edited by GS on 07/09/16 (add TempPISel)
        Dim Str As String
        Try
            DropSqlObject("TempPIH_" & FormName & IIf(Fashion, "Fashion", "").ToString & "_" & gbUser, "TABLE")

            Str = "SELECT STK04_InvHead.INV_RECID, case when STK04_InvHead.INV_Prefix = 'N' then 'Normal' else 'Other' END INV_Prefix , STK04_InvHead.INV_Counter , STK04_InvHead.INV_INVREF, "
            Str &= " STK04_InvHead.INV_INVDATE, STK04_InvHead.INV_STATUS, STK04_InvHead.INV_LOCATION, INV_DELIVERY, "
            Str &= " INV_ShipDate, INV_PRICETYPE, INV_OVERPER, INV_OVERSRC, INV_CASHDISC, INV_ITEMDISC,"
            Str &= " INV_TOTLENQTY, INV_TOTWEIQTY, INV_TOTPICQTY, STK04_InvHead.INV_CURR, STK04_InvHead.INV_TOTVALUE, INV_ADJAMNTFC, INV_FrwdInvDate,"
            Str &= " INV_FrwdACNo, INV_OverheadAmntLC, INV_OverheadAmntCC, INV_OverheadAmntCV, INV_OverheadAmntFC,"
            Str &= " STK04_InvHead.INV_VATCC, STK04_InvHead.INV_VATLC, INV_DSTAMP,  STK04_InvHead.INV_USTAMP, STK04_InvHead.inv_updated, Sacc_accno1, Sacc_accno2, Sacc_accsub,"
            Str &= " Sup_Curr, inv_comp, comp_name, loc_desc, sup_code, sup_name, stk04_supplier.Sup_LocFor, inv_transfno,"
            Str &= " inv_SENDdate, Prc_desc, [user_name] AS Username, STK04_InvHead.Inv_UPDDate, STK04_InvHead.INV_ACCGEN, INV_Rks,"

            'Abir T. @ 20.11.2013
            'If Details ...Qty Formuals In Measurement Unit
            If Mode = 0 Then Str &= " FLQTY, NLQTY, FWQTY, NWQTY, FPQTY, NPQTY, RLQTY , RWqty , RPQty ,"
            '...landed Cost
            Str &= " CASE WHEN STK04_InvHead.INV_CURR = '" & gbCurr1 & "' THEN SubDet.LandAmtLC"
            Str &= " WHEN STK04_InvHead.INV_CURR = '" & gbCurr2 & "' THEN SubDet.LandAmtCC"
            Str &= " ELSE SubDet.LandAmtFC END AS LandAmt, case when isnull( inv_invdiscval,0)=  0 then SubDet.TotInvDisc else inv_invdiscval end   TotInvDisc,"
            '...Forwarder Aux name
            Str &= " CASE WHEN INV_FrwdACNo IS NULL THEN '' WHEN INV_FrwdACNo = 'SILENT' THEN ''"
            Str &= " WHEN INV_FrwdACNo = '' THEN '' ELSE AUX_Name END AS FwdName,"
            'Rates

            '''''''''ABIR T. @ 11.01.2014
            '    Dim RateApp As String
            '   RateApp = ExecuteScalar("SELECT App_Rate FROM Com_Application WHERE App_Code = '" & gbApp & "'")

            Str &= "   INV_operatorLC    AS opr1,"
            Str &= " ISNULL((   INV_Ratelc "
            Str &= "  ), 0) AS Rate1,"
            Str &= "  INV_operatorCC      AS opr2,"
            Str &= " ISNULL((  INV_Ratecc "
            Str &= "  ), 0) AS Rate2"

            Str &= " ,STUFF(( SELECT  ',' + SUPT_Desc AS 'data()'"
            Str &= " FROM stk04_SuppTypeLink"
            Str &= " 						INNER JOIN stk04_SuppType ON SUPTL_TypeCode = SUPT_Code"
            Str &= " WHERE SUPTL_SupRecid = Sup_RecId"
            Str &= " 			  FOR"
            Str &= " 				XML PATH('')"
            Str &= " 			  ), 1, 1, '') [SupType]"

            ''''''''''''
            'ch @05/08/2019 mail(FW: PetMart_Sales invoice/Purchase invoice: Hide company information)
            If Mode <> 0 AndAlso ReportName.ToUpper.Contains("PETMART") Then
                Str &= " , (SELECT Price_Desc FROM Stk04_SalPriceType WHERE  Price_Code = SUBSTRING(INV_PIBasepriceType1,0,CHARINDEX('|',INV_PIBasepriceType1,0)) ) +"
                Str &= " REPLACE(SUBSTRING(INV_PIBasepriceType1,CHARINDEX('|',INV_PIBasepriceType1,0), LEN(INV_PIBasepriceType1)),'|',' ') HEadPIBasepriceType1    "
                Str &= " , (SELECT Price_Desc FROM Stk04_SalPriceType WHERE  Price_Code = SUBSTRING(INV_PIBasepriceType2,0,CHARINDEX('|',INV_PIBasepriceType2,0)) ) +"
                Str &= " REPLACE(SUBSTRING(INV_PIBasepriceType2,CHARINDEX('|',INV_PIBasepriceType2,0), LEN(INV_PIBasepriceType2)),'|',' ') HEadPIBasepriceType2    "
            End If
            Str &= " INTO TempPIH_" & FormName & IIf(Fashion, "Fashion", "").ToString & "_" & gbUser
            Str &= " FROM STK04_InvHead"
            Str &= " LEFT JOIN stk04_supplier on sup_recid = inv_supp"
            Str &= " LEFT JOIN stk04_suppacc on Sacc_suprecid =SUP_recid"
            Str &= " and Sacc_comp=inv_comp"
            Str &= " LEFT JOIN com_company on  comp_code =inv_comp"
            Str &= " LEFT JOIN stk04_location on inv_location=loc_code"
            Str &= " LEFT JOIN stk04_pricetype on inv_pricetype =prc_code"
            Str &= " LEFT JOIN Com_userinfo on user_code = inv_ustamp"

            'Added by Abir T. @ 20.11.2013
            'Rates
            Str &= " LEFT JOIN Com_DailyRate ON DR_Date = INV_DELIVERY"
            Str &= " And DR_CurCode = (CASE WHEN INV_CURR = '" & gbCurr1 & "' THEN '" & gbCurr2 & "' ELSE INV_CURR END )"
            '...landed Cost
            Str &= " LEFT JOIN ("
            Str &= " SELECT INVD_Hrecid, ISNULL(SUM(INVD_Purqty * INVD_UlandcostLC), 0) AS LandAmtLC ,"
            Str &= " ISNULL(SUM(INVD_Purqty * INVD_UlandcostcC), 0) AS LandAmtcC ,"
            Str &= " ISNULL(SUM(INVD_Purqty * INVD_UlandcostfC), 0) AS LandAmtfC ,"
            'ch @ 08/02/2019 MODIFCATION ROUNDING DISC  MAIL (FW: edding error)
            '     Str &= "  ISNULL(Sum(  ( round(isnull(Invd_ucost  ,0)* (1-(Invd_Itmdisc/100.000)) ,3) * isnull(INVD_Purqty,0) - isnull(INVD_TNCOST,0)  ) ), 0) AS TotInvDisc"
            Str &= "  ISNULL(Sum(  CASE WHEN INVD_STATUS='F' THEN 0 ELSE  ( abs(isnull(Invd_ucost  ,0)) * (1-(Invd_Itmdisc/100.000)) * isnull(INVD_Purqty,0)) - isnull(INVD_TNCOST,0)    END ), 0) AS TotInvDisc"
            Str &= " FROM STK04_INVDET GROUP BY INVD_Hrecid) AS SubDet ON SubDet.INVD_Hrecid = INV_RECID"
            '...Forwarder Aux name
            Str &= " LEFT JOIN ACC01_Auxiliary ON AUX_Accno1 = SUBSTRING(INV_FrwdACNo, 1, 6)"
            Str &= " AND AUX_Accno2 = SUBSTRING(INV_FrwdACNo, 7, 4)"
            Str &= " AND AUX_Subno = SUBSTRING(INV_FrwdACNo, 11, 1) AND AUX_Comp = inv_Comp"
            Str &= " AND Aux_Year=YEAR(INV_DELIVERY)"
            'CH @ 02/05/2017 (RITA SUGGESTION) ERROR ON PRINTING PURCHASE INVOICE DUPLICATE ITEM IN REPORT ( NO SPECIFIC CURRUNCY ) 
            Str &= " AND AUX_CURR=INV_CURR "
            'If Details ...Qty Formuals In Measurement Unit
            If Mode = 0 Then
                Str &= " LEFT JOIN (SELECT mod_hrecid,"
                Str &= " ISNULL(SUM(CASE WHEN MOD_Stat='F' and Meas_Type='L' THEN round(Qty," & gbDecNo & ") ELSE 0 END),0) AS FLQTY," '..Free-Lenght
                Str &= " ISNULL(SUM(CASE WHEN MOD_Stat='N' and Meas_Type='L' THEN round(Qty," & gbDecNo & ") ELSE 0 END),0) AS NLQTY," '...Normal-lenght
                Str &= " ISNULL(SUM(CASE WHEN MOD_Stat='R' and Meas_Type='L' THEN round(Qty," & gbDecNo & ") ELSE 0 END),0) AS RLQTY," '...Return-lenght
                Str &= " ISNULL(SUM(CASE WHEN MOD_Stat='F' and Meas_Type='W' THEN round(Qty," & gbDecNo & ") ELSE 0 END),0) AS FWQTY," '...free- weight
                Str &= " ISNULL(SUM(CASE WHEN MOD_Stat='N' and Meas_Type='W' THEN round(Qty," & gbDecNo & ") ELSE 0 END),0) AS NWQTY," '...normal- weight
                Str &= " ISNULL(SUM(CASE WHEN MOD_Stat='R' and Meas_Type='W' THEN round(Qty," & gbDecNo & ") ELSE 0 END),0) AS RWQTY," '...Return-weight
                Str &= " ISNULL(SUM(CASE WHEN MOD_Stat='F' and Meas_Type='P' THEN round(Qty," & gbDecNo & ") ELSE 0 END),0) AS FPQTY," '...free-piece
                Str &= " ISNULL(SUM(CASE WHEN MOD_Stat='N' and Meas_Type='P' THEN round(Qty," & gbDecNo & ") ELSE 0 END),0) AS NPQTY," '...normal piece
                Str &= " ISNULL(SUM(CASE WHEN MOD_Stat='R' and Meas_Type='P' THEN round(Qty," & gbDecNo & ") ELSE 0 END),0) AS RPQTY " '...Return-weight
                Str &= " FROM V_PORD2_PIGEtDetTot GROUP BY mod_hrecid) AS TempQty"
                Str &= " ON TempQty.mod_hrecid = STK04_InvHead.INV_RECID"
            End If

            If Recid <> "" Then 'added by GS on 07/09/16
                If Val(Recid) <> 0 Then Str &= " WHERE inv_recid = " & Recid
                'added by GS on 07/09/16
            Else
                Str &= " INNER JOIN " & TempPISel & " TempPISel on TempPISel.INV_RECID = STK04_InvHead.INV_RECID and Chk = 'True' "
            End If
            'GS on 07/09/16

            Execute(Str)
        Catch ex As Exception
            Throw ex
        End Try
    End Sub
    'Public Sub CreatePIDetailsTable(ByVal FormName As String,
    '                                ByVal Fashion As Boolean,
    '                                ByVal Mode As Integer,
    '                                ByVal Recid As String,
    '                                Optional ByVal TempPISel As String = "",
    '                                Optional ByVal ReportName As String = "",
    '                                Optional ByVal PriceType As String = "",
    '                                Optional ByVal MergeSameItems As Boolean = False) 'edited by GS on 07/09/16 (add TempPISel)
    '    Dim Str As String
    '    Dim KeyRef As String

    '    Dim KeySupp As Boolean = False
    '    Dim KeyCatg As Boolean = False
    '    Dim KeySerie As Boolean = False
    '    Dim KeyUsage As Boolean = False
    '    Dim KeyNewUsed As Boolean = False

    '    Dim KeyStyle As Boolean = False
    '    Dim KeyGender As Boolean = False
    '    Dim KeyGrid As Boolean = False
    '    Dim KeySeason As Boolean = False
    '    Dim KeyBrand As Boolean = False
    '    Try
    '        DropSqlObject("TempPID_" & FormName & IIf(Fashion, "Fashion", "").ToString & "_" & gbUser, "TABLE")

    '        KeyRef = ""
    '        For k = 0 To CurrItmKey.Length - 1
    '            'KeyRef &= "[" & CurrItmKeyPend(k) + "]+'/'+"
    '            If UCase(CurrItmKeyPend(k)) = "SFP_SERIE" Then
    '                KeyRef &= "Convert(VARCHAR,[" & CurrItmKeyPend(k) + "])+'/'+"
    '            Else
    '                KeyRef &= "[" & CurrItmKeyPend(k) + "]+'/'+"
    '            End If

    '            If UCase(CurrItmKey(k)) = "STK_SUPPLIER" Then KeySupp = True
    '            If UCase(CurrItmKey(k)) = "STK_CATEGORY" Then KeyCatg = True
    '            If UCase(CurrItmKey(k)) = "STK_SERIE" Then KeySerie = True
    '            If UCase(CurrItmKey(k)) = "STK_USAGE" Then KeyUsage = True
    '            If UCase(CurrItmKey(k)) = "STK_NEWUSED" Then KeyNewUsed = True

    '            If UCase(CurrItmKey(k)) = "STK_SEASON" Then KeySeason = True
    '            If UCase(CurrItmKey(k)) = "STK_BRAND" Then KeyBrand = True
    '            If UCase(CurrItmKey(k)) = "STK_STYLE" Then KeyStyle = True
    '            If UCase(CurrItmKey(k)) = "STK_GENDER" Then KeyGender = True

    '            If UCase(CurrItmKey(k)) = "STK_GRID" Then KeyGrid = True 'added by GS on 05/09/16
    '        Next
    '        KeyRef = KeyRef.Substring(0, KeyRef.Length - 5)

    '        Str = "SELECT INVD_HRECID, INVD_RECID, INVD_STKRECID, INVD_PENDRecId,"
    '        Str &= " INVD_SUPREF, INVD_STATUS, INVD_COUNTER,"
    '        For k = 0 To CurrItmKey.Length - 1
    '            Str &= " ISNULL(" & CurrItmKey(k) + ", " & CurrItmKeyPend(k) + ") AS " & CurrItmKey(k) + ","
    '        Next
    '        If KeySupp = False Then Str &= " ISNULL(Stk_supplier, SFP_SUPPLIER) AS Stk_supplier ,"
    '        If KeyCatg = False Then Str &= " ISNULL(Stk_Category, Sfp_Category) AS Stk_Category ,"
    '        If KeySerie = False Then Str &= " ISNULL(Stk_Serie, SFP_Serie) AS Stk_Serie ,"
    '        If KeyUsage = False Then Str &= " ISNULL(Stk_usage, SFP_Usage) AS Stk_usage ,"
    '        If KeyNewUsed = False Then Str &= " ISNULL(Stk_newused, SFP_newused) AS Stk_newused,"
    '        Str &= " ISNULL(STK_REF, " & KeyRef & ") AS STK_REF,"
    '        Str &= " CASE ISNULL(STK_ItmClass, SFP_ItmClass) WHEN 'E' THEN 'Expiry' WHEN 'F'"
    '        Str &= " THEN 'Fashion' WHEN 'T' THEN 'Trading' WHEN 'B' THEN 'Book' END AS STK_ItmClass,"
    '        Str &= " ISNULL(Stk_Desc, SFP_Desc) AS Stk_Desc, INVD_Remarks,"
    '        Str &= " isnull((SELECT INVB_Barcode + ' ' FROM STK04_InvDBE WHERE INVB_DrecId = INVD_RECID FOR XML PATH('')) "
    '        Str &= " ,( SELECT TOP 1 dbar_barcode FROM dbo.Stk04_DetBarcode WHERE dbar_stkid = INVD_STKRECID ORDER BY dbar_DSTMP DESC )) "
    '        Str &= " ItmBarcode, "
    '        If KeySeason = False Then Str &= " ISNULL(STK_Season, SFP_Season) AS STK_Season ,"
    '        Str &= " (SELECT Seas_Desc FROM Stk04_Season WHERE ISNULL(STK_Season, SFP_Season) = Seas_Code) AS Seas_Desc ,"

    '        If KeyBrand = False Then Str &= " ISNULL(STK_Brand, SFP_Brand) AS STK_Brand ,"
    '        Str &= " (SELECT br_Name FROM Stk04_Brand WHERE ISNULL(STK_Brand, SFP_Brand) = br_Code ) AS br_Name ,"

    '        If KeyGender = False Then Str &= " ISNULL(STK_Gender, SFP_Gender) AS STK_Gender ,"
    '        Str &= " (SELECT G_Desc FROM STK04_Gender WHERE ISNULL(STK_Gender, SFP_Gender) = G_Code ) AS G_Desc ,"

    '        Str &= " ISNULL(STK_ITMTYPE, SFP_ITMTYPE) AS STK_ITMTYPE ,"
    '        Str &= " (SELECT IT_DESC FROM STK04_ItemType WHERE ISNULL(STK_ITMTYPE, SFP_ITMTYPE) = IT_Code ) AS IT_DESC ,"

    '        If KeyStyle = False Then Str &= " ISNULL(STK_Style, SFP_Style) AS STK_Style,"

    '        If KeyGrid = False Then Str &= " ISNULL(STK_Grid, SFP_Grid) AS STK_Grid ," 'added by GS on 27/05/16 'edited by GS on 05/09/16 (add condition)

    '        Str &= " ISNULL(ID1.ID_CODE, ID2.ID_CODE) AS INVD_ITMIDENT ,"
    '        Str &= " ISNULL(ID1.ID_Desc, ID2.ID_Desc) AS ID_Desc ,"
    '        Str &= " ISNULL(stk_IMPLOC, SFP_ImpLoc) AS INVD_IMPLOC,"
    '        Str &= " ISNULL(stk_CONSIG, CASE WHEN SFP_NEWUSED ='C' THEN 'Y' ELSE 'N' END) AS INVD_CONSIG,"
    '        Str &= " ISNULL(STK_SERVICE, SFP_SerReg) AS INVD_SERVICE, ISNULL(stk_promo, 'N') AS INVD_PROMO,"
    '        Str &= " ISNULL(stk_QTYCTRL, 'N') AS INVD_QTYCTRL,"
    '        Str &= " ISNULL(stk_TRANSPOS, 'N') AS INVD_TRANSPOS, ISNULL(stk_CONSUM, 'N') AS INVD_CONSUM,"
    '        Str &= " INVD_PURQTY, INVD_PURUNIT,Meas_Desc AS Stk_SubUNIT, INVD_UCOST, INVD_ITMDISC,"
    '        Str &= " INVD_CSHDISC, INVD_UNCOST, INVD_TNCOST, INVD_ULANDCOSTCC, INVD_ULANDCOSTLC,"
    '        Str &= " INVD_ULANDCOSTFC, INVD_COEF, INVD_RETLC, INVD_RETCC, INVD_RETFC, INVD_LOCATION,"
    '        Str &= " ISNULL(STK_VATPER, SFP_VATPER) STK_VATPER, ISNULL(stk_fccode ,SFP_FCCode) stk_fccode,"
    '        Str &= " INVQ_SuppColor, INVQ_Color, Color_Desc, INVQ_RecID, INVQ_GridCode, INVQ_GridSize,"
    '        Str &= " INVQ_Recqty, ISNULL(STK_VATTYPE, SFP_VATType) STK_VATTYPE"
    '        'Abir T. @ 20.11.2013
    '        If Mode = 0 Then Str &= ",isnull(SP_LCTAXEX,0) SP_LCTAXEX,  isnull(SP_LCTTC,0) SP_LCTTC,  isnull(SP_FCTAXEX,0) SP_FCTAXEX,  isnull(SP_FCTTC,0) SP_FCTTC"
    '        Str &= "  ,  invd_discval "
    '        If Mode <> 0 AndAlso ReportName.ToUpper.Contains("PETMART") Then
    '            Str &= "  , CASE WHEN  [Curr1] = '" & gbCurr1 & "' THEN "
    '            Str &= " CASE WHEN	[bpType1]  = 'NT' THEN  stkprices1.SP_LCTAXEX  ELSE  stkprices1.SP_LCTTC END	"
    '            Str &= " Else"
    '            Str &= " CASE WHEN	[bpType1]  = 'NT' THEN  stkprices1.SP_FCTAXEX  ELSE  stkprices1.SP_FCTTC END	"
    '            Str &= " END	PIBasepriceType1 ,"
    '            Str &= " CASE WHEN  [Curr2] = '" & gbCurr1 & "' THEN "
    '            Str &= " CASE WHEN	[bpType2]  = 'NT' THEN  stkprices2.SP_LCTAXEX  ELSE  stkprices2.SP_LCTTC END	"
    '            Str &= " ELSE"
    '            Str &= " CASE WHEN	[bpType2]  = 'NT' THEN  stkprices2.SP_FCTAXEX  ELSE  stkprices2.SP_FCTTC END	"
    '            Str &= " END	PIBasepriceType2 "
    '            Str &= " , invd_salesLC2 , invd_salesCC2   "
    '        End If
    '        Str &= " INTO TempPID_" & FormName & IIf(Fashion, "Fashion", "").ToString & "_" & gbUser
    '        Str &= " FROM STK04_InvDET"
    '        Str &= " LEFT JOIN STK04_InvDQty ON INVD_RECID = INVQ_Drecid"
    '        Str &= " LEFT JOIN STK04_STKFIL on invd_stkrecid = stk_recid "
    '        Str &= " LEFT JOIN STK04_StkFilPENDing on INVD_PENDRecId = SFP_Recid"
    '        Str &= " LEFT JOIN stk04_itmidentif ID1 ON stk_itmident = ID1.ID_RECID"
    '        Str &= " LEFT JOIN stk04_itmidentif ID2 ON SFP_ItmIdent = ID2.ID_RECID"
    '        Str &= " LEFT JOIN STK04_Measurements ON INVD_PURUNIT = Meas_Unit"
    '        Str &= " LEFT JOIN Stk04_COLOR ON INVQ_Color = Color_Code"

    '        'Abir T. @ 20.11.2013
    '        If Mode = 0 Then
    '            Str &= " LEFT JOIN ( SELECT SP_LCTAXEX, SP_LCTTC, SP_FCTAXEX, SP_FCTTC, SP_StkRecId " & IIf(FormName = FrmRepPI.Name, "", ",INV_RECID HeadInvoiceRecID").ToString & "  "
    '            Str &= " FROM Stk04_stkprices  "
    '            If FormName <> FrmRepPI.Name Then Str &= " Inner JOIN dbo.STK04_InvHead ON SUBSTRING(INV_PIBasepriceType1,0,CHARINDEX('|',INV_PIBasepriceType1,0)) =SP_PriceType "
    '            Str &= " WHERE ISNULL(SP_StartingQty, 0) = 0  "
    '            If FormName = FrmRepPI.Name Then Str &= $" AND SP_PriceType= '{PriceType}' "
    '            Str &= " ) PriceTemp ON SP_StkRecId = STK_RECID " & IIf(FormName = FrmRepPI.Name, "", "AND PriceTemp.HeadInvoiceRecID = STK04_InvDET.INVD_HRECID").ToString & "  "
    '        Else
    '            If ReportName.ToUpper.Contains("PETMART") Then
    '                Str &= " inner join ( SELECT  inv_recid Hrecid  , SUBSTRING(INV_PIBasepriceType1,0,CHARINDEX('|',INV_PIBasepriceType1,0)) PriceType1, "
    '                Str &= "  SUBSTRING( SUBSTRING(INV_PIBasepriceType1, CHARINDEX('|',INV_PIBasepriceType1,0)+1 , LEN(INV_PIBasepriceType1))   ,0  , "
    '                Str &= "   CHARINDEX('|',SUBSTRING(INV_PIBasepriceType1, CHARINDEX('|',INV_PIBasepriceType1,0)+1 , LEN(INV_PIBasepriceType1)),0)  ) [Curr1] , "
    '                Str &= "  REVERSE(SUBSTRING( REVERSE(INV_PIBasepriceType1),0,CHARINDEX('|',REVERSE(INV_PIBasepriceType1),0))) [bpType1] ,"
    '                Str &= "   SUBSTRING(INV_PIBasepriceType2,0,CHARINDEX('|',INV_PIBasepriceType2,0)) PriceType2, "
    '                Str &= "  SUBSTRING( SUBSTRING(INV_PIBasepriceType2, CHARINDEX('|',INV_PIBasepriceType2,0)+1 , LEN(INV_PIBasepriceType2))   ,0  ,"
    '                Str &= "    CHARINDEX('|',SUBSTRING(INV_PIBasepriceType2, CHARINDEX('|',INV_PIBasepriceType2,0)+1 , LEN(INV_PIBasepriceType2)),0)  ) [Curr2] , "
    '                Str &= "  REVERSE(SUBSTRING( REVERSE(INV_PIBasepriceType2),0,CHARINDEX('|',REVERSE(INV_PIBasepriceType2),0))) [bpType2]"
    '                Str &= "     FROM  stk04_invhead   )  TBLPRICES on  Hrecid = INVD_HRECID   "
    '                Str &= " left JOIN ( SELECT SP_StkRecId, SP_PriceType, SP_LCTAXEX, SP_LCTTC, SP_FCTAXEX, SP_FCTTC, SP_FCCODE  
    '                                     FROM dbo.Stk04_stkprices
    '                                     WHERE ISNULL(SP_StartingQty, 0) = 0 ) stkprices1 ON INVD_STKRECID = stkprices1.SP_StkRecId AND stkprices1.SP_PriceType = PriceType1 "
    '                Str &= " left JOIN ( SELECT SP_StkRecId, SP_PriceType, SP_LCTAXEX, SP_LCTTC, SP_FCTAXEX, SP_FCTTC, SP_FCCODE 
    '                                     FROM dbo.Stk04_stkprices
    '                                     WHERE ISNULL(SP_StartingQty, 0) = 0 ) stkprices2 ON INVD_STKRECID = stkprices2.SP_StkRecId AND stkprices2.SP_PriceType = PriceType2 "
    '            End If
    '        End If
    '        If Recid <> "" Then 'added by GS on 07/09/16
    '            Str &= " WHERE 1 = 1"
    '            If Val(Recid) <> 0 Then Str &= " AND INVd_HRECID = " & Recid
    '            'added by GS on 07/09/16
    '        Else
    '            Str &= " INNER JOIN " & TempPISel & " TempPISel on TempPISel.INV_RECID = STK04_InvDET.INVd_HRECID and Chk = 'True' "
    '        End If
    '        'GS on 07/09/16
    '        Execute(Str)
    '    Catch ex As Exception
    '        Throw ex
    '    End Try
    'End Sub

    Public Sub CreatePIDetailsTable(ByVal FormName As String,
                                ByVal Fashion As Boolean,
                                ByVal Mode As Integer,
                                ByVal Recid As String,
                                Optional ByVal TempPISel As String = "",
                                Optional ByVal ReportName As String = "",
                                Optional ByVal PriceType As String = "",
                                Optional ByVal MergeSameItems As Boolean = False) 'edited by GS on 07/09/16 (add TempPISel)
        Dim Str As String
        Dim KeyRef As String

        Dim KeySupp As Boolean = False
        Dim KeyCatg As Boolean = False
        Dim KeySerie As Boolean = False
        Dim KeyUsage As Boolean = False
        Dim KeyNewUsed As Boolean = False

        Dim KeyStyle As Boolean = False
        Dim KeyGender As Boolean = False
        Dim KeyGrid As Boolean = False
        Dim KeySeason As Boolean = False
        Dim KeyBrand As Boolean = False

        Dim FinalTable As String = "TempPID_" & FormName & IIf(Fashion, "Fashion", "").ToString & "_" & gbUser
        Dim RawTable As String = "TempPID_raw_" & FormName & IIf(Fashion, "Fashion", "").ToString & "_" & gbUser
        Dim TargetTable As String = IIf(MergeSameItems, RawTable, FinalTable).ToString

        Try
            DropSqlObject(FinalTable, "TABLE")
            If MergeSameItems Then DropSqlObject(RawTable, "TABLE")

            KeyRef = ""
            For k = 0 To CurrItmKey.Length - 1
                If UCase(CurrItmKeyPend(k)) = "SFP_SERIE" Then
                    KeyRef &= "Convert(VARCHAR,[" & CurrItmKeyPend(k) + "])+'/'+"
                Else
                    KeyRef &= "[" & CurrItmKeyPend(k) + "]+'/'+"
                End If

                If UCase(CurrItmKey(k)) = "STK_SUPPLIER" Then KeySupp = True
                If UCase(CurrItmKey(k)) = "STK_CATEGORY" Then KeyCatg = True
                If UCase(CurrItmKey(k)) = "STK_SERIE" Then KeySerie = True
                If UCase(CurrItmKey(k)) = "STK_USAGE" Then KeyUsage = True
                If UCase(CurrItmKey(k)) = "STK_NEWUSED" Then KeyNewUsed = True

                If UCase(CurrItmKey(k)) = "STK_SEASON" Then KeySeason = True
                If UCase(CurrItmKey(k)) = "STK_BRAND" Then KeyBrand = True
                If UCase(CurrItmKey(k)) = "STK_STYLE" Then KeyStyle = True
                If UCase(CurrItmKey(k)) = "STK_GENDER" Then KeyGender = True

                If UCase(CurrItmKey(k)) = "STK_GRID" Then KeyGrid = True 'added by GS on 05/09/16
            Next
            KeyRef = KeyRef.Substring(0, KeyRef.Length - 5)

            Str = "SELECT INVD_HRECID, INVD_RECID, INVD_STKRECID, INVD_PENDRecId,"
            Str &= " INVD_SUPREF, INVD_STATUS, INVD_COUNTER,"
            For k = 0 To CurrItmKey.Length - 1
                Str &= " ISNULL(" & CurrItmKey(k) + ", " & CurrItmKeyPend(k) + ") AS " & CurrItmKey(k) + ","
            Next
            If KeySupp = False Then Str &= " ISNULL(Stk_supplier, SFP_SUPPLIER) AS Stk_supplier ,"
            If KeyCatg = False Then Str &= " ISNULL(Stk_Category, Sfp_Category) AS Stk_Category ,"
            If KeySerie = False Then Str &= " ISNULL(Stk_Serie, SFP_Serie) AS Stk_Serie ,"
            If KeyUsage = False Then Str &= " ISNULL(Stk_usage, SFP_Usage) AS Stk_usage ,"
            If KeyNewUsed = False Then Str &= " ISNULL(Stk_newused, SFP_newused) AS Stk_newused,"
            Str &= " ISNULL(STK_REF, " & KeyRef & ") AS STK_REF,"
            Str &= " CASE ISNULL(STK_ItmClass, SFP_ItmClass) WHEN 'E' THEN 'Expiry' WHEN 'F'"
            Str &= " THEN 'Fashion' WHEN 'T' THEN 'Trading' WHEN 'B' THEN 'Book' END AS STK_ItmClass,"
            Str &= " ISNULL(Stk_Desc, SFP_Desc) AS Stk_Desc, INVD_Remarks,"
            Str &= " isnull((SELECT INVB_Barcode + ' ' FROM STK04_InvDBE WHERE INVB_DrecId = INVD_RECID FOR XML PATH('')) "
            Str &= " ,( SELECT TOP 1 dbar_barcode FROM dbo.Stk04_DetBarcode WHERE dbar_stkid = INVD_STKRECID ORDER BY dbar_DSTMP DESC )) "
            Str &= " ItmBarcode, "
            If KeySeason = False Then Str &= " ISNULL(STK_Season, SFP_Season) AS STK_Season ,"
            Str &= " (SELECT Seas_Desc FROM Stk04_Season WHERE ISNULL(STK_Season, SFP_Season) = Seas_Code) AS Seas_Desc ,"

            If KeyBrand = False Then Str &= " ISNULL(STK_Brand, SFP_Brand) AS STK_Brand ,"
            Str &= " (SELECT br_Name FROM Stk04_Brand WHERE ISNULL(STK_Brand, SFP_Brand) = br_Code ) AS br_Name ,"

            If KeyGender = False Then Str &= " ISNULL(STK_Gender, SFP_Gender) AS STK_Gender ,"
            Str &= " (SELECT G_Desc FROM STK04_Gender WHERE ISNULL(STK_Gender, SFP_Gender) = G_Code ) AS G_Desc ,"

            Str &= " ISNULL(STK_ITMTYPE, SFP_ITMTYPE) AS STK_ITMTYPE ,"
            Str &= " (SELECT IT_DESC FROM STK04_ItemType WHERE ISNULL(STK_ITMTYPE, SFP_ITMTYPE) = IT_Code ) AS IT_DESC ,"

            If KeyStyle = False Then Str &= " ISNULL(STK_Style, SFP_Style) AS STK_Style,"

            If KeyGrid = False Then Str &= " ISNULL(STK_Grid, SFP_Grid) AS STK_Grid ," 'added by GS on 27/05/16 'edited by GS on 05/09/16 (add condition)

            Str &= " ISNULL(ID1.ID_CODE, ID2.ID_CODE) AS INVD_ITMIDENT ,"
            Str &= " ISNULL(ID1.ID_Desc, ID2.ID_Desc) AS ID_Desc ,"
            Str &= " ISNULL(stk_IMPLOC, SFP_ImpLoc) AS INVD_IMPLOC,"
            Str &= " ISNULL(stk_CONSIG, CASE WHEN SFP_NEWUSED ='C' THEN 'Y' ELSE 'N' END) AS INVD_CONSIG,"
            Str &= " ISNULL(STK_SERVICE, SFP_SerReg) AS INVD_SERVICE, ISNULL(stk_promo, 'N') AS INVD_PROMO,"
            Str &= " ISNULL(stk_QTYCTRL, 'N') AS INVD_QTYCTRL,"
            Str &= " ISNULL(stk_TRANSPOS, 'N') AS INVD_TRANSPOS, ISNULL(stk_CONSUM, 'N') AS INVD_CONSUM,"
            Str &= " INVD_PURQTY, INVD_PURUNIT,Meas_Desc AS Stk_SubUNIT, INVD_UCOST, INVD_ITMDISC,"
            Str &= " INVD_CSHDISC, INVD_UNCOST, INVD_TNCOST, INVD_ULANDCOSTCC, INVD_ULANDCOSTLC,"
            Str &= " INVD_ULANDCOSTFC, INVD_COEF, INVD_RETLC, INVD_RETCC, INVD_RETFC, INVD_LOCATION,"
            Str &= " ISNULL(STK_VATPER, SFP_VATPER) STK_VATPER, ISNULL(stk_fccode ,SFP_FCCode) stk_fccode,"
            Str &= " INVQ_SuppColor, INVQ_Color, Color_Desc, INVQ_RecID, INVQ_GridCode, INVQ_GridSize,"
            Str &= " INVQ_Recqty, ISNULL(STK_VATTYPE, SFP_VATType) STK_VATTYPE"
            'Abir T. @ 20.11.2013
            If Mode = 0 Then Str &= ",isnull(SP_LCTAXEX,0) SP_LCTAXEX,  isnull(SP_LCTTC,0) SP_LCTTC,  isnull(SP_FCTAXEX,0) SP_FCTAXEX,  isnull(SP_FCTTC,0) SP_FCTTC"
            Str &= "  ,  invd_discval "
            If Mode <> 0 AndAlso ReportName.ToUpper.Contains("PETMART") Then
                Str &= "  , CASE WHEN  [Curr1] = '" & gbCurr1 & "' THEN "
                Str &= " CASE WHEN	[bpType1]  = 'NT' THEN  stkprices1.SP_LCTAXEX  ELSE  stkprices1.SP_LCTTC END	"
                Str &= " Else"
                Str &= " CASE WHEN	[bpType1]  = 'NT' THEN  stkprices1.SP_FCTAXEX  ELSE  stkprices1.SP_FCTTC END	"
                Str &= " END	PIBasepriceType1 ,"
                Str &= " CASE WHEN  [Curr2] = '" & gbCurr1 & "' THEN "
                Str &= " CASE WHEN	[bpType2]  = 'NT' THEN  stkprices2.SP_LCTAXEX  ELSE  stkprices2.SP_LCTTC END	"
                Str &= " ELSE"
                Str &= " CASE WHEN	[bpType2]  = 'NT' THEN  stkprices2.SP_FCTAXEX  ELSE  stkprices2.SP_FCTTC END	"
                Str &= " END	PIBasepriceType2 "
                Str &= " , invd_salesLC2 , invd_salesCC2   "
            End If
            Str &= " INTO " & TargetTable
            Str &= " FROM STK04_InvDET"
            Str &= " LEFT JOIN STK04_InvDQty ON INVD_RECID = INVQ_Drecid"
            Str &= " LEFT JOIN STK04_STKFIL on invd_stkrecid = stk_recid "
            Str &= " LEFT JOIN STK04_StkFilPENDing on INVD_PENDRecId = SFP_Recid"
            Str &= " LEFT JOIN stk04_itmidentif ID1 ON stk_itmident = ID1.ID_RECID"
            Str &= " LEFT JOIN stk04_itmidentif ID2 ON SFP_ItmIdent = ID2.ID_RECID"
            Str &= " LEFT JOIN STK04_Measurements ON INVD_PURUNIT = Meas_Unit"
            Str &= " LEFT JOIN Stk04_COLOR ON INVQ_Color = Color_Code"

            'Abir T. @ 20.11.2013
            If Mode = 0 Then
                Str &= " LEFT JOIN ( SELECT SP_LCTAXEX, SP_LCTTC, SP_FCTAXEX, SP_FCTTC, SP_StkRecId " & IIf(FormName = FrmRepPI.Name, "", ",INV_RECID HeadInvoiceRecID").ToString & "  "
                Str &= " FROM Stk04_stkprices  "
                If FormName <> FrmRepPI.Name Then Str &= " Inner JOIN dbo.STK04_InvHead ON SUBSTRING(INV_PIBasepriceType1,0,CHARINDEX('|',INV_PIBasepriceType1,0)) =SP_PriceType "
                Str &= " WHERE ISNULL(SP_StartingQty, 0) = 0  "
                If FormName = FrmRepPI.Name Then Str &= $" AND SP_PriceType= '{PriceType}' "
                Str &= " ) PriceTemp ON SP_StkRecId = STK_RECID " & IIf(FormName = FrmRepPI.Name, "", "AND PriceTemp.HeadInvoiceRecID = STK04_InvDET.INVD_HRECID").ToString & "  "
            Else
                If ReportName.ToUpper.Contains("PETMART") Then
                    Str &= " inner join ( SELECT  inv_recid Hrecid  , SUBSTRING(INV_PIBasepriceType1,0,CHARINDEX('|',INV_PIBasepriceType1,0)) PriceType1, "
                    Str &= "  SUBSTRING( SUBSTRING(INV_PIBasepriceType1, CHARINDEX('|',INV_PIBasepriceType1,0)+1 , LEN(INV_PIBasepriceType1))   ,0  , "
                    Str &= "   CHARINDEX('|',SUBSTRING(INV_PIBasepriceType1, CHARINDEX('|',INV_PIBasepriceType1,0)+1 , LEN(INV_PIBasepriceType1)),0)  ) [Curr1] , "
                    Str &= "  REVERSE(SUBSTRING( REVERSE(INV_PIBasepriceType1),0,CHARINDEX('|',REVERSE(INV_PIBasepriceType1),0))) [bpType1] ,"
                    Str &= "   SUBSTRING(INV_PIBasepriceType2,0,CHARINDEX('|',INV_PIBasepriceType2,0)) PriceType2, "
                    Str &= "  SUBSTRING( SUBSTRING(INV_PIBasepriceType2, CHARINDEX('|',INV_PIBasepriceType2,0)+1 , LEN(INV_PIBasepriceType2))   ,0  ,"
                    Str &= "    CHARINDEX('|',SUBSTRING(INV_PIBasepriceType2, CHARINDEX('|',INV_PIBasepriceType2,0)+1 , LEN(INV_PIBasepriceType2)),0)  ) [Curr2] , "
                    Str &= "  REVERSE(SUBSTRING( REVERSE(INV_PIBasepriceType2),0,CHARINDEX('|',REVERSE(INV_PIBasepriceType2),0))) [bpType2]"
                    Str &= "     FROM  stk04_invhead   )  TBLPRICES on  Hrecid = INVD_HRECID   "
                    Str &= " left JOIN ( SELECT SP_StkRecId, SP_PriceType, SP_LCTAXEX, SP_LCTTC, SP_FCTAXEX, SP_FCTTC, SP_FCCODE  
                                     FROM dbo.Stk04_stkprices
                                     WHERE ISNULL(SP_StartingQty, 0) = 0 ) stkprices1 ON INVD_STKRECID = stkprices1.SP_StkRecId AND stkprices1.SP_PriceType = PriceType1 "
                    Str &= " left JOIN ( SELECT SP_StkRecId, SP_PriceType, SP_LCTAXEX, SP_LCTTC, SP_FCTAXEX, SP_FCTTC, SP_FCCODE 
                                     FROM dbo.Stk04_stkprices
                                     WHERE ISNULL(SP_StartingQty, 0) = 0 ) stkprices2 ON INVD_STKRECID = stkprices2.SP_StkRecId AND stkprices2.SP_PriceType = PriceType2 "
                End If
            End If
            If Recid <> "" Then 'added by GS on 07/09/16
                Str &= " WHERE 1 = 1"
                If Val(Recid) <> 0 Then Str &= " AND INVd_HRECID = " & Recid
                'added by GS on 07/09/16
            Else
                Str &= " INNER JOIN " & TempPISel & " TempPISel on TempPISel.INV_RECID = STK04_InvDET.INVd_HRECID and Chk = 'True' "
            End If
            'GS on 07/09/16
            Execute(Str)

            If MergeSameItems Then

                Dim GroupCols As New List(Of String)
                Dim SumCols As New List(Of String)
                Dim MaxCols As New List(Of String)

                GroupCols.Add("INVD_HRECID")
                GroupCols.Add("INVD_STKRECID")
                GroupCols.Add("INVD_SUPREF")
                GroupCols.Add("INVD_STATUS")
                GroupCols.Add("STK_ItmClass")
                GroupCols.Add("STK_ITMTYPE")
                GroupCols.Add("IT_DESC")
                GroupCols.Add("INVD_ITMIDENT")
                GroupCols.Add("ID_Desc")
                GroupCols.Add("INVQ_SuppColor")
                GroupCols.Add("INVQ_Color")
                GroupCols.Add("INVQ_GridCode")
                GroupCols.Add("INVQ_GridSize")
                GroupCols.Add("STK_VATPER")
                GroupCols.Add("stk_fccode")
                GroupCols.Add("INVD_UCOST")
                GroupCols.Add("INVD_ITMDISC")
                GroupCols.Add("INVD_CSHDISC")
                GroupCols.Add("INVD_UNCOST")
                GroupCols.Add("INVD_ULANDCOSTCC")
                GroupCols.Add("INVD_ULANDCOSTLC")
                GroupCols.Add("INVD_ULANDCOSTFC")
                GroupCols.Add("INVD_COEF")
                GroupCols.Add("STK_VATTYPE")
                GroupCols.Add("invd_discval")
                If Mode = 0 Then
                    GroupCols.Add("SP_LCTAXEX")
                    GroupCols.Add("SP_LCTTC")
                    GroupCols.Add("SP_FCTAXEX")
                    GroupCols.Add("SP_FCTTC")
                End If

                For k = 0 To CurrItmKey.Length - 1
                    If Not GroupCols.Contains(CurrItmKey(k)) Then GroupCols.Add(CurrItmKey(k))
                Next

                If KeyNewUsed = False Then GroupCols.Add("Stk_newused")
                If KeySeason = False Then GroupCols.Add("STK_Season")
                If KeyBrand = False Then GroupCols.Add("STK_Brand")
                If KeyStyle = False Then GroupCols.Add("STK_Style")
                If KeyGrid = False Then GroupCols.Add("STK_Grid")

                SumCols.Add("INVD_PURQTY")
                SumCols.Add("INVQ_Recqty")
                SumCols.Add("INVD_TNCOST")
                SumCols.Add("INVD_RETLC")
                SumCols.Add("INVD_RETCC")
                SumCols.Add("INVD_RETFC")
                If Mode <> 0 AndAlso ReportName.ToUpper.Contains("PETMART") Then
                    SumCols.Add("invd_salesLC2")
                    SumCols.Add("invd_salesCC2")
                End If

                MaxCols.Add("INVD_RECID")
                MaxCols.Add("INVD_PENDRecId")
                MaxCols.Add("INVD_COUNTER")
                If KeySupp = False Then MaxCols.Add("Stk_supplier")
                If KeyCatg = False Then MaxCols.Add("Stk_Category")
                If KeySerie = False Then MaxCols.Add("Stk_Serie")
                If KeyUsage = False Then MaxCols.Add("Stk_usage")
                MaxCols.Add("STK_REF")
                MaxCols.Add("Stk_Desc")
                MaxCols.Add("INVD_Remarks")
                MaxCols.Add("ItmBarcode")
                MaxCols.Add("Seas_Desc")
                MaxCols.Add("br_Name")
                If KeyGender = False Then MaxCols.Add("STK_Gender")
                MaxCols.Add("G_Desc")
                MaxCols.Add("INVD_IMPLOC")
                MaxCols.Add("INVD_CONSIG")
                MaxCols.Add("INVD_SERVICE")
                MaxCols.Add("INVD_PROMO")
                MaxCols.Add("INVD_QTYCTRL")
                MaxCols.Add("INVD_TRANSPOS")
                MaxCols.Add("INVD_CONSUM")
                MaxCols.Add("INVD_PURUNIT")
                MaxCols.Add("Stk_SubUNIT")
                MaxCols.Add("INVD_LOCATION")
                MaxCols.Add("Color_Desc")
                MaxCols.Add("INVQ_RecID")
                If Mode <> 0 AndAlso ReportName.ToUpper.Contains("PETMART") Then
                    MaxCols.Add("PIBasepriceType1")
                    MaxCols.Add("PIBasepriceType2")
                End If

                Dim Str2 As String = "SELECT "
                For Each c As String In GroupCols
                    Str2 &= c & ", "
                Next
                For Each c As String In MaxCols
                    Str2 &= "MAX(" & c & ") AS " & c & ", "
                Next
                For Each c As String In SumCols
                    Str2 &= "SUM(" & c & ") AS " & c & ", "
                Next
                Str2 = Str2.Substring(0, Str2.Length - 2)
                Str2 &= " INTO " & FinalTable
                Str2 &= " FROM " & RawTable
                Str2 &= " GROUP BY " & String.Join(", ", GroupCols)

                Execute(Str2)
                DropSqlObject(RawTable, "TABLE")
            End If

        Catch ex As Exception
            Throw ex
        End Try
    End Sub
    Public Sub CreateFwdTable(ByVal FormName As String, ByVal Fashion As Boolean)
        Dim Str As String
        Try
            DropSqlObject("Tmp_FwdName_" & FormName & IIf(Fashion, "Fashion", "").ToString & "_" & gbUser, "TABLE")

            Str = "Select Main.FPI_InvHeadId, Left(Main.splitName,Len(Main.splitName)-1) AS FwdName "
            Str &= " INTO Tmp_FwdName_" & FormName & IIf(Fashion, "Fashion", "").ToString & "_" & gbUser & " FROM ("
            Str &= " SELECT TOP 1 ( SELECT st1.FWD_NAME + ', ' AS [text()]"
            Str &= " FROM (SELECT DISTINCT FPI_InvHeadId, FWD_CODE,FWD_NAME"
            Str &= " FROM TempPIH_" & FormName & IIf(Fashion, "Fashion", "").ToString & "_" & gbUser
            Str &= " INNER JOIN STK04_FwdPurInv ON FPI_InvHeadId=inv_recid"
            Str &= " INNER JOIN STK04_FwdInvDet ON FPI_FWDRECID=FID_RECID"
            Str &= " INNER JOIN STK04_FwdInvHead ON FID_HRECID=FIH_RECID"
            Str &= " INNER JOIN dbo.STK04_Forwarder ON FIH_FWDCODE=FWD_RECID)  st1"
            Str &= " ORDER BY st1.FWD_CODE FOR XML PATH('') ) AS splitname, FPI_InvHeadId"
            Str &= " FROM (SELECT FPI_InvHeadId,FWD_CODE,FWD_NAME"
            Str &= " FROM TempPIH_" & FormName & IIf(Fashion, "Fashion", "").ToString & "_" & gbUser
            Str &= " INNER JOIN STK04_FwdPurInv ON FPI_InvHeadId=inv_recid"
            Str &= " INNER JOIN STK04_FwdInvDet ON FPI_FWDRECID=FID_RECID"
            Str &= " INNER JOIN STK04_FwdInvHead ON FID_HRECID=FIH_RECID"
            Str &= " INNER JOIN dbo.STK04_Forwarder ON FIH_FWDCODE=FWD_RECID) st2) [Main]"
            Execute(Str)

            Str = "ALTER TABLE Tmp_FwdName_" & FormName & IIf(Fashion, "Fashion", "").ToString & "_" & gbUser
            Str &= " ALTER COLUMN FwdName nvarchar(4000) collate SQL_Latin1_General_CP1256_CI_AS "
            Execute(Str)

        Catch ex As Exception
            Throw ex
        End Try
    End Sub

    'Abir T. @ 21.05.2014
    Public Function GetShipInfoFields(ByVal TempTable As String, Optional ByVal servername As String = "") As String 'modified by GS on 07/09/16 (add serverlink)
        Dim strsql As String
        Try
            DropSqlObject(TempTable, "TABLE")
            strsql = "Select STK04_POShipInfo.*, Aux_Name"
            strsql &= " INTO " & TempTable
            strsql &= " FROM STK04_POShipInfo"
            strsql &= " LEFT JOIN " & servername & "ACC01_Auxiliary"
            strsql &= " On SUBSTRING(POSI_AuxAcc,1,6) collate SQL_Latin1_General_CP1256_CI_AS = Aux_accno1 collate SQL_Latin1_General_CP1256_CI_AS"
            strsql &= " And SUBSTRING(POSI_AuxAcc,7,4) collate SQL_Latin1_General_CP1256_CI_AS = Aux_accno2 collate SQL_Latin1_General_CP1256_CI_AS"
            strsql &= " And SUBSTRING(POSI_AuxAcc,11,1) collate SQL_Latin1_General_CP1256_CI_AS = Aux_SubNo collate SQL_Latin1_General_CP1256_CI_AS"
            strsql &= " And SUBSTRING(POSI_AuxAcc,12,3) collate SQL_Latin1_General_CP1256_CI_AS = Aux_Curr collate SQL_Latin1_General_CP1256_CI_AS"
            strsql &= " And POSI_CompCode collate SQL_Latin1_General_CP1256_CI_AS = Aux_Comp collate SQL_Latin1_General_CP1256_CI_AS"
            strsql &= " And POSI_Year collate SQL_Latin1_General_CP1256_CI_AS = Aux_Year collate SQL_Latin1_General_CP1256_CI_AS"
            Execute(strsql)

            GetShipInfoFields = " , convert(nvarchar(200),POSI_ShipMode)  AS [Ship Mode], POSI_TruckName AS [Truck Name],"
            GetShipInfoFields &= " POSI_Consignee AS [Consignee], POSI_Notify AS [Notify], POSI_Shipper AS [Shipper],"
            GetShipInfoFields &= " POSI_ShippedTo AS [Shipped To], POSI_SoldTo AS [Sold To], POSI_Payment AS [Payment To],"
            GetShipInfoFields &= " POSI_PortOfLoad AS [Port OfLoad], POSI_MasterBL AS [Master BL],"
            GetShipInfoFields &= " POSI_HouseBL AS [House BL], POSI_Delivery AS [Delivery], POSI_Validity AS [Validity],"
            GetShipInfoFields &= " POSI_BankDetails AS [Bank Details], POSI_ContainerRef AS [Container Reference],"
            GetShipInfoFields &= " POSI_AuxAcc AS [Aux. Acc.], Aux_Name AS [Acc. Name],"
            GetShipInfoFields &= " CONVERT(DATETIME, CONVERT(VARCHAR, POSI_ExShipDate), 101) AS [Ex. Ship. Date],"
            GetShipInfoFields &= " CONVERT(DATETIME, CONVERT(VARCHAR, POSI_ActShipDate), 101) AS [Act. Ship. Date],"
            GetShipInfoFields &= " CONVERT(DATETIME, CONVERT(VARCHAR, POSI_ExpArvlDate), 101) AS [Ex. Arvl. Date],"
            GetShipInfoFields &= " POSI_ExpArvlRks AS [Ex. Arvl. Rks],"
            GetShipInfoFields &= " CONVERT(DATETIME, CONVERT(VARCHAR, POSI_ActArvlDate), 101) AS [Act. Arvl. Date],"
            GetShipInfoFields &= " POSI_ActArvlRks AS [Act. Arvl. Rks]"
            GetShipInfoFields &= " , POSI_NoContainers AS [No Containers] "

        Catch ex As Exception
            Throw ex
        End Try
    End Function
    Public Function GetFashionFields(ByVal Source As String, ByVal MaxCnt As Integer) As String
        Dim strsql As String
        Dim drSQL As SqlDataReader
        Try
            GetFashionFields = ""
            Select Case Source
                Case "PI"
                    GetFashionFields &= " DET.INVQ_SuppColor AS [Supp. Color],"
                    GetFashionFields &= " DET.G_Desc AS [Gender],"
                    GetFashionFields &= " DET.IT_DESC AS [Product Group],"
                    GetFashionFields &= " DET.STK_Style AS [Style],"
                    GetFashionFields &= " DET.Color_Desc AS [Color],"
                    GetFashionFields &= " DET.INVQ_GridCode AS [Grid],"
                    For i = 1 To MaxCnt
                        GetFashionFields &= " convert(varchar(5),GS.Size" & i & ") Size" & i & " , "
                    Next
                    If MaxCnt < 30 Then
                        For i As Integer = (MaxCnt + 1) To 30
                            GetFashionFields &= " convert(varchar(5),'') AS Size" & i & ","
                        Next
                    End If

                    For j = 1 To MaxCnt
                        GetFashionFields &= " convert(numeric(18,0),T.Size" & j & ") AS Qty" & j & ", "
                    Next

                    If MaxCnt < 30 Then
                        For i As Integer = (MaxCnt + 1) To 30
                            GetFashionFields &= " convert(numeric(18,0),0) AS Qty" & i & ","
                        Next
                    End If
                Case "PO"
                    GetFashionFields &= " V.MOD_SuppColor AS [Supp. Color], V.G_Desc AS [Gender], V.IT_DESC AS [Product Group],"
                    GetFashionFields &= " V.STK_Style AS [Style], V.Color_Desc AS [Color], V.MOD_GridCode AS [Grid],"
                    For i = 1 To MaxCnt
                        GetFashionFields &= " GS.Size" & i & ", "
                    Next
                    If MaxCnt < 30 Then
                        For i As Integer = (MaxCnt + 1) To 30
                            GetFashionFields &= " '' AS Size" & i & ","
                        Next
                    End If

                    For j = 1 To MaxCnt
                        GetFashionFields &= " T.Size" & j & " AS Qty" & j & ", "
                    Next
                    If MaxCnt < 30 Then
                        For i As Integer = (MaxCnt + 1) To 30
                            GetFashionFields &= " 0 AS Qty" & i & ","
                        Next
                    End If
            End Select
        Catch ex As Exception
            Throw ex
        End Try
    End Function


    Public Sub PrintPI(ByVal frm As Form, ByVal Recid As String, ByVal bMode As String, ByVal VAT As Integer,
                          ByVal Mode As Integer, ByVal LandedCost As Integer, ByVal Fashion As Boolean,
                          ByVal TempGridSize As String, ByVal OrderSupRef As Boolean, Optional ByVal Rev As String = "",
                          Optional ByVal PrintExpiry As Boolean = False, Optional ByVal ShipInfo As Boolean = False,
                          Optional ByVal PrintDirect As Boolean = False, Optional ByVal NbPrint As Integer = 1,
                          Optional ByVal Reprint As Boolean = False, Optional ByVal locationCode As String = "",
                          Optional ByVal servername As String = "", Optional ByVal TempPISel As String = "" _
                         , Optional ByVal PrintTechContent As Boolean = False, Optional ShowLineNumber As Boolean = False,
                         Optional ShowLastCost As Boolean = False, Optional onlyupdate As Boolean = True, Optional PrintTransferToLoc As Boolean = False,
                         Optional PrintAvailableQty As Boolean = False, Optional tmpAvailableQtyLocFiltr As String = "",
                       Optional PrintFundDisbursement As Boolean = False, Optional BasePriceType As String = "", Optional MergeSameItem As Boolean = False)
        '..If Mode = 0 THEN Detailed '..Simple ELSE Regular
        Dim strsql As String
        Dim Str As String
        Dim drSql As SqlDataReader
        Dim drSql2 As SqlDataReader
        Dim ReportName As String
        Dim strFashion As String
        Dim MaxCnt As Integer
        Dim strShip As String
        Dim DIRECTORDER As String
        Dim TempTable As String
        Dim KeyGender As Boolean
        Dim KeyStyle As Boolean
        Dim TempPISubRep1 As String
        Dim TempPISubRep2 As String
        Dim Currency As String
        Dim hasCostcenter As Boolean
        Dim costcenterName, costcenterNumber As String
        Try

            Fashion = False

            ReportName = ExecuteScalar_WOXACT(" select  isnull(SP_PIReport,'') from stk04_sysparam")
            If ReportName.ToUpper.Contains("PETMART") AndAlso PrintFundDisbursement Then
                ReportName = "rptPurInvList_SimplePetMart_WithFunds.rpt"
            End If
            If (Not locationCode Is Nothing) AndAlso locationCode.Trim <> "" Then
                strsql = "SELECT DL_CostCenter FROM  dbo.STK04_LocAcDir WHERE DL_Loc ='" & locationCode.Trim & "'"
                If RecordsExists(strsql) Then
                    If ExecuteScalar(strsql).ToString.Trim <> "" Then
                        costcenterNumber = ExecuteScalar(strsql).ToString.Trim
                        strsql = " SELECT Aux_accno2+'-'+Aux_Name from " & servername & "ACC01_Auxiliary"
                        strsql &= " WHERE Aux_accno1 + right(space(4) + Aux_accno2,4) +Aux_subno ='" & costcenterNumber & "'AND Aux_Comp='" & gbComp & "'"
                        strsql &= " AND Aux_Year ='" & ACYear & "'"
                        costcenterName = ExecuteScalar(strsql).ToString
                        hasCostcenter = True
                    Else
                        hasCostcenter = False
                    End If
                Else
                    hasCostcenter = False
                End If
            Else
                hasCostcenter = False
            End If
            strsql = "SELECT ISNULL(MAX(GD_Order), 0) FROM STK04_DetGRID"
            ReadFromDBToDataReader(strsql, drSql)
            drSql.Read()
            MaxCnt = CInt(drSql.Item(0))
            drSql.Close()
            If MaxCnt > 30 Then MaxCnt = 30


            '''''''''Abir T. @ 06.01.2014
            If Recid <> "" Then
                'One PI to Print -->> get its currency
                strsql = " SELECT INV_Curr FROM STK04_INVHead WHERE INV_RECID = " & Recid
                ReadFromDBToDataReader(strsql, drSql)
                drSql.Read()
                Currency = drSql.Item(0).ToString
                drSql.Close()
            Else
                'Multiple PIs to print

            End If
            '''''''''

            '''''''''Abir T. @ 06.01.2014

            'If this function was called from Purchase Invoice form , then the table was created
            'else , the table should be created here
            TempTable = "Tmp_RepPInv_" & frm.Name & IIf(Fashion, "Fashion", "").ToString & "_" & Rev & gbUser & "_" & gbComp

            If CurrItmKey.Contains("STK_GENDER", StringComparer.OrdinalIgnoreCase) Then KeyGender = True
            If CurrItmKey.Contains("STK_STYLE", StringComparer.OrdinalIgnoreCase) Then KeyStyle = True

            CreatePITable(TempTable, Fashion, KeyGender, KeyStyle, MaxCnt)
            FillPITable(TempTable, Recid, Fashion, frm.Name, KeyGender, KeyStyle, MaxCnt, TempPISel) 'edited by GS on 07/09/16 (add TempPISel)

            'Create SubReports
            TempPISubRep1 = "TempPISubRep1_" & frm.Name & IIf(Fashion, "Fashion", "").ToString & "_" & gbUser
            CreatePITempSubRep(TempPISubRep1, 1, Recid, Currency, TempPISel, PrintFundDisbursement) 'edited by GS on 07/09/16 (add TempPISel)
            If Fashion Then
                TempPISubRep2 = ""
            Else
                TempPISubRep2 = "TempPISubRep2_" & frm.Name & IIf(Fashion, "Fashion", "").ToString & "_" & gbUser
                CreatePITempSubRep(TempPISubRep2, 2, Recid, Currency, TempPISel, PrintFundDisbursement) 'edited by GS on 07/09/16 (add TempPISel)
            End If
            ''''''''''
            'Abir T. @ 21.05.2014 : Add Shipment Info to the report if 'Show Shipment Info' is checked
            strShip = GetShipInfoFields("TempPIShipInfo_" & gbUser, servername) 'modified by GS on 07/09/16 (add serverlink)
            If Fashion Then strFashion = GetFashionFields("PI", MaxCnt)

            'Inv. Header Table 
            CreatePIHeaderTable(frm.Name, Fashion, Mode, Recid, TempPISel, ReportName) 'edited by GS on 07/09/16 (add TempPISel)
            'Inv. Det. Table
            CreatePIDetailsTable(frm.Name, Fashion, Mode, Recid, TempPISel, ReportName, BasePriceType, MergeSameItem) 'edited by GS on 07/09/16 (add TempPISel)
            'Get forwarder names concatenated
            CreateFwdTable(frm.Name, Fashion)

            'REPORT SQL
            DropSqlObject("TempPreview_" & frm.Name & IIf(Fashion, "Fashion", "").ToString & "_" & gbUser, "TABLE")

            'Rachel A. 01.03.2016  adding company logo to report
            strsql = "SELECT  * " ',( SELECT L_Img FROM dbo.COm_Logo WHERE L_Comp='" & gbComp & "' ) [Logo] " 'removed by GS on 15/09/16 (img should be imported from report)
            strsql &= " INTO TempPreview_" & frm.Name & IIf(Fashion, "Fashion", "").ToString & "_" & gbUser
            strsql &= " FROM ( SELECT DISTINCT Head.INV_RECID AS [Inv #],"
            strsql &= " Head.INV_Prefix , Head.INV_Counter , "
            strsql &= " Head.INV_INVREF AS [Supp. Inv#], convert(nvarchar(47), hEAD.Sup_Code + ' - ' + Head.sup_name) AS Supplier,"
            strsql &= " Head.Sacc_accno1 + Head.Sacc_accno2 + Head.Sacc_accsub AS [Supp Acc],"
            strsql &= " CASE WHEN Head.Sup_LocFor = 'F' THEN 'True' ELSE 'False' END AS [Foreign],"
            strsql &= " Head.INV_INVDATE AS Date, CASE WHEN Head.Inv_updated = 'Y' THEN 'True'"
            strsql &= " ELSE 'False' END AS Updated, Head.INV_UPDDATE AS [Upd. Date],"
            strsql &= " CASE WHEN Head.INV_STATUS='R' THEN 'Reverse' ELSE 'Normal' END AS INV_STATUS," 'added by GS on 15/09/16
            strsql &= " CASE WHEN Head.INV_ACCGEN = 'Y' THEN 'True' ELSE 'False' END AS [A/C Gen],"
            strsql &= " Head.inv_transfno AS [Trans #], Head.inv_SENDdate AS [SEND Date],"
            strsql &= " Head.INV_CURR AS Currency,"
            strsql &= " Head.INV_PRICETYPE + ' - ' + Head.prc_desc AS [Price Type],"
            strsql &= " convert(varchar(205),Head.INV_LOCATION + ' - ' + Head.loc_desc) AS Location,"
            If hasCostcenter = True And 1 <> 1 Then
                strsql &= "Convert(varchar(205),'" & costcenterName & "') AS [Project Info],"
            Else
                strsql &= "Convert(varchar(205),'') AS [Project Info],"
            End If
            strsql &= " Head.comp_name AS [Comp Name], Head.INV_Rks AS Remarks,"
            If Fashion Then
                strsql &= " DET.STK_Season + ' - ' + DET.Seas_Desc AS [Season],"
                strsql &= " DET.STK_Brand COLLATE SQL_Latin1_General_CP1256_CI_AS + ' - ' + DET.br_Name COLLATE SQL_Latin1_General_CP1256_CI_AS AS [Brand],"
            End If
            strsql &= " ROUND(Head.INV_CASHDISC, 3) AS [Inv Disc],invd_discval, ROUND(Head.INV_ITEMDISC, 3) AS [Item Disc],"
            strsql &= " CONVERT(DATETIME, CONVERT(VARCHAR, INV_DELIVERY), 101) AS [Delivery Date],"
            strsql &= " Head.INV_ShipDate AS [Ship. Date], username AS [User Name],"
            strsql &= " convert(varchar(3),'" & gbCurr1 & "') AS LC, convert(varchar(3),'" & gbCurr2 & "') AS CC,"
            strsql &= " Head.opr1, Head.Rate1, Head.opr2, Head.Rate2,"

            strsql &= " ROUND(Head.LandAmt, 3) AS LandAmt," '" & IIf(Currency = "LL", 0, 3).ToString & " --> 3 'edited by GS on 11/04/16
            strsql &= " ROUND(Head.INV_VATCC, 3) AS [VAT CC],"
            strsql &= " ROUND(Head.INV_VATLC, 3) AS [VAT LC]," '" & IIf(gbCurr1 = "LL", 0, 3).ToString & " --> 3 'edited by GS on 11/04/16
            strsql &= " ROUND(Head.INV_TOTLENQTY, " & gbDecNo & ") AS [Length Qty],"
            strsql &= " ROUND(Head.INV_TOTWEIQTY, " & gbDecNo & ") AS [Weight Qty],"
            strsql &= " ROUND(HEAD.INV_TOTPICQTY, " & gbDecNo & ") AS [Piece Qty],"
            If Mode = 0 Then
                strsql &= " ROUND(Head.FLQTY, " & gbDecNo & ") AS FLQTY,  ROUND(Head.NLQTY, " & gbDecNo & ") AS NLQTY,ROUND(Head.RLQTY, " & gbDecNo & ") AS RLQTY,"
                strsql &= " ROUND(Head.FWQTY, " & gbDecNo & ") AS FWQTY,  ROUND(Head.NWQTY, " & gbDecNo & ") AS NWQTY, ROUND(Head.RWQTY, " & gbDecNo & ") AS RWQTY,"
                strsql &= " ROUND(Head.FPQTY, " & gbDecNo & ") AS FPQTY,  ROUND(Head.NPQTY, " & gbDecNo & ") AS NPQTY,ROUND(Head.RPQTY, " & gbDecNo & ") AS RPQTY, "
            End If

            strsql &= " ROUND(Head.TotInvDisc, 3) As TotInvDisc," '" & IIf(Currency = "LL", 0, 3).ToString & " --> 3 'edited by GS on 11/04/16
            strsql &= " ROUND(Head.INV_TOTVALUE, 3) AS [Tot Val]," '" & IIf(Currency = "LL", 0, 3).ToString & " --> 3 'edited by GS on 11/04/16
            strsql &= " ROUND(Head.INV_ADJAMNTFC, 3) AS [Adj Amnt FC]," '" & IIf(Currency = "LL", 0, 3).ToString & " --> 3 'edited by GS on 11/04/16
            strsql &= " CASE WHEN Head.INV_OVERSRC = 'N' then   'No Overheads'  WHEN Head.INV_OVERSRC = 'B' THEN 'Both To Forwarder Invoice + Direct To Supplier' WHEN Head.INV_OVERSRC = 'F' THEN 'To Forwarder Invoice' ELSE"
            strsql &= " 'Direct To Supplier' END AS [OH Src.]/*, Head.FwdName*/,"

            strsql &= " CASE WHEN Head.INV_OVERSRC = 'F' THEN Tmp_FwdName.FwdName ELSE"
            strsql &= "  Head.FwdName END AS [FwdName],"

            strsql &= " Head.INV_FrwdInvDate AS [Fwd Inv Date], Head.INV_FrwdACNo AS [Fwd AC No]," 'ROUND(Head.INV_OverheadAmntLC, 0) --> ROUND(Head.INV_OverheadAmntLC, 3) 'edited by GS on 11/04/16
            strsql &= " ROUND(Head.INV_OverheadAmntLC, 3) AS [OH Amt LC],  ROUND(Head.INV_OverheadAmntCC, 3) AS [OH Amt CC],"
            strsql &= " ROUND(Head.INV_OverheadAmntFC, 3) AS [OH Amt FC]," '" & IIf(Currency = "LL", 0, 3).ToString & " --> 3 'edited by GS on 11/04/16
            strsql &= " ROUND(Head.INV_OverheadAmntCV, 3) AS [OH Amt CV]," '" & IIf(Currency = "LL", 0, 3).ToString & " --> 3 'edited by GS on 11/04/16
            strsql &= " ROUND(dbo.convertWithRateOpr('" & gbApp & "','" & gbComp & "','" & gbYear & "',INV_OverheadAmntLC ,'" & gbCurr1 & "',"
            strsql &= " HEAD.INV_CURR, HEAD.INV_INVDATE, CASE WHEN Head.inv_curr = '" & gbCurr1 & "' THEN rate2 else rate1 END,rate2), 3) AS [Tot C/V LC In FC]," '" & IIf(Currency = "LL", 0, 3).ToString & " --> 3 'edited by GS on 11/04/16
            strsql &= " ROUND(dbo.convertWithRateOpr('" & gbApp & "','" & gbComp & "','" & gbYear & "',INV_OverheadAmntCC ,'" & gbCurr2 & "',"
            strsql &= " HEAD.INV_CURR, HEAD.INV_INVDATE, CASE WHEN Head.inv_curr = '" & gbCurr1 & "' THEN rate2 else rate1 END,rate2), 3) AS [Tot C/V CC In FC]," '" & IIf(Currency = "LL", 0, 3).ToString & " --> 3 'edited by GS on 11/04/16

            strsql &= " DET.INVD_Recid AS [DetRecid], DET.INVD_STKRECID AS [StkRecid],"
            strsql &= " DET.INVD_PENDRecId AS [PENDRecid], det.STK_ItmClass AS [Item Class],"
            strsql &= " DET.INVD_SUPREF AS SupRef, CASE WHEN DET.INVD_STATUS = 'N' THEN"
            strsql &= " 'Normal'  WHEN  DET.INVD_STATUS='R' THEN 'Return' ELSE 'Free' END AS Status, det.STK_REF AS [Item Ref],"
            strsql &= " DET.STK_DESC AS Description, DET.ItmBarcode as [Item Barcodes], DET.INVD_ITMIDENT AS [Itm Ident],"
            strsql &= " CASE WHEN DET.INVD_IMPLOC = 'L' THEN 'True' ELSE 'False' END AS Local,"
            strsql &= " CASE WHEN DET.INVD_CONSIG = 'Y' THEN 'True' ELSE 'False' END AS Consignment,"
            strsql &= " CASE WHEN DET.INVD_SERVICE = 'S' THEN 'True' ELSE 'False' END AS SERVICE,"
            strsql &= " CASE WHEN DET.INVD_PROMO = 'Y' THEN 'True' ELSE 'False' END AS PROMO,"

            If Fashion Then strsql &= strFashion
            strsql &= " CASE WHEN DET.INVD_QTYCTRL = 'Y' THEN 'True' ELSE 'False' END AS QTYCTRL,"
            strsql &= " CASE WHEN DET.INVD_TRANSPOS = 'Y' THEN 'True' ELSE 'False' END AS TRANSPOS,"
            strsql &= " CASE WHEN DET.INVD_CONSUM = 'Y' THEN 'True' ELSE 'False' END AS CONSUM,"
            strsql &= " DET.INVD_PURQTY AS PURQTY, Meas_Desc AS PURUNIT,dstech_hscode HSCode,"
            If PrintTransferToLoc Then
                strsql &= " coalesce(PILogT_StorageLoc,DET.INVD_LOCATION) AS [Stor. Loc], "
            Else
                strsql &= " DET.INVD_LOCATION AS [Stor. Loc], "
            End If
            strsql &= " DET.STK_SUPPLIER AS Supp,"
            strsql &= " DET.STK_CATEGORY AS Category, DET.STK_SERIE AS Serie, DET.STK_USAGE AS Usage,"
            strsql &= " DET.STK_NEWUSED AS NewUsed, DET.invd_remarks AS [Det Rem],"
            strsql &= " DET.ID_DESC AS [Itm Ident Desc],    IT_DESC [Product Grp Desc] ,"

            strsql &= " ROUND(DET.INVD_UCOST, 3) AS [U.Cost FC]," '" & IIf(Currency = "LL", 0, 3).ToString & " --> 3 'edited by GS on 11/04/16
            'If gbCurr1 = "LL" Then
            strsql &= " CAST(dbo.convertWithRateOpr('" & gbApp & "','" & gbComp & "','" & gbYear & "',DET.INVD_UCOST, HEAD.INV_CURR, '" & gbCurr1 & "', HEAD.INV_INVDATE, CASE WHEN Head.inv_curr = '" & gbCurr1 & "' THEN rate2 else rate1 END,rate2) AS DECIMAL(18, 0)) AS [U.Cost LC],"
            'Else
            'strsql &= " ROUND(dbo.convert5('" & gbApp & "', '" & gbComp & "', '" & gbYear & "', DET.INVD_UCOST , HEAD.INV_CURR, '" & gbCurr1 & "', HEAD.INV_INVDATE), 3) AS [U.Cost LC],"
            'End If
            strsql &= " ROUND(dbo.convertWithRateOpr('" & gbApp & "','" & gbComp & "','" & gbYear & "',DET.INVD_UCOST , HEAD.INV_CURR, '" & gbCurr2 & "',  HEAD.INV_INVDATE, CASE WHEN Head.inv_curr = '" & gbCurr1 & "' THEN rate2 else rate1 END,rate2), 3) AS [U.Cost CC],"

            strsql &= " ROUND(DET.INVD_ITMDISC, 3) AS [Det Item Disc], ROUND(DET.INVD_CSHDISC, 3) AS [Cash Disc],"
            strsql &= " ROUND(DET.INVD_UNCOST, 3) AS [UN Cost]," '" & IIf(Currency = "LL", 0, 3).ToString & " --> 3 'edited by GS on 11/04/16
            strsql &= " ROUND(DET.INVD_TNCOST, 3) AS [TN Cost]," '" & IIf(Currency = "LL", 0, 3).ToString & " --> 3 'edited by GS on 11/04/16
            'If gbCurr1 = "LL" Then
            strsql &= " CAST(dbo.convertWithRateOpr('" & gbApp & "','" & gbComp & "','" & gbYear & "',DET.INVD_TNCOST, HEAD.INV_CURR, '" & gbCurr1 & "', HEAD.INV_INVDATE, CASE WHEN Head.inv_curr = '" & gbCurr1 & "' THEN rate2 else rate1 END,rate2) AS DECIMAL(18, 0)) AS [TN Cost LC],"
            'Else
            'strsql &= " ROUND(dbo.convert5('" & gbApp & "', '" & gbComp & "', '" & gbYear & "', DET.INVD_TNCOST , HEAD.INV_CURR, '" & gbCurr1 & "', HEAD.INV_INVDATE), 3) AS [TN Cost LC],"
            'End If
            strsql &= " dbo.convertWithRateOpr('" & gbApp & "','" & gbComp & "','" & gbYear & "',DET.INVD_TNCOST , HEAD.INV_CURR, '" & gbCurr2 & "', HEAD.INV_INVDATE, CASE WHEN Head.inv_curr = '" & gbCurr1 & "' THEN rate2 else rate1 END,rate2) AS [TN Cost CC],"
            strsql &= "   DET.STK_VATTYPE AS VATTYPE, isnull(DET.STK_VATPER,0) AS VATPER,  Head.INV_OVERPER AS [OH %],"
            strsql &= " ROUND(DET.INVD_ULANDCOSTLC, 3) AS [Land Cost LC]," '" & IIf(gbCurr1 = "LL", 0, 3).ToString & " --> 3 'edited by GS on 11/04/16
            strsql &= " ROUND(DET.INVD_ULANDCOSTCC, 3) AS [Land Cost CC],"
            'If Currency = "LL" Then
            strsql &= " ROUND(DET.INVD_ULANDCOSTFC  , 3 ) AS [Land Cost FC],"
            'ch @ 15/02/2019
            If ShowLastCost Then
                strsql &= "      ( SELECT TOP 1 INVD_ULANDCOSTLC  FROM      stk04_invhead lasthead  INNER JOIN stk04_invdet lastdet ON inv_recid = invd_hrecid"
                strsql &= "               WHERE     lastdet.INVD_STKRECID = DET.INVD_STKRECID " & IIf(onlyupdate, "AND lasthead.inv_updated = 'Y' ", "").ToString & " AND INVD_STATUS='N'  AND Head.INV_INVDATE >= lasthead.INV_INVDATE	ORDER BY lasthead.INV_INVDATE  DESC) lastcostlc ,"
                'ch @ 15/02/2019
                strsql &= "      ( SELECT TOP 1 INVD_ULANDCOSTcC  FROM      stk04_invhead lasthead  INNER JOIN stk04_invdet lastdet ON inv_recid = invd_hrecid"
                strsql &= "               WHERE     lastdet.INVD_STKRECID = DET.INVD_STKRECID " & IIf(onlyupdate, "AND lasthead.inv_updated = 'Y' ", "").ToString & " AND INVD_STATUS='N' AND Head.INV_INVDATE >= lasthead.INV_INVDATE 	ORDER BY lasthead.INV_INVDATE  DESC ) lastcostCc ,"
                'ch @ 15/02/2019
                strsql &= "      ( SELECT TOP 1 INVD_ULANDCOSTFC  FROM      stk04_invhead lasthead  INNER JOIN stk04_invdet lastdet ON inv_recid = invd_hrecid"
                strsql &= "               WHERE     lastdet.INVD_STKRECID = DET.INVD_STKRECID " & IIf(onlyupdate, "AND lasthead.inv_updated = 'Y' ", "").ToString & " AND INVD_STATUS='N' AND Head.INV_INVDATE >= lasthead.INV_INVDATE 	ORDER BY lasthead.INV_INVDATE  DESC ) lastcostFc ,"

            Else
                strsql &= "     0 lastcostlc ,"
                'ch @ 15/02/2019
                strsql &= "    0 lastcostCc ,"
                'ch @ 15/02/2019
                strsql &= "     0 lastcostFc ,"

            End If

            'strsql &= " ROUND(DET.INVD_ULANDCOSTFC, 3) AS [Land Cost FC]," '" & IIf(Currency = "LL", 0, 3).ToString & " --> 3 'edited by GS on 11/04/16
            'End If
            strsql &= " ROUND(DET.INVD_RETLC, 3) AS [Ret LC]," '" & IIf(gbCurr1 = "LL", 0, 3).ToString & " --> 3 'edited by GS on 11/04/16
            strsql &= " ROUND(DET.INVD_RETCC, 3) AS [Ret CC]  "

            'Abir T. @ 20.11.2013
            '... first Price for the default price type
            If Mode = 0 Then
                strsql &= " ,ROUND(DET.SP_LCTAXEX, 3) AS SP_LCTAXEX," '" & IIf(gbCurr1 = "LL", 0, 3).ToString & " --> 3 'edited by GS on 11/04/16
                strsql &= " ROUND(DET.SP_LCTTC, 3) AS SP_LCTTC," '" & IIf(gbCurr1 = "LL", 0, 3).ToString & " --> 3 'edited by GS on 11/04/16
                strsql &= " ROUND(DET.SP_FCTAXEX, 3) AS SP_FCTAXEX," '" & IIf(Currency = "LL", 0, 3).ToString & " --> 3 'edited by GS on 11/04/16
                strsql &= " ROUND(DET.SP_FCTTC, 3) AS SP_FCTTC," '" & IIf(Currency = "LL", 0, 3).ToString & " --> 3 'edited by GS on 11/04/16
            End If

            strsql = strsql.Substring(0, strsql.Length - 1)
            'If ShipInfo Then
            strsql &= strShip


            strsql &= " , COALESCE(DSTech_Meas,SFP_TechSheet,'') [techContent] " 'ch @ 17/01/2017 wf: 4867
            'ch @ 26/04/2017 
            strsql &= " , INVD_COEF,INVD_RETLC * case when isnull(INVD_COEF,0) = 0  then  0 else 1 end INVD_RETLC   ,"
            strsql &= " ROUND(INVD_RETCC * CASE WHEN ISNULL(INVD_COEF, 0) = 0 THEN 0 ELSE 1 END,3)    INVD_RETfCEx  "
            strsql &= " ,ROUND(INVD_RETCC * ( 1 + ( ISNULL(DET.STK_VATPER, 0) / 100 ) ),3)  INVD_RETfCTTC,"
            strsql &= " ROUND(INVD_RETlC * ( 1 + ( ISNULL(DET.STK_VATPER, 0) / 100 ) ),3)  INVD_RETlCTTC,"

            strsql &= " ISNULL(INVBBarcode,"
            strsql &= "        ( ( SELECT TOP 1"
            strsql &= "                     dbar_barcode"
            strsql &= "            FROM     dbo.Stk04_DetBarcode"
            strsql &= "            WHERE    dbar_stkid = INVD_STKRECID"
            strsql &= "            ORDER BY dbar_DSTMP DESC )"
            strsql &= "        )) INVBBarcode ,"

            strsql &= " Stk04_Brand.br_Name,INV_DSTAMP"

            If gbFundDisbursementModuleIsActive And PrintFundDisbursement Then
                strsql &= " , INVD_RECID  "
            End If

            'ch @ 02/08/2019
            If Mode <> 0 AndAlso ReportName.ToUpper.Contains("PETMART") Then
                strsql &= " , PIBasepriceType1 ,PIBasepriceType2   , HEadPIBasepriceType1    , HEadPIBasepriceType2     "
                strsql &= " ,invd_salesLC2  ,invd_salesCC2, CONVERT(varchar(3000),SupType) SupType "
                If PrintAvailableQty AndAlso tmpAvailableQtyLocFiltr <> "" Then
                    strsql &= " ,isnull(STQ_AVAILABLE,0) STQ_AVAILABLE"
                Else
                    strsql &= " ,0 STQ_AVAILABLE"
                End If
                strsql &= " ,STK_MINQTY,STK_MAXQTY "

                strsql &= "  ,  CASE WHEN STK_OPERATOR = '*' THEN ( DET.INVD_PURQTY * STK_UNITPACK )
                                     ELSE ( DET.INVD_PURQTY / STK_UNITPACK )
                                END [PackInfoQty] "

            End If

            strsql &= "  FROM TempPID_" & frm.Name & IIf(Fashion, "Fashion", "").ToString & "_" & gbUser & " DET"
            strsql &= " LEFT JOIN " & TempTable & " T ON t.stat = DET.INVD_STATUS"
            strsql &= " AND DET.INVD_SUPREF = T.supitmref"
            If Fashion Then
                strsql &= " AND DET.STK_Grid = T.GridCode " 'AND DET.INVD_Color = T.Color"
                For K = 0 To CurrItmKey.Length - 1
                    strsql &= " AND DET." & CurrItmKey(K) & " = T." & CurrItmKey(K)
                Next
            Else
                strsql &= " AND DET.INVD_COUNTER = T.Counter"
            End If
            If frm.Name <> "FrmPInv" And frm.Name <> "FrmPayInfoPI" Then strsql &= " AND DET.INVD_HRECID = T.INVD_HRECID " 'added by GS on 08/09/16
            strsql &= "  LEFT JOIN stk04_stkfil ON INVD_STKRECID=STK_RECID" 'ch @ 17/01/2017 wf: 4867
            strsql &= "  LEFT JOIN dbo.STK04_DSTechnical ON STK04_DSTechnical.DSTech_RecId = STK04_STKFIL.STK_RECID" 'ch @ 17/01/2017 wf: 4867
            strsql &= "  LEFT JOIN dbo.STK04_StkFilPending ON DET.INVD_PENDRecId =SFP_Recid" 'ch @ 17/01/2017 wf: 4867
            strsql &= " INNER JOIN TempPIH_" & frm.Name & IIf(Fashion, "Fashion", "").ToString & "_" & gbUser & " Head On Head.INV_RECID =DET.INVD_HRECID"
            strsql &= " LEFT JOIN STK04_InvPayTerm ON STK04_InvPayTerm.IPAY_IRECID = Head.INV_RECID"
            strsql &= " LEFT JOIN Tmp_FwdName_" & frm.Name & IIf(Fashion, "Fashion", "").ToString & "_" & gbUser & " AS Tmp_FwdName ON FPI_InvHeadId = DET.INVD_HRECID"
            strsql &= "   LEFT JOIN STK04_Measurements On Meas_unit=DET.INVD_PURUNIT "
            strsql &= " LEFT JOIN TempPIShipInfo_" & gbUser & " ShipInfo ON Head.INV_RECID = POSI_PIRecID"
            If Fashion Then strsql &= " LEFT JOIN " & TempGridSize & " GS ON GS.G_Code = DET.INVQ_GridCode"
            strsql &= "  LEFT JOIN ( SELECT  tb1.* ,"
            strsql &= "                      INVB_Barcode INVBBarcode"
            strsql &= "              FROM    ( SELECT    MAX(INVB_RecId) INVBRecId ,"
            strsql &= "                                  INVB_DrecId INVBDrecId"
            strsql &= "                        FROM      dbo.STK04_InvDBE"
            strsql &= "                        GROUP BY  INVB_DrecId"
            strsql &= "                      ) tb1"
            strsql &= "                      INNER JOIN STK04_InvDBE tb2 ON tb1.INVBRecId = tb2.INVB_RecId"
            strsql &= "            ) tb ON INVD_RECID = INVBDrecId"
            strsql &= " LEFT JOIN Stk04_Brand ON Stk04_Brand.br_Code = CASE"
            strsql &= "                                           WHEN STK_RECID IS NULL"
            strsql &= "                                           THEN STK04_StkFilPending.SFP_Brand"
            strsql &= "                                           ELSE STK04_STKFIL.STK_Brand"
            strsql &= "                                           END"
            If PrintAvailableQty AndAlso tmpAvailableQtyLocFiltr <> "" Then
                strsql &= " LEFT JOIN (SELECT stq_stkrecid,SUM(STQ_AVAILABLE) STQ_AVAILABLE FROM stk04_stkqty INNER JOIN " & tmpAvailableQtyLocFiltr & " ON [Loc Code] = STQ_LOCCODE AND chk = 'True' "
                strsql &= " 					GROUP BY stq_stkrecid) stk04_stkqty ON stq_stkrecid = INVD_STKRECID"
            End If

            'Rita M @ 14.02.2020
            If PrintTransferToLoc Then
                strsql &= " LEFT JOIN Stk04_PILogAutoTransfer ON INVD_RECID = PILogT_PIDRecId"
            End If

            If Recid <> "" Then 'added by GS on 07/09/16
                strsql &= " WHERE Head.INV_Recid = " & Recid
                'added by GS on 07/09/16
            Else
                strsql &= " INNER JOIN " & TempPISel & " TempPISel on TempPISel.INV_RECID = Head.INV_Recid and Chk = 'True' "
            End If

            strsql &= " ) AS Temp"
            strsql &= " ORDER BY [Inv #] ASC,"
            If OrderSupRef = True Then strsql &= " SupRef ASC, Status DESC,"
            strsql &= " DetRecid ASC "
            Execute(strsql)
            DIRECTORDER = " ORDER BY [Inv #] ASC,"
            If OrderSupRef = True Then DIRECTORDER &= " SupRef ASC, Status DESC,"
            DIRECTORDER &= " DetRecid ASC "
            DropSqlObject("TempPID_" & frm.Name & IIf(Fashion, "Fashion", "").ToString & "_" & gbUser, "TABLE")
            DropSqlObject("TempPIH_" & frm.Name & IIf(Fashion, "Fashion", "").ToString & "_" & gbUser, "TABLE")
            DropSqlObject("Tmp_FwdName_" & frm.Name & IIf(Fashion, "Fashion", "").ToString & "_" & gbUser, "TABLE")

            strsql = "SELECT * FROM TempPreview_" & frm.Name & IIf(Fashion, "Fashion", "").ToString & "_" & gbUser
            ReadFromDBToDataReader(strsql, drSql)
            If Not drSql.HasRows Then
                MessageBox.Show("No Records Found.", frm.Text, MessageBoxButtons.OK, MessageBoxIcon.Information)
                If frm.Name <> "FrmPInv" And frm.Name <> "FrmPayInfoPI" Then DropSqlObject(TempTable, "TABLE")
                DropSqlObject("TempPreview_" & frm.Name & IIf(Fashion, "Fashion", "").ToString & "_" & gbUser, "TABLE")
                Exit Sub
            End If
            drSql.Read()
            ' Removed by Ch @ 03/01/2019 mail(FW: Wrong coefficient -system problem)
            'Dim Curr, DelDate As String
            'Curr = IIf(drSql.Item("Currency").ToString = gbCurr1, gbCurr2, drSql.Item("Currency").ToString).ToString
            'DelDate = CDate(drSql.Item("Delivery Date")).ToString("dd/MM/yyyy")

            'If Val(Recid) <> 0 Then
            '    If GetRates(CDate(drSql.Item("Delivery Date")), IIf(drSql.Item("Currency").ToString = gbCurr1, gbCurr2, drSql.Item("Currency").ToString).ToString) = False Then
            '        MessageBox.Show("Rates Not Defined For The Currency " & Curr & " And The Date '" + DelDate + "'.", frm.Text, MessageBoxButtons.OK, MessageBoxIcon.Information)
            '        If frm.Name <> "FrmPInv" And frm.Name <> "FrmPayInfoPI" Then DropSqlObject(TempTable, "TABLE")
            '        DropSqlObject("TempPreview_" & frm.Name & IIf(Fashion, "Fashion", "").ToString & "_" & gbUser, "TABLE")
            '        Exit Try
            '    End If
            'End If

            If Mode = 0 Then
                ReportName = "rptPurInvList_Details"
                ReportName &= ".rpt"
                If PrintFundDisbursement Then
                    ReportName = "rptPurInvList_Details_WithFunds.rpt"
                End If
            Else
                If ReportName = "" Then
                    ReportName = "rptPurInvList_Simple"
                    ReportName &= ".rpt"
                    If PrintFundDisbursement Then
                        ReportName = "rptPurInvList_Simple_WithFunds.rpt"
                    End If
                End If
            End If
            If Fashion = True Then ReportName = ReportName.Replace(".rpt", "") & "_Fashion" & ".rpt"


            'Formulas
            Dim rptParam(18, 1) As Object
            rptParam(0, 0) = "gbDecNo" : rptParam(0, 1) = IIf(Fashion = True, 0, gbDecNo)
            rptParam(1, 0) = "PricePrint" : rptParam(1, 1) = IIf(VAT = 0, "'Ex'", "'TTC'")
            rptParam(2, 0) = "Reprint" : rptParam(2, 1) = Reprint
            rptParam(3, 0) = "LandedCost" : rptParam(3, 1) = LandedCost
            rptParam(4, 0) = "PrintExpiry"
            rptParam(4, 1) = IIf(Fashion = True, False, PrintExpiry)

            'Abir T. @ 31.12.2013
            'Search for the default price type description
            Dim PriceType As String
            Dim drPriceType As SqlDataReader
            Str = "SELECT Price_Code FROM Stk04_SalPriceType WHERE Price_Default = 'Y'"
            ReadFromDBToDataReader(Str, drPriceType)
            If drPriceType.HasRows Then
                drPriceType.Read()
                PriceType = drPriceType.Item(0).ToString & " Price "
            Else
                PriceType = "Default Price "
            End If
            rptParam(5, 0) = "PriceType" : rptParam(5, 1) = "'" & IIf(frm.Name = FrmRepPI.Name, BasePriceType, PriceType).ToString & "'"
            rptParam(6, 0) = "Mode" : rptParam(6, 1) = "'" & bMode & "'"
            rptParam(7, 0) = "ShipInfo" : rptParam(7, 1) = ShipInfo
            rptParam(8, 0) = "TechContent" : rptParam(8, 1) = PrintTechContent
            rptParam(9, 0) = "ShowLineNUmber" : rptParam(9, 1) = IIf(ShowLineNumber = True, "'Y'", "'N'")

            If Not Fashion Then

                rptParam(10, 0) = "PurchaseSerNo" : rptParam(10, 1) = PurchaseSerNo
                rptParam(11, 0) = "VATLCROUND" : rptParam(11, 1) = VatLCRound
                rptParam(12, 0) = "ShowLAstCost" : rptParam(12, 1) = ShowLastCost
                rptParam(13, 0) = "OnlyUpdated" : rptParam(13, 1) = onlyupdate

                If Mode <> 0 AndAlso ReportName.ToUpper.Contains("PETMART") Then
                    rptParam(14, 0) = "GbCurr1" : rptParam(14, 1) = "'" & gbCurr1 & "'"
                End If

                If PrintTransferToLoc Then
                    rptParam(15, 0) = "StorageLocCaption" : rptParam(15, 1) = "'R.Loc'"
                Else
                    rptParam(15, 0) = "StorageLocCaption" : rptParam(15, 1) = "'Loc'"
                End If

                If ReportName.ToUpper.Contains("PETMART") Then
                    If PrintAvailableQty Then
                        rptParam(16, 0) = "printAvailQty" : rptParam(16, 1) = "'Y'"
                        Dim nowStamp As String = CDate(ExecuteScalar_WOXACT("SELECT GETDATE()")).ToString("dd/MM/yyyy")
                        rptParam(17, 0) = "NowStamp" : rptParam(17, 1) = "'" & nowStamp & "'"
                        Dim selectedLoc As String = ExecuteScalar_WOXACT("SELECT STUFF((SELECT  ','+ [Location Description]   AS 'data()'  FROM  " & tmpAvailableQtyLocFiltr & " WHERE chk = 'True' FOR XML PATH('') ),1,1,'')   ")
                        rptParam(18, 0) = "selectedLocFiltr" : rptParam(18, 1) = "'" & selectedLoc & "'"
                    Else
                        rptParam(16, 0) = "printAvailQty" : rptParam(16, 1) = "'N'"
                    End If
                End If

            End If

            If PrintDirect = True Then
                PrintPIDirectRep(frm, strsql, ReportName, TempPISubRep1, TempPISubRep2, Fashion, rptParam, NbPrint, DIRECTORDER, FundsQuery("TempPreview_" & frm.Name & IIf(Fashion, "Fashion", "").ToString & "_" & gbUser, PrintFundDisbursement, "PI"))
            Else
                PrintPIWithPrevGrid(frm, strsql, ReportName, TempPISubRep1, TempPISubRep2, Mode, ShipInfo, Currency, PriceType, MaxCnt, Fashion, OrderSupRef, rptParam, PrintTechContent, hasCostcenter, , PrintAvailableQty, tmpAvailableQtyLocFiltr,
                                    FundsQuery("TempPreview_" & frm.Name & IIf(Fashion, "Fashion", "").ToString & "_" & gbUser, PrintFundDisbursement, "PI"), PrintFundDisbursement)
            End If

        Catch exp As Exception
            Throw exp
        Finally
            'DropSqlObject("TempPreview_" & frm.Name & IIf(Fashion, "Fashion", "").ToString & "_" & gbUser, "TABLE")
            'If frm.Name <> "FrmPInv" And frm.Name <> "FrmPayInfoPI" Then DropSqlObject(TempTable, "TABLE")
            'DropSqlObject(TempPISubRep1, "TABLE")
            'DropSqlObject(TempPISubRep2, "TABLE")
            'DropSqlObject("TempPIShipInfo_" & gbUser, "TABLE")
            'Cursor.Current = Cursors.Default
            'If Not frm Is Nothing Then frm = Nothing
            If Not drSql Is Nothing Then
                If drSql.IsClosed = False Then drSql.Close()
                drSql = Nothing
            End If
            DropSqlObject("TempPreview_" & frm.Name & IIf(Fashion, "Fashion", "").ToString & "_" & gbUser, "TABLE")
            If frm.Name <> "FrmPInv" And frm.Name <> "FrmPayInfoPI" Then DropSqlObject(TempTable, "TABLE")
            If Not frm Is Nothing Then frm = Nothing
            DropSqlObject(TempPISubRep1, "TABLE")
            DropSqlObject(TempPISubRep2, "TABLE")
            DropSqlObject("TempPIShipInfo_" & gbUser, "TABLE")
            Cursor.Current = Cursors.Default
        End Try
    End Sub
    'Print Report With Preview Grid
    Public Sub PrintPIWithPrevGrid(ByVal frmPI As Form, ByVal SQLstat As String, ByVal ReportName As String,
                                      ByVal TempPISubRep1 As String, ByVal TempPISubRep2 As String, ByVal Mode As Integer,
                                      ByVal ShipInfo As Boolean, ByVal CurrFC As String, ByVal PriceType As String,
                                      ByVal MaxCnt As Integer, ByVal Fashion As Boolean, ByVal OrderSupRef As Boolean,
                                      Optional ByVal rptParam(,) As Object = Nothing, Optional ByVal techContent As Boolean = False, Optional HasCostCenter As Boolean = False,
                                      Optional PrintTransferToLoc As Boolean = False, Optional PrintAvailableQty As Boolean = False, Optional tmpAvailableQtyLocFiltr As String = "",
                                   Optional FundQuery As String = "", Optional PrintFundDisbursement As Boolean = False)
        Dim frm As frmPreviewGrid
        Dim strsql As String
        Dim Str As String
        Dim drSql As SqlDataReader
        Try
            Cursor.Current = Cursors.WaitCursor
            frm = New frmPreviewGrid
            With frm
                .withLogo = True 'added by GS on 15/09/16

                frm.BuildSql(SQLstat, 27)
                Dim QtyFormat As String
                Select Case gbDecNo
                    Case 0 : QtyFormat = "#,##0"
                    Case 1 : QtyFormat = "#,##0.0"
                    Case 2 : QtyFormat = "#,##0.00"
                    Case 3 : QtyFormat = "#,##0.000"
                End Select
                If PurchaseSerNo Then
                    .ColDesign("Inv #", 0, "Inv #", AlignHorzEnum.Center, AlignVertEnum.Top, False, , , , True, False, , , , , , , , , , , False)
                    .ColDesign("INV_Prefix", 60, "Prefix", AlignHorzEnum.Center, AlignVertEnum.Top, , , , , True, , frmPreviewGrid.Orders.Ascending, , , , , , , , , , , )
                    .ColDesign("INV_Counter", 60, "Inv #", AlignHorzEnum.Center, AlignVertEnum.Top, , , , , True, , frmPreviewGrid.Orders.Ascending, , , , , , , , , , , )
                Else
                    .ColDesign("Inv #", 60, "Inv #", AlignHorzEnum.Center, AlignVertEnum.Top, , , , , True, , frmPreviewGrid.Orders.Ascending, , , , , , , , , , , )
                    .ColDesign("INV_Prefix", 0, "Prefix", AlignHorzEnum.Center, AlignVertEnum.Top, False, , , , True, False, , , , , , , , , , , False)
                    .ColDesign("INV_Counter", 0, "Inv #", AlignHorzEnum.Center, AlignVertEnum.Top, False, , , , True, False, , , , , , , , , , , False)
                End If

                .ColDesign("INV_STATUS", 60, "PI Status", AlignHorzEnum.Center, AlignVertEnum.Top, , , , , True, , frmPreviewGrid.Orders.Ascending) 'added by GS on 15/09/16
                If OrderSupRef Then
                    .ColDesign("DetRecid", 0, "DetRecid", AlignHorzEnum.Center, AlignVertEnum.Top, , , , , True, False, )
                Else
                    .ColDesign("DetRecid", 0, "DetRecid", AlignHorzEnum.Center, AlignVertEnum.Top, , , , , True, False, frmPreviewGrid.Orders.Ascending)
                End If
                .ColDesign("Supp. Inv#", 80, "Supp. Inv#", AlignHorzEnum.Near, AlignVertEnum.Top, , , , , True)
                .ColDesign("Date", 80, "Date", AlignHorzEnum.Center, AlignVertEnum.Top, , , , , True, , , , , , "dd/MM/yyyy")
                'ch @ 25/05/2017 email ( Purchase Module - Reprint PO/ PI)
                ' .ColDesign("Delivery Date", 80, "Delivery Date", AlignHorzEnum.Center, AlignVertEnum.Top, , , , , True, , , , , , "dd/MM/yyyy")
                .ColDesign("Delivery Date", 0, "Delivery Date", AlignHorzEnum.Center, AlignVertEnum.Top, False, , , , , False, , , , )
                .ColDesign("Updated", 60, "Updated", AlignHorzEnum.Center, AlignVertEnum.Top, , , , , True, , , , , , , , , , C1.Win.C1TrueDBGrid.PresentationEnum.CheckBox)
                .ColDesign("Upd. Date", 80, "Upd. Date", AlignHorzEnum.Center, AlignVertEnum.Top, , , , , True, , , , , , "dd/MM/yyyy")
                .ColDesign("A/C Gen", 60, "A/C Gen", AlignHorzEnum.Center, AlignVertEnum.Top, , , , , True, , , , , , , , , , C1.Win.C1TrueDBGrid.PresentationEnum.CheckBox)
                '.ColDesign("OH %", 70, "OH %", AlignHorzEnum.Near, AlignVertEnum.Top)
                .ColDesign("OH %", 70, "OH %", AlignHorzEnum.Near, AlignVertEnum.Top, False, , , , , False, , , , , , , , , , , False)
                '  .ColDesign("OH Src.", 120, "OH Src.", AlignHorzEnum.Near, AlignVertEnum.Top, , , , , True)
                .ColDesign("OH Src.", 0, "OH Src.", AlignHorzEnum.Near, AlignVertEnum.Top, False, , , , , False, , , , , , , , , , , False)
                .ColDesign("Inv Disc", 60, "Inv Disc %", AlignHorzEnum.Far, AlignVertEnum.Top, , , , , True, , , , , , "#,##0.000")
                .ColDesign("invd_discval", 90, "Inv Disc Val", AlignHorzEnum.Far, AlignVertEnum.Top, , , , , True, , , , , , "#,##0.000")
                .ColDesign("Item Disc", 60, "Item Disc", AlignHorzEnum.Far, AlignVertEnum.Top, , , , , True, , , , , , "#,##0.000")
                'ch @ 25/05/2017 email ( Purchase Module - Reprint PO/ PI)
                '.ColDesign("Length Qty", 80, "Total Length Qty", AlignHorzEnum.Far, AlignVertEnum.Top, , , , , True, , , , , , QtyFormat)
                '.ColDesign("Weight Qty", 80, "Total Weight Qty", AlignHorzEnum.Far, AlignVertEnum.Top, , , , , True, , , , , , QtyFormat)
                '.ColDesign("Piece Qty", 80, "Total Piece Qty", AlignHorzEnum.Far, AlignVertEnum.Top, , , , , True, , , , , , QtyFormat)
                .ColDesign("Length Qty", 0, "Total Length Qty", AlignHorzEnum.Far, AlignVertEnum.Top, False, , , , , False, , , , , , , , , , , False)
                .ColDesign("Weight Qty", 0, "Total Weight Qty", AlignHorzEnum.Far, AlignVertEnum.Top, False, , , , , False, , , , , , , , , , , False)
                .ColDesign("Piece Qty", 0, "Total Piece Qty", AlignHorzEnum.Far, AlignVertEnum.Top, False, , , , , False, , , , , , , , , , , False)
                .ColDesign("Currency", 80, "Currency", AlignHorzEnum.Center, AlignVertEnum.Top, , , , , True)

                .ColDesign("Tot Val", 100, "Tot Val", AlignHorzEnum.Far, AlignVertEnum.Top, , , , , True, , , , , , IIf(CurrFC = gbCurr1, "N" & VatLCRound, "#,##0.000").ToString)
                'ch @ 25/05/2017 email ( Purchase Module - Reprint PO/ PI)
                ' .ColDesign("Adj Amnt FC", 100, "Adj Amnt " & CurrFC, AlignHorzEnum.Far, AlignVertEnum.Top, , , , , True, , , , , , IIf(CurrFC = "LL", "#,##0", "#,##0.000").ToString, , , , , , , "Adj Amnt " & CurrFC)
                '.ColDesign("Fwd Inv Date", 80, "Fwd Inv Date", AlignHorzEnum.Center, AlignVertEnum.Top, , , , , True, , , , , , "dd/MM/yyyy") .
                ' .ColDesign("Fwd AC No", 80, "Fwd AC No", AlignHorzEnum.Near, AlignVertEnum.Top, , , , , True)
                '.ColDesign("OH Amt LC", 100, "OH Amt " & gbCurr1, AlignHorzEnum.Far, AlignVertEnum.Top, , , , , True, , , , , , IIf(gbCurr1 = "LL", "#,##0", "#,##0.000").ToString, , , , , , , "OH Amt " & gbCurr1)
                '.ColDesign("OH Amt CC", 100, "OH Amt " & gbCurr2, AlignHorzEnum.Far, AlignVertEnum.Top, , , , , True, , , , , , "#,##0.000", , , , , , , "OH Amt " & gbCurr2)
                '.ColDesign("OH Amt FC", 100, "OH Amt " & CurrFC, AlignHorzEnum.Far, AlignVertEnum.Top, , , , , True, CBool(IIf(CurrFC = gbCurr1 Or CurrFC = gbCurr2, False, True)), , , , , IIf(CurrFC = "LL", "#,##0", "#,##0.000").ToString, , , , , , , "OH Amt " & CurrFC)
                '.ColDesign("OH Amt CV", 100, "Computed OH", AlignHorzEnum.Far, AlignVertEnum.Top, , , , , True, , , , , , IIf(CurrFC = "LL", "#,##0", "#,##0.000").ToString, , , , , , , "Computed OH")
                '.ColDesign("Tot C/V LC In FC", 100, "Tot C/V " & gbCurr1 & " In " & CurrFC, AlignHorzEnum.Far, AlignVertEnum.Top, , , , , True, , , , , , IIf(CurrFC = "LL", "#,##0", "#,##0.000").ToString, , , , , , , "Tot C/V " & gbCurr1 & " In " & CurrFC)
                '.ColDesign("Tot C/V CC In FC", 100, "Tot C/V " & gbCurr2 & " In " & CurrFC, AlignHorzEnum.Far, AlignVertEnum.Top, , , , , True, , , , , , IIf(CurrFC = "LL", "#,##0", "#,##0.000").ToString, , , , , , , "Tot C/V " & gbCurr2 & " In " & CurrFC)
                .ColDesign("Adj Amnt FC", 0, "Adj Amnt " & CurrFC, AlignHorzEnum.Far, AlignVertEnum.Top, False, , , , , False, , , , , , , , , , , False)
                .ColDesign("Fwd Inv Date", 0, "Fwd Inv Date", AlignHorzEnum.Center, AlignVertEnum.Top, False, , , , , False, , , , , , , , , , , False)
                .ColDesign("Fwd AC No", 0, "Fwd AC No", AlignHorzEnum.Near, AlignVertEnum.Top, False, , , , , False, , , , , , , , , , , False)
                .ColDesign("OH Amt LC", 0, "OH Amt " & gbCurr1, AlignHorzEnum.Far, AlignVertEnum.Top, False, , , , , False, , , , , , , , , , , False)
                .ColDesign("OH Amt CC", 0, "OH Amt " & gbCurr2, AlignHorzEnum.Far, AlignVertEnum.Top, False, , , , , False, , , , , , , , , , , False)
                .ColDesign("OH Amt FC", 0, "OH Amt " & CurrFC, AlignHorzEnum.Far, AlignVertEnum.Top, False, , , , , False, , , , , , , , , , , False)
                .ColDesign("OH Amt CV", 0, "Computed OH", AlignHorzEnum.Far, AlignVertEnum.Top, False, , , , , False, , , , , , , , , , , False)
                .ColDesign("Tot C/V LC In FC", 0, "Tot C/V " & gbCurr1 & " In " & CurrFC, AlignHorzEnum.Far, AlignVertEnum.Top, False, , , , , False, , , , , , , , , , , False)
                .ColDesign("Tot C/V CC In FC", 0, "Tot C/V " & gbCurr2 & " In " & CurrFC, AlignHorzEnum.Far, AlignVertEnum.Top, False, , , , , False, , , , , , , , , , , False)


                'ch @ 25/05/2017 email ( Purchase Module - Reprint PO/ PI)
                '.ColDesign("VAT LC", 80, "VAT " & gbCurr1, AlignHorzEnum.Far, AlignVertEnum.Top, , , , , True, , , , , , IIf(gbCurr1 = "LL", "#,##0", "#,##0.000").ToString, , , , , , , "VAT " & gbCurr1)
                '.ColDesign("VAT CC", 80, "VAT " & gbCurr2, AlignHorzEnum.Far, AlignVertEnum.Top, , , , , True, , , , , , "#,##0.000", , , , , , , "VAT " & gbCurr2)
                .ColDesign("VAT LC", 0, "VAT " & gbCurr1, AlignHorzEnum.Far, AlignVertEnum.Top, False, , , , , False, , , , , , , , , , , False)
                .ColDesign("VAT CC", 0, "VAT " & gbCurr2, AlignHorzEnum.Far, AlignVertEnum.Top, False, , , , , False, , , , , , , , , , , False)
                '.ColDesign("Ship. Date", 80, "Ship. Date", AlignHorzEnum.Center, AlignVertEnum.Top, , , , , True, , , , , , "dd/MM/yyyy")
                .ColDesign("Ship. Date", 0, "Ship. Date", AlignHorzEnum.Center, AlignVertEnum.Top, False, , , , , False, , , , , , , , , , , False)
                '.ColDesign("Comp Name", 100, "Comp Name", AlignHorzEnum.Near, AlignVertEnum.Top, , , , , True)
                .ColDesign("Comp Name", 0, "Comp Name", AlignHorzEnum.Near, AlignVertEnum.Top, False, , , , , False, , , , , , , , , , , False)
                .ColDesign("Supplier", 150, "Supplier", AlignHorzEnum.Near, AlignVertEnum.Top, , , , , True)
                .ColDesign("Supp Acc", 80, "Supp Acc", AlignHorzEnum.Near, AlignVertEnum.Top, , , , , True)
                .ColDesign("Foreign", 60, "Foreign", AlignHorzEnum.Center, AlignVertEnum.Top, , , , , True, , , , , , , , , , C1.Win.C1TrueDBGrid.PresentationEnum.CheckBox)
                .ColDesign("Location", 100, "Location", AlignHorzEnum.Near, AlignVertEnum.Top, , , , , True)
                If HasCostCenter = True Then
                    'Project Info
                    .ColDesign("Project Info", 100, "CC/project", AlignHorzEnum.Near, AlignVertEnum.Top, , , , , True)
                Else
                    .ColDesign("Project Info", 0, "CC/project", AlignHorzEnum.Near, AlignVertEnum.Top, , , , , , False)
                End If
                'ch @ 25/05/2017 email ( Purchase Module - Reprint PO/ PI)
                '.ColDesign("Trans #", 60, "Trans #", AlignHorzEnum.Near, AlignVertEnum.Top, , , , , True)
                .ColDesign("Trans #", 0, "Trans #", AlignHorzEnum.Near, AlignVertEnum.Top, False, , , , , False, , , , , , , , , , , False)
                'ch @ 25/05/2017 email ( Purchase Module - Reprint PO/ PI)
                '.ColDesign("SEND Date", 80, "SEND Date", AlignHorzEnum.Near, AlignVertEnum.Top, , , , , True, , , , , , "dd/MM/yyyy")
                .ColDesign("SEND Date", 0, "SEND Date", AlignHorzEnum.Near, AlignVertEnum.Top, False, , , , , False, , , , , , , , , , , False)
                .ColDesign("Price Type", 120, "Price Type", AlignHorzEnum.Near, AlignVertEnum.Top, , , , , True)
                'ch @ 25/05/2017 email ( Purchase Module - Reprint PO/ PI)
                '  .ColDesign("User Name", 60, "User Name", AlignHorzEnum.Near, AlignVertEnum.Top, , , , , True)
                .ColDesign("User Name", 0, "User Name", AlignHorzEnum.Near, AlignVertEnum.Top, False, , , , , False, , , , , , , , , , , False)
                .ColDesign("Remarks", 120, "Remarks", AlignHorzEnum.Near, AlignVertEnum.Top, , , , , True)

                If Fashion Then
                    .ColDesign("Season", 100, "Season", AlignHorzEnum.Near, AlignVertEnum.Top, , , , , True)
                    .ColDesign("Brand", 100, "Brand", AlignHorzEnum.Near, AlignVertEnum.Top, , , , , True)
                    .ColDesign("Supp. Color", 80, "Supp. Color", AlignHorzEnum.Near, AlignVertEnum.Top)
                    .ColDesign("Gender", 100, "Gender", AlignHorzEnum.Near, AlignVertEnum.Top)
                    .ColDesign("Product Group", 100, "Product Group", AlignHorzEnum.Near, AlignVertEnum.Top)
                    .ColDesign("Style", 60, "Style", AlignHorzEnum.Center, AlignVertEnum.Top)
                    .ColDesign("Color", 60, "Color", AlignHorzEnum.Near, AlignVertEnum.Top)
                    .ColDesign("Grid", 60, "Grid", AlignHorzEnum.Center, AlignVertEnum.Top)

                    For i As Integer = 1 To MaxCnt
                        .ColDesign("Size" & i, 60, "Size" & i, AlignHorzEnum.Center, AlignVertEnum.Top, , , , , , False)
                    Next
                    If MaxCnt < 30 Then
                        For i As Integer = (MaxCnt + 1) To 30
                            .ColDesign("Size" & i, 60, "Size" & i, AlignHorzEnum.Center, AlignVertEnum.Top, , , , , , False, , , , , , , , , , , False)
                        Next
                    End If
                    For j As Integer = 1 To MaxCnt
                        .ColDesign("Qty" & j, 60, "Qty" & j, AlignHorzEnum.Center, AlignVertEnum.Top, , , , , , , , , , , QtyFormat)
                    Next
                    If MaxCnt < 30 Then
                        For j As Integer = (MaxCnt + 1) To 30
                            .ColDesign("Qty" & j, 60, "Qty" & j, AlignHorzEnum.Center, AlignVertEnum.Top, , , , , , False, , , , , QtyFormat, , , , , , False)
                        Next
                    End If
                End If

                .ColDesign("LC", 0, "LC", AlignHorzEnum.Near, AlignVertEnum.Top, , , , , , False)
                .ColDesign("CC", 0, "CC", AlignHorzEnum.Near, AlignVertEnum.Top, , , , , , False)

                If OrderSupRef Then
                    .ColDesign("SupRef", 100, "SupRef", AlignHorzEnum.Near, AlignVertEnum.Top, , , , , , , frmPreviewGrid.Orders.Ascending)
                    .ColDesign("Status", 60, "Status", AlignHorzEnum.Center, AlignVertEnum.Top, , , , , , , frmPreviewGrid.Orders.Ascending)
                Else
                    .ColDesign("SupRef", 100, "SupRef", AlignHorzEnum.Near, AlignVertEnum.Top)
                    .ColDesign("Status", 60, "Status", AlignHorzEnum.Center, AlignVertEnum.Top)
                End If

                .ColDesign("Item Ref", 130, "Item Ref", AlignHorzEnum.Near, AlignVertEnum.Top)
                .ColDesign("Item Barcodes", -1, "Item Barcodes", AlignHorzEnum.Near, AlignVertEnum.Top)
                .ColDesign("Description", 130, "Description", AlignHorzEnum.Near, AlignVertEnum.Top)

                .ColDesign("Itm Ident", 0, "Itm Ident", AlignHorzEnum.Center, AlignVertEnum.Top, False, , , , , False, , , , , , , , , , , False)

                .ColDesign("StkRecid", 0, "StkRecid", AlignHorzEnum.Center, AlignVertEnum.Top, , , , , , False)
                .ColDesign("PENDRecid", 0, "PENDRecid", AlignHorzEnum.Center, AlignVertEnum.Top, , , , , , False)
                .ColDesign("Item Class", 70, "Item Class", AlignHorzEnum.Center, AlignVertEnum.Top, , , , , , False)

                .ColDesign("Local", 0, "Local", AlignHorzEnum.Center, AlignVertEnum.Top, False, , , , , False, , , , , , , , , , , False)
                .ColDesign("Consignment", 0, "Consignment", AlignHorzEnum.Center, AlignVertEnum.Top, False, , , , , False, , , , , , , , , , , False)
                .ColDesign("SERVICE", 0, "SERVICE", AlignHorzEnum.Center, AlignVertEnum.Top, False, , , , , False, , , , , , , , , , , False)
                .ColDesign("PROMO", 0, "PROMO", AlignHorzEnum.Center, AlignVertEnum.Top, False, , , , , False, , , , , , , , , , , False)

                .ColDesign("QTYCTRL", 0, "QTYCTRL", AlignHorzEnum.Center, AlignVertEnum.Top, False, , , , , False, , , , , , , , , , , False)
                .ColDesign("TRANSPOS", 0, "TRANSPOS", AlignHorzEnum.Center, AlignVertEnum.Top, False, , , , , False, , , , , , , , , , , False)
                .ColDesign("CONSUM", 0, "CONSUM", AlignHorzEnum.Center, AlignVertEnum.Top, False, , , , , False, , , , , , , , , , , False)
                'ch @ 01/02/2018  mail(Quantity In Purchase to Excel)
                .ColDesign("PURQTY", -1, "PI.QTY", AlignHorzEnum.Far, AlignVertEnum.Top, , , , , , , , , , , , , , , , , )
                .ColDesign("PURUNIT", -1, "PURUNIT", AlignHorzEnum.Center, AlignVertEnum.Top, , , , , , , , , , , , , , , , , False)

                .ColDesign("U.Cost FC", 100, "U.Cost " & CurrFC, AlignHorzEnum.Far, AlignVertEnum.Top, CBool(IIf(CurrFC = gbCurr1 Or CurrFC = gbCurr2, False, True)), , , , , CBool(IIf(CurrFC = gbCurr1 Or CurrFC = gbCurr2, False, True)), , , , , IIf(CurrFC = gbCurr1, "N" & VatLCRound, "#,##0.000").ToString, , , , , , , "U.Cost " & CurrFC)
                ' .ColDesign("U.Cost LC", 100, "U.Cost " & gbCurr1, AlignHorzEnum.Far, AlignVertEnum.Top, , , , , , , , , , , IIf(gbCurr1 = "LL", "#,##0", "#,##0.000").ToString, , , , , , , "U.Cost " & gbCurr1)
                '   .ColDesign("U.Cost CC", 100, "U.Cost " & gbCurr2, AlignHorzEnum.Far, AlignVertEnum.Top, , , , , , , , , , , "#,##0.000", , , , , , , "U.Cost " & gbCurr2)
                .ColDesign("U.Cost LC", 0, "U.Cost " & gbCurr1, AlignHorzEnum.Far, AlignVertEnum.Top, False, , , , , False, , , , , "N" & VatLCRound, , , , , , , "U.Cost " & gbCurr1)
                .ColDesign("U.Cost CC", 0, "U.Cost " & gbCurr2, AlignHorzEnum.Far, AlignVertEnum.Top, False, , , , , False, , , , , "#,##0.000", , , , , , , "U.Cost " & gbCurr2)
                .ColDesign("Det Item Disc", 80, "Det Item Disc", AlignHorzEnum.Far, AlignVertEnum.Top, , , , , , , , , , , , , , , , , False)
                .ColDesign("Cash Disc", 80, "Cash Disc", AlignHorzEnum.Far, AlignVertEnum.Top, , , , , , , , , , , "#,##0.000")
                .ColDesign("UN Cost", 100, "UN Cost", AlignHorzEnum.Far, AlignVertEnum.Top, , , , , , , , , , , IIf(CurrFC = gbCurr1, "N" & VatLCRound, "#,##0.000").ToString)
                .ColDesign("TN Cost", 100, "TN Cost " & CurrFC, AlignHorzEnum.Far, AlignVertEnum.Top, CBool(IIf(CurrFC = gbCurr1 Or CurrFC = gbCurr2, False, True)), , , , , CBool(IIf(CurrFC = gbCurr1 Or CurrFC = gbCurr2, False, True)), , , , , IIf(CurrFC = gbCurr1, "N" & VatLCRound, "#,##0.000").ToString)
                '   .ColDesign("TN Cost LC", 100, "TN Cost " & gbCurr1, AlignHorzEnum.Far, AlignVertEnum.Top, , , , , , , , , , , IIf(gbCurr1 = "LL", "#,##0", "#,##0.000").ToString, , , , , , , "TN Cost " & gbCurr1)
                '  .ColDesign("TN Cost CC", 100, "TN Cost " & gbCurr2, AlignHorzEnum.Far, AlignVertEnum.Top, , , , , , , , , , , "#,##0.000", , , , , , , "TN Cost " & gbCurr2)
                '.ColDesign("Cash Disc", 80, "Cash Disc", AlignHorzEnum.Far, AlignVertEnum.Top, False, , , , , False, , , , , , , , , , , False)
                '.ColDesign("UN Cost", 100, "UN Cost", AlignHorzEnum.Far, AlignVertEnum.Top, False, , , , , False, , , , , , , , , , , False)
                '.ColDesign("TN Cost", 100, "TN Cost " & CurrFC, AlignHorzEnum.Far, AlignVertEnum.Top, False, , , , , False, , , , , , , , , , , False)
                .ColDesign("TN Cost LC", 100, "TN Cost " & gbCurr1, AlignHorzEnum.Far, AlignVertEnum.Top, False, , , , , False, , , , , , , , , , , False)
                .ColDesign("TN Cost CC", 100, "TN Cost " & gbCurr2, AlignHorzEnum.Far, AlignVertEnum.Top, False, , , , , False, , , , , , , , , , , False)

                .ColDesign("Land Cost LC", 100, "Land Cost " & gbCurr1, AlignHorzEnum.Far, AlignVertEnum.Top, , , , , , , , , , , "N" & VatLCRound, , , , , , , "Land Cost " & gbCurr1)
                .ColDesign("Land Cost CC", 100, "Land Cost " & gbCurr2, AlignHorzEnum.Far, AlignVertEnum.Top, , , , , , , , , , , "#,##0.000", , , , , , , "Land Cost " & gbCurr2)
                .ColDesign("Land Cost FC", 100, "Land Cost " & CurrFC, AlignHorzEnum.Far, AlignVertEnum.Top, CBool(IIf(CurrFC = gbCurr1 Or CurrFC = gbCurr2, False, True)), , , , , CBool(IIf(CurrFC = gbCurr1 Or CurrFC = gbCurr2, False, True)), , , , , IIf(CurrFC = gbCurr1, "N" & VatLCRound, "#,##0.000").ToString, , , , , , , "Land Cost " & CurrFC)

                .ColDesign("lastcostlc", 100, "Last Cost " & gbCurr1, AlignHorzEnum.Far, AlignVertEnum.Top, , , , , , , , , , , "N" & VatLCRound, , , , , , )
                .ColDesign("lastcostcc", 100, "Last Cost " & gbCurr2, AlignHorzEnum.Far, AlignVertEnum.Top, , , , , , , , , , , "#,##0.000", , , , , , )
                .ColDesign("lastcostFc", 100, "Last Cost " & CurrFC, AlignHorzEnum.Far, AlignVertEnum.Top, CBool(IIf(CurrFC = gbCurr1 Or CurrFC = gbCurr2, False, True)), , , , , CBool(IIf(CurrFC = gbCurr1 Or CurrFC = gbCurr2, False, True)), , , , , IIf(CurrFC = gbCurr1, "N" & VatLCRound, "#,##0.000").ToString, , , , , , , "Land Cost " & CurrFC)

                '.ColDesign("Ret LC", 100, "Ret " & gbCurr1, AlignHorzEnum.Far, AlignVertEnum.Top, , , , , , , , , , , IIf(gbCurr1 = "LL", "#,##0", "#,##0.000").ToString, , , , , , , "Ret " & gbCurr1)
                '   .ColDesign("Ret CC", 100, "Ret " & gbCurr2, AlignHorzEnum.Far, AlignVertEnum.Top, , , , , , , , , , , "#,##0.000", , , , , , , "Ret " & gbCurr2)
                '.ColDesign("Land Cost LC", 100, "Land Cost " & gbCurr1, AlignHorzEnum.Far, AlignVertEnum.Top, False, , , , , False, , , , , , , , , , , False)
                '.ColDesign("Land Cost CC", 100, "Land Cost " & gbCurr2, AlignHorzEnum.Far, AlignVertEnum.Top, False, , , , , False, , , , , , , , , , , False)
                '.ColDesign("Land Cost FC", 100, "Land Cost " & CurrFC, AlignHorzEnum.Far, AlignVertEnum.Top, False, , , , , False, , , , , , , , , , , False)
                .ColDesign("Ret LC", 100, "Ret " & gbCurr1, AlignHorzEnum.Far, AlignVertEnum.Top, False, , , , , False, , , , , , , , , , , False)
                .ColDesign("Ret CC", 100, "Ret " & gbCurr2, AlignHorzEnum.Far, AlignVertEnum.Top, False, , , , , False, , , , , , , , , , , False)
                .ColDesign("Stor. Loc", -1, "Stor. Loc", AlignHorzEnum.Center, AlignVertEnum.Top, , , , , , , , , , , , , , , , , False)
                .ColDesign("Supp", 0, "Supp", AlignHorzEnum.Center, AlignVertEnum.Top, False, , , , , False, , , , , , , , , , , False)

                .ColDesign("Category", 0, "Category", AlignHorzEnum.Center, AlignVertEnum.Top, False, , , , , False, , , , , , , , , , , False)
                .ColDesign("Serie", 0, "Serial", AlignHorzEnum.Center, AlignVertEnum.Top, False, , , , , False, , , , , , , , , , , False)
                .ColDesign("Usage", 60, "Usage", AlignHorzEnum.Center, AlignVertEnum.Top, False, , , , , False, , , , , , , , , , , False)
                .ColDesign("NewUsed", 70, "NewUsed", AlignHorzEnum.Center, AlignVertEnum.Top, False, , , , , False, , , , , , , , , , , False)
                .ColDesign("Det Rem", 100, "Det Rem", AlignHorzEnum.Near, AlignVertEnum.Top, False, , , , , False, , , , , , , , , , , False)
                .ColDesign("VATTYPE", 70, "VAT Type", AlignHorzEnum.Center, AlignVertEnum.Top)
                .ColDesign("VATPER", 60, "VAT Per", AlignHorzEnum.Far, AlignVertEnum.Top)
                .ColDesign("Itm Ident Desc", 0, "Itm Ident Desc", AlignHorzEnum.Near, AlignVertEnum.Top, False, , , , , False, , , , , , , , , , , False)
                .ColDesign("Product Grp Desc", 0, "", AlignHorzEnum.Near, AlignVertEnum.Top, False, , , , , False, , , , , , , , , , , False)
                'Abir T. @ 21.11.2013
                If Mode = 0 Then
                    .ColDesign("FLQTY", 80, "Free Length QTY", AlignHorzEnum.Far, AlignVertEnum.Top, , , , , True, , , , , , QtyFormat, , , , , , , "Free Length QTY")
                    .ColDesign("NLQTY", 80, "Normal Length QTY", AlignHorzEnum.Far, AlignVertEnum.Top, , , , , True, , , , , , QtyFormat, , , , , , , "Normal Length QTY")
                    .ColDesign("FWQTY", 80, "Free Weight QTY", AlignHorzEnum.Far, AlignVertEnum.Top, , , , , True, , , , , , QtyFormat, , , , , , , "Free Weight QTY")
                    .ColDesign("NWQTY", 80, "Normal Weight QTY", AlignHorzEnum.Far, AlignVertEnum.Top, , , , , True, , , , , , QtyFormat, , , , , , , "Normal Weight QTY")
                    .ColDesign("FPQTY", 80, "Free Piece QTY", AlignHorzEnum.Far, AlignVertEnum.Top, , , , , True, , , , , , QtyFormat, , , , , , , "Free Piece QTY")
                    .ColDesign("NPQTY", 80, "Normal Piece QTY", AlignHorzEnum.Far, AlignVertEnum.Top, , , , , True, , , , , , QtyFormat, , , , , , , "Normal Piece QTY")
                    .ColDesign("RLQTY", 80, "Return Length QTY", AlignHorzEnum.Far, AlignVertEnum.Top, , , , , True, , , , , , QtyFormat, , , , , , , )
                    .ColDesign("RWQTY", 80, "Return Weight QTY", AlignHorzEnum.Far, AlignVertEnum.Top, , , , , True, , , , , , QtyFormat, , , , , , , )
                    .ColDesign("RPQTY", 80, "Return Piece QTY", AlignHorzEnum.Far, AlignVertEnum.Top, , , , , True, , , , , , QtyFormat, , , , , , , )
                    '... first Price for the default price type
                    .ColDesign("SP_LCTAXEX", 100, PriceType & gbCurr1 & " Ex", AlignHorzEnum.Far, AlignVertEnum.Top, , , , , , , , , , , "N" & VatLCRound, , , , , , , PriceType & gbCurr1 & " Ex")
                    .ColDesign("SP_LCTTC", 100, PriceType & gbCurr1 & " TTC", AlignHorzEnum.Far, AlignVertEnum.Top, , , , , , , , , , , "N" & VatLCRound, , , , , , , PriceType & gbCurr1 & " TTC")
                    .ColDesign("SP_FCTAXEX", 100, PriceType & gbCurr2 & " Ex", AlignHorzEnum.Far, AlignVertEnum.Top, , , , , , , , , , , "#,##0.000", , , , , , , PriceType & gbCurr2 & " Ex")
                    .ColDesign("SP_FCTTC", 100, PriceType & gbCurr2 & " TTC", AlignHorzEnum.Far, AlignVertEnum.Top, , , , , , , , , , , "#,##0.000", , , , , , , PriceType & gbCurr2 & " TTC")
                    .ColDesign("HSCode", 0, "HSCode", AlignHorzEnum.Near, AlignVertEnum.Top, False, , , , , False)
                ElseIf ReportName.ToUpper.Contains("PETMART") AndAlso ReportName.ToUpper.Contains("SIMPLE") Then
                    .ColDesign("PIBasepriceType1", 100, "Based Price 1", AlignHorzEnum.Far, AlignVertEnum.Top, , , , , , , , , , , "#,##0.000", , , , )
                    .ColDesign("PIBasepriceType2", 100, "Based Price 2", AlignHorzEnum.Far, AlignVertEnum.Top, , , , , , , , , , , "#,##0.000", , , , )
                    .ColDesign("HEadPIBasepriceType2", 0, "", AlignHorzEnum.Far, AlignVertEnum.Top, False, , , , , False)
                    .ColDesign("HEadPIBasepriceType1", 0, "", AlignHorzEnum.Far, AlignVertEnum.Top, False, , , , , False)
                    .ColDesign("invd_salesLC2", 0, "", AlignHorzEnum.Far, AlignVertEnum.Top, False, , , , , False)
                    .ColDesign("invd_salesCC2", 0, "", AlignHorzEnum.Far, AlignVertEnum.Top, False, , , , , False)
                    .ColDesign("SupType", 250, "Supplier Type", AlignHorzEnum.Near, AlignVertEnum.Top)
                    .ColDesign("HSCode", 70, "HSCode", AlignHorzEnum.Near, AlignVertEnum.Top)
                    .ColDesign("STK_MINQTY", 70, "Min Qty", AlignHorzEnum.Near, AlignVertEnum.Top)
                    .ColDesign("STK_MAXQTY", 70, "Max Qty", AlignHorzEnum.Near, AlignVertEnum.Top)
                    If PrintAvailableQty And tmpAvailableQtyLocFiltr <> "" Then
                        .ColDesign("STQ_AVAILABLE", 100, "Available Qty", AlignHorzEnum.Far, AlignVertEnum.Top, , , , , , , , , , , "#,##0.000", , , , )
                    Else
                        .ColDesign("STQ_AVAILABLE", 0, "Available Qty", AlignHorzEnum.Far, AlignVertEnum.Top, False, , , , , False, , , , , "#,##0.000", , , , )
                    End If
                End If
                'ch @ 25/05/2017 email ( Purchase Module - Reprint PO/ PI)
                '.ColDesign("Rate1", 80, "Rate1", AlignHorzEnum.Far, AlignVertEnum.Top, , , , , True)
                '.ColDesign("Rate2", 80, "Rate2", AlignHorzEnum.Far, AlignVertEnum.Top, , , , , True)
                '.ColDesign("opr1", 80, "opr1", AlignHorzEnum.Center, AlignVertEnum.Top, , , , , True)
                '.ColDesign("opr2", 80, "opr2", AlignHorzEnum.Center, AlignVertEnum.Top, , , , , True)
                .ColDesign("Rate1", 0, "Rate1", AlignHorzEnum.Far, AlignVertEnum.Top, False, , , , , False)
                .ColDesign("Rate2", 0, "Rate2", AlignHorzEnum.Far, AlignVertEnum.Top, False, , , , , False)
                .ColDesign("opr1", 0, "opr1", AlignHorzEnum.Center, AlignVertEnum.Top, False, , , , , False)
                .ColDesign("opr2", 0, "opr2", AlignHorzEnum.Center, AlignVertEnum.Top, False, , , , , False)

                .ColDesign("INVBBarcode", 0, "INVBBarcode", AlignHorzEnum.Center, AlignVertEnum.Top, False, , , , , False)
                .ColDesign("br_Name", 0, "br_Name", AlignHorzEnum.Center, AlignVertEnum.Top, False, , , , , False)

                'ch @ 25/05/2017 email ( Purchase Module - Reprint PO/ PI)
                '.ColDesign("FwdName", 100, "FwdName", AlignHorzEnum.Near, AlignVertEnum.Top, , , , , True)
                ' .ColDesign("LandAmt", 100, "LandAmt", AlignHorzEnum.Far, AlignVertEnum.Top, , , , , True, , , , , , IIf(CurrFC = "LL", "#,##0", "#,##0.000").ToString)
                '.ColDesign("TotInvDisc", 100, "TotInvDisc", AlignHorzEnum.Far, AlignVertEnum.Top, , , , , True, , , , , , IIf(CurrFC = "LL", "#,##0", "#,##0.000").ToString)
                .ColDesign("FwdName", 0, "FwdName", AlignHorzEnum.Near, AlignVertEnum.Top, False, , , , , False, , , , , , , , , , , False)
                .ColDesign("LandAmt", 0, "LandAmt", AlignHorzEnum.Far, AlignVertEnum.Top, False, , , , , False, , , , , )
                .ColDesign("TotInvDisc", 0, "TotInvDisc", AlignHorzEnum.Far, AlignVertEnum.Top, False, , , , , False, , , , , )

                'Abir T. @ 21.05.2014 : ShipInfo Columns
                '.ColDesign("Ship Mode", 60, "Ship Mode", AlignHorzEnum.Near, AlignVertEnum.Top, ShipInfo, , , , True, ShipInfo)
                '.ColDesign("Truck Name", 80, "Truck Name", AlignHorzEnum.Near, AlignVertEnum.Top, ShipInfo, , , , True, ShipInfo)
                '.ColDesign("Consignee", 80, "Consignee", AlignHorzEnum.Near, AlignVertEnum.Top, ShipInfo, , , , True, ShipInfo)
                '.ColDesign("Notify", 80, "Notify", AlignHorzEnum.Near, AlignVertEnum.Top, ShipInfo, , , , True, ShipInfo)
                '.ColDesign("Shipper", 80, "Shipper", AlignHorzEnum.Near, AlignVertEnum.Top, ShipInfo, , , , True, ShipInfo)
                '.ColDesign("Shipped To", 80, "Shipped To", AlignHorzEnum.Near, AlignVertEnum.Top, ShipInfo, , , , True, ShipInfo)
                '.ColDesign("Sold To", 80, "Sold To", AlignHorzEnum.Near, AlignVertEnum.Top, ShipInfo, , , , True, ShipInfo)
                '.ColDesign("Payment To", 80, "Payment To", AlignHorzEnum.Near, AlignVertEnum.Top, ShipInfo, , , , True, ShipInfo)
                '.ColDesign("Port OfLoad", 80, "Port OfLoad", AlignHorzEnum.Near, AlignVertEnum.Top, ShipInfo, , , , True, ShipInfo)
                '.ColDesign("Master BL", 80, "Master BL", AlignHorzEnum.Near, AlignVertEnum.Top, ShipInfo, , , , True, ShipInfo)
                '.ColDesign("House BL", 80, "House BL", AlignHorzEnum.Near, AlignVertEnum.Top, ShipInfo, , , , True, ShipInfo)
                '.ColDesign("Delivery", 80, "Delivery", AlignHorzEnum.Near, AlignVertEnum.Top, ShipInfo, , , , True, ShipInfo)
                '.ColDesign("Validity", 80, "Validity", AlignHorzEnum.Near, AlignVertEnum.Top, ShipInfo, , , , True, ShipInfo)
                '.ColDesign("Bank Details", 80, "Bank Details", AlignHorzEnum.Near, AlignVertEnum.Top, ShipInfo, , , , True, ShipInfo)
                '.ColDesign("Aux. Acc.", 80, "Aux. Acc.", AlignHorzEnum.Near, AlignVertEnum.Top, ShipInfo, , , , True, ShipInfo)
                '.ColDesign("Acc. Name", 80, "Acc. Name", AlignHorzEnum.Near, AlignVertEnum.Top, ShipInfo, , , , True, ShipInfo)
                '.ColDesign("Ex. Ship. Date", 80, "Ex. Ship. Date", AlignHorzEnum.Near, AlignVertEnum.Top, ShipInfo, , , , True, ShipInfo, , , , , "dd/MM/yyyy")
                '.ColDesign("Act. Ship. Date", 80, "Act. Ship. Date", AlignHorzEnum.Near, AlignVertEnum.Top, ShipInfo, , , , True, ShipInfo, , , , , "dd/MM/yyyy")
                '.ColDesign("Ex. Arvl. Date", 80, "Ex. Arvl. Date", AlignHorzEnum.Near, AlignVertEnum.Top, ShipInfo, , , , True, ShipInfo, , , , , "dd/MM/yyyy")
                '.ColDesign("Ex. Arvl. Rks", 120, "Ex. Arvl. Rks", AlignHorzEnum.Near, AlignVertEnum.Top, ShipInfo, , , , True, ShipInfo)
                '.ColDesign("Act. Arvl. Date", 80, "Act. Arvl. Date", AlignHorzEnum.Near, AlignVertEnum.Top, ShipInfo, , , , True, ShipInfo, , , , , "dd/MM/yyyy")
                '.ColDesign("Act. Arvl. Rks", 120, "Act. Arvl. Rks", AlignHorzEnum.Near, AlignVertEnum.Top, ShipInfo, , , , True, ShipInfo)

                '.ColDesign("Container Reference", 80, "Container Reference", AlignHorzEnum.Near, AlignVertEnum.Top, ShipInfo, , , , True, ShipInfo) 'added by GS on 16/09/16
                .ColDesign("Ship Mode", 60, "Ship Mode", AlignHorzEnum.Near, AlignVertEnum.Top, False, , , , , False, , , , , , , , , , , False)
                .ColDesign("Truck Name", 80, "Truck Name", AlignHorzEnum.Near, AlignVertEnum.Top, False, , , , , False, , , , , , , , , , , False)
                .ColDesign("Consignee", 80, "Consignee", AlignHorzEnum.Near, AlignVertEnum.Top, False, , , , , False, , , , , , , , , , , False)
                .ColDesign("Notify", 80, "Notify", AlignHorzEnum.Near, AlignVertEnum.Top, False, , , , , False, , , , , , , , , , , False)
                .ColDesign("Shipper", 80, "Shipper", AlignHorzEnum.Near, AlignVertEnum.Top, False, , , , , False, , , , , , , , , , , False)
                .ColDesign("Shipped To", 80, "Shipped To", AlignHorzEnum.Near, AlignVertEnum.Top, False, , , , , False, , , , , , , , , , , False)
                .ColDesign("Sold To", 80, "Sold To", AlignHorzEnum.Near, AlignVertEnum.Top, False, , , , , False, , , , , , , , , , , False)
                .ColDesign("Payment To", 80, "Payment To", AlignHorzEnum.Near, AlignVertEnum.Top, False, , , , , False, , , , , , , , , , , False)
                .ColDesign("Port OfLoad", 80, "Port OfLoad", AlignHorzEnum.Near, AlignVertEnum.Top, False, , , , , False, , , , , , , , , , , False)
                .ColDesign("Master BL", 80, "Master BL", AlignHorzEnum.Near, AlignVertEnum.Top, False, , , , , False, , , , , , , , , , , False)
                .ColDesign("House BL", 80, "House BL", AlignHorzEnum.Near, AlignVertEnum.Top, False, , , , , False, , , , , , , , , , , False)
                .ColDesign("Delivery", 80, "Delivery", AlignHorzEnum.Near, AlignVertEnum.Top, False, , , , , False, , , , , , , , , , , False)
                .ColDesign("Validity", 80, "Validity", AlignHorzEnum.Near, AlignVertEnum.Top, False, , , , , False, , , , , , , , , , , False)
                .ColDesign("Bank Details", 80, "Bank Details", AlignHorzEnum.Near, AlignVertEnum.Top, False, , , , , False, , , , , , , , , , , False)
                .ColDesign("Aux. Acc.", 80, "Aux. Acc.", AlignHorzEnum.Near, AlignVertEnum.Top, False, , , , , False, , , , , , , , , , , False)
                .ColDesign("Acc. Name", 80, "Acc. Name", AlignHorzEnum.Near, AlignVertEnum.Top, False, , , , , False, , , , , , , , , , , False)
                .ColDesign("Ex. Ship. Date", 80, "Ex. Ship. Date", AlignHorzEnum.Near, AlignVertEnum.Top, False, , , , , False, , , , , , , , , , , False)
                .ColDesign("Act. Ship. Date", 80, "Act. Ship. Date", AlignHorzEnum.Near, AlignVertEnum.Top, False, , , , , False, , , , , , , , , , , False)
                .ColDesign("Ex. Arvl. Date", 80, "Ex. Arvl. Date", AlignHorzEnum.Near, AlignVertEnum.Top, False, , , , , False, , , , , , , , , , , False)
                .ColDesign("Ex. Arvl. Rks", 120, "Ex. Arvl. Rks", AlignHorzEnum.Near, AlignVertEnum.Top, False, , , , , False, , , , , , , , , , , False)
                .ColDesign("Act. Arvl. Date", 80, "Act. Arvl. Date", AlignHorzEnum.Near, AlignVertEnum.Top, False, , , , , False, , , , , , , , , , , False)
                .ColDesign("Act. Arvl. Rks", 120, "Act. Arvl. Rks", AlignHorzEnum.Near, AlignVertEnum.Top, False, , , , , False, , , , , , , , , , , False)
                .ColDesign("No Containers", 80, "No Containers", AlignHorzEnum.Near, AlignVertEnum.Top, False, , , , , False, , , , , , , , , , , False)
                .ColDesign("Container Reference", 80, "Container Reference", AlignHorzEnum.Near, AlignVertEnum.Top, False, , , , , False, , , , , , , , , , , False) 'added by GS on 16/09/16
                .ColDesign("TechContent", 80, "Technical Content", AlignHorzEnum.Near, AlignVertEnum.Top, techContent, , , , True, techContent) 'added by GS on 16/09/16
                'ch @ 16/05/2017 
                .ColDesign("INVD_COEF", 0, "", AlignHorzEnum.Near, AlignVertEnum.Top, False, , , , , False)
                .ColDesign("INVD_RETLC", 0, "", AlignHorzEnum.Near, AlignVertEnum.Top, False, , , , , False)
                .ColDesign("INVD_RETfCEx", 0, "", AlignHorzEnum.Near, AlignVertEnum.Top, False, , , , , False)
                .ColDesign("INVD_RETfCTTC", 0, "", AlignHorzEnum.Near, AlignVertEnum.Top, False, , , , , False)
                .ColDesign("INVD_RETlCTTC", 0, "", AlignHorzEnum.Near, AlignVertEnum.Top, False, , , , , False)

                If gbFundDisbursementModuleIsActive And PrintFundDisbursement Then
                    .ColDesign("INVD_RECID", 0, "", AlignHorzEnum.Near, AlignVertEnum.Top, False, , , , , False)
                End If


                .ColDesign("inv_dstamp", 0, "", AlignHorzEnum.Near, AlignVertEnum.Top, False, , , , , False)
                'Added By Abir T. @ 24.06.2013
                Dim sbr1 As frmPreviewGrid.SubReport
                Dim sbr2 As frmPreviewGrid.SubReport

                Str = "SELECT * FROM " & TempPISubRep1
                sbr1 = New frmPreviewGrid.SubReport("PayTerm")
                sbr1.AddECParameter("@PayTermsStr", Str)
                frm.InitSubReport(sbr1)

                If Not Fashion Then
                    Str = "SELECT PIDRecid,[Batch No.],CASE WHEN LEFT(ExpiryDate,2) ='00' THEN  SUBSTRING(ExpiryDate , 4,LEN(ExpiryDate)) ELSE ExpiryDate END ExpiryDate ,Qty,"
                    Str &= " CASE WHEN LEFT(ProductionDate,2) ='00' THEN  SUBSTRING(ProductionDate , 4,LEN(ProductionDate)) ELSE ProductionDate END [ProductionDate], Barcode   FROM " & TempPISubRep2
                    sbr2 = New frmPreviewGrid.SubReport("ExpQty")
                    sbr2.AddECParameter("@ExpQtyStr", Str)
                    frm.InitSubReport(sbr2)
                End If

                If ReportName = "rptPurInvList_Details_WithFunds.rpt" Or ReportName = "rptPurInvList_Simple_WithFunds.rpt" Or ReportName = "rptPurInvList_SimplePetMart_WithFunds.rpt" Then
                    Dim sbr3 As frmPreviewGrid.SubReport
                    sbr3 = New frmPreviewGrid.SubReport("Fund")
                    sbr3.AddECParameter("@Fund", FundQuery)
                    sbr3.AddECParameter("Curr1", gbCurr1)
                    sbr3.AddECParameter("Curr2", gbCurr2)
                    sbr3.AddECParameter("deci", IIf(gbDecNo = 2, 2, 3).ToString)
                    frm.InitSubReport(sbr3)
                End If

                frm.SendStockImageOption = True
                frm.StockRecIDCol = "StkRecid"
                frm.ShowDialog(CType(strPrev, String(,)), frmPI.Name, ReportName, rptParam)
            End With
        Catch exp As Exception
            Throw exp
        Finally
            If Not frm Is Nothing Then frm = Nothing
            Cursor.Current = Cursors.Default
            If Not drSql Is Nothing Then
                If drSql.IsClosed = False Then drSql.Close()
                drSql = Nothing
            End If
        End Try
    End Sub
    'Print Report direclty , without Preview Grid
    Public Sub PrintPIDirectRep(ByVal frm As Form, ByVal SQLstat As String, ByVal ReportName As String,
                ByVal TempPISubRep1 As String, ByVal TempPISubRep2 As String, ByVal Fashion As Boolean,
                Optional ByVal rptParam(,) As Object = Nothing, Optional ByVal NbPrint As Integer = 1, Optional ORDER As String = "", Optional FundQuery As String = "")
        Dim cnSql As SqlConnection
        Dim cmSql As SqlCommand
        Dim trnSql As SqlTransaction
        Dim strsql As String
        Dim TotNoUser As String
        Dim SelectVch As String
        Dim ViewName As String
        Try
            Cursor.Current = Cursors.WaitCursor
            ViewName = "VPP_" & frm.Name & "_" & gbUser

            cnSql = New SqlConnection(ConnectionString)
            cnSql.Open()

            trnSql = cnSql.BeginTransaction
            cmSql = New SqlCommand(strsql, cnSql, trnSql)

            strsql = "if exists (select * FROM sysobjects where id = object_id(N'" & ViewName & "') "
            strsql &= " and OBJECTPROPERTY(id, N'Isview') = 1) drop view " & ViewName
            cmSql.CommandText = strsql
            cmSql.ExecuteNonQuery()

            'create view
            strsql = " Create View " & ViewName & " AS " & SQLstat
            cmSql.CommandText = strsql
            cmSql.ExecuteNonQuery()
            trnSql.Commit()

            strsql = "SELECT * FROM " & ViewName & " " & ORDER

            If TempPISubRep1 <> "" Then
                strPrev = Nothing
                Dim sbr As ModPrint.SubReport
                Dim str As String
                str = "SELECT * FROM " & TempPISubRep1
                sbr = New ModPrint.SubReport("PayTerm")
                sbr.AddECParameter("@PayTermsStr", str)
                ModPrint.InitSubReport(sbr)
            End If

            If TempPISubRep2 <> "" Then
                strPrev = Nothing
                Dim sbr As ModPrint.SubReport
                Dim str As String
                'ch @ 12/10/2016 
                'str = "SELECT * FROM " & TempPISubRep2
                str = "SELECT PIDRecid,[Batch No.],CASE WHEN LEFT(ExpiryDate,2) ='00' THEN  SUBSTRING(ExpiryDate , 4,LEN(ExpiryDate)) ELSE ExpiryDate END ExpiryDate,"
                str &= " CASE WHEN LEFT(ProductionDate,2) ='00' THEN  SUBSTRING(ProductionDate , 4,LEN(ProductionDate)) ELSE ProductionDate END ProductionDate   ,Qty,Barcode FROM " & TempPISubRep2
                sbr = New ModPrint.SubReport("ExpQty")
                sbr.AddECParameter("@ExpQtyStr", str)
                ModPrint.InitSubReport(sbr)
            End If

            If ReportName = "rptPurInvList_Details_WithFunds.rpt" Or ReportName = "rptPurInvList_Simple_WithFunds.rpt" Or ReportName = "rptPurInvList_SimplePetMart_WithFunds.rpt" Then
                Dim sbr3 As ModPrint.SubReport
                sbr3 = New ModPrint.SubReport("Fund")
                sbr3.AddECParameter("@Fund", FundQuery)
                sbr3.AddECParameter("Curr1", gbCurr1)
                sbr3.AddECParameter("Curr2", gbCurr2)
                sbr3.AddECParameter("deci", IIf(gbDecNo = 2, 2, 3).ToString)
                ModPrint.InitSubReport(sbr3)
            End If

            PreviewProcedure(strsql, ORDER, ReportName, frm, rptParam, NbPrint, True) 'edited by GS on 15/09/16 (add withLogo = True)

        Catch ex As Exception
            Throw ex
        Finally
            Cursor.Current = Cursors.Default
            cmSql = Nothing
            If Not cnSql Is Nothing Then
                If cnSql.State = ConnectionState.Open Then cnSql.Close()
                cnSql = Nothing
            End If
        End Try
    End Sub
#End Region

#Region "Forwarder Invoice"
    'Print Sub Report - Forwarder Invoice
    'Added By Abir T. @ 11.03.2013
    Public Sub CreateInsTempSubRep(ByVal TempSubRep As String)
        Dim Str As String
        Try
            Str = " IF EXISTS (SELECT name FROM sysobjects WHERE type = 'U' AND name = '" & TempSubRep & "')"
            Str &= " DROP TABLE " & TempSubRep
            Execute(Str)

            Str = " CREATE TABLE " & TempSubRep & "("
            Str &= " [Idd] [Numeric](18, 0) NOT NULL,"
            Str &= " [T_Curr] [Varchar] (5)  NULL ,"
            Str &= " [T_Amt] [Numeric](18, 3) NULL )"
            Execute(Str)

            Str = " INSERT INTO " & TempSubRep & " SELECT FID_HRecid, FID_Curr, SUM(FID_OHValue) Amt"
            Str &= " FROM STK04_FwdInvDet GROUP BY FID_Curr, FID_HRecid"
            Execute(Str)
        Catch ex As Exception
            Throw ex
        End Try
    End Sub
    Public Function FwdSumRep(ByVal frm As Form, ByVal Cond As String, ByVal TempSubRep As String, ByVal Reversed As Boolean, Optional ByVal PrintDirect As Boolean = False) As Boolean
        Dim strsql1, strsql2 As String
        '   Dim drSQL As SqlDataReader
        Dim ReportName As String
        Try
            FwdSumRep = False

            strsql1 = "SELECT DISTINCT FWD_COMP + ' - ' + Comp_Name AS [Company],"
            strsql1 &= " FWD_RECID AS [Fwd RecID], FWD_CODE + ' - ' + FWD_NAME AS [Forwarder],"
            strsql1 &= " FID_HRecid AS [Head RecID], FIH_RECID AS [RecID], FIH_FWDINV AS [Inv. No],"
            strsql1 &= " CASE WHEN FIH_NORREV = 'N' THEN 'Normal' ELSE 'Reverse' END AS [N/R],"
            strsql1 &= " FIH_DATE AS [Inv. Date], FIH_RECEIVED AS [Received On],"
            strsql1 &= " FIH_PFWDLC AS [Fwd LC], FIH_PFWDCC AS [Fwd CC],"
            strsql1 &= " FIH_PTXLC AS [PTX LC], FIH_PTXCC AS [PTX CC],"
            strsql1 &= " FIH_TTXLC AS [TTX LC], FIH_TTXCC AS [TTX CC],"
            strsql1 &= " FIH_LC AS [LC], FIH_CC AS [CC], FID_RKS AS [Remarks],"
            strsql1 &= " CASE WHEN FIH_STATUS = '1' THEN 'Not Confirmed'"
            strsql1 &= "  WHEN FIH_STATUS = '2' and FIH_GenToAcc='Y' THEN 'Confirmed & A/C Generated' WHEN FIH_STATUS = '2' and FIH_GenToAcc='N' THEN 'Confirmed' END AS [Status],"
            strsql1 &= " (CASE WHEN FIH_Status = '2' THEN 'True' ELSE 'False' END) AS [Updated],"
            strsql1 &= " FIH_UPDDATE AS [Updated Date], '" & Reversed & "' AS Reversed,"

            strsql1 &= " OH_CODE AS [OH Code], OH_DESC AS [OH Desc], FID_COUNTER AS [Counter],"
            strsql1 &= " (CASE WHEN FID_INCLUDE='Y' THEN 'True' ELSE 'False' END) AS [Incl %],"
            strsql1 &= " CASE WHEN OH_CSTTYPE = 'F' THEN 'Forwarder Fees' ELSE 'Compensation' END AS [Fwd/Comp],"
            strsql1 &= " OH_OHType AS [OH Type], OH_ItmType AS [OH Item Type],"
            strsql1 &= " FID_OHVALUE AS [Value], FID_CURR AS [Curr],"
            strsql1 &= " FID_OHVALLC AS [Value LC], FID_OHVALCC AS [Value CC]"

            strsql2 = " FROM STK04_Overheads"
            strsql2 &= " INNER JOIN STK04_FwdInvDet ON OH_RECID = FID_OHRECID"
            strsql2 &= " INNER JOIN STK04_FwdInvHead ON FID_HRECID = FIH_RECID"
            strsql2 &= " INNER JOIN STK04_Forwarder ON FIH_FWDCODE = FWD_RECID"
            strsql2 &= " INNER JOIN Com_Company ON FWD_COMP = Comp_Code"
            strsql2 &= " WHERE 1=1 "
            If Cond <> "" Then strsql2 &= " " + Cond

            'If Not drSQL.HasRows Then
            '    MessageBox.Show("No Records Found.", frm.Text, MessageBoxButtons.OK, MessageBoxIcon.Information)
            '    FwdSumRep = True : Exit Function
            'End If
            If Not RecordsExists("Select Top 1 1 " & strsql2) Then
                MessageBox.Show("No Records Found.", frm.Text, MessageBoxButtons.OK, MessageBoxIcon.Information)
                FwdSumRep = True : Exit Function
            End If

            ReportName = "rptFwdInvSum.rpt"
            If PrintDirect = True Then
                PrintFISumDirectRep(frm, strsql1 + strsql2, ReportName, TempSubRep)
            Else
                PrintFISumWithPrevGrid(frm, strsql1 + strsql2, ReportName, TempSubRep, Cond)
            End If
        Catch ex As Exception
            Throw ex
        End Try
    End Function
    Public Sub PrintFISumWithPrevGrid(ByVal frm1 As Form, ByVal strsql As String, ByVal ReportName As String, ByVal SubTempTable As String, ByVal Cond As String)
        Dim frm As frmPreviewGrid
        Dim drSql As SqlDataReader
        Dim strsqlPrev As String
        Try
            Cursor.Current = Cursors.WaitCursor
            frm = New frmPreviewGrid

            strsqlPrev = " Select DISTINCT FIH_FWDINV AS [Inv. No] ,   FWD_COMP + ' - ' + Comp_Name AS [Company] ," 'FWD_RECID AS [Fwd RecID] ,
            strsqlPrev &= "   FWD_CODE + ' - ' + FWD_NAME AS [Forwarder] ,  " ' FID_HRecid AS [Head RecID] ,"
            ' strsqlPrev &= " FIH_RECID AS [RecID] , " '   CASE WHEN FIH_NORREV = 'N' THEN 'Normal'  ELSE 'Reverse'   END AS [N/R] ,"
            strsqlPrev &= " FIH_DATE AS [Inv. Date] , FIH_RECEIVED AS [Received On] ,"
            strsqlPrev &= " CASE WHEN FIH_STATUS = '1' THEN 'Not Confirmed'  WHEN FIH_STATUS = '2' and FIH_GenToAcc='Y' THEN 'Confirmed & A/C Generated' WHEN FIH_STATUS = '2' and FIH_GenToAcc='N' THEN 'Confirmed' END AS [Status] ,"
            strsqlPrev &= " FIH_UPDDATE AS [Updated Date] ,OH_CODE AS [OH Code] ,  OH_DESC AS [OH Desc] , FID_RKS AS [Remarks] ,"
            strsqlPrev &= " ( CASE WHEN FID_INCLUDE = 'Y' THEN 'True' ELSE 'False'    END ) AS [Incl %] ,"
            strsqlPrev &= " CASE WHEN OH_CSTTYPE = 'F' THEN 'Forwarder Fees'  ELSE 'Compensation'     END AS [Fwd/Comp] ,"
            strsqlPrev &= " OH_OHType AS [OH Type] ,  OH_ItmType AS [OH Item Type] ,  FID_OHVALUE AS [Value] ,   FID_CURR AS [Curr] ,"
            strsqlPrev &= " FID_OHVALLC AS [Value LC] ,  FID_OHVALCC AS [Value CC],"
            strsqlPrev &= " FIH_PFWDLC +  FIH_PTXLC [PERMANENT EXPENSES LC], FIH_PFWDCC +  FIH_PTXCC [PERMANENT EXPENSES CC],"
            strsqlPrev &= " FIH_PFWDLC AS [Fwd LC], FIH_PFWDCC AS [Fwd CC] ,  FIH_PTXLC AS [PTX LC] , FIH_PTXCC AS [PTX CC] ,"
            strsqlPrev &= " FIH_TTXLC AS [TTX LC] ,  FIH_TTXCC AS [TTX CC] ,     "
            strsqlPrev &= " FIH_PFWDLC + FIH_PTXLC + FIH_TTXLC [Invoice TOTALS LC] , FIH_PFWDCC + FIH_PTXCC + FIH_TTXCC  [Invoice TOTALS CC]"

            'strsqlPrev &= "  FIH_LC AS [LC] ,  FIH_CC AS [CC] ,   ( CASE WHEN FIH_Status = '2' THEN 'True'  ELSE 'False' END ) AS [Updated] , "
            'strsqlPrev &= "  'False' AS Reversed , FID_COUNTER AS [Counter] "

            strsqlPrev &= " FROM STK04_Overheads "
            strsqlPrev &= " INNER JOIN STK04_FwdInvDet ON OH_RECID = FID_OHRECID"
            strsqlPrev &= " INNER JOIN STK04_FwdInvHead ON FID_HRECID = FIH_RECID"
            strsqlPrev &= " INNER JOIN STK04_Forwarder ON FIH_FWDCODE = FWD_RECID"
            strsqlPrev &= " INNER JOIN Com_Company ON FWD_COMP = Comp_Code"
            strsqlPrev &= "  WHERE 1 = 1 "
            If Cond <> "" Then strsqlPrev &= " " + Cond

            frm = New frmPreviewGrid
            strPrev = Nothing
            With frm
                frm.BuildSql(strsqlPrev, , , , , strsql)
                .ColDesign("Inv. No", -1, "Inv. No", C1.Win.C1TrueDBGrid.AlignHorzEnum.Near, C1.Win.C1TrueDBGrid.AlignVertEnum.Top, , , , , True, , frmPreviewGrid.Orders.Ascending)
                .ColDesign("Company", -1, "Company", C1.Win.C1TrueDBGrid.AlignHorzEnum.Near, C1.Win.C1TrueDBGrid.AlignVertEnum.Top, , , , , True)
                ' .ColDesign("Fwd RecID", -1, "Fwd RecID", C1.Win.C1TrueDBGrid.AlignHorzEnum.Center, C1.Win.C1TrueDBGrid.AlignVertEnum.Top, , , , , True, False, , , , , , , , , , , False)
                .ColDesign("Forwarder", -1, "Forwarder", C1.Win.C1TrueDBGrid.AlignHorzEnum.Near, C1.Win.C1TrueDBGrid.AlignVertEnum.Top, , , , , True)
                '   .ColDesign("Head RecID", -1, "Head RecID", C1.Win.C1TrueDBGrid.AlignHorzEnum.Center, , , , , , True, False, , , , , , , , , , , False)
                ' .ColDesign("RecID", -1, "RecID", C1.Win.C1TrueDBGrid.AlignHorzEnum.Center, , , , , , , False, , , , , , , , , , , False)


                .ColDesign("Inv. Date", -1, "Inv. Date", C1.Win.C1TrueDBGrid.AlignHorzEnum.Near, C1.Win.C1TrueDBGrid.AlignVertEnum.Top, , , , , True, , , , , , "dd/MM/yyyy")
                .ColDesign("Received On", -1, "Received On", C1.Win.C1TrueDBGrid.AlignHorzEnum.Near, C1.Win.C1TrueDBGrid.AlignVertEnum.Top, , , , , True, , , , , , "dd/MM/yyyy")
                .ColDesign("Status", -1, "Status", C1.Win.C1TrueDBGrid.AlignHorzEnum.Near, C1.Win.C1TrueDBGrid.AlignVertEnum.Top, , , , , True)
                .ColDesign("Updated Date", 180, "Updated Date", C1.Win.C1TrueDBGrid.AlignHorzEnum.Near, C1.Win.C1TrueDBGrid.AlignVertEnum.Top, , , , , True, , , , , , "dd/MM/yyyy")

                .ColDesign("OH Code", -1, "OH Code", C1.Win.C1TrueDBGrid.AlignHorzEnum.Center)
                .ColDesign("OH Desc", -1, "OH Desc", C1.Win.C1TrueDBGrid.AlignHorzEnum.Near)
                .ColDesign("Remarks", -1, "Remarks", C1.Win.C1TrueDBGrid.AlignHorzEnum.Near)
                .ColDesign("Incl %", -1, "Incl %", AlignHorzEnum.Center, , , , , , , , , , , , , , , , PresentationEnum.CheckBox)
                .ColDesign("Fwd/Comp", -1, "Fwd/Comp", C1.Win.C1TrueDBGrid.AlignHorzEnum.Near)
                .ColDesign("OH Type", -1, "OH Type", C1.Win.C1TrueDBGrid.AlignHorzEnum.Near)
                .ColDesign("OH Item Type", -1, "OH Item Type", C1.Win.C1TrueDBGrid.AlignHorzEnum.Near)
                .ColDesign("Value", -1, "Amount", C1.Win.C1TrueDBGrid.AlignHorzEnum.Far, , , , , , , , , , , , "#,##0.000", , , , , , , "[Value] as [Amount]")
                .ColDesign("Curr", -1, "Curr", C1.Win.C1TrueDBGrid.AlignHorzEnum.Near)
                .ColDesign("Value LC", -1, "Amount " & gbCurr1, C1.Win.C1TrueDBGrid.AlignHorzEnum.Far, , , , , , , , , , , , "N" & VatLCRound, , , , , , , "[Value LC] as [Amount " & gbCurr1 & "]")
                .ColDesign("Value CC", -1, "Amount " & gbCurr2, C1.Win.C1TrueDBGrid.AlignHorzEnum.Far, , , , , , , , , , , , "#,##0.000", , , , , , , "[Value CC] as [Amount " & gbCurr2 & "]")

                .ColDesign("PERMANENT EXPENSES LC", -1, "PERMANENT EXPENSES " & gbCurr1, C1.Win.C1TrueDBGrid.AlignHorzEnum.Far, , , , , , , , , , , , "N" & VatLCRound, , , , , , , "PERMANENT EXPENSES LC] as [PERMANENT EXPENSES " & gbCurr1 & "]")
                .ColDesign("PERMANENT EXPENSES CC", -1, "PERMANENT EXPENSES " & gbCurr2, C1.Win.C1TrueDBGrid.AlignHorzEnum.Far, , , , , , , , , , , , "#,##0.000", , , , , , , "PERMANENT EXPENSES CC] as [PERMANENT EXPENSES " & gbCurr2 & "]")

                .ColDesign("Fwd LC", -1, "Forwarder Fees " & gbCurr1, C1.Win.C1TrueDBGrid.AlignHorzEnum.Far, , , , , , , , , , , , "N" & VatLCRound, , , , , , , "[Fwd LC] as [Forwarder Fees " & gbCurr1 & "]")
                .ColDesign("Fwd CC", -1, "Forwarder Fees " & gbCurr2, C1.Win.C1TrueDBGrid.AlignHorzEnum.Far, , , , , , , , , , , , "#,##0.000", , , , , , , "[Fwd CC] as [Forwarder Fees " & gbCurr2 & "]")

                .ColDesign("PTX LC", -1, "Compensation " & gbCurr1, C1.Win.C1TrueDBGrid.AlignHorzEnum.Far, , , , , , , , , , , , "N" & VatLCRound, , , , , , , "PTX LC] as [Compensation " & gbCurr1 & "]")
                .ColDesign("PTX CC", -1, "Compensation " & gbCurr2, C1.Win.C1TrueDBGrid.AlignHorzEnum.Far, , , , , , , , , , , , "#,##0.000", , , , , , , "PTX CC] as [Compensation " & gbCurr2 & "]")

                .ColDesign("TTX LC", -1, "Temporary Expenses " & gbCurr1, C1.Win.C1TrueDBGrid.AlignHorzEnum.Far, , , , , , , , , , , , "N" & VatLCRound, , , , , , , "TTX LC] as [Temporary Expenses " & gbCurr1 & "]")
                .ColDesign("TTX CC", -1, "Temporary Expenses " & gbCurr2, C1.Win.C1TrueDBGrid.AlignHorzEnum.Far, , , , , , , , , , , , , "#,##0.000", , , , , , "TTX CC] as [Temporary Expenses " & gbCurr2 & "]")

                .ColDesign("Invoice TOTALS LC", -1, "Invoice TOTALS " & gbCurr1, C1.Win.C1TrueDBGrid.AlignHorzEnum.Far, , , , , , , , , , , , "N" & VatLCRound, , , , , , , "Invoice TOTALS LC] as [Invoice TOTALS " & gbCurr1 & "]")
                .ColDesign("Invoice TOTALS CC", -1, "Invoice TOTALS " & gbCurr2, C1.Win.C1TrueDBGrid.AlignHorzEnum.Far, , , , , , , , , , , , "#,##0.000", , , , , , , "Invoice TOTALS CC] as [Invoice TOTALS " & gbCurr2 & "]")

                '.ColDesign("LC", -1, "LC", C1.Win.C1TrueDBGrid.AlignHorzEnum.Center, , , , , , True, False, , , , , , , , , , , False)
                '.ColDesign("CC", -1, "CC", C1.Win.C1TrueDBGrid.AlignHorzEnum.Center, , , , , , True, False, , , , , , , , , , , False)
                '.ColDesign("Updated", -1, "Updated", C1.Win.C1TrueDBGrid.AlignHorzEnum.Center, , , , , , True, False, , , , , , , , , , , False)
                '.ColDesign("Reversed", -1, "Reversed", C1.Win.C1TrueDBGrid.AlignHorzEnum.Center, , , , , , True, False, , , , , , , , , , , False)
                '.ColDesign("Counter", -1, "Counter", C1.Win.C1TrueDBGrid.AlignHorzEnum.Center, , , , , , True, False, , , , , , , , , , , False)

                strPrev = Nothing
                Dim sbr As frmPreviewGrid.SubReport
                Dim str As String
                str = "SELECT * FROM " & SubTempTable
                sbr = New frmPreviewGrid.SubReport("FwdTot")
                sbr.AddECParameter("@FwdTotStr", str)
                frm.InitSubReport(sbr)

                frm.ShowDialog(CType(strPrev, String(,)), frm1.Name, ReportName)
                '  PrintFISumDirectRep(frm, strsql, ReportName, SubTempTable)
            End With
        Catch exp As Exception
            Throw exp
        Finally
            If Not frm Is Nothing Then frm = Nothing
            Cursor.Current = Cursors.Default
            If Not drSql Is Nothing Then
                If drSql.IsClosed = False Then drSql.Close()
                drSql = Nothing
            End If
        End Try
    End Sub
    Public Sub PrintFISumDirectRep(ByVal frm As Form, ByVal SQLstat As String, ByVal ReportName As String, ByVal TempSubRep As String)
        Dim cnSql As SqlConnection
        Dim cmSql As SqlCommand
        Dim trnSql As SqlTransaction
        Dim strsql As String
        Dim TotNoUser As String
        Dim SelectVch As String
        Dim ViewName As String
        Try
            Cursor.Current = Cursors.WaitCursor
            ViewName = "VPP_" & frm.Name & "_" & gbUser

            cnSql = New SqlConnection(ConnectionString)
            cnSql.Open()

            trnSql = cnSql.BeginTransaction
            cmSql = New SqlCommand(strsql, cnSql, trnSql)

            strsql = "IF EXISTS (SELECT * FROM SysObjects WHERE Id = Object_Id(N'" & ViewName & "') "
            strsql &= " AND OBJECTPROPERTY(Id, N'Isview') = 1) DROP VIEW " & ViewName
            cmSql.CommandText = strsql
            cmSql.ExecuteNonQuery()

            'create view
            strsql = " CREATE VIEW " & ViewName & " AS " & SQLstat
            cmSql.CommandText = strsql
            cmSql.ExecuteNonQuery()
            trnSql.Commit()

            strsql = "SELECT * FROM " & ViewName
            strsql &= " ORDER BY [RecID] ASC"

            strPrev = Nothing
            Dim sbr As ModPrint.SubReport
            Dim str As String
            str = "SELECT * FROM " & TempSubRep
            sbr = New ModPrint.SubReport("FwdTot")
            sbr.AddECParameter("@FwdTotStr", str)
            ModPrint.InitSubReport(sbr)

            PreviewProcedure(strsql, "", ReportName, frm)
        Catch ex As Exception
            Throw ex
        Finally
            Cursor.Current = Cursors.Default
            cmSql = Nothing
            If Not cnSql Is Nothing Then
                If cnSql.State = ConnectionState.Open Then cnSql.Close()
                cnSql = Nothing
            End If
        End Try
    End Sub

    Public Sub FwdSumRepByCostElt(ByVal frm As Form, ByVal Cond As String, Optional ByVal PrintDirect As Boolean = False)
        Dim strsql As String
        'Dim drSQL As SqlDataReader
        Dim ReportName As String
        Try
            DropSqlObject("TempSumByCostElt_" & gbUser, "TABLE")

            strsql = "SELECT FWD_CODE AS [Fwd Code], FWD_NAME AS [Fwd Name],"
            strsql &= " '" & gbCurr1 & "' AS LC, '" & gbCurr2 & "' AS CC,"
            strsql &= " CASE WHEN FIH_NORREV = 'N' THEN 'Normal' ELSE 'Reverse' END AS [N/R],"
            strsql &= " (CASE WHEN FIH_NORREV = 'N' THEN SUM(FID_OHVALLC) ELSE 0 END) AS [Normal LC],"
            strsql &= " (CASE WHEN FIH_NORREV = 'R' THEN SUM(FID_OHVALLC) ELSE 0 END) AS [Reversed LC],"
            strsql &= " (CASE WHEN FIH_NORREV = 'N' THEN SUM(FID_OHVALCC) ELSE 0 END) AS [Normal CC],"
            strsql &= " (CASE WHEN FIH_NORREV = 'R' THEN SUM(FID_OHVALCC) ELSE 0 END) AS [Reversed CC]"
            strsql &= " INTO TempSumByCostElt_" & gbUser
            strsql &= " FROM STK04_FwdInvDet"
            strsql &= " INNER JOIN STK04_FwdInvHead On FID_HRECID = FIH_RECID"
            strsql &= " INNER JOIN STK04_Forwarder on FIH_FWDCODE = FWD_RECID"
            strsql &= " WHERE 1 = 1"
            'If UCase(Screen.ActiveForm.Name) <> "FRMPRINTSUM" THEN
            '    strsql &= " AND STK04_FwdInvHead.FIH_RECID  = " & Val(TxtRecid) & ""
            'END If
            If Cond <> "" Then strsql &= " " + Cond
            strsql &= " GROUP BY FWD_CODE, FWD_NAME, FIH_NORREV"
            'strsql &= " ORDER BY FWD_NAME ASC"
            Execute(strsql)

            strsql = "Select * FROM TempSumByCostElt_" & gbUser & " AS TempSumByCostElt"
            ' ReadFromDBToDataReader(strsql, drSQL)
            If Not RecordsExists("Select top 1 1 FROM TempSumByCostElt_" & gbUser) Then
                MessageBox.Show("No Records Found.", frm.Text, MessageBoxButtons.OK, MessageBoxIcon.Information)
                Exit Sub
            End If
            ReportName = "rptFwdInvSumByCost.rpt"

            If PrintDirect = True Then
                PrintFIDirectRepByCostElt(frm, strsql, ReportName)
            Else
                PrintFIWithPrevGridByCostElt(frm, strsql, ReportName)
            End If
        Catch ex As Exception
            Throw ex
        End Try
    End Sub
    Public Sub PrintFIWithPrevGridByCostElt(ByVal frm1 As Form, ByVal strsql As String, ByVal ReportName As String)
        Dim frm As frmPreviewGrid
        Dim drSql As SqlDataReader
        Try
            Cursor.Current = Cursors.WaitCursor
            frm = New frmPreviewGrid
            With frm
                frm.BuildSql(strsql)
                .ColDesign("Fwd Code", 60, "Fwd Code", AlignHorzEnum.Center, , , , , , , , frmPreviewGrid.Orders.Ascending)
                .ColDesign("Fwd Name", 120, "Fwd Name", AlignHorzEnum.Near)
                .ColDesign("LC", 40, gbCurr1, AlignHorzEnum.Near, , , , , , , False, , , , , , , , , , , False, " [CC] as " & gbCurr1)
                .ColDesign("CC", 40, gbCurr2, AlignHorzEnum.Near, , , , , , , False, , , , , , , , , , , False, " [CC] as " & gbCurr2)

                .ColDesign("N/R", 50, "N/R", AlignHorzEnum.Center)
                .ColDesign("Normal LC", 100, "Normal " & gbCurr1, AlignHorzEnum.Far, , , , , , , , , , , , "#,##0.000")
                .ColDesign("Reversed LC", 100, "Reversed " & gbCurr1, AlignHorzEnum.Far, , , , , , , , , , , , "#,##0.000")
                .ColDesign("Normal CC", 100, "Normal " & gbCurr2, AlignHorzEnum.Far, , , , , , , , , , , , "#,##0.000")
                .ColDesign("Reversed CC", 100, "Reversed " & gbCurr2, AlignHorzEnum.Far, , , , , , , , , , , , "#,##0.000")

                frm.ShowDialog(CType(strPrev, String(,)), frm1.Name, ReportName)
            End With
        Catch exp As Exception
            Throw exp
        Finally
            If Not frm Is Nothing Then frm = Nothing
            Cursor.Current = Cursors.Default
            If Not drSql Is Nothing Then
                If drSql.IsClosed = False Then drSql.Close()
                drSql = Nothing
            End If
        End Try
    End Sub
    Public Sub PrintFIDirectRepByCostElt(ByVal frm As Form, ByVal SQLstat As String, ByVal ReportName As String)
        Dim cnSql As SqlConnection
        Dim cmSql As SqlCommand
        Dim trnSql As SqlTransaction
        Dim strsql As String
        Dim TotNoUser As String
        Dim SelectVch As String
        Dim ViewName As String
        Try
            Cursor.Current = Cursors.WaitCursor
            ViewName = "VPP_" & frm.Name & "_" & gbUser

            cnSql = New SqlConnection(ConnectionString)
            cnSql.Open()

            trnSql = cnSql.BeginTransaction
            cmSql = New SqlCommand(strsql, cnSql, trnSql)

            strsql = "IF EXISTS (SELECT * FROM SysObjects WHERE Id = Object_Id(N'" & ViewName & "') "
            strsql &= " AND OBJECTPROPERTY(Id, N'Isview') = 1) DROP VIEW " & ViewName
            cmSql.CommandText = strsql
            cmSql.ExecuteNonQuery()

            'create view
            strsql = " CREATE VIEW " & ViewName & " AS " & SQLstat
            cmSql.CommandText = strsql
            cmSql.ExecuteNonQuery()
            trnSql.Commit()

            strsql = "SELECT * FROM " & ViewName
            strsql &= " ORDER BY [Fwd Name] ASC"

            PreviewProcedure(strsql, "", ReportName, frm)
        Catch ex As Exception
            Throw ex
        Finally
            Cursor.Current = Cursors.Default
            cmSql = Nothing
            If Not cnSql Is Nothing Then
                If cnSql.State = ConnectionState.Open Then cnSql.Close()
                cnSql = Nothing
            End If
        End Try
    End Sub

    Public Function FwdDetRep(ByVal frm As Form, ByVal Cond As String, ByVal RecId As Integer, ByVal TempSubRep As String, ByVal Reversed As Boolean, Optional ByVal PrintDirect As Boolean = False, Optional ByVal CondJoin As String = "") As Boolean
        'Fill the temporary table that will hold the Pur.Inv's and their values
        Dim strsql1, strsql2 As String
        Dim strSub As String
        '   Dim drSQL As SqlDataReader
        Dim dsSQL As DataSet
        Dim ReportName As String
        Try
            FwdDetRep = False

            strsql1 = "SELECT DISTINCT TOP 100 PERCENT STK04_Forwarder.FWD_COMP + ' - ' + Comp_Name AS [Company],"
            strsql1 &= " STK04_Forwarder.FWD_RECID AS [Fwd RecID], STK04_Forwarder.FWD_CODE + ' - ' + STK04_Forwarder.FWD_NAME AS [Forwarder],"
            strsql1 &= " FID_Recid AS [RecID], STK04_FwdInvHead.FIH_RECID AS [FIH RecID], FIH_FWDINV AS [Inv. No],"
            strsql1 &= " CASE WHEN FIH_NORREV = 'N' THEN 'Normal' ELSE 'Reverse' END AS [N/R],"
            strsql1 &= " FIH_DATE AS [Inv. Date], FIH_RECEIVED AS [Received On],"
            strsql1 &= " FIH_PFWDLC AS [Fwd LC], FIH_PFWDCC AS [Fwd CC],"
            strsql1 &= " FIH_PTXLC AS [PTX LC], FIH_PTXCC AS [PTX CC],"
            strsql1 &= " FIH_TTXLC AS [TTX LC], FIH_TTXCC AS [TTX CC],"
            strsql1 &= " FIH_LC AS [LC], FIH_CC AS [CC], FID_RKS AS [Remarks],"
            strsql1 &= " CASE WHEN FIH_STATUS = '1' THEN 'Not Confirmed'"
            strsql1 &= " WHEN FIH_STATUS = '2' and FIH_GenToAcc='Y' THEN 'Confirmed & A/C Generated' WHEN FIH_STATUS = '2' and FIH_GenToAcc='N' THEN 'Confirmed'  END AS [Status],"
            strsql1 &= " (CASE WHEN FIH_Status = '2' THEN 'True' ELSE 'False' END) AS [Updated],"
            strsql1 &= " FIH_UPDDATE AS [Updated Date], '" & Reversed & "' AS Reversed,"

            strsql1 &= " FID_RECID AS [FID RecId], OH_CODE AS [OH Code], OH_DESC AS [OH Desc], FID_COUNTER AS [Counter],"
            strsql1 &= " (CASE WHEN FID_INCLUDE='Y' THEN 'True' ELSE 'False' END) AS [Incl %],"
            strsql1 &= " CASE WHEN OH_CSTTYPE = 'F' THEN 'Fwd' ELSE 'Tax' END AS [Fwd/Tax],"
            strsql1 &= " OH_OHType AS [OH Type], OH_ItmType AS [OH Item Type],"
            strsql1 &= " FID_OHVALUE AS [Value], FID_CURR AS [ OH Curr],"
            strsql1 &= " FID_OHVALLC AS [Value LC],"
            strsql1 &= " FID_OHVALCC AS [Value CC],"

            strsql1 &= " TempPurInv.SupCode AS [Sup Code], TempPurInv.SupRef AS [Sup Ref],"
            strsql1 &= " TempPurInv.Curr AS [Curr],"
            strsql1 &= " TempPurInv.Value AS [Val LC]," ''Value in the purchase invoice currency 
            strsql1 &= " FPI_DCTVALCC AS [Direct Val CC],"
            strsql1 &= " FPI_PROVALCC AS [Prorated Val CC],"
            strsql1 &= " FIH_ADOCDATE00 AS [Doc Date]"

            strsql2 = " FROM STK04_FwdInvDet"
            strsql2 &= " INNER JOIN STK04_Overheads ON FID_OHRECID = OH_RECID"
            strsql2 &= " INNER JOIN STK04_FwdInvHead ON FID_HRECID = FIH_RECID"
            strsql2 &= " INNER JOIN STK04_Forwarder ON FWD_RECID = FIH_FWDCODE"
            strsql2 &= " INNER JOIN STK04_FwdPurInv ON FID_RECID = FPI_FWDRECID"
            strsql2 &= " INNER JOIN Temppurinv_" + gbUser + " TempPurInv ON FPI_SupCode = TempPurInv.SupCode"
            strsql2 &= " AND FPI_SupInv = TempPurInv.SupRef"
            strsql2 &= " INNER JOIN Com_Company ON FWD_COMP = Comp_Code"
            If CondJoin <> "" Then strsql2 &= CondJoin
            strsql2 &= " WHERE 1=1 "

            If Cond <> "" Then strsql2 &= " " + Cond
            strsql2 &= " ORDER BY STK04_FwdInvHead.FIH_RECID, FID_RECID"
            '    ReadFromDBToDataReader(strsql, drSQL)
            If Not RecordsExists("  select top 1 1 " & strsql2) Then
                MessageBox.Show("No Records Found.", frm.Text, MessageBoxButtons.OK, MessageBoxIcon.Information)
                FwdDetRep = True : Exit Function
            End If
            ReportName = "rptFwdInvDet.rpt"

            'Sub Report: PURINV 
            DropSqlObject("TempSubPurInv_" + gbUser, "TABLE")
            strSub = "SELECT TempPurInv.SupName, INV_INVDATE, TempPurInv.Curr, TempPurInv.Value, TempPurInv.ValueCC,"
            strSub &= " FPI_SupCode, FPI_SupInv, FPI_DCTVALCC, FPI_PROVALCC, FID_Hrecid , FID_INCLUDE"
            strSub &= " INTO TempSubPurInv_" + gbUser
            strSub &= " FROM (SELECT DISTINCT SupName,Curr,value,valuecc,SupCode,SupRef FROM TempPurInv_" + gbUser + ")  TempPurInv"
            strSub &= " INNER JOIN STK04_FwdPurInv"
            strSub &= " ON TempPurInv.SupCode = FPI_SupCode"
            strSub &= " AND TempPurInv.SupRef = FPI_SupInv "
            strSub &= " INNER JOIN STK04_FwdInvDet On FPI_FWDRECID = FID_RECID"
            strSub &= " LEFT JOIN STK04_InvHead On STK04_InvHead.INV_RECID = STK04_FwdPurInv.FPI_InvHeadId"
            If RecId <> 0 Then strSub &= " WHERE FID_HRecid = " & RecId
            Execute(strSub)

            If PrintDirect = True Then
                PrintFIDetDirectRep(frm, strsql1 + strsql2, ReportName, TempSubRep, "TempSubPurInv_" + gbUser)
            Else
                PrintFIDetWithPrevGrid(frm, strsql1 + strsql2, ReportName, TempSubRep, "TempSubPurInv_" + gbUser, Reversed, CondJoin)
            End If
        Catch ex As Exception
            Throw ex
        Finally
            DropSqlObject("TempPurInv_" & gbUser, "TABLE")
        End Try
    End Function
    Public Sub PrintFIDetWithPrevGrid(ByVal frm1 As Form, ByVal strsql As String, ByVal ReportName As String, ByVal TempSubRep As String, ByVal TempSubPurInv As String, ByVal Reversed As Boolean, ByVal Cond As String)
        Dim frm As frmPreviewGrid
        Dim strsqlPrev As String
        Try
            Cursor.Current = Cursors.WaitCursor
            frm = New frmPreviewGrid

            strsqlPrev = " SELECT DISTINCT TOP 100 PERCENT FIH_FWDINV AS [Inv. No], STK04_Forwarder.FWD_COMP + ' - ' + Comp_Name AS [Company],"
            strsqlPrev &= " STK04_Forwarder.FWD_RECID AS [Fwd RecID], STK04_Forwarder.FWD_CODE + ' - ' + STK04_Forwarder.FWD_NAME AS [Forwarder],"
            strsqlPrev &= " FID_Recid AS [RecID], STK04_FwdInvHead.FIH_RECID AS [Head RecID], "
            strsqlPrev &= " CASE WHEN FIH_NORREV = 'N' THEN 'Normal' ELSE 'Reverse' END AS [N/R],"
            strsqlPrev &= " FIH_DATE AS [Inv. Date], FIH_RECEIVED AS [Received On],"

            strsqlPrev &= " CASE WHEN FIH_STATUS = '1' THEN 'Not Confirmed'"
            strsqlPrev &= "  WHEN FIH_STATUS = '2' and FIH_GenToAcc='Y' THEN 'Confirmed & A/C Generated' WHEN FIH_STATUS = '2' and FIH_GenToAcc='N' THEN 'Confirmed' END AS [Status],"
            strsqlPrev &= " (CASE WHEN FIH_Status = '2' THEN 'True' ELSE 'False' END) AS [Updated],"
            strsqlPrev &= " FIH_UPDDATE AS [Updated Date], '" & Reversed & "' AS Reversed,"

            strsqlPrev &= " FID_RECID AS [FID RecId], OH_CODE AS [OH Code], OH_DESC AS [OH Desc]," ' FID_COUNTER AS [Counter]," 
            strsqlPrev &= " (CASE WHEN FID_INCLUDE='Y' THEN 'Per' ELSE 'Temp' END) AS [Per/Temp],"
            strsqlPrev &= " CASE WHEN OH_CSTTYPE = 'F' THEN 'Fwd' ELSE 'Tax' END AS [Fwd/Tax],"
            strsqlPrev &= " OH_OHType AS [OH Type], OH_ItmType AS [OH Item Type],"
            strsqlPrev &= " FID_OHVALUE AS [Value], FID_CURR AS [OH Curr],"
            strsqlPrev &= " TempPurInv.SupCode AS [Sup Code], TempPurInv.SupRef AS [Sup Ref],"
            strsqlPrev &= " (CASE WHEN FID_INCLUDE='Y' THEN 'Yes' ELSE 'No' END) AS [Incl %],"
            strsqlPrev &= " TempPurInv.Curr AS [Curr],"
            strsqlPrev &= " TempPurInv.Value AS [Val LC]," ''Value in the purchase invoice currency 
            strsqlPrev &= " case when  COALESCE(FPI_PROVALCC,0) > 0 then  (COALESCE(FPI_PROVALCC,0) *100)/COALESCE(FID_OHVALCC,0) Else 100 end [Share %],"
            strsqlPrev &= " COALESCE(FID_OHVALUE,0) * ((case when  COALESCE(FPI_PROVALCC,0) > 0 then  (COALESCE(FPI_PROVALCC,0) *100)/COALESCE(FID_OHVALCC,0) Else 100 end)/100) [Share Amount In OH],"
            strsqlPrev &= " case when COALESCE(FPI_PROVALCC,0) > 0 then COALESCE(FPI_PROVALCC,0) else COALESCE(FPI_DCTVALCC,0) end [C/V in CC],"


            strsqlPrev &= " FIH_PFWDLC +  FIH_PTXLC [PERMANENT EXPENSES LC], FIH_PFWDCC +  FIH_PTXCC [PERMANENT EXPENSES CC],"
            strsqlPrev &= " FIH_PFWDLC AS [Fwd LC], FIH_PFWDCC AS [Fwd CC] ,  FIH_PTXLC AS [PTX LC] , FIH_PTXCC AS [PTX CC] ,"
            strsqlPrev &= " FIH_TTXLC AS [TTX LC] ,  FIH_TTXCC AS [TTX CC] ,     "
            strsqlPrev &= " FIH_PFWDLC + FIH_PTXLC + FIH_TTXLC [Invoice TOTALS LC] , FIH_PFWDCC + FIH_PTXCC + FIH_TTXCC  [Invoice TOTALS CC] "

            'strsqlPrev &= " FIH_PFWDLC AS [Fwd LC], FIH_PFWDCC AS [Fwd CC],"
            'strsqlPrev &= " FIH_PTXLC AS [PTX LC], FIH_PTXCC AS [PTX CC],"
            'strsqlPrev &= " FIH_TTXLC AS [TTX LC], FIH_TTXCC AS [TTX CC],"
            'strsqlPrev &= " FIH_LC AS [LC], FIH_CC AS [CC],  "
            'strsqlPrev &= " FID_OHVALLC AS [Value LC],"
            'strsqlPrev &= " FID_OHVALCC AS [Value CC],"
            'strsqlPrev &= " FPI_DCTVALCC AS [Direct Val CC],"
            'strsqlPrev &= " FPI_PROVALCC AS [Prorated Val CC],"
            'strsqlPrev &= " FIH_ADOCDATE00 AS [Doc Date]"

            strsqlPrev &= " FROM STK04_FwdInvDet"
            strsqlPrev &= " INNER JOIN STK04_Overheads ON FID_OHRECID = OH_RECID"
            strsqlPrev &= " INNER JOIN STK04_FwdInvHead ON FID_HRECID = FIH_RECID"
            strsqlPrev &= " INNER JOIN STK04_Forwarder ON FWD_RECID = FIH_FWDCODE"
            strsqlPrev &= " INNER JOIN STK04_FwdPurInv ON FID_RECID = FPI_FWDRECID"
            strsqlPrev &= " INNER JOIN Temppurinv_" + gbUser + " TempPurInv ON FPI_SupCode = TempPurInv.SupCode"
            strsqlPrev &= " AND FPI_SupInv = TempPurInv.SupRef"
            strsqlPrev &= " INNER JOIN Com_Company ON FWD_COMP = Comp_Code"
            strsqlPrev &= " Left JOIN stk04_invhead ON STK04_InvHead.INV_RECID = STK04_FwdPurInv.FPI_InvHeadId"
            If Cond <> "" Then strsqlPrev &= " " + Cond
            strsqlPrev &= " WHERE 1=1 "



            With frm
                frm.BuildSql(strsqlPrev, , , , , strsql)
                .ColDesign("Inv. No", -1, "Inv. No", C1.Win.C1TrueDBGrid.AlignHorzEnum.Near, C1.Win.C1TrueDBGrid.AlignVertEnum.Top, , , , , True, , frmPreviewGrid.Orders.Ascending)
                .ColDesign("Company", -1, "Company", C1.Win.C1TrueDBGrid.AlignHorzEnum.Near, C1.Win.C1TrueDBGrid.AlignVertEnum.Top, , , , , True)
                .ColDesign("Fwd RecID", -1, "Fwd RecID", C1.Win.C1TrueDBGrid.AlignHorzEnum.Center, C1.Win.C1TrueDBGrid.AlignVertEnum.Top, , , , , True, False, , , , , , , , , , , False)
                .ColDesign("Forwarder", -1, "Forwarder", C1.Win.C1TrueDBGrid.AlignHorzEnum.Near, C1.Win.C1TrueDBGrid.AlignVertEnum.Top, , , , , True)
                .ColDesign("Head RecID", -1, "Head RecID", C1.Win.C1TrueDBGrid.AlignHorzEnum.Center, , , , , , True, False, , , , , , , , , , , False)
                .ColDesign("RecID", -1, "RecID", C1.Win.C1TrueDBGrid.AlignHorzEnum.Center, , , , , , , False, , , , , , , , , , , False)
                .ColDesign("N/R", 50, "N/R", AlignHorzEnum.Center)
                .ColDesign("Inv. Date", -1, "Inv. Date", C1.Win.C1TrueDBGrid.AlignHorzEnum.Near, C1.Win.C1TrueDBGrid.AlignVertEnum.Top, , , , , True, , , , , , "dd/MM/yyyy")
                .ColDesign("Received On", -1, "Received On", C1.Win.C1TrueDBGrid.AlignHorzEnum.Near, C1.Win.C1TrueDBGrid.AlignVertEnum.Top, , , , , True, , , , , , "dd/MM/yyyy")
                .ColDesign("Status", -1, "Status", C1.Win.C1TrueDBGrid.AlignHorzEnum.Near, C1.Win.C1TrueDBGrid.AlignVertEnum.Top, , , , , True)
                .ColDesign("Updated", 50, "Updated", AlignHorzEnum.Center, , , , , , , , , , , , , , , , PresentationEnum.CheckBox)
                .ColDesign("Updated Date", 180, "Updated Date", C1.Win.C1TrueDBGrid.AlignHorzEnum.Near, C1.Win.C1TrueDBGrid.AlignVertEnum.Top, , , , , True, , , , , , "dd/MM/yyyy")
                .ColDesign("Reversed", 50, "Reversed", AlignHorzEnum.Center, , , , , , , , , , , , , , , , PresentationEnum.CheckBox)

                .ColDesign("FID RecId", -1, "FID RecId", C1.Win.C1TrueDBGrid.AlignHorzEnum.Center, C1.Win.C1TrueDBGrid.AlignVertEnum.Top, , , , , True, False, , , , , , , , , , , False)
                .ColDesign("OH Code", -1, "OH Code", C1.Win.C1TrueDBGrid.AlignHorzEnum.Center)
                .ColDesign("OH Desc", -1, "OH Desc", C1.Win.C1TrueDBGrid.AlignHorzEnum.Near)
                .ColDesign("Per/Temp", -1, "Per/Temp", C1.Win.C1TrueDBGrid.AlignHorzEnum.Near)
                .ColDesign("Fwd/Tax", -1, "Fwd/Tax", C1.Win.C1TrueDBGrid.AlignHorzEnum.Near)
                .ColDesign("OH Type", -1, "OH Type", C1.Win.C1TrueDBGrid.AlignHorzEnum.Near)
                .ColDesign("OH Item Type", -1, "OH Item Type", C1.Win.C1TrueDBGrid.AlignHorzEnum.Near)

                .ColDesign("Value", 80, "Amount", AlignHorzEnum.Far, , , , , , , , , , , , , , , , , , , "[OH Value] as  [Amount]")
                .ColDesign("OH Curr", 50, "OH Curr", AlignHorzEnum.Center, , , , , , , , , , , , , , , , , , , "[OH Curr] as  [Curr]")

                .ColDesign("Sup Code", -1, "Sup Code", C1.Win.C1TrueDBGrid.AlignHorzEnum.Near)
                .ColDesign("Sup Ref", -1, "Sup Ref", C1.Win.C1TrueDBGrid.AlignHorzEnum.Near)
                .ColDesign("Incl %", -1, "Incl %", AlignHorzEnum.Center, , , , , , , , , , , , , , , , PresentationEnum.CheckBox)

                .ColDesign("Curr", -1, "Curr", AlignHorzEnum.Center)
                .ColDesign("Val LC", -1, "Total In OH Curr" & gbCurr1, AlignHorzEnum.Far, , , , , , , , , , , , "N" & VatLCRound, , , , , , , "[Val LC] as [Total In OH Curr" & gbCurr1 & "]")
                .ColDesign("Share %", -1, "Share %", C1.Win.C1TrueDBGrid.AlignHorzEnum.Far, , , , , , , , , , , , "#,##0.00")
                .ColDesign("Share Amount In OH", -1, "Share Amount In OH", C1.Win.C1TrueDBGrid.AlignHorzEnum.Far, , , , , , , , , , , , "#,##0")
                .ColDesign("C/V in CC", -1, "C/V in " & gbCurr2, C1.Win.C1TrueDBGrid.AlignHorzEnum.Far, , , , , , , , , , , , , , , , , , , "[C/V in CC] as [C/V in " & gbCurr2 & "]")

                .ColDesign("PERMANENT EXPENSES LC", -1, "PERMANENT EXPENSES " & gbCurr1, C1.Win.C1TrueDBGrid.AlignHorzEnum.Far, , , , , , , , , , , , "N" & VatLCRound, , , , , , , "PERMANENT EXPENSES LC] as [PERMANENT EXPENSES " & gbCurr1 & "]")
                .ColDesign("PERMANENT EXPENSES CC", -1, "PERMANENT EXPENSES " & gbCurr2, C1.Win.C1TrueDBGrid.AlignHorzEnum.Far, , , , , , , , , , , , "#,##0.000", , , , , , , "PERMANENT EXPENSES CC] as [PERMANENT EXPENSES " & gbCurr2 & "]")

                .ColDesign("Fwd LC", -1, "Forwarder Fees " & gbCurr1, C1.Win.C1TrueDBGrid.AlignHorzEnum.Far, , , , , , , , , , , , "N" & VatLCRound, , , , , , , "[Fwd LC] as [Forwarder Fees " & gbCurr1 & "]")
                .ColDesign("Fwd CC", -1, "Forwarder Fees " & gbCurr2, C1.Win.C1TrueDBGrid.AlignHorzEnum.Far, , , , , , , , , , , , "#,##0.000", , , , , , , "[Fwd CC] as [Forwarder Fees " & gbCurr2 & "]")

                .ColDesign("PTX LC", -1, "Compensation " & gbCurr1, C1.Win.C1TrueDBGrid.AlignHorzEnum.Far, , , , , , , , , , , , "N" & VatLCRound, , , , , , , "PTX LC] as [Compensation " & gbCurr1 & "]")
                .ColDesign("PTX CC", -1, "Compensation " & gbCurr2, C1.Win.C1TrueDBGrid.AlignHorzEnum.Far, , , , , , , , , , , , "#,##0.000", , , , , , , "PTX CC] as [Compensation " & gbCurr2 & "]")

                .ColDesign("TTX LC", -1, "Temporary Expenses " & gbCurr1, C1.Win.C1TrueDBGrid.AlignHorzEnum.Far, , , , , , , , , , , , "N" & VatLCRound, , , , , , , "TTX LC] as [Temporary Expenses " & gbCurr1 & "]")
                .ColDesign("TTX CC", -1, "Temporary Expenses " & gbCurr2, C1.Win.C1TrueDBGrid.AlignHorzEnum.Far, , , , , , , , , , , , "#,##0.000", , , , , , , "TTX CC] as [Temporary Expenses " & gbCurr2 & "]")

                .ColDesign("Invoice TOTALS LC", -1, "Invoice TOTALS " & gbCurr1, C1.Win.C1TrueDBGrid.AlignHorzEnum.Far, , , , , , , , , , , , "N" & VatLCRound, , , , , , , "Invoice TOTALS LC] as [Invoice TOTALS " & gbCurr1 & "]")
                .ColDesign("Invoice TOTALS CC", -1, "Invoice TOTALS " & gbCurr2, C1.Win.C1TrueDBGrid.AlignHorzEnum.Far, , , , , , , , , , , , "#,##0.000", , , , , , , "Invoice TOTALS CC] as [Invoice TOTALS " & gbCurr2 & "]")


                '.ColDesign("Fwd Inv No", 80, "Fwd Inv No", AlignHorzEnum.Near)
                '.ColDesign("N/R", 60, "N/R", AlignHorzEnum.Near)
                '.ColDesign("Inv Date", 80, "Fwd Inv Date", AlignHorzEnum.Near, , , , , , , , , , , , "dd/MM/yyyy")
                '.ColDesign("Received On", 80, "Received On", AlignHorzEnum.Near, , , , , , , , , , , , "dd/MM/yyyy")
                '.ColDesign("LC", 50, "LC", AlignHorzEnum.Center)
                '.ColDesign("CC", 50, "CC", AlignHorzEnum.Center)

                '.ColDesign("Fwd Val" & gbCurr1, 100, "Fwd Val" & gbCurr1, AlignHorzEnum.Far)
                '.ColDesign("Fwd Val" & gbCurr2, 100, "Fwd Val" & gbCurr2, AlignHorzEnum.Far)
                '.ColDesign("Comp Val" & gbCurr1, 100, "Comp Val" & gbCurr1, AlignHorzEnum.Far)
                '.ColDesign("Comp Val" & gbCurr2, 100, "Comp Val" & gbCurr2, AlignHorzEnum.Far)

                '.ColDesign("Status", 80, "Status", AlignHorzEnum.Near)
                '.ColDesign("Updated", 50, "Updated", AlignHorzEnum.Center, , , , , , , , , , , , , , , , PresentationEnum.CheckBox)
                '.ColDesign("Updated Date", 80, "Updated Date", AlignHorzEnum.Near, , , , , , , , , , , , "dd/MM/yyyy")
                '.ColDesign("Doc Date", 80, "Doc Date", AlignHorzEnum.Near, , , , , , , , , , , , "dd/MM/yyyy")
                '.ColDesign("OH Code", 80, "OH Code", AlignHorzEnum.Near)
                '.ColDesign("OH Desc", 100, "OH Desc", AlignHorzEnum.Near)
                '.ColDesign("Cost Type", 80, "Cost Type", AlignHorzEnum.Near)
                '.ColDesign("Fwd Name", 100, "Fwd Name", AlignHorzEnum.Near)
                '.ColDesign("OH Type", 80, "OH Type", AlignHorzEnum.Near)
                '.ColDesign("OH Item Type", 80, "OH Item Type", AlignHorzEnum.Near)



                strPrev = Nothing
                Dim sbr As frmPreviewGrid.SubReport
                Dim str As String
                str = "SELECT * FROM " & TempSubRep
                sbr = New frmPreviewGrid.SubReport("FwdTot")
                sbr.AddECParameter("@FwdTotStr", str)
                frm.InitSubReport(sbr)

                str = "SELECT * FROM " & TempSubPurInv
                sbr = New frmPreviewGrid.SubReport("PURINV")
                sbr.AddECParameter("@PurInvStr", str)
                frm.InitSubReport(sbr)

                frm.ShowDialog(CType(strPrev, String(,)), frm1.Name, ReportName)
            End With
        Catch exp As Exception
            Throw exp
        Finally
            If Not frm Is Nothing Then frm = Nothing
            Cursor.Current = Cursors.Default
        End Try
    End Sub
    Public Sub PrintFIDetDirectRep(ByVal frm As Form, ByVal SQLstat As String, ByVal ReportName As String, ByVal TempSubRep As String, ByVal TempSubPurInv As String)
        Dim cnSql As SqlConnection
        Dim cmSql As SqlCommand
        Dim trnSql As SqlTransaction
        Dim strsql As String
        Dim TotNoUser As String
        Dim SelectVch As String
        Dim ViewName As String
        Try
            Cursor.Current = Cursors.WaitCursor
            ViewName = "VPP_" & frm.Name & "_" & gbUser

            cnSql = New SqlConnection(ConnectionString)
            cnSql.Open()

            trnSql = cnSql.BeginTransaction
            cmSql = New SqlCommand(strsql, cnSql, trnSql)

            strsql = "IF EXISTS (SELECT * FROM SysObjects WHERE Id = Object_Id(N'" & ViewName & "') "
            strsql &= " AND OBJECTPROPERTY(Id, N'Isview') = 1) DROP VIEW " & ViewName
            cmSql.CommandText = strsql
            cmSql.ExecuteNonQuery()

            'create view
            strsql = " CREATE VIEW " & ViewName & " AS " & SQLstat
            cmSql.CommandText = strsql
            cmSql.ExecuteNonQuery()
            trnSql.Commit()

            strsql = "SELECT * FROM " & ViewName
            strsql &= " ORDER BY [RecID] ASC, [OH Code] ASC, [Counter] ASC"

            strPrev = Nothing
            Dim sbr As ModPrint.SubReport
            Dim str As String
            str = "SELECT * FROM " & TempSubRep
            sbr = New ModPrint.SubReport("FwdTot")
            sbr.AddECParameter("@FwdTotStr", str)
            ModPrint.InitSubReport(sbr)

            str = "SELECT * FROM " & TempSubPurInv
            sbr = New ModPrint.SubReport("PURINV")
            sbr.AddECParameter("@PurInvStr", str)
            ModPrint.InitSubReport(sbr)

            PreviewProcedure(strsql, "", ReportName, frm)
        Catch ex As Exception
            Throw ex
        Finally
            Cursor.Current = Cursors.Default
            cmSql = Nothing
            If Not cnSql Is Nothing Then
                If cnSql.State = ConnectionState.Open Then cnSql.Close()
                cnSql = Nothing
            End If
        End Try
    End Sub
    Private Function GetLastTrn(ByVal Year As String, ByVal cnSql As SqlConnection, ByVal trnSql As SqlTransaction, Optional _Company As String = "", Optional accServerName As String = "") As String
        Dim varx As String
        Dim drSQL As SqlDataReader
        Dim WLastNo As Long
        Dim Str As String
        Dim cmSQL As SqlCommand
        Try
            'varx = Format(gbComp, "00") + ACYear
            'varx = gbComp + ACYear
            varx = CStr(IIf(_Company <> "", _Company, gbComp)) + Year
            Str = "SELECT MAX(Trn_Subtrno) AS WlastNo FROM " & accServerName & "ACC01_AccTransaction"
            Str &= " WHERE Trn_SubTrno LIKE '" & varx & "%'"
            'ReadFROMDBToDataReader(Str, drSQL)
            cmSQL = New SqlCommand(Str, cnSql, trnSql)
            drSQL = cmSQL.ExecuteReader()
            If drSQL.HasRows Then
                drSQL.Read()
                If IsDBNull(drSQL.Item("WlastNo")) Then
                    WLastNo = 1
                Else
                    WLastNo = CLng(Val(Mid(drSQL.Item("WlastNo").ToString, 7, 9))) + 1
                End If
            End If
            varx = varx + Format(WLastNo, "000000000")
            GetLastTrn = varx
        Catch ex As Exception
            Throw ex
        Finally
            cmSQL = Nothing
            If Not drSQL Is Nothing Then
                If drSQL.IsClosed = False Then drSQL.Close()
                drSQL = Nothing
            End If
        End Try
    End Function
    'Public Sub Fwd_ValidFwdComp()
    '    Dim rs AS New Recordset
    '    With Screen.ActiveForm
    '        If UCase(.Name) <> "FRMREVFRINV" And UCase(.Name) <> "FRMFWDINV" And UCase(.Name) <> "FRMFWDINV2" THEN Exit Sub
    '        If .TxtCode.Text = "" Or .TXTCOM.Text = "" THEN Exit Sub
    '        strsql = "Select * FROM STK04_Forwarder where fwd_code='" & .TxtCode & "' and fwd_comp = '" & .TXTCOM & "'"
    '        CloseRec(rs, Nothing)
    '        rs.Open(strsql, Demain.ConSQL, adOpenStatic, adLockReadOnly)
    '        If rs.RecordCount = 0 THEN
    '            .LblFwd.Caption = "Not Found..."
    '            .lblfid.Caption = ""
    '            .TxtCode.Text = ""
    '            .TXTCOM.Text = ""
    '            .LblCompP.Caption = ""
    '        ELSE
    '            .LblFwd.Caption = rs.item("Fwd_Name")
    '            .LblFwd.Refresh()
    '            .lblfid.Caption = rs.item("fwd_recid")
    '        END If
    '    END With
    'END Sub

#End Region

#Region "Confirm Forwarder Invoice"

    'Added by Abir T. @ 17.06.2013
    Public Function FWD_ConfAcc(ByVal Recid As Integer, ByVal TempFwdInv As String, ByVal TempNonConfFI As String,
                                ByVal frmName As String, ByRef cmSQL As SqlCommand, ByRef cnSql As SqlConnection,
                                ByRef trnSql As SqlTransaction, ByVal NR As String, accServerName As String, Optional ByVal SkipAccStep As Boolean = False,
                                Optional ByVal OnlyGenToACc As Boolean = False, Optional ByRef noDOEACDef As Boolean = False,
                                Optional tmpTransactionsWoutFunds As String = "", Optional tmpTransactionsWoutFundsAccounts As String = "",
                                Optional FundDisbursementModuleIsActive As Boolean = False,
                                Optional VirtualOpeningVoucherDate As Object = Nothing) As Boolean

        Dim rsttemp As SqlDataReader
        Dim rstdetail As SqlDataReader
        Dim rstclone As SqlDataReader
        Dim drSQL As SqlDataReader
        Dim drSQL2 As SqlDataReader
        Dim Myset As SqlDataReader
        Dim j As Integer
        Dim Str As String
        Dim Reason As String
        Dim StrIns As String
        Dim v_Gtot As Double
        Dim V_LCTOT As Double
        Dim cmSql2 As SqlCommand
        Dim VoucherTypeCode As String
        Try
            FWD_ConfAcc = False

            If SkipAccStep = True Then
                strsql = "UPDATE STK04_FwdInvHEAD SET FIH_Status = 2, FIH_Upddate = '" & Format(Now, "MM/dd/yyyy") & "'"
                strsql &= " WHERE FIH_Recid =" & Val(Recid) & ""
                'Execute(strsql)
                cmSql2 = New SqlCommand(strsql, cnSql, trnSql)
                cmSql2.ExecuteNonQuery()
                Exit Try
            End If

            strsql = "SELECT FIH_Recid, FWD_COMP, Comp_Name, FWD_Code, FWD_Name,"
            strsql &= " FIH_FwdInv, FIH_NorRev, FIH_Date, FIH_Received,"
            strsql &= " (FIH_PFWDLC + FIH_PTXLC + FIH_TTXLC) AS TotLC,"
            strsql &= " (FIH_PFWDCC + FIH_PTXCC + FIH_TTXCC) AS TotCC,"
            strsql &= " CASE WHEN FIH_Status = 1 THEN 'False' ELSE 'True' END Status,"
            strsql &= " FIH_Upddate, '' AS Details, FWD_ACCNO,FWD_VATAccNO"
            strsql &= " FROM VFwdInv"
            strsql &= " LEFT JOIN Com_Company ON FWD_COMP = Comp_Code"
            strsql &= " WHERE FIH_Recid = " & Recid & ""
            'ReadFROMDBToDataReader(strsql, rsttemp)
            cmSQL.CommandText = strsql
            rsttemp = cmSQL.ExecuteReader()
            If Not rsttemp.HasRows Then Exit Function
            rsttemp.Read()

            '------- Part 1  : Verification Part

            '...Verify If Generation Module Defined
            Str = " SELECT TOP 1"
            Str &= " AG_Vtype"
            Str &= " FROM    ( SELECT    AG_Vtype"
            Str &= " FROM      STK04_AcGen"
            Str &= " WHERE     AG_Comp =  '" & rsttemp.Item("FWD_COMP").ToString & "' "
            Str &= " AND Ag_Gentyp <> 'D'  AND AG_Status = 'A' "
            Str &= " UNION"
            Str &= " SELECT    AGD_Vtype AG_Vtype"
            Str &= " FROM      stk04_Acgendet"
            Str &= "   INNER JOIN STK04_AcGen ON AG_Comp =AGD_Comp AND AG_Status = 'A'  "
            Str &= " WHERE     AGD_Comp = '" & rsttemp.Item("FWD_COMP").ToString & "'"
            Str &= " AND AGD_DocType1 = 'FI'"
            Str &= " ) T"

            'Str = "SELECT AG_Vtype FROM STK04_AcGen WHERE AG_Status = 'A'"
            'Str &= " AND AG_Comp='" & rsttemp.Item("FWD_COMP").ToString & "'"
            drSQL = Nothing
            cmSql2 = New SqlCommand(Str, cnSql, trnSql)
            drSQL = cmSql2.ExecuteReader

            'ReadFROMDBToDataReader(Str, drSQL)
            If Not drSQL.HasRows Then

                Reason = "Accounting Control Parameter In Stock A/C Generation Module Not Defined."
                StrIns = "INSERT INTO " & TempNonConfFI & " "
                StrIns &= " SELECT FIRecid ,Comp ,CompName ,FwdRecid, FwdCode,"
                StrIns &= " FwdName ,[Inv.No] ,[N/R] ,InvDate, '" & Reason & "'"
                StrIns &= " FROM " & TempFwdInv
                StrIns &= " WHERE FIRecid = " & Recid
                Execute(StrIns)
                FWD_ConfAcc = True : Exit Try
            End If

            Dim VType As String
            drSQL.Read()
            VType = drSQL.Item(0).ToString
            drSQL.Close()

            Str = ""
            Str = SELSPECIAL("EFR", rsttemp, NR, accServerName)
            drSQL = Nothing
            cmSql2 = New SqlCommand(Str, cnSql, trnSql)
            drSQL = cmSql2.ExecuteReader

            'ReadFROMDBToDataReader(Str, drSQL)
            If Not drSQL.HasRows Then
                Reason = "No Overhead Linked To This Forwarder Invoice."
                StrIns = "INSERT INTO " & TempNonConfFI & " "
                StrIns &= " SELECT FIRecid ,Comp ,CompName ,FwdRecid, FwdCode,"
                StrIns &= " FwdName ,[Inv.No] ,[N/R] ,InvDate, '" & Reason & "'"
                StrIns &= " FROM " & TempFwdInv
                StrIns &= " WHERE FIRecid = " & Recid
                Execute(StrIns)
                FWD_ConfAcc = True : Exit Try
            End If

            'Verify that the voucher type in table STK04_AcGen exist in accounting table
            'ACC01_VOUCHERTYPE For this Fwd Comp & Fwd Year 
            Str = SELSPECIAL("JVB", rsttemp, NR, accServerName, VType)
            cmSql2 = New SqlCommand(Str, cnSql, trnSql)
            rstdetail = cmSql2.ExecuteReader

            'ReadFROMDBToDataReader(Str, rstdetail)
            If Not rstdetail.HasRows Then
                Reason = "Invalid Voucher Type " & VType & "."
                StrIns = "INSERT INTO " & TempNonConfFI & " "
                StrIns &= " SELECT FIRecid ,Comp ,CompName ,FwdRecid, FwdCode,"
                StrIns &= " FwdName ,[Inv.No] ,[N/R] ,InvDate, '" & Reason & "'"
                StrIns &= " FROM " & TempFwdInv
                StrIns &= " WHERE FIRecid = " & Recid
                Execute(StrIns)
                FWD_ConfAcc = True : Exit Try
            End If




            Str = ""
            '...verify Forwarder A/C
            Str = SELSPECIAL("FR", rsttemp, NR, accServerName)
            cmSql2 = New SqlCommand(Str, cnSql, trnSql)
            drSQL2 = cmSql2.ExecuteReader

            'ReadFROMDBToDataReader(Str, drSQL2)
            If drSQL2.HasRows Then
                drSQL2.Read()
                Reason = "Invalid Account Number For The Forwarder " & rsttemp.Item("fwd_name").ToString
                Reason &= " In " & drSQL2.Item("fid_curr").ToString & ""
                StrIns = "INSERT INTO " & TempNonConfFI & " "
                StrIns &= " SELECT FIRecid ,Comp ,CompName ,FwdRecid, FwdCode,"
                StrIns &= " FwdName ,[Inv.No] ,[N/R] ,InvDate, '" & Reason.Replace("'", "''") & "'"
                StrIns &= " FROM " & TempFwdInv
                StrIns &= " WHERE FIRecid = " & Recid
                Execute(StrIns)
                FWD_ConfAcc = True : Exit Try
            End If
            drSQL2.Close()
            'drSQL2 = Nothing

            Str = ""
            '...verify OH A/C
            Str = SELSPECIAL("OV", rsttemp, NR, accServerName)
            cmSql2 = New SqlCommand(Str, cnSql, trnSql)
            drSQL2 = cmSql2.ExecuteReader

            'ReadFROMDBToDataReader(Str, drSQL2)
            If drSQL2.HasRows Then
                Reason = "Invalid Account Number For Overhead(s) " & vbCrLf
                While drSQL2.Read()
                    Reason &= " " & drSQL2.Item("OH_Desc").ToString.Replace("'", "''")
                    Reason &= " In " & drSQL2.Item("FID_Curr").ToString & vbCrLf
                End While

                StrIns = "INSERT INTO " & TempNonConfFI & " "
                StrIns &= " SELECT FIRecid ,Comp ,CompName ,FwdRecid, FwdCode,"
                StrIns &= " FwdName ,[Inv.No] ,[N/R] ,InvDate, '" & Reason & "'"
                StrIns &= " FROM " & TempFwdInv
                StrIns &= " WHERE FIRecid = " & Recid
                Execute(StrIns)
                FWD_ConfAcc = True : Exit Try
            End If


            rstdetail.Read()

            '...INSERT THE FORWARDER ACCOUNTS FIRST FOR EACH CURRENCY EXIST
            ReadSys("ACC01", rsttemp.Item("FWD_COMP").ToString, Year(CDate(rsttemp.Item("FIH_DATE"))).ToString, accServerName)

            '..get max trans date
            Dim LastNo As Integer

            strsql = " SELECT IsNULL(MAX(TRN_VnO1),0) FROM " & accServerName & "ACC01_ACCTRANSACTION"
            strsql &= " WHERE TRN_COMP='" & rsttemp.Item("FWD_COMP").ToString & "'"
            strsql &= " AND TRN_YEAR ='" & Year(CDate(rsttemp.Item("FIH_DATE"))) & "'"
            'strsql &= " AND Trn_VType = '" & rstdetail.Item("VT_CODE").ToString & "'"
            strsql &= " AND Trn_VType = '" & VType & "'"
            Myset = Nothing
            cmSql2 = New SqlCommand(strsql, cnSql, trnSql)
            Myset = cmSql2.ExecuteReader

            'ReadFROMDBToDataReader(Strsql, Myset)
            Myset.Read()
            If rstdetail.Item("VT_Auto").ToString = "1" Then
                If CDbl(rstdetail.Item("VT_LASTNO").ToString) < CDbl(Myset.Item(0).ToString) Then
                    Reason = "Exist In Acounting Transactions Number Greater Than The Used Number."
                    StrIns = "INSERT INTO " & TempNonConfFI & " "
                    StrIns &= " SELECT FIRecid ,Comp ,CompName ,FwdRecid, FwdCode,"
                    StrIns &= " FwdName ,[Inv.No] ,[N/R] ,InvDate, '" & Reason & "'"
                    StrIns &= " FROM " & TempFwdInv
                    StrIns &= " WHERE FIRecid = " & Recid
                    Execute(StrIns)
                    FWD_ConfAcc = True : Exit Try
                Else
                    LastNo = CInt(rstdetail.Item("VT_LASTNO"))
                End If
            Else
                LastNo = CInt(Myset.Item(0)) + 1
            End If
            Myset.Close()
            Myset = Nothing

            '--------- Part 2  : Start generation to Accounting  
            'Insert into table ACC01_AccTransaction
            v_Gtot = 0
            V_LCTOT = 0
            'For j = 1 To drSQL.RecordCount

            While drSQL.Read
                strsql = "EXEC " & accServerName & "AddACC01TRANS '" & rsttemp.Item("FWD_COMP").ToString & "',"
                strsql &= " '" & Year(CDate(rsttemp.Item("FIH_DATE"))) & "',"
                strsql &= " '" & Mid(rsttemp.Item("FWD_ACCNO").ToString, 3, 6) & "',"
                strsql &= " '" & Trim(Mid(rsttemp.Item("FWD_ACCNO").ToString, 9, 4)) & "',"
                strsql &= " '" & Mid(rsttemp.Item("FWD_ACCNO").ToString, 13, 1) & "',"
                strsql &= " '" & drSQL.Item("FID_CURR").ToString & "',"
                strsql &= " '" & Format(rsttemp.Item("FIH_Date"), "MM/dd/yyyy") & "',"
                strsql &= " '" & rstdetail.Item("VT_CODE").ToString & "',"
                'strsql &= " " & rstdetail.Item("VT_LASTNO").ToString & ", 0,"
                strsql &= " " & LastNo & ", 0,"

                '...SELECT MAX ENTRY UNDER SEVERAL CONDITIONS
                Str = " SPSELECT 'MENT',0,'" & rsttemp.Item("FWD_COMP").ToString & "',"
                Str &= " '" & Year(CDate(rsttemp.Item("FIH_DATE"))) & "', 'N',"
                Str &= " '" & rstdetail.Item("VT_CODE").ToString & "',"
                'Str &= "  '" & rstdetail.Item("VT_LASTNO").ToString & "',0,"
                Str &= " '" & LastNo & "', 0,"
                Str &= " '" & Format(rsttemp.Item("FIH_DATE"), "MM/dd/yyyy") & "'"
                'Rita M @ 23.02.2016. Add link to accounting server
                Str &= " , '" & accServerName & "'"
                rstclone = Nothing
                'ReadFROMDBToDataReader(Str, rstclone)
                cmSql2 = New SqlCommand(Str, cnSql, trnSql)
                rstclone = cmSql2.ExecuteReader()
                rstclone.Read()

                strsql &= " " & rstclone.Item(0).ToString & ","
                strsql &= " 'N',"
                strsql &= " 'Forwarder invoice " & rsttemp.Item("FIH_FwdInv").ToString & "',"
                'If rsttemp.Item("FIH_NoRrev").ToString = "N" THEN
                If CDbl(drSQL.Item(0)) < 0 Then strsql = strsql & "'D', " Else strsql = strsql & "'C', "
                'ELSE
                '    If CDbl(drSQL.Item(0)) < 0 THEN strsql = strsql & "'C', " ELSE strsql = strsql & "'D', "
                'END If
                If rsttemp.Item("FIH_NoRrev").ToString.ToUpper = "R" Then
                    strsql &= " " & Math.Abs(CDbl(drSQL.Item(0))) * -1 & ","
                Else
                    strsql &= " " & Math.Abs(CDbl(drSQL.Item(0))) & ","
                End If


                strsql &= " 'ACC01','" & gbCurr1 & "',"
                strsql &= " '" & gbCurr2 & "',"
                strsql &= " '0',"
                strsql &= " 'S00',"
                strsql &= " '0',"

                'strsql &= " '" & GetLastTrn() & "',"
                strsql &= " '" & GetLastTrn(CStr(Year(CDate(rsttemp.Item("FIH_DATE")))), cnSql, trnSql, rsttemp.Item("FWD_COMP").ToString, accServerName) & "',"
                'strsql &= " '" & gbUser & "',"
                strsql &= " N'" & DateStamp() & "', "
                strsql &= " 'DBL','" & rsttemp.Item("fih_fwdinv").ToString & "',"
                strsql &= " '" & Mid(rsttemp.Item("FWD_ACCNO").ToString, 3, 11) + drSQL.Item("FID_CURR").ToString & "',0,0"
                If rsttemp.Item("FIH_NoRrev").ToString.ToUpper = "R" Then
                    strsql &= " , " & Math.Abs(CDbl(drSQL.Item("FID_OHVALLC"))) * -1 & "," & Math.Abs(CDbl(drSQL.Item("FID_OHVALCC"))) * -1
                Else
                    strsql &= " , " & Math.Abs(CDbl(drSQL.Item("FID_OHVALLC"))) & "," & Math.Abs(CDbl(drSQL.Item("FID_OHVALCC")))
                End If

                'to get the total of forwarders accounts
                v_Gtot = v_Gtot + convert5(CDbl(drSQL.Item(0)), drSQL.Item("FID_CURR").ToString, gbCurr2, CDate(rsttemp.Item("FIH_DATE")))
                V_LCTOT = V_LCTOT + convert5(CDbl(drSQL.Item(0)), drSQL.Item("FID_CURR").ToString, gbCurr1, CDate(rsttemp.Item("FIH_DATE")))
                'Execute(strsql)
                cmSql2 = New SqlCommand(strsql, cnSql, trnSql)
                cmSql2.ExecuteNonQuery()

                rstclone.Close()
            End While

            '-------Part 3 : Start Generation of Overheads FROM table STK04_FwdInvDet 
            strsql = "EXECUTE GenerateACCDetailFrw " & rsttemp.Item("fih_recid").ToString & ","
            strsql &= "'" & rsttemp.Item("FWD_COMP").ToString & "',"
            strsql &= "'" & Year(CDate(rsttemp.Item("FIH_DATE"))) & "', '" & rsttemp.Item("FIH_NoRrev").ToString & "',"
            strsql &= "'" & Format(rsttemp.Item("FIH_DATE"), "MM/dd/yyyy") & "',"
            strsql &= "'" & rstdetail.Item("VT_CODE").ToString & "',"
            'strsql &= "" & rstdetail.Item("VT_LASTNO").ToString & ","
            strsql &= " " & LastNo & ","
            strsql &= " '" & gbCurr1 & "','" & gbCurr2 & "', '" & rsttemp.Item("fih_fwdinv").ToString & "',"
            strsql &= " N'" & DateStamp() & "',"
            'If rsttemp.Item("fih_norrev").ToString = "N" THEN
            If v_Gtot < 0 Then strsql &= "'C'," Else strsql &= "'D',"
            'ELSE
            'If v_Gtot < 0 THEN strsql &= "'D'," ELSE strsql &= "'C',"
            'END If
            strsql = strsql & Math.Abs(v_Gtot) & "," & Math.Abs(V_LCTOT) & ""
            'Rita M @ 23.02.2016. Add link to accounting server
            strsql = strsql & ",'" & accServerName & "'"

            'Execute(strsql)
            cmSql2 = New SqlCommand(strsql, cnSql, trnSql)
            cmSql2.ExecuteNonQuery()


            'Generate DOE entry
            strsql = " SELECT SUM(Trn_amtlc * CASE WHEN trn_dc='D' THEN 1 ELSE -1 end), "
            strsql &= " 	SUM(trn_amtcc * CASE WHEN trn_dc='D' THEN 1 ELSE -1 end) FROM dbo.ACC01_AccTransaction WHERE trn_comp='" & rsttemp.Item("FWD_COMP").ToString & "' "
            strsql &= " And Trn_Year='" & Year(CDate(rsttemp.Item("FIH_DATE"))) & "' "
            strsql &= " AND Trn_vtype='" & rstdetail.Item("VT_CODE").ToString & "' AND Trn_vno1= " & LastNo & " and trn_vno2 = 0 and Trn_nr = 'N'"
            strsql &= " HAVING SUM(Trn_amtlc * CASE WHEN trn_dc='D' THEN 1 ELSE -1 end) <> 0  "
            strsql &= " 	OR SUM(trn_amtcc * CASE WHEN trn_dc='D' THEN 1 ELSE -1 end)<>0"
            If RecordsExists(strsql, cmSql2) Then
                'Check if DOE account if defined in location accounting directives for location 00
                strsql = "SELECT TOP 1 1 FROM dbo.STK04_LocAcDir WHERE DL_Loc='00' AND DL_DOEAC<>''"
                If Not RecordsExists(strsql, cmSql2) Then
                    trnSql.Rollback()
                    MessageBox.Show("DOE Account is Not Defined In Accounting Generation in Location 00. Generation is Aborted", "Forwarder Generation", MessageBoxButtons.OK, MessageBoxIcon.Exclamation)
                    noDOEACDef = True : Exit Try
                End If
            End If


            strsql = "exec P_MAG_DOEACC01 '00','" & rsttemp.Item("FWD_COMP").ToString & "','" & Year(CDate(rsttemp.Item("FIH_DATE"))) & "',"
            strsql = strsql + " '" & CDate(rsttemp.Item("FIH_DATE")).ToString("MM/dd/yyyy") & "','" & rstdetail.Item("VT_CODE").ToString & "'," & LastNo & ",0,'" & rsttemp.Item("FIH_NoRrev").ToString & "'"
            strsql = strsql + ",'" & gbCurr1 & "','" & gbCurr2 & "'  ,'" & gbUser & "','" & accServerName & "','" & DateStamp() & "','" & gbDBCompYear & "' " ' , 'Y'"
            cmSql2.CommandText = strsql
            cmSql2.ExecuteNonQuery()

            If FundDisbursementModuleIsActive AndAlso CDate(rsttemp.Item("FIH_DATE")) >= CDate(VirtualOpeningVoucherDate) Then

                strsql = $" DELETE FROM {accServerName}{tmpTransactionsWoutFunds} "

                cmSql2.CommandText = strsql
                cmSql2.ExecuteNonQuery()

                strsql = $" INSERT INTO {accServerName}{tmpTransactionsWoutFunds} 
                                        SELECT  trn_RecId
                                        FROM    {accServerName}ACC01_AccTransaction
                                        WHERE [Trn_Comp]  = '{rsttemp.Item("FWD_COMP").ToString}'
                                          AND [Trn_Year]  = '{Year(CDate(rsttemp.Item("FIH_DATE")))}' 
                                          AND [Trn_date]  = '{CDate(rsttemp.Item("FIH_DATE")).ToString("MM/dd/yyyy")}' 
                                          AND [Trn_vtype] = '{rstdetail.Item("VT_CODE").ToString}'
                                          AND [Trn_vno1]  = {LastNo}
                                          AND [Trn_vno2]  = 0
                                          AND [Trn_nr]    = '{rsttemp.Item("FIH_NoRrev").ToString}'
                                          AND NOT EXISTS ( SELECT TOP 1
                                                               1
                                                           FROM   {accServerName}ACC01_FundTransaction
                                                           WHERE  ACC01_FundTransaction.FTR_TrnRecID = ACC01_AccTransaction.trn_RecId ) "

                cmSql2.CommandText = strsql
                cmSql2.ExecuteNonQuery()

                strsql = $" DELETE FROM {accServerName}{tmpTransactionsWoutFundsAccounts} "

                cmSql2.CommandText = strsql
                cmSql2.ExecuteNonQuery()

                strsql = $" INSERT INTO {accServerName}{tmpTransactionsWoutFundsAccounts}
                                        SELECT  DISTINCT
                                                Trn_Comp ,
                                                Trn_Year ,
                                                Trn_accno1 ,
                                                Trn_accno2 ,
                                                Trn_subno ,
                                                Trn_curr ,
                                                Trn_date
                                        FROM    {accServerName}ACC01_AccTransaction
                                                INNER JOIN {accServerName}{tmpTransactionsWoutFunds} ON trn_RecId = TransactionRecID
                                        WHERE   Trn_Comp = '{rsttemp.Item("FWD_COMP").ToString}'
                                                AND Trn_Year = '{Year(CDate(rsttemp.Item("FIH_DATE")))}' "

                cmSql2.CommandText = strsql
                cmSql2.ExecuteNonQuery()

                strsql = $" EXEC {accServerName}SP_FD_AccountsTableOrViewTemplateStatus  '{rsttemp.Item("FWD_COMP").ToString}', '{Year(CDate(rsttemp.Item("FIH_DATE")))}' , '{tmpTransactionsWoutFundsAccounts}'"

                Using dtExceptionsAcc As New DataTable

                    cmSql2.CommandText = strsql
                    Using daExceptionsAcc As New SqlDataAdapter(cmSql2)
                        daExceptionsAcc.Fill(dtExceptionsAcc)
                    End Using

                    If dtExceptionsAcc.Select(" [Tmp_TemplateIsNotDefined] = 'True' OR [Tmp_RateIsNotDefined] = 'True' ").Count > 0 Then

                        Dim strExceptionsACC As String = ""

                        For Each rw In dtExceptionsAcc.Select(" [Tmp_TemplateIsNotDefined] = 'True' OR [Tmp_RateIsNotDefined] = 'True' ")

                            strExceptionsACC &= $"- Account : {rw("tmp_AccountNo1").ToString} {rw("tmp_AccountNo2").ToString} {rw("tmp_AccountSubNo").ToString} {rw("tmp_AccountCurrency").ToString} --> "

                            If rw("Tmp_TemplateIsNotDefined").ToString = "True" Then
                                strExceptionsACC &= $"Template"
                            ElseIf rw("Tmp_RateIsNotDefined").ToString = "True" Then
                                strExceptionsACC &= $"Rate"
                            End If

                            strExceptionsACC &= $" Is Not Defined, Date : {CDate(rw("Tmp_TransactionDate")).ToString("dd/MM/yyyy")} " & vbCrLf

                        Next

                        Throw New FundsException(strExceptionsACC)

                    End If

                End Using

                strsql = $"INSERT INTO {accServerName}ACC01_FundTransaction 
                                       EXEC {accServerName}SP_FD_AutoDisburse '{rsttemp.Item("FWD_COMP").ToString}', '{Year(CDate(rsttemp.Item("FIH_DATE")))}', '{tmpTransactionsWoutFunds}' "

                cmSql2.CommandText = strsql
                cmSql2.ExecuteNonQuery()

                strsql = $" EXEC {accServerName}SP_FD_BalancingFunds '{rsttemp.Item("FWD_COMP").ToString}',
                                                                      '{Year(CDate(rsttemp.Item("FIH_DATE")))}', 
                                                                      '{CDate(rsttemp.Item("FIH_DATE")).ToString("MM/dd/yyyy")}',
                                                                      '{rstdetail.Item("VT_CODE").ToString}' ,
                                                                      '{LastNo}' ,
                                                                      '0' ,
                                                                      '{rsttemp.Item("FIH_NoRrev").ToString}' "

                Try

                    cmSql2.CommandText = strsql
                    cmSql2.ExecuteNonQuery()

                Catch ex As Exception
                    Throw New FundsException(ex.Message)
                End Try

            End If

            'If the voucher type is incremental , INSERT INTO VOUCHER TYPE GET LAST +1 
            If rstdetail.Item("VT_Auto").ToString = "1" Then
                strsql = " SPSELECT 'INSLNO'," & CDbl(rstdetail.Item("VT_LASTNO")) + 1 & ","
                strsql &= "'" & rsttemp.Item("FWD_COMP").ToString & "',"
                strsql &= "'" & Year(CDate(rsttemp.Item("FIH_DATE"))) & "', '" & NR & "',"
                strsql &= "'" & rstdetail.Item("VT_CODE").ToString & "','', '',null,'" & accServerName & "'"

                'Execute(strsql)
                cmSql2 = New SqlCommand(strsql, cnSql, trnSql)
                cmSql2.ExecuteNonQuery()
            End If
            If OnlyGenToACc Then
                strsql = "UPDATE STK04_FwdInvHEAD SET FIH_GenToAcc = 'Y'"
                strsql &= " WHERE FIH_Recid =" & Val(Recid) & ""
                'Execute(strsql)
                cmSql2 = New SqlCommand(strsql, cnSql, trnSql)
                cmSql2.ExecuteNonQuery()
            Else
                strsql = "UPDATE STK04_FwdInvHEAD SET FIH_Status = 2, FIH_Upddate = '" & Format(Now, "MM/dd/yyyy") & "'"
                strsql &= " WHERE FIH_Recid =" & Val(Recid) & ""
                'Execute(strsql)
                cmSql2 = New SqlCommand(strsql, cnSql, trnSql)
                cmSql2.ExecuteNonQuery()
            End If


            ReadSys(gbApp, gbComp, gbYear)

        Catch ex As Exception
            FWD_ConfAcc = True
            Throw ex
        Finally
            cmSql2 = Nothing
            If Not drSQL Is Nothing Then
                If drSQL.IsClosed = False Then drSQL.Close()
                drSQL = Nothing
            End If
            If Not drSQL2 Is Nothing Then
                If drSQL2.IsClosed = False Then drSQL2.Close()
                drSQL2 = Nothing
            End If
            If Not rsttemp Is Nothing Then
                If rsttemp.IsClosed = False Then rsttemp.Close()
                rsttemp = Nothing
            End If
            If Not rstdetail Is Nothing Then
                If rstdetail.IsClosed = False Then rstdetail.Close()
                rstdetail = Nothing
            End If
            If Not Myset Is Nothing Then
                If Myset.IsClosed = False Then Myset.Close()
                Myset = Nothing
            End If
            If Not rstclone Is Nothing Then
                If rstclone.IsClosed = False Then rstclone.Close()
                rstclone = Nothing
            End If
            'DisposecontrolsArrays(, cmSql2, , , , , drSQL, drSQL2, rstclone, rstdetail, rsttemp)
        End Try
    End Function

    Public Function SELSPECIAL(ByVal Type As String, ByVal rsttemp As SqlDataReader, ByVal NR As String, accServerName As String, Optional ByVal VTCOde As String = "") As String
        Try
            SELSPECIAL = " SPSELECT '" & Type & "'," & rsttemp.Item("FIH_RECID").ToString & ""
            SELSPECIAL &= " , '" & rsttemp.Item("FWD_COMP").ToString & "' ,"
            SELSPECIAL &= " '" & Year(CDate(rsttemp.Item("FIH_DATE"))) & "', '" & NR & "',"
            SELSPECIAL &= " '" & Mid(rsttemp.Item("FWD_ACCNO").ToString, 3, 6) & "' ,"
            SELSPECIAL &= " '" & Mid(rsttemp.Item("FWD_ACCNO").ToString, 9, 4) & "' ,"
            SELSPECIAL &= " '" & Mid(rsttemp.Item("FWD_ACCNO").ToString, 13, 1) & "',null, '" & accServerName & "','" & VTCOde & "'"
        Catch ex As Exception
            Throw ex
        End Try
    End Function
    'Public Function SELSPECIALVAT(ByVal Type As String, ByVal rsttemp As SqlDataReader, ByVal NR As String, accServerName As String) As String
    '    Try
    '        SELSPECIALVAT = " SPSELECT '" & Type & "'," & rsttemp.Item("FIH_RECID").ToString & ""
    '        SELSPECIALVAT &= " , '" & rsttemp.Item("FWD_COMP").ToString & "' ,"
    '        SELSPECIALVAT &= " '" & Year(CDate(rsttemp.Item("FIH_DATE"))) & "', '" & NR & "',"
    '        SELSPECIALVAT &= " '" & Mid(rsttemp.Item("FWD_VATAccNO").ToString, 3, 6) & "' ,"
    '        SELSPECIALVAT &= " '" & Mid(rsttemp.Item("FWD_VATAccNO").ToString, 9, 4) & "' ,"
    '        SELSPECIALVAT &= " '" & Mid(rsttemp.Item("FWD_VATAccNO").ToString, 13, 1) & "',null, '" & accServerName & "'"
    '    Catch ex As Exception
    '        Throw ex
    '    End Try
    'End Function
#End Region

    'Added by Abir T. @ 14.08.2013
    Public Sub StockColorList(ByVal TxtSTKRecid As TextBox, ByVal TxtColor As TextBox, ByVal FrmOwner As Form, Optional ByVal Cond As String = "", Optional ByVal TestActive As Boolean = False)
        Dim Str As String
        Dim ColorChecked As Boolean
        Dim SupColChecked As Boolean
        Dim Color As String
        Dim SupColor As String
        Dim dsColor As DataSet
        Dim drColor As SqlDataReader
        Dim Recid As String
        Try
            ColorChecked = False
            Color = ""

            Cursor.Current = Cursors.WaitCursor
            HosDLL.HOSDLLModvar.ShowStockList(gbUser, ConnectionString, gbApp, HosDLL.HOSDLLModvar.StockListTypes.StockList, False, True, True, "", True, Today.Date, gbComp, gbCurr1, gbCurr2, gbYear, gbFashion, gbExpiry, gbTrading, gbBook, False, False, HosDLL.HOSDLLModvar.Access.None, Cond) ', "Stock List") 'modified by GS on 08/07/16 (add Title param)

            If Not HosDLL.HOSDLLModvar.RetreivedRows Is Nothing Then
                Recid = HosDLL.HOSDLLModvar.RetreivedRows.Rows(0)(HosDLL.HOSDLLModvar.ReturnFieldAlias(ModVar.CurrItmKey, ModVar.CurrItmKeyDesc, HosDLL.HOSDLLModvar.Fields.STK04_STKFIL_STK_RECID)).ToString

                'Test if the color is selected in the stock search
                strsql = " SELECT * FROM dbo.STK04_UserStockList"
                strsql &= " WHERE USL_user='" & gbUser & "' AND USL_Type='S' AND USL_Column='[Color_Code]'"
                If RecordsExists(strsql) Then ColorChecked = True Else ColorChecked = False

                'Test if the Supplier Color is selected in the stock search
                strsql = " SELECT * FROM dbo.STK04_UserStockList"
                strsql &= " WHERE USL_user='" & gbUser & "' AND USL_Type='S' AND USL_Column='SRef_SupColor'"
                If RecordsExists(strsql) Then SupColChecked = True Else SupColChecked = False

                TxtSTKRecid.Text = Recid
                If ColorChecked = True Then
                    Color = HosDLL.HOSDLLModvar.RetreivedRows.Rows(0)(HosDLL.HOSDLLModvar.ReturnFieldAlias(ModVar.CurrItmKey, ModVar.CurrItmKeyDesc, HosDLL.HOSDLLModvar.Fields.stk04_stkqty_STQ_Color)).ToString
                    'If Color <> "" And TestActive Then
                    '    Str = "SELECT * FROM stk04_stkqty WHERE STQ_STKRECID = " & Recid
                    '    Str &= "AND STQ_Color = '" & Color & "' AND STQ_LocCode = '00'"
                    '    If Not RecordsExists(Str) Then
                    '        TxtSTKRecid.Text = "" : Color = "" : Exit Sub
                    '        'MessageBox.Show("This Stock Item Does Not Have The Selected Color.", _
                    '        '            FrmOwner.Text, MessageBoxButtons.OK, MessageBoxIcon.Information)
                    '    End If
                    'End If
                End If
                If SupColChecked = True Then
                    'Get the selected Supplier Color  --> Fill it in the color
                    SupColor = HosDLL.HOSDLLModvar.RetreivedRows.Rows(0)(HosDLL.HOSDLLModvar.ReturnFieldAlias(ModVar.CurrItmKey, ModVar.CurrItmKeyDesc, HosDLL.HOSDLLModvar.Fields.STK04_StkSupRef_SRef_SupColor)).ToString
                End If

                If Color = "" And SupColor <> "" Then
                    Str = "SELECT Color_Code, Color_Reference FROM Stk04_COLOR WHERE Color_Active = 'Y'"
                    Str &= " AND Color_Code = '" & SupColor & "'"
                    ReadFromDBToDataReader(Str, drColor)
                    If drColor.HasRows Then
                        drColor.Read() : Color = drColor.Item(0).ToString
                    Else
                        drColor.Close()
                        Str = "SELECT Color_Code, Color_Reference FROM Stk04_COLOR WHERE Color_Active = 'Y'"
                        Str &= " AND Color_Reference = '" & SupColor & "'"
                        ReadFromDBToDataReader(Str, drColor)
                        If drColor.HasRows Then
                            drColor.Read() : Color = drColor.Item(0).ToString
                        Else
                            drColor.Close()
                            Str = "SELECT Color_Code, Color_Reference FROM Stk04_COLOR WHERE Color_Active = 'Y'"
                            Str &= " AND Color_Reference LIKE '%" & SupColor & "%'"
                            ReadFromDBToDataReader(Str, drColor)
                            If drColor.HasRows Then
                                drColor.Read() : Color = drColor.Item(0).ToString
                            End If
                        End If
                    End If
                    drColor.Close()
                End If

                If Color = "" And Val(TxtSTKRecid.Text) <> 0 Then
                    strsql = "SELECT STQ_Color FROM STK04_STKFIL"
                    strsql &= " LEFT JOIN STK04_STKQTY ON STK_RECID = STQ_STKRECID AND STQ_LOCCODE = '00'"
                    strsql &= " WHERE STK_Recid = " & TxtSTKRecid.Text
                    ReadFromDBToDataSet(strsql, "tbColor", dsColor)
                    If dsColor.Tables(0).Rows.Count = 1 Then Color = dsColor.Tables(0).Rows(0)(0).ToString
                End If

                TxtColor.Text = Color
            End If
        Catch ex As Exception
            Throw ex
        Finally
            Cursor.Current = Cursors.Default
        End Try
    End Sub

    'Added by Abir T. @ 06.06.2014
    'This function will open a grid containing code and description from the parametric table.
    Public Function DisplayField(ByVal FrmOwner As Form, ByRef _Col As Int32, ByRef _DataColumn As C1DataColumn, ByVal DTName As String,
                                   ByVal Table As String, ByVal Code As String, ByVal Description As String, ByVal GroupByField As String) As String
        Dim strsql, sCaption As String
        Dim dt As DataTable
        Dim da As SqlDataAdapter
        Dim frm As frmSearch
        Dim Retfield() As String = {"Code", "Description"}
        Dim Sizefield() As Int16 = {50, 200}
        Dim Alnfield() As String = {"C", "N"}
        Dim tblRowUsage(1, 1) As String
        Dim DrSQL As SqlDataReader

        Try
            DisplayField = ""
            If DTName = "Usage" Then
                tblRowUsage(0, 0) = "N" : tblRowUsage(0, 1) = "Normal Use"
                tblRowUsage(1, 0) = "I" : tblRowUsage(1, 1) = "Internal Use"
                dt = CreateDataTable(DTName, "Code", "Description", tblRowUsage)

            ElseIf DTName = "NewUsed" Then
                Dim RowNumb, i As Integer
                If UCase(gbUser) = "SA" Or UCase(gbUser) = "SU" Then
                    RowNumb = 8
                Else
                    RowNumb = CInt(GetFieldDesc("STK04_APPSTKPRIV", " ASP_USER='" & gbUser & "' AND ASP_APPcODE ='" & gbApp & "'", "COUNT(ISNULL(ASP_STKTAG,0))"))
                End If

                Dim tblRowNewUsed(RowNumb - 1, 1) As String
                If UCase(gbUser) = "SA" Or UCase(gbUser) = "SU" Then
                    tblRowNewUsed(0, 0) = "N" : tblRowNewUsed(0, 1) = "New"
                    tblRowNewUsed(1, 0) = "U" : tblRowNewUsed(1, 1) = "Used"
                    tblRowNewUsed(2, 0) = "F" : tblRowNewUsed(2, 1) = "Finished Product" '...Jad-J...(24/10/2016) P-Production replaced by F-Finished Product
                    tblRowNewUsed(3, 0) = "R" : tblRowNewUsed(3, 1) = "Raw Material"
                    tblRowNewUsed(4, 0) = "S" : tblRowNewUsed(4, 1) = "Semi-Finished Product" '...Jad-J...(24/10/2016) Selling Item replaced by Semi-Finished Product
                    tblRowNewUsed(5, 0) = "T" : tblRowNewUsed(5, 1) = "Title"
                    tblRowNewUsed(6, 0) = "V" : tblRowNewUsed(6, 1) = "Title For Rent"
                    tblRowNewUsed(7, 0) = "C" : tblRowNewUsed(7, 1) = "Consignment Item"
                Else
                    strsql = "SELECT ASP_STKTAG , CASE WHEN ASP_STKTAG='N' THEN 'New' ELSE "
                    strsql &= " CASE WHEN ASP_STKTAG='U' THEN 'Used' ELSE"
                    strsql &= " CASE WHEN ASP_STKTAG='F' THEN 'Finished Product' ELSE " '...Jad-J...(24/10/2016) P-Production replaced by F-Finished Product
                    strsql &= " CASE WHEN ASP_STKTAG='R' THEN 'Raw Material' ELSE	"
                    strsql &= " CASE WHEN ASP_STKTAG='S' THEN 'Semi-Finished Product' ELSE" '...Jad-J...(24/10/2016) Selling Item replaced by Semi-Finished Product
                    strsql &= " CASE WHEN ASP_STKTAG='T' THEN 'Title' ELSE"
                    strsql &= " CASE WHEN ASP_STKTAG='V' THEN 'Title For Rent' ELSE"
                    strsql &= " CASE WHEN ASP_STKTAG='C' THEN 'Consignment Item' ELSE 'New & Consignment' END "
                    strsql &= " END END END END	END END END  FROM STK04_APPSTKPRIV "
                    strsql &= " WHERE ASP_USER='" & gbUser & "' AND ASP_APPcODE ='" & gbApp & "'"
                    ReadFromDBToDataReader(strsql, DrSQL)
                    i = 0
                    If DrSQL.HasRows Then
                        While DrSQL.Read
                            tblRowNewUsed(i, 0) = DrSQL.Item(0).ToString
                            tblRowNewUsed(i, 1) = DrSQL.Item(1).ToString
                            i = i + 1
                        End While
                    End If
                End If
                dt = CreateDataTable(DTName, "Code", "Description", tblRowNewUsed)
            Else
                dt = New DataTable(DTName)
                strsql = "Select " & Code & " as [Code],  " & Description & " as [Description] From " & Table & " Order By " & GroupByField
                da = New SqlDataAdapter(strsql, ConnectionString)
                da.Fill(dt)
            End If

            frm = New frmSearch
            With frm
                .Owner = FrmOwner
                If .ShowDialog("Return", dt, dt.Rows.Count, DTName & " List", Sizefield, Alnfield, Retfield, 0,
                    Nothing, False, 300, 400, , True, , , , , , , , False) = DialogResult.OK Then DisplayField = gbRetSearch(0)
            End With
        Catch ex As Exception
            Throw ex
        Finally
            If Not dt Is Nothing Then dt.Dispose() : dt = Nothing
            If Not da Is Nothing Then da.Dispose() : da = Nothing
            If Not frm Is Nothing Then frm.Dispose() : frm = Nothing
        End Try
    End Function
#Region "Purchase Order"
    Public Sub CreatePOGridQTYF(ByVal TmpFashQty As String, ByVal MaxCnt As Integer)
        Dim i, j As Integer
        Try
            DropSqlObject(TmpFashQty, "TABLE")
            strsql = "Create table " & TmpFashQty
            strsql &= " (Q_RECID numeric(18,0) NOT NULL ,RECID numeric(18,0) NULL , "
            strsql &= " GridCode VARCHAR(4),"
            For j = 1 To MaxCnt
                strsql &= " Size" & j & " numeric(18," & gbDecNo & "),"
            Next
            strsql &= " CONSTRAINT PK_" & TmpFashQty & " PRIMARY KEY  (Q_RECID) "
            strsql &= "  ) ON [PRIMARY]"
            Execute(strsql)

        Catch ex As Exception
            Throw ex
        End Try
    End Sub

    Public Sub CreatePoPendingTemp(ByVal TempPendingDO As String)
        Dim strsql As String
        Try
            strsql = " IF NOT EXISTS (SELECT name FROM sysobjects WHERE type = 'U' AND name = '" & TempPendingDO & "')"
            strsql &= " CREATE TABLE " & TempPendingDO
            strsql &= " (SFP_Recid NUMERIC(18,0) NOT NULL,"
            strsql &= " SFP_Comp varchar(2),"
            strsql &= " SFP_SUPPLIER varchar(4),"
            strsql &= " SFP_CATEGORY varchar(4),"
            strsql &= " SFP_Serie NUMERIC(18,0),"
            strsql &= " SFP_Usage VARCHAR(1), "
            strsql &= " SFP_NEWUSED VARCHAR(1),"
            strsql &= " SFP_Season VARCHAR(2),"
            strsql &= " SFP_Brand VARCHAR(3),"
            strsql &= " SFP_Grid VARCHAR(4),"
            strsql &= " SFP_Gender VARCHAR(2),"
            strsql &= " SFP_Style VARCHAR(3),"
            strsql &= " SFP_SingleRef1 VARCHAR(20),"
            strsql &= " SFP_SingleRef2 VARCHAR(20),"
            strsql &= " SFP_ItmClass VARCHAR(1),"
            strsql &= " SFP_LOCSRC VARCHAR(2),"
            strsql &= " SFP_InfDate DATETIME,"
            strsql &= " SFP_OpSource VARCHAR(2),"
            strsql &= " SFP_Desc VARCHAR(100) COLLATE SQL_Latin1_General_CP1256_CI_AS NULL,"
            strsql &= " SFP_ImpLoc VARCHAR(1),"
            strsql &= " SFP_ItmIdent NUMERIC(18,0),"
            strsql &= " SFP_SerReg VARCHAR(1),"
            strsql &= " SFP_ItmType VARCHAR(3),"
            strsql &= " SFP_UNIT VARCHAR(3),"
            strsql &= " SFP_UNITPACK DECIMAL(18, 3),"
            strsql &= " SFP_SUBUNIT VARCHAR(3),"
            strsql &= " SFP_FCCode VARCHAR(3),"
            strsql &= " SFP_VATType VARCHAR(1),"
            strsql &= " SFP_VATPER DECIMAL(6, 2),"
            strsql &= " SFP_StartTaxDate DATETIME,"
            strsql &= " SFP_CrUStamp VARCHAR(4),"
            strsql &= " SFP_CrDStamp DATETIME,"
            strsql &= " SFP_UStamp varchar(4),"
            strsql &= " SFP_DStamp DATETIME,"
            strsql &= " SFP_HSCode VARCHAR(30),"
            'Rita M @ 10.02.2016. Add VAT status
            strsql &= " SFP_VATSTAT varchar(1),"
            strsql &= " SFP_TechSheet nvarchar(max)  , sfp_operator varchar(1) ,"
            strsql &= " SFP_Status varchar(1)," 'Status : O=Old ; N=New ; M=Modified
            strsql &= " SFP_OldRecid NUMERIC(18,0)," 'Old RecId from STK04_STKFILPending
            strsql &= " FromPO Varchar(2)" 'Fom Po
            strsql &= " CONSTRAINT PK_" & TempPendingDO & " PRIMARY KEY clustered"
            strsql &= " (SFP_Recid) ) ON [PRIMARY]"
            strsql &= " ELSE DELETE FROM " & TempPendingDO
            Execute(strsql)
        Catch ex As Exception
            Throw ex
        End Try
    End Sub

    Public Sub CreatePoGridDetails(ByVal TmpGridDetails As String, ByVal TmpGridExpiry As String, ByVal TmpGridBarcode As String, ByVal DelOrder As String)
        Dim i As Integer
        Try
            DropSqlObject(TmpGridDetails, "TABLE")
            strsql = "Create table " & TmpGridDetails
            strsql &= " (RECID numeric(18,0) NOT NULL ,DOB_Drecid numeric(18,0) NULL , MOD_supitmref varchar(100) ,"
            If DelOrder = "Receiving Order Fashion" Or DelOrder = "Consignment Receiving Order Fashion" Then
                strsql &= " SuppColor VARCHAR(30),"
            End If

            strsql &= " Stk_ref nvarchar(100),"
            If DelOrder = "Receiving Order Fashion" Or DelOrder = "Consignment Receiving Order Fashion" Then
                strsql &= " STK_Gender NVARCHAR(40),"
            End If
            strsql &= " MOD_stat [varchar](30) null,"

            If DelOrder = "Receiving Order Fashion" Or DelOrder = "Consignment Receiving Order Fashion" Then
                strsql &= " ItemType NVARCHAR(30),"
                strsql &= " Style NVARCHAR(20),"
            End If

            strsql &= " MOD_RMRecid numeric(18,0),"
            strsql &= " MOD_Count [int],"
            strsql &= " stk_desc [nvarchar](100)  ," 'wadih @ 01/03/2016

            If DelOrder = "Receiving Order Fashion" Or DelOrder = "Consignment Receiving Order Fashion" Then
                strsql &= " Color VARCHAR(2),"
                strsql &= " ColorN_O VARCHAR(1),"
            End If
            strsql &= " MOD_OrdQty [numeric](18," & gbDecNo & ") ,"
            'CH @ 06/04/2017
            strsql &= " stk_unit [varchar](20),"
            strsql &= " stk_Subunit [varchar](20),"
            strsql &= " MOD_ConsOrdQty [decimal](18," & gbDecNo & ") ,invd_VAT [varchar](100) ,"
            'ch @ 10/04/2018
            strsql &= " COST [decimal](18,3),"
            If DelOrder = "Consignment Receiving Order" Or DelOrder = "Consignment Receiving Order Fashion" Then
                strsql &= " disc [Decimal] (6,2) , net [decimal](18,3) ,  "
            End If
            strsql &= " STK_FCCODE [VARCHAR](3),"
            strsql &= "  new_old [varchar](1), PO [varchar](1) , "
            '''06/06/2014 Modifief Rachel Assaf  '''''''''If DelOrder = "Receiving Order Fashion" Or DelOrder = "Consignment Receiving Order Fashion" Then
            strsql &= " Barcode [NVARCHAR](30),"
            'End If

            strsql &= " ItmClass [varchar](1),"
            strsql &= " btnqc [varchar](1),"
            strsql &= " CONSTRAINT PK_" & TmpGridDetails & " PRIMARY KEY  (RECID) "
            strsql &= "  ) ON [PRIMARY]"
            Execute(strsql)

            If DelOrder = "Receiving Order" Or DelOrder = "Consignment Receiving Order" Then
                'Temp Expiry
                DropSqlObject(TmpGridExpiry, "TABLE")
                strsql = "Create table " & TmpGridExpiry
                strsql &= " (E_RECID [numeric](18,0) not null,"
                strsql &= " NewRECID [numeric](18,0) ,"
                strsql &= " RECID [numeric](18,0) ,"
                strsql &= " DOB_BatchInfo [varchar](15),"
                strsql &= " DOB_ExpiryDate [varchar] (10),"
                strsql &= " MOD_OrdQty [numeric](18,3),"
                strsql &= " exp_stat [varchar](1) ,"
                strsql &= " DOQ_ProductionDt [varchar] (10),"
                strsql &= " [btnQC] [varchar](1) ,"
                strsql &= " CONSTRAINT PK_" & TmpGridExpiry & " PRIMARY KEY  (E_RECID) "
                strsql &= "  ) ON [PRIMARY]"

                Execute(strsql)

                'Temp Barcode
                DropSqlObject(TmpGridBarcode, "TABLE")
                strsql = "Create table " & TmpGridBarcode
                strsql &= " (B_RECID [numeric](18,0) not null,DOB_RECID [numeric](18,0) NULL," ' intial Recid 
                strsql &= "  RECID [numeric](18,0) ," ' linked to DO Temptable [RECID]  
                strsql &= " E_RECID [numeric](18,0) ," ' linked to Expiry temptable [E_RECID] 
                strsql &= " DOB_Barcode [varchar](20),"
                strsql &= " bar_stat [varchar](1) ,"
                strsql &= " DOB_Drecid [numeric](18, 0)," ' linked to QC Head Table 
                strsql &= " CONSTRAINT PK_" & TmpGridBarcode & " PRIMARY KEY  (B_RECID) "
                strsql &= "  ) ON [PRIMARY]"
                Execute(strsql)
            End If
        Catch ex As Exception
            Throw ex
        End Try
    End Sub

    Public Sub PrintRO(ByVal Ownerfrm As Form, ByVal cntTitle As String, ByVal DelOrder As String, ByVal TmpGridDetails As String, ByVal TmpFashQty As String, ByVal TmpGridBarcode As String, ByVal Code As String, ByVal txtCurr As String, ByVal ChkBarcode As Boolean, ByVal ChkCost As Boolean, _
                       ByVal ChkExpiry As Boolean, ByVal ChkTechContent As Boolean, ByVal chkPrintQC As Boolean, ByVal rbPrintLatin As Boolean, ByVal MaxCnt As Integer)
        Dim frm As frmPreviewGrid
        Dim strsql As String
        Dim strPrev(,) As String

        Try
            Cursor.Current = Cursors.WaitCursor
            strPrev = Nothing
            frm = New frmPreviewGrid
            'REPORT SQL
            'If (DelOrder = "Receiving Order Fashion" Or DelOrder = "Consignment Receiving Order Fashion") And ChkBarcode = True Then
            '    strsql = " SELECT Distinct "
            '    strsql &= " DOH_Recid as [DO #],DOD_Recid,DOH_Ref as [Reference], DOD_Counter,DOH_Date as [Date],case when DOH_Status ='N' then 'Normal' else 'Reverse' end as [Status],"
            '    strsql &= " Sup_code as [Supplier], sup_name as [Supplier Name], "
            '    strsql &= " DOH_loc as [Loc.],Loc_Desc as [Loc. Name] ,DOH_ActDelDate as [Actual Delivery Date], DOH_Curr as [FC Code]  ,DOH_PurRecid as [Purchase #],DOH_PoRecid as [PO #], "
            '    strsql &= " DOD_ItmRef as [Sup Ref.], CASE WHEN dod_stat='n' THEN 'Normal' ELSE CASE WHEN dod_stat='f' THEN 'Free' else case when dod_stat='r' THEN 'Return' END END END  AS [Details Status]," 'CASE WHEN DOD_Stat = 'F' THEN 'Free' WHEN DOD_Stat = 'R' THEN 'Return' WHEN DOD_Stat = 'N' THEN 'Normal' WHEN DOD_Stat is NULL THEN 'Normal' END AS [Details Status],"
            '    strsql &= " Stk04_stkfil.STK_REF as [Stk Ref.],Stk04_stkfil.stk_desc as [Item Description],Stk04_stkfil.stk_brand as [Item Brand], "
            '    strsql &= " br_Code as [Brd], br_Name as [Brand],Seas_Code as [Seas],Seas_Desc as [Season],"
            '    strsql &= " DOD_Color as [Supp. Color],Color_Desc AS [Color],   COALESCE(DOB_GridCode,fashion.gridcode) AS [Grid],DOD_Counter as [Counter],stk_Style as [Style],  stk04_stkfil.stk_gender as [Gender],stk_ItmType as [ItemType],"
            '    If ChkBarcode = True Then
            '        strsql &= "    DOQ_GridSize  [Size] , DOB_Barcode [Barcode] , "
            '    Else
            '        strsql &= "   COALESCE(DOB_GridSize,gs.size1)  [Size] ,   ISNULL(DOB_Barcode,'') [Barcode] , "
            '    End If
            '    If ChkBarcode = True Then
            '        'DOQ_Recqty
            '        strsql &= " DOQ_Recqty as [Qty], CONVERT(NUMERIC(18,3),CASE ISNULL(DOD_ConsQty,0) WHEN 0 THEN 0 else (DOD_ConsQty   /DOD_Recqty)*DOQ_Recqty end )  as [Cons.Qty], meas_desc as [Unit], "
            '    Else
            '        strsql &= " DOD_Recqty as [Qty],  "
            '        strsql &= " CASE WHEN V_Pord2_GetStkMeas.MEAS_SIGN = '*' THEN DOQ_Recqty * V_Pord2_GetStkMeas.MEAS_VALUE "
            '        strsql &= " WHEN V_Pord2_GetStkMeas.MEAS_SIGN = '/' THEN DOQ_Recqty / V_Pord2_GetStkMeas.MEAS_VALUE  end AS [Cons.Qty], meas_desc as [Unit],"
            '    End If

            '    strsql &= " CASE WHEN DOD_vat = 'T' THEN 'T' WHEN DOD_vat = 'N' then 'NT' when  DOD_vat = 'E' THEN 'E' end as [VAT Type],	ISNULL(STK04_STKFIL.STK_VATPER,0) AS[VAT %], "

            '    'cost from stk04 stkprices
            '    If DelOrder = "Receiving Order Fashion" Or DelOrder = "Consignment Receiving Order Fashion" Then
            '        strsql &= " SP_LCTTC as [Price LC], SP_FCTTC as [Price CC] ,"
            '    End If
            '    If ChkCost = True Then
            '        If DelOrder = "Consignment Receiving Order Fashion" And ChkBarcode = True Then
            '            strsql &= ""
            '        Else
            '            strsql &= " , DOD_AvgCostLC as [AvgCostLC],DOD_AvgCostCC  as [AvgCostCC], DOD_AvgCostFC as [AvgCostFC] , isnull(DOD_Disc,0)  as disc "
            '            strsql &= "  ,DOD_NetCostLC as [NetCostLC],DOD_NetCostCC  as [NetCostCC], DOD_NetCostFC as [NetCostFC]"
            '            strsql &= " ,Consignment.STK_AVGCOSTLC as [UNITCOSTLC], Consignment.STK_AVGCOSTCC as [UNITCOSTCC],dbo.[convert5]('" & gbApp & "','" & gbComp & "','" & gbYear & "',"
            '            strsql &= " Consignment.STK_AVGCOSTFC,Consignment.STK_FCCODE,DOH_Curr,'" & CType(Now, Date).ToString("MM/dd/yyyy") & "')  as [UNITCOSTFC]  , "
            '        End If
            '    End If
            '    strsql &= "    COALESCE(DSTech_Meas,'') [TechContent], total.FLQTY, total.NLQTY, total.FWQTY, total.NWQTY, total.FPQTY, total.NPQTY, total.RLQTY, total.RWQTY, total.RPQTY"
            '    strsql &= " From stk04_PurDelOrdH Inner join stk04_supplier  On Sup_recid  =  DOH_SupRecid"
            '    strsql &= " Inner join Stk04_location On loc_code  =  DOH_Loc"
            '    strsql &= " LEFT JOIN stk04_PurDelOrdD ON DOD_HRecid = DOH_Recid"
            '    strsql &= " LEFT JOIN dbo.stk04_PurDelOrdDBE ON DOD_Recid = DOB_Drecid "
            '    strsql &= " INNER JOIN Stk04_stkfil ON Stk04_stkfil.stk_recid = dod_stkrecid"
            '    strsql &= "  inner join stk04_measurements on meas_unit =  DOD_Unit "
            '    strsql &= " LEFT JOIN " & TmpGridDetails & " Grid ON DOD_Recid=Grid.DOB_Drecid"
            '    strsql &= " LEFT JOIN " & TmpFashQty & " Fashion on Fashion.Recid=Grid.Recid"
            '    If DelOrder = "Receiving Order Fashion" Then
            '        strsql &= " LEFT JOIN TempGridSizeSP_DeliveryOrderF" & gbUser & " GS ON GS.G_Code = Fashion.GridCode"
            '    Else
            '        strsql &= " LEFT JOIN TempGridSizeSP_DeliveryOrderCF" & gbUser & " GS ON GS.G_Code = Fashion.GridCode"
            '    End If
            '    'prices from stk04 stkprices with default price type and default qty
            '    strsql &= " LEFT JOIN (SELECT SP_StkRecId,SP_LCTTC,SP_LCTAXEX,SP_FCTAXEX,SP_FCTTC,SP_FCCODE FROM stk04_stkprices  "
            '    strsql &= " INNER JOIN Stk04_SalPriceType ON  price_code = SP_PriceType AND  Price_Default='y' WHERE ISNULL(SP_StartingQty, 0) = 0 ) temp ON temp.SP_StkRecId =dod_stkrecid"
            '    strsql &= " LEFT JOIN stk04_brand on  br_Code=DOH_Brand"
            '    strsql &= " LEFT JOIN Stk04_Season On  Seas_Code=DOH_Season"
            '    strsql &= " LEFT JOIN Stk04_COLOR ON Color_Code=DOD_ColorCode"
            '    strsql &= "	LEFT JOIN dbo.STK04_DSTechnical ON STK04_DSTechnical.DSTech_RecId = STK04_STKFIL.STK_RECID"
            '    '   If ChkBarcode = True Then
            '    strsql &= "  LEFT JOIN dbo.stk04_PurDelOrdDQty ON stk04_PurDelOrdDQty.DOQ_Drecid = stk04_PurDelOrdD.DOD_Recid "
            '    'End If
            '    strsql &= " left JOIN ( SELECT  DOH_Recid AS [hRecID] ,  "
            '    strsql &= " SUM(( CASE WHEN meas_type = 'L' AND SUBSTRING(MOD_stat, 1, 1) = 'F' THEN MOD_ConsOrdqty ELSE 0 END )) AS FLQTY , "
            '    strsql &= " SUM(( CASE WHEN meas_type = 'L' AND SUBSTRING(MOD_stat, 1, 1) = 'N' THEN MOD_ConsOrdqty ELSE 0 END )) AS NLQTY , "
            '    strsql &= " SUM(( CASE WHEN meas_type = 'W' AND SUBSTRING(MOD_stat, 1, 1) = 'F' THEN MOD_ConsOrdqty ELSE 0 END )) AS FWQTY , "
            '    strsql &= " SUM(( CASE WHEN meas_type = 'W' AND SUBSTRING(MOD_stat, 1, 1) = 'N' THEN MOD_ConsOrdqty ELSE 0 END )) AS NWQTY , "
            '    strsql &= " SUM(( CASE WHEN meas_type = 'P' AND SUBSTRING(MOD_stat, 1, 1) = 'F' THEN MOD_ConsOrdqty ELSE 0 END )) AS FPQTY , "
            '    strsql &= " SUM(( CASE WHEN meas_type = 'P' AND SUBSTRING(MOD_stat, 1, 1) = 'N' THEN MOD_ConsOrdqty ELSE 0 END )) AS NPQTY , "
            '    strsql &= " SUM(( CASE WHEN meas_type = 'L' AND SUBSTRING(MOD_stat, 1, 1) = 'R' THEN MOD_ConsOrdqty ELSE 0 END )) AS RLQTY , "
            '    strsql &= " SUM(( CASE WHEN meas_type = 'W' AND SUBSTRING(MOD_stat, 1, 1) = 'R' THEN MOD_ConsOrdqty ELSE 0 END )) AS RWQTY , "
            '    strsql &= " SUM(( CASE WHEN meas_type = 'P' AND SUBSTRING(MOD_stat, 1, 1) = 'R' THEN MOD_ConsOrdqty ELSE 0 END )) AS RPQTY "
            '    strsql &= " FROM    " & TmpGridDetails & " "
            '    strsql &= " INNER JOIN V_Pord2_GetStkMeas ON MOD_rmrecid = stk_recid   "
            '    strsql &= " LEFT JOIN stk04_PurDelOrdD ON stk04_PurDelOrdD.DOD_Recid =  " & TmpGridDetails & ".DOB_Drecid "
            '    strsql &= " LEFT JOIN stk04_PurDelOrdH ON stk04_PurDelOrdH.DOH_Recid = stk04_PurDelOrdD.DOD_Hrecid "
            '    strsql &= " GROUP BY DOh_Recid ) AS Total ON Total.hRecID = DOH_Recid "
            '    strsql &= " Inner JOIN V_Pord2_GetStkMeas ON V_Pord2_GetStkMeas.STK_ACTIVE = STK04_STKFIL.STK_ACTIVE AND V_Pord2_GetStkMeas.stk_recid = STK04_STKFIL.STK_RECID  "
            '    strsql &= " AND V_Pord2_GetStkMeas.Stk_unit = STK04_STKFIL.STK_UNIT AND V_Pord2_GetStkMeas.STK_ItmClass = STK04_STKFIL.STK_ItmClass "
            '    strsql &= " where Dod_hrecid IN " & Code


            'Else
            strsql = " SELECT Distinct "
                strsql &= " DOH_Recid as [DO #],DOD_Recid,DOH_Ref as [Reference], DOD_Counter,DOH_Date as [Date],case when DOH_Status ='N' then 'Normal' else 'Reverse' end as [Status],"
                strsql &= " Sup_code as [Supplier], sup_name as [Supplier Name], "
                strsql &= " DOH_loc as [Loc.],Loc_Desc as [Loc. Name] ,DOH_ActDelDate as [Actual Delivery Date],DOH_Curr as [FC Code]  ,DOH_PurRecid as [Purchase #],DOH_PoRecid as [PO #], "
                strsql &= " DOD_ItmRef as [Sup Ref.], CASE WHEN dod_stat='n' THEN 'Normal' ELSE CASE WHEN dod_stat='f' THEN 'Free' else case when dod_stat='r' THEN 'Return' END END END  AS [Details Status]," 'CASE WHEN DOD_Stat = 'F' THEN 'Free' WHEN DOD_Stat = 'R' THEN 'Return' WHEN DOD_Stat = 'N' THEN 'Normal' WHEN DOD_Stat is NULL THEN 'Normal' END AS [Details Status],"
                strsql &= " Stk04_stkfil.STK_REF as [Stk Ref.],Stk04_stkfil.stk_desc as [Item Description],Stk04_stkfil.stk_brand as [Item Brand], "
            'If DelOrder = "Receiving Order Fashion" Or DelOrder = "Consignment Receiving Order Fashion" Then
            '    strsql &= " br_Code as [Brd], br_Name as [Brand],Seas_Code as [Seas],Seas_Desc as [Season],"
            '    strsql &= " DOD_Color as [Supp. Color],Color_Desc AS [Color], Fashion.GridCode AS [Grid],DOD_Counter as [Counter],Grid.Style as [Style], Grid.stk_gender as [Gender],Grid.ItemType as [ItemType],"
            '    'For i = 1 To MaxCnt  removed by ch @ 12/12/2016 wf: 4774
            '    For i = 1 To 30
            '        If i <= MaxCnt Then
            '            strsql &= " GS.Size" & i & " as Size" & i & ", "
            '        Else
            '            strsql &= " '' as Size" & i & ", "
            '        End If
            '    Next
            '    If ChkBarcode = False Then
            '        'For j = 1 To MaxCnt  removed by ch @ 12/12/2016 wf: 4774 
            '        For j = 1 To 30
            '            If j <= MaxCnt Then
            '                strsql &= " Fashion.Size" & j & " AS Qty" & j & ", "
            '            Else
            '                strsql &= " '' AS Qty" & j & ", "
            '            End If
            '        Next
            '    ElseIf ChkBarcode = True Then
            '        ' For k = 1 To MaxCnt  removed by ch @ 12/12/2016 wf: 4774
            '        For k = 1 To 30
            '            If k <= MaxCnt Then
            '                strsql &= " Barcode.Tmp_Size" & k & " AS Barcode" & k & ", "
            '            Else
            '                strsql &= "  '' AS Barcode" & k & ", "
            '            End If
            '        Next
            '    End If
            'End If
            strsql &= " DOD_Recqty as [Qty], "
                strsql &= " /*CASE WHEN V_Pord2_GetStkMeas.MEAS_SIGN = '*' THEN DOQ_Recqty * V_Pord2_GetStkMeas.MEAS_VALUE  "
                strsql &= " WHEN V_Pord2_GetStkMeas.MEAS_SIGN = '/' THEN DOQ_Recqty / V_Pord2_GetStkMeas.MEAS_VALUE  end */ DOD_ConsQty AS [Cons.Qty], meas. meas_desc as [Unit], "
                strsql &= " CASE WHEN DOD_vat = 'T' THEN 'T' WHEN DOD_vat = 'N' then 'NT' when  DOD_vat = 'E' THEN 'E' end as [VAT Type],	ISNULL(STK04_STKFIL.STK_VATPER,0) AS[VAT %], "

            'cost from stk04 stkprices
            'If DelOrder = "Receiving Order Fashion" Or DelOrder = "Consignment Receiving Order Fashion" Then
            '    strsql &= " SP_LCTTC as [Price LC], SP_FCTTC as [Price CC]  ,"
            'End If 
            If ChkBarcode = True And ChkExpiry = False Then
                        strsql &= "   ISNULL(DOB_Barcode,'') as [Barcode] ,"
                    ElseIf ChkExpiry = True Then
                        strsql &= " DOQ_BatchInfo as [Lot No],CASE WHEN LEFT (DOQ_ExpiryDate ,3)='00.' THEN SUBSTRING(DOQ_ExpiryDate,4,LEN(DOQ_ExpiryDate)) ELSE DOQ_ExpiryDate END  as [Expiry Date],"
                        strsql &= " CASE WHEN LEFT(STQ_ProductionDt, 3) = '00.' THEN SUBSTRING(STQ_ProductionDt, 4, LEN(STQ_ProductionDt)) Else STQ_ProductionDt END AS [Production Date] ,"
                        strsql &= " DOB_Barcode as [Barcode],DOQ_Recqty as [Qty Exp] ,"
                    End If

            If ChkCost = True Then
                'If DelOrder = "Consignment Receiving Order Fashion" And ChkBarcode = True Then
                '    strsql &= ""
                'Else
                strsql &= " DOD_AvgCostLC as [AvgCostLC],DOD_AvgCostCC  as [AvgCostCC], DOD_AvgCostFC as [AvgCostFC]  ,   isnull(DOD_Disc,0)  as disc "
                        strsql &= " ,DOD_NetCostLC as [NetCostLC],DOD_NetCostCC  as [NetCostCC], DOD_NetCostFC as [NetCostFC]"
                        strsql &= " ,Consignment.STK_AVGCOSTLC as [UNITCOSTLC], Consignment.STK_AVGCOSTCC as [UNITCOSTCC],dbo.[convert5]('" & gbApp & "','" & gbComp & "','" & gbYear & "',"
                        strsql &= " Consignment.STK_AVGCOSTFC,Consignment.STK_FCCODE,DOH_Curr,'" & CType(Now, Date).ToString("MM/dd/yyyy") & "')  as [UNITCOSTFC]   ,"
                'End If
            End If
                strsql &= "    COALESCE(DSTech_Meas,'') [TechContent], total.FLQTY, total.NLQTY, total.FWQTY, total.NWQTY, total.FPQTY, total.NPQTY, total.RLQTY, total.RWQTY, total.RPQTY"
                strsql &= " From stk04_PurDelOrdH Inner join stk04_supplier  On Sup_recid  =  DOH_SupRecid"
                strsql &= " Inner join Stk04_location On loc_code  =  DOH_Loc"
                strsql &= " LEFT JOIN stk04_PurDelOrdD ON DOD_HRecid = DOH_Recid"
                'prices from stk04 stkprices with default price type and default qty
                strsql &= " LEFT JOIN (SELECT SP_StkRecId,SP_LCTTC,SP_LCTAXEX,SP_FCTAXEX,SP_FCTTC,SP_FCCODE FROM stk04_stkprices  "
                strsql &= " INNER JOIN Stk04_SalPriceType ON  price_code = SP_PriceType AND  Price_Default='y' WHERE ISNULL(SP_StartingQty, 0) = 0 ) temp ON temp.SP_StkRecId =dod_stkrecid"

                strsql &= " INNER JOIN Stk04_stkfil ON Stk04_stkfil.stk_recid = dod_stkrecid"
                If ChkCost = True Then
                'If DelOrder = "Consignment Receiving Order Fashion" And ChkBarcode = True Then
                '    strsql &= ""
                'Else
                strsql &= " LEFT JOIN STK04_STKFIL Consignment ON "
                        For i = 0 To CurrItmKey.Length - 1
                            If UCase(CurrItmKey(i)) <> "STK_NEWUSED" Then
                                strsql &= " STK04_STKFIL." & CurrItmKey(i) & " = consignment." & CurrItmKey(i) & " and "
                            End If
                        Next
                        If DelOrder = "Consignment Receiving Order" Then
                            strsql &= " Consignment.STK_NEWUSED='C' "
                        Else
                            strsql &= " 1=1"
                        End If
                        strsql &= " AND STK04_STKFIL.STK_recid = consignment.STK_recid "
                    'End If
                End If
            strsql &= " LEFT JOIN stk04_PurDelOrdDQty ON DOD_Recid=DOQ_Drecid  "
            If ChkBarcode = True Or ChkExpiry = True Then

                        strsql &= " LEFT JOIN stk04_PurDelOrdDBE ON DOD_Recid=DOB_Drecid  AND "
                        strsql &= " COALESCE(dOB_BatchInfo,'')=COALESCE(DOQ_BatchInfo,'') AND COALESCE(DOB_ExpiryDate,'')=COALESCE(DOQ_ExpiryDate,'')"
                        strsql &= " AND COALESCE(DOB_GridCode,'')=COALESCE(DOQ_GridCode,'') AND COALESCE(DOB_GridSize,'')=COALESCE(DOQ_GridSize,'')"

                'ElseIf DelOrder = "Receiving Order Fashion" Or DelOrder = "Consignment Receiving Order Fashion" Then
                '    strsql &= " LEFT JOIN " & TmpGridDetails & " Grid ON DOD_Recid=DOB_Drecid"
                '    strsql &= " LEFT JOIN " & TmpFashQty & " Fashion on Fashion.Recid=Grid.Recid"
                '    If DelOrder = "Receiving Order Fashion" Then
                '        strsql &= " LEFT JOIN TempGridSizeSP_DeliveryOrderF" & gbUser & " GS ON GS.G_Code = Fashion.GridCode"
                '    Else
                '        strsql &= " LEFT JOIN TempGridSizeSP_DeliveryOrderCF" & gbUser & " GS ON GS.G_Code = Fashion.GridCode"
                '    End If

                '    strsql &= " LEFT JOIN stk04_brand on  br_Code=DOH_Brand"
                '    strsql &= " LEFT JOIN Stk04_Season On  Seas_Code=DOH_Season"
                '    strsql &= " LEFT JOIN Stk04_COLOR ON Color_Code=DOD_ColorCode"
                '    If ChkBarcode = True Then
                '        strsql &= " LEFT JOIN " & TmpGridBarcode & "F Barcode ON Barcode.Tmp_DRecId = grid.RECID  "
                '    End If
            End If
                strsql &= " LEFT JOIN dbo.STK04_DSTechnical ON STK04_DSTechnical.DSTech_RecId = STK04_STKFIL.STK_RECID "
                strsql &= " left JOIN ( SELECT  DOH_Recid AS [hRecID] ,  "
                strsql &= " SUM(( CASE WHEN meas_type = 'L' AND SUBSTRING(MOD_stat, 1, 1) = 'F' THEN MOD_ConsOrdqty ELSE 0 END )) AS FLQTY , "
                strsql &= " SUM(( CASE WHEN meas_type = 'L' AND SUBSTRING(MOD_stat, 1, 1) = 'N' THEN MOD_ConsOrdqty ELSE 0 END )) AS NLQTY , "
                strsql &= " SUM(( CASE WHEN meas_type = 'W' AND SUBSTRING(MOD_stat, 1, 1) = 'F' THEN MOD_ConsOrdqty ELSE 0 END )) AS FWQTY , "
                strsql &= " SUM(( CASE WHEN meas_type = 'W' AND SUBSTRING(MOD_stat, 1, 1) = 'N' THEN MOD_ConsOrdqty ELSE 0 END )) AS NWQTY , "
                strsql &= " SUM(( CASE WHEN meas_type = 'P' AND SUBSTRING(MOD_stat, 1, 1) = 'F' THEN MOD_ConsOrdqty ELSE 0 END )) AS FPQTY , "
                strsql &= " SUM(( CASE WHEN meas_type = 'P' AND SUBSTRING(MOD_stat, 1, 1) = 'N' THEN MOD_ConsOrdqty ELSE 0 END )) AS NPQTY , "
                strsql &= " SUM(( CASE WHEN meas_type = 'L' AND SUBSTRING(MOD_stat, 1, 1) = 'R' THEN MOD_ConsOrdqty ELSE 0 END )) AS RLQTY , "
                strsql &= " SUM(( CASE WHEN meas_type = 'W' AND SUBSTRING(MOD_stat, 1, 1) = 'R' THEN MOD_ConsOrdqty ELSE 0 END )) AS RWQTY , "
                strsql &= " SUM(( CASE WHEN meas_type = 'P' AND SUBSTRING(MOD_stat, 1, 1) = 'R' THEN MOD_ConsOrdqty ELSE 0 END )) AS RPQTY  "
                strsql &= " FROM    " & TmpGridDetails & " "
                strsql &= " INNER JOIN V_Pord2_GetStkMeas ON MOD_rmrecid = stk_recid   "
                strsql &= " LEFT JOIN stk04_PurDelOrdD ON stk04_PurDelOrdD.DOD_Recid =  " & TmpGridDetails & ".DOB_Drecid "
                strsql &= " LEFT JOIN stk04_PurDelOrdH ON stk04_PurDelOrdH.DOH_Recid = stk04_PurDelOrdD.DOD_Hrecid "
                strsql &= " GROUP BY DOh_Recid ) AS Total ON Total.hRecID = DOH_Recid "
                strsql &= " Inner JOIN V_Pord2_GetStkMeas ON V_Pord2_GetStkMeas.STK_ACTIVE = STK04_STKFIL.STK_ACTIVE AND V_Pord2_GetStkMeas.stk_recid = STK04_STKFIL.STK_RECID  "
                strsql &= " AND V_Pord2_GetStkMeas.Stk_unit = STK04_STKFIL.STK_UNIT AND V_Pord2_GetStkMeas.STK_ItmClass = STK04_STKFIL.STK_ItmClass "
                strsql &= " inner join stk04_measurements meas on meas.meas_unit = DOD_Unit "
                If ChkExpiry Then
                    'Get Production date
                    strsql &= " LEFT JOIN stk04_stkqty ON stq_loccode='00' AND stq_stkrecid=dod_stkrecid AND coalesce(STQ_Color,'') = coalesce(DOD_Color,'')"
                    strsql &= "     and coalesce(STQ_Grid,'') = coalesce(DOQ_GridCode,'') "
                    strsql &= "     and coalesce(STQ_DetGrid,'') = coalesce(DOQ_GridSize,'') "
                    strsql &= "     and coalesce(STQ_ExpiryDt,'') = coalesce(DOQ_ExpiryDate,'') "
                    strsql &= "     and coalesce(STQ_BatchInfo,'') = coalesce(DOQ_BatchInfo,'') "
                End If
                strsql &= " where Dod_hrecid IN " & Code
                ' strsql &= "	ORDER BY DOH_Recid asc"
                'End If

                Dim drs As SqlDataReader
            'ch @ 12/12/2016 wf: 4774 (requested by rita )
            'If DelOrder = "Receiving Order Fashion" Or DelOrder = "Consignment Receiving Order Fashion" Then
            '    If ChkBarcode.Checked = True Then
            '        strsql &= " AND DOB_Barcode IS NOT NULL AND DOB_Barcode<>''"
            '    End If

            'End If
            ReadFromDBToDataReader(strsql, drs)

            If Not drs.HasRows Then
                MessageBox.Show("No Records Found.", cntTitle, MessageBoxButtons.OK, MessageBoxIcon.Exclamation)
                Exit Sub
            End If
            If chkPrintQC = True Then
                Dim PrintQC As System.Threading.Thread
                '     printingId = txtCode.Text.Trim
                PrintQC = New System.Threading.Thread(Sub() PrintQCAnswers(Ownerfrm, rbPrintLatin, cntTitle, Code))
                PrintQC.Start()
                '   PrintQCAnswers(txtCode.Text.Trim)
            End If
            With frm

                frm.BuildSql(strsql)
                .ColDesign("DOD_Recid", 0, "", C1.Win.C1TrueDBGrid.AlignHorzEnum.Center, C1.Win.C1TrueDBGrid.AlignVertEnum.Top, False, , , , , False, frmPreviewGrid.Orders.Ascending)
                .ColDesign("DO #", -1, "RO #", C1.Win.C1TrueDBGrid.AlignHorzEnum.Center, C1.Win.C1TrueDBGrid.AlignVertEnum.Top, , , , , True, , frmPreviewGrid.Orders.Ascending)
                .ColDesign("Reference", -1, "Reference", C1.Win.C1TrueDBGrid.AlignHorzEnum.Center, C1.Win.C1TrueDBGrid.AlignVertEnum.Top, , , , , True, , )
                .ColDesign("DOD_counter", -1, "DOD_counter", C1.Win.C1TrueDBGrid.AlignHorzEnum.Center, C1.Win.C1TrueDBGrid.AlignVertEnum.Top, False, , , , True, False, , , , , , , , , , , False)
                .ColDesign("FC Code", -1, "Curr", C1.Win.C1TrueDBGrid.AlignHorzEnum.Center, , False, , , , , False, , , , , , , , , , , , "[FC Code] as [Curr]")
                .ColDesign("Date", -1, "Date", C1.Win.C1TrueDBGrid.AlignHorzEnum.Near, C1.Win.C1TrueDBGrid.AlignVertEnum.Top, , , , , True, , , , , , "dd/MM/yyyy")
                .ColDesign("Status", -1, "Status", C1.Win.C1TrueDBGrid.AlignHorzEnum.Near, C1.Win.C1TrueDBGrid.AlignVertEnum.Top, , , , , True)
                .ColDesign("Supplier", -1, "Supplier", C1.Win.C1TrueDBGrid.AlignHorzEnum.Center, C1.Win.C1TrueDBGrid.AlignVertEnum.Top, , , , , True)
                .ColDesign("Supplier Name", -1, "Supplier Name", C1.Win.C1TrueDBGrid.AlignHorzEnum.Near, C1.Win.C1TrueDBGrid.AlignVertEnum.Top, , , , , True)
                .ColDesign("Loc.", -1, "Loc.", C1.Win.C1TrueDBGrid.AlignHorzEnum.Center, C1.Win.C1TrueDBGrid.AlignVertEnum.Top, , , , , True)
                .ColDesign("Loc. Name", -1, "Loc. Name", C1.Win.C1TrueDBGrid.AlignHorzEnum.Near, C1.Win.C1TrueDBGrid.AlignVertEnum.Top, , , , , True)
                .ColDesign("Actual Delivery Date", -1, "Actual Delivery Date", C1.Win.C1TrueDBGrid.AlignHorzEnum.Center, C1.Win.C1TrueDBGrid.AlignVertEnum.Top, , , , , True, , , , , , "dd/MM/yyyy")
                .ColDesign("Sup Ref.", -1, "Sup Ref.", C1.Win.C1TrueDBGrid.AlignHorzEnum.Near, C1.Win.C1TrueDBGrid.AlignVertEnum.Top, , , , , True)
                .ColDesign("Stk Ref.", -1, "Item reference", C1.Win.C1TrueDBGrid.AlignHorzEnum.Near, C1.Win.C1TrueDBGrid.AlignVertEnum.Top, , , , , True)
                .ColDesign("Item Description", -1, "Item Description", C1.Win.C1TrueDBGrid.AlignHorzEnum.Near)
                .ColDesign("Item Brand", -1, "Item Brand", C1.Win.C1TrueDBGrid.AlignHorzEnum.Center, , False, , , , , False)
                If DelOrder = "Receiving Order" Or DelOrder = "Receiving Order Fashion" Then
                    .ColDesign("Purchase #", -1, "Linked to PI", C1.Win.C1TrueDBGrid.AlignHorzEnum.Center, C1.Win.C1TrueDBGrid.AlignVertEnum.Top, , , , , True, , , , , , , , , , , , , "[Purchase #] as [Linked to PI]")
                    .ColDesign("PO #", -1, "PO #", C1.Win.C1TrueDBGrid.AlignHorzEnum.Center, C1.Win.C1TrueDBGrid.AlignVertEnum.Top, , , , , True)
                Else
                    .ColDesign("Purchase #", -1, "Linked to PI", C1.Win.C1TrueDBGrid.AlignHorzEnum.Center, C1.Win.C1TrueDBGrid.AlignVertEnum.Top, False, , , , True, False, , , , , , , , , , , False)
                    .ColDesign("PO #", -1, "PO #", C1.Win.C1TrueDBGrid.AlignHorzEnum.Center, C1.Win.C1TrueDBGrid.AlignVertEnum.Top, False, , , , True, False, , , , , , , , , , , False)
                End If
                'If DelOrder = "Receiving Order Fashion" Or DelOrder = "Consignment Receiving Order Fashion" Then
                '    .ColDesign("Brd", -1, "Brd", C1.Win.C1TrueDBGrid.AlignHorzEnum.Near, C1.Win.C1TrueDBGrid.AlignVertEnum.Top, , , , , True)
                '    .ColDesign("Brand", -1, "Brand", C1.Win.C1TrueDBGrid.AlignHorzEnum.Near, C1.Win.C1TrueDBGrid.AlignVertEnum.Top, , , , , True)
                '    .ColDesign("Seas", -1, "Seas", C1.Win.C1TrueDBGrid.AlignHorzEnum.Near, C1.Win.C1TrueDBGrid.AlignVertEnum.Top, , , , , True)
                '    .ColDesign("Season", -1, "Season", C1.Win.C1TrueDBGrid.AlignHorzEnum.Near, C1.Win.C1TrueDBGrid.AlignVertEnum.Top, , , , , True)
                '    .ColDesign("Supp. Color", -1, "Supp. Color", C1.Win.C1TrueDBGrid.AlignHorzEnum.Near, C1.Win.C1TrueDBGrid.AlignVertEnum.Top, , , , , True)
                '    .ColDesign("Color", -1, "Color", C1.Win.C1TrueDBGrid.AlignHorzEnum.Near, C1.Win.C1TrueDBGrid.AlignVertEnum.Top, , , , , True)
                '    .ColDesign("Grid", -1, "Grid", C1.Win.C1TrueDBGrid.AlignHorzEnum.Near, C1.Win.C1TrueDBGrid.AlignVertEnum.Top, , , , , True)
                '    .ColDesign("Counter", -1, "Counter", C1.Win.C1TrueDBGrid.AlignHorzEnum.Center, C1.Win.C1TrueDBGrid.AlignVertEnum.Top, , , , , , False)
                '    .ColDesign("Style", -1, "Style", C1.Win.C1TrueDBGrid.AlignHorzEnum.Center, C1.Win.C1TrueDBGrid.AlignVertEnum.Top, , , , , True)
                '    .ColDesign("Gender", -1, "Gender", C1.Win.C1TrueDBGrid.AlignHorzEnum.Center, C1.Win.C1TrueDBGrid.AlignVertEnum.Top, , , , , True)
                '    .ColDesign("ItemType", -1, "Prod.Group", C1.Win.C1TrueDBGrid.AlignHorzEnum.Center, C1.Win.C1TrueDBGrid.AlignVertEnum.Top, , , , , True, , , , , , , , , , , , , "[ItemType] as [Prod.Group]")

                '    If (DelOrder = "Receiving Order Fashion" Or DelOrder = "Consignment Receiving Order Fashion") And ChkBarcode = True Then
                '        .ColDesign("Size", -1, "Size", C1.Win.C1TrueDBGrid.AlignHorzEnum.Center, C1.Win.C1TrueDBGrid.AlignVertEnum.Top, , , , , True)
                '        .ColDesign("Barcode", -1, "Barcode", C1.Win.C1TrueDBGrid.AlignHorzEnum.Center, C1.Win.C1TrueDBGrid.AlignVertEnum.Top, , , , , True)


                '    Else
                '        'For i = 1 To MaxCnt  removed by ch @ 12/12/2016 wf: 4774 
                '        For i = 1 To 30
                '            .ColDesign("Size" & i, -1, "Size" & i, C1.Win.C1TrueDBGrid.AlignHorzEnum.Center, , False, , , , , False)
                '        Next
                '        If ChkBarcode = False Then
                '            'For j = 1 To MaxCnt  removed by ch @ 12/12/2016 wf: 4774 
                '            For j = 1 To 30
                '                If j <= MaxCnt Then
                '                    .ColDesign("Qty" & j, -1, "Qty" & j, C1.Win.C1TrueDBGrid.AlignHorzEnum.Far, , , , , , , , , , , , "#,##0")
                '                Else
                '                    .ColDesign("Qty" & j, 0, "Qty" & j, C1.Win.C1TrueDBGrid.AlignHorzEnum.Far, , False, , , , , False, , , , , "#,##0")
                '                End If
                '            Next
                '        ElseIf ChkBarcode = True Then
                '            ' For k = 1 To MaxCnt  removed by ch @ 12/12/2016 wf: 4774 
                '            For k = 1 To 30
                '                If k <= MaxCnt Then
                '                    .ColDesign("Barcode" & k, -1, "Barcode" & k, C1.Win.C1TrueDBGrid.AlignHorzEnum.Center)
                '                Else
                '                    .ColDesign("Barcode" & k, 0, "Barcode" & k, C1.Win.C1TrueDBGrid.AlignHorzEnum.Center, , False, , , , , False)
                '                End If
                '            Next
                '        End If
                '    End If
                'End If
                .ColDesign("Details Status", -1, "Details Status", C1.Win.C1TrueDBGrid.AlignHorzEnum.Center, , , , , , )
                'If DelOrder = "Receiving Order Fashion" Or DelOrder = "Consignment Receiving Order Fashion" Then
                '    .ColDesign("Qty", -1, "Qty", C1.Win.C1TrueDBGrid.AlignHorzEnum.Far, , , , , , , , , , , , "#,##0")
                '    .ColDesign("Cons.Qty", -1, "Cons.Qty", C1.Win.C1TrueDBGrid.AlignHorzEnum.Far, , , , , , , , , , , , "#,##0")
                'Else
                .ColDesign("Qty", -1, "Qty", C1.Win.C1TrueDBGrid.AlignHorzEnum.Far, , , , , , , , , , , , "#,###.000")
                    .ColDesign("Cons.Qty", -1, "Cons.Qty", C1.Win.C1TrueDBGrid.AlignHorzEnum.Far, , , , , , , , , , , , "#,###.000")
                    'End If

                    .ColDesign("Unit", -1, "Unit", C1.Win.C1TrueDBGrid.AlignHorzEnum.Center, C1.Win.C1TrueDBGrid.AlignVertEnum.Top, , , , , True)
                .ColDesign("VAT Type", -1, "VAT Type", C1.Win.C1TrueDBGrid.AlignHorzEnum.Center, C1.Win.C1TrueDBGrid.AlignVertEnum.Top, , , , , True)
                .ColDesign("VAT %", -1, "VAT %", C1.Win.C1TrueDBGrid.AlignHorzEnum.Center, C1.Win.C1TrueDBGrid.AlignVertEnum.Top, , , , , True, , , , , , "#,##0")
                'If DelOrder = "Receiving Order Fashion" Or DelOrder = "Consignment Receiving Order Fashion" Then
                '    .ColDesign("Price LC", 0, "Price " & gbCurr1 & "", C1.Win.C1TrueDBGrid.AlignHorzEnum.Far, , False, , , , , False, , , , , "N" & VatLCRound, , , , , , , )
                '    .ColDesign("Price CC", 0, "Price " & gbCurr2 & "", C1.Win.C1TrueDBGrid.AlignHorzEnum.Far, , False, , , , , False, , , , , "#,##0.000", , , , , , , )
                'End If

                If ChkCost = True Then
                    If DelOrder = "Consignment Receiving Order Fashion" And ChkBarcode = True Then
                    Else
                        .ColDesign("AvgCostLC", -1, "Unit Cost " & gbCurr1 & "", C1.Win.C1TrueDBGrid.AlignHorzEnum.Far, , , , , , , , , , , , "N" & VatLCRound, , , , , , , "AvgCostLC as [AvgCost " & gbCurr1 & "]")
                        .ColDesign("AvgCostCC", -1, "Unit Cost " & gbCurr2 & "", C1.Win.C1TrueDBGrid.AlignHorzEnum.Far, , , , , , , , , , , , "#,##0.000", , , , , , , "AvgCostCC as [AvgCost " & gbCurr2 & "]")
                        If UCase(txtCurr.Trim) = gbCurr1 Or UCase(txtCurr.Trim) = gbCurr2 Then
                            .ColDesign("AvgCostFC", -1, "Unit Cost FC", C1.Win.C1TrueDBGrid.AlignHorzEnum.Far, , False, , , , , False, , , , , "#,##0.000")
                        Else
                            .ColDesign("AvgCostFC", -1, "Unit Cost FC", C1.Win.C1TrueDBGrid.AlignHorzEnum.Far, , , , , , , , , , , , "#,##0.000")
                        End If
                        .ColDesign("NetCostLC", -1, "Net Cost " & gbCurr1 & "", C1.Win.C1TrueDBGrid.AlignHorzEnum.Far, , , , , , , , , , , , "N" & VatLCRound, , , , , , )
                        .ColDesign("NetCostCC", -1, "Net Cost " & gbCurr2 & "", C1.Win.C1TrueDBGrid.AlignHorzEnum.Far, , , , , , , , , , , , "#,##0.000", , , , , , , )
                        If UCase(txtCurr.Trim) = gbCurr1 Or UCase(txtCurr.Trim) = gbCurr2 Then
                            .ColDesign("NetCostFC", -1, "Net Cost FC", C1.Win.C1TrueDBGrid.AlignHorzEnum.Far, , False, , , , , False, , , , , "#,##0.000")
                        Else
                            .ColDesign("NetCostFC", -1, "Net Cost FC", C1.Win.C1TrueDBGrid.AlignHorzEnum.Far, , , , , , , , , , , , "#,##0.000")
                        End If
                        .ColDesign("Disc", -1, "Disc %", C1.Win.C1TrueDBGrid.AlignHorzEnum.Far, , , , , , , , , , , , "#0.00", , , , , , )
                    End If
                End If
                If DelOrder = "Receiving Order" Or DelOrder = "Consignment Receiving Order" Then
                    If ChkBarcode = True And ChkExpiry = False Then
                        .ColDesign("Barcode", -1, "Barcode", C1.Win.C1TrueDBGrid.AlignHorzEnum.Center)
                    ElseIf ChkExpiry = True Then
                        .ColDesign("Lot No", -1, "Lot No", C1.Win.C1TrueDBGrid.AlignHorzEnum.Near)
                        .ColDesign("Expiry Date", -1, "Expiry Date", C1.Win.C1TrueDBGrid.AlignHorzEnum.Center, , , , , , , , , , , , "dd/MM/yyyy")
                        .ColDesign("Production Date", -1, "Production Date", C1.Win.C1TrueDBGrid.AlignHorzEnum.Center, , , , , , , , , , , , "dd/MM/yyyy")
                        .ColDesign("Barcode", -1, "Barcode", C1.Win.C1TrueDBGrid.AlignHorzEnum.Near)
                        .ColDesign("Qty Exp", -1, "Qty Exp", C1.Win.C1TrueDBGrid.AlignHorzEnum.Far, , , False)
                    End If
                End If
                If ChkCost = True Then
                    If DelOrder = "Consignment Receiving Order Fashion" And ChkBarcode = True Then

                    Else
                        .ColDesign("UNITCOSTLC", -1, "Current Cost " & gbCurr1 & "", C1.Win.C1TrueDBGrid.AlignHorzEnum.Center, , , , , , , , , , , , "N" & VatLCRound, , , , , , , "UNITCOSTLC as [UNITCOST" & gbCurr1 & "]")
                        .ColDesign("UNITCOSTCC", -1, "Current Cost " & gbCurr2 & "", C1.Win.C1TrueDBGrid.AlignHorzEnum.Center, , , , , , , , , , , , "#,##0.000", , , , , , , "UNITCOSTCC as [UNITCOST" & gbCurr2 & "]")
                        If UCase(txtCurr.Trim) = gbCurr1 Or UCase(txtCurr.Trim) = gbCurr2 Then
                            .ColDesign("UNITCOSTFC", -1, "Current Cost FC", C1.Win.C1TrueDBGrid.AlignHorzEnum.Center, , False, , , , , False, , , , , , , , , , , False)
                        Else
                            .ColDesign("UNITCOSTFC", -1, "Current Cost FC", C1.Win.C1TrueDBGrid.AlignHorzEnum.Center, , , , , , , , , , , , "#,##0.000")
                        End If
                    End If

                End If

                .ColDesign("FLQTY", -1, "", C1.Win.C1TrueDBGrid.AlignHorzEnum.Far, , , False, , , , False)
                .ColDesign("NLQTY", -1, "", C1.Win.C1TrueDBGrid.AlignHorzEnum.Far, , , False, , , , False)
                .ColDesign("FWQTY", -1, "", C1.Win.C1TrueDBGrid.AlignHorzEnum.Far, , , False, , , , False)
                .ColDesign("NWQTY", -1, "", C1.Win.C1TrueDBGrid.AlignHorzEnum.Far, , , False, , , , False)
                .ColDesign("FPQTY", -1, "", C1.Win.C1TrueDBGrid.AlignHorzEnum.Far, , , False, , , , False)
                .ColDesign("NPQTY", -1, "", C1.Win.C1TrueDBGrid.AlignHorzEnum.Far, , , False, , , , False)
                .ColDesign("RLQTY", -1, "", C1.Win.C1TrueDBGrid.AlignHorzEnum.Far, , , False, , , , False)
                .ColDesign("RWQTY", -1, "", C1.Win.C1TrueDBGrid.AlignHorzEnum.Far, , , False, , , , False)
                .ColDesign("RPQTY", -1, "", C1.Win.C1TrueDBGrid.AlignHorzEnum.Far, , , False, , , , False)

                .ColDesign("TechContent", 115, "Technical Content", C1.Win.C1TrueDBGrid.AlignHorzEnum.Near, , ChkTechContent, , , , , ChkTechContent)
                strsql = " select "
                strsql &= "sum((case when meas_type='L' and substring(MOD_stat,1,1)='F' then  MOD_ConsOrdqty else 0 end ) ) as FLQTY," '...free Length item
                strsql &= "sum((case when meas_type='L' and substring(MOD_stat,1,1)='N' then  MOD_ConsOrdqty else 0 end ) ) as NLQTY," '...normal Length item
                strsql &= "sum((case when meas_type='W' and substring(MOD_stat,1,1)='F' then  MOD_ConsOrdqty else 0 end ) ) as FWQTY," '...free weight item
                strsql &= "sum((case when meas_type='W' and substring(MOD_stat,1,1)='N' then  MOD_ConsOrdqty else 0 end ) ) as NWQTY," '...normal weight item
                strsql &= "sum((case when meas_type='P' and substring(MOD_stat,1,1)='F' then  MOD_ConsOrdqty else 0 end ) ) as FPQTY," '...free pieces item
                strsql &= "sum((case when meas_type='P' and substring(MOD_stat,1,1)='N' then  MOD_ConsOrdqty else 0 end ) ) as NPQTY," '...normal pieces item

                strsql &= "sum((case when meas_type='L' and substring(MOD_stat,1,1)='R' then  MOD_ConsOrdqty else 0 end ) ) as RLQTY,"
                strsql &= "sum((case when meas_type='W' and substring(MOD_stat,1,1)='R' then  MOD_ConsOrdqty else 0 end ) ) as RWQTY,"
                strsql &= "sum((case when meas_type='P' and substring(MOD_stat,1,1)='R' then  MOD_ConsOrdqty else 0 end ) ) as RPQTY"
                strsql &= " from " & TmpGridDetails & " inner join V_Pord2_GetStkMeas "
                strsql &= " on MOD_rmrecid =stk_recid  "

                Dim drsql As SqlDataReader
                ReadFromDBToDataReader(strsql, drsql)

                Dim rptParam(16, 1) As Object
                drsql.Read()
                rptParam(0, 0) = "FreeLQty"
                rptParam(0, 1) = (drsql.Item("FLQTY").ToString)

                rptParam(1, 0) = "FreeWQy"
                rptParam(1, 1) = (drsql.Item("FWQTY").ToString)

                rptParam(2, 0) = "FreeQt"
                rptParam(2, 1) = (drsql.Item("FPQTY").ToString)

                rptParam(3, 0) = "ReturnLQty"
                rptParam(3, 1) = (drsql.Item("RLQTY").ToString)

                rptParam(4, 0) = "ReturnWQy"
                rptParam(4, 1) = (drsql.Item("RWQTY").ToString)

                rptParam(5, 0) = "ReturnQt"
                rptParam(5, 1) = (drsql.Item("RPQTY").ToString)

                rptParam(6, 0) = "ILQty"
                rptParam(6, 1) = (drsql.Item("NLQTY").ToString)

                rptParam(7, 0) = "IWQty"
                rptParam(7, 1) = (drsql.Item("NWQTY").ToString)

                rptParam(8, 0) = "NormQty"
                rptParam(8, 1) = (drsql.Item("NPQTY").ToString)

                rptParam(9, 0) = "decno"
                rptParam(9, 1) = gbDecNo

                rptParam(12, 0) = "TITLE"
                rptParam(12, 1) = "'" & DelOrder & "'"


                rptParam(10, 0) = "Ccur1"
                rptParam(10, 1) = "'" & gbCurr1 & "'"

                rptParam(11, 0) = "Ccur2"
                rptParam(11, 1) = "'" & gbCurr2 & "'"
                rptParam(15, 0) = "TechContent"
                rptParam(15, 1) = ChkTechContent
                '  rptParam(16, 0) = "ConsQty"
                'rptParam(16, 1) = chkConsQty

                'If DelOrder = "Receiving Order" Or DelOrder = "Consignment Receiving Order" Then
                rptParam(13, 0) = "decno"
                    rptParam(13, 1) = gbDecNo

                    strsql = " Select Count(Dod_hrecid) as count from stk04_PurDelOrdD where DOD_Hrecid in " & Code
                    ReadFromDBToDataReader(strsql, drsql)
                    If drsql.HasRows Then
                        drsql.Read()
                        rptParam(14, 0) = "Count"
                        rptParam(14, 1) = drsql.Item("count").ToString
                    End If

                    If ChkCost = True Then
                        If ChkBarcode = True And ChkExpiry = False Then
                            .ShowDialog(strPrev, Ownerfrm.Name, "rptDelOrdCostBarcode.rpt", rptParam)
                        ElseIf ChkExpiry = True Then
                            .ShowDialog(strPrev, Ownerfrm.Name, "rptDelOrdCostExpiry.rpt", rptParam)
                        Else
                            .ShowDialog(strPrev, Ownerfrm.Name, "rptDelOrdWithCost.rpt", rptParam)
                        End If
                    Else
                        If ChkBarcode = True And ChkExpiry = False Then
                            .ShowDialog(strPrev, Ownerfrm.Name, "rptDelOrdBarcode.rpt", rptParam)
                        ElseIf ChkExpiry = True Then
                            .ShowDialog(strPrev, Ownerfrm.Name, "rptDelOrdExpiry.rpt", rptParam)
                        Else
                            .ShowDialog(strPrev, Ownerfrm.Name, "rptDelOrd.rpt", rptParam)
                        End If
                    End If
                    'ElseIf DelOrder = "Receiving Order Fashion" Or DelOrder = "Consignment Receiving Order Fashion" Then
                '    If ChkBarcode = True Then
                '        .ShowDialog(strPrev, Ownerfrm.Name, "rptDelOrdFashBarcode.rpt", rptParam)
                '    ElseIf ChkCost = True And ChkBarcode = False Then
                '        .ShowDialog(strPrev, Ownerfrm.Name, "rptDelOrdFashionCost.rpt", rptParam)
                '    Else
                '        .ShowDialog(strPrev, Ownerfrm.Name, "rptDelOrdFashion.rpt", rptParam)
                '    End If
                'End If
            End With
        Catch ex As Exception
            Throw ex
        Finally
            If Not frm Is Nothing Then frm = Nothing
        End Try
    End Sub

    Private Delegate Sub Printdelg()

    Private Sub PrintQCAnswers(ByVal OwnerFrm As Form, ByVal rbPrintLatin As Boolean, ByVal cntTitle As String, Optional ByVal HeadReci As String = "")
        Dim strsql As String
        Dim i As Int32

        Dim myReport As New ReportDocument
        Dim frm As frmPreviewPrint
        Dim pvCollection As New CrystalDecisions.Shared.ParameterValues
        Dim pdvParam As New CrystalDecisions.Shared.ParameterDiscreteValue
        Dim SubReports As SubReport()
        Dim tbCurrent As CrystalDecisions.CrystalReports.Engine.Table
        Dim tliCurrent As CrystalDecisions.Shared.TableLogOnInfo
        Try
            If OwnerFrm.InvokeRequired Then
                Dim d As New Printdelg(Sub() PrintQCAnswers(OwnerFrm, rbPrintLatin, cntTitle, HeadReci))
                OwnerFrm.Invoke(d)
                Exit Sub
            End If
            Cursor.Current = Cursors.WaitCursor

            If HeadReci.Trim = "" Then Exit Try
            'strsql = " SELECT DOH_Ref [RecivingRef],RankChnge ,"
            'strsql &= " ROQCH_RecID ,"
            'strsql &= " ROQCH_RqDecision ,"
            'strsql &= " ROQCH_FCode ,"
            'strsql &= " ROQCH_Desc ,"
            'strsql &= " ROQCH_Version ,"
            'strsql &= " ROQCD_Type ,"
            'strsql &= " ROQCD_DispSeq ,"
            'strsql &= " ROQCD_Mandatory ,"
            'strsql &= " ROQCD_NotifyVal ,"
            'strsql &= " ROQCD_MinVal ,"
            'strsql &= " ROQCD_maxVal ,"
            'strsql &= " ROQCD_DispEng [ROQCD_DispEng] ,"
            'strsql &= " ROQCD_Remark ,"
            'strsql &= " ROQCS_DescEng [ROQCS_DescEng]"
            'strsql &= " FROM   ( SELECT    DENSE_RANK() OVER ( PARTITION BY ROQCH_RecID ORDER BY ROQCH_RecID, DOD_Recid, DOQ_RecID ) RankChnge ,"
            'strsql &= " *"
            'strsql &= " FROM      dbo.stk04_PurDelOrdH RecHeader"
            'strsql &= " INNER JOIN dbo.stk04_PurDelOrdD Recdetails ON Recdetails.DOD_Hrecid = RecHeader.DOH_Recid"
            'strsql &= " INNER JOIN dbo.STK04_STKFIL ON STK04_STKFIL.STK_RECID = Recdetails.DOD_StkRecid"
            'strsql &= " INNER JOIN dbo.Stk04_Supplier ON Stk04_Supplier.Sup_RecId = RecHeader.DOH_SupRecid"
            'strsql &= " LEFT JOIN dbo.stk04_PurDelOrdDQty RecQTYDeat ON RecQTYDeat.DOQ_Drecid = Recdetails.DOD_Recid"
            'strsql &= " INNER   JOIN dbo.Stk04_PurDelOrdDQCH RECQCH ON RECQCH.PurQCH_PrDReciD = Recdetails.DOD_Recid"
            'strsql &= " AND ISNULL(RECQCH.PurQCH_PrQDRecId,"
            'strsql &= " 0) = ISNULL(RecQTYDeat.DOQ_RecID,"
            'strsql &= " 0)"
            'strsql &= " INNER JOIN Stk04_PurDelOrdDQCd ON Stk04_PurDelOrdDQCD.PurQCD_HRecID = RECQCH.PurQCH_ReciD"
            'strsql &= " INNER JOIN dbo.Stk04_ROQCFormH ON Stk04_ROQCFormH.ROQCH_RecID = RECQCH.PurQCH_QCHRecId"
            'strsql &= " INNER JOIN dbo.stk04_ROQCFormD ON stk04_ROQCFormD.ROQCD_HRecID = Stk04_ROQCFormH.ROQCH_RecID"
            'strsql &= " AND stk04_ROQCFormD.ROQCD_RecID = Stk04_PurDelOrdDQCD.PurQCD_QCDRecID"
            'strsql &= " LEFT JOIN dbo.stk04_ROQCFormDSub ON stk04_ROQCFormDSub.ROQCS_DRecid = stk04_ROQCFormD.ROQCD_RecID"
            'strsql &= " AND ROQCS_CurActive = 'Y'"
            'strsql &= " WHERE     DOH_Recid = " & HeadReci & ""
            'strsql &= " ) t"
            'strsql &= " WHERE  RankChnge % 6 = 1"
            'strsql &= " AND ROQCD_Active = 'Y'"
            'strsql &= " ORDER BY ROQCH_RecID ,"
            'strsql &= " ROQCD_DispSeq"





            strsql = " SELECT DISTINCT"
            strsql &= "  DOH_Dstamp [LstRvDate] , DOH_Ref [RecivingRef] ,"
            strsql &= "   DENSE_RANK() OVER ( PARTITION BY ROQCH_RecID,ROQCH_RecID ORDER BY myRank,ROQCH_RecID )     RankChnge , "
            strsql &= " ROQCH_RecID ,"
            strsql &= " ROQCH_RqDecision ,"
            strsql &= " ROQCH_FCode ,"
            strsql &= " ROQCH_Desc ,"
            strsql &= " ROQCH_Version ,"
            strsql &= " ROQCD_Type ,"
            strsql &= " ROQCD_DispSeq ,"
            strsql &= " ROQCD_Mandatory ,"
            strsql &= " ROQCD_NotifyVal ,"
            strsql &= " ROQCD_MinVal ,"
            strsql &= " ROQCD_maxVal ,"
            If rbPrintLatin = True Then
                strsql &= " ROQCD_DispEng [ROQCD_DispEng] ,"
            Else
                strsql &= " isnull(ROQCD_DispArb,ROQCD_DispEng) [ROQCD_DispEng] ,"
            End If

            strsql &= " ROQCD_Remark ,"
            If rbPrintLatin = True Then
                strsql &= " ROQCS_DescEng [ROQCS_DescEng] "
            Else
                strsql &= " isnull(ROQCS_DescArb,ROQCS_DescEng) [ROQCS_DescEng] "
            End If

            strsql &= " ,ROQCD_RecID "
            strsql &= " FROM   ( SELECT    DENSE_RANK() OVER ( PARTITION BY ROQCH_RecID ORDER BY ROQCH_RecID, DOD_Recid, DOQ_RecID ) myRank ,"
            strsql &= " *"
            strsql &= " FROM      dbo.stk04_PurDelOrdH RecHeader"
            strsql &= " INNER JOIN dbo.stk04_PurDelOrdD Recdetails ON Recdetails.DOD_Hrecid = RecHeader.DOH_Recid"
            strsql &= " INNER JOIN dbo.STK04_STKFIL ON STK04_STKFIL.STK_RECID = Recdetails.DOD_StkRecid"
            strsql &= " INNER JOIN dbo.Stk04_Supplier ON Stk04_Supplier.Sup_RecId = RecHeader.DOH_SupRecid"
            strsql &= " LEFT JOIN dbo.stk04_PurDelOrdDQty RecQTYDeat ON RecQTYDeat.DOQ_Drecid = Recdetails.DOD_Recid"
            strsql &= " INNER   JOIN dbo.Stk04_PurDelOrdDQCH RECQCH ON RECQCH.PurQCH_PrDReciD = Recdetails.DOD_Recid"
            strsql &= " AND ISNULL(RECQCH.PurQCH_PrQDRecId,"
            strsql &= " 0) = ISNULL(RecQTYDeat.DOQ_RecID,"
            strsql &= " 0)"
            strsql &= " INNER JOIN Stk04_PurDelOrdDQCd ON Stk04_PurDelOrdDQCD.PurQCD_HRecID = RECQCH.PurQCH_ReciD"
            strsql &= " INNER JOIN dbo.Stk04_ROQCFormH ON Stk04_ROQCFormH.ROQCH_RecID = RECQCH.PurQCH_QCHRecId"
            strsql &= " INNER JOIN ( SELECT *"
            strsql &= " FROM   dbo.stk04_ROQCFormD"
            strsql &= " UNION ALL"
            strsql &= " SELECT	DISTINCT"
            strsql &= " -1 ,"
            strsql &= " [Subhead].ROQCd_hRecID ,"
            strsql &= " '1' ,"
            strsql &= " 'y' ,"
            strsql &= " 'Orderd Quantity' ,"
            strsql &= " '' ,"
            strsql &= " '-1' ,"
            strsql &= " '' ,"
            strsql &= " '' ,"
            strsql &= " '' ,"
            strsql &= " NULL ,"
            strsql &= " NULL ,"
            strsql &= " NULL ,"
            strsql &= " GETDATE() ,"
            strsql &= " 'SU' ,"
            strsql &= " GETDATE() ,"
            strsql &= " 'SU'"
            strsql &= " FROM   stk04_ROQCFormD [Subhead]"
            If gbExpiry = True Then
                strsql &= " UNION ALL"
                strsql &= " SELECT	DISTINCT"
                strsql &= " -2 ,"
                strsql &= " [Subhead].ROQCd_hRecID ,"
                strsql &= " '1' ,"
                strsql &= " 'y' ,"
                strsql &= " 'Lot #' ,"
                strsql &= " '' ,"
                strsql &= " '-2' ,"
                strsql &= " '' ,"
                strsql &= " '' ,"
                strsql &= " '' ,"
                strsql &= " NULL ,"
                strsql &= " NULL ,"
                strsql &= " NULL ,"
                strsql &= " GETDATE() ,"
                strsql &= " 'SU' ,"
                strsql &= " GETDATE() ,"
                strsql &= " 'SU'"
                strsql &= " FROM   stk04_ROQCFormD [Subhead]"
                strsql &= " UNION ALL"
                strsql &= " SELECT	DISTINCT"
                strsql &= " -3 ,"
                strsql &= " [Subhead].ROQCd_hRecID ,"
                strsql &= " '1' ,"
                strsql &= " 'y' ,"
                strsql &= " 'Expiry Date' ,"
                strsql &= " '' ,"
                strsql &= " '-3' ,"
                strsql &= " '' ,"
                strsql &= " '' ,"
                strsql &= " '' ,"
                strsql &= " NULL ,"
                strsql &= " NULL ,"
                strsql &= " NULL ,"
                strsql &= " GETDATE() ,"
                strsql &= " 'SU' ,"
                strsql &= " GETDATE() ,"
                strsql &= " 'SU'"
                strsql &= " FROM   stk04_ROQCFormD [Subhead]"
            End If

            strsql &= " UNION ALL"
            strsql &= " SELECT	DISTINCT"
            strsql &= " -4 ,"
            strsql &= " [Subhead].ROQCd_hRecID ,"
            strsql &= " '1' ,"
            strsql &= " 'y' ,"
            strsql &= " 'Date' ,"
            strsql &= " '' ,"
            strsql &= " '-4' ,"
            strsql &= " '' ,"
            strsql &= " '' ,"
            strsql &= " '' ,"
            strsql &= " NULL ,"
            strsql &= " NULL ,"
            strsql &= " NULL ,"
            strsql &= " GETDATE() ,"
            strsql &= " 'SU' ,"
            strsql &= " GETDATE() ,"
            strsql &= " 'SU'"
            strsql &= " FROM   stk04_ROQCFormD [Subhead]"
            strsql &= " ) stk04_ROQCFormD ON stk04_ROQCFormD.ROQCD_HRecID = Stk04_ROQCFormH.ROQCH_RecID"
            strsql &= " AND ( stk04_ROQCFormD.ROQCD_RecID = Stk04_PurDelOrdDQCD.PurQCD_QCDRecID"
            strsql &= " OR ROQCD_RecID IN ("
            strsql &= " -1, -2, -3, -4 )"
            strsql &= " )"
            strsql &= " LEFT JOIN dbo.stk04_ROQCFormDSub ON stk04_ROQCFormDSub.ROQCS_DRecid = stk04_ROQCFormD.ROQCD_RecID"
            strsql &= " AND ROQCS_CurActive = 'Y'"
            strsql &= " WHERE     DOH_Recid IN " & HeadReci & ""
            strsql &= " ) t"
            strsql &= " WHERE  myRank % 6 = 1"
            strsql &= " AND ROQCD_Active = 'Y'"
            strsql &= " ORDER BY ROQCH_RecID ,"
            strsql &= " ROQCD_DispSeq"
            If RecordsExists(strsql) = False Then Exit Try

            With myReport
#If DEBUG Then
                .Load(gbExePath & "\..\..\PurReports\rptQCFormPrinting.rpt")
#Else
                .Load(gbExePath & "\PurReports\rptQCFormPrinting.rpt")
#End If
                .DataSourceConnections(0).SetConnection(gbServer, gbDBname, "SA", gbSQLpswd)
                .SetParameterValue("@strsql", strsql)


            End With
            ' the below 6 Sub Report Is Used Rank % 6 to divid the item into 6 coluns 
            ' Dense_Rank() SubCounter is used to to Divid the 6 coluns in diffren items 
            ' SubReport 1 ---------------------------------------------------------------------------------------------

            strsql = " SELECT  DENSE_RANK() OVER ( ORDER BY RankChnge ) [SubCounter] ,"
            strsql &= " RankChnge ,"
            strsql &= " ROQCH_RecID ,"
            strsql &= " ROQCH_RqDecision ,"
            strsql &= " ROQCH_FCode ,"
            strsql &= " ROQCH_Desc ,"
            strsql &= " ROQCH_Version ,"
            strsql &= " ROQCD_Type ,"
            strsql &= " ROQCD_DispSeq ,"
            strsql &= " ROQCD_Mandatory ,"
            strsql &= " ROQCD_NotifyVal ,"
            strsql &= " ROQCD_MinVal ,"
            strsql &= " ROQCD_maxVal ,"
            ' strsql &= " ROQCD_DispEng [ROQCD_DispEng] ,"
            If rbPrintLatin = True Then
                strsql &= " ROQCD_DispEng [ROQCD_DispEng] ,"
            Else
                strsql &= " isnull(ROQCD_DispArb,ROQCD_DispEng) [ROQCD_DispEng] ,"
            End If
            strsql &= " ROQCD_Remark ,"
            '    strsql &= " ROQCS_DescEng [ROQCS_DescEng] ,"
            If rbPrintLatin = True Then
                strsql &= " ROQCS_DescEng [ROQCS_DescEng] "
            Else
                strsql &= " isnull(ROQCS_DescArb,ROQCS_DescEng) [ROQCS_DescEng] "
            End If
            strsql &= " , STK_RECID ,"
            strsql &= " STK_REF ,"
            strsql &= " STK_DESC ,"
            strsql &= " Sup_Name ,"
            strsql &= " PurQCD_AnsYN ,"
            strsql &= " PurQCD_AnsDate ,"
            strsql &= " PurQCD_Val ,"
            strsql &= " PurQCD_Remrk ,"
            strsql &= " ROQCS_Choice ,"
            strsql &= " CASE WHEN CHARINDEX('|' + CONVERT(VARCHAR, ROQCS_Choice) + '|',"
            strsql &= " '|||' + CONVERT(VARCHAR, PurQCD_MultCh) + '|', 0) <> 0"
            strsql &= " THEN 'Y'"
            strsql &= " ELSE 'N'"
            strsql &= " END [answerMult] ,"
            strsql &= " PurQCD_MultCh ,"
            strsql &= " CHARINDEX('|' + CONVERT(VARCHAR, ROQCS_Choice) + '|',"
            strsql &= " '|||' + CONVERT(VARCHAR, PurQCD_MultCh) + '|', 0) ,"
            strsql &= " PurQCH_AccQty ,"
            strsql &= " PurQCH_RjQty ,"
            strsql &= " PurQCH_Reson , DOQ_ExpiryDate, DOQ_BatchInfo, DOH_Date"
            strsql &= "   , CASE WHEN ISNULL(PurQCH_AccQty,0) + ISNULL(PurQCH_RjQty,0)"
            strsql &= "	=0 THEN ISNULL(DOQ_Recqty,DOD_Recqty) ELSE ISNULL(PurQCH_AccQty,0) + ISNULL(PurQCH_RjQty,0) END [OrderdQty]"

            strsql &= " FROM    ( SELECT    DENSE_RANK() OVER (PARTITION BY ROQCH_RecID ORDER BY ROQCH_RecID, DOD_Recid, DOQ_RecID ) RankChnge ,"
            strsql &= " *"
            strsql &= " FROM      dbo.stk04_PurDelOrdH RecHeader"
            strsql &= " INNER JOIN dbo.stk04_PurDelOrdD Recdetails ON Recdetails.DOD_Hrecid = RecHeader.DOH_Recid"
            strsql &= " INNER JOIN dbo.STK04_STKFIL ON STK04_STKFIL.STK_RECID = Recdetails.DOD_StkRecid"
            strsql &= " INNER JOIN dbo.Stk04_Supplier ON Stk04_Supplier.Sup_RecId = RecHeader.DOH_SupRecid"
            strsql &= " LEFT JOIN dbo.stk04_PurDelOrdDQty RecQTYDeat ON RecQTYDeat.DOQ_Drecid = Recdetails.DOD_Recid"
            strsql &= " INNER   JOIN dbo.Stk04_PurDelOrdDQCH RECQCH ON RECQCH.PurQCH_PrDReciD = Recdetails.DOD_Recid"
            strsql &= " AND ISNULL(RECQCH.PurQCH_PrQDRecId,"
            strsql &= " 0) = ISNULL(RecQTYDeat.DOQ_RecID,"
            strsql &= " 0)"
            strsql &= " INNER JOIN Stk04_PurDelOrdDQCd ON Stk04_PurDelOrdDQCD.PurQCD_HRecID = RECQCH.PurQCH_ReciD"
            strsql &= " INNER JOIN dbo.Stk04_ROQCFormH ON Stk04_ROQCFormH.ROQCH_RecID = RECQCH.PurQCH_QCHRecId"
            strsql &= " INNER JOIN dbo.stk04_ROQCFormD ON stk04_ROQCFormD.ROQCD_HRecID = Stk04_ROQCFormH.ROQCH_RecID"
            strsql &= " AND stk04_ROQCFormD.ROQCD_RecID = Stk04_PurDelOrdDQCD.PurQCD_QCDRecID"
            strsql &= " LEFT JOIN dbo.stk04_ROQCFormDSub ON stk04_ROQCFormDSub.ROQCS_DRecid = stk04_ROQCFormD.ROQCD_RecID"
            strsql &= " WHERE     DOH_Recid in " & HeadReci & ""
            strsql &= " ) t"
            strsql &= " WHERE   RankChnge % 6 = 1"
            strsql &= " ORDER BY DOD_Recid ,"
            strsql &= " DOQ_RecID ,"
            strsql &= " ROQCD_DispSeq"
            pdvParam.Value = strsql
            pvCollection.Add(pdvParam)
            myReport.Subreports("Sub1").DataDefinition.ParameterFields("@strsqlSub1").ApplyCurrentValues(pvCollection)
            pvCollection.Clear()
            myReport.SetParameterValue("@strsqlSub1", strsql, "Sub1")
            ' ---------------------------------------------------------------------------------------------------------------
            ' SubReport 2 ---------------------------------------------------------------------------------------------
            strsql = " SELECT  DENSE_RANK() OVER ( ORDER BY RankChnge ) [SubCounter] ,"
            strsql &= " RankChnge ,"
            strsql &= " ROQCH_RecID ,"
            strsql &= " ROQCH_RqDecision ,"
            strsql &= " ROQCH_FCode ,"
            strsql &= " ROQCH_Desc ,"
            strsql &= " ROQCH_Version ,"
            strsql &= " ROQCD_Type ,"
            strsql &= " ROQCD_DispSeq ,"
            strsql &= " ROQCD_Mandatory ,"
            strsql &= " ROQCD_NotifyVal ,"
            strsql &= " ROQCD_MinVal ,"
            strsql &= " ROQCD_maxVal ,"
            '   strsql &= " ROQCD_DispEng [ROQCD_DispEng] ,"
            If rbPrintLatin = True Then
                strsql &= " ROQCD_DispEng [ROQCD_DispEng] ,"
            Else
                strsql &= " isnull(ROQCD_DispArb,ROQCD_DispEng) [ROQCD_DispEng] ,"
            End If
            strsql &= " ROQCD_Remark ,"
            '      strsql &= " ROQCS_DescEng [ROQCS_DescEng] ,"
            If rbPrintLatin = True Then
                strsql &= " ROQCS_DescEng [ROQCS_DescEng] "
            Else
                strsql &= " isnull(ROQCS_DescArb,ROQCS_DescEng) [ROQCS_DescEng] "
            End If
            strsql &= "  , STK_RECID ,"
            strsql &= " STK_REF ,"
            strsql &= " STK_DESC ,"
            strsql &= " Sup_Name ,"
            strsql &= " PurQCD_AnsYN ,"
            strsql &= " PurQCD_AnsDate ,"
            strsql &= " PurQCD_Val ,"
            strsql &= " PurQCD_Remrk ,"
            strsql &= " ROQCS_Choice ,"
            strsql &= " CASE WHEN CHARINDEX('|' + CONVERT(VARCHAR, ROQCS_Choice) + '|',"
            strsql &= " '|||' + CONVERT(VARCHAR, PurQCD_MultCh) + '|', 0) <> 0"
            strsql &= " THEN 'Y'"
            strsql &= " ELSE 'N'"
            strsql &= " END [answerMult] ,"
            strsql &= " PurQCD_MultCh ,"
            strsql &= " CHARINDEX('|' + CONVERT(VARCHAR, ROQCS_Choice) + '|',"
            strsql &= " '|||' + CONVERT(VARCHAR, PurQCD_MultCh) + '|', 0) ,"
            strsql &= " PurQCH_AccQty ,"
            strsql &= " PurQCH_RjQty ,"
            strsql &= " PurQCH_Reson , DOQ_ExpiryDate, DOQ_BatchInfo, DOH_Date"
            strsql &= "   , CASE WHEN ISNULL(PurQCH_AccQty,0) + ISNULL(PurQCH_RjQty,0)"
            strsql &= "	=0 THEN ISNULL(DOQ_Recqty,DOD_Recqty) ELSE ISNULL(PurQCH_AccQty,0) + ISNULL(PurQCH_RjQty,0) END [OrderdQty]"

            strsql &= " FROM    ( SELECT    DENSE_RANK() OVER (PARTITION BY ROQCH_RecID ORDER BY ROQCH_RecID, DOD_Recid, DOQ_RecID ) RankChnge ,"
            strsql &= " *"
            strsql &= " FROM      dbo.stk04_PurDelOrdH RecHeader"
            strsql &= " INNER JOIN dbo.stk04_PurDelOrdD Recdetails ON Recdetails.DOD_Hrecid = RecHeader.DOH_Recid"
            strsql &= " INNER JOIN dbo.STK04_STKFIL ON STK04_STKFIL.STK_RECID = Recdetails.DOD_StkRecid"
            strsql &= " INNER JOIN dbo.Stk04_Supplier ON Stk04_Supplier.Sup_RecId = RecHeader.DOH_SupRecid"
            strsql &= " LEFT JOIN dbo.stk04_PurDelOrdDQty RecQTYDeat ON RecQTYDeat.DOQ_Drecid = Recdetails.DOD_Recid"
            strsql &= " INNER   JOIN dbo.Stk04_PurDelOrdDQCH RECQCH ON RECQCH.PurQCH_PrDReciD = Recdetails.DOD_Recid"
            strsql &= " AND ISNULL(RECQCH.PurQCH_PrQDRecId,"
            strsql &= " 0) = ISNULL(RecQTYDeat.DOQ_RecID,"
            strsql &= " 0)"
            strsql &= " INNER JOIN Stk04_PurDelOrdDQCd ON Stk04_PurDelOrdDQCD.PurQCD_HRecID = RECQCH.PurQCH_ReciD"
            strsql &= " INNER JOIN dbo.Stk04_ROQCFormH ON Stk04_ROQCFormH.ROQCH_RecID = RECQCH.PurQCH_QCHRecId"
            strsql &= " INNER JOIN dbo.stk04_ROQCFormD ON stk04_ROQCFormD.ROQCD_HRecID = Stk04_ROQCFormH.ROQCH_RecID"
            strsql &= " AND stk04_ROQCFormD.ROQCD_RecID = Stk04_PurDelOrdDQCD.PurQCD_QCDRecID"
            strsql &= " LEFT JOIN dbo.stk04_ROQCFormDSub ON stk04_ROQCFormDSub.ROQCS_DRecid = stk04_ROQCFormD.ROQCD_RecID"
            strsql &= " WHERE     DOH_Recid in " & HeadReci & ""
            strsql &= " ) t"
            strsql &= " WHERE   RankChnge % 6 = 2"
            strsql &= " ORDER BY DOD_Recid ,"
            strsql &= " DOQ_RecID ,"
            strsql &= " ROQCD_DispSeq"
            pdvParam.Value = strsql
            pvCollection.Add(pdvParam)
            myReport.Subreports("Sub2").DataDefinition.ParameterFields("@strsqlSub2").ApplyCurrentValues(pvCollection)
            pvCollection.Clear()
            myReport.SetParameterValue("@strsqlSub2", strsql, "Sub2")
            ' ---------------------------------------------------------------------------------------------------------------
            ' SubReport 3 ---------------------------------------------------------------------------------------------
            strsql = " SELECT  DENSE_RANK() OVER ( ORDER BY RankChnge ) [SubCounter] ,"
            strsql &= " RankChnge ,"
            strsql &= " ROQCH_RecID ,"
            strsql &= " ROQCH_RqDecision ,"
            strsql &= " ROQCH_FCode ,"
            strsql &= " ROQCH_Desc ,"
            strsql &= " ROQCH_Version ,"
            strsql &= " ROQCD_Type ,"
            strsql &= " ROQCD_DispSeq ,"
            strsql &= " ROQCD_Mandatory ,"
            strsql &= " ROQCD_NotifyVal ,"
            strsql &= " ROQCD_MinVal ,"
            strsql &= " ROQCD_maxVal ,"
            '   strsql &= " ROQCD_DispEng [ROQCD_DispEng] ,"
            If rbPrintLatin = True Then
                strsql &= " ROQCD_DispEng [ROQCD_DispEng] ,"
            Else
                strsql &= " isnull(ROQCD_DispArb,ROQCD_DispEng) [ROQCD_DispEng] ,"
            End If
            strsql &= " ROQCD_Remark ,"
            '   strsql &= " ROQCS_DescEng [ROQCS_DescEng] ,"
            If rbPrintLatin = True Then
                strsql &= " ROQCS_DescEng [ROQCS_DescEng] "
            Else
                strsql &= " isnull(ROQCS_DescArb,ROQCS_DescEng) [ROQCS_DescEng] "
            End If
            strsql &= ", STK_RECID ,"
            strsql &= " STK_REF ,"
            strsql &= " STK_DESC ,"
            strsql &= " Sup_Name ,"
            strsql &= " PurQCD_AnsYN ,"
            strsql &= " PurQCD_AnsDate ,"
            strsql &= " PurQCD_Val ,"
            strsql &= " PurQCD_Remrk ,"
            strsql &= " ROQCS_Choice ,"
            strsql &= " CASE WHEN CHARINDEX('|' + CONVERT(VARCHAR, ROQCS_Choice) + '|',"
            strsql &= " '|||' + CONVERT(VARCHAR, PurQCD_MultCh) + '|', 0) <> 0"
            strsql &= " THEN 'Y'"
            strsql &= " ELSE 'N'"
            strsql &= " END [answerMult] ,"
            strsql &= " PurQCD_MultCh ,"
            strsql &= " CHARINDEX('|' + CONVERT(VARCHAR, ROQCS_Choice) + '|',"
            strsql &= " '|||' + CONVERT(VARCHAR, PurQCD_MultCh) + '|', 0) ,"
            strsql &= " PurQCH_AccQty ,"
            strsql &= " PurQCH_RjQty ,"
            strsql &= " PurQCH_Reson , DOQ_ExpiryDate, DOQ_BatchInfo, DOH_Date"
            strsql &= "   , CASE WHEN ISNULL(PurQCH_AccQty,0) + ISNULL(PurQCH_RjQty,0)"
            strsql &= "	=0 THEN ISNULL(DOQ_Recqty,DOD_Recqty) ELSE ISNULL(PurQCH_AccQty,0) + ISNULL(PurQCH_RjQty,0) END [OrderdQty]"

            strsql &= " FROM    ( SELECT   DENSE_RANK() OVER (PARTITION BY ROQCH_RecID ORDER BY ROQCH_RecID, DOD_Recid, DOQ_RecID ) RankChnge ,"
            strsql &= " *"
            strsql &= " FROM      dbo.stk04_PurDelOrdH RecHeader"
            strsql &= " INNER JOIN dbo.stk04_PurDelOrdD Recdetails ON Recdetails.DOD_Hrecid = RecHeader.DOH_Recid"
            strsql &= " INNER JOIN dbo.STK04_STKFIL ON STK04_STKFIL.STK_RECID = Recdetails.DOD_StkRecid"
            strsql &= " INNER JOIN dbo.Stk04_Supplier ON Stk04_Supplier.Sup_RecId = RecHeader.DOH_SupRecid"
            strsql &= " LEFT JOIN dbo.stk04_PurDelOrdDQty RecQTYDeat ON RecQTYDeat.DOQ_Drecid = Recdetails.DOD_Recid"
            strsql &= " INNER   JOIN dbo.Stk04_PurDelOrdDQCH RECQCH ON RECQCH.PurQCH_PrDReciD = Recdetails.DOD_Recid"
            strsql &= " AND ISNULL(RECQCH.PurQCH_PrQDRecId,"
            strsql &= " 0) = ISNULL(RecQTYDeat.DOQ_RecID,"
            strsql &= " 0)"
            strsql &= " INNER JOIN Stk04_PurDelOrdDQCd ON Stk04_PurDelOrdDQCD.PurQCD_HRecID = RECQCH.PurQCH_ReciD"
            strsql &= " INNER JOIN dbo.Stk04_ROQCFormH ON Stk04_ROQCFormH.ROQCH_RecID = RECQCH.PurQCH_QCHRecId"
            strsql &= " INNER JOIN dbo.stk04_ROQCFormD ON stk04_ROQCFormD.ROQCD_HRecID = Stk04_ROQCFormH.ROQCH_RecID"
            strsql &= " AND stk04_ROQCFormD.ROQCD_RecID = Stk04_PurDelOrdDQCD.PurQCD_QCDRecID"
            strsql &= " LEFT JOIN dbo.stk04_ROQCFormDSub ON stk04_ROQCFormDSub.ROQCS_DRecid = stk04_ROQCFormD.ROQCD_RecID"
            strsql &= " WHERE     DOH_Recid in " & HeadReci & ""
            strsql &= " ) t"
            strsql &= " WHERE   RankChnge % 6 = 3"
            strsql &= " ORDER BY DOD_Recid ,"
            strsql &= " DOQ_RecID ,"
            strsql &= " ROQCD_DispSeq"

            pdvParam.Value = strsql
            pvCollection.Add(pdvParam)
            myReport.Subreports("Sub3").DataDefinition.ParameterFields("@strsqlSub3").ApplyCurrentValues(pvCollection)
            pvCollection.Clear()
            myReport.SetParameterValue("@strsqlSub3", strsql, "Sub3")

            ' ---------------------------------------------------------------------------------------------------------------
            ' SubReport 4 ---------------------------------------------------------------------------------------------
            strsql = " SELECT  DENSE_RANK() OVER ( ORDER BY RankChnge ) [SubCounter] ,"
            strsql &= " RankChnge ,"
            strsql &= " ROQCH_RecID ,"
            strsql &= " ROQCH_RqDecision ,"
            strsql &= " ROQCH_FCode ,"
            strsql &= " ROQCH_Desc ,"
            strsql &= " ROQCH_Version ,"
            strsql &= " ROQCD_Type ,"
            strsql &= " ROQCD_DispSeq ,"
            strsql &= " ROQCD_Mandatory ,"
            strsql &= " ROQCD_NotifyVal ,"
            strsql &= " ROQCD_MinVal ,"
            strsql &= " ROQCD_maxVal ,"
            '  strsql &= " ROQCD_DispEng [ROQCD_DispEng] ,"
            If rbPrintLatin = True Then
                strsql &= " ROQCD_DispEng [ROQCD_DispEng] ,"
            Else
                strsql &= " isnull(ROQCD_DispArb,ROQCD_DispEng) [ROQCD_DispEng] ,"
            End If
            strsql &= " ROQCD_Remark ,"
            '   strsql &= " ROQCS_DescEng [ROQCS_DescEng] ,"
            If rbPrintLatin = True Then
                strsql &= " ROQCS_DescEng [ROQCS_DescEng] "
            Else
                strsql &= " isnull(ROQCS_DescArb,ROQCS_DescEng) [ROQCS_DescEng] "
            End If
            strsql &= " , STK_RECID ,"
            strsql &= " STK_REF ,"
            strsql &= " STK_DESC ,"
            strsql &= " Sup_Name ,"
            strsql &= " PurQCD_AnsYN ,"
            strsql &= " PurQCD_AnsDate ,"
            strsql &= " PurQCD_Val ,"
            strsql &= " PurQCD_Remrk ,"
            strsql &= " ROQCS_Choice ,"
            strsql &= " CASE WHEN CHARINDEX('|' + CONVERT(VARCHAR, ROQCS_Choice) + '|',"
            strsql &= " '|||' + CONVERT(VARCHAR, PurQCD_MultCh) + '|', 0) <> 0"
            strsql &= " THEN 'Y'"
            strsql &= " ELSE 'N'"
            strsql &= " END [answerMult] ,"
            strsql &= " PurQCD_MultCh ,"
            strsql &= " CHARINDEX('|' + CONVERT(VARCHAR, ROQCS_Choice) + '|',"
            strsql &= " '|||' + CONVERT(VARCHAR, PurQCD_MultCh) + '|', 0) ,"
            strsql &= " PurQCH_AccQty ,"
            strsql &= " PurQCH_RjQty ,"
            strsql &= " PurQCH_Reson , DOQ_ExpiryDate, DOQ_BatchInfo, DOH_Date"
            strsql &= "   , CASE WHEN ISNULL(PurQCH_AccQty,0) + ISNULL(PurQCH_RjQty,0)"
            strsql &= "	=0 THEN ISNULL(DOQ_Recqty,DOD_Recqty) ELSE ISNULL(PurQCH_AccQty,0) + ISNULL(PurQCH_RjQty,0) END [OrderdQty]"

            strsql &= " FROM    ( SELECT   DENSE_RANK() OVER (PARTITION BY ROQCH_RecID ORDER BY ROQCH_RecID, DOD_Recid, DOQ_RecID ) RankChnge ,"
            strsql &= " *"
            strsql &= " FROM      dbo.stk04_PurDelOrdH RecHeader"
            strsql &= " INNER JOIN dbo.stk04_PurDelOrdD Recdetails ON Recdetails.DOD_Hrecid = RecHeader.DOH_Recid"
            strsql &= " INNER JOIN dbo.STK04_STKFIL ON STK04_STKFIL.STK_RECID = Recdetails.DOD_StkRecid"
            strsql &= " INNER JOIN dbo.Stk04_Supplier ON Stk04_Supplier.Sup_RecId = RecHeader.DOH_SupRecid"
            strsql &= " LEFT JOIN dbo.stk04_PurDelOrdDQty RecQTYDeat ON RecQTYDeat.DOQ_Drecid = Recdetails.DOD_Recid"
            strsql &= " INNER   JOIN dbo.Stk04_PurDelOrdDQCH RECQCH ON RECQCH.PurQCH_PrDReciD = Recdetails.DOD_Recid"
            strsql &= " AND ISNULL(RECQCH.PurQCH_PrQDRecId,"
            strsql &= " 0) = ISNULL(RecQTYDeat.DOQ_RecID,"
            strsql &= " 0)"
            strsql &= " INNER JOIN Stk04_PurDelOrdDQCd ON Stk04_PurDelOrdDQCD.PurQCD_HRecID = RECQCH.PurQCH_ReciD"
            strsql &= " INNER JOIN dbo.Stk04_ROQCFormH ON Stk04_ROQCFormH.ROQCH_RecID = RECQCH.PurQCH_QCHRecId"
            strsql &= " INNER JOIN dbo.stk04_ROQCFormD ON stk04_ROQCFormD.ROQCD_HRecID = Stk04_ROQCFormH.ROQCH_RecID"
            strsql &= " AND stk04_ROQCFormD.ROQCD_RecID = Stk04_PurDelOrdDQCD.PurQCD_QCDRecID"
            strsql &= " LEFT JOIN dbo.stk04_ROQCFormDSub ON stk04_ROQCFormDSub.ROQCS_DRecid = stk04_ROQCFormD.ROQCD_RecID"
            strsql &= " WHERE     DOH_Recid in " & HeadReci & ""
            strsql &= " ) t"
            strsql &= " WHERE   RankChnge % 6 = 4"
            strsql &= " ORDER BY DOD_Recid ,"
            strsql &= " DOQ_RecID ,"
            strsql &= " ROQCD_DispSeq"

            pdvParam.Value = strsql
            pvCollection.Add(pdvParam)
            myReport.Subreports("Sub4").DataDefinition.ParameterFields("@strsqlSub4").ApplyCurrentValues(pvCollection)
            pvCollection.Clear()
            myReport.SetParameterValue("@strsqlSub4", strsql, "Sub4")
            ' ---------------------------------------------------------------------------------------------------------------
            ' SubReport 5 ---------------------------------------------------------------------------------------------
            strsql = " SELECT  DENSE_RANK() OVER ( ORDER BY RankChnge ) [SubCounter] ,"
            strsql &= " RankChnge ,"
            strsql &= " ROQCH_RecID ,"
            strsql &= " ROQCH_RqDecision ,"
            strsql &= " ROQCH_FCode ,"
            strsql &= " ROQCH_Desc ,"
            strsql &= " ROQCH_Version ,"
            strsql &= " ROQCD_Type ,"
            strsql &= " ROQCD_DispSeq ,"
            strsql &= " ROQCD_Mandatory ,"
            strsql &= " ROQCD_NotifyVal ,"
            strsql &= " ROQCD_MinVal ,"
            strsql &= " ROQCD_maxVal ,"
            '   strsql &= " ROQCD_DispEng [ROQCD_DispEng] ,"
            If rbPrintLatin = True Then
                strsql &= " ROQCD_DispEng [ROQCD_DispEng] ,"
            Else
                strsql &= " isnull(ROQCD_DispArb,ROQCD_DispEng) [ROQCD_DispEng] ,"
            End If
            strsql &= " ROQCD_Remark ,"
            '    strsql &= " ROQCS_DescEng [ROQCS_DescEng] ,"
            If rbPrintLatin = True Then
                strsql &= " ROQCS_DescEng [ROQCS_DescEng] "
            Else
                strsql &= " isnull(ROQCS_DescArb,ROQCS_DescEng) [ROQCS_DescEng] "
            End If

            strsql &= " , STK_RECID ,"
            strsql &= " STK_REF ,"
            strsql &= " STK_DESC ,"
            strsql &= " Sup_Name ,"
            strsql &= " PurQCD_AnsYN ,"
            strsql &= " PurQCD_AnsDate ,"
            strsql &= " PurQCD_Val ,"
            strsql &= " PurQCD_Remrk ,"
            strsql &= " ROQCS_Choice ,"
            strsql &= " CASE WHEN CHARINDEX('|' + CONVERT(VARCHAR, ROQCS_Choice) + '|',"
            strsql &= " '|||' + CONVERT(VARCHAR, PurQCD_MultCh) + '|', 0) <> 0"
            strsql &= " THEN 'Y'"
            strsql &= " ELSE 'N'"
            strsql &= " END [answerMult] ,"
            strsql &= " PurQCD_MultCh ,"
            strsql &= " CHARINDEX('|' + CONVERT(VARCHAR, ROQCS_Choice) + '|',"
            strsql &= " '|||' + CONVERT(VARCHAR, PurQCD_MultCh) + '|', 0) ,"
            strsql &= " PurQCH_AccQty ,"
            strsql &= " PurQCH_RjQty ,"
            strsql &= " PurQCH_Reson , DOQ_ExpiryDate, DOQ_BatchInfo, DOH_Date"
            strsql &= "   , CASE WHEN ISNULL(PurQCH_AccQty,0) + ISNULL(PurQCH_RjQty,0)"
            strsql &= "	=0 THEN ISNULL(DOQ_Recqty,DOD_Recqty) ELSE ISNULL(PurQCH_AccQty,0) + ISNULL(PurQCH_RjQty,0) END [OrderdQty]"

            strsql &= " FROM    ( SELECT    DENSE_RANK() OVER (PARTITION BY ROQCH_RecID ORDER BY ROQCH_RecID, DOD_Recid, DOQ_RecID ) RankChnge ,"
            strsql &= " *"
            strsql &= " FROM      dbo.stk04_PurDelOrdH RecHeader"
            strsql &= " INNER JOIN dbo.stk04_PurDelOrdD Recdetails ON Recdetails.DOD_Hrecid = RecHeader.DOH_Recid"
            strsql &= " INNER JOIN dbo.STK04_STKFIL ON STK04_STKFIL.STK_RECID = Recdetails.DOD_StkRecid"
            strsql &= " INNER JOIN dbo.Stk04_Supplier ON Stk04_Supplier.Sup_RecId = RecHeader.DOH_SupRecid"
            strsql &= " LEFT JOIN dbo.stk04_PurDelOrdDQty RecQTYDeat ON RecQTYDeat.DOQ_Drecid = Recdetails.DOD_Recid"
            strsql &= " INNER   JOIN dbo.Stk04_PurDelOrdDQCH RECQCH ON RECQCH.PurQCH_PrDReciD = Recdetails.DOD_Recid"
            strsql &= " AND ISNULL(RECQCH.PurQCH_PrQDRecId,"
            strsql &= " 0) = ISNULL(RecQTYDeat.DOQ_RecID,"
            strsql &= " 0)"
            strsql &= " INNER JOIN Stk04_PurDelOrdDQCd ON Stk04_PurDelOrdDQCD.PurQCD_HRecID = RECQCH.PurQCH_ReciD"
            strsql &= " INNER JOIN dbo.Stk04_ROQCFormH ON Stk04_ROQCFormH.ROQCH_RecID = RECQCH.PurQCH_QCHRecId"
            strsql &= " INNER JOIN dbo.stk04_ROQCFormD ON stk04_ROQCFormD.ROQCD_HRecID = Stk04_ROQCFormH.ROQCH_RecID"
            strsql &= " AND stk04_ROQCFormD.ROQCD_RecID = Stk04_PurDelOrdDQCD.PurQCD_QCDRecID"
            strsql &= " LEFT JOIN dbo.stk04_ROQCFormDSub ON stk04_ROQCFormDSub.ROQCS_DRecid = stk04_ROQCFormD.ROQCD_RecID"
            strsql &= " WHERE     DOH_Recid in " & HeadReci & ""
            strsql &= " ) t"
            strsql &= " WHERE   RankChnge % 6 = 5"
            strsql &= " ORDER BY DOD_Recid ,"
            strsql &= " DOQ_RecID ,"
            strsql &= " ROQCD_DispSeq"


            pdvParam.Value = strsql
            pvCollection.Add(pdvParam)
            myReport.Subreports("Sub5").DataDefinition.ParameterFields("@strsqlSub5").ApplyCurrentValues(pvCollection)
            pvCollection.Clear()
            myReport.SetParameterValue("@strsqlSub5", strsql, "Sub5")
            ' ---------------------------------------------------------------------------------------------------------------
            ' SubReport 6 ---------------------------------------------------------------------------------------------
            strsql = " SELECT  DENSE_RANK() OVER ( ORDER BY RankChnge ) [SubCounter] ,"
            strsql &= " RankChnge ,"
            strsql &= " ROQCH_RecID ,"
            strsql &= " ROQCH_RqDecision ,"
            strsql &= " ROQCH_FCode ,"
            strsql &= " ROQCH_Desc ,"
            strsql &= " ROQCH_Version ,"
            strsql &= " ROQCD_Type ,"
            strsql &= " ROQCD_DispSeq ,"
            strsql &= " ROQCD_Mandatory ,"
            strsql &= " ROQCD_NotifyVal ,"
            strsql &= " ROQCD_MinVal ,"
            strsql &= " ROQCD_maxVal ,"
            '  strsql &= " ROQCD_DispEng [ROQCD_DispEng] ,"
            If rbPrintLatin = True Then
                strsql &= " ROQCD_DispEng [ROQCD_DispEng] ,"
            Else
                strsql &= " isnull(ROQCD_DispArb,ROQCD_DispEng) [ROQCD_DispEng] ,"
            End If
            strsql &= " ROQCD_Remark ,"
            ' strsql &= " ROQCS_DescEng [ROQCS_DescEng] ,"
            If rbPrintLatin = True Then
                strsql &= " ROQCS_DescEng [ROQCS_DescEng] "
            Else
                strsql &= " isnull(ROQCS_DescArb,ROQCS_DescEng) [ROQCS_DescEng] "
            End If

            strsql &= "  , STK_RECID ,"
            strsql &= " STK_REF ,"
            strsql &= " STK_DESC ,"
            strsql &= " Sup_Name ,"
            strsql &= " PurQCD_AnsYN ,"
            strsql &= " PurQCD_AnsDate ,"
            strsql &= " PurQCD_Val ,"
            strsql &= " PurQCD_Remrk ,"
            strsql &= " ROQCS_Choice ,"
            strsql &= " CASE WHEN CHARINDEX('|' + CONVERT(VARCHAR, ROQCS_Choice) + '|',"
            strsql &= " '|||' + CONVERT(VARCHAR, PurQCD_MultCh) + '|', 0) <> 0"
            strsql &= " THEN 'Y'"
            strsql &= " ELSE 'N'"
            strsql &= " END [answerMult] ,"
            strsql &= " PurQCD_MultCh ,"
            strsql &= " CHARINDEX('|' + CONVERT(VARCHAR, ROQCS_Choice) + '|',"
            strsql &= " '|||' + CONVERT(VARCHAR, PurQCD_MultCh) + '|', 0) ,"
            strsql &= " PurQCH_AccQty ,"
            strsql &= " PurQCH_RjQty ,"
            strsql &= " PurQCH_Reson , DOQ_ExpiryDate, DOQ_BatchInfo, DOH_Date"
            strsql &= "   , CASE WHEN ISNULL(PurQCH_AccQty,0) + ISNULL(PurQCH_RjQty,0)"
            strsql &= "	=0 THEN ISNULL(DOQ_Recqty,DOD_Recqty) ELSE ISNULL(PurQCH_AccQty,0) + ISNULL(PurQCH_RjQty,0) END [OrderdQty]"

            strsql &= " FROM    ( SELECT    DENSE_RANK() OVER (PARTITION BY ROQCH_RecID ORDER BY ROQCH_RecID, DOD_Recid, DOQ_RecID ) RankChnge ,"
            strsql &= " *"
            strsql &= " FROM      dbo.stk04_PurDelOrdH RecHeader"
            strsql &= " INNER JOIN dbo.stk04_PurDelOrdD Recdetails ON Recdetails.DOD_Hrecid = RecHeader.DOH_Recid"
            strsql &= " INNER JOIN dbo.STK04_STKFIL ON STK04_STKFIL.STK_RECID = Recdetails.DOD_StkRecid"
            strsql &= " INNER JOIN dbo.Stk04_Supplier ON Stk04_Supplier.Sup_RecId = RecHeader.DOH_SupRecid"
            strsql &= " LEFT JOIN dbo.stk04_PurDelOrdDQty RecQTYDeat ON RecQTYDeat.DOQ_Drecid = Recdetails.DOD_Recid"
            strsql &= " INNER   JOIN dbo.Stk04_PurDelOrdDQCH RECQCH ON RECQCH.PurQCH_PrDReciD = Recdetails.DOD_Recid"
            strsql &= " AND ISNULL(RECQCH.PurQCH_PrQDRecId,"
            strsql &= " 0) = ISNULL(RecQTYDeat.DOQ_RecID,"
            strsql &= " 0)"
            strsql &= " INNER JOIN Stk04_PurDelOrdDQCd ON Stk04_PurDelOrdDQCD.PurQCD_HRecID = RECQCH.PurQCH_ReciD"
            strsql &= " INNER JOIN dbo.Stk04_ROQCFormH ON Stk04_ROQCFormH.ROQCH_RecID = RECQCH.PurQCH_QCHRecId"
            strsql &= " INNER JOIN dbo.stk04_ROQCFormD ON stk04_ROQCFormD.ROQCD_HRecID = Stk04_ROQCFormH.ROQCH_RecID"
            strsql &= " AND stk04_ROQCFormD.ROQCD_RecID = Stk04_PurDelOrdDQCD.PurQCD_QCDRecID"
            strsql &= " LEFT JOIN dbo.stk04_ROQCFormDSub ON stk04_ROQCFormDSub.ROQCS_DRecid = stk04_ROQCFormD.ROQCD_RecID"
            strsql &= " WHERE     DOH_Recid in " & HeadReci & ""
            strsql &= " ) t"
            strsql &= " WHERE   RankChnge % 6 = 0"
            strsql &= " ORDER BY DOD_Recid ,"
            strsql &= " DOQ_RecID ,"
            strsql &= " ROQCD_DispSeq"
            pdvParam.Value = strsql
            pvCollection.Add(pdvParam)
            myReport.Subreports("Sub6").DataDefinition.ParameterFields("@strsqlSub6").ApplyCurrentValues(pvCollection)
            pvCollection.Clear()
            myReport.SetParameterValue("@strsqlSub6", strsql, "Sub6")
            ' ---------------------------------------------------------------------------------------------------------------
            ' the below Sub Report Is Used to Show the Header Row Number is Used to set the number of the Item in the report 
            ' Dense_Rank() is used to know how many time do i need to show the header
            ' sub8 -----------------------------------------------------------------------------------------------------------
            strsql = " SELECT ROW_NUMBER() OVER (PARTITION BY [SubCounter], ROQCH_RecID ORDER BY  RankChnge) [ItemNumber] ,* FROM ("
            strsql &= " SELECT DENSE_RANK() OVER ( ORDER BY RankChnge ) [SubCounter] ,"
            strsql &= " RankChnge ,"
            strsql &= " ROQCH_RecID ,"
            strsql &= " ROQCH_RqDecision ,"
            strsql &= " ROQCH_FCode ,"
            strsql &= " ROQCH_Desc ,"
            strsql &= " ROQCH_Version ,"
            strsql &= " STK_RECID ,"
            strsql &= " STK_REF ,"
            strsql &= " STK_DESC ,DOQ_ExpiryDate, DOQ_BatchInfo,"
            strsql &= " Sup_Name ,"
            strsql &= " PurQCH_AccQty ,"
            strsql &= " PurQCH_RjQty ,"
            strsql &= " PurQCH_Reson"
            strsql &= " FROM   ( SELECT    DENSE_RANK() OVER ( PARTITION BY ROQCH_RecID ORDER BY ROQCH_RecID, DOD_Recid, DOQ_RecID ) RankChnge ,"
            strsql &= " *"
            strsql &= " FROM      dbo.stk04_PurDelOrdH RecHeader"
            strsql &= " INNER JOIN dbo.stk04_PurDelOrdD Recdetails ON Recdetails.DOD_Hrecid = RecHeader.DOH_Recid"
            strsql &= " INNER JOIN dbo.STK04_STKFIL ON STK04_STKFIL.STK_RECID = Recdetails.DOD_StkRecid"
            strsql &= " INNER JOIN dbo.Stk04_Supplier ON Stk04_Supplier.Sup_RecId = RecHeader.DOH_SupRecid"
            strsql &= " LEFT JOIN dbo.stk04_PurDelOrdDQty RecQTYDeat ON RecQTYDeat.DOQ_Drecid = Recdetails.DOD_Recid"
            strsql &= " INNER   JOIN dbo.Stk04_PurDelOrdDQCH RECQCH ON RECQCH.PurQCH_PrDReciD = Recdetails.DOD_Recid"
            strsql &= " AND ISNULL(RECQCH.PurQCH_PrQDRecId,"
            strsql &= " 0) = ISNULL(RecQTYDeat.DOQ_RecID,"
            strsql &= " 0)"
            strsql &= " "
            strsql &= " INNER JOIN dbo.Stk04_ROQCFormH ON Stk04_ROQCFormH.ROQCH_RecID = RECQCH.PurQCH_QCHRecId"
            strsql &= " "
            strsql &= " WHERE     DOH_Recid in " & HeadReci & ""
            strsql &= " ) t"
            strsql &= " WHERE  RankChnge % 6 = 1"
            strsql &= " UNION ALL"
            strsql &= " SELECT DENSE_RANK() OVER ( ORDER BY RankChnge ) [SubCounter] ,"
            strsql &= " RankChnge ,"
            strsql &= " ROQCH_RecID ,"
            strsql &= " ROQCH_RqDecision ,"
            strsql &= " ROQCH_FCode ,"
            strsql &= " ROQCH_Desc ,"
            strsql &= " ROQCH_Version ,"
            strsql &= " STK_RECID ,"
            strsql &= " STK_REF ,"
            strsql &= " STK_DESC ,DOQ_ExpiryDate, DOQ_BatchInfo,"
            strsql &= " Sup_Name ,"
            strsql &= " PurQCH_AccQty ,"
            strsql &= " PurQCH_RjQty ,"
            strsql &= " PurQCH_Reson"
            strsql &= " FROM   ( SELECT    DENSE_RANK() OVER ( PARTITION BY ROQCH_RecID ORDER BY ROQCH_RecID, DOD_Recid, DOQ_RecID ) RankChnge ,"
            strsql &= " *"
            strsql &= " FROM      dbo.stk04_PurDelOrdH RecHeader"
            strsql &= " INNER JOIN dbo.stk04_PurDelOrdD Recdetails ON Recdetails.DOD_Hrecid = RecHeader.DOH_Recid"
            strsql &= " INNER JOIN dbo.STK04_STKFIL ON STK04_STKFIL.STK_RECID = Recdetails.DOD_StkRecid"
            strsql &= " INNER JOIN dbo.Stk04_Supplier ON Stk04_Supplier.Sup_RecId = RecHeader.DOH_SupRecid"
            strsql &= " LEFT JOIN dbo.stk04_PurDelOrdDQty RecQTYDeat ON RecQTYDeat.DOQ_Drecid = Recdetails.DOD_Recid"
            strsql &= " INNER   JOIN dbo.Stk04_PurDelOrdDQCH RECQCH ON RECQCH.PurQCH_PrDReciD = Recdetails.DOD_Recid"
            strsql &= " AND ISNULL(RECQCH.PurQCH_PrQDRecId,"
            strsql &= " 0) = ISNULL(RecQTYDeat.DOQ_RecID,"
            strsql &= " 0)"
            strsql &= " "
            strsql &= " INNER JOIN dbo.Stk04_ROQCFormH ON Stk04_ROQCFormH.ROQCH_RecID = RECQCH.PurQCH_QCHRecId"
            strsql &= " "
            strsql &= " WHERE     DOH_Recid in " & HeadReci & ""
            strsql &= " ) t"
            strsql &= " WHERE  RankChnge % 6 = 2"
            strsql &= " UNION ALL"
            strsql &= " SELECT DENSE_RANK() OVER ( ORDER BY RankChnge ) [SubCounter] ,"
            strsql &= " RankChnge ,"
            strsql &= " ROQCH_RecID ,"
            strsql &= " ROQCH_RqDecision ,"
            strsql &= " ROQCH_FCode ,"
            strsql &= " ROQCH_Desc ,"
            strsql &= " ROQCH_Version ,"
            strsql &= " STK_RECID ,"
            strsql &= " STK_REF ,"
            strsql &= " STK_DESC ,DOQ_ExpiryDate, DOQ_BatchInfo,"
            strsql &= " Sup_Name ,"
            strsql &= " PurQCH_AccQty ,"
            strsql &= " PurQCH_RjQty ,"
            strsql &= " PurQCH_Reson"
            strsql &= " FROM   ( SELECT    DENSE_RANK() OVER ( PARTITION BY ROQCH_RecID ORDER BY ROQCH_RecID, DOD_Recid, DOQ_RecID ) RankChnge ,"
            strsql &= " *"
            strsql &= " FROM      dbo.stk04_PurDelOrdH RecHeader"
            strsql &= " INNER JOIN dbo.stk04_PurDelOrdD Recdetails ON Recdetails.DOD_Hrecid = RecHeader.DOH_Recid"
            strsql &= " INNER JOIN dbo.STK04_STKFIL ON STK04_STKFIL.STK_RECID = Recdetails.DOD_StkRecid"
            strsql &= " INNER JOIN dbo.Stk04_Supplier ON Stk04_Supplier.Sup_RecId = RecHeader.DOH_SupRecid"
            strsql &= " LEFT JOIN dbo.stk04_PurDelOrdDQty RecQTYDeat ON RecQTYDeat.DOQ_Drecid = Recdetails.DOD_Recid"
            strsql &= " INNER   JOIN dbo.Stk04_PurDelOrdDQCH RECQCH ON RECQCH.PurQCH_PrDReciD = Recdetails.DOD_Recid"
            strsql &= " AND ISNULL(RECQCH.PurQCH_PrQDRecId,"
            strsql &= " 0) = ISNULL(RecQTYDeat.DOQ_RecID,"
            strsql &= " 0)"
            strsql &= " "
            strsql &= " INNER JOIN dbo.Stk04_ROQCFormH ON Stk04_ROQCFormH.ROQCH_RecID = RECQCH.PurQCH_QCHRecId"
            strsql &= " "
            strsql &= " WHERE     DOH_Recid in " & HeadReci & ""
            strsql &= " ) t"
            strsql &= " WHERE  RankChnge % 6 = 3"
            strsql &= " UNION"
            strsql &= " SELECT DENSE_RANK() OVER ( ORDER BY RankChnge ) [SubCounter] ,"
            strsql &= " RankChnge ,"
            strsql &= " ROQCH_RecID ,"
            strsql &= " ROQCH_RqDecision ,"
            strsql &= " ROQCH_FCode ,"
            strsql &= " ROQCH_Desc ,"
            strsql &= " ROQCH_Version ,"
            strsql &= " STK_RECID ,"
            strsql &= " STK_REF ,"
            strsql &= " STK_DESC ,DOQ_ExpiryDate, DOQ_BatchInfo,"
            strsql &= " Sup_Name ,"
            strsql &= " PurQCH_AccQty ,"
            strsql &= " PurQCH_RjQty ,"
            strsql &= " PurQCH_Reson"
            strsql &= " FROM   ( SELECT    DENSE_RANK() OVER ( PARTITION BY ROQCH_RecID ORDER BY ROQCH_RecID, DOD_Recid, DOQ_RecID ) RankChnge ,"
            strsql &= " *"
            strsql &= " FROM      dbo.stk04_PurDelOrdH RecHeader"
            strsql &= " INNER JOIN dbo.stk04_PurDelOrdD Recdetails ON Recdetails.DOD_Hrecid = RecHeader.DOH_Recid"
            strsql &= " INNER JOIN dbo.STK04_STKFIL ON STK04_STKFIL.STK_RECID = Recdetails.DOD_StkRecid"
            strsql &= " INNER JOIN dbo.Stk04_Supplier ON Stk04_Supplier.Sup_RecId = RecHeader.DOH_SupRecid"
            strsql &= " LEFT JOIN dbo.stk04_PurDelOrdDQty RecQTYDeat ON RecQTYDeat.DOQ_Drecid = Recdetails.DOD_Recid"
            strsql &= " INNER   JOIN dbo.Stk04_PurDelOrdDQCH RECQCH ON RECQCH.PurQCH_PrDReciD = Recdetails.DOD_Recid"
            strsql &= " AND ISNULL(RECQCH.PurQCH_PrQDRecId,"
            strsql &= " 0) = ISNULL(RecQTYDeat.DOQ_RecID,"
            strsql &= " 0)"
            strsql &= " "
            strsql &= " INNER JOIN dbo.Stk04_ROQCFormH ON Stk04_ROQCFormH.ROQCH_RecID = RECQCH.PurQCH_QCHRecId"
            strsql &= " "
            strsql &= " WHERE     DOH_Recid in " & HeadReci & ""
            strsql &= " ) t"
            strsql &= " WHERE  RankChnge % 6 = 4"
            strsql &= " UNION ALL"
            strsql &= " SELECT DENSE_RANK() OVER ( ORDER BY RankChnge ) [SubCounter] ,"
            strsql &= " RankChnge ,"
            strsql &= " ROQCH_RecID ,"
            strsql &= " ROQCH_RqDecision ,"
            strsql &= " ROQCH_FCode ,"
            strsql &= " ROQCH_Desc ,"
            strsql &= " ROQCH_Version ,"
            strsql &= " STK_RECID ,"
            strsql &= " STK_REF ,"
            strsql &= " STK_DESC ,DOQ_ExpiryDate, DOQ_BatchInfo,"
            strsql &= " Sup_Name ,"
            strsql &= " PurQCH_AccQty ,"
            strsql &= " PurQCH_RjQty ,"
            strsql &= " PurQCH_Reson"
            strsql &= " FROM   ( SELECT    DENSE_RANK() OVER ( PARTITION BY ROQCH_RecID ORDER BY ROQCH_RecID, DOD_Recid, DOQ_RecID ) RankChnge ,"
            strsql &= " *"
            strsql &= " FROM      dbo.stk04_PurDelOrdH RecHeader"
            strsql &= " INNER JOIN dbo.stk04_PurDelOrdD Recdetails ON Recdetails.DOD_Hrecid = RecHeader.DOH_Recid"
            strsql &= " INNER JOIN dbo.STK04_STKFIL ON STK04_STKFIL.STK_RECID = Recdetails.DOD_StkRecid"
            strsql &= " INNER JOIN dbo.Stk04_Supplier ON Stk04_Supplier.Sup_RecId = RecHeader.DOH_SupRecid"
            strsql &= " LEFT JOIN dbo.stk04_PurDelOrdDQty RecQTYDeat ON RecQTYDeat.DOQ_Drecid = Recdetails.DOD_Recid"
            strsql &= " INNER   JOIN dbo.Stk04_PurDelOrdDQCH RECQCH ON RECQCH.PurQCH_PrDReciD = Recdetails.DOD_Recid"
            strsql &= " AND ISNULL(RECQCH.PurQCH_PrQDRecId,"
            strsql &= " 0) = ISNULL(RecQTYDeat.DOQ_RecID,"
            strsql &= " 0)"
            strsql &= " "
            strsql &= " INNER JOIN dbo.Stk04_ROQCFormH ON Stk04_ROQCFormH.ROQCH_RecID = RECQCH.PurQCH_QCHRecId"
            strsql &= " "
            strsql &= " WHERE     DOH_Recid in " & HeadReci & ""
            strsql &= " ) t"
            strsql &= " WHERE  RankChnge % 6 = 5"
            strsql &= " UNION"
            strsql &= " SELECT DENSE_RANK() OVER ( ORDER BY RankChnge ) [SubCounter] ,"
            strsql &= " RankChnge ,"
            strsql &= " ROQCH_RecID ,"
            strsql &= " ROQCH_RqDecision ,"
            strsql &= " ROQCH_FCode ,"
            strsql &= " ROQCH_Desc ,"
            strsql &= " ROQCH_Version ,"
            strsql &= " STK_RECID ,"
            strsql &= " STK_REF ,"
            strsql &= " STK_DESC ,DOQ_ExpiryDate, DOQ_BatchInfo,"
            strsql &= " Sup_Name ,"
            strsql &= " PurQCH_AccQty ,"
            strsql &= " PurQCH_RjQty ,"
            strsql &= " PurQCH_Reson"
            strsql &= " FROM   ( SELECT    DENSE_RANK() OVER ( PARTITION BY ROQCH_RecID ORDER BY ROQCH_RecID, DOD_Recid, DOQ_RecID ) RankChnge ,"
            strsql &= " *"
            strsql &= " FROM      dbo.stk04_PurDelOrdH RecHeader"
            strsql &= " INNER JOIN dbo.stk04_PurDelOrdD Recdetails ON Recdetails.DOD_Hrecid = RecHeader.DOH_Recid"
            strsql &= " INNER JOIN dbo.STK04_STKFIL ON STK04_STKFIL.STK_RECID = Recdetails.DOD_StkRecid"
            strsql &= " INNER JOIN dbo.Stk04_Supplier ON Stk04_Supplier.Sup_RecId = RecHeader.DOH_SupRecid"
            strsql &= " LEFT JOIN dbo.stk04_PurDelOrdDQty RecQTYDeat ON RecQTYDeat.DOQ_Drecid = Recdetails.DOD_Recid"
            strsql &= " INNER   JOIN dbo.Stk04_PurDelOrdDQCH RECQCH ON RECQCH.PurQCH_PrDReciD = Recdetails.DOD_Recid"
            strsql &= " AND ISNULL(RECQCH.PurQCH_PrQDRecId,"
            strsql &= " 0) = ISNULL(RecQTYDeat.DOQ_RecID,"
            strsql &= " 0)"
            strsql &= " "
            strsql &= " INNER JOIN dbo.Stk04_ROQCFormH ON Stk04_ROQCFormH.ROQCH_RecID = RECQCH.PurQCH_QCHRecId"
            strsql &= " "
            strsql &= " WHERE     DOH_Recid in " & HeadReci & ""
            strsql &= " ) t"
            strsql &= " WHERE  RankChnge % 6 = 0"
            strsql &= " ) T1"
            myReport.Subreports("Sub7").DataDefinition.ParameterFields("@strsqlSub7").ApplyCurrentValues(pvCollection)
            pvCollection.Clear()
            myReport.SetParameterValue("@strsqlSub7", strsql, "Sub7")
            '----------------------------------------------------------------------------------------------------------------------------------------

            With myReport.DataDefinition
                '   .FormulaFields("VATReg").Text = "'V.A.T reg: " & sVATreg.Replace("'", "''").Trim & "'"
                .FormulaFields("fomuser").Text = "'" & gbUser.Replace("'", "''").Trim & "'"
                .FormulaFields("fomCompName").Text = "'" & gbCompName.Replace("'", "''").Trim & "'"

                .FormulaFields("gbDecNo").Text = "" & gbDecNo & ""
            End With

            frm = New frmPreviewPrint


            With frm
                .Owner = OwnerFrm
                .SendToBack()
                .Show(myReport, cntTitle, 1, , True, , , , , , , , , , False)

            End With

        Catch exload As LoadSaveReportException

            Throw New LoadSaveReportException("Incorrect path for loading report.", _
                      EngineExceptionErrorID.LoadingReportFailed)

        Catch ex As Exception
            MessageBox.Show("Error: " & ex.Message, cntTitle, MessageBoxButtons.OK, MessageBoxIcon.Error)

        Finally
            tbCurrent = Nothing
            tliCurrent = Nothing
            frm = Nothing
            Cursor.Current = Cursors.Default
        End Try
    End Sub

    Public Sub CreatePoQCtemps(ByVal TempQcHeader As String, ByVal TempQcDetails As String)
        Dim strsql As String
        Try
            DropSqlObject(TempQcHeader, "TABLE")
            DropSqlObject(TempQcDetails, "TABLE")
            strsql = " CREATE TABLE " & TempQcHeader & ""
            strsql &= " ("
            strsql &= " [ReciD] [numeric] (18, 0) NOT NULL,"
            strsql &= " [MyNewReciD] [numeric] (18, 0)  NULL,"
            strsql &= " [PrDReciD] [numeric] (18, 0) NULL,"
            strsql &= " [PrQDRecId] [numeric] (18, 0) NULL,"
            strsql &= " [QCHRecId] [numeric] (18, 0) NULL,"
            strsql &= " [Reson] [nvarchar] (100)  NULL,"
            strsql &= " [RjQty] [decimal] (18, 3) NULL , "
            strsql &= " [AccQty] [decimal] (18, 3) NULL , "
            strsql &= " [OldNew] [varchar] (1) NULL "
            strsql &= " )"
            strsql &= " ; CREATE TABLE " & TempQcDetails & ""
            strsql &= " ("
            strsql &= " [ReciD] [numeric] (18, 0) NOT NULL,"
            strsql &= " [HRecID] [numeric] (18, 0) NULL,"
            strsql &= " [QCDRecID] [numeric] (18, 0) NULL,"
            strsql &= " [AnsYN] [varchar] (1) NULL,"
            strsql &= " [AnsDate] [datetime] NULL,"
            strsql &= " [Val] [numeric] (11, 2) NULL,"
            strsql &= " [Remrk] [nvarchar] (100) COLLATE SQL_Latin1_General_CP1256_CI_AS NULL ,"
            strsql &= " [MultCh] [nvarchar] (500) NULL"
            strsql &= " )"
            Execute(strsql)

        Catch ex As Exception
            MessageBox.Show("Error : " & ex.Message, "", MessageBoxButtons.OK, MessageBoxIcon.Error)
        End Try
    End Sub

    Public Sub fillPoQualityControlTables(ByVal RecIh As String, ByVal TempQcHeader As String, ByVal TempQcDetails As String)
        Dim strsql As String
        Try
            CreatePoQCtemps(TempQcHeader, TempQcDetails)
            strsql = " INSERT  INTO dbo." & TempQcHeader & ""
            strsql &= " SELECT  PurQCH_ReciD ,"
            strsql &= " PurQCH_ReciD ,"
            strsql &= " PurQCH_PrDReciD ,"
            strsql &= " PurQCH_PrQDRecId ,"
            strsql &= " PurQCH_QCHRecId ,"
            strsql &= " PurQCH_Reson ,"
            strsql &= " PurQCH_RjQty ,"
            strsql &= " PurQCH_AccQty ,"
            strsql &= " 'O'"
            strsql &= " FROM    dbo.stk04_PurDelOrdH"
            strsql &= " INNER JOIN dbo.stk04_PurDelOrdD ON stk04_PurDelOrdD.DOD_Hrecid = stk04_PurDelOrdH.DOH_Recid"
            strsql &= " INNER JOIN dbo.Stk04_PurDelOrdDQCH ON Stk04_PurDelOrdDQCH.PurQCH_PrDReciD = stk04_PurDelOrdD.DOD_Recid"
            strsql &= " WHERE   DOH_Recid in " & RecIh & ""
            strsql &= "  ;  INSERT INTO dbo." & TempQcDetails & ""
            strsql &= " SELECT    PurQCD_ReciD ,"
            strsql &= " PurQCD_HRecID ,"
            strsql &= " PurQCD_QCDRecID ,"
            strsql &= " PurQCD_AnsYN ,"
            strsql &= " PurQCD_AnsDate ,"
            strsql &= " PurQCD_Val ,"
            strsql &= " PurQCD_Remrk ,"
            strsql &= " PurQCD_MultCh"
            strsql &= " FROM    dbo.stk04_PurDelOrdH"
            strsql &= " INNER JOIN dbo.stk04_PurDelOrdD ON stk04_PurDelOrdD.DOD_Hrecid = stk04_PurDelOrdH.DOH_Recid"
            strsql &= " INNER JOIN dbo.Stk04_PurDelOrdDQCH ON Stk04_PurDelOrdDQCH.PurQCH_PrDReciD = stk04_PurDelOrdD.DOD_Recid"
            strsql &= " INNER JOIN  dbo.Stk04_PurDelOrdDQCD ON Stk04_PurDelOrdDQCD.PurQCD_HRecID = Stk04_PurDelOrdDQCH.PurQCH_ReciD"
            strsql &= " WHERE   DOH_Recid in " & RecIh & ""
            Execute(strsql)

        Catch ex As Exception

        End Try
    End Sub
#End Region

    Private Function FundsQuery(_ReportTempTable As String, _WithData As Boolean, _frmFrom As String, Optional _Cond As String = "") As String
        Dim strsql As String
        Try
            strsql = $"  SELECT FTR_RecID ,
                                FTR_TrnRecID ,"

            If _frmFrom = "PO" Then
                strsql &= " FTR_MODSupItmRef, 
                   FTR_MODStat,
                   FTR_MODCount, 
                            ( FTR_AmountToDisburse / ReportTempTable.Total_Amount ) * 100 AS [FTR_Percentage] ,"
            Else
                strsql &= " ( FTR_AmountToDisburse / ReportTempTable.[TN Cost] ) * 100 AS [FTR_Percentage], "
            End If

            strsql &= $"        FDT_Code ,
                                FDT_Description ,
                                FTR_AmountToDisburse ,
                                FTR_Currency ,
                                FTR_OperLC ,
                                FTR_RateToLC ,
                                FTR_CounterValueLC ,
                                FTR_OperFreshDollar ,
                                FTR_RateFreshDollar ,
                                FTR_CounterValueFreshDollar ,
                                FTR_Remarks
                         FROM   {IIf(_frmFrom = "PO", "STK04_FundTransactionPODet", "STK04_FundTransactionPIDet")} fndTrn "


            If _frmFrom = "PO" Then
                strsql &= $" INNER JOIN( SELECT MOD_Hrecid ,
                                                MOD_stat ,
                                                MOD_supitmref ,
                                                MOD_Count,
                                                Total_Amount
                                         FROM   V_PORD2_MODETGRID"
                If _ReportTempTable = "" Then
                    If _Cond <> "" Then strsql &= " INNER JOIN ( SELECT RMO_recid
                                                                 FROM   PORD2_PORep
                                                               ) PORD2_PORep ON MOD_Hrecid = RMO_recid
                                                    WHERE " & _Cond
                Else
                    strsql &= " INNER JOIN " & _ReportTempTable & " TempPOSel ON MOD_Hrecid = RMO_Recid  AND Chk = 'True' "
                End If

                strsql &= " ) ReportTempTable ON FTR_TrnRecID = MOD_Hrecid
                                                AND FTR_MODSupItmRef = MOD_supitmref
                                                AND FTR_MODStat = MOD_stat
                                                AND FTR_MODCount = MOD_Count "


            Else
                'strsql &= $" INNER JOIN {_ReportTempTable} ReportTempTable ON {IIf(_frmFrom = "PO", "fndTrn.FTR_TrnRecID = ReportTempTable.RMO_Recid and Chk = 'True'", "fndTrn.FTR_TrnRecID = ReportTempTable.INVD_RECID")}"
                strsql &= $" INNER JOIN {_ReportTempTable} ReportTempTable ON fndTrn.FTR_TrnRecID = ReportTempTable.INVD_RECID "

            End If

            strsql &= " INNER JOIN " & gbAccountingLinkedServerForFunds & "ACC01_FundTypes ON FTR_FDTRecID = FDT_RecID
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

    Public Sub GetBestDeal(_Owner As Form, _ItemRecID As String, _Currency As String)
        Dim msgToShow As String
        Try

            If _ItemRecID = "" Then Exit Try
            If Not (_Currency.Trim = gbCurr1 OrElse _Currency.Trim = gbCurr2) Then Exit Try

            Using dtBestDeal As New DataTable

                Using daBestDeal As New SqlDataAdapter($"EXEC dbo.SP_PORD2_GetBestSupplierPrice {_ItemRecID}, '{gbComp}' , '{_Currency.Trim}', '{gbCurr1}', '{gbCurr2}'",
                                                       ConnectionString)
                    daBestDeal.Fill(dtBestDeal)
                End Using

                If dtBestDeal.Rows.Count = 1 Then

                    msgToShow = $"<u><b>Best Deal (Ctrl + h)</b></u> <br/><br/>"
                    msgToShow &= $"<b>Invoice :</b> {dtBestDeal(0)("InvoicePrefix").ToString} - {dtBestDeal(0)("InvoiceCounter").ToString} <br/>"
                    msgToShow &= $"<b>Supplier :</b> {dtBestDeal(0)("InvoiceSupplier").ToString} <br/>"
                    msgToShow &= $"<b>Supplier Invoice :</b> {dtBestDeal(0)("InvoiceReference").ToString} <br/>"
                    msgToShow &= $"<b>Date :</b> {CDate(dtBestDeal(0)("InvoiceDate").ToString).ToString("dd/MM/yyyy")} <br/><br/>"
                    msgToShow &= $"<b>Best Supplier Price :</b> {CDbl(dtBestDeal(0)("BestPrice").ToString).ToString($"N{IIf(_Currency.Trim = gbCurr1, "0", "3").ToString}")} {_Currency.Trim}"

                    ToastNotification.DefaultToastGlowColor = eToastGlowColor.Blue
                    ToastNotification.ToastBackColor = Color.White
                    ToastNotification.ToastForeColor = Color.Navy
                    ToastNotification.DefaultTimeoutInterval = 10000
                    ToastNotification.ToastFont = New Font("Arial", 12)
                    ToastNotification.Show(_Owner, msgToShow, eToastPosition.MiddleRight)

                End If

            End Using

        Catch ex As Exception
            'ShowNotification(_Owner, "Cannot Get The Best Deal")
        End Try
    End Sub


#Region "Fashion Report"

    Public Sub FashionReport(_InvoiceRecID As String, _tempTable As String, _TransactionType As String, lstMatrix As C1TrueDBGrid)

        Dim colCount As Integer = 0
        Dim proratedCoef As Double = 0
        Dim itemCount As Integer = 0
        Dim proratedCoefValue As Double = 0
        Try

            Using dtInvoiceInfo, dtMatrixReport As New DataTable

                CreateAndDesignInvoiceInfoDT(_InvoiceRecID, dtInvoiceInfo, _TransactionType)

                If dtInvoiceInfo.Rows.Count > 0 Then

                    CreateAndFillTempTable(_InvoiceRecID, _tempTable, _TransactionType)

                    If _TransactionType <> "PI" Then
                        CreateAndDesignMatrixReportDT(_tempTable, dtMatrixReport, dtInvoiceInfo(0)("invCurrency").ToString, _TransactionType, lstMatrix)
                    Else
                        CreateAndDesignMatrixReportDTPI(_tempTable, dtMatrixReport, dtInvoiceInfo(0)("invCurrency").ToString, lstMatrix)
                    End If

                    colCount = dtMatrixReport.Columns.Count - 2

                    If _TransactionType = "PO" Then
                        CreateExcelReportPO("exlFashionPO.xlsx", "Order#", 13, dtMatrixReport, colCount, dtInvoiceInfo(0)("invTitle").ToString, dtInvoiceInfo(0)("invCurrency").ToString,
                                            dtInvoiceInfo, dtInvoiceInfo(0)("RMO_VATLC").ToString, dtInvoiceInfo(0)("RMO_VATFC").ToString)
                    ElseIf _TransactionType = "PI" Then

                        For Each row In dtMatrixReport.Select(" columnType = 'H' ")
                            proratedCoef += CDbl(dtMatrixReport(0)("RetailPriceCC").ToString) / CDbl(dtMatrixReport(0)("UnitCostCC").ToString)
                            itemCount += 1
                        Next

                        proratedCoefValue = proratedCoef / CDbl(itemCount)

                        CreateExcelReportPI("exlFashionPI.xlsx", "Inv#", 14, dtMatrixReport, colCount, dtInvoiceInfo(0)("invTitle").ToString, dtInvoiceInfo(0)("invCurrency").ToString, dtInvoiceInfo,
                                            proratedCoefValue, dtInvoiceInfo(0)("INV_VATLC").ToString, dtInvoiceInfo(0)("INV_VATCC").ToString)
                    Else
                        CreateExcelReportRO("exlFashionRO.xlsx", "RO#", 12, dtMatrixReport, colCount, dtInvoiceInfo(0)("invTitle").ToString, dtInvoiceInfo(0)("invCurrency").ToString, dtInvoiceInfo)
                    End If

                End If

            End Using

        Catch ex As Exception
            Throw
        Finally
            Try

                If Not String.IsNullOrEmpty(_tempTable) Then
                    DropSqlObject(_tempTable, "TABLE")
                End If

            Catch exF As Exception
                'Nothing
            End Try
        End Try
    End Sub

    Private Sub CreateAndFillTempTable(_InvoiceRecID As String, _TempTable As String, _TransactionType As String)
        Dim strsql As String
        Try
            If _TransactionType = "PI" Then
                strsql = $" SELECT  INVD_RECID ,
                                ISNULL(STK_REF, SFP_SingleRef1) STK_REF ,
                                ISNULL(STK_DESC, SFP_Desc) STK_DESC ,
                                INVD_LOCATION ,
                                INVQ_GridCode ,
                                INVQ_GridSize ,
                                INVQ_Color ,
                                Color_Reference ,
                                INVQ_SuppColor ,
                                INVQ_Recqty INVD_PURQTY ,
                                ISNULL(STK_UNIT, SFP_UNIT) STK_UNIT ,
                                INVD_STATUS,
                                INVD_UCOST ,
                                dbo.convert5FC('{gbApp}', '{gbComp}', '{gbYear}', INVD_UCOST, INV_CURR, '{gbCurr1}',
                                               INV_RateLC, INV_INVDATE) UnitCostLC ,
                                dbo.convert5FC('{gbApp}', '{gbComp}', '{gbYear}', INVD_UCOST, INV_CURR, '{gbCurr2}',
                                               INV_RateCC, INV_INVDATE) UnitCostcC ,
                                INVD_ITMDISC ,
                                INVD_RETLC ,
                                INVD_RETCC ,
                                (INVD_TNCOST / INVD_PURQTY) * INVQ_Recqty INVD_TNCOST,
                                dbo.convert5FC('{gbApp}', '{gbComp}', '{gbYear}',
                                               ( ( INVD_TNCOST / INVD_PURQTY ) * INVQ_Recqty ),
                                               INV_CURR, '{gbCurr1}', INV_RateLC, INV_INVDATE) TotalNetLC ,
                                dbo.convert5FC('{gbApp}', '{gbComp}', '{gbYear}',
                                               ( ( INVD_TNCOST / INVD_PURQTY ) * INVQ_Recqty ),
                                               INV_CURR, '{gbCurr2}', INV_RateCC, INV_INVDATE) TotalNetcC
                        INTO {_TempTable}
                        FROM    dbo.STK04_InvDET
                                INNER JOIN dbo.STK04_InvHead ON STK04_InvHead.INV_RECID = STK04_InvDET.INVD_HRECID
                                LEFT JOIN dbo.STK04_STKFIL ON STK04_STKFIL.STK_RECID = STK04_InvDET.INVD_STKRECID
                                LEFT JOIN dbo.STK04_StkFilPending ON STK04_StkFilPending.SFP_Recid = STK04_InvDET.INVD_PendRecId
                                INNER JOIN ( SELECT INVQ_RecID ,
                                                    INVQ_Drecid ,
                                                    INVQ_GridCode ,
                                                    INVQ_GridSize ,
                                                    INVQ_Recqty ,
                                                    INVQ_Color ,
                                                    Color_Reference ,
                                                    INVQ_SuppColor
                                             FROM   STK04_InvDQty
                                                    INNER JOIN dbo.Stk04_COLOR ON Stk04_COLOR.Color_Code = STK04_InvDQty.INVQ_Color
                                           ) STK04_InvDQty ON STK04_InvDQty.INVQ_Drecid = STK04_InvDET.INVD_RECID 
                        WHERE INVD_HRECID = {_InvoiceRecID} "
            ElseIf _TransactionType = "PO" Then
                strsql = $" SELECT  MOD_RMRecid ,
                                ISNULL(STK_REF, SFP_SingleRef1) STK_REF ,
                                ISNULL(STK_DESC, SFP_Desc) STK_DESC ,
                                MORDQ_GridCode ,
                                MORDQ_GridSize ,
                                MORDQ_Color ,
                                Color_Reference ,
                                MORDQ_SuppColor ,
                                MORDQ_Recqty MOD_PURQTY ,
                                ISNULL(STK_UNIT, SFP_UNIT) STK_UNIT ,
                                MOD_Stat,
                                MOD_UnitCost ,
                                ROUND(dbo.convert5('{gbApp}', '{gbComp}', '{gbYear}', MOD_UnitCost, RMO_cur, '{gbCurr1}',
                                                    RMO_Date), 3) AS [UnitCostLC] ,
                                ROUND(dbo.convert5('{gbApp}', '{gbComp}', '{gbYear}', MOD_UnitCost, RMO_cur, '{gbCurr2}',
                                                    RMO_Date), 3) AS [UnitCostCC] ,
                                MOD_ItmDisc ,
                                ( MOD_NetTot / MOD_OrdQty ) * MORDQ_Recqty MOD_NetTot, 
                                ROUND(dbo.convert5('{gbApp}', '{gbComp}', '{gbYear}',
                                                   ( ( MOD_NetTot / MOD_OrdQty ) * MORDQ_Recqty ),
                                                   RMO_cur, '{gbCurr1}', RMO_Date), 3) AS [TotalNetLC] ,
                                ROUND(dbo.convert5('{gbApp}', '{gbComp}', '{gbYear}',
                                                   ( ( MOD_NetTot / MOD_OrdQty ) * MORDQ_Recqty ),
                                                   RMO_cur, '{gbCurr2}', RMO_Date), 3) AS [TotalNetCC]
                        INTO {_TempTable}
                        FROM    dbo.Stk04_MOrdDet
                                INNER JOIN dbo.Stk04_MOrdH ON Stk04_MOrdH.RMO_Recid = Stk04_MOrdDet.MOD_HRecid
                                LEFT JOIN dbo.STK04_STKFIL ON STK04_STKFIL.STK_RECID = Stk04_MOrdDet.MOD_RMRecid
                                LEFT JOIN dbo.STK04_StkFilPending ON STK04_StkFilPending.SFP_Recid = Stk04_MOrdDet.MOD_PendRecid
                                INNER JOIN ( SELECT MORDQ_RecID ,
                                                    MORDQ_HRecid ,
                                                    MORDQ_GridCode ,
                                                    MORDQ_GridSize ,
                                                    MORDQ_Recqty ,
                                                    MORDQ_Color ,
                                                    Color_Reference ,
                                                    MORDQ_SuppColor ,
                                                    MORDQ_SupItmRef ,
                                                    MORDQ_Stat ,
                                                    MORDQ_Count
                                             FROM   STK04_MordQty
                                                    INNER JOIN dbo.Stk04_COLOR ON Stk04_COLOR.Color_Code = MORDQ_Color
                                           ) STK04_MordQty ON STK04_MordQty.MORDQ_HRecid = Stk04_MOrdDet.MOD_HRecid
                                                              AND STK04_MordQty.MORDQ_SupItmRef = Stk04_MOrdDet.MOD_SupItmRef
                                                              AND STK04_MordQty.MORDQ_Stat = Stk04_MOrdDet.MOD_Stat
                                                              AND STK04_MordQty.MORDQ_Count = Stk04_MOrdDet.MOD_Count
                        WHERE   MOD_HRecid = {_InvoiceRecID} "
            Else

                strsql = $" SELECT DOD_Hrecid ,
                                   ISNULL(STK_REF, STK_SingleRef1) STK_REF ,
                                   STK_DESC STK_DESC ,
                                   stk04_PurDelOrdD.DOD_Stat ,
                                   DOQ_GridCode ,
                                   DOQ_GridSize ,
                                   DOD_ColorCode ,
                                   Color_Reference ,
                                   DOD_Color ,
                                   DOQ_Recqty MOD_PURQTY ,
                                   STK_UNIT ,
                                   DOD_NetCostLC ,
                                   DOD_NetCostCC ,
                                   ISNULL(DOD_Disc,0) DOD_Disc
                            INTO {_TempTable}
                            FROM   dbo.stk04_PurDelOrdD
                                   INNER JOIN dbo.STK04_STKFIL ON STK04_STKFIL.STK_RECID = DOD_StkRecid
                                   INNER JOIN ( SELECT DOQ_RecID ,
                                                       DOQ_Drecid ,
                                                       DOQ_GridCode ,
                                                       DOQ_GridSize ,
                                                       CASE WHEN DOD_Stat = 'R' THEN -1 * DOQ_Recqty
                                                            ELSE DOQ_Recqty
                                                       END DOQ_Recqty ,
                                                       DOD_ColorCode ColorCode ,
                                                       Color_Reference ,
                                                       DOD_Color ColorSupp ,
                                                       DOD_ItmRef ,
                                                       DOD_Stat ,
                                                       DOD_Counter
                                                FROM   stk04_PurDelOrdDQty
                                                       INNER JOIN dbo.stk04_PurDelOrdD ON stk04_PurDelOrdD.DOD_Recid = stk04_PurDelOrdDQty.DOQ_Drecid
                                                       INNER JOIN dbo.Stk04_COLOR ON Stk04_COLOR.Color_Code = DOD_ColorCode
                                              ) stk04_PurDelOrdDQty ON stk04_PurDelOrdDQty.DOQ_Drecid = stk04_PurDelOrdD.DOD_Recid
                            WHERE  DOD_Hrecid = {_InvoiceRecID} "

            End If

            Execute_WOXACT(strsql)

        Catch ex As Exception
            Throw
        End Try
    End Sub

    Private Sub CreateAndDesignInvoiceInfoDT(_InvoiceRecID As String, _dtInvoiceInfo As DataTable, _TransactionType As String)
        Dim strsql As String
        Try
            If _TransactionType = "PI" Then

                strsql = $"  SELECT CONCAT('Inv # ', CASE WHEN INV_Prefix = 'Normal' THEN 'N'
                                          ELSE 'O'
                                     END, ' ', CONVERT(VARCHAR, INV_Counter)) invTitle ,
                                    INV_INVREF ,
                                    Sup_CodeName ,
                                    INV_INVDATE ,
                                    INV_Rks invRemarks ,
                                    INV_CURR invCurrency ,
                                    LocationName ,
                                    INV_DELIVERY ,
                                    INV_UPDATED ,
                                    INV_UPDDATE ,
                                    PriceType ,
                                    Brand ,
                                    Season ,
                                    INV_VATLC ,
                                    INV_VATCC
                             FROM   ( SELECT    INV_RECID ,
                                                INV_Counter,
                                                CASE WHEN INV_Prefix = 'N' THEN 'Normal'
                                                     ELSE 'Other'
                                                END INV_Prefix ,
                                                INV_INVREF ,
                                                CONVERT(VARCHAR, INV_INVDATE, 103) INV_INVDATE ,
                                                CASE INV_STATUS
                                                  WHEN 'P' THEN 'Normal'
                                                  ELSE 'Reverse'
                                                END INV_STATUS ,
                                                INV_Rks ,
                                                INV_CURR ,
                                                Sup_Code + ' - ' + Sup_Name Sup_CodeName ,
                                                INV_LOCATION + ' - ' + Loc_Desc LocationName ,
                                                CONVERT(VARCHAR, INV_DELIVERY, 103) INV_DELIVERY ,
                                                INV_UPDATED ,
                                                 CONVERT(VARCHAR, INV_UPDDATE, 103) INV_UPDDATE ,
                                                INV_PRICETYPE + ' - ' + PRC_DESC PriceType ,
                                                INV_Brand + ' - ' + br_Name Brand ,
                                                INV_Season + ' - ' + Seas_Desc Season,
                                                INV_VATLC ,
                                                INV_VATCC
                                      FROM      STK04_InvHead
                                                INNER JOIN dbo.Stk04_Supplier ON Stk04_Supplier.Sup_RecId = STK04_InvHead.INV_SUPP
                                                INNER JOIN dbo.Stk04_Brand ON Stk04_Brand.br_Code = STK04_InvHead.INV_Brand
                                                INNER JOIN dbo.Stk04_Season ON Stk04_Season.Seas_Code = STK04_InvHead.INV_Season
                                                INNER JOIN dbo.STK04_PriceType ON STK04_PriceType.PRC_CODE = STK04_InvHead.INV_PRICETYPE
                                                INNER JOIN dbo.STK04_Location ON STK04_Location.Loc_Code = STK04_InvHead.INV_LOCATION
                                      WHERE     INV_RECID = {_InvoiceRecID}
                                    ) tblInvoiceDet "

            ElseIf _TransactionType = "PO" Then

                strsql = $"   SELECT CONCAT('Order # ', CONVERT(VARCHAR, RMO_Recid)) invTitle ,
                                     Sup_CodeName ,
                                     RMO_DATE ,
                                     RMO_Remark invRemarks ,
                                     RMO_cur invCurrency ,
                                     Brand ,
                                     Season ,
                                     RMO_DelDate ,
                                     LocationName ,
                                     PriceType, 
		                             RMO_POSTATUS,
                                     RMO_VATFC ,
                                     RMO_VATLC
                              FROM   ( SELECT    RMO_Recid ,
                                                 CONVERT(VARCHAR, RMO_Date, 103) RMO_DATE ,
                                                 CASE RMO_POStatus
                                                   WHEN 'U' THEN 'Under Process'
                                                   WHEN 'C' THEN 'Canceled'
                                                   ELSE 'Finished'
                                                 END RMO_POSTATUS ,
                                                 RMO_Remark ,
                                                 RMO_cur ,
                                                 Sup_Code + ' - ' + Sup_Name Sup_CodeName ,
                                                 RMO_Brand + ' - ' + br_Name Brand ,
                                                 RMO_Season + ' - ' + Seas_Desc Season ,
                                                 RMO_IssLoc + ' - ' + Loc_Desc LocationName ,
                                                 CONVERT(VARCHAR, RMO_DelDate, 103) RMO_DelDate ,
                                                 RMO_Pricetype + ' - ' + PRC_DESC PriceType,
                                                 RMO_VATFC ,
                                                 RMO_VATLC
                                       FROM      dbo.Stk04_MOrdH
                                                 INNER JOIN dbo.Stk04_Supplier ON Stk04_Supplier.Sup_RecId = Stk04_MOrdH.RMO_SupRecid
                                                 INNER JOIN dbo.Stk04_Brand ON Stk04_Brand.br_Code = RMO_Brand
                                                 INNER JOIN dbo.Stk04_Season ON Stk04_Season.Seas_Code = RMO_Season
                                                 INNER JOIN dbo.STK04_Location ON STK04_Location.Loc_Code = RMO_IssLoc
                                                 INNER JOIN dbo.STK04_PriceType ON STK04_PriceType.PRC_CODE = RMO_Pricetype
                                       WHERE     RMO_Recid = {_InvoiceRecID}
                                     ) tblInvoiceDet"

            Else
                strsql = $" SELECT  CONCAT('RO # ', CONVERT(VARCHAR, DOH_Recid)) invTitle ,
                                    Sup_CodeName ,
                                    DOH_Ref ,
                                    DOH_rem invRemarks ,
                                    DOH_Curr invCurrency ,
                                    DOH_PurRecid ,
                                    DOH_ActDelDate ,
                                    LocationName ,
                                    Brand ,
                                    Season
                           FROM     ( SELECT    DOH_Recid ,
                                                CONVERT(VARCHAR, DOH_Date, 103) RMO_DATE ,
                                                CASE DOH_Status
                                                  WHEN 'N' THEN 'Normal'
                                                  WHEN 'C' THEN 'Consignment'
                                                  ELSE 'Finished'
                                                END DOH_Status ,
                                                '' DOH_rem ,
                                                DOH_Curr ,
                                                Sup_Code + ' - ' + Sup_Name Sup_CodeName ,
                                                DOH_Ref ,
                                                DOH_Brand + ' - ' + br_Name Brand ,
                                                DOH_Season + ' - ' + Seas_Desc Season ,
                                                DOH_PurRecid ,
                                                CONVERT(VARCHAR, DOH_ActDelDate, 103) DOH_ActDelDate ,
                                                DOH_loc + ' - ' + Loc_Desc LocationName
                                      FROM      dbo.stk04_PurDelOrdH
                                                INNER JOIN dbo.Stk04_Supplier ON Stk04_Supplier.Sup_RecId = DOH_SupRecid
                                                INNER JOIN dbo.Stk04_Brand ON Stk04_Brand.br_Code = DOH_Brand
                                                INNER JOIN dbo.STK04_Location ON STK04_Location.Loc_Code = DOH_loc
                                                INNER JOIN dbo.Stk04_Season ON Stk04_Season.Seas_Code = DOH_Season
                                      WHERE     DOH_Recid = {_InvoiceRecID}
                                    ) tblInvoiceDet "
            End If


            Using daSQL As New SqlDataAdapter(strsql, ConnectionString)
                daSQL.Fill(_dtInvoiceInfo)
            End Using

        Catch ex As Exception
            Throw
        End Try
    End Sub

    Private Sub CreateAndDesignMatrixReportDT(_tmpTable As String, _dtMatrixReport As DataTable, _InvoiceCurrency As String, _TransactionType As String, lstMatrix As C1TrueDBGrid)
        Dim strsql As String
        Dim RecordNumber, maxNumberOfSizes As Integer
        Try

            _dtMatrixReport.Columns.Add("RecordNumber", GetType(System.Int32))
            _dtMatrixReport.Columns.Add("ColumnType", GetType(System.String))
            _dtMatrixReport.Columns.Add("ItemReference", GetType(System.String))
            _dtMatrixReport.Columns.Add("ItemDescription", GetType(System.String))
            If _TransactionType = "RO" Then
                _dtMatrixReport.Columns.Add("DOD_STAt", GetType(System.String))
            Else
                _dtMatrixReport.Columns.Add("MOD_Stat", GetType(System.String))
            End If
            _dtMatrixReport.Columns.Add("ItemColor", GetType(System.String))

            Using dtItemsGrids, dtItemsTotals As New DataTable

                Using daSQL As New SqlDataAdapter("", ConnectionString)

                    If _TransactionType = "PO" Then

                        strsql = $" SELECT DISTINCT [MORDQ_GridCode] , [STK_REF] , COUNT(DISTINCT [MORDQ_GridSize]) NumberOfSizes
                                FROM    {_tmpTable}
                                GROUP BY [MORDQ_GridCode] , [STK_REF]"

                    Else

                        strsql = $" SELECT DISTINCT [DOQ_GridCode] , [STK_REF] , COUNT(DISTINCT [DOQ_GridSize]) NumberOfSizes
                                FROM    {_tmpTable}
                                GROUP BY [DOQ_GridCode] , [STK_REF]"

                    End If
                    daSQL.SelectCommand.CommandText = strsql
                    daSQL.Fill(dtItemsGrids)


                    If _TransactionType = "PO" Then

                        strsql = $" SELECT STK_REF Barcode ,
                                    ISNULL(MORDQ_Color, '') ColorCode ,
                                    SUM(MOD_PURQTY) SumQty ,
                                    SUM(MOD_NetTot) SumNet ,
                                    SUM(TotalNetLC) TotalNetLC ,
                                    SUM(TotalNetCC) TotalNetCC ,
                                    Mod_unitCost [UnitCost],
                                    UnitCostLC [UnitCostLC],
                                    UnitCostCC [UnitCostCC],
                                    mod_itmdisc [UnitDisc],
                                    mod_stat
                                FROM   {_tmpTable}
                                GROUP BY STK_REF ,
                                    ISNULL(MORDQ_Color, ''),
                                    Mod_unitCost ,
                                    mod_itmdisc,
                                    [UnitCostLC] ,
                                    [UnitCostCC],
                                    mod_stat"

                    Else

                        'strsql = $" SELECT STK_REF Barcode ,
                        '            ISNULL(DOD_ColorCode, '') ColorCode ,
                        '            SUM(MOD_PURQTY) SumQty 
                        '      --    ,  SUM(MOD_NetTot) SumNet
                        '        FROM   {_tmpTable}
                        '        GROUP BY STK_REF ,
                        '            ISNULL(DOD_ColorCode, '') ; "

                        strsql = $"  SELECT STK_REF Barcode ,
                                            ISNULL(DOD_ColorCode, '') ColorCode ,
                                            SUM(MOD_PURQTY) SumQty ,
                                            SUM(dod_netcostLC) SumNetCostLC ,
                                            SUM(dod_netcostCC) SumNetCostCC ,
                                            DOD_STAt
                                     FROM   {_tmpTable}
                                     GROUP BY STK_REF ,
                                            DOD_STAt ,
                                            ISNULL(DOD_ColorCode, '')"

                    End If
                    daSQL.SelectCommand.CommandText = strsql
                    daSQL.Fill(dtItemsTotals)

                End Using

                If dtItemsGrids.Rows.Count > 0 Then

                    'Get max number of sizes
                    RecordNumber = 0
                    maxNumberOfSizes = CInt(dtItemsGrids.Compute("MAX(NumberOfSizes)", " 1=1 "))

                    'start by building the datatable structure

                    For i As Integer = 1 To maxNumberOfSizes
                        _dtMatrixReport.Columns.Add($"Col{i.ToString}")
                    Next

                    If _TransactionType <> "RO" Then
                        _dtMatrixReport.Columns.Add("UnitCost", GetType(System.Double))
                        _dtMatrixReport.Columns.Add("UnitCostLC", GetType(System.Double))
                        _dtMatrixReport.Columns.Add("UnitCostCC", GetType(System.Double))
                        _dtMatrixReport.Columns.Add("UnitDisc", GetType(System.Double))
                        _dtMatrixReport.Columns.Add("TotalNet", GetType(System.Double))
                        _dtMatrixReport.Columns.Add("TotalNetLC", GetType(System.Double))
                        _dtMatrixReport.Columns.Add("TotalNetCC", GetType(System.Double))
                    Else
                        _dtMatrixReport.Columns.Add("TotalQty", GetType(System.Double))
                        _dtMatrixReport.Columns.Add("TotalNetCostLC", GetType(System.Double))
                        _dtMatrixReport.Columns.Add("TotalNetCostCC", GetType(System.Double))
                    End If

                    If _TransactionType = "PO" Then _dtMatrixReport.Columns.Add("TotalQty", GetType(System.Double))

                    If _TransactionType = "PI" Then
                        _dtMatrixReport.Columns.Add("RetailPriceLC", GetType(System.Double))
                        _dtMatrixReport.Columns.Add("RetailPriceCC", GetType(System.Double))
                    End If

                    For Each itemsGrid In dtItemsGrids.Select()

                        If _TransactionType = "PO" Then

                            strsql = $" DECLARE @Sizes NVARCHAR(max);
                                   DECLARE @Query NVARCHAR(max);

                                   SET @Sizes = ( SELECT '[' + MORDQ_GridSize + '] ,' AS 'data()'
                                                  FROM   ( SELECT DISTINCT
                                                           GD_Order, ISNULL(NULLIF([MORDQ_GridSize], ''), 'ALL') [MORDQ_GridSize]
                                                           FROM  {_tmpTable}
                                                           LEFT JOIN dbo.stk04_DetGRID ON GD_Code = '{itemsGrid("MORDQ_GridCode").ToString}' AND GD_Code = MORDQ_GridCode AND GD_SizeDesc = MORDQ_GridSize
                                                           WHERE ISNULL(NULLIF([MORDQ_GridCode], ''), '*-*-') = '{itemsGrid("MORDQ_GridCode").ToString}'
                                                             AND STK_REF = '{itemsGrid("STK_REF").ToString}'
                                                          ) s
                                                  ORDER BY GD_Order
                                                 FOR
                                                  XML PATH('')
                                                 )   

                                   SET @Sizes = STUFF(@Sizes, LEN(@Sizes), 1, '')  

                                   SET @Query = N'
                                   SELECT * FROM   
                                   (
                                       SELECT STK_REF [Reference], 
                                              STK_DESC [Description],
                                              MOD_Stat  [Status],
                                              MORDQ_Color [ColorCode],
                                              Color_Reference [ColorDesc],
                                              ISNULL(NULLIF(MORDQ_GridSize, ''''), ''ALL'') [MORDQ_GridSize],
                                              MOD_PURQTY
                                       FROM 
                                           {_tmpTable}
                                       WHERE ISNULL(NULLIF([MORDQ_GridCode], ''''), ''*-*-'')  = ''{itemsGrid("MORDQ_GridCode").ToString}'' AND STK_REF = ''{itemsGrid("STK_REF").ToString}''

                                   ) t 
                                   PIVOT(
                                       SUM(MOD_PURQTY) 
                                       FOR MORDQ_GridSize IN (
                                        ' + @Sizes + ' )
                                   ) AS pivot_table
                                   ORDER BY [Reference], [Description], [ColorCode], [ColorDesc] '

                                   EXEC  (@Query)
                                   "

                        Else

                            strsql = $" DECLARE @Sizes NVARCHAR(max);
                                   DECLARE @Query NVARCHAR(max);

                                   SET @Sizes = ( SELECT '[' + DOQ_GridSize + '] ,' AS 'data()'
                                                  FROM   ( SELECT DISTINCT
                                                           GD_Order, ISNULL(NULLIF([DOQ_GridSize], ''), 'ALL') [DOQ_GridSize]
                                                           FROM  {_tmpTable}
                                                           LEFT JOIN dbo.stk04_DetGRID ON GD_Code = '{itemsGrid("DOQ_GridCode").ToString}' AND GD_Code = DOQ_GridCode AND GD_SizeDesc = DOQ_GridSize
                                                           WHERE ISNULL(NULLIF([DOQ_GridCode], ''), '*-*-') = '{itemsGrid("DOQ_GridCode").ToString}'
                                                             AND STK_REF = '{itemsGrid("STK_REF").ToString}'
                                                          ) s
                                                  ORDER BY GD_Order
                                                 FOR
                                                  XML PATH('')
                                                 )   

                                   SET @Sizes = STUFF(@Sizes, LEN(@Sizes), 1, '')  

                                   SET @Query = N'
                                   SELECT * FROM   
                                   (
                                       SELECT STK_REF [Reference], 
                                              STK_DESC [Description],
                                              dod_stat [Status],
                                              DOD_ColorCode [ColorCode],
                                              Color_Reference [ColorDesc],
                                              ISNULL(NULLIF(DOQ_GridSize, ''''), ''ALL'') [DOQ_GridSize],
                                              MOD_PURQTY
                                       FROM 
                                           {_tmpTable}
                                       WHERE ISNULL(NULLIF([DOQ_GridCode], ''''), ''*-*-'')  = ''{itemsGrid("DOQ_GridCode").ToString}'' AND STK_REF = ''{itemsGrid("STK_REF").ToString}''
                                       and MOD_PURQTY <> 0
                                   ) t 
                                   PIVOT(
                                       SUM(MOD_PURQTY) 
                                       FOR DOQ_GridSize IN (
                                        ' + @Sizes + ' )
                                   ) AS pivot_table
                                   ORDER BY [Reference], [Description], [ColorCode], [ColorDesc] '

                                   EXEC  (@Query)
                                   "

                        End If


                        Using dtGrid As New DataTable

                            Using daGrid As New SqlDataAdapter(strsql, ConnectionString)
                                daGrid.Fill(dtGrid)
                            End Using

                            If dtGrid.Rows.Count > 0 Then

                                Dim mdr As DataRow

                                mdr = _dtMatrixReport.NewRow

                                RecordNumber += 1
                                mdr("RecordNumber") = RecordNumber
                                mdr("ColumnType") = "H"

                                If itemsGrid(IIf(_TransactionType = "PI", "INVQ_GridCode", IIf(_TransactionType = "PO", "MORDQ_GridCode", "DOQ_GridCode")).ToString).ToString = "*-*-" Then
                                    mdr("ItemReference") = "Other"
                                    mdr("ItemDescription") = "Trading Items"
                                Else
                                    mdr("ItemReference") = dtGrid(0)("Reference").ToString
                                    mdr("ItemDescription") = dtGrid(0)("Description").ToString
                                End If

                                mdr("ItemColor") = ""

                                If _TransactionType = "PO" Then
                                    For i As Integer = 5 To dtGrid.Columns.Count - 1

                                        mdr($"Col{(i - 4).ToString}") = "'" & dtGrid.Columns(i).ColumnName.ToString

                                        If itemsGrid(IIf(_TransactionType = "PI", "INVQ_GridCode", IIf(_TransactionType = "PO", "MORDQ_GridCode", "DOQ_GridCode")).ToString).ToString <> "*-*-" Then

                                            For Each rec In dtItemsTotals.Select($" Barcode = '{dtGrid(0)("Reference").ToString}' ")

                                                mdr("TotalQty") = CDbl(dtItemsTotals.Compute("SUM(SumQty)", $" Barcode = '{dtGrid(0)("Reference").ToString}' "))
                                                mdr("TotalNet") = CDbl(dtItemsTotals.Compute("SUM(SumNet)", $" Barcode = '{dtGrid(0)("Reference").ToString}' "))
                                                mdr("TotalNetLC") = CDbl(dtItemsTotals.Compute("SUM(TotalNetLC)", $" Barcode = '{dtGrid(0)("Reference").ToString}' "))
                                                mdr("TotalNetCC") = CDbl(dtItemsTotals.Compute("SUM(TotalNetCC)", $" Barcode = '{dtGrid(0)("Reference").ToString}' "))
                                                mdr("UnitCost") = rec("UnitCost").ToString
                                                mdr("UnitCostLC") = rec("UnitCostLC").ToString
                                                mdr("UnitCostCC") = rec("UnitCostCC").ToString
                                                mdr("UnitDisc") = rec("UnitDisc").ToString
                                                mdr("MOD_STAT") = ""

                                            Next

                                        End If

                                    Next
                                Else

                                    For i As Integer = 5 To dtGrid.Columns.Count - 1

                                        mdr($"Col{(i - 4).ToString}") = dtGrid.Columns(i).ColumnName.ToString

                                        If itemsGrid("DOQ_GridCode").ToString <> "*-*-" Then

                                            For Each rec In dtItemsTotals.Select($" Barcode = '{dtGrid(0)("Reference").ToString}'")

                                                mdr("TotalQty") = CDbl(dtItemsTotals.Compute("SUM(SumQty)", $" Barcode = '{dtGrid(0)("Reference").ToString}' "))
                                                mdr("TotalNetCostLC") = CDbl(dtItemsTotals.Compute("SUM(SumNetCostLC)", $" Barcode = '{dtGrid(0)("Reference").ToString}'  "))
                                                mdr("TotalNetCostCC") = CDbl(dtItemsTotals.Compute("SUM(SumNetCostCC)", $" Barcode = '{dtGrid(0)("Reference").ToString}'  "))
                                                mdr("DOD_STAT") = ""

                                            Next

                                        End If

                                    Next

                                End If


                                _dtMatrixReport.Rows.Add(mdr)

                                For Each rw In dtGrid.Select()

                                    mdr = _dtMatrixReport.NewRow

                                    RecordNumber += 1
                                    mdr("RecordNumber") = RecordNumber
                                    mdr("ColumnType") = "D"

                                    If itemsGrid(IIf(_TransactionType = "PI", "INVQ_GridCode", IIf(_TransactionType = "PO", "MORDQ_GridCode", "DOQ_GridCode")).ToString).ToString = "*-*-" Then
                                        mdr("ItemReference") = rw("Reference").ToString
                                        mdr("ItemDescription") = rw("Description").ToString
                                    Else
                                        mdr("ItemReference") = ""
                                        mdr("ItemDescription") = ""
                                    End If

                                    mdr("ItemColor") = rw("ColorDesc").ToString

                                    If _TransactionType = "PO" Then
                                        For i As Integer = 5 To dtGrid.Columns.Count - 1

                                            mdr($"Col{(i - 4).ToString}") = rw(dtGrid.Columns(i).ColumnName.ToString).ToString

                                            For Each rec In dtItemsTotals.Select($" Barcode = '{rw("Reference").ToString.Replace("'", "''")}' 
                                                                            AND ColorCode = '{rw("ColorCode").ToString.Replace("'", "''")}'
                                                                            and mod_stat = '{rw("Status").ToString.Replace("'", "''")}'")

                                                mdr("TotalQty") = CDbl(rec("SumQty"))

                                                If _TransactionType <> "RO" Then

                                                    mdr("TotalNet") = CDbl(rec("SumNet"))
                                                    mdr("TotalNetLC") = CDbl(rec("TotalNetLC"))
                                                    mdr("TotalNetCC") = CDbl(rec("TotalNetCC"))
                                                    mdr("UnitCost") = rec("UnitCost").ToString
                                                    mdr("UnitCostLC") = rec("UnitCostLC").ToString
                                                    mdr("UnitCostCC") = rec("UnitCostCC").ToString
                                                    mdr("UnitDisc") = rec("UnitDisc").ToString
                                                    mdr("MOD_STAT") = rec("MOD_STAT").ToString
                                                End If
                                            Next
                                        Next
                                    Else
                                        For i As Integer = 5 To dtGrid.Columns.Count - 1

                                            mdr($"Col{(i - 4).ToString}") = rw(dtGrid.Columns(i).ColumnName.ToString).ToString

                                            For Each rec In dtItemsTotals.Select($" Barcode = '{rw("Reference").ToString.Replace("'", "''")}' 
                                                                            AND ColorCode = '{rw("ColorCode").ToString.Replace("'", "''")}' AND DOD_STAT = '{rw("STATUS").ToString.Replace("'", "''")}' ")

                                                mdr("TotalQty") = CDbl(rec("SumQty"))
                                                mdr("TotalNetCostLC") = CDbl(rec("SumNetCostLC"))
                                                mdr("TotalNetCostCC") = CDbl(rec("SumNetCostCC"))
                                                mdr("DOD_STAT") = rec("DOD_STAT").ToString
                                            Next
                                        Next
                                    End If



                                    _dtMatrixReport.Rows.Add(mdr)

                                Next

                            End If

                        End Using

                    Next

                Else

                    _dtMatrixReport.Columns.Add("TotalQty", GetType(System.Double))
                    If _TransactionType <> "RO" Then _dtMatrixReport.Columns.Add("TotalNet", GetType(System.Double))

                End If

            End Using

            With lstMatrix

                .DataSource = Nothing
                .DataSource = _dtMatrixReport
                .AllowSort = False
                .AllowUpdate = False
                .AllowDelete = False
                .AllowAddNew = False
                .FetchRowStyles = True
                .AllowFilter = False
                .FilterBar = False

                With .Splits(0).DisplayColumns("RecordNumber")
                    .Visible = False
                    .AllowSizing = False
                End With

                With .Splits(0).DisplayColumns("ColumnType")
                    .Visible = False
                    .AllowSizing = False
                End With

                With .Splits(0).DisplayColumns("ItemReference")
                    .DataColumn.Caption = "Reference"
                    .Style.HorizontalAlignment = AlignHorzEnum.Near
                    .Style.VerticalAlignment = AlignVertEnum.Center
                    .Merge = True
                    .Width = 140 '120
                End With

                With .Splits(0).DisplayColumns("ItemDescription")
                    .DataColumn.Caption = "Item Description"
                    .Style.HorizontalAlignment = AlignHorzEnum.Near
                    .Style.VerticalAlignment = AlignVertEnum.Center
                    .Merge = True
                    .Width = 250
                End With

                With .Splits(0).DisplayColumns("ItemColor")
                    .DataColumn.Caption = "Color"
                    .Style.HorizontalAlignment = AlignHorzEnum.Center
                    .Style.VerticalAlignment = AlignVertEnum.Center
                    .Width = 90
                End With

                If maxNumberOfSizes > 0 Then

                    For i As Integer = 1 To maxNumberOfSizes
                        With .Splits(0).DisplayColumns($"Col{i.ToString}")
                            .DataColumn.Caption = $"Size {i.ToString}"
                            .Style.HorizontalAlignment = AlignHorzEnum.Center
                            .Style.VerticalAlignment = AlignVertEnum.Center
                            .DataColumn.NumberFormat = $"N" & gbDecNo
                            .Style.BackColor = Color.FromArgb(230, 230, 230)
                            .Width = 70
                        End With
                    Next

                End If

                If _TransactionType <> "RO" Then


                    With .Splits(0).DisplayColumns("UnitCost")
                        .DataColumn.Caption = "Unit Cost"
                        .Style.HorizontalAlignment = AlignHorzEnum.Center
                        .Style.VerticalAlignment = AlignVertEnum.Center
                        .DataColumn.NumberFormat = $"N" & gbDecNo
                        .Width = 100
                    End With

                    With .Splits(0).DisplayColumns("UnitCostLC")
                        .DataColumn.Caption = "Unit Cost " & gbCurr1
                        .Style.HorizontalAlignment = AlignHorzEnum.Center
                        .Style.VerticalAlignment = AlignVertEnum.Center
                        .DataColumn.NumberFormat = $"N" & gbDecNo
                        .Width = 100
                    End With

                    With .Splits(0).DisplayColumns("UnitCostCC")
                        .DataColumn.Caption = "Unit Cost " & gbCurr2
                        .Style.HorizontalAlignment = AlignHorzEnum.Center
                        .Style.VerticalAlignment = AlignVertEnum.Center
                        .DataColumn.NumberFormat = $"N" & gbDecNo
                        .Width = 100
                    End With

                    With .Splits(0).DisplayColumns("UnitDisc")
                        .DataColumn.Caption = "Unit Discount"
                        .Style.HorizontalAlignment = AlignHorzEnum.Center
                        .Style.VerticalAlignment = AlignVertEnum.Center
                        .DataColumn.NumberFormat = $"N" & gbDecNo
                        .Width = 100
                    End With

                    With .Splits(0).DisplayColumns("TotalNet")
                        .DataColumn.Caption = $"Net Total {_InvoiceCurrency}"
                        .Style.HorizontalAlignment = AlignHorzEnum.Center
                        .Style.VerticalAlignment = AlignVertEnum.Center
                        If _InvoiceCurrency = gbCurr1 Then .DataColumn.NumberFormat = "#,##0" Else .DataColumn.NumberFormat = "#,##0.000"
                        .Width = 130
                    End With

                    With .Splits(0).DisplayColumns("TotalNetLC")
                        .DataColumn.Caption = $"Net Total {gbCurr1}"
                        .Style.HorizontalAlignment = AlignHorzEnum.Center
                        .Style.VerticalAlignment = AlignVertEnum.Center
                        If _InvoiceCurrency = gbCurr1 Then .DataColumn.NumberFormat = "#,##0" Else .DataColumn.NumberFormat = "#,##0.000"
                        .Width = 130
                    End With

                    With .Splits(0).DisplayColumns("TotalNetCC")
                        .DataColumn.Caption = $"Net Total {gbCurr2}"
                        .Style.HorizontalAlignment = AlignHorzEnum.Center
                        .Style.VerticalAlignment = AlignVertEnum.Center
                        If _InvoiceCurrency = gbCurr1 Then .DataColumn.NumberFormat = "#,##0" Else .DataColumn.NumberFormat = "#,##0.000"
                        .Width = 130
                    End With

                End If


                With .Splits(0).DisplayColumns("TotalQty")
                    .DataColumn.Caption = "Total Qty"
                    .Style.HorizontalAlignment = AlignHorzEnum.Center
                    .Style.VerticalAlignment = AlignVertEnum.Center
                    .DataColumn.NumberFormat = $"N" & gbDecNo
                    .Width = 100
                End With

                For i As Integer = 0 To .Columns.Count - 1
                    .Splits(0).DisplayColumns(i).HeadingStyle.HorizontalAlignment = AlignHorzEnum.Center
                    .Splits(0).DisplayColumns(i).HeadingStyle.VerticalAlignment = AlignVertEnum.Center
                    .Splits(0).DisplayColumns(i).AllowSizing = False

                    If _dtMatrixReport.Columns(i).DataType Is System.Type.GetType("System.String") Then
                        .Splits(0).DisplayColumns(i).DataColumn.FilterOperator = "LIKE"
                    Else
                        .Splits(0).DisplayColumns(i).DataColumn.FilterOperator = "="
                    End If
                Next i

            End With

        Catch ex As Exception
            Throw
        End Try
    End Sub

    Private Sub CreateAndDesignMatrixReportDTPI(_tmpTable As String, _dtMatrixReport As DataTable, _InvoiceCurrency As String, lstMatrix As C1TrueDBGrid)
        Dim strsql As String
        Dim RecordNumber, maxNumberOfSizes As Integer
        Try

            _dtMatrixReport.Columns.Add("RecordNumber", GetType(System.Int32))
            _dtMatrixReport.Columns.Add("ColumnType", GetType(System.String))
            _dtMatrixReport.Columns.Add("ItemReference", GetType(System.String))
            _dtMatrixReport.Columns.Add("ItemDescription", GetType(System.String))
            _dtMatrixReport.Columns.Add("INVD_STATUS", GetType(System.String))
            _dtMatrixReport.Columns.Add("INVD_LOCATION", GetType(System.String))
            _dtMatrixReport.Columns.Add("ItemColor", GetType(System.String))

            Using dtItemsGrids, dtItemsTotals As New DataTable

                Using daSQL As New SqlDataAdapter("", ConnectionString)

                    strsql = $" SELECT DISTINCT [INVQ_GridCode] , [STK_REF] , COUNT(DISTINCT [INVQ_GridSize]) NumberOfSizes
                                FROM    {_tmpTable}
                                GROUP BY [INVQ_GridCode] , [STK_REF]"


                    daSQL.SelectCommand.CommandText = strsql
                    daSQL.Fill(dtItemsGrids)


                    strsql = $"  SELECT STK_REF Barcode ,
                                            ISNULL(INVQ_Color, '') ColorCode ,
                                            SUM(INVD_PURQTY) SumQty ,
                                            SUM(INVD_TNCOST) SumNet ,
                                            SUM(TotalNetLC) TotalNetLC ,
                                            SUM(TotalNetCC) TotalNetCC ,
                                            INVD_LOCATION ,
                                            INVD_STATUS ,
                                            INVD_UCOST [UnitCost] ,
                                            UnitCostLC [UnitCostLC] ,
                                            UnitCostCC [UnitCostCC] ,
                                            INVD_ITMDISC [UnitDisc] ,
                                            invd_retLC [RetailPriceLC] ,
                                            invd_retCC [RetailPriceCC]
                                     FROM   {_tmpTable}
                                     GROUP BY STK_REF ,
                                            ISNULL(INVQ_Color, '') ,
                                            INVD_UCOST ,
                                            INVD_ITMDISC ,
                                            [UnitCostLC] ,
                                            [UnitCostCC] ,
                                            INVD_LOCATION ,
                                            INVD_STATUS ,
                                            invd_retLC ,
                                            invd_retCC "

                    daSQL.SelectCommand.CommandText = strsql
                    daSQL.Fill(dtItemsTotals)

                End Using

                If dtItemsGrids.Rows.Count > 0 Then

                    'Get max number of sizes
                    RecordNumber = 0
                    maxNumberOfSizes = CInt(dtItemsGrids.Compute("MAX(NumberOfSizes)", " 1=1 "))

                    'start by building the datatable structure

                    For i As Integer = 1 To maxNumberOfSizes
                        _dtMatrixReport.Columns.Add($"Col{i.ToString}")
                    Next

                    _dtMatrixReport.Columns.Add("TotalQty", GetType(System.Double))
                    _dtMatrixReport.Columns.Add("UnitCost", GetType(System.Double))
                    _dtMatrixReport.Columns.Add("UnitCostLC", GetType(System.Double))
                    _dtMatrixReport.Columns.Add("UnitCostCC", GetType(System.Double))
                    _dtMatrixReport.Columns.Add("RetailPriceLC", GetType(System.Double))
                    _dtMatrixReport.Columns.Add("RetailPriceCC", GetType(System.Double))
                    _dtMatrixReport.Columns.Add("UnitDisc", GetType(System.Double))
                    _dtMatrixReport.Columns.Add("TotalNet", GetType(System.Double))
                    _dtMatrixReport.Columns.Add("TotalNetLC", GetType(System.Double))
                    _dtMatrixReport.Columns.Add("TotalNetCC", GetType(System.Double))

                    For Each itemsGrid In dtItemsGrids.Select()


                        strsql = $" DECLARE @Sizes NVARCHAR(max);
                                   DECLARE @Query NVARCHAR(max);

                                   SET @Sizes = ( SELECT '[' + INVQ_GridSize + '] ,' AS 'data()'
                                                  FROM   ( SELECT DISTINCT
                                                           GD_Order, ISNULL(NULLIF([INVQ_GridSize], ''), 'ALL') [INVQ_GridSize]
                                                           FROM  {_tmpTable}
                                                           LEFT JOIN dbo.stk04_DetGRID ON GD_Code = '{itemsGrid("INVQ_GridCode").ToString}' AND GD_Code = INVQ_GridCode AND GD_SizeDesc = INVQ_GridSize
                                                           WHERE ISNULL(NULLIF([INVQ_GridCode], ''), '*-*-') = '{itemsGrid("INVQ_GridCode").ToString}'
                                                             AND STK_REF = '{itemsGrid("STK_REF").ToString}'
                                                          ) s
                                                  ORDER BY GD_Order
                                                 FOR
                                                  XML PATH('')
                                                 )   

                                   SET @Sizes = STUFF(@Sizes, LEN(@Sizes), 1, '')  

                                   SET @Query = N'
                                   SELECT * FROM   
                                   (
                                       SELECT STK_REF [Reference], 
                                              STK_DESC [Description],
                                              INVD_Status [Status],
                                              INVQ_Color [ColorCode],
                                              Color_Reference [ColorDesc],
                                              ISNULL(NULLIF(INVQ_GridSize, ''''), ''ALL'') [INVQ_GridSize],
                                              INVD_PURQTY
                                       FROM 
                                           {_tmpTable}
                                       WHERE ISNULL(NULLIF([INVQ_GridCode], ''''), ''*-*-'')  = ''{itemsGrid("INVQ_GridCode").ToString}'' AND STK_REF = ''{itemsGrid("STK_REF").ToString}''

                                   ) t 
                                   PIVOT(
                                       SUM(INVD_PURQTY) 
                                       FOR INVQ_GridSize IN (
                                        ' + @Sizes + ' )
                                   ) AS pivot_table
                                   ORDER BY [Reference], [Description], [ColorCode], [ColorDesc] '

                                   EXEC  (@Query)
                                   "




                        Using dtGrid As New DataTable

                            Using daGrid As New SqlDataAdapter(strsql, ConnectionString)
                                daGrid.Fill(dtGrid)
                            End Using

                            If dtGrid.Rows.Count > 0 Then

                                Dim mdr As DataRow

                                mdr = _dtMatrixReport.NewRow

                                RecordNumber += 1
                                mdr("RecordNumber") = RecordNumber
                                mdr("ColumnType") = "H"

                                If itemsGrid("INVQ_GridCode").ToString = "*-*-" Then
                                    mdr("ItemReference") = "Other"
                                    mdr("ItemDescription") = "Trading Items"
                                Else
                                    mdr("ItemReference") = dtGrid(0)("Reference").ToString
                                    mdr("ItemDescription") = dtGrid(0)("Description").ToString
                                End If

                                mdr("ItemColor") = ""

                                For i As Integer = 5 To dtGrid.Columns.Count - 1

                                    mdr($"Col{(i - 4).ToString}") = "'" & dtGrid.Columns(i).ColumnName.ToString

                                    If itemsGrid("INVQ_GridCode").ToString <> "*-*-" Then

                                        For Each rec In dtItemsTotals.Select($" Barcode = '{dtGrid(0)("Reference").ToString}'")

                                            mdr("TotalQty") = CDbl(dtItemsTotals.Compute("SUM(SumQty)", $" Barcode = '{dtGrid(0)("Reference").ToString}' "))
                                            mdr("TotalNet") = CDbl(dtItemsTotals.Compute("SUM(SumNet)", $" Barcode = '{dtGrid(0)("Reference").ToString}' "))
                                            mdr("TotalNetLC") = CDbl(dtItemsTotals.Compute("SUM(TotalNetLC)", $" Barcode = '{dtGrid(0)("Reference").ToString}'  "))
                                            mdr("TotalNetCC") = CDbl(dtItemsTotals.Compute("SUM(TotalNetCC)", $" Barcode = '{dtGrid(0)("Reference").ToString}'  "))
                                            mdr("UnitCost") = rec("UnitCost").ToString
                                            mdr("UnitCostLC") = rec("UnitCostLC").ToString
                                            mdr("UnitCostCC") = rec("UnitCostCC").ToString
                                            mdr("UnitDisc") = rec("UnitDisc").ToString
                                            mdr("RetailPriceLC") = rec("RetailPriceLC").ToString
                                            mdr("RetailPriceCC") = rec("RetailPriceCC").ToString
                                            mdr("INVD_LOCATION") = ""
                                            mdr("INVD_STATUS") = ""

                                        Next

                                    End If

                                Next

                                _dtMatrixReport.Rows.Add(mdr)

                                For Each rw In dtGrid.Select()

                                    mdr = _dtMatrixReport.NewRow

                                    RecordNumber += 1
                                    mdr("RecordNumber") = RecordNumber
                                    mdr("ColumnType") = "D"

                                    If itemsGrid("INVQ_GridCode").ToString = "*-*-" Then
                                        mdr("ItemReference") = rw("Reference").ToString
                                        mdr("ItemDescription") = rw("Description").ToString
                                    Else
                                        mdr("ItemReference") = ""
                                        mdr("ItemDescription") = ""
                                    End If

                                    mdr("ItemColor") = rw("ColorDesc").ToString

                                    For i As Integer = 5 To dtGrid.Columns.Count - 1

                                        mdr($"Col{(i - 4).ToString}") = rw(dtGrid.Columns(i).ColumnName.ToString).ToString

                                        For Each rec In dtItemsTotals.Select($" Barcode = '{rw("Reference").ToString.Replace("'", "''")}' 
                                                                            AND ColorCode = '{rw("ColorCode").ToString.Replace("'", "''")}' AND INVD_STATUS = '{rw("STATUS").ToString.Replace("'", "''")}' ")

                                            mdr("TotalQty") = CDbl(rec("SumQty"))
                                            mdr("TotalNet") = CDbl(rec("SumNet"))
                                            mdr("TotalNetLC") = CDbl(rec("TotalNetLC"))
                                            mdr("TotalNetCC") = CDbl(rec("TotalNetCC"))
                                            mdr("UnitCost") = rec("UnitCost").ToString
                                            mdr("UnitCostLC") = rec("UnitCostLC").ToString
                                            mdr("UnitCostCC") = rec("UnitCostCC").ToString
                                            mdr("UnitDisc") = rec("UnitDisc").ToString
                                            mdr("RetailPriceLC") = CDbl(rec("RetailPriceLC"))
                                            mdr("RetailPriceCC") = CDbl(rec("RetailPriceCC"))
                                            mdr("INVD_LOCATION") = rec("INVD_LOCATION").ToString
                                            mdr("INVD_STATUS") = rec("INVD_STATUS").ToString
                                        Next
                                    Next

                                    _dtMatrixReport.Rows.Add(mdr)

                                Next

                            End If

                        End Using

                    Next

                Else

                    _dtMatrixReport.Columns.Add("TotalQty", GetType(System.Double))
                    _dtMatrixReport.Columns.Add("TotalNet", GetType(System.Double))

                End If

            End Using

            With lstMatrix

                .DataSource = Nothing
                .DataSource = _dtMatrixReport
                .AllowSort = False
                .AllowUpdate = False
                .AllowDelete = False
                .AllowAddNew = False
                .FetchRowStyles = True
                .AllowFilter = False
                .FilterBar = False

                With .Splits(0).DisplayColumns("RecordNumber")
                    .Visible = False
                    .AllowSizing = False
                End With

                With .Splits(0).DisplayColumns("ColumnType")
                    .Visible = False
                    .AllowSizing = False
                End With

                With .Splits(0).DisplayColumns("ItemReference")
                    .DataColumn.Caption = "Reference"
                    .Style.HorizontalAlignment = AlignHorzEnum.Near
                    .Style.VerticalAlignment = AlignVertEnum.Center
                    .Merge = True
                    .Width = 140 '120
                End With

                With .Splits(0).DisplayColumns("ItemDescription")
                    .DataColumn.Caption = "Item Description"
                    .Style.HorizontalAlignment = AlignHorzEnum.Near
                    .Style.VerticalAlignment = AlignVertEnum.Center
                    .Merge = True
                    .Width = 250
                End With

                With .Splits(0).DisplayColumns("ItemColor")
                    .DataColumn.Caption = "Color"
                    .Style.HorizontalAlignment = AlignHorzEnum.Center
                    .Style.VerticalAlignment = AlignVertEnum.Center
                    .Width = 90
                End With

                If maxNumberOfSizes > 0 Then

                    For i As Integer = 1 To maxNumberOfSizes
                        With .Splits(0).DisplayColumns($"Col{i.ToString}")
                            .DataColumn.Caption = $"Size {i.ToString}"
                            .Style.HorizontalAlignment = AlignHorzEnum.Center
                            .Style.VerticalAlignment = AlignVertEnum.Center
                            .DataColumn.NumberFormat = $"N" & gbDecNo
                            .Style.BackColor = Color.FromArgb(230, 230, 230)
                            .Width = 70
                        End With
                    Next

                End If


                With .Splits(0).DisplayColumns("UnitCost")
                        .DataColumn.Caption = "Unit Cost"
                        .Style.HorizontalAlignment = AlignHorzEnum.Center
                        .Style.VerticalAlignment = AlignVertEnum.Center
                        .DataColumn.NumberFormat = $"N" & gbDecNo
                        .Width = 100
                    End With

                    With .Splits(0).DisplayColumns("UnitCostLC")
                        .DataColumn.Caption = "Unit Cost " & gbCurr1
                        .Style.HorizontalAlignment = AlignHorzEnum.Center
                        .Style.VerticalAlignment = AlignVertEnum.Center
                        .DataColumn.NumberFormat = $"N" & gbDecNo
                        .Width = 100
                    End With

                    With .Splits(0).DisplayColumns("UnitCostCC")
                        .DataColumn.Caption = "Unit Cost " & gbCurr2
                        .Style.HorizontalAlignment = AlignHorzEnum.Center
                        .Style.VerticalAlignment = AlignVertEnum.Center
                        .DataColumn.NumberFormat = $"N" & gbDecNo
                        .Width = 100
                    End With

                    With .Splits(0).DisplayColumns("UnitDisc")
                        .DataColumn.Caption = "Unit Discount"
                        .Style.HorizontalAlignment = AlignHorzEnum.Center
                        .Style.VerticalAlignment = AlignVertEnum.Center
                        .DataColumn.NumberFormat = $"N" & gbDecNo
                        .Width = 100
                    End With

                    With .Splits(0).DisplayColumns("TotalNet")
                        .DataColumn.Caption = $"Net Total {_InvoiceCurrency}"
                        .Style.HorizontalAlignment = AlignHorzEnum.Center
                        .Style.VerticalAlignment = AlignVertEnum.Center
                        If _InvoiceCurrency = gbCurr1 Then .DataColumn.NumberFormat = "#,##0" Else .DataColumn.NumberFormat = "#,##0.000"
                        .Width = 130
                    End With

                    With .Splits(0).DisplayColumns("TotalNetLC")
                        .DataColumn.Caption = $"Net Total {gbCurr1}"
                        .Style.HorizontalAlignment = AlignHorzEnum.Center
                        .Style.VerticalAlignment = AlignVertEnum.Center
                        If _InvoiceCurrency = gbCurr1 Then .DataColumn.NumberFormat = "#,##0" Else .DataColumn.NumberFormat = "#,##0.000"
                        .Width = 130
                    End With

                    With .Splits(0).DisplayColumns("TotalNetCC")
                        .DataColumn.Caption = $"Net Total {gbCurr2}"
                        .Style.HorizontalAlignment = AlignHorzEnum.Center
                        .Style.VerticalAlignment = AlignVertEnum.Center
                        If _InvoiceCurrency = gbCurr1 Then .DataColumn.NumberFormat = "#,##0" Else .DataColumn.NumberFormat = "#,##0.000"
                        .Width = 130
                    End With



                    With .Splits(0).DisplayColumns("TotalQty")
                    .DataColumn.Caption = "Total Qty"
                    .Style.HorizontalAlignment = AlignHorzEnum.Center
                    .Style.VerticalAlignment = AlignVertEnum.Center
                    .DataColumn.NumberFormat = $"N" & gbDecNo
                    .Width = 100
                End With

                For i As Integer = 0 To .Columns.Count - 1
                    .Splits(0).DisplayColumns(i).HeadingStyle.HorizontalAlignment = AlignHorzEnum.Center
                    .Splits(0).DisplayColumns(i).HeadingStyle.VerticalAlignment = AlignVertEnum.Center
                    .Splits(0).DisplayColumns(i).AllowSizing = False

                    If _dtMatrixReport.Columns(i).DataType Is System.Type.GetType("System.String") Then
                        .Splits(0).DisplayColumns(i).DataColumn.FilterOperator = "LIKE"
                    Else
                        .Splits(0).DisplayColumns(i).DataColumn.FilterOperator = "="
                    End If
                Next i

            End With

        Catch ex As Exception
            Throw
        End Try
    End Sub

    Public AlphaRange() As String = {"Start", "A", "B", "C", "D", "E", "F", "G", "H", "I", "J", "K", "L", "M", "N",
                        "O", "P", "Q", "R", "S", "T", "U", "V", "W", "X", "Y", "Z", "END"}

    Private Sub CreateExcelReportPO(ByRef _FileName As String, ByRef _SheetName As String,
                          ByRef _GridStartCell As Integer, dtMatrixReport As DataTable,
                          colCount As Integer, OrderNo As String, OrderCurr As String, dtInvoiceInfo As DataTable, VatLC As String, VatFC As String)

        Dim xlsWorkBook As Excel.Workbook
        Dim xlsWorkSheet As Excel.Worksheet
        Dim xls As New Excel.Application
        Dim xlFilePath As String
        Dim i As Integer
        Dim columnCounter As Integer
        Dim totalQtySum As Double = 0 ' To hold the sum of TotalQty
        Dim totalNetSum As Double = 0 ' To hold the sum of TotalNet
        Dim totalNetSumLC As Double = 0 ' To hold the sum of TotalNetLC
        Dim totalNetSumCC As Double = 0 ' To hold the sum of TotalNetCC
        Dim AmountFormat As String
        Try

#If DEBUG Then
            xlFilePath = gbExePath & "\..\..\PurReports\" & _FileName
#Else
        xlFilePath = gbExePath & "\PurReports\" & _FileName
#End If

            If gbDecNo = 0 Then
                AmountFormat = "#,##0"
            Else
                AmountFormat = "#,##0."
                For d As Integer = 1 To gbDecNo
                    AmountFormat &= "0"
                Next
            End If

            xlsWorkBook = xls.Workbooks.Open(xlFilePath, , True)
            xlsWorkSheet = CType(xlsWorkBook.Sheets(_SheetName), Excel.Worksheet)

            ' Dictionary to map static column names to new header names
            Dim headerMapping As New Dictionary(Of String, String) From {
            {"ItemReference", "Reference"},
            {"ItemDescription", "Item Description"},
            {"MOD_STAT", "Type"},
            {"ItemColor", "Color"},
            {"TotalQty", "Total Qty"},
            {"UnitCost", "Unit Cost " & OrderCurr},
            {"UnitCostLC", "Unit Cost " & gbCurr1},
            {"UnitCostCC", "Unit Cost " & gbCurr2},
            {"UnitDisc", "Unit Discount %"},
            {"TotalNet", "Net Total " & OrderCurr},
            {"TotalNetLC", "Net Total " & gbCurr1},
            {"TotalNetCC", "Net Total " & gbCurr2}
            }

            ' Counter for dynamic size columns (Col1, Col2, etc.)
            Dim sizeCounter As Integer = 1

            With xlsWorkSheet
                .Range("CompanyName").FormulaR1C1 = gbCompName
                .Range("title").FormulaR1C1 = OrderNo
                .Range("ExportedBy").FormulaR1C1 = "Exported By " & gbUser & " On " & CDate(Now.Date)
                .Range("SupplierName").FormulaR1C1 = dtInvoiceInfo(0)("Sup_CodeName").ToString
                .Range("Date").FormulaR1C1 = dtInvoiceInfo(0)("RMO_DATE").ToString
                .Range("Expected_Date").FormulaR1C1 = dtInvoiceInfo(0)("RMO_DelDate").ToString
                .Range("Currency").FormulaR1C1 = dtInvoiceInfo(0)("invCurrency").ToString
                .Range("Location").FormulaR1C1 = dtInvoiceInfo(0)("LocationName").ToString
                .Range("Pricing_Type").FormulaR1C1 = dtInvoiceInfo(0)("PriceType").ToString
                .Range("Brand").FormulaR1C1 = dtInvoiceInfo(0)("Brand").ToString
                .Range("Season").FormulaR1C1 = dtInvoiceInfo(0)("Season").ToString
                .Range("PO_Status").FormulaR1C1 = dtInvoiceInfo(0)("RMO_POSTATUS").ToString


                ' Get the indices of "Total Qty" and other columns in the DataTable
                Dim totalQtyIndex As Integer = dtMatrixReport.Columns.IndexOf("TotalQty")
                Dim unitCostIndex As Integer = dtMatrixReport.Columns.IndexOf("UnitCost")

                ' Column counter to track position in Excel output, starting from the first column in Excel
                columnCounter = 1

                ' Skip RecordNumber and ColumnType, start from the third column in dtMatrixReport (ItemReference)
                For i = 3 To colCount + 2
                    Dim columnIndex As Integer = i - 1

                    ' Skip "Total Qty" at first; we'll handle it before "Unit Cost"
                    If columnIndex = totalQtyIndex Then
                        Continue For
                    End If

                    ' Insert "Total Qty" before "Unit Cost"
                    If columnIndex = unitCostIndex Then
                        ' Insert "Total Qty" column before "Unit Cost"
                        Dim totalQtyColumnName As String = dtMatrixReport.Columns(totalQtyIndex).ColumnName
                        Dim totalQtyCellAddress As String = AlphaRange(columnCounter) & CStr(_GridStartCell - 1)

                        ' Use the header mapping to get the new name
                        Dim totalQtyHeader As String = If(headerMapping.ContainsKey(totalQtyColumnName), headerMapping(totalQtyColumnName), totalQtyColumnName)

                        ' Set "Total Qty" header in Excel
                        With .Range(totalQtyCellAddress)
                            .Font.Color = RGB(128, 0, 0) ' Set font color to (128, 0, 0)
                            .FormulaR1C1 = totalQtyHeader
                            .Font.Bold = True
                            With .Borders
                                .LineStyle = Excel.XlLineStyle.xlContinuous
                                .Weight = Excel.XlBorderWeight.xlThin
                                .Color = RGB(0, 0, 0) ' Black border
                            End With
                            .Interior.Color = RGB(192, 192, 192)
                            .EntireColumn.ColumnWidth = 15
                        End With

                        ' Move to next column position in Excel
                        columnCounter += 1
                    End If

                    ' Output the current column (including "Unit Cost" when reached)
                    Dim columnName As String = dtMatrixReport.Columns(columnIndex).ColumnName
                    Dim cellAddress As String = AlphaRange(columnCounter) & CStr(_GridStartCell - 1)

                    ' Use the header mapping to get the new name or assign dynamic sizes
                    Dim headerName As String

                    If headerMapping.ContainsKey(columnName) Then
                        headerName = headerMapping(columnName)
                    ElseIf columnName.StartsWith("Col") Then
                        ' Assign "Size X" dynamically to columns starting with "Col"
                        headerName = "Size " & sizeCounter
                        sizeCounter += 1
                        .Range(cellAddress).Interior.Color = RGB(192, 192, 192)
                        .Range(cellAddress).EntireColumn.ColumnWidth = 15
                        .Range(cellAddress).NumberFormat = "@"
                    Else
                        headerName = columnName ' Default to original name if no mapping exists
                    End If

                    ' Set the header for the current column
                    With .Range(cellAddress)
                        .Font.Color = RGB(128, 0, 0) ' Set font color to (128, 0, 0) for headers
                        .FormulaR1C1 = headerName
                        .Font.Bold = True
                        With .Borders
                            .LineStyle = Excel.XlLineStyle.xlContinuous
                            .Weight = Excel.XlBorderWeight.xlThin
                            .Color = RGB(0, 0, 0) ' Black border
                        End With
                    End With

                    ' Set column widths based on header names immediately after defining the header
                    Select Case headerName
                        Case "Reference"
                            .Range(cellAddress).EntireColumn.ColumnWidth = 20 ' Width for Reference column
                            .Range(cellAddress).Interior.Color = RGB(192, 192, 192)
                        Case "Item Description"
                            .Range(cellAddress).EntireColumn.ColumnWidth = 40 ' Width for Item Description column
                            .Range(cellAddress).Interior.Color = RGB(192, 192, 192)
                        Case "MOD_Stat"
                            .Range(cellAddress).Value = "Type"
                            .Range(cellAddress).EntireColumn.ColumnWidth = 15 ' Width for Type column
                            .Range(cellAddress).Interior.Color = RGB(192, 192, 192)
                        Case "Color"
                            .Range(cellAddress).EntireColumn.ColumnWidth = 15 ' Width for Color column
                            .Range(cellAddress).Interior.Color = RGB(192, 192, 192)
                        Case "Unit Cost " & OrderCurr
                            .Range(cellAddress).EntireColumn.ColumnWidth = 15 ' Width for Unit Cost column
                            .Range(cellAddress).Interior.Color = RGB(192, 192, 192)
                        Case "Unit Cost " & gbCurr1
                            .Range(cellAddress).EntireColumn.ColumnWidth = 15 ' Width for Unit Cost LC column
                            .Range(cellAddress).Interior.Color = RGB(192, 192, 192)
                        Case "Unit Cost " & gbCurr2
                            .Range(cellAddress).EntireColumn.ColumnWidth = 15 ' Width for Unit Cost CC column
                            .Range(cellAddress).Interior.Color = RGB(192, 192, 192)
                        Case "Unit Discount %"
                            .Range(cellAddress).EntireColumn.ColumnWidth = 15 ' Width for Unit Discount column
                            .Range(cellAddress).Interior.Color = RGB(192, 192, 192)
                        Case "Net Total " & OrderCurr
                            .Range(cellAddress).EntireColumn.ColumnWidth = 15 ' Width for Net Total column
                            .Range(cellAddress).Interior.Color = RGB(192, 192, 192)
                        Case "Net Total " & gbCurr1
                            .Range(cellAddress).EntireColumn.ColumnWidth = 15 ' Width for Net Total LC column
                            .Range(cellAddress).Interior.Color = RGB(192, 192, 192)
                        Case "Net Total " & gbCurr2
                            .Range(cellAddress).EntireColumn.ColumnWidth = 15 ' Width for Net Total CC column
                            .Range(cellAddress).Interior.Color = RGB(192, 192, 192)
                        Case "Total Qty"
                            .Range(cellAddress).EntireColumn.ColumnWidth = 15 ' Width for Total Qty column
                            .Range(cellAddress).Interior.Color = RGB(192, 192, 192)
                    End Select

                    ' Move to next column position in Excel
                    columnCounter += 1
                Next

                ' Create a list to hold the size columns dynamically
                Dim sizeColumns As New List(Of String)

                ' Identify the dynamic size columns (those starting with "Col") that have ColumnType "H"
                For Each col As DataColumn In dtMatrixReport.Columns
                    If col.ColumnName.StartsWith("Col") Then
                        sizeColumns.Add(col.ColumnName)
                    End If
                Next

                ' Total number of dynamic size columns
                Dim sizeColumnCount As Integer = sizeColumns.Count

                ' Variable to track the current row in Excel
                Dim currentRow As Integer = _GridStartCell ' Start from the first row after the headers

                ' Loop through the DataTable to insert data into Excel and sum up totals
                For Each row As DataRow In dtMatrixReport.Rows
                    ' For header rows (ColumnType = "H")
                    If row("ColumnType").ToString() = "H" Then
                        ' Fill header columns (Reference, Item Description, Color, TotalQty, etc.)
                        .Range("A" & currentRow).Value = row("ItemReference")
                        .Range("B" & currentRow).Value = row("ItemDescription")
                        .Range("C" & currentRow).Value = row("MOD_STAT")
                        .Range("D" & currentRow).Value = row("ItemColor") ' Place color under "Color" column

                        ' Dynamically fill in the size columns (Col1, Col2, ..., ColN)
                        For i = 1 To sizeColumnCount - 1
                            Dim columnLetter As String = AlphaRange(4 + i) ' Start filling from the 5th column (D)
                            .Range(columnLetter & currentRow).Value = row(sizeColumns(i))
                            .Range(columnLetter & currentRow).Interior.Color = RGB(192, 192, 192) ' Light gray for header size columns
                        Next

                        ' Fill the remaining fixed columns (TotalQty) only if they exist
                        .Range(AlphaRange(4 + sizeColumnCount) & currentRow).Value = row("TotalQty")

                        ' Only fill UnitCost, UnitDisc, and TotalNet if the transaction type is not "RO"

                        If dtMatrixReport.Columns.Contains("UnitCost") Then
                            .Range(AlphaRange(5 + sizeColumnCount) & currentRow).Value = row("UnitCost")
                            .Range(AlphaRange(5 + sizeColumnCount) & currentRow).NumberFormat = AmountFormat
                        End If

                        If dtMatrixReport.Columns.Contains("UnitCostLC") Then
                            .Range(AlphaRange(6 + sizeColumnCount) & currentRow).Value = row("UnitCostLC")
                            .Range(AlphaRange(6 + sizeColumnCount) & currentRow).NumberFormat = AmountFormat
                        End If

                        If dtMatrixReport.Columns.Contains("UnitCostCC") Then
                            .Range(AlphaRange(7 + sizeColumnCount) & currentRow).Value = row("UnitCostCC")
                            .Range(AlphaRange(7 + sizeColumnCount) & currentRow).NumberFormat = AmountFormat
                        End If

                        If dtMatrixReport.Columns.Contains("UnitDisc") Then
                            .Range(AlphaRange(8 + sizeColumnCount) & currentRow).Value = row("UnitDisc")
                        End If

                        If dtMatrixReport.Columns.Contains("TotalNet") Then
                            .Range(AlphaRange(9 + sizeColumnCount) & currentRow).Value = row("TotalNet")
                            .Range(AlphaRange(9 + sizeColumnCount) & currentRow).NumberFormat = AmountFormat
                        End If

                        If dtMatrixReport.Columns.Contains("TotalNetLC") Then
                            .Range(AlphaRange(10 + sizeColumnCount) & currentRow).Value = row("TotalNetLC")
                            .Range(AlphaRange(10 + sizeColumnCount) & currentRow).NumberFormat = AmountFormat
                        End If

                        If dtMatrixReport.Columns.Contains("TotalNetCC") Then
                            .Range(AlphaRange(11 + sizeColumnCount) & currentRow).Value = row("TotalNetCC")
                            .Range(AlphaRange(11 + sizeColumnCount) & currentRow).NumberFormat = AmountFormat
                        End If

                        ' Sum up TotalQty and TotalNet values if they exist
                        totalQtySum += CDbl(row("TotalQty"))
                        totalNetSum += If(dtMatrixReport.Columns.Contains("TotalNet"), CDbl(row("TotalNet")), 0) ' Check before summing
                        totalNetSumLC += If(dtMatrixReport.Columns.Contains("TotalNetLC"), CDbl(row("TotalNetLC")), 0) ' Check before summing
                        totalNetSumCC += If(dtMatrixReport.Columns.Contains("TotalNetCC"), CDbl(row("TotalNetCC")), 0) ' Check before summing


                        ' Apply header row formatting and borders
                        With .Range("A" & currentRow & ":" & AlphaRange(11 + sizeColumnCount) & currentRow)
                            .Font.Bold = True
                            '.Interior.ColorIndex = xlNone ' No background color for header rows 
                        End With

                    ElseIf row("ColumnType").ToString() = "D" Then ' For detail rows (ColumnType = "D")
                        ' Fill only Reference, ItemDescription, and ItemColor columns
                        .Range("A" & currentRow).Value = row("ItemReference")
                        .Range("B" & currentRow).Value = row("ItemDescription")
                        .Range("C" & currentRow).Value = row("MOD_Stat")
                        .Range("D" & currentRow).Value = row("ItemColor")

                        ' Dynamically fill in the size columns (Col1, Col2, ..., ColN)
                        For i = 1 To sizeColumnCount - 1
                            Dim columnLetter As String = AlphaRange(4 + i)
                            .Range(columnLetter & currentRow).Value = row(sizeColumns(i))
                        Next
                    End If

                    currentRow += 1 ' Move to the next row in Excel
                Next

                ' Now, insert the total row at the end
                Dim totalRow As Integer = currentRow ' The row after the last item

                ' Set the totals for TotalQty
                .Range(AlphaRange(4 + sizeColumnCount) & totalRow).Value = totalQtySum
                .Range(AlphaRange(4 + sizeColumnCount) & totalRow).Interior.Color = RGB(255, 255, 0)


                .Range(AlphaRange(9 + sizeColumnCount) & totalRow).Value = totalNetSum
                .Range(AlphaRange(9 + sizeColumnCount) & totalRow).Interior.Color = RGB(255, 255, 0)
                .Range(AlphaRange(9 + sizeColumnCount) & totalRow).NumberFormat = AmountFormat

                .Range(AlphaRange(10 + sizeColumnCount) & totalRow).Value = totalNetSumLC
                .Range(AlphaRange(10 + sizeColumnCount) & totalRow).Interior.Color = RGB(255, 255, 0)
                .Range(AlphaRange(10 + sizeColumnCount) & totalRow).NumberFormat = AmountFormat

                .Range(AlphaRange(11 + sizeColumnCount) & totalRow).Value = totalNetSumCC
                .Range(AlphaRange(11 + sizeColumnCount) & totalRow).Interior.Color = RGB(255, 255, 0)
                .Range(AlphaRange(11 + sizeColumnCount) & totalRow).NumberFormat = AmountFormat

                ' Apply bold and yellow background to the total row
                With .Range(AlphaRange(4 + sizeColumnCount) & totalRow & ":" & AlphaRange(11 + sizeColumnCount) & totalRow)
                    .Font.Bold = True
                    '.Interior.Color = RGB(255, 255, 0) ' Highlight in yellow
                End With

                Dim VatLcRow As Integer = totalRow + 1 ' Row just under the total row

                With .Range(AlphaRange(11 + sizeColumnCount) & VatLcRow)
                    .Value = "VAT: " & IIf(OrderCurr <> gbCurr1, VatFC, VatFC).ToString
                    .Interior.Color = RGB(255, 255, 0)
                    .NumberFormat = AmountFormat
                    .Font.Bold = True
                    .HorizontalAlignment = Excel.XlHAlign.xlHAlignRight
                End With

            End With

            xls.Visible = True
            xlsWorkSheet.Protect("Fct$erv", False, True, True, , True, True)

        Catch ex As Exception
            ' Handle any exceptions that occur during Excel operations
            MsgBox("An error occurred: " & ex.Message)
        Finally
            ReleaseComObject(xlsWorkSheet)
            ReleaseComObject(xlsWorkBook)
            ReleaseComObject(xls)
        End Try
    End Sub

    Private Sub CreateExcelReportRO(ByRef _FileName As String, ByRef _SheetName As String,
                          ByRef _GridStartCell As Integer, dtMatrixReport As DataTable,
                          colCount As Integer, OrderNo As String, OrderCurr As String, dtInvoiceInfo As DataTable)

        Dim xlsWorkBook As Excel.Workbook
        Dim xlsWorkSheet As Excel.Worksheet
        Dim xls As New Excel.Application
        Dim xlFilePath As String
        Dim i As Integer
        Dim columnCounter As Integer
        Dim totalQtySum As Double = 0 ' To hold the sum of TotalQty
        Dim totalNetSum As Double = 0 ' To hold the sum of TotalNet
        Dim totalNetSumLC As Double = 0 ' To hold the sum of TotalNetLC
        Dim totalNetSumCC As Double = 0 ' To hold the sum of TotalNetCC
        Dim AmountFormat As String
        Try

#If DEBUG Then
            xlFilePath = gbExePath & "\..\..\PurReports\" & _FileName
#Else
        xlFilePath = gbExePath & "\PurReports\" & _FileName
#End If

            If gbDecNo = 0 Then
                AmountFormat = "#,##0"
            Else
                AmountFormat = "#,##0."
                For d As Integer = 1 To gbDecNo
                    AmountFormat &= "0"
                Next
            End If

            xlsWorkBook = xls.Workbooks.Open(xlFilePath, , True)
            xlsWorkSheet = CType(xlsWorkBook.Sheets(_SheetName), Excel.Worksheet)

            ' Dictionary to map static column names to new header names
            Dim headerMapping As New Dictionary(Of String, String) From {
            {"ItemReference", "Reference"},
            {"ItemDescription", "Item Description"},
            {"ItemColor", "Color"},
            {"TotalQty", "Total Qty"},
            {"TotalNetCostLC", "Total Net Cost " & gbCurr1},
            {"TotalNetCostCC", "Total Net Cost " & gbCurr2},
            {"DOD_STAt", "Type"}
            }

            ' Counter for dynamic size columns (Col1, Col2, etc.)
            Dim sizeCounter As Integer = 1

            With xlsWorkSheet
                .Range("CompanyName").FormulaR1C1 = gbCompName
                .Range("title").FormulaR1C1 = OrderNo
                .Range("ExportedBy").FormulaR1C1 = "Exported By " & gbUser & " On " & CDate(Now.Date)
                .Range("SupplierName").FormulaR1C1 = dtInvoiceInfo(0)("Sup_CodeName").ToString
                .Range("Receiving_Date").FormulaR1C1 = dtInvoiceInfo(0)("DOH_ActDelDate").ToString
                .Range("Location").FormulaR1C1 = dtInvoiceInfo(0)("LocationName").ToString
                .Range("RO_Ref").FormulaR1C1 = dtInvoiceInfo(0)("DOH_Ref").ToString
                .Range("Brand").FormulaR1C1 = dtInvoiceInfo(0)("Brand").ToString
                .Range("Season").FormulaR1C1 = dtInvoiceInfo(0)("Season").ToString
                .Range("Linked_To_PI").FormulaR1C1 = dtInvoiceInfo(0)("DOH_PurRecid").ToString

                ' Column counter to track position in Excel output, starting from the first column in Excel
                columnCounter = 1

                ' Skip RecordNumber and ColumnType, start from the third column in dtMatrixReport (ItemReference)
                For i = 3 To colCount + 2
                    Dim columnIndex As Integer = i - 1

                    ' Output the current column (including "Unit Cost" when reached)
                    Dim columnName As String = dtMatrixReport.Columns(columnIndex).ColumnName
                    Dim cellAddress As String = AlphaRange(columnCounter) & CStr(_GridStartCell - 1)

                    ' Use the header mapping to get the new name or assign dynamic sizes
                    Dim headerName As String

                    If headerMapping.ContainsKey(columnName) Then
                        headerName = headerMapping(columnName)
                    ElseIf columnName.StartsWith("Col") Then
                        ' Assign "Size X" dynamically to columns starting with "Col"
                        headerName = "Size " & sizeCounter
                        sizeCounter += 1
                        .Range(cellAddress).Interior.Color = RGB(192, 192, 192)
                        .Range(cellAddress).EntireColumn.ColumnWidth = 15
                    Else
                        headerName = columnName ' Default to original name if no mapping exists
                    End If

                    ' Set the header for the current column
                    With .Range(cellAddress)
                        .Font.Color = RGB(128, 0, 0) ' Set font color to (128, 0, 0) for headers
                        .FormulaR1C1 = headerName
                        .Font.Bold = True
                        With .Borders
                            .LineStyle = Excel.XlLineStyle.xlContinuous
                            .Weight = Excel.XlBorderWeight.xlThin
                            .Color = RGB(0, 0, 0) ' Black border
                        End With
                    End With

                    ' Set column widths based on header names immediately after defining the header
                    Select Case headerName
                        Case "Reference"
                            .Range(cellAddress).EntireColumn.ColumnWidth = 20 ' Width for Reference column
                            .Range(cellAddress).Interior.Color = RGB(192, 192, 192)
                        Case "Item Description"
                            .Range(cellAddress).EntireColumn.ColumnWidth = 40 ' Width for Item Description column
                            .Range(cellAddress).Interior.Color = RGB(192, 192, 192)
                        Case "Type"
                            .Range(cellAddress).EntireColumn.ColumnWidth = 15 ' Width for Type column
                            .Range(cellAddress).Interior.Color = RGB(192, 192, 192)
                        Case "Color"
                            .Range(cellAddress).EntireColumn.ColumnWidth = 15 ' Width for Color column
                            .Range(cellAddress).Interior.Color = RGB(192, 192, 192)
                        Case "Total Qty"
                            .Range(cellAddress).EntireColumn.ColumnWidth = 15 ' Width for Total Qty column
                            .Range(cellAddress).Interior.Color = RGB(192, 192, 192)
                        Case "Total Net Cost " & gbCurr1
                            .Range(cellAddress).EntireColumn.ColumnWidth = 17 ' Width for Retail Price LC column
                            .Range(cellAddress).Interior.Color = RGB(192, 192, 192)
                        Case "Total Net Cost " & gbCurr2
                            .Range(cellAddress).EntireColumn.ColumnWidth = 17 ' Width for Retail Price CC column
                            .Range(cellAddress).Interior.Color = RGB(192, 192, 192)
                    End Select

                    ' Move to next column position in Excel
                    columnCounter += 1
                Next

                ' Create a list to hold the size columns dynamically
                Dim sizeColumns As New List(Of String)

                ' Identify the dynamic size columns (those starting with "Col") that have ColumnType "H"
                For Each col As DataColumn In dtMatrixReport.Columns
                    If col.ColumnName.StartsWith("Col") Then
                        sizeColumns.Add(col.ColumnName)
                    End If
                Next

                ' Total number of dynamic size columns
                Dim sizeColumnCount As Integer = sizeColumns.Count

                ' Variable to track the current row in Excel
                Dim currentRow As Integer = _GridStartCell ' Start from the first row after the headers

                ' Loop through the DataTable to insert data into Excel and sum up totals
                For Each row As DataRow In dtMatrixReport.Rows
                    ' For header rows (ColumnType = "H")
                    If row("ColumnType").ToString() = "H" Then
                        ' Fill header columns (Reference, Item Description, Color, TotalQty, etc.)
                        .Range("A" & currentRow).Value = row("ItemReference")
                        .Range("B" & currentRow).Value = row("ItemDescription")
                        .Range("C" & currentRow).Value = row("DOD_STAt")
                        .Range("D" & currentRow).Value = row("ItemColor")

                        ' Dynamically fill in the size columns (Col1, Col2, ..., ColN)
                        For i = 1 To sizeColumnCount - 1
                            Dim columnLetter As String = AlphaRange(4 + i) ' Start filling from the 6th column (D)
                            .Range(columnLetter & currentRow).Value = row(sizeColumns(i))
                            .Range(columnLetter & currentRow).Interior.Color = RGB(192, 192, 192) ' Light gray for header size columns
                        Next

                        ' Fill the remaining fixed columns (TotalQty) only if they exist
                        .Range(AlphaRange(4 + sizeColumnCount) & currentRow).Value = row("TotalQty")

                        ' Only fill UnitCost, UnitDisc, and TotalNet if the transaction type is not "RO"

                        If dtMatrixReport.Columns.Contains("TotalNetCostLC") Then
                            .Range(AlphaRange(5 + sizeColumnCount) & currentRow).Value = row("TotalNetCostLC")
                            .Range(AlphaRange(5 + sizeColumnCount) & currentRow).NumberFormat = AmountFormat
                        End If

                        If dtMatrixReport.Columns.Contains("TotalNetCostCC") Then
                            .Range(AlphaRange(6 + sizeColumnCount) & currentRow).Value = row("TotalNetCostCC")
                            .Range(AlphaRange(6 + sizeColumnCount) & currentRow).NumberFormat = AmountFormat
                        End If

                        ' Sum up TotalQty and TotalNet values if they exist
                        totalQtySum += CDbl(row("TotalQty"))

                        ' Apply header row formatting and borders
                        With .Range("A" & currentRow & ":" & AlphaRange(6 + sizeColumnCount) & currentRow)
                            .Font.Bold = True
                            '.Interior.ColorIndex = xlNone ' No background color for header rows 
                        End With

                    ElseIf row("ColumnType").ToString() = "D" Then ' For detail rows (ColumnType = "D")
                        ' Fill only Reference, ItemDescription, and ItemColor columns
                        .Range("A" & currentRow).Value = row("ItemReference")
                        .Range("B" & currentRow).Value = row("ItemDescription")
                        .Range("C" & currentRow).Value = row("DOD_STAt")
                        .Range("D" & currentRow).Value = row("ItemColor")


                        ' Dynamically fill in the size columns (Col1, Col2, ..., ColN)
                        For i = 1 To sizeColumnCount - 1
                            Dim columnLetter As String = AlphaRange(4 + i)
                            .Range(columnLetter & currentRow).Value = row(sizeColumns(i))
                        Next
                    End If

                    currentRow += 1 ' Move to the next row in Excel
                Next

                ' Now, insert the total row at the end
                Dim totalRow As Integer = currentRow ' The row after the last item

                ' Set the totals for TotalQty
                .Range(AlphaRange(4 + sizeColumnCount) & totalRow).Value = totalQtySum
                .Range(AlphaRange(4 + sizeColumnCount) & totalRow).Interior.Color = RGB(255, 255, 0)

                ' Apply bold and yellow background to the total row
                With .Range(AlphaRange(4 + sizeColumnCount) & totalRow & ":" & AlphaRange(6 + sizeColumnCount) & totalRow)
                    .Font.Bold = True
                    '.Interior.Color = RGB(255, 255, 0) ' Highlight in yellow
                End With
            End With
            xls.Visible = True
            xlsWorkSheet.Protect("Fct$erv", False, True, True, , True, True)

        Catch ex As Exception
            ' Handle any exceptions that occur during Excel operations
            MsgBox("An error occurred: " & ex.Message)
        Finally
            ReleaseComObject(xlsWorkSheet)
            ReleaseComObject(xlsWorkBook)
            ReleaseComObject(xls)
        End Try
    End Sub

    Private Sub CreateExcelReportPI(ByRef _FileName As String, ByRef _SheetName As String,
                          ByRef _GridStartCell As Integer, dtMatrixReport As DataTable,
                          colCount As Integer, OrderNo As String, OrderCurr As String, dtInvoiceInfo As DataTable, proratedCoefValue As Double, VatLC As String, VatFC As String)

        Dim xlsWorkBook As Excel.Workbook
        Dim xlsWorkSheet As Excel.Worksheet
        Dim xls As New Excel.Application
        Dim xlFilePath As String
        Dim i As Integer
        Dim columnCounter As Integer
        Dim totalQtySum As Double = 0 ' To hold the sum of TotalQty
        Dim totalNetSum As Double = 0 ' To hold the sum of TotalNet
        Dim totalNetSumLC As Double = 0 ' To hold the sum of TotalNetLC
        Dim totalNetSumCC As Double = 0 ' To hold the sum of TotalNetCC
        Dim AmountFormat As String = ""
        Try

#If DEBUG Then
            xlFilePath = gbExePath & "\..\..\PurReports\" & _FileName
#Else
        xlFilePath = gbExePath & "\PurReports\" & _FileName
#End If

            If gbDecNo = 0 Then
                AmountFormat = "#,##0"
            Else
                AmountFormat = "#,##0."
                For d As Integer = 1 To gbDecNo
                    AmountFormat &= "0"
                Next
            End If

            xlsWorkBook = xls.Workbooks.Open(xlFilePath, , True)
            xlsWorkSheet = CType(xlsWorkBook.Sheets(_SheetName), Excel.Worksheet)

            ' Dictionary to map static column names to new header names
            Dim headerMapping As New Dictionary(Of String, String) From {
            {"ItemReference", "Reference"},
            {"ItemDescription", "Item Description"},
            {"ItemColor", "Color"},
            {"TotalQty", "Total Qty"},
            {"UnitCost", "Unit Cost " & OrderCurr},
            {"UnitCostLC", "Unit Cost " & gbCurr1},
            {"UnitCostCC", "Unit Cost " & gbCurr2},
            {"UnitDisc", "Unit Discount %"},
            {"TotalNet", "Net Total " & OrderCurr},
            {"TotalNetLC", "Net Total " & gbCurr1},
            {"TotalNetCC", "Net Total " & gbCurr2},
            {"RetailPriceLC", "Retail Price " & gbCurr1},
            {"RetailPriceCC", "Retail Price " & gbCurr2},
            {"INVD_LOCATION", "Location"},
            {"INVD_STATUS", "Type"}
            }

            ' Counter for dynamic size columns (Col1, Col2, etc.)
            Dim sizeCounter As Integer = 1

            With xlsWorkSheet
                .Range("CompanyName").FormulaR1C1 = gbCompName
                .Range("title").FormulaR1C1 = OrderNo
                .Range("ExportedBy").FormulaR1C1 = "Exported By " & gbUser & " On " & CDate(Now.Date)
                .Range("SupplierName").FormulaR1C1 = dtInvoiceInfo(0)("Sup_CodeName").ToString
                .Range("Supplier_Inv").FormulaR1C1 = dtInvoiceInfo(0)("INV_INVREF").ToString
                .Range("Date").FormulaR1C1 = dtInvoiceInfo(0)("INV_INVDATE").ToString
                .Range("Expected_Date").FormulaR1C1 = dtInvoiceInfo(0)("INV_DELIVERY").ToString
                .Range("Update_To_Stock").FormulaR1C1 = dtInvoiceInfo(0)("INV_UPDATED").ToString
                .Range("Update_Date").FormulaR1C1 = dtInvoiceInfo(0)("INV_UPDDATE").ToString
                .Range("Currency").FormulaR1C1 = dtInvoiceInfo(0)("invCurrency").ToString
                .Range("Iss._Loc").FormulaR1C1 = dtInvoiceInfo(0)("LocationName").ToString
                .Range("Pricing_Type").FormulaR1C1 = dtInvoiceInfo(0)("PriceType").ToString
                .Range("Brand").FormulaR1C1 = dtInvoiceInfo(0)("Brand").ToString
                .Range("Season").FormulaR1C1 = dtInvoiceInfo(0)("Season").ToString

                ' Column counter to track position in Excel output, starting from the first column in Excel
                columnCounter = 1

                ' Skip RecordNumber and ColumnType, start from the third column in dtMatrixReport (ItemReference)
                For i = 3 To colCount + 2
                    Dim columnIndex As Integer = i - 1

                    ' Output the current column (including "Unit Cost" when reached)
                    Dim columnName As String = dtMatrixReport.Columns(columnIndex).ColumnName
                    Dim cellAddress As String = AlphaRange(columnCounter) & CStr(_GridStartCell - 1)

                    ' Use the header mapping to get the new name or assign dynamic sizes
                    Dim headerName As String

                    If headerMapping.ContainsKey(columnName) Then
                        headerName = headerMapping(columnName)
                    ElseIf columnName.StartsWith("Col") Then
                        ' Assign "Size X" dynamically to columns starting with "Col"
                        headerName = "Size " & sizeCounter
                        sizeCounter += 1
                        .Range(cellAddress).Interior.Color = RGB(192, 192, 192)
                        .Range(cellAddress).EntireColumn.ColumnWidth = 15
                    Else
                        headerName = columnName ' Default to original name if no mapping exists
                    End If

                    ' Set the header for the current column
                    With .Range(cellAddress)
                        .Font.Color = RGB(128, 0, 0) ' Set font color to (128, 0, 0) for headers
                        .FormulaR1C1 = headerName
                        .Font.Bold = True
                        With .Borders
                            .LineStyle = Excel.XlLineStyle.xlContinuous
                            .Weight = Excel.XlBorderWeight.xlThin
                            .Color = RGB(0, 0, 0) ' Black border
                        End With
                    End With

                    ' Set column widths based on header names immediately after defining the header
                    Select Case headerName
                        Case "Reference"
                            .Range(cellAddress).EntireColumn.ColumnWidth = 25 ' Width for Reference column
                            .Range(cellAddress).Interior.Color = RGB(192, 192, 192)
                        Case "Item Description"
                            .Range(cellAddress).EntireColumn.ColumnWidth = 40 ' Width for Item Description column
                            .Range(cellAddress).Interior.Color = RGB(192, 192, 192)
                        Case "Type"
                            .Range(cellAddress).EntireColumn.ColumnWidth = 15 ' Width for Type column
                            .Range(cellAddress).Interior.Color = RGB(192, 192, 192)
                        Case "Location"
                            .Range(cellAddress).EntireColumn.ColumnWidth = 15 ' Width for Location column
                            .Range(cellAddress).Interior.Color = RGB(192, 192, 192)
                        Case "Color"
                            .Range(cellAddress).EntireColumn.ColumnWidth = 15 ' Width for Color column
                            .Range(cellAddress).Interior.Color = RGB(192, 192, 192)
                        Case "Unit Cost " & OrderCurr
                            .Range(cellAddress).EntireColumn.ColumnWidth = 15 ' Width for Unit Cost column
                            .Range(cellAddress).Interior.Color = RGB(192, 192, 192)
                        Case "Unit Cost " & gbCurr1
                            .Range(cellAddress).EntireColumn.ColumnWidth = 15 ' Width for Unit Cost LC column
                            .Range(cellAddress).Interior.Color = RGB(192, 192, 192)
                        Case "Unit Cost " & gbCurr2
                            .Range(cellAddress).EntireColumn.ColumnWidth = 15 ' Width for Unit Cost CC column
                            .Range(cellAddress).Interior.Color = RGB(192, 192, 192)
                        Case "Unit Discount %"
                            .Range(cellAddress).EntireColumn.ColumnWidth = 15 ' Width for Unit Discount column
                            .Range(cellAddress).Interior.Color = RGB(192, 192, 192)
                        Case "Net Total " & OrderCurr
                            .Range(cellAddress).EntireColumn.ColumnWidth = 15 ' Width for Net Total column
                            .Range(cellAddress).Interior.Color = RGB(192, 192, 192)
                        Case "Net Total " & gbCurr1
                            .Range(cellAddress).EntireColumn.ColumnWidth = 15 ' Width for Net Total LC column
                            .Range(cellAddress).Interior.Color = RGB(192, 192, 192)
                        Case "Net Total " & gbCurr2
                            .Range(cellAddress).EntireColumn.ColumnWidth = 15 ' Width for Net Total CC column
                            .Range(cellAddress).Interior.Color = RGB(192, 192, 192)
                        Case "Total Qty"
                            .Range(cellAddress).EntireColumn.ColumnWidth = 15 ' Width for Total Qty column
                            .Range(cellAddress).Interior.Color = RGB(192, 192, 192)
                        Case "Retail Price " & gbCurr1
                            .Range(cellAddress).EntireColumn.ColumnWidth = 15 ' Width for Retail Price LC column
                            .Range(cellAddress).Interior.Color = RGB(192, 192, 192)
                        Case "Retail Price " & gbCurr2
                            .Range(cellAddress).EntireColumn.ColumnWidth = 15 ' Width for Retail Price CC column
                            .Range(cellAddress).Interior.Color = RGB(192, 192, 192)
                    End Select

                    ' Move to next column position in Excel
                    columnCounter += 1
                Next

                ' Create a list to hold the size columns dynamically
                Dim sizeColumns As New List(Of String)

                ' Identify the dynamic size columns (those starting with "Col") that have ColumnType "H"
                For Each col As DataColumn In dtMatrixReport.Columns
                    If col.ColumnName.StartsWith("Col") Then
                        sizeColumns.Add(col.ColumnName)
                    End If
                Next

                ' Total number of dynamic size columns
                Dim sizeColumnCount As Integer = sizeColumns.Count

                ' Variable to track the current row in Excel
                Dim currentRow As Integer = _GridStartCell ' Start from the first row after the headers

                ' Loop through the DataTable to insert data into Excel and sum up totals
                For Each row As DataRow In dtMatrixReport.Rows
                    ' For header rows (ColumnType = "H")
                    If row("ColumnType").ToString() = "H" Then
                        ' Fill header columns (Reference, Item Description, Color, TotalQty, etc.)
                        .Range("A" & currentRow).Value = row("ItemReference")
                        .Range("B" & currentRow).Value = row("ItemDescription")
                        .Range("C" & currentRow).Value = row("Invd_status")
                        .Range("D" & currentRow).Value = row("Invd_location")
                        .Range("E" & currentRow).Value = row("ItemColor")

                        ' Dynamically fill in the size columns (Col1, Col2, ..., ColN)
                        For i = 1 To sizeColumnCount - 1
                            Dim columnLetter As String = AlphaRange(5 + i) ' Start filling from the 6th column (D)
                            .Range(columnLetter & currentRow).Value = row(sizeColumns(i))
                            .Range(columnLetter & currentRow).Interior.Color = RGB(192, 192, 192) ' Light gray for header size columns
                        Next

                        ' Fill the remaining fixed columns (TotalQty) only if they exist
                        .Range(AlphaRange(5 + sizeColumnCount) & currentRow).Value = row("TotalQty")

                        ' Only fill UnitCost, UnitDisc, and TotalNet if the transaction type is not "RO"

                        If dtMatrixReport.Columns.Contains("UnitCost") Then
                            .Range(AlphaRange(6 + sizeColumnCount) & currentRow).Value = row("UnitCost")
                            .Range(AlphaRange(6 + sizeColumnCount) & currentRow).NumberFormat = AmountFormat
                        End If

                        If dtMatrixReport.Columns.Contains("UnitCostLC") Then
                            .Range(AlphaRange(7 + sizeColumnCount) & currentRow).Value = row("UnitCostLC")
                            .Range(AlphaRange(7 + sizeColumnCount) & currentRow).NumberFormat = AmountFormat
                        End If

                        If dtMatrixReport.Columns.Contains("UnitCostCC") Then
                            .Range(AlphaRange(8 + sizeColumnCount) & currentRow).Value = row("UnitCostCC")
                            .Range(AlphaRange(8 + sizeColumnCount) & currentRow).NumberFormat = AmountFormat
                        End If

                        If dtMatrixReport.Columns.Contains("RetailPriceLC") Then
                            .Range(AlphaRange(9 + sizeColumnCount) & currentRow).Value = row("RetailPriceLC")
                            .Range(AlphaRange(9 + sizeColumnCount) & currentRow).NumberFormat = AmountFormat
                        End If

                        If dtMatrixReport.Columns.Contains("RetailPriceCC") Then
                            .Range(AlphaRange(10 + sizeColumnCount) & currentRow).Value = row("RetailPriceCC")
                            .Range(AlphaRange(10 + sizeColumnCount) & currentRow).NumberFormat = AmountFormat
                        End If

                        If dtMatrixReport.Columns.Contains("UnitDisc") Then
                            .Range(AlphaRange(11 + sizeColumnCount) & currentRow).Value = row("UnitDisc")
                            .Range(AlphaRange(11 + sizeColumnCount) & currentRow).NumberFormat = AmountFormat
                        End If

                        If dtMatrixReport.Columns.Contains("TotalNet") Then
                            .Range(AlphaRange(12 + sizeColumnCount) & currentRow).Value = row("TotalNet")
                            .Range(AlphaRange(12 + sizeColumnCount) & currentRow).NumberFormat = AmountFormat
                        End If

                        If dtMatrixReport.Columns.Contains("TotalNetLC") Then
                            .Range(AlphaRange(13 + sizeColumnCount) & currentRow).Value = row("TotalNetLC")
                            .Range(AlphaRange(13 + sizeColumnCount) & currentRow).NumberFormat = AmountFormat
                        End If

                        If dtMatrixReport.Columns.Contains("TotalNetCC") Then
                            .Range(AlphaRange(14 + sizeColumnCount) & currentRow).Value = row("TotalNetCC")
                            .Range(AlphaRange(14 + sizeColumnCount) & currentRow).NumberFormat = AmountFormat
                        End If

                        ' Sum up TotalQty and TotalNet values if they exist
                        totalQtySum += CDbl(row("TotalQty"))
                        totalNetSum += If(dtMatrixReport.Columns.Contains("TotalNet"), CDbl(row("TotalNet")), 0) ' Check before summing
                        totalNetSumLC += If(dtMatrixReport.Columns.Contains("TotalNetLC"), CDbl(row("TotalNetLC")), 0) ' Check before summing
                        totalNetSumCC += If(dtMatrixReport.Columns.Contains("TotalNetCC"), CDbl(row("TotalNetCC")), 0) ' Check before summing


                        ' Apply header row formatting and borders
                        With .Range("A" & currentRow & ":" & AlphaRange(10 + sizeColumnCount) & currentRow)
                            .Font.Bold = True
                            '.Interior.ColorIndex = xlNone ' No background color for header rows  
                        End With

                    ElseIf row("ColumnType").ToString() = "D" Then ' For detail rows (ColumnType = "D")
                        ' Fill only Reference, ItemDescription, and ItemColor columns
                        .Range("A" & currentRow).Value = row("ItemReference")
                        .Range("B" & currentRow).Value = row("ItemDescription")
                        .Range("C" & currentRow).Value = row("Invd_status")
                        .Range("D" & currentRow).Value = row("Invd_location")
                        .Range("E" & currentRow).Value = row("ItemColor")


                        ' Dynamically fill in the size columns (Col1, Col2, ..., ColN)
                        For i = 1 To sizeColumnCount - 1
                            Dim columnLetter As String = AlphaRange(5 + i)
                            .Range(columnLetter & currentRow).Value = row(sizeColumns(i))
                        Next
                    End If

                    currentRow += 1 ' Move to the next row in Excel
                Next

                ' Now, insert the total row at the end
                Dim totalRow As Integer = currentRow ' The row after the last item

                ' Set the totals for TotalQty
                .Range(AlphaRange(5 + sizeColumnCount) & totalRow).Value = totalQtySum
                .Range(AlphaRange(5 + sizeColumnCount) & totalRow).Interior.Color = RGB(255, 255, 0)

                Dim proratedCoefRow As Integer = totalRow + 1 ' Row just under the total row

                ' Label in the "Retail Price CC" column (or adjust based on your layout)
                .Range(AlphaRange(9 + sizeColumnCount) & proratedCoefRow).Value = "Prorated Coef.:"
                .Range(AlphaRange(9 + sizeColumnCount) & proratedCoefRow).Font.Bold = True

                ' Value of the prorated coefficient
                .Range(AlphaRange(10 + sizeColumnCount) & proratedCoefRow).Value = proratedCoefValue
                .Range(AlphaRange(10 + sizeColumnCount) & proratedCoefRow).Interior.Color = RGB(255, 255, 0)
                .Range(AlphaRange(10 + sizeColumnCount) & proratedCoefRow).NumberFormat = AmountFormat


                .Range(AlphaRange(12 + sizeColumnCount) & totalRow).Value = totalNetSum
                .Range(AlphaRange(12 + sizeColumnCount) & totalRow).Interior.Color = RGB(255, 255, 0)
                .Range(AlphaRange(12 + sizeColumnCount) & totalRow).NumberFormat = AmountFormat

                .Range(AlphaRange(13 + sizeColumnCount) & totalRow).Value = totalNetSumLC
                .Range(AlphaRange(13 + sizeColumnCount) & totalRow).Interior.Color = RGB(255, 255, 0)
                .Range(AlphaRange(13 + sizeColumnCount) & totalRow).NumberFormat = AmountFormat

                .Range(AlphaRange(14 + sizeColumnCount) & totalRow).Value = totalNetSumCC
                .Range(AlphaRange(14 + sizeColumnCount) & totalRow).Interior.Color = RGB(255, 255, 0)
                .Range(AlphaRange(14 + sizeColumnCount) & totalRow).NumberFormat = AmountFormat

                ' Apply bold and yellow background to the total row
                With .Range(AlphaRange(5 + sizeColumnCount) & totalRow & ":" & AlphaRange(14 + sizeColumnCount) & totalRow)
                    .Font.Bold = True
                    '.Interior.Color = RGB(255, 255, 0) ' Highlight in yellow
                End With


                Dim VatLcRow As Integer = totalRow + 1 ' Row just under the total row

                With .Range(AlphaRange(14 + sizeColumnCount) & VatLcRow)
                    .Value = "VAT: " & IIf(OrderCurr <> gbCurr1, VatFC, VatFC).ToString
                    .Interior.Color = RGB(255, 255, 0)
                    .NumberFormat = AmountFormat
                    .Font.Bold = True
                    .HorizontalAlignment = Excel.XlHAlign.xlHAlignRight
                End With

            End With

            xls.Visible = True
            xlsWorkSheet.Protect("Fct$erv", False, True, True, , True, True)

        Catch ex As Exception
            ' Handle any exceptions that occur during Excel operations
            MsgBox("An error occurred: " & ex.Message)
        Finally
            ReleaseComObject(xlsWorkSheet)
            ReleaseComObject(xlsWorkBook)
            ReleaseComObject(xls)
        End Try
    End Sub


#End Region


End Module
