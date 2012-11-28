'    Copyright 2012 Daniel Wagner O. de Medeiros
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

Imports DWSIM.DWSIM.SimulationObjects.Reactors
Imports DWSIM.DWSIM.SimulationObjects

Public Class GibbsInitialEstimatesEditorForm

    Dim loaded As Boolean = False
    Public gr As Reactor_Gibbs
    Public form As FormFlowsheet
    Public inlet As Streams.MaterialStream
    Dim cv As New DWSIM.SistemasDeUnidades.Conversor
    Dim su As DWSIM.SistemasDeUnidades.Unidades

    Public ie As List(Of Double)

    Private Sub GibbsInitialEstimatesEditorForm_Load(ByVal sender As System.Object, ByVal e As System.EventArgs) Handles MyBase.Load

        Dim nf As String = form.Options.NumberFormat
        su = form.Options.SelectedUnitSystem

        Me.grid.Columns(1).HeaderText += " (" & su.spmp_molarflow & ")"
        Me.grid.Columns(2).HeaderText += " (" & su.spmp_molarflow & ")"

        If ie.Count <> gr.ComponentIDs.Count Then
            ie = New List(Of Double)
            For Each s As String In gr.ComponentIDs
                ie.Add(0.0#)
            Next
        End If

        Dim i As Integer = 0
        For Each s As String In gr.ComponentIDs
            Me.grid.Rows.Add(New Object() {DWSIM.App.GetComponentName(s), Format(cv.ConverterDoSI(su.spmp_molarflow, inlet.Fases(0).Componentes(s).MolarFlow.GetValueOrDefault), nf), Format(cv.ConverterDoSI(su.spmp_molarflow, ie(i)), nf)})
            i += 1
        Next

        loaded = True

    End Sub

    Private Sub grid_CellValueChanged(ByVal sender As Object, ByVal e As System.Windows.Forms.DataGridViewCellEventArgs) Handles grid.CellValueChanged
        If loaded Then
            ie(e.RowIndex) = cv.ConverterParaSI(su.spmp_molarflow, grid.Rows(e.RowIndex).Cells(e.ColumnIndex).Value)
        End If
    End Sub

    Private Sub Button4_Click(ByVal sender As System.Object, ByVal e As System.EventArgs) Handles Button4.Click
        For Each r As DataGridViewRow In grid.Rows
            r.Cells(2).Value = r.Cells(1).Value
        Next
    End Sub
End Class