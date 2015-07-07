<Global.Microsoft.VisualBasic.CompilerServices.DesignerGenerated()> _
Partial Class ScriptEditorForm
    Inherits System.Windows.Forms.Form

    'Form overrides dispose to clean up the component list.
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
        Me.components = New System.ComponentModel.Container()
        Dim resources As System.ComponentModel.ComponentResourceManager = New System.ComponentModel.ComponentResourceManager(GetType(ScriptEditorForm))
        Me.FaTabStrip2 = New FarsiLibrary.Win.FATabStrip()
        Me.FaTabStripItem3 = New FarsiLibrary.Win.FATabStripItem()
        Me.textBoxTooltip = New System.Windows.Forms.TextBox()
        Me.listBoxAutoComplete = New GListBox
        Me.imageList1 = New System.Windows.Forms.ImageList(Me.components)
        Me.txtScript = New Alsing.Windows.Forms.SyntaxBoxControl()
        Me.FaTabStripItem4 = New FarsiLibrary.Win.FATabStripItem()
        Me.ListBox1 = New System.Windows.Forms.ListBox()
        Me.ToolStrip1 = New System.Windows.Forms.ToolStrip()
        Me.OpenToolStripButton = New System.Windows.Forms.ToolStripButton()
        Me.SaveToolStripButton = New System.Windows.Forms.ToolStripButton()
        Me.PrintToolStripButton = New System.Windows.Forms.ToolStripButton()
        Me.toolStripSeparator = New System.Windows.Forms.ToolStripSeparator()
        Me.CutToolStripButton = New System.Windows.Forms.ToolStripButton()
        Me.CopyToolStripButton = New System.Windows.Forms.ToolStripButton()
        Me.PasteToolStripButton = New System.Windows.Forms.ToolStripButton()
        Me.toolStripSeparator1 = New System.Windows.Forms.ToolStripSeparator()
        Me.tscb1 = New System.Windows.Forms.ToolStripComboBox()
        Me.tscb2 = New System.Windows.Forms.ToolStripComboBox()
        Me.ToolStripSeparator2 = New System.Windows.Forms.ToolStripSeparator()
        Me.ToolStripButton2 = New System.Windows.Forms.ToolStripButton()
        Me.ToolStripButton3 = New System.Windows.Forms.ToolStripButton()
        Me.ToolStripButton4 = New System.Windows.Forms.ToolStripButton()
        Me.ToolStripButton5 = New System.Windows.Forms.ToolStripButton()
        Me.ofd1 = New System.Windows.Forms.OpenFileDialog()
        Me.sfd1 = New System.Windows.Forms.SaveFileDialog()
        Me.ofd2 = New System.Windows.Forms.OpenFileDialog()
        Me.pd1 = New System.Windows.Forms.PrintDialog()
        Me.treeViewItems = New System.Windows.Forms.TreeView()
        CType(Me.FaTabStrip2, System.ComponentModel.ISupportInitialize).BeginInit()
        Me.FaTabStrip2.SuspendLayout()
        Me.FaTabStripItem3.SuspendLayout()
        Me.FaTabStripItem4.SuspendLayout()
        Me.ToolStrip1.SuspendLayout()
        Me.SuspendLayout()
        '
        'FaTabStrip2
        '
        resources.ApplyResources(Me.FaTabStrip2, "FaTabStrip2")
        Me.FaTabStrip2.AlwaysShowClose = False
        Me.FaTabStrip2.AlwaysShowMenuGlyph = False
        Me.FaTabStrip2.Items.AddRange(New FarsiLibrary.Win.FATabStripItem() {Me.FaTabStripItem3, Me.FaTabStripItem4})
        Me.FaTabStrip2.Name = "FaTabStrip2"
        Me.FaTabStrip2.SelectedItem = Me.FaTabStripItem3
        '
        'FaTabStripItem3
        '
        resources.ApplyResources(Me.FaTabStripItem3, "FaTabStripItem3")
        Me.FaTabStripItem3.CanClose = False
        Me.FaTabStripItem3.Controls.Add(Me.textBoxTooltip)
        Me.FaTabStripItem3.Controls.Add(Me.listBoxAutoComplete)
        Me.FaTabStripItem3.Controls.Add(Me.txtScript)
        Me.FaTabStripItem3.IsDrawn = True
        Me.FaTabStripItem3.Name = "FaTabStripItem3"
        '
        'textBoxTooltip
        '
        resources.ApplyResources(Me.textBoxTooltip, "textBoxTooltip")
        Me.textBoxTooltip.BackColor = System.Drawing.Color.FromArgb(CType(CType(255, Byte), Integer), CType(CType(255, Byte), Integer), CType(CType(225, Byte), Integer))
        Me.textBoxTooltip.BorderStyle = System.Windows.Forms.BorderStyle.FixedSingle
        Me.textBoxTooltip.Name = "textBoxTooltip"
        Me.textBoxTooltip.ReadOnly = True
        '
        'listBoxAutoComplete
        '
        resources.ApplyResources(Me.listBoxAutoComplete, "listBoxAutoComplete")
        Me.listBoxAutoComplete.BorderStyle = System.Windows.Forms.BorderStyle.FixedSingle
        Me.listBoxAutoComplete.DrawMode = System.Windows.Forms.DrawMode.OwnerDrawFixed
        Me.listBoxAutoComplete.FormattingEnabled = True
        Me.listBoxAutoComplete.ImageList = Me.imageList1
        Me.listBoxAutoComplete.Name = "listBoxAutoComplete"
        '
        'imageList1
        '
        Me.imageList1.ImageStream = CType(resources.GetObject("imageList1.ImageStream"), System.Windows.Forms.ImageListStreamer)
        Me.imageList1.TransparentColor = System.Drawing.Color.Lime
        Me.imageList1.Images.SetKeyName(0, "")
        Me.imageList1.Images.SetKeyName(1, "")
        Me.imageList1.Images.SetKeyName(2, "")
        Me.imageList1.Images.SetKeyName(3, "")
        Me.imageList1.Images.SetKeyName(4, "")
        '
        'txtScript
        '
        resources.ApplyResources(Me.txtScript, "txtScript")
        Me.txtScript.ActiveView = Alsing.Windows.Forms.ActiveView.BottomRight
        Me.txtScript.AllowBreakPoints = False
        Me.txtScript.AutoListPosition = Nothing
        Me.txtScript.AutoListSelectedText = "a123"
        Me.txtScript.AutoListVisible = False
        Me.txtScript.BackColor = System.Drawing.Color.White
        Me.txtScript.BorderStyle = Alsing.Windows.Forms.BorderStyle.None
        Me.txtScript.CopyAsRTF = True
        Me.txtScript.FontName = "Courier new"
        Me.txtScript.HighLightActiveLine = True
        Me.txtScript.InfoTipCount = 1
        Me.txtScript.InfoTipPosition = Nothing
        Me.txtScript.InfoTipSelectedIndex = 1
        Me.txtScript.InfoTipVisible = False
        Me.txtScript.LockCursorUpdate = False
        Me.txtScript.Name = "txtScript"
        Me.txtScript.ParseOnPaste = True
        Me.txtScript.ShowGutterMargin = False
        Me.txtScript.ShowScopeIndicator = False
        Me.txtScript.ShowTabGuides = True
        Me.txtScript.SmoothScroll = True
        Me.txtScript.SplitviewH = -4
        Me.txtScript.SplitviewV = -4
        Me.txtScript.TabGuideColor = System.Drawing.Color.FromArgb(CType(CType(244, Byte), Integer), CType(CType(243, Byte), Integer), CType(CType(234, Byte), Integer))
        Me.txtScript.WhitespaceColor = System.Drawing.SystemColors.ControlDark
        '
        'FaTabStripItem4
        '
        resources.ApplyResources(Me.FaTabStripItem4, "FaTabStripItem4")
        Me.FaTabStripItem4.CanClose = False
        Me.FaTabStripItem4.Controls.Add(Me.ListBox1)
        Me.FaTabStripItem4.IsDrawn = True
        Me.FaTabStripItem4.Name = "FaTabStripItem4"
        Me.FaTabStripItem4.Selected = True
        '
        'ListBox1
        '
        resources.ApplyResources(Me.ListBox1, "ListBox1")
        Me.ListBox1.BorderStyle = System.Windows.Forms.BorderStyle.None
        Me.ListBox1.FormattingEnabled = True
        Me.ListBox1.Name = "ListBox1"
        Me.ListBox1.SelectionMode = System.Windows.Forms.SelectionMode.MultiExtended
        '
        'ToolStrip1
        '
        resources.ApplyResources(Me.ToolStrip1, "ToolStrip1")
        Me.ToolStrip1.Items.AddRange(New System.Windows.Forms.ToolStripItem() {Me.OpenToolStripButton, Me.SaveToolStripButton, Me.PrintToolStripButton, Me.toolStripSeparator, Me.CutToolStripButton, Me.CopyToolStripButton, Me.PasteToolStripButton, Me.toolStripSeparator1, Me.tscb1, Me.tscb2, Me.ToolStripSeparator2, Me.ToolStripButton2, Me.ToolStripButton3, Me.ToolStripButton4, Me.ToolStripButton5})
        Me.ToolStrip1.Name = "ToolStrip1"
        '
        'OpenToolStripButton
        '
        resources.ApplyResources(Me.OpenToolStripButton, "OpenToolStripButton")
        Me.OpenToolStripButton.DisplayStyle = System.Windows.Forms.ToolStripItemDisplayStyle.Image
        Me.OpenToolStripButton.Name = "OpenToolStripButton"
        '
        'SaveToolStripButton
        '
        resources.ApplyResources(Me.SaveToolStripButton, "SaveToolStripButton")
        Me.SaveToolStripButton.DisplayStyle = System.Windows.Forms.ToolStripItemDisplayStyle.Image
        Me.SaveToolStripButton.Name = "SaveToolStripButton"
        '
        'PrintToolStripButton
        '
        resources.ApplyResources(Me.PrintToolStripButton, "PrintToolStripButton")
        Me.PrintToolStripButton.DisplayStyle = System.Windows.Forms.ToolStripItemDisplayStyle.Image
        Me.PrintToolStripButton.Name = "PrintToolStripButton"
        '
        'toolStripSeparator
        '
        resources.ApplyResources(Me.toolStripSeparator, "toolStripSeparator")
        Me.toolStripSeparator.Name = "toolStripSeparator"
        '
        'CutToolStripButton
        '
        resources.ApplyResources(Me.CutToolStripButton, "CutToolStripButton")
        Me.CutToolStripButton.DisplayStyle = System.Windows.Forms.ToolStripItemDisplayStyle.Image
        Me.CutToolStripButton.Name = "CutToolStripButton"
        '
        'CopyToolStripButton
        '
        resources.ApplyResources(Me.CopyToolStripButton, "CopyToolStripButton")
        Me.CopyToolStripButton.DisplayStyle = System.Windows.Forms.ToolStripItemDisplayStyle.Image
        Me.CopyToolStripButton.Name = "CopyToolStripButton"
        '
        'PasteToolStripButton
        '
        resources.ApplyResources(Me.PasteToolStripButton, "PasteToolStripButton")
        Me.PasteToolStripButton.DisplayStyle = System.Windows.Forms.ToolStripItemDisplayStyle.Image
        Me.PasteToolStripButton.Name = "PasteToolStripButton"
        '
        'toolStripSeparator1
        '
        resources.ApplyResources(Me.toolStripSeparator1, "toolStripSeparator1")
        Me.toolStripSeparator1.Name = "toolStripSeparator1"
        '
        'tscb1
        '
        resources.ApplyResources(Me.tscb1, "tscb1")
        Me.tscb1.DropDownStyle = System.Windows.Forms.ComboBoxStyle.DropDownList
        Me.tscb1.Name = "tscb1"
        Me.tscb1.Sorted = True
        '
        'tscb2
        '
        resources.ApplyResources(Me.tscb2, "tscb2")
        Me.tscb2.DropDownStyle = System.Windows.Forms.ComboBoxStyle.DropDownList
        Me.tscb2.DropDownWidth = 35
        Me.tscb2.Name = "tscb2"
        '
        'ToolStripSeparator2
        '
        resources.ApplyResources(Me.ToolStripSeparator2, "ToolStripSeparator2")
        Me.ToolStripSeparator2.Name = "ToolStripSeparator2"
        '
        'ToolStripButton2
        '
        resources.ApplyResources(Me.ToolStripButton2, "ToolStripButton2")
        Me.ToolStripButton2.DisplayStyle = System.Windows.Forms.ToolStripItemDisplayStyle.Image
        Me.ToolStripButton2.Image = Global.DWSIM.My.Resources.Resources.add
        Me.ToolStripButton2.Name = "ToolStripButton2"
        '
        'ToolStripButton3
        '
        resources.ApplyResources(Me.ToolStripButton3, "ToolStripButton3")
        Me.ToolStripButton3.DisplayStyle = System.Windows.Forms.ToolStripItemDisplayStyle.Image
        Me.ToolStripButton3.Image = Global.DWSIM.My.Resources.Resources.delete
        Me.ToolStripButton3.Name = "ToolStripButton3"
        '
        'ToolStripButton4
        '
        resources.ApplyResources(Me.ToolStripButton4, "ToolStripButton4")
        Me.ToolStripButton4.Alignment = System.Windows.Forms.ToolStripItemAlignment.Right
        Me.ToolStripButton4.DisplayStyle = System.Windows.Forms.ToolStripItemDisplayStyle.Image
        Me.ToolStripButton4.Image = Global.DWSIM.My.Resources.Resources.shape_square_add
        Me.ToolStripButton4.Name = "ToolStripButton4"
        '
        'ToolStripButton5
        '
        resources.ApplyResources(Me.ToolStripButton5, "ToolStripButton5")
        Me.ToolStripButton5.Alignment = System.Windows.Forms.ToolStripItemAlignment.Right
        Me.ToolStripButton5.DisplayStyle = System.Windows.Forms.ToolStripItemDisplayStyle.Image
        Me.ToolStripButton5.Image = Global.DWSIM.My.Resources.Resources.shape_square_delete
        Me.ToolStripButton5.Name = "ToolStripButton5"
        '
        'ofd1
        '
        Me.ofd1.FileName = "OpenFileDialog1"
        resources.ApplyResources(Me.ofd1, "ofd1")
        Me.ofd1.Multiselect = True
        Me.ofd1.SupportMultiDottedExtensions = True
        '
        'sfd1
        '
        resources.ApplyResources(Me.sfd1, "sfd1")
        Me.sfd1.SupportMultiDottedExtensions = True
        '
        'ofd2
        '
        resources.ApplyResources(Me.ofd2, "ofd2")
        Me.ofd2.Multiselect = True
        Me.ofd2.SupportMultiDottedExtensions = True
        '
        'pd1
        '
        Me.pd1.UseEXDialog = True
        '
        'treeViewItems
        '
        resources.ApplyResources(Me.treeViewItems, "treeViewItems")
        Me.treeViewItems.LineColor = System.Drawing.Color.Empty
        Me.treeViewItems.Name = "treeViewItems"
        Me.treeViewItems.PathSeparator = "."
        '
        'ScriptEditorForm
        '
        resources.ApplyResources(Me, "$this")
        Me.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font
        Me.Controls.Add(Me.FaTabStrip2)
        Me.Controls.Add(Me.ToolStrip1)
        Me.FormBorderStyle = System.Windows.Forms.FormBorderStyle.SizableToolWindow
        Me.Name = "ScriptEditorForm"
        Me.ShowIcon = False
        Me.ShowInTaskbar = False
        CType(Me.FaTabStrip2, System.ComponentModel.ISupportInitialize).EndInit()
        Me.FaTabStrip2.ResumeLayout(False)
        Me.FaTabStripItem3.ResumeLayout(False)
        Me.FaTabStripItem3.PerformLayout()
        Me.FaTabStripItem4.ResumeLayout(False)
        Me.ToolStrip1.ResumeLayout(False)
        Me.ToolStrip1.PerformLayout()
        Me.ResumeLayout(False)
        Me.PerformLayout()

    End Sub
    Public WithEvents FaTabStrip2 As FarsiLibrary.Win.FATabStrip
    Public WithEvents FaTabStripItem3 As FarsiLibrary.Win.FATabStripItem
    Public WithEvents txtScript As Alsing.Windows.Forms.SyntaxBoxControl
    Public WithEvents FaTabStripItem4 As FarsiLibrary.Win.FATabStripItem
    Public WithEvents ListBox1 As System.Windows.Forms.ListBox
    Public WithEvents ToolStrip1 As System.Windows.Forms.ToolStrip
    Public WithEvents OpenToolStripButton As System.Windows.Forms.ToolStripButton
    Public WithEvents SaveToolStripButton As System.Windows.Forms.ToolStripButton
    Public WithEvents PrintToolStripButton As System.Windows.Forms.ToolStripButton
    Public WithEvents toolStripSeparator As System.Windows.Forms.ToolStripSeparator
    Public WithEvents CutToolStripButton As System.Windows.Forms.ToolStripButton
    Public WithEvents CopyToolStripButton As System.Windows.Forms.ToolStripButton
    Public WithEvents PasteToolStripButton As System.Windows.Forms.ToolStripButton
    Public WithEvents toolStripSeparator1 As System.Windows.Forms.ToolStripSeparator
    Public WithEvents tscb1 As System.Windows.Forms.ToolStripComboBox
    Public WithEvents tscb2 As System.Windows.Forms.ToolStripComboBox
    Public WithEvents ToolStripSeparator2 As System.Windows.Forms.ToolStripSeparator
    Public WithEvents ToolStripButton2 As System.Windows.Forms.ToolStripButton
    Public WithEvents ToolStripButton3 As System.Windows.Forms.ToolStripButton
    Public WithEvents ToolStripButton4 As System.Windows.Forms.ToolStripButton
    Public WithEvents ToolStripButton5 As System.Windows.Forms.ToolStripButton
    Public WithEvents ofd1 As System.Windows.Forms.OpenFileDialog
    Public WithEvents sfd1 As System.Windows.Forms.SaveFileDialog
    Public WithEvents ofd2 As System.Windows.Forms.OpenFileDialog
    Public WithEvents pd1 As System.Windows.Forms.PrintDialog
    Private WithEvents textBoxTooltip As System.Windows.Forms.TextBox
    Friend WithEvents listBoxAutoComplete As GListBox
    Public WithEvents treeViewItems As System.Windows.Forms.TreeView
    Public WithEvents imageList1 As System.Windows.Forms.ImageList
End Class
