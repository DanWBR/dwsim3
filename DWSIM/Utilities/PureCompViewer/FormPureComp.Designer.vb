<Global.Microsoft.VisualBasic.CompilerServices.DesignerGenerated()> _
Partial Class FormPureComp
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
        Dim resources As System.ComponentModel.ComponentResourceManager = New System.ComponentModel.ComponentResourceManager(GetType(FormPureComp))
        Dim DataGridViewCellStyle1 As System.Windows.Forms.DataGridViewCellStyle = New System.Windows.Forms.DataGridViewCellStyle()
        Dim DataGridViewCellStyle2 As System.Windows.Forms.DataGridViewCellStyle = New System.Windows.Forms.DataGridViewCellStyle()
        Dim DataGridViewCellStyle3 As System.Windows.Forms.DataGridViewCellStyle = New System.Windows.Forms.DataGridViewCellStyle()
        Me.GroupBox1 = New System.Windows.Forms.GroupBox()
        Me.Button1 = New System.Windows.Forms.Button()
        Me.chkEnableEdit = New System.Windows.Forms.CheckBox()
        Me.ComboBox1 = New System.Windows.Forms.ComboBox()
        Me.Label1 = New System.Windows.Forms.Label()
        Me.FaTabStrip1 = New FarsiLibrary.Win.FATabStrip()
        Me.FaTabStripItem1 = New FarsiLibrary.Win.FATabStripItem()
        Me.GroupBox2 = New System.Windows.Forms.GroupBox()
        Me.GridProps = New System.Windows.Forms.DataGridView()
        Me.Column1 = New System.Windows.Forms.DataGridViewTextBoxColumn()
        Me.Column2 = New System.Windows.Forms.DataGridViewTextBoxColumn()
        Me.Column3 = New System.Windows.Forms.DataGridViewTextBoxColumn()
        Me.FaTabStripItem6 = New FarsiLibrary.Win.FATabStripItem()
        Me.GroupBox3 = New System.Windows.Forms.GroupBox()
        Me.pbRender = New System.Windows.Forms.PictureBox()
        Me.GroupBox5 = New System.Windows.Forms.GroupBox()
        Me.tbInChI = New System.Windows.Forms.TextBox()
        Me.Label7 = New System.Windows.Forms.Label()
        Me.tbSMILES = New System.Windows.Forms.TextBox()
        Me.Label6 = New System.Windows.Forms.Label()
        Me.tbMODFAC = New System.Windows.Forms.TextBox()
        Me.Label5 = New System.Windows.Forms.Label()
        Me.tbUNIFAC = New System.Windows.Forms.TextBox()
        Me.Label4 = New System.Windows.Forms.Label()
        Me.tbFormula = New System.Windows.Forms.TextBox()
        Me.Label2 = New System.Windows.Forms.Label()
        Me.FaTabStripItem2 = New FarsiLibrary.Win.FATabStripItem()
        Me.GraphCp = New ZedGraph.ZedGraphControl()
        Me.FaTabStripItem3 = New FarsiLibrary.Win.FATabStripItem()
        Me.GraphPvap = New ZedGraph.ZedGraphControl()
        Me.FaTabStripItem4 = New FarsiLibrary.Win.FATabStripItem()
        Me.GraphVisc = New ZedGraph.ZedGraphControl()
        Me.FaTabStripItem5 = New FarsiLibrary.Win.FATabStripItem()
        Me.GraphDHVAP = New ZedGraph.ZedGraphControl()
        Me.GroupBox1.SuspendLayout()
        CType(Me.FaTabStrip1, System.ComponentModel.ISupportInitialize).BeginInit()
        Me.FaTabStrip1.SuspendLayout()
        Me.FaTabStripItem1.SuspendLayout()
        Me.GroupBox2.SuspendLayout()
        CType(Me.GridProps, System.ComponentModel.ISupportInitialize).BeginInit()
        Me.FaTabStripItem6.SuspendLayout()
        Me.GroupBox3.SuspendLayout()
        CType(Me.pbRender, System.ComponentModel.ISupportInitialize).BeginInit()
        Me.GroupBox5.SuspendLayout()
        Me.FaTabStripItem2.SuspendLayout()
        Me.FaTabStripItem3.SuspendLayout()
        Me.FaTabStripItem4.SuspendLayout()
        Me.FaTabStripItem5.SuspendLayout()
        Me.SuspendLayout()
        '
        'GroupBox1
        '
        resources.ApplyResources(Me.GroupBox1, "GroupBox1")
        Me.GroupBox1.Controls.Add(Me.Button1)
        Me.GroupBox1.Controls.Add(Me.chkEnableEdit)
        Me.GroupBox1.Controls.Add(Me.ComboBox1)
        Me.GroupBox1.Controls.Add(Me.Label1)
        Me.GroupBox1.Controls.Add(Me.FaTabStrip1)
        Me.GroupBox1.Name = "GroupBox1"
        Me.GroupBox1.TabStop = False
        '
        'Button1
        '
        resources.ApplyResources(Me.Button1, "Button1")
        Me.Button1.ImageKey = Global.DWSIM.My.Resources.DWSIM.String1
        Me.Button1.Name = "Button1"
        Me.Button1.UseVisualStyleBackColor = True
        '
        'chkEnableEdit
        '
        resources.ApplyResources(Me.chkEnableEdit, "chkEnableEdit")
        Me.chkEnableEdit.ImageKey = Global.DWSIM.My.Resources.DWSIM.String1
        Me.chkEnableEdit.Name = "chkEnableEdit"
        Me.chkEnableEdit.UseVisualStyleBackColor = True
        '
        'ComboBox1
        '
        resources.ApplyResources(Me.ComboBox1, "ComboBox1")
        Me.ComboBox1.DropDownStyle = System.Windows.Forms.ComboBoxStyle.DropDownList
        Me.ComboBox1.DropDownWidth = 300
        Me.ComboBox1.Name = "ComboBox1"
        '
        'Label1
        '
        resources.ApplyResources(Me.Label1, "Label1")
        Me.Label1.ImageKey = Global.DWSIM.My.Resources.DWSIM.String1
        Me.Label1.Name = "Label1"
        '
        'FaTabStrip1
        '
        resources.ApplyResources(Me.FaTabStrip1, "FaTabStrip1")
        Me.FaTabStrip1.AlwaysShowClose = False
        Me.FaTabStrip1.Items.AddRange(New FarsiLibrary.Win.FATabStripItem() {Me.FaTabStripItem1, Me.FaTabStripItem6, Me.FaTabStripItem2, Me.FaTabStripItem3, Me.FaTabStripItem4, Me.FaTabStripItem5})
        Me.FaTabStrip1.Name = "FaTabStrip1"
        Me.FaTabStrip1.SelectedItem = Me.FaTabStripItem1
        '
        'FaTabStripItem1
        '
        resources.ApplyResources(Me.FaTabStripItem1, "FaTabStripItem1")
        Me.FaTabStripItem1.CanClose = False
        Me.FaTabStripItem1.Controls.Add(Me.GroupBox2)
        Me.FaTabStripItem1.IsDrawn = True
        Me.FaTabStripItem1.Name = "FaTabStripItem1"
        Me.FaTabStripItem1.Selected = True
        '
        'GroupBox2
        '
        resources.ApplyResources(Me.GroupBox2, "GroupBox2")
        Me.GroupBox2.Controls.Add(Me.GridProps)
        Me.GroupBox2.Name = "GroupBox2"
        Me.GroupBox2.TabStop = False
        '
        'GridProps
        '
        resources.ApplyResources(Me.GridProps, "GridProps")
        Me.GridProps.AllowUserToAddRows = False
        Me.GridProps.AllowUserToDeleteRows = False
        Me.GridProps.AllowUserToResizeColumns = False
        Me.GridProps.AllowUserToResizeRows = False
        Me.GridProps.AutoSizeColumnsMode = System.Windows.Forms.DataGridViewAutoSizeColumnsMode.Fill
        Me.GridProps.ColumnHeadersHeightSizeMode = System.Windows.Forms.DataGridViewColumnHeadersHeightSizeMode.AutoSize
        Me.GridProps.Columns.AddRange(New System.Windows.Forms.DataGridViewColumn() {Me.Column1, Me.Column2, Me.Column3})
        Me.GridProps.Name = "GridProps"
        Me.GridProps.RowHeadersVisible = False
        Me.GridProps.SelectionMode = System.Windows.Forms.DataGridViewSelectionMode.CellSelect
        '
        'Column1
        '
        Me.Column1.AutoSizeMode = System.Windows.Forms.DataGridViewAutoSizeColumnMode.Fill
        DataGridViewCellStyle1.Alignment = System.Windows.Forms.DataGridViewContentAlignment.MiddleLeft
        DataGridViewCellStyle1.BackColor = System.Drawing.Color.LightGray
        Me.Column1.DefaultCellStyle = DataGridViewCellStyle1
        Me.Column1.FillWeight = 60.0!
        resources.ApplyResources(Me.Column1, "Column1")
        Me.Column1.Name = "Column1"
        Me.Column1.ReadOnly = True
        Me.Column1.ToolTipText = Global.DWSIM.My.Resources.DWSIM.String1
        '
        'Column2
        '
        Me.Column2.AutoSizeMode = System.Windows.Forms.DataGridViewAutoSizeColumnMode.Fill
        DataGridViewCellStyle2.Alignment = System.Windows.Forms.DataGridViewContentAlignment.MiddleRight
        Me.Column2.DefaultCellStyle = DataGridViewCellStyle2
        Me.Column2.FillWeight = 20.0!
        resources.ApplyResources(Me.Column2, "Column2")
        Me.Column2.Name = "Column2"
        Me.Column2.ReadOnly = True
        Me.Column2.ToolTipText = Global.DWSIM.My.Resources.DWSIM.String1
        '
        'Column3
        '
        Me.Column3.AutoSizeMode = System.Windows.Forms.DataGridViewAutoSizeColumnMode.Fill
        DataGridViewCellStyle3.Alignment = System.Windows.Forms.DataGridViewContentAlignment.MiddleLeft
        DataGridViewCellStyle3.BackColor = System.Drawing.Color.LightGray
        Me.Column3.DefaultCellStyle = DataGridViewCellStyle3
        Me.Column3.FillWeight = 20.0!
        resources.ApplyResources(Me.Column3, "Column3")
        Me.Column3.Name = "Column3"
        Me.Column3.ReadOnly = True
        Me.Column3.ToolTipText = Global.DWSIM.My.Resources.DWSIM.String1
        '
        'FaTabStripItem6
        '
        resources.ApplyResources(Me.FaTabStripItem6, "FaTabStripItem6")
        Me.FaTabStripItem6.CanClose = False
        Me.FaTabStripItem6.Controls.Add(Me.GroupBox3)
        Me.FaTabStripItem6.Controls.Add(Me.GroupBox5)
        Me.FaTabStripItem6.IsDrawn = True
        Me.FaTabStripItem6.Name = "FaTabStripItem6"
        '
        'GroupBox3
        '
        resources.ApplyResources(Me.GroupBox3, "GroupBox3")
        Me.GroupBox3.Controls.Add(Me.pbRender)
        Me.GroupBox3.Name = "GroupBox3"
        Me.GroupBox3.TabStop = False
        '
        'pbRender
        '
        resources.ApplyResources(Me.pbRender, "pbRender")
        Me.pbRender.Name = "pbRender"
        Me.pbRender.TabStop = False
        '
        'GroupBox5
        '
        resources.ApplyResources(Me.GroupBox5, "GroupBox5")
        Me.GroupBox5.Controls.Add(Me.tbInChI)
        Me.GroupBox5.Controls.Add(Me.Label7)
        Me.GroupBox5.Controls.Add(Me.tbSMILES)
        Me.GroupBox5.Controls.Add(Me.Label6)
        Me.GroupBox5.Controls.Add(Me.tbMODFAC)
        Me.GroupBox5.Controls.Add(Me.Label5)
        Me.GroupBox5.Controls.Add(Me.tbUNIFAC)
        Me.GroupBox5.Controls.Add(Me.Label4)
        Me.GroupBox5.Controls.Add(Me.tbFormula)
        Me.GroupBox5.Controls.Add(Me.Label2)
        Me.GroupBox5.Name = "GroupBox5"
        Me.GroupBox5.TabStop = False
        '
        'tbInChI
        '
        resources.ApplyResources(Me.tbInChI, "tbInChI")
        Me.tbInChI.Name = "tbInChI"
        Me.tbInChI.ReadOnly = True
        '
        'Label7
        '
        resources.ApplyResources(Me.Label7, "Label7")
        Me.Label7.ImageKey = Global.DWSIM.My.Resources.DWSIM.String1
        Me.Label7.Name = "Label7"
        '
        'tbSMILES
        '
        resources.ApplyResources(Me.tbSMILES, "tbSMILES")
        Me.tbSMILES.Name = "tbSMILES"
        Me.tbSMILES.ReadOnly = True
        '
        'Label6
        '
        resources.ApplyResources(Me.Label6, "Label6")
        Me.Label6.ImageKey = Global.DWSIM.My.Resources.DWSIM.String1
        Me.Label6.Name = "Label6"
        '
        'tbMODFAC
        '
        resources.ApplyResources(Me.tbMODFAC, "tbMODFAC")
        Me.tbMODFAC.Name = "tbMODFAC"
        Me.tbMODFAC.ReadOnly = True
        '
        'Label5
        '
        resources.ApplyResources(Me.Label5, "Label5")
        Me.Label5.ImageKey = Global.DWSIM.My.Resources.DWSIM.String1
        Me.Label5.Name = "Label5"
        '
        'tbUNIFAC
        '
        resources.ApplyResources(Me.tbUNIFAC, "tbUNIFAC")
        Me.tbUNIFAC.Name = "tbUNIFAC"
        Me.tbUNIFAC.ReadOnly = True
        '
        'Label4
        '
        resources.ApplyResources(Me.Label4, "Label4")
        Me.Label4.ImageKey = Global.DWSIM.My.Resources.DWSIM.String1
        Me.Label4.Name = "Label4"
        '
        'tbFormula
        '
        resources.ApplyResources(Me.tbFormula, "tbFormula")
        Me.tbFormula.Name = "tbFormula"
        Me.tbFormula.ReadOnly = True
        '
        'Label2
        '
        resources.ApplyResources(Me.Label2, "Label2")
        Me.Label2.ImageKey = Global.DWSIM.My.Resources.DWSIM.String1
        Me.Label2.Name = "Label2"
        '
        'FaTabStripItem2
        '
        resources.ApplyResources(Me.FaTabStripItem2, "FaTabStripItem2")
        Me.FaTabStripItem2.CanClose = False
        Me.FaTabStripItem2.Controls.Add(Me.GraphCp)
        Me.FaTabStripItem2.IsDrawn = True
        Me.FaTabStripItem2.Name = "FaTabStripItem2"
        '
        'GraphCp
        '
        resources.ApplyResources(Me.GraphCp, "GraphCp")
        Me.GraphCp.BorderStyle = System.Windows.Forms.BorderStyle.Fixed3D
        Me.GraphCp.IsAntiAlias = True
        Me.GraphCp.IsAutoScrollRange = True
        Me.GraphCp.IsEnableSelection = True
        Me.GraphCp.IsZoomOnMouseCenter = True
        Me.GraphCp.Name = "GraphCp"
        Me.GraphCp.ScrollGrace = 0.0R
        Me.GraphCp.ScrollMaxX = 0.0R
        Me.GraphCp.ScrollMaxY = 0.0R
        Me.GraphCp.ScrollMaxY2 = 0.0R
        Me.GraphCp.ScrollMinX = 0.0R
        Me.GraphCp.ScrollMinY = 0.0R
        Me.GraphCp.ScrollMinY2 = 0.0R
        '
        'FaTabStripItem3
        '
        resources.ApplyResources(Me.FaTabStripItem3, "FaTabStripItem3")
        Me.FaTabStripItem3.CanClose = False
        Me.FaTabStripItem3.Controls.Add(Me.GraphPvap)
        Me.FaTabStripItem3.IsDrawn = True
        Me.FaTabStripItem3.Name = "FaTabStripItem3"
        '
        'GraphPvap
        '
        resources.ApplyResources(Me.GraphPvap, "GraphPvap")
        Me.GraphPvap.BorderStyle = System.Windows.Forms.BorderStyle.Fixed3D
        Me.GraphPvap.IsAntiAlias = True
        Me.GraphPvap.IsAutoScrollRange = True
        Me.GraphPvap.Name = "GraphPvap"
        Me.GraphPvap.ScrollGrace = 0.0R
        Me.GraphPvap.ScrollMaxX = 0.0R
        Me.GraphPvap.ScrollMaxY = 0.0R
        Me.GraphPvap.ScrollMaxY2 = 0.0R
        Me.GraphPvap.ScrollMinX = 0.0R
        Me.GraphPvap.ScrollMinY = 0.0R
        Me.GraphPvap.ScrollMinY2 = 0.0R
        '
        'FaTabStripItem4
        '
        resources.ApplyResources(Me.FaTabStripItem4, "FaTabStripItem4")
        Me.FaTabStripItem4.CanClose = False
        Me.FaTabStripItem4.Controls.Add(Me.GraphVisc)
        Me.FaTabStripItem4.IsDrawn = True
        Me.FaTabStripItem4.Name = "FaTabStripItem4"
        '
        'GraphVisc
        '
        resources.ApplyResources(Me.GraphVisc, "GraphVisc")
        Me.GraphVisc.BorderStyle = System.Windows.Forms.BorderStyle.Fixed3D
        Me.GraphVisc.IsAntiAlias = True
        Me.GraphVisc.IsAutoScrollRange = True
        Me.GraphVisc.Name = "GraphVisc"
        Me.GraphVisc.ScrollGrace = 0.0R
        Me.GraphVisc.ScrollMaxX = 0.0R
        Me.GraphVisc.ScrollMaxY = 0.0R
        Me.GraphVisc.ScrollMaxY2 = 0.0R
        Me.GraphVisc.ScrollMinX = 0.0R
        Me.GraphVisc.ScrollMinY = 0.0R
        Me.GraphVisc.ScrollMinY2 = 0.0R
        '
        'FaTabStripItem5
        '
        resources.ApplyResources(Me.FaTabStripItem5, "FaTabStripItem5")
        Me.FaTabStripItem5.CanClose = False
        Me.FaTabStripItem5.Controls.Add(Me.GraphDHVAP)
        Me.FaTabStripItem5.IsDrawn = True
        Me.FaTabStripItem5.Name = "FaTabStripItem5"
        '
        'GraphDHVAP
        '
        resources.ApplyResources(Me.GraphDHVAP, "GraphDHVAP")
        Me.GraphDHVAP.BorderStyle = System.Windows.Forms.BorderStyle.Fixed3D
        Me.GraphDHVAP.IsAntiAlias = True
        Me.GraphDHVAP.IsAutoScrollRange = True
        Me.GraphDHVAP.Name = "GraphDHVAP"
        Me.GraphDHVAP.ScrollGrace = 0.0R
        Me.GraphDHVAP.ScrollMaxX = 0.0R
        Me.GraphDHVAP.ScrollMaxY = 0.0R
        Me.GraphDHVAP.ScrollMaxY2 = 0.0R
        Me.GraphDHVAP.ScrollMinX = 0.0R
        Me.GraphDHVAP.ScrollMinY = 0.0R
        Me.GraphDHVAP.ScrollMinY2 = 0.0R
        '
        'FormPureComp
        '
        resources.ApplyResources(Me, "$this")
        Me.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font
        Me.Controls.Add(Me.GroupBox1)
        Me.FormBorderStyle = System.Windows.Forms.FormBorderStyle.FixedToolWindow
        Me.Name = "FormPureComp"
        Me.ShowInTaskbar = False
        Me.GroupBox1.ResumeLayout(False)
        Me.GroupBox1.PerformLayout()
        CType(Me.FaTabStrip1, System.ComponentModel.ISupportInitialize).EndInit()
        Me.FaTabStrip1.ResumeLayout(False)
        Me.FaTabStripItem1.ResumeLayout(False)
        Me.GroupBox2.ResumeLayout(False)
        CType(Me.GridProps, System.ComponentModel.ISupportInitialize).EndInit()
        Me.FaTabStripItem6.ResumeLayout(False)
        Me.GroupBox3.ResumeLayout(False)
        CType(Me.pbRender, System.ComponentModel.ISupportInitialize).EndInit()
        Me.GroupBox5.ResumeLayout(False)
        Me.GroupBox5.PerformLayout()
        Me.FaTabStripItem2.ResumeLayout(False)
        Me.FaTabStripItem3.ResumeLayout(False)
        Me.FaTabStripItem4.ResumeLayout(False)
        Me.FaTabStripItem5.ResumeLayout(False)
        Me.ResumeLayout(False)

    End Sub
    Public WithEvents GroupBox1 As System.Windows.Forms.GroupBox
    Public WithEvents ComboBox1 As System.Windows.Forms.ComboBox
    Public WithEvents Label1 As System.Windows.Forms.Label
    Public WithEvents FaTabStrip1 As FarsiLibrary.Win.FATabStrip
    Public WithEvents FaTabStripItem1 As FarsiLibrary.Win.FATabStripItem
    Public WithEvents FaTabStripItem2 As FarsiLibrary.Win.FATabStripItem
    Public WithEvents FaTabStripItem3 As FarsiLibrary.Win.FATabStripItem
    Public WithEvents FaTabStripItem4 As FarsiLibrary.Win.FATabStripItem
    Public WithEvents GroupBox2 As System.Windows.Forms.GroupBox
    Public WithEvents GridProps As System.Windows.Forms.DataGridView
    Public WithEvents GraphCp As ZedGraph.ZedGraphControl
    Public WithEvents GraphPvap As ZedGraph.ZedGraphControl
    Public WithEvents GraphVisc As ZedGraph.ZedGraphControl
    Public WithEvents FaTabStripItem5 As FarsiLibrary.Win.FATabStripItem
    Public WithEvents GraphDHVAP As ZedGraph.ZedGraphControl
    Friend WithEvents chkEnableEdit As System.Windows.Forms.CheckBox
    Friend WithEvents Button1 As System.Windows.Forms.Button
    Friend WithEvents FaTabStripItem6 As FarsiLibrary.Win.FATabStripItem
    Friend WithEvents GroupBox5 As System.Windows.Forms.GroupBox
    Friend WithEvents GroupBox3 As System.Windows.Forms.GroupBox
    Friend WithEvents pbRender As System.Windows.Forms.PictureBox
    Friend WithEvents tbInChI As System.Windows.Forms.TextBox
    Friend WithEvents Label7 As System.Windows.Forms.Label
    Friend WithEvents tbSMILES As System.Windows.Forms.TextBox
    Friend WithEvents Label6 As System.Windows.Forms.Label
    Friend WithEvents tbMODFAC As System.Windows.Forms.TextBox
    Friend WithEvents Label5 As System.Windows.Forms.Label
    Friend WithEvents tbUNIFAC As System.Windows.Forms.TextBox
    Friend WithEvents Label4 As System.Windows.Forms.Label
    Friend WithEvents tbFormula As System.Windows.Forms.TextBox
    Friend WithEvents Label2 As System.Windows.Forms.Label
    Friend WithEvents Column1 As System.Windows.Forms.DataGridViewTextBoxColumn
    Friend WithEvents Column2 As System.Windows.Forms.DataGridViewTextBoxColumn
    Friend WithEvents Column3 As System.Windows.Forms.DataGridViewTextBoxColumn
End Class
