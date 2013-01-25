<Global.Microsoft.VisualBasic.CompilerServices.DesignerGenerated()> _
Partial Class FormDataRegression
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
        Dim resources As System.ComponentModel.ComponentResourceManager = New System.ComponentModel.ComponentResourceManager(GetType(FormDataRegression))
        Dim DataGridViewCellStyle1 As System.Windows.Forms.DataGridViewCellStyle = New System.Windows.Forms.DataGridViewCellStyle
        Dim DataGridViewCellStyle2 As System.Windows.Forms.DataGridViewCellStyle = New System.Windows.Forms.DataGridViewCellStyle
        Dim DataGridViewCellStyle3 As System.Windows.Forms.DataGridViewCellStyle = New System.Windows.Forms.DataGridViewCellStyle
        Me.GroupBox1 = New System.Windows.Forms.GroupBox
        Me.tbRegResults = New System.Windows.Forms.TextBox
        Me.btnDoReg = New System.Windows.Forms.Button
        Me.GroupBox2 = New System.Windows.Forms.GroupBox
        Me.Panel1 = New System.Windows.Forms.Panel
        Me.Button2 = New System.Windows.Forms.Button
        Me.Button1 = New System.Windows.Forms.Button
        Me.Label9 = New System.Windows.Forms.Label
        Me.gridInEst = New System.Windows.Forms.DataGridView
        Me.colpar = New System.Windows.Forms.DataGridViewTextBoxColumn
        Me.colval = New System.Windows.Forms.DataGridViewTextBoxColumn
        Me.btnCancel = New System.Windows.Forms.Button
        Me.cbPunit = New System.Windows.Forms.ComboBox
        Me.Label8 = New System.Windows.Forms.Label
        Me.cbTunit = New System.Windows.Forms.ComboBox
        Me.Label1 = New System.Windows.Forms.Label
        Me.chkIncludeSD = New System.Windows.Forms.CheckBox
        Me.btnAdvConfig = New System.Windows.Forms.Button
        Me.cbObjFunc = New System.Windows.Forms.ComboBox
        Me.LabelWithDivider12 = New System.Windows.Forms.LabelWithDivider
        Me.Label7 = New System.Windows.Forms.Label
        Me.Label2 = New System.Windows.Forms.Label
        Me.cbRegMethod = New System.Windows.Forms.ComboBox
        Me.Label3 = New System.Windows.Forms.Label
        Me.Label6 = New System.Windows.Forms.Label
        Me.cbCompound1 = New System.Windows.Forms.ComboBox
        Me.LabelWithDivider3 = New System.Windows.Forms.LabelWithDivider
        Me.Label4 = New System.Windows.Forms.Label
        Me.GridExpData = New System.Windows.Forms.DataGridView
        Me.ContextMenuStrip1 = New System.Windows.Forms.ContextMenuStrip(Me.components)
        Me.ToolStripMenuItem1 = New System.Windows.Forms.ToolStripMenuItem
        Me.ToolStripMenuItem2 = New System.Windows.Forms.ToolStripMenuItem
        Me.ToolStripMenuItem3 = New System.Windows.Forms.ToolStripMenuItem
        Me.ToolStripMenuItem4 = New System.Windows.Forms.ToolStripMenuItem
        Me.ToolStripMenuItem5 = New System.Windows.Forms.ToolStripMenuItem
        Me.ToolStripMenuItem6 = New System.Windows.Forms.ToolStripMenuItem
        Me.ToolStripMenuItem7 = New System.Windows.Forms.ToolStripMenuItem
        Me.cbCompound2 = New System.Windows.Forms.ComboBox
        Me.cbDataType = New System.Windows.Forms.ComboBox
        Me.cbModel = New System.Windows.Forms.ComboBox
        Me.Label5 = New System.Windows.Forms.Label
        Me.LabelWithDivider1 = New System.Windows.Forms.LabelWithDivider
        Me.LabelWithDivider2 = New System.Windows.Forms.LabelWithDivider
        Me.GroupBox3 = New System.Windows.Forms.GroupBox
        Me.graph = New ZedGraph.ZedGraphControl
        Me.GroupBox4 = New System.Windows.Forms.GroupBox
        Me.tbDescription = New System.Windows.Forms.TextBox
        Me.Label11 = New System.Windows.Forms.Label
        Me.tbTitle = New System.Windows.Forms.TextBox
        Me.Label10 = New System.Windows.Forms.Label
        Me.colx1 = New System.Windows.Forms.DataGridViewTextBoxColumn
        Me.colx2 = New System.Windows.Forms.DataGridViewTextBoxColumn
        Me.coly1 = New System.Windows.Forms.DataGridViewTextBoxColumn
        Me.colT = New System.Windows.Forms.DataGridViewTextBoxColumn
        Me.colP = New System.Windows.Forms.DataGridViewTextBoxColumn
        Me.GroupBox1.SuspendLayout()
        Me.GroupBox2.SuspendLayout()
        Me.Panel1.SuspendLayout()
        CType(Me.gridInEst, System.ComponentModel.ISupportInitialize).BeginInit()
        CType(Me.GridExpData, System.ComponentModel.ISupportInitialize).BeginInit()
        Me.ContextMenuStrip1.SuspendLayout()
        Me.GroupBox3.SuspendLayout()
        Me.GroupBox4.SuspendLayout()
        Me.SuspendLayout()
        '
        'GroupBox1
        '
        resources.ApplyResources(Me.GroupBox1, "GroupBox1")
        Me.GroupBox1.Controls.Add(Me.tbRegResults)
        Me.GroupBox1.Name = "GroupBox1"
        Me.GroupBox1.TabStop = False
        '
        'tbRegResults
        '
        resources.ApplyResources(Me.tbRegResults, "tbRegResults")
        Me.tbRegResults.Name = "tbRegResults"
        Me.tbRegResults.ReadOnly = True
        '
        'btnDoReg
        '
        resources.ApplyResources(Me.btnDoReg, "btnDoReg")
        Me.btnDoReg.Name = "btnDoReg"
        Me.btnDoReg.UseVisualStyleBackColor = True
        '
        'GroupBox2
        '
        resources.ApplyResources(Me.GroupBox2, "GroupBox2")
        Me.GroupBox2.Controls.Add(Me.Panel1)
        Me.GroupBox2.Name = "GroupBox2"
        Me.GroupBox2.TabStop = False
        '
        'Panel1
        '
        resources.ApplyResources(Me.Panel1, "Panel1")
        Me.Panel1.Controls.Add(Me.Button2)
        Me.Panel1.Controls.Add(Me.Button1)
        Me.Panel1.Controls.Add(Me.Label9)
        Me.Panel1.Controls.Add(Me.gridInEst)
        Me.Panel1.Controls.Add(Me.btnCancel)
        Me.Panel1.Controls.Add(Me.cbPunit)
        Me.Panel1.Controls.Add(Me.Label8)
        Me.Panel1.Controls.Add(Me.cbTunit)
        Me.Panel1.Controls.Add(Me.Label1)
        Me.Panel1.Controls.Add(Me.chkIncludeSD)
        Me.Panel1.Controls.Add(Me.btnAdvConfig)
        Me.Panel1.Controls.Add(Me.cbObjFunc)
        Me.Panel1.Controls.Add(Me.LabelWithDivider12)
        Me.Panel1.Controls.Add(Me.Label7)
        Me.Panel1.Controls.Add(Me.Label2)
        Me.Panel1.Controls.Add(Me.cbRegMethod)
        Me.Panel1.Controls.Add(Me.btnDoReg)
        Me.Panel1.Controls.Add(Me.Label3)
        Me.Panel1.Controls.Add(Me.Label6)
        Me.Panel1.Controls.Add(Me.cbCompound1)
        Me.Panel1.Controls.Add(Me.LabelWithDivider3)
        Me.Panel1.Controls.Add(Me.Label4)
        Me.Panel1.Controls.Add(Me.GridExpData)
        Me.Panel1.Controls.Add(Me.cbCompound2)
        Me.Panel1.Controls.Add(Me.cbDataType)
        Me.Panel1.Controls.Add(Me.cbModel)
        Me.Panel1.Controls.Add(Me.Label5)
        Me.Panel1.Controls.Add(Me.LabelWithDivider1)
        Me.Panel1.Controls.Add(Me.LabelWithDivider2)
        Me.Panel1.Name = "Panel1"
        '
        'Button2
        '
        resources.ApplyResources(Me.Button2, "Button2")
        Me.Button2.Name = "Button2"
        Me.Button2.UseVisualStyleBackColor = True
        '
        'Button1
        '
        resources.ApplyResources(Me.Button1, "Button1")
        Me.Button1.Name = "Button1"
        Me.Button1.UseVisualStyleBackColor = True
        '
        'Label9
        '
        resources.ApplyResources(Me.Label9, "Label9")
        Me.Label9.Name = "Label9"
        '
        'gridInEst
        '
        Me.gridInEst.AllowUserToAddRows = False
        Me.gridInEst.AllowUserToDeleteRows = False
        Me.gridInEst.AllowUserToResizeRows = False
        DataGridViewCellStyle1.BackColor = System.Drawing.Color.WhiteSmoke
        Me.gridInEst.AlternatingRowsDefaultCellStyle = DataGridViewCellStyle1
        Me.gridInEst.AutoSizeColumnsMode = System.Windows.Forms.DataGridViewAutoSizeColumnsMode.Fill
        Me.gridInEst.ColumnHeadersHeightSizeMode = System.Windows.Forms.DataGridViewColumnHeadersHeightSizeMode.AutoSize
        Me.gridInEst.Columns.AddRange(New System.Windows.Forms.DataGridViewColumn() {Me.colpar, Me.colval})
        resources.ApplyResources(Me.gridInEst, "gridInEst")
        Me.gridInEst.Name = "gridInEst"
        Me.gridInEst.RowHeadersVisible = False
        '
        'colpar
        '
        resources.ApplyResources(Me.colpar, "colpar")
        Me.colpar.Name = "colpar"
        Me.colpar.ReadOnly = True
        '
        'colval
        '
        DataGridViewCellStyle2.Format = "N4"
        Me.colval.DefaultCellStyle = DataGridViewCellStyle2
        resources.ApplyResources(Me.colval, "colval")
        Me.colval.Name = "colval"
        '
        'btnCancel
        '
        resources.ApplyResources(Me.btnCancel, "btnCancel")
        Me.btnCancel.Name = "btnCancel"
        Me.btnCancel.UseVisualStyleBackColor = True
        '
        'cbPunit
        '
        Me.cbPunit.DropDownStyle = System.Windows.Forms.ComboBoxStyle.DropDownList
        Me.cbPunit.FormattingEnabled = True
        Me.cbPunit.Items.AddRange(New Object() {resources.GetString("cbPunit.Items"), resources.GetString("cbPunit.Items1"), resources.GetString("cbPunit.Items2"), resources.GetString("cbPunit.Items3"), resources.GetString("cbPunit.Items4"), resources.GetString("cbPunit.Items5"), resources.GetString("cbPunit.Items6"), resources.GetString("cbPunit.Items7"), resources.GetString("cbPunit.Items8"), resources.GetString("cbPunit.Items9"), resources.GetString("cbPunit.Items10"), resources.GetString("cbPunit.Items11"), resources.GetString("cbPunit.Items12"), resources.GetString("cbPunit.Items13"), resources.GetString("cbPunit.Items14"), resources.GetString("cbPunit.Items15"), resources.GetString("cbPunit.Items16"), resources.GetString("cbPunit.Items17"), resources.GetString("cbPunit.Items18")})
        resources.ApplyResources(Me.cbPunit, "cbPunit")
        Me.cbPunit.Name = "cbPunit"
        '
        'Label8
        '
        resources.ApplyResources(Me.Label8, "Label8")
        Me.Label8.Name = "Label8"
        '
        'cbTunit
        '
        Me.cbTunit.DropDownStyle = System.Windows.Forms.ComboBoxStyle.DropDownList
        Me.cbTunit.FormattingEnabled = True
        Me.cbTunit.Items.AddRange(New Object() {resources.GetString("cbTunit.Items"), resources.GetString("cbTunit.Items1"), resources.GetString("cbTunit.Items2"), resources.GetString("cbTunit.Items3")})
        resources.ApplyResources(Me.cbTunit, "cbTunit")
        Me.cbTunit.Name = "cbTunit"
        '
        'Label1
        '
        resources.ApplyResources(Me.Label1, "Label1")
        Me.Label1.Name = "Label1"
        '
        'chkIncludeSD
        '
        resources.ApplyResources(Me.chkIncludeSD, "chkIncludeSD")
        Me.chkIncludeSD.Name = "chkIncludeSD"
        Me.chkIncludeSD.UseVisualStyleBackColor = True
        '
        'btnAdvConfig
        '
        resources.ApplyResources(Me.btnAdvConfig, "btnAdvConfig")
        Me.btnAdvConfig.Name = "btnAdvConfig"
        Me.btnAdvConfig.UseVisualStyleBackColor = True
        '
        'cbObjFunc
        '
        resources.ApplyResources(Me.cbObjFunc, "cbObjFunc")
        Me.cbObjFunc.DropDownStyle = System.Windows.Forms.ComboBoxStyle.DropDownList
        Me.cbObjFunc.FormattingEnabled = True
        Me.cbObjFunc.Items.AddRange(New Object() {resources.GetString("cbObjFunc.Items"), resources.GetString("cbObjFunc.Items1"), resources.GetString("cbObjFunc.Items2"), resources.GetString("cbObjFunc.Items3"), resources.GetString("cbObjFunc.Items4"), resources.GetString("cbObjFunc.Items5")})
        Me.cbObjFunc.Name = "cbObjFunc"
        '
        'LabelWithDivider12
        '
        resources.ApplyResources(Me.LabelWithDivider12, "LabelWithDivider12")
        Me.LabelWithDivider12.Gap = 5
        Me.LabelWithDivider12.Name = "LabelWithDivider12"
        '
        'Label7
        '
        resources.ApplyResources(Me.Label7, "Label7")
        Me.Label7.Name = "Label7"
        '
        'Label2
        '
        resources.ApplyResources(Me.Label2, "Label2")
        Me.Label2.Name = "Label2"
        '
        'cbRegMethod
        '
        resources.ApplyResources(Me.cbRegMethod, "cbRegMethod")
        Me.cbRegMethod.DropDownStyle = System.Windows.Forms.ComboBoxStyle.DropDownList
        Me.cbRegMethod.FormattingEnabled = True
        Me.cbRegMethod.Items.AddRange(New Object() {resources.GetString("cbRegMethod.Items"), resources.GetString("cbRegMethod.Items1"), resources.GetString("cbRegMethod.Items2"), resources.GetString("cbRegMethod.Items3")})
        Me.cbRegMethod.Name = "cbRegMethod"
        '
        'Label3
        '
        resources.ApplyResources(Me.Label3, "Label3")
        Me.Label3.Name = "Label3"
        '
        'Label6
        '
        resources.ApplyResources(Me.Label6, "Label6")
        Me.Label6.Name = "Label6"
        '
        'cbCompound1
        '
        Me.cbCompound1.DropDownStyle = System.Windows.Forms.ComboBoxStyle.DropDownList
        Me.cbCompound1.FormattingEnabled = True
        resources.ApplyResources(Me.cbCompound1, "cbCompound1")
        Me.cbCompound1.Name = "cbCompound1"
        '
        'LabelWithDivider3
        '
        resources.ApplyResources(Me.LabelWithDivider3, "LabelWithDivider3")
        Me.LabelWithDivider3.Gap = 5
        Me.LabelWithDivider3.Name = "LabelWithDivider3"
        '
        'Label4
        '
        resources.ApplyResources(Me.Label4, "Label4")
        Me.Label4.Name = "Label4"
        '
        'GridExpData
        '
        DataGridViewCellStyle3.BackColor = System.Drawing.Color.WhiteSmoke
        Me.GridExpData.AlternatingRowsDefaultCellStyle = DataGridViewCellStyle3
        resources.ApplyResources(Me.GridExpData, "GridExpData")
        Me.GridExpData.AutoSizeColumnsMode = System.Windows.Forms.DataGridViewAutoSizeColumnsMode.Fill
        Me.GridExpData.ColumnHeadersHeightSizeMode = System.Windows.Forms.DataGridViewColumnHeadersHeightSizeMode.AutoSize
        Me.GridExpData.Columns.AddRange(New System.Windows.Forms.DataGridViewColumn() {Me.colx1, Me.colx2, Me.coly1, Me.colT, Me.colP})
        Me.GridExpData.ContextMenuStrip = Me.ContextMenuStrip1
        Me.GridExpData.Name = "GridExpData"
        Me.GridExpData.RowHeadersVisible = False
        '
        'ContextMenuStrip1
        '
        Me.ContextMenuStrip1.Items.AddRange(New System.Windows.Forms.ToolStripItem() {Me.ToolStripMenuItem1, Me.ToolStripMenuItem2, Me.ToolStripMenuItem3, Me.ToolStripMenuItem4, Me.ToolStripMenuItem5, Me.ToolStripMenuItem6, Me.ToolStripMenuItem7})
        Me.ContextMenuStrip1.Name = "ContextMenuStrip1"
        resources.ApplyResources(Me.ContextMenuStrip1, "ContextMenuStrip1")
        '
        'ToolStripMenuItem1
        '
        Me.ToolStripMenuItem1.Image = Global.DWSIM.My.Resources.Resources.lightning
        Me.ToolStripMenuItem1.Name = "ToolStripMenuItem1"
        resources.ApplyResources(Me.ToolStripMenuItem1, "ToolStripMenuItem1")
        '
        'ToolStripMenuItem2
        '
        Me.ToolStripMenuItem2.Image = Global.DWSIM.My.Resources.Resources.lightning
        Me.ToolStripMenuItem2.Name = "ToolStripMenuItem2"
        resources.ApplyResources(Me.ToolStripMenuItem2, "ToolStripMenuItem2")
        '
        'ToolStripMenuItem3
        '
        Me.ToolStripMenuItem3.Image = Global.DWSIM.My.Resources.Resources.lightning
        Me.ToolStripMenuItem3.Name = "ToolStripMenuItem3"
        resources.ApplyResources(Me.ToolStripMenuItem3, "ToolStripMenuItem3")
        '
        'ToolStripMenuItem4
        '
        Me.ToolStripMenuItem4.Image = Global.DWSIM.My.Resources.Resources.lightning
        Me.ToolStripMenuItem4.Name = "ToolStripMenuItem4"
        resources.ApplyResources(Me.ToolStripMenuItem4, "ToolStripMenuItem4")
        '
        'ToolStripMenuItem5
        '
        Me.ToolStripMenuItem5.Image = Global.DWSIM.My.Resources.Resources.lightning
        Me.ToolStripMenuItem5.Name = "ToolStripMenuItem5"
        resources.ApplyResources(Me.ToolStripMenuItem5, "ToolStripMenuItem5")
        '
        'ToolStripMenuItem6
        '
        Me.ToolStripMenuItem6.Image = Global.DWSIM.My.Resources.Resources.lightning
        Me.ToolStripMenuItem6.Name = "ToolStripMenuItem6"
        resources.ApplyResources(Me.ToolStripMenuItem6, "ToolStripMenuItem6")
        '
        'ToolStripMenuItem7
        '
        Me.ToolStripMenuItem7.Image = Global.DWSIM.My.Resources.Resources.lightning
        Me.ToolStripMenuItem7.Name = "ToolStripMenuItem7"
        resources.ApplyResources(Me.ToolStripMenuItem7, "ToolStripMenuItem7")
        '
        'cbCompound2
        '
        Me.cbCompound2.DropDownStyle = System.Windows.Forms.ComboBoxStyle.DropDownList
        Me.cbCompound2.FormattingEnabled = True
        resources.ApplyResources(Me.cbCompound2, "cbCompound2")
        Me.cbCompound2.Name = "cbCompound2"
        '
        'cbDataType
        '
        Me.cbDataType.DropDownStyle = System.Windows.Forms.ComboBoxStyle.DropDownList
        Me.cbDataType.FormattingEnabled = True
        Me.cbDataType.Items.AddRange(New Object() {resources.GetString("cbDataType.Items"), resources.GetString("cbDataType.Items1"), resources.GetString("cbDataType.Items2"), resources.GetString("cbDataType.Items3"), resources.GetString("cbDataType.Items4"), resources.GetString("cbDataType.Items5")})
        resources.ApplyResources(Me.cbDataType, "cbDataType")
        Me.cbDataType.Name = "cbDataType"
        '
        'cbModel
        '
        Me.cbModel.DropDownStyle = System.Windows.Forms.ComboBoxStyle.DropDownList
        Me.cbModel.FormattingEnabled = True
        Me.cbModel.Items.AddRange(New Object() {resources.GetString("cbModel.Items"), resources.GetString("cbModel.Items1"), resources.GetString("cbModel.Items2"), resources.GetString("cbModel.Items3"), resources.GetString("cbModel.Items4"), resources.GetString("cbModel.Items5"), resources.GetString("cbModel.Items6"), resources.GetString("cbModel.Items7")})
        resources.ApplyResources(Me.cbModel, "cbModel")
        Me.cbModel.Name = "cbModel"
        '
        'Label5
        '
        resources.ApplyResources(Me.Label5, "Label5")
        Me.Label5.Name = "Label5"
        '
        'LabelWithDivider1
        '
        resources.ApplyResources(Me.LabelWithDivider1, "LabelWithDivider1")
        Me.LabelWithDivider1.Gap = 5
        Me.LabelWithDivider1.Name = "LabelWithDivider1"
        '
        'LabelWithDivider2
        '
        resources.ApplyResources(Me.LabelWithDivider2, "LabelWithDivider2")
        Me.LabelWithDivider2.Gap = 5
        Me.LabelWithDivider2.Name = "LabelWithDivider2"
        '
        'GroupBox3
        '
        resources.ApplyResources(Me.GroupBox3, "GroupBox3")
        Me.GroupBox3.Controls.Add(Me.graph)
        Me.GroupBox3.Name = "GroupBox3"
        Me.GroupBox3.TabStop = False
        '
        'graph
        '
        resources.ApplyResources(Me.graph, "graph")
        Me.graph.Name = "graph"
        Me.graph.ScrollGrace = 0
        Me.graph.ScrollMaxX = 0
        Me.graph.ScrollMaxY = 0
        Me.graph.ScrollMaxY2 = 0
        Me.graph.ScrollMinX = 0
        Me.graph.ScrollMinY = 0
        Me.graph.ScrollMinY2 = 0
        '
        'GroupBox4
        '
        resources.ApplyResources(Me.GroupBox4, "GroupBox4")
        Me.GroupBox4.Controls.Add(Me.tbDescription)
        Me.GroupBox4.Controls.Add(Me.Label11)
        Me.GroupBox4.Controls.Add(Me.tbTitle)
        Me.GroupBox4.Controls.Add(Me.Label10)
        Me.GroupBox4.Name = "GroupBox4"
        Me.GroupBox4.TabStop = False
        '
        'tbDescription
        '
        resources.ApplyResources(Me.tbDescription, "tbDescription")
        Me.tbDescription.Name = "tbDescription"
        '
        'Label11
        '
        resources.ApplyResources(Me.Label11, "Label11")
        Me.Label11.Name = "Label11"
        '
        'tbTitle
        '
        resources.ApplyResources(Me.tbTitle, "tbTitle")
        Me.tbTitle.Name = "tbTitle"
        '
        'Label10
        '
        resources.ApplyResources(Me.Label10, "Label10")
        Me.Label10.Name = "Label10"
        '
        'colx1
        '
        resources.ApplyResources(Me.colx1, "colx1")
        Me.colx1.Name = "colx1"
        '
        'colx2
        '
        resources.ApplyResources(Me.colx2, "colx2")
        Me.colx2.Name = "colx2"
        '
        'coly1
        '
        resources.ApplyResources(Me.coly1, "coly1")
        Me.coly1.Name = "coly1"
        '
        'colT
        '
        resources.ApplyResources(Me.colT, "colT")
        Me.colT.Name = "colT"
        '
        'colP
        '
        resources.ApplyResources(Me.colP, "colP")
        Me.colP.Name = "colP"
        '
        'FormDataRegression
        '
        resources.ApplyResources(Me, "$this")
        Me.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font
        Me.Controls.Add(Me.GroupBox4)
        Me.Controls.Add(Me.GroupBox3)
        Me.Controls.Add(Me.GroupBox2)
        Me.Controls.Add(Me.GroupBox1)
        Me.Name = "FormDataRegression"
        Me.GroupBox1.ResumeLayout(False)
        Me.GroupBox1.PerformLayout()
        Me.GroupBox2.ResumeLayout(False)
        Me.Panel1.ResumeLayout(False)
        Me.Panel1.PerformLayout()
        CType(Me.gridInEst, System.ComponentModel.ISupportInitialize).EndInit()
        CType(Me.GridExpData, System.ComponentModel.ISupportInitialize).EndInit()
        Me.ContextMenuStrip1.ResumeLayout(False)
        Me.GroupBox3.ResumeLayout(False)
        Me.GroupBox4.ResumeLayout(False)
        Me.GroupBox4.PerformLayout()
        Me.ResumeLayout(False)

    End Sub
    Friend WithEvents GroupBox1 As System.Windows.Forms.GroupBox
    Friend WithEvents btnDoReg As System.Windows.Forms.Button
    Friend WithEvents GroupBox2 As System.Windows.Forms.GroupBox
    Friend WithEvents cbCompound2 As System.Windows.Forms.ComboBox
    Friend WithEvents cbCompound1 As System.Windows.Forms.ComboBox
    Friend WithEvents Label3 As System.Windows.Forms.Label
    Friend WithEvents Label2 As System.Windows.Forms.Label
    Friend WithEvents GroupBox3 As System.Windows.Forms.GroupBox
    Friend WithEvents cbModel As System.Windows.Forms.ComboBox
    Friend WithEvents Label4 As System.Windows.Forms.Label
    Friend WithEvents cbDataType As System.Windows.Forms.ComboBox
    Friend WithEvents Label5 As System.Windows.Forms.Label
    Public WithEvents LabelWithDivider2 As System.Windows.Forms.LabelWithDivider
    Public WithEvents LabelWithDivider1 As System.Windows.Forms.LabelWithDivider
    Public WithEvents LabelWithDivider12 As System.Windows.Forms.LabelWithDivider
    Friend WithEvents Panel1 As System.Windows.Forms.Panel
    Friend WithEvents cbObjFunc As System.Windows.Forms.ComboBox
    Friend WithEvents Label7 As System.Windows.Forms.Label
    Friend WithEvents cbRegMethod As System.Windows.Forms.ComboBox
    Friend WithEvents Label6 As System.Windows.Forms.Label
    Public WithEvents LabelWithDivider3 As System.Windows.Forms.LabelWithDivider
    Public WithEvents GridExpData As System.Windows.Forms.DataGridView
    Friend WithEvents graph As ZedGraph.ZedGraphControl
    Friend WithEvents tbRegResults As System.Windows.Forms.TextBox
    Friend WithEvents btnAdvConfig As System.Windows.Forms.Button
    Friend WithEvents chkIncludeSD As System.Windows.Forms.CheckBox
    Friend WithEvents cbPunit As System.Windows.Forms.ComboBox
    Friend WithEvents Label8 As System.Windows.Forms.Label
    Friend WithEvents cbTunit As System.Windows.Forms.ComboBox
    Friend WithEvents Label1 As System.Windows.Forms.Label
    Friend WithEvents btnCancel As System.Windows.Forms.Button
    Public WithEvents gridInEst As System.Windows.Forms.DataGridView
    Friend WithEvents Label9 As System.Windows.Forms.Label
    Friend WithEvents colpar As System.Windows.Forms.DataGridViewTextBoxColumn
    Friend WithEvents colval As System.Windows.Forms.DataGridViewTextBoxColumn
    Friend WithEvents GroupBox4 As System.Windows.Forms.GroupBox
    Friend WithEvents tbTitle As System.Windows.Forms.TextBox
    Friend WithEvents Label10 As System.Windows.Forms.Label
    Friend WithEvents tbDescription As System.Windows.Forms.TextBox
    Friend WithEvents Label11 As System.Windows.Forms.Label
    Friend WithEvents Button2 As System.Windows.Forms.Button
    Friend WithEvents Button1 As System.Windows.Forms.Button
    Friend WithEvents ContextMenuStrip1 As System.Windows.Forms.ContextMenuStrip
    Friend WithEvents ToolStripMenuItem1 As System.Windows.Forms.ToolStripMenuItem
    Friend WithEvents ToolStripMenuItem2 As System.Windows.Forms.ToolStripMenuItem
    Friend WithEvents ToolStripMenuItem3 As System.Windows.Forms.ToolStripMenuItem
    Friend WithEvents ToolStripMenuItem4 As System.Windows.Forms.ToolStripMenuItem
    Friend WithEvents ToolStripMenuItem5 As System.Windows.Forms.ToolStripMenuItem
    Friend WithEvents ToolStripMenuItem6 As System.Windows.Forms.ToolStripMenuItem
    Friend WithEvents ToolStripMenuItem7 As System.Windows.Forms.ToolStripMenuItem
    Friend WithEvents colx1 As System.Windows.Forms.DataGridViewTextBoxColumn
    Friend WithEvents colx2 As System.Windows.Forms.DataGridViewTextBoxColumn
    Friend WithEvents coly1 As System.Windows.Forms.DataGridViewTextBoxColumn
    Friend WithEvents colT As System.Windows.Forms.DataGridViewTextBoxColumn
    Friend WithEvents colP As System.Windows.Forms.DataGridViewTextBoxColumn
End Class
