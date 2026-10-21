
'Company : App
'Module : Supper Store 
'Usage : Back Office
'Type : Form
'Created : 21/12/2005 
'Created by : Rabih Abou Antoun
'Last Update : 18/08/2016 
'Modified by : Toufic Youssef
'-----------------------------------------
Imports System.Data.SqlClient
Imports C1.Win.C1TrueDBGrid
Imports CrystalDecisions.CrystalReports.Engine
Imports UserControls.ERPHelp
Imports DevComponents.DotNetBar.Controls


Public Class frmSelectStkItems
    Inherits DevComponents.DotNetBar.OfficeForm

#Region " Windows Form Designer generated code "

    Public Sub New()
        MyBase.New()

        'This call is required by the Windows Form Designer.
        InitializeComponent()

        'Add any initialization after the InitializeComponent() call

    End Sub

    'Form overrides dispose to clean up the component list.
    Protected Overloads Overrides Sub Dispose(ByVal disposing As Boolean)
        If disposing Then
            If Not (components Is Nothing) Then
                components.Dispose()
            End If
        End If
        MyBase.Dispose(disposing)
    End Sub

    'Required by the Windows Form Designer
    Private components As System.ComponentModel.IContainer

    'NOTE: The following procedure is required by the Windows Form Designer
    'It can be modified using the Windows Form Designer.  
    'Do not modify it using the code editor.
    Friend WithEvents NullConMenu As System.Windows.Forms.ContextMenu
    Friend WithEvents ConMenu As System.Windows.Forms.ContextMenu
    Friend WithEvents ctmCopy As System.Windows.Forms.MenuItem
    Friend WithEvents ctmCut As System.Windows.Forms.MenuItem
    Friend WithEvents ctmPast As System.Windows.Forms.MenuItem
    Friend WithEvents Clock As System.Windows.Forms.Timer
    Friend WithEvents tdbDetails As C1.Win.C1TrueDBGrid.C1TrueDBGrid
    Friend WithEvents btnUnCheck As System.Windows.Forms.Button
    Friend WithEvents btnSave As System.Windows.Forms.Button
    Friend WithEvents btnCheck As System.Windows.Forms.Button
    Friend WithEvents btnShow As System.Windows.Forms.Button
    Friend WithEvents btnClose As System.Windows.Forms.Button
    Friend WithEvents btnClear As System.Windows.Forms.Button
    Friend WithEvents Label58 As System.Windows.Forms.Label
    Friend WithEvents GroupBox1 As System.Windows.Forms.GroupBox
    Friend WithEvents txtMchName1 As System.Windows.Forms.TextBox
    Friend WithEvents txtMchName2 As System.Windows.Forms.TextBox
    Friend WithEvents txtMchName3 As System.Windows.Forms.TextBox
    Friend WithEvents txtMchName4 As System.Windows.Forms.TextBox
    Friend WithEvents Label2 As System.Windows.Forms.Label
    Friend WithEvents Label3 As System.Windows.Forms.Label
    Friend WithEvents Label4 As System.Windows.Forms.Label
    Friend WithEvents Label5 As System.Windows.Forms.Label
    Friend WithEvents txtMemo2 As System.Windows.Forms.TextBox
    Friend WithEvents Label56 As System.Windows.Forms.Label
    Friend WithEvents lblPosition As System.Windows.Forms.Label
    Friend WithEvents Label6 As System.Windows.Forms.Label
    Friend WithEvents txtItmNo As System.Windows.Forms.Label
    Friend WithEvents GroupBox4 As System.Windows.Forms.GroupBox
    Friend WithEvents rbGen As System.Windows.Forms.RadioButton
    Friend WithEvents rbDateRng As System.Windows.Forms.RadioButton
    Friend WithEvents btnSearch As System.Windows.Forms.Button
    Friend WithEvents txtStartDate As C1.Win.C1Input.C1DateEdit
    Friend WithEvents txtEndDate As C1.Win.C1Input.C1DateEdit
    Friend WithEvents ErrProvider As System.Windows.Forms.ErrorProvider
    Friend WithEvents txtDatePhyCnt As System.Windows.Forms.Label
    Friend WithEvents Label7 As System.Windows.Forms.Label
    Friend WithEvents Label8 As System.Windows.Forms.Label
    Friend WithEvents Label9 As System.Windows.Forms.Label
    Friend WithEvents GroupBox2 As System.Windows.Forms.GroupBox
    Friend WithEvents rb2 As System.Windows.Forms.RadioButton
    Friend WithEvents rb1 As System.Windows.Forms.RadioButton
    Friend WithEvents GroupBox3 As System.Windows.Forms.GroupBox
    Friend WithEvents Label11 As System.Windows.Forms.Label
    Friend WithEvents chkBook As System.Windows.Forms.CheckBox
    Friend WithEvents txtBookCat As System.Windows.Forms.TextBox
    Friend WithEvents btnBookCat As System.Windows.Forms.Button
    Friend WithEvents tdbBookCat As C1.Win.C1TrueDBGrid.C1TrueDBGrid
    Friend WithEvents chekall As System.Windows.Forms.Button
    Friend WithEvents pnlclacc As System.Windows.Forms.Panel
    Friend WithEvents txtGeneralSearch As DevComponents.DotNetBar.Controls.TextBoxX
    Friend WithEvents Label1 As System.Windows.Forms.Label
    Friend WithEvents Panel1 As System.Windows.Forms.Panel
    Friend WithEvents ImgLst As System.Windows.Forms.ImageList
    Friend WithEvents Label10 As System.Windows.Forms.Label
    Friend WithEvents stBar As System.Windows.Forms.StatusBar
    Friend WithEvents stbDate As System.Windows.Forms.StatusBarPanel
    Friend WithEvents stbRemark As System.Windows.Forms.StatusBarPanel
    Friend WithEvents stbLocation As System.Windows.Forms.StatusBarPanel
    Friend WithEvents stbTime As System.Windows.Forms.StatusBarPanel
    Friend WithEvents TxtMemo1 As System.Windows.Forms.TextBox
    Friend WithEvents FlowLayoutPanel1 As System.Windows.Forms.FlowLayoutPanel
    Friend WithEvents Panel2 As System.Windows.Forms.Panel
    Friend WithEvents Panel3 As System.Windows.Forms.Panel
    Friend WithEvents pnlHead As DevComponents.DotNetBar.PanelEx
    Friend WithEvents lblTitle As DevComponents.DotNetBar.Controls.ReflectionLabel
    Friend WithEvents ChkBarcode As System.Windows.Forms.CheckBox
    Friend WithEvents Label13 As System.Windows.Forms.Label
    Friend WithEvents txtMchName6 As System.Windows.Forms.TextBox
    Friend WithEvents Label12 As System.Windows.Forms.Label
    Friend WithEvents txtMchName5 As System.Windows.Forms.TextBox
    Friend WithEvents FlowLayoutPanel2 As FlowLayoutPanel
    Friend WithEvents btnImportTransfer As Button
    Friend WithEvents ChkNoBooks As System.Windows.Forms.CheckBox
    <System.Diagnostics.DebuggerStepThrough()> Private Sub InitializeComponent()
        Me.components = New System.ComponentModel.Container()
        Dim Style1 As C1.Win.C1TrueDBGrid.Style = New C1.Win.C1TrueDBGrid.Style()
        Dim Style2 As C1.Win.C1TrueDBGrid.Style = New C1.Win.C1TrueDBGrid.Style()
        Dim Style3 As C1.Win.C1TrueDBGrid.Style = New C1.Win.C1TrueDBGrid.Style()
        Dim Style4 As C1.Win.C1TrueDBGrid.Style = New C1.Win.C1TrueDBGrid.Style()
        Dim Style5 As C1.Win.C1TrueDBGrid.Style = New C1.Win.C1TrueDBGrid.Style()
        Dim Style6 As C1.Win.C1TrueDBGrid.Style = New C1.Win.C1TrueDBGrid.Style()
        Dim Style7 As C1.Win.C1TrueDBGrid.Style = New C1.Win.C1TrueDBGrid.Style()
        Dim Style8 As C1.Win.C1TrueDBGrid.Style = New C1.Win.C1TrueDBGrid.Style()
        Dim resources As System.ComponentModel.ComponentResourceManager = New System.ComponentModel.ComponentResourceManager(GetType(frmSelectStkItems))
        Dim Style9 As C1.Win.C1TrueDBGrid.Style = New C1.Win.C1TrueDBGrid.Style()
        Dim Style10 As C1.Win.C1TrueDBGrid.Style = New C1.Win.C1TrueDBGrid.Style()
        Dim Style11 As C1.Win.C1TrueDBGrid.Style = New C1.Win.C1TrueDBGrid.Style()
        Dim Style12 As C1.Win.C1TrueDBGrid.Style = New C1.Win.C1TrueDBGrid.Style()
        Dim Style13 As C1.Win.C1TrueDBGrid.Style = New C1.Win.C1TrueDBGrid.Style()
        Dim Style14 As C1.Win.C1TrueDBGrid.Style = New C1.Win.C1TrueDBGrid.Style()
        Dim Style15 As C1.Win.C1TrueDBGrid.Style = New C1.Win.C1TrueDBGrid.Style()
        Dim Style16 As C1.Win.C1TrueDBGrid.Style = New C1.Win.C1TrueDBGrid.Style()
        Dim Style17 As C1.Win.C1TrueDBGrid.Style = New C1.Win.C1TrueDBGrid.Style()
        Dim Style18 As C1.Win.C1TrueDBGrid.Style = New C1.Win.C1TrueDBGrid.Style()
        Dim Style19 As C1.Win.C1TrueDBGrid.Style = New C1.Win.C1TrueDBGrid.Style()
        Dim Style20 As C1.Win.C1TrueDBGrid.Style = New C1.Win.C1TrueDBGrid.Style()
        Dim Style21 As C1.Win.C1TrueDBGrid.Style = New C1.Win.C1TrueDBGrid.Style()
        Dim Style22 As C1.Win.C1TrueDBGrid.Style = New C1.Win.C1TrueDBGrid.Style()
        Dim Style23 As C1.Win.C1TrueDBGrid.Style = New C1.Win.C1TrueDBGrid.Style()
        Dim Style24 As C1.Win.C1TrueDBGrid.Style = New C1.Win.C1TrueDBGrid.Style()
        Dim Style25 As C1.Win.C1TrueDBGrid.Style = New C1.Win.C1TrueDBGrid.Style()
        Dim Style26 As C1.Win.C1TrueDBGrid.Style = New C1.Win.C1TrueDBGrid.Style()
        Me.NullConMenu = New System.Windows.Forms.ContextMenu()
        Me.ConMenu = New System.Windows.Forms.ContextMenu()
        Me.ctmCopy = New System.Windows.Forms.MenuItem()
        Me.ctmCut = New System.Windows.Forms.MenuItem()
        Me.ctmPast = New System.Windows.Forms.MenuItem()
        Me.Clock = New System.Windows.Forms.Timer(Me.components)
        Me.tdbDetails = New C1.Win.C1TrueDBGrid.C1TrueDBGrid()
        Me.btnUnCheck = New System.Windows.Forms.Button()
        Me.btnSave = New System.Windows.Forms.Button()
        Me.btnCheck = New System.Windows.Forms.Button()
        Me.btnShow = New System.Windows.Forms.Button()
        Me.btnClose = New System.Windows.Forms.Button()
        Me.btnClear = New System.Windows.Forms.Button()
        Me.Label58 = New System.Windows.Forms.Label()
        Me.GroupBox1 = New System.Windows.Forms.GroupBox()
        Me.Label13 = New System.Windows.Forms.Label()
        Me.txtMchName6 = New System.Windows.Forms.TextBox()
        Me.Label12 = New System.Windows.Forms.Label()
        Me.txtMchName5 = New System.Windows.Forms.TextBox()
        Me.Label9 = New System.Windows.Forms.Label()
        Me.Label5 = New System.Windows.Forms.Label()
        Me.Label4 = New System.Windows.Forms.Label()
        Me.Label3 = New System.Windows.Forms.Label()
        Me.Label2 = New System.Windows.Forms.Label()
        Me.txtMchName3 = New System.Windows.Forms.TextBox()
        Me.txtMchName2 = New System.Windows.Forms.TextBox()
        Me.txtMchName1 = New System.Windows.Forms.TextBox()
        Me.txtMchName4 = New System.Windows.Forms.TextBox()
        Me.txtMemo2 = New System.Windows.Forms.TextBox()
        Me.Label56 = New System.Windows.Forms.Label()
        Me.lblPosition = New System.Windows.Forms.Label()
        Me.Label6 = New System.Windows.Forms.Label()
        Me.txtItmNo = New System.Windows.Forms.Label()
        Me.GroupBox4 = New System.Windows.Forms.GroupBox()
        Me.FlowLayoutPanel2 = New System.Windows.Forms.FlowLayoutPanel()
        Me.rbGen = New System.Windows.Forms.RadioButton()
        Me.btnSearch = New System.Windows.Forms.Button()
        Me.ChkBarcode = New System.Windows.Forms.CheckBox()
        Me.rbDateRng = New System.Windows.Forms.RadioButton()
        Me.txtStartDate = New C1.Win.C1Input.C1DateEdit()
        Me.txtEndDate = New C1.Win.C1Input.C1DateEdit()
        Me.ErrProvider = New System.Windows.Forms.ErrorProvider(Me.components)
        Me.txtDatePhyCnt = New System.Windows.Forms.Label()
        Me.Label7 = New System.Windows.Forms.Label()
        Me.Label8 = New System.Windows.Forms.Label()
        Me.GroupBox2 = New System.Windows.Forms.GroupBox()
        Me.rb1 = New System.Windows.Forms.RadioButton()
        Me.rb2 = New System.Windows.Forms.RadioButton()
        Me.GroupBox3 = New System.Windows.Forms.GroupBox()
        Me.ChkNoBooks = New System.Windows.Forms.CheckBox()
        Me.btnBookCat = New System.Windows.Forms.Button()
        Me.txtBookCat = New System.Windows.Forms.TextBox()
        Me.chkBook = New System.Windows.Forms.CheckBox()
        Me.Label11 = New System.Windows.Forms.Label()
        Me.tdbBookCat = New C1.Win.C1TrueDBGrid.C1TrueDBGrid()
        Me.chekall = New System.Windows.Forms.Button()
        Me.pnlclacc = New System.Windows.Forms.Panel()
        Me.txtGeneralSearch = New DevComponents.DotNetBar.Controls.TextBoxX()
        Me.Label1 = New System.Windows.Forms.Label()
        Me.Panel1 = New System.Windows.Forms.Panel()
        Me.btnImportTransfer = New System.Windows.Forms.Button()
        Me.ImgLst = New System.Windows.Forms.ImageList(Me.components)
        Me.Label10 = New System.Windows.Forms.Label()
        Me.stbTime = New System.Windows.Forms.StatusBarPanel()
        Me.stbLocation = New System.Windows.Forms.StatusBarPanel()
        Me.stbRemark = New System.Windows.Forms.StatusBarPanel()
        Me.stbDate = New System.Windows.Forms.StatusBarPanel()
        Me.stBar = New System.Windows.Forms.StatusBar()
        Me.TxtMemo1 = New System.Windows.Forms.TextBox()
        Me.FlowLayoutPanel1 = New System.Windows.Forms.FlowLayoutPanel()
        Me.Panel2 = New System.Windows.Forms.Panel()
        Me.Panel3 = New System.Windows.Forms.Panel()
        Me.pnlHead = New DevComponents.DotNetBar.PanelEx()
        Me.lblTitle = New DevComponents.DotNetBar.Controls.ReflectionLabel()
        CType(Me.tdbDetails, System.ComponentModel.ISupportInitialize).BeginInit()
        Me.GroupBox1.SuspendLayout()
        Me.GroupBox4.SuspendLayout()
        Me.FlowLayoutPanel2.SuspendLayout()
        CType(Me.txtStartDate, System.ComponentModel.ISupportInitialize).BeginInit()
        CType(Me.txtEndDate, System.ComponentModel.ISupportInitialize).BeginInit()
        CType(Me.ErrProvider, System.ComponentModel.ISupportInitialize).BeginInit()
        Me.GroupBox2.SuspendLayout()
        Me.GroupBox3.SuspendLayout()
        CType(Me.tdbBookCat, System.ComponentModel.ISupportInitialize).BeginInit()
        Me.pnlclacc.SuspendLayout()
        Me.Panel1.SuspendLayout()
        CType(Me.stbTime, System.ComponentModel.ISupportInitialize).BeginInit()
        CType(Me.stbLocation, System.ComponentModel.ISupportInitialize).BeginInit()
        CType(Me.stbRemark, System.ComponentModel.ISupportInitialize).BeginInit()
        CType(Me.stbDate, System.ComponentModel.ISupportInitialize).BeginInit()
        Me.FlowLayoutPanel1.SuspendLayout()
        Me.Panel2.SuspendLayout()
        Me.Panel3.SuspendLayout()
        Me.pnlHead.SuspendLayout()
        Me.SuspendLayout()
        '
        'ConMenu
        '
        Me.ConMenu.MenuItems.AddRange(New System.Windows.Forms.MenuItem() {Me.ctmCopy, Me.ctmCut, Me.ctmPast})
        '
        'ctmCopy
        '
        Me.ctmCopy.Index = 0
        Me.ctmCopy.Text = "Cop&y"
        '
        'ctmCut
        '
        Me.ctmCut.Index = 1
        Me.ctmCut.Text = "Cu&t"
        '
        'ctmPast
        '
        Me.ctmPast.Index = 2
        Me.ctmPast.Text = "&Paste"
        '
        'Clock
        '
        '
        'tdbDetails
        '
        Me.tdbDetails.AllowColMove = False
        Me.tdbDetails.AllowColSelect = False
        Me.tdbDetails.AllowFilter = False
        Me.tdbDetails.AllowRowSelect = False
        Me.tdbDetails.AllowRowSizing = C1.Win.C1TrueDBGrid.RowSizingEnum.None
        Me.tdbDetails.AllowSort = False
        Me.tdbDetails.CaptionHeight = 17
        Me.tdbDetails.CaptionStyle = Style1
        Me.tdbDetails.CollapseColor = System.Drawing.Color.Black
        Me.tdbDetails.Dock = System.Windows.Forms.DockStyle.Fill
        Me.tdbDetails.EditorStyle = Style2
        Me.tdbDetails.EvenRowStyle = Style3
        Me.tdbDetails.ExpandColor = System.Drawing.Color.Black
        Me.tdbDetails.ExtendRightColumn = True
        Me.tdbDetails.FetchRowStyles = True
        Me.tdbDetails.FilterBar = True
        Me.tdbDetails.FilterBarStyle = Style4
        Me.tdbDetails.FooterStyle = Style5
        Me.tdbDetails.GroupByCaption = "Drag a column header here to group by that column"
        Me.tdbDetails.GroupStyle = Style6
        Me.tdbDetails.HeadingStyle = Style7
        Me.tdbDetails.HighLightRowStyle = Style8
        Me.tdbDetails.Images.Add(CType(resources.GetObject("tdbDetails.Images"), System.Drawing.Image))
        Me.tdbDetails.InactiveStyle = Style9
        Me.tdbDetails.Location = New System.Drawing.Point(0, 343)
        Me.tdbDetails.MarqueeStyle = C1.Win.C1TrueDBGrid.MarqueeEnum.DottedCellBorder
        Me.tdbDetails.Name = "tdbDetails"
        Me.tdbDetails.OddRowStyle = Style10
        Me.tdbDetails.PreviewInfo.Location = New System.Drawing.Point(0, 0)
        Me.tdbDetails.PreviewInfo.Size = New System.Drawing.Size(0, 0)
        Me.tdbDetails.PreviewInfo.ZoomFactor = 75.0R
        Me.tdbDetails.PrintInfo.PageSettings = CType(resources.GetObject("tdbDetails.PrintInfo.PageSettings"), System.Drawing.Printing.PageSettings)
        Me.tdbDetails.PrintInfo.ShowOptionsDialog = False
        Me.tdbDetails.RecordSelectorStyle = Style11
        Me.tdbDetails.RecordSelectorWidth = 17
        Me.tdbDetails.RowDivider.Color = System.Drawing.Color.DarkGray
        Me.tdbDetails.RowDivider.Style = C1.Win.C1TrueDBGrid.LineStyleEnum.[Single]
        Me.tdbDetails.RowHeight = 15
        Me.tdbDetails.RowSubDividerColor = System.Drawing.Color.DarkGray
        Me.tdbDetails.SelectedStyle = Style12
        Me.tdbDetails.Size = New System.Drawing.Size(983, 253)
        Me.tdbDetails.Style = Style13
        Me.tdbDetails.TabIndex = 4
        Me.tdbDetails.PropBag = resources.GetString("tdbDetails.PropBag")
        '
        'btnUnCheck
        '
        Me.btnUnCheck.BackColor = System.Drawing.SystemColors.Control
        Me.btnUnCheck.Cursor = System.Windows.Forms.Cursors.Hand
        Me.btnUnCheck.Enabled = False
        Me.btnUnCheck.FlatAppearance.BorderColor = System.Drawing.Color.Navy
        Me.btnUnCheck.FlatAppearance.MouseDownBackColor = System.Drawing.Color.RoyalBlue
        Me.btnUnCheck.FlatAppearance.MouseOverBackColor = System.Drawing.Color.White
        Me.btnUnCheck.FlatStyle = System.Windows.Forms.FlatStyle.Flat
        Me.btnUnCheck.Font = New System.Drawing.Font("Microsoft Sans Serif", 8.25!, System.Drawing.FontStyle.Bold)
        Me.btnUnCheck.ForeColor = System.Drawing.SystemColors.ControlText
        Me.btnUnCheck.Image = CType(resources.GetObject("btnUnCheck.Image"), System.Drawing.Image)
        Me.btnUnCheck.ImageAlign = System.Drawing.ContentAlignment.TopLeft
        Me.btnUnCheck.ImeMode = System.Windows.Forms.ImeMode.NoControl
        Me.btnUnCheck.Location = New System.Drawing.Point(247, 27)
        Me.btnUnCheck.Name = "btnUnCheck"
        Me.btnUnCheck.Size = New System.Drawing.Size(111, 38)
        Me.btnUnCheck.TabIndex = 6
        Me.btnUnCheck.Text = "    UnCheck All F6       "
        Me.btnUnCheck.TextAlign = System.Drawing.ContentAlignment.MiddleRight
        Me.btnUnCheck.UseVisualStyleBackColor = False
        '
        'btnSave
        '
        Me.btnSave.BackColor = System.Drawing.SystemColors.Control
        Me.btnSave.Cursor = System.Windows.Forms.Cursors.Hand
        Me.btnSave.Enabled = False
        Me.btnSave.FlatAppearance.BorderColor = System.Drawing.Color.Navy
        Me.btnSave.FlatAppearance.MouseDownBackColor = System.Drawing.Color.RoyalBlue
        Me.btnSave.FlatAppearance.MouseOverBackColor = System.Drawing.Color.White
        Me.btnSave.FlatStyle = System.Windows.Forms.FlatStyle.Flat
        Me.btnSave.Font = New System.Drawing.Font("Microsoft Sans Serif", 8.25!, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point, CType(0, Byte))
        Me.btnSave.ForeColor = System.Drawing.SystemColors.ControlText
        Me.btnSave.Image = CType(resources.GetObject("btnSave.Image"), System.Drawing.Image)
        Me.btnSave.ImageAlign = System.Drawing.ContentAlignment.TopLeft
        Me.btnSave.Location = New System.Drawing.Point(689, 27)
        Me.btnSave.Name = "btnSave"
        Me.btnSave.Size = New System.Drawing.Size(111, 38)
        Me.btnSave.TabIndex = 8
        Me.btnSave.Text = "    Save   " & Global.Microsoft.VisualBasic.ChrW(13) & Global.Microsoft.VisualBasic.ChrW(10) & " F9     "
        Me.btnSave.TextAlign = System.Drawing.ContentAlignment.MiddleRight
        Me.btnSave.UseVisualStyleBackColor = False
        '
        'btnCheck
        '
        Me.btnCheck.BackColor = System.Drawing.SystemColors.Control
        Me.btnCheck.Cursor = System.Windows.Forms.Cursors.Hand
        Me.btnCheck.Enabled = False
        Me.btnCheck.FlatAppearance.BorderColor = System.Drawing.Color.Navy
        Me.btnCheck.FlatAppearance.MouseDownBackColor = System.Drawing.Color.RoyalBlue
        Me.btnCheck.FlatAppearance.MouseOverBackColor = System.Drawing.Color.White
        Me.btnCheck.FlatStyle = System.Windows.Forms.FlatStyle.Flat
        Me.btnCheck.Font = New System.Drawing.Font("Microsoft Sans Serif", 8.25!, System.Drawing.FontStyle.Bold)
        Me.btnCheck.ForeColor = System.Drawing.SystemColors.ControlText
        Me.btnCheck.Image = CType(resources.GetObject("btnCheck.Image"), System.Drawing.Image)
        Me.btnCheck.ImageAlign = System.Drawing.ContentAlignment.TopLeft
        Me.btnCheck.ImeMode = System.Windows.Forms.ImeMode.NoControl
        Me.btnCheck.Location = New System.Drawing.Point(130, 27)
        Me.btnCheck.Name = "btnCheck"
        Me.btnCheck.Size = New System.Drawing.Size(111, 38)
        Me.btnCheck.TabIndex = 5
        Me.btnCheck.Tag = "true"
        Me.btnCheck.Text = "      Check All  F5    "
        Me.btnCheck.TextAlign = System.Drawing.ContentAlignment.MiddleRight
        Me.btnCheck.UseVisualStyleBackColor = False
        '
        'btnShow
        '
        Me.btnShow.BackColor = System.Drawing.SystemColors.Control
        Me.btnShow.Cursor = System.Windows.Forms.Cursors.Hand
        Me.btnShow.FlatAppearance.BorderColor = System.Drawing.Color.Navy
        Me.btnShow.FlatAppearance.MouseDownBackColor = System.Drawing.Color.RoyalBlue
        Me.btnShow.FlatAppearance.MouseOverBackColor = System.Drawing.Color.White
        Me.btnShow.FlatStyle = System.Windows.Forms.FlatStyle.Flat
        Me.btnShow.Font = New System.Drawing.Font("Microsoft Sans Serif", 8.25!, System.Drawing.FontStyle.Bold)
        Me.btnShow.ForeColor = System.Drawing.SystemColors.ControlText
        Me.btnShow.Image = CType(resources.GetObject("btnShow.Image"), System.Drawing.Image)
        Me.btnShow.ImageAlign = System.Drawing.ContentAlignment.MiddleLeft
        Me.btnShow.ImeMode = System.Windows.Forms.ImeMode.NoControl
        Me.btnShow.Location = New System.Drawing.Point(13, 27)
        Me.btnShow.Name = "btnShow"
        Me.btnShow.Size = New System.Drawing.Size(111, 38)
        Me.btnShow.TabIndex = 4
        Me.btnShow.Text = "Show " & Global.Microsoft.VisualBasic.ChrW(13) & Global.Microsoft.VisualBasic.ChrW(10) & "F4    "
        Me.btnShow.TextAlign = System.Drawing.ContentAlignment.MiddleRight
        Me.btnShow.UseVisualStyleBackColor = False
        '
        'btnClose
        '
        Me.btnClose.BackColor = System.Drawing.SystemColors.Control
        Me.btnClose.Cursor = System.Windows.Forms.Cursors.Hand
        Me.btnClose.FlatAppearance.BorderColor = System.Drawing.Color.Navy
        Me.btnClose.FlatAppearance.MouseDownBackColor = System.Drawing.Color.RoyalBlue
        Me.btnClose.FlatAppearance.MouseOverBackColor = System.Drawing.Color.White
        Me.btnClose.FlatStyle = System.Windows.Forms.FlatStyle.Flat
        Me.btnClose.Font = New System.Drawing.Font("Microsoft Sans Serif", 8.25!, System.Drawing.FontStyle.Bold)
        Me.btnClose.ForeColor = System.Drawing.SystemColors.ControlText
        Me.btnClose.Image = CType(resources.GetObject("btnClose.Image"), System.Drawing.Image)
        Me.btnClose.ImageAlign = System.Drawing.ContentAlignment.TopLeft
        Me.btnClose.ImeMode = System.Windows.Forms.ImeMode.NoControl
        Me.btnClose.Location = New System.Drawing.Point(858, 27)
        Me.btnClose.Name = "btnClose"
        Me.btnClose.Size = New System.Drawing.Size(111, 38)
        Me.btnClose.TabIndex = 5
        Me.btnClose.Text = " Close " & Global.Microsoft.VisualBasic.ChrW(13) & Global.Microsoft.VisualBasic.ChrW(10) & "F12   "
        Me.btnClose.TextAlign = System.Drawing.ContentAlignment.MiddleRight
        Me.btnClose.UseVisualStyleBackColor = False
        '
        'btnClear
        '
        Me.btnClear.BackColor = System.Drawing.SystemColors.Control
        Me.btnClear.Cursor = System.Windows.Forms.Cursors.Hand
        Me.btnClear.FlatAppearance.BorderColor = System.Drawing.Color.Navy
        Me.btnClear.FlatAppearance.MouseDownBackColor = System.Drawing.Color.RoyalBlue
        Me.btnClear.FlatAppearance.MouseOverBackColor = System.Drawing.Color.White
        Me.btnClear.FlatStyle = System.Windows.Forms.FlatStyle.Flat
        Me.btnClear.Font = New System.Drawing.Font("Microsoft Sans Serif", 8.25!, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point, CType(0, Byte))
        Me.btnClear.ForeColor = System.Drawing.SystemColors.ControlText
        Me.btnClear.Image = CType(resources.GetObject("btnClear.Image"), System.Drawing.Image)
        Me.btnClear.ImageAlign = System.Drawing.ContentAlignment.TopLeft
        Me.btnClear.Location = New System.Drawing.Point(562, 27)
        Me.btnClear.Name = "btnClear"
        Me.btnClear.Size = New System.Drawing.Size(111, 38)
        Me.btnClear.TabIndex = 7
        Me.btnClear.Text = "    Clear     " & Global.Microsoft.VisualBasic.ChrW(13) & Global.Microsoft.VisualBasic.ChrW(10) & "F7      "
        Me.btnClear.TextAlign = System.Drawing.ContentAlignment.MiddleRight
        Me.btnClear.UseVisualStyleBackColor = False
        '
        'Label58
        '
        Me.Label58.BackColor = System.Drawing.Color.Transparent
        Me.Label58.Font = New System.Drawing.Font("Microsoft Sans Serif", 9.0!, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point, CType(0, Byte))
        Me.Label58.ForeColor = System.Drawing.Color.Navy
        Me.Label58.Location = New System.Drawing.Point(3, 5)
        Me.Label58.Name = "Label58"
        Me.Label58.Size = New System.Drawing.Size(83, 18)
        Me.Label58.TabIndex = 1
        Me.Label58.Text = "Count Date"
        Me.Label58.TextAlign = System.Drawing.ContentAlignment.MiddleLeft
        '
        'GroupBox1
        '
        Me.GroupBox1.Anchor = CType(((System.Windows.Forms.AnchorStyles.Top Or System.Windows.Forms.AnchorStyles.Left) _
            Or System.Windows.Forms.AnchorStyles.Right), System.Windows.Forms.AnchorStyles)
        Me.GroupBox1.BackColor = System.Drawing.Color.FromArgb(CType(CType(227, Byte), Integer), CType(CType(239, Byte), Integer), CType(CType(255, Byte), Integer))
        Me.GroupBox1.Controls.Add(Me.Label13)
        Me.GroupBox1.Controls.Add(Me.txtMchName6)
        Me.GroupBox1.Controls.Add(Me.Label12)
        Me.GroupBox1.Controls.Add(Me.txtMchName5)
        Me.GroupBox1.Controls.Add(Me.Label9)
        Me.GroupBox1.Controls.Add(Me.Label5)
        Me.GroupBox1.Controls.Add(Me.Label4)
        Me.GroupBox1.Controls.Add(Me.Label3)
        Me.GroupBox1.Controls.Add(Me.Label2)
        Me.GroupBox1.Controls.Add(Me.txtMchName3)
        Me.GroupBox1.Controls.Add(Me.txtMchName2)
        Me.GroupBox1.Controls.Add(Me.txtMchName1)
        Me.GroupBox1.Controls.Add(Me.txtMchName4)
        Me.GroupBox1.Font = New System.Drawing.Font("Microsoft Sans Serif", 9.0!, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point, CType(0, Byte))
        Me.GroupBox1.ForeColor = System.Drawing.Color.Maroon
        Me.GroupBox1.Location = New System.Drawing.Point(3, 3)
        Me.GroupBox1.Name = "GroupBox1"
        Me.GroupBox1.Size = New System.Drawing.Size(973, 45)
        Me.GroupBox1.TabIndex = 0
        Me.GroupBox1.TabStop = False
        Me.GroupBox1.Text = "Count Machine or User Name"
        '
        'Label13
        '
        Me.Label13.Anchor = System.Windows.Forms.AnchorStyles.None
        Me.Label13.BackColor = System.Drawing.Color.Transparent
        Me.Label13.Font = New System.Drawing.Font("Microsoft Sans Serif", 9.0!, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point, CType(0, Byte))
        Me.Label13.ForeColor = System.Drawing.Color.Navy
        Me.Label13.Location = New System.Drawing.Point(856, 16)
        Me.Label13.Name = "Label13"
        Me.Label13.Size = New System.Drawing.Size(16, 18)
        Me.Label13.TabIndex = 173
        Me.Label13.Text = "6"
        Me.Label13.TextAlign = System.Drawing.ContentAlignment.MiddleCenter
        '
        'txtMchName6
        '
        Me.txtMchName6.Anchor = System.Windows.Forms.AnchorStyles.None
        Me.txtMchName6.CharacterCasing = System.Windows.Forms.CharacterCasing.Upper
        Me.txtMchName6.Font = New System.Drawing.Font("Microsoft Sans Serif", 8.25!, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point, CType(0, Byte))
        Me.txtMchName6.Location = New System.Drawing.Point(872, 16)
        Me.txtMchName6.MaxLength = 5
        Me.txtMchName6.Name = "txtMchName6"
        Me.txtMchName6.ReadOnly = True
        Me.txtMchName6.Size = New System.Drawing.Size(96, 20)
        Me.txtMchName6.TabIndex = 174
        Me.txtMchName6.TextAlign = System.Windows.Forms.HorizontalAlignment.Center
        '
        'Label12
        '
        Me.Label12.Anchor = System.Windows.Forms.AnchorStyles.None
        Me.Label12.BackColor = System.Drawing.Color.Transparent
        Me.Label12.Font = New System.Drawing.Font("Microsoft Sans Serif", 9.0!, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point, CType(0, Byte))
        Me.Label12.ForeColor = System.Drawing.Color.Navy
        Me.Label12.Location = New System.Drawing.Point(730, 16)
        Me.Label12.Name = "Label12"
        Me.Label12.Size = New System.Drawing.Size(16, 18)
        Me.Label12.TabIndex = 171
        Me.Label12.Text = "5"
        Me.Label12.TextAlign = System.Drawing.ContentAlignment.MiddleCenter
        '
        'txtMchName5
        '
        Me.txtMchName5.Anchor = System.Windows.Forms.AnchorStyles.None
        Me.txtMchName5.CharacterCasing = System.Windows.Forms.CharacterCasing.Upper
        Me.txtMchName5.Font = New System.Drawing.Font("Microsoft Sans Serif", 8.25!, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point, CType(0, Byte))
        Me.txtMchName5.Location = New System.Drawing.Point(746, 16)
        Me.txtMchName5.MaxLength = 5
        Me.txtMchName5.Name = "txtMchName5"
        Me.txtMchName5.ReadOnly = True
        Me.txtMchName5.Size = New System.Drawing.Size(96, 20)
        Me.txtMchName5.TabIndex = 172
        Me.txtMchName5.TextAlign = System.Windows.Forms.HorizontalAlignment.Center
        '
        'Label9
        '
        Me.Label9.BackColor = System.Drawing.Color.Transparent
        Me.Label9.Font = New System.Drawing.Font("Microsoft Sans Serif", 9.0!, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point, CType(0, Byte))
        Me.Label9.ForeColor = System.Drawing.Color.Navy
        Me.Label9.Location = New System.Drawing.Point(3, 16)
        Me.Label9.Name = "Label9"
        Me.Label9.Size = New System.Drawing.Size(185, 17)
        Me.Label9.TabIndex = 0
        Me.Label9.Text = "Input Format : ITxxx , Uxxxx"
        Me.Label9.TextAlign = System.Drawing.ContentAlignment.MiddleLeft
        '
        'Label5
        '
        Me.Label5.Anchor = System.Windows.Forms.AnchorStyles.None
        Me.Label5.BackColor = System.Drawing.Color.Transparent
        Me.Label5.Font = New System.Drawing.Font("Microsoft Sans Serif", 9.0!, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point, CType(0, Byte))
        Me.Label5.ForeColor = System.Drawing.Color.Navy
        Me.Label5.Location = New System.Drawing.Point(600, 16)
        Me.Label5.Name = "Label5"
        Me.Label5.Size = New System.Drawing.Size(16, 18)
        Me.Label5.TabIndex = 4
        Me.Label5.Text = "4"
        Me.Label5.TextAlign = System.Drawing.ContentAlignment.MiddleCenter
        '
        'Label4
        '
        Me.Label4.Anchor = System.Windows.Forms.AnchorStyles.None
        Me.Label4.BackColor = System.Drawing.Color.Transparent
        Me.Label4.Font = New System.Drawing.Font("Microsoft Sans Serif", 9.0!, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point, CType(0, Byte))
        Me.Label4.ForeColor = System.Drawing.Color.Navy
        Me.Label4.Location = New System.Drawing.Point(468, 16)
        Me.Label4.Name = "Label4"
        Me.Label4.Size = New System.Drawing.Size(16, 18)
        Me.Label4.TabIndex = 3
        Me.Label4.Text = "3"
        Me.Label4.TextAlign = System.Drawing.ContentAlignment.MiddleCenter
        '
        'Label3
        '
        Me.Label3.Anchor = System.Windows.Forms.AnchorStyles.None
        Me.Label3.BackColor = System.Drawing.Color.Transparent
        Me.Label3.Font = New System.Drawing.Font("Microsoft Sans Serif", 9.0!, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point, CType(0, Byte))
        Me.Label3.ForeColor = System.Drawing.Color.Navy
        Me.Label3.Location = New System.Drawing.Point(337, 16)
        Me.Label3.Name = "Label3"
        Me.Label3.Size = New System.Drawing.Size(16, 18)
        Me.Label3.TabIndex = 2
        Me.Label3.Text = "2"
        Me.Label3.TextAlign = System.Drawing.ContentAlignment.MiddleCenter
        '
        'Label2
        '
        Me.Label2.Anchor = System.Windows.Forms.AnchorStyles.None
        Me.Label2.BackColor = System.Drawing.Color.Transparent
        Me.Label2.Font = New System.Drawing.Font("Microsoft Sans Serif", 9.0!, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point, CType(0, Byte))
        Me.Label2.ForeColor = System.Drawing.Color.Navy
        Me.Label2.Location = New System.Drawing.Point(204, 16)
        Me.Label2.Name = "Label2"
        Me.Label2.Size = New System.Drawing.Size(16, 18)
        Me.Label2.TabIndex = 0
        Me.Label2.Text = "1"
        Me.Label2.TextAlign = System.Drawing.ContentAlignment.MiddleCenter
        '
        'txtMchName3
        '
        Me.txtMchName3.Anchor = System.Windows.Forms.AnchorStyles.None
        Me.txtMchName3.CharacterCasing = System.Windows.Forms.CharacterCasing.Upper
        Me.txtMchName3.Font = New System.Drawing.Font("Microsoft Sans Serif", 8.25!, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point, CType(0, Byte))
        Me.txtMchName3.Location = New System.Drawing.Point(484, 16)
        Me.txtMchName3.MaxLength = 5
        Me.txtMchName3.Name = "txtMchName3"
        Me.txtMchName3.ReadOnly = True
        Me.txtMchName3.Size = New System.Drawing.Size(96, 20)
        Me.txtMchName3.TabIndex = 3
        Me.txtMchName3.TextAlign = System.Windows.Forms.HorizontalAlignment.Center
        '
        'txtMchName2
        '
        Me.txtMchName2.Anchor = System.Windows.Forms.AnchorStyles.None
        Me.txtMchName2.CharacterCasing = System.Windows.Forms.CharacterCasing.Upper
        Me.txtMchName2.Font = New System.Drawing.Font("Microsoft Sans Serif", 8.25!, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point, CType(0, Byte))
        Me.txtMchName2.Location = New System.Drawing.Point(353, 16)
        Me.txtMchName2.MaxLength = 5
        Me.txtMchName2.Name = "txtMchName2"
        Me.txtMchName2.ReadOnly = True
        Me.txtMchName2.Size = New System.Drawing.Size(96, 20)
        Me.txtMchName2.TabIndex = 2
        Me.txtMchName2.TextAlign = System.Windows.Forms.HorizontalAlignment.Center
        '
        'txtMchName1
        '
        Me.txtMchName1.Anchor = System.Windows.Forms.AnchorStyles.None
        Me.txtMchName1.CharacterCasing = System.Windows.Forms.CharacterCasing.Upper
        Me.txtMchName1.Font = New System.Drawing.Font("Microsoft Sans Serif", 8.25!, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point, CType(0, Byte))
        Me.txtMchName1.Location = New System.Drawing.Point(220, 16)
        Me.txtMchName1.MaxLength = 5
        Me.txtMchName1.Name = "txtMchName1"
        Me.txtMchName1.Size = New System.Drawing.Size(96, 20)
        Me.txtMchName1.TabIndex = 1
        Me.txtMchName1.TextAlign = System.Windows.Forms.HorizontalAlignment.Center
        '
        'txtMchName4
        '
        Me.txtMchName4.Anchor = System.Windows.Forms.AnchorStyles.None
        Me.txtMchName4.CharacterCasing = System.Windows.Forms.CharacterCasing.Upper
        Me.txtMchName4.Font = New System.Drawing.Font("Microsoft Sans Serif", 8.25!, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point, CType(0, Byte))
        Me.txtMchName4.Location = New System.Drawing.Point(616, 16)
        Me.txtMchName4.MaxLength = 5
        Me.txtMchName4.Name = "txtMchName4"
        Me.txtMchName4.ReadOnly = True
        Me.txtMchName4.Size = New System.Drawing.Size(96, 20)
        Me.txtMchName4.TabIndex = 4
        Me.txtMchName4.TextAlign = System.Windows.Forms.HorizontalAlignment.Center
        '
        'txtMemo2
        '
        Me.txtMemo2.Anchor = CType(((System.Windows.Forms.AnchorStyles.Top Or System.Windows.Forms.AnchorStyles.Left) _
            Or System.Windows.Forms.AnchorStyles.Right), System.Windows.Forms.AnchorStyles)
        Me.txtMemo2.Location = New System.Drawing.Point(90, 57)
        Me.txtMemo2.MaxLength = 3000
        Me.txtMemo2.Multiline = True
        Me.txtMemo2.Name = "txtMemo2"
        Me.txtMemo2.ScrollBars = System.Windows.Forms.ScrollBars.Vertical
        Me.txtMemo2.Size = New System.Drawing.Size(889, 52)
        Me.txtMemo2.TabIndex = 5
        '
        'Label56
        '
        Me.Label56.BackColor = System.Drawing.Color.Transparent
        Me.Label56.Font = New System.Drawing.Font("Microsoft Sans Serif", 9.0!, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point, CType(0, Byte))
        Me.Label56.ForeColor = System.Drawing.Color.Navy
        Me.Label56.Location = New System.Drawing.Point(7, 4)
        Me.Label56.Name = "Label56"
        Me.Label56.Size = New System.Drawing.Size(56, 19)
        Me.Label56.TabIndex = 0
        Me.Label56.Text = "Record"
        Me.Label56.TextAlign = System.Drawing.ContentAlignment.MiddleCenter
        '
        'lblPosition
        '
        Me.lblPosition.BackColor = System.Drawing.Color.SkyBlue
        Me.lblPosition.BorderStyle = System.Windows.Forms.BorderStyle.FixedSingle
        Me.lblPosition.Font = New System.Drawing.Font("Microsoft Sans Serif", 8.25!, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point, CType(0, Byte))
        Me.lblPosition.ForeColor = System.Drawing.Color.Red
        Me.lblPosition.Location = New System.Drawing.Point(71, 4)
        Me.lblPosition.Name = "lblPosition"
        Me.lblPosition.Size = New System.Drawing.Size(152, 19)
        Me.lblPosition.TabIndex = 1
        Me.lblPosition.Tag = "0/0"
        Me.lblPosition.Text = " 0/0"
        Me.lblPosition.TextAlign = System.Drawing.ContentAlignment.MiddleCenter
        '
        'Label6
        '
        Me.Label6.Anchor = CType((System.Windows.Forms.AnchorStyles.Top Or System.Windows.Forms.AnchorStyles.Right), System.Windows.Forms.AnchorStyles)
        Me.Label6.BackColor = System.Drawing.Color.Transparent
        Me.Label6.Font = New System.Drawing.Font("Microsoft Sans Serif", 9.0!, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point, CType(0, Byte))
        Me.Label6.ForeColor = System.Drawing.Color.Navy
        Me.Label6.Location = New System.Drawing.Point(625, 4)
        Me.Label6.Name = "Label6"
        Me.Label6.Size = New System.Drawing.Size(174, 19)
        Me.Label6.TabIndex = 2
        Me.Label6.Text = "# of References Selected"
        Me.Label6.TextAlign = System.Drawing.ContentAlignment.MiddleCenter
        '
        'txtItmNo
        '
        Me.txtItmNo.Anchor = CType((System.Windows.Forms.AnchorStyles.Top Or System.Windows.Forms.AnchorStyles.Right), System.Windows.Forms.AnchorStyles)
        Me.txtItmNo.BackColor = System.Drawing.Color.AliceBlue
        Me.txtItmNo.BorderStyle = System.Windows.Forms.BorderStyle.FixedSingle
        Me.txtItmNo.Font = New System.Drawing.Font("Microsoft Sans Serif", 9.0!, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point, CType(0, Byte))
        Me.txtItmNo.ForeColor = System.Drawing.Color.Navy
        Me.txtItmNo.Location = New System.Drawing.Point(802, 4)
        Me.txtItmNo.Name = "txtItmNo"
        Me.txtItmNo.Size = New System.Drawing.Size(174, 19)
        Me.txtItmNo.TabIndex = 3
        Me.txtItmNo.Tag = "Text"
        Me.txtItmNo.Text = "0"
        Me.txtItmNo.TextAlign = System.Drawing.ContentAlignment.MiddleCenter
        '
        'GroupBox4
        '
        Me.GroupBox4.Anchor = CType(((System.Windows.Forms.AnchorStyles.Top Or System.Windows.Forms.AnchorStyles.Left) _
            Or System.Windows.Forms.AnchorStyles.Right), System.Windows.Forms.AnchorStyles)
        Me.GroupBox4.BackColor = System.Drawing.Color.FromArgb(CType(CType(227, Byte), Integer), CType(CType(239, Byte), Integer), CType(CType(255, Byte), Integer))
        Me.GroupBox4.Controls.Add(Me.FlowLayoutPanel2)
        Me.GroupBox4.Font = New System.Drawing.Font("Microsoft Sans Serif", 9.0!, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point, CType(0, Byte))
        Me.GroupBox4.ForeColor = System.Drawing.Color.Maroon
        Me.GroupBox4.Location = New System.Drawing.Point(3, 96)
        Me.GroupBox4.Name = "GroupBox4"
        Me.GroupBox4.Size = New System.Drawing.Size(969, 38)
        Me.GroupBox4.TabIndex = 2
        Me.GroupBox4.TabStop = False
        Me.GroupBox4.Text = "Selection Option"
        '
        'FlowLayoutPanel2
        '
        Me.FlowLayoutPanel2.Anchor = CType(((System.Windows.Forms.AnchorStyles.Top Or System.Windows.Forms.AnchorStyles.Left) _
            Or System.Windows.Forms.AnchorStyles.Right), System.Windows.Forms.AnchorStyles)
        Me.FlowLayoutPanel2.Controls.Add(Me.rbGen)
        Me.FlowLayoutPanel2.Controls.Add(Me.btnSearch)
        Me.FlowLayoutPanel2.Controls.Add(Me.ChkBarcode)
        Me.FlowLayoutPanel2.Controls.Add(Me.rbDateRng)
        Me.FlowLayoutPanel2.Controls.Add(Me.txtStartDate)
        Me.FlowLayoutPanel2.Controls.Add(Me.txtEndDate)
        Me.FlowLayoutPanel2.Location = New System.Drawing.Point(5, 14)
        Me.FlowLayoutPanel2.Name = "FlowLayoutPanel2"
        Me.FlowLayoutPanel2.Size = New System.Drawing.Size(969, 26)
        Me.FlowLayoutPanel2.TabIndex = 2
        '
        'rbGen
        '
        Me.rbGen.Checked = True
        Me.rbGen.ForeColor = System.Drawing.Color.Navy
        Me.rbGen.Location = New System.Drawing.Point(3, 3)
        Me.rbGen.Name = "rbGen"
        Me.rbGen.Size = New System.Drawing.Size(121, 19)
        Me.rbGen.TabIndex = 0
        Me.rbGen.TabStop = True
        Me.rbGen.Text = "General Scope"
        '
        'btnSearch
        '
        Me.btnSearch.BackColor = System.Drawing.SystemColors.Control
        Me.btnSearch.Cursor = System.Windows.Forms.Cursors.Hand
        Me.btnSearch.FlatAppearance.BorderColor = System.Drawing.Color.Navy
        Me.btnSearch.FlatAppearance.MouseDownBackColor = System.Drawing.Color.RoyalBlue
        Me.btnSearch.FlatAppearance.MouseOverBackColor = System.Drawing.Color.White
        Me.btnSearch.FlatStyle = System.Windows.Forms.FlatStyle.Flat
        Me.btnSearch.Font = New System.Drawing.Font("Microsoft Sans Serif", 8.25!, System.Drawing.FontStyle.Bold)
        Me.btnSearch.ForeColor = System.Drawing.SystemColors.ControlText
        Me.btnSearch.Image = CType(resources.GetObject("btnSearch.Image"), System.Drawing.Image)
        Me.btnSearch.ImageAlign = System.Drawing.ContentAlignment.MiddleLeft
        Me.btnSearch.ImeMode = System.Windows.Forms.ImeMode.NoControl
        Me.btnSearch.Location = New System.Drawing.Point(130, 3)
        Me.btnSearch.Name = "btnSearch"
        Me.btnSearch.Size = New System.Drawing.Size(170, 23)
        Me.btnSearch.TabIndex = 1
        Me.btnSearch.Text = "Dates of Phys. Count F8"
        Me.btnSearch.TextAlign = System.Drawing.ContentAlignment.MiddleRight
        Me.btnSearch.UseVisualStyleBackColor = False
        Me.btnSearch.Visible = False
        '
        'ChkBarcode
        '
        Me.ChkBarcode.ForeColor = System.Drawing.Color.Navy
        Me.ChkBarcode.Location = New System.Drawing.Point(306, 3)
        Me.ChkBarcode.Name = "ChkBarcode"
        Me.ChkBarcode.Size = New System.Drawing.Size(242, 19)
        Me.ChkBarcode.TabIndex = 2
        Me.ChkBarcode.Text = "Show Barcode For Trading Items"
        '
        'rbDateRng
        '
        Me.rbDateRng.Anchor = CType((System.Windows.Forms.AnchorStyles.Top Or System.Windows.Forms.AnchorStyles.Right), System.Windows.Forms.AnchorStyles)
        Me.rbDateRng.ForeColor = System.Drawing.Color.Navy
        Me.rbDateRng.Location = New System.Drawing.Point(554, 3)
        Me.rbDateRng.Name = "rbDateRng"
        Me.rbDateRng.Size = New System.Drawing.Size(241, 19)
        Me.rbDateRng.TabIndex = 3
        Me.rbDateRng.Text = "Select Not Selected (Date Range)"
        '
        'txtStartDate
        '
        Me.txtStartDate.Anchor = CType((System.Windows.Forms.AnchorStyles.Top Or System.Windows.Forms.AnchorStyles.Right), System.Windows.Forms.AnchorStyles)
        '
        '
        '
        Me.txtStartDate.Calendar.Name = ""
        Me.txtStartDate.EditMask = "99/99/9999"
        Me.txtStartDate.Font = New System.Drawing.Font("Microsoft Sans Serif", 9.0!, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point, CType(0, Byte))
        Me.txtStartDate.FormatType = C1.Win.C1Input.FormatTypeEnum.ShortDate
        Me.txtStartDate.Location = New System.Drawing.Point(801, 3)
        Me.txtStartDate.Name = "txtStartDate"
        Me.txtStartDate.Size = New System.Drawing.Size(128, 21)
        Me.txtStartDate.TabIndex = 4
        Me.txtStartDate.Tag = Nothing
        Me.txtStartDate.TextAlign = System.Windows.Forms.HorizontalAlignment.Center
        Me.txtStartDate.Visible = False
        '
        'txtEndDate
        '
        Me.txtEndDate.Anchor = CType((System.Windows.Forms.AnchorStyles.Top Or System.Windows.Forms.AnchorStyles.Right), System.Windows.Forms.AnchorStyles)
        '
        '
        '
        Me.txtEndDate.Calendar.Name = ""
        Me.txtEndDate.EditMask = "99/99/9999"
        Me.txtEndDate.Font = New System.Drawing.Font("Microsoft Sans Serif", 9.0!, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point, CType(0, Byte))
        Me.txtEndDate.FormatType = C1.Win.C1Input.FormatTypeEnum.ShortDate
        Me.txtEndDate.Location = New System.Drawing.Point(3, 32)
        Me.txtEndDate.Name = "txtEndDate"
        Me.txtEndDate.Size = New System.Drawing.Size(128, 21)
        Me.txtEndDate.TabIndex = 5
        Me.txtEndDate.Tag = Nothing
        Me.txtEndDate.TextAlign = System.Windows.Forms.HorizontalAlignment.Center
        Me.txtEndDate.Visible = False
        '
        'ErrProvider
        '
        Me.ErrProvider.ContainerControl = Me
        '
        'txtDatePhyCnt
        '
        Me.txtDatePhyCnt.BackColor = System.Drawing.Color.AliceBlue
        Me.txtDatePhyCnt.BorderStyle = System.Windows.Forms.BorderStyle.FixedSingle
        Me.txtDatePhyCnt.Font = New System.Drawing.Font("Microsoft Sans Serif", 9.0!, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point, CType(0, Byte))
        Me.txtDatePhyCnt.ForeColor = System.Drawing.Color.Navy
        Me.txtDatePhyCnt.Location = New System.Drawing.Point(92, 5)
        Me.txtDatePhyCnt.Name = "txtDatePhyCnt"
        Me.txtDatePhyCnt.Size = New System.Drawing.Size(123, 18)
        Me.txtDatePhyCnt.TabIndex = 2
        Me.txtDatePhyCnt.TextAlign = System.Drawing.ContentAlignment.MiddleCenter
        '
        'Label7
        '
        Me.Label7.BackColor = System.Drawing.Color.Transparent
        Me.Label7.Font = New System.Drawing.Font("Microsoft Sans Serif", 9.0!, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point, CType(0, Byte))
        Me.Label7.ForeColor = System.Drawing.Color.Navy
        Me.Label7.Location = New System.Drawing.Point(3, 0)
        Me.Label7.Name = "Label7"
        Me.Label7.Size = New System.Drawing.Size(82, 18)
        Me.Label7.TabIndex = 2
        Me.Label7.Text = "Memo1"
        Me.Label7.TextAlign = System.Drawing.ContentAlignment.MiddleLeft
        '
        'Label8
        '
        Me.Label8.BackColor = System.Drawing.Color.Transparent
        Me.Label8.Font = New System.Drawing.Font("Microsoft Sans Serif", 9.0!, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point, CType(0, Byte))
        Me.Label8.ForeColor = System.Drawing.Color.Navy
        Me.Label8.Location = New System.Drawing.Point(3, 57)
        Me.Label8.Name = "Label8"
        Me.Label8.Size = New System.Drawing.Size(82, 18)
        Me.Label8.TabIndex = 172
        Me.Label8.Text = "Memo2"
        Me.Label8.TextAlign = System.Drawing.ContentAlignment.MiddleLeft
        '
        'GroupBox2
        '
        Me.GroupBox2.Controls.Add(Me.rb1)
        Me.GroupBox2.Controls.Add(Me.rb2)
        Me.GroupBox2.Font = New System.Drawing.Font("Microsoft Sans Serif", 5.0!, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point, CType(0, Byte))
        Me.GroupBox2.ForeColor = System.Drawing.Color.Maroon
        Me.GroupBox2.Location = New System.Drawing.Point(3, 23)
        Me.GroupBox2.Name = "GroupBox2"
        Me.GroupBox2.Size = New System.Drawing.Size(81, 28)
        Me.GroupBox2.TabIndex = 4
        Me.GroupBox2.TabStop = False
        Me.GroupBox2.Text = "Warehouse"
        Me.GroupBox2.Visible = False
        '
        'rb1
        '
        Me.rb1.Checked = True
        Me.rb1.Font = New System.Drawing.Font("Microsoft Sans Serif", 5.0!, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point, CType(0, Byte))
        Me.rb1.ForeColor = System.Drawing.SystemColors.ControlText
        Me.rb1.Location = New System.Drawing.Point(2, 6)
        Me.rb1.Name = "rb1"
        Me.rb1.Size = New System.Drawing.Size(79, 24)
        Me.rb1.TabIndex = 1
        Me.rb1.TabStop = True
        Me.rb1.Text = "WareHouse12"
        Me.rb1.TextAlign = System.Drawing.ContentAlignment.MiddleCenter
        '
        'rb2
        '
        Me.rb2.Font = New System.Drawing.Font("Microsoft Sans Serif", 5.0!, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point, CType(0, Byte))
        Me.rb2.ForeColor = System.Drawing.SystemColors.ControlText
        Me.rb2.Location = New System.Drawing.Point(2, 22)
        Me.rb2.Name = "rb2"
        Me.rb2.Size = New System.Drawing.Size(79, 24)
        Me.rb2.TabIndex = 0
        Me.rb2.Text = "WareHouse12"
        Me.rb2.TextAlign = System.Drawing.ContentAlignment.MiddleCenter
        '
        'GroupBox3
        '
        Me.GroupBox3.Anchor = CType(((System.Windows.Forms.AnchorStyles.Top Or System.Windows.Forms.AnchorStyles.Left) _
            Or System.Windows.Forms.AnchorStyles.Right), System.Windows.Forms.AnchorStyles)
        Me.GroupBox3.BackColor = System.Drawing.Color.FromArgb(CType(CType(227, Byte), Integer), CType(CType(239, Byte), Integer), CType(CType(255, Byte), Integer))
        Me.GroupBox3.Controls.Add(Me.ChkNoBooks)
        Me.GroupBox3.Controls.Add(Me.btnBookCat)
        Me.GroupBox3.Controls.Add(Me.txtBookCat)
        Me.GroupBox3.Controls.Add(Me.chkBook)
        Me.GroupBox3.Controls.Add(Me.Label11)
        Me.GroupBox3.Font = New System.Drawing.Font("Microsoft Sans Serif", 9.0!, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point, CType(0, Byte))
        Me.GroupBox3.ForeColor = System.Drawing.Color.Maroon
        Me.GroupBox3.Location = New System.Drawing.Point(3, 54)
        Me.GroupBox3.Name = "GroupBox3"
        Me.GroupBox3.Size = New System.Drawing.Size(970, 36)
        Me.GroupBox3.TabIndex = 1
        Me.GroupBox3.TabStop = False
        Me.GroupBox3.Text = "Filtration Parameters"
        '
        'ChkNoBooks
        '
        Me.ChkNoBooks.ForeColor = System.Drawing.Color.Navy
        Me.ChkNoBooks.Location = New System.Drawing.Point(156, 14)
        Me.ChkNoBooks.Name = "ChkNoBooks"
        Me.ChkNoBooks.Size = New System.Drawing.Size(120, 19)
        Me.ChkNoBooks.TabIndex = 1
        Me.ChkNoBooks.Text = "Exclude Books"
        '
        'btnBookCat
        '
        Me.btnBookCat.Anchor = CType((System.Windows.Forms.AnchorStyles.Top Or System.Windows.Forms.AnchorStyles.Right), System.Windows.Forms.AnchorStyles)
        Me.btnBookCat.Font = New System.Drawing.Font("Wingdings 3", 6.0!, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, CType(2, Byte))
        Me.btnBookCat.ForeColor = System.Drawing.SystemColors.ControlText
        Me.btnBookCat.Image = CType(resources.GetObject("btnBookCat.Image"), System.Drawing.Image)
        Me.btnBookCat.ImeMode = System.Windows.Forms.ImeMode.NoControl
        Me.btnBookCat.Location = New System.Drawing.Point(944, 13)
        Me.btnBookCat.Name = "btnBookCat"
        Me.btnBookCat.Size = New System.Drawing.Size(18, 18)
        Me.btnBookCat.TabIndex = 5
        Me.btnBookCat.TabStop = False
        '
        'txtBookCat
        '
        Me.txtBookCat.Anchor = CType((System.Windows.Forms.AnchorStyles.Top Or System.Windows.Forms.AnchorStyles.Right), System.Windows.Forms.AnchorStyles)
        Me.txtBookCat.Font = New System.Drawing.Font("Microsoft Sans Serif", 9.75!, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, CType(0, Byte))
        Me.txtBookCat.Location = New System.Drawing.Point(824, 11)
        Me.txtBookCat.MaxLength = 20
        Me.txtBookCat.Name = "txtBookCat"
        Me.txtBookCat.ReadOnly = True
        Me.txtBookCat.Size = New System.Drawing.Size(140, 22)
        Me.txtBookCat.TabIndex = 4
        '
        'chkBook
        '
        Me.chkBook.ForeColor = System.Drawing.Color.Navy
        Me.chkBook.Location = New System.Drawing.Point(26, 14)
        Me.chkBook.Name = "chkBook"
        Me.chkBook.Size = New System.Drawing.Size(120, 19)
        Me.chkBook.TabIndex = 0
        Me.chkBook.Text = "Books only"
        '
        'Label11
        '
        Me.Label11.Anchor = CType((System.Windows.Forms.AnchorStyles.Top Or System.Windows.Forms.AnchorStyles.Right), System.Windows.Forms.AnchorStyles)
        Me.Label11.BackColor = System.Drawing.Color.Transparent
        Me.Label11.Font = New System.Drawing.Font("Microsoft Sans Serif", 9.0!, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point, CType(0, Byte))
        Me.Label11.ForeColor = System.Drawing.Color.Navy
        Me.Label11.Location = New System.Drawing.Point(704, 11)
        Me.Label11.Name = "Label11"
        Me.Label11.Size = New System.Drawing.Size(114, 22)
        Me.Label11.TabIndex = 3
        Me.Label11.Text = "Book Categories"
        Me.Label11.TextAlign = System.Drawing.ContentAlignment.MiddleLeft
        '
        'tdbBookCat
        '
        Me.tdbBookCat.AllowColMove = False
        Me.tdbBookCat.AllowColSelect = False
        Me.tdbBookCat.AllowRowSelect = False
        Me.tdbBookCat.AllowRowSizing = C1.Win.C1TrueDBGrid.RowSizingEnum.None
        Me.tdbBookCat.CaptionHeight = 17
        Me.tdbBookCat.CaptionStyle = Style14
        Me.tdbBookCat.CollapseColor = System.Drawing.Color.Black
        Me.tdbBookCat.EditorStyle = Style15
        Me.tdbBookCat.EvenRowStyle = Style16
        Me.tdbBookCat.ExpandColor = System.Drawing.Color.Black
        Me.tdbBookCat.FetchRowStyles = True
        Me.tdbBookCat.FilterBar = True
        Me.tdbBookCat.FilterBarStyle = Style17
        Me.tdbBookCat.FooterStyle = Style18
        Me.tdbBookCat.GroupByCaption = "Drag a column header here to group by that column"
        Me.tdbBookCat.GroupStyle = Style19
        Me.tdbBookCat.HeadingStyle = Style20
        Me.tdbBookCat.HighLightRowStyle = Style21
        Me.tdbBookCat.Images.Add(CType(resources.GetObject("tdbBookCat.Images"), System.Drawing.Image))
        Me.tdbBookCat.InactiveStyle = Style22
        Me.tdbBookCat.Location = New System.Drawing.Point(707, 142)
        Me.tdbBookCat.MarqueeStyle = C1.Win.C1TrueDBGrid.MarqueeEnum.DottedCellBorder
        Me.tdbBookCat.Name = "tdbBookCat"
        Me.tdbBookCat.OddRowStyle = Style23
        Me.tdbBookCat.PreviewInfo.Location = New System.Drawing.Point(0, 0)
        Me.tdbBookCat.PreviewInfo.Size = New System.Drawing.Size(0, 0)
        Me.tdbBookCat.PreviewInfo.ZoomFactor = 75.0R
        Me.tdbBookCat.PrintInfo.PageSettings = CType(resources.GetObject("tdbBookCat.PrintInfo.PageSettings"), System.Drawing.Printing.PageSettings)
        Me.tdbBookCat.PrintInfo.ShowOptionsDialog = False
        Me.tdbBookCat.RecordSelectorStyle = Style24
        Me.tdbBookCat.RecordSelectorWidth = 17
        Me.tdbBookCat.RowDivider.Color = System.Drawing.Color.DarkGray
        Me.tdbBookCat.RowDivider.Style = C1.Win.C1TrueDBGrid.LineStyleEnum.[Single]
        Me.tdbBookCat.RowHeight = 15
        Me.tdbBookCat.RowSubDividerColor = System.Drawing.Color.DarkGray
        Me.tdbBookCat.SelectedStyle = Style25
        Me.tdbBookCat.Size = New System.Drawing.Size(259, 204)
        Me.tdbBookCat.Style = Style26
        Me.tdbBookCat.TabIndex = 174
        Me.tdbBookCat.Visible = False
        Me.tdbBookCat.PropBag = resources.GetString("tdbBookCat.PropBag")
        '
        'chekall
        '
        Me.chekall.Location = New System.Drawing.Point(511, 376)
        Me.chekall.Name = "chekall"
        Me.chekall.Size = New System.Drawing.Size(75, 23)
        Me.chekall.TabIndex = 177
        Me.chekall.Tag = "True"
        Me.chekall.Text = "check all"
        Me.chekall.UseVisualStyleBackColor = True
        Me.chekall.Visible = False
        '
        'pnlclacc
        '
        Me.pnlclacc.Anchor = CType(((System.Windows.Forms.AnchorStyles.Top Or System.Windows.Forms.AnchorStyles.Left) _
            Or System.Windows.Forms.AnchorStyles.Right), System.Windows.Forms.AnchorStyles)
        Me.pnlclacc.Controls.Add(Me.txtGeneralSearch)
        Me.pnlclacc.Controls.Add(Me.Label1)
        Me.pnlclacc.Location = New System.Drawing.Point(1, 110)
        Me.pnlclacc.Name = "pnlclacc"
        Me.pnlclacc.Size = New System.Drawing.Size(646, 30)
        Me.pnlclacc.TabIndex = 6
        '
        'txtGeneralSearch
        '
        Me.txtGeneralSearch.Anchor = CType(((System.Windows.Forms.AnchorStyles.Top Or System.Windows.Forms.AnchorStyles.Left) _
            Or System.Windows.Forms.AnchorStyles.Right), System.Windows.Forms.AnchorStyles)
        '
        '
        '
        Me.txtGeneralSearch.Border.Class = "TextBoxBorder"
        Me.txtGeneralSearch.Border.CornerType = DevComponents.DotNetBar.eCornerType.Square
        Me.txtGeneralSearch.ButtonCustom.Text = "X"
        Me.txtGeneralSearch.ButtonCustom.Visible = True
        Me.txtGeneralSearch.Enabled = False
        Me.txtGeneralSearch.Location = New System.Drawing.Point(132, 5)
        Me.txtGeneralSearch.Name = "txtGeneralSearch"
        Me.txtGeneralSearch.Size = New System.Drawing.Size(511, 20)
        Me.txtGeneralSearch.TabIndex = 1
        Me.txtGeneralSearch.WatermarkImage = CType(resources.GetObject("txtGeneralSearch.WatermarkImage"), System.Drawing.Image)
        Me.txtGeneralSearch.WatermarkImageAlignment = System.Drawing.ContentAlignment.MiddleRight
        Me.txtGeneralSearch.WatermarkText = "Search   (Reference, Description , ...)"
        '
        'Label1
        '
        Me.Label1.BackColor = System.Drawing.Color.Transparent
        Me.Label1.Font = New System.Drawing.Font("Microsoft Sans Serif", 9.0!, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point, CType(0, Byte))
        Me.Label1.ForeColor = System.Drawing.Color.DimGray
        Me.Label1.Location = New System.Drawing.Point(7, 5)
        Me.Label1.Name = "Label1"
        Me.Label1.Size = New System.Drawing.Size(122, 18)
        Me.Label1.TabIndex = 0
        Me.Label1.Text = "Master Search"
        '
        'Panel1
        '
        Me.Panel1.BackColor = System.Drawing.Color.FromArgb(CType(CType(227, Byte), Integer), CType(CType(239, Byte), Integer), CType(CType(255, Byte), Integer))
        Me.Panel1.Controls.Add(Me.btnImportTransfer)
        Me.Panel1.Controls.Add(Me.btnClear)
        Me.Panel1.Controls.Add(Me.btnUnCheck)
        Me.Panel1.Controls.Add(Me.btnSave)
        Me.Panel1.Controls.Add(Me.btnCheck)
        Me.Panel1.Controls.Add(Me.btnShow)
        Me.Panel1.Controls.Add(Me.btnClose)
        Me.Panel1.Controls.Add(Me.Label6)
        Me.Panel1.Controls.Add(Me.txtItmNo)
        Me.Panel1.Controls.Add(Me.Label56)
        Me.Panel1.Controls.Add(Me.lblPosition)
        Me.Panel1.Dock = System.Windows.Forms.DockStyle.Bottom
        Me.Panel1.Location = New System.Drawing.Point(0, 596)
        Me.Panel1.Name = "Panel1"
        Me.Panel1.Size = New System.Drawing.Size(983, 67)
        Me.Panel1.TabIndex = 5
        '
        'btnImportTransfer
        '
        Me.btnImportTransfer.BackColor = System.Drawing.SystemColors.Control
        Me.btnImportTransfer.Cursor = System.Windows.Forms.Cursors.Hand
        Me.btnImportTransfer.Enabled = False
        Me.btnImportTransfer.FlatAppearance.BorderColor = System.Drawing.Color.Navy
        Me.btnImportTransfer.FlatAppearance.MouseDownBackColor = System.Drawing.Color.RoyalBlue
        Me.btnImportTransfer.FlatAppearance.MouseOverBackColor = System.Drawing.Color.White
        Me.btnImportTransfer.FlatStyle = System.Windows.Forms.FlatStyle.Flat
        Me.btnImportTransfer.Font = New System.Drawing.Font("Microsoft Sans Serif", 8.25!, System.Drawing.FontStyle.Bold)
        Me.btnImportTransfer.ForeColor = System.Drawing.SystemColors.ControlText
        Me.btnImportTransfer.Image = CType(resources.GetObject("btnImportTransfer.Image"), System.Drawing.Image)
        Me.btnImportTransfer.ImageAlign = System.Drawing.ContentAlignment.MiddleLeft
        Me.btnImportTransfer.ImeMode = System.Windows.Forms.ImeMode.NoControl
        Me.btnImportTransfer.Location = New System.Drawing.Point(397, 27)
        Me.btnImportTransfer.Name = "btnImportTransfer"
        Me.btnImportTransfer.Size = New System.Drawing.Size(111, 38)
        Me.btnImportTransfer.TabIndex = 9
        Me.btnImportTransfer.Text = "    Import From " & Global.Microsoft.VisualBasic.ChrW(13) & Global.Microsoft.VisualBasic.ChrW(10) & "     Transfer F3      "
        Me.btnImportTransfer.TextAlign = System.Drawing.ContentAlignment.MiddleRight
        Me.btnImportTransfer.UseVisualStyleBackColor = False
        '
        'ImgLst
        '
        Me.ImgLst.ImageStream = CType(resources.GetObject("ImgLst.ImageStream"), System.Windows.Forms.ImageListStreamer)
        Me.ImgLst.TransparentColor = System.Drawing.Color.Transparent
        Me.ImgLst.Images.SetKeyName(0, "Magnifying Glass.ico")
        '
        'Label10
        '
        Me.Label10.Anchor = CType(((System.Windows.Forms.AnchorStyles.Top Or System.Windows.Forms.AnchorStyles.Left) _
            Or System.Windows.Forms.AnchorStyles.Right), System.Windows.Forms.AnchorStyles)
        Me.Label10.Font = New System.Drawing.Font("Tahoma", 8.0!, System.Drawing.FontStyle.Bold)
        Me.Label10.ForeColor = System.Drawing.Color.Maroon
        Me.Label10.Location = New System.Drawing.Point(651, 110)
        Me.Label10.Name = "Label10"
        Me.Label10.Size = New System.Drawing.Size(326, 30)
        Me.Label10.TabIndex = 0
        Me.Label10.Text = "Data Selected Is Resulted From All Brands + All Categories + All Product Groups +" &
    " All Item Identifications Selected"
        Me.Label10.TextAlign = System.Drawing.ContentAlignment.MiddleRight
        '
        'stbTime
        '
        Me.stbTime.Name = "stbTime"
        Me.stbTime.Text = "10:30 PM"
        Me.stbTime.Width = 80
        '
        'stbLocation
        '
        Me.stbLocation.Name = "stbLocation"
        Me.stbLocation.Text = "Current Location: LocName"
        Me.stbLocation.Width = 150
        '
        'stbRemark
        '
        Me.stbRemark.Name = "stbRemark"
        Me.stbRemark.Width = 20
        '
        'stbDate
        '
        Me.stbDate.Alignment = System.Windows.Forms.HorizontalAlignment.Center
        Me.stbDate.Name = "stbDate"
        Me.stbDate.Text = "01/01/2004"
        '
        'stBar
        '
        Me.stBar.Location = New System.Drawing.Point(0, 663)
        Me.stBar.Name = "stBar"
        Me.stBar.Panels.AddRange(New System.Windows.Forms.StatusBarPanel() {Me.stbDate, Me.stbRemark, Me.stbLocation, Me.stbTime})
        Me.stBar.ShowPanels = True
        Me.stBar.Size = New System.Drawing.Size(983, 22)
        Me.stBar.SizingGrip = False
        Me.stBar.TabIndex = 16
        '
        'TxtMemo1
        '
        Me.TxtMemo1.Anchor = CType(((System.Windows.Forms.AnchorStyles.Top Or System.Windows.Forms.AnchorStyles.Left) _
            Or System.Windows.Forms.AnchorStyles.Right), System.Windows.Forms.AnchorStyles)
        Me.TxtMemo1.Location = New System.Drawing.Point(90, 0)
        Me.TxtMemo1.MaxLength = 3000
        Me.TxtMemo1.Multiline = True
        Me.TxtMemo1.Name = "TxtMemo1"
        Me.TxtMemo1.ReadOnly = True
        Me.TxtMemo1.ScrollBars = System.Windows.Forms.ScrollBars.Vertical
        Me.TxtMemo1.Size = New System.Drawing.Size(889, 53)
        Me.TxtMemo1.TabIndex = 3
        '
        'FlowLayoutPanel1
        '
        Me.FlowLayoutPanel1.BackColor = System.Drawing.Color.FromArgb(CType(CType(227, Byte), Integer), CType(CType(239, Byte), Integer), CType(CType(255, Byte), Integer))
        Me.FlowLayoutPanel1.Controls.Add(Me.GroupBox1)
        Me.FlowLayoutPanel1.Controls.Add(Me.GroupBox3)
        Me.FlowLayoutPanel1.Controls.Add(Me.GroupBox4)
        Me.FlowLayoutPanel1.Dock = System.Windows.Forms.DockStyle.Top
        Me.FlowLayoutPanel1.Location = New System.Drawing.Point(0, 63)
        Me.FlowLayoutPanel1.Name = "FlowLayoutPanel1"
        Me.FlowLayoutPanel1.Size = New System.Drawing.Size(983, 137)
        Me.FlowLayoutPanel1.TabIndex = 1
        '
        'Panel2
        '
        Me.Panel2.BackColor = System.Drawing.Color.FromArgb(CType(CType(227, Byte), Integer), CType(CType(239, Byte), Integer), CType(CType(255, Byte), Integer))
        Me.Panel2.Controls.Add(Me.Label58)
        Me.Panel2.Controls.Add(Me.txtDatePhyCnt)
        Me.Panel2.Dock = System.Windows.Forms.DockStyle.Top
        Me.Panel2.Location = New System.Drawing.Point(0, 34)
        Me.Panel2.Name = "Panel2"
        Me.Panel2.Size = New System.Drawing.Size(983, 29)
        Me.Panel2.TabIndex = 0
        '
        'Panel3
        '
        Me.Panel3.BackColor = System.Drawing.Color.FromArgb(CType(CType(227, Byte), Integer), CType(CType(239, Byte), Integer), CType(CType(255, Byte), Integer))
        Me.Panel3.Controls.Add(Me.TxtMemo1)
        Me.Panel3.Controls.Add(Me.Label10)
        Me.Panel3.Controls.Add(Me.GroupBox2)
        Me.Panel3.Controls.Add(Me.Label8)
        Me.Panel3.Controls.Add(Me.Label7)
        Me.Panel3.Controls.Add(Me.txtMemo2)
        Me.Panel3.Controls.Add(Me.pnlclacc)
        Me.Panel3.Dock = System.Windows.Forms.DockStyle.Top
        Me.Panel3.Location = New System.Drawing.Point(0, 200)
        Me.Panel3.Name = "Panel3"
        Me.Panel3.Size = New System.Drawing.Size(983, 143)
        Me.Panel3.TabIndex = 2
        '
        'pnlHead
        '
        Me.pnlHead.CanvasColor = System.Drawing.SystemColors.Control
        Me.pnlHead.ColorSchemeStyle = DevComponents.DotNetBar.eDotNetBarStyle.StyleManagerControlled
        Me.pnlHead.Controls.Add(Me.lblTitle)
        Me.pnlHead.DisabledBackColor = System.Drawing.Color.Empty
        Me.pnlHead.Dock = System.Windows.Forms.DockStyle.Top
        Me.pnlHead.Location = New System.Drawing.Point(0, 0)
        Me.pnlHead.Name = "pnlHead"
        Me.pnlHead.Size = New System.Drawing.Size(983, 34)
        Me.pnlHead.Style.Alignment = System.Drawing.StringAlignment.Center
        Me.pnlHead.Style.BackColor1.ColorSchemePart = DevComponents.DotNetBar.eColorSchemePart.PanelBackground
        Me.pnlHead.Style.BackColor2.ColorSchemePart = DevComponents.DotNetBar.eColorSchemePart.PanelBackground2
        Me.pnlHead.Style.Border = DevComponents.DotNetBar.eBorderType.SingleLine
        Me.pnlHead.Style.BorderColor.ColorSchemePart = DevComponents.DotNetBar.eColorSchemePart.PanelBorder
        Me.pnlHead.Style.ForeColor.ColorSchemePart = DevComponents.DotNetBar.eColorSchemePart.PanelText
        Me.pnlHead.Style.GradientAngle = 90
        Me.pnlHead.TabIndex = 178
        '
        'lblTitle
        '
        Me.lblTitle.Anchor = CType(((System.Windows.Forms.AnchorStyles.Top Or System.Windows.Forms.AnchorStyles.Left) _
            Or System.Windows.Forms.AnchorStyles.Right), System.Windows.Forms.AnchorStyles)
        '
        '
        '
        Me.lblTitle.BackgroundStyle.CornerType = DevComponents.DotNetBar.eCornerType.Square
        Me.lblTitle.Location = New System.Drawing.Point(0, 0)
        Me.lblTitle.Name = "lblTitle"
        Me.lblTitle.Size = New System.Drawing.Size(720, 34)
        Me.lblTitle.TabIndex = 64
        Me.lblTitle.Text = "<b><font size=""+3""><i>Select Stock Items </i><font color=""#B02B2C"">  To Count</fo" &
    "nt></font></b>" & Global.Microsoft.VisualBasic.ChrW(13) & Global.Microsoft.VisualBasic.ChrW(10)
        '
        'frmSelectStkItems
        '
        Me.AutoScaleBaseSize = New System.Drawing.Size(5, 13)
        Me.BackColor = System.Drawing.Color.FromArgb(CType(CType(206, Byte), Integer), CType(CType(220, Byte), Integer), CType(CType(237, Byte), Integer))
        Me.ClientSize = New System.Drawing.Size(983, 685)
        Me.ControlBox = False
        Me.Controls.Add(Me.tdbDetails)
        Me.Controls.Add(Me.Panel3)
        Me.Controls.Add(Me.FlowLayoutPanel1)
        Me.Controls.Add(Me.Panel2)
        Me.Controls.Add(Me.pnlHead)
        Me.Controls.Add(Me.Panel1)
        Me.Controls.Add(Me.chekall)
        Me.Controls.Add(Me.stBar)
        Me.Controls.Add(Me.tdbBookCat)
        Me.DoubleBuffered = True
        Me.FormBorderStyle = System.Windows.Forms.FormBorderStyle.None
        Me.KeyPreview = True
        Me.MaximizeBox = False
        Me.MinimizeBox = False
        Me.Name = "frmSelectStkItems"
        Me.StartPosition = System.Windows.Forms.FormStartPosition.CenterScreen
        Me.Text = "Select Stock Items to Count"
        CType(Me.tdbDetails, System.ComponentModel.ISupportInitialize).EndInit()
        Me.GroupBox1.ResumeLayout(False)
        Me.GroupBox1.PerformLayout()
        Me.GroupBox4.ResumeLayout(False)
        Me.FlowLayoutPanel2.ResumeLayout(False)
        Me.FlowLayoutPanel2.PerformLayout()
        CType(Me.txtStartDate, System.ComponentModel.ISupportInitialize).EndInit()
        CType(Me.txtEndDate, System.ComponentModel.ISupportInitialize).EndInit()
        CType(Me.ErrProvider, System.ComponentModel.ISupportInitialize).EndInit()
        Me.GroupBox2.ResumeLayout(False)
        Me.GroupBox3.ResumeLayout(False)
        Me.GroupBox3.PerformLayout()
        CType(Me.tdbBookCat, System.ComponentModel.ISupportInitialize).EndInit()
        Me.pnlclacc.ResumeLayout(False)
        Me.Panel1.ResumeLayout(False)
        CType(Me.stbTime, System.ComponentModel.ISupportInitialize).EndInit()
        CType(Me.stbLocation, System.ComponentModel.ISupportInitialize).EndInit()
        CType(Me.stbRemark, System.ComponentModel.ISupportInitialize).EndInit()
        CType(Me.stbDate, System.ComponentModel.ISupportInitialize).EndInit()
        Me.FlowLayoutPanel1.ResumeLayout(False)
        Me.Panel2.ResumeLayout(False)
        Me.Panel3.ResumeLayout(False)
        Me.Panel3.PerformLayout()
        Me.pnlHead.ResumeLayout(False)
        Me.ResumeLayout(False)

    End Sub

