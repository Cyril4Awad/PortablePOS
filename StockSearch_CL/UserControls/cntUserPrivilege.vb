#Region "Imports"

Imports System.Windows.Forms
Imports System.Data.SqlClient
Imports C1.Win.C1TrueDBGrid
Imports ApplicationsDesign.UserNotifications
Imports ApplicationsDesign.Grid

#End Region

'The privileges of a user on the portable menu items, for the whole database (tbl_CPP_UserPriv)
'Only the menu items visible on this database are listed (CM_MenuActive of tbl_CPP_ControlizedMenu)
Public Class cntUserPrivilege

#Region "Declarations"

    Private cntTitle As String = "User Privilege"
    Private dtUsers As DataTable
    Private dtPriv As DataTable

    'The user whose steps are shown in the grid : the users grid is put back on it when the user cancels a change
    Private shownUser As String = ""
    Private loading As Boolean

#End Region

#Region "Control"

    Private Sub cnt_Load(sender As Object, e As EventArgs) Handles Me.Load
        Try

            dtPriv = New DataTable

            loading = True

            dtUsers = GetTable(" SELECT userCode [User Code], userName [User Name]
                                 FROM   V_CPP_Users
                                 WHERE  userActive = 'Y'
                                 ORDER BY userCode ")
            DesignUsersGrid()

            loading = False

            LoadData()
            CenterGrids()

            Me.ActiveControl = LstUsers

        Catch ex As Exception
            loading = False
            MessageBox.Show("Error: " & ex.Message, cntTitle, MessageBoxButtons.OK, MessageBoxIcon.Error)
        End Try
    End Sub

#End Region

#Region "Grid Data / Design"

    Private Function GetTable(_StrQuery As String) As DataTable
        Dim dt As New DataTable
        Using daSql As New SqlDataAdapter(_StrQuery, clsConfig.SQLConnectionString)
            daSql.SelectCommand.CommandTimeout = 1800
            daSql.Fill(dt)
        End Using
        Return dt
    End Function

    'The user of the selected row of the users grid ("" when none)
    Private Function SelectedUser() As String
        If dtUsers Is Nothing OrElse dtUsers.Rows.Count = 0 Then Return ""
        Dim current As DataRowView = TryCast(Me.BindingContext(dtUsers).Current, DataRowView)
        Return If(current Is Nothing, "", current("User Code").ToString)
    End Function

    Private Sub SelectUserRow(userCode As String)
        If dtUsers Is Nothing OrElse dtUsers.Rows.Count = 0 Then Exit Sub

        loading = True
        Try
            Dim idx As Integer = -1
            For i As Integer = 0 To dtUsers.Rows.Count - 1
                If dtUsers.Rows(i)("User Code").ToString.Equals(userCode, StringComparison.OrdinalIgnoreCase) Then
                    idx = i
                    Exit For
                End If
            Next
            If idx >= 0 Then
                Me.BindingContext(dtUsers).Position = idx
            End If
        Finally
            loading = False
        End Try
    End Sub

    Private Sub LstUsers_RowColChange(sender As Object, e As RowColChangeEventArgs) Handles LstUsers.RowColChange
        Try

            If loading OrElse SelectedUser() = shownUser Then Exit Try

            If Not ConfirmLeave() Then
                SelectUserRow(shownUser)
                Exit Try
            End If

            LoadData()

        Catch ex As Exception
            MessageBox.Show("Error: " & ex.Message, cntTitle, MessageBoxButtons.OK, MessageBoxIcon.Error)
        End Try
    End Sub

    Private Sub LstUsers_FilterChange(sender As Object, e As EventArgs) Handles LstUsers.FilterChange
        Try

            GridFilterChange(LstUsers)

        Catch ex As Exception

        End Try
    End Sub

    'Changes not saved : asks to save them before showing another user / company (False = stay on this one)
    Private Function ConfirmLeave() As Boolean

        LstPriv.UpdateData()
        If dtPriv.GetChanges Is Nothing Then Return True

        Select Case MessageBox.Show("Save the changes of user " & shownUser & " ?", cntTitle, MessageBoxButtons.YesNoCancel, MessageBoxIcon.Question)
            Case DialogResult.Yes
                Return SaveData()
            Case DialogResult.No
                Return True
            Case Else
                Return False
        End Select

    End Function

    'The menu items of this database (active in the controlized menu) with the privilege of the user
    Private Sub LoadData()
        Try

            shownUser = SelectedUser()

            Using frmW As New ApplicationsDesign.frmWaiting
                frmW.Show("Get Privileges")

                dtPriv = New DataTable

                Using daPriv As New SqlDataAdapter(" SELECT  CM_RecKey [Step Code] ,
                                                             ISNULL(CM_MenuTitle, CM_RecKey) [Menu Item] ,
                                                             CONVERT(BIT, ISNULL(UP_Allowed, 0)) [Allowed] ,
                                                             UP_PrivTill [Priv Till]
                                                     FROM    dbo.tbl_CPP_ControlizedMenu
                                                             LEFT JOIN dbo.tbl_CPP_UserPriv ON UP_StepCode = CM_RecKey
                                                                                             AND UP_UserCode = @UserCode
                                                     WHERE   CM_MenuActive = 1
                                                             AND @UserCode <> ''
                                                     ORDER BY CASE LEFT(CM_RecKey, 1) WHEN 'C' THEN 1 WHEN 'S' THEN 2 WHEN 'T' THEN 3 WHEN 'X' THEN 4
                                                                                      WHEN 'P' THEN 5 WHEN 'L' THEN 6 WHEN 'R' THEN 7 ELSE 8 END ,
                                                              CM_RecKey ", clsConfig.SQLConnectionString)
                    daPriv.SelectCommand.CommandTimeout = 1800
                    daPriv.SelectCommand.Parameters.Add("@UserCode", SqlDbType.VarChar, 4).Value = shownUser
                    daPriv.Fill(dtPriv)
                End Using

                dtPriv.AcceptChanges()

                DesignGrid()

            End Using

        Catch ex As Exception
            Throw
        End Try
    End Sub

    Private Sub DesignUsersGrid()

        With LstUsers
            .DataSource = dtUsers
            .ExtendRightColumn = False
            .FilterBar = True
            .FetchRowStyles = True
            .AllowSort = False
            .AllowUpdate = False
            .AllowAddNew = False
            .AllowDelete = False
            .AlternatingRows = False
            .EvenRowStyle.BackColor = Drawing.Color.White
            .OddRowStyle.BackColor = Drawing.Color.White
            .MarqueeStyle = MarqueeEnum.HighlightRow

            With .Splits(0).DisplayColumns("User Code")
                .HeadingStyle.HorizontalAlignment = AlignHorzEnum.Center
                .Style.HorizontalAlignment = AlignHorzEnum.Center
                .Style.VerticalAlignment = AlignVertEnum.Center
                .Style.BackColor = Drawing.Color.White
                .Locked = True
                .AllowFocus = True
                .Width = 70
            End With

            With .Splits(0).DisplayColumns("User Name")
                .HeadingStyle.HorizontalAlignment = AlignHorzEnum.Center
                .Style.HorizontalAlignment = AlignHorzEnum.Near
                .Style.VerticalAlignment = AlignVertEnum.Center
                .Style.BackColor = Drawing.Color.White
                .Locked = True
                .AllowFocus = True
                .Width = 170
            End With

            SetGirdHeaders(LstUsers, dtUsers)
        End With

    End Sub

    Private Sub DesignGrid()
        Try

            With LstPriv
                .DataSource = dtPriv
                .ExtendRightColumn = False
                .FilterBar = True
                .FetchRowStyles = True
                .AllowSort = False
                .AllowUpdate = True
                .AllowAddNew = False
                .AllowDelete = False

                'Rows are white only (no alternating colors, no blue on the editable columns)
                .AlternatingRows = False
                .EvenRowStyle.BackColor = Drawing.Color.White
                .OddRowStyle.BackColor = Drawing.Color.White

                With .Splits(0).DisplayColumns("Step Code")
                    .HeadingStyle.HorizontalAlignment = AlignHorzEnum.Center
                    .Style.HorizontalAlignment = AlignHorzEnum.Center
                    .Style.VerticalAlignment = AlignVertEnum.Center
                    .Style.BackColor = Drawing.Color.White
                    .Locked = True
                    .AllowFocus = False
                    .Width = 80
                End With

                With .Splits(0).DisplayColumns("Menu Item")
                    .HeadingStyle.HorizontalAlignment = AlignHorzEnum.Center
                    .Style.HorizontalAlignment = AlignHorzEnum.Near
                    .Style.VerticalAlignment = AlignVertEnum.Center
                    .Style.BackColor = Drawing.Color.White
                    .Locked = True
                    .AllowFocus = False
                    .Width = 250
                End With

                .Columns("Allowed").ValueItems.Presentation = PresentationEnum.CheckBox
                With .Splits(0).DisplayColumns("Allowed")
                    .HeadingStyle.HorizontalAlignment = AlignHorzEnum.Center
                    .Style.HorizontalAlignment = AlignHorzEnum.Center
                    .Style.VerticalAlignment = AlignVertEnum.Center
                    .Style.BackColor = Drawing.Color.White
                    .Locked = False
                    .AllowFocus = True
                    .Width = 80
                End With

                'Empty = no end date
                .Columns("Priv Till").NumberFormat = "dd/MM/yyyy"
                .Columns("Priv Till").EditMask = "99/99/9999"
                .Columns("Priv Till").EditMaskUpdate = True
                With .Splits(0).DisplayColumns("Priv Till")
                    .HeadingStyle.HorizontalAlignment = AlignHorzEnum.Center
                    .Style.HorizontalAlignment = AlignHorzEnum.Center
                    .Style.VerticalAlignment = AlignVertEnum.Center
                    .Style.BackColor = Drawing.Color.White
                    .Locked = False
                    .AllowFocus = True
                    .Width = 100
                End With

                SetGirdHeaders(LstPriv, dtPriv)

            End With

        Catch ex As Exception
            MessageBox.Show("Error: " & ex.Message, cntTitle, MessageBoxButtons.OK, MessageBoxIcon.Error)
        End Try

    End Sub

    Private Sub LstPriv_FilterChange(sender As Object, e As EventArgs) Handles LstPriv.FilterChange
        Try

            GridFilterChange(LstPriv)

        Catch ex As Exception

        End Try
    End Sub

    'The two grids keep the width of their columns and stay in the middle, the buttons follow the steps grid
    Private Sub cnt_Resize(sender As Object, e As EventArgs) Handles Me.Resize
        Try

            CenterGrids()

        Catch ex As Exception

        End Try
    End Sub

    Private Sub CenterGrids()

        Const gap As Integer = 20
        Dim totalWidth As Integer = LstUsers.Width + gap + LstPriv.Width
        LstUsers.Left = Math.Max(0, (Me.ClientSize.Width - totalWidth) \ 2)
        LstPriv.Left = LstUsers.Right + gap

        lblUsers.Left = LstUsers.Left
        lblSteps.Left = LstPriv.Left

        btnAllowAll.Left = LstPriv.Left
        btnDenyAll.Left = btnAllowAll.Right + 6
        btnUnlimited.Left = btnDenyAll.Right + 6
        btnDuplicate.Left = btnUnlimited.Right + 6
        btnSave.Left = LstPriv.Right - btnSave.Width

    End Sub

    'code / display for the duplicate dialog (same shape as the old user combo)
    Private Function UsersForDuplicate() As DataTable
        Dim dt As New DataTable
        dt.Columns.Add("code", GetType(String))
        dt.Columns.Add("display", GetType(String))
        For Each row As DataRow In dtUsers.Rows
            Dim code As String = row("User Code").ToString()
            dt.Rows.Add(code, code & " - " & row("User Name").ToString())
        Next
        Return dt
    End Function

#End Region

#Region "Manipulation"

    'Priv Till of the current row back to empty (no end date)
    Private Sub ClearPrivTill()

        If dtPriv.Rows.Count = 0 Then Exit Sub

        Dim current As DataRowView = TryCast(Me.BindingContext(dtPriv).Current, DataRowView)
        If current Is Nothing OrElse IsDBNull(current("Priv Till")) Then Exit Sub

        current("Priv Till") = DBNull.Value
        current.EndEdit()
        LstPriv.Refresh()

    End Sub

    Private Sub btnUnlimited_Click(sender As Object, e As EventArgs) Handles btnUnlimited.Click
        Try

            LstPriv.UpdateData()
            ClearPrivTill()

        Catch ex As Exception
            MessageBox.Show("Error: " & ex.Message, cntTitle, MessageBoxButtons.OK, MessageBoxIcon.Error)
        End Try
    End Sub

    'Delete / Backspace on the Priv Till cell (not typing in it) : unlimited
    Private Sub LstPriv_KeyDown(sender As Object, e As KeyEventArgs) Handles LstPriv.KeyDown
        Try

            If e.KeyCode <> Keys.Delete AndAlso e.KeyCode <> Keys.Back Then Exit Try
            If LstPriv.EditActive OrElse LstPriv.Col < 0 Then Exit Try
            If LstPriv.Columns(LstPriv.Col).DataField <> "Priv Till" Then Exit Try

            ClearPrivTill()
            e.Handled = True

        Catch ex As Exception

        End Try
    End Sub

    'The date erased while typing in the cell : unlimited (an empty text cannot be converted to a date)
    Private Sub LstPriv_BeforeColUpdate(sender As Object, e As BeforeColUpdateEventArgs) Handles LstPriv.BeforeColUpdate
        Try

            If LstPriv.Columns(e.ColIndex).DataField <> "Priv Till" Then Exit Try
            If LstPriv.Columns(e.ColIndex).Text.Any(AddressOf Char.IsDigit) Then Exit Try

            e.Cancel = True
            Me.BeginInvoke(New MethodInvoker(Sub() ClearPrivTill()))

        Catch ex As Exception

        End Try
    End Sub

    Private Sub btnAllowAll_Click(sender As Object, e As EventArgs) Handles btnAllowAll.Click, btnDenyAll.Click
        Try

            LstPriv.UpdateData()

            Dim allowed As Boolean = sender Is btnAllowAll
            For Each row As DataRow In dtPriv.Rows
                If CBool(row("Allowed")) <> allowed Then row("Allowed") = allowed
            Next

        Catch ex As Exception
            MessageBox.Show("Error: " & ex.Message, cntTitle, MessageBoxButtons.OK, MessageBoxIcon.Error)
        End Try
    End Sub

    Private Sub btnSave_Click(sender As Object, e As EventArgs) Handles btnSave.Click
        Try

            If shownUser = "" Then
                ShowNotification(Me.ParentForm, "Please Choose The User", 3000, 12, DevComponents.DotNetBar.eToastPosition.MiddleCenter)
                Me.ActiveControl = LstUsers
                Exit Try
            End If

            LstPriv.UpdateData()

            If dtPriv.GetChanges Is Nothing Then
                ShowNotification(Me.ParentForm, "No Changes To Save", 3000, 12, DevComponents.DotNetBar.eToastPosition.MiddleCenter)
                Exit Try
            End If

            If SaveData() Then
                LoadData()
                ShowNotification(Me.ParentForm, "Privileges Saved", 3000, 12, DevComponents.DotNetBar.eToastPosition.MiddleCenter)
            End If

        Catch ex As Exception
            MessageBox.Show("Error: " & ex.Message, cntTitle, MessageBoxButtons.OK, MessageBoxIcon.Error)
        End Try
    End Sub

    'Copies the saved privileges of the shown user to another user (the privileges of that user are replaced)
    Private Sub btnDuplicate_Click(sender As Object, e As EventArgs) Handles btnDuplicate.Click
        Try

            If shownUser = "" Then
                ShowNotification(Me.ParentForm, "Please Choose The User", 3000, 12, DevComponents.DotNetBar.eToastPosition.MiddleCenter)
                Me.ActiveControl = LstUsers
                Exit Try
            End If

            'The saved privileges are copied : the changes on the screen are saved first
            LstPriv.UpdateData()
            If dtPriv.GetChanges IsNot Nothing Then
                If MessageBox.Show("Save the changes of user " & shownUser & " before duplicating ?", cntTitle, MessageBoxButtons.YesNo, MessageBoxIcon.Question) <> DialogResult.Yes Then Exit Try
                If Not SaveData() Then Exit Try
                LoadData()
            End If

            If CInt(modCom.SQL_ExecuteScalar(" SELECT COUNT(*) FROM dbo.tbl_CPP_UserPriv WHERE UP_UserCode = '" & shownUser.Replace("'", "''") & "' ")) = 0 Then
                MessageBox.Show("User " & shownUser & " has no privileges to duplicate", cntTitle, MessageBoxButtons.OK, MessageBoxIcon.Exclamation)
                Exit Try
            End If

            Dim targetUsers As List(Of String)
            Using frmDup As New frmDuplicatePrivilege(shownUser, UsersForDuplicate())
                If frmDup.ShowDialog(Me.ParentForm) <> DialogResult.OK Then Exit Try
                targetUsers = frmDup.TargetUsers
            End Using

            'The ticked users who already have privileges : they are replaced, asked once for all of them
            Dim withPrivileges As New List(Of String)
            For Each targetUser As String In targetUsers
                If CInt(modCom.SQL_ExecuteScalar(" SELECT COUNT(*) FROM dbo.tbl_CPP_UserPriv WHERE UP_UserCode = '" & targetUser.Replace("'", "''") & "' ")) > 0 Then
                    withPrivileges.Add(targetUser)
                End If
            Next

            If withPrivileges.Count > 0 _
                AndAlso MessageBox.Show("These users already have privileges, they will be replaced by the privileges of user " & shownUser & " :" & vbCrLf &
                                        String.Join(", ", withPrivileges) & vbCrLf & vbCrLf & "Continue ?",
                                        cntTitle, MessageBoxButtons.YesNo, MessageBoxIcon.Question) <> DialogResult.Yes Then
                Exit Try
            End If

            DuplicateData(targetUsers)

            ShowNotification(Me.ParentForm, "Privileges Of " & shownUser & " Copied To " & targetUsers.Count & " User(s)", 3000, 12, DevComponents.DotNetBar.eToastPosition.MiddleCenter)

        Catch ex As Exception
            MessageBox.Show("Error: " & ex.Message, cntTitle, MessageBoxButtons.OK, MessageBoxIcon.Error)
        End Try
    End Sub

    'The rows of the target users are replaced by the rows of the shown user, all the users in one transaction
    Private Sub DuplicateData(targetUsers As List(Of String))

        Using cnsql As New SqlConnection(clsConfig.SQLConnectionString)

            cnsql.Open()

            Using trnSql As SqlTransaction = cnsql.BeginTransaction

                Using cmsql As New SqlCommand(" DELETE FROM dbo.tbl_CPP_UserPriv WHERE UP_UserCode = @TargetUser ;

                                                INSERT INTO dbo.tbl_CPP_UserPriv ( UP_UserCode , UP_StepCode , UP_PrivTill , UP_Allowed , UP_UStamp , UP_DStamp )
                                                SELECT  @TargetUser , UP_StepCode , UP_PrivTill , UP_Allowed , @UStamp , GETDATE()
                                                FROM    dbo.tbl_CPP_UserPriv
                                                WHERE   UP_UserCode = @SourceUser ; ", cnsql, trnSql)

                    cmsql.CommandTimeout = 900
                    cmsql.Parameters.Add("@SourceUser", SqlDbType.VarChar, 4).Value = shownUser
                    cmsql.Parameters.Add("@TargetUser", SqlDbType.VarChar, 4)
                    cmsql.Parameters.Add("@UStamp", SqlDbType.VarChar, 4).Value = Strings.Left(If(clsConfig.SystemUser, ""), 4)

                    For Each targetUser As String In targetUsers
                        cmsql.Parameters("@TargetUser").Value = targetUser
                        cmsql.ExecuteNonQuery()
                    Next

                End Using

                trnSql.Commit()

            End Using

        End Using

    End Sub

    'Saves the changed rows of the grid in one transaction (all or nothing)
    Private Function SaveData() As Boolean

        Using cnsql As New SqlConnection(clsConfig.SQLConnectionString)

            Dim trnSql As SqlTransaction = Nothing
            Try

                cnsql.Open()
                trnSql = cnsql.BeginTransaction

                Using cmsql As New SqlCommand(" UPDATE dbo.tbl_CPP_UserPriv
                                                SET    UP_Allowed = @Allowed ,
                                                       UP_PrivTill = @PrivTill ,
                                                       UP_UStamp = @UStamp ,
                                                       UP_DStamp = GETDATE()
                                                WHERE  UP_UserCode = @UserCode
                                                       AND UP_StepCode = @StepCode ;

                                                IF @@ROWCOUNT = 0
                                                    INSERT INTO dbo.tbl_CPP_UserPriv ( UP_UserCode , UP_StepCode , UP_PrivTill , UP_Allowed , UP_UStamp , UP_DStamp )
                                                    VALUES ( @UserCode , @StepCode , @PrivTill , @Allowed , @UStamp , GETDATE() ) ; ", cnsql, trnSql)

                    cmsql.CommandTimeout = 900
                    cmsql.Parameters.Add("@UserCode", SqlDbType.VarChar, 4).Value = shownUser
                    cmsql.Parameters.Add("@UStamp", SqlDbType.VarChar, 4).Value = Strings.Left(If(clsConfig.SystemUser, ""), 4)
                    cmsql.Parameters.Add("@StepCode", SqlDbType.VarChar, 10)
                    cmsql.Parameters.Add("@Allowed", SqlDbType.Bit)
                    cmsql.Parameters.Add("@PrivTill", SqlDbType.DateTime)

                    For Each row As DataRow In dtPriv.Rows
                        If row.RowState <> DataRowState.Modified Then Continue For
                        cmsql.Parameters("@StepCode").Value = row("Step Code")
                        cmsql.Parameters("@Allowed").Value = CBool(row("Allowed"))
                        cmsql.Parameters("@PrivTill").Value = If(IsDBNull(row("Priv Till")), CObj(DBNull.Value), CDate(row("Priv Till")).Date)
                        cmsql.ExecuteNonQuery()
                    Next

                End Using

                trnSql.Commit()
                dtPriv.AcceptChanges()
                Return True

            Catch ex As Exception
                Try
                    trnSql?.Rollback()
                Catch exTRN As Exception
                End Try
                MessageBox.Show("Error: " & ex.Message, cntTitle, MessageBoxButtons.OK, MessageBoxIcon.Error)
                Return False
            End Try

        End Using

    End Function

#End Region

End Class
