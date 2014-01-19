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


Imports DWSIM.DWSIM.ClassesBasicasTermodinamica
Imports System.IO
Imports System.Text

Public Class FormConfigUNIQUAC

    Inherits FormConfigBase

    Public Loaded = False
    Public param As System.Collections.Specialized.StringDictionary

    Private Sub ConfigFormUNIQUAC_Load(ByVal sender As System.Object, ByVal e As System.EventArgs) Handles MyBase.Load

        Loaded = False

        Me.Text = DWSIM.App.GetLocalString("ConfigurarPacotedePropriedades") & _pp.Tag & ")"

        With Me.KryptonDataGridView1.Rows
            .Clear()
            For Each kvp As KeyValuePair(Of String, Double) In _pp.Parameters
                .Add(New Object() {kvp.Key, DWSIM.App.GetLocalString(kvp.Key), kvp.Value})
            Next
        End With

        Me.KryptonDataGridView2.DataSource = Nothing

        If _pp.ComponentName.ToString.Contains("Raoult") Or _
           _pp.ComponentName.ToString.Contains(DWSIM.App.GetLocalString("Vapor")) Then
            Me.FaTabStripItem2.Visible = False
            Exit Sub
        Else
            Me.FaTabStripItem2.Visible = True
        End If

        Me.KryptonDataGridView2.Rows.Clear()

        Dim ppu As DWSIM.SimulationObjects.PropertyPackages.UNIQUACPropertyPackage = _pp

        Dim nf As String = "0.####"

        For Each cp As ConstantProperties In _comps.Values
gt0:        If ppu.m_pr.InteractionParameters.ContainsKey(cp.Name) Then
                For Each cp2 As ConstantProperties In _comps.Values
                    If cp.Name <> cp2.Name Then
                        If Not ppu.m_pr.InteractionParameters(cp.Name).ContainsKey(cp2.Name) Then
                            'check if collection has id2 as primary id
                            If ppu.m_pr.InteractionParameters.ContainsKey(cp2.Name) Then
                                If Not ppu.m_pr.InteractionParameters(cp2.Name).ContainsKey(cp.Name) Then
                                    ppu.m_pr.InteractionParameters(cp.Name).Add(cp2.Name, New DWSIM.SimulationObjects.PropertyPackages.Auxiliary.PR_IPData)
                                    Dim a12 As Double = ppu.m_pr.InteractionParameters(cp.Name)(cp2.Name).kij
                                    KryptonDataGridView2.Rows.Add(New Object() {DWSIM.App.GetComponentName(cp.Name), DWSIM.App.GetComponentName(cp2.Name), Format(a12, nf)})
                                    With KryptonDataGridView2.Rows(KryptonDataGridView2.Rows.Count - 1)
                                        .Cells(0).Tag = cp.Name
                                        .Cells(1).Tag = cp2.Name
                                    End With
                                End If
                            End If
                        Else
                            Dim a12 As Double = ppu.m_pr.InteractionParameters(cp.Name)(cp2.Name).kij
                            KryptonDataGridView2.Rows.Add(New Object() {DWSIM.App.GetComponentName(cp.Name), DWSIM.App.GetComponentName(cp2.Name), Format(a12, nf)})
                            With KryptonDataGridView2.Rows(KryptonDataGridView2.Rows.Count - 1)
                                .Cells(0).Tag = cp.Name
                                .Cells(1).Tag = cp2.Name
                            End With
                        End If
                    End If
                Next
            Else
                ppu.m_pr.InteractionParameters.Add(cp.Name, New Dictionary(Of String, DWSIM.SimulationObjects.PropertyPackages.Auxiliary.PR_IPData))
                GoTo gt0
            End If
        Next

        dgvu1.Rows.Clear()

        For Each cp As ConstantProperties In _comps.Values
