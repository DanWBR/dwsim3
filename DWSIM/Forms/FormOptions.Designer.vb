<Global.Microsoft.VisualBasic.CompilerServices.DesignerGenerated()> _
Partial Class FormOptions
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
        Dim resources As System.ComponentModel.ComponentResourceManager = New System.ComponentModel.ComponentResourceManager(GetType(FormOptions))
        Dim DataGridViewCellStyle1 As System.Windows.Forms.DataGridViewCellStyle = New System.Windows.Forms.DataGridViewCellStyle()
        Dim DataGridViewCellStyle2 As System.Windows.Forms.DataGridViewCellStyle = New System.Windows.Forms.DataGridViewCellStyle()
        Dim DataGridViewCellStyle3 As System.Windows.Forms.DataGridViewCellStyle = New System.Windows.Forms.DataGridViewCellStyle()
        Me.FaTabStrip1 = New FarsiLibrary.Win.FATabStrip()
        Me.FaTabStripItem1 = New FarsiLibrary.Win.FATabStripItem()
        Me.GroupBox2 = New System.Windows.Forms.GroupBox()
        Me.chkStorePreviousSolutions = New System.Windows.Forms.CheckBox()
        Me.chkSolverBreak = New System.Windows.Forms.CheckBox()
        Me.Label12 = New System.Windows.Forms.Label()
        Me.tbSolverTimeout = New System.Windows.Forms.TextBox()
        Me.Label11 = New System.Windows.Forms.Label()
        Me.GroupBoxAzureConfig = New System.Windows.Forms.GroupBox()
        Me.tbServiceBusNamespace = New System.Windows.Forms.TextBox()
        Me.Label8 = New System.Windows.Forms.Label()
        Me.cbSolverMode = New System.Windows.Forms.ComboBox()
        Me.Label2 = New System.Windows.Forms.Label()
        Me.GroupBoxNetworkComputerConfig = New System.Windows.Forms.GroupBox()
        Me.tbServerPort = New System.Windows.Forms.TextBox()
        Me.tbServerIP = New System.Windows.Forms.TextBox()
        Me.Label15 = New System.Windows.Forms.Label()
        Me.Label16 = New System.Windows.Forms.Label()
        Me.GroupBox7 = New System.Windows.Forms.GroupBox()
        Me.GroupBox8 = New System.Windows.Forms.GroupBox()
        Me.tbGPUCaps = New System.Windows.Forms.TextBox()
        Me.cbGPU = New System.Windows.Forms.ComboBox()
        Me.Label7 = New System.Windows.Forms.Label()
        Me.chkEnableGPUProcessing = New System.Windows.Forms.CheckBox()
        Me.cbParallelism = New System.Windows.Forms.ComboBox()
        Me.Label6 = New System.Windows.Forms.Label()
        Me.chkEnableParallelCalcs = New System.Windows.Forms.CheckBox()
        Me.FaTabStripItem3 = New FarsiLibrary.Win.FATabStripItem()
        Me.GroupBox11 = New System.Windows.Forms.GroupBox()
        Me.cbudb = New System.Windows.Forms.CheckBox()
        Me.Button11 = New System.Windows.Forms.Button()
        Me.Button7 = New System.Windows.Forms.Button()
        Me.GroupBox4 = New System.Windows.Forms.GroupBox()
        Me.dgvdb = New System.Windows.Forms.DataGridView()
        Me.Column12 = New System.Windows.Forms.DataGridViewTextBoxColumn()
        Me.Column14 = New System.Windows.Forms.DataGridViewTextBoxColumn()
        Me.Column15 = New System.Windows.Forms.DataGridViewTextBoxColumn()
        Me.Column13 = New System.Windows.Forms.DataGridViewImageColumn()
        Me.BtnEdit = New System.Windows.Forms.DataGridViewImageColumn()
        Me.FaTabStripItem6 = New FarsiLibrary.Win.FATabStripItem()
        Me.GroupBox12 = New System.Windows.Forms.GroupBox()
        Me.Button4 = New System.Windows.Forms.Button()
        Me.GroupBox10 = New System.Windows.Forms.GroupBox()
        Me.dgvIPDB = New System.Windows.Forms.DataGridView()
        Me.DataGridViewTextBoxColumn1 = New System.Windows.Forms.DataGridViewTextBoxColumn()
        Me.DataGridViewTextBoxColumn2 = New System.Windows.Forms.DataGridViewTextBoxColumn()
        Me.DataGridViewTextBoxColumn3 = New System.Windows.Forms.DataGridViewTextBoxColumn()
        Me.DataGridViewImageColumn1 = New System.Windows.Forms.DataGridViewImageColumn()
        Me.FaTabStripItem2 = New FarsiLibrary.Win.FATabStripItem()
        Me.GroupBox3 = New System.Windows.Forms.GroupBox()
        Me.Button2 = New System.Windows.Forms.Button()
        Me.KryptonLabel3 = New System.Windows.Forms.Label()
        Me.TrackBar1 = New System.Windows.Forms.TrackBar()
        Me.KryptonLabel2 = New System.Windows.Forms.Label()
        Me.KryptonButton1 = New System.Windows.Forms.Button()
        Me.KryptonTextBox1 = New System.Windows.Forms.TextBox()
        Me.KryptonLabel1 = New System.Windows.Forms.Label()
        Me.KryptonCheckBox6 = New System.Windows.Forms.CheckBox()
        Me.FaTabStripItem5 = New FarsiLibrary.Win.FATabStripItem()
        Me.GroupBox5 = New System.Windows.Forms.GroupBox()
        Me.lbpaths = New System.Windows.Forms.ListBox()
        Me.Label5 = New System.Windows.Forms.Label()
        Me.btnrmpath = New System.Windows.Forms.Button()
        Me.btnaddpath = New System.Windows.Forms.Button()
        Me.tbaddpath = New System.Windows.Forms.TextBox()
        Me.Label4 = New System.Windows.Forms.Label()
        Me.FaTabStripItem7 = New FarsiLibrary.Win.FATabStripItem()
        Me.GroupBox1 = New System.Windows.Forms.GroupBox()
        Me.ComboBoxUILanguage = New System.Windows.Forms.ComboBox()
        Me.Label3 = New System.Windows.Forms.Label()
        Me.GroupBox9 = New System.Windows.Forms.GroupBox()
        Me.chkShowWhatsNew = New System.Windows.Forms.CheckBox()
        Me.KryptonCheckBox1 = New System.Windows.Forms.CheckBox()
        Me.chkUpdates = New System.Windows.Forms.CheckBox()
        Me.GroupBox6 = New System.Windows.Forms.GroupBox()
        Me.cbDebugLevel = New System.Windows.Forms.ComboBox()
        Me.Label13 = New System.Windows.Forms.Label()
        Me.chkconsole = New System.Windows.Forms.CheckBox()
        Me.ImageList1 = New System.Windows.Forms.ImageList(Me.components)
        Me.FolderBrowserDialog1 = New System.Windows.Forms.FolderBrowserDialog()
        Me.ofdcs = New System.Windows.Forms.OpenFileDialog()
        Me.OpenFileDialog1 = New System.Windows.Forms.OpenFileDialog()
        Me.SuperToolTip1 = New Omarslvd.Windows.Forms.SuperToolTip()
        CType(Me.FaTabStrip1, System.ComponentModel.ISupportInitialize).BeginInit()
        Me.FaTabStrip1.SuspendLayout()
        Me.FaTabStripItem1.SuspendLayout()
        Me.GroupBox2.SuspendLayout()
        Me.GroupBoxAzureConfig.SuspendLayout()
        Me.GroupBoxNetworkComputerConfig.SuspendLayout()
        Me.GroupBox7.SuspendLayout()
        Me.GroupBox8.SuspendLayout()
        Me.FaTabStripItem3.SuspendLayout()
        Me.GroupBox11.SuspendLayout()
        Me.GroupBox4.SuspendLayout()
        CType(Me.dgvdb, System.ComponentModel.ISupportInitialize).BeginInit()
        Me.FaTabStripItem6.SuspendLayout()
        Me.GroupBox12.SuspendLayout()
        Me.GroupBox10.SuspendLayout()
        CType(Me.dgvIPDB, System.ComponentModel.ISupportInitialize).BeginInit()
        Me.FaTabStripItem2.SuspendLayout()
        Me.GroupBox3.SuspendLayout()
        CType(Me.TrackBar1, System.ComponentModel.ISupportInitialize).BeginInit()
        Me.FaTabStripItem5.SuspendLayout()
        Me.GroupBox5.SuspendLayout()
        Me.FaTabStripItem7.SuspendLayout()
        Me.GroupBox1.SuspendLayout()
        Me.GroupBox9.SuspendLayout()
        Me.GroupBox6.SuspendLayout()
        Me.SuspendLayout()
        '
        'FaTabStrip1
        '
        Me.FaTabStrip1.AlwaysShowClose = False
        Me.FaTabStrip1.AlwaysShowMenuGlyph = False
        resources.ApplyResources(Me.FaTabStrip1, "FaTabStrip1")
        Me.FaTabStrip1.Items.AddRange(New FarsiLibrary.Win.FATabStripItem() {Me.FaTabStripItem1, Me.FaTabStripItem3, Me.FaTabStripItem6, Me.FaTabStripItem2, Me.FaTabStripItem5, Me.FaTabStripItem7})
        Me.FaTabStrip1.Name = "FaTabStrip1"
        Me.FaTabStrip1.SelectedItem = Me.FaTabStripItem1
        Me.SuperToolTip1.SetSuperToolTipColor1(Me.FaTabStrip1, System.Drawing.Color.Empty)
        Me.SuperToolTip1.SetSuperToolTipColor2(Me.FaTabStrip1, System.Drawing.Color.Empty)
        '
        'FaTabStripItem1
        '
        Me.FaTabStripItem1.CanClose = False
        Me.FaTabStripItem1.Controls.Add(Me.GroupBox2)
        Me.FaTabStripItem1.Controls.Add(Me.GroupBox7)
        Me.FaTabStripItem1.IsDrawn = True
        Me.FaTabStripItem1.Name = "FaTabStripItem1"
        Me.FaTabStripItem1.Selected = True
        resources.ApplyResources(Me.FaTabStripItem1, "FaTabStripItem1")
        Me.SuperToolTip1.SetSuperToolTipColor1(Me.FaTabStripItem1, System.Drawing.Color.Empty)
        Me.SuperToolTip1.SetSuperToolTipColor2(Me.FaTabStripItem1, System.Drawing.Color.Empty)
        '
        'GroupBox2
        '
        Me.GroupBox2.Controls.Add(Me.chkStorePreviousSolutions)
        Me.GroupBox2.Controls.Add(Me.chkSolverBreak)
        Me.GroupBox2.Controls.Add(Me.Label12)
        Me.GroupBox2.Controls.Add(Me.tbSolverTimeout)
        Me.GroupBox2.Controls.Add(Me.Label11)
        Me.GroupBox2.Controls.Add(Me.GroupBoxAzureConfig)
        Me.GroupBox2.Controls.Add(Me.cbSolverMode)
        Me.GroupBox2.Controls.Add(Me.Label2)
        Me.GroupBox2.Controls.Add(Me.GroupBoxNetworkComputerConfig)
        resources.ApplyResources(Me.GroupBox2, "GroupBox2")
        Me.GroupBox2.Name = "GroupBox2"
        Me.SuperToolTip1.SetSuperToolTipColor1(Me.GroupBox2, System.Drawing.Color.Empty)
        Me.SuperToolTip1.SetSuperToolTipColor2(Me.GroupBox2, System.Drawing.Color.Empty)
        Me.GroupBox2.TabStop = False
        '
        'chkStorePreviousSolutions
        '
        resources.ApplyResources(Me.chkStorePreviousSolutions, "chkStorePreviousSolutions")
        Me.chkStorePreviousSolutions.Name = "chkStorePreviousSolutions"
        Me.SuperToolTip1.SetSuperToolTipColor1(Me.chkStorePreviousSolutions, System.Drawing.Color.Empty)
        Me.SuperToolTip1.SetSuperToolTipColor2(Me.chkStorePreviousSolutions, System.Drawing.Color.Empty)
        '
        'chkSolverBreak
        '
        resources.ApplyResources(Me.chkSolverBreak, "chkSolverBreak")
        Me.chkSolverBreak.Name = "chkSolverBreak"
        Me.SuperToolTip1.SetSuperToolTipColor1(Me.chkSolverBreak, System.Drawing.Color.Empty)
        Me.SuperToolTip1.SetSuperToolTipColor2(Me.chkSolverBreak, System.Drawing.Color.Empty)
        '
        'Label12
        '
        resources.ApplyResources(Me.Label12, "Label12")
        Me.Label12.Name = "Label12"
        Me.SuperToolTip1.SetSuperToolTipColor1(Me.Label12, System.Drawing.Color.Empty)
        Me.SuperToolTip1.SetSuperToolTipColor2(Me.Label12, System.Drawing.Color.Empty)
        '
        'tbSolverTimeout
        '
        resources.ApplyResources(Me.tbSolverTimeout, "tbSolverTimeout")
        Me.tbSolverTimeout.Name = "tbSolverTimeout"
        Me.SuperToolTip1.SetSuperToolTipColor1(Me.tbSolverTimeout, System.Drawing.Color.Empty)
        Me.SuperToolTip1.SetSuperToolTipColor2(Me.tbSolverTimeout, System.Drawing.Color.Empty)
        '
        'Label11
        '
        resources.ApplyResources(Me.Label11, "Label11")
        Me.Label11.Name = "Label11"
        Me.SuperToolTip1.SetSuperToolTipColor1(Me.Label11, System.Drawing.Color.Empty)
        Me.SuperToolTip1.SetSuperToolTipColor2(Me.Label11, System.Drawing.Color.Empty)
        '
        'GroupBoxAzureConfig
        '
        Me.GroupBoxAzureConfig.Controls.Add(Me.tbServiceBusNamespace)
        Me.GroupBoxAzureConfig.Controls.Add(Me.Label8)
        resources.ApplyResources(Me.GroupBoxAzureConfig, "GroupBoxAzureConfig")
        Me.GroupBoxAzureConfig.Name = "GroupBoxAzureConfig"
        Me.SuperToolTip1.SetSuperToolTipColor1(Me.GroupBoxAzureConfig, System.Drawing.Color.Empty)
        Me.SuperToolTip1.SetSuperToolTipColor2(Me.GroupBoxAzureConfig, System.Drawing.Color.Empty)
        Me.GroupBoxAzureConfig.TabStop = False
        '
        'tbServiceBusNamespace
        '
        resources.ApplyResources(Me.tbServiceBusNamespace, "tbServiceBusNamespace")
        Me.tbServiceBusNamespace.Name = "tbServiceBusNamespace"
        Me.SuperToolTip1.SetSuperToolTipColor1(Me.tbServiceBusNamespace, System.Drawing.Color.Empty)
        Me.SuperToolTip1.SetSuperToolTipColor2(Me.tbServiceBusNamespace, System.Drawing.Color.Empty)
        '
        'Label8
        '
        resources.ApplyResources(Me.Label8, "Label8")
        Me.Label8.Name = "Label8"
        Me.SuperToolTip1.SetSuperToolTipColor1(Me.Label8, System.Drawing.Color.Empty)
        Me.SuperToolTip1.SetSuperToolTipColor2(Me.Label8, System.Drawing.Color.Empty)
        '
        'cbSolverMode
        '
        Me.cbSolverMode.DropDownStyle = System.Windows.Forms.ComboBoxStyle.DropDownList
        Me.cbSolverMode.FormattingEnabled = True
        Me.cbSolverMode.Items.AddRange(New Object() {resources.GetString("cbSolverMode.Items"), resources.GetString("cbSolverMode.Items1"), resources.GetString("cbSolverMode.Items2"), resources.GetString("cbSolverMode.Items3"), resources.GetString("cbSolverMode.Items4")})
        resources.ApplyResources(Me.cbSolverMode, "cbSolverMode")
        Me.cbSolverMode.Name = "cbSolverMode"
        Me.SuperToolTip1.SetSuperToolTipColor1(Me.cbSolverMode, System.Drawing.Color.Empty)
        Me.SuperToolTip1.SetSuperToolTipColor2(Me.cbSolverMode, System.Drawing.Color.Empty)
        '
        'Label2
        '
        resources.ApplyResources(Me.Label2, "Label2")
        Me.Label2.Name = "Label2"
        Me.SuperToolTip1.SetSuperToolTipColor1(Me.Label2, System.Drawing.Color.Empty)
        Me.SuperToolTip1.SetSuperToolTipColor2(Me.Label2, System.Drawing.Color.Empty)
        '
        'GroupBoxNetworkComputerConfig
        '
        Me.GroupBoxNetworkComputerConfig.Controls.Add(Me.tbServerPort)
        Me.GroupBoxNetworkComputerConfig.Controls.Add(Me.tbServerIP)
        Me.GroupBoxNetworkComputerConfig.Controls.Add(Me.Label15)
        Me.GroupBoxNetworkComputerConfig.Controls.Add(Me.Label16)
        resources.ApplyResources(Me.GroupBoxNetworkComputerConfig, "GroupBoxNetworkComputerConfig")
        Me.GroupBoxNetworkComputerConfig.Name = "GroupBoxNetworkComputerConfig"
        Me.SuperToolTip1.SetSuperToolTipColor1(Me.GroupBoxNetworkComputerConfig, System.Drawing.Color.Empty)
        Me.SuperToolTip1.SetSuperToolTipColor2(Me.GroupBoxNetworkComputerConfig, System.Drawing.Color.Empty)
        Me.GroupBoxNetworkComputerConfig.TabStop = False
        '
        'tbServerPort
        '
        resources.ApplyResources(Me.tbServerPort, "tbServerPort")
        Me.tbServerPort.Name = "tbServerPort"
        Me.SuperToolTip1.SetSuperToolTipColor1(Me.tbServerPort, System.Drawing.Color.Empty)
        Me.SuperToolTip1.SetSuperToolTipColor2(Me.tbServerPort, System.Drawing.Color.Empty)
        '
        'tbServerIP
        '
        resources.ApplyResources(Me.tbServerIP, "tbServerIP")
        Me.tbServerIP.Name = "tbServerIP"
        Me.SuperToolTip1.SetSuperToolTipColor1(Me.tbServerIP, System.Drawing.Color.Empty)
        Me.SuperToolTip1.SetSuperToolTipColor2(Me.tbServerIP, System.Drawing.Color.Empty)
        '
        'Label15
        '
        resources.ApplyResources(Me.Label15, "Label15")
        Me.Label15.Name = "Label15"
        Me.SuperToolTip1.SetSuperToolTipColor1(Me.Label15, System.Drawing.Color.Empty)
        Me.SuperToolTip1.SetSuperToolTipColor2(Me.Label15, System.Drawing.Color.Empty)
        '
        'Label16
        '
        resources.ApplyResources(Me.Label16, "Label16")
        Me.Label16.Name = "Label16"
        Me.SuperToolTip1.SetSuperToolTipColor1(Me.Label16, System.Drawing.Color.Empty)
        Me.SuperToolTip1.SetSuperToolTipColor2(Me.Label16, System.Drawing.Color.Empty)
        '
        'GroupBox7
        '
        resources.ApplyResources(Me.GroupBox7, "GroupBox7")
        Me.GroupBox7.Controls.Add(Me.GroupBox8)
        Me.GroupBox7.Controls.Add(Me.cbGPU)
        Me.GroupBox7.Controls.Add(Me.Label7)
        Me.GroupBox7.Controls.Add(Me.chkEnableGPUProcessing)
        Me.GroupBox7.Controls.Add(Me.cbParallelism)
        Me.GroupBox7.Controls.Add(Me.Label6)
        Me.GroupBox7.Controls.Add(Me.chkEnableParallelCalcs)
        Me.GroupBox7.Name = "GroupBox7"
        Me.SuperToolTip1.SetSuperToolTipColor1(Me.GroupBox7, System.Drawing.Color.Empty)
        Me.SuperToolTip1.SetSuperToolTipColor2(Me.GroupBox7, System.Drawing.Color.Empty)
        Me.GroupBox7.TabStop = False
        '
        'GroupBox8
        '
        Me.GroupBox8.Controls.Add(Me.tbGPUCaps)
        resources.ApplyResources(Me.GroupBox8, "GroupBox8")
        Me.GroupBox8.Name = "GroupBox8"
        Me.SuperToolTip1.SetSuperToolTipColor1(Me.GroupBox8, System.Drawing.Color.Empty)
        Me.SuperToolTip1.SetSuperToolTipColor2(Me.GroupBox8, System.Drawing.Color.Empty)
        Me.GroupBox8.TabStop = False
        '
        'tbGPUCaps
        '
        resources.ApplyResources(Me.tbGPUCaps, "tbGPUCaps")
        Me.tbGPUCaps.Name = "tbGPUCaps"
        Me.tbGPUCaps.ReadOnly = True
        Me.SuperToolTip1.SetSuperToolTipColor1(Me.tbGPUCaps, System.Drawing.Color.Empty)
        Me.SuperToolTip1.SetSuperToolTipColor2(Me.tbGPUCaps, System.Drawing.Color.Empty)
        '
        'cbGPU
        '
        Me.cbGPU.DropDownStyle = System.Windows.Forms.ComboBoxStyle.DropDownList
        Me.cbGPU.FormattingEnabled = True
        resources.ApplyResources(Me.cbGPU, "cbGPU")
        Me.cbGPU.Name = "cbGPU"
        Me.SuperToolTip1.SetSuperToolTipColor1(Me.cbGPU, System.Drawing.Color.Empty)
        Me.SuperToolTip1.SetSuperToolTipColor2(Me.cbGPU, System.Drawing.Color.Empty)
        '
        'Label7
        '
        resources.ApplyResources(Me.Label7, "Label7")
        Me.Label7.Name = "Label7"
        Me.SuperToolTip1.SetSuperToolTipColor1(Me.Label7, System.Drawing.Color.Empty)
        Me.SuperToolTip1.SetSuperToolTipColor2(Me.Label7, System.Drawing.Color.Empty)
        '
        'chkEnableGPUProcessing
        '
        resources.ApplyResources(Me.chkEnableGPUProcessing, "chkEnableGPUProcessing")
        Me.chkEnableGPUProcessing.Name = "chkEnableGPUProcessing"
        Me.SuperToolTip1.SetSuperToolTipColor1(Me.chkEnableGPUProcessing, System.Drawing.Color.Empty)
        Me.SuperToolTip1.SetSuperToolTipColor2(Me.chkEnableGPUProcessing, System.Drawing.Color.Empty)
        '
        'cbParallelism
        '
        Me.cbParallelism.DropDownStyle = System.Windows.Forms.ComboBoxStyle.DropDownList
        Me.cbParallelism.FormattingEnabled = True
        resources.ApplyResources(Me.cbParallelism, "cbParallelism")
        Me.cbParallelism.Name = "cbParallelism"
        Me.SuperToolTip1.SetSuperToolTipColor1(Me.cbParallelism, System.Drawing.Color.Empty)
        Me.SuperToolTip1.SetSuperToolTipColor2(Me.cbParallelism, System.Drawing.Color.Empty)
        '
        'Label6
        '
        resources.ApplyResources(Me.Label6, "Label6")
        Me.Label6.Name = "Label6"
        Me.SuperToolTip1.SetSuperToolTipColor1(Me.Label6, System.Drawing.Color.Empty)
        Me.SuperToolTip1.SetSuperToolTipColor2(Me.Label6, System.Drawing.Color.Empty)
        '
        'chkEnableParallelCalcs
        '
        resources.ApplyResources(Me.chkEnableParallelCalcs, "chkEnableParallelCalcs")
        Me.chkEnableParallelCalcs.Name = "chkEnableParallelCalcs"
        Me.SuperToolTip1.SetSuperToolTipColor1(Me.chkEnableParallelCalcs, System.Drawing.Color.Empty)
        Me.SuperToolTip1.SetSuperToolTipColor2(Me.chkEnableParallelCalcs, System.Drawing.Color.Empty)
        '
        'FaTabStripItem3
        '
        Me.FaTabStripItem3.CanClose = False
        Me.FaTabStripItem3.Controls.Add(Me.GroupBox11)
        Me.FaTabStripItem3.Controls.Add(Me.GroupBox4)
        Me.FaTabStripItem3.IsDrawn = True
        Me.FaTabStripItem3.Name = "FaTabStripItem3"
        resources.ApplyResources(Me.FaTabStripItem3, "FaTabStripItem3")
        Me.SuperToolTip1.SetSuperToolTipColor1(Me.FaTabStripItem3, System.Drawing.Color.Empty)
        Me.SuperToolTip1.SetSuperToolTipColor2(Me.FaTabStripItem3, System.Drawing.Color.Empty)
        '
        'GroupBox11
        '
        resources.ApplyResources(Me.GroupBox11, "GroupBox11")
        Me.GroupBox11.Controls.Add(Me.cbudb)
        Me.GroupBox11.Controls.Add(Me.Button11)
        Me.GroupBox11.Controls.Add(Me.Button7)
        Me.GroupBox11.Name = "GroupBox11"
        Me.SuperToolTip1.SetSuperToolTipColor1(Me.GroupBox11, System.Drawing.Color.Empty)
        Me.SuperToolTip1.SetSuperToolTipColor2(Me.GroupBox11, System.Drawing.Color.Empty)
        Me.GroupBox11.TabStop = False
        '
        'cbudb
        '
        resources.ApplyResources(Me.cbudb, "cbudb")
        Me.cbudb.Name = "cbudb"
        Me.SuperToolTip1.SetSuperToolTipColor1(Me.cbudb, System.Drawing.Color.Empty)
        Me.SuperToolTip1.SetSuperToolTipColor2(Me.cbudb, System.Drawing.Color.Empty)
        Me.cbudb.UseVisualStyleBackColor = True
        '
        'Button11
        '
        resources.ApplyResources(Me.Button11, "Button11")
        Me.Button11.Name = "Button11"
        Me.SuperToolTip1.SetSuperToolTipColor1(Me.Button11, System.Drawing.Color.Empty)
        Me.SuperToolTip1.SetSuperToolTipColor2(Me.Button11, System.Drawing.Color.Empty)
        Me.Button11.UseVisualStyleBackColor = True
        '
        'Button7
        '
        resources.ApplyResources(Me.Button7, "Button7")
        Me.Button7.Name = "Button7"
        Me.SuperToolTip1.SetSuperToolTipColor1(Me.Button7, System.Drawing.Color.Empty)
        Me.SuperToolTip1.SetSuperToolTipColor2(Me.Button7, System.Drawing.Color.Empty)
        Me.Button7.UseVisualStyleBackColor = True
        '
        'GroupBox4
        '
        resources.ApplyResources(Me.GroupBox4, "GroupBox4")
        Me.GroupBox4.Controls.Add(Me.dgvdb)
        Me.GroupBox4.Name = "GroupBox4"
        Me.SuperToolTip1.SetSuperToolTipColor1(Me.GroupBox4, System.Drawing.Color.Empty)
        Me.SuperToolTip1.SetSuperToolTipColor2(Me.GroupBox4, System.Drawing.Color.Empty)
        Me.GroupBox4.TabStop = False
        '
        'dgvdb
        '
        Me.dgvdb.AllowUserToAddRows = False
        Me.dgvdb.AllowUserToDeleteRows = False
        Me.dgvdb.AllowUserToResizeColumns = False
        Me.dgvdb.AllowUserToResizeRows = False
        Me.dgvdb.AutoSizeColumnsMode = System.Windows.Forms.DataGridViewAutoSizeColumnsMode.Fill
        Me.dgvdb.BackgroundColor = System.Drawing.SystemColors.Control
        Me.dgvdb.BorderStyle = System.Windows.Forms.BorderStyle.Fixed3D
        Me.dgvdb.CellBorderStyle = System.Windows.Forms.DataGridViewCellBorderStyle.SingleHorizontal
        Me.dgvdb.ColumnHeadersHeightSizeMode = System.Windows.Forms.DataGridViewColumnHeadersHeightSizeMode.AutoSize
        Me.dgvdb.ColumnHeadersVisible = False
        Me.dgvdb.Columns.AddRange(New System.Windows.Forms.DataGridViewColumn() {Me.Column12, Me.Column14, Me.Column15, Me.Column13, Me.BtnEdit})
        resources.ApplyResources(Me.dgvdb, "dgvdb")
        Me.dgvdb.GridColor = System.Drawing.SystemColors.Control
        Me.dgvdb.Name = "dgvdb"
        Me.dgvdb.ReadOnly = True
        Me.dgvdb.RowHeadersBorderStyle = System.Windows.Forms.DataGridViewHeaderBorderStyle.None
        Me.dgvdb.RowHeadersVisible = False
        Me.dgvdb.RowTemplate.DefaultCellStyle.Padding = New System.Windows.Forms.Padding(0, 5, 0, 5)
        Me.dgvdb.RowTemplate.Height = 38
        Me.dgvdb.SelectionMode = System.Windows.Forms.DataGridViewSelectionMode.FullRowSelect
        Me.dgvdb.ShowCellErrors = False
        Me.SuperToolTip1.SetSuperToolTipColor1(Me.dgvdb, System.Drawing.Color.Empty)
        Me.SuperToolTip1.SetSuperToolTipColor2(Me.dgvdb, System.Drawing.Color.Empty)
        '
        'Column12
        '
        Me.Column12.AutoSizeMode = System.Windows.Forms.DataGridViewAutoSizeColumnMode.AllCells
        Me.Column12.FillWeight = 10.0!
        resources.ApplyResources(Me.Column12, "Column12")
        Me.Column12.Name = "Column12"
        Me.Column12.ReadOnly = True
        Me.Column12.Resizable = System.Windows.Forms.DataGridViewTriState.[False]
        '
        'Column14
        '
        Me.Column14.AutoSizeMode = System.Windows.Forms.DataGridViewAutoSizeColumnMode.AllCells
        Me.Column14.FillWeight = 30.0!
        resources.ApplyResources(Me.Column14, "Column14")
        Me.Column14.Name = "Column14"
        Me.Column14.ReadOnly = True
        Me.Column14.Resizable = System.Windows.Forms.DataGridViewTriState.[False]
        '
        'Column15
        '
        Me.Column15.AutoSizeMode = System.Windows.Forms.DataGridViewAutoSizeColumnMode.Fill
        resources.ApplyResources(Me.Column15, "Column15")
        Me.Column15.Name = "Column15"
        Me.Column15.ReadOnly = True
        Me.Column15.Resizable = System.Windows.Forms.DataGridViewTriState.[False]
        '
        'Column13
        '
        Me.Column13.AutoSizeMode = System.Windows.Forms.DataGridViewAutoSizeColumnMode.None
        DataGridViewCellStyle1.Alignment = System.Windows.Forms.DataGridViewContentAlignment.MiddleCenter
        DataGridViewCellStyle1.BackColor = System.Drawing.Color.FromArgb(CType(CType(224, Byte), Integer), CType(CType(224, Byte), Integer), CType(CType(224, Byte), Integer))
        DataGridViewCellStyle1.NullValue = CType(resources.GetObject("DataGridViewCellStyle1.NullValue"), Object)
        Me.Column13.DefaultCellStyle = DataGridViewCellStyle1
        Me.Column13.FillWeight = 10.0!
        resources.ApplyResources(Me.Column13, "Column13")
        Me.Column13.Name = "Column13"
        Me.Column13.ReadOnly = True
        Me.Column13.Resizable = System.Windows.Forms.DataGridViewTriState.[False]
        '
        'BtnEdit
        '
        Me.BtnEdit.AutoSizeMode = System.Windows.Forms.DataGridViewAutoSizeColumnMode.None
        DataGridViewCellStyle2.Alignment = System.Windows.Forms.DataGridViewContentAlignment.MiddleCenter
        DataGridViewCellStyle2.BackColor = System.Drawing.Color.FromArgb(CType(CType(224, Byte), Integer), CType(CType(224, Byte), Integer), CType(CType(224, Byte), Integer))
        DataGridViewCellStyle2.NullValue = CType(resources.GetObject("DataGridViewCellStyle2.NullValue"), Object)
        Me.BtnEdit.DefaultCellStyle = DataGridViewCellStyle2
        Me.BtnEdit.FillWeight = 10.0!
        resources.ApplyResources(Me.BtnEdit, "BtnEdit")
        Me.BtnEdit.Image = Global.DWSIM.My.Resources.Resources.cross
        Me.BtnEdit.Name = "BtnEdit"
        Me.BtnEdit.ReadOnly = True
        Me.BtnEdit.Resizable = System.Windows.Forms.DataGridViewTriState.[False]
        '
        'FaTabStripItem6
        '
        Me.FaTabStripItem6.CanClose = False
        Me.FaTabStripItem6.Controls.Add(Me.GroupBox12)
        Me.FaTabStripItem6.Controls.Add(Me.GroupBox10)
        Me.FaTabStripItem6.IsDrawn = True
        Me.FaTabStripItem6.Name = "FaTabStripItem6"
        resources.ApplyResources(Me.FaTabStripItem6, "FaTabStripItem6")
        Me.SuperToolTip1.SetSuperToolTipColor1(Me.FaTabStripItem6, System.Drawing.Color.Empty)
        Me.SuperToolTip1.SetSuperToolTipColor2(Me.FaTabStripItem6, System.Drawing.Color.Empty)
        '
        'GroupBox12
        '
        resources.ApplyResources(Me.GroupBox12, "GroupBox12")
        Me.GroupBox12.Controls.Add(Me.Button4)
        Me.GroupBox12.Name = "GroupBox12"
        Me.SuperToolTip1.SetSuperToolTipColor1(Me.GroupBox12, System.Drawing.Color.Empty)
        Me.SuperToolTip1.SetSuperToolTipColor2(Me.GroupBox12, System.Drawing.Color.Empty)
        Me.GroupBox12.TabStop = False
        '
        'Button4
        '
        resources.ApplyResources(Me.Button4, "Button4")
        Me.Button4.Name = "Button4"
        Me.SuperToolTip1.SetSuperToolTipColor1(Me.Button4, System.Drawing.Color.Empty)
        Me.SuperToolTip1.SetSuperToolTipColor2(Me.Button4, System.Drawing.Color.Empty)
        Me.Button4.UseVisualStyleBackColor = True
        '
        'GroupBox10
        '
        resources.ApplyResources(Me.GroupBox10, "GroupBox10")
        Me.GroupBox10.Controls.Add(Me.dgvIPDB)
        Me.GroupBox10.Name = "GroupBox10"
        Me.SuperToolTip1.SetSuperToolTipColor1(Me.GroupBox10, System.Drawing.Color.Empty)
        Me.SuperToolTip1.SetSuperToolTipColor2(Me.GroupBox10, System.Drawing.Color.Empty)
        Me.GroupBox10.TabStop = False
        '
        'dgvIPDB
        '
        Me.dgvIPDB.AllowUserToAddRows = False
        Me.dgvIPDB.AllowUserToDeleteRows = False
        Me.dgvIPDB.AllowUserToResizeColumns = False
        Me.dgvIPDB.AllowUserToResizeRows = False
        Me.dgvIPDB.AutoSizeColumnsMode = System.Windows.Forms.DataGridViewAutoSizeColumnsMode.Fill
        Me.dgvIPDB.BackgroundColor = System.Drawing.SystemColors.Control
        Me.dgvIPDB.BorderStyle = System.Windows.Forms.BorderStyle.Fixed3D
        Me.dgvIPDB.CellBorderStyle = System.Windows.Forms.DataGridViewCellBorderStyle.SingleHorizontal
        Me.dgvIPDB.ColumnHeadersHeightSizeMode = System.Windows.Forms.DataGridViewColumnHeadersHeightSizeMode.AutoSize
        Me.dgvIPDB.ColumnHeadersVisible = False
        Me.dgvIPDB.Columns.AddRange(New System.Windows.Forms.DataGridViewColumn() {Me.DataGridViewTextBoxColumn1, Me.DataGridViewTextBoxColumn2, Me.DataGridViewTextBoxColumn3, Me.DataGridViewImageColumn1})
        resources.ApplyResources(Me.dgvIPDB, "dgvIPDB")
        Me.dgvIPDB.GridColor = System.Drawing.SystemColors.Control
        Me.dgvIPDB.Name = "dgvIPDB"
        Me.dgvIPDB.ReadOnly = True
        Me.dgvIPDB.RowHeadersBorderStyle = System.Windows.Forms.DataGridViewHeaderBorderStyle.None
        Me.dgvIPDB.RowHeadersVisible = False
        Me.dgvIPDB.RowTemplate.DefaultCellStyle.Padding = New System.Windows.Forms.Padding(0, 5, 0, 5)
        Me.dgvIPDB.RowTemplate.Height = 38
        Me.dgvIPDB.SelectionMode = System.Windows.Forms.DataGridViewSelectionMode.FullRowSelect
        Me.dgvIPDB.ShowCellErrors = False
        Me.SuperToolTip1.SetSuperToolTipColor1(Me.dgvIPDB, System.Drawing.Color.Empty)
        Me.SuperToolTip1.SetSuperToolTipColor2(Me.dgvIPDB, System.Drawing.Color.Empty)
        '
        'DataGridViewTextBoxColumn1
        '
        Me.DataGridViewTextBoxColumn1.AutoSizeMode = System.Windows.Forms.DataGridViewAutoSizeColumnMode.AllCells
        Me.DataGridViewTextBoxColumn1.FillWeight = 10.0!
        resources.ApplyResources(Me.DataGridViewTextBoxColumn1, "DataGridViewTextBoxColumn1")
        Me.DataGridViewTextBoxColumn1.Name = "DataGridViewTextBoxColumn1"
        Me.DataGridViewTextBoxColumn1.ReadOnly = True
        Me.DataGridViewTextBoxColumn1.Resizable = System.Windows.Forms.DataGridViewTriState.[False]
        '
        'DataGridViewTextBoxColumn2
        '
        Me.DataGridViewTextBoxColumn2.AutoSizeMode = System.Windows.Forms.DataGridViewAutoSizeColumnMode.AllCells
        Me.DataGridViewTextBoxColumn2.FillWeight = 30.0!
        resources.ApplyResources(Me.DataGridViewTextBoxColumn2, "DataGridViewTextBoxColumn2")
        Me.DataGridViewTextBoxColumn2.Name = "DataGridViewTextBoxColumn2"
        Me.DataGridViewTextBoxColumn2.ReadOnly = True
        Me.DataGridViewTextBoxColumn2.Resizable = System.Windows.Forms.DataGridViewTriState.[False]
        '
        'DataGridViewTextBoxColumn3
        '
        Me.DataGridViewTextBoxColumn3.AutoSizeMode = System.Windows.Forms.DataGridViewAutoSizeColumnMode.Fill
        resources.ApplyResources(Me.DataGridViewTextBoxColumn3, "DataGridViewTextBoxColumn3")
        Me.DataGridViewTextBoxColumn3.Name = "DataGridViewTextBoxColumn3"
        Me.DataGridViewTextBoxColumn3.ReadOnly = True
        Me.DataGridViewTextBoxColumn3.Resizable = System.Windows.Forms.DataGridViewTriState.[False]
        '
        'DataGridViewImageColumn1
        '
        Me.DataGridViewImageColumn1.AutoSizeMode = System.Windows.Forms.DataGridViewAutoSizeColumnMode.None
        DataGridViewCellStyle3.Alignment = System.Windows.Forms.DataGridViewContentAlignment.MiddleCenter
        DataGridViewCellStyle3.BackColor = System.Drawing.Color.FromArgb(CType(CType(224, Byte), Integer), CType(CType(224, Byte), Integer), CType(CType(224, Byte), Integer))
        DataGridViewCellStyle3.NullValue = CType(resources.GetObject("DataGridViewCellStyle3.NullValue"), Object)
        Me.DataGridViewImageColumn1.DefaultCellStyle = DataGridViewCellStyle3
        Me.DataGridViewImageColumn1.FillWeight = 10.0!
        resources.ApplyResources(Me.DataGridViewImageColumn1, "DataGridViewImageColumn1")
        Me.DataGridViewImageColumn1.Name = "DataGridViewImageColumn1"
        Me.DataGridViewImageColumn1.ReadOnly = True
        Me.DataGridViewImageColumn1.Resizable = System.Windows.Forms.DataGridViewTriState.[False]
        '
        'FaTabStripItem2
        '
        Me.FaTabStripItem2.CanClose = False
        Me.FaTabStripItem2.Controls.Add(Me.GroupBox3)
        Me.FaTabStripItem2.IsDrawn = True
        Me.FaTabStripItem2.Name = "FaTabStripItem2"
        resources.ApplyResources(Me.FaTabStripItem2, "FaTabStripItem2")
        Me.SuperToolTip1.SetSuperToolTipColor1(Me.FaTabStripItem2, System.Drawing.Color.Empty)
        Me.SuperToolTip1.SetSuperToolTipColor2(Me.FaTabStripItem2, System.Drawing.Color.Empty)
        '
        'GroupBox3
        '
        resources.ApplyResources(Me.GroupBox3, "GroupBox3")
        Me.GroupBox3.Controls.Add(Me.Button2)
        Me.GroupBox3.Controls.Add(Me.KryptonLabel3)
        Me.GroupBox3.Controls.Add(Me.TrackBar1)
        Me.GroupBox3.Controls.Add(Me.KryptonLabel2)
        Me.GroupBox3.Controls.Add(Me.KryptonButton1)
        Me.GroupBox3.Controls.Add(Me.KryptonTextBox1)
        Me.GroupBox3.Controls.Add(Me.KryptonLabel1)
        Me.GroupBox3.Controls.Add(Me.KryptonCheckBox6)
        Me.GroupBox3.Name = "GroupBox3"
        Me.SuperToolTip1.SetSuperToolTipColor1(Me.GroupBox3, System.Drawing.Color.Empty)
        Me.SuperToolTip1.SetSuperToolTipColor2(Me.GroupBox3, System.Drawing.Color.Empty)
        Me.GroupBox3.TabStop = False
        '
        'Button2
        '
        resources.ApplyResources(Me.Button2, "Button2")
        Me.Button2.Name = "Button2"
        Me.SuperToolTip1.SetSuperToolTipColor1(Me.Button2, System.Drawing.Color.Empty)
        Me.SuperToolTip1.SetSuperToolTipColor2(Me.Button2, System.Drawing.Color.Empty)
        '
        'KryptonLabel3
        '
        resources.ApplyResources(Me.KryptonLabel3, "KryptonLabel3")
        Me.KryptonLabel3.Name = "KryptonLabel3"
        Me.SuperToolTip1.SetSuperToolTipColor1(Me.KryptonLabel3, System.Drawing.Color.Empty)
        Me.SuperToolTip1.SetSuperToolTipColor2(Me.KryptonLabel3, System.Drawing.Color.Empty)
        '
        'TrackBar1
        '
        resources.ApplyResources(Me.TrackBar1, "TrackBar1")
        Me.TrackBar1.Maximum = 60
        Me.TrackBar1.Minimum = 1
        Me.TrackBar1.Name = "TrackBar1"
        Me.SuperToolTip1.SetSuperToolTipColor1(Me.TrackBar1, System.Drawing.Color.Empty)
        Me.SuperToolTip1.SetSuperToolTipColor2(Me.TrackBar1, System.Drawing.Color.Empty)
        Me.TrackBar1.Value = 5
        '
        'KryptonLabel2
        '
        resources.ApplyResources(Me.KryptonLabel2, "KryptonLabel2")
        Me.KryptonLabel2.Name = "KryptonLabel2"
        Me.SuperToolTip1.SetSuperToolTipColor1(Me.KryptonLabel2, System.Drawing.Color.Empty)
        Me.SuperToolTip1.SetSuperToolTipColor2(Me.KryptonLabel2, System.Drawing.Color.Empty)
        '
        'KryptonButton1
        '
        resources.ApplyResources(Me.KryptonButton1, "KryptonButton1")
        Me.KryptonButton1.Name = "KryptonButton1"
        Me.SuperToolTip1.SetSuperToolTipColor1(Me.KryptonButton1, System.Drawing.Color.Empty)
        Me.SuperToolTip1.SetSuperToolTipColor2(Me.KryptonButton1, System.Drawing.Color.Empty)
        '
        'KryptonTextBox1
        '
        resources.ApplyResources(Me.KryptonTextBox1, "KryptonTextBox1")
        Me.KryptonTextBox1.Name = "KryptonTextBox1"
        Me.KryptonTextBox1.ReadOnly = True
        Me.SuperToolTip1.SetSuperToolTipColor1(Me.KryptonTextBox1, System.Drawing.Color.Empty)
        Me.SuperToolTip1.SetSuperToolTipColor2(Me.KryptonTextBox1, System.Drawing.Color.Empty)
        '
        'KryptonLabel1
        '
        resources.ApplyResources(Me.KryptonLabel1, "KryptonLabel1")
        Me.KryptonLabel1.Name = "KryptonLabel1"
        Me.SuperToolTip1.SetSuperToolTipColor1(Me.KryptonLabel1, System.Drawing.Color.Empty)
        Me.SuperToolTip1.SetSuperToolTipColor2(Me.KryptonLabel1, System.Drawing.Color.Empty)
        '
        'KryptonCheckBox6
        '
        resources.ApplyResources(Me.KryptonCheckBox6, "KryptonCheckBox6")
        Me.KryptonCheckBox6.Name = "KryptonCheckBox6"
        Me.SuperToolTip1.SetSuperToolTipColor1(Me.KryptonCheckBox6, System.Drawing.Color.Empty)
        Me.SuperToolTip1.SetSuperToolTipColor2(Me.KryptonCheckBox6, System.Drawing.Color.Empty)
        '
        'FaTabStripItem5
        '
        Me.FaTabStripItem5.CanClose = False
        Me.FaTabStripItem5.Controls.Add(Me.GroupBox5)
        Me.FaTabStripItem5.IsDrawn = True
        Me.FaTabStripItem5.Name = "FaTabStripItem5"
        resources.ApplyResources(Me.FaTabStripItem5, "FaTabStripItem5")
        Me.SuperToolTip1.SetSuperToolTipColor1(Me.FaTabStripItem5, System.Drawing.Color.Empty)
        Me.SuperToolTip1.SetSuperToolTipColor2(Me.FaTabStripItem5, System.Drawing.Color.Empty)
        '
        'GroupBox5
        '
        Me.GroupBox5.Controls.Add(Me.lbpaths)
        Me.GroupBox5.Controls.Add(Me.Label5)
        Me.GroupBox5.Controls.Add(Me.btnrmpath)
        Me.GroupBox5.Controls.Add(Me.btnaddpath)
        Me.GroupBox5.Controls.Add(Me.tbaddpath)
        Me.GroupBox5.Controls.Add(Me.Label4)
        resources.ApplyResources(Me.GroupBox5, "GroupBox5")
        Me.GroupBox5.Name = "GroupBox5"
        Me.SuperToolTip1.SetSuperToolTipColor1(Me.GroupBox5, System.Drawing.Color.Empty)
        Me.SuperToolTip1.SetSuperToolTipColor2(Me.GroupBox5, System.Drawing.Color.Empty)
        Me.GroupBox5.TabStop = False
        '
        'lbpaths
        '
        Me.lbpaths.FormattingEnabled = True
        resources.ApplyResources(Me.lbpaths, "lbpaths")
        Me.lbpaths.Name = "lbpaths"
        Me.SuperToolTip1.SetSuperToolTipColor1(Me.lbpaths, System.Drawing.Color.Empty)
        Me.SuperToolTip1.SetSuperToolTipColor2(Me.lbpaths, System.Drawing.Color.Empty)
        '
        'Label5
        '
        resources.ApplyResources(Me.Label5, "Label5")
        Me.Label5.Name = "Label5"
        Me.SuperToolTip1.SetSuperToolTipColor1(Me.Label5, System.Drawing.Color.Empty)
        Me.SuperToolTip1.SetSuperToolTipColor2(Me.Label5, System.Drawing.Color.Empty)
        '
        'btnrmpath
        '
        resources.ApplyResources(Me.btnrmpath, "btnrmpath")
        Me.btnrmpath.Name = "btnrmpath"
        Me.SuperToolTip1.SetSuperToolTipColor1(Me.btnrmpath, System.Drawing.Color.Empty)
        Me.SuperToolTip1.SetSuperToolTipColor2(Me.btnrmpath, System.Drawing.Color.Empty)
        Me.btnrmpath.UseVisualStyleBackColor = True
        '
        'btnaddpath
        '
        resources.ApplyResources(Me.btnaddpath, "btnaddpath")
        Me.btnaddpath.Name = "btnaddpath"
        Me.SuperToolTip1.SetSuperToolTipColor1(Me.btnaddpath, System.Drawing.Color.Empty)
        Me.SuperToolTip1.SetSuperToolTipColor2(Me.btnaddpath, System.Drawing.Color.Empty)
        Me.btnaddpath.UseVisualStyleBackColor = True
        '
        'tbaddpath
        '
        resources.ApplyResources(Me.tbaddpath, "tbaddpath")
        Me.tbaddpath.Name = "tbaddpath"
        Me.SuperToolTip1.SetSuperToolTipColor1(Me.tbaddpath, System.Drawing.Color.Empty)
        Me.SuperToolTip1.SetSuperToolTipColor2(Me.tbaddpath, System.Drawing.Color.Empty)
        '
        'Label4
        '
        resources.ApplyResources(Me.Label4, "Label4")
        Me.Label4.Name = "Label4"
        Me.SuperToolTip1.SetSuperToolTipColor1(Me.Label4, System.Drawing.Color.Empty)
        Me.SuperToolTip1.SetSuperToolTipColor2(Me.Label4, System.Drawing.Color.Empty)
        '
        'FaTabStripItem7
        '
        Me.FaTabStripItem7.CanClose = False
        Me.FaTabStripItem7.Controls.Add(Me.GroupBox1)
        Me.FaTabStripItem7.Controls.Add(Me.GroupBox9)
        Me.FaTabStripItem7.Controls.Add(Me.GroupBox6)
        Me.FaTabStripItem7.IsDrawn = True
        Me.FaTabStripItem7.Name = "FaTabStripItem7"
        resources.ApplyResources(Me.FaTabStripItem7, "FaTabStripItem7")
        Me.SuperToolTip1.SetSuperToolTipColor1(Me.FaTabStripItem7, System.Drawing.Color.Empty)
        Me.SuperToolTip1.SetSuperToolTipColor2(Me.FaTabStripItem7, System.Drawing.Color.Empty)
        '
        'GroupBox1
        '
        resources.ApplyResources(Me.GroupBox1, "GroupBox1")
        Me.GroupBox1.Controls.Add(Me.ComboBoxUILanguage)
        Me.GroupBox1.Controls.Add(Me.Label3)
        Me.GroupBox1.Name = "GroupBox1"
        Me.SuperToolTip1.SetSuperToolTipColor1(Me.GroupBox1, System.Drawing.Color.Empty)
        Me.SuperToolTip1.SetSuperToolTipColor2(Me.GroupBox1, System.Drawing.Color.Empty)
        Me.GroupBox1.TabStop = False
        '
        'ComboBoxUILanguage
        '
        Me.ComboBoxUILanguage.DropDownStyle = System.Windows.Forms.ComboBoxStyle.DropDownList
        Me.ComboBoxUILanguage.FormattingEnabled = True
        Me.ComboBoxUILanguage.Items.AddRange(New Object() {resources.GetString("ComboBoxUILanguage.Items"), resources.GetString("ComboBoxUILanguage.Items1"), resources.GetString("ComboBoxUILanguage.Items2"), resources.GetString("ComboBoxUILanguage.Items3")})
        resources.ApplyResources(Me.ComboBoxUILanguage, "ComboBoxUILanguage")
        Me.ComboBoxUILanguage.Name = "ComboBoxUILanguage"
        Me.SuperToolTip1.SetSuperToolTipColor1(Me.ComboBoxUILanguage, System.Drawing.Color.Empty)
        Me.SuperToolTip1.SetSuperToolTipColor2(Me.ComboBoxUILanguage, System.Drawing.Color.Empty)
        '
        'Label3
        '
        resources.ApplyResources(Me.Label3, "Label3")
        Me.Label3.Name = "Label3"
        Me.SuperToolTip1.SetSuperToolTipColor1(Me.Label3, System.Drawing.Color.Empty)
        Me.SuperToolTip1.SetSuperToolTipColor2(Me.Label3, System.Drawing.Color.Empty)
        '
        'GroupBox9
        '
        Me.GroupBox9.Controls.Add(Me.chkShowWhatsNew)
        Me.GroupBox9.Controls.Add(Me.KryptonCheckBox1)
        Me.GroupBox9.Controls.Add(Me.chkUpdates)
        resources.ApplyResources(Me.GroupBox9, "GroupBox9")
        Me.GroupBox9.Name = "GroupBox9"
        Me.SuperToolTip1.SetSuperToolTipColor1(Me.GroupBox9, System.Drawing.Color.Empty)
        Me.SuperToolTip1.SetSuperToolTipColor2(Me.GroupBox9, System.Drawing.Color.Empty)
        Me.GroupBox9.TabStop = False
        '
        'chkShowWhatsNew
        '
        resources.ApplyResources(Me.chkShowWhatsNew, "chkShowWhatsNew")
        Me.chkShowWhatsNew.Name = "chkShowWhatsNew"
        Me.SuperToolTip1.SetSuperToolTipColor1(Me.chkShowWhatsNew, System.Drawing.Color.Empty)
        Me.SuperToolTip1.SetSuperToolTipColor2(Me.chkShowWhatsNew, System.Drawing.Color.Empty)
        '
        'KryptonCheckBox1
        '
        resources.ApplyResources(Me.KryptonCheckBox1, "KryptonCheckBox1")
        Me.KryptonCheckBox1.Name = "KryptonCheckBox1"
        Me.SuperToolTip1.SetSuperToolTipColor1(Me.KryptonCheckBox1, System.Drawing.Color.Empty)
        Me.SuperToolTip1.SetSuperToolTipColor2(Me.KryptonCheckBox1, System.Drawing.Color.Empty)
        '
        'chkUpdates
        '
        resources.ApplyResources(Me.chkUpdates, "chkUpdates")
        Me.chkUpdates.Name = "chkUpdates"
        Me.SuperToolTip1.SetSuperToolTipColor1(Me.chkUpdates, System.Drawing.Color.Empty)
        Me.SuperToolTip1.SetSuperToolTipColor2(Me.chkUpdates, System.Drawing.Color.Empty)
        '
        'GroupBox6
        '
        Me.GroupBox6.Controls.Add(Me.cbDebugLevel)
        Me.GroupBox6.Controls.Add(Me.Label13)
        Me.GroupBox6.Controls.Add(Me.chkconsole)
        resources.ApplyResources(Me.GroupBox6, "GroupBox6")
        Me.GroupBox6.Name = "GroupBox6"
        Me.SuperToolTip1.SetSuperToolTipColor1(Me.GroupBox6, System.Drawing.Color.Empty)
        Me.SuperToolTip1.SetSuperToolTipColor2(Me.GroupBox6, System.Drawing.Color.Empty)
        Me.GroupBox6.TabStop = False
        '
        'cbDebugLevel
        '
        Me.cbDebugLevel.DropDownStyle = System.Windows.Forms.ComboBoxStyle.DropDownList
        Me.cbDebugLevel.FormattingEnabled = True
        Me.cbDebugLevel.Items.AddRange(New Object() {resources.GetString("cbDebugLevel.Items"), resources.GetString("cbDebugLevel.Items1"), resources.GetString("cbDebugLevel.Items2"), resources.GetString("cbDebugLevel.Items3")})
        resources.ApplyResources(Me.cbDebugLevel, "cbDebugLevel")
        Me.cbDebugLevel.Name = "cbDebugLevel"
        Me.SuperToolTip1.SetSuperToolTipColor1(Me.cbDebugLevel, System.Drawing.Color.Empty)
        Me.SuperToolTip1.SetSuperToolTipColor2(Me.cbDebugLevel, System.Drawing.Color.Empty)
        '
        'Label13
        '
        resources.ApplyResources(Me.Label13, "Label13")
        Me.Label13.Name = "Label13"
        Me.SuperToolTip1.SetSuperToolTipColor1(Me.Label13, System.Drawing.Color.Empty)
        Me.SuperToolTip1.SetSuperToolTipColor2(Me.Label13, System.Drawing.Color.Empty)
        '
        'chkconsole
        '
        resources.ApplyResources(Me.chkconsole, "chkconsole")
        Me.chkconsole.Name = "chkconsole"
        Me.SuperToolTip1.SetSuperToolTipColor1(Me.chkconsole, System.Drawing.Color.Empty)
        Me.SuperToolTip1.SetSuperToolTipColor2(Me.chkconsole, System.Drawing.Color.Empty)
        '
        'ImageList1
        '
        Me.ImageList1.ImageStream = CType(resources.GetObject("ImageList1.ImageStream"), System.Windows.Forms.ImageListStreamer)
        Me.ImageList1.TransparentColor = System.Drawing.Color.Transparent
        Me.ImageList1.Images.SetKeyName(0, "br.png")
        Me.ImageList1.Images.SetKeyName(1, "gb.png")
        Me.ImageList1.Images.SetKeyName(2, "us.png")
        Me.ImageList1.Images.SetKeyName(3, "ru.png")
        Me.ImageList1.Images.SetKeyName(4, "fr.png")
        Me.ImageList1.Images.SetKeyName(5, "de.png")
        Me.ImageList1.Images.SetKeyName(6, "jp.png")
        Me.ImageList1.Images.SetKeyName(7, "id.png")
        Me.ImageList1.Images.SetKeyName(8, "es.png")
        Me.ImageList1.Images.SetKeyName(9, "cn.png")
        '
        'FolderBrowserDialog1
        '
        resources.ApplyResources(Me.FolderBrowserDialog1, "FolderBrowserDialog1")
        '
        'ofdcs
        '
        Me.ofdcs.AddExtension = False
        resources.ApplyResources(Me.ofdcs, "ofdcs")
        Me.ofdcs.RestoreDirectory = True
        Me.ofdcs.SupportMultiDottedExtensions = True
        '
        'OpenFileDialog1
        '
        resources.ApplyResources(Me.OpenFileDialog1, "OpenFileDialog1")
        Me.OpenFileDialog1.SupportMultiDottedExtensions = True
        '
        'SuperToolTip1
        '
        Me.SuperToolTip1.AutoPopDelay = 50000
        Me.SuperToolTip1.InitialDelay = 500
        Me.SuperToolTip1.ReshowDelay = 200
        Me.SuperToolTip1.ShowAlways = True
        '
        'FormOptions
        '
        resources.ApplyResources(Me, "$this")
        Me.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font
        Me.Controls.Add(Me.FaTabStrip1)
        Me.FormBorderStyle = System.Windows.Forms.FormBorderStyle.FixedToolWindow
        Me.Name = "FormOptions"
        Me.ShowIcon = False
        Me.ShowInTaskbar = False
        Me.SuperToolTip1.SetSuperToolTipColor1(Me, System.Drawing.Color.Empty)
        Me.SuperToolTip1.SetSuperToolTipColor2(Me, System.Drawing.Color.Empty)
        CType(Me.FaTabStrip1, System.ComponentModel.ISupportInitialize).EndInit()
        Me.FaTabStrip1.ResumeLayout(False)
        Me.FaTabStripItem1.ResumeLayout(False)
        Me.GroupBox2.ResumeLayout(False)
        Me.GroupBox2.PerformLayout()
        Me.GroupBoxAzureConfig.ResumeLayout(False)
        Me.GroupBoxAzureConfig.PerformLayout()
        Me.GroupBoxNetworkComputerConfig.ResumeLayout(False)
        Me.GroupBoxNetworkComputerConfig.PerformLayout()
        Me.GroupBox7.ResumeLayout(False)
        Me.GroupBox7.PerformLayout()
        Me.GroupBox8.ResumeLayout(False)
        Me.GroupBox8.PerformLayout()
        Me.FaTabStripItem3.ResumeLayout(False)
        Me.GroupBox11.ResumeLayout(False)
        Me.GroupBox11.PerformLayout()
        Me.GroupBox4.ResumeLayout(False)
        CType(Me.dgvdb, System.ComponentModel.ISupportInitialize).EndInit()
        Me.FaTabStripItem6.ResumeLayout(False)
        Me.GroupBox12.ResumeLayout(False)
        Me.GroupBox10.ResumeLayout(False)
        CType(Me.dgvIPDB, System.ComponentModel.ISupportInitialize).EndInit()
        Me.FaTabStripItem2.ResumeLayout(False)
        Me.GroupBox3.ResumeLayout(False)
        Me.GroupBox3.PerformLayout()
        CType(Me.TrackBar1, System.ComponentModel.ISupportInitialize).EndInit()
        Me.FaTabStripItem5.ResumeLayout(False)
        Me.GroupBox5.ResumeLayout(False)
        Me.GroupBox5.PerformLayout()
        Me.FaTabStripItem7.ResumeLayout(False)
        Me.GroupBox1.ResumeLayout(False)
        Me.GroupBox1.PerformLayout()
        Me.GroupBox9.ResumeLayout(False)
        Me.GroupBox9.PerformLayout()
        Me.GroupBox6.ResumeLayout(False)
        Me.GroupBox6.PerformLayout()
        Me.ResumeLayout(False)

    End Sub
    Public WithEvents FaTabStrip1 As FarsiLibrary.Win.FATabStrip
    Public WithEvents FaTabStripItem1 As FarsiLibrary.Win.FATabStripItem
    Public WithEvents FaTabStripItem2 As FarsiLibrary.Win.FATabStripItem
    Public WithEvents KryptonCheckBox1 As System.Windows.Forms.CheckBox
    Public WithEvents GroupBox3 As System.Windows.Forms.GroupBox
    Public WithEvents KryptonCheckBox6 As System.Windows.Forms.CheckBox
    Public WithEvents KryptonButton1 As System.Windows.Forms.Button
    Public WithEvents KryptonTextBox1 As System.Windows.Forms.TextBox
    Public WithEvents KryptonLabel1 As System.Windows.Forms.Label
    Public WithEvents FolderBrowserDialog1 As System.Windows.Forms.FolderBrowserDialog
    Public WithEvents KryptonLabel3 As System.Windows.Forms.Label
    Public WithEvents TrackBar1 As System.Windows.Forms.TrackBar
    Public WithEvents KryptonLabel2 As System.Windows.Forms.Label
    Public WithEvents FaTabStripItem3 As FarsiLibrary.Win.FATabStripItem
    Public WithEvents GroupBox11 As System.Windows.Forms.GroupBox
    Public WithEvents cbudb As System.Windows.Forms.CheckBox
    Public WithEvents Button11 As System.Windows.Forms.Button
    Public WithEvents Button7 As System.Windows.Forms.Button
    Public WithEvents GroupBox4 As System.Windows.Forms.GroupBox
    Public WithEvents dgvdb As System.Windows.Forms.DataGridView
    Public WithEvents ofdcs As System.Windows.Forms.OpenFileDialog
    Public WithEvents OpenFileDialog1 As System.Windows.Forms.OpenFileDialog
    Public WithEvents GroupBox1 As System.Windows.Forms.GroupBox
    Public WithEvents ImageList1 As System.Windows.Forms.ImageList
    Public WithEvents Label3 As System.Windows.Forms.Label
    Public WithEvents Button2 As System.Windows.Forms.Button
    Public WithEvents FaTabStripItem5 As FarsiLibrary.Win.FATabStripItem
    Public WithEvents GroupBox5 As System.Windows.Forms.GroupBox
    Public WithEvents btnaddpath As System.Windows.Forms.Button
    Public WithEvents tbaddpath As System.Windows.Forms.TextBox
    Public WithEvents Label4 As System.Windows.Forms.Label
    Public WithEvents Label5 As System.Windows.Forms.Label
    Public WithEvents btnrmpath As System.Windows.Forms.Button
    Public WithEvents lbpaths As System.Windows.Forms.ListBox
    Public WithEvents SuperToolTip1 As Omarslvd.Windows.Forms.SuperToolTip
    Public WithEvents chkUpdates As System.Windows.Forms.CheckBox
    Public WithEvents GroupBox7 As System.Windows.Forms.GroupBox
    Public WithEvents chkEnableParallelCalcs As System.Windows.Forms.CheckBox
    Friend WithEvents cbParallelism As System.Windows.Forms.ComboBox
    Friend WithEvents Label6 As System.Windows.Forms.Label
    Friend WithEvents cbGPU As System.Windows.Forms.ComboBox
    Friend WithEvents Label7 As System.Windows.Forms.Label
    Public WithEvents chkEnableGPUProcessing As System.Windows.Forms.CheckBox
    Friend WithEvents GroupBox8 As System.Windows.Forms.GroupBox
    Friend WithEvents tbGPUCaps As System.Windows.Forms.TextBox
    Friend WithEvents FaTabStripItem6 As FarsiLibrary.Win.FATabStripItem
    Friend WithEvents GroupBox12 As System.Windows.Forms.GroupBox
    Public WithEvents Button4 As System.Windows.Forms.Button
    Friend WithEvents GroupBox10 As System.Windows.Forms.GroupBox
    Public WithEvents dgvIPDB As System.Windows.Forms.DataGridView
    Friend WithEvents DataGridViewTextBoxColumn1 As System.Windows.Forms.DataGridViewTextBoxColumn
    Friend WithEvents DataGridViewTextBoxColumn2 As System.Windows.Forms.DataGridViewTextBoxColumn
    Friend WithEvents DataGridViewTextBoxColumn3 As System.Windows.Forms.DataGridViewTextBoxColumn
    Friend WithEvents DataGridViewImageColumn1 As System.Windows.Forms.DataGridViewImageColumn
    Friend WithEvents Column12 As System.Windows.Forms.DataGridViewTextBoxColumn
    Friend WithEvents Column14 As System.Windows.Forms.DataGridViewTextBoxColumn
    Friend WithEvents Column15 As System.Windows.Forms.DataGridViewTextBoxColumn
    Friend WithEvents Column13 As System.Windows.Forms.DataGridViewImageColumn
    Friend WithEvents BtnEdit As System.Windows.Forms.DataGridViewImageColumn
    Friend WithEvents FaTabStripItem7 As FarsiLibrary.Win.FATabStripItem
    Friend WithEvents GroupBox2 As System.Windows.Forms.GroupBox
    Friend WithEvents GroupBoxAzureConfig As System.Windows.Forms.GroupBox
    Friend WithEvents tbServiceBusNamespace As System.Windows.Forms.TextBox
    Friend WithEvents Label8 As System.Windows.Forms.Label
    Friend WithEvents cbSolverMode As System.Windows.Forms.ComboBox
    Friend WithEvents Label2 As System.Windows.Forms.Label
    Friend WithEvents Label12 As System.Windows.Forms.Label
    Friend WithEvents tbSolverTimeout As System.Windows.Forms.TextBox
    Friend WithEvents Label11 As System.Windows.Forms.Label
    Friend WithEvents GroupBoxNetworkComputerConfig As System.Windows.Forms.GroupBox
    Friend WithEvents tbServerPort As System.Windows.Forms.TextBox
    Friend WithEvents tbServerIP As System.Windows.Forms.TextBox
    Friend WithEvents Label15 As System.Windows.Forms.Label
    Friend WithEvents Label16 As System.Windows.Forms.Label
    Friend WithEvents cbDebugLevel As System.Windows.Forms.ComboBox
    Friend WithEvents Label13 As System.Windows.Forms.Label
    Public WithEvents chkconsole As System.Windows.Forms.CheckBox
    Friend WithEvents GroupBox9 As System.Windows.Forms.GroupBox
    Friend WithEvents GroupBox6 As System.Windows.Forms.GroupBox
    Friend WithEvents ComboBoxUILanguage As System.Windows.Forms.ComboBox
    Public WithEvents chkSolverBreak As System.Windows.Forms.CheckBox
    Public WithEvents chkStorePreviousSolutions As System.Windows.Forms.CheckBox
    Public WithEvents chkShowWhatsNew As System.Windows.Forms.CheckBox
End Class
