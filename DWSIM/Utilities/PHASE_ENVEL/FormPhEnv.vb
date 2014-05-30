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

Public Class FormPhEnv

    Inherits System.Windows.Forms.Form

    Dim mat As DWSIM.SimulationObjects.Streams.MaterialStream
    Dim Frm As FormFlowsheet

    Dim cp As DWSIM.Utilities.TCP.Methods

    Public su As New DWSIM.SistemasDeUnidades.Unidades
    Public cv As New DWSIM.SistemasDeUnidades.Conversor
    Public nf As String

    Private loaded As Boolean = False
    Private calculated As Boolean = False
    Private qualitycalc As Boolean = False
    Private showoppoint As Boolean = True

    'desmembrar vetores
    Dim PB, PO, TVB, TVD, HB, HO, SB, SO, VB, VO, TE, PE, PHsI, PHsII, THsI, THsII, TQ, PQ As New ArrayList
    Dim UT, UP, UH, US, UV As New ArrayList
    Dim PC As ArrayList
    Dim ot, op, ov, oh, os As Double
    Dim strname As String = ""

    Dim fpec As FormPEC

    Private Sub FormPhEnv_Load(ByVal sender As System.Object, ByVal e As System.EventArgs) Handles MyBase.Load

        Me.ComboBox1.SelectedIndex = 0

        Me.Frm = My.Application.ActiveSimulation

        Me.cp = New DWSIM.Utilities.TCP.Methods

        Me.su = Frm.Options.SelectedUnitSystem
        Me.nf = Frm.Options.NumberFormat

        Me.ComboBox3.Items.Clear()
        For Each mat In Me.Frm.Collections.CLCS_MaterialStreamCollection.Values
            If mat.GraphicObject.Calculated Then Me.ComboBox3.Items.Add(mat.GraphicObject.Tag.ToString)
        Next

        If Me.ComboBox3.Items.Count > 0 Then Me.ComboBox3.SelectedIndex = 0

        Me.Text = DWSIM.App.GetLocalString("DWSIMUtilitriosDiagr1")

        If Frm.Options.SelectedPropertyPackage.ComponentName.Contains("(PR)") Or _
           Frm.Options.SelectedPropertyPackage.ComponentName.Contains("(SRK)") Then
            Me.CheckBox1.Enabled = True
            Me.TextBox1.Enabled = True
            Me.CheckBox3.Enabled = True
        Else
            Me.CheckBox1.Enabled = False
            Me.TextBox1.Enabled = False
            Me.CheckBox3.Enabled = False
        End If

    End Sub

    Private Sub Button1_Click(ByVal sender As System.Object, ByVal e As System.EventArgs) Handles Button1.Click

        If Not Me.ComboBox3.SelectedItem Is Nothing Then

            Dim x As Double

            If Me.CheckBox1.Checked Then
                If Double.TryParse(TextBox1.Text, x) Then
                    If x >= 0 And x <= 1 Then
                        GoTo exec
                    Else
                        MessageBox.Show(DWSIM.App.GetLocalString("Ovalorinformadoparaa"), DWSIM.App.GetLocalString("Parmetroinvlido"), MessageBoxButtons.OK, MessageBoxIcon.Error)
                        Exit Sub
                    End If
                Else
                End If
            End If

