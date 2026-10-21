
'Company: App
'Module: ERP Module
'Creation Date: july 01 2013 
'Created by: Rachel Assaf
'Last Update Date: 
'Modified by: 
'------------------------------------------

Imports C1.Win
Imports C1.Win.C1Input
Imports System.Data.SqlClient
Imports C1.Win.C1TrueDBGrid
Imports Purchase.Classes.LockRecord
Imports UserControls.ERPHelp
Imports CrystalDecisions.CrystalReports.Engine
Imports System.Threading
Imports DevComponents.DotNetBar
Imports DLL_StockSearch

Public Class FrmDO
    Dim MaxRecID As Long = 0
    Dim row As DataRow
    Dim save As Boolean = False
    Private OldVal As String
    Dim FirstColKey As Integer
    Dim LastColKey As Integer
    Dim BInsBarcodeCancel As Boolean
    Private CreateItm As Boolean
    Dim OldCode As String
    Dim TempNonAddedItemsStk As String
    Dim dsNonAddedItemsStk As DataSet
    Private printingId As String
    Private RecivingHeader As String
    Private MaxCnt As Integer
    Dim Exists As Boolean = True

    Public UserCode As String
    Dim strsql As String
    Dim Wtype As String
    Dim BarcodeOld As String = ""
    Dim bExists, Old, refreshControl As Boolean
    Dim GridSizeColumn As Integer
    Private cntTitle As String = "Receiving Order - Fashion & Consignment"
    Private lcSteps As String
    Dim lcChangeKey, lcChanges, ColorExpiryError, wModify, lcRef As Boolean
    Private current_S, normal_S As C1.Win.C1TrueDBGrid.Style
    Private lcPrivAdd, lcPrivModify, lcPrivDelete, lcPrivView, lcPrivPrint As Boolean
    Public DelOrder As String

    Dim TmpGridDetails, TmpGridExpiry, TmpGridBarcode, TmpFashQty, TempPendSearch, TempPendingDO As String
    Private TempNonRevItems As String
    'Dim TmpGridBarcodeFinal As String 'added by GS on 09/06/16
    Private lcItemClass As String
    Dim daDetDetails, daDetExpiry, daDetBarcode, daDetBarcodeF, daDetFqty As SqlDataAdapter
    Dim dsDetDetails, dsDetExpiry, dsDetBarcode, dsDetBarcodeF, dsDetFqty As DataSet
    Dim BarcodeRed As Boolean = False
    Dim FBarcodeRed As Boolean = False
    Dim ExpiryRed As Boolean = False
    Private DtQualityControlHeader As DataTable
    Private PrintQC As Thread
    Private KeyStyle, keyGender, BrandKey, KeyGrid As Boolean
    Dim OldValue As String
    Private TempQcHeader, TempQcDetails As String
    Private PODATE As String ' TEST TO FORBIT THE RO DATE TO BE  LESS THEN IMPORTED PO DATE CH @ 09/03/2017 
    Private ReferenceCollectionItmRef As New AutoCompleteStringCollection
    Private Delegate Sub fillremdictomrydelg()
    Private Str_BasePriceType1, Str_BasePriceType2 As String
    Private supplierneedapproval As Boolean = False
    Private PreventCreatItemWOBar As Boolean
    Private ObjSumNormal, ObjSumReturn, objSum, ObjSumFree As Object

    Public Overloads Sub show(ByRef _Steps As String)
        Try
            lcSteps = _Steps
            Me.Show()
            Application.DoEvents()
        Catch exp As Exception
            Throw exp
        End Try
    End Sub

    Private Sub FrmDO_FormClosing(ByVal sender As Object, ByVal e As System.Windows.Forms.FormClosingEventArgs) Handles Me.FormClosing
        Try
            If Wtype <> "" And lcChanges = True Then
                If MessageBox.Show("Do You Want To Discard The Current Record?", cntTitle, _
                    MessageBoxButtons.YesNo, MessageBoxIcon.Question) = MsgBoxResult.No Then
                    e.Cancel = True
                    Exit Sub
                End If
            End If
            If txtCode.Text <> "" And Wtype <> "" Then
                AllLocks(AllLocksType.ReleaseLock, "stk04_PurDelOrdH", txtCode.Text, cntTitle, Me.Name)
            End If
            If txtOrderNo.Text.Trim <> "" Then
                AllLocks(AllLocksType.ReleaseLock, "STK04_mordH", txtOrderNo.Text.Trim, cntTitle, Me.Name)
            End If

            strsql = "delete from sysLockRecords where lr_Table = 'STK04_StkFilPending' and lr_Machine='" & gbPCName & "'"
            strsql &= " and lr_Application='" & gbApp & "' and lr_Menu='" & lcSteps & "'"
            Execute(strsql)


            If DelOrder = "Receiving Order" Then
                UpdateStepsLogging("MnuDO1")
                CType(Me.ParentForm, frmMain).MnuDO1.Checked = False
            ElseIf DelOrder = "Receiving Order Fashion" Then
                UpdateStepsLogging("MnuDOF1")
                CType(Me.ParentForm, frmMain).MnuDOF1.Checked = False
            ElseIf DelOrder = "Consignment Receiving Order" Then
                UpdateStepsLogging("MnuCDO1")
                CType(Me.ParentForm, frmMain).MnuCDO1.Checked = False
            ElseIf DelOrder = "Consignment Receiving Order Fashion" Then
                UpdateStepsLogging("MnuCDOF1")
                CType(Me.ParentForm, frmMain).MnuCDOF1.Checked = False
            End If
            DropSqlObject(TmpGridBarcode, "TABLE")
            DropSqlObject(TmpGridBarcode & "F", "TABLE")
            DropSqlObject(TmpGridExpiry, "TABLE")
            DropSqlObject(TmpGridDetails, "TABLE")
            DropSqlObject("Tmp_" & TmpGridDetails, "TABLE")
            DropSqlObject(TempPendingDO, "TABLE")
            DropSqlObject(TempNonAddedItemsStk, "TABLE")
            DropSqlObject(TmpFashQty, "TABLE") '...Jad-J...email:Some temporary table not dropped 23/11/2016
            DropSqlObject(TempQcHeader, "TABLE")
            DropSqlObject(TempNonRevItems, "TABLE")
            DropSqlObject(TempQcDetails, "TABLE")
            DropSqlObject("temp_ItemValidation_" & lcSteps & "_" & gbUser, "TABLE")
        Catch exp As Exception
            MessageBox.Show("Error : " & exp.Message, cntTitle, _
            MessageBoxButtons.OK, MessageBoxIcon.Error)
        End Try
    End Sub

    Private Sub FrmDO_KeyDown(ByVal sender As Object, ByVal e As System.Windows.Forms.KeyEventArgs) Handles Me.KeyDown
        Try
            If Print.Visible = True Then
                If e.KeyCode = Keys.Enter Then
                    If BtnOk.Focus Then BtnOk_Click(BtnOk, New System.EventArgs)
                End If
            End If
            If e.Control = True AndAlso e.KeyCode = Keys.P Then
                If btnPending.Focus Then btnPending_Click(btnPending, New System.EventArgs)
                Exit Sub
            ElseIf e.Control = True AndAlso e.KeyCode = Keys.F8 Then
                If btnStkSearch.Focus Then btnStkSearch_Click(btnStkSearch, New System.EventArgs)
                Exit Sub
            ElseIf e.Control = True AndAlso e.KeyCode = Keys.M And BarcodeF.Visible = False Then
                If btnCreate.Focus Then btnCreate_Click(btnCreate, New System.EventArgs)
                Exit Sub
            End If

            Select Case e.KeyCode
                Case Keys.F3
                    If e.Control = True Then
                        If GrpExpBarcode.Visible = True And BtnGenerateBarcodeTrading.Focus Then
                            BtnGenerateBarcodeTrading_Click(BtnGenerateBarcodeTrading, New System.EventArgs)
                        ElseIf BarcodeF.Visible = True And BtnGenerateBarcodeFashion.Focus Then
                            BtnGenerateBarcodeFashion_Click(BtnGenerateBarcodeFashion, New System.EventArgs)
                        End If
                    Else
                        If btnAdd.Focus Then btnAdd_Click(btnAdd, New System.EventArgs)
                    End If
                Case Keys.F4 : If btnModify.Focus Then btnModify_Click(btnModify, New System.EventArgs)
                Case Keys.F5 : If btnDelete.Focus Then btnDelete_Click(btnDelete, New System.EventArgs)
                Case Keys.F6 : If btnCancel.Focus Then btnCancel_Click(btnCancel, New System.EventArgs)
                Case Keys.F7
                    If e.Control = True Then
                        If BarcodeF.Visible = True And lstDelivery.Columns("new_old").Text <> "O" Then btnClear_Click(btnClear, New System.EventArgs)
                    Else
                        If btnUndo.Focus Then btnUndo_Click(btnUndo, New System.EventArgs)
                    End If
                Case Keys.F8 : If btnSearch.Focus Then btnSearch_Click(btnSearch, New System.EventArgs)
                Case Keys.F9
                    If e.Control = True Then
                        If BarcodeF.Visible = True Then btnOK1_Click(btnOK1, New System.EventArgs)
                    Else
                        If GrpExpBarcode.Visible = True Then
                            BtnOKExpiryBarcode_Click(BtnOKExpiryBarcode, New System.EventArgs)
                            Exit Sub
                        Else
                            If btnSave.Focus Then btnSave_Click(btnSave, New System.EventArgs)
                        End If
                    End If
                Case Keys.F11 : If btnPreview.Focus Then btnPreview_Click(btnPreview, New System.EventArgs)
                Case Keys.F12
                    If btnClose.Enabled = True Then
                        Me.Close() : e.Handled = True
                    End If
                Case Keys.Escape : If Print.Visible = True Then Print.Visible = False
            End Select

        Catch ex As Exception
            MessageBox.Show("General Error : " & ex.Message, Me.Text, MessageBoxButtons.OK, MessageBoxIcon.Warning)
        End Try
    End Sub

    Private Sub FrmDO_Load(ByVal sender As Object, ByVal e As System.EventArgs) Handles Me.Load
        Try

            PrepareHelp(Me, ConnectionString, gbApp, lcSteps)
            BtnImportFromXls.Visible = True
            KeyGrid = False
            'cntTitle = "" & DelOrder
            wModify = False
            With stBar
                .Panels(0).Text = Date.Today.ToShortDateString
                .Panels(1).Text = ""
                .Panels(2).Text = "Login User: " & gbUser
                .Panels(3).Text = Now.ToLongTimeString
            End With
            lstDelivery.AllowAddNew = False
            TmpGridDetails = "TempGridDetail_" & lcSteps & "_" & gbUser
            TempNonRevItems = "TempNonRevItems_" & lcSteps & "_" & gbUser
            TempPendSearch = "TempDOPending_" & lcSteps & "_" & gbUser
            TempPendingDO = "TempGridDetailPending_" & lcSteps & "_" & gbUser
            TempNonAddedItemsStk = "TempNonAddedItemsFashion_" & lcSteps & "_" & gbUser
            'TempQcHeader, TempQcDetails
            TempQcHeader = "TempPurDelOrdDQCH_" & lcSteps & "_" & gbUser
            TempQcDetails = "TempPurDelOrdDQCD_" & lcSteps & "_" & gbUser
            If DelOrder = "Receiving Order" Then
                TempNonRevItems &= "D"
                TmpGridDetails &= "D"
                TempPendSearch &= "D"
                TempPendingDO &= "D"
                TempQcHeader &= "D"
                TempQcDetails &= "D"
                FillDictionaryByItemRef()
            ElseIf DelOrder = "Receiving Order Fashion" Then
                TempNonRevItems &= "F"
                TmpGridDetails &= "F"
                TempPendSearch &= "F"
                TempPendingDO &= "F"
                TempQcHeader &= "F"
                TempQcDetails &= "F"
                FillDictionaryByItemRef()
                BtnImportFromXls.Visible = False
            ElseIf DelOrder = "Consignment Receiving Order" Then
                TempNonRevItems &= "C"
                TmpGridDetails &= "C"
                TempPendSearch &= "C"
                TempPendingDO &= "C"
                TempQcHeader &= "C"
                TempQcDetails &= "C"
                FillDictionaryByItemRef()
            ElseIf DelOrder = "Consignment Receiving Order Fashion" Then
                TempNonRevItems &= "CF"
                TmpGridDetails &= "CF"
                TempPendSearch &= "CF"
                TempPendingDO &= "CF"
                TempQcHeader &= "CF"
                TempQcDetails &= "CF"
                FillDictionaryByItemRef()
                BtnImportFromXls.Visible = False
            End If


            TmpGridExpiry = "TempGridExpiry_" & lcSteps & "_" & gbUser
            If DelOrder = "Receiving Order" Then
                TmpGridExpiry &= "D"
            ElseIf DelOrder = "Receiving Order Fashion" Then
                TmpGridExpiry &= "F"
            ElseIf DelOrder = "Consignment Receiving Order" Then
                TmpGridExpiry &= "C"
            ElseIf DelOrder = "Consignment Receiving Order Fashion" Then
                TmpGridExpiry &= "CF"
            End If

            TmpGridBarcode = "TempGridBarcode_" & lcSteps & "_" & gbUser
            ''TmpGridBarcodeFinal = "TmpGridBarcodeFinal_" & lcSteps & "_" & gbUser 'added by GS on 09/06/16
            TmpFashQty = "TempFashQTY_" & lcSteps & "_" & gbUser
            If DelOrder = "Receiving Order" Then
                TmpGridBarcode &= "D"
            ElseIf DelOrder = "Receiving Order Fashion" Then
                TmpFashQty &= "F"
                TmpGridBarcode &= "F"
            ElseIf DelOrder = "Consignment Receiving Order" Then
                TmpGridBarcode &= "C"
            ElseIf DelOrder = "Consignment Receiving Order Fashion" Then
                TmpFashQty &= "CF"
                TmpGridBarcode &= "CF"
            End If
            If Me.Width > 1279 Then
                TXTUNIT.Width = 70
                txtSUBUnit.Width = 70
            End If
            FashOrTrading()
            If DelOrder = "Receiving Order Fashion" Or DelOrder = "Consignment Receiving Order Fashion" Then
                dsDetBarcodeF = New DataSet
                dsDetFqty = New DataSet
                PnlHead2.Visible = True
                btnBarcode.Visible = True
                KeyStyle = False
                keyGender = False
                For i = 0 To CurrItmKey.Length - 1
                    If UCase(CurrItmKey(i)) = "STYLE" Or UCase(CurrItmKey(i)) = "STK_STYLE" Then KeyStyle = True
                    If UCase(CurrItmKey(i)) = "STK_GENDER" Then keyGender = True
                    If UCase(CurrItmKey(i)) = "STK_BRAND" Then BrandKey = True
                    If UCase(CurrItmKey(i)) = "STK_GRID" Then KeyGrid = True : Exit For
                Next

                If DelOrder = "Receiving Order" Then
                    strsql = "Exec P_GetSizeGrid 'DeliveryOrderD','" & gbUser & "'"
                ElseIf DelOrder = "Receiving Order Fashion" Then
                    strsql = "Exec P_GetSizeGrid 'DeliveryOrderF','" & gbUser & "'"
                ElseIf DelOrder = "Consignment Receiving Order" Then
                    strsql = "Exec P_GetSizeGrid 'DeliveryOrderC','" & gbUser & "'"
                ElseIf DelOrder = "Consignment Receiving Order Fashion" Then
                    strsql = "Exec P_GetSizeGrid 'DeliveryOrderCF','" & gbUser & "'"
                End If

                Execute(strsql)
                strsql = "SELECT ISNULL(MAX(GD_Order), 0) FROM STK04_DetGRID"
                Dim drSql As SqlDataReader
                ReadFromDBToDataReader(strsql, drSql)
                drSql.Read()
                MaxCnt = CInt(drSql.Item(0))

                CreatePOGridQTYF(TmpFashQty, MaxCnt)
                RefreshLstGrid(lstFashQty, True, TmpFashQty, daDetFqty, dsDetFqty)
                Flowhead.Visible = False
                'PnlHead3.Visible = False

            End If
            If DelOrder = "Consignment Receiving Order" Or DelOrder = "Consignment Receiving Order Fashion" Then
                'PnlHead3.Visible = False
                Flowhead.Visible = False
                lblOrderNo.Visible = False
                txtOrderNo.Visible = False
                btnPurchOrder.Visible = False
                btnPurchOrder.Tag = "Negation"
                PnlCurr.Visible = True
            Else
                PnlCurr.Visible = False
            End If

            CreatePoGridDetails(TmpGridDetails, TmpGridExpiry, TmpGridBarcode, DelOrder)
            RefreshLstGrid(lstDelivery, True, TmpGridDetails, daDetDetails, dsDetDetails)
            If DelOrder = "Receiving Order" Or DelOrder = "Consignment Receiving Order" Then
                RefreshLstGrid(LstExpiry, True, TmpGridExpiry, daDetExpiry, dsDetExpiry)
                RefreshLstGrid(LstBarcode, True, TmpGridBarcode, daDetBarcode, dsDetBarcode)
            End If

            CreatePoPendingTemp(TempPendingDO)
            If DelOrder = "Receiving Order Fashion" Or DelOrder = "Consignment Receiving Order Fashion" Then CreateLstBarcode()
            CreateContextMenu(Me)

            Me.Text = DelOrder

            txtMode.Text = "..."
            txtMode.ForeColor = Color.Gray
            txtUstmp.Text = "..."
            txtUstmp.ForeColor = Color.Gray
            txtDstmp.Text = "..."
            txtDstmp.ForeColor = Color.Gray

            current_S = New C1.Win.C1TrueDBGrid.Style
            normal_S = New C1.Win.C1TrueDBGrid.Style

            With current_S
                .BackColor = Color.FromArgb(255, 224, 192)
                .ForeColor = Color.Maroon
            End With

            With normal_S
                .BackColor = Color.White
                .ForeColor = Color.Navy
            End With

            'Get Privilige
            If gbUser = "SU" Then
                lcPrivAdd = True
                lcPrivModify = True
                lcPrivDelete = True
                lcPrivView = True
                lcPrivPrint = True
                PreventCreatItemWOBar = False '...CH...WF:7955
            Else
                If DelOrder = "Receiving Order" Or DelOrder = "Consignment Receiving Order" Then
                    PnlHead2.Visible = False
                End If
                TestPriv(Me, lcSteps)
                lcPrivAdd = gbPrivAdd
                lcPrivModify = gbPrivModify
                lcPrivDelete = gbPrivDelete
                lcPrivView = gbPrivView
                lcPrivPrint = gbPrivPrint
                If DelOrder = "Receiving Order" Or DelOrder = "Consignment Receiving Order" Then
                    PreventCreatItemWOBar = NegationPriv(lcSteps, "btnPrevCreaItemWOBar") '...CH...WF:7955
                Else
                    PreventCreatItemWOBar = False
                End If
            End If
            btnAdd.Enabled = lcPrivAdd
            btnModify.Enabled = lcPrivModify
            btnDelete.Enabled = lcPrivDelete
            btnPreview.Enabled = lcPrivPrint
            btnSearch.Enabled = lcPrivView
            btnStkSearch.Enabled = False
            btnPending.Enabled = False
            btnCreate.Enabled = False
            If DelOrder = "Receiving Order" Then
                Me.Text = "Receiving Order"
                cntTitle = "Receiving Order"
            ElseIf DelOrder = "Receiving Order Fashion" Then
                Me.Text = "Receiving Order With Size And Color Matrix"
                cntTitle = "Receiving Order With Size And Color Matrix"
            ElseIf DelOrder = "Consignment Receiving Order" Then
                Me.Text = "Consignment Receiving Order"
                cntTitle = "Consignment Receiving Order"
            ElseIf DelOrder = "Consignment Receiving Order Fashion" Then
                Me.Text = "Consignment Receiving Order With Size And Color Matrix"
                cntTitle = "Consignment Receiving Order With Size And Color Matrix"
            End If

            If DelOrder = "Consignment Receiving Order" Or DelOrder = "Consignment Receiving Order Fashion" Then
                ' PnlConsigment.Visible = True
            End If
            LstVisbility(False, False, lstDelivery.Columns("ItmClass").Text.Trim)
            btnCancel.Enabled = False
            btnSave.Enabled = False
            EnabledControls(Me, False)
            ReadOnlyCtr(Me, True, True)
            btnUndo.Enabled = False
            VisibleBtn(Me, False)
            ' PnlConsigment.Visible = False
            lstDelivery.AllowUpdate = False
            LstBarcode.AllowUpdate = False
            LstBarcodeF.AllowUpdate = False
            LstExpiry.AllowUpdate = False
            btnQA.Enabled = False
            Me.Refresh()
            Dim spaces As Integer
            spaces = CInt((Me.Width - ((btnAdd.Left * 2) + (btnAdd.Width * 12))) / 11)
            btnModify.Left = btnAdd.Left + btnAdd.Width + spaces
            btnDelete.Left = btnModify.Left + btnModify.Width + spaces
            btnCancel.Left = btnDelete.Left + btnDelete.Width + spaces
            btnUndo.Left = btnCancel.Left + btnCancel.Width + spaces
            btnPending.Left = btnUndo.Left + btnUndo.Width + spaces
            btnStkSearch.Left = btnPending.Left + btnPending.Width + spaces
            btnSearch.Left = btnStkSearch.Left + btnStkSearch.Width + spaces
            btnCreate.Left = btnSearch.Left + btnSearch.Width + spaces
            btnSave.Left = btnCreate.Left + btnCreate.Width + spaces
            btnPreview.Left = btnSave.Left + btnSave.Width + spaces
            DefaultPriceTypes()
            If btnAdd.Visible And btnAdd.Enabled Then btnAdd.Focus()

            btnFashionRep.Visible = DelOrder.Contains("Fashion")
            btnFashionRep.Enabled = DelOrder.Contains("Fashion")

        Catch ex As Exception
            MessageBox.Show("Error : " & ex.Message, "", MessageBoxButtons.OK, MessageBoxIcon.Error)
        End Try
    End Sub

    Private Sub LstVisbility(ByVal ExpiryVisible As Boolean, ByVal BarcodeVisible As Boolean, ByVal ItemClass As String, Optional FromQC As Boolean = False)
        Try

            If GrpExpBarcode.Visible = False Then

                With GrpExpBarcode
                    .Visible = False
                End With
                Exit Sub
            Else
                With GrpExpBarcode
                    .Top = lstDelivery.Top + PnlGrid.Top + lstDelivery.RowTop(lstDelivery.Row) + lstDelivery.RowHeight
                    .Left = 21
                    ' .Width = 356
                    Dim j As Integer
                    If UCase(ItemClass) = "B" Or UCase(ItemClass) = "T" Then
                        j = 220
                    Else
                        '   j = 425
                        j = 680
                    End If
                    .Height = 233
                    'ch @ 06/10/2016
                    If GrpExpBarcode.Visible Then
                        If GrpExpBarcode.Height + GrpExpBarcode.Top > Me.Height Then
                            GrpExpBarcode.Top = GrpExpBarcode.Top - GrpExpBarcode.Height - lstDelivery.RowHeight
                        End If
                    End If
                    .Visible = True
                    .BringToFront()

                    .Width = j + 3
                    .Refresh()
                    Me.Update()



                End With
            End If

            If ExpiryVisible = False And BarcodeVisible = False Then
                GrpExpBarcode.Visible = False
                lstDelivery.Width = 984
                LstExpiry.Visible = False : lstDelivery.Splits(0).DisplayColumns("MOD_OrdQty").Locked = True
                LstBarcode.Visible = False
            ElseIf ExpiryVisible = False And BarcodeVisible = True Then
                GetBarcode(ItemClass)
                LstExpiry.Visible = False : lstDelivery.Splits(0).DisplayColumns("MOD_OrdQty").Locked = True
                lstDelivery.Width = 859
                LstBarcode.Visible = True
            ElseIf ExpiryVisible = True And BarcodeVisible = False Then
                GetExpiry(ItemClass)
                GetBarcode(ItemClass)
                lstDelivery.Width = 858
                LstExpiry.Visible = True : lstDelivery.Splits(0).DisplayColumns("MOD_OrdQty").Locked = True
                '  LstExpiry.Width += LstBarcode.Width
                LstBarcode.Visible = False
                GrpExpBarcode.Width = LstExpiry.Width + 10
            Else
                GetExpiry(ItemClass)
                GetBarcode(ItemClass)
                lstDelivery.Width = 634
                LstBarcode.Visible = True
                LstExpiry.Visible = True : lstDelivery.Splits(0).DisplayColumns("MOD_OrdQty").Locked = True

            End If


            'If LstExpiry.Visible = True Then
            '    If UCase(lstDelivery.Columns("new_old").Text.Trim) = "O" Or UCase(lstDelivery.Columns("new_old").Text.Trim) = "U" Then
            '        LstExpiry.AllowAddNew = False
            '    Else
            '        LstExpiry.AllowAddNew = True
            '    End If
            'Else
            '    LstExpiry.AllowAddNew = True
            'End If
            If DelOrder = "Receiving Order Fashion" Or DelOrder = "Consignment Receiving Order Fashion" Then lstDelivery.Splits(0).DisplayColumns("MOD_OrdQty").Locked = True

            lstDelivery.Refresh()
            LstExpiry.Refresh()
            LstBarcode.Refresh()

        Catch ex As Exception
            MessageBox.Show("Error : " & ex.Message, "", MessageBoxButtons.OK, MessageBoxIcon.Error)
        End Try
    End Sub


    Private Sub btnAdd_Click(ByVal sender As System.Object, ByVal e As System.EventArgs) Handles btnAdd.Click
        Try
            Wtype = "ADD"
            EnabledButtons(Me, True, btnAdd, btnUndo, btnDelete, btnModify, btnPreview)
            btnPreview.Enabled = lcPrivPrint


            EnabledControls(Me, True)
            EnabledButtons(Me, True, btnAdd, btnUndo, btnDelete, btnModify)
            ReadOnlyCtr(Me, False, False, TXTUNIT, txtSUBUnit)
            TXTUNIT.BackColor = Color.AliceBlue : txtSUBUnit.BackColor = Color.AliceBlue
            ClearControls(Me, ErrorProvider1)
            INITCOL(True)
            RefreshControls()
            CreatePoQCtemps(TempQcHeader, TempQcDetails)
            VisibleBtn(Me, True)
            With txtMode
                .Text = "Add a new record."
                .ForeColor = Color.Red
                .Image = btnAdd.Image
            End With
            With txtUstmp
                .Text = gbUser
                .ForeColor = Color.Red
            End With
            With txtDstmp
                .Text = Now.ToLongDateString
                .ForeColor = Color.Red
            End With

            InitGrids()
            ClearControls(Me, ErrorProvider1)
            INITCOL(True)
            RefQtyLbl(True)
            txtCode.Enabled = False

            txtCode.ReadOnly = True

            txtDate.Value = CDate(Now.Date)
            txtActualDate.Value = CDate(Now.Date)

            lstDelivery.AllowUpdate = False
            lstDelivery.AllowDelete = True
            lstDelivery.AllowAddNew = False

            LstBarcode.AllowUpdate = True
            LstBarcode.AllowDelete = True
            LstBarcode.AllowAddNew = True

            LstBarcodeF.AllowUpdate = True
            LstBarcodeF.AllowDelete = True
            LstBarcodeF.AllowAddNew = True

            LstExpiry.AllowUpdate = True
            LstExpiry.AllowDelete = True
            LstExpiry.AllowAddNew = True

            VisibleBtn(Me, True)
            LstVisbility(False, False, lstDelivery.Columns("ItmClass").Text.Trim)
            RefQtyLbl(True)
            txtCode.BackColor = Color.LightSteelBlue
            Flowhead.Visible = True
            ' PnlHead3.Visible = True
            If DelOrder = "Consignment Receiving Order" Or DelOrder = "Consignment Receiving Order Fashion" Then
                ' PnlConsigment.Visible = True
                btnCurr.Enabled = True
                txtCurr.ReadOnly = False
                btnPending.Enabled = False
            Else
                'PnlConsigment.Visible = True
                'PnlConsigment.Enabled = False
                txtPrice.ReadOnly = True
                txtPrice.TabStop = False
                'LblCostCurr.Visible = True
            End If
            OldVal = ""
            OldValue = ""
            txtSupp.Focus()

            DefaultSeason()

        Catch ex As Exception
            MessageBox.Show("Error: " & ex.Message, cntTitle, MessageBoxButtons.OK, MessageBoxIcon.Error)
        End Try
    End Sub

    Private Sub btnModify_Click(ByVal sender As System.Object, ByVal e As System.EventArgs) Handles btnModify.Click

        Try
            Wtype = "MODIFY"


            btnPreview.Enabled = lcPrivPrint

            With txtMode
                .Text = "Modify an existing record."
                .ForeColor = Color.Red
                .Image = btnModify.Image
            End With
            With txtUstmp
                .Text = "..."
                .ForeColor = Color.Red
            End With
            With txtDstmp
                .Text = "..."
                .ForeColor = Color.Red
            End With

            InitGrids()
            EnabledButtons(Me, True, btnAdd, btnLoc, btnSeason, btnBrand, btnDelete, btnModify)
            ClearControls(Me, ErrorProvider1)
            INITCOL(True)
            RefQtyLbl(True)
            VisibleBtn(Me, True)
            EnabledControls(Me, True)
            ReadOnlyCtr(Me, False, False, TXTUNIT, txtSUBUnit)
            TXTUNIT.BackColor = Color.AliceBlue : txtSUBUnit.BackColor = Color.AliceBlue
            lcChangeKey = False

            RefreshControls()
            CreatePoQCtemps(TempQcHeader, TempQcDetails)
            txtCode.ReadOnly = False

            txtBrand.ReadOnly = True
            txtSeason.ReadOnly = True
            lstDelivery.AllowUpdate = False
            lstDelivery.AllowDelete = True
            lstDelivery.AllowAddNew = False

            LstBarcode.AllowUpdate = True
            LstBarcode.AllowDelete = True
            LstBarcode.AllowAddNew = True

            LstBarcodeF.AllowUpdate = True
            LstBarcodeF.AllowDelete = True
            LstBarcodeF.AllowAddNew = True

            LstExpiry.AllowUpdate = True
            LstExpiry.AllowDelete = True
            LstExpiry.AllowAddNew = True


            txtLoc.ReadOnly = True
            VisibleBtn(Me, True)
            LstVisbility(False, False, lstDelivery.Columns("ItmClass").Text.Trim)
            RefQtyLbl(True)
            Flowhead.Visible = True
            'PnlHead3.Visible = True
            txtOrderNo.Enabled = False
            btnPurchOrder.Enabled = False
            If DelOrder = "Consignment Receiving Order" Or DelOrder = "Consignment Receiving Order Fashion" Then
                ' PnlConsigment.Visible = True
                btnCurr.Enabled = False
                txtCurr.ReadOnly = True
                btnPending.Enabled = False
            Else
                ' PnlConsigment.Visible = True
                ' PnlConsigment.Enabled = False
                txtPrice.ReadOnly = True
                txtPrice.TabStop = False
                '   LblCostCurr.Visible = True
            End If
            OldVal = ""
            OldValue = ""
            txtCode.Focus()
        Catch ex As Exception
            MessageBox.Show("Error: " & ex.Message, "", MessageBoxButtons.OK, MessageBoxIcon.Error)
        End Try
    End Sub



    Private Sub lstDelivery_AfterDelete(ByVal sender As Object, ByVal e As System.EventArgs) Handles lstDelivery.AfterDelete
        Try
            'R.A. 07/05/2014''''''''''''''''''''''''''''''''
            If UCase(lstDelivery.Columns("ITMCLASS").Text.Trim) = "F" Then
                RefGridSizes(lstFashQty.Columns("GridCode").Text.Trim)
                'ElseIf UCase(lstDelivery.Columns("ITMCLASS").Text.Trim) = "E" Then
                '    LstVisbility(True, True, lstDelivery.Columns("ITMCLASS").Text.Trim)
                'ElseIf UCase(lstDelivery.Columns("ITMCLASS").Text.Trim) = "B" Or UCase(lstDelivery.Columns("ITMCLASS").Text.Trim) = "T" Then
                '    LstVisbility(False, True, lstDelivery.Columns("ITMCLASS").Text.Trim)
                'Else
                '    lstFashQty.Visible = False
                '    LstVisbility(False, False, lstDelivery.Columns("ITMCLASS").Text.Trim)
            End If

            RefQtyLbl(False)

            If dsDetDetails.Tables("tblDetails").Select("").Length() = 0 Then
                txtOrderNo.Text = ""
                btnSupp.Enabled = True : txtSupp.ReadOnly = False
                btnBrand.Enabled = True : txtBrand.ReadOnly = False
                btnSeason.Enabled = True : txtSeason.ReadOnly = False
                txtDate.ReadOnly = False
                txtCurr.ReadOnly = False : btnCurr.Enabled = True
                PnlFashQty.Visible = False
                If DelOrder = "Receiving Order Fashion" Or DelOrder = "Consignment Receiving Order Fashion" Then lstDelivery.Columns("Color").FooterText = ""
            End If
            lstDelivery.UpdateData() : daDetDetails.Update(dsDetDetails.Tables("tblDetails"))
            lstDelivery.Refresh() : UpdateDtQualityControlHeader()
            ObjSumNormal = dsDetDetails.Tables("tblDetails").Compute("Sum( MOD_OrdQty)", " MOD_stat='N' ")
            ObjSumFree = dsDetDetails.Tables("tblDetails").Compute("Sum( MOD_OrdQty)", " MOD_stat='F' ")
            ObjSumReturn = dsDetDetails.Tables("tblDetails").Compute("Sum( MOD_OrdQty)", " MOD_stat='R' ")
            objSum = CDbl(IIf(ObjSumNormal Is DBNull.Value, "0", ObjSumNormal)) + CDbl(IIf(ObjSumFree Is DBNull.Value, "0", ObjSumFree)) - CDbl(IIf(ObjSumReturn Is DBNull.Value, "0", ObjSumReturn))
            lstDelivery.Splits(0).DisplayColumns("MOD_OrdQty").DataColumn.FooterText = CType(objSum, Double).ToString("N" & gbDecNo)
        Catch ex As Exception
            MessageBox.Show("Error: " & ex.Message, cntTitle, MessageBoxButtons.OK, MessageBoxIcon.Error)
        End Try
    End Sub
    Private Sub lstDelivery_BeforeDelete(ByVal sender As Object, ByVal e As C1.Win.C1TrueDBGrid.CancelEventArgs) Handles lstDelivery.BeforeDelete

        Try
            If Wtype = "" Or Wtype = "DELETE" Then e.Cancel = True : Exit Sub
            If lstDelivery.Columns("new_old").Text.Trim = "O" Then e.Cancel = True : Exit Sub
            If MessageBox.Show("Are You Sure You Want To Delete This Selected line ?", cntTitle, MessageBoxButtons.YesNo, MessageBoxIcon.Question) = vbNo Then
                e.Cancel = True
                Exit Sub
            Else
                strsql = " DELETE FROM " & TempQcDetails & ""
                strsql &= " FROM " & TempQcDetails & " details INNER JOIN"
                strsql &= " " & TempQcHeader & " header ON header.ReciD = details.HRecID"
                strsql &= " WHERE PrDReciD = " & Val(lstDelivery.Columns("RECID").Text) & ""
                strsql &= " DELETE FROM  " & TempQcHeader & "  WHERE PrDReciD = " & Val(lstDelivery.Columns("RECID").Text) & ""
                Execute(strsql)
                If DelOrder = "Receiving Order" Or DelOrder = "Consignment Receiving Order" Then
                    strsql = " UPDATE " & TmpGridBarcode & " set bar_Stat = 'D' WHERE  RECID='" & Val(lstDelivery.Columns("RECID").Text) & "'"
                    strsql &= " ; UPDATE " & TmpGridExpiry & " set exp_stat='D' WHERE  RECID='" & Val(lstDelivery.Columns("RECID").Text) & "'"
                    Execute(strsql)
                    LstVisbility(False, False, lstDelivery.Columns("ItmClass").Text.Trim)
                Else
                    strsql = " UPDATE " & TmpGridBarcode & "F set tmp_Stat = 'D' WHERE  tmp_Drecid='" & Val(lstDelivery.Columns("RECID").Text) & "';"
                    strsql &= " Delete from " & TmpFashQty & " where Recid='" & Val(lstDelivery.Columns("RECID").Text) & "'"
                    Execute(strsql)
                    dsDetFqty.Tables(0).Clear()
                    daDetFqty.Fill(dsDetFqty, dsDetFqty.Tables(0).TableName)
                    PnlFashQty.Visible = False
                End If
                strsql = " SELECT  * FROM  " & TmpGridDetails
                strsql &= " INNER JOIN " & TempPendingDO & " ON SFP_RECID = MOD_RMRecid "
                'ch @ 02/06/2017 
                strsql &= " left join ( select RECID ItemID,SFP_Recid PendId  from " & TmpGridDetails & " INNER join  "
                strsql &= TempPendingDO & " ON SFP_RECID = MOD_RMRecid  WHERE new_old = 'w' AND SFP_Status = 'N'"
                strsql &= " and Recid<>'" & Val(lstDelivery.Columns("RECID").Text) & "' ) test ON SFP_Recid=PendId  "
                strsql &= "  WHERE new_old = 'w' AND SFP_Status = 'N' "
                strsql &= " AND Recid='" & Val(lstDelivery.Columns("RECID").Text) & "' AND  ItemID IS null"
                If RecordsExists(strsql) = True Then
                    strsql = " DELETE FROM " & TempPendingDO & "  FROM  " & TmpGridDetails
                    strsql &= " INNER JOIN " & TempPendingDO & " ON SFP_RECID = MOD_RMRecid  WHERE new_old = 'w' AND SFP_Status = 'N'"
                    strsql &= " AND Recid='" & Val(lstDelivery.Columns("RECID").Text) & "'"
                    Execute(strsql)
                Else
                    Dim drsql As SqlDataReader
                    strsql = "SELECT SFP_OldRecid FROM " & TmpGridDetails & " inner join " & TempPendingDO & "  on SFP_Recid = MOD_RMRecid  WHERE new_old='w'"
                    ReadFromDBToDataReader(strsql, drsql)
                    If drsql.HasRows Then
                        drsql.Read()
                        AllLocks(AllLocksType.ReleaseLock, "STK04_StkFilPending", drsql.Item("SFP_OldRecid").ToString, cntTitle, lcSteps)
                    End If
                End If

                If dsDetDetails.Tables("tblDetails").Select("").Length() = 0 Then
                    txtOrderNo.Text = ""
                    btnSupp.Enabled = True : txtSupp.ReadOnly = False
                    btnBrand.Enabled = True : txtBrand.ReadOnly = False
                    btnSeason.Enabled = True : txtSeason.ReadOnly = False
                    txtDate.ReadOnly = False
                    txtCurr.ReadOnly = False : btnCurr.Enabled = True
                    PnlFashQty.Visible = False
                    lstDelivery.Columns("Color").FooterText = ""
                End If

            End If

            INITCOL(False)
            txtSTkID.Text = ""

        Catch ex As Exception
            MessageBox.Show("Error: " & ex.Message, cntTitle, MessageBoxButtons.OK, MessageBoxIcon.Error)
        End Try
    End Sub
    Private Sub btnCancel_Click(ByVal sender As System.Object, ByVal e As System.EventArgs) Handles btnCancel.Click
        Try
            If OldValue <> "" And Wtype <> "" Then
                AllLocks(AllLocksType.ReleaseLock, "stk04_PurDelOrdH", txtCode.Text.Trim, cntTitle, Me.Name)
            End If
            If OldVal <> "" Then
                AllLocks(AllLocksType.ReleaseLock, "STK04_mordH", OldVal, cntTitle, Me.Name)
            End If

            strsql = "delete from sysLockRecords where lr_Table = 'STK04_StkFilPending' and lr_Machine='" & gbPCName & "'"
            strsql &= " and lr_Application='" & gbApp & "' and lr_Menu='" & lcSteps & "'"
            Execute(strsql)
            GrpExpBarcode.Visible = False
            Wtype = ""
            lcChanges = False
            lcChangeKey = False
            save = False
            OldVal = ""
            OldValue = ""
            With txtMode
                .Text = "..."
                .ForeColor = SystemColors.GrayText
                .Image = Nothing
            End With
            With txtUstmp
                .Text = "..."
                .ForeColor = SystemColors.GrayText
            End With
            With txtDstmp
                .Text = "..."
                .ForeColor = SystemColors.GrayText
            End With
            Print.Visible = False
            ClearControls(Me, ErrorProvider1)
            INITCOL(True)
            InitGrids()
            RefQtyLbl(True)
            lstDelivery.AllowAddNew = False
            LstBarcode.AllowAddNew = False
            LstExpiry.AllowAddNew = False
            LstVisbility(False, False, lstDelivery.Columns("ItmClass").Text.Trim)

            EnabledButtons(Me, False, btnDelete, btnAdd, btnModify, btnSearch, btnClose, btnPreview)

            btnAdd.Enabled = lcPrivAdd
            btnModify.Enabled = lcPrivModify
            btnDelete.Enabled = lcPrivDelete
            btnSearch.Enabled = lcPrivView
            btnPreview.Enabled = lcPrivPrint
            btnCancel.Enabled = False
            btnUndo.Enabled = False
            btnSave.Enabled = False

            lstDelivery.AllowUpdate = False
            lstDelivery.AllowDelete = False
            lstDelivery.AllowAddNew = False

            LstBarcode.AllowUpdate = False
            LstBarcode.AllowDelete = False
            LstBarcode.AllowAddNew = False

            LstBarcodeF.AllowUpdate = False
            LstBarcodeF.AllowDelete = False
            LstBarcodeF.AllowAddNew = False

            LstExpiry.AllowUpdate = False
            LstExpiry.AllowDelete = False
            LstExpiry.AllowAddNew = False

            EnabledControls(Me, False)


            VisibleBtn(Me, False)
            LstVisbility(False, False, lstDelivery.Columns("ItmClass").Text.Trim)

            lstDelivery.Col = 0
            lstDelivery.Splits(0).HorizontalOffset = 0
            lstDelivery.Refresh()
            PnlFashQty.Visible = False
            Flowhead.Visible = False
            '  PnlHead3.Visible = False
            'If DelOrder = "Consignment Receiving Order" Or DelOrder = "Consignment Receiving Order Fashion" Then
            ' PnlConsigment.Visible = False
            ' End If

            btnCreate.Text = "Add" & vbCrLf & "New Item" & vbCrLf & "Ctrl+ M"
            Me.Refresh()
        Catch ex As Exception
            MessageBox.Show("Error: " & ex.Message, "", MessageBoxButtons.OK, MessageBoxIcon.Error)
        End Try
    End Sub

    Private Sub btnUndo_Click(ByVal sender As System.Object, ByVal e As System.EventArgs) Handles btnUndo.Click
        Try
            If txtCode.Text.Trim = "" Then txtCode.Focus() : Exit Sub
            RefreshControls()
            txtCode.Focus()
        Catch ex As Exception
            MessageBox.Show("Error: " & ex.Message, cntTitle, MessageBoxButtons.OK, MessageBoxIcon.Error)
        End Try
    End Sub

    Private Sub btnClose_Click(ByVal sender As System.Object, ByVal e As System.EventArgs) Handles btnClose.Click
        Try
            Me.Close()
        Catch exp As Exception
            MessageBox.Show("Error : " & exp.Message, cntTitle, _
            MessageBoxButtons.OK, MessageBoxIcon.Error)
        End Try
    End Sub

    Private Sub btnDelete_Click(ByVal sender As System.Object, ByVal e As System.EventArgs) Handles btnDelete.Click
        Try

            Wtype = "DELETE"

            EnabledButtons(Me, False, btnSearch, btnCancel, btnSave, btnClose)
            btnPreview.Enabled = lcPrivPrint

            EnabledControls(Me, True)

            VisibleBtn(Me, False)
            EnabledButtons(Me, False, btnSearch, btnCancel, btnSave, btnClose, btnPreview)

            ClearControls(Me, ErrorProvider1)
            INITCOL(True)
            RefreshControls()
            'Enable Key
            With txtMode
                .Text = "Canceling An Existing Receiving Order."
                .ForeColor = Color.Red
                .Image = btnDelete.Image
            End With
            With txtUstmp
                .Text = "..."
                .ForeColor = Color.Red
            End With
            With txtDstmp
                .Text = "..."
                .ForeColor = Color.Red
            End With

            InitGrids()
            lstDelivery.AllowAddNew = False
            LstBarcode.AllowAddNew = False
            LstExpiry.AllowAddNew = False
            lstDelivery.AllowUpdate = False
            LstBarcode.AllowUpdate = False
            LstBarcodeF.AllowUpdate = False
            LstExpiry.AllowUpdate = False
            txtBrand.ReadOnly = True
            txtSeason.ReadOnly = True
            VisibleBtn(Me, False)
            LstVisbility(False, False, lstDelivery.Columns("ItmClass").Text.Trim)
            txtCode.ReadOnly = False
            RefQtyLbl(True)

            ReadOnlyCtr(Me, True, True, txtDate, txtSupp, txtDelOrder)
            lstDelivery.AllowUpdate = False
            txtCode.ReadOnly = False
            txtSupp.ReadOnly = False
            btnSupp.Visible = True
            btnSupp.Enabled = True
            txtDate.Enabled = True
            txtDate.ReadOnly = False
            OldVal = ""
            OldValue = ""
            txtCode.Focus()
        Catch ex As Exception
            MessageBox.Show("Error : " & ex.Message, cntTitle, _
            MessageBoxButtons.OK, MessageBoxIcon.Error)
        End Try
    End Sub

    Private Sub btnSupp_Click(ByVal sender As System.Object, ByVal e As System.EventArgs) Handles btnSupp.Click
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

            strsql = " select Sup_cODE as [Code],Sup_name as [Supplier Name]"
            strsql &= " From Stk04_Supplier "
            If UCase(gbUser) <> "SA" And UCase(gbUser) <> "SU" Then
                strsql &= " left join Stk04_BusUnitManagPriv on BUMP_SuppCode=Sup_cODE"
            End If
            strsql &= " WHERE 1=1 "
            If UCase(gbUser) <> "SA" And UCase(gbUser) <> "SU" Then
                strsql &= " and BUMP_UserCode='" & gbUser & "' AND BUMP_Access='1'  "
            End If
            strsql &= " and  Sup_Active='Y'"

            cnSql = New SqlConnection(ConnectionString)
            cnSql.Open()
            daSql = New SqlDataAdapter(strsql, cnSql)
            dsSql = New DataSet
            daSql.Fill(dsSql, "tbcode3")
            cnSql.Close()
            frm = New frmSearch

            With frm
                .Owner = Me
                If .ShowDialog("Return", dsSql.Tables("tbcode3"), dsSql.Tables("tbcode3").Rows.Count, _
                "Supplier", Sizefield, Alnfield, Retfield, 0, Nothing, False, 400, 500, , True) = DialogResult.OK Then
                    If gbRetSearch(0) = "" Then
                        txtSupp.Focus()
                    Else
                        If gbRetSearch(0) <> "" Then
                            txtSupp.Text = gbRetSearch(0)
                            lblSupp.Text = gbRetSearch(1)
                            txtSupp.Focus()
                            SendKeys.Send("{TAB}")
                        Else
                            txtSupp.Focus()
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
    'Added by Abir T. @ 17.05.2013 
    'Get the color list from stockquantity table where the location = ‘00’
    Public Function StkQtyColorStrsql(Optional ByVal Cond As String = "") As String
        Try
            StkQtyColorStrsql = "SELECT DISTINCT STQ_Color AS Code, Color_Reference AS Description"
            StkQtyColorStrsql &= " FROM stk04_stkqty"
            StkQtyColorStrsql &= " INNER JOIN Stk04_COLOR ON Color_Code = STQ_Color"
            StkQtyColorStrsql &= " WHERE STQ_Color Is Not NULL AND STQ_LOCCODE = '00'"
            If Cond <> "" Then StkQtyColorStrsql &= " AND " & Cond
        Catch ex As Exception
            Throw ex
        End Try
    End Function

    Private Sub InsertNonAddedItemsStk(ByVal TempTable As String, ByVal StkRecid As String, ByVal Reason As String, _
                                Optional ByVal SupCol As String = "", Optional ByVal Color As String = "")
        Dim Str As String
        Try
            Str = "INSERT INTO " & TempTable
            Str &= " SELECT STK_Recid, STK_Ref, '" & SupCol & "', STK_Desc,"
            Str &= " '" & Color & "', '" & Reason.Replace("'", "''") & "' AS Reason"
            Str &= " FROM STK04_STKFIL WHERE STK_Recid = " & StkRecid
            Execute(Str)
        Catch ex As Exception
            Throw ex
        End Try
    End Sub

    Private Sub btnStkSearch_Click(ByVal sender As System.Object, ByVal e As System.EventArgs) Handles btnStkSearch.Click
        Dim Strsql As String
        Dim Str As String
        Dim fcsCtr As Control
        Dim SuppRef As String = ""
        Dim SuppColor As String = ""
        Dim dsSQL As DataSet
        Dim dt As DataTable
        Dim drSQL2 As SqlDataReader
        Dim ChkCol As Boolean
        Dim ChkBar As Boolean
        Dim ChkSupRef As Boolean
        Dim ChkSupColor As Boolean
        Dim Color As String = ""
        Dim Barcode As String = ""
        Dim LastCost As Double
        Dim ValidItem As Boolean
        Dim UpdSupRef As Boolean
        Dim MSG As String
        Dim i, j, k As Integer
        Dim SupCol As String

        Dim frm As frmMsgGrid
        Dim SendMessage As String

        Try
            If Wtype = "DELETE" Or Wtype = "" Then Exit Sub
            If Wtype = "MODIFY" And txtCode.Text = "" Then Exit Sub
            If lstDelivery.Enabled = False Then Exit Sub

            Cursor.Current = Cursors.WaitCursor

            If DelOrder = "Receiving Order Fashion" Or DelOrder = "Consignment Receiving Order Fashion" Then
                If txtSeason.Text.Trim = "" Then
                    ErrorProvider1.SetError(txtSeason, "You Should Choose A Season ")
                    fcsCtr = txtSeason
                End If
                If txtBrand.Text.Trim = "" Then
                    ErrorProvider1.SetError(txtBrand, "You Should Choose A Brand ")
                    fcsCtr = txtBrand
                End If
            End If
            If txtActualDate.Value Is DBNull.Value Then
                ErrorProvider1.SetError(txtActualDate, "Actual Date Can Not Be Null ")
                fcsCtr = txtActualDate
            End If
            If txtLoc.Text.Trim = "" Then
                ErrorProvider1.SetError(txtLoc, "You Should Choose Receiving Location ")
                fcsCtr = txtLoc
            End If
            If txtDate.Value Is DBNull.Value Then
                ErrorProvider1.SetError(txtDate, "Date Can Not Be Null ")
                fcsCtr = txtDate
            End If
            If txtDelOrder.Text.Trim = "" Then
                ErrorProvider1.SetError(txtDelOrder, "You Should Put A Del. Order # ")
                fcsCtr = txtDelOrder
            End If
            If txtSupp.Text.Trim = "" Then
                ErrorProvider1.SetError(txtSupp, "You Should Choose A Supplier ")
                fcsCtr = txtSupp
            End If
            If Wtype <> "ADD" Then
                If txtCode.Text.Trim = "" Then
                    ErrorProvider1.SetError(txtCode, "Code Can Not Be Null ")
                    fcsCtr = txtCode
                End If
            End If
            If DelOrder = "Consignment Receiving Order" Or DelOrder = "Consignment Receiving Order Fashion" Then
                If txtCurr.Text.Trim = "" Then
                    ErrorProvider1.SetError(txtCurr, "Currency Can Not Be Null ")
                    fcsCtr = txtCurr
                    Exit Sub
                End If
            End If
            If Not fcsCtr Is Nothing Then
                MessageBox.Show("Missing Data.", cntTitle, MessageBoxButtons.OK, MessageBoxIcon.Question)
                fcsCtr.Focus()
                Exit Try
            End If

            If DelOrder = "Receiving Order Fashion" Then
                'HosDLL.HOSDLLModvar.ShowStockList(gbUser, ConnectionString, gbApp, HosDLL.HOSDLLModvar.StockListTypes.StockList, True, True, True, "", False, Today.Date, gbComp, gbCurr1, gbCurr2, gbYear, gbFashion, gbExpiry, gbTrading, gbBook, True, False, HosDLL.HOSDLLModvar.Access.Purchase, "[ItemProperties] like '%STK_ACTIVE=Y%' AND [ItemProperties] like '%STK_CurrentArchive=C%' AND [ItemProperties] like '%STK_HaltPoPi=N%' AND [ItemProperties] not like '%STK_NEWUSED=C%' AND [ItemProperties] like '%Stk_ItmClass=F%' AND [ItemProperties] LIKE '%Stk_Brand=" & txtBrand.Text.Trim.Replace("'", "''") & "%' AND [ItemProperties] LIKE '%Stk_Season=" & txtSeason.Text.Trim.Replace("'", "''") & "%' ") ', "Stock List") 'modified by GS on 08/07/16 (add Title param)

                If clsStockSearch.LoadStockSearch(Me, FrmStockSearch.StockSearchType.SearchWithCheck, FrmStockSearch.StockSearchReturnValue.SSrv_TempTable, Nothing, _
                                                 {{"  ISNULL(STK_HaltPoPi,'N') = 'N' OR (ISNULL(STK_HaltPoPi,'N') = 'Y' and STK_HaltActTill < getdate()) ", "This Item Is Halted For PO/PI"}, _
                                                  {"  STK_ItmClass = 'F'  ", "Only Fashion Item(s) Should Be Selected"}, _
                                                  {"  Stk_Brand = '" & txtBrand.Text.Trim.Replace("'", "''") & "'  ", "Brand Should Be " & lblBrand.Text.Trim & " "}, _
                                                  {"  Stk_Season = '" & txtSeason.Text.Trim.Replace("'", "''") & "'  ", "Season Should Be " & lblSeason.Text.Trim & " "}, _
                                                  {"  STK_NEWUSED <> 'C'  ", "Consignment Item(s) Cannot Be Selected"}, _
                                                  {"  STK_ACTIVE = 'Y' ", "Not Active Item"}, _
                                                  {"  not exists (select top 1 1 from V_StockNormalCons  where stk_recid =NSTK_RECID ) ", "Normal Consignment Item(s) Cannot Be Selected"} _
                                                 }) <> Windows.Forms.DialogResult.OK Then

                    Exit Try

                End If

            ElseIf DelOrder = "Consignment Receiving Order Fashion" Then
                'HosDLL.HOSDLLModvar.ShowStockList(gbUser, ConnectionString, gbApp, HosDLL.HOSDLLModvar.StockListTypes.StockList, True, True, True, "", False, Today.Date, gbComp, gbCurr1, gbCurr2, gbYear, gbFashion, gbExpiry, gbTrading, gbBook, True, False, HosDLL.HOSDLLModvar.Access.Purchase, "[ItemProperties] like '%STK_ACTIVE=Y%' AND [ItemProperties] like '%STK_CurrentArchive=C%' AND [ItemProperties] like '%STK_HaltPoPi=N%' AND [ItemProperties]   like '%STK_NEWUSED=C%' AND [ItemProperties] like '%Stk_ItmClass=F%' AND [ItemProperties] LIKE '%Stk_Brand=" & txtBrand.Text.Trim.Replace("'", "''") & "%' AND [ItemProperties] LIKE '%Stk_Season=" & txtSeason.Text.Trim.Replace("'", "''") & "%' AND [ItemProperties] LIKE '%STK_SUPPLIER=" & txtSupp.Text.Trim.Replace("'", "''") & "%' AND [ItemProperties] LIKE '%STK_SUPPLIER=" & txtSupp.Text.Trim.Replace("'", "''") & "%'") ', "Stock List") 'modified by GS on 08/07/16 (add Title param)

                If clsStockSearch.LoadStockSearch(Me, FrmStockSearch.StockSearchType.SearchWithCheck, FrmStockSearch.StockSearchReturnValue.SSrv_TempTable, Nothing, _
                                                 {{"  ISNULL(STK_HaltPoPi,'N') = 'N' OR (ISNULL(STK_HaltPoPi,'N') = 'Y' and STK_HaltActTill < getdate()) ", "This Item Is Halted For PO/PI"}, _
                                                  {"  STK_ItmClass = 'F'  ", "Only Fashion Item(s) Should Be Selected"}, _
                                                  {"  Stk_Brand = '" & txtBrand.Text.Trim.Replace("'", "''") & "'  ", "Brand Should Be " & lblBrand.Text.Trim & " "}, _
                                                  {"  Stk_Season = '" & txtSeason.Text.Trim.Replace("'", "''") & "'  ", "Season Should Be " & lblSeason.Text.Trim & " "}, _
                                                  {"  STK_SUPPLIER = '" & txtSupp.Text.Trim.Replace("'", "''") & "'  ", "Supplier Should Be " & lblSupp.Text.Trim & " "}, _
                                                  {"  STK_NEWUSED = 'C'  ", "Only Consignement Item(s) Should Be Selected"}, _
                                                  {"  STK_ACTIVE = 'Y' ", "Not Active Item"} _
                                                 }) <> Windows.Forms.DialogResult.OK Then

                    Exit Try

                End If

            ElseIf DelOrder = "Receiving Order" Then
                'HosDLL.HOSDLLModvar.ShowStockList(gbUser, ConnectionString, gbApp, HosDLL.HOSDLLModvar.StockListTypes.StockList, True, True, True, "", False, Today.Date, gbComp, gbCurr1, gbCurr2, gbYear, gbFashion, gbExpiry, gbTrading, gbBook, True, False, HosDLL.HOSDLLModvar.Access.Purchase, "[ItemProperties] like '%STK_ACTIVE=Y%' AND [ItemProperties] like '%STK_CurrentArchive=C%' AND [ItemProperties] like '%STK_HaltPoPi=N%' AND [ItemProperties] not like '%STK_NEWUSED=C%' AND [ItemProperties] not like '%Stk_ItmClass=F%' ") ', "Stock List") 'modified by GS on 08/07/16 (add Title param)

                If clsStockSearch.LoadStockSearch(Me, FrmStockSearch.StockSearchType.SearchWithCheck, FrmStockSearch.StockSearchReturnValue.SSrv_TempTable, Nothing, _
                                                 {{"  ISNULL(STK_HaltPoPi,'N') = 'N' OR (ISNULL(STK_HaltPoPi,'N') = 'Y' and STK_HaltActTill < getdate()) ", "This Item Is Halted For PO/PI"}, _
                                                  {"  STK_ItmClass <> 'F'  ", "Fashion Item(s) Cannot Be Selected"}, _
                                                  {"  STK_NEWUSED <> 'C'  ", "Consignment Item(s) Cannot Be Selected"}, _
                                                  {"  STK_ACTIVE = 'Y' ", "Not Active Item"}, _
                                                   {"  not exists (select top 1 1 from V_StockNormalCons  where stk_recid =NSTK_RECID ) ", "Normal Consignment Item(s) Cannot Be Selected"} _
                                                 }) <> Windows.Forms.DialogResult.OK Then

                    Exit Try

                End If

            ElseIf DelOrder = "Consignment Receiving Order" Then
                'HosDLL.HOSDLLModvar.ShowStockList(gbUser, ConnectionString, gbApp, HosDLL.HOSDLLModvar.StockListTypes.StockList, True, True, True, "", False, Today.Date, gbComp, gbCurr1, gbCurr2, gbYear, gbFashion, gbExpiry, gbTrading, gbBook, True, False, HosDLL.HOSDLLModvar.Access.Purchase, "[ItemProperties] like '%STK_ACTIVE=Y%' AND [ItemProperties] like '%STK_CurrentArchive=C%' AND [ItemProperties] like '%STK_HaltPoPi=N%' AND [ItemProperties]   like '%STK_NEWUSED=C%' AND [ItemProperties] not like '%Stk_ItmClass=F%' AND [ItemProperties] LIKE '%STK_SUPPLIER=" & txtSupp.Text.Trim.Replace("'", "''") & "%'") ', "Stock List") 'modified by GS on 08/07/16 (add Title param)

                If clsStockSearch.LoadStockSearch(Me, FrmStockSearch.StockSearchType.SearchWithCheck, FrmStockSearch.StockSearchReturnValue.SSrv_TempTable, Nothing, _
                                                 {{"  ISNULL(STK_HaltPoPi,'N') = 'N' OR (ISNULL(STK_HaltPoPi,'N') = 'Y' and STK_HaltActTill < getdate()) ", "This Item Is Halted For PO/PI"}, _
                                                  {"  STK_ItmClass <> 'F'  ", "Fashion Item(s) Cannot Be Selected"}, _
                                                  {"  STK_NEWUSED = 'C'  ", "Only Consignement Item(s) Should Be Selected"}, _
                                                  {"  STK_SUPPLIER = '" & txtSupp.Text.Trim.Replace("'", "''") & "'  ", "Supplier Should Be " & lblSupp.Text.Trim & " "}, _
                                                  {"  STK_ACTIVE = 'Y' ", "Not Active Item"} _
                                                 }) <> Windows.Forms.DialogResult.OK Then

                    Exit Try

                End If

            End If
            ErrorProvider1.SetError(lstDelivery, "")

            If Not clsStockSearch.TempTableExists Then Exit Try
            If Not clsStockSearch.TempTableItemsExists Then Exit Try

            'Strsql = " SELECT * FROM dbo.STK04_UserStockList"
            'Strsql &= " WHERE USL_user='" & gbUser & "' AND USL_Type='S' AND USL_Column='[SRef_SupRef]'"
            'ReadFromDBToDataReader(Strsql, drSQL2)
            'If drSQL2.HasRows Then
            '    ' d()
            'End If


            'Test if the Supplier Reference is selected in the stock search

            ChkSupRef = clsStockSearch.ss_FldSuppRef

            If DelOrder = "Receiving Order Fashion" Or DelOrder = "Consignment Receiving Order Fashion" Then

                'Test if the Supplier Color is selected in the stock search
                ChkSupColor = clsStockSearch.ss_FldSuppColor

                'Test if the color is selected in the stock search
                ChkCol = clsStockSearch.ss_FldColor

                'Test if the Barcode is selected in the stock search
                ChkBar = clsStockSearch.ss_FldBarcode

            Else
                ChkCol = False : ChkBar = False : ChkSupColor = False
            End If

            'temptable to put int the unimported item from the selection of stock search 
            CreateTempNonAddedItemsStk(TempNonAddedItemsStk)


            'Test on every item if already exist 
            Strsql = "SELECT [ss_STK_RECID] AS RecID, *"
            Strsql &= " FROM " & StkSearchTempTable & " "
            If DelOrder = "Consignment Receiving Order" Or DelOrder = "Consignment Receiving Order Fashion" Then Strsql &= " left join (select stk_recid , STK_NEWUSED  from stk04_stkfil)  stk on [ss_STK_RECID]  =stk.stk_recid "
            Strsql &= " WHERE 1=1 "
            'ch @ 28/06/2017 
            If DelOrder = "Consignment Receiving Order" Or DelOrder = "Consignment Receiving Order Fashion" Then
                Strsql &= " and stk.STK_NEWUSED ='C'  "
            End If
            Strsql &= " order by ss_roworder "
            ReadFromDBToDataSet(Strsql, "tblStkSearch", dsSQL)
            dt = dsSQL.Tables("tblStkSearch")
            If dt.Rows.Count = 0 Then
                If Wtype = "ADD" Then
                    txtSupp.ReadOnly = False : btnSupp.Visible = True
                End If

                If DelOrder = "Receiving Order Fashion" Or DelOrder = "Consignment Receiving Order Fashion" Then
                    txtBrand.ReadOnly = False : btnBrand.Visible = True
                    txtSeason.ReadOnly = False : btnSeason.Visible = True
                End If
                Exit Sub
            End If
            For j = 0 To dt.Rows.Count - 1
                ValidItem = True

                If DelOrder = "Receiving Order" Or DelOrder = "Consignment Receiving Order" Then
                    'Trading 
                    ValidItem = True
                Else
                    'Fashion
                    If ChkCol = True Then
                        Color = dt(j)("ss_Color").ToString

                    ElseIf ChkBar = True Then
                        Barcode = dt(j)("ss_Barcode").ToString
                        'Get the color related to the combination barcode/Item 
                        Str = "SELECT DISTINCT STK_RECID, dbar_barcode, dbar_color FROM STK04_Stkfil"
                        Str &= " LEFT JOIN Stk04_DetBarcode ON  dbar_stkid = STK_RECID"
                        Str &= " WHERE dbar_color IS NOT NULL AND dbar_barcode = '" & Barcode & "'"
                        Str &= " AND STK_RECID = '" & Val(dt(j)("RecID")) & "'"
                        ReadFromDBToDataReader(Str, drSQL2)
                        If drSQL2.HasRows Then
                            drSQL2.Read()
                            Color = drSQL2.Item("dbar_color").ToString
                        End If
                    Else
                        'NoColor, No Barcode selected
                        Dim dsSQL2 As DataSet
                        Str = StkQtyColorStrsql(" STQ_STKRECID = " & Val(dt(j)("RecID")))
                        ReadFromDBToDataSet(Str, "tbColor", dsSQL2)
                        If dsSQL2.Tables("tbColor").Rows.Count = 1 Then
                            Color = dsSQL2.Tables("tbColor")(0)(0).ToString
                        End If
                    End If


                    MSG = ""
                    SupCol = ""
                    'Test if (Item + Color) is duplicated 
                    Strsql = "SELECT * FROM " & TmpGridDetails
                    Strsql &= " WHERE MOD_RMRecid = " & Val(dt(j)("RecID"))
                    Strsql &= " AND MOD_Stat = 'N' AND COLOR = '" & Color & "'"
                    If RecordsExists(Strsql) Then
                        MSG = "Item Already Added, Cannot Duplicate Item."
                        ValidItem = False
                        If ChkSupColor Then ChkSupColor = True : SupCol = dt(j)("Supplier Color").ToString Else SupCol = "" 'to be checked
                        InsertNonAddedItemsStk(TempNonAddedItemsStk, dt(j)("RecID").ToString, MSG, SupCol, Color)

                    Else
                        ValidItem = True
                    End If
                End If


                'Get the selected Supplier Color
                If ChkSupColor Then
                    If Not IsDBNull(dt(j)("Supplier Color")) Then SuppColor = dt(j)("Supplier Color").ToString 'to be checked
                End If
                Dim drSQL As SqlDataReader
                If ChkSupRef Then
                    'Get the selected Supplier Reference
                    If Not IsDBNull(dt(j)("ss_SuppRef")) Then
                        SuppRef = dt(j)("ss_SuppRef").ToString
                    Else
                        SuppRef = ""
                    End If


                    If (DelOrder = "Receiving Order Fashion" Or DelOrder = "Consignment Receiving Order Fashion") And SuppColor = "" And SuppRef <> "" And Color <> "" Then
                        'Get the supplier Color for this item , supplier , supplier reference & color
                        Strsql = " SELECT SRef_SupColor FROM STK04_StkSupRef"
                        Strsql &= " WHERE SRef_SupRecId = '" & GetFieldDesc("STK04_supplier", "Sup_Code='" & txtSupp.Text.Trim.Replace("'", "''") & "'", "Sup_recid") & "'"
                        Strsql &= " AND SRef_STKRECID = " & dt(j)("RecID").ToString.Replace("'", "''")
                        Strsql &= " AND SRef_SupRef = '" & SuppRef.Replace("'", "''") & "'"
                        Strsql &= " AND SRef_ItmColor = '" & Color.Replace("'", "''") & "'"

                        ReadFromDBToDataReader(Strsql, drSQL)
                        If drSQL.HasRows Then
                            drSQL.Read()
                            SuppColor = drSQL.Item(0).ToString
                        Else
                            SuppColor = ""
                        End If
                    End If
                Else
                    'Get the last supplier reference for this item , supplier , & color (in case if fashion)
                    Strsql = " SELECT TOP 1 SRef_SupRef, SRef_SupColor FROM STK04_StkSupRef"
                    Strsql &= " WHERE SRef_SupRecId = '" & GetFieldDesc("Stk04_supplier", "sup_code='" & txtSupp.Text.Trim & "'", "sup_recid") & "'"
                    Strsql &= " AND SRef_STKRECID = " & dt(j)("RecID").ToString
                    If (DelOrder = "Receiving Order Fashion" Or DelOrder = "Consignment Receiving Order Fashion") Then Strsql &= " AND SRef_ItmColor = '" & Color.Replace("'", "''") & "'"
                    Strsql &= " ORDER BY SREF_RecId DESC"
                    ReadFromDBToDataReader(Strsql, drSQL)
                    If drSQL.HasRows Then
                        drSQL.Read()
                        SuppRef = drSQL.Item(0).ToString
                        If (DelOrder = "Receiving Order Fashion" Or DelOrder = "Consignment Receiving Order Fashion") And SuppColor = "" And Not IsDBNull(drSQL.Item(1)) Then SuppColor = drSQL.Item(1).ToString
                    Else
                        SuppRef = ""
                    End If
                End If

                'insering items choosen from stk search into the grid
                If ValidItem = True Then
                    lstDelivery.Focus()
                    row = dsDetDetails.Tables("tblDetails").NewRow


                    Dim dt1 As DataTable
                    dt1 = dsDetDetails.Tables("tblDetails")
                    Dim dv As New DataView(dt1)
                    dv.RowFilter = "RecId =Max(RecId)"
                    If dv.Count = 0 Then
                        MaxRecID = 0
                    Else
                        MaxRecID = CLng(dv(0)(0).ToString)
                    End If

                    row("RECID") = MaxRecID + 1

                    ''''to change 01/07/2013

                    If RowStkInfo(CStr(Val(dt(j)("RecID"))), CStr(IIf(ChkCol = False, "", Color.Replace("'", "''"))), CStr(IIf(ChkSupRef = False, "", SuppRef.Replace("'", "''"))), CStr(IIf(ChkSupColor = False, "", SuppColor.Replace("'", "''")))) = 0 Then
                        dsDetDetails.Tables("tblDetails").Rows.Add(row)
                        daDetDetails.Update(dsDetDetails, "tblDetails")
                    End If
                    Color = ""
                    SuppRef = ""
                    SuppColor = ""
                End If
            Next

            'uopio()
            Dim reason As Boolean
            'Strsql = " select count(distinct Reason)  cnt from " & TempNonAddedItemsStk

            Dim cnt As String = GetFieldDesc(TempNonAddedItemsStk, "1=1", "count(distinct Reason)")
            If cnt = "1" Then
                reason = False
                SendMessage = GetFieldDesc(TempNonAddedItemsStk, "1=1", " Reason ")
            Else
                reason = True
            End If

            If RecordsExists("SELECT TOP 1 ItemRecid FROM " & TempNonAddedItemsStk) Then
                frm = New frmMsgGrid
                With frm
                    If reason = True Then SendMessage = "List Of Items Already Added, Cannot Duplicate Items With Same Color."
                    ' .ShowReason = False
                    .ShowPI = False
                    .TempListItems = TempNonAddedItemsStk
                    .cntTitle = cntTitle
                    .FormName = Me.Name
                    .Fashion = CBool(IIf(DelOrder = "Receiving Order Fashion" Or DelOrder = "Consignment Receiving Order Fashion", True, False))
                    .ShowSupCol = ChkSupColor
                    .Width = 900
                    .Height = 400
                    .ShowReason = reason
                    .ShowDialog(SendMessage)
                End With
            End If

            'R.A. 07/05/2014''''''''''''''''''''''''''''''''
            'If UCase(lstDelivery.Columns("ItmClass").Text.Trim) = "F" Then
            'ElseIf UCase(lstDelivery.Columns("ItmClass").Text.Trim) = "E" Then
            '    LstVisbility(True, True, UCase(lstDelivery.Columns("ItmClass").Text.Trim))
            'ElseIf UCase(lstDelivery.Columns("ItmClass").Text.Trim) = "T" Or UCase(lstDelivery.Columns("ItmClass").Text.Trim) = "B" Then
            '    LstVisbility(False, True, UCase(lstDelivery.Columns("ItmClass").Text.Trim))
            'End If
            If Not dsDetDetails.Tables("tblDetails") Is Nothing Then dsDetDetails.Tables("tblDetails").Clear()
            daDetDetails.Fill(dsDetDetails, "tblDetails")
            RefQtyLbl(False)

            ReadonlyConsFash()


        Catch exp As Exception
            MessageBox.Show("Error : " & exp.Message, cntTitle, MessageBoxButtons.OK, MessageBoxIcon.Error)
        Finally
            Cursor.Current = Cursors.Default
        End Try
    End Sub

    Private Sub btnSearch_Click(ByVal sender As System.Object, ByVal e As System.EventArgs) Handles btnSearch.Click

        Dim strsql As String
        Dim cnSql As SqlConnection
        Dim dsSql As DataSet
        Dim daSql As SqlDataAdapter
        Dim frm As frmSearch
        Dim Sizefield() As Int16
        Dim Alnfield() As String
        Dim Retfield() As String
        Dim i As Integer
        If DelOrder = "Receiving Order" Or DelOrder = "Receiving Order Fashion" Then
            If DelOrder = "Receiving Order Fashion" Then
                Sizefield = {-1, -1, -1, -1, 130, -1, -1, -1, -1, -1, -1, -1, -1}
            Else
                Sizefield = {-1, -1, -1, -1, 180, -1, -1, -1, -1, -1, -1, 0, 0}
            End If
            Alnfield = {"CS", "C", "N", "C", "C", "N", "C", "N", "C", "C", "C", "C", "C"}
            Retfield = {"RO#", "Supplier", "Supplier Name", "Status", "RO Ref.", "RO Date", "Loc", "Loc. Name", "Actual RO Date", "Linked To PO", "Linked To PI", "Brand", "Season"}
            i = 800
        Else
            If DelOrder = "Consignment Receiving Order Fashion" Then
                Sizefield = {-1, -1, -1, -1, 130, -1, -1, -1, -1, -1, -1, -1}
            Else
                Sizefield = {-1, -1, 180, -1, 180, -1, -1, 150, -1, -1, 0, 0}
            End If
            Alnfield = {"C", "C", "N", "C", "C", "N", "C", "N", "C", "C", "C", "C"}
            Retfield = {"RO#", "Supplier", "Supplier Name", "Status", "RO Ref.", "RO Date", "Loc", "Loc. Name", "Actual RO Date", "Linked To PI", "Brand", "Season"}
            i = 675
        End If
        Try

            Cursor.Current = Cursors.WaitCursor
            strsql = "Select distinct DOH_Recid as [RO#],Sup_code as [Supplier], sup_name as [Supplier Name],"
            strsql &= " case when DOH_status='N' then 'Normal' else 'Reverse' end as [Status],"
            strsql &= " DOH_Ref as [RO Ref.],DOH_Date as [RO Date], DOH_loc as [Loc],"
            strsql &= " Loc_Desc as [Loc. Name],DOH_ActDelDate as [Actual RO Date] "
            If DelOrder = "Receiving Order" Or DelOrder = "Receiving Order Fashion" Then
                strsql &= " ,  convert(varchar,DOH_PORecid) as [Linked To PO] "
            End If
            strsql &= " ,  convert(varchar,DOH_PurRecid) as [Linked To PI] "
            strsql &= " , DOH_Brand [Brand], DOH_Season [Season] "
            strsql &= " From stk04_PurDelOrdH Inner join stk04_supplier  "
            strsql &= " On Sup_recid  =  DOH_SupRecid Inner join Stk04_location "
            strsql &= " On loc_code  =  DOH_Loc inner join stk04_purdelordd "
            strsql &= " on doh_RecId=DOD_Hrecid inner join stk04_stkfil "
            strsql &= " on DOD_StkRecId = stk_recid where 1=1 "

            If DelOrder = "Receiving Order" Then
                strsql &= "   and DOH_Type='T' and stk_newused <> 'C'"
            ElseIf DelOrder = "Receiving Order Fashion" Then
                strsql &= "   AND DOH_Type='F' and stk_newused <> 'C'"
            ElseIf DelOrder = "Consignment Receiving Order" Then
                strsql &= "   AND stk_newused = 'C' AND DOH_Type= 'T'"
            ElseIf DelOrder = "Consignment Receiving Order Fashion" Then
                strsql &= "    AND stk_newused = 'C' and  DOH_Type='F'"
            End If

            If Wtype = "DELETE" Or Wtype = "MODIFY" Then
                strsql &= " AND CONVERT(VARCHAR,DOH_SupRecid) + DOH_Ref+ CONVERT(VARCHAR,DOH_Date) "
                strsql &= " NOT IN ( SELECT CONVERT(VARCHAR,DOH_SupRecid) + DOH_Ref+ CONVERT(VARCHAR,DOH_Date)  "
                strsql &= " FROM stk04_PurDelOrdH WHERE doh_status = 'R')"
            End If
            'In modify mode, forbid the user to modify an RO that was purchased
            If Wtype = "MODIFY" OrElse Wtype = "DELETE" Then strsql &= " AND DOH_PurRecid IS null"

            'ch @ 20/02/2019
            '  strsql &= " Order By DOH_Date desc"
            strsql &= " ORDER BY DOH_Recid desc ,DOH_Date  desc "
            Dim drs As SqlDataReader
            ReadFromDBToDataReader(strsql, drs)
            If Not drs.HasRows Then
                MessageBox.Show("No Records Found.", cntTitle, MessageBoxButtons.OK, MessageBoxIcon.Exclamation)
                Exit Sub
            End If

            cnSql = New SqlConnection(ConnectionString)
            cnSql.Open()
            daSql = New SqlDataAdapter(strsql, cnSql)
            dsSql = New DataSet
            daSql.Fill(dsSql, "tbcode")
            cnSql.Close()
            frm = New frmSearch

            With frm
                .Owner = Me
                If .ShowDialog(IIf(Wtype = "ADD", "Locked", "Return").ToString, dsSql.Tables("tbcode"), dsSql.Tables("tbcode").Rows.Count, _
                DelOrder, Sizefield, Alnfield, Retfield, 0, Nothing, False, 950, 500, _
                False, True) = DialogResult.OK Then
                    If gbRetSearch(0) = "" Then
                        txtCode.Focus()
                    Else
                        If Wtype = "ADD" Then Exit Try
                        'If lcCurrentCode <> "" AndAlso gbRetSearch(0) <> lcCurrentCode And wtype <> "DELETE" Then
                        If lcChanges = True AndAlso Wtype <> "" AndAlso txtCode.Text <> "" Then

                            If MessageBox.Show("Do You Want To Discard Changes ?", cntTitle, MessageBoxButtons.YesNo, MessageBoxIcon.Question) = MsgBoxResult.Yes Then
                                If txtCode.Text.Trim <> "" And Wtype <> "" Then
                                    AllLocks(AllLocksType.ReleaseLock, "stk04_PurDelOrdH", txtCode.Text.Trim, cntTitle, Me.Name)
                                End If


                                '06.02.2014 Lock the pending items in modify mode
                                Dim Str As String
                                Dim drOldPend As SqlDataReader
                                Str = "SELECT SFP_OldRecid FROM " & TmpGridDetails & " inner join " & TempPendingDO & "  on SFP_Recid = MOD_RMRecid  WHERE new_old='w'"
                                ReadFromDBToDataReader(Str, drOldPend)
                                If drOldPend.HasRows Then
                                    While drOldPend.Read
                                        AllLocks(AllLocksType.ReleaseLock, "STK04_StkFilPending", drOldPend.Item(0).ToString, cntTitle, lcSteps)
                                    End While
                                End If

                                txtCode.Text = gbRetSearch(0)
                                RefreshControls()
                                txtCode.Focus()
                            Else
                                Exit Sub
                            End If
                        Else
                            If txtCode.Text <> "" And Wtype <> "" Then
                                AllLocks(AllLocksType.ReleaseLock, "stk04_PurDelOrdH", txtCode.Text.Trim, cntTitle, Me.Name)
                            End If

                            '06.02.2014 Lock the pending items in modify mode
                            Dim Str As String
                            Dim drOldPend As SqlDataReader
                            Str = "SELECT SFP_OldRecid FROM " & TmpGridDetails & " inner join " & TempPendingDO & "  on SFP_Recid = MOD_RMRecid  WHERE new_old='w'"
                            ReadFromDBToDataReader(Str, drOldPend)
                            If drOldPend.HasRows Then
                                While drOldPend.Read
                                    AllLocks(AllLocksType.ReleaseLock, "STK04_StkFilPending", drOldPend.Item(0).ToString, cntTitle, lcSteps)
                                End While
                            End If

                            txtCode.Text = gbRetSearch(0)
                            RefreshControls()
                            If Wtype = "" Then
                                With txtMode
                                    .Text = "View Mode."
                                    .ForeColor = Color.Red
                                    .Image = btnSearch.Image
                                End With
                                txtUstmp.ForeColor = Color.Red
                                txtDstmp.ForeColor = Color.Red
                                EnabledControls(Me, True)
                                ReadOnlyCtr(Me, True, True, txtDate, txtSupp, txtDelOrder)
                                lstDelivery.AllowUpdate = False
                                txtCode.ReadOnly = False
                                txtSupp.ReadOnly = False
                                btnSupp.Visible = True
                                btnSupp.Enabled = True
                                txtDate.Enabled = True
                                txtDate.ReadOnly = False
                            End If
                        End If
                    End If
                    txtCode.Enabled = True

                End If
            End With
            txtCode.Focus()
            Me.Refresh()

        Catch exp As Exception
            MessageBox.Show("Error : " & exp.Message, cntTitle, _
            MessageBoxButtons.OK, MessageBoxIcon.Error)
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
    Private Sub InsertStkFil(ByVal cmSql As SqlCommand, ByVal category As String, ByVal ItmIdent As String, ByVal Usage As String, ByVal NewUsed As String, ByVal ItmType As String, _
                                ByVal SourceLoc As String, ByVal SourceDate As String, ByVal ImpLoc As String, ByVal SerReg As String, ByVal UNITPACK As String, _
                                ByVal SubUnit As String, ByVal Curr As String, ByVal VatType As String, ByVal VATPER As String, ByVal TaxdATE As String, ByVal Ref1 As String, _
                                ByVal Ref2 As String, ByVal Grid As String, ByVal ItmClass As String, ByVal Gender As String, ByVal Styl As String, ByVal unit As String, _
                                ByVal SFP_Desc As String, ByVal Brand As String, ByVal Season As String, ByVal VATStat As String, ByVal Serie As String, _
                                Optional ByVal createInLoc As Boolean = True, Optional ByVal techSheet As String = "", Optional ByVal UNITOPERATOR As String = "" _
                                , Optional ByVal PendRecid As String = "")
        Dim strsql, NEWUSED1 As String
        Dim arrImage() As Byte
        Dim i, j As Integer
        Try


            strsql = " DECLARE @SerNo AS NUMERIC,@RecId AS NUMERIC,@ItmIdent AS NUMERIC,@RecIdBarc as NUMERIC	"
            'ch @ 11/11/2016 wf: 4618 
            ' If (txtStatus.Text = "C" And i = 1) OrElse txtStatus.Text <> "C" Then
            'strsql &= " SELECT  @SerNo = ISNULL(MAX(stk_serie), 0) + 1 FROM STK04_STKFIL"
            'Else
            '  strsql &= " SELECT  @SerNo = ISNULL(MAX(stk_serie), 0)  FROM STK04_STKFIL"
            ' End If
            '  strsql &= " where stk_supplier = '" & txtSupp.Text.Trim.Replace("'", "''") & "' "
            '  strsql &= " and stk_category = '" & category.Trim.Replace("'", "''") & "'"

            strsql &= " SELECT  @RecId = ISNULL(MAX(stk_recid), 0) + 1 FROM STK04_STKFIL"

            'strsql &= " SELECT  @ItmIdent = ID_RECID FROM stk04_itmidentif "
            'strsql &= " where id_code='" & ItmIdent.Trim.Replace("'", "''") & "'"

            strsql &= " SELECT  @RecIdBarc = ISNULL(MAX(dbar_recid), 0) FROM Stk04_DetBarcode "

            strsql &= " INSERT INTO dbo.STK04_STKFIL "
            strsql &= " VALUES  ( @RecId , '" & gbComp & "' , '" & txtSupp.Text.Trim.Replace("'", "''") & "' , "
            '  strsql &= " '" & category.Trim.Replace("'", "''") & "' , @SerNo, "
            strsql &= " '" & category.Trim.Replace("'", "''") & "' ," & Serie & ", "
            strsql &= " '" & Usage.Trim.Replace("'", "''") & "' , "

            strsql &= " '" & NewUsed.Trim.Replace("'", "''") & "' , "

            If SourceLoc.Trim = "" Then
                strsql &= " null,"
            Else
                strsql &= " '" & SourceLoc.Trim.Replace("'", "''") & "' , "
            End If
            If SourceDate.Trim <> "" Then
                strsql &= " '" & CType(SourceDate, Date).ToString("MM/dd/yyyy") & "' , "
            Else
                strsql &= "null, "
            End If
            ' If ChkActive.Checked = True Then
            strsql &= " 'Y' , "
            'Else
            'strsql &= "'N', "
            'End If
            strsql &= " '" & SFP_Desc.Trim.Replace("'", "''") & "' , Null , "
            strsql &= " '" & CType(Now, Date).ToString("MM/dd/yyyy") & "' , "
            'If rbLocal.Checked = True Then
            strsql &= " '" & ImpLoc.Trim.Replace("'", "''") & "' ,"
            'Else
            '    strsql &= "'I', "
            'End If
            If NewUsed.Trim = "C" And i = 1 Then
                strsql &= " 'Y',"
            Else
                strsql &= "'N', "
            End If
            strsql &= " '" & ItmIdent & "',"
            'If rbService.Checked = True Then
            strsql &= " '" & SerReg.Trim.Replace("'", "''") & "' , "
            ' Else
            'strsql &= "'R', "
            'End If
            'If ChkPromotion.Checked = True Then
            '    strsql &= "'Y' , "
            'Else
            strsql &= "'N',"
            ' End If
            strsql &= "'" & ItmType.Trim.Replace("'", "''") & "' ,"
            strsql &= " NULL, " 'STK_BOOKID - numeric"
            strsql &= " '" & unit.Trim.Replace("'", "''") & "' , "
            'If ChkQtyControl.Checked = True Then
            '    strsql &= " 'Y' , "
            'Else
            If NewUsed.Trim.Replace("'", "''") = "C" Then
                strsql &= "'Y' , "
            Else
                strsql &= "'N' , "
            End If
            'End If
            'If ChkTransposition.Checked = True Then
            '    strsql &= " 'Y',"
            'Else
            strsql &= " 'N',"
            'End If
            'If ChkSpPromotion.Checked = True Then
            '    strsql &= " 'Y' , "
            'Else
            strsql &= " 'N', "
            'End If
            'If Not TxtSpSTDate.Value Is DBNull.Value Then
            '    strsql &= " '" & CType(TxtSpSTDate.Text, Date).ToString("MM/dd/yyyy") & "' , "
            'Else
            strsql &= "null, "
            'End If
            'If Not TxtSpEdDate.Value Is DBNull.Value Then
            'strsql &= " '" & CType(TxtSpEdDate.Text, Date).ToString("MM/dd/yyyy") & "' , "
            'Else
            strsql &= "null, "
            'End If
            'If ChkConsumable.Checked = True Then
            '    strsql &= "'Y' ,"
            'Else
            strsql &= "'Y', "
            'End If
            'If TxtLifeUsageCount.Text.Trim = "" Then
            strsql &= "0, "
            'Else
            '    strsql &= "'" & TxtLifeUsageCount.Text.Trim & "', "
            'End If
            If UNITPACK.Trim.Trim = "" Then
                strsql &= "null,"
            Else
                strsql &= "'" & UNITPACK.Trim.Replace("'", "''") & "',"
            End If
            If SubUnit.Trim = "" Then
                strsql &= "null,"
            Else
                strsql &= "'" & SubUnit.Trim.Replace("'", "''") & "',"
            End If
            'If TxtBBFDate.Value Is DBNull.Value Then
            '    strsql &= " null,"
            'Else
            strsql &= " '" & CType(Now, Date).ToString("MM/dd/yyyy") & "',"
            '  End If
            strsql &= " 0,0,0,0,0,0,"
            strsql &= " '" & Curr.Trim.Replace("'", "''") & "' ,"
            strsql &= " '" & VatType.Trim.Replace("'", "''") & "' , "
            If VATPER.Trim = "" Then
                strsql &= " NULL , "
            Else
                strsql &= "'" & CDbl(VATPER.Trim) & "', "
            End If
            If TaxdATE.Trim = "" Then
                strsql &= " null,"
            Else
                strsql &= "'" & CType(TaxdATE, Date).ToString("MM/dd/yyyy") & "', "
            End If
            ' If TxtMinQty.Text.Trim = "" Then
            strsql &= " NULL, "
            ' Else
            'strsql &= "'" & TxtMinQty.Text.Trim.Replace("'", "''") & "', "
            ' End If
            'If TxtMaxQty.Text.Trim = "" Then
            strsql &= " NULL, "
            'Else
            'strsql &= "'" & TxtMaxQty.Text.Trim.Replace("'", "''") & "', "
            'End If
            'If rbUserEntry.Checked = True Then
            strsql &= " 'U',"
            'Else
            'strsql &= " 'N',"
            ' End If
            'If TxtDefDate.Value Is DBNull.Value Then
            '    strsql &= " null,"
            'Else
            strsql &= " '" & CType(Now, Date).ToString("MM/dd/yyyy") & "',"
            ' End If
            'If TxtItmOfferQty.Text.Trim <> "" Then
            '    strsql &= " '" & TxtItmOfferQty.Text.Trim & "',"
            'Else
            'strsql &= " null,"
            ''End If
            ''If Not TxtItemOffered.Tag Is Nothing AndAlso TxtItemOffered.Tag.ToString <> "" Then
            ''    strsql &= " '" & TxtItemOffered.Tag.ToString & "', "
            ''Else
            'strsql &= " null,"
            ''End If
            ''If TxtItmOfferedQty.Text.Trim <> "" Then
            ''    strsql &= " '" & TxtItmOfferedQty.Text.Trim & "',"
            ''Else
            'strsql &= " null,"
            'End If
            strsql &= " getdate() ,'" & gbUser & "' , "
            strsql &= " '" & ItmClass & "' , "
            strsql &= " 'DO' , 0,0,0,"
            If Ref1.Trim <> "" Then
                strsql &= " '" & Ref1.Trim.Replace("'", "''") & "', "
            Else
                strsql &= " null,"
            End If
            If Ref2.Trim <> "" Then
                strsql &= " '" & Ref2.Trim.Replace("'", "''") & "', "
            Else
                strsql &= " null,"
            End If
            If Season.Trim <> "" Then
                strsql &= " '" & Season.Trim.Replace("'", "''") & "', "
            Else
                strsql &= " null,"
            End If
            If Brand.Trim <> "" Then
                strsql &= " '" & Brand.Trim.Replace("'", "''") & "', "
            Else
                strsql &= " null,"
            End If
            If Grid.Trim <> "" Then
                strsql &= " '" & Grid.Trim.Replace("'", "''") & "', "
            Else
                strsql &= " null,"
            End If
            If Gender.Trim <> "" Then
                strsql &= " '" & Gender.Trim.Replace("'", "''") & "', "
            Else
                strsql &= " null,"
            End If
            If Styl.Trim <> "" Then
                strsql &= " '" & Styl.Trim.Replace("'", "''") & "', "
            Else
                strsql &= " null,"
            End If
            'If ChkFollowFid.Checked = True Then
            '    strsql &= " 'Y',"
            'Else
            strsql &= " 'N',"
            'End If
            'If ChkFranchSale.Checked = True Then
            '    strsql &= " 'Y',"
            'Else
            strsql &= " 'N',"
            'End If
            'If ChkOutsideTrade.Checked = True Then
            '    strsql &= " 'Y',"
            'Else
            strsql &= " 'N',"
            'End If
            ' If rbCurrent.Checked = True Then
            strsql &= " 'C',"
            'Else
            '    strsql &= " 'A',"
            'End If
            'If TxtActivityTill.Value Is DBNull.Value Then
            strsql &= " null,"
            ' Else
            ' strsql &= " '" & CType(TxtActivityTill.Text, Date).ToString("MM/dd/yyyy") & "',"
            ' End If
            'If ChkPOPI.Checked = True Then
            '    strsql &= " 'Y',"
            'Else
            strsql &= " 'N',"
            'End If
            'If ChkSales.Checked = True Then
            '    strsql &= " 'Y',"
            'Else
            strsql &= " 'N',"
            'End If
            'If ChkTransfer.Checked = True Then
            '    strsql &= " 'Y',"
            'Else
            strsql &= " 'N',"
            'End If
            ' If ChkAllowReturn.Checked = True Then
            strsql &= " 'Y',"
            'Else
            '    strsql &= " 'N',"
            'End If
            strsql &= " getdate() , 'n', null , 'n' , 'y', '" & VATStat & "' "
            strsql &= " , '" & UNITOPERATOR & "'"
            strsql &= " , 'N' " '  Cash Van
            strsql &= ", NULL " 'STK_RecalculateLC
            strsql &= " )"

            'ch @ 12/11/2017 wf: 4867
            If techSheet <> "" Then
                strsql &= " ; insert into STK04_DSTechnical VALUES  (   @RecId  , N'" & techSheet.Trim.Replace("'", "''") & "' ,  null,  null , NULL , GETDATE() ,  '" & gbUser & "' , NULL , null ,  null ,  null , null ,  NULL ,  NULL ,  NULL , NULL )"
            End If

            If createInLoc = True Then
                strsql &= " ; insert into stk04_stkqty SELECT @RecId ,'" & txtLoc.Text.Trim.Replace("'", "''") & "','0', GETDATE(), "
                strsql &= " 'O','0','0',GETDATE(),'" & gbUser & "',"
                If ItmClass = "T" Or ItmClass = "B" Then
                    strsql &= " ( SELECT isnull(MAX(STQ_RecId),0) FROM stk04_stkqty) +  ROW_NUMBER() OVER (ORDER BY new_old) ,"
                    strsql &= " NULL,NULL,NULL,NULL,NULL"
                ElseIf ItmClass = "F" Then
                    strsql &= " (SELECT isnull(MAX(STQ_RecId),0) FROM stk04_stkqty) + ROW_NUMBER() OVER "
                    strsql &= " (ORDER BY Color,GD_code,GD_SizeDesc) ,"
                    strsql &= " Color,GD_code,GD_SizeDesc,NULL,NULL "
                ElseIf ItmClass = "E" Then
                    strsql &= " (SELECT isnull(MAX(STQ_RecId),0) FROM stk04_stkqty) + ROW_NUMBER() OVER "
                    strsql &= " (ORDER BY DOB_ExpiryDate,DOB_BatchInfo) ,"
                    strsql &= " NULL,NULL,NULL,DOB_ExpiryDate,DOB_BatchInfo"
                End If
                'Rita M @ 18.02.2014
                'Transfer Pending Out,Transfer Pending In
                strsql &= ",0,0"
                'Rita M @ 21.11.2016. Allow return sales
                strsql &= " ,null"
                'Rita M @ 21.11.2016. Processing in Production
                strsql &= " ,0,NULL"
                'strsql &= " FROM dbo.STK04_Location "
                If ItmClass = "F" Then
                    strsql &= " FROM (select DISTINCT Color,GD_code,GD_SizeDesc,new_old "
                    strsql &= " FROM  " & TmpGridDetails
                    strsql &= " INNER JOIN stk04_DetGRID ON GD_code='" & Grid & "') ColorSize  where ColorSize.new_old='w'"
                Else
                    strsql &= " FROM (SELECT DISTINCT DOB_ExpiryDate,DOB_BatchInfo,new_old "
                    strsql &= " FROM " & TmpGridDetails & " t1 "
                    strsql &= " LEFT JOIN " & TmpGridExpiry & " expiry on expiry.RECID=t1.RECID and expiry.DOB_BatchInfo<>'' AND expiry.exp_stat<>'D' "
                    If PendRecid <> "" Then strsql &= "    WHERE MOD_RMRecid = " & PendRecid & ""
                    strsql &= " )  Expiry  where Expiry.new_old='w'  "
                End If
            End If

            strsql &= " ; insert into stk04_stkqty SELECT @RecId ,'00','0', GETDATE(), "
            strsql &= " 'O','0','0',GETDATE(),'" & gbUser & "',"
            If ItmClass = "T" Or ItmClass = "B" Then
                strsql &= " ( SELECT isnull(MAX(STQ_RecId),0) FROM stk04_stkqty) +  ROW_NUMBER() OVER (ORDER BY new_old) ,"
                strsql &= " NULL,NULL,NULL,NULL,NULL"
            ElseIf ItmClass = "F" Then
                strsql &= " (SELECT isnull(MAX(STQ_RecId),0) FROM stk04_stkqty) + ROW_NUMBER() OVER "
                strsql &= " (ORDER BY Color,GD_code,GD_SizeDesc) ,"
                strsql &= " Color,GD_code,GD_SizeDesc,NULL,NULL "
            ElseIf ItmClass = "E" Then
                strsql &= " (SELECT isnull(MAX(STQ_RecId),0) FROM stk04_stkqty) + ROW_NUMBER() OVER "
                strsql &= " (ORDER BY DOB_ExpiryDate,DOB_BatchInfo) ,"
                strsql &= " NULL,NULL,NULL,DOB_ExpiryDate,DOB_BatchInfo"
            End If
            'Rita M @ 18.02.2014
            'Transfer Pending Out,Transfer Pending In
            strsql &= ",0,0"
            'Rita M @ 21.11.2016. Allow return sales
            If ItmClass = "E" Then
                strsql &= " ,(SELECT STK_ReturnSale FROM stk04_stkfil WHERE stk_recid=@RecId) "
            Else
                strsql &= " ,null"
            End If
            'Rita M @ 21.11.2016. Processing in Production
            strsql &= " ,0"
            'strsql &= " FROM dbo.STK04_Location "
            If ItmClass = "F" Then
                strsql &= " , null "
                strsql &= " FROM (select DISTINCT Color,GD_code,GD_SizeDesc,new_old "
                strsql &= " FROM  " & TmpGridDetails
                strsql &= " INNER JOIN stk04_DetGRID ON GD_code='" & Grid & "') ColorSize  where ColorSize.new_old='w'"
            Else
                strsql &= " , DOQ_ProductionDt "
                strsql &= " FROM (SELECT DISTINCT DOB_ExpiryDate,DOB_BatchInfo,DOQ_ProductionDt,new_old "
                strsql &= " FROM " & TmpGridDetails & " t2 "
                strsql &= " LEFT JOIN " & TmpGridExpiry & " expiry on expiry.RECID=t2.RECID and expiry.DOB_BatchInfo<>'' AND expiry.exp_stat<>'D' "
                If PendRecid <> "" Then strsql &= "    WHERE MOD_RMRecid = " & PendRecid & ""
                strsql &= " )  Expiry  where Expiry.new_old='w' "
            End If

            cmSql.CommandText = strsql
            cmSql.ExecuteNonQuery()

            ''''''''''''''''''''''''''''''''''''''''''''''''''''''''''''''''''''''''''''''''''''''''''''''''''''''''''''''''''''''''''''''''''



        Catch ex As Exception
            Throw ex
        End Try
    End Sub

    Private Sub Clock_Tick(ByVal sender As System.Object, ByVal e As System.EventArgs) Handles Clock.Tick
        Try
            stBar.Panels(3).Text = Now.ToLongTimeString
        Catch ex As Exception
            MessageBox.Show("Error : " & ex.Message, cntTitle, MessageBoxButtons.OK, MessageBoxIcon.Error)
        End Try
    End Sub



    Private Function TestStkFil() As Boolean
        Dim sCtrl As Control
        Dim Str, StockKey1, StockKey2 As String
        Dim StrInserted As String
        Dim DRS As SqlDataReader
        Dim InsertPendItem As Boolean = False
        Try
            TestStkFil = True
            StockKey1 = ""
            For i = 0 To CurrItmKeyPend.Length - 1
                If UCase(CurrItmKeyPend(i)) = "SFP_SERIE" Then
                    StockKey1 &= " convert(varchar,isnull(stk1." & CurrItmKeyPend(i) & ",0)) + "
                Else
                    StockKey1 &= " convert(varchar,isnull(stk1." & CurrItmKeyPend(i) & ",'')) + "
                End If
            Next
            StockKey1 = StockKey1.Substring(0, Len(StockKey1) - 2)

            StockKey2 = ""
            For i = 0 To CurrItmKeyPend.Length - 1
                If UCase(CurrItmKeyPend(i)) = "SFP_SERIE" Then
                    StockKey2 &= " convert(varchar,isnull(stk2." & CurrItmKeyPend(i) & ",0)) + "
                Else
                    StockKey2 &= " convert(varchar,isnull(stk2." & CurrItmKeyPend(i) & ",'')) + "
                End If
            Next
            StockKey2 = StockKey2.Substring(0, Len(StockKey2) - 2)

            'Test if the item was added in stk04_stkfil
            Str = " Select * from " & TmpGridDetails & " det"
            Str &= " INNER JOIN STK04_STKFIL ON " '(STK04_STKFIL.STK_REF= det.stk_ref or STK04_STKFIL.STK_REF= " & gbComp & "/ + det.stk_ref) "
            Str &= " STK04_STKFIL.STK_REF= det.stk_ref where new_old='w' "

            ReadFromDBToDataReader(Str, DRS)
            If DRS.HasRows Then
                DRS.Read()

                'test if item was added with different item classification
                strsql = Str
                strsql &= " and STK04_STKFIL.stk_ItmClass <> det.ItmClass "
                Dim drs2 As SqlDataReader
                ReadFromDBToDataReader(strsql, drs2)
                If drs2.HasRows Then
                    drs2.Read()

                    MessageBox.Show("The Item '" & DRS.Item("Stk_desc").ToString & "' Was Created With Different Item Classification.", cntTitle, MessageBoxButtons.OK, MessageBoxIcon.Exclamation)
                    TestStkFil = False
                    Exit Try
                Else
                    If DelOrder = "Receiving Order" Or DelOrder = "Consignment Receiving Order" Then
                        'getting the recid of the item from stock file
                        strsql = " Update " & TmpGridDetails & " set new_old='n' , MOD_RMRecid=" & DRS.Item("STK_RecID").ToString
                        strsql &= " where recid=" & DRS.Item("RecID").ToString
                        Execute(strsql)
                    Else
                        'Test in case of fashion if different season and brand
                        Str &= " AND (ISNULL(STK_SEASON,'')<>'" & txtSeason.Text.Trim & "' or ISNULL(STK_BRAND,'')<>'" & txtBrand.Text.Trim & "') "
                        Dim drs1 As SqlDataReader
                        ReadFromDBToDataReader(Str, drs1)
                        If drs1.HasRows Then
                            drs1.Read()
                            MessageBox.Show("The Item '" & DRS.Item("Stk_desc").ToString & "' Was Created With Different Season Or Brand.", cntTitle, MessageBoxButtons.OK, MessageBoxIcon.Exclamation)
                            TestStkFil = False
                            Exit Try
                        Else
                            'getting the recid of the item from stock file
                            strsql = " Update " & TmpGridDetails & " set new_old='n' , MOD_RMRecid=" & DRS.Item("stk_RecID").ToString
                            strsql &= " where recid=" & DRS.Item("RecID").ToString
                            Execute(strsql)
                        End If
                    End If
                End If
            Else


                'test if the item was added in pending stock
                Str = " Select stk2.SFP_Recid ,RecID,stk2.SFP_Desc,MOD_RMRecid,SFP_OldRecid from " & TmpGridDetails
                Str &= " LEFT JOIN " & TempPendingDO & " STK1  ON  SFP_RECID=MOD_RMRecid "
                Str &= " LEFT JOIN STK04_STKFILpending stk2 ON " & StockKey2 & " =" & StockKey1
                Str &= " where new_old='w' AND stk2.SFP_Desc IS not NULL  AND stk2.SFP_Desc <>''"
                ReadFromDBToDataReader(Str, DRS)
                If DRS.HasRows Then
                    DRS.Read()

                    'test if item was added with different item classification
                    strsql = Str
                    strsql &= " and  stk2.SFP_ItmClass <> ItmClass  "
                    Dim drs3 As SqlDataReader
                    ReadFromDBToDataReader(strsql, drs3)
                    If drs3.HasRows Then
                        drs3.Read()

                        MessageBox.Show("The Item " & DRS.Item("SFP_desc").ToString & " Was Created In Pending File With Different Item Classification.", cntTitle, MessageBoxButtons.OK, MessageBoxIcon.Exclamation)
                        TestStkFil = False
                        Exit Try
                    Else
                        If DelOrder = "Receiving Order" Or DelOrder = "Consignment Receiving Order" Then
                            'getting the recid of the item from temp stock file 
                            strsql = " Update " & TempPendingDO
                            strsql &= " set SFP_Status='o' , SFP_OldRecid =" & DRS.Item("SFP_Recid").ToString
                            strsql &= " where  SFP_Recid=" & DRS.Item("MOD_RMRecid").ToString
                            Execute(strsql)
                            Exit Function
                        Else
                            'Test in case of fashion if different season and brand
                            Str &= " AND (ISNULL(stk2.Sfp_SEASON,'')<>'" & txtSeason.Text.Trim & "' or ISNULL(stk2.Sfp_BRAND,'')<>'" & txtBrand.Text.Trim & "') "
                            Dim drs2 As SqlDataReader
                            ReadFromDBToDataReader(Str, drs2)
                            If drs2.HasRows Then
                                drs2.Read()
                                MessageBox.Show("The Item '" & DRS.Item("SFP_desc").ToString & "' Was Created In Pending File With Different Season Or Brand.", cntTitle, MessageBoxButtons.OK, MessageBoxIcon.Exclamation)
                                TestStkFil = False
                                Exit Try
                            Else
                                'getting the recid of the item from Temp stock file
                                strsql = " Update " & TempPendingDO
                                strsql &= " set SFP_Status='o' , SFP_OldRecid =" & DRS.Item("SFP_Recid").ToString
                                strsql &= " where  SFP_Recid=" & DRS.Item("MOD_RMRecid").ToString
                                Execute(strsql)

                            End If
                        End If
                    End If
                End If

            End If

        Catch ex As Exception
            Throw ex
        End Try
    End Function
    Private Sub btnSave_Click(ByVal sender As System.Object, ByVal e As System.EventArgs) Handles btnSave.Click
        Dim drsql As SqlDataReader
        Dim fcsCtr As Control
        Dim cnSql As SqlConnection


        Dim cmSql As SqlCommand
        Dim cmsql2 As SqlCommand
        Dim cmsql3 As SqlCommand
        Dim trnSql As SqlTransaction
        Dim drSqlTrans As SqlDataReader
        Dim drsqlL As SqlDataReader

        Dim NewItemRecId As Integer
        Try
            save = False
            UPDATEGRID()
            daDetDetails.Update(dsDetDetails.Tables("tblDetails"))
            'ch @ 27/05/2019
            If PreventCreatItemWOBar AndAlso RecordsExists(" select top 1 1 from " & TmpGridDetails & " Grid inner join " & TempPendingDO & " ON SFP_RECID=MOD_RMRecid   " & _
                                                           "  WHERE new_old='w' and  NOT EXISTS (SELECT TOP 1 1 FROM  " & TmpGridBarcode & " Barcode  WHERE Barcode.recid = Grid.recid )  ") Then
                ShowNotification(Me, "You Should Define At Least One Barcode For Each Pending Item Created", 2000, 14, eToastPosition.MiddleCenter)
                Exit Try
            End If
            If DelOrder = "Receiving Order" Or DelOrder = "Consignment Receiving Order" Then
                LstExpiry.UpdateData()
                daDetExpiry.Update(dsDetExpiry.Tables("tblDetails"))
                LstBarcode.UpdateData()
                daDetBarcode.Update(dsDetBarcode.Tables("tblDetails"))
            Else
                If Not daDetBarcodeF Is Nothing Then LstBarcodeF.UpdateData() : daDetBarcodeF.Update(dsDetBarcodeF.Tables("tblDetails"))
                If Not daDetFqty Is Nothing Then lstFashQty.UpdateData() : daDetFqty.Update(dsDetFqty.Tables("tblDetails"))
            End If

            If DelOrder = "Receiving Order Fashion" Or DelOrder = "Consignment Receiving Order Fashion" Then
                If txtSeason.Text.Trim = "" Then
                    ErrorProvider1.SetError(txtSeason, "You Should Choose A Season ")
                    fcsCtr = txtSeason
                End If
                If txtBrand.Text.Trim = "" Then
                    ErrorProvider1.SetError(txtBrand, "You Should Choose A Brand ")
                    fcsCtr = txtBrand
                End If
            End If
            If txtActualDate.Value Is DBNull.Value Then
                ErrorProvider1.SetError(txtActualDate, "Actual Date Can Not Be Null ")
                fcsCtr = txtActualDate
            End If
            If txtLoc.Text.Trim = "" Then
                ErrorProvider1.SetError(txtLoc, "You Should Choose Receiving Location ")
                fcsCtr = txtLoc
            End If
            If DelOrder = "Consignment Receiving Order" Or DelOrder = "Consignment Receiving Order Fashion" Then
                If txtCurr.Text.Trim = "" Then
                    ErrorProvider1.SetError(txtCurr, "You Should Choose A Currency ")
                    fcsCtr = txtCurr
                End If
            End If

            If txtDate.Value Is DBNull.Value Then
                ErrorProvider1.SetError(txtDate, "Date Can Not Be Null ")
                fcsCtr = txtDate
            End If
            If txtDelOrder.Text.Trim = "" Then
                ErrorProvider1.SetError(txtDelOrder, "You Should Put A Del. Order # ")
                fcsCtr = txtDelOrder
            End If
            If txtSupp.Text.Trim = "" Then
                ErrorProvider1.SetError(txtSupp, "You Should Choose A Supplier ")
                fcsCtr = txtSupp
            End If
            If Wtype <> "ADD" Then
                If txtCode.Text.Trim = "" Then
                    ErrorProvider1.SetError(txtCode, "Code Can Not Be Null ")
                    fcsCtr = txtCode
                End If
            End If



            If Not fcsCtr Is Nothing Then
                MessageBox.Show("Missing Data.", cntTitle, MessageBoxButtons.OK, MessageBoxIcon.Question)
                fcsCtr.Focus()
                Exit Try
            End If
            If dsDetDetails.Tables("tblDetails").Select("").Length() = 0 Then
                MessageBox.Show("You should add items to the " & DelOrder, cntTitle, MessageBoxButtons.OK, MessageBoxIcon.Question)
                lstDelivery.Focus()
                Exit Sub
            End If
            If GrpExpBarcode.Visible = True AndAlso GrpExpBarcode.Visible = True Then
                MessageBox.Show("Barcode/Expiry Details Is opened , Please Close It To Continue", cntTitle, MessageBoxButtons.OK, MessageBoxIcon.Information)
                Exit Try
            End If
          
            Cursor.Current = Cursors.WaitCursor
            'Verify data in the grid
            If VerifyGridData() = False Then Exit Sub

            btnSave.Enabled = False
            If Wtype = "DELETE" Then OldCode = txtCode.Text.Trim
            DropSqlObject("Tmp_" & TmpGridDetails, "TABLE")
            strsql = " Create table Tmp_" & TmpGridDetails
            strsql &= " (Tmp_RECID numeric(18,0) NOT NULL ,Tmp_PendingID numeric(18,0) NULL ,Tmp_StkID numeric(18,0) NULL ,"
            strsql &= " CONSTRAINT PK_Tmp_" & TmpGridDetails & " PRIMARY KEY  (Tmp_RECID) "
            strsql &= "  ) ON [PRIMARY]"
            Execute(strsql)

            cnSql = New SqlConnection(ConnectionString & ";MultipleActiveResultSets=True;Asynchronous Processing=true")
            cnSql.Open()
            trnSql = cnSql.BeginTransaction
            cmSql = New SqlCommand(strsql, cnSql, trnSql)

            'test on stk04_stkfil and pending stk fil if someone answered same item.
            If TestStkFil() = False Then Exit Sub


            strsql = " SELECT  distinct   MOD_RMRecid,SFP_CATEGORY, SFP_ItmIdent, SFP_Brand , SFP_Season,"
            strsql &= " SFP_Usage , SFP_NEWUSED, SFP_ItmType, SFP_LOCSRC , SFP_InfDate, SFP_ImpLoc ,"
            strsql &= " SFP_SerReg, SFP_UNITPACK, SFP_SUBUNIT, SFP_FCCode, SFP_VATType, SFP_VATPER,"
            strsql &= " SFP_Desc, SFP_StartTaxDate, SFP_SingleRef1,  SFP_SingleRef2, SFP_Grid, SFP_Gender,"
            strsql &= " SFP_Style, SFP_UNIT,sfp_ItmClass ItmClass ,MOD_RMRecid,SFP_OldRecid,SFP_VATSTAT,SFP_TechSheet ,SFP_OPERATOR  "
            'ch @ 17/11/2016 wf : 4618  
            strsql &= ", SFP_serie"
            strsql &= " FROM " & TmpGridDetails
            strsql &= " Left Join " & TempPendingDO & " ON  SFP_RECID=MOD_RMRecid "
            strsql &= " where new_old='w'"

            ReadFromDBToDataReader(strsql, drsql)
            If drsql.HasRows Then
                i = 1
                While drsql.Read


                    If DelOrder = "Consignment Receiving Order" Or DelOrder = "Consignment Receiving Order Fashion" Then
                        InsertStkFil(cmSql, drsql.Item("SFP_CATEGORY").ToString, drsql.Item("SFP_ItmIdent").ToString, drsql.Item("SFP_Usage").ToString, "C", drsql.Item("SFP_ItmType").ToString, _
                                    drsql.Item("SFP_LOCSRC").ToString, drsql.Item("SFP_InfDate").ToString, drsql.Item("SFP_ImpLoc").ToString, drsql.Item("SFP_SerReg").ToString, _
                                    drsql.Item("SFP_UNITPACK").ToString, drsql.Item("SFP_SUBUNIT").ToString, drsql.Item("SFP_FCCode").ToString, drsql.Item("SFP_VATType").ToString, drsql.Item("SFP_VATPER").ToString, _
                                    drsql.Item("SFP_StartTaxDate").ToString, drsql.Item("SFP_SingleRef1").ToString, drsql.Item("SFP_SingleRef2").ToString, drsql.Item("SFP_Grid").ToString, drsql.Item("ItmClass").ToString, _
                                    drsql.Item("SFP_Gender").ToString, drsql.Item("SFP_Style").ToString, drsql.Item("SFP_UNIT").ToString, drsql.Item("SFP_Desc").ToString, _
                                    drsql.Item("SFP_Brand").ToString, drsql.Item("SFP_Season").ToString, drsql.Item("SFP_VATSTAT").ToString, drsql.Item("SFP_serie").ToString, , drsql.Item("SFP_TechSheet").ToString, drsql.Item("SFP_OPERATOR").ToString, drsql.Item("MOD_RMRecid").ToString)

                        NewItemRecId = CInt(GetFieldDesc("stk04_stkfil", "1=1", "max(stk_recid)", cmSql))

                        InsertStkFil(cmSql, drsql.Item("SFP_CATEGORY").ToString, drsql.Item("SFP_ItmIdent").ToString, drsql.Item("SFP_Usage").ToString, "N", drsql.Item("SFP_ItmType").ToString, _
                                    drsql.Item("SFP_LOCSRC").ToString, drsql.Item("SFP_InfDate").ToString, drsql.Item("SFP_ImpLoc").ToString, drsql.Item("SFP_SerReg").ToString, _
                                    drsql.Item("SFP_UNITPACK").ToString, drsql.Item("SFP_SUBUNIT").ToString, drsql.Item("SFP_FCCode").ToString, drsql.Item("SFP_VATType").ToString, drsql.Item("SFP_VATPER").ToString, _
                                    drsql.Item("SFP_StartTaxDate").ToString, drsql.Item("SFP_SingleRef1").ToString, drsql.Item("SFP_SingleRef2").ToString, drsql.Item("SFP_Grid").ToString, drsql.Item("ItmClass").ToString, _
                                    drsql.Item("SFP_Gender").ToString, drsql.Item("SFP_Style").ToString, drsql.Item("SFP_UNIT").ToString, drsql.Item("SFP_Desc").ToString, _
                                    drsql.Item("SFP_Brand").ToString, drsql.Item("SFP_Season").ToString, drsql.Item("SFP_VATSTAT").ToString, drsql.Item("SFP_serie").ToString, False, drsql.Item("SFP_TechSheet").ToString, drsql.Item("SFP_OPERATOR").ToString, drsql.Item("MOD_RMRecid").ToString)
                    Else

                        InsertStkFil(cmSql, drsql.Item("SFP_CATEGORY").ToString, drsql.Item("SFP_ItmIdent").ToString, drsql.Item("SFP_Usage").ToString, drsql.Item("SFP_NEWUSED").ToString, drsql.Item("SFP_ItmType").ToString, _
                                        drsql.Item("SFP_LOCSRC").ToString, drsql.Item("SFP_InfDate").ToString, drsql.Item("SFP_ImpLoc").ToString, drsql.Item("SFP_SerReg").ToString, _
                                        drsql.Item("SFP_UNITPACK").ToString, drsql.Item("SFP_SUBUNIT").ToString, drsql.Item("SFP_FCCode").ToString, drsql.Item("SFP_VATType").ToString, drsql.Item("SFP_VATPER").ToString, _
                                        drsql.Item("SFP_StartTaxDate").ToString, drsql.Item("SFP_SingleRef1").ToString, drsql.Item("SFP_SingleRef2").ToString, drsql.Item("SFP_Grid").ToString, drsql.Item("ItmClass").ToString, _
                                        drsql.Item("SFP_Gender").ToString, drsql.Item("SFP_Style").ToString, drsql.Item("SFP_UNIT").ToString, drsql.Item("SFP_Desc").ToString, _
                                        drsql.Item("SFP_Brand").ToString, drsql.Item("SFP_Season").ToString, drsql.Item("SFP_VATSTAT").ToString, drsql.Item("SFP_serie").ToString, , drsql.Item("SFP_TechSheet").ToString, drsql.Item("SFP_OPERATOR").ToString, drsql.Item("MOD_RMRecid").ToString)

                        NewItemRecId = CInt(GetFieldDesc("stk04_stkfil", "1=1", "max(stk_recid)", cmSql))
                    End If
                    If NewItemRecId <> 0 Then ' And CDbl(drsql.Item("SFP_OldRecid").ToString) <> 0 
                        strsql = "UPDATE Stk04_MOrdDet SET MOD_RMRecid='" & NewItemRecId & "',MOD_PendRecid=null WHERE MOD_PendRecid='" & drsql.Item("SFP_OldRecid").ToString & "'"
                        cmSql.CommandText = strsql
                        cmSql.ExecuteNonQuery()

                        strsql = "UPDATE STK04_InvDET SET INVD_STKRECID='" & NewItemRecId & "',INVD_PendRecId=null WHERE INVD_PendRecId='" & drsql.Item("SFP_OldRecid").ToString & "'"
                        cmSql.CommandText = strsql
                        cmSql.ExecuteNonQuery()

                        strsql = "UPDATE STK04_PoTemplateDet SET POTD_StkRecid='" & NewItemRecId & "',POTD_PendRecid=null WHERE POTD_PendRecid='" & drsql.Item("SFP_OldRecid").ToString & "'"
                        cmSql.CommandText = strsql
                        cmSql.ExecuteNonQuery()
                        'ch @ 10/01/2017 wf: 4867
                        strsql = "UPDATE tbl_RequisitionDetails SET ReqD_StkId='" & NewItemRecId & "',ReqD_PendRecId=null WHERE ReqD_PendRecId='" & drsql.Item("SFP_OldRecid").ToString & "'"
                        cmSql.CommandText = strsql
                        cmSql.ExecuteNonQuery()
                        'ch @ 25/01/2017 wf: 4867
                        strsql = "UPDATE tbl_ReqItmApproval SET App_Stkid='" & NewItemRecId & "',App_PenStkId=null WHERE App_PenStkId='" & drsql.Item("SFP_OldRecid").ToString & "'"
                        cmSql.CommandText = strsql
                        cmSql.ExecuteNonQuery()
                        strsql = "delete FROM dbo.STK04_StkFilPending WHERE SFP_Recid='" & drsql.Item("SFP_OldRecid").ToString & "'"
                        cmSql.CommandText = strsql
                        cmSql.ExecuteNonQuery()


                    End If
                    strsql = "Insert into  Tmp_" & TmpGridDetails & " Values ('" & i & "','" & drsql.Item("MOD_RMRecid").ToString & "','" & NewItemRecId & "')"
                    Execute(strsql)
                    i = i + 1
                End While
            End If



            'Update Counter
            strsql = "update " & TmpGridDetails & " SET mod_count= counter "
            strsql &= " FROM (SELECT ROW_NUMBER() OVER (PARTITION BY mod_supitmref, LEFT(mod_stat,1) ORDER BY recid) counter,Recid recid2 "
            strsql &= " FROM " & TmpGridDetails & ") test WHERE recid2=recid"
            Execute(strsql)



            Dim recid As String

            ReadFromDBToDataReader("select sup_Recid from  stk04_supplier where sup_code ='" & txtSupp.Text.Trim & "' ", drsql)
            If drsql.HasRows Then
                drsql.Read()

                '..Insert header
                If Wtype = "ADD" Or Wtype = "DELETE" Then

                    recid = GetMaxRecId("Stk04_PurDelOrdH", "DOH_Recid")
                    txtCode.Text = recid
                    strsql = "Insert into  Stk04_PurDelOrdH values(" & recid & "," & IIf(txtOrderNo.Text.Trim = "", "Null", Val(txtOrderNo.Text.Trim.Replace("'", "''"))).ToString & "," & drsql.Item("sup_Recid").ToString & ","
                    strsql &= " '" & txtDelOrder.Text.Trim.Replace("'", "''") & "','" & Format(txtDate.Value, "MM/dd/yyyy") & "',"
                    If Wtype <> "DELETE" Then
                        strsql &= "'N',"
                    Else
                        strsql &= "'R',"
                    End If
                    strsql &= " '" & txtLoc.Text.Trim & "','" & Format(txtActualDate.Value, "MM/dd/yyyy") & "',"
                    strsql &= " Null , GETDATE(),'" & gbUser & "',"
                    If DelOrder = "Receiving Order" Or DelOrder = "Consignment Receiving Order" Then
                        strsql &= " 'T', null,null,"
                    ElseIf DelOrder = "Receiving Order Fashion" Or DelOrder = "Consignment Receiving Order Fashion" Then
                        strsql &= " 'F','" & txtBrand.Text.Trim & "','" & txtSeason.Text.Trim & "',"
                    End If
                    strsql &= " GETDATE(),'" & gbUser & "',"
                    If DelOrder = "Consignment Receiving Order" Or DelOrder = "Consignment Receiving Order Fashion" Then
                        strsql &= "'" & txtCurr.Text.Trim.Replace("'", "''") & "' )"
                    Else
                        strsql &= " Null )"
                    End If

                    cmSql.CommandText = strsql
                    cmSql.ExecuteNonQuery()
                End If
            End If

            If Wtype = "ADD" Or Wtype = "DELETE" Then
                ' Update the REcID To be USe in Other Temp Table fo
                strsql = " UPDATE  " & TmpGridDetails & ""
                strsql &= " SET     DOB_Drecid = NEwDOB_Drecid"
                strsql &= " FROM    ( SELECT    RECID oldRecid ,"
                strsql &= " ROW_NUMBER() OVER ( ORDER BY MOD_supitmref )"
                strsql &= " + ( SELECT  ISNULL(MAX(DOD_Recid), 0)"
                strsql &= " FROM    Stk04_PurDelOrdD"
                strsql &= " ) AS NEwDOB_Drecid"
                strsql &= " FROM      " & TmpGridDetails & ""
                strsql &= " ) t"
                strsql &= " WHERE   oldRecid = Recid"
                cmSql.CommandText = strsql
                cmSql.ExecuteNonQuery()
                ' QC Head Table Updated
                strsql = " UPDATE " & TempQcHeader & " SET"
                strsql &= " PrDReciD =  DOB_Drecid"
                strsql &= " FROM " & TmpGridDetails & " Details WHERE"
                strsql &= " PrDReciD = Details.RECID"
                cmSql.CommandText = strsql
                cmSql.ExecuteNonQuery()

                strsql = " UPDATE  " & TempQcHeader & ""
                strsql &= " SET     MyNewReciD = NEwDOB_Drecid"
                strsql &= " FROM    ( SELECT    ReciD oldRecid ,"
                strsql &= " ROW_NUMBER() OVER ( ORDER BY PrDReciD )"
                strsql &= " + ( SELECT  ISNULL(MAX(PurQCH_ReciD), 0)"
                strsql &= " FROM    Stk04_PurDelOrdDQCH"
                strsql &= " ) AS NEwDOB_Drecid"
                strsql &= " FROM      " & TempQcHeader & ""
                strsql &= " ) t"
                strsql &= " WHERE   oldRecid = Recid"
                cmSql.CommandText = strsql
                cmSql.ExecuteNonQuery()
                strsql = " UPDATE " & TempQcDetails & " SET"
                strsql &= " HRecID =  MyNewReciD"
                strsql &= " FROM " & TempQcHeader & " header WHERE"
                strsql &= " HRecID = header.RECID"
                cmSql.CommandText = strsql
                cmSql.ExecuteNonQuery()

                If DelOrder.Contains("Fashion") = False Then
                    strsql = " UPDATE " & TmpGridExpiry & ""
                    strsql &= " SET    NewRECID = NEwDOB_Drecid"
                    strsql &= " FROM   ( SELECT    E_RECID oldRecid ,"
                    strsql &= " ROW_NUMBER() OVER ( ORDER BY expirygrid.RECID )"
                    strsql &= " + ( SELECT  ISNULL(MAX(DOQ_RecID), 0)"
                    strsql &= " FROM    dbo.stk04_PurDelOrdDQty"
                    strsql &= " ) AS NEwDOB_Drecid"
                    strsql &= " FROM      " & TmpGridExpiry & " expirygrid"
                    strsql &= " INNER JOIN " & TmpGridDetails & " detailgrid"
                    strsql &= " ON detailgrid.RECID = expirygrid.RECID"
                    strsql &= " "
                    strsql &= " ) t"
                    strsql &= " WHERE  oldRecid = E_RECID"
                    cmSql.CommandText = strsql
                    cmSql.ExecuteNonQuery()

                    strsql = " UPDATE " & TempQcHeader & " SET"
                    strsql &= " PrQDRecId =  NewRECID"
                    strsql &= " FROM " & TmpGridExpiry & " Details WHERE"
                    strsql &= " PrQDRecId = Details.E_RECID"
                    cmSql.CommandText = strsql
                    cmSql.ExecuteNonQuery()
                End If


                '..Insert Details
                strsql = "Insert into  Stk04_PurDelOrdD Select  ROW_NUMBER() OVER ( ORDER BY recid, MOD_supitmref ) + (SELECT  ISNULL(MAX(DOD_Recid), 0) FROM Stk04_PurDelOrdD)  ," & recid & ",  REPLACE(REPLACE(RTRIM(LTRIM(MOD_SupItmRef)), CHAR(13), ''), CHAR(10), '') MOD_supitmref,"
                strsql &= " LEFT(MOD_stat,1),"
                strsql &= " MOD_Count, case when new_old='w'THEN Tmp_StkID else MOD_RMRecid END, MOD_OrdQty ,isnull(stk04_stkfil.stk_unit,pend.sfp_unit), MOD_ConsOrdQty, "
                ' If DelOrder = "Consignment Receiving Order Fashion" Or DelOrder = "Consignment Receiving Order" Then
                If DelOrder = "Consignment Receiving Order Fashion" Or DelOrder = "Consignment Receiving Order" Then
                    strsql &= " dbo.[convert5]('" & gbApp & "','" & gbComp & "','" & gbYear & "',"
                    strsql &= " cost, Grid.STK_FCCODE,'" & gbCurr1 & "','" & CType(Now, Date).ToString("MM/dd/yyyy") & "') UnitCostLC,"
                    strsql &= " dbo.[convert5]('" & gbApp & "','" & gbComp & "','" & gbYear & "',"
                    strsql &= " cost,Grid.STK_FCCODE,'" & gbCurr2 & "','" & CType(Now, Date).ToString("MM/dd/yyyy") & "') UnitCostCC,"
                    strsql &= " (CASE WHEN Grid.STK_FCCODE<>'" & gbCurr1 & "' AND Grid.STK_FCCODE<>'" & gbCurr2 & "' "
                    strsql &= " THEN  cost else dbo.[convert5]('" & gbApp & "','" & gbComp & "','" & gbYear & "',"
                    strsql &= " cost,Grid.STK_FCCODE,'" & gbCurr2 & "','" & CType(Now, Date).ToString("MM/dd/yyyy") & "') END) UnitCostFC,"
                Else
                    strsql &= " dbo.[convert5]('" & gbApp & "','" & gbComp & "','" & gbYear & "',"
                    strsql &= " cost, '" & gbCurr2 & "','" & gbCurr1 & "','" & CType(Now, Date).ToString("MM/dd/yyyy") & "') UnitCostLC,"
                    strsql &= " cost  UnitCostCC,"
                    strsql &= "   dbo.[convert5]('" & gbApp & "','" & gbComp & "','" & gbYear & "',"
                    strsql &= " cost,'" & gbCurr2 & "',stk04_stkfil.STK_FCCODE,'" & CType(Now, Date).ToString("MM/dd/yyyy") & "')   UnitCostFC,"
                End If


                ' ElseIf DelOrder = "Receiving Order Fashion" Or DelOrder = "Receiving Order" Then
                '    strsql &= " 0, 0,0,"
                'End If
                strsql &= " CASE WHEN Invd_vat = 'Taxable' THEN 'T' WHEN Invd_vat = 'Not Taxable' then 'N' when  Invd_vat = 'Taxable Embeded' THEN 'E' else  LEFT( Invd_vat,1) end , "
                'strsql &= " LEFT( Invd_vat,1),"
                strsql &= " getdate(),'" & gbUser & "', "
                If DelOrder = "Receiving Order" Or DelOrder = "Consignment Receiving Order" Then
                    strsql &= " null, "
                ElseIf DelOrder = "Receiving Order Fashion" Or DelOrder = "Consignment Receiving Order Fashion" Then
                    strsql &= " grid.SuppColor, "
                End If
                strsql &= " getdate(),'" & gbUser & "', "
                If DelOrder = "Receiving Order" Or DelOrder = "Consignment Receiving Order" Then
                    strsql &= " null  "
                ElseIf DelOrder = "Receiving Order Fashion" Or DelOrder = "Consignment Receiving Order Fashion" Then
                    strsql &= " Grid.Color "
                End If
                If DelOrder = "Consignment Receiving Order Fashion" Or DelOrder = "Consignment Receiving Order" Then
                    strsql &= " , disc "
                    strsql &= " , dbo.[convert5]('" & gbApp & "','" & gbComp & "','" & gbYear & "',"
                    strsql &= " net,Grid.STK_FCCODE,'" & gbCurr1 & "','" & CType(Now, Date).ToString("MM/dd/yyyy") & "') netCostLC,"
                    strsql &= " dbo.[convert5]('" & gbApp & "','" & gbComp & "','" & gbYear & "',"
                    strsql &= " net,Grid.STK_FCCODE,'" & gbCurr2 & "','" & CType(Now, Date).ToString("MM/dd/yyyy") & "') netCostCC,"
                    strsql &= " (CASE WHEN Grid.STK_FCCODE<>'" & gbCurr1 & "' AND Grid.STK_FCCODE<>'" & gbCurr2 & "' "
                    strsql &= " THEN  net else dbo.[convert5]('" & gbApp & "','" & gbComp & "','" & gbYear & "',"
                    strsql &= " net,Grid.STK_FCCODE,'" & gbCurr2 & "','" & CType(Now, Date).ToString("MM/dd/yyyy") & "') END) NetCostFC "
                Else
                    strsql &= "  , Null "
                    strsql &= " , dbo.[convert5]('" & gbApp & "','" & gbComp & "','" & gbYear & "',"
                    strsql &= " (cost * MOD_OrdQty),'" & gbCurr2 & "','" & gbCurr1 & "','" & CType(Now, Date).ToString("MM/dd/yyyy") & "') netCostLC,"
                    strsql &= " (cost * MOD_OrdQty) netCostCC,"
                    strsql &= "  dbo.[convert5]('" & gbApp & "','" & gbComp & "','" & gbYear & "',"
                    strsql &= " (cost * MOD_OrdQty),'" & gbCurr2 & "',stk04_stkfil.STK_FCCODE,'" & CType(Now, Date).ToString("MM/dd/yyyy") & "')   NetCostFC "
                End If

                strsql &= " From " & TmpGridDetails & " Grid "
                strsql &= "    Left join stk04_stkfil on stk_recid = MOD_RMRecid "
                strsql &= " Left Join " & TempPendingDO & " pend ON  SFP_RECID=MOD_RMRecid "
                strsql &= " LEFT JOIN TMP_" & TmpGridDetails & " ON Tmp_PendingID= MOD_RMRecid where MOD_RMRecid IS NOT NULL "

                cmSql.CommandText = strsql
                cmSql.ExecuteNonQuery()


                If DelOrder = "Receiving Order" Or DelOrder = "Consignment Receiving Order" Then
                    '..insert details stk04_PurDelOrdDBE
                    strsql = "  INSERT  INTO stk04_PurDelOrdDBE"
                    strsql &= " SELECT  ROW_NUMBER() OVER ( ORDER BY DOD_RECID )+ ( SELECT  ISNULL(MAX(DOB_RecID), 0)FROM    stk04_PurDelOrdDBE) ,temp.* FROM ("
                    strsql &= " SELECT DOD_RECID , Barcode.DOB_Barcode ,Expiry.DOB_ExpiryDate ,Expiry.DOB_BatchInfo ,"
                    strsql &= " NULL grid,NULL color, GETDATE() Cdate,'" & gbUser & "' cuser,GETDATE() date,'" & gbUser & "' ouser"
                    strsql &= " FROM    " & TmpGridDetails & " Grid"
                    strsql &= " LEFT JOIN  " & TmpGridExpiry & " Expiry ON grid.recid = expiry.recid"
                    strsql &= " LEFT JOIN " & TmpGridBarcode & "  Barcode ON Barcode.recid = Grid.recid"
                    strsql &= " AND Barcode.e_recid = Expiry.e_recid"
                    strsql &= " LEFT JOIN stk04_PurDelOrdD ON DOD_ItmRef=grid.MOD_supitmref"
                    strsql &= " AND DOD_Stat=LEFT(grid.MOD_stat,1) AND DOD_Counter=grid.MOD_Count AND dod_hrecid=" & recid
                    strsql &= " left join stk04_stkfil on stk_recid=DOD_StkRecid "
                    strsql &= " WHERE    stk_itmclass='e' and  ISNULL (barcode.bar_stat,'') <>'D'AND  ISNULL (Expiry.exp_stat,'') <>'D' AND ISNULL (Expiry.exp_stat,'')<>''  AND   ISNULL (Barcode.DOB_Barcode,'')<>''"
                    strsql &= " UNION SELECT DOD_RECID ,Barcode.DOB_Barcode ,null ,null,NULL ,NULL ,"
                    strsql &= " GETDATE() cdate,'" & gbUser & "' cuser ,GETDATE() date,'" & gbUser & "' ouser "
                    strsql &= " FROM    " & TmpGridDetails & " Grid"
                    strsql &= " LEFT JOIN " & TmpGridBarcode & " Barcode ON Barcode.recid = Grid.recid"
                    strsql &= " LEFT JOIN stk04_PurDelOrdD ON DOD_ItmRef=grid.MOD_supitmref"
                    strsql &= " AND DOD_Stat=LEFT(grid.MOD_stat,1) AND DOD_Counter=grid.MOD_Count AND dod_hrecid=" & recid
                    strsql &= " left join stk04_stkfil on stk_recid=DOD_StkRecid  WHERE  barcode.bar_stat<>'D'AND   ISNULL (Barcode.DOB_Barcode,'')<>'' and (stk_itmclass='b' or stk_itmclass='t' )  ) temp "

                    cmSql.CommandText = strsql
                    cmSql.ExecuteNonQuery()

                    '..insert details stk04_PurDelOrdDQty
                    strsql = "  INSERT  INTO stk04_PurDelOrdDQty"
                    strsql &= " SELECT  ROW_NUMBER() OVER ( ORDER BY DOD_RECID )+ ( SELECT  ISNULL(MAX(DOQ_RecID), 0)FROM    stk04_PurDelOrdDQTY) ,temp.* FROM ("
                    strsql &= " SELECT DISTINCT DOD_RECID , Expiry.DOB_ExpiryDate ,Expiry.DOB_BatchInfo ,"
                    strsql &= " NULL grid,NULL color,Expiry.MOD_OrdQty ,GETDATE() Cdate,'" & gbUser & "' cuser,GETDATE() date,'" & gbUser & "' ouser"
                    strsql &= " FROM    " & TmpGridDetails & " Grid"
                    strsql &= " LEFT JOIN  " & TmpGridExpiry & " Expiry ON grid.recid = expiry.recid"
                    strsql &= " LEFT JOIN " & TmpGridBarcode & "  Barcode ON Barcode.recid = Grid.recid"
                    strsql &= " AND Barcode.e_recid = Expiry.e_recid"
                    strsql &= " LEFT JOIN stk04_PurDelOrdD ON DOD_ItmRef=grid.MOD_supitmref"
                    strsql &= " AND DOD_Stat=LEFT(grid.MOD_stat,1) AND DOD_Counter=grid.MOD_Count AND dod_hrecid=" & recid
                    strsql &= " left join stk04_stkfil on stk_recid=DOD_StkRecid "
                    strsql &= " WHERE   Expiry.MOD_OrdQty > = 0 and  stk_itmclass='e' AND ISNULL (Expiry.exp_stat,'')<>'D' and ISNULL (barcode.bar_stat,'')<>'D' ) temp "

                    cmSql.CommandText = strsql
                    cmSql.ExecuteNonQuery()


                    strsql = " INSERT  INTO dbo.Stk04_PurDelOrdDQCH"
                    strsql &= " SELECT  MyNewReciD ,"
                    strsql &= " PrDReciD ,"
                    strsql &= " PrQDRecId ,"
                    strsql &= " QCHRecId ,"
                    strsql &= " Reson ,"
                    strsql &= " RjQty ,AccQty,"
                    strsql &= " GETDATE() ,"
                    strsql &= " '" & gbUser & "' ,"
                    strsql &= " GETDATE() ,"
                    strsql &= " '" & gbUser & "'"
                    strsql &= " FROM    " & TempQcHeader & ""
                    strsql &= " INSERT  INTO dbo.Stk04_PurDelOrdDQCD"
                    strsql &= " SELECT  ROW_NUMBER() OVER ( ORDER BY ReciD )"
                    strsql &= " + ( SELECT  ISNULL(MAX(PurQCD_ReciD), 0)"
                    strsql &= " FROM    Stk04_PurDelOrdDQCD"
                    strsql &= " ) ,"
                    strsql &= " HRecID ,"
                    strsql &= " QCDRecID ,"
                    strsql &= " AnsYN ,"
                    strsql &= " AnsDate ,"
                    strsql &= " Val ,"
                    strsql &= " Remrk ,"
                    strsql &= " MultCh ,"
                    strsql &= " GETDATE() ,"
                    strsql &= " '" & gbUser & "' ,"
                    strsql &= " GETDATE() ,"
                    strsql &= " '" & gbUser & "'"
                    strsql &= " FROM    " & TempQcDetails & ""
                    cmSql.CommandText = strsql
                    cmSql.ExecuteNonQuery()

                ElseIf DelOrder = "Receiving Order Fashion" Or DelOrder = "Consignment Receiving Order Fashion" Then
                    InsertFashionItemBarcode(cmSql, cnSql, trnSql)
                    strsql = " INSERT INTO stk04_PurDelOrdDQty select * from Temp_PurDelOrdDQty" & gbUser & " WHERE ISNULL(DOQ_Recqty,0) <> 0 ; 
                               INSERT INTO stk04_PurDelOrdDBE select * from Temp_PurDelOrdDBE" & gbUser & ""

                    cmSql.CommandText = strsql
                    cmSql.ExecuteNonQuery()
                End If
            ElseIf Wtype = "MODIFY" Then

                ''''Update header Deliver Order
                strsql = "UPDATE Stk04_PurDelOrdH set "
                strsql &= " DOH_PORecid= " & IIf(txtOrderNo.Text.Trim = "", "Null", Val(txtOrderNo.Text.Replace("'", "''"))).ToString & " ,"
                strsql &= " DOH_loc='" & txtLoc.Text.Trim & "',DOH_ActDelDate='" & Format(txtActualDate.Value, "MM/dd/yyyy") & "',"
                strsql &= " DOH_Dstamp= getdate(),DOH_Ustamp='" & gbUser & "' where DOH_Recid=" & txtCode.Text.Trim

                cmSql.CommandText = strsql
                cmSql.ExecuteNonQuery()
                strsql = " UPDATE  " & TmpGridDetails & ""
                strsql &= " SET     DOB_Drecid = NEwDOB_Drecid"
                strsql &= " FROM    ( SELECT    RECID oldRecid ,"
                strsql &= " ROW_NUMBER() OVER ( ORDER BY MOD_supitmref )"
                strsql &= " + ( SELECT  ISNULL(MAX(DOD_Recid), 0)"
                strsql &= " FROM    Stk04_PurDelOrdD"
                strsql &= " ) AS NEwDOB_Drecid"
                strsql &= " FROM      " & TmpGridDetails & ""
                strsql &= "  WHERE   DOB_Drecid IS NULL ) t"
                strsql &= " WHERE   oldRecid = Recid "
                strsql &= " and  DOB_Drecid is NULL"

                cmSql.CommandText = strsql
                cmSql.ExecuteNonQuery()
                ' QC Head Table Updated
                strsql = " UPDATE " & TempQcHeader & " SET"
                strsql &= " PrDReciD =  DOB_Drecid"
                strsql &= " FROM " & TmpGridDetails & " Details WHERE"
                strsql &= " PrDReciD = Details.RECID  and OldNew ='N' "
                cmSql.CommandText = strsql
                cmSql.ExecuteNonQuery()

                strsql = " UPDATE  " & TempQcHeader & ""
                strsql &= " SET     MyNewReciD = NEwDOB_Drecid"
                strsql &= " FROM    ( SELECT    ReciD oldRecid ,"
                strsql &= " ROW_NUMBER() OVER ( ORDER BY PrDReciD )"
                strsql &= " + ( SELECT  ISNULL(MAX(PurQCH_ReciD), 0)"
                strsql &= " FROM    Stk04_PurDelOrdDQCH"
                strsql &= " ) AS NEwDOB_Drecid"
                strsql &= " FROM      " & TempQcHeader & "  WHERE   OldNew = 'N' "
                strsql &= " ) t"
                strsql &= " WHERE   oldRecid = Recid and OldNew ='N' "
                cmSql.CommandText = strsql
                cmSql.ExecuteNonQuery()
                strsql = " UPDATE " & TempQcDetails & " SET"
                strsql &= " HRecID =  MyNewReciD"
                strsql &= " FROM " & TempQcHeader & " header WHERE"
                strsql &= " HRecID = header.RECID and OldNew ='N' "

                cmSql.CommandText = strsql
                cmSql.ExecuteNonQuery()
                If DelOrder.Contains("Fashion") = False Then
                    strsql = " UPDATE " & TmpGridExpiry & ""
                    strsql &= " SET    NewRECID = NEwDOB_Drecid"
                    strsql &= " FROM   ( SELECT    E_RECID oldRecid ,"
                    strsql &= " ROW_NUMBER() OVER ( ORDER BY expirygrid.RECID )"
                    strsql &= " + ( SELECT  ISNULL(MAX(DOQ_RecID), 0)"
                    strsql &= " FROM    dbo.stk04_PurDelOrdDQty"
                    strsql &= " ) AS NEwDOB_Drecid"
                    strsql &= " FROM      " & TmpGridExpiry & " expirygrid"
                    strsql &= " INNER JOIN " & TmpGridDetails & " detailgrid"
                    strsql &= " ON detailgrid.RECID = expirygrid.RECID"
                    strsql &= "       WHERE    NewRECID IS NULL  "
                    '    strsql &= " WHERE    new_old='N' or new_old='P' or new_old='w'"
                    strsql &= " ) t"
                    strsql &= " WHERE  oldRecid = E_RECID  and NewRECID is null "
                    cmSql.CommandText = strsql
                    cmSql.ExecuteNonQuery()


                    strsql = " UPDATE " & TempQcHeader & " SET"
                    strsql &= " PrQDRecId =  NewRECID"
                    strsql &= " FROM " & TmpGridExpiry & " Details WHERE"
                    strsql &= " PrQDRecId = Details.E_RECID"
                    cmSql.CommandText = strsql
                    cmSql.ExecuteNonQuery()
                End If




                ''''Update Details Receiving Order
                strsql = "Insert into  Stk04_PurDelOrdD Select  ROW_NUMBER() OVER ( ORDER BY recid, MOD_supitmref ) + (SELECT  ISNULL(MAX(DOD_Recid), 0) FROM Stk04_PurDelOrdD)  ,"
                strsql &= txtCode.Text.Trim & ", REPLACE(REPLACE(RTRIM(LTRIM(MOD_supitmref)), CHAR(13), ''), CHAR(10), '')  ,"
                strsql &= " LEFT(MOD_stat,1), MOD_Count,case when new_old='w'THEN Tmp_StkID else MOD_RMRecid END,MOD_OrdQty,stk04_stkfil.stk_unit,MOD_ConsOrdQty, "
                ' ch @ 16/04/2018 request by tania if user print thid DO We Are Showing the Curent cost in Stock we should showing the cost of item during this do 
                ' If DelOrder = "Consignment Receiving Order Fashion" Or DelOrder = "Consignment Receiving Order" Then

                strsql &= " dbo.[convert5]('" & gbApp & "','" & gbComp & "','" & gbYear & "',"
                strsql &= " cost,'" & gbCurr2 & "','" & gbCurr1 & "','" & CType(Now, Date).ToString("MM/dd/yyyy") & "') UnitCostLC,"
                strsql &= "   cost UnitCostCC,"
                strsql &= "  dbo.[convert5]('" & gbApp & "','" & gbComp & "','" & gbYear & "',"
                strsql &= " cost,'" & gbCurr2 & "',STK_FCCODE,'" & CType(Now, Date).ToString("MM/dd/yyyy") & "')   UnitCostFC,"

                'ElseIf DelOrder = "Receiving Order Fashion" Or DelOrder = "Receiving Order" Then
                'strsql &= " 0, 0,0,"
                ' End If

                strsql &= " LEFT( Invd_vat,1),getdate(),'" & gbUser & "', "
                If DelOrder = "Receiving Order" Or DelOrder = "Consignment Receiving Order" Then
                    strsql &= " null, "
                ElseIf DelOrder = "Receiving Order Fashion" Or DelOrder = "Consignment Receiving Order Fashion" Then
                    strsql &= " grid.SuppColor, "
                End If
                strsql &= " getdate(),'" & gbUser & "', "
                If DelOrder = "Receiving Order" Or DelOrder = "Consignment Receiving Order" Then
                    strsql &= " null  "
                ElseIf DelOrder = "Receiving Order Fashion" Or DelOrder = "Consignment Receiving Order Fashion" Then
                    strsql &= " Grid.Color "
                End If
                If DelOrder = "Consignment Receiving Order Fashion" Or DelOrder = "Consignment Receiving Order" Then
                    strsql &= " , disc "
                    strsql &= " , dbo.[convert5]('" & gbApp & "','" & gbComp & "','" & gbYear & "',"
                    strsql &= " net,STK_FCCODE,'" & gbCurr1 & "','" & CType(Now, Date).ToString("MM/dd/yyyy") & "') netCostLC,"
                    strsql &= " dbo.[convert5]('" & gbApp & "','" & gbComp & "','" & gbYear & "',"
                    strsql &= " net,STK_FCCODE,'" & gbCurr2 & "','" & CType(Now, Date).ToString("MM/dd/yyyy") & "') netCostCC,"
                    strsql &= " (CASE WHEN STK_FCCODE<>'" & gbCurr1 & "' AND STK_FCCODE<>'" & gbCurr2 & "' "
                    strsql &= " THEN  net else dbo.[convert5]('" & gbApp & "','" & gbComp & "','" & gbYear & "',"
                    strsql &= " net,STK_FCCODE,'" & gbCurr2 & "','" & CType(Now, Date).ToString("MM/dd/yyyy") & "') END) NetCostFC "
                Else
                    strsql &= "  , Null "
                    strsql &= " , dbo.[convert5]('" & gbApp & "','" & gbComp & "','" & gbYear & "',"
                    strsql &= " (cost * MOD_OrdQty),'" & gbCurr2 & "','" & gbCurr1 & "','" & CType(Now, Date).ToString("MM/dd/yyyy") & "') netCostLC,"
                    strsql &= "   (cost * MOD_OrdQty)  netCostCC,"
                    strsql &= "    dbo.[convert5]('" & gbApp & "','" & gbComp & "','" & gbYear & "',"
                    strsql &= " (cost * MOD_OrdQty),'" & gbCurr2 & "',STK_FCCODE,'" & CType(Now, Date).ToString("MM/dd/yyyy") & "')  NetCostFC "
                End If


                strsql &= " From " & TmpGridDetails & " Grid "
                strsql &= "    Left join (select stk_recid,stk_unit  from stk04_stkfil ) stk04_stkfil on stk_recid = MOD_RMRecid "
                strsql &= " LEFT JOIN TMP_" & TmpGridDetails & " ON Tmp_PendingID= MOD_RMRecid"
                strsql &= " where new_old='N' or new_old='P' or new_old='w'"
                cmSql.CommandText = strsql
                cmSql.ExecuteNonQuery()

                strsql = "Update Stk04_PurDelOrdD set DOD_ItmRef=Grid.MOD_supitmref , DOD_Counter=grid.mod_count "
                strsql &= " From " & TmpGridDetails & " Grid "
                strsql &= " where new_old='U' and DOD_Recid=Grid.DOB_Drecid AND Grid.DOB_Drecid IS NOT null AND DOD_Stat='n' "
                cmSql.CommandText = strsql
                cmSql.ExecuteNonQuery()

                '..Update details barcode and expiry
                If DelOrder = "Receiving Order" Or DelOrder = "Consignment Receiving Order" Then
                    strsql = "  INSERT  INTO stk04_PurDelOrdDBE"
                    strsql &= " SELECT  ROW_NUMBER() OVER ( ORDER BY DOD_RECID )+ ( SELECT  ISNULL(MAX(DOB_RecID), 0)FROM    stk04_PurDelOrdDBE) ,temp.* FROM ("
                    strsql &= " SELECT stk04_PurDelOrdD.DOD_RECID , Barcode.DOB_Barcode ,Expiry.DOB_ExpiryDate ,Expiry.DOB_BatchInfo ,"
                    strsql &= " NULL grid,NULL color, GETDATE() Cdate,'" & gbUser & "' cuser,GETDATE() date,'" & gbUser & "' ouser"
                    strsql &= " FROM    " & TmpGridDetails & " Grid"
                    strsql &= " LEFT JOIN  " & TmpGridExpiry & " Expiry ON grid.recid = expiry.recid"
                    strsql &= " LEFT JOIN " & TmpGridBarcode & "  Barcode ON Barcode.recid = Grid.recid"
                    strsql &= " AND Barcode.e_recid = Expiry.e_recid"
                    strsql &= " LEFT JOIN stk04_PurDelOrdD ON DOD_ItmRef=grid.MOD_supitmref"
                    strsql &= " AND DOD_Stat=LEFT(grid.MOD_stat,1) AND DOD_Counter=grid.MOD_Count AND dod_hrecid=" & txtCode.Text.Trim
                    strsql &= " left join stk04_stkfil on stk_recid=DOD_StkRecid "
                    strsql &= " WHERE Grid.new_old<>'O' and  stk_itmclass='e' and  ISNULL (barcode.bar_stat,'') <>'D'AND ISNULL (Expiry.exp_stat,'')<>'D'  AND ISNULL (Expiry.exp_stat,'')<>''  AND   ISNULL (Barcode.DOB_Barcode,'')<>'' "
                    strsql &= " UNION SELECT DOD_RECID ,Barcode.DOB_Barcode ,null ,null,NULL ,NULL ,"
                    strsql &= " GETDATE() cdate,'" & gbUser & "' cuser ,GETDATE() date,'" & gbUser & "' ouser "
                    strsql &= " FROM    " & TmpGridDetails & " Grid"
                    strsql &= " LEFT JOIN " & TmpGridBarcode & " Barcode ON Barcode.recid = Grid.recid"
                    strsql &= " LEFT JOIN stk04_PurDelOrdD ON DOD_ItmRef=grid.MOD_supitmref"
                    strsql &= " AND DOD_Stat=LEFT(grid.MOD_stat,1) AND DOD_Counter=grid.MOD_Count AND dod_hrecid=" & txtCode.Text.Trim
                    strsql &= " left join stk04_stkfil on stk_recid=DOD_StkRecid  WHERE  Grid.new_old<>'O' and Barcode.DOB_Barcode<>'' and barcode.bar_stat<>'D' AND  ISNULL(Barcode.DOB_Barcode,'')<>'' and (stk_itmclass='b' or stk_itmclass='t' )  ) temp "
                    cmSql.CommandText = strsql
                    cmSql.ExecuteNonQuery()

                    strsql = "  INSERT  INTO stk04_PurDelOrdDQty"
                    strsql &= " SELECT  ROW_NUMBER() OVER ( ORDER BY DOD_RECID )+ ( SELECT  ISNULL(MAX(DOQ_RecID), 0)FROM    stk04_PurDelOrdDQTY) ,temp.* FROM ("
                    strsql &= " SELECT DISTINCT DOD_RECID , Expiry.DOB_ExpiryDate ,Expiry.DOB_BatchInfo ,"
                    strsql &= " NULL grid,NULL color,Expiry.MOD_OrdQty ,GETDATE() Cdate,'" & gbUser & "' cuser,GETDATE() date,'" & gbUser & "' ouser"
                    strsql &= " FROM    " & TmpGridDetails & " Grid"
                    strsql &= " LEFT JOIN  " & TmpGridExpiry & " Expiry ON grid.recid = expiry.recid"
                    strsql &= " LEFT JOIN " & TmpGridBarcode & "  Barcode ON Barcode.recid = Grid.recid"
                    strsql &= " AND Barcode.e_recid = Expiry.e_recid"
                    strsql &= " LEFT JOIN stk04_PurDelOrdD ON DOD_ItmRef=grid.MOD_supitmref"
                    strsql &= " AND DOD_Stat=LEFT(grid.MOD_stat,1) AND DOD_Counter=grid.MOD_Count AND dod_hrecid=" & txtCode.Text.Trim
                    strsql &= " left join stk04_stkfil on stk_recid=DOD_StkRecid "
                    strsql &= " WHERE  Grid.new_old<>'O' and Expiry.MOD_OrdQty >= 0 and  ISNULL (barcode.bar_stat,'')<>'D'AND ISNULL (Expiry.exp_stat,'')<>'D' and stk_itmclass='e'  ) temp "
                    cmSql.CommandText = strsql
                    cmSql.ExecuteNonQuery()

                ElseIf DelOrder = "Receiving Order Fashion" Or DelOrder = "Consignment Receiving Order Fashion" Then
                    InsertFashionItemBarcode(cmSql, cnSql, trnSql)
                    strsql = " INSERT INTO stk04_PurDelOrdDQty select * from Temp_PurDelOrdDQty" & gbUser & " ; INSERT INTO stk04_PurDelOrdDBE select * from Temp_PurDelOrdDBE" & gbUser

                    cmSql.CommandText = strsql
                    cmSql.ExecuteNonQuery()
                End If

                strsql = " INSERT  INTO dbo.Stk04_PurDelOrdDQCH"
                strsql &= " SELECT  MyNewReciD ,"
                strsql &= " PrDReciD ,"
                strsql &= " PrQDRecId ,"
                strsql &= " QCHRecId ,"
                strsql &= " Reson ,"
                strsql &= " RjQty ,AccQty,"
                strsql &= " GETDATE() ,"
                strsql &= " '" & gbUser & "' ,"
                strsql &= " GETDATE() ,"
                strsql &= " '" & gbUser & "'"
                strsql &= " FROM    " & TempQcHeader & " where OldNew ='N' "

                cmSql.CommandText = strsql
                cmSql.ExecuteNonQuery()

                strsql = "  INSERT  INTO dbo.Stk04_PurDelOrdDQCD"
                strsql &= " SELECT  ROW_NUMBER() OVER ( ORDER BY Details.ReciD )"
                strsql &= " + ( SELECT  ISNULL(MAX(PurQCD_ReciD), 0)"
                strsql &= " FROM    Stk04_PurDelOrdDQCD "
                strsql &= " ) ,"
                strsql &= " HRecID ,"
                strsql &= " QCDRecID ,"
                strsql &= " AnsYN ,"
                strsql &= " AnsDate ,"
                strsql &= " Val ,"
                strsql &= " Remrk ,"
                strsql &= " MultCh ,"
                strsql &= " GETDATE() ,"
                strsql &= " '" & gbUser & "' ,"
                strsql &= " GETDATE() ,"
                strsql &= " '" & gbUser & "'"
                strsql &= " FROM    " & TempQcDetails & " Details   "
                strsql &= "   INNER JOIN " & TempQcHeader & " header ON  header.MyNewReciD = Details.HRecID"
                strsql &= " 	WHERE OldNew ='N' "

                cmSql.CommandText = strsql
                cmSql.ExecuteNonQuery()

            End If





            '''''Inserting New Supp Ref In Table STK04_StkSupRef
            If DelOrder = "Receiving Order Fashion" Or DelOrder = "Consignment Receiving Order Fashion" Then
                strsql = " INSERT  INTO   STK04_StkSupRef "
                strsql &= "  SELECT temp.* ,ROW_NUMBER() OVER ( ORDER BY MOD_RMRecid ) + ( SELECT  ISNULL(MAX(SREF_RecId), 0) FROM    STK04_StkSupRef  )  from ( "
                strsql &= " SELECT DISTINCT case when new_old='w'THEN Tmp_StkID else MOD_RMRecid END as MOD_RMRecid , " & drsql.Item("sup_Recid").ToString & " A1,  REPLACE(REPLACE(RTRIM(LTRIM(MOD_supitmref)), CHAR(13), ''), CHAR(10), '')  A2,  'DO' A3,'" & gbUser & "' A4, GETDATE() A5, Grid.SuppColor A6, Grid.Color A7"
                strsql &= " From " & TmpGridDetails & " Grid"
                strsql &= " LEFT JOIN TMP_" & TmpGridDetails & " ON Tmp_PendingID= MOD_RMRecid"
                strsql &= " where   CONVERT(VARCHAR,MOD_RMRecid ) + CONVERT(VARCHAR," & drsql.Item("sup_Recid").ToString & ")+ CONVERT(VARCHAR, Grid.MOD_supitmref) + CONVERT(VARCHAR, Grid.SuppColor)+ CONVERT(VARCHAR, Grid.Color) NOT IN (  Select  "
                strsql &= " CONVERT(VARCHAR,SRef_StkRecId ) + CONVERT(VARCHAR,SRef_SupRecId)+ CONVERT(VARCHAR, ISNULL(SRef_SupRef,'')) + CONVERT(VARCHAR, ISNULL(SRef_SupColor,'')) + CONVERT(VARCHAR, ISNULL(SRef_ItmColor,''))  FROM STK04_StkSupRef "
                strsql &= " WHERE SRef_StkRecId = case when new_old='w'THEN Tmp_StkID else Grid.MOD_RMRecid END  AND SRef_SupRecId =" & drsql.Item("sup_Recid").ToString & ")) TEMP"
                cmSql.CommandText = strsql
                cmSql.ExecuteNonQuery()
            Else
                strsql = " INSERT  INTO   STK04_StkSupRef "
                strsql &= "  SELECT temp.* ,ROW_NUMBER() OVER ( ORDER BY MOD_RMRecid ) + ( SELECT  ISNULL(MAX(SREF_RecId), 0) FROM    STK04_StkSupRef  )  from ( "
                strsql &= " SELECT DISTINCT  case when new_old='w'THEN Tmp_StkID else MOD_RMRecid END as MOD_RMRecid , " & drsql.Item("sup_Recid").ToString & " A1,   REPLACE(REPLACE(RTRIM(LTRIM(MOD_SupItmRef)), CHAR(13), ''), CHAR(10), '') MOD_supitmref,  'DO' A2,'" & gbUser & "' A3, GETDATE() A4, NULL A5, NULL A6 "
                strsql &= " From " & TmpGridDetails & " Grid"
                strsql &= " LEFT JOIN TMP_" & TmpGridDetails & " ON Tmp_PendingID= MOD_RMRecid"
                strsql &= " where   CONVERT(VARCHAR,MOD_RMRecid ) + CONVERT(VARCHAR," & drsql.Item("sup_Recid").ToString & ")+ CONVERT(VARCHAR, Grid.MOD_supitmref)  NOT IN (  Select  "
                strsql &= " CONVERT(VARCHAR,SRef_StkRecId ) + CONVERT(VARCHAR,SRef_SupRecId)+ CONVERT(VARCHAR, ISNULL(SRef_SupRef,'')) + CONVERT(VARCHAR, ISNULL(SRef_SupColor,'')) + CONVERT(VARCHAR, ISNULL(SRef_ItmColor,''))  FROM STK04_StkSupRef "
                strsql &= " WHERE SRef_StkRecId = case when new_old='w' THEN Tmp_StkID else Grid.MOD_RMRecid END  AND SRef_SupRecId =" & drsql.Item("sup_Recid").ToString & ")) TEMP"
                cmSql.CommandText = strsql
                cmSql.ExecuteNonQuery()
            End If

            If DelOrder = "Consignment Receiving Order" Or DelOrder = "Consignment Receiving Order Fashion" Then
                If Wtype <> "DELETE" Then
                    'ch @ 20/05/2019  as agreed with Tania We Should Updtae THe Cost Of Normal Item From This Step , Sine In Update Pi Will Not Be Updated Old Qty +New Qty = 0  mail(ERP_Purchase_update PI of consignment item turned to normal)
                    strsql = "UPDATE STK04_STKFIL SET"
                    strsql &= " [STK_UNITCOSTLC]= case WHEN DOD_StkRecid Is NULL then dbo.convert5('" & gbApp & "','" & gbComp & "','" & gbYear & "', (net / MOD_OrdQty ), '" & txtCurr.Text.Trim.Replace("'", "''") & "', '" & gbCurr1 & "','" & Format(Now.Date, "MM/dd/yyyy") & "') else STK_UNITCOSTLC end  ,"
                    strsql &= " [STK_UNITCOSTCC]=  case WHEN DOD_StkRecid Is NULL then dbo.convert5('" & gbApp & "','" & gbComp & "','" & gbYear & "',  (net / MOD_OrdQty ), '" & txtCurr.Text.Trim.Replace("'", "''") & "', '" & gbCurr2 & "','" & Format(Now.Date, "MM/dd/yyyy") & "') else STK_UNITCOSTCC  end ,"
                    strsql &= " [STK_UNITCOSTFC]= case when DOD_StkRecid  Is NULL then (  dbo.convert5('" & gbApp & "','" & gbComp & "','" & gbYear & "', (net / MOD_OrdQty ), '" & txtCurr.Text.Trim.Replace("'", "''") & "', STK04_STKFIL.stk_FCCode,'" & Format(Now.Date, "MM/dd/yyyy") & "') ) else STK_UNITCOSTFC end  ,"
                    strsql &= " [STK_AVGCOSTLC]=dbo.convert5('" & gbApp & "','" & gbComp & "','" & gbYear & "',  (net / MOD_OrdQty ), '" & txtCurr.Text.Trim.Replace("'", "''") & "', '" & gbCurr1 & "','" & Format(Now.Date, "MM/dd/yyyy") & "'),"
                    strsql &= " [STK_AVGCOSTCC]=dbo.convert5('" & gbApp & "','" & gbComp & "','" & gbYear & "',  (net / MOD_OrdQty ), '" & txtCurr.Text.Trim.Replace("'", "''") & "', '" & gbCurr2 & "','" & Format(Now.Date, "MM/dd/yyyy") & "'),"
                    strsql &= " [STK_AVGCOSTFC]=(dbo.convert5('" & gbApp & "','" & gbComp & "','" & gbYear & "',  (net / MOD_OrdQty ), '" & txtCurr.Text.Trim.Replace("'", "''") & "', STK04_STKFIL.stk_fccode,'" & Format(Now.Date, "MM/dd/yyyy") & "')  ) "
                    'strsql &= " [STK_SUPREF]= MOD_supitmref"
                    strsql &= " FROM " & TmpGridDetails & " Grid inner join ( SELECT   MAX(recid)recid,  MOD_RMRecid  FROM  " & TmpGridDetails & " GROUP BY MOD_RMRecid ) DistinctItem ON  DistinctItem.recid = Grid.RECID AND DistinctItem.MOD_RMRecid = Grid.MOD_RMRecid "
                    'strsql &= "LEFT JOIN TMP_" & TmpGridDetails & " ON Tmp_PendingID= MOD_RMRecid WHERE  STK_RECID=case when new_old='w' THEN Tmp_StkID else Grid.MOD_RMRecid END  AND new_old<>'O'"
                    strsql &= " LEFT JOIN TMP_" & TmpGridDetails & " ON Tmp_PendingID= Grid.MOD_RMRecid "
                    strsql &= "  INNER JOIN V_StockNormalCons ON  CASE WHEN new_old = 'w' THEN Tmp_StkID ELSE Grid.MOD_RMRecid  END = CSTK_RECID"
                    If txtCode.Text.Trim <> "" Then strsql &= " LEFT JOIN ( SELECT DISTINCT DOD_StkRecid   FROM stk04_purdelordd WHERE DOD_Hrecid <>'" & txtCode.Text.Trim & "'  ) PurchasedBefore ON  DOD_StkRecid =    Grid.MOD_RMRecid "
                    strsql &= " WHERE  (STK_RECID=case when new_old='w' THEN Tmp_StkID else Grid.MOD_RMRecid END  OR  (STK_RECID = NSTK_RECID AND DOD_StkRecid IS NULL  )   )  and  new_old<>'O'  "
                    cmSql.CommandText = strsql
                    cmSql.ExecuteNonQuery()
                End If
            End If



            '...Insert Into Stk04_StkQty

            strsql = "select  * from "
            strsql &= " ( SELECT ItmClass ,mod_rmrecid ,RECID , new_old,'Temp' OrgItem " & IIf(DelOrder.Contains("Fashion"), ", color ", "").ToString & "    FROM " & TmpGridDetails
            strsql &= "   UNION ALL  "
            strsql &= "  SELECT ItmClass ,NSTK_RECID, RECID, new_old ,'Cons' OrgItem " & IIf(DelOrder.Contains("Fashion"), ", color ", "").ToString & "    FROM  " & TmpGridDetails
            strsql &= "  INNER JOIN V_StockNormalCons ON CSTK_RECID = mod_rmrecid  ) TempItems "
            If DelOrder = "Receiving Order" Or DelOrder = "Consignment Receiving Order" Then
                strsql &= " LEFT JOIN " & TmpGridExpiry & " expiry on expiry.RECID=TempItems.RECID and expiry.DOB_BatchInfo<>'' AND expiry.exp_stat<>'D' "
            Else
                strsql &= " LEFT JOIN " & TmpFashQty & " Fashion on Fashion.Recid=TempItems.Recid"
            End If
            strsql &= " where new_old<>'w' AND new_old<>'O'"

            cmsql2 = New SqlCommand(strsql, cnSql, trnSql)
            cmsql2.CommandTimeout = 0
            drsqlL = cmsql2.ExecuteReader()
            'ReadFromDBToDataReaderWithconnection(strsql, drsqlL, cmsql2)

            'Dim j As Integer
            'If DelOrder = "Consignment Receiving Order" Or DelOrder = "Consignment Receiving Order Fashion" Then
            '    j = 2
            'Else
            '    j = 1
            'End If

            cmSql.CommandText = " SET NOCOUNT ON "
            cmSql.ExecuteNonQuery()
            With drsqlL
                If .HasRows Then

                    While .Read()
                        Dim loc As String
                        For i = 0 To 1
                            If .Item("OrgItem").ToString = "Cons" Then i = 1
                            If i = 0 Then
                                loc = txtLoc.Text.Trim
                            Else
                                loc = "00"
                            End If
                            If DelOrder = "Receiving Order" Or DelOrder = "Consignment Receiving Order" Then
                                InsertMissingRecQty(cmSql, .Item("ItmClass").ToString, loc, CLng(.Item("mod_rmrecid").ToString), _
                                                                "", "", "", CStr(IIf(.Item("DOB_ExpiryDate").ToString = "", "", .Item("DOB_ExpiryDate").ToString)), _
                                                                CStr(IIf(.Item("DOB_BatchInfo").ToString = "", "", .Item("DOB_BatchInfo").ToString)), CStr(IIf(.Item("DOQ_ProductionDt").ToString = "", "", .Item("DOQ_ProductionDt").ToString)))
                            Else
                                Dim strsql1 As String
                                Dim drstrsql1 As SqlDataReader
                                strsql1 = " SELECT GD_SizeDesc FROM stk04_DetGRID "
                                strsql1 &= " WHERE GD_Code = '" & .Item("GridCode").ToString & "'"
                                ReadFromDBToDataReader(strsql1, drstrsql1)
                                If drstrsql1.HasRows Then
                                    While drstrsql1.Read
                                        InsertMissingRecQty(cmSql, .Item("ItmClass").ToString, loc, CLng(.Item("mod_rmrecid").ToString), _
                                                                        .Item("GridCode").ToString, drstrsql1.Item("GD_SizeDesc").ToString, .Item("Color").ToString, "", "")
                                    End While
                                End If
                            End If
                        Next
                    End While
                End If
            End With

            Disposecontrols(, cmsql2, drsqlL)
            strsql = " Select top 1 1 "
            strsql &= " FROM (select case when new_old='w'THEN Tmp_StkID else MOD_RMRecid END as MOD_RMRecid  ,"
            strsql &= "   ISNULL(SUM( CASE WHEN LEFT(mod_stat, 1) = 'R' THEN -1 ELSE 1 END * CASE WHEN Grid.itmclass='E' then 
                                " & IIf(DelOrder.Contains("Fashion") = False, "Expiry.MOD_OrdQty", "0").ToString & "  ELSE Grid.MOD_OrdQty end), 0) AS MOD_OrdQty , 
                            " & IIf(DelOrder.Contains("Fashion") = False, "Expiry.DOB_Expirydate,Expiry.DOB_BatchInfo,", "").ToString & "
                            new_old,Tmp_StkID  "
            strsql &= " FROM " & TmpGridDetails & " Grid "
            strsql &= " LEFT JOIN TMP_" & TmpGridDetails & " tmp2 ON Tmp_PendingID= MOD_RMRecid"

            If DelOrder.Contains("Fashion") = False Then
                strsql &= " LEFT JOIN " & TmpGridExpiry & " Expiry  on Expiry.RECID=Grid.RECID"
            Else
                strsql &= " LEFT JOIN " & TmpFashQty & " fashion  on fashion.RECID=Grid.RECID"
            End If

            strsql &= "  group by MOD_RMRecid, " & IIf(DelOrder.Contains("Fashion") = False, "Expiry.DOB_Expirydate,Expiry.DOB_BatchInfo,", "").ToString & "new_old,Tmp_StkID ) as T1 "
            strsql &= "  inner join stk04_stkqty on  STQ_STKRECID = T1.MOD_RMRecid   "

            If DelOrder.Contains("Fashion") = False Then
                strsql &= " AND isnull(t1.DOB_Expirydate,'')=isnull(STQ_ExpiryDt,'')"
                strsql &= " AND isnull(t1.DOB_BatchInfo,'') =isnull(STQ_BatchInfo,'') "
            End If

            If DelOrder <> "Consignment Receiving Order" AndAlso DelOrder <> "Consignment Receiving Order Fashion" Then
                strsql &= " and  exists(select Top 1 1 from stk04_stkfil where stk_recid = STQ_STKRECID and stk_qtyctrl='Y' ) "
            End If
            strsql &= " AND STQ_LOCCODE='" & txtLoc.Text.Trim & "' "
            If Wtype = "ADD" Or Wtype = "MODIFY" Then
                strsql &= " and  STQ_AVAILABLE + MOD_OrdQty < 0 "
            ElseIf Wtype = "DELETE" Then
                strsql &= "  and STQ_AVAILABLE - MOD_OrdQty < 0 "
            End If
            If RecordsExists(strsql, cmSql) Then
                If Not trnSql Is Nothing Then trnSql.Rollback()
                MessageBox.Show("Exists Item With No Available Qty ", cntTitle, MessageBoxButtons.OK, MessageBoxIcon.Question)
                Exit Sub
            End If
            '...Update StockQty
            strsql = "Update Stk04_StkQty Set  "
            If Wtype = "ADD" Or Wtype = "MODIFY" Then
                'strsql &= "  STQ_AVAILABLE =STQ_AVAILABLE + (CASE WHEN T1.stat='R' THEN  (-1) * MOD_OrdQty ELSE MOD_OrdQty END ),"
                strsql &= "  STQ_AVAILABLE =STQ_AVAILABLE + MOD_OrdQty,"
            ElseIf Wtype = "DELETE" Then
                strsql &= "  STQ_AVAILABLE =STQ_AVAILABLE - MOD_OrdQty,"
            End If
            If DelOrder = "Receiving Order" Or DelOrder = "Consignment Receiving Order" Then
                strsql &= " STQ_ExpiryDt=isnull(t1.DOB_Expirydate,null) ,STQ_BatchInfo=isnull(t1.DOB_BatchInfo,null)"
            Else
                strsql &= " STQ_Grid=isnull(t1.DOQ_GridCode,null),STQ_DetGrid=isnull(DOQ_GridSize,null),STQ_Color=isnull(t1.color,null) "
            End If
            strsql &= " ,STQ_DSTAMP = GETDATE() , STQ_USTAMP='" & gbUser & "'"
            strsql &= " FROM (select case when new_old='w'THEN Tmp_StkID else MOD_RMRecid END as MOD_RMRecid  ,"
            If DelOrder = "Receiving Order" Or DelOrder = "Consignment Receiving Order" Then
                strsql &= "   ISNULL(SUM( CASE WHEN LEFT(mod_stat, 1) = 'R' THEN -1 ELSE 1 END * CASE WHEN Grid.itmclass='E' then Expiry.MOD_OrdQty ELSE Grid.MOD_OrdQty end), 0) AS MOD_OrdQty , Expiry.DOB_Expirydate,Expiry.DOB_BatchInfo,new_old,Tmp_StkID  "
            Else
                strsql &= " Sum( CASE WHEN LEFT(mod_stat, 1) = 'R' THEN -1 ELSE 1 END * Fashion.DOQ_Recqty) as MOD_OrdQty,Fashion.DOQ_GridCode,Fashion.DOQ_GridSize,Grid.Color,new_old,Tmp_StkID "
            End If
            strsql &= " FROM " & TmpGridDetails & " Grid "
            strsql &= " LEFT JOIN TMP_" & TmpGridDetails & " tmp2 ON Tmp_PendingID= MOD_RMRecid"

            If DelOrder = "Receiving Order Fashion" Or DelOrder = "Consignment Receiving Order Fashion" Then
                strsql &= " LEFT JOIN stk04_PurDelOrdD ON DOD_ItmRef=grid.MOD_supitmref"
                strsql &= " AND DOD_Stat=LEFT(grid.MOD_stat,1) AND DOD_Counter=grid.MOD_Count AND dod_hrecid="
                If Wtype = "ADD" Then
                    strsql &= " ( SELECT  ISNULL(MAX(DOH_RecID), 0) FROM stk04_PurDelOrdH)  "
                Else
                    strsql &= txtCode.Text.Trim
                End If
                strsql &= " LEFT JOIN temp_PurDelOrdDQty" & gbUser & " Fashion  on  Fashion.DOQ_Drecid=DOD_Recid"
                strsql &= "    group by MOD_RMRecid,Fashion.DOQ_GridCode,Fashion.DOQ_GridSize,Grid.Color,new_old,Tmp_StkID ) as T1 "
            Else
                strsql &= " LEFT JOIN " & TmpGridExpiry & " Expiry  on Expiry.RECID=Grid.RECID"
                If Wtype = "MODIFY" Then
                    strsql &= "  WHERE  Grid.new_old<>'O'  group by MOD_RMRecid, Expiry.DOB_Expirydate,Expiry.DOB_BatchInfo,new_old,Tmp_StkID ) as T1 "
                Else
                    strsql &= "  group by MOD_RMRecid, Expiry.DOB_Expirydate,Expiry.DOB_BatchInfo,new_old,Tmp_StkID ) as T1 "
                End If
            End If
            strsql &= " Where STQ_STKRECID = T1.MOD_RMRecid   "
            If DelOrder = "Receiving Order Fashion" Or DelOrder = "Consignment Receiving Order Fashion" Then
                strsql &= " AND isnull(t1.DOQ_GridCode,'')=isnull(STQ_Grid,'') " '---grid code 
                strsql &= " AND isnull(t1.DOQ_GridSize,'') =isnull(STQ_DetGrid,'') " '---grid size (S,M,L,XL) J.....+1" 
                strsql &= " AND isnull(t1.color,'') =isnull(STQ_Color,'') "
            Else
                strsql &= " AND isnull(t1.DOB_Expirydate,'')=isnull(STQ_ExpiryDt,'')"
                strsql &= " AND isnull(t1.DOB_BatchInfo,'') =isnull(STQ_BatchInfo,'') "
            End If
            strsql &= " AND STQ_LOCCODE='" & txtLoc.Text.Trim & "'"
            cmSql.CommandText = strsql
            cmSql.ExecuteNonQuery()

            'If Wtype <> "DELETE" And (DelOrder = "Consignment Receiving Order" Or DelOrder = "Consignment Receiving Order Fashion") Then
            '    strsql = "UPDATE STK04_STKFIL SET [STK_UNITCOSTLC]=UnitCostLC,[STK_UNITCOSTCC]=UnitCostCC,"
            '    strsql &= " [STK_UNITCOSTFC]=UnitCostFC,[STK_AVGCOSTLC]=UnitCostLC,"
            '    strsql &= " [STK_AVGCOSTCC]=UnitCostCC,[STK_AVGCOSTFC]=UnitCostFC, [STK_SUPREF]= MOD_supitmref"
            '    strsql &= " FROM " & TmpGridDetails & " "
            '    strsql &= " LEFT JOIN TMP_" & TmpGridDetails & " tmp2 ON Tmp_PendingID= MOD_RMRecid WHERE  STK_RECID=case when new_old='w'THEN Tmp_StkID else MOD_RMRecid END"

            '    cmSql.CommandText = strsql
            '    cmSql.ExecuteNonQuery()
            'End If


            '..insert new barcode and new expiry in stk04_detbarcode
            If DelOrder = "Receiving Order" Or DelOrder = "Consignment Receiving Order" Then
                strsql = "  INSERT  INTO   stk04_detbarcode "
                strsql &= " SELECT  ROW_NUMBER() OVER ( ORDER BY MOD_RMRecid ) + ( SELECT  ISNULL(MAX(dbar_recid), 0)"
                strsql &= " FROM    stk04_detbarcode   ) ,temp.* FROM ("
                strsql &= " SELECT case when new_old='w'THEN Tmp_StkID else Grid.MOD_RMRecid END as MOD_RMRecid  , NULL gridCode, NULL detgrid,  NULL color,"
                strsql &= " ISNULL(expiry.DOB_ExpiryDate, NULL) expdate , ISNULL(expiry.DOB_BatchInfo, NULL) batch , ISNULL(barcode.DOB_Barcode, NULL) barcode,GETDATE() DSTMP ,'" & gbUser & "' USTMP,GETDATE() CrDSTMP,'" & gbUser & "' CrUSTMP "
                strsql &= " From " & TmpGridDetails & " grid"
                strsql &= " LEFT JOIN TMP_" & TmpGridDetails & " ON Tmp_PendingID= Grid.MOD_RMRecid"

                strsql &= " LEFT JOIN " & TmpGridExpiry & "  expiry ON Grid.recid = expiry.recid"
                strsql &= " LEFT JOIN " & TmpGridBarcode & " barcode ON barcode.recid = grid.recid AND barcode.e_recid = expiry.e_recid"
                strsql &= " WHERE  grid.ItmClass='e' AND CONVERT(VARCHAR,ISNULL(expiry.DOB_ExpiryDate, NULL) ) + CONVERT(VARCHAR,ISNULL(expiry.DOB_BatchInfo, NULL) )+"
                strsql &= " CONVERT(VARCHAR,ISNULL(barcode.DOB_Barcode, NULL))   NOT  IN ( SELECT CONVERT(VARCHAR,ISNULL(dbar_expdate,NULL))+"
                strsql &= " CONVERT(VARCHAR,ISNULL(dbar_batch,NULL))+ CONVERT(VARCHAR,ISNULL(dbar_barcode,NULL)) FROM    Stk04_DetBarcode  WHERE dbar_stkid=CASE WHEN new_old = 'w' THEN Tmp_StkID Else Grid.MOD_RMRecid  END )"
                strsql &= " AND barcode.DOB_Barcode <> '' AND barcode.DOB_Barcode  is NOT null"

                strsql &= " UNION SELECT case when new_old='w'THEN Tmp_StkID else MOD_RMRecid END as MOD_RMRecid  ,NULL grid,NULL detgrid,NULL color,NULL expdate,NULL batch,"
                strsql &= " ISNULL(" & TmpGridBarcode & ".DOB_Barcode, NULL)  barcode,GETDATE() DSTMP,'" & gbUser & "' USTMP,GETDATE() CrDSTMP ,'" & gbUser & "' CrUSTMP  FROM  " & TmpGridDetails
                strsql &= " LEFT JOIN TMP_" & TmpGridDetails & " ON Tmp_PendingID= MOD_RMRecid"
                strsql &= " LEFT JOIN " & TmpGridBarcode & " ON " & TmpGridBarcode & ".recid = " & TmpGridDetails & ".recid"
                strsql &= " WHERE   (bar_stat = 'N'  "
                strsql &= " or bar_stat = 'A') " 'added by GS on 09/06/16
                strsql &= " AND   DOB_Barcode NOT  IN (  SELECT  dbar_barcode  FROM    Stk04_DetBarcode  WHERE dbar_stkid=MOD_RMRecid ) "
                strsql &= " AND ItmClass<>'e'  AND " & TmpGridBarcode & ".DOB_Barcode<>'' ) temp"
                cmSql.CommandText = strsql
                cmSql.ExecuteNonQuery()
            ElseIf DelOrder = "Receiving Order Fashion" Or DelOrder = "Consignment Receiving Order Fashion" Then

                DropSqlObjectWithTrans("Tmp_detbarcode" & gbUser, "TABLE", cmSql)
                strsql = " SELECT * INTO Tmp_detbarcode" & gbUser & " FROM dbo.stk04_detbarcode WHERE 1=2    "
                cmSql.CommandText = strsql
                cmSql.ExecuteNonQuery()

                For i = 1 To MaxCnt
                    strsql = " insert into Tmp_detbarcode" & gbUser & " "
                    strsql &= " select ( SELECT  ISNULL(MAX(dbar_recid),(SELECT MAX(dbar_recid) FROM stk04_detbarcode)) "
                    strsql &= " FROM Tmp_detbarcode" & gbUser & " ) + ROW_NUMBER() OVER ( ORDER BY MOD_RMRecid ) ,case when new_old='w'THEN Tmp_StkID else MOD_RMRecid END as MOD_RMRecid,"
                    ' strsql &= " case when new_old='w'THEN Tmp_StkID else MOD_RMRecid END as MOD_RMRecid, "
                    strsql &= " Fashion.GridCode,GD_SizeDesc,color, null , null ,  Tmp_Size" & i & " ,GETDATE(),'" & gbUser & "',GETDATE(),'" & gbUser & "'  "
                    strsql &= " From " & TmpGridDetails
                    strsql &= " LEFT JOIN TMP_" & TmpGridDetails & " ON Tmp_PendingID= MOD_RMRecid"
                    strsql &= " LEFT JOIN " & TmpFashQty & " Fashion on Fashion.Recid=" & TmpGridDetails & ".Recid"
                    strsql &= " LEFT JOIN  " & TmpGridBarcode & "F On Tmp_DRecid= " & TmpGridDetails & ".RECID "
                    strsql &= " LEFT JOIN stk04_DetGRID on GD_Code = GridCode "
                    strsql &= " where 1=1 and  Tmp_Stat <> 'D' and Tmp_OldNew" & i & "='N' and Tmp_Size" & i & " <>''  and Tmp_Size" & i & " is not null "
                    strsql &= " and  new_old<>'O' and GD_order='" & i & "'"
                    cmSql.CommandText = strsql
                    cmSql.ExecuteNonQuery()
                Next i
                strsql = " Insert into stk04_detbarcode SELECT * FROM Tmp_detbarcode" & gbUser
                cmSql.CommandText = strsql
                cmSql.ExecuteNonQuery()
            End If


            cmSql.CommandText = " SET NOCOUNT off "
            cmSql.ExecuteNonQuery()
            trnSql.Commit()

            CreatePoQCtemps(TempQcHeader, TempQcDetails)

            DropSqlObject("Tmp_" & TmpGridDetails, "TABLE")
            DropSqlObject("TempIns1_" & gbUser, "TABLE")

            Cursor.Current = Cursors.Default

            Dim MSG As String
            If Wtype = "ADD" Then
                MSG = "Record Successfully Added"
            ElseIf Wtype = "MODIFY" Then
                MSG = "Record Successfully Modified"
            ElseIf Wtype = "DELETE" Then

                MSG = "Record " & OldCode & " Successfully Canceled. A Reverse RO is Created to Cancel It"
            End If

            Dim frm As New frmMsg
            'If Wtype = "DELETE" Then
            '    frm.ShowDialog(MSG & " , DO # : ", OldCode, 38)
            'Else

            frm.ShowDialog(MSG & " , RO # : ", txtCode.Text.Trim, 38)
            ' End If
            frm = Nothing



            'If Wtype = "ADD" Then
            '    MessageBox.Show("Record Successfully Added", cntTitle, _
            '    MessageBoxButtons.OK, MessageBoxIcon.Information)
            'ElseIf Wtype = "MODIFY" Then
            '    MessageBox.Show("Record Successfully Modified", cntTitle, _
            '    MessageBoxButtons.OK, MessageBoxIcon.Information)
            'ElseIf Wtype = "DELETE" Then
            '    MessageBox.Show("Record Successfully Deleted", cntTitle, _
            '    MessageBoxButtons.OK, MessageBoxIcon.Information)
            'End If
            save = True

            If OldValue <> "" And Wtype <> "" Then
                AllLocks(AllLocksType.ReleaseLock, "stk04_PurDelOrdH", txtCode.Text.Trim, cntTitle, Me.Name)
            End If
            If OldVal <> "" Then
                AllLocks(AllLocksType.ReleaseLock, "STK04_mordH", OldVal, cntTitle, Me.Name)
            End If
            strsql = "delete from sysLockRecords where lr_Table = 'STK04_StkFilPending' and lr_Machine='" & gbPCName & "'"
            strsql &= " and lr_Application='" & gbApp & "' and lr_Menu='" & lcSteps & "'"
            Execute(strsql)

            If MessageBox.Show("Do You Want To Print Report ?", cntTitle, MessageBoxButtons.YesNo, MessageBoxIcon.Question) = MsgBoxResult.Yes Then
                RefreshControls()
                ChkExpiry.Checked = False
                ChkCost.Checked = False
                ChkBarcode.Checked = False
                ChkExpiry.Enabled = True
                ChkCost.Enabled = True
                ChkBarcode.Enabled = True
                btnSave.Enabled = False
                If gbExpiry Then ChkExpiry.Visible = True Else ChkExpiry.Visible = False
                If DelOrder = "Receiving Order Fashion" Or DelOrder = "Consignment Receiving Order Fashion" Then
                    ChkExpiry.Visible = False
                End If
                save = True
                ChkExpiry.Checked = False
                ChkCost.Checked = False
                ChkBarcode.Checked = False
                If Wtype <> "ADD" And txtCode.Text.Trim = "" Then MessageBox.Show("You Must Choose A Receiving Order !", cntTitle, MessageBoxButtons.OK, MessageBoxIcon.Warning) : Exit Sub
                Dim rb As String
                If DelOrder = "Receiving Order Fashion" Or DelOrder = "Consignment Receiving Order Fashion" Then
                    ChkExpiry.Visible = False
                End If
                If DelOrder = "Receiving Order Fashion" Or DelOrder = "Receiving Order" Then
                    ChkCost.Visible = False
                End If
                EnabledButtons(Me, False, btnCancel, btnPreview, btnClose)
                If txtCode.Text.Trim = "" Then MessageBox.Show("You Must Choose A Receiving Order !", cntTitle, MessageBoxButtons.OK, MessageBoxIcon.Warning) : Exit Sub
                ' Dim rb As String
                If Print.Visible = False Then
                    Print.Top = PnlButton.Top + btnPreview.Top - 129
                    Print.Left = btnSave.Left + 50
                    If Print.Width + Print.Left > Me.Width Then
                        Print.Left = Print.Left - 50
                    End If
                    Print.Visible = True
                    ChkCost.AutoCheck = True
                    ChkBarcode.AutoCheck = True
                    ChkExpiry.AutoCheck = True
                    ChkCost.Focus()
                    BtnOk.Enabled = True
                    btnSendToxls.Enabled = True

                    btnFashionRep.Visible = DelOrder.Contains("Fashion")
                    btnFashionRep.Enabled = DelOrder.Contains("Fashion")

                    Exit Sub
                    'Else
                    '    Print.Visible = False
                    '    If Wtype = "ADD" Or Wtype = "MODIFY" Then

                    '            btnSave.Enabled = True
                    '            ClearControls(Me, ErrorProvider1)
                    '            INITCOL(True)
                    '            InitGrids()
                    '            txtDate.Value = CDate(Now.Date)
                    '            txtActualDate.Value = CDate(Now.Date)
                    '            LstBarcode.Visible = False
                    '            LstExpiry.Visible = False

                    '            RefQtyLbl(True)
                    '            'lstDelivery.Refresh()
                    '            LstVisbility(False, False, lstDelivery.Columns("ItmClass").Text.Trim)
                    '            lstDelivery.Refresh()
                    '            lstDelivery.Col = 0
                    '            lstDelivery.Splits(0).HorizontalOffset = 0

                    '            btnSearch.Enabled = True : btnPending.Enabled = True : btnCreate.Enabled = True
                    '            If Wtype <> "ADD" Then txtDstmp.Text = "..." : txtUstmp.Text = "..."
                    '            If Wtype = "ADD" Then
                    '                With txtMode
                    '                    .Text = "Add a new record."
                    '                    .ForeColor = Color.Red
                    '                    .Image = btnAdd.Image
                    '                End With
                    '                With txtUstmp
                    '                    .Text = gbUser
                    '                    .ForeColor = Color.Red
                    '                End With
                    '                With txtDstmp
                    '                    .Text = Now.ToLongDateString
                    '                    .ForeColor = Color.Red
                    '                End With
                    '                ' EnabledButtons(Me, True, btnAdd, btnUndo, btnDelete, btnModify)
                    '                txtSupp.Focus()
                    '            Else : txtCode.Focus()
                    '            End If

                    '            btnSave.Enabled = True

                    '        LstVisbility(False, False, lstDelivery.Columns("ItmClass").Text.Trim)
                    '        lstDelivery.Refresh()
                    '        lstDelivery.Col = 0
                    '        lstDelivery.Splits(0).HorizontalOffset = 0
                    '        OldVal = ""
                    '            If Wtype = "MODIFY" Or Wtype = "ADD" Then
                    '                 EnabledButtons(Me, True, btnAdd, btnLoc, btnSeason, btnBrand, btnDelete, btnModify)
                    '                VisibleBtn(Me, True)
                    '                btnBrand.Enabled = True : btnSeason.Enabled = True : btnLoc.Enabled = True : btnPurchOrder.Enabled = True : btnSupp.Enabled = True

                    '            End If

                    '            If Wtype = "ADD" Then
                    '                EnabledButtons(Me, True, btnAdd, btnUndo, btnDelete, btnModify, btnPreview)
                    '                If OldValue <> "" And Wtype <> "" Then
                    '                    AllLocks(AllLocksType.ReleaseLock, "stk04_PurDelOrdH", txtCode.Text.Trim, cntTitle, Me.Name)
                    '                End If
                    '                If OldVal <> "" Then
                    '                    AllLocks(AllLocksType.ReleaseLock, "STK04_mordH", OldVal, cntTitle, Me.Name)
                    '                End If
                    '                strsql = "delete from sysLockRecords where lr_Table = 'STK04_StkFilPending' and lr_Machine='" & gbPCName & "'"
                    '                strsql &= " and lr_Application='" & gbApp & "' and lr_Menu='" & lcSteps & "'"
                    '                Execute(strsql)
                    '                With txtDstmp
                    '                    .Text = Now.ToLongDateString
                    '                    .ForeColor = Color.Red
                    '                End With
                    '                VisibleBtn(Me, True) : txtSupp.Focus()
                    '            Else
                    '                txtCode.Focus()
                    '            End If
                    '    End If
                End If


            Else
                save = False
            End If
            If Wtype = "ADD" Then
                btnAdd_Click(btnAdd, New System.EventArgs)
            ElseIf Wtype = "MODIFY" Then
                btnModify_Click(btnModify, New System.EventArgs)
            ElseIf Wtype = "DELETE" Then
                btnDelete_Click(btnDelete, New System.EventArgs)
            End If


            btnSave.Enabled = True
            'ClearControls(Me, ErrorProvider1)
            'INITCOL(True)
            'InitGrids()
            'txtDate.Value = CDate(Now.Date)
            'txtActualDate.Value = CDate(Now.Date)
            'LstBarcode.Visible = False
            'LstExpiry.Visible = False

            'RefQtyLbl(True)
            ''lstDelivery.Refresh()
            'LstVisbility(False, False, lstDelivery.Columns("ItmClass").Text.Trim)
            'lstDelivery.Refresh()
            'lstDelivery.Col = 0
            'lstDelivery.Splits(0).HorizontalOffset = 0
            'ClearGridFiltration(lstDelivery)

            'OldVal = ""
            'If DelOrder = "Consignment Receiving Order" Or DelOrder = "Consignment Receiving Order Fashion" Then
            '    PnlConsigment.Visible = True
            '    btnPending.Enabled = False
            'End If
            '    If Wtype = "MODIFY" Or Wtype = "ADD" Then
            '        btnLoc.Enabled = True
            '        VisibleBtn(Me, True) : txtDate.Value = DBNull.Value : txtActualDate.Value = DBNull.Value : btnLoc.Enabled = False
            '        btnBrand.Enabled = True : btnSeason.Enabled = True : btnLoc.Enabled = True : btnPurchOrder.Enabled = True : btnSupp.Enabled = True

            '    End If
            'lcChangeKey = False : lcChanges = False
            If Wtype <> "ADD" Then txtDstmp.Text = "..." : txtUstmp.Text = "..."
            If Wtype = "ADD" Then
                With txtDstmp
                    .Text = Now.ToLongDateString
                    .ForeColor = Color.Red
                End With
                VisibleBtn(Me, True) : txtSupp.Focus()
            Else
                txtCode.Focus()
            End If

            lcChangeKey = False
            lcChanges = False


        Catch exp As Exception
            Try
                If Not trnSql Is Nothing Then trnSql.Rollback()
            Catch ex1 As Exception
            End Try

            DropSqlObject("Tmp_" & TmpGridDetails, "TABLE")
            If exp.Message.IndexOf("The DELETE statement conflicted with the REFERENCE constraint") >= 0 Then
                MessageBox.Show("Attention:" & vbCrLf & "This Record Has Other Related Record(s), You Can't Delete It.", _
                cntTitle, MessageBoxButtons.OK, MessageBoxIcon.Exclamation)
            ElseIf exp.Message.IndexOf("Violation of UNIQUE KEY constraint 'IX_stk04_PurDelOrdH'. ") >= 0 Then
                MessageBox.Show("Attention:" & vbCrLf & "Same DO # and DO Ref # Already Exist , Change Them To Save.", _
                cntTitle, MessageBoxButtons.OK, MessageBoxIcon.Exclamation)
                txtDelOrder.Focus()
                'Rita M @ 31.03.2014
            ElseIf exp.Message.IndexOf("Violation of UNIQUE KEY constraint 'UK_STK04_STKFIL'") >= 0 Then
                MessageBox.Show("Attention:" & vbCrLf & "Single Reference 1 Exist For Another Item.", _
                cntTitle, MessageBoxButtons.OK, MessageBoxIcon.Exclamation)
            ElseIf exp.Message.IndexOf("Violation of UNIQUE KEY constraint 'UK_STK04_STKFIL2'") >= 0 Then
                MessageBox.Show("Attention:" & vbCrLf & "Single Reference 2 Exist For Another Item.", _
                cntTitle, MessageBoxButtons.OK, MessageBoxIcon.Exclamation)
            ElseIf Not DuplicateKey(exp, cntTitle) Then
                MessageBox.Show("Error : " & exp.Message, cntTitle, _
                MessageBoxButtons.OK, MessageBoxIcon.Error)
            End If
        Finally
            If Not cnSql Is Nothing Then
                If cnSql.State = ConnectionState.Open Then cnSql.Close()
                cnSql = Nothing
            End If
            cmSql = Nothing : cmsql2 = Nothing : trnSql = Nothing
            'ch @ 06/10/2016 
            btnSave.Enabled = True
        End Try
    End Sub

    Private Sub btnPreview_Click(ByVal sender As System.Object, ByVal e As System.EventArgs) Handles btnPreview.Click


        Try
            ChkExpiry.Checked = False
            ChkCost.Checked = False
            ChkTechContent.Checked = False
            ChkBarcode.Checked = False
            ChkExpiry.Enabled = True
            ChkCost.Enabled = True
            ChkBarcode.Enabled = True
            rbPrintLatin.Enabled = True
            rbPrintArabic.Enabled = True
            If DelOrder.Contains("Fashion") = True Then
                chkPrintQC.Visible = False
                rbPrintArabic.Visible = False
                rbPrintLatin.Visible = False
            Else
                chkPrintQC.Visible = True
                rbPrintArabic.Visible = True
                rbPrintLatin.Visible = True
            End If
            If gbExpiry Then ChkExpiry.Visible = True Else ChkExpiry.Visible = False
            If DelOrder = "Receiving Order Fashion" Or DelOrder = "Consignment Receiving Order Fashion" Or gbExpiry = False Then
                ChkExpiry.Visible = False
            End If
            If DelOrder = "Receiving Order Fashion" Or DelOrder = "Receiving Order" Then
                ChkCost.Visible = False
            End If
            If txtCode.Text.Trim = "" Then MessageBox.Show("You Must Choose A Receiving Order !", cntTitle, MessageBoxButtons.OK, MessageBoxIcon.Warning) : Exit Sub
            Dim rb As String
            If Print.Visible = False Then
                Print.Top = PnlButton.Top + btnPreview.Top - 129
                Print.Left = btnSave.Left + 50
                If Print.Width + Print.Left > Me.Width Then
                    Print.Left = Print.Left - 50
                End If
                Print.Visible = True
                ChkCost.AutoCheck = True
                ChkBarcode.AutoCheck = True
                ChkExpiry.AutoCheck = True
                '   EnabledButtons(Me, False, btnPreview, btnClose)
                BtnOk.Enabled = True
                BtnOk.Focus()
                Exit Sub
            Else
                Print.Visible = False
                'If Wtype = "MODIFY" Then
                '    EnabledButtons(Me, True, btnAdd, btnLoc, btnSeason, btnBrand, btnDelete, btnModify)
                'ElseIf Wtype = "DELETE" Then
                '    EnabledButtons(Me, False, btnSearch, btnCancel, btnSave, btnClose)
                'ElseIf Wtype = "" Then
                '    EnabledButtons(Me, False, btnDelete, btnAdd, btnModify, btnSearch, btnClose, btnPreview)
                'End If
                If save = False Then Exit Sub
                If Wtype = "ADD" Then
                    btnAdd_Click(btnAdd, New System.EventArgs)
                ElseIf Wtype = "MODIFY" Then
                    btnModify_Click(btnModify, New System.EventArgs)
                ElseIf Wtype = "DELETE" Then
                    btnDelete_Click(btnDelete, New System.EventArgs)
                End If
                'If Wtype = "ADD" Or Wtype = "MODIFY" Then
                '    btnSave.Enabled = True
                '    ClearControls(Me, ErrorProvider1)
                '    INITCOL(True)
                '    InitGrids()
                '    txtDate.Value = CDate(Now.Date)
                '    txtActualDate.Value = CDate(Now.Date)
                '    LstBarcode.Visible = False
                '    LstExpiry.Visible = False

                '    RefQtyLbl(True)
                '    'lstDelivery.Refresh()
                '        LstVisbility(False, False, lstDelivery.Columns("ItmClass").Text.Trim)
                '        btnSearch.Enabled = True : btnPending.Enabled = True : btnCreate.Enabled = True : btnStkSearch.Enabled = True
                '    lstDelivery.Refresh()
                '    lstDelivery.Col = 0
                '    lstDelivery.Splits(0).HorizontalOffset = 0
                '    OldVal = ""
                '        If Wtype = "MODIFY" Or Wtype = "ADD" Then
                '            EnabledButtons(Me, True, btnAdd, btnLoc, btnSeason, btnBrand, btnDelete, btnModify)
                '            VisibleBtn(Me, True)
                '            btnBrand.Enabled = True : btnSeason.Enabled = True : btnLoc.Enabled = True : btnPurchOrder.Enabled = True : btnSupp.Enabled = True

                '        End If

                '        If Wtype <> "ADD" Then txtDstmp.Text = "..." : txtUstmp.Text = "..."
                '        If Wtype = "ADD" Then
                '            EnabledButtons(Me, True, btnAdd, btnUndo, btnDelete, btnModify, btnPreview)
                '            With txtMode
                '                .Text = "Add a new record."
                '                .ForeColor = Color.Red
                '                .Image = btnAdd.Image
                '            End With
                '            With txtUstmp
                '                .Text = gbUser
                '                .ForeColor = Color.Red
                '            End With
                '            With txtDstmp
                '                .Text = Now.ToLongDateString
                '                .ForeColor = Color.Red
                '            End With
                '            ' EnabledButtons(Me, True, btnAdd, btnUndo, btnDelete, btnModify)
                '            txtSupp.Focus()
                '        Else : txtCode.Focus()
                '        End If

                '    ElseIf Wtype = "DELETE" Then
                '        ClearControls(Me, ErrorProvider1)
                '        INITCOL(True)
                '        InitGrids()
                '        btnPreview.Enabled = lcPrivPrint
                '        txtCode.Focus()
                '    End If
                '    lcChanges = False
                '    lcChangeKey = False
            End If
            If DelOrder.Contains("Fashion") = True Then
                chkPrintQC.Visible = False
                rbPrintArabic.Visible = False
                rbPrintLatin.Visible = False
            Else
                chkPrintQC.Visible = True
                rbPrintArabic.Visible = True
                rbPrintLatin.Visible = True
            End If
            BtnOk.Focus()
        Catch ex As Exception
            MessageBox.Show("Error:" & vbCrLf & ex.Message, cntTitle, _
            MessageBoxButtons.OK, MessageBoxIcon.Error)
        End Try

    End Sub
    Private Sub BtnOk_Click(ByVal sender As System.Object, ByVal e As System.EventArgs) Handles BtnOk.Click
        Dim Code As String
        Dim reportN As String
        Try
            Print.Visible = False
            btnClose.Enabled = False
            Cursor.Current = Cursors.WaitCursor
            btnPreview.Enabled = False
            Code = "('" & txtCode.Text.Trim.Replace("'", "''") & "')"

            PrintRO(Me, cntTitle, DelOrder, TmpGridDetails, TmpFashQty, TmpGridBarcode, Code, txtCurr.Text.Trim.Replace("'", "''"), ChkBarcode.Checked, _
                    ChkCost.Checked, ChkExpiry.Checked, ChkTechContent.Checked, chkPrintQC.Checked, rbPrintLatin.Checked, MaxCnt)


            'If chkPrintQC.Checked = True Then
            '    PrintQCAnswers(txtCode.Text.Trim)
            'End If
            If save = True Then
                If Wtype = "ADD" Then
                    btnAdd_Click(btnAdd, New System.EventArgs)
                ElseIf Wtype = "MODIFY" Then
                    btnModify_Click(btnModify, New System.EventArgs)
                ElseIf Wtype = "DELETE" Then
                    btnDelete_Click(btnDelete, New System.EventArgs)
                End If
                'If Wtype = "ADD" Or Wtype = "MODIFY" Then
                '    If OldValue <> "" And Wtype <> "" Then
                '        AllLocks(AllLocksType.ReleaseLock, "stk04_PurDelOrdH", txtCode.Text.Trim, cntTitle, Me.Name)
                '    End If
                '    If OldVal <> "" Then
                '        AllLocks(AllLocksType.ReleaseLock, "STK04_mordH", OldVal, cntTitle, Me.Name)
                '    End If
                '    strsql = "delete from sysLockRecords where lr_Table = 'STK04_StkFilPending' and lr_Machine='" & gbPCName & "'"
                '    strsql &= " and lr_Application='" & gbApp & "' and lr_Menu='" & lcSteps & "'"
                '    Execute(strsql)
                '    btnSave.Enabled = True
                '    ClearControls(Me, ErrorProvider1)
                '    INITCOL(True)
                '    InitGrids()
                '    txtDate.Value = CDate(Now.Date)
                '    txtActualDate.Value = CDate(Now.Date)
                '    LstBarcode.Visible = False
                '    LstExpiry.Visible = False

                '    RefQtyLbl(True)
                '    'lstDelivery.Refresh()
                '    LstVisbility(False, False, lstDelivery.Columns("ItmClass").Text.Trim)
                '    lstDelivery.Refresh()
                '    lstDelivery.Col = 0
                '    lstDelivery.Splits(0).HorizontalOffset = 0
                '    OldVal = ""
                '    If Wtype = "MODIFY" Or Wtype = "ADD" Then
                '         btnLoc.Enabled = True
                '        VisibleBtn(Me, True) : txtDate.Value = DBNull.Value : txtActualDate.Value = DBNull.Value : btnLoc.Enabled = False
                '        btnBrand.Enabled = True : btnSeason.Enabled = True : btnLoc.Enabled = True : btnPurchOrder.Enabled = True : btnSupp.Enabled = True

                '    End If

                '    If DelOrder = "Consignment Receiving Order" Or DelOrder = "Consignment Receiving Order Fashion" Then
                '        If Wtype = "ADD" Then
                '            btnCurr.Enabled = True
                '            txtCurr.ReadOnly = False
                '        End If
                '    End If

                '    If Wtype <> "ADD" Then txtDstmp.Text = "..." : txtUstmp.Text = "..."
                '    If Wtype = "ADD" Then
                '        With txtDstmp
                '            .Text = Now.ToLongDateString
                '            .ForeColor = Color.Red
                '        End With
                '        VisibleBtn(Me, True) : txtSupp.Focus()
                '    Else
                '        txtCode.Focus()
                '    End If
                '    lcChanges = False
                'ElseIf Wtype = "DELETE" Then
                '    ClearControls(Me, ErrorProvider1)
                '    INITCOL(True)
                '    InitGrids()
                '    EnabledButtons(Me, False, btnSearch, btnCancel, btnSave, btnClose)
                '    btnPreview.Enabled = lcPrivPrint
                '    txtCode.Focus()
                'End If
                'lcChanges = False
                'save = False
                'Exit Sub
            End If

            save = False
            btnSave.Enabled = True
            'btnPreview_Click(btnPreview, New System.EventArgs)
        Catch ex As Exception
            MessageBox.Show("General Error : " & ex.Message, cntTitle, MessageBoxButtons.OK, MessageBoxIcon.Error)
        Finally

            btnPreview.Enabled = True
            Cursor.Current = Cursors.Default
            btnClose.Enabled = True
        End Try
    End Sub
    Private Sub ChkBarcode_Click(ByVal sender As Object, ByVal e As System.EventArgs) Handles ChkBarcode.Click
        Try
            If DelOrder = "Consignment Receiving Order Fashion" And ChkBarcode.Checked = True Then
                ChkCost.Checked = False : ChkCost.Enabled = False
            Else
                ChkCost.Enabled = True
            End If
        Catch ex As Exception
            MessageBox.Show("Error: " & ex.Message, cntTitle, _
            MessageBoxButtons.OK, MessageBoxIcon.Error)
        End Try
    End Sub

    Private Sub ChkExpiry_Click(ByVal sender As Object, ByVal e As System.EventArgs) Handles ChkExpiry.Click
        Try
            If (DelOrder = "Consignment Receiving Order" Or DelOrder = "Receiving Order") And ChkExpiry.Checked = True Then
                ChkBarcode.Checked = False : ChkBarcode.Enabled = False
            Else
                ChkBarcode.Enabled = True
            End If
        Catch ex As Exception
            MessageBox.Show("Error: " & ex.Message, cntTitle, _
            MessageBoxButtons.OK, MessageBoxIcon.Error)
        End Try
    End Sub
    Private Sub btnStatus(ByVal TxtStatus As TextBox, ByVal FrmOwner As Form)
        Dim frm As frmSearch
        Dim tblRow(2, 1) As String
        Dim Sizefield() As Int16 = {50, 150}
        Dim Alnfield() As String = {"C", "N"}
        Dim Retfield() As String = {"Code", "Status"}

        Try
            If Not DtQualityControlHeader Is Nothing AndAlso txtRecid.Text.Trim <> "" Then
                If DtQualityControlHeader.Select("PrDReciD =  " & txtRecid.Text & " ").Count <> 0 Then

                    MessageBox.Show("This Record Is linked To A Quality Control , Status Can't Be Changed ", cntTitle, MessageBoxButtons.OK, MessageBoxIcon.Information)
                    Exit Try
                End If
            End If
            Cursor.Current = Cursors.WaitCursor
            tblRow(0, 0) = "F" : tblRow(0, 1) = "Free"
            tblRow(1, 0) = "N" : tblRow(1, 1) = "Normal"
            tblRow(2, 0) = "R" : tblRow(2, 1) = "Return"

            frm = New frmSearch

            With frm
                .Owner = Me
                If .ShowDialog("Return", CreateDataTable("tblStat", "Code", "Status", tblRow), 2, _
                    "Status", Sizefield, Alnfield, Retfield, 0, Nothing, False, 250, 200, , True) = DialogResult.OK Then

                    If gbRetSearch(0) <> "" Then
                        TxtStatus.Text = gbRetSearch(0)
                    End If
                    TxtStatus.Focus()
                    SendKeys.Send("{TAB}")
                Else
                    TxtStatus.Focus()
                End If
            End With
            lstDelivery.UpdateData()
            daDetDetails.Update(dsDetDetails.Tables("tblDetails"))
        Catch ex As Exception
            MessageBox.Show("Error: " & ex.Message, cntTitle, MessageBoxButtons.OK, MessageBoxIcon.Error)
        Finally
            gbRetSearch = Nothing
            frm = Nothing
            Retfield = Nothing
            Alnfield = Nothing
            Sizefield = Nothing
            Cursor.Current = Cursors.Default
        End Try
    End Sub

    Private Sub InitGrids()
        Try
            strsql = "delete from " & TmpGridDetails
            strsql &= " delete from " & TempPendingDO
            Execute(strsql)
            dsDetDetails.Tables(0).Clear()
            daDetDetails.Fill(dsDetDetails, dsDetDetails.Tables(0).TableName)
            For Idx As Integer = 0 To lstDelivery.Columns.Count - 1
                lstDelivery.Columns(Idx).FilterText = ""
            Next
            lstDelivery.SelectedRows.Clear()
            'lstDelivery.ClearFields()
            If DelOrder = "Receiving Order" Or DelOrder = "Consignment Receiving Order" Then
                strsql = "delete from " & TmpGridExpiry
                Execute(strsql)
                dsDetExpiry.Tables(0).Clear()
                daDetExpiry.Fill(dsDetExpiry, dsDetExpiry.Tables(0).TableName)
                For Idx As Integer = 0 To LstExpiry.Columns.Count - 1
                    LstExpiry.Columns(Idx).FilterText = ""
                Next
                LstExpiry.SelectedRows.Clear()
                'LstExpiry.ClearFields()

                strsql = "delete from " & TmpGridBarcode
                Execute(strsql)
                dsDetBarcode.Tables(0).Clear()
                daDetBarcode.Fill(dsDetBarcode, dsDetBarcode.Tables(0).TableName)
                For Idx As Integer = 0 To LstBarcode.Columns.Count - 1
                    LstBarcode.Columns(Idx).FilterText = ""
                Next
                LstBarcode.SelectedRows.Clear()
            ElseIf DelOrder = "Receiving Order Fashion" Or DelOrder = "Consignment Receiving Order Fashion" Then
                lstDelivery.Columns("Color").FooterText = ""
                strsql = "delete from " & TmpGridBarcode & "F"
                Execute(strsql)
                If Not dsDetBarcodeF.Tables("tblDetails") Is Nothing Then

                    dsDetBarcodeF.Tables(0).Clear()
                    daDetBarcodeF.Fill(dsDetBarcodeF, dsDetBarcodeF.Tables(0).TableName)
                    For Idx As Integer = 0 To LstBarcodeF.Columns.Count - 1
                        LstBarcodeF.Columns(Idx).FilterText = ""
                    Next
                End If

                LstBarcodeF.SelectedRows.Clear()

                strsql = "delete from " & TmpFashQty
                Execute(strsql)
                If Not dsDetFqty.Tables("tblDetails") Is Nothing Then

                    dsDetFqty.Tables(0).Clear()
                    daDetFqty.Fill(dsDetFqty, dsDetFqty.Tables(0).TableName)
                    For Idx As Integer = 0 To lstFashQty.Columns.Count - 1
                        lstFashQty.Columns(Idx).FilterText = ""
                    Next
                End If

                lstFashQty.SelectedRows.Clear()
            End If
            lcChanges = False
            If DelOrder = "Receiving Order" Or DelOrder = "Consignment Receiving Order" Then
                ClearGridFiltration(LstBarcode)
                ClearGridFiltration(LstExpiry)
            Else
                ClearGridFiltration(LstBarcodeF)
            End If
            ReadonlyConsFash()

            LstExpiry.Visible = False
            LstBarcode.Visible = False
            BarcodeF.Visible = False
            Print.Visible = False
            BarcodeRed = False
            ExpiryRed = False
            lstDelivery.Splits(0).DisplayColumns("MOD_OrdQty").DataColumn.FooterText = CType(0, Double).ToString("N" & gbDecNo)
            If DelOrder = "Consignment Receiving Order" Or DelOrder = "Consignment Receiving Order Fashion" Then
                lstDelivery.Splits(0).DisplayColumns("Net").DataColumn.FooterText = CType(0, Double).ToString("N3")
            End If
        Catch ex As Exception
            Throw ex
        End Try
    End Sub

    Private Function VerifyGridData() As Boolean
        Dim drs() As DataRow
        Try

            '**Verify Item Recid
            drs = dsDetDetails.Tables("tblDetails").Select("ISNULL(MOD_supitmref,'') = '' ")
            If drs.Length > 0 Then
                MessageBox.Show("Supplier Reference Cannot Be Empty", cntTitle, MessageBoxButtons.OK, MessageBoxIcon.Exclamation)
                lstDelivery.Focus()
                VerifyGridData = False
                Exit Try
            End If
            drs = dsDetDetails.Tables("tblDetails").Select("ISNULL(MOD_RMRecid,0) = 0 ")
            If drs.Length > 0 Then
                MessageBox.Show("Item Code Cannot Be Empty", cntTitle, MessageBoxButtons.OK, MessageBoxIcon.Exclamation)
                lstDelivery.Focus()
                VerifyGridData = False
                Exit Try
            End If
            drs = dsDetDetails.Tables("tblDetails").Select("ISNULL(MOD_OrdQty,0) = 0  ")
            If drs.Length > 0 Then
                MessageBox.Show("Qty Cannot Be Empty Or 0 ", cntTitle, MessageBoxButtons.OK, MessageBoxIcon.Exclamation)
                lstDelivery.Focus()
                VerifyGridData = False
                Exit Try
            End If
            drs = dsDetDetails.Tables("tblDetails").Select("ISNULL(cost,0) < 0  ")
            If drs.Length > 0 Then
                MessageBox.Show("Cost Should Be Greater Than 0", cntTitle, MessageBoxButtons.OK, MessageBoxIcon.Exclamation)
                lstDelivery.Focus()
                VerifyGridData = False
                Exit Try
            End If
            If DelOrder = "Consignment Receiving Order" Or DelOrder = "Consignment Receiving Order Fashion" Then
                drs = dsDetDetails.Tables("tblDetails").Select("ISNULL(disc,0) < 0  ")
                If drs.Length > 0 Then
                    MessageBox.Show("Disc Should Be Greater Than 0", cntTitle, MessageBoxButtons.OK, MessageBoxIcon.Exclamation)
                    lstDelivery.Focus()
                    VerifyGridData = False
                    Exit Try
                End If
                drs = dsDetDetails.Tables("tblDetails").Select("ISNULL(disc,0) > 100  ")
                If drs.Length > 0 Then
                    MessageBox.Show("Disc Should Be Less Than 100", cntTitle, MessageBoxButtons.OK, MessageBoxIcon.Exclamation)
                    lstDelivery.Focus()
                    VerifyGridData = False
                    Exit Try
                End If
            End If
            '**Verify 0 Qty not expiry
            If Not DtQualityControlHeader Is Nothing AndAlso DtQualityControlHeader.Select("  1=1 ").Count > 0 Then
                If DtQualityControlHeader.Select(" isnull(AccQty,0) = 0 AND isnull(RjQty,0)= 0 ").Count <> 0 Then
                    drs = dsDetDetails.Tables("tblDetails").Select("ISNULL(MOD_ORDQTY,0) = 0 and itmclass<>'E'")

                    If drs.Length > 0 Then
                        MessageBox.Show("Quantity Can't Be 0 ", cntTitle, MessageBoxButtons.OK, MessageBoxIcon.Exclamation)
                        lstDelivery.Focus()
                        VerifyGridData = False
                        Exit Try
                    End If
                End If
            Else
                drs = dsDetDetails.Tables("tblDetails").Select("ISNULL(MOD_ORDQTY,0) = 0 and itmclass<>'E'")

                If drs.Length > 0 Then
                    MessageBox.Show("Quantity Can't Be 0 ", cntTitle, MessageBoxButtons.OK, MessageBoxIcon.Exclamation)
                    lstDelivery.Focus()
                    VerifyGridData = False
                    Exit Try
                End If

            End If


            If DelOrder = "Receiving Order Fashion" Or DelOrder = "Consignment Receiving Order Fashion" Then

                '**Verify supplier Color is empty
                drs = dsDetDetails.Tables("tblDetails").Select("ISNULL(SuppColor,'') = '' and itmclass='F'")
                If drs.Length > 0 Then
                    MessageBox.Show("Supp. Color Couldn't be Null.", cntTitle, MessageBoxButtons.OK, MessageBoxIcon.Exclamation)
                    lstDelivery.Focus()
                    VerifyGridData = False
                    Exit Try
                End If

                '**Verify Gender is empty
                drs = dsDetDetails.Tables("tblDetails").Select("ISNULL(STK_Gender,'') = '' and itmclass='F'")
                If drs.Length > 0 Then
                    MessageBox.Show("Gender Couldn't be Null.", cntTitle, MessageBoxButtons.OK, MessageBoxIcon.Exclamation)
                    lstDelivery.Focus()
                    VerifyGridData = False
                    Exit Try
                End If

                '**Verify Color is empty
                drs = dsDetDetails.Tables("tblDetails").Select("ISNULL(Color,'') = '' and itmclass='F'")
                If drs.Length > 0 Then
                    MessageBox.Show("Color Couldn't be Null.", cntTitle, MessageBoxButtons.OK, MessageBoxIcon.Exclamation)
                    lstDelivery.Focus()
                    VerifyGridData = False
                    Exit Try
                End If

            End If
            '**Verify 0 Qty  Expiry items 
            If Not DtQualityControlHeader Is Nothing AndAlso DtQualityControlHeader.Select("  1=1 ").Count > 0 Then
                If DtQualityControlHeader.Select(" isnull(AccQty,0) = 0 AND isnull(RjQty,0)= 0 ").Count <> 0 Then
                    drs = dsDetDetails.Tables("tblDetails").Select(" itmclass='E'")
                    If drs.Length > 0 Then
                        strsql = "SELECT  * FROM  " & TmpGridDetails & " LEFT JOIN  " & TmpGridExpiry
                        strsql &= " ON dbo." & TmpGridDetails & ".RECID = dbo." & TmpGridExpiry & ".RECID"
                        strsql &= " WHERE   ITMCLASS='E'  AND(E_RECID IS NULL or " & TmpGridExpiry & ".MOD_OrdQty is null) AND ISNULL(exp_staT,'') <> 'D'"
                        If RecordsExists(strsql) = True Then
                            MessageBox.Show("Expiry Quantity Cannot Be 0 ", cntTitle, MessageBoxButtons.OK, MessageBoxIcon.Exclamation)
                            GrpBarcodeButtons.Visible = True
                            BtnGenerateBarcodeTrading.Enabled = True
                            BtnOKExpiryBarcode.Enabled = True
                            LstVisbility(True, True, lstDelivery.Columns("ItmClass").Text.Trim)
                            LstExpiry.Focus()
                            VerifyGridData = False
                            Exit Try
                        End If
                    End If
                End If
            Else
                drs = dsDetDetails.Tables("tblDetails").Select(" itmclass='E'")
                If drs.Length > 0 Then
                    strsql = "SELECT  * FROM  " & TmpGridDetails & " LEFT JOIN  " & TmpGridExpiry
                    strsql &= " ON dbo." & TmpGridDetails & ".RECID = dbo." & TmpGridExpiry & ".RECID"
                    strsql &= " WHERE   ITMCLASS='E'  AND (E_RECID IS NULL or " & TmpGridExpiry & ".MOD_OrdQty is null) AND ISNULL(exp_staT,'') <> 'D'"
                    If RecordsExists(strsql) = True Then
                        MessageBox.Show("Expiry Quantity Cannot Be 0 ", cntTitle, MessageBoxButtons.OK, MessageBoxIcon.Exclamation)
                        GrpBarcodeButtons.Visible = True
                        BtnGenerateBarcodeTrading.Enabled = True
                        BtnOKExpiryBarcode.Enabled = True
                        LstVisbility(True, True, lstDelivery.Columns("ItmClass").Text.Trim)
                        LstExpiry.Focus()
                        VerifyGridData = False
                        Exit Try
                    End If
                End If
            End If





            VerifyGridData = True

        Catch ex As Exception
            Throw ex
        End Try
    End Function


    Private Sub RefreshControls(Optional ByVal Key As Boolean = False)
        Dim strsql As String
        Dim drSql As SqlDataReader
        Dim Code As String
        Try
            refreshControl = False
            For Idx As Integer = 0 To lstDelivery.Columns.Count - 1
                lstDelivery.Columns(Idx).FilterText = ""
            Next
            For Idx As Integer = 0 To LstBarcode.Columns.Count - 1
                LstBarcode.Columns(Idx).FilterText = ""
            Next
            For Idx As Integer = 0 To LstExpiry.Columns.Count - 1
                LstExpiry.Columns(Idx).FilterText = ""
            Next

            ClearGridFiltration(lstDelivery)

            lstDelivery.Refresh()
            lstDelivery.Col = 0
            lstDelivery.Splits(0).HorizontalOffset = 0

            strsql = " SELECT *  , case when DOH_status='N' then 'Normal' else 'Reverse' end as [Status] FROM Stk04_PurDelOrdH"
            strsql &= " LEFT JOIN stk04_supplier ON SUP_RECID =DOH_SupRecid  "
            strsql &= " LEFT JOIN  STK04_Location ON DOH_loc=Loc_Code"
            If DelOrder = "Receiving Order Fashion" Or DelOrder = "Consignment Receiving Order Fashion" Then
                strsql &= " LEFT JOIN stk04_brand on  br_Code=DOH_Brand"
                strsql &= " LEFT JOIN Stk04_Season On  Seas_Code=DOH_Season"
            End If
            If Key = False Then
                strsql &= " where DOH_Recid=" & Val(txtCode.Text.Trim)
            Else
                strsql &= " where DOH_SupRecid=" & GetFieldDesc("Stk04_Supplier", "sup_code = '" & txtSupp.Text.Trim.Replace("'", "''") & "'", "Sup_RecId")
                strsql &= " and DOH_Date='" & CDate(txtDate.Value).ToString("MM/dd/yyyy") & "' and DOH_Ref='" & txtDelOrder.Text.Trim.Replace("'", "''") & "'"
            End If
            'In modify mode, forbid the user to modify an RO that was purchased
            If Wtype = "MODIFY" OrElse Wtype = "DELETE" Then strsql &= " AND DOH_PurRecid IS null"
            ReadFromDBToDataReader(strsql, drSql)
            With drSql
                If .HasRows Then
                    .Read()

                    ClearControls(Me, ErrorProvider1)
                    INITCOL(True)
                    InitGrids()
                    refreshControl = False
                    txtCode.Text = .Item("DOH_Recid").ToString
                    OldValue = .Item("DOH_Recid").ToString
                    lblStatus.Text = .Item("Status").ToString
                    If UCase(gbUser) <> "SA" And UCase(gbUser) <> "SU" Then
                        'test on  supplier privileges
                        strsql = "SELECT BUMP_SuppCode FROM dbo.Stk04_BusUnitManagPriv"
                        strsql &= " WHERE BUMP_UserCode='" & gbUser & "' AND BUMP_Access='1' AND BUMP_SuppCode='" & .Item("Sup_Code").ToString.Replace("'", "''") & "'"
                        If RecordsExists(strsql) = False Then
                            MessageBox.Show("You Don’t Have Privilege For The Supplier " & .Item("Sup_Name").ToString, cntTitle, MessageBoxButtons.OK, MessageBoxIcon.Error)
                            txtCode.Focus()
                            Exit Sub
                        End If

                        'Test if user can access this Brand
                        If DelOrder = "Receiving Order Fashion" Or DelOrder = "Consignment Receiving Order Fashion" Then
                            strsql = "SELECT BUMP_SuppCode FROM dbo.Stk04_BusUnitManagPriv"
                            strsql &= " WHERE BUMP_UserCode='" & gbUser & "' AND BUMP_Access='1' AND BUMP_BrandCode='" & .Item("br_Code").ToString.Replace("'", "''") & "'"
                            If RecordsExists(strsql) = False Then
                                MessageBox.Show("You Don’t Have Privilege For The Brand " & .Item("Br_Name").ToString, cntTitle, MessageBoxButtons.OK, MessageBoxIcon.Error)
                                txtCode.Focus()
                                Exit Sub
                            End If
                        End If
                    End If



                    If Wtype <> "" Then
                        If AllLocks(AllLocksType.CreateLock, "Stk04_PurDelOrdH", OldValue, cntTitle, Me.Name, cntTitle) = False Then
                            txtCode.Focus()
                            Exit Sub
                        End If
                    End If


                    '06.02.2014 Lock the pending items in modify mode
                    Dim Str As String
                    Dim drOldPend As SqlDataReader
                    Str = "SELECT SFP_OldRecid FROM " & TmpGridDetails & " inner join " & TempPendingDO & "  on SFP_Recid = MOD_RMRecid  WHERE new_old='w'"
                    ReadFromDBToDataReader(Str, drOldPend)
                    If drOldPend.HasRows Then
                        While drOldPend.Read
                            AllLocks(AllLocksType.ReleaseLock, "STK04_StkFilPending", drOldPend.Item(0).ToString, cntTitle, lcSteps)
                        End While
                    End If

                    txtSupp.Text = .Item("Sup_Code").ToString
                    lblSupp.Text = .Item("Sup_Name").ToString
                    txtCurr.Text = .Item("DOH_Curr").ToString
                    '   LblCostCurr.Text = txtCurr.Text
                    txtDelOrder.Text = .Item("DOH_Ref").ToString
                    If .Item("DOH_Date").ToString = "" Then
                        txtDate.Value = DBNull.Value
                    Else
                        txtDate.Value = CDate(.Item("DOH_Date")).ToString("dd/MM/yyyy")
                    End If
                    txtOrderNo.Text = .Item("DOH_PORecid").ToString
                    OldVal = txtOrderNo.Text
                    txtLoc.Text = .Item("Loc_Code").ToString
                    lblLoc.Text = .Item("Loc_Desc").ToString
                    If .Item("DOH_ActDelDate").ToString = "" Then
                        txtActualDate.Value = DBNull.Value
                    Else
                        txtActualDate.Value = CDate(.Item("DOH_ActDelDate")).ToString("dd/MM/yyyy")
                    End If

                    If Wtype = "MODIFY" AndAlso vatdate.Trim <> "" AndAlso Not txtActualDate.Value Is DBNull.Value AndAlso CDate(txtActualDate.Value) < CDate(vatdate) Then
                        MessageBox.Show(DelOrder & " Date Should Be Greater Than Or Equal " & vatdate, Me.Text, MessageBoxButtons.OK, MessageBoxIcon.Information)
                        ClearControls(Me, ErrorProvider1)
                        Exit Try
                    ElseIf Wtype = "MODIFY" AndAlso vatdate.Trim <> "" AndAlso Not txtDate.Value Is DBNull.Value AndAlso CDate(txtDate.Value) < CDate(vatdate) Then
                        MessageBox.Show(DelOrder & " Date Should Be Greater Than Or Equal " & vatdate, Me.Text, MessageBoxButtons.OK, MessageBoxIcon.Information)
                        ClearControls(Me, ErrorProvider1)
                        Exit Try
                    End If

                    If DelOrder = "Receiving Order Fashion" Or DelOrder = "Consignment Receiving Order Fashion" Then
                        txtBrand.Text = .Item("br_Code").ToString
                        lblBrand.Text = .Item("br_Name").ToString
                        txtSeason.Text = .Item("Seas_code").ToString
                        lblSeason.Text = .Item("seas_Desc").ToString
                    End If

                    txtUstmp.Text = .Item("DOH_Ustamp").ToString
                    txtDstmp.Text = CDate(.Item("DOH_Dstamp")).ToString("dddd, MMMM dd, yyyy, hh:mm:ss")



                    ''' grids in Trading mode 
                    If DelOrder = "Receiving Order" Or DelOrder = "Consignment Receiving Order" Then



                        ''insering into Details Grid
                        strsql = " Delete from " & TmpGridDetails & " insert into " & TmpGridDetails
                        strsql &= " SELECT   /*Distinct ROW_NUMBER() OVER ( ORDER BY DOD_itmref ) + (SELECT  ISNULL(MAX(Recid), 0) FROM " & TmpGridDetails & ")*/DOD_Recid  , DOD_Recid , DOD_itmref ,"

                        '   strsql &= " Stk_ref,DOD_Stat,dod_stkrecid," 'CASE WHEN DOD_Stat = 'F' THEN 'Free' WHEN DOD_Stat = 'R' THEN 'Return' WHEN DOD_Stat = 'N' THEN 'Normal' WHEN DOD_Stat is NULL THEN 'Normal' END AS [status],"
                        strsql &= " Stk_ref, DOD_Stat   ,dod_stkrecid," 'CASE WHEN DOD_Stat = 'F' THEN 'Free' WHEN DOD_Stat = 'R' THEN 'Return' WHEN DOD_Stat = 'N' THEN 'Normal' WHEN DOD_Stat is NULL THEN 'Normal' END AS [status],"
                        strsql &= " DOD_Counter ," '  "
                        strsql &= "stk_desc , stk04_PurDelOrdD.DOD_Recqty,MEASU.meas_desc,MEASUB.Meas_desc, "
                        strsql &= " DOD_ConsQty,DOD_vat , "
                        'strsql &= " CASE WHEN DOD_vat = 'T' THEN 'Taxable' WHEN DOD_vat = 'N' then 'Not Taxable' when  DOD_vat = 'E' THEN 'Taxable Embeded' end , 'O', null"

                        strsql &= "CASE WHEN STK_FCCODE = '" & gbCurr1 & "' THEN DOD_AVgCostLC  WHEN STK_FCCODE = '" & gbCurr2 & "' THEN DOD_AvgCostCC  ELSE     DOD_AvgCostFC  END   AS 'DOD_AVgCost'  ,"
                        If DelOrder = "Consignment Receiving Order" Then
                            strsql &= " DOD_Disc  ,  CASE WHEN STK_FCCODE = '" & gbCurr1 & "' THEN DOD_NetCostLC  WHEN STK_FCCODE = '" & gbCurr2 & "' THEN DOD_NetCostCC  ELSE     DOD_NetCostFC  END   AS [DOD_NetCost] ,  "
                        End If

                        'strsql &= " DOD_AvgCostCC ,"
                        'strsql &= " DOD_AvgCostFC ,"

                        strsql &= " [STK_FCCODE],"
                        strsql &= "  'O' , null ,NULL, STK_ItmClass  , '' "
                        strsql &= " From  stk04_PurDelOrdD  "
                        strsql &= " INNER JOIN Stk04_stkfil ON stk_recid = dod_stkrecid "
                        strsql &= " inner join stk04_measurements MEASU on measu.meas_unit = DOD_unit  "
                        strsql &= " inner join stk04_measurements MEASUB on measub.meas_unit = stk_subunit  "
                        '  If DelOrder = "Consignment Receiving Order" Or DelOrder = "Consignment Receiving Order Fashion" Then strsql &= " left join Stk04_stkprices on    STK_RECID = SP_StkRecId "
                        ' strsql &= " LEFT JOIN stk04_PurDelOrdDBE ON DOB_Drecid= DOD_RECID "
                        strsql &= " WHERE (DOD_HRecid = " & Val(txtCode.Text) & ")"
                        ' strsql &= " AND stk_Category <> 'tmp' "
                        Execute(strsql)

                        dsDetDetails.Tables(0).Clear()
                        daDetDetails.Fill(dsDetDetails, dsDetDetails.Tables(0).TableName)
                        RefQtyLbl(False)
                        lstDelivery.Refresh()

                        ''insering into Expiry Grid
                        strsql = " Delete from " & TmpGridExpiry & " INSERT INTO " & TmpGridExpiry
                        strsql &= " SELECT /*ROW_NUMBER() OVER (ORDER BY DOQ_BatchInfo),NULL,*/temp.* FROM (  SELECT distinct DOQ_RecID [DOQ_RecID1] ,DOQ_RecID, Recid ,"
                        strsql &= " DOQ_BatchInfo,DOQ_ExpiryDate,DOQ_Recqty,'O' stat,STQ_ProductionDt, '' [btn]"
                        strsql &= " from " & TmpGridDetails & " Grid "
                        strsql &= "  LEFT JOIN stk04_PurDelOrdDQty ON stk04_PurDelOrdDQty.DOQ_Drecid= Grid.DOB_Drecid "
                        strsql &= " LEFT JOIN dbo.stk04_stkqty ON MOD_RMRecid = STQ_STKRECID AND STQ_LOCCODE='00' AND COALESCE(STQ_ExpiryDt,'')=COALESCE(DOQ_ExpiryDate,'') "
                        strsql &= " AND COALESCE(STQ_BatchInfo,'')=COALESCE(DOQ_BatchInfo,'')"
                        strsql &= " WHERE DOQ_BatchInfo<>'' AND DOQ_ExpiryDate<>'' ) temp"
                        Execute(strsql)

                        ''insering into Barcode Grid
                        strsql = " DELETE FROM " & TmpGridBarcode & " insert into " & TmpGridBarcode
                        strsql &= " SELECT ROW_NUMBER() OVER ( ORDER BY RECID ) ,tempo.* from ( Select  DOB_RECID,RECID ,  E_RECID ,  detbarc.DOB_Barcode , 'O' stat"
                        strsql &= " ,detbarc.DOB_Drecid " 'added by GS on 09/06/16
                        strsql &= " FROM    stk04_PurDelOrdD det"
                        strsql &= " INNER JOIN dbo.STK04_STKFIL ON STK_RECID=DOD_StkRecid"
                        strsql &= " LEFT JOIN stk04_PurDelOrdDBE detbarc ON DOD_Recid = DOB_Drecid"
                        strsql &= " INNER JOIN " & TmpGridExpiry & " temp ON temp.DOB_BatchInfo = detbarc.DOB_BatchInfo"
                        strsql &= " AND temp.DOB_ExpiryDate = detbarc.DOB_ExpiryDate"
                        strsql &= " WHERE   DOD_HRecid = " & txtCode.Text.Trim & " AND STK_ItmClass='E' AND detbarc.DOB_Barcode<>'' "
                        strsql &= " UNION SELECT  DOB_RECID,  RECID , NULL , detbarc.DOB_Barcode , 'O' stat"
                        strsql &= " ,detbarc.DOB_Drecid " 'added by GS on 09/06/16
                        strsql &= " FROM    stk04_PurDelOrdD det INNER JOIN dbo.STK04_STKFIL ON STK_RECID=DOD_StkRecid"
                        strsql &= " LEFT JOIN stk04_PurDelOrdDBE detbarc ON DOD_Recid = DOB_Drecid"
                        strsql &= " INNER JOIN " & TmpGridDetails & " temp ON temp.DOB_Drecid= det.DOD_Recid"
                        strsql &= " WHERE   DOD_HRecid = " & txtCode.Text.Trim & " AND detbarc.DOB_Barcode <>'' AND (STK04_STKFIL.STK_ItmClass='T' OR STK04_STKFIL.STK_ItmClass='B')) tempo"
                        Execute(strsql)

                        Code = "('" & txtCode.Text.Trim.Replace("'", "''") & "')"
                        fillPoQualityControlTables(Code, TempQcHeader, TempQcDetails)
                        UpdateDtQualityControlHeader()

                        '''grids in fashion mode 
                    ElseIf DelOrder = "Receiving Order Fashion" Or DelOrder = "Consignment Receiving Order Fashion" Then

                        Dim k As Integer
                        Dim GridSizeArr() As String
                        Dim strGrid As String
                        Dim drSizes As SqlDataReader

                        strsql = "Delete from " & TmpGridDetails & " ; delete from " & TmpFashQty & " ; delete from " & TmpGridBarcode & "F"
                        Execute(strsql)

                        Str = " SELECT  DISTINCT DOD_Hrecid,DOD_recid, DOD_ItmRef, DOD_Stat,"
                        Str &= " DOD_StkRecid, DOD_ColorCode, DOQ_GridCode ,DOD_Counter"
                        Str &= " FROM dbo.stk04_PurDelOrdD"
                        Str &= " INNER JOIN stk04_PurDelOrdDQty ON DOQ_Drecid=DOD_Recid"
                        Str &= " INNER JOIN stk04_PurDelOrdH  ON DOD_Hrecid=DOH_Recid"
                        Str &= " WHERE DOH_Recid = " & Val(txtCode.Text.Trim)
                        ReadFromDBToDataReader(Str, drSql)
                        k = 0
                        With drSql
                            If .HasRows Then
                                While .Read
                                    strGrid = "SELECT GD_SizeDesc FROM dbo.stk04_DetGRID"
                                    strGrid &= " WHERE GD_Code='" & .Item("DOQ_GridCode").ToString & "'"
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


                                    strsql = " INSERT INTO  " & TmpGridDetails
                                    strsql &= " SELECT DISTINCT (SELECT ISNULL(MAX(Recid),0) + 1 FROM " & TmpGridDetails & ") AS RECID,DOD_RECid, DOD_itmref, DOD_Color,"
                                    strsql &= " Stk_ref,STK_Gender, DOD_stat , STK_ITMTYPE, " 'CASE WHEN DOD_stat = 'N' THEN 'Normal' WHEN DOD_stat = 'F' THEN 'Free' WHEN DOD_stat = 'R' THEN 'Return' END AS DOD_Stat,STK_ITMTYPE,"
                                    strsql &= " STK_Style,"
                                    strsql &= " stk_recid,DOD_Counter,  Stk_Desc,DOD_ColorCode,'O',"
                                    strsql &= " DOD_recqty,"
                                    strsql &= " MEASU.Meas_desc,MEASUB.Meas_desc, DOD_ConsQty,"
                                    strsql &= " DOD_vat ,"
                                    If DelOrder = "Consignment Receiving Order" Or DelOrder = "Consignment Receiving Order Fashion" Then
                                        If txtCurr.Text = gbCurr1 Then
                                            strsql &= " DOD_AvgCostLC ,  DOD_Disc , DOD_NetCostLC   "
                                        ElseIf txtCurr.Text = gbCurr2 Then
                                            strsql &= " DOD_AvgCostCC , DOD_Disc ,DOD_NetCostcc  "
                                        Else
                                            strsql &= "  DOD_AvgCostFC ,DOD_Disc , DOD_NetCostcc   "
                                        End If
                                    Else
                                        strsql &= "  (CASE WHEN [STK_FCCODE]='" & gbCurr1 & "' "
                                        strsql &= " THEN [DOD_AvgCostLC] WHEN [STK_FCCODE]='" & gbCurr2 & "' "
                                        strsql &= " THEN [DOD_AvgCostCC] ELSE  dbo.[convert5]('" & gbApp & "','" & gbComp & "','" & gbYear & "',"
                                        strsql &= " DOD_AvgCostFC,STK_FCCODE,'" & txtCurr.Text.Trim & "','" & CType(Now, Date).ToString("MM/dd/yyyy") & "')   END) UnitCost  "
                                    End If
                                    strsql &= " ,'" & txtCurr.Text.Trim & "' ,'O',null,null , STK_ItmClass , '' "
                                    strsql &= " FROM   dbo.stk04_PurDelOrdD"
                                    strsql &= " INNER JOIN stk04_PurDelOrdDQty ON DOQ_Drecid=DOD_Recid"
                                    strsql &= " Left Join stk04_stkfil on stk_recid = DOD_stkrecid"
                                    strsql &= " inner join stk04_measurements MEASU on measu.meas_unit = DOD_unit  "
                                    strsql &= " inner join stk04_measurements MEASUB on measub.meas_unit = stk_subunit  "
                                    If DelOrder = "Consignment Receiving Order" Or DelOrder = "Consignment Receiving Order Fashion" Then strsql &= " left join Stk04_stkprices on    STK_RECID = SP_StkRecId AND isnull(SP_StartingQty,0)  = 0"
                                    strsql &= " LEFT JOIN dbo.stk04_Gender ON STK_Gender=G_Code"
                                    strsql &= " INNER JOIN stk04_PurDelOrdH  ON DOD_Hrecid=DOH_Recid"
                                    strsql &= " WHERE DOH_Recid = " & Val(txtCode.Text.Trim)
                                    strsql &= " AND DOD_ItmRef = '" & .Item("DOD_ItmRef").ToString & "'"
                                    strsql &= " AND DOD_Stat = '" & .Item("DOD_Stat").ToString & "'"
                                    strsql &= " AND DOD_STKRECID = '" & .Item("DOD_StkRecid").ToString & "'"
                                    strsql &= " AND DOD_ColorCode = '" & .Item("DOD_ColorCode").ToString & "'"
                                    strsql &= " AND STK_ACTIVE ='Y'"
                                    strsql &= " AND STK_CurrentArchive = 'C'"
                                    strsql &= " AND STK_ItmClass = 'F'  AND DOD_RECID='" & .Item("DOD_RECID").ToString & "' "

                                    Execute(strsql)
                                    strsql = ""
                                    dsDetDetails.Tables(0).Clear()
                                    daDetDetails.Fill(dsDetDetails, dsDetDetails.Tables(0).TableName)
                                    lstDelivery.Refresh()


                                    ''INSERING INTO QTY FASHION LST 
                                    strsql = " INSERT INTO  " & TmpFashQty
                                    strsql &= " SELECT DISTINCT (SELECT ISNULL(MAX(Recid),0) + 1 FROM " & TmpFashQty & ")  , "
                                    strsql &= " (SELECT  MAX(Recid)   FROM " & TmpGridDetails & ") AS RECID,"
                                    strsql &= " stk_grid,"
                                    For k = 1 To MaxCnt
                                        If k <= GridSizeArr.Length Then
                                            strsql &= " (SELECT Distinct CASE WHEN  DOQ_Recqty=0 THEN NULL ELSE DOQ_Recqty END  FROM dbo.stk04_PurDelOrdD "
                                            strsql &= " INNER JOIN stk04_PurDelOrdDQty ON DOQ_Drecid = DOD_Recid "
                                            strsql &= " INNER JOIN stk04_PurDelOrdH  ON DOD_Hrecid=DOH_Recid "
                                            strsql &= " WHERE DOH_Recid = " & Val(txtCode.Text.Trim)
                                            strsql &= " AND DOD_RECID = '" & .Item("DOD_RECID").ToString & "'"
                                            strsql &= " AND DOD_ColorCode = '" & .Item("DOD_ColorCode").ToString & "'"
                                            strsql &= " AND DOQ_GridSize = '" & GridSizeArr(k - 1) & "'),"
                                        Else
                                            strsql &= " NULL,"
                                        End If
                                    Next

                                    strsql = strsql.Substring(1, strsql.Length() - 2)
                                    strsql &= " FROM   dbo.stk04_PurDelOrdD"
                                    strsql &= " INNER JOIN stk04_PurDelOrdH  ON DOD_Hrecid=DOH_Recid"
                                    strsql &= " LEFT JOIN stk04_stkfil on stk_recid =  DOD_StkRecid"
                                    strsql &= " WHERE DOH_Recid = " & Val(txtCode.Text.Trim)
                                    strsql &= " AND DOD_ItmRef = '" & .Item("DOD_ItmRef").ToString & "'"
                                    strsql &= " AND DOD_Stat = '" & .Item("DOD_Stat").ToString & "'"
                                    strsql &= " AND DOD_STKRECID = '" & .Item("DOD_StkRecid").ToString & "'"
                                    strsql &= " AND DOD_ColorCode = '" & .Item("DOD_ColorCode").ToString & "'"
                                    strsql &= " AND STK_ACTIVE ='Y'"
                                    strsql &= " AND STK_CurrentArchive = 'C'"
                                    strsql &= " AND STK_ItmClass = 'F' AND DOD_RECID='" & .Item("DOD_RECID").ToString & "' "


                                    Execute(strsql)
                                    strsql = ""
                                    dsDetFqty.Tables(0).Clear()
                                    daDetFqty.Fill(dsDetFqty, dsDetFqty.Tables(0).TableName)
                                    lstFashQty.Refresh()



                                    ''INSERING BARCODES

                                    strsql = " INSERT INTO  " & TmpGridBarcode & "F"
                                    strsql &= " SELECT ROW_NUMBER() OVER ( ORDER BY Grid.RECid  )+(SELECT ISNULL(MAX(Tmp_Recid),0)   FROM " & TmpGridBarcode & "F) , Grid.RECid , grid.Color , "
                                    For k = 1 To MaxCnt
                                        If k <= GridSizeArr.Length Then
                                            strsql &= "  min(CASE WHEN gd_order =  " & (k) & "  THEN DOB_Barcode"
                                            strsql &= " Else  Convert(varchar(20), NULL)"
                                            strsql &= " END)  SIZE" & k & ",'O',"
                                        Else
                                            strsql &= "Convert(varchar(20), NULL) ,'O',"
                                        End If
                                    Next
                                    strsql &= " 'O' FROM  " & TmpGridDetails & " Grid "
                                    strsql &= " INNER JOIN ( SELECT    ROW_NUMBER() OVER ( PARTITION BY DOB_GridSize "
                                    strsql &= " ORDER BY DOB_GridSize ) counter ,*"
                                    strsql &= " FROM stk04_PurDelOrdDBE "
                                    strsql &= " WHERE DOB_Drecid='" & .Item("DOD_recid").ToString & "')"
                                    strsql &= " stk04_PurDelOrdDBE ON stk04_PurDelOrdDBE.DOB_Drecid =  Grid.DOB_DRECid "
                                    strsql &= " INNER JOIN stk04_DetGRID ON DOB_GridCode = GD_Code AND DOB_GridSize = GD_SizeDesc"
                                    strsql &= " GROUP BY  Grid.RECID , Grid.Color, counter"
                                    Execute(strsql)

                                End While
                            End If
                        End With
                        RefGridSizes(lstFashQty.Columns("GridCode").Text.Trim)
                    End If
                    If DelOrder = "Consignment Receiving Order Fashion" Or DelOrder = "Consignment Receiving Order" Then
                        '    PnlConsigment.Visible = True

                        txtPrice.ReadOnly = True
                        'txtCurr.Text = lstDelivery.Columns("STK_FCcode").Text.Trim
                        '   txtPrice.Text = lstDelivery.Columns("COST").Text.Trim
                        If txtPrice.Text.Trim <> "" Then
                            txtPrice.Text = CType(txtPrice.Text, Double).ToString("N3")
                        End If
                    Else
                        If Wtype <> "" Then
                            '  PnlConsigment.Visible = True
                            'PnlConsigment.Enabled = False
                            txtPrice.ReadOnly = True
                            txtPrice.TabStop = False
                            '  LblCostCurr.Visible = True
                        End If
                    End If
                    ReadonlyConsFash()
                    RefQtyLbl(False)
                    LstVisbility(False, False, lstDelivery.Columns("ItmClass").Text.Trim)
                    ObjSumNormal = dsDetDetails.Tables("tblDetails").Compute("Sum( MOD_OrdQty)", " MOD_stat='N' ")
                    ObjSumFree = dsDetDetails.Tables("tblDetails").Compute("Sum( MOD_OrdQty)", " MOD_stat='F' ")
                    ObjSumReturn = dsDetDetails.Tables("tblDetails").Compute("Sum( MOD_OrdQty)", " MOD_stat='R' ")
                    objSum = CDbl(IIf(ObjSumNormal Is DBNull.Value, "0", ObjSumNormal)) + CDbl(IIf(ObjSumFree Is DBNull.Value, "0", ObjSumFree)) - CDbl(IIf(ObjSumReturn Is DBNull.Value, "0", ObjSumReturn))
                    lstDelivery.Splits(0).DisplayColumns("MOD_OrdQty").DataColumn.FooterText = CType(objSum, Double).ToString("N" & gbDecNo)
                Else
                    If OldValue <> "" And Wtype <> "" Then
                        AllLocks(AllLocksType.ReleaseLock, "Stk04_PurDelOrdH", OldValue, cntTitle, Me.Name)
                    End If
                    refreshControl = False
                    txtUstmp.Text = "..."
                    txtDstmp.Text = "..."
                End If
            End With
            lcChanges = False
            lcChangeKey = False
        Catch ex As Exception
            MessageBox.Show("Error: " & ex.Message, cntTitle, MessageBoxButtons.OK, MessageBoxIcon.Error)
        End Try
    End Sub

    Private Sub ReadonlyConsFash()
        Try
            'If DelOrder <> "Receiving Order" Then
            If Wtype = "" Or Wtype = "DELETE" Then Exit Sub

            If dsDetDetails.Tables("tblDetails").Select("").Length() <> 0 Then
                If Wtype = "ADD" Then
                    txtSupp.ReadOnly = True : btnSupp.Enabled = False
                    txtDate.ReadOnly = True
                    btnCurr.Enabled = False : txtCurr.ReadOnly = True
                End If

                If DelOrder = "Consignment Receiving Order Fashion" Or DelOrder = "Receiving Order Fashion" Then
                    txtBrand.ReadOnly = True : btnBrand.Enabled = False
                    txtSeason.ReadOnly = True : btnSeason.Enabled = False
                End If
            Else
                txtSupp.ReadOnly = False : btnSupp.Enabled = True
                txtDate.ReadOnly = False
                If DelOrder = "Consignment Receiving Order Fashion" Or DelOrder = "Receiving Order Fashion" And Wtype = "ADD" Then
                    txtBrand.ReadOnly = False : btnBrand.Enabled = True
                    txtSeason.ReadOnly = False : btnSeason.Enabled = True
                End If
                If DelOrder = "Consignment Receiving Order Fashion" Or DelOrder = "Consignment Receiving Order" Then
                    If txtCurr.Text.Trim <> "" Then
                        txtCurr.ReadOnly = True
                        btnCurr.Enabled = False
                    End If
                End If
            End If


        Catch ex As Exception
            MessageBox.Show("Error: " & ex.Message, cntTitle, MessageBoxButtons.OK, MessageBoxIcon.Error)
        End Try
    End Sub

    Public Sub RefGridSizes(ByVal GridCode As String)
        Dim strsql As String
        Dim drSQL As SqlDataReader
        Dim j As Int64

        Try
            If DelOrder = "Receiving Order" Then
                strsql = "Exec P_GetSizeGrid 'DeliveryOrderD','" & gbUser & "'"
            ElseIf DelOrder = "Receiving Order Fashion" Then
                strsql = "Exec P_GetSizeGrid 'DeliveryOrderF','" & gbUser & "'"
            End If
            Execute(strsql)

            strsql = " select * from " & TmpFashQty
            If txtRecid.Text.Trim = "" Then
                If lstDelivery.Columns("Recid").Text.Trim <> "" Then strsql &= " where recid=" & lstDelivery.Columns("Recid").Text.Trim
            Else
                strsql &= " where GridCode='" & GridCode & "' and recid=" & txtRecid.Text.Trim
            End If

            ClearGridFiltration(lstFashQty)

            If Not dsDetFqty.Tables("tblDetails") Is Nothing Then dsDetFqty.Tables("tblDetails").Clear()

            If Not daDetFqty Is Nothing Then
                daDetFqty.SelectCommand.CommandText = strsql
                daDetFqty.Fill(dsDetFqty, "tblDetails")
                lstFashQty.Refresh()
                lstFashQty.Enabled = True
            End If
            PnlFashQty.Visible = True
            lstFashQty.Visible = True
            ' If GridCode = "" Then Exit Sub

            If DelOrder = "Receiving Order Fashion" Then
                strsql = "SELECT * FROM TempGridSizeSP_DeliveryOrderF" & gbUser
            Else
                strsql = "SELECT * FROM TempGridSizeSP_DeliveryOrderCF" & gbUser
            End If
            strsql &= " WHERE G_CODE = '" & lstFashQty.Columns("GridCode").Text.Trim & "'"


            Dim VisibleColumns As Integer = 0


            ReadFromDBToDataReader(strsql, drSQL)
            If drSQL.HasRows Then
                drSQL.Read()
                For j = 1 To MaxCnt
                    lstFashQty.Columns("Size" & j).Caption = drSQL.Item("Size" & j).ToString
                    If drSQL.Item("Size" & j).ToString = "" Then
                        lstFashQty.Splits(0).DisplayColumns("Size" & j).Visible = False

                        'Added By Rachelle Assaf On 30.July.2013
                        lstFashQty.Splits(0).DisplayColumns("Size" & j).Width = 0

                    Else
                        lstFashQty.Splits(0).DisplayColumns("Size" & j).Visible = True
                        VisibleColumns += 1
                    End If
                Next
            Else
                For j = 1 To MaxCnt
                    lstFashQty.Splits(0).DisplayColumns("Size" & j).Visible = False
                Next
            End If

            ''haitham 
            If VisibleColumns - 1 < 14 Then
                PnlFashQty.Height = 55

                For ColIdx As Integer = 3 To VisibleColumns + 2
                    '''' If VisibleColumns = 0 Then Exit For
                    'Formula: Column Width = ([Grid Width]-[Width of the First Column That Is not included in the resising algorith]) / [Number Of Visible Columns]
                    If VisibleColumns > 1 Then
                        lstFashQty.Splits(0).DisplayColumns(ColIdx).Width = CInt((lstFashQty.Width - lstFashQty.Splits(0).DisplayColumns(2).Width) / CDbl(VisibleColumns)) - 25
                    Else
                        lstFashQty.Splits(0).DisplayColumns(ColIdx).Width = CInt((lstFashQty.Width - lstFashQty.Splits(0).DisplayColumns(2).Width) / CDbl(VisibleColumns)) - 25
                    End If
                Next
            Else
                PnlFashQty.Height = 75
                For ColIdx As Integer = 3 To VisibleColumns + 2
                    lstFashQty.Splits(0).DisplayColumns(ColIdx).Width = 50
                Next
            End If

        Catch Ex As Exception
            Throw Ex
        End Try
    End Sub

    Private Sub RefQtyLbl(ByVal clear As Boolean)
        Dim strsql1 As String
        Dim drsql As SqlDataReader
        Try

            If clear = False Then


                lstDelivery.UpdateData()
                daDetDetails.Update(dsDetDetails.Tables("tblDetails"))
                '...TOTAL NET
                strsql1 = " " ' " * ROUND((CASE  WHEN  Meas_Sign='*'  THEN 1 * Meas_Value ELSE  1/ Meas_Value END ),2)"

                strsql = "select "
                strsql &= "sum((case when COALESCE(STK.MEAS_TYPE, Pend2.Meas_Type) ='L' and substring(MOD_stat,1,1)='F' then  ISNULL(MOD_ConsOrdqty,0) else 0 end )" & strsql1 & " ) as [free Length item]," '...free Length item
                strsql &= "sum((case when COALESCE(STK.MEAS_TYPE, Pend2.Meas_Type) ='L' and substring(MOD_stat,1,1)='N' then  ISNULL(MOD_ConsOrdqty,0) else 0 end )" & strsql1 & " ) as [normal Length item]," '...normal Length item
                strsql &= "sum((case when COALESCE(STK.MEAS_TYPE, Pend2.Meas_Type) ='W' and substring(MOD_stat,1,1)='F' then  ISNULL(MOD_ConsOrdqty,0) else 0 end )" & strsql1 & " ) as [free weight item]," '...free weight item
                strsql &= "sum((case when COALESCE(STK.MEAS_TYPE, Pend2.Meas_Type) ='W' and substring(MOD_stat,1,1)='N' then  ISNULL(MOD_ConsOrdqty,0) else 0 end )" & strsql1 & " ) as [normal weight item]," '...normal weight item
                strsql &= "sum((case when COALESCE(STK.MEAS_TYPE, Pend2.Meas_Type) ='P' and substring(MOD_stat,1,1)='F' then  ISNULL(MOD_ConsOrdqty,0) else 0 end )" & strsql1 & " ) as [free pieces item]," '...free pieces item
                strsql &= "sum((case when COALESCE(STK.MEAS_TYPE, Pend2.Meas_Type) ='P' and substring(MOD_stat,1,1)='N' then  ISNULL(MOD_ConsOrdqty,0) else 0 end )" & strsql1 & " ) as [normal pieces item]," '...normal pieces item

                strsql &= "sum((case when COALESCE(STK.MEAS_TYPE, Pend2.Meas_Type) ='L' and substring(MOD_stat,1,1)='R' then  ISNULL(MOD_ConsOrdqty,0) else 0 end ) ) as [Return Length item],"
                strsql &= "sum((case when COALESCE(STK.MEAS_TYPE, Pend2.Meas_Type) ='W' and substring(MOD_stat,1,1)='R' then  ISNULL(MOD_ConsOrdqty,0) else 0 end ) ) as [Return weight item],"
                strsql &= "sum((case when COALESCE(STK.MEAS_TYPE, Pend2.Meas_Type) ='P' and substring(MOD_stat,1,1)='R' then  ISNULL(MOD_ConsOrdqty,0) else 0 end ) ) as [Return pieces item] "
                strsql &= " from " & TmpGridDetails
                strsql &= " LEFT JOIN V_Pord2_GetStkMeas STK ON STK_Recid = MOD_RmRecid"

                'strsql &= " LEFT JOIN (SELECT SFP_Recid, MEAS_VALUE, SFP_UNIT, SFP_SUBUNIT,"
                'strsql &= " SFP_UNITPACK, MEAS_SIGN, Meas_unit, Meas_type, SFP_ItmClass "
                'strsql &= " FROM STK04_StkFilPending" '& TempPendingDO
                'strsql &= " INNER JOIN STK04_Measurements  on Meas_unit= SFP_SUBUNIT)"
                'strsql &= " Pend ON Pend.SFP_Recid = MOD_RmRecid"

                strsql &= " LEFT JOIN (SELECT SFP_Recid, MEAS_VALUE, SFP_UNIT, SFP_SUBUNIT  ,   "
                strsql &= " SFP_UNITPACK, MEAS_SIGN, Meas_unit, Meas_type, SFP_ItmClass"
                strsql &= " FROM " & TempPendingDO
                strsql &= " INNER JOIN STK04_Measurements  on Meas_unit= SFP_SUBUNIT)"
                strsql &= " Pend2 ON Pend2.SFP_Recid = MOD_RmRecid"

                strsql &= " WHERE ISNULL(STK_ItmClass, '') " & IIf(DelOrder = "Receiving Order Fashion" Or DelOrder = "Consignment Receiving Order Fashion", "= 'F'", "<> 'F'").ToString
                ' strsql &= " OR ISNULL(Pend.SFP_ItmClass, '') " & IIf(DelOrder = "Receiving Order Fashion" Or DelOrder = "Consignment Receiving Order Fashion", "= 'F'", "<> 'F'").ToString
                strsql &= " OR ISNULL(Pend2.SFP_ItmClass, '') " & IIf(DelOrder = "Receiving Order Fashion" Or DelOrder = "Consignment Receiving Order Fashion", "= 'F'", "<> 'F'").ToString


                ReadFromDBToDataReader(strsql, drsql)

                With drsql
                    If .HasRows Then
                        .Read()
                        If Not .Item("free Length item").ToString = "" Then
                            txtMFree.Text = Format(.Item("free Length item"), "#,##0.000").ToString()
                            txtMNormal.Text = Format(.Item("normal Length item"), "#,##0.000").ToString()
                            txtkgFree.Text = Format(.Item("free weight item"), "#,##0.000").ToString()
                            txtkgNormal.Text = Format(.Item("normal weight item"), "#,##0.000").ToString()
                            txtPcsFree.Text = Format(.Item("free pieces item"), "#,##0.000").ToString()
                            txtPcsNormal.Text = Format(.Item("normal pieces item"), "#,##0.000").ToString()
                            txtMReturn.Text = Format(.Item("Return Length item"), "#,##0.000").ToString()
                            txtkgReturn.Text = Format(.Item("Return weight item"), "#,##0.000").ToString()
                            txtPcsReturn.Text = Format(.Item("Return pieces item"), "#,##0.000").ToString()
                            ''  txtnamount(i).Caption = Format(IIf(.HasRows > 0, .Item(i - 2), 0), "#,##0.0000")
                        Else
                            txtMFree.Text = CStr(0)
                            txtMNormal.Text = CStr(0)
                            txtkgFree.Text = CStr(0)
                            txtkgNormal.Text = CStr(0)
                            txtPcsFree.Text = CStr(0)
                            txtPcsNormal.Text = CStr(0)
                            txtMReturn.Text = CStr(0)
                            txtkgReturn.Text = CStr(0)
                            txtPcsReturn.Text = CStr(0)
                        End If
                    End If
                End With
            Else
                txtMFree.Text = CStr(0)
                txtMNormal.Text = CStr(0)
                txtkgFree.Text = CStr(0)
                txtkgNormal.Text = CStr(0)
                txtPcsFree.Text = CStr(0)
                txtPcsNormal.Text = CStr(0)
                txtMReturn.Text = CStr(0)
                txtkgReturn.Text = CStr(0)
                txtPcsReturn.Text = CStr(0)
            End If


        Catch ex As Exception
            MessageBox.Show("Error: " & ex.Message, cntTitle, MessageBoxButtons.OK, MessageBoxIcon.Error)
        End Try
    End Sub

    Private Sub btnLoc_Click(ByVal sender As System.Object, ByVal e As System.EventArgs) Handles btnLoc.Click
        Dim Active As Boolean
        Try
            If Wtype = "ADD" Then Active = True Else Active = False
            LocationList(txtLoc, lblLoc, Me, " Loc_Type1 <> 'H' AND Loc_Code <> '00' ", Active) 'AND Loc_CoCode = '" & gbComp & "'", Active)
        Catch ex As Exception
            MessageBox.Show("Error: " & ex.Message, cntTitle, MessageBoxButtons.OK, MessageBoxIcon.Error)
        End Try
        'Dim strsql As String
        'Dim cnSql As SqlConnection
        'Dim dsSql As DataSet
        'Dim daSql As SqlDataAdapter
        'Dim frm As frmSearch
        'Dim Sizefield() As Int16 = {80, 200}
        'Dim Alnfield() As String = {"C", "C"}
        'Dim Retfield() As String = {"Code", "Location"}

        'Try

        '    Cursor.Current = Cursors.WaitCursor

        '    strsql = " SELECT LOC_CODE as [Code],LOC_DESC as [Location] "
        '    strsql &= " from stk04_location  where loc_active='Y'  and Loc_Type1 <> 'H' AND Loc_Code <> '00'"


        '    cnSql = New SqlConnection(ConnectionString)
        '    cnSql.Open()
        '    daSql = New SqlDataAdapter(strsql, cnSql)
        '    dsSql = New DataSet
        '    daSql.Fill(dsSql, "tbcode3")
        '    cnSql.Close()
        '    frm = New frmSearch

        '    With frm
        '        .Owner = Me
        '        If .ShowDialog("Return", dsSql.Tables("tbcode3"), dsSql.Tables("tbcode3").Rows.Count, _
        '        "Receiving Location", Sizefield, Alnfield, Retfield, 0, Nothing, False, 350, 400, , True) = DialogResult.OK Then
        '            If gbRetSearch(0) = "" Then
        '                txtSupp.Focus()
        '            Else
        '                If gbRetSearch(0) <> "" Then
        '                    txtLoc.Text = gbRetSearch(0)
        '                    lblLoc.Text = gbRetSearch(1)
        '                    txtLoc.Focus()
        '                    SendKeys.Send("{TAB}")
        '                Else
        '                    txtLoc.Focus()
        '                End If
        '            End If
        '        End If
        '    End With

        'Catch exp As Exception
        '    Throw exp
        'Finally
        '    gbRetSearch = Nothing
        '    dsSql = Nothing : daSql = Nothing
        '    If Not cnSql Is Nothing Then
        '        If cnSql.State = ConnectionState.Open Then cnSql.Close()
        '        cnSql = Nothing
        '    End If
        '    frm = Nothing
        '    Cursor.Current = Cursors.Default
        'End Try
    End Sub

    Private Sub btnBrand_Click(ByVal sender As System.Object, ByVal e As System.EventArgs) Handles btnBrand.Click
        Dim Cond As String
        Try
            If UCase(gbUser) <> "SA" And UCase(gbUser) <> "SU" Then
                Cond = " br_code IN (SELECT BUMP_BrandCode FROM dbo.Stk04_BusUnitManagPriv "
                Cond &= " WHERE BUMP_UserCode='" & gbUser & "' AND BUMP_Access='1')"
            End If

            BrandList(txtBrand, lblBrand, Me, Cond, , True)
        Catch ex As Exception
            MessageBox.Show("Error: " & ex.Message, cntTitle, MessageBoxButtons.OK, MessageBoxIcon.Error)
        End Try
    End Sub

    Private Sub btnSeason_Click(ByVal sender As System.Object, ByVal e As System.EventArgs) Handles btnSeason.Click
        Try
            SeasonsList(txtSeason, lblSeason, Me, , , False)
        Catch ex As Exception
            MessageBox.Show("Error: " & ex.Message, cntTitle, MessageBoxButtons.OK, MessageBoxIcon.Error)
        End Try
    End Sub

    Private Sub txtCode_Enter(ByVal sender As Object, ByVal e As System.EventArgs) Handles txtCode.Enter, txtSupp.Enter, txtDelOrder.Enter, txtCurr.Enter, _
    txtOrderNo.Enter, txtLoc.Enter, txtBrand.Enter, txtSeason.Enter, txtMFree.Enter, txtMNormal.Enter, txtMReturn.Enter, txtkgFree.Enter, txtPrice.Enter, _
    txtkgNormal.Enter, txtkgReturn.Enter, txtPcsFree.Enter, txtPcsNormal.Enter, txtPcsReturn.Enter, txtSupRef.Enter, txtSupColor.Enter, txtRef.Enter, txtColor.Enter, txtQty.Enter, txtStatus.Enter
        Try
            FocusStyle(CType(sender, TextBox), "Enter")

        Catch exp As Exception
            MessageBox.Show("Error : " & exp.Message, cntTitle, _
            MessageBoxButtons.OK, MessageBoxIcon.Error)
        End Try
    End Sub

    Private Sub txtCode_Leave(ByVal sender As Object, ByVal e As System.EventArgs) Handles txtCode.Leave, txtSupp.Leave, txtDelOrder.Leave, txtCurr.Leave, _
    txtOrderNo.Leave, txtLoc.Leave, txtBrand.Leave, txtSeason.Leave, txtMFree.Leave, txtMNormal.Leave, txtMReturn.Leave, txtkgFree.Leave, txtPrice.Leave, _
    txtkgNormal.Leave, txtkgReturn.Leave, txtPcsFree.Leave, txtPcsNormal.Leave, txtPcsReturn.Leave, txtSupRef.Leave, txtSupColor.Leave, txtRef.Leave, txtColor.Leave, txtQty.Leave, txtStatus.Leave
        Try
            FocusStyle(CType(sender, TextBox), "Leave")
        Catch exp As Exception
            MessageBox.Show("Error : " & exp.Message, cntTitle, _
            MessageBoxButtons.OK, MessageBoxIcon.Error)
        End Try
    End Sub
    Private Sub txtSupp_TextChanged(ByVal sender As System.Object, ByVal e As System.EventArgs) Handles txtSupp.TextChanged, txtDelOrder.TextChanged
        Try
            lcChangeKey = True
            lcChanges = True
            ErrorProvider1.SetError(CType(sender, TextBox), "")
        Catch ex As Exception
            MessageBox.Show("Error : " & ex.Message, cntTitle, _
            MessageBoxButtons.OK, MessageBoxIcon.Error)
        End Try
    End Sub
    Private Sub txtLoc_TextChanged(ByVal sender As System.Object, ByVal e As System.EventArgs) Handles _
    txtLoc.TextChanged, txtBrand.TextChanged, txtSeason.TextChanged, txtMFree.TextChanged, txtMNormal.TextChanged, txtMReturn.TextChanged, txtkgFree.TextChanged, _
    txtkgNormal.TextChanged, txtkgReturn.TextChanged, txtPcsFree.TextChanged, txtPcsNormal.TextChanged, txtPcsReturn.TextChanged, TxtGridV.TextChanged, txtCurr.TextChanged
        Try

            lcChanges = True
            ErrorProvider1.SetError(CType(sender, TextBox), "")
        Catch ex As Exception
            MessageBox.Show("Error : " & ex.Message, cntTitle, _
            MessageBoxButtons.OK, MessageBoxIcon.Error)
        End Try
    End Sub
    Private Sub txtOrderNo_TextChanged(ByVal sender As System.Object, ByVal e As System.EventArgs) Handles txtOrderNo.TextChanged
        Try
            'OldVal = ""
            lcChanges = True
            ErrorProvider1.SetError(CType(sender, TextBox), "")
        Catch ex As Exception
            MessageBox.Show("Error : " & ex.Message, cntTitle, _
            MessageBoxButtons.OK, MessageBoxIcon.Error)
        End Try
    End Sub
    Private Sub txtDate_Enter(ByVal sender As Object, ByVal e As System.EventArgs) Handles txtDate.Enter, txtActualDate.Enter
        Try
            FocusStyle(CType(sender, C1DateEdit), "Enter")
        Catch exp As Exception
            MessageBox.Show("Error : " & exp.Message, cntTitle, _
            MessageBoxButtons.OK, MessageBoxIcon.Error)
        End Try
    End Sub
    Private Sub txtDate_Leave(ByVal sender As Object, ByVal e As System.EventArgs) Handles txtDate.Leave, txtActualDate.Leave
        Try
            FocusStyle(CType(sender, C1DateEdit), "Leave")
        Catch exp As Exception
            MessageBox.Show("Error : " & exp.Message, cntTitle, _
            MessageBoxButtons.OK, MessageBoxIcon.Error)
        End Try
    End Sub



    Private Function keyValidate() As Boolean
        Dim drSql As SqlDataReader
        Try
            If lcChangeKey = False Then Exit Function
            If Me.ActiveControl.Name = "txtSupp" Or Me.ActiveControl.Name = "txtDelOrder" Or Me.ActiveControl.Name = "txtDate" Then Exit Function


            If Not IsDate(txtDate.Text) Then Exit Function


            Dim SuppRecId As String = GetFieldDesc("stk04_supplier", "sup_code ='" & txtSupp.Text & "'", "sup_Recid")
            If txtSupp.Text.Trim <> "" And txtDelOrder.Text.Trim <> "" And Not txtDate.Value Is DBNull.Value Then
                strsql = " SELECT DOH_Recid FROM dbo.stk04_PurDelOrdH WHERE DOH_SupRecid='" & SuppRecId & "' AND DOH_Ref='" & txtDelOrder.Text.Trim.Replace("'", "''") & "' AND DOH_Date='" & CDate(txtDate.Value).ToString("MM/dd/yyyy") & "'"
                ReadFromDBToDataReader(strsql, drSql)
                If drSql.HasRows Then
                    drSql.Read()
                    txtCode.Text = drSql.Item("DOH_Recid").ToString
                    If Wtype <> "ADD" Then
                        RefreshControls(True)
                        keyValidate = True
                    Else
                        MessageBox.Show("Receiving Order Already Exists.", cntTitle, MessageBoxButtons.OK, MessageBoxIcon.Exclamation)
                        LstVisbility(False, False, lstDelivery.Columns("ItmClass").Text.Trim)
                        ClearControls(Me, ErrorProvider1)
                        INITCOL(True)
                        InitGrids()
                        keyValidate = False
                        txtSupp.Focus()

                    End If
                Else
                    If Wtype <> "ADD" Then
                        MessageBox.Show("Invalid Receiving Order", cntTitle, MessageBoxButtons.OK, MessageBoxIcon.Exclamation)
                        LstVisbility(False, False, lstDelivery.Columns("ItmClass").Text.Trim)
                        ClearControls(Me, ErrorProvider1)
                        INITCOL(True)
                        InitGrids()
                        keyValidate = False
                        If Wtype <> "ADD" Then txtUstmp.Text = "..." : txtDstmp.Text = "..."
                        RefQtyLbl(True)
                        txtCode.Focus()
                    Else
                        'ch @ 25/04/2018 as per tania and wadih remove this test : client fairco 
                        'strsql = "SELECT    loc_code , Loc_Desc  FROM stk04_location"
                        'strsql &= " INNER JOIN ( SELECT ISNULL(MIN(loc_code), '') AS loccode FROM   stk04_location"
                        'strsql &= " WHERE  loc_cocode = '" & gbComp & "' AND loc_active = 'Y'"
                        'strsql &= " AND Loc_Type1 <> 'H' AND Loc_Code <> '00' ) AS T1 ON loccode = loc_code "
                        'ReadFromDBToDataReader(strsql, drSql)
                        'If drSql.HasRows Then
                        '    drSql.Read()
                        '    txtLoc.Text = drSql.Item("loc_code").ToString
                        '    lblLoc.Text = drSql.Item("Loc_Desc").ToString
                        'End If
                    End If
                    lcChangeKey = False
                End If
            Else
                If Wtype <> "ADD" Then
                    ClearControls(Me, ErrorProvider1)
                    INITCOL(True)
                    InitGrids()
                    keyValidate = False
                    txtUstmp.Text = "..." : txtDstmp.Text = "..."
                    RefQtyLbl(True)

                    txtCode.Focus()
                End If
            End If

        Catch ex As Exception
            MessageBox.Show("General Error : " & ex.Message, Me.Text, MessageBoxButtons.OK, MessageBoxIcon.Warning)
        End Try
    End Function

    Private Sub ConnMenu_MouseDown(ByVal sender As System.Object, ByVal e As System.Windows.Forms.MouseEventArgs) _
    Handles txtDesc.MouseDown, txtCode.MouseDown, txtSupp.MouseDown, txtDelOrder.MouseDown, txtQty.MouseDown, txtCurr.MouseDown, txtPrice.MouseDown, TxtGridV.MouseDown, _
    txtOrderNo.MouseDown, txtLoc.MouseDown, txtBrand.MouseDown, txtSeason.MouseDown, txtMFree.MouseDown, txtMNormal.MouseDown, txtMReturn.MouseDown, txtkgFree.MouseDown, _
    txtkgNormal.MouseDown, txtkgReturn.MouseDown, txtPcsFree.MouseDown, txtPcsNormal.MouseDown, txtPcsReturn.MouseDown, txtDate.MouseDown, txtActualDate.MouseDown, txtSUBUnit.MouseDown, _
    TXTUNIT.MouseDown
        Dim i As Int32
        Dim txt As TextBox

        Try

            txt = CType(sender, TextBox)
            If e.Button = MouseButtons.Right Then
                If sender Is txtDesc Then
                    txt.ContextMenu = NullConMenu
                Else
                    ShowContextMenu(sender)
                End If
            End If

        Catch exp As Exception
            MessageBox.Show("Error: " & exp.Message, cntTitle, _
            MessageBoxButtons.OK, MessageBoxIcon.Error)
        Finally
            txt = Nothing
        End Try
    End Sub



    Private Sub RefreshLstGrid(ByVal LIST As C1TrueDBGrid, ByVal DropCreateTable As Boolean, ByVal tempTable As String, ByRef daDetGrid As SqlDataAdapter, ByRef dsDetGrid As DataSet)
        Dim strsql As String
        Dim cbsql As SqlCommandBuilder
        Dim cnSql As SqlConnection
        Dim i As Integer
        Dim drSql As SqlDataReader

        Try
            'Create Temporary Table Related To the Grid

            dsDetGrid = New DataSet

            strsql = " Select * from " & tempTable

            cnSql = New SqlConnection(ConnectionString)
            cnSql.Open()


            If Not dsDetGrid.Tables("tblDetails") Is Nothing Then dsDetGrid.Tables.Remove("tblDetails")
            daDetGrid = New SqlDataAdapter(strsql, cnSql)
            daDetGrid.TableMappings.Add("Table", "tblDetails")


            With daDetGrid
                .FillSchema(dsDetGrid, SchemaType.Mapped)
                .Fill(dsDetGrid, "tblDetails")
            End With

            cbsql = New SqlCommandBuilder(daDetGrid)
            'daDetGrid.UpdateCommand = cbsql.GetUpdateCommand
            'daDetGrid.InsertCommand = cbsql.GetInsertCommand
            'daDetGrid.DeleteCommand = cbsql.GetDeleteCommand

            cbsql = Nothing
            If LIST.Name = "lstDelivery" Then
                With lstDelivery

                    .FetchRowStyles = True
                    .AllowColMove = False
                    .ColumnFooters = True
                    .EditActive = False
                    If Not .DataSource Is Nothing Then .DataSource = Nothing
                    .ClearFields()
                    .DataSource = dsDetGrid.Tables("tblDetails").DefaultView

                    .AllowFilter = True
                    'btnqc
                    With .Splits(0).DisplayColumns("btnqc")
                        .Visible = True
                        '.Width = 15
                        .Width = btnQA.Width
                        .DataColumn.Caption = "QC"
                        .ButtonText = True
                        .Locked = False : .ButtonAlways = True
                        .Style.ForeColor = Color.Transparent

                    End With
                    .Splits(0).CaptionStyle.ForeColor = Color.Red
                    .Splits(0).CaptionStyle.BackColor = Color.White
                    .Splits(0).CaptionStyle.HorizontalAlignment = AlignHorzEnum.Center

                    ' .Splits(0).Caption = "Receiving Order Details"

                    .Splits(0).DisplayColumns("RECID").Visible = False
                    .Splits(0).DisplayColumns("RECID").AllowSizing = False

                    .Splits(0).DisplayColumns("ItmClass").Visible = False
                    .Splits(0).DisplayColumns("ItmClass").AllowSizing = False

                    .Splits(0).DisplayColumns("DOB_Drecid").Visible = False
                    .Splits(0).DisplayColumns("DOB_Drecid").AllowSizing = False

                    '.Splits(0).DisplayColumns("STK_FCCODE").Visible = False
                    '.Splits(0).DisplayColumns("STK_FCCODE").AllowSizing = False

                    '.Splits(0).DisplayColumns("COST").Visible = False
                    '.Splits(0).DisplayColumns("COST").AllowSizing = False
                    With .Splits(0).DisplayColumns("COST")
                        .Width = txtPrice.Width - 5
                        .Style.HorizontalAlignment = AlignHorzEnum.Far
                        .HeadingStyle.HorizontalAlignment = AlignHorzEnum.Far
                        .DataColumn.FilterOperator = "like"
                        .DataColumn.NumberFormat = "#,##0.000"
                        If DelOrder = "Consignment Receiving Order" Or DelOrder = "Consignment Receiving Order Fashion" Then
                            .DataColumn.Caption = "Cost"
                            .Locked = True
                        Else
                            .DataColumn.Caption = "Cost " & gbCurr2
                            .Locked = True
                        End If

                    End With
                    With .Splits(0).DisplayColumns("STK_FCCODE")
                        .Width = 0
                        .Visible = False
                        .AllowSizing = False
                        .Locked = True
                    End With
                    If DelOrder = "Consignment Receiving Order" Or DelOrder = "Consignment Receiving Order Fashion" Then

                        With .Splits(0).DisplayColumns("Disc")
                            .Width = TxtDisc.Width
                            .Style.HorizontalAlignment = AlignHorzEnum.Near
                            .HeadingStyle.HorizontalAlignment = AlignHorzEnum.Far
                            .DataColumn.Caption = "Disc %"
                            .DataColumn.FilterOperator = "like"
                            .DataColumn.NumberFormat = "#0.00"
                            .Locked = False
                        End With
                        With .Splits(0).DisplayColumns("Net")
                            .Width = TxtNet.Width
                            .Style.HorizontalAlignment = AlignHorzEnum.Near
                            .HeadingStyle.HorizontalAlignment = AlignHorzEnum.Far
                            .DataColumn.Caption = "Net"
                            .DataColumn.FilterOperator = "like"
                            .DataColumn.NumberFormat = "#,##0.000"
                            .Locked = True
                        End With

                    End If

                    With .Splits(0).DisplayColumns("MOD_supitmref")
                        '  If DelOrder = "Receiving Order" Or DelOrder = "Consignment Receiving Order" Then
                        '.Width = 168
                        ' Else
                        '.Width = 83
                        '  End If
                        .Width = txtSupRef.Width
                        .Style.HorizontalAlignment = AlignHorzEnum.Near
                        .HeadingStyle.HorizontalAlignment = AlignHorzEnum.Near
                        .DataColumn.Caption = "Supp.Ref."
                        .DataColumn.FilterOperator = "like"

                    End With
                    With .Splits(0).DisplayColumns("stk_ref")
                        'If DelOrder = "Receiving Order" Or DelOrder = "Consignment Receiving Order" Then
                        '    .Width = 195
                        'Else
                        '    .Width = 105
                        'End If
                        .Width = txtRef.Width
                        .Style.HorizontalAlignment = AlignHorzEnum.Near
                        .HeadingStyle.HorizontalAlignment = AlignHorzEnum.Near
                        .DataColumn.Caption = "Item Ref."
                        .DataColumn.FilterOperator = "like"
                    End With
                    If DelOrder = "Receiving Order Fashion" Or DelOrder = "Consignment Receiving Order Fashion" Then
                        With .Splits(0).DisplayColumns("SuppColor")
                            '    .Width = 83
                            .Width = txtSupColor.Width
                            .DataColumn.Caption = "Supp. Color"
                            .DataColumn.FilterOperator = "LIKE"
                        End With
                        With .Splits(0).DisplayColumns("STK_GENDER")
                            '.Width = 46
                            .Width = txtGender.Width
                            .Locked = False
                            .DataColumn.Caption = "Gender"
                            .DataColumn.FilterOperator = "LIKE"
                        End With
                        With .Splits(0).DisplayColumns("ItemType")
                            '  .Width = 53
                            .Width = txtProdGroup.Width
                            .Locked = False
                            .DataColumn.Caption = "Prd.Grp"
                            .DataColumn.FilterOperator = "LIKE"
                        End With
                        With .Splits(0).DisplayColumns("Style")
                            '.Width = 49
                            .Width = txtStyle.Width
                            .Locked = False
                            .DataColumn.Caption = "Style"
                            .DataColumn.FilterOperator = "LIKE"
                        End With

                        With .Splits(0).DisplayColumns("Color")
                            '  .Width = 44
                            .Width = txtColor.Width
                            .DataColumn.Caption = "Color"
                            .FetchStyle = True
                            .DataColumn.FilterOperator = "LIKE"
                        End With

                        With .Splits(0).DisplayColumns("ColorN_O")
                            .Width = 0 : .Locked = False
                            .Visible = False
                            .AllowSizing = False
                        End With



                    End If
                    With .Splits(0).DisplayColumns("Barcode")
                        '.Width = 27
                        .Width = btnBarcode.Width
                        .DataColumn.Caption = "Brcd"
                        .ButtonText = True
                        .Locked = False : .ButtonAlways = True
                    End With
                    '  ItmClass
                    If DelOrder = "Receiving Order" Then
                        'With .Splits(0).DisplayColumns("ItmClass")
                        '    .Visible = True
                        '    .Width = 27
                        '    .DataColumn.Caption = "QA"
                        '    .ButtonText = True
                        '    .Locked = False : .ButtonAlways = True
                        '    .Style.ForeColor = Color.Transparent
                        'End With
                    End If

                    .Splits(0).DisplayColumns("MOD_RMRecid").Visible = False
                    .Splits(0).DisplayColumns("MOD_RMRecid").AllowSizing = False

                    .Splits(0).DisplayColumns("PO").Visible = False
                    .Splits(0).DisplayColumns("PO").AllowSizing = False

                    With .Splits(0).DisplayColumns("MOD_Count")
                        .Style.HorizontalAlignment = AlignHorzEnum.Near
                        .HeadingStyle.HorizontalAlignment = AlignHorzEnum.Near
                        .DataColumn.Caption = "Ctr"
                        .Visible = False
                        .AllowSizing = False
                    End With
                    With .Splits(0).DisplayColumns("MOD_stat")
                        '   If DelOrder = "Receiving Order" Or DelOrder = "Consignment Receiving Order" Then
                        '.Width = 54
                        '   Else
                        '  .Width = 41
                        'End If
                        .Width = txtStatus.Width
                        .Style.HorizontalAlignment = AlignHorzEnum.Center
                        .HeadingStyle.HorizontalAlignment = AlignHorzEnum.Center
                        .DataColumn.Caption = "Status"
                        .DataColumn.FilterOperator = "like"
                    End With
                    With .Splits(0).DisplayColumns("stk_desc")
                        'If DelOrder = "Receiving Order" Or DelOrder = "Consignment Receiving Order" Then
                        '    .Width = 280
                        'Else
                        '    .Width = 198
                        'End If
                        .Width = txtDesc.Width - 5
                        .AllowFocus = True
                        .Style.HorizontalAlignment = AlignHorzEnum.Near
                        .HeadingStyle.HorizontalAlignment = AlignHorzEnum.Near
                        .DataColumn.Caption = "Description"
                        .DataColumn.FilterOperator = "like"
                        .Locked = True
                    End With
                    With .Splits(0).DisplayColumns("MOD_OrdQty")
                        '.Width = 59
                        .Width = txtQty.Width
                        .Style.HorizontalAlignment = AlignHorzEnum.Far
                        .HeadingStyle.HorizontalAlignment = AlignHorzEnum.Far
                        .DataColumn.Caption = "Rec. Qty"
                        .DataColumn.FilterOperator = "like"
                        If gbDecNo = 0 Then
                            .DataColumn.NumberFormat = "#,##0"
                        ElseIf gbDecNo = 1 Then
                            .DataColumn.NumberFormat = "#,##0.0"
                        ElseIf gbDecNo = 2 Then
                            .DataColumn.NumberFormat = "#,##0.00"
                        ElseIf gbDecNo = 3 Then
                            .DataColumn.NumberFormat = "#,##0.000"
                        End If
                        If DelOrder = "Receiving Order Fashion" Or DelOrder = "Co  nsignment Receiving Order Fashion" Then
                            .DataColumn.NumberFormat = "#,##0"
                        End If
                        .Locked = False
                    End With

                    With .Splits(0).DisplayColumns("stk_Subunit")
                        '.Width = 55
                        .Width = txtSUBUnit.Width
                        .Style.HorizontalAlignment = AlignHorzEnum.Far
                        .HeadingStyle.HorizontalAlignment = AlignHorzEnum.Far
                        .DataColumn.Caption = "Sub Unit"
                        .DataColumn.FilterOperator = "like"
                        .Locked = True
                        .AllowFocus = True
                    End With
                    With .Splits(0).DisplayColumns("stk_unit")
                        '.Width = 45
                        .Width = TXTUNIT.Width
                        .Style.HorizontalAlignment = AlignHorzEnum.Far
                        .HeadingStyle.HorizontalAlignment = AlignHorzEnum.Far
                        .DataColumn.Caption = "Unit"
                        .DataColumn.FilterOperator = "like"
                        .Locked = True
                        .AllowFocus = True
                    End With

                    With .Splits(0).DisplayColumns("MOD_ConsOrdQty")
                        ' .Width = 59
                        .Width = txtConsQty.Width
                        .Style.HorizontalAlignment = AlignHorzEnum.Far
                        .HeadingStyle.HorizontalAlignment = AlignHorzEnum.Far
                        .DataColumn.Caption = "Cons. Qty"
                        .DataColumn.FilterOperator = "like"
                        If gbDecNo = 0 Then
                            .DataColumn.NumberFormat = "#,##0"
                        ElseIf gbDecNo = 1 Then
                            .DataColumn.NumberFormat = "#,##0.0"
                        ElseIf gbDecNo = 2 Then
                            .DataColumn.NumberFormat = "#,##0.00"
                        ElseIf gbDecNo = 3 Then
                            .DataColumn.NumberFormat = "#,##0.000"
                        End If
                        If DelOrder = "Receiving Order Fashion" Or DelOrder = "Consignment Receiving Order Fashion" Then
                            .DataColumn.NumberFormat = "#,##0"
                        End If
                        .AllowFocus = True
                        .Locked = True
                    End With

                    With .Splits(0).DisplayColumns("invd_VAT")
                        '.Width = 35
                        .Width = txtVat.Width
                        .Style.HorizontalAlignment = AlignHorzEnum.Center
                        .HeadingStyle.HorizontalAlignment = AlignHorzEnum.Center
                        .DataColumn.Caption = "VAT"
                        .DataColumn.FilterOperator = "like"

                        .Locked = False
                    End With
                    .Splits(0).DisplayColumns("new_old").Visible = False
                    .Splits(0).DisplayColumns("new_old").AllowSizing = False
                    .FilterBar = True




                End With
                lstDelivery.AllowAddNew = False
                lstDelivery.AllowUpdate = False

            ElseIf LIST.Name = "LstExpiry" Then
                With LstExpiry
                    .EditActive = False
                    .FetchRowStyles = True
                    .AllowColMove = False

                    If Not .DataSource Is Nothing Then .DataSource = Nothing
                    .ClearFields()
                    .DataSource = dsDetGrid.Tables("tblDetails").DefaultView

                    .AllowFilter = True
                    .FilterBar = True

                    .Splits(0).CaptionStyle.ForeColor = Color.Red
                    .Splits(0).CaptionStyle.BackColor = Color.White
                    .Splits(0).CaptionStyle.HorizontalAlignment = AlignHorzEnum.Center

                    .Splits(0).Caption = "Expiry Date Details"


                    With .Splits(0).DisplayColumns("E_RECID")
                        .Width = 80
                        .Style.HorizontalAlignment = AlignHorzEnum.Far
                        .HeadingStyle.HorizontalAlignment = AlignHorzEnum.Far
                        .DataColumn.Caption = "E_RECID"
                        .DataColumn.FilterOperator = "like"
                        .Visible = False : .AllowSizing = False
                    End With

                    .Splits(0).DisplayColumns("RECID").Visible = False
                    .Splits(0).DisplayColumns("RECID").AllowSizing = False


                    .Splits(0).DisplayColumns("NewRECID").Visible = False
                    .Splits(0).DisplayColumns("NewRECID").AllowSizing = False

                    With .Splits(0).DisplayColumns("DOB_BatchInfo")
                        .Width = 130
                        .Style.HorizontalAlignment = AlignHorzEnum.Far
                        .HeadingStyle.HorizontalAlignment = AlignHorzEnum.Far
                        .DataColumn.Caption = "Lot No"
                        .DataColumn.FilterOperator = "like"
                        .Button = True
                    End With
                    With .Splits(0).DisplayColumns("DOB_ExpiryDate")
                        .Width = 80
                        .Style.HorizontalAlignment = AlignHorzEnum.Near
                        .HeadingStyle.HorizontalAlignment = AlignHorzEnum.Near
                        .DataColumn.Caption = "Expiry Date"
                        .DataColumn.FilterOperator = "like"
                        .DataColumn.EditMask = "99.99.9999"
                        .DataColumn.EditMaskUpdate = True
                        .Button = True
                    End With
                    With .Splits(0).DisplayColumns("MOD_OrdQty")
                        .Width = 130
                        .Style.HorizontalAlignment = AlignHorzEnum.Far
                        .DataColumn.Caption = "Qty"
                        .DataColumn.FilterOperator = "like"
                        .HeadingStyle.HorizontalAlignment = AlignHorzEnum.Far
                    End With
                    .Splits(0).DisplayColumns("exp_stat").Visible = False
                    .Splits(0).DisplayColumns("exp_stat").AllowSizing = False

                    With .Splits(0).DisplayColumns("DOQ_ProductionDt")
                        .Width = 80
                        .Style.HorizontalAlignment = AlignHorzEnum.Near
                        .HeadingStyle.HorizontalAlignment = AlignHorzEnum.Near
                        .DataColumn.Caption = "Production Date"
                        .DataColumn.FilterOperator = "like"
                        .DataColumn.EditMask = "99.99.9999"
                        .DataColumn.EditMaskUpdate = True
                        .Button = True
                    End With

                    With .Splits(0).DisplayColumns("btnQC")
                        .Visible = True
                        .Width = 27
                        .DataColumn.Caption = "QC"
                        .ButtonText = True
                        .Locked = False : .ButtonAlways = True
                        .Style.ForeColor = Color.Transparent

                    End With

                End With

            ElseIf LIST.Name = "LstBarcode" Then

                With LstBarcode
                    .EditActive = False
                    .FetchRowStyles = True
                    If Not .DataSource Is Nothing Then .DataSource = Nothing
                    .ClearFields()
                    .DataSource = dsDetGrid.Tables("tblDetails").DefaultView

                    .AllowFilter = True

                    .Splits(0).CaptionStyle.ForeColor = Color.Red
                    .Splits(0).CaptionStyle.BackColor = Color.White
                    .Splits(0).CaptionStyle.HorizontalAlignment = AlignHorzEnum.Center

                    .Splits(0).Caption = "Barcode Details"

                    With .Splits(0).DisplayColumns("B_RECID")
                        .Width = 80
                        .Style.HorizontalAlignment = AlignHorzEnum.Near
                        .HeadingStyle.HorizontalAlignment = AlignHorzEnum.Near
                        .DataColumn.Caption = "B_RECID"
                        .DataColumn.FilterOperator = "like"
                        .Visible = False : .AllowSizing = False
                    End With
                    .Splits(0).DisplayColumns("DOB_RECID").Visible = False
                    .Splits(0).DisplayColumns("DOB_RECID").AllowSizing = False

                    .Splits(0).DisplayColumns("RECID").Visible = False
                    .Splits(0).DisplayColumns("RECID").AllowSizing = False

                    .Splits(0).DisplayColumns("E_RECID").Visible = False
                    .Splits(0).DisplayColumns("E_RECID").AllowSizing = False

                    .Splits(0).DisplayColumns("DOB_Drecid").Visible = False 'added by GS on 09/06/16
                    .Splits(0).DisplayColumns("DOB_Drecid").AllowSizing = False 'added by GS on 09/06/16

                    With .Splits(0).DisplayColumns("DOB_Barcode")
                        .Width = 50
                        .Style.HorizontalAlignment = AlignHorzEnum.Near
                        .HeadingStyle.HorizontalAlignment = AlignHorzEnum.Near
                        .DataColumn.Caption = "Barcode"
                        .DataColumn.FilterOperator = "like"
                        .Locked = False
                        .Button = True
                    End With
                    .Splits(0).DisplayColumns("bar_stat").Visible = False
                    .Splits(0).DisplayColumns("bar_stat").AllowSizing = False

                    .FilterBar = True
                End With
            ElseIf LIST Is lstFashQty Then
                With lstFashQty
                    .EditActive = False
                    .FetchRowStyles = True
                    If Not .DataSource Is Nothing Then .DataSource = Nothing
                    .ClearFields()
                    .DataSource = dsDetGrid.Tables("tblDetails").DefaultView

                    .AllowFilter = True

                    .Splits(0).CaptionStyle.ForeColor = Color.Red
                    .Splits(0).CaptionStyle.BackColor = Color.White
                    .Splits(0).CaptionStyle.HorizontalAlignment = AlignHorzEnum.Center

                    .Splits(0).Caption = "Quantity Per Size"

                    .Splits(0).DisplayColumns("Q_RECID").Visible = False
                    .Splits(0).DisplayColumns("Q_RECID").AllowSizing = False
                    .Splits(0).DisplayColumns("Q_RECID").Width = 0

                    .Splits(0).DisplayColumns("RECID").Visible = False
                    .Splits(0).DisplayColumns("RECID").AllowSizing = False
                    .Splits(0).DisplayColumns("RECID").Width = 0

                    With .Splits(0).DisplayColumns("GridCode")
                        .Width = 80 : .Locked = True
                        .DataColumn.Caption = "Grid Code"
                        .DataColumn.FilterOperator = "LIKE"

                        .AllowFocus = True
                    End With

                    For j = 1 To MaxCnt
                        With .Splits(0).DisplayColumns("Size" & j)
                            .Width = 50
                            .DataColumn.Caption = "Size" & j
                            .DataColumn.FilterOperator = "LIKE"
                            .DataColumn.NumberFormat = "#,##0"

                        End With
                    Next

                End With
            End If

            With LIST
                For i = 0 To .Columns.Count - 1

                    If dsDetGrid.Tables("tblDetails").Columns(i).DataType Is System.Type.GetType("System.String") Then
                        .Splits(0).DisplayColumns(i).DataColumn.FilterOperator = "LIKE"
                    Else
                        .Splits(0).DisplayColumns(i).DataColumn.FilterOperator = "="
                    End If
                    .Splits(0).DisplayColumns(i).HeadingStyle.HorizontalAlignment = AlignHorzEnum.Center
                    .Splits(0).DisplayColumns(i).HeadingStyle.VerticalAlignment = AlignVertEnum.Center
                    .Splits(0).DisplayColumns(i).Style.VerticalAlignment = AlignVertEnum.Center
                    If LIST Is lstDelivery Then
                        .Splits(0).DisplayColumns(i).AllowSizing = False
                        .Splits(0).DisplayColumns(i).Locked = True
                    End If
                Next
                .Refresh()

            End With





        Catch ex As Exception
            Throw ex
        End Try
    End Sub

    Private Sub CreateLstBarcode()
        Dim i As Integer

        Try

            DropSqlObject(TmpGridBarcode & "F", "TABLE")

            strsql = "CREATE TABLE " & TmpGridBarcode & "F" & " ("
            strsql &= " Tmp_RecId numeric(18,0) NOT NULL,"
            strsql &= " Tmp_DRecId numeric(18,0)  NULL,"
            strsql &= " Tmp_Color varchar(2) NOT NULL,"
            For i = 1 To MaxCnt
                strsql &= " Tmp_Size" & i & " varchar(40),"
                strsql &= " Tmp_OldNew" & i & " varchar(1),"
            Next
            strsql &= " Tmp_Stat varchar(1),"   'A:Add, M:Modify, D: Delete (not used if fashion item)
            strsql &= " CONSTRAINT PK_" & TmpGridBarcode & "F" & " PRIMARY KEY  (Tmp_RecId)"
            strsql &= " )"

            Execute(strsql)


        Catch ex As Exception
            Throw ex
        End Try
    End Sub

    Private Sub InsertFashionItemBarcode(ByVal cmSql As SqlCommand, Optional ByVal cnsql As SqlConnection = Nothing, Optional ByVal trsql As SqlTransaction = Nothing)
        Dim drSQL As SqlDataReader
        Dim drGrid As SqlDataReader
        Dim strGrid As String
        Dim cmsql2 As SqlCommand
        Dim GridSizeArr() As String
        Dim i As Integer
        Dim k As Integer
        Try
            '**Insert details 
            If DelOrder = "Receiving Order Fashion" Or DelOrder = "Consignment Receiving Order Fashion" Then
                DropSqlObject("Temp_PurDelOrdDQty" & gbUser, "TABLE")

                strsql = " SELECT * INTO Temp_PurDelOrdDQty" & gbUser & " FROM dbo.stk04_PurDelOrdDQty WHERE 1=2 "
                'Execute(strsql)
                cmSql.CommandText = strsql
                cmSql.ExecuteNonQuery()

                DropSqlObject("Temp_PurDelOrdDBE" & gbUser, "TABLE")

                strsql = " SELECT * INTO Temp_PurDelOrdDBE" & gbUser & " FROM dbo.stk04_PurDelOrdDBE WHERE 1=2 "
                'Execute(strsql)
                cmSql.CommandText = strsql
                cmSql.ExecuteNonQuery()

                strsql = "SELECT * FROM " & TmpGridDetails
                strsql &= " INNER JOIN " & TmpFashQty & " ON " & TmpFashQty & ".RECID=" & TmpGridDetails & ".RECID"
                If Wtype <> "DELETE" Then strsql &= " where new_old<>'O'"

                If Not cmSql Is Nothing Then
                    cmsql2 = New SqlCommand(strsql, cnsql, trsql)
                    drSQL = cmsql2.ExecuteReader
                Else
                    ReadFromDBToDataReader(strsql, drSQL)
                End If


                'cmSql.CommandText = strsql
                'drSQL = cmSql.ExecuteReader
                k = 0
                With drSQL
                    If .HasRows Then
                        While .Read

                            strGrid = "SELECT GD_SizeDesc FROM dbo.stk04_DetGRID"
                            strGrid &= " WHERE GD_Code='" & .Item("GridCode").ToString & "'"
                            strGrid &= " ORDER BY GD_Order"
                            ReadFromDBToDataReader(strGrid, drGrid)
                            'cmSql.CommandText = strGrid
                            'drGrid = cmSql.ExecuteReader

                            i = 0
                            If drGrid.HasRows Then
                                While drGrid.Read
                                    ReDim Preserve GridSizeArr(i)
                                    GridSizeArr(i) = drGrid.Item("GD_SizeDesc").ToString
                                    i = i + 1
                                End While
                            End If
                            If Not drGrid Is Nothing Then
                                If drGrid.IsClosed = False Then drGrid.Close()
                                drGrid = Nothing
                            End If

                            For k = 1 To MaxCnt
                                If k <= GridSizeArr.Length Then
                                    strsql = " DECLARE @Count AS NUMERIC"
                                    strsql &= " SELECT @Count= ISNULL(MAX(DOQ_RecID),( SELECT  ISNULL(MAX(DOQ_RecID), 0)FROM stk04_PurDelOrdDQty))  FROM Temp_PurDelOrdDQty" & gbUser
                                    strsql &= " INSERT  INTO Temp_PurDelOrdDQty" & gbUser
                                    strsql &= " SELECT ROW_NUMBER() OVER ( ORDER BY DOD_RECID ) + @Count    ,temp.* FROM ("
                                    strsql &= " SELECT DISTINCT DOD_RECID  ,null EXPIRY,null BATCH,'" & .Item("GridCode").ToString & "' GridCode,'" & GridSizeArr(k - 1) & "' GridSize,"
                                    strsql &= Val(.Item("Size" & k).ToString) & " qty,GETDATE() cdate,'" & gbUser & "' cuser ,GETDATE() date,'" & gbUser & "' oUser "
                                    strsql &= " FROM " & TmpGridDetails & " Grid"
                                    strsql &= " LEFT JOIN " & TmpFashQty & " fasion  ON  fasion.recid = Grid.recid"
                                    strsql &= " LEFT JOIN stk04_PurDelOrdD ON DOD_ItmRef=grid.MOD_supitmref"
                                    strsql &= " AND DOD_Stat=LEFT(grid.MOD_stat,1) AND DOD_Counter=grid.MOD_Count AND dod_hrecid="
                                    If Wtype = "ADD" Or Wtype = "DELETE" Then
                                        strsql &= " ( SELECT  ISNULL(MAX(DOH_RecID), 0) FROM stk04_PurDelOrdH)  "
                                    Else
                                        strsql &= txtCode.Text.Trim
                                    End If
                                    strsql &= " WHERE  Grid.RECID='" & .Item("RECID").ToString & "'"
                                    If Wtype <> "DELETE" Then strsql &= "  and (new_old='N' or new_old='P' or new_old='W')) temp" Else strsql &= " ) temp"

                                    cmSql.CommandText = strsql
                                    cmSql.ExecuteNonQuery()
                                    'Execute(strsql)
                                End If
                            Next


                            '''insert into Temp_PurDelOrdDBE
                            For k = 1 To GridSizeArr.Length
                                ' If .Item("Size" & k).ToString <> "" AndAlso Not .Item("Size" & k).ToString Is DBNull.Value Then
                                strsql = " DECLARE @Count1 AS NUMERIC"
                                strsql &= " SELECT @Count1= ISNULL(MAX(DOB_RecID),( SELECT  ISNULL(MAX(DOB_RecID), 0)FROM stk04_PurDelOrdDBE))  FROM Temp_PurDelOrdDBE" & gbUser & ""
                                strsql &= " INSERT  INTO Temp_PurDelOrdDBE" & gbUser
                                strsql &= " SELECT  ROW_NUMBER() OVER ( ORDER BY DOD_RECID ) + @Count1 ,temp.* FROM ("
                                strsql &= " SELECT distinct DOD_RECID ,Tmp_Size" & k & " , "
                                strsql &= " null EXPIRY,null BATCH,'" & .Item("GridCode").ToString & "' GridCode,'" & GridSizeArr(k - 1) & "' GridSize,"
                                strsql &= " GETDATE() cdate,'" & gbUser & "' cuser ,GETDATE() date,'" & gbUser & "' oUser "
                                strsql &= " FROM    " & TmpGridDetails & " Grid"
                                strsql &= " LEFT JOIN " & TmpFashQty & " fasion on fasion.recid = Grid.recid"
                                strsql &= " LEFT JOIN " & TmpGridBarcode & "F Barcode ON Barcode.Tmp_DRecId = Grid.recid"
                                strsql &= " LEFT JOIN stk04_PurDelOrdD ON DOD_ItmRef=grid.MOD_supitmref"
                                strsql &= " AND DOD_Stat=LEFT(grid.MOD_stat,1) AND DOD_Counter=grid.MOD_Count AND dod_hrecid="
                                If Wtype = "ADD" Or Wtype = "DELETE" Then
                                    strsql &= " ( SELECT  ISNULL(MAX(DOH_RecID), 0) FROM stk04_PurDelOrdH)  WHERE   Tmp_Size" & k & "<>''  "
                                Else
                                    strsql &= txtCode.Text.Trim & "   WHERE   Tmp_Size" & k & "<>''   "
                                End If
                                strsql &= " AND Tmp_Stat <> 'D' AND Grid.RECID='" & .Item("RECID").ToString & "'"
                                If Wtype <> "DELETE" Then strsql &= " and (new_old='N' or new_old='P' or new_old='W')) temp" Else strsql &= " ) temp"

                                cmSql.CommandText = strsql
                                cmSql.ExecuteNonQuery()
                                'Execute(strsql)
                                ' End If
                            Next
                        End While
                    End If


                End With



            End If


        Catch exp As Exception
            Throw exp
        Finally
            Disposecontrols(, cmsql2, drSQL)
        End Try
    End Sub

    Private Sub txtCode_Validating(ByVal sender As Object, ByVal e As System.ComponentModel.CancelEventArgs) Handles txtCode.Validating
        Dim strsql As String
        Dim bExists As Boolean


        Try
            If OldValue <> "" And OldValue <> txtCode.Text.Trim And Wtype <> "" Then
                AllLocks(AllLocksType.ReleaseLock, "stk04_PurDelOrdH", OldValue, cntTitle, Me.Name)
            End If

            '06.02.2014 Lock the pending items in modify mode
            Dim Str As String
            Dim drOldPend As SqlDataReader
            Str = "SELECT SFP_OldRecid FROM " & TmpGridDetails & " inner join " & TempPendingDO & "  on SFP_Recid = MOD_RMRecid  WHERE new_old='w'"
            ReadFromDBToDataReader(Str, drOldPend)
            If drOldPend.HasRows Then
                While drOldPend.Read
                    AllLocks(AllLocksType.ReleaseLock, "STK04_StkFilPending", drOldPend.Item(0).ToString, cntTitle, lcSteps)
                End While
            End If
            If lcChangeKey = True Then
                If txtCode.Text.Trim <> "" Then
                    If txtCode.Text.IndexOf("'") >= 0 Then
                        e.Cancel = True
                        MessageBox.Show("Invalid Data Entry, ['] not allowed.", cntTitle, _
                        MessageBoxButtons.OK, MessageBoxIcon.Question)
                        Exit Try
                    End If
                    If Wtype = "" Then
                        With txtMode
                            .Text = "View Mode."
                            .ForeColor = Color.Red
                            .Image = btnSearch.Image
                        End With
                        txtUstmp.ForeColor = Color.Red
                        txtDstmp.ForeColor = Color.Red
                    End If

                    strsql = " Select * From stk04_PurDelOrdH Inner join stk04_supplier  "
                    strsql &= " On Sup_recid  =  DOH_SupRecid Inner join Stk04_location "
                    strsql &= " On loc_code  =  DOH_Loc inner join stk04_purdelordd "
                    strsql &= " on doh_RecId=DOD_Hrecid inner join stk04_stkfil "
                    strsql &= " on DOD_StkRecId = stk_recid where DOH_Recid='" & txtCode.Text.Trim & "'"
                    If DelOrder = "Receiving Order" Then
                        strsql &= "    and DOH_Type='T' and stk_newused <> 'C'"
                    ElseIf DelOrder = "Receiving Order Fashion" Then
                        strsql &= "    and  DOH_Type='F' and stk_newused <> 'C'"
                    ElseIf DelOrder = "Consignment Receiving Order" Then
                        strsql &= "    AND stk_newused = 'C' AND DOH_Type='T'"
                    ElseIf DelOrder = "Consignment Receiving Order Fashion" Then
                        strsql &= "    AND stk_newused = 'C' and  DOH_Type='F'"
                    End If
                    If Wtype = "DELETE" Or Wtype = "MODIFY" Then
                        strsql &= " AND CONVERT(VARCHAR,DOH_SupRecid) + DOH_Ref+ CONVERT(VARCHAR,DOH_Date) "
                        strsql &= " NOT IN ( SELECT CONVERT(VARCHAR,DOH_SupRecid) + DOH_Ref+ CONVERT(VARCHAR,DOH_Date)  "
                        strsql &= " FROM stk04_PurDelOrdH WHERE doh_status = 'R')"
                        strsql &= " AND DOH_PurRecid IS NULL"
                    End If


                    bExists = RecordsExists(strsql)

                    If Wtype = "ADD" Then
                        If bExists = True Then
                            e.Cancel = True
                            MessageBox.Show("Receiving Order Already Exist.", cntTitle, _
                                MessageBoxButtons.OK, MessageBoxIcon.Error)
                            LstVisbility(False, False, lstDelivery.Columns("ItmClass").Text.Trim)
                        Else
                            lcChangeKey = False
                        End If
                    Else
                        If bExists = False Then
                            e.Cancel = True
                            MessageBox.Show("Receiving Order Does Not Exist.", cntTitle, MessageBoxButtons.OK, MessageBoxIcon.Exclamation)
                            ClearControls(Me, Nothing)
                            INITCOL(True)
                            InitGrids() : RefQtyLbl(True)
                            lcChangeKey = True : txtDstmp.Text = "..." : txtUstmp.Text = "..." : LstVisbility(False, False, lstDelivery.Columns("ItmClass").Text.Trim)
                        Else
                            RefreshControls()

                        End If
                    End If
                Else
                    LstVisbility(False, False, lstDelivery.Columns("ItmClass").Text.Trim)
                    ClearControls(Me, Nothing)
                    INITCOL(True)
                    RefQtyLbl(True) : InitGrids()

                    If Wtype <> "ADD" Then txtDstmp.Text = "..." : txtUstmp.Text = "..."
                End If
            End If
        Catch ex As Exception
            MessageBox.Show("Error : " & ex.Message, cntTitle, _
            MessageBoxButtons.OK, MessageBoxIcon.Error)
        End Try

    End Sub

    Private Sub ItemBarcode(ByVal stk_recid As String, ByVal RowRecId As Long)
        Dim drs() As DataRow
        Dim strsql As String
        Dim cnSql As SqlConnection
        Dim dsSql As DataSet
        Dim daSql As SqlDataAdapter
        Dim frm As frmSearch
        Dim Sizefield() As Int16 = {0, 200}
        Dim Alnfield() As String = {"C", "C"}
        Dim Retfield() As String = {"dbar_stkid", "Barcode"}
        Dim txtB As String

        Try

            drs = dsDetBarcode.Tables("tblDetails").Select("recid='" & txtRecid.Text.Trim & "' AND bar_stat<>'D' and B_RecId <> " & RowRecId)
            If UCase(txtItmClass.Text.Trim) = "E" Then
                If drs.Length >= Val(LstExpiry.Columns("QTY").Text.Trim) Then
                    'If LstBarcode.Columns("DOB_Barcode").Text.Trim = "" Then
                    MessageBox.Show("Barcode Number Can't Be Bigger Than Expiry Quantity.", cntTitle, MessageBoxButtons.OK, MessageBoxIcon.Exclamation)
                    LstExpiry.Columns("QTY").Text = CStr(drs.Length)
                    LstBarcode.Focus()
                    LstBarcode.Row = LstBarcode.Row - 1
                    Exit Sub
                    'End If
                End If
            Else
                If drs.Length >= Val(txtQty.Text.Trim) Then
                    ' If LstBarcode.Columns("DOB_Barcode").Text.Trim = "" Then
                    MessageBox.Show("Barcode Number Can't Be Bigger Than Quantity.", cntTitle, MessageBoxButtons.OK, MessageBoxIcon.Exclamation)
                    If Val(txtQty.Text.Trim) = 0 Then
                        txtQty.Focus()
                    End If
                    txtQty.Text = CStr(drs.Length)
                    NetCalculate()
                    LstBarcode.Row = LstBarcode.Row - 1
                    Exit Sub
                    'End If
                End If
            End If

            Cursor.Current = Cursors.WaitCursor

            'strsql = " Insert into " & TmpGridBarcode
            'strsql &= " SELECT ROW_NUMBER() OVER ( ORDER BY stk_recid ) ,dbar_stkid
            strsql = " Select dbar_stkid,dbar_barcode as [Barcode] FROM Stk04_DetBarcode"
            strsql &= " WHERE dbar_stkid =" & stk_recid & ""
            strsql &= " AND dbar_barcode not in ( SELECT DOB_Barcode FROM  " & TmpGridBarcode & " where bar_stat<>'D' ) "
            If UCase(txtItmClass.Text.Trim) = "E" Then
                strsql &= " AND dbar_batch='" & LstExpiry.Columns("DOB_BatchInfo").Text.Trim & "' and dbar_expdate='" & LstExpiry.Columns("DOB_ExpiryDate").Text.Trim & "'"
            End If

            If RecordsExists(strsql) = False Then Exit Sub

            cnSql = New SqlConnection(ConnectionString)
            cnSql.Open()
            daSql = New SqlDataAdapter(strsql, cnSql)
            dsSql = New DataSet
            daSql.Fill(dsSql, "tbcode3")
            cnSql.Close()
            frm = New frmSearch

            With frm
                .Owner = Me

                If .ShowDialog("Return", dsSql.Tables("tbcode3"), dsSql.Tables("tbcode3").Rows.Count, _
                "Barcode", Sizefield, Alnfield, Retfield, 0, Nothing, False, 350, 300, , True) = DialogResult.OK Then
                    If gbRetSearch(0) = "" Then
                        txtSupp.Focus()
                    Else
                        If gbRetSearch(0) <> "" Then

                            LstBarcode.Columns("DOB_Barcode").Text = gbRetSearch(1)
                            'drs = dsDetBarcode.Tables("tblDetails").Select("recid='" & txtRecid.Text.Trim & "' AND bar_stat<>'D'")

                            'If UCase(txtItmClass.Text.Trim) = "E" Then
                            '    If drs.Length >= CDbl(LstExpiry.Columns("QTY").Text.Trim) Then
                            '        MessageBox.Show("Barcode Number Can't Be Bigger Than Expiry Quantity.", cntTitle, MessageBoxButtons.OK, MessageBoxIcon.Exclamation)
                            '        Me.BindingContext(dsDetBarcode.Tables("tblDetails").DefaultView).CancelCurrentEdit()
                            '        Exit Sub
                            '    End If
                            'Else
                            '    If drs.Length >= Val(txtQty.Text.Trim) Then
                            '        MessageBox.Show("Barcode Number Can't Be Bigger Than Quantity.", cntTitle, MessageBoxButtons.OK, MessageBoxIcon.Exclamation)
                            '        Me.BindingContext(dsDetBarcode.Tables("tblDetails").DefaultView).CancelCurrentEdit()
                            '        If Val(txtQty.Text.Trim) = 0 Then
                            '            txtQty.Focus()
                            '        End If
                            '        Exit Sub
                            '    End If
                            'End If
                            If BInsBarcodeCancel = False Then
                                LstBarcode.Columns("bar_stat").Text = "O"
                                LstBarcode.Row = LstBarcode.RowBookmark(LstBarcode.Row) + 1
                            End If
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

    Private Sub ItemExpiry(ByVal stk_recid As String)

        Dim strsql As String
        Dim cnSql As SqlConnection
        Dim dsSql As DataSet
        Dim daSql As SqlDataAdapter
        Dim frm As frmSearch
        Dim Sizefield() As Int16 = {130, 0, 0, 130, 130}
        Dim Alnfield() As String = {"C", "C", "C", "C", "C"}
        Dim Retfield() As String = {"Lot No", "Expiry Date hiden", "Production Date Hidden"}
        Dim txtB As String
        Try

            Cursor.Current = Cursors.WaitCursor

            'strsql = " Insert into " & TmpGridBarcode
            'strsql &= " SELECT ROW_NUMBER() OVER ( ORDER BY stk_recid ) ,dbar_stkid
            strsql = " Select distinct STQ_BatchInfo as [Lot No],STQ_ExpiryDt as [Expiry Date hiden] ,STQ_ProductionDt as [Production Date Hidden], "
            strsql &= " CASE WHEN  SUBSTRING(STQ_ExpiryDt,1,2) = '00' THEN"
            strsql &= " SUBSTRING(STQ_ExpiryDt,4,8)  ELSE STQ_ExpiryDt end [Expiry Date] "
            strsql &= " ,CASE WHEN SUBSTRING(STQ_ProductionDt, 1, 2) = '00' THEN SUBSTRING(STQ_ProductionDt, 4, 8) ELSE STQ_ProductionDt END [Production Date]	"
            strsql &= " FROM stk04_stkqty "
            strsql &= " WHERE STQ_STKRECID =" & stk_recid & " AND STQ_BatchInfo <>'' and STQ_LOCCODE='00'"
            'ch @ 10/10/2016 mail(ERP_Central/Purchase_Control on expiry date items)
            '   strsql &= " AND  CONVERT(DATETIME,'" & CStr(Format(CDate(txtDate.Text), "MM/dd/yyy")) & "'   )   <=  CASE WHEN  LEFT(STQ_ExpiryDt,2) ='00' THEN  DATEADD(ss   ,-1  , DATEDIFF(month, 0,  CONVERT(DATETIME,(REPLACE(ISNULL(STQ_ExpiryDt,''),'00','01')),103))) ELSE   CONVERT(DATETIME,STQ_ExpiryDt,103)  END  "
            'strsql &= " AND  CONVERT(DATETIME,'" & CStr(Format(CDate(txtDate.Text), "dd/MM/yyy")) & "',103 )   <=  CASE WHEN  LEFT(STQ_ExpiryDt,2) ='00'    THEN CASE WHEN MONTH(CONVERT(DATETIME,( REPLACE(ISNULL(STQ_ExpiryDt,''), '00', '01') ), 103)) =12 THEN  DATEADD(d, 30,CONVERT(DATETIME,( REPLACE(ISNULL(STQ_ExpiryDt,''), '00', '01') ), 103)) ELSE DATEADD(ss, -1, DATEADD(month, DATEDIFF(month, 0,  CONVERT(DATETIME,( REPLACE(ISNULL(STQ_ExpiryDt,''), '00', '01') ), 103)), 0)) END  ELSE   CONVERT(DATETIME,STQ_ExpiryDt,103 )  END  "
            strsql &= " AND  CONVERT(DATETIME,'" & CStr(Format(CDate(txtDate.Text), "dd/MM/yyy")) & "',103 )   <=  CASE WHEN  LEFT(STQ_ExpiryDt,2) ='00'    THEN DATEADD(DAY,-1,DATEADD(MONTH ,1, CONVERT(DATETIME, ( REPLACE(ISNULL(STQ_ExpiryDt,''), '00', '01') ), 103))) ELSE   CONVERT(DATETIME,STQ_ExpiryDt,103 )  END  "
            strsql &= " AND ISNULL(CONVERT (VARCHAR, [STQ_BatchInfo]) +'-' + CONVERT(VARCHAR, [STQ_ExpiryDt]),'') NOT IN "
            strsql &= " ( SELECT  ISNULL(CONVERT (VARCHAR, DOB_BatchInfo) +'-' + CONVERT(VARCHAR,DOB_ExpiryDate),'') FROM  " & TmpGridExpiry
            strsql &= " where exp_stat<>'D' and recid=" & txtRecid.Text.Trim & ")"
            If RecordsExists(strsql) = False Then Exit Sub

            cnSql = New SqlConnection(ConnectionString)
            cnSql.Open()
            daSql = New SqlDataAdapter(strsql, cnSql)
            dsSql = New DataSet
            daSql.Fill(dsSql, "tbcode3")
            cnSql.Close()
            frm = New frmSearch

            With frm
                .Owner = Me


                If .ShowDialog("Return", dsSql.Tables("tbcode3"), dsSql.Tables("tbcode3").Rows.Count, _
                "Expiry Dates", Sizefield, Alnfield, Retfield, 0, Nothing, False, 450, 300, , True) = DialogResult.OK Then
                    If gbRetSearch(0) = "" Then
                        txtSupp.Focus()
                    Else
                        If gbRetSearch(0) <> "" Then
                            LstExpiry.Columns("DOB_BatchInfo").Text = gbRetSearch(0)
                            LstExpiry(LstExpiry.Row, "DOB_ExpiryDate") = gbRetSearch(1)
                            LstExpiry(LstExpiry.Row, "DOQ_ProductionDt") = gbRetSearch(2)
                            LstExpiry.Columns("exp_stat").Text = "O"

                            Dim e1 As C1.Win.C1TrueDBGrid.ColEventArgs
                            LstExpiry_AfterColUpdate(LstExpiry, e1)
                            LstExpiry.UpdateData()

                            LstExpiry.Col = LstExpiry.Splits(0).DisplayColumns.IndexOf(LstExpiry.Columns("MOD_OrdQty"))
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

    Private Sub TxtGridV_Validating(ByVal sender As Object, ByVal e As System.ComponentModel.CancelEventArgs) Handles TxtGridV.Validating
        Dim bExists As Boolean
        Dim strsql As String

        'Try


        '    If dsDetBarcodeF.Tables.Count <> 0 Then
        '        If dsDetBarcodeF.Tables("tblDetails").Rows.Count <> 0 Then
        '            If MessageBox.Show("Are You Sure You Want To Modify The Grid Code? If Yes, You Will Loose All Your Barcodes", cntTitle, MessageBoxButtons.YesNo, MessageBoxIcon.Question) = vbYes Then
        '                If Not dsDetBarcodeF.Tables("tblDetails") Is Nothing Then dsDetBarcodeF.Tables("tblDetails").Clear()
        '                LstBarcode.ClearFields()
        '            Else
        '                'TxtGridV.Text = lcOldGrid
        '                Exit Sub
        '            End If
        '        End If
        '    End If

        '    If TxtGridV.Text.Trim <> "" Then
        '        strsql = "SELECT G_Desc FROM stk04_grid WHERE G_Code='" & TxtGridV.Text.Trim.Replace("'", "''") & "' "
        '        strsql &= " AND G_Active='Y' "

        '        bExists = RecordsExists(strsql)

        '        If bExists = False Then
        '            e.Cancel = True
        '            If Not TxtGridV.Tag Is Nothing AndAlso TxtGridV.Tag.ToString = "StkKey" Then lcChangeKey = False
        '            MessageBox.Show("Invalid Grid Size", cntTitle, _
        '                MessageBoxButtons.OK, MessageBoxIcon.Question)
        '        Else
        '            TxtGridVDesc.Text = GetFieldDesc("stk04_grid", "G_Code='" & TxtGridV.Text.Trim.Replace("'", "''") & "' AND G_Active='Y' ", "G_Desc")

        '            CreateLstBarcode()
        '        End If
        '    Else
        '        TxtGridVDesc.Text = ""
        '        If Not dsDetBarcodeF.Tables("tblDetails") Is Nothing Then dsDetBarcodeF.Tables("tblDetails").Clear()
        '    End If


        'Catch ex As Exception
        '    MessageBox.Show("Error : " & ex.Message, cntTitle, _
        '    MessageBoxButtons.OK, MessageBoxIcon.Error)
        'End Try
    End Sub

    Private Sub txtBrand_Validating(ByVal sender As Object, ByVal e As System.ComponentModel.CancelEventArgs) Handles txtBrand.Validating
        Dim strsql As String
        Dim drSql As SqlDataReader
        Try

            If Trim(txtBrand.Text) = "" Then
                lblBrand.Text = ""
                Exit Sub
            End If
            strsql = " SELECT  br_Code AS [Code] , br_Name AS [Brand] FROM Stk04_Brand WHERE br_Active='y' "
            strsql &= " AND br_Code='" & txtBrand.Text.Trim & "'" ' and RMO_POStatus='U'"

            ReadFromDBToDataReader(strsql, drSql)

            With drSql
                If .HasRows Then
                    .Read()
                    txtBrand.Text = CStr(IIf(IsDBNull(.Item("Code").ToString), "", .Item("Code").ToString))
                    lblBrand.Text = CStr(IIf(IsDBNull(.Item("Brand").ToString), "", .Item("Brand").ToString))
                    'Test if user can access this Brand
                    If UCase(gbUser) <> "SA" And UCase(gbUser) <> "SU" Then
                        strsql = "SELECT BUMP_SuppCode FROM dbo.Stk04_BusUnitManagPriv"
                        strsql &= " WHERE BUMP_UserCode='" & gbUser & "' AND BUMP_Access='1' AND BUMP_BrandCode='" & txtBrand.Text.Trim.Replace("'", "''") & "'"
                        If RecordsExists(strsql) = False Then
                            e.Cancel = True
                            txtBrand.Text = ""
                            lblBrand.Text = ""
                            MessageBox.Show("You Have No Privilege To Purchase From This Brand", cntTitle, MessageBoxButtons.OK, MessageBoxIcon.Exclamation)
                            Exit Sub
                        End If
                    End If
                Else
                    txtBrand.Text = ""
                    lblBrand.Text = ""
                    MessageBox.Show("Brand does Not Exist", cntTitle, MessageBoxButtons.OK, MessageBoxIcon.Exclamation)
                    txtBrand.Focus()
                End If

            End With
        Catch exp As Exception
            MessageBox.Show("Error: " & exp.Message, cntTitle, _
            MessageBoxButtons.OK, MessageBoxIcon.Error)
        End Try
    End Sub

    Private Sub txtSeason_Validating(ByVal sender As Object, ByVal e As System.ComponentModel.CancelEventArgs) Handles txtSeason.Validating
        Dim strsql As String
        Dim drSql As SqlDataReader
        Try

            If Trim(txtSeason.Text) = "" Then
                txtSeason.Text = ""
                lblSeason.Text = ""
                Exit Sub
            End If
            strsql = " SELECT Seas_Code AS [Code] , Seas_Desc AS [Season] FROM stk04_season WHERE Seas_Active='y' "
            strsql &= " AND Seas_Code='" & txtSeason.Text.Trim & "'"

            ReadFromDBToDataReader(strsql, drSql)

            With drSql
                If .HasRows Then
                    .Read()
                    txtSeason.Text = CStr(IIf(IsDBNull(.Item("Code").ToString), "", .Item("Code").ToString))
                    lblSeason.Text = CStr(IIf(IsDBNull(.Item("Season").ToString), "", .Item("Season").ToString))
                Else
                    txtSeason.Text = ""
                    lblSeason.Text = ""
                    MessageBox.Show("Season Does Not Exist", cntTitle, MessageBoxButtons.OK, MessageBoxIcon.Exclamation)
                    txtSeason.Focus()
                End If

            End With
        Catch exp As Exception
            MessageBox.Show("Error: " & exp.Message, cntTitle, _
            MessageBoxButtons.OK, MessageBoxIcon.Error)
        End Try
    End Sub

    Private Sub txtLoc_Validating(ByVal sender As Object, ByVal e As System.ComponentModel.CancelEventArgs) Handles txtLoc.Validating
        Dim drSQL As SqlDataReader
        Dim Active As Boolean
        Dim Cond As String
        Try
            If Wtype = "DELETE" Or Wtype = "" Then Exit Sub
            If txtLoc.Text = "" Then lblLoc.Text = "" : Exit Sub
            If txtLoc.Text.IndexOf("'") >= 0 Then
                e.Cancel = True
                MessageBox.Show("Invalid Data Entry, ['] Not Allowed.", cntTitle, MessageBoxButtons.OK, MessageBoxIcon.Information)
                Exit Try
            End If
            Cond = "LOC_CODE = '" & txtLoc.Text & "' and LOC_CODE <> '00'" ' AND Loc_CoCode='" & gbComp & "'"
            If Wtype = "ADD" Then Active = True Else Active = False
            ReadFromDBToDataReader(LocationStrsql(Cond, Active), drSQL)
            If Not drSQL.HasRows Then
                e.Cancel = True
                MessageBox.Show("Invalid Location", cntTitle, MessageBoxButtons.OK, MessageBoxIcon.Exclamation)
                Exit Sub
            End If
            drSQL.Read()
            lblLoc.Text = drSQL.Item("Description").ToString
        Catch ex As Exception
            MessageBox.Show("Error : " & ex.Message, cntTitle, MessageBoxButtons.OK, MessageBoxIcon.Error)
        Finally
            If Not drSQL Is Nothing Then
                If drSQL.IsClosed = False Then drSQL.Close()
                drSQL = Nothing
            End If
        End Try
        'Dim strsql As String
        'Dim drSql As SqlDataReader
        'Try

        '    If Wtype <> "ADD" Then Exit Sub

        '    If Trim(txtLoc.Text) = "" Then
        '        lblLoc.Text = ""
        '        Exit Sub
        '    End If

        '    strsql = " SELECT LOC_CODE as [Code],LOC_DESC as [Location] "
        '    strsql &= " from stk04_location  where loc_active='Y'  and Loc_Type1 <> 'H' AND Loc_Code <> '00'" 'and loc_cocode='" & gbComp & "'
        '    strsql &= " and LOC_CODE='" & txtLoc.Text.Trim.Replace("'", "''") & "'"

        '    ReadFromDBToDataReader(strsql, drSql)

        '    With drSql
        '        If .HasRows Then
        '            .Read()
        '            txtLoc.Text = CStr(IIf(IsDBNull(.Item("Code").ToString), "", .Item("Code").ToString))
        '            lblLoc.Text = CStr(IIf(IsDBNull(.Item("Location").ToString), "", .Item("Location").ToString))
        '        Else
        '            txtLoc.Text = ""
        '            lblLoc.Text = ""
        '            MessageBox.Show("Location does Not Exist", cntTitle, MessageBoxButtons.OK, MessageBoxIcon.Exclamation)
        '            txtLoc.Focus()
        '        End If

        '    End With
        'Catch exp As Exception
        '    MessageBox.Show("Error: " & exp.Message, cntTitle, _
        '    MessageBoxButtons.OK, MessageBoxIcon.Error)
        'End Try
    End Sub

    Private Sub txtSupp_Validating(ByVal sender As Object, ByVal e As System.ComponentModel.CancelEventArgs) Handles txtSupp.Validating
        Dim strsql As String
        Dim drSql As SqlDataReader
        Try

            ' If Wtype = "" Or Wtype = "DELETE" Then Exit Sub
            If Trim(txtSupp.Text) = "" Then
                lblSupp.Text = ""
                Exit Sub
            End If

            strsql = " select Sup_cODE as [Code],Sup_name as [Supplier Name] From Stk04_Supplier where  Sup_Active='y'"
            strsql &= " and Sup_cODE='" & txtSupp.Text.Trim.Replace("'", "''") & "'"

            ReadFromDBToDataReader(strsql, drSql)

            With drSql
                If .HasRows Then
                    .Read()
                    txtSupp.Text = CStr(IIf(IsDBNull(.Item("Code").ToString), "", .Item("Code").ToString))
                    lblSupp.Text = CStr(IIf(IsDBNull(.Item("Supplier Name").ToString), "", .Item("Supplier Name").ToString))

                    'Test if user can access this supplier
                    If UCase(gbUser) <> "SA" And UCase(gbUser) <> "SU" Then
                        strsql = "SELECT BUMP_SuppCode FROM dbo.Stk04_BusUnitManagPriv"
                        strsql &= " WHERE BUMP_UserCode='" & gbUser & "' AND BUMP_Access='1' AND BUMP_SuppCode='" & txtSupp.Text.Trim.Replace("'", "''") & "'"
                        If RecordsExists(strsql) = False Then
                            e.Cancel = True
                            txtSupp.Text = ""
                            lblSupp.Text = ""
                            MessageBox.Show("You Have No Privilege To Purchase From This Supplier", cntTitle, MessageBoxButtons.OK, MessageBoxIcon.Exclamation)
                            Exit Sub
                        End If
                    End If
                    keyValidate()
                Else
                    txtSupp.Text = ""
                    lblSupp.Text = ""
                    MessageBox.Show("Supplier does Not Exist", cntTitle, MessageBoxButtons.OK, MessageBoxIcon.Exclamation)
                    txtSupp.Focus()
                End If
            End With
            keyValidate()
           

        Catch exp As Exception
            MessageBox.Show("Error: " & exp.Message, cntTitle, _
            MessageBoxButtons.OK, MessageBoxIcon.Error)
        End Try
    End Sub

    Private Sub txtSupp_KeyDown(ByVal sender As Object, ByVal e As System.Windows.Forms.KeyEventArgs) Handles txtSupp.KeyDown
        Try
            If btnSupp.Enabled = True AndAlso e.Modifiers = Keys.Alt AndAlso e.KeyCode = Keys.Down Then
                btnSupp_Click(btnSupp, New System.EventArgs)
            End If
        Catch ex As Exception
            MessageBox.Show("Error : " & ex.Message, cntTitle, _
            MessageBoxButtons.OK, MessageBoxIcon.Error)
        End Try
    End Sub

    Private Sub txtColor_KeyDown(ByVal sender As Object, ByVal e As System.Windows.Forms.KeyEventArgs) Handles txtColor.KeyDown
        Try
            If txtColor.ButtonCustom.Enabled = True AndAlso e.Modifiers = Keys.Alt AndAlso e.KeyCode = Keys.Down Then
                btnColor_Click(txtColor.ButtonCustom, New System.EventArgs)
            End If
        Catch ex As Exception
            MessageBox.Show("Error : " & ex.Message, cntTitle, _
            MessageBoxButtons.OK, MessageBoxIcon.Error)
        End Try
    End Sub

    Private Sub txtBrand_KeyPress(ByVal sender As Object, ByVal e As System.Windows.Forms.KeyPressEventArgs) Handles txtBrand.KeyPress
        Try
            If Asc(e.KeyChar) = 22 Then
                e.Handled = True : ctMenuPast(Me)
            Else
                e.Handled = ValidGet(CType(sender, TextBox), e.KeyChar, StringType.Other, 3, , True)
            End If
        Catch ex As Exception
            MessageBox.Show("Error: " & ex.Message, cntTitle, MessageBoxButtons.OK, MessageBoxIcon.Error)
        End Try
    End Sub

    Private Sub txtSeason_KeyPress(ByVal sender As Object, ByVal e As System.Windows.Forms.KeyPressEventArgs) Handles txtSeason.KeyPress
        Try
            If Asc(e.KeyChar) = 22 Then
                e.Handled = True : ctMenuPast(Me)
            Else
                e.Handled = ValidGet(CType(sender, TextBox), e.KeyChar, StringType.Other, 2, , True)
            End If
        Catch ex As Exception
            MessageBox.Show("Error: " & ex.Message, cntTitle, MessageBoxButtons.OK, MessageBoxIcon.Error)
        End Try
    End Sub

    Private Sub txtSupp_KeyPress(ByVal sender As Object, ByVal e As System.Windows.Forms.KeyPressEventArgs) Handles txtSupp.KeyPress
        Try
            If Asc(e.KeyChar) = 22 Then
                e.Handled = True : ctMenuPast(Me)
            Else
                e.Handled = ValidGet(CType(sender, TextBox), e.KeyChar, StringType.Other, 4, , True)
            End If
        Catch ex As Exception
            MessageBox.Show("Error: " & ex.Message, cntTitle, MessageBoxButtons.OK, MessageBoxIcon.Error)
        End Try
    End Sub

    Private Sub txtDelOrder_KeyPress(ByVal sender As Object, ByVal e As System.Windows.Forms.KeyPressEventArgs) Handles txtDelOrder.KeyPress
        Try
            If Asc(e.KeyChar) = 22 Then
                e.Handled = True : ctMenuPast(Me)
            Else
                e.Handled = ValidGet(CType(sender, TextBox), e.KeyChar, StringType.Other, 15, , True)
            End If
        Catch ex As Exception
            MessageBox.Show("Error: " & ex.Message, cntTitle, MessageBoxButtons.OK, MessageBoxIcon.Error)
        End Try
    End Sub

    Private Sub txtOrderNo_KeyPress(ByVal sender As Object, ByVal e As System.Windows.Forms.KeyPressEventArgs) Handles txtOrderNo.KeyPress
        Try
            If Asc(e.KeyChar) = 22 Then
                e.Handled = True : ctMenuPast(Me)
            Else
                e.Handled = ValidGet(CType(sender, TextBox), e.KeyChar, StringType.Numeric, 15, 0, False)
            End If
        Catch ex As Exception
            MessageBox.Show("Error: " & ex.Message, cntTitle, MessageBoxButtons.OK, MessageBoxIcon.Error)
        End Try
    End Sub

    Private Sub txtLoc_KeyPress(ByVal sender As Object, ByVal e As System.Windows.Forms.KeyPressEventArgs) Handles txtLoc.KeyPress
        Try
            If Asc(e.KeyChar) = 22 Then
                e.Handled = True : ctMenuPast(Me)
            Else
                e.Handled = ValidGet(CType(sender, TextBox), e.KeyChar, StringType.Other, 2, 0, True)
            End If
        Catch ex As Exception
            MessageBox.Show("Error: " & ex.Message, cntTitle, MessageBoxButtons.OK, MessageBoxIcon.Error)
        End Try
    End Sub

    Private Sub TxtGridV_KeyPress(ByVal sender As Object, ByVal e As System.Windows.Forms.KeyPressEventArgs) Handles TxtGridV.KeyPress
        Try
            If Asc(e.KeyChar) = 22 Then
                e.Handled = True : ctMenuPast(Me)
            Else
                e.Handled = ValidGet(CType(sender, TextBox), e.KeyChar, StringType.Other, 4, , False)
            End If
        Catch ex As Exception
            MessageBox.Show("Error: " & ex.Message, cntTitle, MessageBoxButtons.OK, MessageBoxIcon.Error)
        End Try
    End Sub

    Private Sub txtStatus_KeyDown(ByVal sender As Object, ByVal e As System.Windows.Forms.KeyEventArgs) Handles txtStatus.KeyDown
        Try
            If txtRecid.Text.Trim <> "" AndAlso e.KeyCode = Keys.Delete AndAlso Not DtQualityControlHeader Is Nothing Then
                If DtQualityControlHeader.Select("PrDReciD =  " & txtRecid.Text & " ").Count <> 0 Then
                    e.Handled = True
                    MessageBox.Show("This Record Is linked To A Quality Control , Status Can't Be Changed", cntTitle, MessageBoxButtons.OK, MessageBoxIcon.Information)
                End If
            ElseIf txtStatus.ButtonCustom.Enabled = True AndAlso e.Modifiers = Keys.Alt AndAlso e.KeyCode = Keys.Down Then
                btnStat_Click(txtStatus.ButtonCustom, New System.EventArgs)
            End If
        Catch ex As Exception
            MessageBox.Show("Error : " & ex.Message, cntTitle, _
            MessageBoxButtons.OK, MessageBoxIcon.Error)
        End Try
    End Sub

    Private Sub txtLoc_KeyDown(ByVal sender As Object, ByVal e As System.Windows.Forms.KeyEventArgs) Handles txtLoc.KeyDown
        Try
            If btnLoc.Enabled = True AndAlso e.Modifiers = Keys.Alt AndAlso e.KeyCode = Keys.Down Then
                btnLoc_Click(btnLoc, New System.EventArgs)
            End If
        Catch ex As Exception
            MessageBox.Show("Error : " & ex.Message, cntTitle, _
            MessageBoxButtons.OK, MessageBoxIcon.Error)
        End Try
    End Sub

    Private Sub txtBrand_KeyDown(ByVal sender As Object, ByVal e As System.Windows.Forms.KeyEventArgs) Handles txtBrand.KeyDown
        Try
            If btnBrand.Visible = True AndAlso btnBrand.Enabled = True AndAlso e.Modifiers = Keys.Alt AndAlso e.KeyCode = Keys.Down Then
                btnBrand_Click(btnBrand, New System.EventArgs)
            End If
        Catch ex As Exception
            MessageBox.Show("Error : " & ex.Message, cntTitle, _
            MessageBoxButtons.OK, MessageBoxIcon.Error)
        End Try
    End Sub

    Private Sub txtSeason_KeyDown(ByVal sender As Object, ByVal e As System.Windows.Forms.KeyEventArgs) Handles txtSeason.KeyDown
        Try
            If btnSeason.Visible = True AndAlso btnSeason.Enabled = True AndAlso e.Modifiers = Keys.Alt AndAlso e.KeyCode = Keys.Down Then
                btnSeason_Click(btnSeason, New System.EventArgs)
            End If
        Catch ex As Exception
            MessageBox.Show("Error : " & ex.Message, cntTitle, _
            MessageBoxButtons.OK, MessageBoxIcon.Error)
        End Try
    End Sub

    Private Sub txtOrderNo_KeyDown(ByVal sender As Object, ByVal e As System.Windows.Forms.KeyEventArgs) Handles txtOrderNo.KeyDown
        Try
            If txtSupp.Text.Trim = "" Then
                MessageBox.Show("You Should Choose a Supplier.", cntTitle, MessageBoxButtons.OK, MessageBoxIcon.Exclamation)
                ' ReadonlyConsFash()
                txtRef.Text = "" : txtSupRef.Text = "" : txtSupColor.Text = ""
                txtSupp.Focus()
                Exit Sub
            ElseIf txtDate.Value Is DBNull.Value Then
                MessageBox.Show("Date Can Not Be Null.", cntTitle, MessageBoxButtons.OK, MessageBoxIcon.Exclamation)
                'ReadonlyConsFash()
                txtRef.Text = "" : txtSupRef.Text = "" : txtSupColor.Text = ""
                txtDate.Focus()
                Exit Sub
            ElseIf txtDelOrder.Text.Trim = "" Then
                MessageBox.Show("You Should enter A Receiving Order Ref.", cntTitle, MessageBoxButtons.OK, MessageBoxIcon.Exclamation)
                ' ReadonlyConsFash()
                txtRef.Text = "" : txtSupRef.Text = "" : txtSupColor.Text = ""
                txtDelOrder.Focus()
                Exit Sub
            End If
            If btnPurchOrder.Enabled = True AndAlso e.Modifiers = Keys.Alt AndAlso e.KeyCode = Keys.Down Then
                btnPurchOrder_Click(btnPurchOrder, New System.EventArgs)
            End If
        Catch ex As Exception
            MessageBox.Show("Error : " & ex.Message, cntTitle, _
            MessageBoxButtons.OK, MessageBoxIcon.Error)
        End Try
    End Sub

    Private Sub txtOrderNo_Validating(ByVal sender As Object, ByVal e As System.ComponentModel.CancelEventArgs) Handles txtOrderNo.Validating
        Dim strsql As String
        Dim drSql As SqlDataReader
        Try
            If Not (Wtype = "ADD" Or Wtype = "MODIFY") Then Exit Sub
            If txtDelOrder.Text.Trim = "" Then txtOrderNo.Text = "" : Exit Sub
            If txtOrderNo.Text.Trim <> "" Then
                If txtOrderNo.Text.IndexOf("'") >= 0 Then
                    e.Cancel = True
                    MessageBox.Show("Invalid Data Entry, ['] not allowed.", cntTitle, _
                    MessageBoxButtons.OK, MessageBoxIcon.Question)
                    Exit Try
                End If
            End If

            If OldVal = txtOrderNo.Text.Trim Then Exit Sub 'OldVal = "" :
            If OldVal <> "" Then
                AllLocks(AllLocksType.ReleaseLock, "STK04_mordH", OldVal, cntTitle, Me.Name)
            End If

            If txtOrderNo.Text.Trim <> "" Then

                'TEST IF PO IS STILL SAME 
                If Wtype = "MODIFY" Then
                    strsql = " SELECT DOH_Recid FROM dbo.stk04_PurDelOrdH WHERE DOH_Recid='" & txtCode.Text.Trim & "' AND DOH_PORecid= '" & txtOrderNo.Text.Trim & "'"
                    If RecordsExists(strsql) = True Then
                        Exit Sub
                    End If
                End If

                strsql = " SELECT DISTINCT V_PORD2_ManOrdList.RMO_RECID AS [PO #], V_PORD2_ManOrdList.RMO_DATE AS [PO Date], CASE V_PORD2_ManOrdList.RMO_POStatus WHEN 'U' "
                strsql &= " THEN 'Under Process' WHEN 'C' THEN 'Canceled' WHEN 'F' THEN 'Finished' ELSE '' END AS [Status], V_PORD2_ManOrdList.SUP_CODE AS [Sup. Code],"
                strsql &= " V_PORD2_ManOrdList.SUP_NAME AS [Sup. Name], CASE V_PORD2_ManOrdList.Sup_LocFor WHEN 'F' THEN 'True' ELSE 'False' END AS [Foreign], "
                strsql &= " V_PORD2_ManOrdList.RMO_DelDate AS [Delivery], V_PORD2_ManOrdList.RMO_ManualNo AS [Manual No.], V_PORD2_ManOrdList.RMO_ShipDate AS [Ship. Date], "
                strsql &= " V_PORD2_ManOrdList.RMO_TotOrdVal AS [PO Total], V_PORD2_ManOrdList.RMO_cur AS [Curr], V_PORD2_ManOrdList.Conf AS [App. No], "
                strsql &= " V_PORD2_ManOrdList.RMO_Remark AS [Remarks] From V_PORD2_ManOrdList  Inner Join (Select MOD_Hrecid from  V_PORD2_MOSTKQty Left join "
                strsql &= " V_PORD2_DOSTKQty On MOD_Hrecid = DOH_PORecid and MOD_RMRecid  = DOD_Stkrecid LEFT JOIN dbo.STK04_STKFIL ON STK_RECID=MOD_RMRecid  "
                If DelOrder = "Consignment Receiving Order Fashion" Or DelOrder = "Consignment Receiving Order" Then
                    strsql &= " AND STK_NEWUSED = 'c' "
                Else
                    strsql &= " AND STK_NEWUSED <> 'c'"
                End If
                strsql &= "  left JOIN STK04_StkFilPending ON sfp_recid = MOD_PendRecid "
                If DelOrder = "Consignment Receiving Order Fashion" Or DelOrder = "Consignment Receiving Order" Then
                    strsql &= " AND Sfp_NEWUSED = 'c' "
                Else
                    strsql &= " AND Sfp_NEWUSED <> 'c'"
                End If
                strsql &= AppStkCond("STK_Recid", , True, False, BUAccess.Purchase, True, , True)
                strsql &= " LEFT JOIN (SELECT  DOH_PORecid , dod_stkrecid ,"
                strsql &= " SUM(DOQ_Recqty * CASE WHEN doh_status = 'N' THEN 1 ELSE -1 END) AS DOQ_Recqty FROM stk04_PurDelOrdDQty "
                strsql &= " INNER JOIN dbo.stk04_PurDelOrdD ON DOD_Recid=DOQ_Drecid INNER JOIN dbo.stk04_PurDelOrdH ON DOD_Hrecid=DOH_Recid    "
                strsql &= " GROUP BY DOH_PORecid ,DOD_stkRecid,DOQ_Recqty  )TEMP ON MOD_Hrecid = TEMP.DOH_PORecid  AND MOD_RMRecid = TEMP.DOD_Stkrecid"
                strsql &= " WHERE  MOqty - ISNULL(CASE WHEN STK_ItmClass='F' OR STK_ItmClass='E' THEN TEMP.DOQ_Recqty ELSE dod_qty END , 0) > 0"
                strsql &= " ) as T1 On T1.MOD_Hrecid =V_PORD2_ManOrdList.RMO_Recid where 1=1 "

                strsql &= " and V_PORD2_ManOrdList.rmo_comp='" & gbComp & "'  and V_PORD2_ManOrdList.ConfPI =0  and ((V_PORD2_ManOrdList.conf <> 0 and V_PORD2_ManOrdList.sup_approve='Y') "
                strsql &= " or ( V_PORD2_ManOrdList.sup_approve='N'))  and V_PORD2_ManOrdList.Sup_code ='" & txtSupp.Text.Trim.Replace("'", "''") & "' "
                strsql &= " and V_PORD2_ManOrdList.RMO_POStatus='U' "

                If DelOrder = "Receiving Order Fashion" Or DelOrder = "Consignment Receiving Order Fashion" Then
                    strsql &= " AND V_PORD2_ManOrdList.RMO_POType = 'F' "
                    If txtBrand.Text.Trim <> "" And txtSeason.Text.Trim <> "" Then strsql &= " AND V_PORD2_ManOrdList.RMO_Brand='" & txtBrand.Text.Trim & "' and V_PORD2_ManOrdList.RMO_Season='" & txtSeason.Text.Trim & "'"
                Else
                    strsql &= " AND V_PORD2_ManOrdList.RMO_POType <> 'F' "
                End If
                If Wtype = "DELETE" Then strsql &= " AND V_PORD2_ManOrdList.ConfPI = 0 AND V_PORD2_ManOrdList.conf = 0 AND V_PORD2_ManOrdList.RMO_POStatus <> 'F'"
                strsql &= " and V_PORD2_ManOrdList.RMO_Recid ='" & txtOrderNo.Text.Trim.Replace("'", "''") & "'"

                ReadFromDBToDataReader(strsql, drSql)

                With drSql
                    If .HasRows Then
                        .Read()
                        If CDate(.Item("PO Date").ToString) > CDate(txtDate.Value) Then
                            MessageBox.Show("Cannot Import PO With Date Greater Then RO Date", cntTitle, MessageBoxButtons.OK, MessageBoxIcon.Exclamation)
                            Me.ActiveControl = txtOrderNo
                            Exit Sub
                        End If
                        If txtOrderNo.Text.Trim <> "" Then
                            If AllLocks(AllLocksType.CreateLock, "STK04_mordH", txtOrderNo.Text.Trim, cntTitle, Me.Name, "Purchase Order") = False Then
                                txtOrderNo.Focus() : Exit Sub
                            End If
                        End If
                        If txtOrderNo.Text.Trim <> OldVal And OldVal <> "" And txtOrderNo.Text.Trim <> "" Then
                            If MessageBox.Show("Are You Sure Want To Change The PO #, The System Will Replace The Defined Items By New PO Details", cntTitle, MessageBoxButtons.YesNo, MessageBoxIcon.Question) = vbNo Then
                                txtOrderNo.Text = OldVal
                                Exit Sub
                            Else
                                OldVal = txtOrderNo.Text.Trim
                            End If
                        End If
                        If OldVal <> "" Then
                            AllLocks(AllLocksType.ReleaseLock, "STK04_mordH", OldValue, cntTitle, Me.Name)
                        End If

                        txtOrderNo.Text = .Item("PO #").ToString



                        If DelOrder = "Receiving Order Fashion" Or DelOrder = "Consignment Receiving Order Fashion" Then
                            If dsDetDetails.Tables("tblDetails").Select("").Length() = 0 Then
                                txtBrand.Text = ""
                                txtSeason.Text = ""
                            End If
                        End If
                        OldVal = txtOrderNo.Text.Trim
                        FillGridDetails() : Exit Sub
                    Else
                        txtOrderNo.Text = ""
                        MessageBox.Show("PO # Does Not Exist", cntTitle, MessageBoxButtons.OK, MessageBoxIcon.Exclamation)
                        e.Cancel = True
                    End If
                End With
            Else
                If OldVal <> "" Then
                    AllLocks(AllLocksType.ReleaseLock, "STK04_mordH", OldVal, cntTitle, Me.Name)
                End If
            End If


            If txtOrderNo.Text.Trim <> OldVal And OldVal <> "" And txtOrderNo.Text.Trim <> "" Then
                If MessageBox.Show("Are You Sure Want To Change The PO #, The System Will Replace The Defined Items By New PO Details", cntTitle, MessageBoxButtons.YesNo, MessageBoxIcon.Question) = vbYes Then

                    strsql = "delete from " & TmpGridDetails & " where new_old='P' or new_old='W' "
                    Execute(strsql)
                    dsDetDetails.Tables(0).Clear()
                    daDetDetails.Fill(dsDetDetails, dsDetDetails.Tables(0).TableName)
                    For Idx As Integer = 0 To lstDelivery.Columns.Count - 1
                        lstDelivery.Columns(Idx).FilterText = ""
                    Next
                    lstDelivery.SelectedRows.Clear()
                    OldVal = ""
                Else
                    txtOrderNo.Text = CStr(IIf(Val(OldVal) = 0, "", OldVal))
                    Exit Sub
                End If
            End If
            ReadonlyConsFash()
            RefQtyLbl(False)
        Catch exp As Exception
            MessageBox.Show("Error: " & exp.Message, cntTitle, _
            MessageBoxButtons.OK, MessageBoxIcon.Error)
        End Try
    End Sub

    Private Sub txtDelOrder_Validating(ByVal sender As Object, ByVal e As System.ComponentModel.CancelEventArgs) Handles txtDelOrder.Validating
        Try
            keyValidate()

            If RecordsExists("SELECT TOP 1 1  FROM stk04_PurDelOrdH WHERE UPPER(DOH_Ref) = UPPER('" & txtDelOrder.Text.Trim & "')") Then
                MessageBox.Show("RO Ref. # Exist", cntTitle, MessageBoxButtons.OK, MessageBoxIcon.Exclamation)
            End If

        Catch ex As Exception
            MessageBox.Show("Error : " & ex.Message, cntTitle, _
            MessageBoxButtons.OK, MessageBoxIcon.Error)
        End Try
    End Sub

    Private Sub btnPurchOrder_Click(ByVal sender As System.Object, ByVal e As System.EventArgs) Handles btnPurchOrder.Click
        Dim strSQL1 As String
        Try
            If txtSupp.Text.Trim = "" Then
                MessageBox.Show("You Should Choose a Supplier.", cntTitle, MessageBoxButtons.OK, MessageBoxIcon.Exclamation)
                ' ReadonlyConsFash()
                txtRef.Text = "" : txtSupRef.Text = "" : txtSupColor.Text = ""
                txtSupp.Focus()
                Exit Sub
            ElseIf txtDate.Value Is DBNull.Value AndAlso supplierneedapproval = False Then
                MessageBox.Show("Date Can Not Be Null.", cntTitle, MessageBoxButtons.OK, MessageBoxIcon.Exclamation)
                'ReadonlyConsFash()
                txtRef.Text = "" : txtSupRef.Text = "" : txtSupColor.Text = ""
                txtDate.Focus()
                Exit Sub
            ElseIf txtDelOrder.Text.Trim = "" AndAlso supplierneedapproval = False Then
                MessageBox.Show("You Should enter A Receiving Order Ref.", cntTitle, MessageBoxButtons.OK, MessageBoxIcon.Exclamation)
                ' ReadonlyConsFash()
                txtRef.Text = "" : txtSupRef.Text = "" : txtSupColor.Text = ""
                txtDelOrder.Focus()
                Exit Sub
            End If

            strSQL1 = " Inner Join (Select MOD_Hrecid from  V_PORD2_MOSTKQty Left join V_PORD2_DOSTKQty"
            strSQL1 &= " On MOD_Hrecid = DOH_PORecid and MOD_RMRecid  = DOD_Stkrecid"
            strSQL1 &= " LEFT JOIN dbo.STK04_STKFIL ON STK_RECID=MOD_RMRecid "
            If DelOrder = "Consignment Receiving Order Fashion" Or DelOrder = "Consignment Receiving Order" Then
                strSQL1 &= " AND STK_NEWUSED = 'c' "
            Else
                strSQL1 &= " AND STK_NEWUSED <> 'c'"
            End If

            strSQL1 &= " left JOIN STK04_StkFilPending ON sfp_recid = MOD_PendRecid"
            If DelOrder = "Consignment Receiving Order" Or DelOrder = "Consignment Receiving Order Fashion" Then
                strSQL1 &= " AND Sfp_NEWUSED = 'c' "
            Else
                strSQL1 &= " AND Sfp_NEWUSED <> 'c'"
            End If
            strSQL1 &= AppStkCond("STK_RECID", , True, False, BUAccess.Purchase, True, , True)
            strSQL1 &= " LEFT JOIN (SELECT  DOH_PORecid , dod_stkrecid ,"
            strSQL1 &= " SUM(DOQ_Recqty * CASE WHEN doh_status = 'N' THEN 1 ELSE -1 END) AS DOQ_Recqty FROM stk04_PurDelOrdDQty "
            strSQL1 &= " INNER JOIN dbo.stk04_PurDelOrdD ON DOD_Recid=DOQ_Drecid INNER JOIN dbo.stk04_PurDelOrdH ON DOD_Hrecid=DOH_Recid    "
            strSQL1 &= " GROUP BY DOH_PORecid ,DOD_stkRecid,DOQ_Recqty  )TEMP ON MOD_Hrecid = TEMP.DOH_PORecid  AND MOD_RMRecid = TEMP.DOD_Stkrecid"
            strSQL1 &= " WHERE  MOqty - ISNULL(CASE WHEN STK_ItmClass = 'F' OR STK_ItmClass = 'E'"
            strSQL1 &= " THEN TEMP.DOQ_Recqty ELSE dod_qty END, 0) > 0 ) as T1 On T1.MOD_Hrecid =V_PORD2_ManOrdList.RMO_Recid"

            '... MO Approved Case Need To Be approved Or Not Confirmed By A Pur Inv
            strsql = " and V_PORD2_ManOrdList.ConfPI =0  and ((V_PORD2_ManOrdList.conf <> 0 and V_PORD2_ManOrdList.sup_approve='Y') or ( V_PORD2_ManOrdList.sup_approve='N'))"
            strsql &= "  and V_PORD2_ManOrdList.Sup_code ='" & txtSupp.Text.Trim & "' and V_PORD2_ManOrdList.RMO_POStatus='U'"

            RMManOrdGrid(txtDelOrder, strsql, strSQL1)

        Catch exp As Exception
            MessageBox.Show("Error: " & exp.Message, cntTitle, MessageBoxButtons.OK, MessageBoxIcon.Error)
        End Try
    End Sub

    Sub RMManOrdGrid(ByVal Text1 As TextBox, Optional ByVal Cond As String = "", Optional ByVal Cond1 As String = "")

        Dim cnSql As SqlConnection
        Dim dsSql As DataSet
        Dim daSql As SqlDataAdapter
        Dim frm As frmSearch

        Dim Sizefield() As Int16 = {80, 80, 90, 70, 120, 50, 80, 80, 80, 100, 50, 50, 120}
        Dim Alnfield() As String = {"C", "N", "N", "C", "N", "C", "N", "N", "N", "F", "C", "C", "N"}
        Dim Retfield() As String = {"PO #", "PO Date", "Status", "Sup. Code", "Sup. Name", "Foreign", "Delivery", _
                                    "Manual No.", "Ship. Date", "PO Total", "Curr", "App. No", "Remarks"}
        Dim ColumnPresentation(,) As Object = {{"Foreign", PresentationEnum.CheckBox}}

        Try

            Cursor.Current = Cursors.WaitCursor
            cnSql = New SqlConnection(ConnectionString)
            cnSql.Open()

            strsql = " SELECT DISTINCT convert(varchar,V_PORD2_ManOrdList.RMO_RECID) AS [PO #], V_PORD2_ManOrdList.RMO_DATE AS [PO Date],"
            strsql &= " CASE V_PORD2_ManOrdList.RMO_POStatus WHEN 'U' THEN 'Under Process' WHEN 'C' THEN 'Canceled'"
            strsql &= " WHEN 'F' THEN 'Finished' ELSE '' END AS [Status],"
            strsql &= " V_PORD2_ManOrdList.SUP_CODE AS [Sup. Code], V_PORD2_ManOrdList.SUP_NAME AS [Sup. Name],"
            strsql &= " CASE V_PORD2_ManOrdList.Sup_LocFor WHEN 'F' THEN 'True' ELSE 'False' END AS [Foreign],"
            strsql &= " V_PORD2_ManOrdList.RMO_DelDate AS [Delivery], V_PORD2_ManOrdList.RMO_ManualNo AS [Manual No.],"
            strsql &= " V_PORD2_ManOrdList.RMO_ShipDate AS [Ship. Date], V_PORD2_ManOrdList.RMO_TotOrdVal AS [PO Total], V_PORD2_ManOrdList.RMO_cur AS [Curr],"
            strsql &= " V_PORD2_ManOrdList.Conf AS [App. No], V_PORD2_ManOrdList.RMO_Remark AS [Remarks]"
            strsql &= " From V_PORD2_ManOrdList"
            If DelOrder = "Receiving Order Fashion" Or DelOrder = "Consignment Receiving Order Fashion" Then
                If txtBrand.Text.Trim <> "" And txtSeason.Text.Trim <> "" Then strsql &= " left join V_PORD2_ManOrdHead  ON V_PORD2_ManOrdHead.RMO_RECID=V_PORD2_ManOrdList.RMO_RECID"
            End If
            strsql &= " " + Cond1
            strsql &= " where 1=1 and V_PORD2_ManOrdList.rmo_comp='" & gbComp & "'"
            strsql &= " " + Cond

            If DelOrder = "Receiving Order Fashion" Or DelOrder = "Consignment Receiving Order Fashion" Then
                strsql &= " AND V_PORD2_ManOrdList.RMO_POType = 'F' "
                If txtBrand.Text.Trim <> "" And txtSeason.Text.Trim <> "" Then strsql &= " AND V_PORD2_ManOrdHead.RMO_Brand='" & txtBrand.Text.Trim & "' and V_PORD2_ManOrdHead.RMO_Season='" & txtSeason.Text.Trim & "'"
            Else
                strsql &= " AND V_PORD2_ManOrdList.RMO_POType <> 'F' "
            End If
            If Wtype = "DELETE" Then strsql &= " AND V_PORD2_ManOrdList.ConfPI = 0 AND V_PORD2_ManOrdList.conf = 0 AND V_PORD2_ManOrdList.RMO_POStatus <> 'F'"

            strsql &= " order by [PO Date]  desc "
            daSql = New SqlDataAdapter(strsql, cnSql)
            dsSql = New DataSet
            daSql.Fill(dsSql, "tbcode3")
            cnSql.Close()
            frm = New frmSearch
            If dsSql.Tables("tbcode3").Rows.Count = 0 AndAlso supplierneedapproval Then
                MessageBoxEx.Show("There Is No PO To Be Imported" & IIf(txtSupp.Text.Trim = "", "", " For This Supplier").ToString, cntTitle, MessageBoxButtons.OK, MessageBoxIcon.Exclamation)
                txtSupp.Focus()
                Exit Sub
            End If
            With frm
                .Owner = Me
                If .ShowDialog("Return", dsSql.Tables("tbcode3"), dsSql.Tables("tbcode3").Rows.Count, _
                "Purchase Order #", Sizefield, Alnfield, Retfield, 0, Nothing, False, 1000, 500, , True, , , , , , , , , , , , , , , , , , , , , , , , , ColumnPresentation) = DialogResult.OK Then
                    If gbRetSearch(0) = "" Then
                        txtDelOrder.Focus()
                    ElseIf CDate(gbRetSearch(1)) > CDate(txtDate.Value) Then
                        MessageBox.Show("Cannot Import PO With Date Greater Then RO Date", cntTitle, MessageBoxButtons.OK, MessageBoxIcon.Exclamation)
                        Me.ActiveControl = txtOrderNo
                    Else
                        If gbRetSearch(0) <> "" Then
                            'OldVal = txtOrderNo.Text
                            'txtOrderNo.Text = gbRetSearch(0)
                            If OldVal <> "" Then
                                AllLocks(AllLocksType.ReleaseLock, "STK04_mordH", OldVal, cntTitle, Me.Name)
                            End If

                            OldVal = txtOrderNo.Text
                            txtOrderNo.Text = gbRetSearch(0)
                            PODATE = gbRetSearch(1).Format("dd")
                            If txtOrderNo.Text.Trim <> "" Then
                                If AllLocks(AllLocksType.CreateLock, "STK04_mordH", txtOrderNo.Text.Trim, cntTitle, Me.Name, "Purchase Order") = False Then
                                    txtOrderNo.Focus() : Exit Sub
                                End If
                            End If
                            '  If Wtype = "ADD" Then
                            If OldVal = txtOrderNo.Text.Trim Then OldVal = "" : Exit Sub
                            If txtOrderNo.Text.Trim <> OldVal And OldVal <> "" Then
                                If MessageBox.Show("Are You Sure Want To Change The PO #, The System Will Replace The Defined Items By New PO Details", cntTitle, MessageBoxButtons.YesNo, MessageBoxIcon.Question) = vbNo Then
                                    txtOrderNo.Text = OldVal
                                    Exit Sub
                                Else
                                    OldVal = txtOrderNo.Text
                                End If
                            End If
                            'End If

                            OldVal = txtOrderNo.Text.Trim
                            FillGridDetails()
                            txtLoc.Focus()
                        Else
                            txtOrderNo.Focus()
                        End If
                    End If
                Else
                    If supplierneedapproval Then
                        txtSupp.Focus()
                        Exit Sub
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

    Private Sub FillGridDetails()
        Dim drsql As SqlDataReader
        Try
            ' ch @ 13/04/2018 no Consignment option when importing from a po 
            If DelOrder = "Receiving Order Fashion" Then 'Or DelOrder = "Consignment Receiving Order Fashion" Then
                strsql = " Delete from " & TmpFashQty & " where Recid in (select recid from " & TmpGridDetails & " where PO='P')"
                strsql &= " Delete from " & TmpGridDetails & " where PO='P'"
            Else
                strsql = " Delete from " & TmpGridDetails & " where PO='P'"
                strsql &= " Delete from " & TempPendingDO & " where FromPo='P'"
            End If
            Execute(strsql)
            daDetDetails.Update(dsDetDetails.Tables("tblDetails"))
            For Idx As Integer = 0 To lstDelivery.Columns.Count - 1
                lstDelivery.Columns(Idx).FilterText = ""
            Next

            lcChanges = True
            If DelOrder = "Receiving Order Fashion" Then 'Or DelOrder = "Consignment Receiving Order Fashion" Then
                Dim k As Integer
                Dim GridSizeArr() As String
                Dim Str, strGrid As String
                Dim drSizes As SqlDataReader

                Str = " SELECT  DISTINCT MOD_HRecid, MOD_SupItmRef, MOD_Stat,"
                Str &= " MOD_RMRecid, MOD_PendRecid,  MORDQ_Color MOD_Color, MORDQ_GridCode MOD_GridCode,MOD_LineNO"
                Str &= " FROM Stk04_MOrdDet"
                Str &= " INNER JOIN dbo.STK04_MordQty ON STK04_MordQty.MORDQ_HRecid = Stk04_MOrdDet.MOD_HRecid
                                AND STK04_MordQty.MORDQ_SupItmRef = Stk04_MOrdDet.MOD_SupItmRef
                                AND STK04_MordQty.MORDQ_Stat = Stk04_MOrdDet.MOD_Stat
                                AND STK04_MordQty.MORDQ_Count = Stk04_MOrdDet.MOD_Count "
                Str &= " WHERE MOD_Hrecid = '" & Val(txtOrderNo.Text.Trim.Replace("'", "''")) & "' order by MOD_LineNO"
                ReadFromDBToDataReader(Str, drsql)
                k = 0
                With drsql
                    If .HasRows Then
                        While .Read()
                            Dim strCnt As String
                            Dim CNT As Integer
                            Dim drSQLCnt As SqlDataReader
                            strCnt = "SELECT ISNULL(MAX(RECID),0) FROM " & TmpGridDetails & ""
                            ReadFromDBToDataReader(strCnt, drSQLCnt)
                            drSQLCnt.Read()

                            CNT = CInt(drSQLCnt.Item(0).ToString)
                            strGrid = "SELECT GD_SizeDesc FROM dbo.stk04_DetGRID"
                            strGrid &= " WHERE GD_Code='" & .Item("MOD_GridCode").ToString & "'"
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
                         
                            Dim STR1 As String = ""

                            STR1 = " INSERT INTO " & TempPendingDO
                            STR1 &= " SELECT (SELECT ISNULL (MAX( SFP_Recid),0) FROM " & TempPendingDO & ") + ROW_NUMBER() OVER (ORDER BY SFP_Comp )  ,SFP_Comp,SFP_SUPPLIER,SFP_CATEGORY,SFP_Serie,SFP_Usage,SFP_NEWUSED,SFP_Season,SFP_Brand,"
                            STR1 &= " SFP_Grid, SFP_Gender, SFP_Style, SFP_SingleRef1, SFP_SingleRef2, SFP_ItmClass, SFP_LOCSRC, SFP_InfDate, SFP_OpSource,"
                            STR1 &= " SFP_Desc, SFP_ImpLoc, SFP_ItmIdent, SFP_SerReg, SFP_ItmType, SFP_UNIT, SFP_UNITPACK, SFP_SUBUNIT, SFP_FCCode, SFP_VATType,"
                            STR1 &= " SFP_VATPER, SFP_StartTaxDate, SFP_CrUStamp, SFP_CrDStamp, SFP_UStamp, SFP_DStamp, SFP_HSCode ,SFP_VATSTAT ,SFP_TechSheet,sfp_operator "
                            STR1 &= " , 'O', SFP_Recid , 'P' FROM STK04_StkFilPending"
                            STR1 &= " WHERE  NOT EXISTS (SELECT SFP_Recid FROM " & TempPendingDO & " WHERE SFP_OldRecid = STK04_StkFilPending.SFP_Recid ) and "
                            STR1 &= " STK04_StkFilPending.SFP_Recid IN ( select SFP_Recid  "
                            STR1 &= " FROM   Stk04_MOrdH "
                            STR1 &= " INNER JOIN dbo.Stk04_MOrdDet ON MOD_HRecid = RMO_Recid  "
                            STR1 &= "  INNER Join dbo.STK04_MordQty ON STK04_MordQty.MORDQ_HRecid = Stk04_MOrdDet.MOD_HRecid And STK04_MordQty.MORDQ_SupItmRef = Stk04_MOrdDet.MOD_SupItmRef And STK04_MordQty.MORDQ_Stat = Stk04_MOrdDet.MOD_Stat And STK04_MordQty.MORDQ_Count = Stk04_MOrdDet.MOD_Count "
                            STR1 &= " INNER JOIN ( SELECT MOD_Hrecid Hrecid , MOD_SupItmRef SupItmRef ,   MOD_Stat Stat ,"
                            STR1 &= " COALESCE(MOD_RMRecid,MOD_PendRecid) RMRecid , MOD_Color Color , SUM(ISNULL(MOD_OrdQty, 0))"
                            STR1 &= " - SUM(ISNULL(DOD_QTY, 0)) BalQTY FROM   ( SELECT    MOD_Hrecid ,  MOD_SupItmRef , MOD_Stat ,"
                            STR1 &= " MOD_RMRecid , MOD_PendRecid, MORDQ_Color MOD_Color , SUM(ISNULL(MORDQ_Recqty, 0)) MOD_OrdQty  FROM Stk04_MOrdDet "
                            STR1 &= "  INNER Join dbo.STK04_MordQty ON STK04_MordQty.MORDQ_HRecid = Stk04_MOrdDet.MOD_HRecid And STK04_MordQty.MORDQ_SupItmRef = Stk04_MOrdDet.MOD_SupItmRef And STK04_MordQty.MORDQ_Stat = Stk04_MOrdDet.MOD_Stat And STK04_MordQty.MORDQ_Count = Stk04_MOrdDet.MOD_Count "
                            STR1 &= " GROUP BY  MOD_Hrecid , MOD_SupItmRef,  MOD_Stat, MOD_RMRecid, MOD_PendRecid,  MORDQ_Color ) MOrdDet"
                            STR1 &= " LEFT JOIN ( SELECT DOH_PORecid , DOD_STKRECID , SUM(ISNULL(DOD_QTY, 0)) DOD_QTY"
                            STR1 &= " FROM V_PORD2_DOSTKQty1 GROUP BY DOH_PORecid , DOD_STKRECID ) DOSTKQty1 ON MOD_Hrecid = DOH_PORecid"
                            STR1 &= " AND DOD_STKRECID = COALESCE(MOD_RMRECID,MOrdDet.MOD_PendRecid) GROUP BY MOD_Hrecid ,"
                            STR1 &= " MOD_SupItmRef , MOD_Stat, MOD_RMRecid, MOD_PendRecid, MOD_Color"
                            STR1 &= " ) ordqtybal ON MOD_Hrecid = Hrecid AND MOD_SupItmRef = SupItmRef AND MOD_Stat = Stat"
                            STR1 &= " AND COALESCE(MOD_RMRecid,MOD_PendRecid) = RMRecid AND MORDQ_Color = Color      "
                            STR1 &= " LEFT JOIN Stk04_stkfil ON stk_recid = MOD_RMRecid"
                            If DelOrder = "Consignment Receiving Order" Or DelOrder = "Consignment Receiving Order Fashion" Then
                                STR1 &= " AND STK_NEWUSED  = 'c' "
                            Else
                                STR1 &= " AND STK_NEWUSED <> 'c' "
                            End If
                            STR1 &= " inner JOIN STK04_StkFilPending ON sfp_recid = MOD_PendRecid"
                            If DelOrder = "Consignment Receiving Order" Or DelOrder = "Consignment Receiving Order Fashion" Then
                                STR1 &= " AND Sfp_NEWUSED = 'c' "
                            Else
                                STR1 &= " AND Sfp_NEWUSED <> 'c'"
                            End If
                            STR1 &= " LEFT JOIN  dbo.Stk04_Gender ON COALESCE(STK_Gender,SFP_Gender)=G_Code "
                            STR1 &= " INNER JOIN stk04_measurements ON Meas_unit = COALESCE(stk_subunit,SFP_SUBUNIT) "
                            STR1 &= AppStkCond("STK_Recid", , True, False, BUAccess.Purchase, True, , True)
                            STR1 &= " WHERE MOD_PendRecid<>0 AND NOT MOD_PendRecid is NULL AND MOD_Hrecid = '" & Val(txtOrderNo.Text.Trim.Replace("'", "''")) & "'"
                            STR1 &= " AND MOD_SupItmRef = '" & .Item("MOD_SupItmRef").ToString & "'"
                            STR1 &= " AND MOD_Stat = '" & .Item("MOD_Stat").ToString & "'"
                            If Val(.Item("MOD_RMRecid").ToString) <> 0 Then
                                STR1 &= " AND MOD_RMRecid = '" & .Item("MOD_RMRecid").ToString & "'"
                            ElseIf Val(.Item("MOD_PendRecid").ToString) <> 0 Then
                                STR1 &= " AND MOD_PendRecid = '" & .Item("MOD_PendRecid").ToString & "'"
                            End If
                            STR1 &= " AND MORDQ_Color = '" & .Item("MOD_Color").ToString & "'"
                            STR1 &= " AND COALESCE(STK_ItmClass,SFP_ITMCLASS) = 'F' AND MOD_OrdQty > 0 "
                            If txtBrand.Text.Trim <> "" And txtSeason.Text.Trim <> "" Then STR1 &= " and RMO_Brand= '" & txtBrand.Text.Trim & "' and RMO_Season = '" & txtSeason.Text.Trim & "'"
                            STR1 &= " )"
                            Execute(STR1)

                            strsql = " INSERT INTO  " & TmpGridDetails
                            strsql &= " SELECT " & CNT & " + ROW_NUMBER() OVER (ORDER BY MOD_LineNO ) AS Cnt, "
                            strsql &= "  ASD ,"
                            strsql &= "  MOD_SupItmRef ,"
                            strsql &= "  MOD_SuppColor ,"
                            strsql &= "  stk_ref ,"
                            strsql &= "  G_Code ,"
                            strsql &= "  MOD_Stat ,"
                            strsql &= "  STK_ITMTYPE ,"
                            strsql &= "  STK_Style ,"
                            strsql &= "  STK_RECID ,"
                            strsql &= "  MOD_Count ,"
                            strsql &= "  Stk_Desc ,"
                            strsql &= "  MOD_Color ,"
                            strsql &= "  OLD ,"
                            strsql &= "  MOD_OrdQty ,"
                            strsql &= "  SFP_unit ,"
                            strsql &= "  SFP_SUBunit ,"
                            strsql &= "  MOD_ConsOrdQty ,"
                            strsql &= "  SFP_VATTYPE ,"
                            strsql &= "  Cost , FcCode , "
                            strsql &= "  WQWE ,"
                            strsql &= "  pppopoo ,"
                            strsql &= "  QRQT ,"
                            strsql &= "  STK_ItmClass ,"
                            strsql &= "  btn "
                            strsql &= " FROM ("

                            strsql &= " SELECT DISTINCT  null ASD, MOD_SupItmRef, ISNULL(MOD_SuppColor,MORDQ_SuppColor) MOD_SuppColor, ISNULL(stk_ref,"
                            For i = 0 To CurrItmKeyPend.Length - 1
                                'ch @ 17/11/2016 wf: 4618 to avoid a error  (Error converting data type varchar to numeric.)
                                If CurrItmKeyPend(i).ToUpper = "SFP_SERIE" Then
                                    strsql &= " convert(varchar ," & CurrItmKeyPend(i) & ") + '/' + "
                                Else
                                    strsql &= CurrItmKeyPend(i) & " + '/' + "
                                End If
                            Next
                            strsql = strsql.Substring(1, strsql.Length() - 9)
                            strsql &= " ) stk_ref,G_Code,"
                            strsql &= " MOD_Stat," 'CASE WHEN MOD_Stat = 'N' THEN 'Normal' WHEN MOD_Stat = 'F' THEN 'Free' WHEN MOD_Stat = 'R' THEN 'Return' END AS MOD_Stat,"
                            strsql &= " ISNULL(STK_ITMTYPE,SFP_ITMTYPE) STK_ITMTYPE, ISNULL(STK_Style,SFP_Style) STK_Style,"

                            strsql &= " ISNULL(STK_RECID,SFP_RECID) STK_RECID,NULL MOD_Count,ISNULL(Stk_Desc,SFP_DESC)  Stk_Desc ,MORDQ_Color MOD_Color,'O' OLD,"

                            ''sum of qty will be updated at the end with the sum of fashion qty
                            'strsql &= " BalQTY AS MOD_OrdQty,"
                            strsql &= " 0 AS MOD_OrdQty,"
                            strsql &= " MEASU.meas_desc SFP_unit," 'MOD_OrdUnit, " ' MOD_UnitCost,  "
                            'CH @ 21/06/2017 
                            strsql &= " MEASUB.meas_desc SFP_SUBunit,"
                            'strsql &= " CASE meas_sign WHEN '*' THEN  (BalQTY)* ISNULL(stk_unitpack,sFP_unitpack)  *"
                            'strsql &= "  meas_value ELSE ( ISNULL(stk_unitpack,sFP_unitpack) * (BalQTY / Meas_value ))End MOD_ConsOrdQty ,"
                            strsql &= " 0 AS MOD_ConsOrdQty,"
                            Str = "Select case Meas_Sign WHEN '*' THEN  case when pn1.sfp_operator ='*' then " & Val(txtQty.Text.Trim) & " *  pn1.SFP_UNITPACK  * STK04_Measurements.Meas_Value else " & Val(txtQty.Text.Trim) & " /  pn1.SFP_UNITPACK  * STK04_Measurements.Meas_Value end "
                            Str &= " ELSE case when pn1.sfp_operator ='*' then (  pn1.SFP_UNITPACK   * " & Val(txtQty.Text.Trim) & " ) /   STK04_Measurements.Meas_Value else (   " & Val(txtQty.Text.Trim) & "  / pn1.SFP_UNITPACK  ) /   STK04_Measurements.Meas_Value end  End [ctr]"
                            Str &= " FROM   STK04_StkFilPending pn1"
                            Str &= " left JOIN STK04_Measurements ON Meas_unit =  pn1.SFP_SUBUNIT "
                            Str &= " WHERE  pn1.SFP_Recid = " & Val(txtSTkID.Text.Trim) & " "
                            'W FOR NEW ITEM  /  P FROM po
                            strsql &= " ISNULL(STK_VATTYPE ,SFP_VATTYPE ) SFP_VATTYPE, "
                            If txtCurr.Text.Trim = "" Then
                                strsql &= "     ISNULL(STK_AVGCOSTCC ,0)   Cost , "
                            Else
                                strsql &= "    (CASE WHEN '" & txtCurr.Text.Trim & "'='" & gbCurr1 & "' "
                                strsql &= "  THEN ISNULL(STK_AVGCOSTLC ,SFP_AVGCOSTLC)  WHEN '" & txtCurr.Text.Trim & "'='" & gbCurr2 & "' "
                                strsql &= "  THEN ISNULL(STK_AVGCOSTCC ,SFP_AVGCOSTCC)  ELSE dbo.[convert5]('" & gbApp & "','" & gbComp & "','" & gbYear & "',"
                                strsql &= " ISNULL(STK_AVGCOSTFC ,SFP_AVGCOSTFC),STK_FCCODE,'" & txtCurr.Text.Trim & "','" & CType(Now, Date).ToString("MM/dd/yyyy") & "')   END) Cost  ,"
                            End If
                            strsql &= " RMO_cur  FcCode, CASE WHEN MOD_RMRecid IS NULL THEN 'W' ELSE case when STK_ItmClass='e' then 's' else 'N' END END WQWE,'P' pppopoo,null QRQT , COALESCE(STK_ItmClass,SFP_ITMCLASS) STK_ItmClass , '' btn,MOD_LineNO "
                            strsql &= " FROM   Stk04_MOrdH "
                            strsql &= " INNER JOIN dbo.Stk04_MOrdDet ON MOD_HRecid = RMO_Recid  "

                            strsql &= "  INNER Join dbo.STK04_MordQty ON STK04_MordQty.MORDQ_HRecid = Stk04_MOrdDet.MOD_HRecid And STK04_MordQty.MORDQ_SupItmRef = Stk04_MOrdDet.MOD_SupItmRef And STK04_MordQty.MORDQ_Stat = Stk04_MOrdDet.MOD_Stat And STK04_MordQty.MORDQ_Count = Stk04_MOrdDet.MOD_Count"
                            strsql &= "  INNER JOIN ( SELECT MOD_Hrecid Hrecid , MOD_SupItmRef SupItmRef ,   MOD_Stat Stat ,"
                            strsql &= "  COALESCE(MOD_RMRecid,MOD_PendRecid) RMRecid , MOD_Color Color , SUM(ISNULL(MOD_OrdQty, 0))"
                            strsql &= " - SUM(ISNULL(DOD_QTY, 0)) BalQTY FROM   ( SELECT    MOD_Hrecid ,  MOD_SupItmRef , MOD_Stat ,"
                            strsql &= " MOD_RMRecid , MOD_PendRecid, MORDQ_Color MOD_Color , SUM(ISNULL(MOD_OrdQty, 0)) MOD_OrdQty  FROM Stk04_MOrdDet "
                            strsql &= "	 INNER Join dbo.STK04_MordQty ON STK04_MordQty.MORDQ_HRecid = Stk04_MOrdDet.MOD_HRecid And STK04_MordQty.MORDQ_SupItmRef = Stk04_MOrdDet.MOD_SupItmRef And STK04_MordQty.MORDQ_Stat = Stk04_MOrdDet.MOD_Stat And STK04_MordQty.MORDQ_Count = Stk04_MOrdDet.MOD_Count
                             "
                            strsql &= " GROUP BY  MOD_Hrecid , MOD_SupItmRef,  MOD_Stat, MOD_RMRecid, MOD_PendRecid,  MORDQ_Color ) MOrdDet"
                            strsql &= " LEFT JOIN ( SELECT DOH_PORecid , DOD_STKRECID , SUM(ISNULL(DOD_QTY, 0)) DOD_QTY"
                            strsql &= " FROM V_PORD2_DOSTKQty1 GROUP BY DOH_PORecid , DOD_STKRECID ) DOSTKQty1 ON MOD_Hrecid = DOH_PORecid"
                            strsql &= " AND DOD_STKRECID = COALESCE(MOD_RMRECID,MOrdDet.MOD_PendRecid) GROUP BY MOD_Hrecid ,"
                            strsql &= " MOD_SupItmRef , MOD_Stat, MOD_RMRecid, MOD_PendRecid, MOD_Color"
                            strsql &= " ) ordqtybal ON MOD_Hrecid = Hrecid AND MOD_SupItmRef = SupItmRef AND MOD_Stat = Stat"
                            strsql &= " AND COALESCE(MOD_RMRecid,MOD_PendRecid) = RMRecid AND MORDQ_Color = Color      "
                            strsql &= " LEFT JOIN Stk04_stkfil ON stk_recid = MOD_RMRecid"
                            '  If DelOrder = "Consignment Receiving Order" Or DelOrder = "Consignment Receiving Order Fashion" Then
                            'strsql &= " AND STK_NEWUSED  = 'c' "
                            '  Else
                            strsql &= " AND STK_NEWUSED <> 'c' "
                            ' End If
                            ''strsql &= " left JOIN STK04_StkFilPending ON sfp_recid = MOD_PendRecid"
                            'temp Pending Do to take the pending recid not the STK04_StkFilPending recid 
                            strsql &= " left JOIN " & TempPendingDO & " ON SFP_OldRecid = MOD_PendRecid  "
                            ' If DelOrder = "Consignment Receiving Order" Or DelOrder = "Consignment Receiving Order Fashion" Then
                            'strsql &= " AND Sfp_NEWUSED = 'c' "
                            '  Else
                            strsql &= " AND Sfp_NEWUSED <> 'c'"
                            ' End If
                            strsql &= " LEFT JOIN  dbo.Stk04_Gender ON COALESCE(STK_Gender,SFP_Gender)=G_Code "
                            strsql &= " inner join stk04_measurements MEASU on measu.meas_unit =  ISNULL(stk_unit ,SFP_unit )  "
                            strsql &= " inner join stk04_measurements MEASUB on measub.meas_unit = COALESCE(stk_subunit,SFP_SUBUNIT)  "
                            strsql &= AppStkCond("STK_Recid", , True, False, BUAccess.Purchase, True, , True)
                            strsql &= " WHERE MOD_Hrecid = '" & Val(txtOrderNo.Text.Trim.Replace("'", "''")) & "'"
                            strsql &= " AND MOD_SupItmRef = '" & .Item("MOD_SupItmRef").ToString & "'"
                            strsql &= " AND MOD_Stat = '" & .Item("MOD_Stat").ToString & "'"
                            If Val(.Item("MOD_RMRecid").ToString) <> 0 Then
                                strsql &= " AND MOD_RMRecid = '" & .Item("MOD_RMRecid").ToString & "'"
                            ElseIf Val(.Item("MOD_PendRecid").ToString) <> 0 Then
                                strsql &= " AND MOD_PendRecid = '" & .Item("MOD_PendRecid").ToString & "'"
                            End If
                            strsql &= " AND MORDQ_Color = '" & .Item("MOD_Color").ToString & "'"
                            strsql &= " AND COALESCE(STK_ItmClass,SFP_ITMCLASS) = 'F' AND MOD_OrdQty>0"
                            If txtBrand.Text.Trim <> "" And txtSeason.Text.Trim <> "" Then Str &= " and RMO_Brand= '" & txtBrand.Text.Trim & "' and RMO_Season = '" & txtSeason.Text.Trim & "'"
                            strsql &= " ) AS Temp"
                            Execute(strsql)
                            strsql = ""
                            If txtBrand.Text.Trim = "" Or txtSeason.Text.Trim = "" Then
                                strsql = " select br_code,Seas_Code,br_name,Seas_Active,Seas_Desc,br_Active from Stk04_MOrdH "
                                strsql &= " left join stk04_brand on br_code=RMO_Brand  left join stk04_season on Seas_Code=RMO_Season "
                                strsql &= " WHERE RMO_Recid = '" & Val(txtOrderNo.Text.Trim.Replace("'", "''")) & "'"
                                ReadFromDBToDataReader(strsql, drsql)
                                If drsql.HasRows Then
                                    drsql.Read()
                                    If UCase(drsql.Item("br_Active").ToString) = "N" Then
                                        MessageBox.Show("Brand of this PO is Inactive.", cntTitle, MessageBoxButtons.OK, MessageBoxIcon.Exclamation)
                                        txtOrderNo.Text = "" : txtOrderNo.Focus()
                                        Exit Sub
                                    Else
                                        If drsql.Item("br_code").ToString = "" Then
                                            MessageBox.Show("Brand of this PO,Does Not Exist.", cntTitle, MessageBoxButtons.OK, MessageBoxIcon.Exclamation)
                                            txtOrderNo.Text = "" : txtOrderNo.Focus()
                                            Exit Sub
                                        Else
                                            txtBrand.Text = drsql.Item("br_code").ToString
                                            lblBrand.Text = drsql.Item("br_name").ToString
                                        End If
                                    End If
                                    If UCase(drsql.Item("Seas_Active").ToString) = "N" Then
                                        MessageBox.Show("Season of this PO is Inactive.", cntTitle, MessageBoxButtons.OK, MessageBoxIcon.Exclamation)
                                        txtOrderNo.Text = "" : txtOrderNo.Focus()
                                        Exit Sub
                                    Else
                                        If drsql.Item("Seas_Code").ToString = "" Then
                                            MessageBox.Show("Season of this PO,Does Not Exist.", cntTitle, MessageBoxButtons.OK, MessageBoxIcon.Exclamation)
                                            txtOrderNo.Text = "" : txtOrderNo.Focus()
                                            Exit Sub
                                        Else
                                            txtSeason.Text = drsql.Item("Seas_Code").ToString
                                            lblSeason.Text = drsql.Item("Seas_Desc").ToString
                                        End If
                                    End If
                                Else
                                    MessageBox.Show("Brand and Season of this PO,Does Not Exist.", cntTitle, MessageBoxButtons.OK, MessageBoxIcon.Exclamation)
                                    txtOrderNo.Text = "" : txtOrderNo.Focus()
                                    Exit Sub
                                End If
                                btnBrand.Enabled = False : btnSeason.Enabled = False : txtBrand.ReadOnly = True : txtSeason.ReadOnly = True
                            End If
                            'insert into Quantity Fashion 
                            '
                            strsql = " INSERT INTO  " & TmpFashQty
                            strsql &= " SELECT  ( SELECT  ISNULL(MAX(Q_RECID),0) FROM " & TmpFashQty & " ) + ROW_NUMBER() OVER (ORDER BY MORDQ_GridCode ) AS Cnt,  "
                            strsql &= " (Select max (recid) from  " & TmpGridDetails & ") ,* from (SELECT distinct MORDQ_GridCode , "
                            For k = 1 To MaxCnt
                                If k <= GridSizeArr.Length Then
                                    strsql &= " (SELECT CASE WHEN MIN(MORDQ_Recqty) - sum(ISNULL(DOQ_Recqty, 0))<=0 THEN NULL ELSE MIN(MORDQ_Recqty) - sum(ISNULL(DOQ_Recqty, 0)) END FROM STK04_morddet"
                                    strsql &= " INNER JOIN dbo.STK04_MordQty ON STK04_MordQty.MORDQ_HRecid = Stk04_MOrdDet.MOD_HRecid
                                                              AND STK04_MordQty.MORDQ_SupItmRef = Stk04_MOrdDet.MOD_SupItmRef
                                                              AND STK04_MordQty.MORDQ_Stat = Stk04_MOrdDet.MOD_Stat
                                                              AND STK04_MordQty.MORDQ_Count = Stk04_MOrdDet.MOD_Count "
                                    strsql &= " LEFT JOIN dbo.stk04_PurDelOrdH ON MOD_Hrecid = DOH_PORecid"
                                    strsql &= " LEFT JOIN dbo.stk04_PurDelOrdD ON DOD_Hrecid = DOH_Recid AND MOD_RMRecid = dod_stkrecid AND DOD_ColorCode =MORDQ_Color"
                                    strsql &= " LEFT JOIN stk04_PurDelOrdDQty ON dbo.stk04_PurDelOrdD.DOD_Recid = dbo.stk04_PurDelOrdDQty.DOQ_Drecid AND DOQ_Gridsize=MORDQ_GridSize"

                                    strsql &= " WHERE MOD_Hrecid = " & .Item("MOD_HRecid").ToString
                                    strsql &= " AND MOD_SupItmRef = '" & .Item("MOD_SupItmRef").ToString & "'"
                                    strsql &= " AND MOD_Stat = '" & .Item("MOD_Stat").ToString & "'"
                                    If Val(.Item("MOD_RMRecid").ToString) <> 0 Then
                                        strsql &= " AND MOD_RMRecid = '" & .Item("MOD_RMRecid").ToString & "'"
                                    ElseIf Val(.Item("MOD_PendRecid").ToString) <> 0 Then
                                        strsql &= " AND MOD_PendRecid = '" & .Item("MOD_PendRecid").ToString & "'"
                                    End If
                                    strsql &= " AND MOrDq_Color = '" & .Item("MOD_Color").ToString & "'"
                                    strsql &= " AND MORDQ_GridSize = '" & GridSizeArr(k - 1) & "') AS QTY" & k & ","
                                Else
                                    strsql &= " NULL AS Size" & k & ","
                                End If
                            Next
                            strsql = strsql.Substring(1, strsql.Length() - 2)
                            strsql &= " FROM Stk04_MOrdH "
                            strsql &= " INNER JOIN dbo.Stk04_MOrdDet ON MOD_HRecid = RMO_Recid  "
                            strsql &= " INNER JOIN dbo.STK04_MordQty ON  STK04_MordQty.MORDQ_HRecid = Stk04_MOrdDet.MOD_HRecid 
                                        AND STK04_MordQty.MORDQ_SupItmRef = Stk04_MOrdDet.MOD_SupItmRef AND STK04_MordQty.MORDQ_Stat = Stk04_MOrdDet.MOD_Stat 
                                        AND STK04_MordQty.MORDQ_Count = Stk04_MOrdDet.MOD_Count"
                            strsql &= " INNER JOIN ( SELECT MOD_Hrecid Hrecid , MOD_SupItmRef SupItmRef ,   MOD_Stat Stat ,"
                            strsql &= " COALESCE(MOD_RMRecid,MOD_PendRecid) RMRecid , MOD_Color Color , SUM(ISNULL(MOD_OrdQty, 0))"
                            strsql &= " - SUM(ISNULL(DOD_QTY, 0)) BalQTY FROM   ( SELECT    MOD_Hrecid ,  MOD_SupItmRef , MOD_Stat ,"
                            strsql &= " MOD_RMRecid , MOD_PendRecid, MORDQ_Color MOD_Color , SUM(ISNULL(MORDQ_Recqty, 0)) MOD_OrdQty  FROM Stk04_MOrdDet "
                            strsql &= " INNER JOIN dbo.STK04_MordQty ON  STK04_MordQty.MORDQ_HRecid = Stk04_MOrdDet.MOD_HRecid 
                                        AND STK04_MordQty.MORDQ_SupItmRef = Stk04_MOrdDet.MOD_SupItmRef AND STK04_MordQty.MORDQ_Stat = Stk04_MOrdDet.MOD_Stat 
                                        AND STK04_MordQty.MORDQ_Count = Stk04_MOrdDet.MOD_Count"
                            strsql &= " GROUP BY  MOD_Hrecid , MOD_SupItmRef,  MOD_Stat, MOD_RMRecid, MOD_PendRecid,  MORDQ_Color ) MOrdDet"
                            strsql &= " LEFT JOIN ( SELECT DOH_PORecid , DOD_STKRECID , SUM(ISNULL(DOD_QTY, 0)) DOD_QTY"
                            strsql &= " FROM V_PORD2_DOSTKQty1 GROUP BY DOH_PORecid , DOD_STKRECID ) DOSTKQty1 ON MOD_Hrecid = DOH_PORecid"
                            strsql &= " AND DOD_STKRECID = COALESCE(MOD_RMRECID,MOrdDet.MOD_PendRecid) GROUP BY MOD_Hrecid ,"
                            strsql &= " MOD_SupItmRef , MOD_Stat, MOD_RMRecid, MOD_PendRecid, MOD_Color"
                            strsql &= " ) ordqtybal ON MOD_Hrecid = Hrecid AND MOD_SupItmRef = SupItmRef AND MOD_Stat = Stat"
                            strsql &= " AND COALESCE(MOD_RMRecid,MOD_PendRecid) = RMRecid AND MOrDq_Color = Color      "
                            strsql &= " LEFT JOIN Stk04_stkfil ON stk_recid = MOD_RMRecid"
                            '  If DelOrder = "Consignment Receiving Order" Or DelOrder = "Consignment Receiving Order Fashion" Then
                            'strsql &= " AND STK_NEWUSED  = 'c' "
                            '  Else
                            strsql &= " AND STK_NEWUSED  <> 'c'"
                            '  End If
                            ''strsql &= " left JOIN STK04_StkFilPending ON sfp_recid = MOD_PendRecid"
                            'temp Pending Do to take the pending recid not the STK04_StkFilPending recid 
                            strsql &= " left JOIN " & TempPendingDO & " ON SFP_OldRecid = MOD_PendRecid  "

                            '  If DelOrder = "Consignment Receiving Order" Or DelOrder = "Consignment Receiving Order Fashion" Then
                            'strsql &= " AND Sfp_NEWUSED = 'c' "
                            '  Else
                            strsql &= " AND Sfp_NEWUSED <> 'c'"
                            'End If
                            ' strsql &= " LEFT JOIN  dbo.Stk04_Group ON STK_Group=Grp_Code "
                            strsql &= " LEFT JOIN  dbo.Stk04_Gender ON COALESCE(STK_Gender,SFP_Gender)=G_Code "
                            strsql &= " INNER JOIN stk04_measurements ON Meas_unit = COALESCE(stk_subunit,SFP_SUBUNIT) "
                            strsql &= AppStkCond("STK_recid", , True, False, BUAccess.Purchase, True, , True)
                            strsql &= " WHERE MOD_Hrecid = '" & Val(txtOrderNo.Text.Trim.Replace("'", "''")) & "'"
                            strsql &= " AND MOD_SupItmRef = '" & .Item("MOD_SupItmRef").ToString & "'"
                            strsql &= " AND MOD_Stat = '" & .Item("MOD_Stat").ToString & "'"
                            If Val(.Item("MOD_RMRecid").ToString) <> 0 Then
                                strsql &= " AND MOD_RMRecid = '" & .Item("MOD_RMRecid").ToString & "'"
                            ElseIf Val(.Item("MOD_PendRecid").ToString) <> 0 Then
                                strsql &= " AND MOD_PendRecid = '" & .Item("MOD_PendRecid").ToString & "'"
                            End If
                            strsql &= " AND MOrDq_Color = '" & .Item("MOD_Color").ToString & "'"
                            strsql &= " AND COALESCE(STK_ItmClass,SFP_ITMCLASS) = 'F' AND MOD_OrdQty>0"
                            If txtBrand.Text.Trim <> "" And txtSeason.Text.Trim <> "" Then Str &= " and RMO_Brand= '" & txtBrand.Text.Trim & "' and RMO_Season = '" & txtSeason.Text.Trim & "'"
                            strsql &= " ) as temp"
                            Execute(strsql)
                        End While


                        'update the sum qty of all sizes in the details grid
                        strsql = " update " & TmpGridDetails & " SET MOD_OrdQty =fashion.totQty,"
                        strsql &= " MOD_ConsOrdQty=CASE meas_sign WHEN '*' THEN case when COALESCE(stk_operator,SFP_operator)='*' then   (fashion.totQty)* ISNULL(stk_unitpack,sFP_unitpack) * "
                        strsql &= "  meas_value else   (fashion.totQty)/ ISNULL(stk_unitpack,sFP_unitpack) * "
                        strsql &= "  meas_value end   ELSE case when COALESCE(stk_operator,SFP_operator)='*' then ( (fashion.totQty  * ISNULL(stk_unitpack,sFP_unitpack))  / Meas_value ) else ( (fashion.totQty /ISNULL(stk_unitpack,sFP_unitpack)) / Meas_value ) end End "
                        strsql &= " FROM " & TmpGridDetails & " left join  (select recid, "
                        For k = 1 To MaxCnt
                            If k <= GridSizeArr.Length Then
                                strsql &= " isnull(Size" & k & ",0) + "

                            End If
                        Next k
                        strsql = strsql.Substring(1, strsql.Length() - 3)
                        strsql &= " as totQty  from " & TmpFashQty & " ) Fashion on Fashion.Recid=" & TmpGridDetails & ".Recid"
                        strsql &= " LEFT JOIN Stk04_stkfil ON stk_recid = " & TmpGridDetails & ".MOD_RMRecid"
                        '   If DelOrder = "Consignment Receiving Order" Or DelOrder = "Consignment Receiving Order Fashion" Then
                        'strsql &= " AND STK_NEWUSED  = 'c' "
                        ' Else
                        strsql &= " AND STK_NEWUSED  <> 'c'"
                        ' End If
                        'temp Pending Do to take the pending recid not the STK04_StkFilPending recid 
                        strsql &= " left JOIN " & TempPendingDO & " ON SFP_RECID = " & TmpGridDetails & ".MOD_RMRecid"

                        'If DelOrder = "Consignment Receiving Order" Or DelOrder = "Consignment Receiving Order Fashion" Then
                        'strsql &= " AND Sfp_NEWUSED = 'c' "
                        ' Else
                        strsql &= " AND Sfp_NEWUSED <> 'c'"
                        ' End If
                        strsql &= " INNER JOIN stk04_measurements ON Meas_unit = COALESCE(STK04_STKFIL.stk_subunit,SFP_SUBUNIT) "
                        strsql &= " WHERE Fashion.totQty  IS NOT NULL AND  Fashion.totQty > 0 "
                        Execute(strsql)
                        'Update Counter
                        strsql = "   DELETE FROM " & TmpGridDetails & " WHERE MOD_OrdQty <= 0 ; update " & TmpGridDetails & " SET mod_count= counter "
                        strsql &= " FROM (SELECT ROW_NUMBER() OVER (PARTITION BY mod_supitmref,mod_stat ORDER BY recid) counter,Recid recid2 "
                        strsql &= " FROM " & TmpGridDetails & ") test WHERE recid2=recid"
                        Execute(strsql)
                        strsql = " Select * from " & TmpGridDetails & " order by Recid "
                        'cnSql = New SqlConnection(ConnectionString)
                        'cnSql.Open()
                        'If Not dsDetDetails.Tables("tblDetails") Is Nothing Then dsDetDetails.Tables.Remove("tblDetails")
                        'daDetDetails = New SqlDataAdapter(strsql, cnSql)
                        'daDetDetails.TableMappings.Add("Table", "tblDetails")
                        dsDetDetails.Tables(0).Clear()
                        daDetDetails.Fill(dsDetDetails, dsDetDetails.Tables(0).TableName)
                        lstDelivery.Refresh()
                        ReadonlyConsFash()
                        INITCOL(False, True)
                        RefGridSizes(lstFashQty.Columns("GridCode").Text.Trim)
                    End If
                End With

            Else

                Dim STR1 As String = ""

                STR1 = " INSERT INTO  " & TmpGridBarcode & ""
                STR1 &= " SELECT  (select isnull(max(B_RECID),0)  from " & TmpGridBarcode & " ) + POBr_Recid , " '-- B_RECID - numeric"
                STR1 &= " (SELECT dbar_recid FROM  dbo.STK04_DetBarcode WHERE dbar_barcode COLLATE  SQL_Latin1_General_CP1256_CI_AS = POBr_Barcode COLLATE  SQL_Latin1_General_CP1256_CI_AS ) , " '-- DOB_RECID - numeric"
                STR1 &= " POBr_DetRecID +  (SELECT ISNULL (MAX( SFP_Recid),0) FROM " & TempPendingDO & ") , " '-- RECID - numeric"
                STR1 &= " NULL , " '-- E_RECID - numeric"
                STR1 &= " POBr_Barcode , 'O'  , null " '-- bar_stat - v
                STR1 &= " From STK04_MordDBE where  POBr_HedRecID = " & Val(txtOrderNo.Text.Trim.Replace("'", "''")) & " "
                Execute(STR1)

                STR1 = " INSERT INTO " & TempPendingDO
                STR1 &= " SELECT (SELECT ISNULL (MAX( SFP_Recid),0) FROM " & TempPendingDO & ") + ROW_NUMBER() OVER (ORDER BY SFP_Comp )  ,SFP_Comp,SFP_SUPPLIER,SFP_CATEGORY,SFP_Serie,SFP_Usage,SFP_NEWUSED,SFP_Season,SFP_Brand,"
                STR1 &= " SFP_Grid, SFP_Gender, SFP_Style, SFP_SingleRef1, SFP_SingleRef2, SFP_ItmClass, SFP_LOCSRC, SFP_InfDate, SFP_OpSource,"
                STR1 &= " SFP_Desc, SFP_ImpLoc, SFP_ItmIdent, SFP_SerReg, SFP_ItmType, SFP_UNIT, SFP_UNITPACK,SFP_SUBUNIT, SFP_FCCode, SFP_VATType,"
                STR1 &= " SFP_VATPER, SFP_StartTaxDate, SFP_CrUStamp, SFP_CrDStamp, SFP_UStamp, SFP_DStamp, SFP_HSCode, SFP_VATSTAT ,SFP_TechSheet , sfp_operator "
                STR1 &= " , 'O', SFP_Recid ,'P' FROM STK04_StkFilPending"
                STR1 &= " WHERE NOT EXISTS (SELECT SFP_Recid FROM " & TempPendingDO & " WHERE SFP_OldRecid = STK04_StkFilPending.SFP_Recid ) and STK04_StkFilPending.SFP_Recid IN ( select SFP_Recid  "
                STR1 &= " FROM    V_PORD2_MOSTKQty1 T0"
                STR1 &= " LEFT JOIN V_PORD2_DOSTKQty1 ON MOD_Hrecid = DOH_PORecid"
                STR1 &= " AND dod_itmRef = Mod_SupitmRef AND DOD_StkRecid = MOD_RMRecid AND DOD_Stat = MOD_stat"
                STR1 &= " LEFT JOIN Stk04_stkfil ON stk_recid = MOD_RMRecid"
                STR1 &= " AND STK_NEWUSED  <> 'c'"
                STR1 &= " left JOIN STK04_StkFilPending ON sfp_recid = MOD_PendRecid"
                STR1 &= " AND Sfp_NEWUSED <> 'c'"
                STR1 &= " INNER JOIN stk04_measurements ON Meas_unit = ISNULL(stk_subunit,sFP_subunit)"
                STR1 &= AppStkCond("stk_recid", , True, False, BUAccess.Purchase, True, , True)
                STR1 &= " WHERE(MOD_HRecid = '" & Val(txtOrderNo.Text.Trim.Replace("'", "''")) & "') and ( MOQty - ISNULL(DOD_Qty, 0)) > 0 "
                STR1 &= " )"
                Execute(STR1)
                strsql = " INSERT INTO " & TmpGridDetails
                strsql &= " SELECT distinct ROW_NUMBER() OVER ( ORDER BY MOD_LineNO ) + (SELECT  ISNULL(MAX(Recid), 0) FROM " & TmpGridDetails & ")  , tmpp.col1, "
                strsql &= " MOD_SupItmRef, stk_ref, [Status], MOD_RMRecid, col5, stk_desc, QTY, SFP_unit , SFP_subunit, col2, col3, Cost, STK_FCCODE, WQWE, popopo, barcd, STK_ItmClass, [btn]"
                strsql &= " FROM ( SELECT DISTINCT Null col1, MOD_SupItmRef,MOD_LineNO ,"
                strsql &= "  ISNULL(stk_ref,"
                For i = 0 To CurrItmKeyPend.Length - 1
                    If CurrItmKeyPend(i).ToUpper = "SFP_SERIE" Then
                        strsql &= " convert(varchar ," & CurrItmKeyPend(i) & ") + '/' + "
                    Else
                        strsql &= CurrItmKeyPend(i) & " + '/' + "
                    End If
                Next
                strsql = strsql.Substring(1, strsql.Length() - 9)
                'ch @ 06/10/2016 
                ' strsql &= " ) stk_ref, MOD_Stat [Status],isnull(MOD_RMRecid," & TempPendingDO & ".SFP_recid ) MOD_RMRecid," 'CASE WHEN MOD_Stat = 'F' THEN 'Free' WHEN MOD_Stat = 'R' THEN 'Return' WHEN MOD_Stat = 'N' THEN 'Normal' WHEN MOD_Stat is NULL THEN 'Normal' END AS [status]
                strsql &= " ) stk_ref,  isnull(MOD_Stat,'N')  AS [Status],isnull(MOD_RMRecid," & TempPendingDO & ".SFP_recid ) MOD_RMRecid,"
                strsql &= " 1 col5,ISNULL(stk_desc,sfp_desc) stk_desc, CASE WHEN    ISNULL(STK_ItmClass, SFP_ItmClass) ='E' THEN 0 ELSE ( MOQty - ISNULL(DOD_Qty, 0) ) end  as QTY, ISNULL(stk_unit ,SFP_unit ) SFP_unit,ISNULL(stk_subunit ,SFP_subunit ) SFP_subunit ,"
                strsql &= " ( CASE WHEN    ISNULL(STK_ItmClass, SFP_ItmClass) ='E' THEN 0 ELSE  ( MOQty - ISNULL(DOD_Qty, 0) ) END ) * ISNULL(stk_unitPack,sfp_unitPack) * CASE WHEN Meas_sign = '*' THEN Meas_Value "
                strsql &= " ELSE 1 / Meas_value  END col2,"
                'R.A. 07/05/2014''''''''''''''''''''''''''''''''
                ' strsql &= " CASE WHEN ISNULL(STK_VATTYPE,SFP_VATTYPE) = 'T' THEN 'T-Taxable' WHEN STK_VATTYPE = 'N'"
                ' strsql &= " AND STK_VatPer > 0 THEN 'E-Taxable Embeded' ELSE 'N-Not Taxable' END col3,"
                strsql &= " ISNULL(STK_VATTYPE,SFP_VATTYPE) col3,"
                strsql &= " MOD_UnitCost Cost, "
                'If txtCurr.Text.Trim = "" Then
                '    strsql &= "    STK_AVGCOSTCC  Cost ,"
                'Else
                '    strsql &= " case when  STK_FCCODE is null then 0 else case when  '" & txtCurr.Text.Trim & "' = '" & gbCurr1 & "' then STK_AVGCOSTLC when  '" & txtCurr.Text.Trim & "' = '" & gbCurr2 & "' then STK_AVGCOSTCC else dbo.[convert5]('" & gbApp & "','" & gbComp & "','" & gbYear & "',"
                '    strsql &= " STK_AVGCOSTFC,STK_FCCODE,'" & txtCurr.Text.Trim & "','" & CType(Now, Date).ToString("MM/dd/yyyy") & "')    end  end Cost ,"
                'End If
                strsql &= " '" & txtCurr.Text.Trim & "' STK_FCCODE ,"
                strsql &= " (CASE WHEN MOD_RMRecid IS  NULL THEN 'W' ELSE  CASE WHEN STK_ItmClass='e' THEN 'S'ELSE 'N' END  END ) WQWE,"

                strsql &= " 'P' popopo,null barcd,isnull( STK_ItmClass,SFP_ItmClass) STK_ItmClass, '' [btn] "

                ''''''''''''''''''''''''''''''''''''''''''''''''
                strsql &= " FROM    V_PORD2_MOSTKQty1 T0"
                strsql &= " LEFT JOIN V_PORD2_DOSTKQty1 ON MOD_Hrecid = DOH_PORecid"
                strsql &= " AND dod_itmRef = Mod_SupitmRef AND DOD_StkRecid = MOD_RMRecid AND DOD_Stat = MOD_stat"
                ' strsql &= " AND MOD_Hrecid <> DOH_PORecid"
                strsql &= " LEFT JOIN Stk04_stkfil ON stk_recid = MOD_RMRecid"
                strsql &= " AND STK_NEWUSED <> 'c'"
                ' strsql &= " left JOIN STK04_StkFilPending ON sfp_recid = MOD_PendRecid"
                'temp Pending Do to take the pending recid not the STK04_StkFilPending recid 
                strsql &= " left JOIN " & TempPendingDO & " ON SFP_OldRecid = MOD_PendRecid  "
                strsql &= " AND Sfp_NEWUSED <> 'c'"
                strsql &= " INNER JOIN stk04_measurements ON Meas_unit = ISNULL(stk_subunit,sFP_subunit)"
                strsql &= AppStkCond("stk_recid", , True, False, BUAccess.Purchase, True, , True)

                strsql &= " WHERE(MOD_HRecid = '" & Val(txtOrderNo.Text.Trim.Replace("'", "''")) & "') and ( MOQty - ISNULL(DOD_Qty, 0)) > 0 "
                strsql &= " ) tmpp "

                'strsql &= "   LEFT JOIN dbo.Stk04_MOrdDet ON Stk04_MOrdDet.MOD_SupItmRef = tmpp.MOD_SupItmRef AND Stk04_MOrdDet.MOD_Stat = tmpp.Status "
                'strsql &= "									AND MOD_HRecid = '" & Val(txtOrderNo.Text.Trim.Replace("'", "''")) & "'"

                Execute(strsql)

                strsql = "update " & TmpGridDetails & " SET mod_count= counter "
                strsql &= " FROM (SELECT ROW_NUMBER() OVER (PARTITION BY mod_supitmref, LEFT(mod_stat,1) ORDER BY recid) counter,Recid recid2 "
                strsql &= " FROM " & TmpGridDetails & ") test WHERE recid2=recid"
                Execute(strsql)


                strsql = " Select * from " & TmpGridDetails & " order by Recid "
                ' daDetDetails.TableMappings.Add("Table", "tblDetails")
                dsDetDetails.Tables(0).Clear()
                daDetDetails.Fill(dsDetDetails, dsDetDetails.Tables(0).TableName)
            End If


            RefQtyLbl(False)
            LstVisbility(False, False, lstDelivery.Columns("ItmClass").Text.Trim)

            '''insert into TempPending is zer is PO Pending items 
            ''' INSERT INTO temp
            strsql = " SELECT DISTINCT STK04_StkFilPending.* FROM dbo.STK04_StkFilPending "
            strsql &= " INNER JOIN Stk04_MOrdDet ON dbo.STK04_StkFilPending.SFP_Recid = dbo.Stk04_MOrdDet.MOD_PendRecid"
            strsql &= " WHERE MOD_HRecid= " & txtOrderNo.Text.Trim

            ReadFromDBToDataReader(strsql, drsql)
            If drsql.HasRows Then
                While drsql.Read
                    InsertPendItem(TempPendingDO, drsql.Item("SFP_Recid").ToString)
                End While
            End If

            ''update stkID from stkpending to temppending 
            strsql = " UPDATE " & TmpGridDetails & " SET MOD_RMRecid=sfp_recid"
            strsql &= " FROM " & TmpGridDetails & " INNER JOIN " & TempPendingDO & " ON MOD_RMRecid=SFP_OldRecid"
            strsql &= " WHERE (po = 'P')"
            Execute(strsql)

            ''''test of this PO have items to be added
            strsql = " Select * from " & TmpGridDetails & " where PO='P' "
            ReadFromDBToDataReader(strsql, drsql)
            If Not drsql.HasRows Then
                MessageBox.Show("This PO # is Already Linked To a Receiving Order", cntTitle, MessageBoxButtons.OK, MessageBoxIcon.Exclamation)
                txtOrderNo.Text = ""
                txtOrderNo.Focus()
            End If

            ObjSumNormal = dsDetDetails.Tables("tblDetails").Compute("Sum( MOD_OrdQty)", " MOD_stat='N' ")
            ObjSumReturn = dsDetDetails.Tables("tblDetails").Compute("Sum( MOD_OrdQty)", " MOD_stat='R' ")
            objSum = CDbl(IIf(ObjSumNormal Is DBNull.Value, "0", ObjSumNormal)) - CDbl(IIf(ObjSumReturn Is DBNull.Value, "0", ObjSumReturn))
            lstDelivery.Splits(0).DisplayColumns("MOD_OrdQty").DataColumn.FooterText = CType(objSum, Double).ToString("N" & gbDecNo)
        Catch ex As Exception
            Throw ex
        End Try
    End Sub

    Private Sub btnStat_Click(ByVal sender As System.Object, ByVal e As System.EventArgs) Handles txtStatus.ButtonCustomClick
        Try
            btnStatus(txtStatus, Me)
        Catch ex As Exception
            MessageBox.Show("Error : " & ex.Message, cntTitle, _
        MessageBoxButtons.OK, MessageBoxIcon.Error)
        End Try
    End Sub

    Private Sub btnColor_Click(ByVal sender As System.Object, ByVal e As System.EventArgs) Handles txtColor.ButtonCustomClick
        Dim lbl As New Label
        Try
            ColorList(txtColor, lbl, Me, "", True, False)
        Catch ex As Exception
            MessageBox.Show("Error : " & ex.Message, cntTitle, _
            MessageBoxButtons.OK, MessageBoxIcon.Error)
        End Try
    End Sub

    Private Sub FashOrTrading()
        Try
         
            If DelOrder = "Receiving Order" Or DelOrder = "Consignment Receiving Order" Then
                PnlHead2.Visible = False
                PnlFashQty.Visible = False
                txtSupColor.Visible = False
                txtGender.Visible = False
                txtProdGroup.Visible = False
                txtStyle.Visible = False
                txtColor.Visible = False
                btnBarcode.Visible = True

                btnBarcode.Enabled = False

                txtSupRef.Width = 170
                txtRef.Width = 200
                '  txtStatus.Width = 55
                '  txtDesc.Width = 303

                If DelOrder = "Consignment Receiving Order" Then
                    TxtDisc.ReadOnly = False
                    TxtDisc.Visible = True
                    TxtNet.Visible = True
                    TxtDisc.TabStop = True
                    txtDesc.Width = Flowhead.Width - (25 + txtSupRef.Width + txtRef.Width + _
                                    txtStatus.Width + txtQty.Width + TXTUNIT.Width + txtSUBUnit.Width + txtConsQty.Width + _
                                    txtPrice.Width + TxtDisc.Width + TxtNet.Width + txtVat.Width + btnBarcode.Width + btnQA.Width)
                Else
                    TxtDisc.Visible = False
                    TxtNet.Visible = False
                    txtDesc.Width = Flowhead.Width - (25 + txtSupRef.Width + txtRef.Width + txtStatus.Width + _
                                    txtQty.Width + TXTUNIT.Width + txtSUBUnit.Width + txtConsQty.Width + txtPrice.Width + txtVat.Width + btnBarcode.Width + btnQA.Width)
                End If
            Else
                PnlFashQty.Visible = False
                If DelOrder = "Consignment Receiving Order Fashion" Then
                    TxtDisc.Visible = True
                    TxtNet.Visible = True
                    TxtDisc.ReadOnly = False
                    TxtDisc.TabStop = True
                    txtDesc.Width = Flowhead.Width - (25 + txtSupRef.Width + +TxtNet.Width + TxtDisc.Width + txtSupColor.Width + txtGender.Width + txtProdGroup.Width + txtStyle.Width + txtColor.Width + txtRef.Width + txtStatus.Width + txtQty.Width + _
                                 TXTUNIT.Width + txtSUBUnit.Width + txtConsQty.Width + txtPrice.Width + txtVat.Width + btnBarcode.Width + btnQA.Width)
                Else
                    TxtDisc.Visible = False
                    TxtNet.Visible = False
                    txtDesc.Width = Flowhead.Width - (25 + txtSupRef.Width + txtSupColor.Width + txtGender.Width + txtProdGroup.Width + txtStyle.Width + txtColor.Width + txtRef.Width + txtStatus.Width + txtQty.Width + _
                                    TXTUNIT.Width + txtSUBUnit.Width + txtConsQty.Width + txtPrice.Width + txtVat.Width + btnBarcode.Width + btnQA.Width)
                End If
            End If

        Catch ex As Exception
            MessageBox.Show("Error : " & ex.Message, cntTitle, _
            MessageBoxButtons.OK, MessageBoxIcon.Error)
        End Try
    End Sub

    Private Sub txtStatus_Validating(ByVal sender As Object, ByVal e As System.ComponentModel.CancelEventArgs) Handles txtStatus.Validating
        Try
            If txtStatus.Text.Trim = "" Then Exit Sub
            Select Case UCase(txtStatus.Text.Trim.Replace("'", "''"))
                '    Case "N", "F", "NORMAL", "FREE", "R", "Return", "RETURN"
                Case "F", "FREE" : txtStatus.Text = "F" : RefQtyLbl(False)
                Case "N", "NORMAL" : txtStatus.Text = "N" : RefQtyLbl(False)
                Case "R", "RETURN" : txtStatus.Text = "R" : RefQtyLbl(False)
                Case "C", "CONSIGNMENT" : txtStatus.Text = "C" : RefQtyLbl(False)
                Case Else
                    MessageBox.Show("Invalid Status", cntTitle, MessageBoxButtons.OK, MessageBoxIcon.Exclamation)
                    txtStatus.Text = ""
                    txtStatus.Focus()
                    Exit Sub
            End Select
            If txtOldNew.Text = "W" AndAlso txtStatus.Text.Trim = "R" Then
                ShowNotification(Me, "Pending Item Status Can Be N or F", 2000, 14, DevComponents.DotNetBar.eToastPosition.MiddleCenter)
                txtStatus.Text = ""
                txtStatus.Focus()
                Exit Sub
            End If
            If txtItmClass.Text.Trim = "F" Then
                e.Cancel = CheckItemQtyCntrEcF()
            Else
                e.Cancel = CheckItemQtyCntrEc()
            End If

        Catch ex As Exception
            MessageBox.Show("Error : " & ex.Message, cntTitle, _
            MessageBoxButtons.OK, MessageBoxIcon.Error)
        End Try
    End Sub



    Private Sub txtColor_Validating(ByVal sender As Object, ByVal e As System.ComponentModel.CancelEventArgs) Handles txtColor.Validating
        Try
            If txtColor.Text.Trim = "" Then Exit Sub
            If Not RecordsExists(ColorsStrsql("Color_Code = '" & txtColor.Text.Trim.Replace("'", "''") & "'", False)) Then
                MessageBox.Show("Invalid Color", cntTitle, MessageBoxButtons.OK, MessageBoxIcon.Exclamation)
                txtColor.Text = ""
                txtColor.Focus()
                Exit Sub
            End If
            If txtSTkID.Text.Trim <> "" Then
                strsql = "SELECT STQ_RecId FROM stk04_stkqty WHERE STQ_STKRECID='" & txtSTkID.Text.Trim & "' "
                strsql &= " AND STQ_Color='" & txtColor.Text.Trim & "'  AND STQ_LOCCODE='00'"
                Dim drsql As SqlDataReader
                ReadFromDBToDataReader(strsql, drsql)
                If drsql.HasRows Then
                    txtColorON.Text = "O"
                    txtColor.ForeColor = Color.Black
                Else
                    txtColorON.Text = "N"
                    txtColor.ForeColor = Color.Red
                End If
                e.Cancel = CheckItemQtyCntrEcF()
            End If
        Catch ex As Exception
            MessageBox.Show("Error : " & ex.Message, cntTitle, _
            MessageBoxButtons.OK, MessageBoxIcon.Error)
        End Try
    End Sub

    Private Sub GetItem(ByVal itemkey As String, Optional ByVal recId As String = "", Optional ByVal SupRef As String = "", Optional ByVal SuppColor As String = "", Optional ByVal Color As String = "", Optional ByVal SuppColorValidate As String = "")
        Dim dsSql1 As New DataSet
        Dim count As Int32
        Dim ModTrs As Boolean
        Dim frm As frmSearch
        Dim daa As New DataTable
        Dim drs As DataRow()
        Dim OneItem As Boolean = False
        Dim Sizefield() As Int16 = {}
        Dim Alnfield() As String = {}
        Dim Retfield() As String = {}
        Try
            If txtSupRef.Text.Trim = "" And txtSupColor.Text.Trim = "" And txtRef.Text.Trim = "" Then Exit Sub
            Exists = True
            Dim suprecId As String = GetFieldDesc("Stk04_supplier", "Sup_Code='" & txtSupp.Text.Trim.Replace("'", "''") & "'", "Sup_RecId")
            Dim cond As String
            If DelOrder = "Receiving Order" Then
                cond = "  (STK_ItmClass='B' or STK_ItmClass='E' or STK_ItmClass='T') and STK_NEWUSED<>'c'"
                cond &= " and not exists ( select top 1 1 from V_StockNormalCons  where stk_recid =NSTK_RECID ) "
            ElseIf DelOrder = "Receiving Order Fashion" Then
                cond = "   STK_ItmClass='F'and STK_NEWUSED<>'c' and  STK_Brand='" & txtBrand.Text.Trim & "' and STK_Season='" & txtSeason.Text.Trim & "'"
                cond &= " and not exists ( select top 1 1 from V_StockNormalCons  where stk_recid =NSTK_RECID ) "
            ElseIf DelOrder = "Consignment Receiving Order" Then
                cond = "   (STK_ItmClass='B' or STK_ItmClass='E' or STK_ItmClass='T') and STK_NEWUSED='c' AND stk_supplier='" & txtSupp.Text.Trim & "'"
                cond &= " and not exists ( select top 1 1 from V_StockNormalCons  where stk_recid =NSTK_RECID ) "
            ElseIf DelOrder = "Consignment Receiving Order Fashion" Then
                cond = "    STK_ItmClass='F'and STK_NEWUSED='c' AND stk_supplier='" & txtSupp.Text.Trim & "'"
                cond &= " and  STK_Brand='" & txtBrand.Text.Trim & "' and STK_Season='" & txtSeason.Text.Trim & "'"
                cond &= " and not exists ( select top 1 1 from V_StockNormalCons  where stk_recid =NSTK_RECID ) "
            End If
            If recId <> "" Then
                cond &= " and stk_recid=" & recId
            End If
            'stock item filtration 
            'If SupRef <> "" Then
            '    cond &= " and SRef_SupRef='" & SupRef & "'"
            'End If
            If SuppColor <> "" Then
                cond &= " and SRef_SupColor='" & SuppColor & "'"
            End If
            If Color <> "" Then
                cond &= " and STQ_Color='" & Color & "'"
            End If
            If SuppColorValidate <> "" Then
                cond &= " and SRef_SupRef='" & itemkey.Trim.Replace("'", "''") & "' and SRef_SupColor='" & SuppColorValidate & "'"
            End If
            itemValidation(itemkey.Trim.Replace("'", "''"), "temp_ItemValidation_" & lcSteps & "_" & gbUser, , cond, , , suprecId)
            strsql = " SELECT DISTINCT temp.Stk_Recid AS [RecID], supref as [Supplier Ref], "
            If DelOrder = "Receiving Order Fashion" Or DelOrder = "Consignment Receiving Order Fashion" Then
                strsql &= " suprefcolor as [Sup.Color],  "
            End If
            strsql &= " temp.STK_SingleRef1 as [Reference 1],temp.STK_REF AS [Item Ref.],"
            If DelOrder = "Receiving Order Fashion" Or DelOrder = "Consignment Receiving Order Fashion" Then
                strsql &= " Color_Reference as [Color], "
            End If

            strsql &= " temp.Stk_desc AS [Description] "
            If DelOrder = "Receiving Order Fashion" Or DelOrder = "Consignment Receiving Order Fashion" Then
                strsql &= " , Stk04_stkfil.stk_gender AS [Gender], Stk04_stkfil.STK_ITMTYPE AS [Prd.Grp]"
                strsql &= " , Stk04_stkfil.STK_Style AS [Style], Stk04_stkfil.STK_GRID AS [Grid],STQ_Color"
            End If
            strsql &= " ,Stk04_stkfil.STK_Active,Stk04_stkfil.stk_currentarchive,Stk04_stkfil.STK_HaltActTill,Stk04_stkfil.STK_BBFDate,Stk04_stkfil.STK_HaltPoPi,Stk04_stkfil.STK_ItmClass,Stk04_stkfil.stk_Subunit,Stk04_stkfil.stk_vattype,STQ_Color,Stk04_stkfil.stk_unit ,measu.meas_desc UDESC, measub.meas_desc Subdesc"
            strsql &= " from temp_ItemValidation_" & lcSteps & "_" & gbUser & " temp "
            strsql &= " inner join  Stk04_stkfil on  Stk04_stkfil.Stk_Recid=temp.Stk_Recid"
            strsql &= " inner join stk04_measurements measu on measu.meas_unit = stk_unit  "
            strsql &= " inner join stk04_measurements measub on measub.meas_unit = stk_subunit  "
            strsql &= " LEFT JOIN bibl1_Book ON stk_bookid = bo_recid"
            strsql &= " LEFT JOIN Stk04_COLOR ON STQ_Color = dbo.Stk04_COLOR.Color_Code  "
            strsql &= " AND    isnull(bo_type,'')<>'R' "
            dsSql1 = Nothing


            ReadFromDBToDataSet(strsql, "tbcode", dsSql1)
            If dsSql1.Tables(0).Rows.Count = 0 Then

                If GetPendingInfo(itemkey.Trim) = "NO ITEM FOUND" Then
                    If DelOrder = "Consignment Receiving Order" OrElse DelOrder = "Consignment Receiving Order Fashion" Then
                        itemValidation(itemkey.Trim.Replace("'", "''"), , daa, cond.Replace(" AND stk_supplier='" & txtSupp.Text.Trim & "'", " "), , , suprecId)
                        If daa.Rows.Count > 0 Then
                            ShowNotification(Me, "This Item exist For Another Supplier", 5000, 12, DevComponents.DotNetBar.eToastPosition.MiddleCenter)
                            INITCOL(True, False)
                            txtSupRef.Focus()
                            Exit Sub
                        Else
                            If MessageBox.Show("Item Not Found In Stock File, Do You Want To Create It ?", _
                                                       cntTitle, MessageBoxButtons.YesNo, MessageBoxIcon.Question) = vbNo Then
                                INITCOL(True, False)
                                txtSupRef.Focus()
                                Exit Sub
                            Else
                                btnCreate_Click(btnCreate, New System.EventArgs)
                            End If
                        End If
                    Else
                        If RecordsExists("SELECT TOP 1 1 FROM  STK04_STKFIL WHERE (STK_REF='" & itemkey.Trim.Replace("'", "''") & "' OR STK_SINGLEREF1 = '" & itemkey.Trim.Replace("'", "''") & "' OR STK_SINGLEREF2='" & itemkey.Trim.Replace("'", "''") & "' ) AND STK_ACTIVE ='n' ") Then
                            ShowNotification(Me, "Inactive Item", 3000, 12, DevComponents.DotNetBar.eToastPosition.MiddleCenter)
                            INITCOL(True, False)
                            txtSupRef.Focus()
                            Exit Sub
                        End If
                        If MessageBox.Show("Item Not Found In Stock File, Do You Want To Create It ?", _
                                                        cntTitle, MessageBoxButtons.YesNo, MessageBoxIcon.Question) = vbNo Then
                            INITCOL(True, False)
                            txtSupRef.Focus()
                            Exit Sub
                        Else
                            btnCreate_Click(btnCreate, New System.EventArgs)
                        End If
                    End If

                    Exists = False
                    Exit Try
                Else
                    Exit Sub
                End If

            End If

            If dsSql1.Tables(0).Rows.Count > 0 Then


                'Check if in trading mode the result is one item
                'If DelOrder = "Receiving Order" Or DelOrder = "Consignment Receiving Order" Then
                i = 0 : OneItem = True
                drs = dsSql1.Tables(0).Select("1=1", "")
                For i = 0 To drs.Length - 1
                    If i = 0 Then
                        txtSTkID.Text = CStr(CInt(drs(i)("recid")))
                    Else
                        If txtSTkID.Text <> CStr(CInt(drs(i)("recid"))) Then
                            OneItem = False
                            i = drs.Length - 1
                        End If
                    End If
                Next
                'End If
                If OneItem = True Then
                    If txtSupRef.Text.Trim = "" Or txtSupRef.Text = dsSql1.Tables(0).Rows(0)("Item Ref.").ToString Then
                        If dsSql1.Tables(0).Rows(0)("Supplier Ref").ToString <> "" Then

                            txtSupRef.Text = dsSql1.Tables(0).Rows(0)("Supplier Ref").ToString
                        Else
                            txtSupRef.Text = dsSql1.Tables(0).Rows(0)("Reference 1").ToString
                        End If
                    End If

                    Exists = True
                    If dsSql1.Tables(0).Rows(0)("stk_currentarchive").ToString = "A" Then
                        MessageBox.Show("Archived Item!", cntTitle, MessageBoxButtons.OK, MessageBoxIcon.Exclamation)
                        Exists = False : INITCOL(True) : Exit Sub
                    ElseIf UCase(dsSql1.Tables(0).Rows(0)("stk_active").ToString) = "N" Then
                        MessageBox.Show("Inactive Item!", cntTitle, MessageBoxButtons.OK, MessageBoxIcon.Exclamation)
                        Exists = False : INITCOL(True) : Exit Sub
                    ElseIf dsSql1.Tables(0).Rows(0)("STK_HaltActTill").ToString <> "" Then
                        If CType(dsSql1.Tables(0).Rows(0)("STK_HaltActTill"), Date).ToString("MM/dd/yyyy") > CType(Now, Date).ToString("MM/dd/yyyy") And UCase(dsSql1.Tables(0).Rows(0)("STK_HaltPoPi").ToString) = "Y" Then
                            MessageBox.Show("Item’s Activity Is Halted!", cntTitle, MessageBoxButtons.OK, MessageBoxIcon.Exclamation)
                            Exists = False : INITCOL(True) : Exit Sub
                        End If
                    End If


                    '...test if BBFDate < Receiving Order's Date................................................................
                    If Not dsSql1.Tables(0).Rows(0)("STK_BBFDate").ToString = "" Then
                        If CDate(dsSql1.Tables(0).Rows(0)("STK_BBFDate")) > CDate(txtDate.Value) Then
                            MessageBox.Show("This Item Has A BBF Date > than Receiving Order's Date." + Chr(13) + "BBF Date :" & CDate(dsSql1.Tables(0).Rows(0)("STK_BBFDate")).ToString("dd/MM/yyyy").ToString, cntTitle, MessageBoxButtons.OK, MessageBoxIcon.Exclamation)
                            INITCOL(True)
                            Exists = False
                            Exit Sub
                        End If
                    End If

                    '...Verify If Is Duplicate supplier code +Item Reference
                    'If PORD2_GETPROD(lstDelivery.Columns("MOD_supitmref").Text.Trim.Replace("'", "''"), txtSupp.Text, "", dsSql1.Tables(0).Rows(0)("RecID").ToString) = True Then
                    '    INITCOL(True)
                    '    InitGrids()
                    '    Exit Sub
                    'End If
                    Dim newRow As DataRow

                    '...Refresh item Infos 
                    Dim dt As DataTable
                    dt = dsDetDetails.Tables("tblDetails")
                    Dim dv As New DataView(dt)
                    dv.RowFilter = "RecId =Max(RecId)"
                    If dv.Count = 0 Then
                        MaxRecID = 0
                    Else
                        MaxRecID = CLng(dv(0)(0).ToString)
                    End If

                    txtRecid.Text = CStr(MaxRecID + 1)

                    lcChanges = True

                    If txtSupRef.Text.Trim = "" Then txtSupRef.Text = dsSql1.Tables(0).Rows(0)("Supplier Ref").ToString
                    If txtSupRef.Text.Trim = "" Then txtSupRef.Text = dsSql1.Tables(0).Rows(0)("Reference 1").ToString() '("Reference 1").ToString
                    ' If txtSupRef.Text.Trim = dsSql1.Tables(0).Rows(0)("Item Ref.").ToString Then txtSupRef.Text = dsSql1.Tables(0).Rows(0)("Reference 1").ToString() '("Reference 1").ToString

                    txtRef.Text = dsSql1.Tables(0).Rows(0)("Item Ref.").ToString

                    txtSTkID.Text = dsSql1.Tables(0).Rows(0)("Recid").ToString
                    txtDesc.Text = dsSql1.Tables(0).Rows(0)("Description").ToString
                    txtSUBUnit.Tag = dsSql1.Tables(0).Rows(0)("stk_Subunit").ToString
                    TXTUNIT.Tag = dsSql1.Tables(0).Rows(0)("stk_unit").ToString
                    txtSUBUnit.Text = dsSql1.Tables(0).Rows(0)("Subdesc").ToString
                    TXTUNIT.Text = dsSql1.Tables(0).Rows(0)("udesc").ToString
                    txtItmClass.Text = dsSql1.Tables(0).Rows(0)("STK_ItmClass").ToString

                    'btnBarcode.Enabled = True
                    If DelOrder = "Receiving Order Fashion" Or DelOrder = "Consignment Receiving Order Fashion" Then

                        txtQty.ReadOnly = True
                        txtGender.Text = dsSql1.Tables(0).Rows(0)("Gender").ToString
                        txtProdGroup.Text = dsSql1.Tables(0).Rows(0)("Prd.Grp").ToString
                        txtStyle.Text = dsSql1.Tables(0).Rows(0)("Style").ToString
                        If txtSupColor.Text.Trim = "" Then txtSupColor.Text = dsSql1.Tables(0).Rows(0)("Sup.Color").ToString
                        If dsSql1.Tables(0).Rows.Count = 1 Then
                            txtColor.Text = dsSql1.Tables(0).Rows(0)("STQ_Color").ToString
                        End If
                        If txtColor.Text.Trim <> "" Then
                            strsql = "SELECT STQ_RecId FROM stk04_stkqty WHERE STQ_STKRECID='" & txtSTkID.Text.Trim & "' "
                            strsql &= " AND STQ_Color='" & txtColor.Text.Trim & "'  AND STQ_LOCCODE='00'"
                            Dim drsql As SqlDataReader
                            ReadFromDBToDataReader(strsql, drsql)
                            If drsql.HasRows Then
                                txtColorON.Text = "O"
                                txtColor.ForeColor = Drawing.Color.Black
                            Else
                                txtColorON.Text = "N"
                                txtColor.ForeColor = Drawing.Color.Red
                            End If
                        End If

                        strsql = "INSERT INTO " & TmpFashQty
                        strsql &= " VALUES ('" & CStr(CDbl(GetMaxRecId(TmpFashQty, "Q_RECID"))) & "','" & txtRecid.Text.Trim & "',"
                        strsql &= " '" & dsSql1.Tables(0).Rows(0)("Grid").ToString & "'"
                        For i = 0 To MaxCnt - 1
                            strsql &= " ,NULL"
                        Next
                        strsql &= " )"
                        Execute(strsql)


                        If Not dsDetFqty.Tables("tblDetails") Is Nothing Then dsDetFqty.Tables("tblDetails").Clear()
                        daDetFqty.Fill(dsDetFqty, "tblDetails")

                        lstFashQty.AllowUpdate = True
                        RefGridSizes(dsSql1.Tables(0).Rows(0)("Grid").ToString)
                        PnlFashQty.Visible = True
                    End If
                    ' If DelOrder = "Consignment Receiving Order" Or DelOrder = "Consignment Receiving Order Fashion" Then
                    ' PnlConsigment.Visible = True
                    If DelOrder = "Consignment Receiving Order" Or DelOrder = "Consignment Receiving Order Fashion" Then
                        txtPrice.ReadOnly = False
                        TxtDisc.ReadOnly = False
                    Else
                        txtPrice.ReadOnly = True
                        TxtDisc.ReadOnly = True
                    End If
                    If txtCurr.Text.Trim = "" Then
                        strsql = "select  STK_AVGCOSTCC from Stk04_stkfil "
                    Else
                        strsql = "select STK_FCCODE,STK_AVGCOSTCC,dbo.[convert5]('" & gbApp & "','" & gbComp & "','" & gbYear & "',"
                        strsql &= " STK_AVGCOSTFC,STK_FCCODE,'" & txtCurr.Text.Trim & "','" & CType(Now, Date).ToString("MM/dd/yyyy") & "')STK_AVGCOSTFC ,STK_AVGCOSTLC from Stk04_stkfil "
                    End If
                    strsql &= " where stk_recid = " & txtSTkID.Text.Trim
                    Dim DRSS As SqlDataReader
                    ReadFromDBToDataReader(strsql, DRSS)
                    If DRSS.HasRows Then
                        DRSS.Read()
                        ' TxtItemCurr.Text = DRSS.Item("STK_FCCODE").ToString
                        '   LblCostCurr.Text = DRSS.Item("STK_FCCODE").ToString
                        If txtCurr.Text.Trim = "" Then
                            txtPrice.Text = DRSS.Item("STK_AVGCOSTCC").ToString
                        Else
                            If txtCurr.Text.Trim = gbCurr1 Then
                                txtPrice.Text = DRSS.Item("STK_AVGCOSTLC").ToString
                            ElseIf txtCurr.Text.Trim = gbCurr2 Then
                                txtPrice.Text = DRSS.Item("STK_AVGCOSTCC").ToString
                            Else
                                txtPrice.Text = DRSS.Item("STK_AVGCOSTFC").ToString
                            End If
                        End If
                        If txtPrice.Text.Trim <> "" Then
                            txtPrice.Text = CType(txtPrice.Text, Double).ToString("N3")
                        End If
                    End If

                    '  End If


                    txtStatus.Text = "N"

                    txtCounter.Text = CStr(GetDetSer(txtSupRef.Text.Trim.Replace("'", "''"), txtStatus.Text.Trim.Substring(0, 1), CInt(txtSTkID.Text.Trim)))
                    lcChanges = True
                    lcRef = False
                    Old = False

                    txtVat.Text = dsSql1.Tables(0).Rows(0)("stk_vattype").ToString
                    'If dsSql1.Tables(0).Rows(0)("stk_vattype").ToString = "T" Then txtVat.Text = "Taxable"
                    'If dsSql1.Tables(0).Rows(0)("stk_vattype").ToString = "E" Then txtVat.Text = "Taxable Embeded"
                    'If dsSql1.Tables(0).Rows(0)("stk_vattype").ToString = "N" Then txtVat.Text = "Not Taxable"

                    If DelOrder = "Receiving Order" Or DelOrder = "Consignment Receiving Order" Then

                        LstBarcode.Enabled = True
                        LstExpiry.Enabled = True

                        LstBarcode.AllowAddNew = True
                        LstBarcode.AllowUpdate = True
                        LstBarcode.AllowDelete = True

                        LstExpiry.AllowAddNew = True
                        LstExpiry.AllowUpdate = True
                        LstExpiry.AllowDelete = True

                        If UCase(txtItmClass.Text.Trim) = "F" Then
                            txtQty.ReadOnly = True
                            GrpExpBarcode.Visible = False
                        ElseIf UCase(txtItmClass.Text.Trim) = "E" Then
                            txtQty.ReadOnly = True
                            GrpExpBarcode.Visible = True
                            GrpBarcodeButtons.Visible = True
                            BtnGenerateBarcodeTrading.Enabled = True
                            BtnOKExpiryBarcode.Enabled = True

                            LstVisbility(True, True, txtItmClass.Text.Trim)

                        Else
                            txtQty.ReadOnly = False
                            GrpExpBarcode.Visible = False
                            ' LstVisbility(False, True, txtItmClass.Text.Trim)
                        End If
                    End If
                    Exit Try
                End If
            End If

            Dim k As Integer
            If OneItem = False Then
                strsql &= " and STK_CurrentArchive='c' and stk_active='y'"
                ReadFromDBToDataSet(strsql, "tbcode", dsSql1)
                If DelOrder = "Receiving Order Fashion" Or DelOrder = "Consignment Receiving Order Fashion" Then
                    k = 820
                    ReDim Preserve Sizefield(24)
                    ReDim Preserve Alnfield(24)
                    ReDim Preserve Retfield(24)
                Else
                    k = 730
                    ReDim Preserve Sizefield(16)
                    ReDim Preserve Alnfield(16)
                    ReDim Preserve Retfield(16)
                End If
                If DelOrder = "Receiving Order" Or DelOrder = "Consignment Receiving Order" Then
                    Sizefield(0) = 0 : Alnfield(0) = "C" : Retfield(0) = "RecID"
                    Sizefield(1) = 80 : Alnfield(1) = "N" : Retfield(1) = "Supplier Ref"

                    Sizefield(2) = 80 : Alnfield(2) = "N" : Retfield(2) = "Reference 1"
                    Sizefield(3) = 220 : Alnfield(3) = "N" : Retfield(3) = "Item Ref."


                    Sizefield(4) = 300
                    Alnfield(4) = "N"
                    Retfield(4) = "Description"

                    Sizefield(5) = 0 : Alnfield(5) = "N" : Retfield(5) = "STK_Active"
                    Sizefield(6) = 0 : Alnfield(6) = "N" : Retfield(6) = "stk_currentarchive"
                    Sizefield(7) = 0 : Alnfield(7) = "N" : Retfield(7) = "STK_HaltActTill"
                    Sizefield(8) = 0 : Alnfield(8) = "N" : Retfield(8) = "STK_BBFDate"
                    Sizefield(9) = 0 : Alnfield(9) = "N" : Retfield(9) = "STK_HaltPoPi"
                    Sizefield(10) = 0 : Alnfield(10) = "N" : Retfield(10) = "STK_ItmClass"
                    Sizefield(11) = 0 : Alnfield(11) = "N" : Retfield(11) = "stk_Subunit"
                    Sizefield(12) = 0 : Alnfield(12) = "N" : Retfield(12) = "stk_vattype"
                    Sizefield(13) = 0 : Alnfield(13) = "N" : Retfield(13) = "STQ_Color"
                    Sizefield(14) = 0 : Alnfield(14) = "N" : Retfield(14) = "Stk_unit"
                    Sizefield(15) = 0 : Alnfield(15) = "N" : Retfield(15) = "UDesc"
                    Sizefield(16) = 0 : Alnfield(16) = "N" : Retfield(16) = "SubDesc"
                ElseIf DelOrder = "Receiving Order Fashion" Or DelOrder = "Consignment Receiving Order Fashion" Then

                    Sizefield(0) = 0 : Alnfield(0) = "C" : Retfield(0) = "RecID"
                    Sizefield(1) = 80 : Alnfield(1) = "N" : Retfield(1) = "Supplier Ref"
                    Sizefield(2) = 80 : Alnfield(2) = "N" : Retfield(2) = "Sup.Color"

                    Alnfield(3) = "N" : Retfield(3) = "Reference 1"
                    Sizefield(4) = 180 : Alnfield(4) = "N" : Retfield(4) = "Item Ref."
                    Sizefield(5) = 50 : Alnfield(5) = "N" : Retfield(5) = "Color"

                    Sizefield(6) = 200
                    Alnfield(6) = "N"
                    Retfield(6) = "Description"

                    Sizefield(7) = 40
                    Alnfield(7) = "N"
                    Retfield(7) = "Gender"

                    Sizefield(8) = 55
                    Alnfield(8) = "N"
                    Retfield(8) = "Prd.Grp"

                    Sizefield(9) = 40
                    Alnfield(9) = "N"
                    Retfield(9) = "Style"

                    Sizefield(10) = 40
                    Alnfield(10) = "N"
                    Retfield(10) = "Grid"

                    Sizefield(11) = 0 : Alnfield(11) = "N" : Retfield(11) = "STQ_Color"

                    Sizefield(12) = 0 : Alnfield(12) = "N" : Retfield(12) = "STK_Active"
                    Sizefield(13) = 0 : Alnfield(13) = "N" : Retfield(13) = "stk_currentarchive"
                    Sizefield(14) = 0 : Alnfield(14) = "N" : Retfield(14) = "STK_HaltActTill"
                    Sizefield(15) = 0 : Alnfield(15) = "N" : Retfield(15) = "STK_BBFDate"
                    Sizefield(16) = 0 : Alnfield(16) = "N" : Retfield(16) = "STK_HaltPoPi"
                    Sizefield(17) = 0 : Alnfield(17) = "N" : Retfield(17) = "STK_ItmClass"
                    Sizefield(18) = 0 : Alnfield(18) = "N" : Retfield(18) = "stk_Subunit"
                    Sizefield(19) = 0 : Alnfield(19) = "N" : Retfield(19) = "stk_vattype"
                    Sizefield(20) = 0 : Alnfield(20) = "N" : Retfield(20) = "STQ_Color1"
                    Sizefield(21) = 0 : Alnfield(21) = "N" : Retfield(21) = "Stk_unit"
                    Sizefield(22) = 0 : Alnfield(22) = "N" : Retfield(22) = "UDESC"
                    Sizefield(23) = 0 : Alnfield(23) = "N" : Retfield(23) = "Subdesc"
                End If
                frm = New frmSearch
                '''''''''   lcCurrentCode = txtCode.Text
                With frm
                    .Owner = Me
                    If .ShowDialog("Return", dsSql1.Tables("tbcode"), dsSql1.Tables("tbcode").Rows.Count, _
                        "List Of Duplicated References (Supplier Reference, Single Reference 1, Template Reference, Barcode and ISBN)", Sizefield, Alnfield, Retfield, 0, Nothing, False, CShort(k), 400, False, True) = DialogResult.OK Then

                        With lstDelivery
                            If lstDelivery(.Row, "MOD_supitmref").ToString.Trim <> gbRetSearch(1) Then ModTrs = True

                            If DelOrder = "Receiving Order Fashion" Or DelOrder = "Consignment Receiving Order Fashion" Then

                                GetItem(gbRetSearch(4), gbRetSearch(0), gbRetSearch(1), gbRetSearch(2), gbRetSearch(20))
                            Else

                                GetItem(gbRetSearch(3), gbRetSearch(0), gbRetSearch(1))
                            End If
                            lcRef = False
                            txtSupRef.Focus()

                        End With
                    Else
                        'INITCOL(True)
                        txtSTkID.Text = ""
                        Exists = False
                    End If
                End With
            End If
            lcChanges = False
            lcRef = False
        Catch ex As Exception
            MessageBox.Show("Error : " & ex.Message, cntTitle, _
            MessageBoxButtons.OK, MessageBoxIcon.Error)
        Finally
            daa.Dispose()
            daa = Nothing
        End Try
    End Sub
    Private Sub CreateTempNonAddedItemsStk(ByVal TempTable As String)
        Dim Str As String
        'Create the table TempNonAddedItems to insert the invalid items (not inserted to the grid) & the reason
        Try
            DropSqlObject(TempTable, "TABLE")
            Str = "SELECT STK_Recid AS ItemRecid, STK_Ref AS ItemRef, SRef_SupColor AS SupCol,"
            Str &= " STK_Desc AS ItemDesc, SRef_ItmColor AS Color, CONVERT(VARCHAR(1000),'') AS Reason"
            Str &= " INTO " & TempTable & " FROM STK04_STKFIL"
            Str &= " INNER JOIN STK04_StkSupRef ON STK_RECID = SRef_StkRecId"
            Str &= " WHERE 1 = 2"
            Execute(Str)
        Catch ex As Exception
            Throw ex
        End Try
    End Sub
    Public Sub INITCOL(ByVal RMInit As Boolean, Optional ByVal delete As Boolean = False)
        Try
            If delete = True Then
                If DelOrder = "Receiving Order" Or DelOrder = "Consignment Receiving Order" Then
                    strsql = " UPDATE " & TmpGridBarcode & " set bar_Stat = 'D' WHERE  RECID='" & Val(txtRecid.Text.Trim) & "'"
                    strsql &= " ; UPDATE " & TmpGridExpiry & " set exp_stat='D' WHERE  RECID='" & Val(txtRecid.Text.Trim) & "'"
                    Execute(strsql)
                    LstVisbility(False, False, lstDelivery.Columns("ItmClass").Text.Trim)
                Else
                    strsql = " UPDATE " & TmpGridBarcode & "F set tmp_Stat = 'D' WHERE  tmp_Drecid='" & Val(txtRecid.Text.Trim) & "';"
                    strsql &= " Delete from " & TmpFashQty & " where Recid='" & Val(txtRecid.Text.Trim) & "'"
                    Execute(strsql)
                    dsDetFqty.Tables(0).Clear()
                    daDetFqty.Fill(dsDetFqty, dsDetFqty.Tables(0).TableName)
                    PnlFashQty.Visible = False
                End If

                txtSTkID.Text = ""
            End If
            DropSqlObject(TempNonAddedItemsStk, "TABLE")
            txtRecid.Text = ""
            txtSupRef.Text = ""
            txtSupColor.Text = ""
            txtRef.Text = ""
            txtGender.Text = ""
            txtStatus.Text = ""
            txtProdGroup.Text = ""
            txtStyle.Text = ""
            txtDesc.Text = ""
            txtColor.Text = ""
            txtSTkID.Text = ""
            txtQty.Text = ""
            txtSUBUnit.Text = ""
            TXTUNIT.Text = ""
            txtSUBUnit.Tag = ""
            TXTUNIT.Tag = ""
            txtConsQty.Text = ""
            txtVat.Text = ""
            txtPrice.Text = ""
            TxtNet.Text = ""
            TxtDisc.Text = ""
            ' TxtItemCurr.Text = ""
            '   LblCostCurr.Text = ""
            txtOldNew.Text = ""
            btnBarcode.Enabled = False
            'If DelOrder = "Consignment Receiving Order" Or DelOrder = "Consignment Receiving Order Fashion" Then
            '    btnStat.Visible = False
            'End If
            If Wtype = "ADD" And txtDate.Value Is DBNull.Value And txtActualDate.Value Is DBNull.Value Then
                txtDate.Value = CDate(Now.Date)
                txtActualDate.Value = CDate(Now.Date)
            End If
            GrpExpBarcode.Visible = False
            PnlFashQty.Visible = False
            lcRef = True
        Catch ex As Exception
            Throw ex
        End Try
    End Sub

    Private Function RowStkInfo(ByVal recid As String, Optional ByVal color As String = "", Optional ByVal SuppRef As String = "", Optional ByVal SuppColor As String = "") As Double
        Dim drsql As SqlDataReader
        Try
            With lstDelivery
                strsql = "select Distinct stk_recid,stk_ref,stk_desc,STK_SingleRef1, stk_unit, stk_Subunit ,measu.meas_desc Udesc,measub.meas_desc Subdesc, stk_currentarchive,stk_active,STK_HaltActTill,SRef_SupRef,STK_Ref,STK_HaltPoPi, STK_ItmClass,"
                strsql &= " VDET.meas_sign ,VDET. Meas_value,stk_vattype,STK_BBFDate, STK_ItmClass "
                If DelOrder = "Receiving Order Fashion" Or DelOrder = "Consignment Receiving Order Fashion" Then
                    strsql &= " , STK_Grid,(SELECT ST_CODE FROM stk04_STYLE WHERE ST_CODE = STK_Style) AS Style "
                    strsql &= " , stk_gender as Gender,STK_ITMTYPE AS ItemType,SRef_SupColor,STQ_Color "
                End If
                strsql &= " , '" & txtCurr.Text.Trim & "'[STK_FCCODE], "
                If txtCurr.Text.Trim = gbCurr1 Then
                    strsql &= " STK_AVGCOSTLC UnitCost   "
                ElseIf txtCurr.Text.Trim = gbCurr2 Then
                    strsql &= " STK_AVGCOSTCC UnitCost     "
                ElseIf txtCurr.Text.Trim <> "" Then
                    strsql &= " dbo.[convert5]('" & gbApp & "','" & gbComp & "','" & gbYear & "',"
                    strsql &= " STK_AVGCOSTFC,STK_FCCODE,'" & txtCurr.Text.Trim & "','" & CType(Now, Date).ToString("MM/dd/yyyy") & "') UnitCost  "
                Else
                    strsql &= " STK_AVGCOSTCC UnitCost     "
                End If
                If DelOrder = "Consignment Receiving Order" Or DelOrder = "Consignment Receiving Order Fashion" Then
                    If txtCurr.Text.Trim = gbCurr1 Then
                        strsql &= "  ,  0  disc, STK_AVGCOSTLC net "
                    ElseIf txtCurr.Text.Trim = gbCurr2 Then
                        strsql &= "   , 0  disc ,STK_AVGCOSTCC  net  "
                    Else
                        strsql &= "  ,  0 disc ,  dbo.[convert5]('" & gbApp & "','" & gbComp & "','" & gbYear & "',"
                        strsql &= " STK_AVGCOSTFC,STK_FCCODE,'" & txtCurr.Text.Trim & "','" & CType(Now, Date).ToString("MM/dd/yyyy") & "')  net "
                    End If
                End If
                strsql &= " from V_PORD2_STKDetails VDET "
                strsql &= " LEFT JOIN Stk04_stkprices  ON SP_StkRecId = stk_recid AND isnull(SP_StartingQty,0)  = 0"
                strsql &= " LEFT JOIN stk04_stkqty ON STQ_STKRECID = stk_recid " 'AND STQ_LOCCODE='" & txtLoc.Text.Trim.Replace("'", "''") & "' "
                strsql &= " LEFT JOIN Stk04_StkSupRef ON STK_RECID = SRef_StkRecId AND COALESCE(SRef_ItmColor,'') = COALESCE(STQ_Color,'') "
                strsql &= " inner join stk04_measurements measu on measu.meas_unit = stk_unit "
                strsql &= " inner join stk04_measurements measub on measub.meas_unit = stk_subunit "
                strsql &= AppStkCond("STK_Recid", , True, False, BUAccess.Purchase) & " where 1 = 1 "

                strsql &= " AND stk_usage <> 'p'"
                '  strsql &= " AND stk_Category <> 'tmp'"
                strsql &= " AND isnull(BO_Type,'') <> 'R' "
                If DelOrder = "Receiving Order" Then
                    strsql &= " And (STK_ItmClass='B' or STK_ItmClass='E' or STK_ItmClass='T') and STK_NEWUSED<>'c'"
                ElseIf DelOrder = "Receiving Order Fashion" Then
                    strsql &= " And  STK_ItmClass='F'and STK_NEWUSED<>'c' and  STK_Brand='" & txtBrand.Text.Trim & "' "
                    strsql &= " and STK_Season='" & txtSeason.Text.Trim & "'"
                ElseIf DelOrder = "Consignment Receiving Order" Then
                    strsql &= " And (STK_ItmClass='B' or STK_ItmClass='E' or STK_ItmClass='T') and STK_NEWUSED='c' AND stk_supplier='" & txtSupp.Text.Trim & "'"
                ElseIf DelOrder = "Consignment Receiving Order Fashion" Then
                    strsql &= " And  STK_ItmClass='F'and STK_NEWUSED='c' AND stk_supplier='" & txtSupp.Text.Trim & "'"
                    strsql &= " and  STK_Brand='" & txtBrand.Text.Trim & "' and STK_Season='" & txtSeason.Text.Trim & "'"
                End If
                If recid <> "" Then
                    strsql &= " AND STK_RECID=" & recid & " "
                End If
                If color <> "" Then
                    strsql &= " AND STQ_Color='" & color & "' "
                End If
                If SuppRef <> "" Then
                    strsql &= " AND SRef_SupRef='" & SuppRef & "' "
                End If
                If SuppColor <> "" Then
                    strsql &= " AND SRef_SupColor='" & SuppColor & "' "
                End If

                drsql = Nothing
                ReadFromDBToDataReader(strsql, drsql)

                If Not drsql.HasRows Then
                    MessageBox.Show("Item Does Not Exist In Stock.", cntTitle, MessageBoxButtons.OK, MessageBoxIcon.Exclamation)
                    SuppColor = ""
                    color = ""
                    SuppRef = ""
                    recid = ""
                    Exists = False
                    RowStkInfo = 1
                    Exit Function

                Else

                    With drsql
                        .Read()

                        If .Item("stk_currentarchive").ToString = "A" Then
                            MessageBox.Show("Archived Item!", cntTitle, MessageBoxButtons.OK, MessageBoxIcon.Exclamation)
                            RowStkInfo = 2 : Exists = False : INITCOL(True) : lstDelivery.Col = FirstColKey : Exit Function
                        ElseIf UCase(.Item("stk_active").ToString) = "N" Then
                            MessageBox.Show("Inactive Item!", cntTitle, MessageBoxButtons.OK, MessageBoxIcon.Exclamation)
                            RowStkInfo = 2 : Exists = False : INITCOL(True) : lstDelivery.Col = FirstColKey : Exit Function
                        ElseIf .Item("STK_HaltActTill").ToString <> "" Then
                            If CType(.Item("STK_HaltActTill"), Date).ToString("MM/dd/yyyy") > CType(Now, Date).ToString("MM/dd/yyyy") And UCase(drsql.Item("STK_HaltPoPi").ToString) = "Y" Then
                                MessageBox.Show("Item’s Activity Is Halted!", cntTitle, MessageBoxButtons.OK, MessageBoxIcon.Exclamation)
                                RowStkInfo = 2 : Exists = False : INITCOL(True) : lstDelivery.Col = FirstColKey : Exit Function
                            End If
                        End If


                        '...test if BBFDate < Receiving Order's Date
                        If Not .Item("STK_BBFDate") Is DBNull.Value Then
                            If CDate(.Item("STK_BBFDate")) > CDate(txtDate.Value) Then
                                InsertNonAddedItemsStk(TempNonAddedItemsStk, .Item("STK_recid").ToString, "This Item Has A BBF Date > than Receiving Order's Date." + Chr(13) + "BBF Date :" & CDate(.Item("STK_BBFDate")).ToString("dd/MM/yyyy"), SuppColor, color)
                                '  MessageBox.Show("This Item Has A BBF Date > than Receiving Order's Date." + Chr(13) + "BBF Date :" & CDate(.Item("STK_BBFDate")).ToString("dd/MM/yyyy").ToString, cntTitle, MessageBoxButtons.OK, MessageBoxIcon.Exclamation)
                                '  RowStkInfo = 2 : INITCOL(True) : lstDelivery.Col = FirstColKey
                                ' Exists = False
                                ' Exit Function
                            End If
                        End If
                        '...Verify If Is Duplicate supplier code +Item Reference
                        'If PORD2_GETPROD(lstDelivery.Columns("MOD_supitmref").Text.Trim.Replace("'", "''"), txtSupp.Text, "", .Item("stk_recid").ToString) = True Then
                        '    INITCOL(True) : RowStkInfo = 1
                        '    InitGrids()
                        '    Exit Function
                        'End If

                        '...Refresh item Infos 


                        If SuppRef <> "" Then
                            row("MOD_supitmref") = SuppRef
                        Else
                            row("MOD_supitmref") = .Item("STK_SingleRef1").ToString '("Reference 1").ToString
                        End If

                        'row("MOD_supitmref") = .Item("SRef_SupRef").ToString
                        row("Stk_ref") = .Item("stk_ref").ToString
                        row("MOD_stat") = "N"
                        row("MOD_RMRecid") = .Item("stk_recid").ToString
                        row("stk_desc") = .Item("stk_desc").ToString
                        'row("stk_Subunit") = .Item("stk_Subunit").ToString
                        '  row("stk_unit") = .Item("stk_unit").ToString

                        row("stk_Subunit") = .Item("subdesc").ToString
                        row("stk_unit") = .Item("udesc").ToString
                        row("ItmClass") = .Item("STK_ItmClass").ToString
                        row("new_old") = "S"
                        If DelOrder = "Receiving Order Fashion" Or DelOrder = "Consignment Receiving Order Fashion" Then
                            row("STK_Gender") = .Item("Gender").ToString
                            row("ItemType") = .Item("ItemType").ToString
                            row("Style") = .Item("Style").ToString

                            row("SuppColor") = .Item("SRef_SupColor").ToString
                            If color <> "" Then row("Color") = .Item("STQ_Color").ToString
                            'row("Color").FooterText = GetFieldDesc("Stk04_COLOR", " Color_Code='" & lstDelivery.Columns("Color").Text.Trim & "'", "Color_Reference")
                        End If
                        row("STK_FCCODE") = .Item("STK_FCCODE").ToString
                        row("COST") = .Item("UnitCost").ToString
                        If DelOrder = "Consignment Receiving Order" Or DelOrder = "Consignment Receiving Order Fashion" Then
                            row("Net") = .Item("net").ToString
                            row("disc") = .Item("disc").ToString
                        End If
                        row("MOD_Count") = CStr(GetDetSer(lstDelivery.Columns("MOD_supitmref").Text, lstDelivery.Columns("MOD_stat").Text, CInt(row("RECID"))))

                        row("invd_VAT") = .Item("stk_vattype").ToString

                        'If .Item("stk_vattype").ToString = "T" Then row("invd_VAT") = "Taxable"
                        'If .Item("stk_vattype").ToString = "E" Then row("invd_VAT") = "Taxable Embeded"
                        'If .Item("stk_vattype").ToString = "N" Then row("invd_VAT") = "Not Taxable"
                        lstDelivery.UpdateData()
                        RowStkInfo = 0

                        SuppColor = ""
                        color = ""
                        SuppRef = ""
                        recid = ""
                    End With
                End If

            End With

        Catch ex As Exception
            MessageBox.Show("Error : " & ex.Message, cntTitle, MessageBoxButtons.OK, MessageBoxIcon.Error)
        End Try
    End Function

    Private Sub lstDelivery_FetchRowStyle(ByVal sender As Object, ByVal e As C1.Win.C1TrueDBGrid.FetchRowStyleEventArgs) Handles lstDelivery.FetchRowStyle
        Try
            If UCase(lstDelivery.Columns("new_old").CellValue(e.Row).ToString) = "S" Then
                e.CellStyle.BackColor = System.Drawing.Color.MistyRose
            ElseIf UCase(lstDelivery.Columns("new_old").CellValue(e.Row).ToString) = "W" Then
                e.CellStyle.BackColor = System.Drawing.Color.Lavender

            Else
                e.CellStyle.BackColor = System.Drawing.Color.Transparent
            End If


        Catch ex As Exception
            MessageBox.Show("Error : " & ex.Message, cntTitle, _
            MessageBoxButtons.OK, MessageBoxIcon.Error)
        End Try
    End Sub
    Private Sub lstDelivery_OnAddNew(ByVal sender As Object, ByVal e As System.EventArgs) Handles lstDelivery.OnAddNew
        Dim MaxRecID As Long
        Try
            'If lstDelivery.Columns(lstDelivery.Col).FilterText <> "" Then Exit Sub


            Dim dt As DataTable
            dt = dsDetDetails.Tables("tblDetails")

            Dim dv As New DataView(dt)
            LstVisbility(False, False, lstDelivery.Columns("ItmClass").Text.Trim)

            dv.RowFilter = "RecId =Max(RecId)"

            If dv.Count = 0 Then
                MaxRecID = 0
            Else
                MaxRecID = CLng(dv(0)(0).ToString)
            End If

            With lstDelivery.Splits(0).DisplayColumns("new_old").DataColumn
                .Text = "N"
                .RefreshCell()
                lstDelivery.AllowDelete = True
            End With
            With lstDelivery.Splits(0).DisplayColumns("RecId").DataColumn
                .Value = MaxRecID + 1
                .RefreshCell()
            End With
            'lstDelivery.UpdateData()
            ReadonlyConsFash()
            If DelOrder = "Receiving Order Fashion" Or DelOrder = "Consignment Receiving Order Fashion" Then
                lstDelivery.Columns("Color").FooterText = ""
                txtBrand.ReadOnly = True : btnBrand.Enabled = False
                txtSeason.ReadOnly = True : btnSeason.Enabled = False
                With lstDelivery.Splits(0).DisplayColumns("STK_SEASON").DataColumn
                    .Value = txtSeason.Text
                    .RefreshCell()
                End With
                With lstDelivery.Splits(0).DisplayColumns("STK_BRAND").DataColumn
                    .Value = txtBrand.Text
                    .RefreshCell()
                End With
            End If
            'AppStkCond()


            Dim drsql As SqlDataReader
            Dim NewUsed As String
            If UCase(gbUser) = "SA" Or UCase(gbUser) = "SU" Then
                With lstDelivery.Splits(0).DisplayColumns("STK_NewUsed").DataColumn
                    If DelOrder = "Consignment Receiving Order" Or DelOrder = "Consignment Receiving Order Fashion" Then
                        .Value = "C"
                        lstDelivery.Splits(0).DisplayColumns("STK_NewUsed").Locked = True
                    Else
                        .Value = "N"
                    End If
                    .RefreshCell()
                End With
            Else
                strsql = "SELECT ASP_STKTag FROM STK04_APPSTKPRIV WHERE ASP_USER='" & gbUser & "' AND ASP_APPcODE ='" & gbApp & "'"
                If DelOrder = "Consignment Receiving Order" Or DelOrder = "Consignment Receiving Order Fashion" Then
                    strsql &= " AND ASP_STKTag = 'C'"
                Else
                    strsql &= " AND ASP_STKTag <> 'C'"
                End If
                ReadFromDBToDataReader(strsql, drsql)
                If drsql.HasRows Then
                    drsql.Read()

                    If UCase(drsql.Item("ASP_STKTag").ToString) = "N" Then
                        With lstDelivery.Splits(0).DisplayColumns("STK_NewUsed").DataColumn
                            .Value = "N"
                            .RefreshCell()
                        End With
                    End If
                    If DelOrder = "Consignment Receiving Order" Or DelOrder = "Consignment Receiving Order Fashion" Then
                        With lstDelivery.Splits(0).DisplayColumns("STK_NewUsed").DataColumn
                            .Value = "C"
                            lstDelivery.Splits(0).DisplayColumns("STK_NewUsed").Locked = True
                            .RefreshCell()
                        End With
                    End If
                End If
            End If

        Catch ex As Exception
            MessageBox.Show("Error: " & ex.Message, cntTitle, _
            MessageBoxButtons.OK, MessageBoxIcon.Error)
        End Try
    End Sub

    Private Function GetDetSer(ByVal SupRef As String, ByVal stat As String, ByVal Cnt As Integer) As Integer
        Dim drsql As SqlDataReader
        Dim str As String
        Try
            'str = "SELECT ISNULL(MAX(MOD_Count),0)+1 as [Ctr] from " & TmpGridDetails
            'str &= " WHERE      MOD_Supitmref ='" & SupRef.Trim.Replace("'", "''") & "' and MOD_Stat='" & stat & "'"

            str = "SELECT ISNULL(MAX(MOD_Count),0)+1 as [Ctr] from " & TmpGridDetails
            str &= " WHERE MOD_Stat='" & Replace(stat, "'", "''") & "'"
            str &= " AND MOD_Supitmref ='" & Replace(SupRef.Trim, "'", "''") & "'"
            str &= " AND RecId <> " & Cnt
            ReadFromDBToDataReader(str, drsql)
            If drsql.HasRows Then
                drsql.Read()
                GetDetSer = CInt(drsql.Item("Ctr"))
            End If

            drsql = Nothing
        Catch ex As Exception
            Throw ex
        Finally
            If Not drsql Is Nothing Then
                If drsql.IsClosed = False Then drsql.Close()
                drsql = Nothing
            End If
        End Try
    End Function

    Private Sub ClearGridFiltration(ByVal tdbGrid As C1TrueDBGrid)
        Dim i As Int32
        Try
            With tdbGrid
                For i = 0 To .Columns.Count - 1
                    With .Columns(i)
                        .FilterText = ""
                        .RefreshCell()
                    End With
                Next
            End With
        Catch ex As Exception
            Throw ex
        End Try
    End Sub

    Private Sub txtRef_TextChanged(ByVal sender As Object, ByVal e As System.EventArgs) Handles txtRef.TextChanged
        Try
            'INITCOL(False)
            'txtSTkID.Text = ""
            lcChanges = True
            lcRef = True
            GrpExpBarcode.Visible = False
            LstVisbility(False, False, txtItmClass.Text.Trim)
            ErrorProvider1.SetError(CType(sender, TextBox), "")

            If wModify Then
                INITCOL(True, False)
                wModify = False
            End If
        Catch ex As Exception
            MessageBox.Show("Error : " & ex.Message, cntTitle, _
            MessageBoxButtons.OK, MessageBoxIcon.Error)
        End Try
    End Sub

    Private Sub txtSupRef_Validating(ByVal sender As Object, ByVal e As System.ComponentModel.CancelEventArgs) Handles txtSupRef.Validating, txtSupColor.Validating, txtRef.Validating
        Try
            If txtSupp.Text.Trim = "" Then
                Exit Sub
            ElseIf txtDate.Value Is DBNull.Value Then
                Exit Sub
            ElseIf txtDelOrder.Text.Trim = "" Then
                Exit Sub
            ElseIf txtLoc.Text.Trim = "" Then
                Exit Sub
            End If
            If DelOrder = "Receiving Order Fashion" Or DelOrder = "Consignment Receiving Order Fashion" Then
                If txtBrand.Text.Trim = "" Then
                    Exit Sub
                ElseIf txtSeason.Text.Trim = "" Then
                    Exit Sub
                End If
            End If
            If sender.ToString <> "" Then
                If sender Is txtSupRef And txtSupRef.Text.Trim <> "" And lcRef = True Then
                    GetItem(txtSupRef.Text.Trim.Replace("'", "''"))
                End If

                If sender Is txtSupColor And txtSupColor.Text.Trim <> "" And lcRef = True Then
                    If txtSupRef.Text.Trim <> "" Then GetItem(txtSupRef.Text.Trim, , , , , txtSupColor.Text.Trim.Replace("'", "''"))

                End If
                If sender Is txtRef And txtRef.Text.Trim <> "" And lcRef = True Then
                    GetItem(txtRef.Text.Trim.Replace("'", "''"))
                    If Exists = False Then
                        INITCOL(True, True)
                        txtRef.Focus()
                    End If
                ElseIf txtRef.Text.Trim = "" And sender Is txtRef Then 'And txtSTkID.Text.Trim = ""
                    INITCOL(True, True)
                    txtSupRef.Focus()
                End If

            End If

        Catch ex As Exception
            MessageBox.Show("Error : " & ex.Message, cntTitle, MessageBoxButtons.OK, MessageBoxIcon.Error)
        End Try
    End Sub

    Private Sub txtQty_Validating(ByVal sender As Object, ByVal e As System.ComponentModel.CancelEventArgs) Handles txtQty.Validating
        Dim drsql As SqlDataReader
        Dim ItmQty As Object
        Try

            If txtSTkID.Text.Trim = "" Then Exit Sub
            If txtQty.Text.Trim = "" Then Exit Sub

            If txtItmClass.Text.Trim.ToUpper = "T" Then
                ItmQty = dsDetDetails.Tables("tblDetails").Compute(" sum(MOD_OrdQty) ", _
                                " DOB_Drecid is null and MOD_stat = 'R' and MOD_RMRecid = " & txtSTkID.Text.Trim & _
                                IIf(wModify = True, " AND RECID <> " & txtRecid.Text.Trim & "", "").ToString)
                If ItmQty Is DBNull.Value Then
                    ItmQty = 0
                Else
                    ItmQty = CDbl(ItmQty)
                End If
                ItmQty = (CDbl(ItmQty) + CDbl(txtQty.Text.Trim))

                If QtyCntrEc(txtSTkID.Text.Trim, ItmQty.ToString) Then
                    '  ShowNotification(Me, "  Qty Cntr  ", 2500, 14, DevComponents.DotNetBar.eToastPosition.MiddleCenter)
                    e.Cancel = True
                    Exit Try
                End If

            End If
            'update consolidated QTy
            If Val(txtQty.Text.Trim) > 0 Then btnBarcode.Enabled = True Else btnBarcode.Enabled = False

            If UCase(GetFieldDesc(TmpGridDetails, "MOD_RMRecid=" & txtSTkID.Text.Trim & "", "new_old")) = "W" Then

                Dim Str As String
                Str = " Select case Meas_Sign WHEN '*' THEN case when pn2.sfp_operator ='*' then  " & Val(txtQty.Text.Trim) & " *  pn2.SFP_UNITPACK   * STK04_Measurements.Meas_Value else "
                Str &= "  " & Val(txtQty.Text.Trim) & " /  pn2.SFP_UNITPACK   * STK04_Measurements.Meas_Value end "
                Str &= " ELSE  case when pn2.sfp_operator ='*' then (  SFP_UNITPACK   * " & Val(txtQty.Text.Trim) & " ) /  STK04_Measurements.Meas_Value else (" & Val(txtQty.Text.Trim) & " / SFP_UNITPACK) /  STK04_Measurements.Meas_Value end End [ctr]"
                Str &= " From " & TempPendingDO & "  pn2 "
                Str &= " left JOIN STK04_Measurements ON Meas_unit = pn2.SFP_SUBUNIT "
                Str &= " WHERE  pn2.SFP_Recid = " & Val(txtSTkID.Text.Trim) & " "

                ReadFromDBToDataReader(Str, drsql)
                If drsql.HasRows Then
                    drsql.Read()
                    txtConsQty.Text = Format(drsql.Item("Ctr"), "###0.000").ToString()
                    drsql = Nothing
                End If
            Else
                strsql = " SELECT  CASE meas_sign "
                strsql &= " WHEN '*' THEN case when stk_operator='*' then " & Val(txtQty.Text.Trim) & " * stk_unitpack * meas_value else "
                strsql &= " (" & Val(txtQty.Text.Trim) & " / stk_unitpack) * meas_value end "
                strsql &= " ELSE  case when stk_operator ='*' then ( stk_unitpack * " & Val(txtQty.Text.Trim) & " ) / Meas_value else "
                strsql &= " (" & Val(txtQty.Text.Trim) & "  / stk_unitpack ) / Meas_value  end  End as [Ctr] FROM V_Pord2_GetStkMeas WHERE   stk_recid = '" & txtSTkID.Text.Trim & "' "

                ReadFromDBToDataReader(strsql, drsql)
                If drsql.HasRows Then
                    drsql.Read()
                    txtConsQty.Text = Format(drsql.Item("Ctr"), "###0.000").ToString()
                    drsql = Nothing
                End If
            End If

            'test if qty is biggegr than barcode number 
            If DelOrder = "Receiving Order" Or DelOrder = "Consignment Receiving Order" Then

                LstBarcode.UpdateData()
                daDetBarcode.Update(dsDetBarcode.Tables("tblDetails"))

                strsql = "  SELECT  recid , COUNT(*) Counter FROM  " & TmpGridBarcode & " "
                strsql &= " where Dob_barcode<>'' AND NOT dob_barcode is NULL and recid  =" & txtRecid.Text.Trim & " and bar_stat<>'D' GROUP BY recid  "

                ReadFromDBToDataReader(strsql, drsql)
                If drsql.HasRows Then
                    While drsql.Read
                        If Val(drsql.Item("Counter").ToString) > Val(txtQty.Text.Trim) Then
                            MessageBox.Show("Barcode Number Is Bigger than Quantity Per Item", cntTitle, MessageBoxButtons.OK, MessageBoxIcon.Exclamation)
                            e.Cancel = True
                            txtQty.Focus()
                            Exit Sub
                        End If

                    End While
                    drsql = Nothing
                End If
            End If
            NetCalculate()
        Catch ex As Exception
            MessageBox.Show("Error : " & ex.Message, cntTitle, MessageBoxButtons.OK, MessageBoxIcon.Error)
        End Try
    End Sub

    Private Sub txtSupColor_TextChanged(ByVal sender As System.Object, ByVal e As System.EventArgs) Handles txtSupColor.TextChanged, txtSupRef.TextChanged
        Try

            lcChanges = True
            ErrorProvider1.SetError(CType(sender, TextBox), "")
        Catch ex As Exception
            MessageBox.Show("Error : " & ex.Message, cntTitle, _
            MessageBoxButtons.OK, MessageBoxIcon.Error)
        End Try
    End Sub

    Private Sub txtCode_TextChanged(ByVal sender As System.Object, ByVal e As System.EventArgs) Handles txtCode.TextChanged
        Try
            lcChangeKey = True
            lcChanges = True
            ErrorProvider1.SetError(CType(sender, TextBox), "")
        Catch ex As Exception
            MessageBox.Show("Error : " & ex.Message, cntTitle, _
            MessageBoxButtons.OK, MessageBoxIcon.Error)
        End Try
    End Sub

    Private Sub txtDate_TextChanged(ByVal sender As System.Object, ByVal e As System.EventArgs) Handles txtDate.TextChanged, txtActualDate.TextChanged
        Try
            lcChanges = True
            If sender Is txtDate Then lcChangeKey = True
            ErrorProvider1.SetError(CType(sender, C1DateEdit), "")
        Catch ex As Exception
            MessageBox.Show("Error : " & ex.Message, cntTitle, _
            MessageBoxButtons.OK, MessageBoxIcon.Error)
        End Try
    End Sub

    Private Sub LstBarcode_AfterColUpdate(ByVal sender As Object, ByVal e As C1.Win.C1TrueDBGrid.ColEventArgs) Handles LstBarcode.AfterColUpdate
        Dim drs() As DataRow
        Try
            If Wtype = "" Or Wtype = "DELETE" Then Exit Sub
            ErrorProvider1.SetError(LstBarcode, "")
            If LstBarcode.Columns("DOB_Barcode").Text.Trim = "" Then Exit Sub

            '  If LstBarcode.Columns("stk_desc").Text.Trim = "" Then RefQtyLbl(True) Else RefQtyLbl(False)
        Catch ex As Exception
            MessageBox.Show("Error : " & ex.Message, cntTitle, _
            MessageBoxButtons.OK, MessageBoxIcon.Error)
        End Try
    End Sub

    Private Sub LstBarcode_ButtonClick(ByVal sender As Object, ByVal e As C1.Win.C1TrueDBGrid.ColEventArgs) Handles LstBarcode.ButtonClick
        Dim Col As Integer
        Dim drs As DataRow()
        Try
            If Wtype = "" Or Wtype = "DELETE" Then Exit Sub
            If UCase(txtOldNew.Text) = "W" Then Exit Sub
            If txtSTkID.Text.Trim = "" Then Exit Sub



            If txtItmClass.Text.Trim = "E" Then
                If Val(LstExpiry.Columns("MOD_OrdQty").Text.Trim) <= 0 Then
                    LstExpiry.Focus()
                    Exit Sub
                End If
            End If

            Col = LstBarcode.Col
            If LstBarcode.Splits(0).DisplayColumns(Col).Locked = True Then Exit Sub

            Cursor.Current = Cursors.WaitCursor
            Select Case UCase(LstBarcode.Columns(Col).DataField)

                Case "DOB_BARCODE" : ItemBarcode(txtSTkID.Text.Trim, CLng(Val(LstBarcode(LstBarcode.Row, "B_RecId").ToString)))

                    LstBarcode.UpdateData()
                    daDetBarcode.Update(dsDetBarcode.Tables("tblDetails"))
            End Select

        Catch ex As Exception
            MessageBox.Show("Error: " & ex.Message, cntTitle, MessageBoxButtons.OK, MessageBoxIcon.Error)
        End Try
    End Sub

    Private Sub LstBarcode_OnAddNew(ByVal sender As Object, ByVal e As System.EventArgs) Handles LstBarcode.OnAddNew
        Dim drs As DataRow()
        Dim MaxRecID As Long
        Try
            '  drs = dsDetBarcode.Tables("tblDetails").Select("recid='" & txtRecid.Text.Trim & "' AND bar_stat<>'D'")

            'If UCase(txtItmClass.Text.Trim) = "E" Then
            '    If drs.Length >= Val(LstExpiry.Columns("QTY").Text.Trim) Then
            '        MessageBox.Show("Barcode Number Can't Be Bigger Than Expiry Quantity.", cntTitle, MessageBoxButtons.OK, MessageBoxIcon.Exclamation)
            '        'e.cancel = True
            '        'LstBarcode.Focus()
            '        ' LstBarcode.AllowAddNew = False
            '        LstBarcode.Row = LstBarcode.Row - 1
            '        Exit Sub
            '    End If
            'Else
            '    If drs.Length >= Val(txtQty.Text.Trim) Then
            '        MessageBox.Show("Barcode Number Can't Be Bigger Than Quantity.", cntTitle, MessageBoxButtons.OK, MessageBoxIcon.Exclamation)
            '        If Val(txtQty.Text.Trim) = 0 Then
            '            txtQty.Focus()
            '        End If
            '        LstBarcode.Row = LstBarcode.Row - 1
            '        Exit Sub
            '    End If
            'End If

            If UCase(txtItmClass.Text.Trim) = "E" Then
                With LstBarcode.Splits(0).DisplayColumns("recid").DataColumn
                    .Value = txtRecid.Text.Trim
                    .RefreshCell()
                End With
                With LstBarcode.Splits(0).DisplayColumns("e_recid").DataColumn
                    .Value = LstExpiry.Columns("e_recid").Text
                    .RefreshCell()
                End With
            ElseIf UCase(txtItmClass.Text.Trim) = "B" Or UCase(txtItmClass.Text.Trim) = "T" Then
                With LstBarcode.Splits(0).DisplayColumns("recid").DataColumn
                    .Value = txtRecid.Text.Trim
                    .RefreshCell()
                End With
                With LstBarcode.Splits(0).DisplayColumns("e_recid").DataColumn
                    .Value = ""
                    .RefreshCell()
                End With
            End If

            With LstBarcode.Splits(0).DisplayColumns("B_RECID").DataColumn
                .Value = Val(GetMaxRecId(TmpGridBarcode, "B_RECID"))
                .RefreshCell()
            End With


        Catch ex As Exception
            MessageBox.Show("Error: " & ex.Message, cntTitle, _
            MessageBoxButtons.OK, MessageBoxIcon.Error)
        End Try
    End Sub


    Private Sub LstExpiry_AfterColUpdate(ByVal sender As Object, ByVal e As C1.Win.C1TrueDBGrid.ColEventArgs) Handles LstExpiry.AfterColUpdate
        Dim sFltr As String
        Dim sTotal As Double
        Dim objSum As Object

        Try
            ErrorProvider1.SetError(LstExpiry, "")
            With LstExpiry.Splits(0).DisplayColumns("DOQ_ProductionDt").DataColumn
                If .Text = "__.__.____" Then

                    .Text = ""
                    .Value = ""

                End If

            End With

            LstExpiry.UpdateData()
            daDetExpiry.Update(dsDetExpiry.Tables("tblDetails"))
            If (Val(LstExpiry.Columns("Dob_expirydate").Text.Trim) = 0 Or LstExpiry.Columns("Dob_batchinfo").Text.Trim = "") AndAlso Val(LstExpiry.Columns("MOD_OrdQty").Text.Trim) <> 0 Then
                MessageBox.Show("Expiry Date And Lot No are mandatory", cntTitle, MessageBoxButtons.OK, MessageBoxIcon.Exclamation)
                LstExpiry.Focus()
                Exit Sub
            End If
            objSum = dsDetExpiry.Tables("tblDetails").Compute("Sum(MOD_OrdQty)", sFltr)
            If LstExpiry.Col = LstExpiry.Splits(0).DisplayColumns.IndexOf(LstExpiry.Columns("MOD_OrdQty")) OrElse (Not objSum Is DBNull.Value AndAlso CDbl(Val(txtQty.Text.Trim)) <> CType(objSum, Double)) Then

                If objSum Is DBNull.Value Then
                    sTotal = 0 : txtQty.Text = "0"
                    btnBarcode.Enabled = False
                Else
                    sTotal = CType(objSum, Double)
                    txtQty.Text = CStr(sTotal)
                    If Val(txtQty.Text.Trim) > 0 Then btnBarcode.Enabled = True Else btnBarcode.Enabled = False
                    'If UCase(txtStatus.Text.Trim) = "R" Or UCase(txtStatus.Text.Trim) = "RETURN" Then
                    '    txtQty.Text = CStr(Val(txtQty.Text.Trim) * (-1))
                    'End If
                    Dim drsql As SqlDataReader
                    'update consolidated QTy
                    If UCase(lstDelivery.Columns("New_old").Text.Trim) = "W" Then

                        Dim Str As String
                        Str = "Select case Meas_Sign WHEN '*' THEN case when pn1.sfp_operator='*' then  " & Val(txtQty.Text.Trim) & " *  pn1.SFP_UNITPACK  * STK04_Measurements.Meas_Value else  " & Val(txtQty.Text.Trim) & " /  pn1.SFP_UNITPACK  * STK04_Measurements.Meas_Value end "
                        Str &= " ELSE case when pn1.sfp_operator='*' then  (  pn1.SFP_UNITPACK   * " & Val(txtQty.Text.Trim) & " ) /   STK04_Measurements.Meas_Value else  (       " & Val(txtQty.Text.Trim) & " / pn1.SFP_UNITPACK ) /   STK04_Measurements.Meas_Value end  End [ctr]"
                        Str &= " FROM   STK04_StkFilPending pn1"
                        Str &= " left JOIN STK04_Measurements ON Meas_unit =  pn1.SFP_SUBUNIT "
                        Str &= " WHERE  pn1.SFP_Recid = " & Val(txtSTkID.Text.Trim) & " "


                        ReadFromDBToDataReader(Str, drsql)
                        If drsql.HasRows Then
                            drsql.Read()
                            txtConsQty.Text = Format(drsql.Item("Ctr"), "#,##0.000").ToString()
                        Else
                            Str = " Select case Meas_Sign WHEN '*' THEN " & Val(txtQty.Text.Trim) & " *  pn2.SFP_UNITPACK   * STK04_Measurements.Meas_Value"
                            Str &= " ELSE (  SFP_UNITPACK   * " & Val(txtQty.Text.Trim) & " ) /   STK04_Measurements.Meas_Value End [ctr]"
                            Str &= " From " & TempPendingDO & "  pn2 "
                            Str &= " left JOIN STK04_Measurements ON Meas_unit = pn2.SFP_SUBUNIT "
                            Str &= " WHERE  pn2.SFP_Recid = " & Val(txtSTkID.Text.Trim) & " "

                            ReadFromDBToDataReader(Str, drsql)
                            If drsql.HasRows Then
                                drsql.Read()
                                txtConsQty.Text = Format(drsql.Item("Ctr"), "#,##0.000").ToString()
                            End If
                        End If
                    Else
                        strsql = " SELECT  CASE meas_sign"
                        strsql &= " WHEN '*' THEN " & Val(txtQty.Text.Trim) & " * stk_unitpack * meas_value"
                        strsql &= " ELSE ( stk_unitpack * " & Val(txtQty.Text.Trim) & " ) / Meas_value End as [Ctr] FROM V_Pord2_GetStkMeas WHERE   stk_recid = '" & txtSTkID.Text.Trim & "' "

                        ReadFromDBToDataReader(strsql, drsql)
                        If drsql.HasRows Then
                            drsql.Read()
                            txtConsQty.Text = Format(drsql.Item("Ctr"), "#,##0.000").ToString()
                        End If
                    End If
                End If
                NetCalculate()
            End If

            If LstExpiry.Columns("DOB_BatchInfo").Text.Contains(" ") Or LstExpiry.Columns("DOB_BatchInfo").Text.Contains(System.Environment.NewLine) Then
                LstExpiry.Columns("DOB_BatchInfo").Text = LstExpiry.Columns("DOB_BatchInfo").Text.Trim.Replace(" ", "").Replace(System.Environment.NewLine, "")
            End If

            Select Case UCase(LstExpiry.Columns(LstExpiry.Col).DataField)
                Case "MOD_ORDQTY"
                    RefQtyLbl(False)
            End Select
            ' If lstDelivery.Columns("stk_desc").Text.Trim = "" Then RefQtyLbl(True) Else RefQtyLbl(False)
        Catch ex As Exception
            MessageBox.Show("Error : " & ex.Message, cntTitle, _
            MessageBoxButtons.OK, MessageBoxIcon.Error)
        End Try
    End Sub

    Private Sub GetBarcode(ByVal ItemClass As String)
        Dim strsql As String

        Try

            'If lcItemClass <> "Fashion" And lcItemClass <> "Expiry" Then Exit Sub

            ''DropSqlObject(TmpGridBarcodeFinal, "TABLE") 'added by GS on 09/06/16

            strsql = "select * from " & TmpGridBarcode
            ''strsql = "select " & TmpGridBarcode & ".* into " & TmpGridBarcodeFinal & " from " & TmpGridBarcode 'strsql = "select * from " & TmpGridBarcode 'modified by GS on 09/06/16
            ''strsql &= " INNER JOIN stk04_PurDelOrdDBE ON " & TmpGridBarcode & ".DOB_RecID = stk04_PurDelOrdDBE.DOB_RecID " 'added by GS on 09/06/16

            If UCase(ItemClass) = "E" Then
                If txtRecid.Text.Trim = "" Then
                    strsql &= " where E_RECID='" & Val(LstExpiry.Columns("E_RECID").Text) & "' and RECID='" & Val(lstDelivery.Columns("Recid").Text.Trim) & "'   and bar_Stat<>'D' "
                Else
                    strsql &= " where E_RECID='" & Val(LstExpiry.Columns("E_RECID").Text) & "' and RECID='" & Val(txtRecid.Text.Trim) & "'   and bar_Stat<>'D' "
                End If
                'ch @  5/10/2016
                If Wtype <> "ADD" And lstDelivery.Columns("DOB_Drecid").Text.Trim <> "" And wModify = True Then
                    strsql &= " and DOB_Drecid = '" & Val(lstDelivery.Columns("DOB_Drecid").Text.Trim) & "'" 'added by GS on 09/06/16
                End If
            ElseIf UCase(ItemClass) = "B" Or UCase(ItemClass) = "T" Then
                If txtRecid.Text.Trim = "" Then
                    strsql &= " where RECID='" & Val(lstDelivery.Columns("Recid").Text.Trim) & "'  and bar_Stat <>'D' "
                Else
                    strsql &= " where RECID='" & Val(txtRecid.Text.Trim) & "'  and bar_Stat <>'D' "
                End If
                'ch @  5/10/2016
                If Wtype <> "ADD" And lstDelivery.Columns("DOB_Drecid").Text.Trim <> "" And wModify = True Then
                    strsql &= " and DOB_Drecid = '" & Val(lstDelivery.Columns("DOB_Drecid").Text.Trim) & "'" 'added by GS on 09/06/16
                End If
            End If


            ''Execute(strsql) 'added by GS on 09/06/16
            ''strsql = "select * from " & TmpGridBarcodeFinal 'added by GS on 09/06/16

            ClearGridFiltration(LstBarcode)

            If Not dsDetBarcode.Tables("tblDetails") Is Nothing Then dsDetBarcode.Tables("tblDetails").Clear()

            If Not daDetBarcode Is Nothing Then
                daDetBarcode.SelectCommand.CommandText = strsql
                daDetBarcode.Fill(dsDetBarcode, "tblDetails")
                LstBarcode.Refresh()
                LstBarcode.Enabled = True
            End If


        Catch ex As Exception
            Throw ex
        End Try

    End Sub
    Private Sub GetExpiry(ByVal ItmClass As String)
        Dim strsql As String

        Try

            'If lcItemClass <> "Fashion" And lcItemClass <> "Expiry" Then Exit Sub

            strsql = "select * from " & TmpGridExpiry
            If UCase(ItmClass) = "F" Then
                strsql &= " where Tmp_Color='" & LstExpiry.Columns("Color_Code").Text & "' and bar_Stat<>'D' and MOD_OrdQty<>0 "
            ElseIf UCase(ItmClass) = "E" Then
                If txtRecid.Text.Trim = "" Then
                    strsql &= " where   RECID='" & Val(lstDelivery.Columns("Recid").Text.Trim) & "' and MOD_OrdQty<>0 and DOB_BatchInfo<>'' and exp_Stat<>'D'"
                Else
                    strsql &= " where   RECID='" & Val(txtRecid.Text.Trim) & "' and MOD_OrdQty<>0 and DOB_BatchInfo<>'' and exp_Stat<>'D'"
                End If
            End If
            ClearGridFiltration(LstExpiry)

            If Not dsDetExpiry.Tables("tblDetails") Is Nothing Then dsDetExpiry.Tables("tblDetails").Clear()

            daDetExpiry.SelectCommand.CommandText = strsql
            daDetExpiry.Fill(dsDetExpiry, "tblDetails")
            LstExpiry.Enabled = True

            'If tdbDetails.Columns("Expiry").Text = "Y" Then tdbCount.AllowAddNew = True Else tdbCount.AllowAddNew = False
            'tdbCount.Col = tdbCount.Columns.IndexOf(tdbCount.Columns("Qty"))

        Catch ex As Exception
            Throw ex
        End Try

    End Sub


    Private Sub LstExpiry_RowColChange(ByVal sender As Object, ByVal e As C1.Win.C1TrueDBGrid.RowColChangeEventArgs) Handles LstExpiry.RowColChange
        Try

            If Not (e Is Nothing OrElse (e.LastRow <> -1 AndAlso e.LastRow = LstExpiry.Row)) Then
                Me.BindingContext(dsDetExpiry.Tables("tblDetails").DefaultView).EndCurrentEdit()
                If Not dsDetExpiry.GetChanges Is Nothing Then daDetExpiry.Update(dsDetExpiry, "tblDetails")
            End If

            With LstExpiry
                If .Splits(0).Rows.Count <> 0 Then
                    .SelectedRows.Clear()
                    .SelectedRows.Add(.Row)
                    If .SelectedRows.Count <> 0 Then
                        If txtItmClass.Text.Trim <> "" Then
                            GetBarcode(txtItmClass.Text.Trim)
                        Else
                            GetBarcode(lstDelivery.Columns("ItmClass").Text.Trim)
                        End If
                    Else
                        dsDetBarcode.Tables("tblDetails").Clear()
                    End If
                Else
                    .Enabled = False
                    dsDetExpiry.Tables("tblDetails").Clear()
                    ClearGridFiltration(LstExpiry)
                End If
            End With

        Catch ex As Exception
            MessageBox.Show("Error : " & ex.Message, cntTitle, _
                                    MessageBoxButtons.OK, MessageBoxIcon.Error)
        End Try
    End Sub

    Private Sub lstDelivery_Validating(ByVal sender As Object, ByVal e As System.ComponentModel.CancelEventArgs) Handles lstDelivery.Validating, LstBarcode.Validating, LstExpiry.Validating
        Dim Cnt As Integer
        Dim ColNo As Integer
        Try

            ReadonlyConsFash()

            If sender Is LstExpiry Then
                'test if qty is biggegr than barcode number 
                If LstExpiry.Columns("E_REcid").Text.Trim <> "" Then
                    Dim drsql As SqlDataReader

                    strsql = " SELECT  bar.recid , COUNT(*) Counter FROM " & TmpGridExpiry & " expiry"
                    strsql &= " LEFT JOIN " & TmpGridBarcode & " bar ON bar.E_RECID=expiry.E_RECID"
                    strsql &= " where  expiry.recid=" & LstExpiry.Columns("Recid").Text.Trim & " and expiry.e_recid=" & LstExpiry.Columns("E_REcid").Text.Trim
                    strsql &= " and DOB_Drecid = '" & Val(lstDelivery.Columns("DOB_Drecid").Text.Trim) & "'" 'added by GS on 09/06/16
                    strsql &= " and bar_stat<>'D' GROUP BY bar.recid  "

                    ReadFromDBToDataReader(strsql, drsql)
                    If drsql.HasRows Then
                        While drsql.Read
                            If Val(drsql.Item("Counter").ToString) > Val(LstExpiry.Columns("MOD_OrdQty").Text.Trim) Then
                                MessageBox.Show("Barcode Number Is Bigger than Quantity Per Item", cntTitle, MessageBoxButtons.OK, MessageBoxIcon.Exclamation)
                                e.Cancel = True
                                Exit Sub
                            End If

                        End While
                    End If
                    drsql = Nothing
                End If
            End If

            If wModify = True Then Exit Sub

            If sender Is lstDelivery Then
                If txtSTkID.Text.Trim = "" Then
                    If DelOrder = "Receiving Order" Or DelOrder = "Consignment Receiving Order" Then
                        GrpExpBarcode.Visible = False
                        LstVisbility(False, False, lstDelivery.Columns("ItmClass").Text.Trim)
                    Else
                        PnlFashQty.Visible = False
                    End If
                Else
                    Exit Sub
                End If
                '    PnlConsigment.Visible = False
            End If
            If Wtype = "" Or Wtype = "DELETE" Then Exit Sub
            If sender Is LstExpiry Then
                With LstExpiry
                    If LstExpiry.Columns("recid").Text.Trim = "" Then Exit Sub
                    If Val(.Columns("MOD_ORDQTY").Text.Trim) <> 0 And (.Columns("DOB_batchInfo").Text.Trim = "" Or .Columns("DOB_ExpiryDate").Text.Trim = "") Then
                        MessageBox.Show("Lot No Or Expiry Date Couldn't be Empty.", cntTitle, MessageBoxButtons.OK, MessageBoxIcon.Exclamation)
                        .Focus()
                        Exit Sub

                    End If
                    If Not DtQualityControlHeader Is Nothing AndAlso txtRecid.Text.Trim <> "" Then
                        If DtQualityControlHeader.Select("PrDReciD =  " & Val(LstExpiry(.Row, "RECID").ToString.Trim) & " and  PrQDRecId =  " & Val(LstExpiry(.Row, "E_RECID").ToString.Trim) & " and (isnull(AccQty,0) <> 0 OR isnull(RjQty,0) <> 0) ").Count = 0 Then
                            If Val(LstExpiry(.Row, "MOD_ORDQTY").ToString.Trim) = 0 And Val(.Columns("MOD_ORDQTY").Text.Trim) = 0 And UCase(txtItmClass.Text.Trim) = "E" Then
                                MessageBox.Show("Quantity Couldn't be Empty.", cntTitle, MessageBoxButtons.OK, MessageBoxIcon.Exclamation)
                                .Focus()

                                Exit Sub
                            End If
                        End If
                    Else

                    End If

                End With

                If LstExpiry.Columns("DOB_BatchInfo").Text.Trim = "" Then
                    LstExpiry.EditActive = False
                    Me.BindingContext(dsDetExpiry.Tables("tblDetails").DefaultView).CancelCurrentEdit()
                    dsDetExpiry.Tables("tblDetails").AcceptChanges()
                    Exit Sub
                End If

            End If
            If sender Is LstBarcode Then
                If LstBarcode.Columns(LstBarcode.Col).Text.Trim <> "" Then
                    Me.BindingContext(dsDetBarcode.Tables("tblDetails").DefaultView).EndCurrentEdit()
                End If
            End If



        Catch EX As Exception
            MessageBox.Show("Error: " & EX.Message, cntTitle, _
            MessageBoxButtons.OK, MessageBoxIcon.Error)
        End Try
    End Sub
    Private Sub txtDate_Validating(ByVal sender As Object, ByVal e As System.ComponentModel.CancelEventArgs) Handles txtDate.Validating, txtActualDate.Validating
        Try
            If sender Is txtDate Then keyValidate()
            If Not IsDate(CType(sender, C1DateEdit).Text) Then Exit Sub

            If (CType(sender, C1DateEdit).Value) Is DBNull.Value Then Exit Sub


            If Not CType(sender, C1DateEdit).ReadOnly AndAlso vatdate.Trim <> "" AndAlso Not CType(sender, C1DateEdit).Value Is DBNull.Value AndAlso CDate(CType(sender, C1DateEdit).Value) < CDate(vatdate) Then
                MessageBox.Show("Recieving Order Date Should Be Greater Than Or Equal " & vatdate, Me.Text, MessageBoxButtons.OK, MessageBoxIcon.Information)
                e.Cancel = True
                Exit Try
            End If
           
            If Wtype <> "ADD" And sender Is txtDate Then Exit Sub

            If Not txtDate.Value Is DBNull.Value Then
                If CDate(CType(sender, C1DateEdit).Value) < CDate("01/01/1800") Then
                    MessageBox.Show("Invalid Date.", cntTitle, MessageBoxButtons.OK, MessageBoxIcon.Exclamation)
                    e.Cancel = True
                    Exit Sub
                End If
            End If

            If Not txtActualDate.Value Is DBNull.Value Then
                If CDate(CType(sender, C1DateEdit).Value) < CDate("01/01/1800") Then
                    MessageBox.Show("Invalid Date.", cntTitle, MessageBoxButtons.OK, MessageBoxIcon.Exclamation)
                    e.Cancel = True
                    Exit Sub
                End If
            End If
            If Not txtDate.Value Is DBNull.Value And Not txtActualDate.Value Is DBNull.Value Then



                If txtActualDate.ReadOnly = False AndAlso Not CDate(txtActualDate.Value) >= CDate(txtDate.Value) Then
                    MessageBox.Show("Actual Date Should Be Greater or Equal than Delivery Date .", cntTitle, MessageBoxButtons.OK, MessageBoxIcon.Exclamation)
                    e.Cancel = True
                    Exit Sub
                End If

            End If

            'If CType(sender, Control) Is txtActualDate Then
            '    If Not txtActualDate.ReadOnly AndAlso vatdate.Trim <> "" AndAlso Not txtActualDate.Value Is DBNull.Value AndAlso CDate(txtActualDate.Value) < CDate(vatdate) Then
            '        MessageBox.Show("Purchase Invoice Date Should Be Greater Than Or Equal " & vatdate, Me.Text, MessageBoxButtons.OK, MessageBoxIcon.Information)
            '        e.Cancel = True
            '        Exit Try
            '    End If
            'End If

            If Not (CType(sender, C1DateEdit).Value) Is DBNull.Value Then
                If Wtype <> "ADD" Then Exit Sub
                If txtDate.Value Is DBNull.Value Or txtActualDate.Value Is DBNull.Value Then Exit Sub
                If CDate(txtDate.Value) > CDate(Now.Date) And Not CDate(txtDate.Value) = CDate(Now.Date) Then
                    MessageBox.Show("Date Should Be Equal or Less Than Today's Date", cntTitle, MessageBoxButtons.OK, MessageBoxIcon.Exclamation)
                    txtDate.Focus()
                    Exit Sub
                End If
            End If


        Catch ex As Exception
            MessageBox.Show("Error : " & ex.Message, cntTitle, _
            MessageBoxButtons.OK, MessageBoxIcon.Error)
        End Try
    End Sub

    Private Sub lstDelivery_DoubleClick(ByVal sender As Object, ByVal e As System.EventArgs) Handles lstDelivery.DoubleClick
        Dim GRIDCODE As String
        Try

            If txtLoc.Text.Trim = "" Then MessageBox.Show("You Should Pick A Location.", cntTitle, MessageBoxButtons.OK, MessageBoxIcon.Exclamation) : txtLoc.Focus() : Exit Sub

            If dsDetDetails.Tables("tblDetails").Rows.Count = 0 Then Exit Sub
            If UCase(lstDelivery.Columns("new_old").Text.Trim) = "O" Then Exit Sub
            If lstDelivery.FilterActive = True Then Exit Sub
            If lstDelivery.Columns(lstDelivery.Col).DataField = "MOD_OrdQty" OrElse lstDelivery.Columns(lstDelivery.Col).DataField = "COST" OrElse lstDelivery.Columns(lstDelivery.Col).DataField = "disc" Then
                Exit Try
            End If
            With lstDelivery
                lcChanges = True

                '  btnCreate.Text = " Modify New Item Ctrl+ M"

                wModify = False

                txtRecid.Text = .Columns("RECID").Text.Trim
                txtSupRef.Text = .Columns("MOD_supitmref").Text.Trim
                txtRef.Text = .Columns("Stk_Ref").Text.Trim
                txtOldNew.Text = .Columns("new_old").Text.Trim
                txtSTkID.Text = .Columns("MOD_RMRecid").Text.Trim
                txtDesc.Text = .Columns("stk_desc").Text.Trim

                wModify = True

                If DelOrder = "Receiving Order Fashion" Or DelOrder = "Consignment Receiving Order Fashion" Then
                    If Val(txtQty.Text.Trim) > 0 Then btnBarcode.Enabled = True Else btnBarcode.Enabled = False
                    txtQty.ReadOnly = True
                    txtQty.Enabled = False
                    txtGender.Text = .Columns("STK_Gender").Text.Trim
                    txtProdGroup.Text = .Columns("ItemType").Text.Trim
                    txtStyle.Text = .Columns("Style").Text.Trim
                    txtSupColor.Text = .Columns("SuppColor").Text.Trim
                    txtColor.Text = .Columns("Color").Text.Trim
                    lstFashQty.Enabled = True
                    '''''''''''' RefGridSizes(dsSql1.Tables(0).Rows(0)("Grid").ToString)
                End If
                txtStatus.Text = .Columns("MOD_stat").Text.Trim
                txtCounter.Text = .Columns("MOD_Count").Text.Trim
                txtQty.Text = .Columns("MOD_OrdQty").Text.Trim
                txtConsQty.Text = .Columns("MOD_ConsOrdQty").Text.Trim
                txtSUBUnit.Text = .Columns("stk_Subunit").Text.Trim
                TXTUNIT.Text = .Columns("stk_unit").Text.Trim
                txtVat.Text = .Columns("invd_VAT").Text.Trim
                txtItmClass.Text = .Columns("ItmClass").Text.Trim

                'TxtItemCurr.Text = .Columns("STK_FCCODE").Text.Trim
                txtPrice.Text = .Columns("COST").Text.Trim
                If txtPrice.Text.Trim <> "" Then
                    txtPrice.Text = CType(txtPrice.Text, Double).ToString("N3")
                End If
                If DelOrder = "Consignment Receiving Order Fashion" Or DelOrder = "Consignment Receiving Order" Then

                    txtPrice.ReadOnly = False
                    'txtCurr.Text = .Columns("STK_FCCODE").Text.Trim
                    TxtDisc.Text = .Columns("Disc").Text.Trim
                    If TxtDisc.Text.Trim <> "" Then
                        TxtDisc.Text = CType(TxtDisc.Text, Double).ToString("N3")
                    End If
                    TxtNet.Text = .Columns("Net").Text.Trim
                    If TxtNet.Text.Trim <> "" Then
                        TxtNet.Text = CType(TxtNet.Text, Double).ToString("N3")
                    End If
                Else
                    'PnlConsigment.Visible = True
                    If txtOldNew.Text = "W" Then
                        txtPrice.Text = "0"
                    End If
                End If

                If UCase(lstDelivery.Columns("NEW_OLD").Text.Trim) = "S" And (DelOrder = "Receiving Order Fashion" Or DelOrder = "Consignment Receiving Order Fashion") Then

                    GRIDCODE = GetFieldDesc("stk04_stkfil", "stk_recid=" & txtSTkID.Text.Trim, "STK_Grid")

                    strsql = " Select recid from " & TmpFashQty & " where recid = " & txtRecid.Text.Trim
                    If RecordsExists(strsql) = False Then
                        strsql = "INSERT INTO " & TmpFashQty
                        strsql &= " VALUES ('" & CStr(CDbl(GetMaxRecId(TmpFashQty, "Q_RECID"))) & "','" & txtRecid.Text.Trim & "',"
                        strsql &= " '" & GRIDCODE & "'"
                        For i = 0 To MaxCnt - 1
                            strsql &= " ,NULL"
                        Next
                        strsql &= " )"
                        Execute(strsql)
                    End If

                    If Not dsDetFqty.Tables("tblDetails") Is Nothing Then dsDetFqty.Tables("tblDetails").Clear()
                    daDetFqty.Fill(dsDetFqty, "tblDetails")

                    lstFashQty.AllowUpdate = True
                    RefGridSizes(GRIDCODE)
                    PnlFashQty.Visible = True
                ElseIf UCase(lstDelivery.Columns("NEW_OLD").Text.Trim) = "W" Then
                    Dim drsql As SqlDataReader
                    'strsql = " Select Sfp_Grid From stk04_stkfilpending where sfp_recid=" & txtSTkID.Text.Trim
                    'ReadFromDBToDataReader(strsql, drsql)
                    'If drsql.HasRows Then
                    '    drsql.Read()
                    '    GRIDCODE = drsql.Item("Sfp_Grid").ToString
                    'Else
                    strsql = " Select Sfp_Grid From " & TempPendingDO & " where sfp_recid=" & txtSTkID.Text.Trim
                    ReadFromDBToDataReader(strsql, drsql)
                    If drsql.HasRows Then
                        drsql.Read()
                        GRIDCODE = drsql.Item("Sfp_Grid").ToString
                    End If
                    'End If
                Else
                    GRIDCODE = GetFieldDesc("stk04_stkfil", "stk_recid=" & txtSTkID.Text.Trim, "stk_grid")
                End If

                If DelOrder = "Receiving Order" Or DelOrder = "Consignment Receiving Order" Then

                    LstExpiry.AllowUpdate = True
                    LstExpiry.AllowAddNew = True
                    LstExpiry.AllowDelete = True

                    LstBarcode.AllowUpdate = True
                    LstBarcode.AllowAddNew = True
                    LstBarcode.AllowDelete = True

                    If UCase(txtItmClass.Text.Trim) = "F" Then
                        txtQty.ReadOnly = True
                        GrpExpBarcode.Visible = False

                    ElseIf UCase(txtItmClass.Text.Trim) = "E" Then
                        txtQty.ReadOnly = True
                        GrpExpBarcode.Visible = True
                        GrpBarcodeButtons.Visible = True
                        BtnGenerateBarcodeTrading.Enabled = True
                        BtnOKExpiryBarcode.Enabled = True
                        LstVisbility(True, True, txtItmClass.Text.Trim)

                    Else
                        txtQty.ReadOnly = False
                        GrpExpBarcode.Visible = False
                        ' LstVisbility(False, True, txtItmClass.Text.Trim)
                    End If
                Else
                    lstFashQty.AllowUpdate = True

                    GrpExpBarcode.Visible = False
                    RefGridSizes(GRIDCODE)
                End If
                Old = False
                lcChanges = False
                lcRef = False
                If UCase(txtOldNew.Text.Trim) = "W" Then
                    btnCreate.Text = "Modify" & vbCrLf & "New Item" & vbCrLf & "Ctrl+ M  "
                Else
                    btnCreate.Text = "Add" & vbCrLf & "New Item" & vbCrLf & "Ctrl+ M"

                End If
                If Val(txtQty.Text.Trim) > 0 Then btnBarcode.Enabled = True Else btnBarcode.Enabled = False
                txtSupRef.Focus()
            End With
        Catch ex As Exception
            MessageBox.Show(ex.Message, cntTitle & ": General Error.", MessageBoxButtons.OK, MessageBoxIcon.Error)
        End Try
    End Sub

    Private Sub LstExpiry_ButtonClick(ByVal sender As Object, ByVal e As C1.Win.C1TrueDBGrid.ColEventArgs) Handles LstExpiry.ButtonClick
        Dim Col As Integer
        Dim QCtitle As String
        Dim selectedRecID, ACCQTy As String
        Dim sTotal As Double
        Dim objSum As Object
        Try

            If e.Column.Name = "QC" Then
                Dim tempferm As frmQC
                tempferm = New frmQC
                If LstExpiry.Columns("DOB_ExpiryDate").Text.Trim = "" OrElse LstExpiry.Columns("DOB_ExpiryDate").Text.Trim = "__.__.____" OrElse _
                    LstExpiry.Columns("DOB_BatchInfo").Text.Trim = "" OrElse LstExpiry.Columns("RECID").Text.Trim = "" OrElse _
                    LstExpiry.Columns("MOD_OrdQty").Text.Trim = "" OrElse LstExpiry.Columns("E_RECID").Text.Trim = "" Then
                    Exit Try
                End If
                QCtitle = "Ref : " & lstDelivery.Columns("Stk_Ref").Text.Trim & " - Desc : "
                QCtitle &= "  " & lstDelivery.Columns("stk_desc").Text.Trim & "   ( " & LstExpiry.Columns("DOB_BatchInfo").Text.Trim & " - "
                QCtitle &= " " & LstExpiry.Columns("DOB_ExpiryDate").Text.Trim & " ) "
                If tempferm.ShowDialog(LstExpiry.Columns("MOD_OrdQty").Text.Trim, QCtitle, LstExpiry.Columns("RECID").Text.Trim, LstExpiry.Columns("E_RECID").Text.Trim, TempQcHeader, TempQcDetails, ACCQTy, Not BtnOKExpiryBarcode.Visible) = Windows.Forms.DialogResult.OK Then

                    LstExpiry.Col = LstExpiry.Columns.IndexOf(LstExpiry.Columns("MOD_OrdQty"))
                    LstExpiry.Columns("MOD_OrdQty").Text = ACCQTy
                    UpdateDtQualityControlHeader()
                    Dim e1 As C1.Win.C1TrueDBGrid.ColEventArgs
                    ' e1.ColIndex = LstExpiry.Columns.IndexOf(LstExpiry.Columns("MOD_OrdQty"))
                    LstExpiry_AfterColUpdate(LstExpiry, e1)
                    LstExpiry.UpdateData()

                    SendKeys.Send("{Down}")

                End If
                Exit Try
            End If
            If txtSTkID.Text.Trim = "" Then Exit Sub
            For i = 0 To LstExpiry.Columns.Count - 1
                If LstExpiry.Columns(i).FilterText <> "" Then
                    MessageBox.Show("Clear Grid Filtration Before Choosing a New Expiry/Lot #", cntTitle, MessageBoxButtons.OK, MessageBoxIcon.Exclamation)
                    Exit Try
                End If
            Next

            If Wtype = "" Or Wtype = "DELETE" Then Exit Sub
            '  If UCase(lstDelivery.Columns("new_old").Text.Trim) = "O" Or UCase(lstDelivery.Columns("new_old").Text.Trim) = "W" Then Exit Sub
            If UCase(txtOldNew.Text) = "W" Then Exit Sub
            If Wtype = "" Or Wtype = "DELETE" Then Exit Sub

            Col = LstExpiry.Col
            If LstExpiry.Splits(0).DisplayColumns(Col).Locked = True Then Exit Sub
            Cursor.Current = Cursors.WaitCursor
            ItemExpiry(txtSTkID.Text.Trim)
        Catch ex As Exception
            MessageBox.Show("Error: " & ex.Message, cntTitle, MessageBoxButtons.OK, MessageBoxIcon.Error)
        End Try
    End Sub

    Private Sub LstExpiry_OnAddNew(ByVal sender As Object, ByVal e As System.EventArgs) Handles LstExpiry.OnAddNew

        Try

            With LstExpiry.Splits(0).DisplayColumns("E_RECID").DataColumn
                .Value = Val(GetMaxRecId(TmpGridExpiry, "E_RECID"))
                .RefreshCell()
            End With

            If UCase(txtItmClass.Text.Trim) = "E" Then
                With LstExpiry.Splits(0).DisplayColumns("recid").DataColumn
                    .Value = txtRecid.Text.Trim
                    .RefreshCell()
                End With

            End If
        Catch ex As Exception
            MessageBox.Show("Error: " & ex.Message, cntTitle, _
            MessageBoxButtons.OK, MessageBoxIcon.Error)
        End Try
    End Sub
    Private Sub LstExpiry_AfterFilter(ByVal sender As Object, ByVal e As C1.Win.C1TrueDBGrid.FilterEventArgs) Handles LstExpiry.AfterFilter
        Try
            If DelOrder = "Receiving Order Fashion" Or DelOrder = "Consignment Receiving Order Fashion" Then Exit Sub
            LstExpiry.Row = 0
            LstExpiry.SelectedRows.Clear()
            LstExpiry.SelectedRows.Add(0)
            If Not dsDetBarcode Is Nothing Then GetBarcode(txtItmClass.Text.Trim)
        Catch ex As Exception
            MessageBox.Show("Error : " & ex.Message, cntTitle, MessageBoxButtons.OK, MessageBoxIcon.Error)
        End Try
    End Sub

    Private Sub LstExpiry_AfterDelete(ByVal sender As Object, ByVal e As System.EventArgs) Handles LstExpiry.AfterDelete
        Dim sFltr As String
        Try

            LstExpiry.UpdateData()
            daDetExpiry.Update(dsDetExpiry.Tables("tblDetails"))

            UpdateDtQualityControlHeader()
            RefQtyLbl(False)


        Catch ex As Exception
            MessageBox.Show("Error: " & ex.Message, cntTitle, _
            MessageBoxButtons.OK, MessageBoxIcon.Error)
        End Try
    End Sub

    Private Sub UpdateLine()
        Dim ds As DataSet
        Dim newRow, drsStock() As DataRow
        Try
            If txtSupRef.Text.Trim = "" Then
                MessageBox.Show("Supp.Ref. Can't Be Empty.", cntTitle, MessageBoxButtons.OK, MessageBoxIcon.Exclamation)
                txtSupRef.Focus()
                Exit Sub
            End If
            If DelOrder = "Receiving Order Fashion" Or DelOrder = "Consignment Receiving Order Fashion" Then
                If txtSupColor.Text.Trim = "" Then
                    MessageBox.Show("Supp.Color Can't Be Empty.", cntTitle, MessageBoxButtons.OK, MessageBoxIcon.Exclamation)
                    txtSupColor.Focus()
                    Exit Sub
                End If
                If txtColor.Text.Trim = "" Then
                    MessageBox.Show("You Must Choose A Color.", cntTitle, MessageBoxButtons.OK, MessageBoxIcon.Exclamation)
                    txtColor.Focus()
                    Exit Sub
                End If
            End If

            If txtQty.Text.Trim.Length >= 14 OrElse txtPrice.Text.Trim.Length >= 14 _
              OrElse TxtNet.Text.Trim.Length >= 14 Then
                ShowNotification(Me, "Invalid Value", 3000, 14, DevComponents.DotNetBar.eToastPosition.MiddleCenter)
                Exit Try
            End If

            lstDelivery.UpdateData()
            daDetDetails.Update(dsDetDetails.Tables("tblDetails"))

            Select Case UCase(txtStatus.Text.Trim.Replace("'", "''"))
                '    Case "N", "F", "NORMAL", "FREE", "R", "Return", "RETURN"
                Case "F", "FREE" : txtStatus.Text = "F" : RefQtyLbl(False)
                Case "N", "NORMAL" : txtStatus.Text = "N" : RefQtyLbl(False)
                Case "R", "RETURN" : txtStatus.Text = "R" : RefQtyLbl(False)
                Case "C", "CONSIGNMENT" : txtStatus.Text = "C" : RefQtyLbl(False)
                Case Else
                    MessageBox.Show("Invalid Status", cntTitle, MessageBoxButtons.OK, MessageBoxIcon.Exclamation)
                    txtStatus.Text = ""
                    txtStatus.Focus()
                    Exit Sub
            End Select
            If txtOldNew.Text = "W" AndAlso txtStatus.Text.Trim = "R" Then
                ShowNotification(Me, "Pending Item Status Can Be N or F", 2000, 14, DevComponents.DotNetBar.eToastPosition.MiddleCenter)
                txtStatus.Text = ""
                txtStatus.Focus()
                Exit Sub
            End If
            If DelOrder = "Receiving Order Fashion" Or DelOrder = "Consignment Receiving Order Fashion" Then
                lstFashQty.UpdateData()
                daDetFqty.Update(dsDetFqty.Tables("tblDetails"))
                If FashItemAlreadyExist() = False Then
                    Exit Sub
                End If
            End If


            If wModify = False Then
                newRow = dsDetDetails.Tables("tblDetails").NewRow
                With newRow


                    .Item("RecID") = txtRecid.Text.Trim
                    .Item("MOD_supitmref") = txtSupRef.Text.Trim
                    .Item("Stk_ref") = txtRef.Text.Trim
                    .Item("MOD_stat") = txtStatus.Text.Trim
                    .Item("MOD_RMRecid") = txtSTkID.Text.Trim
                    ' .Item("MOD_Count") = txtCounter.Text.Trim
                    .Item("stk_desc") = txtDesc.Text.Trim                    
                    .Item("MOD_OrdQty") = txtQty.Text.Trim

                    .Item("stk_Subunit") = txtSUBUnit.Text
                    .Item("stk_unit") = TXTUNIT.Text
                    .Item("MOD_ConsOrdQty") = txtConsQty.Text.Trim
                    .Item("invd_VAT") = txtVat.Text
                    If UCase(lstDelivery.Columns("new_old").Text.Trim) <> "W" Then
                        .Item("new_old") = "N"
                    Else
                        InsertPendItem(TempPendingDO, txtSTkID.Text.Trim)
                    End If

                    .Item("ItmClass") = txtItmClass.Text
                    If DelOrder = "Receiving Order Fashion" Or DelOrder = "Consignment Receiving Order Fashion" Then
                        PnlFashQty.Visible = False
                        .Item("SuppColor") = txtSupColor.Text.Trim
                        .Item("STK_Gender") = txtGender.Text.Trim
                        .Item("ItemType") = txtProdGroup.Text.Trim
                        .Item("Style") = txtStyle.Text.Trim
                        .Item("Color") = txtColor.Text.Trim
                        .Item("ColorN_O") = txtColorON.Text.Trim

                        'checking if this color to this item is new in stock 
                        strsql = "SELECT STQ_RecId FROM stk04_stkqty WHERE STQ_STKRECID='" & txtSTkID.Text.Trim & "' "
                        strsql &= " AND STQ_Color='" & txtColor.Text.Trim & "'  AND STQ_LOCCODE='00'"
                        Dim drsql As SqlDataReader
                        ReadFromDBToDataReader(strsql, drsql)
                        If drsql.HasRows Then
                            .Item("ColorN_O") = "O"
                        Else
                            .Item("ColorN_O") = "N"
                        End If
                        'get color description in the footer 
                        If lstDelivery.Columns("Color").Text.Trim <> "" Then
                            lstDelivery.Columns("Color").FooterText = GetFieldDesc("Stk04_COLOR", " Color_Code='" & lstDelivery.Columns("Color").Text.Trim & "'", "Color_Reference")
                        Else
                            lstDelivery.Columns("Color").FooterText = ""
                        End If
                    End If
                    .Item("STK_FCCODE") = txtCurr.Text.Trim
                    .Item("COST") = txtPrice.Text.Trim
                    If DelOrder = "Consignment Receiving Order" Or DelOrder = "Consignment Receiving Order Fashion" Then

                        .Item("net") = TxtNet.Text.Trim
                        .Item("disc") = TxtDisc.Text.Trim
                    End If
                End With
                dsDetDetails.Tables("tblDetails").Rows.Add(newRow)


                'Update Row to Real Temp Table
                ds = dsDetDetails.GetChanges
                If Not ds Is Nothing Then
                    daDetDetails.Update(ds)
                    dsDetDetails.Merge(ds) : ds = Nothing
                End If
                dsDetDetails.AcceptChanges()
            Else


                strsql = " Update " & TmpGridDetails & " SET"
                strsql &= " MOD_supitmref='" & txtSupRef.Text.Trim.Replace("'", "''") & "',"
                strsql &= " MOD_stat='" & txtStatus.Text.Trim.Replace("'", "''") & "',"
                strsql &= " MOD_OrdQty= '" & CDec(txtQty.Text.Trim) & "' ,"
                strsql &= " MOD_ConsOrdQty= '" & CDec(IIf(txtConsQty.Text.Trim = "", "0", txtConsQty.Text.Trim)) & "' ,"
                '     strsql &= " stk_fccode= '" & (TxtItemCurr.Text.Trim) & "' ,"
                strsql &= " cost= '" & CDec(IIf(txtPrice.Text.Trim = "", "0", txtPrice.Text.Trim)) & "' ,"
                If DelOrder = "Consignment Receiving Order" Or DelOrder = "Consignment Receiving Order Fashion" Then
                    strsql &= " Disc= '" & CDec(IIf(TxtDisc.Text.Trim = "", "0", TxtDisc.Text.Trim)) & "' ,"
                    strsql &= " net= '" & CDec(IIf(TxtNet.Text.Trim = "", "0", TxtNet.Text.Trim)) & "' ,"
                End If
                strsql &= " new_old = CASE WHEN new_old='W' THEN 'W' ELSE 'N' END  "

                If DelOrder = "Receiving Order Fashion" Or DelOrder = "Consignment Receiving Order Fashion" Then
                    strsql &= " ,SuppColor='" & txtSupColor.Text.Trim.Replace("'", "''") & "',"
                    strsql &= " Color='" & txtColor.Text.Trim.Replace("'", "''") & "' "

                End If
                strsql &= " WHERE RECID=" & txtRecid.Text.Trim & " "

                Execute(strsql)

            End If

            If Not dsDetDetails.Tables("tblDetails") Is Nothing Then dsDetDetails.Tables("tblDetails").Clear()
            daDetDetails.Fill(dsDetDetails, "tblDetails")

            lstDelivery.Refresh()

            ReadonlyConsFash()

            Old = False
            RefQtyLbl(False)
            INITCOL(True)
            btnBarcode.Enabled = False
            lcChanges = True
            wModify = False

            ObjSumNormal = dsDetDetails.Tables("tblDetails").Compute("Sum( MOD_OrdQty)", " MOD_stat='N' ")
            ObjSumFree = dsDetDetails.Tables("tblDetails").Compute("Sum( MOD_OrdQty)", " MOD_stat='F' ")
            ObjSumReturn = dsDetDetails.Tables("tblDetails").Compute("Sum( MOD_OrdQty)", " MOD_stat='R' ")
            objSum = CDbl(IIf(ObjSumNormal Is DBNull.Value, "0", ObjSumNormal)) + CDbl(IIf(ObjSumFree Is DBNull.Value, "0", ObjSumFree)) - CDbl(IIf(ObjSumReturn Is DBNull.Value, "0", ObjSumReturn))
            lstDelivery.Splits(0).DisplayColumns("MOD_OrdQty").DataColumn.FooterText = CType(objSum, Double).ToString("N" & gbDecNo)
            If DelOrder = "Consignment Receiving Order" Or DelOrder = "Consignment Receiving Order Fashion" Then
                ObjSumNormal = dsDetDetails.Tables("tblDetails").Compute("Sum( Net)", " MOD_stat='N' ")
                ObjSumFree = dsDetDetails.Tables("tblDetails").Compute("Sum( Net)", " MOD_stat='F' ")
                ObjSumReturn = dsDetDetails.Tables("tblDetails").Compute("Sum( Net)", " MOD_stat='R' ")
                objSum = CDbl(IIf(ObjSumNormal Is DBNull.Value, "0", ObjSumNormal)) + CDbl(IIf(ObjSumFree Is DBNull.Value, "0", ObjSumFree)) - CDbl(IIf(ObjSumReturn Is DBNull.Value, "0", ObjSumReturn))
                lstDelivery.Splits(0).DisplayColumns("Net").DataColumn.FooterText = CType(objSum, Double).ToString("N3")
            End If
            btnCreate.Text = "Add" & vbCrLf & "New Item" & vbCrLf & "Ctrl+ M"
        Catch ex As Exception
            MessageBox.Show("Error : " & ex.Message, cntTitle, _
            MessageBoxButtons.OK, MessageBoxIcon.Error)
        End Try
    End Sub

    Private Sub LstExpiry_BeforeColEdit(ByVal sender As Object, ByVal e As C1.Win.C1TrueDBGrid.BeforeColEditEventArgs) Handles LstExpiry.BeforeColEdit
        Try
            If LstExpiry.FilterActive = True Then Exit Sub

            If LstExpiry.Columns(e.ColIndex).DataField = "MOD_OrdQty" AndAlso (LstExpiry.Columns("Dob_batchinfo").Text = "" Or LstExpiry.Columns("DOB_ExpiryDate").Text = "") Then e.Cancel = True : Exit Try

            If UCase(LstExpiry.Columns("exp_stat").Text) <> "N" AndAlso LstExpiry.Columns(e.ColIndex).DataField = "DOQ_ProductionDt" Then e.Cancel = True : Exit Try

            If txtSTkID.Text.Trim = "" Then Old = True : e.Cancel = True : Exit Sub
            If Not DtQualityControlHeader Is Nothing Then
                If DtQualityControlHeader.Select("PrDReciD =  " & LstExpiry.Columns("RECID").Text.Trim & " and  PrQDRecId =  " & LstExpiry.Columns("E_RECID").Text.Trim & "  and (isnull(AccQty,0) <> 0 or isnull(RjQty,0) <> 0) ").Count <> 0 Then
                    e.Cancel = True
                    MessageBox.Show("You Can Only Change Your Quantity From Accepted Quantity In Quantity Control Form", cntTitle, MessageBoxButtons.OK, MessageBoxIcon.Information)
                    Exit Try
                End If
            End If

        Catch ex As Exception
            MessageBox.Show("Error : " & ex.Message, cntTitle, _
                MessageBoxButtons.OK, MessageBoxIcon.Error)
        End Try
    End Sub
    Private Sub LstExpiry_BeforeColUpdate(ByVal sender As Object, ByVal e As C1.Win.C1TrueDBGrid.BeforeColUpdateEventArgs) Handles LstExpiry.BeforeColUpdate
        Dim col As Int32
        Dim ItmQty As Object
        Try
            If UCase(txtItmClass.Text.Trim) = "E" Then
                col = e.ColIndex
                With LstExpiry.Columns(col)
                    If .DataField = "DOB_ExpiryDate" Then
                        If .Text = "__.__.____" OrElse .Text = "" Then
                            .Text = "" : .Value = DBNull.Value : LstExpiry.Columns("MOD_OrdQty").Text = "0" : .RefreshCell()
                        ElseIf Len(.Text.Replace("_", "").Replace(" ", "")) < 10 OrElse Len(.Text.Replace("_", "").Replace(" ", "").Substring(.Text.IndexOf(".") + 1)) <> 7 _
                            OrElse Len(.Text.Replace("_", "").Replace(" ", "").Substring(0, .Text.IndexOf("."))) <> 2 OrElse Not IsDate(.Text.Replace(" ", "").Replace("00", "01")) _
                            OrElse .Text.Replace(" ", "").Substring(6, 4) < "1800" OrElse (.Text.Substring(3, 2) = "00") Then
                            MessageBox.Show("Invalid Expiry Date.", cntTitle, MessageBoxButtons.OK, MessageBoxIcon.Warning)
                            e.Cancel = True
                        ElseIf (.Text.Substring(0, 2) = "00") Then
                            'ch @ 10/10/2016
                            Dim months As Integer = CInt(IIf(Month(CDate(.Text.Replace("00", "01"))) + 1 = 13, 1, Month(CDate(.Text.Replace("00", "01"))) + 1))
                            Dim years As Integer = CInt(IIf(Month(CDate(.Text.Replace("00", "01"))) + 1 = 13, Year(CDate(.Text.Replace("00", "01"))) + 1, Year(CDate(.Text.Replace("00", "01")))))
                            Dim aDate As DateTime = CDate("01." & months & "." & years) 'sample date
                            Dim StartDate As DateTime = aDate.AddMonths(-1).AddDays(-(aDate.Day - 1))
                            Dim EndDate As DateTime = StartDate.AddDays(DateTime.DaysInMonth(StartDate.Year, StartDate.Month) - 1)
                            If EndDate < CDate(txtDate.Text) Then
                                MessageBox.Show("Item Expired!", cntTitle, MessageBoxButtons.OK, MessageBoxIcon.Warning)
                                e.Cancel = True : Exit Sub
                            End If
                        ElseIf IsDate(.Text.Replace(" ", "")) Then
                            If CDate(.Text.Replace(" ", "")) < CDate(txtDate.Text.Trim) Then
                                MessageBox.Show("Item Expired!", cntTitle, MessageBoxButtons.OK, MessageBoxIcon.Warning)
                                e.Cancel = True : Exit Sub
                            End If

                        End If
                    ElseIf .DataField = "DOQ_ProductionDt" Then
                        If .Text = "__.__.____" OrElse .Text = "" Then
                            .Text = " " : .Value = DBNull.Value : .RefreshCell()
                        ElseIf Len(.Text.Replace("_", "").Replace(" ", "")) < 10 OrElse Len(.Text.Replace("_", "").Replace(" ", "").Substring(.Text.IndexOf(".") + 1)) <> 7 _
                            OrElse Len(.Text.Replace("_", "").Replace(" ", "").Substring(0, .Text.IndexOf("."))) <> 2 OrElse Not IsDate(.Text.Replace(" ", "").Replace("00", "01")) _
                            OrElse .Text.Replace(" ", "").Substring(6, 4) < "1800" OrElse (.Text.Substring(3, 2) = "00") Then
                            MessageBox.Show("Invalid Production Date.", cntTitle, MessageBoxButtons.OK, MessageBoxIcon.Warning)
                            e.Cancel = True
                        ElseIf (.Text.Substring(0, 2) = "00") Then
                            Dim months As Integer = CInt(IIf(Month(CDate(.Text.Replace("00", "01"))) + 1 = 13, 1, Month(CDate(.Text.Replace("00", "01"))) + 1))
                            Dim years As Integer = CInt(IIf(Month(CDate(.Text.Replace("00", "01"))) + 1 = 13, Year(CDate(.Text.Replace("00", "01"))) + 1, Year(CDate(.Text.Replace("00", "01")))))
                            Dim aDate As DateTime = CDate("01." & months & "." & years) 'sample date
                            Dim StartDate As DateTime = aDate.AddMonths(-1).AddDays(-(aDate.Day - 1))
                            Dim EndDate As DateTime = StartDate.AddDays(DateTime.DaysInMonth(StartDate.Year, StartDate.Month) - 1)
                            If EndDate > CDate(txtDate.Text) Then
                                MessageBox.Show("Production Date Cannot Be Greater Than Receiving Order Date!", cntTitle, MessageBoxButtons.OK, MessageBoxIcon.Warning)
                                e.Cancel = True : Exit Sub
                            End If
                        ElseIf IsDate(.Text.Replace(" ", "")) Then
                            If CDate(.Text.Replace(" ", "")) > CDate(txtDate.Text.Trim) Then
                                MessageBox.Show("Production Date Cannot Be Greater Than Receiving Order Date!", cntTitle, MessageBoxButtons.OK, MessageBoxIcon.Warning)
                                e.Cancel = True : Exit Sub
                            End If
                        ElseIf Len(.Text.Replace("_", "").Replace(" ", "")) < 10 OrElse Len(.Text.Replace("_", "").Replace(" ", "").Substring(.Text.IndexOf(".") + 1)) <> 7 _
                            OrElse Len(.Text.Replace("_", "").Replace(" ", "").Substring(0, .Text.IndexOf("."))) <> 2 OrElse Not IsDate(.Text.Replace(" ", "").Replace("00", "01")) _
                            OrElse .Text.Replace(" ", "").Substring(6, 4) < "1800" OrElse (.Text.Substring(3, 2) = "00") Then
                            MessageBox.Show("Invalid Production Date.", cntTitle, MessageBoxButtons.OK, MessageBoxIcon.Warning)
                            e.Cancel = True
                        End If

                    End If

                    If LstExpiry.Columns("DOB_ExpiryDate").Text <> "__.__.____" AndAlso LstExpiry.Columns("DOB_ExpiryDate").Text <> "" _
                            AndAlso LstExpiry.Columns("Dob_batchinfo").Text <> "" Then

                        If dsDetExpiry.Tables("tblDetails").Select("DOB_ExpiryDate='" & LstExpiry.Columns("DOB_ExpiryDate").Text & "' and Dob_batchinfo ='" & LstExpiry.Columns("Dob_batchinfo").Text & "' and  e_RecId <> '" & LstExpiry.Columns("e_RecId").Text & "'").Length >= 1 Then
                            MessageBox.Show("Expiry Date And Lot # Already Exist.", cntTitle, MessageBoxButtons.OK, MessageBoxIcon.Warning)
                            e.Cancel = True
                        End If
                    End If

                    'test if this expiry and lot number already exist in same item 
                    If LstExpiry.Columns("DOB_ExpiryDate").Text <> "__.__.____" AndAlso LstExpiry.Columns("DOB_ExpiryDate").Text.Trim <> "" _
                            AndAlso LstExpiry.Columns("Dob_batchinfo").Text.Trim <> "" AndAlso Val(LstExpiry.Columns("MOD_OrdQty").Text.Trim) <> 0 Then
                        strsql = "  SELECT  * FROM   " & TmpGridExpiry
                        strsql &= " WHERE recid=" & LstExpiry.Columns("RECID").Text.Trim & " and  E_RECID<>" & LstExpiry.Columns("E_RECID").Text.Trim & " and exp_stat <> 'D' AND  ISNULL(CONVERT (VARCHAR, DOB_BatchInfo) + '-'"
                        strsql &= " + CONVERT(VARCHAR, DOB_ExpiryDate), '')= ISNULL('" & LstExpiry.Columns("Dob_batchinfo").Text.Trim & "'  + '-'"
                        strsql &= " +  '" & LstExpiry.Columns("DOB_ExpiryDate").Text.Trim & "'  , '')"
                        If RecordsExists(strsql) = True Then
                            MessageBox.Show("Expiry Date And Lot # Already Exist.", cntTitle, MessageBoxButtons.OK, MessageBoxIcon.Warning)
                            e.Cancel = True
                        End If
                    End If
                End With

                If LstExpiry.Columns("MOD_OrdQty").Text <> "" AndAlso CDbl(LstExpiry.Columns("MOD_OrdQty").Text) <> 0 Then
                    If txtItmClass.Text.Trim.ToUpper = "E" Then
                        Using cnsql As New SqlConnection(ConnectionString)
                            cnsql.Open()
                            strsql = " SELECT ISNULL(SUM(t2.MOD_OrdQty),0) FROM " & TmpGridDetails & " t1"
                            strsql &= " INNER JOIN " & TmpGridExpiry & " t2 ON t1.recid = t2.recid AND DOB_Drecid IS NULL"
                            strsql &= " WHERE MOD_RMRecid = '" & txtSTkID.Text.Trim & "'"
                            strsql &= " AND MOD_stat = 'R'"
                            strsql &= " AND ISNULL(DOB_BatchInfo,'') = '" & LstExpiry.Columns("Dob_batchinfo").Text & "'"
                            strsql &= " AND ISNULL(DOB_ExpiryDate,'') = '" & LstExpiry.Columns("DOB_ExpiryDate").Text & "'"
                            strsql &= " AND new_old <> 'D'"
                            strsql &= " AND t1.recid <> " & txtRecid.Text.Trim & ""
                            Using cmsql As New SqlCommand(strsql, cnsql)
                                ItmQty = cmsql.ExecuteScalar
                            End Using
                            cnsql.Close()
                        End Using

                        If ItmQty Is DBNull.Value Then
                            ItmQty = 0
                        Else
                            ItmQty = CDbl(ItmQty)
                        End If
                        ItmQty = (CDbl(ItmQty) + CDbl(LstExpiry.Columns("MOD_OrdQty").Text))

                        If QtyCntrEc(txtSTkID.Text.Trim, ItmQty.ToString, LstExpiry.Columns("DOB_ExpiryDate").Text, LstExpiry.Columns("Dob_batchinfo").Text) Then
                            'ShowNotification(Me, "  Qty Cntr  ", 2500, 14, DevComponents.DotNetBar.eToastPosition.MiddleCenter)
                            e.Cancel = True
                            Exit Try
                        End If
                    End If

                End If

                If LstExpiry.Columns("DOB_ExpiryDate").Text.Trim <> "__.__.____" AndAlso LstExpiry.Columns("DOB_ExpiryDate").Text.Trim <> "" _
                                AndAlso LstExpiry.Columns("DOB_BatchInfo").Text.Trim <> "" Then
                    strsql = " SELECT STQ_BatchInfo,STQ_ProductionDt FROM dbo.stk04_stkqty WHERE STQ_LOCCODE='00' AND STQ_STKRECID='" & txtSTkID.Text.Trim & "'"
                    strsql &= " AND STQ_BatchInfo='" & LstExpiry.Columns("DOB_BatchInfo").Text.Trim.Replace("'", "''") & "'"
                    strsql &= " AND STQ_ExpiryDt='" & LstExpiry.Columns("DOB_ExpiryDate").Text.Trim.Replace("'", "''") & "'"

                    Dim daSql As SqlDataAdapter
                    Dim dtTbl As DataTable

                    Using cnSql As New SqlConnection(ConnectionString)
                        cnSql.Open()
                        Using cmSql As New SqlCommand(strsql, cnSql)
                            daSql = New SqlDataAdapter(cmSql)
                            dtTbl = New DataTable

                            daSql.Fill(dtTbl)

                            If dtTbl.Rows.Count > 0 Then
                                ExpiryRed = False
                                LstExpiry.Columns("exp_stat").Text = "O"
                                If dtTbl(0)("STQ_ProductionDt").ToString <> "" Then
                                    LstExpiry.Columns("DOQ_ProductionDT").Text = dtTbl(0)("STQ_ProductionDt").ToString
                                Else
                                    LstExpiry.Columns("DOQ_ProductionDT").Text = ""
                                End If
                            Else
                                ExpiryRed = True
                                LstExpiry.Columns("exp_stat").Text = "N"
                            End If
                        End Using
                    End Using
                End If
            End If
        Catch ex As Exception
            MessageBox.Show("Error : " & ex.Message, cntTitle, _
            MessageBoxButtons.OK, MessageBoxIcon.Error)
        End Try
    End Sub

    Private Sub LstBarcodeF_AfterUpdate(ByVal sender As Object, ByVal e As System.EventArgs) Handles LstBarcodeF.AfterUpdate
        Try
            daDetBarcodeF.Update(dsDetBarcodeF.Tables("tblDetails"))
        Catch ex As Exception
            MessageBox.Show("Error: " & ex.Message, cntTitle, MessageBoxButtons.OK, MessageBoxIcon.Error)
        End Try
    End Sub

    Private Sub lstFashQty_BeforeColEdit(ByVal sender As Object, ByVal e As C1.Win.C1TrueDBGrid.BeforeColEditEventArgs) Handles lstFashQty.BeforeColEdit
        Try
            If txtRecid.Text.Trim = "" Then e.Cancel = True
        Catch ex As Exception
            MessageBox.Show("Error: " & ex.Message, cntTitle, MessageBoxButtons.OK, MessageBoxIcon.Error)
        End Try
    End Sub

    Private Sub lstDelivery_Enter(ByVal sender As Object, ByVal e As System.EventArgs) Handles LstBarcode.Enter, lstDelivery.Enter, LstExpiry.Enter, lstFashQty.Enter, LstBarcodeF.Enter
        Try
            With CType(sender, C1TrueDBGrid)
                If .Splits(0).Rows.Count > 0 Then
                    '.Splits(0).AddCellStyle(C1.Win.C1TrueDBGrid.CellStyleFlag.CurrentCell, current_S)
                End If
            End With

            If sender Is lstDelivery Then
                '    If UCase(lstDelivery.Columns(lstDelivery.Col).DataField) = "BARCODE" Then Exit Sub

                If txtSTkID.Text.Trim <> "" Then Exit Sub
                If DelOrder = "Receiving Order" Or DelOrder = "Consignment Receiving Order" Then
                    'GrpExpBarcode.Visible = True
                    'If UCase(lstDelivery.Columns("ITMCLASS").Text.Trim) = "E" Then
                    '    LstVisbility(True, True, lstDelivery.Columns("ITMCLASS").Text.Trim)
                    'Else
                    '    LstVisbility(False, True, lstDelivery.Columns("ITMCLASS").Text.Trim)
                    'End If
                Else            ' GrpExpBarcode.Visible = False
                    RefGridSizes(lstFashQty.Columns("GridCode").Text.Trim)
                End If
                'If DelOrder = "Consignment Receiving Order Fashion" Or DelOrder = "Consignment Receiving Order" Then
                '    '  PnlConsigment.Visible = True

                '    txtPrice.ReadOnly = True
                '    'txtCurr.Text = lstDelivery.Columns("STK_FCcode").Text.Trim
                '    txtPrice.Text = lstDelivery.Columns("COST").Text.Trim
                '    If txtPrice.Text.Trim <> "" Then
                '        txtPrice.Text = CType(txtPrice.Text, Double).ToString("N3")
                '    End If
                'End If
            End If
            '  If UCase(lstDelivery.Columns(lstDelivery.Col).DataField) = "BARCODE" Then Exit Sub
            If DelOrder = "Consignment Receiving Order Fashion" Or DelOrder = "Receiving Order Fashion" Then

                If lstDelivery.Columns("Color").Text.Trim <> "" Then
                    lstDelivery.Columns("Color").FooterText = GetFieldDesc("Stk04_COLOR", " Color_Code='" & lstDelivery.Columns("Color").Text.Trim & "'", "Color_Reference")
                Else
                    lstDelivery.Columns("Color").FooterText = ""
                End If
            End If
        Catch exp As Exception
            MessageBox.Show("Error: " & exp.Message, cntTitle, _
            MessageBoxButtons.OK, MessageBoxIcon.Error)
        End Try
    End Sub

    Private Sub lstDelivery_Leave(ByVal sender As Object, ByVal e As System.EventArgs) Handles lstDelivery.Leave, LstBarcode.Leave, LstExpiry.Leave, lstFashQty.Leave, LstBarcodeF.Leave
        Dim sFltr As String
        Dim sTotal As Integer
        Dim objSum As Object
        Try

            With CType(sender, C1TrueDBGrid)
                If .Splits(0).Rows.Count > 0 Then
                    '.Splits(0).AddCellStyle(C1.Win.C1TrueDBGrid.CellStyleFlag.CurrentCell, normal_S)
                End If
            End With
            If sender Is LstBarcode Then
                If LstBarcode.Columns(LstBarcode.Col).Text.Trim = "" Then
                    Me.BindingContext(dsDetBarcode.Tables("tblDetails").DefaultView).EndCurrentEdit()
                End If
            End If
        Catch exp As Exception
            MessageBox.Show("Error: " & exp.Message, cntTitle, _
            MessageBoxButtons.OK, MessageBoxIcon.Error)
        End Try
    End Sub

    Private Sub LstBarcode_BeforeDelete(ByVal sender As Object, ByVal e As C1.Win.C1TrueDBGrid.CancelEventArgs) Handles LstBarcode.BeforeDelete
        Try
            'LstBarcode.UpdateData()
            If Wtype = "" Or Wtype = "DELETE" Then e.Cancel = True : Exit Sub
            If txtRecid.Text.Trim = "" Then e.Cancel = True : Exit Sub

            LstBarcode.Columns("bar_stat").Text = "D"

            LstBarcode.UpdateData()
            daDetBarcode.Update(dsDetBarcode.Tables("tblDetails"))
            e.Cancel = True

            GetBarcode(txtItmClass.Text.Trim)

            If Not dsDetBarcode.Tables("tblDetails") Is Nothing Then dsDetBarcode.Tables("tblDetails").Clear()
            With daDetBarcode
                .FillSchema(dsDetBarcode, SchemaType.Mapped)
                .Fill(dsDetBarcode, "tblDetails")
            End With

        Catch ex As Exception
            MessageBox.Show("Error: " & ex.Message, cntTitle, _
            MessageBoxButtons.OK, MessageBoxIcon.Error)
        End Try
    End Sub

    Private Sub LstExpiry_BeforeDelete(ByVal sender As Object, ByVal e As C1.Win.C1TrueDBGrid.CancelEventArgs) Handles LstExpiry.BeforeDelete
        Dim sFltr As String
        Dim sTotal As Integer
        Dim objSum As Object
        Dim drs As DataRow()
        Try
            If Wtype = "" Or Wtype = "DELETE" Then e.Cancel = True : Exit Sub
            If txtSTkID.Text.Trim = "" Then e.Cancel = True : Exit Sub
            LstExpiry.Columns("exp_stat").Text = "D"


            drs = dsDetBarcode.Tables("tblDetails").Select("  [E_RECID] = " & Val(LstExpiry.Columns("E_RECID").Text.Trim.Replace("'", "''")) & "")

            If drs.Length > 0 Then
                If MessageBox.Show("Are You Sure You Want To Delete This Expiry Date And Lot #? If YES, The Barcodes Related Will Be Deleted As Well", cntTitle, MessageBoxButtons.YesNo, MessageBoxIcon.Question) = vbNo Then
                    e.Cancel = True
                    Exit Sub
                End If
            End If
            LstExpiry.UpdateData()
            daDetExpiry.Update(dsDetExpiry.Tables("tblDetails"))
            ExpiryRed = False
            e.Cancel = True
            strsql = " DELETE FROM " & TempQcDetails & ""
            strsql &= " FROM " & TempQcDetails & " details INNER JOIN"
            strsql &= " " & TempQcHeader & " header ON header.ReciD = details.HRecID"
            strsql &= " WHERE PrDReciD = " & Val(LstExpiry.Columns("RECID").Text) & " and PrQDRecId =  '" & Val(LstExpiry.Columns("E_RECID").Text) & "'  "
            strsql &= " DELETE FROM  " & TempQcHeader & "  WHERE PrDReciD = " & Val(LstExpiry.Columns("RECID").Text) & ""
            strsql &= " and PrQDRecId = '" & Val(LstExpiry.Columns("E_RECID").Text) & "' "
            Execute(strsql)
            strsql = " UPDATE " & TmpGridBarcode & " set bar_Stat = 'D' where E_RECID='" & Val(LstExpiry.Columns("E_RECID").Text) & "' and RECID='" & Val(txtRecid.Text.Trim) & "'"
            Execute(strsql)
            GetBarcode(txtItmClass.Text.Trim)

            GetExpiry(txtItmClass.Text.Trim)

            sFltr = "exp_stat <> 'D' and recid =" & txtRecid.Text.Trim
            objSum = dsDetExpiry.Tables("tblDetails").Compute("Sum(MOD_OrdQty)", sFltr)
            If objSum Is DBNull.Value Then
                sTotal = 0 : lstDelivery.Splits(0).DisplayColumns("MOD_OrdQty").DataColumn.FooterText = ""
                txtQty.Text = CStr(sTotal)
                txtConsQty.Text = CStr(sTotal)
            Else
                sTotal = CInt(CType(objSum, Double))
                ''txtQty.Text = CStr(sTotal - Val(LstExpiry.Columns("MOD_OrdQty").Text.Trim))
                txtQty.Text = CStr(sTotal)
                NetCalculate()
                'update consolidated QTy
                If UCase(lstDelivery.Columns("New_old").Text.Trim) = "W" Then

                    Dim Str As String
                    Str = "Select case Meas_Sign WHEN '*' THEN case when pn1.sfp_operator='*' then " & Val(txtQty.Text.Trim) & " *  pn1.SFP_UNITPACK  * STK04_Measurements.Meas_Value else " & Val(txtQty.Text.Trim) & " /  pn1.SFP_UNITPACK  * STK04_Measurements.Meas_Value end "
                    Str &= " ELSE case when pn1.sfp_operator='*' then  (  pn1.SFP_UNITPACK   * " & Val(txtQty.Text.Trim) & " ) /   STK04_Measurements.Meas_Value else  (  " & Val(txtQty.Text.Trim) & " / pn1.SFP_UNITPACK   ) /   STK04_Measurements.Meas_Value end  End [ctr]"
                    Str &= " FROM   STK04_StkFilPending pn1"
                    Str &= " left JOIN STK04_Measurements ON Meas_unit =  pn1.SFP_SUBUNIT "
                    Str &= " WHERE  pn1.SFP_Recid = " & Val(txtSTkID.Text.Trim) & " "

                    Dim drSQL As SqlDataReader
                    ReadFromDBToDataReader(Str, drSQL)
                    If drSQL.HasRows Then
                        drSQL.Read()
                        txtConsQty.Text = Format(drSQL.Item("Ctr"), "#,##0.000").ToString()
                    Else
                        Str = " Select case Meas_Sign WHEN '*' THEN " & Val(txtQty.Text.Trim) & " *  pn2.SFP_UNITPACK   * STK04_Measurements.Meas_Value"
                        Str &= " ELSE (  SFP_UNITPACK   * " & Val(txtQty.Text.Trim) & " ) /   STK04_Measurements.Meas_Value End [ctr]"
                        Str &= " From " & TempPendingDO & "  pn2 "
                        Str &= " left JOIN STK04_Measurements ON Meas_unit = pn2.SFP_SUBUNIT "
                        Str &= " WHERE  pn2.SFP_Recid = " & Val(txtSTkID.Text.Trim) & " "

                        ReadFromDBToDataReader(Str, drSQL)
                        If drSQL.HasRows Then
                            drSQL.Read()
                            txtConsQty.Text = Format(drSQL.Item("Ctr"), "#,##0.000").ToString()
                        End If
                    End If
                Else
                    strsql = " SELECT  CASE meas_sign"
                    strsql &= " WHEN '*' THEN " & Val(txtQty.Text.Trim) & " * stk_unitpack * meas_value"
                    strsql &= " ELSE ( stk_unitpack * " & Val(txtQty.Text.Trim) & " ) / Meas_value End as [Ctr] FROM V_Pord2_GetStkMeas WHERE   stk_recid = '" & txtSTkID.Text.Trim & "' "
                    Dim drsql As SqlDataReader
                    ReadFromDBToDataReader(strsql, drsql)
                    If drsql.HasRows Then
                        drsql.Read()
                        txtConsQty.Text = Format(drsql.Item("Ctr"), "#,##0.000").ToString()
                    End If
                End If
            End If


            If Not dsDetExpiry.Tables("tblDetails") Is Nothing Then dsDetExpiry.Tables("tblDetails").Clear()
            With daDetExpiry
                .FillSchema(dsDetExpiry, SchemaType.Mapped)
                .Fill(dsDetExpiry, "tblDetails")
            End With



            Dim sum As Integer = 0


        Catch ex As Exception
            MessageBox.Show("Error: " & ex.Message, cntTitle, _
            MessageBoxButtons.OK, MessageBoxIcon.Error)
        End Try
    End Sub
    Private Sub LstExpiry_BeforeRowColChange(ByVal sender As Object, ByVal e As C1.Win.C1TrueDBGrid.CancelEventArgs) Handles LstExpiry.BeforeRowColChange
        Dim drsql As SqlDataReader
        Try

            If txtSTkID.Text.Trim = "" Then Exit Sub

            If LstExpiry.DestinationRow <> LstExpiry.RowBookmark(LstExpiry.Row) Then

                If Val(LstExpiry.Columns("MOD_OrdQty").Text.Trim) = 0 Then
                    If Not DtQualityControlHeader Is Nothing AndAlso txtRecid.Text.Trim <> "" Then
                        If DtQualityControlHeader.Select("PrDReciD =  " & LstExpiry.Columns("RECID").Text.Trim & " and  PrQDRecId =  " & LstExpiry.Columns("E_RECID").Text.Trim & " and (isnull(AccQty,0) <> 0 OR isnull(RjQty,0) <> 0) ").Count = 0 Then
                            MessageBox.Show("Quantity Can Not be 0.", cntTitle, MessageBoxButtons.OK, MessageBoxIcon.Exclamation)
                            e.Cancel = True
                            LstExpiry.Col = LstExpiry.Splits(0).DisplayColumns.IndexOf(LstExpiry.Columns("MOD_OrdQty"))

                        End If
                    Else
                        MessageBox.Show("Quantity Can Not be 0.", cntTitle, MessageBoxButtons.OK, MessageBoxIcon.Exclamation)
                        e.Cancel = True
                        LstExpiry.Col = LstExpiry.Splits(0).DisplayColumns.IndexOf(LstExpiry.Columns("MOD_OrdQty"))
                    End If

                ElseIf LstExpiry.Columns("DOB_BatchInfo").Text.Trim = "" Then
                    MessageBox.Show("Lot No. Can Not be Null.", cntTitle, MessageBoxButtons.OK, MessageBoxIcon.Exclamation)
                    e.Cancel = True
                    LstExpiry.Col = LstExpiry.Splits(0).DisplayColumns.IndexOf(LstExpiry.Columns("DOB_BatchInfo"))
                ElseIf LstExpiry.Columns("DOB_ExpiryDate").Text.Trim = "" Then
                    MessageBox.Show("Expiry Date Can Not be Null.", cntTitle, MessageBoxButtons.OK, MessageBoxIcon.Exclamation)
                    e.Cancel = True
                    LstExpiry.Col = LstExpiry.Splits(0).DisplayColumns.IndexOf(LstExpiry.Columns("DOB_ExpiryDate"))
                End If

                ColorExpiryError = False


                If LstExpiry.Columns("DOB_BatchInfo").Text.Trim = "" Then
                    LstExpiry.EditActive = False
                    Me.BindingContext(dsDetExpiry.Tables("tblDetails").DefaultView).CancelCurrentEdit()
                    dsDetExpiry.Tables("tblDetails").AcceptChanges()
                    Exit Sub
                End If
                If LstExpiry.Columns("DOB_ExpiryDate").Text.Trim = "" OrElse LstExpiry.Columns("DOB_ExpiryDate").Text = "__.__.____" _
                    OrElse LstExpiry.Columns("DOB_BatchInfo").Text.Trim = "" Then
                    ErrorProvider1.SetError(LstExpiry, "The Expiry Date And - No Can't Be Null.")
                    e.Cancel = True
                    LstExpiry.Focus()
                    ColorExpiryError = True
                Else
                    ErrorProvider1.SetError(LstExpiry, "")
                End If

            End If

            'test if qty is biggegr than barcode number 
            If LstExpiry.Columns("E_REcid").Text.Trim <> "" Then
                strsql = " SELECT  bar.recid , COUNT(*) Counter FROM   " & TmpGridExpiry & " expiry"
                strsql &= " LEFT JOIN " & TmpGridBarcode & " bar ON bar.E_RECID=expiry.E_RECID"
                strsql &= " where  expiry.recid=" & txtRecid.Text.Trim & " and expiry.e_recid=" & LstExpiry.Columns("E_REcid").Text.Trim
                strsql &= " and bar_stat<>'D' GROUP BY bar.recid  "

                ReadFromDBToDataReader(strsql, drsql)
                If drsql.HasRows Then
                    While drsql.Read
                        If Val(drsql.Item("Counter").ToString) > Val(LstExpiry.Columns("MOD_OrdQty").Text.Trim) Then
                            MessageBox.Show("Barcode Number Is Bigger than Quantity Per Item", cntTitle, MessageBoxButtons.OK, MessageBoxIcon.Exclamation)
                            e.Cancel = True

                            Exit Sub
                        End If

                    End While
                End If
                drsql = Nothing
            End If
            ColorExpiryError = False
        Catch ex As Exception
            MessageBox.Show("Error : " & ex.Message, cntTitle, MessageBoxButtons.OK, MessageBoxIcon.Error)
        End Try
    End Sub

    Private Sub LstExpiry_KeyPress(ByVal sender As Object, ByVal e As System.Windows.Forms.KeyPressEventArgs) Handles LstExpiry.KeyPress
        Dim txt As New TextBox

        Try

            If Wtype = "DELETE" Or Wtype = "" Then Exit Sub
            If txtSTkID.Text.Trim = "" Then Exit Sub

            With LstExpiry
                'If .FilterActive = False AndAlso (.Col = .Splits(0).DisplayColumns.IndexOf(.Columns("DOB_BatchInfo")) OrElse _
                '.Col = .Splits(0).DisplayColumns.IndexOf(.Columns("DOB_ExpiryDate"))) AndAlso .Splits(0).DisplayColumns(.Col).Locked = False Then
                txt.Text = .Columns(.Col).Text
                If .EditActive = False Or .FilterActive = True Then
                    txt.Select(0, txt.Text.Length)
                Else
                    txt.Select(.SelectionStart, .SelectionLength)
                End If
                Select Case .Col
                    Case .Splits(0).DisplayColumns.IndexOf(.Columns("DOB_BatchInfo"))
                        If (Microsoft.VisualBasic.Asc(e.KeyChar) = 32) Then
                            e.Handled = True
                            Exit Try
                        End If
                        e.Handled = ValidGet(txt, e.KeyChar, StringType.Other, 15)
                    Case .Splits(0).DisplayColumns.IndexOf(.Columns("DOB_ExpiryDate"))
                        SendKeys.Send("{F2}")
                    Case .Splits(0).DisplayColumns.IndexOf(.Columns("DOQ_ProductionDt"))
                        SendKeys.Send("{F2}")
                    Case .Splits(0).DisplayColumns.IndexOf(.Columns("MOD_ORDQTY"))
                        ' e.Handled = ValidGet(txt, e.KeyChar, StringType.Numeric, 18, CShort(gbDecNo))
                        If Asc(e.KeyChar) = 22 Then
                            e.Handled = True : ctMenuPast(Me, "Long")
                        Else
                            e.Handled = ValidGet(txt, e.KeyChar, StringType.Numeric, 15, CShort(gbDecNo), True)
                        End If
                End Select
                ' End If
            End With
        Catch exp As Exception

            MessageBox.Show("Error : " & exp.Message, cntTitle,
            MessageBoxButtons.OK, MessageBoxIcon.Error)
        Finally
            txt = Nothing
        End Try
    End Sub
    Private Sub LstBarcode_BeforeColUpdate(ByVal sender As Object, ByVal e As C1.Win.C1TrueDBGrid.BeforeColUpdateEventArgs) Handles LstBarcode.BeforeColUpdate
        Dim drs() As DataRow
        Dim strsql As String
        Dim bExist As Boolean
        Try


            'Check if length of barcode is greater than 20 characters by using copy paste
            If UCase(txtItmClass.Text.Trim) = "F" AndAlso LstBarcode.Columns("Tmp_Size" & e.ColIndex - 1 & "").Text.Length > 20 Then
                MessageBox.Show("Barcode Should Be Less Than 20 Characters", cntTitle, MessageBoxButtons.OK, MessageBoxIcon.Exclamation)
                e.Cancel = True
                Exit Sub
            ElseIf (UCase(txtItmClass.Text.Trim) = "T" Or UCase(txtItmClass.Text.Trim) = "B" Or UCase(txtItmClass.Text.Trim) = "E") _
                    AndAlso LstBarcode.Columns("DOB_barcode").Text.Length > 20 Then
                MessageBox.Show("Barcode Should Be Less Than 20 Characters", cntTitle, MessageBoxButtons.OK, MessageBoxIcon.Exclamation)
                e.Cancel = True
                Exit Sub
            End If

            'Don't allow the user to enter same barcode twice
            drs = dsDetBarcode.Tables("tblDetails").Select("[DOB_Barcode] = '" & LstBarcode.Columns("DOB_Barcode").Text.Trim.Replace("'", "''") & "'  and [B_RECID] <> " & Val(LstBarcode.Columns("B_RECID").Text.Trim.Replace("'", "''")) & "")
            If drs.Length > 0 Then
                MessageBox.Show("Barcode Already Exist", cntTitle, MessageBoxButtons.OK, MessageBoxIcon.Exclamation)
                e.Cancel = True
                If Me.BindingContext(dsDetBarcode.Tables("tblDetails").DefaultView).Count > 0 Then
                    If UCase(txtItmClass.Text.Trim) = "T" Or UCase(txtItmClass.Text.Trim) = "B" Or UCase(txtItmClass.Text.Trim) = "E" Then
                        LstBarcode.Columns("DOB_barcode").Text = ""
                    End If
                End If
                Exit Sub
            End If
            If LstBarcode.Columns("DOB_Barcode").Text.Trim = "" Then Exit Sub
            strsql = " Select dbar_stkid,dbar_barcode as [Barcode] FROM Stk04_DetBarcode"
            strsql &= " WHERE  "
            strsql &= "  dbar_barcode ='" & LstBarcode.Columns("DOB_Barcode").Text.Trim.Replace("'", "''") & "'"

            If RecordsExists(strsql) = False Then
                ''test if zis barcodes  exists for another item 
                strsql = " Select * FROM " & TmpGridBarcode
                strsql &= " WHERE  bar_stat<>'D' and  DOB_Barcode ='" & LstBarcode.Columns("DOB_Barcode").Text.Trim.Replace("'", "''") & "'"
                strsql &= " and [B_RECID] <> " & Val(LstBarcode.Columns("B_RECID").Text.Trim.Replace("'", "''")) & ""
                If RecordsExists(strsql) = True Then
                    MessageBox.Show("This Barcode Already Exist For Another Item.", cntTitle, MessageBoxButtons.OK, MessageBoxIcon.Exclamation)
                    e.Cancel = True
                    Exit Sub
                End If
                LstBarcode.Columns("bar_stat").Text = "N"
            Else
                LstBarcode.Columns("bar_stat").Text = "O"

                strsql = " Select dbar_stkid,dbar_barcode as [Barcode] FROM Stk04_DetBarcode"
                strsql &= " WHERE  dbar_stkid<>" & txtSTkID.Text.Trim
                strsql &= "  and dbar_barcode ='" & LstBarcode.Columns("DOB_Barcode").Text.Trim.Replace("'", "''") & "'"
                If RecordsExists(strsql) = True Then
                    MessageBox.Show("This Barcode Already Exist For Another Item.", cntTitle, MessageBoxButtons.OK, MessageBoxIcon.Exclamation)
                    e.Cancel = True
                    Exit Sub
                End If
            End If

            'drs = dsDetBarcode.Tables("tblDetails").Select("recid='" & txtRecid.Text.Trim & "' AND bar_stat<>'D'")

            'If UCase(txtItmClass.Text.Trim) = "E" Then

            '    If drs.Length >= Val(LstExpiry.Columns("QTY").Text.Trim) Then
            '        MessageBox.Show("Barcode Number Can't Be Bigger Than Expiry Quantity.", cntTitle, MessageBoxButtons.OK, MessageBoxIcon.Exclamation)
            '        'LstBarcode.Columns(LstBarcode.Col).Text = ""
            '        e.Cancel = True
            '        Exit Sub
            '        'LstExpiry.Col = LstExpiry.Splits(0).DisplayColumns.IndexOf(LstExpiry.Columns("QTY"))
            '        ''Me.BindingContext(dsDetBarcode.Tables("tblDetails").DefaultView).CancelCurrentEdit() 
            '    End If
            'Else
            '    If drs.Length >= Val(txtQty.Text.Trim) Then
            '        MessageBox.Show("Barcode Number Can't Be Bigger Than Quantity.", cntTitle, MessageBoxButtons.OK, MessageBoxIcon.Exclamation)
            '        If Val(txtQty.Text.Trim) = 0 Then
            '            txtQty.Focus()
            '        End If
            '        'LstBarcode.Columns(LstBarcode.Col).Text = ""
            '        e.Cancel = True
            '        Exit Sub
            '    End If
            'End If
            'Check if barcode is EAN 13 or UPCA
            If IsNumeric(LstBarcode.Columns("DOB_Barcode").Text) Then
                If CheckEAN13((LstBarcode.Columns("DOB_Barcode").Text)) = False Or _
                    CheckUPCA(LstBarcode.Columns("DOB_Barcode").Text) = False Then
                    ShowNotification(Me, "Invalid Barcode", 3000, 12, DevComponents.DotNetBar.eToastPosition.MiddleCenter)
                    'MessageBox.Show("Invalid Barcode", cntTitle, MessageBoxButtons.OK, MessageBoxIcon.Exclamation)
                    'e.Cancel = True
                    'Exit Sub
                End If
            End If
            LstBarcode.Columns("DOB_Barcode").Text = LstBarcode.Columns("DOB_Barcode").Text.Trim


        Catch ex As Exception
            MessageBox.Show("Error : " & ex.Message, cntTitle, _
            MessageBoxButtons.OK, MessageBoxIcon.Error)
        End Try
    End Sub
    Private Sub LstBarcode_KeyPress(ByVal sender As Object, ByVal e As System.Windows.Forms.KeyPressEventArgs) Handles LstBarcode.KeyPress

        Dim txt As New TextBox
        Dim drs As DataRow()
        Try
            'drs = dsDetBarcode.Tables("tblDetails").Select("recid='" & txtRecid.Text.Trim & "' AND bar_stat<>'D'")
            'If UCase(txtItmClass.Text.Trim) = "E" Then
            '    If drs.Length >= Val(LstExpiry.Columns("QTY").Text.Trim) Then
            '        MessageBox.Show("Barcode Number Can't Be Bigger Than Expiry Quantity.", cntTitle, MessageBoxButtons.OK, MessageBoxIcon.Exclamation)
            '        'e.cancel = True
            '        'LstBarcode.Focus()
            '        e.Handled = True

            '        '  LstBarcode.Row = LstBarcode.Row - 1
            '        Exit Sub
            '    End If
            'Else
            '    If drs.Length >= Val(txtQty.Text.Trim) Then
            '        MessageBox.Show("Barcode Number Can't Be Bigger Than Quantity.", cntTitle, MessageBoxButtons.OK, MessageBoxIcon.Exclamation)
            '        If Val(txtQty.Text.Trim) = 0 Then
            '            txtQty.Focus()
            '        End If
            '        e.Handled = True
            '        '   LstBarcode.Row = LstBarcode.Row - 1
            '        Exit Sub
            '    End If
            'End If
            If txtItmClass.Text.Trim = "E" Then
                If Val(LstExpiry.Columns("MOD_OrdQty").Text.Trim) <= 0 Then
                    LstExpiry.Focus()
                    Exit Sub
                End If
            End If
            ErrorProvider1.SetError(CType(sender, C1TrueDBGrid), "")

            If dsDetBarcode.Tables.Count <> 0 Then
                If dsDetBarcode.Tables("tblDetails").Rows.Count = 0 Then Exit Sub
            End If

            With LstBarcode
                If .Splits(0).DisplayColumns(.Col).DataColumn.Text = "" Then Exit Sub
                If .FilterActive = False AndAlso .Splits(0).DisplayColumns(.Col).Locked = False Then

                    txt.Text = .Columns(.Col).Text

                    If .EditActive = False Then
                        txt.Select(0, txt.Text.Length)
                    Else
                        txt.Select(.SelectionStart, .SelectionLength)
                    End If

                    e.Handled = ValidGet(txt, e.KeyChar, StringType.Other, 20)

                End If
            End With
        Catch ex As Exception
            MessageBox.Show("Error : " & ex.Message, cntTitle, _
            MessageBoxButtons.OK, MessageBoxIcon.Error)
        Finally
            txt = Nothing
        End Try
    End Sub
    Private Sub LstBarcode_BeforeColEdit(ByVal sender As Object, ByVal e As C1.Win.C1TrueDBGrid.BeforeColEditEventArgs) Handles LstBarcode.BeforeColEdit
        Dim drs() As DataRow
        Try
            If LstBarcode.FilterActive = True Then Exit Sub
            If txtSTkID.Text.Trim = "" Then e.Cancel = True : Exit Sub ' Old = True : 
            If LstBarcode.FilterActive = True Then Exit Sub
            If txtSTkID.Text.Trim = "" Then
                MessageBox.Show("You Should Choose The Item Before.", cntTitle, MessageBoxButtons.OK, MessageBoxIcon.Exclamation)
                e.Cancel = True
                lstDelivery.Col = lstDelivery.Splits(0).DisplayColumns.IndexOf(lstDelivery.Columns("MOD_supitmref"))
                Exit Sub
            End If
            'drs = dsDetBarcode.Tables("tblDetails").Select("recid='" & txtRecid.Text.Trim & "' AND bar_stat<>'D'")

            If UCase(txtItmClass.Text.Trim) = "T" Or UCase(txtItmClass.Text.Trim) = "B" Then
                If Val(txtQty.Text.Trim) = 0 Then
                    MessageBox.Show("Barcode Number Can't Be Bigger Than Expiry Quantity.", cntTitle, MessageBoxButtons.OK, MessageBoxIcon.Exclamation)
                    e.Cancel = True
                    Me.BindingContext(dsDetBarcode.Tables("tblDetails").DefaultView).CancelCurrentEdit()
                    txtQty.Focus()
                    Exit Sub
                End If
            End If

            '    If drs.Length >= Val(LstExpiry.Columns("QTY").Text.Trim) Then
            '        MessageBox.Show("Barcode Number Can't Be Bigger Than Expiry Quantity.", cntTitle, MessageBoxButtons.OK, MessageBoxIcon.Exclamation)
            '        e.Cancel = True
            '        LstExpiry.Col = LstExpiry.Splits(0).DisplayColumns.IndexOf(lstDelivery.Columns("QTY"))
            '        'Me.BindingContext(dsDetBarcode.Tables("tblDetails").DefaultView).CancelCurrentEdit()
            '        Exit Sub
            '    End If
            'Else
            '    If drs.Length >= Val(txtQty.Text.Trim) Then
            '        MessageBox.Show("Barcode Number Can't Be Bigger Than Quantity.", cntTitle, MessageBoxButtons.OK, MessageBoxIcon.Exclamation)
            '        If Val(txtQty.Text.Trim) = 0 Then
            '            txtQty.Focus()
            '        End If
            '        'LstBarcode.Columns(LstBarcode.Col).Text = ""
            '        e.Cancel = True
            '        Exit Sub
            '    End If
            'End If
        Catch ex As Exception
            MessageBox.Show("Error : " & ex.Message, cntTitle, _
            MessageBoxButtons.OK, MessageBoxIcon.Error)
        End Try
    End Sub
    Private Sub LstExpiry_FetchRowStyle(ByVal sender As Object, ByVal e As C1.Win.C1TrueDBGrid.FetchRowStyleEventArgs) Handles LstExpiry.FetchRowStyle
        Try


            If UCase(LstExpiry.Columns("exp_stat").CellValue(e.Row).ToString) = "N" Then
                e.CellStyle.ForeColor = System.Drawing.Color.Red
            Else
                e.CellStyle.ForeColor = System.Drawing.Color.Navy
            End If

        Catch ex As Exception
            MessageBox.Show("Error : " & ex.Message, cntTitle, _
            MessageBoxButtons.OK, MessageBoxIcon.Error)
        End Try
    End Sub

    Private Sub LstBarcode_BeforeRowColChange(ByVal sender As Object, ByVal e As C1.Win.C1TrueDBGrid.CancelEventArgs) Handles LstBarcode.BeforeRowColChange
        Dim drs As DataRow()
        Try
            If LstBarcode.Columns("DOB_Barcode").Text.Trim = "" Then
                LstBarcode.EditActive = False
                Me.BindingContext(dsDetBarcode.Tables("tblDetails").DefaultView).CancelCurrentEdit()
                dsDetBarcode.Tables("tblDetails").AcceptChanges()
                Exit Sub
            End If
            'drs = dsDetBarcode.Tables("tblDetails").Select("recid='" & txtRecid.Text.Trim & "' AND bar_stat<>'D'")

            'If UCase(txtItmClass.Text.Trim) = "E" Then

            '    If drs.Length >= Val(LstExpiry.Columns("QTY").Text.Trim) Then
            '        '  MessageBox.Show("Barcode Number Can't Be Bigger Than Expiry Quantity.", cntTitle, MessageBoxButtons.OK, MessageBoxIcon.Exclamation)
            '        e.Cancel = True
            '        Me.BindingContext(dsDetBarcode.Tables("tblDetails").DefaultView).CancelCurrentEdit()
            '        Exit Sub
            '    End If
            'Else
            '    If drs.Length >= Val(txtQty.Text.Trim) Then
            '        '  MessageBox.Show("Barcode Number Can't Be Bigger Than Quantity.", cntTitle, MessageBoxButtons.OK, MessageBoxIcon.Exclamation)
            '        e.Cancel = True
            '        Me.BindingContext(dsDetBarcode.Tables("tblDetails").DefaultView).CancelCurrentEdit()
            '        Exit Sub
            '    End If
            'End If
        Catch ex As Exception
            MessageBox.Show("Error : " & ex.Message, cntTitle, MessageBoxButtons.OK, MessageBoxIcon.Error)
        End Try
    End Sub
    Private Sub LstBarcode_FetchRowStyle(ByVal sender As Object, ByVal e As C1.Win.C1TrueDBGrid.FetchRowStyleEventArgs) Handles LstBarcode.FetchRowStyle

        Try
            If LstBarcode.Columns("bar_stat").CellValue(e.Row).ToString = "O" Then
                e.CellStyle.ForeColor = System.Drawing.Color.Navy
            Else
                e.CellStyle.ForeColor = System.Drawing.Color.Red
            End If

        Catch ex As Exception
            MessageBox.Show("Error : " & ex.Message, cntTitle, _
            MessageBoxButtons.OK, MessageBoxIcon.Error)
        End Try
    End Sub

    Private Sub LstBarcode_Error(ByVal sender As Object, ByVal e As C1.Win.C1TrueDBGrid.ErrorEventArgs) Handles LstBarcode.Error
        e.Handled = True
    End Sub

    Private Sub LstBarcode_BeforeUpdate(ByVal sender As Object, ByVal e As C1.Win.C1TrueDBGrid.CancelEventArgs) Handles LstBarcode.BeforeUpdate
        Try
            If LstBarcode.Columns(LstBarcode.Col).Text.Trim <> "" Then
                Me.BindingContext(dsDetBarcode.Tables("tblDetails").DefaultView).EndCurrentEdit()
            End If

        Catch ex As Exception
            MessageBox.Show("Error: " & ex.Message, cntTitle, MessageBoxButtons.OK, MessageBoxIcon.Error)
        End Try
    End Sub
    Private Sub LstExpiry_KeyDown(ByVal sender As Object, ByVal e As System.Windows.Forms.KeyEventArgs) Handles LstExpiry.KeyDown
        Dim tdb As C1TrueDBGrid


        Try
            If Wtype = "DELETE" Or Wtype = "" Then Exit Sub
            If txtSTkID.Text.Trim = "" Then Exit Sub

            If UCase(txtOldNew.Text) = "W" Then
                If e.Modifiers = Keys.Alt AndAlso e.KeyCode = Keys.Down Then
                    e.Handled = True
                End If
                Exit Sub
            End If


            tdb = CType(sender, C1TrueDBGrid)
            If e.KeyValue = 93 Then
                tdb.Controls(2).ContextMenu = NullConMenu
            End If
            If e.Modifiers = Keys.Alt AndAlso e.KeyCode = Keys.Down Then
                e.Handled = True
                ItemExpiry(txtSTkID.Text.Trim)
            End If
        Catch ex As Exception
            MessageBox.Show("Error : " & ex.Message, cntTitle, _
                    MessageBoxButtons.OK, MessageBoxIcon.Error)
        End Try
    End Sub
    Private Sub LstBarcode_AfterUpdate(ByVal sender As Object, ByVal e As System.EventArgs) Handles LstBarcode.AfterUpdate
        Try

            daDetBarcode.Update(dsDetBarcode.Tables("tblDetails"))
        Catch ex As Exception
            MessageBox.Show("Error: " & ex.Message, cntTitle, MessageBoxButtons.OK, MessageBoxIcon.Error)
        End Try
    End Sub
    Private Sub txtRef_KeyDown(ByVal sender As Object, ByVal e As System.Windows.Forms.KeyEventArgs) Handles txtConsQty.KeyDown, txtPrice.KeyDown, _
        txtRef.KeyDown, txtSupRef.KeyDown, txtSupColor.KeyDown, txtGender.KeyDown, txtQty.KeyDown, txtDesc.KeyDown, txtSUBUnit.KeyDown, _
        txtStatus.KeyDown, txtProdGroup.KeyDown, txtStyle.KeyDown, txtColor.KeyDown, LstExpiry.KeyDown, LstBarcode.KeyDown, lstFashQty.KeyDown, TxtDisc.KeyDown


        Dim dPrice, dQty As Double
        Dim bExists As Boolean

        Try
            If e.KeyCode = Keys.F6 Or e.KeyCode = Keys.F9 Or e.KeyCode = Keys.F12 Or e.KeyCode = Keys.Escape Then Exit Sub

      
            If txtSupp.Text.Trim = "" Then
                MessageBox.Show("You Should Choose a Supplier.", cntTitle, MessageBoxButtons.OK, MessageBoxIcon.Exclamation)
                ' ReadonlyConsFash()
                txtRef.Text = "" : txtSupRef.Text = "" : txtSupColor.Text = ""
                txtSupp.Focus()
                Exit Sub
            ElseIf txtDate.Value Is DBNull.Value Then
                MessageBox.Show("Date Can Not Be Null.", cntTitle, MessageBoxButtons.OK, MessageBoxIcon.Exclamation)
                'ReadonlyConsFash()
                txtRef.Text = "" : txtSupRef.Text = "" : txtSupColor.Text = ""
                txtDate.Focus()
                Exit Sub
            ElseIf txtDelOrder.Text.Trim = "" Then
                MessageBox.Show("You Should enter A Receiving Order Ref.", cntTitle, MessageBoxButtons.OK, MessageBoxIcon.Exclamation)
                ' ReadonlyConsFash()
                txtRef.Text = "" : txtSupRef.Text = "" : txtSupColor.Text = ""
                txtDelOrder.Focus()
                Exit Sub


            ElseIf txtLoc.Text.Trim = "" Then
                MessageBox.Show("You Should Pick A Location.", cntTitle, MessageBoxButtons.OK, MessageBoxIcon.Exclamation)
                'ReadonlyConsFash()
                txtRef.Text = "" : txtSupRef.Text = "" : txtSupColor.Text = ""
                txtLoc.Focus()
                Exit Sub
            End If
            If DelOrder = "Consignment Receiving Order" Or DelOrder = "Consignment Receiving Order Fashion" Then
                If txtCurr.Text.Trim = "" Then
                    MessageBox.Show("Currency Can Not Be Null.", cntTitle, MessageBoxButtons.OK, MessageBoxIcon.Exclamation)

                    txtRef.Text = "" : txtSupRef.Text = "" : txtSupColor.Text = ""
                    txtCurr.Focus()
                    Exit Sub
                End If
            End If
            If DelOrder = "Receiving Order Fashion" Or DelOrder = "Consignment Receiving Order Fashion" Then

                If txtBrand.Text.Trim = "" Then
                    MessageBox.Show("You Should Pick A Brand.", cntTitle, MessageBoxButtons.OK, MessageBoxIcon.Exclamation)
                    ' ReadonlyConsFash()
                    txtRef.Text = "" : txtSupRef.Text = "" : txtSupColor.Text = ""
                    txtBrand.Focus()
                    Exit Sub
                ElseIf txtSeason.Text.Trim = "" Then
                    MessageBox.Show("You Should Pick A Season.", cntTitle, MessageBoxButtons.OK, MessageBoxIcon.Exclamation)
                    '  ReadonlyConsFash()
                    txtRef.Text = "" : txtSupRef.Text = "" : txtSupColor.Text = ""
                    txtSeason.Focus()
                    Exit Sub
                End If
            End If
            ReadonlyConsFash()

            If e.KeyCode = Keys.Enter Then
                If Wtype = "" Or Wtype = "DELETE" Then Exit Sub

                'If txtOldNew.Text = "w" Then Exit Sub
                'If UCase(lstDelivery.Columns("new_old").Text.Trim) = "O" Or UCase(lstDelivery.Columns("new_old").Text.Trim) = "W" Then Exit Sub
                If txtSTkID.Text.Trim = "" Then Exit Sub
                If txtRef.Text.Trim = "" Then txtRef.Focus() : Exit Sub

                If TxtDisc.Text.Trim <> "" AndAlso IsNumeric(TxtDisc.Text.Trim) Then
                    If CDbl(TxtDisc.Text.Trim) < 0 Then
                        TxtDisc.Focus()

                        MessageBox.Show("Invalid Discount ", cntTitle, _
                   MessageBoxButtons.OK, MessageBoxIcon.Exclamation)
                        Exit Sub
                    ElseIf CDbl(TxtDisc.Text.Trim) > 100 Then
                        TxtDisc.Focus()
                        MessageBox.Show("Discount Should Be Less Than 100 %", cntTitle, _
                 MessageBoxButtons.OK, MessageBoxIcon.Exclamation)
                        Exit Sub
                    End If
                End If
                If sender Is lstFashQty Then
                    lstFashQty.UpdateData()
                End If
                If sender Is LstBarcode Or sender Is LstExpiry Then
                    LstBarcode.UpdateData()
                    LstExpiry.UpdateData()
                    daDetExpiry.Update(dsDetExpiry.Tables("tblDetails"))
                ElseIf sender Is txtRef Then
                    txtSupRef_Validating(txtRef, New System.ComponentModel.CancelEventArgs)
                End If

                If DelOrder = "Receiving Order" Or DelOrder = "Consignment Receiving Order" Then
                    Dim drs As DataRow()
                    drs = dsDetBarcode.Tables("tblDetails").Select("recid='" & txtRecid.Text.Trim & "' AND bar_stat<>'D'")

                    If UCase(txtItmClass.Text.Trim) = "E" Then
                        If drs.Length > Val(LstExpiry.Columns("QTY").Text.Trim) Then
                            MessageBox.Show("Barcode Number Can't Be Bigger Than Expiry Quantity.", cntTitle, MessageBoxButtons.OK, MessageBoxIcon.Exclamation)
                            LstExpiry.Columns("QTY").Text = CStr(drs.Length)

                            Exit Sub
                        End If
                        'ch @ 08/06/2016
                        If Val(LstExpiry.Columns("DOB_Expirydate").Text.Trim) = 0 Then
                            MessageBox.Show("Expiry Date Can't Be Null.", cntTitle, MessageBoxButtons.OK, MessageBoxIcon.Exclamation)
                            If GrpExpBarcode.Visible = False Then GrpExpBarcode.Visible = True
                            LstExpiry.Focus()
                            Exit Sub
                        End If
                        'ch @ 08/06/2016
                        If LstExpiry.Columns("DOB_BatchInfo").Text.Trim = "" Then
                            MessageBox.Show("Lot No Can't Be Null.", cntTitle, MessageBoxButtons.OK, MessageBoxIcon.Exclamation)
                            If GrpExpBarcode.Visible = False Then GrpExpBarcode.Visible = True
                            LstExpiry.Focus()
                            Exit Sub
                        End If
                        If Val(LstExpiry.Columns("QTY").Text.Trim) = 0 Then
                            MessageBox.Show("Quantity Can't Be Null.", cntTitle, MessageBoxButtons.OK, MessageBoxIcon.Exclamation)
                            Exit Sub
                        End If
                    Else
                        If drs.Length > Val(txtQty.Text.Trim) Then
                            MessageBox.Show("Barcode Number Can't Be Bigger Than Quantity.", cntTitle, MessageBoxButtons.OK, MessageBoxIcon.Exclamation)
                            If Val(txtQty.Text.Trim) = 0 Then
                                txtQty.Focus()
                            End If
                            If Not DtQualityControlHeader Is Nothing AndAlso txtRecid.Text.Trim <> "" Then
                                If DtQualityControlHeader.Select("PrDReciD =  " & txtRecid.Text & " and isnull(AccQty,0) <> 0 and isnull(RjQty,0) <> 0 ").Count <> 0 Then
                                    'e.Handled = True
                                    'MessageBox.Show("You Can Only Change Your Quantity From Accepted Quantity In Quantity Control Form", cntTitle, MessageBoxButtons.OK, MessageBoxIcon.Information)
                                    'Exit Try
                                    btnBarcode_Click(btnBarcode, New System.EventArgs)
                                Else
                                    txtQty.Text = CStr(drs.Length)
                                End If
                            Else
                                txtQty.Text = CStr(drs.Length)
                            End If


                            Exit Sub
                        End If
                    End If
                End If
                If UCase(txtItmClass.Text.Trim) = "E" And (sender Is LstExpiry Or sender Is LstBarcode) Then
                    Dim sFltr As String
                    Dim sTotal As Double
                    Dim objSum As Object
                    With LstExpiry
                        LstExpiry.UpdateData()
                        daDetExpiry.Update(dsDetExpiry.Tables("tblDetails"))
                        If Val(LstExpiry.Columns("MOD_OrdQty").Text.Trim) <= 0 Then
                            LstExpiry.Focus()

                            Exit Sub

                        End If
                        If LstExpiry.Col = LstExpiry.Splits(0).DisplayColumns.IndexOf(LstExpiry.Columns("MOD_OrdQty")) Then
                            objSum = dsDetExpiry.Tables("tblDetails").Compute("Sum(MOD_OrdQty)", sFltr)
                            If objSum Is DBNull.Value Then
                                sTotal = 0 : txtQty.Text = "0"
                            Else
                                sTotal = CDbl(CType(objSum, Double))
                                txtQty.Text = CStr(sTotal)

                                'If UCase(txtStatus.Text.Trim) = "R" Or UCase(txtStatus.Text.Trim) = "RETURN" Then
                                '    txtQty.Text = CStr(Val(txtQty.Text.Trim) * (-1))
                                'End If
                                'update consolidated QTy
                                If UCase(lstDelivery.Columns("New_old").Text.Trim) = "W" Then

                                    Dim Str As String
                                    Str = " Select case Meas_Sign WHEN '*' THEN case when pn2.sfp_operator ='*' then " & Val(txtQty.Text.Trim) & " *  pn2.SFP_UNITPACK   * STK04_Measurements.Meas_Value else " & Val(txtQty.Text.Trim) & " /  pn2.SFP_UNITPACK   * STK04_Measurements.Meas_Value end "
                                    Str &= " ELSE case when pn2.sfp_operator='*' then  (  SFP_UNITPACK   * " & Val(txtQty.Text.Trim) & " ) /   STK04_Measurements.Meas_Value else  ( " & Val(txtQty.Text.Trim) & " / SFP_UNITPACK ) /   STK04_Measurements.Meas_Value end  End [ctr]"
                                    Str &= " From " & TempPendingDO & "  pn2 "
                                    Str &= " left JOIN STK04_Measurements ON Meas_unit = pn2.SFP_SUBUNIT "
                                    Str &= " WHERE  pn2.SFP_Recid = " & Val(txtSTkID.Text.Trim) & " "

                                    Dim drSQL As SqlDataReader
                                    ReadFromDBToDataReader(Str, drSQL)
                                    If drSQL.HasRows Then
                                        drSQL.Read()
                                        txtConsQty.Text = Format(drSQL.Item("Ctr"), "#,##0.000").ToString()
                                    End If
                                Else
                                    strsql = " SELECT  CASE meas_sign"
                                    strsql &= " WHEN '*' THEN " & Val(txtQty.Text.Trim) & " * stk_unitpack * meas_value"
                                    strsql &= " ELSE ( stk_unitpack * " & Val(txtQty.Text.Trim) & " ) / Meas_value End as [Ctr] FROM V_Pord2_GetStkMeas WHERE   stk_recid = '" & txtSTkID.Text.Trim & "' "
                                    Dim drsql As SqlDataReader
                                    ReadFromDBToDataReader(strsql, drsql)
                                    If drsql.HasRows Then
                                        drsql.Read()
                                        txtConsQty.Text = Format(drsql.Item("Ctr"), "#,##0.000").ToString()
                                    End If
                                End If
                            End If
                        End If
                        Select Case UCase(LstExpiry.Columns(LstExpiry.Col).DataField)
                            Case "MOD_ORDQTY"
                                RefQtyLbl(False)
                        End Select
                        If LstExpiry.Columns("recid").Text.Trim = "" Then Exit Sub
                        If Val(.Columns("MOD_ORDQTY").Text.Trim) <> 0 And (.Columns("DOB_batchInfo").Text.Trim = "" Or .Columns("DOB_ExpiryDate").Text.Trim = "") Then
                            MessageBox.Show("Lot No Or Expiry Date Couldn't be Empty.", cntTitle, MessageBoxButtons.OK, MessageBoxIcon.Exclamation)
                            If .Columns("DOB_batchInfo").Text.Trim = "" Then
                                .Col = .Splits(0).DisplayColumns.IndexOf(LstExpiry.Columns("DOB_batchInfo"))
                            ElseIf .Columns("DOB_ExpiryDate").Text.Trim = "" Then
                                .Col = .Splits(0).DisplayColumns.IndexOf(LstExpiry.Columns("DOB_ExpiryDate"))
                            End If
                            .Focus()
                            Exit Sub

                            If Val(LstExpiry(.Row, "MOD_ORDQTY").ToString.Trim) = 0 And Val(.Columns("MOD_ORDQTY").Text.Trim) = 0 And UCase(lstDelivery.Columns("itmclass").Text.Trim) = "E" Then
                                MessageBox.Show("Quantity Couldn't be Empty.", cntTitle, MessageBoxButtons.OK, MessageBoxIcon.Exclamation)
                                .Focus()

                                Exit Sub
                            End If
                        End If
                    End With

                    If LstExpiry.Columns("DOB_BatchInfo").Text.Trim = "" Then
                        LstExpiry.EditActive = False
                        Me.BindingContext(dsDetExpiry.Tables("tblDetails").DefaultView).CancelCurrentEdit()
                        dsDetExpiry.Tables("tblDetails").AcceptChanges()
                        Exit Sub
                    End If
                End If
                NetCalculate()


                If Wtype <> "" Then
                    If txtSTkID.Text.Trim = "" Then
                        MessageBox.Show("Item Key Couldn't be Empty.", cntTitle, MessageBoxButtons.OK, MessageBoxIcon.Exclamation)
                        If txtSupRef.Text.Trim = "" Then
                            txtSupRef.Focus()
                        Else
                            txtRef.Focus()
                        End If
                        Exit Sub
                    End If


                    If DelOrder = "Receiving Order Fashion" Or DelOrder = "Consignment Receiving Order Fashion" Then
                        'text on valid color 

                        'If Not RecordsExists(ColorsStrsql("Color_Code = '" & txtColor.Text.Trim.Replace("'", "''") & "'", False)) Then
                        '    MessageBox.Show("Invalid Color", cntTitle, MessageBoxButtons.OK, MessageBoxIcon.Exclamation)
                        '    txtColor.Text = ""
                        '    txtColor.Focus()
                        '    Exit Sub
                        'End If
                        If txtColor.Text.Trim = "" Then
                            MessageBox.Show("Color Couldn't be Null.", cntTitle, MessageBoxButtons.OK, MessageBoxIcon.Exclamation)
                            txtColor.Focus()
                            Exit Sub
                        ElseIf txtSupColor.Text.Trim = "" Then
                            MessageBox.Show("Supp. Color Couldn't be Null.", cntTitle, MessageBoxButtons.OK, MessageBoxIcon.Exclamation)
                            txtSupColor.Focus()
                            Exit Sub
                        End If
                    End If

                End If
                If Not DtQualityControlHeader Is Nothing AndAlso txtRecid.Text.Trim <> "" Then
                    If DtQualityControlHeader.Select("PrDReciD =  " & txtRecid.Text & " and (isnull(AccQty,0) <> 0 or isnull(RjQty,0) <> 0) ").Count = 0 Then
                        If Val(txtQty.Text.Trim) = 0 Then
                            MessageBox.Show("Quantity Couldn't be Null.", cntTitle, MessageBoxButtons.OK, MessageBoxIcon.Exclamation)
                            If UCase(txtItmClass.Text.Trim) = "E" Then
                                LstExpiry.Focus()
                            ElseIf UCase(txtItmClass.Text.Trim) = "F" Then
                                lstFashQty.Focus()
                            Else
                                txtQty.Focus()
                            End If
                            Exit Sub
                        End If
                    End If

                Else
                    If Val(txtQty.Text.Trim) = 0 Then
                        MessageBox.Show("Quantity Couldn't be Null.", cntTitle, MessageBoxButtons.OK, MessageBoxIcon.Exclamation)
                        If UCase(txtItmClass.Text.Trim) = "E" Then
                            LstExpiry.Focus()
                        ElseIf UCase(txtItmClass.Text.Trim) = "F" Then
                            lstFashQty.Focus()
                        Else
                            txtQty.Focus()
                        End If
                        Exit Sub
                    End If
                End If


                'update consolidated QTy
                If UCase(GetFieldDesc(TmpGridDetails, "MOD_RMRecid=" & txtSTkID.Text.Trim & "", "new_old")) = "W" Then

                    Dim Str As String
                    Str = " Select case Meas_Sign WHEN '*' THEN case when pn2.sfp_operator='*' then  " & Val(txtQty.Text.Trim) & " *  pn2.SFP_UNITPACK   * STK04_Measurements.Meas_Value else  " & Val(txtQty.Text.Trim) & " /  pn2.SFP_UNITPACK   * STK04_Measurements.Meas_Value end "
                    Str &= " ELSE case when pn2.sfp_operator ='*' then  (  SFP_UNITPACK   * " & Val(txtQty.Text.Trim) & " ) /   STK04_Measurements.Meas_Value  else   (   " & Val(txtQty.Text.Trim) & "/ SFP_UNITPACK) /   STK04_Measurements.Meas_Value  end End [ctr]"
                    Str &= " From " & TempPendingDO & "  pn2 "
                    Str &= " left JOIN STK04_Measurements ON Meas_unit = pn2.SFP_SUBUNIT "
                    Str &= " WHERE  pn2.SFP_Recid = " & Val(txtSTkID.Text.Trim) & " "

                    Dim drSQL As SqlDataReader
                    ReadFromDBToDataReader(Str, drSQL)
                    If drSQL.HasRows Then
                        drSQL.Read()
                        txtConsQty.Text = Format(drSQL.Item("Ctr"), "#,##0.000").ToString()

                    End If
                Else
                    strsql = " SELECT  CASE meas_sign"
                    strsql &= " WHEN '*' THEN case when stk_operator='*' then " & Val(txtQty.Text.Trim) & " * stk_unitpack * meas_value else "
                    strsql &= "  (" & Val(txtQty.Text.Trim) & " / stk_unitpack ) * meas_value end "
                    strsql &= " ELSE case when stk_operator='*' then ( stk_unitpack * " & Val(txtQty.Text.Trim) & " ) / Meas_value else ( " & Val(txtQty.Text.Trim) & " / stk_unitpack  ) / Meas_value end  End as [Ctr] FROM V_Pord2_GetStkMeas WHERE   stk_recid = '" & txtSTkID.Text.Trim & "' "
                    Dim drsql As SqlDataReader
                    ReadFromDBToDataReader(strsql, drsql)
                    If drsql.HasRows Then
                        drsql.Read()
                        txtConsQty.Text = Format(drsql.Item("Ctr"), "#,##0.000").ToString()
                    End If
                End If

                UpdateLine()
                txtSupRef.Focus()
            ElseIf e.KeyCode = Keys.Escape Then

                INITCOL(True, True)
            Else
                If sender Is LstBarcode Then
                    Dim tdb As C1TrueDBGrid
                    Try
                        If Wtype = "DELETE" Or Wtype = "" Then Exit Sub
                        If txtSTkID.Text.Trim = "" Then Exit Sub

                        tdb = CType(sender, C1TrueDBGrid)
                        If e.KeyValue = 93 Then
                            tdb.Controls(2).ContextMenu = NullConMenu
                        End If
                        If e.Modifiers = Keys.Alt AndAlso e.KeyCode = Keys.Down Then
                            If UCase(txtOldNew.Text) = "W" Then Exit Sub
                            ItemBarcode(txtSTkID.Text.Trim, CLng(Val(LstBarcode(LstBarcode.Row, "B_RecId").ToString)))
                        End If
                    Catch ex As Exception
                        MessageBox.Show("Error : " & ex.Message, cntTitle, _
                                MessageBoxButtons.OK, MessageBoxIcon.Error)
                    End Try
                End If
            End If

        Catch ex As Exception
            MessageBox.Show(ex.Message, cntTitle & ": General Error.", _
            MessageBoxButtons.OK, MessageBoxIcon.Error)
        End Try
    End Sub


    Private Sub lstDelivery_RowColChange(ByVal sender As Object, ByVal e As C1.Win.C1TrueDBGrid.RowColChangeEventArgs) Handles lstDelivery.RowColChange
        Try
            If txtSTkID.Text.Trim <> "" Or e Is Nothing Then Exit Sub
            '  If UCase(lstDelivery.Columns(lstDelivery.Col).DataField) = "BARCODE" Then Exit Sub

            If UCase(lstDelivery.Columns("new_old").Text.Trim) = "O" OrElse UCase(lstDelivery.Columns("ItmClass").Text.Trim) = "F" OrElse UCase(lstDelivery.Columns("ItmClass").Text.Trim) = "E" Then
                lstDelivery.AllowUpdate = False
            Else
                lstDelivery.AllowUpdate = True
                lstDelivery.Splits(0).DisplayColumns("MOD_OrdQty").Locked = False
                If DelOrder = "Consignment Receiving Order Fashion" Or DelOrder = "Consignment Receiving Order" Then
                    lstDelivery.Splits(0).DisplayColumns("COST").Locked = False
                    lstDelivery.Splits(0).DisplayColumns("disc").Locked = False
                End If

            End If

            If e.LastRow <> lstDelivery.RowBookmark(lstDelivery.Row) Then

                If UCase(lstDelivery.Columns("new_old").Text.Trim) = "W" Then
                    btnCreate.Text = "Modify" & vbCrLf & "New Item" & vbCrLf & "Ctrl+ M"
                Else
                    btnCreate.Text = "Add" & vbCrLf & "New Item" & vbCrLf & "Ctrl+ M"
                End If

                If DelOrder = "Receiving Order Fashion" Or DelOrder = "Consignment Receiving Order Fashion" Then
                    If lstDelivery.Columns("Color").Text.Trim <> "" Then
                        lstDelivery.Columns("Color").FooterText = GetFieldDesc("Stk04_COLOR", " Color_Code='" & lstDelivery.Columns("Color").Text.Trim & "'", "Color_Reference")
                    Else
                        lstDelivery.Columns("Color").FooterText = ""
                    End If
                End If

                If lstDelivery.Columns("MOD_supitmref").Text.Trim = "" Then GrpExpBarcode.Visible = False : LstVisbility(False, False, lstDelivery.Columns("ITMCLASS").Text.Trim) : Exit Sub

                If DelOrder = "Receiving Order" Or DelOrder = "Consignment Receiving Order" Then
                    LstExpiry.AllowAddNew = False
                    LstExpiry.AllowDelete = False
                    LstExpiry.AllowUpdate = False

                    LstBarcode.AllowAddNew = False
                    LstBarcode.AllowDelete = False
                    LstBarcode.AllowUpdate = False

                    'R.A. 07/05/2014''''''''''''''''''''''''''''''''
                    GrpExpBarcode.Visible = False
                    'If UCase(lstDelivery.Columns("ITMCLASS").Text.Trim) = "E" Then
                    '    GetExpiry(lstDelivery.Columns("ITMCLASS").Text.Trim)
                    '    GetBarcode(lstDelivery.Columns("ITMCLASS").Text.Trim)
                    '    LstVisbility(True, True, lstDelivery.Columns("ITMCLASS").Text.Trim)
                    'Else
                    '    GetBarcode(lstDelivery.Columns("ITMCLASS").Text.Trim)
                    '    LstVisbility(False, True, lstDelivery.Columns("ITMCLASS").Text.Trim)
                    'End If
                Else
                    lstFashQty.AllowUpdate = False
                    GrpExpBarcode.Visible = False
                    RefGridSizes(lstFashQty.Columns("GridCode").Text.Trim)
                    'lstFashQty.Enabled = False
                End If

                If DelOrder = "Consignment Receiving Order Fashion" Or DelOrder = "Consignment Receiving Order" Then
                    '  PnlConsigment.Visible = True

                    txtPrice.ReadOnly = True
                    'txtCurr.Text = lstDelivery.Columns("STK_FCcode").Text.Trim
                    '    txtPrice.Text = lstDelivery.Columns("COST").Text.Trim
                    If txtPrice.Text.Trim <> "" Then
                        txtPrice.Text = CType(txtPrice.Text, Double).ToString("N3")
                    End If
                End If


            End If
        Catch ex As Exception
            MessageBox.Show("Error : " & ex.Message, cntTitle, _
            MessageBoxButtons.OK, MessageBoxIcon.Error)
        End Try
    End Sub

    Private Sub lstFashQty_BeforeColUpdate(ByVal sender As Object, ByVal e As C1.Win.C1TrueDBGrid.BeforeColUpdateEventArgs) Handles lstFashQty.BeforeColUpdate
        Dim txt As New TextBox
        Dim drsql As SqlDataReader
        Dim drs As Object
        Dim ItmQty As Object
        Dim strsql As String
        Try
            With lstFashQty
                If DelOrder = "Receiving Order Fashion" Or DelOrder = "Consignment Receiving Order Fashion" Then
                    If UCase(Mid(.Columns(.Col).DataField, 1, 4)) = "SIZE" Then

                        If lstFashQty.Columns(lstFashQty.Col).Text.Trim <> "" AndAlso CDbl(lstFashQty.Columns(lstFashQty.Col).Text.Trim) <> 0 Then

                            Using cnsql As New SqlConnection(ConnectionString)
                                cnsql.Open()
                                strsql = " SELECT ISNULL(SUM(" & e.Column.DataColumn.DataField & "),0) FROM " & TmpGridDetails & " t1"
                                strsql &= " INNER JOIN " & TmpFashQty & " t2 ON t1.recid = t2.recid"
                                strsql &= " WHERE DOB_Drecid IS null"
                                If txtStatus.Text.Trim.ToUpper = "R" Then
                                    strsql &= " AND MOD_stat = 'R'"
                                Else
                                    strsql &= " AND MOD_stat <> 'R'"
                                End If
                                strsql &= " AND MOD_RMRecid = '" & txtSTkID.Text.Trim & "'"
                                strsql &= " AND GridCode = '" & lstFashQty.Columns("GridCode").Text.Trim.Replace("'", "''") & "'"
                                strsql &= " AND color = '" & txtColor.Text.Trim.Replace("'", "''") & "'"
                                strsql &= " AND t2.recid <> " & txtRecid.Text.Trim & ""
                                Using cmsql As New SqlCommand(strsql, cnsql)
                                    ItmQty = cmsql.ExecuteScalar
                                End Using
                                cnsql.Close()
                            End Using

                            If ItmQty Is DBNull.Value Then
                                ItmQty = 0
                            Else
                                ItmQty = CDbl(ItmQty)
                            End If
                            ItmQty = (CDbl(ItmQty) + CDbl(lstFashQty.Columns(lstFashQty.Col).Text))
                            If QtyCntrEc(txtSTkID.Text.Trim, ItmQty.ToString, , , , lstFashQty.Columns("GridCode").Text, lstFashQty.Columns(lstFashQty.Col).ToString, txtColor.Text.Trim) Then
                                e.Cancel = True
                                Exit Try
                            End If

                        End If

                        Dim Qty As Double = 0
                        txt.Text = .Columns(.Col).Text
                        'If UCase(txtStatus.Text.Trim) = "R" Or UCase(txtStatus.Text.Trim) = "RETURN" Then
                        '    lstDelivery.Columns(.Col).Text = CStr(Val(txt.Text.Trim) * (-1))
                        'End If
                        If DecimalValid(15, CShort(gbDecNo), txt) = False Then
                            e.Cancel = True
                            MessageBox.Show("Quantity Of " & .Columns(.Col).DataField & " Length Is Too Big", cntTitle, MessageBoxButtons.OK, MessageBoxIcon.Warning)
                            Exit Try
                        End If
                        For j = 1 To MaxCnt

                            Qty += Val(.Columns("SIZE" & j).Text)
                        Next
                        txtQty.Text = CStr(Qty)
                        NetCalculate()
                        'ch @ 16/12/2016  test case when we have generate barcode to the old qty and the user enter a qty <  to the old (then we have to prompt a msg to inform the user that the barcodes will be  deleted in this case )
                        strsql = " update " & TmpGridBarcode & "F set Tmp_Stat='D'  where tmp_Drecid='" & .Columns("RECID").Text & "'"
                        For i = 1 To MaxCnt
                            If Not dsDetBarcodeF.Tables("tblDetails") Is Nothing AndAlso Val(.Columns("Size" & i).Text) < Val(dsDetBarcodeF.Tables("tblDetails").Select(" tmp_Drecid='" & .Columns("RECID").Text & "' and tmp_size" & i & " is not null  ").Length) Then
                                If MessageBox.Show("Existing Barcodes Linked To This Size, Will Be Deleted , Are You Sure You Want To Continue", cntTitle, MessageBoxButtons.YesNo, MessageBoxIcon.Question) = Windows.Forms.DialogResult.Yes Then
                                    Execute("update  top(" & Val(dsDetBarcodeF.Tables("tblDetails").Select(" tmp_Drecid='" & .Columns("RECID").Text & "' ").Length) - Val(.Columns("Size" & i).Text) & ")  " & _
                                            " table_1  set table_1.tmp_size" & i & "=NULL from  " & TmpGridBarcode & "F table_1 inner join (select   " & _
                                            " top(" & Val(dsDetBarcodeF.Tables("tblDetails").Select(" tmp_Drecid='" & .Columns("RECID").Text & "' ").Length) - Val(.Columns("Size" & i).Text) & ") tmp_recid From  " & TmpGridBarcode & "F  " & _
                                            "where tmp_Drecid='" & .Columns("RECID").Text & "' and Tmp_size" & i & " is not null AND TMP_STAT='N'  ORDER BY  Tmp_RecId desc ) Table_2 on  " & _
                                            "table_1.Tmp_Recid=table_2.tmp_Recid ")
                                Else
                                    e.Cancel = True
                                    Exit Try
                                End If
                            End If
                            strsql &= " and tmp_size" & i & " is null"
                        Next
                        Execute(strsql)
                        GetBarcodeF()
                        'update consolidated QTy
                        If UCase(GetFieldDesc(TmpGridDetails, "MOD_RMRecid=" & txtSTkID.Text.Trim & "", "new_old")) = "W" Then

                            Dim Str As String
                            Str = " Select case Meas_Sign WHEN '*' THEN case when pn2.sfp_operator='*' then  " & Val(txtQty.Text.Trim) & " *  pn2.SFP_UNITPACK   * STK04_Measurements.Meas_Value else  " & Val(txtQty.Text.Trim) & " /  pn2.SFP_UNITPACK   * STK04_Measurements.Meas_Value end "
                            Str &= " ELSE case when sfp_operator='*' then  (  SFP_UNITPACK   * " & Val(txtQty.Text.Trim) & " ) /   STK04_Measurements.Meas_Value else  (" & Val(txtQty.Text.Trim) & "/ SFP_UNITPACK ) /   STK04_Measurements.Meas_Value  End end  [ctr]"
                            Str &= " From " & TempPendingDO & "  pn2 "
                            Str &= " left JOIN STK04_Measurements ON Meas_unit = pn2.SFP_SUBUNIT "
                            Str &= " WHERE  pn2.SFP_Recid = " & Val(txtSTkID.Text.Trim) & " "

                            ReadFromDBToDataReader(Str, drsql)
                            If drsql.HasRows Then
                                drsql.Read()
                                txtConsQty.Text = Format(drsql.Item("Ctr"), "###0.000").ToString()
                                drsql = Nothing
                            End If
                        Else
                            strsql = " SELECT  CASE meas_sign"
                            strsql &= " WHEN '*' THEN " & Val(txtQty.Text.Trim) & " * stk_unitpack * meas_value"
                            strsql &= " ELSE ( stk_unitpack * " & Val(txtQty.Text.Trim) & " ) / Meas_value End as [Ctr] FROM V_Pord2_GetStkMeas WHERE   stk_recid = '" & txtSTkID.Text.Trim & "' "

                            ReadFromDBToDataReader(strsql, drsql)
                            If drsql.HasRows Then
                                drsql.Read()
                                txtConsQty.Text = Format(drsql.Item("Ctr"), "###0.000").ToString()
                                drsql = Nothing
                            End If
                        End If
                    End If
                End If
            End With


            'test if qty is bigger than barcode number 
            If dsDetBarcodeF.Tables.Count <> 0 Then
                'ch @ 23/12/2016 calculation wrong 
                'drs = dsDetBarcodeF.Tables("tblDetails").Compute("count(Tmp_DRecId)", "Tmp_Stat<>'D' and Tmp_DRecId='" & txtRecid.Text.Trim & "'  ")
                drs = dsDetBarcodeF.Tables("tblDetails").Compute("count(tmp_" & lstFashQty.Columns(lstFashQty.Col).DataField & ")", "Tmp_Stat<>'D' and Tmp_DRecId='" & txtRecid.Text.Trim & "'  ")
                If CType(drs, Double) > Val(txt.Text) Then
                    MessageBox.Show("Barcode Number Can't Be Bigger Than Quantity Of This Size.", cntTitle, MessageBoxButtons.OK, MessageBoxIcon.Exclamation)
                    e.Cancel = True
                    Exit Sub
                End If
            End If

            'strsql = " SELECT  bar.Tmp_DRecId , COUNT(*) Counter FROM   " & TmpFashQty & " qty"
            'strsql &= " LEFT JOIN " & TmpGridBarcode & "F bar ON bar.Tmp_DRecId=qty.RECID"
            'strsql &= " where  qty.recid=" & txtRecid.Text.Trim & "  AND bar.tmp_" & lstFashQty.Columns(lstFashQty.Col).DataField & " = qty." & lstFashQty.Columns(lstFashQty.Col).DataField
            'strsql &= " GROUP BY bar.Tmp_DRecId  "

            'ReadFromDBToDataReader(strsql, drsql)
            'If drsql.HasRows Then
            '    While drsql.Read
            '        If Val(drsql.Item("Counter").ToString) > Val(txt.Text) Then
            '            MessageBox.Show("Barcode Number Is Bigger than Quantity Per Item", cntTitle, MessageBoxButtons.OK, MessageBoxIcon.Exclamation)
            '            e.Cancel = True
            '            Exit Sub
            '        End If
            '    End While
            '    drsql = Nothing
            'End If
        Catch ex As Exception
            MessageBox.Show("Error : " & ex.Message, cntTitle, _
            MessageBoxButtons.OK, MessageBoxIcon.Error)
        End Try
    End Sub
    Private Sub lstFashQty_FilterChange(ByVal sender As Object, ByVal e As System.EventArgs) Handles lstFashQty.FilterChange
        Try
            If lstFashQty.FilterActive = True Then
                lstFashQty.AllowFilter = False : GridFilterChange(lstFashQty)
            End If
        Catch ex As Exception
            MessageBox.Show("Error: " & ex.Message, cntTitle, MessageBoxButtons.OK, MessageBoxIcon.Error)
        End Try
    End Sub
    Private Sub LstExpiry_FilterChange(ByVal sender As Object, ByVal e As System.EventArgs) Handles LstExpiry.FilterChange
        Try
            If LstExpiry.FilterActive = True Then
                LstExpiry.AllowFilter = False : GridFilterChange(LstExpiry)
            End If
        Catch ex As Exception
            MessageBox.Show("Error: " & ex.Message, cntTitle, MessageBoxButtons.OK, MessageBoxIcon.Error)
        End Try
    End Sub
    Private Sub LstBarcode_FilterChange(ByVal sender As Object, ByVal e As System.EventArgs) Handles LstBarcode.FilterChange
        Try
            If LstBarcode.FilterActive = True Then
                LstBarcode.AllowFilter = False : GridFilterChange(LstBarcode)
            End If
        Catch ex As Exception
            MessageBox.Show("Error: " & ex.Message, cntTitle, MessageBoxButtons.OK, MessageBoxIcon.Error)
        End Try
    End Sub
    Private Sub lstFashQty_AfterColUpdate(ByVal sender As Object, ByVal e As C1.Win.C1TrueDBGrid.ColEventArgs) Handles lstFashQty.AfterColUpdate
        Try
            ErrorProvider1.SetError(lstFashQty, "")
            lstFashQty.UpdateData()
            Application.DoEvents()
            lstFashQty.Refresh()
        Catch ex As Exception
            MessageBox.Show("Error : " & ex.Message, cntTitle, _
            MessageBoxButtons.OK, MessageBoxIcon.Error)
        End Try
    End Sub
    Private Sub lstFashQty_Error(ByVal sender As Object, ByVal e As C1.Win.C1TrueDBGrid.ErrorEventArgs) Handles lstFashQty.Error
        e.Handled = True
    End Sub

    Private Sub lstFashQty_AfterUpdate(ByVal sender As Object, ByVal e As System.EventArgs) Handles lstFashQty.AfterUpdate
        Try
            daDetFqty.Update(dsDetFqty.Tables("tblDetails"))
            If Val(txtQty.Text.Trim) > 0 Then btnBarcode.Enabled = True Else btnBarcode.Enabled = False
        Catch ex As Exception
            MessageBox.Show("Error: " & ex.Message, cntTitle, MessageBoxButtons.OK, MessageBoxIcon.Error)
        End Try
    End Sub

    Private Sub RefreshLstBarcode()
        Dim strsql As String
        Dim cbsql As SqlCommandBuilder
        Dim cnSql As SqlConnection
        Dim i As Integer
        Dim drSql As SqlDataReader

        Try
            'Create Temporary Table Related To the Grid

            TxtGridVDesc.Text = GetFieldDesc("stk04_grid", "G_Code='" & TxtGridV.Text.Trim.Replace("'", "''") & "' AND G_Active='Y' ", "G_Desc")

            GridSizeColumn = CInt(GetFieldDesc("stk04_DetGRID ", "GD_Code='" & lstFashQty.Columns("GridCode").Text.Trim & "'", "COUNT(GD_SizeDesc)"))

            If UCase(lstDelivery.Columns("ItmClass").Text.Trim) = "F" Or UCase(txtItmClass.Text.Trim) = "F" Then
                strsql = "SELECT GD_SizeDesc FROM dbo.stk04_DetGRID WHERE GD_Code='" & lstFashQty.Columns("GridCode").Text.Trim & "'"
                strsql &= " and GD_SizeDesc is not null order by GD_Order"
                ReadFromDBToDataReader(strsql, drSql)
            End If

            cnSql = New SqlConnection(ConnectionString)
            cnSql.Open()

            strsql = "SELECT "
            strsql &= " Tmp_RecId,Tmp_DRecId,Tmp_Color, "
            For i = 1 To GridSizeColumn
                strsql &= " Tmp_Size" & i & ","
                strsql &= " Tmp_OldNew" & i & ","
            Next
            strsql &= " Tmp_Stat "
            strsql &= " from " & TmpGridBarcode & "F"
            strsql &= " where Tmp_Stat <> 'D'  "
            strsql &= " ORDER BY   Tmp_RecId "

            dsDetBarcodeF = New DataSet

            If Not dsDetBarcodeF.Tables("tblDetails") Is Nothing Then dsDetBarcodeF.Tables.Remove("tblDetails")
            daDetBarcodeF = New SqlDataAdapter(strsql, cnSql)
            daDetBarcodeF.TableMappings.Add("Table", "tblDetails")

            With daDetBarcodeF
                .FillSchema(dsDetBarcodeF, SchemaType.Mapped)
                .Fill(dsDetBarcodeF, "tblDetails")
            End With

            cbsql = New SqlCommandBuilder(daDetBarcodeF)
            daDetBarcodeF.UpdateCommand = cbsql.GetUpdateCommand
            daDetBarcodeF.InsertCommand = cbsql.GetInsertCommand
            daDetBarcodeF.DeleteCommand = cbsql.GetDeleteCommand

            cbsql = Nothing

            With LstBarcodeF
                .EditActive = False
                .FetchRowStyles = True

                If Not .DataSource Is Nothing Then .DataSource = Nothing
                .ClearFields()
                .DataSource = dsDetBarcodeF.Tables("tblDetails").DefaultView

                .AllowFilter = True

                With .Splits(0).DisplayColumns("Tmp_RecId")
                    .Visible = False
                    .AllowSizing = False
                End With
                With .Splits(0).DisplayColumns("Tmp_DRecId")
                    .Visible = False
                    .AllowSizing = False
                End With
                With .Splits(0).DisplayColumns("Tmp_Stat")
                    .Visible = False
                    .AllowSizing = False
                End With
                With .Splits(0).DisplayColumns("Tmp_Color")
                    .Visible = False
                    .AllowSizing = False
                End With


                If drSql.HasRows Then
                    i = 0
                    While drSql.Read()
                        i = i + 1
                        With LstBarcodeF.Splits(0).DisplayColumns("Tmp_Size" & i)
                            .DataColumn.Caption = drSql.Item("GD_SizeDesc").ToString
                            .DataColumn.Tag = i
                            .HeadingStyle.HorizontalAlignment = AlignHorzEnum.Center
                            .Width = 120
                            .Button = True
                            .DataColumn.FilterOperator = "like"
                            .FetchStyle = True
                            If Old = True Then
                                .Locked = True
                            Else
                                .Locked = False
                            End If
                            If lstFashQty.Columns("size" & i).Text = "" OrElse lstFashQty.Columns("size" & i).Text = "0" Then .Visible = False : .AllowSizing = False
                            If Old = True Then .Locked = True Else .Locked = False
                        End With
                        LstBarcodeF.Splits(0).DisplayColumns("Tmp_OldNew" & i).Visible = False
                        LstBarcodeF.Splits(0).DisplayColumns("Tmp_OldNew" & i).AllowSizing = False
                    End While
                End If
                .FilterBar = True

            End With
            GetBarcodeF()
            LstBarcodeF.Refresh()
            LstBarcodeF.Col = 0 : LstBarcodeF.Focus()
        Catch ex As Exception
            Throw ex
        End Try
    End Sub
    Private Sub GetBarcodeF()
        Dim strsql As String

        Try

            'If RecordsExists("select * from " & TmpGridBarcode & "F where tmp_stat='N'") = False Then If Not dsDetBarcodeF.Tables("tblDetails") Is Nothing Then dsDetBarcodeF.Tables("tblDetails").Clear() : Exit Sub
            strsql = "select * from " & TmpGridBarcode & "F"
            If txtSTkID.Text.Trim = "" Then
                strsql &= " where Tmp_DRecId='" & Val(lstDelivery.Columns("RECID").Text.Trim) & "'  and tmp_Stat <>'D' "
            Else
                strsql &= " where Tmp_DRecId='" & Val(txtRecid.Text.Trim) & "'  and tmp_Stat <>'D' "
            End If

            ClearGridFiltration(LstBarcodeF)

            If Not dsDetBarcodeF.Tables("tblDetails") Is Nothing Then dsDetBarcodeF.Tables("tblDetails").Clear()


            If Not daDetBarcodeF Is Nothing Then
                daDetBarcodeF.SelectCommand.CommandText = strsql
                daDetBarcodeF.Fill(dsDetBarcodeF, "tblDetails")
                LstBarcodeF.Enabled = True
            End If

        Catch ex As Exception
            Throw ex
        End Try

    End Sub
    Private Sub lstDelivery_ButtonClick(ByVal sender As Object, ByVal e As C1.Win.C1TrueDBGrid.ColEventArgs) Handles lstDelivery.ButtonClick
        Dim txt As New TextBox
        Dim lbl As New Label
        Dim Str As String
        Dim Col As Integer
        Dim DRSQL As SqlDataReader
        Try
            If lstDelivery.Columns("new_old").Text.Trim = "" Then Exit Sub
            If txtSTkID.Text.Trim <> "" Then Exit Sub

            With lstDelivery

                Dim ColNo As Integer
                If DelOrder = "Receiving Order Fashion" Or DelOrder = "Consignment Receiving Order Fashion" Then
                    If Val(lstDelivery(lstDelivery.Row, "MOD_OrdQty").ToString.Trim) = 0 Then
                        MessageBox.Show("Quantity Couldn't be Empty.", cntTitle, MessageBoxButtons.OK, MessageBoxIcon.Warning)

                        If DelOrder = "Receiving Order Fashion" Or DelOrder = "Consignment Receiving Order Fashion" Then
                            lstFashQty.Col = lstFashQty.Splits(0).DisplayColumns.IndexOf(lstFashQty.Columns("Size1"))
                            'Else
                            '    If UCase(lstDelivery.Columns("ItmClass").CellValue(lstDelivery.Row).ToString) = "E" Then
                            '        LstExpiry.Col = LstExpiry.Splits(0).DisplayColumns.IndexOf(LstExpiry.Columns("MOD_OrdQty"))
                            '        LstExpiry.Focus()
                            '    Else
                            '        lstDelivery.Col = lstDelivery.Splits(0).DisplayColumns.IndexOf(lstDelivery.Columns("MOD_OrdQty"))
                            '        lstDelivery.Focus()
                            '    End If
                        End If
                        Exit Sub
                    End If

                    BarcodeF.Width = 989
                    BarcodeF.Height = 409

                    BarcodeF.Left = 0
                    BarcodeF.Top = 127

                    BarcodeF.Visible = True
                    GetBarcodeF()
                    TxtGridV.Text = lstFashQty.Columns("GridCode").Text : RefreshLstBarcode()
                    If UCase(lstDelivery.Columns("new_old").Text.Trim) = "O" Then
                        btnOK1.Enabled = True : btnClear.Enabled = False
                    Else
                        btnOK1.Enabled = True : btnClear.Enabled = True
                    End If
                    If txtSTkID.Text.Trim = "" Then
                        btnOK1.Enabled = True : btnClear.Enabled = False
                        BtnGenerateBarcodeFashion.Enabled = False
                    Else
                        btnOK1.Enabled = True : btnClear.Enabled = True
                        BtnGenerateBarcodeFashion.Enabled = True
                    End If

                Else
                    If e.Column.Name = "QC" Then
                        If lstDelivery.Columns("ItmClass").Text.Trim = "E" Then
                            btnCancel.Focus()
                            GrpExpBarcode.Visible = True
                            LstExpiry.Enabled = False
                            GrpExpBarcode.Focus()
                            GrpBarcodeButtons.Visible = False
                            BtnOKExpiryBarcode.Enabled = False : BtnGenerateBarcodeTrading.Enabled = False

                            LstVisbility(True, False, lstDelivery.Columns("ItmClass").Text.Trim)
                            Exit Try
                        End If

                        Dim tempfrm As frmQC
                        Dim QCtitle As String
                        Dim selectedRecID, ACCQTy As String
                        tempfrm = New frmQC
                        With lstDelivery
                            QCtitle = "Ref : " & .Columns("Stk_Ref").Text.Trim & " - Desc : " & .Columns("stk_desc").Text.Trim & ""
                            tempfrm.ShowDialog(.Columns("MOD_OrdQty").Text.Trim, QCtitle, .Columns("RECID").Text.Trim, "", TempQcHeader, TempQcDetails, ACCQTy, True)
                        End With



                        Exit Try
                    End If
                    If DelOrder = "Receiving Order" Or DelOrder = "Consignment Receiving Order" Then
                        '   If GrpExpBarcode.Visible = True Then GrpExpBarcode.Visible = False : Exit Sub
                        GrpExpBarcode.Visible = True
                        GrpBarcodeButtons.Visible = False
                        BtnOKExpiryBarcode.Enabled = False : BtnGenerateBarcodeTrading.Enabled = False
                        If UCase(lstDelivery.Columns("ItmClass").Text.Trim) = "E" Then
                            LstVisbility(True, True, lstDelivery.Columns("ItmClass").Text.Trim)
                        Else
                            LstVisbility(False, True, lstDelivery.Columns("ItmClass").Text.Trim)
                        End If

                    End If
                End If
            End With
        Catch ex As Exception
            MessageBox.Show("Error: " & ex.Message, cntTitle, _
            MessageBoxButtons.OK, MessageBoxIcon.Error)
        End Try
    End Sub
    Private Sub LstBarcodeF_ButtonClick(ByVal sender As Object, ByVal e As C1.Win.C1TrueDBGrid.ColEventArgs) Handles LstBarcodeF.ButtonClick
        Dim drs() As DataRow
        Try
            'If Wtype = "" Or Wtype = "DELETE" Then Exit Sub
            If txtSTkID.Text.Trim = "" Then Exit Sub
            If UCase(txtOldNew.Text) = "W" Then Exit Sub
            'If lstDelivery.Columns("new_old").Text = "O" And Wtype = "MODIFY" Then Old = True : Exit Sub
            'If Wtype = "" Or Wtype = "DELETE" Then Exit Sub

            ItemBarcodeF(e.ColIndex)
        Catch ex As Exception
            MessageBox.Show("Error : " & ex.Message, cntTitle, _
            MessageBoxButtons.OK, MessageBoxIcon.Error)
        End Try
    End Sub


    Private Sub LstBarcodeF_OnAddNew(ByVal sender As Object, ByVal e As System.EventArgs) Handles LstBarcodeF.OnAddNew
        Dim dt As DataTable
        Dim MaxCnt As Long
        Dim MaxRecID As Long
        Try
            If txtSTkID.Text.Trim = "" Then Exit Sub
            With LstBarcodeF.Splits(0).DisplayColumns("Tmp_RecId").DataColumn
                .Value = Val(GetMaxRecId(TmpGridBarcode & "F", "Tmp_RecId"))
                .RefreshCell()
            End With
            With LstBarcodeF.Splits(0).DisplayColumns("Tmp_Stat").DataColumn
                .Value = "N"
                .RefreshCell()
            End With
            With LstBarcodeF.Splits(0).DisplayColumns("Tmp_DRecId").DataColumn
                .Value = txtRecid.Text.Trim
                .RefreshCell()
            End With
            With LstBarcodeF.Splits(0).DisplayColumns("Tmp_Color").DataColumn
                .Value = txtColor.Text.Trim
                .RefreshCell()
            End With
            If AllowWriteInBarc() = False Then

                MessageBox.Show("Please Fill Higher Rows First", cntTitle, MessageBoxButtons.OK, MessageBoxIcon.Exclamation)
                Exit Sub
            End If
        Catch ex As Exception
            MessageBox.Show("Error: " & ex.Message, cntTitle, _
            MessageBoxButtons.OK, MessageBoxIcon.Error)
        End Try
    End Sub
    Private Function AllowWriteInBarc() As Boolean
        Dim drs() As DataRow
        Dim MinRecID As Long
        Try
            AllowWriteInBarc = True

            Dim dt As DataTable
            dt = dsDetBarcodeF.Tables("tblDetails")
            Dim dv As New DataView(dt)

            dv.RowFilter = "Tmp_RecId=Min(Tmp_RecId)"
            If dv.Count = 0 Then
                MinRecID = 0
            Else
                MinRecID = CLng(dv(0)(0).ToString)
            End If

            If UCase(txtItmClass.Text.Trim) = "T" Or UCase(txtItmClass.Text.Trim) = "B" Or UCase(txtItmClass.Text.Trim) = "E" Then
                drs = dsDetBarcodeF.Tables("tblDetails").Select("([Tmp_Barcode] = '' or [Tmp_Barcode] is null) and [Tmp_RecId] < '" & LstBarcodeF.Columns("Tmp_RecId").Text & "'")
            ElseIf UCase(txtItmClass.Text.Trim) = "F" Then
                '''''''drs = dsSqlBarc.Tables("tbBarc").Select("([Tmp_Size" & LstBarcode.Col - 1 & "] = '' ''''''''''''''''''''''''''''''or [Tmp_Size" & LstBarcode.Col - 1 & "] is null)                                 and [Tmp_Color] = '" & LstColorsExpiry.Columns("Color_Code").Text & "' and [Tmp_RecId] < '" & Val(LstBarcode.Columns("Tmp_RecId").Text) & "'")
                drs = dsDetBarcodeF.Tables("tblDetails").Select("([Tmp_Size" & LstBarcodeF.Columns(LstBarcodeF.Col).Tag.ToString & "] = '' or [Tmp_Size" & LstBarcodeF.Columns(LstBarcodeF.Col).Tag.ToString & "] is null) and [Tmp_Color] = '" & txtColor.Text.Trim & "' and [Tmp_RecId] < '" & LstBarcodeF.Columns("Tmp_RecId").Text.Trim & "'")
            End If

            If drs.Length <> 0 Then  'Don't allow the user to write because there are empty records in higher rows
                AllowWriteInBarc = False

                If Me.BindingContext(dsDetBarcodeF.Tables("tblDetails").DefaultView).Count > 0 Then
                    LstBarcode.EditActive = False
                    Me.BindingContext(dsDetBarcodeF.Tables("tblDetails").DefaultView).CancelCurrentEdit()
                    dsDetBarcodeF.Tables("tblDetails").AcceptChanges()
                End If
            End If
        Catch ex As Exception
            Throw ex
        End Try
    End Function
    Private Sub btnOK1_Click(ByVal sender As System.Object, ByVal e As System.EventArgs) Handles btnOK1.Click
        Try
            LstBarcodeF.UpdateData() : daDetBarcodeF.Update(dsDetBarcodeF.Tables("tblDetails"))

            BarcodeF.Visible = False
            LstVisbility(False, False, lstDelivery.Columns("ItmClass").Text.Trim)

            lstDelivery.Col = 0
            lstDelivery.Splits(0).HorizontalOffset = 0
            lstDelivery.Refresh()
            If txtSTkID.Text.Trim <> "" Then txtSupRef.Focus()
        Catch ex As Exception
            MessageBox.Show("Error : " & ex.Message, cntTitle, _
            MessageBoxButtons.OK, MessageBoxIcon.Error)
        End Try
    End Sub

    Private Sub btnClear_Click(ByVal sender As System.Object, ByVal e As System.EventArgs) Handles btnClear.Click
        Try
            If btnClear.Enabled = False Then Exit Sub

            strsql = "delete from " & TmpGridBarcode & "F where Tmp_DRecid= " & txtRecid.Text.Trim & " "
            Execute(strsql)
            dsDetBarcodeF.Tables(0).Clear()
            daDetBarcodeF.Fill(dsDetBarcodeF, dsDetBarcodeF.Tables(0).TableName)
            For Idx As Integer = 0 To LstBarcodeF.Columns.Count - 1
                LstBarcodeF.Columns(Idx).FilterText = ""
            Next

            LstBarcodeF.SelectedRows.Clear()

        Catch ex As Exception
            MessageBox.Show("Error : " & ex.Message, cntTitle, _
            MessageBoxButtons.OK, MessageBoxIcon.Error)
        End Try
    End Sub

    Private Sub ItemBarcodeF(ByVal Col As Integer)
        Dim drs As DataRow()
        Dim strsql As String
        Dim cnSql As SqlConnection
        Dim dsSql As DataSet
        Dim daSql As SqlDataAdapter
        Dim frm As frmSearch
        Dim Sizefield() As Int16 = {0, 200}
        Dim Alnfield() As String = {"C", "C"}
        Dim Retfield() As String = {"dbar_stkid", "Barcode"}
        Dim txtB As String

        Try

            Cursor.Current = Cursors.WaitCursor
            strsql = " Select dbar_stkid,dbar_barcode as [Barcode] FROM Stk04_DetBarcode"
            strsql &= " WHERE dbar_grid='" & lstFashQty.Columns("GridCode").Text.Trim & "'"
            strsql &= " AND dbar_Detgrid='" & LstBarcodeF.Splits(0).DisplayColumns(Col).Name & "' "
            strsql &= " AND dbar_color='" & txtColor.Text.Trim & "'"
            strsql &= " and dbar_stkid='" & txtSTkID.Text.Trim & "'"
            strsql &= " AND dbar_barcode not in ( SELECT isnull( Tmp_Size" & LstBarcodeF.Columns(LstBarcodeF.Col).Tag.ToString & ",'') FROM  " & TmpGridBarcode & "F  where tmp_stat<>'D')"

            If RecordsExists(strsql) = False Then Exit Sub

            cnSql = New SqlConnection(ConnectionString)
            cnSql.Open()
            daSql = New SqlDataAdapter(strsql, cnSql)
            dsSql = New DataSet
            daSql.Fill(dsSql, "tbcode3")
            cnSql.Close()
            frm = New frmSearch

            With frm
                .Owner = Me

                If .ShowDialog("Return", dsSql.Tables("tbcode3"), dsSql.Tables("tbcode3").Rows.Count, _
                "Barcode", Sizefield, Alnfield, Retfield, 0, Nothing, False, 350, 300, , True) = DialogResult.OK Then
                    If gbRetSearch(0) = "" Then
                        txtSupp.Focus()
                    Else
                        If gbRetSearch(0) <> "" Then
                            If AllowWriteInBarc() = False Then

                                MessageBox.Show("Please Fill Higher Rows First", cntTitle, MessageBoxButtons.OK, MessageBoxIcon.Exclamation)
                                Exit Sub
                            End If
                            LstBarcodeF.Columns(Col).Text = gbRetSearch(1)

                            'test on quantity per size and number of barcodes 
                            drs = dsDetBarcodeF.Tables("tblDetails").Select("Tmp_Stat<>'D' and Tmp_DRecId='" & txtRecid.Text.Trim & "' and tmp_size" & CInt(LstBarcodeF.Columns(LstBarcodeF.Col).Tag) & "<>''")

                            If drs.Length >= Val(lstFashQty.Columns("Size" & CInt(LstBarcodeF.Columns(LstBarcodeF.Col).Tag)).Text.Trim) Then
                                MessageBox.Show("Barcode Number Can't Be Bigger Than Quantity Of This Size.", cntTitle, MessageBoxButtons.OK, MessageBoxIcon.Exclamation)
                                Me.BindingContext(dsDetBarcodeF.Tables("tblDetails").DefaultView).CancelCurrentEdit()
                                Exit Sub
                            End If
                            LstBarcodeF.Columns(Col + 1).Text = "O"
                            LstBarcodeF.Columns("Tmp_Stat").Text = "N"

                            LstBarcodeF.UpdateData()
                            daDetBarcodeF.Update(dsDetBarcodeF.Tables("tblDetails"))
                            SendKeys.Send("{DOWN}")
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
    Private Sub LstBarcodeF_BeforeRowColChange(ByVal sender As Object, ByVal e As C1.Win.C1TrueDBGrid.CancelEventArgs) Handles LstBarcodeF.BeforeRowColChange
        'Dim drs As DataRow()
        'Try

        '    'Don't allow the user to enter same barcode twice
        '    If LstBarcodeF.Columns(LstBarcodeF.Col).Text.Trim.Replace("'", "''") = "" Then Exit Sub
        '    'LstBarcodeF.UpdateData()
        '    If GridSizeColumn <> 1 Then
        '        strsql = "  Tmp_Stat<>'D' and ("
        '        For i = 1 To GridSizeColumn
        '            'If CInt(LstBarcodeF.Columns(LstBarcodeF.Col).Tag) <> i Then 'Or LstBarcodeF.DestinationRow <> LstBarcodeF.RowBookmark(LstBarcodeF.Row) 
        '            ' If LstBarcodeF.Columns(e.ColIndex) <> i Then
        '            strsql &= " [Tmp_Size" & i & "]='" & LstBarcodeF.Columns(LstBarcodeF.Col).Text & "' OR "
        '            'End If
        '        Next
        '        strsql = strsql.Substring(1, strsql.Length - 4)
        '        strsql &= " )"
        '        drs = dsDetBarcodeF.Tables("tblDetails").Select(strsql)
        '        If drs.Length <> 0 Then
        '            ' If drs(0)(LstBarcodeF.Col, DataRowVersion.Original).ToString <> drs(0)(LstBarcodeF.Col, DataRowVersion.Current).ToString Then
        '            'If  LstBarcodeF.Col, DataRowVersion.Original).text.trim <> LstBarcodeF.Col, DataRowVersion.Current then
        '            MessageBox.Show("Barcode Already Exist", cntTitle, MessageBoxButtons.OK, MessageBoxIcon.Exclamation)
        '            e.Cancel = True
        '            LstBarcodeF.Columns(LstBarcodeF.Col).Text = ""
        '            Exit Sub
        '        End If

        '        strsql = " select * from " & TmpGridBarcode & "F  where Tmp_Stat<>'D'  and ("
        '        For i = 1 To GridSizeColumn
        '            If CInt(LstBarcodeF.Columns(LstBarcodeF.Col).Tag) <> i Then
        '                strsql &= "  [Tmp_Size" & i & "]='" & LstBarcodeF.Columns(LstBarcodeF.Col).Text.Trim.Replace("'", "''") & "' OR "
        '            End If
        '        Next
        '        strsql = strsql.Substring(1, strsql.Length - 4)
        '        strsql &= " )"
        '        If RecordsExists(strsql) = True Then
        '            MessageBox.Show("Barcode Already Exist", cntTitle, MessageBoxButtons.OK, MessageBoxIcon.Exclamation)
        '            e.Cancel = True
        '            LstBarcodeF.Columns(LstBarcodeF.Col).Text = ""
        '            Exit Sub
        '        End If
        '    End If
        'Catch ex As Exception
        '    MessageBox.Show("Error: " & ex.Message, cntTitle, MessageBoxButtons.OK, MessageBoxIcon.Error)
        'End Try
    End Sub
    Private Sub LstBarcodeF_FilterChange(ByVal sender As Object, ByVal e As System.EventArgs) Handles LstBarcodeF.FilterChange
        Try
            If LstBarcodeF.FilterActive = True Then
                LstBarcodeF.AllowFilter = False : GridFilterChange(LstBarcodeF)
            End If
        Catch ex As Exception
            MessageBox.Show("Error: " & ex.Message, cntTitle, MessageBoxButtons.OK, MessageBoxIcon.Error)
        End Try
    End Sub
    Private Sub LstBarcodeF_Error(ByVal sender As Object, ByVal e As C1.Win.C1TrueDBGrid.ErrorEventArgs) Handles LstBarcodeF.Error
        e.Handled = True
    End Sub
    Private Sub LstBarcodeF_KeyDown(ByVal sender As Object, ByVal e As System.Windows.Forms.KeyEventArgs) Handles LstBarcodeF.KeyDown
        Try

            Select Case e.KeyCode
                Case Keys.Escape : If btnOK1.Focus Then btnOK1_Click(btnOK1, New System.EventArgs)
            End Select

            If Wtype = "DELETE" Or Wtype = "" Then Exit Sub
            If txtSTkID.Text.Trim = "" Then Old = True : Exit Sub
            If UCase(txtOldNew.Text) = "W" Then Exit Sub

            LstBarcodeF.Enabled = True
            If BarcodeF.Visible = True AndAlso e.Modifiers = Keys.Alt AndAlso e.KeyCode = Keys.Down Then
                e.Handled = True
                ItemBarcodeF(LstBarcodeF.Col)
            End If

        Catch ex As Exception
            MessageBox.Show("Error : " & ex.Message, cntTitle, _
            MessageBoxButtons.OK, MessageBoxIcon.Error)
        End Try
    End Sub
    Private Sub LstBarcodeF_FetchCellStyle(ByVal sender As Object, ByVal e As C1.Win.C1TrueDBGrid.FetchCellStyleEventArgs) Handles LstBarcodeF.FetchCellStyle
        Try

            If LstBarcodeF.Columns(e.Col + 1).CellValue(e.Row).ToString = "N" Then
                e.CellStyle.ForeColor = System.Drawing.Color.Red
            Else
                e.CellStyle.ForeColor = System.Drawing.Color.Navy
            End If
            'If LstBarcodeF.Columns(LstBarcodeF.Col + 1).Text = "N" Then

        Catch ex As Exception
            MessageBox.Show("Error : " & ex.Message, cntTitle, _
            MessageBoxButtons.OK, MessageBoxIcon.Error)
        End Try
    End Sub
    Private Sub LstBarcodeF_BeforeUpdate(ByVal sender As Object, ByVal e As C1.Win.C1TrueDBGrid.CancelEventArgs) Handles LstBarcodeF.BeforeUpdate
        Try
            If LstBarcodeF.Columns(LstBarcodeF.Col).Text.Trim <> "" Then
                Me.BindingContext(dsDetBarcodeF.Tables("tblDetails").DefaultView).EndCurrentEdit()
            End If

        Catch ex As Exception
            MessageBox.Show("Error: " & ex.Message, cntTitle, MessageBoxButtons.OK, MessageBoxIcon.Error)
        End Try
    End Sub
    Private Sub LstBarcodeF_BeforeColEdit(ByVal sender As Object, ByVal e As C1.Win.C1TrueDBGrid.BeforeColEditEventArgs) Handles LstBarcodeF.BeforeColEdit
        Dim drs As DataRow()
        Try
            If LstBarcodeF.FilterActive = True Then Exit Sub
            If txtSTkID.Text.Trim = "" Then Old = True : e.Cancel = True : Exit Sub
            If lstDelivery.Columns("DOB_Drecid").Text.Trim <> "" Then Exit Sub

            '  BarcodeOld = LstBarcodeF.Columns(LstBarcodeF.Col).Text.Trim.Replace("'", "''")

            If AllowWriteInBarc() = False Then
                e.Cancel = True
                MessageBox.Show("Please Fill Higher Rows First", cntTitle, MessageBoxButtons.OK, MessageBoxIcon.Exclamation)
            End If

            'test on quantity per size and number of barcodes 
            drs = dsDetBarcodeF.Tables("tblDetails").Select("Tmp_Stat<>'D' and Tmp_DRecId='" & txtRecid.Text.Trim & "' and Tmp_recid<>'" & LstBarcodeF.Columns("Tmp_RecID").Text & "' and tmp_size" & CInt(LstBarcodeF.Columns(LstBarcodeF.Col).Tag) & "<>''")

            If drs.Length >= Val(lstFashQty.Columns("Size" & CInt(LstBarcodeF.Columns(LstBarcodeF.Col).Tag)).Text.Trim) Then
                MessageBox.Show("Barcode Number Can't Be Bigger Than Quantity Of This Size.", cntTitle, MessageBoxButtons.OK, MessageBoxIcon.Exclamation)
                e.Cancel = True
                Exit Sub
            End If

        Catch ex As Exception
            MessageBox.Show("Error : " & ex.Message, cntTitle, _
            MessageBoxButtons.OK, MessageBoxIcon.Error)
        End Try
    End Sub
    Private Sub LstBarcodeF_AfterFilter(ByVal sender As Object, ByVal e As C1.Win.C1TrueDBGrid.FilterEventArgs) Handles LstBarcodeF.AfterFilter
        Try
            LstBarcodeF.Row = 0
            LstBarcodeF.SelectedRows.Clear()
            LstBarcodeF.SelectedRows.Add(0)
        Catch ex As Exception
            MessageBox.Show("Error : " & ex.Message, cntTitle, MessageBoxButtons.OK, MessageBoxIcon.Error)
        End Try
    End Sub

    Private Sub LstBarcodeF_AfterColUpdate(ByVal sender As Object, ByVal e As C1.Win.C1TrueDBGrid.ColEventArgs) Handles LstBarcodeF.AfterColUpdate
        Try
            ErrorProvider1.SetError(LstBarcodeF, "")
            'If LstBarcodeF.Columns("Tmp_Size1").Text.Trim = "" Then Exit Sub
            LstBarcodeF.UpdateData()

            Application.DoEvents()
            LstBarcodeF.Refresh()
        Catch ex As Exception
            MessageBox.Show("Error : " & ex.Message, cntTitle, _
            MessageBoxButtons.OK, MessageBoxIcon.Error)
        End Try
    End Sub
    Private Sub LstBarcodeF_BeforeColUpdate(ByVal sender As Object, ByVal e As C1.Win.C1TrueDBGrid.BeforeColUpdateEventArgs) Handles LstBarcodeF.BeforeColUpdate
        Dim drs() As DataRow
        Dim strsql As String
        Dim bExist As Boolean
        Try

            If LstBarcodeF.Columns(e.ColIndex).Text.Trim = "" Then
                Exit Sub
            End If

            'Check if length of barcode is greater than 20 characters by using copy paste
            If LstBarcodeF.Columns(e.ColIndex).Text.Length > 20 Then
                MessageBox.Show("Barcode Should Be Less Than 20 Characters", cntTitle, MessageBoxButtons.OK, MessageBoxIcon.Exclamation)
                e.Cancel = True
                Exit Sub
            End If

            'Don't allow the user to enter same barcode twice
            If GridSizeColumn <> 1 Then
                strsql = "select Tmp_RecId from " & TmpGridBarcode & "F where ("
                For i = 1 To GridSizeColumn
                    If CInt(LstBarcodeF.Columns(e.ColIndex).Tag) <> i Then
                        strsql &= " [Tmp_Size" & i & "]='" & LstBarcodeF.Columns(e.ColIndex).Text.Trim & "' OR"
                    End If
                Next
                If Len(strsql) > 2 Then
                    strsql = strsql.Substring(0, strsql.Length - 2)
                End If
                strsql &= " ) OR ([Tmp_Size" & CInt(LstBarcodeF.Columns(e.ColIndex).Tag) & "] = '" & LstBarcodeF.Columns(e.ColIndex).Text.Trim & "' ) "
            ElseIf GridSizeColumn = 1 Then
                strsql = " select Tmp_RecId from " & TmpGridBarcode & "F where  "
                strsql &= " [Tmp_Size" & CInt(LstBarcodeF.Columns(LstBarcodeF.Col).Tag) & "]='" & LstBarcodeF.Columns(LstBarcodeF.Col).Text.Trim.Replace("'", "''") & "'  "
            End If
            strsql &= " AND Tmp_RecId<>'" & LstBarcodeF.Columns("Tmp_RecId").Text.Trim & "'  and Tmp_Stat<>'D' "
            bExist = RecordsExists(strsql)


            If bExist = True Then
                MessageBox.Show("Barcode Already Exist", cntTitle, MessageBoxButtons.OK, MessageBoxIcon.Exclamation)
                e.Cancel = True
                If Me.BindingContext(dsDetBarcodeF.Tables("tblDetails").DefaultView).Count > 0 Then
                    LstBarcodeF.Columns(e.ColIndex).Text = ""
                End If
                'If LstBarcodeF.Columns(LstBarcodeF.Col).Text.Trim <> "" Then
                '    Me.BindingContext(dsDetBarcodeF.Tables("tblDetails").DefaultView).EndCurrentEdit()
                'End If
                Exit Sub
            End If

            'Check if barcode already exist for another item
            strsql = "SELECT dbar_stkid FROM dbo.Stk04_DetBarcode WHERE dbar_stkid <> '" & txtRecid.Text & "' and "
            strsql &= " dbar_barcode='" & LstBarcodeF.Columns(e.ColIndex).Text.Trim.Replace("'", "''") & "'"

            If RecordsExists(strsql) = True Then
                MessageBox.Show("Barcode Already Exist", cntTitle, MessageBoxButtons.OK, MessageBoxIcon.Exclamation)
                e.Cancel = True
                If Me.BindingContext(dsDetBarcodeF.Tables("tblDetails").DefaultView).Count > 0 Then
                    LstBarcodeF.Columns(e.ColIndex).Text = ""
                End If
                'If LstBarcodeF.Columns(LstBarcodeF.Col).Text.Trim <> "" Then
                '    Me.BindingContext(dsDetBarcodeF.Tables("tblDetails").DefaultView).EndCurrentEdit()
                'End If
                Exit Sub
            Else
                LstBarcodeF.Columns(e.ColIndex + 1).Text = "N"
            End If

            'Check if barcode is EAN 13 or UPCA 
            If IsNumeric(LstBarcodeF.Columns(e.ColIndex).Text.Trim) Then
                If CheckEAN13((LstBarcodeF.Columns(e.ColIndex).Text.Trim)) = False Or _
                    CheckUPCA(LstBarcodeF.Columns(e.ColIndex).Text.Trim) = False Then
                    ShowNotification(Me, "Invalid Barcode", 3000, 12, DevComponents.DotNetBar.eToastPosition.MiddleCenter)
                    'MessageBox.Show("Invalid Barcode", cntTitle, MessageBoxButtons.OK, MessageBoxIcon.Exclamation)
                    'e.Cancel = True
                    'Exit Sub
                End If
            End If


        Catch ex As Exception
            MessageBox.Show("General Error : " & ex.Message, Me.Text, MessageBoxButtons.OK, MessageBoxIcon.Warning)
        End Try
    End Sub
    Private Sub LstBarcodeF_BeforeDelete(ByVal sender As Object, ByVal e As C1.Win.C1TrueDBGrid.CancelEventArgs) Handles LstBarcodeF.BeforeDelete
        Try
            If Wtype = "" Or Wtype = "DELETE" Then e.Cancel = True : Exit Sub
            If txtSTkID.Text.Trim = "" Then e.Cancel = True : Exit Sub

            With LstBarcodeF.Splits(0).DisplayColumns("Tmp_Stat").DataColumn
                .Value = "D"
                .RefreshCell()
            End With


            LstBarcodeF.UpdateData()
            daDetBarcodeF.Update(dsDetBarcodeF.Tables("tblDetails"))
            e.Cancel = True

            If Not dsDetBarcodeF.Tables("tblDetails") Is Nothing Then dsDetBarcodeF.Tables("tblDetails").Clear()
            With daDetBarcodeF
                .FillSchema(dsDetBarcodeF, SchemaType.Mapped)
                .Fill(dsDetBarcodeF, "tblDetails")
            End With

        Catch ex As Exception
            MessageBox.Show("Error: " & ex.Message, cntTitle, _
            MessageBoxButtons.OK, MessageBoxIcon.Error)
        End Try
    End Sub

    Private Sub btnPending_Click(ByVal sender As System.Object, ByVal e As System.EventArgs) Handles btnPending.Click
        Dim frmselect As frmSearchWithCheck
        Dim dsSearch As DataSet
        Dim daSearch As SqlDataAdapter
        Dim cbsqlSearch As SqlCommandBuilder
        Dim cnsql As SqlConnection

        Dim cnSqlConcat As SqlConnection
        Dim cmSqlConcat As SqlCommand
        Dim drSqlConcat As SqlDataReader

        Dim Sizefield() As Int16 = {}
        Dim Alnfield() As String = {}
        Dim Retfield() As String = {}

        Dim LastCost As Double
        Dim i, J As Integer

        Try
            If Wtype = "DELETE" Or Wtype = "VIEW" Then Exit Sub
            If Wtype = "MODIFY" And txtCode.Text = "" Then Exit Sub
            If lstDelivery.Enabled = False Then Exit Sub

            Cursor.Current = Cursors.WaitCursor

            If txtSupp.Text.Trim = "" Then
                MessageBox.Show("You Should Choose a Supplier.", cntTitle, MessageBoxButtons.OK, MessageBoxIcon.Exclamation)
                ReadonlyConsFash()
                txtRef.Text = "" : txtSupRef.Text = "" : txtSupColor.Text = ""
                txtSupp.Focus()
                Exit Sub
            ElseIf txtDate.Value Is DBNull.Value Then
                MessageBox.Show("Date Can Not Be Null.", cntTitle, MessageBoxButtons.OK, MessageBoxIcon.Exclamation)
                ReadonlyConsFash()
                txtRef.Text = "" : txtSupRef.Text = "" : txtSupColor.Text = ""
                txtDate.Focus()
                Exit Sub
            ElseIf txtDelOrder.Text.Trim = "" Then
                MessageBox.Show("You Should enter A Receiving Order Ref.", cntTitle, MessageBoxButtons.OK, MessageBoxIcon.Exclamation)
                ReadonlyConsFash()
                txtRef.Text = "" : txtSupRef.Text = "" : txtSupColor.Text = ""
                txtDelOrder.Focus()
                Exit Sub
            ElseIf txtLoc.Text.Trim = "" Then
                MessageBox.Show("You Should Pick A Location.", cntTitle, MessageBoxButtons.OK, MessageBoxIcon.Exclamation)
                ReadonlyConsFash()
                txtRef.Text = "" : txtSupRef.Text = "" : txtSupColor.Text = ""
                txtLoc.Focus()
                Exit Sub
            End If

            If DelOrder = "Receiving Order Fashion" Or DelOrder = "Consignment Receiving Order Fashion" Then

                If txtBrand.Text.Trim = "" Then
                    MessageBox.Show("You Should Pick A Brand.", cntTitle, MessageBoxButtons.OK, MessageBoxIcon.Exclamation)
                    ReadonlyConsFash()
                    txtRef.Text = "" : txtSupRef.Text = "" : txtSupColor.Text = ""
                    txtBrand.Focus()
                    Exit Sub
                ElseIf txtSeason.Text.Trim = "" Then
                    MessageBox.Show("You Should Pick A Season.", cntTitle, MessageBoxButtons.OK, MessageBoxIcon.Exclamation)
                    ReadonlyConsFash()
                    txtRef.Text = "" : txtSupRef.Text = "" : txtSupColor.Text = ""
                    txtSeason.Focus()
                    Exit Sub
                End If
            End If
            If DelOrder = "Consignment Receiving Order" Or DelOrder = "Consignment Receiving Order Fashion" Then
                If txtCurr.Text.Trim = "" Then
                    MessageBox.Show("Currency Can Not Be Null.", cntTitle, MessageBoxButtons.OK, MessageBoxIcon.Exclamation)
                    ReadonlyConsFash()
                    txtRef.Text = "" : txtSupRef.Text = "" : txtSupColor.Text = ""
                    txtCurr.Focus()
                    Exit Sub
                End If
            End If


            cnsql = New SqlConnection(ConnectionString)
            cnsql.Open()

            frmselect = New frmSearchWithCheck


            If DelOrder = "Receiving Order Fashion" Or DelOrder = "Consignment Receiving Order Fashion" Then
                'ch @ 10/12/2016 this lopp will always return i=4 
                'For i = 0 To 3
                '    If KeyStyle = False Then i = i + 1
                '    If BrandKey = False Then i = i + 1
                '    If KeyGrid = False Then i = i + 1
                'Next
                i = 4
            Else
                i = 0
            End If


            ReDim Preserve Sizefield(CurrItmKeyPend.Length + 5 + i)
            ReDim Preserve Alnfield(CurrItmKeyPend.Length + 5 + i)
            ReDim Preserve Retfield(CurrItmKeyPend.Length + 5 + i)


            Sizefield(0) = 50 : Alnfield(0) = "C" : Retfield(0) = "Chk"
            Sizefield(1) = 0 : Alnfield(1) = "C" : Retfield(1) = "Recid"
            ' Sizefield(2) = 150 : Alnfield(2) = "N" : Retfield(2) = "Item. Ref"
            For i = 0 To CurrItmKeyPend.Length - 1
                If UCase(CurrItmKeyPend(i)) = "SFP_COMP" Then
                    Sizefield(i + 2) = 0
                ElseIf UCase(CurrItmKeyPend(i)) = "SFP_SINGLEREF1" Or UCase(CurrItmKeyPend(i)) = "SFP_SINGLEREF2" Then
                    Sizefield(i + 2) = 120
                Else
                    Sizefield(i + 2) = 60
                End If
                Alnfield(i + 2) = "C"
                Retfield(i + 2) = CurrItmKeyDesc(i)
            Next

            Sizefield(CurrItmKeyPend.Length + 2) = 220 : Alnfield(CurrItmKeyPend.Length + 2) = "N" : Retfield(CurrItmKeyPend.Length + 2) = "Description"
            Sizefield(CurrItmKeyPend.Length + 3) = 150 : Alnfield(CurrItmKeyPend.Length + 3) = "N" : Retfield(CurrItmKeyPend.Length + 3) = "Product Group"
            J = 900
            If DelOrder = "Receiving Order Fashion" Or DelOrder = "Consignment Receiving Order Fashion" Then
                If KeyStyle = False Then Sizefield(CurrItmKeyPend.Length + 4) = 150 : Alnfield(CurrItmKeyPend.Length + 4) = "N" : Retfield(CurrItmKeyPend.Length + 4) = "Style"
                J = 1100
                If BrandKey = False Then Sizefield(CurrItmKeyPend.Length + 5) = 150 : Alnfield(CurrItmKeyPend.Length + 5) = "N" : Retfield(CurrItmKeyPend.Length + 5) = "Brand"
                If KeyGrid = False Then Sizefield(CurrItmKeyPend.Length + 6) = 0 : Alnfield(CurrItmKeyPend.Length + 6) = "N" : Retfield(CurrItmKeyPend.Length + 6) = "GRID"
            End If

            Dim ii As Integer = 0
            While Alnfield(ii) <> Nothing
                ii += 1
            End While

            Sizefield(ii) = 0 : Alnfield(ii) = "N" : Retfield(ii) = ""
            DropSqlObject(TempPendSearch, "TABLE")

            strsql = " IF NOT EXISTS (SELECT name FROM sysobjects WHERE type = 'U' AND name = '" & TempPendSearch & "')"
            strsql &= " CREATE TABLE " & TempPendSearch
            strsql &= " (Chk VARCHAR(5),"
            strsql &= " Recid NUMERIC(18,0) NOT NULL,"

            'strsql &= " [Item. Ref.] NVARCHAR(100), "
            'Stock Item Key
            For i = 0 To CurrItmKeyPend.Length - 1
                strsql &= " [" & CurrItmKeyDesc(i) & "] [varchar] (20) NULL,"
            Next
            strsql &= " [Description] NVARCHAR(120) COLLATE SQL_Latin1_General_CP1256_CI_AS NULL, "
            strsql &= " [Product Group] NVARCHAR(30)  "
            If DelOrder = "Receiving Order Fashion" Or DelOrder = "Consignment Receiving Order Fashion" Then
                If KeyStyle = False Then strsql &= ", Style NVARCHAR(20)"
                If BrandKey = False Then strsql &= ", Brand NVARCHAR(80)"
                If KeyGrid = False Then strsql &= ", GRID  VARCHAR(5)"
            End If
            strsql &= " ,RowOrder Numeric(18,0)"
            strsql &= " CONSTRAINT PK_" & TempPendSearch & " PRIMARY KEY clustered "
            strsql &= " (Recid) ) ON [PRIMARY]"
            strsql &= " ELSE DELETE FROM " & TempPendSearch
            Execute(strsql)


            strsql = "INSERT INTO " & TempPendSearch & " "
            strsql &= " SELECT 'False' AS Chk, SFP_Recid As Recid,"
            'For i = 0 To CurrItmKeyPend.Length - 1
            '    strsql &= " " & CurrItmKeyPend(i) & " + '/' + "
            'Next
            For i = 0 To CurrItmKeyPend.Length - 1
                strsql &= " " & CurrItmKeyPend(i) & " AS [" & CurrItmKeyDesc(i) & "],"
            Next
            strsql = strsql.Trim.Substring(0, strsql.Trim.Length)
            'strsql &= " AS [Item. Ref.] , 
            strsql &= " SFP_Desc AS Description,IT_DESC AS [Product Group]"
            If DelOrder = "Receiving Order Fashion" Or DelOrder = "Consignment Receiving Order Fashion" Then
                If KeyStyle = False Then strsql &= ", ST_DESC AS Style"
                If BrandKey = False Then strsql &= ", br_Name AS Brand "
                If KeyGrid = False Then strsql &= ", SFP_GRID AS GRID"
            End If
            strsql &= ",-1 [RowOrder]"
            strsql &= " FROM STK04_StkFilPending"
            If DelOrder = "Receiving Order Fashion" Or DelOrder = "Consignment Receiving Order Fashion" Then strsql &= " INNER JOIN Stk04_Brand ON br_Code = SFP_Brand"
            strsql &= " INNER JOIN STK04_ItemType ON IT_Code = SFP_ItmType"
            strsql &= AppStkCond("SFP_Recid", , True, False, BUAccess.Purchase, True)
            If DelOrder = "Receiving Order Fashion" Or DelOrder = "Consignment Receiving Order Fashion" Then strsql &= " INNER JOIN stk04_STYLE ON ST_Code = SFP_Style"
            strsql &= " WHERE SFP_ItmClass " & IIf(DelOrder = "Receiving Order Fashion" Or DelOrder = "Consignment Receiving Order Fashion", "= 'F'", "<> 'F'").ToString
            If DelOrder = "Receiving Order Fashion" Or DelOrder = "Consignment Receiving Order Fashion" Then
                strsql &= " AND SFP_Season = '" & txtSeason.Text.Trim & "' AND SFP_Brand = '" & txtBrand.Text.Trim & "'"
            End If
            If DelOrder = "Receiving Order" Then
                strsql &= " And (SFP_ItmClass='B' or SFP_ItmClass='E' or SFP_ItmClass='T') and Sfp_NEWUSED<>'c'"
            ElseIf DelOrder = "Receiving Order Fashion" Then
                strsql &= " And  SFP_ItmClass='F'and Sfp_NEWUSED<>'c' and  SFP_Brand='" & txtBrand.Text.Trim & "' and SFP_Season='" & txtSeason.Text.Trim & "'"
            ElseIf DelOrder = "Consignment Receiving Order" Then
                strsql &= " And (SFP_ItmClass='B' or SFP_ItmClass='E' or SFP_ItmClass='T') and SFP_NEWUSED='c' AND sfp_supplier='" & txtSupp.Text.Trim & "'"
            ElseIf DelOrder = "Consignment Receiving Order Fashion" Then
                strsql &= " And  SFP_ItmClass='F'and SFP_NEWUSED='c' AND sFP_supplier='" & txtSupp.Text.Trim & "'"
                strsql &= " and  SFP_Brand='" & txtBrand.Text.Trim & "' and SFP_Season='" & txtSeason.Text.Trim & "'"
            End If
            Execute(strsql)

            strsql = "SELECT * FROM " & TempPendSearch & " "
            If Not RecordsExists(strsql) Then
                MessageBox.Show("No Records Found.", cntTitle, MessageBoxButtons.OK, MessageBoxIcon.Exclamation)
                Exit Sub
            End If
            daSearch = New SqlDataAdapter(strsql, cnsql)
            dsSearch = New DataSet
            daSearch.TableMappings.Add("Table", "PendStk")
            cbsqlSearch = New SqlCommandBuilder(daSearch)
            daSearch.UpdateCommand = cbsqlSearch.GetUpdateCommand
            cbsqlSearch = Nothing
            daSearch.Fill(dsSearch)

            With frmselect
                .Owner = Me
                If .ShowDialog("Locked", dsSearch.Tables("PendStk"), dsSearch.Tables("PendStk").Rows.Count, "Pending Stock File", _
                        Sizefield, Alnfield, Retfield, 0, Nothing, False, CShort(J), 500, , True, , , , , , , , , , , , , , , , , , , , , , , "RowOrder") = DialogResult.OK Then

                    daSearch.Update(dsSearch)

                    cnSqlConcat = New SqlConnection(ConnectionString)
                    cnSqlConcat.Open()
                    cmSqlConcat = New SqlCommand(strsql, cnSqlConcat)

                    strsql = "SELECT * FROM " & TempPendSearch & " WHERE Chk = 'True' order by RowOrder"

                    cmSqlConcat.CommandText = strsql
                    drSqlConcat = cmSqlConcat.ExecuteReader

                    With drSqlConcat
                        If .HasRows Then
                            While .Read()

                                'insert into TempDoPending
                                InsertPendItem(TempPendingDO, drSqlConcat.Item("RecID").ToString)

                                strsql = " INSERT INTO " & TmpGridDetails
                                strsql &= " SELECT ROW_NUMBER() OVER (ORDER BY sfp_Recid) + "
                                strsql &= " (SELECT ISNULL(MAX(recid), 0) FROM " & TmpGridDetails & " ) , TEMP.*"
                                strsql &= " from (Select DISTINCT  null col1, null col2,"
                                If DelOrder = "Receiving Order Fashion" Or DelOrder = "Consignment Receiving Order Fashion" Then
                                    strsql &= "null  col3,  "
                                End If
                                For i = 0 To CurrItmKeyPend.Length - 1
                                    'ch @ 17/11/2016 wf: 4618 to avoid a error  (Error converting data type varchar to numeric.)
                                    If CurrItmKeyPend(i).ToUpper = "SFP_SERIE" Then
                                        strsql &= " convert(varchar ," & CurrItmKeyPend(i) & ") + '/' + "
                                    Else
                                        strsql &= " " & CurrItmKeyPend(i) & " + '/' + "
                                    End If
                                Next
                                strsql = strsql.Trim.Substring(0, strsql.Trim.Length - 8)
                                strsql &= " col4 ,"
                                If DelOrder = "Receiving Order Fashion" Or DelOrder = "Consignment Receiving Order Fashion" Then
                                    strsql &= "sfp_gender,"
                                End If
                                strsql &= "'N' col5,"
                                If DelOrder = "Receiving Order Fashion" Or DelOrder = "Consignment Receiving Order Fashion" Then
                                    strsql &= "  SFP_ItmType ,SFP_Style,"
                                End If
                                strsql &= " ( SELECT ISNULL(MAX(SFP_Recid),0 ) FROM " & TempPendingDO & " ) sfp_Recid,1 col6," ' CASE WHEN DOD_Stat = 'F' THEN 'Free' WHEN DOD_Stat = 'R' THEN 'Return' WHEN DOD_Stat = 'N' THEN 'Normal' WHEN DOD_Stat is NULL THEN 'Normal' END AS [status],  "
                                strsql &= " SFP_DESC,"
                                If DelOrder = "Receiving Order Fashion" Or DelOrder = "Consignment Receiving Order Fashion" Then
                                    strsql &= " null col7,null col8,"
                                End If
                                strsql &= " 0 col9, measu.meas_desc sfp_unit, measub.meas_desc SFP_subUNIT, "
                                'strsql &= " 0 col10, CASE WHEN SFP_VATType = 'T' THEN 'T-Taxable' WHEN SFP_VATType = 'N'"
                                'strsql &= " AND SFP_VatPer > 0 THEN 'E-Taxable Embeded' ELSE 'N-Not Taxable' END ncol0,  'w' col11 , Null col13 "
                                strsql &= " 0 col10,  SFP_VATType  ncol0   "
                                strsql &= "  , 0 UnitCost  "
                                If DelOrder = "Consignment Receiving Order" Then
                                    strsql &= " , 0 disc, 0 net  "
                                End If
                                ' If DelOrder = "Receiving Order Fashion" Or DelOrder = "Consignment Receiving Order Fashion" Then

                                strsql &= " , '" & txtCurr.Text.Trim & "' curr, 'w' col11 , Null col13  ,null col12"
                                '  End If
                                strsql &= ", SFP_ItmClass  , '' BTN"

                                strsql &= " FROM " & TempPendSearch & " PendSearch"
                                strsql &= " INNER JOIN STK04_StkFilPending Pend ON PendSearch.RecID = Pend.SFP_RecID"
                                strsql &= " inner join stk04_measurements  measu on measu.meas_unit = sfp_unit  "
                                strsql &= " inner join stk04_measurements measub on measub.meas_unit = SFP_subUNIT  "
                                strsql &= " WHERE PendSearch.RECID = " & drSqlConcat.Item("RecID").ToString
                                strsql &= " ) TEMP"
                                Execute(strsql)


                                If DelOrder = "Receiving Order Fashion" Or DelOrder = "Consignment Receiving Order Fashion" Then
                                    strsql = "INSERT INTO " & TmpFashQty
                                    strsql &= " select '" & CStr(CDbl(GetMaxRecId(TmpFashQty, "Q_RECID"))) & "',(SELECT ISNULL(MAX(recid), 0) FROM " & TmpGridDetails & " ),"
                                    strsql &= "  SFP_Grid "
                                    For i = 0 To MaxCnt - 1
                                        strsql &= " ,NULL"
                                    Next
                                    strsql &= " FROM " & TempPendSearch & " PendSearch"
                                    strsql &= " INNER JOIN STK04_StkFilPending Pend ON PendSearch.RecID = Pend.SFP_RecID"
                                    strsql &= " WHERE PendSearch.RECID = " & drSqlConcat.Item("RecID").ToString
                                    Execute(strsql)

                                    If Not dsDetFqty.Tables("tblDetails") Is Nothing Then dsDetFqty.Tables("tblDetails").Clear()
                                    daDetFqty.Fill(dsDetFqty, "tblDetails")
                                End If
                            End While
                        End If
                    End With
                End If
            End With

            If Not dsDetDetails.Tables("tblDetails") Is Nothing Then dsDetDetails.Tables("tblDetails").Clear()
            daDetDetails.Fill(dsDetDetails, "tblDetails")


            RefQtyLbl(False)

            ReadonlyConsFash()
        Catch exp As Exception
            MessageBox.Show("Error : " & exp.Message, cntTitle, MessageBoxButtons.OK, MessageBoxIcon.Error)
        Finally
            Cursor.Current = Cursors.Default
            frmselect = Nothing
            'cmsql = Nothing
            'If Not drsql Is Nothing Then drsql.Close() : drsql = Nothing
            If Not cnsql Is Nothing Then
                If cnsql.State = ConnectionState.Open Then cnsql.Close()
                cnsql = Nothing
            End If
            cmSqlConcat = Nothing
            If Not drSqlConcat Is Nothing Then drSqlConcat.Close() : drSqlConcat = Nothing
            If Not cnSqlConcat Is Nothing Then
                If cnSqlConcat.State = ConnectionState.Open Then cnSqlConcat.Close()
                cnSqlConcat = Nothing
            End If
        End Try
    End Sub

    Private Sub btnCreate_Click(ByVal sender As System.Object, ByVal e As System.EventArgs) Handles btnCreate.Click
        Dim GpXEn As String
        Try
            If GrpExpBarcode.Visible = True AndAlso GrpBarcodeButtons.Visible = True Then
                'MessageBox.Show("You Should Close The Expiry Frame.", cntTitle, MessageBoxButtons.OK, MessageBoxIcon.Exclamation)
                'Exit Sub
            End If
            If txtSupp.Text.Trim = "" Then
                MessageBox.Show("You Should Choose a Supplier.", cntTitle, MessageBoxButtons.OK, MessageBoxIcon.Exclamation)
                ' ReadonlyConsFash()
                txtRef.Text = "" : txtSupRef.Text = "" : txtSupColor.Text = ""
                txtSupp.Focus()
                Exit Sub
            ElseIf txtDate.Value Is DBNull.Value Then
                MessageBox.Show("Date Can Not Be Null.", cntTitle, MessageBoxButtons.OK, MessageBoxIcon.Exclamation)
                'ReadonlyConsFash()
                txtRef.Text = "" : txtSupRef.Text = "" : txtSupColor.Text = ""
                txtDate.Focus()
                Exit Sub
            ElseIf txtDelOrder.Text.Trim = "" Then
                MessageBox.Show("You Should enter A Receiving Order Ref.", cntTitle, MessageBoxButtons.OK, MessageBoxIcon.Exclamation)
                ' ReadonlyConsFash()
                txtRef.Text = "" : txtSupRef.Text = "" : txtSupColor.Text = ""
                txtDelOrder.Focus()
                Exit Sub
            ElseIf txtLoc.Text.Trim = "" Then
                MessageBox.Show("You Should Pick A Location.", cntTitle, MessageBoxButtons.OK, MessageBoxIcon.Exclamation)
                'ReadonlyConsFash()
                txtRef.Text = "" : txtSupRef.Text = "" : txtSupColor.Text = ""
                txtLoc.Focus()
                Exit Sub
            End If

            If DelOrder = "Receiving Order Fashion" Or DelOrder = "Consignment Receiving Order Fashion" Then

                If txtBrand.Text.Trim = "" Then
                    MessageBox.Show("You Should Pick A Brand.", cntTitle, MessageBoxButtons.OK, MessageBoxIcon.Exclamation)
                    ' ReadonlyConsFash()
                    txtRef.Text = "" : txtSupRef.Text = "" : txtSupColor.Text = ""
                    txtBrand.Focus()
                    Exit Sub
                ElseIf txtSeason.Text.Trim = "" Then
                    MessageBox.Show("You Should Pick A Season.", cntTitle, MessageBoxButtons.OK, MessageBoxIcon.Exclamation)
                    ' ReadonlyConsFash()
                    txtRef.Text = "" : txtSupRef.Text = "" : txtSupColor.Text = ""
                    txtSeason.Focus()
                    Exit Sub
                End If
            End If
            If DelOrder = "Consignment Receiving Order" Or DelOrder = "Consignment Receiving Order Fashion" Then
                If txtCurr.Text.Trim = "" Then
                    MessageBox.Show("Currency Can Not Be Null.", cntTitle, MessageBoxButtons.OK, MessageBoxIcon.Exclamation)
                    ' ReadonlyConsFash()
                    txtRef.Text = "" : txtSupRef.Text = "" : txtSupColor.Text = ""
                    txtCurr.Focus()
                    Exit Sub
                End If
            End If
            ReadonlyConsFash()



            Dim frm As New FrmStkInfo
            With frm

                'test to see if the item needs to be created or modified
                'strsql = "SELECT * FROM " & TempPendingDO
                'strsql &= " WHERE SFP_Recid = " & Val(txtSTkID.Text.Trim)
                If UCase(txtOldNew.Text.Trim) = "W" Then
                    .FormName = DelOrder
                    'CH @ 08/09/2016     
                    .Source = "DO"
                    .Mode = "MODIFY"
                    .BtnClear.Visible = False
                    .lcSteps = lcSteps
                    .RecID = CInt(txtSTkID.Text.Trim)
                    .PendRecid = GetFieldDesc(TmpGridDetails & " LEFT JOIN " & TempPendingDO & " ON    SFP_Recid = MOD_RMRecid ", "  MOD_RMRecid= " & .RecID & " ", "SFP_OldRecid")
                    If .PendRecid = "" Then Exit Sub
                    GpXEn = GetFieldDesc(TempPendingDO, " sfp_recid='" & .RecID & "' ", "case when sfp_status='N' then 'True' else 'False' end")
                    .grpVAT.Enabled = CBool(IIf(GpXEn = "", "False", GpXEn))
                    .TempPendingInfo = TempPendingDO
                    .Supplier = txtSupp.Text

                    '.TxtUsageDesc.Tag = "Negation"
                    '.TxtSuppDesc.Tag = "Negation"
                    '.TxtCategDesc.Tag = "Negation"

                    If DelOrder = "Consignment Receiving Order" Or DelOrder = "Consignment Receiving Order Fashion" Then
                        .BtnStatus.Visible = False
                        .TxtStatus.ReadOnly = True
                        .BtnSupp.Visible = False
                        .TxtSupp.ReadOnly = True
                        .ConsDo = True
                    Else
                        .ConsDo = False
                    End If
                    If DelOrder = "Receiving Order Fashion" Or DelOrder = "Consignment Receiving Order Fashion" Then
                        .lcItemClass = "Fashion"
                        .Brand = txtBrand.Text : .Season = txtSeason.Text
                    Else
                        .lcItemClass = "Trading"
                    End If
                    'ReDim Preserve .ArrayKey(CurrItmKey.Length - 1)
                    'For i = 0 To CurrItmKey.Length - 1
                    '    .ArrayKey(i) = ""
                    'Next
                Else

                    .FormName = DelOrder
                    .Mode = "ADD"
                    .RecID = 0  ' kenit 0
                    'CH @ 08/09/2016     
                    .Source = "DO"
                    .TempPendingInfo = TempPendingDO
                    .Supplier = txtSupp.Text
                    .TxtSupp.Text = txtSupp.Text
                    .Curr = IIf(txtCurr.Text.Trim = gbCurr1, "", txtCurr.Text.Trim).ToString
                    Dim drsql As SqlDataReader
                    Dim NewUsed As String
                    If UCase(gbUser) = "SA" Or UCase(gbUser) = "SU" Then
                        If DelOrder = "Consignment Receiving Order" Or DelOrder = "Consignment Receiving Order Fashion" Then
                            .TxtStatus.Text = "C"
                        Else
                            'ch @ 24/10/2016 wf :4528   
                            If gbStockTag <> "" Then
                                .TxtStatus.Text = gbStockTag
                            Else
                                .TxtStatus.Text = "N"
                            End If
                        End If
                    Else
                        If DelOrder = "Consignment Receiving Order" Or DelOrder = "Consignment Receiving Order Fashion" Then
                            If GetFieldDesc(" STK04_APPSTKPRIV", "   ASP_USER='" & gbUser & "' AND ASP_APPcODE ='" & gbApp & "' AND ASP_STKTag = 'C' ", "ASP_STKTag") <> "" Then
                                .TxtStatus.Text = "C"
                            End If
                        Else
                            If gbStockTag <> "" AndAlso GetFieldDesc("STK04_APPSTKPRIV", "   ASP_USER='" & gbUser & "' AND ASP_APPcODE ='" & gbApp & "' AND ASP_STKTag ='" & gbStockTag & "'", "ASP_STKTag") <> "" Then
                                .TxtStatus.Text = gbStockTag
                            Else
                                .TxtStatus.Text = "N"
                            End If
                        End If
                    End If

                    If DelOrder = "Consignment Receiving Order" Or DelOrder = "Consignment Receiving Order Fashion" Then

                        .BtnStatus.Visible = False
                        .TxtStatus.ReadOnly = True
                        .BtnSupp.Visible = False
                        .TxtSupp.ReadOnly = True
                        .ConsDo = True
                    Else
                        ' .BtnSupp.Visible = False
                        ' .TxtSupp.ReadOnly = True
                        .ConsDo = False
                    End If
                    If DelOrder = "Receiving Order Fashion" Or DelOrder = "Consignment Receiving Order Fashion" Then
                        .lcItemClass = "Fashion"

                        .Brand = txtBrand.Text : .Season = txtSeason.Text

                    Else
                        .lcItemClass = "Trading"

                    End If
                    ReDim Preserve .ArrayKey(CurrItmKey.Length - 1)
                    For i = 0 To CurrItmKey.Length - 1
                        .ArrayKey(i) = ""
                    Next

                End If
                .ShowDialog()
                CreateItm = .CreateItm

                Dim Modified As Boolean
                Modified = .Modified

                If Modified = True Then
                    strsql = " Update " & TmpGridDetails & " SET"
                    strsql &= " stk_desc ='" & .txtFullDesc.Text.Trim.Replace("'", "''") & "',"
                    strsql &= " stk_Subunit='" & .txtUnitDesc.Text.Trim.Replace("'", "''") & "',"
                    strsql &= " stk_unit='" & .txtSubUnitDesc.Text.Trim.Replace("'", "''") & "',"
                    strsql &= " invd_VAT='" & .VAT.Trim.Replace("'", "''") & "',"
                    strsql &= " MOD_stat='" & txtStatus.Text.Trim.Replace("'", "''") & "',"
                    If DelOrder = "Consignment Receiving Order" Or DelOrder = "Consignment Receiving Order Fashion" Then
                        strsql &= " stk_fccode= '" & (.TxtCurr.Text.Trim) & "' ,"
                    End If
                    strsql &= " new_old = 'W'  "
                    strsql &= " ,MOD_ConsOrdQty=case Meas_Sign WHEN '*' THEN case when pn2.sfp_operator='*' then   MOD_OrdQty *  pn2.SFP_UNITPACK   * STK04_Measurements.Meas_Value else   MOD_OrdQty /  pn2.SFP_UNITPACK   * STK04_Measurements.Meas_Value end "
                    strsql &= " ELSE case when pn2.sfp_operator='*' then  (  SFP_UNITPACK   * MOD_OrdQty ) /  STK04_Measurements.Meas_Value else  ( MOD_OrdQty / SFP_UNITPACK) /  STK04_Measurements.Meas_Value end  End  "
                    strsql &= " From " & TempPendingDO & "  pn2 "
                    strsql &= " left JOIN STK04_Measurements ON Meas_unit = pn2.SFP_SUBUNIT "
                    strsql &= " WHERE RECID=" & txtRecid.Text.Trim & " and  pn2.SFP_Recid = " & Val(txtSTkID.Text.Trim) & " "
                    Execute(strsql)


                    If Not dsDetDetails.Tables("tblDetails") Is Nothing Then dsDetDetails.Tables("tblDetails").Clear()
                    daDetDetails.Fill(dsDetDetails, "tblDetails")

                    INITCOL(True)
                    Exit Sub
                End If
                If CreateItm = True Then
                    Dim dt As DataTable
                    dt = dsDetDetails.Tables("tblDetails")
                    Dim dv As New DataView(dt)
                    dv.RowFilter = "RecId =Max(RecId)"
                    If dv.Count = 0 Then
                        MaxRecID = 0
                    Else
                        MaxRecID = CLng(dv(0)(0).ToString)
                    End If

                    txtRecid.Text = CStr(MaxRecID + 1)

                    txtDesc.Text = .Description
                    If DelOrder = "Receiving Order Fashion" Or DelOrder = "Consignment Receiving Order Fashion" Then

                        strsql = "INSERT INTO " & TmpFashQty
                        strsql &= " VALUES ('" & CStr(CDbl(GetMaxRecId(TmpFashQty, "Q_RECID"))) & "','" & txtRecid.Text.Trim & "',"
                        strsql &= " '" & .Grid & "'"
                        For i = 0 To MaxCnt - 1
                            strsql &= " ,NULL"
                        Next
                        strsql &= " )"
                        Execute(strsql)

                        If Not dsDetFqty.Tables("tblDetails") Is Nothing Then dsDetFqty.Tables("tblDetails").Clear()
                        daDetFqty.Fill(dsDetFqty, "tblDetails")

                        lstFashQty.AllowUpdate = True
                        RefGridSizes(.Grid)
                        PnlFashQty.Visible = True
                        txtGender.Text = .TxtGender.Text
                        txtProdGroup.Text = .ItmType
                        txtStyle.Text = .Style

                    End If
                    txtOldNew.Text = "w"
                    txtVat.Text = .VAT
                    Dim ref As String = ""
                    For i = 0 To CurrItmKey.Length - 1
                        ref &= .ArrayKey(i).Trim & "/"
                    Next
                    ref = ref.Trim.Substring(0, ref.Trim.Length - 1)
                    txtRef.Text = ref.Trim
                    txtSUBUnit.Tag = .SubUnit
                    TXTUNIT.Tag = .Unit
                    txtSUBUnit.Text = .txtSubUnitDesc.Text.Trim
                    TXTUNIT.Text = .txtUnitDesc.Text.Trim
                    txtVat.Text = .VAT
                    txtSTkID.Text = CStr(.RecID)
                    txtItmClass.Text = .txtItmClass.Text
                    'TxtItemCurr.Text = .TxtCurr.Text
                    txtPrice.Text = "0"
                    If DelOrder = "Consignment Receiving Order Fashion" Or DelOrder = "Consignment Receiving Order" Then
                        '  PnlConsigment.Visible = True
                        txtPrice.ReadOnly = False
                    End If

                    txtStatus.Text = "N"

                    'insering into tempgrid
                    strsql = " insert into " & TmpGridDetails
                    strsql &= " values(" & txtRecid.Text.Trim & " , null,'" & txtSupRef.Text.Trim.Replace("'", "''") & "' , "
                    If DelOrder = "Receiving Order Fashion" Or DelOrder = "Consignment Receiving Order Fashion" Then
                        strsql &= " null , "
                    End If
                    strsql &= " '" & txtRef.Text.Trim.Replace("'", "''") & "',"
                    If DelOrder = "Receiving Order Fashion" Or DelOrder = "Consignment Receiving Order Fashion" Then
                        strsql &= " '" & txtGender.Text.Trim.Replace("'", "''") & "',"
                    End If
                    'ch @ 06/10/2016    
                    'strsql &= "'N' , "
                    strsql &= "'N' , "
                    If DelOrder = "Receiving Order Fashion" Or DelOrder = "Consignment Receiving Order Fashion" Then
                        strsql &= " '" & txtProdGroup.Text.Trim.Replace("'", "''") & "','" & txtStyle.Text.Trim.Replace("'", "''") & "',"
                    End If
                    strsql &= " " & txtSTkID.Text.Trim & " , 1 , " 'CASE WHEN DOD_Stat = 'F' THEN 'Free' WHEN DOD_Stat = 'R' THEN 'Return' WHEN DOD_Stat = 'N' THEN 'Normal' WHEN DOD_Stat is NULL THEN 'Normal' END AS [status],  "
                    strsql &= "'" & txtDesc.Text.Trim.Replace("'", "''") & "' , "
                    If DelOrder = "Receiving Order Fashion" Or DelOrder = "Consignment Receiving Order Fashion" Then
                        strsql &= " null , null , "
                    End If
                    strsql &= " 0 ,'" & TXTUNIT.Text.Trim.Replace("'", "''") & "'  ,'" & txtSUBUnit.Text.Trim.Replace("'", "''") & "' , "
                    strsql &= " 0 , '" & txtVat.Text.Trim.Replace("'", "''") & "' "
                    strsql &= " , 0    "
                    If DelOrder = "Consignment Receiving Order" Or DelOrder = "Consignment Receiving Order Fashion" Then
                        strsql &= " , 0 , 0    "
                    End If
                    ' If DelOrder = "Receiving Order Fashion" Or DelOrder = "Consignment Receiving Order Fashion" Then
                    strsql &= " ,'" & txtCurr.Text.Trim & "'    "
                    'End If 
                    strsql &= "  , 'w', NULL  ,NULL  , '" & txtItmClass.Text.Trim.Replace("'", "''") & "' , ''  )"
                    Execute(strsql)

                    If UCase(txtItmClass.Text.Trim) = "B" Or UCase(txtItmClass.Text.Trim) = "T" Then
                        txtQty.ReadOnly = False
                    End If

                    wModify = True

                    If Not dsDetDetails.Tables("tblDetails") Is Nothing Then dsDetDetails.Tables("tblDetails").Clear()
                    daDetDetails.Fill(dsDetDetails, "tblDetails")

                    lstDelivery.Refresh()


                End If
                If .CreateItm = False Then
                    INITCOL(True)
                    txtSupRef.Focus()
                    Exit Sub
                Else
                    lcRef = False
                    If DelOrder = "Receiving Order Fashion" Or DelOrder = "Consignment Receiving Order Fashion" Or UCase(txtItmClass.Text.Trim) = "E" Then
                        If Val(txtQty.Text.Trim) > 0 Then btnBarcode.Enabled = True Else btnBarcode.Enabled = False
                        txtQty.ReadOnly = True
                    End If
                    ' Dim row As DataGridViewRow 
                     
                    btnCreate.Text = "Modify" & vbCrLf & "New Item" & vbCrLf & "Ctrl+ M"
                    lstDelivery.Row = dsDetDetails.Tables("tblDetails").Rows.IndexOf(dsDetDetails.Tables("tblDetails").Select("recid = '" & txtRecid.Text.Trim & "'")(0))
                    lstDelivery.SelectedRows.Clear()
                    lstDelivery.SelectedRows.Add(lstDelivery.Row)

                    If UCase(txtItmClass.Text.Trim) = "T" Or UCase(txtItmClass.Text.Trim) = "B" Then
                        '  GrpExpBarcode.Visible = True
                        '  LstVisbility(False, True, txtItmClass.Text.Trim)
                    ElseIf UCase(txtItmClass.Text.Trim) = "E" Then
                        GrpExpBarcode.Visible = True
                        GrpBarcodeButtons.Visible = True
                        BtnGenerateBarcodeTrading.Enabled = True
                        BtnOKExpiryBarcode.Enabled = True
                        LstVisbility(True, True, txtItmClass.Text.Trim)
                    End If
                    txtSupRef.Focus()
                End If

            End With
            btnCreate.Text = "Add" & vbCrLf & "New Item" & vbCrLf & "Ctrl+ M"

        Catch ex As Exception
            MessageBox.Show("Error : " & ex.Message, cntTitle, MessageBoxButtons.OK, MessageBoxIcon.Error)
        End Try
    End Sub

    Private Sub btnBarcode_Click(ByVal sender As System.Object, ByVal e As System.EventArgs) Handles btnBarcode.Click
        Dim txt As New TextBox
        Dim lbl As New Label
        Dim Str As String
        Dim Col As Integer
        Dim DRSQL As SqlDataReader
        Try
            If txtSTkID.Text.Trim = "" Then Exit Sub
            With lstDelivery


                Dim ColNo As Integer

                If DelOrder = "Receiving Order Fashion" Or DelOrder = "Consignment Receiving Order Fashion" Then
                    If Val(txtQty.Text.Trim) = 0 Then
                        MessageBox.Show("Quantity Couldn't be Empty.", cntTitle, MessageBoxButtons.OK, MessageBoxIcon.Warning)

                        If DelOrder = "Receiving Order Fashion" Or DelOrder = "Consignment Receiving Order Fashion" Then
                            lstFashQty.Col = lstFashQty.Splits(0).DisplayColumns.IndexOf(lstFashQty.Columns("Size1"))
                            lstFashQty.Focus()
                        Else
                            If UCase(lstDelivery.Columns("ItmClass").CellValue(lstDelivery.Row).ToString) = "E" Then
                                LstExpiry.Col = LstExpiry.Splits(0).DisplayColumns.IndexOf(LstExpiry.Columns("MOD_OrdQty"))
                                LstExpiry.Focus()
                            Else
                                lstDelivery.Col = lstDelivery.Splits(0).DisplayColumns.IndexOf(lstDelivery.Columns("MOD_OrdQty"))
                                lstDelivery.Focus()
                            End If
                        End If
                        Exit Sub
                    End If
                    LstBarcodeF.Enabled = True
                    LstBarcodeF.AllowAddNew = True
                    LstBarcodeF.AllowUpdate = True
                    LstBarcodeF.AllowDelete = True
                    BarcodeF.Width = 989
                    BarcodeF.Height = 409

                    BarcodeF.Left = 0
                    BarcodeF.Top = 125

                    BarcodeF.Visible = True
                    btnClear.Enabled = True
                    btnOK1.Enabled = True
                    BtnGenerateBarcodeFashion.Enabled = True

                    lstFashQty.UpdateData()
                    daDetFqty.Update(dsDetFqty.Tables("tblDetails"))
                    GetBarcodeF()
                    TxtGridV.Text = lstFashQty.Columns("GridCode").Text : RefreshLstBarcode()

                    btnOK1.Enabled = True : btnClear.Enabled = True
                Else


                    If DelOrder = "Receiving Order" Or DelOrder = "Consignment Receiving Order" Then
                        '   If GrpExpBarcode.Visible = True Then GrpExpBarcode.Visible = False : Exit Sub
                        GrpExpBarcode.Visible = True
                        GrpBarcodeButtons.Visible = True
                        BtnGenerateBarcodeTrading.Enabled = True
                        BtnOKExpiryBarcode.Enabled = True

                        If UCase(txtItmClass.Text.Trim) = "E" Then

                            LstVisbility(True, True, txtItmClass.Text.Trim)
                        Else
                            LstVisbility(False, True, txtItmClass.Text.Trim)
                        End If

                    End If
                    GrpBarcodeButtons.Visible = True
                    BtnGenerateBarcodeTrading.Enabled = True
                    BtnOKExpiryBarcode.Enabled = True
                End If
            End With
        Catch ex As Exception
            MessageBox.Show("Error: " & ex.Message, cntTitle, _
            MessageBoxButtons.OK, MessageBoxIcon.Error)
        End Try
    End Sub

    Private Function GetPendingInfo(ByVal ValueKey As String, Optional ByVal CostUpd As Boolean = False) As String
        Dim strsql1 As String
        Dim strsql2 As String
        Dim Str As String
        Dim drSQL1 As SqlDataReader
        Dim drSQL2 As SqlDataReader
        Dim ItemKeyBack, ItemKeyWOBack As String
        Try
            GetPendingInfo = ""
            With lstDelivery



                'Search The Item In the STK04_StkFilPending table
                strsql1 = "SELECT Pend.*,measu.meas_desc udesc , measub.meas_desc subdesc "

                strsql1 &= " FROM STK04_StkFilPending AS Pend"
                strsql1 &= " inner join stk04_measurements measu on measu.meas_unit = sfp_unit "
                strsql1 &= " inner join stk04_measurements measub on measub.meas_unit = sfp_subunit "
                strsql1 &= " WHERE SFP_ItmClass " & IIf(DelOrder = "Receiving Order Fashion" Or DelOrder = "Consignment Receiving Order Fashion", "= 'F'", "<> 'F'").ToString
                If DelOrder = "Receiving Order Fashion" Or DelOrder = "Consignment Receiving Order Fashion" Then
                    strsql1 &= " AND SFP_Season = '" & txtSeason.Text.Trim & "' AND SFP_Brand = '" & txtBrand.Text.Trim & "'"
                End If
                strsql1 &= " AND   ( "
                For i = 0 To CurrItmKeyPend.Length - 1
                    If UCase(CurrItmKeyPend(i)) <> "SFP_COMP" Then
                        strsql1 &= " Convert(varchar," & CurrItmKeyPend(i) & ")  + '/' +"
                    End If
                Next
                strsql1 = strsql1.Substring(0, strsql1.Length() - 8)
                strsql1 &= "='" & ValueKey.Trim.Replace("'", "''") & "' or "
                For i = 0 To CurrItmKeyPend.Length - 1
                    If UCase(CurrItmKeyPend(i)) <> "SFP_COMP" Then
                        strsql1 &= " Convert(varchar," & CurrItmKeyPend(i) & ")  +"
                    End If
                Next
                strsql1 = strsql1.Substring(0, strsql1.Length() - 2)
                strsql1 &= "='" & ValueKey.Trim.Replace("'", "''") & "' or "
                strsql1 &= " SFP_SingleRef1='" & ValueKey.Trim.Replace("'", "''") & "')"

                ReadFromDBToDataReader(strsql1, drSQL1)


                strsql2 = "SELECT Pend.*,measu.meas_desc udesc , measub.meas_desc subdesc "

                strsql2 &= " FROM " & TempPendingDO & " AS Pend"
                strsql2 &= " inner join stk04_measurements measu on measu.meas_unit = sfp_unit "
                strsql2 &= " inner join stk04_measurements measub on measub.meas_unit = sfp_subunit "
                strsql2 &= AppStkCond("SFP_Recid", , True, False, BUAccess.Purchase, True)
                strsql2 &= " WHERE SFP_ItmClass " & IIf(DelOrder = "Receiving Order Fashion" Or DelOrder = "Consignment Receiving Order Fashion", "= 'F'", "<> 'F'").ToString
                If DelOrder = "Receiving Order Fashion" Or DelOrder = "Consignment Receiving Order Fashion" Then
                    strsql2 &= " AND SFP_Season = '" & txtSeason.Text.Trim & "' AND SFP_Brand = '" & txtBrand.Text.Trim & "'"
                End If
                strsql2 &= " and ( "
                For i = 0 To CurrItmKeyPend.Length - 1
                    If UCase(CurrItmKeyPend(i)) <> "SFP_COMP" Then
                        strsql2 &= " Convert(varchar," & CurrItmKeyPend(i) & ")  + '/' +"
                    End If
                Next
                strsql2 = strsql2.Substring(0, strsql2.Length() - 8)
                strsql2 &= "='" & ValueKey.Trim.Replace("'", "''") & "' or "
                For i = 0 To CurrItmKeyPend.Length - 1
                    If UCase(CurrItmKeyPend(i)) <> "SFP_COMP" Then
                        strsql2 &= " Convert(varchar," & CurrItmKeyPend(i) & ") +"
                    End If
                Next
                strsql2 = strsql2.Substring(0, strsql2.Length() - 2)
                strsql2 &= "='" & ValueKey.Trim.Replace("'", "''") & "' or SFP_SingleRef1='" & ValueKey.Trim.Replace("'", "''") & "')"

                ReadFromDBToDataReader(strsql2, drSQL2)
                If drSQL2.HasRows Then
                    drSQL2.Read()

                    'Verify if the item was changed
                    'If .Columns("MOD_PendRecid").Text <> drSQL2.Item("SFP_Recid").ToString And CostUpd = False Then
                    'If .Columns("MOD_PendRecid").Text <> drSQL2.Item("SFP_Recid").ToString Then
                    '    If .Columns("MOD_PendRecid").Text <> "" Then
                    '        Str = "DELETE FROM " & TempPendingDO & " WHERE SFP_Recid = " & .Columns("MOD_PendRecid").Text
                    '        Execute(Str)
                    '    End If

                    '...refresh item Infos
                    txtRecid.Text = CStr(CDbl(GetMaxRecId(TmpGridDetails, "RECID")))
                    txtItmClass.Text = drSQL2.Item("SFP_ITMCLASS").ToString
                    txtOldNew.Text = "W"
                    Dim ref As String = ""
                    For i = 0 To CurrItmKeyPend.Length - 1
                        ref &= drSQL2.Item(CurrItmKeyPend(i)).ToString & "/"
                    Next
                    ref = ref.Trim.Substring(0, ref.Trim.Length - 1)
                    txtRef.Text = ref.Trim
                    txtSTkID.Text = drSQL2.Item("SFP_Recid").ToString
                    'ch @ 05/06/2017  no need to insert into TempPendingDO its already inserted 
                    '''insert into temp TempPendingDO
                    'InsertPendItem(TempPendingDO, txtSTkID.Text.Trim)

                    'txtSTkID.Text = CStr(CDbl(GetMaxRecId(TempPendingDO, "SFP_recid")) - 1)

                    txtDesc.Text = drSQL2.Item("SFP_desc").ToString
                    If DelOrder = "Receiving Order Fashion" Or DelOrder = "Consignment Receiving Order Fashion" Then
                        If Val(txtQty.Text.Trim) > 0 Then btnBarcode.Enabled = True Else btnBarcode.Enabled = False
                        txtGender.Text = drSQL2.Item("SFP_Gender").ToString
                        txtProdGroup.Text = drSQL2.Item("SFP_ItmType").ToString
                        txtStyle.Text = drSQL2.Item("SFP_Style").ToString

                        'adding qty fashion 
                        strsql = "INSERT INTO " & TmpFashQty
                        strsql &= " VALUES ('" & CStr(CDbl(GetMaxRecId(TmpFashQty, "Q_RECID"))) & "','" & txtRecid.Text.Trim & "',"
                        strsql &= " '" & drSQL2.Item("SFP_Grid").ToString & "'"
                        For i = 0 To MaxCnt - 1
                            strsql &= " ,NULL"
                        Next
                        strsql &= " )"
                        Execute(strsql)

                        If Not dsDetFqty.Tables("tblDetails") Is Nothing Then dsDetFqty.Tables("tblDetails").Clear()
                        daDetFqty.Fill(dsDetFqty, "tblDetails")
                        lstFashQty.AllowUpdate = True
                        RefGridSizes(drSQL2.Item("SFP_Grid").ToString)
                        PnlFashQty.Visible = True

                    Else

                        LstBarcode.AllowAddNew = True
                        LstBarcode.AllowUpdate = True
                        LstBarcode.AllowDelete = True

                        LstExpiry.AllowAddNew = True
                        LstExpiry.AllowUpdate = True
                        LstExpiry.AllowDelete = True

                        If UCase(txtItmClass.Text.Trim) = "F" Then
                            txtQty.ReadOnly = True
                            GrpExpBarcode.Visible = False

                        ElseIf UCase(txtItmClass.Text.Trim) = "E" Then
                            txtQty.ReadOnly = True
                            GrpExpBarcode.Visible = True
                            GrpBarcodeButtons.Visible = True
                            BtnGenerateBarcodeTrading.Enabled = True
                            BtnOKExpiryBarcode.Enabled = True

                            LstVisbility(True, True, txtItmClass.Text.Trim)
                            'Else
                            'txtQty.ReadOnly = False
                            'GrpExpBarcode.Visible = True
                            'LstVisbility(False, True, txtItmClass.Text.Trim)
                        End If

                    End If
                    If DelOrder = "Consignment Receiving Order" Or DelOrder = "Consignment Receiving Order Fashion" Then

                        txtPrice.ReadOnly = False
                        'txtCurr.Text = drSQL2.Item("SFP_FCCODE").ToString
                        txtPrice.Text = ""

                    End If
                    TXTUNIT.Tag = drSQL2.Item("SFP_UNIT").ToString
                    txtSUBUnit.Tag = drSQL2.Item("SFP_subUNIT").ToString
                    TXTUNIT.Text = drSQL2.Item("udesc").ToString
                    txtSUBUnit.Text = drSQL2.Item("subdesc").ToString
                    txtVat.Text = drSQL2.Item("SFP_VATType").ToString
                    txtStatus.Text = "N"

                    'insering into tempgrid

                    strsql = " insert into " & TmpGridDetails
                    strsql &= " values(" & txtRecid.Text.Trim & " , null , null ,"
                    If DelOrder = "Receiving Order Fashion" Then 'Or DelOrder = "Consignment Receiving Order Fashion" Then
                        strsql &= "null, "
                    End If
                    strsql &= " '" & txtRef.Text.Trim.Replace("'", "''") & "',"
                    If DelOrder = "Receiving Order Fashion" Then 'Or DelOrder = "Consignment Receiving Order Fashion" Then
                        strsql &= " '" & txtGender.Text.Trim.Replace("'", "''") & "',"
                    End If
                    strsql &= "'N',"
                    If DelOrder = "Receiving Order Fashion" Then 'Or DelOrder = "Consignment Receiving Order Fashion" Then
                        strsql &= " '" & txtProdGroup.Text.Trim.Replace("'", "''") & "','" & txtStyle.Text.Trim.Replace("'", "''") & "',"
                    End If
                    strsql &= " " & txtSTkID.Text.Trim & ",1 ," '  
                    strsql &= "'" & txtDesc.Text.Trim.Replace("'", "''") & "' ,"
                    If DelOrder = "Receiving Order Fashion" Then 'Or DelOrder = "Consignment Receiving Order Fashion" Then
                        strsql &= " null,null,"
                    End If
                    strsql &= " 0,'" & TXTUNIT.Text.Trim.Replace("'", "''") & "', '" & txtSUBUnit.Text.Trim.Replace("'", "''") & "', "
                    strsql &= " 0,'" & txtVat.Text.Trim.Replace("'", "''") & "' , 0 , "
                    If DelOrder = "Consignment Receiving Order" Or DelOrder = "Consignment Receiving Order Fashion" Then
                        strsql &= " 0   , 0   ,"
                    End If
                    'ch @ 06/10/2016
                    ' If DelOrder = "Receiving Order Fashion" Or DelOrder = "Consignment Receiving Order Fashion" Then
                    ' End If
                    'If DelOrder = "Consignment Receiving Order" Or DelOrder = "Consignment Receiving Order Fashion" Then
                    '    strsql &= " ,'" & txtCurr.Text.Trim & "' ,0 "
                    'End If
                    'If DelOrder = "Receiving Order Fashion" Or DelOrder = "Consignment Receiving Order Fashion" Then

                    'End If
                    strsql &= "  '" & txtCurr.Text.Trim & "' ,'w',null , null, '" & txtItmClass.Text.Trim.Replace("'", "''") & "','' )"
                    Execute(strsql)

                    wModify = True

                    If Not dsDetDetails.Tables("tblDetails") Is Nothing Then dsDetDetails.Tables("tblDetails").Clear()
                    daDetDetails.Fill(dsDetDetails, "tblDetails")

                    lstDelivery.Refresh()

                    ' End If
                    Select Case drSQL2.Item("SFP_OpSource").ToString
                        Case "PO" : GetPendingInfo = "Purchase Order"
                        Case "PI" : GetPendingInfo = "Purchase Invoice"
                        Case "DO" : GetPendingInfo = "Receiving Order"
                            'ch @ 23/09/2016
                        Case "RQ" : GetPendingInfo = "Requisition Order"
                        Case "ST" : GetPendingInfo = "Supplier Template"
                        Case Else : GetPendingInfo = ""
                    End Select
                    If drSQL2.Item("SFP_Status").ToString = "N" Then GetPendingInfo = "This PO"


                ElseIf drSQL1.HasRows Then
                    drSQL1.Read()

                    'Verify if the item was changed
                    'If .Columns("MOD_PendRecid").Text <> drSQL1.Item("SFP_Recid").ToString Then
                    '    If .Columns("MOD_PendRecid").Text <> "" Then
                    '        Str = "DELETE FROM " & TempPendingDO & " WHERE SFP_Recid = " & .Columns("MOD_PendRecid").Text
                    '        Execute(Str)
                    '    End If

                    '...refresh item Infos
                    txtRecid.Text = CStr(CDbl(GetMaxRecId(TmpGridDetails, "RECID")))
                    txtItmClass.Text = drSQL1.Item("SFP_ITMCLASS").ToString

                    Dim ref As String = ""
                    For i = 0 To CurrItmKey.Length - 1
                        ref &= drSQL1.Item(CurrItmKeyPend(i)).ToString & "/"
                    Next
                    ref = ref.Trim.Substring(0, ref.Trim.Length - 1)
                    txtRef.Text = ref.Trim
                    txtSTkID.Text = drSQL1.Item("SFP_Recid").ToString
                    '''insert into temp TempPendingDO
                    InsertPendItem(TempPendingDO, txtSTkID.Text.Trim)

                    txtSTkID.Text = CStr(CDbl(GetMaxRecId(TempPendingDO, "SFP_RECID")) - 1)

                    txtDesc.Text = drSQL1.Item("SFP_desc").ToString
                    If DelOrder = "Receiving Order Fashion" Or DelOrder = "Consignment Receiving Order Fashion" Then

                        txtGender.Text = drSQL1.Item("SFP_Gender").ToString
                        txtProdGroup.Text = drSQL1.Item("SFP_ItmType").ToString
                        txtStyle.Text = drSQL1.Item("SFP_Style").ToString

                        'adding qty fashion 
                        strsql = "INSERT INTO " & TmpFashQty
                        strsql &= " VALUES ('" & CStr(CDbl(GetMaxRecId(TmpFashQty, "Q_RECID"))) & "','" & txtRecid.Text.Trim & "',"
                        strsql &= " '" & drSQL1.Item("SFP_Grid").ToString & "'"
                        For i = 0 To MaxCnt - 1
                            strsql &= " ,NULL"
                        Next
                        strsql &= " )"
                        Execute(strsql)
                        If Not dsDetFqty.Tables("tblDetails") Is Nothing Then dsDetFqty.Tables("tblDetails").Clear()
                        daDetFqty.Fill(dsDetFqty, "tblDetails")
                        lstFashQty.AllowUpdate = True
                        RefGridSizes(drSQL1.Item("SFP_Grid").ToString)
                        PnlFashQty.Visible = True
                    Else

                        LstBarcode.AllowAddNew = True
                        LstBarcode.AllowUpdate = True
                        LstBarcode.AllowDelete = True

                        LstExpiry.AllowAddNew = True
                        LstExpiry.AllowUpdate = True
                        LstExpiry.AllowDelete = True

                        If UCase(txtItmClass.Text.Trim) = "F" Then
                            txtQty.ReadOnly = True
                            GrpExpBarcode.Visible = False

                        ElseIf UCase(txtItmClass.Text.Trim) = "E" Then
                            txtQty.ReadOnly = True
                            GrpExpBarcode.Visible = True
                            GrpBarcodeButtons.Visible = True
                            BtnGenerateBarcodeTrading.Enabled = True
                            BtnOKExpiryBarcode.Enabled = True

                            LstVisbility(True, True, txtItmClass.Text.Trim)
                            ' Else
                            'txtQty.ReadOnly = False
                            'GrpExpBarcode.Visible = True
                            'LstVisbility(False, True, txtItmClass.Text.Trim)
                        End If

                    End If
                    TXTUNIT.Text = drSQL1.Item("udesc").ToString
                    txtSUBUnit.Text = drSQL1.Item("subdesc").ToString
                    TXTUNIT.Tag = drSQL1.Item("SFP_UNIT").ToString
                    txtSUBUnit.Tag = drSQL1.Item("SFP_subUNIT").ToString
                    txtVat.Text = drSQL1.Item("SFP_VATType").ToString
                    'TxtItemCurr.Text = drSQL1.Item("SFP_FcCode").ToString
                    txtStatus.Text = "N"
                    'insering into tempgrid

                    strsql = " insert into " & TmpGridDetails
                    strsql &= " values(" & txtRecid.Text.Trim & " , null , null ,"
                    If DelOrder = "Receiving Order Fashion" Then ' Or DelOrder = "Consignment Receiving Order Fashion" Then
                        strsql &= "null, "
                    End If
                    strsql &= " '" & txtRef.Text.Trim.Replace("'", "''") & "',"
                    If DelOrder = "Receiving Order Fashion" Then ' Or DelOrder = "Consignment Receiving Order Fashion" Then
                        strsql &= " '" & txtGender.Text.Trim.Replace("'", "''") & "',"
                    End If
                    strsql &= "'N',"
                    If DelOrder = "Receiving Order Fashion" Then '  Or DelOrder = "Consignment Receiving Order Fashion" Then
                        strsql &= " '" & txtProdGroup.Text.Trim.Replace("'", "''") & "','" & txtStyle.Text.Trim.Replace("'", "''") & "',"
                    End If
                    strsql &= " '" & txtSTkID.Text.Trim & "',1 ," '  
                    strsql &= "'" & txtDesc.Text.Trim.Replace("'", "''") & "' ,"
                    If DelOrder = "Receiving Order Fashion" Then ' Or DelOrder = "Consignment Receiving Order Fashion" Then
                        strsql &= " null,null,"
                    End If
                    strsql &= " 0, '" & TXTUNIT.Text.Trim.Replace("'", "''") & "','" & txtSUBUnit.Text.Trim.Replace("'", "''") & "', "
                    strsql &= " 0,'" & txtVat.Text.Trim.Replace("'", "''") & "' "

                    strsql &= "   , 0     "
                    If DelOrder = "Consignment Receiving Order" Or DelOrder = "Consignment Receiving Order Fashion" Then
                        strsql &= " , 0   , 0     "
                    End If
                    strsql &= " , '" & txtCurr.Text.Trim & "','w', NULL,NULL, '" & txtItmClass.Text.Trim.Replace("'", "''") & "','' )"
                    Execute(strsql)

                    wModify = True

                    If Not dsDetDetails.Tables("tblDetails") Is Nothing Then dsDetDetails.Tables("tblDetails").Clear()
                    daDetDetails.Fill(dsDetDetails, "tblDetails")

                    lstDelivery.Refresh()


                    Select Case drSQL1.Item("SFP_OpSource").ToString
                        Case "PO" : GetPendingInfo = "Purchase Order"
                        Case "PI" : GetPendingInfo = "Purchase Invoice"
                        Case "DO" : GetPendingInfo = "Receiving Order"
                            'ch @ 23/09/2016
                        Case "RQ" : GetPendingInfo = "Receiving Order"
                        Case "ST" : GetPendingInfo = "Supplier Template"
                        Case Else : GetPendingInfo = ""
                    End Select

                    strsql = "SELECT SFP_Recid FROM " & TempPendingDO
                    strsql &= " WHERE SFP_Status = 'N' AND"
                    strsql &= " SFP_Recid = '" & drSQL1.Item("SFP_Recid").ToString & "'"
                    If RecordsExists(strsql) Then GetPendingInfo = "This PO"
                    'Else
                    '    GetPendingInfo = "This PO"
                    'End If
                    wModify = True
                Else
                    '..last MO & Pur Inv
                    '.Columns("Last_Confirmed").Text = "0"
                    ''..last MO & Pur Inv
                    '.Columns("LAST_ORDERED").Text = "0"
                    '.Columns("LAST_PURCHASED").Text = "0"

                    GetPendingInfo = "NO ITEM FOUND"
                End If
                lcRef = False
            End With
        Catch ex As Exception
            Throw ex
        Finally
            If Not drSQL1 Is Nothing Then
                If drSQL1.IsClosed = False Then drSQL1.Close()
                drSQL1 = Nothing
            End If
            If Not drSQL2 Is Nothing Then
                If drSQL2.IsClosed = False Then drSQL2.Close()
                drSQL2 = Nothing
            End If
        End Try
    End Function
    Private Sub lstFashQty_KeyPress(ByVal sender As Object, ByVal e As System.Windows.Forms.KeyPressEventArgs) Handles lstFashQty.KeyPress
        Dim txt As New TextBox
        Try
            txt.Text = lstFashQty.Columns(lstFashQty.Col).Text
            If DelOrder = "Receiving Order Fashion" Or DelOrder = "Consignment Receiving Order Fashion" Then
                For J = 0 To MaxCnt
                    If lstFashQty.Columns(lstFashQty.Col).DataField = "Size" & J Then
                        e.Handled = ValidGet(txt, e.KeyChar, StringType.Numeric, 18, 0)
                    End If
                Next
            End If
        Catch ex As Exception
            MessageBox.Show("Error: " & ex.Message, cntTitle, _
            MessageBoxButtons.OK, MessageBoxIcon.Error)
        End Try
    End Sub


    Private Sub lstDelivery_FilterChange(ByVal sender As Object, ByVal e As System.EventArgs) Handles lstDelivery.FilterChange

        Try
            If lstDelivery.FilterActive = True Then
                lstDelivery.AllowFilter = False : GridFilterChange(lstDelivery)
            End If
            If DelOrder = "Consignment Receiving Order Fashion" Or DelOrder = "Receiving Order Fashion" Then
                If lstDelivery.Columns("Color").Text.Trim <> "" Then
                    lstDelivery.Columns("Color").FooterText = GetFieldDesc("Stk04_COLOR", " Color_Code='" & lstDelivery.Columns("Color").Text.Trim & "'", "Color_Reference")
                Else
                    lstDelivery.Columns("Color").FooterText = ""
                End If
            End If
            ObjSumNormal = dsDetDetails.Tables("tblDetails").Compute("Sum( MOD_OrdQty)", " MOD_stat='N' " & IIf(CType(lstDelivery.DataSource, DataView).RowFilter <> "", " and " & CType(lstDelivery.DataSource, DataView).RowFilter, "").ToString)
            ObjSumFree = dsDetDetails.Tables("tblDetails").Compute("Sum( MOD_OrdQty)", " MOD_stat='F' " & IIf(CType(lstDelivery.DataSource, DataView).RowFilter <> "", " and " & CType(lstDelivery.DataSource, DataView).RowFilter, "").ToString)
            ObjSumReturn = dsDetDetails.Tables("tblDetails").Compute("Sum( MOD_OrdQty)", " MOD_stat='R' " & IIf(CType(lstDelivery.DataSource, DataView).RowFilter <> "", " and " & CType(lstDelivery.DataSource, DataView).RowFilter, "").ToString)
            objSum = CDbl(IIf(ObjSumNormal Is DBNull.Value, "0", ObjSumNormal)) + CDbl(IIf(ObjSumFree Is DBNull.Value, "0", ObjSumFree)) - CDbl(IIf(ObjSumReturn Is DBNull.Value, "0", ObjSumReturn))
            lstDelivery.Splits(0).DisplayColumns("MOD_OrdQty").DataColumn.FooterText = CType(objSum, Double).ToString("N" & gbDecNo)
            If DelOrder = "Consignment Receiving Order" Or DelOrder = "Consignment Receiving Order Fashion" Then
                ObjSumNormal = dsDetDetails.Tables("tblDetails").Compute("Sum( Net)", " MOD_stat='N' " & IIf(CType(lstDelivery.DataSource, DataView).RowFilter <> "", " and " & CType(lstDelivery.DataSource, DataView).RowFilter, "").ToString)
                ObjSumFree = dsDetDetails.Tables("tblDetails").Compute("Sum( Net)", " MOD_stat='F' " & IIf(CType(lstDelivery.DataSource, DataView).RowFilter <> "", " and " & CType(lstDelivery.DataSource, DataView).RowFilter, "").ToString)
                ObjSumReturn = dsDetDetails.Tables("tblDetails").Compute("Sum( Net)", " MOD_stat='R'" & IIf(CType(lstDelivery.DataSource, DataView).RowFilter <> "", " and " & CType(lstDelivery.DataSource, DataView).RowFilter, "").ToString)
                objSum = CDbl(IIf(ObjSumNormal Is DBNull.Value, "0", ObjSumNormal)) + CDbl(IIf(ObjSumFree Is DBNull.Value, "0", ObjSumFree)) - CDbl(IIf(ObjSumReturn Is DBNull.Value, "0", ObjSumReturn))
                lstDelivery.Splits(0).DisplayColumns("Net").DataColumn.FooterText = CType(objSum, Double).ToString("N3")
            End If
        Catch ex As Exception
            MessageBox.Show("Error: " & ex.Message, cntTitle, MessageBoxButtons.OK, MessageBoxIcon.Error)
        End Try
    End Sub

    Private Sub lstDelivery_KeyDown(ByVal sender As Object, ByVal e As System.Windows.Forms.KeyEventArgs) Handles lstDelivery.KeyDown
        Try

            Select Case e.KeyCode

                Case Keys.Escape : GrpExpBarcode.Visible = False
            End Select


        Catch ex As Exception
            MessageBox.Show("General Error : " & ex.Message, Me.Text, MessageBoxButtons.OK, MessageBoxIcon.Warning)
        End Try
    End Sub

    Private Sub txtColor_KeyPress(ByVal sender As Object, ByVal e As System.Windows.Forms.KeyPressEventArgs) Handles txtColor.KeyPress
        Try
            If Asc(e.KeyChar) = 22 Then
                e.Handled = True : ctMenuPast(Me)
            Else
                e.Handled = ValidGet(CType(sender, TextBox), e.KeyChar, StringType.Other, 2, , True)
            End If
        Catch ex As Exception
            MessageBox.Show("Error: " & ex.Message, cntTitle, MessageBoxButtons.OK, MessageBoxIcon.Error)
        End Try
    End Sub

    Private Sub txtSupColor_KeyPress(ByVal sender As Object, ByVal e As System.Windows.Forms.KeyPressEventArgs) Handles txtSupColor.KeyPress
        Try
            If Asc(e.KeyChar) = 22 Then
                e.Handled = True : ctMenuPast(Me)
            Else
                e.Handled = ValidGet(CType(sender, TextBox), e.KeyChar, StringType.Other, 30, , True)
            End If
        Catch ex As Exception
            MessageBox.Show("Error: " & ex.Message, cntTitle, MessageBoxButtons.OK, MessageBoxIcon.Error)
        End Try
    End Sub

    Private Sub txtSupRef_KeyPress(ByVal sender As Object, ByVal e As System.Windows.Forms.KeyPressEventArgs) Handles txtSupRef.KeyPress
        Try
            If Asc(e.KeyChar) = 22 Then
                e.Handled = True : ctMenuPast(Me)
            Else
                e.Handled = ValidGet(CType(sender, TextBox), e.KeyChar, StringType.Other, 30, , True)
            End If
        Catch ex As Exception
            MessageBox.Show("Error: " & ex.Message, cntTitle, MessageBoxButtons.OK, MessageBoxIcon.Error)
        End Try
    End Sub

    Private Sub txtQty_KeyPress(ByVal sender As Object, ByVal e As System.Windows.Forms.KeyPressEventArgs) Handles txtQty.KeyPress
        Try
            If Not DtQualityControlHeader Is Nothing AndAlso txtRecid.Text.Trim <> "" Then
                If DtQualityControlHeader.Select("PrDReciD =  " & txtRecid.Text & " and (isnull(AccQty,0) <> 0 or isnull(RjQty,0) <> 0 )").Count <> 0 Then
                    e.Handled = True
                    MessageBox.Show("You Can Only Change Your Quantity From Accepted Quantity In Quantity Control Form", cntTitle, MessageBoxButtons.OK, MessageBoxIcon.Information)
                    Exit Try
                End If
            End If

            If Asc(e.KeyChar) = 22 Then
                e.Handled = True : ctMenuPast(Me, "Long")
            Else
                e.Handled = ValidGet(CType(sender, TextBox), e.KeyChar, StringType.Numeric, 15, 3, True)
            End If
        Catch ex As Exception
            MessageBox.Show("Error: " & ex.Message, cntTitle, MessageBoxButtons.OK, MessageBoxIcon.Error)
        End Try
    End Sub

    Private Sub lstDelivery_FetchCellStyle(ByVal sender As Object, ByVal e As C1.Win.C1TrueDBGrid.FetchCellStyleEventArgs) Handles lstDelivery.FetchCellStyle
        Try
            'If UCase(lstDelivery.Columns("ColorN_O").CellValue(e.Row).ToString) = "N" Then
            '    e.CellStyle.ForeColor = Color.Red
            'Else
            '    e.CellStyle.ForeColor = Color.Navy
            'End If
        Catch ex As Exception
            MessageBox.Show("Error: " & ex.Message, cntTitle, MessageBoxButtons.OK, MessageBoxIcon.Error)
        End Try
    End Sub

    Private Sub LstBarcodeF_KeyPress(ByVal sender As Object, ByVal e As System.Windows.Forms.KeyPressEventArgs) Handles LstBarcodeF.KeyPress

    End Sub

    Private Sub txtCode_KeyPress(ByVal sender As Object, ByVal e As System.Windows.Forms.KeyPressEventArgs) Handles txtCode.KeyPress
        Try
            If Asc(e.KeyChar) = 22 Then
                e.Handled = True : ctMenuPast(Me, "Long")
            Else
                e.Handled = ValidGet(CType(sender, TextBox), e.KeyChar, StringType.Numeric, 15, , False, )
            End If
        Catch ex As Exception
            MessageBox.Show("Error: " & ex.Message, cntTitle, MessageBoxButtons.OK, MessageBoxIcon.Error)
        End Try
    End Sub

    Private Sub txtQty_TextChanged(ByVal sender As System.Object, ByVal e As System.EventArgs) Handles txtQty.TextChanged

    End Sub

    Private Sub lstFashQty_MouseDown(ByVal sender As Object, ByVal e As System.Windows.Forms.MouseEventArgs) Handles lstFashQty.MouseDown, LstExpiry.MouseDown
        Dim tdb As C1TrueDBGrid
        Try
            tdb = CType(sender, C1TrueDBGrid)
            If e.Button = MouseButtons.Right AndAlso tdb.Controls.Count > 2 Then
                tdb.Controls(2).ContextMenu = NullConMenu
            End If
        Catch ex As Exception
            MessageBox.Show("Error : " & ex.Message, cntTitle, MessageBoxButtons.OK, MessageBoxIcon.Error)
        End Try
    End Sub

    Private Sub btnCurr_Click(ByVal sender As System.Object, ByVal e As System.EventArgs) Handles btnCurr.Click
        Dim strsql As String
        Dim cnSql As SqlConnection
        Dim dsSql As DataSet
        Dim daSql As SqlDataAdapter
        Dim frm As frmSearch
        Dim Sizefield() As Int16 = {40, 160}
        Dim Alnfield() As String = {"C", "N"}
        Dim Retfield() As String = {"Currency", "Description"}

        Try

            Cursor.Current = Cursors.WaitCursor
            strsql = "select Cur_code as [Currency], Cur_Desc as [Description] from Com_Currency"
            cnSql = New SqlConnection(ConnectionString)
            cnSql.Open()
            daSql = New SqlDataAdapter(strsql, cnSql)
            dsSql = New DataSet
            daSql.Fill(dsSql, "tbcode3")
            cnSql.Close()
            frm = New frmSearch

            With frm
                .Owner = Me
                If .ShowDialog("Return", dsSql.Tables("tbcode3"), dsSql.Tables("tbcode3").Rows.Count, _
                "Currency", Sizefield, Alnfield, Retfield, 0, Nothing, False, 250, 300, , True) = DialogResult.OK Then
                    If gbRetSearch(0) = "" Then
                        txtCode.Focus()
                    Else
                        If gbRetSearch(0) <> "" Then
                            txtCurr.Text = gbRetSearch(0)
                            'LblCostCurr.Text = gbRetSearch(0)
                            txtCurr.Focus()
                            SendKeys.Send("{TAB}")
                        Else
                            txtCurr.Focus()
                        End If
                    End If
                End If
            End With

        Catch exp As Exception
            MessageBox.Show("Error : " & exp.Message, "", _
            MessageBoxButtons.OK, MessageBoxIcon.Error)
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

    Private Sub txtCurr_KeyDown(ByVal sender As Object, ByVal e As System.Windows.Forms.KeyEventArgs) Handles txtCurr.KeyDown
        Try
            If e.Modifiers = Keys.Alt AndAlso e.KeyCode = Keys.Down AndAlso btnCurr.Visible = True AndAlso btnCurr.Focus Then
                btnCurr_Click(btnCurr, New System.EventArgs)
            End If
        Catch ex As Exception
            MessageBox.Show("Error: " & ex.Message, cntTitle, MessageBoxButtons.OK, MessageBoxIcon.Error)
        End Try
    End Sub

    Private Sub txtCurr_KeyPress(ByVal sender As Object, ByVal e As System.Windows.Forms.KeyPressEventArgs) Handles txtCurr.KeyPress
        Try
            If Asc(e.KeyChar) = 22 Then
                e.Handled = True : ctMenuPast(Me)
            Else
                e.Handled = ValidGet(CType(sender, TextBox), e.KeyChar, StringType.Other, 3, , False)
            End If
        Catch ex As Exception
            MessageBox.Show("Error: " & ex.Message, cntTitle, MessageBoxButtons.OK, MessageBoxIcon.Error)
        End Try
    End Sub

    Private Sub txtCurr_Validating(ByVal sender As Object, ByVal e As System.ComponentModel.CancelEventArgs) Handles txtCurr.Validating
        Try
            If Trim(txtCurr.Text) = "" Then
                Exit Sub
            End If
            If txtCurr.Text.IndexOf("'") >= 0 Then
                e.Cancel = True
                MessageBox.Show("Invalid Data Entry, ['] Not Allowed.", cntTitle, _
                MessageBoxButtons.OK, MessageBoxIcon.Question)
                Exit Try
            End If

            strsql = "select * from Com_Currency where Cur_code ='" & txtCurr.Text.Replace("'", "''") & "'"
            Dim drsql As SqlDataReader
            ReadFromDBToDataReader(strsql, drsql)
            With drsql
                If .HasRows Then
                    .Read()
                    If txtSTkID.Text.Trim <> "" Then
                        'strsql = "select STK_FCCODE,STK_AVGCOSTCC,STK_AVGCOSTFC,STK_AVGCOSTLC from Stk04_stkfil where stk_recid=" & txtSTkID.Text.Trim
                        'Dim DRSS As SqlDataReader
                        'ReadFromDBToDataReader(strsql, DRSS)
                        'If DRSS.HasRows ThenGrpExpBarcode
                        '    DRSS.Read()
                        '    txtCurr.Text = DRSS.Item("STK_FCCODE").ToString
                        '    If txtCurr.Text.Trim = gbCurr1 Then
                        '        txtPrice.Text = DRSS.Item("STK_AVGCOSTLC").ToString
                        '    ElseIf txtCurr.Text.Trim = gbCurr2 Then
                        '        txtPrice.Text = DRSS.Item("STK_AVGCOSTCC").ToString
                        '    Else
                        '        txtPrice.Text = DRSS.Item("STK_AVGCOSTFC").ToString
                        '    End If
                        'End If
                    End If
                Else
                    txtCurr.Text = ""
                    'LblCostCurr.Text = ""
                    MessageBox.Show("Code does Not Exist", cntTitle, MessageBoxButtons.OK, MessageBoxIcon.Exclamation)
                    txtCurr.Focus()
                End If
            End With
        Catch ex As Exception
            MessageBox.Show("Error : " & ex.Message, cntTitle, MessageBoxButtons.OK, MessageBoxIcon.Error)
        End Try
    End Sub


    Private Sub txtPrice_KeyPress(ByVal sender As Object, ByVal e As System.Windows.Forms.KeyPressEventArgs) Handles txtPrice.KeyPress
        Try
            If Asc(e.KeyChar) = 22 Then
                e.Handled = True : ctMenuPast(Me, "Long")
            Else
                If txtCurr.Text.Trim = gbCurr1 Then
                    e.Handled = ValidGet(CType(sender, TextBox), e.KeyChar, StringType.Numeric, 18, 0, False)
                Else
                    e.Handled = ValidGet(CType(sender, TextBox), e.KeyChar, StringType.Numeric, 18, 3, False)
                End If
            End If
        Catch ex As Exception
            MessageBox.Show("Error : " & ex.Message, cntTitle, MessageBoxButtons.OK, MessageBoxIcon.Error)
        End Try
    End Sub
    Private Sub txtDisc_KeyPress(ByVal sender As Object, ByVal e As System.Windows.Forms.KeyPressEventArgs) Handles TxtDisc.KeyPress
        Try
            If Asc(e.KeyChar) = 22 Then
                e.Handled = True : ctMenuPast(Me, "Numeric", , , , False)
            Else
                e.Handled = ValidGet(CType(sender, TextBox), e.KeyChar, StringType.Numeric, 5, 2, False, )
            End If
        Catch ex As Exception
            MessageBox.Show("Error : " & ex.Message, cntTitle, MessageBoxButtons.OK, MessageBoxIcon.Error)
        End Try
    End Sub
    Private Sub txtPrice_Validating(ByVal sender As Object, ByVal e As System.ComponentModel.CancelEventArgs) Handles txtPrice.Validating, TxtDisc.Validating
        Try
            If sender Is txtPrice Then
                If txtPrice.Text.Trim <> "" AndAlso Not IsNumeric(txtPrice.Text) Then
                    e.Cancel = True
                    MessageBox.Show("Invalid value.", cntTitle, _
                    MessageBoxButtons.OK, MessageBoxIcon.Exclamation)
                    Exit Sub
                End If
                If txtPrice.Text.Length > 15 Then
                    e.Cancel = True
                    MessageBox.Show("Invalid value.", cntTitle, _
                    MessageBoxButtons.OK, MessageBoxIcon.Exclamation)
                ElseIf txtPrice.Text.Trim <> "" Then
                    txtPrice.Text = CDbl(txtPrice.Text).ToString("N3")
                End If
            Else
                If TxtDisc.Text.Trim <> "" AndAlso IsNumeric(TxtDisc.Text.Trim) Then
                    If CDbl(TxtDisc.Text.Trim) < 0 Then
                        e.Cancel = True
                        MessageBox.Show("Invalid Discount ", cntTitle, _
                   MessageBoxButtons.OK, MessageBoxIcon.Exclamation)
                    ElseIf CDbl(TxtDisc.Text.Trim) > 100 Then
                        e.Cancel = True
                        MessageBox.Show("Discount Should Be Less Than 100 %", cntTitle, _
                 MessageBoxButtons.OK, MessageBoxIcon.Exclamation)
                    End If
                End If
            End If
            NetCalculate()
        Catch ex As Exception
            MessageBox.Show("Error : " & ex.Message, cntTitle, MessageBoxButtons.OK, MessageBoxIcon.Error)
        End Try
    End Sub
    Private Sub LstBarcode_BeforeInsert(ByVal sender As Object, ByVal e As C1.Win.C1TrueDBGrid.CancelEventArgs) Handles LstBarcode.BeforeInsert
        Dim drs As DataRow()
        Dim MaxRecID As Long
        Try
            drs = dsDetBarcode.Tables("tblDetails").Select("recid='" & txtRecid.Text.Trim & "' AND bar_stat<>'D'")

            If UCase(txtItmClass.Text.Trim) = "E" Then
                If drs.Length >= Val(LstExpiry.Columns("QTY").Text.Trim) Then
                    MessageBox.Show("Barcode Number Can't Be Bigger Than Expiry Quantity.", cntTitle, MessageBoxButtons.OK, MessageBoxIcon.Exclamation)
                    LstExpiry.Columns("QTY").Text = CStr(drs.Length)
                    e.Cancel = True
                    BInsBarcodeCancel = True
                    Exit Sub
                End If
            Else
                If drs.Length >= Val(txtQty.Text.Trim) Then
                    MessageBox.Show("Barcode Number Can't Be Bigger Than Quantity.", cntTitle, MessageBoxButtons.OK, MessageBoxIcon.Exclamation)
                    If Val(txtQty.Text.Trim) = 0 Then
                        txtQty.Focus()
                    End If
                    txtQty.Text = CStr(drs.Length)
                    NetCalculate()
                    If Val(txtQty.Text.Trim) > 0 Then btnBarcode.Enabled = True Else btnBarcode.Enabled = False
                    e.Cancel = True
                    BInsBarcodeCancel = True
                    Exit Sub
                End If
            End If
            BInsBarcodeCancel = False
        Catch ex As Exception
            MessageBox.Show("Error : " & ex.Message, cntTitle, MessageBoxButtons.OK, MessageBoxIcon.Error)
        End Try
    End Sub

    Private Sub lstDelivery_AfterFilter(ByVal sender As Object, ByVal e As C1.Win.C1TrueDBGrid.FilterEventArgs) Handles lstDelivery.AfterFilter
        Try
            lstDelivery.Row = 0
            lstDelivery.SelectedRows.Clear()
            lstDelivery.SelectedRows.Add(0)

        Catch ex As Exception
            MessageBox.Show("Error : " & ex.Message, cntTitle, _
                             MessageBoxButtons.OK, MessageBoxIcon.Error)
        End Try
    End Sub

    Private Function FashItemAlreadyExist() As Boolean
        Try
            FashItemAlreadyExist = True

            If txtSTkID.Text.Trim <> "" Then Exit Function
            If txtColor.Text.Trim <> "" Then Exit Function

            strsql = " SELECT * FROM " & TmpGridDetails & " WHERE COLOR='" & txtColor.Text.Trim & "' AND MOD_RMRecid='" & txtSTkID.Text.Trim & "'"
            If RecordsExists(strsql) = True Then
                MessageBox.Show("Item Already Exists.", cntTitle, MessageBoxButtons.OK, MessageBoxIcon.Exclamation)
                FashItemAlreadyExist = False
            End If

        Catch ex As Exception
            Throw ex
        End Try
    End Function

    Private Sub Print_KeyDown(ByVal sender As Object, ByVal e As System.Windows.Forms.KeyEventArgs) Handles Print.KeyDown
        Try
            If Print.Visible = True Then
                If e.KeyCode = Keys.Enter Then
                    If BtnOk.Focus Then BtnOk_Click(BtnOk, New System.EventArgs)
                End If
            End If
        Catch ex As Exception
            MessageBox.Show("Error : " & ex.Message, cntTitle, MessageBoxButtons.OK, MessageBoxIcon.Error)
        End Try
    End Sub

    Private Sub BtnOKExpiryBarcode_Click(ByVal sender As System.Object, ByVal e As System.EventArgs) Handles BtnOKExpiryBarcode.Click
        Try

            ClearGridFiltration(LstExpiry)
            '  If e.KeyCode = Keys.Enter Then
            If Wtype = "" Or Wtype = "DELETE" Then Exit Sub
            'If txtOldNew.Text = "w" Then Exit Sub
            'If UCase(lstDelivery.Columns("new_old").Text.Trim) = "O" Or UCase(lstDelivery.Columns("new_old").Text.Trim) = "W" Then Exit Sub
            If txtSTkID.Text.Trim = "" Then Exit Sub
            If txtRef.Text.Trim = "" Then txtRef.Focus() : Exit Sub


            LstExpiry.UpdateData()
            daDetExpiry.Update(dsDetExpiry.Tables("tblDetails"))
            LstBarcode.UpdateData()
            daDetBarcode.Update(dsDetBarcode.Tables("tblDetails"))
            If ColorExpiryError = True Then
                MessageBox.Show("The Expiry Date And - No Can't Be Null.", cntTitle, MessageBoxButtons.OK, MessageBoxIcon.Exclamation)
                LstExpiry.Focus()
                Exit Sub
            End If
            If DelOrder = "Receiving Order" Or DelOrder = "Consignment Receiving Order" Then
                Dim drs As DataRow()
                drs = dsDetBarcode.Tables("tblDetails").Select("recid='" & txtRecid.Text.Trim & "' AND bar_stat<>'D'")

                If UCase(txtItmClass.Text.Trim) = "E" Then
                    If drs.Length > Val(LstExpiry.Columns("QTY").Text.Trim) Then
                        MessageBox.Show("Barcode Number Can't Be Bigger Than Expiry Quantity.", cntTitle, MessageBoxButtons.OK, MessageBoxIcon.Exclamation)
                        LstExpiry.Columns("QTY").Text = CStr(drs.Length)
                        Exit Sub
                    End If
                    If Not DtQualityControlHeader Is Nothing Then
                        If DtQualityControlHeader.Select("PrDReciD =  '" & LstExpiry.Columns("RECID").Text.Trim & "' and  PrQDRecId =  '" & LstExpiry.Columns("E_RECID").Text.Trim & "'  and (isnull(AccQty,0) <> 0 or isnull(RjQty,0) <> 0) ").Count = 0 Then
                            If Val(LstExpiry.Columns("QTY").Text.Trim) = 0 Then
                                MessageBox.Show("Quantity Can't Be Null.", cntTitle, MessageBoxButtons.OK, MessageBoxIcon.Exclamation)
                                Exit Sub
                            End If
                        End If
                    Else

                        If Val(LstExpiry.Columns("QTY").Text.Trim) = 0 Then
                            MessageBox.Show("Quantity Can't Be Null.", cntTitle, MessageBoxButtons.OK, MessageBoxIcon.Exclamation)
                            Exit Sub
                        End If
                    End If

                    If dsDetExpiry.Tables("tblDetails").Select("Recid =  " & LstExpiry.Columns("RECID").Text.Trim & " and (exp_stat = '' or isnull(DOB_BatchInfo,'') = '' or isnull(DOB_ExpiryDate,'') = '') ").Count > 0 Then
                        MessageBox.Show("Expiry Date And Lot # Can't Be Null.", cntTitle, MessageBoxButtons.OK, MessageBoxIcon.Exclamation)
                        Exit Sub
                    End If

                Else
                    If drs.Length > Val(txtQty.Text.Trim) Then
                        MessageBox.Show("Barcode Number Can't Be Bigger Than Quantity.", cntTitle, MessageBoxButtons.OK, MessageBoxIcon.Exclamation)
                        If Val(txtQty.Text.Trim) = 0 Then
                            txtQty.Focus()
                        End If
                        txtQty.Text = CStr(drs.Length)
                        NetCalculate()
                        If Val(txtQty.Text.Trim) > 0 Then btnBarcode.Enabled = True Else btnBarcode.Enabled = False
                        Exit Sub
                    End If
                End If
            End If
            If UCase(txtItmClass.Text.Trim) = "E" And (sender Is LstExpiry Or sender Is LstBarcode) Then
                Dim sFltr As String
                Dim sTotal As Double
                Dim objSum As Object
                With LstExpiry
                    LstExpiry.UpdateData()
                    daDetExpiry.Update(dsDetExpiry.Tables("tblDetails"))
                    If Val(LstExpiry.Columns("MOD_OrdQty").Text.Trim) <= 0 Then
                        LstExpiry.Focus()

                        Exit Sub

                    End If
                    If LstExpiry.Col = LstExpiry.Splits(0).DisplayColumns.IndexOf(LstExpiry.Columns("MOD_OrdQty")) Then
                        objSum = dsDetExpiry.Tables("tblDetails").Compute("Sum(MOD_OrdQty)", sFltr)
                        If objSum Is DBNull.Value Then
                            sTotal = 0 : txtQty.Text = "0"
                            If Val(txtQty.Text.Trim) > 0 Then btnBarcode.Enabled = True Else btnBarcode.Enabled = False
                        Else
                            sTotal = CDbl(CType(objSum, Double))
                            txtQty.Text = CStr(sTotal)
                            If Val(txtQty.Text.Trim) > 0 Then btnBarcode.Enabled = True Else btnBarcode.Enabled = False
                            'If UCase(txtStatus.Text.Trim) = "R" Or UCase(txtStatus.Text.Trim) = "RETURN" Then
                            '    txtQty.Text = CStr(Val(txtQty.Text.Trim) * (-1))
                            'End If
                            'update consolidated QTy
                            If UCase(lstDelivery.Columns("New_old").Text.Trim) = "W" Then

                                Dim Str As String
                                Str = " Select case Meas_Sign WHEN '*' THEN case when pn2.sfp_operator='*' then  " & Val(txtQty.Text.Trim) & " *  pn2.SFP_UNITPACK   * STK04_Measurements.Meas_Value else  " & Val(txtQty.Text.Trim) & " /  pn2.SFP_UNITPACK   * STK04_Measurements.Meas_Value end "
                                Str &= " ELSE case when pn2.sfp_operator='*' then (  SFP_UNITPACK   * " & Val(txtQty.Text.Trim) & " ) /   STK04_Measurements.Meas_Value else ( " & Val(txtQty.Text.Trim) & " / SFP_UNITPACK ) /   STK04_Measurements.Meas_Value end  End [ctr]"
                                Str &= " From " & TempPendingDO & "  pn2 "
                                Str &= " left JOIN STK04_Measurements ON Meas_unit = pn2.SFP_SUBUNIT "
                                Str &= " WHERE  pn2.SFP_Recid = " & Val(txtSTkID.Text.Trim) & " "

                                Dim drSQL As SqlDataReader
                                ReadFromDBToDataReader(Str, drSQL)
                                If drSQL.HasRows Then
                                    drSQL.Read()
                                    txtConsQty.Text = Format(drSQL.Item("Ctr"), "#,##0.000").ToString()
                                End If
                            Else
                                strsql = " SELECT  CASE meas_sign"
                                strsql &= " WHEN '*' THEN " & Val(txtQty.Text.Trim) & " * stk_unitpack * meas_value"
                                strsql &= " ELSE ( stk_unitpack * " & Val(txtQty.Text.Trim) & " ) / Meas_value End as [Ctr] FROM V_Pord2_GetStkMeas WHERE   stk_recid = '" & txtSTkID.Text.Trim & "' "
                                Dim drsql As SqlDataReader
                                ReadFromDBToDataReader(strsql, drsql)
                                If drsql.HasRows Then
                                    drsql.Read()
                                    txtConsQty.Text = Format(drsql.Item("Ctr"), "#,##0.000").ToString()
                                End If
                            End If
                        End If
                        NetCalculate()
                    End If
                    Select Case UCase(LstExpiry.Columns(LstExpiry.Col).DataField)
                        Case "MOD_ORDQTY"
                            RefQtyLbl(False)
                    End Select
                    If LstExpiry.Columns("recid").Text.Trim = "" Then Exit Sub
                    If Val(.Columns("MOD_ORDQTY").Text.Trim) <> 0 And (.Columns("DOB_batchInfo").Text.Trim = "" Or .Columns("DOB_ExpiryDate").Text.Trim = "") Then
                        MessageBox.Show("Lot No Or Expiry Date Couldn't be Empty.", cntTitle, MessageBoxButtons.OK, MessageBoxIcon.Exclamation)
                        If .Columns("DOB_batchInfo").Text.Trim = "" Then
                            .Col = .Splits(0).DisplayColumns.IndexOf(LstExpiry.Columns("DOB_batchInfo"))
                        ElseIf .Columns("DOB_ExpiryDate").Text.Trim = "" Then
                            .Col = .Splits(0).DisplayColumns.IndexOf(LstExpiry.Columns("DOB_ExpiryDate"))
                        End If
                        .Focus()
                        Exit Sub

                        If Val(LstExpiry(.Row, "MOD_ORDQTY").ToString.Trim) = 0 And Val(.Columns("MOD_ORDQTY").Text.Trim) = 0 And UCase(lstDelivery.Columns("itmclass").Text.Trim) = "E" Then
                            MessageBox.Show("Quantity Couldn't be Empty.", cntTitle, MessageBoxButtons.OK, MessageBoxIcon.Exclamation)
                            .Focus()

                            Exit Sub
                        End If
                    End If
                End With

                If LstExpiry.Columns("DOB_BatchInfo").Text.Trim = "" Then
                    LstExpiry.EditActive = False
                    Me.BindingContext(dsDetExpiry.Tables("tblDetails").DefaultView).CancelCurrentEdit()
                    dsDetExpiry.Tables("tblDetails").AcceptChanges()
                    Exit Sub
                End If
            End If



            If Wtype <> "" Then
                If txtSTkID.Text.Trim = "" Then
                    MessageBox.Show("Item Key Couldn't be Empty.", cntTitle, MessageBoxButtons.OK, MessageBoxIcon.Exclamation)
                    If txtSupRef.Text.Trim = "" Then
                        txtSupRef.Focus()
                    Else
                        txtRef.Focus()
                    End If
                    Exit Sub
                End If


                If DelOrder = "Receiving Order Fashion" Or DelOrder = "Consignment Receiving Order Fashion" Then
                    'text on valid color 

                    'If Not RecordsExists(ColorsStrsql("Color_Code = '" & txtColor.Text.Trim.Replace("'", "''") & "'", False)) Then
                    '    MessageBox.Show("Invalid Color", cntTitle, MessageBoxButtons.OK, MessageBoxIcon.Exclamation)
                    '    txtColor.Text = ""
                    '    txtColor.Focus()
                    '    Exit Sub
                    'End If
                    If txtColor.Text.Trim = "" Then
                        MessageBox.Show("Color Couldn't be Null.", cntTitle, MessageBoxButtons.OK, MessageBoxIcon.Exclamation)
                        txtColor.Focus()
                        Exit Sub
                    ElseIf txtSupColor.Text.Trim = "" Then
                        MessageBox.Show("Supp. Color Couldn't be Null.", cntTitle, MessageBoxButtons.OK, MessageBoxIcon.Exclamation)
                        txtSupColor.Focus()
                        Exit Sub
                    End If
                End If

            End If

            If Val(txtQty.Text.Trim) = 0 Then
                If Not DtQualityControlHeader Is Nothing Then
                    If DtQualityControlHeader.Select("PrDReciD =  " & txtRecid.Text & "   and (isnull(AccQty,0) <> 0 or isnull(RjQty,0) <> 0) ").Count = 0 Then
                        MessageBox.Show("Quantity Couldn't be Null.", cntTitle, MessageBoxButtons.OK, MessageBoxIcon.Exclamation)
                        If UCase(txtItmClass.Text.Trim) = "E" Then
                            LstExpiry.Focus()
                        ElseIf UCase(txtItmClass.Text.Trim) = "F" Then
                            lstFashQty.Focus()
                        Else
                            txtQty.Focus()
                        End If
                        btnBarcode.Enabled = False
                        Exit Sub
                    End If
                Else
                    MessageBox.Show("Quantity Couldn't be Null.", cntTitle, MessageBoxButtons.OK, MessageBoxIcon.Exclamation)
                    If UCase(txtItmClass.Text.Trim) = "E" Then
                        LstExpiry.Focus()
                    ElseIf UCase(txtItmClass.Text.Trim) = "F" Then
                        lstFashQty.Focus()
                    Else
                        txtQty.Focus()
                    End If
                    btnBarcode.Enabled = False
                    Exit Sub
                End If

            End If

            'update consolidated QTy
            If UCase(GetFieldDesc(TmpGridDetails, "MOD_RMRecid=" & txtSTkID.Text.Trim & "", "new_old")) = "W" Then

                Dim Str As String
                Str = " Select case Meas_Sign WHEN '*' THEN case when pn2.sfp_operator='*' then  " & Val(txtQty.Text.Trim) & " *  pn2.SFP_UNITPACK   * STK04_Measurements.Meas_Value else  " & Val(txtQty.Text.Trim) & " /  pn2.SFP_UNITPACK   * STK04_Measurements.Meas_Value end "
                Str &= " ELSE case when sfp_operator='*' then (  SFP_UNITPACK   * " & Val(txtQty.Text.Trim) & " ) /   STK04_Measurements.Meas_Value else (" & Val(txtQty.Text.Trim) & " / SFP_UNITPACK ) /   STK04_Measurements.Meas_Value end  End [ctr]"
                Str &= " From " & TempPendingDO & "  pn2 "
                Str &= " left JOIN STK04_Measurements ON Meas_unit = pn2.SFP_SUBUNIT "
                Str &= " WHERE  pn2.SFP_Recid = " & Val(txtSTkID.Text.Trim) & " "

                Dim drSQL As SqlDataReader
                ReadFromDBToDataReader(Str, drSQL)
                If drSQL.HasRows Then
                    drSQL.Read()
                    txtConsQty.Text = Format(drSQL.Item("Ctr"), "#,##0.000").ToString()

                End If
            Else
                strsql = " SELECT  CASE meas_sign"
                strsql &= " WHEN '*' THEN " & Val(txtQty.Text.Trim) & " * stk_unitpack * meas_value"
                strsql &= " ELSE ( stk_unitpack * " & Val(txtQty.Text.Trim) & " ) / Meas_value End as [Ctr] FROM V_Pord2_GetStkMeas WHERE   stk_recid = '" & txtSTkID.Text.Trim & "' "
                Dim drsql As SqlDataReader
                ReadFromDBToDataReader(strsql, drsql)
                If drsql.HasRows Then
                    drsql.Read()
                    txtConsQty.Text = Format(drsql.Item("Ctr"), "#,##0.000").ToString()
                End If
            End If
            If CheckItemQtyCntrEc() Then Exit Try

            UpdateLine()
            txtSupRef.Focus()

            'End If
        Catch ex As Exception
            MessageBox.Show("Error: " & ex.Message, cntTitle, MessageBoxButtons.OK, MessageBoxIcon.Error)
        End Try
    End Sub

    Private Sub BtnGenerateBarcodeTrading_Click(ByVal sender As System.Object, ByVal e As System.EventArgs) Handles BtnGenerateBarcodeTrading.Click
        Dim WLast As Double
        Dim BarcodeLength As Integer
        Dim Barc, BarcChkBarc As String
        Dim BarcodeStartZero As Boolean
        Dim drs As DataRow()
        Try
            ClearGridFiltration(LstBarcode)

            If LstExpiry.Columns("RECID").Text.Trim <> "" AndAlso dsDetExpiry.Tables("tblDetails").Select("Recid =  " & LstExpiry.Columns("RECID").Text.Trim & " and (exp_stat = '' or isnull(DOB_BatchInfo,'') = '' or isnull(DOB_ExpiryDate,'') = '') ").Count > 0 Then
                MessageBox.Show("Expiry Date And Lot # Can't Be Null.", cntTitle, MessageBoxButtons.OK, MessageBoxIcon.Exclamation)
                Exit Sub
            End If

            If dsDetBarcode.Tables.Count = 0 Or LstBarcode.Columns(0) Is Nothing Then
                Exit Sub
            End If

            If Val(txtQty.Text.Trim) = 0 Then
                MessageBox.Show("Quantity Couldn't be Null.", cntTitle, MessageBoxButtons.OK, MessageBoxIcon.Exclamation)
                If UCase(txtItmClass.Text.Trim) = "E" Then
                    LstExpiry.Focus()
                ElseIf UCase(txtItmClass.Text.Trim) = "F" Then
                    lstFashQty.Focus()
                Else
                    txtQty.Focus()
                End If
                btnBarcode.Enabled = False
                Exit Sub
            End If

            If Val(LstExpiry(LstExpiry.Row, "MOD_ORDQTY").ToString.Trim) = 0 And Val(LstExpiry.Columns("MOD_ORDQTY").Text.Trim) = 0 And UCase(txtItmClass.Text.Trim) = "E" Then
                MessageBox.Show("Quantity Couldn't be Empty.", cntTitle, MessageBoxButtons.OK, MessageBoxIcon.Exclamation)
                LstExpiry.Focus()
                Exit Sub
            End If

            drs = dsDetBarcode.Tables("tblDetails").Select("recid='" & txtRecid.Text.Trim & "' AND bar_stat<>'D'")

            If UCase(txtItmClass.Text.Trim) = "E" Then
                If drs.Length >= Val(LstExpiry.Columns("QTY").Text.Trim) Then
                    MessageBox.Show("Barcode Number Can't Be Bigger Than Expiry Quantity.", cntTitle, MessageBoxButtons.OK, MessageBoxIcon.Exclamation)
                    LstExpiry.Columns("QTY").Text = CStr(drs.Length)
                    BInsBarcodeCancel = True
                    Exit Sub
                End If
            Else
                If drs.Length >= Val(txtQty.Text.Trim) Then
                    MessageBox.Show("Barcode Number Can't Be Bigger Than Quantity.", cntTitle, MessageBoxButtons.OK, MessageBoxIcon.Exclamation)
                    If Val(txtQty.Text.Trim) = 0 Then
                        txtQty.Focus()
                    End If
                    txtQty.Text = CStr(drs.Length)
                    NetCalculate()
                    BInsBarcodeCancel = True
                    If Val(txtQty.Text.Trim) > 0 Then btnBarcode.Enabled = True Else btnBarcode.Enabled = False
                    Exit Sub
                End If
            End If
            BInsBarcodeCancel = False

            'If lcItemClass = "Expiry" 
            'If txtItmClass.Text.Trim = "E" And LstExpiryDet(LstExpiryDet.Row, "E_RecId").ToString = "" Then
            '    MessageBox.Show("Please Fill The Expiry Date And Lot # First", cntTitle, MessageBoxButtons.OK, MessageBoxIcon.Exclamation)
            '    LstExpiryDet.Focus()
            '    LstExpiryDet.Col = LstExpiryDet.Splits(0).DisplayColumns.IndexOf(LstExpiryDet.Columns("BatchInfo"))
            '    Exit Sub
            'End If


            'LstBarcode.Row = LstBarcode.Splits(0).Rows.Count - 1
            'If UCase(lstDelivery.Columns("ItmClass").Text.Trim) = "E" Then
            '    If TestExpiryQty("Barcode") = True Then Exit Sub
            'Else
            '    If TestTradingQty() = True Then Exit Sub
            'End If

            strsql = "Select SP_LastBarcode From stk04_SysParam"
            BarcodeStartZero = False
            If RecordsExists(strsql) = True Then
                Barc = GetFieldDesc("stk04_SysParam", "1=1", "SP_LastBarcode")
                If Barc.Substring(0, 1) = "0" Then BarcodeStartZero = True : BarcodeLength = Barc.Length
                WLast = CDbl(Barc) + 1

                If BarcodeStartZero = True Then
                    BarcChkBarc = "000000000000".Substring(0, BarcodeLength - WLast.ToString.Length) + CStr(WLast)
                Else
                    BarcChkBarc = CStr(WLast)
                End If
                chkBarc(BarcChkBarc)
                Execute(" UPDATE stk04_SysParam SET SP_LastBarcode = '" & BarcChkBarc & "' ")
            Else
                MessageBox.Show("No Counter For Barcode Numbers Defined", cntTitle, MessageBoxButtons.OK, MessageBoxIcon.Information)
                Exit Sub
            End If
        Catch ex As Exception
            MessageBox.Show("Error: " & ex.Message, cntTitle, MessageBoxButtons.OK, MessageBoxIcon.Error)
        End Try
    End Sub

    Private Sub chkBarc(ByVal Barc As String)
        Dim Tun(13) As String
        Dim Ttab(12) As String
        Dim Wchk As String
        Dim WVal As Integer
        Dim WRem As Integer
        Dim WRem2 As Integer
        Dim row As DataRow
        Try
            ' If bMode = "R" Then Exit Sub
            If Barc = "" OrElse (Barc.Length <> 12 And Barc.Length <> 11) Or (Barc.Length = 12 AndAlso Barc.Substring(11, 1) = "") OrElse (Barc.Length = 11 AndAlso Barc.Substring(10, 1) = "") Then
                MessageBox.Show("Invalid Last Barcode Number In System Parameters", cntTitle, MessageBoxButtons.OK, MessageBoxIcon.Exclamation)
                Exit Sub
            End If

            'calculate check difit of barcode
            Wchk = ""
            WVal = 0
            For i = 1 To Barc.Length
                Tun(i) = Barc.Substring(i - 1, 1)

                If Barc.Length = 11 Then
                    If i = Int(i / 2) * 2 Then
                        Ttab(i) = CStr(Val(Tun(i)) * 1)
                    Else
                        Ttab(i) = CStr(Val(Tun(i)) * 3)
                    End If
                Else
                    If i = Int(i / 2) * 2 Then
                        Ttab(i) = CStr(Val(Tun(i)) * 3)
                    Else
                        Ttab(i) = CStr(Val(Tun(i)) * 1)
                    End If
                End If
              
                WVal = CInt(WVal + CDbl(Ttab(i)))
            Next i
            'divide by 10,get reminder
            WRem = CInt(WVal - (Int(WVal / 10) * 10))
            If WRem <> 0 Then
                WRem2 = 10 - WRem
            Else
                WRem2 = 0
            End If
            Wchk = CStr(WRem2)

            If DelOrder = "Receiving Order Fashion" Or DelOrder = "Consignment Receiving Order Fashion" Then
                LstBarcodeF.Focus()
                LstBarcodeF.Columns(LstBarcodeF.Col).Text = Barc.Substring(0, Barc.Length) + Wchk
                LstBarcodeF.Columns(LstBarcodeF.Col + 1).Text = "N"
                LstBarcodeF.UpdateData()
                daDetBarcodeF.Update(dsDetBarcodeF.Tables("tblDetails"))

                LstBarcodeF.Focus()
                'SendKeys.Send("{DOWN}")
                LstBarcodeF.Row = LstBarcodeF.Row + 1
            Else
                LstBarcode.Row = 0 : LstBarcode.Focus()
                row = dsDetBarcode.Tables("tblDetails").NewRow
                'B_RECID()
                'DOB_RECID()
                'RECID()
                'E_RECID()
                'DOB_Barcode()
                'bar_stat()
                row("B_RecId") = GetMaxRecId(TmpGridBarcode, "B_RecId")
                row("RecId") = txtRecid.Text.Trim
                If txtItmClass.Text.Trim = "E" Then
                    row("E_RecId") = LstExpiry.Columns("E_RecId").Text
                End If
                row(LstBarcode.Col) = Barc.Substring(0, Barc.Length) + Wchk
                row("Bar_Stat") = "A"
                dsDetBarcode.Tables("tblDetails").Rows.Add(row)

                daDetBarcode.Update(dsDetBarcode, "tblDetails")
                LstBarcode.Focus()

                strsql = "DELETE FROM " & TmpGridBarcode & " WHERE (DOB_Barcode IS NULL OR DOB_Barcode='' ) AND Bar_stat='A'"
                Execute(strsql)

                If Not dsDetBarcode.Tables("tblDetails") Is Nothing Then dsDetBarcode.Tables("tblDetails").Clear()
                daDetBarcode.Fill(dsDetBarcode, "tblDetails")
            End If
        Catch ex As Exception
            Throw ex
        End Try
    End Sub

    Private Sub txtVat_MouseHover(ByVal sender As Object, ByVal e As System.EventArgs) Handles txtVat.MouseHover
        Dim str As String
        Try
            If UCase(txtVat.Text.Trim) = "T" Then
                str = "Taxable"
            ElseIf UCase(txtVat.Text.Trim) = "E" Then
                str = "Taxable Embeded"
            ElseIf UCase(txtVat.Text.Trim) = "N" Then
                str = "Not Taxable"
            Else
                str = ""
            End If
            ToolTip1.SetToolTip(txtVat, str)
        Catch ex As Exception
            MessageBox.Show("Error: " & ex.Message, cntTitle, MessageBoxButtons.OK, MessageBoxIcon.Error)
        End Try
    End Sub


    Private Sub lstDelivery_FetchCellTips(ByVal sender As Object, ByVal e As C1.Win.C1TrueDBGrid.FetchCellTipsEventArgs) Handles lstDelivery.FetchCellTips
        Dim str As String

        Try

            If e.Row < 0 OrElse e.Column Is Nothing OrElse lstDelivery.Columns("Recid").CellText(e.Row) = "" Then _
                Exit Try


            With lstDelivery
                Select Case e.Column.DataColumn.DataField
                    Case "invd_VAT"
                        If UCase(lstDelivery.Columns("invd_VAT").CellText(e.Row)) = "T" Then
                            str = "Taxable"
                        ElseIf UCase(lstDelivery.Columns("invd_VAT").CellText(e.Row)) = "E" Then
                            str = "Taxable Embeded"
                        ElseIf UCase(lstDelivery.Columns("invd_VAT").CellText(e.Row)) = "N" Then
                            str = "Not Taxable"
                        Else
                            str = ""
                        End If
                    Case "MOD_stat"
                        If UCase(lstDelivery.Columns("MOD_stat").CellText(e.Row)) = "N" Then
                            str = "N"
                        ElseIf UCase(lstDelivery.Columns("MOD_stat").CellText(e.Row)) = "R" Then
                            str = "Reverse"
                        ElseIf UCase(lstDelivery.Columns("MOD_stat").CellText(e.Row)) = "F" Then
                            str = "Free"
                        Else
                            str = ""
                        End If
                    Case "ItemType"
                        str = lstDelivery.Columns("ItemType").CellText(e.Row)
                    Case "Style"
                        str = lstDelivery.Columns("Style").CellText(e.Row)
                    Case "stk_ref"
                        str = lstDelivery.Columns("stk_ref").CellText(e.Row)

                    Case Else : str = ""
                End Select
            End With

            e.CellTip = "" : ToolTip1.SetToolTip(lstDelivery, str)

        Catch ex As Exception
            MessageBox.Show("General Error : " & ex.Message, Me.Text, MessageBoxButtons.OK, MessageBoxIcon.Error)
        End Try
    End Sub

    Private Sub BtnGenerateBarcodeFashion_Click(ByVal sender As System.Object, ByVal e As System.EventArgs) Handles BtnGenerateBarcodeFashion.Click
        Dim WLast As Double
        Dim BarcodeFLength As Integer
        Dim BarcF, BarcChkBarcF As String
        Dim BarcodeFStartZero As Boolean
        Dim drs As DataRow()
        Dim row As Integer
        Try
            row = LstBarcodeF.Row
            ClearGridFiltration(LstBarcodeF)
            LstBarcodeF.Row = row
            If dsDetBarcodeF.Tables.Count = 0 Or LstBarcodeF.Columns(0) Is Nothing Then Exit Sub

            If AllowWriteInBarc() = False Then
                MessageBox.Show("Please Fill Higher Rows First", cntTitle, MessageBoxButtons.OK, MessageBoxIcon.Exclamation)
                LstBarcodeF.Focus() : Exit Sub
            End If

            'test on quantity per size and number of barcodes 

            drs = dsDetBarcodeF.Tables("tblDetails").Select("Tmp_Stat<>'D' and Tmp_DRecId='" & txtRecid.Text.Trim & "' and tmp_size" & CInt(LstBarcodeF.Columns(LstBarcodeF.Col).Tag) & "<>''")

            If drs.Length >= Val(lstFashQty.Columns("Size" & CInt(LstBarcodeF.Columns(LstBarcodeF.Col).Tag)).Text.Trim) Then
                MessageBox.Show("Barcode Number Can't Be Bigger Than Quantity Of This Size.", cntTitle, MessageBoxButtons.OK, MessageBoxIcon.Exclamation)
                Me.BindingContext(dsDetBarcodeF.Tables("tblDetails").DefaultView).CancelCurrentEdit()
                Exit Sub
            End If

            strsql = "Select SP_LastBarcode From stk04_SysParam"
            BarcodeFStartZero = False
            If RecordsExists(strsql) = True Then
                BarcF = GetFieldDesc("stk04_SysParam", "1=1", "SP_LastBarcode")
                If BarcF.Substring(0, 1) = "0" Then BarcodeFStartZero = True : BarcodeFLength = BarcF.Length
                WLast = CDbl(BarcF) + 1

                If BarcodeFStartZero = True Then
                    BarcChkBarcF = "000000000000".Substring(0, BarcodeFLength - WLast.ToString.Length) + CStr(WLast)
                Else
                    BarcChkBarcF = CStr(WLast)
                End If
                chkBarc(BarcChkBarcF)
                Execute(" UPDATE stk04_SysParam SET SP_LastBarcode = '" & BarcChkBarcF & "' ")
            Else
                MessageBox.Show("No Counter For Barcode Numbers Defined", cntTitle, MessageBoxButtons.OK, MessageBoxIcon.Information)
                Exit Sub
            End If
        Catch ex As Exception
            MessageBox.Show("Error: " & ex.Message, cntTitle, MessageBoxButtons.OK, MessageBoxIcon.Error)
        End Try
    End Sub


    Private Sub txtItmClass_TextChanged(sender As Object, e As System.EventArgs) Handles txtItmClass.TextChanged
        Try
            If txtItmClass.Text = "T" Then
                btnQA.Enabled = True
            Else
                btnQA.Enabled = False
            End If
        Catch ex As Exception

        End Try
    End Sub


    Public Function InsertPendItem(ByVal TempPending As String, ByVal Recid As String) As String
        Dim trnsql As SqlTransaction
        Dim cnSql As SqlConnection
        Dim cmSql As SqlCommand
        Dim Str As String
        Try
            InsertPendItem = ""
            cnSql = New SqlConnection(ConnectionString)
            cnSql.Open()
            trnsql = cnSql.BeginTransaction

            Str = "Select ISNULL(MAX(SFP_Recid),0) +1 FROM " & TempPending & ""
            cmSql = New SqlCommand(Str, cnSql, trnsql)
            InsertPendItem = CStr(cmSql.ExecuteScalar())

            Str = " IF NOT EXISTS (SELECT SFP_Recid FROM " & TempPending & " WHERE SFP_OldRecid = " & Val(Recid) & ")"
            Str &= " INSERT INTO " & TempPending
            Str &= " SELECT " & InsertPendItem & ", SFP_Comp,SFP_SUPPLIER,SFP_CATEGORY,SFP_Serie,SFP_Usage,SFP_NEWUSED,SFP_Season,SFP_Brand,"
            Str &= " SFP_Grid, SFP_Gender, SFP_Style, SFP_SingleRef1, SFP_SingleRef2, SFP_ItmClass, SFP_LOCSRC, SFP_InfDate, SFP_OpSource,"
            Str &= " SFP_Desc, SFP_ImpLoc, SFP_ItmIdent, SFP_SerReg, SFP_ItmType, SFP_UNIT, SFP_UNITPACK, SFP_SUBUNIT, SFP_FCCode, SFP_VATType,"
            Str &= " SFP_VATPER, SFP_StartTaxDate, SFP_CrUStamp, SFP_CrDStamp, SFP_UStamp, SFP_DStamp, SFP_HSCode ,SFP_VATSTAT,SFP_TechSheet,sfp_operator "
            Str &= " , 'O', SFP_Recid ,'' FROM STK04_StkFilPending"
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


#Region "Quality Control Mod (Toufic 08-02-2017)"
 


    Private Sub btnQA_Click(sender As Object, e As System.EventArgs) Handles btnQA.Click
        Dim tempferm As frmQC
        Dim QCtitle As String
        Dim selectedRecID, ACCQTy As String
        '   Dim DrNew, DrFrom As DataRow
        Try
            If txtQty.Text.Trim = "" OrElse CDbl(txtQty.Text.Trim) = 0 Then
                MessageBox.Show("Please Fill Quantity To Use Quality Control Form ", cntTitle, MessageBoxButtons.OK, MessageBoxIcon.Information)
                txtQty.Focus()
                Exit Try
            End If
            If txtStatus.Text.Trim <> "N" Then
                MessageBox.Show("Item Status Should Be Normal To Be Able to Create A Quality Control ", cntTitle, MessageBoxButtons.OK, MessageBoxIcon.Information)
                txtQty.Focus()
                Exit Try
            End If
            tempferm = New frmQC
            QCtitle = "Ref : " & txtRef.Text.Trim & " - Desc : " & txtDesc.Text.Trim & ""
            If tempferm.ShowDialog(txtQty.Text, QCtitle, txtRecid.Text.Trim, "", TempQcHeader, TempQcDetails, ACCQTy) = Windows.Forms.DialogResult.OK Then
                UpdateDtQualityControlHeader()
                txtQty.Text = ACCQTy
                NetCalculate()
                txtRef.Focus()
                SendKeys.Send("{ENTER}")

            End If

        Catch ex As Exception

        End Try
    End Sub


 
    Private Sub UpdateDtQualityControlHeader()
        Dim cnsql As SqlConnection
        Dim dasql As SqlDataAdapter

        Try

            strsql = "SELECT * FROM  " & TempQcHeader & " "
            cnsql = New SqlConnection(ConnectionString)
            dasql = New SqlDataAdapter(strsql, cnsql)
            DtQualityControlHeader = New DataTable
            dasql.Fill(DtQualityControlHeader)

        Catch ex As Exception
            Throw ex
        End Try
    End Sub
#End Region


    Private Sub txtStatus_KeyPress(sender As Object, e As System.Windows.Forms.KeyPressEventArgs) Handles txtStatus.KeyPress
        Try
            If txtRecid.Text.Trim = "" Then Exit Try
            If Not DtQualityControlHeader Is Nothing Then
                If DtQualityControlHeader.Select("PrDReciD =  " & txtRecid.Text & " ").Count <> 0 Then
                    e.Handled = True
                    MessageBox.Show("This Record Is linked To A Quality Control , Status Can't Be Changed", cntTitle, MessageBoxButtons.OK, MessageBoxIcon.Information)
                    Exit Try
                End If
            End If

        Catch ex As Exception
            MessageBox.Show("Error: " & ex.Message, cntTitle, MessageBoxButtons.OK, MessageBoxIcon.Error)
        End Try

    End Sub

    Private Sub FillDictionaryByItemRef()
        Dim DT As DataTable

        Try
            If Me.InvokeRequired Then
                Dim d As New fillremdictomrydelg(AddressOf FillDictionaryByItemRef)
                Me.Invoke(d)
            Else
                If DelOrder.Contains("Consignment") Then
                    DT = ReadFromDBToDataTable("SELECT STK_SingleRef1 STK_SingleRef1 FROM dbo.STK04_STKFIL WHERE STK_NEWUSED  = 'C' AND (STK_HaltPoPi = 'N' or (STK_HaltPoPi ='Y'  and stk_haltacttill < convert(DATETIME, Getdate(),103) ) ) " & IIf(DelOrder.Contains("Fashion"), " AND Stk_Brand='" & txtBrand.Text.Trim.Replace("'", "''") & "' AND Stk_Season='" & txtSeason.Text.Trim.Replace("'", "''") & "' AND  STK_ItmClass = 'F' ", " AND  STK_ItmClass <> 'F' ").ToString)
                Else
                    DT = ReadFromDBToDataTable("SELECT STK_SingleRef1 STK_SingleRef1 FROM dbo.STK04_STKFIL WHERE STK_NEWUSED<>'C' AND (STK_HaltPoPi = 'N' or (STK_HaltPoPi ='Y'  and stk_haltacttill < convert(DATETIME, Getdate(),103) ) ) " & IIf(DelOrder.Contains("Fashion"), " AND Stk_Brand='" & txtBrand.Text.Trim.Replace("'", "''") & "' AND Stk_Season='" & txtSeason.Text.Trim.Replace("'", "''") & "' AND  STK_ItmClass = 'F' ", " AND  STK_ItmClass <> 'F' ").ToString)
                End If
                If DT.Rows.Count > 0 Then
                    For Each dr As DataRow In DT.Rows
                        ReferenceCollectionItmRef.Add(dr("STK_SingleRef1").ToString())
                    Next
                End If
                With txtRef
                    .AutoCompleteCustomSource = ReferenceCollectionItmRef
                    .AutoCompleteMode = AutoCompleteMode.Suggest
                    .AutoCompleteSource = AutoCompleteSource.CustomSource
                End With
            End If
        Catch ex As Exception
            Throw ex
        End Try
    End Sub

    Private Sub PnlHead2_Validating(sender As Object, e As System.ComponentModel.CancelEventArgs) Handles PnlHead2.Validating
        Try
            If DelOrder.Contains("Fashion") Then
                If Not ReferenceCollectionItmRef Is Nothing Then ReferenceCollectionItmRef.Clear()
                If txtBrand.Text.Trim <> "" AndAlso txtSeason.Text.Trim <> "" Then FillDictionaryByItemRef()
            End If
        Catch ex As Exception
            MessageBox.Show("Error : " & ex.Message, cntTitle, _
            MessageBoxButtons.OK, MessageBoxIcon.Error)
        End Try
    End Sub



    Private Function QtyCntrEc(ByVal _ItemReciD As String, ByVal _TotalQty As String, _
                               Optional ByVal _ExpiryDate As String = "", Optional ByVal _LotNumber As String = "", Optional _ItmDesc As String = "", _
                               Optional ByVal _Grid As String = "", Optional ByVal _Size As String = "", Optional ByVal _Color As String = "") As Boolean
        Dim strsql As String
        Dim obj_Result As Object
        Dim drsql As SqlDataReader
        Try
            If txtStatus.Text.Trim.ToUpper <> "R" Then Exit Try
            If _TotalQty.Trim = "" OrElse IsNumeric(_TotalQty) = False Then
                _TotalQty = "0"
            End If
            strsql = " SELECT  AvailbleQTy"
            strsql &= " FROM    ( SELECT    STK_RECID ,"
            strsql &= " STK_QTYCTRL ,"
            strsql &= " STK_ItmClass"
            strsql &= " FROM      STK04_STKFIL"
            strsql &= " WHERE     STK_QTYCTRL = 'Y'"
            strsql &= "  and STK_RECID = " & _ItemReciD & "  "
            strsql &= " ) STK04_STKFIL"
            strsql &= " INNER JOIN ( SELECT STQ_STKRECID ,"
            strsql &= " ( STQ_AVAILABLE - STQ_RESERVED - STQ_TransPendO ) AvailbleQTy "
            strsql &= " FROM   stk04_stkqty "
            strsql &= " WHERE  STQ_LOCCODE = '" & txtLoc.Text.Trim.Replace("'", "''") & "' "

            strsql &= " AND isnull(STQ_ExpiryDt ,'')='" & _ExpiryDate & "' "
            strsql &= " AND isnull(STQ_BatchInfo,'') ='" & _LotNumber.Trim.Replace("'", "''") & "' "

            strsql &= " AND isnull(STQ_Color,'') ='" & _Color.Trim.Replace("'", "''") & "' "
            strsql &= " AND isnull(STQ_Grid,'') ='" & _Grid.Trim.Replace("'", "''") & "' "
            strsql &= " AND isnull(STQ_DetGrid,'') ='" & _Size.Trim.Replace("'", "''") & "' "

            strsql &= " ) stk04_stkqty ON STQ_STKRECID = STK_RECID "
            strsql &= " WHERE AvailbleQTy - (" & _TotalQty & ") <0 "

            Using Cnsql As New SqlConnection(ConnectionString)
                Cnsql.Open()
                Using Cmsql As New SqlCommand(strsql, Cnsql)
                    obj_Result = Cmsql.ExecuteScalar
                End Using
                Cnsql.Close()
            End Using
            If obj_Result Is Nothing OrElse obj_Result Is DBNull.Value _
                OrElse obj_Result.ToString = "" Then
                Dim ctrlType, LocRec As String

                Using Cnsql As New SqlConnection(ConnectionString)
                    Cnsql.Open()
                    strsql = " SELECT TOP 1"
                    strsql &= "         STK_QTYCTRL ,"
                    strsql &= "         STQ_STKRECID"
                    strsql &= " FROM    STK04_STKFIL"
                    strsql &= "         LEFT JOIN stk04_stkqty ON STQ_STKRECID = STK_RECID"
                    strsql &= " AND  STQ_LOCCODE = '" & txtLoc.Text.Trim.Replace("'", "''") & "' "

                    strsql &= " AND isnull(STQ_ExpiryDt ,'')='" & _ExpiryDate & "' "
                    strsql &= " AND isnull(STQ_BatchInfo,'') ='" & _LotNumber.Trim.Replace("'", "''") & "' "

                    strsql &= " AND isnull(STQ_Color,'') ='" & _Color.Trim.Replace("'", "''") & "' "
                    strsql &= " AND isnull(STQ_Grid,'') ='" & _Grid.Trim.Replace("'", "''") & "' "
                    strsql &= " AND isnull(STQ_DetGrid,'') ='" & _Size.Trim.Replace("'", "''") & "' "

                    strsql &= " where STk_RECID = " & _ItemReciD
                    Using Cmsql As New SqlCommand(strsql, Cnsql)
                        drsql = Cmsql.ExecuteReader
                        If drsql.HasRows Then
                            drsql.Read()
                            ctrlType = drsql.Item("STK_QTYCTRL").ToString
                            LocRec = drsql.Item("STQ_STKRECID").ToString
                            drsql.Close()
                            If Not drsql Is Nothing Then drsql = Nothing
                        End If
                    End Using
                    Cnsql.Close()
                End Using
                If (Not ctrlType Is Nothing) AndAlso ctrlType.Trim = "Y" AndAlso LocRec.Trim = "" Then
                    If _ItmDesc = "" Then
                        MessageBox.Show("This Item Does Not Have Quantity In This Location", "Quantity Control", MessageBoxButtons.OK, MessageBoxIcon.Information)
                    Else
                        Dim ExpMsg As String = ""
                        If _ExpiryDate <> "" Then ExpMsg &= vbCrLf & "Expiry Date: " & _ExpiryDate
                        If _LotNumber <> "" Then ExpMsg &= vbCrLf & "Lot #: " & _LotNumber
                        If _Size <> "" Then ExpMsg &= vbCrLf & "Size: " & _Size
                        MessageBox.Show("Item """ & _ItmDesc & """ Does Not Have Quantity In This Location" & ExpMsg, "Quantity Control", MessageBoxButtons.OK, MessageBoxIcon.Information)
                    End If
                    Return True
                Else
                    Return False
                End If

            Else
                Using Cnsql As New SqlConnection(ConnectionString)
                    Cnsql.Open()
                    If _ItmDesc = "" Then
                        strsql = " SELECT 'This Item Have ' + cast(STQ_AVAILABLE - STQ_RESERVED - STQ_TransPendO AS NVARCHAR) + ' Available Quantity' "
                    Else
                        strsql = " SELECT 'Item """ & _ItmDesc.Replace("'", "''") & """ Have ' + cast(STQ_AVAILABLE - STQ_RESERVED - STQ_TransPendO AS NVARCHAR) + ' Available Quantity' "
                    End If
                    strsql &= " FROM dbo.STK04_STKFIL INNER JOIN dbo.stk04_stkqty ON STQ_STKRECID = STK_RECID"
                    strsql &= " WHERE  STQ_LOCCODE = '" & txtLoc.Text.Trim.Replace("'", "''") & "' "

                    strsql &= " AND isnull(STQ_ExpiryDt ,'')='" & _ExpiryDate & "' "
                    strsql &= " AND isnull(STQ_BatchInfo,'') ='" & _LotNumber.Trim.Replace("'", "''") & "' "

                    strsql &= " AND isnull(STQ_Color,'') ='" & _Color.Trim.Replace("'", "''") & "' "
                    strsql &= " AND isnull(STQ_Grid,'') ='" & _Grid.Trim.Replace("'", "''") & "' "
                    strsql &= " AND isnull(STQ_DetGrid,'') ='" & _Size.Trim.Replace("'", "''") & "' "

                    strsql &= " and STK_RECID = " & _ItemReciD & "  "
                    Using Cmsql As New SqlCommand(strsql, Cnsql)
                        obj_Result = Cmsql.ExecuteScalar
                    End Using
                    Cnsql.Close()
                End Using

                If obj_Result Is Nothing OrElse obj_Result Is DBNull.Value _
                OrElse obj_Result.ToString = "" Then
                    If _ItmDesc = "" Then
                        MessageBox.Show("This Item Does Not Have Quantity In This Location", "Quantity Control", MessageBoxButtons.OK, MessageBoxIcon.Information)
                    Else
                        Dim ExpMsg As String = ""
                        If _ExpiryDate <> "" Then ExpMsg &= vbCrLf & "Expiry Date: " & _ExpiryDate
                        If _LotNumber <> "" Then ExpMsg &= vbCrLf & "Lot #: " & _LotNumber
                        If _Size <> "" Then ExpMsg &= vbCrLf & "Size: " & _Size
                        MessageBox.Show("Item """ & _ItmDesc & """ Does Not Have Quantity In This Location" & ExpMsg, "Quantity Control", MessageBoxButtons.OK, MessageBoxIcon.Information)
                    End If
                Else
                    If _ItmDesc = "" Then
                        MessageBox.Show(obj_Result.ToString, "Quantity Control", MessageBoxButtons.OK, MessageBoxIcon.Information)
                    Else
                        Dim ExpMsg As String = ""
                        If _ExpiryDate <> "" Then ExpMsg &= vbCrLf & "Expiry Date: " & _ExpiryDate
                        If _LotNumber <> "" Then ExpMsg &= vbCrLf & "Lot #: " & _LotNumber
                        If _Size <> "" Then ExpMsg &= vbCrLf & "Size: " & _Size
                        MessageBox.Show(obj_Result.ToString & ExpMsg, "Quantity Control", MessageBoxButtons.OK, MessageBoxIcon.Information)
                    End If
                End If
                Return True
            End If
        Catch ex As Exception
            Throw ex
        Finally
            Disposecontrols(, , drsql)
        End Try
    End Function

    Private Function CheckItemQtyCntrEc() As Boolean
        Dim ItmQty As Object
        Dim strsql As String
        Try
            If txtStatus.Text.Trim.ToUpper <> "R" Then Exit Try
            If txtItmClass.Text = "E" Then
                For Each drs As DataRow In dsDetExpiry.Tables("tblDetails").Rows
                    With drs
                        LstExpiry.UpdateData()
                        daDetExpiry.Update(dsDetExpiry.Tables("tblDetails"))
                        If dsDetExpiry.Tables("tblDetails").Select(" recid =" & txtRecid.Text.Trim).Length = 0 Then Exit Try
                        If .Item("NewRECID").ToString = "" AndAlso .Item("RECID").ToString = txtRecid.Text.Trim AndAlso .Item("exp_stat").ToString <> "D" Then
                            Using cnsql As New SqlConnection(ConnectionString)
                                cnsql.Open()
                                strsql = " SELECT ISNULL(SUM(t2.MOD_OrdQty),0) FROM " & TmpGridDetails & " t1"
                                strsql &= " INNER JOIN " & TmpGridExpiry & " t2 ON t1.recid = t2.recid AND DOB_Drecid IS NULL"
                                strsql &= " WHERE MOD_RMRecid = '" & txtSTkID.Text.Trim & "'"
                                strsql &= " AND MOD_stat = 'R'"
                                strsql &= " AND ISNULL(DOB_BatchInfo,'') = '" & .Item("Dob_batchinfo").ToString & "'"
                        strsql &= " AND ISNULL(DOB_ExpiryDate,'') = '" & .Item("DOB_ExpiryDate").ToString & "'"
                        strsql &= " AND new_old <> 'D'"
                        strsql &= " AND t1.recid <> " & txtRecid.Text.Trim & ""
                        Using cmsql As New SqlCommand(strsql, cnsql)
                            ItmQty = cmsql.ExecuteScalar
                        End Using
                        cnsql.Close()
                            End Using

                            If ItmQty Is DBNull.Value Then
                                ItmQty = 0
                            Else
                                ItmQty = CDbl(ItmQty)
                            End If
                            If LstExpiry.Columns("MOD_OrdQty").Text.Trim <> "" Then
                                ItmQty = (CDbl(ItmQty) + CDbl(LstExpiry.Columns("MOD_OrdQty").Text))
                            End If

                            If QtyCntrEc(txtSTkID.Text.Trim, ItmQty.ToString, .Item("DOB_ExpiryDate").ToString, .Item("Dob_batchinfo").ToString, txtDesc.Text.Trim) Then
                                'ShowNotification(Me, "  Qty Cntr  ", 2500, 14, DevComponents.DotNetBar.eToastPosition.MiddleCenter)
                                Return True
                            End If
                        End If
                    End With
                Next
            Else
                If txtQty.Text.Trim = "" Then Return False
                If txtItmClass.Text.Trim.ToUpper = "T" Then
                    ItmQty = dsDetDetails.Tables("tblDetails").Compute(" sum(MOD_OrdQty) ", _
                                    " DOB_Drecid is null and MOD_stat = 'R' and MOD_RMRecid = " & txtSTkID.Text.Trim & _
                                    IIf(wModify = True, " AND RECID <> " & txtRecid.Text.Trim & "", "").ToString)
                    If ItmQty Is DBNull.Value Then
                        ItmQty = 0
                    Else
                        ItmQty = CDbl(ItmQty)
                    End If
                    ItmQty = (CDbl(ItmQty) + CDbl(txtQty.Text.Trim))

                    If QtyCntrEc(txtSTkID.Text.Trim, ItmQty.ToString, , , txtDesc.Text.Trim) Then
                        'ShowNotification(Me, "  Qty Cntr  ", 2500, 14, DevComponents.DotNetBar.eToastPosition.MiddleCenter)
                        Return True
                    End If
                End If
            End If
        Catch ex As Exception
            Throw ex
        End Try
    End Function

    Private Function CheckItemQtyCntrEcF() As Boolean
        Dim ItmQty As Object
        Dim strsql As String
        Try
            If txtStatus.Text.Trim.ToUpper <> "R" Then Exit Try
            For i As Integer = 1 To MaxCnt
                If lstFashQty.Columns("Size" & i).Text <> "" AndAlso CDbl(lstFashQty.Columns("Size" & i).Text) <> 0 Then
                    Using cnsql As New SqlConnection(ConnectionString)
                        cnsql.Open()

                        strsql &= " SELECT ISNULL(SUM(size" & i & "),0) FROM " & TmpGridDetails & " t1"
                        strsql &= " INNER JOIN " & TmpFashQty & " t2 ON t1.recid = t2.recid"
                        strsql &= " WHERE DOB_Drecid IS null"
                        strsql &= " AND MOD_stat = 'R'"
                        strsql &= " AND MOD_RMRecid = '" & txtSTkID.Text.Trim & "'"
                strsql &= " AND GridCode = '" & lstFashQty.Columns("GridCode").Text.Trim.Replace("'", "''") & "'"
                strsql &= " AND color = '" & txtColor.Text.Trim.Replace("'", "''") & "'"
                strsql &= " AND t2.recid <> " & txtRecid.Text.Trim & ""

                Using cmsql As New SqlCommand(strsql, cnsql)
                    ItmQty = cmsql.ExecuteScalar
                End Using
                cnsql.Close()
                    End Using

                    If ItmQty Is DBNull.Value Then
                        ItmQty = 0
                    Else
                        ItmQty = CDbl(ItmQty)
                    End If
                    ItmQty = (CDbl(ItmQty) + CDbl(lstFashQty.Columns("Size" & i).Text))

                    If QtyCntrEc(txtSTkID.Text.Trim, ItmQty.ToString, , , txtDesc.Text.Trim, lstFashQty.Columns("GridCode").Text, lstFashQty.Columns("Size" & i).ToString, txtColor.Text.Trim) Then
                        Return True
                    End If

                End If
            Next
        Catch ex As Exception
            Throw ex
        End Try
    End Function

    Private Sub NetCalculate()
        Try
            If DelOrder = "Consignment Receiving Order" Or DelOrder = "Consignment Receiving Order Fashion" Then
                If TxtDisc.Text.Trim = "" Then TxtDisc.Text = "0"
                If Val(txtPrice.Text.Trim) = 0 Then
                    TxtNet.Text = CDbl(0).ToString("N3")
                Else
                    If IsNumeric(TxtDisc.Text.Trim) AndAlso _
                         txtQty.Text.Trim <> "" AndAlso IsNumeric(txtQty.Text.Trim) AndAlso _
                        txtPrice.Text.Trim <> "" AndAlso IsNumeric(txtPrice.Text.Trim) Then
                        TxtNet.Text = CDbl(CDbl(CDbl(txtPrice.Text.Trim) * CDbl(txtQty.Text.Trim)) * CDbl((1 - (CDbl(TxtDisc.Text.Trim) / 100)))).ToString("N3")
                    End If
                End If
            End If
        Catch ex As Exception
            Throw ex
        End Try
    End Sub

   
    Private Sub lstDelivery_BeforeColUpdate(sender As Object, e As BeforeColUpdateEventArgs) Handles lstDelivery.BeforeColUpdate
        Dim str As String
        Try
            If e.Column.DataColumn.DataField = "MOD_OrdQty" OrElse e.Column.DataColumn.DataField = "COST" OrElse e.Column.DataColumn.DataField = "disc" Then
                If lstDelivery.Columns(e.ColIndex).Text.Trim <> "" AndAlso IsNumeric(lstDelivery.Columns(e.ColIndex).Text.Trim) = False Then
                    e.Cancel = True
                    lstDelivery.Columns(e.ColIndex).Text = "0"
                    ShowNotification(Me, "Invalid Decimal", 2000, 14, DevComponents.DotNetBar.eToastPosition.MiddleCenter)
                    Exit Try
                End If
                If lstDelivery.Columns(e.ColIndex).Text.Trim = "" OrElse IsNumeric(lstDelivery.Columns(e.ColIndex).Text.Trim) = False OrElse (e.Column.DataColumn.Caption = "Rec. Qty" AndAlso CDbl(lstDelivery.Columns(e.ColIndex).Text.Trim) = 0) Then
                    e.Cancel = True
                    lstDelivery.Columns(e.ColIndex).Text = "0"
                    ShowNotification(Me, "" & e.Column.DataColumn.Caption & " Can't Be Empty " & IIf(e.Column.DataColumn.Caption = "Rec. Qty", " Or 0 ", "").ToString, 2000, 14, DevComponents.DotNetBar.eToastPosition.MiddleCenter)
                    Exit Try
                End If

                If e.Column.DataColumn.DataField = "disc" Then
                    If CDbl(lstDelivery.Columns(e.ColIndex).Text.Trim) >= 100 Then
                        e.Cancel = True
                        ShowNotification(Me, "Discount Should Be Less Than 100 %", 2000, 14, DevComponents.DotNetBar.eToastPosition.MiddleCenter)
                        Exit Try
                    ElseIf CDbl(lstDelivery.Columns(e.ColIndex).Text.Trim) < 0 Then
                        e.Cancel = True
                        ShowNotification(Me, "Discount Should Be Greater Than 0", 2000, 14, DevComponents.DotNetBar.eToastPosition.MiddleCenter)
                        Exit Try
                    End If
                    If lstDelivery.Columns("MOD_OrdQty").Text.Trim = "" OrElse lstDelivery.Columns("MOD_OrdQty").Text.Trim = "" Then
                        e.Cancel = True
                        ShowNotification(Me, "Quantity And Cost Can't Be Empty", 2000, 14, DevComponents.DotNetBar.eToastPosition.MiddleCenter)
                        Exit Try
                    End If
                ElseIf e.Column.DataColumn.DataField = "Cost" OrElse e.Column.DataColumn.DataField = "MOD_OrdQty" Then
                    If CDbl(lstDelivery.Columns(e.ColIndex).Text.Trim) < 0 Then
                        e.Cancel = True
                        ShowNotification(Me, IIf(e.Column.DataColumn.DataField = "MOD_OrdQty", "Qty", "Cost").ToString & " Should Be Greater Than 0", 2000, 14, DevComponents.DotNetBar.eToastPosition.MiddleCenter)
                        Exit Try
                    End If
                    End If
                If e.Column.DataColumn.DataField = "MOD_OrdQty" AndAlso lstDelivery.Columns(e.ColIndex).Text.Trim.Length > 10 Then
                    e.Cancel = True
                    ShowNotification(Me, "Invalid Qty", 2000, 14, DevComponents.DotNetBar.eToastPosition.MiddleCenter)
                    Exit Try
                ElseIf lstDelivery.Columns(e.ColIndex).Text.Trim.Length > 14 Then
                    e.Cancel = True
                    ShowNotification(Me, "Invalid Total Amount", 2000, 14, DevComponents.DotNetBar.eToastPosition.MiddleCenter)
                    Exit Try
                End If
                If lstDelivery.Columns("MOD_OrdQty").Text.Trim = "0" OrElse lstDelivery.Columns("MOD_OrdQty").Text.Trim = "" Then Exit Sub
                    If lstDelivery.Columns("new_old").Text.Trim = "W" Then
                    str = " Select case Meas_Sign WHEN '*' THEN case when pn2.sfp_operator ='*' then  " & Val(txtQty.Text.Trim) & " *  pn2.SFP_UNITPACK   * STK04_Measurements.Meas_Value else "
                        str &= "  " & Val(lstDelivery.Columns("MOD_OrdQty").Text.Trim) & " /  pn2.SFP_UNITPACK   * STK04_Measurements.Meas_Value end "
                        str &= " ELSE  case when pn2.sfp_operator ='*' then (  SFP_UNITPACK   * " & Val(lstDelivery.Columns("MOD_OrdQty").Text.Trim) & " ) /  STK04_Measurements.Meas_Value else (" & Val(lstDelivery.Columns("MOD_OrdQty").Text.Trim) & " / SFP_UNITPACK) /  STK04_Measurements.Meas_Value end End [ctr]"
                        str &= " From " & TempPendingDO & "  pn2 "
                        str &= " left JOIN STK04_Measurements ON Meas_unit = pn2.SFP_SUBUNIT "
                        str &= " WHERE  pn2.SFP_Recid = " & Val(lstDelivery.Columns("MOD_RMRecid").Text.Trim) & " "
                Else
                    str = " SELECT  CASE meas_sign "
                    str &= " WHEN '*' THEN case when stk_operator='*' then " & Val(lstDelivery.Columns("MOD_OrdQty").Text.Trim) & " * stk_unitpack * meas_value else "
                    str &= " (" & Val(lstDelivery.Columns("MOD_OrdQty").Text.Trim) & " / stk_unitpack) * meas_value end "
                    str &= " ELSE  case when stk_operator ='*' then ( stk_unitpack * " & Val(lstDelivery.Columns("MOD_OrdQty").Text.Trim) & " ) / Meas_value else "
                    str &= " (" & Val(lstDelivery.Columns("MOD_OrdQty").Text.Trim) & "  / stk_unitpack ) / Meas_value  end  End as [Ctr] FROM V_Pord2_GetStkMeas WHERE   stk_recid = '" & Val(lstDelivery.Columns("MOD_RMRecid").Text.Trim) & "' "

                    End If
                    lstDelivery.Columns("MOD_ConsOrdQty").Text = ExecuteScalar(str)
                'If lstDelivery.Columns("MOD_ConsOrdQty").Text <> "" AndAlso IsNumeric(lstDelivery.Columns("MOD_ConsOrdQty").Text) AndAlso CDbl(lstDelivery.Columns("MOD_ConsOrdQty").Text).ToString("N3").Length > 10 Then
                'e.Cancel = True
                '   ShowNotification(Me, "Invalid Consolidate Qty", 2000, 14, DevComponents.DotNetBar.eToastPosition.MiddleCenter)
                '  Exit Try
                '  End If
                If DelOrder = "Consignment Receiving Order" Then
                    lstDelivery.Columns("net").Text = CDbl(CDbl(CDbl(lstDelivery.Columns("COST").Text.Trim) * CDbl(lstDelivery.Columns("MOD_OrdQty").Text.Trim)) * CDbl((1 - (CDbl(lstDelivery.Columns("disc").Text.Trim) / 100)))).ToString()
                    If lstDelivery.Columns("net").Text.Length > 14 Then
                        e.Cancel = True
                        ShowNotification(Me, "Invalid Total Amount", 2000, 14, DevComponents.DotNetBar.eToastPosition.MiddleCenter)
                        Exit Try
                    End If
                End If
                If lstDelivery.Columns("MOD_OrdQty").Text.Trim <> "" AndAlso lstDelivery.Columns("NEW_OLD").Text = "S" Then
                    lstDelivery.Columns("NEW_OLD").Text = "N"
                End If
            End If
        Catch ex As Exception
            e.Cancel = True
            MessageBox.Show(ex.Message, cntTitle, MessageBoxButtons.OK, MessageBoxIcon.Error)
        End Try

    End Sub
    Private Sub UPDATEGRID()
        Dim ds As DataSet
        Try
            lstDelivery.UpdateData()
            ds = dsDetDetails.GetChanges
            If Not ds Is Nothing Then
                daDetDetails.Update(ds)
                dsDetDetails.Merge(ds) : ds = Nothing
            End If
            dsDetDetails.AcceptChanges()
            lstDelivery.RefetchRow()
        Catch ex As Exception
            Throw ex
        End Try
    End Sub
    Private Sub lstDelivery_AfterColUpdate(sender As Object, e As ColEventArgs) Handles lstDelivery.AfterColUpdate
        Try
            UPDATEGRID()
            ObjSumNormal = dsDetDetails.Tables("tblDetails").Compute("Sum( MOD_OrdQty)", " MOD_stat='N' ")
            ObjSumFree = dsDetDetails.Tables("tblDetails").Compute("Sum( MOD_OrdQty)", " MOD_stat='F' ")
            ObjSumReturn = dsDetDetails.Tables("tblDetails").Compute("Sum( MOD_OrdQty)", " MOD_stat='R' ")
            objSum = CDbl(IIf(ObjSumNormal Is DBNull.Value, "0", ObjSumNormal)) + CDbl(IIf(ObjSumFree Is DBNull.Value, "0", ObjSumFree)) - CDbl(IIf(ObjSumReturn Is DBNull.Value, "0", ObjSumReturn))
            lstDelivery.Splits(0).DisplayColumns("MOD_OrdQty").DataColumn.FooterText = CType(objSum, Double).ToString("N" & gbDecNo)
        Catch ex As Exception
            MessageBox.Show(ex.Message, cntTitle, MessageBoxButtons.OK, MessageBoxIcon.Error)
        End Try
    End Sub

#Region "Used For Pricing In Xls"
    Private Sub DefaultPriceTypes()
        Dim strsql As String
        Try
            strsql &= " SELECT SP_PIBasepriceType1,SP_PIBasepriceType2 FROM  dbo.stk04_SysParam "
            Using Cnsql As New SqlConnection(ConnectionString)
                Cnsql.Open()
                Using Dt As New DataTable
                    Using Da As New SqlDataAdapter(strsql, Cnsql)
                        Da.Fill(Dt)
                    End Using
                    Str_BasePriceType1 = Dt.Rows(0)("SP_PIBasepriceType1").ToString
                    Str_BasePriceType2 = Dt.Rows(0)("SP_PIBasepriceType2").ToString
                End Using
                Cnsql.Close()
            End Using
        Catch ex As Exception
            Throw ex
        End Try
    End Sub
    Private Sub btnSendToxls_Click(sender As Object, e As EventArgs) Handles btnSendToxls.Click
        Dim ExcellFile As String
        Dim titel As String
        Dim dt As DataTable
        Dim ColNames As String
        Try

            ExcellFile = Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData) + "\" + cntTitle & " For Pricing.xlsx"
            System.IO.File.Delete(ExcellFile)
            Dim FI As New System.IO.FileInfo(ExcellFile)
            Dim Package As New OfficeOpenXml.ExcelPackage(FI)
            strsql = " SELECT  DOH_Recid [RO #],STK_REF [Item Ref] ,"
            strsql &= " STK_DESC [Item Desc],"
            strsql &= " meas_desc [Unit] ,"
            strsql &= " STK_AVGCOSTLC [Avg Cost " & gbCurr1 & "],"
            strsql &= " STK_AVGCOSTCC [Avg Cost " & gbCurr2 & "] , "
            strsql &= " DOD_Recqty [Pur Qty] "
            If DelOrder.ToUpper.Contains("Consignment".ToUpper) Then
                strsql &= " ,  DOH_Curr [PI Curr] ,"
                strsql &= " DOD_Disc [item Disc],"
                strsql &= " DOD_NetCostLC [Net Cost " & gbCurr1 & "]  , "
                strsql &= " DOD_NetCostCC [Net Cost " & gbCurr2 & "]   "
            End If
            If Str_BasePriceType1.Trim <> "" Then strsql &= " ,  0 [" & Str_BasePriceType1 & "]   "
            If Str_BasePriceType2.Trim <> "" Then strsql &= "  , 0  [" & Str_BasePriceType2 & "]   "
            strsql &= " FROM    stk04_PurDelOrdH"
            strsql &= " INNER JOIN stk04_PurDelOrdD ON stk04_PurDelOrdD.DOD_Hrecid = stk04_PurDelOrdH.DOH_Recid"
            strsql &= " INNER JOIN dbo.STK04_STKFIL  ON STK04_STKFIL.STK_RECID = stk04_PurDelOrdD.DOD_StkRecid"
            strsql &= " inner join stk04_measurements on stk_unit= meas_unit "
            strsql &= " INNER JOIN STK04_ItemType ON STK04_ItemType.IT_Code = STK04_STKFIL.STK_ITMTYPE  AND IT_DynamicUpdPrice ='Y'"

            strsql &= "  Where  DOH_Recid = " & txtCode.Text.Trim.Replace("'", "''") & "  order by DOD_Recid "
            Using Dt_Xls As New DataTable
                Using Cnsql As New SqlConnection(ConnectionString)
                    Cnsql.Open()
                    Using Da_xls As New SqlDataAdapter(strsql, Cnsql)
                        Da_xls.Fill(Dt_Xls)
                    End Using
                    Cnsql.Close()
                End Using
                If Dt_Xls.Rows.Count = 0 Then
                    ShowNotification(Me, "No Records Are Allowed To Be Priced", 2000, 14, eToastPosition.MiddleCenter)
                    Exit Try
                End If
                titel = "RO " & txtCode.Text.Trim.Replace("'", "''")
                If Str_BasePriceType1.Trim <> "" Then
                    titel &= " Using Base price " & Str_BasePriceType1 & " And " & Str_BasePriceType2 & ""
                ElseIf Str_BasePriceType1.Trim <> "" Then
                    titel &= " Using Base price " & Str_BasePriceType1 & ""
                ElseIf Str_BasePriceType2.Trim <> "" Then
                    titel &= " Using Base price " & Str_BasePriceType2 & ""
                End If



                SendToXls(Dt_Xls, titel, Package)

                strsql = " SELECT  Price_Code + '|' + Cur_Code + '|' + TTCNT [Base Price Type],"
                strsql &= " Price_Code [Price Type Code] ,"
                strsql &= " Price_Desc [Price Type Description] ,"
                strsql &= " Cur_Code [Curr] ,"
                strsql &= " TTCNT"
                strsql &= " FROM    dbo.Stk04_SalPriceType"
                strsql &= " CROSS JOIN dbo.Com_Currency"
                strsql &= " CROSS JOIN ( SELECT 'TTC' TTCNT"
                strsql &= " UNION ALL"
                strsql &= " SELECT 'NT'"
                strsql &= " ) t1    WHERE Cur_Code IN ('" & gbCurr1 & "','" & gbCurr2 & "') "
                strsql &= " ORDER BY Price_Code"

                Using Cnsql As New SqlConnection(ConnectionString)
                    Cnsql.Open()
                    Using Da_xls As New SqlDataAdapter(strsql, Cnsql)
                        Dt_Xls.Clear()
                        Dt_Xls.Reset()
                        Dt_Xls.Columns.Clear()
                        Da_xls.Fill(Dt_Xls)
                    End Using
                    Cnsql.Close()
                End Using
                SendToXls(Dt_Xls, "list Of Base Prices", Package)

                Package.Save()
                Package.File.Attributes = IO.FileAttributes.Hidden
                Process.Start(ExcellFile)


            End Using
        Catch ex As Exception
            If ex.Message.Contains("being used by another process") Then
                MessageBox.Show("Excel File is Already Opened", cntTitle, MessageBoxButtons.OK, MessageBoxIcon.Information)
            Else
                MessageBox.Show("Error: " & ex.Message, cntTitle, MessageBoxButtons.OK, MessageBoxIcon.Error)
            End If
        End Try
    End Sub
    Private Sub SendToXls(ByVal dt As DataTable, ByVal titel As String, ByRef Package As OfficeOpenXml.ExcelPackage)
        Try
            Dim WS As OfficeOpenXml.ExcelWorksheet
            Dim MyrichTextcoloction As OfficeOpenXml.Style.ExcelRichTextCollection
            Dim MyrichText As OfficeOpenXml.Style.ExcelRichText
            WS = Package.Workbook.Worksheets.Add(titel.Trim)
            WS.Cells(1, 1).LoadFromDataTable(dt, True)
            WS.Cells(1, 1, 1, dt.Columns.Count).Style.Fill.PatternType = OfficeOpenXml.Style.ExcelFillStyle.Solid
            WS.Cells(1, 1, 1, dt.Columns.Count).Style.Fill.BackgroundColor.SetColor(Color.FromArgb(0, 88, 176))
            WS.Cells(1, 1, 1, dt.Columns.Count).Style.Font.Bold = True
            WS.Cells(1, 1, 1, dt.Columns.Count).Style.Font.Color.SetColor(Color.White)
            WS.Cells(1, 1, 1, dt.Columns.Count).Style.HorizontalAlignment = OfficeOpenXml.Style.ExcelHorizontalAlignment.Left
            WS.Cells(1, 1, WS.Dimension.Rows, dt.Columns.Count).AutoFitColumns()
            WS.Cells(1, 1, WS.Dimension.Rows, dt.Columns.Count).AutoFilter = True
            WS.Cells(1, 1, WS.Dimension.Rows, dt.Columns.Count).Style.Border.Top.Style = OfficeOpenXml.Style.ExcelBorderStyle.Thin
            WS.Cells(1, 1, WS.Dimension.Rows, dt.Columns.Count).Style.Border.Top.Color.SetColor(Color.White)
            WS.Cells(1, 1, WS.Dimension.Rows, dt.Columns.Count).Style.Border.Bottom.Style = OfficeOpenXml.Style.ExcelBorderStyle.Thin
            WS.Cells(1, 1, WS.Dimension.Rows, dt.Columns.Count).Style.Border.Bottom.Color.SetColor(Color.White)
            WS.Cells(1, 1, WS.Dimension.Rows, dt.Columns.Count).Style.Border.Right.Style = OfficeOpenXml.Style.ExcelBorderStyle.Thin
            WS.Cells(1, 1, WS.Dimension.Rows, dt.Columns.Count).Style.Border.Right.Color.SetColor(Color.White)
            WS.Cells(1, 1, WS.Dimension.Rows, dt.Columns.Count).Style.Border.Left.Style = OfficeOpenXml.Style.ExcelBorderStyle.Thin
            WS.Cells(1, 1, WS.Dimension.Rows, dt.Columns.Count).Style.Border.Left.Color.SetColor(Color.White)
            WS.Row(1).Height = 15.75
            WS.Cells(2, 1, WS.Dimension.Rows, dt.Columns.Count).Style.Fill.PatternType = OfficeOpenXml.Style.ExcelFillStyle.Solid
            WS.Cells(2, 1, WS.Dimension.Rows, dt.Columns.Count).Style.Fill.BackgroundColor.SetColor(Color.FromArgb(221, 235, 247))
            For k As Integer = 1 To dt.Columns.Count + 1
                WS.Column(k).Width += 4
            Next
            WS.PrinterSettings.PaperSize = OfficeOpenXml.ePaperSize.A4
            WS.PrinterSettings.Orientation = OfficeOpenXml.eOrientation.Landscape
        Catch ex As Exception
            Throw ex
        End Try
    End Sub

#End Region


    Private Sub BtnImportFromXls_Click(sender As Object, e As EventArgs) Handles BtnImportFromXls.Click
        Dim ImpFromExlsTemp As String
        Dim ExlPatha As String
        Dim Strsql As String
        Dim Str As String
        Dim dsSQL As DataSet
        Dim dt As DataTable
        Dim drSQL As SqlDataReader
        Dim drSQL2 As SqlDataReader
        Dim ChkCol As Boolean
        Dim ChkBar As Boolean
        Dim Color As String
        Dim SuppRef As String
        Dim SuppColor As String
        Dim ItmClass As String
        Dim Barcode As String
        Dim LastCost As Double
        Dim ValidItem As Boolean
        Dim UpdSupRef As Boolean
        Dim MSG As String
        Dim RevMsg As String
        Dim RevQty As Boolean
        Dim i, j, k As Integer
        Dim XVal1, XVal2 As Double
        Dim diffcurr As Boolean
        Dim ChkSupRef As Boolean
        Dim ChkSupColor As Boolean
        Dim ChkItmClass As Boolean
        Dim cnSql As SqlConnection
        Dim cmSql As SqlCommand
        Dim ValidLocation As Boolean
        Dim SendMessage As String
        Dim ItemFromSupp As Boolean
        Dim TempNoItemFromSupp As String
        Dim ShowSupCol As Boolean
        Dim SupCol As String
        Dim discount, Quantity As Double
        Dim Str_barcode, BarcodeToEnter As String
        Dim statusItem As String
        Try
            If Wtype = "DELETE" Or Wtype = "VIEW" Then Exit Sub
            If Wtype = "MODIFY" And txtCode.Text = "" Then Exit Sub
            If Print.Visible = True Then Print.Visible = False
            If lstDelivery.Enabled = False Then Exit Sub
            ClearGridFiltration(LstExpiry)

            diffcurr = False
            Cursor.Current = Cursors.WaitCursor
            If txtDate.Value Is DBNull.Value Or txtSupp.Text.Trim = "" Or txtDelOrder.Text.Trim = "" Or txtLoc.Text.Trim = "" Or
                ((DelOrder = "Consignment Receiving Order" Or DelOrder = "Consignment Receiving Order Fashion") And txtCurr.Text.Trim = "") Then
                MessageBoxEx.Show("The Supplier ,RO Ref #,Location, Currency & Delivery Date Should be Filled.", cntTitle, MessageBoxButtons.OK, MessageBoxIcon.Exclamation)
                If txtSupp.Text = "" Then txtSupp.Focus() : Exit Sub
                If txtDelOrder.Text = "" Then txtDelOrder.Focus() : Exit Sub
                If txtLoc.Text = "" Then txtLoc.Focus() : Exit Sub
                If (DelOrder = "Consignment Receiving Order" Or DelOrder = "Consignment Receiving Order Fashion") And txtCurr.Text.Trim = "" Then txtCurr.Focus() : Exit Sub
                txtDate.Focus() : Exit Sub
            End If

            If DelOrder = "Consignment Receiving Order Fashion" Or DelOrder = "Receiving Order Fashion" Then
                If txtBrand.Text = "" Or txtSeason.Text = "" Then
                    MessageBoxEx.Show("Brand & Season Should be Filled.", cntTitle, MessageBoxButtons.OK, MessageBoxIcon.Exclamation)
                    If txtBrand.Text = "" Then txtBrand.Focus() : Exit Sub
                    If txtSeason.Text = "" Then txtSeason.Focus() : Exit Sub
                End If
            Else
                If UCase(txtItmClass.Text.Trim) = "E" Then
                    If Val(LstExpiry.Columns("DOB_Expirydate").Text.Trim) = 0 Then
                        MessageBox.Show("Expiry Date Can't Be Null.", cntTitle, MessageBoxButtons.OK, MessageBoxIcon.Exclamation)
                        If GrpExpBarcode.Visible = False Then GrpExpBarcode.Visible = True
                        LstExpiry.Focus()
                        Exit Sub
                    End If
                    If LstExpiry.Columns("DOB_BatchInfo").Text.Trim = "" Then
                        MessageBox.Show("Lot No Can't Be Null.", cntTitle, MessageBoxButtons.OK, MessageBoxIcon.Exclamation)
                        If GrpExpBarcode.Visible = False Then GrpExpBarcode.Visible = True
                        LstExpiry.Focus()
                        Exit Sub
                    End If
                End If
            End If


            ImpFromExlsTemp = "ImpFromExlsTemp_" & lcSteps & gbUser
            Using Frmimpexl As New FrmImportFromXls
                With Frmimpexl
                    .TempRetiveTable = ImpFromExlsTemp
                    .Str_BasePriceType1 = ""
                    .Str_BasePriceType2 = ""
                    If DelOrder = "Consignment Receiving Order" Or DelOrder = "Consignment Receiving Order Fashion" Then
                        .from = "ROC"
                    Else
                        .from = "RO"
                    End If
                    If .ShowDialog() = Windows.Forms.DialogResult.Cancel Then
                        Exit Try
                    End If
                    .ExlPatha = ExlPatha
                End With
            End Using
            Cursor.Current = Cursors.WaitCursor
            cnSql = New SqlConnection(ConnectionString)
            cnSql.Open()
            Strsql = " SELECT  ItemRecID RecID , ItemRef,ItemBarcode ,"
            Strsql &= " [Line No],Quantity,[Unit Cost],[Item Discount] ,[Line No]"
            Strsql &= " From  " & ImpFromExlsTemp & "  ORDER BY CONVERT(NUMERIC(18,0) ,[Line No]  )  "
            ReadFromDBToDataSet(Strsql, "tbStkSearch", dsSQL)
            dt = dsSQL.Tables("tbStkSearch")
            CreateTempNonRevItems(TempNonRevItems)
            cmSql = New SqlCommand(Strsql, cnSql)
            For j = 0 To dt.Rows.Count - 1
                ValidItem = True
                Str_barcode = ""
                Str_barcode = dt(j)("ItemBarcode").ToString.Trim
                BarcodeToEnter = Str_barcode
                If Str_barcode.Trim = "" Then Str_barcode = GetItemBatcode(dt(j)("RecID").ToString)
                If IsNumeric(dt(j)("Quantity").ToString) Then
                    Quantity = CDbl(dt(j)("Quantity").ToString)
                Else
                    Quantity = 1
                End If
                If IsNumeric(dt(j)("Unit Cost").ToString) AndAlso (DelOrder = "Consignment Receiving Order" Or DelOrder = "Consignment Receiving Order Fashion") Then 'AndAlso CDbl(dt(j)("Unit Cost").ToString) > 0 
                    LastCost = CDbl(dt(j)("Unit Cost").ToString)
                End If
                If Quantity < 0 Then
                    statusItem = "R"
                    Quantity = Math.Abs(Quantity)
                    LastCost = Math.Abs(LastCost)
                Else
                    statusItem = "N"
                End If

                If IsNumeric(dt(j)("Item Discount").ToString) AndAlso CDbl(dt(j)("Item Discount").ToString) <> -1 AndAlso CDbl(dt(j)("Item Discount").ToString) > 0 Then
                    discount = CDbl(dt(j)("Item Discount").ToString)
                End If

                ValidItem = True
                SuppRef = "" : SuppColor = ""
                ItmClass = ExecuteScalar("SELECT STK_ItmClass FROM STK04_STKFIL WHERE STK_RECID = " & Val(dt(j)("RecID"))).ToString
                Strsql = " SELECT TOP 1 SRef_SupRef, SRef_SupColor FROM STK04_StkSupRef"
                Strsql &= " WHERE SRef_SupRecId = '" & GetFieldDesc("Stk04_supplier", "sup_code='" & txtSupp.Text.Trim & "'", "sup_recid") & "'"
                Strsql &= " AND SRef_STKRECID = " & dt(j)("RecID").ToString
                Strsql &= " ORDER BY SREF_RecId DESC"
                ReadFromDBToDataReader(Strsql, drSQL)
                If drSQL.HasRows Then
                    drSQL.Read()
                    SuppRef = drSQL.Item(0).ToString
                    If (DelOrder = "Consignment Receiving Order Fashion" Or DelOrder = "Receiving Order Fashion") And SuppColor = "" And Not IsDBNull(drSQL.Item(1)) Then
                        SuppColor = drSQL.Item(1).ToString
                    End If
                End If

                RevQty = False
                ValidLocation = True
                If ValidItem And RevQty = False Then
                    ' if item already exist update the old one 
                    If RecordsExists(" select top 1 1 from " & TmpGridDetails & " where MOD_stat ='N'  and  MOD_RMRecid =" & dt(j)("RecID").ToString) AndAlso Wtype = "ADD" Then
                        Strsql = "  update  " & TmpGridDetails & " set MOD_OrdQty = MOD_OrdQty + " & Quantity & " , MOD_ConsOrdQty =  (SELECT CASE Meas_Sign WHEN '*' THEN "
                        Strsql &= "  case when stk_operator ='*' then (MOD_OrdQty + " & Quantity & ") * Stk_UnitPack * Meas_Value  else (MOD_OrdQty + " & Quantity & ") / Stk_UnitPack * Meas_Value  end ELSE"
                        Strsql &= " case when stk_operator ='*' then Stk_unitPack * ( MOD_OrdQty + " & Quantity & ") /Meas_value else ((MOD_OrdQty + " & Quantity & ") /Stk_unitPack  ) /Meas_value end   "
                        Strsql &= " END FROM V_Pord2_GetStkMeas  WHERE STK_Recid=" & Val(dt(j)("RecID")) & ") where MOD_stat ='N'  and  MOD_RMRecid =" & dt(j)("RecID").ToString
                        cmSql.CommandText = Strsql
                        cmSql.ExecuteNonQuery()

                    Else
                        Strsql = " INSERT INTO " & TmpGridDetails
                        Strsql &= " SELECT ROW_NUMBER() OVER (ORDER BY stk_recid) + "
                        Strsql &= " (SELECT ISNULL(MAX(recid), 0) FROM " & TmpGridDetails & " ) , TEMP.*"
                        Strsql &= " from (Select DISTINCT  null col1,  "
                        If SuppRef <> "" Then
                            Strsql &= " '" & SuppRef.Replace("'", "''") & "' As MOD_supitmref,"
                        Else
                            Strsql &= " isnull(stkFil.STK_SINGLEREF1,'" & Str_barcode.Trim & "') As MOD_supitmref,"
                        End If
                        If DelOrder = "Receiving Order Fashion" Or DelOrder = "Consignment Receiving Order Fashion" Then
                            Strsql &= "null  col3,  "
                        End If
                        Strsql &= " stk_ref    ,"
                        If DelOrder = "Receiving Order Fashion" Or DelOrder = "Consignment Receiving Order Fashion" Then
                            Strsql &= "stk_gender,"
                        End If
                        Strsql &= "  '" & statusItem & "'   Status,"
                        If DelOrder = "Receiving Order Fashion" Or DelOrder = "Consignment Receiving Order Fashion" Then
                            Strsql &= "  stk_ItmType ,stk_Style,"
                        End If
                        Strsql &= " " & Val(dt(j)("RecID")) & "  stk_Recid,1 count,"
                        Strsql &= " Stk_DESC,"
                        If DelOrder = "Receiving Order Fashion" Or DelOrder = "Consignment Receiving Order Fashion" Then
                            Strsql &= " null col7,null col8,"
                        End If
                        Strsql &= " " & Quantity & " AS OrdQty , measu.meas_desc stk_unit, measub.meas_desc stk_subUNIT, "

                        Strsql &= " (SELECT CASE Meas_Sign WHEN '*' THEN case when stk_operator ='*' then " & Quantity & " * Stk_UnitPack * Meas_Value  else " & Quantity & " / Stk_UnitPack * Meas_Value  end ELSE"
                        Strsql &= " case when stk_operator ='*' then Stk_unitPack * " & Quantity & " /Meas_value else (" & Quantity & " /Stk_unitPack  ) /Meas_value end  END FROM V_Pord2_GetStkMeas"
                        Strsql &= " WHERE STK_Recid=" & Val(dt(j)("RecID")) & ") AS ConsOrdQty,   stk_VATType  VAT   "
                        If DelOrder <> "Consignment Receiving Order" AndAlso DelOrder <> "Consignment Receiving Order Fashion" Then
                            Strsql &= "  ,  stk_avgcostcc  UnitCost  "
                        Else
                            Strsql &= "  , " & LastCost & "  UnitCost  "
                        End If
                        If DelOrder = "Consignment Receiving Order" Then
                            Strsql &= " ,  " & discount & " AS ItemDisc ,  Round(" & Quantity & " * ROUND(abs(" & LastCost & "),3) * (1-(" & discount & "/100.000)),3) AS   net  "
                        End If
                        Strsql &= " , stk_fccode curr, 'N' col11 , Null PO  ,'' barcode"
                        Strsql &= ", stk_ItmClass  , '' BTN"
                        Strsql &= " FROM stk04_stkfil stkFil"
                        Strsql &= " INNER JOIN stk04_measurements measu ON meas_unit = stk_unit "
                        Strsql &= " INNER JOIN stk04_measurements measub ON measub.meas_unit = stk_subunit "
                        Strsql &= " WHERE stkFil.STK_RECID = " & dt(j)("RecID").ToString
                        If DelOrder = "Consignment Receiving Order" Or DelOrder = "Consignment Receiving Order Fashion" Then
                            Strsql &= "  and STK_NEWUSED ='C' and stk_supplier = '" & txtSupp.Text.Trim & "'"
                        Else
                            Strsql &= "  and STK_NEWUSED <>'C'"
                        End If
                        Strsql &= " ) TEMP"
                        cmSql.CommandText = Strsql
                        cmSql.ExecuteNonQuery()


                        If ItmClass.ToUpper.Trim.StartsWith("T") AndAlso BarcodeToEnter.Trim <> "" AndAlso
                                    Not RecordsExists(" SELECT TOP 1 1 FROM " & TmpGridBarcode & " WHERE DOB_Barcode = '" & BarcodeToEnter.Trim.Replace("'", "''") & "'") Then
                            Dim dbarecid As String = ExecuteScalar(" SELECT TOP 1 dbar_recid FROM dbo.Stk04_DetBarcode WHERE dbar_barcode = '" & BarcodeToEnter.Trim.Replace("'", "''") & "' AND dbar_stkid = " & dt(j)("RecID").ToString)
                            If dbarecid.Trim <> "" Then
                                Strsql = " INSERT  INTO " & TmpGridBarcode
                                Strsql &= "         SELECT  ISNULL(MAX(B_RECID), 0) + 1 , null ,"
                                Strsql &= "                 (SELECT ISNULL(MAX(recid), 0) FROM " & TmpGridDetails & " ) ,"
                                Strsql &= "                 NULL ,"
                                Strsql &= "                 '" & BarcodeToEnter.Trim.Replace("'", "''") & "' ,"
                                Strsql &= "                 'O' ,null FROM " & TmpGridBarcode
                                cmSql.CommandText = Strsql
                                cmSql.ExecuteNonQuery()
                            Else
                                Strsql = " SELECT TOP 1 1 FROM dbo.Stk04_DetBarcode WHERE dbar_barcode = '" & BarcodeToEnter.Trim.Replace("'", "''") & "'"
                                Strsql &= " UNION "
                                Strsql &= " SELECT TOP 1 1 FROM dbo.STK04_InvDBE WHERE INVB_Barcode = '" & BarcodeToEnter.Trim.Replace("'", "''") & "'"
                                If Not RecordsExists(Strsql) Then
                                    Strsql = " INSERT  INTO " & TmpGridBarcode
                                    Strsql &= "         SELECT  ISNULL(MAX(B_RECID), 0) + 1 ,Null ,"
                                    Strsql &= "                 (SELECT ISNULL(MAX(recid), 0) FROM " & TmpGridDetails & " ) ,"
                                    Strsql &= "                 NULL ,"
                                    Strsql &= "                 '" & BarcodeToEnter.Trim.Replace("'", "''") & "' ,"
                                    Strsql &= "                 'N' , null FROM " & TmpGridBarcode
                                    cmSql.CommandText = Strsql
                                    cmSql.ExecuteNonQuery()
                                End If
                            End If
                        End If

                    End If
                End If
            Next
            If (DelOrder = "Consignment Receiving Order" Or DelOrder = "Consignment Receiving Order Fashion") Then
                ShowNotification(Me, "Only Consignment Items For The Selected Supplier Will Be Imported ", 5000, 13, eToastPosition.MiddleCenter)
            End If
            cnSql.ClearAllPools()
            lcChanges = True
            dsDetDetails.Tables(0).Clear()
            daDetDetails.Fill(dsDetDetails, dsDetDetails.Tables(0).TableName)
            lstDelivery.Refresh()

            ObjSumNormal = dsDetDetails.Tables("tblDetails").Compute("Sum( MOD_OrdQty)", " MOD_stat='N' ")
            ObjSumFree = dsDetDetails.Tables("tblDetails").Compute("Sum( MOD_OrdQty)", " MOD_stat='F' ")
            ObjSumReturn = dsDetDetails.Tables("tblDetails").Compute("Sum( MOD_OrdQty)", " MOD_stat='R' ")
            objSum = CDbl(IIf(ObjSumNormal Is DBNull.Value, "0", ObjSumNormal)) + CDbl(IIf(ObjSumFree Is DBNull.Value, "0", ObjSumFree)) - CDbl(IIf(ObjSumReturn Is DBNull.Value, "0", ObjSumReturn))
            lstDelivery.Splits(0).DisplayColumns("MOD_OrdQty").DataColumn.FooterText = CType(objSum, Double).ToString("N" & gbDecNo)

            UpdateDtQualityControlHeader()
            ReadonlyConsFash()
            RefQtyLbl(False)
            LstVisbility(False, False, lstDelivery.Columns("ItmClass").Text.Trim)
            txtSupRef.Focus()
        Catch ex As Exception
            MessageBox.Show("Error: " & ex.Message, cntTitle, MessageBoxButtons.OK, MessageBoxIcon.Error)
        Finally
            Disposecontrols(cnSql, cmSql)
            Cursor.Current = Cursors.Default
        End Try
    End Sub
    Private Sub CreateTempNonRevItems(ByVal TempTable As String)
        Dim Str As String
        'Create the table TempNonRevItems to insert the invalid items (not inserted to the grid) & the reason
        Try
            DropSqlObject(TempTable, "TABLE")
            Str = "SELECT STK_Recid AS ItemRecid, STK_Ref AS ItemRef, SRef_SupColor AS SupCol,"
            Str &= " STK_Desc AS ItemDesc, Color_Reference AS Color, CONVERT(VARCHAR(1000),'') AS Reason"
            Str &= " INTO " & TempTable & " FROM STK04_STKFIL"
            Str &= " INNER JOIN STK04_StkSupRef ON STK_RECID = SRef_StkRecId"
            Str &= " INNER JOIN Stk04_COLOR ON SRef_ItmColor = Color_Code"
            Str &= " WHERE 1 = 2"
            Execute(Str)
        Catch ex As Exception
            Throw ex
        End Try
    End Sub
    Private Function GetItemBatcode(ByVal ItemRecid As String) As String
        Dim Obj_barcode As Object
        Dim Str_barcode As String
        Dim strsql As String
        Try
            strsql = " SELECT TOP 1"
            strsql &= " ISNULL(dbar_barcode, '')"
            strsql &= " FROM    stk04_detbarcode"
            strsql &= " WHERE   dbar_stkid = " & ItemRecid & ""
            strsql &= " AND ISNULL(dbar_grid, '') = ''"
            strsql &= " AND ISNULL(dbar_Detgrid, '')= ''"
            strsql &= " AND ISNULL(dbar_color, '')= ''"
            strsql &= " AND ISNULL(dbar_expdate, '')= ''"
            strsql &= " AND ISNULL(dbar_batch, '')= ''"
            strsql &= " ORDER BY dbar_recid DESC"

            Using Br_Cnsql As New SqlConnection(ConnectionString)
                Br_Cnsql.Open()
                Using Br_Cmsql As New SqlCommand(strsql, Br_Cnsql)
                    Obj_barcode = Nothing
                    Str_barcode = ""
                    Obj_barcode = Br_Cmsql.ExecuteScalar
                    If Not (Obj_barcode Is DBNull.Value OrElse Obj_barcode Is Nothing) Then
                        Str_barcode = Obj_barcode.ToString
                    End If
                End Using
                Br_Cnsql.Close()
            End Using
            Return Str_barcode
        Catch ex As Exception
            Throw ex
        End Try
    End Function
    Private Sub InsertNonRevItems(ByVal TempTable As String, ByVal StkRecid As String, ByVal Reason As String, _
                                  Optional ByVal SupCol As String = "", Optional ByVal Color As String = "")
        Dim Str As String
        Try
            Str = "INSERT INTO " & TempTable
            Str &= " SELECT STK_Recid, STK_Ref, '" & SupCol & "', STK_Desc,"
            Str &= " '" & Color & "', '" & Reason.Replace("'", "''") & "' AS Reason"
            Str &= " FROM STK04_STKFIL WHERE STK_Recid = " & StkRecid
            Execute(Str)
        Catch ex As Exception
            Throw ex
        End Try
    End Sub

    Private Sub GroupBox1_Validating(sender As Object, e As System.ComponentModel.CancelEventArgs) Handles GroupBox1.Validating
        Try
            If txtSupp.Text.Trim <> "" AndAlso DelOrder <> "Consignment Receiving Order" AndAlso DelOrder <> "Consignment Receiving Order Fashion" AndAlso Wtype = "ADD" _
                AndAlso GetFieldDesc("stk04_supplier", "sup_active='Y' and SUP_Code='" & txtSupp.Text.Trim.Replace("'", "''") & "'", "SUP_APPROVE") = "Y" And txtOrderNo.Text.Trim = "" AndAlso Not gbIsOffDb Then
                If txtDate.Value Is DBNull.Value Then
                    MessageBox.Show("Date Can Not Be Null.", cntTitle, MessageBoxButtons.OK, MessageBoxIcon.Exclamation)
                    txtRef.Text = "" : txtSupRef.Text = "" : txtSupColor.Text = ""
                    txtDate.Focus()
                    e.Cancel = True
                    Exit Sub
                ElseIf txtDelOrder.Text.Trim = "" Then
                    MessageBox.Show("You Should enter A Receiving Order Ref.", cntTitle, MessageBoxButtons.OK, MessageBoxIcon.Exclamation)
                    txtRef.Text = "" : txtSupRef.Text = "" : txtSupColor.Text = ""
                    txtDelOrder.Focus()
                    e.Cancel = True
                    Exit Sub
                End If
                Dim msg As String
                msg = "Select Purchase Order From Approved PO List"
                If Not Me.ActiveControl Is btnPurchOrder Then
                    MessageBoxEx.Show(msg, cntTitle, MessageBoxButtons.OK, MessageBoxIcon.Exclamation)
                End If
                supplierneedapproval = True
                btnPurchOrder_Click(btnPurchOrder, New System.EventArgs) : Exit Sub
            Else
                supplierneedapproval = False
            End If
        Catch ex As Exception
            MessageBox.Show("Error: " & ex.Message, cntTitle, MessageBoxButtons.OK, MessageBoxIcon.Error)
        End Try
    End Sub

    Private Sub LstExpiry_Click(sender As Object, e As EventArgs) Handles LstExpiry.Click

    End Sub

    Private Sub DefaultSeason()
        Dim strDefaultSeason As String
        Try

            If cntTitle.Contains("With Size And Color Matrix") Then

                If txtSeason.Text.Trim = "" Then

                    strDefaultSeason = ExecuteScalar_WOXACT(" SELECT Seas_Code FROM dbo.Stk04_Season WITH (NOLOCK) WHERE Seas_CurrOpSeas = 'Y' ")

                    If Not String.IsNullOrEmpty(strDefaultSeason) Then
                        txtSeason.Text = strDefaultSeason
                        txtSeason_Validating(txtSeason, New System.ComponentModel.CancelEventArgs)
                    End If

                End If

            End If

        Catch ex As Exception

        End Try
    End Sub

    Private Sub btnFashionRep_Click(sender As Object, e As EventArgs) Handles btnFashionRep.Click
        Try
            If txtCode.Text.Trim = "" Then Exit Try

            FashionReport(txtCode.Text.Trim, $"tmp_RepRO_Matrix_{gbComp}_{gbUser}", "RO", lstMatrix)

        Catch ex As Exception
            MessageBox.Show("Error : " & ex.Message, cntTitle, MessageBoxButtons.OK, MessageBoxIcon.Error)
        End Try
    End Sub

End Class