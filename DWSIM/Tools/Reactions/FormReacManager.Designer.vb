<Global.Microsoft.VisualBasic.CompilerServices.DesignerGenerated()> _
Partial Class FormReacManager
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
        Dim resources As System.ComponentModel.ComponentResourceManager = New System.ComponentModel.ComponentResourceManager(GetType(FormReacManager))
        Me.GroupBox2 = New System.Windows.Forms.GroupBox()
        Me.KryptonButton8 = New System.Windows.Forms.Button()
        Me.KryptonButton6 = New System.Windows.Forms.Button()
        Me.KryptonButton3 = New System.Windows.Forms.Button()
        Me.KryptonButton2 = New System.Windows.Forms.Button()
        Me.GridRSets = New System.Windows.Forms.DataGridView()
        Me.Column1 = New System.Windows.Forms.DataGridViewTextBoxColumn()
        Me.Column3 = New System.Windows.Forms.DataGridViewTextBoxColumn()
        Me.Column2 = New System.Windows.Forms.DataGridViewTextBoxColumn()
        Me.GroupBox4 = New System.Windows.Forms.GroupBox()
        Me.Label1 = New System.Windows.Forms.Label()
        Me.Button3 = New System.Windows.Forms.Button()
        Me.Button2 = New System.Windows.Forms.Button()
        Me.Button1 = New System.Windows.Forms.Button()
        Me.KryptonButton5 = New System.Windows.Forms.Button()
        Me.KryptonButton4 = New System.Windows.Forms.Button()
        Me.KryptonButton1 = New System.Windows.Forms.Button()
        Me.KryptonButton9 = New System.Windows.Forms.Button()
        Me.KryptonButton10 = New System.Windows.Forms.Button()
        Me.GridRxns = New System.Windows.Forms.DataGridView()
        Me.DataGridViewTextBoxColumn1 = New System.Windows.Forms.DataGridViewTextBoxColumn()
        Me.DataGridViewTextBoxColumn2 = New System.Windows.Forms.DataGridViewTextBoxColumn()
        Me.DataGridViewTextBoxColumn3 = New System.Windows.Forms.DataGridViewTextBoxColumn()
        Me.ColumnID = New System.Windows.Forms.DataGridViewTextBoxColumn()
        Me.OpenFileDialog1 = New System.Windows.Forms.OpenFileDialog()
        Me.SaveFileDialog1 = New System.Windows.Forms.SaveFileDialog()
        Me.GroupBox2.SuspendLayout()
        CType(Me.GridRSets, System.ComponentModel.ISupportInitialize).BeginInit()
        Me.GroupBox4.SuspendLayout()
        CType(Me.GridRxns, System.ComponentModel.ISupportInitialize).BeginInit()
        Me.SuspendLayout()
        '
        'GroupBox2
        '
        resources.ApplyResources(Me.GroupBox2, "GroupBox2")
        Me.GroupBox2.Controls.Add(Me.KryptonButton8)
        Me.GroupBox2.Controls.Add(Me.KryptonButton6)
        Me.GroupBox2.Controls.Add(Me.KryptonButton3)
        Me.GroupBox2.Controls.Add(Me.KryptonButton2)
        Me.GroupBox2.Controls.Add(Me.GridRSets)
        Me.GroupBox2.Name = "GroupBox2"
        Me.GroupBox2.TabStop = False
        '
        'KryptonButton8
        '
        resources.ApplyResources(Me.KryptonButton8, "KryptonButton8")
        Me.KryptonButton8.ImageKey = Global.DWSIM.My.Resources.DWSIM.NewVersionAvailable
        Me.KryptonButton8.Name = "KryptonButton8"
        '
        'KryptonButton6
        '
        resources.ApplyResources(Me.KryptonButton6, "KryptonButton6")
        Me.KryptonButton6.ImageKey = Global.DWSIM.My.Resources.DWSIM.NewVersionAvailable
        Me.KryptonButton6.Name = "KryptonButton6"
        '
        'KryptonButton3
        '
        resources.ApplyResources(Me.KryptonButton3, "KryptonButton3")
        Me.KryptonButton3.ImageKey = Global.DWSIM.My.Resources.DWSIM.NewVersionAvailable
        Me.KryptonButton3.Name = "KryptonButton3"
        '
        'KryptonButton2
        '
        resources.ApplyResources(Me.KryptonButton2, "KryptonButton2")
        Me.KryptonButton2.ImageKey = Global.DWSIM.My.Resources.DWSIM.NewVersionAvailable
        Me.KryptonButton2.Name = "KryptonButton2"
        '
        'GridRSets
        '
        resources.ApplyResources(Me.GridRSets, "GridRSets")
        Me.GridRSets.AllowUserToAddRows = False
        Me.GridRSets.AllowUserToDeleteRows = False
        Me.GridRSets.AllowUserToResizeRows = False
        Me.GridRSets.AutoSizeColumnsMode = System.Windows.Forms.DataGridViewAutoSizeColumnsMode.Fill
        Me.GridRSets.ColumnHeadersHeightSizeMode = System.Windows.Forms.DataGridViewColumnHeadersHeightSizeMode.AutoSize
        Me.GridRSets.Columns.AddRange(New System.Windows.Forms.DataGridViewColumn() {Me.Column1, Me.Column3, Me.Column2})
        Me.GridRSets.MultiSelect = False
        Me.GridRSets.Name = "GridRSets"
        Me.GridRSets.ReadOnly = True
        Me.GridRSets.RowHeadersVisible = False
        Me.GridRSets.SelectionMode = System.Windows.Forms.DataGridViewSelectionMode.FullRowSelect
        Me.GridRSets.ShowEditingIcon = False
        Me.GridRSets.ShowRowErrors = False
        '
        'Column1
        '
        resources.ApplyResources(Me.Column1, "Column1")
        Me.Column1.Name = "Column1"
        Me.Column1.ReadOnly = True
        Me.Column1.ToolTipText = Global.DWSIM.My.Resources.DWSIM.NewVersionAvailable
        '
        'Column3
        '
        resources.ApplyResources(Me.Column3, "Column3")
        Me.Column3.Name = "Column3"
        Me.Column3.ReadOnly = True
        Me.Column3.ToolTipText = Global.DWSIM.My.Resources.DWSIM.NewVersionAvailable
        '
        'Column2
        '
        resources.ApplyResources(Me.Column2, "Column2")
        Me.Column2.Name = "Column2"
        Me.Column2.ReadOnly = True
        Me.Column2.ToolTipText = Global.DWSIM.My.Resources.DWSIM.NewVersionAvailable
        '
        'GroupBox4
        '
        resources.ApplyResources(Me.GroupBox4, "GroupBox4")
        Me.GroupBox4.Controls.Add(Me.Label1)
        Me.GroupBox4.Controls.Add(Me.Button3)
        Me.GroupBox4.Controls.Add(Me.Button2)
        Me.GroupBox4.Controls.Add(Me.Button1)
        Me.GroupBox4.Controls.Add(Me.KryptonButton5)
        Me.GroupBox4.Controls.Add(Me.KryptonButton4)
        Me.GroupBox4.Controls.Add(Me.KryptonButton1)
        Me.GroupBox4.Controls.Add(Me.KryptonButton9)
        Me.GroupBox4.Controls.Add(Me.KryptonButton10)
        Me.GroupBox4.Controls.Add(Me.GridRxns)
        Me.GroupBox4.Name = "GroupBox4"
        Me.GroupBox4.TabStop = False
        '
        'Label1
        '
        resources.ApplyResources(Me.Label1, "Label1")
        Me.Label1.ImageKey = Global.DWSIM.My.Resources.DWSIM.NewVersionAvailable
        Me.Label1.Name = "Label1"
        '
        'Button3
        '
        resources.ApplyResources(Me.Button3, "Button3")
        Me.Button3.ImageKey = Global.DWSIM.My.Resources.DWSIM.NewVersionAvailable
        Me.Button3.Name = "Button3"
        Me.Button3.UseVisualStyleBackColor = True
        '
        'Button2
        '
        resources.ApplyResources(Me.Button2, "Button2")
        Me.Button2.ImageKey = Global.DWSIM.My.Resources.DWSIM.NewVersionAvailable
        Me.Button2.Name = "Button2"
        Me.Button2.UseVisualStyleBackColor = True
        '
        'Button1
        '
        resources.ApplyResources(Me.Button1, "Button1")
        Me.Button1.ImageKey = Global.DWSIM.My.Resources.DWSIM.NewVersionAvailable
        Me.Button1.Name = "Button1"
        Me.Button1.UseVisualStyleBackColor = True
        '
        'KryptonButton5
        '
        resources.ApplyResources(Me.KryptonButton5, "KryptonButton5")
        Me.KryptonButton5.ImageKey = Global.DWSIM.My.Resources.DWSIM.NewVersionAvailable
        Me.KryptonButton5.Name = "KryptonButton5"
        '
        'KryptonButton4
        '
        resources.ApplyResources(Me.KryptonButton4, "KryptonButton4")
        Me.KryptonButton4.ImageKey = Global.DWSIM.My.Resources.DWSIM.NewVersionAvailable
        Me.KryptonButton4.Name = "KryptonButton4"
        '
        'KryptonButton1
        '
        resources.ApplyResources(Me.KryptonButton1, "KryptonButton1")
        Me.KryptonButton1.ImageKey = Global.DWSIM.My.Resources.DWSIM.NewVersionAvailable
        Me.KryptonButton1.Name = "KryptonButton1"
        '
        'KryptonButton9
        '
        resources.ApplyResources(Me.KryptonButton9, "KryptonButton9")
        Me.KryptonButton9.ImageKey = Global.DWSIM.My.Resources.DWSIM.NewVersionAvailable
        Me.KryptonButton9.Name = "KryptonButton9"
        '
        'KryptonButton10
        '
        resources.ApplyResources(Me.KryptonButton10, "KryptonButton10")
        Me.KryptonButton10.ImageKey = Global.DWSIM.My.Resources.DWSIM.NewVersionAvailable
        Me.KryptonButton10.Name = "KryptonButton10"
        '
        'GridRxns
        '
        resources.ApplyResources(Me.GridRxns, "GridRxns")
        Me.GridRxns.AllowUserToAddRows = False
        Me.GridRxns.AllowUserToDeleteRows = False
        Me.GridRxns.AllowUserToResizeRows = False
        Me.GridRxns.AutoSizeColumnsMode = System.Windows.Forms.DataGridViewAutoSizeColumnsMode.Fill
        Me.GridRxns.ColumnHeadersHeightSizeMode = System.Windows.Forms.DataGridViewColumnHeadersHeightSizeMode.AutoSize
        Me.GridRxns.Columns.AddRange(New System.Windows.Forms.DataGridViewColumn() {Me.DataGridViewTextBoxColumn1, Me.DataGridViewTextBoxColumn2, Me.DataGridViewTextBoxColumn3, Me.ColumnID})
        Me.GridRxns.MultiSelect = False
        Me.GridRxns.Name = "GridRxns"
        Me.GridRxns.ReadOnly = True
        Me.GridRxns.RowHeadersVisible = False
        Me.GridRxns.SelectionMode = System.Windows.Forms.DataGridViewSelectionMode.FullRowSelect
        Me.GridRxns.ShowEditingIcon = False
        Me.GridRxns.ShowRowErrors = False
        '
        'DataGridViewTextBoxColumn1
        '
        resources.ApplyResources(Me.DataGridViewTextBoxColumn1, "DataGridViewTextBoxColumn1")
        Me.DataGridViewTextBoxColumn1.Name = "DataGridViewTextBoxColumn1"
        Me.DataGridViewTextBoxColumn1.ReadOnly = True
        Me.DataGridViewTextBoxColumn1.ToolTipText = Global.DWSIM.My.Resources.DWSIM.NewVersionAvailable
        '
        'DataGridViewTextBoxColumn2
        '
        resources.ApplyResources(Me.DataGridViewTextBoxColumn2, "DataGridViewTextBoxColumn2")
        Me.DataGridViewTextBoxColumn2.Name = "DataGridViewTextBoxColumn2"
        Me.DataGridViewTextBoxColumn2.ReadOnly = True
        Me.DataGridViewTextBoxColumn2.ToolTipText = Global.DWSIM.My.Resources.DWSIM.NewVersionAvailable
        '
        'DataGridViewTextBoxColumn3
        '
        resources.ApplyResources(Me.DataGridViewTextBoxColumn3, "DataGridViewTextBoxColumn3")
        Me.DataGridViewTextBoxColumn3.Name = "DataGridViewTextBoxColumn3"
        Me.DataGridViewTextBoxColumn3.ReadOnly = True
        Me.DataGridViewTextBoxColumn3.ToolTipText = Global.DWSIM.My.Resources.DWSIM.NewVersionAvailable
        '
        'ColumnID
        '
        resources.ApplyResources(Me.ColumnID, "ColumnID")
        Me.ColumnID.Name = "ColumnID"
        Me.ColumnID.ReadOnly = True
        Me.ColumnID.ToolTipText = Global.DWSIM.My.Resources.DWSIM.NewVersionAvailable
        '
        'OpenFileDialog1
        '
        Me.OpenFileDialog1.DefaultExt = "xml"
        resources.ApplyResources(Me.OpenFileDialog1, "OpenFileDialog1")
        Me.OpenFileDialog1.SupportMultiDottedExtensions = True
        '
        'SaveFileDialog1
        '
        Me.SaveFileDialog1.DefaultExt = "xml"
        resources.ApplyResources(Me.SaveFileDialog1, "SaveFileDialog1")
        Me.SaveFileDialog1.FilterIndex = 2
        Me.SaveFileDialog1.RestoreDirectory = True
        Me.SaveFileDialog1.SupportMultiDottedExtensions = True
        '
        'FormReacManager
        '
        resources.ApplyResources(Me, "$this")
        Me.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font
        Me.Controls.Add(Me.GroupBox4)
        Me.Controls.Add(Me.GroupBox2)
        Me.DoubleBuffered = True
        Me.FormBorderStyle = System.Windows.Forms.FormBorderStyle.FixedToolWindow
        Me.Name = "FormReacManager"
        Me.ShowIcon = False
        Me.ShowInTaskbar = False
        Me.GroupBox2.ResumeLayout(False)
        CType(Me.GridRSets, System.ComponentModel.ISupportInitialize).EndInit()
        Me.GroupBox4.ResumeLayout(False)
        Me.GroupBox4.PerformLayout()
        CType(Me.GridRxns, System.ComponentModel.ISupportInitialize).EndInit()
        Me.ResumeLayout(False)

    End Sub
    Public WithEvents GroupBox2 As System.Windows.Forms.GroupBox
    Public WithEvents GridRSets As System.Windows.Forms.DataGridView
    Public WithEvents KryptonButton8 As System.Windows.Forms.Button
    Public WithEvents KryptonButton6 As System.Windows.Forms.Button
    Public WithEvents KryptonButton3 As System.Windows.Forms.Button
    Public WithEvents KryptonButton2 As System.Windows.Forms.Button
    Public WithEvents GroupBox4 As System.Windows.Forms.GroupBox
    Public WithEvents KryptonButton1 As System.Windows.Forms.Button
    Public WithEvents KryptonButton9 As System.Windows.Forms.Button
    Public WithEvents KryptonButton10 As System.Windows.Forms.Button
    Public WithEvents GridRxns As System.Windows.Forms.DataGridView
    Public WithEvents KryptonButton5 As System.Windows.Forms.Button
    Public WithEvents KryptonButton4 As System.Windows.Forms.Button
    Public WithEvents OpenFileDialog1 As System.Windows.Forms.OpenFileDialog
    Public WithEvents SaveFileDialog1 As System.Windows.Forms.SaveFileDialog
    Public WithEvents Column1 As System.Windows.Forms.DataGridViewTextBoxColumn
    Public WithEvents Column3 As System.Windows.Forms.DataGridViewTextBoxColumn
    Public WithEvents Column2 As System.Windows.Forms.DataGridViewTextBoxColumn
    Public WithEvents DataGridViewTextBoxColumn1 As System.Windows.Forms.DataGridViewTextBoxColumn
    Public WithEvents DataGridViewTextBoxColumn2 As System.Windows.Forms.DataGridViewTextBoxColumn
    Public WithEvents DataGridViewTextBoxColumn3 As System.Windows.Forms.DataGridViewTextBoxColumn
    Public WithEvents ColumnID As System.Windows.Forms.DataGridViewTextBoxColumn
    Public WithEvents Button1 As System.Windows.Forms.Button
    Public WithEvents Label1 As System.Windows.Forms.Label
    Public WithEvents Button3 As System.Windows.Forms.Button
    Public WithEvents Button2 As System.Windows.Forms.Button
End Class
