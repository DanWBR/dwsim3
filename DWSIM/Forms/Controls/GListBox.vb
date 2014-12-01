Namespace UserControls

    ' GListBoxItem class 
    Public Class GListBoxItem
        Private _myText As String
        Private _myImageIndex As Integer
        ' properties 
        Public Property Text() As String
            Get
                Return _myText
            End Get
            Set(ByVal value As String)
                _myText = value
            End Set
        End Property
        Public Property ImageIndex() As Integer
            Get
                Return _myImageIndex
            End Get
            Set(ByVal value As Integer)
                _myImageIndex = value
            End Set
        End Property
        'constructor
        Public Sub New(ByVal text As String, ByVal index As Integer)
            _myText = text
            _myImageIndex = index
        End Sub
        Public Sub New(ByVal text As String)
            Me.New(text, -1)
        End Sub
        Public Sub New()
            Me.New("")
        End Sub
        Public Overrides Function ToString() As String
            Return _myText
        End Function
    End Class
    'End of GListBoxItem class
    ' GListBox class 
    Public Class GListBox
        Inherits ListBox
        Private _myImageList As ImageList
        Public Property ImageList() As ImageList
            Get
                Return _myImageList
            End Get
            Set(ByVal value As ImageList)
                _myImageList = value
            End Set
        End Property
        Public Sub New()
            ' Set owner draw mode
            Me.DrawMode = DrawMode.OwnerDrawFixed
        End Sub
        Protected Overrides Sub OnDrawItem(ByVal e As System.Windows.Forms.DrawItemEventArgs)
            e.DrawBackground()
            e.DrawFocusRectangle()
            Dim item As GListBoxItem
            Dim bounds As Rectangle = e.Bounds
            Try
                item = DirectCast(Items(e.Index), GListBoxItem)
                If item.ImageIndex <> -1 Then
                    Dim imageSize As Size = _myImageList.ImageSize
                    _myImageList.Draw(e.Graphics, bounds.Left, bounds.Top, item.ImageIndex)
                    e.Graphics.DrawString(item.Text, e.Font, New SolidBrush(e.ForeColor), bounds.Left + imageSize.Width, bounds.Top)
                Else
                    e.Graphics.DrawString(item.Text, e.Font, New SolidBrush(e.ForeColor), bounds.Left, bounds.Top)
                End If
            Catch
                If e.Index <> -1 Then
                    e.Graphics.DrawString(Items(e.Index).ToString(), e.Font, New SolidBrush(e.ForeColor), bounds.Left, bounds.Top)
                Else
                    e.Graphics.DrawString(Text, e.Font, New SolidBrush(e.ForeColor), bounds.Left, bounds.Top)
                End If
            End Try
            MyBase.OnDrawItem(e)
        End Sub
    End Class

#Region "MemberItem class"
    ''' <summary>
    ''' Used for storing member items which are then
    ''' alphabetically sorted.
    ''' </summary>
    Public Class MemberItem
        Implements IComparable
        Public DisplayText As String
        Public Tag As Object

        Public Function CompareTo(ByVal obj As Object) As Integer Implements IComparable.CompareTo
            Dim result As Integer = 1
            If obj IsNot Nothing Then
                If TypeOf obj Is MemberItem Then
                    Dim memberItem As MemberItem = DirectCast(obj, MemberItem)
                    Return (Me.DisplayText.CompareTo(memberItem.DisplayText))
                Else
                    Throw New ArgumentException()
                End If
            End If


            Return result
        End Function

    End Class
#End Region

End Namespace
