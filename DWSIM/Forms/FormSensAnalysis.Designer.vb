<Global.Microsoft.VisualBasic.CompilerServices.DesignerGenerated()> _
Partial Class FormSensAnalysis
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
        Dim resources As System.ComponentModel.ComponentResourceManager = New System.ComponentModel.ComponentResourceManager(GetType(FormSensAnalysis))
        Dim DataGridViewCellStyle6 As System.Windows.Forms.DataGridViewCellStyle = New System.Windows.Forms.DataGridViewCellStyle()
        Dim DataGridViewCellStyle7 As System.Windows.Forms.DataGridViewCellStyle = New System.Windows.Forms.DataGridViewCellStyle()
        Dim DataGridViewCellStyle8 As System.Windows.Forms.DataGridViewCellStyle = New System.Windows.Forms.DataGridViewCellStyle()
        Dim DataGridViewCellStyle9 As System.Windows.Forms.DataGridViewCellStyle = New System.Windows.Forms.DataGridViewCellStyle()
        Dim DataGridViewCellStyle10 As System.Windows.Forms.DataGridViewCellStyle = New System.Windows.Forms.DataGridViewCellStyle()
        Me.btnRun = New System.Windows.Forms.Button()
        Me.Label1 = New System.Windows.Forms.Label()
        Me.cbObjIndVar1 = New System.Windows.Forms.ComboBox()
        Me.GroupBox1 = New System.Windows.Forms.GroupBox()
        Me.GroupBox7 = New System.Windows.Forms.GroupBox()
        Me.tbStats = New System.Windows.Forms.TextBox()
        Me.dgvResults = New System.Windows.Forms.DataGridView()
        Me.Column1 = New System.Windows.Forms.DataGridViewTextBoxColumn()
        Me.Column2 = New System.Windows.Forms.DataGridViewTextBoxColumn()
        Me.GroupBox2 = New System.Windows.Forms.GroupBox()
        Me.btnDeleteCase = New System.Windows.Forms.Button()
        Me.btnSaveCase = New System.Windows.Forms.Button()
        Me.btnCopyCase = New System.Windows.Forms.Button()
        Me.btnNewCase = New System.Windows.Forms.Button()
        Me.lbCases = New System.Windows.Forms.ListBox()
        Me.GroupBox3 = New System.Windows.Forms.GroupBox()
        Me.tbCaseDesc = New System.Windows.Forms.TextBox()
        Me.Label4 = New System.Windows.Forms.Label()
        Me.tbCaseName = New System.Windows.Forms.TextBox()
        Me.Label3 = New System.Windows.Forms.Label()
        Me.GroupBox4 = New System.Windows.Forms.GroupBox()
        Me.tbUpperLimIndVar1 = New System.Windows.Forms.TextBox()
        Me.Label8 = New System.Windows.Forms.Label()
        Me.tbUnitIndVar1 = New System.Windows.Forms.TextBox()
        Me.Label2 = New System.Windows.Forms.Label()
        Me.tbLowerLimIndVar1 = New System.Windows.Forms.TextBox()
        Me.Label7 = New System.Windows.Forms.Label()
        Me.nuNumPointsIndVar1 = New System.Windows.Forms.NumericUpDown()
        Me.Label6 = New System.Windows.Forms.Label()
        Me.cbPropIndVar1 = New System.Windows.Forms.ComboBox()
        Me.Label5 = New System.Windows.Forms.Label()
        Me.GroupBox5 = New System.Windows.Forms.GroupBox()
        Me.pnIndvar2 = New System.Windows.Forms.Panel()
        Me.Label14 = New System.Windows.Forms.Label()
        Me.cbObjIndVar2 = New System.Windows.Forms.ComboBox()
        Me.tbUpperLimIndVar2 = New System.Windows.Forms.TextBox()
        Me.Label13 = New System.Windows.Forms.Label()
        Me.Label9 = New System.Windows.Forms.Label()
        Me.cbPropIndVar2 = New System.Windows.Forms.ComboBox()
        Me.tbUnitIndVar2 = New System.Windows.Forms.TextBox()
        Me.Label12 = New System.Windows.Forms.Label()
        Me.Label10 = New System.Windows.Forms.Label()
        Me.nuNumPointsIndVar2 = New System.Windows.Forms.NumericUpDown()
        Me.tbLowerLimIndVar2 = New System.Windows.Forms.TextBox()
        Me.Label11 = New System.Windows.Forms.Label()
        Me.chkIndVar2 = New System.Windows.Forms.CheckBox()
        Me.GroupBox6 = New System.Windows.Forms.GroupBox()
        Me.rbExp = New System.Windows.Forms.RadioButton()
        Me.rbVar = New System.Windows.Forms.RadioButton()
        Me.gbExp = New System.Windows.Forms.GroupBox()
        Me.GroupBox9 = New System.Windows.Forms.GroupBox()
        Me.dgVariables = New System.Windows.Forms.DataGridView()
        Me.Column8 = New System.Windows.Forms.DataGridViewTextBoxColumn()
        Me.DataGridViewTextBoxColumn1 = New System.Windows.Forms.DataGridViewTextBoxColumn()
        Me.DataGridViewComboBoxColumn2 = New System.Windows.Forms.DataGridViewComboBoxColumn()
        Me.Column4 = New System.Windows.Forms.DataGridViewComboBoxColumn()
        Me.Column10 = New System.Windows.Forms.DataGridViewTextBoxColumn()
        Me.Column9 = New System.Windows.Forms.DataGridViewTextBoxColumn()
        Me.ToolStrip1 = New System.Windows.Forms.ToolStrip()
        Me.tsbAddVar = New System.Windows.Forms.ToolStripButton()
        Me.tsbDelVar = New System.Windows.Forms.ToolStripButton()
        Me.GroupBox8 = New System.Windows.Forms.GroupBox()
        Me.btnClear = New System.Windows.Forms.Button()
        Me.btnVerify = New System.Windows.Forms.Button()
        Me.tbCurrentValue = New System.Windows.Forms.TextBox()
        Me.Label15 = New System.Windows.Forms.Label()
        Me.tbExpression = New System.Windows.Forms.TextBox()
        Me.Label17 = New System.Windows.Forms.Label()
        Me.gbVar = New System.Windows.Forms.GroupBox()
        Me.GroupBox10 = New System.Windows.Forms.GroupBox()
        Me.dgDepVariables = New System.Windows.Forms.DataGridView()
        Me.DataGridViewTextBoxColumn2 = New System.Windows.Forms.DataGridViewTextBoxColumn()
        Me.DataGridViewComboBoxColumn1 = New System.Windows.Forms.DataGridViewComboBoxColumn()
        Me.DataGridViewComboBoxColumn3 = New System.Windows.Forms.DataGridViewComboBoxColumn()
        Me.DataGridViewTextBoxColumn5 = New System.Windows.Forms.DataGridViewTextBoxColumn()
        Me.ToolStrip2 = New System.Windows.Forms.ToolStrip()
        Me.ToolStripButton1 = New System.Windows.Forms.ToolStripButton()
        Me.ToolStripButton2 = New System.Windows.Forms.ToolStripButton()
        Me.Panel1 = New System.Windows.Forms.Panel()
        Me.btnAbort = New System.Windows.Forms.Button()
        Me.LinkLabel1 = New System.Windows.Forms.LinkLabel()
        Me.PictureBox1 = New System.Windows.Forms.PictureBox()
        Me.GroupBox1.SuspendLayout()
        Me.GroupBox7.SuspendLayout()
        CType(Me.dgvResults, System.ComponentModel.ISupportInitialize).BeginInit()
        Me.GroupBox2.SuspendLayout()
        Me.GroupBox3.SuspendLayout()
        Me.GroupBox4.SuspendLayout()
        CType(Me.nuNumPointsIndVar1, System.ComponentModel.ISupportInitialize).BeginInit()
        Me.GroupBox5.SuspendLayout()
        Me.pnIndvar2.SuspendLayout()
        CType(Me.nuNumPointsIndVar2, System.ComponentModel.ISupportInitialize).BeginInit()
        Me.GroupBox6.SuspendLayout()
        Me.gbExp.SuspendLayout()
        Me.GroupBox9.SuspendLayout()
        CType(Me.dgVariables, System.ComponentModel.ISupportInitialize).BeginInit()
        Me.ToolStrip1.SuspendLayout()
        Me.GroupBox8.SuspendLayout()
        Me.gbVar.SuspendLayout()
        Me.GroupBox10.SuspendLayout()
        CType(Me.dgDepVariables, System.ComponentModel.ISupportInitialize).BeginInit()
        Me.ToolStrip2.SuspendLayout()
        Me.Panel1.SuspendLayout()
        CType(Me.PictureBox1, System.ComponentModel.ISupportInitialize).BeginInit()
        Me.SuspendLayout()
        '
        'btnRun
        '
        resources.ApplyResources(Me.btnRun, "btnRun")
        Me.btnRun.ImageKey = Global.DWSIM.My.Resources.DWSIM.NewVersionAvailable
        Me.btnRun.Name = "btnRun"
        Me.btnRun.UseVisualStyleBackColor = True
        '
        'Label1
        '
        resources.ApplyResources(Me.Label1, "Label1")
        Me.Label1.ImageKey = Global.DWSIM.My.Resources.DWSIM.NewVersionAvailable
        Me.Label1.Name = "Label1"
        '
        'cbObjIndVar1
        '
        resources.ApplyResources(Me.cbObjIndVar1, "cbObjIndVar1")
        Me.cbObjIndVar1.DropDownStyle = System.Windows.Forms.ComboBoxStyle.DropDownList
        Me.cbObjIndVar1.DropDownWidth = 250
        Me.cbObjIndVar1.FormattingEnabled = True
        Me.cbObjIndVar1.Name = "cbObjIndVar1"
        '
        'GroupBox1
        '
        resources.ApplyResources(Me.GroupBox1, "GroupBox1")
        Me.GroupBox1.Controls.Add(Me.GroupBox7)
        Me.GroupBox1.Controls.Add(Me.dgvResults)
        Me.GroupBox1.Name = "GroupBox1"
        Me.GroupBox1.TabStop = False
        '
        'GroupBox7
        '
        resources.ApplyResources(Me.GroupBox7, "GroupBox7")
        Me.GroupBox7.Controls.Add(Me.tbStats)
        Me.GroupBox7.Name = "GroupBox7"
        Me.GroupBox7.TabStop = False
        '
        'tbStats
        '
        resources.ApplyResources(Me.tbStats, "tbStats")
        Me.tbStats.Name = "tbStats"
        Me.tbStats.ReadOnly = True
        '
        'dgvResults
        '
        resources.ApplyResources(Me.dgvResults, "dgvResults")
        Me.dgvResults.AllowUserToAddRows = False
        Me.dgvResults.AllowUserToDeleteRows = False
        Me.dgvResults.AutoSizeColumnsMode = System.Windows.Forms.DataGridViewAutoSizeColumnsMode.Fill
        Me.dgvResults.ClipboardCopyMode = System.Windows.Forms.DataGridViewClipboardCopyMode.EnableAlwaysIncludeHeaderText
        DataGridViewCellStyle6.Alignment = System.Windows.Forms.DataGridViewContentAlignment.MiddleCenter
        DataGridViewCellStyle6.BackColor = System.Drawing.SystemColors.Control
        DataGridViewCellStyle6.Font = New System.Drawing.Font("Microsoft Sans Serif", 8.25!, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, CType(0, Byte))
        DataGridViewCellStyle6.ForeColor = System.Drawing.SystemColors.WindowText
        DataGridViewCellStyle6.SelectionBackColor = System.Drawing.SystemColors.Highlight
        DataGridViewCellStyle6.SelectionForeColor = System.Drawing.SystemColors.HighlightText
        DataGridViewCellStyle6.WrapMode = System.Windows.Forms.DataGridViewTriState.[True]
        Me.dgvResults.ColumnHeadersDefaultCellStyle = DataGridViewCellStyle6
        Me.dgvResults.ColumnHeadersHeightSizeMode = System.Windows.Forms.DataGridViewColumnHeadersHeightSizeMode.AutoSize
        Me.dgvResults.Columns.AddRange(New System.Windows.Forms.DataGridViewColumn() {Me.Column1, Me.Column2})
        Me.dgvResults.Name = "dgvResults"
        Me.dgvResults.ReadOnly = True
        Me.dgvResults.RowHeadersVisible = False
        Me.dgvResults.RowHeadersWidthSizeMode = System.Windows.Forms.DataGridViewRowHeadersWidthSizeMode.DisableResizing
        Me.dgvResults.SelectionMode = System.Windows.Forms.DataGridViewSelectionMode.CellSelect
        '
        'Column1
        '
        DataGridViewCellStyle7.Alignment = System.Windows.Forms.DataGridViewContentAlignment.MiddleCenter
        Me.Column1.DefaultCellStyle = DataGridViewCellStyle7
        resources.ApplyResources(Me.Column1, "Column1")
        Me.Column1.Name = "Column1"
        Me.Column1.ReadOnly = True
        Me.Column1.ToolTipText = Global.DWSIM.My.Resources.DWSIM.NewVersionAvailable
        '
        'Column2
        '
        DataGridViewCellStyle8.Alignment = System.Windows.Forms.DataGridViewContentAlignment.MiddleCenter
        Me.Column2.DefaultCellStyle = DataGridViewCellStyle8
        resources.ApplyResources(Me.Column2, "Column2")
        Me.Column2.Name = "Column2"
        Me.Column2.ReadOnly = True
        Me.Column2.ToolTipText = Global.DWSIM.My.Resources.DWSIM.NewVersionAvailable
        '
        'GroupBox2
        '
        resources.ApplyResources(Me.GroupBox2, "GroupBox2")
        Me.GroupBox2.Controls.Add(Me.btnDeleteCase)
        Me.GroupBox2.Controls.Add(Me.btnSaveCase)
        Me.GroupBox2.Controls.Add(Me.btnCopyCase)
        Me.GroupBox2.Controls.Add(Me.btnNewCase)
        Me.GroupBox2.Controls.Add(Me.lbCases)
        Me.GroupBox2.Name = "GroupBox2"
        Me.GroupBox2.TabStop = False
        '
        'btnDeleteCase
        '
        resources.ApplyResources(Me.btnDeleteCase, "btnDeleteCase")
        Me.btnDeleteCase.ImageKey = Global.DWSIM.My.Resources.DWSIM.NewVersionAvailable
        Me.btnDeleteCase.Name = "btnDeleteCase"
        Me.btnDeleteCase.UseVisualStyleBackColor = True
        '
        'btnSaveCase
        '
        resources.ApplyResources(Me.btnSaveCase, "btnSaveCase")
        Me.btnSaveCase.ImageKey = Global.DWSIM.My.Resources.DWSIM.NewVersionAvailable
        Me.btnSaveCase.Name = "btnSaveCase"
        Me.btnSaveCase.UseVisualStyleBackColor = True
        '
        'btnCopyCase
        '
        resources.ApplyResources(Me.btnCopyCase, "btnCopyCase")
        Me.btnCopyCase.ImageKey = Global.DWSIM.My.Resources.DWSIM.NewVersionAvailable
        Me.btnCopyCase.Name = "btnCopyCase"
        Me.btnCopyCase.UseVisualStyleBackColor = True
        '
        'btnNewCase
        '
        resources.ApplyResources(Me.btnNewCase, "btnNewCase")
        Me.btnNewCase.ImageKey = Global.DWSIM.My.Resources.DWSIM.NewVersionAvailable
        Me.btnNewCase.Name = "btnNewCase"
        Me.btnNewCase.UseVisualStyleBackColor = True
        '
        'lbCases
        '
        resources.ApplyResources(Me.lbCases, "lbCases")
        Me.lbCases.FormattingEnabled = True
        Me.lbCases.Name = "lbCases"
        '
        'GroupBox3
        '
        resources.ApplyResources(Me.GroupBox3, "GroupBox3")
        Me.GroupBox3.Controls.Add(Me.tbCaseDesc)
        Me.GroupBox3.Controls.Add(Me.Label4)
        Me.GroupBox3.Controls.Add(Me.tbCaseName)
        Me.GroupBox3.Controls.Add(Me.Label1)
        Me.GroupBox3.Name = "GroupBox3"
        Me.GroupBox3.TabStop = False
        '
        'tbCaseDesc
        '
        resources.ApplyResources(Me.tbCaseDesc, "tbCaseDesc")
        Me.tbCaseDesc.Name = "tbCaseDesc"
        '
        'Label4
        '
        resources.ApplyResources(Me.Label4, "Label4")
        Me.Label4.ImageKey = Global.DWSIM.My.Resources.DWSIM.NewVersionAvailable
        Me.Label4.Name = "Label4"
        '
        'tbCaseName
        '
        resources.ApplyResources(Me.tbCaseName, "tbCaseName")
        Me.tbCaseName.Name = "tbCaseName"
        '
        'Label3
        '
        resources.ApplyResources(Me.Label3, "Label3")
        Me.Label3.ImageKey = Global.DWSIM.My.Resources.DWSIM.NewVersionAvailable
        Me.Label3.Name = "Label3"
        '
        'GroupBox4
        '
        resources.ApplyResources(Me.GroupBox4, "GroupBox4")
        Me.GroupBox4.Controls.Add(Me.tbUpperLimIndVar1)
        Me.GroupBox4.Controls.Add(Me.Label8)
        Me.GroupBox4.Controls.Add(Me.tbUnitIndVar1)
        Me.GroupBox4.Controls.Add(Me.Label2)
        Me.GroupBox4.Controls.Add(Me.tbLowerLimIndVar1)
        Me.GroupBox4.Controls.Add(Me.Label7)
        Me.GroupBox4.Controls.Add(Me.nuNumPointsIndVar1)
        Me.GroupBox4.Controls.Add(Me.Label6)
        Me.GroupBox4.Controls.Add(Me.cbPropIndVar1)
        Me.GroupBox4.Controls.Add(Me.Label5)
        Me.GroupBox4.Controls.Add(Me.cbObjIndVar1)
        Me.GroupBox4.Controls.Add(Me.Label3)
        Me.GroupBox4.Name = "GroupBox4"
        Me.GroupBox4.TabStop = False
        '
        'tbUpperLimIndVar1
        '
        resources.ApplyResources(Me.tbUpperLimIndVar1, "tbUpperLimIndVar1")
        Me.tbUpperLimIndVar1.Name = "tbUpperLimIndVar1"
        '
        'Label8
        '
        resources.ApplyResources(Me.Label8, "Label8")
        Me.Label8.ImageKey = Global.DWSIM.My.Resources.DWSIM.NewVersionAvailable
        Me.Label8.Name = "Label8"
        '
        'tbUnitIndVar1
        '
        resources.ApplyResources(Me.tbUnitIndVar1, "tbUnitIndVar1")
        Me.tbUnitIndVar1.Name = "tbUnitIndVar1"
        Me.tbUnitIndVar1.ReadOnly = True
        Me.tbUnitIndVar1.TabStop = False
        '
        'Label2
        '
        resources.ApplyResources(Me.Label2, "Label2")
        Me.Label2.ImageKey = Global.DWSIM.My.Resources.DWSIM.NewVersionAvailable
        Me.Label2.Name = "Label2"
        '
        'tbLowerLimIndVar1
        '
        resources.ApplyResources(Me.tbLowerLimIndVar1, "tbLowerLimIndVar1")
        Me.tbLowerLimIndVar1.Name = "tbLowerLimIndVar1"
        '
        'Label7
        '
        resources.ApplyResources(Me.Label7, "Label7")
        Me.Label7.ImageKey = Global.DWSIM.My.Resources.DWSIM.NewVersionAvailable
        Me.Label7.Name = "Label7"
        '
        'nuNumPointsIndVar1
        '
        resources.ApplyResources(Me.nuNumPointsIndVar1, "nuNumPointsIndVar1")
        Me.nuNumPointsIndVar1.Minimum = New Decimal(New Integer() {1, 0, 0, 0})
        Me.nuNumPointsIndVar1.Name = "nuNumPointsIndVar1"
        Me.nuNumPointsIndVar1.Value = New Decimal(New Integer() {10, 0, 0, 0})
        '
        'Label6
        '
        resources.ApplyResources(Me.Label6, "Label6")
        Me.Label6.ImageKey = Global.DWSIM.My.Resources.DWSIM.NewVersionAvailable
        Me.Label6.Name = "Label6"
        '
        'cbPropIndVar1
        '
        resources.ApplyResources(Me.cbPropIndVar1, "cbPropIndVar1")
        Me.cbPropIndVar1.DropDownStyle = System.Windows.Forms.ComboBoxStyle.DropDownList
        Me.cbPropIndVar1.DropDownWidth = 250
        Me.cbPropIndVar1.FormattingEnabled = True
        Me.cbPropIndVar1.Name = "cbPropIndVar1"
        '
        'Label5
        '
        resources.ApplyResources(Me.Label5, "Label5")
        Me.Label5.ImageKey = Global.DWSIM.My.Resources.DWSIM.NewVersionAvailable
        Me.Label5.Name = "Label5"
        '
        'GroupBox5
        '
        resources.ApplyResources(Me.GroupBox5, "GroupBox5")
        Me.GroupBox5.Controls.Add(Me.pnIndvar2)
        Me.GroupBox5.Controls.Add(Me.chkIndVar2)
        Me.GroupBox5.Name = "GroupBox5"
        Me.GroupBox5.TabStop = False
        '
        'pnIndvar2
        '
        resources.ApplyResources(Me.pnIndvar2, "pnIndvar2")
        Me.pnIndvar2.Controls.Add(Me.Label14)
        Me.pnIndvar2.Controls.Add(Me.cbObjIndVar2)
        Me.pnIndvar2.Controls.Add(Me.tbUpperLimIndVar2)
        Me.pnIndvar2.Controls.Add(Me.Label13)
        Me.pnIndvar2.Controls.Add(Me.Label9)
        Me.pnIndvar2.Controls.Add(Me.cbPropIndVar2)
        Me.pnIndvar2.Controls.Add(Me.tbUnitIndVar2)
        Me.pnIndvar2.Controls.Add(Me.Label12)
        Me.pnIndvar2.Controls.Add(Me.Label10)
        Me.pnIndvar2.Controls.Add(Me.nuNumPointsIndVar2)
        Me.pnIndvar2.Controls.Add(Me.tbLowerLimIndVar2)
        Me.pnIndvar2.Controls.Add(Me.Label11)
        Me.pnIndvar2.Name = "pnIndvar2"
        '
        'Label14
        '
        resources.ApplyResources(Me.Label14, "Label14")
        Me.Label14.ImageKey = Global.DWSIM.My.Resources.DWSIM.NewVersionAvailable
        Me.Label14.Name = "Label14"
        '
        'cbObjIndVar2
        '
        resources.ApplyResources(Me.cbObjIndVar2, "cbObjIndVar2")
        Me.cbObjIndVar2.DropDownStyle = System.Windows.Forms.ComboBoxStyle.DropDownList
        Me.cbObjIndVar2.DropDownWidth = 250
        Me.cbObjIndVar2.FormattingEnabled = True
        Me.cbObjIndVar2.Name = "cbObjIndVar2"
        '
        'tbUpperLimIndVar2
        '
        resources.ApplyResources(Me.tbUpperLimIndVar2, "tbUpperLimIndVar2")
        Me.tbUpperLimIndVar2.Name = "tbUpperLimIndVar2"
        '
        'Label13
        '
        resources.ApplyResources(Me.Label13, "Label13")
        Me.Label13.ImageKey = Global.DWSIM.My.Resources.DWSIM.NewVersionAvailable
        Me.Label13.Name = "Label13"
        '
        'Label9
        '
        resources.ApplyResources(Me.Label9, "Label9")
        Me.Label9.ImageKey = Global.DWSIM.My.Resources.DWSIM.NewVersionAvailable
        Me.Label9.Name = "Label9"
        '
        'cbPropIndVar2
        '
        resources.ApplyResources(Me.cbPropIndVar2, "cbPropIndVar2")
        Me.cbPropIndVar2.DropDownStyle = System.Windows.Forms.ComboBoxStyle.DropDownList
        Me.cbPropIndVar2.DropDownWidth = 250
        Me.cbPropIndVar2.FormattingEnabled = True
        Me.cbPropIndVar2.Name = "cbPropIndVar2"
        '
        'tbUnitIndVar2
        '
        resources.ApplyResources(Me.tbUnitIndVar2, "tbUnitIndVar2")
        Me.tbUnitIndVar2.Name = "tbUnitIndVar2"
        Me.tbUnitIndVar2.ReadOnly = True
        Me.tbUnitIndVar2.TabStop = False
        '
        'Label12
        '
        resources.ApplyResources(Me.Label12, "Label12")
        Me.Label12.ImageKey = Global.DWSIM.My.Resources.DWSIM.NewVersionAvailable
        Me.Label12.Name = "Label12"
        '
        'Label10
        '
        resources.ApplyResources(Me.Label10, "Label10")
        Me.Label10.ImageKey = Global.DWSIM.My.Resources.DWSIM.NewVersionAvailable
        Me.Label10.Name = "Label10"
        '
        'nuNumPointsIndVar2
        '
        resources.ApplyResources(Me.nuNumPointsIndVar2, "nuNumPointsIndVar2")
        Me.nuNumPointsIndVar2.Minimum = New Decimal(New Integer() {1, 0, 0, 0})
        Me.nuNumPointsIndVar2.Name = "nuNumPointsIndVar2"
        Me.nuNumPointsIndVar2.Value = New Decimal(New Integer() {10, 0, 0, 0})
        '
        'tbLowerLimIndVar2
        '
        resources.ApplyResources(Me.tbLowerLimIndVar2, "tbLowerLimIndVar2")
        Me.tbLowerLimIndVar2.Name = "tbLowerLimIndVar2"
        '
        'Label11
        '
        resources.ApplyResources(Me.Label11, "Label11")
        Me.Label11.ImageKey = Global.DWSIM.My.Resources.DWSIM.NewVersionAvailable
        Me.Label11.Name = "Label11"
        '
        'chkIndVar2
        '
        resources.ApplyResources(Me.chkIndVar2, "chkIndVar2")
        Me.chkIndVar2.ImageKey = Global.DWSIM.My.Resources.DWSIM.NewVersionAvailable
        Me.chkIndVar2.Name = "chkIndVar2"
        Me.chkIndVar2.UseVisualStyleBackColor = True
        '
        'GroupBox6
        '
        resources.ApplyResources(Me.GroupBox6, "GroupBox6")
        Me.GroupBox6.Controls.Add(Me.rbExp)
        Me.GroupBox6.Controls.Add(Me.rbVar)
        Me.GroupBox6.Controls.Add(Me.gbExp)
        Me.GroupBox6.Controls.Add(Me.gbVar)
        Me.GroupBox6.Name = "GroupBox6"
        Me.GroupBox6.TabStop = False
        '
        'rbExp
        '
        resources.ApplyResources(Me.rbExp, "rbExp")
        Me.rbExp.ImageKey = Global.DWSIM.My.Resources.DWSIM.NewVersionAvailable
        Me.rbExp.Name = "rbExp"
        Me.rbExp.UseVisualStyleBackColor = True
        '
        'rbVar
        '
        resources.ApplyResources(Me.rbVar, "rbVar")
        Me.rbVar.Checked = True
        Me.rbVar.ImageKey = Global.DWSIM.My.Resources.DWSIM.NewVersionAvailable
        Me.rbVar.Name = "rbVar"
        Me.rbVar.TabStop = True
        Me.rbVar.UseVisualStyleBackColor = True
        '
        'gbExp
        '
        resources.ApplyResources(Me.gbExp, "gbExp")
        Me.gbExp.Controls.Add(Me.GroupBox9)
        Me.gbExp.Controls.Add(Me.GroupBox8)
        Me.gbExp.Name = "gbExp"
        Me.gbExp.TabStop = False
        '
        'GroupBox9
        '
        resources.ApplyResources(Me.GroupBox9, "GroupBox9")
        Me.GroupBox9.Controls.Add(Me.dgVariables)
        Me.GroupBox9.Controls.Add(Me.ToolStrip1)
        Me.GroupBox9.Name = "GroupBox9"
        Me.GroupBox9.TabStop = False
        '
        'dgVariables
        '
        resources.ApplyResources(Me.dgVariables, "dgVariables")
        Me.dgVariables.AllowUserToAddRows = False
        Me.dgVariables.AllowUserToDeleteRows = False
        Me.dgVariables.AllowUserToResizeRows = False
        Me.dgVariables.AutoSizeColumnsMode = System.Windows.Forms.DataGridViewAutoSizeColumnsMode.Fill
        Me.dgVariables.AutoSizeRowsMode = System.Windows.Forms.DataGridViewAutoSizeRowsMode.DisplayedCells
        Me.dgVariables.ClipboardCopyMode = System.Windows.Forms.DataGridViewClipboardCopyMode.EnableAlwaysIncludeHeaderText
        Me.dgVariables.ColumnHeadersBorderStyle = System.Windows.Forms.DataGridViewHeaderBorderStyle.[Single]
        DataGridViewCellStyle9.Alignment = System.Windows.Forms.DataGridViewContentAlignment.MiddleCenter
        DataGridViewCellStyle9.BackColor = System.Drawing.SystemColors.Control
        DataGridViewCellStyle9.Font = New System.Drawing.Font("Microsoft Sans Serif", 8.25!, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, CType(0, Byte))
        DataGridViewCellStyle9.ForeColor = System.Drawing.SystemColors.WindowText
        DataGridViewCellStyle9.SelectionBackColor = System.Drawing.SystemColors.Highlight
        DataGridViewCellStyle9.SelectionForeColor = System.Drawing.SystemColors.HighlightText
        Me.dgVariables.ColumnHeadersDefaultCellStyle = DataGridViewCellStyle9
        Me.dgVariables.ColumnHeadersHeightSizeMode = System.Windows.Forms.DataGridViewColumnHeadersHeightSizeMode.AutoSize
        Me.dgVariables.Columns.AddRange(New System.Windows.Forms.DataGridViewColumn() {Me.Column8, Me.DataGridViewTextBoxColumn1, Me.DataGridViewComboBoxColumn2, Me.Column4, Me.Column10, Me.Column9})
        Me.dgVariables.Name = "dgVariables"
        Me.dgVariables.RowHeadersBorderStyle = System.Windows.Forms.DataGridViewHeaderBorderStyle.[Single]
        '
        'Column8
        '
        resources.ApplyResources(Me.Column8, "Column8")
        Me.Column8.Name = "Column8"
        Me.Column8.SortMode = System.Windows.Forms.DataGridViewColumnSortMode.NotSortable
        Me.Column8.ToolTipText = Global.DWSIM.My.Resources.DWSIM.NewVersionAvailable
        '
        'DataGridViewTextBoxColumn1
        '
        Me.DataGridViewTextBoxColumn1.FillWeight = 20.0!
        resources.ApplyResources(Me.DataGridViewTextBoxColumn1, "DataGridViewTextBoxColumn1")
        Me.DataGridViewTextBoxColumn1.Name = "DataGridViewTextBoxColumn1"
        Me.DataGridViewTextBoxColumn1.SortMode = System.Windows.Forms.DataGridViewColumnSortMode.NotSortable
        Me.DataGridViewTextBoxColumn1.ToolTipText = Global.DWSIM.My.Resources.DWSIM.NewVersionAvailable
        '
        'DataGridViewComboBoxColumn2
        '
        Me.DataGridViewComboBoxColumn2.DisplayStyle = System.Windows.Forms.DataGridViewComboBoxDisplayStyle.ComboBox
        Me.DataGridViewComboBoxColumn2.FillWeight = 30.0!
        resources.ApplyResources(Me.DataGridViewComboBoxColumn2, "DataGridViewComboBoxColumn2")
        Me.DataGridViewComboBoxColumn2.Name = "DataGridViewComboBoxColumn2"
        Me.DataGridViewComboBoxColumn2.Sorted = True
        Me.DataGridViewComboBoxColumn2.ToolTipText = Global.DWSIM.My.Resources.DWSIM.NewVersionAvailable
        '
        'Column4
        '
        Me.Column4.DisplayStyle = System.Windows.Forms.DataGridViewComboBoxDisplayStyle.ComboBox
        Me.Column4.FillWeight = 40.0!
        resources.ApplyResources(Me.Column4, "Column4")
        Me.Column4.Name = "Column4"
        Me.Column4.ToolTipText = Global.DWSIM.My.Resources.DWSIM.NewVersionAvailable
        '
        'Column10
        '
        Me.Column10.FillWeight = 20.0!
        resources.ApplyResources(Me.Column10, "Column10")
        Me.Column10.Name = "Column10"
        Me.Column10.ReadOnly = True
        Me.Column10.SortMode = System.Windows.Forms.DataGridViewColumnSortMode.NotSortable
        Me.Column10.ToolTipText = Global.DWSIM.My.Resources.DWSIM.NewVersionAvailable
        '
        'Column9
        '
        Me.Column9.FillWeight = 10.0!
        resources.ApplyResources(Me.Column9, "Column9")
        Me.Column9.Name = "Column9"
        Me.Column9.ReadOnly = True
        Me.Column9.SortMode = System.Windows.Forms.DataGridViewColumnSortMode.NotSortable
        Me.Column9.ToolTipText = Global.DWSIM.My.Resources.DWSIM.NewVersionAvailable
        '
        'ToolStrip1
        '
        resources.ApplyResources(Me.ToolStrip1, "ToolStrip1")
        Me.ToolStrip1.GripStyle = System.Windows.Forms.ToolStripGripStyle.Hidden
        Me.ToolStrip1.Items.AddRange(New System.Windows.Forms.ToolStripItem() {Me.tsbAddVar, Me.tsbDelVar})
        Me.ToolStrip1.Name = "ToolStrip1"
        '
        'tsbAddVar
        '
        resources.ApplyResources(Me.tsbAddVar, "tsbAddVar")
        Me.tsbAddVar.DisplayStyle = System.Windows.Forms.ToolStripItemDisplayStyle.Image
        Me.tsbAddVar.Image = Global.DWSIM.My.Resources.Resources.add
        Me.tsbAddVar.Name = "tsbAddVar"
        Me.tsbAddVar.Text = Global.DWSIM.My.Resources.DWSIM.NewVersionAvailable
        '
        'tsbDelVar
        '
        resources.ApplyResources(Me.tsbDelVar, "tsbDelVar")
        Me.tsbDelVar.DisplayStyle = System.Windows.Forms.ToolStripItemDisplayStyle.Image
        Me.tsbDelVar.Image = Global.DWSIM.My.Resources.Resources.delete1
        Me.tsbDelVar.Name = "tsbDelVar"
        '
        'GroupBox8
        '
        resources.ApplyResources(Me.GroupBox8, "GroupBox8")
        Me.GroupBox8.Controls.Add(Me.btnClear)
        Me.GroupBox8.Controls.Add(Me.btnVerify)
        Me.GroupBox8.Controls.Add(Me.tbCurrentValue)
        Me.GroupBox8.Controls.Add(Me.Label15)
        Me.GroupBox8.Controls.Add(Me.tbExpression)
        Me.GroupBox8.Controls.Add(Me.Label17)
        Me.GroupBox8.Name = "GroupBox8"
        Me.GroupBox8.TabStop = False
        '
        'btnClear
        '
        resources.ApplyResources(Me.btnClear, "btnClear")
        Me.btnClear.Image = Global.DWSIM.My.Resources.Resources.cross
        Me.btnClear.ImageKey = Global.DWSIM.My.Resources.DWSIM.NewVersionAvailable
        Me.btnClear.Name = "btnClear"
        Me.btnClear.UseVisualStyleBackColor = True
        '
        'btnVerify
        '
        resources.ApplyResources(Me.btnVerify, "btnVerify")
        Me.btnVerify.Image = Global.DWSIM.My.Resources.Resources.tick1
        Me.btnVerify.ImageKey = Global.DWSIM.My.Resources.DWSIM.NewVersionAvailable
        Me.btnVerify.Name = "btnVerify"
        Me.btnVerify.UseVisualStyleBackColor = True
        '
        'tbCurrentValue
        '
        resources.ApplyResources(Me.tbCurrentValue, "tbCurrentValue")
        Me.tbCurrentValue.Name = "tbCurrentValue"
        Me.tbCurrentValue.ReadOnly = True
        '
        'Label15
        '
        resources.ApplyResources(Me.Label15, "Label15")
        Me.Label15.ImageKey = Global.DWSIM.My.Resources.DWSIM.NewVersionAvailable
        Me.Label15.Name = "Label15"
        '
        'tbExpression
        '
        resources.ApplyResources(Me.tbExpression, "tbExpression")
        Me.tbExpression.Name = "tbExpression"
        '
        'Label17
        '
        resources.ApplyResources(Me.Label17, "Label17")
        Me.Label17.ImageKey = Global.DWSIM.My.Resources.DWSIM.NewVersionAvailable
        Me.Label17.Name = "Label17"
        '
        'gbVar
        '
        resources.ApplyResources(Me.gbVar, "gbVar")
        Me.gbVar.Controls.Add(Me.GroupBox10)
        Me.gbVar.Name = "gbVar"
        Me.gbVar.TabStop = False
        '
        'GroupBox10
        '
        resources.ApplyResources(Me.GroupBox10, "GroupBox10")
        Me.GroupBox10.Controls.Add(Me.dgDepVariables)
        Me.GroupBox10.Controls.Add(Me.ToolStrip2)
        Me.GroupBox10.Name = "GroupBox10"
        Me.GroupBox10.TabStop = False
        '
        'dgDepVariables
        '
        resources.ApplyResources(Me.dgDepVariables, "dgDepVariables")
        Me.dgDepVariables.AllowUserToAddRows = False
        Me.dgDepVariables.AllowUserToDeleteRows = False
        Me.dgDepVariables.AllowUserToResizeRows = False
        Me.dgDepVariables.AutoSizeColumnsMode = System.Windows.Forms.DataGridViewAutoSizeColumnsMode.Fill
        Me.dgDepVariables.AutoSizeRowsMode = System.Windows.Forms.DataGridViewAutoSizeRowsMode.DisplayedCells
        Me.dgDepVariables.ClipboardCopyMode = System.Windows.Forms.DataGridViewClipboardCopyMode.EnableAlwaysIncludeHeaderText
        Me.dgDepVariables.ColumnHeadersBorderStyle = System.Windows.Forms.DataGridViewHeaderBorderStyle.[Single]
        DataGridViewCellStyle10.Alignment = System.Windows.Forms.DataGridViewContentAlignment.MiddleCenter
        DataGridViewCellStyle10.BackColor = System.Drawing.SystemColors.Control
        DataGridViewCellStyle10.Font = New System.Drawing.Font("Microsoft Sans Serif", 8.25!, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, CType(0, Byte))
        DataGridViewCellStyle10.ForeColor = System.Drawing.SystemColors.WindowText
        DataGridViewCellStyle10.SelectionBackColor = System.Drawing.SystemColors.Highlight
        DataGridViewCellStyle10.SelectionForeColor = System.Drawing.SystemColors.HighlightText
        Me.dgDepVariables.ColumnHeadersDefaultCellStyle = DataGridViewCellStyle10
        Me.dgDepVariables.ColumnHeadersHeightSizeMode = System.Windows.Forms.DataGridViewColumnHeadersHeightSizeMode.AutoSize
        Me.dgDepVariables.Columns.AddRange(New System.Windows.Forms.DataGridViewColumn() {Me.DataGridViewTextBoxColumn2, Me.DataGridViewComboBoxColumn1, Me.DataGridViewComboBoxColumn3, Me.DataGridViewTextBoxColumn5})
        Me.dgDepVariables.Name = "dgDepVariables"
        Me.dgDepVariables.RowHeadersBorderStyle = System.Windows.Forms.DataGridViewHeaderBorderStyle.[Single]
        '
        'DataGridViewTextBoxColumn2
        '
        resources.ApplyResources(Me.DataGridViewTextBoxColumn2, "DataGridViewTextBoxColumn2")
        Me.DataGridViewTextBoxColumn2.Name = "DataGridViewTextBoxColumn2"
        Me.DataGridViewTextBoxColumn2.SortMode = System.Windows.Forms.DataGridViewColumnSortMode.NotSortable
        Me.DataGridViewTextBoxColumn2.ToolTipText = Global.DWSIM.My.Resources.DWSIM.NewVersionAvailable
        '
        'DataGridViewComboBoxColumn1
        '
        Me.DataGridViewComboBoxColumn1.DisplayStyle = System.Windows.Forms.DataGridViewComboBoxDisplayStyle.ComboBox
        Me.DataGridViewComboBoxColumn1.FillWeight = 30.0!
        resources.ApplyResources(Me.DataGridViewComboBoxColumn1, "DataGridViewComboBoxColumn1")
        Me.DataGridViewComboBoxColumn1.Name = "DataGridViewComboBoxColumn1"
        Me.DataGridViewComboBoxColumn1.Sorted = True
        Me.DataGridViewComboBoxColumn1.ToolTipText = Global.DWSIM.My.Resources.DWSIM.NewVersionAvailable
        '
        'DataGridViewComboBoxColumn3
        '
        Me.DataGridViewComboBoxColumn3.DisplayStyle = System.Windows.Forms.DataGridViewComboBoxDisplayStyle.ComboBox
        Me.DataGridViewComboBoxColumn3.FillWeight = 60.0!
        resources.ApplyResources(Me.DataGridViewComboBoxColumn3, "DataGridViewComboBoxColumn3")
        Me.DataGridViewComboBoxColumn3.Name = "DataGridViewComboBoxColumn3"
        Me.DataGridViewComboBoxColumn3.ToolTipText = Global.DWSIM.My.Resources.DWSIM.NewVersionAvailable
        '
        'DataGridViewTextBoxColumn5
        '
        Me.DataGridViewTextBoxColumn5.FillWeight = 10.0!
        resources.ApplyResources(Me.DataGridViewTextBoxColumn5, "DataGridViewTextBoxColumn5")
        Me.DataGridViewTextBoxColumn5.Name = "DataGridViewTextBoxColumn5"
        Me.DataGridViewTextBoxColumn5.ReadOnly = True
        Me.DataGridViewTextBoxColumn5.SortMode = System.Windows.Forms.DataGridViewColumnSortMode.NotSortable
        Me.DataGridViewTextBoxColumn5.ToolTipText = Global.DWSIM.My.Resources.DWSIM.NewVersionAvailable
        '
        'ToolStrip2
        '
        resources.ApplyResources(Me.ToolStrip2, "ToolStrip2")
        Me.ToolStrip2.GripStyle = System.Windows.Forms.ToolStripGripStyle.Hidden
        Me.ToolStrip2.Items.AddRange(New System.Windows.Forms.ToolStripItem() {Me.ToolStripButton1, Me.ToolStripButton2})
        Me.ToolStrip2.Name = "ToolStrip2"
        '
        'ToolStripButton1
        '
        resources.ApplyResources(Me.ToolStripButton1, "ToolStripButton1")
        Me.ToolStripButton1.DisplayStyle = System.Windows.Forms.ToolStripItemDisplayStyle.Image
        Me.ToolStripButton1.Image = Global.DWSIM.My.Resources.Resources.add
        Me.ToolStripButton1.Name = "ToolStripButton1"
        Me.ToolStripButton1.Text = Global.DWSIM.My.Resources.DWSIM.NewVersionAvailable
        '
        'ToolStripButton2
        '
        resources.ApplyResources(Me.ToolStripButton2, "ToolStripButton2")
        Me.ToolStripButton2.DisplayStyle = System.Windows.Forms.ToolStripItemDisplayStyle.Image
        Me.ToolStripButton2.Image = Global.DWSIM.My.Resources.Resources.delete1
        Me.ToolStripButton2.Name = "ToolStripButton2"
        '
        'Panel1
        '
        resources.ApplyResources(Me.Panel1, "Panel1")
        Me.Panel1.BackColor = System.Drawing.SystemColors.Control
        Me.Panel1.BorderStyle = System.Windows.Forms.BorderStyle.FixedSingle
        Me.Panel1.Controls.Add(Me.btnAbort)
        Me.Panel1.Controls.Add(Me.GroupBox2)
        Me.Panel1.Controls.Add(Me.GroupBox6)
        Me.Panel1.Controls.Add(Me.btnRun)
        Me.Panel1.Controls.Add(Me.GroupBox5)
        Me.Panel1.Controls.Add(Me.GroupBox1)
        Me.Panel1.Controls.Add(Me.GroupBox4)
        Me.Panel1.Controls.Add(Me.GroupBox3)
        Me.Panel1.Name = "Panel1"
        '
        'btnAbort
        '
        resources.ApplyResources(Me.btnAbort, "btnAbort")
        Me.btnAbort.ImageKey = Global.DWSIM.My.Resources.DWSIM.NewVersionAvailable
        Me.btnAbort.Name = "btnAbort"
        Me.btnAbort.UseVisualStyleBackColor = True
        '
        'LinkLabel1
        '
        resources.ApplyResources(Me.LinkLabel1, "LinkLabel1")
        Me.LinkLabel1.ImageKey = Global.DWSIM.My.Resources.DWSIM.NewVersionAvailable
        Me.LinkLabel1.Name = "LinkLabel1"
        Me.LinkLabel1.TabStop = True
        Me.LinkLabel1.UseCompatibleTextRendering = True
        '
        'PictureBox1
        '
        resources.ApplyResources(Me.PictureBox1, "PictureBox1")
        Me.PictureBox1.Image = Global.DWSIM.My.Resources.Resources.dialog_information
        Me.PictureBox1.Name = "PictureBox1"
        Me.PictureBox1.TabStop = False
        '
        'FormSensAnalysis
        '
        resources.ApplyResources(Me, "$this")
        Me.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font
        Me.BackColor = System.Drawing.Color.White
        Me.Controls.Add(Me.PictureBox1)
        Me.Controls.Add(Me.LinkLabel1)
        Me.Controls.Add(Me.Panel1)
        Me.FormBorderStyle = System.Windows.Forms.FormBorderStyle.FixedToolWindow
        Me.Name = "FormSensAnalysis"
        Me.ShowIcon = False
        Me.ShowInTaskbar = False
        Me.GroupBox1.ResumeLayout(False)
        Me.GroupBox7.ResumeLayout(False)
        Me.GroupBox7.PerformLayout()
        CType(Me.dgvResults, System.ComponentModel.ISupportInitialize).EndInit()
        Me.GroupBox2.ResumeLayout(False)
        Me.GroupBox3.ResumeLayout(False)
        Me.GroupBox3.PerformLayout()
        Me.GroupBox4.ResumeLayout(False)
        Me.GroupBox4.PerformLayout()
        CType(Me.nuNumPointsIndVar1, System.ComponentModel.ISupportInitialize).EndInit()
        Me.GroupBox5.ResumeLayout(False)
        Me.GroupBox5.PerformLayout()
        Me.pnIndvar2.ResumeLayout(False)
        Me.pnIndvar2.PerformLayout()
        CType(Me.nuNumPointsIndVar2, System.ComponentModel.ISupportInitialize).EndInit()
        Me.GroupBox6.ResumeLayout(False)
        Me.GroupBox6.PerformLayout()
        Me.gbExp.ResumeLayout(False)
        Me.GroupBox9.ResumeLayout(False)
        Me.GroupBox9.PerformLayout()
        CType(Me.dgVariables, System.ComponentModel.ISupportInitialize).EndInit()
        Me.ToolStrip1.ResumeLayout(False)
        Me.ToolStrip1.PerformLayout()
        Me.GroupBox8.ResumeLayout(False)
        Me.GroupBox8.PerformLayout()
        Me.gbVar.ResumeLayout(False)
        Me.GroupBox10.ResumeLayout(False)
        Me.GroupBox10.PerformLayout()
        CType(Me.dgDepVariables, System.ComponentModel.ISupportInitialize).EndInit()
        Me.ToolStrip2.ResumeLayout(False)
        Me.ToolStrip2.PerformLayout()
        Me.Panel1.ResumeLayout(False)
        CType(Me.PictureBox1, System.ComponentModel.ISupportInitialize).EndInit()
        Me.ResumeLayout(False)
        Me.PerformLayout()

    End Sub
    Public WithEvents btnRun As System.Windows.Forms.Button
    Public WithEvents Label1 As System.Windows.Forms.Label
    Public WithEvents cbObjIndVar1 As System.Windows.Forms.ComboBox
    Public WithEvents GroupBox1 As System.Windows.Forms.GroupBox
    Public WithEvents dgvResults As System.Windows.Forms.DataGridView
    Public WithEvents GroupBox2 As System.Windows.Forms.GroupBox
    Public WithEvents btnDeleteCase As System.Windows.Forms.Button
    Public WithEvents btnSaveCase As System.Windows.Forms.Button
    Public WithEvents btnCopyCase As System.Windows.Forms.Button
    Public WithEvents btnNewCase As System.Windows.Forms.Button
    Public WithEvents lbCases As System.Windows.Forms.ListBox
    Public WithEvents GroupBox3 As System.Windows.Forms.GroupBox
    Public WithEvents tbCaseName As System.Windows.Forms.TextBox
    Public WithEvents Label3 As System.Windows.Forms.Label
    Public WithEvents tbCaseDesc As System.Windows.Forms.TextBox
    Public WithEvents Label4 As System.Windows.Forms.Label
    Public WithEvents GroupBox4 As System.Windows.Forms.GroupBox
    Public WithEvents cbPropIndVar1 As System.Windows.Forms.ComboBox
    Public WithEvents Label5 As System.Windows.Forms.Label
    Public WithEvents tbUpperLimIndVar1 As System.Windows.Forms.TextBox
    Public WithEvents Label8 As System.Windows.Forms.Label
    Public WithEvents tbUnitIndVar1 As System.Windows.Forms.TextBox
    Public WithEvents Label2 As System.Windows.Forms.Label
    Public WithEvents tbLowerLimIndVar1 As System.Windows.Forms.TextBox
    Public WithEvents Label7 As System.Windows.Forms.Label
    Public WithEvents nuNumPointsIndVar1 As System.Windows.Forms.NumericUpDown
    Public WithEvents Label6 As System.Windows.Forms.Label
    Public WithEvents GroupBox5 As System.Windows.Forms.GroupBox
    Public WithEvents tbUpperLimIndVar2 As System.Windows.Forms.TextBox
    Public WithEvents Label9 As System.Windows.Forms.Label
    Public WithEvents tbUnitIndVar2 As System.Windows.Forms.TextBox
    Public WithEvents Label10 As System.Windows.Forms.Label
    Public WithEvents tbLowerLimIndVar2 As System.Windows.Forms.TextBox
    Public WithEvents Label11 As System.Windows.Forms.Label
    Public WithEvents nuNumPointsIndVar2 As System.Windows.Forms.NumericUpDown
    Public WithEvents Label12 As System.Windows.Forms.Label
    Public WithEvents cbPropIndVar2 As System.Windows.Forms.ComboBox
    Public WithEvents Label13 As System.Windows.Forms.Label
    Public WithEvents cbObjIndVar2 As System.Windows.Forms.ComboBox
    Public WithEvents Label14 As System.Windows.Forms.Label
    Public WithEvents GroupBox6 As System.Windows.Forms.GroupBox
    Public WithEvents Panel1 As System.Windows.Forms.Panel
    Public WithEvents GroupBox7 As System.Windows.Forms.GroupBox
    Public WithEvents tbStats As System.Windows.Forms.TextBox
    Public WithEvents btnAbort As System.Windows.Forms.Button
    Public WithEvents LinkLabel1 As System.Windows.Forms.LinkLabel
    Public WithEvents PictureBox1 As System.Windows.Forms.PictureBox
    Public WithEvents chkIndVar2 As System.Windows.Forms.CheckBox
    Public WithEvents pnIndvar2 As System.Windows.Forms.Panel
    Public WithEvents GroupBox8 As System.Windows.Forms.GroupBox
    Public WithEvents btnClear As System.Windows.Forms.Button
    Public WithEvents btnVerify As System.Windows.Forms.Button
    Public WithEvents tbCurrentValue As System.Windows.Forms.TextBox
    Public WithEvents Label15 As System.Windows.Forms.Label
    Public WithEvents tbExpression As System.Windows.Forms.TextBox
    Public WithEvents Label17 As System.Windows.Forms.Label
    Public WithEvents GroupBox9 As System.Windows.Forms.GroupBox
    Public WithEvents dgVariables As System.Windows.Forms.DataGridView
    Public WithEvents ToolStrip1 As System.Windows.Forms.ToolStrip
    Public WithEvents tsbAddVar As System.Windows.Forms.ToolStripButton
    Public WithEvents tsbDelVar As System.Windows.Forms.ToolStripButton
    Public WithEvents gbExp As System.Windows.Forms.GroupBox
    Public WithEvents gbVar As System.Windows.Forms.GroupBox
    Public WithEvents rbVar As System.Windows.Forms.RadioButton
    Public WithEvents rbExp As System.Windows.Forms.RadioButton
    Public WithEvents GroupBox10 As System.Windows.Forms.GroupBox
    Public WithEvents dgDepVariables As System.Windows.Forms.DataGridView
    Public WithEvents ToolStrip2 As System.Windows.Forms.ToolStrip
    Public WithEvents ToolStripButton1 As System.Windows.Forms.ToolStripButton
    Public WithEvents ToolStripButton2 As System.Windows.Forms.ToolStripButton
    Friend WithEvents Column1 As System.Windows.Forms.DataGridViewTextBoxColumn
    Friend WithEvents Column2 As System.Windows.Forms.DataGridViewTextBoxColumn
    Friend WithEvents Column8 As System.Windows.Forms.DataGridViewTextBoxColumn
    Friend WithEvents DataGridViewTextBoxColumn1 As System.Windows.Forms.DataGridViewTextBoxColumn
    Friend WithEvents DataGridViewComboBoxColumn2 As System.Windows.Forms.DataGridViewComboBoxColumn
    Friend WithEvents Column4 As System.Windows.Forms.DataGridViewComboBoxColumn
    Friend WithEvents Column10 As System.Windows.Forms.DataGridViewTextBoxColumn
    Friend WithEvents Column9 As System.Windows.Forms.DataGridViewTextBoxColumn
    Friend WithEvents DataGridViewTextBoxColumn2 As System.Windows.Forms.DataGridViewTextBoxColumn
    Friend WithEvents DataGridViewComboBoxColumn1 As System.Windows.Forms.DataGridViewComboBoxColumn
    Friend WithEvents DataGridViewComboBoxColumn3 As System.Windows.Forms.DataGridViewComboBoxColumn
    Friend WithEvents DataGridViewTextBoxColumn5 As System.Windows.Forms.DataGridViewTextBoxColumn
End Class
