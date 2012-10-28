<Global.Microsoft.VisualBasic.CompilerServices.DesignerGenerated()> _
Partial Class PipeEditorForm
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
        Dim resources As System.ComponentModel.ComponentResourceManager = New System.ComponentModel.ComponentResourceManager(GetType(PipeEditorForm))
        Me.GroupBox1 = New System.Windows.Forms.GroupBox
        Me.KryptonRadioButton2 = New System.Windows.Forms.RadioButton
        Me.KryptonRadioButton1 = New System.Windows.Forms.RadioButton
        Me.GraphControl = New ZedGraph.ZedGraphControl
        Me.PipeEditor1 = New PipeEditor
        Me.Button1 = New System.Windows.Forms.Button
        Me.GroupBox1.SuspendLayout()
        Me.SuspendLayout()
        '
        'GroupBox1
        '
        Me.GroupBox1.Controls.Add(Me.KryptonRadioButton2)
        Me.GroupBox1.Controls.Add(Me.KryptonRadioButton1)
        Me.GroupBox1.Controls.Add(Me.GraphControl)
        resources.ApplyResources(Me.GroupBox1, "GroupBox1")
        Me.GroupBox1.Name = "GroupBox1"
        Me.GroupBox1.TabStop = False
        '
        'KryptonRadioButton2
        '
        resources.ApplyResources(Me.KryptonRadioButton2, "KryptonRadioButton2")
        Me.KryptonRadioButton2.Name = "KryptonRadioButton2"
        Me.KryptonRadioButton2.AutoSize = True
        '
        'KryptonRadioButton1
        '
        Me.KryptonRadioButton1.Checked = True
        resources.ApplyResources(Me.KryptonRadioButton1, "KryptonRadioButton1")
        Me.KryptonRadioButton1.Name = "KryptonRadioButton1"
        Me.KryptonRadioButton1.TabStop = True
        Me.KryptonRadioButton1.AutoSize = True
        '
        'GraphControl
        '
        Me.GraphControl.BackColor = System.Drawing.Color.WhiteSmoke
        resources.ApplyResources(Me.GraphControl, "GraphControl")
        Me.GraphControl.IsAntiAlias = True
        Me.GraphControl.IsAutoScrollRange = True
        Me.GraphControl.Name = "GraphControl"
        Me.GraphControl.ScrollGrace = 0
        Me.GraphControl.ScrollMaxX = 0
        Me.GraphControl.ScrollMaxY = 0
        Me.GraphControl.ScrollMaxY2 = 0
        Me.GraphControl.ScrollMinX = 0
        Me.GraphControl.ScrollMinY = 0
        Me.GraphControl.ScrollMinY2 = 0
        '
        'PipeEditor1
        '
        Me.PipeEditor1.BackColor = System.Drawing.Color.Transparent
        resources.ApplyResources(Me.PipeEditor1, "PipeEditor1")
        Me.PipeEditor1.Name = "PipeEditor1"
        Me.PipeEditor1.Profile = Nothing
        '
        'Button1
        '
        resources.ApplyResources(Me.Button1, "Button1")
        Me.Button1.Name = "Button1"
        Me.Button1.UseVisualStyleBackColor = True
        '
        'PipeEditorForm
        '
        resources.ApplyResources(Me, "$this")
        Me.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font
        Me.Controls.Add(Me.Button1)
        Me.Controls.Add(Me.GroupBox1)
        Me.Controls.Add(Me.PipeEditor1)
        Me.FormBorderStyle = System.Windows.Forms.FormBorderStyle.FixedToolWindow
        Me.HelpButton = True
        Me.MaximizeBox = False
        Me.MinimizeBox = False
        Me.Name = "PipeEditorForm"
        Me.ShowIcon = False
        Me.ShowInTaskbar = False
        Me.GroupBox1.ResumeLayout(False)
        Me.ResumeLayout(False)

    End Sub
    Public WithEvents PipeEditor1 As PipeEditor

    Public WithEvents GroupBox1 As System.Windows.Forms.GroupBox
    Public WithEvents GraphControl As ZedGraph.ZedGraphControl
    Public WithEvents KryptonRadioButton2 As System.Windows.Forms.RadioButton
    Public WithEvents KryptonRadioButton1 As System.Windows.Forms.RadioButton
    Public WithEvents Button1 As System.Windows.Forms.Button
End Class
