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
        Me.components = New System.ComponentModel.Container
        Dim resources As System.ComponentModel.ComponentResourceManager = New System.ComponentModel.ComponentResourceManager(GetType(FormPureComp))
        Dim DataGridViewCellStyle1 As System.Windows.Forms.DataGridViewCellStyle = New System.Windows.Forms.DataGridViewCellStyle
        Dim DataGridViewCellStyle2 As System.Windows.Forms.DataGridViewCellStyle = New System.Windows.Forms.DataGridViewCellStyle
        Dim DataGridViewCellStyle3 As System.Windows.Forms.DataGridViewCellStyle = New System.Windows.Forms.DataGridViewCellStyle
        Dim DataGridViewCellStyle4 As System.Windows.Forms.DataGridViewCellStyle = New System.Windows.Forms.DataGridViewCellStyle
        Dim DataGridViewCellStyle5 As System.Windows.Forms.DataGridViewCellStyle = New System.Windows.Forms.DataGridViewCellStyle
        Dim DataGridViewCellStyle6 As System.Windows.Forms.DataGridViewCellStyle = New System.Windows.Forms.DataGridViewCellStyle
        Dim DataGridViewCellStyle7 As System.Windows.Forms.DataGridViewCellStyle = New System.Windows.Forms.DataGridViewCellStyle
        Me.GroupBox1 = New System.Windows.Forms.GroupBox
        Me.Button1 = New System.Windows.Forms.Button
        Me.chkEnableEdit = New System.Windows.Forms.CheckBox
        Me.ComboBox1 = New System.Windows.Forms.ComboBox
        Me.Label1 = New System.Windows.Forms.Label
        Me.FaTabStrip1 = New FarsiLibrary.Win.FATabStrip
        Me.FaTabStripItem1 = New FarsiLibrary.Win.FATabStripItem
        Me.GroupBox4 = New System.Windows.Forms.GroupBox
        Me.GridMODFAC = New System.Windows.Forms.DataGridView
        Me.DataGridViewTextBoxColumn3 = New System.Windows.Forms.DataGridViewTextBoxColumn
        Me.DataGridViewTextBoxColumn4 = New System.Windows.Forms.DataGridViewTextBoxColumn
        Me.GroupBox3 = New System.Windows.Forms.GroupBox
        Me.GridUNIFAC = New System.Windows.Forms.DataGridView
        Me.DataGridViewTextBoxColumn1 = New System.Windows.Forms.DataGridViewTextBoxColumn
        Me.DataGridViewTextBoxColumn2 = New System.Windows.Forms.DataGridViewTextBoxColumn
        Me.GroupBox2 = New System.Windows.Forms.GroupBox
        Me.GridProps = New System.Windows.Forms.DataGridView
        Me.Column1 = New System.Windows.Forms.DataGridViewTextBoxColumn
        Me.Column2 = New System.Windows.Forms.DataGridViewTextBoxColumn
        Me.Column3 = New System.Windows.Forms.DataGridViewTextBoxColumn
        Me.FaTabStripItem2 = New FarsiLibrary.Win.FATabStripItem
        Me.GraphCp = New ZedGraph.ZedGraphControl
        Me.FaTabStripItem3 = New FarsiLibrary.Win.FATabStripItem
        Me.GraphPvap = New ZedGraph.ZedGraphControl
        Me.FaTabStripItem4 = New FarsiLibrary.Win.FATabStripItem
        Me.GraphVisc = New ZedGraph.ZedGraphControl
        Me.FaTabStripItem5 = New FarsiLibrary.Win.FATabStripItem
        Me.GraphDHVAP = New ZedGraph.ZedGraphControl
        Me.GroupBox1.SuspendLayout()
        CType(Me.FaTabStrip1, System.ComponentModel.ISupportInitialize).BeginInit()
        Me.FaTabStrip1.SuspendLayout()
        Me.FaTabStripItem1.SuspendLayout()
        Me.GroupBox4.SuspendLayout()
        CType(Me.GridMODFAC, System.ComponentModel.ISupportInitialize).BeginInit()
        Me.GroupBox3.SuspendLayout()
        CType(Me.GridUNIFAC, System.ComponentModel.ISupportInitialize).BeginInit()
        Me.GroupBox2.SuspendLayout()
        CType(Me.GridProps, System.ComponentModel.ISupportInitialize).BeginInit()
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
        Me.Button1.Name = "Button1"
        Me.Button1.UseVisualStyleBackColor = True
        '
        'chkEnableEdit
        '
        resources.ApplyResources(Me.chkEnableEdit, "chkEnableEdit")
        Me.chkEnableEdit.Name = "chkEnableEdit"
        Me.chkEnableEdit.UseVisualStyleBackColor = True
        '
        'ComboBox1
        '
        Me.ComboBox1.DropDownStyle = System.Windows.Forms.ComboBoxStyle.DropDownList
        Me.ComboBox1.DropDownWidth = 300
        resources.ApplyResources(Me.ComboBox1, "ComboBox1")
        Me.ComboBox1.Name = "ComboBox1"
        '
        'Label1
        '
        resources.ApplyResources(Me.Label1, "Label1")
        Me.Label1.Name = "Label1"
        '
        'FaTabStrip1
        '
        Me.FaTabStrip1.AlwaysShowClose = False
        resources.ApplyResources(Me.FaTabStrip1, "FaTabStrip1")
        Me.FaTabStrip1.Items.AddRange(New FarsiLibrary.Win.FATabStripItem() {Me.FaTabStripItem1, Me.FaTabStripItem2, Me.FaTabStripItem3, Me.FaTabStripItem4, Me.FaTabStripItem5})
        Me.FaTabStrip1.Name = "FaTabStrip1"
        Me.FaTabStrip1.SelectedItem = Me.FaTabStripItem1
        '
        'FaTabStripItem1
        '
        Me.FaTabStripItem1.CanClose = False
        Me.FaTabStripItem1.Controls.Add(Me.GroupBox4)
        Me.FaTabStripItem1.Controls.Add(Me.GroupBox3)
        Me.FaTabStripItem1.Controls.Add(Me.GroupBox2)
        Me.FaTabStripItem1.IsDrawn = True
        Me.FaTabStripItem1.Name = "FaTabStripItem1"
        Me.FaTabStripItem1.Selected = True
        resources.ApplyResources(Me.FaTabStripItem1, "FaTabStripItem1")
        '
        'GroupBox4
        '
        resources.ApplyResources(Me.GroupBox4, "GroupBox4")
        Me.GroupBox4.Controls.Add(Me.GridMODFAC)
        Me.GroupBox4.Name = "GroupBox4"
        Me.GroupBox4.TabStop = False
        '
        'GridMODFAC
        '
        Me.GridMODFAC.AllowUserToAddRows = False
        Me.GridMODFAC.AllowUserToDeleteRows = False
        Me.GridMODFAC.AllowUserToResizeColumns = False
        Me.GridMODFAC.AllowUserToResizeRows = False
        Me.GridMODFAC.AutoSizeColumnsMode = System.Windows.Forms.DataGridViewAutoSizeColumnsMode.Fill
        Me.GridMODFAC.ColumnHeadersHeightSizeMode = System.Windows.Forms.DataGridViewColumnHeadersHeightSizeMode.AutoSize
        Me.GridMODFAC.Columns.AddRange(New System.Windows.Forms.DataGridViewColumn() {Me.DataGridViewTextBoxColumn3, Me.DataGridViewTextBoxColumn4})
        resources.ApplyResources(Me.GridMODFAC, "GridMODFAC")
        Me.GridMODFAC.Name = "GridMODFAC"
        Me.GridMODFAC.ReadOnly = True
        Me.GridMODFAC.RowHeadersVisible = False
        Me.GridMODFAC.SelectionMode = System.Windows.Forms.DataGridViewSelectionMode.CellSelect
        '
        'DataGridViewTextBoxColumn3
        '
        DataGridViewCellStyle1.Alignment = System.Windows.Forms.DataGridViewContentAlignment.MiddleLeft
        DataGridViewCellStyle1.BackColor = System.Drawing.Color.LightGray
        Me.DataGridViewTextBoxColumn3.DefaultCellStyle = DataGridViewCellStyle1
        resources.ApplyResources(Me.DataGridViewTextBoxColumn3, "DataGridViewTextBoxColumn3")
        Me.DataGridViewTextBoxColumn3.Name = "DataGridViewTextBoxColumn3"
        Me.DataGridViewTextBoxColumn3.ReadOnly = True
        '
        'DataGridViewTextBoxColumn4
        '
        DataGridViewCellStyle2.Alignment = System.Windows.Forms.DataGridViewContentAlignment.MiddleRight
        DataGridViewCellStyle2.ForeColor = System.Drawing.Color.SteelBlue
        Me.DataGridViewTextBoxColumn4.DefaultCellStyle = DataGridViewCellStyle2
        resources.ApplyResources(Me.DataGridViewTextBoxColumn4, "DataGridViewTextBoxColumn4")
        Me.DataGridViewTextBoxColumn4.Name = "DataGridViewTextBoxColumn4"
        Me.DataGridViewTextBoxColumn4.ReadOnly = True
        '
        'GroupBox3
        '
        resources.ApplyResources(Me.GroupBox3, "GroupBox3")
        Me.GroupBox3.Controls.Add(Me.GridUNIFAC)
        Me.GroupBox3.Name = "GroupBox3"
        Me.GroupBox3.TabStop = False
        '
        'GridUNIFAC
        '
        Me.GridUNIFAC.AllowUserToAddRows = False
        Me.GridUNIFAC.AllowUserToDeleteRows = False
        Me.GridUNIFAC.AllowUserToResizeColumns = False
        Me.GridUNIFAC.AllowUserToResizeRows = False
        Me.GridUNIFAC.AutoSizeColumnsMode = System.Windows.Forms.DataGridViewAutoSizeColumnsMode.Fill
        Me.GridUNIFAC.ColumnHeadersHeightSizeMode = System.Windows.Forms.DataGridViewColumnHeadersHeightSizeMode.AutoSize
        Me.GridUNIFAC.Columns.AddRange(New System.Windows.Forms.DataGridViewColumn() {Me.DataGridViewTextBoxColumn1, Me.DataGridViewTextBoxColumn2})
        resources.ApplyResources(Me.GridUNIFAC, "GridUNIFAC")
        Me.GridUNIFAC.Name = "GridUNIFAC"
        Me.GridUNIFAC.ReadOnly = True
        Me.GridUNIFAC.RowHeadersVisible = False
        Me.GridUNIFAC.SelectionMode = System.Windows.Forms.DataGridViewSelectionMode.CellSelect
        '
        'DataGridViewTextBoxColumn1
        '
        DataGridViewCellStyle3.Alignment = System.Windows.Forms.DataGridViewContentAlignment.MiddleLeft
        DataGridViewCellStyle3.BackColor = System.Drawing.Color.LightGray
        Me.DataGridViewTextBoxColumn1.DefaultCellStyle = DataGridViewCellStyle3
        resources.ApplyResources(Me.DataGridViewTextBoxColumn1, "DataGridViewTextBoxColumn1")
        Me.DataGridViewTextBoxColumn1.Name = "DataGridViewTextBoxColumn1"
        Me.DataGridViewTextBoxColumn1.ReadOnly = True
        '
        'DataGridViewTextBoxColumn2
        '
        DataGridViewCellStyle4.Alignment = System.Windows.Forms.DataGridViewContentAlignment.MiddleRight
        DataGridViewCellStyle4.ForeColor = System.Drawing.Color.SteelBlue
        Me.DataGridViewTextBoxColumn2.DefaultCellStyle = DataGridViewCellStyle4
        resources.ApplyResources(Me.DataGridViewTextBoxColumn2, "DataGridViewTextBoxColumn2")
        Me.DataGridViewTextBoxColumn2.Name = "DataGridViewTextBoxColumn2"
        Me.DataGridViewTextBoxColumn2.ReadOnly = True
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
        Me.GridProps.AllowUserToAddRows = False
        Me.GridProps.AllowUserToDeleteRows = False
        Me.GridProps.AllowUserToResizeColumns = False
        Me.GridProps.AllowUserToResizeRows = False
        Me.GridProps.AutoSizeColumnsMode = System.Windows.Forms.DataGridViewAutoSizeColumnsMode.Fill
        Me.GridProps.ColumnHeadersHeightSizeMode = System.Windows.Forms.DataGridViewColumnHeadersHeightSizeMode.AutoSize
        Me.GridProps.Columns.AddRange(New System.Windows.Forms.DataGridViewColumn() {Me.Column1, Me.Column2, Me.Column3})
        resources.ApplyResources(Me.GridProps, "GridProps")
        Me.GridProps.Name = "GridProps"
        Me.GridProps.RowHeadersVisible = False
        Me.GridProps.SelectionMode = System.Windows.Forms.DataGridViewSelectionMode.CellSelect
        '
        'Column1
        '
        Me.Column1.AutoSizeMode = System.Windows.Forms.DataGridViewAutoSizeColumnMode.Fill
        DataGridViewCellStyle5.Alignment = System.Windows.Forms.DataGridViewContentAlignment.MiddleLeft
        DataGridViewCellStyle5.BackColor = System.Drawing.Color.LightGray
        Me.Column1.DefaultCellStyle = DataGridViewCellStyle5
        resources.ApplyResources(Me.Column1, "Column1")
        Me.Column1.Name = "Column1"
        Me.Column1.ReadOnly = True
        '
        'Column2
        '
        Me.Column2.AutoSizeMode = System.Windows.Forms.DataGridViewAutoSizeColumnMode.AllCells
        DataGridViewCellStyle6.Alignment = System.Windows.Forms.DataGridViewContentAlignment.MiddleRight
        Me.Column2.DefaultCellStyle = DataGridViewCellStyle6
        resources.ApplyResources(Me.Column2, "Column2")
        Me.Column2.Name = "Column2"
        Me.Column2.ReadOnly = True
        '
        'Column3
        '
        Me.Column3.AutoSizeMode = System.Windows.Forms.DataGridViewAutoSizeColumnMode.AllCells
        DataGridViewCellStyle7.Alignment = System.Windows.Forms.DataGridViewContentAlignment.MiddleLeft
        DataGridViewCellStyle7.BackColor = System.Drawing.Color.LightGray
        Me.Column3.DefaultCellStyle = DataGridViewCellStyle7
        resources.ApplyResources(Me.Column3, "Column3")
        Me.Column3.Name = "Column3"
        Me.Column3.ReadOnly = True
        '
        'FaTabStripItem2
        '
        Me.FaTabStripItem2.CanClose = False
        Me.FaTabStripItem2.Controls.Add(Me.GraphCp)
        Me.FaTabStripItem2.IsDrawn = True
        Me.FaTabStripItem2.Name = "FaTabStripItem2"
        resources.ApplyResources(Me.FaTabStripItem2, "FaTabStripItem2")
        '
        'GraphCp
        '
        Me.GraphCp.BorderStyle = System.Windows.Forms.BorderStyle.Fixed3D
        resources.ApplyResources(Me.GraphCp, "GraphCp")
        Me.GraphCp.IsAntiAlias = True
        Me.GraphCp.IsAutoScrollRange = True
        Me.GraphCp.IsEnableSelection = True
        Me.GraphCp.IsZoomOnMouseCenter = True
        Me.GraphCp.Name = "GraphCp"
        Me.GraphCp.ScrollGrace = 0
        Me.GraphCp.ScrollMaxX = 0
        Me.GraphCp.ScrollMaxY = 0
        Me.GraphCp.ScrollMaxY2 = 0
        Me.GraphCp.ScrollMinX = 0
        Me.GraphCp.ScrollMinY = 0
        Me.GraphCp.ScrollMinY2 = 0
        '
        'FaTabStripItem3
        '
        Me.FaTabStripItem3.CanClose = False
        Me.FaTabStripItem3.Controls.Add(Me.GraphPvap)
        Me.FaTabStripItem3.IsDrawn = True
        Me.FaTabStripItem3.Name = "FaTabStripItem3"
        resources.ApplyResources(Me.FaTabStripItem3, "FaTabStripItem3")
        '
        'GraphPvap
        '
        Me.GraphPvap.BorderStyle = System.Windows.Forms.BorderStyle.Fixed3D
        resources.ApplyResources(Me.GraphPvap, "GraphPvap")
        Me.GraphPvap.IsAntiAlias = True
        Me.GraphPvap.IsAutoScrollRange = True
        Me.GraphPvap.Name = "GraphPvap"
        Me.GraphPvap.ScrollGrace = 0
        Me.GraphPvap.ScrollMaxX = 0
        Me.GraphPvap.ScrollMaxY = 0
        Me.GraphPvap.ScrollMaxY2 = 0
        Me.GraphPvap.ScrollMinX = 0
        Me.GraphPvap.ScrollMinY = 0
        Me.GraphPvap.ScrollMinY2 = 0
        '
        'FaTabStripItem4
        '
        Me.FaTabStripItem4.CanClose = False
        Me.FaTabStripItem4.Controls.Add(Me.GraphVisc)
        Me.FaTabStripItem4.IsDrawn = True
        Me.FaTabStripItem4.Name = "FaTabStripItem4"
        resources.ApplyResources(Me.FaTabStripItem4, "FaTabStripItem4")
        '
        'GraphVisc
        '
        Me.GraphVisc.BorderStyle = System.Windows.Forms.BorderStyle.Fixed3D
        resources.ApplyResources(Me.GraphVisc, "GraphVisc")
        Me.GraphVisc.IsAntiAlias = True
        Me.GraphVisc.IsAutoScrollRange = True
        Me.GraphVisc.Name = "GraphVisc"
        Me.GraphVisc.ScrollGrace = 0
        Me.GraphVisc.ScrollMaxX = 0
        Me.GraphVisc.ScrollMaxY = 0
        Me.GraphVisc.ScrollMaxY2 = 0
        Me.GraphVisc.ScrollMinX = 0
        Me.GraphVisc.ScrollMinY = 0
        Me.GraphVisc.ScrollMinY2 = 0
        '
        'FaTabStripItem5
        '
        Me.FaTabStripItem5.CanClose = False
        Me.FaTabStripItem5.Controls.Add(Me.GraphDHVAP)
        Me.FaTabStripItem5.IsDrawn = True
        Me.FaTabStripItem5.Name = "FaTabStripItem5"
        resources.ApplyResources(Me.FaTabStripItem5, "FaTabStripItem5")
        '
        'GraphDHVAP
        '
        Me.GraphDHVAP.BorderStyle = System.Windows.Forms.BorderStyle.Fixed3D
        resources.ApplyResources(Me.GraphDHVAP, "GraphDHVAP")
        Me.GraphDHVAP.IsAntiAlias = True
        Me.GraphDHVAP.IsAutoScrollRange = True
        Me.GraphDHVAP.Name = "GraphDHVAP"
        Me.GraphDHVAP.ScrollGrace = 0
        Me.GraphDHVAP.ScrollMaxX = 0
        Me.GraphDHVAP.ScrollMaxY = 0
        Me.GraphDHVAP.ScrollMaxY2 = 0
        Me.GraphDHVAP.ScrollMinX = 0
        Me.GraphDHVAP.ScrollMinY = 0
        Me.GraphDHVAP.ScrollMinY2 = 0
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
        Me.GroupBox4.ResumeLayout(False)
        CType(Me.GridMODFAC, System.ComponentModel.ISupportInitialize).EndInit()
        Me.GroupBox3.ResumeLayout(False)
        CType(Me.GridUNIFAC, System.ComponentModel.ISupportInitialize).EndInit()
        Me.GroupBox2.ResumeLayout(False)
        CType(Me.GridProps, System.ComponentModel.ISupportInitialize).EndInit()
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
    Public WithEvents GroupBox3 As System.Windows.Forms.GroupBox
    Public WithEvents GroupBox2 As System.Windows.Forms.GroupBox
    Public WithEvents GridProps As System.Windows.Forms.DataGridView
    Public WithEvents GridUNIFAC As System.Windows.Forms.DataGridView
    Public WithEvents GraphCp As ZedGraph.ZedGraphControl
    Public WithEvents GraphPvap As ZedGraph.ZedGraphControl
    Public WithEvents GraphVisc As ZedGraph.ZedGraphControl
    Public WithEvents FaTabStripItem5 As FarsiLibrary.Win.FATabStripItem
    Public WithEvents GraphDHVAP As ZedGraph.ZedGraphControl
    Public WithEvents GroupBox4 As System.Windows.Forms.GroupBox
    Public WithEvents GridMODFAC As System.Windows.Forms.DataGridView
    Public WithEvents DataGridViewTextBoxColumn3 As System.Windows.Forms.DataGridViewTextBoxColumn
    Public WithEvents DataGridViewTextBoxColumn4 As System.Windows.Forms.DataGridViewTextBoxColumn
    Public WithEvents DataGridViewTextBoxColumn1 As System.Windows.Forms.DataGridViewTextBoxColumn
    Public WithEvents DataGridViewTextBoxColumn2 As System.Windows.Forms.DataGridViewTextBoxColumn
    Friend WithEvents chkEnableEdit As System.Windows.Forms.CheckBox
    Friend WithEvents Button1 As System.Windows.Forms.Button
    Friend WithEvents Column1 As System.Windows.Forms.DataGridViewTextBoxColumn
    Friend WithEvents Column2 As System.Windows.Forms.DataGridViewTextBoxColumn
    Friend WithEvents Column3 As System.Windows.Forms.DataGridViewTextBoxColumn
End Class
