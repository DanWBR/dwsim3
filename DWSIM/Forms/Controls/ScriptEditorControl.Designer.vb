<Global.Microsoft.VisualBasic.CompilerServices.DesignerGenerated()> _
Partial Class ScriptEditorControl
    Inherits System.Windows.Forms.UserControl

    'UserControl overrides dispose to clean up the component list.
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
        Dim resources As System.ComponentModel.ComponentResourceManager = New System.ComponentModel.ComponentResourceManager(GetType(ScriptEditorControl))
        Me.TableLayoutPanel1 = New System.Windows.Forms.TableLayoutPanel()
        Me.txtScript = New Alsing.Windows.Forms.SyntaxBoxControl()
        Me.Panel1 = New System.Windows.Forms.Panel()
        Me.ComboBox2 = New System.Windows.Forms.ComboBox()
        Me.Label2 = New System.Windows.Forms.Label()
        Me.ComboBox1 = New System.Windows.Forms.ComboBox()
        Me.Label1 = New System.Windows.Forms.Label()
        Me.CheckBox1 = New System.Windows.Forms.CheckBox()
        Me.TableLayoutPanel1.SuspendLayout()
        Me.Panel1.SuspendLayout()
        Me.SuspendLayout()
        '
        'TableLayoutPanel1
        '
        resources.ApplyResources(Me.TableLayoutPanel1, "TableLayoutPanel1")
        Me.TableLayoutPanel1.Controls.Add(Me.txtScript, 0, 1)
        Me.TableLayoutPanel1.Controls.Add(Me.Panel1, 0, 0)
        Me.TableLayoutPanel1.Name = "TableLayoutPanel1"
        '
        'txtScript
        '
        Me.txtScript.ActiveView = Alsing.Windows.Forms.ActiveView.BottomRight
        Me.txtScript.AllowBreakPoints = False
        Me.txtScript.AutoListPosition = Nothing
        Me.txtScript.AutoListSelectedText = "a123"
        Me.txtScript.AutoListVisible = False
        Me.txtScript.BackColor = System.Drawing.Color.White
        Me.txtScript.BorderStyle = Alsing.Windows.Forms.BorderStyle.None
        Me.txtScript.CopyAsRTF = True
        resources.ApplyResources(Me.txtScript, "txtScript")
        Me.txtScript.FontName = "Courier new"
        Me.txtScript.HighLightActiveLine = True
        Me.txtScript.InfoTipCount = 1
        Me.txtScript.InfoTipPosition = Nothing
        Me.txtScript.InfoTipSelectedIndex = 1
        Me.txtScript.InfoTipVisible = False
        Me.txtScript.LockCursorUpdate = False
        Me.txtScript.Name = "txtScript"
        Me.txtScript.ParseOnPaste = True
        Me.txtScript.ShowGutterMargin = False
        Me.txtScript.ShowScopeIndicator = False
        Me.txtScript.ShowTabGuides = True
        Me.txtScript.SmoothScroll = True
        Me.txtScript.SplitviewH = -4
        Me.txtScript.SplitviewV = -4
        Me.txtScript.TabGuideColor = System.Drawing.Color.FromArgb(CType(CType(244, Byte), Integer), CType(CType(243, Byte), Integer), CType(CType(234, Byte), Integer))
        Me.txtScript.WhitespaceColor = System.Drawing.SystemColors.ControlDark
        '
        'Panel1
        '
        Me.Panel1.Controls.Add(Me.ComboBox2)
        Me.Panel1.Controls.Add(Me.Label2)
        Me.Panel1.Controls.Add(Me.ComboBox1)
        Me.Panel1.Controls.Add(Me.Label1)
        Me.Panel1.Controls.Add(Me.CheckBox1)
        resources.ApplyResources(Me.Panel1, "Panel1")
        Me.Panel1.Name = "Panel1"
        '
        'ComboBox2
        '
        resources.ApplyResources(Me.ComboBox2, "ComboBox2")
        Me.ComboBox2.DropDownStyle = System.Windows.Forms.ComboBoxStyle.DropDownList
        Me.ComboBox2.FormattingEnabled = True
        Me.ComboBox2.Name = "ComboBox2"
        '
        'Label2
        '
        resources.ApplyResources(Me.Label2, "Label2")
        Me.Label2.Name = "Label2"
        '
        'ComboBox1
        '
        resources.ApplyResources(Me.ComboBox1, "ComboBox1")
        Me.ComboBox1.DropDownStyle = System.Windows.Forms.ComboBoxStyle.DropDownList
        Me.ComboBox1.FormattingEnabled = True
        Me.ComboBox1.Name = "ComboBox1"
        '
        'Label1
        '
        resources.ApplyResources(Me.Label1, "Label1")
        Me.Label1.Name = "Label1"
        '
        'CheckBox1
        '
        resources.ApplyResources(Me.CheckBox1, "CheckBox1")
        Me.CheckBox1.Name = "CheckBox1"
        Me.CheckBox1.UseVisualStyleBackColor = True
        '
        'ScriptEditorControl
        '
        resources.ApplyResources(Me, "$this")
        Me.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font
        Me.Controls.Add(Me.TableLayoutPanel1)
        Me.Name = "ScriptEditorControl"
        Me.TableLayoutPanel1.ResumeLayout(False)
        Me.Panel1.ResumeLayout(False)
        Me.Panel1.PerformLayout()
        Me.ResumeLayout(False)

    End Sub
    Friend WithEvents TableLayoutPanel1 As System.Windows.Forms.TableLayoutPanel
    Public WithEvents txtScript As Alsing.Windows.Forms.SyntaxBoxControl
    Friend WithEvents Panel1 As System.Windows.Forms.Panel
    Friend WithEvents ComboBox2 As System.Windows.Forms.ComboBox
    Friend WithEvents Label2 As System.Windows.Forms.Label
    Friend WithEvents ComboBox1 As System.Windows.Forms.ComboBox
    Friend WithEvents Label1 As System.Windows.Forms.Label
    Friend WithEvents CheckBox1 As System.Windows.Forms.CheckBox

End Class
