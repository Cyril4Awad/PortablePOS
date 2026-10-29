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
        Me.LstPriv = New C1.Win.C1TrueDBGrid.C1TrueDBGrid()
        Me.LabelX1 = New DevComponents.DotNetBar.LabelX()
        Me.Label7 = New System.Windows.Forms.Label()
        Me.cmbUser = New System.Windows.Forms.ComboBox()
        Me.btnAllowAll = New System.Windows.Forms.Button()
        Me.btnDenyAll = New System.Windows.Forms.Button()
        Me.btnSave = New System.Windows.Forms.Button()
        Me.btnUnlimited = New System.Windows.Forms.Button()
        Me.btnDuplicate = New System.Windows.Forms.Button()
        CType(Me.LstPriv, System.ComponentModel.ISupportInitialize).BeginInit()
        Me.SuspendLayout()
        '
        'LstPriv
        '
        Me.LstPriv.AllowColMove = False
        Me.LstPriv.AllowColSelect = False
        Me.LstPriv.AllowFilter = False
        Me.LstPriv.AllowRowSizing = C1.Win.C1TrueDBGrid.RowSizingEnum.None
        Me.LstPriv.AllowSort = False
        Me.LstPriv.AllowUpdateOnBlur = True
        Me.LstPriv.AlternatingRows = False
        Me.LstPriv.Anchor = CType((System.Windows.Forms.AnchorStyles.Top Or System.Windows.Forms.AnchorStyles.Bottom), System.Windows.Forms.AnchorStyles)
        Me.LstPriv.BackColor = System.Drawing.Color.White
        Me.LstPriv.CaptionHeight = 17
        Me.LstPriv.EmptyRows = True
        Me.LstPriv.ExtendRightColumn = False
        Me.LstPriv.FetchRowStyles = True
        Me.LstPriv.FilterBar = True
        Me.LstPriv.Font = New System.Drawing.Font("Microsoft Sans Serif", 8.25!, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point, CType(0, Byte))
        Me.LstPriv.GroupByCaption = "Drag a column header here to group by that column"
        Me.LstPriv.Location = New System.Drawing.Point(32, 131)
        Me.LstPriv.MultiSelect = C1.Win.C1TrueDBGrid.MultiSelectEnum.None
        Me.LstPriv.Name = "LstPriv"
        Me.LstPriv.PreviewInfo.Location = New System.Drawing.Point(0, 0)
        Me.LstPriv.PreviewInfo.Size = New System.Drawing.Size(0, 0)
        Me.LstPriv.PreviewInfo.ZoomFactor = 75.0R
        Me.LstPriv.PrintInfo.MeasurementDevice = C1.Win.C1TrueDBGrid.PrintInfo.MeasurementDeviceEnum.Screen
        Me.LstPriv.PrintInfo.MeasurementPrinterName = Nothing
        Me.LstPriv.RowHeight = 18
        Me.LstPriv.Size = New System.Drawing.Size(550, 278)
        Me.LstPriv.TabIndex = 5
        Me.LstPriv.Text = "C1TrueDBGrid1"
        Me.LstPriv.UseCompatibleTextRendering = False
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
        Me.LabelX1.TabIndex = 405
        Me.LabelX1.Text = "User Privilege"
        '
        'Label7
        '
        Me.Label7.BackColor = System.Drawing.Color.Transparent
        Me.Label7.Font = New System.Drawing.Font("Microsoft Sans Serif", 9.0!, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point, CType(0, Byte))
        Me.Label7.ForeColor = System.Drawing.Color.Navy
        Me.Label7.Location = New System.Drawing.Point(29, 76)
        Me.Label7.Name = "Label7"
        Me.Label7.Size = New System.Drawing.Size(45, 19)
        Me.Label7.TabIndex = 406
        Me.Label7.Text = "User"
        '
        'cmbUser
        '
        Me.cmbUser.DropDownStyle = System.Windows.Forms.ComboBoxStyle.DropDownList
        Me.cmbUser.Font = New System.Drawing.Font("Microsoft Sans Serif", 8.25!)
        Me.cmbUser.FormattingEnabled = True
        Me.cmbUser.Location = New System.Drawing.Point(80, 75)
        Me.cmbUser.Name = "cmbUser"
        Me.cmbUser.Size = New System.Drawing.Size(230, 21)
        Me.cmbUser.TabIndex = 1
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
        Me.btnAllowAll.Location = New System.Drawing.Point(32, 412)
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
        Me.btnDenyAll.Location = New System.Drawing.Point(138, 412)
        Me.btnDenyAll.Name = "btnDenyAll"
        Me.btnDenyAll.Size = New System.Drawing.Size(100, 28)
        Me.btnDenyAll.TabIndex = 4
        Me.btnDenyAll.Text = "Deny All"
        Me.btnDenyAll.UseVisualStyleBackColor = False
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
        Me.btnUnlimited.Location = New System.Drawing.Point(244, 412)
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
        Me.btnDuplicate.Location = New System.Drawing.Point(360, 412)
        Me.btnDuplicate.Name = "btnDuplicate"
        Me.btnDuplicate.Size = New System.Drawing.Size(100, 28)
        Me.btnDuplicate.TabIndex = 6
        Me.btnDuplicate.Text = "Duplicate"
        Me.btnDuplicate.UseVisualStyleBackColor = False
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
        Me.btnSave.Location = New System.Drawing.Point(679, 412)
        Me.btnSave.Name = "btnSave"
        Me.btnSave.Size = New System.Drawing.Size(100, 28)
        Me.btnSave.TabIndex = 6
        Me.btnSave.Text = "Save"
        Me.btnSave.UseVisualStyleBackColor = False
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
        Me.Controls.Add(Me.cmbUser)
        Me.Controls.Add(Me.Label7)
        Me.Controls.Add(Me.LabelX1)
        Me.Name = "cntUserPrivilege"
        Me.Size = New System.Drawing.Size(812, 452)
        CType(Me.LstPriv, System.ComponentModel.ISupportInitialize).EndInit()
        Me.ResumeLayout(False)

    End Sub

    Friend WithEvents LstPriv As C1.Win.C1TrueDBGrid.C1TrueDBGrid
    Friend WithEvents LabelX1 As DevComponents.DotNetBar.LabelX
    Friend WithEvents Label7 As Windows.Forms.Label
    Friend WithEvents cmbUser As Windows.Forms.ComboBox
    Friend WithEvents btnAllowAll As Windows.Forms.Button
    Friend WithEvents btnDenyAll As Windows.Forms.Button
    Friend WithEvents btnSave As Windows.Forms.Button
    Friend WithEvents btnUnlimited As Windows.Forms.Button
    Friend WithEvents btnDuplicate As Windows.Forms.Button
End Class
