<Global.Microsoft.VisualBasic.CompilerServices.DesignerGenerated()> _
Partial Class frmObjListView
    Inherits WeifenLuo.WinFormsUI.Docking.DockContent

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
        Dim resources As System.ComponentModel.ComponentResourceManager = New System.ComponentModel.ComponentResourceManager(GetType(frmObjListView))
        Me.ImageList2 = New System.Windows.Forms.ImageList(Me.components)
        Me.DataGridView1 = New System.Windows.Forms.DataGridView()
        Me.col0 = New System.Windows.Forms.DataGridViewTextBoxColumn()
        Me.col1 = New System.Windows.Forms.DataGridViewImageColumn()
        Me.col2 = New System.Windows.Forms.DataGridViewTextBoxColumn()
        Me.dckMenu = New System.Windows.Forms.ContextMenuStrip(Me.components)
        Me.FloatToolStripMenuItem = New System.Windows.Forms.ToolStripMenuItem()
        Me.DockLeftToolStripMenuItem = New System.Windows.Forms.ToolStripMenuItem()
        Me.DockRightToolStripMenuItem = New System.Windows.Forms.ToolStripMenuItem()
        Me.DockTopToolStripMenuItem = New System.Windows.Forms.ToolStripMenuItem()
        Me.DockBottomToolStripMenuItem = New System.Windows.Forms.ToolStripMenuItem()
        Me.DockLeftAutoHideToolStripMenuItem = New System.Windows.Forms.ToolStripMenuItem()
        Me.DockRightAutoHideToolStripMenuItem = New System.Windows.Forms.ToolStripMenuItem()
        Me.DockTopAutoHideToolStripMenuItem = New System.Windows.Forms.ToolStripMenuItem()
        Me.DockBottomAutoHideToolStripMenuItem = New System.Windows.Forms.ToolStripMenuItem()
        Me.DocumentToolStripMenuItem = New System.Windows.Forms.ToolStripMenuItem()
        CType(Me.DataGridView1, System.ComponentModel.ISupportInitialize).BeginInit()
        Me.dckMenu.SuspendLayout()
        Me.SuspendLayout()
        '
        'ImageList2
        '
        Me.ImageList2.ImageStream = CType(resources.GetObject("ImageList2.ImageStream"), System.Windows.Forms.ImageListStreamer)
        Me.ImageList2.TransparentColor = System.Drawing.Color.White
        Me.ImageList2.Images.SetKeyName(0, "rot_comp.png")
        Me.ImageList2.Images.SetKeyName(1, "hx_cooler.png")
        Me.ImageList2.Images.SetKeyName(2, "hx_heater.png")
        Me.ImageList2.Images.SetKeyName(3, "node_mix.png")
        Me.ImageList2.Images.SetKeyName(4, "node_split.png")
        Me.ImageList2.Images.SetKeyName(5, "pipe-m.png")
        Me.ImageList2.Images.SetKeyName(6, "rot_pump.png")
        Me.ImageList2.Images.SetKeyName(7, "r_conv3.png")
        Me.ImageList2.Images.SetKeyName(8, "r_cstr2.png")
        Me.ImageList2.Images.SetKeyName(9, "r_equib2.png")
        Me.ImageList2.Images.SetKeyName(10, "r_gibbs2.png")
        Me.ImageList2.Images.SetKeyName(11, "r_pfr2.png")
        Me.ImageList2.Images.SetKeyName(12, "lo_matre.png")
        Me.ImageList2.Images.SetKeyName(13, "tank-m.png")
        Me.ImageList2.Images.SetKeyName(14, "rot_expa.png")
        Me.ImageList2.Images.SetKeyName(15, "valve-m.png")
        Me.ImageList2.Images.SetKeyName(16, "col_short.png")
        Me.ImageList2.Images.SetKeyName(17, "col_dist.png")
        Me.ImageList2.Images.SetKeyName(18, "col_absorb.png")
        Me.ImageList2.Images.SetKeyName(19, "col_absorbcond.png")
        Me.ImageList2.Images.SetKeyName(20, "col_absorbrebo.png")
        Me.ImageList2.Images.SetKeyName(21, "arrow_mat.png")
        Me.ImageList2.Images.SetKeyName(22, "arrow_energy.png")
        Me.ImageList2.Images.SetKeyName(23, "lo_adjust.png")
        Me.ImageList2.Images.SetKeyName(24, "lo_specif.png")
        Me.ImageList2.Images.SetKeyName(25, "lo_energyre.png")
        Me.ImageList2.Images.SetKeyName(26, "vessel_compsep.png")
        Me.ImageList2.Images.SetKeyName(27, "vessel_filter.png")
        Me.ImageList2.Images.SetKeyName(28, "vessel_flash.png")
        Me.ImageList2.Images.SetKeyName(29, "vessel_solidsep.png")
        Me.ImageList2.Images.SetKeyName(30, "orifice-m.png")
        Me.ImageList2.Images.SetKeyName(31, "uo_custom.png")
        Me.ImageList2.Images.SetKeyName(32, "uo_excel.png")
        Me.ImageList2.Images.SetKeyName(33, "uo_flowsheet.png")
        Me.ImageList2.Images.SetKeyName(34, "colan2.jpg")
        Me.ImageList2.Images.SetKeyName(35, "hx.png")
        '
        'DataGridView1
        '
        Me.DataGridView1.AllowUserToAddRows = False
        Me.DataGridView1.AllowUserToDeleteRows = False
        Me.DataGridView1.AllowUserToOrderColumns = True
        Me.DataGridView1.AllowUserToResizeColumns = False
        Me.DataGridView1.AllowUserToResizeRows = False
        Me.DataGridView1.AutoSizeColumnsMode = System.Windows.Forms.DataGridViewAutoSizeColumnsMode.Fill
        Me.DataGridView1.BackgroundColor = System.Drawing.Color.White
        Me.DataGridView1.BorderStyle = System.Windows.Forms.BorderStyle.None
        Me.DataGridView1.CellBorderStyle = System.Windows.Forms.DataGridViewCellBorderStyle.None
        Me.DataGridView1.ColumnHeadersHeightSizeMode = System.Windows.Forms.DataGridViewColumnHeadersHeightSizeMode.AutoSize
        Me.DataGridView1.ColumnHeadersVisible = False
        Me.DataGridView1.Columns.AddRange(New System.Windows.Forms.DataGridViewColumn() {Me.col0, Me.col1, Me.col2})
        resources.ApplyResources(Me.DataGridView1, "DataGridView1")
        Me.DataGridView1.MultiSelect = False
        Me.DataGridView1.Name = "DataGridView1"
        Me.DataGridView1.ReadOnly = True
        Me.DataGridView1.RowHeadersVisible = False
        Me.DataGridView1.RowTemplate.Height = 40
        Me.DataGridView1.SelectionMode = System.Windows.Forms.DataGridViewSelectionMode.FullRowSelect
        '
        'col0
        '
        resources.ApplyResources(Me.col0, "col0")
        Me.col0.Name = "col0"
        Me.col0.ReadOnly = True
        '
        'col1
        '
        Me.col1.AutoSizeMode = System.Windows.Forms.DataGridViewAutoSizeColumnMode.None
        resources.ApplyResources(Me.col1, "col1")
        Me.col1.Name = "col1"
        Me.col1.ReadOnly = True
        '
        'col2
        '
        Me.col2.AutoSizeMode = System.Windows.Forms.DataGridViewAutoSizeColumnMode.Fill
        resources.ApplyResources(Me.col2, "col2")
        Me.col2.Name = "col2"
        Me.col2.ReadOnly = True
        Me.col2.SortMode = System.Windows.Forms.DataGridViewColumnSortMode.Programmatic
        '
        'dckMenu
        '
        Me.dckMenu.Items.AddRange(New System.Windows.Forms.ToolStripItem() {Me.FloatToolStripMenuItem, Me.DockLeftToolStripMenuItem, Me.DockRightToolStripMenuItem, Me.DockTopToolStripMenuItem, Me.DockBottomToolStripMenuItem, Me.DockLeftAutoHideToolStripMenuItem, Me.DockRightAutoHideToolStripMenuItem, Me.DockTopAutoHideToolStripMenuItem, Me.DockBottomAutoHideToolStripMenuItem, Me.DocumentToolStripMenuItem})
        Me.dckMenu.Name = "dckMenu"
        resources.ApplyResources(Me.dckMenu, "dckMenu")
        '
        'FloatToolStripMenuItem
        '
        Me.FloatToolStripMenuItem.Name = "FloatToolStripMenuItem"
        resources.ApplyResources(Me.FloatToolStripMenuItem, "FloatToolStripMenuItem")
        '
        'DockLeftToolStripMenuItem
        '
        Me.DockLeftToolStripMenuItem.Name = "DockLeftToolStripMenuItem"
        resources.ApplyResources(Me.DockLeftToolStripMenuItem, "DockLeftToolStripMenuItem")
        '
        'DockRightToolStripMenuItem
        '
        Me.DockRightToolStripMenuItem.Name = "DockRightToolStripMenuItem"
        resources.ApplyResources(Me.DockRightToolStripMenuItem, "DockRightToolStripMenuItem")
        '
        'DockTopToolStripMenuItem
        '
        Me.DockTopToolStripMenuItem.Name = "DockTopToolStripMenuItem"
        resources.ApplyResources(Me.DockTopToolStripMenuItem, "DockTopToolStripMenuItem")
        '
        'DockBottomToolStripMenuItem
        '
        Me.DockBottomToolStripMenuItem.Name = "DockBottomToolStripMenuItem"
        resources.ApplyResources(Me.DockBottomToolStripMenuItem, "DockBottomToolStripMenuItem")
        '
        'DockLeftAutoHideToolStripMenuItem
        '
        Me.DockLeftAutoHideToolStripMenuItem.Name = "DockLeftAutoHideToolStripMenuItem"
        resources.ApplyResources(Me.DockLeftAutoHideToolStripMenuItem, "DockLeftAutoHideToolStripMenuItem")
        '
        'DockRightAutoHideToolStripMenuItem
        '
        Me.DockRightAutoHideToolStripMenuItem.Name = "DockRightAutoHideToolStripMenuItem"
        resources.ApplyResources(Me.DockRightAutoHideToolStripMenuItem, "DockRightAutoHideToolStripMenuItem")
        '
        'DockTopAutoHideToolStripMenuItem
        '
        Me.DockTopAutoHideToolStripMenuItem.Name = "DockTopAutoHideToolStripMenuItem"
        resources.ApplyResources(Me.DockTopAutoHideToolStripMenuItem, "DockTopAutoHideToolStripMenuItem")
        '
        'DockBottomAutoHideToolStripMenuItem
        '
        Me.DockBottomAutoHideToolStripMenuItem.Name = "DockBottomAutoHideToolStripMenuItem"
        resources.ApplyResources(Me.DockBottomAutoHideToolStripMenuItem, "DockBottomAutoHideToolStripMenuItem")
        '
        'DocumentToolStripMenuItem
        '
        Me.DocumentToolStripMenuItem.Name = "DocumentToolStripMenuItem"
        resources.ApplyResources(Me.DocumentToolStripMenuItem, "DocumentToolStripMenuItem")
        '
        'frmObjListView
        '
        Me.AllowDrop = True
        resources.ApplyResources(Me, "$this")
        Me.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font
        Me.CloseButton = False
        Me.Controls.Add(Me.DataGridView1)
        Me.DockAreas = CType(((((WeifenLuo.WinFormsUI.Docking.DockAreas.Float Or WeifenLuo.WinFormsUI.Docking.DockAreas.DockLeft) _
            Or WeifenLuo.WinFormsUI.Docking.DockAreas.DockRight) _
            Or WeifenLuo.WinFormsUI.Docking.DockAreas.DockTop) _
            Or WeifenLuo.WinFormsUI.Docking.DockAreas.DockBottom), WeifenLuo.WinFormsUI.Docking.DockAreas)
        Me.DoubleBuffered = True
        Me.HideOnClose = True
        Me.Name = "frmObjListView"
        Me.ShowHint = WeifenLuo.WinFormsUI.Docking.DockState.DockRight
        Me.ShowInTaskbar = False
        Me.TabPageContextMenuStrip = Me.dckMenu
        Me.TabText = Me.Text
        CType(Me.DataGridView1, System.ComponentModel.ISupportInitialize).EndInit()
        Me.dckMenu.ResumeLayout(False)
        Me.ResumeLayout(False)

    End Sub
    Public WithEvents ImageList2 As System.Windows.Forms.ImageList
    Friend WithEvents DataGridView1 As System.Windows.Forms.DataGridView
    Friend WithEvents col0 As System.Windows.Forms.DataGridViewTextBoxColumn
    Friend WithEvents col1 As System.Windows.Forms.DataGridViewImageColumn
    Friend WithEvents col2 As System.Windows.Forms.DataGridViewTextBoxColumn
    Friend WithEvents dckMenu As System.Windows.Forms.ContextMenuStrip
    Friend WithEvents FloatToolStripMenuItem As System.Windows.Forms.ToolStripMenuItem
    Friend WithEvents DockLeftToolStripMenuItem As System.Windows.Forms.ToolStripMenuItem
    Friend WithEvents DockRightToolStripMenuItem As System.Windows.Forms.ToolStripMenuItem
    Friend WithEvents DockTopToolStripMenuItem As System.Windows.Forms.ToolStripMenuItem
    Friend WithEvents DockBottomToolStripMenuItem As System.Windows.Forms.ToolStripMenuItem
    Friend WithEvents DockLeftAutoHideToolStripMenuItem As System.Windows.Forms.ToolStripMenuItem
    Friend WithEvents DockRightAutoHideToolStripMenuItem As System.Windows.Forms.ToolStripMenuItem
    Friend WithEvents DockTopAutoHideToolStripMenuItem As System.Windows.Forms.ToolStripMenuItem
    Friend WithEvents DockBottomAutoHideToolStripMenuItem As System.Windows.Forms.ToolStripMenuItem
    Friend WithEvents DocumentToolStripMenuItem As System.Windows.Forms.ToolStripMenuItem
End Class
