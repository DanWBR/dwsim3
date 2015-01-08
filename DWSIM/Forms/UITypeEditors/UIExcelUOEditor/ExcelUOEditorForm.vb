'    Excel Unit Editor Form 
'    Copyright 2014 Gregor Reichert
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


Imports Microsoft.VisualBasic.Strings
Imports NetOffice
Imports Excel = NetOffice.ExcelApi
Imports NetOffice.ExcelApi.Enums

Public Class ExcelUOEditorForm
    Public FilePath As String

    Private Sub BtnSearch_Click(sender As System.Object, e As System.EventArgs) Handles BtnSearch.Click
        OpenFileDialog1.FileName = TbFileName.Text
        OpenFileDialog1.Filter = "Excel files|*.xls; *xlsx"

        OpenFileDialog1.ValidateNames = True
        OpenFileDialog1.CheckFileExists = True
        OpenFileDialog1.CheckPathExists = True

        If OpenFileDialog1.ShowDialog() = System.Windows.Forms.DialogResult.OK Then
            TbFileName.Text = OpenFileDialog1.FileName
        End If
    End Sub

    Private Function ExctractFilepath(ByVal S As String) As String
        Dim P1, P2 As Integer
        P1 = InStr(1, S, "(") + 1
        P2 = InStrRev(S, "\") + 1

        Return Mid(S, P1, P2 - P1)
    End Function
    Private Sub BtnEdit_Click(sender As System.Object, e As System.EventArgs) Handles BtnEdit.Click
        If TbFileName.Text <> "" Then
            If My.Computer.FileSystem.FileExists(TbFileName.Text) Then
                Dim excelType As Type = Type.GetTypeFromProgID("Excel.Application")
                Dim excelProxy As Object = Activator.CreateInstance(excelType)
                Using xcl As New Excel.Application(Nothing, excelProxy)
                    For Each CurrAddin As Excel.AddIn In xcl.AddIns
                        If CurrAddin.Installed Then
                            CurrAddin.Installed = False
                            CurrAddin.Installed = True
                        End If
                    Next
                    xcl.Visible = True
                    xcl.Workbooks.Open(TbFileName.Text, True, False)
                End Using
            Else
                MessageBox.Show(DWSIM.App.GetLocalString("Oarquivonoexisteoufo"), DWSIM.App.GetLocalString("Erroaoabrirarquivo"), MessageBoxButtons.OK, MessageBoxIcon.Error)
            End If
        End If
    End Sub

    Private Sub BtnNew_Click(sender As System.Object, e As System.EventArgs) Handles BtnNew.Click
        Dim FileName As String = My.Application.ActiveSimulation.Text
        OpenFileDialog1.Title = "New Filename"
        OpenFileDialog1.Filter = "Excel files|*.xlsx; *xls"
        OpenFileDialog1.ValidateNames = False
        OpenFileDialog1.CheckFileExists = False
        OpenFileDialog1.CheckPathExists = True
        OpenFileDialog1.InitialDirectory = ExctractFilepath(FileName)

        If OpenFileDialog1.ShowDialog() = System.Windows.Forms.DialogResult.OK Then
            Dim s As String = OpenFileDialog1.FileName
            FileCopy(My.Application.Info.DirectoryPath & "\TemplateExcelUO.xlsx", s)
            TbFileName.Text = s
        End If
    End Sub

End Class