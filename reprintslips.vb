Imports C1.Win.C1Input
Imports System.Data.SqlClient
Imports System.Text
Imports C1.Win.C1TrueDBGrid
Imports UserControls.ERPHelp

Public Class frmReprintSlips

    Dim cntTitle As String = "Reprint Documents"
    Dim lcChanges As Boolean
    Dim lcPrivView As Boolean
    Dim lcPrivPrint As Boolean
    Dim lcSteps As String
    Dim current_S As C1.Win.C1TrueDBGrid.Style
    Dim normal_S As C1.Win.C1TrueDBGrid.Style
    Dim TempTable, TempTableDO As String
    Dim ResOrderRecID As Long
    Dim dsRepSlip As DataSet
    Dim daRepSlip As SqlDataAdapter
    Dim oldvalue, olddesc As String
    Private strPrev(,) As String

    '...Jad  
    Private AllowViewTransTotal As Boolean
    Private chkExcelLayoutVisible As Boolean
    Dim loading As Boolean

    Public Overloads Sub show(ByRef _Steps As String)
        Try
            lcSteps = _Steps
            Me.Show()
            Application.DoEvents()
        Catch exp As Exception
            Throw exp
        End Try
    End Sub

    Private Sub btnTypeSlip_Click(ByVal sender As System.Object, ByVal e As System.EventArgs) Handles btnTypeSlip.Click
        Dim frm As frmSearch
        Dim tblRow(19, 1) As String

        Dim Sizefield() As Int16 = {80, 240}
        Dim Alnfield() As String = {"C", "N"}
        Dim Retfield() As String = {"Code", "Description"}
        Try
            btnTypeSlip.Enabled = False
            Cursor.Current = Cursors.WaitCursor
            tblRow(0, 0) = "IN" : tblRow(0, 1) = "Invoice"
            tblRow(1, 0) = "CN" : tblRow(1, 1) = "Credit Note With Item Qty"
            tblRow(2, 0) = "CV" : tblRow(2, 1) = "Credit Note Value"
            tblRow(3, 0) = "UR" : tblRow(3, 1) = "Repairs"
            tblRow(4, 0) = "A1" : tblRow(4, 1) = "Adjustment In"
            tblRow(5, 0) = "A2" : tblRow(5, 1) = "Adjustment Out"
            tblRow(6, 0) = "PU" : tblRow(6, 1) = "Purchase of Used Books"
            tblRow(7, 0) = "PX" : tblRow(7, 1) = "Reverse Purchase of Used Books"
            tblRow(8, 0) = "DO" : tblRow(8, 1) = "Delivery Order"
            tblRow(9, 0) = "CI" : tblRow(9, 1) = "Cash In"
            tblRow(10, 0) = "CO" : tblRow(10, 1) = "Cash Out"
            tblRow(11, 0) = "TR" : tblRow(11, 1) = "Transposition"
            tblRow(12, 0) = "AS" : tblRow(12, 1) = "Assembly"
            tblRow(13, 0) = "DS" : tblRow(13, 1) = "Disposal"
            tblRow(14, 0) = "RC" : tblRow(14, 1) = "Client Receipt"
            'Christina Add A New Type of Splits ,WR -Reservation
            tblRow(15, 0) = "WR" : tblRow(15, 1) = "Reservation"

            tblRow(16, 0) = "CS" : tblRow(16, 1) = "Consumption Slip" 'added by GS on 09/05/16
            tblRow(17, 0) = "PR" : tblRow(17, 1) = "Production" 'added by ty on 29/07/16
            tblRow(18, 0) = "DV" : tblRow(18, 1) = "Debit Note Value"
            tblRow(19, 0) = "RX" : tblRow(19, 1) = "Pre-Receipt"
            frm = New frmSearch
            With frm
                .Owner = Me
                If .ShowDialog("Return", CreateDataTable("tblBasedOn", "Code", "Description", tblRow), 4,
                   "Type of Document", Sizefield, Alnfield, Retfield, 0, Nothing, False, , 500, , True) = DialogResult.OK Then

                    If gbRetSearch(0) <> "" Then
                        txtTypeSlip.Text = gbRetSearch(0)
                        lblTypeSlip.Text = gbRetSearch(1)
                    End If
                    txtTypeSlip.Focus()
                    SendKeys.Send("{TAB}")
                Else
                    txtTypeSlip.Focus()
                End If
            End With
            Catch exp As Exception
            MessageBox.Show("Error: " & exp.Message, cntTitle, MessageBoxButtons.OK, MessageBoxIcon.Error)
        Finally
            btnTypeSlip.Enabled = True
            gbRetSearch = Nothing
            frm = Nothing
            Retfield = Nothing
            Alnfield = Nothing
            Sizefield = Nothing
            Cursor.Current = Cursors.Default
        End Try
    End Sub

    Private Sub txtTypeSlip_Enter(ByVal sender As Object, ByVal e As System.EventArgs) Handles txtTypeSlip.Enter
        Try
            FocusStyle(CType(sender, TextBox), "Enter")
        Catch exp As Exception
            MessageBox.Show("Error : " & exp.Message, cntTitle, _
            MessageBoxButtons.OK, MessageBoxIcon.Error)
        End Try
    End Sub

    Private Sub txtTypeSlip_KeyDown(ByVal sender As Object, ByVal e As System.Windows.Forms.KeyEventArgs) Handles txtTypeSlip.KeyDown
        Try
            If btnTypeSlip.Visible = True AndAlso e.Modifiers = Keys.Alt AndAlso e.KeyCode = Keys.Down Then
                btnTypeSlip_Click(btnTypeSlip, New System.EventArgs)
            End If
        Catch ex As Exception
            MessageBox.Show("Error: " & ex.Message, cntTitle, MessageBoxButtons.OK, MessageBoxIcon.Error)
        End Try
    End Sub

    Private Sub txtTypeSlip_KeyPress(ByVal sender As Object, ByVal e As System.Windows.Forms.KeyPressEventArgs) Handles txtTypeSlip.KeyPress
        Try
            If Asc(e.KeyChar) = 22 Then
                e.Handled = True : ctMenuPast(Me, "Char")
            Else
                e.Handled = ValidGet(CType(sender, TextBox), e.KeyChar, StringType.Other, 2, , False)
            End If
        Catch ex As Exception
            MessageBox.Show("Error: " & ex.Message, cntTitle, MessageBoxButtons.OK, MessageBoxIcon.Error)
        End Try
    End Sub

    Private Sub txtTypeSlip_Leave(ByVal sender As Object, ByVal e As System.EventArgs) Handles txtTypeSlip.Leave
        Try
            FocusStyle(CType(sender, TextBox), "Leave")
        Catch exp As Exception
            MessageBox.Show("Error : " & exp.Message, cntTitle, _
            MessageBoxButtons.OK, MessageBoxIcon.Error)
        End Try
    End Sub

    Private Sub txtTypeSlip_MouseDown(ByVal sender As Object, ByVal e As System.Windows.Forms.MouseEventArgs) Handles txtTypeSlip.MouseDown
        Try
            If e.Button = MouseButtons.Right Then ShowContextMenu(sender)
        Catch ex As Exception
            MessageBox.Show("Error : " & ex.Message, cntTitle, _
MessageBoxButtons.OK, MessageBoxIcon.Error)
        End Try
    End Sub

    Private Sub txtTypeSlip_TextChanged(ByVal sender As Object, ByVal e As System.EventArgs) Handles txtTypeSlip.TextChanged
        Try
            If GbRc.Visible Then GbRc.Visible = False
            ErrorProvider1.SetError(CType(sender, TextBox), "")
            lblTypeSlip.Text = ""

            If txtTypeSlip.Text = "" Then
                rbLaser.Enabled = True : rbSlip.Enabled = True
                oldvalue = ""
                olddesc = ""
            End If

            lblPreReceiptNote.Visible = (txtTypeSlip.Text.Trim.ToUpper = "RX")

        Catch ex As Exception
            MessageBox.Show("Error : " & ex.Message, cntTitle, _
            MessageBoxButtons.OK, MessageBoxIcon.Error)
        End Try
    End Sub


    Private Sub txtTypeSlip_Validating(ByVal sender As Object, ByVal e As System.ComponentModel.CancelEventArgs) Handles txtTypeSlip.Validating
        Try
            If txtTypeSlip.Text.Trim <> "" Then

                If txtTypeSlip.Text.IndexOf("'") >= 0 Then
                    e.Cancel = True
                    MessageBox.Show("Invalid Data Entry, ['] Not Allowed.", cntTitle, _
                    MessageBoxButtons.OK, MessageBoxIcon.Information)
                    Exit Try
                End If

                If txtTypeSlip.Text <> oldvalue Then
                    rbSlip.Enabled = True
                    rbLaser.Checked = False
                    If DefaultLayout = "L" Then rbSlip.Checked = False
                    Select Case txtTypeSlip.Text
                        Case "IN" : txtTypeSlip.Text = "IN" : lblTypeSlip.Text = "Invoice" : rbLaser.Enabled = True : If DefaultLayout = "L" Then rbLaser.Checked = True Else rbSlip.Checked = True
                        Case "CN" : txtTypeSlip.Text = "CN" : lblTypeSlip.Text = "Credit Note" : rbLaser.Enabled = True : If DefaultLayout = "L" Then rbLaser.Checked = True Else rbSlip.Checked = True
                        Case "CV" : txtTypeSlip.Text = "CV" : lblTypeSlip.Text = "Credit Note Value" : rbLaser.Enabled = True : If DefaultLayout = "L" Then rbLaser.Checked = True Else rbSlip.Checked = True
                        Case "DV" : txtTypeSlip.Text = "DV" : lblTypeSlip.Text = "Debit Note Value" : rbLaser.Enabled = True : If DefaultLayout = "L" Then rbLaser.Checked = True Else rbSlip.Checked = True
                        Case "UR" : txtTypeSlip.Text = "UR" : lblTypeSlip.Text = "Repairs" : rbLaser.Enabled = True : If DefaultLayout = "L" Then rbLaser.Checked = True Else rbSlip.Checked = True 'rbSlip.Checked = True : rbLaser.Enabled = False 'modified by GS on 12/07/16
                        Case "A1" : txtTypeSlip.Text = "A1" : lblTypeSlip.Text = "Adjustment In" : rbLaser.Enabled = True : If DefaultLayout = "L" Then rbLaser.Checked = True Else rbSlip.Checked = True 'rbSlip.Checked = True : rbLaser.Enabled = False 'modified by GS on 12/07/16
                        Case "A2" : txtTypeSlip.Text = "A2" : lblTypeSlip.Text = "Adjustment Out" : rbLaser.Enabled = True : If DefaultLayout = "L" Then rbLaser.Checked = True Else rbSlip.Checked = True 'rbSlip.Checked = True : rbLaser.Enabled = False 'modified by GS on 12/07/16
                        Case "PU" : txtTypeSlip.Text = "PU" : lblTypeSlip.Text = "Purchase of Used Books" : rbLaser.Enabled = True : If DefaultLayout = "L" Then rbLaser.Checked = True Else rbSlip.Checked = True
                        Case "PX" : txtTypeSlip.Text = "PX" : lblTypeSlip.Text = "Reverse Purchase of Used Books" : rbLaser.Enabled = True : If DefaultLayout = "L" Then rbLaser.Checked = True Else rbSlip.Checked = True
                        Case "DO" : txtTypeSlip.Text = "DO" : lblTypeSlip.Text = "Delivery Order" : rbSlip.Enabled = False : rbLaser.Enabled = True : rbLaser.Checked = True : rbSlip.Checked = False
                        Case "CI" : txtTypeSlip.Text = "CI" : lblTypeSlip.Text = "Cash In" : rbSlip.Checked = True : rbLaser.Enabled = False
                        Case "CO" : txtTypeSlip.Text = "CO" : lblTypeSlip.Text = "Cash Out" : rbSlip.Checked = True : rbLaser.Enabled = False
                        Case "TR" : txtTypeSlip.Text = "TR" : lblTypeSlip.Text = "Transposition" : rbLaser.Enabled = True : If DefaultLayout = "L" Then rbLaser.Checked = True Else rbSlip.Checked = True 'rbSlip.Checked = True : rbLaser.Enabled = False 'modified by GS on 12/07/16
                        Case "AS" : txtTypeSlip.Text = "AS" : lblTypeSlip.Text = "Assembly" : rbLaser.Enabled = True : If DefaultLayout = "L" Then rbLaser.Checked = True Else rbSlip.Checked = True 'rbSlip.Checked = True : rbLaser.Enabled = False 'modified by GS on 12/07/16
                        Case "DS" : txtTypeSlip.Text = "DS" : lblTypeSlip.Text = "Disposal" : rbLaser.Enabled = True : If DefaultLayout = "L" Then rbLaser.Checked = True Else rbSlip.Checked = True 'rbSlip.Checked = True : rbLaser.Enabled = False 'modified by GS on 12/07/16
                        Case "RC" : txtTypeSlip.Text = "RC" : lblTypeSlip.Text = "Client Receipt" : rbLaser.Enabled = True : If DefaultLayout = "L" Then rbLaser.Checked = True Else rbSlip.Checked = True
                        Case "RX"

                            If ClocHO Then
                                txtTypeSlip.Text = "RX" : lblTypeSlip.Text = "Pre-Receipt" : rbLaser.Enabled = True : If DefaultLayout = "L" Then rbLaser.Checked = True Else rbSlip.Checked = True
                            Else
                                rbSlip.Checked = True
                                ShowNotification(Me, "Pre-Receipt is available only for Head Office Locations")
                                e.Cancel = True
                                Exit Try
                            End If

                        Case "WR" : txtTypeSlip.Text = "WR" : lblTypeSlip.Text = "Reservation" : rbSlip.Enabled = False : rbLaser.Enabled = True : rbLaser.Checked = True : rbSlip.Checked = False

                        Case "CS" : txtTypeSlip.Text = "CS" : lblTypeSlip.Text = "Consumption Slip" : rbLaser.Enabled = True : If DefaultLayout = "L" Then rbLaser.Checked = True Else rbSlip.Checked = True 'added by GS on 09/05/16
                        Case "PR" : txtTypeSlip.Text = "PR" : lblTypeSlip.Text = "Production" : rbLaser.Enabled = True : If DefaultLayout = "L" Then rbLaser.Checked = True Else rbSlip.Checked = True 'ty on 29/07/2016
                        Case "" : txtTypeSlip.Text = "" : lblTypeSlip.Text = ""
                        Case Else : rbSlip.Checked = True : MessageBox.Show("Invalid Type of Slip", cntTitle, MessageBoxButtons.OK, MessageBoxIcon.Information) : e.Cancel = True : Exit Try
                    End Select

                    '...Jad JE...WF:5033
                    chkReservedInvoiceOnly.Checked = False
                    If ClocHO = True Then
                        chkReservedInvoiceOnly.Visible = txtTypeSlip.Text = "IN"
                    End If
                    '...
                    chkExcelLayout.Checked = False
                    chkExcelLayout.Visible = (txtTypeSlip.Text.Trim = "IN" Or txtTypeSlip.Text.Trim = "CN") And chkExcelLayoutVisible

                    chkAE.Checked = False
                    chkAE.Visible = (txtTypeSlip.Text.Trim = "IN" Or txtTypeSlip.Text.Trim = "CN")

                    chkImportedOffer.Checked = False
                    chkImportedOffer.Visible = (txtTypeSlip.Text.Trim = "IN" Or txtTypeSlip.Text.Trim = "CN")

                    gbCurr.Visible = txtTypeSlip.Text.Trim = "IN" Or txtTypeSlip.Text.Trim = "CN" '_
                    ' Or txtTypeSlip.Text.Trim = "CV" Or txtTypeSlip.Text.Trim = "RC"

                    If txtTypeSlip.Text.Trim = "CV" Or txtTypeSlip.Text.Trim = "DV" Or txtTypeSlip.Text.Trim = "RC" Or txtTypeSlip.Text.Trim = "RX" Then

                        If Copercurr = gbCurr1 Then
                            rbLC.Checked = True
                        ElseIf Copercurr = gbCurr2 Then
                            rbCC.Checked = True
                        Else 'FC
                            rbOriginal.Checked = True
                        End If

                    End If


                    If txtTypeSlip.Text.Trim = "IN" Or txtTypeSlip.Text.Trim = "CN" Or txtTypeSlip.Text.Trim = "DO" Then
                        btnFashionReport.Visible = gbFashion
                        btnFashionReport.Enabled = gbFashion
                        DesignButtons()
                    Else
                        btnFashionReport.Visible = False
                        btnFashionReport.Enabled = False
                        DesignButtons()
                    End If

                    '  chkLink.Visible = txtTypeSlip.Text = "CN"
                    'Check if the Type of Slip has an active record in POS_STOCKTRANSACTION
                    If TypeSlipExists() = False Then
                        MessageBox.Show("No Active Transaction Type Defined.", cntTitle, MessageBoxButtons.OK, MessageBoxIcon.Information)
                        txtTypeSlip.Text = ""
                        lblTypeSlip.Text = ""
                        oldvalue = ""
                        olddesc = ""
                        e.Cancel = True
                        Exit Try
                    End If

                    If Not dsRepSlip Is Nothing Then
                        BuildDetails()
                        lblRecSel.Text = dsRepSlip.Tables("tblRepSlip").Select("Chk = 'True'").Count().ToString & " Records Selected"
                        If txtStartDate.Text = "" Then txtStartDate.Value = CType(WorkingDay, Date).ToString("dd/MM/yyyy")
                        If txtEndDate.Text = "" Then txtEndDate.Value = CType(WorkingDay, Date).ToString("dd/MM/yyyy")

                        'txtStartDate.ValueIsDbNull = True
                        'txtEndDate.ValueIsDbNull = True
                        'txtStartDate.Text = ""
                        'txtEndDate.Text = ""


                        Execute_WOXACT("Delete from " & TempTable)
                            If Not dsRepSlip.Tables(0) Is Nothing Then dsRepSlip.Tables(0).Clear()
                            daRepSlip.Fill(dsRepSlip, dsRepSlip.Tables(0).TableName)
                            LstItems.UpdateData()
                        End If

                        oldvalue = txtTypeSlip.Text
                    olddesc = lblTypeSlip.Text
                Else
                    lblTypeSlip.Text = olddesc
                End If
            Else
                If Not dsRepSlip Is Nothing Then
                    If txtStartDate.Text = "" Then txtStartDate.Value = CType(WorkingDay, Date).ToString("dd/MM/yyyy")
                    If txtEndDate.Text = "" Then txtEndDate.Value = CType(WorkingDay, Date).ToString("dd/MM/yyyy")

                    'txtStartDate.Text = ""
                    'txtEndDate.Text = ""
                    '...Jad-J...WF:5033
                    chkReservedInvoiceOnly.Checked = False : chkReservedInvoiceOnly.Visible = False
                    chkExcelLayout.Checked = False : chkExcelLayout.Visible = False

                    chkImportedOffer.Checked = False : chkImportedOffer.Visible = False
                    chkAE.Checked = False : chkAE.Visible = False

                    rbLaser.Visible = True : rbSlip.Visible = True
                    rbLaser.Enabled = True : rbSlip.Enabled = True
                    ClearGrid()
                End If
            End If
            'txtStartDate.Focus()
        Catch ex As Exception
            MessageBox.Show("Error : " & ex.Message, cntTitle, MessageBoxButtons.OK, MessageBoxIcon.Error)
        End Try
    End Sub

    Private Sub txtEndDate_Enter(ByVal sender As Object, ByVal e As System.EventArgs) Handles txtEndDate.Enter, txtStartDate.Enter
        Try
            FocusStyle(CType(sender, C1DateEdit), "Enter")
        Catch ex As Exception
            MessageBox.Show("Error: " & ex.Message, cntTitle, MessageBoxButtons.OK, MessageBoxIcon.Error)
        End Try
    End Sub

    Private Sub txtEndDate_KeyPress(ByVal sender As Object, ByVal e As System.Windows.Forms.KeyPressEventArgs) Handles txtEndDate.KeyPress, txtStartDate.KeyPress
        Try
            ErrorProvider1.SetError(CType(sender, C1DateEdit), "")
        Catch ex As Exception
            MessageBox.Show("Error: " & ex.Message, cntTitle, MessageBoxButtons.OK, MessageBoxIcon.Error)
        End Try
    End Sub

    Private Sub txtEndDate_Leave(ByVal sender As Object, ByVal e As System.EventArgs) Handles txtEndDate.Leave, txtStartDate.Leave
        Try
            FocusStyle(CType(sender, C1DateEdit), "Leave")
        Catch ex As Exception
            MessageBox.Show("Error: " & ex.Message, cntTitle, MessageBoxButtons.OK, MessageBoxIcon.Error)
        End Try
    End Sub

    Private Sub txtEndDate_TextChanged(ByVal sender As Object, ByVal e As System.EventArgs) Handles txtEndDate.TextChanged, txtStartDate.TextChanged
        Try
            ErrorProvider1.SetError(CType(sender, TextBox), "")
        Catch ex As Exception
            MessageBox.Show("Error : " & ex.Message, cntTitle, _
            MessageBoxButtons.OK, MessageBoxIcon.Error)
        End Try
    End Sub

    Private Sub txtEndDate_Validating(ByVal sender As Object, ByVal e As System.ComponentModel.CancelEventArgs) Handles txtEndDate.Validating, txtStartDate.Validating
        Try
            If (CType(sender, C1DateEdit).Value) Is DBNull.Value Then Exit Sub

            If Not IsDate(CType(sender, C1DateEdit).Value) OrElse CDate(CType(sender, C1DateEdit).Value) < CDate("01/01/1800") Then
                ShowNotification(Me, "Invalid Date", 3000, 12, DevComponents.DotNetBar.eToastPosition.MiddleCenter)
                e.Cancel = True
                Exit Sub
            End If

            If Not txtStartDate.Value Is DBNull.Value And Not txtEndDate.Value Is DBNull.Value Then
                If CDate(txtStartDate.Value) > CDate(txtEndDate.Value) Then
                    ShowNotification(Me, "Starting Date should be Less then End Date", 3000, 12, DevComponents.DotNetBar.eToastPosition.MiddleCenter)

                    If Not ((sender Is txtStartDate And ActiveControl Is txtEndDate) OrElse (sender Is txtEndDate And ActiveControl Is txtStartDate)) Then
                        e.Cancel = True
                    End If

                End If
            End If

        Catch ex As Exception
            'MessageBox.Show("Invalid Date Format.", cntTitle, _
            'MessageBoxButtons.OK, MessageBoxIcon.Error)
        End Try
    End Sub

    Private Sub frmReprintSlips_FormClosing(ByVal sender As Object, ByVal e As System.Windows.Forms.FormClosingEventArgs) Handles Me.FormClosing
        Try

            If Not TempTable Is Nothing Then DropSqlObject(TempTable, "TABLE")

            UpdateStepsLogging("MnuSubRepSlips")
            CType(Me.ParentForm, frmMainMenus).MnuSubRepSlips.Checked = False
            frmSubReprintSlips = Nothing
        Catch ex As Exception
            MessageBox.Show("Error: " & ex.Message, cntTitle, MessageBoxButtons.OK, MessageBoxIcon.Error)
        End Try
    End Sub

    Private Sub frmReprintSlips_KeyDown(ByVal sender As Object, ByVal e As System.Windows.Forms.KeyEventArgs) Handles Me.KeyDown
        Try
            If e.Control = True AndAlso e.Shift = True Then
                'To Ignore Alignment
                e.Handled = True
            Else
                Select Case e.KeyCode
                    Case Keys.F3 : If btnProceed.Focus Then btnProceed_Click(btnProceed, New System.EventArgs)
                    Case Keys.F6 : If btnShow.Focus Then btnShow_Click(btnShow, New System.EventArgs)
                    Case Keys.F4 : If btnCheck.Focus Then btnCheck_Click(btnCheck, New System.EventArgs)
                    Case Keys.F5 : If btnUncheck.Focus Then btnUncheck_Click(btnUncheck, New System.EventArgs)
                    Case Keys.F7 : If btnFashionReport.Focus Then btnFashionReport_Click(btnFashionReport, New System.EventArgs)
                    Case Keys.F10 : If BtnDirectPrint.Focus Then btnPreview_Click(BtnDirectPrint, New System.EventArgs)
                    Case Keys.F11 : If btnPreview.Focus Then btnPreview_Click(btnPreview, New System.EventArgs)
                    Case Keys.F12
                        If btnClose.Focus = True Then
                            Me.Close() : e.Handled = True
                        End If
                        'Close GrpBefPrint on Escape in case opened above Preview button
                    Case Keys.Escape
                        If GrpBefPrint.Visible = True Then GrpBefPrint.Visible = False
                        If GbRc.Visible Then GbRc.Visible = False
                    Case Keys.Enter : If GrpBefPrint.Visible = True AndAlso btnOk.Focus Then btnOk_Click(btnOk, New System.EventArgs)
                End Select
            End If
        Catch exp As Exception
            MessageBox.Show("Error: " & exp.Message, cntTitle, _
            MessageBoxButtons.OK, MessageBoxIcon.Error)
        End Try
    End Sub

    Private Sub frmReprintSlips_Load(ByVal sender As Object, ByVal e As System.EventArgs) Handles Me.Load
        Try
            loading = True
            PrepareHelp(Me, ConnectionString, gbApp, lcSteps)

            CreateContextMenu(Me)

            Me.Text = cntTitle

            GrpBefPrint.Visible = False

            With stBar
                .Panels(0).Text = Date.Today.ToShortDateString
                .Panels(1).Text = ""
                .Panels(2).Text = "Login User: " & gbUser
                .Panels(3).Text = Now.ToLongTimeString
            End With

            LstItems.AllowFilter = True

            If gbUser = "SU" Then
                lcPrivView = True
                lcPrivPrint = True
                AllowViewTransTotal = True
                chkExcelLayoutVisible = True
            Else
                TestPriv(Me, lcSteps)
                lcPrivView = gbPrivView
                lcPrivPrint = gbPrivPrint
                AllowViewTransTotal = NegationPriv(lcSteps, "btnAllowToViewTransTotal") '...Jad
                chkExcelLayoutVisible = NegationPriv(lcSteps, "BtnLayoutExportNegation")
            End If
            txtStartDate.Value = CType(WorkingDay, Date).ToString("dd/MM/yyyy")
            txtEndDate.Value = CType(WorkingDay, Date).ToString("dd/MM/yyyy")
            PnlStartEndDate.Visible = Not GbDenyPrintPOS
            EnabledControls(Me, False)
            EnabledButtons(Me, False, btnClose)
            'ch  @ 05/03/2016 wf : 3446 
            If DefaultLayout = "L" Then
                rbLaser.Checked = True
                rbSlip.Checked = False
            End If
            btnProceed.Enabled = lcPrivView
            LstItems.Enabled = False

            rbLC.Text = gbCurr1 'added by GS on 10/05/16
            rbCC.Text = gbCurr2 'added by GS on 10/05/16

            If Copercurr = gbCurr1 Then
                rbLC.Checked = True
            ElseIf Copercurr = gbCurr2 Then
                rbCC.Checked = True
            ElseIf Copercurr = "FC" Then
                rbOriginal.Visible = True
                rbOriginal.Checked = True
            End If
            TempTable = "TempReprintSlips_" & gbUser
            TempTableDO = "TempDOFromReprintSlips_" & gbUser
            If btnProceed.Enabled = True Then
                btnProceed_Click(btnProceed, New System.EventArgs)
                'Me.ActiveControl = txtTypeSlip
                txtTypeSlip_Validating(txtTypeSlip, New System.ComponentModel.CancelEventArgs)
                pnlAutoShow_Validating(pnlAutoShow, New System.ComponentModel.CancelEventArgs)
            End If

            BtnDirectPrint.Enabled = False
        Catch exp As Exception
            MessageBox.Show("Error: " & exp.Message, cntTitle, MessageBoxButtons.OK, MessageBoxIcon.Error)
        Finally
            loading = False
        End Try
    End Sub

    Private Sub btnProceed_Click(ByVal sender As System.Object, ByVal e As System.EventArgs) Handles btnProceed.Click
        Try

            EnabledControls(Me, True)
            EnabledButtons(Me, True, btnProceed, btnPreview, btnCheck, btnUncheck)
            txtTypeSlip.Text = "IN" 'ch @ 06/09/2018 mail(RE: ERP_POS Operations_Reprint Documents: Enhancements)
            lblRecSel.Text = "0 Records Selected"
            btnPreview.Enabled = lcPrivView
            BtnDirectPrint.Enabled = lcPrivView And Not chkExcelLayout.Checked
            btnCheck.Enabled = True : btnUncheck.Enabled = True
            LstItems.Enabled = True
            BuildDetails()
            UpdateGrid(False)

        Catch ex As Exception
            MessageBox.Show("Error: " & ex.Message, cntTitle, MessageBoxButtons.OK, MessageBoxIcon.Error)
        End Try
    End Sub

    Private Sub btnShow_Click(ByVal sender As System.Object, ByVal e As System.EventArgs) Handles btnShow.Click
        Try
            If txtTypeSlip.Text.Trim = "" Then
                ErrorProvider1.SetError(txtTypeSlip, "Type of Document can't be empty.")
                MessageBox.Show("Missing Data", cntTitle, MessageBoxButtons.OK, MessageBoxIcon.Information)
                txtTypeSlip.Focus()
                Exit Try
            End If

            If txtStartDate.Text = "" Then
                ErrorProvider1.SetError(txtStartDate, "Start Date can't be empty.")
                MessageBox.Show("Missing Data", cntTitle, MessageBoxButtons.OK, MessageBoxIcon.Information)
                txtStartDate.Focus()
                Exit Try
            End If

            If txtEndDate.Text = "" Then
                ErrorProvider1.SetError(txtEndDate, "End Date can't be empty.")
                MessageBox.Show("Missing Data", cntTitle, MessageBoxButtons.OK, MessageBoxIcon.Information)
                txtEndDate.Focus()
                Exit Try
            End If

            lblRecSel.Text = "0 Records Selected"
            btnPreview.Enabled = lcPrivView
            BtnDirectPrint.Enabled = lcPrivView And Not chkExcelLayout.Checked
            btnCheck.Enabled = True : btnUncheck.Enabled = True
            LstItems.Enabled = True
            BuildDetails()

            UpdateGrid(Not loading)

            'Me.ActiveControl = LstItems

            'oldvalue = ""
        Catch ex As Exception
            MessageBox.Show("Error: " & ex.Message, cntTitle, MessageBoxButtons.OK, MessageBoxIcon.Error)
        End Try
    End Sub

    Private Sub btnPreview_Click(ByVal sender As System.Object, ByVal e As System.EventArgs) Handles btnPreview.Click, BtnDirectPrint.Click
        Dim WSerno() As Long
        '   Dim frm As FrmPrintSO
        Dim dr As DataRow
        Dim PrintingCurr As String = "" '...PrintRC
        Dim rptCDV As String
        Try
            If BtnDirectPrint Is sender Then BtnDirectPrint.Enabled = False

            If BtnDirectPrint Is sender AndAlso rbLaser.Checked And (txtTypeSlip.Text.Trim = "IN" Or txtTypeSlip.Text.Trim = "CN") And Not grpOptionFund.Visible Then
                If gbFundDisbursementModuleIsActive Then
                    grpOptionFund.Visible = True
                    Exit Try
                End If
            End If

            grpOptionFund.Visible = False

            If Not dsRepSlip.GetChanges Is Nothing Then daRepSlip.Update(dsRepSlip, "tblRepSlip")

            If (txtTypeSlip.Text.Trim = "IN" Or txtTypeSlip.Text.Trim = "CN") And chkExcelLayout.Checked = True Then
                If LstItems.Splits(0).DisplayColumns.Item("InvHead").DataColumn.Text = "" Then
                    MessageBox.Show("No Records Selected.", cntTitle, MessageBoxButtons.OK, MessageBoxIcon.Information)
                    Exit Try
                End If
            Else
                If dsRepSlip.Tables(0).Select("Chk = 'true'").Length = 0 Then
                    MessageBox.Show("No Records Selected.", cntTitle, MessageBoxButtons.OK, MessageBoxIcon.Information)
                    Exit Try
                End If
            End If

            Select Case txtTypeSlip.Text.Trim
                Case "CV", "DV", "RC" ', "RX"
                    If sender Is BtnDirectPrint Then
                        chkShowCompName.Checked = True
                    Else
                        If rbLaser.Checked AndAlso GbRc.Visible = False Then
                            chkShowCompName.Checked = True
                            GbRc.Visible = True
                            chkShowCompName.Focus()
                            Exit Try
                        Else
                            GbRc.Visible = False
                        End If
                    End If

            End Select

            Select Case txtTypeSlip.Text.Trim
                Case "CV", "DV"
                    rptCDV = ExecuteScalar("SELECT SL_RptCDVReceipt  FROM pos_splocation WHERE SL_Loc = '" & Cloc & "' ")
                    If rptCDV = "" Then rptCDV = "rptNormClReceipt.rpt"
            End Select

            '...Jad JE - Invoicing in FC Currency
            If rbLC.Checked = True Then
                PrintingCurr = gbCurr1
            ElseIf rbCC.Checked = True Then
                PrintingCurr = gbCurr2
            Else
                PrintingCurr = "FC"
            End If

            Select Case txtTypeSlip.Text.Trim

                Case "IN", "CN"
                    If sender Is BtnDirectPrint Then
                        Dim tocurr As String = ""
                        If rbLC.Checked Then
                            If Copercurr <> gbCurr1 Then
                                tocurr = gbCurr1
                            End If
                        ElseIf rbCC.Checked Then
                            If Copercurr <> gbCurr2 Then
                                tocurr = gbCurr2
                            End If
                        Else
                            tocurr = "FC"
                        End If

                        PrintSalesInvoiceSlipLaser(rbSlip.Checked, Nothing, CStr(IIf(txtTypeSlip.Text.Trim = "IN" OrElse txtTypeSlip.Text.Trim = "PX", "S", "C")), Nothing, chkPrintClient.Checked, chkPrintTech.Checked, Me, TempTable, True, chkSupSubTot.Checked, CBool(IIf(RecordsExists(" SELECT TOP 1 1 FROM pos_splocation WHERE SL_BundleComm = 'N' AND SL_Loc = '" & Cloc & "' "), False, True)), tocurr, , , True, , , , CBool(IIf(rbSlip.Checked, False, SysParam_PrintCLBalance)), , , , "Def", , , , , CBool(IIf(sender Is BtnDirectPrint, False, True)),,
                        chkFundDisbursement.Checked, FundsQuery(TempTable, chkFundDisbursement.Checked), chkFundDisbursementPayment.Checked)
                    Else
                        Using frm As New FrmPrintSO
                            With frm
                                .From = Me
                                .rbSlip = rbSlip.Checked
                                .TempTable = TempTable

                                If chkExcelLayout.Checked Then
                                    .ExcelLayoutOnly = True
                                    .WSerno = CDbl(LstItems.Splits(0).DisplayColumns.Item("InvHead").DataColumn.Text)
                                Else
                                    .ExcelLayoutOnly = False
                                    .btnNormalExl.Visible = False
                                    .btnNormalExl.Enabled = False
                                End If

                                .TypeSlip = txtTypeSlip.Text
                                Dim tocurr As String = ""
                                If rbLC.Checked Then
                                    If Copercurr <> gbCurr1 Then
                                        tocurr = gbCurr1
                                    End If
                                ElseIf rbCC.Checked Then
                                    If Copercurr <> gbCurr2 Then
                                        tocurr = gbCurr2
                                    End If
                                Else
                                    tocurr = "FC"
                                End If
                                .TOCURR = tocurr
                                .ShowDialog()
                            End With
                        End Using
                    End If



                Case "PU", "PX"
                    If sender Is BtnDirectPrint Then
                        Dim tocurr As String = ""
                        If rbLC.Checked Then
                            If Copercurr <> gbCurr1 Then
                                tocurr = gbCurr1
                            End If
                        ElseIf rbCC.Checked Then
                            If Copercurr <> gbCurr2 Then
                                tocurr = gbCurr2
                            End If
                        Else
                            tocurr = "FC"
                        End If
                        PrintSalesInvoiceSlipLaser(rbSlip.Checked, Nothing, CStr(IIf(txtTypeSlip.Text.Trim = "IN" OrElse txtTypeSlip.Text.Trim = "PX", "S", "C")), Nothing, chkPrintClient.Checked, chkPrintTech.Checked, Me, TempTable, True, chkSupSubTot.Checked, chkPrintBund.Checked, tocurr, , , chkLink.Checked, , , , , , , , "Def", )
                    Else
                        GrpBefPrint.Visible = True
                        GrpBefPrint.BringToFront()
                        chkPrintBund.Visible = False
                        chkPrintClient.Visible = True : chkPrintSupRef.Checked = False
                        chkPrintTech.Visible = False
                        chkSupSubTot.Visible = False
                        chkPrintSupRef.Visible = False
                        GrpBefPrint.Height = 65
                        GrpBefPrint.Top = btnPreview.Top - 70
                        btnOk.Top = chkPrintClient.Top + 20
                        btnOk.Focus()
                    End If
                Case "A1", "A2"
                    'Show GrpBefPrint, hide unwanted checkboxes, resize frame
                    If sender Is BtnDirectPrint Then
                        PrintAdjust(Cloc, Nothing, True, txtTypeSlip.Text.Trim, chkPrintSupRef.Checked, "", Me, TempTable, rbLaser.Checked, , True) 'edited by GS on 12/07/16 (add rbLaser.Checked)
                    Else
                        GrpBefPrint.Visible = True
                        GrpBefPrint.BringToFront()
                        chkPrintBund.Visible = False
                        chkPrintClient.Visible = False
                        chkPrintTech.Visible = False
                        chkSupSubTot.Visible = False
                        chkPrintSupRef.Visible = True : chkPrintSupRef.Checked = False
                        GrpBefPrint.Height = 65
                        GrpBefPrint.Top = btnPreview.Top - 70
                        btnOk.Top = chkPrintSupRef.Top + 20
                        btnOk.Focus()
                    End If
                Case "UR"
                    'Send TempTable as parameter
                    printRepairSlip(Cloc, Nothing, True, True, Nothing, 1, Me, TempTable, rbLaser.Checked, , CBool(IIf(sender Is BtnDirectPrint, True, False))) 'edited by GS on 12/07/16 (add rbLaser.Checked)
                Case "WR"
                    'Send TempTable as parameter
                    PrintReservationTransaction(Me, , TempTable)
                Case "DO"

                    'Send TempTable as parameter
                    DropSqlObject(TempTableDO, "TABLE")

                    strsql = "  CREATE TABLE [dbo]." & TempTableDO & " ("
                    strsql &= " [pk] [numeric](18,0) not NULL  "
                    strsql &= " CONSTRAINT PK_" & TempTableDO & " PRIMARY KEY  (pk) "
                    strsql &= " ) ON [PRIMARY]"

                    Execute_WOXACT(strsql)

                    Execute_WOXACT(" iNSERT iNTO " & TempTableDO & " SELECT  DOH_RecId fROM " & TempTable & " WHERE chk = 'True' ")
                    If sender Is BtnDirectPrint Then
                        Using frms As New frmPrintOffer
                            frms.PrintRep(TempTableDO, False, "Delivery", True)
                        End Using
                    Else
                        PrintDeliveryOrder(Me, , TempTableDO)
                    End If
                Case "CI", "CO"
                    'Send TempTable & RecIDs in array as parameters
                    i = 0
                    For Each dr In dsRepSlip.Tables("tblRepSlip").Select("chk = 'True'")
                        i = i + 1
                        ReDim Preserve WSerno(i)
                        WSerno(i) = CLng(dr("InvHead").ToString)
                    Next dr


                    PrintCashPOS(WSerno, txtTypeSlip.Text.Trim, ConvTime(False), Me, TempTable, , , CBool(IIf(sender Is BtnDirectPrint, True, False)))

                Case "TR", "AS"
                    'Send TempTable & user as parameters
                    PrintSlipTransAss(Nothing, CStr(IIf(txtTypeSlip.Text.Trim = "TR", "Transposition", "Assembly")), "", gbUser, Me, TempTable, rbLaser.Checked, , CBool(IIf(txtTypeSlip.Text.Trim = "TR", NegationPriv("MnuSubTransposition", "BtnShowCostNegation"), NegationPriv("MnuSubAssembly", "BtnShowCostNegation"))), CBool(IIf(sender Is BtnDirectPrint, True, False))) 'edited by GS on 12/07/16 (add rbLaser.Checked)
                Case "PR"
                    'Send TempTable & user as parameters
                    PrintSlipTransAss(Nothing, "Production", "", gbUser, Me, TempTable, rbLaser.Checked, , NegationPriv("MnuProduction", "btnalowcost"), CBool(IIf(sender Is BtnDirectPrint, True, False))) 'edited by GS on 12/07/16 (add rbLaser.Checked)

                Case "DS"
                    'Send TempTable as parameter
                    PrintDisposal(Cloc, Nothing, True, "", True, "", "", Me, TempTable, rbLaser.Checked, CBool(IIf(sender Is BtnDirectPrint, True, False))) 'edited by GS on 12/07/16 (add rbLaser.Checked)



                Case "CV"
                    PrintRC(Me, Cloc, CLocName, Nothing, 1, False, False, rbSlip.Checked, rptCDV, False, "Credit Notes Values :", TempTable, , PrintingCurr, , chkShowCompName.Checked, CBool(IIf(sender Is BtnDirectPrint, True, False)))
                Case "DV"
                    PrintRC(Me, Cloc, CLocName, Nothing, 1, False, False, rbSlip.Checked, rptCDV, False, "Debit Notes Values :", TempTable, , PrintingCurr, , chkShowCompName.Checked, CBool(IIf(sender Is BtnDirectPrint, True, False)))
                Case "RC"
                    PrintRC(Me, Cloc, CLocName, Nothing, 1, False, False, rbSlip.Checked, "", False, "Client Receipt : ", TempTable, , PrintingCurr, , chkShowCompName.Checked, CBool(IIf(sender Is BtnDirectPrint, True, False)))
                Case "RX"
                    PrintRC(Me, Cloc, CLocName, Nothing, 1, False, False, rbSlip.Checked, "", False, "Pre-Receipt : ", TempTable, , PrintingCurr, , chkShowCompName.Checked, CBool(IIf(sender Is BtnDirectPrint, True, False)), True)
                    'added by GS on 09/05/16
                Case "CS"
                    PrintConsump(CBool(IIf(sender Is BtnDirectPrint, True, False)))
                    'GS on 09/05/16
            End Select

        Catch ex As Exception
            MessageBox.Show("Error: " & ex.Message, cntTitle, MessageBoxButtons.OK, MessageBoxIcon.Error)
        Finally
            If sender Is BtnDirectPrint Then BtnDirectPrint.Enabled = True
        End Try
    End Sub

    'added by GS on 09/05/16
    Private Sub PrintConsump(Optional printdirect As Boolean = False) 'ByVal ConsumpID As Integer)
        Dim strsql, strsqlView As String

        Dim drSql As SqlDataReader

        Try
            Cursor.Current = Cursors.WaitCursor


            strsql = " SELECT"

            strsql = strsql + " POS_CONSUMPTIONH.CONSH_RECID, CASE WHEN POS_CONSUMPTIONH.CONSH_STATUS = 'N' THEN 'Normal' ELSE 'Reverse' END CONSH_STATUS, POS_CONSUMPTIONH.CONSH_DATE, POS_CONSUMPTIONH.CONSH_CSHREG, POS_CONSUMPTIONH.CONSH_REMARKS,"

            strsql &= "CASE WHEN POS_CONSUMPTIOND.CONSD_TYPE = 'I' THEN 'Internal Use' WHEN POS_CONSUMPTIOND.CONSD_TYPE = 'P' THEN 'Production Use' ELSE 'Consumables & Services' END CONSD_TYPE,"
            strsql = strsql + " POS_CONSUMPTIOND.CONSD_QTY, POS_CONSUMPTIOND.CONSD_CURR, POS_CONSUMPTIOND.CONSD_COSTLC, POS_CONSUMPTIOND.CONSD_CSTCC, POS_CONSUMPTIOND.CONSD_USRKS,"
            strsql &= "POS_CONSUMPTIOND.CONSD_BARCODE,"

            If CRetVal = "1" Then
                strsql &= " isnull((SELECT TOP 1 case when ltrim(dbar_barcode) = '' then null else dbar_barcode end dbar_barcode FROM dbo.STK04_DetBarcode WHERE dbar_stkid = Stk_Recid"
                strsql &= " AND ISNULL(dbar_expdate,'') = ISNULL(CONSD_ExpiryDt,'')"
                strsql &= " AND ISNULL(dbar_batch,'')=ISNULL(CONSD_BatchInfo,'')),STK_REF) "
            ElseIf CRetVal = "2" Then
                strsql &= " STK_REF "
            ElseIf CRetVal = "3" Then
                strsql &= " ISNULL(STK_SingleRef1,STK_REF) "
            ElseIf CRetVal = "4" Then
                strsql &= " ISNULL(STK_SingleRef2,STK_REF) "
            End If

            strsql &= " STK_REF, POS_CONSUMPTIOND.CONSD_MACHINE, POS_CONSUMPTIOND.CONSD_MACHPOOL,"
            strsql = strsql + " POS_STKFIL.STK_SUPPLIER, POS_STKFIL.STK_CATEGORY, POS_STKFIL.STK_SERIE, POS_STKFIL.STK_DESC, POS_STKFIL.STK_FCCODE,"

            strsql &= " STK04_MACHINE.MACH_CODE1, STK04_MACHINE.MACH_CODE2,"
            strsql = strsql + " STK04_MACHINE.MACH_DESC,"
            strsql &= " STK04_MachinePool.MACHP_CODE,"
            strsql = strsql + " STK04_MachinePool.MACHP_DESC,"
            strsql = strsql + " STK04_Location.Loc_Desc"
            'CH @ CHRISTINA 17/06/2016 WF:3844
            strsql &= " 		,PjD_DivCode,PjD_Description ,PjS_SubDivCode ,PjS_Description "
            'ch @ 16/06/2017 wf: 5509
            strsql &= " 	,CASE WHEN  SUBSTRING(POS_CONSUMPTIOND.CONSD_ExpiryDt,1,2) = '00' THEN SUBSTRING(POS_CONSUMPTIOND.CONSD_ExpiryDt,4,8)  ELSE POS_CONSUMPTIOND.CONSD_ExpiryDt end  CONSD_ExpiryDt "
            strsql &= " ,    POS_CONSUMPTIOND.CONSD_BatchInfo  "
            If rbLaser.Checked Then strsql &= " , Meas_desc  "
            strsql = strsql + " From"
            'strsql = strsql + " dbo.POS_CONSUMPTIOND POS_CONSUMPTIOND,"
            'strsql = strsql + " dbo.POS_STKFIL POS_STKFIL,"
            'strsql = strsql + " dbo.POS_CONSUMPTIONH POS_CONSUMPTIONH,"
            'strsql = strsql + " dbo.STK04_MACHINE STK04_MACHINE,"
            'strsql = strsql + " dbo.STK04_MachinePool STK04_MachinePool,"
            'strsql = strsql + " dbo.STK04_Location STK04_Location"
            'strsql = strsql + " Where"
            'strsql = strsql + " POS_CONSUMPTIOND.CONSD_STKID = POS_STKFIL.STK_RECID AND"
            'strsql = strsql + " POS_CONSUMPTIOND.CONSD_HEADID = POS_CONSUMPTIONH.CONSH_RECID AND"
            'strsql = strsql + " POS_CONSUMPTIOND.CONSD_MACHINE *= STK04_MACHINE.MACH_RECID AND"
            'strsql = strsql + " POS_CONSUMPTIOND.CONSD_MACHPOOL *= STK04_MachinePool.MACHP_RECID AND"
            'strsql = strsql + " POS_CONSUMPTIONH.CONSH_LOCATION = STK04_Location.Loc_Code"
            'strsql = strsql + " and consh_recid = " & ConsumpID & ""
            strsql = strsql + " dbo.POS_CONSUMPTIOND POS_CONSUMPTIOND INNER JOIN " & CStr(IIf(ClocHO = False, "POS_STKFIL", "STK04_STKFIL")) & " POS_STKFIL "
            strsql = strsql + " ON POS_CONSUMPTIOND.CONSD_STKID = POS_STKFIL.STK_RECID "
            If rbLaser.Checked Then strsql &= " left join STK04_Measurements on stk_unit = Meas_Unit  "
            strsql = strsql + " INNER JOIN dbo.POS_CONSUMPTIONH POS_CONSUMPTIONH "
            strsql = strsql + " ON POS_CONSUMPTIOND.CONSD_HEADID = POS_CONSUMPTIONH.CONSH_RECID and CONSH_LOCATION = CONSD_LOC "
            strsql = strsql + " LEFT JOIN dbo.STK04_MACHINE STK04_MACHINE "
            strsql = strsql + " ON POS_CONSUMPTIOND.CONSD_MACHINE = STK04_MACHINE.MACH_RECID "
            strsql = strsql + " LEFT JOIN dbo.STK04_MachinePool STK04_MachinePool "
            strsql = strsql + " ON POS_CONSUMPTIOND.CONSD_MACHPOOL = STK04_MachinePool.MACHP_RECID "
            strsql = strsql + " INNER JOIN dbo.STK04_Location STK04_Location "
            strsql = strsql + " ON POS_CONSUMPTIONH.CONSH_LOCATION = STK04_Location.Loc_Code "

            'strsql = strsql + " where consh_recid = " & ConsumpID & ""
            strsql = strsql + " INNER JOIN " & TempTable & " ON consh_recid = [Cons. ID] and Chk = 'True'"
            'CH @ 17/06/2016
            strsql &= "		LEFT JOIN   tbl_Cst_ProjectDiv ON tbl_Cst_ProjectDiv.PjD_Recid = POS_CONSUMPTIOND.CONSD_DivRecid"
            strsql &= "		LEFT JOIN  tbl_Cst_ProjectSubDiv ON tbl_Cst_ProjectSubDiv.PjS_DivRecid = tbl_Cst_ProjectDiv.PjD_Recid "
            strsql &= " AND tbl_Cst_ProjectSubDiv.PjS_Recid = POS_CONSUMPTIOND.CONSD_SubDivRecid  "
            strsql &= " where CONSH_LOCATION ='" & Cloc & "' "
            ' sqlstat &= " And isnull(SP_PriceType,'" & PPriceU & "')='" & PPriceU & "'" 'added by GS on 20/09/13


            strsqlView = " SELECT POS_CONSUMPTIONH.CONSH_RECID ,"
            strsqlView &= " CASE WHEN POS_CONSUMPTIONH.CONSH_STATUS = 'N' THEN 'Normal'"
            strsqlView &= " ELSE 'Reverse'"
            strsqlView &= " END CONSH_STATUS ,"
            strsqlView &= " POS_CONSUMPTIONH.CONSH_DATE ,"
            strsqlView &= " POS_CONSUMPTIONH.CONSH_REMARKS ,"
            strsqlView &= " CASE WHEN POS_CONSUMPTIOND.CONSD_TYPE = 'I' THEN 'Internal Use'"
            strsqlView &= " WHEN POS_CONSUMPTIOND.CONSD_TYPE = 'P' THEN 'Production Use'"
            strsqlView &= " ELSE 'Consumables & Services'"
            strsqlView &= " END CONSD_TYPE,"
            strsqlView &= " SUM(CONSD_COSTLC)  [TLC] ,SUM(CONSD_CSTCC) [TCC]"
            strsqlView = strsqlView + " From"
            strsqlView = strsqlView + " dbo.POS_CONSUMPTIOND POS_CONSUMPTIOND INNER JOIN " & CStr(IIf(ClocHO = False, "POS_STKFIL", "STK04_STKFIL")) & " POS_STKFIL "
            strsqlView = strsqlView + " ON POS_CONSUMPTIOND.CONSD_STKID = POS_STKFIL.STK_RECID "
            strsqlView = strsqlView + " INNER JOIN dbo.POS_CONSUMPTIONH POS_CONSUMPTIONH "
            strsqlView = strsqlView + " ON POS_CONSUMPTIOND.CONSD_HEADID = POS_CONSUMPTIONH.CONSH_RECID and CONSH_LOCATION = CONSD_LOC "
            strsqlView = strsqlView + " LEFT JOIN dbo.STK04_MACHINE STK04_MACHINE "
            strsqlView = strsqlView + " ON POS_CONSUMPTIOND.CONSD_MACHINE = STK04_MACHINE.MACH_RECID "
            strsqlView = strsqlView + " LEFT JOIN dbo.STK04_MachinePool STK04_MachinePool "
            strsqlView = strsqlView + " ON POS_CONSUMPTIOND.CONSD_MACHPOOL = STK04_MachinePool.MACHP_RECID "
            strsqlView = strsqlView + " INNER JOIN dbo.STK04_Location STK04_Location "
            strsqlView = strsqlView + " ON POS_CONSUMPTIONH.CONSH_LOCATION = STK04_Location.Loc_Code "
            strsqlView = strsqlView + " INNER JOIN " & TempTable & " ON consh_recid = [Cons. ID] and Chk = 'True'"
            strsqlView &= "		LEFT JOIN   tbl_Cst_ProjectDiv ON tbl_Cst_ProjectDiv.PjD_Recid = POS_CONSUMPTIOND.CONSD_DivRecid"
            strsqlView &= "		LEFT JOIN  tbl_Cst_ProjectSubDiv ON tbl_Cst_ProjectSubDiv.PjS_DivRecid = tbl_Cst_ProjectDiv.PjD_Recid "
            strsqlView &= " AND tbl_Cst_ProjectSubDiv.PjS_Recid = POS_CONSUMPTIOND.CONSD_SubDivRecid  "
            strsqlView &= " where CONSH_LOCATION ='" & Cloc & "' "
            strsqlView &= "   GROUP BY CONSH_RECID,CONSH_STATUS,CONSH_STATUS,CONSH_DATE,CONSH_REMARKS,CONSD_TYPE"


            Dim rptParam(9, 1) As Object

            rptParam(1, 0) = "Ccurr1"
            rptParam(1, 1) = "'" & gbCurr1 & "'"

            rptParam(2, 0) = "Ccurr2"
            rptParam(2, 1) = "'" & gbCurr2 & "'"

            rptParam(3, 0) = "location"
            rptParam(3, 1) = "'" & Cloc & "'"

            'added by GS on 19/08/13
            rptParam(4, 0) = "decno"
            rptParam(4, 1) = gbDecNo


            rptParam(5, 0) = "Opcurr"
            rptParam(5, 1) = "'" & Copercurr & "'"

            rptParam(6, 0) = "FoReference" : rptParam(6, 1) = "'" & CStr(IIf(CRetVal = "1", "Barcode", "Reference")) & "'"

            rptParam(7, 0) = "Pround"
            rptParam(7, 1) = "" & VatLCRound & ""

            rptParam(8, 0) = "FoPrintCost"
            rptParam(8, 1) = "'" & CStr(IIf(NegationPriv("MnuConsumpSlip", "btnPrintCost"), "Y", "N")) & "'"

            'GS on 19/08/13

            ''added by GS on 01/10/13
            'rptParam(6, 0) = "TechContent"
            'rptParam(6, 1) = "'F'"

            'rptParam(7, 0) = "SupRef"
            'rptParam(7, 1) = "'F'"

            'rptParam(8, 0) = "BreakBySupRef"
            'rptParam(8, 1) = "'F'"
            ''GS on 01/10/13

            ''added by GS on 04/11/13
            'rptParam(9, 0) = "Info"
            'rptParam(9, 1) = "'Consump #'"
            ''GS on 04/11/13

            'PrintPODirectRep(Me, strsql, "rptConsumSlip.rpt", 1, rptParam, " ORDER BY consh_recid DESC ")
            ' PrintPODirectRep(Me, strsql, "rptTransferRep.rpt")

            If printdirect Then
                PrintPODirectRep(Me, strsql, IIf(rbLaser.Checked, "rptConsumLaser.rpt", "rptConsumSlip.rpt").ToString, 1, rptParam, " ORDER BY consh_recid DESC ")
            Else
                ReadFromDBToDataReader(strsql, drSql)
                Using frm As New frmPreviewGrid
                    With frm
                        frm.BuildSql(strsql)
                        frm.BuildSql(strsqlView, , , , , strsql)
                        .ColDesign("CONSH_RECID", -1, "Cons Slip", C1.Win.C1TrueDBGrid.AlignHorzEnum.Center, , , , , , , , frmPreviewGrid.Orders.Ascending, , , , , , , , , , )
                        .ColDesign("CONSD_TYPE", 0, "Type", C1.Win.C1TrueDBGrid.AlignHorzEnum.Center, , False, , , , , False, )
                        .ColDesign("TLC", -1, "Total " & gbCurr1, C1.Win.C1TrueDBGrid.AlignHorzEnum.Far, , AllowViewTransTotal, , , , , AllowViewTransTotal, , , , , VatLCFormat)
                        .ColDesign("TCC", -1, "Total " & gbCurr2, C1.Win.C1TrueDBGrid.AlignHorzEnum.Far, , AllowViewTransTotal, , , , , AllowViewTransTotal, , , , , "###,###,##0.###")
                        '  .ColDesign("CONSD_QTY", -1, "Qty", C1.Win.C1TrueDBGrid.AlignHorzEnum.Far)
                        '.ColDesign("CONSD_CURR", 0, "Curr", C1.Win.C1TrueDBGrid.AlignHorzEnum.Near, , False, , , , , False, , , , , , , , , , , False)
                        '.ColDesign("CONSD_COSTLC", -1, "Cost " & gbCurr1, C1.Win.C1TrueDBGrid.AlignHorzEnum.Far)
                        '.ColDesign("CONSD_CSTCC", -1, "Cost " & gbCurr2, C1.Win.C1TrueDBGrid.AlignHorzEnum.Far)
                        '.ColDesign("CONSD_USRKS", 300, "Info", C1.Win.C1TrueDBGrid.AlignHorzEnum.Near)
                        '.ColDesign("CONSD_BARCODE", -1, "Reference", C1.Win.C1TrueDBGrid.AlignHorzEnum.Near)
                        '.ColDesign("CONSD_MACHINE", 0, "Machine", C1.Win.C1TrueDBGrid.AlignHorzEnum.Near, , False, , , , , False, , , , , , , , , , , False)
                        '.ColDesign("CONSD_MACHPOOL", 0, "Machine Pool", C1.Win.C1TrueDBGrid.AlignHorzEnum.Near, , False, , , , , False, , , , , , , , , , , False)
                        '.ColDesign("STK_SUPPLIER", 0, "SUPPLIER", C1.Win.C1TrueDBGrid.AlignHorzEnum.Near, , False, , , , , False, , , , , , , , , , , False)
                        '.ColDesign("STK_CATEGORY", 0, "CATEGORY", C1.Win.C1TrueDBGrid.AlignHorzEnum.Near, , False, , , , , False, , , , , , , , , , , False)
                        '.ColDesign("STK_SERIE", 0, "Serial", C1.Win.C1TrueDBGrid.AlignHorzEnum.Near, , False, , , , , False, , , , , , , , , , , False)
                        '.ColDesign("STK_DESC", -1, "Description", C1.Win.C1TrueDBGrid.AlignHorzEnum.Near)
                        '.ColDesign("STK_FCCODE", 0, "FCCODE", C1.Win.C1TrueDBGrid.AlignHorzEnum.Near, , False, , , , , False, , , , , , , , , , , False)
                        .ColDesign("CONSH_STATUS", -1, "Status", C1.Win.C1TrueDBGrid.AlignHorzEnum.Near, , , , , , )
                        .ColDesign("CONSH_DATE", -1, "Date", C1.Win.C1TrueDBGrid.AlignHorzEnum.Center, , , , , , , , , , , , "dd/MM/yyyy")
                        '.ColDesign("CONSH_CSHREG", -1, "C.R", C1.Win.C1TrueDBGrid.AlignHorzEnum.Near, , , , , , )
                        .ColDesign("CONSH_REMARKS", 300, "Remarks", C1.Win.C1TrueDBGrid.AlignHorzEnum.Near, , , , , , )
                        '.ColDesign("MACH_CODE1", -1, "Mach Code 1", C1.Win.C1TrueDBGrid.AlignHorzEnum.Center)
                        '.ColDesign("MACH_CODE2", -1, "Mach Code 2", C1.Win.C1TrueDBGrid.AlignHorzEnum.Center)
                        '.ColDesign("MACH_DESC", -1, "Mach Desc", C1.Win.C1TrueDBGrid.AlignHorzEnum.Near)
                        '.ColDesign("MACHP_CODE", -1, "Mach Pool Code", C1.Win.C1TrueDBGrid.AlignHorzEnum.Center)
                        '.ColDesign("MACHP_DESC", -1, "Mach Pool Desc", C1.Win.C1TrueDBGrid.AlignHorzEnum.Near)
                        '.ColDesign("Loc_Desc", -1, "Location Desc", C1.Win.C1TrueDBGrid.AlignHorzEnum.Near)

                        '.ColDesign("Active", 50, "Active", C1.Win.C1TrueDBGrid.AlignHorzEnum.Center, , , , , , , , , , , , , , , , C1.Win.C1TrueDBGrid.PresentationEnum.CheckBox)


                        If Not drSql.HasRows Then
                            MessageBox.Show("No Records Found.", cntTitle, MessageBoxButtons.OK, MessageBoxIcon.Exclamation)
                            Exit Sub
                        End If
                        .ShowDialog(strPrev, Me.Name, IIf(rbLaser.Checked, "rptConsumLaser.rpt", "rptConsumSlip.rpt").ToString, rptParam, , , , , , , , , , , , , , , , , , printdirect)
                    End With
                End Using
            End If


        Catch ex As Exception
            MessageBox.Show("General Error : " & ex.Message, cntTitle, MessageBoxButtons.OK, MessageBoxIcon.Error)
        Finally

            Cursor.Current = Cursors.Default
        End Try
    End Sub

    Private Sub btnClose_Click(ByVal sender As System.Object, ByVal e As System.EventArgs) Handles btnClose.Click
        Try
            If lcChanges = True Then
                If MessageBox.Show("Do You Want To Discard Changes?", cntTitle, MessageBoxButtons.YesNo, MessageBoxIcon.Question) = MsgBoxResult.Yes Then
                    Me.Close()
                Else
                    Exit Sub
                End If
            Else
                Me.Close()
            End If
        Catch exp As Exception
            MessageBox.Show("Error : " & exp.Message, cntTitle,
            MessageBoxButtons.OK, MessageBoxIcon.Error)
        End Try
    End Sub

    Private Sub BuildDetails()

        Dim i As Int32
        Dim strsql As String
        Dim cbSql As SqlCommandBuilder
        Dim cmSql As SqlCommand
        Dim cnSql As SqlConnection
        Dim drSql As SqlDataReader
        'Dim UnitWidth As Double

        Try

            '--------Create Temp Table for Grid-------------

            DropSqlObject(TempTable, "TABLE")

            strsql = "CREATE TABLE [dbo].[" + TempTable + "] ( "

            Select Case txtTypeSlip.Text.Trim
                Case "IN", "CN", "CV", "DV", "PU", "RC", "PX", "RX"
                    'To be replaced with temptables corresponding
                    'InvHead needed for print function but won't be shown in grid
                    strsql &= " [Chk] [varchar](5) NOT NULL ,"
                    strsql &= " [InvHead] [numeric] (18,0) NOT NULL, "
                    strsql &= " [DocType] [varchar](2) NOT NULL ,"
                    strsql &= " [Prefix] [varchar](1) NOT NULL ,"
                    strsql &= " [Serial] [numeric](18,0) NOT NULL ,"
                    strsql &= " [Date] [datetime] NULL ,"
                    strsql &= " [ClientNo] [numeric](18,0) NULL ,"
                    strsql &= " [ClientSubNo] [numeric](2,0) NULL ,"
                    strsql &= " [ClientName] [nvarchar](50) NULL ,"
                    strsql &= " [Address] [nvarchar](300) NULL ,"
                    'ch @07/08/2019 mail(Maliks-ERP-POS Operation-Stock RV# In Reprint Documents)
                    If txtTypeSlip.Text.Trim = "RC" Then strsql &= " [RV#] [varchar](15) NULL ,"
                    strsql &= " [TotalAmount] [decimal](18,3) NULL ,"
                    If Copercurr = "FC" Then
                        strsql &= " [InvCurr] [varchar](3) NOT NULL ,"
                    End If
                    strsql &= " [INVH_OriginalCURR] [varchar](3) NOT NULL ,"
                    If txtTypeSlip.Text.Trim = "IN" Or txtTypeSlip.Text.Trim = "CN" OrElse txtTypeSlip.Text.Trim = "PU" OrElse txtTypeSlip.Text.Trim = "PX" Then
                        strsql &= "  [BrBa] [Varchar](200) NULL ,"
                    End If

                    If txtTypeSlip.Text.Trim = "IN" Or txtTypeSlip.Text.Trim = "CN" Then
                        If chkImportedOffer.Checked Then strsql &= "  [ImpFromOffer] [nvarchar](max) NULL ,"
                        If gbISOSModuleIsActive Then strsql &= " [ISOS#] [nvarchar](100) NULL ,"
                        If chkAE.Checked Then strsql &= "  [EMP] [varchar](5) ,  [Ename] [varchar](30) , "
                    End If

                    strsql &= " [CashReg] varchar(2) , "
                    strsql &= " [UserStamp] varchar(4) , "
                    strsql &= " [Remarks] [nvarchar](3000) NULL "
                    'added by GS on 13/05/16
                    If txtTypeSlip.Text.Trim = "IN" Or txtTypeSlip.Text.Trim = "CN" _
                         Or txtTypeSlip.Text.Trim = "CV" Or txtTypeSlip.Text.Trim = "DV" Or txtTypeSlip.Text.Trim = "RC" Or txtTypeSlip.Text.Trim = "RX" OrElse txtTypeSlip.Text.Trim = "PU" OrElse txtTypeSlip.Text.Trim = "PX" Then
                        strsql &= " ,[INVHNET] [numeric] (18, 3) NULL"
                        strsql &= " ,[INVHRATE] [numeric] (18, 3) NULL"
                        If Copercurr = "FC" Then strsql &= " ,[INVHNetFC] [numeric] (18, 3) NULL "

                        strsql &= "  ,[ClientRecID] [numeric] (18,0) NOT NULL"

                    End If

                    strsql &= " , PaymentMode nvarchar(400)"

                    If txtTypeSlip.Text.Trim = "IN" Or txtTypeSlip.Text.Trim = "CN" Then
                        strsql &= "  , [OnAcSett] [varchar](1) NULL "
                    End If

                    'GS on 13/05/16
                    strsql &= " Primary Key CLUSTERED ([DocType],[Prefix],[Serial],[InvHead])) "
                Case "CI", "CO"
                    strsql &= " [Chk] [varchar](5) NOT NULL ,"
                    'InvHead needed for print function but won't be shown in grid
                    strsql &= " [InvHead] [numeric] (18,0) NOT NULL, "
                    strsql &= " [DocType] [varchar](2) NOT NULL ,"
                    strsql &= " [Prefix] [varchar](1) NOT NULL ,"
                    strsql &= " [Serial] [numeric](18,0) NOT NULL ,"
                    strsql &= " [Date] [datetime] NULL ,"
                    strsql &= " [TotalAmount] [decimal](18,3) NULL ,"
                    strsql &= " [Currency] [varchar](3) NULL ,"
                    strsql &= " [CashReg] varchar(2) , "
                    strsql &= " [UserStamp] varchar(4) , "
                    strsql &= " [Remarks] [nvarchar](3000) NULL "
                    strsql &= " Primary Key CLUSTERED ([DocType],[Prefix],[Serial],[InvHead])) "
                Case "UR"
                    strsql &= " [Chk] [varchar](5) NOT NULL ,"

                    strsql &= " [DocType] [varchar](2) NOT NULL ,"
                    strsql &= " [Prefix] [varchar](1) NOT NULL ,"
                    strsql &= " [Serial] [numeric](18,0) NOT NULL ,"
                    strsql &= " [Date] [datetime] NULL ,"
                    strsql &= " [ClientNo] [numeric](18,0) NULL ,"
                    strsql &= " [ClientSubNo] [numeric](2,0) NULL ,"
                    strsql &= " [ClientName] [nvarchar](50) NULL ,"
                    strsql &= " Primary Key CLUSTERED ([DocType],[Prefix],[Serial])) "
                Case "WR"
                    strsql &= " [Chk] [varchar](5) NOT NULL ,"
                    strsql &= " [roh_recid] [numeric](18,0) NOT NULL ,"
                    strsql &= " [DocType] [varchar](2) NOT NULL ,"
                    strsql &= " [Prefix] [varchar](1) NOT NULL ,"
                    strsql &= " [Serial] [numeric](18,0) NOT NULL ,"
                    strsql &= " [Status] [varchar](25) NOT NULL,"
                    strsql &= " [Date] [datetime] NULL ,"
                    strsql &= " [SOPrefix] [varchar] (4) Not NULL ,"
                    strsql &= " [SOReference] [numeric](18,0) Not NULL ,"
                    strsql &= " [SOSeqNo] [numeric](2,0) Not NULL ,"
                    strsql &= " [ClientNo] [numeric](18,0) NULL ,"
                    strsql &= " [ClientSubNo] [numeric](2,0) NULL ,"
                    strsql &= " [ClientName] [nvarchar](50) NULL ,"
                    strsql &= " Primary Key CLUSTERED ([roh_recid])) "
                Case "DO"

                    strsql &= " [Chk] [varchar](5) NOT NULL ,"
                    strsql &= " [DOH_RecId] [numeric](18,0) NOT NULL ,"
                    strsql &= " [DocType] [varchar](2)  NULL ,"
                    strsql &= " [Prefix] [varchar](1)  NULL ,"
                    strsql &= " [Serial] [numeric](18,0)  NULL ,"
                    strsql &= " [Status] [varchar](25)  NULL,"
                    strsql &= " [Date] [datetime] NULL ,"
                    strsql &= " [SOPrefix] [varchar] (4)  NULL ,"
                    strsql &= " [SOReference] [numeric](18,0)  NULL ,"
                    strsql &= " [SOSeqNo] [numeric](2,0)  NULL ,"
                    strsql &= " [ClientNo] [numeric](18,0) NULL ,"
                    strsql &= " [ClientSubNo] [numeric](2,0) NULL ,"
                    strsql &= " [ClientName] [nvarchar](50) NULL ,"
                    strsql &= " [Status DO] [nvarchar](50) NULL,"
                    strsql &= " Primary Key CLUSTERED ([DOH_RecId])) "
                Case "A1", "A2"
                    strsql &= " [Chk] [varchar](5) NOT NULL ,"
                    strsql &= " [DocType] [varchar](2) NOT NULL ,"
                    strsql &= " [Prefix] [varchar](1) NOT NULL ,"
                    strsql &= " [Serial] [numeric](18,0) NOT NULL ,"
                    strsql &= " [TransDate] [datetime] NULL ,"
                    strsql &= " [Date] [datetime] NULL ,"
                    strsql &= " [ClientNo] [numeric](18,0) NULL ,"
                    strsql &= " [ClientSubNo] [numeric](2,0) NULL ,"
                    strsql &= " [ClientName] [nvarchar](50) NULL ,"
                    strsql &= " [Address] [nvarchar](300) NULL ,"
                    strsql &= " [CashReg] varchar(2) , "
                    strsql &= " [UserStamp] varchar(4) , "
                    strsql &= " [Remarks] [nvarchar](3000) NULL "
                    strsql &= " Primary Key CLUSTERED ([DocType],[Prefix],[Serial])) "
                Case "TR", "AS", "PR"
                    strsql &= " [Chk] [varchar](5) NOT NULL ,"
                    strsql &= " [Serial] [numeric](18,0) NOT NULL ,"
                    strsql &= " [Date] [datetime] NULL ,"
                    strsql &= " [Status] [varchar](1) NOT NULL ,"
                    strsql &= " [StatusDesc] [varchar](15) NOT NULL ,"
                    strsql &= " [Remarks] [nvarchar](3000) NULL "
                    strsql &= " Primary Key CLUSTERED ([Serial],[Status])) "
                    'added by GS on 09/05/16
                Case "DS"
                    strsql &= " [Chk] [varchar](5) NOT NULL ,"
                    strsql &= " [Serial] [numeric](18,0) NOT NULL ,"
                    strsql &= " [TransDate] [datetime] NULL ,"
                    strsql &= " [Date] [datetime] NULL ,"
                    strsql &= " [Status] [varchar](1) NOT NULL ,"
                    strsql &= " [Remarks] [nvarchar](3000) NULL "
                    strsql &= " Primary Key CLUSTERED ([Serial],[Status])) "
                Case "CS"
                    strsql &= " [Chk] [varchar](5) NOT NULL,"
                    strsql &= " [Cons. ID] [numeric] (18, 0) NOT NULL,"
                    strsql &= " [Status] [varchar] (7) NOT NULL,"
                    strsql &= " [Type] [varchar] (22) NOT NULL,"
                    strsql &= " [Date] [datetime] NOT NULL,"
                    strsql &= " [CashReg] varchar(2) , "
                    strsql &= " [UserStamp] varchar(4) , "
                    strsql &= " [Remarks] [varchar] (250) NULL,"
                    strsql &= " Primary Key CLUSTERED ([Cons. ID])) "
                    'GS on 09/05/16
                Case Else
                    Exit Try
            End Select


            cnSql = New SqlConnection(ConnectionString)
            cnSql.Open()
            cmSql = New SqlCommand(strsql, cnSql)
            cmSql.ExecuteNonQuery()

            cbSql = Nothing

            'Built instance variable
            strsql = "Select * From " & TempTable & " order by date desc"
            strsql &= IIf(txtTypeSlip.Text.Trim <> "CS", ",Serial desc", ",[Cons. ID] desc").ToString 'modified by GS on 09/05/16 'add IIF

            dsRepSlip = New DataSet
            daRepSlip = New SqlDataAdapter(strsql, cnSql)

            With daRepSlip
                .TableMappings.Add("Table", "tblRepSlip")
                .Fill(dsRepSlip)
            End With

            cbSql = New SqlCommandBuilder(daRepSlip)

            With daRepSlip
                .UpdateCommand = cbSql.GetUpdateCommand
                .InsertCommand = cbSql.GetInsertCommand
                .DeleteCommand = cbSql.GetDeleteCommand
            End With

            With LstItems
                .DataSource = dsRepSlip.Tables("tblRepSlip").DefaultView
                .AllowSort = True

                With .Splits(0).DisplayColumns("chk")
                    .DataColumn.ValueItems.Presentation = PresentationEnum.CheckBox
                    .Width = 30
                    .Style.HorizontalAlignment = AlignHorzEnum.Center
                    .Style.VerticalAlignment = AlignVertEnum.Center
                    .DataColumn.Caption = ""
                    .AllowSizing = True

                    '...Jad JE...WF:5033
                    If chkReservedInvoiceOnly.Visible = True And chkReservedInvoiceOnly.Checked = True And txtTypeSlip.Text.Trim = "IN" Then
                        .Visible = False
                        btnCheck.Visible = False
                        btnUncheck.Visible = False
                        btnPreview.Enabled = False
                        BtnDirectPrint.Enabled = False
                        gbCurr.Visible = False : gbPrint.Visible = False
                    Else
                        If chkExcelLayout.Checked = True AndAlso (txtTypeSlip.Text.Trim = "IN" Or txtTypeSlip.Text.Trim = "CN") Then
                            .Visible = False
                            btnCheck.Visible = False
                            btnUncheck.Visible = False
                        Else
                            .Visible = True
                            btnCheck.Visible = True
                            btnUncheck.Visible = True
                        End If
                        btnPreview.Enabled = lcPrivView
                        BtnDirectPrint.Enabled = lcPrivView And Not chkExcelLayout.Checked
                        gbCurr.Visible = txtTypeSlip.Text = "IN" Or txtTypeSlip.Text = "CN" '_
                        ' Or txtTypeSlip.Text = "CV" Or txtTypeSlip.Text = "RC"

                        If (txtTypeSlip.Text.Trim = "IN" Or txtTypeSlip.Text.Trim = "CN") And chkExcelLayout.Checked = True Then
                            gbPrint.Visible = False
                        Else
                            gbPrint.Visible = True
                        End If

                    End If
                    '....

                End With
                If txtTypeSlip.Text <> "WR" And txtTypeSlip.Text <> "DO" Then
                    If Not dsRepSlip.Tables(0).Columns("InvHead") Is Nothing Then
                        With .Splits(0).DisplayColumns("InvHead")
                            .Visible = False
                            .AllowSizing = False
                        End With
                    End If
                End If
                If Not dsRepSlip.Tables(0).Columns("DocType") Is Nothing Then
                    With .Splits(0).DisplayColumns("DocType")
                        .DataColumn.Caption = "Doc.Type"
                        .Style.HorizontalAlignment = AlignHorzEnum.Center
                        .Style.VerticalAlignment = AlignVertEnum.Center
                        .AllowSizing = True
                        Select Case txtTypeSlip.Text
                            Case "UR", "TR", "AS", "DS", "A1", "A2", "PR"
                                .Width = 60
                            Case Else
                                .Width = 40
                        End Select

                    End With
                End If

                If Not dsRepSlip.Tables(0).Columns("Prefix") Is Nothing Then
                    With .Splits(0).DisplayColumns("Prefix")
                        .DataColumn.Caption = "Pref."
                        .Style.HorizontalAlignment = AlignHorzEnum.Center
                        .Style.VerticalAlignment = AlignVertEnum.Center
                        Select Case txtTypeSlip.Text
                            Case "UR", "TR", "AS", "DS", "A1", "A2", "PR"
                                .Width = 70
                                .AllowSizing = True
                            Case Else
                                .Width = 30
                                .AllowSizing = True
                        End Select
                    End With
                End If
                'ch @07/08/2019 mail(Maliks-ERP-POS Operation-Stock RV# In Reprint Documents)
                If txtTypeSlip.Text.Trim = "RC" Then
                    With .Splits(0).DisplayColumns("RV#")
                        .DataColumn.Caption = "RV #"
                        .Style.HorizontalAlignment = AlignHorzEnum.Center
                        .Style.VerticalAlignment = AlignVertEnum.Center
                        .Width = 80
                        .AllowSizing = True
                    End With
                End If
                If Not dsRepSlip.Tables(0).Columns("Serial") Is Nothing Then
                    With .Splits(0).DisplayColumns("Serial")
                        .DataColumn.Caption = "Serial"
                        .Style.HorizontalAlignment = AlignHorzEnum.Center
                        .Style.VerticalAlignment = AlignVertEnum.Center

                        Select Case txtTypeSlip.Text
                            Case "UR", "TR", "AS", "DS", "A1", "A2", "PR"
                                .Width = 70
                                .AllowSizing = True
                            Case Else
                                .Width = 80
                                .AllowSizing = True
                        End Select
                    End With
                End If
                If txtTypeSlip.Text = "WR" OrElse txtTypeSlip.Text = "DO" Then
                    With .Splits(0).DisplayColumns("Status")
                        .DataColumn.Caption = "Type"
                        .Style.HorizontalAlignment = AlignHorzEnum.Center
                        .Style.VerticalAlignment = AlignVertEnum.Center
                        .AllowSizing = True
                        .Width = 150

                    End With

                    If txtTypeSlip.Text = "WR" Then
                        With .Splits(0).DisplayColumns("roh_recid")
                            .Visible = False
                            .AllowSizing = False
                            .Locked = True

                            .Width = 0

                        End With
                    ElseIf txtTypeSlip.Text = "DO" Then
                        With .Splits(0).DisplayColumns("DOH_RecId")
                            .Visible = False
                            .AllowSizing = False
                            .Locked = True

                            .Width = 0

                        End With

                        With .Splits(0).DisplayColumns("Status DO")
                            .DataColumn.Caption = "Status"
                            .Style.HorizontalAlignment = AlignHorzEnum.Near
                            .Style.VerticalAlignment = AlignVertEnum.Center
                            .AllowSizing = True
                            .Width = 100
                        End With
                    End If
                End If
                If Not dsRepSlip.Tables(0).Columns("Date") Is Nothing Then
                    With .Splits(0).DisplayColumns("Date")
                        .Style.HorizontalAlignment = AlignHorzEnum.Center
                        If txtTypeSlip.Text = "DS" OrElse txtTypeSlip.Text = "A1" OrElse txtTypeSlip.Text = "A2" Then
                            .DataColumn.Caption = "Date Stamp"
                        Else
                            .DataColumn.Caption = "Date"
                        End If

                        .DataColumn.NumberFormat = "dd/MM/yyyy"
                        'Select txtTypeSlip.Text
                        '    Case "UR", "TR", "AS", "DS", "A1", "A2"
                        '        .Width = 90
                        '    Case Else
                        '        .Width = 40
                        'End Select
                        .Width = 80
                        .AllowSizing = True
                    End With
                End If

                If txtTypeSlip.Text = "DS" OrElse txtTypeSlip.Text = "A1" OrElse txtTypeSlip.Text = "A2" Then
                    With .Splits(0).DisplayColumns("TransDate")
                        .Style.HorizontalAlignment = AlignHorzEnum.Center
                        .DataColumn.Caption = "Transaction Date"
                        .DataColumn.NumberFormat = "dd/MM/yyyy"
                        .Width = 120
                        .AllowSizing = True
                    End With
                End If

                If txtTypeSlip.Text = "WR" OrElse txtTypeSlip.Text = "DO" Then
                    With .Splits(0).DisplayColumns("SOPrefix")
                        .DataColumn.Caption = "SO Prefix"
                        .Style.HorizontalAlignment = AlignHorzEnum.Center
                        .Style.VerticalAlignment = AlignVertEnum.Center
                        .Width = 50
                        .AllowSizing = True
                    End With
                    With .Splits(0).DisplayColumns("SOReference")
                        .DataColumn.Caption = "SO Reference"
                        .Style.HorizontalAlignment = AlignHorzEnum.Center
                        .Style.VerticalAlignment = AlignVertEnum.Center
                        .Width = 70
                        .AllowSizing = True
                    End With
                    With .Splits(0).DisplayColumns("SOSeqNo")
                        .DataColumn.Caption = "SO Seq No"
                        .Style.HorizontalAlignment = AlignHorzEnum.Center
                        .Style.VerticalAlignment = AlignVertEnum.Center
                        .Width = 50
                        .AllowSizing = True
                    End With
                End If
                If Not dsRepSlip.Tables(0).Columns("ClientNo") Is Nothing Then
                    With .Splits(0).DisplayColumns("ClientNo")
                        .DataColumn.Caption = "Client #"
                        .Style.HorizontalAlignment = AlignHorzEnum.Near
                        .Style.VerticalAlignment = AlignVertEnum.Center
                        .Width = 60
                        Select Case txtTypeSlip.Text
                            Case "A1", "A2"
                                .Visible = False
                                .AllowSizing = False
                        End Select
                    End With
                End If

                If Not dsRepSlip.Tables(0).Columns("ClientSubNo") Is Nothing Then
                    With .Splits(0).DisplayColumns("ClientSubNo")
                        .DataColumn.Caption = "Sub"
                        .Style.HorizontalAlignment = AlignHorzEnum.Center
                        .Style.VerticalAlignment = AlignVertEnum.Center
                        .Width = 30
                        Select Case txtTypeSlip.Text
                            Case "A1", "A2"
                                .Visible = False
                                .AllowSizing = False
                        End Select
                    End With
                End If

                If Not dsRepSlip.Tables(0).Columns("ClientName") Is Nothing Then
                    With .Splits(0).DisplayColumns("ClientName")
                        .DataColumn.Caption = "Client Name"
                        .Style.HorizontalAlignment = AlignHorzEnum.Near
                        .Style.VerticalAlignment = AlignVertEnum.Center
                        .Width = 195
                        Select Case txtTypeSlip.Text
                            Case "A1", "A2"
                                .Visible = False
                                .AllowSizing = False
                        End Select
                    End With
                End If
                If txtTypeSlip.Text <> "WR" OrElse txtTypeSlip.Text <> "DO" Then
                    If Not dsRepSlip.Tables(0).Columns("Address") Is Nothing Then
                        With .Splits(0).DisplayColumns("Address")
                            .DataColumn.Caption = "Address"
                            .Style.HorizontalAlignment = AlignHorzEnum.Near
                            .Style.VerticalAlignment = AlignVertEnum.Center
                            If txtTypeSlip.Text.Trim = "IN" Or txtTypeSlip.Text.Trim = "CN" Then
                                .Width = 120
                            Else
                                .Width = 150
                            End If

                            Select Case txtTypeSlip.Text
                                Case "A1", "A2"
                                    .Visible = False
                                    .AllowSizing = False
                            End Select

                        End With
                    End If

                    If Not dsRepSlip.Tables(0).Columns("TotalAmount") Is Nothing Then
                        With .Splits(0).DisplayColumns("TotalAmount")
                            .DataColumn.Caption = "Total Amount"
                            .Style.HorizontalAlignment = AlignHorzEnum.Far
                            .Style.VerticalAlignment = AlignVertEnum.Center
                            .DataColumn.NumberFormat = "N3"

                            '...Jad-J...WF:5033
                            If txtTypeSlip.Text.Trim = "IN" And chkReservedInvoiceOnly.Checked = True Then
                                .Visible = False : .AllowSizing = False
                            Else
                                .Visible = AllowViewTransTotal : .AllowSizing = AllowViewTransTotal
                            End If

                        End With
                    End If

                    If Not dsRepSlip.Tables(0).Columns("Currency") Is Nothing Then
                        With .Splits(0).DisplayColumns("Currency")
                            .DataColumn.Caption = "Currency"
                            .Style.HorizontalAlignment = AlignHorzEnum.Center
                            .Style.VerticalAlignment = AlignVertEnum.Center
                        End With
                    End If

                    If Copercurr = "FC" Then

                        If Not dsRepSlip.Tables(0).Columns("InvCurr") Is Nothing Then
                            With .Splits(0).DisplayColumns("InvCurr")
                                .DataColumn.Caption = "Curr"
                                .Style.HorizontalAlignment = AlignHorzEnum.Center
                                .Style.VerticalAlignment = AlignVertEnum.Center
                                .Width = 35
                            End With
                        End If
                        If Not dsRepSlip.Tables(0).Columns("INVHNetFC") Is Nothing Then
                            With .Splits(0).DisplayColumns("INVHNetFC")
                                .Visible = False
                                .AllowSizing = False
                            End With
                        End If

                    End If

                    If Not dsRepSlip.Tables(0).Columns("INVH_OriginalCURR") Is Nothing Then
                        With .Splits(0).DisplayColumns("INVH_OriginalCURR")
                            .Visible = False
                            .AllowSizing = False
                        End With
                    End If


                    If Not dsRepSlip.Tables(0).Columns("ClientRecID") Is Nothing Then
                        With .Splits(0).DisplayColumns("ClientRecID")
                            .Visible = False
                            .AllowSizing = False
                        End With
                    End If

                    If txtTypeSlip.Text.Trim = "IN" Or txtTypeSlip.Text.Trim = "CN" Then
                        strsql &= "  ,[BrBa] [Varchar](20) NOT NULL"
                        If Not dsRepSlip.Tables(0).Columns("BrBa") Is Nothing Then
                            With .Splits(0).DisplayColumns("BrBa")
                                .DataColumn.Caption = "BR/BA"
                                .Style.HorizontalAlignment = AlignHorzEnum.Near
                                .Style.VerticalAlignment = AlignVertEnum.Center
                                .Width = 50
                            End With
                        End If
                    End If

                    If txtTypeSlip.Text.Trim = "IN" Or txtTypeSlip.Text.Trim = "CN" Then
                        If chkImportedOffer.Checked Then
                            If Not dsRepSlip.Tables(0).Columns("ImpFromOffer") Is Nothing Then
                                With .Splits(0).DisplayColumns("ImpFromOffer")
                                    .DataColumn.Caption = "Imported From Offer"
                                    .Style.HorizontalAlignment = AlignHorzEnum.Center
                                    .Style.VerticalAlignment = AlignVertEnum.Center
                                    .Width = 130
                                End With
                            End If
                        End If

                        If chkAE.Checked Then
                            With .Splits(0).DisplayColumns("EMP")
                                .Width = 75
                                .DataColumn.Caption = "A/E Code"
                                .Style.HorizontalAlignment = AlignHorzEnum.Center
                            End With
                            With .Splits(0).DisplayColumns("Ename")
                                .Width = 100
                                .DataColumn.Caption = "A/E Name"
                                .Style.HorizontalAlignment = AlignHorzEnum.Near
                            End With
                        End If


                        With .Splits(0).DisplayColumns("OnAcSett")
                            .Visible = False
                            .AllowSizing = False
                        End With

                        If Not dsRepSlip.Tables(0).Columns("ISOS#") Is Nothing Then
                            With .Splits(0).DisplayColumns("ISOS#")
                                .DataColumn.Caption = "ISOS #"
                                .Style.HorizontalAlignment = AlignHorzEnum.Near
                                .Style.VerticalAlignment = AlignVertEnum.Center
                                .Visible = gbISOSModuleIsActive
                                .AllowSizing = gbISOSModuleIsActive
                            End With
                        End If

                    End If

                    If Not dsRepSlip.Tables(0).Columns("Status") Is Nothing Then
                        With .Splits(0).DisplayColumns("Status")
                            .DataColumn.Caption = "Type"
                            .Style.HorizontalAlignment = AlignHorzEnum.Near
                            .Style.VerticalAlignment = AlignVertEnum.Center
                            .AllowSizing = True
                        End With
                    End If

                    If Not dsRepSlip.Tables(0).Columns("CashReg") Is Nothing Then
                        With .Splits(0).DisplayColumns("CashReg")
                            .DataColumn.Caption = "Cash Reg"
                            .Style.HorizontalAlignment = AlignHorzEnum.Center
                            .Style.VerticalAlignment = AlignVertEnum.Center
                            .Width = 80
                        End With
                    End If

                    If Not dsRepSlip.Tables(0).Columns("UserStamp") Is Nothing Then
                        With .Splits(0).DisplayColumns("UserStamp")
                            .DataColumn.Caption = "User Stamp"
                            .Style.HorizontalAlignment = AlignHorzEnum.Center
                            .Style.VerticalAlignment = AlignVertEnum.Center
                            .Width = 80
                        End With
                    End If

                    If Not dsRepSlip.Tables(0).Columns("Remarks") Is Nothing Then
                        With .Splits(0).DisplayColumns("Remarks")
                            .DataColumn.Caption = "Remarks"
                            .Style.HorizontalAlignment = AlignHorzEnum.Near
                            .Style.VerticalAlignment = AlignVertEnum.Center
                            If txtTypeSlip.Text.Trim = "IN" Or txtTypeSlip.Text.Trim = "CN" Then
                                .Width = 80
                            Else
                                .Width = 100
                            End If


                        End With
                    End If

                    If Not dsRepSlip.Tables(0).Columns("PaymentMode") Is Nothing Then
                        With .Splits(0).DisplayColumns("PaymentMode")
                            .DataColumn.Caption = "Payment Mode"
                            .Style.HorizontalAlignment = AlignHorzEnum.Near
                            .Style.VerticalAlignment = AlignVertEnum.Center
                            .AllowSizing = True
                            .Width = 200
                        End With
                    End If


                    'added by GS on 13/05/16
                    If Not dsRepSlip.Tables(0).Columns("Cons. ID") Is Nothing Then
                        With .Splits(0).DisplayColumns("Cons. ID")
                            .DataColumn.Caption = "Cons. ID"
                            .Style.HorizontalAlignment = AlignHorzEnum.Center
                            .Style.VerticalAlignment = AlignVertEnum.Center
                            '.Width = 200
                        End With
                    End If
                    'GS on 13/05/16
                End If
                Select Case txtTypeSlip.Text.Trim
                    Case "TR", "AS", "PR"
                        With .Splits(0).DisplayColumns("Status")
                            .DataColumn.Caption = "Status"
                            .Style.HorizontalAlignment = AlignHorzEnum.Near
                            .Style.VerticalAlignment = AlignVertEnum.Center
                            .AllowSizing = False
                            .Visible = False
                        End With
                        With .Splits(0).DisplayColumns("StatusDesc")
                            .DataColumn.Caption = "Status"
                            .Style.HorizontalAlignment = AlignHorzEnum.Near
                            .Style.VerticalAlignment = AlignVertEnum.Center
                            .AllowSizing = True
                        End With

                End Select
                'added by GS on 13/05/16
                If txtTypeSlip.Text.Trim = "IN" Or txtTypeSlip.Text.Trim = "CN" _
                                Or txtTypeSlip.Text.Trim = "CV" Or txtTypeSlip.Text.Trim = "DV" Or txtTypeSlip.Text.Trim = "RC" Or txtTypeSlip.Text.Trim = "RX" OrElse txtTypeSlip.Text.Trim = "PU" OrElse txtTypeSlip.Text.Trim = "PX" Then

                    .Splits(0).DisplayColumns("INVHNET").Visible = False
                    .Splits(0).DisplayColumns("INVHNET").AllowSizing = False
                    .Splits(0).DisplayColumns("INVHRATE").Visible = False
                    .Splits(0).DisplayColumns("INVHRATE").AllowSizing = False
                    .Splits(0).DisplayColumns("TotalAmount").DataColumn.NumberFormat = IIf(rbLC.Checked, "N" & VatLCRound, "N3").ToString
                Else
                    'If txtTypeSlip.Text <> "WR" OrElse txtTypeSlip.Text <> "DO" Then
                    If Not dsRepSlip.Tables(0).Columns("TotalAmount") Is Nothing Then
                        .Splits(0).DisplayColumns("TotalAmount").DataColumn.NumberFormat = IIf(rbLC.Checked, "N" & VatLCRound, "N3").ToString
                    End If
                    'End If
                End If
                'GS on 13/05/16
                For i = 0 To .Columns.Count - 1
                    If dsRepSlip.Tables("tblRepSlip").Columns(i).DataType Is System.Type.GetType("System.String") Then
                        .Splits(0).DisplayColumns(i).DataColumn.FilterOperator = "LIKE"
                    Else
                        .Splits(0).DisplayColumns(i).DataColumn.FilterOperator = "="
                    End If
                    .Splits(0).DisplayColumns(i).HeadingStyle.HorizontalAlignment = AlignHorzEnum.Center
                    .Splits(0).DisplayColumns(i).HeadingStyle.VerticalAlignment = AlignVertEnum.Center

                    .Splits(0).DisplayColumns(i).Locked = True
                Next i

                .Splits(0).DisplayColumns("chk").Locked = False
                .Splits(0).DisplayColumns("chk").AllowSizing = False

            End With


        Catch ex As Exception
            dsRepSlip = Nothing
            daRepSlip = Nothing
            Throw ex
        Finally
            cbSql = Nothing
            cmSql = Nothing
            If Not cnSql Is Nothing Then
                If cnSql.State = ConnectionState.Open Then cnSql.Close()
                cnSql = Nothing
            End If
        End Try

    End Sub

    Private Sub UpdateGrid(Optional promptmsg As Boolean = True)
        Dim DateVar As String
        Try

            Select Case txtTypeSlip.Text.Trim
                Case "IN", "RC", "PU", "PX", "CN", "CV", "DV", "RX"
                    strsql = "Delete from " & TempTable & "; "
                    strsql &= "Insert Into " & TempTable & " "
                    strsql &= "Select distinct "
                    strsql &= "'False', INVH_RecId ,INVH_DOCTYP1,INVH_DOCPREFIX,INVH_SERNO,INVH_DATE,CL_Code,CL_NO,INVH_CLNAME,"
                    'Rita M @ 12.08.2017. In case of client receipt of credit note value only read the client address from folio
                    If txtTypeSlip.Text.Trim = "RC" Or txtTypeSlip.Text.Trim = "RX" Or txtTypeSlip.Text.Trim = "CV" Or txtTypeSlip.Text.Trim = "DV" Then strsql &= " Fo_addres "

                    strsql &= " INVH_CLADDTEL,"
                    'ch @07/08/2019 mail(Maliks-ERP-POS Operation-Stock RV# In Reprint Documents)
                    If txtTypeSlip.Text.Trim = "RC" Then strsql &= "    (SELECT TOP 1 CT_RVNo FROM POS_CASHTRANSACTION WHERE CT_LOCATION = invh_loc AND   CT_INVHEADER = INVH_RECID )  [RV#] ,"
                    'added by GS on 13/05/16
                    If txtTypeSlip.Text.Trim = "IN" Or txtTypeSlip.Text.Trim = "CN" _
                                   Or txtTypeSlip.Text.Trim = "CV" Or txtTypeSlip.Text.Trim = "DV" Or txtTypeSlip.Text.Trim = "RC" Or txtTypeSlip.Text.Trim = "RX" OrElse txtTypeSlip.Text.Trim = "PU" OrElse txtTypeSlip.Text.Trim = "PX" Then
                        If rbLC.Checked Then
                            If Copercurr = gbCurr1 Then
                                strsql &= " INVH_Net"
                            ElseIf Copercurr = "FC" Then
                                strsql &= "   case when  '" & rbLC.Text & "' = INVH_CurrFC then INVH_NetFC else   INVH_Net * INVH_RATE end "
                            Else
                                strsql &= " INVH_Net * INVH_RATE"
                            End If
                        ElseIf rbCC.Checked = True Then
                            If Copercurr = gbCurr1 Then
                                strsql &= " INVH_Net / INVH_RATE"
                            ElseIf Copercurr = "FC" Then
                                strsql &= " case when '" & rbCC.Text & "' = INVH_CurrFC then INVH_NetFC else  INVH_Net end "
                            Else
                                strsql &= " INVH_Net"
                            End If
                        ElseIf rbOriginal.Checked = True Then
                            strsql &= " INVH_NetFC"
                        End If

                        If Copercurr = "FC" Then

                            If rbLC.Checked = True Then
                                strsql &= " , '" & gbCurr1 & "'"
                            ElseIf rbCC.Checked = True Then
                                strsql &= " , '" & gbCurr2 & "'"
                            ElseIf rbOriginal.Checked = True Then
                                strsql &= " , INVH_CurrFC"
                            End If

                        End If

                        strsql &= " , INVH_Curr" & CStr(IIf(Copercurr = "FC", "FC", ""))

                    Else
                        'GS on 13/05/16
                        strsql &= " INVH_Net" & CStr(IIf(Copercurr = "FC", "FC", ""))
                        strsql &= " , INVH_CurrFC, INVH_CurrFC"
                    End If

                    If txtTypeSlip.Text = "IN" Or txtTypeSlip.Text = "CN" OrElse txtTypeSlip.Text = "PU" OrElse txtTypeSlip.Text = "PX" Then
                        strsql &= " , ISNULL('BA ' + ( SELECT CASE WHEN CT_OPSIGN = '+' THEN '-'"
                        strsql &= "                              ELSE ''"
                        strsql &= "                         END + CAST(CT_BonAchatSer AS VARCHAR) + ', '"
                        strsql &= "                  FROM   POS_CASHTRANSACTION"
                        strsql &= "                  WHERE  CT_LOCATION = INVH_LOC"
                        strsql &= "                         AND CT_INVHEADER = INVH_RecId"
                        strsql &= "                         AND ISNULL(CT_BonAchatSer, 0) <> 0"
                        strsql &= "                         AND CT_SETTLTYPE = '314'"
                        strsql &= "                  ORDER BY CT_BonAchatSer ASC"
                        strsql &= "                FOR"
                        strsql &= "                  XML PATH('')"
                        strsql &= "                ), '') + ISNULL('BR '"
                        strsql &= "                                + ( SELECT   CAST(CT_BonAchatSer AS VARCHAR)"
                        strsql &= "                                    FROM     POS_CASHTRANSACTION"
                        strsql &= "                                    WHERE    CT_LOCATION = INVH_LOC"
                        strsql &= "                                             AND CT_INVHEADER = INVH_RecId"
                        strsql &= "                                             AND ISNULL(CT_BonAchatSer,"
                        strsql &= "                                                       0) <> 0"
                        strsql &= "                                             AND CT_SETTLTYPE = '315'"
                        strsql &= "                                  ), '')"
                        strsql &= " BonAchatSer "
                    End If

                    If txtTypeSlip.Text.Trim = "IN" Or txtTypeSlip.Text.Trim = "CN" Then
                        If chkImportedOffer.Checked Then strsql &= $" , ISNULL(STUFF(( SELECT   ', ' + offerNo
                                                                                       FROM     POS_SALEINVCO
                                                                                                INNER JOIN ( SELECT DOFR_Counter ,
                                                                                                                    DOFR_HeadRecId ,
                                                                                                                    DOFR_LOC
                                                                                                             FROM   STK04_OfferDetails
                                                                                                             WHERE DOFR_LOC = '{Cloc}'
                                                                                                           ) STK04_OfferDetails ON DOFR_Counter = POS_SALEINVCO.INVCO_OfferCounter
                                                                                                                      AND DOFR_HeadRecId = INVCO_OfferRecId
                                                                                                                      AND DOFR_LOC = INVCO_OfferLoc
                                                                                                INNER JOIN ( SELECT CONCAT(HOFR_Prefix,
                                                                                                                      ' - ',
                                                                                                                      HOFR_RECID,
                                                                                                                      ' / ',
                                                                                                                      HOFR_SeqNo) AS offerNo ,
                                                                                                                    HOFR_LOC ,
                                                                                                                    HOFR_HeadRecId ,
                                                                                                                    HOFR_CLTCODE
                                                                                                             FROM   STK04_OfferHeader
                                                                                                              WHERE HOFR_LOC = '{Cloc}'
                                                                                                           ) STK04_OfferHeader ON HOFR_LOC = DOFR_LOC
                                                                                                                      AND HOFR_HeadRecId = DOFR_HeadRecId
                                                                                       WHERE    INVCO_InvLoc = '{Cloc}'
                                                                                                AND INVCO_InvHeadID = INVH_RECID
                                                                                                AND HOFR_CLTCODE = CL_RecID
                                                                                       GROUP BY INVCO_InvHeadID ,
                                                                                                offerNo
                                                                                     FOR
                                                                                       XML PATH('')
                                                    ), 1, 1, ''), '') offerNo "
                        If gbISOSModuleIsActive Then strsql &= " , SUBSTRING(( SELECT DISTINCT
                                                                                         ', ' + IL_OrderCode
                                                                                FROM    POS_IsosLogging
                                                                                WHERE   IL_Location = INVH_LOC
                                                                                        AND IL_InvHeadRecid = INVH_RECID
                                                                                        AND IL_TransactionType = 'S'
                                                                              FOR
                                                                                XML PATH('')
                                                                              ), 3, 8000) AS IL_OrderCode "
                        If chkAE.Checked Then strsql &= " , INVH_EMP , E_Name  "
                    End If

                    strsql &= " , INVH_CSHREG "
                    strsql &= " , INVH_USTAMP "

                    If txtTypeSlip.Text = "RC" Or txtTypeSlip.Text.Trim = "RX" Then
                        strsql &= " , REPLACE(REPLACE(INVH_RKS, CHAR(13), ' '), CHAR(10),' ') INVH_RKS "
                    Else
                        strsql &= " ,INVH_RKS "
                    End If

                    'added by GS on 13/05/16
                    If txtTypeSlip.Text = "IN" Or txtTypeSlip.Text = "CN" _
                                                    Or txtTypeSlip.Text = "CV" Or txtTypeSlip.Text = "DV" Or txtTypeSlip.Text = "RC" Or txtTypeSlip.Text.Trim = "RX" OrElse txtTypeSlip.Text = "PU" OrElse txtTypeSlip.Text = "PX" Then

                        strsql &= " ,INVH_NET"
                        strsql &= " ,INVH_RATE"
                        If Copercurr = "FC" Then strsql &= " ,INVH_NETFC "
                    End If

                    strsql &= " ,INVH_CLIENTSUP "

                    'Rita M @ 23.03.2021. Show payment mode as concatenated
                    strsql &= " ,SUBSTRING(("
                    strsql &= " SELECT distinct '; ' + ST_DESC"
                    strsql &= "  FROM " & IIf(txtTypeSlip.Text.Trim = "RX", "POS_PreReceiptCT", "").ToString & " pos_cashtransaction INNER JOIN stk04_settlementtype ON st_loc=ct_location AND st_code=CT_SETTLTYPE"
                    strsql &= "  WHERE ct_recid='T' "
                    If txtStartDate.Value.ToString = "" AndAlso txtEndDate.Value.ToString <> "" Then
                        strsql &= " and FLOOR(CAST(ct_opdate AS FLOAT)) <= FLOOR(CAST(Convert(DateTime, '" & CDate(txtEndDate.Text).ToString("MM/dd/yyyy") & "') AS FLOAT))  "
                    ElseIf txtStartDate.Value.ToString <> "" AndAlso txtEndDate.Value.ToString = "" Then
                        strsql &= " and FLOOR(CAST(ct_opdate AS FLOAT)) >=   FLOOR(CAST( CONVERT(DATETIME, '" & CDate(txtStartDate.Text).ToString("MM/dd/yyyy") & "') AS FLOAT))  " '" & CDate(txtStartDate.Text).ToString("MM/dd/yyyy") & "'"
                    ElseIf txtStartDate.Value.ToString <> "" AndAlso txtEndDate.Value.ToString <> "" Then
                        strsql &= " and FLOOR(CAST(ct_opdate AS FLOAT)) Between    FLOOR(CAST( CONVERT(DATETIME, '" & CDate(txtStartDate.Text).ToString("MM/dd/yyyy") & "') AS FLOAT))  and  FLOOR(CAST( CONVERT(DATETIME, '" & CDate(txtEndDate.Text).ToString("MM/dd/yyyy") & "') AS FLOAT))  "  '"    '" & CDate(txtStartDate.Text).ToString("MM/dd/yyyy") & "' And '" & CDate(txtEndDate.Text).ToString("MM/dd/yyyy") & "'"
                    End If
                    strsql &= " And ct_location = invh_loc And ct_invheader = invh_recid"
                    strsql &= " FOR XML path('')),3,300)  PaymentMode"

                    If txtTypeSlip.Text.Trim = "IN" Or txtTypeSlip.Text.Trim = "CN" Then
                        strsql &= " ,     CASE WHEN CT_LOCATION IS NULL THEN 'N'
                                                 ELSE 'Y'
                                            END OnAcSett "
                    End If

                    'GS on 13/05/16
                    strsql &= " from " & IIf(txtTypeSlip.Text.Trim = "RX", "POS_PreReceipt", "").ToString & " Pos_saleinvhead "
                    strsql &= "Left join STK04_Clients on CL_RECID = INVH_CLIENTSUP "
                    If txtTypeSlip.Text.Trim = "RC" Or txtTypeSlip.Text.Trim = "RX" Or txtTypeSlip.Text.Trim = "CV" Or txtTypeSlip.Text.Trim = "DV" Then strsql &= " LEFT JOIN dbo.Com_Folio ON Com_Folio.Fo_serno = Stk04_Clients.CL_FolioNo AND Com_Folio.Fo_subno = Stk04_Clients.CL_FolioSubNo"
                    strsql &= " inner join POS_STOCKTRANSACTION POS_STOCKTRANSACTION "
                    strsql &= "On POS_SALEINVHEAD.INVH_LOC = POS_STOCKTRANSACTION.STT_LOCATION "
                    strsql &= "AND POS_SALEINVHEAD.INVH_DOCTYP1 = POS_STOCKTRANSACTION.STT_DOCTYPE1 "
                    strsql &= "AND POS_SALEINVHEAD.INVH_DOCPREFIX = POS_STOCKTRANSACTION.STT_PREFIX "

                    If (txtTypeSlip.Text.Trim = "IN" Or txtTypeSlip.Text.Trim = "CN") AndAlso chkAE.Checked Then strsql &= $" LEFT JOIN {IIf(ClocHO = False, "POS_Employee", "STK04_Employee")} POS_Employee ON INVH_EMP = POS_Employee.E_Code "

                    If txtTypeSlip.Text.Trim = "IN" Or txtTypeSlip.Text.Trim = "CN" Then
                        strsql &= " LEFT JOIN ( SELECT  CT_LOCATION ,
                                                        CT_INVHEADER ,
                                                        MAX(CT_SETTLTYPE) OnAcSett
                                                FROM    POS_CASHTRANSACTION
                                                WHERE   CT_SETTLTYPE = '500'
                                                        AND CT_LOCATION = '" & Cloc & "'
                                                GROUP BY CT_LOCATION ,
                                                        CT_INVHEADER
                                              ) POS_CASHTRANSACTION ON CT_LOCATION = INVH_LOC
                                                                       AND CT_INVHEADER = INVH_RECID "

                        'If gbISOSModuleIsActive Then strsql &= " LEFT JOIN ( SELECT  IL_Location ,
                        '                                                             IL_InvHeadRecid ,
                        '                                                             IL_OrderCode
                        '                                                     FROM    POS_IsosLogging
                        '                                                     WHERE   IL_Location = '" & Cloc & "'
                        '                                                   ) POS_IsosLogging ON IL_Location = INVH_LOC
                        '                                                                        AND IL_InvHeadRecid = INVH_RECID"
                    End If

                    strsql &= " where 1=1 "

                    strsql &= "and INVH_DOCTYP1 = '" & txtTypeSlip.Text.Trim & "' "
                    strsql &= "and invH_LOC = '" & Cloc & "' "

                    '...Jad JE...WF:5033
                    If txtTypeSlip.Text.Trim = "IN" Then
                        strsql &= " AND "
                        If chkReservedInvoiceOnly.Checked = True Then
                            strsql &= " NOT "
                        End If
                        strsql &= " EXISTS(SELECT top 1 1 FROM dbo.POS_SALEINVDET WHERE INVD_HEADID =  INVH_RECID  AND INVD_LOC = INVH_LOC) "
                    End If

                    'To use in date filtration
                    '    DateVar = "INVH_DATE"
                    DateVar = "  FLOOR(CAST(INVH_DATE AS FLOAT))  "
                Case "A1", "A2"
                    strsql = "Delete from " & TempTable & "; "
                    strsql &= "Insert Into " & TempTable & " "
                    strsql &= "Select distinct "
                    strsql &= "'False', INVH_DOCTYP1,INVH_DOCPREFIX,INVH_SERNO,INVH_DATE,INVH_DSTAMP,CL_Code,CL_NO,CL_NAME,INVH_CLADDTEL, "
                    strsql &= " INVH_CSHREG , "
                    strsql &= " INVH_USTAMP , "
                    strsql &= " INVH_RKS "
                    strsql &= "from Pos_saleinvhead "
                    strsql &= "Left join STK04_Clients on CL_RECID = INVH_CLIENTSUP "
                    strsql &= "inner join POS_STOCKTRANSACTION POS_STOCKTRANSACTION "
                    strsql &= "On POS_SALEINVHEAD.INVH_LOC = POS_STOCKTRANSACTION.STT_LOCATION "
                    strsql &= "AND POS_SALEINVHEAD.INVH_DOCTYP1 = POS_STOCKTRANSACTION.STT_DOCTYPE1 "
                    strsql &= "AND POS_SALEINVHEAD.INVH_DOCPREFIX = POS_STOCKTRANSACTION.STT_PREFIX "
                    strsql &= " where 1=1 "

                    strsql &= "and INVH_DOCTYP1 = '" & txtTypeSlip.Text.Trim & "' "
                    strsql &= "and invH_LOC = '" & Cloc & "' "

                    'To use in date filtration
                    '    DateVar = "INVH_DATE"
                    DateVar = "  FLOOR(CAST(INVH_DATE AS FLOAT))  "
                Case "WR"
                    strsql = "Delete from " & TempTable & "; "
                    strsql &= "Insert Into " & TempTable & " "
                    strsql &= "Select 'False',roh_recid , "
                    strsql &= " Roh_DOCTYP1,Roh_DOCPREFIX,Roh_SERNO, CASE WHEN ROH_Status = 'N' THEN 'Normal' "
                    strsql &= " WHEN ROH_Status='R' THEN 'Reverse'"
                    strsql &= " WHEN ROH_Status='D' THEN 'Reverse Since Delivered' "
                    strsql &= " WHEN  ROH_Status='I' then 'Reverse Since Invoiced' 	 ELSE 'Reverse' "
                    strsql &= "  END STATUS , Roh_ResDATE,HOFR_Prefix,HOFR_RECID,HOFR_SeqNo,CL_Code,CL_NO,HOFR_CLTNAME  "
                    strsql &= "  from STK04_ResOrdh INNER JOIN STK04_OfferHeader ON "
                    strsql &= "  dbo.STK04_OfferHeader.HOFR_LOC = dbo.stk04_ResOrdH.ROH_Loc "
                    strsql &= "   AND dbo.STK04_OfferHeader.HOFR_HeadRecId = dbo.stk04_ResOrdH.ROH_OffHeadID "
                    strsql &= " INNER JOIN dbo.Stk04_Clients ON dbo.Stk04_Clients.CL_RecID = dbo.STK04_OfferHeader.HOFR_CLTCODE "
                    strsql &= " where 1=1 "

                    strsql &= "and Roh_DOCTYP1 = '" & txtTypeSlip.Text.Trim & "' "
                    strsql &= "and ROh_Loc = '" & Cloc & "' "

                    'To use in date filtration
                    '   DateVar = "Roh_ResDATE"
                    DateVar = "  FLOOR(CAST(Roh_ResDATE AS FLOAT))  "
                Case "DO"
                    strsql = "Delete from " & TempTable & "; "
                    strsql &= "Insert Into " & TempTable & " "
                    strsql &= "Select 'False',DOH_RecId , "
                    strsql &= " DOH_DocTyp1,DOH_DocPrefix,DOH_Serno, CASE WHEN  DOH_Status='D' then 'Delivery' ELSE 'Withdrawal' "
                    strsql &= "  END STATUS , DOH_delDate,HOFR_Prefix,HOFR_RECID,HOFR_SeqNo,CL_Code,CL_NO,ISNULL(HOFR_CLTNAME, DOH_CLTNAME) HOFR_CLTNAME  "
                    strsql &= " ,ISNULL(Status, 'Not Invoiced') [Status DO] "
                    strsql &= " FROM    dbo.STK04_DelOrdh "
                    strsql &= " left JOIN (SELECT DISTINCT   HOFR_Prefix , "
                    strsql &= " HOFR_RECID , "
                    strsql &= " HOFR_SeqNo , "
                    strsql &= " ISNULL(Stk04_Clients.CL_Code, CltD.CL_Code) CL_Code , "
                    strsql &= " ISNULL(Stk04_Clients.CL_No, CltD.CL_No) CL_NO ,DOD_HrecId ,DOD_Loc , "
                    strsql &= " CASE WHEN SUM(COALESCE(sales1.INVD_QTY,
                                                       sales2.Invd_qty, 0)) = SUM(DOD_DelQty)
                                     THEN 'Totally Invoiced'
                                     WHEN SUM(COALESCE(sales1.INVD_QTY,
                                                       sales2.Invd_qty, 0)) <> 0
                                     THEN CASE WHEN DOH_InvCLT = 'Y' THEN 'Totally Invoiced' ELSE 'Partially Invoiced' END
                                     ELSE 'Not Invoiced'
                                 END [Status] ,"
                    strsql &= " HOFR_CLTNAME FROM STK04_DelOrdD   LEFT JOIN dbo.STK04_OfferDetails ON dbo.STK04_OfferDetails.DOFR_Counter = dbo.STK04_DelOrdD.DOD_OffCounter "
                    strsql &= " AND dbo.STK04_OfferDetails.DOFR_HeadRecId = dbo.STK04_DelOrdD.DOD_OffHRecID "
                    strsql &= " AND dbo.STK04_OfferDetails.DOFR_LOC = dbo.STK04_DelOrdD.DOD_OffLoc "
                    strsql &= " LEFT JOIN dbo.STK04_DelOrdH ON	STK04_DelOrdH.DOH_Loc = STK04_DelOrdD.DOD_Loc AND STK04_DelOrdH.DOH_RecId = STK04_DelOrdD.DOD_HrecId "
                    strsql &= " LEFT JOIN Stk04_Clients CltD ON CltD.CL_RecID = DOH_CLTCODE "
                    strsql &= " LEFT JOIN dbo.STK04_OfferHeader ON HOFR_HeadRecId = DOFR_HeadRecId "
                    strsql &= " AND HOFR_LOC = DOFR_LOC "
                    strsql &= " LEFT JOIN dbo.Stk04_Clients ON dbo.Stk04_Clients.CL_RecID = dbo.STK04_OfferHeader.HOFR_CLTCODE "
                    strsql &= " LEFT  JOIN dbo.POS_SALEINVDO ON POS_SALEINVDO.INVDO_DOLoc = STK04_DelOrdD.DOD_Loc
                                                              AND POS_SALEINVDO.INVDO_DORecId = STK04_DelOrdD.DOD_HrecId
                                                              AND POS_SALEINVDO.INVDO_DOCounter = STK04_DelOrdD.DOD_Counter
                                LEFT JOIN dbo.POS_SALEINVDET sales1 ON sales1.INVD_LOC = POS_SALEINVDO.INVDO_InvLoc
                                                          AND sales1.INVD_HEADID = POS_SALEINVDO.INVDO_InvHeadID
                                                          AND sales1.INVD_Counter = POS_SALEINVDO.INVDO_InvCounter
                                LEFT JOIN ( SELECT  SUM(INVD_QTY) Invd_qty ,
                                                    INVCO_OfferLoc ,
                                                    INVCO_OfferRecId ,
                                                    INVCO_OfferCounter
                                            FROM    POS_SALEINVCO
                                                    LEFT JOIN dbo.POS_SALEINVDET sales2 ON sales2.INVD_LOC = POS_SALEINVCO.INVCO_InvLoc
                                                          AND sales2.INVD_HEADID = POS_SALEINVCO.INVCO_InvHeadID
                                                          AND sales2.INVD_Counter = POS_SALEINVCO.INVCO_InvCounter
                                            GROUP BY INVCO_OfferLoc ,
                                                    INVCO_OfferRecId ,
                                                    INVCO_OfferCounter
                                          ) sales2 ON sales2.INVCO_OfferLoc = DOD_OffLoc
                                                      AND sales2.INVCO_OfferRecId = DOD_OffHRecID
                                                      AND sales2.INVCO_OfferCounter = DOD_OffCounter
							    GROUP BY  HOFR_Prefix ,
                                    HOFR_RECID ,
                                    HOFR_SeqNo ,
                                    ISNULL(Stk04_Clients.CL_Code, CltD.CL_Code) ,
                                    ISNULL(Stk04_Clients.CL_No, CltD.CL_No) ,
                                    DOD_HrecId ,
                                    DOD_Loc ,
                                    HOFR_CLTNAME,
                                    DOH_InvCLT "
                    strsql &= " ) STK04_DelOrdD ON  STK04_DelOrdD.DOD_LoC = dbo.STK04_DelOrdH.DOH_Loc "
                    strsql &= " AND STK04_DelOrdD.DOD_HrecId = dbo.STK04_DelOrdH.DOH_RecId "
                    strsql &= " where 1=1 "

                    strsql &= "and DOH_DocTyp1 = '" & txtTypeSlip.Text.Trim & "' "
                    strsql &= "and DOh_Loc = '" & Cloc & "' "

                    'To use in date filtration
                    '  DateVar = "DOH_delDate"
                    DateVar = "  FLOOR(CAST(DOH_delDate AS FLOAT))  "
                Case "UR"

                    strsql = "Delete from " & TempTable & "; "
                    strsql &= "Insert Into " & TempTable & " "
                    strsql &= "Select distinct "
                    strsql &= "'False',RepH_DOCTYP1, RepH_DOCPREFIX, RepH_SerNo, RepH_Date,CL_Code,CL_NO, isnull(RepH_CltName,CL_NAME) CL_NAME "
                    strsql &= "FROM dbo.POS_REPAIRITEMSHead INNER JOIN dbo.POS_REPAIRITEMSDet "
                    strsql &= "ON dbo.POS_REPAIRITEMSHead.RepH_Loc = dbo.POS_REPAIRITEMSDet.RepD_Loc "
                    strsql &= "AND dbo.POS_REPAIRITEMSHead.RepH_RecId = dbo.POS_REPAIRITEMSDet.RepD_HRecId "
                    strsql &= "INNER JOIN " & CStr(IIf(ClocHO = False, "POS_STKFIL", "STK04_STKFIL")) & " POS_STKFIL ON dbo.POS_REPAIRITEMSDet.RepD_StkId = POS_STKFIL.STK_RECID "
                    strsql &= "INNER JOIN dbo.stk04_location ON dbo.POS_REPAIRITEMSHead.RepH_Loc = dbo.stk04_location.loc_code "

                    strsql &= "inner join POS_STOCKTRANSACTION POS_STOCKTRANSACTION "
                    strsql &= "On POS_REPAIRITEMSHead.RepH_Loc = POS_STOCKTRANSACTION.STT_LOCATION "
                    strsql &= "AND POS_REPAIRITEMSHead.RepH_DOCTYP1 = POS_STOCKTRANSACTION.STT_DOCTYPE1 "
                    strsql &= "AND POS_REPAIRITEMSHead.RepH_DOCPREFIX = POS_STOCKTRANSACTION.STT_PREFIX "

                    strsql &= "Left join STK04_Clients on CL_RECID = RepH_ClRecID "

                    strsql &= " where 1=1 "
                    strsql &= "and RepH_Loc = '" & Cloc & "' "
                    'To use in date filtration
                    '       DateVar = "RepH_Date"
                    DateVar = "  FLOOR(CAST(RepH_Date AS FLOAT))  "
                Case "CI", "CO"
                    strsql = "Delete from " & TempTable & "; "
                    strsql &= "Insert Into " & TempTable & " "
                    strsql &= "Select distinct "
                    strsql &= "'False' [Chk], INVH_RECID, "
                    strsql &= "POS_SALEINVHEAD.INVH_DOCTYP1, POS_SALEINVHEAD.INVH_DOCPREFIX, POS_SALEINVHEAD.INVH_SERNO, "
                    strsql &= "POS_SALEINVHEAD.INVH_DATE, "
                    strsql &= " case when POS_SALEINVHEAD.INVH_CURRFC is null then POS_SALEINVHEAD.INVH_NET else POS_SALEINVHEAD.INVH_NETFC end ,isnull(POS_SALEINVHEAD.INVH_CURRFC,POS_SALEINVHEAD.INVH_CURR), "
                    strsql &= " POS_SALEINVHEAD.INVH_CSHREG , "
                    strsql &= " POS_SALEINVHEAD.INVH_USTAMP , "
                    strsql &= " POS_SALEINVHEAD.INVH_RKS "
                    strsql &= "From POS_SALEINVHEAD "
                    strsql &= "inner join POS_STOCKTRANSACTION POS_STOCKTRANSACTION "
                    strsql &= "On POS_SALEINVHEAD.INVH_LOC = POS_STOCKTRANSACTION.STT_LOCATION "
                    strsql &= "AND POS_SALEINVHEAD.INVH_DOCTYP1 = POS_STOCKTRANSACTION.STT_DOCTYPE1 "
                    strsql &= "AND POS_SALEINVHEAD.INVH_DOCPREFIX = POS_STOCKTRANSACTION.STT_PREFIX "
                    strsql &= " where 1=1 "

                    strsql &= "and POS_SALEINVHEAD.INVH_DOCTYP1 = '" & txtTypeSlip.Text.Trim & "' "
                    strsql &= "and POS_saleinvhead.invH_LOC = '" & Cloc & "' "

                    'To use in date filtration
                    '   DateVar = "INVH_DATE"
                    DateVar = "  FLOOR(CAST(INVH_DATE AS FLOAT))  "
                Case "TR", "AS", "PR"
                    strsql = "Delete from " & TempTable & "; "
                    strsql &= "Insert Into " & TempTable & " "
                    strsql &= "Select distinct "
                    strsql &= "'False' [Chk], TRSPH_RECID, "
                    strsql &= "TRSPH_DATE,TRSPH_STATUS, case TRSPH_STATUS when 'N' then 'Normal' else 'Reverse' end  ,RIGHT(POS_TRANSPOSITIONHEADER.TRSPH_REMARKS,LEN(POS_TRANSPOSITIONHEADER.TRSPH_REMARKS)-1) as TRSPH_REMARKS "
                    strsql &= "From POS_TRANSPOSITIONHEADER "
                    strsql &= "inner join POS_TRANSPOSITIONDETAILS ON POS_TRANSPOSITIONDETAILS.TRSPD_HRECID = POS_TRANSPOSITIONHEADER.TRSPH_RECID "

                    strsql &= " where 1=1 "
                    strsql &= "And TRSPH_Location = '" & Cloc & "' "

                    Select Case txtTypeSlip.Text.Trim
                        Case "TR"
                            strsql &= "And Left(TRSPH_REMARKS,1) = 't' "
                        Case "AS"
                            strsql &= "And Left(TRSPH_REMARKS,1) = 'a' "
                        Case "PR"
                            strsql &= "And Left(TRSPH_REMARKS,1) = 'P' "
                    End Select


                    'To use in date filtration
                    '  DateVar = "convert(DATETIME,TRSPH_DATE,103)" '...Jad-J...WF:4728
                    DateVar = "  FLOOR(CAST(TRSPH_DATE AS FLOAT))  "
                Case "DS"

                    strsql = "Delete from " & TempTable & "; "
                    strsql &= "Insert Into " & TempTable & " "
                    strsql &= "Select distinct"
                    strsql &= "'False' [Chk], "
                    strsql &= "DISPH_RECID [Serial],DISPH_DATE [TransDate] ,DISPH_DSTAMP [Date],DISPH_STATUS [Status],DISPH_REMARKS [Remarks] "
                    strsql &= "from POS_DISPOSALHEADER "
                    strsql &= "inner join POS_DISPOSALDETAILS ON "
                    strsql &= "DISPH_RECID = POS_DISPOSALDETAILS.DISPD_HRECID And "
                    strsql &= "DISPH_LOCATION = DISPD_LOCATION And DISPD_STATUS = DISPH_STATUS "

                    strsql &= " where 1=1 "
                    strsql &= "And DISPH_LOCATION = '" & Cloc & "' " ' ch @ 20/10/2015
                    'To use in date filtration
                    '  DateVar = "DISPH_DATE" '"DISPH_DSTAMP" 'edited by GS on 21/07/16
                    DateVar = "  FLOOR(CAST(DISPH_DATE AS FLOAT))  "
                    'added by GS on 09/05/16
                Case "CS"
                    strsql = "Delete from " & TempTable & "; "
                    strsql &= "Insert Into " & TempTable & " "
                    strsql &= "Select distinct "
                    strsql &= "'False' [Chk], CONSH_RECID,"
                    strsql &= "CASE WHEN CONSH_STATUS = 'N' THEN 'Normal' ELSE 'Reverse' END Status,"
                    strsql &= "CASE WHEN CONSD_TYPE = 'I' THEN 'Internal Use' WHEN CONSD_TYPE = 'P' THEN 'Production Use' ELSE 'Consumables & Services' END Type,"
                    strsql &= "CONSH_DATE, "
                    strsql &= " CONSH_CSHREG , "
                    strsql &= " CONSH_USTAMP , "
                    strsql &= " CONSH_REMARKS "
                    strsql &= "FROM POS_CONSUMPTIONH INNER JOIN POS_CONSUMPTIOND  ON CONSH_LOCATION = CONSD_LOC AND CONSH_RECID = CONSD_HEADID AND CONSH_STATUS = CONSD_STATUS "
                    strsql &= "WHERE 1=1 "
                    strsql &= "AND CONSH_LOCATION = '" & Cloc & "'"

                    'To use in date filtration
                    '  DateVar = "CONSH_DATE"
                    DateVar = "  FLOOR(CAST(CONSH_DATE AS FLOAT))  "
                    'GS on 09/05/16
            End Select

            If txtStartDate.Value.ToString = "" AndAlso txtEndDate.Value.ToString <> "" Then
                strsql &= " and " & DateVar & " <=     FLOOR(CAST( CONVERT(DATETIME, '" & CDate(txtEndDate.Text).ToString("MM/dd/yyyy") & "') AS FLOAT))  "
            ElseIf txtStartDate.Value.ToString <> "" AndAlso txtEndDate.Value.ToString = "" Then
                strsql &= " and " & DateVar & " >=   FLOOR(CAST( CONVERT(DATETIME, '" & CDate(txtStartDate.Text).ToString("MM/dd/yyyy") & "') AS FLOAT))  " '" & CDate(txtStartDate.Text).ToString("MM/dd/yyyy") & "'"
            ElseIf txtStartDate.Value.ToString <> "" AndAlso txtEndDate.Value.ToString <> "" Then
                strsql &= " and " & DateVar & " Between    FLOOR(CAST( CONVERT(DATETIME, '" & CDate(txtStartDate.Text).ToString("MM/dd/yyyy") & "') AS FLOAT))  and  FLOOR(CAST( CONVERT(DATETIME, '" & CDate(txtEndDate.Text).ToString("MM/dd/yyyy") & "') AS FLOAT))  "  '"    '" & CDate(txtStartDate.Text).ToString("MM/dd/yyyy") & "' And '" & CDate(txtEndDate.Text).ToString("MM/dd/yyyy") & "'"
            End If

            '  strsql &= " AND FLOOR(CAST(INVH_DATE AS FLOAT))  >=FLOOR(CAST(CONVERT(DATETIME, '" & CDate(WorkingDay).ToString("MM/dd/yyyy") & "' )AS FLOAT))  "

            Execute(strsql.ToString)

            If Not dsRepSlip.Tables(0) Is Nothing Then dsRepSlip.Tables(0).Clear()
            daRepSlip.Fill(dsRepSlip, dsRepSlip.Tables(0).TableName)
            LstItems.UpdateData()

            If dsRepSlip.Tables("tblRepSlip").Rows.Count = 0 AndAlso promptmsg = True Then
                MessageBox.Show("Selected Type of Document does not contain any record.", cntTitle,
                MessageBoxButtons.OK, MessageBoxIcon.Information)
                'txtTypeSlip.Text = ""
                'txtStartDate.Value = Today
                'txtEndDate.Value = Today

                'txtStartDate.Text = ""
                'txtEndDate.Text = ""

                'Select Case txtTypeSlip.Text
                '    Case "IN", "CN", "CV", "RC", "PU", "PX", "DO"
                '        rbLaser.Enabled = True : rbSlip.Enabled = True
                '    Case "A1", "A2", "UR", "CI", "CO", "TR", "AS", "DS"
                '        rbLaser.Enabled = False : rbSlip.Checked = True
                'End Select

            End If

            daRepSlip.Update(dsRepSlip.Tables("tblRepSlip"))

        Catch ex As Exception
            MessageBox.Show("Error: " & ex.Message, cntTitle,
MessageBoxButtons.OK, MessageBoxIcon.Error)
        End Try
    End Sub

    Private Sub btnUncheck_Click(ByVal sender As System.Object, ByVal e As System.EventArgs) Handles btnUncheck.Click
        Dim strFilter As String
        Try
            If dsRepSlip.Tables("tblRepSlip") Is Nothing Then Exit Sub

            strFilter = dsRepSlip.Tables("tblRepSlip").DefaultView.RowFilter

            If strFilter.Trim <> "" Then
                strFilter = " AND " & strFilter
            End If

            For Each dr As DataRow In dsRepSlip.Tables("tblRepSlip").Select(" chk = 'True' " & strFilter)
                dr("Chk") = False
            Next
            'recalculate Records Selected
            lblRecSel.Text = dsRepSlip.Tables("tblRepSlip").Select("Chk = 'True'").Count().ToString & "  Records Selected"
        Catch ex As Exception
            MessageBox.Show("Error: " & ex.Message, cntTitle, MessageBoxButtons.OK, MessageBoxIcon.Error)
        End Try
    End Sub

    Private Sub btnCheck_Click(ByVal sender As Object, ByVal e As System.EventArgs) Handles btnCheck.Click
        Dim strFilter As String
        Try
            If dsRepSlip.Tables("tblRepSlip") Is Nothing Then Exit Sub

            strFilter = dsRepSlip.Tables("tblRepSlip").DefaultView.RowFilter

            If strFilter.Trim <> "" Then
                strFilter = " AND " & strFilter
            End If

            For Each dr As DataRow In dsRepSlip.Tables("tblRepSlip").Select(" chk = 'False' " & strFilter)

                If gbReprintAcInvAfterSplit AndAlso (txtTypeSlip.Text.Trim = "IN" OrElse txtTypeSlip.Text.Trim = "CN") AndAlso dr("OnAcSett").ToString = "Y" Then
                    dr("Chk") = True
                Else

                    If TransactionDateLessThanLastMod0001ProccessDate(CDate(dr("Date")).ToString("dd/MM/yyyy"), False) = True Then
                    Else
                        dr("Chk") = True
                    End If

                End If

            Next
            'recalculate Records Selected
            lblRecSel.Text = dsRepSlip.Tables("tblRepSlip").Select("Chk = 'True'").Count().ToString & "  Records Selected"
        Catch ex As Exception
            MessageBox.Show("Error: " & ex.Message, cntTitle, MessageBoxButtons.OK, MessageBoxIcon.Error)
        End Try
    End Sub

    Private Sub LstItems_AfterColUpdate(ByVal sender As Object, ByVal e As C1.Win.C1TrueDBGrid.ColEventArgs) Handles LstItems.AfterColUpdate
        Try
            If e.ColIndex = 0 Then
                LstItems.UpdateData()
                LstItems.Refresh()
                'recalculate Records Selected
                lblRecSel.Text = dsRepSlip.Tables("tblRepSlip").Select("Chk = 'True'").Count().ToString & "  Records Selected"
            End If

        Catch ex As Exception
            MessageBox.Show("Error: " & ex.Message, cntTitle, MessageBoxButtons.OK, MessageBoxIcon.Error)
        End Try
    End Sub

    Private Sub btnOk_Click(ByVal sender As System.Object, ByVal e As System.EventArgs) Handles btnOk.Click
        Try
            GrpBefPrint.Visible = False
            Select Case txtTypeSlip.Text.Trim
                Case "A1", "A2"
                    PrintAdjust(Cloc, Nothing, True, txtTypeSlip.Text.Trim, chkPrintSupRef.Checked, "", Me, TempTable, rbLaser.Checked) 'edited by GS on 12/07/16 (add rbLaser.Checked)
                Case "IN", "CN", "PU", "PX"
                    'added by GS on 10/05/16
                    'strsql = "SELECT OP_OPCurr FROM STK04_OPParam WHERE OP_Loc='" & Cloc & "' AND OP_Active = 'Y'"
                    'Dim CurrLoc As String = ExecuteScalar(strsql)
                    Dim tocurr As String = ""
                    If rbLC.Checked Then
                        If Copercurr <> gbCurr1 Then
                            tocurr = gbCurr1
                        End If
                    ElseIf rbCC.Checked Then
                        If Copercurr <> gbCurr2 Then
                            tocurr = gbCurr2
                        End If
                    Else
                        tocurr = "FC"
                    End If

                    'GS on 10/05/16
                    PrintSalesInvoiceSlipLaser(rbSlip.Checked, Nothing, CStr(IIf(txtTypeSlip.Text.Trim = "IN" OrElse txtTypeSlip.Text.Trim = "PX", "S", "C")), Nothing, chkPrintClient.Checked, chkPrintTech.Checked, Me, TempTable, True, chkSupSubTot.Checked, chkPrintBund.Checked, tocurr, , , chkLink.Checked, , , , , , , , , ) 'edited by GS on 10/05/16 (add tocurr)
                    ' PrinSerialNo()
            End Select
        Catch ex As Exception
            MessageBox.Show("Error: " & ex.Message, cntTitle, MessageBoxButtons.OK, MessageBoxIcon.Error)
        End Try
    End Sub

    Private Function TypeSlipExists() As Boolean

        Dim cnsql As SqlConnection
        Dim cmsql As SqlCommand
        Dim drsql As SqlDataReader

        Try
            Select Case txtTypeSlip.Text.Trim
                Case "IN", "CN", "CV", "DV", "UR", "A1", "A2", "PU", "PX", "DO", "CI", "CO", "RC", "WR", "RX"
                    cnsql = New SqlConnection(ConnectionString)
                    cnsql.Open()
                    cmsql = New SqlCommand(strsql, cnsql)
                    strsql = "Select Top 1 * from POS_STOCKTRANSACTION "
                    strsql &= "Where STT_DOCTYPE1 = '" & txtTypeSlip.Text.Trim & "' "
                    strsql &= "And STT_LOCATION = '" & Cloc & "' "
                    strsql &= "And STT_Active = 'Y' "
                    cmsql.CommandText = strsql
                    drsql = cmsql.ExecuteReader

                    If drsql.HasRows Then Return True Else Return False
                    drsql.Close()

                    '"TR", "AS", "DS" is not included in validation
                Case "TR", "AS", "DS", "CS", "PR" 'modified by GS on 09/05/16 (add 'CS')
                    Return True
            End Select

        Catch ex As Exception
            Throw ex
        Finally
            Disposecontrols(cnsql, cmsql, drsql)
        End Try
    End Function

    'Private Sub GrpBefPrint_Leave(ByVal sender As Object, ByVal e As System.EventArgs) Handles GrpBefPrint.Leave
    '    If Me.ActiveControl.Name <> "btnPrint" Then GrpBefPrint.Visible = False
    'End Sub

    Private Sub Clock_Tick(ByVal sender As System.Object, ByVal e As System.EventArgs) Handles Clock.Tick
        Try
            stBar.Panels(3).Text = Now.ToLongTimeString
        Catch ex As Exception
        End Try
    End Sub

    Private Sub LstItems_FetchCellTips(sender As Object, e As FetchCellTipsEventArgs) Handles LstItems.FetchCellTips
        Try

            If txtTypeSlip.Text = "RC" Or txtTypeSlip.Text.Trim = "RX" AndAlso Not e.Column Is Nothing Then
                If e.Column.DataColumn.DataField = "Remarks" Then
                    e.CellTip = LstItems.Item(e.Row, "Remarks").ToString
                End If
            End If
        Catch ex As Exception
            MessageBox.Show("Error: " & ex.Message, cntTitle, MessageBoxButtons.OK, MessageBoxIcon.Error)
        End Try
    End Sub

    Private Sub LstItems_FilterChange(ByVal sender As Object, ByVal e As System.EventArgs) Handles LstItems.FilterChange
        Try
            If LstItems.FilterActive = True Then
                LstItems.AllowFilter = False : GridFilterChange(LstItems)
            End If
        Catch ex As Exception
            MessageBox.Show("Error: " & ex.Message, cntTitle, MessageBoxButtons.OK, MessageBoxIcon.Error)
        End Try
    End Sub

    Private Sub rbLaser_CheckedChanged(ByVal sender As Object, ByVal e As System.EventArgs) Handles rbLaser.CheckedChanged
        Try
            If GrpBefPrint.Visible Then GrpBefPrint.Visible = False
            If GbRc.Visible Then GbRc.Visible = False
        Catch ex As Exception
            MessageBox.Show("Error: " & ex.Message, cntTitle, MessageBoxButtons.OK, MessageBoxIcon.Error)
        End Try
    End Sub

    'added by GS on 13/05/16
    Private Sub rbLC_CheckedChanged(sender As System.Object, e As System.EventArgs) Handles rbLC.CheckedChanged, rbCC.CheckedChanged, rbOriginal.CheckedChanged
        Try
            If CType(sender, RadioButton).Checked = False Then Exit Try
            If Not Me.ActiveControl Is rbLC AndAlso Not Me.ActiveControl Is rbCC AndAlso Not Me.ActiveControl Is rbOriginal Then Exit Try

            If txtTypeSlip.Text.Trim = "IN" Or txtTypeSlip.Text.Trim = "CN" _
                         Or txtTypeSlip.Text.Trim = "CV" Or txtTypeSlip.Text.Trim = "DV" Or txtTypeSlip.Text.Trim = "RC" Or txtTypeSlip.Text.Trim = "RX" Then

                If dsRepSlip Is Nothing Then Exit Sub

                If Not dsRepSlip.GetChanges Is Nothing Then daRepSlip.Update(dsRepSlip, "tblRepSlip")

                strsql = "update " & TempTable
                If rbLC.Checked Then
                    If Copercurr = gbCurr1 Then
                        strsql &= " set TotalAmount = INVHNet"
                    ElseIf Copercurr = "FC" Then
                        strsql &= " set TotalAmount =  case when  '" & rbLC.Text & "' = INVH_OriginalCURR then INVHNetFC else  INVHNet * INVHRATE end "
                    Else
                        strsql &= " set TotalAmount = INVHNet * INVHRATE"
                    End If
                    If Copercurr = "FC" Then strsql &= ", InvCurr = '" & rbLC.Text & "'"
                ElseIf rbCC.Checked Then
                    If Copercurr = gbCurr1 Then
                        strsql &= " set TotalAmount = INVHNet / INVHRATE"
                    ElseIf Copercurr = "FC" Then
                        strsql &= " set TotalAmount =  case when '" & rbCC.Text & "' = INVH_OriginalCURR then INVHNetFC else  INVHNet end "
                    Else
                        strsql &= " set TotalAmount = INVHNet"
                    End If
                    If Copercurr = "FC" Then strsql &= ", InvCurr = '" & rbCC.Text & "'"
                ElseIf rbOriginal.Checked Then
                    strsql &= " set TotalAmount = INVHNetFC"
                    strsql &= ", InvCurr = INVH_OriginalCURR"
                End If
                Execute_WOXACT(strsql)

                dsRepSlip.Clear()
                daRepSlip.Fill(dsRepSlip)

                LstItems.Splits(0).DisplayColumns("TotalAmount").DataColumn.NumberFormat = IIf(rbLC.Checked And rbLC.Text = "LL", "N0", IIf(rbCC.Checked And rbCC.Text = "LL", "N0", "N3")).ToString
            End If
        Catch ex As Exception
            MessageBox.Show("Error: " & ex.Message, cntTitle, MessageBoxButtons.OK, MessageBoxIcon.Error)
        End Try
    End Sub


    'added by GS on 18/07/16 forbid ctrl+v in chk columns
    Private Sub Lst_KeyPress(sender As Object, e As System.Windows.Forms.KeyPressEventArgs) Handles LstItems.KeyPress
        Try
            If Asc(e.KeyChar) = 22 And CType(sender, C1TrueDBGrid).Columns(CType(sender, C1TrueDBGrid).Col).DataField = "chk" Then
                e.Handled = True
            End If
        Catch ex As Exception
            MessageBox.Show("Error: " & ex.Message, cntTitle, MessageBoxButtons.OK, MessageBoxIcon.Error)
        End Try
    End Sub


    Private Sub pnlAutoShow_Validating(sender As Object, e As System.ComponentModel.CancelEventArgs) Handles pnlAutoShow.Validating
        Try

            If Me.ActiveControl Is btnShow Then Exit Try

            If txtTypeSlip.Text.Trim = "" OrElse
                txtStartDate.Text.Trim = "" OrElse
                txtEndDate.Text.Trim = "" Then

                If LstItems.DataSource IsNot Nothing Then ClearGrid()

            End If


            btnShow_Click(btnShow, New System.EventArgs)



        Catch ex As Exception
            MessageBox.Show("Error: " & ex.Message, cntTitle, MessageBoxButtons.OK, MessageBoxIcon.Error)
        End Try
    End Sub

    Private Sub ClearGrid()
        Try

            lblRecSel.Text = "0 Records Selected"
            btnPreview.Enabled = False
            BtnDirectPrint.Enabled = False
            ClearGridFiltration(LstItems)
            Execute_WOXACT("Delete from " & TempTable)
            If Not dsRepSlip.Tables(0) Is Nothing Then dsRepSlip.Tables(0).Clear()
            daRepSlip.Fill(dsRepSlip, dsRepSlip.Tables(0).TableName)
            LstItems.UpdateData()

        Catch ex As Exception
            Throw
        End Try
    End Sub

    Private Sub chkExcelLayout_CheckedChanged(sender As Object, e As EventArgs) Handles chkExcelLayout.CheckedChanged
        Try

            gbPrint.Visible = Not chkExcelLayout.Checked
            BtnDirectPrint.Enabled = lcPrivView And Not chkExcelLayout.Checked
            If chkExcelLayout.Checked = True Then
                rbLaser.Checked = True
            End If

        Catch ex As Exception

        End Try
    End Sub

    Private Sub LstItems_Click(sender As Object, e As EventArgs) Handles LstItems.Click

    End Sub

    Private Sub GrpBefPrint_VisibleChanged(sender As Object, e As EventArgs) Handles GrpBefPrint.VisibleChanged
        Try
            BtnDirectPrint.Enabled = Not GrpBefPrint.Visible And Not chkExcelLayout.Checked
        Catch ex As Exception
            MessageBox.Show("Error: " & ex.Message, cntTitle, MessageBoxButtons.OK, MessageBoxIcon.Error)
        End Try
    End Sub

    Private Sub GbRc_VisibleChanged(sender As Object, e As EventArgs) Handles GbRc.VisibleChanged
        Try
            BtnDirectPrint.Enabled = Not GbRc.Visible And Not chkExcelLayout.Checked
        Catch ex As Exception
            MessageBox.Show("Error: " & ex.Message, cntTitle, MessageBoxButtons.OK, MessageBoxIcon.Error)
        End Try
    End Sub

    Private Sub LstItems_BeforeColEdit(sender As Object, e As BeforeColEditEventArgs) Handles LstItems.BeforeColEdit
        Try
            If LstItems.FilterActive Then Exit Try

            If gbReprintAcInvAfterSplit AndAlso (txtTypeSlip.Text.Trim = "IN" OrElse txtTypeSlip.Text.Trim = "CN") AndAlso LstItems.Splits(0).DisplayColumns.Item("OnAcSett").DataColumn.Text = "Y" Then

            Else
                If TransactionDateLessThanLastMod0001ProccessDate(CDate(LstItems.Splits(0).DisplayColumns.Item("Date").DataColumn.Text).ToString("dd/MM/yyyy")) = True Then
                    e.Cancel = True
                    Exit Try
                End If
            End If

        Catch ex As Exception
            MessageBox.Show("Error: " & ex.Message, cntTitle, MessageBoxButtons.OK, MessageBoxIcon.Error)
        End Try
    End Sub

    Private Sub btnFashionReport_Click(sender As Object, e As EventArgs) Handles btnFashionReport.Click
        Dim InvNum As String
        Try

            'strsql = "select InvHead from " & TempTable & " WHERE chk = 'true' "
            '    InvNum = ExecuteScalar_WOXACT(strsql)

            If txtTypeSlip.Text <> "DO" Then
                FashionReport(LstItems.Splits(0).DisplayColumns.Item("InvHead").DataColumn.Text, "SI", lstMatrix, txtTypeSlip.Text)
            Else
                FashionReport(LstItems.Splits(0).DisplayColumns.Item("DOH_RecId").DataColumn.Text, "DO", lstMatrix, txtTypeSlip.Text)
            End If


        Catch ex As Exception
            MessageBox.Show("Error: " & ex.Message, cntTitle, MessageBoxButtons.OK, MessageBoxIcon.Error)
        End Try
    End Sub

    Private Sub DesignButtons()
        Dim SpcBetBtns As Integer
        Dim NbrButtons As Integer
        Try

            NbrButtons = CInt(IIf((txtTypeSlip.Text.Trim = "IN" Or txtTypeSlip.Text.Trim = "CN" Or txtTypeSlip.Text.Trim = "DO") AndAlso gbFashion AndAlso btnFashionReport.Visible, "6", "5").ToString)

            SpcBetBtns = CInt((Me.Width - ((btnProceed.Left * 2) + (btnProceed.Width * NbrButtons))) / (NbrButtons - 1))
            btnShow.Left = btnProceed.Left + btnProceed.Width + SpcBetBtns

            If gbFashion AndAlso (txtTypeSlip.Text.Trim = "IN" Or txtTypeSlip.Text.Trim = "CN" Or txtTypeSlip.Text.Trim = "DO") Then
                btnFashionReport.Left = btnShow.Left + btnShow.Width + SpcBetBtns
                BtnDirectPrint.Left = btnFashionReport.Left + btnFashionReport.Width + SpcBetBtns
                btnPreview.Left = BtnDirectPrint.Left + BtnDirectPrint.Width + SpcBetBtns
                btnClose.Left = btnPreview.Left + btnPreview.Width + SpcBetBtns - 30
            Else
                BtnDirectPrint.Left = btnShow.Left + btnShow.Width + SpcBetBtns
                btnPreview.Left = BtnDirectPrint.Left + BtnDirectPrint.Width + SpcBetBtns
                btnClose.Left = btnPreview.Left + btnPreview.Width + SpcBetBtns - 30
            End If

        Catch ex As Exception
            MessageBox.Show("Error: " & ex.Message, cntTitle, MessageBoxButtons.OK, MessageBoxIcon.Error)
        End Try

    End Sub

End Class