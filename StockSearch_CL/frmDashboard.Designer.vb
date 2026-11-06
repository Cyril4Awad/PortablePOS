<Global.Microsoft.VisualBasic.CompilerServices.DesignerGenerated()>
Partial Class frmDashboard
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
        Dim resources As System.ComponentModel.ComponentResourceManager = New System.ComponentModel.ComponentResourceManager(GetType(frmDashboard))
        Me.PictureBox2 = New System.Windows.Forms.PictureBox()
        Me.CntHeader = New ApplicationsDesign.cntFormsHeader()
        Me.btnPotentialClients = New System.Windows.Forms.Button()
        Me.Panel1 = New System.Windows.Forms.Panel()
        Me.btnNegations = New System.Windows.Forms.Button()
        Me.btnDeliveryProof = New System.Windows.Forms.Button()
        Me.btnUserPrivilege = New System.Windows.Forms.Button()
        Me.btnMissingItems = New System.Windows.Forms.Button()
        Me.pnlContainer = New System.Windows.Forms.Panel()
        CType(Me.PictureBox2, System.ComponentModel.ISupportInitialize).BeginInit()
        Me.Panel1.SuspendLayout()
        Me.SuspendLayout()
        '
        'PictureBox2
        '
        Me.PictureBox2.Anchor = CType((System.Windows.Forms.AnchorStyles.Bottom Or System.Windows.Forms.AnchorStyles.Right), System.Windows.Forms.AnchorStyles)
        Me.PictureBox2.Image = CType(resources.GetObject("PictureBox2.Image"), System.Drawing.Image)
        Me.PictureBox2.ImeMode = System.Windows.Forms.ImeMode.NoControl
        Me.PictureBox2.Location = New System.Drawing.Point(11, 40)
        Me.PictureBox2.Name = "PictureBox2"
        Me.PictureBox2.Size = New System.Drawing.Size(152, 46)
        Me.PictureBox2.SizeMode = System.Windows.Forms.PictureBoxSizeMode.Zoom
        Me.PictureBox2.TabIndex = 381
        Me.PictureBox2.TabStop = False
        '
        'CntHeader
        '
        Me.CntHeader.BackColor = System.Drawing.Color.Transparent
        Me.CntHeader.Buttons = True
        Me.CntHeader.Dock = System.Windows.Forms.DockStyle.Top
        Me.CntHeader.FormTitle = "Portable POS - Application Backend"
        Me.CntHeader.Location = New System.Drawing.Point(0, 0)
        Me.CntHeader.MaximizeEnabled = False
        Me.CntHeader.MinimizeEnabled = True
        Me.CntHeader.Name = "CntHeader"
        Me.CntHeader.Size = New System.Drawing.Size(1178, 40)
        Me.CntHeader.TabIndex = 448
        '
        'btnPotentialClients
        '
        Me.btnPotentialClients.BackColor = System.Drawing.Color.White
        Me.btnPotentialClients.FlatAppearance.BorderColor = System.Drawing.Color.White
        Me.btnPotentialClients.FlatAppearance.MouseDownBackColor = System.Drawing.Color.FromArgb(CType(CType(8, Byte), Integer), CType(CType(98, Byte), Integer), CType(CType(170, Byte), Integer))
        Me.btnPotentialClients.FlatAppearance.MouseOverBackColor = System.Drawing.Color.Silver
        Me.btnPotentialClients.FlatStyle = System.Windows.Forms.FlatStyle.Flat
        Me.btnPotentialClients.Font = New System.Drawing.Font("Tahoma", 10.0!)
        Me.btnPotentialClients.ForeColor = System.Drawing.Color.FromArgb(CType(CType(8, Byte), Integer), CType(CType(98, Byte), Integer), CType(CType(170, Byte), Integer))
        Me.btnPotentialClients.Image = CType(resources.GetObject("btnPotentialClients.Image"), System.Drawing.Image)
        Me.btnPotentialClients.ImageAlign = System.Drawing.ContentAlignment.MiddleLeft
        Me.btnPotentialClients.Location = New System.Drawing.Point(4, 5)
        Me.btnPotentialClients.Name = "btnPotentialClients"
        Me.btnPotentialClients.Padding = New System.Windows.Forms.Padding(0, 0, 5, 0)
        Me.btnPotentialClients.Size = New System.Drawing.Size(167, 51)
        Me.btnPotentialClients.TabIndex = 451
        Me.btnPotentialClients.Text = "Potential Clients"
        Me.btnPotentialClients.TextAlign = System.Drawing.ContentAlignment.MiddleRight
        Me.btnPotentialClients.UseVisualStyleBackColor = False
        '
        'Panel1
        '
        Me.Panel1.BackColor = System.Drawing.Color.White
        Me.Panel1.Controls.Add(Me.btnDeliveryProof)
        Me.Panel1.Controls.Add(Me.btnNegations)
        Me.Panel1.Controls.Add(Me.btnUserPrivilege)
        Me.Panel1.Controls.Add(Me.btnMissingItems)
        Me.Panel1.Controls.Add(Me.btnPotentialClients)
        Me.Panel1.Dock = System.Windows.Forms.DockStyle.Top
        Me.Panel1.Location = New System.Drawing.Point(0, 40)
        Me.Panel1.Name = "Panel1"
        Me.Panel1.Size = New System.Drawing.Size(1178, 61)
        Me.Panel1.TabIndex = 452
        '
        'btnNegations
        '
        Me.btnNegations.FlatAppearance.BorderColor = System.Drawing.Color.White
        Me.btnNegations.FlatAppearance.MouseDownBackColor = System.Drawing.Color.FromArgb(CType(CType(8, Byte), Integer), CType(CType(98, Byte), Integer), CType(CType(170, Byte), Integer))
        Me.btnNegations.FlatAppearance.MouseOverBackColor = System.Drawing.Color.Silver
        Me.btnNegations.FlatStyle = System.Windows.Forms.FlatStyle.Flat
        Me.btnNegations.Font = New System.Drawing.Font("Tahoma", 10.0!)
        Me.btnNegations.ForeColor = System.Drawing.Color.FromArgb(CType(CType(8, Byte), Integer), CType(CType(98, Byte), Integer), CType(CType(170, Byte), Integer))
        Me.btnNegations.Image = CType(resources.GetObject("btnNegations.Image"), System.Drawing.Image)
        Me.btnNegations.ImageAlign = System.Drawing.ContentAlignment.MiddleLeft
        Me.btnNegations.Location = New System.Drawing.Point(532, 5)
        Me.btnNegations.Name = "btnNegations"
        Me.btnNegations.Padding = New System.Windows.Forms.Padding(0, 0, 5, 0)
        Me.btnNegations.Size = New System.Drawing.Size(167, 51)
        Me.btnNegations.TabIndex = 454
        Me.btnNegations.Text = "Negations"
        Me.btnNegations.UseVisualStyleBackColor = True
        '
        'btnDeliveryProof
        '
        Me.btnDeliveryProof.FlatAppearance.BorderColor = System.Drawing.Color.White
        Me.btnDeliveryProof.FlatAppearance.MouseDownBackColor = System.Drawing.Color.FromArgb(CType(CType(8, Byte), Integer), CType(CType(98, Byte), Integer), CType(CType(170, Byte), Integer))
        Me.btnDeliveryProof.FlatAppearance.MouseOverBackColor = System.Drawing.Color.Silver
        Me.btnDeliveryProof.FlatStyle = System.Windows.Forms.FlatStyle.Flat
        Me.btnDeliveryProof.Font = New System.Drawing.Font("Tahoma", 10.0!)
        Me.btnDeliveryProof.ForeColor = System.Drawing.Color.FromArgb(CType(CType(8, Byte), Integer), CType(CType(98, Byte), Integer), CType(CType(170, Byte), Integer))
        Me.btnDeliveryProof.Image = CType(resources.GetObject("btnDeliveryProof.Image"), System.Drawing.Image)
        Me.btnDeliveryProof.ImageAlign = System.Drawing.ContentAlignment.MiddleLeft
        Me.btnDeliveryProof.Location = New System.Drawing.Point(705, 5)
        Me.btnDeliveryProof.Name = "btnDeliveryProof"
        Me.btnDeliveryProof.Padding = New System.Windows.Forms.Padding(0, 0, 5, 0)
        Me.btnDeliveryProof.Size = New System.Drawing.Size(167, 51)
        Me.btnDeliveryProof.TabIndex = 455
        Me.btnDeliveryProof.Text = "Delivery Proof"
        Me.btnDeliveryProof.TextAlign = System.Drawing.ContentAlignment.MiddleRight
        Me.btnDeliveryProof.UseVisualStyleBackColor = True
        '
        'btnUserPrivilege
        '
        Me.btnUserPrivilege.FlatAppearance.BorderColor = System.Drawing.Color.White
        Me.btnUserPrivilege.FlatAppearance.MouseDownBackColor = System.Drawing.Color.FromArgb(CType(CType(8, Byte), Integer), CType(CType(98, Byte), Integer), CType(CType(170, Byte), Integer))
        Me.btnUserPrivilege.FlatAppearance.MouseOverBackColor = System.Drawing.Color.Silver
        Me.btnUserPrivilege.FlatStyle = System.Windows.Forms.FlatStyle.Flat
        Me.btnUserPrivilege.Font = New System.Drawing.Font("Tahoma", 10.0!)
        Me.btnUserPrivilege.ForeColor = System.Drawing.Color.FromArgb(CType(CType(8, Byte), Integer), CType(CType(98, Byte), Integer), CType(CType(170, Byte), Integer))
        Me.btnUserPrivilege.Image = CType(resources.GetObject("btnUserPrivilege.Image"), System.Drawing.Image)
        Me.btnUserPrivilege.ImageAlign = System.Drawing.ContentAlignment.MiddleLeft
        Me.btnUserPrivilege.Location = New System.Drawing.Point(350, 5)
        Me.btnUserPrivilege.Name = "btnUserPrivilege"
        Me.btnUserPrivilege.Padding = New System.Windows.Forms.Padding(0, 0, 5, 0)
        Me.btnUserPrivilege.Size = New System.Drawing.Size(167, 51)
        Me.btnUserPrivilege.TabIndex = 453
        Me.btnUserPrivilege.Text = "User Privilege"
        Me.btnUserPrivilege.TextAlign = System.Drawing.ContentAlignment.MiddleRight
        Me.btnUserPrivilege.UseVisualStyleBackColor = True
        '
        'btnMissingItems
        '
        Me.btnMissingItems.FlatAppearance.BorderColor = System.Drawing.Color.White
        Me.btnMissingItems.FlatAppearance.MouseDownBackColor = System.Drawing.Color.FromArgb(CType(CType(8, Byte), Integer), CType(CType(98, Byte), Integer), CType(CType(170, Byte), Integer))
        Me.btnMissingItems.FlatAppearance.MouseOverBackColor = System.Drawing.Color.Silver
        Me.btnMissingItems.FlatStyle = System.Windows.Forms.FlatStyle.Flat
        Me.btnMissingItems.Font = New System.Drawing.Font("Tahoma", 10.0!)
        Me.btnMissingItems.ForeColor = System.Drawing.Color.FromArgb(CType(CType(8, Byte), Integer), CType(CType(98, Byte), Integer), CType(CType(170, Byte), Integer))
        Me.btnMissingItems.Image = CType(resources.GetObject("btnMissingItems.Image"), System.Drawing.Image)
        Me.btnMissingItems.ImageAlign = System.Drawing.ContentAlignment.MiddleLeft
        Me.btnMissingItems.Location = New System.Drawing.Point(177, 5)
        Me.btnMissingItems.Name = "btnMissingItems"
        Me.btnMissingItems.Padding = New System.Windows.Forms.Padding(0, 0, 5, 0)
        Me.btnMissingItems.Size = New System.Drawing.Size(167, 51)
        Me.btnMissingItems.TabIndex = 452
        Me.btnMissingItems.Text = "Missing Items"
        Me.btnMissingItems.TextAlign = System.Drawing.ContentAlignment.MiddleRight
        Me.btnMissingItems.UseVisualStyleBackColor = True
        '
        'pnlContainer
        '
        Me.pnlContainer.Anchor = CType((((System.Windows.Forms.AnchorStyles.Top Or System.Windows.Forms.AnchorStyles.Bottom) _
            Or System.Windows.Forms.AnchorStyles.Left) _
            Or System.Windows.Forms.AnchorStyles.Right), System.Windows.Forms.AnchorStyles)
        Me.pnlContainer.Location = New System.Drawing.Point(10, 113)
        Me.pnlContainer.Name = "pnlContainer"
        Me.pnlContainer.Size = New System.Drawing.Size(1158, 486)
        Me.pnlContainer.TabIndex = 453
        '
        'frmDashboard
        '
        Me.AutoScaleDimensions = New System.Drawing.SizeF(6.0!, 13.0!)
        Me.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font
        Me.BackColor = System.Drawing.Color.FromArgb(CType(CType(227, Byte), Integer), CType(CType(239, Byte), Integer), CType(CType(255, Byte), Integer))
        Me.ClientSize = New System.Drawing.Size(1178, 610)
        Me.Controls.Add(Me.pnlContainer)
        Me.Controls.Add(Me.Panel1)
        Me.Controls.Add(Me.PictureBox2)
        Me.Controls.Add(Me.CntHeader)
        Me.FormBorderStyle = System.Windows.Forms.FormBorderStyle.None
        Me.KeyPreview = True
        Me.MaximizeBox = False
        Me.MinimizeBox = False
        Me.Name = "frmDashboard"
        Me.StartPosition = System.Windows.Forms.FormStartPosition.CenterScreen
        Me.Text = "frmDashboard"
        Me.WindowState = System.Windows.Forms.FormWindowState.Maximized
        CType(Me.PictureBox2, System.ComponentModel.ISupportInitialize).EndInit()
        Me.Panel1.ResumeLayout(False)
        Me.ResumeLayout(False)

    End Sub

    Friend WithEvents PictureBox2 As Windows.Forms.PictureBox
    Friend WithEvents CntHeader As ApplicationsDesign.cntFormsHeader
    Friend WithEvents btnPotentialClients As Windows.Forms.Button
    Friend WithEvents Panel1 As Windows.Forms.Panel
    Friend WithEvents pnlContainer As Windows.Forms.Panel
    Friend WithEvents btnMissingItems As Windows.Forms.Button
    Friend WithEvents btnUserPrivilege As Windows.Forms.Button
    Friend WithEvents btnNegations As Windows.Forms.Button
    Friend WithEvents btnDeliveryProof As Windows.Forms.Button
End Class
