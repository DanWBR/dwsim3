Imports System.Reflection

Public Class AboutBoxMONO

    Private Sub AboutBox_Load(ByVal sender As System.Object, ByVal e As System.EventArgs) Handles MyBase.Load

        Dim dt As DateTime = CType("01/01/2000", DateTime).AddDays(My.Application.Info.Version.Build).AddSeconds(My.Application.Info.Version.Revision * 2)
        Version.Text = "Version " & My.Application.Info.Version.Major & "." & My.Application.Info.Version.Minor & _
        ", Build " & My.Application.Info.Version.Build & " (" & Format(dt, "dd/MM/yyyy HH:mm") & ")"

        Copyright.Text = My.Application.Info.Copyright

        LblOSInfo.Text = My.Computer.Info.OSFullName & ", Version " & My.Computer.Info.OSVersion & ", " & My.Computer.Info.OSPlatform & " Platform"
        If DWSIM.App.IsRunningOnMono() Then
            Dim displayName As MethodInfo = Type.GetType("Mono.Runtime").GetMethod("GetDisplayName", BindingFlags.NonPublic Or BindingFlags.[Static])
            If displayName IsNot Nothing Then
                LblCLRInfo.Text = "Mono " + displayName.Invoke(Nothing, Nothing) + " / " + System.Runtime.InteropServices.RuntimeEnvironment.GetSystemVersion.ToString()
            Else
                LblCLRInfo.Text = System.Runtime.InteropServices.RuntimeEnvironment.GetSystemVersion.ToString()
            End If
        Else
            LblCLRInfo.Text = System.Runtime.InteropServices.RuntimeEnvironment.GetSystemVersion.ToString()
        End If

    End Sub

    Private Sub Button1_Click(ByVal sender As System.Object, ByVal e As System.EventArgs) Handles Button1.Click
        Me.Close()
    End Sub

    Private Sub LblCLRInfo_Click(ByVal sender As System.Object, ByVal e As System.EventArgs) Handles LblCLRInfo.Click

    End Sub
End Class