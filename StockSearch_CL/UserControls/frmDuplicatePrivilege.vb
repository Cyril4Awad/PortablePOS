#Region "Imports"

Imports System.Drawing
Imports System.Windows.Forms
Imports C1.Win.C1TrueDBGrid

#End Region

'Small modal of the User Privilege tab : the users who get a copy of the privileges of the shown user (ticked in the grid)
'Returns DialogResult.OK with TargetUsers set
Public Class frmDuplicatePrivilege
    Inherits Form

#Region "Declarations"

    Private ReadOnly LstTargets As New C1TrueDBGrid
    Private ReadOnly btnCheckAll As New Button
    Private ReadOnly btnUncheckAll As New Button
    Private ReadOnly btnOK As New Button
    Private ReadOnly btnCancel As New Button
    Private ReadOnly dtTargets As New DataTable

    Public ReadOnly Property TargetUsers As New List(Of String)

#End Region

#Region "Control"

    'users : code / display of the users (the source user is not in the list)
    Public Sub New(sourceUser As String, users As DataTable)

        Dim brandBlue As Color = Color.FromArgb(8, 98, 170)

        Me.Text = "Duplicate Privileges"
        Me.FormBorderStyle = FormBorderStyle.FixedDialog
        Me.StartPosition = FormStartPosition.CenterParent
        Me.MaximizeBox = False
        Me.MinimizeBox = False
        Me.ShowInTaskbar = False
        Me.BackColor = Color.FromArgb(227, 239, 255)
        Me.ClientSize = New Size(380, 420)
        Me.Font = New Font("Microsoft Sans Serif", 8.25!)

        Dim lblInfo As New Label With {
            .Text = "Copy the privileges of user " & sourceUser & " to :",
            .Font = New Font("Microsoft Sans Serif", 9.0!, FontStyle.Bold),
            .ForeColor = Color.Navy,
            .BackColor = Color.Transparent,
            .Location = New Point(20, 15),
            .Size = New Size(340, 19)}

        dtTargets.Columns.Add("Select", GetType(Boolean))
        dtTargets.Columns.Add("User Code", GetType(String))
        dtTargets.Columns.Add("User Name", GetType(String))
        For Each row As DataRow In users.Rows
            Dim code As String = row("code").ToString
            If code.Equals(sourceUser, StringComparison.OrdinalIgnoreCase) Then Continue For
            Dim display As String = row("display").ToString
            dtTargets.Rows.Add(False, code, If(display.StartsWith(code & " - "), display.Substring(code.Length + 3), display))
        Next

        LstTargets.Location = New Point(20, 40)
        LstTargets.Size = New Size(340, 300)
        LstTargets.Font = New Font("Microsoft Sans Serif", 8.25!, FontStyle.Bold)
        LstTargets.RowHeight = 18
        LstTargets.AllowColMove = False
        LstTargets.AllowColSelect = False
        LstTargets.AllowRowSizing = RowSizingEnum.None
        LstTargets.EmptyRows = True

        StyleButton(btnCheckAll, "Check All", Color.White, brandBlue)
        btnCheckAll.Location = New Point(20, 348)

        StyleButton(btnUncheckAll, "Uncheck All", Color.White, brandBlue)
        btnUncheckAll.Location = New Point(130, 348)

        StyleButton(btnCancel, "Cancel", Color.White, brandBlue)
        btnCancel.Location = New Point(154, 378)
        btnCancel.DialogResult = DialogResult.Cancel

        StyleButton(btnOK, "Duplicate", brandBlue, Color.White)
        btnOK.Location = New Point(260, 378)
        AddHandler btnOK.Click, AddressOf btnOK_Click
        AddHandler btnCheckAll.Click, AddressOf btnCheckAll_Click
        AddHandler btnUncheckAll.Click, AddressOf btnUncheckAll_Click

        Me.Controls.AddRange({lblInfo, LstTargets, btnCheckAll, btnUncheckAll, btnCancel, btnOK})
        Me.CancelButton = btnCancel

        AddHandler Me.Load, AddressOf frm_Load
        AddHandler LstTargets.Click, AddressOf LstTargets_Click

    End Sub

    Private Sub frm_Load(sender As Object, e As EventArgs)
        Try

            With LstTargets
                .DataSource = dtTargets
                .ExtendRightColumn = True
                .FilterBar = True
                .AllowSort = False
                .AllowUpdate = True
                .AllowAddNew = False
                .AllowDelete = False

                'Rows are white only
                .AlternatingRows = False
                .EvenRowStyle.BackColor = Color.White
                .OddRowStyle.BackColor = Color.White

                .Columns("Select").ValueItems.Presentation = PresentationEnum.CheckBox
                With .Splits(0).DisplayColumns("Select")
                    .HeadingStyle.HorizontalAlignment = AlignHorzEnum.Center
                    .Style.HorizontalAlignment = AlignHorzEnum.Center
                    .Style.BackColor = Color.White
                    .Locked = False
                    .AllowFocus = True
                    .Width = 50
                End With

                With .Splits(0).DisplayColumns("User Code")
                    .HeadingStyle.HorizontalAlignment = AlignHorzEnum.Center
                    .Style.HorizontalAlignment = AlignHorzEnum.Center
                    .Style.BackColor = Color.White
                    .Locked = True
                    .AllowFocus = True
                    .Width = 70
                End With

                With .Splits(0).DisplayColumns("User Name")
                    .HeadingStyle.HorizontalAlignment = AlignHorzEnum.Center
                    .Style.HorizontalAlignment = AlignHorzEnum.Near
                    .Style.BackColor = Color.White
                    .Locked = True
                    .AllowFocus = True
                End With
            End With

        Catch ex As Exception
            MessageBox.Show("Error: " & ex.Message, Me.Text, MessageBoxButtons.OK, MessageBoxIcon.Error)
        End Try
    End Sub

    Private Sub StyleButton(btn As Button, text As String, backColor As Color, foreColor As Color)
        btn.Text = text
        btn.Size = New Size(100, 28)
        btn.FlatStyle = FlatStyle.Flat
        btn.FlatAppearance.BorderColor = Color.FromArgb(8, 98, 170)
        btn.BackColor = backColor
        btn.ForeColor = foreColor
        btn.Font = New Font("Tahoma", 9.0!, If(backColor = Color.White, FontStyle.Regular, FontStyle.Bold))
        btn.Cursor = Cursors.Hand
    End Sub

    Private Sub SetAllTargets(selected As Boolean)

        LstTargets.UpdateData()
        For Each row As DataRow In dtTargets.Rows
            row("Select") = selected
        Next
        LstTargets.Refresh()

    End Sub

    Private Sub btnCheckAll_Click(sender As Object, e As EventArgs)
        SetAllTargets(True)
    End Sub

    Private Sub btnUncheckAll_Click(sender As Object, e As EventArgs)
        SetAllTargets(False)
    End Sub

    'Clicking a user (not only its check box) ticks / unticks it
    Private Sub LstTargets_Click(sender As Object, e As EventArgs)
        Try

            If LstTargets.Col < 0 OrElse LstTargets.Columns(LstTargets.Col).DataField = "Select" Then Exit Try
            Dim current As DataRowView = TryCast(Me.BindingContext(dtTargets).Current, DataRowView)
            If current Is Nothing Then Exit Try
            current("Select") = Not CBool(current("Select"))
            current.EndEdit()
            LstTargets.Refresh()

        Catch ex As Exception

        End Try
    End Sub

    Private Sub btnOK_Click(sender As Object, e As EventArgs)

        LstTargets.UpdateData()

        TargetUsers.Clear()
        For Each row As DataRow In dtTargets.Rows
            If CBool(row("Select")) Then TargetUsers.Add(row("User Code").ToString)
        Next

        If TargetUsers.Count = 0 Then
            MessageBox.Show("Please Choose The Users", Me.Text, MessageBoxButtons.OK, MessageBoxIcon.Exclamation)
            Exit Sub
        End If

        Me.DialogResult = DialogResult.OK

    End Sub

#End Region

End Class
