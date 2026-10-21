<Global.Microsoft.VisualBasic.CompilerServices.DesignerGenerated()> _
Partial Class cntPotentialClients
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
        Dim resources As System.ComponentModel.ComponentResourceManager = New System.ComponentModel.ComponentResourceManager(GetType(cntPotentialClients))
        Me.LstPC = New C1.Win.C1TrueDBGrid.C1TrueDBGrid()
        Me.btnLoad = New System.Windows.Forms.Button()
        Me.Label1 = New System.Windows.Forms.Label()
        Me.Label7 = New System.Windows.Forms.Label()
        Me.txtStartDate = New C1.Win.C1Input.C1DateEdit()
        Me.txtEndDate = New C1.Win.C1Input.C1DateEdit()
        Me.LabelX1 = New DevComponents.DotNetBar.LabelX()
        CType(Me.LstPC, System.ComponentModel.ISupportInitialize).BeginInit()
        CType(Me.txtStartDate, System.ComponentModel.ISupportInitialize).BeginInit()
        CType(Me.txtEndDate, System.ComponentModel.ISupportInitialize).BeginInit()
        Me.SuspendLayout()
        '
        'LstPC
        '
        Me.LstPC.AllowColMove = False
        Me.LstPC.AllowColSelect = False
        Me.LstPC.AllowFilter = False
        Me.LstPC.AllowRowSizing = C1.Win.C1TrueDBGrid.RowSizingEnum.None
        Me.LstPC.AllowSort = False
        Me.LstPC.AllowUpdateOnBlur = False
        Me.LstPC.AlternatingRows = True
        Me.LstPC.Anchor = CType((((System.Windows.Forms.AnchorStyles.Top Or System.Windows.Forms.AnchorStyles.Bottom) _
            Or System.Windows.Forms.AnchorStyles.Left) _
            Or System.Windows.Forms.AnchorStyles.Right), System.Windows.Forms.AnchorStyles)
        Me.LstPC.BackColor = System.Drawing.Color.White
        Me.LstPC.CaptionHeight = 17
        Me.LstPC.ColumnFooters = True
        Me.LstPC.EmptyRows = True
        Me.LstPC.ExtendRightColumn = True
        Me.LstPC.FetchRowStyles = True
        Me.LstPC.FilterBar = True
        Me.LstPC.Font = New System.Drawing.Font("Microsoft Sans Serif", 8.25!, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point, CType(0, Byte))
        Me.LstPC.GroupByCaption = "Drag a column header here to group by that column"
        Me.LstPC.Images.Add(CType(resources.GetObject("LstPC.Images"), System.Drawing.Image))
        Me.LstPC.Location = New System.Drawing.Point(32, 131)
        Me.LstPC.MultiSelect = C1.Win.C1TrueDBGrid.MultiSelectEnum.None
        Me.LstPC.Name = "LstPC"
        Me.LstPC.PreviewInfo.Location = New System.Drawing.Point(0, 0)
        Me.LstPC.PreviewInfo.Size = New System.Drawing.Size(0, 0)
        Me.LstPC.PreviewInfo.ZoomFactor = 75.0R
        Me.LstPC.PrintInfo.MeasurementDevice = C1.Win.C1TrueDBGrid.PrintInfo.MeasurementDeviceEnum.Screen
        Me.LstPC.PrintInfo.MeasurementPrinterName = Nothing
        Me.LstPC.RowHeight = 15
        Me.LstPC.Size = New System.Drawing.Size(747, 278)
        Me.LstPC.TabIndex = 415
        Me.LstPC.Text = "C1TrueDBGrid1"
        Me.LstPC.UseCompatibleTextRendering = False
        Me.LstPC.PropBag = resources.GetString("LstPC.PropBag")
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
        Me.btnLoad.TabIndex = 411
        Me.btnLoad.UseVisualStyleBackColor = False
        '
        'Label1
        '
        Me.Label1.BackColor = System.Drawing.Color.Transparent
        Me.Label1.Font = New System.Drawing.Font("Microsoft Sans Serif", 9.0!, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point, CType(0, Byte))
        Me.Label1.ForeColor = System.Drawing.Color.Navy
        Me.Label1.Location = New System.Drawing.Point(279, 77)
        Me.Label1.Name = "Label1"
        Me.Label1.Size = New System.Drawing.Size(38, 19)
        Me.Label1.TabIndex = 414
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
        Me.Label7.TabIndex = 413
        Me.Label7.Tag = ""
        Me.Label7.Text = "Logging Between"
        '
        'txtStartDate
        '
        Me.txtStartDate.AllowDrop = True
        Me.txtStartDate.BackColor = System.Drawing.Color.White
        Me.txtStartDate.BorderStyle = System.Windows.Forms.BorderStyle.FixedSingle
        '
        '
        '
        Me.txtStartDate.Calendar.Name = ""
        Me.txtStartDate.DateTimeInput = False
        Me.txtStartDate.EditMask = "99/99/9999"
        Me.txtStartDate.EmptyAsNull = True
        Me.txtStartDate.ErrorInfo.ErrorMessage = "Invalid data Entry."
        Me.txtStartDate.ErrorInfo.ErrorMessageCaption = "Reprint Transfers"
        Me.txtStartDate.FormatType = C1.Win.C1Input.FormatTypeEnum.ShortDate
        Me.txtStartDate.Location = New System.Drawing.Point(155, 76)
        Me.txtStartDate.MaxLength = 10
        Me.txtStartDate.Name = "txtStartDate"
        Me.txtStartDate.Size = New System.Drawing.Size(118, 20)
        Me.txtStartDate.TabIndex = 409
        Me.txtStartDate.Tag = Nothing
        Me.txtStartDate.TextAlign = System.Windows.Forms.HorizontalAlignment.Center
        '
        'txtEndDate
        '
        Me.txtEndDate.BackColor = System.Drawing.Color.White
        Me.txtEndDate.BorderStyle = System.Windows.Forms.BorderStyle.FixedSingle
        '
        '
        '
        Me.txtEndDate.Calendar.Name = ""
        Me.txtEndDate.DateTimeInput = False
        Me.txtEndDate.EditMask = "99/99/9999"
        Me.txtEndDate.EmptyAsNull = True
        Me.txtEndDate.ErrorInfo.ErrorMessage = "Invalid data Entry."
        Me.txtEndDate.ErrorInfo.ErrorMessageCaption = "Reprint Transfers"
        Me.txtEndDate.FormatType = C1.Win.C1Input.FormatTypeEnum.ShortDate
        Me.txtEndDate.Location = New System.Drawing.Point(323, 76)
        Me.txtEndDate.MaxLength = 10
        Me.txtEndDate.Name = "txtEndDate"
        Me.txtEndDate.Size = New System.Drawing.Size(120, 20)
        Me.txtEndDate.TabIndex = 410
        Me.txtEndDate.Tag = Nothing
        Me.txtEndDate.TextAlign = System.Windows.Forms.HorizontalAlignment.Center
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
        Me.LabelX1.TabIndex = 412
        Me.LabelX1.Text = "Potential Clients"
        '
        'cntPotentialClients
        '
        Me.AutoScaleDimensions = New System.Drawing.SizeF(6.0!, 13.0!)
        Me.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font
        Me.BackColor = System.Drawing.Color.FromArgb(CType(CType(227, Byte), Integer), CType(CType(239, Byte), Integer), CType(CType(255, Byte), Integer))
        Me.Controls.Add(Me.LstPC)
        Me.Controls.Add(Me.btnLoad)
        Me.Controls.Add(Me.Label1)
        Me.Controls.Add(Me.Label7)
        Me.Controls.Add(Me.txtStartDate)
        Me.Controls.Add(Me.txtEndDate)
        Me.Controls.Add(Me.LabelX1)
        Me.Name = "cntPotentialClients"
        Me.Size = New System.Drawing.Size(812, 444)
        CType(Me.LstPC, System.ComponentModel.ISupportInitialize).EndInit()
        CType(Me.txtStartDate, System.ComponentModel.ISupportInitialize).EndInit()
        CType(Me.txtEndDate, System.ComponentModel.ISupportInitialize).EndInit()
        Me.ResumeLayout(False)
        Me.PerformLayout()

    End Sub

    Friend WithEvents LstPC As C1.Win.C1TrueDBGrid.C1TrueDBGrid
    Friend WithEvents btnLoad As Windows.Forms.Button
    Friend WithEvents Label1 As Windows.Forms.Label
    Friend WithEvents Label7 As Windows.Forms.Label
    Friend WithEvents txtStartDate As C1.Win.C1Input.C1DateEdit
    Friend WithEvents txtEndDate As C1.Win.C1Input.C1DateEdit
    Friend WithEvents LabelX1 As DevComponents.DotNetBar.LabelX
End Class
