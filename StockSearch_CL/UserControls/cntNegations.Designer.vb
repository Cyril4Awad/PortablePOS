<Global.Microsoft.VisualBasic.CompilerServices.DesignerGenerated()> _
Partial Class cntNegations
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
        Dim resources As System.ComponentModel.ComponentResourceManager = New System.ComponentModel.ComponentResourceManager(GetType(cntNegations))
        Me.LstUsers = New C1.Win.C1TrueDBGrid.C1TrueDBGrid()
        Me.LstForms = New C1.Win.C1TrueDBGrid.C1TrueDBGrid()
        Me.LstButtons = New C1.Win.C1TrueDBGrid.C1TrueDBGrid()
        Me.LabelX1 = New DevComponents.DotNetBar.LabelX()
        Me.Label1 = New System.Windows.Forms.Label()
        Me.cmbCompany = New System.Windows.Forms.ComboBox()
        Me.lblUsers = New System.Windows.Forms.Label()
        Me.lblForms = New System.Windows.Forms.Label()
        Me.lblButtons = New System.Windows.Forms.Label()
        Me.btnSave = New System.Windows.Forms.Button()
        CType(Me.LstUsers, System.ComponentModel.ISupportInitialize).BeginInit()
        CType(Me.LstForms, System.ComponentModel.ISupportInitialize).BeginInit()
        CType(Me.LstButtons, System.ComponentModel.ISupportInitialize).BeginInit()
        Me.SuspendLayout()
        '
        'LstUsers
        '
        Me.LstUsers.AllowColMove = False
        Me.LstUsers.AllowColSelect = False
        Me.LstUsers.AllowFilter = False
        Me.LstUsers.AllowRowSizing = C1.Win.C1TrueDBGrid.RowSizingEnum.None
        Me.LstUsers.AllowSort = False
        Me.LstUsers.Anchor = CType((System.Windows.Forms.AnchorStyles.Top Or System.Windows.Forms.AnchorStyles.Bottom), System.Windows.Forms.AnchorStyles)
        Me.LstUsers.BackColor = System.Drawing.Color.White
        Me.LstUsers.EmptyRows = True
        Me.LstUsers.FilterBar = True
        Me.LstUsers.Font = New System.Drawing.Font("Microsoft Sans Serif", 8.25!, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point, CType(0, Byte))
        Me.LstUsers.GroupByCaption = "Drag a column header here to group by that column"
        Me.LstUsers.Images.Add(CType(resources.GetObject("LstUsers.Images"), System.Drawing.Image))
        Me.LstUsers.Location = New System.Drawing.Point(6, 131)
        Me.LstUsers.MultiSelect = C1.Win.C1TrueDBGrid.MultiSelectEnum.None
        Me.LstUsers.Name = "LstUsers"
        Me.LstUsers.PreviewInfo.Location = New System.Drawing.Point(0, 0)
        Me.LstUsers.PreviewInfo.Size = New System.Drawing.Size(0, 0)
        Me.LstUsers.PreviewInfo.ZoomFactor = 75.0R
        Me.LstUsers.PrintInfo.MeasurementDevice = C1.Win.C1TrueDBGrid.PrintInfo.MeasurementDeviceEnum.Screen
        Me.LstUsers.PrintInfo.MeasurementPrinterName = Nothing
        Me.LstUsers.RowHeight = 18
        Me.LstUsers.Size = New System.Drawing.Size(280, 268)
        Me.LstUsers.TabIndex = 2
        Me.LstUsers.Text = "C1TrueDBGrid1"
        Me.LstUsers.UseCompatibleTextRendering = False
        Me.LstUsers.PropBag = resources.GetString("LstUsers.PropBag")
        '
        'LstForms
        '
        Me.LstForms.AllowColMove = False
        Me.LstForms.AllowColSelect = False
        Me.LstForms.AllowFilter = False
        Me.LstForms.AllowRowSizing = C1.Win.C1TrueDBGrid.RowSizingEnum.None
        Me.LstForms.AllowSort = False
        Me.LstForms.Anchor = CType((System.Windows.Forms.AnchorStyles.Top Or System.Windows.Forms.AnchorStyles.Bottom), System.Windows.Forms.AnchorStyles)
        Me.LstForms.BackColor = System.Drawing.Color.White
        Me.LstForms.EmptyRows = True
        Me.LstForms.Font = New System.Drawing.Font("Microsoft Sans Serif", 8.25!, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point, CType(0, Byte))
        Me.LstForms.GroupByCaption = "Drag a column header here to group by that column"
        Me.LstForms.Images.Add(CType(resources.GetObject("LstForms.Images"), System.Drawing.Image))
        Me.LstForms.Location = New System.Drawing.Point(306, 131)
        Me.LstForms.MultiSelect = C1.Win.C1TrueDBGrid.MultiSelectEnum.None
        Me.LstForms.Name = "LstForms"
        Me.LstForms.PreviewInfo.Location = New System.Drawing.Point(0, 0)
        Me.LstForms.PreviewInfo.Size = New System.Drawing.Size(0, 0)
        Me.LstForms.PreviewInfo.ZoomFactor = 75.0R
        Me.LstForms.PrintInfo.MeasurementDevice = C1.Win.C1TrueDBGrid.PrintInfo.MeasurementDeviceEnum.Screen
        Me.LstForms.PrintInfo.MeasurementPrinterName = Nothing
        Me.LstForms.RowHeight = 18
        Me.LstForms.Size = New System.Drawing.Size(280, 268)
        Me.LstForms.TabIndex = 3
        Me.LstForms.Text = "C1TrueDBGrid2"
        Me.LstForms.UseCompatibleTextRendering = False
        Me.LstForms.PropBag = resources.GetString("LstForms.PropBag")
        '
        'LstButtons
        '
        Me.LstButtons.AllowColMove = False
        Me.LstButtons.AllowColSelect = False
        Me.LstButtons.AllowFilter = False
        Me.LstButtons.AllowRowSizing = C1.Win.C1TrueDBGrid.RowSizingEnum.None
        Me.LstButtons.AllowSort = False
        Me.LstButtons.Anchor = CType((System.Windows.Forms.AnchorStyles.Top Or System.Windows.Forms.AnchorStyles.Bottom), System.Windows.Forms.AnchorStyles)
        Me.LstButtons.BackColor = System.Drawing.Color.White
        Me.LstButtons.EmptyRows = True
        Me.LstButtons.Font = New System.Drawing.Font("Microsoft Sans Serif", 8.25!, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point, CType(0, Byte))
        Me.LstButtons.GroupByCaption = "Drag a column header here to group by that column"
        Me.LstButtons.Images.Add(CType(resources.GetObject("LstButtons.Images"), System.Drawing.Image))
        Me.LstButtons.Location = New System.Drawing.Point(606, 131)
        Me.LstButtons.MultiSelect = C1.Win.C1TrueDBGrid.MultiSelectEnum.None
        Me.LstButtons.Name = "LstButtons"
        Me.LstButtons.PreviewInfo.Location = New System.Drawing.Point(0, 0)
        Me.LstButtons.PreviewInfo.Size = New System.Drawing.Size(0, 0)
        Me.LstButtons.PreviewInfo.ZoomFactor = 75.0R
        Me.LstButtons.PrintInfo.MeasurementDevice = C1.Win.C1TrueDBGrid.PrintInfo.MeasurementDeviceEnum.Screen
        Me.LstButtons.PrintInfo.MeasurementPrinterName = Nothing
        Me.LstButtons.RowHeight = 18
        Me.LstButtons.Size = New System.Drawing.Size(280, 268)
        Me.LstButtons.TabIndex = 4
        Me.LstButtons.Text = "C1TrueDBGrid3"
        Me.LstButtons.UseCompatibleTextRendering = False
        Me.LstButtons.PropBag = resources.GetString("LstButtons.PropBag")
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
        Me.LabelX1.Size = New System.Drawing.Size(900, 52)
        Me.LabelX1.TabIndex = 405
        Me.LabelX1.Text = "Negations"
        '
        'Label1
        '
        Me.Label1.BackColor = System.Drawing.Color.Transparent
        Me.Label1.Font = New System.Drawing.Font("Microsoft Sans Serif", 9.0!, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point, CType(0, Byte))
        Me.Label1.ForeColor = System.Drawing.Color.Navy
        Me.Label1.Location = New System.Drawing.Point(6, 76)
        Me.Label1.Name = "Label1"
        Me.Label1.Size = New System.Drawing.Size(70, 19)
        Me.Label1.TabIndex = 407
        Me.Label1.Text = "Company"
        '
        'cmbCompany
        '
        Me.cmbCompany.DropDownStyle = System.Windows.Forms.ComboBoxStyle.DropDownList
        Me.cmbCompany.Font = New System.Drawing.Font("Microsoft Sans Serif", 8.25!)
        Me.cmbCompany.FormattingEnabled = True
        Me.cmbCompany.Location = New System.Drawing.Point(81, 75)
        Me.cmbCompany.Name = "cmbCompany"
        Me.cmbCompany.Size = New System.Drawing.Size(200, 21)
        Me.cmbCompany.TabIndex = 1
        '
        'lblUsers
        '
        Me.lblUsers.BackColor = System.Drawing.Color.Transparent
        Me.lblUsers.Font = New System.Drawing.Font("Microsoft Sans Serif", 9.0!, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point, CType(0, Byte))
        Me.lblUsers.ForeColor = System.Drawing.Color.Navy
        Me.lblUsers.Location = New System.Drawing.Point(6, 109)
        Me.lblUsers.Name = "lblUsers"
        Me.lblUsers.Size = New System.Drawing.Size(280, 19)
        Me.lblUsers.TabIndex = 408
        Me.lblUsers.Text = "Users"
        '
        'lblForms
        '
        Me.lblForms.BackColor = System.Drawing.Color.Transparent
        Me.lblForms.Font = New System.Drawing.Font("Microsoft Sans Serif", 9.0!, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point, CType(0, Byte))
        Me.lblForms.ForeColor = System.Drawing.Color.Navy
        Me.lblForms.Location = New System.Drawing.Point(306, 109)
        Me.lblForms.Name = "lblForms"
        Me.lblForms.Size = New System.Drawing.Size(280, 19)
        Me.lblForms.TabIndex = 409
        Me.lblForms.Text = "Allowed Forms"
        '
        'lblButtons
        '
        Me.lblButtons.BackColor = System.Drawing.Color.Transparent
        Me.lblButtons.Font = New System.Drawing.Font("Microsoft Sans Serif", 9.0!, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point, CType(0, Byte))
        Me.lblButtons.ForeColor = System.Drawing.Color.Navy
        Me.lblButtons.Location = New System.Drawing.Point(606, 109)
        Me.lblButtons.Name = "lblButtons"
        Me.lblButtons.Size = New System.Drawing.Size(280, 19)
        Me.lblButtons.TabIndex = 410
        Me.lblButtons.Text = "Negations"
        '
        'btnSave
        '
        Me.btnSave.Anchor = System.Windows.Forms.AnchorStyles.Bottom
        Me.btnSave.BackColor = System.Drawing.Color.FromArgb(CType(CType(8, Byte), Integer), CType(CType(98, Byte), Integer), CType(CType(170, Byte), Integer))
        Me.btnSave.Cursor = System.Windows.Forms.Cursors.Hand
        Me.btnSave.FlatAppearance.BorderSize = 0
        Me.btnSave.FlatStyle = System.Windows.Forms.FlatStyle.Flat
        Me.btnSave.Font = New System.Drawing.Font("Tahoma", 9.0!, System.Drawing.FontStyle.Bold)
        Me.btnSave.ForeColor = System.Drawing.Color.White
        Me.btnSave.Location = New System.Drawing.Point(786, 412)
        Me.btnSave.Name = "btnSave"
        Me.btnSave.Size = New System.Drawing.Size(100, 28)
        Me.btnSave.TabIndex = 5
        Me.btnSave.Text = "Save"
        Me.btnSave.UseVisualStyleBackColor = False
        '
        'cntNegations
        '
        Me.AutoScaleDimensions = New System.Drawing.SizeF(6.0!, 13.0!)
        Me.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font
        Me.BackColor = System.Drawing.Color.FromArgb(CType(CType(227, Byte), Integer), CType(CType(239, Byte), Integer), CType(CType(255, Byte), Integer))
        Me.Controls.Add(Me.btnSave)
        Me.Controls.Add(Me.LstButtons)
        Me.Controls.Add(Me.LstForms)
        Me.Controls.Add(Me.LstUsers)
        Me.Controls.Add(Me.lblButtons)
        Me.Controls.Add(Me.lblForms)
        Me.Controls.Add(Me.lblUsers)
        Me.Controls.Add(Me.cmbCompany)
        Me.Controls.Add(Me.Label1)
        Me.Controls.Add(Me.LabelX1)
        Me.Name = "cntNegations"
        Me.Size = New System.Drawing.Size(900, 452)
        CType(Me.LstUsers, System.ComponentModel.ISupportInitialize).EndInit()
        CType(Me.LstForms, System.ComponentModel.ISupportInitialize).EndInit()
        CType(Me.LstButtons, System.ComponentModel.ISupportInitialize).EndInit()
        Me.ResumeLayout(False)

    End Sub

    Friend WithEvents LstUsers As C1.Win.C1TrueDBGrid.C1TrueDBGrid
    Friend WithEvents LstForms As C1.Win.C1TrueDBGrid.C1TrueDBGrid
    Friend WithEvents LstButtons As C1.Win.C1TrueDBGrid.C1TrueDBGrid
    Friend WithEvents LabelX1 As DevComponents.DotNetBar.LabelX
    Friend WithEvents Label1 As Windows.Forms.Label
    Friend WithEvents cmbCompany As Windows.Forms.ComboBox
    Friend WithEvents lblUsers As Windows.Forms.Label
    Friend WithEvents lblForms As Windows.Forms.Label
    Friend WithEvents lblButtons As Windows.Forms.Label
    Friend WithEvents btnSave As Windows.Forms.Button
End Class
