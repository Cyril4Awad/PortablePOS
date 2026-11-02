<Global.Microsoft.VisualBasic.CompilerServices.DesignerGenerated()> _
Partial Class cntUserPrivilege
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
        Dim resources As System.ComponentModel.ComponentResourceManager = New System.ComponentModel.ComponentResourceManager(GetType(cntUserPrivilege))
        Me.LstUsers = New C1.Win.C1TrueDBGrid.C1TrueDBGrid()
        Me.LstPriv = New C1.Win.C1TrueDBGrid.C1TrueDBGrid()
        Me.LabelX1 = New DevComponents.DotNetBar.LabelX()
        Me.lblUsers = New System.Windows.Forms.Label()
        Me.lblSteps = New System.Windows.Forms.Label()
        Me.btnAllowAll = New System.Windows.Forms.Button()
        Me.btnDenyAll = New System.Windows.Forms.Button()
        Me.btnSave = New System.Windows.Forms.Button()
        Me.btnUnlimited = New System.Windows.Forms.Button()
        Me.btnDuplicate = New System.Windows.Forms.Button()
        CType(Me.LstUsers, System.ComponentModel.ISupportInitialize).BeginInit()
        CType(Me.LstPriv, System.ComponentModel.ISupportInitialize).BeginInit()
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
        Me.LstUsers.Location = New System.Drawing.Point(25, 121)
        Me.LstUsers.MultiSelect = C1.Win.C1TrueDBGrid.MultiSelectEnum.None
        Me.LstUsers.Name = "LstUsers"
        Me.LstUsers.PreviewInfo.Location = New System.Drawing.Point(0, 0)
        Me.LstUsers.PreviewInfo.Size = New System.Drawing.Size(0, 0)
        Me.LstUsers.PreviewInfo.ZoomFactor = 75.0R
        Me.LstUsers.PrintInfo.MeasurementDevice = C1.Win.C1TrueDBGrid.PrintInfo.MeasurementDeviceEnum.Screen
        Me.LstUsers.PrintInfo.MeasurementPrinterName = Nothing
        Me.LstUsers.RowHeight = 18
        Me.LstUsers.Size = New System.Drawing.Size(280, 293)
        Me.LstUsers.TabIndex = 1
        Me.LstUsers.Text = "C1TrueDBGrid1"
        Me.LstUsers.UseCompatibleTextRendering = False
        Me.LstUsers.PropBag = resources.GetString("LstUsers.PropBag")
        '
        'LstPriv
        '
        Me.LstPriv.AllowColMove = False
        Me.LstPriv.AllowColSelect = False
        Me.LstPriv.AllowFilter = False
        Me.LstPriv.AllowRowSizing = C1.Win.C1TrueDBGrid.RowSizingEnum.None
        Me.LstPriv.AllowSort = False
        Me.LstPriv.Anchor = CType((System.Windows.Forms.AnchorStyles.Top Or System.Windows.Forms.AnchorStyles.Bottom), System.Windows.Forms.AnchorStyles)
        Me.LstPriv.BackColor = System.Drawing.Color.White
        Me.LstPriv.EmptyRows = True
        Me.LstPriv.FetchRowStyles = True
        Me.LstPriv.FilterBar = True
        Me.LstPriv.Font = New System.Drawing.Font("Microsoft Sans Serif", 8.25!, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point, CType(0, Byte))
        Me.LstPriv.GroupByCaption = "Drag a column header here to group by that column"
        Me.LstPriv.Images.Add(CType(resources.GetObject("LstPriv.Images"), System.Drawing.Image))
        Me.LstPriv.Location = New System.Drawing.Point(325, 121)
        Me.LstPriv.MultiSelect = C1.Win.C1TrueDBGrid.MultiSelectEnum.None
        Me.LstPriv.Name = "LstPriv"
        Me.LstPriv.PreviewInfo.Location = New System.Drawing.Point(0, 0)
        Me.LstPriv.PreviewInfo.Size = New System.Drawing.Size(0, 0)
        Me.LstPriv.PreviewInfo.ZoomFactor = 75.0R
        Me.LstPriv.PrintInfo.MeasurementDevice = C1.Win.C1TrueDBGrid.PrintInfo.MeasurementDeviceEnum.Screen
        Me.LstPriv.PrintInfo.MeasurementPrinterName = Nothing
        Me.LstPriv.RowHeight = 18
        Me.LstPriv.Size = New System.Drawing.Size(550, 293)
        Me.LstPriv.TabIndex = 2
        Me.LstPriv.Text = "C1TrueDBGrid2"
        Me.LstPriv.UseCompatibleTextRendering = False
        Me.LstPriv.PropBag = resources.GetString("LstPriv.PropBag")
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
        Me.LabelX1.Text = "User Privilege"
        '
        'lblUsers
        '
        Me.lblUsers.BackColor = System.Drawing.Color.Transparent
        Me.lblUsers.Font = New System.Drawing.Font("Microsoft Sans Serif", 9.0!, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point, CType(0, Byte))
        Me.lblUsers.ForeColor = System.Drawing.Color.Navy
        Me.lblUsers.Location = New System.Drawing.Point(25, 99)
        Me.lblUsers.Name = "lblUsers"
        Me.lblUsers.Size = New System.Drawing.Size(280, 19)
        Me.lblUsers.TabIndex = 406
        Me.lblUsers.Text = "Users"
        '
        'lblSteps
        '
        Me.lblSteps.BackColor = System.Drawing.Color.Transparent
        Me.lblSteps.Font = New System.Drawing.Font("Microsoft Sans Serif", 9.0!, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point, CType(0, Byte))
        Me.lblSteps.ForeColor = System.Drawing.Color.Navy
        Me.lblSteps.Location = New System.Drawing.Point(325, 99)
        Me.lblSteps.Name = "lblSteps"
        Me.lblSteps.Size = New System.Drawing.Size(550, 19)
        Me.lblSteps.TabIndex = 407
        Me.lblSteps.Text = "Menu Steps"
        '
        'btnAllowAll
        '
        Me.btnAllowAll.Anchor = CType((System.Windows.Forms.AnchorStyles.Bottom Or System.Windows.Forms.AnchorStyles.Left), System.Windows.Forms.AnchorStyles)
        Me.btnAllowAll.BackColor = System.Drawing.Color.White
        Me.btnAllowAll.Cursor = System.Windows.Forms.Cursors.Hand
        Me.btnAllowAll.FlatAppearance.BorderColor = System.Drawing.Color.FromArgb(CType(CType(8, Byte), Integer), CType(CType(98, Byte), Integer), CType(CType(170, Byte), Integer))
        Me.btnAllowAll.FlatAppearance.MouseOverBackColor = System.Drawing.Color.Silver
        Me.btnAllowAll.FlatStyle = System.Windows.Forms.FlatStyle.Flat
        Me.btnAllowAll.Font = New System.Drawing.Font("Tahoma", 9.0!)
        Me.btnAllowAll.ForeColor = System.Drawing.Color.FromArgb(CType(CType(8, Byte), Integer), CType(CType(98, Byte), Integer), CType(CType(170, Byte), Integer))
        Me.btnAllowAll.Location = New System.Drawing.Point(325, 427)
        Me.btnAllowAll.Name = "btnAllowAll"
        Me.btnAllowAll.Size = New System.Drawing.Size(100, 28)
        Me.btnAllowAll.TabIndex = 3
        Me.btnAllowAll.Text = "Allow All"
        Me.btnAllowAll.UseVisualStyleBackColor = False
        '
        'btnDenyAll
        '
        Me.btnDenyAll.Anchor = CType((System.Windows.Forms.AnchorStyles.Bottom Or System.Windows.Forms.AnchorStyles.Left), System.Windows.Forms.AnchorStyles)
        Me.btnDenyAll.BackColor = System.Drawing.Color.White
        Me.btnDenyAll.Cursor = System.Windows.Forms.Cursors.Hand
        Me.btnDenyAll.FlatAppearance.BorderColor = System.Drawing.Color.FromArgb(CType(CType(8, Byte), Integer), CType(CType(98, Byte), Integer), CType(CType(170, Byte), Integer))
        Me.btnDenyAll.FlatAppearance.MouseOverBackColor = System.Drawing.Color.Silver
        Me.btnDenyAll.FlatStyle = System.Windows.Forms.FlatStyle.Flat
        Me.btnDenyAll.Font = New System.Drawing.Font("Tahoma", 9.0!)
        Me.btnDenyAll.ForeColor = System.Drawing.Color.FromArgb(CType(CType(8, Byte), Integer), CType(CType(98, Byte), Integer), CType(CType(170, Byte), Integer))
        Me.btnDenyAll.Location = New System.Drawing.Point(431, 427)
        Me.btnDenyAll.Name = "btnDenyAll"
        Me.btnDenyAll.Size = New System.Drawing.Size(100, 28)
        Me.btnDenyAll.TabIndex = 4
        Me.btnDenyAll.Text = "Deny All"
        Me.btnDenyAll.UseVisualStyleBackColor = False
        '
        'btnSave
        '
        Me.btnSave.Anchor = CType((System.Windows.Forms.AnchorStyles.Bottom Or System.Windows.Forms.AnchorStyles.Right), System.Windows.Forms.AnchorStyles)
        Me.btnSave.BackColor = System.Drawing.Color.FromArgb(CType(CType(8, Byte), Integer), CType(CType(98, Byte), Integer), CType(CType(170, Byte), Integer))
        Me.btnSave.Cursor = System.Windows.Forms.Cursors.Hand
        Me.btnSave.FlatAppearance.BorderSize = 0
        Me.btnSave.FlatStyle = System.Windows.Forms.FlatStyle.Flat
        Me.btnSave.Font = New System.Drawing.Font("Tahoma", 9.0!, System.Drawing.FontStyle.Bold)
        Me.btnSave.ForeColor = System.Drawing.Color.White
        Me.btnSave.Location = New System.Drawing.Point(775, 427)
        Me.btnSave.Name = "btnSave"
        Me.btnSave.Size = New System.Drawing.Size(100, 28)
        Me.btnSave.TabIndex = 7
        Me.btnSave.Text = "Save"
        Me.btnSave.UseVisualStyleBackColor = False
        '
        'btnUnlimited
        '
        Me.btnUnlimited.Anchor = CType((System.Windows.Forms.AnchorStyles.Bottom Or System.Windows.Forms.AnchorStyles.Left), System.Windows.Forms.AnchorStyles)
        Me.btnUnlimited.BackColor = System.Drawing.Color.White
        Me.btnUnlimited.Cursor = System.Windows.Forms.Cursors.Hand
        Me.btnUnlimited.FlatAppearance.BorderColor = System.Drawing.Color.FromArgb(CType(CType(8, Byte), Integer), CType(CType(98, Byte), Integer), CType(CType(170, Byte), Integer))
        Me.btnUnlimited.FlatAppearance.MouseOverBackColor = System.Drawing.Color.Silver
        Me.btnUnlimited.FlatStyle = System.Windows.Forms.FlatStyle.Flat
        Me.btnUnlimited.Font = New System.Drawing.Font("Tahoma", 9.0!)
        Me.btnUnlimited.ForeColor = System.Drawing.Color.FromArgb(CType(CType(8, Byte), Integer), CType(CType(98, Byte), Integer), CType(CType(170, Byte), Integer))
        Me.btnUnlimited.Location = New System.Drawing.Point(537, 427)
        Me.btnUnlimited.Name = "btnUnlimited"
        Me.btnUnlimited.Size = New System.Drawing.Size(110, 28)
        Me.btnUnlimited.TabIndex = 5
        Me.btnUnlimited.Text = "Clear Date"
        Me.btnUnlimited.UseVisualStyleBackColor = False
        '
        'btnDuplicate
        '
        Me.btnDuplicate.Anchor = CType((System.Windows.Forms.AnchorStyles.Bottom Or System.Windows.Forms.AnchorStyles.Left), System.Windows.Forms.AnchorStyles)
        Me.btnDuplicate.BackColor = System.Drawing.Color.White
        Me.btnDuplicate.Cursor = System.Windows.Forms.Cursors.Hand
        Me.btnDuplicate.FlatAppearance.BorderColor = System.Drawing.Color.FromArgb(CType(CType(8, Byte), Integer), CType(CType(98, Byte), Integer), CType(CType(170, Byte), Integer))
        Me.btnDuplicate.FlatAppearance.MouseOverBackColor = System.Drawing.Color.Silver
        Me.btnDuplicate.FlatStyle = System.Windows.Forms.FlatStyle.Flat
        Me.btnDuplicate.Font = New System.Drawing.Font("Tahoma", 9.0!)
        Me.btnDuplicate.ForeColor = System.Drawing.Color.FromArgb(CType(CType(8, Byte), Integer), CType(CType(98, Byte), Integer), CType(CType(170, Byte), Integer))
        Me.btnDuplicate.Location = New System.Drawing.Point(653, 427)
        Me.btnDuplicate.Name = "btnDuplicate"
        Me.btnDuplicate.Size = New System.Drawing.Size(100, 28)
        Me.btnDuplicate.TabIndex = 6
        Me.btnDuplicate.Text = "Duplicate"
        Me.btnDuplicate.UseVisualStyleBackColor = False
        '
        'cntUserPrivilege
        '
        Me.AutoScaleDimensions = New System.Drawing.SizeF(6.0!, 13.0!)
        Me.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font
        Me.BackColor = System.Drawing.Color.FromArgb(CType(CType(227, Byte), Integer), CType(CType(239, Byte), Integer), CType(CType(255, Byte), Integer))
        Me.Controls.Add(Me.btnSave)
        Me.Controls.Add(Me.btnDuplicate)
        Me.Controls.Add(Me.btnUnlimited)
        Me.Controls.Add(Me.btnDenyAll)
        Me.Controls.Add(Me.btnAllowAll)
        Me.Controls.Add(Me.LstPriv)
        Me.Controls.Add(Me.LstUsers)
        Me.Controls.Add(Me.lblSteps)
        Me.Controls.Add(Me.lblUsers)
        Me.Controls.Add(Me.LabelX1)
        Me.Name = "cntUserPrivilege"
        Me.Size = New System.Drawing.Size(900, 467)
        CType(Me.LstUsers, System.ComponentModel.ISupportInitialize).EndInit()
        CType(Me.LstPriv, System.ComponentModel.ISupportInitialize).EndInit()
        Me.ResumeLayout(False)

    End Sub

    Friend WithEvents LstUsers As C1.Win.C1TrueDBGrid.C1TrueDBGrid
    Friend WithEvents LstPriv As C1.Win.C1TrueDBGrid.C1TrueDBGrid
    Friend WithEvents LabelX1 As DevComponents.DotNetBar.LabelX
    Friend WithEvents lblUsers As Windows.Forms.Label
    Friend WithEvents lblSteps As Windows.Forms.Label
    Friend WithEvents btnAllowAll As Windows.Forms.Button
    Friend WithEvents btnDenyAll As Windows.Forms.Button
    Friend WithEvents btnSave As Windows.Forms.Button
    Friend WithEvents btnUnlimited As Windows.Forms.Button
    Friend WithEvents btnDuplicate As Windows.Forms.Button
End Class
