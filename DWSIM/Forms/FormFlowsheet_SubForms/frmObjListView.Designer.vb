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
        Me.components = New System.ComponentModel.Container
        Dim resources As System.ComponentModel.ComponentResourceManager = New System.ComponentModel.ComponentResourceManager(GetType(frmObjListView))
        Me.ImageList2 = New System.Windows.Forms.ImageList(Me.components)
        Me.DataGridView1 = New System.Windows.Forms.DataGridView
        Me.col0 = New System.Windows.Forms.DataGridViewTextBoxColumn
        Me.col1 = New System.Windows.Forms.DataGridViewImageColumn
        Me.col2 = New System.Windows.Forms.DataGridViewTextBoxColumn
        CType(Me.DataGridView1, System.ComponentModel.ISupportInitialize).BeginInit()
        Me.SuspendLayout()
        '
        'ImageList2
        '
        Me.ImageList2.ImageStream = CType(resources.GetObject("ImageList2.ImageStream"), System.Windows.Forms.ImageListStreamer)
        Me.ImageList2.TransparentColor = System.Drawing.Color.White
        Me.ImageList2.Images.SetKeyName(0, "compressor copy1.png")
        Me.ImageList2.Images.SetKeyName(1, "cooler copy1.png")
        Me.ImageList2.Images.SetKeyName(2, "heater copy1.png")
        Me.ImageList2.Images.SetKeyName(3, "node_in copy1.png")
        Me.ImageList2.Images.SetKeyName(4, "node_out copy1.png")
        Me.ImageList2.Images.SetKeyName(5, "pipe copy1.png")
        Me.ImageList2.Images.SetKeyName(6, "pump copy1.png")
        Me.ImageList2.Images.SetKeyName(7, "r_conv1.png")
        Me.ImageList2.Images.SetKeyName(8, "r_cstr.png")
        Me.ImageList2.Images.SetKeyName(9, "r_equil1.png")
        Me.ImageList2.Images.SetKeyName(10, "r_gibbs1.png")
        Me.ImageList2.Images.SetKeyName(11, "r_pfr.png")
        Me.ImageList2.Images.SetKeyName(12, "reciclo_mini.png")
        Me.ImageList2.Images.SetKeyName(13, "tank copy1.png")
        Me.ImageList2.Images.SetKeyName(14, "turbina copy.png")
        Me.ImageList2.Images.SetKeyName(15, "valve copy1.png")
        Me.ImageList2.Images.SetKeyName(16, "vessel copy1.png")
        Me.ImageList2.Images.SetKeyName(17, "arrow_right.png")
        Me.ImageList2.Images.SetKeyName(18, "arrow_right2.png")
        Me.ImageList2.Images.SetKeyName(19, "ajuste_mini.png")
        Me.ImageList2.Images.SetKeyName(20, "especificacao_mini.png")
        Me.ImageList2.Images.SetKeyName(21, "reciclo_e.png")
        Me.ImageList2.Images.SetKeyName(22, "comp_separator.png")
        Me.ImageList2.Images.SetKeyName(23, "orifice2.png")
        Me.ImageList2.Images.SetKeyName(24, "greyscale_20.gif")
        Me.ImageList2.Images.SetKeyName(25, "colan2.jpg")
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
        Me.TabText = Me.Text
        CType(Me.DataGridView1, System.ComponentModel.ISupportInitialize).EndInit()
        Me.ResumeLayout(False)

    End Sub
    Public WithEvents ImageList2 As System.Windows.Forms.ImageList
    Friend WithEvents DataGridView1 As System.Windows.Forms.DataGridView
    Friend WithEvents col0 As System.Windows.Forms.DataGridViewTextBoxColumn
    Friend WithEvents col1 As System.Windows.Forms.DataGridViewImageColumn
    Friend WithEvents col2 As System.Windows.Forms.DataGridViewTextBoxColumn
End Class
