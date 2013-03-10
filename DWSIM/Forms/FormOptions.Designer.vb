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
        Me.FaTabStrip1 = New FarsiLibrary.Win.FATabStrip()
        Me.FaTabStripItem3 = New FarsiLibrary.Win.FATabStripItem()
        Me.GroupBox11 = New System.Windows.Forms.GroupBox()
        Me.cbudb = New System.Windows.Forms.CheckBox()
        Me.Button11 = New System.Windows.Forms.Button()
        Me.Button7 = New System.Windows.Forms.Button()
        Me.GroupBox4 = New System.Windows.Forms.GroupBox()
        Me.Label2 = New System.Windows.Forms.Label()
        Me.dgvdb = New System.Windows.Forms.DataGridView()
        Me.Column12 = New System.Windows.Forms.DataGridViewTextBoxColumn()
        Me.Column13 = New System.Windows.Forms.DataGridViewImageColumn()
        Me.Column14 = New System.Windows.Forms.DataGridViewTextBoxColumn()
        Me.Column15 = New System.Windows.Forms.DataGridViewTextBoxColumn()
        Me.Column1 = New System.Windows.Forms.DataGridViewButtonColumn()
        Me.FaTabStripItem1 = New FarsiLibrary.Win.FATabStripItem()
        Me.GroupBox7 = New System.Windows.Forms.GroupBox()
        Me.GroupBox8 = New System.Windows.Forms.GroupBox()
        Me.tbGPUCaps = New System.Windows.Forms.TextBox()
        Me.cbGPU = New System.Windows.Forms.ComboBox()
        Me.Label7 = New System.Windows.Forms.Label()
        Me.chkEnableGPUProcessing = New System.Windows.Forms.CheckBox()
        Me.cbParallelism = New System.Windows.Forms.ComboBox()
        Me.Label6 = New System.Windows.Forms.Label()
        Me.chkEnableParallelCalcs = New System.Windows.Forms.CheckBox()
        Me.GroupBox6 = New System.Windows.Forms.GroupBox()
        Me.chkUpdates = New System.Windows.Forms.CheckBox()
        Me.chkconsole = New System.Windows.Forms.CheckBox()
        Me.GroupBox2 = New System.Windows.Forms.GroupBox()
        Me.KryptonCheckBox1 = New System.Windows.Forms.CheckBox()
        Me.FaTabStripItem4 = New FarsiLibrary.Win.FATabStripItem()
        Me.GroupBox1 = New System.Windows.Forms.GroupBox()
        Me.Button1 = New System.Windows.Forms.Button()
        Me.ListView1 = New System.Windows.Forms.ListView()
        Me.ImageList1 = New System.Windows.Forms.ImageList(Me.components)
        Me.Label3 = New System.Windows.Forms.Label()
        Me.Label1 = New System.Windows.Forms.Label()
        Me.FaTabStripItem2 = New FarsiLibrary.Win.FATabStripItem()
        Me.GroupBox3 = New System.Windows.Forms.GroupBox()
        Me.Button2 = New System.Windows.Forms.Button()
        Me.PictureBox1 = New System.Windows.Forms.PictureBox()
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
        Me.FolderBrowserDialog1 = New System.Windows.Forms.FolderBrowserDialog()
        Me.ofdcs = New System.Windows.Forms.OpenFileDialog()
        Me.OpenFileDialog1 = New System.Windows.Forms.OpenFileDialog()
        Me.SuperToolTip1 = New Omarslvd.Windows.Forms.SuperToolTip()
        CType(Me.FaTabStrip1, System.ComponentModel.ISupportInitialize).BeginInit()
        Me.FaTabStrip1.SuspendLayout()
        Me.FaTabStripItem3.SuspendLayout()
        Me.GroupBox11.SuspendLayout()
        Me.GroupBox4.SuspendLayout()
        CType(Me.dgvdb, System.ComponentModel.ISupportInitialize).BeginInit()
        Me.FaTabStripItem1.SuspendLayout()
        Me.GroupBox7.SuspendLayout()
        Me.GroupBox8.SuspendLayout()
        Me.GroupBox6.SuspendLayout()
        Me.GroupBox2.SuspendLayout()
        Me.FaTabStripItem4.SuspendLayout()
        Me.GroupBox1.SuspendLayout()
        Me.FaTabStripItem2.SuspendLayout()
        Me.GroupBox3.SuspendLayout()
        CType(Me.PictureBox1, System.ComponentModel.ISupportInitialize).BeginInit()
        CType(Me.TrackBar1, System.ComponentModel.ISupportInitialize).BeginInit()
        Me.FaTabStripItem5.SuspendLayout()
        Me.GroupBox5.SuspendLayout()
        Me.SuspendLayout()
        '
        'FaTabStrip1
        '
        resources.ApplyResources(Me.FaTabStrip1, "FaTabStrip1")
        Me.FaTabStrip1.AlwaysShowClose = False
        Me.FaTabStrip1.AlwaysShowMenuGlyph = False
        Me.FaTabStrip1.Items.AddRange(New FarsiLibrary.Win.FATabStripItem() {Me.FaTabStripItem3, Me.FaTabStripItem1, Me.FaTabStripItem4, Me.FaTabStripItem2, Me.FaTabStripItem5})
        Me.FaTabStrip1.Name = "FaTabStrip1"
        Me.FaTabStrip1.SelectedItem = Me.FaTabStripItem3
        Me.SuperToolTip1.SetSuperToolTipColor1(Me.FaTabStrip1, System.Drawing.Color.Empty)
        Me.SuperToolTip1.SetSuperToolTipColor2(Me.FaTabStrip1, System.Drawing.Color.Empty)
        Me.SuperToolTip1.SetToolTip(Me.FaTabStrip1, Global.DWSIM.My.Resources.DWSIM.NewVersionAvailable)
        '
        'FaTabStripItem3
        '
        resources.ApplyResources(Me.FaTabStripItem3, "FaTabStripItem3")
        Me.FaTabStripItem3.CanClose = False
        Me.FaTabStripItem3.Controls.Add(Me.GroupBox11)
        Me.FaTabStripItem3.Controls.Add(Me.GroupBox4)
        Me.FaTabStripItem3.IsDrawn = True
        Me.FaTabStripItem3.Name = "FaTabStripItem3"
        Me.FaTabStripItem3.Selected = True
        Me.SuperToolTip1.SetSuperToolTipColor1(Me.FaTabStripItem3, System.Drawing.Color.Empty)
        Me.SuperToolTip1.SetSuperToolTipColor2(Me.FaTabStripItem3, System.Drawing.Color.Empty)
        Me.SuperToolTip1.SetToolTip(Me.FaTabStripItem3, Global.DWSIM.My.Resources.DWSIM.NewVersionAvailable)
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
        Me.SuperToolTip1.SetToolTip(Me.GroupBox11, Global.DWSIM.My.Resources.DWSIM.NewVersionAvailable)
        '
        'cbudb
        '
        resources.ApplyResources(Me.cbudb, "cbudb")
        Me.cbudb.ImageKey = Global.DWSIM.My.Resources.DWSIM.NewVersionAvailable
        Me.cbudb.Name = "cbudb"
        Me.SuperToolTip1.SetSuperToolTipColor1(Me.cbudb, System.Drawing.Color.Empty)
        Me.SuperToolTip1.SetSuperToolTipColor2(Me.cbudb, System.Drawing.Color.Empty)
        Me.SuperToolTip1.SetToolTip(Me.cbudb, Global.DWSIM.My.Resources.DWSIM.NewVersionAvailable)
        Me.cbudb.UseVisualStyleBackColor = True
        '
        'Button11
        '
        resources.ApplyResources(Me.Button11, "Button11")
        Me.Button11.ImageKey = Global.DWSIM.My.Resources.DWSIM.NewVersionAvailable
        Me.Button11.Name = "Button11"
        Me.SuperToolTip1.SetSuperToolTipColor1(Me.Button11, System.Drawing.Color.Empty)
        Me.SuperToolTip1.SetSuperToolTipColor2(Me.Button11, System.Drawing.Color.Empty)
        Me.SuperToolTip1.SetToolTip(Me.Button11, Global.DWSIM.My.Resources.DWSIM.NewVersionAvailable)
        Me.Button11.UseVisualStyleBackColor = True
        '
        'Button7
        '
        resources.ApplyResources(Me.Button7, "Button7")
        Me.Button7.ImageKey = Global.DWSIM.My.Resources.DWSIM.NewVersionAvailable
        Me.Button7.Name = "Button7"
        Me.SuperToolTip1.SetSuperToolTipColor1(Me.Button7, System.Drawing.Color.Empty)
        Me.SuperToolTip1.SetSuperToolTipColor2(Me.Button7, System.Drawing.Color.Empty)
        Me.SuperToolTip1.SetToolTip(Me.Button7, Global.DWSIM.My.Resources.DWSIM.NewVersionAvailable)
        Me.Button7.UseVisualStyleBackColor = True
        '
        'GroupBox4
        '
        resources.ApplyResources(Me.GroupBox4, "GroupBox4")
        Me.GroupBox4.Controls.Add(Me.Label2)
        Me.GroupBox4.Controls.Add(Me.dgvdb)
        Me.GroupBox4.Name = "GroupBox4"
        Me.SuperToolTip1.SetSuperToolTipColor1(Me.GroupBox4, System.Drawing.Color.Empty)
        Me.SuperToolTip1.SetSuperToolTipColor2(Me.GroupBox4, System.Drawing.Color.Empty)
        Me.GroupBox4.TabStop = False
        Me.SuperToolTip1.SetToolTip(Me.GroupBox4, Global.DWSIM.My.Resources.DWSIM.NewVersionAvailable)
        '
        'Label2
        '
        resources.ApplyResources(Me.Label2, "Label2")
        Me.Label2.ImageKey = Global.DWSIM.My.Resources.DWSIM.NewVersionAvailable
        Me.Label2.Name = "Label2"
        Me.SuperToolTip1.SetSuperToolTipColor1(Me.Label2, System.Drawing.Color.Empty)
        Me.SuperToolTip1.SetSuperToolTipColor2(Me.Label2, System.Drawing.Color.Empty)
        Me.SuperToolTip1.SetToolTip(Me.Label2, Global.DWSIM.My.Resources.DWSIM.NewVersionAvailable)
        '
        'dgvdb
        '
        resources.ApplyResources(Me.dgvdb, "dgvdb")
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
        Me.dgvdb.Columns.AddRange(New System.Windows.Forms.DataGridViewColumn() {Me.Column12, Me.Column13, Me.Column14, Me.Column15, Me.Column1})
        Me.dgvdb.GridColor = System.Drawing.SystemColors.Control
        Me.dgvdb.Name = "dgvdb"
        Me.dgvdb.ReadOnly = True
        Me.dgvdb.RowHeadersBorderStyle = System.Windows.Forms.DataGridViewHeaderBorderStyle.None
        Me.dgvdb.RowHeadersVisible = False
        Me.dgvdb.RowTemplate.DefaultCellStyle.Padding = New System.Windows.Forms.Padding(0, 5, 0, 5)
        Me.dgvdb.SelectionMode = System.Windows.Forms.DataGridViewSelectionMode.FullRowSelect
        Me.dgvdb.ShowCellErrors = False
        Me.SuperToolTip1.SetSuperToolTipColor1(Me.dgvdb, System.Drawing.Color.Empty)
        Me.SuperToolTip1.SetSuperToolTipColor2(Me.dgvdb, System.Drawing.Color.Empty)
        Me.SuperToolTip1.SetToolTip(Me.dgvdb, Global.DWSIM.My.Resources.DWSIM.NewVersionAvailable)
        '
        'Column12
        '
        Me.Column12.AutoSizeMode = System.Windows.Forms.DataGridViewAutoSizeColumnMode.AllCells
        Me.Column12.FillWeight = 10.0!
        resources.ApplyResources(Me.Column12, "Column12")
        Me.Column12.Name = "Column12"
        Me.Column12.ReadOnly = True
        Me.Column12.Resizable = System.Windows.Forms.DataGridViewTriState.[False]
        Me.Column12.ToolTipText = Global.DWSIM.My.Resources.DWSIM.NewVersionAvailable
        '
        'Column13
        '
        Me.Column13.AutoSizeMode = System.Windows.Forms.DataGridViewAutoSizeColumnMode.AllCells
        Me.Column13.FillWeight = 10.0!
        resources.ApplyResources(Me.Column13, "Column13")
        Me.Column13.Name = "Column13"
        Me.Column13.ReadOnly = True
        Me.Column13.Resizable = System.Windows.Forms.DataGridViewTriState.[False]
        Me.Column13.ToolTipText = Global.DWSIM.My.Resources.DWSIM.NewVersionAvailable
        '
        'Column14
        '
        Me.Column14.AutoSizeMode = System.Windows.Forms.DataGridViewAutoSizeColumnMode.AllCells
        Me.Column14.FillWeight = 30.0!
        resources.ApplyResources(Me.Column14, "Column14")
        Me.Column14.Name = "Column14"
        Me.Column14.ReadOnly = True
        Me.Column14.Resizable = System.Windows.Forms.DataGridViewTriState.[False]
        Me.Column14.ToolTipText = Global.DWSIM.My.Resources.DWSIM.NewVersionAvailable
        '
        'Column15
        '
        Me.Column15.AutoSizeMode = System.Windows.Forms.DataGridViewAutoSizeColumnMode.Fill
        resources.ApplyResources(Me.Column15, "Column15")
        Me.Column15.Name = "Column15"
        Me.Column15.ReadOnly = True
        Me.Column15.Resizable = System.Windows.Forms.DataGridViewTriState.[False]
        Me.Column15.ToolTipText = Global.DWSIM.My.Resources.DWSIM.NewVersionAvailable
        '
        'Column1
        '
        Me.Column1.AutoSizeMode = System.Windows.Forms.DataGridViewAutoSizeColumnMode.AllCells
        resources.ApplyResources(Me.Column1, "Column1")
        Me.Column1.Name = "Column1"
        Me.Column1.ReadOnly = True
        Me.Column1.ToolTipText = Global.DWSIM.My.Resources.DWSIM.NewVersionAvailable
        '
        'FaTabStripItem1
        '
        resources.ApplyResources(Me.FaTabStripItem1, "FaTabStripItem1")
        Me.FaTabStripItem1.CanClose = False
        Me.FaTabStripItem1.Controls.Add(Me.GroupBox7)
        Me.FaTabStripItem1.Controls.Add(Me.GroupBox6)
        Me.FaTabStripItem1.Controls.Add(Me.GroupBox2)
        Me.FaTabStripItem1.IsDrawn = True
        Me.FaTabStripItem1.Name = "FaTabStripItem1"
        Me.SuperToolTip1.SetSuperToolTipColor1(Me.FaTabStripItem1, System.Drawing.Color.Empty)
        Me.SuperToolTip1.SetSuperToolTipColor2(Me.FaTabStripItem1, System.Drawing.Color.Empty)
        Me.SuperToolTip1.SetToolTip(Me.FaTabStripItem1, Global.DWSIM.My.Resources.DWSIM.NewVersionAvailable)
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
        Me.SuperToolTip1.SetToolTip(Me.GroupBox7, Global.DWSIM.My.Resources.DWSIM.NewVersionAvailable)
        '
        'GroupBox8
        '
        resources.ApplyResources(Me.GroupBox8, "GroupBox8")
        Me.GroupBox8.Controls.Add(Me.tbGPUCaps)
        Me.GroupBox8.Name = "GroupBox8"
        Me.SuperToolTip1.SetSuperToolTipColor1(Me.GroupBox8, System.Drawing.Color.Empty)
        Me.SuperToolTip1.SetSuperToolTipColor2(Me.GroupBox8, System.Drawing.Color.Empty)
        Me.GroupBox8.TabStop = False
        Me.SuperToolTip1.SetToolTip(Me.GroupBox8, Global.DWSIM.My.Resources.DWSIM.NewVersionAvailable)
        '
        'tbGPUCaps
        '
        resources.ApplyResources(Me.tbGPUCaps, "tbGPUCaps")
        Me.tbGPUCaps.Name = "tbGPUCaps"
        Me.tbGPUCaps.ReadOnly = True
        Me.SuperToolTip1.SetSuperToolTipColor1(Me.tbGPUCaps, System.Drawing.Color.Empty)
        Me.SuperToolTip1.SetSuperToolTipColor2(Me.tbGPUCaps, System.Drawing.Color.Empty)
        Me.SuperToolTip1.SetToolTip(Me.tbGPUCaps, Global.DWSIM.My.Resources.DWSIM.NewVersionAvailable)
        '
        'cbGPU
        '
        resources.ApplyResources(Me.cbGPU, "cbGPU")
        Me.cbGPU.DropDownStyle = System.Windows.Forms.ComboBoxStyle.DropDownList
        Me.cbGPU.FormattingEnabled = True
        Me.cbGPU.Name = "cbGPU"
        Me.SuperToolTip1.SetSuperToolTipColor1(Me.cbGPU, System.Drawing.Color.Empty)
        Me.SuperToolTip1.SetSuperToolTipColor2(Me.cbGPU, System.Drawing.Color.Empty)
        Me.SuperToolTip1.SetToolTip(Me.cbGPU, Global.DWSIM.My.Resources.DWSIM.NewVersionAvailable)
        '
        'Label7
        '
        resources.ApplyResources(Me.Label7, "Label7")
        Me.Label7.ImageKey = Global.DWSIM.My.Resources.DWSIM.NewVersionAvailable
        Me.Label7.Name = "Label7"
        Me.SuperToolTip1.SetSuperToolTipColor1(Me.Label7, System.Drawing.Color.Empty)
        Me.SuperToolTip1.SetSuperToolTipColor2(Me.Label7, System.Drawing.Color.Empty)
        Me.SuperToolTip1.SetToolTip(Me.Label7, Global.DWSIM.My.Resources.DWSIM.NewVersionAvailable)
        '
        'chkEnableGPUProcessing
        '
        resources.ApplyResources(Me.chkEnableGPUProcessing, "chkEnableGPUProcessing")
        Me.chkEnableGPUProcessing.ImageKey = Global.DWSIM.My.Resources.DWSIM.NewVersionAvailable
        Me.chkEnableGPUProcessing.Name = "chkEnableGPUProcessing"
        Me.SuperToolTip1.SetSuperToolTipColor1(Me.chkEnableGPUProcessing, System.Drawing.Color.Empty)
        Me.SuperToolTip1.SetSuperToolTipColor2(Me.chkEnableGPUProcessing, System.Drawing.Color.Empty)
        Me.SuperToolTip1.SetToolTip(Me.chkEnableGPUProcessing, Global.DWSIM.My.Resources.DWSIM.NewVersionAvailable)
        '
        'cbParallelism
        '
        resources.ApplyResources(Me.cbParallelism, "cbParallelism")
        Me.cbParallelism.DropDownStyle = System.Windows.Forms.ComboBoxStyle.DropDownList
        Me.cbParallelism.FormattingEnabled = True
        Me.cbParallelism.Name = "cbParallelism"
        Me.SuperToolTip1.SetSuperToolTipColor1(Me.cbParallelism, System.Drawing.Color.Empty)
        Me.SuperToolTip1.SetSuperToolTipColor2(Me.cbParallelism, System.Drawing.Color.Empty)
        Me.SuperToolTip1.SetToolTip(Me.cbParallelism, Global.DWSIM.My.Resources.DWSIM.NewVersionAvailable)
        '
        'Label6
        '
        resources.ApplyResources(Me.Label6, "Label6")
        Me.Label6.ImageKey = Global.DWSIM.My.Resources.DWSIM.NewVersionAvailable
        Me.Label6.Name = "Label6"
        Me.SuperToolTip1.SetSuperToolTipColor1(Me.Label6, System.Drawing.Color.Empty)
        Me.SuperToolTip1.SetSuperToolTipColor2(Me.Label6, System.Drawing.Color.Empty)
        Me.SuperToolTip1.SetToolTip(Me.Label6, Global.DWSIM.My.Resources.DWSIM.NewVersionAvailable)
        '
        'chkEnableParallelCalcs
        '
        resources.ApplyResources(Me.chkEnableParallelCalcs, "chkEnableParallelCalcs")
        Me.chkEnableParallelCalcs.ImageKey = Global.DWSIM.My.Resources.DWSIM.NewVersionAvailable
        Me.chkEnableParallelCalcs.Name = "chkEnableParallelCalcs"
        Me.SuperToolTip1.SetSuperToolTipColor1(Me.chkEnableParallelCalcs, System.Drawing.Color.Empty)
        Me.SuperToolTip1.SetSuperToolTipColor2(Me.chkEnableParallelCalcs, System.Drawing.Color.Empty)
        Me.SuperToolTip1.SetToolTip(Me.chkEnableParallelCalcs, Global.DWSIM.My.Resources.DWSIM.NewVersionAvailable)
        '
        'GroupBox6
        '
        resources.ApplyResources(Me.GroupBox6, "GroupBox6")
        Me.GroupBox6.Controls.Add(Me.chkUpdates)
        Me.GroupBox6.Controls.Add(Me.chkconsole)
        Me.GroupBox6.Name = "GroupBox6"
        Me.SuperToolTip1.SetSuperToolTipColor1(Me.GroupBox6, System.Drawing.Color.Empty)
        Me.SuperToolTip1.SetSuperToolTipColor2(Me.GroupBox6, System.Drawing.Color.Empty)
        Me.GroupBox6.TabStop = False
        Me.SuperToolTip1.SetToolTip(Me.GroupBox6, Global.DWSIM.My.Resources.DWSIM.NewVersionAvailable)
        '
        'chkUpdates
        '
        resources.ApplyResources(Me.chkUpdates, "chkUpdates")
        Me.chkUpdates.ImageKey = Global.DWSIM.My.Resources.DWSIM.NewVersionAvailable
        Me.chkUpdates.Name = "chkUpdates"
        Me.SuperToolTip1.SetSuperToolTipColor1(Me.chkUpdates, System.Drawing.Color.Empty)
        Me.SuperToolTip1.SetSuperToolTipColor2(Me.chkUpdates, System.Drawing.Color.Empty)
        Me.SuperToolTip1.SetToolTip(Me.chkUpdates, Global.DWSIM.My.Resources.DWSIM.NewVersionAvailable)
        '
        'chkconsole
        '
        resources.ApplyResources(Me.chkconsole, "chkconsole")
        Me.chkconsole.ImageKey = Global.DWSIM.My.Resources.DWSIM.NewVersionAvailable
        Me.chkconsole.Name = "chkconsole"
        Me.SuperToolTip1.SetSuperToolTipColor1(Me.chkconsole, System.Drawing.Color.Empty)
        Me.SuperToolTip1.SetSuperToolTipColor2(Me.chkconsole, System.Drawing.Color.Empty)
        Me.SuperToolTip1.SetToolTip(Me.chkconsole, Global.DWSIM.My.Resources.DWSIM.NewVersionAvailable)
        '
        'GroupBox2
        '
        resources.ApplyResources(Me.GroupBox2, "GroupBox2")
        Me.GroupBox2.Controls.Add(Me.KryptonCheckBox1)
        Me.GroupBox2.Name = "GroupBox2"
        Me.SuperToolTip1.SetSuperToolTipColor1(Me.GroupBox2, System.Drawing.Color.Empty)
        Me.SuperToolTip1.SetSuperToolTipColor2(Me.GroupBox2, System.Drawing.Color.Empty)
        Me.GroupBox2.TabStop = False
        Me.SuperToolTip1.SetToolTip(Me.GroupBox2, Global.DWSIM.My.Resources.DWSIM.NewVersionAvailable)
        '
        'KryptonCheckBox1
        '
        resources.ApplyResources(Me.KryptonCheckBox1, "KryptonCheckBox1")
        Me.KryptonCheckBox1.ImageKey = Global.DWSIM.My.Resources.DWSIM.NewVersionAvailable
        Me.KryptonCheckBox1.Name = "KryptonCheckBox1"
        Me.SuperToolTip1.SetSuperToolTipColor1(Me.KryptonCheckBox1, System.Drawing.Color.Empty)
        Me.SuperToolTip1.SetSuperToolTipColor2(Me.KryptonCheckBox1, System.Drawing.Color.Empty)
        Me.SuperToolTip1.SetToolTip(Me.KryptonCheckBox1, Global.DWSIM.My.Resources.DWSIM.NewVersionAvailable)
        '
        'FaTabStripItem4
        '
        resources.ApplyResources(Me.FaTabStripItem4, "FaTabStripItem4")
        Me.FaTabStripItem4.CanClose = False
        Me.FaTabStripItem4.Controls.Add(Me.GroupBox1)
        Me.FaTabStripItem4.IsDrawn = True
        Me.FaTabStripItem4.Name = "FaTabStripItem4"
        Me.SuperToolTip1.SetSuperToolTipColor1(Me.FaTabStripItem4, System.Drawing.Color.Empty)
        Me.SuperToolTip1.SetSuperToolTipColor2(Me.FaTabStripItem4, System.Drawing.Color.Empty)
        Me.SuperToolTip1.SetToolTip(Me.FaTabStripItem4, Global.DWSIM.My.Resources.DWSIM.NewVersionAvailable)
        '
        'GroupBox1
        '
        resources.ApplyResources(Me.GroupBox1, "GroupBox1")
        Me.GroupBox1.Controls.Add(Me.Button1)
        Me.GroupBox1.Controls.Add(Me.ListView1)
        Me.GroupBox1.Controls.Add(Me.Label3)
        Me.GroupBox1.Controls.Add(Me.Label1)
        Me.GroupBox1.Name = "GroupBox1"
        Me.SuperToolTip1.SetSuperToolTipColor1(Me.GroupBox1, System.Drawing.Color.Empty)
        Me.SuperToolTip1.SetSuperToolTipColor2(Me.GroupBox1, System.Drawing.Color.Empty)
        Me.GroupBox1.TabStop = False
        Me.SuperToolTip1.SetToolTip(Me.GroupBox1, Global.DWSIM.My.Resources.DWSIM.NewVersionAvailable)
        '
        'Button1
        '
        resources.ApplyResources(Me.Button1, "Button1")
        Me.Button1.ImageKey = Global.DWSIM.My.Resources.DWSIM.NewVersionAvailable
        Me.Button1.Name = "Button1"
        Me.SuperToolTip1.SetSuperToolTipColor1(Me.Button1, System.Drawing.Color.Empty)
        Me.SuperToolTip1.SetSuperToolTipColor2(Me.Button1, System.Drawing.Color.Empty)
        Me.SuperToolTip1.SetToolTip(Me.Button1, Global.DWSIM.My.Resources.DWSIM.NewVersionAvailable)
        Me.Button1.UseVisualStyleBackColor = True
        '
        'ListView1
        '
        resources.ApplyResources(Me.ListView1, "ListView1")
        Me.ListView1.FullRowSelect = True
        Me.ListView1.Items.AddRange(New System.Windows.Forms.ListViewItem() {CType(resources.GetObject("ListView1.Items"), System.Windows.Forms.ListViewItem), CType(resources.GetObject("ListView1.Items1"), System.Windows.Forms.ListViewItem), CType(resources.GetObject("ListView1.Items2"), System.Windows.Forms.ListViewItem), CType(resources.GetObject("ListView1.Items3"), System.Windows.Forms.ListViewItem)})
        Me.ListView1.LargeImageList = Me.ImageList1
        Me.ListView1.MultiSelect = False
        Me.ListView1.Name = "ListView1"
        Me.ListView1.SmallImageList = Me.ImageList1
        Me.SuperToolTip1.SetSuperToolTipColor1(Me.ListView1, System.Drawing.Color.Empty)
        Me.SuperToolTip1.SetSuperToolTipColor2(Me.ListView1, System.Drawing.Color.Empty)
        Me.SuperToolTip1.SetToolTip(Me.ListView1, Global.DWSIM.My.Resources.DWSIM.NewVersionAvailable)
        Me.ListView1.UseCompatibleStateImageBehavior = False
        Me.ListView1.View = System.Windows.Forms.View.List
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
        'Label3
        '
        resources.ApplyResources(Me.Label3, "Label3")
        Me.Label3.ImageKey = Global.DWSIM.My.Resources.DWSIM.NewVersionAvailable
        Me.Label3.Name = "Label3"
        Me.SuperToolTip1.SetSuperToolTipColor1(Me.Label3, System.Drawing.Color.Empty)
        Me.SuperToolTip1.SetSuperToolTipColor2(Me.Label3, System.Drawing.Color.Empty)
        Me.SuperToolTip1.SetToolTip(Me.Label3, Global.DWSIM.My.Resources.DWSIM.NewVersionAvailable)
        '
        'Label1
        '
        resources.ApplyResources(Me.Label1, "Label1")
        Me.Label1.ImageKey = Global.DWSIM.My.Resources.DWSIM.NewVersionAvailable
        Me.Label1.Name = "Label1"
        Me.SuperToolTip1.SetSuperToolTipColor1(Me.Label1, System.Drawing.Color.Empty)
        Me.SuperToolTip1.SetSuperToolTipColor2(Me.Label1, System.Drawing.Color.Empty)
        Me.SuperToolTip1.SetToolTip(Me.Label1, Global.DWSIM.My.Resources.DWSIM.NewVersionAvailable)
        '
        'FaTabStripItem2
        '
        resources.ApplyResources(Me.FaTabStripItem2, "FaTabStripItem2")
        Me.FaTabStripItem2.CanClose = False
        Me.FaTabStripItem2.Controls.Add(Me.GroupBox3)
        Me.FaTabStripItem2.IsDrawn = True
        Me.FaTabStripItem2.Name = "FaTabStripItem2"
        Me.SuperToolTip1.SetSuperToolTipColor1(Me.FaTabStripItem2, System.Drawing.Color.Empty)
        Me.SuperToolTip1.SetSuperToolTipColor2(Me.FaTabStripItem2, System.Drawing.Color.Empty)
        Me.SuperToolTip1.SetToolTip(Me.FaTabStripItem2, Global.DWSIM.My.Resources.DWSIM.NewVersionAvailable)
        '
        'GroupBox3
        '
        resources.ApplyResources(Me.GroupBox3, "GroupBox3")
        Me.GroupBox3.Controls.Add(Me.Button2)
        Me.GroupBox3.Controls.Add(Me.PictureBox1)
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
        Me.SuperToolTip1.SetToolTip(Me.GroupBox3, Global.DWSIM.My.Resources.DWSIM.NewVersionAvailable)
        '
        'Button2
        '
        resources.ApplyResources(Me.Button2, "Button2")
        Me.Button2.ImageKey = Global.DWSIM.My.Resources.DWSIM.NewVersionAvailable
        Me.Button2.Name = "Button2"
        Me.SuperToolTip1.SetSuperToolTipColor1(Me.Button2, System.Drawing.Color.Empty)
        Me.SuperToolTip1.SetSuperToolTipColor2(Me.Button2, System.Drawing.Color.Empty)
        Me.SuperToolTip1.SetToolTip(Me.Button2, Global.DWSIM.My.Resources.DWSIM.NewVersionAvailable)
        '
        'PictureBox1
        '
        resources.ApplyResources(Me.PictureBox1, "PictureBox1")
        Me.PictureBox1.BackColor = System.Drawing.Color.Transparent
        Me.PictureBox1.Image = Global.DWSIM.My.Resources.Resources.icon_info
        Me.PictureBox1.Name = "PictureBox1"
        Me.SuperToolTip1.SetSuperToolTipColor1(Me.PictureBox1, System.Drawing.Color.Empty)
        Me.SuperToolTip1.SetSuperToolTipColor2(Me.PictureBox1, System.Drawing.Color.Empty)
        Me.PictureBox1.TabStop = False
        Me.SuperToolTip1.SetToolTip(Me.PictureBox1, Global.DWSIM.My.Resources.DWSIM.NewVersionAvailable)
        '
        'KryptonLabel3
        '
        resources.ApplyResources(Me.KryptonLabel3, "KryptonLabel3")
        Me.KryptonLabel3.ImageKey = Global.DWSIM.My.Resources.DWSIM.NewVersionAvailable
        Me.KryptonLabel3.Name = "KryptonLabel3"
        Me.SuperToolTip1.SetSuperToolTipColor1(Me.KryptonLabel3, System.Drawing.Color.Empty)
        Me.SuperToolTip1.SetSuperToolTipColor2(Me.KryptonLabel3, System.Drawing.Color.Empty)
        Me.SuperToolTip1.SetToolTip(Me.KryptonLabel3, Global.DWSIM.My.Resources.DWSIM.NewVersionAvailable)
        '
        'TrackBar1
        '
        resources.ApplyResources(Me.TrackBar1, "TrackBar1")
        Me.TrackBar1.Maximum = 60
        Me.TrackBar1.Minimum = 1
        Me.TrackBar1.Name = "TrackBar1"
        Me.SuperToolTip1.SetSuperToolTipColor1(Me.TrackBar1, System.Drawing.Color.Empty)
        Me.SuperToolTip1.SetSuperToolTipColor2(Me.TrackBar1, System.Drawing.Color.Empty)
        Me.SuperToolTip1.SetToolTip(Me.TrackBar1, Global.DWSIM.My.Resources.DWSIM.NewVersionAvailable)
        Me.TrackBar1.Value = 5
        '
        'KryptonLabel2
        '
        resources.ApplyResources(Me.KryptonLabel2, "KryptonLabel2")
        Me.KryptonLabel2.ImageKey = Global.DWSIM.My.Resources.DWSIM.NewVersionAvailable
        Me.KryptonLabel2.Name = "KryptonLabel2"
        Me.SuperToolTip1.SetSuperToolTipColor1(Me.KryptonLabel2, System.Drawing.Color.Empty)
        Me.SuperToolTip1.SetSuperToolTipColor2(Me.KryptonLabel2, System.Drawing.Color.Empty)
        Me.SuperToolTip1.SetToolTip(Me.KryptonLabel2, Global.DWSIM.My.Resources.DWSIM.NewVersionAvailable)
        '
        'KryptonButton1
        '
        resources.ApplyResources(Me.KryptonButton1, "KryptonButton1")
        Me.KryptonButton1.ImageKey = Global.DWSIM.My.Resources.DWSIM.NewVersionAvailable
        Me.KryptonButton1.Name = "KryptonButton1"
        Me.SuperToolTip1.SetSuperToolTipColor1(Me.KryptonButton1, System.Drawing.Color.Empty)
        Me.SuperToolTip1.SetSuperToolTipColor2(Me.KryptonButton1, System.Drawing.Color.Empty)
        Me.SuperToolTip1.SetToolTip(Me.KryptonButton1, Global.DWSIM.My.Resources.DWSIM.NewVersionAvailable)
        '
        'KryptonTextBox1
        '
        resources.ApplyResources(Me.KryptonTextBox1, "KryptonTextBox1")
        Me.KryptonTextBox1.Name = "KryptonTextBox1"
        Me.KryptonTextBox1.ReadOnly = True
        Me.SuperToolTip1.SetSuperToolTipColor1(Me.KryptonTextBox1, System.Drawing.Color.Empty)
        Me.SuperToolTip1.SetSuperToolTipColor2(Me.KryptonTextBox1, System.Drawing.Color.Empty)
        Me.SuperToolTip1.SetToolTip(Me.KryptonTextBox1, Global.DWSIM.My.Resources.DWSIM.NewVersionAvailable)
        '
        'KryptonLabel1
        '
        resources.ApplyResources(Me.KryptonLabel1, "KryptonLabel1")
        Me.KryptonLabel1.ImageKey = Global.DWSIM.My.Resources.DWSIM.NewVersionAvailable
        Me.KryptonLabel1.Name = "KryptonLabel1"
        Me.SuperToolTip1.SetSuperToolTipColor1(Me.KryptonLabel1, System.Drawing.Color.Empty)
        Me.SuperToolTip1.SetSuperToolTipColor2(Me.KryptonLabel1, System.Drawing.Color.Empty)
        Me.SuperToolTip1.SetToolTip(Me.KryptonLabel1, Global.DWSIM.My.Resources.DWSIM.NewVersionAvailable)
        '
        'KryptonCheckBox6
        '
        resources.ApplyResources(Me.KryptonCheckBox6, "KryptonCheckBox6")
        Me.KryptonCheckBox6.ImageKey = Global.DWSIM.My.Resources.DWSIM.NewVersionAvailable
        Me.KryptonCheckBox6.Name = "KryptonCheckBox6"
        Me.SuperToolTip1.SetSuperToolTipColor1(Me.KryptonCheckBox6, System.Drawing.Color.Empty)
        Me.SuperToolTip1.SetSuperToolTipColor2(Me.KryptonCheckBox6, System.Drawing.Color.Empty)
        Me.SuperToolTip1.SetToolTip(Me.KryptonCheckBox6, Global.DWSIM.My.Resources.DWSIM.NewVersionAvailable)
        '
        'FaTabStripItem5
        '
        resources.ApplyResources(Me.FaTabStripItem5, "FaTabStripItem5")
        Me.FaTabStripItem5.CanClose = False
        Me.FaTabStripItem5.Controls.Add(Me.GroupBox5)
        Me.FaTabStripItem5.IsDrawn = True
        Me.FaTabStripItem5.Name = "FaTabStripItem5"
        Me.SuperToolTip1.SetSuperToolTipColor1(Me.FaTabStripItem5, System.Drawing.Color.Empty)
        Me.SuperToolTip1.SetSuperToolTipColor2(Me.FaTabStripItem5, System.Drawing.Color.Empty)
        Me.SuperToolTip1.SetToolTip(Me.FaTabStripItem5, Global.DWSIM.My.Resources.DWSIM.NewVersionAvailable)
        '
        'GroupBox5
        '
        resources.ApplyResources(Me.GroupBox5, "GroupBox5")
        Me.GroupBox5.Controls.Add(Me.lbpaths)
        Me.GroupBox5.Controls.Add(Me.Label5)
        Me.GroupBox5.Controls.Add(Me.btnrmpath)
        Me.GroupBox5.Controls.Add(Me.btnaddpath)
        Me.GroupBox5.Controls.Add(Me.tbaddpath)
        Me.GroupBox5.Controls.Add(Me.Label4)
        Me.GroupBox5.Name = "GroupBox5"
        Me.SuperToolTip1.SetSuperToolTipColor1(Me.GroupBox5, System.Drawing.Color.Empty)
        Me.SuperToolTip1.SetSuperToolTipColor2(Me.GroupBox5, System.Drawing.Color.Empty)
        Me.GroupBox5.TabStop = False
        Me.SuperToolTip1.SetToolTip(Me.GroupBox5, Global.DWSIM.My.Resources.DWSIM.NewVersionAvailable)
        '
        'lbpaths
        '
        resources.ApplyResources(Me.lbpaths, "lbpaths")
        Me.lbpaths.FormattingEnabled = True
        Me.lbpaths.Name = "lbpaths"
        Me.SuperToolTip1.SetSuperToolTipColor1(Me.lbpaths, System.Drawing.Color.Empty)
        Me.SuperToolTip1.SetSuperToolTipColor2(Me.lbpaths, System.Drawing.Color.Empty)
        Me.SuperToolTip1.SetToolTip(Me.lbpaths, Global.DWSIM.My.Resources.DWSIM.NewVersionAvailable)
        '
        'Label5
        '
        resources.ApplyResources(Me.Label5, "Label5")
        Me.Label5.ImageKey = Global.DWSIM.My.Resources.DWSIM.NewVersionAvailable
        Me.Label5.Name = "Label5"
        Me.SuperToolTip1.SetSuperToolTipColor1(Me.Label5, System.Drawing.Color.Empty)
        Me.SuperToolTip1.SetSuperToolTipColor2(Me.Label5, System.Drawing.Color.Empty)
        Me.SuperToolTip1.SetToolTip(Me.Label5, Global.DWSIM.My.Resources.DWSIM.NewVersionAvailable)
        '
        'btnrmpath
        '
        resources.ApplyResources(Me.btnrmpath, "btnrmpath")
        Me.btnrmpath.ImageKey = Global.DWSIM.My.Resources.DWSIM.NewVersionAvailable
        Me.btnrmpath.Name = "btnrmpath"
        Me.SuperToolTip1.SetSuperToolTipColor1(Me.btnrmpath, System.Drawing.Color.Empty)
        Me.SuperToolTip1.SetSuperToolTipColor2(Me.btnrmpath, System.Drawing.Color.Empty)
        Me.SuperToolTip1.SetToolTip(Me.btnrmpath, Global.DWSIM.My.Resources.DWSIM.NewVersionAvailable)
        Me.btnrmpath.UseVisualStyleBackColor = True
        '
        'btnaddpath
        '
        resources.ApplyResources(Me.btnaddpath, "btnaddpath")
        Me.btnaddpath.ImageKey = Global.DWSIM.My.Resources.DWSIM.NewVersionAvailable
        Me.btnaddpath.Name = "btnaddpath"
        Me.SuperToolTip1.SetSuperToolTipColor1(Me.btnaddpath, System.Drawing.Color.Empty)
        Me.SuperToolTip1.SetSuperToolTipColor2(Me.btnaddpath, System.Drawing.Color.Empty)
        Me.SuperToolTip1.SetToolTip(Me.btnaddpath, Global.DWSIM.My.Resources.DWSIM.NewVersionAvailable)
        Me.btnaddpath.UseVisualStyleBackColor = True
        '
        'tbaddpath
        '
        resources.ApplyResources(Me.tbaddpath, "tbaddpath")
        Me.tbaddpath.Name = "tbaddpath"
        Me.SuperToolTip1.SetSuperToolTipColor1(Me.tbaddpath, System.Drawing.Color.Empty)
        Me.SuperToolTip1.SetSuperToolTipColor2(Me.tbaddpath, System.Drawing.Color.Empty)
        Me.SuperToolTip1.SetToolTip(Me.tbaddpath, Global.DWSIM.My.Resources.DWSIM.NewVersionAvailable)
        '
        'Label4
        '
        resources.ApplyResources(Me.Label4, "Label4")
        Me.Label4.ImageKey = Global.DWSIM.My.Resources.DWSIM.NewVersionAvailable
        Me.Label4.Name = "Label4"
        Me.SuperToolTip1.SetSuperToolTipColor1(Me.Label4, System.Drawing.Color.Empty)
        Me.SuperToolTip1.SetSuperToolTipColor2(Me.Label4, System.Drawing.Color.Empty)
        Me.SuperToolTip1.SetToolTip(Me.Label4, Global.DWSIM.My.Resources.DWSIM.NewVersionAvailable)
        '
        'FolderBrowserDialog1
        '
        resources.ApplyResources(Me.FolderBrowserDialog1, "FolderBrowserDialog1")
        Me.FolderBrowserDialog1.SelectedPath = Global.DWSIM.My.Resources.DWSIM.NewVersionAvailable
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
        Me.SuperToolTip1.SetToolTip(Me, Global.DWSIM.My.Resources.DWSIM.NewVersionAvailable)
        CType(Me.FaTabStrip1, System.ComponentModel.ISupportInitialize).EndInit()
        Me.FaTabStrip1.ResumeLayout(False)
        Me.FaTabStripItem3.ResumeLayout(False)
        Me.GroupBox11.ResumeLayout(False)
        Me.GroupBox11.PerformLayout()
        Me.GroupBox4.ResumeLayout(False)
        Me.GroupBox4.PerformLayout()
        CType(Me.dgvdb, System.ComponentModel.ISupportInitialize).EndInit()
        Me.FaTabStripItem1.ResumeLayout(False)
        Me.GroupBox7.ResumeLayout(False)
        Me.GroupBox7.PerformLayout()
        Me.GroupBox8.ResumeLayout(False)
        Me.GroupBox8.PerformLayout()
        Me.GroupBox6.ResumeLayout(False)
        Me.GroupBox6.PerformLayout()
        Me.GroupBox2.ResumeLayout(False)
        Me.GroupBox2.PerformLayout()
        Me.FaTabStripItem4.ResumeLayout(False)
        Me.GroupBox1.ResumeLayout(False)
        Me.GroupBox1.PerformLayout()
        Me.FaTabStripItem2.ResumeLayout(False)
        Me.GroupBox3.ResumeLayout(False)
        Me.GroupBox3.PerformLayout()
        CType(Me.PictureBox1, System.ComponentModel.ISupportInitialize).EndInit()
        CType(Me.TrackBar1, System.ComponentModel.ISupportInitialize).EndInit()
        Me.FaTabStripItem5.ResumeLayout(False)
        Me.GroupBox5.ResumeLayout(False)
        Me.GroupBox5.PerformLayout()
        Me.ResumeLayout(False)

    End Sub
    Public WithEvents FaTabStrip1 As FarsiLibrary.Win.FATabStrip
    Public WithEvents FaTabStripItem1 As FarsiLibrary.Win.FATabStripItem
    Public WithEvents FaTabStripItem2 As FarsiLibrary.Win.FATabStripItem
    Public WithEvents GroupBox2 As System.Windows.Forms.GroupBox
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
    Public WithEvents PictureBox1 As System.Windows.Forms.PictureBox
    Public WithEvents FaTabStripItem3 As FarsiLibrary.Win.FATabStripItem
    Public WithEvents GroupBox11 As System.Windows.Forms.GroupBox
    Public WithEvents cbudb As System.Windows.Forms.CheckBox
    Public WithEvents Button11 As System.Windows.Forms.Button
    Public WithEvents Button7 As System.Windows.Forms.Button
    Public WithEvents GroupBox4 As System.Windows.Forms.GroupBox
    Public WithEvents Label2 As System.Windows.Forms.Label
    Public WithEvents dgvdb As System.Windows.Forms.DataGridView
    Public WithEvents ofdcs As System.Windows.Forms.OpenFileDialog
    Public WithEvents OpenFileDialog1 As System.Windows.Forms.OpenFileDialog
    Public WithEvents Column12 As System.Windows.Forms.DataGridViewTextBoxColumn
    Public WithEvents Column13 As System.Windows.Forms.DataGridViewImageColumn
    Public WithEvents Column14 As System.Windows.Forms.DataGridViewTextBoxColumn
    Public WithEvents Column15 As System.Windows.Forms.DataGridViewTextBoxColumn
    Public WithEvents Column1 As System.Windows.Forms.DataGridViewButtonColumn
    Public WithEvents FaTabStripItem4 As FarsiLibrary.Win.FATabStripItem
    Public WithEvents GroupBox1 As System.Windows.Forms.GroupBox
    Public WithEvents Button1 As System.Windows.Forms.Button
    Public WithEvents ListView1 As System.Windows.Forms.ListView
    Public WithEvents ImageList1 As System.Windows.Forms.ImageList
    Public WithEvents Label3 As System.Windows.Forms.Label
    Public WithEvents Label1 As System.Windows.Forms.Label
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
    Public WithEvents GroupBox6 As System.Windows.Forms.GroupBox
    Public WithEvents chkconsole As System.Windows.Forms.CheckBox
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
End Class
