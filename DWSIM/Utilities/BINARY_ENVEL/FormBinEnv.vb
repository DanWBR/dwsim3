Imports DWSIM.DWSIM.ClassesBasicasTermodinamica
Imports DWSIM.DWSIM.SimulationObjects.Streams

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

Imports DWSIM.DWSIM.SimulationObjects.PropertyPackages

Public Class FormBinEnv

    Inherits System.Windows.Forms.Form

    Dim mat As DWSIM.SimulationObjects.Streams.MaterialStream
    Dim Frm As FormFlowsheet

    Public su As New DWSIM.SistemasDeUnidades.Unidades
    Public cv As New DWSIM.SistemasDeUnidades.Conversor
    Public nf As String
    Dim mw1, mw2 As Double

    Private loaded As Boolean = False

    Dim fpec As FormPEC

    Dim P, T As Double

    Private Sub FormBinEnv_Load(ByVal sender As System.Object, ByVal e As System.EventArgs) Handles MyBase.Load

        Me.Text = DWSIM.App.GetLocalString("DWSIMUtilitriosDiagr")

        Me.Frm = My.Application.ActiveSimulation

        If Me.Frm.Options.SelectedComponents.Count > 1 Then

            Me.su = Frm.Options.SelectedUnitSystem
            Me.nf = Frm.Options.NumberFormat

            Me.cbComp1.Items.Clear()
            Me.cbComp2.Items.Clear()
            For Each co As ConstantProperties In Frm.Options.SelectedComponents.Values
                Me.cbComp1.Items.Add(DWSIM.App.GetComponentName(co.Name))
                Me.cbComp2.Items.Add(DWSIM.App.GetComponentName(co.Name))
            Next

            Me.cbPropPack.Items.Clear()
            For Each pp As propertypackage In Me.Frm.Options.PropertyPackages.Values
                Me.cbPropPack.Items.Add(pp.Tag & " (" & pp.ComponentName & ")" & " [" & pp.UniqueID & "]")
            Next

            If Me.cbPropPack.Items.Count > 0 Then Me.cbPropPack.SelectedIndex = 0

            If Me.cbComp1.Items.Count > 0 Then Me.cbComp1.SelectedIndex = 0
            If Me.cbComp2.Items.Count > 1 Then Me.cbComp2.SelectedIndex = 1

            cbXAxisBasis.SelectedIndex = 0

            Me.lblP.Text = su.spmp_pressure
            Me.lblT.Text = su.spmp_temperature

            Me.tbP.Text = Format(cv.ConverterDoSI(su.spmp_pressure, 101325), nf)
            Me.tbT.Text = Format(cv.ConverterDoSI(su.spmp_temperature, 298.15), nf)

        Else

            MessageBox.Show(DWSIM.App.GetLocalString("BinEnvError_TwoCompoundsMinimum"), DWSIM.App.GetLocalString("Erro"), MessageBoxButtons.OK, MessageBoxIcon.Error)
            Me.Close()

        End If


    End Sub

    Private Sub Button1_Click(ByVal sender As System.Object, ByVal e As System.EventArgs) Handles Button1.Click

        If Me.cbComp1.SelectedItem.ToString <> Me.cbComp2.SelectedItem.ToString Then

            With Me.GraphControl.GraphPane.Legend
                .Position = ZedGraph.LegendPos.TopCenter
                .Border.IsVisible = False
                .FontSpec.Size = 10
                .FontSpec.IsDropShadow = False
            End With

            Me.Enabled = False

            Me.mat = New MaterialStream("", "")

            For Each phase As DWSIM.ClassesBasicasTermodinamica.Fase In mat.Fases.Values
                For Each cp As ConstantProperties In Me.Frm.Options.SelectedComponents.Values
                    If DWSIM.App.GetComponentName(cp.Name) = cbComp1.SelectedItem.ToString Then
                        With phase
                            .Componentes.Add(cp.Name, New DWSIM.ClassesBasicasTermodinamica.Substancia(cp.Name, ""))
                            .Componentes(cp.Name).ConstantProperties = cp
                            mw1 = cp.Molar_Weight
                        End With
                        Exit For
                    End If
                Next
            Next

            For Each phase As DWSIM.ClassesBasicasTermodinamica.Fase In mat.Fases.Values
                For Each cp As ConstantProperties In Me.Frm.Options.SelectedComponents.Values
                    If DWSIM.App.GetComponentName(cp.Name) = cbComp2.SelectedItem.ToString Then
                        With phase
                            .Componentes.Add(cp.Name, New DWSIM.ClassesBasicasTermodinamica.Substancia(cp.Name, ""))
                            .Componentes(cp.Name).ConstantProperties = cp
                            mw2 = cp.Molar_Weight
                        End With
                        Exit For
                    End If
                Next
            Next

            P = cv.ConverterParaSI(su.spmp_pressure, tbP.Text)
            T = cv.ConverterParaSI(su.spmp_temperature, tbT.Text)

            Dim tipocalc As String = ""
            If Me.RadioButton1.Checked Then
                tipocalc = "T-x-y"
            ElseIf Me.RadioButton2.Checked Then
                tipocalc = "P-x-y"
            ElseIf Me.RadioButton3.Checked Then
                tipocalc = "(T)x-y"
            ElseIf Me.RadioButton4.Checked Then
                tipocalc = "(P)x-y"
            End If

            Me.BackgroundWorker1.RunWorkerAsync(New Object() {tipocalc, P, T})

            fpec = New FormPEC
            Try
                fpec.ShowDialog(Me)
            Catch ex As Exception
                fpec.Close()
                Throw ex
            End Try

        Else

            MessageBox.Show(DWSIM.App.GetLocalString("BinEnvError_DuplicateCompound"), DWSIM.App.GetLocalString("Erro"), MessageBoxButtons.OK, MessageBoxIcon.Error)

        End If

    End Sub

    Private Sub BackgroundWorker1_DoWork(ByVal sender As Object, ByVal e As System.ComponentModel.DoWorkEventArgs) Handles BackgroundWorker1.DoWork

        Dim pp As DWSIM.SimulationObjects.PropertyPackages.PropertyPackage
        Dim pp2 As New DWSIM.SimulationObjects.PropertyPackages.RaoultPropertyPackage()
        Dim uniqueID As String = cbPropPack.SelectedItem.ToString.Substring(cbPropPack.SelectedItem.ToString.IndexOf("[") + 1, cbPropPack.SelectedItem.ToString.Length - cbPropPack.SelectedItem.ToString.IndexOf("[") - 2)
        pp = Me.Frm.Options.PropertyPackages(uniqueID)
        mat.SetFlowsheet(Me.Frm)
        pp.CurrentMaterialStream = mat
        pp2.CurrentMaterialStream = mat
        e.Result = New Object() {pp.DW_ReturnBinaryEnvelope(e.Argument), pp2.DW_ReturnBinaryEnvelope(e.Argument)}

    End Sub

    Private Sub BackgroundWorker1_RunWorkerCompleted(ByVal sender As Object, ByVal e As System.ComponentModel.RunWorkerCompletedEventArgs) Handles BackgroundWorker1.RunWorkerCompleted

        Me.Enabled = True

        fpec.Close()

        Dim r = e.Result(0)
        Dim r2 = e.Result(1)

        Dim c(1) As String
        c(0) = cbComp1.SelectedItem.ToString
        c(1) = cbComp2.SelectedItem.ToString

        Dim i As Integer

        'convert x axis
        Select Case cbXAxisBasis.SelectedIndex
            Case 0
            Case 1
                For i = 0 To r(0).count - 1
                    r(0)(i) = r(0)(i) * mw1 / (r(0)(i) * mw1 + (1 - r(0)(i)) * mw2)
                Next
                For i = 0 To r(0).count - 1
                    r2(0)(i) = r2(0)(i) * mw1 / (r2(0)(i) * mw1 + (1 - r2(0)(i)) * mw2)
                Next
                For i = 0 To r(0).count - 1
                    r(3)(i) = r(3)(i) * mw1 / (r(3)(i) * mw1 + (1 - r(3)(i)) * mw2)
                Next
                For i = 0 To r(0).count - 1
                    r(4)(i) = r(4)(i) * mw1 / (r(4)(i) * mw1 + (1 - r(4)(i)) * mw2)
                Next
            Case 2
                For i = 0 To r(0).count - 1
                    r(0)(i) = r(0)(i) * 100
                Next
                For i = 0 To r(0).count - 1
                    r2(0)(i) = r2(0)(i) * 100
                Next
                For i = 0 To r(0).count - 1
                    r(3)(i) = r(3)(i) * 100
                Next
                For i = 0 To r(0).count - 1
                    r(4)(i) = r(4)(i) * 100
                Next
            Case 3
                For i = 0 To r(0).count - 1
                    r(0)(i) = r(0)(i) * mw1 / (r(0)(i) * mw1 + (1 - r(0)(i)) * mw2) * 100
                Next
                For i = 0 To r(0).count - 1
                    r2(0)(i) = r2(0)(i) * mw1 / (r2(0)(i) * mw1 + (1 - r2(0)(i)) * mw2) * 100
                Next
                For i = 0 To r(0).count - 1
                    r(3)(i) = r(3)(i) * mw1 / (r(3)(i) * mw1 + (1 - r(3)(i)) * mw2) * 100
                Next
                For i = 0 To r(0).count - 1
                    r(4)(i) = r(4)(i) * mw1 / (r(4)(i) * mw1 + (1 - r(4)(i)) * mw2) * 100
                Next
        End Select

        If Me.RadioButton1.Checked Then

            Dim px, py1, py2, pxi, py1i, py2i, px1l1, px1l2, py3 As New ArrayList
            px = r(0)
            py1 = r(1)
            py2 = r(2)
            pxi = r2(0)
            py1i = r2(1)
            py2i = r2(2)

            px1l1 = r(3)
            px1l2 = r(4)
            py3 = r(5)

            Dim vx1, vx2, vy1, vy2, vxi, vy1i, vy2i, vx1l1, vx1l2, vy3 As New ArrayList

            i = 0
            Do
                If py1(i) <> 0.0# Then
                    vx1.Add(px(i))
                    vy1.Add(cv.ConverterDoSI(su.spmp_temperature, py1(i)))
                End If
                If py2(i) <> 0.0# Then
                    vx2.Add(px(i))
                    vy2.Add(cv.ConverterDoSI(su.spmp_temperature, py2(i)))
                End If
                i += 1
            Loop Until i = px.Count

            i = 0
            Do
                vxi.Add(pxi(i))
                vy1i.Add(cv.ConverterDoSI(su.spmp_temperature, py1i(i)))
                vy2i.Add(cv.ConverterDoSI(su.spmp_temperature, py2i(i)))
                i += 1
            Loop Until i = pxi.Count

            If px1l1.Count > 0 Then
                i = 0
                Do
                    vx1l1.Add(px1l1(i))
                    vx1l2.Add(px1l2(i))
                    vy3.Add(cv.ConverterDoSI(su.spmp_temperature, py3(i)))
                    i += 1
                Loop Until i = px1l1.Count

            End If

            With Me.Grid1.Columns
                .Clear()
                .Add("c1", "x (" & c(0) & ")")
                .Add("c2", "Tbub (" & su.spmp_temperature & ")")
                .Add("c3", "x (" & c(0) & ")")
                .Add("c4", "Tdew (" & su.spmp_temperature & ")")
            End With
            For Each co As DataGridViewColumn In Me.Grid1.Columns
                co.SortMode = DataGridViewColumnSortMode.NotSortable
                co.DefaultCellStyle.Alignment = DataGridViewContentAlignment.MiddleRight
                co.HeaderCell.Style.Alignment = DataGridViewContentAlignment.MiddleCenter
            Next
            Dim j, k As Integer
            Dim data(11, Math.Max(vx1.Count - 1, vx2.Count - 1)) As String
            j = 0
            For Each d As Double In vx1
                data(0, j) = vx1(j)
                data(1, j) = vy1(j)
                j = j + 1
            Next
            j = 0
            For Each d As Double In vx2
                data(2, j) = vx2(j)
                data(3, j) = vy2(j)
                j = j + 1
            Next
            With Me.Grid1.Rows
                .Clear()
                k = 0
                Do
                    .Add()
                    j = 0
                    Do
                        If Double.TryParse(data(j, k), New Double) Then
                            .Item(k).Cells(j).Value = Format(CDbl(data(j, k)), nf)
                        Else
                            .Item(k).Cells(j).Value = data(j, k)
                        End If
                        j = j + 1
                    Loop Until j = 4
                    k = k + 1
                Loop Until k = Math.Max(vx1.Count, vx2.Count)
            End With

            With Me.GraphControl.GraphPane
                .Title.Text = c(0) & " / " & c(1) & vbCrLf & "P = " & cv.ConverterDoSI(su.spmp_pressure, P) & " " & su.spmp_pressure
                .CurveList.Clear()
                With .AddCurve(DWSIM.App.GetLocalString("PontosdeBolha"), vx1.ToArray(GetType(Double)), vy1.ToArray(GetType(Double)), Color.DeepSkyBlue, ZedGraph.SymbolType.Circle)
                    .Color = Color.SteelBlue
                    .Line.IsSmooth = True
                    .Symbol.Fill.Type = ZedGraph.FillType.Solid
                End With
                With .AddCurve(DWSIM.App.GetLocalString("PontosdeOrvalho"), vx2.ToArray(GetType(Double)), vy2.ToArray(GetType(Double)), Color.SlateBlue, ZedGraph.SymbolType.Circle)
                    .Color = Color.YellowGreen
                    .Line.IsSmooth = True
                    .Symbol.Fill.Type = ZedGraph.FillType.Solid
                End With
                With .AddCurve(DWSIM.App.GetLocalString("Ideal"), vxi.ToArray(GetType(Double)), vy1i.ToArray(GetType(Double)), Color.DeepSkyBlue, ZedGraph.SymbolType.Circle)
                    .Color = Color.SteelBlue
                    .Line.IsSmooth = True
                    .Line.Style = Drawing2D.DashStyle.Dash
                    .Symbol.IsVisible = False
                End With
                With .AddCurve(DWSIM.App.GetLocalString("Ideal"), vxi.ToArray(GetType(Double)), vy2i.ToArray(GetType(Double)), Color.SlateBlue, ZedGraph.SymbolType.Circle)
                    .Color = Color.YellowGreen
                    .Line.IsSmooth = True
                    .Line.Style = Drawing2D.DashStyle.Dash
                    .Symbol.IsVisible = False
                End With
                If vx1l1.Count > 0 Then
                    With .AddCurve(DWSIM.App.GetLocalString("LLE LP1"), vx1l1.ToArray(GetType(Double)), vy3.ToArray(GetType(Double)), Color.Red, ZedGraph.SymbolType.Diamond)
                        .Symbol.Fill.Type = ZedGraph.FillType.Solid
                        .Line.IsVisible = False
                    End With
                    With .AddCurve(DWSIM.App.GetLocalString("LLE LP2"), vx1l2.ToArray(GetType(Double)), vy3.ToArray(GetType(Double)), Color.Red, ZedGraph.SymbolType.Diamond)
                        .Symbol.Fill.Type = ZedGraph.FillType.Solid
                        .Line.IsVisible = False
                    End With
                End If
                .XAxis.Title.Text = cbXAxisBasis.SelectedItem.ToString & " / " & c(0)
                .YAxis.Title.Text = "T / " & su.spmp_temperature
                Me.GraphControl.IsAutoScrollRange = True
                Select Case cbXAxisBasis.SelectedIndex
                    Case 0, 1
                        Me.GraphControl.GraphPane.XAxis.Scale.Max = 1
                    Case 2, 3
                        Me.GraphControl.GraphPane.XAxis.Scale.Max = 100
                End Select
                .AxisChange(Me.CreateGraphics)
                Me.GraphControl.Invalidate()
            End With

        ElseIf Me.RadioButton2.Checked Then

            Dim px, py1, py2, pxi, py1i, py2i, px1l1, px1l2, py3 As New ArrayList
            px = r(0)
            py1 = r(1)
            py2 = r(2)
            pxi = r2(0)
            py1i = r2(1)
            py2i = r2(2)
            px1l1 = r(3)
            px1l2 = r(4)
            py3 = r(5)

            Dim vx1, vx2, vy1, vy2, vxi, vy1i, vy2i, vx1l1, vx1l2, vy3 As New ArrayList

            i = 0
            Do
                If py1(i) <> 0.0# Then
                    vx1.Add(px(i))
                    vy1.Add(cv.ConverterDoSI(su.spmp_pressure, py1(i)))
                End If
                If py2(i) <> 0.0# Then
                    vx2.Add(px(i))
                    vy2.Add(cv.ConverterDoSI(su.spmp_pressure, py2(i)))
                End If
                i += 1
            Loop Until i = px.Count

            i = 0
            Do
                vxi.Add(pxi(i))
                vy1i.Add(cv.ConverterDoSI(su.spmp_pressure, py1i(i)))
                vy2i.Add(cv.ConverterDoSI(su.spmp_pressure, py2i(i)))
                i += 1
            Loop Until i = pxi.Count

            If px1l1.Count > 0 Then
                i = 0
                Do
                    vx1l1.Add(px1l1(i))
                    vx1l2.Add(px1l2(i))
                    vy3.Add(cv.ConverterDoSI(su.spmp_pressure, py3(i)))
                    i += 1
                Loop Until i = px1l1.Count
            End If

            With Me.Grid1.Columns
                .Clear()
                .Add("c1", "x (" & c(0) & ")")
                .Add("c2", "Pbub (" & su.spmp_pressure & ")")
                .Add("c3", "x (" & c(0) & ")")
                .Add("c4", "Pdew (" & su.spmp_pressure & ")")
            End With
            For Each co As DataGridViewColumn In Me.Grid1.Columns
                co.SortMode = DataGridViewColumnSortMode.NotSortable
                co.DefaultCellStyle.Alignment = DataGridViewContentAlignment.MiddleRight
                co.HeaderCell.Style.Alignment = DataGridViewContentAlignment.MiddleCenter
            Next
            Dim j, k As Integer
            Dim data(11, Math.Max(vx1.Count - 1, vx2.Count - 1)) As String
            j = 0
            For Each d As Double In vx1
                data(0, j) = vx1(j)
                data(1, j) = vy1(j)
                j = j + 1
            Next
            j = 0
            For Each d As Double In vx2
                data(2, j) = vx2(j)
                data(3, j) = vy2(j)
                j = j + 1
            Next
            With Me.Grid1.Rows
                .Clear()
                k = 0
                Do
                    .Add()
                    j = 0
                    Do
                        If Double.TryParse(data(j, k), New Double) Then
                            .Item(k).Cells(j).Value = Format(CDbl(data(j, k)), nf)
                        Else
                            .Item(k).Cells(j).Value = data(j, k)
                        End If
                        j = j + 1
                    Loop Until j = 4
                    k = k + 1
                Loop Until k = Math.Max(vx1.Count, vx2.Count)
            End With

            With Me.GraphControl.GraphPane
                .Title.Text = c(0) & " / " & c(1) & vbCrLf & "T = " & cv.ConverterDoSI(su.spmp_temperature, T) & " " & su.spmp_temperature
                .CurveList.Clear()
                With .AddCurve(DWSIM.App.GetLocalString("PontosdeBolha"), vx1.ToArray(GetType(Double)), vy1.ToArray(GetType(Double)), Color.DeepSkyBlue, ZedGraph.SymbolType.Circle)
                    .Color = Color.SteelBlue
                    .Line.IsSmooth = True
                    .Symbol.Fill.Type = ZedGraph.FillType.Solid
                End With
                With .AddCurve(DWSIM.App.GetLocalString("PontosdeOrvalho"), vx2.ToArray(GetType(Double)), vy2.ToArray(GetType(Double)), Color.SlateBlue, ZedGraph.SymbolType.Circle)
                    .Color = Color.YellowGreen
                    .Line.IsSmooth = True
                    .Symbol.Fill.Type = ZedGraph.FillType.Solid
                End With
                With .AddCurve(DWSIM.App.GetLocalString("Ideal"), vxi.ToArray(GetType(Double)), vy1i.ToArray(GetType(Double)), Color.DeepSkyBlue, ZedGraph.SymbolType.None)
                    .Color = Color.SteelBlue
                    .Line.IsSmooth = True
                    .Line.Style = Drawing2D.DashStyle.Dash
                    .Symbol.IsVisible = False
                End With
                With .AddCurve(DWSIM.App.GetLocalString("Ideal"), vxi.ToArray(GetType(Double)), vy2i.ToArray(GetType(Double)), Color.SlateBlue, ZedGraph.SymbolType.None)
                    .Color = Color.YellowGreen
                    .Line.IsSmooth = True
                    .Line.Style = Drawing2D.DashStyle.Dash
                    .Symbol.IsVisible = False
                End With
                If vx1l1.Count > 0 Then
                    With .AddCurve(DWSIM.App.GetLocalString("LLE LP1"), vx1l1.ToArray(GetType(Double)), vy3.ToArray(GetType(Double)), Color.Red, ZedGraph.SymbolType.Diamond)
                        .Line.IsVisible = False
                        .Symbol.Fill.Type = ZedGraph.FillType.Solid
                    End With
                    With .AddCurve(DWSIM.App.GetLocalString("LLE LP2"), vx1l2.ToArray(GetType(Double)), vy3.ToArray(GetType(Double)), Color.Red, ZedGraph.SymbolType.Diamond)
                        .Line.IsVisible = False
                        .Symbol.Fill.Type = ZedGraph.FillType.Solid
                    End With
                End If
                .XAxis.Title.Text = cbXAxisBasis.SelectedItem.ToString & " / " & c(0)
                .YAxis.Title.Text = "P / " & su.spmp_pressure
                Me.GraphControl.IsAutoScrollRange = True
                Select Case cbXAxisBasis.SelectedIndex
                    Case 0, 1
                        Me.GraphControl.GraphPane.XAxis.Scale.Max = 1
                    Case 2, 3
                        Me.GraphControl.GraphPane.XAxis.Scale.Max = 100
                End Select
                .AxisChange(Me.CreateGraphics)
                Me.GraphControl.Invalidate()
            End With

        ElseIf Me.RadioButton3.Checked Then

            Dim px, py, pxi, pyi As New ArrayList
            px = r(0)
            py = r(1)
            pxi = r2(0)
            pyi = r2(1)

            Dim vx, vy, vxi, vyi As New ArrayList

            i = 0
            Do
                vx.Add(px(i))
                vy.Add(py(i))
                i += 1
            Loop Until i = px.Count

            i = 0
            Do
                vxi.Add(pxi(i))
                vyi.Add(pyi(i))
                i += 1
            Loop Until i = pxi.Count

            With Me.Grid1.Columns
                .Clear()
                .Add("c1", "x (" & c(0) & ")")
                .Add("c2", "y (" & c(0) & ")")
            End With
            For Each co As DataGridViewColumn In Me.Grid1.Columns
                co.SortMode = DataGridViewColumnSortMode.NotSortable
                co.DefaultCellStyle.Alignment = DataGridViewContentAlignment.MiddleRight
                co.HeaderCell.Style.Alignment = DataGridViewContentAlignment.MiddleCenter
            Next
            Dim j, k As Integer
            Dim data(11, vx.Count - 1) As String
            j = 0
            For Each d As Double In px
                data(0, j) = vx(j)
                data(1, j) = vy(j)
                j = j + 1
            Next
            With Me.Grid1.Rows
                .Clear()
                k = 0
                Do
                    .Add()
                    j = 0
                    Do
                        If Double.TryParse(data(j, k), New Double) Then
                            .Item(k).Cells(j).Value = Format(CDbl(data(j, k)), nf)
                        Else
                            .Item(k).Cells(j).Value = data(j, k)
                        End If
                        j = j + 1
                    Loop Until j = 2
                    k = k + 1
                Loop Until k = vx.Count
            End With

            With Me.GraphControl.GraphPane
                .Title.Text = c(0) & " / " & c(1) & vbCrLf & "P = " & cv.ConverterDoSI(su.spmp_pressure, P) & " " & su.spmp_pressure
                .CurveList.Clear()
                With .AddCurve("", vx.ToArray(GetType(Double)), vy.ToArray(GetType(Double)), Color.SlateBlue, ZedGraph.SymbolType.Circle)
                    .Color = Color.SteelBlue
                    .Line.IsSmooth = True
                    .Symbol.Fill.Type = ZedGraph.FillType.Solid
                End With
                With .AddCurve("", vxi.ToArray(GetType(Double)), vyi.ToArray(GetType(Double)), Color.SlateBlue, ZedGraph.SymbolType.Circle)
                    .Color = Color.SteelBlue
                    .Line.IsSmooth = True
                    .Line.Style = Drawing2D.DashStyle.Dash
                    .Symbol.IsVisible = False
                End With
                .XAxis.Title.Text = cbXAxisBasis.SelectedItem.ToString & " / " & c(0)
                .YAxis.Title.Text = DWSIM.App.GetLocalString("FraoMolary") & c(0)
                Me.GraphControl.IsAutoScrollRange = True
                Select Case cbXAxisBasis.SelectedIndex
                    Case 0, 1
                        Me.GraphControl.GraphPane.XAxis.Scale.Max = 1
                    Case 2, 3
                        Me.GraphControl.GraphPane.XAxis.Scale.Max = 100
                End Select
                .AxisChange(Me.CreateGraphics)
                Me.GraphControl.Invalidate()
            End With

        ElseIf Me.RadioButton4.Checked Then

            Dim px, py, pxi, pyi As New ArrayList
            px = r(0)
            py = r(1)
            pxi = r2(0)
            pyi = r2(1)

            Dim vx, vy, vxi, vyi As New ArrayList

            i = 0
            Do
                vx.Add(px(i))
                vy.Add(py(i))
                i += 1
            Loop Until i = px.Count

            i = 0
            Do
                vxi.Add(pxi(i))
                vyi.Add(pyi(i))
                i += 1
            Loop Until i = pxi.Count

            With Me.Grid1.Columns
                .Clear()
                .Add("c1", "x (" & c(0) & ")")
                .Add("c2", "y (" & c(0) & ")")
            End With
            For Each co As DataGridViewColumn In Me.Grid1.Columns
                co.SortMode = DataGridViewColumnSortMode.NotSortable
                co.DefaultCellStyle.Alignment = DataGridViewContentAlignment.MiddleRight
                co.HeaderCell.Style.Alignment = DataGridViewContentAlignment.MiddleCenter
            Next
            Dim j, k As Integer
            Dim data(11, vx.Count - 1) As String
            j = 0
            For Each d As Double In px
                data(0, j) = vx(j)
                data(1, j) = vy(j)
                j = j + 1
            Next
            With Me.Grid1.Rows
                .Clear()
                k = 0
                Do
                    .Add()
                    j = 0
                    Do
                        If Double.TryParse(data(j, k), New Double) Then
                            .Item(k).Cells(j).Value = Format(CDbl(data(j, k)), nf)
                        Else
                            .Item(k).Cells(j).Value = data(j, k)
                        End If
                        j = j + 1
                    Loop Until j = 2
                    k = k + 1
                Loop Until k = vx.Count
            End With

            With Me.GraphControl.GraphPane
                .Title.Text = c(0) & " / " & c(1) & vbCrLf & "T = " & cv.ConverterDoSI(su.spmp_temperature, T) & " " & su.spmp_temperature
                .CurveList.Clear()
                With .AddCurve("", vx.ToArray(GetType(Double)), vy.ToArray(GetType(Double)), Color.SlateBlue, ZedGraph.SymbolType.Circle)
                    .Color = Color.SteelBlue
                    .Line.IsSmooth = True
                    .Symbol.Fill.Type = ZedGraph.FillType.Solid
                End With
                With .AddCurve("", vxi.ToArray(GetType(Double)), vyi.ToArray(GetType(Double)), Color.SlateBlue, ZedGraph.SymbolType.Circle)
                    .Color = Color.SteelBlue
                    .Line.IsSmooth = True
                    .Line.Style = Drawing2D.DashStyle.Dash
                    .Symbol.IsVisible = False
                End With
                .XAxis.Title.Text = cbXAxisBasis.SelectedItem.ToString & " / " & c(0)
                .YAxis.Title.Text = DWSIM.App.GetLocalString("FraoMolary") & c(0)
                Me.GraphControl.IsAutoScrollRange = True
                Select Case cbXAxisBasis.SelectedIndex
                    Case 0, 1
                        Me.GraphControl.GraphPane.XAxis.Scale.Max = 1
                    Case 2, 3
                        Me.GraphControl.GraphPane.XAxis.Scale.Max = 100
                End Select
                .AxisChange(Me.CreateGraphics)
                Me.GraphControl.Invalidate()
            End With

        Else

            Dim px, py As ArrayList
            px = r(0)
            py = r(1)

            Dim vx, vy As New ArrayList

            i = 0
            Do
                vx.Add(px(i))
                vy.Add(py(i))
                i += 1
            Loop Until i = px.Count

            With Me.Grid1.Columns
                .Clear()
                .Add("c1", "x (" & c(0) & ")")
                .Add("c2", DWSIM.App.GetLocalString("DeltaGRT"))
            End With
            For Each co As DataGridViewColumn In Me.Grid1.Columns
                co.SortMode = DataGridViewColumnSortMode.NotSortable
                co.DefaultCellStyle.Alignment = DataGridViewContentAlignment.MiddleRight
                co.HeaderCell.Style.Alignment = DataGridViewContentAlignment.MiddleCenter
            Next
            Dim j, k As Integer
            Dim data(11, vx.Count - 1) As String
            j = 0
            For Each d As Double In px
                data(0, j) = vx(j)
                data(1, j) = vy(j)
                j = j + 1
            Next
            With Me.Grid1.Rows
                .Clear()
                k = 0
                Do
                    .Add()
                    j = 0
                    Do
                        If Double.TryParse(data(j, k), New Double) Then
                            .Item(k).Cells(j).Value = Format(CDbl(data(j, k)), nf)
                        Else
                            .Item(k).Cells(j).Value = data(j, k)
                        End If
                        j = j + 1
                    Loop Until j = 2
                    k = k + 1
                Loop Until k = vx.Count
            End With

            With Me.GraphControl.GraphPane
                .Title.Text = c(0) & " / " & c(1) & vbCrLf & "T = " & cv.ConverterDoSI(su.spmp_temperature, T) & " " & su.spmp_temperature & ", P = " & cv.ConverterDoSI(su.spmp_pressure, P) & " " & su.spmp_pressure
                .CurveList.Clear()
                With .AddCurve("", vx.ToArray(GetType(Double)), vy.ToArray(GetType(Double)), Color.SlateBlue, ZedGraph.SymbolType.Circle)
                    .Color = Color.SteelBlue
                    .Line.IsSmooth = True
                    .Symbol.Fill.Type = ZedGraph.FillType.Solid
                End With
                .XAxis.Title.Text = DWSIM.App.GetLocalString("FraoMolarx") & c(0)
                .YAxis.Title.Text = DWSIM.App.GetLocalString("DeltaGRT")
                .AxisChange(Me.CreateGraphics)
                Me.GraphControl.IsAutoScrollRange = True
                Select Case cbXAxisBasis.SelectedIndex
                    Case 0, 1
                        Me.GraphControl.GraphPane.XAxis.Scale.Max = 1
                    Case 2, 3
                        Me.GraphControl.GraphPane.XAxis.Scale.Max = 100
                End Select
                Me.GraphControl.Invalidate()
            End With

        End If

    End Sub

End Class