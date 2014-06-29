<Global.Microsoft.VisualBasic.CompilerServices.DesignerGenerated()> _
Partial Class FormPEC
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
        Dim resources As System.ComponentModel.ComponentResourceManager = New System.ComponentModel.ComponentResourceManager(GetType(FormPEC))
        Me.Label2 = New System.Windows.Forms.Label()
        Me.Label1 = New System.Windows.Forms.Label()
        Me.SpinningProgress1 = New CircularProgress.SpinningProgress.SpinningProgress()
        Me.btnCancel = New System.Windows.Forms.Button()
        Me.SuspendLayout()
        '
        'Label2
        '
        resources.ApplyResources(Me.Label2, "Label2")
        Me.Label2.ImageKey = Global.DWSIM.My.Resources.DWSIM.String1
        Me.Label2.Name = "Label2"
        '
        'Label1
        '
        resources.ApplyResources(Me.Label1, "Label1")
        Me.Label1.ImageKey = Global.DWSIM.My.Resources.DWSIM.String1
        Me.Label1.Name = "Label1"
        '
        'SpinningProgress1
        '
        resources.ApplyResources(Me.SpinningProgress1, "SpinningProgress1")
        Me.SpinningProgress1.ActiveSegmentColour = System.Drawing.Color.RoyalBlue
        Me.SpinningProgress1.AutoIncrementFrequency = 75.0R
        Me.SpinningProgress1.BehindTransistionSegmentIsActive = False
        Me.SpinningProgress1.InactiveSegmentColour = System.Drawing.Color.LightSteelBlue
        Me.SpinningProgress1.Name = "SpinningProgress1"
        Me.SpinningProgress1.TransistionSegment = 10
        Me.SpinningProgress1.TransistionSegmentColour = System.Drawing.Color.LightSteelBlue
        '
        'btnCancel
        '
        resources.ApplyResources(Me.btnCancel, "btnCancel")
        Me.btnCancel.ImageKey = Global.DWSIM.My.Resources.DWSIM.String1
        Me.btnCancel.Name = "btnCancel"
        Me.btnCancel.UseVisualStyleBackColor = True
        '
        'FormPEC
        '
        resources.ApplyResources(Me, "$this")
        Me.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font
        Me.ControlBox = False
        Me.Controls.Add(Me.btnCancel)
        Me.Controls.Add(Me.Label2)
        Me.Controls.Add(Me.Label1)
        Me.Controls.Add(Me.SpinningProgress1)
        Me.FormBorderStyle = System.Windows.Forms.FormBorderStyle.FixedToolWindow
        Me.MaximizeBox = False
        Me.MinimizeBox = False
        Me.Name = "FormPEC"
        Me.ShowIcon = False
        Me.ShowInTaskbar = False
        Me.SizeGripStyle = System.Windows.Forms.SizeGripStyle.Hide
        Me.ResumeLayout(False)
        Me.PerformLayout()

    End Sub
    Public WithEvents Label2 As System.Windows.Forms.Label
    Public WithEvents Label1 As System.Windows.Forms.Label
    Public WithEvents SpinningProgress1 As CircularProgress.SpinningProgress.SpinningProgress
    Friend WithEvents btnCancel As System.Windows.Forms.Button
End Class
