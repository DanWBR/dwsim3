Imports Microsoft.VisualBasic.Strings
Imports NetOffice
Imports Excel = NetOffice.ExcelApi
Imports NetOffice.ExcelApi.Enums

Public Class ExcelUOEditorForm
    Public FilePath As String

    Private Sub BtnSearch_Click(sender As System.Object, e As System.EventArgs) Handles BtnSearch.Click
        OpenFileDialog1.FileName = TbFileName.Text
        OpenFileDialog1.Filter = "Excel files|*.xls; *xlsx"

        If OpenFileDialog1.ShowDialog() = System.Windows.Forms.DialogResult.OK Then
            Dim s As String = OpenFileDialog1.FileName
            TbFileName.Text = Strings.Right(s, Len(s) - InStrRev(s, "\"))
        End If
    End Sub

    Private Function ExctractFilepath(ByVal S As String) As String
        Dim P1, P2 As Integer
        P1 = InStr(1, S, "(") + 1
        P2 = InStrRev(S, "\") + 1

        Return Mid(S, P1, P2 - P1)
    End Function
    Private Sub Button2_Click(sender As System.Object, e As System.EventArgs) Handles Button2.Click
        Dim fp As String = My.Application.ActiveSimulation.Text

        If TbFileName.Text <> "" Then
            Dim xcl As New Excel.Application()
            Dim mybook As Excel.Workbook
            Dim EXFN As String = ExctractFilepath(fp)

            xcl.Visible = True
            mybook = xcl.Workbooks.Open(EXFN & TbFileName.Text)

        End If
    End Sub
End Class