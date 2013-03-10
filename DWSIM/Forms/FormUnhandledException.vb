'    Copyright 2008 Daniel Wagner O. de Medeiros
'
'    This file is part of DWSIM.
'
'    DWSIM is free software: you can redistribute it and/or modify
'    it under the terms of the GNU General Public License as published by
'    the Free Software Foundation, either version 3 of the License, or
'    (at your option) any later version.
'
'    DWSIM is distributed in the hope that it will be useful,
'    but WITHOUT ANY WARRANTY; without even the implied warranty of
'    MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE.  See the
'    GNU General Public License for more details.
'
'    You should have received a copy of the GNU General Public License
'    along with DWSIM.  If not, see <http://www.gnu.org/licenses/>.

Public Class FormUnhandledException

    Inherits System.Windows.Forms.Form

    Dim Loaded As Boolean = False
    Public ex As Exception

    Private Sub KryptonButton1_Click(ByVal sender As System.Object, ByVal e As System.EventArgs) Handles KryptonButton1.Click
        Me.Close()
        FormMain.sairdevez = True
        FormMain.Close()
    End Sub

    Private Sub KryptonButton2_Click(ByVal sender As System.Object, ByVal e As System.EventArgs) Handles KryptonButton2.Click
        Me.Close()
    End Sub

    Private Sub FormUnhandledException_Shown(ByVal sender As Object, ByVal e As System.EventArgs) Handles Me.Shown
        Me.Loaded = True
    End Sub

    Private Sub KryptonButton4_Click(ByVal sender As System.Object, ByVal e As System.EventArgs) Handles KryptonButton4.Click

        If Me.KryptonCheckBox1.Checked = False Then
            Application.Exit()
        Else
            FormMain.SairDiretoERRO = True
            Application.Restart()
        End If

    End Sub

    Private Sub KryptonCheckButton1_Click(ByVal sender As System.Object, ByVal e As System.EventArgs) Handles Button1.Click
        If Me.Loaded = True Then
            If Me.Height < 400 Then
                Me.Button1.Text = DWSIM.App.GetLocalString("Detalhes2")
                Me.Height = 455
            Else
                Me.Button1.Text = DWSIM.App.GetLocalString("Detalhes3")
                Me.Height = 231
            End If
        End If
    End Sub

    Private Sub FormUnhandledException_Load(ByVal sender As System.Object, ByVal e As System.EventArgs) Handles MyBase.Load
        Me.Button1.Text = DWSIM.App.GetLocalString("Detalhes3")
    End Sub

    Private Sub Button2_Click(ByVal sender As System.Object, ByVal e As System.EventArgs) Handles Button2.Click

        Dim msg As New SendFileTo.MAPI()
        msg.AddRecipientTo("dwsim@inforside.com.br")
        msg.SendMailPopup("DWSIM Exception", "[PLEASE ADD EXCEPTION DETAILS HERE]" & vbCrLf & vbCrLf & "DWSIM version: " & My.Application.Info.Version.ToString & vbCrLf & ex.ToString)

    End Sub

End Class