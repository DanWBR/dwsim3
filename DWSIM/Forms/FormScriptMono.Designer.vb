<Global.Microsoft.VisualBasic.CompilerServices.DesignerGenerated()> Partial Class FormScriptMono
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
        Me.components = New System.ComponentModel.Container
        Dim resources As System.ComponentModel.ComponentResourceManager = New System.ComponentModel.ComponentResourceManager(GetType(FormScript))
        Me.txtOutput = New System.Windows.Forms.TextBox
        Me.ToolStrip1 = New System.Windows.Forms.ToolStrip
        Me.OpenToolStripButton = New System.Windows.Forms.ToolStripButton
        Me.SaveToolStripButton = New System.Windows.Forms.ToolStripButton
        Me.PrintToolStripButton = New System.Windows.Forms.ToolStripButton
        Me.toolStripSeparator = New System.Windows.Forms.ToolStripSeparator
        Me.CutToolStripButton = New System.Windows.Forms.ToolStripButton
        Me.CopyToolStripButton = New System.Windows.Forms.ToolStripButton
        Me.PasteToolStripButton = New System.Windows.Forms.ToolStripButton
        Me.toolStripSeparator1 = New System.Windows.Forms.ToolStripSeparator
        Me.tscb1 = New System.Windows.Forms.ToolStripComboBox
        Me.tscb2 = New System.Windows.Forms.ToolStripComboBox
        Me.ToolStripSeparator2 = New System.Windows.Forms.ToolStripSeparator
        Me.ToolStripButton2 = New System.Windows.Forms.ToolStripButton
        Me.ToolStripButton3 = New System.Windows.Forms.ToolStripButton
        Me.ToolStripButton4 = New System.Windows.Forms.ToolStripButton
        Me.ToolStripButton5 = New System.Windows.Forms.ToolStripButton
        Me.ToolStripSeparator4 = New System.Windows.Forms.ToolStripSeparator
        Me.ToolStripButton1 = New System.Windows.Forms.ToolStripButton
        Me.FaTabStripItem1 = New FarsiLibrary.Win.FATabStripItem
        Me.FaTabStripItem2 = New FarsiLibrary.Win.FATabStripItem
        Me.txtErrorList = New System.Windows.Forms.TextBox
        Me.FaTabStrip1 = New FarsiLibrary.Win.FATabStrip
        Me.ofd1 = New System.Windows.Forms.OpenFileDialog
        Me.sfd1 = New System.Windows.Forms.SaveFileDialog
        Me.FaTabStrip2 = New FarsiLibrary.Win.FATabStrip
        Me.FaTabStripItem3 = New FarsiLibrary.Win.FATabStripItem
        Me.txtScript = New System.Windows.Forms.TextBox
        Me.FaTabStripItem4 = New FarsiLibrary.Win.FATabStripItem
        Me.ListBox1 = New System.Windows.Forms.ListBox
        Me.FaTabStripItem5 = New FarsiLibrary.Win.FATabStripItem
        Me.treeViewItems = New System.Windows.Forms.TreeView
        Me.imageList1 = New System.Windows.Forms.ImageList(Me.components)
        Me.ofd2 = New System.Windows.Forms.OpenFileDialog
        Me.pd1 = New System.Windows.Forms.PrintDialog
        Me.textBoxTooltip = New System.Windows.Forms.TextBox
        Me.ToolStrip1.SuspendLayout()
        Me.FaTabStripItem1.SuspendLayout()
        Me.FaTabStripItem2.SuspendLayout()
        CType(Me.FaTabStrip1, System.ComponentModel.ISupportInitialize).BeginInit()
        Me.FaTabStrip1.SuspendLayout()
        CType(Me.FaTabStrip2, System.ComponentModel.ISupportInitialize).BeginInit()
        Me.FaTabStrip2.SuspendLayout()
        Me.FaTabStripItem3.SuspendLayout()
        Me.FaTabStripItem4.SuspendLayout()
        Me.FaTabStripItem5.SuspendLayout()
        Me.SuspendLayout()
        '
        'txtOutput
        '
        Me.txtOutput.BackColor = System.Drawing.SystemColors.Window
        resources.ApplyResources(Me.txtOutput, "txtOutput")
        Me.txtOutput.Name = "txtOutput"
        Me.txtOutput.ReadOnly = True
        '
        'ToolStrip1
        '
        Me.ToolStrip1.Items.AddRange(New System.Windows.Forms.ToolStripItem() {Me.OpenToolStripButton, Me.SaveToolStripButton, Me.PrintToolStripButton, Me.toolStripSeparator, Me.CutToolStripButton, Me.CopyToolStripButton, Me.PasteToolStripButton, Me.toolStripSeparator1, Me.tscb1, Me.tscb2, Me.ToolStripSeparator2, Me.ToolStripButton2, Me.ToolStripButton3, Me.ToolStripButton4, Me.ToolStripButton5, Me.ToolStripSeparator4, Me.ToolStripButton1})
        resources.ApplyResources(Me.ToolStrip1, "ToolStrip1")
        Me.ToolStrip1.Name = "ToolStrip1"
        '
        'OpenToolStripButton
        '
        Me.OpenToolStripButton.DisplayStyle = System.Windows.Forms.ToolStripItemDisplayStyle.Image
        resources.ApplyResources(Me.OpenToolStripButton, "OpenToolStripButton")
        Me.OpenToolStripButton.Name = "OpenToolStripButton"
        '
        'SaveToolStripButton
        '
        Me.SaveToolStripButton.DisplayStyle = System.Windows.Forms.ToolStripItemDisplayStyle.Image
        resources.ApplyResources(Me.SaveToolStripButton, "SaveToolStripButton")
        Me.SaveToolStripButton.Name = "SaveToolStripButton"
        '
        'PrintToolStripButton
        '
        Me.PrintToolStripButton.DisplayStyle = System.Windows.Forms.ToolStripItemDisplayStyle.Image
        resources.ApplyResources(Me.PrintToolStripButton, "PrintToolStripButton")
        Me.PrintToolStripButton.Name = "PrintToolStripButton"
        '
        'toolStripSeparator
        '
        Me.toolStripSeparator.Name = "toolStripSeparator"
        resources.ApplyResources(Me.toolStripSeparator, "toolStripSeparator")
        '
        'CutToolStripButton
        '
        Me.CutToolStripButton.DisplayStyle = System.Windows.Forms.ToolStripItemDisplayStyle.Image
        resources.ApplyResources(Me.CutToolStripButton, "CutToolStripButton")
        Me.CutToolStripButton.Name = "CutToolStripButton"
        '
        'CopyToolStripButton
        '
        Me.CopyToolStripButton.DisplayStyle = System.Windows.Forms.ToolStripItemDisplayStyle.Image
        resources.ApplyResources(Me.CopyToolStripButton, "CopyToolStripButton")
        Me.CopyToolStripButton.Name = "CopyToolStripButton"
        '
        'PasteToolStripButton
        '
        Me.PasteToolStripButton.DisplayStyle = System.Windows.Forms.ToolStripItemDisplayStyle.Image
        resources.ApplyResources(Me.PasteToolStripButton, "PasteToolStripButton")
        Me.PasteToolStripButton.Name = "PasteToolStripButton"
        '
        'toolStripSeparator1
        '
        Me.toolStripSeparator1.Name = "toolStripSeparator1"
        resources.ApplyResources(Me.toolStripSeparator1, "toolStripSeparator1")
        '
        'tscb1
        '
        Me.tscb1.DropDownStyle = System.Windows.Forms.ComboBoxStyle.DropDownList
        Me.tscb1.Name = "tscb1"
        resources.ApplyResources(Me.tscb1, "tscb1")
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
        Me.ToolStripSeparator2.Name = "ToolStripSeparator2"
        resources.ApplyResources(Me.ToolStripSeparator2, "ToolStripSeparator2")
        '
        'ToolStripButton2
        '
        Me.ToolStripButton2.DisplayStyle = System.Windows.Forms.ToolStripItemDisplayStyle.Image
        Me.ToolStripButton2.Image = Global.DWSIM.My.Resources.Resources.add
        resources.ApplyResources(Me.ToolStripButton2, "ToolStripButton2")
        Me.ToolStripButton2.Name = "ToolStripButton2"
        '
        'ToolStripButton3
        '
        Me.ToolStripButton3.DisplayStyle = System.Windows.Forms.ToolStripItemDisplayStyle.Image
        Me.ToolStripButton3.Image = Global.DWSIM.My.Resources.Resources.delete
        resources.ApplyResources(Me.ToolStripButton3, "ToolStripButton3")
        Me.ToolStripButton3.Name = "ToolStripButton3"
        '
        'ToolStripButton4
        '
        Me.ToolStripButton4.Alignment = System.Windows.Forms.ToolStripItemAlignment.Right
        Me.ToolStripButton4.DisplayStyle = System.Windows.Forms.ToolStripItemDisplayStyle.Image
        Me.ToolStripButton4.Image = Global.DWSIM.My.Resources.Resources.shape_square_add
        resources.ApplyResources(Me.ToolStripButton4, "ToolStripButton4")
        Me.ToolStripButton4.Name = "ToolStripButton4"
        '
        'ToolStripButton5
        '
        Me.ToolStripButton5.Alignment = System.Windows.Forms.ToolStripItemAlignment.Right
        Me.ToolStripButton5.DisplayStyle = System.Windows.Forms.ToolStripItemDisplayStyle.Image
        Me.ToolStripButton5.Image = Global.DWSIM.My.Resources.Resources.shape_square_delete
        resources.ApplyResources(Me.ToolStripButton5, "ToolStripButton5")
        Me.ToolStripButton5.Name = "ToolStripButton5"
        '
        'ToolStripSeparator4
        '
        Me.ToolStripSeparator4.Name = "ToolStripSeparator4"
        resources.ApplyResources(Me.ToolStripSeparator4, "ToolStripSeparator4")
        '
        'ToolStripButton1
        '
        Me.ToolStripButton1.DisplayStyle = System.Windows.Forms.ToolStripItemDisplayStyle.Image
        Me.ToolStripButton1.Image = Global.DWSIM.My.Resources.Resources.control_play
        resources.ApplyResources(Me.ToolStripButton1, "ToolStripButton1")
        Me.ToolStripButton1.Name = "ToolStripButton1"
        '
        'FaTabStripItem1
        '
        Me.FaTabStripItem1.CanClose = False
        Me.FaTabStripItem1.Controls.Add(Me.txtOutput)
        Me.FaTabStripItem1.IsDrawn = True
        Me.FaTabStripItem1.Name = "FaTabStripItem1"
        Me.FaTabStripItem1.Selected = True
        resources.ApplyResources(Me.FaTabStripItem1, "FaTabStripItem1")
        '
        'FaTabStripItem2
        '
        Me.FaTabStripItem2.CanClose = False
        Me.FaTabStripItem2.Controls.Add(Me.txtErrorList)
        Me.FaTabStripItem2.IsDrawn = True
        Me.FaTabStripItem2.Name = "FaTabStripItem2"
        resources.ApplyResources(Me.FaTabStripItem2, "FaTabStripItem2")
        '
        'txtErrorList
        '
        Me.txtErrorList.BackColor = System.Drawing.SystemColors.Window
        resources.ApplyResources(Me.txtErrorList, "txtErrorList")
        Me.txtErrorList.Name = "txtErrorList"
        Me.txtErrorList.ReadOnly = True
        '
        'FaTabStrip1
        '
        Me.FaTabStrip1.AlwaysShowClose = False
        Me.FaTabStrip1.AlwaysShowMenuGlyph = False
        resources.ApplyResources(Me.FaTabStrip1, "FaTabStrip1")
        Me.FaTabStrip1.Items.AddRange(New FarsiLibrary.Win.FATabStripItem() {Me.FaTabStripItem1, Me.FaTabStripItem2})
        Me.FaTabStrip1.Name = "FaTabStrip1"
        Me.FaTabStrip1.SelectedItem = Me.FaTabStripItem1
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
        'FaTabStrip2
        '
        Me.FaTabStrip2.AlwaysShowClose = False
        Me.FaTabStrip2.AlwaysShowMenuGlyph = False
        resources.ApplyResources(Me.FaTabStrip2, "FaTabStrip2")
        Me.FaTabStrip2.Items.AddRange(New FarsiLibrary.Win.FATabStripItem() {Me.FaTabStripItem3, Me.FaTabStripItem4})
        Me.FaTabStrip2.Name = "FaTabStrip2"
        Me.FaTabStrip2.SelectedItem = Me.FaTabStripItem3
        '
        'FaTabStripItem3
        '
        Me.FaTabStripItem3.CanClose = False
        Me.FaTabStripItem3.Controls.Add(Me.txtScript)
        Me.FaTabStripItem3.IsDrawn = True
        Me.FaTabStripItem3.Name = "FaTabStripItem3"
        Me.FaTabStripItem3.Selected = True
        resources.ApplyResources(Me.FaTabStripItem3, "FaTabStripItem3")
        '
        'txtScript
        '
        resources.ApplyResources(Me.txtScript, "txtScript")
        Me.txtScript.Name = "txtScript"
        '
        'FaTabStripItem4
        '
        Me.FaTabStripItem4.CanClose = False
        Me.FaTabStripItem4.Controls.Add(Me.ListBox1)
        Me.FaTabStripItem4.IsDrawn = True
        Me.FaTabStripItem4.Name = "FaTabStripItem4"
        resources.ApplyResources(Me.FaTabStripItem4, "FaTabStripItem4")
        '
        'ListBox1
        '
        Me.ListBox1.BorderStyle = System.Windows.Forms.BorderStyle.None
        resources.ApplyResources(Me.ListBox1, "ListBox1")
        Me.ListBox1.FormattingEnabled = True
        Me.ListBox1.Name = "ListBox1"
        Me.ListBox1.SelectionMode = System.Windows.Forms.SelectionMode.MultiExtended
        '
        'FaTabStripItem5
        '
        Me.FaTabStripItem5.Controls.Add(Me.treeViewItems)
        Me.FaTabStripItem5.IsDrawn = True
        Me.FaTabStripItem5.Name = "FaTabStripItem5"
        Me.FaTabStripItem5.Selected = True
        resources.ApplyResources(Me.FaTabStripItem5, "FaTabStripItem5")
        '
        'treeViewItems
        '
        resources.ApplyResources(Me.treeViewItems, "treeViewItems")
        Me.treeViewItems.LineColor = System.Drawing.Color.Empty
        Me.treeViewItems.Name = "treeViewItems"
        Me.treeViewItems.PathSeparator = "."
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
        'textBoxTooltip
        '
        Me.textBoxTooltip.BackColor = System.Drawing.Color.FromArgb(CType(CType(255, Byte), Integer), CType(CType(255, Byte), Integer), CType(CType(225, Byte), Integer))
        Me.textBoxTooltip.BorderStyle = System.Windows.Forms.BorderStyle.FixedSingle
        resources.ApplyResources(Me.textBoxTooltip, "textBoxTooltip")
        Me.textBoxTooltip.Name = "textBoxTooltip"
        Me.textBoxTooltip.ReadOnly = True
        '
        'FormScript
        '
        resources.ApplyResources(Me, "$this")
        Me.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font
        Me.Controls.Add(Me.FaTabStrip2)
        Me.Controls.Add(Me.FaTabStrip1)
        Me.Controls.Add(Me.ToolStrip1)
        Me.DoubleBuffered = True
        Me.FormBorderStyle = System.Windows.Forms.FormBorderStyle.SizableToolWindow
        Me.Name = "FormScript"
        Me.ToolStrip1.ResumeLayout(False)
        Me.ToolStrip1.PerformLayout()
        Me.FaTabStripItem1.ResumeLayout(False)
        Me.FaTabStripItem1.PerformLayout()
        Me.FaTabStripItem2.ResumeLayout(False)
        Me.FaTabStripItem2.PerformLayout()
        CType(Me.FaTabStrip1, System.ComponentModel.ISupportInitialize).EndInit()
        Me.FaTabStrip1.ResumeLayout(False)
        CType(Me.FaTabStrip2, System.ComponentModel.ISupportInitialize).EndInit()
        Me.FaTabStrip2.ResumeLayout(False)
        Me.FaTabStripItem3.ResumeLayout(False)
        Me.FaTabStripItem3.PerformLayout()
        Me.FaTabStripItem4.ResumeLayout(False)
        Me.FaTabStripItem5.ResumeLayout(False)
        Me.ResumeLayout(False)
        Me.PerformLayout()

    End Sub
    Public WithEvents txtOutput As System.Windows.Forms.TextBox
    Public WithEvents ToolStrip1 As System.Windows.Forms.ToolStrip
    Public WithEvents FaTabStripItem1 As FarsiLibrary.Win.FATabStripItem
    Public WithEvents FaTabStripItem2 As FarsiLibrary.Win.FATabStripItem
    Public WithEvents FaTabStrip1 As FarsiLibrary.Win.FATabStrip
    Public WithEvents txtErrorList As System.Windows.Forms.TextBox
    Public WithEvents OpenToolStripButton As System.Windows.Forms.ToolStripButton
    Public WithEvents SaveToolStripButton As System.Windows.Forms.ToolStripButton
    Public WithEvents PrintToolStripButton As System.Windows.Forms.ToolStripButton
    Public WithEvents toolStripSeparator As System.Windows.Forms.ToolStripSeparator
    Public WithEvents CutToolStripButton As System.Windows.Forms.ToolStripButton
    Public WithEvents CopyToolStripButton As System.Windows.Forms.ToolStripButton
    Public WithEvents PasteToolStripButton As System.Windows.Forms.ToolStripButton
    Public WithEvents toolStripSeparator1 As System.Windows.Forms.ToolStripSeparator
    Public WithEvents ToolStripSeparator2 As System.Windows.Forms.ToolStripSeparator
    Public WithEvents ToolStripButton1 As System.Windows.Forms.ToolStripButton
    Public WithEvents ofd1 As System.Windows.Forms.OpenFileDialog
    Public WithEvents sfd1 As System.Windows.Forms.SaveFileDialog
    Public WithEvents FaTabStrip2 As FarsiLibrary.Win.FATabStrip
    Public WithEvents FaTabStripItem3 As FarsiLibrary.Win.FATabStripItem
    Public WithEvents FaTabStripItem4 As FarsiLibrary.Win.FATabStripItem
    Public WithEvents ToolStripButton2 As System.Windows.Forms.ToolStripButton
    Public WithEvents ToolStripButton3 As System.Windows.Forms.ToolStripButton
    Public WithEvents ListBox1 As System.Windows.Forms.ListBox
    Public WithEvents ToolStripButton4 As System.Windows.Forms.ToolStripButton
    Public WithEvents ToolStripButton5 As System.Windows.Forms.ToolStripButton
    Public WithEvents ofd2 As System.Windows.Forms.OpenFileDialog
    Public WithEvents pd1 As System.Windows.Forms.PrintDialog
    Public WithEvents ToolStripSeparator4 As System.Windows.Forms.ToolStripSeparator
    Public WithEvents tscb1 As System.Windows.Forms.ToolStripComboBox
    Public WithEvents tscb2 As System.Windows.Forms.ToolStripComboBox
    Public WithEvents imageList1 As System.Windows.Forms.ImageList
    Public WithEvents listBoxAutoComplete As GListBox
    Public WithEvents textBoxTooltip As System.Windows.Forms.TextBox
    Public WithEvents FaTabStripItem5 As FarsiLibrary.Win.FATabStripItem
    Public WithEvents treeViewItems As System.Windows.Forms.TreeView
    Friend WithEvents txtScript As System.Windows.Forms.TextBox
End Class
