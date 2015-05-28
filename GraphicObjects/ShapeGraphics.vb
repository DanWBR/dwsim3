'Copyright (C) 2002 Microsoft Corporation
'All rights reserved.
'
'THIS CODE AND INFORMATION IS PROVIDED "AS IS" WITHOUT WARRANTY OF ANY KIND, EITHER
'EXPRESSED OR IMPLIED, INCLUDING BUT NOT LIMITED TO THE IMPLIED WARRANTIES OF
'MERCHANTIBILITY AND/OR FITNESS FOR A PARTICULAR PURPOSE.
'
'Date: June 2002
'Author: Duncan Mackenzie
'Modified by: Daniel W. O. de Medeiros
'
'Requires the release version of .NET Framework

Imports System.Drawing
Imports System.Drawing.Drawing2D

Namespace GraphicObjects

    <Serializable()> Public MustInherit Class ShapeGraphic

        Inherits GraphicObject

        Protected m_lineWidth As Single = 3
        Protected m_lineColor As Color = Color.Black
        Protected m_fillColor As Color = Color.LightGray
        Protected m_fill As Boolean = False
        Protected m_gradientmode As Boolean = True
        Protected m_gradientcolor1 As Color = Color.LightGray
        Protected m_gradientcolor2 As Color = Color.White

        Public Sub UpdateStatus(ByRef gobj As ShapeGraphic)

            Select Case gobj.Status
                Case GraphicObjects.Status.Calculated
                    gobj.LineColor = Color.SteelBlue
                Case GraphicObjects.Status.Calculating
                    gobj.LineColor = Color.YellowGreen
                Case GraphicObjects.Status.ErrorCalculating
                    gobj.LineColor = Color.DarkRed
                Case GraphicObjects.Status.Idle
                    gobj.LineColor = Color.SteelBlue
                Case GraphicObjects.Status.Inactive
                    gobj.LineColor = Color.Gray
            End Select

        End Sub

        Public Sub UpdateStatusC(ByRef ConnPen As Pen, ByRef Conn As ConnectorGraphic, ByRef p1 As Point, ByRef p2 As Point)

            Dim color1, color2, color3 As Color
            color1 = Color.SteelBlue
            color2 = Color.Salmon
            color3 = Color.YellowGreen

            If Conn.AttachedFrom.Status = GraphicObjects.Status.Calculated And Conn.AttachedTo.Status = GraphicObjects.Status.Calculated Then
                ConnPen = New Pen(New LinearGradientBrush(p1, p2, color1, color1), Me.LineWidth)
            ElseIf Conn.AttachedFrom.Status = GraphicObjects.Status.Calculated And Conn.AttachedTo.Status = GraphicObjects.Status.ErrorCalculating Then
                ConnPen = New Pen(New LinearGradientBrush(p1, p2, color1, color2), Me.LineWidth)
            ElseIf Conn.AttachedFrom.Status = GraphicObjects.Status.ErrorCalculating And Conn.AttachedTo.Status = GraphicObjects.Status.Calculated Then
                ConnPen = New Pen(New LinearGradientBrush(p1, p2, color2, color1), Me.LineWidth)
            ElseIf Conn.AttachedFrom.Status = GraphicObjects.Status.ErrorCalculating And Conn.AttachedTo.Status = GraphicObjects.Status.ErrorCalculating Then
                ConnPen = New Pen(New LinearGradientBrush(p1, p2, color2, color2), Me.LineWidth)
            ElseIf Conn.AttachedFrom.Status = GraphicObjects.Status.Calculating And Conn.AttachedTo.Status = GraphicObjects.Status.ErrorCalculating Then
                ConnPen = New Pen(New LinearGradientBrush(p1, p2, color3, color2), Me.LineWidth)
            ElseIf Conn.AttachedFrom.Status = GraphicObjects.Status.ErrorCalculating And Conn.AttachedTo.Status = GraphicObjects.Status.Calculating Then
                ConnPen = New Pen(New LinearGradientBrush(p1, p2, color2, color3), Me.LineWidth)
            ElseIf Conn.AttachedFrom.Status = GraphicObjects.Status.Calculating And Conn.AttachedTo.Status = GraphicObjects.Status.Calculated Then
                ConnPen = New Pen(New LinearGradientBrush(p1, p2, color3, color1), Me.LineWidth)
            ElseIf Conn.AttachedFrom.Status = GraphicObjects.Status.Calculated And Conn.AttachedTo.Status = GraphicObjects.Status.Calculating Then
                ConnPen = New Pen(New LinearGradientBrush(p1, p2, color1, color3), Me.LineWidth)
            End If

            With ConnPen
                .EndCap = Drawing2D.LineCap.ArrowAnchor
                .StartCap = Drawing2D.LineCap.NoAnchor
                .LineJoin = LineJoin.Round
            End With

        End Sub

        Public Overridable Property LineWidth() As Single
            Get
                Return m_lineWidth
            End Get
            Set(ByVal Value As Single)
                If Value > 0 Then
                    m_lineWidth = Value
                Else
                    Throw New ArgumentOutOfRangeException("LineWidth", "Line Width must be > 0")
                End If
            End Set
        End Property

        Public Overridable Property GradientMode() As Boolean
            Get
                Return m_gradientmode
            End Get
            Set(ByVal Value As Boolean)
                m_gradientmode = Value
            End Set
        End Property

        Public Overridable Property LineColor() As Color
            Get
                Return m_lineColor
            End Get
            Set(ByVal Value As Color)
                m_lineColor = Value
            End Set
        End Property

        Public Overridable Property Fill() As Boolean
            Get
                Return m_fill
            End Get
            Set(ByVal Value As Boolean)
                m_fill = Value
            End Set
        End Property

        Public Overridable Property FillColor() As Color
            Get
                Return m_fillColor
            End Get
            Set(ByVal Value As Color)
                m_fillColor = Value
            End Set
        End Property

        Public Overridable Property GradientColor1() As Color
            Get
                Return m_gradientcolor1
            End Get
            Set(ByVal Value As Color)
                m_gradientcolor1 = Value
            End Set
        End Property

        Public Overridable Property GradientColor2() As Color
            Get
                Return m_gradientcolor2
            End Get
            Set(ByVal Value As Color)
                m_gradientcolor2 = Value
            End Set
        End Property

        Public Overrides Sub Draw(ByVal g As System.Drawing.Graphics)
            MyBase.Draw(g)
        End Sub

        Protected Function GetRoundedLine(ByVal points As PointF(), ByVal cornerRadius As Single) As GraphicsPath
            Dim path As New GraphicsPath()
            Dim previousEndPoint As PointF = PointF.Empty
            Dim thisradius As Single = cornerRadius
            Dim dx, dy As Single
            For i As Integer = 1 To points.Length - 1
                Dim startPoint As PointF = points(i - 1)
                Dim endPoint As PointF = points(i)

                thisradius = cornerRadius
                dx = Math.Abs(endPoint.X - startPoint.X)
                dy = Math.Abs(endPoint.Y - startPoint.Y)
                If dx + dy <= 2 * cornerRadius Then
                    thisradius = (dx + dy) / 2
                End If

                If i > 1 Then
                    ' shorten start point and add bezier curve for all but the first line segment:
                    Dim cornerPoint As PointF = startPoint
                    LengthenLine(endPoint, startPoint, -thisradius)
                    Dim controlPoint1 As PointF = cornerPoint
                    Dim controlPoint2 As PointF = cornerPoint
                    LengthenLine(previousEndPoint, controlPoint1, -thisradius / 2)
                    LengthenLine(startPoint, controlPoint2, -thisradius / 2)
                    path.AddBezier(previousEndPoint, controlPoint1, controlPoint2, startPoint)
                End If
                If i + 1 < points.Length Then
                    ' shorten end point of all but the last line segment.
                    LengthenLine(startPoint, endPoint, -thisradius)
                End If

                path.AddLine(startPoint, endPoint)
                previousEndPoint = endPoint
            Next
            Return path
        End Function

        Public Sub LengthenLine(ByVal startPoint As PointF, ByRef endPoint As PointF, ByVal pixelCount As Single)
            If startPoint.Equals(endPoint) Then
                Return
            End If
            ' not a line
            Dim dx As Double = endPoint.X - startPoint.X
            Dim dy As Double = endPoint.Y - startPoint.Y
            If dx = 0 Then
                ' vertical line:
                If endPoint.Y < startPoint.Y Then
                    endPoint.Y -= pixelCount
                Else
                    endPoint.Y += pixelCount
                End If
            ElseIf dy = 0 Then
                ' horizontal line:
                If endPoint.X < startPoint.X Then
                    endPoint.X -= pixelCount
                Else
                    endPoint.X += pixelCount
                End If
            Else
                ' non-horizontal, non-vertical line:
                Dim length As Double = Math.Sqrt(dx * dx + dy * dy)
                Dim scale As Double = (length + pixelCount) / length
                dx *= scale
                dy *= scale
                endPoint.X = startPoint.X + Convert.ToSingle(dx)
                endPoint.Y = startPoint.Y + Convert.ToSingle(dy)
            End If
        End Sub

    End Class

    <Serializable()> Public Class PumpGraphic

        Inherits ShapeGraphic

#Region "Constructors"
        Public Sub New()
            Me.TipoObjeto = GraphicObjects.TipoObjeto.Pump
            Me.Description = "Bomba"
        End Sub

        Public Sub New(ByVal graphicPosition As Point)
            Me.New()
            Me.SetPosition(graphicPosition)
        End Sub

        Public Sub New(ByVal posX As Integer, ByVal posY As Integer)
            Me.New(New Point(posX, posY))
        End Sub

        Public Sub New(ByVal graphicPosition As Point, ByVal graphicSize As Size)
            Me.New(graphicPosition)
            Me.SetSize(graphicSize)
            Me.AutoSize = False
        End Sub

        Public Sub New(ByVal posX As Integer, ByVal posY As Integer, ByVal graphicSize As Size)
            Me.New(New Point(posX, posY), graphicSize)
        End Sub

        Public Sub New(ByVal posX As Integer, ByVal posY As Integer, ByVal width As Integer, ByVal height As Integer)
            Me.New(New Point(posX, posY), New Size(width, height))
        End Sub

        Public Sub New(ByVal graphicPosition As Point, ByVal Rotation As Single)
            Me.New()
            Me.SetPosition(graphicPosition)
            Me.Rotation = Rotation
        End Sub

        Public Sub New(ByVal posX As Integer, ByVal posY As Integer, ByVal Rotation As Single)
            Me.New(New Point(posX, posY), Rotation)
        End Sub

        Public Sub New(ByVal graphicPosition As Point, ByVal graphicSize As Size, ByVal Rotation As Single)
            Me.New(graphicPosition, Rotation)
            Me.SetSize(graphicSize)
            Me.AutoSize = False
        End Sub

        Public Sub New(ByVal posX As Integer, ByVal posY As Integer, ByVal graphicSize As Size, ByVal Rotation As Single)
            Me.New(New Point(posX, posY), graphicSize, Rotation)
        End Sub

        Public Sub New(ByVal posX As Integer, ByVal posY As Integer, ByVal width As Integer, _
                               ByVal height As Integer, ByVal Rotation As Single)
            Me.New(New Point(posX, posY), New Size(width, height), Rotation)
        End Sub

#End Region


        Public Overrides Sub CreateConnectors(InCount As Integer, OutCount As Integer)

            Dim myIC1 As New ConnectionPoint
            myIC1.Position = New Point(X, Y + 0.5 * Height)
            myIC1.Type = ConType.ConIn

            Dim myIC2 As New ConnectionPoint
            myIC2.Position = New Point(X + 0.5 * Width, Y + Height)
            myIC2.Type = ConType.ConEn

            Dim myOC1 As New ConnectionPoint
            myOC1.Position = New Point(X + Width, Y + 0.1 * Height)
            myOC1.Type = ConType.ConOut

            With InputConnectors

                If .Count = 2 Then
                    If Me.FlippedH Then
                        .Item(0).Position = New Point(X + Width, Y + 0.5 * Height)
                        .Item(1).Position = New Point(X + 0.5 * Width, Y + Height)
                    Else
                        .Item(0).Position = New Point(X, Y + 0.5 * Height)
                        .Item(1).Position = New Point(X + 0.5 * Width, Y + Height)
                    End If
                ElseIf .Count = 1 Then
                    .Item(0).Position = New Point(X, Y + 0.5 * Height)
                    .Add(myIC2)
                Else
                    .Add(myIC1)
                    .Add(myIC2)
                End If

            End With

            With OutputConnectors

                If .Count <> 0 Then
                    If Me.FlippedH Then
                        .Item(0).Position = New Point(X, Y + 0.1 * Height)
                    Else
                        .Item(0).Position = New Point(X + Width, Y + 0.1 * Height)
                    End If
                Else
                    .Add(myOC1)
                End If

            End With

            Me.EnergyConnector.Position = New Point(X + 0.5 * Width, Y + Height)
            Me.EnergyConnector.Type = ConType.ConEn

        End Sub

        Public Overrides Sub Draw(ByVal g As System.Drawing.Graphics)

            CreateConnectors(0, 0)

            UpdateStatus(Me)

            Dim pt As Point
            Dim raio, angulo As Double
            Dim con As ConnectionPoint
            For Each con In Me.InputConnectors
                pt = con.Position
                raio = ((pt.X - Me.X) ^ 2 + (pt.Y - Me.Y) ^ 2) ^ 0.5
                angulo = Math.Atan2(pt.Y - Me.Y, pt.X - Me.X)
                pt.X = Me.X + raio * Math.Cos(angulo + Me.Rotation / 360 * 2 * Math.PI)
                pt.Y = Me.Y + raio * Math.Sin(angulo + Me.Rotation / 360 * 2 * Math.PI)
                con.Position = pt
            Next
            For Each con In Me.OutputConnectors
                pt = con.Position
                raio = ((pt.X - Me.X) ^ 2 + (pt.Y - Me.Y) ^ 2) ^ 0.5
                angulo = Math.Atan2(pt.Y - Me.Y, pt.X - Me.X)
                pt.X = Me.X + raio * Math.Cos(angulo + Me.Rotation / 360 * 2 * Math.PI)
                pt.Y = Me.Y + raio * Math.Sin(angulo + Me.Rotation / 360 * 2 * Math.PI)
                con.Position = pt
            Next

            Dim gContainer As System.Drawing.Drawing2D.GraphicsContainer
            Dim myMatrix As Drawing2D.Matrix
            gContainer = g.BeginContainer()
            myMatrix = g.Transform()
            If m_Rotation <> 0 Then
                myMatrix.RotateAt(m_Rotation, New PointF(X, Y), Drawing.Drawing2D.MatrixOrder.Append)
                g.Transform = myMatrix
            End If

            Dim rect1 As New RectangleF(X + 0.1 * Width, Y, 0.8 * Width, 0.8 * Height)

            Dim pt3 As New PointF(X + 0.1 * Width, Y + Height)
            Dim pt4 As New PointF(X + 0.2 * Width, Y + 0.65 * Height)

            Dim pt5 As New PointF(X + 0.9 * Width, Y + Height)
            Dim pt6 As New PointF(X + 0.8 * Width, Y + 0.65 * Height)

            Dim pt7 As New PointF(X + 0.1 * Width, Y + Height)
            Dim pt8 As New PointF(X + 0.9 * Width, Y + Height)

            Dim pt9 As New PointF(X + 0.5 * Width, Y)
            Dim pt10 As New PointF(X + Width, Y)
            Dim pt11 As New PointF(X + Width, Y + 0.25 * Height)
            Dim pt12 As New PointF(X + 0.88 * Width, Y + 0.25 * Height)

            If Me.FlippedH Then
                pt3 = New PointF(X + 0.9 * Width, Y + Height)
                pt4 = New PointF(X + 0.8 * Width, Y + 0.65 * Height)

                pt5 = New PointF(X + 0.1 * Width, Y + Height)
                pt6 = New PointF(X + 0.2 * Width, Y + 0.65 * Height)

                pt7 = New PointF(X + 0.9 * Width, Y + Height)
                pt8 = New PointF(X + 0.1 * Width, Y + Height)

                pt9 = New PointF(X + 0.5 * Width, Y)
                pt10 = New PointF(X, Y)
                pt11 = New PointF(X, Y + 0.25 * Height)
                pt12 = New PointF(X + 0.12 * Width, Y + 0.25 * Height)
            End If

            Dim myPen As New Pen(Me.LineColor, Me.LineWidth)
            Dim myPen2 As New Pen(Color.White, 0)
            Dim rect As New Rectangle(X, Y, Width, Height)

            g.SmoothingMode = SmoothingMode.AntiAlias
            'g.DrawRectangle(myPen2, rect)
            g.DrawEllipse(myPen, rect1)
            g.DrawPolygon(myPen, New PointF() {pt3, pt4, pt6, pt5})
            g.DrawPolygon(myPen, New PointF() {pt9, pt10, pt11, pt12})

            Dim strdist As SizeF = g.MeasureString(Me.Tag, New Font("Arial", 10, FontStyle.Bold, GraphicsUnit.Pixel, 0, False), New PointF(0, 0), New StringFormat(StringFormatFlags.NoClip, 0))
            Dim strx As Single = (Me.Width - strdist.Width) / 2
            'g.FillRectangle(Brushes.White, X + strx, Y + CSng(Height + 5), strdist.Width, strdist.Height)
            g.DrawString(Me.Tag, New Font("Arial", 10, FontStyle.Bold, GraphicsUnit.Pixel, 0, False), New SolidBrush(Me.LineColor), X + strx, Y + Height + 5)

            ' Create a path that consists of a single ellipse.
            Dim path As New GraphicsPath()
            path.AddEllipse(rect1)
            ' Use the path to construct a brush.
            Dim pthGrBrush As New PathGradientBrush(path)
            ' Set the color at the center of the path to blue.
            pthGrBrush.CenterColor = Me.GradientColor2

            ' Set the color along the entire boundary 
            ' of the path to aqua.
            Dim colors As Color() = {Me.GradientColor1}
            pthGrBrush.SurroundColors = colors


            If Me.Fill Then
                If Me.GradientMode = False Then
                    g.FillPolygon(New SolidBrush(Me.FillColor), New PointF() {pt3, pt4, pt6, pt5})
                    g.FillPolygon(New SolidBrush(Me.FillColor), New PointF() {pt9, pt10, pt11, pt12})
                    g.FillEllipse(New SolidBrush(Me.FillColor), rect1)
                Else
                    g.FillPolygon(New SolidBrush(Me.GradientColor1), New PointF() {pt3, pt4, pt6, pt5})
                    g.FillPolygon(New SolidBrush(Me.GradientColor1), New PointF() {pt9, pt10, pt11, pt12})
                    g.FillEllipse(pthGrBrush, rect1)
                End If
            End If

            g.EndContainer(gContainer)

        End Sub

    End Class

    <Serializable()> Public Class PipeGraphic
        Inherits ShapeGraphic

#Region "Constructors"
        Public Sub New()
            Me.TipoObjeto = GraphicObjects.TipoObjeto.Pipe
            Me.Description = "Tubulao"
        End Sub

        Public Sub New(ByVal graphicPosition As Point)
            Me.New()
            Me.SetPosition(graphicPosition)
        End Sub

        Public Sub New(ByVal posX As Integer, ByVal posY As Integer)
            Me.New(New Point(posX, posY))
        End Sub

        Public Sub New(ByVal graphicPosition As Point, ByVal graphicSize As Size)
            Me.New(graphicPosition)
            Me.SetSize(graphicSize)
            Me.AutoSize = False
        End Sub

        Public Sub New(ByVal posX As Integer, ByVal posY As Integer, ByVal graphicSize As Size)
            Me.New(New Point(posX, posY), graphicSize)
        End Sub

        Public Sub New(ByVal posX As Integer, ByVal posY As Integer, ByVal width As Integer, ByVal height As Integer)
            Me.New(New Point(posX, posY), New Size(width, height))
        End Sub

        Public Sub New(ByVal graphicPosition As Point, ByVal Rotation As Single)
            Me.New()
            Me.SetPosition(graphicPosition)
            Me.Rotation = Rotation
        End Sub

        Public Sub New(ByVal posX As Integer, ByVal posY As Integer, ByVal Rotation As Single)
            Me.New(New Point(posX, posY), Rotation)
        End Sub

        Public Sub New(ByVal graphicPosition As Point, ByVal graphicSize As Size, ByVal Rotation As Single)
            Me.New(graphicPosition, Rotation)
            Me.SetSize(graphicSize)
            Me.AutoSize = False
        End Sub

        Public Sub New(ByVal posX As Integer, ByVal posY As Integer, ByVal graphicSize As Size, ByVal Rotation As Single)
            Me.New(New Point(posX, posY), graphicSize, Rotation)
        End Sub

        Public Sub New(ByVal posX As Integer, ByVal posY As Integer, ByVal width As Integer, _
                               ByVal height As Integer, ByVal Rotation As Single)
            Me.New(New Point(posX, posY), New Size(width, height), Rotation)
        End Sub

#End Region


        Public Overrides Sub CreateConnectors(InCount As Integer, OutCount As Integer)
            Dim myIC1 As New ConnectionPoint
            myIC1.Position = New Point(X, Y + 0.5 * Height)
            myIC1.Type = ConType.ConIn

            Dim myOC1 As New ConnectionPoint
            myOC1.Position = New Point(X + Width, Y + 0.5 * Height)
            myOC1.Type = ConType.ConOut

            Me.EnergyConnector.Position = New Point(X + 0.5 * Width, Y + Height)
            Me.EnergyConnector.Type = ConType.ConEn

            With InputConnectors

                If .Count <> 0 Then
                    If Me.FlippedH Then
                        .Item(0).Position = New Point(X + Width, Y + 0.5 * Height)
                    Else
                        .Item(0).Position = New Point(X, Y + 0.5 * Height)
                    End If
                Else
                    .Add(myIC1)
                End If

            End With

            With OutputConnectors

                If .Count <> 0 Then
                    If Me.FlippedH Then
                        .Item(0).Position = New Point(X, Y + 0.5 * Height)
                    Else
                        .Item(0).Position = New Point(X + Width, Y + 0.5 * Height)
                    End If
                Else
                    .Add(myOC1)
                End If

            End With


        End Sub

        Public Overrides Sub Draw(ByVal g As System.Drawing.Graphics)

            CreateConnectors(0, 0)

            UpdateStatus(Me)

            Dim pt As Point
            Dim raio, angulo As Double
            Dim con As ConnectionPoint
            For Each con In Me.InputConnectors
                pt = con.Position
                raio = ((pt.X - Me.X) ^ 2 + (pt.Y - Me.Y) ^ 2) ^ 0.5
                angulo = Math.Atan2(pt.Y - Me.Y, pt.X - Me.X)
                pt.X = Me.X + raio * Math.Cos(angulo + Me.Rotation / 360 * 2 * Math.PI)
                pt.Y = Me.Y + raio * Math.Sin(angulo + Me.Rotation / 360 * 2 * Math.PI)
                con.Position = pt
            Next
            For Each con In Me.OutputConnectors
                pt = con.Position
                raio = ((pt.X - Me.X) ^ 2 + (pt.Y - Me.Y) ^ 2) ^ 0.5
                angulo = Math.Atan2(pt.Y - Me.Y, pt.X - Me.X)
                pt.X = Me.X + raio * Math.Cos(angulo + Me.Rotation / 360 * 2 * Math.PI)
                pt.Y = Me.Y + raio * Math.Sin(angulo + Me.Rotation / 360 * 2 * Math.PI)
                con.Position = pt
            Next
            With Me.EnergyConnector
                pt = .Position
                raio = ((pt.X - Me.X) ^ 2 + (pt.Y - Me.Y) ^ 2) ^ 0.5
                angulo = Math.Atan2(pt.Y - Me.Y, pt.X - Me.X)
                pt.X = Me.X + raio * Math.Cos(angulo + Me.Rotation / 360 * 2 * Math.PI)
                pt.Y = Me.Y + raio * Math.Sin(angulo + Me.Rotation / 360 * 2 * Math.PI)
                .Position = pt
            End With

            Dim gContainer As System.Drawing.Drawing2D.GraphicsContainer
            Dim myMatrix As Drawing2D.Matrix
            gContainer = g.BeginContainer()
            myMatrix = g.Transform()
            If m_Rotation <> 0 Then
                myMatrix.RotateAt(m_Rotation, New PointF(X, Y), _
                    Drawing.Drawing2D.MatrixOrder.Append)
                g.Transform = myMatrix
            End If
            Dim rect As New Rectangle(X, Y, Width, Height)
            Dim lgb1 As New LinearGradientBrush(rect, Me.GradientColor1, Me.GradientColor2, LinearGradientMode.Vertical)
            If Me.Fill Then
                If Me.GradientMode = False Then
                    g.FillRectangle(New SolidBrush(Me.FillColor), rect)
                Else
                    g.FillRectangle(lgb1, rect)
                End If
            End If
            Dim myPen As New Pen(Me.LineColor, Me.LineWidth)
            g.SmoothingMode = SmoothingMode.AntiAlias
            g.DrawRectangle(myPen, rect)

            Dim strdist As SizeF = g.MeasureString(Me.Tag, New Font("Arial", 10, FontStyle.Bold, GraphicsUnit.Pixel, 0, False), New PointF(0, 0), New StringFormat(StringFormatFlags.NoClip, 0))
            Dim strx As Single = (Me.Width - strdist.Width) / 2
            'g.FillRectangle(Brushes.White, X + strx, Y + CSng(Height + 5), strdist.Width, strdist.Height)
            g.DrawString(Me.Tag, New Font("Arial", 10, FontStyle.Bold, GraphicsUnit.Pixel, 0, False), New SolidBrush(Me.LineColor), X + strx, Y + Height + 5)

            g.EndContainer(gContainer)
        End Sub

    End Class

    <Serializable()> Public Class TankGraphic
        Inherits ShapeGraphic

#Region "Constructors"
        Public Sub New()
            Me.TipoObjeto = GraphicObjects.TipoObjeto.Tank
            Me.Description = "Tanque"
        End Sub

        Public Sub New(ByVal graphicPosition As Point)
            Me.New()
            Me.SetPosition(graphicPosition)
        End Sub

        Public Sub New(ByVal posX As Integer, ByVal posY As Integer)
            Me.New(New Point(posX, posY))
        End Sub

        Public Sub New(ByVal graphicPosition As Point, ByVal graphicSize As Size)
            Me.New(graphicPosition)
            Me.SetSize(graphicSize)
            Me.AutoSize = False
        End Sub

        Public Sub New(ByVal posX As Integer, ByVal posY As Integer, ByVal graphicSize As Size)
            Me.New(New Point(posX, posY), graphicSize)
        End Sub

        Public Sub New(ByVal posX As Integer, ByVal posY As Integer, ByVal width As Integer, ByVal height As Integer)
            Me.New(New Point(posX, posY), New Size(width, height))
        End Sub

        Public Sub New(ByVal graphicPosition As Point, ByVal Rotation As Single)
            Me.New()
            Me.SetPosition(graphicPosition)
            Me.Rotation = Rotation
        End Sub

        Public Sub New(ByVal posX As Integer, ByVal posY As Integer, ByVal Rotation As Single)
            Me.New(New Point(posX, posY), Rotation)
        End Sub

        Public Sub New(ByVal graphicPosition As Point, ByVal graphicSize As Size, ByVal Rotation As Single)
            Me.New(graphicPosition, Rotation)
            Me.SetSize(graphicSize)
            Me.AutoSize = False
        End Sub

        Public Sub New(ByVal posX As Integer, ByVal posY As Integer, ByVal graphicSize As Size, ByVal Rotation As Single)
            Me.New(New Point(posX, posY), graphicSize, Rotation)
        End Sub

        Public Sub New(ByVal posX As Integer, ByVal posY As Integer, ByVal width As Integer, _
                               ByVal height As Integer, ByVal Rotation As Single)
            Me.New(New Point(posX, posY), New Size(width, height), Rotation)
        End Sub

#End Region

        Public Overrides Sub CreateConnectors(InCount As Integer, OutCount As Integer)
            Dim myIC1 As New ConnectionPoint
            myIC1.Position = New Point(X, Y + 0.5 * Height)
            myIC1.Type = ConType.ConIn

            Dim myOC1 As New ConnectionPoint
            myOC1.Position = New Point(X + Width, Y + 0.5 * Height)
            myOC1.Type = ConType.ConOut

            Me.EnergyConnector.Position = New Point(X + 0.5 * Width, Y + Height)
            Me.EnergyConnector.Type = ConType.ConEn

            With InputConnectors

                If .Count <> 0 Then
                    If Me.FlippedH Then
                        .Item(0).Position = New Point(X + Width, Y + 0.5 * Height)
                    Else
                        .Item(0).Position = New Point(X, Y + 0.5 * Height)
                    End If
                Else
                    .Add(myIC1)
                End If

            End With

            With OutputConnectors

                If .Count <> 0 Then
                    If Me.FlippedH Then
                        .Item(0).Position = New Point(X, Y + 0.5 * Height)
                    Else
                        .Item(0).Position = New Point(X + Width, Y + 0.5 * Height)
                    End If
                Else
                    .Add(myOC1)
                End If

            End With

        End Sub

        Public Overrides Sub Draw(ByVal g As System.Drawing.Graphics)

            CreateConnectors(0, 0)

            UpdateStatus(Me)

            Dim pt As Point
            Dim raio, angulo As Double
            Dim con As ConnectionPoint
            For Each con In Me.InputConnectors
                pt = con.Position
                raio = ((pt.X - Me.X) ^ 2 + (pt.Y - Me.Y) ^ 2) ^ 0.5
                angulo = Math.Atan2(pt.Y - Me.Y, pt.X - Me.X)
                pt.X = Me.X + raio * Math.Cos(angulo + Me.Rotation / 360 * 2 * Math.PI)
                pt.Y = Me.Y + raio * Math.Sin(angulo + Me.Rotation / 360 * 2 * Math.PI)
                con.Position = pt
            Next
            For Each con In Me.OutputConnectors
                pt = con.Position
                raio = ((pt.X - Me.X) ^ 2 + (pt.Y - Me.Y) ^ 2) ^ 0.5
                angulo = Math.Atan2(pt.Y - Me.Y, pt.X - Me.X)
                pt.X = Me.X + raio * Math.Cos(angulo + Me.Rotation / 360 * 2 * Math.PI)
                pt.Y = Me.Y + raio * Math.Sin(angulo + Me.Rotation / 360 * 2 * Math.PI)
                con.Position = pt
            Next
            With Me.EnergyConnector
                pt = .Position
                raio = ((pt.X - Me.X) ^ 2 + (pt.Y - Me.Y) ^ 2) ^ 0.5
                angulo = Math.Atan2(pt.Y - Me.Y, pt.X - Me.X)
                pt.X = Me.X + raio * Math.Cos(angulo + Me.Rotation / 360 * 2 * Math.PI)
                pt.Y = Me.Y + raio * Math.Sin(angulo + Me.Rotation / 360 * 2 * Math.PI)
                .Position = pt
            End With

            Dim gContainer As System.Drawing.Drawing2D.GraphicsContainer
            Dim myMatrix As Drawing2D.Matrix
            gContainer = g.BeginContainer()
            myMatrix = g.Transform()
            If m_Rotation <> 0 Then
                myMatrix.RotateAt(m_Rotation, New PointF(X, Y), Drawing.Drawing2D.MatrixOrder.Append)
                g.Transform = myMatrix
            End If

            Dim rect1 As New Rectangle(X + 0.1 * Width, Y + 0.1 * Height, 0.8 * Width, 0.8 * Height)
            Dim rect2 As New RectangleF(X + 0.1 * Width, Y, 0.8 * Width, 0.2 * Height)
            Dim rect3 As New RectangleF(X + 0.1 * Width, Y + 0.8 * Height, 0.8 * Width, 0.2 * Height)

            Dim myPen As New Pen(Me.LineColor, Me.LineWidth)

            Dim myPen2 As New Pen(Color.White, 0)

            Dim rect As New Rectangle(X, Y, Width, Height)

            g.SmoothingMode = SmoothingMode.AntiAlias
            'g.DrawRectangle(myPen2, rect)
            g.DrawEllipse(myPen, rect2)
            g.DrawEllipse(myPen, rect3)
            g.DrawRectangle(myPen, rect1)

            Dim strdist As SizeF = g.MeasureString(Me.Tag, New Font("Arial", 10, FontStyle.Bold, GraphicsUnit.Pixel, 0, False), New PointF(0, 0), New StringFormat(StringFormatFlags.NoClip, 0))
            Dim strx As Single = (Me.Width - strdist.Width) / 2
            'g.FillRectangle(Brushes.White, X + strx, Y + CSng(Height + 5), strdist.Width, strdist.Height)
            g.DrawString(Me.Tag, New Font("Arial", 10, FontStyle.Bold, GraphicsUnit.Pixel, 0, False), New SolidBrush(Me.LineColor), X + strx, Y + Height + 5)

            Dim lgb1 As New LinearGradientBrush(rect1, Me.GradientColor1, Me.GradientColor2, LinearGradientMode.Horizontal)
            lgb1.SetBlendTriangularShape(0.5)
            Dim lgb2 As New LinearGradientBrush(rect3, Me.GradientColor1, Me.GradientColor2, LinearGradientMode.Horizontal)
            lgb2.SetBlendTriangularShape(0.5)

            If Me.Fill Then
                If Me.GradientMode = True Then
                    g.FillRectangle(lgb1, rect1)
                    g.FillEllipse(New SolidBrush(Me.FillColor), rect2)
                    g.FillEllipse(lgb2, rect3)
                Else
                    g.FillRectangle(New SolidBrush(Me.FillColor), rect1)
                    g.FillEllipse(New SolidBrush(Me.FillColor), rect2)
                    g.FillEllipse(New SolidBrush(Me.FillColor), rect3)
                End If
            End If

            g.EndContainer(gContainer)

        End Sub

    End Class

    <Serializable()> Public Class ConnectorGraphic
        Inherits ShapeGraphic

        Protected m_AttFrom As GraphicObject = Nothing
        Protected m_AttTo As GraphicObject = Nothing
        Protected m_AttFromIndex As Integer
        Protected m_AttToIndex As Integer

        Protected m_AttFromEn As Boolean = False
        Protected m_AttToEn As Boolean = False

#Region "Constructors"
        Public Sub New()
        End Sub

        Public Sub New(ByVal startPosition As Point)
            Me.New()
            Me.SetStartPosition(startPosition)
            Me.IsConnector = True
        End Sub

        Public Sub New(ByVal posX As Integer, ByVal posY As Integer)
            Me.New(New Point(posX, posY))
            Me.IsConnector = True
        End Sub

        Public Sub New(ByVal startPosition As Point, ByVal endPosition As Point)
            Me.New(startPosition)
            Me.SetEndPosition(endPosition)
            Me.AutoSize = False
            Me.IsConnector = True
        End Sub

        Public Sub New(ByVal startX As Integer, ByVal startY As Integer, ByVal endPosition As Point)
            Me.New(New Point(startX, startY), endPosition)
            Me.IsConnector = True
        End Sub

        Public Sub New(ByVal startX As Integer, ByVal startY As Integer, ByVal endX As Integer, ByVal endY As Integer)
            Me.New(New Point(startX, startY), New Point(endX, endY))
            Me.IsConnector = True
        End Sub

        Public Sub New(ByVal startPosition As Point, ByVal endPosition As Point, ByVal lineWidth As Single, ByVal lineColor As Color)
            Me.New(startPosition)
            Me.SetEndPosition(endPosition)
            Me.LineWidth = lineWidth
            Me.LineColor = lineColor
            Me.AutoSize = False
            Me.IsConnector = True
        End Sub

        Public Sub New(ByVal startX As Integer, ByVal startY As Integer, ByVal endX As Integer, ByVal endY As Integer, ByVal lineWidth As Single, ByVal lineColor As Color)
            Me.New(New Point(startX, startY), New Point(endX, endY))
            Me.LineWidth = lineWidth
            Me.LineColor = lineColor
            Me.AutoSize = False
            Me.IsConnector = True
        End Sub

#End Region

        Public Overloads Overrides Function HitTest(ByVal pt As System.Drawing.Point) As Boolean
            Dim gp As New Drawing2D.GraphicsPath()
            Dim myMatrix As New Drawing2D.Matrix()
            Dim myPen As New Pen(Me.LineColor, Me.LineWidth)
            gp.AddLine(X, Y, X + Width, Y + Height)
            myMatrix.RotateAt(Me.Rotation, New PointF(Me.X, Me.Y), Drawing.Drawing2D.MatrixOrder.Append)
            gp.Transform(myMatrix)
            Return gp.IsOutlineVisible(pt, myPen)
        End Function

        Public Function GetStartPosition() As Point
            Return Me.GetPosition()
        End Function

        Public Sub SetStartPosition(ByVal Value As Point)
            Me.SetPosition(Value)
        End Sub

        Public Function GetEndPosition() As Point
            Dim endPosition As New Point(Me.m_Position.X, Me.m_Position.Y)
            endPosition.X += Me.m_Size.Width
            endPosition.Y += Me.m_Size.Height
            Return endPosition
        End Function

        Public Sub SetEndPosition(ByVal Value As Point)
            m_Size.Width = Value.X - Me.m_Position.X
            m_Size.Height = Value.Y - Me.m_Position.Y
        End Sub

        Public Property AttachedFromConnectorIndex() As Integer
            Get
                Return m_AttFromIndex
            End Get
            Set(ByVal index As Integer)
                m_AttFromIndex = index
            End Set
        End Property

        Public Property AttachedToConnectorIndex() As Integer
            Get
                Return m_AttToIndex
            End Get
            Set(ByVal index As Integer)
                m_AttToIndex = index
            End Set
        End Property

        Public Property AttachedToEnergy() As Boolean
            Get
                Return m_AttToEn
            End Get
            Set(ByVal value As Boolean)
                m_AttToEn = value
            End Set
        End Property

        Public Property AttachedFromEnergy() As Boolean
            Get
                Return m_AttFromEn
            End Get
            Set(ByVal value As Boolean)
                m_AttFromEn = value
            End Set
        End Property

        Public Property AttachedFrom() As GraphicObject
            Get
                Return m_AttFrom
            End Get
            Set(ByVal gObj As GraphicObject)
                m_AttFrom = gObj
            End Set
        End Property

        Public Property AttachedTo() As GraphicObject
            Get
                Return m_AttTo
            End Get
            Set(ByVal gObj As GraphicObject)
                m_AttTo = gObj
            End Set
        End Property

        Public Overrides Sub Draw(ByVal g As System.Drawing.Graphics)
            Dim gContainer As System.Drawing.Drawing2D.GraphicsContainer
            Dim myMatrix As Drawing2D.Matrix
            myMatrix = g.Transform()
            gContainer = g.BeginContainer()
            If m_Rotation <> 0 Then
                myMatrix.RotateAt(m_Rotation, New PointF(X, Y), Drawing.Drawing2D.MatrixOrder.Append)
                g.Transform = myMatrix
            End If
            g.SmoothingMode = SmoothingMode.AntiAlias
            Me.LineColor = Color.FromArgb(255, 100, 150, 200)
            Me.LineWidth = 1

            'posicionar pontos nos primeiros slots livres
            Dim StartPos, EndPos As New Point
            If Me.AttachedFrom.TipoObjeto = GraphicObjects.TipoObjeto.EnergyStream Then
                If Me.AttachedTo.TipoObjeto = GraphicObjects.TipoObjeto.CustomUO Or _
                Me.AttachedTo.TipoObjeto = GraphicObjects.TipoObjeto.ShortcutColumn Or _
                Me.AttachedTo.TipoObjeto = GraphicObjects.TipoObjeto.OT_EnergyRecycle Or _
                Me.AttachedTo.TipoObjeto = GraphicObjects.TipoObjeto.CapeOpenUO Or _
                Me.AttachedTo.TipoObjeto = GraphicObjects.TipoObjeto.Vessel Then
                    StartPos = Me.AttachedFrom.OutputConnectors(0).Position
                    EndPos = Me.AttachedTo.InputConnectors(Me.AttachedToConnectorIndex).Position
                Else
                    StartPos = Me.AttachedFrom.OutputConnectors(0).Position
                    EndPos = Me.AttachedTo.EnergyConnector.Position
                End If
            ElseIf Me.AttachedFromConnectorIndex = -1 Then
                StartPos = Me.AttachedFrom.EnergyConnector.Position
                EndPos = Me.AttachedTo.InputConnectors(Me.AttachedToConnectorIndex).Position
            Else
                StartPos = Me.AttachedFrom.OutputConnectors(Me.AttachedFromConnectorIndex).Position
                EndPos = Me.AttachedTo.InputConnectors(Me.AttachedToConnectorIndex).Position
            End If

            'StartPos = New Point(Me.X, Me.Y)
            'EndPos = New Point(Me.X + Me.Width, Me.Y + Me.Height)

            Dim myPen As New Pen(Me.LineColor, Me.LineWidth)
            With myPen
                .EndCap = Drawing2D.LineCap.ArrowAnchor
                .StartCap = Drawing2D.LineCap.NoAnchor
                .LineJoin = LineJoin.Round
            End With

            UpdateStatusC(myPen, Me, StartPos, EndPos)

            Dim delta As Integer = 0
            Dim delta2 As Integer = 0
            delta = 15
            delta2 = 30

            Dim p1, p2, p3, p4, p5, p6 As Point

            If Not Me.AttachedFrom.FlippedH And Not Me.AttachedTo.FlippedH Then

                If StartPos.Y < EndPos.Y And StartPos.X < EndPos.X Then
                    p1 = New Point(StartPos.X, StartPos.Y)
                    p2 = New Point(StartPos.X + delta, StartPos.Y)
                    p3 = New Point(StartPos.X + delta, EndPos.Y)
                    p4 = New Point(EndPos.X, EndPos.Y)
                    g.DrawPath(myPen, Me.GetRoundedLine(New PointF() {p1, p2, p3, p4}, 4))
                ElseIf StartPos.Y < EndPos.Y And StartPos.X >= EndPos.X Then
                    p1 = New Point(StartPos.X, StartPos.Y)
                    p2 = New Point(StartPos.X + delta, StartPos.Y)
                    p3 = New Point(StartPos.X + delta, StartPos.Y + delta2)
                    p4 = New Point(EndPos.X - delta, StartPos.Y + delta2)
                    p5 = New Point(EndPos.X - delta, EndPos.Y)
                    p6 = New Point(EndPos.X, EndPos.Y)
                    g.DrawPath(myPen, Me.GetRoundedLine(New PointF() {p1, p2, p3, p4, p5, p6}, 4))
                ElseIf StartPos.Y >= EndPos.Y And StartPos.X < EndPos.X Then
                    p1 = New Point(StartPos.X, StartPos.Y)
                    p2 = New Point(StartPos.X + delta, StartPos.Y)
                    p3 = New Point(StartPos.X + delta, EndPos.Y)
                    p4 = New Point(EndPos.X, EndPos.Y)
                    g.DrawPath(myPen, Me.GetRoundedLine(New PointF() {p1, p2, p3, p4}, 4))
                ElseIf StartPos.Y >= EndPos.Y And StartPos.X >= EndPos.X Then
                    p1 = New Point(StartPos.X, StartPos.Y)
                    p2 = New Point(StartPos.X + delta, StartPos.Y)
                    p3 = New Point(StartPos.X + delta, StartPos.Y - delta2)
                    p4 = New Point(EndPos.X - delta, StartPos.Y - delta2)
                    p5 = New Point(EndPos.X - delta, EndPos.Y)
                    p6 = New Point(EndPos.X, EndPos.Y)
                    g.DrawPath(myPen, Me.GetRoundedLine(New PointF() {p1, p2, p3, p4, p5, p6}, 4))
                End If

            ElseIf Not Me.AttachedFrom.FlippedH And Me.AttachedTo.FlippedH Then

                If StartPos.Y < EndPos.Y And StartPos.X < EndPos.X Then
                    p1 = New Point(StartPos.X, StartPos.Y)
                    p2 = New Point(EndPos.X + delta, StartPos.Y)
                    p3 = New Point(EndPos.X + delta, EndPos.Y)
                    p4 = New Point(EndPos.X, EndPos.Y)
                    g.DrawPath(myPen, Me.GetRoundedLine(New PointF() {p1, p2, p3, p4}, 4))
                ElseIf StartPos.Y < EndPos.Y And StartPos.X >= EndPos.X Then
                    p1 = New Point(StartPos.X, StartPos.Y)
                    p2 = New Point(StartPos.X + delta, StartPos.Y)
                    p3 = New Point(StartPos.X + delta, EndPos.Y)
                    p4 = New Point(EndPos.X, EndPos.Y)
                    g.DrawPath(myPen, Me.GetRoundedLine(New PointF() {p1, p2, p3, p4}, 4))
                ElseIf StartPos.Y >= EndPos.Y And StartPos.X < EndPos.X Then
                    p1 = New Point(StartPos.X, StartPos.Y)
                    p2 = New Point(EndPos.X + delta, StartPos.Y)
                    p3 = New Point(EndPos.X + delta, EndPos.Y)
                    p4 = New Point(EndPos.X, EndPos.Y)
                    g.DrawPath(myPen, Me.GetRoundedLine(New PointF() {p1, p2, p3, p4}, 4))
                ElseIf StartPos.Y >= EndPos.Y And StartPos.X >= EndPos.X Then
                    p1 = New Point(StartPos.X, StartPos.Y)
                    p2 = New Point(StartPos.X + delta, StartPos.Y)
                    p3 = New Point(StartPos.X + delta, EndPos.Y)
                    p4 = New Point(EndPos.X, EndPos.Y)
                    g.DrawPath(myPen, Me.GetRoundedLine(New PointF() {p1, p2, p3, p4}, 4))
                End If

            ElseIf Me.AttachedFrom.FlippedH And Not Me.AttachedTo.FlippedH Then

                If StartPos.Y < EndPos.Y And StartPos.X < EndPos.X Then
                    p1 = New Point(StartPos.X, StartPos.Y)
                    p2 = New Point(StartPos.X - delta, StartPos.Y)
                    p3 = New Point(StartPos.X - delta, EndPos.Y)
                    p4 = New Point(EndPos.X, EndPos.Y)
                    g.DrawPath(myPen, Me.GetRoundedLine(New PointF() {p1, p2, p3, p4}, 4))
                ElseIf StartPos.Y < EndPos.Y And StartPos.X >= EndPos.X Then
                    p1 = New Point(StartPos.X, StartPos.Y)
                    p2 = New Point(EndPos.X - delta, StartPos.Y)
                    p3 = New Point(EndPos.X - delta, EndPos.Y)
                    p4 = New Point(EndPos.X, EndPos.Y)
                    g.DrawPath(myPen, Me.GetRoundedLine(New PointF() {p1, p2, p3, p4}, 4))
                ElseIf StartPos.Y >= EndPos.Y And StartPos.X < EndPos.X Then
                    p1 = New Point(StartPos.X, StartPos.Y)
                    p2 = New Point(StartPos.X - delta, StartPos.Y)
                    p3 = New Point(StartPos.X - delta, EndPos.Y)
                    p4 = New Point(EndPos.X, EndPos.Y)
                    g.DrawPath(myPen, Me.GetRoundedLine(New PointF() {p1, p2, p3, p4}, 4))
                ElseIf StartPos.Y >= EndPos.Y And StartPos.X >= EndPos.X Then
                    p1 = New Point(StartPos.X, StartPos.Y)
                    p2 = New Point(EndPos.X - delta, StartPos.Y)
                    p3 = New Point(EndPos.X - delta, EndPos.Y)
                    p4 = New Point(EndPos.X, EndPos.Y)
                    g.DrawPath(myPen, Me.GetRoundedLine(New PointF() {p1, p2, p3, p4}, 4))
                End If

            ElseIf Me.AttachedFrom.FlippedH And Me.AttachedTo.FlippedH Then

                If StartPos.Y < EndPos.Y And StartPos.X < EndPos.X Then
                    p1 = New Point(StartPos.X, StartPos.Y)
                    p2 = New Point(StartPos.X - delta, StartPos.Y)
                    p3 = New Point(StartPos.X - delta, StartPos.Y - delta2)
                    p4 = New Point(EndPos.X + delta, StartPos.Y - delta2)
                    p5 = New Point(EndPos.X + delta, EndPos.Y)
                    p6 = New Point(EndPos.X, EndPos.Y)
                    g.DrawPath(myPen, Me.GetRoundedLine(New PointF() {p1, p2, p3, p4, p5, p6}, 4))
                ElseIf StartPos.Y < EndPos.Y And StartPos.X >= EndPos.X Then
                    p1 = New Point(StartPos.X, StartPos.Y)
                    p2 = New Point(StartPos.X - delta, StartPos.Y)
                    p3 = New Point(StartPos.X - delta, EndPos.Y)
                    p4 = New Point(EndPos.X, EndPos.Y)
                    g.DrawPath(myPen, Me.GetRoundedLine(New PointF() {p1, p2, p3, p4}, 4))
                ElseIf StartPos.Y >= EndPos.Y And StartPos.X < EndPos.X Then
                    p1 = New Point(StartPos.X, StartPos.Y)
                    p2 = New Point(StartPos.X - delta, StartPos.Y)
                    p3 = New Point(StartPos.X - delta, StartPos.Y + delta2)
                    p4 = New Point(EndPos.X + delta, StartPos.Y + delta2)
                    p5 = New Point(EndPos.X + delta, EndPos.Y)
                    p6 = New Point(EndPos.X, EndPos.Y)
                    g.DrawPath(myPen, Me.GetRoundedLine(New PointF() {p1, p2, p3, p4, p5, p6}, 4))
                ElseIf StartPos.Y >= EndPos.Y And StartPos.X >= EndPos.X Then
                    p1 = New Point(StartPos.X, StartPos.Y)
                    p2 = New Point(StartPos.X - delta, StartPos.Y)
                    p3 = New Point(StartPos.X - delta, EndPos.Y)
                    p4 = New Point(EndPos.X, EndPos.Y)
                    g.DrawPath(myPen, Me.GetRoundedLine(New PointF() {p1, p2, p3, p4}, 4))
                End If

            End If

            g.EndContainer(gContainer)

        End Sub

        Public Overrides Function SaveData() As System.Collections.Generic.List(Of System.Xml.Linq.XElement)
            Return New System.Collections.Generic.List(Of System.Xml.Linq.XElement)
        End Function

        Public Overrides Function LoadData(data As System.Collections.Generic.List(Of System.Xml.Linq.XElement)) As Boolean
            Return True
        End Function

    End Class

    <Serializable()> Public Class NodeInGraphic
        Inherits ShapeGraphic

#Region "Constructors"
        Public Sub New()
            Me.TipoObjeto = GraphicObjects.TipoObjeto.NodeIn
            Me.Description = "Misturador"
            Me.m_mixpoint = True
        End Sub

        Public Sub New(ByVal graphicPosition As Point)
            Me.New()
            Me.SetPosition(graphicPosition)
        End Sub

        Public Sub New(ByVal posX As Integer, ByVal posY As Integer)
            Me.New(New Point(posX, posY))
        End Sub

        Public Sub New(ByVal graphicPosition As Point, ByVal graphicSize As Size)
            Me.New(graphicPosition)
            Me.SetSize(graphicSize)
            Me.AutoSize = False
        End Sub

        Public Sub New(ByVal posX As Integer, ByVal posY As Integer, ByVal graphicSize As Size)
            Me.New(New Point(posX, posY), graphicSize)
        End Sub

        Public Sub New(ByVal posX As Integer, ByVal posY As Integer, ByVal width As Integer, ByVal height As Integer)
            Me.New(New Point(posX, posY), New Size(width, height))
        End Sub

        Public Sub New(ByVal graphicPosition As Point, ByVal Rotation As Single)
            Me.New()
            Me.SetPosition(graphicPosition)
            Me.Rotation = Rotation
        End Sub

        Public Sub New(ByVal posX As Integer, ByVal posY As Integer, ByVal Rotation As Single)
            Me.New(New Point(posX, posY), Rotation)
        End Sub

        Public Sub New(ByVal graphicPosition As Point, ByVal graphicSize As Size, ByVal Rotation As Single)
            Me.New(graphicPosition, Rotation)
            Me.SetSize(graphicSize)
            Me.AutoSize = False
        End Sub

        Public Sub New(ByVal posX As Integer, ByVal posY As Integer, ByVal graphicSize As Size, ByVal Rotation As Single)
            Me.New(New Point(posX, posY), graphicSize, Rotation)
        End Sub

        Public Sub New(ByVal posX As Integer, ByVal posY As Integer, ByVal width As Integer, _
                               ByVal height As Integer, ByVal Rotation As Single)
            Me.New(New Point(posX, posY), New Size(width, height), Rotation)
        End Sub

#End Region

        Public Overrides Sub CreateConnectors(InCount As Integer, OutCount As Integer)
            Dim myIC1 As New ConnectionPoint
            myIC1.Position = New Point(X, Y + 0.0 * Height)
            myIC1.Type = ConType.ConIn

            Dim myIC2 As New ConnectionPoint
            myIC2.Position = New Point(X, Y + 0.2 * Height)
            myIC2.Type = ConType.ConIn

            Dim myIC3 As New ConnectionPoint
            myIC3.Position = New Point(X, Y + 0.4 * Height)
            myIC3.Type = ConType.ConIn

            Dim myIC4 As New ConnectionPoint
            myIC4.Position = New Point(X, Y + 0.6 * Height)
            myIC4.Type = ConType.ConIn

            Dim myIC5 As New ConnectionPoint
            myIC5.Position = New Point(X, Y + 0.8 * Height)
            myIC5.Type = ConType.ConIn

            Dim myIC6 As New ConnectionPoint
            myIC6.Position = New Point(X, Y + 1.0 * Height)
            myIC6.Type = ConType.ConIn

            Dim myOC1 As New ConnectionPoint
            myOC1.Position = New Point(X + Width, Y + 0.5 * Height)
            myOC1.Type = ConType.ConOut

            With InputConnectors

                If .Count <> 0 Then
                    If .Count = 3 Then
                        .Add(myIC4)
                        .Add(myIC5)
                        .Add(myIC6)
                    End If
                    If Me.FlippedH Then
                        .Item(0).Position = New Point(X + Width, Y)
                        .Item(1).Position = New Point(X + Width, Y + 0.2 * Height)
                        .Item(2).Position = New Point(X + Width, Y + 0.4 * Height)
                        .Item(3).Position = New Point(X + Width, Y + 0.6 * Height)
                        .Item(4).Position = New Point(X + Width, Y + 0.8 * Height)
                        .Item(5).Position = New Point(X + Width, Y + 1.0 * Height)
                    Else
                        .Item(0).Position = New Point(X, Y)
                        .Item(1).Position = New Point(X, Y + 0.2 * Height)
                        .Item(2).Position = New Point(X, Y + 0.4 * Height)
                        .Item(3).Position = New Point(X, Y + 0.6 * Height)
                        .Item(4).Position = New Point(X, Y + 0.8 * Height)
                        .Item(5).Position = New Point(X, Y + 1.0 * Height)
                    End If
                Else
                    .Add(myIC1)
                    .Add(myIC2)
                    .Add(myIC3)
                    .Add(myIC4)
                    .Add(myIC5)
                    .Add(myIC6)
                End If

            End With

            With OutputConnectors

                If .Count <> 0 Then
                    If Me.FlippedH Then
                        .Item(0).Position = New Point(X, Y + 0.5 * Height)
                    Else
                        .Item(0).Position = New Point(X + Width, Y + 0.5 * Height)
                    End If
                Else
                    .Add(myOC1)
                End If

            End With
        End Sub

        Public Overrides Sub Draw(ByVal g As Graphics)

            CreateConnectors(0, 0)

            UpdateStatus(Me)

            Dim pt As Point
            Dim raio, angulo As Double
            Dim con As ConnectionPoint
            For Each con In Me.InputConnectors
                pt = con.Position
                raio = ((pt.X - Me.X) ^ 2 + (pt.Y - Me.Y) ^ 2) ^ 0.5
                angulo = Math.Atan2(pt.Y - Me.Y, pt.X - Me.X)
                pt.X = Me.X + raio * Math.Cos(angulo + Me.Rotation / 360 * 2 * Math.PI)
                pt.Y = Me.Y + raio * Math.Sin(angulo + Me.Rotation / 360 * 2 * Math.PI)
                con.Position = pt
            Next
            For Each con In Me.OutputConnectors
                pt = con.Position
                raio = ((pt.X - Me.X) ^ 2 + (pt.Y - Me.Y) ^ 2) ^ 0.5
                angulo = Math.Atan2(pt.Y - Me.Y, pt.X - Me.X)
                pt.X = Me.X + raio * Math.Cos(angulo + Me.Rotation / 360 * 2 * Math.PI)
                pt.Y = Me.Y + raio * Math.Sin(angulo + Me.Rotation / 360 * 2 * Math.PI)
                con.Position = pt
            Next

            Dim gContainer As Drawing2D.GraphicsContainer
            Dim myMatrix As Drawing2D.Matrix
            gContainer = g.BeginContainer()
            myMatrix = g.Transform()
            If m_Rotation <> 0 Then
                myMatrix.RotateAt(m_Rotation, New PointF(X, Y), _
                    Drawing2D.MatrixOrder.Append)
                g.Transform = myMatrix
            End If

            Dim myPenE As New Pen(Me.LineColor, Me.LineWidth)
            Dim myPen2 As New Pen(Color.White, 0)
            Dim rect As New Rectangle(X, Y, Width, Height)

            Dim gp As Drawing2D.GraphicsPath = New Drawing2D.GraphicsPath
            If Me.FlippedH Then
                gp.AddLine(CInt(X), CInt(Y + 0.5 * Height), CInt(X + 0.5 * Width), CInt(Y))
                gp.AddLine(CInt(X + 0.5 * Width), CInt(Y), CInt(X + Width), CInt(Y))
                gp.AddLine(CInt(X + Width), CInt(Y), CInt(X + Width), CInt(Y + Height))
                gp.AddLine(CInt(X + Width), CInt(Y + Height), CInt(X + 0.5 * Width), CInt(Y + Height))
                gp.AddLine(CInt(X + 0.5 * Width), CInt(Y + Height), CInt(X), CInt(Y + 0.5 * Height))
            Else
                gp.AddLine(CInt(X + Width), CInt(Y + 0.5 * Height), CInt(X + 0.5 * Width), CInt(Y))
                gp.AddLine(CInt(X + 0.5 * Width), CInt(Y), CInt(X), CInt(Y))
                gp.AddLine(CInt(X), CInt(Y), CInt(X), CInt(Y + Height))
                gp.AddLine(CInt(X), CInt(Y + Height), CInt(X + 0.5 * Width), CInt(Y + Height))
                gp.AddLine(CInt(X + 0.5 * Width), CInt(Y + Height), CInt(X + Width), CInt(Y + 0.5 * Height))
            End If

            gp.CloseFigure()

            g.SmoothingMode = SmoothingMode.AntiAlias
            g.DrawPath(myPenE, Me.GetRoundedLine(gp.PathPoints, 1))

            g.SmoothingMode = SmoothingMode.AntiAlias

            Dim strdist As SizeF = g.MeasureString(Me.Tag, New Font("Arial", 10, FontStyle.Bold, GraphicsUnit.Pixel, 0, False), New PointF(0, 0), New StringFormat(StringFormatFlags.NoClip, 0))
            Dim strx As Single = (Me.Width - strdist.Width) / 2
            'g.FillRectangle(Brushes.White, X + strx, Y + CSng(Height + 5), strdist.Width, strdist.Height)
            g.DrawString(Me.Tag, New Font("Arial", 10, FontStyle.Bold, GraphicsUnit.Pixel, 0, False), New SolidBrush(Me.LineColor), X + strx, Y + Height + 5)

            Dim pgb1 As New PathGradientBrush(gp)
            pgb1.CenterColor = Me.GradientColor2
            'lgb1.SetBlendTriangularShape(0.5)
            pgb1.SurroundColors = New Color() {Me.GradientColor1}

            If Me.Fill Then
                If Me.GradientMode = False Then
                    g.FillPath(New SolidBrush(Me.FillColor), gp)
                Else
                    g.FillPath(pgb1, gp)
                End If
            End If

            g.EndContainer(gContainer)
        End Sub

    End Class

    <Serializable()> Public Class NodeOutGraphic
        Inherits ShapeGraphic

#Region "Constructors"
        Public Sub New()
            Me.TipoObjeto = GraphicObjects.TipoObjeto.NodeOut
            Me.Description = "Divisor"
            Me.m_splitpoint = True
        End Sub

        Public Sub New(ByVal graphicPosition As Point)
            Me.New()
            Me.SetPosition(graphicPosition)
        End Sub

        Public Sub New(ByVal posX As Integer, ByVal posY As Integer)
            Me.New(New Point(posX, posY))
        End Sub

        Public Sub New(ByVal graphicPosition As Point, ByVal graphicSize As Size)
            Me.New(graphicPosition)
            Me.SetSize(graphicSize)
            Me.AutoSize = False
        End Sub

        Public Sub New(ByVal posX As Integer, ByVal posY As Integer, ByVal graphicSize As Size)
            Me.New(New Point(posX, posY), graphicSize)
        End Sub

        Public Sub New(ByVal posX As Integer, ByVal posY As Integer, ByVal width As Integer, ByVal height As Integer)
            Me.New(New Point(posX, posY), New Size(width, height))
        End Sub

        Public Sub New(ByVal graphicPosition As Point, ByVal Rotation As Single)
            Me.New()
            Me.SetPosition(graphicPosition)
            Me.Rotation = Rotation
        End Sub

        Public Sub New(ByVal posX As Integer, ByVal posY As Integer, ByVal Rotation As Single)
            Me.New(New Point(posX, posY), Rotation)
        End Sub

        Public Sub New(ByVal graphicPosition As Point, ByVal graphicSize As Size, ByVal Rotation As Single)
            Me.New(graphicPosition, Rotation)
            Me.SetSize(graphicSize)
            Me.AutoSize = False
        End Sub

        Public Sub New(ByVal posX As Integer, ByVal posY As Integer, ByVal graphicSize As Size, ByVal Rotation As Single)
            Me.New(New Point(posX, posY), graphicSize, Rotation)
        End Sub

        Public Sub New(ByVal posX As Integer, ByVal posY As Integer, ByVal width As Integer, _
                               ByVal height As Integer, ByVal Rotation As Single)
            Me.New(New Point(posX, posY), New Size(width, height), Rotation)
        End Sub

#End Region

        Public Overrides Sub CreateConnectors(InCount As Integer, OutCount As Integer)

            Dim myIC1 As New ConnectionPoint

            myIC1.Position = New Point(X, Y + 0.5 * Height)
            myIC1.Type = ConType.ConIn

            Dim myOC1 As New ConnectionPoint
            myOC1.Position = New Point(X + Width, Y)
            myOC1.Type = ConType.ConOut

            Dim myOC2 As New ConnectionPoint
            myOC2.Position = New Point(X + Width, Y + 0.5 * Height)
            myOC2.Type = ConType.ConOut

            Dim myOC3 As New ConnectionPoint
            myOC3.Position = New Point(X + Width, Y + Height)
            myOC3.Type = ConType.ConOut

            With InputConnectors

                If .Count <> 0 Then
                    If Me.FlippedH Then
                        .Item(0).Position = New Point(X + Width, Y + 0.5 * Height)
                    Else
                        .Item(0).Position = New Point(X, Y + 0.5 * Height)
                    End If
                Else
                    .Add(myIC1)
                End If

            End With

            With OutputConnectors

                If .Count <> 0 Then
                    If Me.FlippedH Then
                        .Item(0).Position = New Point(X, Y)
                        .Item(1).Position = New Point(X, Y + 0.5 * Height)
                        .Item(2).Position = New Point(X, Y + Height)
                    Else
                        .Item(0).Position = New Point(X + Width, Y)
                        .Item(1).Position = New Point(X + Width, Y + 0.5 * Height)
                        .Item(2).Position = New Point(X + Width, Y + Height)
                    End If
                Else
                    .Add(myOC1)
                    .Add(myOC2)
                    .Add(myOC3)
                End If

            End With
        End Sub

        Public Overrides Sub Draw(ByVal g As Graphics)

            CreateConnectors(0, 0)

            UpdateStatus(Me)

            Dim pt As Point
            Dim raio, angulo As Double
            Dim con As ConnectionPoint
            For Each con In Me.InputConnectors
                pt = con.Position
                raio = ((pt.X - Me.X) ^ 2 + (pt.Y - Me.Y) ^ 2) ^ 0.5
                angulo = Math.Atan2(pt.Y - Me.Y, pt.X - Me.X)
                pt.X = Me.X + raio * Math.Cos(angulo + Me.Rotation / 360 * 2 * Math.PI)
                pt.Y = Me.Y + raio * Math.Sin(angulo + Me.Rotation / 360 * 2 * Math.PI)
                con.Position = pt
            Next
            For Each con In Me.OutputConnectors
                pt = con.Position
                raio = ((pt.X - Me.X) ^ 2 + (pt.Y - Me.Y) ^ 2) ^ 0.5
                angulo = Math.Atan2(pt.Y - Me.Y, pt.X - Me.X)
                pt.X = Me.X + raio * Math.Cos(angulo + Me.Rotation / 360 * 2 * Math.PI)
                pt.Y = Me.Y + raio * Math.Sin(angulo + Me.Rotation / 360 * 2 * Math.PI)
                con.Position = pt
            Next

            Dim gContainer As Drawing2D.GraphicsContainer
            Dim myMatrix As Drawing2D.Matrix
            gContainer = g.BeginContainer()
            myMatrix = g.Transform()
            If m_Rotation <> 0 Then
                myMatrix.RotateAt(m_Rotation, New PointF(X, Y), _
                    Drawing2D.MatrixOrder.Append)
                g.Transform = myMatrix
            End If
            Dim myPenE As New Pen(Me.LineColor, Me.LineWidth)
            Dim myPen2 As New Pen(Color.White, 0)
            Dim rect As New Rectangle(X, Y, Width, Height)

            Dim gp As Drawing2D.GraphicsPath = New Drawing2D.GraphicsPath
            If Me.FlippedH Then
                gp.AddLine(CInt(X + Width), CInt(Y + 0.5 * Height), CInt(X + 0.5 * Width), CInt(Y))
                gp.AddLine(CInt(X + 0.5 * Width), CInt(Y), CInt(X), CInt(Y))
                gp.AddLine(CInt(X), CInt(Y), CInt(X), CInt(Y + Height))
                gp.AddLine(CInt(X), CInt(Y + Height), CInt(X + 0.5 * Width), CInt(Y + Height))
                gp.AddLine(CInt(X + 0.5 * Width), CInt(Y + Height), CInt(X + Width), CInt(Y + 0.5 * Height))
            Else
                gp.AddLine(CInt(X), CInt(Y + 0.5 * Height), CInt(X + 0.5 * Width), CInt(Y))
                gp.AddLine(CInt(X + 0.5 * Width), CInt(Y), CInt(X + Width), CInt(Y))
                gp.AddLine(CInt(X + Width), CInt(Y), CInt(X + Width), CInt(Y + Height))
                gp.AddLine(CInt(X + Width), CInt(Y + Height), CInt(X + 0.5 * Width), CInt(Y + Height))
                gp.AddLine(CInt(X + 0.5 * Width), CInt(Y + Height), CInt(X), CInt(Y + 0.5 * Height))
            End If

            gp.CloseFigure()

            g.SmoothingMode = SmoothingMode.AntiAlias
            g.DrawPath(myPenE, Me.GetRoundedLine(gp.PathPoints, 1))


            Dim strdist As SizeF = g.MeasureString(Me.Tag, New Font("Arial", 10, FontStyle.Bold, GraphicsUnit.Pixel, 0, False), New PointF(0, 0), New StringFormat(StringFormatFlags.NoClip, 0))
            Dim strx As Single = (Me.Width - strdist.Width) / 2
            'g.FillRectangle(Brushes.White, X + strx, Y + CSng(Height + 5), strdist.Width, strdist.Height)
            g.DrawString(Me.Tag, New Font("Arial", 10, FontStyle.Bold, GraphicsUnit.Pixel, 0, False), New SolidBrush(Me.LineColor), X + strx, Y + Height + 5)

            Dim pgb1 As New PathGradientBrush(gp)
            pgb1.CenterColor = Me.GradientColor2
            'lgb1.SetBlendTriangularShape(0.5)
            pgb1.SurroundColors = New Color() {Me.GradientColor1}

            If Me.Fill Then
                If Me.GradientMode = False Then
                    g.FillPath(New SolidBrush(Me.FillColor), gp)
                Else
                    g.FillPath(pgb1, gp)
                End If
            End If

            g.EndContainer(gContainer)
        End Sub

    End Class

    <Serializable()> Public Class NodeEnGraphic
        Inherits ShapeGraphic

#Region "Constructors"
        Public Sub New()
            Me.TipoObjeto = GraphicObjects.TipoObjeto.NodeEn
        End Sub

        Public Sub New(ByVal graphicPosition As Point)
            Me.New()
            Me.SetPosition(graphicPosition)
        End Sub

        Public Sub New(ByVal posX As Integer, ByVal posY As Integer)
            Me.New(New Point(posX, posY))
        End Sub

        Public Sub New(ByVal graphicPosition As Point, ByVal graphicSize As Size)
            Me.New(graphicPosition)
            Me.SetSize(graphicSize)
            Me.AutoSize = False
        End Sub

        Public Sub New(ByVal posX As Integer, ByVal posY As Integer, ByVal graphicSize As Size)
            Me.New(New Point(posX, posY), graphicSize)
        End Sub

        Public Sub New(ByVal posX As Integer, ByVal posY As Integer, ByVal width As Integer, ByVal height As Integer)
            Me.New(New Point(posX, posY), New Size(width, height))
        End Sub

        Public Sub New(ByVal graphicPosition As Point, ByVal Rotation As Single)
            Me.New()
            Me.SetPosition(graphicPosition)
            Me.Rotation = Rotation
        End Sub

        Public Sub New(ByVal posX As Integer, ByVal posY As Integer, ByVal Rotation As Single)
            Me.New(New Point(posX, posY), Rotation)
        End Sub

        Public Sub New(ByVal graphicPosition As Point, ByVal graphicSize As Size, ByVal Rotation As Single)
            Me.New(graphicPosition, Rotation)
            Me.SetSize(graphicSize)
            Me.AutoSize = False
        End Sub

        Public Sub New(ByVal posX As Integer, ByVal posY As Integer, ByVal graphicSize As Size, ByVal Rotation As Single)
            Me.New(New Point(posX, posY), graphicSize, Rotation)
        End Sub

        Public Sub New(ByVal posX As Integer, ByVal posY As Integer, ByVal width As Integer, _
                               ByVal height As Integer, ByVal Rotation As Single)
            Me.New(New Point(posX, posY), New Size(width, height), Rotation)
        End Sub

#End Region

        Public Overrides Sub CreateConnectors(InCount As Integer, OutCount As Integer)
            Dim myIC1 As New ConnectionPoint
            myIC1.Position = New Point(X, Y)
            myIC1.Type = ConType.ConIn

            Dim myIC2 As New ConnectionPoint
            myIC2.Position = New Point(X, Y + Height)
            myIC2.Type = ConType.ConEn

            Dim myOC1 As New ConnectionPoint
            myOC1.Position = New Point(X + Width, Y + 0.5 * Height)
            myOC1.Type = ConType.ConOut

            Me.EnergyConnector.Position = myIC2.Position
            Me.EnergyConnector.Type = ConType.ConEn

            With InputConnectors

                If .Count <> 0 Then
                    If Me.FlippedH Then
                        .Item(0).Position = New Point(X + Width, Y)
                        .Item(1).Position = New Point(X + Width, Y + Height)
                    Else
                        .Item(0).Position = New Point(X, Y)
                        .Item(1).Position = New Point(X, Y + Height)
                    End If
                Else
                    .Add(myIC1)
                    .Add(myIC2)
                End If

            End With

            With OutputConnectors

                If .Count <> 0 Then
                    If Me.FlippedH Then
                        .Item(0).Position = New Point(X, Y + 0.5 * Height)
                    Else
                        .Item(0).Position = New Point(X + Width, Y + 0.5 * Height)
                    End If
                Else
                    .Add(myOC1)
                End If

            End With
        End Sub

        Public Overrides Sub Draw(ByVal g As Graphics)

            CreateConnectors(0, 0)

            UpdateStatus(Me)

            Dim pt As Point
            Dim raio, angulo As Double
            Dim con As ConnectionPoint
            For Each con In Me.InputConnectors
                pt = con.Position
                raio = ((pt.X - Me.X) ^ 2 + (pt.Y - Me.Y) ^ 2) ^ 0.5
                angulo = Math.Atan2(pt.Y - Me.Y, pt.X - Me.X)
                pt.X = Me.X + raio * Math.Cos(angulo + Me.Rotation / 360 * 2 * Math.PI)
                pt.Y = Me.Y + raio * Math.Sin(angulo + Me.Rotation / 360 * 2 * Math.PI)
                con.Position = pt
            Next
            For Each con In Me.OutputConnectors
                pt = con.Position
                raio = ((pt.X - Me.X) ^ 2 + (pt.Y - Me.Y) ^ 2) ^ 0.5
                angulo = Math.Atan2(pt.Y - Me.Y, pt.X - Me.X)
                pt.X = Me.X + raio * Math.Cos(angulo + Me.Rotation / 360 * 2 * Math.PI)
                pt.Y = Me.Y + raio * Math.Sin(angulo + Me.Rotation / 360 * 2 * Math.PI)
                con.Position = pt
            Next

            Dim gContainer As Drawing2D.GraphicsContainer
            Dim myMatrix As Drawing2D.Matrix
            gContainer = g.BeginContainer()
            myMatrix = g.Transform()
            If m_Rotation <> 0 Then
                myMatrix.RotateAt(m_Rotation, New PointF(X, Y), _
                    Drawing2D.MatrixOrder.Append)
                g.Transform = myMatrix
            End If

            Dim myPenE As New Pen(Me.LineColor, Me.LineWidth)
            Dim myPen2 As New Pen(Color.White, 0)
            Dim rect As New Rectangle(X, Y, Width, Height)

            Dim gp As Drawing2D.GraphicsPath = New Drawing2D.GraphicsPath
            If Me.FlippedH Then
                gp.AddLine(CInt(X), CInt(Y + 0.5 * Height), CInt(X + 0.5 * Width), CInt(Y))
                gp.AddLine(CInt(X + 0.5 * Width), CInt(Y), CInt(X + Width), CInt(Y))
                gp.AddLine(CInt(X + Width), CInt(Y), CInt(X + Width), CInt(Y + Height))
                gp.AddLine(CInt(X + Width), CInt(Y + Height), CInt(X + 0.5 * Width), CInt(Y + Height))
                gp.AddLine(CInt(X + 0.5 * Width), CInt(Y + Height), CInt(X), CInt(Y + 0.5 * Height))
            Else
                gp.AddLine(CInt(X + Width), CInt(Y + 0.5 * Height), CInt(X + 0.5 * Width), CInt(Y))
                gp.AddLine(CInt(X + 0.5 * Width), CInt(Y), CInt(X), CInt(Y))
                gp.AddLine(CInt(X), CInt(Y), CInt(X), CInt(Y + Height))
                gp.AddLine(CInt(X), CInt(Y + Height), CInt(X + 0.5 * Width), CInt(Y + Height))
                gp.AddLine(CInt(X + 0.5 * Width), CInt(Y + Height), CInt(X + Width), CInt(Y + 0.5 * Height))
            End If

            gp.CloseFigure()

            g.SmoothingMode = SmoothingMode.AntiAlias
            g.DrawPath(myPenE, gp)

            g.SmoothingMode = SmoothingMode.AntiAlias

            Dim strdist As SizeF = g.MeasureString(Me.Tag, New Font("Arial", 10, FontStyle.Bold, GraphicsUnit.Pixel, 0, False), New PointF(0, 0), New StringFormat(StringFormatFlags.NoClip, 0))
            Dim strx As Single = (Me.Width - strdist.Width) / 2
            'g.FillRectangle(Brushes.White, X + strx, Y + CSng(Height + 5), strdist.Width, strdist.Height)
            g.DrawString(Me.Tag, New Font("Arial", 10, FontStyle.Bold, GraphicsUnit.Pixel, 0, False), New SolidBrush(Me.LineColor), X + strx, Y + Height + 5)

            Dim pgb1 As New PathGradientBrush(gp)
            pgb1.CenterColor = Me.GradientColor2
            'lgb1.SetBlendTriangularShape(0.5)
            pgb1.SurroundColors = New Color() {Me.GradientColor1}

            If Me.Fill Then
                If Me.GradientMode = False Then
                    g.FillPath(New SolidBrush(Me.FillColor), gp)
                Else
                    g.FillPath(pgb1, gp)
                End If
            End If

            g.EndContainer(gContainer)
        End Sub

        Protected Overrides Sub Finalize()
            MyBase.Finalize()
        End Sub
    End Class

    <Serializable()> Public Class VesselGraphic

        Inherits ShapeGraphic

#Region "Constructors"
        Public Sub New()
            Me.TipoObjeto = GraphicObjects.TipoObjeto.Vessel
            Me.Description = "VasoSeparadorGL"
            Me.m_splitpoint = True
        End Sub

        Public Sub New(ByVal graphicPosition As Point)
            Me.New()
            Me.SetPosition(graphicPosition)
        End Sub

        Public Sub New(ByVal posX As Integer, ByVal posY As Integer)
            Me.New(New Point(posX, posY))
        End Sub

        Public Sub New(ByVal graphicPosition As Point, ByVal graphicSize As Size)
            Me.New(graphicPosition)
            Me.SetSize(graphicSize)
            Me.AutoSize = False
        End Sub

        Public Sub New(ByVal posX As Integer, ByVal posY As Integer, ByVal graphicSize As Size)
            Me.New(New Point(posX, posY), graphicSize)
        End Sub

        Public Sub New(ByVal posX As Integer, ByVal posY As Integer, ByVal width As Integer, ByVal height As Integer)
            Me.New(New Point(posX, posY), New Size(width, height))
        End Sub

        Public Sub New(ByVal graphicPosition As Point, ByVal Rotation As Single)
            Me.New()
            Me.SetPosition(graphicPosition)
            Me.Rotation = Rotation
        End Sub

        Public Sub New(ByVal posX As Integer, ByVal posY As Integer, ByVal Rotation As Single)
            Me.New(New Point(posX, posY), Rotation)
        End Sub

        Public Sub New(ByVal graphicPosition As Point, ByVal graphicSize As Size, ByVal Rotation As Single)
            Me.New(graphicPosition, Rotation)
            Me.SetSize(graphicSize)
            Me.AutoSize = False
        End Sub

        Public Sub New(ByVal posX As Integer, ByVal posY As Integer, ByVal graphicSize As Size, ByVal Rotation As Single)
            Me.New(New Point(posX, posY), graphicSize, Rotation)
        End Sub

        Public Sub New(ByVal posX As Integer, ByVal posY As Integer, ByVal width As Integer, _
                               ByVal height As Integer, ByVal Rotation As Single)
            Me.New(New Point(posX, posY), New Size(width, height), Rotation)
        End Sub

#End Region


        Public Overrides Sub CreateConnectors(InCount As Integer, OutCount As Integer)

            Dim myIC1 As New ConnectionPoint
            myIC1.Position = New Point(X + 0.25 * Width, Y + 0.2 * Height)
            myIC1.Type = ConType.ConIn

            Dim myIC2 As New ConnectionPoint
            myIC2.Position = New Point(X + 0.25 * Width, Y + 0.3 * Height)
            myIC2.Type = ConType.ConIn

            Dim myIC3 As New ConnectionPoint
            myIC3.Position = New Point(X + 0.25 * Width, Y + 0.4 * Height)
            myIC3.Type = ConType.ConIn

            Dim myIC4 As New ConnectionPoint
            myIC4.Position = New Point(X + 0.25 * Width, Y + 0.5 * Height)
            myIC4.Type = ConType.ConIn

            Dim myIC5 As New ConnectionPoint
            myIC5.Position = New Point(X + 0.25 * Width, Y + 0.6 * Height)
            myIC5.Type = ConType.ConIn

            Dim myIC6 As New ConnectionPoint
            myIC6.Position = New Point(X + 0.25 * Width, Y + 0.7 * Height)
            myIC6.Type = ConType.ConIn

            Dim myOC1 As New ConnectionPoint
            myOC1.Position = New Point(X + 0.827 * Width, Y + (0.1 + 0.127 / 2) * Height)
            myOC1.Type = ConType.ConOut

            Dim myOC2 As New ConnectionPoint
            myOC2.Position = New Point(X + 0.827 * Width, Y + (0.773 + 0.127 / 2) * Height)
            myOC2.Type = ConType.ConOut

            Dim myOC3 As New ConnectionPoint
            myOC3.Position = New Point(X + 0.5 * Width, Y + Height)
            myOC3.Type = ConType.ConOut

            Dim myIC7 As New ConnectionPoint
            myIC7.Position = New Point(X + 0.25 * Width, Y + 0.8 * Height)
            myIC7.Type = ConType.ConEn

            With InputConnectors

                If .Count <> 0 Then
                    If .Count = 1 Then
                        .Add(myIC2)
                        .Add(myIC3)
                        .Add(myIC4)
                        .Add(myIC5)
                        .Add(myIC6)
                        .Add(myIC7)
                    End If
                    If .Count = 6 Then
                        .Add(myIC7)
                    End If
                    If Not Me.FlippedH Then
                        .Item(0).Position = New Point(X + 0.25 * Width, Y + 0.2 * Height)
                        .Item(1).Position = New Point(X + 0.25 * Width, Y + 0.3 * Height)
                        .Item(2).Position = New Point(X + 0.25 * Width, Y + 0.4 * Height)
                        .Item(3).Position = New Point(X + 0.25 * Width, Y + 0.5 * Height)
                        .Item(4).Position = New Point(X + 0.25 * Width, Y + 0.6 * Height)
                        .Item(5).Position = New Point(X + 0.25 * Width, Y + 0.7 * Height)
                        .Item(6).Position = New Point(X + 0.25 * Width, Y + 0.8 * Height)
                    Else
                        .Item(0).Position = New Point(X + (1 - 0.25) * Width, Y + 0.2 * Height)
                        .Item(1).Position = New Point(X + (1 - 0.25) * Width, Y + 0.3 * Height)
                        .Item(2).Position = New Point(X + (1 - 0.25) * Width, Y + 0.4 * Height)
                        .Item(3).Position = New Point(X + (1 - 0.25) * Width, Y + 0.5 * Height)
                        .Item(4).Position = New Point(X + (1 - 0.25) * Width, Y + 0.6 * Height)
                        .Item(5).Position = New Point(X + (1 - 0.25) * Width, Y + 0.7 * Height)
                        .Item(6).Position = New Point(X + (1 - 0.25) * Width, Y + 0.8 * Height)
                    End If
                Else
                    .Add(myIC1)
                    .Add(myIC2)
                    .Add(myIC3)
                    .Add(myIC4)
                    .Add(myIC5)
                    .Add(myIC6)
                    .Add(myIC7)
                End If

            End With

            With OutputConnectors

                If .Count = 2 Then .Add(myOC3)

                If .Count <> 0 Then
                    If Me.FlippedH Then
                        .Item(0).Position = New Point(X + 0.3 * Width, Y + (0.1 + 0.127 / 2) * Height)
                        .Item(1).Position = New Point(X + 0.3 * Width, Y + (0.773 + 0.127 / 2) * Height)
                        .Item(2).Position = New Point(X + 0.5 * Width, Y + Height)
                    Else
                        .Item(0).Position = New Point(X + 0.827 * Width, Y + (0.1 + 0.127 / 2) * Height)
                        .Item(1).Position = New Point(X + 0.827 * Width, Y + (0.773 + 0.127 / 2) * Height)
                        .Item(2).Position = New Point(X + 0.5 * Width, Y + Height)
                    End If
                Else
                    .Add(myOC1)
                    .Add(myOC2)
                    .Add(myOC3)
                End If

            End With
        End Sub

        Public Overrides Sub Draw(ByVal g As System.Drawing.Graphics)

            CreateConnectors(0, 0)

            UpdateStatus(Me)

            Dim pt As Point
            Dim raio, angulo As Double
            Dim con As ConnectionPoint
            For Each con In Me.InputConnectors
                pt = con.Position
                raio = ((pt.X - Me.X) ^ 2 + (pt.Y - Me.Y) ^ 2) ^ 0.5
                angulo = Math.Atan2(pt.Y - Me.Y, pt.X - Me.X)
                pt.X = Me.X + raio * Math.Cos(angulo + Me.Rotation / 360 * 2 * Math.PI)
                pt.Y = Me.Y + raio * Math.Sin(angulo + Me.Rotation / 360 * 2 * Math.PI)
                con.Position = pt
            Next
            For Each con In Me.OutputConnectors
                pt = con.Position
                raio = ((pt.X - Me.X) ^ 2 + (pt.Y - Me.Y) ^ 2) ^ 0.5
                angulo = Math.Atan2(pt.Y - Me.Y, pt.X - Me.X)
                pt.X = Me.X + raio * Math.Cos(angulo + Me.Rotation / 360 * 2 * Math.PI)
                pt.Y = Me.Y + raio * Math.Sin(angulo + Me.Rotation / 360 * 2 * Math.PI)
                con.Position = pt
            Next
            With Me.EnergyConnector
                pt = .Position
                raio = ((pt.X - Me.X) ^ 2 + (pt.Y - Me.Y) ^ 2) ^ 0.5
                angulo = Math.Atan2(pt.Y - Me.Y, pt.X - Me.X)
                pt.X = Me.X + raio * Math.Cos(angulo + Me.Rotation / 360 * 2 * Math.PI)
                pt.Y = Me.Y + raio * Math.Sin(angulo + Me.Rotation / 360 * 2 * Math.PI)
                .Position = pt
            End With

            Dim gContainer As System.Drawing.Drawing2D.GraphicsContainer
            Dim myMatrix As Drawing2D.Matrix
            gContainer = g.BeginContainer()
            myMatrix = g.Transform()
            If m_Rotation <> 0 Then
                myMatrix.RotateAt(m_Rotation, New PointF(X, Y), Drawing.Drawing2D.MatrixOrder.Append)
                g.Transform = myMatrix
            End If

            Dim rect3 As New Rectangle(X + 0.7 * Width, Y + 0.1 * Height, 0.127 * Width, 0.127 * Height)
            Dim rect4 As New Rectangle(X + 0.7 * Width, Y + 0.773 * Height, 0.127 * Width, 0.127 * Height)
            If Me.FlippedH = True Then
                rect3 = New Rectangle(X + 0.3 * Width, Y + 0.1 * Height, 0.127 * Width, 0.127 * Height)
                rect4 = New Rectangle(X + 0.3 * Width, Y + 0.773 * Height, 0.127 * Width, 0.127 * Height)
            End If

            Dim myPen As New Pen(Me.LineColor, Me.LineWidth)

            Dim myPen2 As New Pen(Color.White, 0)

            Dim rect As New Rectangle(X, Y, Width, Height)

            g.SmoothingMode = SmoothingMode.AntiAlias
            'g.DrawRectangle(myPen2, rect)
            If Me.FlippedH = True Then
                Me.DrawRoundRect(g, myPen, X + 0.4 * Width, Y, 0.45 * Width, Height, 10, Brushes.Transparent)
            Else
                Me.DrawRoundRect(g, myPen, X + 0.25 * Width, Y, 0.45 * Width, Height, 10, Brushes.Transparent)
            End If
            g.DrawRectangle(myPen, rect3)
            g.DrawRectangle(myPen, rect4)

            Dim strdist As SizeF = g.MeasureString(Me.Tag, New Font("Arial", 10, FontStyle.Bold, GraphicsUnit.Pixel, 0, False), New PointF(0, 0), New StringFormat(StringFormatFlags.NoClip, 0))
            Dim strx As Single = (Me.Width - strdist.Width) / 2
            'g.FillRectangle(Brushes.White, X + strx, Y + CSng(Height + 5), strdist.Width, strdist.Height)
            g.DrawString(Me.Tag, New Font("Arial", 10, FontStyle.Bold, GraphicsUnit.Pixel, 0, False), New SolidBrush(Me.LineColor), X + strx, Y + Height + 5)

            Dim gp As Drawing2D.GraphicsPath = New Drawing2D.GraphicsPath
            Dim radius As Integer = 3
            gp.AddLine(X + radius, Y, X + Width - radius, Y)
            gp.AddArc(X + Width - radius, Y, radius, radius, 270, 90)
            gp.AddLine(X + Width, Y + radius, X + Width, Y + Height - radius)
            gp.AddArc(X + Width - radius, Y + Height - radius, radius, radius, 0, 90)
            gp.AddLine(X + Width - radius, Y + Height, X + radius, Y + Height)
            gp.AddArc(X, Y + Height - radius, radius, radius, 90, 90)
            gp.AddLine(X, Y + Height - radius, X, Y + radius)
            gp.AddArc(X, Y, radius, radius, 180, 90)
            Dim lgb1 As LinearGradientBrush
            lgb1 = New LinearGradientBrush(rect, Me.GradientColor1, Me.GradientColor2, LinearGradientMode.Horizontal)
            lgb1.SetBlendTriangularShape(0.5)
            'lgb1.CenterColor = Me.GradientColor1
            'lgb1.SetBlendTriangularShape(0.5)
            'lgb1.SurroundColors = New Color() {Me.GradientColor2}
            'lgb1.WrapMode = WrapMode.Tile
            If Me.Fill Then
                g.FillRectangle(New SolidBrush(Me.FillColor), rect3)
                g.FillRectangle(New SolidBrush(Me.FillColor), rect4)
                If Me.GradientMode = False Then
                    If Me.FlippedH = True Then
                        Me.DrawRoundRect(g, myPen, X + 0.4 * Width, Y, 0.45 * Width, Height, 6, New SolidBrush(Me.FillColor))
                    Else
                        Me.DrawRoundRect(g, myPen, X + 0.25 * Width, Y, 0.45 * Width, Height, 6, New SolidBrush(Me.FillColor))
                    End If
                Else
                    If Me.FlippedH = True Then
                        Me.DrawRoundRect(g, myPen, X + 0.4 * Width, Y, 0.45 * Width, Height, 6, lgb1)
                    Else
                        Me.DrawRoundRect(g, myPen, X + 0.25 * Width, Y, 0.45 * Width, Height, 6, lgb1)
                    End If
                End If
            End If

            g.EndContainer(gContainer)
            gp.Dispose()

        End Sub

        Public Sub DrawRoundRect(ByVal g As Graphics, ByVal p As Pen, ByVal x As Integer, ByVal y As Integer, ByVal width As Integer, ByVal height As Integer, ByVal radius As Integer, ByVal myBrush As Brush)

            Dim gp As Drawing2D.GraphicsPath = New Drawing2D.GraphicsPath

            gp.AddLine(x + radius, y, x + width - radius, y)
            gp.AddArc(x + width - radius, y, radius, radius, 270, 90)
            gp.AddLine(x + width, y + radius, x + width, y + height - radius)
            gp.AddArc(x + width - radius, y + height - radius, radius, radius, 0, 90)
            gp.AddLine(x + width - radius, y + height, x + radius, y + height)
            gp.AddArc(x, y + height - radius, radius, radius, 90, 90)
            gp.AddLine(x, y + height - radius, x, y + radius)
            gp.AddArc(x, y, radius, radius, 180, 90)

            gp.CloseFigure()

            g.DrawPath(p, gp)
            g.FillPath(myBrush, gp)

            gp.Dispose()

        End Sub

    End Class

    <Serializable()> Public Class CompressorGraphic

        Inherits ShapeGraphic

#Region "Constructors"
        Public Sub New()
            Me.TipoObjeto = GraphicObjects.TipoObjeto.Compressor
            Me.Description = "CompressorAdiabtico"
        End Sub

        Public Sub New(ByVal graphicPosition As Point)
            Me.New()
            Me.SetPosition(graphicPosition)
        End Sub

        Public Sub New(ByVal posX As Integer, ByVal posY As Integer)
            Me.New(New Point(posX, posY))
        End Sub

        Public Sub New(ByVal graphicPosition As Point, ByVal graphicSize As Size)
            Me.New(graphicPosition)
            Me.SetSize(graphicSize)
            Me.AutoSize = False
        End Sub

        Public Sub New(ByVal posX As Integer, ByVal posY As Integer, ByVal graphicSize As Size)
            Me.New(New Point(posX, posY), graphicSize)
        End Sub

        Public Sub New(ByVal posX As Integer, ByVal posY As Integer, ByVal width As Integer, ByVal height As Integer)
            Me.New(New Point(posX, posY), New Size(width, height))
        End Sub

        Public Sub New(ByVal graphicPosition As Point, ByVal Rotation As Single)
            Me.New()
            Me.SetPosition(graphicPosition)
            Me.Rotation = Rotation
        End Sub

        Public Sub New(ByVal posX As Integer, ByVal posY As Integer, ByVal Rotation As Single)
            Me.New(New Point(posX, posY), Rotation)
        End Sub

        Public Sub New(ByVal graphicPosition As Point, ByVal graphicSize As Size, ByVal Rotation As Single)
            Me.New(graphicPosition, Rotation)
            Me.SetSize(graphicSize)
            Me.AutoSize = False
        End Sub

        Public Sub New(ByVal posX As Integer, ByVal posY As Integer, ByVal graphicSize As Size, ByVal Rotation As Single)
            Me.New(New Point(posX, posY), graphicSize, Rotation)
        End Sub

        Public Sub New(ByVal posX As Integer, ByVal posY As Integer, ByVal width As Integer, _
                               ByVal height As Integer, ByVal Rotation As Single)
            Me.New(New Point(posX, posY), New Size(width, height), Rotation)
        End Sub

#End Region

        Public Overrides Sub CreateConnectors(InCount As Integer, OutCount As Integer)

            Dim myIC1 As New ConnectionPoint
            myIC1.Position = New Point(X, Y + 0.5 * Height)
            myIC1.Type = ConType.ConIn

            Dim myIC2 As New ConnectionPoint
            myIC2.Position = New Point(X + 0.5 * Width, Y + Height)
            myIC2.Type = ConType.ConEn

            Dim myOC1 As New ConnectionPoint
            myOC1.Position = New Point(X + Width, Y + 0.5 * Height)
            myOC1.Type = ConType.ConOut

            Me.EnergyConnector.Position = New Point(X + 0.5 * Width, Y + Height)
            Me.EnergyConnector.Type = ConType.ConEn

            With InputConnectors

                If .Count = 2 Then
                    If Me.FlippedH Then
                        .Item(0).Position = New Point(X + Width, Y + 0.5 * Height)
                        .Item(1).Position = New Point(X + 0.5 * Width, Y + Height)
                    Else
                        .Item(0).Position = New Point(X, Y + 0.5 * Height)
                        .Item(1).Position = New Point(X + 0.5 * Width, Y + Height)
                    End If
                Else
                    .Add(myIC1)
                    .Add(myIC2)
                End If

            End With

            With OutputConnectors

                If .Count <> 0 Then
                    If Me.FlippedH Then
                        .Item(0).Position = New Point(X, Y + 0.5 * Height)
                    Else
                        .Item(0).Position = New Point(X + Width, Y + 0.5 * Height)
                    End If
                Else
                    .Add(myOC1)
                End If

            End With

        End Sub

        Public Overrides Sub Draw(ByVal g As System.Drawing.Graphics)

            CreateConnectors(0, 0)

            UpdateStatus(Me)

            Dim pt As Point
            Dim raio, angulo As Double
            Dim con As ConnectionPoint
            For Each con In Me.InputConnectors
                pt = con.Position
                raio = ((pt.X - Me.X) ^ 2 + (pt.Y - Me.Y) ^ 2) ^ 0.5
                angulo = Math.Atan2(pt.Y - Me.Y, pt.X - Me.X)
                pt.X = Me.X + raio * Math.Cos(angulo + Me.Rotation / 360 * 2 * Math.PI)
                pt.Y = Me.Y + raio * Math.Sin(angulo + Me.Rotation / 360 * 2 * Math.PI)
                con.Position = pt
            Next
            For Each con In Me.OutputConnectors
                pt = con.Position
                raio = ((pt.X - Me.X) ^ 2 + (pt.Y - Me.Y) ^ 2) ^ 0.5
                angulo = Math.Atan2(pt.Y - Me.Y, pt.X - Me.X)
                pt.X = Me.X + raio * Math.Cos(angulo + Me.Rotation / 360 * 2 * Math.PI)
                pt.Y = Me.Y + raio * Math.Sin(angulo + Me.Rotation / 360 * 2 * Math.PI)
                con.Position = pt
            Next
            With Me.EnergyConnector
                pt = .Position
                raio = ((pt.X - Me.X) ^ 2 + (pt.Y - Me.Y) ^ 2) ^ 0.5
                angulo = Math.Atan2(pt.Y - Me.Y, pt.X - Me.X)
                pt.X = Me.X + raio * Math.Cos(angulo + Me.Rotation / 360 * 2 * Math.PI)
                pt.Y = Me.Y + raio * Math.Sin(angulo + Me.Rotation / 360 * 2 * Math.PI)
                .Position = pt
            End With

            Dim gContainer As System.Drawing.Drawing2D.GraphicsContainer
            Dim myMatrix As Drawing2D.Matrix
            gContainer = g.BeginContainer()
            myMatrix = g.Transform()
            If m_Rotation <> 0 Then
                myMatrix.RotateAt(m_Rotation, New PointF(X, Y), Drawing.Drawing2D.MatrixOrder.Append)
                g.Transform = myMatrix
            End If


            Dim myPen As New Pen(Me.LineColor, Me.LineWidth)
            Dim myPen2 As New Pen(Color.White, 0)
            Dim rect As New Rectangle(X, Y, Width, Height)

            g.SmoothingMode = SmoothingMode.AntiAlias
            'g.DrawRectangle(myPen2, rect)

            Dim strdist As SizeF = g.MeasureString(Me.Tag, New Font("Arial", 10, FontStyle.Bold, GraphicsUnit.Pixel, 0, False), New PointF(0, 0), New StringFormat(StringFormatFlags.NoClip, 0))
            Dim strx As Single = (Me.Width - strdist.Width) / 2
            'g.FillRectangle(Brushes.White, X + strx, Y + CSng(Height + 5), strdist.Width, strdist.Height)
            g.DrawString(Me.Tag, New Font("Arial", 10, FontStyle.Bold, GraphicsUnit.Pixel, 0, False), New SolidBrush(Me.LineColor), X + strx, Y + Height + 5)

            Dim gp As Drawing2D.GraphicsPath = New Drawing2D.GraphicsPath
            If Me.FlippedH = False Then
                gp.AddLine(CInt(X), CInt(Y), CInt(X + Width), CInt(Y + 0.3 * Height))
                gp.AddLine(CInt(X + Width), CInt(Y + 0.3 * Height), CInt(X + Width), CInt(Y + 0.7 * Height))
                gp.AddLine(CInt(X + Width), CInt(Y + 0.7 * Height), CInt(X), CInt(Y + Height))
                gp.AddLine(CInt(X), CInt(Y + Height), CInt(X), CInt(Y))
            Else
                gp.AddLine(CInt(X + Width), CInt(Y), CInt(X), CInt(Y + 0.3 * Height))
                gp.AddLine(CInt(X), CInt(Y + 0.3 * Height), CInt(X), CInt(Y + 0.7 * Height))
                gp.AddLine(CInt(X), CInt(Y + 0.7 * Height), CInt(X + Width), CInt(Y + Height))
                gp.AddLine(CInt(X + Width), CInt(Y + Height), CInt(X + Width), CInt(Y))
            End If

            gp.CloseFigure()

            g.DrawPath(myPen, Me.GetRoundedLine(gp.PathPoints, 1))

            Dim pgb1 As New LinearGradientBrush(New PointF(X, Y), New PointF(X, Y + Height), Me.GradientColor1, Me.GradientColor2)

            If Me.Fill Then
                If Me.GradientMode = False Then
                    g.FillPath(New SolidBrush(Me.FillColor), gp)
                Else
                    g.FillPath(pgb1, gp)
                End If
            End If


            gp.Dispose()
            g.EndContainer(gContainer)

        End Sub

    End Class

    <Serializable()> Public Class HeaterGraphic

        Inherits ShapeGraphic

#Region "Constructors"
        Public Sub New()
            Me.TipoObjeto = GraphicObjects.TipoObjeto.Heater
            Me.Description = "Aquecedor"
        End Sub

        Public Sub New(ByVal graphicPosition As Point)
            Me.New()
            Me.SetPosition(graphicPosition)
        End Sub

        Public Sub New(ByVal posX As Integer, ByVal posY As Integer)
            Me.New(New Point(posX, posY))
        End Sub

        Public Sub New(ByVal graphicPosition As Point, ByVal graphicSize As Size)
            Me.New(graphicPosition)
            Me.SetSize(graphicSize)
            Me.AutoSize = False
        End Sub

        Public Sub New(ByVal posX As Integer, ByVal posY As Integer, ByVal graphicSize As Size)
            Me.New(New Point(posX, posY), graphicSize)
        End Sub

        Public Sub New(ByVal posX As Integer, ByVal posY As Integer, ByVal width As Integer, ByVal height As Integer)
            Me.New(New Point(posX, posY), New Size(width, height))
        End Sub

        Public Sub New(ByVal graphicPosition As Point, ByVal Rotation As Single)
            Me.New()
            Me.SetPosition(graphicPosition)
            Me.Rotation = Rotation
        End Sub

        Public Sub New(ByVal posX As Integer, ByVal posY As Integer, ByVal Rotation As Single)
            Me.New(New Point(posX, posY), Rotation)
        End Sub

        Public Sub New(ByVal graphicPosition As Point, ByVal graphicSize As Size, ByVal Rotation As Single)
            Me.New(graphicPosition, Rotation)
            Me.SetSize(graphicSize)
            Me.AutoSize = False
        End Sub

        Public Sub New(ByVal posX As Integer, ByVal posY As Integer, ByVal graphicSize As Size, ByVal Rotation As Single)
            Me.New(New Point(posX, posY), graphicSize, Rotation)
        End Sub

        Public Sub New(ByVal posX As Integer, ByVal posY As Integer, ByVal width As Integer, _
                               ByVal height As Integer, ByVal Rotation As Single)
            Me.New(New Point(posX, posY), New Size(width, height), Rotation)
        End Sub

#End Region

        Public Overrides Sub CreateConnectors(InCount As Integer, OutCount As Integer)

            Dim myIC1 As New ConnectionPoint
            myIC1.Position = New Point(X, Y + 0.5 * Height)
            myIC1.Type = ConType.ConIn

            Dim myIC2 As New ConnectionPoint
            myIC2.Position = New Point(X + 0.5 * Width, Y + Height)
            myIC2.Type = ConType.ConEn

            Dim myOC1 As New ConnectionPoint
            myOC1.Position = New Point(X + Width, Y + 0.5 * Height)
            myOC1.Type = ConType.ConOut

            Me.EnergyConnector.Position = New Point(X + 0.5 * Width, Y + Height)
            Me.EnergyConnector.Type = ConType.ConEn

            With InputConnectors

                If .Count = 2 Then
                    If Me.FlippedH Then
                        .Item(0).Position = New Point(X + Width, Y + 0.5 * Height)
                        .Item(1).Position = New Point(X + 0.5 * Width, Y + Height)
                    Else
                        .Item(0).Position = New Point(X, Y + 0.5 * Height)
                        .Item(1).Position = New Point(X + 0.5 * Width, Y + Height)
                    End If
                Else
                    .Add(myIC1)
                    .Add(myIC2)
                End If

            End With

            With OutputConnectors

                If .Count <> 0 Then
                    If Me.FlippedH Then
                        .Item(0).Position = New Point(X, Y + 0.5 * Height)
                    Else
                        .Item(0).Position = New Point(X + Width, Y + 0.5 * Height)
                    End If
                Else
                    .Add(myOC1)
                End If

            End With

        End Sub

        Public Overrides Sub Draw(ByVal g As System.Drawing.Graphics)

            CreateConnectors(0, 0)

            UpdateStatus(Me)

            Dim pt As Point
            Dim raio, angulo As Double
            Dim con As ConnectionPoint
            For Each con In Me.InputConnectors
                pt = con.Position
                raio = ((pt.X - Me.X) ^ 2 + (pt.Y - Me.Y) ^ 2) ^ 0.5
                angulo = Math.Atan2(pt.Y - Me.Y, pt.X - Me.X)
                pt.X = Me.X + raio * Math.Cos(angulo + Me.Rotation / 360 * 2 * Math.PI)
                pt.Y = Me.Y + raio * Math.Sin(angulo + Me.Rotation / 360 * 2 * Math.PI)
                con.Position = pt
            Next
            For Each con In Me.OutputConnectors
                pt = con.Position
                raio = ((pt.X - Me.X) ^ 2 + (pt.Y - Me.Y) ^ 2) ^ 0.5
                angulo = Math.Atan2(pt.Y - Me.Y, pt.X - Me.X)
                pt.X = Me.X + raio * Math.Cos(angulo + Me.Rotation / 360 * 2 * Math.PI)
                pt.Y = Me.Y + raio * Math.Sin(angulo + Me.Rotation / 360 * 2 * Math.PI)
                con.Position = pt
            Next
            With Me.EnergyConnector
                pt = .Position
                raio = ((pt.X - Me.X) ^ 2 + (pt.Y - Me.Y) ^ 2) ^ 0.5
                angulo = Math.Atan2(pt.Y - Me.Y, pt.X - Me.X)
                pt.X = Me.X + raio * Math.Cos(angulo + Me.Rotation / 360 * 2 * Math.PI)
                pt.Y = Me.Y + raio * Math.Sin(angulo + Me.Rotation / 360 * 2 * Math.PI)
                .Position = pt
            End With

            Dim gContainer As System.Drawing.Drawing2D.GraphicsContainer
            Dim myMatrix As Drawing2D.Matrix
            gContainer = g.BeginContainer()
            myMatrix = g.Transform()
            If m_Rotation <> 0 Then
                myMatrix.RotateAt(m_Rotation, New PointF(X, Y), Drawing.Drawing2D.MatrixOrder.Append)
                g.Transform = myMatrix
            End If


            Dim myPen As New Pen(Me.LineColor, Me.LineWidth)
            Dim myPen2 As New Pen(Color.White, 0)
            Dim rect As New Rectangle(X, Y, Width, Height)

            'g.DrawRectangle(myPen2, rect)

            Dim gp As Drawing2D.GraphicsPath = New Drawing2D.GraphicsPath
            gp.AddLine(CInt(X), CInt(Y + 0.5 * Height), CInt(X + 0.5 * Width), CInt(Y))
            gp.AddLine(CInt(X + 0.5 * Width), CInt(Y), CInt(X + Width), CInt(Y + 0.5 * Height))
            gp.AddLine(CInt(X + Width), CInt(Y + 0.5 * Height), CInt(X + 0.5 * Width), CInt(Y + Height))
            gp.AddLine(CInt(X + 0.5 * Width), CInt(Y + Height), CInt(X), CInt(Y + 0.5 * Height))

            gp.CloseFigure()

            g.SmoothingMode = SmoothingMode.AntiAlias
            g.DrawPath(myPen, gp)

            Dim strdist As SizeF = g.MeasureString(Me.Tag, New Font("Arial", 10, FontStyle.Bold, GraphicsUnit.Pixel, 0, False), New PointF(0, 0), New StringFormat(StringFormatFlags.NoClip, 0))
            Dim strx As Single = (Me.Width - strdist.Width) / 2
            'g.FillRectangle(Brushes.White, X + strx, Y + CSng(Height + 5), strdist.Width, strdist.Height)
            g.DrawString(Me.Tag, New Font("Arial", 10, FontStyle.Bold, GraphicsUnit.Pixel, 0, False), New SolidBrush(Me.LineColor), X + strx, Y + Height + 5)

            Dim pgb1 As New PathGradientBrush(gp)
            pgb1.CenterColor = Me.GradientColor2
            'lgb1.SetBlendTriangularShape(0.5)
            pgb1.SurroundColors = New Color() {Me.GradientColor1}

            If Me.Fill Then
                If Me.GradientMode = False Then
                    g.FillPath(New SolidBrush(Me.FillColor), gp)
                Else
                    g.FillPath(pgb1, gp)
                End If
            End If

            Dim size As SizeF
            Dim fontA As New Font("Arial", 10, 3, GraphicsUnit.Pixel, 0, False)
            size = g.MeasureString("A", fontA)

            Dim ax, ay As Integer
            ax = Me.X + (Me.Width - size.Width) / 2
            ay = Me.Y + (Me.Height - size.Height) / 2

            g.SmoothingMode = SmoothingMode.AntiAlias
            g.TextRenderingHint = Text.TextRenderingHint.AntiAlias
            g.DrawString("A", fontA, Brushes.DarkOrange, ax, ay)

            gp.Dispose()
            g.EndContainer(gContainer)

        End Sub

    End Class

    <Serializable()> Public Class CoolerGraphic

        Inherits ShapeGraphic

#Region "Constructors"
        Public Sub New()
            Me.TipoObjeto = GraphicObjects.TipoObjeto.Cooler
            Me.Description = "Resfriador"
        End Sub

        Public Sub New(ByVal graphicPosition As Point)
            Me.New()
            Me.SetPosition(graphicPosition)
        End Sub

        Public Sub New(ByVal posX As Integer, ByVal posY As Integer)
            Me.New(New Point(posX, posY))
        End Sub

        Public Sub New(ByVal graphicPosition As Point, ByVal graphicSize As Size)
            Me.New(graphicPosition)
            Me.SetSize(graphicSize)
            Me.AutoSize = False
        End Sub

        Public Sub New(ByVal posX As Integer, ByVal posY As Integer, ByVal graphicSize As Size)
            Me.New(New Point(posX, posY), graphicSize)
        End Sub

        Public Sub New(ByVal posX As Integer, ByVal posY As Integer, ByVal width As Integer, ByVal height As Integer)
            Me.New(New Point(posX, posY), New Size(width, height))
        End Sub

        Public Sub New(ByVal graphicPosition As Point, ByVal Rotation As Single)
            Me.New()
            Me.SetPosition(graphicPosition)
            Me.Rotation = Rotation
        End Sub

        Public Sub New(ByVal posX As Integer, ByVal posY As Integer, ByVal Rotation As Single)
            Me.New(New Point(posX, posY), Rotation)
        End Sub

        Public Sub New(ByVal graphicPosition As Point, ByVal graphicSize As Size, ByVal Rotation As Single)
            Me.New(graphicPosition, Rotation)
            Me.SetSize(graphicSize)
            Me.AutoSize = False
        End Sub

        Public Sub New(ByVal posX As Integer, ByVal posY As Integer, ByVal graphicSize As Size, ByVal Rotation As Single)
            Me.New(New Point(posX, posY), graphicSize, Rotation)
        End Sub

        Public Sub New(ByVal posX As Integer, ByVal posY As Integer, ByVal width As Integer, _
                               ByVal height As Integer, ByVal Rotation As Single)
            Me.New(New Point(posX, posY), New Size(width, height), Rotation)
        End Sub

#End Region

        Public Overrides Sub CreateConnectors(InCount As Integer, OutCount As Integer)

            Dim myIC1 As New ConnectionPoint
            myIC1.Position = New Point(X, Y + 0.5 * Height)
            myIC1.Type = ConType.ConIn

            Dim myOC1 As New ConnectionPoint
            myOC1.Position = New Point(X + Width, Y + 0.5 * Height)
            myOC1.Type = ConType.ConOut

            Me.EnergyConnector.Position = New Point(X + 0.5 * Width, Y + Height)
            Me.EnergyConnector.Type = ConType.ConEn

            With InputConnectors

                If .Count <> 0 Then
                    If Me.FlippedH Then
                        .Item(0).Position = New Point(X + Width, Y + 0.5 * Height)
                    Else
                        .Item(0).Position = New Point(X, Y + 0.5 * Height)

                    End If
                Else
                    .Add(myIC1)
                End If

            End With

            With OutputConnectors

                If .Count <> 0 Then
                    If Me.FlippedH Then
                        .Item(0).Position = New Point(X, Y + 0.5 * Height)
                    Else
                        .Item(0).Position = New Point(X + Width, Y + 0.5 * Height)
                    End If
                Else
                    .Add(myOC1)
                End If

            End With

        End Sub


        Public Overrides Sub Draw(ByVal g As System.Drawing.Graphics)

            CreateConnectors(0, 0)

            UpdateStatus(Me)

            Dim pt As Point
            Dim raio, angulo As Double
            Dim con As ConnectionPoint
            For Each con In Me.InputConnectors
                pt = con.Position
                raio = ((pt.X - Me.X) ^ 2 + (pt.Y - Me.Y) ^ 2) ^ 0.5
                angulo = Math.Atan2(pt.Y - Me.Y, pt.X - Me.X)
                pt.X = Me.X + raio * Math.Cos(angulo + Me.Rotation / 360 * 2 * Math.PI)
                pt.Y = Me.Y + raio * Math.Sin(angulo + Me.Rotation / 360 * 2 * Math.PI)
                con.Position = pt
            Next
            For Each con In Me.OutputConnectors
                pt = con.Position
                raio = ((pt.X - Me.X) ^ 2 + (pt.Y - Me.Y) ^ 2) ^ 0.5
                angulo = Math.Atan2(pt.Y - Me.Y, pt.X - Me.X)
                pt.X = Me.X + raio * Math.Cos(angulo + Me.Rotation / 360 * 2 * Math.PI)
                pt.Y = Me.Y + raio * Math.Sin(angulo + Me.Rotation / 360 * 2 * Math.PI)
                con.Position = pt
            Next
            With Me.EnergyConnector
                pt = .Position
                raio = ((pt.X - Me.X) ^ 2 + (pt.Y - Me.Y) ^ 2) ^ 0.5
                angulo = Math.Atan2(pt.Y - Me.Y, pt.X - Me.X)
                pt.X = Me.X + raio * Math.Cos(angulo + Me.Rotation / 360 * 2 * Math.PI)
                pt.Y = Me.Y + raio * Math.Sin(angulo + Me.Rotation / 360 * 2 * Math.PI)
                .Position = pt
            End With

            Dim gContainer As System.Drawing.Drawing2D.GraphicsContainer
            Dim myMatrix As Drawing2D.Matrix
            gContainer = g.BeginContainer()
            myMatrix = g.Transform()
            If m_Rotation <> 0 Then
                myMatrix.RotateAt(m_Rotation, New PointF(X, Y), Drawing.Drawing2D.MatrixOrder.Append)
                g.Transform = myMatrix
            End If


            Dim myPen As New Pen(Me.LineColor, Me.LineWidth)
            Dim myPen2 As New Pen(Color.White, 0)
            Dim rect As New Rectangle(X, Y, Width, Height)
            g.SmoothingMode = SmoothingMode.AntiAlias
            'g.DrawRectangle(myPen2, rect)

            Dim gp As Drawing2D.GraphicsPath = New Drawing2D.GraphicsPath
            gp.AddLine(CInt(X), CInt(Y + 0.5 * Height), CInt(X + 0.5 * Width), CInt(Y))
            gp.AddLine(CInt(X + 0.5 * Width), CInt(Y), CInt(X + Width), CInt(Y + 0.5 * Height))
            gp.AddLine(CInt(X + Width), CInt(Y + 0.5 * Height), CInt(X + 0.5 * Width), CInt(Y + Height))
            gp.AddLine(CInt(X + 0.5 * Width), CInt(Y + Height), CInt(X), CInt(Y + 0.5 * Height))

            gp.CloseFigure()

            g.DrawPath(myPen, gp)

            Dim strdist As SizeF = g.MeasureString(Me.Tag, New Font("Arial", 10, FontStyle.Bold, GraphicsUnit.Pixel, 0, False), New PointF(0, 0), New StringFormat(StringFormatFlags.NoClip, 0))
            Dim strx As Single = (Me.Width - strdist.Width) / 2
            'g.FillRectangle(Brushes.White, X + strx, Y + CSng(Height + 5), strdist.Width, strdist.Height)
            g.DrawString(Me.Tag, New Font("Arial", 10, FontStyle.Bold, GraphicsUnit.Pixel, 0, False), New SolidBrush(Me.LineColor), X + strx, Y + Height + 5)

            Dim pgb1 As New PathGradientBrush(gp)
            pgb1.CenterColor = Me.GradientColor2
            'lgb1.SetBlendTriangularShape(0.5)
            pgb1.SurroundColors = New Color() {Me.GradientColor1}

            If Me.Fill Then
                If Me.GradientMode = False Then
                    g.FillPath(New SolidBrush(Me.FillColor), gp)
                Else
                    g.FillPath(pgb1, gp)
                End If
            End If

            Dim size As SizeF
            Dim fontA As New Font("Arial", 10, 3, GraphicsUnit.Pixel, 0, False)
            size = g.MeasureString("R", fontA)

            Dim ax, ay As Integer
            ax = Me.X + (Me.Width - size.Width) / 2
            ay = Me.Y + (Me.Height - size.Height) / 2

            g.SmoothingMode = SmoothingMode.AntiAlias
            g.TextRenderingHint = Text.TextRenderingHint.AntiAlias
            g.DrawString("R", fontA, Brushes.DarkBlue, ax, ay)

            gp.Dispose()
            g.EndContainer(gContainer)

        End Sub

    End Class

    <Serializable()> Public Class MaterialStreamGraphic

        Inherits ShapeGraphic

#Region "Constructors"
        Public Sub New()
            Me.TipoObjeto = GraphicObjects.TipoObjeto.MaterialStream
            Me.Description = "CorrentedeMatria"
        End Sub

        Public Sub New(ByVal graphicPosition As Point)
            Me.New()
            Me.SetPosition(graphicPosition)
        End Sub

        Public Sub New(ByVal posX As Integer, ByVal posY As Integer)
            Me.New(New Point(posX, posY))
        End Sub

        Public Sub New(ByVal graphicPosition As Point, ByVal graphicSize As Size)
            Me.New(graphicPosition)
            Me.SetSize(graphicSize)
            Me.AutoSize = False
        End Sub

        Public Sub New(ByVal posX As Integer, ByVal posY As Integer, ByVal graphicSize As Size)
            Me.New(New Point(posX, posY), graphicSize)
        End Sub

        Public Sub New(ByVal posX As Integer, ByVal posY As Integer, ByVal width As Integer, ByVal height As Integer)
            Me.New(New Point(posX, posY), New Size(width, height))
        End Sub

        Public Sub New(ByVal graphicPosition As Point, ByVal Rotation As Single)
            Me.New()
            Me.SetPosition(graphicPosition)
            Me.Rotation = Rotation
        End Sub

        Public Sub New(ByVal posX As Integer, ByVal posY As Integer, ByVal Rotation As Single)
            Me.New(New Point(posX, posY), Rotation)
        End Sub

        Public Sub New(ByVal graphicPosition As Point, ByVal graphicSize As Size, ByVal Rotation As Single)
            Me.New(graphicPosition, Rotation)
            Me.SetSize(graphicSize)
            Me.AutoSize = False
        End Sub

        Public Sub New(ByVal posX As Integer, ByVal posY As Integer, ByVal graphicSize As Size, ByVal Rotation As Single)
            Me.New(New Point(posX, posY), graphicSize, Rotation)
        End Sub

        Public Sub New(ByVal posX As Integer, ByVal posY As Integer, ByVal width As Integer, _
                               ByVal height As Integer, ByVal Rotation As Single)
            Me.New(New Point(posX, posY), New Size(width, height), Rotation)
        End Sub

#End Region

        Public Overrides Sub CreateConnectors(InCount As Integer, OutCount As Integer)

            Dim myIC1 As New ConnectionPoint
            myIC1.Position = New Point(X, Y + 0.5 * Height)
            myIC1.Type = ConType.ConIn

            Dim myOC1 As New ConnectionPoint
            myOC1.Position = New Point(X + Width, Y + 0.5 * Height)
            myOC1.Type = ConType.ConOut

            With InputConnectors

                If .Count <> 0 Then
                    If Me.FlippedH Then
                        .Item(0).Position = New Point(X + Width, Y + 0.5 * Height)
                    Else
                        .Item(0).Position = New Point(X, Y + 0.5 * Height)
                    End If
                Else
                    .Add(myIC1)
                End If

            End With

            With OutputConnectors

                If .Count <> 0 Then
                    If Me.FlippedH Then
                        .Item(0).Position = New Point(X, Y + 0.5 * Height)
                    Else
                        .Item(0).Position = New Point(X + Width, Y + 0.5 * Height)
                    End If
                Else
                    .Add(myOC1)
                End If

            End With
        End Sub

        Public Overrides Sub Draw(ByVal g As System.Drawing.Graphics)

            Dim gContainer As System.Drawing.Drawing2D.GraphicsContainer
            Dim myMatrix As Drawing2D.Matrix
            myMatrix = g.Transform()
            gContainer = g.BeginContainer()
            If m_Rotation <> 0 Then
                myMatrix.RotateAt(m_Rotation, New PointF(X, Y), Drawing.Drawing2D.MatrixOrder.Append)
                g.Transform = myMatrix
            End If

            CreateConnectors(0, 0)

            Dim pt As Point
            Dim raio, angulo As Double
            Dim con As ConnectionPoint
            For Each con In Me.InputConnectors
                pt = con.Position
                raio = ((pt.X - Me.X) ^ 2 + (pt.Y - Me.Y) ^ 2) ^ 0.5
                angulo = Math.Atan2(pt.Y - Me.Y, pt.X - Me.X)
                pt.X = Me.X + raio * Math.Cos(angulo + Me.Rotation / 360 * 2 * Math.PI)
                pt.Y = Me.Y + raio * Math.Sin(angulo + Me.Rotation / 360 * 2 * Math.PI)
                con.Position = pt
            Next
            For Each con In Me.OutputConnectors
                pt = con.Position
                raio = ((pt.X - Me.X) ^ 2 + (pt.Y - Me.Y) ^ 2) ^ 0.5
                angulo = Math.Atan2(pt.Y - Me.Y, pt.X - Me.X)
                pt.X = Me.X + raio * Math.Cos(angulo + Me.Rotation / 360 * 2 * Math.PI)
                pt.Y = Me.Y + raio * Math.Sin(angulo + Me.Rotation / 360 * 2 * Math.PI)
                con.Position = pt
            Next

            UpdateStatus(Me)

            Dim myPen As New Pen(Me.LineColor, Me.LineWidth)
            Dim myPen2 As New Pen(Color.White, 0)
            Dim rect As New Rectangle(X, Y, Width, Height)

            g.SmoothingMode = SmoothingMode.AntiAlias

            Dim gp As Drawing2D.GraphicsPath = New Drawing2D.GraphicsPath
            If Me.FlippedH = False Then
                gp.AddLine(CInt(X), CInt(Y + 0.35 * Height), CInt(X + 0.75 * Width), CInt(Y + 0.35 * Height))
                gp.AddLine(CInt(X + 0.75 * Width), CInt(Y + 0.35 * Height), CInt(X + 0.75 * Width), CInt(Y + 0.25 * Height))
                gp.AddLine(CInt(X + 0.75 * Width), CInt(Y + 0.25 * Height), CInt(X + Width), CInt(Y + 0.5 * Height))
                gp.AddLine(CInt(X + Width), CInt(Y + 0.5 * Height), CInt(X + 0.75 * Width), CInt(Y + 0.75 * Height))
                gp.AddLine(CInt(X + 0.75 * Width), CInt(Y + 0.75 * Height), CInt(X + 0.75 * Width), CInt(Y + 0.65 * Height))
                gp.AddLine(CInt(X + 0.75 * Width), CInt(Y + 0.65 * Height), CInt(X), CInt(Y + 0.65 * Height))
                gp.AddLine(CInt(X), CInt(Y + 0.65 * Height), CInt(X), CInt(Y + 0.35 * Height))
            Else
                gp.AddLine(CInt(X + Width), CInt(Y + 0.35 * Height), CInt(X + 0.25 * Width), CInt(Y + 0.35 * Height))
                gp.AddLine(CInt(X + 0.25 * Width), CInt(Y + 0.35 * Height), CInt(X + 0.25 * Width), CInt(Y + 0.25 * Height))
                gp.AddLine(CInt(X + 0.25 * Width), CInt(Y + 0.25 * Height), CInt(X), CInt(Y + 0.5 * Height))
                gp.AddLine(CInt(X), CInt(Y + 0.5 * Height), CInt(X + 0.25 * Width), CInt(Y + 0.75 * Height))
                gp.AddLine(CInt(X + 0.25 * Width), CInt(Y + 0.75 * Height), CInt(X + 0.25 * Width), CInt(Y + 0.65 * Height))
                gp.AddLine(CInt(X + 0.25 * Width), CInt(Y + 0.65 * Height), CInt(X + Width), CInt(Y + 0.65 * Height))
                gp.AddLine(CInt(X + Width), CInt(Y + 0.65 * Height), CInt(X + Width), CInt(Y + 0.35 * Height))
            End If


            gp.CloseFigure()

            g.DrawPath(myPen, Me.GetRoundedLine(gp.PathPoints, 1))

            Dim strdist As SizeF = g.MeasureString(Me.Tag, New Font("Arial", 10, FontStyle.Bold, GraphicsUnit.Pixel, 0, False), New PointF(0, 0), New StringFormat(StringFormatFlags.NoClip, 0))
            Dim strx As Single = (Me.Width - strdist.Width) / 2
            ''g.FillRectangle(Brushes.White, X + strx, Y + CSng(Height + 5), strdist.Width, strdist.Height)
            g.DrawString(Me.Tag, New Font("Arial", 10, FontStyle.Bold, GraphicsUnit.Pixel, 0, False), New SolidBrush(Me.LineColor), X + strx, Y + Height + 5)


            Dim pgb1 As New LinearGradientBrush(New PointF(X, Y + 0.25 * Height), New PointF(X, Y + 0.75 * Height), Me.GradientColor1, Me.GradientColor2)
            'pgb1.CenterColor = Me.GradientColor1
            'pgb1.SurroundColors = New Color() {Me.GradientColor2}

            If Me.Fill Then
                If Me.GradientMode = False Then
                    g.FillPath(New SolidBrush(Me.FillColor), gp)
                Else
                    g.FillPath(pgb1, gp)
                End If
            End If
            gp.Dispose()
            g.EndContainer(gContainer)

        End Sub

    End Class

    <Serializable()> Public Class EnergyStreamGraphic

        Inherits ShapeGraphic

#Region "Constructors"
        Public Sub New()
            Me.TipoObjeto = GraphicObjects.TipoObjeto.EnergyStream
            Me.IsEnergyStream = True
            Me.Description = "Correntedeenergia"
        End Sub

        Public Sub New(ByVal graphicPosition As Point)
            Me.New()
            Me.SetPosition(graphicPosition)
        End Sub

        Public Sub New(ByVal posX As Integer, ByVal posY As Integer)
            Me.New(New Point(posX, posY))
        End Sub

        Public Sub New(ByVal graphicPosition As Point, ByVal graphicSize As Size)
            Me.New(graphicPosition)
            Me.SetSize(graphicSize)
            Me.AutoSize = False
        End Sub

        Public Sub New(ByVal posX As Integer, ByVal posY As Integer, ByVal graphicSize As Size)
            Me.New(New Point(posX, posY), graphicSize)
        End Sub

        Public Sub New(ByVal posX As Integer, ByVal posY As Integer, ByVal width As Integer, ByVal height As Integer)
            Me.New(New Point(posX, posY), New Size(width, height))
        End Sub

        Public Sub New(ByVal graphicPosition As Point, ByVal Rotation As Single)
            Me.New()
            Me.SetPosition(graphicPosition)
            Me.Rotation = Rotation
        End Sub

        Public Sub New(ByVal posX As Integer, ByVal posY As Integer, ByVal Rotation As Single)
            Me.New(New Point(posX, posY), Rotation)
        End Sub

        Public Sub New(ByVal graphicPosition As Point, ByVal graphicSize As Size, ByVal Rotation As Single)
            Me.New(graphicPosition, Rotation)
            Me.SetSize(graphicSize)
            Me.AutoSize = False
        End Sub

        Public Sub New(ByVal posX As Integer, ByVal posY As Integer, ByVal graphicSize As Size, ByVal Rotation As Single)
            Me.New(New Point(posX, posY), graphicSize, Rotation)
        End Sub

        Public Sub New(ByVal posX As Integer, ByVal posY As Integer, ByVal width As Integer, _
                               ByVal height As Integer, ByVal Rotation As Single)
            Me.New(New Point(posX, posY), New Size(width, height), Rotation)
        End Sub

#End Region

        Public Overrides Sub CreateConnectors(InCount As Integer, OutCount As Integer)

            Dim myIC1 As New ConnectionPoint
            myIC1.Position = New Point(X, Y + 0.5 * Height)
            myIC1.Type = ConType.ConIn

            Dim myOC1 As New ConnectionPoint
            myOC1.Position = New Point(X + Width, Y + 0.5 * Height)
            myOC1.Type = ConType.ConOut

            With InputConnectors

                If .Count <> 0 Then
                    If Me.FlippedH Then
                        .Item(0).Position = New Point(X + Width, Y + 0.5 * Height)
                    Else
                        .Item(0).Position = New Point(X, Y + 0.5 * Height)
                    End If
                Else
                    .Add(myIC1)
                End If

            End With

            With OutputConnectors

                If .Count <> 0 Then
                    If Me.FlippedH Then
                        .Item(0).Position = New Point(X, Y + 0.5 * Height)
                    Else
                        .Item(0).Position = New Point(X + Width, Y + 0.5 * Height)
                    End If
                Else
                    .Add(myOC1)
                End If

            End With

        End Sub

        Public Overrides Sub Draw(ByVal g As System.Drawing.Graphics)

            CreateConnectors(0, 0)

            UpdateStatus(Me)

            Dim pt As Point
            Dim raio, angulo As Double
            Dim con As ConnectionPoint
            For Each con In Me.InputConnectors
                pt = con.Position
                raio = ((pt.X - Me.X) ^ 2 + (pt.Y - Me.Y) ^ 2) ^ 0.5
                angulo = Math.Atan2(pt.Y - Me.Y, pt.X - Me.X)
                pt.X = Me.X + raio * Math.Cos(angulo + Me.Rotation / 360 * 2 * Math.PI)
                pt.Y = Me.Y + raio * Math.Sin(angulo + Me.Rotation / 360 * 2 * Math.PI)
                con.Position = pt
            Next
            For Each con In Me.OutputConnectors
                pt = con.Position
                raio = ((pt.X - Me.X) ^ 2 + (pt.Y - Me.Y) ^ 2) ^ 0.5
                angulo = Math.Atan2(pt.Y - Me.Y, pt.X - Me.X)
                pt.X = Me.X + raio * Math.Cos(angulo + Me.Rotation / 360 * 2 * Math.PI)
                pt.Y = Me.Y + raio * Math.Sin(angulo + Me.Rotation / 360 * 2 * Math.PI)
                con.Position = pt
            Next

            Dim gContainer As System.Drawing.Drawing2D.GraphicsContainer
            Dim myMatrix As Drawing2D.Matrix
            gContainer = g.BeginContainer()
            myMatrix = g.Transform()
            If m_Rotation <> 0 Then
                myMatrix.RotateAt(m_Rotation, New PointF(X, Y), Drawing.Drawing2D.MatrixOrder.Append)
                g.Transform = myMatrix
            End If


            Dim myPen As New Pen(Me.LineColor, Me.LineWidth)
            Dim myPen2 As New Pen(Color.White, 0)
            Dim rect As New Rectangle(X, Y, Width, Height)

            'g.DrawRectangle(myPen2, rect)

            Dim gp As Drawing2D.GraphicsPath = New Drawing2D.GraphicsPath
            If Me.FlippedH = False Then
                gp.AddLine(CInt(X), CInt(Y + 0.35 * Height), CInt(X + 0.75 * Width), CInt(Y + 0.35 * Height))
                gp.AddLine(CInt(X + 0.75 * Width), CInt(Y + 0.35 * Height), CInt(X + 0.75 * Width), CInt(Y + 0.25 * Height))
                gp.AddLine(CInt(X + 0.75 * Width), CInt(Y + 0.25 * Height), CInt(X + Width), CInt(Y + 0.5 * Height))
                gp.AddLine(CInt(X + Width), CInt(Y + 0.5 * Height), CInt(X + 0.75 * Width), CInt(Y + 0.75 * Height))
                gp.AddLine(CInt(X + 0.75 * Width), CInt(Y + 0.75 * Height), CInt(X + 0.75 * Width), CInt(Y + 0.65 * Height))
                gp.AddLine(CInt(X + 0.75 * Width), CInt(Y + 0.65 * Height), CInt(X), CInt(Y + 0.65 * Height))
                gp.AddLine(CInt(X), CInt(Y + 0.65 * Height), CInt(X), CInt(Y + 0.35 * Height))
            Else
                gp.AddLine(CInt(X + Width), CInt(Y + 0.35 * Height), CInt(X + 0.25 * Width), CInt(Y + 0.35 * Height))
                gp.AddLine(CInt(X + 0.25 * Width), CInt(Y + 0.35 * Height), CInt(X + 0.25 * Width), CInt(Y + 0.25 * Height))
                gp.AddLine(CInt(X + 0.25 * Width), CInt(Y + 0.25 * Height), CInt(X), CInt(Y + 0.5 * Height))
                gp.AddLine(CInt(X), CInt(Y + 0.5 * Height), CInt(X + 0.25 * Width), CInt(Y + 0.75 * Height))
                gp.AddLine(CInt(X + 0.25 * Width), CInt(Y + 0.75 * Height), CInt(X + 0.25 * Width), CInt(Y + 0.65 * Height))
                gp.AddLine(CInt(X + 0.25 * Width), CInt(Y + 0.65 * Height), CInt(X + Width), CInt(Y + 0.65 * Height))
                gp.AddLine(CInt(X + Width), CInt(Y + 0.65 * Height), CInt(X + Width), CInt(Y + 0.35 * Height))
            End If

            gp.CloseFigure()

            g.SmoothingMode = SmoothingMode.AntiAlias
            g.DrawPath(myPen, gp)
            Dim strdist As SizeF = g.MeasureString(Me.Tag, New Font("Arial", 10, FontStyle.Bold, GraphicsUnit.Pixel, 0, False), New PointF(0, 0), New StringFormat(StringFormatFlags.NoClip, 0))
            Dim strx As Single = (Me.Width - strdist.Width) / 2
            'g.FillRectangle(Brushes.White, X + strx, Y + CSng(Height + 5), strdist.Width, strdist.Height)
            'g.FillRectangle(Brushes.White, X + strx, Y + CSng(Height + 5), strdist.Width, strdist.Height)
            g.DrawString(Me.Tag, New Font("Arial", 10, FontStyle.Bold, GraphicsUnit.Pixel, 0, False), New SolidBrush(Me.LineColor), X + strx, Y + Height + 5)

            If Me.Fill Then
                g.FillPath(New SolidBrush(Me.FillColor), gp)
            End If
            gp.Dispose()
            g.EndContainer(gContainer)

        End Sub

    End Class

    <Serializable()> Public Class ValveGraphic

        Inherits ShapeGraphic

#Region "Constructors"
        Public Sub New()
            Me.TipoObjeto = GraphicObjects.TipoObjeto.Valve
            Me.Description = "Vlvula"
        End Sub

        Public Sub New(ByVal graphicPosition As Point)
            Me.New()
            Me.SetPosition(graphicPosition)
        End Sub

        Public Sub New(ByVal posX As Integer, ByVal posY As Integer)
            Me.New(New Point(posX, posY))
        End Sub

        Public Sub New(ByVal graphicPosition As Point, ByVal graphicSize As Size)
            Me.New(graphicPosition)
            Me.SetSize(graphicSize)
            Me.AutoSize = False
        End Sub

        Public Sub New(ByVal posX As Integer, ByVal posY As Integer, ByVal graphicSize As Size)
            Me.New(New Point(posX, posY), graphicSize)
        End Sub

        Public Sub New(ByVal posX As Integer, ByVal posY As Integer, ByVal width As Integer, ByVal height As Integer)
            Me.New(New Point(posX, posY), New Size(width, height))
        End Sub

        Public Sub New(ByVal graphicPosition As Point, ByVal Rotation As Single)
            Me.New()
            Me.SetPosition(graphicPosition)
            Me.Rotation = Rotation
        End Sub

        Public Sub New(ByVal posX As Integer, ByVal posY As Integer, ByVal Rotation As Single)
            Me.New(New Point(posX, posY), Rotation)
        End Sub

        Public Sub New(ByVal graphicPosition As Point, ByVal graphicSize As Size, ByVal Rotation As Single)
            Me.New(graphicPosition, Rotation)
            Me.SetSize(graphicSize)
            Me.AutoSize = False
        End Sub

        Public Sub New(ByVal posX As Integer, ByVal posY As Integer, ByVal graphicSize As Size, ByVal Rotation As Single)
            Me.New(New Point(posX, posY), graphicSize, Rotation)
        End Sub

        Public Sub New(ByVal posX As Integer, ByVal posY As Integer, ByVal width As Integer, _
                               ByVal height As Integer, ByVal Rotation As Single)
            Me.New(New Point(posX, posY), New Size(width, height), Rotation)
        End Sub

#End Region

        Public Overrides Sub CreateConnectors(InCount As Integer, OutCount As Integer)

            Dim myIC1 As New ConnectionPoint
            myIC1.Position = New Point(X, Y + 0.5 * Height)
            myIC1.Type = ConType.ConIn

            Dim myOC1 As New ConnectionPoint
            myOC1.Position = New Point(X + Width, Y + 0.5 * Height)
            myOC1.Type = ConType.ConOut

            Me.EnergyConnector.Position = New Point(X + 0.5 * Width, Y + Height)
            Me.EnergyConnector.Type = ConType.ConEn

            With InputConnectors

                If .Count <> 0 Then
                    If Me.FlippedH Then
                        .Item(0).Position = New Point(X + Width, Y + 0.5 * Height)
                    Else
                        .Item(0).Position = New Point(X, Y + 0.5 * Height)
                    End If
                Else
                    .Add(myIC1)
                End If

            End With

            With OutputConnectors

                If .Count <> 0 Then
                    If Me.FlippedH Then
                        .Item(0).Position = New Point(X, Y + 0.5 * Height)
                    Else
                        .Item(0).Position = New Point(X + Width, Y + 0.5 * Height)
                    End If
                Else
                    .Add(myOC1)
                End If

            End With

        End Sub

        Public Overrides Sub Draw(ByVal g As System.Drawing.Graphics)

            CreateConnectors(0, 0)

            UpdateStatus(Me)

            Dim pt As Point
            Dim raio, angulo As Double
            Dim con As ConnectionPoint
            For Each con In Me.InputConnectors
                pt = con.Position
                raio = ((pt.X - Me.X) ^ 2 + (pt.Y - Me.Y) ^ 2) ^ 0.5
                angulo = Math.Atan2(pt.Y - Me.Y, pt.X - Me.X)
                pt.X = Me.X + raio * Math.Cos(angulo + Me.Rotation / 360 * 2 * Math.PI)
                pt.Y = Me.Y + raio * Math.Sin(angulo + Me.Rotation / 360 * 2 * Math.PI)
                con.Position = pt
            Next
            For Each con In Me.OutputConnectors
                pt = con.Position
                raio = ((pt.X - Me.X) ^ 2 + (pt.Y - Me.Y) ^ 2) ^ 0.5
                angulo = Math.Atan2(pt.Y - Me.Y, pt.X - Me.X)
                pt.X = Me.X + raio * Math.Cos(angulo + Me.Rotation / 360 * 2 * Math.PI)
                pt.Y = Me.Y + raio * Math.Sin(angulo + Me.Rotation / 360 * 2 * Math.PI)
                con.Position = pt
            Next
            With Me.EnergyConnector
                pt = .Position
                raio = ((pt.X - Me.X) ^ 2 + (pt.Y - Me.Y) ^ 2) ^ 0.5
                angulo = Math.Atan2(pt.Y - Me.Y, pt.X - Me.X)
                pt.X = Me.X + raio * Math.Cos(angulo + Me.Rotation / 360 * 2 * Math.PI)
                pt.Y = Me.Y + raio * Math.Sin(angulo + Me.Rotation / 360 * 2 * Math.PI)
                .Position = pt
            End With

            Dim gContainer As System.Drawing.Drawing2D.GraphicsContainer
            Dim myMatrix As Drawing2D.Matrix
            gContainer = g.BeginContainer()
            myMatrix = g.Transform()
            If m_Rotation <> 0 Then
                myMatrix.RotateAt(m_Rotation, New PointF(X, Y), Drawing.Drawing2D.MatrixOrder.Append)
                g.Transform = myMatrix
            End If


            Dim myPen As New Pen(Me.LineColor, Me.LineWidth)
            Dim myPen2 As New Pen(Color.White, 0)
            Dim rect As New Rectangle(X, Y, Width, Height)

            g.SmoothingMode = SmoothingMode.AntiAlias
            'g.DrawRectangle(myPen2, rect)

            Dim gp As Drawing2D.GraphicsPath = New Drawing2D.GraphicsPath
            gp.AddLine(CInt(X), CInt(Y + 0.2 * Height), CInt(X + 0.5 * Width), CInt(Y + 0.5 * Height))
            gp.AddLine(CInt(X + 0.5 * Width), CInt(Y + 0.5 * Height), CInt(X + Width), CInt(Y + 0.2 * Height))
            gp.AddLine(CInt(X + Width), CInt(Y + 0.2 * Height), CInt(X + Width), CInt(Y + 0.8 * Height))
            gp.AddLine(CInt(X + Width), CInt(Y + 0.8 * Height), CInt(X + 0.5 * Width), CInt(Y + 0.5 * Height))
            gp.AddLine(CInt(X + 0.5 * Width), CInt(Y + 0.5 * Height), CInt(X), CInt(Y + 0.8 * Height))
            gp.AddLine(CInt(X), CInt(Y + 0.8 * Height), CInt(X), CInt(Y + 0.2 * Height))

            gp.CloseFigure()

            g.DrawPath(myPen, Me.GetRoundedLine(gp.PathPoints, 1))

            Dim strdist As SizeF = g.MeasureString(Me.Tag, New Font("Arial", 10, FontStyle.Bold, GraphicsUnit.Pixel, 0, False), New PointF(0, 0), New StringFormat(StringFormatFlags.NoClip, 0))
            Dim strx As Single = (Me.Width - strdist.Width) / 2
            'g.FillRectangle(Brushes.White, X + strx, Y + CSng(Height + 5), strdist.Width, strdist.Height)
            g.DrawString(Me.Tag, New Font("Arial", 10, FontStyle.Bold, GraphicsUnit.Pixel, 0, False), New SolidBrush(Me.LineColor), X + strx, Y + Height + 5)

            Dim pgb1 As New PathGradientBrush(gp)
            pgb1.CenterColor = Me.GradientColor1
            'lgb1.SetBlendTriangularShape(0.5)
            pgb1.SurroundColors = New Color() {Me.GradientColor2}

            If Me.Fill Then
                If Me.GradientMode = False Then
                    g.FillPath(New SolidBrush(Me.FillColor), gp)
                Else
                    g.FillPath(pgb1, gp)
                End If
            End If
            gp.Dispose()
            g.EndContainer(gContainer)

        End Sub

    End Class

    <Serializable()> Public Class TurbineGraphic

        Inherits ShapeGraphic

#Region "Constructors"
        Public Sub New()
            Me.TipoObjeto = GraphicObjects.TipoObjeto.Expander
            Me.Description = "TurbinaAdiabtica"
        End Sub

        Public Sub New(ByVal graphicPosition As Point)
            Me.New()
            Me.SetPosition(graphicPosition)
        End Sub

        Public Sub New(ByVal posX As Integer, ByVal posY As Integer)
            Me.New(New Point(posX, posY))
        End Sub

        Public Sub New(ByVal graphicPosition As Point, ByVal graphicSize As Size)
            Me.New(graphicPosition)
            Me.SetSize(graphicSize)
            Me.AutoSize = False
        End Sub

        Public Sub New(ByVal posX As Integer, ByVal posY As Integer, ByVal graphicSize As Size)
            Me.New(New Point(posX, posY), graphicSize)
        End Sub

        Public Sub New(ByVal posX As Integer, ByVal posY As Integer, ByVal width As Integer, ByVal height As Integer)
            Me.New(New Point(posX, posY), New Size(width, height))
        End Sub

        Public Sub New(ByVal graphicPosition As Point, ByVal Rotation As Single)
            Me.New()
            Me.SetPosition(graphicPosition)
            Me.Rotation = Rotation
        End Sub

        Public Sub New(ByVal posX As Integer, ByVal posY As Integer, ByVal Rotation As Single)
            Me.New(New Point(posX, posY), Rotation)
        End Sub

        Public Sub New(ByVal graphicPosition As Point, ByVal graphicSize As Size, ByVal Rotation As Single)
            Me.New(graphicPosition, Rotation)
            Me.SetSize(graphicSize)
            Me.AutoSize = False
        End Sub

        Public Sub New(ByVal posX As Integer, ByVal posY As Integer, ByVal graphicSize As Size, ByVal Rotation As Single)
            Me.New(New Point(posX, posY), graphicSize, Rotation)
        End Sub

        Public Sub New(ByVal posX As Integer, ByVal posY As Integer, ByVal width As Integer, _
                               ByVal height As Integer, ByVal Rotation As Single)
            Me.New(New Point(posX, posY), New Size(width, height), Rotation)
        End Sub

#End Region


        Public Overrides Sub CreateConnectors(InCount As Integer, OutCount As Integer)
            Dim myIC1 As New ConnectionPoint
            myIC1.Position = New Point(X, Y + 0.5 * Height)
            myIC1.Type = ConType.ConIn

            Dim myOC1 As New ConnectionPoint
            myOC1.Position = New Point(X + Width, Y + 0.5 * Height)
            myOC1.Type = ConType.ConOut

            Me.EnergyConnector.Position = New Point(X + 0.5 * Width, Y + Height)
            Me.EnergyConnector.Type = ConType.ConEn

            With InputConnectors

                If .Count = 2 Then
                    If Me.FlippedH Then
                        .Item(0).Position = New Point(X + Width, Y + 0.5 * Height)
                    Else
                        .Item(0).Position = New Point(X, Y + 0.5 * Height)
                    End If
                Else
                    .Add(myIC1)
                End If

            End With

            With OutputConnectors

                If .Count <> 0 Then
                    If Me.FlippedH Then
                        .Item(0).Position = New Point(X, Y + 0.5 * Height)
                    Else
                        .Item(0).Position = New Point(X + Width, Y + 0.5 * Height)
                    End If
                Else
                    .Add(myOC1)
                End If

            End With

        End Sub

        Public Overrides Sub Draw(ByVal g As System.Drawing.Graphics)

            CreateConnectors(0, 0)

            UpdateStatus(Me)

            Dim pt As Point
            Dim raio, angulo As Double
            Dim con As ConnectionPoint
            For Each con In Me.InputConnectors
                pt = con.Position
                raio = ((pt.X - Me.X) ^ 2 + (pt.Y - Me.Y) ^ 2) ^ 0.5
                angulo = Math.Atan2(pt.Y - Me.Y, pt.X - Me.X)
                pt.X = Me.X + raio * Math.Cos(angulo + Me.Rotation / 360 * 2 * Math.PI)
                pt.Y = Me.Y + raio * Math.Sin(angulo + Me.Rotation / 360 * 2 * Math.PI)
                con.Position = pt
            Next
            For Each con In Me.OutputConnectors
                pt = con.Position
                raio = ((pt.X - Me.X) ^ 2 + (pt.Y - Me.Y) ^ 2) ^ 0.5
                angulo = Math.Atan2(pt.Y - Me.Y, pt.X - Me.X)
                pt.X = Me.X + raio * Math.Cos(angulo + Me.Rotation / 360 * 2 * Math.PI)
                pt.Y = Me.Y + raio * Math.Sin(angulo + Me.Rotation / 360 * 2 * Math.PI)
                con.Position = pt
            Next
            With Me.EnergyConnector
                pt = .Position
                raio = ((pt.X - Me.X) ^ 2 + (pt.Y - Me.Y) ^ 2) ^ 0.5
                angulo = Math.Atan2(pt.Y - Me.Y, pt.X - Me.X)
                pt.X = Me.X + raio * Math.Cos(angulo + Me.Rotation / 360 * 2 * Math.PI)
                pt.Y = Me.Y + raio * Math.Sin(angulo + Me.Rotation / 360 * 2 * Math.PI)
                .Position = pt
            End With

            Dim gContainer As System.Drawing.Drawing2D.GraphicsContainer
            Dim myMatrix As Drawing2D.Matrix
            gContainer = g.BeginContainer()
            myMatrix = g.Transform()
            If m_Rotation <> 0 Then
                myMatrix.RotateAt(m_Rotation, New PointF(X, Y), Drawing.Drawing2D.MatrixOrder.Append)
                g.Transform = myMatrix
            End If


            Dim myPen As New Pen(Me.LineColor, Me.LineWidth)
            Dim myPen2 As New Pen(Color.White, 0)
            Dim rect As New Rectangle(X, Y, Width, Height)

            g.SmoothingMode = SmoothingMode.AntiAlias
            'g.DrawRectangle(myPen2, rect)

            Dim strdist As SizeF = g.MeasureString(Me.Tag, New Font("Arial", 10, FontStyle.Bold, GraphicsUnit.Pixel, 0, False), New PointF(0, 0), New StringFormat(StringFormatFlags.NoClip, 0))
            Dim strx As Single = (Me.Width - strdist.Width) / 2
            'g.FillRectangle(Brushes.White, X + strx, Y + CSng(Height + 5), strdist.Width, strdist.Height)
            g.DrawString(Me.Tag, New Font("Arial", 10, FontStyle.Bold, GraphicsUnit.Pixel, 0, False), New SolidBrush(Me.LineColor), X + strx, Y + Height + 5)

            Dim gp As Drawing2D.GraphicsPath = New Drawing2D.GraphicsPath
            If Me.FlippedH = False Then
                gp.AddLine(CInt(X), CInt(Y + 0.3 * Height), CInt(X), CInt(Y + 0.7 * Height))
                gp.AddLine(CInt(X), CInt(Y + 0.7 * Height), CInt(X + Width), CInt(Y + Height))
                gp.AddLine(CInt(X + Width), CInt(Y + Height), CInt(X + Width), CInt(Y))
                gp.AddLine(CInt(X + Width), CInt(Y), CInt(X), CInt(Y + 0.3 * Height))
            Else
                gp.AddLine(CInt(X + Width), CInt(Y + 0.3 * Height), CInt(X + Width), CInt(Y + 0.7 * Height))
                gp.AddLine(CInt(X + Width), CInt(Y + 0.7 * Height), CInt(X), CInt(Y + Height))
                gp.AddLine(CInt(X), CInt(Y + Height), CInt(X), CInt(Y))
                gp.AddLine(CInt(X), CInt(Y), CInt(X + Width), CInt(Y + 0.3 * Height))
            End If


            gp.CloseFigure()

            g.DrawPath(myPen, Me.GetRoundedLine(gp.PathPoints, 1))

            Dim pgb1 As New LinearGradientBrush(New PointF(X, Y), New PointF(X, Y + Height), Me.GradientColor1, Me.GradientColor2)
            'pgb1.CenterColor = Me.GradientColor1
            'lgb1.SetBlendTriangularShape(0.5)
            'pgb1.SurroundColors = New Color() {Me.GradientColor2}

            If Me.Fill Then
                If Me.GradientMode = False Then
                    g.FillPath(New SolidBrush(Me.FillColor), gp)
                Else
                    g.FillPath(pgb1, gp)
                End If
            End If


            gp.Dispose()
            g.EndContainer(gContainer)

        End Sub

    End Class

    <Serializable()> Public Class TPVesselGraphic

        Inherits ShapeGraphic

#Region "Constructors"
        Public Sub New()
            Me.TipoObjeto = GraphicObjects.TipoObjeto.TPVessel
            Me.Description = "VasoSeparadorGL"
        End Sub

        Public Sub New(ByVal graphicPosition As Point)
            Me.New()
            Me.SetPosition(graphicPosition)
        End Sub

        Public Sub New(ByVal posX As Integer, ByVal posY As Integer)
            Me.New(New Point(posX, posY))
        End Sub

        Public Sub New(ByVal graphicPosition As Point, ByVal graphicSize As Size)
            Me.New(graphicPosition)
            Me.SetSize(graphicSize)
            Me.AutoSize = False
        End Sub

        Public Sub New(ByVal posX As Integer, ByVal posY As Integer, ByVal graphicSize As Size)
            Me.New(New Point(posX, posY), graphicSize)
        End Sub

        Public Sub New(ByVal posX As Integer, ByVal posY As Integer, ByVal width As Integer, ByVal height As Integer)
            Me.New(New Point(posX, posY), New Size(width, height))
        End Sub

        Public Sub New(ByVal graphicPosition As Point, ByVal Rotation As Single)
            Me.New()
            Me.SetPosition(graphicPosition)
            Me.Rotation = Rotation
        End Sub

        Public Sub New(ByVal posX As Integer, ByVal posY As Integer, ByVal Rotation As Single)
            Me.New(New Point(posX, posY), Rotation)
        End Sub

        Public Sub New(ByVal graphicPosition As Point, ByVal graphicSize As Size, ByVal Rotation As Single)
            Me.New(graphicPosition, Rotation)
            Me.SetSize(graphicSize)
            Me.AutoSize = False
        End Sub

        Public Sub New(ByVal posX As Integer, ByVal posY As Integer, ByVal graphicSize As Size, ByVal Rotation As Single)
            Me.New(New Point(posX, posY), graphicSize, Rotation)
        End Sub

        Public Sub New(ByVal posX As Integer, ByVal posY As Integer, ByVal width As Integer, _
                               ByVal height As Integer, ByVal Rotation As Single)
            Me.New(New Point(posX, posY), New Size(width, height), Rotation)
        End Sub

#End Region


        Public Overrides Sub CreateConnectors(InCount As Integer, OutCount As Integer)

            Dim myIC1 As New ConnectionPoint
            myIC1.Position = New Point(X + 0.125 * Width, Y + 0.5 * Height)
            myIC1.Type = ConType.ConIn

            Dim myOC1 As New ConnectionPoint
            myOC1.Position = New Point(X + 0.7 * Width, Y + (0.1 + 0.127 / 2) * Height)
            myOC1.Type = ConType.ConOut

            Dim myOC2 As New ConnectionPoint
            myOC2.Position = New Point(X + 0.7 * Width, Y + (0.773 + 0.127 / 2) * Height)
            myOC2.Type = ConType.ConOut

            Dim myOC3 As New ConnectionPoint
            myOC3.Position = New Point(X + 0.7 * Width, Y + 0.5 * Height)
            myOC3.Type = ConType.ConOut

            Me.EnergyConnector.Position = New Point(X + 0.5 * Width, Y + Height)
            Me.EnergyConnector.Type = ConType.ConEn

            With InputConnectors

                If .Count <> 0 Then
                    .Item(0).Position = New Point(X + 0.125 * Width, Y + 0.5 * Height)
                Else
                    .Add(myIC1)
                End If

            End With

            With OutputConnectors

                If .Count <> 0 Then
                    .Item(0).Position = New Point(X + 0.7 * Width, Y + (0.1 + 0.127 / 2) * Height)
                    .Item(1).Position = New Point(X + 0.7 * Width, Y + (0.773 + 0.127 / 2) * Height)
                    .Item(2).Position = New Point(X + 0.7 * Width, Y + 0.5 * Height)
                Else
                    .Add(myOC1)
                    .Add(myOC2)
                    .Add(myOC3)
                End If

            End With

        End Sub
        Public Overrides Sub Draw(ByVal g As System.Drawing.Graphics)

            UpdateStatus(Me)

            CreateConnectors(0, 0)

            Dim pt As Point
            Dim raio, angulo As Double
            Dim con As ConnectionPoint
            For Each con In Me.InputConnectors
                pt = con.Position
                raio = ((pt.X - Me.X) ^ 2 + (pt.Y - Me.Y) ^ 2) ^ 0.5
                angulo = Math.Atan2(pt.Y - Me.Y, pt.X - Me.X)
                pt.X = Me.X + raio * Math.Cos(angulo + Me.Rotation / 360 * 2 * Math.PI)
                pt.Y = Me.Y + raio * Math.Sin(angulo + Me.Rotation / 360 * 2 * Math.PI)
                con.Position = pt
            Next
            For Each con In Me.OutputConnectors
                pt = con.Position
                raio = ((pt.X - Me.X) ^ 2 + (pt.Y - Me.Y) ^ 2) ^ 0.5
                angulo = Math.Atan2(pt.Y - Me.Y, pt.X - Me.X)
                pt.X = Me.X + raio * Math.Cos(angulo + Me.Rotation / 360 * 2 * Math.PI)
                pt.Y = Me.Y + raio * Math.Sin(angulo + Me.Rotation / 360 * 2 * Math.PI)
                con.Position = pt
            Next
            With Me.EnergyConnector
                pt = .Position
                raio = ((pt.X - Me.X) ^ 2 + (pt.Y - Me.Y) ^ 2) ^ 0.5
                angulo = Math.Atan2(pt.Y - Me.Y, pt.X - Me.X)
                pt.X = Me.X + raio * Math.Cos(angulo + Me.Rotation / 360 * 2 * Math.PI)
                pt.Y = Me.Y + raio * Math.Sin(angulo + Me.Rotation / 360 * 2 * Math.PI)
                .Position = pt
            End With

            Dim gContainer As System.Drawing.Drawing2D.GraphicsContainer
            Dim myMatrix As Drawing2D.Matrix
            gContainer = g.BeginContainer()
            myMatrix = g.Transform()
            If m_Rotation <> 0 Then
                myMatrix.RotateAt(m_Rotation, New PointF(X, Y), Drawing.Drawing2D.MatrixOrder.Append)
                g.Transform = myMatrix
            End If

            Dim rect2 As New Rectangle(X + 0.123 * Width, Y + 0.5 * Height, 0.127 * Width, 0.127 * Height)
            Dim rect3 As New Rectangle(X + 0.7 * Width, Y + 0.1 * Height, 0.127 * Width, 0.127 * Height)
            Dim rect4 As New Rectangle(X + 0.7 * Width, Y + 0.773 * Height, 0.127 * Width, 0.127 * Height)

            Dim myPen As New Pen(Me.LineColor, Me.LineWidth)

            Dim myPen2 As New Pen(Color.White, 0)

            Dim rect As New Rectangle(X, Y, Width, Height)

            g.SmoothingMode = SmoothingMode.AntiAlias
            'g.DrawRectangle(myPen2, rect)
            Me.DrawRoundRect(g, myPen, X + 0.25 * Width, Y, 0.45 * Width, Height, 10, Brushes.Transparent)
            g.DrawRectangle(myPen, rect2)
            g.DrawRectangle(myPen, rect3)
            g.DrawRectangle(myPen, rect4)

            Dim strdist As SizeF = g.MeasureString(Me.Tag, New Font("Arial", 10, FontStyle.Bold, GraphicsUnit.Pixel, 0, False), New PointF(0, 0), New StringFormat(StringFormatFlags.NoClip, 0))
            Dim strx As Single = (Me.Width - strdist.Width) / 2
            'g.FillRectangle(Brushes.White, X + strx, Y + CSng(Height + 5), strdist.Width, strdist.Height)
            g.DrawString(Me.Tag, New Font("Arial", 10, FontStyle.Bold, GraphicsUnit.Pixel, 0, False), New SolidBrush(Me.LineColor), X + strx, Y + Height + 5)

            Dim gp As Drawing2D.GraphicsPath = New Drawing2D.GraphicsPath
            Dim radius As Integer = 3
            gp.AddLine(X + radius, Y, X + Width - radius, Y)
            gp.AddArc(X + Width - radius, Y, radius, radius, 270, 90)
            gp.AddLine(X + Width, Y + radius, X + Width, Y + Height - radius)
            gp.AddArc(X + Width - radius, Y + Height - radius, radius, radius, 0, 90)
            gp.AddLine(X + Width - radius, Y + Height, X + radius, Y + Height)
            gp.AddArc(X, Y + Height - radius, radius, radius, 90, 90)
            gp.AddLine(X, Y + Height - radius, X, Y + radius)
            gp.AddArc(X, Y, radius, radius, 180, 90)
            Dim lgb1 As New PathGradientBrush(gp)
            lgb1.CenterColor = Me.GradientColor1
            'lgb1.SetBlendTriangularShape(0.5)
            lgb1.SurroundColors = New Color() {Me.GradientColor2}
            If Me.Fill Then
                If Me.GradientMode = False Then
                    g.FillRectangle(New SolidBrush(Me.FillColor), rect3)
                    g.FillRectangle(New SolidBrush(Me.FillColor), rect4)
                    g.FillRectangle(New SolidBrush(Me.FillColor), rect2)
                    Me.DrawRoundRect(g, myPen, X + 0.25 * Width, Y, 0.45 * Width, Height, 3, New SolidBrush(Me.FillColor))
                Else
                    g.FillRectangle(New SolidBrush(Me.FillColor), rect3)
                    g.FillRectangle(New SolidBrush(Me.FillColor), rect4)
                    g.FillRectangle(New SolidBrush(Me.FillColor), rect2)
                    Me.DrawRoundRect(g, myPen, X + 0.25 * Width, Y, 0.45 * Width, Height, 3, lgb1)
                End If
            End If

            g.EndContainer(gContainer)
            gp.Dispose()

        End Sub

        Public Sub DrawRoundRect(ByVal g As Graphics, ByVal p As Pen, ByVal x As Integer, ByVal y As Integer, ByVal width As Integer, ByVal height As Integer, ByVal radius As Integer, ByVal myBrush As Brush)

            Dim gp As Drawing2D.GraphicsPath = New Drawing2D.GraphicsPath

            gp.AddLine(x + radius, y, x + width - radius, y)
            gp.AddArc(x + width - radius, y, radius, radius, 270, 90)
            gp.AddLine(x + width, y + radius, x + width, y + height - radius)
            gp.AddArc(x + width - radius, y + height - radius, radius, radius, 0, 90)
            gp.AddLine(x + width - radius, y + height, x + radius, y + height)
            gp.AddArc(x, y + height - radius, radius, radius, 90, 90)
            gp.AddLine(x, y + height - radius, x, y + radius)
            gp.AddArc(x, y, radius, radius, 180, 90)

            gp.CloseFigure()

            g.DrawPath(p, gp)
            g.FillPath(myBrush, gp)

            gp.Dispose()

        End Sub

    End Class

    <Serializable()> Public Class ConnectToolGraphic

        Inherits ShapeGraphic

        Protected m_AttFrom As GraphicObject = Nothing
        Protected m_AttTo As GraphicObject = Nothing
        Protected m_AttFromIndex As Integer
        Protected m_AttToIndex As Integer

        Protected m_AttFromEn As Boolean = False
        Protected m_AttToEn As Boolean = False

#Region "Constructors"
        Public Sub New()
        End Sub

        Public Sub New(ByVal startPosition As Point)
            Me.New()
            Me.SetStartPosition(startPosition)
            Me.IsConnector = True
        End Sub

        Public Sub New(ByVal posX As Integer, ByVal posY As Integer)
            Me.New(New Point(posX, posY))
            Me.IsConnector = True
        End Sub

        Public Sub New(ByVal startPosition As Point, ByVal endPosition As Point)
            Me.New(startPosition)
            Me.SetEndPosition(endPosition)
            Me.AutoSize = False
            Me.IsConnector = True
        End Sub

        Public Sub New(ByVal startX As Integer, ByVal startY As Integer, ByVal endPosition As Point)
            Me.New(New Point(startX, startY), endPosition)
            Me.IsConnector = True
        End Sub

        Public Sub New(ByVal startX As Integer, ByVal startY As Integer, ByVal endX As Integer, ByVal endY As Integer)
            Me.New(New Point(startX, startY), New Point(endX, endY))
            Me.IsConnector = True
        End Sub

        Public Sub New(ByVal startPosition As Point, ByVal endPosition As Point, ByVal lineWidth As Single, ByVal lineColor As Color)
            Me.New(startPosition)
            Me.SetEndPosition(endPosition)
            Me.LineWidth = lineWidth
            Me.LineColor = lineColor
            Me.AutoSize = False
            Me.IsConnector = True
        End Sub

        Public Sub New(ByVal startX As Integer, ByVal startY As Integer, ByVal endX As Integer, ByVal endY As Integer, ByVal lineWidth As Single, ByVal lineColor As Color)
            Me.New(New Point(startX, startY), New Point(endX, endY))
            Me.LineWidth = lineWidth
            Me.LineColor = lineColor
            Me.AutoSize = False
            Me.IsConnector = True
        End Sub

#End Region

        Public Overloads Overrides Function HitTest(ByVal pt As System.Drawing.Point) As Boolean
            Dim gp As New Drawing2D.GraphicsPath()
            Dim myMatrix As New Drawing2D.Matrix()
            Dim myPen As New Pen(Me.LineColor, Me.LineWidth)
            gp.AddLine(X, Y, X + Width, Y + Height)
            myMatrix.RotateAt(Me.Rotation, New PointF(Me.X, Me.Y), Drawing.Drawing2D.MatrixOrder.Append)
            gp.Transform(myMatrix)
            Return gp.IsOutlineVisible(pt, myPen)
        End Function

        Public Function GetStartPosition() As Point
            Return Me.GetPosition()
        End Function

        Public Sub SetStartPosition(ByVal Value As Point)
            Me.SetPosition(Value)
        End Sub

        Public Function GetEndPosition() As Point
            Dim endPosition As New Point(Me.m_Position.X, Me.m_Position.Y)
            endPosition.X += Me.m_Size.Width
            endPosition.Y += Me.m_Size.Height
            Return endPosition
        End Function

        Public Sub SetEndPosition(ByVal Value As Point)
            m_Size.Width = Value.X - Me.m_Position.X
            m_Size.Height = Value.Y - Me.m_Position.Y
        End Sub

        Public Overrides Sub Draw(ByVal g As System.Drawing.Graphics)

            Dim gContainer As System.Drawing.Drawing2D.GraphicsContainer

            gContainer = g.BeginContainer()

            Dim myPen As New Pen(Color.Tomato, 4)
            With myPen
                .DashStyle = DashStyle.Dot
                .DashCap = DashCap.Round
                .EndCap = Drawing2D.LineCap.ArrowAnchor
                .StartCap = Drawing2D.LineCap.DiamondAnchor
            End With

            'posicionar pontos nos primeiros slots livres
            g.SmoothingMode = SmoothingMode.AntiAlias
            g.DrawLines(myPen, New Point() {New Point(Me.X, Me.Y), New Point(Me.Width, Me.Height)})

            g.EndContainer(gContainer)

        End Sub

    End Class

    <Serializable()> Public Class AdjustGraphic
        Inherits ShapeGraphic

        Protected m_mvPT, m_cvPT, m_rvPT As GraphicObject

        Public Property ConnectedToMv() As GraphicObject
            Get
                Return m_mvPT
            End Get
            Set(ByVal value As GraphicObject)
                m_mvPT = value
            End Set
        End Property

        Public Property ConnectedToRv() As GraphicObject
            Get
                Return m_rvPT
            End Get
            Set(ByVal value As GraphicObject)
                m_rvPT = value
            End Set
        End Property

        Public Property ConnectedToCv() As GraphicObject
            Get
                Return m_cvPT
            End Get
            Set(ByVal value As GraphicObject)
                m_cvPT = value
            End Set
        End Property

#Region "Constructors"
        Public Sub New()
            Me.TipoObjeto = GraphicObjects.TipoObjeto.OT_Ajuste
            Me.Description = "Ajuste"
        End Sub

        Public Sub New(ByVal graphicPosition As Point)
            Me.New()
            Me.SetPosition(graphicPosition)
        End Sub

        Public Sub New(ByVal posX As Integer, ByVal posY As Integer)
            Me.New(New Point(posX, posY))
        End Sub

        Public Sub New(ByVal graphicPosition As Point, ByVal graphicSize As Size)
            Me.New(graphicPosition)
            Me.SetSize(graphicSize)
            Me.AutoSize = False
        End Sub

        Public Sub New(ByVal posX As Integer, ByVal posY As Integer, ByVal graphicSize As Size)
            Me.New(New Point(posX, posY), graphicSize)
        End Sub

        Public Sub New(ByVal posX As Integer, ByVal posY As Integer, ByVal width As Integer, ByVal height As Integer)
            Me.New(New Point(posX, posY), New Size(width, height))
        End Sub

        Public Sub New(ByVal graphicPosition As Point, ByVal Rotation As Single)
            Me.New()
            Me.SetPosition(graphicPosition)
            Me.Rotation = Rotation
        End Sub

        Public Sub New(ByVal posX As Integer, ByVal posY As Integer, ByVal Rotation As Single)
            Me.New(New Point(posX, posY), Rotation)
        End Sub

        Public Sub New(ByVal graphicPosition As Point, ByVal graphicSize As Size, ByVal Rotation As Single)
            Me.New(graphicPosition, Rotation)
            Me.SetSize(graphicSize)
            Me.AutoSize = False
        End Sub

        Public Sub New(ByVal posX As Integer, ByVal posY As Integer, ByVal graphicSize As Size, ByVal Rotation As Single)
            Me.New(New Point(posX, posY), graphicSize, Rotation)
        End Sub

        Public Sub New(ByVal posX As Integer, ByVal posY As Integer, ByVal width As Integer, _
                               ByVal height As Integer, ByVal Rotation As Single)
            Me.New(New Point(posX, posY), New Size(width, height), Rotation)
        End Sub

#End Region

        Public Overrides Sub CreateConnectors(InCount As Integer, OutCount As Integer)

            Dim mySC1 As New ConnectionPoint
            mySC1.Position = New Point(X, Y + 0.5 * Height)
            mySC1.Type = ConType.ConEn

            Dim mySC2 As New ConnectionPoint
            mySC2.Position = New Point(X + 0.5 * Width, Y)
            mySC2.Type = ConType.ConEn

            Dim mySC3 As New ConnectionPoint
            mySC3.Position = New Point(X + 0.5 * Width, Y + Height)
            mySC3.Type = ConType.ConEn

            With SpecialConnectors

                If .Count <> 0 Then
                    .Item(0).Position = New Point(X, Y + 0.5 * Height)
                    .Item(1).Position = New Point(X + 0.5 * Width, Y)
                    .Item(2).Position = New Point(X + 0.5 * Width, Y + Height)
                Else
                    .Add(mySC1)
                    .Add(mySC2)
                    .Add(mySC3)
                End If

            End With

        End Sub

        Public Overrides Sub Draw(ByVal g As Graphics)

            CreateConnectors(0, 0)

            UpdateStatus(Me)


            Dim pt As Point
            Dim raio, angulo As Double
            Dim con As ConnectionPoint
            For Each con In Me.InputConnectors
                pt = con.Position
                raio = ((pt.X - Me.X) ^ 2 + (pt.Y - Me.Y) ^ 2) ^ 0.5
                angulo = Math.Atan2(pt.Y - Me.Y, pt.X - Me.X)
                pt.X = Me.X + raio * Math.Cos(angulo + Me.Rotation / 360 * 2 * Math.PI)
                pt.Y = Me.Y + raio * Math.Sin(angulo + Me.Rotation / 360 * 2 * Math.PI)
                con.Position = pt
            Next
            For Each con In Me.OutputConnectors
                pt = con.Position
                raio = ((pt.X - Me.X) ^ 2 + (pt.Y - Me.Y) ^ 2) ^ 0.5
                angulo = Math.Atan2(pt.Y - Me.Y, pt.X - Me.X)
                pt.X = Me.X + raio * Math.Cos(angulo + Me.Rotation / 360 * 2 * Math.PI)
                pt.Y = Me.Y + raio * Math.Sin(angulo + Me.Rotation / 360 * 2 * Math.PI)
                con.Position = pt
            Next

            Dim gContainer As Drawing2D.GraphicsContainer
            Dim myMatrix As Drawing2D.Matrix
            gContainer = g.BeginContainer()
            myMatrix = g.Transform()
            If m_Rotation <> 0 Then
                myMatrix.RotateAt(m_Rotation, New PointF(X, Y), _
                    Drawing2D.MatrixOrder.Append)
                g.Transform = myMatrix
            End If

            If Not Me.ConnectedToMv Is Nothing Then
                Dim cpen As New Pen(Color.Red, 2)
                With cpen
                    .DashStyle = DashStyle.Dot
                    .DashCap = DashCap.Round
                    .EndCap = Drawing2D.LineCap.ArrowAnchor
                    .StartCap = Drawing2D.LineCap.Flat
                End With
                g.DrawLines(cpen, New Point() {New Point(Me.X + Me.Width / 2, Me.Y + Me.Height / 2), New Point(Me.m_mvPT.X, Me.Y + Me.Height / 2), Me.m_mvPT.GetPosition})
            End If
            If Not Me.ConnectedToCv Is Nothing Then
                Dim cpen As New Pen(Color.Red, 2)
                With cpen
                    .DashStyle = DashStyle.Dot
                    .DashCap = DashCap.Round
                    .EndCap = Drawing2D.LineCap.ArrowAnchor
                    .StartCap = Drawing2D.LineCap.DiamondAnchor
                End With
                g.DrawLines(cpen, New Point() {New Point(Me.X + Me.Width / 2, Me.Y + Me.Height / 2), New Point(Me.m_cvPT.X, Me.Y + Me.Height / 2), Me.m_cvPT.GetPosition})
            End If
            If Not Me.ConnectedToRv Is Nothing Then
                Dim cpen As New Pen(Color.Red, 2)
                With cpen
                    .DashStyle = DashStyle.Dot
                    .DashCap = DashCap.Round
                    .EndCap = Drawing2D.LineCap.ArrowAnchor
                    .StartCap = Drawing2D.LineCap.DiamondAnchor
                End With
                g.DrawLines(cpen, New Point() {New Point(Me.X + Me.Width / 2, Me.Y + Me.Height / 2), New Point(Me.m_rvPT.X, Me.Y + Me.Height / 2), Me.m_rvPT.GetPosition})
            End If

            Dim rect2 As New Rectangle(X - Width * 1 / 2, Y - Height * 1 / 2, Width * 2, Height * 2)
            ' Create a path that consists of a single ellipse.
            Dim path2 As New GraphicsPath()
            path2.AddEllipse(rect2)
            ' Use the path to construct a brush.
            Dim pthGrBrush2 As New PathGradientBrush(path2)
            ' Set the color at the center of the path to blue.
            If Me.Calculated Then
                pthGrBrush2.CenterColor = Color.SteelBlue
            Else
                pthGrBrush2.CenterColor = Color.Red
            End If
            ' Set the color along the entire boundary 
            ' of the path to aqua.
            Dim colors2 As Color() = {Color.Transparent}
            pthGrBrush2.SurroundColors = colors2
            pthGrBrush2.SetSigmaBellShape(1)
            g.FillEllipse(pthGrBrush2, rect2)

            Dim rect As New Rectangle(X, Y, Width, Height)

            ' Create a path that consists of a single ellipse.
            Dim path As New GraphicsPath()
            path.AddEllipse(rect)
            ' Use the path to construct a brush.
            Dim pthGrBrush As New PathGradientBrush(path)
            ' Set the color at the center of the path to blue.
            pthGrBrush.CenterColor = Color.White

            ' Set the color along the entire boundary 
            ' of the path to aqua.
            Dim colors As Color() = {Color.LightSalmon}
            pthGrBrush.SurroundColors = colors

            pthGrBrush.SetSigmaBellShape(1)

            If Me.Fill Then
                If Me.GradientMode = False Then
                    g.FillEllipse(New SolidBrush(Me.FillColor), rect)
                Else
                    g.FillEllipse(pthGrBrush, rect)
                End If
            End If

            Dim size As SizeF
            Dim fontA As New Font("Arial", 10, FontStyle.Bold, GraphicsUnit.Pixel, 0, False)
            size = g.MeasureString("A", fontA)

            Dim ax, ay As Integer
            ax = Me.X + (Me.Width - size.Width) / 2
            ay = Me.Y + (Me.Height - size.Height) / 2

            g.SmoothingMode = SmoothingMode.AntiAlias
            g.TextRenderingHint = Text.TextRenderingHint.AntiAlias
            g.DrawString("A", fontA, Brushes.Red, ax, ay)

            Dim myPen As New Pen(Color.Red, 2)
            g.DrawEllipse(myPen, rect)
            g.TextRenderingHint = Text.TextRenderingHint.SystemDefault

            Dim strdist As SizeF = g.MeasureString(Me.Tag, New Font("Arial", 10, FontStyle.Bold, GraphicsUnit.Pixel, 0, False), New PointF(0, 0), New StringFormat(StringFormatFlags.NoClip, 0))
            Dim strx As Single = (Me.Width - strdist.Width) / 2
            'g.FillRectangle(Brushes.White, X + strx, Y + CSng(Height + 5), strdist.Width, strdist.Height)
            g.DrawString(Me.Tag, New Font("Arial", 10, FontStyle.Bold, GraphicsUnit.Pixel, 0, False), New SolidBrush(Me.LineColor), X + strx, Y + Height + 5)

            g.EndContainer(gContainer)

        End Sub

        Public Overrides Function SaveData() As System.Collections.Generic.List(Of System.Xml.Linq.XElement)

            Dim elements As System.Collections.Generic.List(Of System.Xml.Linq.XElement) = MyBase.SaveData()

            With elements
                If Not ConnectedToCv Is Nothing Then .Add(New XElement("ConnectedToCvID", ConnectedToCv.Name))
                If Not ConnectedToMv Is Nothing Then .Add(New XElement("ConnectedToMvID", ConnectedToMv.Name))
                If Not ConnectedToRv Is Nothing Then .Add(New XElement("ConnectedToRvID", ConnectedToRv.Name))
            End With

            Return elements

        End Function

        Public Overrides Function LoadData(data As System.Collections.Generic.List(Of System.Xml.Linq.XElement)) As Boolean

            MyBase.LoadData(data)

        End Function

    End Class

    <Serializable()> Public Class SpecGraphic
        Inherits ShapeGraphic

        Protected m_svPT, m_tvPT, m_rvPT As GraphicObject

        Public Property ConnectedToSv() As GraphicObject
            Get
                Return m_svPT
            End Get
            Set(ByVal value As GraphicObject)
                m_svPT = value
            End Set
        End Property

        Public Property ConnectedToTv() As GraphicObject
            Get
                Return m_tvPT
            End Get
            Set(ByVal value As GraphicObject)
                m_tvPT = value
            End Set
        End Property

#Region "Constructors"
        Public Sub New()
            Me.TipoObjeto = GraphicObjects.TipoObjeto.OT_Especificacao
            Me.Description = "Especificao"
        End Sub

        Public Sub New(ByVal graphicPosition As Point)
            Me.New()
            Me.SetPosition(graphicPosition)
        End Sub

        Public Sub New(ByVal posX As Integer, ByVal posY As Integer)
            Me.New(New Point(posX, posY))
        End Sub

        Public Sub New(ByVal graphicPosition As Point, ByVal graphicSize As Size)
            Me.New(graphicPosition)
            Me.SetSize(graphicSize)
            Me.AutoSize = False
        End Sub

        Public Sub New(ByVal posX As Integer, ByVal posY As Integer, ByVal graphicSize As Size)
            Me.New(New Point(posX, posY), graphicSize)
        End Sub

        Public Sub New(ByVal posX As Integer, ByVal posY As Integer, ByVal width As Integer, ByVal height As Integer)
            Me.New(New Point(posX, posY), New Size(width, height))
        End Sub

        Public Sub New(ByVal graphicPosition As Point, ByVal Rotation As Single)
            Me.New()
            Me.SetPosition(graphicPosition)
            Me.Rotation = Rotation
        End Sub

        Public Sub New(ByVal posX As Integer, ByVal posY As Integer, ByVal Rotation As Single)
            Me.New(New Point(posX, posY), Rotation)
        End Sub

        Public Sub New(ByVal graphicPosition As Point, ByVal graphicSize As Size, ByVal Rotation As Single)
            Me.New(graphicPosition, Rotation)
            Me.SetSize(graphicSize)
            Me.AutoSize = False
        End Sub

        Public Sub New(ByVal posX As Integer, ByVal posY As Integer, ByVal graphicSize As Size, ByVal Rotation As Single)
            Me.New(New Point(posX, posY), graphicSize, Rotation)
        End Sub

        Public Sub New(ByVal posX As Integer, ByVal posY As Integer, ByVal width As Integer, _
                               ByVal height As Integer, ByVal Rotation As Single)
            Me.New(New Point(posX, posY), New Size(width, height), Rotation)
        End Sub

#End Region


        Public Overrides Sub CreateConnectors(InCount As Integer, OutCount As Integer)

            Dim mySC1 As New ConnectionPoint
            mySC1.Position = New Point(X, Y + 0.5 * Height)
            mySC1.Type = ConType.ConEn

            Dim mySC2 As New ConnectionPoint
            mySC2.Position = New Point(X + 0.5 * Width, Y)
            mySC2.Type = ConType.ConEn

            Dim mySC3 As New ConnectionPoint
            mySC3.Position = New Point(X + 0.5 * Width, Y + Height)
            mySC3.Type = ConType.ConEn

            With SpecialConnectors

                If .Count <> 0 Then
                    .Item(0).Position = New Point(X, Y + 0.5 * Height)
                    .Item(1).Position = New Point(X + 0.5 * Width, Y)
                    .Item(2).Position = New Point(X + 0.5 * Width, Y + Height)
                Else
                    .Add(mySC1)
                    .Add(mySC2)
                    .Add(mySC3)
                End If

            End With

        End Sub
        Public Overrides Sub Draw(ByVal g As Graphics)

            CreateConnectors(0, 0)

            UpdateStatus(Me)

            If Not Me.Active Then Me.LineColor = Color.Yellow

            Dim pt As Point
            Dim raio, angulo As Double
            Dim con As ConnectionPoint
            For Each con In Me.InputConnectors
                pt = con.Position
                raio = ((pt.X - Me.X) ^ 2 + (pt.Y - Me.Y) ^ 2) ^ 0.5
                angulo = Math.Atan2(pt.Y - Me.Y, pt.X - Me.X)
                pt.X = Me.X + raio * Math.Cos(angulo + Me.Rotation / 360 * 2 * Math.PI)
                pt.Y = Me.Y + raio * Math.Sin(angulo + Me.Rotation / 360 * 2 * Math.PI)
                con.Position = pt
            Next
            For Each con In Me.OutputConnectors
                pt = con.Position
                raio = ((pt.X - Me.X) ^ 2 + (pt.Y - Me.Y) ^ 2) ^ 0.5
                angulo = Math.Atan2(pt.Y - Me.Y, pt.X - Me.X)
                pt.X = Me.X + raio * Math.Cos(angulo + Me.Rotation / 360 * 2 * Math.PI)
                pt.Y = Me.Y + raio * Math.Sin(angulo + Me.Rotation / 360 * 2 * Math.PI)
                con.Position = pt
            Next

            Dim gContainer As Drawing2D.GraphicsContainer
            Dim myMatrix As Drawing2D.Matrix
            gContainer = g.BeginContainer()
            myMatrix = g.Transform()
            If m_Rotation <> 0 Then
                myMatrix.RotateAt(m_Rotation, New PointF(X, Y), _
                    Drawing2D.MatrixOrder.Append)
                g.Transform = myMatrix
            End If

            If Not Me.ConnectedToSv Is Nothing Then
                Dim cpen As New Pen(Color.Red, 2)
                With cpen
                    .DashStyle = DashStyle.Dot
                    .DashCap = DashCap.Round
                    .EndCap = Drawing2D.LineCap.ArrowAnchor
                    .StartCap = Drawing2D.LineCap.Flat
                End With
                g.DrawLines(cpen, New Point() {New Point(Me.X + Me.Width, Me.Y + Me.Height / 2), New Point(Me.m_svPT.X + Me.m_svPT.Width / 2, Me.Y + Me.Height / 2), New Point(Me.m_svPT.X + Me.m_svPT.Width / 2, Me.m_svPT.Y + Me.m_svPT.Height / 2)})
            End If

            If Not Me.ConnectedToTv Is Nothing Then
                Dim cpen As New Pen(Color.Red, 2)
                With cpen
                    .DashStyle = DashStyle.Dot
                    .DashCap = DashCap.Round
                    .EndCap = Drawing2D.LineCap.ArrowAnchor
                    .StartCap = Drawing2D.LineCap.DiamondAnchor
                End With
                g.DrawLines(cpen, New Point() {New Point(Me.X + Me.Width, Me.Y + Me.Height / 2), New Point(Me.m_tvPT.X + Me.m_tvPT.Width / 2, Me.Y + Me.Height / 2), New Point(Me.m_tvPT.X + Me.m_tvPT.Width / 2, Me.m_tvPT.Y + Me.m_tvPT.Height / 2)})
            End If

            Dim rect2 As New Rectangle(X - Width * 1 / 2, Y - Height * 1 / 2, Width * 2, Height * 2)
            ' Create a path that consists of a single ellipse.
            Dim path2 As New GraphicsPath()
            path2.AddEllipse(rect2)
            ' Use the path to construct a brush.
            Dim pthGrBrush2 As New PathGradientBrush(path2)
            ' Set the color at the center of the path to blue.
            If Me.Calculated Then
                pthGrBrush2.CenterColor = Color.SteelBlue
            Else
                pthGrBrush2.CenterColor = Color.Red
            End If
            If Not Me.Active Then pthGrBrush2.CenterColor = Color.DimGray
            ' Set the color along the entire boundary 
            ' of the path to aqua.
            Dim colors2 As Color() = {Color.Transparent}
            pthGrBrush2.SurroundColors = colors2
            pthGrBrush2.SetSigmaBellShape(1)
            g.FillEllipse(pthGrBrush2, rect2)

            Dim rect As New Rectangle(X, Y, Width, Height)

            ' Create a path that consists of a single ellipse.
            Dim path As New GraphicsPath()
            path.AddEllipse(rect)
            ' Use the path to construct a brush.
            Dim pthGrBrush As New PathGradientBrush(path)
            ' Set the color at the center of the path to blue.
            pthGrBrush.CenterColor = Color.White

            ' Set the color along the entire boundary 
            ' of the path to aqua.
            Dim colors As Color() = {Color.LightSteelBlue}
            pthGrBrush.SurroundColors = colors

            pthGrBrush.SetSigmaBellShape(1)

            If Me.Fill Then
                If Me.GradientMode = False Then
                    g.FillEllipse(New SolidBrush(Me.FillColor), rect)
                Else
                    g.FillEllipse(pthGrBrush, rect)
                End If
            End If

            Dim size As SizeF
            Dim fontA As New Font("Arial", 10, FontStyle.Bold, GraphicsUnit.Pixel, 0, False)
            size = g.MeasureString("E", fontA)

            Dim ax, ay As Integer
            ax = Me.X + (Me.Width - size.Width) / 2
            ay = Me.Y + (Me.Height - size.Height) / 2

            g.SmoothingMode = SmoothingMode.AntiAlias
            g.TextRenderingHint = Text.TextRenderingHint.AntiAlias

            g.DrawString("E", fontA, Brushes.Blue, ax, ay)

            Dim myPen As New Pen(Color.SteelBlue, 2)
            g.DrawEllipse(myPen, rect)
            g.TextRenderingHint = Text.TextRenderingHint.SystemDefault

            Dim strdist As SizeF = g.MeasureString(Me.Tag, New Font("Arial", 10, FontStyle.Bold, GraphicsUnit.Pixel, 0, False), New PointF(0, 0), New StringFormat(StringFormatFlags.NoClip, 0))
            Dim strx As Single = (Me.Width - strdist.Width) / 2
            'g.FillRectangle(Brushes.White, X + strx, Y + CSng(Height + 5), strdist.Width, strdist.Height)
            g.DrawString(Me.Tag, New Font("Arial", 10, FontStyle.Bold, GraphicsUnit.Pixel, 0, False), New SolidBrush(Me.LineColor), X + strx, Y + Height + 5)

            g.EndContainer(gContainer)
        End Sub

        Public Overrides Function SaveData() As System.Collections.Generic.List(Of System.Xml.Linq.XElement)

            Dim elements As System.Collections.Generic.List(Of System.Xml.Linq.XElement) = MyBase.SaveData()

            With elements
                If Not ConnectedToSv Is Nothing Then .Add(New XElement("ConnectedToCvID", ConnectedToSv.Name))
                If Not ConnectedToTv Is Nothing Then .Add(New XElement("ConnectedToMvID", ConnectedToTv.Name))
            End With

            Return elements

        End Function

        Public Overrides Function LoadData(data As System.Collections.Generic.List(Of System.Xml.Linq.XElement)) As Boolean

            MyBase.LoadData(data)



        End Function

    End Class

    <Serializable()> Public Class RecycleGraphic
        Inherits ShapeGraphic

#Region "Constructors"
        Public Sub New()
            Me.TipoObjeto = GraphicObjects.TipoObjeto.OT_Reciclo
            Me.Description = "Reciclo"
        End Sub

        Public Sub New(ByVal graphicPosition As Point)
            Me.New()
            Me.SetPosition(graphicPosition)
        End Sub

        Public Sub New(ByVal posX As Integer, ByVal posY As Integer)
            Me.New(New Point(posX, posY))
        End Sub

        Public Sub New(ByVal graphicPosition As Point, ByVal graphicSize As Size)
            Me.New(graphicPosition)
            Me.SetSize(graphicSize)
            Me.AutoSize = False
        End Sub

        Public Sub New(ByVal posX As Integer, ByVal posY As Integer, ByVal graphicSize As Size)
            Me.New(New Point(posX, posY), graphicSize)
        End Sub

        Public Sub New(ByVal posX As Integer, ByVal posY As Integer, ByVal width As Integer, ByVal height As Integer)
            Me.New(New Point(posX, posY), New Size(width, height))
        End Sub

        Public Sub New(ByVal graphicPosition As Point, ByVal Rotation As Single)
            Me.New()
            Me.SetPosition(graphicPosition)
            Me.Rotation = Rotation
        End Sub

        Public Sub New(ByVal posX As Integer, ByVal posY As Integer, ByVal Rotation As Single)
            Me.New(New Point(posX, posY), Rotation)
        End Sub

        Public Sub New(ByVal graphicPosition As Point, ByVal graphicSize As Size, ByVal Rotation As Single)
            Me.New(graphicPosition, Rotation)
            Me.SetSize(graphicSize)
            Me.AutoSize = False
        End Sub

        Public Sub New(ByVal posX As Integer, ByVal posY As Integer, ByVal graphicSize As Size, ByVal Rotation As Single)
            Me.New(New Point(posX, posY), graphicSize, Rotation)
        End Sub

        Public Sub New(ByVal posX As Integer, ByVal posY As Integer, ByVal width As Integer, _
                               ByVal height As Integer, ByVal Rotation As Single)
            Me.New(New Point(posX, posY), New Size(width, height), Rotation)
        End Sub

#End Region

        Public Overrides Sub CreateConnectors(InCount As Integer, OutCount As Integer)
            Dim myIC1 As New ConnectionPoint
            myIC1.Position = New Point(X, Y + 0.5 * Height)
            myIC1.Type = ConType.ConIn

            Dim myOC1 As New ConnectionPoint
            myOC1.Position = New Point(X + Width, Y + 0.5 * Height)
            myOC1.Type = ConType.ConOut

            Me.EnergyConnector.Position = New Point(X + 0.5 * Width, Y + Height)
            Me.EnergyConnector.Type = ConType.ConEn

            With InputConnectors

                If .Count <> 0 Then
                    If Me.FlippedH Then
                        .Item(0).Position = New Point(X + Width, Y + 0.5 * Height)
                    Else
                        .Item(0).Position = New Point(X, Y + 0.5 * Height)
                    End If
                Else
                    .Add(myIC1)
                End If

            End With

            With OutputConnectors

                If .Count <> 0 Then
                    If Me.FlippedH Then
                        .Item(0).Position = New Point(X, Y + 0.5 * Height)
                    Else
                        .Item(0).Position = New Point(X + Width, Y + 0.5 * Height)
                    End If
                Else
                    .Add(myOC1)
                End If

            End With
        End Sub


        Public Overrides Sub Draw(ByVal g As Graphics)

            CreateConnectors(0, 0)

            UpdateStatus(Me)

            Dim pt As Point
            Dim raio, angulo As Double
            Dim con As ConnectionPoint
            For Each con In Me.InputConnectors
                pt = con.Position
                raio = ((pt.X - Me.X) ^ 2 + (pt.Y - Me.Y) ^ 2) ^ 0.5
                angulo = Math.Atan2(pt.Y - Me.Y, pt.X - Me.X)
                pt.X = Me.X + raio * Math.Cos(angulo + Me.Rotation / 360 * 2 * Math.PI)
                pt.Y = Me.Y + raio * Math.Sin(angulo + Me.Rotation / 360 * 2 * Math.PI)
                con.Position = pt
            Next
            For Each con In Me.OutputConnectors
                pt = con.Position
                raio = ((pt.X - Me.X) ^ 2 + (pt.Y - Me.Y) ^ 2) ^ 0.5
                angulo = Math.Atan2(pt.Y - Me.Y, pt.X - Me.X)
                pt.X = Me.X + raio * Math.Cos(angulo + Me.Rotation / 360 * 2 * Math.PI)
                pt.Y = Me.Y + raio * Math.Sin(angulo + Me.Rotation / 360 * 2 * Math.PI)
                con.Position = pt
            Next

            Dim gContainer As Drawing2D.GraphicsContainer
            Dim myMatrix As Drawing2D.Matrix
            gContainer = g.BeginContainer()
            myMatrix = g.Transform()
            If m_Rotation <> 0 Then
                myMatrix.RotateAt(m_Rotation, New PointF(X, Y), _
                    Drawing2D.MatrixOrder.Prepend)
                g.Transform = myMatrix
            End If

            Dim rect2 As New Rectangle(X - Width * 1 / 2, Y - Height * 1 / 2, Width * 2, Height * 2)
            ' Create a path that consists of a single ellipse.
            Dim path2 As New GraphicsPath()
            path2.AddEllipse(rect2)
            ' Use the path to construct a brush.
            Dim pthGrBrush2 As New PathGradientBrush(path2)
            ' Set the color at the center of the path to blue.
            If Me.Calculated Then
                pthGrBrush2.CenterColor = Color.SteelBlue
            Else
                pthGrBrush2.CenterColor = Color.Red
            End If
            ' Set the color along the entire boundary 
            ' of the path to aqua.
            Dim colors2 As Color() = {Color.Transparent}
            pthGrBrush2.SurroundColors = colors2
            pthGrBrush2.SetSigmaBellShape(1)
            g.FillEllipse(pthGrBrush2, rect2)

            Dim rect As New Rectangle(X, Y, Width, Height)

            ' Create a path that consists of a single ellipse.
            Dim path As New GraphicsPath()
            path.AddEllipse(rect)
            ' Use the path to construct a brush.
            Dim pthGrBrush As New PathGradientBrush(path)
            ' Set the color at the center of the path to blue.
            pthGrBrush.CenterColor = Color.White

            ' Set the color along the entire boundary 
            ' of the path to aqua.
            Dim colors As Color() = {Color.LightGreen}
            pthGrBrush.SurroundColors = colors

            pthGrBrush.SetSigmaBellShape(1)

            If Me.Fill Then
                If Me.GradientMode = False Then
                    g.FillEllipse(New SolidBrush(Me.FillColor), rect)
                Else
                    g.FillEllipse(pthGrBrush, rect)
                End If
            End If

            Dim size As SizeF
            Dim fontA As New Font("Arial", 10, FontStyle.Bold, GraphicsUnit.Pixel, 0, False)
            size = g.MeasureString("R", fontA)

            Dim ax, ay As Integer
            ax = Me.X + (Me.Width - size.Width) / 2
            ay = Me.Y + (Me.Height - size.Height) / 2

            g.SmoothingMode = SmoothingMode.AntiAlias
            g.TextRenderingHint = Text.TextRenderingHint.AntiAlias
            g.DrawString("R", fontA, Brushes.Green, ax, ay)

            Dim myPen As New Pen(Color.SteelBlue, 2)
            g.DrawEllipse(myPen, rect)
            g.TextRenderingHint = Text.TextRenderingHint.SystemDefault

            Dim strdist As SizeF = g.MeasureString(Me.Tag, New Font("Arial", 10, FontStyle.Bold, GraphicsUnit.Pixel, 0, False), New PointF(0, 0), New StringFormat(StringFormatFlags.NoClip, 0))
            Dim strx As Single = (Me.Width - strdist.Width) / 2
            'g.FillRectangle(Brushes.White, X + strx, Y + CSng(Height + 5), strdist.Width, strdist.Height)
            g.DrawString(Me.Tag, New Font("Arial", 10, FontStyle.Bold, GraphicsUnit.Pixel, 0, False), New SolidBrush(Me.LineColor), X + strx, Y + Height + 5)

            g.EndContainer(gContainer)

        End Sub

    End Class

    <Serializable()> Public Class EnergyRecycleGraphic
        Inherits ShapeGraphic

#Region "Constructors"
        Public Sub New()
            Me.TipoObjeto = GraphicObjects.TipoObjeto.OT_EnergyRecycle
            Me.Description = "EnergyRecycle"
        End Sub

        Public Sub New(ByVal graphicPosition As Point)
            Me.New()
            Me.SetPosition(graphicPosition)
        End Sub

        Public Sub New(ByVal posX As Integer, ByVal posY As Integer)
            Me.New(New Point(posX, posY))
        End Sub

        Public Sub New(ByVal graphicPosition As Point, ByVal graphicSize As Size)
            Me.New(graphicPosition)
            Me.SetSize(graphicSize)
            Me.AutoSize = False
        End Sub

        Public Sub New(ByVal posX As Integer, ByVal posY As Integer, ByVal graphicSize As Size)
            Me.New(New Point(posX, posY), graphicSize)
        End Sub

        Public Sub New(ByVal posX As Integer, ByVal posY As Integer, ByVal width As Integer, ByVal height As Integer)
            Me.New(New Point(posX, posY), New Size(width, height))
        End Sub

        Public Sub New(ByVal graphicPosition As Point, ByVal Rotation As Single)
            Me.New()
            Me.SetPosition(graphicPosition)
            Me.Rotation = Rotation
        End Sub

        Public Sub New(ByVal posX As Integer, ByVal posY As Integer, ByVal Rotation As Single)
            Me.New(New Point(posX, posY), Rotation)
        End Sub

        Public Sub New(ByVal graphicPosition As Point, ByVal graphicSize As Size, ByVal Rotation As Single)
            Me.New(graphicPosition, Rotation)
            Me.SetSize(graphicSize)
            Me.AutoSize = False
        End Sub

        Public Sub New(ByVal posX As Integer, ByVal posY As Integer, ByVal graphicSize As Size, ByVal Rotation As Single)
            Me.New(New Point(posX, posY), graphicSize, Rotation)
        End Sub

        Public Sub New(ByVal posX As Integer, ByVal posY As Integer, ByVal width As Integer, _
                               ByVal height As Integer, ByVal Rotation As Single)
            Me.New(New Point(posX, posY), New Size(width, height), Rotation)
        End Sub

#End Region

        Public Overrides Sub CreateConnectors(InCount As Integer, OutCount As Integer)

            Dim myIC1 As New ConnectionPoint
            myIC1.Position = New Point(X, Y + 0.5 * Height)
            myIC1.Type = ConType.ConEn

            Dim myOC1 As New ConnectionPoint
            myOC1.Position = New Point(X + Width, Y + 0.5 * Height)
            myOC1.Type = ConType.ConEn

            Me.EnergyConnector.Position = New Point(X + 0.5 * Width, Y + Height)
            Me.EnergyConnector.Type = ConType.ConEn

            With InputConnectors

                If .Count <> 0 Then
                    If Me.FlippedH Then
                        .Item(0).Position = New Point(X + Width, Y + 0.5 * Height)
                    Else
                        .Item(0).Position = New Point(X, Y + 0.5 * Height)
                    End If
                Else
                    .Add(myIC1)
                End If

            End With

            With OutputConnectors

                If .Count <> 0 Then
                    If Me.FlippedH Then
                        .Item(0).Position = New Point(X, Y + 0.5 * Height)
                    Else
                        .Item(0).Position = New Point(X + Width, Y + 0.5 * Height)
                    End If
                Else
                    .Add(myOC1)
                End If

            End With

        End Sub

        Public Overrides Sub Draw(ByVal g As Graphics)

            CreateConnectors(0, 0)

            UpdateStatus(Me)

            Dim pt As Point
            Dim raio, angulo As Double
            Dim con As ConnectionPoint
            For Each con In Me.InputConnectors
                pt = con.Position
                raio = ((pt.X - Me.X) ^ 2 + (pt.Y - Me.Y) ^ 2) ^ 0.5
                angulo = Math.Atan2(pt.Y - Me.Y, pt.X - Me.X)
                pt.X = Me.X + raio * Math.Cos(angulo + Me.Rotation / 360 * 2 * Math.PI)
                pt.Y = Me.Y + raio * Math.Sin(angulo + Me.Rotation / 360 * 2 * Math.PI)
                con.Position = pt
            Next
            For Each con In Me.OutputConnectors
                pt = con.Position
                raio = ((pt.X - Me.X) ^ 2 + (pt.Y - Me.Y) ^ 2) ^ 0.5
                angulo = Math.Atan2(pt.Y - Me.Y, pt.X - Me.X)
                pt.X = Me.X + raio * Math.Cos(angulo + Me.Rotation / 360 * 2 * Math.PI)
                pt.Y = Me.Y + raio * Math.Sin(angulo + Me.Rotation / 360 * 2 * Math.PI)
                con.Position = pt
            Next

            Dim gContainer As Drawing2D.GraphicsContainer
            Dim myMatrix As Drawing2D.Matrix
            gContainer = g.BeginContainer()
            myMatrix = g.Transform()
            If m_Rotation <> 0 Then
                myMatrix.RotateAt(m_Rotation, New PointF(X, Y), _
                    Drawing2D.MatrixOrder.Prepend)
                g.Transform = myMatrix
            End If

            Dim rect2 As New Rectangle(X - Width * 1 / 2, Y - Height * 1 / 2, Width * 2, Height * 2)
            ' Create a path that consists of a single ellipse.
            Dim path2 As New GraphicsPath()
            path2.AddEllipse(rect2)
            ' Use the path to construct a brush.
            Dim pthGrBrush2 As New PathGradientBrush(path2)
            ' Set the color at the center of the path to blue.
            If Me.Calculated Then
                pthGrBrush2.CenterColor = Color.YellowGreen
            Else
                pthGrBrush2.CenterColor = Color.Red
            End If
            ' Set the color along the entire boundary 
            ' of the path to aqua.
            Dim colors2 As Color() = {Color.Transparent}
            pthGrBrush2.SurroundColors = colors2
            pthGrBrush2.SetSigmaBellShape(1)
            g.FillEllipse(pthGrBrush2, rect2)

            Dim rect As New Rectangle(X, Y, Width, Height)

            ' Create a path that consists of a single ellipse.
            Dim path As New GraphicsPath()
            path.AddEllipse(rect)
            ' Use the path to construct a brush.
            Dim pthGrBrush As New PathGradientBrush(path)
            ' Set the color at the center of the path to blue.
            pthGrBrush.CenterColor = Color.White

            ' Set the color along the entire boundary 
            ' of the path to aqua.
            Dim colors As Color() = {Color.LightYellow}
            pthGrBrush.SurroundColors = colors

            pthGrBrush.SetSigmaBellShape(1)

            If Me.Fill Then
                If Me.GradientMode = False Then
                    g.FillEllipse(New SolidBrush(Me.FillColor), rect)
                Else
                    g.FillEllipse(pthGrBrush, rect)
                End If
            End If

            Dim size As SizeF
            Dim fontA As New Font("Arial", 10, FontStyle.Bold, GraphicsUnit.Pixel, 0, False)
            size = g.MeasureString("R", fontA)

            Dim ax, ay As Integer
            ax = Me.X + (Me.Width - size.Width) / 2
            ay = Me.Y + (Me.Height - size.Height) / 2

            g.SmoothingMode = SmoothingMode.AntiAlias
            g.TextRenderingHint = Text.TextRenderingHint.AntiAlias
            g.DrawString("R", fontA, Brushes.YellowGreen, ax, ay)

            Dim myPen As New Pen(Color.YellowGreen, 2)
            g.DrawEllipse(myPen, rect)
            g.TextRenderingHint = Text.TextRenderingHint.SystemDefault

            Dim strdist As SizeF = g.MeasureString(Me.Tag, New Font("Arial", 10, FontStyle.Bold, GraphicsUnit.Pixel, 0, False), New PointF(0, 0), New StringFormat(StringFormatFlags.NoClip, 0))
            Dim strx As Single = (Me.Width - strdist.Width) / 2
            'g.FillRectangle(Brushes.White, X + strx, Y + CSng(Height + 5), strdist.Width, strdist.Height)
            g.DrawString(Me.Tag, New Font("Arial", 10, FontStyle.Bold, GraphicsUnit.Pixel, 0, False), New SolidBrush(Me.LineColor), X + strx, Y + Height + 5)

            g.EndContainer(gContainer)

        End Sub

    End Class

    <Serializable()> Public Class ReactorConversionGraphic

        Inherits ShapeGraphic

#Region "Constructors"

        Public Sub New()
            Me.TipoObjeto = GraphicObjects.TipoObjeto.RCT_Conversion
            Me.Description = "ReatorConversao"
            Me.m_splitpoint = True
        End Sub

        Public Sub New(ByVal graphicPosition As Point)
            Me.New()
            Me.SetPosition(graphicPosition)
        End Sub

        Public Sub New(ByVal posX As Integer, ByVal posY As Integer)
            Me.New(New Point(posX, posY))
        End Sub

        Public Sub New(ByVal graphicPosition As Point, ByVal graphicSize As Size)
            Me.New(graphicPosition)
            Me.SetSize(graphicSize)
            Me.AutoSize = False
        End Sub

        Public Sub New(ByVal posX As Integer, ByVal posY As Integer, ByVal graphicSize As Size)
            Me.New(New Point(posX, posY), graphicSize)
        End Sub

        Public Sub New(ByVal posX As Integer, ByVal posY As Integer, ByVal width As Integer, ByVal height As Integer)
            Me.New(New Point(posX, posY), New Size(width, height))
        End Sub

        Public Sub New(ByVal graphicPosition As Point, ByVal Rotation As Single)
            Me.New()
            Me.SetPosition(graphicPosition)
            Me.Rotation = Rotation
        End Sub

        Public Sub New(ByVal posX As Integer, ByVal posY As Integer, ByVal Rotation As Single)
            Me.New(New Point(posX, posY), Rotation)
        End Sub

        Public Sub New(ByVal graphicPosition As Point, ByVal graphicSize As Size, ByVal Rotation As Single)
            Me.New(graphicPosition, Rotation)
            Me.SetSize(graphicSize)
            Me.AutoSize = False
        End Sub

        Public Sub New(ByVal posX As Integer, ByVal posY As Integer, ByVal graphicSize As Size, ByVal Rotation As Single)
            Me.New(New Point(posX, posY), graphicSize, Rotation)
        End Sub

        Public Sub New(ByVal posX As Integer, ByVal posY As Integer, ByVal width As Integer, _
                               ByVal height As Integer, ByVal Rotation As Single)
            Me.New(New Point(posX, posY), New Size(width, height), Rotation)
        End Sub

#End Region


        Public Overrides Sub CreateConnectors(InCount As Integer, OutCount As Integer)

            Dim myIC1 As New ConnectionPoint
            myIC1.Position = New Point(X + 0.125 * Width, Y + 0.5 * Height)
            myIC1.Type = ConType.ConIn

            Dim myIC2 As New ConnectionPoint
            myIC2.Position = New Point(X + 0.125 * Width, Y + 0.7 * Height)
            myIC2.Type = ConType.ConEn

            Dim myOC1 As New ConnectionPoint
            myOC1.Position = New Point(X + 0.5 * Width, Y)
            myOC1.Type = ConType.ConOut

            Dim myOC2 As New ConnectionPoint
            myOC2.Position = New Point(X + 0.5 * Width, Y + Height)
            myOC2.Type = ConType.ConOut

            With InputConnectors

                If .Count <> 0 Then
                    If Me.FlippedH Then
                        .Item(0).Position = New Point(X + 0.875 * Width, Y + 0.5 * Height)
                        .Item(1).Position = New Point(X + 0.875 * Width, Y + 0.7 * Height)
                    Else
                        .Item(0).Position = New Point(X + 0.125 * Width, Y + 0.5 * Height)
                        .Item(1).Position = New Point(X + 0.125 * Width, Y + 0.7 * Height)
                    End If
                Else
                    .Add(myIC1)
                    .Add(myIC2)
                End If

            End With

            With OutputConnectors

                If .Count <> 0 Then
                    If Me.FlippedH Then
                        .Item(0).Position = New Point(X + 0.3 * Width, Y + (0.1 + 0.127 / 2) * Height)
                        .Item(1).Position = New Point(X + 0.3 * Width, Y + (0.773 + 0.127 / 2) * Height)
                    Else
                        .Item(0).Position = New Point(X + 0.7 * Width, Y + (0.1 + 0.127 / 2) * Height)
                        .Item(1).Position = New Point(X + 0.7 * Width, Y + (0.773 + 0.127 / 2) * Height)
                    End If
                Else
                    .Add(myOC1)
                    .Add(myOC2)
                End If

            End With

            With Me.EnergyConnector
                If Me.FlippedH Then
                    .Position = New Point(X + 0.875 * Width, Y + 0.7 * Height)
                Else
                    .Position = New Point(X + 0.125 * Width, Y + 0.7 * Height)
                End If
            End With

        End Sub

        Public Overrides Sub Draw(ByVal g As System.Drawing.Graphics)

            CreateConnectors(0, 0)

            UpdateStatus(Me)

            Dim pt As Point
            Dim raio, angulo As Double
            Dim con As ConnectionPoint
            For Each con In Me.InputConnectors
                pt = con.Position
                raio = ((pt.X - Me.X) ^ 2 + (pt.Y - Me.Y) ^ 2) ^ 0.5
                angulo = Math.Atan2(pt.Y - Me.Y, pt.X - Me.X)
                pt.X = Me.X + raio * Math.Cos(angulo + Me.Rotation / 360 * 2 * Math.PI)
                pt.Y = Me.Y + raio * Math.Sin(angulo + Me.Rotation / 360 * 2 * Math.PI)
                con.Position = pt
            Next
            For Each con In Me.OutputConnectors
                pt = con.Position
                raio = ((pt.X - Me.X) ^ 2 + (pt.Y - Me.Y) ^ 2) ^ 0.5
                angulo = Math.Atan2(pt.Y - Me.Y, pt.X - Me.X)
                pt.X = Me.X + raio * Math.Cos(angulo + Me.Rotation / 360 * 2 * Math.PI)
                pt.Y = Me.Y + raio * Math.Sin(angulo + Me.Rotation / 360 * 2 * Math.PI)
                con.Position = pt
            Next
            With Me.EnergyConnector
                pt = .Position
                raio = ((pt.X - Me.X) ^ 2 + (pt.Y - Me.Y) ^ 2) ^ 0.5
                angulo = Math.Atan2(pt.Y - Me.Y, pt.X - Me.X)
                pt.X = Me.X + raio * Math.Cos(angulo + Me.Rotation / 360 * 2 * Math.PI)
                pt.Y = Me.Y + raio * Math.Sin(angulo + Me.Rotation / 360 * 2 * Math.PI)
                .Position = pt
            End With

            Dim gContainer As System.Drawing.Drawing2D.GraphicsContainer
            Dim myMatrix As Drawing2D.Matrix
            gContainer = g.BeginContainer()
            myMatrix = g.Transform()
            If m_Rotation <> 0 Then
                myMatrix.RotateAt(m_Rotation, New PointF(X, Y), Drawing.Drawing2D.MatrixOrder.Append)
                g.Transform = myMatrix
            End If

            Dim rect2 As New Rectangle(X + 0.123 * Width, Y + 0.5 * Height, 0.127 * Width, 0.127 * Height)
            Dim rect3 As New Rectangle(X + 0.7 * Width, Y + 0.1 * Height, 0.127 * Width, 0.127 * Height)
            Dim rect4 As New Rectangle(X + 0.7 * Width, Y + 0.773 * Height, 0.127 * Width, 0.127 * Height)
            If Me.FlippedH = True Then
                rect2 = New Rectangle(X + (1 - 0.123) * Width, Y + 0.5 * Height, 0.127 * Width, 0.127 * Height)
                rect3 = New Rectangle(X + 0.3 * Width, Y + 0.1 * Height, 0.127 * Width, 0.127 * Height)
                rect4 = New Rectangle(X + 0.3 * Width, Y + 0.773 * Height, 0.127 * Width, 0.127 * Height)
            End If

            Dim myPen As New Pen(Me.LineColor, Me.LineWidth)

            Dim myPen2 As New Pen(Color.White, 0)

            Dim rect As New Rectangle(X, Y, Width, Height)

            g.SmoothingMode = SmoothingMode.AntiAlias
            'g.DrawRectangle(myPen2, rect)


            If Me.FlippedH = True Then
                Me.DrawRoundRect(g, myPen, X + 0.4 * Width, Y, 0.45 * Width, Height, 10, Brushes.Transparent)
            Else
                Me.DrawRoundRect(g, myPen, X + 0.25 * Width, Y, 0.45 * Width, Height, 10, Brushes.Transparent)
            End If
            g.DrawRectangle(myPen, rect2)
            g.DrawRectangle(myPen, rect3)
            g.DrawRectangle(myPen, rect4)

            Dim strdist As SizeF = g.MeasureString(Me.Tag, New Font("Arial", 10, FontStyle.Bold, GraphicsUnit.Pixel, 0, False), New PointF(0, 0), New StringFormat(StringFormatFlags.NoClip, 0))
            Dim strx As Single = (Me.Width - strdist.Width) / 2
            'g.FillRectangle(Brushes.White, X + strx, Y + CSng(Height + 5), strdist.Width, strdist.Height)
            g.DrawString(Me.Tag, New Font("Arial", 10, FontStyle.Bold, GraphicsUnit.Pixel, 0, False), New SolidBrush(Me.LineColor), X + strx, Y + Height + 5)

            Dim gp As Drawing2D.GraphicsPath = New Drawing2D.GraphicsPath
            Dim radius As Integer = 3
            gp.AddLine(X + radius, Y, X + Width - radius, Y)
            gp.AddArc(X + Width - radius, Y, radius, radius, 270, 90)
            gp.AddLine(X + Width, Y + radius, X + Width, Y + Height - radius)
            gp.AddArc(X + Width - radius, Y + Height - radius, radius, radius, 0, 90)
            gp.AddLine(X + Width - radius, Y + Height, X + radius, Y + Height)
            gp.AddArc(X, Y + Height - radius, radius, radius, 90, 90)
            gp.AddLine(X, Y + Height - radius, X, Y + radius)
            gp.AddArc(X, Y, radius, radius, 180, 90)
            Dim lgb1 As LinearGradientBrush
            lgb1 = New LinearGradientBrush(rect, Me.GradientColor1, Me.GradientColor2, LinearGradientMode.Horizontal)
            lgb1.SetBlendTriangularShape(0.5)
            'lgb1.CenterColor = Me.GradientColor1
            'lgb1.SetBlendTriangularShape(0.5)
            'lgb1.SurroundColors = New Color() {Me.GradientColor2}
            'lgb1.WrapMode = WrapMode.Tile
            If Me.Fill Then
                If Me.GradientMode = False Then
                    g.FillRectangle(New SolidBrush(Me.FillColor), rect3)
                    g.FillRectangle(New SolidBrush(Me.FillColor), rect4)
                    g.FillRectangle(New SolidBrush(Me.FillColor), rect2)
                    If Me.FlippedH = True Then
                        Me.DrawRoundRect(g, myPen, X + 0.4 * Width, Y, 0.45 * Width, Height, 6, New SolidBrush(Me.FillColor))
                    Else
                        Me.DrawRoundRect(g, myPen, X + 0.25 * Width, Y, 0.45 * Width, Height, 6, New SolidBrush(Me.FillColor))
                    End If
                Else
                    g.FillRectangle(New SolidBrush(Me.FillColor), rect3)
                    g.FillRectangle(New SolidBrush(Me.FillColor), rect4)
                    g.FillRectangle(New SolidBrush(Me.FillColor), rect2)
                    If Me.FlippedH = True Then
                        Me.DrawRoundRect(g, myPen, X + 0.4 * Width, Y, 0.45 * Width, Height, 6, lgb1)
                    Else
                        Me.DrawRoundRect(g, myPen, X + 0.25 * Width, Y, 0.45 * Width, Height, 6, lgb1)
                    End If
                End If
            End If

            If Me.FlippedH Then
                g.DrawLines(myPen, New PointF() {New PointF(X + 0.4 * Width, Y + 0.3 * Height), New PointF(X + 0.85 * Width, Me.Y + 0.3 * Height)})
                g.DrawLines(myPen, New PointF() {New PointF(X + 0.4 * Width, Y + 0.7 * Height), New PointF(X + 0.85 * Width, Me.Y + 0.7 * Height)})
                g.DrawLines(myPen, New PointF() {New PointF(X + 0.4 * Width, Y + 0.3 * Height), New PointF(X + 0.85 * Width, Me.Y + 0.7 * Height)})
                g.DrawLines(myPen, New PointF() {New PointF(X + 0.4 * Width, Y + 0.7 * Height), New PointF(X + 0.85 * Width, Me.Y + 0.3 * Height)})
            Else
                g.DrawLines(myPen, New PointF() {New PointF(X + 0.25 * Width, Y + 0.3 * Height), New PointF(X + 0.7 * Width, Me.Y + 0.3 * Height)})
                g.DrawLines(myPen, New PointF() {New PointF(X + 0.25 * Width, Y + 0.7 * Height), New PointF(X + 0.7 * Width, Me.Y + 0.7 * Height)})
                g.DrawLines(myPen, New PointF() {New PointF(X + 0.25 * Width, Y + 0.3 * Height), New PointF(X + 0.7 * Width, Me.Y + 0.7 * Height)})
                g.DrawLines(myPen, New PointF() {New PointF(X + 0.25 * Width, Y + 0.7 * Height), New PointF(X + 0.7 * Width, Me.Y + 0.3 * Height)})
            End If

            Dim size As SizeF
            Dim fontA As New Font("Arial", 8, FontStyle.Bold, GraphicsUnit.Pixel, 0, False)
            size = g.MeasureString("C", fontA)

            Dim ax, ay As Integer
            If Me.FlippedH Then
                ax = Me.X + 1.3 * (Me.Width - size.Width) / 2
                ay = Me.Y + Me.Height - size.Height
            Else
                ax = Me.X + (Me.Width - size.Width) / 2
                ay = Me.Y + Me.Height - size.Height
            End If

            g.SmoothingMode = SmoothingMode.AntiAlias
            g.TextRenderingHint = Text.TextRenderingHint.AntiAlias
            g.DrawString("C", fontA, New SolidBrush(Me.LineColor), ax, ay)

            g.EndContainer(gContainer)
            gp.Dispose()

        End Sub

        Public Sub DrawRoundRect(ByVal g As Graphics, ByVal p As Pen, ByVal x As Integer, ByVal y As Integer, ByVal width As Integer, ByVal height As Integer, ByVal radius As Integer, ByVal myBrush As Brush)

            Dim gp As Drawing2D.GraphicsPath = New Drawing2D.GraphicsPath

            gp.AddLine(x + radius, y, x + width - radius, y)
            gp.AddArc(x + width - radius, y, radius, radius, 270, 90)
            gp.AddLine(x + width, y + radius, x + width, y + height - radius)
            gp.AddArc(x + width - radius, y + height - radius, radius, radius, 0, 90)
            gp.AddLine(x + width - radius, y + height, x + radius, y + height)
            gp.AddArc(x, y + height - radius, radius, radius, 90, 90)
            gp.AddLine(x, y + height - radius, x, y + radius)
            gp.AddArc(x, y, radius, radius, 180, 90)

            gp.CloseFigure()

            g.DrawPath(p, gp)
            g.FillPath(myBrush, gp)

            gp.Dispose()

        End Sub

    End Class

    <Serializable()> Public Class ReactorEquilibriumGraphic

        Inherits ShapeGraphic

#Region "Constructors"

        Public Sub New()
            Me.TipoObjeto = GraphicObjects.TipoObjeto.RCT_Equilibrium
            Me.Description = "ReatorEquilibrio"
            Me.m_splitpoint = True
        End Sub

        Public Sub New(ByVal graphicPosition As Point)
            Me.New()
            Me.SetPosition(graphicPosition)
        End Sub

        Public Sub New(ByVal posX As Integer, ByVal posY As Integer)
            Me.New(New Point(posX, posY))
        End Sub

        Public Sub New(ByVal graphicPosition As Point, ByVal graphicSize As Size)
            Me.New(graphicPosition)
            Me.SetSize(graphicSize)
            Me.AutoSize = False
        End Sub

        Public Sub New(ByVal posX As Integer, ByVal posY As Integer, ByVal graphicSize As Size)
            Me.New(New Point(posX, posY), graphicSize)
        End Sub

        Public Sub New(ByVal posX As Integer, ByVal posY As Integer, ByVal width As Integer, ByVal height As Integer)
            Me.New(New Point(posX, posY), New Size(width, height))
        End Sub

        Public Sub New(ByVal graphicPosition As Point, ByVal Rotation As Single)
            Me.New()
            Me.SetPosition(graphicPosition)
            Me.Rotation = Rotation
        End Sub

        Public Sub New(ByVal posX As Integer, ByVal posY As Integer, ByVal Rotation As Single)
            Me.New(New Point(posX, posY), Rotation)
        End Sub

        Public Sub New(ByVal graphicPosition As Point, ByVal graphicSize As Size, ByVal Rotation As Single)
            Me.New(graphicPosition, Rotation)
            Me.SetSize(graphicSize)
            Me.AutoSize = False
        End Sub

        Public Sub New(ByVal posX As Integer, ByVal posY As Integer, ByVal graphicSize As Size, ByVal Rotation As Single)
            Me.New(New Point(posX, posY), graphicSize, Rotation)
        End Sub

        Public Sub New(ByVal posX As Integer, ByVal posY As Integer, ByVal width As Integer, _
                               ByVal height As Integer, ByVal Rotation As Single)
            Me.New(New Point(posX, posY), New Size(width, height), Rotation)
        End Sub

#End Region

        Public Overrides Sub CreateConnectors(InCount As Integer, OutCount As Integer)
            Dim myIC1 As New ConnectionPoint
            myIC1.Position = New Point(X + 0.125 * Width, Y + 0.5 * Height)
            myIC1.Type = ConType.ConIn

            Dim myIC2 As New ConnectionPoint
            myIC2.Position = New Point(X + 0.125 * Width, Y + 0.7 * Height)
            myIC2.Type = ConType.ConEn

            Dim myOC1 As New ConnectionPoint
            myOC1.Position = New Point(X + 0.5 * Width, Y)
            myOC1.Type = ConType.ConOut

            Dim myOC2 As New ConnectionPoint
            myOC2.Position = New Point(X + 0.5 * Width, Y + Height)
            myOC2.Type = ConType.ConOut

            With InputConnectors

                If .Count <> 0 Then
                    If Me.FlippedH Then
                        .Item(0).Position = New Point(X + 0.875 * Width, Y + 0.5 * Height)
                        .Item(1).Position = New Point(X + 0.875 * Width, Y + 0.7 * Height)
                    Else
                        .Item(0).Position = New Point(X + 0.125 * Width, Y + 0.5 * Height)
                        .Item(1).Position = New Point(X + 0.125 * Width, Y + 0.7 * Height)
                    End If
                Else
                    .Add(myIC1)
                    .Add(myIC2)
                End If

            End With

            With OutputConnectors

                If .Count <> 0 Then
                    If Me.FlippedH Then
                        .Item(0).Position = New Point(X + 0.3 * Width, Y + (0.1 + 0.127 / 2) * Height)
                        .Item(1).Position = New Point(X + 0.3 * Width, Y + (0.773 + 0.127 / 2) * Height)
                    Else
                        .Item(0).Position = New Point(X + 0.7 * Width, Y + (0.1 + 0.127 / 2) * Height)
                        .Item(1).Position = New Point(X + 0.7 * Width, Y + (0.773 + 0.127 / 2) * Height)
                    End If
                Else
                    .Add(myOC1)
                    .Add(myOC2)
                End If

            End With

            With Me.EnergyConnector
                If Me.FlippedH Then
                    .Position = New Point(X + 0.875 * Width, Y + 0.7 * Height)
                Else
                    .Position = New Point(X + 0.125 * Width, Y + 0.7 * Height)
                End If
            End With

        End Sub

        Public Overrides Sub Draw(ByVal g As System.Drawing.Graphics)

            CreateConnectors(0, 0)

            UpdateStatus(Me)

            Dim pt As Point
            Dim raio, angulo As Double
            Dim con As ConnectionPoint
            For Each con In Me.InputConnectors
                pt = con.Position
                raio = ((pt.X - Me.X) ^ 2 + (pt.Y - Me.Y) ^ 2) ^ 0.5
                angulo = Math.Atan2(pt.Y - Me.Y, pt.X - Me.X)
                pt.X = Me.X + raio * Math.Cos(angulo + Me.Rotation / 360 * 2 * Math.PI)
                pt.Y = Me.Y + raio * Math.Sin(angulo + Me.Rotation / 360 * 2 * Math.PI)
                con.Position = pt
            Next
            For Each con In Me.OutputConnectors
                pt = con.Position
                raio = ((pt.X - Me.X) ^ 2 + (pt.Y - Me.Y) ^ 2) ^ 0.5
                angulo = Math.Atan2(pt.Y - Me.Y, pt.X - Me.X)
                pt.X = Me.X + raio * Math.Cos(angulo + Me.Rotation / 360 * 2 * Math.PI)
                pt.Y = Me.Y + raio * Math.Sin(angulo + Me.Rotation / 360 * 2 * Math.PI)
                con.Position = pt
            Next
            With Me.EnergyConnector
                pt = .Position
                raio = ((pt.X - Me.X) ^ 2 + (pt.Y - Me.Y) ^ 2) ^ 0.5
                angulo = Math.Atan2(pt.Y - Me.Y, pt.X - Me.X)
                pt.X = Me.X + raio * Math.Cos(angulo + Me.Rotation / 360 * 2 * Math.PI)
                pt.Y = Me.Y + raio * Math.Sin(angulo + Me.Rotation / 360 * 2 * Math.PI)
                .Position = pt
            End With

            Dim gContainer As System.Drawing.Drawing2D.GraphicsContainer
            Dim myMatrix As Drawing2D.Matrix
            gContainer = g.BeginContainer()
            myMatrix = g.Transform()
            If m_Rotation <> 0 Then
                myMatrix.RotateAt(m_Rotation, New PointF(X, Y), Drawing.Drawing2D.MatrixOrder.Append)
                g.Transform = myMatrix
            End If

            Dim rect2 As New Rectangle(X + 0.123 * Width, Y + 0.5 * Height, 0.127 * Width, 0.127 * Height)
            Dim rect3 As New Rectangle(X + 0.7 * Width, Y + 0.1 * Height, 0.127 * Width, 0.127 * Height)
            Dim rect4 As New Rectangle(X + 0.7 * Width, Y + 0.773 * Height, 0.127 * Width, 0.127 * Height)
            If Me.FlippedH = True Then
                rect2 = New Rectangle(X + (1 - 0.123) * Width, Y + 0.5 * Height, 0.127 * Width, 0.127 * Height)
                rect3 = New Rectangle(X + 0.3 * Width, Y + 0.1 * Height, 0.127 * Width, 0.127 * Height)
                rect4 = New Rectangle(X + 0.3 * Width, Y + 0.773 * Height, 0.127 * Width, 0.127 * Height)
            End If

            Dim myPen As New Pen(Me.LineColor, Me.LineWidth)

            Dim myPen2 As New Pen(Color.White, 0)

            Dim rect As New Rectangle(X, Y, Width, Height)

            g.SmoothingMode = SmoothingMode.AntiAlias
            'g.DrawRectangle(myPen2, rect)
            If Me.FlippedH = True Then
                Me.DrawRoundRect(g, myPen, X + 0.4 * Width, Y, 0.45 * Width, Height, 10, Brushes.Transparent)
            Else
                Me.DrawRoundRect(g, myPen, X + 0.25 * Width, Y, 0.45 * Width, Height, 10, Brushes.Transparent)
            End If
            g.DrawRectangle(myPen, rect2)
            g.DrawRectangle(myPen, rect3)
            g.DrawRectangle(myPen, rect4)

            Dim strdist As SizeF = g.MeasureString(Me.Tag, New Font("Arial", 10, FontStyle.Bold, GraphicsUnit.Pixel, 0, False), New PointF(0, 0), New StringFormat(StringFormatFlags.NoClip, 0))
            Dim strx As Single = (Me.Width - strdist.Width) / 2
            'g.FillRectangle(Brushes.White, X + strx, Y + CSng(Height + 5), strdist.Width, strdist.Height)
            g.DrawString(Me.Tag, New Font("Arial", 10, FontStyle.Bold, GraphicsUnit.Pixel, 0, False), New SolidBrush(Me.LineColor), X + strx, Y + Height + 5)

            Dim gp As Drawing2D.GraphicsPath = New Drawing2D.GraphicsPath
            Dim radius As Integer = 3
            gp.AddLine(X + radius, Y, X + Width - radius, Y)
            gp.AddArc(X + Width - radius, Y, radius, radius, 270, 90)
            gp.AddLine(X + Width, Y + radius, X + Width, Y + Height - radius)
            gp.AddArc(X + Width - radius, Y + Height - radius, radius, radius, 0, 90)
            gp.AddLine(X + Width - radius, Y + Height, X + radius, Y + Height)
            gp.AddArc(X, Y + Height - radius, radius, radius, 90, 90)
            gp.AddLine(X, Y + Height - radius, X, Y + radius)
            gp.AddArc(X, Y, radius, radius, 180, 90)
            Dim lgb1 As LinearGradientBrush
            lgb1 = New LinearGradientBrush(rect, Me.GradientColor1, Me.GradientColor2, LinearGradientMode.Horizontal)
            lgb1.SetBlendTriangularShape(0.5)
            'lgb1.CenterColor = Me.GradientColor1
            'lgb1.SetBlendTriangularShape(0.5)
            'lgb1.SurroundColors = New Color() {Me.GradientColor2}
            'lgb1.WrapMode = WrapMode.Tile
            If Me.Fill Then
                If Me.GradientMode = False Then
                    g.FillRectangle(New SolidBrush(Me.FillColor), rect3)
                    g.FillRectangle(New SolidBrush(Me.FillColor), rect4)
                    g.FillRectangle(New SolidBrush(Me.FillColor), rect2)
                    If Me.FlippedH = True Then
                        Me.DrawRoundRect(g, myPen, X + 0.4 * Width, Y, 0.45 * Width, Height, 6, New SolidBrush(Me.FillColor))
                    Else
                        Me.DrawRoundRect(g, myPen, X + 0.25 * Width, Y, 0.45 * Width, Height, 6, New SolidBrush(Me.FillColor))
                    End If
                Else
                    g.FillRectangle(New SolidBrush(Me.FillColor), rect3)
                    g.FillRectangle(New SolidBrush(Me.FillColor), rect4)
                    g.FillRectangle(New SolidBrush(Me.FillColor), rect2)
                    If Me.FlippedH = True Then
                        Me.DrawRoundRect(g, myPen, X + 0.4 * Width, Y, 0.45 * Width, Height, 6, lgb1)
                    Else
                        Me.DrawRoundRect(g, myPen, X + 0.25 * Width, Y, 0.45 * Width, Height, 6, lgb1)
                    End If
                End If
            End If

            If Me.FlippedH Then
                g.DrawLines(myPen, New PointF() {New PointF(X + 0.4 * Width, Y + 0.3 * Height), New PointF(X + 0.85 * Width, Me.Y + 0.3 * Height)})
                g.DrawLines(myPen, New PointF() {New PointF(X + 0.4 * Width, Y + 0.7 * Height), New PointF(X + 0.85 * Width, Me.Y + 0.7 * Height)})
                g.DrawLines(myPen, New PointF() {New PointF(X + 0.4 * Width, Y + 0.3 * Height), New PointF(X + 0.85 * Width, Me.Y + 0.7 * Height)})
                g.DrawLines(myPen, New PointF() {New PointF(X + 0.4 * Width, Y + 0.7 * Height), New PointF(X + 0.85 * Width, Me.Y + 0.3 * Height)})
            Else
                g.DrawLines(myPen, New PointF() {New PointF(X + 0.25 * Width, Y + 0.3 * Height), New PointF(X + 0.7 * Width, Me.Y + 0.3 * Height)})
                g.DrawLines(myPen, New PointF() {New PointF(X + 0.25 * Width, Y + 0.7 * Height), New PointF(X + 0.7 * Width, Me.Y + 0.7 * Height)})
                g.DrawLines(myPen, New PointF() {New PointF(X + 0.25 * Width, Y + 0.3 * Height), New PointF(X + 0.7 * Width, Me.Y + 0.7 * Height)})
                g.DrawLines(myPen, New PointF() {New PointF(X + 0.25 * Width, Y + 0.7 * Height), New PointF(X + 0.7 * Width, Me.Y + 0.3 * Height)})
            End If

            Dim size As SizeF
            Dim fontA As New Font("Arial", 8, FontStyle.Bold, GraphicsUnit.Pixel, 0, False)
            size = g.MeasureString("E", fontA)

            Dim ax, ay As Integer
            If Me.FlippedH Then
                ax = Me.X + 1.3 * (Me.Width - size.Width) / 2
                ay = Me.Y + Me.Height - size.Height
            Else
                ax = Me.X + (Me.Width - size.Width) / 2
                ay = Me.Y + Me.Height - size.Height
            End If

            g.SmoothingMode = SmoothingMode.AntiAlias
            g.TextRenderingHint = Text.TextRenderingHint.AntiAlias
            g.DrawString("E", fontA, New SolidBrush(Me.LineColor), ax, ay)

            g.EndContainer(gContainer)
            gp.Dispose()

        End Sub

        Public Sub DrawRoundRect(ByVal g As Graphics, ByVal p As Pen, ByVal x As Integer, ByVal y As Integer, ByVal width As Integer, ByVal height As Integer, ByVal radius As Integer, ByVal myBrush As Brush)

            Dim gp As Drawing2D.GraphicsPath = New Drawing2D.GraphicsPath

            gp.AddLine(x + radius, y, x + width - radius, y)
            gp.AddArc(x + width - radius, y, radius, radius, 270, 90)
            gp.AddLine(x + width, y + radius, x + width, y + height - radius)
            gp.AddArc(x + width - radius, y + height - radius, radius, radius, 0, 90)
            gp.AddLine(x + width - radius, y + height, x + radius, y + height)
            gp.AddArc(x, y + height - radius, radius, radius, 90, 90)
            gp.AddLine(x, y + height - radius, x, y + radius)
            gp.AddArc(x, y, radius, radius, 180, 90)

            gp.CloseFigure()

            g.DrawPath(p, gp)
            g.FillPath(myBrush, gp)

            gp.Dispose()

        End Sub

    End Class

    <Serializable()> Public Class ReactorGibbsGraphic

        Inherits ShapeGraphic

#Region "Constructors"

        Public Sub New()
            Me.TipoObjeto = GraphicObjects.TipoObjeto.RCT_Gibbs
            Me.Description = "ReatorGibbs"
            Me.m_splitpoint = True
        End Sub

        Public Sub New(ByVal graphicPosition As Point)
            Me.New()
            Me.SetPosition(graphicPosition)
        End Sub

        Public Sub New(ByVal posX As Integer, ByVal posY As Integer)
            Me.New(New Point(posX, posY))
        End Sub

        Public Sub New(ByVal graphicPosition As Point, ByVal graphicSize As Size)
            Me.New(graphicPosition)
            Me.SetSize(graphicSize)
            Me.AutoSize = False
        End Sub

        Public Sub New(ByVal posX As Integer, ByVal posY As Integer, ByVal graphicSize As Size)
            Me.New(New Point(posX, posY), graphicSize)
        End Sub

        Public Sub New(ByVal posX As Integer, ByVal posY As Integer, ByVal width As Integer, ByVal height As Integer)
            Me.New(New Point(posX, posY), New Size(width, height))
        End Sub

        Public Sub New(ByVal graphicPosition As Point, ByVal Rotation As Single)
            Me.New()
            Me.SetPosition(graphicPosition)
            Me.Rotation = Rotation
        End Sub

        Public Sub New(ByVal posX As Integer, ByVal posY As Integer, ByVal Rotation As Single)
            Me.New(New Point(posX, posY), Rotation)
        End Sub

        Public Sub New(ByVal graphicPosition As Point, ByVal graphicSize As Size, ByVal Rotation As Single)
            Me.New(graphicPosition, Rotation)
            Me.SetSize(graphicSize)
            Me.AutoSize = False
        End Sub

        Public Sub New(ByVal posX As Integer, ByVal posY As Integer, ByVal graphicSize As Size, ByVal Rotation As Single)
            Me.New(New Point(posX, posY), graphicSize, Rotation)
        End Sub

        Public Sub New(ByVal posX As Integer, ByVal posY As Integer, ByVal width As Integer, _
                               ByVal height As Integer, ByVal Rotation As Single)
            Me.New(New Point(posX, posY), New Size(width, height), Rotation)
        End Sub

#End Region


        Public Overrides Sub CreateConnectors(InCount As Integer, OutCount As Integer)
            Dim myIC1 As New ConnectionPoint
            myIC1.Position = New Point(X + 0.125 * Width, Y + 0.5 * Height)
            myIC1.Type = ConType.ConIn

            Dim myIC2 As New ConnectionPoint
            myIC2.Position = New Point(X + 0.125 * Width, Y + 0.7 * Height)
            myIC2.Type = ConType.ConEn

            Dim myOC1 As New ConnectionPoint
            myOC1.Position = New Point(X + 0.5 * Width, Y)
            myOC1.Type = ConType.ConOut

            Dim myOC2 As New ConnectionPoint
            myOC2.Position = New Point(X + 0.5 * Width, Y + Height)
            myOC2.Type = ConType.ConOut

            With InputConnectors

                If .Count <> 0 Then
                    If Me.FlippedH Then
                        .Item(0).Position = New Point(X + 0.875 * Width, Y + 0.5 * Height)
                        .Item(1).Position = New Point(X + 0.875 * Width, Y + 0.7 * Height)
                    Else
                        .Item(0).Position = New Point(X + 0.125 * Width, Y + 0.5 * Height)
                        .Item(1).Position = New Point(X + 0.125 * Width, Y + 0.7 * Height)
                    End If
                Else
                    .Add(myIC1)
                    .Add(myIC2)
                End If

            End With

            With OutputConnectors

                If .Count <> 0 Then
                    If Me.FlippedH Then
                        .Item(0).Position = New Point(X + 0.3 * Width, Y + (0.1 + 0.127 / 2) * Height)
                        .Item(1).Position = New Point(X + 0.3 * Width, Y + (0.773 + 0.127 / 2) * Height)
                    Else
                        .Item(0).Position = New Point(X + 0.7 * Width, Y + (0.1 + 0.127 / 2) * Height)
                        .Item(1).Position = New Point(X + 0.7 * Width, Y + (0.773 + 0.127 / 2) * Height)
                    End If
                Else
                    .Add(myOC1)
                    .Add(myOC2)
                End If

            End With

            With Me.EnergyConnector
                If Me.FlippedH Then
                    .Position = New Point(X + 0.875 * Width, Y + 0.7 * Height)
                Else
                    .Position = New Point(X + 0.125 * Width, Y + 0.7 * Height)
                End If
            End With


        End Sub

        Public Overrides Sub Draw(ByVal g As System.Drawing.Graphics)

            CreateConnectors(0, 0)

            UpdateStatus(Me)

            Dim pt As Point
            Dim raio, angulo As Double
            Dim con As ConnectionPoint
            For Each con In Me.InputConnectors
                pt = con.Position
                raio = ((pt.X - Me.X) ^ 2 + (pt.Y - Me.Y) ^ 2) ^ 0.5
                angulo = Math.Atan2(pt.Y - Me.Y, pt.X - Me.X)
                pt.X = Me.X + raio * Math.Cos(angulo + Me.Rotation / 360 * 2 * Math.PI)
                pt.Y = Me.Y + raio * Math.Sin(angulo + Me.Rotation / 360 * 2 * Math.PI)
                con.Position = pt
            Next
            For Each con In Me.OutputConnectors
                pt = con.Position
                raio = ((pt.X - Me.X) ^ 2 + (pt.Y - Me.Y) ^ 2) ^ 0.5
                angulo = Math.Atan2(pt.Y - Me.Y, pt.X - Me.X)
                pt.X = Me.X + raio * Math.Cos(angulo + Me.Rotation / 360 * 2 * Math.PI)
                pt.Y = Me.Y + raio * Math.Sin(angulo + Me.Rotation / 360 * 2 * Math.PI)
                con.Position = pt
            Next
            With Me.EnergyConnector
                pt = .Position
                raio = ((pt.X - Me.X) ^ 2 + (pt.Y - Me.Y) ^ 2) ^ 0.5
                angulo = Math.Atan2(pt.Y - Me.Y, pt.X - Me.X)
                pt.X = Me.X + raio * Math.Cos(angulo + Me.Rotation / 360 * 2 * Math.PI)
                pt.Y = Me.Y + raio * Math.Sin(angulo + Me.Rotation / 360 * 2 * Math.PI)
                .Position = pt
            End With

            Dim gContainer As System.Drawing.Drawing2D.GraphicsContainer
            Dim myMatrix As Drawing2D.Matrix
            gContainer = g.BeginContainer()
            myMatrix = g.Transform()
            If m_Rotation <> 0 Then
                myMatrix.RotateAt(m_Rotation, New PointF(X, Y), Drawing.Drawing2D.MatrixOrder.Append)
                g.Transform = myMatrix
            End If

            Dim rect2 As New Rectangle(X + 0.123 * Width, Y + 0.5 * Height, 0.127 * Width, 0.127 * Height)
            Dim rect3 As New Rectangle(X + 0.7 * Width, Y + 0.1 * Height, 0.127 * Width, 0.127 * Height)
            Dim rect4 As New Rectangle(X + 0.7 * Width, Y + 0.773 * Height, 0.127 * Width, 0.127 * Height)
            If Me.FlippedH = True Then
                rect2 = New Rectangle(X + (1 - 0.123) * Width, Y + 0.5 * Height, 0.127 * Width, 0.127 * Height)
                rect3 = New Rectangle(X + 0.3 * Width, Y + 0.1 * Height, 0.127 * Width, 0.127 * Height)
                rect4 = New Rectangle(X + 0.3 * Width, Y + 0.773 * Height, 0.127 * Width, 0.127 * Height)
            End If

            Dim myPen As New Pen(Me.LineColor, Me.LineWidth)

            Dim myPen2 As New Pen(Color.White, 0)

            Dim rect As New Rectangle(X, Y, Width, Height)

            g.SmoothingMode = SmoothingMode.AntiAlias
            'g.DrawRectangle(myPen2, rect)
            If Me.FlippedH = True Then
                Me.DrawRoundRect(g, myPen, X + 0.4 * Width, Y, 0.45 * Width, Height, 10, Brushes.Transparent)
            Else
                Me.DrawRoundRect(g, myPen, X + 0.25 * Width, Y, 0.45 * Width, Height, 10, Brushes.Transparent)
            End If
            g.DrawRectangle(myPen, rect2)
            g.DrawRectangle(myPen, rect3)
            g.DrawRectangle(myPen, rect4)

            Dim strdist As SizeF = g.MeasureString(Me.Tag, New Font("Arial", 10, FontStyle.Bold, GraphicsUnit.Pixel, 0, False), New PointF(0, 0), New StringFormat(StringFormatFlags.NoClip, 0))
            Dim strx As Single = (Me.Width - strdist.Width) / 2
            'g.FillRectangle(Brushes.White, X + strx, Y + CSng(Height + 5), strdist.Width, strdist.Height)
            g.DrawString(Me.Tag, New Font("Arial", 10, FontStyle.Bold, GraphicsUnit.Pixel, 0, False), New SolidBrush(Me.LineColor), X + strx, Y + Height + 5)

            Dim gp As Drawing2D.GraphicsPath = New Drawing2D.GraphicsPath
            Dim radius As Integer = 3
            gp.AddLine(X + radius, Y, X + Width - radius, Y)
            gp.AddArc(X + Width - radius, Y, radius, radius, 270, 90)
            gp.AddLine(X + Width, Y + radius, X + Width, Y + Height - radius)
            gp.AddArc(X + Width - radius, Y + Height - radius, radius, radius, 0, 90)
            gp.AddLine(X + Width - radius, Y + Height, X + radius, Y + Height)
            gp.AddArc(X, Y + Height - radius, radius, radius, 90, 90)
            gp.AddLine(X, Y + Height - radius, X, Y + radius)
            gp.AddArc(X, Y, radius, radius, 180, 90)
            Dim lgb1 As LinearGradientBrush
            lgb1 = New LinearGradientBrush(rect, Me.GradientColor1, Me.GradientColor2, LinearGradientMode.Horizontal)
            lgb1.SetBlendTriangularShape(0.5)
            'lgb1.CenterColor = Me.GradientColor1
            'lgb1.SetBlendTriangularShape(0.5)
            'lgb1.SurroundColors = New Color() {Me.GradientColor2}
            'lgb1.WrapMode = WrapMode.Tile
            If Me.Fill Then
                If Me.GradientMode = False Then
                    g.FillRectangle(New SolidBrush(Me.FillColor), rect3)
                    g.FillRectangle(New SolidBrush(Me.FillColor), rect4)
                    g.FillRectangle(New SolidBrush(Me.FillColor), rect2)
                    If Me.FlippedH = True Then
                        Me.DrawRoundRect(g, myPen, X + 0.4 * Width, Y, 0.45 * Width, Height, 6, New SolidBrush(Me.FillColor))
                    Else
                        Me.DrawRoundRect(g, myPen, X + 0.25 * Width, Y, 0.45 * Width, Height, 6, New SolidBrush(Me.FillColor))
                    End If
                Else
                    g.FillRectangle(New SolidBrush(Me.FillColor), rect3)
                    g.FillRectangle(New SolidBrush(Me.FillColor), rect4)
                    g.FillRectangle(New SolidBrush(Me.FillColor), rect2)
                    If Me.FlippedH = True Then
                        Me.DrawRoundRect(g, myPen, X + 0.4 * Width, Y, 0.45 * Width, Height, 6, lgb1)
                    Else
                        Me.DrawRoundRect(g, myPen, X + 0.25 * Width, Y, 0.45 * Width, Height, 6, lgb1)
                    End If
                End If
            End If

            If Me.FlippedH Then
                g.DrawLines(myPen, New PointF() {New PointF(X + 0.4 * Width, Y + 0.3 * Height), New PointF(X + 0.85 * Width, Me.Y + 0.3 * Height)})
                g.DrawLines(myPen, New PointF() {New PointF(X + 0.4 * Width, Y + 0.7 * Height), New PointF(X + 0.85 * Width, Me.Y + 0.7 * Height)})
                g.DrawLines(myPen, New PointF() {New PointF(X + 0.4 * Width, Y + 0.3 * Height), New PointF(X + 0.85 * Width, Me.Y + 0.7 * Height)})
                g.DrawLines(myPen, New PointF() {New PointF(X + 0.4 * Width, Y + 0.7 * Height), New PointF(X + 0.85 * Width, Me.Y + 0.3 * Height)})
            Else
                g.DrawLines(myPen, New PointF() {New PointF(X + 0.25 * Width, Y + 0.3 * Height), New PointF(X + 0.7 * Width, Me.Y + 0.3 * Height)})
                g.DrawLines(myPen, New PointF() {New PointF(X + 0.25 * Width, Y + 0.7 * Height), New PointF(X + 0.7 * Width, Me.Y + 0.7 * Height)})
                g.DrawLines(myPen, New PointF() {New PointF(X + 0.25 * Width, Y + 0.3 * Height), New PointF(X + 0.7 * Width, Me.Y + 0.7 * Height)})
                g.DrawLines(myPen, New PointF() {New PointF(X + 0.25 * Width, Y + 0.7 * Height), New PointF(X + 0.7 * Width, Me.Y + 0.3 * Height)})
            End If

            Dim size As SizeF
            Dim fontA As New Font("Arial", 8, FontStyle.Bold, GraphicsUnit.Pixel, 0, False)
            size = g.MeasureString("G", fontA)

            Dim ax, ay As Integer
            If Me.FlippedH Then
                ax = Me.X + 1.3 * (Me.Width - size.Width) / 2
                ay = Me.Y + Me.Height - size.Height
            Else
                ax = Me.X + (Me.Width - size.Width) / 2
                ay = Me.Y + Me.Height - size.Height
            End If

            g.SmoothingMode = SmoothingMode.AntiAlias
            g.TextRenderingHint = Text.TextRenderingHint.AntiAlias
            g.DrawString("G", fontA, New SolidBrush(Me.LineColor), ax, ay)

            g.EndContainer(gContainer)
            gp.Dispose()

        End Sub

        Public Sub DrawRoundRect(ByVal g As Graphics, ByVal p As Pen, ByVal x As Integer, ByVal y As Integer, ByVal width As Integer, ByVal height As Integer, ByVal radius As Integer, ByVal myBrush As Brush)

            Dim gp As Drawing2D.GraphicsPath = New Drawing2D.GraphicsPath

            gp.AddLine(x + radius, y, x + width - radius, y)
            gp.AddArc(x + width - radius, y, radius, radius, 270, 90)
            gp.AddLine(x + width, y + radius, x + width, y + height - radius)
            gp.AddArc(x + width - radius, y + height - radius, radius, radius, 0, 90)
            gp.AddLine(x + width - radius, y + height, x + radius, y + height)
            gp.AddArc(x, y + height - radius, radius, radius, 90, 90)
            gp.AddLine(x, y + height - radius, x, y + radius)
            gp.AddArc(x, y, radius, radius, 180, 90)

            gp.CloseFigure()

            g.DrawPath(p, gp)
            g.FillPath(myBrush, gp)

            gp.Dispose()

        End Sub

    End Class

    <Serializable()> Public Class ReactorCSTRGraphic

        Inherits ShapeGraphic

#Region "Constructors"

        Public Sub New()
            Me.TipoObjeto = GraphicObjects.TipoObjeto.RCT_CSTR
            Me.Description = "ReatorCSTR"
        End Sub

        Public Sub New(ByVal graphicPosition As Point)
            Me.New()
            Me.SetPosition(graphicPosition)
        End Sub

        Public Sub New(ByVal posX As Integer, ByVal posY As Integer)
            Me.New(New Point(posX, posY))
        End Sub

        Public Sub New(ByVal graphicPosition As Point, ByVal graphicSize As Size)
            Me.New(graphicPosition)
            Me.SetSize(graphicSize)
            Me.AutoSize = False
        End Sub

        Public Sub New(ByVal posX As Integer, ByVal posY As Integer, ByVal graphicSize As Size)
            Me.New(New Point(posX, posY), graphicSize)
        End Sub

        Public Sub New(ByVal posX As Integer, ByVal posY As Integer, ByVal width As Integer, ByVal height As Integer)
            Me.New(New Point(posX, posY), New Size(width, height))
        End Sub

        Public Sub New(ByVal graphicPosition As Point, ByVal Rotation As Single)
            Me.New()
            Me.SetPosition(graphicPosition)
            Me.Rotation = Rotation
        End Sub

        Public Sub New(ByVal posX As Integer, ByVal posY As Integer, ByVal Rotation As Single)
            Me.New(New Point(posX, posY), Rotation)
        End Sub

        Public Sub New(ByVal graphicPosition As Point, ByVal graphicSize As Size, ByVal Rotation As Single)
            Me.New(graphicPosition, Rotation)
            Me.SetSize(graphicSize)
            Me.AutoSize = False
        End Sub

        Public Sub New(ByVal posX As Integer, ByVal posY As Integer, ByVal graphicSize As Size, ByVal Rotation As Single)
            Me.New(New Point(posX, posY), graphicSize, Rotation)
        End Sub

        Public Sub New(ByVal posX As Integer, ByVal posY As Integer, ByVal width As Integer, _
                               ByVal height As Integer, ByVal Rotation As Single)
            Me.New(New Point(posX, posY), New Size(width, height), Rotation)
        End Sub

#End Region

        Public Overrides Sub CreateConnectors(InCount As Integer, OutCount As Integer)
            Dim myIC1 As New ConnectionPoint
            myIC1.Position = New Point(X, Y + 0.5 * Height)
            myIC1.Type = ConType.ConIn

            Dim myIC2 As New ConnectionPoint
            myIC2.Position = New Point(X + 0.125 * Width, Y + 0.7 * Height)
            myIC2.Type = ConType.ConEn

            Dim myOC1 As New ConnectionPoint
            myOC1.Position = New Point(X + Width, Y + 0.5 * Height)
            myOC1.Type = ConType.ConOut

            Me.EnergyConnector.Position = New Point(X + 0.5 * Width, Y + Height)
            Me.EnergyConnector.Type = ConType.ConEn

            With InputConnectors

                If .Count <> 0 Then
                    If Me.FlippedH Then
                        .Item(0).Position = New Point(X + Width, Y + 0.5 * Height)
                        .Item(1).Position = New Point(X + 0.125 * Width, Y + 0.7 * Height)
                    Else
                        .Item(0).Position = New Point(X, Y + 0.5 * Height)
                        .Item(1).Position = New Point(X + 0.125 * Width, Y + 0.7 * Height)
                    End If
                Else
                    .Add(myIC1)
                    .Add(myIC2)
                End If

            End With

            With OutputConnectors

                If .Count <> 0 Then
                    If Me.FlippedH Then
                        .Item(0).Position = New Point(X, Y + 0.5 * Height)
                    Else
                        .Item(0).Position = New Point(X + Width, Y + 0.5 * Height)
                    End If
                Else
                    .Add(myOC1)
                End If

            End With


        End Sub


        Public Overrides Sub Draw(ByVal g As System.Drawing.Graphics)

            CreateConnectors(0, 0)

            UpdateStatus(Me)

            Dim pt As Point
            Dim raio, angulo As Double
            Dim con As ConnectionPoint
            For Each con In Me.InputConnectors
                pt = con.Position
                raio = ((pt.X - Me.X) ^ 2 + (pt.Y - Me.Y) ^ 2) ^ 0.5
                angulo = Math.Atan2(pt.Y - Me.Y, pt.X - Me.X)
                pt.X = Me.X + raio * Math.Cos(angulo + Me.Rotation / 360 * 2 * Math.PI)
                pt.Y = Me.Y + raio * Math.Sin(angulo + Me.Rotation / 360 * 2 * Math.PI)
                con.Position = pt
            Next
            For Each con In Me.OutputConnectors
                pt = con.Position
                raio = ((pt.X - Me.X) ^ 2 + (pt.Y - Me.Y) ^ 2) ^ 0.5
                angulo = Math.Atan2(pt.Y - Me.Y, pt.X - Me.X)
                pt.X = Me.X + raio * Math.Cos(angulo + Me.Rotation / 360 * 2 * Math.PI)
                pt.Y = Me.Y + raio * Math.Sin(angulo + Me.Rotation / 360 * 2 * Math.PI)
                con.Position = pt
            Next
            With Me.EnergyConnector
                pt = .Position
                raio = ((pt.X - Me.X) ^ 2 + (pt.Y - Me.Y) ^ 2) ^ 0.5
                angulo = Math.Atan2(pt.Y - Me.Y, pt.X - Me.X)
                pt.X = Me.X + raio * Math.Cos(angulo + Me.Rotation / 360 * 2 * Math.PI)
                pt.Y = Me.Y + raio * Math.Sin(angulo + Me.Rotation / 360 * 2 * Math.PI)
                .Position = pt
            End With

            Dim gContainer As System.Drawing.Drawing2D.GraphicsContainer
            Dim myMatrix As Drawing2D.Matrix
            gContainer = g.BeginContainer()
            myMatrix = g.Transform()
            If m_Rotation <> 0 Then
                myMatrix.RotateAt(m_Rotation, New PointF(X, Y), Drawing.Drawing2D.MatrixOrder.Append)
                g.Transform = myMatrix
            End If

            Dim rect1 As New Rectangle(X + 0.1 * Width, Y + 0.1 * Height, 0.8 * Width, 0.8 * Height)
            Dim rect2 As New RectangleF(X + 0.1 * Width, Y, 0.8 * Width, 0.2 * Height)
            Dim rect3 As New RectangleF(X + 0.1 * Width, Y + 0.8 * Height, 0.8 * Width, 0.2 * Height)

            Dim myPen As New Pen(Me.LineColor, Me.LineWidth)

            Dim myPen2 As New Pen(Color.White, 0)

            Dim rect As New Rectangle(X, Y, Width, Height)

            g.SmoothingMode = SmoothingMode.AntiAlias
            'g.DrawRectangle(myPen2, rect)
            g.DrawEllipse(myPen, rect3)
            g.DrawRectangle(myPen, rect1)

            Dim strdist As SizeF = g.MeasureString(Me.Tag, New Font("Arial", 10, FontStyle.Bold, GraphicsUnit.Pixel, 0, False), New PointF(0, 0), New StringFormat(StringFormatFlags.NoClip, 0))
            Dim strx As Single = (Me.Width - strdist.Width) / 2
            'g.FillRectangle(Brushes.White, X + strx, Y + CSng(Height + 5), strdist.Width, strdist.Height)
            g.DrawString(Me.Tag, New Font("Arial", 10, FontStyle.Bold, GraphicsUnit.Pixel, 0, False), New SolidBrush(Me.LineColor), X + strx, Y + Height + 5)

            Dim lgb1 As New LinearGradientBrush(rect1, Me.GradientColor1, Me.GradientColor2, LinearGradientMode.Horizontal)
            lgb1.SetBlendTriangularShape(0.5)
            Dim lgb2 As New LinearGradientBrush(rect3, Me.GradientColor1, Me.GradientColor2, LinearGradientMode.Horizontal)
            lgb2.SetBlendTriangularShape(0.5)

            If Me.Fill Then
                If Me.GradientMode = True Then
                    g.FillRectangle(lgb1, rect1)
                    g.FillEllipse(lgb1, rect2)
                    g.FillEllipse(lgb2, rect3)
                Else
                    g.FillRectangle(New SolidBrush(Me.FillColor), rect1)
                    g.FillEllipse(New SolidBrush(Me.FillColor), rect2)
                    g.FillEllipse(New SolidBrush(Me.FillColor), rect3)
                End If
            End If

            g.DrawEllipse(myPen, rect2)

            If Me.Fill Then
                If Me.GradientMode = True Then
                    g.FillEllipse(lgb1, rect2)
                Else
                    g.FillEllipse(New SolidBrush(Me.FillColor), rect2)
                End If
            End If

            g.DrawLines(myPen, New PointF() {New PointF(X + 0.5 * Width, Y - 0.1 * Height), New PointF(X + 0.5 * Width, Y + 0.7 * Height)})
            g.DrawEllipse(myPen, New RectangleF(X + 0.2 * Width, Y + 0.6 * Height, 0.3 * Width, 0.1 * Height))
            g.DrawEllipse(myPen, New RectangleF(X + 0.5 * Width, Y + 0.6 * Height, 0.3 * Width, 0.1 * Height))

            g.EndContainer(gContainer)

        End Sub

    End Class

    <Serializable()> Public Class ReactorPFRGraphic

        Inherits ShapeGraphic

#Region "Constructors"

        Public Sub New()
            Me.TipoObjeto = GraphicObjects.TipoObjeto.RCT_PFR
            Me.Description = "ReatorPFR"
        End Sub

        Public Sub New(ByVal graphicPosition As Point)
            Me.New()
            Me.SetPosition(graphicPosition)
        End Sub

        Public Sub New(ByVal posX As Integer, ByVal posY As Integer)
            Me.New(New Point(posX, posY))
        End Sub

        Public Sub New(ByVal graphicPosition As Point, ByVal graphicSize As Size)
            Me.New(graphicPosition)
            Me.SetSize(graphicSize)
            Me.AutoSize = False
        End Sub

        Public Sub New(ByVal posX As Integer, ByVal posY As Integer, ByVal graphicSize As Size)
            Me.New(New Point(posX, posY), graphicSize)
        End Sub

        Public Sub New(ByVal posX As Integer, ByVal posY As Integer, ByVal width As Integer, ByVal height As Integer)
            Me.New(New Point(posX, posY), New Size(width, height))
        End Sub

        Public Sub New(ByVal graphicPosition As Point, ByVal Rotation As Single)
            Me.New()
            Me.SetPosition(graphicPosition)
            Me.Rotation = Rotation
        End Sub

        Public Sub New(ByVal posX As Integer, ByVal posY As Integer, ByVal Rotation As Single)
            Me.New(New Point(posX, posY), Rotation)
        End Sub

        Public Sub New(ByVal graphicPosition As Point, ByVal graphicSize As Size, ByVal Rotation As Single)
            Me.New(graphicPosition, Rotation)
            Me.SetSize(graphicSize)
            Me.AutoSize = False
        End Sub

        Public Sub New(ByVal posX As Integer, ByVal posY As Integer, ByVal graphicSize As Size, ByVal Rotation As Single)
            Me.New(New Point(posX, posY), graphicSize, Rotation)
        End Sub

        Public Sub New(ByVal posX As Integer, ByVal posY As Integer, ByVal width As Integer, _
                               ByVal height As Integer, ByVal Rotation As Single)
            Me.New(New Point(posX, posY), New Size(width, height), Rotation)
        End Sub

#End Region

        Public Overrides Sub CreateConnectors(InCount As Integer, OutCount As Integer)
            Dim myIC1 As New ConnectionPoint
            myIC1.Position = New Point(X, Y + 0.5 * Height)
            myIC1.Type = ConType.ConIn

            Dim myIC2 As New ConnectionPoint
            myIC2.Position = New Point(X + 0.5 * Width, Y + Height)
            myIC2.Type = ConType.ConEn

            Dim myOC1 As New ConnectionPoint
            myOC1.Position = New Point(X + Width, Y + 0.5 * Height)
            myOC1.Type = ConType.ConOut

            Me.EnergyConnector.Position = New Point(X + 0.5 * Width, Y + Height)
            Me.EnergyConnector.Type = ConType.ConEn

            With InputConnectors

                If .Count <> 0 Then
                    If Me.FlippedH Then
                        .Item(0).Position = New Point(X + Width, Y + 0.5 * Height)
                        .Item(1).Position = New Point(X + 0.5 * Width, Y + Height)
                    Else
                        .Item(0).Position = New Point(X, Y + 0.5 * Height)
                        .Item(1).Position = New Point(X + 0.5 * Width, Y + Height)
                    End If
                Else
                    .Add(myIC1)
                    .Add(myIC2)
                End If

            End With

            With OutputConnectors

                If .Count <> 0 Then
                    If Me.FlippedH Then
                        .Item(0).Position = New Point(X, Y + 0.5 * Height)
                    Else
                        .Item(0).Position = New Point(X + Width, Y + 0.5 * Height)
                    End If
                Else
                    .Add(myOC1)
                End If

            End With
        End Sub
        Public Overrides Sub Draw(ByVal g As System.Drawing.Graphics)

            CreateConnectors(0, 0)

            UpdateStatus(Me)

            Dim pt As Point
            Dim raio, angulo As Double
            Dim con As ConnectionPoint
            For Each con In Me.InputConnectors
                pt = con.Position
                raio = ((pt.X - Me.X) ^ 2 + (pt.Y - Me.Y) ^ 2) ^ 0.5
                angulo = Math.Atan2(pt.Y - Me.Y, pt.X - Me.X)
                pt.X = Me.X + raio * Math.Cos(angulo + Me.Rotation / 360 * 2 * Math.PI)
                pt.Y = Me.Y + raio * Math.Sin(angulo + Me.Rotation / 360 * 2 * Math.PI)
                con.Position = pt
            Next
            For Each con In Me.OutputConnectors
                pt = con.Position
                raio = ((pt.X - Me.X) ^ 2 + (pt.Y - Me.Y) ^ 2) ^ 0.5
                angulo = Math.Atan2(pt.Y - Me.Y, pt.X - Me.X)
                pt.X = Me.X + raio * Math.Cos(angulo + Me.Rotation / 360 * 2 * Math.PI)
                pt.Y = Me.Y + raio * Math.Sin(angulo + Me.Rotation / 360 * 2 * Math.PI)
                con.Position = pt
            Next
            With Me.EnergyConnector
                pt = .Position
                raio = ((pt.X - Me.X) ^ 2 + (pt.Y - Me.Y) ^ 2) ^ 0.5
                angulo = Math.Atan2(pt.Y - Me.Y, pt.X - Me.X)
                pt.X = Me.X + raio * Math.Cos(angulo + Me.Rotation / 360 * 2 * Math.PI)
                pt.Y = Me.Y + raio * Math.Sin(angulo + Me.Rotation / 360 * 2 * Math.PI)
                .Position = pt
            End With

            Dim gContainer As System.Drawing.Drawing2D.GraphicsContainer
            Dim myMatrix As Drawing2D.Matrix
            gContainer = g.BeginContainer()
            myMatrix = g.Transform()
            If m_Rotation <> 0 Then
                myMatrix.RotateAt(m_Rotation, New PointF(X, Y), _
                    Drawing.Drawing2D.MatrixOrder.Append)
                g.Transform = myMatrix
            End If
            Dim rect As New Rectangle(X, Y, Width, Height)
            Dim lgb1 As New LinearGradientBrush(rect, Me.GradientColor1, Me.GradientColor2, LinearGradientMode.Vertical)
            If Me.Fill Then
                If Me.GradientMode = False Then
                    g.FillRectangle(New SolidBrush(Me.FillColor), rect)
                Else
                    g.FillRectangle(lgb1, rect)
                End If
            End If
            Dim myPen As New Pen(Me.LineColor, Me.LineWidth)
            g.SmoothingMode = SmoothingMode.AntiAlias
            g.DrawRectangle(myPen, rect)

            Dim strdist As SizeF = g.MeasureString(Me.Tag, New Font("Arial", 10, FontStyle.Bold, GraphicsUnit.Pixel, 0, False), New PointF(0, 0), New StringFormat(StringFormatFlags.NoClip, 0))
            Dim strx As Single = (Me.Width - strdist.Width) / 2
            'g.FillRectangle(Brushes.White, X + strx, Y + CSng(Height + 5), strdist.Width, strdist.Height)
            g.DrawString(Me.Tag, New Font("Arial", 10, FontStyle.Bold, GraphicsUnit.Pixel, 0, False), New SolidBrush(Me.LineColor), X + strx, Y + Height + 5)

            Dim rec1 As New Rectangle(X + 0.1 * Width, Y, 0.8 * Width, Height)
            g.FillRectangle(New HatchBrush(HatchStyle.SmallCheckerBoard, Me.LineColor, Color.Transparent), rec1)
            g.DrawRectangle(myPen, rec1)

            g.EndContainer(gContainer)

        End Sub


    End Class

    <Serializable()> Public Class HeatExchangerGraphic
        Inherits ShapeGraphic

#Region "Constructors"
        Public Sub New()
            'Defines the unitop type.
            TipoObjeto = GraphicObjects.TipoObjeto.HeatExchanger
            'Creates 2 in and 2 out connection points.
            Me.m_mixpoint = True
            Me.m_splitpoint = True
            CreateConnectors(2, 2)
            Me.Description = "HeatExchanger"
        End Sub

        Public Sub New(ByVal graphicPosition As Point)
            Me.New()
            Me.SetPosition(graphicPosition)
        End Sub

        Public Sub New(ByVal posX As Integer, ByVal posY As Integer)
            Me.New(New Point(posX, posY))
        End Sub

        Public Sub New(ByVal graphicPosition As Point, ByVal graphicSize As Size)
            Me.New(graphicPosition)
            Me.SetSize(graphicSize)
            Me.AutoSize = False
        End Sub

        Public Sub New(ByVal posX As Integer, ByVal posY As Integer, ByVal graphicSize As Size)
            Me.New(New Point(posX, posY), graphicSize)
        End Sub

        Public Sub New(ByVal posX As Integer, ByVal posY As Integer, ByVal width As Integer, ByVal height As Integer)
            Me.New(New Point(posX, posY), New Size(width, height))
        End Sub

        Public Sub New(ByVal graphicPosition As Point, ByVal Rotation As Single)
            Me.New()
            Me.SetPosition(graphicPosition)
            Me.Rotation = Rotation
        End Sub

        Public Sub New(ByVal posX As Integer, ByVal posY As Integer, ByVal Rotation As Single)
            Me.New(New Point(posX, posY), Rotation)
        End Sub

        Public Sub New(ByVal graphicPosition As Point, ByVal graphicSize As Size, ByVal Rotation As Single)
            Me.New(graphicPosition, Rotation)
            Me.SetSize(graphicSize)
            Me.AutoSize = False
        End Sub

        Public Sub New(ByVal posX As Integer, ByVal posY As Integer, ByVal graphicSize As Size, ByVal Rotation As Single)
            Me.New(New Point(posX, posY), graphicSize, Rotation)
        End Sub

        Public Sub New(ByVal posX As Integer, ByVal posY As Integer, ByVal width As Integer, _
                               ByVal height As Integer, ByVal Rotation As Single)
            Me.New(New Point(posX, posY), New Size(width, height), Rotation)
        End Sub

#End Region

        Public Overrides Sub PositionConnectors()

            MyBase.PositionConnectors()

            With InputConnectors
                If FlippedH Then
                    .Item(0).Position = New Point(X + Width, Y + 0.5 * Height)
                    .Item(1).Position = New Point(X + 0.5 * Width, Y)
                Else
                    .Item(0).Position = New Point(X, Y + 0.5 * Height)
                    .Item(1).Position = New Point(X + 0.5 * Width, Y)
                End If
            End With

            With OutputConnectors
                If FlippedH Then
                    .Item(0).Position = New Point(X, Y + 0.5 * Height)
                    .Item(1).Position = New Point(X + 0.5 * Width, Y + Height)
                Else
                    .Item(0).Position = New Point(X + Width, Y + 0.5 * Height)
                    .Item(1).Position = New Point(X + 0.5 * Width, Y + Height)
                End If
            End With

        End Sub


        Public Overrides Sub Draw(ByVal g As Graphics)

            'Initializes drawing.
            MyBase.Draw(g)

            UpdateStatus(Me)

            Dim gContainer As Drawing2D.GraphicsContainer
            Dim myMatrix As Drawing2D.Matrix

            gContainer = g.BeginContainer()

            myMatrix = g.Transform()
            If m_Rotation <> 0 Then
                myMatrix.RotateAt(m_Rotation, New PointF(X, Y), _
                    Drawing2D.MatrixOrder.Prepend)
                g.Transform = myMatrix
            End If

            'Dim rect2 As New Rectangle(X - Width * 1 / 2, Y - Height * 1 / 2, Width * 2, Height * 2)
            'Dim rect2 As New Rectangle(X, Y, Width, Height)
            ' Create a path that consists of a single ellipse.
            'Dim path2 As New GraphicsPath()
            'path2.AddEllipse(rect2)

            '' Use the path to construct a brush.
            'Dim pthGrBrush2 As New PathGradientBrush(path2)
            '' Set the color at the center of the path to blue.
            'If Me.Calculated Then
            '    pthGrBrush2.CenterColor = Color.SteelBlue
            'Else
            '    pthGrBrush2.CenterColor = Color.Red
            'End If
            '' Set the color along the entire boundary 
            '' of the path to aqua.
            'Dim colors2 As Color() = {Color.Transparent}
            'pthGrBrush2.SurroundColors = colors2
            'pthGrBrush2.SetSigmaBellShape(1)
            'g.FillEllipse(pthGrBrush2, rect2)

            Dim rect As New Rectangle(X, Y, Width, Height)

            ' Create a path that consists of a single ellipse.
            Dim path As New GraphicsPath()
            path.AddEllipse(rect)
            ' Use the path to construct a brush.
            Dim pthGrBrush As New PathGradientBrush(path)
            ' Set the color at the center of the path to blue.
            pthGrBrush.CenterColor = Me.GradientColor2

            ' Set the color along the entire boundary 
            ' of the path to aqua.
            Dim colors As Color() = {Me.GradientColor1}
            pthGrBrush.SurroundColors = colors
            pthGrBrush.SetSigmaBellShape(1)

            Dim myPen As New Pen(Me.LineColor, Me.LineWidth)
            g.SmoothingMode = SmoothingMode.AntiAlias
            g.DrawEllipse(myPen, rect)
            If Me.Fill Then
                If Me.GradientMode = False Then
                    g.FillEllipse(New SolidBrush(Me.FillColor), rect)
                Else
                    g.FillEllipse(pthGrBrush, rect)
                End If
            End If
            g.DrawLine(myPen, CInt(X), CInt(Y + Height), CInt(X + (2 / 8) * Width), CInt(Y + (3 / 8) * Height))
            g.DrawLine(myPen, CInt(X + (2 / 8) * Width), CInt(Y + (3 / 8) * Height), CInt(X + (6 / 8) * Width), CInt(Y + (5 / 8) * Height))
            g.DrawLine(myPen, CInt(X + (6 / 8) * Width), CInt(Y + (5 / 8) * Height), CInt(X + Width), CInt(Y))

            g.TextRenderingHint = Text.TextRenderingHint.SystemDefault

            Dim strdist As SizeF = g.MeasureString(Me.Tag, New Font("Arial", 10, FontStyle.Bold, GraphicsUnit.Pixel, 0, False), New PointF(0, 0), New StringFormat(StringFormatFlags.NoClip, 0))
            Dim strx As Single = (Me.Width - strdist.Width) / 2
            'g.FillRectangle(Brushes.White, X + strx, Y + CSng(Height + 5), strdist.Width, strdist.Height)
            g.DrawString(Me.Tag, New Font("Arial", 10, FontStyle.Bold, GraphicsUnit.Pixel, 0, False), New SolidBrush(Me.LineColor), X + strx, Y + Height + 5)

            g.EndContainer(gContainer)

        End Sub

    End Class

    <Serializable()> Public Class ShorcutColumnGraphic

        Inherits ShapeGraphic

#Region "Constructors"
        Public Sub New()
            Me.TipoObjeto = GraphicObjects.TipoObjeto.ShortcutColumn
            CreateConnectors(2, 2)
            Me.InputConnectors(1).Type = ConType.ConEn
            Me.Description = "ShortcutColumn"
            Me.m_splitpoint = True
        End Sub

        Public Sub New(ByVal graphicPosition As Point)
            Me.New()
            Me.SetPosition(graphicPosition)
        End Sub

        Public Sub New(ByVal posX As Integer, ByVal posY As Integer)
            Me.New(New Point(posX, posY))
        End Sub

        Public Sub New(ByVal graphicPosition As Point, ByVal graphicSize As Size)
            Me.New(graphicPosition)
            Me.SetSize(graphicSize)
            Me.AutoSize = False
        End Sub

        Public Sub New(ByVal posX As Integer, ByVal posY As Integer, ByVal graphicSize As Size)
            Me.New(New Point(posX, posY), graphicSize)
        End Sub

        Public Sub New(ByVal posX As Integer, ByVal posY As Integer, ByVal width As Integer, ByVal height As Integer)
            Me.New(New Point(posX, posY), New Size(width, height))
        End Sub

        Public Sub New(ByVal graphicPosition As Point, ByVal Rotation As Single)
            Me.New()
            Me.SetPosition(graphicPosition)
            Me.Rotation = Rotation
        End Sub

        Public Sub New(ByVal posX As Integer, ByVal posY As Integer, ByVal Rotation As Single)
            Me.New(New Point(posX, posY), Rotation)
        End Sub

        Public Sub New(ByVal graphicPosition As Point, ByVal graphicSize As Size, ByVal Rotation As Single)
            Me.New(graphicPosition, Rotation)
            Me.SetSize(graphicSize)
            Me.AutoSize = False
        End Sub

        Public Sub New(ByVal posX As Integer, ByVal posY As Integer, ByVal graphicSize As Size, ByVal Rotation As Single)
            Me.New(New Point(posX, posY), graphicSize, Rotation)
        End Sub

        Public Sub New(ByVal posX As Integer, ByVal posY As Integer, ByVal width As Integer, _
                               ByVal height As Integer, ByVal Rotation As Single)
            Me.New(New Point(posX, posY), New Size(width, height), Rotation)
        End Sub

#End Region

        Public Overrides Sub PositionConnectors()

            MyBase.PositionConnectors()

            With InputConnectors
                If FlippedH Then
                    .Item(0).Position = New Point(X + Width, Y + 0.5 * Height)
                    .Item(1).Position = New Point(X, Y + 0.825 * Height)
                Else
                    .Item(0).Position = New Point(X, Y + 0.5 * Height)
                    .Item(1).Position = New Point(X + Width, Y + 0.825 * Height)
                End If
            End With

            With OutputConnectors
                If FlippedH Then
                    If Me.Shape = 0 Then
                        .Item(0).Position = New Point(X, Y + 0.3 * Height)
                    Else
                        .Item(0).Position = New Point(X, Y + 0.02 * Height)
                    End If
                    .Item(1).Position = New Point(X, Y + 0.98 * Height)
                Else
                    If Me.Shape = 0 Then
                        .Item(0).Position = New Point(X + Width, Y + 0.3 * Height)
                    Else
                        .Item(0).Position = New Point(X + Width, Y + 0.02 * Height)
                    End If
                    .Item(1).Position = New Point(X + Width, Y + 0.98 * Height)
                End If
            End With

            With Me.EnergyConnector
                If FlippedH Then
                    .Position = New Point(X, Y + 0.175 * Height)
                Else
                    .Position = New Point(X + Width, Y + 0.175 * Height)
                End If
            End With

        End Sub

        Public Overrides Sub Draw(ByVal g As System.Drawing.Graphics)

            MyBase.Draw(g)

            UpdateStatus(Me)

            Dim gContainer As System.Drawing.Drawing2D.GraphicsContainer
            Dim myMatrix As Drawing2D.Matrix
            gContainer = g.BeginContainer()
            myMatrix = g.Transform()
            If m_Rotation <> 0 Then
                myMatrix.RotateAt(m_Rotation, New PointF(X, Y), Drawing.Drawing2D.MatrixOrder.Append)
                g.Transform = myMatrix
            End If

            Dim myPen As New Pen(Me.LineColor, Me.LineWidth)
            Dim myPen1 As New Pen(Me.LineColor, Me.LineWidth)
            With myPen1
                .EndCap = LineCap.ArrowAnchor
            End With
            Dim myPen2 As New Pen(Color.White, 0)

            Dim rect As New Rectangle(X, Y, Width, Height)

            g.SmoothingMode = SmoothingMode.AntiAlias
            'g.DrawRectangle(myPen2, rect)

            Dim lgb1 As LinearGradientBrush
            lgb1 = New LinearGradientBrush(rect, Me.GradientColor1, Me.GradientColor2, LinearGradientMode.Horizontal)
            lgb1.SetBlendTriangularShape(0.5)
            'lgb1.CenterColor = Me.GradientColor1

            Dim path As New GraphicsPath()
            path.AddEllipse(rect)
            Dim pthGrBrush As New PathGradientBrush(path)
            pthGrBrush.CenterColor = Me.GradientColor2
            Dim colors As Color() = {Me.GradientColor1}
            pthGrBrush.SurroundColors = colors
            pthGrBrush.SetSigmaBellShape(0.5)

            If Me.Fill Then
                If Me.GradientMode = False Then
                    If Me.FlippedH = True Then
                        Me.DrawRoundRect(g, myPen, X + (1 - 0.1 - 0.2) * Width, Y + 0.1 * Height, 0.2 * 1.25 * Width, 0.8 * Height, 20, New SolidBrush(Me.FillColor))
                        g.FillEllipse(New SolidBrush(Me.FillColor), CSng(X + (0.475 - 0.15) * Width), CSng(Y + 0.1 * Height), CSng(0.15 * 1.25 * Width), CSng(0.15 * Height))
                        g.FillEllipse(New SolidBrush(Me.FillColor), CSng(X + (0.475 - 0.15) * Width), CSng(Y + 0.75 * Height), CSng(0.15 * 1.25 * Width), CSng(0.15 * Height))
                    Else
                        Me.DrawRoundRect(g, myPen, X + (0.05) * 1.25 * Width, Y + 0.1 * Height, 0.2 * 1.25 * Width, 0.8 * Height, 20, New SolidBrush(Me.FillColor))
                        g.FillEllipse(New SolidBrush(Me.FillColor), CSng(X + 0.525 * 1.25 * Width), CSng(Y + 0.1 * Height), CSng(0.15 * 1.25 * Width), CSng(0.15 * Height))
                        g.FillEllipse(New SolidBrush(Me.FillColor), CSng(X + 0.525 * 1.25 * Width), CSng(Y + 0.75 * Height), CSng(0.15 * 1.25 * Width), CSng(0.15 * Height))
                    End If
                Else
                    If Me.FlippedH = True Then
                        Me.DrawRoundRect(g, myPen, X + (1 - 0.1 - 0.2) * Width, Y + 0.1 * Height, 0.2 * 1.25 * Width, 0.8 * Height, 20, lgb1)
                        g.FillEllipse(pthGrBrush, CSng(X + (0.475 - 0.15) * Width), CSng(Y + 0.1 * Height), CSng(0.15 * 1.25 * Width), CSng(0.15 * Height))
                        g.FillEllipse(pthGrBrush, CSng(X + (0.475 - 0.15) * Width), CSng(Y + 0.75 * Height), CSng(0.15 * 1.25 * Width), CSng(0.15 * Height))
                    Else
                        Me.DrawRoundRect(g, myPen, X + 0.05 * 1.25 * Width, Y + 0.1 * Height, 0.2 * 1.25 * Width, 0.8 * Height, 20, lgb1)
                        g.FillEllipse(pthGrBrush, CSng(X + 0.525 * 1.25 * Width), CSng(Y + 0.1 * Height), CSng(0.15 * 1.25 * Width), CSng(0.15 * Height))
                        g.FillEllipse(pthGrBrush, CSng(X + 0.525 * 1.25 * Width), CSng(Y + 0.75 * Height), CSng(0.15 * 1.25 * Width), CSng(0.15 * Height))
                    End If
                End If
            End If

            If Me.FlippedH = True Then

                Me.DrawRoundRect(g, myPen, X + (1 - 0.1 - 0.2) * Width, Y + 0.1 * Height, 0.2 * 1.25 * Width, 0.8 * Height, 20, Brushes.Transparent)
                g.DrawLines(myPen, New PointF() {New PointF(X + 0.825 * Width, Y + 0.1 * Height), New PointF(X + 0.825 * Width, Y + 0.02 * Height), New PointF(X + 0.4 * Width, Y + 0.02 * Height), New PointF(X + 0.4 * Width, Y + 0.1 * Height)})
                g.DrawLines(myPen, New PointF() {New PointF(X + 0.825 * Width, Y + 0.9 * Height), New PointF(X + 0.825 * Width, Y + 0.98 * Height), New PointF(X + 0.4 * Width, Y + 0.98 * Height), New PointF(X + 0.4 * Width, Y + 0.9 * Height)})

                g.DrawEllipse(myPen, CSng(X + (1 - 0.525 - 0.15) * Width), CSng(Y + 0.1 * Height), CSng(0.15 * 1.25 * Width), CSng(0.15 * Height))
                g.DrawEllipse(myPen, CSng(X + (1 - 0.525 - 0.15) * Width), CSng(Y + 0.75 * Height), CSng(0.15 * 1.25 * Width), CSng(0.15 * Height))

                g.DrawLines(myPen, New PointF() {New PointF(X + 0.4 * Width, Y + 0.25 * Height), New PointF(X + 0.4 * Width, Y + 0.3 * Height)})
                g.DrawLines(myPen, New PointF() {New PointF(X + 0.4 * Width, Y + 0.75 * Height), New PointF(X + 0.4 * Width, Y + 0.7 * Height)})

                g.DrawLine(myPen1, CSng(X + 0.4 * Width), CSng(Y + 0.3 * Height), CSng(X + 0.7 * Width), CSng(Y + 0.3 * Height))
                g.DrawLine(myPen1, CSng(X + 0.4 * Width), CSng(Y + 0.7 * Height), CSng(X + 0.7 * Width), CSng(Y + 0.7 * Height))

                g.DrawLine(myPen1, CSng(X + 0.4 * Width), CSng(Y + 0.98 * Height), CSng(X), CSng(Y + 0.98 * Height))

                If Me.Shape = 1 Then
                    g.DrawLine(myPen1, CSng(X + 0.4 * Width), CSng(Y + 0.02 * Height), CSng(X), CSng(Y + 0.02 * Height))
                    g.DrawLine(myPen1, CSng(X + 0.4 * Width), CSng(Y + 0.3 * Height), CSng(X), CSng(Y + 0.3 * Height))
                Else
                    g.DrawLine(myPen1, CSng(X + 0.4 * Width), CSng(Y + 0.3 * Height), CSng(X), CSng(Y + 0.3 * Height))
                End If

                g.DrawLines(myPen, New PointF() {New PointF(X + 0.6 * Width, Y + 0.175 * Height), New PointF(X + 0.45 * Width, Y + 0.175 * Height), New PointF(X + 0.425 * Width, Y + 0.125 * Height), New PointF(X + 0.375 * Width, Y + 0.225 * Height), New PointF(X + 0.35 * Width, Y + 0.175 * Height)})
                g.DrawLine(myPen1, CSng(X + 0.35 * Width), CSng(Y + 0.175 * Height), CSng(X), CSng(Y + 0.175 * Height))

                g.DrawLines(myPen, New PointF() {New PointF(X + 0.5 * Width, Y + 0.825 * Height), New PointF(X + 0.45 * Width, Y + 0.825 * Height), New PointF(X + 0.425 * Width, Y + 0.875 * Height), New PointF(X + 0.375 * Width, Y + 0.775 * Height), New PointF(X + 0.35 * Width, Y + 0.825 * Height), New PointF(X, Y + 0.825 * Height)})
                g.DrawLine(myPen1, CSng(X + 0.5 * Width), CSng(Y + 0.825 * Height), CSng(X + 0.6 * Width), CSng(Y + 0.825 * Height))

                g.DrawLine(myPen1, CSng(X + Width), CSng(Y + 0.5 * Height), CSng(X + 0.95 * Width), CSng(Y + 0.5 * Height))

                g.DrawLine(myPen, CSng(X + 0.7 * Width), CSng(Y + 0.2 * Height), CSng(X + 0.95 * Width), CSng(Y + 0.2 * Height))
                g.DrawLine(myPen, CSng(X + 0.7 * Width), CSng(Y + 0.3 * Height), CSng(X + 0.95 * Width), CSng(Y + 0.3 * Height))
                g.DrawLine(myPen, CSng(X + 0.7 * Width), CSng(Y + 0.4 * Height), CSng(X + 0.95 * Width), CSng(Y + 0.4 * Height))
                g.DrawLine(myPen, CSng(X + 0.7 * Width), CSng(Y + 0.5 * Height), CSng(X + 0.95 * Width), CSng(Y + 0.5 * Height))
                g.DrawLine(myPen, CSng(X + 0.7 * Width), CSng(Y + 0.6 * Height), CSng(X + 0.95 * Width), CSng(Y + 0.6 * Height))
                g.DrawLine(myPen, CSng(X + 0.7 * Width), CSng(Y + 0.7 * Height), CSng(X + 0.95 * Width), CSng(Y + 0.7 * Height))
                g.DrawLine(myPen, CSng(X + 0.7 * Width), CSng(Y + 0.8 * Height), CSng(X + 0.95 * Width), CSng(Y + 0.8 * Height))

            Else

                Me.DrawRoundRect(g, myPen, X + 0.05 * 1.25 * Width, Y + 0.1 * Height, 0.2 * 1.25 * Width, 0.8 * Height, 20, Brushes.Transparent)
                g.DrawLines(myPen, New PointF() {New PointF(X + 0.175 * Width, Y + 0.1 * Height), New PointF(X + 0.175 * Width, Y + 0.02 * Height), New PointF(X + 0.6 * 1.25 * Width, Y + 0.02 * Height), New PointF(X + 0.6 * 1.25 * Width, Y + 0.1 * Height)})
                g.DrawLines(myPen, New PointF() {New PointF(X + 0.175 * Width, Y + 0.9 * Height), New PointF(X + 0.175 * Width, Y + 0.98 * Height), New PointF(X + 0.6 * 1.25 * Width, Y + 0.98 * Height), New PointF(X + 0.6 * 1.25 * Width, Y + 0.9 * Height)})

                g.DrawEllipse(myPen, CSng(X + 0.525 * 1.25 * Width), CSng(Y + 0.1 * Height), CSng(0.15 * 1.25 * Width), CSng(0.15 * Height))
                g.DrawEllipse(myPen, CSng(X + 0.525 * 1.25 * Width), CSng(Y + 0.75 * Height), CSng(0.15 * 1.25 * Width), CSng(0.15 * Height))

                g.DrawLines(myPen, New PointF() {New PointF(X + 0.6 * 1.25 * Width, Y + 0.25 * Height), New PointF(X + 0.6 * 1.25 * Width, Y + 0.3 * Height)})
                g.DrawLines(myPen, New PointF() {New PointF(X + 0.6 * 1.25 * Width, Y + 0.75 * Height), New PointF(X + 0.6 * 1.25 * Width, Y + 0.7 * Height)})

                g.DrawLine(myPen1, CSng(X + 0.6 * 1.25 * Width), CSng(Y + 0.3 * Height), CSng(X + 0.25 * 1.25 * Width), CSng(Y + 0.3 * Height))
                g.DrawLine(myPen1, CSng(X + 0.6 * 1.25 * Width), CSng(Y + 0.7 * Height), CSng(X + 0.25 * 1.25 * Width), CSng(Y + 0.7 * Height))

                g.DrawLine(myPen1, CSng(X + 0.6 * 1.25 * Width), CSng(Y + 0.98 * Height), CSng(X + Width), CSng(Y + 0.98 * Height))

                If Me.Shape = 1 Then
                    g.DrawLine(myPen1, CSng(X + 0.6 * 1.25 * Width), CSng(Y + 0.02 * Height), CSng(X + Width), CSng(Y + 0.02 * Height))
                    g.DrawLine(myPen1, CSng(X + 0.6 * 1.25 * Width), CSng(Y + 0.3 * Height), CSng(X + Width), CSng(Y + 0.3 * Height))
                Else
                    g.DrawLine(myPen1, CSng(X + 0.6 * 1.25 * Width), CSng(Y + 0.3 * Height), CSng(X + Width), CSng(Y + 0.3 * Height))
                End If

                g.DrawLines(myPen, New PointF() {New PointF(X + 0.4 * 1.25 * Width, Y + 0.175 * Height), New PointF(X + 0.55 * 1.25 * Width, Y + 0.175 * Height), New PointF(X + 0.575 * 1.25 * Width, Y + 0.125 * Height), New PointF(X + 0.625 * 1.25 * Width, Y + 0.225 * Height), New PointF(X + 0.65 * 1.25 * Width, Y + 0.175 * Height)})
                g.DrawLine(myPen1, CSng(X + 0.65 * 1.25 * Width), CSng(Y + 0.175 * Height), CSng(X + Width), CSng(Y + 0.175 * Height))

                g.DrawLines(myPen, New PointF() {New PointF(X + 0.5 * 1.25 * Width, Y + 0.825 * Height), New PointF(X + 0.55 * 1.25 * Width, Y + 0.825 * Height), New PointF(X + 0.575 * 1.25 * Width, Y + 0.875 * Height), New PointF(X + 0.625 * 1.25 * Width, Y + 0.775 * Height), New PointF(X + 0.65 * 1.25 * Width, Y + 0.825 * Height), New PointF(X + Width, Y + 0.825 * Height)})
                g.DrawLine(myPen1, CSng(X + 0.5 * 1.25 * Width), CSng(Y + 0.825 * Height), CSng(X + 0.4 * 1.25 * Width), CSng(Y + 0.825 * Height))

                g.DrawLine(myPen1, CSng(X), CSng(Y + 0.5 * Height), CSng(X + 0.05 * 1.25 * Width), CSng(Y + 0.5 * Height))

                g.DrawLine(myPen, CSng(X + 0.05 * 1.25 * Width), CSng(Y + 0.2 * Height), CSng(X + 0.31 * Width), CSng(Y + 0.2 * Height))
                g.DrawLine(myPen, CSng(X + 0.05 * 1.25 * Width), CSng(Y + 0.3 * Height), CSng(X + 0.31 * Width), CSng(Y + 0.3 * Height))
                g.DrawLine(myPen, CSng(X + 0.05 * 1.25 * Width), CSng(Y + 0.4 * Height), CSng(X + 0.31 * Width), CSng(Y + 0.4 * Height))
                g.DrawLine(myPen, CSng(X + 0.05 * 1.25 * Width), CSng(Y + 0.5 * Height), CSng(X + 0.31 * Width), CSng(Y + 0.5 * Height))
                g.DrawLine(myPen, CSng(X + 0.05 * 1.25 * Width), CSng(Y + 0.6 * Height), CSng(X + 0.31 * Width), CSng(Y + 0.6 * Height))
                g.DrawLine(myPen, CSng(X + 0.05 * 1.25 * Width), CSng(Y + 0.7 * Height), CSng(X + 0.31 * Width), CSng(Y + 0.7 * Height))
                g.DrawLine(myPen, CSng(X + 0.05 * 1.25 * Width), CSng(Y + 0.8 * Height), CSng(X + 0.31 * Width), CSng(Y + 0.8 * Height))

            End If
            'g.DrawRectangle(myPen, rect2)
            'g.DrawRectangle(myPen, rect3)
            'g.DrawRectangle(myPen, rect4)

            Dim strdist As SizeF = g.MeasureString(Me.Tag, New Font("Arial", 10, FontStyle.Bold, GraphicsUnit.Pixel, 0, False), New PointF(0, 0), New StringFormat(StringFormatFlags.NoClip, 0))
            Dim strx As Single = (Me.Width - strdist.Width) / 2
            'g.FillRectangle(Brushes.White, X + strx, Y + CSng(Height + 5), strdist.Width, strdist.Height)
            g.DrawString(Me.Tag, New Font("Arial", 10, FontStyle.Bold, GraphicsUnit.Pixel, 0, False), New SolidBrush(Me.LineColor), X + strx, Y + Height + 5)

            g.EndContainer(gContainer)

        End Sub

        Public Sub DrawRoundRect(ByVal g As Graphics, ByVal p As Pen, ByVal x As Integer, ByVal y As Integer, ByVal width As Integer, ByVal height As Integer, ByVal radius As Integer, ByVal myBrush As Brush)

            Dim gp As Drawing2D.GraphicsPath = New Drawing2D.GraphicsPath

            gp.AddLine(x + radius, y, x + width - radius, y)
            gp.AddArc(x + width - radius, y, radius, radius, 270, 90)
            gp.AddLine(x + width, y + radius, x + width, y + height - radius)
            gp.AddArc(x + width - radius, y + height - radius, radius, radius, 0, 90)
            gp.AddLine(x + width - radius, y + height, x + radius, y + height)
            gp.AddArc(x, y + height - radius, radius, radius, 90, 90)
            gp.AddLine(x, y + height - radius, x, y + radius)
            gp.AddArc(x, y, radius, radius, 180, 90)

            gp.CloseFigure()

            g.DrawPath(p, gp)
            g.FillPath(myBrush, gp)

            gp.Dispose()

        End Sub

    End Class

    <Serializable()> Public Class DistillationColumnGraphic

        Inherits ShapeGraphic

#Region "Constructors"
        Public Sub New()
            Me.TipoObjeto = GraphicObjects.TipoObjeto.DistillationColumn
            Me.Description = "DistillationColumn"
            Me.m_mixpoint = True
            Me.m_splitpoint = True
            'CreateConnectors(2, 2)
            'Me.InputConnectors(1).Type = ConType.ConEn
        End Sub

        Public Sub New(ByVal graphicPosition As Point)
            Me.New()
            Me.SetPosition(graphicPosition)
        End Sub

        Public Sub New(ByVal posX As Integer, ByVal posY As Integer)
            Me.New(New Point(posX, posY))
        End Sub

        Public Sub New(ByVal graphicPosition As Point, ByVal graphicSize As Size)
            Me.New(graphicPosition)
            Me.SetSize(graphicSize)
            Me.AutoSize = False
        End Sub

        Public Sub New(ByVal posX As Integer, ByVal posY As Integer, ByVal graphicSize As Size)
            Me.New(New Point(posX, posY), graphicSize)
        End Sub

        Public Sub New(ByVal posX As Integer, ByVal posY As Integer, ByVal width As Integer, ByVal height As Integer)
            Me.New(New Point(posX, posY), New Size(width, height))
        End Sub

        Public Sub New(ByVal graphicPosition As Point, ByVal Rotation As Single)
            Me.New()
            Me.SetPosition(graphicPosition)
            Me.Rotation = Rotation
        End Sub

        Public Sub New(ByVal posX As Integer, ByVal posY As Integer, ByVal Rotation As Single)
            Me.New(New Point(posX, posY), Rotation)
        End Sub

        Public Sub New(ByVal graphicPosition As Point, ByVal graphicSize As Size, ByVal Rotation As Single)
            Me.New(graphicPosition, Rotation)
            Me.SetSize(graphicSize)
            Me.AutoSize = False
        End Sub

        Public Sub New(ByVal posX As Integer, ByVal posY As Integer, ByVal graphicSize As Size, ByVal Rotation As Single)
            Me.New(New Point(posX, posY), graphicSize, Rotation)
        End Sub

        Public Sub New(ByVal posX As Integer, ByVal posY As Integer, ByVal width As Integer, _
                               ByVal height As Integer, ByVal Rotation As Single)
            Me.New(New Point(posX, posY), New Size(width, height), Rotation)
        End Sub

#End Region

        Public Overrides Sub PositionConnectors()

            MyBase.PositionConnectors()

            If Not Me.AdditionalInfo Is Nothing Then

                Dim obj1(Me.InputConnectors.Count), obj2(Me.InputConnectors.Count) As Double
                Dim obj3(Me.OutputConnectors.Count), obj4(Me.OutputConnectors.Count) As Double
                obj1 = Me.AdditionalInfo(0)
                obj2 = Me.AdditionalInfo(1)
                obj3 = Me.AdditionalInfo(2)
                obj4 = Me.AdditionalInfo(3)

                Try
                    Dim i As Integer = 0
                    For Each ic As ConnectionPoint In Me.InputConnectors
                        ic.Position = New Point(Me.X + obj1(i), Me.Y + obj2(i))
                        i = i + 1
                    Next
                    i = 0
                    For Each oc As ConnectionPoint In Me.OutputConnectors
                        oc.Position = New Point(Me.X + obj3(i), Me.Y + obj4(i))
                        i = i + 1
                    Next
                Catch ex As Exception

                End Try

            End If

        End Sub

        Public Overrides Sub Draw(ByVal g As System.Drawing.Graphics)

            MyBase.Draw(g)

            UpdateStatus(Me)

            Dim gContainer As System.Drawing.Drawing2D.GraphicsContainer
            Dim myMatrix As Drawing2D.Matrix
            gContainer = g.BeginContainer()
            myMatrix = g.Transform()
            If m_Rotation <> 0 Then
                myMatrix.RotateAt(m_Rotation, New PointF(X, Y), Drawing.Drawing2D.MatrixOrder.Append)
                g.Transform = myMatrix
            End If

            Dim myPen As New Pen(Me.LineColor, Me.LineWidth)
            Dim myPen1 As New Pen(Me.LineColor, Me.LineWidth)
            With myPen1
                .EndCap = LineCap.ArrowAnchor
            End With
            Dim myPen2 As New Pen(Color.White, 0)
            Dim myPen3 As New Pen(Me.LineColor, Me.LineWidth)
            With myPen3
                .DashStyle = DashStyle.Dot
            End With

            Dim rect As New Rectangle(X, Y, Width, Height)

            g.SmoothingMode = SmoothingMode.AntiAlias
            'g.DrawRectangle(myPen2, rect)

            Dim lgb1 As LinearGradientBrush
            lgb1 = New LinearGradientBrush(rect, Me.GradientColor1, Me.GradientColor2, LinearGradientMode.Horizontal)
            lgb1.SetBlendTriangularShape(0.5)
            'lgb1.CenterColor = Me.GradientColor1

            Dim path As New GraphicsPath()
            path.AddEllipse(rect)
            Dim pthGrBrush As New PathGradientBrush(path)
            pthGrBrush.CenterColor = Me.GradientColor2
            Dim colors As Color() = {Me.GradientColor1}
            pthGrBrush.SurroundColors = colors
            pthGrBrush.SetSigmaBellShape(0.5)

            If Me.Fill Then
                If Me.GradientMode = False Then
                    If Me.FlippedH = True Then
                        Me.DrawRoundRect(g, myPen, X + (1 - 0.1 - 0.2) * Width, Y + 0.2 * Height, 0.2 * 1.25 * Width, 0.7 * Height, 20, New SolidBrush(Me.FillColor))
                        Me.DrawRoundRect(g, myPen, X + 0.1 * Width, Y + 0.1 * Height, 0.15 * 1.25 * Width, 0.15 * Height, 20, lgb1)
                        g.FillEllipse(New SolidBrush(Me.FillColor), CSng(X + (0.475 - 0.15) * Width), CSng(Y + 0.0 * Height), CSng(0.15 * 1.25 * Width), CSng(0.15 * Height))
                        g.FillEllipse(New SolidBrush(Me.FillColor), CSng(X + (0.475 - 0.15) * Width), CSng(Y + 0.75 * Height), CSng(0.15 * 1.25 * Width), CSng(0.15 * Height))
                    Else
                        Me.DrawRoundRect(g, myPen, X + (0.05) * 1.25 * Width, Y + 0.2 * Height, 0.2 * 1.25 * Width, 0.7 * Height, 20, New SolidBrush(Me.FillColor))
                        g.FillEllipse(New SolidBrush(Me.FillColor), CSng(X + 0.525 * 1.25 * Width), CSng(Y), CSng(0.15 * 1.25 * Width), CSng(0.15 * Height))
                        g.FillEllipse(New SolidBrush(Me.FillColor), CSng(X + 0.525 * 1.25 * Width), CSng(Y + 0.75 * Height), CSng(0.15 * 1.25 * Width), CSng(0.15 * Height))
                    End If
                Else
                    If Me.FlippedH = True Then
                        Me.DrawRoundRect(g, myPen, X + (1 - 0.1 - 0.2) * Width, Y + 0.2 * Height, 0.2 * 1.25 * Width, 0.7 * Height, 20, lgb1)
                        Me.DrawRoundRect(g, myPen, X + 0.1 * Width, Y + 0.1 * Height, 0.15 * 1.25 * Width, 0.15 * Height, 20, lgb1)
                        g.FillEllipse(pthGrBrush, CSng(X + (0.475 - 0.15) * Width), CSng(Y + 0.0 * Height), CSng(0.15 * 1.25 * Width), CSng(0.15 * Height))
                        g.FillEllipse(pthGrBrush, CSng(X + (0.475 - 0.15) * Width), CSng(Y + 0.75 * Height), CSng(0.15 * 1.25 * Width), CSng(0.15 * Height))
                    Else
                        Me.DrawRoundRect(g, myPen, X + 0.05 * 1.25 * Width, Y + 0.2 * Height, 0.2 * 1.25 * Width, 0.7 * Height, 20, lgb1)
                        Me.DrawRoundRect(g, myPen, X + 0.525 * 1.25 * Width, Y + 0.1 * Height, 0.15 * 1.25 * Width, 0.15 * Height, 20, lgb1)
                        g.FillEllipse(pthGrBrush, CSng(X + 0.3 * 1.25 * Width), CSng(Y), CSng(0.15 * 1.25 * Width), CSng(0.15 * Height))
                        g.FillEllipse(pthGrBrush, CSng(X + 0.525 * 1.25 * Width), CSng(Y + 0.75 * Height), CSng(0.15 * 1.25 * Width), CSng(0.15 * Height))
                    End If
                End If
            End If

            If Me.FlippedH = True Then

                'Me.DrawRoundRect(g, myPen, X + (1 - 0.1 - 0.2) * Width, Y + 0.2 * Height, 0.2 * 1.25 * Width, 0.7 * Height, 20, Brushes.Transparent)
                'g.DrawLines(myPen, New PointF() {New PointF(X + 0.825 * Width, Y + 0.1 * Height), New PointF(X + 0.825 * Width, Y + 0.02 * Height), New PointF(X + 0.4 * Width, Y + 0.02 * Height), New PointF(X + 0.4 * Width, Y + 0.1 * Height)})
                'g.DrawLines(myPen, New PointF() {New PointF(X + 0.825 * Width, Y + 0.9 * Height), New PointF(X + 0.825 * Width, Y + 0.98 * Height), New PointF(X + 0.4 * Width, Y + 0.98 * Height), New PointF(X + 0.4 * Width, Y + 0.9 * Height)})

                Me.DrawRoundRect(g, myPen, X + 0.7 * Width, Y + 0.2 * Height, 0.2 * 1.25 * Width, 0.7 * Height, 20, Brushes.Transparent)
                Me.DrawRoundRect(g, myPen, X + 0.1 * Width, Y + 0.1 * Height, 0.15 * 1.25 * Width, 0.15 * Height, 20, Brushes.Transparent)
                g.DrawLines(myPen, New PointF() {New PointF(X + 0.175 * Width, Y + 0.1 * Height), New PointF(X + 0.175 * Width, Y + 0.02 * Height), New PointF(X + 0.32 * 1.25 * Width, Y + 0.02 * Height)})
                g.DrawLines(myPen, New PointF() {New PointF(X + 0.38 * 1.25 * Width, Y + 0.02 * Height), New PointF(X + 0.8 * Width, Y + 0.02 * Height), New PointF(X + 0.8 * Width, Y + 0.2 * Height)})
                g.DrawLines(myPen, New PointF() {New PointF(X + 0.4 * Width, Y + 0.9 * Height), New PointF(X + 0.4 * Width, Y + 0.98 * Height), New PointF(X + 0.8 * Width, Y + 0.98 * Height), New PointF(X + 0.8 * Width, Y + 0.9 * Height)})

                g.DrawEllipse(myPen, CSng(X + (1 - 0.525 - 0.15) * Width), CSng(Y + 0.0 * Height), CSng(0.15 * 1.25 * Width), CSng(0.15 * Height))
                g.DrawEllipse(myPen, CSng(X + (1 - 0.525 - 0.15) * Width), CSng(Y + 0.75 * Height), CSng(0.15 * 1.25 * Width), CSng(0.15 * Height))

                g.DrawLines(myPen, New PointF() {New PointF(X + 0.175 * Width, Y + 0.25 * Height), New PointF(X + 0.175 * Width, Y + 0.3 * Height)})
                g.DrawLines(myPen, New PointF() {New PointF(X + 0.4 * Width, Y + 0.75 * Height), New PointF(X + 0.4 * Width, Y + 0.7 * Height)})

                g.DrawLine(myPen1, CSng(X + 0.4 * Width), CSng(Y + 0.3 * Height), CSng(X + 0.7 * Width), CSng(Y + 0.3 * Height))
                g.DrawLine(myPen1, CSng(X + 0.4 * Width), CSng(Y + 0.7 * Height), CSng(X + 0.7 * Width), CSng(Y + 0.7 * Height))

                g.DrawLine(myPen1, CSng(X + 0.6 * Width), CSng(Y + 0.98 * Height), CSng(X), CSng(Y + 0.98 * Height))

                If Me.Shape = 1 Then
                    g.DrawLine(myPen1, CSng(X + 0.4 * Width), CSng(Y + 0.02 * Height), CSng(X), CSng(Y + 0.02 * Height))
                    g.DrawLine(myPen1, CSng(X + 0.4 * Width), CSng(Y + 0.3 * Height), CSng(X), CSng(Y + 0.3 * Height))
                ElseIf Me.Shape = 0 Then
                    g.DrawLine(myPen1, CSng(X + 0.4 * Width), CSng(Y + 0.3 * Height), CSng(X), CSng(Y + 0.3 * Height))
                Else
                    g.DrawLine(myPen1, CSng(X + 0.4 * Width), CSng(Y + 0.02 * Height), CSng(X), CSng(Y + 0.02 * Height))
                End If

                'g.DrawLines(myPen, New PointF() {New PointF(X + 0.6 * Width, Y + 0.07 * Height), New PointF(X + 0.45 * Width, Y + 0.07 * Height), New PointF(X + 0.425 * Width, Y + 0.125 * Height), New PointF(X + 0.375 * Width, Y + 0.225 * Height), New PointF(X + 0.35 * Width, Y + 0.175 * Height)})
                'g.DrawLine(myPen1, CSng(X + 0.35 * Width), CSng(Y + 0.175 * Height), CSng(X), CSng(Y + 0.175 * Height))

                g.DrawLines(myPen, New PointF() {New PointF(X + (1 - 0.4) * Width, Y + 0.07 * Height), New PointF(X + (1 - 0.5) * Width, Y + 0.07 * Height), New PointF(X + (1 - 0.55) * Width, Y + 0.125 * Height), New PointF(X + (1 - 0.6) * Width, Y + 0.02 * Height), New PointF(X + (1 - 0.65) * Width, Y + 0.08 * Height)})
                g.DrawLine(myPen3, CSng(X + 0.275 * 1.25 * Width), CSng(Y + 0.08 * Height), CSng(X + 0.1 * Width), CSng(Y + 0.08 * Height))
                g.DrawLine(myPen1, CSng(X + 0.1 * 1.25 * Width), CSng(Y + 0.08 * Height), CSng(X), CSng(Y + 0.08 * Height))

                g.DrawLines(myPen, New PointF() {New PointF(X + 0.5 * Width, Y + 0.825 * Height), New PointF(X + 0.45 * Width, Y + 0.825 * Height), New PointF(X + 0.425 * Width, Y + 0.875 * Height), New PointF(X + 0.375 * Width, Y + 0.775 * Height), New PointF(X + 0.35 * Width, Y + 0.825 * Height), New PointF(X, Y + 0.825 * Height)})
                g.DrawLine(myPen1, CSng(X + 0.5 * Width), CSng(Y + 0.825 * Height), CSng(X + 0.6 * Width), CSng(Y + 0.825 * Height))

                g.DrawLine(myPen1, CSng(X + Width), CSng(Y + 0.5 * Height), CSng(X + 0.95 * Width), CSng(Y + 0.5 * Height))

                'g.DrawLine(myPen, CSng(X + 0.7 * Width), CSng(Y + 0.2 * Height), CSng(X + 0.95 * Width), CSng(Y + 0.2 * Height))
                g.DrawLine(myPen, CSng(X + 0.7 * Width), CSng(Y + 0.3 * Height), CSng(X + 0.95 * Width), CSng(Y + 0.3 * Height))
                g.DrawLine(myPen, CSng(X + 0.7 * Width), CSng(Y + 0.4 * Height), CSng(X + 0.95 * Width), CSng(Y + 0.4 * Height))
                g.DrawLine(myPen, CSng(X + 0.7 * Width), CSng(Y + 0.5 * Height), CSng(X + 0.95 * Width), CSng(Y + 0.5 * Height))
                g.DrawLine(myPen, CSng(X + 0.7 * Width), CSng(Y + 0.6 * Height), CSng(X + 0.95 * Width), CSng(Y + 0.6 * Height))
                g.DrawLine(myPen, CSng(X + 0.7 * Width), CSng(Y + 0.7 * Height), CSng(X + 0.95 * Width), CSng(Y + 0.7 * Height))
                g.DrawLine(myPen, CSng(X + 0.7 * Width), CSng(Y + 0.8 * Height), CSng(X + 0.95 * Width), CSng(Y + 0.8 * Height))

            Else

                Me.DrawRoundRect(g, myPen, X + 0.05 * 1.25 * Width, Y + 0.2 * Height, 0.2 * 1.25 * Width, 0.7 * Height, 20, Brushes.Transparent)
                Me.DrawRoundRect(g, myPen, X + 0.525 * 1.25 * Width, Y + 0.1 * Height, 0.15 * 1.25 * Width, 0.15 * Height, 20, Brushes.Transparent)
                g.DrawLines(myPen, New PointF() {New PointF(X + 0.175 * Width, Y + 0.2 * Height), New PointF(X + 0.175 * Width, Y + 0.02 * Height), New PointF(X + 0.32 * 1.25 * Width, Y + 0.02 * Height)})
                g.DrawLines(myPen, New PointF() {New PointF(X + 0.43 * 1.25 * Width, Y + 0.02 * Height), New PointF(X + 0.6 * 1.25 * Width, Y + 0.02 * Height), New PointF(X + 0.6 * 1.25 * Width, Y + 0.1 * Height)})
                g.DrawLines(myPen, New PointF() {New PointF(X + 0.175 * Width, Y + 0.9 * Height), New PointF(X + 0.175 * Width, Y + 0.98 * Height), New PointF(X + 0.6 * 1.25 * Width, Y + 0.98 * Height), New PointF(X + 0.6 * 1.25 * Width, Y + 0.9 * Height)})

                g.DrawEllipse(myPen, CSng(X + 0.3 * 1.25 * Width), CSng(Y), CSng(0.15 * 1.25 * Width), CSng(0.15 * Height))
                g.DrawEllipse(myPen, CSng(X + 0.525 * 1.25 * Width), CSng(Y + 0.75 * Height), CSng(0.15 * 1.25 * Width), CSng(0.15 * Height))

                g.DrawLines(myPen, New PointF() {New PointF(X + 0.6 * 1.25 * Width, Y + 0.25 * Height), New PointF(X + 0.6 * 1.25 * Width, Y + 0.3 * Height)})
                g.DrawLines(myPen, New PointF() {New PointF(X + 0.6 * 1.25 * Width, Y + 0.75 * Height), New PointF(X + 0.6 * 1.25 * Width, Y + 0.7 * Height)})

                g.DrawLine(myPen1, CSng(X + 0.6 * 1.25 * Width), CSng(Y + 0.3 * Height), CSng(X + 0.25 * 1.25 * Width), CSng(Y + 0.3 * Height))
                g.DrawLine(myPen1, CSng(X + 0.6 * 1.25 * Width), CSng(Y + 0.7 * Height), CSng(X + 0.25 * 1.25 * Width), CSng(Y + 0.7 * Height))

                g.DrawLine(myPen1, CSng(X + 0.6 * 1.25 * Width), CSng(Y + 0.98 * Height), CSng(X + Width), CSng(Y + 0.98 * Height))

                If Me.Shape = 1 Then
                    g.DrawLine(myPen1, CSng(X + 0.6 * 1.25 * Width), CSng(Y + 0.02 * Height), CSng(X + Width), CSng(Y + 0.02 * Height))
                    g.DrawLine(myPen1, CSng(X + 0.6 * 1.25 * Width), CSng(Y + 0.3 * Height), CSng(X + Width), CSng(Y + 0.3 * Height))
                ElseIf Me.Shape = 0 Then
                    g.DrawLine(myPen1, CSng(X + 0.6 * 1.25 * Width), CSng(Y + 0.3 * Height), CSng(X + Width), CSng(Y + 0.3 * Height))
                Else
                    g.DrawLine(myPen1, CSng(X + 0.6 * 1.25 * Width), CSng(Y + 0.02 * Height), CSng(X + Width), CSng(Y + 0.02 * Height))
                End If

                g.DrawLines(myPen, New PointF() {New PointF(X + 0.2 * 1.25 * Width, Y + 0.07 * Height), New PointF(X + 0.3 * 1.25 * Width, Y + 0.07 * Height), New PointF(X + 0.35 * 1.25 * Width, Y + 0.125 * Height), New PointF(X + 0.4 * 1.25 * Width, Y + 0.02 * Height), New PointF(X + 0.45 * 1.25 * Width, Y + 0.08 * Height)})
                g.DrawLine(myPen3, CSng(X + 0.45 * 1.25 * Width), CSng(Y + 0.08 * Height), CSng(X + 0.65 * 1.25 * Width), CSng(Y + 0.08 * Height))
                g.DrawLine(myPen1, CSng(X + 0.65 * 1.25 * Width), CSng(Y + 0.08 * Height), CSng(X + Width), CSng(Y + 0.08 * Height))

                g.DrawLines(myPen, New PointF() {New PointF(X + 0.5 * 1.25 * Width, Y + 0.825 * Height), New PointF(X + 0.55 * 1.25 * Width, Y + 0.825 * Height), New PointF(X + 0.575 * 1.25 * Width, Y + 0.875 * Height), New PointF(X + 0.625 * 1.25 * Width, Y + 0.775 * Height), New PointF(X + 0.65 * 1.25 * Width, Y + 0.825 * Height), New PointF(X + Width, Y + 0.825 * Height)})
                g.DrawLine(myPen1, CSng(X + 0.5 * 1.25 * Width), CSng(Y + 0.825 * Height), CSng(X + 0.4 * 1.25 * Width), CSng(Y + 0.825 * Height))

                g.DrawLine(myPen1, CSng(X), CSng(Y + 0.5 * Height), CSng(X + 0.05 * 1.25 * Width), CSng(Y + 0.5 * Height))

                'g.DrawLine(myPen, CSng(X + 0.05 * 1.25 * Width), CSng(Y + 0.2 * Height), CSng(X + 0.31 * Width), CSng(Y + 0.2 * Height))
                g.DrawLine(myPen, CSng(X + 0.05 * 1.25 * Width), CSng(Y + 0.3 * Height), CSng(X + 0.31 * Width), CSng(Y + 0.3 * Height))
                g.DrawLine(myPen, CSng(X + 0.05 * 1.25 * Width), CSng(Y + 0.4 * Height), CSng(X + 0.31 * Width), CSng(Y + 0.4 * Height))
                g.DrawLine(myPen, CSng(X + 0.05 * 1.25 * Width), CSng(Y + 0.5 * Height), CSng(X + 0.31 * Width), CSng(Y + 0.5 * Height))
                g.DrawLine(myPen, CSng(X + 0.05 * 1.25 * Width), CSng(Y + 0.6 * Height), CSng(X + 0.31 * Width), CSng(Y + 0.6 * Height))
                g.DrawLine(myPen, CSng(X + 0.05 * 1.25 * Width), CSng(Y + 0.7 * Height), CSng(X + 0.31 * Width), CSng(Y + 0.7 * Height))
                g.DrawLine(myPen, CSng(X + 0.05 * 1.25 * Width), CSng(Y + 0.8 * Height), CSng(X + 0.31 * Width), CSng(Y + 0.8 * Height))

            End If

            Me.DrawRoundRect(g, myPen, Me.X, Me.Y, Me.Width, Me.Height, 5, New SolidBrush(Color.Transparent))

            Dim strdist As SizeF = g.MeasureString(Me.Tag, New Font("Arial", 10, FontStyle.Bold, GraphicsUnit.Pixel, 0, False), New PointF(0, 0), New StringFormat(StringFormatFlags.NoClip, 0))
            Dim strx As Single = (Me.Width - strdist.Width) / 2
            'g.FillRectangle(Brushes.White, X + strx, Y + CSng(Height + 5), strdist.Width, strdist.Height)
            g.DrawString(Me.Tag, New Font("Arial", 10, FontStyle.Bold, GraphicsUnit.Pixel, 0, False), New SolidBrush(Me.LineColor), X + strx, Y + Height + 5)

            g.EndContainer(gContainer)

        End Sub

        Public Sub DrawRoundRect(ByVal g As Graphics, ByVal p As Pen, ByVal x As Integer, ByVal y As Integer, ByVal width As Integer, ByVal height As Integer, ByVal radius As Integer, ByVal myBrush As Brush)

            Dim gp As Drawing2D.GraphicsPath = New Drawing2D.GraphicsPath

            gp.AddLine(x + radius, y, x + width - radius, y)
            gp.AddArc(x + width - radius, y, radius, radius, 270, 90)
            gp.AddLine(x + width, y + radius, x + width, y + height - radius)
            gp.AddArc(x + width - radius, y + height - radius, radius, radius, 0, 90)
            gp.AddLine(x + width - radius, y + height, x + radius, y + height)
            gp.AddArc(x, y + height - radius, radius, radius, 90, 90)
            gp.AddLine(x, y + height - radius, x, y + radius)
            gp.AddArc(x, y, radius, radius, 180, 90)

            gp.CloseFigure()

            g.DrawPath(p, gp)
            g.FillPath(myBrush, gp)

            gp.Dispose()

        End Sub

    End Class

    <Serializable()> Public Class AbsorptionColumnGraphic

        Inherits ShapeGraphic

#Region "Constructors"
        Public Sub New()
            Me.TipoObjeto = GraphicObjects.TipoObjeto.AbsorptionColumn
            Me.Description = "AbsorptionColumn"
            Me.m_mixpoint = True
            Me.m_splitpoint = True
            'CreateConnectors(2, 2)
            'Me.InputConnectors(1).Type = ConType.ConEn
        End Sub

        Public Sub New(ByVal graphicPosition As Point)
            Me.New()
            Me.SetPosition(graphicPosition)
        End Sub

        Public Sub New(ByVal posX As Integer, ByVal posY As Integer)
            Me.New(New Point(posX, posY))
        End Sub

        Public Sub New(ByVal graphicPosition As Point, ByVal graphicSize As Size)
            Me.New(graphicPosition)
            Me.SetSize(graphicSize)
            Me.AutoSize = False
        End Sub

        Public Sub New(ByVal posX As Integer, ByVal posY As Integer, ByVal graphicSize As Size)
            Me.New(New Point(posX, posY), graphicSize)
        End Sub

        Public Sub New(ByVal posX As Integer, ByVal posY As Integer, ByVal width As Integer, ByVal height As Integer)
            Me.New(New Point(posX, posY), New Size(width, height))
        End Sub

        Public Sub New(ByVal graphicPosition As Point, ByVal Rotation As Single)
            Me.New()
            Me.SetPosition(graphicPosition)
            Me.Rotation = Rotation
        End Sub

        Public Sub New(ByVal posX As Integer, ByVal posY As Integer, ByVal Rotation As Single)
            Me.New(New Point(posX, posY), Rotation)
        End Sub

        Public Sub New(ByVal graphicPosition As Point, ByVal graphicSize As Size, ByVal Rotation As Single)
            Me.New(graphicPosition, Rotation)
            Me.SetSize(graphicSize)
            Me.AutoSize = False
        End Sub

        Public Sub New(ByVal posX As Integer, ByVal posY As Integer, ByVal graphicSize As Size, ByVal Rotation As Single)
            Me.New(New Point(posX, posY), graphicSize, Rotation)
        End Sub

        Public Sub New(ByVal posX As Integer, ByVal posY As Integer, ByVal width As Integer, _
                               ByVal height As Integer, ByVal Rotation As Single)
            Me.New(New Point(posX, posY), New Size(width, height), Rotation)
        End Sub

#End Region

        Public Overrides Sub PositionConnectors()

            MyBase.PositionConnectors()

            If Not Me.AdditionalInfo Is Nothing Then

                Dim obj1(Me.InputConnectors.Count), obj2(Me.InputConnectors.Count) As Double
                Dim obj3(Me.OutputConnectors.Count), obj4(Me.OutputConnectors.Count) As Double
                obj1 = Me.AdditionalInfo(0)
                obj2 = Me.AdditionalInfo(1)
                obj3 = Me.AdditionalInfo(2)
                obj4 = Me.AdditionalInfo(3)

                Try
                    Dim i As Integer = 0
                    For Each ic As ConnectionPoint In Me.InputConnectors
                        ic.Position = New Point(Me.X + obj1(i), Me.Y + obj2(i))
                        i = i + 1
                    Next
                    i = 0
                    For Each oc As ConnectionPoint In Me.OutputConnectors
                        oc.Position = New Point(Me.X + obj3(i), Me.Y + obj4(i))
                        i = i + 1
                    Next
                Catch ex As Exception

                End Try

            End If

        End Sub

        Public Overrides Sub Draw(ByVal g As System.Drawing.Graphics)

            MyBase.Draw(g)

            UpdateStatus(Me)

            If Me.Status = GraphicObjects.Status.Calculating Then

            End If

            Dim gContainer As System.Drawing.Drawing2D.GraphicsContainer
            Dim myMatrix As Drawing2D.Matrix
            gContainer = g.BeginContainer()
            myMatrix = g.Transform()
            If m_Rotation <> 0 Then
                myMatrix.RotateAt(m_Rotation, New PointF(X, Y), Drawing.Drawing2D.MatrixOrder.Append)
                g.Transform = myMatrix
            End If

            Dim myPen As New Pen(Me.LineColor, Me.LineWidth)
            Dim myPen1 As New Pen(Me.LineColor, Me.LineWidth)
            With myPen1
                .EndCap = LineCap.ArrowAnchor
            End With
            Dim myPen2 As New Pen(Color.White, 0)

            Dim rect As New Rectangle(X, Y, Width, Height)

            g.SmoothingMode = SmoothingMode.AntiAlias
            'g.DrawRectangle(myPen2, rect)

            Dim lgb1 As LinearGradientBrush
            lgb1 = New LinearGradientBrush(rect, Me.GradientColor1, Me.GradientColor2, LinearGradientMode.Horizontal)
            lgb1.SetBlendTriangularShape(0.5)
            'lgb1.CenterColor = Me.GradientColor1

            Dim path As New GraphicsPath()
            path.AddEllipse(rect)
            Dim pthGrBrush As New PathGradientBrush(path)
            pthGrBrush.CenterColor = Me.GradientColor2
            Dim colors As Color() = {Me.GradientColor1}
            pthGrBrush.SurroundColors = colors
            pthGrBrush.SetSigmaBellShape(0.5)

            If Me.Fill Then
                If Me.GradientMode = False Then
                    If Me.FlippedH = True Then
                        Me.DrawRoundRect(g, myPen, X + (1 - 0.1 - 0.2) * Width, Y + 0.1 * Height, 0.2 * 1.25 * Width, 0.8 * Height, 20, New SolidBrush(Me.FillColor))
                        'g.FillEllipse(New SolidBrush(Me.FillColor), CSng(X + (0.475 - 0.15) * Width), CSng(Y + 0.1 * Height), CSng(0.15 * 1.25 * Width), CSng(0.15 * Height))
                        'g.FillEllipse(New SolidBrush(Me.FillColor), CSng(X + (0.475 - 0.15) * Width), CSng(Y + 0.75 * Height), CSng(0.15 * 1.25 * Width), CSng(0.15 * Height))
                    Else
                        Me.DrawRoundRect(g, myPen, X + (0.05) * 1.25 * Width, Y + 0.1 * Height, 0.2 * 1.25 * Width, 0.8 * Height, 20, New SolidBrush(Me.FillColor))
                        'g.FillEllipse(New SolidBrush(Me.FillColor), CSng(X + 0.525 * 1.25 * Width), CSng(Y + 0.1 * Height), CSng(0.15 * 1.25 * Width), CSng(0.15 * Height))
                        'g.FillEllipse(New SolidBrush(Me.FillColor), CSng(X + 0.525 * 1.25 * Width), CSng(Y + 0.75 * Height), CSng(0.15 * 1.25 * Width), CSng(0.15 * Height))
                    End If
                Else
                    If Me.FlippedH = True Then
                        Me.DrawRoundRect(g, myPen, X + (1 - 0.1 - 0.2) * Width, Y + 0.1 * Height, 0.2 * 1.25 * Width, 0.8 * Height, 20, lgb1)
                        'g.FillEllipse(pthGrBrush, CSng(X + (0.475 - 0.15) * Width), CSng(Y + 0.1 * Height), CSng(0.15 * 1.25 * Width), CSng(0.15 * Height))
                        'g.FillEllipse(pthGrBrush, CSng(X + (0.475 - 0.15) * Width), CSng(Y + 0.75 * Height), CSng(0.15 * 1.25 * Width), CSng(0.15 * Height))
                    Else
                        Me.DrawRoundRect(g, myPen, X + 0.05 * 1.25 * Width, Y + 0.1 * Height, 0.2 * 1.25 * Width, 0.8 * Height, 20, lgb1)
                        'g.FillEllipse(pthGrBrush, CSng(X + 0.525 * 1.25 * Width), CSng(Y + 0.1 * Height), CSng(0.15 * 1.25 * Width), CSng(0.15 * Height))
                        'g.FillEllipse(pthGrBrush, CSng(X + 0.525 * 1.25 * Width), CSng(Y + 0.75 * Height), CSng(0.15 * 1.25 * Width), CSng(0.15 * Height))
                    End If
                End If
            End If

            If Me.FlippedH = True Then

                Me.DrawRoundRect(g, myPen, X + (1 - 0.1 - 0.2) * Width, Y + 0.1 * Height, 0.2 * 1.25 * Width, 0.8 * Height, 20, Brushes.Transparent)
                g.DrawLines(myPen, New PointF() {New PointF(X + 0.825 * Width, Y + 0.1 * Height), New PointF(X + 0.825 * Width, Y + 0.02 * Height), New PointF(X + 0.4 * Width, Y + 0.02 * Height)})
                g.DrawLines(myPen, New PointF() {New PointF(X + 0.825 * Width, Y + 0.9 * Height), New PointF(X + 0.825 * Width, Y + 0.98 * Height), New PointF(X + 0.4 * Width, Y + 0.98 * Height)})

                'g.DrawEllipse(myPen, CSng(X + (1 - 0.525 - 0.15) * Width), CSng(Y + 0.1 * Height), CSng(0.15 * 1.25 * Width), CSng(0.15 * Height))
                'g.DrawEllipse(myPen, CSng(X + (1 - 0.525 - 0.15) * Width), CSng(Y + 0.75 * Height), CSng(0.15 * 1.25 * Width), CSng(0.15 * Height))

                'g.DrawLines(myPen, New PointF() {New PointF(X + 0.4 * Width, Y + 0.25 * Height), New PointF(X + 0.4 * Width, Y + 0.3 * Height)})
                'g.DrawLines(myPen, New PointF() {New PointF(X + 0.4 * Width, Y + 0.75 * Height), New PointF(X + 0.4 * Width, Y + 0.7 * Height)})

                'g.DrawLine(myPen1, CSng(X + 0.4 * Width), CSng(Y + 0.3 * Height), CSng(X + 0.7 * Width), CSng(Y + 0.3 * Height))
                'g.DrawLine(myPen1, CSng(X + 0.4 * Width), CSng(Y + 0.7 * Height), CSng(X + 0.7 * Width), CSng(Y + 0.7 * Height))

                g.DrawLine(myPen1, CSng(X + 0.4 * Width), CSng(Y + 0.98 * Height), CSng(X), CSng(Y + 0.98 * Height))

                g.DrawLine(myPen1, CSng(X + 0.4 * Width), CSng(Y + 0.02 * Height), CSng(X), CSng(Y + 0.02 * Height))


                'g.DrawLines(myPen, New PointF() {New PointF(X + 0.6 * Width, Y + 0.175 * Height), New PointF(X + 0.45 * Width, Y + 0.175 * Height), New PointF(X + 0.425 * Width, Y + 0.125 * Height), New PointF(X + 0.375 * Width, Y + 0.225 * Height), New PointF(X + 0.35 * Width, Y + 0.175 * Height)})
                'g.DrawLine(myPen1, CSng(X + 0.35 * Width), CSng(Y + 0.175 * Height), CSng(X), CSng(Y + 0.175 * Height))

                'g.DrawLines(myPen, New PointF() {New PointF(X + 0.5 * Width, Y + 0.825 * Height), New PointF(X + 0.45 * Width, Y + 0.825 * Height), New PointF(X + 0.425 * Width, Y + 0.875 * Height), New PointF(X + 0.375 * Width, Y + 0.775 * Height), New PointF(X + 0.35 * Width, Y + 0.825 * Height), New PointF(X, Y + 0.825 * Height)})
                'g.DrawLine(myPen1, CSng(X + 0.5 * Width), CSng(Y + 0.825 * Height), CSng(X + 0.6 * Width), CSng(Y + 0.825 * Height))

                g.DrawLine(myPen1, CSng(X + Width), CSng(Y + 0.2 * Height), CSng(X + 0.95 * Width), CSng(Y + 0.2 * Height))
                g.DrawLine(myPen1, CSng(X + Width), CSng(Y + 0.8 * Height), CSng(X + 0.95 * Width), CSng(Y + 0.8 * Height))

                g.DrawLine(myPen, CSng(X + 0.7 * Width), CSng(Y + 0.2 * Height), CSng(X + 0.95 * Width), CSng(Y + 0.2 * Height))
                g.DrawLine(myPen, CSng(X + 0.7 * Width), CSng(Y + 0.3 * Height), CSng(X + 0.95 * Width), CSng(Y + 0.3 * Height))
                g.DrawLine(myPen, CSng(X + 0.7 * Width), CSng(Y + 0.4 * Height), CSng(X + 0.95 * Width), CSng(Y + 0.4 * Height))
                g.DrawLine(myPen, CSng(X + 0.7 * Width), CSng(Y + 0.5 * Height), CSng(X + 0.95 * Width), CSng(Y + 0.5 * Height))
                g.DrawLine(myPen, CSng(X + 0.7 * Width), CSng(Y + 0.6 * Height), CSng(X + 0.95 * Width), CSng(Y + 0.6 * Height))
                g.DrawLine(myPen, CSng(X + 0.7 * Width), CSng(Y + 0.7 * Height), CSng(X + 0.95 * Width), CSng(Y + 0.7 * Height))
                g.DrawLine(myPen, CSng(X + 0.7 * Width), CSng(Y + 0.8 * Height), CSng(X + 0.95 * Width), CSng(Y + 0.8 * Height))

            Else

                Me.DrawRoundRect(g, myPen, X + 0.05 * 1.25 * Width, Y + 0.1 * Height, 0.2 * 1.25 * Width, 0.8 * Height, 20, Brushes.Transparent)
                g.DrawLines(myPen, New PointF() {New PointF(X + 0.175 * Width, Y + 0.1 * Height), New PointF(X + 0.175 * Width, Y + 0.02 * Height), New PointF(X + 0.6 * 1.25 * Width, Y + 0.02 * Height)})
                g.DrawLines(myPen, New PointF() {New PointF(X + 0.175 * Width, Y + 0.9 * Height), New PointF(X + 0.175 * Width, Y + 0.98 * Height), New PointF(X + 0.6 * 1.25 * Width, Y + 0.98 * Height)})

                'g.DrawEllipse(myPen, CSng(X + 0.525 * 1.25 * Width), CSng(Y + 0.1 * Height), CSng(0.15 * 1.25 * Width), CSng(0.15 * Height))
                'g.DrawEllipse(myPen, CSng(X + 0.525 * 1.25 * Width), CSng(Y + 0.75 * Height), CSng(0.15 * 1.25 * Width), CSng(0.15 * Height))

                'g.DrawLines(myPen, New PointF() {New PointF(X + 0.6 * 1.25 * Width, Y + 0.25 * Height), New PointF(X + 0.6 * 1.25 * Width, Y + 0.3 * Height)})
                'g.DrawLines(myPen, New PointF() {New PointF(X + 0.6 * 1.25 * Width, Y + 0.75 * Height), New PointF(X + 0.6 * 1.25 * Width, Y + 0.7 * Height)})

                'g.DrawLine(myPen1, CSng(X + 0.6 * 1.25 * Width), CSng(Y + 0.3 * Height), CSng(X + 0.25 * 1.25 * Width), CSng(Y + 0.3 * Height))
                'g.DrawLine(myPen1, CSng(X + 0.6 * 1.25 * Width), CSng(Y + 0.7 * Height), CSng(X + 0.25 * 1.25 * Width), CSng(Y + 0.7 * Height))

                g.DrawLine(myPen1, CSng(X + 0.6 * 1.25 * Width), CSng(Y + 0.98 * Height), CSng(X + Width), CSng(Y + 0.98 * Height))

                g.DrawLine(myPen1, CSng(X + 0.6 * 1.25 * Width), CSng(Y + 0.02 * Height), CSng(X + Width), CSng(Y + 0.02 * Height))


                'g.DrawLines(myPen, New PointF() {New PointF(X + 0.4 * 1.25 * Width, Y + 0.175 * Height), New PointF(X + 0.55 * 1.25 * Width, Y + 0.175 * Height), New PointF(X + 0.575 * 1.25 * Width, Y + 0.125 * Height), New PointF(X + 0.625 * 1.25 * Width, Y + 0.225 * Height), New PointF(X + 0.65 * 1.25 * Width, Y + 0.175 * Height)})
                'g.DrawLine(myPen1, CSng(X + 0.65 * 1.25 * Width), CSng(Y + 0.175 * Height), CSng(X + Width), CSng(Y + 0.175 * Height))

                'g.DrawLines(myPen, New PointF() {New PointF(X + 0.5 * 1.25 * Width, Y + 0.825 * Height), New PointF(X + 0.55 * 1.25 * Width, Y + 0.825 * Height), New PointF(X + 0.575 * 1.25 * Width, Y + 0.875 * Height), New PointF(X + 0.625 * 1.25 * Width, Y + 0.775 * Height), New PointF(X + 0.65 * 1.25 * Width, Y + 0.825 * Height), New PointF(X + Width, Y + 0.825 * Height)})
                'g.DrawLine(myPen1, CSng(X + 0.5 * 1.25 * Width), CSng(Y + 0.825 * Height), CSng(X + 0.4 * 1.25 * Width), CSng(Y + 0.825 * Height))

                g.DrawLine(myPen1, CSng(X), CSng(Y + 0.2 * Height), CSng(X + 0.05 * 1.25 * Width), CSng(Y + 0.2 * Height))
                g.DrawLine(myPen1, CSng(X), CSng(Y + 0.8 * Height), CSng(X + 0.05 * 1.25 * Width), CSng(Y + 0.8 * Height))

                g.DrawLine(myPen, CSng(X + 0.05 * 1.25 * Width), CSng(Y + 0.2 * Height), CSng(X + 0.31 * Width), CSng(Y + 0.2 * Height))
                g.DrawLine(myPen, CSng(X + 0.05 * 1.25 * Width), CSng(Y + 0.3 * Height), CSng(X + 0.31 * Width), CSng(Y + 0.3 * Height))
                g.DrawLine(myPen, CSng(X + 0.05 * 1.25 * Width), CSng(Y + 0.4 * Height), CSng(X + 0.31 * Width), CSng(Y + 0.4 * Height))
                g.DrawLine(myPen, CSng(X + 0.05 * 1.25 * Width), CSng(Y + 0.5 * Height), CSng(X + 0.31 * Width), CSng(Y + 0.5 * Height))
                g.DrawLine(myPen, CSng(X + 0.05 * 1.25 * Width), CSng(Y + 0.6 * Height), CSng(X + 0.31 * Width), CSng(Y + 0.6 * Height))
                g.DrawLine(myPen, CSng(X + 0.05 * 1.25 * Width), CSng(Y + 0.7 * Height), CSng(X + 0.31 * Width), CSng(Y + 0.7 * Height))
                g.DrawLine(myPen, CSng(X + 0.05 * 1.25 * Width), CSng(Y + 0.8 * Height), CSng(X + 0.31 * Width), CSng(Y + 0.8 * Height))

            End If

            Me.DrawRoundRect(g, myPen, Me.X, Me.Y, Me.Width, Me.Height, 5, New SolidBrush(Color.Transparent))

            Dim strdist As SizeF = g.MeasureString(Me.Tag, New Font("Arial", 10, FontStyle.Bold, GraphicsUnit.Pixel, 0, False), New PointF(0, 0), New StringFormat(StringFormatFlags.NoClip, 0))
            Dim strx As Single = (Me.Width - strdist.Width) / 2
            'g.FillRectangle(Brushes.White, X + strx, Y + CSng(Height + 5), strdist.Width, strdist.Height)
            g.DrawString(Me.Tag, New Font("Arial", 10, FontStyle.Bold, GraphicsUnit.Pixel, 0, False), New SolidBrush(Me.LineColor), X + strx, Y + Height + 5)

            g.EndContainer(gContainer)

        End Sub


        Public Sub DrawRoundRect(ByVal g As Graphics, ByVal p As Pen, ByVal x As Integer, ByVal y As Integer, ByVal width As Integer, ByVal height As Integer, ByVal radius As Integer, ByVal myBrush As Brush)

            Dim gp As Drawing2D.GraphicsPath = New Drawing2D.GraphicsPath

            gp.AddLine(x + radius, y, x + width - radius, y)
            gp.AddArc(x + width - radius, y, radius, radius, 270, 90)
            gp.AddLine(x + width, y + radius, x + width, y + height - radius)
            gp.AddArc(x + width - radius, y + height - radius, radius, radius, 0, 90)
            gp.AddLine(x + width - radius, y + height, x + radius, y + height)
            gp.AddArc(x, y + height - radius, radius, radius, 90, 90)
            gp.AddLine(x, y + height - radius, x, y + radius)
            gp.AddArc(x, y, radius, radius, 180, 90)

            gp.CloseFigure()

            g.DrawPath(p, gp)
            g.FillPath(myBrush, gp)

            gp.Dispose()

        End Sub

    End Class

    <Serializable()> Public Class ReboiledAbsorberGraphic

        Inherits ShapeGraphic

#Region "Constructors"
        Public Sub New()
            Me.TipoObjeto = GraphicObjects.TipoObjeto.ReboiledAbsorber
            'CreateConnectors(2, 2)
            'Me.InputConnectors(1).Type = ConType.ConEn
            Me.Description = "ReboiledAbsorber"
            Me.m_mixpoint = True
            Me.m_splitpoint = True
        End Sub

        Public Sub New(ByVal graphicPosition As Point)
            Me.New()
            Me.SetPosition(graphicPosition)
        End Sub

        Public Sub New(ByVal posX As Integer, ByVal posY As Integer)
            Me.New(New Point(posX, posY))
        End Sub

        Public Sub New(ByVal graphicPosition As Point, ByVal graphicSize As Size)
            Me.New(graphicPosition)
            Me.SetSize(graphicSize)
            Me.AutoSize = False
        End Sub

        Public Sub New(ByVal posX As Integer, ByVal posY As Integer, ByVal graphicSize As Size)
            Me.New(New Point(posX, posY), graphicSize)
        End Sub

        Public Sub New(ByVal posX As Integer, ByVal posY As Integer, ByVal width As Integer, ByVal height As Integer)
            Me.New(New Point(posX, posY), New Size(width, height))
        End Sub

        Public Sub New(ByVal graphicPosition As Point, ByVal Rotation As Single)
            Me.New()
            Me.SetPosition(graphicPosition)
            Me.Rotation = Rotation
        End Sub

        Public Sub New(ByVal posX As Integer, ByVal posY As Integer, ByVal Rotation As Single)
            Me.New(New Point(posX, posY), Rotation)
        End Sub

        Public Sub New(ByVal graphicPosition As Point, ByVal graphicSize As Size, ByVal Rotation As Single)
            Me.New(graphicPosition, Rotation)
            Me.SetSize(graphicSize)
            Me.AutoSize = False
        End Sub

        Public Sub New(ByVal posX As Integer, ByVal posY As Integer, ByVal graphicSize As Size, ByVal Rotation As Single)
            Me.New(New Point(posX, posY), graphicSize, Rotation)
        End Sub

        Public Sub New(ByVal posX As Integer, ByVal posY As Integer, ByVal width As Integer, _
                               ByVal height As Integer, ByVal Rotation As Single)
            Me.New(New Point(posX, posY), New Size(width, height), Rotation)
        End Sub

#End Region

        Public Overrides Sub PositionConnectors()

            MyBase.PositionConnectors()

            If Not Me.AdditionalInfo Is Nothing Then

                Dim obj1(Me.InputConnectors.Count), obj2(Me.InputConnectors.Count) As Double
                Dim obj3(Me.OutputConnectors.Count), obj4(Me.OutputConnectors.Count) As Double
                obj1 = Me.AdditionalInfo(0)
                obj2 = Me.AdditionalInfo(1)
                obj3 = Me.AdditionalInfo(2)
                obj4 = Me.AdditionalInfo(3)

                Try
                    Dim i As Integer = 0
                    For Each ic As ConnectionPoint In Me.InputConnectors
                        ic.Position = New Point(Me.X + obj1(i), Me.Y + obj2(i))
                        i = i + 1
                    Next
                    i = 0
                    For Each oc As ConnectionPoint In Me.OutputConnectors
                        oc.Position = New Point(Me.X + obj3(i), Me.Y + obj4(i))
                        i = i + 1
                    Next
                Catch ex As Exception

                End Try

            End If

        End Sub

        Public Overrides Sub Draw(ByVal g As System.Drawing.Graphics)

            MyBase.Draw(g)

            UpdateStatus(Me)

            Dim gContainer As System.Drawing.Drawing2D.GraphicsContainer
            Dim myMatrix As Drawing2D.Matrix
            gContainer = g.BeginContainer()
            myMatrix = g.Transform()
            If m_Rotation <> 0 Then
                myMatrix.RotateAt(m_Rotation, New PointF(X, Y), Drawing.Drawing2D.MatrixOrder.Append)
                g.Transform = myMatrix
            End If

            Dim myPen As New Pen(Me.LineColor, Me.LineWidth)
            Dim myPen1 As New Pen(Me.LineColor, Me.LineWidth)
            With myPen1
                .EndCap = LineCap.ArrowAnchor
            End With
            Dim myPen2 As New Pen(Color.White, 0)

            Dim rect As New Rectangle(X, Y, Width, Height)

            g.SmoothingMode = SmoothingMode.AntiAlias
            'g.DrawRectangle(myPen2, rect)

            Dim lgb1 As LinearGradientBrush
            lgb1 = New LinearGradientBrush(rect, Me.GradientColor1, Me.GradientColor2, LinearGradientMode.Horizontal)
            lgb1.SetBlendTriangularShape(0.5)
            'lgb1.CenterColor = Me.GradientColor1

            Dim path As New GraphicsPath()
            path.AddEllipse(rect)
            Dim pthGrBrush As New PathGradientBrush(path)
            pthGrBrush.CenterColor = Me.GradientColor2
            Dim colors As Color() = {Me.GradientColor1}
            pthGrBrush.SurroundColors = colors
            pthGrBrush.SetSigmaBellShape(0.5)

            If Me.Fill Then
                If Me.GradientMode = False Then
                    If Me.FlippedH = True Then
                        Me.DrawRoundRect(g, myPen, X + (1 - 0.1 - 0.2) * Width, Y + 0.1 * Height, 0.2 * 1.25 * Width, 0.8 * Height, 20, New SolidBrush(Me.FillColor))
                        'g.FillEllipse(New SolidBrush(Me.FillColor), CSng(X + (0.475 - 0.15) * Width), CSng(Y + 0.1 * Height), CSng(0.15 * 1.25 * Width), CSng(0.15 * Height))
                        g.FillEllipse(New SolidBrush(Me.FillColor), CSng(X + (0.475 - 0.15) * Width), CSng(Y + 0.75 * Height), CSng(0.15 * 1.25 * Width), CSng(0.15 * Height))
                    Else
                        Me.DrawRoundRect(g, myPen, X + (0.05) * 1.25 * Width, Y + 0.1 * Height, 0.2 * 1.25 * Width, 0.8 * Height, 20, New SolidBrush(Me.FillColor))
                        'g.FillEllipse(New SolidBrush(Me.FillColor), CSng(X + 0.525 * 1.25 * Width), CSng(Y + 0.1 * Height), CSng(0.15 * 1.25 * Width), CSng(0.15 * Height))
                        g.FillEllipse(New SolidBrush(Me.FillColor), CSng(X + 0.525 * 1.25 * Width), CSng(Y + 0.75 * Height), CSng(0.15 * 1.25 * Width), CSng(0.15 * Height))
                    End If
                Else
                    If Me.FlippedH = True Then
                        Me.DrawRoundRect(g, myPen, X + (1 - 0.1 - 0.2) * Width, Y + 0.1 * Height, 0.2 * 1.25 * Width, 0.8 * Height, 20, lgb1)
                        'g.FillEllipse(pthGrBrush, CSng(X + (0.475 - 0.15) * Width), CSng(Y + 0.1 * Height), CSng(0.15 * 1.25 * Width), CSng(0.15 * Height))
                        g.FillEllipse(pthGrBrush, CSng(X + (0.475 - 0.15) * Width), CSng(Y + 0.75 * Height), CSng(0.15 * 1.25 * Width), CSng(0.15 * Height))
                    Else
                        Me.DrawRoundRect(g, myPen, X + 0.05 * 1.25 * Width, Y + 0.1 * Height, 0.2 * 1.25 * Width, 0.8 * Height, 20, lgb1)
                        'g.FillEllipse(pthGrBrush, CSng(X + 0.525 * 1.25 * Width), CSng(Y + 0.1 * Height), CSng(0.15 * 1.25 * Width), CSng(0.15 * Height))
                        g.FillEllipse(pthGrBrush, CSng(X + 0.525 * 1.25 * Width), CSng(Y + 0.75 * Height), CSng(0.15 * 1.25 * Width), CSng(0.15 * Height))
                    End If
                End If
            End If

            If Me.FlippedH = True Then

                Me.DrawRoundRect(g, myPen, X + (1 - 0.1 - 0.2) * Width, Y + 0.1 * Height, 0.2 * 1.25 * Width, 0.8 * Height, 20, Brushes.Transparent)
                g.DrawLines(myPen, New PointF() {New PointF(X + 0.825 * Width, Y + 0.1 * Height), New PointF(X + 0.825 * Width, Y + 0.02 * Height), New PointF(X + 0.4 * Width, Y + 0.02 * Height)})
                g.DrawLines(myPen, New PointF() {New PointF(X + 0.825 * Width, Y + 0.9 * Height), New PointF(X + 0.825 * Width, Y + 0.98 * Height), New PointF(X + 0.4 * Width, Y + 0.98 * Height), New PointF(X + 0.4 * Width, Y + 0.9 * Height)})

                'g.DrawEllipse(myPen, CSng(X + (1 - 0.525 - 0.15) * Width), CSng(Y + 0.1 * Height), CSng(0.15 * 1.25 * Width), CSng(0.15 * Height))
                g.DrawEllipse(myPen, CSng(X + (1 - 0.525 - 0.15) * Width), CSng(Y + 0.75 * Height), CSng(0.15 * 1.25 * Width), CSng(0.15 * Height))

                'g.DrawLines(myPen, New PointF() {New PointF(X + 0.4 * Width, Y + 0.25 * Height), New PointF(X + 0.4 * Width, Y + 0.3 * Height)})
                g.DrawLines(myPen, New PointF() {New PointF(X + 0.4 * Width, Y + 0.75 * Height), New PointF(X + 0.4 * Width, Y + 0.7 * Height)})

                'g.DrawLine(myPen1, CSng(X + 0.4 * Width), CSng(Y + 0.3 * Height), CSng(X + 0.7 * Width), CSng(Y + 0.3 * Height))
                g.DrawLine(myPen1, CSng(X + 0.4 * Width), CSng(Y + 0.7 * Height), CSng(X + 0.7 * Width), CSng(Y + 0.7 * Height))

                g.DrawLine(myPen1, CSng(X + 0.4 * Width), CSng(Y + 0.98 * Height), CSng(X), CSng(Y + 0.98 * Height))

                g.DrawLine(myPen1, CSng(X + 0.4 * Width), CSng(Y + 0.02 * Height), CSng(X), CSng(Y + 0.02 * Height))


                'g.DrawLines(myPen, New PointF() {New PointF(X + 0.6 * Width, Y + 0.175 * Height), New PointF(X + 0.45 * Width, Y + 0.175 * Height), New PointF(X + 0.425 * Width, Y + 0.125 * Height), New PointF(X + 0.375 * Width, Y + 0.225 * Height), New PointF(X + 0.35 * Width, Y + 0.175 * Height)})
                'g.DrawLine(myPen1, CSng(X + 0.35 * Width), CSng(Y + 0.175 * Height), CSng(X), CSng(Y + 0.175 * Height))

                g.DrawLines(myPen, New PointF() {New PointF(X + 0.5 * Width, Y + 0.825 * Height), New PointF(X + 0.45 * Width, Y + 0.825 * Height), New PointF(X + 0.425 * Width, Y + 0.875 * Height), New PointF(X + 0.375 * Width, Y + 0.775 * Height), New PointF(X + 0.35 * Width, Y + 0.825 * Height), New PointF(X, Y + 0.825 * Height)})
                g.DrawLine(myPen1, CSng(X + 0.5 * Width), CSng(Y + 0.825 * Height), CSng(X + 0.6 * Width), CSng(Y + 0.825 * Height))

                g.DrawLine(myPen1, CSng(X + Width), CSng(Y + 0.2 * Height), CSng(X + 0.95 * Width), CSng(Y + 0.2 * Height))
                g.DrawLine(myPen1, CSng(X + Width), CSng(Y + 0.8 * Height), CSng(X + 0.95 * Width), CSng(Y + 0.8 * Height))

                g.DrawLine(myPen, CSng(X + 0.7 * Width), CSng(Y + 0.2 * Height), CSng(X + 0.95 * Width), CSng(Y + 0.2 * Height))
                g.DrawLine(myPen, CSng(X + 0.7 * Width), CSng(Y + 0.3 * Height), CSng(X + 0.95 * Width), CSng(Y + 0.3 * Height))
                g.DrawLine(myPen, CSng(X + 0.7 * Width), CSng(Y + 0.4 * Height), CSng(X + 0.95 * Width), CSng(Y + 0.4 * Height))
                g.DrawLine(myPen, CSng(X + 0.7 * Width), CSng(Y + 0.5 * Height), CSng(X + 0.95 * Width), CSng(Y + 0.5 * Height))
                g.DrawLine(myPen, CSng(X + 0.7 * Width), CSng(Y + 0.6 * Height), CSng(X + 0.95 * Width), CSng(Y + 0.6 * Height))
                g.DrawLine(myPen, CSng(X + 0.7 * Width), CSng(Y + 0.7 * Height), CSng(X + 0.95 * Width), CSng(Y + 0.7 * Height))
                g.DrawLine(myPen, CSng(X + 0.7 * Width), CSng(Y + 0.8 * Height), CSng(X + 0.95 * Width), CSng(Y + 0.8 * Height))

            Else

                Me.DrawRoundRect(g, myPen, X + 0.05 * 1.25 * Width, Y + 0.1 * Height, 0.2 * 1.25 * Width, 0.8 * Height, 20, Brushes.Transparent)
                g.DrawLines(myPen, New PointF() {New PointF(X + 0.175 * Width, Y + 0.1 * Height), New PointF(X + 0.175 * Width, Y + 0.02 * Height), New PointF(X + 0.6 * 1.25 * Width, Y + 0.02 * Height)})
                g.DrawLines(myPen, New PointF() {New PointF(X + 0.175 * Width, Y + 0.9 * Height), New PointF(X + 0.175 * Width, Y + 0.98 * Height), New PointF(X + 0.6 * 1.25 * Width, Y + 0.98 * Height), New PointF(X + 0.6 * 1.25 * Width, Y + 0.9 * Height)})

                'g.DrawEllipse(myPen, CSng(X + 0.525 * 1.25 * Width), CSng(Y + 0.1 * Height), CSng(0.15 * 1.25 * Width), CSng(0.15 * Height))
                g.DrawEllipse(myPen, CSng(X + 0.525 * 1.25 * Width), CSng(Y + 0.75 * Height), CSng(0.15 * 1.25 * Width), CSng(0.15 * Height))

                'g.DrawLines(myPen, New PointF() {New PointF(X + 0.6 * 1.25 * Width, Y + 0.25 * Height), New PointF(X + 0.6 * 1.25 * Width, Y + 0.3 * Height)})
                g.DrawLines(myPen, New PointF() {New PointF(X + 0.6 * 1.25 * Width, Y + 0.75 * Height), New PointF(X + 0.6 * 1.25 * Width, Y + 0.7 * Height)})

                'g.DrawLine(myPen1, CSng(X + 0.6 * 1.25 * Width), CSng(Y + 0.3 * Height), CSng(X + 0.25 * 1.25 * Width), CSng(Y + 0.3 * Height))
                g.DrawLine(myPen1, CSng(X + 0.6 * 1.25 * Width), CSng(Y + 0.7 * Height), CSng(X + 0.25 * 1.25 * Width), CSng(Y + 0.7 * Height))

                g.DrawLine(myPen1, CSng(X + 0.6 * 1.25 * Width), CSng(Y + 0.98 * Height), CSng(X + Width), CSng(Y + 0.98 * Height))

                g.DrawLine(myPen1, CSng(X + 0.6 * 1.25 * Width), CSng(Y + 0.02 * Height), CSng(X + Width), CSng(Y + 0.02 * Height))


                'g.DrawLines(myPen, New PointF() {New PointF(X + 0.4 * 1.25 * Width, Y + 0.175 * Height), New PointF(X + 0.55 * 1.25 * Width, Y + 0.175 * Height), New PointF(X + 0.575 * 1.25 * Width, Y + 0.125 * Height), New PointF(X + 0.625 * 1.25 * Width, Y + 0.225 * Height), New PointF(X + 0.65 * 1.25 * Width, Y + 0.175 * Height)})
                'g.DrawLine(myPen1, CSng(X + 0.65 * 1.25 * Width), CSng(Y + 0.175 * Height), CSng(X + Width), CSng(Y + 0.175 * Height))

                g.DrawLines(myPen, New PointF() {New PointF(X + 0.5 * 1.25 * Width, Y + 0.825 * Height), New PointF(X + 0.55 * 1.25 * Width, Y + 0.825 * Height), New PointF(X + 0.575 * 1.25 * Width, Y + 0.875 * Height), New PointF(X + 0.625 * 1.25 * Width, Y + 0.775 * Height), New PointF(X + 0.65 * 1.25 * Width, Y + 0.825 * Height), New PointF(X + Width, Y + 0.825 * Height)})
                g.DrawLine(myPen1, CSng(X + 0.5 * 1.25 * Width), CSng(Y + 0.825 * Height), CSng(X + 0.4 * 1.25 * Width), CSng(Y + 0.825 * Height))

                g.DrawLine(myPen1, CSng(X), CSng(Y + 0.2 * Height), CSng(X + 0.05 * 1.25 * Width), CSng(Y + 0.2 * Height))
                g.DrawLine(myPen1, CSng(X), CSng(Y + 0.8 * Height), CSng(X + 0.05 * 1.25 * Width), CSng(Y + 0.8 * Height))

                g.DrawLine(myPen, CSng(X + 0.05 * 1.25 * Width), CSng(Y + 0.2 * Height), CSng(X + 0.31 * Width), CSng(Y + 0.2 * Height))
                g.DrawLine(myPen, CSng(X + 0.05 * 1.25 * Width), CSng(Y + 0.3 * Height), CSng(X + 0.31 * Width), CSng(Y + 0.3 * Height))
                g.DrawLine(myPen, CSng(X + 0.05 * 1.25 * Width), CSng(Y + 0.4 * Height), CSng(X + 0.31 * Width), CSng(Y + 0.4 * Height))
                g.DrawLine(myPen, CSng(X + 0.05 * 1.25 * Width), CSng(Y + 0.5 * Height), CSng(X + 0.31 * Width), CSng(Y + 0.5 * Height))
                g.DrawLine(myPen, CSng(X + 0.05 * 1.25 * Width), CSng(Y + 0.6 * Height), CSng(X + 0.31 * Width), CSng(Y + 0.6 * Height))
                g.DrawLine(myPen, CSng(X + 0.05 * 1.25 * Width), CSng(Y + 0.7 * Height), CSng(X + 0.31 * Width), CSng(Y + 0.7 * Height))
                g.DrawLine(myPen, CSng(X + 0.05 * 1.25 * Width), CSng(Y + 0.8 * Height), CSng(X + 0.31 * Width), CSng(Y + 0.8 * Height))

            End If

            Me.DrawRoundRect(g, myPen, Me.X, Me.Y, Me.Width, Me.Height, 5, New SolidBrush(Color.Transparent))

            Dim strdist As SizeF = g.MeasureString(Me.Tag, New Font("Arial", 10, FontStyle.Bold, GraphicsUnit.Pixel, 0, False), New PointF(0, 0), New StringFormat(StringFormatFlags.NoClip, 0))
            Dim strx As Single = (Me.Width - strdist.Width) / 2
            'g.FillRectangle(Brushes.White, X + strx, Y + CSng(Height + 5), strdist.Width, strdist.Height)
            g.DrawString(Me.Tag, New Font("Arial", 10, FontStyle.Bold, GraphicsUnit.Pixel, 0, False), New SolidBrush(Me.LineColor), X + strx, Y + Height + 5)

            g.EndContainer(gContainer)

        End Sub

        Public Sub DrawRoundRect(ByVal g As Graphics, ByVal p As Pen, ByVal x As Integer, ByVal y As Integer, ByVal width As Integer, ByVal height As Integer, ByVal radius As Integer, ByVal myBrush As Brush)

            Dim gp As Drawing2D.GraphicsPath = New Drawing2D.GraphicsPath

            gp.AddLine(x + radius, y, x + width - radius, y)
            gp.AddArc(x + width - radius, y, radius, radius, 270, 90)
            gp.AddLine(x + width, y + radius, x + width, y + height - radius)
            gp.AddArc(x + width - radius, y + height - radius, radius, radius, 0, 90)
            gp.AddLine(x + width - radius, y + height, x + radius, y + height)
            gp.AddArc(x, y + height - radius, radius, radius, 90, 90)
            gp.AddLine(x, y + height - radius, x, y + radius)
            gp.AddArc(x, y, radius, radius, 180, 90)

            gp.CloseFigure()

            g.DrawPath(p, gp)
            g.FillPath(myBrush, gp)

            gp.Dispose()

        End Sub

    End Class

    <Serializable()> Public Class RefluxedAbsorberGraphic

        Inherits ShapeGraphic

#Region "Constructors"
        Public Sub New()
            Me.TipoObjeto = GraphicObjects.TipoObjeto.RefluxedAbsorber
            'CreateConnectors(2, 2)
            'Me.InputConnectors(1).Type = ConType.ConEn
            Me.Description = "RefluxedAbsorber"
            Me.m_mixpoint = True
            Me.m_splitpoint = True
        End Sub

        Public Sub New(ByVal graphicPosition As Point)
            Me.New()
            Me.SetPosition(graphicPosition)
        End Sub

        Public Sub New(ByVal posX As Integer, ByVal posY As Integer)
            Me.New(New Point(posX, posY))
        End Sub

        Public Sub New(ByVal graphicPosition As Point, ByVal graphicSize As Size)
            Me.New(graphicPosition)
            Me.SetSize(graphicSize)
            Me.AutoSize = False
        End Sub

        Public Sub New(ByVal posX As Integer, ByVal posY As Integer, ByVal graphicSize As Size)
            Me.New(New Point(posX, posY), graphicSize)
        End Sub

        Public Sub New(ByVal posX As Integer, ByVal posY As Integer, ByVal width As Integer, ByVal height As Integer)
            Me.New(New Point(posX, posY), New Size(width, height))
        End Sub

        Public Sub New(ByVal graphicPosition As Point, ByVal Rotation As Single)
            Me.New()
            Me.SetPosition(graphicPosition)
            Me.Rotation = Rotation
        End Sub

        Public Sub New(ByVal posX As Integer, ByVal posY As Integer, ByVal Rotation As Single)
            Me.New(New Point(posX, posY), Rotation)
        End Sub

        Public Sub New(ByVal graphicPosition As Point, ByVal graphicSize As Size, ByVal Rotation As Single)
            Me.New(graphicPosition, Rotation)
            Me.SetSize(graphicSize)
            Me.AutoSize = False
        End Sub

        Public Sub New(ByVal posX As Integer, ByVal posY As Integer, ByVal graphicSize As Size, ByVal Rotation As Single)
            Me.New(New Point(posX, posY), graphicSize, Rotation)
        End Sub

        Public Sub New(ByVal posX As Integer, ByVal posY As Integer, ByVal width As Integer, _
                               ByVal height As Integer, ByVal Rotation As Single)
            Me.New(New Point(posX, posY), New Size(width, height), Rotation)
        End Sub

#End Region

        Public Overrides Sub PositionConnectors()

            MyBase.PositionConnectors()

            If Not Me.AdditionalInfo Is Nothing Then

                Dim obj1(Me.InputConnectors.Count), obj2(Me.InputConnectors.Count) As Double
                Dim obj3(Me.OutputConnectors.Count), obj4(Me.OutputConnectors.Count) As Double
                obj1 = Me.AdditionalInfo(0)
                obj2 = Me.AdditionalInfo(1)
                obj3 = Me.AdditionalInfo(2)
                obj4 = Me.AdditionalInfo(3)

                Try
                    Dim i As Integer = 0
                    For Each ic As ConnectionPoint In Me.InputConnectors
                        ic.Position = New Point(Me.X + obj1(i), Me.Y + obj2(i))
                        i = i + 1
                    Next
                    i = 0
                    For Each oc As ConnectionPoint In Me.OutputConnectors
                        oc.Position = New Point(Me.X + obj3(i), Me.Y + obj4(i))
                        i = i + 1
                    Next
                Catch ex As Exception

                End Try

            End If

        End Sub

        Public Overrides Sub Draw(ByVal g As System.Drawing.Graphics)

            MyBase.Draw(g)

            UpdateStatus(Me)

            Dim gContainer As System.Drawing.Drawing2D.GraphicsContainer
            Dim myMatrix As Drawing2D.Matrix
            gContainer = g.BeginContainer()
            myMatrix = g.Transform()
            If m_Rotation <> 0 Then
                myMatrix.RotateAt(m_Rotation, New PointF(X, Y), Drawing.Drawing2D.MatrixOrder.Append)
                g.Transform = myMatrix
            End If

            Dim myPen As New Pen(Me.LineColor, Me.LineWidth)
            Dim myPen1 As New Pen(Me.LineColor, Me.LineWidth)
            With myPen1
                .EndCap = LineCap.ArrowAnchor
            End With
            Dim myPen2 As New Pen(Color.White, 0)
            Dim myPen3 As New Pen(Me.LineColor, Me.LineWidth)
            With myPen3
                .DashStyle = DashStyle.Dot
            End With

            Dim rect As New Rectangle(X, Y, Width, Height)

            g.SmoothingMode = SmoothingMode.AntiAlias
            'g.DrawRectangle(myPen2, rect)

            Dim lgb1 As LinearGradientBrush
            lgb1 = New LinearGradientBrush(rect, Me.GradientColor1, Me.GradientColor2, LinearGradientMode.Horizontal)
            lgb1.SetBlendTriangularShape(0.5)
            'lgb1.CenterColor = Me.GradientColor1

            Dim path As New GraphicsPath()
            path.AddEllipse(rect)
            Dim pthGrBrush As New PathGradientBrush(path)
            pthGrBrush.CenterColor = Me.GradientColor2
            Dim colors As Color() = {Me.GradientColor1}
            pthGrBrush.SurroundColors = colors
            pthGrBrush.SetSigmaBellShape(0.5)

            If Me.Fill Then
                If Me.GradientMode = False Then
                    If Me.FlippedH = True Then
                        Me.DrawRoundRect(g, myPen, X + (1 - 0.1 - 0.2) * Width, Y + 0.2 * Height, 0.2 * 1.25 * Width, 0.7 * Height, 20, New SolidBrush(Me.FillColor))
                        Me.DrawRoundRect(g, myPen, X + 0.1 * Width, Y + 0.1 * Height, 0.15 * 1.25 * Width, 0.15 * Height, 20, lgb1)
                        g.FillEllipse(New SolidBrush(Me.FillColor), CSng(X + (0.475 - 0.15) * Width), CSng(Y + 0.0 * Height), CSng(0.15 * 1.25 * Width), CSng(0.15 * Height))
                        'g.FillEllipse(New SolidBrush(Me.FillColor), CSng(X + (0.475 - 0.15) * Width), CSng(Y + 0.75 * Height), CSng(0.15 * 1.25 * Width), CSng(0.15 * Height))
                    Else
                        Me.DrawRoundRect(g, myPen, X + (0.05) * 1.25 * Width, Y + 0.2 * Height, 0.2 * 1.25 * Width, 0.7 * Height, 20, New SolidBrush(Me.FillColor))
                        g.FillEllipse(New SolidBrush(Me.FillColor), CSng(X + 0.525 * 1.25 * Width), CSng(Y), CSng(0.15 * 1.25 * Width), CSng(0.15 * Height))
                        'g.FillEllipse(New SolidBrush(Me.FillColor), CSng(X + 0.525 * 1.25 * Width), CSng(Y + 0.75 * Height), CSng(0.15 * 1.25 * Width), CSng(0.15 * Height))
                    End If
                Else
                    If Me.FlippedH = True Then
                        Me.DrawRoundRect(g, myPen, X + (1 - 0.1 - 0.2) * Width, Y + 0.2 * Height, 0.2 * 1.25 * Width, 0.7 * Height, 20, lgb1)
                        Me.DrawRoundRect(g, myPen, X + 0.1 * Width, Y + 0.1 * Height, 0.15 * 1.25 * Width, 0.15 * Height, 20, lgb1)
                        g.FillEllipse(pthGrBrush, CSng(X + (0.475 - 0.15) * Width), CSng(Y + 0.0 * Height), CSng(0.15 * 1.25 * Width), CSng(0.15 * Height))
                        'g.FillEllipse(pthGrBrush, CSng(X + (0.475 - 0.15) * Width), CSng(Y + 0.75 * Height), CSng(0.15 * 1.25 * Width), CSng(0.15 * Height))
                    Else
                        Me.DrawRoundRect(g, myPen, X + 0.05 * 1.25 * Width, Y + 0.2 * Height, 0.2 * 1.25 * Width, 0.7 * Height, 20, lgb1)
                        Me.DrawRoundRect(g, myPen, X + 0.525 * 1.25 * Width, Y + 0.1 * Height, 0.15 * 1.25 * Width, 0.15 * Height, 20, lgb1)
                        g.FillEllipse(pthGrBrush, CSng(X + 0.3 * 1.25 * Width), CSng(Y), CSng(0.15 * 1.25 * Width), CSng(0.15 * Height))
                        'g.FillEllipse(pthGrBrush, CSng(X + 0.525 * 1.25 * Width), CSng(Y + 0.75 * Height), CSng(0.15 * 1.25 * Width), CSng(0.15 * Height))
                    End If
                End If
            End If

            If Me.FlippedH = True Then

                'Me.DrawRoundRect(g, myPen, X + (1 - 0.1 - 0.2) * Width, Y + 0.2 * Height, 0.2 * 1.25 * Width, 0.7 * Height, 20, Brushes.Transparent)
                'g.DrawLines(myPen, New PointF() {New PointF(X + 0.825 * Width, Y + 0.1 * Height), New PointF(X + 0.825 * Width, Y + 0.02 * Height), New PointF(X + 0.4 * Width, Y + 0.02 * Height), New PointF(X + 0.4 * Width, Y + 0.1 * Height)})
                'g.DrawLines(myPen, New PointF() {New PointF(X + 0.825 * Width, Y + 0.9 * Height), New PointF(X + 0.825 * Width, Y + 0.98 * Height), New PointF(X + 0.4 * Width, Y + 0.98 * Height), New PointF(X + 0.4 * Width, Y + 0.9 * Height)})

                Me.DrawRoundRect(g, myPen, X + 0.7 * Width, Y + 0.2 * Height, 0.2 * 1.25 * Width, 0.7 * Height, 20, Brushes.Transparent)
                Me.DrawRoundRect(g, myPen, X + 0.1 * Width, Y + 0.1 * Height, 0.15 * 1.25 * Width, 0.15 * Height, 20, Brushes.Transparent)
                g.DrawLines(myPen, New PointF() {New PointF(X + 0.175 * Width, Y + 0.1 * Height), New PointF(X + 0.175 * Width, Y + 0.02 * Height), New PointF(X + 0.32 * 1.25 * Width, Y + 0.02 * Height)})
                g.DrawLines(myPen, New PointF() {New PointF(X + 0.38 * 1.25 * Width, Y + 0.02 * Height), New PointF(X + 0.8 * Width, Y + 0.02 * Height), New PointF(X + 0.8 * Width, Y + 0.2 * Height)})
                'g.DrawLines(myPen, New PointF() {New PointF(X + 0.4 * Width, Y + 0.9 * Height), New PointF(X + 0.4 * Width, Y + 0.98 * Height), New PointF(X + 0.8 * Width, Y + 0.98 * Height), New PointF(X + 0.8 * Width, Y + 0.9 * Height)})

                g.DrawEllipse(myPen, CSng(X + (1 - 0.525 - 0.15) * Width), CSng(Y + 0.0 * Height), CSng(0.15 * 1.25 * Width), CSng(0.15 * Height))
                'g.DrawEllipse(myPen, CSng(X + (1 - 0.525 - 0.15) * Width), CSng(Y + 0.75 * Height), CSng(0.15 * 1.25 * Width), CSng(0.15 * Height))

                g.DrawLines(myPen, New PointF() {New PointF(X + 0.175 * Width, Y + 0.25 * Height), New PointF(X + 0.175 * Width, Y + 0.3 * Height)})
                'g.DrawLines(myPen, New PointF() {New PointF(X + 0.4 * Width, Y + 0.75 * Height), New PointF(X + 0.4 * Width, Y + 0.7 * Height)})

                g.DrawLine(myPen1, CSng(X + 0.175 * Width), CSng(Y + 0.3 * Height), CSng(X + 0.7 * Width), CSng(Y + 0.3 * Height))
                'g.DrawLine(myPen1, CSng(X + 0.4 * Width), CSng(Y + 0.7 * Height), CSng(X + 0.7 * Width), CSng(Y + 0.7 * Height))

                g.DrawLine(myPen1, CSng(X + 0.8 * Width), CSng(Y + 0.98 * Height), CSng(X), CSng(Y + 0.98 * Height))
                g.DrawLine(myPen, CSng(X + 0.8 * Width), CSng(Y + 0.9 * Height), CSng(X + 0.8 * Width), CSng(Y + 0.98 * Height))

                If Me.Shape = 1 Then
                    g.DrawLine(myPen1, CSng(X + 0.2 * Width), CSng(Y + 0.02 * Height), CSng(X), CSng(Y + 0.02 * Height))
                    g.DrawLine(myPen1, CSng(X + 0.2 * Width), CSng(Y + 0.3 * Height), CSng(X), CSng(Y + 0.3 * Height))
                ElseIf Me.Shape = 0 Then
                    g.DrawLine(myPen1, CSng(X + 0.2 * Width), CSng(Y + 0.3 * Height), CSng(X), CSng(Y + 0.3 * Height))
                Else
                    g.DrawLine(myPen1, CSng(X + 0.2 * Width), CSng(Y + 0.02 * Height), CSng(X), CSng(Y + 0.02 * Height))
                End If

                'g.DrawLines(myPen, New PointF() {New PointF(X + 0.6 * Width, Y + 0.07 * Height), New PointF(X + 0.45 * Width, Y + 0.07 * Height), New PointF(X + 0.425 * Width, Y + 0.125 * Height), New PointF(X + 0.375 * Width, Y + 0.225 * Height), New PointF(X + 0.35 * Width, Y + 0.175 * Height)})
                'g.DrawLine(myPen1, CSng(X + 0.35 * Width), CSng(Y + 0.175 * Height), CSng(X), CSng(Y + 0.175 * Height))

                g.DrawLines(myPen, New PointF() {New PointF(X + (1 - 0.4) * Width, Y + 0.07 * Height), New PointF(X + (1 - 0.5) * Width, Y + 0.07 * Height), New PointF(X + (1 - 0.55) * Width, Y + 0.125 * Height), New PointF(X + (1 - 0.6) * Width, Y + 0.02 * Height), New PointF(X + (1 - 0.65) * Width, Y + 0.08 * Height)})
                g.DrawLine(myPen3, CSng(X + 0.275 * 1.25 * Width), CSng(Y + 0.08 * Height), CSng(X + 0.1 * Width), CSng(Y + 0.08 * Height))
                g.DrawLine(myPen1, CSng(X + 0.1 * 1.25 * Width), CSng(Y + 0.08 * Height), CSng(X), CSng(Y + 0.08 * Height))

                'g.DrawLines(myPen, New PointF() {New PointF(X + 0.5 * Width, Y + 0.825 * Height), New PointF(X + 0.45 * Width, Y + 0.825 * Height), New PointF(X + 0.425 * Width, Y + 0.875 * Height), New PointF(X + 0.375 * Width, Y + 0.775 * Height), New PointF(X + 0.35 * Width, Y + 0.825 * Height), New PointF(X, Y + 0.825 * Height)})
                'g.DrawLine(myPen1, CSng(X + 0.5 * Width), CSng(Y + 0.825 * Height), CSng(X + 0.6 * Width), CSng(Y + 0.825 * Height))

                g.DrawLine(myPen1, CSng(X + Width), CSng(Y + 0.5 * Height), CSng(X + 0.95 * Width), CSng(Y + 0.5 * Height))

                'g.DrawLine(myPen, CSng(X + 0.7 * Width), CSng(Y + 0.2 * Height), CSng(X + 0.95 * Width), CSng(Y + 0.2 * Height))
                g.DrawLine(myPen, CSng(X + 0.7 * Width), CSng(Y + 0.3 * Height), CSng(X + 0.95 * Width), CSng(Y + 0.3 * Height))
                g.DrawLine(myPen, CSng(X + 0.7 * Width), CSng(Y + 0.4 * Height), CSng(X + 0.95 * Width), CSng(Y + 0.4 * Height))
                g.DrawLine(myPen, CSng(X + 0.7 * Width), CSng(Y + 0.5 * Height), CSng(X + 0.95 * Width), CSng(Y + 0.5 * Height))
                g.DrawLine(myPen, CSng(X + 0.7 * Width), CSng(Y + 0.6 * Height), CSng(X + 0.95 * Width), CSng(Y + 0.6 * Height))
                g.DrawLine(myPen, CSng(X + 0.7 * Width), CSng(Y + 0.7 * Height), CSng(X + 0.95 * Width), CSng(Y + 0.7 * Height))
                g.DrawLine(myPen, CSng(X + 0.7 * Width), CSng(Y + 0.8 * Height), CSng(X + 0.95 * Width), CSng(Y + 0.8 * Height))

            Else

                Me.DrawRoundRect(g, myPen, X + 0.05 * 1.25 * Width, Y + 0.2 * Height, 0.2 * 1.25 * Width, 0.7 * Height, 20, Brushes.Transparent)
                Me.DrawRoundRect(g, myPen, X + 0.525 * 1.25 * Width, Y + 0.1 * Height, 0.15 * 1.25 * Width, 0.15 * Height, 20, Brushes.Transparent)
                g.DrawLines(myPen, New PointF() {New PointF(X + 0.175 * Width, Y + 0.2 * Height), New PointF(X + 0.175 * Width, Y + 0.02 * Height), New PointF(X + 0.32 * 1.25 * Width, Y + 0.02 * Height)})
                g.DrawLines(myPen, New PointF() {New PointF(X + 0.43 * 1.25 * Width, Y + 0.02 * Height), New PointF(X + 0.6 * 1.25 * Width, Y + 0.02 * Height), New PointF(X + 0.6 * 1.25 * Width, Y + 0.1 * Height)})
                g.DrawLines(myPen, New PointF() {New PointF(X + 0.175 * Width, Y + 0.9 * Height), New PointF(X + 0.175 * Width, Y + 0.98 * Height), New PointF(X + 0.6 * 1.25 * Width, Y + 0.98 * Height)})

                g.DrawEllipse(myPen, CSng(X + 0.3 * 1.25 * Width), CSng(Y), CSng(0.15 * 1.25 * Width), CSng(0.15 * Height))
                'g.DrawEllipse(myPen, CSng(X + 0.525 * 1.25 * Width), CSng(Y + 0.75 * Height), CSng(0.15 * 1.25 * Width), CSng(0.15 * Height))

                g.DrawLines(myPen, New PointF() {New PointF(X + 0.6 * 1.25 * Width, Y + 0.25 * Height), New PointF(X + 0.6 * 1.25 * Width, Y + 0.3 * Height)})
                'g.DrawLines(myPen, New PointF() {New PointF(X + 0.6 * 1.25 * Width, Y + 0.75 * Height), New PointF(X + 0.6 * 1.25 * Width, Y + 0.7 * Height)})

                g.DrawLine(myPen1, CSng(X + 0.6 * 1.25 * Width), CSng(Y + 0.3 * Height), CSng(X + 0.25 * 1.25 * Width), CSng(Y + 0.3 * Height))
                'g.DrawLine(myPen1, CSng(X + 0.6 * 1.25 * Width), CSng(Y + 0.7 * Height), CSng(X + 0.25 * 1.25 * Width), CSng(Y + 0.7 * Height))

                g.DrawLine(myPen1, CSng(X + 0.6 * 1.25 * Width), CSng(Y + 0.98 * Height), CSng(X + Width), CSng(Y + 0.98 * Height))

                If Me.Shape = 1 Then
                    g.DrawLine(myPen1, CSng(X + 0.6 * 1.25 * Width), CSng(Y + 0.02 * Height), CSng(X + Width), CSng(Y + 0.02 * Height))
                    g.DrawLine(myPen1, CSng(X + 0.6 * 1.25 * Width), CSng(Y + 0.3 * Height), CSng(X + Width), CSng(Y + 0.3 * Height))
                ElseIf Me.Shape = 0 Then
                    g.DrawLine(myPen1, CSng(X + 0.6 * 1.25 * Width), CSng(Y + 0.3 * Height), CSng(X + Width), CSng(Y + 0.3 * Height))
                Else
                    g.DrawLine(myPen1, CSng(X + 0.6 * 1.25 * Width), CSng(Y + 0.02 * Height), CSng(X + Width), CSng(Y + 0.02 * Height))
                End If

                g.DrawLines(myPen, New PointF() {New PointF(X + 0.2 * 1.25 * Width, Y + 0.07 * Height), New PointF(X + 0.3 * 1.25 * Width, Y + 0.07 * Height), New PointF(X + 0.35 * 1.25 * Width, Y + 0.125 * Height), New PointF(X + 0.4 * 1.25 * Width, Y + 0.02 * Height), New PointF(X + 0.45 * 1.25 * Width, Y + 0.08 * Height)})
                g.DrawLine(myPen3, CSng(X + 0.45 * 1.25 * Width), CSng(Y + 0.08 * Height), CSng(X + 0.65 * 1.25 * Width), CSng(Y + 0.08 * Height))
                'g.DrawLine(myPen1, CSng(X + 0.65 * 1.25 * Width), CSng(Y + 0.08 * Height), CSng(X + Width), CSng(Y + 0.08 * Height))

                'g.DrawLines(myPen, New PointF() {New PointF(X + 0.5 * 1.25 * Width, Y + 0.825 * Height), New PointF(X + 0.55 * 1.25 * Width, Y + 0.825 * Height), New PointF(X + 0.575 * 1.25 * Width, Y + 0.875 * Height), New PointF(X + 0.625 * 1.25 * Width, Y + 0.775 * Height), New PointF(X + 0.65 * 1.25 * Width, Y + 0.825 * Height), New PointF(X + Width, Y + 0.825 * Height)})
                'g.DrawLine(myPen1, CSng(X + 0.5 * 1.25 * Width), CSng(Y + 0.825 * Height), CSng(X + 0.4 * 1.25 * Width), CSng(Y + 0.825 * Height))

                g.DrawLine(myPen1, CSng(X), CSng(Y + 0.5 * Height), CSng(X + 0.05 * 1.25 * Width), CSng(Y + 0.5 * Height))

                'g.DrawLine(myPen, CSng(X + 0.05 * 1.25 * Width), CSng(Y + 0.2 * Height), CSng(X + 0.31 * Width), CSng(Y + 0.2 * Height))
                g.DrawLine(myPen, CSng(X + 0.05 * 1.25 * Width), CSng(Y + 0.3 * Height), CSng(X + 0.31 * Width), CSng(Y + 0.3 * Height))
                g.DrawLine(myPen, CSng(X + 0.05 * 1.25 * Width), CSng(Y + 0.4 * Height), CSng(X + 0.31 * Width), CSng(Y + 0.4 * Height))
                g.DrawLine(myPen, CSng(X + 0.05 * 1.25 * Width), CSng(Y + 0.5 * Height), CSng(X + 0.31 * Width), CSng(Y + 0.5 * Height))
                g.DrawLine(myPen, CSng(X + 0.05 * 1.25 * Width), CSng(Y + 0.6 * Height), CSng(X + 0.31 * Width), CSng(Y + 0.6 * Height))
                g.DrawLine(myPen, CSng(X + 0.05 * 1.25 * Width), CSng(Y + 0.7 * Height), CSng(X + 0.31 * Width), CSng(Y + 0.7 * Height))
                g.DrawLine(myPen, CSng(X + 0.05 * 1.25 * Width), CSng(Y + 0.8 * Height), CSng(X + 0.31 * Width), CSng(Y + 0.8 * Height))

            End If

            Me.DrawRoundRect(g, myPen, Me.X, Me.Y, Me.Width, Me.Height, 5, New SolidBrush(Color.Transparent))

            Dim strdist As SizeF = g.MeasureString(Me.Tag, New Font("Arial", 10, FontStyle.Bold, GraphicsUnit.Pixel, 0, False), New PointF(0, 0), New StringFormat(StringFormatFlags.NoClip, 0))
            Dim strx As Single = (Me.Width - strdist.Width) / 2
            'g.FillRectangle(Brushes.White, X + strx, Y + CSng(Height + 5), strdist.Width, strdist.Height)
            g.DrawString(Me.Tag, New Font("Arial", 10, FontStyle.Bold, GraphicsUnit.Pixel, 0, False), New SolidBrush(Me.LineColor), X + strx, Y + Height + 5)

            g.EndContainer(gContainer)

        End Sub

        Public Sub DrawRoundRect(ByVal g As Graphics, ByVal p As Pen, ByVal x As Integer, ByVal y As Integer, ByVal width As Integer, ByVal height As Integer, ByVal radius As Integer, ByVal myBrush As Brush)

            Dim gp As Drawing2D.GraphicsPath = New Drawing2D.GraphicsPath

            gp.AddLine(x + radius, y, x + width - radius, y)
            gp.AddArc(x + width - radius, y, radius, radius, 270, 90)
            gp.AddLine(x + width, y + radius, x + width, y + height - radius)
            gp.AddArc(x + width - radius, y + height - radius, radius, radius, 0, 90)
            gp.AddLine(x + width - radius, y + height, x + radius, y + height)
            gp.AddArc(x, y + height - radius, radius, radius, 90, 90)
            gp.AddLine(x, y + height - radius, x, y + radius)
            gp.AddArc(x, y, radius, radius, 180, 90)

            gp.CloseFigure()

            g.DrawPath(p, gp)
            g.FillPath(myBrush, gp)

            gp.Dispose()

        End Sub

    End Class

    <Serializable()> Public Class ComponentSeparatorGraphic

        Inherits ShapeGraphic

#Region "Constructors"

        Public Sub New()
            Me.TipoObjeto = GraphicObjects.TipoObjeto.ComponentSeparator
            Me.Description = "ComponentSeparator"
            Me.m_mixpoint = False
            Me.m_splitpoint = True
        End Sub

        Public Sub New(ByVal graphicPosition As Point)
            Me.New()
            Me.SetPosition(graphicPosition)
        End Sub

        Public Sub New(ByVal posX As Integer, ByVal posY As Integer)
            Me.New(New Point(posX, posY))
        End Sub

        Public Sub New(ByVal graphicPosition As Point, ByVal graphicSize As Size)
            Me.New(graphicPosition)
            Me.SetSize(graphicSize)
            Me.AutoSize = False
        End Sub

        Public Sub New(ByVal posX As Integer, ByVal posY As Integer, ByVal graphicSize As Size)
            Me.New(New Point(posX, posY), graphicSize)
        End Sub

        Public Sub New(ByVal posX As Integer, ByVal posY As Integer, ByVal width As Integer, ByVal height As Integer)
            Me.New(New Point(posX, posY), New Size(width, height))
        End Sub

        Public Sub New(ByVal graphicPosition As Point, ByVal Rotation As Single)
            Me.New()
            Me.SetPosition(graphicPosition)
            Me.Rotation = Rotation
        End Sub

        Public Sub New(ByVal posX As Integer, ByVal posY As Integer, ByVal Rotation As Single)
            Me.New(New Point(posX, posY), Rotation)
        End Sub

        Public Sub New(ByVal graphicPosition As Point, ByVal graphicSize As Size, ByVal Rotation As Single)
            Me.New(graphicPosition, Rotation)
            Me.SetSize(graphicSize)
            Me.AutoSize = False
        End Sub

        Public Sub New(ByVal posX As Integer, ByVal posY As Integer, ByVal graphicSize As Size, ByVal Rotation As Single)
            Me.New(New Point(posX, posY), graphicSize, Rotation)
        End Sub

        Public Sub New(ByVal posX As Integer, ByVal posY As Integer, ByVal width As Integer, _
                               ByVal height As Integer, ByVal Rotation As Single)
            Me.New(New Point(posX, posY), New Size(width, height), Rotation)
        End Sub

#End Region

        Public Overrides Sub CreateConnectors(InCount As Integer, OutCount As Integer)

            Dim myIC1 As New ConnectionPoint
            myIC1.Position = New Point(X + 0.125 * Width, Y + 0.5 * Height)
            myIC1.Type = ConType.ConIn

            Dim myOC1 As New ConnectionPoint
            myOC1.Position = New Point(X + 0.5 * Width, Y)
            myOC1.Type = ConType.ConOut

            Dim myOC2 As New ConnectionPoint
            myOC2.Position = New Point(X + 0.5 * Width, Y + Height)
            myOC2.Type = ConType.ConOut

            With InputConnectors

                If .Count <> 0 Then
                    If Me.FlippedH Then
                        .Item(0).Position = New Point(X + 0.875 * Width, Y + 0.5 * Height)
                    Else
                        .Item(0).Position = New Point(X + 0.125 * Width, Y + 0.5 * Height)
                    End If
                Else
                    .Add(myIC1)
                End If

            End With

            With OutputConnectors

                If .Count <> 0 Then
                    If Me.FlippedH Then
                        .Item(0).Position = New Point(X + 0.3 * Width, Y + (0.1 + 0.127 / 2) * Height)
                        .Item(1).Position = New Point(X + 0.3 * Width, Y + (0.773 + 0.127 / 2) * Height)
                    Else
                        .Item(0).Position = New Point(X + 0.7 * Width, Y + (0.1 + 0.127 / 2) * Height)
                        .Item(1).Position = New Point(X + 0.7 * Width, Y + (0.773 + 0.127 / 2) * Height)
                    End If
                Else
                    .Add(myOC1)
                    .Add(myOC2)
                End If

            End With

            With Me.EnergyConnector
                If Me.FlippedH Then
                    .Position = New Point(X + 0.125 * Width, Y + 0.5 * Height)
                Else
                    .Position = New Point(X + 0.875 * Width, Y + 0.5 * Height)
                End If
            End With

        End Sub

        Public Overrides Sub Draw(ByVal g As System.Drawing.Graphics)

            CreateConnectors(0, 0)

            UpdateStatus(Me)

            Dim pt As Point
            Dim raio, angulo As Double
            Dim con As ConnectionPoint
            For Each con In Me.InputConnectors
                pt = con.Position
                raio = ((pt.X - Me.X) ^ 2 + (pt.Y - Me.Y) ^ 2) ^ 0.5
                angulo = Math.Atan2(pt.Y - Me.Y, pt.X - Me.X)
                pt.X = Me.X + raio * Math.Cos(angulo + Me.Rotation / 360 * 2 * Math.PI)
                pt.Y = Me.Y + raio * Math.Sin(angulo + Me.Rotation / 360 * 2 * Math.PI)
                con.Position = pt
            Next
            For Each con In Me.OutputConnectors
                pt = con.Position
                raio = ((pt.X - Me.X) ^ 2 + (pt.Y - Me.Y) ^ 2) ^ 0.5
                angulo = Math.Atan2(pt.Y - Me.Y, pt.X - Me.X)
                pt.X = Me.X + raio * Math.Cos(angulo + Me.Rotation / 360 * 2 * Math.PI)
                pt.Y = Me.Y + raio * Math.Sin(angulo + Me.Rotation / 360 * 2 * Math.PI)
                con.Position = pt
            Next
            With Me.EnergyConnector
                pt = .Position
                raio = ((pt.X - Me.X) ^ 2 + (pt.Y - Me.Y) ^ 2) ^ 0.5
                angulo = Math.Atan2(pt.Y - Me.Y, pt.X - Me.X)
                pt.X = Me.X + raio * Math.Cos(angulo + Me.Rotation / 360 * 2 * Math.PI)
                pt.Y = Me.Y + raio * Math.Sin(angulo + Me.Rotation / 360 * 2 * Math.PI)
                .Position = pt
            End With

            Dim gContainer As System.Drawing.Drawing2D.GraphicsContainer
            Dim myMatrix As Drawing2D.Matrix
            gContainer = g.BeginContainer()
            myMatrix = g.Transform()
            If m_Rotation <> 0 Then
                myMatrix.RotateAt(m_Rotation, New PointF(X, Y), Drawing.Drawing2D.MatrixOrder.Append)
                g.Transform = myMatrix
            End If

            Dim rect2 As New Rectangle(X + 0.123 * Width, Y + 0.5 * Height, 0.127 * Width, 0.127 * Height)
            Dim rect3 As New Rectangle(X + 0.7 * Width, Y + 0.1 * Height, 0.127 * Width, 0.127 * Height)
            Dim rect4 As New Rectangle(X + 0.7 * Width, Y + 0.773 * Height, 0.127 * Width, 0.127 * Height)
            If Me.FlippedH = True Then
                rect2 = New Rectangle(X + (1 - 0.123) * Width, Y + 0.5 * Height, 0.127 * Width, 0.127 * Height)
                rect3 = New Rectangle(X + 0.3 * Width, Y + 0.1 * Height, 0.127 * Width, 0.127 * Height)
                rect4 = New Rectangle(X + 0.3 * Width, Y + 0.773 * Height, 0.127 * Width, 0.127 * Height)
            End If

            Dim myPen As New Pen(Me.LineColor, Me.LineWidth)

            Dim myPen2 As New Pen(Color.White, 0)

            Dim rect As New Rectangle(X, Y, Width, Height)

            g.SmoothingMode = SmoothingMode.AntiAlias
            'g.DrawRectangle(myPen2, rect)
            If Me.FlippedH = True Then
                Me.DrawRoundRect(g, myPen, X + 0.4 * Width, Y, 0.45 * Width, Height, 10, Brushes.Transparent)
            Else
                Me.DrawRoundRect(g, myPen, X + 0.25 * Width, Y, 0.45 * Width, Height, 10, Brushes.Transparent)
            End If
            g.DrawRectangle(myPen, rect2)
            g.DrawRectangle(myPen, rect3)
            g.DrawRectangle(myPen, rect4)

            Dim strdist As SizeF = g.MeasureString(Me.Tag, New Font("Arial", 10, FontStyle.Bold, GraphicsUnit.Pixel, 0, False), New PointF(0, 0), New StringFormat(StringFormatFlags.NoClip, 0))
            Dim strx As Single = (Me.Width - strdist.Width) / 2
            'g.FillRectangle(Brushes.White, X + strx, Y + CSng(Height + 5), strdist.Width, strdist.Height)
            g.DrawString(Me.Tag, New Font("Arial", 10, FontStyle.Bold, GraphicsUnit.Pixel, 0, False), New SolidBrush(Me.LineColor), X + strx, Y + Height + 5)

            Dim gp As Drawing2D.GraphicsPath = New Drawing2D.GraphicsPath
            Dim radius As Integer = 3
            gp.AddLine(X + radius, Y, X + Width - radius, Y)
            gp.AddArc(X + Width - radius, Y, radius, radius, 270, 90)
            gp.AddLine(X + Width, Y + radius, X + Width, Y + Height - radius)
            gp.AddArc(X + Width - radius, Y + Height - radius, radius, radius, 0, 90)
            gp.AddLine(X + Width - radius, Y + Height, X + radius, Y + Height)
            gp.AddArc(X, Y + Height - radius, radius, radius, 90, 90)
            gp.AddLine(X, Y + Height - radius, X, Y + radius)
            gp.AddArc(X, Y, radius, radius, 180, 90)
            Dim lgb1 As LinearGradientBrush
            lgb1 = New LinearGradientBrush(rect, Me.GradientColor1, Me.GradientColor2, LinearGradientMode.Horizontal)
            lgb1.SetBlendTriangularShape(0.5)
            'lgb1.CenterColor = Me.GradientColor1
            'lgb1.SetBlendTriangularShape(0.5)
            'lgb1.SurroundColors = New Color() {Me.GradientColor2}
            'lgb1.WrapMode = WrapMode.Tile
            If Me.Fill Then
                If Me.GradientMode = False Then
                    g.FillRectangle(New SolidBrush(Me.FillColor), rect3)
                    g.FillRectangle(New SolidBrush(Me.FillColor), rect4)
                    g.FillRectangle(New SolidBrush(Me.FillColor), rect2)
                    If Me.FlippedH = True Then
                        Me.DrawRoundRect(g, myPen, X + 0.4 * Width, Y, 0.45 * Width, Height, 6, New SolidBrush(Me.FillColor))
                    Else
                        Me.DrawRoundRect(g, myPen, X + 0.25 * Width, Y, 0.45 * Width, Height, 6, New SolidBrush(Me.FillColor))
                    End If
                Else
                    g.FillRectangle(New SolidBrush(Me.FillColor), rect3)
                    g.FillRectangle(New SolidBrush(Me.FillColor), rect4)
                    g.FillRectangle(New SolidBrush(Me.FillColor), rect2)
                    If Me.FlippedH = True Then
                        Me.DrawRoundRect(g, myPen, X + 0.4 * Width, Y, 0.45 * Width, Height, 6, lgb1)
                    Else
                        Me.DrawRoundRect(g, myPen, X + 0.25 * Width, Y, 0.45 * Width, Height, 6, lgb1)
                    End If
                End If
            End If

            If Me.FlippedH Then
                g.DrawLines(myPen, New PointF() {New PointF(X + 0.4 * Width, Y + 0.3 * Height), New PointF(X + 0.85 * Width, Me.Y + 0.3 * Height)})
                g.DrawLines(myPen, New PointF() {New PointF(X + 0.4 * Width, Y + 0.7 * Height), New PointF(X + 0.85 * Width, Me.Y + 0.7 * Height)})
                g.DrawLines(myPen, New PointF() {New PointF(X + 0.4 * Width, Y + 0.3 * Height), New PointF(X + 0.85 * Width, Me.Y + 0.7 * Height)})
                g.DrawLines(myPen, New PointF() {New PointF(X + 0.4 * Width, Y + 0.7 * Height), New PointF(X + 0.85 * Width, Me.Y + 0.3 * Height)})
            Else
                g.DrawLines(myPen, New PointF() {New PointF(X + 0.25 * Width, Y + 0.3 * Height), New PointF(X + 0.7 * Width, Me.Y + 0.3 * Height)})
                g.DrawLines(myPen, New PointF() {New PointF(X + 0.25 * Width, Y + 0.7 * Height), New PointF(X + 0.7 * Width, Me.Y + 0.7 * Height)})
                g.DrawLines(myPen, New PointF() {New PointF(X + 0.25 * Width, Y + 0.3 * Height), New PointF(X + 0.7 * Width, Me.Y + 0.7 * Height)})
                g.DrawLines(myPen, New PointF() {New PointF(X + 0.25 * Width, Y + 0.7 * Height), New PointF(X + 0.7 * Width, Me.Y + 0.3 * Height)})
            End If

            Dim size As SizeF
            Dim fontA As New Font("Arial", 8, FontStyle.Bold, GraphicsUnit.Pixel, 0, False)
            size = g.MeasureString("CS", fontA)

            Dim ax, ay As Integer
            If Me.FlippedH Then
                ax = Me.X + (Me.Width - size.Width) / 2
                ay = Me.Y + Me.Height - size.Height
            Else
                ax = Me.X + (Me.Width - size.Width) / 2
                ay = Me.Y + Me.Height - size.Height
            End If

            g.SmoothingMode = SmoothingMode.AntiAlias
            g.TextRenderingHint = Text.TextRenderingHint.AntiAlias
            g.DrawString("CS", fontA, New SolidBrush(Me.LineColor), ax, ay)

            g.EndContainer(gContainer)
            gp.Dispose()

        End Sub

        Public Sub DrawRoundRect(ByVal g As Graphics, ByVal p As Pen, ByVal x As Integer, ByVal y As Integer, ByVal width As Integer, ByVal height As Integer, ByVal radius As Integer, ByVal myBrush As Brush)

            Dim gp As Drawing2D.GraphicsPath = New Drawing2D.GraphicsPath

            gp.AddLine(x + radius, y, x + width - radius, y)
            gp.AddArc(x + width - radius, y, radius, radius, 270, 90)
            gp.AddLine(x + width, y + radius, x + width, y + height - radius)
            gp.AddArc(x + width - radius, y + height - radius, radius, radius, 0, 90)
            gp.AddLine(x + width - radius, y + height, x + radius, y + height)
            gp.AddArc(x, y + height - radius, radius, radius, 90, 90)
            gp.AddLine(x, y + height - radius, x, y + radius)
            gp.AddArc(x, y, radius, radius, 180, 90)

            gp.CloseFigure()

            g.DrawPath(p, gp)
            g.FillPath(myBrush, gp)

            gp.Dispose()

        End Sub

    End Class

    <Serializable()> Public Class SolidSeparatorGraphic

        Inherits ShapeGraphic

#Region "Constructors"

        Public Sub New()
            Me.TipoObjeto = GraphicObjects.TipoObjeto.SolidSeparator
            Me.Description = "SolidSeparator"
            Me.m_mixpoint = False
            Me.m_splitpoint = True
        End Sub

        Public Sub New(ByVal graphicPosition As Point)
            Me.New()
            Me.SetPosition(graphicPosition)
        End Sub

        Public Sub New(ByVal posX As Integer, ByVal posY As Integer)
            Me.New(New Point(posX, posY))
        End Sub

        Public Sub New(ByVal graphicPosition As Point, ByVal graphicSize As Size)
            Me.New(graphicPosition)
            Me.SetSize(graphicSize)
            Me.AutoSize = False
        End Sub

        Public Sub New(ByVal posX As Integer, ByVal posY As Integer, ByVal graphicSize As Size)
            Me.New(New Point(posX, posY), graphicSize)
        End Sub

        Public Sub New(ByVal posX As Integer, ByVal posY As Integer, ByVal width As Integer, ByVal height As Integer)
            Me.New(New Point(posX, posY), New Size(width, height))
        End Sub

        Public Sub New(ByVal graphicPosition As Point, ByVal Rotation As Single)
            Me.New()
            Me.SetPosition(graphicPosition)
            Me.Rotation = Rotation
        End Sub

        Public Sub New(ByVal posX As Integer, ByVal posY As Integer, ByVal Rotation As Single)
            Me.New(New Point(posX, posY), Rotation)
        End Sub

        Public Sub New(ByVal graphicPosition As Point, ByVal graphicSize As Size, ByVal Rotation As Single)
            Me.New(graphicPosition, Rotation)
            Me.SetSize(graphicSize)
            Me.AutoSize = False
        End Sub

        Public Sub New(ByVal posX As Integer, ByVal posY As Integer, ByVal graphicSize As Size, ByVal Rotation As Single)
            Me.New(New Point(posX, posY), graphicSize, Rotation)
        End Sub

        Public Sub New(ByVal posX As Integer, ByVal posY As Integer, ByVal width As Integer, _
                               ByVal height As Integer, ByVal Rotation As Single)
            Me.New(New Point(posX, posY), New Size(width, height), Rotation)
        End Sub

#End Region

        Public Overrides Sub CreateConnectors(InCount As Integer, OutCount As Integer)

            Dim myIC1 As New ConnectionPoint
            myIC1.Position = New Point(X + 0.125 * Width, Y + 0.5 * Height)
            myIC1.Type = ConType.ConIn

            Dim myOC1 As New ConnectionPoint
            myOC1.Position = New Point(X + 0.5 * Width, Y)
            myOC1.Type = ConType.ConOut

            Dim myOC2 As New ConnectionPoint
            myOC2.Position = New Point(X + 0.5 * Width, Y + Height)
            myOC2.Type = ConType.ConOut

            With InputConnectors

                If .Count <> 0 Then
                    If Me.FlippedH Then
                        .Item(0).Position = New Point(X + 0.875 * Width, Y + 0.5 * Height)
                    Else
                        .Item(0).Position = New Point(X + 0.125 * Width, Y + 0.5 * Height)
                    End If
                Else
                    .Add(myIC1)
                End If

            End With

            With OutputConnectors

                If .Count <> 0 Then
                    If Me.FlippedH Then
                        .Item(0).Position = New Point(X + 0.3 * Width, Y + (0.1 + 0.127 / 2) * Height)
                        .Item(1).Position = New Point(X + 0.3 * Width, Y + (0.773 + 0.127 / 2) * Height)
                    Else
                        .Item(0).Position = New Point(X + 0.7 * Width, Y + (0.1 + 0.127 / 2) * Height)
                        .Item(1).Position = New Point(X + 0.7 * Width, Y + (0.773 + 0.127 / 2) * Height)
                    End If
                Else
                    .Add(myOC1)
                    .Add(myOC2)
                End If

            End With

            With Me.EnergyConnector
                If Me.FlippedH Then
                    .Position = New Point(X + 0.125 * Width, Y + 0.5 * Height)
                Else
                    .Position = New Point(X + 0.875 * Width, Y + 0.5 * Height)
                End If
            End With

        End Sub

        Public Overrides Sub Draw(ByVal g As System.Drawing.Graphics)

            CreateConnectors(0, 0)

            UpdateStatus(Me)

            Dim pt As Point
            Dim raio, angulo As Double
            Dim con As ConnectionPoint
            For Each con In Me.InputConnectors
                pt = con.Position
                raio = ((pt.X - Me.X) ^ 2 + (pt.Y - Me.Y) ^ 2) ^ 0.5
                angulo = Math.Atan2(pt.Y - Me.Y, pt.X - Me.X)
                pt.X = Me.X + raio * Math.Cos(angulo + Me.Rotation / 360 * 2 * Math.PI)
                pt.Y = Me.Y + raio * Math.Sin(angulo + Me.Rotation / 360 * 2 * Math.PI)
                con.Position = pt
            Next
            For Each con In Me.OutputConnectors
                pt = con.Position
                raio = ((pt.X - Me.X) ^ 2 + (pt.Y - Me.Y) ^ 2) ^ 0.5
                angulo = Math.Atan2(pt.Y - Me.Y, pt.X - Me.X)
                pt.X = Me.X + raio * Math.Cos(angulo + Me.Rotation / 360 * 2 * Math.PI)
                pt.Y = Me.Y + raio * Math.Sin(angulo + Me.Rotation / 360 * 2 * Math.PI)
                con.Position = pt
            Next
            With Me.EnergyConnector
                pt = .Position
                raio = ((pt.X - Me.X) ^ 2 + (pt.Y - Me.Y) ^ 2) ^ 0.5
                angulo = Math.Atan2(pt.Y - Me.Y, pt.X - Me.X)
                pt.X = Me.X + raio * Math.Cos(angulo + Me.Rotation / 360 * 2 * Math.PI)
                pt.Y = Me.Y + raio * Math.Sin(angulo + Me.Rotation / 360 * 2 * Math.PI)
                .Position = pt
            End With

            Dim gContainer As System.Drawing.Drawing2D.GraphicsContainer
            Dim myMatrix As Drawing2D.Matrix
            gContainer = g.BeginContainer()
            myMatrix = g.Transform()
            If m_Rotation <> 0 Then
                myMatrix.RotateAt(m_Rotation, New PointF(X, Y), Drawing.Drawing2D.MatrixOrder.Append)
                g.Transform = myMatrix
            End If

            Dim rect2 As New Rectangle(X + 0.123 * Width, Y + 0.5 * Height, 0.127 * Width, 0.127 * Height)
            Dim rect3 As New Rectangle(X + 0.7 * Width, Y + 0.1 * Height, 0.127 * Width, 0.127 * Height)
            Dim rect4 As New Rectangle(X + 0.7 * Width, Y + 0.773 * Height, 0.127 * Width, 0.127 * Height)
            If Me.FlippedH = True Then
                rect2 = New Rectangle(X + (1 - 0.123) * Width, Y + 0.5 * Height, 0.127 * Width, 0.127 * Height)
                rect3 = New Rectangle(X + 0.3 * Width, Y + 0.1 * Height, 0.127 * Width, 0.127 * Height)
                rect4 = New Rectangle(X + 0.3 * Width, Y + 0.773 * Height, 0.127 * Width, 0.127 * Height)
            End If

            Dim myPen As New Pen(Me.LineColor, Me.LineWidth)

            Dim myPen2 As New Pen(Color.White, 0)

            Dim rect As New Rectangle(X, Y, Width, Height)

            g.SmoothingMode = SmoothingMode.AntiAlias
            'g.DrawRectangle(myPen2, rect)
            If Me.FlippedH = True Then
                Me.DrawRoundRect(g, myPen, X + 0.4 * Width, Y, 0.45 * Width, Height, 10, Brushes.Transparent)
            Else
                Me.DrawRoundRect(g, myPen, X + 0.25 * Width, Y, 0.45 * Width, Height, 10, Brushes.Transparent)
            End If
            g.DrawRectangle(myPen, rect2)
            g.DrawRectangle(myPen, rect3)
            g.DrawRectangle(myPen, rect4)

            Dim strdist As SizeF = g.MeasureString(Me.Tag, New Font("Arial", 10, FontStyle.Bold, GraphicsUnit.Pixel, 0, False), New PointF(0, 0), New StringFormat(StringFormatFlags.NoClip, 0))
            Dim strx As Single = (Me.Width - strdist.Width) / 2
            'g.FillRectangle(Brushes.White, X + strx, Y + CSng(Height + 5), strdist.Width, strdist.Height)
            g.DrawString(Me.Tag, New Font("Arial", 10, FontStyle.Bold, GraphicsUnit.Pixel, 0, False), New SolidBrush(Me.LineColor), X + strx, Y + Height + 5)

            Dim gp As Drawing2D.GraphicsPath = New Drawing2D.GraphicsPath
            Dim radius As Integer = 3
            gp.AddLine(X + radius, Y, X + Width - radius, Y)
            gp.AddArc(X + Width - radius, Y, radius, radius, 270, 90)
            gp.AddLine(X + Width, Y + radius, X + Width, Y + Height - radius)
            gp.AddArc(X + Width - radius, Y + Height - radius, radius, radius, 0, 90)
            gp.AddLine(X + Width - radius, Y + Height, X + radius, Y + Height)
            gp.AddArc(X, Y + Height - radius, radius, radius, 90, 90)
            gp.AddLine(X, Y + Height - radius, X, Y + radius)
            gp.AddArc(X, Y, radius, radius, 180, 90)
            Dim lgb1 As LinearGradientBrush
            lgb1 = New LinearGradientBrush(rect, Me.GradientColor1, Me.GradientColor2, LinearGradientMode.Horizontal)
            lgb1.SetBlendTriangularShape(0.5)
            'lgb1.CenterColor = Me.GradientColor1
            'lgb1.SetBlendTriangularShape(0.5)
            'lgb1.SurroundColors = New Color() {Me.GradientColor2}
            'lgb1.WrapMode = WrapMode.Tile
            If Me.Fill Then
                If Me.GradientMode = False Then
                    g.FillRectangle(New SolidBrush(Me.FillColor), rect3)
                    g.FillRectangle(New SolidBrush(Me.FillColor), rect4)
                    g.FillRectangle(New SolidBrush(Me.FillColor), rect2)
                    If Me.FlippedH = True Then
                        Me.DrawRoundRect(g, myPen, X + 0.4 * Width, Y, 0.45 * Width, Height, 6, New SolidBrush(Me.FillColor))
                    Else
                        Me.DrawRoundRect(g, myPen, X + 0.25 * Width, Y, 0.45 * Width, Height, 6, New SolidBrush(Me.FillColor))
                    End If
                Else
                    g.FillRectangle(New SolidBrush(Me.FillColor), rect3)
                    g.FillRectangle(New SolidBrush(Me.FillColor), rect4)
                    g.FillRectangle(New SolidBrush(Me.FillColor), rect2)
                    If Me.FlippedH = True Then
                        Me.DrawRoundRect(g, myPen, X + 0.4 * Width, Y, 0.45 * Width, Height, 6, lgb1)
                    Else
                        Me.DrawRoundRect(g, myPen, X + 0.25 * Width, Y, 0.45 * Width, Height, 6, lgb1)
                    End If
                End If
            End If

            If Me.FlippedH Then
                g.DrawLines(myPen, New PointF() {New PointF(X + 0.4 * Width, Y + 0.3 * Height), New PointF(X + 0.85 * Width, Me.Y + 0.3 * Height)})
                g.DrawLines(myPen, New PointF() {New PointF(X + 0.4 * Width, Y + 0.7 * Height), New PointF(X + 0.85 * Width, Me.Y + 0.7 * Height)})
                g.DrawLines(myPen, New PointF() {New PointF(X + 0.4 * Width, Y + 0.3 * Height), New PointF(X + 0.85 * Width, Me.Y + 0.7 * Height)})
                g.DrawLines(myPen, New PointF() {New PointF(X + 0.4 * Width, Y + 0.7 * Height), New PointF(X + 0.85 * Width, Me.Y + 0.3 * Height)})
            Else
                g.DrawLines(myPen, New PointF() {New PointF(X + 0.25 * Width, Y + 0.3 * Height), New PointF(X + 0.7 * Width, Me.Y + 0.3 * Height)})
                g.DrawLines(myPen, New PointF() {New PointF(X + 0.25 * Width, Y + 0.7 * Height), New PointF(X + 0.7 * Width, Me.Y + 0.7 * Height)})
                g.DrawLines(myPen, New PointF() {New PointF(X + 0.25 * Width, Y + 0.3 * Height), New PointF(X + 0.7 * Width, Me.Y + 0.7 * Height)})
                g.DrawLines(myPen, New PointF() {New PointF(X + 0.25 * Width, Y + 0.7 * Height), New PointF(X + 0.7 * Width, Me.Y + 0.3 * Height)})
            End If

            Dim size As SizeF
            Dim fontA As New Font("Arial", 8, FontStyle.Bold, GraphicsUnit.Pixel, 0, False)
            size = g.MeasureString("SS", fontA)

            Dim ax, ay As Integer
            If Me.FlippedH Then
                ax = Me.X + (Me.Width - size.Width) / 2
                ay = Me.Y + Me.Height - size.Height
            Else
                ax = Me.X + (Me.Width - size.Width) / 2
                ay = Me.Y + Me.Height - size.Height
            End If

            g.SmoothingMode = SmoothingMode.AntiAlias
            g.TextRenderingHint = Text.TextRenderingHint.AntiAlias
            g.DrawString("SS", fontA, New SolidBrush(Me.LineColor), ax, ay)

            g.EndContainer(gContainer)
            gp.Dispose()

        End Sub

        Public Sub DrawRoundRect(ByVal g As Graphics, ByVal p As Pen, ByVal x As Integer, ByVal y As Integer, ByVal width As Integer, ByVal height As Integer, ByVal radius As Integer, ByVal myBrush As Brush)

            Dim gp As Drawing2D.GraphicsPath = New Drawing2D.GraphicsPath

            gp.AddLine(x + radius, y, x + width - radius, y)
            gp.AddArc(x + width - radius, y, radius, radius, 270, 90)
            gp.AddLine(x + width, y + radius, x + width, y + height - radius)
            gp.AddArc(x + width - radius, y + height - radius, radius, radius, 0, 90)
            gp.AddLine(x + width - radius, y + height, x + radius, y + height)
            gp.AddArc(x, y + height - radius, radius, radius, 90, 90)
            gp.AddLine(x, y + height - radius, x, y + radius)
            gp.AddArc(x, y, radius, radius, 180, 90)

            gp.CloseFigure()

            g.DrawPath(p, gp)
            g.FillPath(myBrush, gp)

            gp.Dispose()

        End Sub

    End Class

    <Serializable()> Public Class FilterGraphic

        Inherits ShapeGraphic

#Region "Constructors"

        Public Sub New()
            Me.TipoObjeto = GraphicObjects.TipoObjeto.Filter
            Me.Description = "Filter"
            Me.m_mixpoint = False
            Me.m_splitpoint = True
        End Sub

        Public Sub New(ByVal graphicPosition As Point)
            Me.New()
            Me.SetPosition(graphicPosition)
        End Sub

        Public Sub New(ByVal posX As Integer, ByVal posY As Integer)
            Me.New(New Point(posX, posY))
        End Sub

        Public Sub New(ByVal graphicPosition As Point, ByVal graphicSize As Size)
            Me.New(graphicPosition)
            Me.SetSize(graphicSize)
            Me.AutoSize = False
        End Sub

        Public Sub New(ByVal posX As Integer, ByVal posY As Integer, ByVal graphicSize As Size)
            Me.New(New Point(posX, posY), graphicSize)
        End Sub

        Public Sub New(ByVal posX As Integer, ByVal posY As Integer, ByVal width As Integer, ByVal height As Integer)
            Me.New(New Point(posX, posY), New Size(width, height))
        End Sub

        Public Sub New(ByVal graphicPosition As Point, ByVal Rotation As Single)
            Me.New()
            Me.SetPosition(graphicPosition)
            Me.Rotation = Rotation
        End Sub

        Public Sub New(ByVal posX As Integer, ByVal posY As Integer, ByVal Rotation As Single)
            Me.New(New Point(posX, posY), Rotation)
        End Sub

        Public Sub New(ByVal graphicPosition As Point, ByVal graphicSize As Size, ByVal Rotation As Single)
            Me.New(graphicPosition, Rotation)
            Me.SetSize(graphicSize)
            Me.AutoSize = False
        End Sub

        Public Sub New(ByVal posX As Integer, ByVal posY As Integer, ByVal graphicSize As Size, ByVal Rotation As Single)
            Me.New(New Point(posX, posY), graphicSize, Rotation)
        End Sub

        Public Sub New(ByVal posX As Integer, ByVal posY As Integer, ByVal width As Integer, _
                               ByVal height As Integer, ByVal Rotation As Single)
            Me.New(New Point(posX, posY), New Size(width, height), Rotation)
        End Sub

#End Region

        Public Overrides Sub CreateConnectors(InCount As Integer, OutCount As Integer)

            Dim myIC1 As New ConnectionPoint
            myIC1.Position = New Point(X + 0.125 * Width, Y + 0.5 * Height)
            myIC1.Type = ConType.ConIn

            Dim myOC1 As New ConnectionPoint
            myOC1.Position = New Point(X + 0.5 * Width, Y)
            myOC1.Type = ConType.ConOut

            Dim myOC2 As New ConnectionPoint
            myOC2.Position = New Point(X + 0.5 * Width, Y + Height)
            myOC2.Type = ConType.ConOut

            With InputConnectors

                If .Count <> 0 Then
                    If Me.FlippedH Then
                        .Item(0).Position = New Point(X + 0.875 * Width, Y + 0.5 * Height)
                    Else
                        .Item(0).Position = New Point(X + 0.125 * Width, Y + 0.5 * Height)
                    End If
                Else
                    .Add(myIC1)
                End If

            End With

            With OutputConnectors

                If .Count <> 0 Then
                    If Me.FlippedH Then
                        .Item(0).Position = New Point(X + 0.3 * Width, Y + (0.1 + 0.127 / 2) * Height)
                        .Item(1).Position = New Point(X + 0.3 * Width, Y + (0.773 + 0.127 / 2) * Height)
                    Else
                        .Item(0).Position = New Point(X + 0.7 * Width, Y + (0.1 + 0.127 / 2) * Height)
                        .Item(1).Position = New Point(X + 0.7 * Width, Y + (0.773 + 0.127 / 2) * Height)
                    End If
                Else
                    .Add(myOC1)
                    .Add(myOC2)
                End If

            End With

            With Me.EnergyConnector
                If Me.FlippedH Then
                    .Position = New Point(X + 0.125 * Width, Y + 0.5 * Height)
                Else
                    .Position = New Point(X + 0.875 * Width, Y + 0.5 * Height)
                End If
            End With

        End Sub

        Public Overrides Sub Draw(ByVal g As System.Drawing.Graphics)

            CreateConnectors(0, 0)

            UpdateStatus(Me)

            Dim pt As Point
            Dim raio, angulo As Double
            Dim con As ConnectionPoint
            For Each con In Me.InputConnectors
                pt = con.Position
                raio = ((pt.X - Me.X) ^ 2 + (pt.Y - Me.Y) ^ 2) ^ 0.5
                angulo = Math.Atan2(pt.Y - Me.Y, pt.X - Me.X)
                pt.X = Me.X + raio * Math.Cos(angulo + Me.Rotation / 360 * 2 * Math.PI)
                pt.Y = Me.Y + raio * Math.Sin(angulo + Me.Rotation / 360 * 2 * Math.PI)
                con.Position = pt
            Next
            For Each con In Me.OutputConnectors
                pt = con.Position
                raio = ((pt.X - Me.X) ^ 2 + (pt.Y - Me.Y) ^ 2) ^ 0.5
                angulo = Math.Atan2(pt.Y - Me.Y, pt.X - Me.X)
                pt.X = Me.X + raio * Math.Cos(angulo + Me.Rotation / 360 * 2 * Math.PI)
                pt.Y = Me.Y + raio * Math.Sin(angulo + Me.Rotation / 360 * 2 * Math.PI)
                con.Position = pt
            Next
            With Me.EnergyConnector
                pt = .Position
                raio = ((pt.X - Me.X) ^ 2 + (pt.Y - Me.Y) ^ 2) ^ 0.5
                angulo = Math.Atan2(pt.Y - Me.Y, pt.X - Me.X)
                pt.X = Me.X + raio * Math.Cos(angulo + Me.Rotation / 360 * 2 * Math.PI)
                pt.Y = Me.Y + raio * Math.Sin(angulo + Me.Rotation / 360 * 2 * Math.PI)
                .Position = pt
            End With

            Dim gContainer As System.Drawing.Drawing2D.GraphicsContainer
            Dim myMatrix As Drawing2D.Matrix
            gContainer = g.BeginContainer()
            myMatrix = g.Transform()
            If m_Rotation <> 0 Then
                myMatrix.RotateAt(m_Rotation, New PointF(X, Y), Drawing.Drawing2D.MatrixOrder.Append)
                g.Transform = myMatrix
            End If

            Dim rect2 As New Rectangle(X + 0.123 * Width, Y + 0.5 * Height, 0.127 * Width, 0.127 * Height)
            Dim rect3 As New Rectangle(X + 0.7 * Width, Y + 0.1 * Height, 0.127 * Width, 0.127 * Height)
            Dim rect4 As New Rectangle(X + 0.7 * Width, Y + 0.773 * Height, 0.127 * Width, 0.127 * Height)
            If Me.FlippedH = True Then
                rect2 = New Rectangle(X + (1 - 0.123) * Width, Y + 0.5 * Height, 0.127 * Width, 0.127 * Height)
                rect3 = New Rectangle(X + 0.3 * Width, Y + 0.1 * Height, 0.127 * Width, 0.127 * Height)
                rect4 = New Rectangle(X + 0.3 * Width, Y + 0.773 * Height, 0.127 * Width, 0.127 * Height)
            End If

            Dim myPen As New Pen(Me.LineColor, Me.LineWidth)

            Dim myPen2 As New Pen(Color.White, 0)

            Dim rect As New Rectangle(X, Y, Width, Height)

            g.SmoothingMode = SmoothingMode.AntiAlias
            'g.DrawRectangle(myPen2, rect)
            If Me.FlippedH = True Then
                Me.DrawRoundRect(g, myPen, X + 0.4 * Width, Y, 0.45 * Width, Height, 10, Brushes.Transparent)
            Else
                Me.DrawRoundRect(g, myPen, X + 0.25 * Width, Y, 0.45 * Width, Height, 10, Brushes.Transparent)
            End If
            g.DrawRectangle(myPen, rect2)
            g.DrawRectangle(myPen, rect3)
            g.DrawRectangle(myPen, rect4)

            Dim strdist As SizeF = g.MeasureString(Me.Tag, New Font("Arial", 10, FontStyle.Bold, GraphicsUnit.Pixel, 0, False), New PointF(0, 0), New StringFormat(StringFormatFlags.NoClip, 0))
            Dim strx As Single = (Me.Width - strdist.Width) / 2
            'g.FillRectangle(Brushes.White, X + strx, Y + CSng(Height + 5), strdist.Width, strdist.Height)
            g.DrawString(Me.Tag, New Font("Arial", 10, FontStyle.Bold, GraphicsUnit.Pixel, 0, False), New SolidBrush(Me.LineColor), X + strx, Y + Height + 5)

            Dim gp As Drawing2D.GraphicsPath = New Drawing2D.GraphicsPath
            Dim radius As Integer = 3
            gp.AddLine(X + radius, Y, X + Width - radius, Y)
            gp.AddArc(X + Width - radius, Y, radius, radius, 270, 90)
            gp.AddLine(X + Width, Y + radius, X + Width, Y + Height - radius)
            gp.AddArc(X + Width - radius, Y + Height - radius, radius, radius, 0, 90)
            gp.AddLine(X + Width - radius, Y + Height, X + radius, Y + Height)
            gp.AddArc(X, Y + Height - radius, radius, radius, 90, 90)
            gp.AddLine(X, Y + Height - radius, X, Y + radius)
            gp.AddArc(X, Y, radius, radius, 180, 90)
            Dim lgb1 As LinearGradientBrush
            lgb1 = New LinearGradientBrush(rect, Me.GradientColor1, Me.GradientColor2, LinearGradientMode.Horizontal)
            lgb1.SetBlendTriangularShape(0.5)
            'lgb1.CenterColor = Me.GradientColor1
            'lgb1.SetBlendTriangularShape(0.5)
            'lgb1.SurroundColors = New Color() {Me.GradientColor2}
            'lgb1.WrapMode = WrapMode.Tile
            If Me.Fill Then
                If Me.GradientMode = False Then
                    g.FillRectangle(New SolidBrush(Me.FillColor), rect3)
                    g.FillRectangle(New SolidBrush(Me.FillColor), rect4)
                    g.FillRectangle(New SolidBrush(Me.FillColor), rect2)
                    If Me.FlippedH = True Then
                        Me.DrawRoundRect(g, myPen, X + 0.4 * Width, Y, 0.45 * Width, Height, 6, New SolidBrush(Me.FillColor))
                    Else
                        Me.DrawRoundRect(g, myPen, X + 0.25 * Width, Y, 0.45 * Width, Height, 6, New SolidBrush(Me.FillColor))
                    End If
                Else
                    g.FillRectangle(New SolidBrush(Me.FillColor), rect3)
                    g.FillRectangle(New SolidBrush(Me.FillColor), rect4)
                    g.FillRectangle(New SolidBrush(Me.FillColor), rect2)
                    If Me.FlippedH = True Then
                        Me.DrawRoundRect(g, myPen, X + 0.4 * Width, Y, 0.45 * Width, Height, 6, lgb1)
                    Else
                        Me.DrawRoundRect(g, myPen, X + 0.25 * Width, Y, 0.45 * Width, Height, 6, lgb1)
                    End If
                End If
            End If

            If Me.FlippedH Then
                g.DrawLines(myPen, New PointF() {New PointF(X + 0.4 * Width, Y + 0.3 * Height), New PointF(X + 0.85 * Width, Me.Y + 0.3 * Height)})
                g.DrawLines(myPen, New PointF() {New PointF(X + 0.4 * Width, Y + 0.7 * Height), New PointF(X + 0.85 * Width, Me.Y + 0.7 * Height)})
                g.DrawLines(myPen, New PointF() {New PointF(X + 0.4 * Width, Y + 0.3 * Height), New PointF(X + 0.85 * Width, Me.Y + 0.7 * Height)})
                g.DrawLines(myPen, New PointF() {New PointF(X + 0.4 * Width, Y + 0.7 * Height), New PointF(X + 0.85 * Width, Me.Y + 0.3 * Height)})
            Else
                g.DrawLines(myPen, New PointF() {New PointF(X + 0.25 * Width, Y + 0.3 * Height), New PointF(X + 0.7 * Width, Me.Y + 0.3 * Height)})
                g.DrawLines(myPen, New PointF() {New PointF(X + 0.25 * Width, Y + 0.7 * Height), New PointF(X + 0.7 * Width, Me.Y + 0.7 * Height)})
                g.DrawLines(myPen, New PointF() {New PointF(X + 0.25 * Width, Y + 0.3 * Height), New PointF(X + 0.7 * Width, Me.Y + 0.7 * Height)})
                g.DrawLines(myPen, New PointF() {New PointF(X + 0.25 * Width, Y + 0.7 * Height), New PointF(X + 0.7 * Width, Me.Y + 0.3 * Height)})
            End If

            Dim size As SizeF
            Dim fontA As New Font("Arial", 8, FontStyle.Bold, GraphicsUnit.Pixel, 0, False)
            size = g.MeasureString("F", fontA)

            Dim ax, ay As Integer
            If Me.FlippedH Then
                ax = Me.X + (Me.Width - size.Width) / 2
                ay = Me.Y + Me.Height - size.Height
            Else
                ax = Me.X + (Me.Width - size.Width) / 2
                ay = Me.Y + Me.Height - size.Height
            End If

            g.SmoothingMode = SmoothingMode.AntiAlias
            g.TextRenderingHint = Text.TextRenderingHint.AntiAlias
            g.DrawString("F", fontA, New SolidBrush(Me.LineColor), ax, ay)

            g.EndContainer(gContainer)
            gp.Dispose()

        End Sub

        Public Sub DrawRoundRect(ByVal g As Graphics, ByVal p As Pen, ByVal x As Integer, ByVal y As Integer, ByVal width As Integer, ByVal height As Integer, ByVal radius As Integer, ByVal myBrush As Brush)

            Dim gp As Drawing2D.GraphicsPath = New Drawing2D.GraphicsPath

            gp.AddLine(x + radius, y, x + width - radius, y)
            gp.AddArc(x + width - radius, y, radius, radius, 270, 90)
            gp.AddLine(x + width, y + radius, x + width, y + height - radius)
            gp.AddArc(x + width - radius, y + height - radius, radius, radius, 0, 90)
            gp.AddLine(x + width - radius, y + height, x + radius, y + height)
            gp.AddArc(x, y + height - radius, radius, radius, 90, 90)
            gp.AddLine(x, y + height - radius, x, y + radius)
            gp.AddArc(x, y, radius, radius, 180, 90)

            gp.CloseFigure()

            g.DrawPath(p, gp)
            g.FillPath(myBrush, gp)

            gp.Dispose()

        End Sub

    End Class

    <Serializable()> Public Class OrificePlateGraphic

        Inherits ShapeGraphic

#Region "Constructors"
        Public Sub New()
            Me.TipoObjeto = GraphicObjects.TipoObjeto.OrificePlate
            Me.Description = "OrificePlate"
        End Sub

        Public Sub New(ByVal graphicPosition As Point)
            Me.New()
            Me.SetPosition(graphicPosition)
        End Sub

        Public Sub New(ByVal posX As Integer, ByVal posY As Integer)
            Me.New(New Point(posX, posY))
        End Sub

        Public Sub New(ByVal graphicPosition As Point, ByVal graphicSize As Size)
            Me.New(graphicPosition)
            Me.SetSize(graphicSize)
            Me.AutoSize = False
        End Sub

        Public Sub New(ByVal posX As Integer, ByVal posY As Integer, ByVal graphicSize As Size)
            Me.New(New Point(posX, posY), graphicSize)
        End Sub

        Public Sub New(ByVal posX As Integer, ByVal posY As Integer, ByVal width As Integer, ByVal height As Integer)
            Me.New(New Point(posX, posY), New Size(width, height))
        End Sub

        Public Sub New(ByVal graphicPosition As Point, ByVal Rotation As Single)
            Me.New()
            Me.SetPosition(graphicPosition)
            Me.Rotation = Rotation
        End Sub

        Public Sub New(ByVal posX As Integer, ByVal posY As Integer, ByVal Rotation As Single)
            Me.New(New Point(posX, posY), Rotation)
        End Sub

        Public Sub New(ByVal graphicPosition As Point, ByVal graphicSize As Size, ByVal Rotation As Single)
            Me.New(graphicPosition, Rotation)
            Me.SetSize(graphicSize)
            Me.AutoSize = False
        End Sub

        Public Sub New(ByVal posX As Integer, ByVal posY As Integer, ByVal graphicSize As Size, ByVal Rotation As Single)
            Me.New(New Point(posX, posY), graphicSize, Rotation)
        End Sub

        Public Sub New(ByVal posX As Integer, ByVal posY As Integer, ByVal width As Integer, _
                               ByVal height As Integer, ByVal Rotation As Single)
            Me.New(New Point(posX, posY), New Size(width, height), Rotation)
        End Sub

#End Region

        Public Overrides Sub CreateConnectors(InCount As Integer, OutCount As Integer)

            Dim myIC1 As New ConnectionPoint
            myIC1.Position = New Point(X, Y + 0.5 * Height)
            myIC1.Type = ConType.ConIn

            Dim myOC1 As New ConnectionPoint
            myOC1.Position = New Point(X + Width, Y + 0.5 * Height)
            myOC1.Type = ConType.ConOut

            Me.EnergyConnector.Position = New Point(X + 0.5 * Width, Y + Height)
            Me.EnergyConnector.Type = ConType.ConEn

            With InputConnectors

                If .Count <> 0 Then
                    If Me.FlippedH Then
                        .Item(0).Position = New Point(X + Width, Y + 0.5 * Height)
                    Else
                        .Item(0).Position = New Point(X, Y + 0.5 * Height)
                    End If
                Else
                    .Add(myIC1)
                End If

            End With

            With OutputConnectors

                If .Count <> 0 Then
                    If Me.FlippedH Then
                        .Item(0).Position = New Point(X, Y + 0.5 * Height)
                    Else
                        .Item(0).Position = New Point(X + Width, Y + 0.5 * Height)
                    End If
                Else
                    .Add(myOC1)
                End If

            End With


        End Sub

        Public Overrides Sub Draw(ByVal g As System.Drawing.Graphics)

            CreateConnectors(0, 0)

            UpdateStatus(Me)

            Dim pt As Point
            Dim raio, angulo As Double
            Dim con As ConnectionPoint
            For Each con In Me.InputConnectors
                pt = con.Position
                raio = ((pt.X - Me.X) ^ 2 + (pt.Y - Me.Y) ^ 2) ^ 0.5
                angulo = Math.Atan2(pt.Y - Me.Y, pt.X - Me.X)
                pt.X = Me.X + raio * Math.Cos(angulo + Me.Rotation / 360 * 2 * Math.PI)
                pt.Y = Me.Y + raio * Math.Sin(angulo + Me.Rotation / 360 * 2 * Math.PI)
                con.Position = pt
            Next
            For Each con In Me.OutputConnectors
                pt = con.Position
                raio = ((pt.X - Me.X) ^ 2 + (pt.Y - Me.Y) ^ 2) ^ 0.5
                angulo = Math.Atan2(pt.Y - Me.Y, pt.X - Me.X)
                pt.X = Me.X + raio * Math.Cos(angulo + Me.Rotation / 360 * 2 * Math.PI)
                pt.Y = Me.Y + raio * Math.Sin(angulo + Me.Rotation / 360 * 2 * Math.PI)
                con.Position = pt
            Next
            With Me.EnergyConnector
                pt = .Position
                raio = ((pt.X - Me.X) ^ 2 + (pt.Y - Me.Y) ^ 2) ^ 0.5
                angulo = Math.Atan2(pt.Y - Me.Y, pt.X - Me.X)
                pt.X = Me.X + raio * Math.Cos(angulo + Me.Rotation / 360 * 2 * Math.PI)
                pt.Y = Me.Y + raio * Math.Sin(angulo + Me.Rotation / 360 * 2 * Math.PI)
                .Position = pt
            End With

            Dim gContainer As System.Drawing.Drawing2D.GraphicsContainer
            Dim myMatrix As Drawing2D.Matrix
            gContainer = g.BeginContainer()
            myMatrix = g.Transform()
            If m_Rotation <> 0 Then
                myMatrix.RotateAt(m_Rotation, New PointF(X, Y), Drawing.Drawing2D.MatrixOrder.Append)
                g.Transform = myMatrix
            End If


            Dim myPen As New Pen(Me.LineColor, Me.LineWidth)
            Dim myPen2 As New Pen(Color.White, 0)
            Dim rect As New Rectangle(X, Y, Width, Height)

            g.SmoothingMode = SmoothingMode.AntiAlias
            'g.DrawRectangle(myPen2, rect)

            Dim gp As Drawing2D.GraphicsPath = New Drawing2D.GraphicsPath
            gp.AddEllipse(rect)
            gp.AddEllipse(CInt(X + 0.3 * Width), CInt(Y + 0.3 * Height), CInt(0.4 * Width), CInt(0.4 * Height))
            Dim rect2 As New Rectangle(X + 0.4 * Width, Y - 0.3 * Height, 0.2 * Width, 0.3 * Height)
            gp.AddRectangle(rect2)

            gp.CloseFigure()

            g.DrawPath(myPen, gp)

            Dim strdist As SizeF = g.MeasureString(Me.Tag, New Font("Arial", 10, FontStyle.Bold, GraphicsUnit.Pixel, 0, False), New PointF(0, 0), New StringFormat(StringFormatFlags.NoClip, 0))
            Dim strx As Single = (Me.Width - strdist.Width) / 2
            'g.FillRectangle(Brushes.White, X + strx, Y + CSng(Height + 5), strdist.Width, strdist.Height)
            g.DrawString(Me.Tag, New Font("Arial", 10, FontStyle.Bold, GraphicsUnit.Pixel, 0, False), New SolidBrush(Me.LineColor), X + strx, Y + Height + 5)

            Dim pgb1 As New PathGradientBrush(gp)
            pgb1.CenterColor = Me.GradientColor1
            pgb1.SurroundColors = New Color() {Me.GradientColor2}
            pgb1.SetSigmaBellShape(0.6)

            If Me.Fill Then
                If Me.GradientMode = False Then
                    g.FillPath(New SolidBrush(Me.FillColor), gp)
                Else
                    g.FillPath(pgb1, gp)
                End If
            End If
            gp.Dispose()
            g.EndContainer(gContainer)

        End Sub

    End Class

    <Serializable()> Public Class CustomUOGraphic
        Inherits ShapeGraphic

#Region "Constructors"
        Public Sub New()
            Me.TipoObjeto = GraphicObjects.TipoObjeto.CustomUO
            Me.Description = "Custom Unit Operation"
            Me.m_mixpoint = True
            Me.m_splitpoint = True
        End Sub

        Public Sub New(ByVal graphicPosition As Point)
            Me.New()
            Me.SetPosition(graphicPosition)
        End Sub

        Public Sub New(ByVal posX As Integer, ByVal posY As Integer)
            Me.New(New Point(posX, posY))
        End Sub

        Public Sub New(ByVal graphicPosition As Point, ByVal graphicSize As Size)
            Me.New(graphicPosition)
            Me.SetSize(graphicSize)
            Me.AutoSize = False
        End Sub

        Public Sub New(ByVal posX As Integer, ByVal posY As Integer, ByVal graphicSize As Size)
            Me.New(New Point(posX, posY), graphicSize)
        End Sub

        Public Sub New(ByVal posX As Integer, ByVal posY As Integer, ByVal width As Integer, ByVal height As Integer)
            Me.New(New Point(posX, posY), New Size(width, height))
        End Sub

        Public Sub New(ByVal graphicPosition As Point, ByVal Rotation As Single)
            Me.New()
            Me.SetPosition(graphicPosition)
            Me.Rotation = Rotation
        End Sub

        Public Sub New(ByVal posX As Integer, ByVal posY As Integer, ByVal Rotation As Single)
            Me.New(New Point(posX, posY), Rotation)
        End Sub

        Public Sub New(ByVal graphicPosition As Point, ByVal graphicSize As Size, ByVal Rotation As Single)
            Me.New(graphicPosition, Rotation)
            Me.SetSize(graphicSize)
            Me.AutoSize = False
        End Sub

        Public Sub New(ByVal posX As Integer, ByVal posY As Integer, ByVal graphicSize As Size, ByVal Rotation As Single)
            Me.New(New Point(posX, posY), graphicSize, Rotation)
        End Sub

        Public Sub New(ByVal posX As Integer, ByVal posY As Integer, ByVal width As Integer, _
                               ByVal height As Integer, ByVal Rotation As Single)
            Me.New(New Point(posX, posY), New Size(width, height), Rotation)
        End Sub

#End Region

        Public Overrides Sub CreateConnectors(InCount As Integer, OutCount As Integer)


            Dim myIC1 As New ConnectionPoint
            myIC1.Position = New Point(X, Y)
            myIC1.Type = ConType.ConIn

            Dim myIC2 As New ConnectionPoint
            myIC2.Position = New Point(X, Y + 1 * 0.13 * Height)
            myIC2.Type = ConType.ConIn

            Dim myIC3 As New ConnectionPoint
            myIC3.Position = New Point(X, Y + 2 * 0.13 * Height)
            myIC3.Type = ConType.ConIn

            Dim myIC4 As New ConnectionPoint
            myIC4.Position = New Point(X + 0.5 * Width, Y)
            myIC4.Type = ConType.ConEn

            Dim myIC5 As New ConnectionPoint
            myIC5.Position = New Point(X, Y + 3 * 0.13 * Height)
            myIC5.Type = ConType.ConIn

            Dim myIC6 As New ConnectionPoint
            myIC6.Position = New Point(X, Y + 4 * 0.13 * Height)
            myIC6.Type = ConType.ConIn

            Dim myIC7 As New ConnectionPoint
            myIC7.Position = New Point(X, Y + 5 * 0.13 * Height)
            myIC7.Type = ConType.ConIn

            Dim myOC1 As New ConnectionPoint
            myOC1.Position = New Point(X + Width, Y)
            myOC1.Type = ConType.ConOut

            Dim myOC2 As New ConnectionPoint
            myOC2.Position = New Point(X + Width, Y + 1 * 0.13 * Height)
            myOC2.Type = ConType.ConOut

            Dim myOC3 As New ConnectionPoint
            myOC3.Position = New Point(X + Width, Y + 2 * 0.13 * Height)
            myOC3.Type = ConType.ConOut

            Dim myOC4 As New ConnectionPoint
            myOC4.Position = New Point(X + 0.5 * Width, Y + Height)
            myOC4.Type = ConType.ConEn

            Dim myOC5 As New ConnectionPoint
            myOC5.Position = New Point(X + Width, Y + 3 * 0.13 * Height)
            myOC5.Type = ConType.ConIn

            Dim myOC6 As New ConnectionPoint
            myOC6.Position = New Point(X + Width, Y + 4 * 0.13 * Height)
            myOC6.Type = ConType.ConIn

            Dim myOC7 As New ConnectionPoint
            myOC7.Position = New Point(X + Width, Y + 5 * 0.13 * Height)
            myOC7.Type = ConType.ConIn

            With InputConnectors

                If .Count <> 0 Then
                    If .Count = 3 Then
                        .Add(myIC4)
                    ElseIf .Count = 4 Then
                        .Add(myIC5)
                        .Add(myIC6)
                        .Add(myIC7)
                    Else
                        If Me.FlippedH Then
                            .Item(0).Position = New Point(X + Width, Y)
                            .Item(1).Position = New Point(X + Width, Y + 1 * 0.13 * Height)
                            .Item(2).Position = New Point(X + Width, Y + 2 * 0.13 * Height)
                            .Item(3).Position = New Point(X + 0.5 * Width, Y)
                            .Item(4).Position = New Point(X + Width, Y + 3 * 0.13 * Height)
                            .Item(5).Position = New Point(X + Width, Y + 4 * 0.13 * Height)
                            .Item(6).Position = New Point(X + Width, Y + 5 * 0.13 * Height)
                        Else
                            .Item(0).Position = New Point(X, Y)
                            .Item(1).Position = New Point(X, Y + 1 * 0.13 * Height)
                            .Item(2).Position = New Point(X, Y + 2 * 0.13 * Height)
                            .Item(3).Position = New Point(X + 0.5 * Width, Y)
                            .Item(4).Position = New Point(X, Y + 3 * 0.13 * Height)
                            .Item(5).Position = New Point(X, Y + 4 * 0.13 * Height)
                            .Item(6).Position = New Point(X, Y + 5 * 0.13 * Height)
                        End If
                    End If
                Else
                    .Add(myIC1)
                    .Add(myIC2)
                    .Add(myIC3)
                    .Add(myIC4)
                    .Add(myIC5)
                    .Add(myIC6)
                    .Add(myIC7)
                End If

            End With

            With OutputConnectors

                If .Count <> 0 Then
                    If .Count = 3 Then
                        .Add(myOC4)
                    ElseIf .Count = 4 Then
                        .Add(myOC5)
                        .Add(myOC6)
                        .Add(myOC7)
                    Else
                        If Me.FlippedH Then
                            .Item(0).Position = New Point(X, Y)
                            .Item(1).Position = New Point(X, Y + 1 * 0.13 * Height)
                            .Item(2).Position = New Point(X, Y + 2 * 0.13 * Height)
                            .Item(3).Position = New Point(X + 0.5 * Width, Y)
                            .Item(4).Position = New Point(X, Y + 3 * 0.13 * Height)
                            .Item(5).Position = New Point(X, Y + 4 * 0.13 * Height)
                            .Item(6).Position = New Point(X, Y + 5 * 0.13 * Height)
                        Else
                            .Item(0).Position = New Point(X + Width, Y)
                            .Item(1).Position = New Point(X + Width, Y + 1 * 0.13 * Height)
                            .Item(2).Position = New Point(X + Width, Y + 2 * 0.13 * Height)
                            .Item(3).Position = New Point(X + 0.5 * Width, Y)
                            .Item(4).Position = New Point(X + Width, Y + 3 * 0.13 * Height)
                            .Item(5).Position = New Point(X + Width, Y + 4 * 0.13 * Height)
                            .Item(6).Position = New Point(X + Width, Y + 5 * 0.13 * Height)
                        End If
                    End If
                Else
                    .Add(myOC1)
                    .Add(myOC2)
                    .Add(myOC3)
                    .Add(myOC4)
                    .Add(myOC5)
                    .Add(myOC6)
                    .Add(myOC7)
                End If

            End With

        End Sub

        Public Overrides Sub Draw(ByVal g As Graphics)

            CreateConnectors(0, 0)

            UpdateStatus(Me)

            Dim pt As Point
            Dim raio, angulo As Double
            Dim con As ConnectionPoint
            For Each con In Me.InputConnectors
                pt = con.Position
                raio = ((pt.X - Me.X) ^ 2 + (pt.Y - Me.Y) ^ 2) ^ 0.5
                angulo = Math.Atan2(pt.Y - Me.Y, pt.X - Me.X)
                pt.X = Me.X + raio * Math.Cos(angulo + Me.Rotation / 360 * 2 * Math.PI)
                pt.Y = Me.Y + raio * Math.Sin(angulo + Me.Rotation / 360 * 2 * Math.PI)
                con.Position = pt
            Next
            For Each con In Me.OutputConnectors
                pt = con.Position
                raio = ((pt.X - Me.X) ^ 2 + (pt.Y - Me.Y) ^ 2) ^ 0.5
                angulo = Math.Atan2(pt.Y - Me.Y, pt.X - Me.X)
                pt.X = Me.X + raio * Math.Cos(angulo + Me.Rotation / 360 * 2 * Math.PI)
                pt.Y = Me.Y + raio * Math.Sin(angulo + Me.Rotation / 360 * 2 * Math.PI)
                con.Position = pt
            Next

            Dim gContainer As Drawing2D.GraphicsContainer
            Dim myMatrix As Drawing2D.Matrix
            gContainer = g.BeginContainer()
            myMatrix = g.Transform()
            If m_Rotation <> 0 Then
                myMatrix.RotateAt(m_Rotation, New PointF(X, Y), _
                    Drawing2D.MatrixOrder.Append)
                g.Transform = myMatrix
            End If

            Dim myPenE As New Pen(Me.LineColor, Me.LineWidth)
            Dim myPen2 As New Pen(Color.White, 0)
            Dim rect As New Rectangle(X, Y, Width, Height)

            Dim gp As Drawing2D.GraphicsPath = New Drawing2D.GraphicsPath

            gp.AddLine(CInt(X), CInt(Y), CInt(X + Width), CInt(Y))
            gp.AddLine(CInt(X + Width), CInt(Y), CInt(X + Width), CInt(Y + Height))
            gp.AddLine(CInt(X + Width), CInt(Y + Height), CInt(X), CInt(Y + Height))
            gp.AddLine(CInt(X), CInt(Y + Height), CInt(X), CInt(Y))

            gp.CloseFigure()

            g.SmoothingMode = SmoothingMode.AntiAlias
            g.DrawPath(myPenE, gp)

            g.SmoothingMode = SmoothingMode.AntiAlias

            Dim strdist As SizeF = g.MeasureString(Me.Tag, New Font("Arial", 10, FontStyle.Bold, GraphicsUnit.Pixel, 0, False), New PointF(0, 0), New StringFormat(StringFormatFlags.NoClip, 0))
            Dim strx As Single = (Me.Width - strdist.Width) / 2
            'g.FillRectangle(Brushes.White, X + strx, Y + CSng(Height + 5), strdist.Width, strdist.Height)
            g.DrawString(Me.Tag, New Font("Arial", 10, FontStyle.Bold, GraphicsUnit.Pixel, 0, False), New SolidBrush(Me.LineColor), X + strx, Y + Height + 5)

            Dim pgb1 As New PathGradientBrush(gp)
            pgb1.CenterColor = Me.GradientColor2
            'lgb1.SetBlendTriangularShape(0.5)
            pgb1.SurroundColors = New Color() {Me.GradientColor1}

            If Me.Fill Then
                If Me.GradientMode = False Then
                    g.FillPath(New SolidBrush(Me.FillColor), gp)
                Else
                    g.FillPath(pgb1, gp)
                End If
            End If

            Dim size As SizeF
            Dim fontA As New Font("Arial", 10, 3, GraphicsUnit.Pixel, 0, False)
            size = g.MeasureString("UO", fontA)

            Dim ax, ay As Integer
            ax = Me.X + (Me.Width - size.Width) / 2
            ay = Me.Y + (Me.Height - size.Height) / 2

            g.SmoothingMode = SmoothingMode.AntiAlias
            g.TextRenderingHint = Text.TextRenderingHint.AntiAlias
            g.DrawString("UO", fontA, Brushes.SteelBlue, ax, ay)

            g.EndContainer(gContainer)

        End Sub

    End Class

    <Serializable()> Public Class ExcelUOGraphic

        Inherits ShapeGraphic

#Region "Constructors"
        Public Sub New()
            Me.TipoObjeto = GraphicObjects.TipoObjeto.ExcelUO
            Me.Description = "ExcelUO"
        End Sub

        Public Sub New(ByVal graphicPosition As Point)
            Me.New()
            Me.SetPosition(graphicPosition)
        End Sub

        Public Sub New(ByVal posX As Integer, ByVal posY As Integer)
            Me.New(New Point(posX, posY))
        End Sub

        Public Sub New(ByVal graphicPosition As Point, ByVal graphicSize As Size)
            Me.New(graphicPosition)
            Me.SetSize(graphicSize)
            Me.AutoSize = False
        End Sub

        Public Sub New(ByVal posX As Integer, ByVal posY As Integer, ByVal graphicSize As Size)
            Me.New(New Point(posX, posY), graphicSize)
        End Sub

        Public Sub New(ByVal posX As Integer, ByVal posY As Integer, ByVal width As Integer, ByVal height As Integer)
            Me.New(New Point(posX, posY), New Size(width, height))
        End Sub

        Public Sub New(ByVal graphicPosition As Point, ByVal Rotation As Single)
            Me.New()
            Me.SetPosition(graphicPosition)
            Me.Rotation = Rotation
        End Sub

        Public Sub New(ByVal posX As Integer, ByVal posY As Integer, ByVal Rotation As Single)
            Me.New(New Point(posX, posY), Rotation)
        End Sub

        Public Sub New(ByVal graphicPosition As Point, ByVal graphicSize As Size, ByVal Rotation As Single)
            Me.New(graphicPosition, Rotation)
            Me.SetSize(graphicSize)
            Me.AutoSize = False
        End Sub

        Public Sub New(ByVal posX As Integer, ByVal posY As Integer, ByVal graphicSize As Size, ByVal Rotation As Single)
            Me.New(New Point(posX, posY), graphicSize, Rotation)
        End Sub

        Public Sub New(ByVal posX As Integer, ByVal posY As Integer, ByVal width As Integer, _
                               ByVal height As Integer, ByVal Rotation As Single)
            Me.New(New Point(posX, posY), New Size(width, height), Rotation)
        End Sub

#End Region

        Public Overrides Sub CreateConnectors(InCount As Integer, OutCount As Integer)

            Dim myIC1 As New ConnectionPoint
            myIC1.Position = New Point(X, Y + 0 * Height)
            myIC1.Type = ConType.ConIn

            Dim myIC2 As New ConnectionPoint
            myIC2.Position = New Point(X, Y + 0.33 * Height)
            myIC2.Type = ConType.ConIn

            Dim myIC3 As New ConnectionPoint
            myIC3.Position = New Point(X, Y + 0.66 * Height)
            myIC3.Type = ConType.ConIn

            Dim myIC4 As New ConnectionPoint
            myIC4.Position = New Point(X, Y + 1 * Height)
            myIC4.Type = ConType.ConIn


            Dim myEC1 As New ConnectionPoint
            myEC1.Position = New Point(X + 0.5 * Width, Y + Height)
            myEC1.Type = ConType.ConEn


            Dim myOC1 As New ConnectionPoint
            myOC1.Position = New Point(X + Width, Y + 0 * Height)
            myOC1.Type = ConType.ConOut

            Dim myOC2 As New ConnectionPoint
            myOC2.Position = New Point(X + Width, Y + 0.33 * Height)
            myOC2.Type = ConType.ConOut

            Dim myOC3 As New ConnectionPoint
            myOC3.Position = New Point(X + Width, Y + 0.66 * Height)
            myOC3.Type = ConType.ConOut

            Dim myOC4 As New ConnectionPoint
            myOC4.Position = New Point(X + Width, Y + 1 * Height)
            myOC4.Type = ConType.ConOut

            Me.EnergyConnector.Position = New Point(X + 0.5 * Width, Y + Height)
            Me.EnergyConnector.Type = ConType.ConEn

            With InputConnectors

                If .Count <> 0 Then
                    If Me.FlippedH Then
                        .Item(0).Position = New Point(X + Width, Y + 0 * Height)
                        .Item(1).Position = New Point(X + Width, Y + 0.33 * Height)
                        .Item(2).Position = New Point(X + Width, Y + 0.66 * Height)
                        .Item(3).Position = New Point(X + Width, Y + 1 * Height)
                        .Item(4).Position = New Point(X + 0.5 * Width, Y + Height)
                    Else
                        .Item(0).Position = New Point(X, Y + 0 * Height)
                        .Item(1).Position = New Point(X, Y + 0.33 * Height)
                        .Item(2).Position = New Point(X, Y + 0.66 * Height)
                        .Item(3).Position = New Point(X, Y + 1 * Height)
                        .Item(4).Position = New Point(X + 0.5 * Width, Y + Height)
                    End If
                Else
                    .Add(myIC1)
                    .Add(myIC2)
                    .Add(myIC3)
                    .Add(myIC4)
                    .Add(myEC1)
                End If

            End With

            With OutputConnectors

                If .Count <> 0 Then
                    If Me.FlippedH Then
                        .Item(0).Position = New Point(X, Y + 0 * Height)
                        .Item(1).Position = New Point(X, Y + 0.33 * Height)
                        .Item(2).Position = New Point(X, Y + 0.66 * Height)
                        .Item(3).Position = New Point(X, Y + 1 * Height)
                    Else
                        .Item(0).Position = New Point(X + Width, Y + 0 * Height)
                        .Item(1).Position = New Point(X + Width, Y + 0.33 * Height)
                        .Item(2).Position = New Point(X + Width, Y + 0.66 * Height)
                        .Item(3).Position = New Point(X + Width, Y + 1 * Height)
                    End If
                Else
                    .Add(myOC1)
                    .Add(myOC2)
                    .Add(myOC3)
                    .Add(myOC4)
                End If

            End With

        End Sub

        Public Overrides Sub Draw(ByVal g As System.Drawing.Graphics)

            CreateConnectors(0, 0)

            UpdateStatus(Me)

            Dim pt As Point
            Dim raio, angulo As Double
            Dim con As ConnectionPoint
            For Each con In Me.InputConnectors
                pt = con.Position
                raio = ((pt.X - Me.X) ^ 2 + (pt.Y - Me.Y) ^ 2) ^ 0.5
                angulo = Math.Atan2(pt.Y - Me.Y, pt.X - Me.X)
                pt.X = Me.X + raio * Math.Cos(angulo + Me.Rotation / 360 * 2 * Math.PI)
                pt.Y = Me.Y + raio * Math.Sin(angulo + Me.Rotation / 360 * 2 * Math.PI)
                con.Position = pt
            Next
            For Each con In Me.OutputConnectors
                pt = con.Position
                raio = ((pt.X - Me.X) ^ 2 + (pt.Y - Me.Y) ^ 2) ^ 0.5
                angulo = Math.Atan2(pt.Y - Me.Y, pt.X - Me.X)
                pt.X = Me.X + raio * Math.Cos(angulo + Me.Rotation / 360 * 2 * Math.PI)
                pt.Y = Me.Y + raio * Math.Sin(angulo + Me.Rotation / 360 * 2 * Math.PI)
                con.Position = pt
            Next
            With Me.EnergyConnector
                pt = .Position
                raio = ((pt.X - Me.X) ^ 2 + (pt.Y - Me.Y) ^ 2) ^ 0.5
                angulo = Math.Atan2(pt.Y - Me.Y, pt.X - Me.X)
                pt.X = Me.X + raio * Math.Cos(angulo + Me.Rotation / 360 * 2 * Math.PI)
                pt.Y = Me.Y + raio * Math.Sin(angulo + Me.Rotation / 360 * 2 * Math.PI)
                .Position = pt
            End With

            Dim gContainer As System.Drawing.Drawing2D.GraphicsContainer
            Dim myMatrix As Drawing2D.Matrix
            gContainer = g.BeginContainer()
            myMatrix = g.Transform()
            If m_Rotation <> 0 Then
                myMatrix.RotateAt(m_Rotation, New PointF(X, Y), Drawing.Drawing2D.MatrixOrder.Append)
                g.Transform = myMatrix
            End If


            Dim myPen As New Pen(Me.LineColor, Me.LineWidth)
            Dim myPen2 As New Pen(Color.White, 0)
            Dim rect As New Rectangle(X, Y, Width, Height)

            g.FillRectangle(Brushes.LightBlue, rect)
            g.DrawRectangle(myPen, rect)

            Dim gp As Drawing2D.GraphicsPath = New Drawing2D.GraphicsPath
            gp.AddLine(CInt(X), CInt(Y + 0.5 * Height), CInt(X + 0.5 * Width), CInt(Y))
            gp.AddLine(CInt(X + 0.5 * Width), CInt(Y), CInt(X + Width), CInt(Y + 0.5 * Height))
            gp.AddLine(CInt(X + Width), CInt(Y + 0.5 * Height), CInt(X + 0.5 * Width), CInt(Y + Height))
            gp.AddLine(CInt(X + 0.5 * Width), CInt(Y + Height), CInt(X), CInt(Y + 0.5 * Height))

            gp.CloseFigure()

            g.SmoothingMode = SmoothingMode.AntiAlias
            g.DrawPath(myPen, gp)

            Dim strdist As SizeF = g.MeasureString(Me.Tag, New Font("Arial", 10, FontStyle.Bold, GraphicsUnit.Pixel, 0, False), New PointF(0, 0), New StringFormat(StringFormatFlags.NoClip, 0))
            Dim strx As Single = (Me.Width - strdist.Width) / 2
            'g.FillRectangle(Brushes.White, X + strx, Y + CSng(Height + 5), strdist.Width, strdist.Height)
            g.DrawString(Me.Tag, New Font("Arial", 10, FontStyle.Bold, GraphicsUnit.Pixel, 0, False), New SolidBrush(Me.LineColor), X + strx, Y + Height + 5)

            Dim pgb1 As New PathGradientBrush(gp)
            pgb1.CenterColor = Me.GradientColor2
            'lgb1.SetBlendTriangularShape(0.5)
            pgb1.SurroundColors = New Color() {Me.GradientColor1}

            If Me.Fill Then
                If Me.GradientMode = False Then
                    g.FillPath(New SolidBrush(Me.FillColor), gp)
                Else
                    g.FillPath(pgb1, gp)
                End If
            End If

            Dim size As SizeF
            Dim fontA As New Font("Arial", 10, 3, GraphicsUnit.Pixel, 0, False)
            size = g.MeasureString("E", fontA)

            Dim ax, ay As Integer
            ax = Me.X + (Me.Width - size.Width) / 2
            ay = Me.Y + (Me.Height - size.Height) / 2

            g.SmoothingMode = SmoothingMode.AntiAlias
            g.TextRenderingHint = Text.TextRenderingHint.AntiAlias
            g.DrawString("E", fontA, Brushes.DarkOrange, ax, ay)

            gp.Dispose()
            g.EndContainer(gContainer)

        End Sub

    End Class

    <Serializable()> Public Class FlowsheetUOGraphic

        Inherits ShapeGraphic

#Region "Constructors"
        Public Sub New()
            Me.TipoObjeto = GraphicObjects.TipoObjeto.FlowsheetUO
            Me.Description = "FlowsheetUO"
        End Sub

        Public Sub New(ByVal graphicPosition As Point)
            Me.New()
            Me.SetPosition(graphicPosition)
        End Sub

        Public Sub New(ByVal posX As Integer, ByVal posY As Integer)
            Me.New(New Point(posX, posY))
        End Sub

        Public Sub New(ByVal graphicPosition As Point, ByVal graphicSize As Size)
            Me.New(graphicPosition)
            Me.SetSize(graphicSize)
            Me.AutoSize = False
        End Sub

        Public Sub New(ByVal posX As Integer, ByVal posY As Integer, ByVal graphicSize As Size)
            Me.New(New Point(posX, posY), graphicSize)
        End Sub

        Public Sub New(ByVal posX As Integer, ByVal posY As Integer, ByVal width As Integer, ByVal height As Integer)
            Me.New(New Point(posX, posY), New Size(width, height))
        End Sub

        Public Sub New(ByVal graphicPosition As Point, ByVal Rotation As Single)
            Me.New()
            Me.SetPosition(graphicPosition)
            Me.Rotation = Rotation
        End Sub

        Public Sub New(ByVal posX As Integer, ByVal posY As Integer, ByVal Rotation As Single)
            Me.New(New Point(posX, posY), Rotation)
        End Sub

        Public Sub New(ByVal graphicPosition As Point, ByVal graphicSize As Size, ByVal Rotation As Single)
            Me.New(graphicPosition, Rotation)
            Me.SetSize(graphicSize)
            Me.AutoSize = False
        End Sub

        Public Sub New(ByVal posX As Integer, ByVal posY As Integer, ByVal graphicSize As Size, ByVal Rotation As Single)
            Me.New(New Point(posX, posY), graphicSize, Rotation)
        End Sub

        Public Sub New(ByVal posX As Integer, ByVal posY As Integer, ByVal width As Integer, _
                               ByVal height As Integer, ByVal Rotation As Single)
            Me.New(New Point(posX, posY), New Size(width, height), Rotation)
        End Sub

#End Region

        Public Overrides Sub CreateConnectors(InCount As Integer, OutCount As Integer)

            Dim myIC1 As New ConnectionPoint With {.Position = New Point(X, Y + 0.1 * Height), .Type = ConType.ConIn}
            Dim myIC2 As New ConnectionPoint With {.Position = New Point(X, Y + 0.2 * Height), .Type = ConType.ConIn}
            Dim myIC3 As New ConnectionPoint With {.Position = New Point(X, Y + 0.3 * Height), .Type = ConType.ConIn}
            Dim myIC4 As New ConnectionPoint With {.Position = New Point(X, Y + 0.4 * Height), .Type = ConType.ConIn}
            Dim myIC5 As New ConnectionPoint With {.Position = New Point(X, Y + 0.5 * Height), .Type = ConType.ConIn}
            Dim myIC6 As New ConnectionPoint With {.Position = New Point(X, Y + 0.6 * Height), .Type = ConType.ConIn}
            Dim myIC7 As New ConnectionPoint With {.Position = New Point(X, Y + 0.7 * Height), .Type = ConType.ConIn}
            Dim myIC8 As New ConnectionPoint With {.Position = New Point(X, Y + 0.8 * Height), .Type = ConType.ConIn}
            Dim myIC9 As New ConnectionPoint With {.Position = New Point(X, Y + 0.9 * Height), .Type = ConType.ConIn}
            Dim myIC10 As New ConnectionPoint With {.Position = New Point(X, Y + 1.0 * Height), .Type = ConType.ConIn}

            Dim myOC1 As New ConnectionPoint With {.Position = New Point(X + Width, Y + 0.1 * Height), .Type = ConType.ConOut}
            Dim myOC2 As New ConnectionPoint With {.Position = New Point(X + Width, Y + 0.2 * Height), .Type = ConType.ConOut}
            Dim myOC3 As New ConnectionPoint With {.Position = New Point(X + Width, Y + 0.3 * Height), .Type = ConType.ConOut}
            Dim myOC4 As New ConnectionPoint With {.Position = New Point(X + Width, Y + 0.4 * Height), .Type = ConType.ConOut}
            Dim myOC5 As New ConnectionPoint With {.Position = New Point(X + Width, Y + 0.5 * Height), .Type = ConType.ConOut}
            Dim myOC6 As New ConnectionPoint With {.Position = New Point(X + Width, Y + 0.6 * Height), .Type = ConType.ConOut}
            Dim myOC7 As New ConnectionPoint With {.Position = New Point(X + Width, Y + 0.7 * Height), .Type = ConType.ConOut}
            Dim myOC8 As New ConnectionPoint With {.Position = New Point(X + Width, Y + 0.8 * Height), .Type = ConType.ConOut}
            Dim myOC9 As New ConnectionPoint With {.Position = New Point(X + Width, Y + 0.9 * Height), .Type = ConType.ConOut}
            Dim myOC10 As New ConnectionPoint With {.Position = New Point(X + Width, Y + 1.0 * Height), .Type = ConType.ConOut}

            With InputConnectors

                If .Count <> 0 Then
                    If Me.FlippedH Then
                        .Item(0).Position = New Point(X + Width, Y + 0.1 * Height)
                        .Item(1).Position = New Point(X + Width, Y + 0.2 * Height)
                        .Item(2).Position = New Point(X + Width, Y + 0.3 * Height)
                        .Item(3).Position = New Point(X + Width, Y + 0.4 * Height)
                        .Item(4).Position = New Point(X + Width, Y + 0.5 * Height)
                        .Item(5).Position = New Point(X + Width, Y + 0.6 * Height)
                        .Item(6).Position = New Point(X + Width, Y + 0.7 * Height)
                        .Item(7).Position = New Point(X + Width, Y + 0.8 * Height)
                        .Item(8).Position = New Point(X + Width, Y + 0.9 * Height)
                        .Item(9).Position = New Point(X + Width, Y + 1.0 * Height)
                    Else
                        .Item(0).Position = New Point(X, Y + 0.1 * Height)
                        .Item(1).Position = New Point(X, Y + 0.2 * Height)
                        .Item(2).Position = New Point(X, Y + 0.3 * Height)
                        .Item(3).Position = New Point(X, Y + 0.4 * Height)
                        .Item(4).Position = New Point(X, Y + 0.5 * Height)
                        .Item(5).Position = New Point(X, Y + 0.6 * Height)
                        .Item(6).Position = New Point(X, Y + 0.7 * Height)
                        .Item(7).Position = New Point(X, Y + 0.8 * Height)
                        .Item(8).Position = New Point(X, Y + 0.9 * Height)
                        .Item(9).Position = New Point(X, Y + 1.0 * Height)
                    End If
                Else
                    .Add(myIC1)
                    .Add(myIC2)
                    .Add(myIC3)
                    .Add(myIC4)
                    .Add(myIC5)
                    .Add(myIC6)
                    .Add(myIC7)
                    .Add(myIC8)
                    .Add(myIC9)
                    .Add(myIC10)
                End If

            End With

            With OutputConnectors

                If .Count <> 0 Then
                    If Me.FlippedH Then
                        .Item(0).Position = New Point(X, Y + 0.1 * Height)
                        .Item(1).Position = New Point(X, Y + 0.2 * Height)
                        .Item(2).Position = New Point(X, Y + 0.3 * Height)
                        .Item(3).Position = New Point(X, Y + 0.4 * Height)
                        .Item(4).Position = New Point(X, Y + 0.5 * Height)
                        .Item(5).Position = New Point(X, Y + 0.6 * Height)
                        .Item(6).Position = New Point(X, Y + 0.7 * Height)
                        .Item(7).Position = New Point(X, Y + 0.8 * Height)
                        .Item(8).Position = New Point(X, Y + 0.9 * Height)
                        .Item(9).Position = New Point(X, Y + 1.0 * Height)
                    Else
                        .Item(0).Position = New Point(X + Width, Y + 0.1 * Height)
                        .Item(1).Position = New Point(X + Width, Y + 0.2 * Height)
                        .Item(2).Position = New Point(X + Width, Y + 0.3 * Height)
                        .Item(3).Position = New Point(X + Width, Y + 0.4 * Height)
                        .Item(4).Position = New Point(X + Width, Y + 0.5 * Height)
                        .Item(5).Position = New Point(X + Width, Y + 0.6 * Height)
                        .Item(6).Position = New Point(X + Width, Y + 0.7 * Height)
                        .Item(7).Position = New Point(X + Width, Y + 0.8 * Height)
                        .Item(8).Position = New Point(X + Width, Y + 0.9 * Height)
                        .Item(9).Position = New Point(X + Width, Y + 1.0 * Height)
                    End If
                Else
                    .Add(myOC1)
                    .Add(myOC2)
                    .Add(myOC3)
                    .Add(myOC4)
                    .Add(myOC5)
                    .Add(myOC6)
                    .Add(myOC7)
                    .Add(myOC8)
                    .Add(myOC9)
                    .Add(myOC10)
                End If

            End With

        End Sub

        Public Overrides Sub Draw(ByVal g As System.Drawing.Graphics)

            CreateConnectors(0, 0)

            UpdateStatus(Me)

            Dim pt As Point
            Dim raio, angulo As Double
            Dim con As ConnectionPoint
            For Each con In Me.InputConnectors
                pt = con.Position
                raio = ((pt.X - Me.X) ^ 2 + (pt.Y - Me.Y) ^ 2) ^ 0.5
                angulo = Math.Atan2(pt.Y - Me.Y, pt.X - Me.X)
                pt.X = Me.X + raio * Math.Cos(angulo + Me.Rotation / 360 * 2 * Math.PI)
                pt.Y = Me.Y + raio * Math.Sin(angulo + Me.Rotation / 360 * 2 * Math.PI)
                con.Position = pt
            Next
            For Each con In Me.OutputConnectors
                pt = con.Position
                raio = ((pt.X - Me.X) ^ 2 + (pt.Y - Me.Y) ^ 2) ^ 0.5
                angulo = Math.Atan2(pt.Y - Me.Y, pt.X - Me.X)
                pt.X = Me.X + raio * Math.Cos(angulo + Me.Rotation / 360 * 2 * Math.PI)
                pt.Y = Me.Y + raio * Math.Sin(angulo + Me.Rotation / 360 * 2 * Math.PI)
                con.Position = pt
            Next
            With Me.EnergyConnector
                pt = .Position
                raio = ((pt.X - Me.X) ^ 2 + (pt.Y - Me.Y) ^ 2) ^ 0.5
                angulo = Math.Atan2(pt.Y - Me.Y, pt.X - Me.X)
                pt.X = Me.X + raio * Math.Cos(angulo + Me.Rotation / 360 * 2 * Math.PI)
                pt.Y = Me.Y + raio * Math.Sin(angulo + Me.Rotation / 360 * 2 * Math.PI)
                .Position = pt
            End With

            Dim gContainer As System.Drawing.Drawing2D.GraphicsContainer
            Dim myMatrix As Drawing2D.Matrix
            gContainer = g.BeginContainer()
            myMatrix = g.Transform()
            If m_Rotation <> 0 Then
                myMatrix.RotateAt(m_Rotation, New PointF(X, Y), Drawing.Drawing2D.MatrixOrder.Append)
                g.Transform = myMatrix
            End If


            Dim myPen As New Pen(Me.LineColor, Me.LineWidth)
            Dim myPen2 As New Pen(Color.White, 0)
            Dim rect As New Rectangle(X, Y, Width, Height)

            g.FillRectangle(Brushes.LightBlue, rect)
            g.DrawRectangle(myPen, rect)

            Dim gp As Drawing2D.GraphicsPath = New Drawing2D.GraphicsPath
            gp.AddLine(CInt(X), CInt(Y + 0.5 * Height), CInt(X + 0.5 * Width), CInt(Y))
            gp.AddLine(CInt(X + 0.5 * Width), CInt(Y), CInt(X + Width), CInt(Y + 0.5 * Height))
            gp.AddLine(CInt(X + Width), CInt(Y + 0.5 * Height), CInt(X + 0.5 * Width), CInt(Y + Height))
            gp.AddLine(CInt(X + 0.5 * Width), CInt(Y + Height), CInt(X), CInt(Y + 0.5 * Height))

            gp.CloseFigure()

            g.SmoothingMode = SmoothingMode.AntiAlias
            g.DrawPath(myPen, gp)

            Dim strdist As SizeF = g.MeasureString(Me.Tag, New Font("Arial", 10, FontStyle.Bold, GraphicsUnit.Pixel, 0, False), New PointF(0, 0), New StringFormat(StringFormatFlags.NoClip, 0))
            Dim strx As Single = (Me.Width - strdist.Width) / 2
            'g.FillRectangle(Brushes.White, X + strx, Y + CSng(Height + 5), strdist.Width, strdist.Height)
            g.DrawString(Me.Tag, New Font("Arial", 10, FontStyle.Bold, GraphicsUnit.Pixel, 0, False), New SolidBrush(Me.LineColor), X + strx, Y + Height + 5)

            Dim pgb1 As New PathGradientBrush(gp)
            pgb1.CenterColor = Me.GradientColor2
            'lgb1.SetBlendTriangularShape(0.5)
            pgb1.SurroundColors = New Color() {Me.GradientColor1}

            If Me.Fill Then
                If Me.GradientMode = False Then
                    g.FillPath(New SolidBrush(Me.FillColor), gp)
                Else
                    g.FillPath(pgb1, gp)
                End If
            End If

            Dim size As SizeF
            Dim fontA As New Font("Arial", 10, 3, GraphicsUnit.Pixel, 0, False)
            size = g.MeasureString("F", fontA)

            Dim ax, ay As Integer
            ax = Me.X + (Me.Width - size.Width) / 2
            ay = Me.Y + (Me.Height - size.Height) / 2

            g.SmoothingMode = SmoothingMode.AntiAlias
            g.TextRenderingHint = Text.TextRenderingHint.AntiAlias
            g.DrawString("F", fontA, Brushes.DarkOrange, ax, ay)

            gp.Dispose()
            g.EndContainer(gContainer)

        End Sub

    End Class


    <Serializable()> Public Class CapeOpenUOGraphic

        Inherits ShapeGraphic

#Region "Constructors"
        Public Sub New()
            Me.TipoObjeto = GraphicObjects.TipoObjeto.CapeOpenUO
            Me.Description = "CapeOpenUnitOperation"
            Me.m_mixpoint = True
            Me.m_splitpoint = True
        End Sub

        Public Sub New(ByVal graphicPosition As Point)
            Me.New()
            Me.SetPosition(graphicPosition)
        End Sub

        Public Sub New(ByVal posX As Integer, ByVal posY As Integer)
            Me.New(New Point(posX, posY))
        End Sub

        Public Sub New(ByVal graphicPosition As Point, ByVal graphicSize As Size)
            Me.New(graphicPosition)
            Me.SetSize(graphicSize)
            Me.AutoSize = False
        End Sub

        Public Sub New(ByVal posX As Integer, ByVal posY As Integer, ByVal graphicSize As Size)
            Me.New(New Point(posX, posY), graphicSize)
        End Sub

        Public Sub New(ByVal posX As Integer, ByVal posY As Integer, ByVal width As Integer, ByVal height As Integer)
            Me.New(New Point(posX, posY), New Size(width, height))
        End Sub

        Public Sub New(ByVal graphicPosition As Point, ByVal Rotation As Single)
            Me.New()
            Me.SetPosition(graphicPosition)
            Me.Rotation = Rotation
        End Sub

        Public Sub New(ByVal posX As Integer, ByVal posY As Integer, ByVal Rotation As Single)
            Me.New(New Point(posX, posY), Rotation)
        End Sub

        Public Sub New(ByVal graphicPosition As Point, ByVal graphicSize As Size, ByVal Rotation As Single)
            Me.New(graphicPosition, Rotation)
            Me.SetSize(graphicSize)
            Me.AutoSize = False
        End Sub

        Public Sub New(ByVal posX As Integer, ByVal posY As Integer, ByVal graphicSize As Size, ByVal Rotation As Single)
            Me.New(New Point(posX, posY), graphicSize, Rotation)
        End Sub

        Public Sub New(ByVal posX As Integer, ByVal posY As Integer, ByVal width As Integer, _
                               ByVal height As Integer, ByVal Rotation As Single)
            Me.New(New Point(posX, posY), New Size(width, height), Rotation)
        End Sub

#End Region

        Public Overrides Sub PositionConnectors()

            MyBase.PositionConnectors()

            If Not Me.AdditionalInfo Is Nothing Then

                Dim obj1(Me.InputConnectors.Count), obj2(Me.InputConnectors.Count) As Double
                Dim obj3(Me.OutputConnectors.Count), obj4(Me.OutputConnectors.Count) As Double
                obj1 = Me.AdditionalInfo(0)
                obj2 = Me.AdditionalInfo(1)
                obj3 = Me.AdditionalInfo(2)
                obj4 = Me.AdditionalInfo(3)

                Try
                    Dim i As Integer = 0
                    For Each ic As ConnectionPoint In Me.InputConnectors
                        ic.Position = New Point(Me.X + obj1(i), Me.Y + obj2(i))
                        i = i + 1
                    Next
                    i = 0
                    For Each oc As ConnectionPoint In Me.OutputConnectors
                        oc.Position = New Point(Me.X + obj3(i), Me.Y + obj4(i))
                        i = i + 1
                    Next
                Catch ex As Exception

                End Try

            End If

        End Sub

        Public Overrides Sub Draw(ByVal g As System.Drawing.Graphics)

            MyBase.Draw(g)

            UpdateStatus(Me)

            Dim pt As Point
            Dim raio, angulo As Double
            Dim con As ConnectionPoint
            For Each con In Me.InputConnectors
                pt = con.Position
                raio = ((pt.X - Me.X) ^ 2 + (pt.Y - Me.Y) ^ 2) ^ 0.5
                angulo = Math.Atan2(pt.Y - Me.Y, pt.X - Me.X)
                pt.X = Me.X + raio * Math.Cos(angulo + Me.Rotation / 360 * 2 * Math.PI)
                pt.Y = Me.Y + raio * Math.Sin(angulo + Me.Rotation / 360 * 2 * Math.PI)
                con.Position = pt
            Next
            For Each con In Me.OutputConnectors
                pt = con.Position
                raio = ((pt.X - Me.X) ^ 2 + (pt.Y - Me.Y) ^ 2) ^ 0.5
                angulo = Math.Atan2(pt.Y - Me.Y, pt.X - Me.X)
                pt.X = Me.X + raio * Math.Cos(angulo + Me.Rotation / 360 * 2 * Math.PI)
                pt.Y = Me.Y + raio * Math.Sin(angulo + Me.Rotation / 360 * 2 * Math.PI)
                con.Position = pt
            Next

            Dim gContainer As Drawing2D.GraphicsContainer
            Dim myMatrix As Drawing2D.Matrix
            gContainer = g.BeginContainer()
            myMatrix = g.Transform()
            If m_Rotation <> 0 Then
                myMatrix.RotateAt(m_Rotation, New PointF(X, Y), _
                    Drawing2D.MatrixOrder.Append)
                g.Transform = myMatrix
            End If


            Select Case ShapeOverride
                Case ShapeIcon.AbsorptionColumn
                    Dim myPen As New Pen(Me.LineColor, Me.LineWidth)
                    Dim myPen1 As New Pen(Me.LineColor, Me.LineWidth)
                    With myPen1
                        .EndCap = LineCap.ArrowAnchor
                    End With
                    Dim myPen2 As New Pen(Color.White, 0)
                    Dim rect As New Rectangle(X, Y, Width, Height)
                    g.SmoothingMode = SmoothingMode.AntiAlias
                    Dim lgb1 As LinearGradientBrush
                    lgb1 = New LinearGradientBrush(rect, Me.GradientColor1, Me.GradientColor2, LinearGradientMode.Horizontal)
                    lgb1.SetBlendTriangularShape(0.5)
                    Dim path As New GraphicsPath()
                    path.AddEllipse(rect)
                    Dim pthGrBrush As New PathGradientBrush(path)
                    pthGrBrush.CenterColor = Me.GradientColor2
                    Dim colors As Color() = {Me.GradientColor1}
                    pthGrBrush.SurroundColors = colors
                    pthGrBrush.SetSigmaBellShape(0.5)
                    If Me.Fill Then
                        If Me.GradientMode = False Then
                            If Me.FlippedH = True Then
                                Me.DrawRoundRect(g, myPen, X + (1 - 0.1 - 0.2) * Width, Y + 0.1 * Height, 0.2 * 1.25 * Width, 0.8 * Height, 20, New SolidBrush(Me.FillColor))
                            Else
                                Me.DrawRoundRect(g, myPen, X + (0.05) * 1.25 * Width, Y + 0.1 * Height, 0.2 * 1.25 * Width, 0.8 * Height, 20, New SolidBrush(Me.FillColor))
                            End If
                        Else
                            If Me.FlippedH = True Then
                                Me.DrawRoundRect(g, myPen, X + (1 - 0.1 - 0.2) * Width, Y + 0.1 * Height, 0.2 * 1.25 * Width, 0.8 * Height, 20, lgb1)
                            Else
                                Me.DrawRoundRect(g, myPen, X + 0.05 * 1.25 * Width, Y + 0.1 * Height, 0.2 * 1.25 * Width, 0.8 * Height, 20, lgb1)
                            End If
                        End If
                    End If
                    If Me.FlippedH = True Then
                        Me.DrawRoundRect(g, myPen, X + (1 - 0.1 - 0.2) * Width, Y + 0.1 * Height, 0.2 * 1.25 * Width, 0.8 * Height, 20, Brushes.Transparent)
                        g.DrawLines(myPen, New PointF() {New PointF(X + 0.825 * Width, Y + 0.1 * Height), New PointF(X + 0.825 * Width, Y + 0.02 * Height), New PointF(X + 0.4 * Width, Y + 0.02 * Height)})
                        g.DrawLines(myPen, New PointF() {New PointF(X + 0.825 * Width, Y + 0.9 * Height), New PointF(X + 0.825 * Width, Y + 0.98 * Height), New PointF(X + 0.4 * Width, Y + 0.98 * Height)})
                        g.DrawLine(myPen1, CSng(X + 0.4 * Width), CSng(Y + 0.98 * Height), CSng(X), CSng(Y + 0.98 * Height))
                        g.DrawLine(myPen1, CSng(X + 0.4 * Width), CSng(Y + 0.02 * Height), CSng(X), CSng(Y + 0.02 * Height))
                        g.DrawLine(myPen1, CSng(X + Width), CSng(Y + 0.2 * Height), CSng(X + 0.95 * Width), CSng(Y + 0.2 * Height))
                        g.DrawLine(myPen1, CSng(X + Width), CSng(Y + 0.8 * Height), CSng(X + 0.95 * Width), CSng(Y + 0.8 * Height))
                        g.DrawLine(myPen, CSng(X + 0.7 * Width), CSng(Y + 0.2 * Height), CSng(X + 0.95 * Width), CSng(Y + 0.2 * Height))
                        g.DrawLine(myPen, CSng(X + 0.7 * Width), CSng(Y + 0.3 * Height), CSng(X + 0.95 * Width), CSng(Y + 0.3 * Height))
                        g.DrawLine(myPen, CSng(X + 0.7 * Width), CSng(Y + 0.4 * Height), CSng(X + 0.95 * Width), CSng(Y + 0.4 * Height))
                        g.DrawLine(myPen, CSng(X + 0.7 * Width), CSng(Y + 0.5 * Height), CSng(X + 0.95 * Width), CSng(Y + 0.5 * Height))
                        g.DrawLine(myPen, CSng(X + 0.7 * Width), CSng(Y + 0.6 * Height), CSng(X + 0.95 * Width), CSng(Y + 0.6 * Height))
                        g.DrawLine(myPen, CSng(X + 0.7 * Width), CSng(Y + 0.7 * Height), CSng(X + 0.95 * Width), CSng(Y + 0.7 * Height))
                        g.DrawLine(myPen, CSng(X + 0.7 * Width), CSng(Y + 0.8 * Height), CSng(X + 0.95 * Width), CSng(Y + 0.8 * Height))
                    Else
                        Me.DrawRoundRect(g, myPen, X + 0.05 * 1.25 * Width, Y + 0.1 * Height, 0.2 * 1.25 * Width, 0.8 * Height, 20, Brushes.Transparent)
                        g.DrawLines(myPen, New PointF() {New PointF(X + 0.175 * Width, Y + 0.1 * Height), New PointF(X + 0.175 * Width, Y + 0.02 * Height), New PointF(X + 0.6 * 1.25 * Width, Y + 0.02 * Height)})
                        g.DrawLines(myPen, New PointF() {New PointF(X + 0.175 * Width, Y + 0.9 * Height), New PointF(X + 0.175 * Width, Y + 0.98 * Height), New PointF(X + 0.6 * 1.25 * Width, Y + 0.98 * Height)})
                        g.DrawLine(myPen1, CSng(X + 0.6 * 1.25 * Width), CSng(Y + 0.98 * Height), CSng(X + Width), CSng(Y + 0.98 * Height))
                        g.DrawLine(myPen1, CSng(X + 0.6 * 1.25 * Width), CSng(Y + 0.02 * Height), CSng(X + Width), CSng(Y + 0.02 * Height))
                        g.DrawLine(myPen1, CSng(X), CSng(Y + 0.2 * Height), CSng(X + 0.05 * 1.25 * Width), CSng(Y + 0.2 * Height))
                        g.DrawLine(myPen1, CSng(X), CSng(Y + 0.8 * Height), CSng(X + 0.05 * 1.25 * Width), CSng(Y + 0.8 * Height))
                        g.DrawLine(myPen, CSng(X + 0.05 * 1.25 * Width), CSng(Y + 0.2 * Height), CSng(X + 0.31 * Width), CSng(Y + 0.2 * Height))
                        g.DrawLine(myPen, CSng(X + 0.05 * 1.25 * Width), CSng(Y + 0.3 * Height), CSng(X + 0.31 * Width), CSng(Y + 0.3 * Height))
                        g.DrawLine(myPen, CSng(X + 0.05 * 1.25 * Width), CSng(Y + 0.4 * Height), CSng(X + 0.31 * Width), CSng(Y + 0.4 * Height))
                        g.DrawLine(myPen, CSng(X + 0.05 * 1.25 * Width), CSng(Y + 0.5 * Height), CSng(X + 0.31 * Width), CSng(Y + 0.5 * Height))
                        g.DrawLine(myPen, CSng(X + 0.05 * 1.25 * Width), CSng(Y + 0.6 * Height), CSng(X + 0.31 * Width), CSng(Y + 0.6 * Height))
                        g.DrawLine(myPen, CSng(X + 0.05 * 1.25 * Width), CSng(Y + 0.7 * Height), CSng(X + 0.31 * Width), CSng(Y + 0.7 * Height))
                        g.DrawLine(myPen, CSng(X + 0.05 * 1.25 * Width), CSng(Y + 0.8 * Height), CSng(X + 0.31 * Width), CSng(Y + 0.8 * Height))
                    End If
                    Me.DrawRoundRect(g, myPen, Me.X, Me.Y, Me.Width, Me.Height, 5, New SolidBrush(Color.Transparent))
                    Dim strdist As SizeF = g.MeasureString(Me.Tag, New Font("Arial", 10, FontStyle.Bold, GraphicsUnit.Pixel, 0, False), New PointF(0, 0), New StringFormat(StringFormatFlags.NoClip, 0))
                    Dim strx As Single = (Me.Width - strdist.Width) / 2
                    'g.FillRectangle(Brushes.White, X + strx, Y + CSng(Height + 5), strdist.Width, strdist.Height)
                    g.DrawString(Me.Tag, New Font("Arial", 10, FontStyle.Bold, GraphicsUnit.Pixel, 0, False), New SolidBrush(Me.LineColor), X + strx, Y + Height + 5)
                    g.EndContainer(gContainer)
                Case ShapeIcon.ComponentSeparator
                    Dim rect2 As New Rectangle(X + 0.123 * Width, Y + 0.5 * Height, 0.127 * Width, 0.127 * Height)
                    Dim rect3 As New Rectangle(X + 0.7 * Width, Y + 0.1 * Height, 0.127 * Width, 0.127 * Height)
                    Dim rect4 As New Rectangle(X + 0.7 * Width, Y + 0.773 * Height, 0.127 * Width, 0.127 * Height)
                    If Me.FlippedH = True Then
                        rect2 = New Rectangle(X + (1 - 0.123) * Width, Y + 0.5 * Height, 0.127 * Width, 0.127 * Height)
                        rect3 = New Rectangle(X + 0.3 * Width, Y + 0.1 * Height, 0.127 * Width, 0.127 * Height)
                        rect4 = New Rectangle(X + 0.3 * Width, Y + 0.773 * Height, 0.127 * Width, 0.127 * Height)
                    End If
                    Dim myPen As New Pen(Me.LineColor, Me.LineWidth)
                    Dim myPen2 As New Pen(Color.White, 0)
                    Dim rect As New Rectangle(X, Y, Width, Height)
                    g.SmoothingMode = SmoothingMode.AntiAlias
                    If Me.FlippedH = True Then
                        Me.DrawRoundRect(g, myPen, X + 0.4 * Width, Y, 0.45 * Width, Height, 10, Brushes.Transparent)
                    Else
                        Me.DrawRoundRect(g, myPen, X + 0.25 * Width, Y, 0.45 * Width, Height, 10, Brushes.Transparent)
                    End If
                    g.DrawRectangle(myPen, rect2)
                    g.DrawRectangle(myPen, rect3)
                    g.DrawRectangle(myPen, rect4)
                    Dim strdist As SizeF = g.MeasureString(Me.Tag, New Font("Arial", 10, FontStyle.Bold, GraphicsUnit.Pixel, 0, False), New PointF(0, 0), New StringFormat(StringFormatFlags.NoClip, 0))
                    Dim strx As Single = (Me.Width - strdist.Width) / 2
                    'g.FillRectangle(Brushes.White, X + strx, Y + CSng(Height + 5), strdist.Width, strdist.Height)
                    g.DrawString(Me.Tag, New Font("Arial", 10, FontStyle.Bold, GraphicsUnit.Pixel, 0, False), New SolidBrush(Me.LineColor), X + strx, Y + Height + 5)
                    Dim gp As Drawing2D.GraphicsPath = New Drawing2D.GraphicsPath
                    Dim radius As Integer = 3
                    gp.AddLine(X + radius, Y, X + Width - radius, Y)
                    gp.AddArc(X + Width - radius, Y, radius, radius, 270, 90)
                    gp.AddLine(X + Width, Y + radius, X + Width, Y + Height - radius)
                    gp.AddArc(X + Width - radius, Y + Height - radius, radius, radius, 0, 90)
                    gp.AddLine(X + Width - radius, Y + Height, X + radius, Y + Height)
                    gp.AddArc(X, Y + Height - radius, radius, radius, 90, 90)
                    gp.AddLine(X, Y + Height - radius, X, Y + radius)
                    gp.AddArc(X, Y, radius, radius, 180, 90)
                    Dim lgb1 As LinearGradientBrush
                    lgb1 = New LinearGradientBrush(rect, Me.GradientColor1, Me.GradientColor2, LinearGradientMode.Horizontal)
                    lgb1.SetBlendTriangularShape(0.5)
                    If Me.Fill Then
                        If Me.GradientMode = False Then
                            g.FillRectangle(New SolidBrush(Me.FillColor), rect3)
                            g.FillRectangle(New SolidBrush(Me.FillColor), rect4)
                            g.FillRectangle(New SolidBrush(Me.FillColor), rect2)
                            If Me.FlippedH = True Then
                                Me.DrawRoundRect(g, myPen, X + 0.4 * Width, Y, 0.45 * Width, Height, 6, New SolidBrush(Me.FillColor))
                            Else
                                Me.DrawRoundRect(g, myPen, X + 0.25 * Width, Y, 0.45 * Width, Height, 6, New SolidBrush(Me.FillColor))
                            End If
                        Else
                            g.FillRectangle(New SolidBrush(Me.FillColor), rect3)
                            g.FillRectangle(New SolidBrush(Me.FillColor), rect4)
                            g.FillRectangle(New SolidBrush(Me.FillColor), rect2)
                            If Me.FlippedH = True Then
                                Me.DrawRoundRect(g, myPen, X + 0.4 * Width, Y, 0.45 * Width, Height, 6, lgb1)
                            Else
                                Me.DrawRoundRect(g, myPen, X + 0.25 * Width, Y, 0.45 * Width, Height, 6, lgb1)
                            End If
                        End If
                    End If
                    If Me.FlippedH Then
                        g.DrawLines(myPen, New PointF() {New PointF(X + 0.4 * Width, Y + 0.3 * Height), New PointF(X + 0.85 * Width, Me.Y + 0.3 * Height)})
                        g.DrawLines(myPen, New PointF() {New PointF(X + 0.4 * Width, Y + 0.7 * Height), New PointF(X + 0.85 * Width, Me.Y + 0.7 * Height)})
                        g.DrawLines(myPen, New PointF() {New PointF(X + 0.4 * Width, Y + 0.3 * Height), New PointF(X + 0.85 * Width, Me.Y + 0.7 * Height)})
                        g.DrawLines(myPen, New PointF() {New PointF(X + 0.4 * Width, Y + 0.7 * Height), New PointF(X + 0.85 * Width, Me.Y + 0.3 * Height)})
                    Else
                        g.DrawLines(myPen, New PointF() {New PointF(X + 0.25 * Width, Y + 0.3 * Height), New PointF(X + 0.7 * Width, Me.Y + 0.3 * Height)})
                        g.DrawLines(myPen, New PointF() {New PointF(X + 0.25 * Width, Y + 0.7 * Height), New PointF(X + 0.7 * Width, Me.Y + 0.7 * Height)})
                        g.DrawLines(myPen, New PointF() {New PointF(X + 0.25 * Width, Y + 0.3 * Height), New PointF(X + 0.7 * Width, Me.Y + 0.7 * Height)})
                        g.DrawLines(myPen, New PointF() {New PointF(X + 0.25 * Width, Y + 0.7 * Height), New PointF(X + 0.7 * Width, Me.Y + 0.3 * Height)})
                    End If
                    Dim size As SizeF
                    Dim fontA As New Font("Arial", 8, FontStyle.Bold, GraphicsUnit.Pixel, 0, False)
                    size = g.MeasureString("CS", fontA)
                    Dim ax, ay As Integer
                    If Me.FlippedH Then
                        ax = Me.X + (Me.Width - size.Width) / 2
                        ay = Me.Y + Me.Height - size.Height
                    Else
                        ax = Me.X + (Me.Width - size.Width) / 2
                        ay = Me.Y + Me.Height - size.Height
                    End If
                    g.SmoothingMode = SmoothingMode.AntiAlias
                    g.TextRenderingHint = Text.TextRenderingHint.AntiAlias
                    g.DrawString("CS", fontA, New SolidBrush(Me.LineColor), ax, ay)
                    g.EndContainer(gContainer)
                    gp.Dispose()
                Case ShapeIcon.Compressor
                    Dim myPen As New Pen(Me.LineColor, Me.LineWidth)
                    Dim myPen2 As New Pen(Color.White, 0)
                    Dim rect As New Rectangle(X, Y, Width, Height)
                    g.SmoothingMode = SmoothingMode.AntiAlias
                    Dim strdist As SizeF = g.MeasureString(Me.Tag, New Font("Arial", 10, FontStyle.Bold, GraphicsUnit.Pixel, 0, False), New PointF(0, 0), New StringFormat(StringFormatFlags.NoClip, 0))
                    Dim strx As Single = (Me.Width - strdist.Width) / 2
                    'g.FillRectangle(Brushes.White, X + strx, Y + CSng(Height + 5), strdist.Width, strdist.Height)
                    g.DrawString(Me.Tag, New Font("Arial", 10, FontStyle.Bold, GraphicsUnit.Pixel, 0, False), New SolidBrush(Me.LineColor), X + strx, Y + Height + 5)
                    Dim gp As Drawing2D.GraphicsPath = New Drawing2D.GraphicsPath
                    If Me.FlippedH = False Then
                        gp.AddLine(CInt(X), CInt(Y), CInt(X + Width), CInt(Y + 0.3 * Height))
                        gp.AddLine(CInt(X + Width), CInt(Y + 0.3 * Height), CInt(X + Width), CInt(Y + 0.7 * Height))
                        gp.AddLine(CInt(X + Width), CInt(Y + 0.7 * Height), CInt(X), CInt(Y + Height))
                        gp.AddLine(CInt(X), CInt(Y + Height), CInt(X), CInt(Y))
                    Else
                        gp.AddLine(CInt(X + Width), CInt(Y), CInt(X), CInt(Y + 0.3 * Height))
                        gp.AddLine(CInt(X), CInt(Y + 0.3 * Height), CInt(X), CInt(Y + 0.7 * Height))
                        gp.AddLine(CInt(X), CInt(Y + 0.7 * Height), CInt(X + Width), CInt(Y + Height))
                        gp.AddLine(CInt(X + Width), CInt(Y + Height), CInt(X + Width), CInt(Y))
                    End If
                    gp.CloseFigure()
                    g.DrawPath(myPen, Me.GetRoundedLine(gp.PathPoints, 1))
                    Dim pgb1 As New LinearGradientBrush(New PointF(X, Y), New PointF(X, Y + Height), Me.GradientColor1, Me.GradientColor2)
                    If Me.Fill Then
                        If Me.GradientMode = False Then
                            g.FillPath(New SolidBrush(Me.FillColor), gp)
                        Else
                            g.FillPath(pgb1, gp)
                        End If
                    End If
                    gp.Dispose()
                    g.EndContainer(gContainer)
                Case ShapeIcon.Cooler
                    Dim myPen As New Pen(Me.LineColor, Me.LineWidth)
                    Dim myPen2 As New Pen(Color.White, 0)
                    Dim rect As New Rectangle(X, Y, Width, Height)
                    g.SmoothingMode = SmoothingMode.AntiAlias
                    Dim gp As Drawing2D.GraphicsPath = New Drawing2D.GraphicsPath
                    gp.AddLine(CInt(X), CInt(Y + 0.5 * Height), CInt(X + 0.5 * Width), CInt(Y))
                    gp.AddLine(CInt(X + 0.5 * Width), CInt(Y), CInt(X + Width), CInt(Y + 0.5 * Height))
                    gp.AddLine(CInt(X + Width), CInt(Y + 0.5 * Height), CInt(X + 0.5 * Width), CInt(Y + Height))
                    gp.AddLine(CInt(X + 0.5 * Width), CInt(Y + Height), CInt(X), CInt(Y + 0.5 * Height))
                    gp.CloseFigure()
                    g.DrawPath(myPen, gp)
                    Dim strdist As SizeF = g.MeasureString(Me.Tag, New Font("Arial", 10, FontStyle.Bold, GraphicsUnit.Pixel, 0, False), New PointF(0, 0), New StringFormat(StringFormatFlags.NoClip, 0))
                    Dim strx As Single = (Me.Width - strdist.Width) / 2
                    'g.FillRectangle(Brushes.White, X + strx, Y + CSng(Height + 5), strdist.Width, strdist.Height)
                    g.DrawString(Me.Tag, New Font("Arial", 10, FontStyle.Bold, GraphicsUnit.Pixel, 0, False), New SolidBrush(Me.LineColor), X + strx, Y + Height + 5)
                    Dim pgb1 As New PathGradientBrush(gp)
                    pgb1.CenterColor = Me.GradientColor2
                    pgb1.SurroundColors = New Color() {Me.GradientColor1}
                    If Me.Fill Then
                        If Me.GradientMode = False Then
                            g.FillPath(New SolidBrush(Me.FillColor), gp)
                        Else
                            g.FillPath(pgb1, gp)
                        End If
                    End If
                    Dim size As SizeF
                    Dim fontA As New Font("Arial", 10, 3, GraphicsUnit.Pixel, 0, False)
                    size = g.MeasureString("R", fontA)
                    Dim ax, ay As Integer
                    ax = Me.X + (Me.Width - size.Width) / 2
                    ay = Me.Y + (Me.Height - size.Height) / 2
                    g.SmoothingMode = SmoothingMode.AntiAlias
                    g.TextRenderingHint = Text.TextRenderingHint.AntiAlias
                    g.DrawString("R", fontA, Brushes.DarkBlue, ax, ay)
                    gp.Dispose()
                    g.EndContainer(gContainer)
                Case ShapeIcon.DefaultShape
                    Dim myPenE As New Pen(Me.LineColor, Me.LineWidth)
                    Dim myPen2 As New Pen(Color.White, 0)
                    Dim rect As New Rectangle(X, Y, Width, Height)
                    Dim gp As Drawing2D.GraphicsPath = New Drawing2D.GraphicsPath
                    gp.AddLine(CInt(X), CInt(Y), CInt(X + Width), CInt(Y))
                    gp.AddLine(CInt(X + Width), CInt(Y), CInt(X + Width), CInt(Y + Height))
                    gp.AddLine(CInt(X + Width), CInt(Y + Height), CInt(X), CInt(Y + Height))
                    gp.AddLine(CInt(X), CInt(Y + Height), CInt(X), CInt(Y))
                    gp.CloseFigure()
                    g.SmoothingMode = SmoothingMode.AntiAlias
                    g.DrawPath(myPenE, Me.GetRoundedLine(gp.PathPoints, 5))
                    g.SmoothingMode = SmoothingMode.AntiAlias
                    Dim strdist As SizeF = g.MeasureString(Me.Tag, New Font("Arial", 10, FontStyle.Regular, GraphicsUnit.Pixel, 0, False), New PointF(0, 0), New StringFormat(StringFormatFlags.NoClip, 0))
                    Dim strx As Single = (Me.Width - strdist.Width) / 2
                    'g.FillRectangle(Brushes.White, X + strx, Y + CSng(Height + 5), strdist.Width, strdist.Height)
                    g.DrawString(Me.Tag, New Font("Arial", 10, FontStyle.Bold, GraphicsUnit.Pixel, 0, False), New SolidBrush(Me.LineColor), X + strx, Y + Height + 5)
                    Dim pgb1 As New PathGradientBrush(gp)
                    pgb1.CenterColor = Me.GradientColor2
                    pgb1.SurroundColors = New Color() {Me.GradientColor1}
                    If Me.Fill Then
                        If Me.GradientMode = False Then
                            g.FillPath(New SolidBrush(Me.FillColor), Me.GetRoundedLine(gp.PathPoints, 5))
                        Else
                            g.FillPath(pgb1, Me.GetRoundedLine(gp.PathPoints, 5))
                        End If
                    End If
                    Dim size As SizeF
                    Dim fontA As New Font("Arial", 12, FontStyle.Bold, GraphicsUnit.Pixel, 0, False)
                    size = g.MeasureString("CO", fontA)
                    Dim ax, ay As Integer
                    ax = Me.X + (Me.Width - size.Width) / 2
                    ay = Me.Y + (Me.Height - size.Height) / 2
                    g.SmoothingMode = SmoothingMode.AntiAlias
                    g.TextRenderingHint = Text.TextRenderingHint.AntiAlias
                    g.DrawString("CO", fontA, Brushes.SteelBlue, ax, ay)
                    g.EndContainer(gContainer)
                Case ShapeIcon.DistillationColumn
                    Dim myPen As New Pen(Me.LineColor, Me.LineWidth)
                    Dim myPen1 As New Pen(Me.LineColor, Me.LineWidth)
                    With myPen1
                        .EndCap = LineCap.ArrowAnchor
                    End With
                    Dim myPen2 As New Pen(Color.White, 0)
                    Dim myPen3 As New Pen(Me.LineColor, Me.LineWidth)
                    With myPen3
                        .DashStyle = DashStyle.Dot
                    End With
                    Dim rect As New Rectangle(X, Y, Width, Height)
                    g.SmoothingMode = SmoothingMode.AntiAlias
                    Dim lgb1 As LinearGradientBrush
                    lgb1 = New LinearGradientBrush(rect, Me.GradientColor1, Me.GradientColor2, LinearGradientMode.Horizontal)
                    lgb1.SetBlendTriangularShape(0.5)
                    Dim path As New GraphicsPath()
                    path.AddEllipse(rect)
                    Dim pthGrBrush As New PathGradientBrush(path)
                    pthGrBrush.CenterColor = Me.GradientColor2
                    Dim colors As Color() = {Me.GradientColor1}
                    pthGrBrush.SurroundColors = colors
                    pthGrBrush.SetSigmaBellShape(0.5)
                    If Me.Fill Then
                        If Me.GradientMode = False Then
                            If Me.FlippedH = True Then
                                Me.DrawRoundRect(g, myPen, X + (1 - 0.1 - 0.2) * Width, Y + 0.2 * Height, 0.2 * 1.25 * Width, 0.7 * Height, 20, New SolidBrush(Me.FillColor))
                                Me.DrawRoundRect(g, myPen, X + 0.1 * Width, Y + 0.1 * Height, 0.15 * 1.25 * Width, 0.15 * Height, 20, lgb1)
                                g.FillEllipse(New SolidBrush(Me.FillColor), CSng(X + (0.475 - 0.15) * Width), CSng(Y + 0.0 * Height), CSng(0.15 * 1.25 * Width), CSng(0.15 * Height))
                                g.FillEllipse(New SolidBrush(Me.FillColor), CSng(X + (0.475 - 0.15) * Width), CSng(Y + 0.75 * Height), CSng(0.15 * 1.25 * Width), CSng(0.15 * Height))
                            Else
                                Me.DrawRoundRect(g, myPen, X + (0.05) * 1.25 * Width, Y + 0.2 * Height, 0.2 * 1.25 * Width, 0.7 * Height, 20, New SolidBrush(Me.FillColor))
                                g.FillEllipse(New SolidBrush(Me.FillColor), CSng(X + 0.525 * 1.25 * Width), CSng(Y), CSng(0.15 * 1.25 * Width), CSng(0.15 * Height))
                                g.FillEllipse(New SolidBrush(Me.FillColor), CSng(X + 0.525 * 1.25 * Width), CSng(Y + 0.75 * Height), CSng(0.15 * 1.25 * Width), CSng(0.15 * Height))
                            End If
                        Else
                            If Me.FlippedH = True Then
                                Me.DrawRoundRect(g, myPen, X + (1 - 0.1 - 0.2) * Width, Y + 0.2 * Height, 0.2 * 1.25 * Width, 0.7 * Height, 20, lgb1)
                                Me.DrawRoundRect(g, myPen, X + 0.1 * Width, Y + 0.1 * Height, 0.15 * 1.25 * Width, 0.15 * Height, 20, lgb1)
                                g.FillEllipse(pthGrBrush, CSng(X + (0.475 - 0.15) * Width), CSng(Y + 0.0 * Height), CSng(0.15 * 1.25 * Width), CSng(0.15 * Height))
                                g.FillEllipse(pthGrBrush, CSng(X + (0.475 - 0.15) * Width), CSng(Y + 0.75 * Height), CSng(0.15 * 1.25 * Width), CSng(0.15 * Height))
                            Else
                                Me.DrawRoundRect(g, myPen, X + 0.05 * 1.25 * Width, Y + 0.2 * Height, 0.2 * 1.25 * Width, 0.7 * Height, 20, lgb1)
                                Me.DrawRoundRect(g, myPen, X + 0.525 * 1.25 * Width, Y + 0.1 * Height, 0.15 * 1.25 * Width, 0.15 * Height, 20, lgb1)
                                g.FillEllipse(pthGrBrush, CSng(X + 0.3 * 1.25 * Width), CSng(Y), CSng(0.15 * 1.25 * Width), CSng(0.15 * Height))
                                g.FillEllipse(pthGrBrush, CSng(X + 0.525 * 1.25 * Width), CSng(Y + 0.75 * Height), CSng(0.15 * 1.25 * Width), CSng(0.15 * Height))
                            End If
                        End If
                    End If
                    If Me.FlippedH = True Then
                        Me.DrawRoundRect(g, myPen, X + 0.7 * Width, Y + 0.2 * Height, 0.2 * 1.25 * Width, 0.7 * Height, 20, Brushes.Transparent)
                        Me.DrawRoundRect(g, myPen, X + 0.1 * Width, Y + 0.1 * Height, 0.15 * 1.25 * Width, 0.15 * Height, 20, Brushes.Transparent)
                        g.DrawLines(myPen, New PointF() {New PointF(X + 0.175 * Width, Y + 0.1 * Height), New PointF(X + 0.175 * Width, Y + 0.02 * Height), New PointF(X + 0.32 * 1.25 * Width, Y + 0.02 * Height)})
                        g.DrawLines(myPen, New PointF() {New PointF(X + 0.38 * 1.25 * Width, Y + 0.02 * Height), New PointF(X + 0.8 * Width, Y + 0.02 * Height), New PointF(X + 0.8 * Width, Y + 0.2 * Height)})
                        g.DrawLines(myPen, New PointF() {New PointF(X + 0.4 * Width, Y + 0.9 * Height), New PointF(X + 0.4 * Width, Y + 0.98 * Height), New PointF(X + 0.8 * Width, Y + 0.98 * Height), New PointF(X + 0.8 * Width, Y + 0.9 * Height)})
                        g.DrawEllipse(myPen, CSng(X + (1 - 0.525 - 0.15) * Width), CSng(Y + 0.0 * Height), CSng(0.15 * 1.25 * Width), CSng(0.15 * Height))
                        g.DrawEllipse(myPen, CSng(X + (1 - 0.525 - 0.15) * Width), CSng(Y + 0.75 * Height), CSng(0.15 * 1.25 * Width), CSng(0.15 * Height))
                        g.DrawLines(myPen, New PointF() {New PointF(X + 0.175 * Width, Y + 0.25 * Height), New PointF(X + 0.175 * Width, Y + 0.3 * Height)})
                        g.DrawLines(myPen, New PointF() {New PointF(X + 0.4 * Width, Y + 0.75 * Height), New PointF(X + 0.4 * Width, Y + 0.7 * Height)})
                        g.DrawLine(myPen1, CSng(X + 0.4 * Width), CSng(Y + 0.3 * Height), CSng(X + 0.7 * Width), CSng(Y + 0.3 * Height))
                        g.DrawLine(myPen1, CSng(X + 0.4 * Width), CSng(Y + 0.7 * Height), CSng(X + 0.7 * Width), CSng(Y + 0.7 * Height))
                        g.DrawLine(myPen1, CSng(X + 0.6 * Width), CSng(Y + 0.98 * Height), CSng(X), CSng(Y + 0.98 * Height))
                        If Me.Shape = 1 Then
                            g.DrawLine(myPen1, CSng(X + 0.4 * Width), CSng(Y + 0.02 * Height), CSng(X), CSng(Y + 0.02 * Height))
                            g.DrawLine(myPen1, CSng(X + 0.4 * Width), CSng(Y + 0.3 * Height), CSng(X), CSng(Y + 0.3 * Height))
                        ElseIf Me.Shape = 0 Then
                            g.DrawLine(myPen1, CSng(X + 0.4 * Width), CSng(Y + 0.3 * Height), CSng(X), CSng(Y + 0.3 * Height))
                        Else
                            g.DrawLine(myPen1, CSng(X + 0.4 * Width), CSng(Y + 0.02 * Height), CSng(X), CSng(Y + 0.02 * Height))
                        End If
                        g.DrawLines(myPen, New PointF() {New PointF(X + (1 - 0.4) * Width, Y + 0.07 * Height), New PointF(X + (1 - 0.5) * Width, Y + 0.07 * Height), New PointF(X + (1 - 0.55) * Width, Y + 0.125 * Height), New PointF(X + (1 - 0.6) * Width, Y + 0.02 * Height), New PointF(X + (1 - 0.65) * Width, Y + 0.08 * Height)})
                        g.DrawLine(myPen3, CSng(X + 0.275 * 1.25 * Width), CSng(Y + 0.08 * Height), CSng(X + 0.1 * Width), CSng(Y + 0.08 * Height))
                        g.DrawLine(myPen1, CSng(X + 0.1 * 1.25 * Width), CSng(Y + 0.08 * Height), CSng(X), CSng(Y + 0.08 * Height))
                        g.DrawLines(myPen, New PointF() {New PointF(X + 0.5 * Width, Y + 0.825 * Height), New PointF(X + 0.45 * Width, Y + 0.825 * Height), New PointF(X + 0.425 * Width, Y + 0.875 * Height), New PointF(X + 0.375 * Width, Y + 0.775 * Height), New PointF(X + 0.35 * Width, Y + 0.825 * Height), New PointF(X, Y + 0.825 * Height)})
                        g.DrawLine(myPen1, CSng(X + 0.5 * Width), CSng(Y + 0.825 * Height), CSng(X + 0.6 * Width), CSng(Y + 0.825 * Height))
                        g.DrawLine(myPen1, CSng(X + Width), CSng(Y + 0.5 * Height), CSng(X + 0.95 * Width), CSng(Y + 0.5 * Height))
                        g.DrawLine(myPen, CSng(X + 0.7 * Width), CSng(Y + 0.3 * Height), CSng(X + 0.95 * Width), CSng(Y + 0.3 * Height))
                        g.DrawLine(myPen, CSng(X + 0.7 * Width), CSng(Y + 0.4 * Height), CSng(X + 0.95 * Width), CSng(Y + 0.4 * Height))
                        g.DrawLine(myPen, CSng(X + 0.7 * Width), CSng(Y + 0.5 * Height), CSng(X + 0.95 * Width), CSng(Y + 0.5 * Height))
                        g.DrawLine(myPen, CSng(X + 0.7 * Width), CSng(Y + 0.6 * Height), CSng(X + 0.95 * Width), CSng(Y + 0.6 * Height))
                        g.DrawLine(myPen, CSng(X + 0.7 * Width), CSng(Y + 0.7 * Height), CSng(X + 0.95 * Width), CSng(Y + 0.7 * Height))
                        g.DrawLine(myPen, CSng(X + 0.7 * Width), CSng(Y + 0.8 * Height), CSng(X + 0.95 * Width), CSng(Y + 0.8 * Height))
                    Else
                        Me.DrawRoundRect(g, myPen, X + 0.05 * 1.25 * Width, Y + 0.2 * Height, 0.2 * 1.25 * Width, 0.7 * Height, 20, Brushes.Transparent)
                        Me.DrawRoundRect(g, myPen, X + 0.525 * 1.25 * Width, Y + 0.1 * Height, 0.15 * 1.25 * Width, 0.15 * Height, 20, Brushes.Transparent)
                        g.DrawLines(myPen, New PointF() {New PointF(X + 0.175 * Width, Y + 0.2 * Height), New PointF(X + 0.175 * Width, Y + 0.02 * Height), New PointF(X + 0.32 * 1.25 * Width, Y + 0.02 * Height)})
                        g.DrawLines(myPen, New PointF() {New PointF(X + 0.43 * 1.25 * Width, Y + 0.02 * Height), New PointF(X + 0.6 * 1.25 * Width, Y + 0.02 * Height), New PointF(X + 0.6 * 1.25 * Width, Y + 0.1 * Height)})
                        g.DrawLines(myPen, New PointF() {New PointF(X + 0.175 * Width, Y + 0.9 * Height), New PointF(X + 0.175 * Width, Y + 0.98 * Height), New PointF(X + 0.6 * 1.25 * Width, Y + 0.98 * Height), New PointF(X + 0.6 * 1.25 * Width, Y + 0.9 * Height)})
                        g.DrawEllipse(myPen, CSng(X + 0.3 * 1.25 * Width), CSng(Y), CSng(0.15 * 1.25 * Width), CSng(0.15 * Height))
                        g.DrawEllipse(myPen, CSng(X + 0.525 * 1.25 * Width), CSng(Y + 0.75 * Height), CSng(0.15 * 1.25 * Width), CSng(0.15 * Height))
                        g.DrawLines(myPen, New PointF() {New PointF(X + 0.6 * 1.25 * Width, Y + 0.25 * Height), New PointF(X + 0.6 * 1.25 * Width, Y + 0.3 * Height)})
                        g.DrawLines(myPen, New PointF() {New PointF(X + 0.6 * 1.25 * Width, Y + 0.75 * Height), New PointF(X + 0.6 * 1.25 * Width, Y + 0.7 * Height)})
                        g.DrawLine(myPen1, CSng(X + 0.6 * 1.25 * Width), CSng(Y + 0.3 * Height), CSng(X + 0.25 * 1.25 * Width), CSng(Y + 0.3 * Height))
                        g.DrawLine(myPen1, CSng(X + 0.6 * 1.25 * Width), CSng(Y + 0.7 * Height), CSng(X + 0.25 * 1.25 * Width), CSng(Y + 0.7 * Height))
                        g.DrawLine(myPen1, CSng(X + 0.6 * 1.25 * Width), CSng(Y + 0.98 * Height), CSng(X + Width), CSng(Y + 0.98 * Height))
                        If Me.Shape = 1 Then
                            g.DrawLine(myPen1, CSng(X + 0.6 * 1.25 * Width), CSng(Y + 0.02 * Height), CSng(X + Width), CSng(Y + 0.02 * Height))
                            g.DrawLine(myPen1, CSng(X + 0.6 * 1.25 * Width), CSng(Y + 0.3 * Height), CSng(X + Width), CSng(Y + 0.3 * Height))
                        ElseIf Me.Shape = 0 Then
                            g.DrawLine(myPen1, CSng(X + 0.6 * 1.25 * Width), CSng(Y + 0.3 * Height), CSng(X + Width), CSng(Y + 0.3 * Height))
                        Else
                            g.DrawLine(myPen1, CSng(X + 0.6 * 1.25 * Width), CSng(Y + 0.02 * Height), CSng(X + Width), CSng(Y + 0.02 * Height))
                        End If
                        g.DrawLines(myPen, New PointF() {New PointF(X + 0.2 * 1.25 * Width, Y + 0.07 * Height), New PointF(X + 0.3 * 1.25 * Width, Y + 0.07 * Height), New PointF(X + 0.35 * 1.25 * Width, Y + 0.125 * Height), New PointF(X + 0.4 * 1.25 * Width, Y + 0.02 * Height), New PointF(X + 0.45 * 1.25 * Width, Y + 0.08 * Height)})
                        g.DrawLine(myPen3, CSng(X + 0.45 * 1.25 * Width), CSng(Y + 0.08 * Height), CSng(X + 0.65 * 1.25 * Width), CSng(Y + 0.08 * Height))
                        g.DrawLine(myPen1, CSng(X + 0.65 * 1.25 * Width), CSng(Y + 0.08 * Height), CSng(X + Width), CSng(Y + 0.08 * Height))
                        g.DrawLines(myPen, New PointF() {New PointF(X + 0.5 * 1.25 * Width, Y + 0.825 * Height), New PointF(X + 0.55 * 1.25 * Width, Y + 0.825 * Height), New PointF(X + 0.575 * 1.25 * Width, Y + 0.875 * Height), New PointF(X + 0.625 * 1.25 * Width, Y + 0.775 * Height), New PointF(X + 0.65 * 1.25 * Width, Y + 0.825 * Height), New PointF(X + Width, Y + 0.825 * Height)})
                        g.DrawLine(myPen1, CSng(X + 0.5 * 1.25 * Width), CSng(Y + 0.825 * Height), CSng(X + 0.4 * 1.25 * Width), CSng(Y + 0.825 * Height))
                        g.DrawLine(myPen1, CSng(X), CSng(Y + 0.5 * Height), CSng(X + 0.05 * 1.25 * Width), CSng(Y + 0.5 * Height))
                        g.DrawLine(myPen, CSng(X + 0.05 * 1.25 * Width), CSng(Y + 0.3 * Height), CSng(X + 0.31 * Width), CSng(Y + 0.3 * Height))
                        g.DrawLine(myPen, CSng(X + 0.05 * 1.25 * Width), CSng(Y + 0.4 * Height), CSng(X + 0.31 * Width), CSng(Y + 0.4 * Height))
                        g.DrawLine(myPen, CSng(X + 0.05 * 1.25 * Width), CSng(Y + 0.5 * Height), CSng(X + 0.31 * Width), CSng(Y + 0.5 * Height))
                        g.DrawLine(myPen, CSng(X + 0.05 * 1.25 * Width), CSng(Y + 0.6 * Height), CSng(X + 0.31 * Width), CSng(Y + 0.6 * Height))
                        g.DrawLine(myPen, CSng(X + 0.05 * 1.25 * Width), CSng(Y + 0.7 * Height), CSng(X + 0.31 * Width), CSng(Y + 0.7 * Height))
                        g.DrawLine(myPen, CSng(X + 0.05 * 1.25 * Width), CSng(Y + 0.8 * Height), CSng(X + 0.31 * Width), CSng(Y + 0.8 * Height))
                    End If
                    Me.DrawRoundRect(g, myPen, Me.X, Me.Y, Me.Width, Me.Height, 5, New SolidBrush(Color.Transparent))
                    Dim strdist As SizeF = g.MeasureString(Me.Tag, New Font("Arial", 10, FontStyle.Bold, GraphicsUnit.Pixel, 0, False), New PointF(0, 0), New StringFormat(StringFormatFlags.NoClip, 0))
                    Dim strx As Single = (Me.Width - strdist.Width) / 2
                    'g.FillRectangle(Brushes.White, X + strx, Y + CSng(Height + 5), strdist.Width, strdist.Height)
                    g.DrawString(Me.Tag, New Font("Arial", 10, FontStyle.Bold, GraphicsUnit.Pixel, 0, False), New SolidBrush(Me.LineColor), X + strx, Y + Height + 5)
                    g.EndContainer(gContainer)
                Case ShapeIcon.Expander
                    Dim myPen As New Pen(Me.LineColor, Me.LineWidth)
                    Dim myPen2 As New Pen(Color.White, 0)
                    Dim rect As New Rectangle(X, Y, Width, Height)
                    g.SmoothingMode = SmoothingMode.AntiAlias
                    Dim strdist As SizeF = g.MeasureString(Me.Tag, New Font("Arial", 10, FontStyle.Bold, GraphicsUnit.Pixel, 0, False), New PointF(0, 0), New StringFormat(StringFormatFlags.NoClip, 0))
                    Dim strx As Single = (Me.Width - strdist.Width) / 2
                    'g.FillRectangle(Brushes.White, X + strx, Y + CSng(Height + 5), strdist.Width, strdist.Height)
                    g.DrawString(Me.Tag, New Font("Arial", 10, FontStyle.Bold, GraphicsUnit.Pixel, 0, False), New SolidBrush(Me.LineColor), X + strx, Y + Height + 5)
                    Dim gp As Drawing2D.GraphicsPath = New Drawing2D.GraphicsPath
                    If Me.FlippedH = False Then
                        gp.AddLine(CInt(X), CInt(Y + 0.3 * Height), CInt(X), CInt(Y + 0.7 * Height))
                        gp.AddLine(CInt(X), CInt(Y + 0.7 * Height), CInt(X + Width), CInt(Y + Height))
                        gp.AddLine(CInt(X + Width), CInt(Y + Height), CInt(X + Width), CInt(Y))
                        gp.AddLine(CInt(X + Width), CInt(Y), CInt(X), CInt(Y + 0.3 * Height))
                    Else
                        gp.AddLine(CInt(X + Width), CInt(Y + 0.3 * Height), CInt(X + Width), CInt(Y + 0.7 * Height))
                        gp.AddLine(CInt(X + Width), CInt(Y + 0.7 * Height), CInt(X), CInt(Y + Height))
                        gp.AddLine(CInt(X), CInt(Y + Height), CInt(X), CInt(Y))
                        gp.AddLine(CInt(X), CInt(Y), CInt(X + Width), CInt(Y + 0.3 * Height))
                    End If
                    gp.CloseFigure()
                    g.DrawPath(myPen, Me.GetRoundedLine(gp.PathPoints, 1))
                    Dim pgb1 As New LinearGradientBrush(New PointF(X, Y), New PointF(X, Y + Height), Me.GradientColor1, Me.GradientColor2)
                    If Me.Fill Then
                        If Me.GradientMode = False Then
                            g.FillPath(New SolidBrush(Me.FillColor), gp)
                        Else
                            g.FillPath(pgb1, gp)
                        End If
                    End If
                    gp.Dispose()
                    g.EndContainer(gContainer)
                Case ShapeIcon.Heater
                    Dim myPen As New Pen(Me.LineColor, Me.LineWidth)
                    Dim myPen2 As New Pen(Color.White, 0)
                    Dim rect As New Rectangle(X, Y, Width, Height)
                    Dim gp As Drawing2D.GraphicsPath = New Drawing2D.GraphicsPath
                    gp.AddLine(CInt(X), CInt(Y + 0.5 * Height), CInt(X + 0.5 * Width), CInt(Y))
                    gp.AddLine(CInt(X + 0.5 * Width), CInt(Y), CInt(X + Width), CInt(Y + 0.5 * Height))
                    gp.AddLine(CInt(X + Width), CInt(Y + 0.5 * Height), CInt(X + 0.5 * Width), CInt(Y + Height))
                    gp.AddLine(CInt(X + 0.5 * Width), CInt(Y + Height), CInt(X), CInt(Y + 0.5 * Height))
                    gp.CloseFigure()
                    g.SmoothingMode = SmoothingMode.AntiAlias
                    g.DrawPath(myPen, gp)
                    Dim strdist As SizeF = g.MeasureString(Me.Tag, New Font("Arial", 10, FontStyle.Bold, GraphicsUnit.Pixel, 0, False), New PointF(0, 0), New StringFormat(StringFormatFlags.NoClip, 0))
                    Dim strx As Single = (Me.Width - strdist.Width) / 2
                    'g.FillRectangle(Brushes.White, X + strx, Y + CSng(Height + 5), strdist.Width, strdist.Height)
                    g.DrawString(Me.Tag, New Font("Arial", 10, FontStyle.Bold, GraphicsUnit.Pixel, 0, False), New SolidBrush(Me.LineColor), X + strx, Y + Height + 5)
                    Dim pgb1 As New PathGradientBrush(gp)
                    pgb1.CenterColor = Me.GradientColor2
                    pgb1.SurroundColors = New Color() {Me.GradientColor1}
                    If Me.Fill Then
                        If Me.GradientMode = False Then
                            g.FillPath(New SolidBrush(Me.FillColor), gp)
                        Else
                            g.FillPath(pgb1, gp)
                        End If
                    End If
                    Dim size As SizeF
                    Dim fontA As New Font("Arial", 10, 3, GraphicsUnit.Pixel, 0, False)
                    size = g.MeasureString("A", fontA)
                    Dim ax, ay As Integer
                    ax = Me.X + (Me.Width - size.Width) / 2
                    ay = Me.Y + (Me.Height - size.Height) / 2
                    g.SmoothingMode = SmoothingMode.AntiAlias
                    g.TextRenderingHint = Text.TextRenderingHint.AntiAlias
                    g.DrawString("A", fontA, Brushes.DarkOrange, ax, ay)
                    gp.Dispose()
                    g.EndContainer(gContainer)
                Case ShapeIcon.HeatExchanger
                    Dim rect As New Rectangle(X, Y, Width, Height)
                    Dim path As New GraphicsPath()
                    path.AddEllipse(rect)
                    Dim pthGrBrush As New PathGradientBrush(path)
                    pthGrBrush.CenterColor = Me.GradientColor2
                    Dim colors As Color() = {Me.GradientColor1}
                    pthGrBrush.SurroundColors = colors
                    pthGrBrush.SetSigmaBellShape(1)
                    Dim myPen As New Pen(Me.LineColor, Me.LineWidth)
                    g.SmoothingMode = SmoothingMode.AntiAlias
                    g.DrawEllipse(myPen, rect)
                    If Me.Fill Then
                        If Me.GradientMode = False Then
                            g.FillEllipse(New SolidBrush(Me.FillColor), rect)
                        Else
                            g.FillEllipse(pthGrBrush, rect)
                        End If
                    End If
                    g.DrawLine(myPen, CInt(X), CInt(Y + Height), CInt(X + (2 / 8) * Width), CInt(Y + (3 / 8) * Height))
                    g.DrawLine(myPen, CInt(X + (2 / 8) * Width), CInt(Y + (3 / 8) * Height), CInt(X + (6 / 8) * Width), CInt(Y + (5 / 8) * Height))
                    g.DrawLine(myPen, CInt(X + (6 / 8) * Width), CInt(Y + (5 / 8) * Height), CInt(X + Width), CInt(Y))
                    g.TextRenderingHint = Text.TextRenderingHint.SystemDefault
                    Dim strdist As SizeF = g.MeasureString(Me.Tag, New Font("Arial", 10, FontStyle.Bold, GraphicsUnit.Pixel, 0, False), New PointF(0, 0), New StringFormat(StringFormatFlags.NoClip, 0))
                    Dim strx As Single = (Me.Width - strdist.Width) / 2
                    'g.FillRectangle(Brushes.White, X + strx, Y + CSng(Height + 5), strdist.Width, strdist.Height)
                    g.DrawString(Me.Tag, New Font("Arial", 10, FontStyle.Bold, GraphicsUnit.Pixel, 0, False), New SolidBrush(Me.LineColor), X + strx, Y + Height + 5)
                    g.EndContainer(gContainer)
                Case ShapeIcon.NodeIn
                    Dim myPenE As New Pen(Me.LineColor, Me.LineWidth)
                    Dim myPen2 As New Pen(Color.White, 0)
                    Dim rect As New Rectangle(X, Y, Width, Height)
                    Dim gp As Drawing2D.GraphicsPath = New Drawing2D.GraphicsPath
                    If Me.FlippedH Then
                        gp.AddLine(CInt(X), CInt(Y + 0.5 * Height), CInt(X + 0.5 * Width), CInt(Y))
                        gp.AddLine(CInt(X + 0.5 * Width), CInt(Y), CInt(X + Width), CInt(Y))
                        gp.AddLine(CInt(X + Width), CInt(Y), CInt(X + Width), CInt(Y + Height))
                        gp.AddLine(CInt(X + Width), CInt(Y + Height), CInt(X + 0.5 * Width), CInt(Y + Height))
                        gp.AddLine(CInt(X + 0.5 * Width), CInt(Y + Height), CInt(X), CInt(Y + 0.5 * Height))
                    Else
                        gp.AddLine(CInt(X + Width), CInt(Y + 0.5 * Height), CInt(X + 0.5 * Width), CInt(Y))
                        gp.AddLine(CInt(X + 0.5 * Width), CInt(Y), CInt(X), CInt(Y))
                        gp.AddLine(CInt(X), CInt(Y), CInt(X), CInt(Y + Height))
                        gp.AddLine(CInt(X), CInt(Y + Height), CInt(X + 0.5 * Width), CInt(Y + Height))
                        gp.AddLine(CInt(X + 0.5 * Width), CInt(Y + Height), CInt(X + Width), CInt(Y + 0.5 * Height))
                    End If
                    gp.CloseFigure()
                    g.SmoothingMode = SmoothingMode.AntiAlias
                    g.DrawPath(myPenE, Me.GetRoundedLine(gp.PathPoints, 1))
                    g.SmoothingMode = SmoothingMode.AntiAlias
                    Dim strdist As SizeF = g.MeasureString(Me.Tag, New Font("Arial", 10, FontStyle.Bold, GraphicsUnit.Pixel, 0, False), New PointF(0, 0), New StringFormat(StringFormatFlags.NoClip, 0))
                    Dim strx As Single = (Me.Width - strdist.Width) / 2
                    'g.FillRectangle(Brushes.White, X + strx, Y + CSng(Height + 5), strdist.Width, strdist.Height)
                    g.DrawString(Me.Tag, New Font("Arial", 10, FontStyle.Bold, GraphicsUnit.Pixel, 0, False), New SolidBrush(Me.LineColor), X + strx, Y + Height + 5)
                    Dim pgb1 As New PathGradientBrush(gp)
                    pgb1.CenterColor = Me.GradientColor2
                    pgb1.SurroundColors = New Color() {Me.GradientColor1}
                    If Me.Fill Then
                        If Me.GradientMode = False Then
                            g.FillPath(New SolidBrush(Me.FillColor), gp)
                        Else
                            g.FillPath(pgb1, gp)
                        End If
                    End If
                    g.EndContainer(gContainer)
                Case ShapeIcon.NodeOut
                    Dim myPenE As New Pen(Me.LineColor, Me.LineWidth)
                    Dim myPen2 As New Pen(Color.White, 0)
                    Dim rect As New Rectangle(X, Y, Width, Height)
                    Dim gp As Drawing2D.GraphicsPath = New Drawing2D.GraphicsPath
                    If Me.FlippedH Then
                        gp.AddLine(CInt(X + Width), CInt(Y + 0.5 * Height), CInt(X + 0.5 * Width), CInt(Y))
                        gp.AddLine(CInt(X + 0.5 * Width), CInt(Y), CInt(X), CInt(Y))
                        gp.AddLine(CInt(X), CInt(Y), CInt(X), CInt(Y + Height))
                        gp.AddLine(CInt(X), CInt(Y + Height), CInt(X + 0.5 * Width), CInt(Y + Height))
                        gp.AddLine(CInt(X + 0.5 * Width), CInt(Y + Height), CInt(X + Width), CInt(Y + 0.5 * Height))
                    Else
                        gp.AddLine(CInt(X), CInt(Y + 0.5 * Height), CInt(X + 0.5 * Width), CInt(Y))
                        gp.AddLine(CInt(X + 0.5 * Width), CInt(Y), CInt(X + Width), CInt(Y))
                        gp.AddLine(CInt(X + Width), CInt(Y), CInt(X + Width), CInt(Y + Height))
                        gp.AddLine(CInt(X + Width), CInt(Y + Height), CInt(X + 0.5 * Width), CInt(Y + Height))
                        gp.AddLine(CInt(X + 0.5 * Width), CInt(Y + Height), CInt(X), CInt(Y + 0.5 * Height))
                    End If
                    gp.CloseFigure()
                    g.SmoothingMode = SmoothingMode.AntiAlias
                    g.DrawPath(myPenE, Me.GetRoundedLine(gp.PathPoints, 1))
                    Dim strdist As SizeF = g.MeasureString(Me.Tag, New Font("Arial", 10, FontStyle.Bold, GraphicsUnit.Pixel, 0, False), New PointF(0, 0), New StringFormat(StringFormatFlags.NoClip, 0))
                    Dim strx As Single = (Me.Width - strdist.Width) / 2
                    'g.FillRectangle(Brushes.White, X + strx, Y + CSng(Height + 5), strdist.Width, strdist.Height)
                    g.DrawString(Me.Tag, New Font("Arial", 10, FontStyle.Bold, GraphicsUnit.Pixel, 0, False), New SolidBrush(Me.LineColor), X + strx, Y + Height + 5)
                    Dim pgb1 As New PathGradientBrush(gp)
                    pgb1.CenterColor = Me.GradientColor2
                    pgb1.SurroundColors = New Color() {Me.GradientColor1}
                    If Me.Fill Then
                        If Me.GradientMode = False Then
                            g.FillPath(New SolidBrush(Me.FillColor), gp)
                        Else
                            g.FillPath(pgb1, gp)
                        End If
                    End If
                    g.EndContainer(gContainer)
                Case ShapeIcon.OrificePlate
                    Dim myPen As New Pen(Me.LineColor, Me.LineWidth)
                    Dim myPen2 As New Pen(Color.White, 0)
                    Dim rect As New Rectangle(X, Y, Width, Height)
                    g.SmoothingMode = SmoothingMode.AntiAlias
                    Dim gp As Drawing2D.GraphicsPath = New Drawing2D.GraphicsPath
                    gp.AddEllipse(rect)
                    gp.AddEllipse(CInt(X + 0.3 * Width), CInt(Y + 0.3 * Height), CInt(0.4 * Width), CInt(0.4 * Height))
                    Dim rect2 As New Rectangle(X + 0.4 * Width, Y - 0.3 * Height, 0.2 * Width, 0.3 * Height)
                    gp.AddRectangle(rect2)
                    gp.CloseFigure()
                    g.DrawPath(myPen, gp)
                    Dim strdist As SizeF = g.MeasureString(Me.Tag, New Font("Arial", 10, FontStyle.Bold, GraphicsUnit.Pixel, 0, False), New PointF(0, 0), New StringFormat(StringFormatFlags.NoClip, 0))
                    Dim strx As Single = (Me.Width - strdist.Width) / 2
                    'g.FillRectangle(Brushes.White, X + strx, Y + CSng(Height + 5), strdist.Width, strdist.Height)
                    g.DrawString(Me.Tag, New Font("Arial", 10, FontStyle.Bold, GraphicsUnit.Pixel, 0, False), New SolidBrush(Me.LineColor), X + strx, Y + Height + 5)
                    Dim pgb1 As New PathGradientBrush(gp)
                    pgb1.CenterColor = Me.GradientColor1
                    pgb1.SurroundColors = New Color() {Me.GradientColor2}
                    pgb1.SetSigmaBellShape(0.6)
                    If Me.Fill Then
                        If Me.GradientMode = False Then
                            g.FillPath(New SolidBrush(Me.FillColor), gp)
                        Else
                            g.FillPath(pgb1, gp)
                        End If
                    End If
                    gp.Dispose()
                    g.EndContainer(gContainer)
                Case ShapeIcon.Pipe
                    Dim rect As New Rectangle(X, Y, Width, Height)
                    Dim lgb1 As New LinearGradientBrush(rect, Me.GradientColor1, Me.GradientColor2, LinearGradientMode.Vertical)
                    If Me.Fill Then
                        If Me.GradientMode = False Then
                            g.FillRectangle(New SolidBrush(Me.FillColor), rect)
                        Else
                            g.FillRectangle(lgb1, rect)
                        End If
                    End If
                    Dim myPen As New Pen(Me.LineColor, Me.LineWidth)
                    g.SmoothingMode = SmoothingMode.AntiAlias
                    g.DrawRectangle(myPen, rect)
                    Dim strdist As SizeF = g.MeasureString(Me.Tag, New Font("Arial", 10, FontStyle.Bold, GraphicsUnit.Pixel, 0, False), New PointF(0, 0), New StringFormat(StringFormatFlags.NoClip, 0))
                    Dim strx As Single = (Me.Width - strdist.Width) / 2
                    'g.FillRectangle(Brushes.White, X + strx, Y + CSng(Height + 5), strdist.Width, strdist.Height)
                    g.DrawString(Me.Tag, New Font("Arial", 10, FontStyle.Bold, GraphicsUnit.Pixel, 0, False), New SolidBrush(Me.LineColor), X + strx, Y + Height + 5)
                    g.EndContainer(gContainer)
                Case ShapeIcon.Pump
                    Dim rect1 As New RectangleF(X + 0.1 * Width, Y, 0.8 * Width, 0.8 * Height)
                    Dim pt3 As New PointF(X + 0.1 * Width, Y + Height)
                    Dim pt4 As New PointF(X + 0.2 * Width, Y + 0.65 * Height)
                    Dim pt5 As New PointF(X + 0.9 * Width, Y + Height)
                    Dim pt6 As New PointF(X + 0.8 * Width, Y + 0.65 * Height)
                    Dim pt7 As New PointF(X + 0.1 * Width, Y + Height)
                    Dim pt8 As New PointF(X + 0.9 * Width, Y + Height)
                    Dim pt9 As New PointF(X + 0.5 * Width, Y)
                    Dim pt10 As New PointF(X + Width, Y)
                    Dim pt11 As New PointF(X + Width, Y + 0.25 * Height)
                    Dim pt12 As New PointF(X + 0.88 * Width, Y + 0.25 * Height)
                    If Me.FlippedH Then
                        pt3 = New PointF(X + 0.9 * Width, Y + Height)
                        pt4 = New PointF(X + 0.8 * Width, Y + 0.65 * Height)
                        pt5 = New PointF(X + 0.1 * Width, Y + Height)
                        pt6 = New PointF(X + 0.2 * Width, Y + 0.65 * Height)
                        pt7 = New PointF(X + 0.9 * Width, Y + Height)
                        pt8 = New PointF(X + 0.1 * Width, Y + Height)
                        pt9 = New PointF(X + 0.5 * Width, Y)
                        pt10 = New PointF(X, Y)
                        pt11 = New PointF(X, Y + 0.25 * Height)
                        pt12 = New PointF(X + 0.12 * Width, Y + 0.25 * Height)
                    End If
                    Dim myPen As New Pen(Me.LineColor, Me.LineWidth)
                    Dim myPen2 As New Pen(Color.White, 0)
                    Dim rect As New Rectangle(X, Y, Width, Height)
                    g.SmoothingMode = SmoothingMode.AntiAlias
                    g.DrawEllipse(myPen, rect1)
                    g.DrawPolygon(myPen, New PointF() {pt3, pt4, pt6, pt5})
                    g.DrawPolygon(myPen, New PointF() {pt9, pt10, pt11, pt12})
                    Dim strdist As SizeF = g.MeasureString(Me.Tag, New Font("Arial", 10, FontStyle.Bold, GraphicsUnit.Pixel, 0, False), New PointF(0, 0), New StringFormat(StringFormatFlags.NoClip, 0))
                    Dim strx As Single = (Me.Width - strdist.Width) / 2
                    'g.FillRectangle(Brushes.White, X + strx, Y + CSng(Height + 5), strdist.Width, strdist.Height)
                    g.DrawString(Me.Tag, New Font("Arial", 10, FontStyle.Bold, GraphicsUnit.Pixel, 0, False), New SolidBrush(Me.LineColor), X + strx, Y + Height + 5)
                    Dim path As New GraphicsPath()
                    path.AddEllipse(rect1)
                    Dim pthGrBrush As New PathGradientBrush(path)
                    pthGrBrush.CenterColor = Me.GradientColor2
                    Dim colors As Color() = {Me.GradientColor1}
                    pthGrBrush.SurroundColors = colors
                    If Me.Fill Then
                        If Me.GradientMode = False Then
                            g.FillPolygon(New SolidBrush(Me.FillColor), New PointF() {pt3, pt4, pt6, pt5})
                            g.FillPolygon(New SolidBrush(Me.FillColor), New PointF() {pt9, pt10, pt11, pt12})
                            g.FillEllipse(New SolidBrush(Me.FillColor), rect1)
                        Else
                            g.FillPolygon(New SolidBrush(Me.GradientColor1), New PointF() {pt3, pt4, pt6, pt5})
                            g.FillPolygon(New SolidBrush(Me.GradientColor1), New PointF() {pt9, pt10, pt11, pt12})
                            g.FillEllipse(pthGrBrush, rect1)
                        End If
                    End If
                    g.EndContainer(gContainer)
                Case ShapeIcon.RCT_Conversion
                    Dim rect2 As New Rectangle(X + 0.123 * Width, Y + 0.5 * Height, 0.127 * Width, 0.127 * Height)
                    Dim rect3 As New Rectangle(X + 0.7 * Width, Y + 0.1 * Height, 0.127 * Width, 0.127 * Height)
                    Dim rect4 As New Rectangle(X + 0.7 * Width, Y + 0.773 * Height, 0.127 * Width, 0.127 * Height)
                    If Me.FlippedH = True Then
                        rect2 = New Rectangle(X + (1 - 0.123) * Width, Y + 0.5 * Height, 0.127 * Width, 0.127 * Height)
                        rect3 = New Rectangle(X + 0.3 * Width, Y + 0.1 * Height, 0.127 * Width, 0.127 * Height)
                        rect4 = New Rectangle(X + 0.3 * Width, Y + 0.773 * Height, 0.127 * Width, 0.127 * Height)
                    End If
                    Dim myPen As New Pen(Me.LineColor, Me.LineWidth)
                    Dim myPen2 As New Pen(Color.White, 0)
                    Dim rect As New Rectangle(X, Y, Width, Height)
                    g.SmoothingMode = SmoothingMode.AntiAlias
                    If Me.FlippedH = True Then
                        Me.DrawRoundRect(g, myPen, X + 0.4 * Width, Y, 0.45 * Width, Height, 10, Brushes.Transparent)
                    Else
                        Me.DrawRoundRect(g, myPen, X + 0.25 * Width, Y, 0.45 * Width, Height, 10, Brushes.Transparent)
                    End If
                    g.DrawRectangle(myPen, rect2)
                    g.DrawRectangle(myPen, rect3)
                    g.DrawRectangle(myPen, rect4)
                    Dim strdist As SizeF = g.MeasureString(Me.Tag, New Font("Arial", 10, FontStyle.Bold, GraphicsUnit.Pixel, 0, False), New PointF(0, 0), New StringFormat(StringFormatFlags.NoClip, 0))
                    Dim strx As Single = (Me.Width - strdist.Width) / 2
                    'g.FillRectangle(Brushes.White, X + strx, Y + CSng(Height + 5), strdist.Width, strdist.Height)
                    g.DrawString(Me.Tag, New Font("Arial", 10, FontStyle.Bold, GraphicsUnit.Pixel, 0, False), New SolidBrush(Me.LineColor), X + strx, Y + Height + 5)
                    Dim gp As Drawing2D.GraphicsPath = New Drawing2D.GraphicsPath
                    Dim radius As Integer = 3
                    gp.AddLine(X + radius, Y, X + Width - radius, Y)
                    gp.AddArc(X + Width - radius, Y, radius, radius, 270, 90)
                    gp.AddLine(X + Width, Y + radius, X + Width, Y + Height - radius)
                    gp.AddArc(X + Width - radius, Y + Height - radius, radius, radius, 0, 90)
                    gp.AddLine(X + Width - radius, Y + Height, X + radius, Y + Height)
                    gp.AddArc(X, Y + Height - radius, radius, radius, 90, 90)
                    gp.AddLine(X, Y + Height - radius, X, Y + radius)
                    gp.AddArc(X, Y, radius, radius, 180, 90)
                    Dim lgb1 As LinearGradientBrush
                    lgb1 = New LinearGradientBrush(rect, Me.GradientColor1, Me.GradientColor2, LinearGradientMode.Horizontal)
                    lgb1.SetBlendTriangularShape(0.5)
                    If Me.Fill Then
                        If Me.GradientMode = False Then
                            g.FillRectangle(New SolidBrush(Me.FillColor), rect3)
                            g.FillRectangle(New SolidBrush(Me.FillColor), rect4)
                            g.FillRectangle(New SolidBrush(Me.FillColor), rect2)
                            If Me.FlippedH = True Then
                                Me.DrawRoundRect(g, myPen, X + 0.4 * Width, Y, 0.45 * Width, Height, 6, New SolidBrush(Me.FillColor))
                            Else
                                Me.DrawRoundRect(g, myPen, X + 0.25 * Width, Y, 0.45 * Width, Height, 6, New SolidBrush(Me.FillColor))
                            End If
                        Else
                            g.FillRectangle(New SolidBrush(Me.FillColor), rect3)
                            g.FillRectangle(New SolidBrush(Me.FillColor), rect4)
                            g.FillRectangle(New SolidBrush(Me.FillColor), rect2)
                            If Me.FlippedH = True Then
                                Me.DrawRoundRect(g, myPen, X + 0.4 * Width, Y, 0.45 * Width, Height, 6, lgb1)
                            Else
                                Me.DrawRoundRect(g, myPen, X + 0.25 * Width, Y, 0.45 * Width, Height, 6, lgb1)
                            End If
                        End If
                    End If
                    If Me.FlippedH Then
                        g.DrawLines(myPen, New PointF() {New PointF(X + 0.4 * Width, Y + 0.3 * Height), New PointF(X + 0.85 * Width, Me.Y + 0.3 * Height)})
                        g.DrawLines(myPen, New PointF() {New PointF(X + 0.4 * Width, Y + 0.7 * Height), New PointF(X + 0.85 * Width, Me.Y + 0.7 * Height)})
                        g.DrawLines(myPen, New PointF() {New PointF(X + 0.4 * Width, Y + 0.3 * Height), New PointF(X + 0.85 * Width, Me.Y + 0.7 * Height)})
                        g.DrawLines(myPen, New PointF() {New PointF(X + 0.4 * Width, Y + 0.7 * Height), New PointF(X + 0.85 * Width, Me.Y + 0.3 * Height)})
                    Else
                        g.DrawLines(myPen, New PointF() {New PointF(X + 0.25 * Width, Y + 0.3 * Height), New PointF(X + 0.7 * Width, Me.Y + 0.3 * Height)})
                        g.DrawLines(myPen, New PointF() {New PointF(X + 0.25 * Width, Y + 0.7 * Height), New PointF(X + 0.7 * Width, Me.Y + 0.7 * Height)})
                        g.DrawLines(myPen, New PointF() {New PointF(X + 0.25 * Width, Y + 0.3 * Height), New PointF(X + 0.7 * Width, Me.Y + 0.7 * Height)})
                        g.DrawLines(myPen, New PointF() {New PointF(X + 0.25 * Width, Y + 0.7 * Height), New PointF(X + 0.7 * Width, Me.Y + 0.3 * Height)})
                    End If
                    Dim size As SizeF
                    Dim fontA As New Font("Arial", 8, FontStyle.Bold, GraphicsUnit.Pixel, 0, False)
                    size = g.MeasureString("C", fontA)
                    Dim ax, ay As Integer
                    If Me.FlippedH Then
                        ax = Me.X + 1.3 * (Me.Width - size.Width) / 2
                        ay = Me.Y + Me.Height - size.Height
                    Else
                        ax = Me.X + (Me.Width - size.Width) / 2
                        ay = Me.Y + Me.Height - size.Height
                    End If
                    g.SmoothingMode = SmoothingMode.AntiAlias
                    g.TextRenderingHint = Text.TextRenderingHint.AntiAlias
                    g.DrawString("C", fontA, New SolidBrush(Me.LineColor), ax, ay)
                    g.EndContainer(gContainer)
                    gp.Dispose()
                Case ShapeIcon.RCT_CSTR
                    Dim rect1 As New Rectangle(X + 0.1 * Width, Y + 0.1 * Height, 0.8 * Width, 0.8 * Height)
                    Dim rect2 As New RectangleF(X + 0.1 * Width, Y, 0.8 * Width, 0.2 * Height)
                    Dim rect3 As New RectangleF(X + 0.1 * Width, Y + 0.8 * Height, 0.8 * Width, 0.2 * Height)
                    Dim myPen As New Pen(Me.LineColor, Me.LineWidth)
                    Dim myPen2 As New Pen(Color.White, 0)
                    Dim rect As New Rectangle(X, Y, Width, Height)
                    g.SmoothingMode = SmoothingMode.AntiAlias
                    g.DrawEllipse(myPen, rect3)
                    g.DrawRectangle(myPen, rect1)
                    Dim strdist As SizeF = g.MeasureString(Me.Tag, New Font("Arial", 10, FontStyle.Bold, GraphicsUnit.Pixel, 0, False), New PointF(0, 0), New StringFormat(StringFormatFlags.NoClip, 0))
                    Dim strx As Single = (Me.Width - strdist.Width) / 2
                    'g.FillRectangle(Brushes.White, X + strx, Y + CSng(Height + 5), strdist.Width, strdist.Height)
                    g.DrawString(Me.Tag, New Font("Arial", 10, FontStyle.Bold, GraphicsUnit.Pixel, 0, False), New SolidBrush(Me.LineColor), X + strx, Y + Height + 5)
                    Dim lgb1 As New LinearGradientBrush(rect1, Me.GradientColor1, Me.GradientColor2, LinearGradientMode.Horizontal)
                    lgb1.SetBlendTriangularShape(0.5)
                    Dim lgb2 As New LinearGradientBrush(rect3, Me.GradientColor1, Me.GradientColor2, LinearGradientMode.Horizontal)
                    lgb2.SetBlendTriangularShape(0.5)
                    If Me.Fill Then
                        If Me.GradientMode = True Then
                            g.FillRectangle(lgb1, rect1)
                            g.FillEllipse(lgb1, rect2)
                            g.FillEllipse(lgb2, rect3)
                        Else
                            g.FillRectangle(New SolidBrush(Me.FillColor), rect1)
                            g.FillEllipse(New SolidBrush(Me.FillColor), rect2)
                            g.FillEllipse(New SolidBrush(Me.FillColor), rect3)
                        End If
                    End If
                    g.DrawEllipse(myPen, rect2)
                    If Me.Fill Then
                        If Me.GradientMode = True Then
                            g.FillEllipse(lgb1, rect2)
                        Else
                            g.FillEllipse(New SolidBrush(Me.FillColor), rect2)
                        End If
                    End If
                    g.DrawLines(myPen, New PointF() {New PointF(X + 0.5 * Width, Y - 0.1 * Height), New PointF(X + 0.5 * Width, Y + 0.7 * Height)})
                    g.DrawEllipse(myPen, New RectangleF(X + 0.2 * Width, Y + 0.6 * Height, 0.3 * Width, 0.1 * Height))
                    g.DrawEllipse(myPen, New RectangleF(X + 0.5 * Width, Y + 0.6 * Height, 0.3 * Width, 0.1 * Height))
                    g.EndContainer(gContainer)
                Case ShapeIcon.RCT_Equilibrium
                    Dim rect2 As New Rectangle(X + 0.123 * Width, Y + 0.5 * Height, 0.127 * Width, 0.127 * Height)
                    Dim rect3 As New Rectangle(X + 0.7 * Width, Y + 0.1 * Height, 0.127 * Width, 0.127 * Height)
                    Dim rect4 As New Rectangle(X + 0.7 * Width, Y + 0.773 * Height, 0.127 * Width, 0.127 * Height)
                    If Me.FlippedH = True Then
                        rect2 = New Rectangle(X + (1 - 0.123) * Width, Y + 0.5 * Height, 0.127 * Width, 0.127 * Height)
                        rect3 = New Rectangle(X + 0.3 * Width, Y + 0.1 * Height, 0.127 * Width, 0.127 * Height)
                        rect4 = New Rectangle(X + 0.3 * Width, Y + 0.773 * Height, 0.127 * Width, 0.127 * Height)
                    End If
                    Dim myPen As New Pen(Me.LineColor, Me.LineWidth)
                    Dim myPen2 As New Pen(Color.White, 0)
                    Dim rect As New Rectangle(X, Y, Width, Height)
                    g.SmoothingMode = SmoothingMode.AntiAlias
                    If Me.FlippedH = True Then
                        Me.DrawRoundRect(g, myPen, X + 0.4 * Width, Y, 0.45 * Width, Height, 10, Brushes.Transparent)
                    Else
                        Me.DrawRoundRect(g, myPen, X + 0.25 * Width, Y, 0.45 * Width, Height, 10, Brushes.Transparent)
                    End If
                    g.DrawRectangle(myPen, rect2)
                    g.DrawRectangle(myPen, rect3)
                    g.DrawRectangle(myPen, rect4)
                    Dim strdist As SizeF = g.MeasureString(Me.Tag, New Font("Arial", 10, FontStyle.Bold, GraphicsUnit.Pixel, 0, False), New PointF(0, 0), New StringFormat(StringFormatFlags.NoClip, 0))
                    Dim strx As Single = (Me.Width - strdist.Width) / 2
                    'g.FillRectangle(Brushes.White, X + strx, Y + CSng(Height + 5), strdist.Width, strdist.Height)
                    g.DrawString(Me.Tag, New Font("Arial", 10, FontStyle.Bold, GraphicsUnit.Pixel, 0, False), New SolidBrush(Me.LineColor), X + strx, Y + Height + 5)
                    Dim gp As Drawing2D.GraphicsPath = New Drawing2D.GraphicsPath
                    Dim radius As Integer = 3
                    gp.AddLine(X + radius, Y, X + Width - radius, Y)
                    gp.AddArc(X + Width - radius, Y, radius, radius, 270, 90)
                    gp.AddLine(X + Width, Y + radius, X + Width, Y + Height - radius)
                    gp.AddArc(X + Width - radius, Y + Height - radius, radius, radius, 0, 90)
                    gp.AddLine(X + Width - radius, Y + Height, X + radius, Y + Height)
                    gp.AddArc(X, Y + Height - radius, radius, radius, 90, 90)
                    gp.AddLine(X, Y + Height - radius, X, Y + radius)
                    gp.AddArc(X, Y, radius, radius, 180, 90)
                    Dim lgb1 As LinearGradientBrush
                    lgb1 = New LinearGradientBrush(rect, Me.GradientColor1, Me.GradientColor2, LinearGradientMode.Horizontal)
                    lgb1.SetBlendTriangularShape(0.5)
                    If Me.Fill Then
                        If Me.GradientMode = False Then
                            g.FillRectangle(New SolidBrush(Me.FillColor), rect3)
                            g.FillRectangle(New SolidBrush(Me.FillColor), rect4)
                            g.FillRectangle(New SolidBrush(Me.FillColor), rect2)
                            If Me.FlippedH = True Then
                                Me.DrawRoundRect(g, myPen, X + 0.4 * Width, Y, 0.45 * Width, Height, 6, New SolidBrush(Me.FillColor))
                            Else
                                Me.DrawRoundRect(g, myPen, X + 0.25 * Width, Y, 0.45 * Width, Height, 6, New SolidBrush(Me.FillColor))
                            End If
                        Else
                            g.FillRectangle(New SolidBrush(Me.FillColor), rect3)
                            g.FillRectangle(New SolidBrush(Me.FillColor), rect4)
                            g.FillRectangle(New SolidBrush(Me.FillColor), rect2)
                            If Me.FlippedH = True Then
                                Me.DrawRoundRect(g, myPen, X + 0.4 * Width, Y, 0.45 * Width, Height, 6, lgb1)
                            Else
                                Me.DrawRoundRect(g, myPen, X + 0.25 * Width, Y, 0.45 * Width, Height, 6, lgb1)
                            End If
                        End If
                    End If
                    If Me.FlippedH Then
                        g.DrawLines(myPen, New PointF() {New PointF(X + 0.4 * Width, Y + 0.3 * Height), New PointF(X + 0.85 * Width, Me.Y + 0.3 * Height)})
                        g.DrawLines(myPen, New PointF() {New PointF(X + 0.4 * Width, Y + 0.7 * Height), New PointF(X + 0.85 * Width, Me.Y + 0.7 * Height)})
                        g.DrawLines(myPen, New PointF() {New PointF(X + 0.4 * Width, Y + 0.3 * Height), New PointF(X + 0.85 * Width, Me.Y + 0.7 * Height)})
                        g.DrawLines(myPen, New PointF() {New PointF(X + 0.4 * Width, Y + 0.7 * Height), New PointF(X + 0.85 * Width, Me.Y + 0.3 * Height)})
                    Else
                        g.DrawLines(myPen, New PointF() {New PointF(X + 0.25 * Width, Y + 0.3 * Height), New PointF(X + 0.7 * Width, Me.Y + 0.3 * Height)})
                        g.DrawLines(myPen, New PointF() {New PointF(X + 0.25 * Width, Y + 0.7 * Height), New PointF(X + 0.7 * Width, Me.Y + 0.7 * Height)})
                        g.DrawLines(myPen, New PointF() {New PointF(X + 0.25 * Width, Y + 0.3 * Height), New PointF(X + 0.7 * Width, Me.Y + 0.7 * Height)})
                        g.DrawLines(myPen, New PointF() {New PointF(X + 0.25 * Width, Y + 0.7 * Height), New PointF(X + 0.7 * Width, Me.Y + 0.3 * Height)})
                    End If
                    Dim size As SizeF
                    Dim fontA As New Font("Arial", 8, FontStyle.Bold, GraphicsUnit.Pixel, 0, False)
                    size = g.MeasureString("E", fontA)
                    Dim ax, ay As Integer
                    If Me.FlippedH Then
                        ax = Me.X + 1.3 * (Me.Width - size.Width) / 2
                        ay = Me.Y + Me.Height - size.Height
                    Else
                        ax = Me.X + (Me.Width - size.Width) / 2
                        ay = Me.Y + Me.Height - size.Height
                    End If
                    g.SmoothingMode = SmoothingMode.AntiAlias
                    g.TextRenderingHint = Text.TextRenderingHint.AntiAlias
                    g.DrawString("E", fontA, New SolidBrush(Me.LineColor), ax, ay)
                    g.EndContainer(gContainer)
                    gp.Dispose()
                Case ShapeIcon.RCT_Gibbs
                    Dim rect2 As New Rectangle(X + 0.123 * Width, Y + 0.5 * Height, 0.127 * Width, 0.127 * Height)
                    Dim rect3 As New Rectangle(X + 0.7 * Width, Y + 0.1 * Height, 0.127 * Width, 0.127 * Height)
                    Dim rect4 As New Rectangle(X + 0.7 * Width, Y + 0.773 * Height, 0.127 * Width, 0.127 * Height)
                    If Me.FlippedH = True Then
                        rect2 = New Rectangle(X + (1 - 0.123) * Width, Y + 0.5 * Height, 0.127 * Width, 0.127 * Height)
                        rect3 = New Rectangle(X + 0.3 * Width, Y + 0.1 * Height, 0.127 * Width, 0.127 * Height)
                        rect4 = New Rectangle(X + 0.3 * Width, Y + 0.773 * Height, 0.127 * Width, 0.127 * Height)
                    End If
                    Dim myPen As New Pen(Me.LineColor, Me.LineWidth)
                    Dim myPen2 As New Pen(Color.White, 0)
                    Dim rect As New Rectangle(X, Y, Width, Height)
                    g.SmoothingMode = SmoothingMode.AntiAlias
                    If Me.FlippedH = True Then
                        Me.DrawRoundRect(g, myPen, X + 0.4 * Width, Y, 0.45 * Width, Height, 10, Brushes.Transparent)
                    Else
                        Me.DrawRoundRect(g, myPen, X + 0.25 * Width, Y, 0.45 * Width, Height, 10, Brushes.Transparent)
                    End If
                    g.DrawRectangle(myPen, rect2)
                    g.DrawRectangle(myPen, rect3)
                    g.DrawRectangle(myPen, rect4)
                    Dim strdist As SizeF = g.MeasureString(Me.Tag, New Font("Arial", 10, FontStyle.Bold, GraphicsUnit.Pixel, 0, False), New PointF(0, 0), New StringFormat(StringFormatFlags.NoClip, 0))
                    Dim strx As Single = (Me.Width - strdist.Width) / 2
                    'g.FillRectangle(Brushes.White, X + strx, Y + CSng(Height + 5), strdist.Width, strdist.Height)
                    g.DrawString(Me.Tag, New Font("Arial", 10, FontStyle.Bold, GraphicsUnit.Pixel, 0, False), New SolidBrush(Me.LineColor), X + strx, Y + Height + 5)
                    Dim gp As Drawing2D.GraphicsPath = New Drawing2D.GraphicsPath
                    Dim radius As Integer = 3
                    gp.AddLine(X + radius, Y, X + Width - radius, Y)
                    gp.AddArc(X + Width - radius, Y, radius, radius, 270, 90)
                    gp.AddLine(X + Width, Y + radius, X + Width, Y + Height - radius)
                    gp.AddArc(X + Width - radius, Y + Height - radius, radius, radius, 0, 90)
                    gp.AddLine(X + Width - radius, Y + Height, X + radius, Y + Height)
                    gp.AddArc(X, Y + Height - radius, radius, radius, 90, 90)
                    gp.AddLine(X, Y + Height - radius, X, Y + radius)
                    gp.AddArc(X, Y, radius, radius, 180, 90)
                    Dim lgb1 As LinearGradientBrush
                    lgb1 = New LinearGradientBrush(rect, Me.GradientColor1, Me.GradientColor2, LinearGradientMode.Horizontal)
                    lgb1.SetBlendTriangularShape(0.5)
                    If Me.Fill Then
                        If Me.GradientMode = False Then
                            g.FillRectangle(New SolidBrush(Me.FillColor), rect3)
                            g.FillRectangle(New SolidBrush(Me.FillColor), rect4)
                            g.FillRectangle(New SolidBrush(Me.FillColor), rect2)
                            If Me.FlippedH = True Then
                                Me.DrawRoundRect(g, myPen, X + 0.4 * Width, Y, 0.45 * Width, Height, 6, New SolidBrush(Me.FillColor))
                            Else
                                Me.DrawRoundRect(g, myPen, X + 0.25 * Width, Y, 0.45 * Width, Height, 6, New SolidBrush(Me.FillColor))
                            End If
                        Else
                            g.FillRectangle(New SolidBrush(Me.FillColor), rect3)
                            g.FillRectangle(New SolidBrush(Me.FillColor), rect4)
                            g.FillRectangle(New SolidBrush(Me.FillColor), rect2)
                            If Me.FlippedH = True Then
                                Me.DrawRoundRect(g, myPen, X + 0.4 * Width, Y, 0.45 * Width, Height, 6, lgb1)
                            Else
                                Me.DrawRoundRect(g, myPen, X + 0.25 * Width, Y, 0.45 * Width, Height, 6, lgb1)
                            End If
                        End If
                    End If
                    If Me.FlippedH Then
                        g.DrawLines(myPen, New PointF() {New PointF(X + 0.4 * Width, Y + 0.3 * Height), New PointF(X + 0.85 * Width, Me.Y + 0.3 * Height)})
                        g.DrawLines(myPen, New PointF() {New PointF(X + 0.4 * Width, Y + 0.7 * Height), New PointF(X + 0.85 * Width, Me.Y + 0.7 * Height)})
                        g.DrawLines(myPen, New PointF() {New PointF(X + 0.4 * Width, Y + 0.3 * Height), New PointF(X + 0.85 * Width, Me.Y + 0.7 * Height)})
                        g.DrawLines(myPen, New PointF() {New PointF(X + 0.4 * Width, Y + 0.7 * Height), New PointF(X + 0.85 * Width, Me.Y + 0.3 * Height)})
                    Else
                        g.DrawLines(myPen, New PointF() {New PointF(X + 0.25 * Width, Y + 0.3 * Height), New PointF(X + 0.7 * Width, Me.Y + 0.3 * Height)})
                        g.DrawLines(myPen, New PointF() {New PointF(X + 0.25 * Width, Y + 0.7 * Height), New PointF(X + 0.7 * Width, Me.Y + 0.7 * Height)})
                        g.DrawLines(myPen, New PointF() {New PointF(X + 0.25 * Width, Y + 0.3 * Height), New PointF(X + 0.7 * Width, Me.Y + 0.7 * Height)})
                        g.DrawLines(myPen, New PointF() {New PointF(X + 0.25 * Width, Y + 0.7 * Height), New PointF(X + 0.7 * Width, Me.Y + 0.3 * Height)})
                    End If
                    Dim size As SizeF
                    Dim fontA As New Font("Arial", 8, FontStyle.Bold, GraphicsUnit.Pixel, 0, False)
                    size = g.MeasureString("G", fontA)
                    Dim ax, ay As Integer
                    If Me.FlippedH Then
                        ax = Me.X + 1.3 * (Me.Width - size.Width) / 2
                        ay = Me.Y + Me.Height - size.Height
                    Else
                        ax = Me.X + (Me.Width - size.Width) / 2
                        ay = Me.Y + Me.Height - size.Height
                    End If
                    g.SmoothingMode = SmoothingMode.AntiAlias
                    g.TextRenderingHint = Text.TextRenderingHint.AntiAlias
                    g.DrawString("G", fontA, New SolidBrush(Me.LineColor), ax, ay)
                    g.EndContainer(gContainer)
                    gp.Dispose()
                Case ShapeIcon.RCT_PFR
                    Dim rect As New Rectangle(X, Y, Width, Height)
                    Dim lgb1 As New LinearGradientBrush(rect, Me.GradientColor1, Me.GradientColor2, LinearGradientMode.Vertical)
                    If Me.Fill Then
                        If Me.GradientMode = False Then
                            g.FillRectangle(New SolidBrush(Me.FillColor), rect)
                        Else
                            g.FillRectangle(lgb1, rect)
                        End If
                    End If
                    Dim myPen As New Pen(Me.LineColor, Me.LineWidth)
                    g.SmoothingMode = SmoothingMode.AntiAlias
                    g.DrawRectangle(myPen, rect)
                    Dim strdist As SizeF = g.MeasureString(Me.Tag, New Font("Arial", 10, FontStyle.Bold, GraphicsUnit.Pixel, 0, False), New PointF(0, 0), New StringFormat(StringFormatFlags.NoClip, 0))
                    Dim strx As Single = (Me.Width - strdist.Width) / 2
                    'g.FillRectangle(Brushes.White, X + strx, Y + CSng(Height + 5), strdist.Width, strdist.Height)
                    g.DrawString(Me.Tag, New Font("Arial", 10, FontStyle.Bold, GraphicsUnit.Pixel, 0, False), New SolidBrush(Me.LineColor), X + strx, Y + Height + 5)
                    Dim rec1 As New Rectangle(X + 0.1 * Width, Y, 0.8 * Width, Height)
                    g.FillRectangle(New HatchBrush(HatchStyle.SmallCheckerBoard, Me.LineColor, Color.Transparent), rec1)
                    g.DrawRectangle(myPen, rec1)
                    g.EndContainer(gContainer)
                Case ShapeIcon.Tank
                    Dim rect1 As New Rectangle(X + 0.1 * Width, Y + 0.1 * Height, 0.8 * Width, 0.8 * Height)
                    Dim rect2 As New RectangleF(X + 0.1 * Width, Y, 0.8 * Width, 0.2 * Height)
                    Dim rect3 As New RectangleF(X + 0.1 * Width, Y + 0.8 * Height, 0.8 * Width, 0.2 * Height)
                    Dim myPen As New Pen(Me.LineColor, Me.LineWidth)
                    Dim myPen2 As New Pen(Color.White, 0)
                    Dim rect As New Rectangle(X, Y, Width, Height)
                    g.SmoothingMode = SmoothingMode.AntiAlias
                    g.DrawEllipse(myPen, rect2)
                    g.DrawEllipse(myPen, rect3)
                    g.DrawRectangle(myPen, rect1)
                    Dim strdist As SizeF = g.MeasureString(Me.Tag, New Font("Arial", 10, FontStyle.Bold, GraphicsUnit.Pixel, 0, False), New PointF(0, 0), New StringFormat(StringFormatFlags.NoClip, 0))
                    Dim strx As Single = (Me.Width - strdist.Width) / 2
                    'g.FillRectangle(Brushes.White, X + strx, Y + CSng(Height + 5), strdist.Width, strdist.Height)
                    g.DrawString(Me.Tag, New Font("Arial", 10, FontStyle.Bold, GraphicsUnit.Pixel, 0, False), New SolidBrush(Me.LineColor), X + strx, Y + Height + 5)
                    Dim lgb1 As New LinearGradientBrush(rect1, Me.GradientColor1, Me.GradientColor2, LinearGradientMode.Horizontal)
                    lgb1.SetBlendTriangularShape(0.5)
                    Dim lgb2 As New LinearGradientBrush(rect3, Me.GradientColor1, Me.GradientColor2, LinearGradientMode.Horizontal)
                    lgb2.SetBlendTriangularShape(0.5)
                    If Me.Fill Then
                        If Me.GradientMode = True Then
                            g.FillRectangle(lgb1, rect1)
                            g.FillEllipse(New SolidBrush(Me.FillColor), rect2)
                            g.FillEllipse(lgb2, rect3)
                        Else
                            g.FillRectangle(New SolidBrush(Me.FillColor), rect1)
                            g.FillEllipse(New SolidBrush(Me.FillColor), rect2)
                            g.FillEllipse(New SolidBrush(Me.FillColor), rect3)
                        End If
                    End If
                    g.EndContainer(gContainer)
                Case ShapeIcon.Valve
                    Dim myPen As New Pen(Me.LineColor, Me.LineWidth)
                    Dim myPen2 As New Pen(Color.White, 0)
                    Dim rect As New Rectangle(X, Y, Width, Height)
                    g.SmoothingMode = SmoothingMode.AntiAlias
                    Dim gp As Drawing2D.GraphicsPath = New Drawing2D.GraphicsPath
                    gp.AddLine(CInt(X), CInt(Y + 0.2 * Height), CInt(X + 0.5 * Width), CInt(Y + 0.5 * Height))
                    gp.AddLine(CInt(X + 0.5 * Width), CInt(Y + 0.5 * Height), CInt(X + Width), CInt(Y + 0.2 * Height))
                    gp.AddLine(CInt(X + Width), CInt(Y + 0.2 * Height), CInt(X + Width), CInt(Y + 0.8 * Height))
                    gp.AddLine(CInt(X + Width), CInt(Y + 0.8 * Height), CInt(X + 0.5 * Width), CInt(Y + 0.5 * Height))
                    gp.AddLine(CInt(X + 0.5 * Width), CInt(Y + 0.5 * Height), CInt(X), CInt(Y + 0.8 * Height))
                    gp.AddLine(CInt(X), CInt(Y + 0.8 * Height), CInt(X), CInt(Y + 0.2 * Height))
                    gp.CloseFigure()
                    g.DrawPath(myPen, Me.GetRoundedLine(gp.PathPoints, 1))
                    Dim strdist As SizeF = g.MeasureString(Me.Tag, New Font("Arial", 10, FontStyle.Bold, GraphicsUnit.Pixel, 0, False), New PointF(0, 0), New StringFormat(StringFormatFlags.NoClip, 0))
                    Dim strx As Single = (Me.Width - strdist.Width) / 2
                    'g.FillRectangle(Brushes.White, X + strx, Y + CSng(Height + 5), strdist.Width, strdist.Height)
                    g.DrawString(Me.Tag, New Font("Arial", 10, FontStyle.Bold, GraphicsUnit.Pixel, 0, False), New SolidBrush(Me.LineColor), X + strx, Y + Height + 5)
                    Dim pgb1 As New PathGradientBrush(gp)
                    pgb1.CenterColor = Me.GradientColor1
                    pgb1.SurroundColors = New Color() {Me.GradientColor2}
                    If Me.Fill Then
                        If Me.GradientMode = False Then
                            g.FillPath(New SolidBrush(Me.FillColor), gp)
                        Else
                            g.FillPath(pgb1, gp)
                        End If
                    End If
                    gp.Dispose()
                    g.EndContainer(gContainer)
                Case ShapeIcon.Vessel
                    Dim rect2 As New Rectangle(X + 0.123 * Width, Y + 0.5 * Height, 0.127 * Width, 0.127 * Height)
                    Dim rect3 As New Rectangle(X + 0.7 * Width, Y + 0.1 * Height, 0.127 * Width, 0.127 * Height)
                    Dim rect4 As New Rectangle(X + 0.7 * Width, Y + 0.773 * Height, 0.127 * Width, 0.127 * Height)
                    If Me.FlippedH = True Then
                        rect2 = New Rectangle(X + (1 - 0.123) * Width, Y + 0.5 * Height, 0.127 * Width, 0.127 * Height)
                        rect3 = New Rectangle(X + 0.3 * Width, Y + 0.1 * Height, 0.127 * Width, 0.127 * Height)
                        rect4 = New Rectangle(X + 0.3 * Width, Y + 0.773 * Height, 0.127 * Width, 0.127 * Height)
                    End If
                    Dim myPen As New Pen(Me.LineColor, Me.LineWidth)
                    Dim myPen2 As New Pen(Color.White, 0)
                    Dim rect As New Rectangle(X, Y, Width, Height)
                    g.SmoothingMode = SmoothingMode.AntiAlias
                    If Me.FlippedH = True Then
                        Me.DrawRoundRect(g, myPen, X + 0.4 * Width, Y, 0.45 * Width, Height, 10, Brushes.Transparent)
                    Else
                        Me.DrawRoundRect(g, myPen, X + 0.25 * Width, Y, 0.45 * Width, Height, 10, Brushes.Transparent)
                    End If
                    g.DrawRectangle(myPen, rect2)
                    g.DrawRectangle(myPen, rect3)
                    g.DrawRectangle(myPen, rect4)
                    Dim strdist As SizeF = g.MeasureString(Me.Tag, New Font("Arial", 10, FontStyle.Bold, GraphicsUnit.Pixel, 0, False), New PointF(0, 0), New StringFormat(StringFormatFlags.NoClip, 0))
                    Dim strx As Single = (Me.Width - strdist.Width) / 2
                    'g.FillRectangle(Brushes.White, X + strx, Y + CSng(Height + 5), strdist.Width, strdist.Height)
                    g.DrawString(Me.Tag, New Font("Arial", 10, FontStyle.Bold, GraphicsUnit.Pixel, 0, False), New SolidBrush(Me.LineColor), X + strx, Y + Height + 5)
                    Dim gp As Drawing2D.GraphicsPath = New Drawing2D.GraphicsPath
                    Dim radius As Integer = 3
                    gp.AddLine(X + radius, Y, X + Width - radius, Y)
                    gp.AddArc(X + Width - radius, Y, radius, radius, 270, 90)
                    gp.AddLine(X + Width, Y + radius, X + Width, Y + Height - radius)
                    gp.AddArc(X + Width - radius, Y + Height - radius, radius, radius, 0, 90)
                    gp.AddLine(X + Width - radius, Y + Height, X + radius, Y + Height)
                    gp.AddArc(X, Y + Height - radius, radius, radius, 90, 90)
                    gp.AddLine(X, Y + Height - radius, X, Y + radius)
                    gp.AddArc(X, Y, radius, radius, 180, 90)
                    Dim lgb1 As LinearGradientBrush
                    lgb1 = New LinearGradientBrush(rect, Me.GradientColor1, Me.GradientColor2, LinearGradientMode.Horizontal)
                    lgb1.SetBlendTriangularShape(0.5)
                    If Me.Fill Then
                        If Me.GradientMode = False Then
                            g.FillRectangle(New SolidBrush(Me.FillColor), rect3)
                            g.FillRectangle(New SolidBrush(Me.FillColor), rect4)
                            g.FillRectangle(New SolidBrush(Me.FillColor), rect2)
                            If Me.FlippedH = True Then
                                Me.DrawRoundRect(g, myPen, X + 0.4 * Width, Y, 0.45 * Width, Height, 6, New SolidBrush(Me.FillColor))
                            Else
                                Me.DrawRoundRect(g, myPen, X + 0.25 * Width, Y, 0.45 * Width, Height, 6, New SolidBrush(Me.FillColor))
                            End If
                        Else
                            g.FillRectangle(New SolidBrush(Me.FillColor), rect3)
                            g.FillRectangle(New SolidBrush(Me.FillColor), rect4)
                            g.FillRectangle(New SolidBrush(Me.FillColor), rect2)
                            If Me.FlippedH = True Then
                                Me.DrawRoundRect(g, myPen, X + 0.4 * Width, Y, 0.45 * Width, Height, 6, lgb1)
                            Else
                                Me.DrawRoundRect(g, myPen, X + 0.25 * Width, Y, 0.45 * Width, Height, 6, lgb1)
                            End If
                        End If
                    End If
                    g.EndContainer(gContainer)
                    gp.Dispose()
            End Select

        End Sub

        Public Sub DrawRoundRect(ByVal g As Graphics, ByVal p As Pen, ByVal x As Integer, ByVal y As Integer, ByVal width As Integer, ByVal height As Integer, ByVal radius As Integer, ByVal myBrush As Brush)

            Dim gp As Drawing2D.GraphicsPath = New Drawing2D.GraphicsPath

            gp.AddLine(x + radius, y, x + width - radius, y)
            gp.AddArc(x + width - radius, y, radius, radius, 270, 90)
            gp.AddLine(x + width, y + radius, x + width, y + height - radius)
            gp.AddArc(x + width - radius, y + height - radius, radius, radius, 0, 90)
            gp.AddLine(x + width - radius, y + height, x + radius, y + height)
            gp.AddArc(x, y + height - radius, radius, radius, 90, 90)
            gp.AddLine(x, y + height - radius, x, y + radius)
            gp.AddArc(x, y, radius, radius, 180, 90)

            gp.CloseFigure()

            g.DrawPath(p, gp)
            g.FillPath(myBrush, gp)

            gp.Dispose()

        End Sub

    End Class

End Namespace