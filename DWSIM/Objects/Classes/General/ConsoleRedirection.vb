Imports System.Text
Imports System.IO
Imports System.Windows.Forms

Namespace ConsoleRedirection
    Public Class TextBoxStreamWriter
        Inherits TextWriter

        Public Sub New()

        End Sub

        Public Overrides Sub Write(ByVal value As Char)
            MyBase.Write(value)
            If Not My.Application.ActiveSimulation Is Nothing Then
                If Not My.Application.ActiveSimulation.FormOutput Is Nothing Then
                    My.Application.ActiveSimulation.FormOutput.TextBox1.AppendText(value.ToString())
                End If
            End If
        End Sub

        Public Overrides ReadOnly Property Encoding() As Encoding
            Get
                Return System.Text.Encoding.UTF8
            End Get
        End Property
    End Class
End Namespace