#End Region

    Private tbPhyCnt As String = "tbPhyCnt_" & vbUser
    Private tbPhyCntView As String = "tbPhyCntView_" & vbUser
    Private daDetails As SqlDataAdapter
    Private dsDetails As DataTable
    Private sStep, LogDate, LogTime As String
    Public fPrivList As Boolean
    Public daDet As SqlDataAdapter
    Public dsDet As DataSet
    Private MchUsr As New ArrayList
    Private dtcategory, dtBrand, dtproduct, dtIdentification As DataTable
    Private MemoPart1, MemoPart2 As String
    Private ShowAvailableQuantity As Boolean = False
    Public Overloads Sub show(ByRef _Step As String)
        sStep = _Step
        Me.Show()
        Application.DoEvents()
    End Sub

    Private Sub frmSelectStkItems_Load(ByVal sender As Object, ByVal e As System.EventArgs) Handles MyBase.Load

        Try
            PrepareHelp(Me, CONNECTION_STRING, vbCApp, sStep)
            Me.ContextMenu = NullConMenu

            AssignContextMenu(Me, ConMenu)

            ' lblLocations.Text = "Location : " & vbCLoc & " - " & vbCLocName

            With stBar
                .Panels(0).Text = Format(Date.Today, "dd/MM/yyyy")
                .Panels(1).Text = ""
                .Panels(2).Text = "Login User: " & vbUser
                .Panels(3).Text = Format(CType(Now.TimeOfDay.ToString, Date), "hh:mm tt")
                .Panels(1).Width = .Width - (.Panels(0).Width + .Panels(2).Width + .Panels(3).Width)
            End With

            With Clock
                .Interval = 100
                .Enabled = True
            End With

            'ButtonStatus(Me, False, False, "btnclose")
            '----------------------------LOG + PRIVILEGE-------------------------------------

            LogDate = Format(CDate(Date.Today), "MM/dd/yyyy")
            LogTime = Format(CType(Now.TimeOfDay.ToString, Date), "HH:mm:ss")

            If vbUser = "SU" Then
                fPrivList = True
                ShowAvailableQuantity = True
            Else
                PriviligeMenu(Me, sStep)
                fPrivList = bPrivList
                ShowAvailableQuantity = NegationPriv(sStep, "btnShowAvailableQuantity")
            End If
            If gbBook = False Then
                GroupBox3.Visible = False
                FlowLayoutPanel1.Height = 92
            End If

            btnShow.Enabled = fPrivList

            rb1.Text = vbLoc1Desc
            rb2.Text = vbLoc2Desc
            If vbEnabledWh2 = "Y" Then
                rb2.Enabled = True
            Else
                rb2.Enabled = False
            End If

            txtDatePhyCnt.Text = WorkingDay 'Format(Date.Today, "dd/MM/yyyy")
            TxtMemo1.Text = "Count Date : " & txtDatePhyCnt.Text
            'DTDetails = New DataTable("Details")
            'With DTDetails.Columns
            '    .Add("cntr", GetType(System.Int32))
            '    .Add("Chck", GetType(System.String))
            '    .Add("Stkid", GetType(System.Int32))
            '    .Add("Reference", GetType(System.String))
            '    If ChkBarcode.Checked Then
            '        .Add("Barcode", GetType(System.String))
            '    End If
            '    .Add("Supplier", GetType(System.String))
            '    .Add("FDesc", GetType(System.String))
            '    .Add("Size", GetType(System.String))
            '    .Add("Color", GetType(System.String))
            '    .Add("Colorcode", GetType(System.String))
            '    .Add("exp", GetType(System.String))
            '    .Add("lot#", GetType(System.String))
            '    .Add("BrandCode", GetType(System.String))
            '    .Add("Brand", GetType(System.String))
            '    .Add("CatCode", GetType(System.String))
            '    .Add("Category", GetType(System.String))
            '    .Add("ItemTypeCode", GetType(System.String))
            '    .Add("ItemType", GetType(System.String))
            '    .Add("AvlbQty", GetType(System.String))
            '    .Add("UnitCode", GetType(System.String))
            'End With
            FillGrid()
            ChkBarcode.Checked = CBool(IIf(CRetVal00 = "1", True, False))

            Dim SpcBetBtns As Integer
            SpcBetBtns = CInt((Panel1.Width - ((btnShow.Left * 2) + (btnShow.Width * 7))) / 6)
            btnCheck.Left = btnShow.Left + btnShow.Width + SpcBetBtns
            btnUnCheck.Left = btnCheck.Left + btnCheck.Width + SpcBetBtns
            btnImportTransfer.Left = btnUnCheck.Left + btnUnCheck.Width + SpcBetBtns
            btnClear.Left = btnImportTransfer.Left + btnImportTransfer.Width + SpcBetBtns
            btnSave.Left = btnClear.Left + btnClear.Width + SpcBetBtns
            btnClose.Left = btnSave.Left + btnSave.Width + SpcBetBtns
            rb1.Focus()
        Catch exc As Exception
            MessageBox.Show(exc.Message, Me.Text, MessageBoxButtons.OK, MessageBoxIcon.Error)
        End Try
    End Sub

    Private Sub FillGrid()

        Dim cnsql As SqlConnection
        Dim strsql As String
        Dim cmsql As SqlCommand
        Dim cbsql As SqlCommandBuilder
        Dim i As Int32

        Try

            If Not dsDetails Is Nothing Then dsDetails.Clear()

            cnsql = New SqlConnection(CONNECTION_STRING)
            cnsql.Open()

            strsql = "if exists (select * from dbo.sysobjects where id = object_id(N'[dbo].[" & tbPhyCnt & "]') "
            strsql = strsql & "and OBJECTPROPERTY(id, N'IsUserTable') = 1) drop table [dbo].[" & tbPhyCnt & "] "
            strsql = strsql & "CREATE TABLE " & tbPhyCnt & " ( [cntr] [numeric] (18,0) IDENTITY ,  [Chck] varchar(5),"
            strsql = strsql & "[Stkid] [numeric] (18,0),"
            strsql = strsql & "[Reference] [varchar] (" & gbmaxreflengh & "),"
            strsql = strsql & "[Supplier] [varchar] (40),"
            '  strsql = strsql & "[Barcode1] [varchar] (20),"
            'strsql = strsql & "[Barcode2] [varchar] (20),"
            strsql = strsql & "[FDesc] [nvarchar] (250),"
            strsql = strsql & "[gridcode] [nvarchar] (250),"
            strsql = strsql & "[Size] [nvarchar] (250),"
            strsql = strsql & "[Color] [nvarchar] (250),"
            strsql = strsql & "[Colorcode] [nvarchar] (250),"
            strsql = strsql & "[exp] [nvarchar] (50),"
            strsql = strsql & "[lot#] [nvarchar] (250),"
            strsql = strsql & "[BrandCode] [varchar] (3),"
            strsql = strsql & "[Brand] [nvarchar] (200),"
            strsql = strsql & "[CatCode] [varchar] (4),"
            strsql = strsql & "[Category] [varchar] (40),"
            strsql = strsql & "[ItemTypeCode] [varchar] (3),"
            strsql = strsql & "[ItemType] [varchar] (30),"
            strsql = strsql & "[ItemIdentifCode] [varchar] (3),"
            strsql = strsql & "[ItemIdentif] [nvarchar] (100),"
            strsql = strsql & "[AvlbQty] [decimal] (18,3),"
            strsql = strsql & "[UnitCode] [varchar] (6) "
            strsql = strsql & "Primary Key CLUSTERED (cntr))  "

            cmsql = New SqlCommand(strsql, cnsql)
            cmsql.ExecuteNonQuery()

            strsql = " create nonclustered index ix_" & tbPhyCnt & " on " & tbPhyCnt
            strsql &= " (Stkid) with ( statistics_norecompute = off, ignore_dup_key = off , allow_row_locks = on , allow_page_locks = on ) on [Primary]"

            cmsql = New SqlCommand(strsql, cnsql)
            cmsql.ExecuteNonQuery()

            strsql = "if exists (select * from dbo.sysobjects where id = object_id(N'[dbo].[" & tbPhyCntView & "]') "
            strsql = strsql & "and OBJECTPROPERTY(id, N'IsUserTable') = 1) drop table [dbo].[" & tbPhyCntView & "] "
            strsql = strsql & "CREATE TABLE " & tbPhyCntView & " ( [cntr] [numeric] (18,0) IDENTITY ,  [Chck] varchar(5),"
            strsql = strsql & "[Stkid] [numeric] (18,0),"
            strsql = strsql & "[Reference] [varchar] (" & gbmaxreflengh & "),"
            If ChkBarcode.Checked Then
                strsql = strsql & "[Barcode] [nvarchar] (20) ,"
            End If
            strsql = strsql & "[Supplier] [varchar] (40),"
            strsql = strsql & "[FDesc] [nvarchar] (250),"
            strsql = strsql & "[BrandCode] [varchar] (3),"
            strsql = strsql & "[Brand] [nvarchar] (200),"
            strsql = strsql & "[CatCode] [varchar] (4),"
            strsql = strsql & "[Category] [varchar] (40),"
            strsql = strsql & "[ItemTypeCode] [varchar] (3),"
            strsql = strsql & "[ItemType] [varchar] (30),"
            strsql = strsql & "[ItemIdentifCode] [varchar] (3),"
            strsql = strsql & "[ItemIdentif] [nvarchar] (100),"
            strsql = strsql & "[AvlbQty] [decimal] (18,3),"
            strsql = strsql & "[UnitCode] [varchar] (6) "
            strsql = strsql & "Primary Key CLUSTERED (cntr))  "

            cmsql.CommandText = strsql
            cmsql.ExecuteNonQuery()

            strsql = " create nonclustered index ix_" & tbPhyCntView & " on " & tbPhyCntView
            strsql &= " (Stkid) with ( statistics_norecompute = off, ignore_dup_key = off , allow_row_locks = on , allow_page_locks = on ) on [Primary]"

            cmsql = New SqlCommand(strsql, cnsql)
            cmsql.ExecuteNonQuery()

            cmsql.Dispose()
            cmsql = Nothing

            strsql = "select * from " & tbPhyCntView

            daDetails = New SqlDataAdapter(strsql, CONNECTION_STRING)
            daDetails.TableMappings.Add("Table", "Details")
            dsDetails = New DataTable

            With daDetails
                .FillSchema(dsDetails, SchemaType.Mapped)
                .Fill(dsDetails)
            End With

            cbsql = New SqlCommandBuilder(daDetails)

            daDetails.UpdateCommand = cbsql.GetUpdateCommand

            cbsql = Nothing

            With tdbDetails

                If Not .DataSource Is Nothing Then .DataSource = Nothing
                .DataSource = Nothing
                .DataSource = dsDetails
                .DataMember = "Details"
                .ColumnFooters = True
                With .Splits(0).DisplayColumns("cntr")
                    .Visible = False
                    .AllowSizing = False
                End With

                With .Splits(0).DisplayColumns("BrandCode")
                    .Visible = False
                    .AllowSizing = False
                End With
                With .Splits(0).DisplayColumns("ItemTypeCode")
                    .Visible = False
                    .AllowSizing = False
                End With
                With .Splits(0).DisplayColumns("ItemIdentifCode")
                    .Visible = False
                    .AllowSizing = False
                End With
                With .Splits(0).DisplayColumns("CatCode")
                    .Visible = False
                    .AllowSizing = False
                End With
                With .Splits(0).DisplayColumns("Chck")
                    .DataColumn.Caption = "Check"
                    .DataColumn.ValueItems.Presentation = PresentationEnum.CheckBox
                    .Width = 50
                    .Style.HorizontalAlignment = AlignHorzEnum.Center
                End With

                With .Splits(0).DisplayColumns("StkId")
                    .Visible = False
                    .AllowSizing = False
                End With

                With .Splits(0).DisplayColumns("Reference")
                    .Width = 150
                    .Locked = True
                    .Style.HorizontalAlignment = AlignHorzEnum.Near

                End With
                If ChkBarcode.Checked Then
                    With .Splits(0).DisplayColumns("Barcode")
                        .Width = 90
                        .Locked = True
                        .Style.HorizontalAlignment = AlignHorzEnum.Center
                    End With
                End If

                With .Splits(0).DisplayColumns("Supplier")
                    .Width = 150
                    .Locked = True
                    .Style.HorizontalAlignment = AlignHorzEnum.Near
                End With

                'With .Splits(0).DisplayColumns("Barcode1")
                '    .Width = 85
                '    .Locked = True
                '    .Style.HorizontalAlignment = AlignHorzEnum.Near
                'End With

                'With .Splits(0).DisplayColumns("Barcode2")
                '    .Width = 85
                '    .Locked = True
                '    .Style.HorizontalAlignment = AlignHorzEnum.Near
                'End With

                With .Splits(0).DisplayColumns("fdesc")
                    .DataColumn.Caption = "Description"
                    .Width = 400
                    .Locked = True
                    .Style.HorizontalAlignment = AlignHorzEnum.Near
                End With


                'With .Splits(0).DisplayColumns("gridcode")
                '    .DataColumn.Caption = "gridcode"
                '    .Width = 300
                '    .Locked = True
                '    .Visible = False
                '    .AllowSizing = False
                '    .Style.HorizontalAlignment = AlignHorzEnum.Center

                'End With
                'With .Splits(0).DisplayColumns("Size")
                '    .DataColumn.Caption = "Size"
                '    .Width = 50
                '    .Locked = True
                '    .Style.HorizontalAlignment = AlignHorzEnum.Center
                '    .Visible = gbFashion
                '    .AllowSizing = gbFashion
                'End With



                'With .Splits(0).DisplayColumns("Color")
                '    .DataColumn.Caption = "Color"
                '    .Width = 75
                '    .Locked = True
                '    .Style.HorizontalAlignment = AlignHorzEnum.Center
                '    .Visible = gbFashion
                '    .AllowSizing = gbFashion
                'End With
                'With .Splits(0).DisplayColumns("Colorcode")
                '    .DataColumn.Caption = "Color"
                '    .Width = 300
                '    .Locked = True
                '    .Visible = False
                '    .AllowSizing = False
                '    .Style.HorizontalAlignment = AlignHorzEnum.Center

                'End With

                'With .Splits(0).DisplayColumns("exp")
                '    .DataColumn.Caption = "Exp"
                '    .Width = 75
                '    .Locked = True
                '    .Style.HorizontalAlignment = AlignHorzEnum.Center
                '    .Visible = gbExpiry
                '    .AllowSizing = gbExpiry
                'End With

                'With .Splits(0).DisplayColumns("lot#")
                '    .DataColumn.Caption = "Lot#"
                '    .Width = 50
                '    .Locked = True
                '    .Style.HorizontalAlignment = AlignHorzEnum.Center
                '    .Visible = gbExpiry
                '    .AllowSizing = gbExpiry
                'End With

                With .Splits(0).DisplayColumns("Brand")
                    .DataColumn.Caption = "Brand"
                    .Width = 160
                    .Locked = True
                    .HeadingStyle.HorizontalAlignment = AlignHorzEnum.Center
                    .Style.Trimming = StringTrimming.None
                    .ButtonHeader = True
                    .HeadingStyle.ForegroundImage = ImgLst.Images(0)
                    .FooterStyle.HorizontalAlignment = AlignHorzEnum.Far
                End With

                With .Splits(0).DisplayColumns("Category")
                    .DataColumn.Caption = "Category"
                    .Width = 160
                    .Locked = True
                    .HeadingStyle.HorizontalAlignment = AlignHorzEnum.Center
                    .Style.Trimming = StringTrimming.None
                    .ButtonHeader = True
                    .HeadingStyle.ForegroundImage = ImgLst.Images(0)
                    .FooterStyle.HorizontalAlignment = AlignHorzEnum.Far
                End With
                With .Splits(0).DisplayColumns("ItemType")
                    .DataColumn.Caption = "Product Group"
                    .Width = 170
                    .Locked = True
                    .HeadingStyle.HorizontalAlignment = AlignHorzEnum.Near
                    .Style.Trimming = StringTrimming.None
                    .ButtonHeader = True
                    .HeadingStyle.ForegroundImage = ImgLst.Images(0)
                    .FooterStyle.HorizontalAlignment = AlignHorzEnum.Far
                End With
                With .Splits(0).DisplayColumns("ItemIdentif")
                    .DataColumn.Caption = "Item Identification"
                    .Width = 170
                    .Locked = True
                    .HeadingStyle.HorizontalAlignment = AlignHorzEnum.Near
                    .Style.Trimming = StringTrimming.None
                    .ButtonHeader = True
                    .HeadingStyle.ForegroundImage = ImgLst.Images(0)
                    .FooterStyle.HorizontalAlignment = AlignHorzEnum.Far
                End With

                With .Splits(0).DisplayColumns("AvlbQty")
                    'If rb1.Checked = True Then
                    '    .DataColumn.Caption = "Av. Qty" & vbLoc1Desc
                    'Else
                    .DataColumn.Caption = "Av. Qty"
                    'End If

                    .Width = 110
                    .Locked = True
                    .Style.HorizontalAlignment = AlignHorzEnum.Far
                    .DataColumn.EditMaskUpdate = True
                    '.DataColumn.NumberFormat = "FormatText Event"
                    .DataColumn.NumberFormat = "N" & DecNo

                    .Visible = ShowAvailableQuantity
                    .AllowSizing = ShowAvailableQuantity

                End With

                With .Splits(0).DisplayColumns("UnitCode")
                    .DataColumn.Caption = "Unit"
                    .Width = 60
                    .Locked = True
                    .Style.HorizontalAlignment = AlignHorzEnum.Center
                End With

                For i = 0 To .Splits(0).DisplayColumns.Count - 1
                    .Splits(0).DisplayColumns(i).HeadingStyle.HorizontalAlignment = AlignHorzEnum.Center
                    .Splits(0).DisplayColumns(i).HeadingStyle.VerticalAlignment = AlignVertEnum.Center

                    If dsDetails.Columns(i).DataType Is System.Type.GetType("System.String") Then
                        .Splits(0).DisplayColumns(i).DataColumn.FilterOperator = "LIKE"
                    Else
                        .Splits(0).DisplayColumns(i).DataColumn.FilterOperator = "="
                    End If

                Next

                .BackColor = SystemColors.Window
                .BringToFront()
            End With

            PositionChanged()

        Catch ex As Exception
            MsgBox(ex.Message, MsgBoxStyle.Critical, Me.Text)
        Finally

            If Not cmsql Is Nothing Then
                cmsql.Dispose()
                cmsql = Nothing
            End If

            If Not cnsql Is Nothing Then
                If cnsql.State = ConnectionState.Open Then cnsql.Close()
                cnsql.Dispose()
                cnsql = Nothing
            End If

        End Try

    End Sub

    Private Sub tdbDetails_AfterColUpdate(sender As Object, e As C1.Win.C1TrueDBGrid.ColEventArgs) Handles tdbDetails.AfterColUpdate
        Dim dr1 As DataRow
        Try
            If ChkBarcode.Checked Then
                For Each dr1 In dsDetails.Select("Stkid='" & tdbDetails.Columns("Stkid").Text & "' AND cntr <> " & tdbDetails.Columns("cntr").Text & "")
                    dr1("Chck") = tdbDetails.Columns("chck").Text.ToString
                Next
            End If
            tdbDetails.UpdateData()
            'daDetails.Update(dsDetails)

        Catch ex As Exception
            MessageBox.Show(ex.Message, Me.Text & " - Error.", MessageBoxButtons.OK, MessageBoxIcon.Error)
        End Try
    End Sub

    Private Sub tdbDetails_FilterChange(sender As Object, e As System.EventArgs) Handles tdbDetails.FilterChange
        Dim sFltr As String
        Try
            If tdbDetails.FilterActive = True Then
                tdbDetails.AllowFilter = False
                Me.Cursor = Cursors.WaitCursor
                sFltr = GetFiltration()
                GridFilterChange(tdbDetails, sFltr)

            End If

        Catch ex As Exception
            MessageBox.Show(ex.Message, Me.Text & " - Error.", MessageBoxButtons.OK, MessageBoxIcon.Error)
        Finally
            Me.Cursor = Cursors.Default
        End Try
    End Sub

    Private Sub tdbDetails_FormatText(ByVal sender As Object, ByVal e As C1.Win.C1TrueDBGrid.FormatTextEventArgs) Handles tdbDetails.FormatText
        'If CInt(e.Value) = CDbl(e.Value) Then
        '    e.Value = CInt(e.Value).ToString
        'Else
        Try
            e.Value = CDbl(e.Value).ToString
        Catch ex As Exception

        End Try


        'End If
    End Sub



    Private Sub frmSelectStkItems_Closing(ByVal sender As Object, ByVal e As System.ComponentModel.CancelEventArgs) Handles MyBase.Closing
        Dim strsql As String

        Dim cnSql As SqlConnection
        Dim cmSql As SqlCommand

        Dim dsSql As DataSet
        Dim msg As String
        Dim sTab As String

        If MsgBox("Do You Want To Close The Form ? ", MsgBoxStyle.Information.YesNo, Me.Text) = MsgBoxResult.No Then
            e.Cancel = True
        Else
            Try

                cnSql = New SqlConnection(CONNECTION_STRING)
                cnSql.Open()

                strsql = "if exists (select * from dbo.sysobjects where id = object_id(N'[dbo].[" & tbPhyCnt & "]') "
                strsql = strsql & "and OBJECTPROPERTY(id, N'IsUserTable') = 1) drop table [dbo].[" & tbPhyCnt & "]  "

                cmSql = New SqlCommand(strsql, cnSql)
                cmSql.ExecuteNonQuery()


                strsql = "if exists (select * from dbo.sysobjects where id = object_id(N'[dbo].[" & tbPhyCntView & "]') "
                strsql = strsql & "and OBJECTPROPERTY(id, N'IsUserTable') = 1) drop table [dbo].[" & tbPhyCntView & "]  "

                cmSql.CommandText = strsql
                cmSql.ExecuteNonQuery()


                sTab = "TmpBookCat_" + vbUser.ToString

                strsql = "if exists (select * from dbo.sysobjects where id = object_id(N'[dbo].[" & sTab & "]') "
                strsql = strsql & "and OBJECTPROPERTY(id, N'IsUserTable') = 1) drop table [dbo].[" & sTab & "]  "
                cmSql = New SqlCommand(strsql, cnSql)
                cmSql.ExecuteNonQuery()

            Catch exc As Exception
                MsgBox(exc.Message, MsgBoxStyle.Critical.OkOnly, Me.Text)

            Finally

                If Not cmSql Is Nothing Then
                    cmSql.Dispose()
                    cmSql = Nothing
                End If

                If Not cnSql Is Nothing Then
                    If cnSql.State = ConnectionState.Open Then cnSql.Close()
                    cnSql.Dispose()
                    cnSql = Nothing
                End If

                If Not dsDetails Is Nothing Then
                    With dsDetails
                        .Clear()
                        .Dispose()
                    End With
                    dsDetails = Nothing
                End If

                If Not daDetails Is Nothing Then
                    daDetails.Dispose()
                    daDetails = Nothing
                End If

                UpdateStepsLogging("mnuSelectStkItm")
                frmMain.mnuSelectStkItm.Checked = False

            End Try

        End If
    End Sub

    Private Sub frmSelectStkItems_KeyDown(ByVal sender As Object, ByVal e As System.Windows.Forms.KeyEventArgs) Handles MyBase.KeyDown
        Select Case e.KeyCode
            Case Keys.F4 : If btnShow.Focus Then btnShow_Click(btnShow, New System.EventArgs)
            Case Keys.F5 : If btnCheck.Focus Then btnCheck_Click(btnCheck, New System.EventArgs)
            Case Keys.F6 : If btnUnCheck.Focus Then btnCheck_Click(btnUnCheck, New System.EventArgs)
            Case Keys.F3 : If btnImportTransfer.Focus Then btnImportTransfer_Click(btnImportTransfer, New System.EventArgs)
            Case Keys.F7 : If btnClear.Focus Then btnClear_Click(btnClear, New System.EventArgs)
            Case Keys.F8 : If btnSearch.Focus Then btnSearch_Click(btnSearch, New System.EventArgs)
            Case Keys.F9 : If btnSave.Focus Then btnSave_Click(btnSave, New System.EventArgs)
            Case Keys.F12 : Me.Close()
        End Select
    End Sub

    Private Sub btnClose_Click(ByVal sender As System.Object, ByVal e As System.EventArgs) Handles btnClose.Click
        Me.Close()
    End Sub

    Private Sub txtMchName_KeyPress(ByVal sender As Object, ByVal e As System.Windows.Forms.KeyPressEventArgs) Handles txtMchName1.KeyPress, txtMchName2.KeyPress, txtMchName3.KeyPress, txtMchName4.KeyPress, txtMchName5.KeyPress, txtMchName6.KeyPress

        If Asc(e.KeyChar) = 8 OrElse Asc(e.KeyChar) = 28 OrElse Asc(e.KeyChar) = 255 Then
            Exit Sub
        End If

        If CType(sender, TextBox).Text.Length = 0 Then
            If e.KeyChar <> "U" AndAlso e.KeyChar <> "I" Then
                e.Handled = True
            End If
        End If

        If CType(sender, TextBox).Text.Length = 1 AndAlso CType(sender, TextBox).Text = "I" Then
            If e.KeyChar <> "T" Then
                e.Handled = True
            End If
        End If
    End Sub

    Private Sub PositionChanged()
        With Me.BindingContext(dsDetails)
            If .Count > 0 Then
                lblPosition.Text = (.Position + 1).ToString & "/" & .Count.ToString
            Else
                lblPosition.Text = "0/0"
            End If
        End With
    End Sub

    Private Sub btnCheck_Click(ByVal sender As System.Object, ByVal e As System.EventArgs) Handles btnCheck.Click, btnUnCheck.Click
        Dim cmsql As SqlCommand
        Dim cnsql As SqlConnection
        Dim strFlt As String
        Dim i As Integer
        Try
            Cursor.Current = Cursors.WaitCursor
            Dim dr As DataRow
            If sender Is btnUnCheck Then
                MemoPart1 = ""
            End If
            TxtMemo1.Text = "Count Date : " & txtDatePhyCnt.Text & MemoPart2 & MemoPart1
            If Not dtBrand Is Nothing Then
                For Each dr In dtBrand.Select()
                    dr.Item("Chk") = "false"
                Next
            End If
            If Not dtcategory Is Nothing Then
                For Each dr In dtcategory.Select()
                    dr.Item("Chk") = "false"
                Next
            End If
            If Not dtproduct Is Nothing Then
                For Each dr In dtproduct.Select()
                    dr.Item("Chk") = "false"
                Next
            End If
            If Not dtIdentification Is Nothing Then
                For Each dr In dtIdentification.Select()
                    dr.Item("Chk") = "false"
                Next
            End If
            If sender Is btnCheck Then
                strsql = "update " & tbPhyCntView & " set [" & tdbDetails.Columns(1).DataField & "] = 'TRUE' where [" & tdbDetails.Columns(1).DataField & "] = 'FALSE' " &
                    CStr(IIf(txtGeneralSearch.Text.Trim <> "" And Not strFlt Is Nothing, " and " & strFlt, ""))
            ElseIf sender Is btnUnCheck Then
                strsql = "update " & tbPhyCntView & " set [" & tdbDetails.Columns(1).DataField & "] = 'FALSE' where [" & tdbDetails.Columns(1).DataField & "] = 'TRUE' " &
                    CStr(IIf(txtGeneralSearch.Text.Trim <> "" And Not strFlt Is Nothing, " and " & strFlt, ""))
            End If
            With tdbDetails
                For i = 0 To .Columns.Count - 1
                    If .Columns(i).FilterText <> "" AndAlso .Columns(i).ValueItems.Presentation <> PresentationEnum.CheckBox Then
                        strsql &= "and [" & .Columns(i).DataField & "] " & .Columns(i).FilterOperator & " "

                        If dsDetails.Columns(i).DataType Is System.Type.GetType("System.Decimal") Then
                            strsql &= .Columns(i).FilterText.Replace("'", "''").Replace("*", "%").Trim & " "
                        ElseIf dsDetails.Columns(i).DataType Is System.Type.GetType("System.DateTime") Then
                            strsql &= "'" & CType(.Columns(i).FilterText.Replace("'", "''").Replace("*", "%").Trim, DateTime).ToString("MM/dd/yyyy") & "' "
                        Else
                            strsql &= "'" & .Columns(i).FilterText.Replace("'", "''").Replace("*", "%").Trim & "' "
                        End If
                    End If
                Next
            End With

            Execute(strsql)
            dsDetails.Clear()
            daDetails.Fill(dsDetails)
            txtItmNo.Text = dsDetails.Compute("count([Chck])", "[Chck]=True").ToString
            tdbDetails.Focus()
        Catch exp As Exception
            MessageBox.Show(exp.Message, Me.Text & ": General Error",
            MessageBoxButtons.OK, MessageBoxIcon.Error)
        Finally
            Cursor.Current = Cursors.Default
        End Try


        'Dim i As Integer
        'Dim cmsql As SqlCommand
        'Dim cnsql As SqlConnection
        'Dim strsql As String
        'Try

        '    Cursor.Current = Cursors.WaitCursor

        '    If tdbDetails.Splits(0).Rows.Count > 0 Then
        '        If CType(sender, Button).Name = "btnCheck" Then
        '            strsql = "update " & tbPhyCnt & " set [Chck]='True' where [Chck]='False' "
        '            If tdbDetails.Columns(2).FilterText <> "" Then strsql = strsql & "and reference like '" & tdbDetails.Columns(2).FilterText.Replace("*", "%") & "' "
        '            If tdbDetails.Columns(3).FilterText <> "" Then strsql = strsql & "and Supplier like '" & tdbDetails.Columns(3).FilterText.Replace("*", "%") & "' "
        '            If tdbDetails.Columns(4).FilterText <> "" Then strsql = strsql & "and barcode1 Like '" & tdbDetails.Columns(4).FilterText.Replace("*", "%") & "' "
        '            If tdbDetails.Columns(5).FilterText <> "" Then strsql = strsql & "and barcode2 Like '" & tdbDetails.Columns(5).FilterText.Replace("*", "%") & "' "
        '            If tdbDetails.Columns(6).FilterText <> "" Then strsql = strsql & "and fdesc Like '" & tdbDetails.Columns(6).FilterText.Replace("*", "%") & "' "
        '            If tdbDetails.Columns(7).FilterText <> "" Then strsql = strsql & "and Brand Like '" & tdbDetails.Columns(7).FilterText.Replace("*", "%") & "' "
        '            If tdbDetails.Columns(8).FilterText <> "" Then strsql = strsql & "and ItemType like '" & tdbDetails.Columns(8).FilterText.Replace("*", "%") & "' "
        '            If tdbDetails.Columns(9).FilterText <> "" Then strsql = strsql & "and avlbqty = " & tdbDetails.Columns(9).FilterText.Replace("*", "%")
        '            If tdbDetails.Columns(10).FilterText <> "" Then strsql = strsql & "and unitcode Like '" & tdbDetails.Columns(10).FilterText.Replace("*", "%") & "' "

        '            cnsql = New SqlConnection(CONNECTION_STRING)
        '            cnsql.Open()
        '            cmsql = New SqlCommand(strsql, cnsql)
        '            cmsql.ExecuteNonQuery()
        '        Else
        '            strsql = "update " & tbPhyCnt & " set [Chck]='False' where [Chck]='True' "
        '            If tdbDetails.Columns(2).FilterText <> "" Then strsql = strsql & "and reference like '" & tdbDetails.Columns(2).FilterText.Replace("*", "%") & "' "
        '            If tdbDetails.Columns(3).FilterText <> "" Then strsql = strsql & "and Supplier like '" & tdbDetails.Columns(3).FilterText.Replace("*", "%") & "' "
        '            If tdbDetails.Columns(4).FilterText <> "" Then strsql = strsql & "and barcode1 Like '" & tdbDetails.Columns(4).FilterText.Replace("*", "%") & "' "
        '            If tdbDetails.Columns(5).FilterText <> "" Then strsql = strsql & "and barcode2 Like '" & tdbDetails.Columns(5).FilterText.Replace("*", "%") & "' "
        '            If tdbDetails.Columns(6).FilterText <> "" Then strsql = strsql & "and fdesc Like '" & tdbDetails.Columns(6).FilterText.Replace("*", "%") & "' "
        '            If tdbDetails.Columns(7).FilterText <> "" Then strsql = strsql & "and Brand Like '" & tdbDetails.Columns(7).FilterText.Replace("*", "%") & "' "
        '            If tdbDetails.Columns(8).FilterText <> "" Then strsql = strsql & "and ItemType like '" & tdbDetails.Columns(8).FilterText.Replace("*", "%") & "' "
        '            If tdbDetails.Columns(9).FilterText <> "" Then strsql = strsql & "and avlbqty = " & tdbDetails.Columns(9).FilterText.Replace("*", "%")
        '            If tdbDetails.Columns(10).FilterText <> "" Then strsql = strsql & "and unitcode Like '" & tdbDetails.Columns(10).FilterText.Replace("*", "%") & "' "

        '            cnsql = New SqlConnection(CONNECTION_STRING)
        '            cnsql.Open()
        '            cmsql = New SqlCommand(strsql, cnsql)
        '            cmsql.ExecuteNonQuery()
        '        End If
        '        strsql = "select * from " & tbPhyCnt
        '        daDetails.SelectCommand.CommandText = strsql
        '        daDetails.Fill(dsDetails, "Details")
        '        tdbDetails.Focus()
        '        tdbDetails.Row = 0
        '    End If

        '    txtItmNo.Text = dsDetails.Tables(0).Compute("count([Chck])", "[Chck]=True").ToString

        'Catch ex As Exception
        '    MsgBox(ex.Message, MsgBoxStyle.Critical, Me.Text)
        'Finally
        '    Cursor.Current = Cursors.Default
        'End Try
    End Sub

    Private Sub btnShow_Click(ByVal sender As Object, ByVal e As System.EventArgs) Handles btnShow.Click

        Dim i As Integer
        Dim vbCnt As Int32
        Dim strsql As String
        Dim cnSql As SqlConnection
        Dim cmSql As SqlCommand
        Dim frm As New FrmStatus
        Dim dr As DataRow

        Try
            TxtMemo1.Text = "Count Date : " & txtDatePhyCnt.Text & MemoPart2 & MemoPart1
            If Not dtBrand Is Nothing Then
                For Each dr In dtBrand.Select()
                    dr.Item("Chk") = "false"
                Next
            End If
            If Not dtcategory Is Nothing Then
                For Each dr In dtcategory.Select()
                    dr.Item("Chk") = "false"
                Next
            End If
            If Not dtproduct Is Nothing Then
                For Each dr In dtproduct.Select()
                    dr.Item("Chk") = "false"
                Next
            End If
            If Not dtIdentification Is Nothing Then
                For Each dr In dtIdentification.Select()
                    dr.Item("Chk") = "false"
                Next
            End If
            txtGeneralSearch.Enabled = True
            With frm
                .lblStatus.Text = "Loading Stock File, Please Wait..."
                .Text = "Loading Stock File"
                .Show()
                .Refresh()
            End With
            dsDetails.Clear()

            Cursor.Current = Cursors.WaitCursor

            strsql = "truncate table " & tbPhyCnt

            strsql = strsql & " insert into " & tbPhyCnt

            strsql &= " SELECT DISTINCT  'False' ,"
            strsql &= " stk_recid ,"
            strsql &= " stk_ref  ,"
            strsql &= " Sup_Name ,"
            strsql &= " LTRIM(RTRIM(stk_desc)) + ( CASE WHEN STK_NEWUSED = 'U' THEN ' (Used)'"
            strsql &= " ELSE ''"
            strsql &= " END ) ,"
            strsql &= " qty1.STQ_Grid ,"
            strsql &= " qty1. STQ_DetGrid ,"
            strsql &= " Color_Reference ,"
            strsql &= " Color_Code ,"
            strsql &= " CASE WHEN SUBSTRING(qty1.STQ_ExpiryDt, 0, 3) = '00'"
            strsql &= " THEN SUBSTRING(qty1.STQ_ExpiryDt, 4, 8)"
            strsql &= " ELSE qty1.STQ_ExpiryDt"
            strsql &= " END AS [exp] ,"
            strsql &= " qty1.STQ_BatchInfo ,"
            strsql &= " br_Code ,br_Name ,Cat_Code,Cat_Desc ,it_code,"
            strsql &= " IT_Desc ,ID_Code, ID_Desc,"
            strsql &= " isnull(qty2.stq_available,0) -isnull(qty2.STQ_RESERVED,0) +isnull(resQty,0)  ,"
            strsql &= " stk_unit"

            If ClocHO Then
                strsql &= " from STK04_stkfil t1 "
                strsql &= " Left JOIN STK04_stkqty qty1 ON stk_recid = stq_stkrecid"
                strsql &= " AND qty1.stq_loccode = '00'"
                strsql &= " LEFT JOIN STK04_stkqty qty2 ON qty2.STQ_STKRECID = qty1.stq_stkrecid AND qty2.stq_loccode ='" & vbCLoc & "'"
                strsql &= " AND ISNULL(qty2.STQ_BatchInfo,'') = ISNULL(qty1.STQ_BatchInfo,'')"
                strsql &= " AND ISNULL(qty2.STQ_ExpiryDt,'') = ISNULL(qty1.STQ_ExpiryDt,'')"
                strsql &= " AND ISNULL(qty2.STQ_Color,'') = ISNULL(qty1.STQ_Color,'')"
                strsql &= " AND ISNULL(qty2.STQ_DetGrid,'') = ISNULL(qty1.STQ_DetGrid,'')"
                strsql &= " AND ISNULL(qty2.STQ_Grid,'') =ISNULL( qty1.STQ_Grid,'')"


                ' strsql = strsql & " inner join STK04_stkqty on stk_recid = stq_stkrecid and stq_loccode = '00' "
            Else
                strsql &= " from pos_stkfil t1 "
                strsql &= " LEFT JOIN pos_stkqty qty1 ON stk_recid = stq_stkrecid"
                strsql &= " AND qty1.stq_loccode = '00'"
                strsql &= " LEFT JOIN pos_stkqty qty2 ON qty2.STQ_STKRECID = qty1.stq_stkrecid AND qty2.stq_loccode ='" & vbCLoc & "'"
                strsql &= " AND ISNULL(qty2.STQ_BatchInfo,'') = ISNULL(qty1.STQ_BatchInfo,'')"
                strsql &= " AND ISNULL(qty2.STQ_ExpiryDt,'') = ISNULL(qty1.STQ_ExpiryDt,'')"
                strsql &= " AND ISNULL(qty2.STQ_Color,'') = ISNULL(qty1.STQ_Color,'')"
                strsql &= " AND ISNULL(qty2.STQ_DetGrid,'') = ISNULL(qty1.STQ_DetGrid,'')"
                strsql &= " AND ISNULL(qty2.STQ_Grid,'') =ISNULL( qty1.STQ_Grid,'')"

                '   strsql = strsql & " inner join pos_stkqty on stk_recid = stq_stkrecid and stq_loccode = '00' "
            End If


            strsql = strsql & " inner join stk04_itemtype on stk_itmtype = IT_Code   "
            strsql = strsql & " inner join STK04_ItmIdentif on STK_ITMIDENT = ID_RECID   "
            strsql = strsql & " inner join stk04_supplier on stk_supplier = Sup_Code   "

            strsql &= "	left JOIN dbo.Stk04_COLOR ON Color_Code = qty1.STQ_Color      "

            strsql &= " LEFT JOIN dbo.Stk04_Brand ON Stk04_Brand.br_Code = t1.STK_Brand  "
            strsql &= " LEFT JOIN dbo.Stk04_Category ON Cat_Code = t1.STK_CATEGORY  "
            If txtBookCat.Text.Trim <> "" And ChkNoBooks.Checked = False Then
                strsql &= " left join bibl1_book on bo_recid = stk_bookid "
            End If

            strsql &= " LEFT JOIN ( "
            strsql &= " SELECT  ROD_StkRecid ,"
            strsql &= " isnull(SUM(ROD_ResQty * CASE WHEN ROH_Status = 'N' THEN 1"
            strsql &= " ELSE -1"
            strsql &= " END),0) resQty,"
            strsql &= " ROD_Color ,"
            strsql &= " ROD_Grid ,"
            strsql &= " ROD_DetGrid ,"
            strsql &= " ROD_ExpDate ,"
            strsql &= " ROD_Batch"
            strsql &= " FROM    dbo.stk04_ResOrdH"
            strsql &= " INNER JOIN dbo.STK04_ResOrdD ON STK04_ResOrdD.ROD_Loc = stk04_ResOrdH.ROH_Loc"
            strsql &= " AND STK04_ResOrdD.ROD_Hrecid = stk04_ResOrdH.ROH_Recid where ROH_Loc = '" & vbCLoc & "'"
            strsql &= " GROUP BY ROD_StkRecid ,"
            strsql &= " ROD_Color ,"
            strsql &= " ROD_Grid ,"
            strsql &= " ROD_DetGrid ,"
            strsql &= " ROD_ExpDate ,"
            strsql &= " ROD_Batch"
            strsql &= " ) tableResQty ON STK_RECID = ROD_StkRecid"
            strsql &= " AND COALESCE(ROD_Grid, '') = COALESCE(qty2.stq_grid,"
            strsql &= " '')"
            strsql &= " AND COALESCE(ROD_DetGrid, '') = COALESCE(qty2.stq_detgrid,"
            strsql &= " '')"
            strsql &= " AND COALESCE(ROD_ExpDate, '') = COALESCE(qty2.STQ_ExpiryDt,"
            strsql &= " '')"
            strsql &= " AND COALESCE(ROD_Batch, '') = COALESCE(qty2.STQ_BatchInfo,"
            strsql &= " '')"
            strsql &= " AND COALESCE(ROD_Color, '') = COALESCE(qty2.STQ_Color,"
            strsql &= " '')"

            strsql &= " WHERE stk_active = 'Y'"
            'strsql &= " AND stk_service = 'R'"
            'ch @ 08/12/2016 
            'strsql &= "  and (COALESCE(qty1.STQ_ExpiryDt, '') <> CASE WHEN STK_ItmClass = 'e' THEN '' ELSE '0' END) "
            'ch @31/07/2019 mail(ERP_Inactivate_Consignment_Related_Normal)
            strsql &= " and not exists ( select top 1 1 from V_StockNormalCons  where stk_recid =NSTK_RECID ) "
            If chkBook.Checked = True Then
                strsql &= " And Stk_BookID IS NOT NULL AND Stk_BookID <> 0 "
            End If

            If txtBookCat.Text.Trim <> "" And ChkNoBooks.Checked = False Then
                strsql &= " AND (BO_CatRec1 IN ('" & Strings.Left(txtBookCat.Text, Len(txtBookCat.Text) - 1).Replace(",", "','") & "') "
                strsql &= " OR BO_CatRec1 IN ('" & Strings.Left(txtBookCat.Text, Len(txtBookCat.Text) - 1).Replace(",", "','") & "') "
                strsql &= " OR BO_CatRec1 IN ('" & Strings.Left(txtBookCat.Text, Len(txtBookCat.Text) - 1).Replace(",", "','") & "') )"
            End If

            If ChkNoBooks.Checked = True Then
                strsql &= " And (Stk_BookID IS NULL OR Stk_BookID = 0) "
            End If

            If rbDateRng.Checked = True Then
                strsql = strsql & " and not exists "
                strsql = strsql & "(select * from tbl_PhysicalCountDetail t2 inner join tbl_PhysicalCountheader t3 on "
                strsql &= "t2.pcd_Hrecid=t3.pch_recid and t2.pcd_loc=t3.pch_loc where qty2.stq_loccode = t2.pcd_loc and t1.stk_recid=t2.pcd_stkid and t3.pch_status='N' "

                If Not txtStartDate.Value Is DBNull.Value Then strsql = strsql & " and pcd_datecount>='" & Format(txtStartDate.Value, "MM/dd/yyyy") & "' "
                If Not txtEndDate.Value Is DBNull.Value Then strsql = strsql & " and pcd_datecount<='" & Format(txtEndDate.Value, "MM/dd/yyyy") & "' "

                'If rb1.Checked = True Then
                strsql &= "and pcd_whouse = '1' AND Pch_Updated ='N' and Pch_Loc = '" & vbCLoc & "'"
                'Else
                '    strsql &= "and pcd_whouse = '2' "
                'End If

                strsql = strsql & " )"

            End If

            cnSql = New SqlConnection(CONNECTION_STRING)
            cnSql.Open()
            cmSql = New SqlCommand(strsql, cnSql)

            cmSql.CommandTimeout = 600

            cmSql.ExecuteNonQuery()
            strsql = "truncate table " & tbPhyCntView
            strsql &= " insert into " & tbPhyCntView
            strsql &= " SELECT distinct Chck,Stkid,Reference," & IIf(ChkBarcode.Checked, "dbar_barcode ,", "").ToString & " Supplier,FDesc,BrandCode,Brand,Catcode,Category,ItemTypeCode,ItemType,ItemIdentifCode,ItemIdentif,SUM(AvlbQty),UnitCode from " & tbPhyCnt
            If ChkBarcode.Checked Then
                strsql &= "      LEFT JOIN dbo.STK04_DetBarcode ON STK04_DetBarcode.dbar_stkid = Stkid "
                strsql &= "                                     AND COALESCE(dbar_grid, '') = COALESCE(gridcode,"
                strsql &= "    '')"
                strsql &= "                                 AND COALESCE(dbar_Detgrid, '') = COALESCE(Size,"
                strsql &= "   '')"
                strsql &= "                                 AND COALESCE(dbar_expdate, '') = COALESCE(exp,"
                strsql &= "   '')"
                strsql &= "                                 AND COALESCE(dbar_batch, '') = COALESCE([lot#],"
                strsql &= "   '')"
                strsql &= "                                 AND COALESCE(dbar_color, '') = COALESCE(Colorcode,"
                strsql &= "   '')"
            End If
            strsql &= " GROUP BY Chck,Stkid,Reference,Supplier,FDesc,Brand,ItemType,ItemIdentifCode,ItemIdentif,UnitCode,catcode,Category,ItemTypecode,BrandCode" & IIf(ChkBarcode.Checked, ",dbar_barcode ", "").ToString & ""

            cmSql.CommandText = strsql
            cmSql.ExecuteNonQuery()
            strsql = "select * from " & tbPhyCntView & " order by 3"

            daDetails.SelectCommand.CommandText = strsql
            dsDetails.Clear()
            daDetails.Fill(dsDetails)
            vbCnt = dsDetails.Rows.Count

            If vbCnt > 0 Then

                With tdbDetails
                    .Focus()
                    .Col = 0
                End With

                btnCheck.Enabled = True
                btnUnCheck.Enabled = True
                btnSave.Enabled = True
                btnClear.Enabled = True
                btnImportTransfer.Enabled = True
                lblPosition.Text = tdbDetails.Row + 1 & "/" & tdbDetails.Splits(0).Rows.Count

            Else

                MessageBox.Show("No Data Were Found", Me.Text, MessageBoxButtons.OK, MessageBoxIcon.Information)
                btnCheck.Enabled = False
                btnImportTransfer.Enabled = False
                lblPosition.Text = "0/0"

            End If

            For i = 0 To tdbDetails.Columns.Count - 1
                tdbDetails.Splits(0).DisplayColumns(i).DataColumn.FilterText = ""
            Next

            If rb1.Checked = True Then
                If Not vbLoc1Desc Is Nothing AndAlso vbLoc1Desc.Trim <> "" Then
                    tdbDetails.Splits(0).DisplayColumns("AvlbQty").DataColumn.Caption = "Av. Qty-" & vbLoc1Desc
                Else
                    tdbDetails.Splits(0).DisplayColumns("AvlbQty").DataColumn.Caption = "Av. Qty"
                End If

            Else
                If Not vbLoc2Desc Is Nothing AndAlso vbLoc2Desc.Trim <> "" Then
                    tdbDetails.Splits(0).DisplayColumns("AvlbQty").DataColumn.Caption = "Av. Qty-" & vbLoc2Desc
                Else

                    tdbDetails.Splits(0).DisplayColumns("AvlbQty").DataColumn.Caption = "Av. Qty"
                End If

            End If

            frm.Close()

        Catch exc As Exception
            frm.Close()
            MessageBox.Show("Error Occured:" & vbCrLf & exc.Message, Me.Text & " : General Error", MessageBoxButtons.OK, MessageBoxIcon.Error)

        Finally
            If Not frm Is Nothing Then frm = Nothing

            If Not cmSql Is Nothing Then
                cmSql.Dispose()
                cmSql = Nothing
            End If

            If Not cnSql Is Nothing Then
                If cnSql.State = ConnectionState.Open Then cnSql.Close()
                cnSql.Dispose()
                cnSql = Nothing
            End If

            Cursor.Current = Cursors.Default
            txtItmNo.Text = "0"

        End Try

    End Sub

    Private Sub tdbDetails_RowColChange(ByVal sender As Object, ByVal e As C1.Win.C1TrueDBGrid.RowColChangeEventArgs) Handles tdbDetails.RowColChange
        If tdbDetails.Splits(0).Rows.Count <> 0 Then
            With Me.BindingContext(dsDetails)
                lblPosition.Text = (.Position + 1).ToString & " / " & .Count.ToString
            End With
        End If
    End Sub

    Private Sub Clock_Tick(ByVal sender As Object, ByVal e As System.EventArgs) Handles Clock.Tick
        stBar.Panels(3).Text = Format(CType(Now.TimeOfDay.ToString, Date), "hh:mm tt")
    End Sub

    Private Sub tdbDetails_Enter(ByVal sender As System.Object, ByVal e As System.EventArgs) Handles tdbDetails.Enter
        CType(sender, C1.Win.C1TrueDBGrid.C1TrueDBGrid).AddCellStyle(CellStyleFlag.CurrentCell, focusCol)
    End Sub

    Private Sub tdbDetails_Leave(ByVal sender As System.Object, ByVal e As System.EventArgs) Handles tdbDetails.Leave
        CType(sender, C1.Win.C1TrueDBGrid.C1TrueDBGrid).AddCellStyle(CellStyleFlag.CurrentCell, freeCol)
    End Sub

    Private Sub tdbPreview_KeyDown(ByVal sender As System.Object, ByVal e As System.Windows.Forms.KeyEventArgs) Handles tdbDetails.KeyDown
        If e.KeyCode = Keys.Delete Then e.Handled = True

    End Sub
    Private Sub ConMenu_Popup(ByVal sender As System.Object, ByVal e As System.EventArgs) Handles ConMenu.Popup
        If Me.ActiveControl.GetType Is GetType(TextBox) Then
            If CType(Me.ActiveControl, TextBox).SelectionLength > 0 Then

                Me.ctmCopy.Enabled = True
                If CType(Me.ActiveControl, TextBox).Enabled = True AndAlso
                       CType(Me.ActiveControl, TextBox).ReadOnly = False Then
                    Me.ctmCut.Enabled = True
                    If Clipboard.GetDataObject.GetDataPresent(DataFormats.Text) Then
                        Me.ctmPast.Enabled = True
                    Else
                        Me.ctmPast.Enabled = False
                    End If
                Else
                    Me.ctmCut.Enabled = False
                    Me.ctmPast.Enabled = False
                End If

            Else
                Me.ctmCopy.Enabled = False
                Me.ctmCut.Enabled = False
                If CType(Me.ActiveControl, TextBox).Enabled = True AndAlso
                   CType(Me.ActiveControl, TextBox).ReadOnly = False Then
                    If Clipboard.GetDataObject.GetDataPresent(DataFormats.Text) Then
                        Me.ctmPast.Enabled = True
                    Else
                        Me.ctmPast.Enabled = False
                    End If
                Else
                    Me.ctmPast.Enabled = False
                End If
            End If
        ElseIf Me.ActiveControl.GetType.ToString = "C1.Win.C1TrueDBGrid.GridEditor" Then
            If CType(Me.ActiveControl, TextBox).SelectionLength > 0 Then
                Me.ctmCopy.Enabled = True
            Else
                Me.ctmCopy.Enabled = False
            End If
            Me.ctmCut.Enabled = False
            Me.ctmPast.Enabled = False
        Else
            Me.ctmCopy.Enabled = True
            Me.ctmCut.Enabled = False
            Me.ctmPast.Enabled = False
        End If
    End Sub

    Private Sub btnSave_Click(ByVal sender As System.Object, ByVal e As System.EventArgs) Handles btnSave.Click
        Dim strsql As String
        Dim trnsql As SqlTransaction
        Dim cnsql As SqlConnection
        Dim cmsql As SqlCommand
        Dim drSql As SqlDataReader
        Dim cnt As Int32
        Dim strPT As String
        Dim HOLinkServer As String
        Dim dtPT As DataTable
        Dim ExistPendTrf As Boolean = False
        Try
            Cursor.Current = Cursors.WaitCursor

            btnSave.Enabled = False
            daDetails.Update(dsDetails)
            'If Date.Today < CDate("01/01/" & vbCyear) OrElse Date.Today > CDate("31/12/" & vbCyear) Then
            '    MsgBox("Transaction Date Must Be Between '01/01/" & vbCyear & "' and '31/12/" & vbCyear & "'.", MsgBoxStyle.Exclamation, Me.Text)
            '    Exit Sub
            'End If

            If CInt(txtItmNo.Text) = 0 Then

                MessageBox.Show("No Stock Items To Save", Me.Text, MessageBoxButtons.OK, MessageBoxIcon.Exclamation)
                Exit Sub
            End If

            If txtMchName1.Text.Trim = "" Then ErrProvider.SetError(txtMchName1, "Count Machine Or User Name 1 Could Not Be Empty")

            If txtMchName1.Text.Trim = "" Then
                MessageBox.Show("Missing count machine or username", Me.Text, MessageBoxButtons.OK, MessageBoxIcon.Exclamation)

                If txtMchName1.Text.Trim = "" Then
                    txtMchName1.Focus()
                End If

                Cursor.Current = Cursors.Default
                Exit Sub
            End If

            'Check if exist pending transfer for checked items

            If ClocHO = True Then
                HOLinkServer = "dbo."
            Else

                strPT = " SELECT Loc_Code, Loc_Desc, Loc_CoCode , Loc_ConnOfLoc, "
                strPT &= " CASE WHEN CHARINDEX('---', loc_sqlserver, 1)<>0 THEN  "
                strPT &= " SUBSTRING(loc_sqlserver, CHARINDEX('---', loc_sqlserver, 1) + 3, LEN(loc_sqlserver))"
                strPT &= " ELSE substring(loc_sqlserver,charindex('--',loc_sqlserver,1)+2,len(loc_sqlserver)) END "
                strPT &= " loc_sqlserver , "
                strPT &= " loc_db , loc_password , loc_type3 FROM STK04_LOCATION WHERE "
                strPT &= " Loc_Code = '00'"

                dtPT = ReadFromDBToDataTable(strPT)

                For Each rw As DataRow In dtPT.Select()

                    If Not LinkServer(rw.Item("loc_sqlserver").ToString.Trim, "sa", Decpassword(rw.Item("loc_password").ToString).Trim, rw.Item("loc_db").ToString.Trim, "N", 0, "1", HOLinkServer) Then
                        Exit Try
                    End If

                Next

            End If

            Dim tblPendingTrf As String = "tmp_InventoryPendingTRF_" & vbUser
            DropSqlObject(tblPendingTrf, "table")

                strPT = "SELECT distinct TTransD_RollDet StockRecordID INTO " & tblPendingTrf
                strPT &= " FROM   " & HOLinkServer & "pos_TransferDetailsHOtmp pos_TransferDetailsHOtmp"
                strPT &= " INNER JOIN ( SELECT TTransH_recid "
                strPT &= " FROM   " & HOLinkServer & "pos_TransferHeaderHOtmp"
                strPT &= " WHERE  TTransH_LocFRom = '" & vbCLoc & "'"
                strPT &= " OR TTransH_LocTo = '" & vbCLoc & "'"
                strPT &= " ) pos_TransferHeaderHOtmp ON pos_TransferHeaderHOtmp.TTransH_recid = pos_TransferDetailsHOtmp.TTransD_Hrecid"

                Using FTTcnsql As New SqlConnection(CONNECTION_STRING)
                    FTTcnsql.Open()

                    Using FTTcmsql As New SqlCommand(strPT, FTTcnsql)
                        FTTcmsql.CommandTimeout = 900
                        FTTcmsql.ExecuteNonQuery()
                    End Using

                    FTTcnsql.Close()
                End Using

                strPT = " SELECT  TOP 1 1 "
                strPT &= " FROM " & tbPhyCntView
                strPT &= " WHERE  Chck = 'True'"
                strPT &= " AND EXISTS ( SELECT TOP 1 1 FROM " & tblPendingTrf
                strPT &= " Where Stkid = StockRecordID )"

                Using Tcnsql As New SqlConnection(CONNECTION_STRING)
                    Tcnsql.Open()

                    Using Tcmsql As New SqlCommand(strPT, Tcnsql)
                        Tcmsql.CommandTimeout = 900
                    If Not Tcmsql Is Nothing Then
                        ExistPendTrf = True
                    Else
                        ExistPendTrf = False
                        End If
                    End Using

                    Tcnsql.Close()
                End Using

                DropSqlObject(tblPendingTrf, "table")

                If ExistPendTrf Then
                    If MessageBox.Show("Exist Items Having Pending Transfers, Do You Want to View Items List ?", Me.Text,
                                       MessageBoxButtons.YesNo, MessageBoxIcon.Question) = Windows.Forms.DialogResult.Yes Then

                        strPT = " SELECT  TTransH_TransNo ,"
                        strPT &= " FromLoc , "
                        strPT &= " ToLoc , "
                        strPT &= " Reference ,"
                        If ChkBarcode.Checked Then
                            strPT = strPT & " Barcode ,"
                        End If
                        strPT &= " FDesc [Description] ,"
                        strPT &= " Color_Reference ,"
                        strPT &= " TTransD_Grid ,"
                        strPT &= " TTransD_DetGrid ,"
                        strPT &= " TTransD_ExpiryDt ,"
                        strPT &= " TTransD_BatchInfo ,"
                        strPT &= " TTransD_QtyTrans ,"
                        strPT &= " Meas_Desc "
                        strPT &= " FROM " & tbPhyCntView
                        strPT &= " INNER JOIN ( SELECT TTransD_RollDet ,"
                        strPT &= "     TTransD_QtyTrans ,"
                        strPT &= "     TTransH_TransNo ,"
                        strPT &= "     TTransD_Color , "
                        strPT &= "     Color_Reference, "
                        strPT &= "     TTransD_Grid ,"
                        strPT &= "     TTransD_DetGrid ,"
                        strPT &= "     TTransD_ExpiryDt ,"
                        strPT &= "     TTransD_BatchInfo ,"
                        strPT &= "     LocFrom.Loc_Desc FromLoc ,"
                        strPT &= "     LocTo.Loc_Desc ToLoc"
                        strPT &= " FROM   " & HOLinkServer & "pos_TransferDetailsHOtmp pos_TransferDetailsHOtmp"
                        strPT &= "     INNER JOIN (SELECT TTransH_recid , TTransH_TransNo , TTransH_IssLoc , TTransH_LocFRom , TTransH_LocTo FROM " & HOLinkServer & "pos_TransferHeaderHOtmp ) "
                        strPT &= "     pos_TransferHeaderHOtmp ON pos_TransferHeaderHOtmp.TTransH_recid = pos_TransferDetailsHOtmp.TTransD_Hrecid"
                        strPT &= " 	   Inner JOIN (SELECT Loc_Code , Loc_Desc FROM  " & HOLinkServer & "STK04_Location) LocFrom ON pos_TransferHeaderHOtmp.TTransH_LocFRom = LocFrom.Loc_Code"
                        strPT &= " 	   Inner JOIN (SELECT Loc_Code , Loc_Desc FROM  " & HOLinkServer & "STK04_Location) LocTo ON pos_TransferHeaderHOtmp.TTransH_LocTo = LocTo.Loc_Code"
                        strPT &= "     LEFT JOIN " & HOLinkServer & "Stk04_COLOR Stk04_COLOR ON Stk04_COLOR.Color_Code = pos_TransferDetailsHOtmp.TTransD_Color "
                        strPT &= " WHERE  TTransH_LocFRom = '" & vbCLoc & "' or  TTransH_LocTo ='" & vbCLoc & "'"
                        strPT &= " ) pos_TransferDetailsHOtmp ON TTransD_RollDet = Stkid"
                        strPT &= " INNER JOIN ( SELECT Meas_Unit, Meas_Desc FROM STK04_Measurements ) Meas ON UnitCode = Meas.Meas_Unit "
                        strPT &= " WHERE   Chck = 'True'"

                        Using frm As New frmPreviewGrid
                            With frm
                                frm.BuildSql(strPT, , , , , , , , 900)

                                If Not .Norecords Then

                                    .ColDesign("TTransH_TransNo", -1, "Transfer Number", AlignHorzEnum.Center, AlignVertEnum.Top, , , , , True, , frmPreviewGrid.Orders.Ascending)
                                    .ColDesign("FromLoc", -1, "From", C1.Win.C1TrueDBGrid.AlignHorzEnum.Near)
                                    .ColDesign("ToLoc", -1, "To", C1.Win.C1TrueDBGrid.AlignHorzEnum.Near)
                                    .ColDesign("Reference", -1, "Reference", , , , , , , , , )
                                    .ColDesign("Description", -1, "Description", C1.Win.C1TrueDBGrid.AlignHorzEnum.Near)
                                    .ColDesign("Color_Reference", -1, "Color", C1.Win.C1TrueDBGrid.AlignHorzEnum.Near, , gbFashion, , , , , gbFashion)
                                    .ColDesign("TTransD_Grid", -1, "Grid", C1.Win.C1TrueDBGrid.AlignHorzEnum.Near, , gbFashion, , , , , gbFashion)
                                    .ColDesign("TTransD_DetGrid", -1, "Size", C1.Win.C1TrueDBGrid.AlignHorzEnum.Near, , gbFashion, , , , , gbFashion)
                                    .ColDesign("TTransD_ExpiryDt", -1, "Expiry Date", C1.Win.C1TrueDBGrid.AlignHorzEnum.Near, , gbExpiry, , , , , gbExpiry)
                                    .ColDesign("TTransD_BatchInfo", -1, "Lot #", C1.Win.C1TrueDBGrid.AlignHorzEnum.Near, , gbExpiry, , , , , gbExpiry)
                                    .ColDesign("TTransD_QtyTrans", -1, "Qty", C1.Win.C1TrueDBGrid.AlignHorzEnum.Center, , , , , , , , , , , , "N3")
                                    .ColDesign("Meas_Desc", -1, "Unit", C1.Win.C1TrueDBGrid.AlignHorzEnum.Center)

                                    ShowNotification(Me, "Exist Items Having Pending Transfers", 3000, 12, DevComponents.DotNetBar.eToastPosition.TopCenter)

                                    .Lbl.Text = "List Of Items Having Pending Transfers as at " & CDate(WorkingDay).ToString("dd/MM/yyyy")
                                    .Lbl.Visible = True

                                    .ShowDialog(Nothing, Me.Name, "", , , , , , , , , , , False)

                                    If Not ClocHO Then
                                        'droplinkedsrvlogin(HOLinkServer)
                                        'DropLinkServer(HOLinkServer)
                                    End If

                                    Exit Try

                                End If

                            End With
                        End Using

                    End If
                    Exit Try
                End If



                ' check If exist a difference between the reserved field on the level of the stock quantity table Between the Head Office And the Stand Alone database 
                If Not ClocHO Then
                'strsql = " SELECT  t.STQ_STKRECID,
                '                   reference ,
                '             FDesc [Description],
                '                   isnull(t.STQ_RESERVED,0) STQ_RESERVED
                '           FROM    dbo.POS_STKQTY t
                '                   INNER JOIN " & tbPhyCntView & " ON stkid = STQ_STKRECID
                '                                                 AND chck = 'true'
                '                   LEFT JOIN ( SELECT  STQ_RESERVED ,
                '                                       STQ_STKRECID ,
                '                                       STQ_LOCCODE
                '                               FROM    " & HOLinkServer & "stk04_stkqty
                '                                       INNER JOIN " & tbPhyCntView & " ON stkid = STQ_STKRECID
                '                                                                     AND chck = 'true'
                '                               WHERE   STQ_LOCCODE = '" & vbCLoc & "'
                '                             ) stkqty ON stkqty.STQ_STKRECID = t.STQ_STKRECID
                '                                         AND stkqty.STQ_LOCCODE = t.STQ_LOCCODE
                '           WHERE   t.STQ_LOCCODE = '" & vbCLoc & "'
                '                   AND t.STQ_RESERVED <> stkqty.STQ_RESERVED "

                strsql = "  SELECT  t.STQ_STKRECID ,
                                    reference ,
                                    FDesc AS [Description] ,
                                    t.STQ_RESERVED
                            FROM    dbo.POS_STKQTY t
                                    INNER JOIN " & tbPhyCntView & " ON stkid = STQ_STKRECID
                                                                  AND chck = 'true'
                            WHERE   t.STQ_LOCCODE = '" & vbCLoc & "'
                                    AND EXISTS ( SELECT 1
                                                 FROM   " & HOLinkServer & "stk04_stkqty
                                                 WHERE  STQ_STKRECID = t.STQ_STKRECID
                                                        AND STQ_LOCCODE = t.STQ_LOCCODE
                                                        AND ISNULL(STQ_RESERVED, 0) <> ISNULL(t.STQ_RESERVED, 0) )"

                Using frm As New frmPreviewGrid
                    With frm
                        frm.BuildSql(strsql, , , , , ,, , 900)

                        If Not .Norecords Then

                            .ColDesign("STQ_STKRECID", 0, "", AlignHorzEnum.Center, AlignVertEnum.Top, False, , , , True, False, frmPreviewGrid.Orders.Ascending)
                            .ColDesign("Reference", -1, "Reference", , , , , , , , , )
                            .ColDesign("Description", -1, "Description", C1.Win.C1TrueDBGrid.AlignHorzEnum.Near)
                            .ColDesign("STQ_RESERVED", -1, "Reserved Qty", C1.Win.C1TrueDBGrid.AlignHorzEnum.Near)

                            .Lbl.Text = "List Of Items Having A Difference In Reserved Quantity."
                            .Lbl.Visible = True

                            .ShowDialog(Nothing, Me.Name, "", , , , , , , , , , , False)

                            Exit Try

                        End If

                    End With
                End Using
            End If

            'If Not ClocHO Then
            '    If DataNotSync(tbPhyCntView, HOLinkServer) = True Then Exit Try
            'End If

            'Check if exist pending Data In HO sync


            If Not ClocHO Then
                HOLinkServer = ""
                strPT = " SELECT Loc_Code, Loc_Desc, Loc_CoCode , Loc_ConnOfLoc, "
                strPT &= " CASE WHEN CHARINDEX('---', Loc_sqlserverSync, 1)<>0 THEN  "
                strPT &= " SUBSTRING(Loc_sqlserverSync, CHARINDEX('---', Loc_sqlserverSync, 1) + 3, LEN(Loc_sqlserverSync))"
                strPT &= " ELSE substring(Loc_sqlserverSync,charindex('--',Loc_sqlserverSync,1)+2,len(Loc_sqlserverSync)) END "
                strPT &= " Loc_sqlserverSync , "
                strPT &= " Loc_dbSync , Loc_passwordSync , loc_type3 FROM STK04_LOCATION WHERE "
                strPT &= " Loc_Code = '00'"

                dtPT = ReadFromDBToDataTable(strPT)

                For Each rw As DataRow In dtPT.Select()

                    If Not LinkServer(rw.Item("Loc_sqlserverSync").ToString.Trim, "sa", Decpassword(rw.Item("Loc_passwordSync").ToString).Trim, rw.Item("Loc_dbSync").ToString.Trim, "N", 0, "1", HOLinkServer) Then
                        Exit Try
                    End If
                Next

                strsql = " SELECT DISTINCT Reference,"
                If ChkBarcode.Checked Then strsql &= " Barcode,"
                strsql &= " FDesc  FROM " & tbPhyCntView & ""
                strsql &= " INNER JOIN " & HOLinkServer & "stk04_stkqty ON Stkid=STQ_STKRECID AND STQ_LOCCODE='" & vbCLoc & "'"
                strsql &= " WHERE   Chck = 'True'"

                Using frm As New frmPreviewGrid
                    With frm
                        frm.BuildSql(strsql, , , , , , , , 900)

                        If Not .Norecords Then

                            .ColDesign("Reference", -1, "Reference", AlignHorzEnum.Center, AlignVertEnum.Top, , , , , True, , frmPreviewGrid.Orders.Ascending)
                            If ChkBarcode.Checked Then .ColDesign("Barcode", -1, "Barcode", C1.Win.C1TrueDBGrid.AlignHorzEnum.Center)
                            .ColDesign("FDesc", -1, "Item Desc", C1.Win.C1TrueDBGrid.AlignHorzEnum.Near)

                            ShowNotification(Me, "Exist Items Having Pending Quantity in HO Sync", 3000, 12, DevComponents.DotNetBar.eToastPosition.TopCenter)
                            .Lbl.Text = "List Of Items Having Pending Quantity in HO Sync " & CDate(WorkingDay).ToString("dd/MM/yyyy")
                            .Lbl.Visible = True
                            .ShowDialog(Nothing, Me.Name, "", , , , , , , , , , , False)



                            Exit Try

                        End If

                    End With
                End Using

            End If
            If ClocHO = True Then
                HOLinkServer = "dbo."
            Else

                strPT = " SELECT Loc_Code, Loc_Desc, Loc_CoCode , Loc_ConnOfLoc, "
                strPT &= " CASE WHEN CHARINDEX('---', loc_sqlserver, 1)<>0 THEN  "
                strPT &= " SUBSTRING(loc_sqlserver, CHARINDEX('---', loc_sqlserver, 1) + 3, LEN(loc_sqlserver))"
                strPT &= " ELSE substring(loc_sqlserver,charindex('--',loc_sqlserver,1)+2,len(loc_sqlserver)) END "
                strPT &= " loc_sqlserver , "
                strPT &= " loc_db , loc_password , loc_type3 FROM STK04_LOCATION WHERE "
                strPT &= " Loc_Code = '00'"

                dtPT = ReadFromDBToDataTable(strPT)

                For Each rw As DataRow In dtPT.Select()

                    If Not LinkServer(rw.Item("loc_sqlserver").ToString.Trim, "sa", Decpassword(rw.Item("loc_password").ToString).Trim, rw.Item("loc_db").ToString.Trim, "N", 0, "1", HOLinkServer) Then
                        Exit Try
                    End If

                Next

            End If
            cnsql = New SqlConnection(CONNECTION_STRING)
            cnsql.Open()

            trnsql = cnsql.BeginTransaction

            ' tdbDetails.UpdateData()
            '    daDetails.Update(dsDetails)

            cmsql = New SqlCommand
            cmsql.Connection = cnsql
            cmsql.Transaction = trnsql

            'Get Last+1
            strsql = "select isnull(max(pch_recid),0)+1 from tbl_PhysicalCountHeader where Pch_Loc ='" & vbCLoc & "'"
            cmsql.CommandText = strsql
            drSql = cmsql.ExecuteReader
            If drSql.Read Then
                cnt = CType(drSql.Item(0).ToString, Int32)

            End If
            MchUsr.Clear()

            If txtMchName1.Text.StartsWith("U") Then MchUsr.Add(txtMchName1.Text)
            If txtMchName2.Text.Trim <> "" AndAlso txtMchName2.Text.StartsWith("U") Then
                MchUsr.Add(txtMchName2.Text)
            End If


            If txtMchName3.Text.Trim <> "" AndAlso txtMchName3.Text.StartsWith("U") Then
                MchUsr.Add(txtMchName3.Text)
            End If
            If txtMchName4.Text.Trim <> "" AndAlso txtMchName4.Text.StartsWith("U") Then
                MchUsr.Add(txtMchName4.Text)
            End If
            If txtMchName5.Text.Trim <> "" AndAlso txtMchName5.Text.StartsWith("U") Then
                MchUsr.Add(txtMchName5.Text)
            End If
            If txtMchName6.Text.Trim <> "" AndAlso txtMchName6.Text.StartsWith("U") Then
                MchUsr.Add(txtMchName6.Text)
            End If
            drSql.Close()


            strsql = " UPDATE  " & tbPhyCnt
            strsql &= " SET     " & tbPhyCnt & ".Chck = " & tbPhyCntView & ".Chck"
            strsql &= " FROM    " & tbPhyCntView
            strsql &= " WHERE   " & tbPhyCntView & ".Reference = " & tbPhyCnt & ".Reference"

            cmsql.CommandText = strsql
            cmsql.ExecuteNonQuery()


            cmsql.Dispose()
            drSql = Nothing
            cmsql = Nothing

            cmsql = New SqlCommand
            cmsql.Connection = cnsql
            cmsql.Transaction = trnsql

            'Insert Into Physical Count Header Table
            strsql = "Insert into tbl_PhysicalCountHeader(pch_recid,pch_loc,pch_whouse,pch_datecount ,Pch_MachUser1 , Pch_MachUser2, Pch_MachUser3, Pch_MachUser4, "


            strsql = strsql & "pch_memo1,pch_memo2,pch_nbritems,pch_status,pch_updated ,Pch_MachUser5,Pch_MachUser6)"
            strsql = strsql & " values( " & cnt & ",'" & vbCLoc.ToString & "',"
            strsql &= "'1',"
            strsql = strsql & "'" & Format(CDate(txtDatePhyCnt.Text.Trim), "MM/dd/yyyy") & "',"
            strsql = strsql & "'" & txtMchName1.Text.Trim.Replace("'", "''") & "',"
            If txtMchName2.Text.Trim <> "" Then
                strsql = strsql & "'" & txtMchName2.Text.Trim.Replace("'", "''") & "',"
            Else
                strsql &= "NULL , "
            End If

            If txtMchName3.Text.Trim <> "" Then
                strsql = strsql & "'" & txtMchName3.Text.Trim.Replace("'", "''") & "',"
            Else
                strsql &= "NULL , "
            End If
            If txtMchName4.Text.Trim <> "" Then
                strsql = strsql & "'" & txtMchName4.Text.Trim.Replace("'", "''") & "',"
            Else
                strsql &= "NULL , "
            End If
            strsql = strsql & "N'" & TxtMemo1.Text.Trim.Replace("'", "''") & "',"
            strsql = strsql & "N'" & txtMemo2.Text.Trim.Replace("'", "''") & "',"
            strsql = strsql & txtItmNo.Text.Trim & ","
            strsql = strsql & "'N','N' ,"
            'ch @ 22/03/2019 mail (Maliks_ERP_Physical Count_Add 5&6th machine)
            If txtMchName5.Text.Trim <> "" Then
                strsql = strsql & "'" & txtMchName5.Text.Trim.Replace("'", "''") & "',"
            Else
                strsql &= "NULL , "
            End If
            If txtMchName6.Text.Trim <> "" Then
                strsql = strsql & "'" & txtMchName6.Text.Trim.Replace("'", "''") & "'"
            Else
                strsql &= "NULL  "
            End If
            strsql &= " )"

            cmsql.CommandText = strsql
            cmsql.ExecuteNonQuery()

            'Insert Into Physical Count Detail Table
            'strsql = "insert into tbl_PhysicalCountDetail(pcd_recid,pcd_loc,pcd_whouse,"
            'strsql &= "pcd_stkid,pcd_barcode1,pcd_barcode2,pcd_datecount,pcd_qty)"
            'strsql = strsql & " select " & cnt & ",'" & vbCLoc.ToString & "','1',"
            'strsql &= "StkId,barcode1,barcode2,'" & Format(CDate(txtDatePhyCnt.Text.Trim), "MM/dd/yyyy") & "',"
            'strsql = strsql & "avlbqty from " & tbPhyCnt & " where Chck='True'"

            strsql = " INSERT INTO dbo.tbl_PhysicalCountDetail"
            strsql &= " select   ROW_NUMBER() OVER(ORDER BY cntr) + (SELECT ISNULL(MAX(pcd_recid),0) FROM tbl_PhysicalCountDetail where Pcd_Loc = '" & vbCLoc & "') ," & cnt & ",'" & vbCLoc.ToString & "','1', StkId,Colorcode,gridcode,Size"
            strsql &= " , RIGHT('00.' + [exp], 10),lot#,'" & Format(CDate(txtDatePhyCnt.Text.Trim), "MM/dd/yyyy") & "', Sum (isnull(qty2.stq_available,0) -isnull(qty2.STQ_RESERVED,0) +isnull(resQty,0))  "
            strsql &= " OVER (PARTITION BY      cntr , StkId , Colorcode , gridcode ,Size ,RIGHT('00.' + [exp], 10) , lot#    ORDER BY       StkId   ) ,"
            strsql &= " NULL ,"
            strsql &= " NULL ,"
            strsql &= " NULL ,"
            strsql &= " NULL ,"
            strsql &= " NULL ,"
            strsql &= " NULL ,"
            strsql &= " NULL ,"
            strsql &= " NULL ,"
            strsql &= " NULL ,"
            strsql &= " NULL"
            strsql &= " ,NULL , NULL, NULL , NULL " 'ch @ 22/03/2019
            strsql &= " from     " & tbPhyCnt & "     "
            strsql &= " LEFT JOIN " & IIf(ClocHO, "Stk04_StkQty", "pos_stkqty") & "  qty2 ON qty2.STQ_STKRECID = Stkid AND qty2.stq_loccode ='" & vbCLoc & "'"
            strsql &= " AND ISNULL([lot#],'') = ISNULL(qty2.STQ_BatchInfo,'')"
            strsql &= " AND ISNULL(case when len([exp])=7 then '00.' + exp else exp end,'') = ISNULL(qty2.STQ_ExpiryDt,'')"
            strsql &= " AND ISNULL(qty2.STQ_Color,'') = ISNULL(Colorcode,'')"
            strsql &= " AND ISNULL(qty2.STQ_DetGrid,'') = ISNULL(Size,'')"
            strsql &= " AND ISNULL(qty2.STQ_Grid,'') =ISNULL([gridcode],'')"
            strsql &= " LEFT JOIN (  SELECT  ROD_StkRecid , isnull(SUM(ROD_ResQty * CASE WHEN ROH_Status = 'N' THEN 1 ELSE -1 END),0) resQty,"
            strsql &= " ROD_Color ,  ROD_Grid ,  ROD_DetGrid ,  ROD_ExpDate , ROD_Batch"
            strsql &= " FROM    dbo.stk04_ResOrdH"
            strsql &= " INNER JOIN dbo.STK04_ResOrdD ON STK04_ResOrdD.ROD_Loc = stk04_ResOrdH.ROH_Loc"
            strsql &= " AND STK04_ResOrdD.ROD_Hrecid = stk04_ResOrdH.ROH_Recid where ROH_Loc = '" & vbCLoc & "'"
            strsql &= " GROUP BY ROD_StkRecid ,  ROD_Color ,  ROD_Grid ,  ROD_DetGrid ,  ROD_ExpDate ,  ROD_Batch"
            strsql &= " ) tableResQty ON Stkid = ROD_StkRecid"
            strsql &= " AND COALESCE(ROD_Grid, '') = COALESCE(gridcode, '')"
            strsql &= " AND COALESCE(ROD_DetGrid, '') = COALESCE(Size,  '')"
            strsql &= " AND COALESCE(ROD_ExpDate, '') = COALESCE(case when len([exp])=7 then '00.' + exp else exp end,  '')"
            strsql &= " AND COALESCE(ROD_Batch, '') = COALESCE([lot#],  '')"
            strsql &= " AND COALESCE(ROD_Color, '') = COALESCE(Colorcode,  '')"
            strsql &= " where Chck='True'"


            cmsql.CommandText = strsql
            cmsql.ExecuteNonQuery()

            strsql = " INSERT INTO dbo.tbl_PhysicalCountDetBarcode"
            strsql &= " SELECT  ROW_NUMBER() OVER(ORDER BY dbar_recid) + (SELECT ISNULL(MAX(PcBarc_RecId),0) FROM tbl_PhysicalCountDetBarcode where PcBarc_LOC = '" & vbCLoc & "')  ,"
            strsql &= " Pcd_RecId ,"
            strsql &= " dbar_barcode,'" & vbCLoc & "'"
            strsql &= " FROM dbo.tbl_PhysicalCountDetail"
            strsql &= " INNER JOIN dbo.Stk04_DetBarcode ON Pcd_stkid =dbar_stkid"

            strsql &= " AND isnull(dbar_expdate,'') = isnull(Pcd_ExpDate,'')"

            'strsql &= " AND CASE WHEN SUBSTRING(dbar_expdate, 0, 3) = '00'"
            'strsql &= " THEN SUBSTRING(dbar_expdate, 4, 8)"
            'strsql &= " ELSE isnull(dbar_expdate,'') end ="

            'strsql &= " /*AND isnull(dbar_expdate,'')=*/  isnull(Pcd_ExpDate,'')"
            strsql &= " AND isnull(dbar_batch,'') = isnull(Pcd_Batch,'')"
            strsql &= " AND isnull(dbar_grid,'') =ISNULL(Pcd_Grid,'')"
            strsql &= " AND ISNULL(dbar_Detgrid,'') = ISNULL(Pcd_DetGrid,'')"
            strsql &= " AND ISNULL(dbar_color,'') = ISNULL(Pcd_Color,'')"


            strsql &= " WHERE Pcd_HRecId = '" & cnt & "' AND Pcd_Loc='" & vbCLoc & "'"




            cmsql.CommandText = strsql
            cmsql.ExecuteNonQuery()

            cmsql.Dispose()
            cmsql = Nothing

            trnsql.Commit()


            'Dim frm As New frmMessage
            'Dim frmFont As New Font("Arial", 40, FontStyle.Bold)
            'frm.txtMessage.Font = frmFont
            'frm.txtMessage.TextAlign = HorizontalAlignment.Center

            'frm.ShowDialog("Records Updated Successfully In Physical Count ID : ", vbCrLf & vbCrLf & cnt.ToString, Me)
            'frm = Nothing
            '    MessageBox.Show("Records Updated Successfully In Physical Count ID : " & cnt.ToString, Me.Text, MessageBoxButtons.OK, MessageBoxIcon.Information)

            If Mid(txtMchName1.Text.Trim, 1, 1) = "U" OrElse Mid(txtMchName2.Text.Trim, 1, 1) = "U" _
                 OrElse Mid(txtMchName3.Text.Trim, 1, 1) = "U" OrElse Mid(txtMchName4.Text.Trim, 1, 1) = "U" OrElse Mid(txtMchName5.Text.Trim, 1, 1) = "U" OrElse Mid(txtMchName6.Text.Trim, 1, 1) = "U" Then
                Dim frm As frmSpecialMsgbox
                frm = New frmSpecialMsgbox
                With frm
                    frm.Chk.Visible = ShowAvailableQuantity
                    frm.Chk.Checked = ShowAvailableQuantity
                    If .ShowDialog(Me.Text, "Records Updated Successfully In Physical Count ID : " & cnt.ToString & vbCrLf & " Do You Want to Print the following Physical Count ?", "Print  Available Qty", 144, frmSpecialMsgbox.ICons.Question) = Windows.Forms.DialogResult.Yes Then
                        PreviewProcedure(cnt, .Chk.Checked)
                    End If
                End With
            Else
                MessageBox.Show("Records Updated Successfully In Physical Count ID : " & cnt.ToString, Me.Text, MessageBoxButtons.OK, MessageBoxIcon.Information)
            End If
            lblPosition.Text = "0/0"
            dsDetails.Clear()
            btnClear_Click(btnClear, New System.EventArgs)
            'If MessageBox.Show("Do You Want to Print the following Physical Count ?", Me.Text, MessageBoxButtons.YesNo, MessageBoxIcon.Question) = Windows.Forms.DialogResult.Yes Then
            '    PreviewProcedure(cnt)
            'End If

