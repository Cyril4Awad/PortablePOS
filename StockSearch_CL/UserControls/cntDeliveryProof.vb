#Region "Imports"

Imports System.Windows.Forms
Imports System.Data.SqlClient
Imports System.IO
Imports C1.Win.C1TrueDBGrid
Imports ApplicationsDesign.UserNotifications
Imports ApplicationsDesign.Grid

#End Region

'The Delivery Proofs saved from the portable (Clients > Delivery Proof, tbl_CPP_DeliveryProof) : view only
'The photo is not loaded with the grid (it can be big), it is read when its View button is pressed
Public Class cntDeliveryProof

#Region "Declarations"

    Private cntTitle As String = "Delivery Proof"
    Private dtDP As DataTable

#End Region

#Region "Control"

    Private Sub cnt_Load(sender As Object, e As EventArgs) Handles Me.Load
        Try

            txtStartDate.ErrorInfo.ErrorMessageCaption = cntTitle
            txtEndDate.ErrorInfo.ErrorMessageCaption = cntTitle

            Me.ActiveControl = txtStartDate

            dtDP = New DataTable
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
            MessageBox.Show("Error: " & ex.Message, cntTitle, MessageBoxButtons.OK, MessageBoxIcon.Error)
        End Try
    End Sub

    Private Sub LoadData(_Empty As Boolean)
        Dim strsql As String
        Try

            Using frmW As New ApplicationsDesign.frmWaiting
                frmW.Show("Get Delivery Proofs")

                'The client is read from Stk04_Clients (not V_CPP_Clients) : a proof stays visible when its client is no longer active
                strsql = " SELECT  DP_LocCode [Location],
                                   DP_RecID [RecID],
                                   ISNULL(CL_Name, CONCAT('Client #', DP_CLRecID)) [Client Name],
                                   ISNULL(Fo_phone1, '') [Phone],
                                   CASE WHEN DP_Photo IS NULL THEN '' ELSE 'View' END [Photo],
                                   DP_Remarks [Remarks],
                                   DP_UserStamp [User Code],
                                   DP_DateStamp [Date Stamp]
                           FROM    dbo.tbl_CPP_DeliveryProof
                                   LEFT JOIN dbo.Stk04_Clients ON Stk04_Clients.CL_RecID = DP_CLRecID
                                   LEFT JOIN dbo.Com_Folio ON Com_Folio.Fo_serno = Stk04_Clients.CL_FolioNo
                                                              AND Com_Folio.Fo_subno = Stk04_Clients.CL_FolioSubNo
                           WHERE "

                If _Empty Then
                    strsql &= " 1=5 "
                Else
                    strsql &= " CONVERT(DATE,DP_DateStamp) BETWEEN @StartDate AND @EndDate "
                End If
                strsql &= " ORDER BY DP_DateStamp DESC  "

                dtDP.Rows.Clear()
                Using daDP As New SqlDataAdapter(strsql, clsConfig.SQLConnectionString)
                    daDP.SelectCommand.CommandTimeout = 1800
                    If Not _Empty Then
                        daDP.SelectCommand.Parameters.Add("@StartDate", SqlDbType.Date).Value = CDate(txtStartDate.Value).Date
                        daDP.SelectCommand.Parameters.Add("@EndDate", SqlDbType.Date).Value = CDate(txtEndDate.Value).Date
                    End If
                    daDP.Fill(dtDP)
                End Using

                DesignGrid()

            End Using

        Catch ex As Exception
            Throw
        End Try
    End Sub

    Private Sub DesignGrid()
        Try

            With LstDP
                .DataSource = dtDP
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
                    .Width = 80
                End With

                'The key of the row with Location, used to read the photo
                .Splits(0).DisplayColumns("RecID").Visible = False

                With .Splits(0).DisplayColumns("Client Name")
                    .HeadingStyle.HorizontalAlignment = AlignHorzEnum.Center
                    .Style.HorizontalAlignment = AlignHorzEnum.Near
                    .Style.VerticalAlignment = AlignVertEnum.Center
                    .Locked = True
                    .Width = Width / 4
                End With

                With .Splits(0).DisplayColumns("Phone")
                    .HeadingStyle.HorizontalAlignment = AlignHorzEnum.Center
                    .Style.HorizontalAlignment = AlignHorzEnum.Near
                    .Style.VerticalAlignment = AlignVertEnum.Center
                    .Locked = True
                    .Width = 120
                End With

                'A button in the cell when the proof has a photo : opens it
                With .Splits(0).DisplayColumns("Photo")
                    .HeadingStyle.HorizontalAlignment = AlignHorzEnum.Center
                    .Style.HorizontalAlignment = AlignHorzEnum.Center
                    .Style.VerticalAlignment = AlignVertEnum.Center
                    .Style.ForeColor = Drawing.Color.FromArgb(8, 98, 170)
                    .Button = True
                    .ButtonText = True
                    .Locked = True
                    .Width = 70
                End With

                With .Splits(0).DisplayColumns("Remarks")
                    .HeadingStyle.HorizontalAlignment = AlignHorzEnum.Center
                    .Style.HorizontalAlignment = AlignHorzEnum.Near
                    .Style.VerticalAlignment = AlignVertEnum.Center
                    .Locked = True
                    .Width = Width / 3
                End With

                With .Splits(0).DisplayColumns("User Code")
                    .HeadingStyle.HorizontalAlignment = AlignHorzEnum.Center
                    .Style.HorizontalAlignment = AlignHorzEnum.Center
                    .Style.VerticalAlignment = AlignVertEnum.Center
                    .Locked = True
                    .Width = 80
                End With

                With .Splits(0).DisplayColumns("Date Stamp")
                    .HeadingStyle.HorizontalAlignment = AlignHorzEnum.Center
                    .Style.HorizontalAlignment = AlignHorzEnum.Center
                    .Style.VerticalAlignment = AlignVertEnum.Center
                    .DataColumn.NumberFormat = "dd/MM/yyyy HH:mm tt"
                    .Locked = True
                    .AutoSize()
                End With

                SetGirdHeaders(LstDP, dtDP)

            End With

        Catch ex As Exception
            MessageBox.Show("Error: " & ex.Message, cntTitle, MessageBoxButtons.OK, MessageBoxIcon.Error)
        End Try

    End Sub

    Private Sub LstDP_FilterChange(sender As Object, e As EventArgs) Handles LstDP.FilterChange
        Try

            GridFilterChange(LstDP)

        Catch ex As Exception

        End Try
    End Sub

