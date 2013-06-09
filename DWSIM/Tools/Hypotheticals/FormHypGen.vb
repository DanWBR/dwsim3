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

Imports DWSIM.DWSIM.Utilities.PetroleumCharacterization
Imports DWSIM.DWSIM.SimulationObjects.PropertyPackages.Auxiliary
Imports DWSIM.DWSIM.Utilities.Hypos.Methods

Public Class FormHypGen

    Inherits System.Windows.Forms.Form

    Public Loaded = False
    Public HypError As Boolean = True

    Public ValUNIFACOK As Boolean = False
    Public ValPropsOK As Boolean = False

    Public ChildParent As FormFlowsheet
    Public frmCheck As FormCheckHypData

    Public constprop As DWSIM.ClassesBasicasTermodinamica.ConstantProperties
    Public vxCp, vyCp, vxPvap, vyPvap, vxVisc, vyVisc, vxHv, vyHv As New ArrayList
    Public Tb, Tc, Pc, Vc, Zc, Hf, Gf, Hvb, MM, w, CSw, CSsp, CSlv As Double

    Public su As DWSIM.SistemasDeUnidades.Unidades
    Public cv As DWSIM.SistemasDeUnidades.Conversor
    Public nf As String

    Public methods As DWSIM.Utilities.Hypos.Methods.HYP
    Friend methods2 As DWSIM.SimulationObjects.PropertyPackages.Auxiliary.PROPS
    Public jb As DWSIM.Utilities.Hypos.Methods.Joback
    Friend m_props As PROPS

    Private Sub FormHypGen_Load(ByVal sender As System.Object, ByVal e As System.EventArgs) Handles MyBase.Load

        cv = New DWSIM.SistemasDeUnidades.Conversor
        jb = New DWSIM.Utilities.Hypos.Methods.Joback
        methods2 = New DWSIM.SimulationObjects.PropertyPackages.Auxiliary.PROPS

        Dim Rnd As New Random()

        Me.TextBox1.Text = "HYPO_" + Rnd.Next(1000, 9999).ToString

        methods = New DWSIM.Utilities.Hypos.Methods.HYP()

        Me.ChildParent = My.Application.ActiveSimulation

        su = ChildParent.Options.SelectedUnitSystem
        nf = ChildParent.Options.NumberFormat

        Me.ComboBox1.SelectedIndex = 0
        Me.ComboBox2.SelectedIndex = 0
        Me.ComboBox3.SelectedIndex = 0
        Me.ComboBox4.SelectedIndex = 0
        Me.ComboBox5.SelectedIndex = 0
        Me.ComboBox6.SelectedIndex = 0
        Me.ComboBox7.SelectedIndex = 0
        Me.ComboBox8.SelectedIndex = 0
        Me.ComboBox9.SelectedIndex = 0
        Me.ComboBox10.SelectedIndex = 0
        Me.ComboBox11.SelectedIndex = 0
        Me.ComboBox12.SelectedIndex = 0
        Me.ComboBox13.SelectedIndex = 0

        With Me.GraphCp.GraphPane
            .CurveList.Clear()
            '.AddCurve(Me.ComboBox2.SelectedItem, Me.m_vx, Me.m_vy, Color.Blue, ZedGraph.SymbolType.Circle)
            .Title.Text = DWSIM.App.GetLocalString("CapacidadeCalorficaP")
            .XAxis.Title.Text = "T"
            .YAxis.Title.Text = "Cp"
            .AxisChange(Me.CreateGraphics)
        End With
        Me.GraphCp.Invalidate()
        With Me.GraphPvap.GraphPane
            .CurveList.Clear()
            '.AddCurve(Me.ComboBox2.SelectedItem, Me.m_vx, Me.m_vy, Color.Blue, ZedGraph.SymbolType.Circle)
            .Title.Text = DWSIM.App.GetLocalString("PressodeVapor")
            .XAxis.Title.Text = "T"
            .YAxis.Title.Text = "Pvap"
            .AxisChange(Me.CreateGraphics)
        End With
        Me.GraphPvap.Invalidate()
        With Me.GraphVisc.GraphPane
            .CurveList.Clear()
            '.AddCurve(Me.ComboBox2.SelectedItem, Me.m_vx, Me.m_vy, Color.Blue, ZedGraph.SymbolType.Circle)
            .Title.Text = DWSIM.App.GetLocalString("ViscosidadeLquido")
            .XAxis.Title.Text = "T"
            .YAxis.Title.Text = "Visc"
            .AxisChange(Me.CreateGraphics)
        End With
        Me.GraphVisc.Invalidate()

        'Grid UNIFAC
        With Me.GridUNIFAC.Rows
            .Clear()
            For Each jg As JobackGroup In jb.UGroups.Values
                .Add(New Object() {CInt(0)})
                .Item(.Count - 1).HeaderCell.Value = jg.Group
                .Item(.Count - 1).HeaderCell.Tag = jg.ID
            Next
        End With

        'Grid Propriedades
        With Me.GridProps.Rows
            .Clear()
            .Add(New Object() {"", su.spmp_molecularWeight, CheckState.Unchecked})
            .Add(New Object() {"", su.spmp_temperature, CheckState.Unchecked})
            .Add(New Object() {"", su.spmp_pressure, CheckState.Unchecked})
            .Add(New Object() {"", su.molar_volume, CheckState.Unchecked})
            .Add(New Object() {"", "-", CheckState.Unchecked})
            .Add(New Object() {"", "-", CheckState.Unchecked})
            .Add(New Object() {"", su.spmp_enthalpy, CheckState.Unchecked})
            .Add(New Object() {"", su.spmp_temperature, CheckState.Unchecked})
            .Add(New Object() {"", su.spmp_enthalpy, CheckState.Unchecked})
            .Add(New Object() {"", su.spmp_enthalpy, CheckState.Unchecked})
            .Add(New Object() {"", "-", CheckState.Unchecked})
            .Add(New Object() {"", "(cal/mL)^0.5", CheckState.Unchecked})
            .Add(New Object() {"", "mL/mol", CheckState.Unchecked})
            .Item(0).HeaderCell.Value = "Massa Molar"
            .Item(1).HeaderCell.Value = DWSIM.App.GetLocalString("TemperaturaCrtica")
            .Item(2).HeaderCell.Value = DWSIM.App.GetLocalString("PressoCrtica")
            .Item(3).HeaderCell.Value = DWSIM.App.GetLocalString("VolumeCrtico")
            .Item(4).HeaderCell.Value = DWSIM.App.GetLocalString("CompressibilidadeCrt")
            .Item(5).HeaderCell.Value = DWSIM.App.GetLocalString("FatorAcntrico")
            .Item(6).HeaderCell.Value = DWSIM.App.GetLocalString("EntalpiadeFormaodoGs")
            .Item(7).HeaderCell.Value = DWSIM.App.GetLocalString("TemperaturaNormaldeE")
            .Item(8).HeaderCell.Value = DWSIM.App.GetLocalString("EntalpiadeVaporizaoa")
            .Item(9).HeaderCell.Value = DWSIM.App.GetLocalString("EnergiadeGibbsdeForm")
            .Item(10).HeaderCell.Value = DWSIM.App.GetLocalString("ChaoSeaderAcentricFactor")
            .Item(11).HeaderCell.Value = DWSIM.App.GetLocalString("ChaoSeaderSolubilityParameter")
            .Item(12).HeaderCell.Value = DWSIM.App.GetLocalString("ChaoSeaderLiquidMolarVolume")
        End With

    End Sub

    Private Sub KryptonDataGridView1_CellEndEdit(ByVal sender As Object, ByVal e As System.Windows.Forms.DataGridViewCellEventArgs) Handles KryptonDataGridView1.CellEndEdit

        Dim row1 As DataGridViewRow
        Try
            vxCp.Clear()
            vyCp.Clear()
            For Each row1 In Me.KryptonDataGridView1.Rows
                If Double.TryParse(row1.Cells(0).Value, New Double) And Double.TryParse(row1.Cells(1).Value, New Double) Then
                    vxCp.Add(cv.ConverterParaSI(Me.ComboBox2.SelectedItem, CDbl(row1.Cells(0).Value)))
                    vyCp.Add(cv.ConverterParaSI(Me.ComboBox3.SelectedItem, CDbl(row1.Cells(1).Value)))
                End If
            Next
            With Me.GraphCp.GraphPane
                Dim vx(vxCp.Count - 1), vy(vyCp.Count - 1) As Double
                Dim i As Integer = 0
                Do
                    vx(i) = cv.ConverterDoSI(Me.ComboBox2.SelectedItem, vxCp(i))
                    vy(i) = cv.ConverterDoSI(Me.ComboBox3.SelectedItem, vyCp(i))
                    i += 1
                Loop Until i = vxCp.Count
                .CurveList.Clear()
                .AddCurve(Me.ComboBox2.SelectedItem, vx, vy, Color.Blue, ZedGraph.SymbolType.Circle)
                .AxisChange(Me.CreateGraphics)
            End With
            Me.GraphCp.Invalidate()
        Catch ex As Exception
            Console.WriteLine(ex.Message)
        End Try

    End Sub

    Private Sub KryptonDataGridView3_CellEndEdit(ByVal sender As Object, ByVal e As System.Windows.Forms.DataGridViewCellEventArgs) Handles KryptonDataGridView3.CellEndEdit
        Dim row1 As DataGridViewRow
        Try
            vxPvap.Clear()
            vyPvap.Clear()
            For Each row1 In Me.KryptonDataGridView3.Rows
                If Double.TryParse(row1.Cells(0).Value, New Double) And Double.TryParse(row1.Cells(1).Value, New Double) Then
                    vxPvap.Add(cv.ConverterParaSI(Me.ComboBox7.SelectedItem, CDbl(row1.Cells(0).Value)))
                    vyPvap.Add(cv.ConverterParaSI(Me.ComboBox6.SelectedItem, CDbl(row1.Cells(1).Value)))
                End If
            Next
            With Me.GraphPvap.GraphPane
                Dim vx(vxPvap.Count - 1), vy(vyPvap.Count - 1) As Double
                Dim i As Integer = 0
                Do
                    vx(i) = cv.ConverterDoSI(Me.ComboBox7.SelectedItem, vxPvap(i))
                    vy(i) = cv.ConverterDoSI(Me.ComboBox6.SelectedItem, vyPvap(i))
                    i += 1
                Loop Until i = vxPvap.Count
                .CurveList.Clear()
                .AddCurve(Me.ComboBox2.SelectedItem, vx, vy, Color.Blue, ZedGraph.SymbolType.Circle)
                .AxisChange(Me.CreateGraphics)
            End With
            Me.GraphPvap.Invalidate()
        Catch ex As Exception
            Console.WriteLine(ex.Message)
        End Try
    End Sub

    Private Sub KryptonDataGridView4_CellEndEdit(ByVal sender As Object, ByVal e As System.Windows.Forms.DataGridViewCellEventArgs) Handles KryptonDataGridView4.CellEndEdit
        Dim row1 As DataGridViewRow
        Try
            vxVisc.Clear()
            vyVisc.Clear()
            For Each row1 In Me.KryptonDataGridView4.Rows
                If Double.TryParse(row1.Cells(0).Value, New Double) And Double.TryParse(row1.Cells(1).Value, New Double) Then
                    vxVisc.Add(cv.ConverterParaSI(Me.ComboBox10.SelectedItem, CDbl(row1.Cells(0).Value)))
                    vyVisc.Add(cv.ConverterParaSI(Me.ComboBox9.SelectedItem, CDbl(row1.Cells(1).Value)))
                End If
            Next
            With Me.GraphVisc.GraphPane
                Dim vx(vxVisc.Count - 1), vy(vyVisc.Count - 1) As Double
                Dim i As Integer = 0
                Do
                    vx(i) = cv.ConverterDoSI(Me.ComboBox10.SelectedItem, vxVisc(i))
                    vy(i) = cv.ConverterDoSI(Me.ComboBox9.SelectedItem, vyVisc(i))
                    i += 1
                Loop Until i = vxVisc.Count
                .CurveList.Clear()
                .AddCurve(Me.ComboBox2.SelectedItem, vx, vy, Color.Blue, ZedGraph.SymbolType.Circle)
                .AxisChange(Me.CreateGraphics)
            End With
            Me.GraphVisc.Invalidate()
        Catch ex As Exception
            Console.WriteLine(ex.Message)
        End Try
    End Sub

    Private Sub RadioButton11_CheckedChanged(ByVal sender As System.Object, ByVal e As System.EventArgs) Handles RadioButton11.CheckedChanged
        If RadioButton11.Checked Then
            TextBox3.Enabled = False
            ComboBox13.Enabled = True
        Else
            TextBox3.Enabled = True
            ComboBox13.Enabled = False
        End If
    End Sub

    Private Sub RadioButton1_CheckedChanged(ByVal sender As System.Object, ByVal e As System.EventArgs) Handles RadioButton1.CheckedChanged
        If RadioButton1.Checked Then
            ComboBox1.Enabled = True
            GroupBox1.Enabled = False
        Else
            ComboBox1.Enabled = False
            GroupBox1.Enabled = True
        End If
    End Sub

    Private Sub RadioButton6_CheckedChanged(ByVal sender As System.Object, ByVal e As System.EventArgs) Handles RadioButton6.CheckedChanged
        If RadioButton6.Checked Then
            ComboBox8.Enabled = True
            GroupBox3.Enabled = False
        Else
            ComboBox8.Enabled = False
            GroupBox3.Enabled = True
        End If
    End Sub

    Private Sub RadioButton8_CheckedChanged(ByVal sender As System.Object, ByVal e As System.EventArgs) Handles RadioButton8.CheckedChanged
        If RadioButton8.Checked Then
            ComboBox11.Enabled = True
            GroupBox4.Enabled = False
        Else
            ComboBox11.Enabled = False
            GroupBox4.Enabled = True
        End If
    End Sub

    Private Sub GridProps_CellValidating(ByVal sender As Object, ByVal e As System.Windows.Forms.DataGridViewCellValidatingEventArgs) Handles GridProps.CellValidating
        If Me.Loaded Then
            If e.ColumnIndex = 0 Then
                Select Case e.RowIndex
                    Case 0
                        If Double.TryParse(e.FormattedValue, New Double) = False Then
                            GridProps.Rows(e.RowIndex).ErrorText = _
                                DWSIM.App.GetLocalString("Ovalorinseridoinvlid")
                            e.Cancel = True
                        ElseIf CDbl(e.FormattedValue) <= 0 Then
                            GridProps.Rows(e.RowIndex).ErrorText = _
                                DWSIM.App.GetLocalString("Ovalorinseridoinvlid")
                            e.Cancel = True
                        End If
                    Case 1
                        If Double.TryParse(e.FormattedValue, New Double) = False Then
                            GridProps.Rows(e.RowIndex).ErrorText = _
                                DWSIM.App.GetLocalString("Ovalorinseridoinvlid")
                            e.Cancel = True
                        ElseIf CDbl(e.FormattedValue) <= 1 Then
                            GridProps.Rows(e.RowIndex).ErrorText = _
                                DWSIM.App.GetLocalString("Ovalorinseridoinvlid")
                            e.Cancel = True
                        End If
                    Case 2
                        If Double.TryParse(e.FormattedValue, New Double) = False Then
                            GridProps.Rows(e.RowIndex).ErrorText = _
                                DWSIM.App.GetLocalString("Ovalorinseridoinvlid")
                            e.Cancel = True
                        ElseIf CDbl(e.FormattedValue) <= 0 Then
                            GridProps.Rows(e.RowIndex).ErrorText = _
                                DWSIM.App.GetLocalString("Ovalorinseridoinvlid")
                            e.Cancel = True
                        End If
                    Case 3
                        If Double.TryParse(e.FormattedValue, New Double) = False Then
                            GridProps.Rows(e.RowIndex).ErrorText = _
                                DWSIM.App.GetLocalString("Ovalorinseridoinvlid")
                            e.Cancel = True
                        ElseIf CDbl(e.FormattedValue) <= 0 Then
                            GridProps.Rows(e.RowIndex).ErrorText = _
                                DWSIM.App.GetLocalString("Ovalorinseridoinvlid")
                            e.Cancel = True
                        End If
                    Case 4
                        If Double.TryParse(e.FormattedValue, New Double) = False Then
                            GridProps.Rows(e.RowIndex).ErrorText = _
                                DWSIM.App.GetLocalString("Ovalorinseridoinvlid")
                            e.Cancel = True
                        ElseIf CDbl(e.FormattedValue) <= 0 Then
                            GridProps.Rows(e.RowIndex).ErrorText = _
                                DWSIM.App.GetLocalString("Ovalorinseridoinvlid")
                            e.Cancel = True
                        End If
                    Case 5
                        If Double.TryParse(e.FormattedValue, New Double) = False Then
                            GridProps.Rows(e.RowIndex).ErrorText = _
                                DWSIM.App.GetLocalString("Ovalorinseridoinvlid")
                            e.Cancel = True
                        End If
                    Case 6
                        If Double.TryParse(e.FormattedValue, New Double) = False Then
                            GridProps.Rows(e.RowIndex).ErrorText = _
                                DWSIM.App.GetLocalString("Ovalorinseridoinvlid")
                            e.Cancel = True
                        End If
                    Case 7
                        If Double.TryParse(e.FormattedValue, New Double) = False Then
                            GridProps.Rows(e.RowIndex).ErrorText = _
                                DWSIM.App.GetLocalString("Ovalorinseridoinvlid")
                            e.Cancel = True
                        ElseIf CDbl(e.FormattedValue) <= 0 Then
                            GridProps.Rows(e.RowIndex).ErrorText = _
                                DWSIM.App.GetLocalString("Ovalorinseridoinvlid")
                            e.Cancel = True
                        End If
                    Case 8
                        If Double.TryParse(e.FormattedValue, New Double) = False Then
                            GridProps.Rows(e.RowIndex).ErrorText = _
                                DWSIM.App.GetLocalString("Ovalorinseridoinvlid")
                            e.Cancel = True
                        End If
                    Case 9
                        If Double.TryParse(e.FormattedValue, New Double) = False Then
                            GridProps.Rows(e.RowIndex).ErrorText = _
                                DWSIM.App.GetLocalString("Ovalorinseridoinvlid")
                            e.Cancel = True
                        End If
                    Case 10
                        If Double.TryParse(e.FormattedValue, New Double) = False Then
                            GridProps.Rows(e.RowIndex).ErrorText = _
                                DWSIM.App.GetLocalString("Ovalorinseridoinvlid")
                            e.Cancel = True
                        End If
                    Case 11
                        If Double.TryParse(e.FormattedValue, New Double) = False Then
                            GridProps.Rows(e.RowIndex).ErrorText = _
                                DWSIM.App.GetLocalString("Ovalorinseridoinvlid")
                            e.Cancel = True
                        End If
                    Case 12
                        If Double.TryParse(e.FormattedValue, New Double) = False Then
                            GridProps.Rows(e.RowIndex).ErrorText = _
                                DWSIM.App.GetLocalString("Ovalorinseridoinvlid")
                            e.Cancel = True
                        End If
                End Select
            End If
        End If
    End Sub

    Private Sub GridProps_CellValueChanged(ByVal sender As Object, ByVal e As System.Windows.Forms.DataGridViewCellEventArgs) Handles GridProps.CellValueChanged
        If Me.Loaded Then
            GridProps.Rows(e.RowIndex).ErrorText = String.Empty
            If e.ColumnIndex = 2 Then
                If Me.GridProps.Rows(e.RowIndex).Cells(2).Value = 1 Then
                    Me.GridProps.Rows(e.RowIndex).Cells(0).ReadOnly = False
                    Me.GridProps.Rows(e.RowIndex).Cells(0).Style.ForeColor = Color.Blue
                Else
                    Me.GridProps.Rows(e.RowIndex).Cells(0).ReadOnly = True
                    Me.GridProps.Rows(e.RowIndex).Cells(0).Style.ForeColor = Color.SteelBlue
                End If
            ElseIf e.ColumnIndex = 0 Then
                Me.ValPropsOK = True
                If Me.GridProps.Rows(e.RowIndex).Cells(2).Value = True Then
                    Select Case e.RowIndex
                        Case 0
                            Me.MM = cv.ConverterParaSI(su.spmp_molecularWeight, Me.GridProps.Rows(e.RowIndex).Cells(0).Value)
                        Case 1
                            Me.Tc = cv.ConverterParaSI(su.spmp_temperature, Me.GridProps.Rows(e.RowIndex).Cells(0).Value)
                        Case 2
                            Me.Pc = cv.ConverterParaSI(su.spmp_pressure, Me.GridProps.Rows(e.RowIndex).Cells(0).Value)
                        Case 3
                            Me.Vc = cv.ConverterParaSI(su.molar_volume, Me.GridProps.Rows(e.RowIndex).Cells(0).Value)
                        Case 4
                            Me.Zc = Me.GridProps.Rows(e.RowIndex).Cells(0).Value
                        Case 5
                            Me.w = Me.GridProps.Rows(e.RowIndex).Cells(0).Value
                        Case 6
                            Me.Hf = cv.ConverterParaSI(su.spmp_enthalpy, Me.GridProps.Rows(e.RowIndex).Cells(0).Value)
                        Case 7
                            Me.Tb = cv.ConverterParaSI(su.spmp_temperature, Me.GridProps.Rows(e.RowIndex).Cells(0).Value)
                        Case 8
                            Me.Hvb = cv.ConverterParaSI(su.spmp_enthalpy, Me.GridProps.Rows(e.RowIndex).Cells(0).Value)
                        Case 9
                            Me.Gf = cv.ConverterParaSI(su.spmp_enthalpy, Me.GridProps.Rows(e.RowIndex).Cells(0).Value)
                        Case 10
                            Me.CSw = Me.GridProps.Rows(e.RowIndex).Cells(0).Value
                        Case 11
                            Me.CSsp = Me.GridProps.Rows(e.RowIndex).Cells(0).Value
                        Case 12
                            Me.CSlv = Me.GridProps.Rows(e.RowIndex).Cells(0).Value
                    End Select
                End If
            End If
        End If
    End Sub

    Private Sub FormHypGen_Shown(ByVal sender As Object, ByVal e As System.EventArgs) Handles Me.Shown
        Loaded = True
    End Sub

    Private Sub GridUNIFAC_CellValidating(ByVal sender As Object, ByVal e As System.Windows.Forms.DataGridViewCellValidatingEventArgs) Handles GridUNIFAC.CellValidating
        If Me.Loaded Then
            If e.ColumnIndex = 0 Then
                If Integer.TryParse(e.FormattedValue, New Integer) = False Then
                    GridUNIFAC.Rows(e.RowIndex).ErrorText = _
                        DWSIM.App.GetLocalString("Ovalorinseridoinvlid")
                    e.Cancel = True
                ElseIf CInt(e.FormattedValue) < 0 Then
                    GridUNIFAC.Rows(e.RowIndex).ErrorText = _
                        DWSIM.App.GetLocalString("Ovalorinseridoinvlid")
                    e.Cancel = True
                End If
            End If
        End If
    End Sub

    Private Sub GridUNIFAC_CellValueChanged(ByVal sender As Object, ByVal e As System.Windows.Forms.DataGridViewCellEventArgs) Handles GridUNIFAC.CellValueChanged

        If Loaded Then
            Dim OK As Boolean = True
            GridUNIFAC.Rows(e.RowIndex).ErrorText = String.Empty

            Dim row1 As DataGridViewRow
            For Each row1 In Me.GridUNIFAC.Rows
                If Integer.TryParse(row1.Cells(0).FormattedValue, New Integer) = False Then
                    OK = False
                ElseIf CInt(row1.Cells(0).FormattedValue) > 0 Then
                    Me.ValUNIFACOK = True
                End If
            Next
            If Me.RadioButton12.Checked Then
                If Not Double.TryParse(TextBox3.Text, New Double) Then OK = False
            End If
            If OK Then
                With Me.GridUNIFAC

                    'get group amounts
                    Dim vn As New ArrayList
                    For Each r As DataGridViewRow In Me.GridUNIFAC.Rows
                        If Not r.Cells(0).Value Is Nothing Then
                            vn.Add(Integer.Parse(r.Cells(0).Value))
                        Else
                            vn.Add(0)
                        End If
                    Next
                    Dim vnd As Int32() = vn.ToArray(Type.GetType("System.Int32"))

                    If Me.RadioButton11.Checked Then
                        Tb = jb.CalcTb(vnd)
                    Else
                        Tb = TextBox3.Text
                    End If
                    If Me.GridProps.Rows(0).Cells(2).Value = False Then
                        MM = jb.CalcMW(vnd)
                        Me.GridProps.Rows(0).Cells(0).Value = cv.ConverterDoSI(su.spmp_molecularWeight, Format(MM, nf))
                    Else
                        MM = cv.ConverterParaSI(su.spmp_molecularWeight, Me.GridProps.Rows(0).Cells(0).Value)
                    End If
                    If Me.GridProps.Rows(1).Cells(2).Value = False Then
                        Tc = jb.CalcTc(Tb, vnd)
                        Me.GridProps.Rows(1).Cells(0).Value = Format(cv.ConverterDoSI(su.spmp_temperature, Tc), nf)
                    Else
                        Tc = cv.ConverterParaSI(su.spmp_temperature, Me.GridProps.Rows(1).Cells(0).Value)
                    End If
                    If Me.GridProps.Rows(2).Cells(2).Value = False Then
                        Pc = jb.CalcPc(vnd)
                        Me.GridProps.Rows(2).Cells(0).Value = cv.ConverterDoSI(su.spmp_pressure, Format(cv.ConverterDoSI(su.spmp_pressure, Pc), nf))
                    Else
                        Pc = cv.ConverterParaSI(su.spmp_pressure, Me.GridProps.Rows(2).Cells(0).Value)
                    End If
                    If Me.GridProps.Rows(3).Cells(2).Value = False Then
                        Vc = jb.CalcVc(vnd)
                        Me.GridProps.Rows(3).Cells(0).Value = cv.ConverterDoSI(su.spmp_pressure, Format(Vc, nf))
                    Else
                        Vc = cv.ConverterParaSI(su.spmp_pressure, Me.GridProps.Rows(3).Cells(0).Value)
                    End If
                    If Me.GridProps.Rows(4).Cells(2).Value = False Then
                        Zc = Pc * Vc / Tc / 8.314 / 1000
                        Me.GridProps.Rows(4).Cells(0).Value = Format(Zc, nf)
                    Else
                        Zc = Me.GridProps.Rows(4).Cells(0).Value
                    End If
                    If Me.GridProps.Rows(5).Cells(2).Value = False Then
                        w = (-Math.Log(Pc / 100000) - 5.92714 + 6.09648 / (Tb / Tc) + 1.28862 * Math.Log(Tb / Tc) - 0.169347 * (Tb / Tc) ^ 6) / (15.2518 - 15.6875 / (Tb / Tc) - 13.4721 * Math.Log(Tb / Tc) + 0.43577 * (Tb / Tc) ^ 6)
                        Me.GridProps.Rows(5).Cells(0).Value = Format(w, nf)
                    Else
                        w = Me.GridProps.Rows(5).Cells(0).Value
                    End If
                    If Me.GridProps.Rows(6).Cells(2).Value = False Then
                        Hf = jb.CalcDHf(vnd) / MM
                        Me.GridProps.Rows(6).Cells(0).Value = Format(cv.ConverterDoSI(su.spmp_enthalpy, Hf), nf)
                    Else
                        Hf = cv.ConverterParaSI(su.spmp_enthalpy, Me.GridProps.Rows(6).Cells(0).Value)
                    End If
                    Me.GridProps.Rows(7).Cells(0).Value = Format(cv.ConverterDoSI(su.spmp_temperature, Tb), nf)
                    If Me.GridProps.Rows(8).Cells(2).Value = False Then
                        Hvb = methods.DHvb_Vetere(Tc, Pc, Tb) / MM
                        Me.GridProps.Rows(8).Cells(0).Value = Format(cv.ConverterDoSI(su.spmp_enthalpy, Hvb), nf)
                    Else
                        Hvb = cv.ConverterParaSI(su.spmp_enthalpy, Me.GridProps.Rows(8).Cells(0).Value)
                    End If
                    If Me.GridProps.Rows(9).Cells(2).Value = False Then
                        Gf = jb.CalcDGf(vnd) / MM
                        Me.GridProps.Rows(9).Cells(0).Value = Format(cv.ConverterDoSI(su.spmp_enthalpy, Gf), nf)
                    Else
                        Gf = cv.ConverterParaSI(su.spmp_enthalpy, Me.GridProps.Rows(9).Cells(0).Value)
                    End If
                    If Me.GridProps.Rows(10).Cells(2).Value = False Then
                        CSw = w
                        Me.GridProps.Rows(10).Cells(0).Value = Format(w, nf)
                    Else
                        CSsp = Me.GridProps.Rows(10).Cells(0).Value
                    End If
                    If Me.GridProps.Rows(11).Cells(2).Value = False Then
                        CSsp = ((Hvb * MM - 8.314 * Tb) * 238.846 * methods2.liq_dens_rackett(Tb, Tc, Pc, w, MM) / MM / 1000000.0) ^ 0.5
                        Me.GridProps.Rows(11).Cells(0).Value = Format(CSsp, nf)
                    Else
                        CSsp = Me.GridProps.Rows(11).Cells(0).Value
                    End If
                    If Me.GridProps.Rows(12).Cells(2).Value = False Then
                        CSlv = 1 / methods2.liq_dens_rackett(Tb, Tc, Pc, w, MM) * MM / 1000 * 1000000.0
                        Me.GridProps.Rows(12).Cells(0).Value = Format(CSlv, nf)
                    Else
                        CSlv = Me.GridProps.Rows(12).Cells(0).Value
                    End If
                End With
            End If
        End If
    End Sub

    Public Function RegressData(ByVal Tc As Double, ByVal Pc As Double, ByVal w As Double, ByVal MM As Double, ByVal Hvb As Double, ByVal Tb As Double, ByVal tipo As Integer, ByVal calcular As Boolean)

        Dim obj As Object
        Dim lmfit As New LMFit

        m_props = New PROPS()

        Dim c_pv(4), c_cp(4), c_vi(4) As Double
        Dim r_cp, r_vi, r_pv, n_cp, n_pv, n_vi, c_hv(4), r_hv, n_hv As Double

        c_pv(0) = 51.73
        c_pv(1) = -2749.6
        c_pv(2) = -5.245
        c_pv(3) = 0.00000713
        c_pv(4) = 2
        c_cp(0) = 33.7
        c_cp(1) = 0.249
        c_cp(2) = 0.000253
        c_cp(3) = -0.000000384
        c_cp(4) = 0.000000000129
        c_vi(0) = -17.255
        c_vi(1) = 1576
        c_vi(2) = 0.86191
        c_vi(3) = 0
        c_vi(4) = 0
        c_hv(0) = 52053000
        c_hv(1) = 0.3199
        c_hv(2) = -0.212
        c_hv(3) = 0.25795
        c_hv(4) = 0

        Select Case tipo
            Case 0
                If calcular Then
                    Me.vxPvap.Clear()
                    Me.vyPvap.Clear()
                    Dim T As Double = 0.6 * Tc
                    Do
                        Me.vxPvap.Add(T)
                        Me.vyPvap.Add(Me.m_props.Pvp_leekesler(T, Tc, Pc, w))
                        T += (0.4 * Tc) / 20
                    Loop Until T >= Tc
                End If
                'regressão dos dados
                'Me.m_lmpvap.LMNoLinearFit(vxPvap.ToArray(GetType(Double)), vyPvap.ToArray(GetType(Double)), c_pv, r_pv, n_pv)
                obj = lmfit.GetCoeffs(vxPvap.ToArray(GetType(Double)), vyPvap.ToArray(GetType(Double)), c_pv.Clone, DWSIM.Utilities.PetroleumCharacterization.LMFit.FitType.Pvap, 1.0E-50, 1.0E-50, 1.0E-50, 1000)
                c_pv = obj(0)
                r_pv = obj(2)
                n_pv = obj(3)
            Case 1
                If calcular Then

                    'get group amounts
                    Dim vn As New ArrayList
                    For Each r As DataGridViewRow In Me.GridUNIFAC.Rows
                        If Not r.Cells(0).Value Is Nothing Then
                            vn.Add(Integer.Parse(r.Cells(0).Value))
                        Else
                            vn.Add(0)
                        End If
                    Next
                    Dim vnd As Int32() = vn.ToArray(Type.GetType("System.Int32"))

                    c_cp(0) = Me.jb.CalcCpA(vnd)
                    c_cp(1) = Me.jb.CalcCpB(vnd)
                    c_cp(2) = Me.jb.CalcCpC(vnd)
                    c_cp(3) = Me.jb.CalcCpD(vnd)

                Else
                    'regressão dos dados
                    'Me.m_lmcp.LMNoLinearFit(vxCp.ToArray(GetType(Double)), vyCp.ToArray(GetType(Double)), c_cp, r_cp, n_cp)
                    obj = lmfit.GetCoeffs(vxCp.ToArray(GetType(Double)), vyCp.ToArray(GetType(Double)), c_cp, DWSIM.Utilities.PetroleumCharacterization.LMFit.FitType.Cp, 1.0E-50, 1.0E-50, 1.0E-50, 1000)
                    c_cp = obj(0)
                    r_cp = obj(2)
                    n_cp = obj(3)
                End If
            Case 2
                If calcular Then
                    Me.vxVisc.Clear()
                    Me.vyVisc.Clear()
                    Dim T As Double = 0.2 * Tc
                    Do
                        Me.vxVisc.Add(T)
                        Me.vyVisc.Add(Me.m_props.viscl_letsti(T, Tc, Pc, w, MM))
                        T += (0.8 * Tc) / 20
                    Loop Until T >= Tc
                End If
                'regressão dos dados
                'Me.m_lmvisc.LMNoLinearFit(vxVisc.ToArray(GetType(Double)), vyVisc.ToArray(GetType(Double)), c_vi, r_vi, n_vi)
                obj = lmfit.GetCoeffs(vxVisc.ToArray(GetType(Double)), vyVisc.ToArray(GetType(Double)), c_vi, DWSIM.Utilities.PetroleumCharacterization.LMFit.FitType.LiqVisc, 1.0E-50, 1.0E-50, 1.0E-50, 1000)
                c_vi = obj(0)
                r_vi = obj(2)
                n_vi = obj(3)
            Case 3
                If calcular Then
                    Me.vxHv.Clear()
                    Me.vyHv.Clear()
                    Dim T As Double = 0.4 * Tc
                    Do
                        Me.vxHv.Add(T / Tc)
                        Me.vyHv.Add(Hvb * ((1 - T / Tc) / (1 - Tb / Tc)) ^ 0.375)
                        T += (0.6 * Tc) / 20
                    Loop Until T >= Tc
                End If
                'regressão dos dados
                'Me.m_lmhvap.LMNoLinearFit(vxHv.ToArray(GetType(Double)), vyHv.ToArray(GetType(Double)), c_hv, r_hv, n_hv)
                obj = lmfit.GetCoeffs(vxHv.ToArray(GetType(Double)), vyHv.ToArray(GetType(Double)), c_hv, DWSIM.Utilities.PetroleumCharacterization.LMFit.FitType.HVap, 1.0E-50, 1.0E-50, 1.0E-50, 1000)
                c_hv = obj(0)
                r_hv = obj(2)
                n_hv = obj(3)
        End Select

        Select Case tipo
            Case 0
                Return New Object() {c_pv, r_pv, n_pv}
            Case 1
                Return New Object() {c_cp, r_cp, n_cp}
            Case 2
                Return New Object() {c_vi, r_vi, n_vi}
            Case 3
                Return New Object() {c_hv, r_hv, n_hv}
            Case Else
                Return Nothing
        End Select

    End Function

    Private Sub KryptonButton3_Click(ByVal sender As System.Object, ByVal e As System.EventArgs) Handles KryptonButton3.Click
        Dim Parameters(4) As Double
        Dim obj As Object = Me.RegressData(Tc, Pc, w, MM, Hvb, Tb, 1, False)
        Parameters = obj(0)
        Dim px, py As New ArrayList
        Dim X As Double = 300
        Do
            px.Add(X)
            py.Add(Parameters(0) + Parameters(1) * X + Parameters(2) * X ^ 2 + Parameters(3) * X ^ 3 + Parameters(4) * X ^ 4)
            X += 50
        Loop Until X >= 1500
        Try
            With Me.GraphCp.GraphPane
                If .CurveList.Count > 1 Then .CurveList.RemoveAt(1)
                .AddCurve(DWSIM.App.GetLocalString("Regresso"), px.ToArray(GetType(Double)), py.ToArray(GetType(Double)), Color.Red, ZedGraph.SymbolType.None)
                .AxisChange(Me.CreateGraphics)
            End With
            Me.GraphCp.Invalidate()
        Catch ex As Exception

        End Try
    End Sub

    Private Sub KryptonButton4_Click(ByVal sender As System.Object, ByVal e As System.EventArgs) Handles KryptonButton4.Click
        Dim Parameters(4) As Double
        Dim obj As Object = Me.RegressData(Tc, Pc, w, MM, Hvb, Tb, 0, False)
        Parameters = obj(0)
        Dim px, py As New ArrayList
        Dim X As Double = 0.2 * Tc
        Do
            px.Add(cv.ConverterDoSI(ComboBox7.SelectedItem, X))
            py.Add(cv.ConverterDoSI(ComboBox6.SelectedItem, Math.Exp(Parameters(0) + Parameters(1) / X + Parameters(2) * Math.Log(X) + Parameters(3) * X ^ Parameters(4))))
            X += (0.8 * Tc) / 20
        Loop Until X >= Tc
        Try
            With Me.GraphPvap.GraphPane
                If .CurveList.Count > 1 Then .CurveList.RemoveAt(1)
                .AddCurve(DWSIM.App.GetLocalString("Regresso"), px.ToArray(GetType(Double)), py.ToArray(GetType(Double)), Color.Red, ZedGraph.SymbolType.None)
                .AxisChange(Me.CreateGraphics)
            End With
            Me.GraphPvap.Invalidate()
        Catch ex As Exception

        End Try
    End Sub

    Private Sub KryptonButton5_Click(ByVal sender As System.Object, ByVal e As System.EventArgs) Handles KryptonButton5.Click
        Dim Parameters(4) As Double
        Dim obj As Object = Me.RegressData(Tc, Pc, w, MM, Hvb, Tb, 2, False)
        Parameters = obj(0)
        Dim px, py As New ArrayList
        Dim X As Double = 0.3 * Tc
        Do
            px.Add(cv.ConverterDoSI(ComboBox10.SelectedItem, X))
            py.Add(cv.ConverterDoSI(ComboBox9.SelectedItem, Math.Exp(Parameters(0) + Parameters(1) / X + Parameters(2) * Math.Log(X) + Parameters(3) * X ^ Parameters(4))))
            X += (0.7 * Tc) / 20
        Loop Until X >= Tc
        Try
            With Me.GraphVisc.GraphPane
                If .CurveList.Count > 1 Then .CurveList.RemoveAt(1)
                .AddCurve(DWSIM.App.GetLocalString("Regresso"), px.ToArray(GetType(Double)), py.ToArray(GetType(Double)), Color.Red, ZedGraph.SymbolType.None)
                .AxisChange(Me.CreateGraphics)
            End With
            Me.GraphVisc.Invalidate()
        Catch ex As Exception

        End Try
    End Sub

    Private Sub KryptonDataGridView3_CellValidating(ByVal sender As Object, ByVal e As System.Windows.Forms.DataGridViewCellValidatingEventArgs) Handles KryptonDataGridView3.CellValidating
        If Me.Loaded Then
            If Double.TryParse(e.FormattedValue, New Double) = False Then
                Me.KryptonDataGridView3.Rows(e.RowIndex).ErrorText = _
                    DWSIM.App.GetLocalString("Ovalorinseridoinvlid")
                e.Cancel = True
            ElseIf CDbl(e.FormattedValue) < 0 Then
                Me.KryptonDataGridView3.Rows(e.RowIndex).ErrorText = _
                    DWSIM.App.GetLocalString("Ovalorinseridoinvlid")
                e.Cancel = True
            End If
        End If
    End Sub

    Private Sub KryptonDataGridView1_CellValidating(ByVal sender As Object, ByVal e As System.Windows.Forms.DataGridViewCellValidatingEventArgs) Handles KryptonDataGridView1.CellValidating
        If Me.Loaded Then
            If Double.TryParse(e.FormattedValue, New Double) = False Then
                Me.KryptonDataGridView1.Rows(e.RowIndex).ErrorText = _
                    DWSIM.App.GetLocalString("Ovalorinseridoinvlid")
                e.Cancel = True
            ElseIf CDbl(e.FormattedValue) < 0 Then
                Me.KryptonDataGridView1.Rows(e.RowIndex).ErrorText = _
                    DWSIM.App.GetLocalString("Ovalorinseridoinvlid")
                e.Cancel = True
            End If
        End If
    End Sub

    Private Sub KryptonDataGridView4_CellValidating(ByVal sender As Object, ByVal e As System.Windows.Forms.DataGridViewCellValidatingEventArgs) Handles KryptonDataGridView4.CellValidating
        If Me.Loaded Then
            If Double.TryParse(e.FormattedValue, New Double) = False Then
                Me.KryptonDataGridView4.Rows(e.RowIndex).ErrorText = _
                    DWSIM.App.GetLocalString("Ovalorinseridoinvlid")
                e.Cancel = True
            ElseIf CDbl(e.FormattedValue) < 0 Then
                Me.KryptonDataGridView4.Rows(e.RowIndex).ErrorText = _
                    DWSIM.App.GetLocalString("Ovalorinseridoinvlid")
                e.Cancel = True
            End If
        End If
    End Sub

    Private Sub KryptonButton2_Click(ByVal sender As System.Object, ByVal e As System.EventArgs) Handles KryptonButton2.Click

        Me.frmCheck = New FormCheckHypData
        frmCheck.AddOnly = False
        frmCheck.ShowDialog(Me)
        If Not Me.HypError Then Me.Close()

    End Sub

    Private Sub KryptonButton1_Click(ByVal sender As System.Object, ByVal e As System.EventArgs) Handles KryptonButton1.Click

        Me.frmCheck = New FormCheckHypData
        frmCheck.AddOnly = True
        frmCheck.ShowDialog(Me)
        If Not Me.HypError Then Me.Close()

    End Sub

End Class