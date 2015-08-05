<Global.Microsoft.VisualBasic.CompilerServices.DesignerGenerated()> _
Partial Class AboutBoxMONO
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
        Dim resources As System.ComponentModel.ComponentResourceManager = New System.ComponentModel.ComponentResourceManager(GetType(AboutBoxMONO))
        Me.Label1 = New System.Windows.Forms.Label()
        Me.Button1 = New System.Windows.Forms.Button()
        Me.Version = New System.Windows.Forms.Label()
        Me.Copyright = New System.Windows.Forms.Label()
        Me.FaTabStrip1 = New FarsiLibrary.Win.FATabStrip()
        Me.FaTabStripItem1 = New FarsiLibrary.Win.FATabStripItem()
        Me.Label9 = New System.Windows.Forms.Label()
        Me.LinkLabel1 = New System.Windows.Forms.LinkLabel()
        Me.Label10 = New System.Windows.Forms.Label()
        Me.Label2 = New System.Windows.Forms.Label()
        Me.Label8 = New System.Windows.Forms.Label()
        Me.LinkLabel2 = New System.Windows.Forms.LinkLabel()
        Me.Label3 = New System.Windows.Forms.Label()
        Me.Label5 = New System.Windows.Forms.Label()
        Me.LblCLRInfo = New System.Windows.Forms.Label()
        Me.Label6 = New System.Windows.Forms.Label()
        Me.Label4 = New System.Windows.Forms.Label()
        Me.LabelLicense = New System.Windows.Forms.Label()
        Me.LblOSInfo = New System.Windows.Forms.Label()
        Me.TextBox1 = New System.Windows.Forms.TextBox()
        Me.Label7 = New System.Windows.Forms.Label()
        Me.FaTabStripItem2 = New FarsiLibrary.Win.FATabStripItem()
        Me.DataGridView1 = New System.Windows.Forms.DataGridView()
        Me.Column1 = New System.Windows.Forms.DataGridViewTextBoxColumn()
        Me.Column2 = New System.Windows.Forms.DataGridViewTextBoxColumn()
        Me.Column3 = New System.Windows.Forms.DataGridViewTextBoxColumn()
        Me.Column4 = New System.Windows.Forms.DataGridViewTextBoxColumn()
        Me.Column5 = New System.Windows.Forms.DataGridViewLinkColumn()
        Me.Column6 = New System.Windows.Forms.DataGridViewTextBoxColumn()
        Me.Column7 = New System.Windows.Forms.DataGridViewLinkColumn()
        Me.FaTabStripItem3 = New FarsiLibrary.Win.FATabStripItem()
        Me.AssemblyInfoListView = New System.Windows.Forms.ListView()
        Me.colAssemblyName = CType(New System.Windows.Forms.ColumnHeader(), System.Windows.Forms.ColumnHeader)
        Me.colAssemblyVersion = CType(New System.Windows.Forms.ColumnHeader(), System.Windows.Forms.ColumnHeader)
        Me.colAssemblyBuilt = CType(New System.Windows.Forms.ColumnHeader(), System.Windows.Forms.ColumnHeader)
        Me.colAssemblyCodeBase = CType(New System.Windows.Forms.ColumnHeader(), System.Windows.Forms.ColumnHeader)
        Me.FaTabStripItem4 = New FarsiLibrary.Win.FATabStripItem()
        Me.AssemblyDetailsListView = New System.Windows.Forms.ListView()
        Me.ColumnHeader1 = CType(New System.Windows.Forms.ColumnHeader(), System.Windows.Forms.ColumnHeader)
        Me.ColumnHeader2 = CType(New System.Windows.Forms.ColumnHeader(), System.Windows.Forms.ColumnHeader)
        Me.AssemblyNamesComboBox = New System.Windows.Forms.ComboBox()
        Me.Lblmem = New System.Windows.Forms.Label()
        Me.Label11 = New System.Windows.Forms.Label()
        CType(Me.FaTabStrip1, System.ComponentModel.ISupportInitialize).BeginInit()
        Me.FaTabStrip1.SuspendLayout()
        Me.FaTabStripItem1.SuspendLayout()
        Me.FaTabStripItem2.SuspendLayout()
        CType(Me.DataGridView1, System.ComponentModel.ISupportInitialize).BeginInit()
        Me.FaTabStripItem3.SuspendLayout()
        Me.FaTabStripItem4.SuspendLayout()
        Me.SuspendLayout()
        '
        'Label1
        '
        Me.Label1.AutoSize = True
        Me.Label1.BackColor = System.Drawing.Color.Transparent
        Me.Label1.Font = New System.Drawing.Font("Microsoft Sans Serif", 8.25!, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point, CType(0, Byte))
        Me.Label1.Location = New System.Drawing.Point(12, 10)
        Me.Label1.Name = "Label1"
        Me.Label1.Size = New System.Drawing.Size(404, 13)
        Me.Label1.TabIndex = 1
        Me.Label1.Text = "DWSIM - Open Source Process Simulation, Modeling and Optimization"
        '
        'Button1
        '
        Me.Button1.Location = New System.Drawing.Point(614, 530)
        Me.Button1.Name = "Button1"
        Me.Button1.Size = New System.Drawing.Size(75, 23)
        Me.Button1.TabIndex = 3
        Me.Button1.Text = "OK"
        Me.Button1.UseVisualStyleBackColor = True
        '
        'Version
        '
        Me.Version.AutoSize = True
        Me.Version.BackColor = System.Drawing.Color.Transparent
        Me.Version.Location = New System.Drawing.Point(12, 28)
        Me.Version.Name = "Version"
        Me.Version.Size = New System.Drawing.Size(121, 13)
        Me.Version.TabIndex = 4
        Me.Version.Text = "Version X, Build Y (date)"
        '
        'Copyright
        '
        Me.Copyright.AutoSize = True
        Me.Copyright.BackColor = System.Drawing.Color.Transparent
        Me.Copyright.Location = New System.Drawing.Point(12, 46)
        Me.Copyright.Name = "Copyright"
        Me.Copyright.Size = New System.Drawing.Size(54, 13)
        Me.Copyright.TabIndex = 5
        Me.Copyright.Text = "Copyright "
        '
        'FaTabStrip1
        '
        Me.FaTabStrip1.AlwaysShowClose = False
        Me.FaTabStrip1.AlwaysShowMenuGlyph = False
        Me.FaTabStrip1.Font = New System.Drawing.Font("Tahoma", 8.25!)
        Me.FaTabStrip1.Items.AddRange(New FarsiLibrary.Win.FATabStripItem() {Me.FaTabStripItem1, Me.FaTabStripItem2, Me.FaTabStripItem3, Me.FaTabStripItem4})
        Me.FaTabStrip1.Location = New System.Drawing.Point(8, 69)
        Me.FaTabStrip1.Name = "FaTabStrip1"
        Me.FaTabStrip1.SelectedItem = Me.FaTabStripItem1
        Me.FaTabStrip1.Size = New System.Drawing.Size(679, 455)
        Me.FaTabStrip1.TabIndex = 23
        Me.FaTabStrip1.Text = "FaTabStrip1"
        '
        'FaTabStripItem1
        '
        Me.FaTabStripItem1.CanClose = False
        Me.FaTabStripItem1.Controls.Add(Me.Lblmem)
        Me.FaTabStripItem1.Controls.Add(Me.Label11)
        Me.FaTabStripItem1.Controls.Add(Me.Label9)
        Me.FaTabStripItem1.Controls.Add(Me.LinkLabel1)
        Me.FaTabStripItem1.Controls.Add(Me.Label10)
        Me.FaTabStripItem1.Controls.Add(Me.Label2)
        Me.FaTabStripItem1.Controls.Add(Me.Label8)
        Me.FaTabStripItem1.Controls.Add(Me.LinkLabel2)
        Me.FaTabStripItem1.Controls.Add(Me.Label3)
        Me.FaTabStripItem1.Controls.Add(Me.Label5)
        Me.FaTabStripItem1.Controls.Add(Me.LblCLRInfo)
        Me.FaTabStripItem1.Controls.Add(Me.Label6)
        Me.FaTabStripItem1.Controls.Add(Me.Label4)
        Me.FaTabStripItem1.Controls.Add(Me.LabelLicense)
        Me.FaTabStripItem1.Controls.Add(Me.LblOSInfo)
        Me.FaTabStripItem1.Controls.Add(Me.TextBox1)
        Me.FaTabStripItem1.Controls.Add(Me.Label7)
        Me.FaTabStripItem1.IsDrawn = True
        Me.FaTabStripItem1.Name = "FaTabStripItem1"
        Me.FaTabStripItem1.Selected = True
        Me.FaTabStripItem1.Size = New System.Drawing.Size(677, 434)
        Me.FaTabStripItem1.TabIndex = 0
        Me.FaTabStripItem1.Title = "General Info"
        '
        'Label9
        '
        Me.Label9.AutoSize = True
        Me.Label9.BackColor = System.Drawing.Color.Transparent
        Me.Label9.ForeColor = System.Drawing.SystemColors.ControlText
        Me.Label9.Location = New System.Drawing.Point(133, 10)
        Me.Label9.Name = "Label9"
        Me.Label9.Size = New System.Drawing.Size(253, 13)
        Me.Label9.TabIndex = 21
        Me.Label9.Text = "Daniel Wagner, Gustavo León  and Gregor Reichert"
        '
        'LinkLabel1
        '
        Me.LinkLabel1.AutoSize = True
        Me.LinkLabel1.BackColor = System.Drawing.Color.Transparent
        Me.LinkLabel1.ForeColor = System.Drawing.SystemColors.ControlText
        Me.LinkLabel1.Location = New System.Drawing.Point(133, 79)
        Me.LinkLabel1.Name = "LinkLabel1"
        Me.LinkLabel1.Size = New System.Drawing.Size(150, 13)
        Me.LinkLabel1.TabIndex = 2
        Me.LinkLabel1.TabStop = True
        Me.LinkLabel1.Text = "http://dwsim.inforside.com.br"
        '
        'Label10
        '
        Me.Label10.AutoSize = True
        Me.Label10.BackColor = System.Drawing.Color.Transparent
        Me.Label10.ForeColor = System.Drawing.SystemColors.ControlText
        Me.Label10.Location = New System.Drawing.Point(12, 10)
        Me.Label10.Name = "Label10"
        Me.Label10.Size = New System.Drawing.Size(65, 13)
        Me.Label10.TabIndex = 20
        Me.Label10.Text = "Developers:"
        '
        'Label2
        '
        Me.Label2.AutoSize = True
        Me.Label2.BackColor = System.Drawing.Color.Transparent
        Me.Label2.ForeColor = System.Drawing.SystemColors.ControlText
        Me.Label2.Location = New System.Drawing.Point(12, 79)
        Me.Label2.Name = "Label2"
        Me.Label2.Size = New System.Drawing.Size(55, 13)
        Me.Label2.TabIndex = 6
        Me.Label2.Text = "Websites:"
        '
        'Label8
        '
        Me.Label8.AutoSize = True
        Me.Label8.BackColor = System.Drawing.Color.Transparent
        Me.Label8.ForeColor = System.Drawing.SystemColors.ControlText
        Me.Label8.Location = New System.Drawing.Point(133, 56)
        Me.Label8.Name = "Label8"
        Me.Label8.Size = New System.Drawing.Size(213, 13)
        Me.Label8.TabIndex = 19
        Me.Label8.Text = "Wendel Marcus (wendel@inforside.com.br)"
        '
        'LinkLabel2
        '
        Me.LinkLabel2.AutoSize = True
        Me.LinkLabel2.BackColor = System.Drawing.Color.Transparent
        Me.LinkLabel2.ForeColor = System.Drawing.SystemColors.ControlText
        Me.LinkLabel2.Location = New System.Drawing.Point(133, 102)
        Me.LinkLabel2.Name = "LinkLabel2"
        Me.LinkLabel2.Size = New System.Drawing.Size(221, 13)
        Me.LinkLabel2.TabIndex = 7
        Me.LinkLabel2.TabStop = True
        Me.LinkLabel2.Text = "http://www.sourceforge.net/projects/dwsim"
        '
        'Label3
        '
        Me.Label3.AutoSize = True
        Me.Label3.BackColor = System.Drawing.Color.Transparent
        Me.Label3.ForeColor = System.Drawing.SystemColors.ControlText
        Me.Label3.Location = New System.Drawing.Point(12, 56)
        Me.Label3.Name = "Label3"
        Me.Label3.Size = New System.Drawing.Size(113, 13)
        Me.Label3.TabIndex = 18
        Me.Label3.Text = "Splash Screen Design:"
        '
        'Label5
        '
        Me.Label5.AutoSize = True
        Me.Label5.BackColor = System.Drawing.Color.Transparent
        Me.Label5.ForeColor = System.Drawing.SystemColors.ControlText
        Me.Label5.Location = New System.Drawing.Point(12, 33)
        Me.Label5.Name = "Label5"
        Me.Label5.Size = New System.Drawing.Size(65, 13)
        Me.Label5.TabIndex = 10
        Me.Label5.Text = "Translators:"
        '
        'LblCLRInfo
        '
        Me.LblCLRInfo.AutoSize = True
        Me.LblCLRInfo.BackColor = System.Drawing.Color.Transparent
        Me.LblCLRInfo.ForeColor = System.Drawing.SystemColors.ControlText
        Me.LblCLRInfo.Location = New System.Drawing.Point(133, 153)
        Me.LblCLRInfo.Name = "LblCLRInfo"
        Me.LblCLRInfo.Size = New System.Drawing.Size(44, 13)
        Me.LblCLRInfo.TabIndex = 17
        Me.LblCLRInfo.Text = "[clrinfo]"
        '
        'Label6
        '
        Me.Label6.AutoSize = True
        Me.Label6.BackColor = System.Drawing.Color.Transparent
        Me.Label6.ForeColor = System.Drawing.SystemColors.ControlText
        Me.Label6.Location = New System.Drawing.Point(133, 33)
        Me.Label6.Name = "Label6"
        Me.Label6.Size = New System.Drawing.Size(506, 13)
        Me.Label6.TabIndex = 11
        Me.Label6.Text = "Daniel Wagner (English), Abad Lira / Gustavo León (Spanish), Rainer Göllnitz / Gr" & _
    "egor Reichert (German)"
        '
        'Label4
        '
        Me.Label4.AutoSize = True
        Me.Label4.BackColor = System.Drawing.Color.Transparent
        Me.Label4.ForeColor = System.Drawing.SystemColors.ControlText
        Me.Label4.Location = New System.Drawing.Point(12, 153)
        Me.Label4.Name = "Label4"
        Me.Label4.Size = New System.Drawing.Size(53, 13)
        Me.Label4.TabIndex = 16
        Me.Label4.Text = "CLR Info:"
        '
        'LabelLicense
        '
        Me.LabelLicense.AutoSize = True
        Me.LabelLicense.BackColor = System.Drawing.Color.Transparent
        Me.LabelLicense.FlatStyle = System.Windows.Forms.FlatStyle.Flat
        Me.LabelLicense.Font = New System.Drawing.Font("Microsoft Sans Serif", 8.25!, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, CType(0, Byte))
        Me.LabelLicense.ForeColor = System.Drawing.SystemColors.ControlText
        Me.LabelLicense.Location = New System.Drawing.Point(12, 212)
        Me.LabelLicense.Name = "LabelLicense"
        Me.LabelLicense.Size = New System.Drawing.Size(422, 13)
        Me.LabelLicense.TabIndex = 12
        Me.LabelLicense.Text = "DWSIM is released under the terms of the GNU General Public License (GPL) version" & _
    " 3:"
        '
        'LblOSInfo
        '
        Me.LblOSInfo.AutoSize = True
        Me.LblOSInfo.BackColor = System.Drawing.Color.Transparent
        Me.LblOSInfo.ForeColor = System.Drawing.SystemColors.ControlText
        Me.LblOSInfo.Location = New System.Drawing.Point(133, 129)
        Me.LblOSInfo.Name = "LblOSInfo"
        Me.LblOSInfo.Size = New System.Drawing.Size(44, 13)
        Me.LblOSInfo.TabIndex = 15
        Me.LblOSInfo.Text = "[osinfo]"
        '
        'TextBox1
        '
        Me.TextBox1.BackColor = System.Drawing.Color.White
        Me.TextBox1.Font = New System.Drawing.Font("Courier New", 8.25!, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, CType(0, Byte))
        Me.TextBox1.Location = New System.Drawing.Point(15, 232)
        Me.TextBox1.Multiline = True
        Me.TextBox1.Name = "TextBox1"
        Me.TextBox1.ReadOnly = True
        Me.TextBox1.ScrollBars = System.Windows.Forms.ScrollBars.Vertical
        Me.TextBox1.Size = New System.Drawing.Size(649, 192)
        Me.TextBox1.TabIndex = 13
        Me.TextBox1.Text = resources.GetString("TextBox1.Text")
        '
        'Label7
        '
        Me.Label7.AutoSize = True
        Me.Label7.BackColor = System.Drawing.Color.Transparent
        Me.Label7.ForeColor = System.Drawing.SystemColors.ControlText
        Me.Label7.Location = New System.Drawing.Point(12, 129)
        Me.Label7.Name = "Label7"
        Me.Label7.Size = New System.Drawing.Size(48, 13)
        Me.Label7.TabIndex = 14
        Me.Label7.Text = "OS Info:"
        '
        'FaTabStripItem2
        '
        Me.FaTabStripItem2.CanClose = False
        Me.FaTabStripItem2.Controls.Add(Me.DataGridView1)
        Me.FaTabStripItem2.IsDrawn = True
        Me.FaTabStripItem2.Name = "FaTabStripItem2"
        Me.FaTabStripItem2.Size = New System.Drawing.Size(677, 398)
        Me.FaTabStripItem2.TabIndex = 1
        Me.FaTabStripItem2.Title = "External Components"
        '
        'DataGridView1
        '
        Me.DataGridView1.AllowUserToAddRows = False
        Me.DataGridView1.AllowUserToDeleteRows = False
        Me.DataGridView1.AllowUserToResizeRows = False
        Me.DataGridView1.AutoSizeColumnsMode = System.Windows.Forms.DataGridViewAutoSizeColumnsMode.AllCells
        Me.DataGridView1.AutoSizeRowsMode = System.Windows.Forms.DataGridViewAutoSizeRowsMode.AllCells
        Me.DataGridView1.CellBorderStyle = System.Windows.Forms.DataGridViewCellBorderStyle.None
        Me.DataGridView1.ColumnHeadersHeightSizeMode = System.Windows.Forms.DataGridViewColumnHeadersHeightSizeMode.AutoSize
        Me.DataGridView1.Columns.AddRange(New System.Windows.Forms.DataGridViewColumn() {Me.Column1, Me.Column2, Me.Column3, Me.Column4, Me.Column5, Me.Column6, Me.Column7})
        Me.DataGridView1.Dock = System.Windows.Forms.DockStyle.Fill
        Me.DataGridView1.Location = New System.Drawing.Point(0, 0)
        Me.DataGridView1.Name = "DataGridView1"
        Me.DataGridView1.ReadOnly = True
        Me.DataGridView1.RowHeadersVisible = False
        Me.DataGridView1.SelectionMode = System.Windows.Forms.DataGridViewSelectionMode.FullRowSelect
        Me.DataGridView1.Size = New System.Drawing.Size(677, 398)
        Me.DataGridView1.TabIndex = 0
        '
        'Column1
        '
        Me.Column1.HeaderText = "Name"
        Me.Column1.Name = "Column1"
        Me.Column1.ReadOnly = True
        Me.Column1.Width = 59
        '
        'Column2
        '
        Me.Column2.HeaderText = "Version"
        Me.Column2.Name = "Column2"
        Me.Column2.ReadOnly = True
        Me.Column2.Width = 67
        '
        'Column3
        '
        Me.Column3.HeaderText = "Year"
        Me.Column3.Name = "Column3"
        Me.Column3.ReadOnly = True
        Me.Column3.Width = 54
        '
        'Column4
        '
        Me.Column4.HeaderText = "Copyright"
        Me.Column4.Name = "Column4"
        Me.Column4.ReadOnly = True
        Me.Column4.Width = 79
        '
        'Column5
        '
        Me.Column5.HeaderText = "Website"
        Me.Column5.Name = "Column5"
        Me.Column5.ReadOnly = True
        Me.Column5.Resizable = System.Windows.Forms.DataGridViewTriState.[True]
        Me.Column5.SortMode = System.Windows.Forms.DataGridViewColumnSortMode.Automatic
        Me.Column5.Width = 71
        '
        'Column6
        '
        Me.Column6.HeaderText = "License Name"
        Me.Column6.Name = "Column6"
        Me.Column6.ReadOnly = True
        Me.Column6.Width = 97
        '
        'Column7
        '
        Me.Column7.HeaderText = "License Text"
        Me.Column7.Name = "Column7"
        Me.Column7.ReadOnly = True
        Me.Column7.Resizable = System.Windows.Forms.DataGridViewTriState.[True]
        Me.Column7.SortMode = System.Windows.Forms.DataGridViewColumnSortMode.Automatic
        Me.Column7.Width = 92
        '
        'FaTabStripItem3
        '
        Me.FaTabStripItem3.CanClose = False
        Me.FaTabStripItem3.Controls.Add(Me.AssemblyInfoListView)
        Me.FaTabStripItem3.IsDrawn = True
        Me.FaTabStripItem3.Name = "FaTabStripItem3"
        Me.FaTabStripItem3.Size = New System.Drawing.Size(677, 398)
        Me.FaTabStripItem3.TabIndex = 2
        Me.FaTabStripItem3.Title = "Loaded Assemblies"
        '
        'AssemblyInfoListView
        '
        Me.AssemblyInfoListView.Columns.AddRange(New System.Windows.Forms.ColumnHeader() {Me.colAssemblyName, Me.colAssemblyVersion, Me.colAssemblyBuilt, Me.colAssemblyCodeBase})
        Me.AssemblyInfoListView.Dock = System.Windows.Forms.DockStyle.Fill
        Me.AssemblyInfoListView.FullRowSelect = True
        Me.AssemblyInfoListView.Location = New System.Drawing.Point(0, 0)
        Me.AssemblyInfoListView.MultiSelect = False
        Me.AssemblyInfoListView.Name = "AssemblyInfoListView"
        Me.AssemblyInfoListView.Size = New System.Drawing.Size(677, 398)
        Me.AssemblyInfoListView.Sorting = System.Windows.Forms.SortOrder.Ascending
        Me.AssemblyInfoListView.TabIndex = 14
        Me.AssemblyInfoListView.UseCompatibleStateImageBehavior = False
        Me.AssemblyInfoListView.View = System.Windows.Forms.View.Details
        '
        'colAssemblyName
        '
        Me.colAssemblyName.Text = "Assembly"
        Me.colAssemblyName.Width = 123
        '
        'colAssemblyVersion
        '
        Me.colAssemblyVersion.Text = "Version"
        Me.colAssemblyVersion.Width = 100
        '
        'colAssemblyBuilt
        '
        Me.colAssemblyBuilt.Text = "Build Date"
        Me.colAssemblyBuilt.Width = 130
        '
        'colAssemblyCodeBase
        '
        Me.colAssemblyCodeBase.Text = "CodeBase"
        Me.colAssemblyCodeBase.Width = 750
        '
        'FaTabStripItem4
        '
        Me.FaTabStripItem4.CanClose = False
        Me.FaTabStripItem4.Controls.Add(Me.AssemblyDetailsListView)
        Me.FaTabStripItem4.Controls.Add(Me.AssemblyNamesComboBox)
        Me.FaTabStripItem4.IsDrawn = True
        Me.FaTabStripItem4.Name = "FaTabStripItem4"
        Me.FaTabStripItem4.Size = New System.Drawing.Size(677, 398)
        Me.FaTabStripItem4.TabIndex = 3
        Me.FaTabStripItem4.Title = "Assembly Details"
        '
        'AssemblyDetailsListView
        '
        Me.AssemblyDetailsListView.Columns.AddRange(New System.Windows.Forms.ColumnHeader() {Me.ColumnHeader1, Me.ColumnHeader2})
        Me.AssemblyDetailsListView.Dock = System.Windows.Forms.DockStyle.Fill
        Me.AssemblyDetailsListView.FullRowSelect = True
        Me.AssemblyDetailsListView.HeaderStyle = System.Windows.Forms.ColumnHeaderStyle.Nonclickable
        Me.AssemblyDetailsListView.Location = New System.Drawing.Point(0, 21)
        Me.AssemblyDetailsListView.Name = "AssemblyDetailsListView"
        Me.AssemblyDetailsListView.Size = New System.Drawing.Size(677, 377)
        Me.AssemblyDetailsListView.Sorting = System.Windows.Forms.SortOrder.Ascending
        Me.AssemblyDetailsListView.TabIndex = 21
        Me.AssemblyDetailsListView.UseCompatibleStateImageBehavior = False
        Me.AssemblyDetailsListView.View = System.Windows.Forms.View.Details
        '
        'ColumnHeader1
        '
        Me.ColumnHeader1.Text = "Assembly Key"
        Me.ColumnHeader1.Width = 120
        '
        'ColumnHeader2
        '
        Me.ColumnHeader2.Text = "Value"
        Me.ColumnHeader2.Width = 700
        '
        'AssemblyNamesComboBox
        '
        Me.AssemblyNamesComboBox.Dock = System.Windows.Forms.DockStyle.Top
        Me.AssemblyNamesComboBox.DropDownStyle = System.Windows.Forms.ComboBoxStyle.DropDownList
        Me.AssemblyNamesComboBox.Location = New System.Drawing.Point(0, 0)
        Me.AssemblyNamesComboBox.Name = "AssemblyNamesComboBox"
        Me.AssemblyNamesComboBox.Size = New System.Drawing.Size(677, 21)
        Me.AssemblyNamesComboBox.Sorted = True
        Me.AssemblyNamesComboBox.TabIndex = 20
        '
        'Lblmem
        '
        Me.Lblmem.AutoSize = True
        Me.Lblmem.BackColor = System.Drawing.Color.Transparent
        Me.Lblmem.Location = New System.Drawing.Point(133, 178)
        Me.Lblmem.Name = "Lblmem"
        Me.Lblmem.Size = New System.Drawing.Size(55, 13)
        Me.Lblmem.TabIndex = 25
        Me.Lblmem.Text = "[meminfo]"
        '
        'Label11
        '
        Me.Label11.AutoSize = True
        Me.Label11.BackColor = System.Drawing.Color.Transparent
        Me.Label11.Location = New System.Drawing.Point(12, 178)
        Me.Label11.Name = "Label11"
        Me.Label11.Size = New System.Drawing.Size(82, 13)
        Me.Label11.TabIndex = 24
        Me.Label11.Text = "Memory Usage:"
        '
        'AboutBoxMONO
        '
        Me.AutoScaleDimensions = New System.Drawing.SizeF(6.0!, 13.0!)
        Me.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font
        Me.BackgroundImageLayout = System.Windows.Forms.ImageLayout.Stretch
        Me.ClientSize = New System.Drawing.Size(699, 589)
        Me.ControlBox = False
        Me.Controls.Add(Me.FaTabStrip1)
        Me.Controls.Add(Me.Copyright)
        Me.Controls.Add(Me.Version)
        Me.Controls.Add(Me.Button1)
        Me.Controls.Add(Me.Label1)
        Me.DoubleBuffered = True
        Me.FormBorderStyle = System.Windows.Forms.FormBorderStyle.FixedSingle
        Me.Name = "AboutBoxMONO"
        Me.ShowIcon = False
        Me.ShowInTaskbar = False
        Me.StartPosition = System.Windows.Forms.FormStartPosition.CenterParent
        Me.Text = "About DWSIM"
        CType(Me.FaTabStrip1, System.ComponentModel.ISupportInitialize).EndInit()
        Me.FaTabStrip1.ResumeLayout(False)
        Me.FaTabStripItem1.ResumeLayout(False)
        Me.FaTabStripItem1.PerformLayout()
        Me.FaTabStripItem2.ResumeLayout(False)
        CType(Me.DataGridView1, System.ComponentModel.ISupportInitialize).EndInit()
        Me.FaTabStripItem3.ResumeLayout(False)
        Me.FaTabStripItem4.ResumeLayout(False)
        Me.ResumeLayout(False)
        Me.PerformLayout()

    End Sub
    Public WithEvents Label1 As System.Windows.Forms.Label
    Public WithEvents Button1 As System.Windows.Forms.Button
    Public WithEvents Version As System.Windows.Forms.Label
    Public WithEvents Copyright As System.Windows.Forms.Label
    Friend WithEvents FaTabStrip1 As FarsiLibrary.Win.FATabStrip
    Friend WithEvents FaTabStripItem1 As FarsiLibrary.Win.FATabStripItem
    Public WithEvents Label9 As System.Windows.Forms.Label
    Public WithEvents LinkLabel1 As System.Windows.Forms.LinkLabel
    Public WithEvents Label10 As System.Windows.Forms.Label
    Public WithEvents Label2 As System.Windows.Forms.Label
    Public WithEvents Label8 As System.Windows.Forms.Label
    Public WithEvents LinkLabel2 As System.Windows.Forms.LinkLabel
    Public WithEvents Label3 As System.Windows.Forms.Label
    Public WithEvents Label5 As System.Windows.Forms.Label
    Public WithEvents LblCLRInfo As System.Windows.Forms.Label
    Public WithEvents Label6 As System.Windows.Forms.Label
    Public WithEvents Label4 As System.Windows.Forms.Label
    Public WithEvents LabelLicense As System.Windows.Forms.Label
    Public WithEvents LblOSInfo As System.Windows.Forms.Label
    Public WithEvents TextBox1 As System.Windows.Forms.TextBox
    Public WithEvents Label7 As System.Windows.Forms.Label
    Friend WithEvents FaTabStripItem2 As FarsiLibrary.Win.FATabStripItem
    Friend WithEvents DataGridView1 As System.Windows.Forms.DataGridView
    Friend WithEvents Column1 As System.Windows.Forms.DataGridViewTextBoxColumn
    Friend WithEvents Column2 As System.Windows.Forms.DataGridViewTextBoxColumn
    Friend WithEvents Column3 As System.Windows.Forms.DataGridViewTextBoxColumn
    Friend WithEvents Column4 As System.Windows.Forms.DataGridViewTextBoxColumn
    Friend WithEvents Column5 As System.Windows.Forms.DataGridViewLinkColumn
    Friend WithEvents Column6 As System.Windows.Forms.DataGridViewTextBoxColumn
    Friend WithEvents Column7 As System.Windows.Forms.DataGridViewLinkColumn
    Friend WithEvents FaTabStripItem3 As FarsiLibrary.Win.FATabStripItem
    Friend WithEvents AssemblyInfoListView As System.Windows.Forms.ListView
    Friend WithEvents colAssemblyName As System.Windows.Forms.ColumnHeader
    Friend WithEvents colAssemblyVersion As System.Windows.Forms.ColumnHeader
    Friend WithEvents colAssemblyBuilt As System.Windows.Forms.ColumnHeader
    Friend WithEvents colAssemblyCodeBase As System.Windows.Forms.ColumnHeader
    Friend WithEvents FaTabStripItem4 As FarsiLibrary.Win.FATabStripItem
    Friend WithEvents AssemblyDetailsListView As System.Windows.Forms.ListView
    Friend WithEvents ColumnHeader1 As System.Windows.Forms.ColumnHeader
    Friend WithEvents ColumnHeader2 As System.Windows.Forms.ColumnHeader
    Friend WithEvents AssemblyNamesComboBox As System.Windows.Forms.ComboBox
    Public WithEvents Lblmem As System.Windows.Forms.Label
    Public WithEvents Label11 As System.Windows.Forms.Label
End Class
