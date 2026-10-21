#Region "Imports"

Imports System.Windows.Forms
Imports System.Data.SqlClient
Imports C1.Win.C1TrueDBGrid
Imports ApplicationsDesign.UserNotifications
Imports ApplicationsDesign.Grid

#End Region

Public Class cntUploadsHistory

#Region "Declarations"

    Private cntTitle As String = "Uploads History"
    Private dtHist As DataTable

#End Region

#Region "Control"

    Private Sub cnt_Load(sender As Object, e As EventArgs) Handles Me.Load
        Try

            txtStartDate.ErrorInfo.ErrorMessageCaption = cntTitle
            txtEndDate.ErrorInfo.ErrorMessageCaption = cntTitle

            Me.ActiveControl = txtStartDate

            dtHist = New DataTable
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

                'strsql = " SELECT  CONCAT (UPLH_LocCode, ' - ', CONVERT(VARCHAR,UPLH_RecID) ) [No] ,
                '                   UPLH_UserCode [User] ,
                '                   STK_REF [Reference] ,
                '                   STK_DESC [Description] ,
                '                   UPLD_Qty [Qty] ,
                '                   UPLH_Remarks [Remarks] ,
                '                   UPLH_Dstmp [Created On] ,
                '                   CASE WHEN UPLH_Used = 0 THEN 'False'
                '                        ELSE 'True'
                '                   END [Used] ,
                '                   UPLH_UsedStamp [Used Stamp]
                '           FROM    dbo.tbl_UploadsHeader
                '                   INNER JOIN dbo.tbl_UploadsDetails ON UPLH_LocCode = UPLD_LocCode
                '                                                        AND UPLH_RecID = UPLD_UPLHRecID
                '                   INNER JOIN dbo.STK04_STKFIL ON STK_RECID = UPLD_ItemRecID 
                '           WHERE "

                'If _Empty Then
                '    strsql &= " 1=5 "
                'Else
                '    strsql &= " UPLH_LocCode = '" & clsConfig.LocationCode & "' "
                '    strsql &= " AND CONVERT(DATE,UPLH_Dstmp) BETWEEN 
                '          '" & CDate(txtStartDate.Text).ToString("MM/dd/yyyy") & "' AND '" & CDate(txtEndDate.Text).ToString("MM/dd/yyyy") & "' "
                'End If
                'strsql &= " ORDER BY UPLH_RecID , UPLD_LineNo "

                dtHist.Rows.Clear()
                Using daHist As New SqlDataAdapter(strsql, clsConfig.SQLConnectionString)
                    daHist.SelectCommand.CommandTimeout = 1800
                    daHist.Fill(dtHist)
                End Using

                DesignGrid()

            End Using

        Catch ex As Exception
            Throw
        End Try
    End Sub

    Private Sub DesignGrid()
        Try

            With LstHist
                .DataSource = dtHist
                .ExtendRightColumn = True
                .FilterBar = True
                .ColumnFooters = True
                .FetchRowStyles = True
                .AllowSort = False
                .AllowUpdate = False
                .AllowAddNew = False
                .AllowDelete = False

                With .Splits(0).DisplayColumns("No")
                    .HeadingStyle.HorizontalAlignment = AlignHorzEnum.Center
                    .Style.HorizontalAlignment = AlignHorzEnum.Center
                    .Style.VerticalAlignment = AlignVertEnum.Center
                    .Merge = True
                    .Locked = True
                    .AutoSize()
                End With

                With .Splits(0).DisplayColumns("User")
                    .HeadingStyle.HorizontalAlignment = AlignHorzEnum.Center
                    .Style.HorizontalAlignment = AlignHorzEnum.Center
                    .Style.VerticalAlignment = AlignVertEnum.Center
                    .Merge = True
                    .Locked = True
                    .AutoSize()
                End With

                With .Splits(0).DisplayColumns("Reference")
                    .HeadingStyle.HorizontalAlignment = AlignHorzEnum.Center
                    .Style.HorizontalAlignment = AlignHorzEnum.Center
                    .Style.VerticalAlignment = AlignVertEnum.Center
                    .Locked = True
                    .AutoSize()
                End With

                With .Splits(0).DisplayColumns("Description")
                    .HeadingStyle.HorizontalAlignment = AlignHorzEnum.Center
                    .Style.HorizontalAlignment = AlignHorzEnum.Near
                    .Style.VerticalAlignment = AlignVertEnum.Center
                    .Locked = True
                    .Width = Width / 2
                End With

                With .Splits(0).DisplayColumns("Qty")
                    .HeadingStyle.HorizontalAlignment = AlignHorzEnum.Center
                    .Style.HorizontalAlignment = AlignHorzEnum.Center
                    .Style.VerticalAlignment = AlignVertEnum.Center
                    .Locked = True
                    .DataColumn.NumberFormat = "N"
                    .AutoSize()
                End With

                With .Splits(0).DisplayColumns("Remarks")
                    .HeadingStyle.HorizontalAlignment = AlignHorzEnum.Center
                    .Style.HorizontalAlignment = AlignHorzEnum.Center
                    .Style.VerticalAlignment = AlignVertEnum.Center
                    .Locked = True
                    .Merge = True
                    .AutoSize()
                End With

                With .Splits(0).DisplayColumns("Created On")
                    .HeadingStyle.HorizontalAlignment = AlignHorzEnum.Center
                    .Style.HorizontalAlignment = AlignHorzEnum.Center
                    .Style.VerticalAlignment = AlignVertEnum.Center
                    .DataColumn.NumberFormat = "dd/MM/yyyy HH:mm tt"
                    .Locked = True
                    .Merge = True
                    .AutoSize()
                End With

                With .Splits(0).DisplayColumns("Used")
                    .HeadingStyle.HorizontalAlignment = AlignHorzEnum.Center
                    .Style.HorizontalAlignment = AlignHorzEnum.Center
                    .Style.VerticalAlignment = AlignVertEnum.Center
                    .DataColumn.ValueItems.Presentation = PresentationEnum.CheckBox
                    .Locked = True
                    .Merge = True
                    .AutoSize()
                End With

                With .Splits(0).DisplayColumns("Used Stamp")
                    .HeadingStyle.HorizontalAlignment = AlignHorzEnum.Center
                    .Style.HorizontalAlignment = AlignHorzEnum.Center
                    .Style.VerticalAlignment = AlignVertEnum.Center
                    .DataColumn.NumberFormat = "dd/MM/yyyy HH:mm tt"
                    .Locked = True
                    .Merge = True
                    .AutoSize()
                End With

                SetGirdHeaders(LstHist, dtHist)

            End With

        Catch ex As Exception
            MessageBox.Show("Error: " & ex.Message, cntTitle, MessageBoxButtons.OK, MessageBoxIcon.Error)
        End Try

    End Sub

    Private Sub LstHist_FilterChange(sender As Object, e As EventArgs) Handles LstHist.FilterChange
        Try

            GridFilterChange(LstHist)

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