gt1:        If ppu.m_uni.InteractionParameters.ContainsKey(cp.Name) Then
                For Each cp2 As ConstantProperties In _comps.Values
                    If cp.Name <> cp2.Name Then
                        If Not ppu.m_uni.InteractionParameters(cp.Name).ContainsKey(cp2.Name) Then
                            'check if collection has id2 as primary id
                            If ppu.m_uni.InteractionParameters.ContainsKey(cp2.Name) Then
                                If Not ppu.m_uni.InteractionParameters(cp2.Name).ContainsKey(cp.Name) Then
                                    ppu.m_uni.InteractionParameters(cp.Name).Add(cp2.Name, New DWSIM.SimulationObjects.PropertyPackages.Auxiliary.UNIQUAC_IPData)
                                    Dim a12 As Double = ppu.m_uni.InteractionParameters(cp.Name)(cp2.Name).A12
                                    Dim a21 As Double = ppu.m_uni.InteractionParameters(cp.Name)(cp2.Name).A21
                                    Dim b12 As Double = ppu.m_uni.InteractionParameters(cp.Name)(cp2.Name).B12
                                    Dim b21 As Double = ppu.m_uni.InteractionParameters(cp.Name)(cp2.Name).B21
                                    Dim c12 As Double = ppu.m_uni.InteractionParameters(cp.Name)(cp2.Name).C12
                                    Dim c21 As Double = ppu.m_uni.InteractionParameters(cp.Name)(cp2.Name).C21
                                    dgvu1.Rows.Add(New Object() {DWSIM.App.GetComponentName(cp.Name), DWSIM.App.GetComponentName(cp2.Name), "", Format(a12, nf), Format(a21, nf), Format(b12, nf), Format(b21, nf), Format(c12, nf), Format(c21, nf)})
                                    With dgvu1.Rows(dgvu1.Rows.Count - 1)
                                        .Cells(0).Tag = cp.Name
                                        .Cells(1).Tag = cp2.Name
                                    End With
                                End If
                            End If
                        Else
                            Dim a12 As Double = ppu.m_uni.InteractionParameters(cp.Name)(cp2.Name).A12
                            Dim a21 As Double = ppu.m_uni.InteractionParameters(cp.Name)(cp2.Name).A21
                            Dim b12 As Double = ppu.m_uni.InteractionParameters(cp.Name)(cp2.Name).B12
                            Dim b21 As Double = ppu.m_uni.InteractionParameters(cp.Name)(cp2.Name).B21
                            Dim c12 As Double = ppu.m_uni.InteractionParameters(cp.Name)(cp2.Name).C12
                            Dim c21 As Double = ppu.m_uni.InteractionParameters(cp.Name)(cp2.Name).C21
                            dgvu1.Rows.Add(New Object() {DWSIM.App.GetComponentName(cp.Name), DWSIM.App.GetComponentName(cp2.Name), "", Format(a12, nf), Format(a21, nf), Format(b12, nf), Format(b21, nf), Format(c12, nf), Format(c21, nf)})
                            With dgvu1.Rows(dgvu1.Rows.Count - 1)
                                .Cells(0).Tag = cp.Name
                                .Cells(1).Tag = cp2.Name
                            End With
                        End If
                    End If
                Next
            Else
                ppu.m_uni.InteractionParameters.Add(cp.Name, New Dictionary(Of String, DWSIM.SimulationObjects.PropertyPackages.Auxiliary.UNIQUAC_IPData))
                GoTo gt1
            End If
        Next

        For Each r As DataGridViewRow In dgvu1.Rows
            Dim cb As DataGridViewComboBoxCell = r.Cells(2)
            cb.Items.Clear()
            Dim ipsets As List(Of DWSIM.ClassesBasicasTermodinamica.InteractionParameter) = DWSIM.Databases.UserIPDB.GetStoredIPsets(r.Cells(0).Value, r.Cells(1).Value, "UNIQUAC")
            cb.Items.Add(ipsets.Count)
            For Each ip As InteractionParameter In ipsets
                Dim strb As New StringBuilder
                For Each kvp As KeyValuePair(Of String, Object) In ip.Parameters
                    strb.Append(kvp.Key & ": " & Double.Parse(kvp.Value).ToString("N2") & ", ")
                Next
                strb.Append("{" & ip.DataType & " / " & ip.Description & "}")
                cb.Items.Add(strb.ToString)
                cb.Tag = ipsets
            Next
            r.Cells(2).Value = cb.Items(0)
        Next

        Loaded = True

    End Sub

    Private Sub KryptonDataGridView1_CellEndEdit(ByVal sender As Object, ByVal e As System.Windows.Forms.DataGridViewCellEventArgs) Handles KryptonDataGridView1.CellEndEdit

        _pp.Parameters(Me.KryptonDataGridView1.Rows(e.RowIndex).Cells(0).Value) = Me.KryptonDataGridView1.Rows(e.RowIndex).Cells(2).Value

    End Sub

    Private Sub KryptonDataGridView1_CellValidating(ByVal sender As Object, ByVal e As System.Windows.Forms.DataGridViewCellValidatingEventArgs) Handles KryptonDataGridView1.CellValidating

        'If Me.Loaded Then
        '    If e.ColumnIndex = 1 Then
        '        If Double.TryParse(e.FormattedValue, New Integer) = False Then
        '            Me.KryptonDataGridView1.Rows(e.RowIndex).ErrorText = _
        '                DWSIM.App.GetLocalString("Ovalorinseridoinvlid")
        '            e.Cancel = True
        '        ElseIf CDbl(e.FormattedValue) < 0 Then
        '            Me.KryptonDataGridView1.Rows(e.RowIndex).ErrorText = _
        '                DWSIM.App.GetLocalString("Ovalorinseridoinvlid")
        '            e.Cancel = True
        '        End If
        '    End If
        'End If

    End Sub

    Private Sub FormConfigPR_Shown(ByVal sender As Object, ByVal e As System.EventArgs) Handles Me.Shown
        Loaded = True
    End Sub

    Public Sub RefreshIPTable()

    End Sub

    Private Sub KryptonButton4_Click(ByVal sender As System.Object, ByVal e As System.EventArgs)
        If Not Me.KryptonDataGridView2.SelectedCells(0) Is Nothing Then
            If Me.KryptonDataGridView2.SelectedCells(0).RowIndex <> Me.KryptonDataGridView2.SelectedCells(0).ColumnIndex Then
                Dim Vc1 As Double = _comps(Me.KryptonDataGridView2.Rows(Me.KryptonDataGridView2.SelectedCells(0).RowIndex).Tag).Critical_Volume
                Dim Vc2 As Double = _comps(Me.KryptonDataGridView2.Columns(Me.KryptonDataGridView2.SelectedCells(0).ColumnIndex).Tag).Critical_Volume

                Dim tmp As Double = 1 - 8 * (Vc1 * Vc2) ^ 0.5 / ((Vc1 ^ (1 / 3) + Vc2 ^ (1 / 3)) ^ 3)

                Me.KryptonDataGridView2.SelectedCells(0).Value = tmp

            End If
        End If
    End Sub

    Private Sub dgvu1_CellValueChanged(ByVal sender As System.Object, ByVal e As System.Windows.Forms.DataGridViewCellEventArgs) Handles dgvu1.CellValueChanged
        If Loaded Then
            Dim ppu As DWSIM.SimulationObjects.PropertyPackages.UNIQUACPropertyPackage = _pp
            Dim value As Object = dgvu1.Rows(e.RowIndex).Cells(e.ColumnIndex).Value
            Dim id1 As String = dgvu1.Rows(e.RowIndex).Cells(0).Tag.ToString
            Dim id2 As String = dgvu1.Rows(e.RowIndex).Cells(1).Tag.ToString
            Select Case e.ColumnIndex
                Case 2
                    Dim cb As DataGridViewComboBoxCell = dgvu1.Rows(e.RowIndex).Cells(2)
                    If value <> "" Then
                        Dim i As Integer = -1
                        For Each s As String In cb.Items
                            If value = s And i <> -1 Then
                                Dim ipset As InteractionParameter = cb.Tag(i)
                                With dgvu1.Rows(e.RowIndex)
                                    If ipset.Parameters.ContainsKey("A12") Then .Cells(3).Value = Double.Parse(ipset.Parameters("A12"), Globalization.CultureInfo.InvariantCulture).ToString("N4")
                                    If ipset.Parameters.ContainsKey("A21") Then .Cells(4).Value = Double.Parse(ipset.Parameters("A21"), Globalization.CultureInfo.InvariantCulture).ToString("N4")
                                    If ipset.Parameters.ContainsKey("B12") Then .Cells(5).Value = Double.Parse(ipset.Parameters("B12"), Globalization.CultureInfo.InvariantCulture).ToString("N4")
                                    If ipset.Parameters.ContainsKey("B21") Then .Cells(6).Value = Double.Parse(ipset.Parameters("B21"), Globalization.CultureInfo.InvariantCulture).ToString("N4")
                                    If ipset.Parameters.ContainsKey("C12") Then .Cells(7).Value = Double.Parse(ipset.Parameters("C12"), Globalization.CultureInfo.InvariantCulture).ToString("N4")
                                    If ipset.Parameters.ContainsKey("C21") Then .Cells(8).Value = Double.Parse(ipset.Parameters("C21"), Globalization.CultureInfo.InvariantCulture).ToString("N4")
                                End With
                            End If
                            i += 1
                        Next
                    End If
                Case 3
                    ppu.m_uni.InteractionParameters(id1)(id2).A12 = value
                Case 4
                    ppu.m_uni.InteractionParameters(id1)(id2).A21 = value
                Case 5
                    ppu.m_uni.InteractionParameters(id1)(id2).B12 = value
                Case 6
                    ppu.m_uni.InteractionParameters(id1)(id2).B21 = value
                Case 7
                    ppu.m_uni.InteractionParameters(id1)(id2).C12 = value
                Case 8
                    ppu.m_uni.InteractionParameters(id1)(id2).C21 = value
            End Select
        End If
    End Sub

    Private Sub KryptonDataGridView2_CellValueChanged(ByVal sender As Object, ByVal e As System.Windows.Forms.DataGridViewCellEventArgs) Handles KryptonDataGridView2.CellValueChanged
        If Loaded Then
            Dim ppu As DWSIM.SimulationObjects.PropertyPackages.UNIQUACPropertyPackage = _pp
            Dim value As Object = KryptonDataGridView2.Rows(e.RowIndex).Cells(e.ColumnIndex).Value
            Dim id1 As String = KryptonDataGridView2.Rows(e.RowIndex).Cells(0).Tag.ToString
            Dim id2 As String = KryptonDataGridView2.Rows(e.RowIndex).Cells(1).Tag.ToString
            Select Case e.ColumnIndex
                Case 2
                    ppu.m_pr.InteractionParameters(id1)(id2).kij = value
            End Select
        End If
    End Sub

    Private Sub Button3_Click(ByVal sender As System.Object, ByVal e As System.EventArgs) Handles Button3.Click
        Dim row As Integer = Me.KryptonDataGridView2.SelectedCells(0).RowIndex

        Dim id1 As String = Me.KryptonDataGridView2.Rows(row).Cells(0).Tag.ToString
        Dim id2 As String = Me.KryptonDataGridView2.Rows(row).Cells(1).Tag.ToString

        Dim comp1, comp2 As ConstantProperties
        comp1 = _comps(id1)
        comp2 = _comps(id2)

        Dim Vc1 As Double = comp1.Critical_Volume
        Dim Vc2 As Double = comp2.Critical_Volume

        Dim tmp As Double = 1 - 8 * (Vc1 * Vc2) ^ 0.5 / ((Vc1 ^ (1 / 3) + Vc2 ^ (1 / 3)) ^ 3)

        Me.KryptonDataGridView2.Rows(row).Cells(2).Value = tmp

    End Sub

    Private Sub Button1_Click(ByVal sender As System.Object, ByVal e As System.EventArgs) Handles Button1.Click, Button2.Click, Button5.Click

        Dim row As Integer = dgvu1.SelectedCells(0).RowIndex
        Dim count As Integer = 0
        Dim delta1 As Double = 10
        Dim delta2 As Double = 10

        Dim ms As New DWSIM.SimulationObjects.Streams.MaterialStream("", "")

        Dim ppn As New DWSIM.SimulationObjects.PropertyPackages.UNIQUACPropertyPackage
        Dim uniquac As New DWSIM.SimulationObjects.PropertyPackages.Auxiliary.UNIQUAC

        Dim ppu, unifac As Object

        If sender.Name = "Button1" Then
            ppu = New DWSIM.SimulationObjects.PropertyPackages.UNIFACPropertyPackage
            unifac = New DWSIM.SimulationObjects.PropertyPackages.Auxiliary.Unifac
        ElseIf sender.Name = "Button5" Then
            ppu = New DWSIM.SimulationObjects.PropertyPackages.UNIFACLLPropertyPackage
            unifac = New DWSIM.SimulationObjects.PropertyPackages.Auxiliary.UnifacLL
        Else
            ppu = New DWSIM.SimulationObjects.PropertyPackages.MODFACPropertyPackage
            unifac = New DWSIM.SimulationObjects.PropertyPackages.Auxiliary.Modfac
        End If

        Dim id1 As String = dgvu1.Rows(row).Cells(0).Tag.ToString
        Dim id2 As String = dgvu1.Rows(row).Cells(1).Tag.ToString

        Dim comp1, comp2 As ConstantProperties
        comp1 = _comps(id1)
        comp2 = _comps(id2)

        With ms
            For Each phase As DWSIM.ClassesBasicasTermodinamica.Fase In ms.Fases.Values
                With phase
                    .Componentes.Add(comp1.Name, New DWSIM.ClassesBasicasTermodinamica.Substancia(comp1.Name, ""))
                    .Componentes(comp1.Name).ConstantProperties = comp1
                    .Componentes.Add(comp2.Name, New DWSIM.ClassesBasicasTermodinamica.Substancia(comp2.Name, ""))
                    .Componentes(comp2.Name).ConstantProperties = comp2
                End With
            Next
        End With

        ppn.CurrentMaterialStream = ms
        ppu.CurrentMaterialStream = ms

        Dim T1 = 298.15

        Dim actu(1), actn(1), actnd(1), fx(1), fxd(1), dfdx(1, 1), x(1), x0(1), dx(1) As Double

        actu(0) = unifac.GAMMA(T1, New Object() {0.25, 0.75}, ppu.RET_VQ(), ppu.RET_VR, ppu.RET_VEKI, 0)
        actu(1) = unifac.GAMMA(T1, New Object() {0.75, 0.25}, ppu.RET_VQ(), ppu.RET_VR, ppu.RET_VEKI, 0)

        x(0) = dgvu1.Rows(row).Cells(3).Value
        x(1) = dgvu1.Rows(row).Cells(4).Value

        If x(0) = 0 Then x(0) = -100
        If x(1) = 0 Then x(1) = 100

        Do

            uniquac.InteractionParameters.Clear()
            uniquac.InteractionParameters.Add(ppn.RET_VIDS()(0), New Dictionary(Of String, DWSIM.SimulationObjects.PropertyPackages.Auxiliary.UNIQUAC_IPData))
            uniquac.InteractionParameters(ppn.RET_VIDS()(0)).Add(ppn.RET_VIDS()(1), New DWSIM.SimulationObjects.PropertyPackages.Auxiliary.UNIQUAC_IPData())
            uniquac.InteractionParameters(ppn.RET_VIDS()(0))(ppn.RET_VIDS()(1)).A12 = x(0)
            uniquac.InteractionParameters(ppn.RET_VIDS()(0))(ppn.RET_VIDS()(1)).A21 = x(1)

            actnd(0) = uniquac.GAMMA(T1, New Object() {0.25, 0.75}, ppn.RET_VIDS, ppn.RET_VQ, ppn.RET_VR, 0)
            actnd(1) = uniquac.GAMMA(T1, New Object() {0.75, 0.25}, ppn.RET_VIDS, ppn.RET_VQ, ppn.RET_VR, 0)

            fx(0) = Math.Log(actu(0) / actnd(0))
            fx(1) = Math.Log(actu(1) / actnd(1))

            uniquac.InteractionParameters.Clear()
            uniquac.InteractionParameters.Add(ppn.RET_VIDS()(0), New Dictionary(Of String, DWSIM.SimulationObjects.PropertyPackages.Auxiliary.UNIQUAC_IPData))
            uniquac.InteractionParameters(ppn.RET_VIDS()(0)).Add(ppn.RET_VIDS()(1), New DWSIM.SimulationObjects.PropertyPackages.Auxiliary.UNIQUAC_IPData())
            uniquac.InteractionParameters(ppn.RET_VIDS()(0))(ppn.RET_VIDS()(1)).A12 = x(0) + delta1
            uniquac.InteractionParameters(ppn.RET_VIDS()(0))(ppn.RET_VIDS()(1)).A21 = x(1)

            actnd(0) = uniquac.GAMMA(T1, New Object() {0.25, 0.75}, ppn.RET_VIDS, ppn.RET_VQ, ppn.RET_VR, 0)
            actnd(1) = uniquac.GAMMA(T1, New Object() {0.75, 0.25}, ppn.RET_VIDS, ppn.RET_VQ, ppn.RET_VR, 0)

            fxd(0) = Math.Log(actu(0) / actnd(0))
            fxd(1) = Math.Log(actu(1) / actnd(1))

            dfdx(0, 0) = -(fxd(0) - fx(0)) / delta1
            dfdx(1, 0) = -(fxd(1) - fx(1)) / delta1

            uniquac.InteractionParameters.Clear()
            uniquac.InteractionParameters.Add(ppn.RET_VIDS()(0), New Dictionary(Of String, DWSIM.SimulationObjects.PropertyPackages.Auxiliary.UNIQUAC_IPData))
            uniquac.InteractionParameters(ppn.RET_VIDS()(0)).Add(ppn.RET_VIDS()(1), New DWSIM.SimulationObjects.PropertyPackages.Auxiliary.UNIQUAC_IPData())
            uniquac.InteractionParameters(ppn.RET_VIDS()(0))(ppn.RET_VIDS()(1)).A12 = x(0)
            uniquac.InteractionParameters(ppn.RET_VIDS()(0))(ppn.RET_VIDS()(1)).A21 = x(1) + delta2

            actnd(0) = uniquac.GAMMA(T1, New Object() {0.25, 0.75}, ppn.RET_VIDS, ppn.RET_VQ, ppn.RET_VR, 0)
            actnd(1) = uniquac.GAMMA(T1, New Object() {0.75, 0.25}, ppn.RET_VIDS, ppn.RET_VQ, ppn.RET_VR, 0)

            fxd(0) = Math.Log(actu(0) / actnd(0))
            fxd(1) = Math.Log(actu(1) / actnd(1))

            dfdx(0, 1) = -(fxd(0) - fx(0)) / delta2
            dfdx(1, 1) = -(fxd(1) - fx(1)) / delta2

            'solve linear system
            DWSIM.MathEx.SysLin.rsolve.rmatrixsolve(dfdx, fx, UBound(fx) + 1, dx)

            x0(0) = x(0)
            x0(1) = x(1)

            x(0) += dx(0)
            x(1) += dx(1)

            count += 1

        Loop Until Math.Abs(fx(0) + fx(1)) < 0.01 Or count > 500

        If count < 500 Then
            dgvu1.Rows(row).Cells(3).Value = x0(0)
            dgvu1.Rows(row).Cells(4).Value = x0(1)
        Else
            If Not Double.IsNaN(x0(0)) Then dgvu1.Rows(row).Cells(3).Value = x0(0) Else dgvu1.Rows(row).Cells(3).Value = 0.0#
            If Not Double.IsNaN(x0(1)) Then dgvu1.Rows(row).Cells(4).Value = x0(1) Else dgvu1.Rows(row).Cells(4).Value = 0.0#
            MessageBox.Show("Parameter estimation through UNIFAC failed: Reached the maximum number of iterations.", "UNIFAC Error", MessageBoxButtons.OK, MessageBoxIcon.Error)
        End If

        dgvu1.Rows(row).Cells(5).Value = 0.0#
        dgvu1.Rows(row).Cells(6).Value = 0.0#
        dgvu1.Rows(row).Cells(7).Value = 0.0#
        dgvu1.Rows(row).Cells(8).Value = 0.0#

    End Sub

    Private Sub Button4_Click(ByVal sender As System.Object, ByVal e As System.EventArgs) Handles Button4.Click
        Process.Start(My.Application.Info.DirectoryPath & Path.DirectorySeparatorChar & "data" & Path.DirectorySeparatorChar & "uniquacip.dat")
    End Sub

End Class