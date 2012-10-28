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
Imports DWSIM.DWSIM.SimulationObjects.PropertyPackages
Imports System.IO

Public Class FormConfigPP

    Inherits FormConfigBase

    Public Loaded = False

    Private Sub FormConfigPR_FormClosing(ByVal sender As Object, ByVal e As System.Windows.Forms.FormClosingEventArgs) Handles Me.FormClosing
        Me.KryptonDataGridView2.DataSource = Nothing
    End Sub

    Private Sub FormConfigPR_Load(ByVal sender As System.Object, ByVal e As System.EventArgs) Handles MyBase.Load

        Me.Text = DWSIM.App.GetLocalString("ConfigurarPacotedePropriedades") & _pp.Tag & ")"


        With Me.KryptonDataGridView1.Rows
            .Clear()
            For Each kvp As KeyValuePair(Of String, Double) In _pp.Parameters
                .Add(New Object() {kvp.Key, DWSIM.App.GetLocalString(kvp.Key), kvp.Value})
            Next
        End With

        Me.KryptonDataGridView2.DataSource = Nothing

        If _pp.ComponentName.ToString.Contains("Raoult") Or _
           _pp.ComponentName.ToString.Contains(DWSIM.App.GetLocalString("Vapor")) Or _
           _pp.ComponentName.ToString.Contains(DWSIM.App.GetLocalString("Chao-Seader")) Or _
           _pp.ComponentName.ToString.Contains(DWSIM.App.GetLocalString("Grayson-Streed")) Then
            Me.FaTabStripItem2.Visible = False
            Me.FaTabStripItem1.Selected = True
            Exit Sub
        Else
            Me.FaTabStripItem2.Visible = True
            Me.FaTabStripItem1.Selected = True
        End If
        Me.FaTabStrip1.SelectedItem = Me.FaTabStripItem1

        Me.KryptonDataGridView2.Rows.Clear()

        Dim nf As String = "0.####"

        If TypeOf _pp Is SRKPropertyPackage Then

            Dim ppu As SRKPropertyPackage = _pp

            For Each cp As ConstantProperties In _comps.Values
gt1:            If ppu.m_pr.InteractionParameters.ContainsKey(cp.Name) Then
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
                    GoTo gt1
                End If
            Next

        ElseIf TypeOf _pp Is UNIFACPropertyPackage Then

            Dim ppu As UNIFACPropertyPackage = _pp

            For Each cp As ConstantProperties In _comps.Values
gtu:            If ppu.m_pr.InteractionParameters.ContainsKey(cp.Name) Then
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
                    GoTo gtu
                End If
            Next

        ElseIf TypeOf _pp Is UNIFACLLPropertyPackage Then

            Dim ppu As UNIFACLLPropertyPackage = _pp

            For Each cp As ConstantProperties In _comps.Values
gtul:           If ppu.m_pr.InteractionParameters.ContainsKey(cp.Name) Then
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
                    GoTo gtul
                End If
            Next

        ElseIf TypeOf _pp Is MODFACPropertyPackage Then

            Dim ppu As MODFACPropertyPackage = _pp

            For Each cp As ConstantProperties In _comps.Values
gtmu:           If ppu.m_pr.InteractionParameters.ContainsKey(cp.Name) Then
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
                    GoTo gtmu
                End If
            Next
        Else

            Dim ppu As PengRobinsonPropertyPackage = _pp

            For Each cp As ConstantProperties In _comps.Values