Err:
        Catch ex As Exception
            Try
                trnsql.Rollback()
            Catch exfdsf As Exception

            End Try

            MsgBox(ex.Message, MsgBoxStyle.Critical, Me.Text & " : General Error")

            'btnClear_Click(btnClear, New System.EventArgs)
        Finally
            If Not cmsql Is Nothing Then
                cmsql.Dispose()
                cmsql = Nothing
            End If
            If Not trnsql Is Nothing Then
                trnsql.Dispose()
                trnsql = Nothing
            End If
            If Not cnsql Is Nothing Then
                If cnsql.State = ConnectionState.Open Then cnsql.Close()
                cnsql.Dispose()
                cnsql = Nothing
            End If
            If Not dtPT Is Nothing Then
                dtPT.Clear()
                dtPT.Dispose()
                dtPT = Nothing
            End If
            btnSave.Enabled = True
        End Try
    End Sub

    Private Sub rb_CheckedChanged(ByVal sender As Object, ByVal e As System.EventArgs) Handles rbDateRng.CheckedChanged, rbGen.CheckedChanged, rb1.CheckedChanged, rb2.CheckedChanged
        If Not dsDetails Is Nothing Then
            dsDetails.Clear()
            ButtonStatus(Me, False, False, "btnClose", "btnDetail1", "btnDetail2", "btnBookCat")
            btnShow.Enabled = fPrivList
        End If

        If CType(sender, RadioButton).Checked = True Then
            If CType(sender, RadioButton).Name = "rbgen" Then
                btnSearch.Visible = False
                txtStartDate.Visible = False
                txtEndDate.Visible = False
            ElseIf CType(sender, RadioButton).Name = "rbDateRng" Then
                btnSearch.Visible = True
                txtStartDate.Visible = True
                txtEndDate.Visible = True
                btnSearch.Enabled = True
            End If
        End If

        If CType(sender, RadioButton).Name = "rb1" OrElse CType(sender, RadioButton).Name = "rb2" Then
            btnSearch.Enabled = True
            If rb1.Checked = True Then
                If Not vbLoc1Desc Is Nothing AndAlso vbLoc1Desc.Trim <> "" Then

                    tdbDetails.Splits(0).DisplayColumns("AvlbQty").DataColumn.Caption = "Av. Qty-" & vbLoc1Desc
                Else
                    tdbDetails.Splits(0).DisplayColumns("AvlbQty").DataColumn.Caption = "Av. Qty"
                End If

            Else
                If Not vbLoc2Desc Is Nothing AndAlso vbLoc2Desc.Trim <> "" Then
                    tdbDetails.Splits(0).DisplayColumns("AvlbQty").DataColumn.Caption = "Av. Qty-" & vbLoc2Desc
                Else
                    tdbDetails.Splits(0).DisplayColumns("AvlbQty").DataColumn.Caption = "Av. Qty"
                End If

            End If
        End If

        txtItmNo.Text = "0"
        lblPosition.Text = "0/0"

    End Sub

    Private Sub txtMchName1_TextChanged(ByVal sender As Object, ByVal e As System.EventArgs) Handles txtMchName1.TextChanged
        If CType(sender, TextBox).Text.Trim <> "" Then
            txtMchName2.ReadOnly = False
        Else
            txtMchName2.Clear()
            txtMchName2.ReadOnly = True
        End If
        If ErrProvider.GetError(txtMchName1) <> "" Then ErrProvider.SetError(CType(sender, TextBox), "")
    End Sub

    Private Sub txtMchName2_TextChanged(ByVal sender As Object, ByVal e As System.EventArgs) Handles txtMchName2.TextChanged
        If CType(sender, TextBox).Text.Trim <> "" Then
            txtMchName3.ReadOnly = False
        Else
            txtMchName3.Clear()
            txtMchName3.ReadOnly = True
        End If
    End Sub

    Private Sub txtMchName3_TextChanged(ByVal sender As Object, ByVal e As System.EventArgs) Handles txtMchName3.TextChanged
        If CType(sender, TextBox).Text.Trim <> "" Then
            txtMchName4.ReadOnly = False
        Else
            txtMchName4.Clear()
            txtMchName4.ReadOnly = True
        End If
    End Sub
    Private Sub txtMchName4_TextChanged(ByVal sender As Object, ByVal e As System.EventArgs) Handles txtMchName4.TextChanged
        If CType(sender, TextBox).Text.Trim <> "" Then
            txtMchName5.ReadOnly = False
        Else
            txtMchName5.Clear()
            txtMchName5.ReadOnly = True
        End If
    End Sub
    Private Sub txtMchName5_TextChanged(ByVal sender As Object, ByVal e As System.EventArgs) Handles txtMchName5.TextChanged
        If CType(sender, TextBox).Text.Trim <> "" Then
            txtMchName6.ReadOnly = False
        Else
            txtMchName6.Clear()
            txtMchName6.ReadOnly = True
        End If
    End Sub

    Private Sub tdbDetails_AfterColEdit(ByVal sender As Object, ByVal e As C1.Win.C1TrueDBGrid.ColEventArgs) Handles tdbDetails.AfterColEdit
        If e.ColIndex = 1 Then
            If tdbDetails.Splits(0).DisplayColumns(1).DataColumn.Text = "False" Then
                txtItmNo.Text = (CInt(txtItmNo.Text.Trim) - 1).ToString
            ElseIf tdbDetails.Splits(0).DisplayColumns(1).DataColumn.Text = "True" Then
                txtItmNo.Text = (CInt(txtItmNo.Text.Trim) + 1).ToString
            End If
        End If
    End Sub

    Private Sub btnClear_Click(ByVal sender As System.Object, ByVal e As System.EventArgs) Handles btnClear.Click
        Dim dr As DataRow
        Try
            MemoPart1 = ""
            MemoPart2 = ""
            TxtMemo1.Text = "Count Date : " & txtDatePhyCnt.Text
            If Not dtBrand Is Nothing Then
                For Each dr In dtBrand.Select()
                    dr.Item("Chk") = "false"
                Next
            End If
            If Not dtcategory Is Nothing Then
                For Each dr In dtcategory.Select()
                    dr.Item("Chk") = "false"
                Next
            End If
            If Not dtproduct Is Nothing Then
                For Each dr In dtproduct.Select()
                    dr.Item("Chk") = "false"
                Next
            End If
            If Not dtIdentification Is Nothing Then
                For Each dr In dtIdentification.Select()
                    dr.Item("Chk") = "false"
                Next
            End If
            dtcategory = Nothing : dtBrand = Nothing : dtproduct = Nothing : dtIdentification = Nothing
            dsDetails.Clear()
            txtItmNo.Text = "0"
            lblPosition.Text = "0/0"
            txtBookCat.Text = ""

            chkBook.Checked = False

            btnCheck.Enabled = False
            btnUnCheck.Enabled = False
            btnClear.Enabled = True
            btnSave.Enabled = False
            btnImportTransfer.Enabled = False
            txtGeneralSearch.Text = ""
            ErrProvider.SetError(txtDatePhyCnt, "")
            ErrProvider.SetError(txtMchName1, "")
            txtMchName1.Text = ""
            txtMchName2.Text = ""
            txtMchName3.Text = ""
            txtMchName4.Text = ""
            txtMchName5.Text = ""
            txtMchName6.Text = ""
            txtMemo2.Text = ""
            ChkNoBooks.Checked = False
            chkBook.Checked = False
            txtDatePhyCnt.Text = WorkingDay
            FillGrid()
            rb1.Focus()
        Catch ex As Exception

        End Try


    End Sub

    Private Sub btnSearch_Click(ByVal sender As Object, ByVal e As System.EventArgs) Handles btnSearch.Click
        Dim strsql As String
        Dim cnSql As SqlConnection
        Dim dsSql As DataSet
        Dim daSql As SqlDataAdapter

        Dim frm As frmNewSearch

        Dim Sizefield() As Int16 = {70, 90, 90, -1, -1, -1, -1, -1, -1, 70, -1, 60, 60}
        Dim Alnfield() As String = {"C", "C", "C", "C", "C", "C", "C", "C", "C", "C", "C", "C", "C"}
        Dim Retfield() As String = {"Physical Date"}
        Try
            Cursor.Current = Cursors.WaitCursor

            strsql = "select pch_WHouse as [WareHouse],convert(varchar(10),pch_datecount,103) as [Physical Date],pch_Recid as [RecId],"
            strsql = strsql & "pch_machuser1 as [Machine/User 1],pch_machuser2 as [Machine/User 2],pch_machuser3 as [Machine/User 3],"
            strsql = strsql & "pch_machuser4 as [Machine/User 4],pch_machuser5 as [Machine/User 5],pch_machuser6 as [Machine/User 6],pch_nbritems as [#StkItem],pch_memo2 as [Memo2],"
            strsql = strsql & "case pch_status when 'N' then 'Normal' when 'C' then 'Cancelled' end as Status,"
            strsql = strsql & "case pch_updated when 'N' then 'No' when 'Y' then 'YES' end as Updated "
            strsql = strsql & "from tbl_PhysicalCountHeader where pch_loc='" & vbCLoc & "'"

            cnSql = New SqlConnection(CONNECTION_STRING)

            cnSql.Open()
            daSql = New SqlDataAdapter(strsql, cnSql)

            dsSql = New DataSet

            With daSql.TableMappings
                .Add("Table", "ParentTable")
            End With

            daSql.Fill(dsSql)

            cnSql.Close()

            frm = New frmNewSearch

            With frm
                .Owner = Me
                If .ShowDialog("Return", dsSql.Tables("ParentTable"), dsSql.Tables("ParentTable").Rows.Count,
                   "Physical Count", Sizefield, Alnfield, Retfield, 0, Nothing, False, -1, 500, , True,
                   , , , , , , , , , ) = DialogResult.OK Then
                    If gbRetSearch(0) <> "" Then

                        If txtStartDate.Value Is DBNull.Value Then
                            txtStartDate.Value = gbRetSearch(0)
                        ElseIf txtEndDate.Value Is DBNull.Value Then
                            txtEndDate.Value = gbRetSearch(0)
                        End If

                    End If


                End If
            End With

        Catch ex As Exception
            MessageBox.Show("Error : " & ex.Message, Me.Text, MessageBoxButtons.OK, MessageBoxIcon.Error)
        Finally
            gbRetSearch = Nothing
            dsSql = Nothing : daSql = Nothing
            If Not cnSql Is Nothing Then cnSql.Close() : cnSql.Dispose()
            frm = Nothing
            Cursor.Current = Cursors.Default
        End Try


    End Sub

    Private Sub txt_Enter(ByVal sender As System.Object, ByVal e As System.EventArgs) Handles txtMchName1.Enter, txtMchName2.Enter, txtMchName3.Enter, txtMchName4.Enter, txtMemo2.Enter, txtBookCat.Enter, txtMchName5.Enter, txtMchName6.Enter
        FocusStyle(CType(sender, TextBox), "Enter")
    End Sub

    Private Sub txt_leave(ByVal sender As System.Object, ByVal e As System.EventArgs) Handles txtMchName1.Leave, txtMchName2.Leave, txtMchName3.Leave, txtMchName4.Leave, txtMemo2.Leave, txtBookCat.Leave, txtMchName5.Leave, txtMchName6.Leave
        FocusStyle(CType(sender, TextBox), "leave")
    End Sub

    Private Sub txtMchName_Validating(ByVal sender As Object, ByVal e As System.ComponentModel.CancelEventArgs) Handles txtMchName1.Validating, txtMchName2.Validating, txtMchName3.Validating, txtMchName4.Validating, txtMchName5.Validating, txtMchName6.Validating
        If CType(sender, TextBox).Text = "" Then GoTo Case2
        If CType(sender, TextBox).Text.ToUpper.StartsWith("U") OrElse CType(sender, TextBox).Text.ToUpper.StartsWith("IT") Then

        Else
            e.Cancel = True
            MessageBox.Show("Count Machine Or User Name Must Starts With 'U' For User Or 'IT' For Machine ", Me.Text, MessageBoxButtons.OK, MessageBoxIcon.Information)
        End If
        If CType(sender, TextBox).Name = "txtMchName1" Then
            If txtMchName1.Text = txtMchName2.Text.Trim OrElse txtMchName1.Text = txtMchName3.Text.Trim OrElse
                txtMchName1.Text = txtMchName4.Text.Trim OrElse txtMchName1.Text = txtMchName5.Text.Trim OrElse txtMchName1.Text = txtMchName6.Text.Trim Then
                e.Cancel = True
                MessageBox.Show("Count Machine Or User Name Must Be Unique", Me.Text, MessageBoxButtons.OK, MessageBoxIcon.Information)

            End If
        ElseIf CType(sender, TextBox).Name = "txtMchName2" Then
            If txtMchName2.Text = txtMchName1.Text.Trim OrElse txtMchName2.Text = txtMchName3.Text.Trim OrElse
                txtMchName2.Text = txtMchName4.Text.Trim OrElse txtMchName2.Text = txtMchName5.Text.Trim OrElse txtMchName2.Text = txtMchName6.Text.Trim Then
                e.Cancel = True
                MessageBox.Show("Count Machine Or User Name Must Be Unique", Me.Text, MessageBoxButtons.OK, MessageBoxIcon.Information)
            End If
        ElseIf CType(sender, TextBox).Name = "txtMchName3" Then
            If txtMchName3.Text = txtMchName1.Text.Trim OrElse txtMchName3.Text = txtMchName2.Text.Trim OrElse
                txtMchName3.Text = txtMchName4.Text.Trim OrElse txtMchName3.Text = txtMchName5.Text.Trim OrElse txtMchName3.Text = txtMchName6.Text.Trim Then
                e.Cancel = True
                MessageBox.Show("Count Machine Or User Name Must Be Unique", Me.Text, MessageBoxButtons.OK, MessageBoxIcon.Information)
            End If
        ElseIf CType(sender, TextBox).Name = "txtMchName4" Then
            If txtMchName4.Text = txtMchName1.Text.Trim OrElse txtMchName4.Text = txtMchName2.Text.Trim OrElse
                txtMchName4.Text = txtMchName3.Text.Trim OrElse txtMchName4.Text = txtMchName5.Text.Trim OrElse txtMchName4.Text = txtMchName6.Text.Trim Then
                e.Cancel = True
                MessageBox.Show("Count Machine Or User Name Must Be Unique", Me.Text, MessageBoxButtons.OK, MessageBoxIcon.Information)
            End If
        ElseIf CType(sender, TextBox).Name = "txtMchName5" Then
            If txtMchName5.Text = txtMchName1.Text.Trim OrElse txtMchName5.Text = txtMchName2.Text.Trim OrElse
                txtMchName5.Text = txtMchName3.Text.Trim OrElse txtMchName5.Text = txtMchName4.Text.Trim OrElse txtMchName5.Text = txtMchName6.Text.Trim Then
                e.Cancel = True
                MessageBox.Show("Count Machine Or User Name Must Be Unique", Me.Text, MessageBoxButtons.OK, MessageBoxIcon.Information)
            End If
        ElseIf CType(sender, TextBox).Name = "txtMchName6" Then
            If txtMchName6.Text = txtMchName1.Text.Trim OrElse txtMchName6.Text = txtMchName2.Text.Trim OrElse
                txtMchName6.Text = txtMchName3.Text.Trim OrElse txtMchName6.Text = txtMchName4.Text.Trim OrElse txtMchName6.Text = txtMchName5.Text.Trim Then
                e.Cancel = True
                MessageBox.Show("Count Machine Or User Name Must Be Unique", Me.Text, MessageBoxButtons.OK, MessageBoxIcon.Information)
            End If
        End If

