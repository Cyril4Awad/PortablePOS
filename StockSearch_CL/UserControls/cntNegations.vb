#Region "Imports"

Imports System.Windows.Forms
Imports System.Data.SqlClient
Imports C1.Win.C1TrueDBGrid
Imports ApplicationsDesign.UserNotifications
Imports ApplicationsDesign.Grid

#End Region

'The negations of a user on the action buttons of the portable, per company (tbl_CPP_NegationButton)
'3 grids : the users, the forms the selected user is allowed to use (tbl_CPP_UserPriv), the actions of the selected form (tbl_CPP_NegButtonSteps)
'The actions themselves are added by script (Tables/015)
Public Class cntNegations

#Region "Declarations"

    Private cntTitle As String = "Negations"
    Private dtUsers As DataTable
    Private dtForms As DataTable
    Private dtButtons As DataTable
    Private dvButtons As DataView

    'The user / company the forms and actions are loaded for
    Private shownUser As String = ""
    Private shownComp As String = ""
    Private loading As Boolean

#End Region

#Region "Control"

    Private Sub cnt_Load(sender As Object, e As EventArgs) Handles Me.Load
        Try

            loading = True

            dtForms = New DataTable
            dtButtons = New DataTable

            cmbCompany.DataSource = GetTable(" SELECT companyCode [code], companyCode + ' - ' + companyName [display]
                                                FROM   V_CPP_Companies
                                                ORDER BY companyCode ")
            cmbCompany.ValueMember = "code"
            cmbCompany.DisplayMember = "display"

            dtUsers = GetTable(" SELECT userCode [User Code], userName [User Name]
                                 FROM   V_CPP_Users
                                 WHERE  userActive = 'Y'
                                 ORDER BY userCode ")
            DesignUsersGrid()

            loading = False

            LoadUser()
            CenterGrids()

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

    Private Sub LstUsers_RowColChange(sender As Object, e As RowColChangeEventArgs) Handles LstUsers.RowColChange
        Try

            If loading OrElse SelectedUser() = shownUser Then Exit Try
            LoadUser()

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

    Private Sub cmbCompany_SelectionChangeCommitted(sender As Object, e As EventArgs) Handles cmbCompany.SelectionChangeCommitted
        Try

            If loading Then Exit Try
            LoadUser()

        Catch ex As Exception
            MessageBox.Show("Error: " & ex.Message, cntTitle, MessageBoxButtons.OK, MessageBoxIcon.Error)
        End Try
    End Sub

    'Changes not saved when another user / company is selected : saved or dropped
    Private Sub AskSaveChanges()

        LstButtons.UpdateData()
        If dtButtons Is Nothing OrElse dtButtons.GetChanges Is Nothing Then Exit Sub

        If MessageBox.Show("Save the negations of user " & shownUser & " ?", cntTitle, MessageBoxButtons.YesNo, MessageBoxIcon.Question) = DialogResult.Yes Then
            SaveData()
        End If

    End Sub

    'The forms the selected user is allowed to use (the super user SU : all of them), and the actions of these forms with the negations of the user
    Private Sub LoadUser()

        AskSaveChanges()

        shownUser = SelectedUser()
        shownComp = If(cmbCompany.SelectedValue?.ToString, "")

        Using frmW As New ApplicationsDesign.frmWaiting
            frmW.Show("Get Negations")

            dtForms = New DataTable
            Using daForms As New SqlDataAdapter(" SELECT  CM_RecKey [Step Code] ,
                                                          ISNULL(CM_MenuTitle, CM_RecKey) [Form]
                                                  FROM    dbo.tbl_CPP_ControlizedMenu
                                                          LEFT JOIN dbo.tbl_CPP_UserPriv ON UP_StepCode = CM_RecKey
                                                                                          AND UP_UserCode = @UserCode
                                                  WHERE   CM_MenuActive = 1
                                                          AND @UserCode <> ''
                                                          AND ( @UserCode = 'SU'
                                                                OR ( UP_Allowed = 1 AND ( UP_PrivTill IS NULL OR UP_PrivTill >= CONVERT(DATE, GETDATE()) ) ) )
                                                  ORDER BY CASE LEFT(CM_RecKey, 1) WHEN 'C' THEN 1 WHEN 'S' THEN 2 WHEN 'T' THEN 3 WHEN 'X' THEN 4
                                                                                   WHEN 'P' THEN 5 WHEN 'L' THEN 6 WHEN 'R' THEN 7 ELSE 8 END ,
                                                           CM_RecKey ", clsConfig.SQLConnectionString)
                daForms.SelectCommand.CommandTimeout = 1800
                daForms.SelectCommand.Parameters.Add("@UserCode", SqlDbType.VarChar, 4).Value = shownUser
                daForms.Fill(dtForms)
            End Using

            dtButtons = New DataTable
            Using daButtons As New SqlDataAdapter(" SELECT  NBS_RecID [RecID] ,
                                                            NBS_StepCode [Step Code] ,
                                                            NBS_Button [Button] ,
                                                            NBS_ButtonDesc [Action] ,
                                                            CONVERT(BIT, ISNULL(NB_HasNeg, 0)) [Allowed]
                                                    FROM    dbo.tbl_CPP_NegButtonSteps
                                                            LEFT JOIN dbo.tbl_CPP_NegationButton ON NB_BSRecID = NBS_RecID
                                                                                                  AND NB_UserCode = @UserCode
                                                                                                  AND NB_Comp = @Comp
                                                    WHERE   @UserCode <> ''
                                                    ORDER BY NBS_StepCode , NBS_ButtonDesc ", clsConfig.SQLConnectionString)
                daButtons.SelectCommand.CommandTimeout = 1800
                daButtons.SelectCommand.Parameters.Add("@UserCode", SqlDbType.VarChar, 4).Value = shownUser
                daButtons.SelectCommand.Parameters.Add("@Comp", SqlDbType.VarChar, 2).Value = shownComp
                daButtons.Fill(dtButtons)
            End Using

            dtButtons.AcceptChanges()
            dvButtons = New DataView(dtButtons)

            DesignFormsGrid()
            DesignButtonsGrid()
            ShowFormButtons()

        End Using

    End Sub

    'Rows are white only (no alternating colors, no blue on the editable columns)
    Private Sub WhiteRows(grid As C1TrueDBGrid)
        grid.AlternatingRows = False
        grid.EvenRowStyle.BackColor = Drawing.Color.White
        grid.OddRowStyle.BackColor = Drawing.Color.White
    End Sub

    'selectable : read only, but a click selects the row (the users and forms grids load the next grid on the selected row)
    Private Sub DesignColumn(grid As C1TrueDBGrid, column As String, width As Integer, alignment As AlignHorzEnum, editable As Boolean, Optional selectable As Boolean = False)
        With grid.Splits(0).DisplayColumns(column)
            .HeadingStyle.HorizontalAlignment = AlignHorzEnum.Center
            .Style.HorizontalAlignment = alignment
            .Style.VerticalAlignment = AlignVertEnum.Center
            .Style.BackColor = Drawing.Color.White
            .Locked = Not editable
            .AllowFocus = editable OrElse selectable
            .Width = width
        End With
    End Sub

    Private Sub DesignUsersGrid()

        With LstUsers
            .DataSource = dtUsers
            .ExtendRightColumn = False
            .FilterBar = True
            .AllowSort = False
            .AllowUpdate = False
            .AllowAddNew = False
            .AllowDelete = False
            WhiteRows(LstUsers)
            .MarqueeStyle = MarqueeEnum.HighlightRow
            DesignColumn(LstUsers, "User Code", 70, AlignHorzEnum.Center, False, True)
            DesignColumn(LstUsers, "User Name", 170, AlignHorzEnum.Near, False, True)
            SetGirdHeaders(LstUsers, dtUsers)
        End With

    End Sub

    Private Sub DesignFormsGrid()

        With LstForms
            .DataSource = dtForms
            .ExtendRightColumn = False
            .FilterBar = False
            .AllowSort = False
            .AllowUpdate = False
            .AllowAddNew = False
            .AllowDelete = False
            WhiteRows(LstForms)
            .MarqueeStyle = MarqueeEnum.HighlightRow
            DesignColumn(LstForms, "Step Code", 70, AlignHorzEnum.Center, False, True)
            DesignColumn(LstForms, "Form", 170, AlignHorzEnum.Near, False, True)
        End With

    End Sub

    Private Sub DesignButtonsGrid()

        With LstButtons
            .DataSource = dvButtons
            .ExtendRightColumn = False
            .FilterBar = False
            .AllowSort = False
            .AllowUpdate = True
            .AllowAddNew = False
            .AllowDelete = False
            WhiteRows(LstButtons)

            .Splits(0).DisplayColumns("RecID").Visible = False
            .Splits(0).DisplayColumns("Step Code").Visible = False
            .Splits(0).DisplayColumns("Button").Visible = False

            DesignColumn(LstButtons, "Action", 170, AlignHorzEnum.Near, False)
            .Columns("Allowed").ValueItems.Presentation = PresentationEnum.CheckBox
            DesignColumn(LstButtons, "Allowed", 70, AlignHorzEnum.Center, True)
        End With

    End Sub

    'The actions grid shows the actions of the form selected in the forms grid (the changes of the other forms are kept until Save)
    Private Sub ShowFormButtons()

        If dvButtons Is Nothing Then Exit Sub

        LstButtons.UpdateData()

        Dim current As DataRowView = Nothing
        If dtForms.Rows.Count > 0 Then current = TryCast(Me.BindingContext(dtForms).Current, DataRowView)

        Dim stepCode As String = If(current Is Nothing, "", current("Step Code").ToString)
        dvButtons.RowFilter = "[Step Code] = '" & stepCode.Replace("'", "''") & "'"

    End Sub

    Private Sub LstForms_RowColChange(sender As Object, e As RowColChangeEventArgs) Handles LstForms.RowColChange
        Try

            ShowFormButtons()

        Catch ex As Exception

        End Try
    End Sub

    'The three grids keep the width of their columns and stay in the middle, the company and Save follow them
    Private Sub cnt_Resize(sender As Object, e As EventArgs) Handles Me.Resize
        Try

            CenterGrids()

        Catch ex As Exception

        End Try
    End Sub

    Private Sub CenterGrids()

        Const gap As Integer = 20
        Dim totalWidth As Integer = LstUsers.Width + gap + LstForms.Width + gap + LstButtons.Width
        LstUsers.Left = Math.Max(0, (Me.ClientSize.Width - totalWidth) \ 2)
        LstForms.Left = LstUsers.Right + gap
        LstButtons.Left = LstForms.Right + gap

        lblUsers.Left = LstUsers.Left
        lblForms.Left = LstForms.Left
        lblButtons.Left = LstButtons.Left

        Label1.Left = LstUsers.Left
        cmbCompany.Left = Label1.Right + 5

        btnSave.Left = LstButtons.Right - btnSave.Width

    End Sub

#End Region

#Region "Manipulation"

    Private Sub btnSave_Click(sender As Object, e As EventArgs) Handles btnSave.Click
        Try

            If shownUser = "" OrElse shownComp = "" Then
                ShowNotification(Me.ParentForm, "Please Choose The User And The Company", 3000, 12, DevComponents.DotNetBar.eToastPosition.MiddleCenter)
                Exit Try
            End If

            LstButtons.UpdateData()

            If dtButtons.GetChanges Is Nothing Then
                ShowNotification(Me.ParentForm, "No Changes To Save", 3000, 12, DevComponents.DotNetBar.eToastPosition.MiddleCenter)
                Exit Try
            End If

            If SaveData() Then
                ShowNotification(Me.ParentForm, "Negations Saved", 3000, 12, DevComponents.DotNetBar.eToastPosition.MiddleCenter)
            End If

        Catch ex As Exception
            MessageBox.Show("Error: " & ex.Message, cntTitle, MessageBoxButtons.OK, MessageBoxIcon.Error)
        End Try
    End Sub

    'Saves the changed actions (all the forms) in one transaction (all or nothing)
    Private Function SaveData() As Boolean

        Using cnsql As New SqlConnection(clsConfig.SQLConnectionString)

            Dim trnSql As SqlTransaction = Nothing
            Try

                cnsql.Open()
                trnSql = cnsql.BeginTransaction

                Using cmsql As New SqlCommand(" UPDATE dbo.tbl_CPP_NegationButton
                                                SET    NB_HasNeg = @HasNeg ,
                                                       NB_StepCode = @StepCode ,
                                                       NB_Button = @Button ,
                                                       NB_UStamp = @UStamp ,
                                                       NB_DStamp = GETDATE()
                                                WHERE  NB_UserCode = @UserCode
                                                       AND NB_Comp = @Comp
                                                       AND NB_BSRecID = @BSRecID ;

                                                IF @@ROWCOUNT = 0
                                                    INSERT INTO dbo.tbl_CPP_NegationButton ( NB_UserCode , NB_Comp , NB_BSRecID , NB_StepCode , NB_Button , NB_HasNeg , NB_UStamp , NB_DStamp )
                                                    VALUES ( @UserCode , @Comp , @BSRecID , @StepCode , @Button , @HasNeg , @UStamp , GETDATE() ) ; ", cnsql, trnSql)

                    cmsql.CommandTimeout = 900
                    cmsql.Parameters.Add("@UserCode", SqlDbType.VarChar, 4).Value = shownUser
                    cmsql.Parameters.Add("@Comp", SqlDbType.VarChar, 2).Value = shownComp
                    cmsql.Parameters.Add("@UStamp", SqlDbType.VarChar, 4).Value = Strings.Left(If(clsConfig.SystemUser, ""), 4)
                    cmsql.Parameters.Add("@BSRecID", SqlDbType.Int)
                    cmsql.Parameters.Add("@StepCode", SqlDbType.VarChar, 10)
                    cmsql.Parameters.Add("@Button", SqlDbType.VarChar, 50)
                    cmsql.Parameters.Add("@HasNeg", SqlDbType.Bit)

                    For Each row As DataRow In dtButtons.Rows
                        If row.RowState <> DataRowState.Modified Then Continue For
                        cmsql.Parameters("@BSRecID").Value = row("RecID")
                        cmsql.Parameters("@StepCode").Value = row("Step Code")
                        cmsql.Parameters("@Button").Value = row("Button")
                        cmsql.Parameters("@HasNeg").Value = CBool(row("Allowed"))
                        cmsql.ExecuteNonQuery()
                    Next

                End Using

                trnSql.Commit()
                dtButtons.AcceptChanges()
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
