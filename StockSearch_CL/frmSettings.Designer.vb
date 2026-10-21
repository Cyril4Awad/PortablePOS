<Global.Microsoft.VisualBasic.CompilerServices.DesignerGenerated()>
Partial Class frmSettings
    Inherits System.Windows.Forms.Form

    'Form overrides dispose to clean up the component list.
    <System.Diagnostics.DebuggerNonUserCode()>
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
    <System.Diagnostics.DebuggerStepThrough()>
    Private Sub InitializeComponent()
        Me.CntHeader = New ApplicationsDesign.cntFormsHeader()
        Me.pnlForm = New System.Windows.Forms.Panel()
        Me.SuspendLayout()
        '
        'CntHeader
        '
        Me.CntHeader.BackColor = System.Drawing.Color.Transparent
        Me.CntHeader.Buttons = True
        Me.CntHeader.Dock = System.Windows.Forms.DockStyle.Top
        Me.CntHeader.FormTitle = "Stock Search"
        Me.CntHeader.Location = New System.Drawing.Point(0, 0)
        Me.CntHeader.MaximizeEnabled = False
        Me.CntHeader.MinimizeEnabled = False
        Me.CntHeader.Name = "CntHeader"
        Me.CntHeader.Size = New System.Drawing.Size(900, 40)
        Me.CntHeader.TabIndex = 449
        '
        'pnlForm
        '
        Me.pnlForm.Anchor = CType((((System.Windows.Forms.AnchorStyles.Top Or System.Windows.Forms.AnchorStyles.Bottom) _
            Or System.Windows.Forms.AnchorStyles.Left) _
            Or System.Windows.Forms.AnchorStyles.Right), System.Windows.Forms.AnchorStyles)
        Me.pnlForm.BackColor = System.Drawing.Color.FromArgb(CType(CType(227, Byte), Integer), CType(CType(239, Byte), Integer), CType(CType(255, Byte), Integer))
        Me.pnlForm.Location = New System.Drawing.Point(6, 46)
        Me.pnlForm.Name = "pnlForm"
        Me.pnlForm.Size = New System.Drawing.Size(887, 507)
        Me.pnlForm.TabIndex = 450
        '
        'frmSettings
        '
        Me.AutoScaleDimensions = New System.Drawing.SizeF(6.0!, 13.0!)
        Me.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font
        Me.BackColor = System.Drawing.Color.White
        Me.ClientSize = New System.Drawing.Size(900, 560)
        Me.ControlBox = False
        Me.Controls.Add(Me.pnlForm)
        Me.Controls.Add(Me.CntHeader)
        Me.FormBorderStyle = System.Windows.Forms.FormBorderStyle.None
        Me.Name = "frmSettings"
        Me.StartPosition = System.Windows.Forms.FormStartPosition.CenterScreen
        Me.Text = "Settings"
        Me.ResumeLayout(False)

    End Sub

    Friend WithEvents CntHeader As ApplicationsDesign.cntFormsHeader
    Friend WithEvents pnlForm As Windows.Forms.Panel
End Class
