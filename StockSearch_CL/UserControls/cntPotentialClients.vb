#Region "Imports"

Imports System.Windows.Forms
Imports System.Data.SqlClient
Imports C1.Win.C1TrueDBGrid
Imports ApplicationsDesign.UserNotifications
Imports ApplicationsDesign.Grid

#End Region

Public Class cntPotentialClients

#Region "Declarations"

    Private cntTitle As String = "Potential Clients"
    Private dtPC As DataTable

#End Region

#Region "Control"

    Private Sub cnt_Load(sender As Object, e As EventArgs) Handles Me.Load
        Try

            txtStartDate.ErrorInfo.ErrorMessageCaption = cntTitle
            txtEndDate.ErrorInfo.ErrorMessageCaption = cntTitle

            Me.ActiveControl = txtStartDate

            dtPC = New DataTable
            LoadData(True)

        Catch ex As Exception

        End Try
    End Sub

#End Region

#Region "Grid Data / Design"

    Private Sub btnLoad_Click(sender As Object, e As EventArgs) Handles btnLoad.Click
        Try

            If txtStartDate.Value Is DBNull.Value Then
                ShowNotification(Me.ParentForm, "Please Fill Start Date", 3000, 12, DevComponents.DotNetBar.eToastPosition.MiddleCenter)
                Me.ActiveControl = txtStartDate
                Exit Try
            End If

            If txtEndDate.Value Is DBNull.Value Then
                ShowNotification(Me.ParentForm, "Please Fill End Date", 3000, 12, DevComponents.DotNetBar.eToastPosition.MiddleCenter)
                Me.ActiveControl = txtEndDate
                Exit Try
            End If

            LoadData(False)

        Catch ex As Exception

        End Try
    End Sub

    Private Sub LoadData(_Empty As Boolean)
        Dim strsql As String
        Try

            Using frmW As New ApplicationsDesign.frmWaiting
                frmW.Show("Get Logging")

                strsql = " SELECT  PC_LocCode [Location],
                                   PC_ClientName [Name],
                                   PC_Business [Business],
                                   PC_Contact [Contact],
                                   PC_PhoneMobile [Phone / Mobile],
                                   PC_Address [Address],
                                   PC_Remarks [Remarks],
                                   PC_UserStamp [User Code],
                                   PC_DateStamp [Date Stamp]
                           FROM    dbo.tbl_CPP_PotentialClients                         
                           WHERE "

                If _Empty Then
                    strsql &= " 1=5 "
                Else
                    strsql &= " CONVERT(DATE,PC_DateStamp) BETWEEN 
                          '" & CDate(txtStartDate.Text).ToString("MM/dd/yyyy") & "' AND '" & CDate(txtEndDate.Text).ToString("MM/dd/yyyy") & "' "
                End If
                strsql &= " ORDER BY PC_DateStamp DESC  "

                dtPC.Rows.Clear()
                Using daMI As New SqlDataAdapter(strsql, clsConfig.SQLConnectionString)
                    daMI.SelectCommand.CommandTimeout = 1800
                    daMI.Fill(dtPC)
                End Using

                DesignGrid()

            End Using

        Catch ex As Exception
            Throw
        End Try
    End Sub

    Private Sub DesignGrid()
        Try

            With LstPC
                .DataSource = dtPC
                .ExtendRightColumn = True
                .FilterBar = True
                .ColumnFooters = True
                .FetchRowStyles = True
                .AllowSort = False
                .AllowUpdate = False
                .AllowAddNew = False
                .AllowDelete = False

                With .Splits(0).DisplayColumns("Location")
                    .HeadingStyle.HorizontalAlignment = AlignHorzEnum.Center
                    .Style.HorizontalAlignment = AlignHorzEnum.Near
                    .Style.VerticalAlignment = AlignVertEnum.Center
                    .Locked = True
                    .Width = 150
                End With

                With .Splits(0).DisplayColumns("Name")
                    .HeadingStyle.HorizontalAlignment = AlignHorzEnum.Center
                    .Style.HorizontalAlignment = AlignHorzEnum.Near
                    .Style.VerticalAlignment = AlignVertEnum.Center
                    .Locked = True
                    .Width = Width / 3
                End With

                With .Splits(0).DisplayColumns("Business")
                    .HeadingStyle.HorizontalAlignment = AlignHorzEnum.Center
                    .Style.HorizontalAlignment = AlignHorzEnum.Near
                    .Style.VerticalAlignment = AlignVertEnum.Center
                    .Locked = True
                    .AutoSize()
                End With

                With .Splits(0).DisplayColumns("Contact")
                    .HeadingStyle.HorizontalAlignment = AlignHorzEnum.Center
                    .Style.HorizontalAlignment = AlignHorzEnum.Near
                    .Style.VerticalAlignment = AlignVertEnum.Center
                    .Locked = True
                    .AutoSize()
                End With

                With .Splits(0).DisplayColumns("Phone / Mobile")
                    .HeadingStyle.HorizontalAlignment = AlignHorzEnum.Center
                    .Style.HorizontalAlignment = AlignHorzEnum.Near
                    .Style.VerticalAlignment = AlignVertEnum.Center
                    .Locked = True
                    .AutoSize()
                End With

                With .Splits(0).DisplayColumns("Address")
                    .HeadingStyle.HorizontalAlignment = AlignHorzEnum.Center
                    .Style.HorizontalAlignment = AlignHorzEnum.Near
                    .Style.VerticalAlignment = AlignVertEnum.Center
                    .Locked = True
                    .AutoSize()
                End With

                With .Splits(0).DisplayColumns("Remarks")
                    .HeadingStyle.HorizontalAlignment = AlignHorzEnum.Center
                    .Style.HorizontalAlignment = AlignHorzEnum.Near
                    .Style.VerticalAlignment = AlignVertEnum.Center
                    .Locked = True
                    .AutoSize()
                End With

                With .Splits(0).DisplayColumns("User Code")
                    .HeadingStyle.HorizontalAlignment = AlignHorzEnum.Center
                    .Style.HorizontalAlignment = AlignHorzEnum.Near
                    .Style.VerticalAlignment = AlignVertEnum.Center
                    .Locked = True
                    .AutoSize()
                End With

                With .Splits(0).DisplayColumns("Date Stamp")
                    .HeadingStyle.HorizontalAlignment = AlignHorzEnum.Center
                    .Style.HorizontalAlignment = AlignHorzEnum.Center
                    .Style.VerticalAlignment = AlignVertEnum.Center
                    .DataColumn.NumberFormat = "dd/MM/yyyy HH:mm tt"
                    .Locked = True
                    .Merge = True
                    .AutoSize()
                End With

                SetGirdHeaders(LstPC, dtPC)

            End With

        Catch ex As Exception
            MessageBox.Show("Error: " & ex.Message, cntTitle, MessageBoxButtons.OK, MessageBoxIcon.Error)
        End Try

    End Sub

    Private Sub LstPC_FilterChange(sender As Object, e As EventArgs) Handles LstPC.FilterChange
        Try

            GridFilterChange(LstPC)

        Catch ex As Exception

        End Try
    End Sub

#End Region

#Region "Manipulation"

    Private Sub txtStrEndDate_Validating(ByVal sender As Object, ByVal e As System.ComponentModel.CancelEventArgs) Handles txtStartDate.Validating, txtEndDate.Validating
        Try

            If (CType(sender, C1.Win.C1Input.C1DateEdit).Value) Is DBNull.Value Then Exit Sub

            If Not txtStartDate.Value Is DBNull.Value And Not txtEndDate.Value Is DBNull.Value Then
                If CDate(txtStartDate.Value) > CDate(txtEndDate.Value) Then
                    MessageBox.Show("Starting Date should be Less than End Date", cntTitle, MessageBoxButtons.OK, MessageBoxIcon.Exclamation)
                    e.Cancel = True
                    Exit Sub
                End If
            End If

            If CDate(CType(sender, C1.Win.C1Input.C1DateEdit).Value) < CDate("01/01/1800") Then
                MessageBox.Show("Invalid Date", cntTitle, MessageBoxButtons.OK, MessageBoxIcon.Exclamation)
                e.Cancel = True
                Exit Sub
            End If

        Catch ex As Exception
        End Try
    End Sub

#End Region

End Class