gt2:            If ppu.m_pr.InteractionParameters.ContainsKey(cp.Name) Then
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
                    GoTo gt2
                End If
            Next

        End If

    End Sub

    Private Sub KryptonDataGridView1_CellEndEdit(ByVal sender As Object, ByVal e As System.Windows.Forms.DataGridViewCellEventArgs) Handles KryptonDataGridView1.CellEndEdit

        _pp.Parameters(Me.KryptonDataGridView1.Rows(e.RowIndex).Cells(0).Value) = Me.KryptonDataGridView1.Rows(e.RowIndex).Cells(2).Value

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

    Private Sub KryptonDataGridView2_CellValueChanged(ByVal sender As Object, ByVal e As System.Windows.Forms.DataGridViewCellEventArgs) Handles KryptonDataGridView2.CellValueChanged
        If Loaded Then
            If TypeOf _pp Is SRKPropertyPackage Then
                Dim ppu As DWSIM.SimulationObjects.PropertyPackages.SRKPropertyPackage = _pp
                Dim value As Object = KryptonDataGridView2.Rows(e.RowIndex).Cells(e.ColumnIndex).Value
                Dim id1 As String = KryptonDataGridView2.Rows(e.RowIndex).Cells(0).Tag.ToString
                Dim id2 As String = KryptonDataGridView2.Rows(e.RowIndex).Cells(1).Tag.ToString
                Select Case e.ColumnIndex
                    Case 2
                        ppu.m_pr.InteractionParameters(id1)(id2).kij = CDbl(value)
                End Select
            ElseIf TypeOf _pp Is PengRobinsonPropertyPackage Then
                Dim ppu As DWSIM.SimulationObjects.PropertyPackages.PengRobinsonPropertyPackage = _pp
                Dim value As Object = KryptonDataGridView2.Rows(e.RowIndex).Cells(e.ColumnIndex).Value
                Dim id1 As String = KryptonDataGridView2.Rows(e.RowIndex).Cells(0).Tag.ToString
                Dim id2 As String = KryptonDataGridView2.Rows(e.RowIndex).Cells(1).Tag.ToString
                Select Case e.ColumnIndex
                    Case 2
                        ppu.m_pr.InteractionParameters(id1)(id2).kij = CDbl(value)
                End Select
            ElseIf TypeOf _pp Is UNIFACPropertyPackage Then
                Dim ppu As DWSIM.SimulationObjects.PropertyPackages.UNIFACPropertyPackage = _pp
                Dim value As Object = KryptonDataGridView2.Rows(e.RowIndex).Cells(e.ColumnIndex).Value
                Dim id1 As String = KryptonDataGridView2.Rows(e.RowIndex).Cells(0).Tag.ToString
                Dim id2 As String = KryptonDataGridView2.Rows(e.RowIndex).Cells(1).Tag.ToString
                Select Case e.ColumnIndex
                    Case 2
                        ppu.m_pr.InteractionParameters(id1)(id2).kij = CDbl(value)
                End Select
            ElseIf TypeOf _pp Is UNIFACLLPropertyPackage Then
                Dim ppu As DWSIM.SimulationObjects.PropertyPackages.UNIFACLLPropertyPackage = _pp
                Dim value As Object = KryptonDataGridView2.Rows(e.RowIndex).Cells(e.ColumnIndex).Value
                Dim id1 As String = KryptonDataGridView2.Rows(e.RowIndex).Cells(0).Tag.ToString
                Dim id2 As String = KryptonDataGridView2.Rows(e.RowIndex).Cells(1).Tag.ToString
                Select Case e.ColumnIndex
                    Case 2
                        ppu.m_pr.InteractionParameters(id1)(id2).kij = CDbl(value)
                End Select
            ElseIf TypeOf _pp Is MODFACPropertyPackage Then
                Dim ppu As DWSIM.SimulationObjects.PropertyPackages.MODFACPropertyPackage = _pp
                Dim value As Object = KryptonDataGridView2.Rows(e.RowIndex).Cells(e.ColumnIndex).Value
                Dim id1 As String = KryptonDataGridView2.Rows(e.RowIndex).Cells(0).Tag.ToString
                Dim id2 As String = KryptonDataGridView2.Rows(e.RowIndex).Cells(1).Tag.ToString
                Select Case e.ColumnIndex
                    Case 2
                        ppu.m_pr.InteractionParameters(id1)(id2).kij = CDbl(value)
                End Select
            End If
        End If
    End Sub

    Private Sub Button3_Click(ByVal sender As System.Object, ByVal e As System.EventArgs) Handles Button3.Click

        For Each r As DataGridViewRow In Me.KryptonDataGridView2.SelectedRows

            Dim id1 As String = r.Cells(0).Tag.ToString
            Dim id2 As String = r.Cells(1).Tag.ToString

            Dim comp1, comp2 As ConstantProperties
            comp1 = _comps(id1)
            comp2 = _comps(id2)

            Dim Vc1 As Double = comp1.Critical_Volume
            Dim Vc2 As Double = comp2.Critical_Volume

            Dim tmp As Double = 1 - 8 * (Vc1 * Vc2) ^ 0.5 / ((Vc1 ^ (1 / 3) + Vc2 ^ (1 / 3)) ^ 3)

            r.Cells(2).Value = tmp

        Next

    End Sub

    Private Sub Button2_Click(ByVal sender As System.Object, ByVal e As System.EventArgs) Handles Button2.Click
        Dim ppu As Object = _pp
        If ppu.ComponentName.Contains("SRK") Then
            Process.Start(My.Application.Info.DirectoryPath & Path.DirectorySeparatorChar & "data" & Path.DirectorySeparatorChar & "srk_ip.dat")
        Else
            Process.Start(My.Application.Info.DirectoryPath & Path.DirectorySeparatorChar & "data" & Path.DirectorySeparatorChar & "pr_ip.dat")
        End If
    End Sub

    Private Sub Button1_Click(ByVal sender As System.Object, ByVal e As System.EventArgs)
        For Each r As DataGridViewRow In Me.KryptonDataGridView2.Rows

            If r.Cells(2).Value = "" Then

                Dim id1 As String = r.Cells(0).Tag.ToString
                Dim id2 As String = r.Cells(1).Tag.ToString

                Dim comp1, comp2 As ConstantProperties
                comp1 = _comps(id1)
                comp2 = _comps(id2)

                Dim Vc1 As Double = comp1.Critical_Volume
                Dim Vc2 As Double = comp2.Critical_Volume

                Dim tmp As Double = 1 - 8 * (Vc1 * Vc2) ^ 0.5 / ((Vc1 ^ (1 / 3) + Vc2 ^ (1 / 3)) ^ 3)

                r.Cells(2).Value = tmp

            End If

        Next

    End Sub
End Class