Case2:
        MemoPart2 = ""
        If txtMchName1.Text.Trim <> "" AndAlso txtMchName1.Text.StartsWith("U") Then
            MemoPart2 &= vbCrLf & "User Name : " & txtMchName1.Text.Trim
        ElseIf txtMchName1.Text.Trim = "i" Then
            e.Cancel = True
            MessageBox.Show("Invalid Name", Me.Text, MessageBoxButtons.OK, MessageBoxIcon.Information)
        ElseIf txtMchName1.Text.Trim <> "" AndAlso txtMchName1.Text.StartsWith("IT") Then
            MemoPart2 &= vbCrLf & "Machine Name : " & txtMchName1.Text.Trim
        End If
        If txtMchName2.Text.Trim <> "" AndAlso txtMchName2.Text.StartsWith("U") Then
            MemoPart2 &= vbCrLf & "User Name : " & txtMchName2.Text.Trim
        ElseIf txtMchName2.Text.Trim = "i" Then
            e.Cancel = True
            MessageBox.Show("Invalid Name", Me.Text, MessageBoxButtons.OK, MessageBoxIcon.Information)
        ElseIf txtMchName2.Text.Trim <> "" AndAlso txtMchName2.Text.StartsWith("IT") Then
            MemoPart2 &= vbCrLf & "Machine Name : " & txtMchName2.Text.Trim
        End If
        If txtMchName3.Text.Trim <> "" AndAlso txtMchName3.Text.StartsWith("U") Then
            MemoPart2 &= vbCrLf & "User Name : " & txtMchName3.Text.Trim
        ElseIf txtMchName3.Text.Trim = "i" Then
            e.Cancel = True
            MessageBox.Show("Invalid Name", Me.Text, MessageBoxButtons.OK, MessageBoxIcon.Information)
        ElseIf txtMchName3.Text.Trim <> "" AndAlso txtMchName3.Text.StartsWith("IT") Then
            MemoPart2 &= vbCrLf & "Machine Name : " & txtMchName3.Text.Trim
        End If
        If txtMchName4.Text.Trim <> "" AndAlso txtMchName4.Text.StartsWith("U") Then
            MemoPart2 &= vbCrLf & "User Name : " & txtMchName4.Text.Trim
        ElseIf txtMchName4.Text.Trim = "i" Then
            e.Cancel = True
            MessageBox.Show("Invalid Name", Me.Text, MessageBoxButtons.OK, MessageBoxIcon.Information)
        ElseIf txtMchName4.Text.Trim <> "" AndAlso txtMchName4.Text.StartsWith("IT") Then
            MemoPart2 &= vbCrLf & "Machine Name : " & txtMchName4.Text.Trim
        End If
        If txtMchName5.Text.Trim <> "" AndAlso txtMchName5.Text.StartsWith("U") Then
            MemoPart2 &= vbCrLf & "User Name : " & txtMchName5.Text.Trim
        ElseIf txtMchName5.Text.Trim = "i" Then
            e.Cancel = True
            MessageBox.Show("Invalid Name", Me.Text, MessageBoxButtons.OK, MessageBoxIcon.Information)
        ElseIf txtMchName5.Text.Trim <> "" AndAlso txtMchName5.Text.StartsWith("IT") Then
            MemoPart2 &= vbCrLf & "Machine Name : " & txtMchName5.Text.Trim
        End If
        If txtMchName6.Text.Trim <> "" AndAlso txtMchName6.Text.StartsWith("U") Then
            MemoPart2 &= vbCrLf & "User Name : " & txtMchName6.Text.Trim
        ElseIf txtMchName6.Text.Trim = "i" Then
            e.Cancel = True
            MessageBox.Show("Invalid Name", Me.Text, MessageBoxButtons.OK, MessageBoxIcon.Information)
        ElseIf txtMchName6.Text.Trim <> "" AndAlso txtMchName6.Text.StartsWith("IT") Then
            MemoPart2 &= vbCrLf & "Machine Name : " & txtMchName6.Text.Trim
        End If
        TxtMemo1.Text = "Count Date : " & txtDatePhyCnt.Text & MemoPart2 & MemoPart1
    End Sub

    Private Sub tdbDetails_AfterFilter(ByVal sender As Object, ByVal e As C1.Win.C1TrueDBGrid.FilterEventArgs) Handles tdbDetails.AfterFilter
        lblPosition.Text = tdbDetails.Row + 1 & "/" & tdbDetails.Splits(0).Rows.Count
        lblPosition.Refresh()
    End Sub

    Private Sub tdbDetails_Error(ByVal sender As Object, ByVal e As C1.Win.C1TrueDBGrid.ErrorEventArgs) Handles tdbDetails.Error
        If e.Exception.Message.IndexOf("The expression contains an invalid string constant:") >= 0 Then
            e.Handled = True
        End If
    End Sub

    Private Sub txtStartDate_Validating(ByVal sender As Object, ByVal e As System.ComponentModel.CancelEventArgs) Handles txtStartDate.Validating

        Try
            If txtStartDate.Text <> "__/__/____" Then
                If txtEndDate.Text <> "" Then
                    If CDate(txtStartDate.Text) > CDate(txtEndDate.Text) Then
                        MessageBox.Show("Start Date Must be Less Than End Date.", Me.Text, MessageBoxButtons.OK, MessageBoxIcon.Exclamation)
                        e.Cancel = True
                        txtStartDate.Focus()
                    End If
                End If
            Else
                txtEndDate.Clear()
            End If
        Catch ex As Exception
            MessageBox.Show(ex.Message, Me.Text, MessageBoxButtons.OK, MessageBoxIcon.Information)
        End Try

    End Sub

    Private Sub txtEndDate_Validating(ByVal sender As Object, ByVal e As System.ComponentModel.CancelEventArgs) Handles txtEndDate.Validating
        Try
            If txtEndDate.Text <> "__/__/____" Then
                If txtStartDate.Text <> "" Then
                    If CDate(txtStartDate.Text) > CDate(txtEndDate.Text) Then
                        MessageBox.Show("Start Date Must be Less Than End Date.", Me.Text, MessageBoxButtons.OK, MessageBoxIcon.Exclamation)
                        e.Cancel = True
                        txtEndDate.Focus()
                    End If
                End If
            End If
        Catch ex As Exception
            MessageBox.Show(ex.Message, Me.Text, MessageBoxButtons.OK, MessageBoxIcon.Information)
        End Try


    End Sub

    Private Sub txtStartDate_ValidationError(ByVal sender As Object, ByVal e As C1.Win.C1Input.ValidationErrorEventArgs) Handles txtStartDate.ValidationError
        With e.ErrorInfo
            .ErrorMessage = "Invalid Date."
            .ErrorMessageCaption = Me.Text
            .ErrorAction = C1.Win.C1Input.ErrorActionEnum.ResetValue
        End With
    End Sub

    Private Sub txtEndDate_ValidationError(ByVal sender As Object, ByVal e As C1.Win.C1Input.ValidationErrorEventArgs) Handles txtStartDate.ValidationError
        With e.ErrorInfo
            .ErrorMessage = "Invalid Date."
            .ErrorMessageCaption = Me.Text
            .ErrorAction = C1.Win.C1Input.ErrorActionEnum.ResetValue
        End With
    End Sub

    Private Sub btnBookCat_Click(ByVal sender As System.Object, ByVal e As System.EventArgs) Handles btnBookCat.Click
        Dim strsql As String
        Dim cnsql As SqlConnection
        Dim cmsql As SqlCommand
        Dim sTab As String
        Dim cbsql As SqlCommandBuilder


        Try
            sTab = "TmpBookCat_" + vbUser.ToString

            'If tdbBookCat.Visible = False Then
            cnsql = New SqlConnection(CONNECTION_STRING)
            cnsql.Open()

            strsql = "if exists (select * from dbo.sysobjects where id = object_id(N'[dbo].[" & sTab & "]') "
            strsql = strsql & "and OBJECTPROPERTY(id, N'IsUserTable') = 1) drop table [dbo].[" & sTab & "]  "
            cmsql = New SqlCommand(strsql, cnsql)
            cmsql.ExecuteNonQuery()

            strsql = "SELECT [Check] = 'FALSE', C_Code [Code], C_Desc [Description] "
            strsql &= " INTO " + sTab + " FROM BIBL1_CATEGORY "
            cmsql.CommandText = strsql
            cmsql.ExecuteNonQuery()

            If txtBookCat.Text <> "" Then
                strsql = "UPDATE " + sTab + " SET [Check] = 'True'"
                strsql &= " WHERE [Code] IN ('" & Strings.Left(txtBookCat.Text, Len(txtBookCat.Text) - 1).Replace(",", "','") & "')"

                cmsql.CommandText = strsql
                cmsql.ExecuteNonQuery()
            End If

            cmsql.Dispose()
            cmsql = Nothing

            strsql = "SELECT * FROM " + sTab

            daDet = New SqlDataAdapter(strsql, CONNECTION_STRING)
            daDet.TableMappings.Add("Table", "Details")
            dsDet = New DataSet

            With daDet
                .FillSchema(dsDet, SchemaType.Mapped)
                .Fill(dsDet, "Details")
            End With

            cbsql = New SqlCommandBuilder(daDet)

            'daDet.UpdateCommand = cbsql.GetUpdateCommand

            cbsql = Nothing

            With tdbBookCat

                If Not .DataSource Is Nothing Then .DataSource = Nothing
                .Left = txtBookCat.Left + GroupBox3.Left + txtBookCat.Width - .Width
                .DataSource = Nothing
                .DataSource = dsDet
                .DataMember = "Details"

                With .Splits(0).DisplayColumns("Check")
                    .DataColumn.Caption = "Check"
                    .DataColumn.ValueItems.Presentation = PresentationEnum.CheckBox
                    .Width = 25
                    .Style.HorizontalAlignment = AlignHorzEnum.Center
                    .HeadingStyle.HorizontalAlignment = AlignHorzEnum.Center
                End With

                With .Splits(0).DisplayColumns("Code")
                    .DataColumn.Caption = "Code"
                    .Width = 40
                    .Style.HorizontalAlignment = AlignHorzEnum.Center
                    .HeadingStyle.HorizontalAlignment = AlignHorzEnum.Center
                    .Locked = True
                End With

                With .Splits(0).DisplayColumns("Description")
                    .Width = 150
                    .Style.HorizontalAlignment = AlignHorzEnum.General
                    .HeadingStyle.HorizontalAlignment = AlignHorzEnum.Center
                    .Locked = True
                End With

                .Visible = True


                .BackColor = SystemColors.Window
                .BringToFront()

                .Focus()
            End With

            'Else
            'tdbBookCat.Update()
            'tdbBookCat.Visible = False
            'End If

        Catch exc As Exception
            MsgBox(exc.Message, MsgBoxStyle.Critical.OkOnly, Me.Text)

        Finally
            If Not cmsql Is Nothing Then
                cmsql.Dispose()
                cmsql = Nothing
            End If

            If Not cnsql Is Nothing Then
                If cnsql.State = ConnectionState.Open Then cnsql.Close()
                cnsql.Dispose()
                cnsql = Nothing
            End If

        End Try
    End Sub

    Private Sub tdbBookCat_AfterColUpdate(ByVal sender As Object, ByVal e As C1.Win.C1TrueDBGrid.ColEventArgs) Handles tdbBookCat.AfterColUpdate

        Try

            If tdbBookCat.Columns(0).Text = "True" Then
                txtBookCat.Text &= tdbBookCat.Columns(1).Text & ","
            Else
                txtBookCat.Text = txtBookCat.Text.Replace(tdbBookCat.Columns(1).Text & ",", "")
            End If
        Catch ex As Exception
            MessageBox.Show(ex.Message, Me.Text, MessageBoxButtons.OK, MessageBoxIcon.Error)
        End Try

    End Sub

    Private Sub tdbBookCat_BeforeColUpdate(sender As Object, e As C1.Win.C1TrueDBGrid.BeforeColUpdateEventArgs) Handles tdbBookCat.BeforeColUpdate
        Try
            If tdbBookCat.FilterActive Then Exit Try
            If tdbBookCat.Columns(0).Text = "True" Then
                tdbBookCat.Columns(0).Text = "False"
            Else
                tdbBookCat.Columns(0).Text = "True"
            End If
        Catch ex As Exception
            MessageBox.Show(ex.Message, Me.Text, MessageBoxButtons.OK, MessageBoxIcon.Error)
        End Try

    End Sub

    Private Sub tdbBookCat_HeadClick(sender As Object, e As C1.Win.C1TrueDBGrid.ColEventArgs) Handles tdbBookCat.HeadClick

        Try
            If UCase(e.Column.DataColumn.DataField) = "Check" Then
                Button1_Click(chekall, New System.EventArgs)

            End If
        Catch ex As Exception

        End Try

    End Sub



    Private Sub tdbBookCat_KeyPress(sender As Object, e As System.Windows.Forms.KeyPressEventArgs) Handles tdbBookCat.KeyPress
        Try

            Dim col As String
            Dim txt As New TextBox

            Try
                col = tdbBookCat.Splits(0).DisplayColumns(tdbBookCat.Col).DataColumn.DataField

                txt.Text = tdbBookCat.Columns(col).Text
                Select Case col
                    Case "Check"
                        If (Microsoft.VisualBasic.Asc(e.KeyChar) = 32) Then
                            e.Handled = False
                            Exit Try
                        End If
                        e.Handled = True

                End Select
            Catch ex As Exception

            End Try
        Catch ex As Exception

        End Try
    End Sub

    Private Sub tdbBookCat_Leave(ByVal sender As Object, ByVal e As System.EventArgs) Handles tdbBookCat.Leave
        Try
            tdbBookCat.Update()
            tdbBookCat.Visible = False

        Catch ex As Exception

        End Try


    End Sub

    Private Sub chkBook_CheckedChanged(ByVal sender As System.Object, ByVal e As System.EventArgs) Handles chkBook.CheckedChanged
        If chkBook.Checked = True Then
            ChkNoBooks.Enabled = False
        Else
            ChkNoBooks.Enabled = True
        End If
    End Sub

    Private Sub ChkNoBooks_CheckedChanged(ByVal sender As System.Object, ByVal e As System.EventArgs) Handles ChkNoBooks.CheckedChanged
        If ChkNoBooks.Checked = True Then
            chkBook.Enabled = False
            btnBookCat.Enabled = False
            txtBookCat.Enabled = False
        Else
            chkBook.Enabled = True
            btnBookCat.Enabled = True
            txtBookCat.Enabled = True
        End If

    End Sub
    Public Sub PreviewProcedure(ByVal id As String, Optional PrintAvQty As Boolean = True)

        Dim strsql, strGenFil As String
        Dim i, j As Int32
        Dim frm As frmPreviewPrint

        ' Objects used to set the parameters in the report
        Dim pvCollection As New CrystalDecisions.Shared.ParameterValues

        Dim pdvParam As New CrystalDecisions.Shared.ParameterDiscreteValue


        ' Objects used to set the proper database connection information
        Dim tbCurrent As CrystalDecisions.CrystalReports.Engine.Table
        Dim tliCurrent As CrystalDecisions.Shared.TableLogOnInfo

        Try

            Cursor.Current = Cursors.WaitCursor


            For j = 0 To MchUsr.Count - 1
                Dim rptInvDisc As New ReportDocument
                ' Load the report
                rptInvDisc.Load(ExePath & "CountReports\PrtInvSheet.rpt")


                For Each tbCurrent In rptInvDisc.Database.Tables
                    tliCurrent = tbCurrent.LogOnInfo

                    With tliCurrent.ConnectionInfo
                        .ServerName = vbServer
                        .UserID = "SA"
                        .Password = REDACTED
                        .DatabaseName = vbDBname
                    End With

                    With tbCurrent
                        Try
                            .ApplyLogOnInfo(tliCurrent)
                        Catch ex As Exception
                            If ex.Message.IndexOf("The table could not be found.") < 0 Then
                                Throw ex
                            End If
                        End Try
                    End With

                Next tbCurrent


                strsql = " select stk_recid,"
                strsql = strsql & " stk_ref ,"

                '   If ChkBarcode.Checked Then
                strsql = strsql & " (SELECT TOP 1 case when ltrim(dbar_barcode) = '' then null else dbar_barcode end dbar_barcode FROM dbo.STK04_DetBarcode WHERE dbar_stkid =  Stk_recid   ) barcode , "
                '  Else
                '   strsql = strsql & " convert(nvarchar(20),'') barcode  , "
                '   End If
                strsql = strsql & "stk_desc + (CASE WHEN STK_NEWUSED = 'U' Then ' (Used)' else '' END) dis ,   Pcd_DetGrid,Color_Reference Color_Desc,REPLACE(Pcd_ExpDate,'00.','') Pcd_ExpDate,Pcd_Batch , "
                strsql = strsql & " pcd_qty,stk_unit, "

                strsql &= " CASE WHEN STK_ItmClass = 'E' AND Pcd_ExpDate IS NULL AND Pcd_Batch IS NULL "
                strsql &= " THEN 'Please define the expiry date and Lot#' "
                strsql &= " WHEN STK_ItmClass = 'F' AND Pcd_Color IS NULL AND Pcd_Grid IS NULL "
                strsql &= " THEN 'Please define the Color and Size' "
                strsql &= " ELSE NULL END HaveExp"

                strsql &= " from tbl_physicalcountdetail "

                strsql = strsql & "inner join tbl_physicalcountheader ON Pch_Recid=Pcd_HRecId AND Pch_Loc = Pcd_Loc AND Pch_Whouse = Pcd_Whouse inner join "
                If ClocHO Then
                    strsql &= " STK04_STKFIL "

                Else
                    strsql &= " pos_stkfil "
                End If

                strsql = strsql & "on stk_recid=pcd_stkid    left JOIN Stk04_COLOR  on Pcd_Color = Color_Code "

                strsql &= "    where pcd_hrecid=" & id.Trim & " and pcd_loc = '" & vbCLoc.ToString & "'"



                pdvParam.Value = strsql
                pvCollection.Add(pdvParam)
                rptInvDisc.DataDefinition.ParameterFields("@strsql").ApplyCurrentValues(pvCollection)
                pvCollection.Clear()

                ' Set the connection information for all the tables used in the report
                ' Leave UserID and Password blank for trusted connection
                '''''''''''''''''''''''''''''''''''''''''''''''''''''

                'With rptInvDisc
                '    .DataSourceConnections(0).SetConnection(vbServer, vbDBname, "SA", vbPwd)
                '    .SetDatabaseLogon("SA", vbPwd, vbServer, vbDBname)
                'End With

                'For Each tbCurrent In rptInvDisc.Database.Tables
                '    tbCurrent.Location = vbDBname & ".dbo." & tbPrintInv
                'Next tbCurrent

                For i = 0 To tdbDetails.Columns.Count - 1
                    If tdbDetails.Columns(i).FilterText <> "" Then
                        strGenFil &= tdbDetails.Splits(0).DisplayColumns(i).Name & " : " & tdbDetails.Columns(i).FilterText & " ' + chr(13) + '"
                    End If
                Next

                With rptInvDisc.DataDefinition
                    .FormulaFields("foLocations").Text = "'" & vbCLocName & "'"
                    .FormulaFields("foAdress").Text = "'" & vbCLocAdress & "'"
                    .FormulaFields("foVatReg").Text = "'VAT registration: " & vbVatNbr & "'"
                    .FormulaFields("foUser").Text = "'" & vbUser & "'"
                    .FormulaFields("foTitle").Text = "'Users'"
                    .FormulaFields("foGenFilter").Text = "' Physical Count ID : " & id.Trim & "' & '   ' & ' Date : " & Now.ToLongDateString.Trim & "' & '   ' & ' WareHouse : " & CStr(IIf(vbCLocName Is Nothing, "", vbCLocName)).Trim.Replace("'", "''") & "'"
                    '     .FormulaFields("foFilter").Text = "'" & strGenFil & "'"
                    .FormulaFields("DecNo").Text = DecNo.ToString
                    .FormulaFields("PrintAvQty").Text = "'" & PrintAvQty & "'"
                    .FormulaFields("PrintBarcode").Text = ChkBarcode.Checked
                End With

                ''rptInvDisc.VerifyDatabase()
                'rptInvDisc.Refresh()

                '' Set the discreet value to the start Reference.
                'pdvParam.Value = tbPrintInv

                '' Add the start Reference value to the parameter collection.
                'pvCollection.Add(pdvParam)

                '' Apply the start Reference parameter value.
                ''    rptInvDisc.DataDefinition.ParameterFields("@TblName").ApplyCurrentValues(pvCollection)

                '' Clear for next parameter
                'pvCollection.Clear()

                'Load Preview form 
                frm = New frmPreviewPrint

                With frm
                    .Owner = Me
                    If MchUsr(j).ToString.StartsWith("IT") Then
                        MchUsr(j) = "Count Machine : " & MchUsr(j).ToString
                    ElseIf MchUsr(j).ToString.Substring(0, 1) = "U" Then
                        MchUsr(j) = "User Name : " & MchUsr(j).ToString
                    End If
                    rptInvDisc.DataDefinition.FormulaFields("foTitle").Text = "'" & MchUsr(j).ToString.Replace("'", "''") & "'"
                    .Show(rptInvDisc, Me.Text, 1, , True, True, True, , , , , , , , )
                End With
                rptInvDisc = Nothing
            Next




        Catch Exp As LoadSaveReportException
            MessageBox.Show("Incorrect path for loading report.",
                     Me.Text & " : Load Report Error", MessageBoxButtons.OK, MessageBoxIcon.Error)

        Catch Exp As Exception
            MessageBox.Show(Exp.Message, Me.Text & " : General Error", MessageBoxButtons.OK, MessageBoxIcon.Error)

        Finally

            frm = Nothing

            pdvParam = Nothing
            tbCurrent = Nothing
            tliCurrent = Nothing
            pvCollection = Nothing

            'rptInvDisc = Nothing



            Cursor.Current = Cursors.Default

        End Try
    End Sub

    Private Sub tdbDetails_KeyPress(sender As Object, e As System.Windows.Forms.KeyPressEventArgs) Handles tdbDetails.KeyPress

        Dim col As String
        Dim txt As New TextBox

        Try
            col = tdbDetails.Splits(0).DisplayColumns(tdbDetails.Col).DataColumn.DataField

            txt.Text = tdbDetails.Columns(col).Text
            Select Case col
                Case "Chck"
                    If (Microsoft.VisualBasic.Asc(e.KeyChar) = 32) Then
                        e.Handled = False
                        Exit Try
                    End If
                    e.Handled = True

            End Select
        Catch ex As Exception

        End Try
    End Sub

    Private Sub Button1_Click(sender As System.Object, e As System.EventArgs) Handles chekall.Click

        Try
            Cursor.Current = Cursors.WaitCursor
            Dim dr As DataRow
            For Each dr In dsDet.Tables("Details").Select(dsDet.Tables("Details").DefaultView.RowFilter.ToString)
                dr.Item("Check") = chekall.Tag.ToString

            Next


            With tdbBookCat
                .Columns("Check").RefreshCell()
                .Focus() : .Col = 0
            End With

            If chekall.Tag.ToString = "False" Then
                chekall.Tag = "True"
                chekall.Text = "    Check All       "
            Else
                chekall.Tag = "False"
                chekall.Text = "    UnCheck All   "
            End If

            For Each dr In dsDet.Tables("Details").Rows
                If dr.Item("Check") = True Then
                    txtBookCat.Text &= dr.Item("Code").ToString & ","
                Else
                    txtBookCat.Text = txtBookCat.Text.Replace(dr.Item("Code").ToString & ",", "")
                End If

            Next


        Catch exp As Exception
            MessageBox.Show(exp.Message, Me.Text & ": General Error",
            MessageBoxButtons.OK, MessageBoxIcon.Error)
        Finally
            Cursor.Current = Cursors.Default
        End Try
    End Sub



    Private Sub txtGeneralSearch_ButtonCustomClick(sender As Object, e As System.EventArgs) Handles txtGeneralSearch.ButtonCustomClick
        Try
            txtGeneralSearch.Text = ""
            populateSearchFromMasterSearch()
        Catch ex As Exception
            MessageBox.Show(ex.Message, Me.Text, MessageBoxButtons.OK, MessageBoxIcon.Error)
        End Try
    End Sub

    Private Sub txtGeneralSearch_Enter(sender As Object, e As System.EventArgs) Handles txtGeneralSearch.Enter
        Try
            FocusStyle(CType(sender, TextBox), "Enter")
        Catch exp As Exception
            MessageBox.Show("Error : " & exp.Message, Me.Text,
            MessageBoxButtons.OK, MessageBoxIcon.Error)
        End Try
    End Sub

    Private Sub txtGeneralSearch_KeyDown(sender As Object, e As System.Windows.Forms.KeyEventArgs) Handles txtGeneralSearch.KeyDown
        Try
            If e.KeyCode = Keys.Escape Then
                Me.DialogResult = DialogResult.Cancel
            ElseIf e.KeyCode = Keys.Down Then
                SendKeys.Send("{TAB}")
            End If
        Catch ex As Exception
            MessageBox.Show(ex.Message, Me.Text, MessageBoxButtons.OK, MessageBoxIcon.Error)
        End Try
    End Sub

    Private Sub txtGeneralSearch_KeyUp(sender As Object, e As System.Windows.Forms.KeyEventArgs) Handles txtGeneralSearch.KeyUp
        Try
            populateSearchFromMasterSearch()
        Catch ex As Exception
            MessageBox.Show(ex.Message, Me.Text, MessageBoxButtons.OK, MessageBoxIcon.Error)
        End Try
    End Sub

    Private Sub populateSearchFromMasterSearch()
        Dim iCount As Int32
        Dim sFltr As String
        Try
            Cursor.Current = Cursors.WaitCursor

            tdbDetails.FilterActive = True

            sFltr = GetFiltration()

            GridFilterChange(tdbDetails, sFltr)

            'Apply...
            'dtSource.DefaultView.RowFilter = sFltr

        Catch ex As Exception
            Throw ex
        Finally
            Application.DoEvents()
            txtGeneralSearch.Focus() : Cursor.Current = Cursors.Default
        End Try
    End Sub

    Private Function GetFiltration() As String
        Dim sFindText As String
        Dim strsql As String
        Dim sColName As String
        Dim i As Integer
        Try

            If txtGeneralSearch.Text.Trim <> "" Then

                sFindText = txtGeneralSearch.Text.Replace("'", "''")
                sFindText = sFindText.Replace("[", "[[]")
                If sFindText.IndexOf("%") >= 0 OrElse sFindText.IndexOf("*") >= 0 Then
                    sFindText = Replace(Replace(sFindText, "*", "[*]"), "%", "[%]")
                End If


                strsql = " ( "

                For i = 0 To tdbDetails.Splits(0).DisplayColumns.Count - 1
                    If tdbDetails.Splits(0).DisplayColumns(i).Visible = True Then 'added by GS on 13/05/16
                        With tdbDetails.Splits(0).DisplayColumns(i)
                            sColName = .DataColumn.DataField

                            If dsDetails.Columns(sColName).DataType Is System.Type.GetType("System.DateTime") Then
                                If sFindText = "" Then
                                    strsql &= "IsNull([" & .DataColumn.DataField & "],'01/01/1900')='01/01/1900' or "
                                ElseIf IsDate(sFindText) = True Then
                                    If sFindText.Length = 10 AndAlso
                                       sFindText.IndexOf(":") < 0 Then
                                        strsql &= " [" & .DataColumn.DataField & "] >= #" &
                                         CDate(sFindText & " 00:00:00").ToString("MM/dd/yyyy HH:mm:ss") & "# and " &
                                         " [" & .DataColumn.DataField & "] <= #" &
                                         CDate(sFindText & " 23:59:59").ToString("MM/dd/yyyy HH:mm:ss") & "# or "
                                    ElseIf sFindText.Length = 19 AndAlso sFindText.IndexOf(":") = 13 Then
                                        strsql &= " [" & .DataColumn.DataField & "] = #" &
                                        CDate(sFindText).ToString("MM/dd/yyyy hh:mm:ss") & "# or "
                                    End If
                                End If
                            ElseIf dsDetails.Columns(sColName).DataType Is System.Type.GetType("System.String") Then
                                If sFindText = "" Then
                                    strsql &= "(IsNull([" & .DataColumn.DataField & "],'') = '' or Trim([" & .DataColumn.DataField & "]) = '') or "
                                Else
                                    strsql &= " [" & .DataColumn.DataField & "] like '%" & sFindText & "%' or "
                                End If
                            ElseIf dsDetails.Columns(sColName).DataType Is System.Type.GetType("System.Boolean") Then
                                strsql &= " [" & .DataColumn.DataField & "] = '" & sFindText & "' or "
                            Else
                                If sFindText = "" Then
                                    strsql &= "IsNull([" & .DataColumn.DataField & "],-0.00001)=-0.00001 or "
                                ElseIf IsNumeric(sFindText) Then
                                    'edited by GS on 13/05/16
                                    'strsql &= " [" & .DataColumn.DataField & "] " & .DataColumn.FilterOperator & " " & _
                                    'CDbl(sFindText) & " or "
                                    strsql &= " [" & .DataColumn.DataField & "] " & IIf(.DataColumn.FilterOperator.Trim = "", "=", .DataColumn.FilterOperator).ToString & " " &
                                    CDbl(sFindText) & " or "
                                    'GS on 13/05/16
                                End If

                            End If
                        End With
                    End If 'added by GS on 13/05/16
                Next

                If Not strsql Is Nothing Then
                    strsql = strsql.Substring(0, strsql.Length - 4)

                    strsql &= " ) "
                Else
                    strsql = ""
                End If

            End If


            Return strsql

        Catch ex As Exception
            Throw ex
        End Try
    End Function

    Private Sub txtGeneralSearch_Leave(sender As Object, e As System.EventArgs) Handles txtGeneralSearch.Leave
        Try
            FocusStyle(CType(sender, TextBox), "Leave")
        Catch exp As Exception
            MessageBox.Show("Error : " & exp.Message, Me.Text,
            MessageBoxButtons.OK, MessageBoxIcon.Error)
        End Try
    End Sub

    Private Sub ResizeCol(lst As C1TrueDBGrid)
        Dim WidthOfCol, i As Integer
        Try
            If Screen.PrimaryScreen.WorkingArea.Width < 1250 Then Exit Try
            lst.Splits(0).DisplayColumns("stk_desc").Width = 0
            WidthOfCol = 0
            For i = 0 To lst.Columns.Count - 1
                If lst.Splits(0).DisplayColumns(i).Visible = True Then
                    WidthOfCol += lst.Splits(0).DisplayColumns(i).Width
                End If
            Next
            lst.Splits(0).DisplayColumns("stk_desc").Width = CInt(Math.Floor((lst.Width - WidthOfCol - lst.RecordSelectorWidth - lst.VScrollBar.Width - i)))
        Catch ex As Exception
            MessageBox.Show("Error : " & ex.Message, Me.Text,
            MessageBoxButtons.OK, MessageBoxIcon.Error)
        End Try
    End Sub

    Private Sub tdbDetails_HeadClick(sender As Object, e As C1.Win.C1TrueDBGrid.ColEventArgs) Handles tdbDetails.HeadClick
        Dim frmselect As frmSearchWithCheck
        Dim dr, drs As DataRow
        Dim test As String
        Dim Alnfield() As String
        Dim Sizefield() As Int16
        Dim da As SqlDataAdapter
        Dim Filtration, Filtration1 As String
        Dim brdsel, categSel, ProduSel, ItemIdentSel As String
        Sizefield = New Int16() {36, 0, 100, 240}
        Alnfield = New String() {"C", "C", "C", "N"}
        Try
            If UCase(e.Column.DataColumn.DataField) = "BRAND" Then
                tdbDetails.AllowSort = False
                frmselect = New frmSearchWithCheck
                If dtBrand Is Nothing Then
                    dtBrand = New DataTable("Brand")
                    strsql = "Select distinct 'false' [chk] ,'' [Chk2],Br_code as [Code], Br_Name as [Description] From stk04_brand inner join " & tbPhyCntView & " on BrandCode=br_code "
                    da = New SqlDataAdapter(strsql, CONNECTION_STRING)
                    da.Fill(dtBrand)
                End If
                With frmselect
                    .Owner = Me
                    If .ShowDialog("Return", dtBrand, dtBrand.Rows.Count, "Brand List", Sizefield, Alnfield, Nothing, 0,
                        Nothing, False, 400, 500, , True, , , , , , , , False, , , , , , , , , , , , , , , , True) = DialogResult.OK Then
                    End If
                End With
            ElseIf UCase(e.Column.DataColumn.DataField) = "CATEGORY" Then
                tdbDetails.AllowSort = False
                frmselect = New frmSearchWithCheck
                If dtcategory Is Nothing Then
                    dtcategory = New DataTable("Category")
                    strsql = "Select distinct 'false' [chk] ,'' [Chk2],Cat_Code as [Code], Cat_Desc as [Description] From Stk04_Category inner join " & tbPhyCntView & " on CatCode=cat_code "
                    da = New SqlDataAdapter(strsql, CONNECTION_STRING)
                    da.Fill(dtcategory)
                End If
                With frmselect
                    .Owner = Me
                    If .ShowDialog("Return", dtcategory, dtcategory.Rows.Count, "Category List", Sizefield, Alnfield, Nothing, 0,
                        Nothing, False, 400, 500, , True, , , , , , , , False, , , , , , , , , , , , , , , , True) = DialogResult.OK Then
                        'For Each drs In dtcategory.Select("")
                        '    If drs.Item("Chk").ToString = "True" Then
                        '        For Each dr In dsDetails.Select(" CatCode ='" & drs.Item("Code").ToString & "'   ")
                        '            dr.Item("Chck") = "True"
                        '        Next
                        '    Else
                        '        For Each dr In dsDetails.Select(" CatCode ='" & drs.Item("Code").ToString & "'   ")
                        '            dr.Item("Chck") = "False"
                        '        Next
                        '    End If
                        'Next
                    End If
                End With
            ElseIf UCase(e.Column.DataColumn.DataField) = "ITEMTYPE" Then
                tdbDetails.AllowSort = False
                frmselect = New frmSearchWithCheck
                If dtproduct Is Nothing Then
                    dtproduct = New DataTable("ItemType")
                    strsql = "Select distinct 'false' [chk] ,'' [Chk2],IT_Code as [Code], IT_DESC as [Description] From stk04_itemtype inner join " & tbPhyCntView & " on ItemTypeCode=IT_Code "
                    da = New SqlDataAdapter(strsql, CONNECTION_STRING)
                    da.Fill(dtproduct)
                End If
                With frmselect
                    .Owner = Me
                    If .ShowDialog("Return", dtproduct, dtproduct.Rows.Count, "Product Group  List", Sizefield, Alnfield, Nothing, 0,
                        Nothing, False, 400, 500, , True, , , , , , , , False, , , , , , , , , , , , , , , , True) = DialogResult.OK Then
                        'For Each drs In dtproduct.Select("")
                        '    If drs.Item("Chk").ToString = "True" Then
                        '        For Each dr In dsDetails.Select(" ItemTypeCode ='" & drs.Item("Code").ToString & "'   ")
                        '            dr.Item("Chck") = "True"
                        '        Next
                        '    Else
                        '        For Each dr In dsDetails.Select(" ItemTypeCode ='" & drs.Item("Code").ToString & "'   ")
                        '            dr.Item("Chck") = "False"
                        '        Next
                        '    End If
                        'Next
                    End If
                End With
            ElseIf UCase(e.Column.DataColumn.DataField) = "ITEMIDENTIF" Then
                tdbDetails.AllowSort = False
                frmselect = New frmSearchWithCheck
                If dtIdentification Is Nothing Then
                    dtIdentification = New DataTable("ItemIdent")
                    strsql = "Select distinct 'false' [chk] ,'' [Chk2],ID_Code as [Code], ID_DESC as [Description] From STK04_ItmIdentif inner join " & tbPhyCntView & " on ItemIdentifCode=ID_Code "
                    da = New SqlDataAdapter(strsql, CONNECTION_STRING)
                    da.Fill(dtIdentification)
                End If
                With frmselect
                    .Owner = Me
                    If .ShowDialog("Return", dtIdentification, dtIdentification.Rows.Count, "Item Identification List", Sizefield, Alnfield, Nothing, 0,
                        Nothing, False, 400, 500, , True, , , , , , , , False, , , , , , , , , , , , , , , , True) = DialogResult.OK Then

                    End If
                End With
            Else
                Exit Try
            End If
            Me.Cursor = Cursors.WaitCursor
            daDetails.Update(dsDetails)
            Execute("update " & tbPhyCntView & " set [Chck] = 'FALSE'")
            Filtration = ""
            Filtration1 = ""
            brdsel = ""
            ProduSel = ""
            ItemIdentSel = ""
            categSel = ""
            MemoPart1 = ""
            If Not dtBrand Is Nothing AndAlso dtBrand.Select(" CHK='true' ").Length > 0 Then
                For Each drs In dtBrand.Select(" CHK='true' ")
                    Filtration1 &= " '" & drs.Item("Code").ToString & "' ,"
                    brdsel &= "," & drs.Item("Description").ToString
                Next
                Filtration &= "or  BrandCode in (" & Mid(Filtration1, 1, Filtration1.Length - 1) & " )  "
                tdbDetails.Columns("Brand").FooterText = "Brands Selected : " & dtBrand.Compute("count([Chk])", "[Chk]=True").ToString & "/" & dtBrand.Rows.Count
                MemoPart1 &= vbCrLf & "Brands Selected ( " & Mid(brdsel.Trim, 2, brdsel.Length) & " ) "
            ElseIf Not dtBrand Is Nothing AndAlso dtBrand.Select(" CHK='true' ").Length = 0 Then
                tdbDetails.Columns("Brand").FooterText = "Brands Selected : 0/" & dtBrand.Rows.Count
            End If
            If Not dtcategory Is Nothing AndAlso dtcategory.Select(" CHK='true' ").Length > 0 Then
                Filtration1 = ""
                For Each drs In dtcategory.Select(" CHK='true' ")
                    Filtration1 &= "   '" & drs.Item("Code").ToString & "',"
                    categSel &= "," & drs.Item("Description").ToString
                Next
                Filtration &= "or CatCode in ( " & Mid(Filtration1, 1, Filtration1.Length - 1) & " ) "
                tdbDetails.Columns("Category").FooterText = "Categories Selected : " & dtcategory.Compute("count([Chk])", "[Chk]=True").ToString & "/" & dtcategory.Rows.Count
                MemoPart1 &= vbCrLf & "Categories Selected ( " & Mid(categSel.Trim, 2, categSel.Length) & " ) "
            ElseIf Not dtcategory Is Nothing AndAlso dtcategory.Select(" CHK='true' ").Length = 0 Then
                tdbDetails.Columns("Category").FooterText = "Categories Selected : 0/" & dtcategory.Rows.Count
            End If
            If Not dtproduct Is Nothing AndAlso dtproduct.Select(" CHK='true' ").Length > 0 Then
                Filtration1 = ""
                For Each drs In dtproduct.Select(" CHK='true'  ")
                    Filtration1 &= "  '" & drs.Item("Code").ToString & "',"
                    ProduSel &= "," & drs.Item("Description").ToString
                Next
                Filtration &= "or ItemTypeCode in (" & Mid(Filtration1, 1, Filtration1.Length - 1) & ") "
                tdbDetails.Columns("ItemType").FooterText = "Product Groups Selected : " & dtproduct.Compute("count([Chk])", "[Chk]=True").ToString & "/" & dtproduct.Rows.Count
                MemoPart1 &= vbCrLf & "Product Groups Selected ( " & Mid(ProduSel.Trim, 2, ProduSel.Length) & " ) "
            ElseIf Not dtproduct Is Nothing AndAlso dtproduct.Select(" CHK='true' ").Length = 0 Then
                tdbDetails.Columns("ItemType").FooterText = "Product Groups Selected : 0/" & dtproduct.Rows.Count
            End If
            If Not dtIdentification Is Nothing AndAlso dtIdentification.Select(" CHK='true' ").Length > 0 Then
                Filtration1 = ""
                For Each drs In dtIdentification.Select(" CHK='true'  ")
                    Filtration1 &= "  '" & drs.Item("Code").ToString & "',"
                    ItemIdentSel &= "," & drs.Item("Description").ToString
                Next
                Filtration &= "or ItemIdentifCode in (" & Mid(Filtration1, 1, Filtration1.Length - 1) & ") "
                tdbDetails.Columns("ItemIdentif").FooterText = "Identifications Selected : " & dtIdentification.Compute("count([Chk])", "[Chk]=True").ToString & "/" & dtIdentification.Rows.Count
                MemoPart1 &= vbCrLf & "Item Identifications Selected ( " & Mid(ItemIdentSel.Trim, 2, ItemIdentSel.Length) & " ) "
            ElseIf Not dtIdentification Is Nothing AndAlso dtIdentification.Select(" CHK='true' ").Length = 0 Then
                tdbDetails.Columns("ItemIdentif").FooterText = " Identifications Selected : 0/" & dtIdentification.Rows.Count
            End If
            If Filtration <> "" Then
                'For Each dr In dsDetails.Select(Mid(Filtration, 5, Filtration.Length))
                '    dr.Item("Chck") = "True"
                'Next 
                Execute("update " & tbPhyCntView & " set [Chck] = 'TRUE'  where  (" & Mid(Filtration, 3, Filtration.Length) & ") ")

            End If
            dsDetails.Clear()
            daDetails.Fill(dsDetails)
            txtItmNo.Text = dsDetails.Compute("count([Chck])", "[Chck]=True").ToString
            TxtMemo1.Text = "Count Date : " & txtDatePhyCnt.Text & MemoPart2 & MemoPart1
        Catch ex As Exception
            MessageBox.Show("Error : " & ex.Message, Me.Text, _
            MessageBoxButtons.OK, MessageBoxIcon.Error)
        Finally
            frmselect = Nothing
            Me.Cursor = Cursors.Default
        End Try
    End Sub



    Private Sub ChkBarcode_CheckedChanged(sender As Object, e As EventArgs) Handles ChkBarcode.CheckedChanged
        Try
            FillGrid()
        Catch ex As Exception

        End Try
    End Sub



    Private Function DataNotSync(ByVal TempTable As String, ByVal HOLServer As String) As Boolean
        Dim strsql, sqlPendingData, msgPendingData, LocSyncSrv, sqlDisc As String
        Try

            msgPendingData = ""

            'Get Location Sync Server, DB
            strsql = "  SELECT CASE WHEN CHARINDEX('---', Loc_sqlserverSync, 1)<>0 THEN  "
            strsql &= " SUBSTRING(Loc_sqlserverSync, CHARINDEX('---', Loc_sqlserverSync, 1) + 3, LEN(Loc_sqlserverSync))"
            strsql &= " ELSE substring(Loc_sqlserverSync,charindex('--',Loc_sqlserverSync,1)+2,len(Loc_sqlserverSync)) END "
            strsql &= " Loc_sqlserverSync , Loc_dbSync FROM STK04_LOCATION WHERE Loc_Code = '" & vbCLoc.Trim & "' "

            LocSyncSrv = ""
            Using dtGetSync As New DataTable
                Using dasql As New SqlDataAdapter(strsql, CONNECTION_STRING)
                    dasql.Fill(dtGetSync)
                End Using

                If dtGetSync.Rows(0)("Loc_sqlserverSync").ToString.Trim = "" Or dtGetSync.Rows(0)("Loc_dbSync").ToString.Trim = "" Then
                    ShowNotification(Me, "Location Sync Server Not Defined, Please Contact Your Administrator", 5000, 12, DevComponents.DotNetBar.eToastPosition.MiddleCenter)
                    Return True
                End If

                If vbDBname.Trim.ToUpper = dtGetSync.Rows(0)("Loc_dbSync").ToString.Trim.ToUpper Then
                    ShowNotification(Me, "Location Sync DB Should Be Different from POS DB, Please Contact Your Administrator", 5000, 12, DevComponents.DotNetBar.eToastPosition.MiddleCenter)
                    Return True
                End If

                If vbServer.Trim.ToUpper = dtGetSync.Rows(0)("Loc_sqlserverSync").ToString.Trim.ToUpper Then
                    LocSyncSrv = "[" & dtGetSync.Rows(0)("Loc_dbSync").ToString & "].dbo."
                Else
                    LocSyncSrv = "[" & dtGetSync.Rows(0)("Loc_sqlserverSync").ToString & "].[" & dtGetSync.Rows(0)("Loc_dbSync").ToString & "].dbo."
                End If

            End Using

            'Offers
            sqlPendingData = " SELECT  TOP 1 1 FROM " & TempTable & " WHERE  Chck = 'True'"
            sqlPendingData &= " AND EXISTS ( SELECT TOP 1 1 FROM " & LocSyncSrv & "STK04_OfferDetails"
            sqlPendingData &= " Where Stkid = DOFR_ITEM )"

            If RecordsExists(sqlPendingData) Then
                msgPendingData &= "> Offers in POS" & vbCrLf
            End If

            sqlPendingData = " SELECT  TOP 1 1 FROM " & TempTable & " WHERE  Chck = 'True'"
            sqlPendingData &= " AND EXISTS ( SELECT TOP 1 1 FROM " & HOLServer & "SyncPOS_" & vbCLoc.Trim & "_OfferDetails"
            sqlPendingData &= " Where Stkid = DOFR_ITEM )"

            If RecordsExists(sqlPendingData) Then
                msgPendingData &= "> Offers in BO" & vbCrLf
            End If

            'Sales Invoices / Adjustments
            sqlPendingData = " SELECT  TOP 1 1 FROM " & TempTable & " WHERE  Chck = 'True'"
            sqlPendingData &= " AND EXISTS ( SELECT TOP 1 1 FROM " & LocSyncSrv & "POS_SALEINVDET"
            sqlPendingData &= " Where Stkid = INVD_STKID )"

            If RecordsExists(sqlPendingData) Then
                msgPendingData &= "> Sales Invoices / Adjustments in POS" & vbCrLf
            End If

            sqlPendingData = " SELECT  TOP 1 1 FROM " & TempTable & " WHERE  Chck = 'True'"
            sqlPendingData &= " AND EXISTS ( SELECT TOP 1 1 FROM " & HOLServer & "SyncPOS_" & vbCLoc.Trim & "_SALEINVDET"
            sqlPendingData &= " Where Stkid = INVD_STKID )"

            If RecordsExists(sqlPendingData) Then
                msgPendingData &= "> Sales Invoices / Adjustments in BO" & vbCrLf
            End If

            'Disposals
            sqlPendingData = " SELECT  TOP 1 1 FROM " & TempTable & " WHERE  Chck = 'True'"
            sqlPendingData &= " AND EXISTS ( SELECT TOP 1 1 FROM " & LocSyncSrv & "POS_DISPOSALDETAILS"
            sqlPendingData &= " Where Stkid = DISPD_STKRECID )"

            If RecordsExists(sqlPendingData) Then
                msgPendingData &= "> Disposals in POS" & vbCrLf
            End If

            sqlPendingData = " SELECT  TOP 1 1 FROM " & TempTable & " WHERE  Chck = 'True'"
            sqlPendingData &= " AND EXISTS ( SELECT TOP 1 1 FROM " & HOLServer & "SyncPOS_" & vbCLoc.Trim & "_DISPOSALDETAILS"
            sqlPendingData &= " Where Stkid = DISPD_STKRECID )"

            If RecordsExists(sqlPendingData) Then
                msgPendingData &= "> Disposals in BO" & vbCrLf
            End If

            'Consumptions
            sqlPendingData = " SELECT  TOP 1 1 FROM " & TempTable & " WHERE  Chck = 'True'"
            sqlPendingData &= " AND EXISTS ( SELECT TOP 1 1 FROM " & LocSyncSrv & "POS_CONSUMPTIOND"
            sqlPendingData &= " Where Stkid = CONSD_STKID )"

            If RecordsExists(sqlPendingData) Then
                msgPendingData &= "> Consumptions in POS" & vbCrLf
            End If

            sqlPendingData = " SELECT  TOP 1 1 FROM " & TempTable & " WHERE  Chck = 'True'"
            sqlPendingData &= " AND EXISTS ( SELECT TOP 1 1 FROM " & HOLServer & "SyncPOS_" & vbCLoc.Trim & "_CONSUMPTIOND"
            sqlPendingData &= " Where Stkid = CONSD_STKID )"

            If RecordsExists(sqlPendingData) Then
                msgPendingData &= "> Consumptions in BO" & vbCrLf
            End If

            'Delivery Orders
            sqlPendingData = " SELECT  TOP 1 1 FROM " & TempTable & " WHERE  Chck = 'True'"
            sqlPendingData &= " AND EXISTS ( SELECT TOP 1 1 FROM " & LocSyncSrv & "STK04_DelOrdD"
            sqlPendingData &= " Where Stkid = DOD_StkRecid )"

            If RecordsExists(sqlPendingData) Then
                msgPendingData &= "> Delivery Orders in POS" & vbCrLf
            End If

            sqlPendingData = " SELECT  TOP 1 1 FROM " & TempTable & " WHERE  Chck = 'True'"
            sqlPendingData &= " AND EXISTS ( SELECT TOP 1 1 FROM " & HOLServer & "SyncPOS_" & vbCLoc.Trim & "_DelOrdD"
            sqlPendingData &= " Where Stkid = DOD_StkRecid )"

            If RecordsExists(sqlPendingData) Then
                msgPendingData &= "> Delivery Orders in BO" & vbCrLf
            End If

            'Transfers
            sqlPendingData = " SELECT  TOP 1 1 FROM " & TempTable & " WHERE  Chck = 'True'"
            sqlPendingData &= " AND EXISTS ( SELECT TOP 1 1 FROM " & LocSyncSrv & "POS_TransferDetailsPOS"
            sqlPendingData &= " Where Stkid = Trans_RollDet )"

            If RecordsExists(sqlPendingData) Then
                msgPendingData &= "> Transfers in POS" & vbCrLf
            End If

            sqlPendingData = " SELECT  TOP 1 1 FROM " & TempTable & " WHERE  Chck = 'True'"
            sqlPendingData &= " AND EXISTS ( SELECT TOP 1 1 FROM " & HOLServer & "SyncPOS_" & vbCLoc.Trim & "_TransferDetailsPOS"
            sqlPendingData &= " Where Stkid = Trans_RollDet )"

            If RecordsExists(sqlPendingData) Then
                msgPendingData &= "> Transfers in BO" & vbCrLf
            End If

            'Transpositions
            sqlPendingData = " SELECT  TOP 1 1 FROM " & TempTable & " WHERE  Chck = 'True'"
            sqlPendingData &= " AND EXISTS ( SELECT TOP 1 1 FROM " & LocSyncSrv & "POS_TRANSPOSITIONDETAILS"
            sqlPendingData &= " Where Stkid = TRSPD_STKRECID )"

            If RecordsExists(sqlPendingData) Then
                msgPendingData &= "> Transpositions in POS" & vbCrLf
            End If

            sqlPendingData = " SELECT  TOP 1 1 FROM " & TempTable & " WHERE  Chck = 'True'"
            sqlPendingData &= " AND EXISTS ( SELECT TOP 1 1 FROM " & HOLServer & "SyncPOS_" & vbCLoc.Trim & "_TRANSPOSITIONDETAILS"
            sqlPendingData &= " Where Stkid = TRSPD_STKRECID )"

            If RecordsExists(sqlPendingData) Then
                msgPendingData &= "> Transpositions in BO" & vbCrLf
            End If

            'Stock QTY pending to be sent to HO
            sqlPendingData = " SELECT  TOP 1 1 FROM " & TempTable & " WHERE  Chck = 'True'"
            sqlPendingData &= " AND EXISTS ( SELECT TOP 1 1 FROM " & LocSyncSrv & "pos_stkqty"
            sqlPendingData &= " Where STQ_Loc = '00' AND Stkid = STQ_STKRECID )"

            If RecordsExists(sqlPendingData) Then
                msgPendingData &= "> Stock QTY" & vbCrLf
            End If

            If msgPendingData.Trim <> "" Then
                MessageBox.Show("Exist Not Synchronized Data, Please Try Again." & vbCrLf, "Synchronization", MessageBoxButtons.OK, MessageBoxIcon.Exclamation)
                Return True
            End If

            'POS Stock QTY Discrepancy
            sqlDisc = "  Select top 1 1 from " & TempTable & " tmp Where tmp.Chck = 'True' "
            sqlDisc &= " AND EXISTS ( SELECT  TOP 1 1 "
            sqlDisc &= " FROM " & HOLServer & "stk04_stkqty main "
            sqlDisc &= " INNER JOIN pos_stkqty pos "
            sqlDisc &= " ON pos.STQ_LOCCODE = main.STQ_LOCCODE "
            sqlDisc &= " AND pos.STQ_STKRECID = main.STQ_STKRECID "
            sqlDisc &= " AND coalesce(pos.STQ_Color,'')		  = coalesce(main.STQ_Color,'')"
            sqlDisc &= " AND coalesce(pos.STQ_Grid,'')		  = coalesce(main.STQ_Grid,'')"
            sqlDisc &= " AND COALESCE(pos.STQ_DetGrid,'')	  = coalesce(main.STQ_DetGrid,'') "
            sqlDisc &= " AND coalesce(pos.STQ_ExpiryDt,'')	  = coalesce(main.STQ_ExpiryDt,'')"
            sqlDisc &= " AND coalesce(pos.STQ_BatchInfo,'')	  = coalesce(main.STQ_BatchInfo,'')"
            sqlDisc &= " WHERE   main.STQ_LOCCODE = '" & vbCLoc.Trim & "' AND pos.STQ_LOCCODE = '" & vbCLoc.Trim & "' "
            sqlDisc &= " AND tmp.Stkid = pos.STQ_STKRECID "
            sqlDisc &= " AND (ISNULL(pos.STQ_AVAILABLE, 0)    <> ISNULL(main.STQ_AVAILABLE, 0) "
            sqlDisc &= " OR ISNULL(pos.STQ_TransPendI, 0)     <> ISNULL(main.STQ_TransPendI, 0) "
            sqlDisc &= " OR ISNULL(pos.STQ_TransPendO, 0)     <> ISNULL(main.STQ_TransPendO, 0) "
            sqlDisc &= " OR ISNULL(pos.STQ_RESERVED, 0)       <> ISNULL(main.STQ_RESERVED, 0) "
            sqlDisc &= " OR ISNULL(pos.STQ_ProcProduction, 0) <> ISNULL(main.STQ_ProcProduction, 0)"
            sqlDisc &= " ))"

            If RecordsExists(sqlDisc) Then
                MessageBox.Show("Exist Discrepancy Between Head Office And POS. Please Contact Your Administrator" & vbCrLf, "Synchronization", MessageBoxButtons.OK, MessageBoxIcon.Exclamation)
                Return True
            End If

            Return False

        Catch ex As Exception
            Throw ex
        End Try
    End Function

    Private Sub btnImportTransfer_Click(sender As Object, e As EventArgs) Handles btnImportTransfer.Click
        Try

            Cursor.Current = Cursors.WaitCursor

            Using frmImpTrf As New frmImportTransfer
                frmImpTrf.CurrWD = WorkingDay
                If frmImpTrf.ShowDialog = Windows.Forms.DialogResult.OK Then

                    Using frmW As New FrmStatus
                        frmW.Show("Importing Transfer, Please Wait...")
                        ImportTransfer(frmImpTrf.SelectedRecID, frmImpTrf.SelectedTransfer)
                    End Using

                End If
            End Using

        Catch exp As Exception
            MessageBox.Show(exp.Message, Me.Text & ": General Error", MessageBoxButtons.OK, MessageBoxIcon.Error)
        Finally
            Cursor.Current = Cursors.Default
        End Try
    End Sub

    Private Sub ImportTransfer(_RecID As String, _TransferInfo As String)
        Dim strsql As String
        Dim newlyChecked As Integer = 0
        Dim notInList As Integer = 0
        Dim vbCnt As Int32

        Try

            tdbDetails.UpdateData()
            daDetails.Update(dsDetails)

            Using cnSql As New SqlConnection(CONNECTION_STRING)
                cnSql.Open()

                Using cmSql As New SqlCommand()
                    cmSql.Connection = cnSql
                    cmSql.CommandTimeout = 600
                    cmSql.Parameters.AddWithValue("@id", CDbl(_RecID))

                    cmSql.CommandText = " SELECT COUNT(DISTINCT Trans_RollDet)
                                          FROM   dbo.pos_TransferDetailsPOS 
                                          WHERE  Header_recid = @id
                                                 AND NOT EXISTS ( SELECT 1
                                                                  FROM   " & tbPhyCntView & " 
                                                                  WHERE  Stkid = Trans_RollDet )"
                    notInList = CInt(cmSql.ExecuteScalar())

                    cmSql.CommandText = " UPDATE    " & tbPhyCntView & "
                                          SET       Chck = 'TRUE'
                                          FROM      " & tbPhyCntView & "
                                          WHERE     ISNULL(Chck, 'FALSE') <> 'TRUE'
                                                    AND EXISTS ( SELECT 1
                                                                 FROM   dbo.pos_TransferDetailsPOS
                                                                 WHERE  Header_recid = @id
                                                                        AND Trans_RollDet = Stkid )"

                    newlyChecked = cmSql.ExecuteNonQuery()

                    cmSql.CommandText = "    UPDATE " & tbPhyCnt & "
                                             SET  Chck = 'TRUE'
                                             FROM " & tbPhyCnt & "
                                             WHERE
                                                  ISNULL(Chck,
                                                  'FALSE') <> 'TRUE'
                                                  AND EXISTS ( SELECT
                                                  1
                                                  FROM
                                                  dbo.pos_TransferDetailsPOS
                                                  WHERE
                                                  Header_recid = @id
                                                  AND Trans_RollDet = Stkid )"

                    cmSql.ExecuteNonQuery()
                End Using
            End Using

            strsql = "select * from " & tbPhyCntView & " order by 3"
            daDetails.SelectCommand.CommandText = strsql
            dsDetails.Clear()
            daDetails.Fill(dsDetails)
            vbCnt = dsDetails.Rows.Count

            If vbCnt > 0 Then
                btnCheck.Enabled = True
                btnUnCheck.Enabled = True
                btnSave.Enabled = True
                btnClear.Enabled = True
                lblPosition.Text = tdbDetails.Row + 1 & "/" & tdbDetails.Splits(0).Rows.Count
                txtItmNo.Text = dsDetails.Compute("count([Chck])", "[Chck]=True").ToString

                MessageBox.Show("All Items Imported From Transfer", Me.Text, MessageBoxButtons.OK, MessageBoxIcon.Information)
            End If

        Catch ex As Exception
            Throw ex
        End Try
    End Sub


End Class
