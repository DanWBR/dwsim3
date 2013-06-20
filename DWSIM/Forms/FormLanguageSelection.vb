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

Public Class FormLanguageSelection

    Inherits System.Windows.Forms.Form

    Sub UpdateLanguage()

        'carrega o idioma atual
        My.MyApplication._CultureInfo = New Globalization.CultureInfo(My.Settings.CultureInfo)
        My.Application.ChangeUICulture(My.Settings.CultureInfo)

        'carrega o gerenciador de recursos
        My.MyApplication._ResourceManager = New System.Resources.ResourceManager("DWSIM.DWSIM", System.Reflection.Assembly.GetExecutingAssembly())

    End Sub

    Private Sub Button2_Click(ByVal sender As System.Object, ByVal e As System.EventArgs) Handles Button2.Click
        My.Settings.CultureInfo = "pt-BR"
        My.Settings.ShowLangForm = False
        UpdateLanguage()
        Me.Close()
    End Sub

    Private Sub Button3_Click(ByVal sender As System.Object, ByVal e As System.EventArgs) Handles Button3.Click
        My.Settings.CultureInfo = "en"
        My.Settings.ShowLangForm = False
        UpdateLanguage()
        Me.Close()
    End Sub

    Private Sub Button4_Click(ByVal sender As System.Object, ByVal e As System.EventArgs) Handles Button4.Click
        My.Settings.CultureInfo = "es"
        My.Settings.ShowLangForm = False
        UpdateLanguage()
        Me.Close()
    End Sub

    Private Sub Button1_Click(ByVal sender As System.Object, ByVal e As System.EventArgs) Handles Button1.Click
        My.Settings.CultureInfo = "de"
        My.Settings.ShowLangForm = False
        UpdateLanguage()
        Me.Close()
    End Sub
End Class