exec:       With Me.GraphControl.GraphPane.Legend
                .Position = ZedGraph.LegendPos.TopCenter
                .Border.IsVisible = False
                .FontSpec.Size = 10
                .FontSpec.IsDropShadow = False
            End With
            If Me.CheckBox1.Enabled Then Me.qualitycalc = Me.CheckBox1.Checked Else Me.qualitycalc = False
            If Me.CheckBox2.Checked Then Me.showoppoint = True Else Me.showoppoint = False
            Me.Enabled = False
            Me.BackgroundWorker1.RunWorkerAsync(New Object() {0, Me.TextBox1.Text, Me.CheckBox1.Checked, Me.CheckBox3.Checked})
            fpec = New FormPEC
            fpec.bw = Me.BackgroundWorker1
            fpec.Label2.Tag = fpec.Label2.Text
            Try
                fpec.ShowDialog(Me)
            Catch ex As Exception
                fpec.Close()
                Me.Frm.WriteToLog(ex.ToString, Color.Red, DWSIM.FormClasses.TipoAviso.Erro)
            End Try
        End If

    End Sub

    Private Sub ComboBox1_SelectedIndexChanged(ByVal sender As System.Object, ByVal e As System.EventArgs) Handles ComboBox1.SelectedIndexChanged

        If Me.loaded And Me.calculated Then

            Select Case ComboBox1.SelectedIndex

                Case 0

                    Dim px1, py1, px2, py2, px3, py3, px4, py4, ph1, ph2, th1, th2 As New ArrayList

                    Dim i As Integer

                    For i = 0 To TVB.Count - 1
                        py1.Add(cv.ConverterDoSI(su.spmp_temperature, TVB(i)))
                        px1.Add(cv.ConverterDoSI(su.spmp_pressure, PB(i)))
                    Next
                    For i = 0 To TVD.Count - 1
                        py2.Add(cv.ConverterDoSI(su.spmp_temperature, TVD(i)))
                        px2.Add(cv.ConverterDoSI(su.spmp_pressure, PO(i)))
                    Next
                    For i = 0 To TE.Count - 1
                        py3.Add(cv.ConverterDoSI(su.spmp_temperature, TE(i)))
                        px3.Add(cv.ConverterDoSI(su.spmp_pressure, PE(i)))
                    Next
                    For i = 0 To TQ.Count - 1
                        py4.Add(cv.ConverterDoSI(su.spmp_temperature, TQ(i)))
                        px4.Add(cv.ConverterDoSI(su.spmp_pressure, PQ(i)))
                    Next
                    
                    With Me.GraphControl.GraphPane
                        .CurveList.Clear()
                        Dim tmp As Object
                        For Each tmp In PC
                            .AddCurve(DWSIM.App.GetLocalString("PontoCrtico"), New Double() {cv.ConverterDoSI(su.spmp_pressure, tmp(1))}, New Double() {cv.ConverterDoSI(su.spmp_temperature, tmp(0))}, Color.Red, ZedGraph.SymbolType.Circle).Symbol.Fill.Type = ZedGraph.FillType.Solid
                        Next
                        With .AddCurve(DWSIM.App.GetLocalString("PontosdeBolha"), px1.ToArray(GetType(Double)), py1.ToArray(GetType(Double)), Color.SlateBlue, ZedGraph.SymbolType.Circle)
                            .Color = Color.SteelBlue
                            .Line.IsSmooth = False
                            .Symbol.Fill.Type = ZedGraph.FillType.Solid
                        End With
                        With .AddCurve(DWSIM.App.GetLocalString("PontosdeOrvalho"), px2.ToArray(GetType(Double)), py2.ToArray(GetType(Double)), Color.DeepSkyBlue, ZedGraph.SymbolType.Circle)
                            .Color = Color.YellowGreen
                            .Line.IsSmooth = False
                            .Symbol.Fill.Type = ZedGraph.FillType.Solid
                        End With
                        If CheckBox3.Checked Then
                            With .AddCurve(DWSIM.App.GetLocalString("LimitedeEstabilidade"), px3.ToArray(GetType(Double)), py3.ToArray(GetType(Double)), Color.Red, ZedGraph.SymbolType.Circle)
                                .Color = Color.DarkOrange
                                .Line.IsSmooth = False
                                .Symbol.Fill.Type = ZedGraph.FillType.Solid
                            End With
                        End If
                        If qualitycalc Then
                            With .AddCurve("V = " & Me.TextBox1.Text, px4.ToArray(GetType(Double)), py4.ToArray(GetType(Double)), Color.Red, ZedGraph.SymbolType.Circle)
                                .Color = Color.DarkGreen
                                .Line.IsSmooth = False
                                .Symbol.Fill.Type = ZedGraph.FillType.Solid
                            End With
                        End If
                        If Me.showoppoint Then
                            With .AddCurve(DWSIM.App.GetLocalString("PontodeOperao"), New Double() {op}, New Double() {ot}, Color.Red, ZedGraph.SymbolType.Circle)
                                .Color = Color.Black
                                .Line.IsSmooth = False
                                .Symbol.Fill.Type = ZedGraph.FillType.Solid
                            End With
                        End If
                        .Title.Text = strname
                        .XAxis.Title.Text = "P / " & su.spmp_pressure
                        .YAxis.Title.Text = "T / " & su.spmp_temperature
                        .AxisChange(Me.CreateGraphics)
                        Me.GraphControl.IsAutoScrollRange = True
                        Me.GraphControl.Invalidate()
                    End With

                Case 1

                    Dim px1, py1, px2, py2 As New ArrayList
                    Dim i As Integer
                    For i = 0 To PB.Count - 1
                        px1.Add(cv.ConverterDoSI(su.spmp_pressure, PB(i)))
                        py1.Add(cv.ConverterDoSI(su.spmp_enthalpy, HB(i)))
                    Next
                    For i = 0 To PO.Count - 1
                        px2.Add(cv.ConverterDoSI(su.spmp_pressure, PO(i)))
                        py2.Add(cv.ConverterDoSI(su.spmp_enthalpy, HO(i)))
                    Next
                    With Me.GraphControl.GraphPane
                        .CurveList.Clear()
                        '.AddCurve(DWSIM.App.GetLocalString("PontoCrtico"), New Double() {cv.ConverterDoSI(su.spmp_temperature, TC)}, New Double() {cv.ConverterDoSI(su.spmp_pressure, PC)}, Color.Black, ZedGraph.SymbolType.Circle)
                        With .AddCurve(DWSIM.App.GetLocalString("PontosdeBolha"), px1.ToArray(GetType(Double)), py1.ToArray(GetType(Double)), Color.SlateBlue, ZedGraph.SymbolType.Circle)
                            .Color = Color.SteelBlue
                            .Line.IsSmooth = False
                            .Symbol.Fill.Type = ZedGraph.FillType.Solid
                        End With
                        With .AddCurve(DWSIM.App.GetLocalString("PontosdeOrvalho"), px2.ToArray(GetType(Double)), py2.ToArray(GetType(Double)), Color.DeepSkyBlue, ZedGraph.SymbolType.Circle)
                            .Color = Color.YellowGreen
                            .Line.IsSmooth = False
                            .Symbol.Fill.Type = ZedGraph.FillType.Solid
                        End With
                        .Title.Text = strname
                        .XAxis.Title.Text = "P / " & su.spmp_pressure
                        .YAxis.Title.Text = "H / " & su.spmp_enthalpy
                        .AxisChange(Me.CreateGraphics)
                        Me.GraphControl.Invalidate()
                        If Me.showoppoint Then
                            With .AddCurve(DWSIM.App.GetLocalString("PontodeOperao"), New Double() {op}, New Double() {oh}, Color.Red, ZedGraph.SymbolType.Circle)
                                .Color = Color.Black
                                .Line.IsSmooth = False
                                .Symbol.Fill.Type = ZedGraph.FillType.Solid
                            End With
                        End If
                    End With

                Case 2

                    Dim px1, py1, px2, py2 As New ArrayList
                    Dim i As Integer
                    For i = 0 To PB.Count - 1
                        px1.Add(cv.ConverterDoSI(su.spmp_pressure, PB(i)))
                        py1.Add(cv.ConverterDoSI(su.spmp_entropy, SB(i)))
                    Next
                    For i = 0 To PO.Count - 1
                        px2.Add(cv.ConverterDoSI(su.spmp_pressure, PO(i)))
                        py2.Add(cv.ConverterDoSI(su.spmp_entropy, SO(i)))
                    Next
                    With Me.GraphControl.GraphPane
                        .CurveList.Clear()
                        With .AddCurve(DWSIM.App.GetLocalString("PontosdeBolha"), px1.ToArray(GetType(Double)), py1.ToArray(GetType(Double)), Color.SlateBlue, ZedGraph.SymbolType.Circle)
                            .Color = Color.SteelBlue
                            .Line.IsSmooth = False
                            .Symbol.Fill.Type = ZedGraph.FillType.Solid
                        End With
                        With .AddCurve(DWSIM.App.GetLocalString("PontosdeOrvalho"), px2.ToArray(GetType(Double)), py2.ToArray(GetType(Double)), Color.DeepSkyBlue, ZedGraph.SymbolType.Circle)
                            .Color = Color.YellowGreen
                            .Line.IsSmooth = False
                            .Symbol.Fill.Type = ZedGraph.FillType.Solid
                        End With
                        .Title.Text = strname
                        .XAxis.Title.Text = "P / " & su.spmp_pressure
                        .YAxis.Title.Text = "S / " & su.spmp_entropy
                        .AxisChange(Me.CreateGraphics)
                        Me.GraphControl.Invalidate()
                        If Me.showoppoint Then
                            With .AddCurve(DWSIM.App.GetLocalString("PontodeOperao"), New Double() {op}, New Double() {os}, Color.Red, ZedGraph.SymbolType.Circle)
                                .Color = Color.Black
                                .Line.IsSmooth = False
                                .Symbol.Fill.Type = ZedGraph.FillType.Solid
                            End With
                        End If
                    End With

                Case 3

                    Dim px1, py1, px2, py2 As New ArrayList
                    Dim i As Integer
                    For i = 0 To PB.Count - 1
                        px1.Add(cv.ConverterDoSI(su.spmp_pressure, PB(i)))
                        py1.Add(cv.ConverterDoSI(su.molar_volume, VB(i)))
                    Next
                    For i = 0 To PO.Count - 1
                        px2.Add(cv.ConverterDoSI(su.spmp_pressure, PO(i)))
                        py2.Add(cv.ConverterDoSI(su.molar_volume, VO(i)))
                    Next
                    With Me.GraphControl.GraphPane
                        .CurveList.Clear()
                        Dim tmp As Object
                        For Each tmp In PC
                            .AddCurve(DWSIM.App.GetLocalString("PontoCrtico"), New Double() {cv.ConverterDoSI(su.spmp_pressure, tmp(1))}, New Double() {cv.ConverterDoSI(su.molar_volume, tmp(2))}, Color.Red, ZedGraph.SymbolType.Circle).Symbol.Fill.Type = ZedGraph.FillType.Solid
                        Next
                        With .AddCurve(DWSIM.App.GetLocalString("PontosdeBolha"), px1.ToArray(GetType(Double)), py1.ToArray(GetType(Double)), Color.SlateBlue, ZedGraph.SymbolType.Circle)
                            .Color = Color.SteelBlue
                            .Line.IsSmooth = False
                            .Symbol.Fill.Type = ZedGraph.FillType.Solid
                        End With
                        With .AddCurve(DWSIM.App.GetLocalString("PontosdeOrvalho"), px2.ToArray(GetType(Double)), py2.ToArray(GetType(Double)), Color.DeepSkyBlue, ZedGraph.SymbolType.Circle)
                            .Color = Color.YellowGreen
                            .Line.IsSmooth = False
                            .Symbol.Fill.Type = ZedGraph.FillType.Solid
                        End With
                        .Title.Text = strname
                        .XAxis.Title.Text = "P / " & su.spmp_pressure
                        .YAxis.Title.Text = "V / " & su.molar_volume
                        .AxisChange(Me.CreateGraphics)
                        Me.GraphControl.Invalidate()
                        If Me.showoppoint Then
                            With .AddCurve(DWSIM.App.GetLocalString("PontodeOperao"), New Double() {op}, New Double() {ov}, Color.Red, ZedGraph.SymbolType.Circle)
                                .Color = Color.Black
                                .Line.IsSmooth = False
                                .Symbol.Fill.Type = ZedGraph.FillType.Solid
                            End With
                        End If
                    End With

                Case 4

                    Dim px1, py1, px2, py2, px3, py3, px4, py4, ph1, ph2, th1, th2 As New ArrayList

                    Dim i As Integer

                    For i = 0 To TVB.Count - 1
                        px1.Add(cv.ConverterDoSI(su.spmp_temperature, TVB(i)))
                        py1.Add(cv.ConverterDoSI(su.spmp_pressure, PB(i)))
                    Next
                    For i = 0 To TVD.Count - 1
                        px2.Add(cv.ConverterDoSI(su.spmp_temperature, TVD(i)))
                        py2.Add(cv.ConverterDoSI(su.spmp_pressure, PO(i)))
                    Next
                    For i = 0 To TE.Count - 1
                        px3.Add(cv.ConverterDoSI(su.spmp_temperature, TE(i)))
                        py3.Add(cv.ConverterDoSI(su.spmp_pressure, PE(i)))
                    Next
                    For i = 0 To TQ.Count - 1
                        px4.Add(cv.ConverterDoSI(su.spmp_temperature, TQ(i)))
                        py4.Add(cv.ConverterDoSI(su.spmp_pressure, PQ(i)))
                    Next

                    With Me.GraphControl.GraphPane
                        .CurveList.Clear()
                        Dim tmp As Object
                        For Each tmp In PC
                            .AddCurve(DWSIM.App.GetLocalString("PontoCrtico"), New Double() {cv.ConverterDoSI(su.spmp_temperature, tmp(0))}, New Double() {cv.ConverterDoSI(su.spmp_pressure, tmp(1))}, Color.Red, ZedGraph.SymbolType.Circle).Symbol.Fill.Type = ZedGraph.FillType.Solid
                        Next
                        With .AddCurve(DWSIM.App.GetLocalString("PontosdeBolha"), px1.ToArray(GetType(Double)), py1.ToArray(GetType(Double)), Color.SlateBlue, ZedGraph.SymbolType.Circle)
                            .Color = Color.SteelBlue
                            .Line.IsSmooth = False
                            .Symbol.Fill.Type = ZedGraph.FillType.Solid
                        End With
                        With .AddCurve(DWSIM.App.GetLocalString("PontosdeOrvalho"), px2.ToArray(GetType(Double)), py2.ToArray(GetType(Double)), Color.DeepSkyBlue, ZedGraph.SymbolType.Circle)
                            .Color = Color.YellowGreen
                            .Line.IsSmooth = False
                            .Symbol.Fill.Type = ZedGraph.FillType.Solid
                        End With
                        If CheckBox3.Checked Then
                            With .AddCurve(DWSIM.App.GetLocalString("LimitedeEstabilidade"), px3.ToArray(GetType(Double)), py3.ToArray(GetType(Double)), Color.Red, ZedGraph.SymbolType.Circle)
                                .Color = Color.DarkOrange
                                .Line.IsSmooth = False
                                .Symbol.Fill.Type = ZedGraph.FillType.Solid
                            End With
                        End If
                        If qualitycalc Then
                            With .AddCurve("V = " & Me.TextBox1.Text, px4.ToArray(GetType(Double)), py4.ToArray(GetType(Double)), Color.Red, ZedGraph.SymbolType.Circle)
                                .Color = Color.DarkGreen
                                .Line.IsSmooth = False
                                .Symbol.Fill.Type = ZedGraph.FillType.Solid
                            End With
                        End If
                        .Title.Text = strname
                        .XAxis.Title.Text = "T / " & su.spmp_temperature
                        .YAxis.Title.Text = "P / " & su.spmp_pressure
                        .AxisChange(Me.CreateGraphics)
                        Me.GraphControl.Invalidate()
                        If Me.showoppoint Then
                            With .AddCurve(DWSIM.App.GetLocalString("PontodeOperao"), New Double() {ot}, New Double() {op}, Color.Red, ZedGraph.SymbolType.Circle)
                                .Color = Color.Black
                                .Line.IsSmooth = False
                                .Symbol.Fill.Type = ZedGraph.FillType.Solid
                            End With
                        End If
                    End With

                Case 5

                    Dim px1, py1, px2, py2 As New ArrayList
                    Dim i As Integer
                    For i = 0 To TVB.Count - 1
                        px1.Add(cv.ConverterDoSI(su.spmp_temperature, TVB(i)))
                        py1.Add(cv.ConverterDoSI(su.spmp_enthalpy, HB(i)))
                    Next
                    For i = 0 To TVD.Count - 1
                        px2.Add(cv.ConverterDoSI(su.spmp_temperature, TVD(i)))
                        py2.Add(cv.ConverterDoSI(su.spmp_enthalpy, HO(i)))
                    Next

                    With Me.GraphControl.GraphPane
                        .CurveList.Clear()
                        With .AddCurve(DWSIM.App.GetLocalString("PontosdeBolha"), px1.ToArray(GetType(Double)), py1.ToArray(GetType(Double)), Color.SlateBlue, ZedGraph.SymbolType.Circle)
                            .Color = Color.SteelBlue
                            .Line.IsSmooth = False
                            .Symbol.Fill.Type = ZedGraph.FillType.Solid
                        End With
                        With .AddCurve(DWSIM.App.GetLocalString("PontosdeOrvalho"), px2.ToArray(GetType(Double)), py2.ToArray(GetType(Double)), Color.DeepSkyBlue, ZedGraph.SymbolType.Circle)
                            .Color = Color.YellowGreen
                            .Line.IsSmooth = False
                            .Symbol.Fill.Type = ZedGraph.FillType.Solid
                        End With
                        .Title.Text = strname
                        .XAxis.Title.Text = "T / " & su.spmp_temperature
                        .YAxis.Title.Text = "H / " & su.spmp_enthalpy
                        .AxisChange(Me.CreateGraphics)
                        Me.GraphControl.Invalidate()
                        If Me.showoppoint Then
                            With .AddCurve(DWSIM.App.GetLocalString("PontodeOperao"), New Double() {ot}, New Double() {oh}, Color.Red, ZedGraph.SymbolType.Circle)
                                .Color = Color.Black
                                .Line.IsSmooth = False
                                .Symbol.Fill.Type = ZedGraph.FillType.Solid
                            End With
                        End If
                    End With

                Case 6

                    Dim px1, py1, px2, py2 As New ArrayList
                    Dim i As Integer
                    For i = 0 To TVB.Count - 1
                        px1.Add(cv.ConverterDoSI(su.spmp_temperature, TVB(i)))
                        py1.Add(cv.ConverterDoSI(su.spmp_entropy, SB(i)))
                    Next
                    For i = 0 To TVD.Count - 1
                        px2.Add(cv.ConverterDoSI(su.spmp_temperature, TVD(i)))
                        py2.Add(cv.ConverterDoSI(su.spmp_entropy, SO(i)))
                    Next

                    With Me.GraphControl.GraphPane
                        .CurveList.Clear()
                        With .AddCurve(DWSIM.App.GetLocalString("PontosdeBolha"), px1.ToArray(GetType(Double)), py1.ToArray(GetType(Double)), Color.SlateBlue, ZedGraph.SymbolType.Circle)
                            .Color = Color.SteelBlue
                            .Line.IsSmooth = False
                            .Symbol.Fill.Type = ZedGraph.FillType.Solid
                        End With
                        With .AddCurve(DWSIM.App.GetLocalString("PontosdeOrvalho"), px2.ToArray(GetType(Double)), py2.ToArray(GetType(Double)), Color.DeepSkyBlue, ZedGraph.SymbolType.Circle)
                            .Color = Color.YellowGreen
                            .Line.IsSmooth = False
                            .Symbol.Fill.Type = ZedGraph.FillType.Solid
                        End With
                        .Title.Text = strname
                        .XAxis.Title.Text = "T / " & su.spmp_temperature
                        .YAxis.Title.Text = "S / " & su.spmp_entropy
                        .AxisChange(Me.CreateGraphics)
                        Me.GraphControl.Invalidate()
                        If Me.showoppoint Then
                            With .AddCurve(DWSIM.App.GetLocalString("PontodeOperao"), New Double() {ot}, New Double() {os}, Color.Red, ZedGraph.SymbolType.Circle)
                                .Color = Color.Black
                                .Line.IsSmooth = False
                                .Symbol.Fill.Type = ZedGraph.FillType.Solid
                            End With
                        End If
                    End With

                Case 7

                    Dim px1, py1, px2, py2 As New ArrayList
                    Dim i As Integer
                    For i = 0 To TVB.Count - 1
                        px1.Add(cv.ConverterDoSI(su.spmp_temperature, TVB(i)))
                        py1.Add(cv.ConverterDoSI(su.molar_volume, VB(i)))
                    Next
                    For i = 0 To TVB.Count - 1
                        px2.Add(cv.ConverterDoSI(su.spmp_temperature, TVD(i)))
                        py2.Add(cv.ConverterDoSI(su.molar_volume, VO(i)))
                    Next

                    With Me.GraphControl.GraphPane
                        .CurveList.Clear()
                        Dim tmp As Object
                        For Each tmp In PC
                            .AddCurve(DWSIM.App.GetLocalString("PontoCrtico"), New Double() {cv.ConverterDoSI(su.spmp_temperature, tmp(0))}, New Double() {cv.ConverterDoSI(su.molar_volume, tmp(2))}, Color.Red, ZedGraph.SymbolType.Circle).Symbol.Fill.Type = ZedGraph.FillType.Solid
                        Next
                        With .AddCurve(DWSIM.App.GetLocalString("PontosdeBolha"), px1.ToArray(GetType(Double)), py1.ToArray(GetType(Double)), Color.SlateBlue, ZedGraph.SymbolType.Circle)
                            .Color = Color.SteelBlue
                            .Line.IsSmooth = False
                            .Symbol.Fill.Type = ZedGraph.FillType.Solid
                        End With
                        With .AddCurve(DWSIM.App.GetLocalString("PontosdeOrvalho"), px2.ToArray(GetType(Double)), py2.ToArray(GetType(Double)), Color.DeepSkyBlue, ZedGraph.SymbolType.Circle)
                            .Color = Color.YellowGreen
                            .Line.IsSmooth = False
                            .Symbol.Fill.Type = ZedGraph.FillType.Solid
                        End With
                        .Title.Text = strname
                        .XAxis.Title.Text = "T / " & su.spmp_temperature
                        .YAxis.Title.Text = "V / " & su.molar_volume
                        .AxisChange(Me.CreateGraphics)
                        Me.GraphControl.Invalidate()
                        If Me.showoppoint Then
                            With .AddCurve(DWSIM.App.GetLocalString("PontodeOperao"), New Double() {ot}, New Double() {ov}, Color.Red, ZedGraph.SymbolType.Circle)
                                .Color = Color.Black
                                .Line.IsSmooth = False
                                .Symbol.Fill.Type = ZedGraph.FillType.Solid
                            End With
                        End If
                    End With

                Case 8

                    Dim px1, py1, px2, py2 As New ArrayList
                    Dim i As Integer
                    For i = 0 To PB.Count - 1
                        px1.Add(cv.ConverterDoSI(su.molar_volume, VB(i)))
                        py1.Add(cv.ConverterDoSI(su.spmp_pressure, PB(i)))
                    Next
                    For i = 0 To PO.Count - 1
                        px2.Add(cv.ConverterDoSI(su.molar_volume, VO(i)))
                        py2.Add(cv.ConverterDoSI(su.spmp_pressure, PO(i)))
                    Next

                    With Me.GraphControl.GraphPane
                        .CurveList.Clear()
                        Dim tmp As Object
                        For Each tmp In PC
                            .AddCurve(DWSIM.App.GetLocalString("PontoCrtico"), New Double() {cv.ConverterDoSI(su.molar_volume, tmp(2))}, New Double() {cv.ConverterDoSI(su.spmp_pressure, tmp(1))}, Color.Red, ZedGraph.SymbolType.Circle).Symbol.Fill.Type = ZedGraph.FillType.Solid
                        Next
                        With .AddCurve(DWSIM.App.GetLocalString("PontosdeBolha"), px1.ToArray(GetType(Double)), py1.ToArray(GetType(Double)), Color.SlateBlue, ZedGraph.SymbolType.Circle)
                            .Color = Color.SteelBlue
                            .Line.IsSmooth = False
                            .Symbol.Fill.Type = ZedGraph.FillType.Solid
                        End With
                        With .AddCurve(DWSIM.App.GetLocalString("PontosdeOrvalho"), px2.ToArray(GetType(Double)), py2.ToArray(GetType(Double)), Color.DeepSkyBlue, ZedGraph.SymbolType.Circle)
                            .Color = Color.YellowGreen
                            .Line.IsSmooth = False
                            .Symbol.Fill.Type = ZedGraph.FillType.Solid
                        End With
                        .Title.Text = strname
                        .XAxis.Title.Text = "V / " & su.molar_volume
                        .YAxis.Title.Text = "P / " & su.spmp_pressure
                        .AxisChange(Me.CreateGraphics)
                        Me.GraphControl.Invalidate()
                        If Me.showoppoint Then
                            With .AddCurve(DWSIM.App.GetLocalString("PontodeOperao"), New Double() {ov}, New Double() {op}, Color.Red, ZedGraph.SymbolType.Circle)
                                .Color = Color.Black
                                .Line.IsSmooth = False
                                .Symbol.Fill.Type = ZedGraph.FillType.Solid
                            End With
                        End If
                    End With

                Case 9

                    Dim px1, py1, px2, py2 As New ArrayList
                    Dim i As Integer
                    For i = 0 To TVB.Count - 1
                        px1.Add(cv.ConverterDoSI(su.molar_volume, VB(i)))
                        py1.Add(cv.ConverterDoSI(su.spmp_temperature, TVB(i)))
                    Next
                    For i = 0 To TVD.Count - 1
                        px2.Add(cv.ConverterDoSI(su.molar_volume, VO(i)))
                        py2.Add(cv.ConverterDoSI(su.spmp_temperature, TVD(i)))
                    Next

                    With Me.GraphControl.GraphPane
                        .CurveList.Clear()
                        Dim tmp As Object
                        For Each tmp In PC
                            .AddCurve(DWSIM.App.GetLocalString("PontoCrtico"), New Double() {cv.ConverterDoSI(su.molar_volume, tmp(2))}, New Double() {cv.ConverterDoSI(su.spmp_temperature, tmp(0))}, Color.Red, ZedGraph.SymbolType.Circle).Symbol.Fill.Type = ZedGraph.FillType.Solid
                        Next
                        With .AddCurve(DWSIM.App.GetLocalString("PontosdeBolha"), px1.ToArray(GetType(Double)), py1.ToArray(GetType(Double)), Color.SlateBlue, ZedGraph.SymbolType.Circle)
                            .Color = Color.SteelBlue
                            .Line.IsSmooth = False
                            .Symbol.Fill.Type = ZedGraph.FillType.Solid
                        End With
                        With .AddCurve(DWSIM.App.GetLocalString("PontosdeOrvalho"), px2.ToArray(GetType(Double)), py2.ToArray(GetType(Double)), Color.DeepSkyBlue, ZedGraph.SymbolType.Circle)
                            .Color = Color.YellowGreen
                            .Line.IsSmooth = False
                            .Symbol.Fill.Type = ZedGraph.FillType.Solid
                        End With
                        .Title.Text = ""
                        .XAxis.Title.Text = "V / " & su.molar_volume
                        .YAxis.Title.Text = "T / " & su.spmp_temperature
                        .AxisChange(Me.CreateGraphics)
                        Me.GraphControl.Invalidate()
                        If Me.showoppoint Then
                            With .AddCurve(DWSIM.App.GetLocalString("PontodeOperao"), New Double() {ov}, New Double() {ot}, Color.Red, ZedGraph.SymbolType.Circle)
                                .Color = Color.Black
                                .Line.IsSmooth = False
                                .Symbol.Fill.Type = ZedGraph.FillType.Solid
                            End With
                        End If
                    End With

                Case 10

                    Dim px1, py1, px2, py2 As New ArrayList
                    Dim i As Integer
                    For i = 0 To TVB.Count - 1
                        px1.Add(cv.ConverterDoSI(su.molar_volume, VB(i)))
                        py1.Add(cv.ConverterDoSI(su.spmp_enthalpy, HB(i)))
                    Next
                    For i = 0 To TVD.Count - 1
                        px2.Add(cv.ConverterDoSI(su.molar_volume, VO(i)))
                        py2.Add(cv.ConverterDoSI(su.spmp_enthalpy, HO(i)))
                    Next

                    With Me.GraphControl.GraphPane
                        .CurveList.Clear()
                        With .AddCurve(DWSIM.App.GetLocalString("PontosdeBolha"), px1.ToArray(GetType(Double)), py1.ToArray(GetType(Double)), Color.SlateBlue, ZedGraph.SymbolType.Circle)
                            .Color = Color.SteelBlue
                            .Line.IsSmooth = False
                            .Symbol.Fill.Type = ZedGraph.FillType.Solid
                        End With
                        With .AddCurve(DWSIM.App.GetLocalString("PontosdeOrvalho"), px2.ToArray(GetType(Double)), py2.ToArray(GetType(Double)), Color.DeepSkyBlue, ZedGraph.SymbolType.Circle)
                            .Color = Color.YellowGreen
                            .Line.IsSmooth = False
                            .Symbol.Fill.Type = ZedGraph.FillType.Solid
                        End With
                        .Title.Text = ""
                        .XAxis.Title.Text = "V / " & su.molar_volume
                        .YAxis.Title.Text = "H / " & su.spmp_enthalpy
                        .AxisChange(Me.CreateGraphics)
                        Me.GraphControl.Invalidate()
                        If Me.showoppoint Then
                            With .AddCurve(DWSIM.App.GetLocalString("PontodeOperao"), New Double() {ov}, New Double() {oh}, Color.Red, ZedGraph.SymbolType.Circle)
                                .Color = Color.Black
                                .Line.IsSmooth = False
                                .Symbol.Fill.Type = ZedGraph.FillType.Solid
                            End With
                        End If
                    End With

                Case 11

                    Dim px1, py1, px2, py2 As New ArrayList
                    Dim i As Integer
                    For i = 0 To TVB.Count - 1
                        px1.Add(cv.ConverterDoSI(su.molar_volume, VB(i)))
                        py1.Add(cv.ConverterDoSI(su.spmp_entropy, SB(i)))
                    Next
                    For i = 0 To TVD.Count - 1
                        px2.Add(cv.ConverterDoSI(su.molar_volume, VO(i)))
                        py2.Add(cv.ConverterDoSI(su.spmp_entropy, SO(i)))
                    Next

                    With Me.GraphControl.GraphPane
                        .CurveList.Clear()
                        With .AddCurve(DWSIM.App.GetLocalString("PontosdeBolha"), px1.ToArray(GetType(Double)), py1.ToArray(GetType(Double)), Color.SlateBlue, ZedGraph.SymbolType.Circle)
                            .Color = Color.SteelBlue
                            .Line.IsSmooth = False
                            .Symbol.Fill.Type = ZedGraph.FillType.Solid
                        End With
                        With .AddCurve(DWSIM.App.GetLocalString("PontosdeOrvalho"), px2.ToArray(GetType(Double)), py2.ToArray(GetType(Double)), Color.DeepSkyBlue, ZedGraph.SymbolType.Circle)
                            .Color = Color.YellowGreen
                            .Line.IsSmooth = False
                            .Symbol.Fill.Type = ZedGraph.FillType.Solid
                        End With
                        .Title.Text = strname
                        .XAxis.Title.Text = "V / " & su.molar_volume
                        .YAxis.Title.Text = "S / " & su.spmp_entropy
                        .AxisChange(Me.CreateGraphics)
                        Me.GraphControl.Invalidate()
                        If Me.showoppoint Then
                            With .AddCurve(DWSIM.App.GetLocalString("PontodeOperao"), New Double() {ov}, New Double() {os}, Color.Red, ZedGraph.SymbolType.Circle)
                                .Color = Color.Black
                                .Line.IsSmooth = False
                                .Symbol.Fill.Type = ZedGraph.FillType.Solid
                            End With
                        End If
                    End With

            End Select

            With Me.GraphControl.GraphPane.Title
                .Text = Me.ComboBox1.SelectedItem & " (PP: " & Me.Frm.Options.SelectedPropertyPackage.ComponentName & ")"
                .FontSpec.Size = 14
            End With

        End If

    End Sub

    Private Sub FormPhEnv_Shown(ByVal sender As Object, ByVal e As System.EventArgs) Handles Me.Shown

        Me.loaded = True

    End Sub

    Private Sub BackgroundWorker1_DoWork(ByVal sender As Object, ByVal e As System.ComponentModel.DoWorkEventArgs) Handles BackgroundWorker1.DoWork

        Dim gobj As Microsoft.Msdn.Samples.GraphicObjects.GraphicObject = Nothing
        gobj = FormFlowsheet.SearchSurfaceObjectsByTag(Me.ComboBox3.SelectedItem, Frm.FormSurface.FlowsheetDesignSurface)
        Me.mat = Frm.Collections.CLCS_MaterialStreamCollection(gobj.Name)
        Me.strname = gobj.Tag

        If Me.showoppoint Then
            ot = cv.ConverterDoSI(su.spmp_temperature, mat.Fases(0).SPMProperties.temperature.GetValueOrDefault)
            op = cv.ConverterDoSI(su.spmp_pressure, mat.Fases(0).SPMProperties.pressure.GetValueOrDefault)
            ov = mat.Fases(0).SPMProperties.molecularWeight.GetValueOrDefault / mat.Fases(0).SPMProperties.density.GetValueOrDefault / 1000
            oh = cv.ConverterDoSI(su.spmp_enthalpy, mat.Fases(0).SPMProperties.enthalpy.GetValueOrDefault)
            os = cv.ConverterDoSI(su.spmp_entropy, mat.Fases(0).SPMProperties.entropy.GetValueOrDefault)
        End If

        Dim pp As DWSIM.SimulationObjects.PropertyPackages.PropertyPackage = Frm.Options.SelectedPropertyPackage

        pp.CurrentMaterialStream = mat

        Dim th1, th2, ph1, ph2 As New ArrayList

        th1.Add(CDbl(0))
        th2.Add(CDbl(0))
        ph1.Add(CDbl(0))
        ph2.Add(CDbl(0))

        e.Result = New Object() {pp.DW_ReturnPhaseEnvelope(e.Argument, Me.BackgroundWorker1), ph1, th1, ph2, th2}

    End Sub

    Private Sub BackgroundWorker1_ProgressChanged(sender As Object, e As System.ComponentModel.ProgressChangedEventArgs) Handles BackgroundWorker1.ProgressChanged
        fpec.Label2.Text = fpec.Label2.Tag.ToString + " " + e.UserState.ToString
    End Sub

    Private Sub BackgroundWorker1_RunWorkerCompleted(ByVal sender As Object, ByVal e As System.ComponentModel.RunWorkerCompletedEventArgs) Handles BackgroundWorker1.RunWorkerCompleted

        Me.Enabled = True

        fpec.Close()

        Dim r = e.Result(0)

        '{TVB, PB, HB, SB, VB, TVD, PO, HO, SO, VO, TE, PE, TH, PHsI, PHsII, TC, PC, VC}
        TVB = r(0)
        PB = r(1)
        HB = r(2)
        SB = r(3)
        VB = r(4)
        TVD = r(5)
        PO = r(6)
        HO = r(7)
        SO = r(8)
        VO = r(9)
        TE = r(10)
        PE = r(11)
        PHsI = e.Result(1)
        THsI = e.Result(2)
        PHsII = e.Result(3)
        THsII = e.Result(4)
        PC = r(15)
        TQ = r(16)
        PQ = r(17)

        calculated = True

        With Me.Grid1.Columns
            .Clear()
            .Add("c1", "Tbol (" & su.spmp_temperature & ")")
            .Add("c2", DWSIM.App.GetLocalString("Pbol") & su.spmp_pressure & ")")
            .Add("c3", "Hbol (" & su.spmp_enthalpy & ")")
            .Add("c4", DWSIM.App.GetLocalString("Sbol") & su.spmp_entropy & ")")
            .Add("c5", DWSIM.App.GetLocalString("Vbolm3mol"))
            .Add("c6", "Torv (" & su.spmp_temperature & ")")
            .Add("c7", DWSIM.App.GetLocalString("Porv") & su.spmp_pressure & ")")
            .Add("c8", "Horv (" & su.spmp_enthalpy & ")")
            .Add("c9", "Sorv (" & su.spmp_entropy & ")")
            .Add("c10", DWSIM.App.GetLocalString("Vorvm3mol"))
            .Add("c11", "Test (" & su.spmp_temperature & ")")
            .Add("c12", DWSIM.App.GetLocalString("Pest") & su.spmp_pressure & ")")
            If Me.CheckBox1.Checked Then
                .Add("c13", "TQ (" & su.spmp_temperature & ")")
                .Add("c14", "PQ (" & su.spmp_pressure & ")")
            End If
        End With

        For Each c As DataGridViewColumn In Me.Grid1.Columns
            c.SortMode = DataGridViewColumnSortMode.NotSortable
            c.DefaultCellStyle.Alignment = DataGridViewContentAlignment.MiddleRight
            c.HeaderCell.Style.Alignment = DataGridViewContentAlignment.MiddleCenter
        Next

        Dim maxl As Integer = DWSIM.MathEx.Common.Max(New Object() {TVB.Count, TVD.Count, TE.Count, TQ.Count}) - 1

        Dim k, j As Integer
        Dim maxc As Integer = 11
        If Me.CheckBox1.Checked Then
            maxc = 13
        End If
        Dim data(maxc, maxl) As String

        j = 0
        For Each d As Double In TVB
            data(0, j) = cv.ConverterDoSI(su.spmp_temperature, d)
            data(1, j) = cv.ConverterDoSI(su.spmp_pressure, PB(j))
            data(2, j) = cv.ConverterDoSI(su.spmp_enthalpy, HB(j))
            data(3, j) = cv.ConverterDoSI(su.spmp_entropy, SB(j))
            data(4, j) = VB(j)
            j = j + 1
        Next
        j = 0
        For Each d As Double In TVD
            data(5, j) = cv.ConverterDoSI(su.spmp_temperature, d)
            data(6, j) = cv.ConverterDoSI(su.spmp_pressure, PO(j))
            data(7, j) = cv.ConverterDoSI(su.spmp_enthalpy, HO(j))
            data(8, j) = cv.ConverterDoSI(su.spmp_entropy, SO(j))
            data(9, j) = VO(j)
            j = j + 1
        Next
        j = 0
        For Each d As Double In TE
            data(10, j) = cv.ConverterDoSI(su.spmp_temperature, d)
            data(11, j) = cv.ConverterDoSI(su.spmp_pressure, PE(j))
            j = j + 1
        Next
        If Me.CheckBox1.Checked Then
            j = 0
            For Each d As Double In TQ
                data(12, j) = cv.ConverterDoSI(su.spmp_temperature, d)
                data(13, j) = cv.ConverterDoSI(su.spmp_pressure, PQ(j))
                j = j + 1
            Next
        End If

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
                Loop Until j = maxc + 1
                k = k + 1
            Loop Until k = maxl + 1
        End With

        Call Me.ComboBox1_SelectedIndexChanged(sender, e)

    End Sub

    Private Sub CheckBox1_CheckedChanged(ByVal sender As System.Object, ByVal e As System.EventArgs) Handles CheckBox1.CheckedChanged
        If Me.CheckBox1.Checked Then Me.TextBox1.Enabled = True Else Me.TextBox1.Enabled = False
    End Sub

    Private Sub CheckBox2_CheckedChanged(ByVal sender As System.Object, ByVal e As System.EventArgs) Handles CheckBox2.CheckedChanged
        If Me.CheckBox2.Checked Then Me.showoppoint = True Else Me.showoppoint = False
    End Sub

    Private Sub FormPhEnv_HelpRequested(sender As System.Object, hlpevent As System.Windows.Forms.HelpEventArgs) Handles MyBase.HelpRequested
        DWSIM.App.HelpRequested("UT_PhaseEnvelope.htm")
    End Sub
End Class