<Global.Microsoft.VisualBasic.CompilerServices.DesignerGenerated()> _
Partial Class cntUploadsHistory
    Inherits System.Windows.Forms.UserControl

    'UserControl overrides dispose to clean up the component list.
    <System.Diagnostics.DebuggerNonUserCode()> _
    Protected Overrides Sub Dispose(ByVal disposing As Boolean)
        Try
            If disposing AndAlso components IsNot Nothing Then
                components.Dispose()
            End If
        Finally
            MyBase.Dispose(disposing)
        End Try
    End Sub

    'Required by the Windows Form Designer
    Private components As System.ComponentModel.IContainer

    'NOTE: The following procedure is required by the Windows Form Designer
    'It can be modified using the Windows Form Designer.  
    'Do not modify it using the code editor.
    <System.Diagnostics.DebuggerStepThrough()> _
    Private Sub InitializeComponent()
        Dim resources As System.ComponentModel.ComponentResourceManager = New System.ComponentModel.ComponentResourceManager(GetType(cntUploadsHistory))
        Me.LabelX1 = New DevComponents.DotNetBar.LabelX()
        Me.Label1 = New System.Windows.Forms.Label()
        Me.Label7 = New System.Windows.Forms.Label()
        Me.txtStartDate = New C1.Win.C1Input.C1DateEdit()
        Me.txtEndDate = New C1.Win.C1Input.C1DateEdit()
        Me.btnLoad = New System.Windows.Forms.Button()
        Me.LstHist = New C1.Win.C1TrueDBGrid.C1TrueDBGrid()
        CType(Me.txtStartDate, System.ComponentModel.ISupportInitialize).BeginInit()
        CType(Me.txtEndDate, System.ComponentModel.ISupportInitialize).BeginInit()
        CType(Me.LstHist, System.ComponentModel.ISupportInitialize).BeginInit()
        Me.SuspendLayout()
        '
        'LabelX1
        '
        '
        '
        '
        Me.LabelX1.BackgroundStyle.BackColor = System.Drawing.Color.White
        Me.LabelX1.BackgroundStyle.BackColor2 = System.Drawing.Color.FromArgb(CType(CType(227, Byte), Integer), CType(CType(239, Byte), Integer), CType(CType(255, Byte), Integer))
        Me.LabelX1.BackgroundStyle.CornerType = DevComponents.DotNetBar.eCornerType.Square
        Me.LabelX1.Dock = System.Windows.Forms.DockStyle.Top
        Me.LabelX1.Font = New System.Drawing.Font("Microsoft Sans Serif", 19.0!)
        Me.LabelX1.ForeColor = System.Drawing.SystemColors.ControlText
        Me.LabelX1.Location = New System.Drawing.Point(0, 0)
        Me.LabelX1.Name = "LabelX1"
        Me.LabelX1.PaddingLeft = 25
        Me.LabelX1.Size = New System.Drawing.Size(812, 52)
        Me.LabelX1.TabIndex = 384
        Me.LabelX1.Text = "Uploads History"
        '
        'Label1
        '
        Me.Label1.BackColor = System.Drawing.Color.Transparent
        Me.Label1.Font = New System.Drawing.Font("Microsoft Sans Serif", 9.0!, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point, CType(0, Byte))
        Me.Label1.ForeColor = System.Drawing.Color.Navy
        Me.Label1.Location = New System.Drawing.Point(279, 77)
        Me.Label1.Name = "Label1"
        Me.Label1.Size = New System.Drawing.Size(38, 19)
        Me.Label1.TabIndex = 388
        Me.Label1.Tag = ""
        Me.Label1.Text = "And"
        '
        'Label7
        '
        Me.Label7.BackColor = System.Drawing.Color.Transparent
        Me.Label7.Font = New System.Drawing.Font("Microsoft Sans Serif", 9.0!, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point, CType(0, Byte))
        Me.Label7.ForeColor = System.Drawing.Color.Navy
        Me.Label7.Location = New System.Drawing.Point(29, 76)
        Me.Label7.Name = "Label7"
        Me.Label7.Size = New System.Drawing.Size(120, 19)
        Me.Label7.TabIndex = 387
        Me.Label7.Tag = ""
        Me.Label7.Text = "Logging Between"
        '
        'txtStartDate
        '
        Me.txtStartDate.AllowDrop = True
        Me.txtStartDate.BackColor = System.Drawing.Color.White
        Me.txtStartDate.BorderStyle = System.Windows.Forms.BorderStyle.FixedSingle
        Me.txtStartDate.DateTimeInput = False
        Me.txtStartDate.EditMask = "99/99/9999"
        Me.txtStartDate.EmptyAsNull = True
        Me.txtStartDate.ErrorInfo.ErrorMessage = "Invalid data Entry."
        Me.txtStartDate.ErrorInfo.ErrorMessageCaption = "Reprint Transfers"
        Me.txtStartDate.FormatType = C1.Win.C1Input.FormatTypeEnum.ShortDate
        Me.txtStartDate.Location = New System.Drawing.Point(155, 76)
        Me.txtStartDate.MaxLength = 10
        Me.txtStartDate.Name = "txtStartDate"
        Me.txtStartDate.Size = New System.Drawing.Size(118, 18)
        Me.txtStartDate.TabIndex = 0
        Me.txtStartDate.Tag = Nothing
        Me.txtStartDate.TextAlign = System.Windows.Forms.HorizontalAlignment.Center
        '
        'txtEndDate
        '
        Me.txtEndDate.BackColor = System.Drawing.Color.White
        Me.txtEndDate.BorderStyle = System.Windows.Forms.BorderStyle.FixedSingle
        Me.txtEndDate.DateTimeInput = False
        Me.txtEndDate.EditMask = "99/99/9999"
        Me.txtEndDate.EmptyAsNull = True
        Me.txtEndDate.ErrorInfo.ErrorMessage = "Invalid data Entry."
        Me.txtEndDate.ErrorInfo.ErrorMessageCaption = "Reprint Transfers"
        Me.txtEndDate.FormatType = C1.Win.C1Input.FormatTypeEnum.ShortDate
        Me.txtEndDate.Location = New System.Drawing.Point(323, 76)
        Me.txtEndDate.MaxLength = 10
        Me.txtEndDate.Name = "txtEndDate"
        Me.txtEndDate.Size = New System.Drawing.Size(120, 18)
        Me.txtEndDate.TabIndex = 1
        Me.txtEndDate.Tag = Nothing
        Me.txtEndDate.TextAlign = System.Windows.Forms.HorizontalAlignment.Center
        '
        'btnLoad
        '
        Me.btnLoad.BackColor = System.Drawing.Color.Transparent
        Me.btnLoad.Cursor = System.Windows.Forms.Cursors.Hand
        Me.btnLoad.FlatAppearance.BorderSize = 0
        Me.btnLoad.FlatAppearance.MouseDownBackColor = System.Drawing.Color.RoyalBlue
        Me.btnLoad.FlatAppearance.MouseOverBackColor = System.Drawing.Color.White
        Me.btnLoad.FlatStyle = System.Windows.Forms.FlatStyle.Flat
        Me.btnLoad.Font = New System.Drawing.Font("Microsoft Sans Serif", 8.25!, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point, CType(0, Byte))
        Me.btnLoad.ForeColor = System.Drawing.Color.Navy
        Me.btnLoad.Image = CType(resources.GetObject("btnLoad.Image"), System.Drawing.Image)
        Me.btnLoad.Location = New System.Drawing.Point(461, 63)
        Me.btnLoad.Margin = New System.Windows.Forms.Padding(0)
        Me.btnLoad.Name = "btnLoad"
        Me.btnLoad.Size = New System.Drawing.Size(47, 43)
        Me.btnLoad.TabIndex = 2
        Me.btnLoad.UseVisualStyleBackColor = False
        '
        'LstHist
        '
        Me.LstHist.AllowColMove = False
        Me.LstHist.AllowColSelect = False
        Me.LstHist.AllowFilter = False
        Me.LstHist.AllowRowSizing = C1.Win.C1TrueDBGrid.RowSizingEnum.None
        Me.LstHist.AllowSort = False
        Me.LstHist.AllowUpdateOnBlur = False
        Me.LstHist.AlternatingRows = True
        Me.LstHist.Anchor = CType((((System.Windows.Forms.AnchorStyles.Top Or System.Windows.Forms.AnchorStyles.Bottom) _
            Or System.Windows.Forms.AnchorStyles.Left) _
            Or System.Windows.Forms.AnchorStyles.Right), System.Windows.Forms.AnchorStyles)
        Me.LstHist.BackColor = System.Drawing.Color.White
        Me.LstHist.CaptionHeight = 17
        Me.LstHist.ColumnFooters = True
        Me.LstHist.EmptyRows = True
        Me.LstHist.ExtendRightColumn = True
        Me.LstHist.FetchRowStyles = True
        Me.LstHist.FilterBar = True
        Me.LstHist.Font = New System.Drawing.Font("Microsoft Sans Serif", 8.25!, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point, CType(0, Byte))
        Me.LstHist.GroupByCaption = "Drag a column header here to group by that column"
        Me.LstHist.Images.Add(CType(resources.GetObject("LstHist.Images"), System.Drawing.Image))
        Me.LstHist.Location = New System.Drawing.Point(32, 131)
        Me.LstHist.MultiSelect = C1.Win.C1TrueDBGrid.MultiSelectEnum.None
        Me.LstHist.Name = "LstHist"
        Me.LstHist.PreviewInfo.Location = New System.Drawing.Point(0, 0)
        Me.LstHist.PreviewInfo.Size = New System.Drawing.Size(0, 0)
        Me.LstHist.PreviewInfo.ZoomFactor = 75.0R
        Me.LstHist.PrintInfo.MeasurementDevice = C1.Win.C1TrueDBGrid.PrintInfo.MeasurementDeviceEnum.Screen
        Me.LstHist.PrintInfo.MeasurementPrinterName = Nothing
        Me.LstHist.RowHeight = 15
        Me.LstHist.Size = New System.Drawing.Size(747, 278)
        Me.LstHist.TabIndex = 401
        Me.LstHist.Text = "C1TrueDBGrid1"
        Me.LstHist.UseCompatibleTextRendering = False
        Me.LstHist.PropBag = resources.GetString("LstHist.PropBag")
        '
        'cntUploadsHistory
        '
        Me.AutoScaleDimensions = New System.Drawing.SizeF(6.0!, 13.0!)
        Me.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font
        Me.BackColor = System.Drawing.Color.FromArgb(CType(CType(227, Byte), Integer), CType(CType(239, Byte), Integer), CType(CType(255, Byte), Integer))
        Me.Controls.Add(Me.LstHist)
        Me.Controls.Add(Me.btnLoad)
        Me.Controls.Add(Me.Label1)
        Me.Controls.Add(Me.Label7)
        Me.Controls.Add(Me.txtStartDate)
        Me.Controls.Add(Me.txtEndDate)
        Me.Controls.Add(Me.LabelX1)
        Me.Name = "cntUploadsHistory"
        Me.Size = New System.Drawing.Size(812, 444)
        CType(Me.txtStartDate, System.ComponentModel.ISupportInitialize).EndInit()
        CType(Me.txtEndDate, System.ComponentModel.ISupportInitialize).EndInit()
        CType(Me.LstHist, System.ComponentModel.ISupportInitialize).EndInit()
        Me.ResumeLayout(False)

    End Sub

    Friend WithEvents LabelX1 As DevComponents.DotNetBar.LabelX
    Friend WithEvents Label1 As System.Windows.Forms.Label
    Friend WithEvents Label7 As System.Windows.Forms.Label
    Friend WithEvents txtStartDate As C1.Win.C1Input.C1DateEdit
    Friend WithEvents txtEndDate As C1.Win.C1Input.C1DateEdit
    Friend WithEvents btnLoad As System.Windows.Forms.Button
    Friend WithEvents LstHist As C1.Win.C1TrueDBGrid.C1TrueDBGrid
End Class