#End Region

#Region "Photo"

    Private Sub LstDP_ButtonClick(sender As Object, e As ColEventArgs) Handles LstDP.ButtonClick
        Try

            If e.Column.DataColumn.DataField <> "Photo" Then Exit Sub
            ShowPhoto()

        Catch ex As Exception
            MessageBox.Show("Error: " & ex.Message, cntTitle, MessageBoxButtons.OK, MessageBoxIcon.Error)
        End Try
    End Sub

    Private Sub LstDP_DoubleClick(sender As Object, e As EventArgs) Handles LstDP.DoubleClick
        Try

            ShowPhoto()

        Catch ex As Exception
            MessageBox.Show("Error: " & ex.Message, cntTitle, MessageBoxButtons.OK, MessageBoxIcon.Error)
        End Try
    End Sub

    'Reads the photo of the current row from the table and shows it in its own window
    Private Sub ShowPhoto()

        If LstDP.Row < 0 OrElse LstDP.Row >= LstDP.RowCount Then Exit Sub
        If LstDP.Columns("Photo").CellText(LstDP.Row) <> "View" Then Exit Sub

        Dim locCode As String = LstDP.Columns("Location").CellValue(LstDP.Row).ToString
        Dim recID As Decimal = CDec(LstDP.Columns("RecID").CellValue(LstDP.Row))
        Dim clientName As String = LstDP.Columns("Client Name").CellText(LstDP.Row)
        Dim dateStamp As String = LstDP.Columns("Date Stamp").CellText(LstDP.Row)

        Dim photo As Byte()
        Using cnsql As New SqlConnection(clsConfig.SQLConnectionString)
            cnsql.Open()
            Using cmsql As New SqlCommand(" SELECT DP_Photo FROM dbo.tbl_CPP_DeliveryProof WHERE DP_LocCode = @LocCode AND DP_RecID = @RecID ", cnsql)
                cmsql.Parameters.Add("@LocCode", SqlDbType.VarChar, 4).Value = locCode
                cmsql.Parameters.Add("@RecID", SqlDbType.Decimal).Value = recID
                Dim result = cmsql.ExecuteScalar
                If result Is Nothing OrElse result Is DBNull.Value Then
                    ShowNotification(Me.ParentForm, "This Delivery Proof has no photo", 3000, 12, DevComponents.DotNetBar.eToastPosition.MiddleCenter)
                    Exit Sub
                End If
                photo = CType(result, Byte())
            End Using
        End Using

        'The image is copied out of the stream, so the stream can be closed while the window is open
        Dim image As Drawing.Image
        Using ms As New MemoryStream(photo)
            Using img = Drawing.Image.FromStream(ms)
                image = New Drawing.Bitmap(img)
            End Using
        End Using

        Using frm As New Form
            frm.Text = $"{cntTitle} - {clientName} - {dateStamp}"
            frm.StartPosition = FormStartPosition.CenterParent
            frm.Size = New Drawing.Size(800, 700)
            frm.MinimizeBox = False
            frm.ShowIcon = False
            frm.ShowInTaskbar = False
            frm.KeyPreview = True
            AddHandler frm.KeyDown, Sub(s, k) If k.KeyCode = Keys.Escape Then frm.Close()

            Dim pic As New PictureBox With {
                .Dock = DockStyle.Fill,
                .SizeMode = PictureBoxSizeMode.Zoom,
                .BackColor = Drawing.Color.Black,
                .Image = image
            }
            frm.Controls.Add(pic)

            frm.ShowDialog(Me.ParentForm)
            image.Dispose()
        End Using

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
