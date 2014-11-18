Module ControlExtensions
    <System.Runtime.CompilerServices.Extension()> _
    Public Sub UIThread(control As Control, code As Action)
        If control.InvokeRequired Then
            control.BeginInvoke(code)
        Else
            code.Invoke()
        End If
    End Sub

    <System.Runtime.CompilerServices.Extension()> _
    Public Sub UIThreadInvoke(control As Control, code As Action)
        If control.InvokeRequired Then
            control.Invoke(code)
        Else
            code.Invoke()
        End If
    End Sub

    <System.Runtime.CompilerServices.Extension()> _
    Public Function DropDownWidth(control As ListView) As Integer
        Dim maxWidth As Integer = 0, temp As Integer = 0
        For Each obj As Object In control.Items
            temp = TextRenderer.MeasureText(obj.ToString(), control.Font).Width
            If temp > maxWidth Then
                maxWidth = temp
            End If
        Next
        Return maxWidth
    End Function

    <System.Runtime.CompilerServices.Extension()> _
    Public Function DropDownHeight(control As ListView) As Integer
        Dim Height As Integer = 0, temp As Integer = 0
        For Each obj As Object In control.Items
            temp = TextRenderer.MeasureText(obj.ToString(), control.Font).Height
            Height += temp
        Next
        Return Height
    End Function

End Module
