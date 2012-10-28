Public Class AboutBoxNET

    Private Sub AboutBox_Load(ByVal sender As System.Object, ByVal e As System.EventArgs) Handles MyBase.Load

        Dim dt As DateTime = CType("01/01/2000", DateTime).AddDays(My.Application.Info.Version.Build).AddSeconds(My.Application.Info.Version.Revision * 2)
        Version.Text = "Version " & My.Application.Info.Version.Major & "." & My.Application.Info.Version.Minor & _
        ", Build " & My.Application.Info.Version.Build & " (" & Format(dt, "dd/MM/yyyy HH:mm") & ")"

        Copyright.Text = My.Application.Info.Copyright

        LblOSInfo.Text = My.Computer.Info.OSFullName & ", Version " & My.Computer.Info.OSVersion & ", " & My.Computer.Info.OSPlatform & " Platform"
        LblCLRInfo.Text = "Microsoft .NET Framework, Runtime Version " & System.Runtime.InteropServices.RuntimeEnvironment.GetSystemVersion.ToString()

    End Sub

    Private Sub Button1_Click(ByVal sender As System.Object, ByVal e As System.EventArgs) Handles Button1.Click
        Me.Close()
    End Sub

    Private Sub LblCLRInfo_Click(ByVal sender As System.Object, ByVal e As System.EventArgs) Handles LblCLRInfo.Click

    End Sub
End Class