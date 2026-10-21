<Global.Microsoft.VisualBasic.CompilerServices.DesignerGenerated()>
Partial Class cntUpload
    Inherits System.Windows.Forms.UserControl

    'UserControl overrides dispose to clean up the component list.
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
        Dim resources As System.ComponentModel.ComponentResourceManager = New System.ComponentModel.ComponentResourceManager(GetType(cntUpload))
        Me.lblHeader = New System.Windows.Forms.Label()
        Me.lblClient = New System.Windows.Forms.Label()
        Me.lstItems = New C1.Win.C1TrueDBGrid.C1TrueDBGrid()
        Me.lblTotal = New System.Windows.Forms.Label()
        Me.lblDateStamp = New System.Windows.Forms.Label()
        CType(Me.lstItems, System.ComponentModel.ISupportInitialize).BeginInit()
        Me.SuspendLayout()
        '
        'lblHeader
        '
        Me.lblHeader.BackColor = System.Drawing.Color.FromArgb(CType(CType(8, Byte), Integer), CType(CType(98, Byte), Integer), CType(CType(170, Byte), Integer))
        Me.lblHeader.Dock = System.Windows.Forms.DockStyle.Top
        Me.lblHeader.Font = New System.Drawing.Font("Tahoma", 12.0!)
        Me.lblHeader.ForeColor = System.Drawing.Color.White
        Me.lblHeader.Location = New System.Drawing.Point(0, 0)
        Me.lblHeader.Name = "lblHeader"
        Me.lblHeader.Padding = New System.Windows.Forms.Padding(3, 2, 0, 0)
        Me.lblHeader.Size = New System.Drawing.Size(837, 27)
        Me.lblHeader.TabIndex = 0
        Me.lblHeader.Text = "Upl Number - User"
        '
        'lblClient
        '
        Me.lblClient.Anchor = CType(((System.Windows.Forms.AnchorStyles.Bottom Or System.Windows.Forms.AnchorStyles.Left) _
            Or System.Windows.Forms.AnchorStyles.Right), System.Windows.Forms.AnchorStyles)
        Me.lblClient.BackColor = System.Drawing.Color.White
        Me.lblClient.BorderStyle = System.Windows.Forms.BorderStyle.FixedSingle
        Me.lblClient.Font = New System.Drawing.Font("Tahoma", 16.0!)
        Me.lblClient.ForeColor = System.Drawing.Color.FromArgb(CType(CType(8, Byte), Integer), CType(CType(98, Byte), Integer), CType(CType(170, Byte), Integer))
        Me.lblClient.Location = New System.Drawing.Point(8, 35)
        Me.lblClient.Name = "lblClient"
        Me.lblClient.Padding = New System.Windows.Forms.Padding(2)
        Me.lblClient.Size = New System.Drawing.Size(821, 37)
        Me.lblClient.TabIndex = 1
        '
        'lstItems
        '
        Me.lstItems.AllowRowSizing = C1.Win.C1TrueDBGrid.RowSizingEnum.None
        Me.lstItems.AllowUpdate = False
        Me.lstItems.AllowUpdateOnBlur = False
        Me.lstItems.Anchor = CType((((System.Windows.Forms.AnchorStyles.Top Or System.Windows.Forms.AnchorStyles.Bottom) _
            Or System.Windows.Forms.AnchorStyles.Left) _
            Or System.Windows.Forms.AnchorStyles.Right), System.Windows.Forms.AnchorStyles)
        Me.lstItems.BackgroundImageLayout = System.Windows.Forms.ImageLayout.Stretch
        Me.lstItems.Caption = "Items"
        Me.lstItems.CaptionHeight = 17
        Me.lstItems.ColumnFooters = True
        Me.lstItems.DirectionAfterEnter = C1.Win.C1TrueDBGrid.DirectionAfterEnterEnum.MoveNone
        Me.lstItems.ExtendRightColumn = True
        Me.lstItems.FetchRowStyles = True
        Me.lstItems.FilterBar = True
        Me.lstItems.ForeColor = System.Drawing.SystemColors.ControlText
        Me.lstItems.GroupByCaption = "Drag a column header here to group by that column"
        Me.lstItems.Images.Add(CType(resources.GetObject("lstItems.Images"), System.Drawing.Image))
        Me.lstItems.Location = New System.Drawing.Point(8, 71)
        Me.lstItems.MultiSelect = C1.Win.C1TrueDBGrid.MultiSelectEnum.None
        Me.lstItems.Name = "lstItems"
        Me.lstItems.PreviewInfo.Location = New System.Drawing.Point(0, 0)
        Me.lstItems.PreviewInfo.Size = New System.Drawing.Size(0, 0)
        Me.lstItems.PreviewInfo.ZoomFactor = 75.0R
        Me.lstItems.PrintInfo.MeasurementDevice = C1.Win.C1TrueDBGrid.PrintInfo.MeasurementDeviceEnum.Screen
        Me.lstItems.PrintInfo.MeasurementPrinterName = Nothing
        Me.lstItems.RowHeight = 18
        Me.lstItems.Size = New System.Drawing.Size(821, 300)
        Me.lstItems.TabIndex = 4
        Me.lstItems.TabStop = False
        Me.lstItems.Text = "C1TrueDBGrid1"
        Me.lstItems.UseCompatibleTextRendering = False
        Me.lstItems.PropBag = resources.GetString("lstItems.PropBag")
        '
        'lblTotal
        '
        Me.lblTotal.Anchor = CType(((System.Windows.Forms.AnchorStyles.Bottom Or System.Windows.Forms.AnchorStyles.Left) _
            Or System.Windows.Forms.AnchorStyles.Right), System.Windows.Forms.AnchorStyles)
        Me.lblTotal.BackColor = System.Drawing.Color.White
        Me.lblTotal.BorderStyle = System.Windows.Forms.BorderStyle.FixedSingle
        Me.lblTotal.Font = New System.Drawing.Font("Tahoma", 10.0!, System.Drawing.FontStyle.Bold)
        Me.lblTotal.ForeColor = System.Drawing.Color.FromArgb(CType(CType(8, Byte), Integer), CType(CType(98, Byte), Integer), CType(CType(170, Byte), Integer))
        Me.lblTotal.Location = New System.Drawing.Point(8, 371)
        Me.lblTotal.Name = "lblTotal"
        Me.lblTotal.Padding = New System.Windows.Forms.Padding(2)
        Me.lblTotal.Size = New System.Drawing.Size(821, 33)
        Me.lblTotal.TabIndex = 5
        Me.lblTotal.Text = "Total = 0"
        Me.lblTotal.TextAlign = System.Drawing.ContentAlignment.MiddleRight
        '
        'lblDateStamp
        '
        Me.lblDateStamp.Anchor = CType(((System.Windows.Forms.AnchorStyles.Bottom Or System.Windows.Forms.AnchorStyles.Left) _
            Or System.Windows.Forms.AnchorStyles.Right), System.Windows.Forms.AnchorStyles)
        Me.lblDateStamp.BackColor = System.Drawing.Color.White
        Me.lblDateStamp.BorderStyle = System.Windows.Forms.BorderStyle.FixedSingle
        Me.lblDateStamp.ForeColor = System.Drawing.Color.FromArgb(CType(CType(8, Byte), Integer), CType(CType(98, Byte), Integer), CType(CType(170, Byte), Integer))
        Me.lblDateStamp.Location = New System.Drawing.Point(8, 409)
        Me.lblDateStamp.Name = "lblDateStamp"
        Me.lblDateStamp.Padding = New System.Windows.Forms.Padding(2)
        Me.lblDateStamp.Size = New System.Drawing.Size(821, 19)
        Me.lblDateStamp.TabIndex = 6
        Me.lblDateStamp.Text = "Date Stamp"
        Me.lblDateStamp.TextAlign = System.Drawing.ContentAlignment.MiddleRight
        '
        'cntUpload
        '
        Me.AutoScaleDimensions = New System.Drawing.SizeF(6.0!, 13.0!)
        Me.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font
        Me.BackColor = System.Drawing.Color.Gainsboro
        Me.Controls.Add(Me.lblDateStamp)
        Me.Controls.Add(Me.lblTotal)
        Me.Controls.Add(Me.lstItems)
        Me.Controls.Add(Me.lblClient)
        Me.Controls.Add(Me.lblHeader)
        Me.Name = "cntUpload"
        Me.Size = New System.Drawing.Size(837, 437)
        CType(Me.lstItems, System.ComponentModel.ISupportInitialize).EndInit()
        Me.ResumeLayout(False)

    End Sub

    Friend WithEvents lblHeader As Windows.Forms.Label
    Friend WithEvents lblClient As Windows.Forms.Label
    Friend WithEvents lstItems As C1.Win.C1TrueDBGrid.C1TrueDBGrid
    Friend WithEvents lblTotal As Windows.Forms.Label
    Friend WithEvents lblDateStamp As Windows.Forms.Label
End Class
