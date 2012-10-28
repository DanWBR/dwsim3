<Global.Microsoft.VisualBasic.CompilerServices.DesignerGenerated()> _
Partial Class SpreadsheetForm
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
        Dim resources As System.ComponentModel.ComponentResourceManager = New System.ComponentModel.ComponentResourceManager(GetType(SpreadsheetForm))
        Dim DataGridViewCellStyle1 As System.Windows.Forms.DataGridViewCellStyle = New System.Windows.Forms.DataGridViewCellStyle
        Dim DataGridViewCellStyle2 As System.Windows.Forms.DataGridViewCellStyle = New System.Windows.Forms.DataGridViewCellStyle
        Me.TableLayoutPanel1 = New System.Windows.Forms.TableLayoutPanel
        Me.DataGridView1 = New System.Windows.Forms.DataGridView
        Me.ContextMenuStrip1 = New System.Windows.Forms.ContextMenuStrip(Me.components)
        Me.CelulaToolStripMenuItem = New System.Windows.Forms.ToolStripMenuItem
        Me.ToolStripSeparator2 = New System.Windows.Forms.ToolStripSeparator
        Me.AvaliarFórmulaToolStripMenuItem = New System.Windows.Forms.ToolStripMenuItem
        Me.ImportarDadosToolStripMenuItem = New System.Windows.Forms.ToolStripMenuItem
        Me.ExportarDadosToolStripMenuItem = New System.Windows.Forms.ToolStripMenuItem
        Me.ToolStripSeparator1 = New System.Windows.Forms.ToolStripSeparator
        Me.LimparToolStripMenuItem = New System.Windows.Forms.ToolStripMenuItem
        Me.CopiarToolStripMenuItem = New System.Windows.Forms.ToolStripMenuItem
        Me.ColarToolStripMenuItem = New System.Windows.Forms.ToolStripMenuItem
        Me.TableLayoutPanel2 = New System.Windows.Forms.TableLayoutPanel
        Me.tbCell = New System.Windows.Forms.TextBox
        Me.Label1 = New System.Windows.Forms.Label
        Me.tbValue = New System.Windows.Forms.TextBox
        Me.Button3 = New System.Windows.Forms.Button
        Me.Button1 = New System.Windows.Forms.Button
        Me.Button2 = New System.Windows.Forms.Button
        Me.TableLayoutPanel3 = New System.Windows.Forms.TableLayoutPanel
        Me.Label2 = New System.Windows.Forms.Label
        Me.chkWriteMode = New System.Windows.Forms.CheckBox
        Me.chkUpdate = New System.Windows.Forms.CheckBox
        Me.tbTolerance = New System.Windows.Forms.TextBox
        Me.A = New System.Windows.Forms.DataGridViewTextBoxColumn
        Me.B = New System.Windows.Forms.DataGridViewTextBoxColumn
        Me.C = New System.Windows.Forms.DataGridViewTextBoxColumn
        Me.D = New System.Windows.Forms.DataGridViewTextBoxColumn
        Me.E = New System.Windows.Forms.DataGridViewTextBoxColumn
        Me.F = New System.Windows.Forms.DataGridViewTextBoxColumn
        Me.G = New System.Windows.Forms.DataGridViewTextBoxColumn
        Me.H = New System.Windows.Forms.DataGridViewTextBoxColumn
        Me.I = New System.Windows.Forms.DataGridViewTextBoxColumn
        Me.J = New System.Windows.Forms.DataGridViewTextBoxColumn
        Me.K = New System.Windows.Forms.DataGridViewTextBoxColumn
        Me.L = New System.Windows.Forms.DataGridViewTextBoxColumn
        Me.M = New System.Windows.Forms.DataGridViewTextBoxColumn
        Me.N = New System.Windows.Forms.DataGridViewTextBoxColumn
        Me.O = New System.Windows.Forms.DataGridViewTextBoxColumn
        Me.P = New System.Windows.Forms.DataGridViewTextBoxColumn
        Me.Q = New System.Windows.Forms.DataGridViewTextBoxColumn
        Me.R = New System.Windows.Forms.DataGridViewTextBoxColumn
        Me.S = New System.Windows.Forms.DataGridViewTextBoxColumn
        Me.T = New System.Windows.Forms.DataGridViewTextBoxColumn
        Me.U = New System.Windows.Forms.DataGridViewTextBoxColumn
        Me.V = New System.Windows.Forms.DataGridViewTextBoxColumn
        Me.W = New System.Windows.Forms.DataGridViewTextBoxColumn
        Me.X = New System.Windows.Forms.DataGridViewTextBoxColumn
        Me.Y = New System.Windows.Forms.DataGridViewTextBoxColumn
        Me.Z = New System.Windows.Forms.DataGridViewTextBoxColumn
        Me.TableLayoutPanel1.SuspendLayout()
        CType(Me.DataGridView1, System.ComponentModel.ISupportInitialize).BeginInit()
        Me.ContextMenuStrip1.SuspendLayout()
        Me.TableLayoutPanel2.SuspendLayout()
        Me.TableLayoutPanel3.SuspendLayout()
        Me.SuspendLayout()
        '
        'TableLayoutPanel1
        '
        Me.TableLayoutPanel1.AccessibleDescription = Nothing
        Me.TableLayoutPanel1.AccessibleName = Nothing
        resources.ApplyResources(Me.TableLayoutPanel1, "TableLayoutPanel1")
        Me.TableLayoutPanel1.BackgroundImage = Nothing
        Me.TableLayoutPanel1.Controls.Add(Me.DataGridView1, 0, 1)
        Me.TableLayoutPanel1.Controls.Add(Me.TableLayoutPanel2, 0, 0)
        Me.TableLayoutPanel1.Controls.Add(Me.TableLayoutPanel3, 0, 2)
        Me.TableLayoutPanel1.Font = Nothing
        Me.TableLayoutPanel1.Name = "TableLayoutPanel1"
        '
        'DataGridView1
        '
        Me.DataGridView1.AccessibleDescription = Nothing
        Me.DataGridView1.AccessibleName = Nothing
        Me.DataGridView1.AllowUserToAddRows = False
        Me.DataGridView1.AllowUserToDeleteRows = False
        DataGridViewCellStyle1.BackColor = System.Drawing.Color.WhiteSmoke
        Me.DataGridView1.AlternatingRowsDefaultCellStyle = DataGridViewCellStyle1
        resources.ApplyResources(Me.DataGridView1, "DataGridView1")
        Me.DataGridView1.BackgroundImage = Nothing
        Me.DataGridView1.BorderStyle = System.Windows.Forms.BorderStyle.Fixed3D
        DataGridViewCellStyle2.Alignment = System.Windows.Forms.DataGridViewContentAlignment.MiddleCenter
        DataGridViewCellStyle2.BackColor = System.Drawing.SystemColors.Control
        DataGridViewCellStyle2.Font = New System.Drawing.Font("Microsoft Sans Serif", 8.25!, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, CType(0, Byte))
        DataGridViewCellStyle2.ForeColor = System.Drawing.SystemColors.WindowText
        DataGridViewCellStyle2.SelectionBackColor = System.Drawing.SystemColors.Highlight
        DataGridViewCellStyle2.SelectionForeColor = System.Drawing.SystemColors.HighlightText
        DataGridViewCellStyle2.WrapMode = System.Windows.Forms.DataGridViewTriState.[True]
        Me.DataGridView1.ColumnHeadersDefaultCellStyle = DataGridViewCellStyle2
        Me.DataGridView1.ColumnHeadersHeightSizeMode = System.Windows.Forms.DataGridViewColumnHeadersHeightSizeMode.AutoSize
        Me.DataGridView1.Columns.AddRange(New System.Windows.Forms.DataGridViewColumn() {Me.A, Me.B, Me.C, Me.D, Me.E, Me.F, Me.G, Me.H, Me.I, Me.J, Me.K, Me.L, Me.M, Me.N, Me.O, Me.P, Me.Q, Me.R, Me.S, Me.T, Me.U, Me.V, Me.W, Me.X, Me.Y, Me.Z})
        Me.DataGridView1.ContextMenuStrip = Me.ContextMenuStrip1
        Me.DataGridView1.Font = Nothing
        Me.DataGridView1.Name = "DataGridView1"
        Me.DataGridView1.RowHeadersWidthSizeMode = System.Windows.Forms.DataGridViewRowHeadersWidthSizeMode.AutoSizeToAllHeaders
        Me.DataGridView1.RowTemplate.Height = 20
        Me.DataGridView1.SelectionMode = System.Windows.Forms.DataGridViewSelectionMode.CellSelect
        Me.DataGridView1.ShowCellErrors = False
        Me.DataGridView1.ShowEditingIcon = False
        Me.DataGridView1.ShowRowErrors = False
        '
        'ContextMenuStrip1
        '
        Me.ContextMenuStrip1.AccessibleDescription = Nothing
        Me.ContextMenuStrip1.AccessibleName = Nothing
        resources.ApplyResources(Me.ContextMenuStrip1, "ContextMenuStrip1")
        Me.ContextMenuStrip1.BackgroundImage = Nothing
        Me.ContextMenuStrip1.Font = Nothing
        Me.ContextMenuStrip1.Items.AddRange(New System.Windows.Forms.ToolStripItem() {Me.CelulaToolStripMenuItem, Me.ToolStripSeparator2, Me.AvaliarFórmulaToolStripMenuItem, Me.ImportarDadosToolStripMenuItem, Me.ExportarDadosToolStripMenuItem, Me.ToolStripSeparator1, Me.LimparToolStripMenuItem, Me.CopiarToolStripMenuItem, Me.ColarToolStripMenuItem})
        Me.ContextMenuStrip1.Name = "ContextMenuStrip1"
        '
        'CelulaToolStripMenuItem
        '
        Me.CelulaToolStripMenuItem.AccessibleDescription = Nothing
        Me.CelulaToolStripMenuItem.AccessibleName = Nothing
        resources.ApplyResources(Me.CelulaToolStripMenuItem, "CelulaToolStripMenuItem")
        Me.CelulaToolStripMenuItem.BackgroundImage = Nothing
        Me.CelulaToolStripMenuItem.Name = "CelulaToolStripMenuItem"
        Me.CelulaToolStripMenuItem.ShortcutKeyDisplayString = Nothing
        '
        'ToolStripSeparator2
        '
        Me.ToolStripSeparator2.AccessibleDescription = Nothing
        Me.ToolStripSeparator2.AccessibleName = Nothing
        resources.ApplyResources(Me.ToolStripSeparator2, "ToolStripSeparator2")
        Me.ToolStripSeparator2.Name = "ToolStripSeparator2"
        '
        'AvaliarFórmulaToolStripMenuItem
        '
        Me.AvaliarFórmulaToolStripMenuItem.AccessibleDescription = Nothing
        Me.AvaliarFórmulaToolStripMenuItem.AccessibleName = Nothing
        resources.ApplyResources(Me.AvaliarFórmulaToolStripMenuItem, "AvaliarFórmulaToolStripMenuItem")
        Me.AvaliarFórmulaToolStripMenuItem.BackgroundImage = Nothing
        Me.AvaliarFórmulaToolStripMenuItem.Image = Global.DWSIM.My.Resources.Resources.calculator
        Me.AvaliarFórmulaToolStripMenuItem.Name = "AvaliarFórmulaToolStripMenuItem"
        Me.AvaliarFórmulaToolStripMenuItem.ShortcutKeyDisplayString = Nothing
        '
        'ImportarDadosToolStripMenuItem
        '
        Me.ImportarDadosToolStripMenuItem.AccessibleDescription = Nothing
        Me.ImportarDadosToolStripMenuItem.AccessibleName = Nothing
        resources.ApplyResources(Me.ImportarDadosToolStripMenuItem, "ImportarDadosToolStripMenuItem")
        Me.ImportarDadosToolStripMenuItem.BackgroundImage = Nothing
        Me.ImportarDadosToolStripMenuItem.Image = Global.DWSIM.My.Resources.Resources.arrow_in1
        Me.ImportarDadosToolStripMenuItem.Name = "ImportarDadosToolStripMenuItem"
        Me.ImportarDadosToolStripMenuItem.ShortcutKeyDisplayString = Nothing
        '
        'ExportarDadosToolStripMenuItem
        '
        Me.ExportarDadosToolStripMenuItem.AccessibleDescription = Nothing
        Me.ExportarDadosToolStripMenuItem.AccessibleName = Nothing
        resources.ApplyResources(Me.ExportarDadosToolStripMenuItem, "ExportarDadosToolStripMenuItem")
        Me.ExportarDadosToolStripMenuItem.BackgroundImage = Nothing
        Me.ExportarDadosToolStripMenuItem.Image = Global.DWSIM.My.Resources.Resources.arrow_out
        Me.ExportarDadosToolStripMenuItem.Name = "ExportarDadosToolStripMenuItem"
        Me.ExportarDadosToolStripMenuItem.ShortcutKeyDisplayString = Nothing
        '
        'ToolStripSeparator1
        '
        Me.ToolStripSeparator1.AccessibleDescription = Nothing
        Me.ToolStripSeparator1.AccessibleName = Nothing
        resources.ApplyResources(Me.ToolStripSeparator1, "ToolStripSeparator1")
        Me.ToolStripSeparator1.Name = "ToolStripSeparator1"
        '
        'LimparToolStripMenuItem
        '
        Me.LimparToolStripMenuItem.AccessibleDescription = Nothing
        Me.LimparToolStripMenuItem.AccessibleName = Nothing
        resources.ApplyResources(Me.LimparToolStripMenuItem, "LimparToolStripMenuItem")
        Me.LimparToolStripMenuItem.BackgroundImage = Nothing
        Me.LimparToolStripMenuItem.Image = Global.DWSIM.My.Resources.Resources.cross
        Me.LimparToolStripMenuItem.Name = "LimparToolStripMenuItem"
        Me.LimparToolStripMenuItem.ShortcutKeyDisplayString = Nothing
        '
        'CopiarToolStripMenuItem
        '
        Me.CopiarToolStripMenuItem.AccessibleDescription = Nothing
        Me.CopiarToolStripMenuItem.AccessibleName = Nothing
        resources.ApplyResources(Me.CopiarToolStripMenuItem, "CopiarToolStripMenuItem")
        Me.CopiarToolStripMenuItem.BackgroundImage = Nothing
        Me.CopiarToolStripMenuItem.Image = Global.DWSIM.My.Resources.Resources.page_copy
        Me.CopiarToolStripMenuItem.Name = "CopiarToolStripMenuItem"
        Me.CopiarToolStripMenuItem.ShortcutKeyDisplayString = Nothing
        '
        'ColarToolStripMenuItem
        '
        Me.ColarToolStripMenuItem.AccessibleDescription = Nothing
        Me.ColarToolStripMenuItem.AccessibleName = Nothing
        resources.ApplyResources(Me.ColarToolStripMenuItem, "ColarToolStripMenuItem")
        Me.ColarToolStripMenuItem.BackgroundImage = Nothing
        Me.ColarToolStripMenuItem.Image = Global.DWSIM.My.Resources.Resources.paste_plain
        Me.ColarToolStripMenuItem.Name = "ColarToolStripMenuItem"
        Me.ColarToolStripMenuItem.ShortcutKeyDisplayString = Nothing
        '
        'TableLayoutPanel2
        '
        Me.TableLayoutPanel2.AccessibleDescription = Nothing
        Me.TableLayoutPanel2.AccessibleName = Nothing
        resources.ApplyResources(Me.TableLayoutPanel2, "TableLayoutPanel2")
        Me.TableLayoutPanel2.BackgroundImage = Nothing
        Me.TableLayoutPanel2.Controls.Add(Me.tbCell, 0, 0)
        Me.TableLayoutPanel2.Controls.Add(Me.Label1, 2, 0)
        Me.TableLayoutPanel2.Controls.Add(Me.tbValue, 3, 0)
        Me.TableLayoutPanel2.Controls.Add(Me.Button3, 4, 0)
        Me.TableLayoutPanel2.Controls.Add(Me.Button1, 5, 0)
        Me.TableLayoutPanel2.Controls.Add(Me.Button2, 1, 0)
        Me.TableLayoutPanel2.Font = Nothing
        Me.TableLayoutPanel2.Name = "TableLayoutPanel2"
        '
        'tbCell
        '
        Me.tbCell.AccessibleDescription = Nothing
        Me.tbCell.AccessibleName = Nothing
        resources.ApplyResources(Me.tbCell, "tbCell")
        Me.tbCell.BackgroundImage = Nothing
        Me.tbCell.Font = Nothing
        Me.tbCell.Name = "tbCell"
        Me.tbCell.ReadOnly = True
        '
        'Label1
        '
        Me.Label1.AccessibleDescription = Nothing
        Me.Label1.AccessibleName = Nothing
        resources.ApplyResources(Me.Label1, "Label1")
        Me.Label1.Font = Nothing
        Me.Label1.Name = "Label1"
        '
        'tbValue
        '
        Me.tbValue.AccessibleDescription = Nothing
        Me.tbValue.AccessibleName = Nothing
        resources.ApplyResources(Me.tbValue, "tbValue")
        Me.tbValue.BackgroundImage = Nothing
        Me.tbValue.Font = Nothing
        Me.tbValue.Name = "tbValue"
        '
        'Button3
        '
        Me.Button3.AccessibleDescription = Nothing
        Me.Button3.AccessibleName = Nothing
        resources.ApplyResources(Me.Button3, "Button3")
        Me.Button3.BackgroundImage = Nothing
        Me.Button3.Font = Nothing
        Me.Button3.Image = Global.DWSIM.My.Resources.Resources.tick
        Me.Button3.Name = "Button3"
        Me.Button3.UseVisualStyleBackColor = True
        '
        'Button1
        '
        Me.Button1.AccessibleDescription = Nothing
        Me.Button1.AccessibleName = Nothing
        resources.ApplyResources(Me.Button1, "Button1")
        Me.Button1.BackgroundImage = Nothing
        Me.Button1.Font = Nothing
        Me.Button1.Image = Global.DWSIM.My.Resources.Resources.cross
        Me.Button1.Name = "Button1"
        Me.Button1.UseVisualStyleBackColor = True
        '
        'Button2
        '
        Me.Button2.AccessibleDescription = Nothing
        Me.Button2.AccessibleName = Nothing
        resources.ApplyResources(Me.Button2, "Button2")
        Me.Button2.BackgroundImage = Nothing
        Me.Button2.Font = Nothing
        Me.Button2.Image = Global.DWSIM.My.Resources.Resources.arrow_refresh
        Me.Button2.Name = "Button2"
        Me.Button2.UseVisualStyleBackColor = True
        '
        'TableLayoutPanel3
        '
        Me.TableLayoutPanel3.AccessibleDescription = Nothing
        Me.TableLayoutPanel3.AccessibleName = Nothing
        resources.ApplyResources(Me.TableLayoutPanel3, "TableLayoutPanel3")
        Me.TableLayoutPanel3.BackgroundImage = Nothing
        Me.TableLayoutPanel3.Controls.Add(Me.Label2, 0, 0)
        Me.TableLayoutPanel3.Controls.Add(Me.chkWriteMode, 0, 0)
        Me.TableLayoutPanel3.Controls.Add(Me.chkUpdate, 0, 0)
        Me.TableLayoutPanel3.Controls.Add(Me.tbTolerance, 3, 0)
        Me.TableLayoutPanel3.Font = Nothing
        Me.TableLayoutPanel3.Name = "TableLayoutPanel3"
        '
        'Label2
        '
        Me.Label2.AccessibleDescription = Nothing
        Me.Label2.AccessibleName = Nothing
        resources.ApplyResources(Me.Label2, "Label2")
        Me.Label2.Font = Nothing
        Me.Label2.Name = "Label2"
        '
        'chkWriteMode
        '
        Me.chkWriteMode.AccessibleDescription = Nothing
        Me.chkWriteMode.AccessibleName = Nothing
        resources.ApplyResources(Me.chkWriteMode, "chkWriteMode")
        Me.chkWriteMode.BackgroundImage = Nothing
        Me.chkWriteMode.Font = Nothing
        Me.chkWriteMode.Name = "chkWriteMode"
        Me.chkWriteMode.UseVisualStyleBackColor = True
        '
        'chkUpdate
        '
        Me.chkUpdate.AccessibleDescription = Nothing
        Me.chkUpdate.AccessibleName = Nothing
        resources.ApplyResources(Me.chkUpdate, "chkUpdate")
        Me.chkUpdate.BackgroundImage = Nothing
        Me.chkUpdate.Checked = True
        Me.chkUpdate.CheckState = System.Windows.Forms.CheckState.Checked
        Me.chkUpdate.Font = Nothing
        Me.chkUpdate.Name = "chkUpdate"
        Me.chkUpdate.UseVisualStyleBackColor = True
        '
        'tbTolerance
        '
        Me.tbTolerance.AccessibleDescription = Nothing
        Me.tbTolerance.AccessibleName = Nothing
        resources.ApplyResources(Me.tbTolerance, "tbTolerance")
        Me.tbTolerance.BackgroundImage = Nothing
        Me.tbTolerance.Font = Nothing
        Me.tbTolerance.Name = "tbTolerance"
        '
        'A
        '
        resources.ApplyResources(Me.A, "A")
        Me.A.Name = "A"
        Me.A.SortMode = System.Windows.Forms.DataGridViewColumnSortMode.NotSortable
        '
        'B
        '
        resources.ApplyResources(Me.B, "B")
        Me.B.Name = "B"
        Me.B.SortMode = System.Windows.Forms.DataGridViewColumnSortMode.NotSortable
        '
        'C
        '
        resources.ApplyResources(Me.C, "C")
        Me.C.Name = "C"
        Me.C.SortMode = System.Windows.Forms.DataGridViewColumnSortMode.NotSortable
        '
        'D
        '
        resources.ApplyResources(Me.D, "D")
        Me.D.Name = "D"
        Me.D.SortMode = System.Windows.Forms.DataGridViewColumnSortMode.NotSortable
        '
        'E
        '
        resources.ApplyResources(Me.E, "E")
        Me.E.Name = "E"
        Me.E.SortMode = System.Windows.Forms.DataGridViewColumnSortMode.NotSortable
        '
        'F
        '
        resources.ApplyResources(Me.F, "F")
        Me.F.Name = "F"
        Me.F.SortMode = System.Windows.Forms.DataGridViewColumnSortMode.NotSortable
        '
        'G
        '
        resources.ApplyResources(Me.G, "G")
        Me.G.Name = "G"
        Me.G.SortMode = System.Windows.Forms.DataGridViewColumnSortMode.NotSortable
        '
        'H
        '
        resources.ApplyResources(Me.H, "H")
        Me.H.Name = "H"
        Me.H.SortMode = System.Windows.Forms.DataGridViewColumnSortMode.NotSortable
        '
        'I
        '
        resources.ApplyResources(Me.I, "I")
        Me.I.Name = "I"
        Me.I.SortMode = System.Windows.Forms.DataGridViewColumnSortMode.NotSortable
        '
        'J
        '
        resources.ApplyResources(Me.J, "J")
        Me.J.Name = "J"
        Me.J.SortMode = System.Windows.Forms.DataGridViewColumnSortMode.NotSortable
        '
        'K
        '
        resources.ApplyResources(Me.K, "K")
        Me.K.Name = "K"
        Me.K.SortMode = System.Windows.Forms.DataGridViewColumnSortMode.NotSortable
        '
        'L
        '
        resources.ApplyResources(Me.L, "L")
        Me.L.Name = "L"
        Me.L.SortMode = System.Windows.Forms.DataGridViewColumnSortMode.NotSortable
        '
        'M
        '
        resources.ApplyResources(Me.M, "M")
        Me.M.Name = "M"
        Me.M.SortMode = System.Windows.Forms.DataGridViewColumnSortMode.NotSortable
        '
        'N
        '
        resources.ApplyResources(Me.N, "N")
        Me.N.Name = "N"
        Me.N.SortMode = System.Windows.Forms.DataGridViewColumnSortMode.NotSortable
        '
        'O
        '
        resources.ApplyResources(Me.O, "O")
        Me.O.Name = "O"
        Me.O.SortMode = System.Windows.Forms.DataGridViewColumnSortMode.NotSortable
        '
        'P
        '
        resources.ApplyResources(Me.P, "P")
        Me.P.Name = "P"
        Me.P.SortMode = System.Windows.Forms.DataGridViewColumnSortMode.NotSortable
        '
        'Q
        '
        resources.ApplyResources(Me.Q, "Q")
        Me.Q.Name = "Q"
        Me.Q.SortMode = System.Windows.Forms.DataGridViewColumnSortMode.NotSortable
        '
        'R
        '
        resources.ApplyResources(Me.R, "R")
        Me.R.Name = "R"
        Me.R.SortMode = System.Windows.Forms.DataGridViewColumnSortMode.NotSortable
        '
        'S
        '
        resources.ApplyResources(Me.S, "S")
        Me.S.Name = "S"
        Me.S.SortMode = System.Windows.Forms.DataGridViewColumnSortMode.NotSortable
        '
        'T
        '
        resources.ApplyResources(Me.T, "T")
        Me.T.Name = "T"
        Me.T.SortMode = System.Windows.Forms.DataGridViewColumnSortMode.NotSortable
        '
        'U
        '
        resources.ApplyResources(Me.U, "U")
        Me.U.Name = "U"
        Me.U.SortMode = System.Windows.Forms.DataGridViewColumnSortMode.NotSortable
        '
        'V
        '
        resources.ApplyResources(Me.V, "V")
        Me.V.Name = "V"
        Me.V.SortMode = System.Windows.Forms.DataGridViewColumnSortMode.NotSortable
        '
        'W
        '
        resources.ApplyResources(Me.W, "W")
        Me.W.Name = "W"
        Me.W.SortMode = System.Windows.Forms.DataGridViewColumnSortMode.NotSortable
        '
        'X
        '
        resources.ApplyResources(Me.X, "X")
        Me.X.Name = "X"
        Me.X.SortMode = System.Windows.Forms.DataGridViewColumnSortMode.NotSortable
        '
        'Y
        '
        resources.ApplyResources(Me.Y, "Y")
        Me.Y.Name = "Y"
        Me.Y.SortMode = System.Windows.Forms.DataGridViewColumnSortMode.NotSortable
        '
        'Z
        '
        resources.ApplyResources(Me.Z, "Z")
        Me.Z.Name = "Z"
        Me.Z.SortMode = System.Windows.Forms.DataGridViewColumnSortMode.NotSortable
        '
        'SpreadsheetForm
        '
        Me.AccessibleDescription = Nothing
        Me.AccessibleName = Nothing
        resources.ApplyResources(Me, "$this")
        Me.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font
        Me.BackgroundImage = Nothing
        Me.CloseButton = False
        Me.Controls.Add(Me.TableLayoutPanel1)
        Me.DoubleBuffered = True
        Me.Font = Nothing
        Me.HideOnClose = True
        Me.Name = "SpreadsheetForm"
        Me.ShowHint = WeifenLuo.WinFormsUI.Docking.DockState.Document
        Me.ToolTipText = Nothing
        Me.TableLayoutPanel1.ResumeLayout(False)
        CType(Me.DataGridView1, System.ComponentModel.ISupportInitialize).EndInit()
        Me.ContextMenuStrip1.ResumeLayout(False)
        Me.TableLayoutPanel2.ResumeLayout(False)
        Me.TableLayoutPanel2.PerformLayout()
        Me.TableLayoutPanel3.ResumeLayout(False)
        Me.TableLayoutPanel3.PerformLayout()
        Me.ResumeLayout(False)

    End Sub
    Public WithEvents TableLayoutPanel1 As System.Windows.Forms.TableLayoutPanel
    Public WithEvents DataGridView1 As System.Windows.Forms.DataGridView
    Public WithEvents TableLayoutPanel2 As System.Windows.Forms.TableLayoutPanel
    Public WithEvents tbCell As System.Windows.Forms.TextBox
    Public WithEvents Button1 As System.Windows.Forms.Button
    Public WithEvents Button2 As System.Windows.Forms.Button
    Public WithEvents Label1 As System.Windows.Forms.Label
    Public WithEvents tbValue As System.Windows.Forms.TextBox
    Public WithEvents Button3 As System.Windows.Forms.Button
    Public WithEvents ContextMenuStrip1 As System.Windows.Forms.ContextMenuStrip
    Public WithEvents ImportarDadosToolStripMenuItem As System.Windows.Forms.ToolStripMenuItem
    Public WithEvents LimparToolStripMenuItem As System.Windows.Forms.ToolStripMenuItem
    Public WithEvents CopiarToolStripMenuItem As System.Windows.Forms.ToolStripMenuItem
    Public WithEvents ColarToolStripMenuItem As System.Windows.Forms.ToolStripMenuItem
    Public WithEvents CelulaToolStripMenuItem As System.Windows.Forms.ToolStripMenuItem
    Public WithEvents ToolStripSeparator2 As System.Windows.Forms.ToolStripSeparator
    Public WithEvents ToolStripSeparator1 As System.Windows.Forms.ToolStripSeparator
    Public WithEvents AvaliarFórmulaToolStripMenuItem As System.Windows.Forms.ToolStripMenuItem
    Public WithEvents ExportarDadosToolStripMenuItem As System.Windows.Forms.ToolStripMenuItem
    Public WithEvents TableLayoutPanel3 As System.Windows.Forms.TableLayoutPanel
    Public WithEvents chkUpdate As System.Windows.Forms.CheckBox
    Public WithEvents tbTolerance As System.Windows.Forms.TextBox
    Public WithEvents Label2 As System.Windows.Forms.Label
    Public WithEvents chkWriteMode As System.Windows.Forms.CheckBox
    Public WithEvents A As System.Windows.Forms.DataGridViewTextBoxColumn
    Public WithEvents B As System.Windows.Forms.DataGridViewTextBoxColumn
    Public WithEvents C As System.Windows.Forms.DataGridViewTextBoxColumn
    Public WithEvents D As System.Windows.Forms.DataGridViewTextBoxColumn
    Public WithEvents E As System.Windows.Forms.DataGridViewTextBoxColumn
    Public WithEvents F As System.Windows.Forms.DataGridViewTextBoxColumn
    Public WithEvents G As System.Windows.Forms.DataGridViewTextBoxColumn
    Public WithEvents H As System.Windows.Forms.DataGridViewTextBoxColumn
    Public WithEvents I As System.Windows.Forms.DataGridViewTextBoxColumn
    Public WithEvents J As System.Windows.Forms.DataGridViewTextBoxColumn
    Public WithEvents K As System.Windows.Forms.DataGridViewTextBoxColumn
    Public WithEvents L As System.Windows.Forms.DataGridViewTextBoxColumn
    Public WithEvents M As System.Windows.Forms.DataGridViewTextBoxColumn
    Public WithEvents N As System.Windows.Forms.DataGridViewTextBoxColumn
    Public WithEvents O As System.Windows.Forms.DataGridViewTextBoxColumn
    Public WithEvents P As System.Windows.Forms.DataGridViewTextBoxColumn
    Public WithEvents Q As System.Windows.Forms.DataGridViewTextBoxColumn
    Public WithEvents R As System.Windows.Forms.DataGridViewTextBoxColumn
    Public WithEvents S As System.Windows.Forms.DataGridViewTextBoxColumn
    Public WithEvents T As System.Windows.Forms.DataGridViewTextBoxColumn
    Public WithEvents U As System.Windows.Forms.DataGridViewTextBoxColumn
    Public WithEvents V As System.Windows.Forms.DataGridViewTextBoxColumn
    Public WithEvents W As System.Windows.Forms.DataGridViewTextBoxColumn
    Public WithEvents X As System.Windows.Forms.DataGridViewTextBoxColumn
    Public WithEvents Y As System.Windows.Forms.DataGridViewTextBoxColumn
    Public WithEvents Z As System.Windows.Forms.DataGridViewTextBoxColumn
End Class
