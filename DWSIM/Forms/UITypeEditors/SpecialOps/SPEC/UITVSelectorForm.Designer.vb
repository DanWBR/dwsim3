<Global.Microsoft.VisualBasic.CompilerServices.DesignerGenerated()> _
Partial Class UITVSelectorForm
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
        Me.KryptonButton2 = New System.Windows.Forms.Button
        Me.KryptonButton1 = New System.Windows.Forms.Button
        Me.GroupBox1 = New System.Windows.Forms.GroupBox
        Me.TreeView3 = New System.Windows.Forms.TreeView
        Me.TreeView2 = New System.Windows.Forms.TreeView
        Me.TreeView1 = New System.Windows.Forms.TreeView
        Me.Label3 = New System.Windows.Forms.Label
        Me.Label2 = New System.Windows.Forms.Label
        Me.Label1 = New System.Windows.Forms.Label
        Me.GroupBox1.SuspendLayout()
        Me.SuspendLayout()
        '
        'KryptonButton2
        '
        Me.KryptonButton2.Location = New System.Drawing.Point(325, 288)
        Me.KryptonButton2.Name = "KryptonButton2"
        Me.KryptonButton2.Size = New System.Drawing.Size(90, 25)
        Me.KryptonButton2.TabIndex = 5
        Me.KryptonButton2.Text = "Cancel"






        '
        'KryptonButton1
        '
        Me.KryptonButton1.Location = New System.Drawing.Point(421, 288)
        Me.KryptonButton1.Name = "KryptonButton1"
        Me.KryptonButton1.Size = New System.Drawing.Size(90, 25)
        Me.KryptonButton1.TabIndex = 4
        Me.KryptonButton1.Text = "OK"






        '
        'GroupBox1
        '
        Me.GroupBox1.Controls.Add(Me.TreeView3)
        Me.GroupBox1.Controls.Add(Me.TreeView2)
        Me.GroupBox1.Controls.Add(Me.TreeView1)
        Me.GroupBox1.Controls.Add(Me.Label3)
        Me.GroupBox1.Controls.Add(Me.Label2)
        Me.GroupBox1.Controls.Add(Me.Label1)
        Me.GroupBox1.Location = New System.Drawing.Point(6, 3)
        Me.GroupBox1.Name = "GroupBox1"
        Me.GroupBox1.Size = New System.Drawing.Size(505, 276)
        Me.GroupBox1.TabIndex = 3
        Me.GroupBox1.TabStop = False
        Me.GroupBox1.Text = "Select target variable"
        '
        'TreeView3
        '
        Me.TreeView3.FullRowSelect = True
        Me.TreeView3.HideSelection = False
        Me.TreeView3.Location = New System.Drawing.Point(338, 32)
        Me.TreeView3.Name = "TreeView3"
        Me.TreeView3.ShowLines = False
        Me.TreeView3.ShowPlusMinus = False
        Me.TreeView3.ShowRootLines = False
        Me.TreeView3.Size = New System.Drawing.Size(160, 238)
        Me.TreeView3.TabIndex = 11
        '
        'TreeView2
        '
        Me.TreeView2.FullRowSelect = True
        Me.TreeView2.HideSelection = False
        Me.TreeView2.Location = New System.Drawing.Point(172, 32)
        Me.TreeView2.Name = "TreeView2"
        Me.TreeView2.ShowLines = False
        Me.TreeView2.ShowPlusMinus = False
        Me.TreeView2.ShowRootLines = False
        Me.TreeView2.Size = New System.Drawing.Size(160, 238)
        Me.TreeView2.TabIndex = 10
        '
        'TreeView1
        '
        Me.TreeView1.FullRowSelect = True
        Me.TreeView1.HideSelection = False
        Me.TreeView1.Location = New System.Drawing.Point(6, 32)
        Me.TreeView1.Name = "TreeView1"
        Me.TreeView1.ShowLines = False
        Me.TreeView1.ShowPlusMinus = False
        Me.TreeView1.ShowRootLines = False
        Me.TreeView1.Size = New System.Drawing.Size(160, 238)
        Me.TreeView1.TabIndex = 9
        '
        'Label3
        '
        Me.Label3.AutoSize = True
        Me.Label3.Location = New System.Drawing.Point(335, 16)
        Me.Label3.Name = "Label3"
        Me.Label3.Size = New System.Drawing.Size(48, 13)
        Me.Label3.TabIndex = 5
        Me.Label3.Text = "Variable:"
        '
        'Label2
        '
        Me.Label2.AutoSize = True
        Me.Label2.Location = New System.Drawing.Point(169, 16)
        Me.Label2.Name = "Label2"
        Me.Label2.Size = New System.Drawing.Size(41, 13)
        Me.Label2.TabIndex = 4
        Me.Label2.Text = "Object:"
        '
        'Label1
        '
        Me.Label1.AutoSize = True
        Me.Label1.Location = New System.Drawing.Point(3, 16)
        Me.Label1.Name = "Label1"
        Me.Label1.Size = New System.Drawing.Size(34, 13)
        Me.Label1.TabIndex = 3
        Me.Label1.Text = "Type:"
        '
        'UITVSelectorForm
        '
        Me.AutoScaleDimensions = New System.Drawing.SizeF(6.0!, 13.0!)
        Me.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font

        Me.ClientSize = New System.Drawing.Size(517, 321)
        Me.Controls.Add(Me.KryptonButton2)
        Me.Controls.Add(Me.KryptonButton1)
        Me.Controls.Add(Me.GroupBox1)
        Me.FormBorderStyle = System.Windows.Forms.FormBorderStyle.FixedToolWindow
        Me.Name = "UITVSelectorForm"
        Me.ShowIcon = False
        Me.ShowInTaskbar = False
        Me.StartPosition = System.Windows.Forms.FormStartPosition.CenterParent
        Me.Text = "UICVSelectorForm"
        Me.GroupBox1.ResumeLayout(False)
        Me.GroupBox1.PerformLayout()
        Me.ResumeLayout(False)

    End Sub
    Public WithEvents KryptonButton2 As System.Windows.Forms.Button
    Public WithEvents KryptonButton1 As System.Windows.Forms.Button
    Public WithEvents GroupBox1 As System.Windows.Forms.GroupBox
    Public WithEvents Label3 As System.Windows.Forms.Label
    Public WithEvents Label2 As System.Windows.Forms.Label
    Public WithEvents Label1 As System.Windows.Forms.Label
    Public WithEvents TreeView3 As System.Windows.Forms.TreeView
    Public WithEvents TreeView2 As System.Windows.Forms.TreeView
    Public WithEvents TreeView1 As System.Windows.Forms.TreeView
End Class
