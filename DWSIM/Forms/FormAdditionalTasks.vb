Public Class FormAdditionalTasks

    Private Sub FormAdditionalTasks_Load(sender As Object, e As EventArgs) Handles MyBase.Load

        lblOSPlatform.Text = My.Computer.Info.OSFullName & " / " & My.Computer.Info.OSVersion & " / " & My.Computer.Info.OSPlatform

        If Environment.Is64BitOperatingSystem Then
            lblOSEnvironment.Text = "64-bit"
        Else
            lblOSEnvironment.Text = "32-bit"
        End If

        If Environment.Is64BitProcess Then
            lblOSEnvironment.Text += " / 64-bit"
        Else
            lblOSEnvironment.Text += " / 32-bit"
        End If

    End Sub

End Class