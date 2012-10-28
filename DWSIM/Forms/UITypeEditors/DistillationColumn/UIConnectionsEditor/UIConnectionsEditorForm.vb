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

Imports DWSIM.DWSIM.SimulationObjects.UnitOps.Auxiliary.SepOps
Imports DWSIM.DWSIM.SimulationObjects.UnitOps
Imports DWSIM.DWSIM.SimulationObjects
Imports Microsoft.MSDN.Samples.GraphicObjects

Public Class UIConnectionsEditorForm

    Dim dc As Column
    Dim form As FormFlowsheet
    Dim cb As Object
    Dim loaded As Boolean = False

    Dim tpl As DWSIM.SimulationObjects.UnitOps.Auxiliary.DGVCBSelectors.Templates
    Dim cvt As DWSIM.SistemasDeUnidades.Conversor

    Private Sub UIConnectionsEditorForm_FormClosing(ByVal sender As Object, ByVal e As System.Windows.Forms.FormClosingEventArgs) Handles Me.FormClosing
        Dim i As Integer = 0
        Dim obj1(dc.GraphicObject.InputConnectors.Count), obj2(dc.GraphicObject.InputConnectors.Count) As Double
        Dim obj3(dc.GraphicObject.OutputConnectors.Count), obj4(dc.GraphicObject.OutputConnectors.Count) As Double
        For Each ic As ConnectionPoint In dc.GraphicObject.InputConnectors
            obj1(i) = -dc.GraphicObject.X + ic.Position.X
            obj2(i) = -dc.GraphicObject.Y + ic.Position.Y
            i = i + 1
        Next
        i = 0
        For Each oc As ConnectionPoint In dc.GraphicObject.OutputConnectors
            obj3(i) = -dc.GraphicObject.X + oc.Position.X
            obj4(i) = -dc.GraphicObject.Y + oc.Position.Y
            i = i + 1
        Next
        dc.GraphicObject.AdditionalInfo = New Object() {obj1, obj2, obj3, obj4}
        form.FormSurface.FlowsheetDesignSurface.SelectedObject = dc.GraphicObject

    End Sub

    Private Sub UIConnectionsEditorForm_Load(ByVal sender As System.Object, ByVal e As System.EventArgs) Handles MyBase.Load

        cvt = New DWSIM.SistemasDeUnidades.Conversor()

        Dim i As Integer = 0

        form = My.Application.ActiveSimulation
        dc = form.Collections.ObjectCollection(form.FormSurface.FlowsheetDesignSurface.SelectedObject.Name)

        tpl = New DWSIM.SimulationObjects.UnitOps.Auxiliary.DGVCBSelectors.Templates(form)

        Dim sgt1 As New DataGridViewComboBoxCell
        Dim sgt2 As New DataGridViewComboBoxCell
        With sgt1.Items
            i = 0
            For Each st As Stage In dc.Stages
                .Add(st.Name)
                i += 1
            Next
        End With
        With sgt2.Items
            i = 0
            For Each st As Stage In dc.Stages
                .Add(st.Name)
                i += 1
            Next
        End With

        'selectors
        dgv1.Columns(1).CellTemplate = tpl.GetMaterialStreamInSelector
        dgv1.Columns(2).CellTemplate = sgt1.Clone
        dgv2.Columns(1).CellTemplate = tpl.GetMaterialStreamOutSelector
        dgv2.Columns(2).CellTemplate = sgt2.Clone
        dgv2.Columns(3).CellTemplate = tpl.GetSideDrawTypeSelector
        dgv3.Columns(1).CellTemplate = tpl.GetMaterialStreamOutSelector
        dgv4.Columns(0).CellTemplate = sgt1.Clone
        dgv4.Columns(1).CellTemplate = tpl.GetEnergyStreamInSelector

        dgv2.Columns(4).HeaderText += " (" & form.Options.SelectedUnitSystem.spmp_molarflow & ")"

        For Each str As StreamInformation In dc.MaterialStreams.Values
            If str.StreamBehavior = StreamInformation.Behavior.Feed Then
                Me.dgv1.Rows.Add(New Object() {dgv1.Rows.Count + 1, ReturnObjTag(str.Name), str.AssociatedStage, str.ID})
            End If
        Next

        For Each str As StreamInformation In dc.MaterialStreams.Values
            If str.StreamBehavior = StreamInformation.Behavior.Sidedraw Then
                Me.dgv2.Rows.Add(New Object() {dgv2.Rows.Count + 1, ReturnObjTag(str.Name), str.AssociatedStage, str.StreamPhase.ToString, cvt.ConverterDoSI(form.Options.SelectedUnitSystem.spmp_molarflow, str.FlowRate.Value), str.ID})
            End If
        Next

        Select Case dc.ColumnType
            Case Column.ColType.DistillationColumn
                Dim id As String = Guid.NewGuid.ToString
                Select Case dc.CondenserType
                    Case Column.condtype.Full_Reflux
                        If Not dc.StreamExists(StreamInformation.Behavior.OverheadVapor) Then
                            id = Guid.NewGuid.ToString
                            dc.MaterialStreams.Add(id.ToString, New StreamInformation(id.ToString, "0", "0", "0", StreamInformation.Type.Material, StreamInformation.Behavior.OverheadVapor, StreamInformation.Phase.V))
                        End If
                    Case Column.condtype.Partial_Condenser
                        If Not dc.StreamExists(StreamInformation.Behavior.Distillate) Then
                            id = Guid.NewGuid.ToString
                            dc.MaterialStreams.Add(id.ToString, New StreamInformation(id.ToString, "0", "0", "0", StreamInformation.Type.Material, StreamInformation.Behavior.Distillate, StreamInformation.Phase.L))
                        End If
                        If Not dc.StreamExists(StreamInformation.Behavior.OverheadVapor) Then
                            id = Guid.NewGuid.ToString
                            dc.MaterialStreams.Add(id.ToString, New StreamInformation(id.ToString, "0", "0", "0", StreamInformation.Type.Material, StreamInformation.Behavior.OverheadVapor, StreamInformation.Phase.V))
                        End If
                    Case Column.condtype.Total_Condenser
                        If Not dc.StreamExists(StreamInformation.Behavior.Distillate) Then
                            id = Guid.NewGuid.ToString
                            dc.MaterialStreams.Add(id.ToString, New StreamInformation(id.ToString, "0", "0", "0", StreamInformation.Type.Material, StreamInformation.Behavior.Distillate, StreamInformation.Phase.L))
                        End If
                End Select
                If Not dc.StreamExists(StreamInformation.Behavior.BottomsLiquid) Then
                    id = Guid.NewGuid.ToString
                    dc.MaterialStreams.Add(id.ToString, New StreamInformation(id.ToString, "0", "0", "0", StreamInformation.Type.Material, StreamInformation.Behavior.BottomsLiquid, StreamInformation.Phase.L))
                End If
            Case Column.ColType.AbsorptionColumn
                Dim id As String = Guid.NewGuid.ToString
                If Not dc.StreamExists(StreamInformation.Behavior.BottomsLiquid) Then
                    id = Guid.NewGuid.ToString
                    dc.MaterialStreams.Add(id.ToString, New StreamInformation(id.ToString, "0", "0", "0", StreamInformation.Type.Material, StreamInformation.Behavior.BottomsLiquid, StreamInformation.Phase.L))
                End If
                If Not dc.StreamExists(StreamInformation.Behavior.OverheadVapor) Then
                    id = Guid.NewGuid.ToString
                    dc.MaterialStreams.Add(id.ToString, New StreamInformation(id.ToString, "0", "0", "0", StreamInformation.Type.Material, StreamInformation.Behavior.OverheadVapor, StreamInformation.Phase.V))
                End If
            Case Column.ColType.ReboiledAbsorber
                Dim id As String = Guid.NewGuid.ToString
                If Not dc.StreamExists(StreamInformation.Behavior.OverheadVapor) Then
                    id = Guid.NewGuid.ToString
                    dc.MaterialStreams.Add(id.ToString, New StreamInformation(id.ToString, "0", "0", "0", StreamInformation.Type.Material, StreamInformation.Behavior.OverheadVapor, StreamInformation.Phase.V))
                End If
                If Not dc.StreamExists(StreamInformation.Behavior.BottomsLiquid) Then
                    id = Guid.NewGuid.ToString
                    dc.MaterialStreams.Add(id.ToString, New StreamInformation(id.ToString, "0", "0", "0", StreamInformation.Type.Material, StreamInformation.Behavior.BottomsLiquid, StreamInformation.Phase.L))
                End If
            Case Column.ColType.RefluxedAbsorber
                Dim id As String = Guid.NewGuid.ToString
                Select Case dc.CondenserType
                    Case Column.condtype.Full_Reflux
                        If Not dc.StreamExists(StreamInformation.Behavior.OverheadVapor) Then
                            id = Guid.NewGuid.ToString
                            dc.MaterialStreams.Add(id.ToString, New StreamInformation(id.ToString, "0", "0", "0", StreamInformation.Type.Material, StreamInformation.Behavior.OverheadVapor, StreamInformation.Phase.V))
                        End If
                    Case Column.condtype.Partial_Condenser
                        If Not dc.StreamExists(StreamInformation.Behavior.Distillate) Then
                            id = Guid.NewGuid.ToString
                            dc.MaterialStreams.Add(id.ToString, New StreamInformation(id.ToString, "0", "0", "0", StreamInformation.Type.Material, StreamInformation.Behavior.Distillate, StreamInformation.Phase.L))
                        End If
                        If Not dc.StreamExists(StreamInformation.Behavior.OverheadVapor) Then
                            id = Guid.NewGuid.ToString
                            dc.MaterialStreams.Add(id.ToString, New StreamInformation(id.ToString, "0", "0", "0", StreamInformation.Type.Material, StreamInformation.Behavior.OverheadVapor, StreamInformation.Phase.V))
                        End If
                    Case Column.condtype.Total_Condenser
                        If Not dc.StreamExists(StreamInformation.Behavior.Distillate) Then
                            id = Guid.NewGuid.ToString
                            dc.MaterialStreams.Add(id.ToString, New StreamInformation(id.ToString, "0", "0", "0", StreamInformation.Type.Material, StreamInformation.Behavior.Distillate, StreamInformation.Phase.L))
                        End If
                End Select
                If Not dc.StreamExists(StreamInformation.Behavior.BottomsLiquid) Then
                    id = Guid.NewGuid.ToString
                    dc.MaterialStreams.Add(id.ToString, New StreamInformation(id.ToString, "0", "0", "0", StreamInformation.Type.Material, StreamInformation.Behavior.BottomsLiquid, StreamInformation.Phase.L))
                End If
        End Select

        Me.dgv3.Rows.Clear()

        Select Case dc.ColumnType
            Case Column.ColType.DistillationColumn
                Dim id As String = Guid.NewGuid.ToString
                Select Case dc.CondenserType
                    Case Column.condtype.Full_Reflux
                        Me.dgv3.Rows.Add(New Object() {DWSIM.App.GetLocalString("DCCondenserVaporProduct"), "", id})
                    Case Column.condtype.Partial_Condenser
                        Me.dgv3.Rows.Add(New Object() {DWSIM.App.GetLocalString("DCCondenser"), "", id})
                        Me.dgv3.Rows.Add(New Object() {DWSIM.App.GetLocalString("DCCondenserVaporProduct"), "", id})
                    Case Column.condtype.Total_Condenser
                        Me.dgv3.Rows.Add(New Object() {DWSIM.App.GetLocalString("DCCondenser"), "", id})
                End Select
                Me.dgv3.Rows.Add(New Object() {DWSIM.App.GetLocalString("DCReboiler"), "", id})
            Case Column.ColType.AbsorptionColumn
                Dim id As String = Guid.NewGuid.ToString
                Me.dgv3.Rows.Add(New Object() {DWSIM.App.GetLocalString("DCOvrhdProd"), "", id})
                Me.dgv3.Rows.Add(New Object() {DWSIM.App.GetLocalString("DCBotProd"), "", id})
            Case Column.ColType.ReboiledAbsorber
                Dim id As String = Guid.NewGuid.ToString
                Me.dgv3.Rows.Add(New Object() {DWSIM.App.GetLocalString("DCOvrhdProd"), "", id})
                Me.dgv3.Rows.Add(New Object() {DWSIM.App.GetLocalString("DCReboiler"), "", id})
            Case Column.ColType.RefluxedAbsorber
                Dim id As String = Guid.NewGuid.ToString
                Select Case dc.CondenserType
                    Case Column.condtype.Full_Reflux
                        Me.dgv3.Rows.Add(New Object() {DWSIM.App.GetLocalString("DCCondenserVaporProduct"), "", id})
                    Case Column.condtype.Partial_Condenser
                        Me.dgv3.Rows.Add(New Object() {DWSIM.App.GetLocalString("DCCondenser"), "", id})
                        Me.dgv3.Rows.Add(New Object() {DWSIM.App.GetLocalString("DCCondenserVaporProduct"), "", id})
                    Case Column.condtype.Total_Condenser
                        Me.dgv3.Rows.Add(New Object() {DWSIM.App.GetLocalString("DCCondenser"), "", id})
                End Select
                Me.dgv3.Rows.Add(New Object() {DWSIM.App.GetLocalString("DCBotProd"), "", id})
        End Select

        For Each str As StreamInformation In dc.MaterialStreams.Values
            Select Case dc.ColumnType
                Case Column.ColType.DistillationColumn
                    Select Case dc.CondenserType
                        Case Column.condtype.Full_Reflux
                            If str.StreamBehavior = StreamInformation.Behavior.OverheadVapor Then
                                Me.dgv3.Rows(0).Cells(1).Value = ReturnObjTag(str.Name)
                                Me.dgv3.Rows(0).Cells(2).Value = str.ID
                            End If
                            If str.StreamBehavior = StreamInformation.Behavior.BottomsLiquid Then
                                Me.dgv3.Rows(1).Cells(1).Value = ReturnObjTag(str.Name)
                                Me.dgv3.Rows(1).Cells(2).Value = str.ID
                            End If
                        Case Column.condtype.Partial_Condenser
                            If str.StreamBehavior = StreamInformation.Behavior.Distillate Then
                                Me.dgv3.Rows(0).Cells(1).Value = ReturnObjTag(str.Name)
                                Me.dgv3.Rows(0).Cells(2).Value = str.ID
                            End If
                            If str.StreamBehavior = StreamInformation.Behavior.OverheadVapor Then
                                Me.dgv3.Rows(1).Cells(1).Value = ReturnObjTag(str.Name)
                                Me.dgv3.Rows(1).Cells(2).Value = str.ID
                            End If
                            If str.StreamBehavior = StreamInformation.Behavior.BottomsLiquid Then
                                Me.dgv3.Rows(2).Cells(1).Value = ReturnObjTag(str.Name)
                                Me.dgv3.Rows(2).Cells(2).Value = str.ID
                            End If
                        Case Column.condtype.Total_Condenser
                            If str.StreamBehavior = StreamInformation.Behavior.Distillate Then
                                Me.dgv3.Rows(0).Cells(1).Value = ReturnObjTag(str.Name)
                                Me.dgv3.Rows(0).Cells(2).Value = str.ID
                            End If
                            If str.StreamBehavior = StreamInformation.Behavior.BottomsLiquid Then
                                Me.dgv3.Rows(1).Cells(1).Value = ReturnObjTag(str.Name)
                                Me.dgv3.Rows(1).Cells(2).Value = str.ID
                            End If
                    End Select
                Case Column.ColType.AbsorptionColumn
                    If str.StreamBehavior = StreamInformation.Behavior.OverheadVapor Then
                        Me.dgv3.Rows(0).Cells(1).Value = ReturnObjTag(str.Name)
                        Me.dgv3.Rows(0).Cells(2).Value = str.ID
                    End If
                    If str.StreamBehavior = StreamInformation.Behavior.BottomsLiquid Then
                        Me.dgv3.Rows(1).Cells(1).Value = ReturnObjTag(str.Name)
                        Me.dgv3.Rows(1).Cells(2).Value = str.ID
                    End If
                Case Column.ColType.ReboiledAbsorber
                    If str.StreamBehavior = StreamInformation.Behavior.OverheadVapor Then
                        Me.dgv3.Rows(0).Cells(1).Value = ReturnObjTag(str.Name)
                        Me.dgv3.Rows(0).Cells(2).Value = str.ID
                    End If
                    If str.StreamBehavior = StreamInformation.Behavior.BottomsLiquid Then
                        Me.dgv3.Rows(1).Cells(1).Value = ReturnObjTag(str.Name)
                        Me.dgv3.Rows(1).Cells(2).Value = str.ID
                    End If
                Case Column.ColType.RefluxedAbsorber
                    Select Case dc.CondenserType
                        Case Column.condtype.Full_Reflux
                            If str.StreamBehavior = StreamInformation.Behavior.OverheadVapor Then
                                Me.dgv3.Rows(0).Cells(1).Value = ReturnObjTag(str.Name)
                                Me.dgv3.Rows(0).Cells(2).Value = str.ID
                            End If
                            If str.StreamBehavior = StreamInformation.Behavior.BottomsLiquid Then
                                Me.dgv3.Rows(1).Cells(1).Value = ReturnObjTag(str.Name)
                                Me.dgv3.Rows(1).Cells(2).Value = str.ID
                            End If
                        Case Column.condtype.Partial_Condenser
                            If str.StreamBehavior = StreamInformation.Behavior.Distillate Then
                                Me.dgv3.Rows(0).Cells(1).Value = ReturnObjTag(str.Name)
                                Me.dgv3.Rows(0).Cells(2).Value = str.ID
                            End If
                            If str.StreamBehavior = StreamInformation.Behavior.OverheadVapor Then
                                Me.dgv3.Rows(1).Cells(1).Value = ReturnObjTag(str.Name)
                                Me.dgv3.Rows(1).Cells(2).Value = str.ID
                            End If
                            If str.StreamBehavior = StreamInformation.Behavior.BottomsLiquid Then
                                Me.dgv3.Rows(2).Cells(1).Value = ReturnObjTag(str.Name)
                                Me.dgv3.Rows(2).Cells(2).Value = str.ID
                            End If
                        Case Column.condtype.Total_Condenser
                            If str.StreamBehavior = StreamInformation.Behavior.Distillate Then
                                Me.dgv3.Rows(0).Cells(1).Value = ReturnObjTag(str.Name)
                                Me.dgv3.Rows(0).Cells(2).Value = str.ID
                            End If
                            If str.StreamBehavior = StreamInformation.Behavior.BottomsLiquid Then
                                Me.dgv3.Rows(1).Cells(1).Value = ReturnObjTag(str.Name)
                                Me.dgv3.Rows(1).Cells(2).Value = str.ID
                            End If
                    End Select
            End Select
        Next

        For Each str As StreamInformation In dc.EnergyStreams.Values
            If str.StreamBehavior = StreamInformation.Behavior.Distillate Then
                Me.dgv4.Rows.Add(New Object() {dc.Stages(0).Name, ReturnObjTag(str.Name), str.ID})
                Me.dgv4.Rows(Me.dgv4.Rows.Count - 1).Cells(0).ReadOnly = True
            ElseIf str.StreamBehavior = StreamInformation.Behavior.BottomsLiquid Then
                Me.dgv4.Rows.Add(New Object() {dc.Stages(dc.Stages.Count - 1).Name, ReturnObjTag(str.Name), str.ID})
                Me.dgv4.Rows(Me.dgv4.Rows.Count - 1).Cells(0).ReadOnly = True
            ElseIf str.StreamBehavior = StreamInformation.Behavior.InterExchanger Then
                Me.dgv4.Rows.Add(New Object() {str.AssociatedStage, ReturnObjTag(str.Name), str.ID})
            End If
        Next

        Select Case dc.ColumnType
            Case Column.ColType.DistillationColumn
                If dgv4.Rows.Count = 0 Then
                    Dim id As String = Guid.NewGuid.ToString
                    dc.EnergyStreams.Add(id.ToString, New StreamInformation(id.ToString, "0", "0", "0", StreamInformation.Type.Energy, StreamInformation.Behavior.Distillate, StreamInformation.Phase.None))
                    Me.dgv4.Rows.Add(New Object() {DWSIM.App.GetLocalString("DCCondenser"), "", id})
                    Me.dgv4.Rows(Me.dgv4.Rows.Count - 1).Cells(0).ReadOnly = True
                    id = Guid.NewGuid.ToString
                    dc.EnergyStreams.Add(id.ToString, New StreamInformation(id.ToString, "0", "0", "0", StreamInformation.Type.Energy, StreamInformation.Behavior.BottomsLiquid, StreamInformation.Phase.None))
                    Me.dgv4.Rows.Add(New Object() {DWSIM.App.GetLocalString("DCReboiler"), "", id})
                    Me.dgv4.Rows(Me.dgv4.Rows.Count - 1).Cells(0).ReadOnly = True
                End If
            Case Column.ColType.ReboiledAbsorber
                If dgv4.Rows.Count = 0 Then
                    Dim id As String = Guid.NewGuid.ToString
                    dc.EnergyStreams.Add(id.ToString, New StreamInformation(id.ToString, "0", "0", "0", StreamInformation.Type.Energy, StreamInformation.Behavior.BottomsLiquid, StreamInformation.Phase.None))
                    Me.dgv4.Rows.Add(New Object() {DWSIM.App.GetLocalString("DCReboiler"), "", id})
                    Me.dgv4.Rows(Me.dgv4.Rows.Count - 1).Cells(0).ReadOnly = True
                End If
            Case Column.ColType.RefluxedAbsorber
                If dgv4.Rows.Count = 0 Then
                    Dim id As String = Guid.NewGuid.ToString
                    dc.EnergyStreams.Add(id.ToString, New StreamInformation(id.ToString, "0", "0", "0", StreamInformation.Type.Energy, StreamInformation.Behavior.Distillate, StreamInformation.Phase.None))
                    Me.dgv4.Rows.Add(New Object() {DWSIM.App.GetLocalString("DCCondenser"), "", id})
                    Me.dgv4.Rows(Me.dgv4.Rows.Count - 1).Cells(0).ReadOnly = True
                End If
        End Select

        loaded = True

    End Sub

    Private Sub ToolStripButton1_Click(ByVal sender As System.Object, ByVal e As System.EventArgs) Handles ToolStripButton1.Click
        With Me.dgv1.Rows
            Dim id As String = Guid.NewGuid.ToString
            .Add(New Object() {dgv1.Rows.Count + 1, "", "", id.ToString})
            dc.MaterialStreams.Add(id.ToString, New StreamInformation(id.ToString, "0", "", "", StreamInformation.Type.Material, StreamInformation.Behavior.Feed, StreamInformation.Phase.B))
        End With
    End Sub

    Private Sub ToolStripButton3_Click(ByVal sender As System.Object, ByVal e As System.EventArgs) Handles ToolStripButton3.Click
        With Me.dgv2.Rows
            Dim id As String = Guid.NewGuid.ToString
            .Add(New Object() {dgv2.Rows.Count + 1, "", "", "L", 0, id.ToString})
            dc.MaterialStreams.Add(id.ToString, New StreamInformation(id.ToString, "0", "", "", StreamInformation.Type.Material, StreamInformation.Behavior.Sidedraw, StreamInformation.Phase.L))
        End With
    End Sub

    Private Sub ToolStripButton2_Click(ByVal sender As System.Object, ByVal e As System.EventArgs) Handles ToolStripButton2.Click
        Dim id As String = dgv1.Rows(dgv1.SelectedCells(0).RowIndex).Cells(3).Value
        With Me.dgv1.Rows
            If Not dc.MaterialStreams(id).Name = "" Then
                Dim idx As Integer = FormFlowsheet.SearchSurfaceObjectsByTag(dc.MaterialStreams(id).Tag, form.FormSurface.FlowsheetDesignSurface).OutputConnectors(0).AttachedConnector.AttachedToConnectorIndex
                form.DisconnectObject(FormFlowsheet.SearchSurfaceObjectsByTag(dc.MaterialStreams(id).Tag, form.FormSurface.FlowsheetDesignSurface), dc.GraphicObject)
                dc.GraphicObject.InputConnectors.RemoveAt(idx)
            End If
            dc.MaterialStreams.Remove(dgv1.Rows(dgv1.SelectedCells(0).RowIndex).Cells(3).Value.ToString)
            .RemoveAt(dgv1.SelectedCells(0).RowIndex)
        End With
    End Sub

    Private Sub ToolStripButton4_Click(ByVal sender As System.Object, ByVal e As System.EventArgs) Handles ToolStripButton4.Click
        Dim id As String = dgv2.Rows(dgv2.SelectedCells(0).RowIndex).Cells(5).Value
        With Me.dgv2.Rows
            If Not dc.MaterialStreams(id).Name = "" Then
                Dim idx As Integer = FormFlowsheet.SearchSurfaceObjectsByTag(dc.MaterialStreams(id).Tag, form.FormSurface.FlowsheetDesignSurface).InputConnectors(0).AttachedConnector.AttachedFromConnectorIndex
                form.DisconnectObject(dc.GraphicObject, FormFlowsheet.SearchSurfaceObjectsByTag(dc.MaterialStreams(id).Tag, form.FormSurface.FlowsheetDesignSurface))
                dc.GraphicObject.OutputConnectors.RemoveAt(idx)
            End If
            dc.MaterialStreams.Remove(dgv2.Rows(dgv2.SelectedCells(0).RowIndex).Cells(5).Value.ToString)
            .RemoveAt(dgv2.SelectedCells(0).RowIndex)
        End With
    End Sub

    Private Shared Function ContainsCONDENSER(ByVal s As String) As Boolean

        If s.Contains("CONDENSER") Then
            Return True
        Else
            Return False
        End If

    End Function

    Private Shared Function ContainsREBOILER(ByVal s As String) As Boolean

        If s.Contains("REBOILER") Then
            Return True
        Else
            Return False
        End If

    End Function

    Private Sub dgv1_CellValueChanged(ByVal sender As Object, ByVal e As System.Windows.Forms.DataGridViewCellEventArgs) Handles dgv1.CellValueChanged
        If loaded Then
            Dim id As String = dgv1.Rows(e.RowIndex).Cells(3).Value
            Select Case e.ColumnIndex
                Case 1
                    If Not dc.MaterialStreams(id).Name = "" Then
                        Try
                            Dim idx As Integer = FormFlowsheet.SearchSurfaceObjectsByTag(dc.MaterialStreams(id).Tag, form.FormSurface.FlowsheetDesignSurface).OutputConnectors(0).AttachedConnector.AttachedToConnectorIndex
                            form.DisconnectObject(FormFlowsheet.SearchSurfaceObjectsByTag(dc.MaterialStreams(id).Tag, form.FormSurface.FlowsheetDesignSurface), dc.GraphicObject)
                            dc.GraphicObject.InputConnectors.RemoveAt(idx)
                        Catch ex As Exception
                        End Try
                    End If
                    dc.MaterialStreams(id).Tag = dgv1.Rows(e.RowIndex).Cells(e.ColumnIndex).Value
                    dc.MaterialStreams(id).Name = FormFlowsheet.SearchSurfaceObjectsByTag(dc.MaterialStreams(id).Tag, form.FormSurface.FlowsheetDesignSurface).Name
                    Dim fidx, tidx As Integer
                    With dc.GraphicObject
                        .InputConnectors.Add(New ConnectionPoint())
                        .InputConnectors(.InputConnectors.Count - 1).Type = ConType.ConIn
                        fidx = 0
                        tidx = .InputConnectors.Count - 1
                        If dc.GraphicObject.FlippedH Then
                            .InputConnectors(.InputConnectors.Count - 1).Position = New Point(dc.GraphicObject.X + dc.GraphicObject.Width, dc.GraphicObject.Y + dc.StageIndex(dc.MaterialStreams(id).AssociatedStage) / dc.NumberOfStages * dc.GraphicObject.Height)
                        Else
                            .InputConnectors(.InputConnectors.Count - 1).Position = New Point(dc.GraphicObject.X, dc.GraphicObject.Y + dc.StageIndex(dc.MaterialStreams(id).AssociatedStage) / dc.NumberOfStages * dc.GraphicObject.Height)
                        End If
                        form.ConnectObject(FormFlowsheet.SearchSurfaceObjectsByTag(dc.MaterialStreams(id).Tag, form.FormSurface.FlowsheetDesignSurface), dc.GraphicObject, fidx, tidx)
                    End With
                Case 2
                    dc.MaterialStreams(id).AssociatedStage = dgv1.Rows(e.RowIndex).Cells(e.ColumnIndex).Value
            End Select
        End If
    End Sub

    Private Sub dgv2_CellValueChanged(ByVal sender As System.Object, ByVal e As System.Windows.Forms.DataGridViewCellEventArgs) Handles dgv2.CellValueChanged
        If loaded Then
            Dim id As String = dgv2.Rows(e.RowIndex).Cells(5).Value
            Dim value As Object = dgv2.Rows(e.RowIndex).Cells(e.ColumnIndex).Value
            Select Case e.ColumnIndex
                Case 1
                    If Not dc.MaterialStreams(id).Name = "" Then
                        Try
                            Dim idx As Integer = FormFlowsheet.SearchSurfaceObjectsByTag(dc.MaterialStreams(id).Tag, form.FormSurface.FlowsheetDesignSurface).InputConnectors(0).AttachedConnector.AttachedFromConnectorIndex
                            form.DisconnectObject(dc.GraphicObject, FormFlowsheet.SearchSurfaceObjectsByTag(dc.MaterialStreams(id).Tag, form.FormSurface.FlowsheetDesignSurface))
                            dc.GraphicObject.OutputConnectors.RemoveAt(idx)
                        Catch ex As Exception
                        End Try
                    End If
                    dc.MaterialStreams(id).Tag = value
                    dc.MaterialStreams(id).Name = FormFlowsheet.SearchSurfaceObjectsByTag(dc.MaterialStreams(id).Tag, form.FormSurface.FlowsheetDesignSurface).Name
                    Dim fidx, tidx As Integer
                    With dc.GraphicObject
                        .OutputConnectors.Add(New ConnectionPoint())
                        fidx = .OutputConnectors.Count - 1
                        tidx = 0
                        .OutputConnectors(.OutputConnectors.Count - 1).Type = ConType.ConOut
                        If dc.GraphicObject.FlippedH Then
                            .OutputConnectors(.OutputConnectors.Count - 1).Position = New Point(dc.GraphicObject.X, dc.GraphicObject.Y + dc.StageIndex(dc.MaterialStreams(id).AssociatedStage) / dc.NumberOfStages * dc.GraphicObject.Height)
                        Else
                            .OutputConnectors(.OutputConnectors.Count - 1).Position = New Point(dc.GraphicObject.X + dc.GraphicObject.Width, dc.GraphicObject.Y + dc.StageIndex(dc.MaterialStreams(id).AssociatedStage) / dc.NumberOfStages * dc.GraphicObject.Height)
                        End If
                        form.ConnectObject(dc.GraphicObject, FormFlowsheet.SearchSurfaceObjectsByTag(dc.MaterialStreams(id).Tag, form.FormSurface.FlowsheetDesignSurface), fidx, tidx)
                    End With
                Case 2
                    dc.MaterialStreams(id).AssociatedStage = value
                Case 3
                    If dgv2.Rows(e.RowIndex).Cells(e.ColumnIndex).Value = "L" Then
                        dc.MaterialStreams(id).StreamPhase = StreamInformation.Phase.L
                    Else
                        dc.MaterialStreams(id).StreamPhase = StreamInformation.Phase.V
                    End If
                Case 4
                    dc.MaterialStreams(id).FlowRate.Value = cvt.ConverterParaSI(form.Options.SelectedUnitSystem.spmp_molarflow, value)
            End Select
        End If
    End Sub

    Private Sub dgv3_CellValueChanged(ByVal sender As System.Object, ByVal e As System.Windows.Forms.DataGridViewCellEventArgs) Handles dgv3.CellValueChanged
        If loaded Then
            Dim id As String = dgv3.Rows(e.RowIndex).Cells(2).Value
            Select Case e.ColumnIndex
                Case 1
                    If Not dc.MaterialStreams(id).Name = "" Then
                        Try
                            Dim idx As Integer = FormFlowsheet.SearchSurfaceObjectsByTag(dc.MaterialStreams(id).Tag, form.FormSurface.FlowsheetDesignSurface).InputConnectors(0).AttachedConnector.AttachedFromConnectorIndex
                            form.DisconnectObject(dc.GraphicObject, FormFlowsheet.SearchSurfaceObjectsByTag(dc.MaterialStreams(id).Tag, form.FormSurface.FlowsheetDesignSurface))
                            dc.GraphicObject.OutputConnectors.RemoveAt(idx)
                        Catch ex As Exception
                        End Try
                    End If
                    dc.MaterialStreams(id).Tag = dgv3.Rows(e.RowIndex).Cells(e.ColumnIndex).Value
                    dc.MaterialStreams(id).Name = FormFlowsheet.SearchSurfaceObjectsByTag(dc.MaterialStreams(id).Tag, form.FormSurface.FlowsheetDesignSurface).Name
                    Dim fidx, tidx As Integer
                    With dc.GraphicObject
                        .OutputConnectors.Add(New ConnectionPoint())
                        .OutputConnectors(.OutputConnectors.Count - 1).Type = ConType.ConOut
                        fidx = .OutputConnectors.Count - 1
                        tidx = 0
                        Dim pos As Point
                        Select Case dc.MaterialStreams(id).StreamBehavior
                            Case StreamInformation.Behavior.OverheadVapor
                                If Not dc.GraphicObject.FlippedH Then
                                    pos = New Point(dc.GraphicObject.X + dc.GraphicObject.Width, dc.GraphicObject.Y + 0.02 * dc.GraphicObject.Height)
                                Else
                                    pos = New Point(dc.GraphicObject.X, dc.GraphicObject.Y + 0.02 * dc.GraphicObject.Height)
                                End If
                            Case StreamInformation.Behavior.Distillate
                                If Not dc.GraphicObject.FlippedH Then
                                    pos = New Point(dc.GraphicObject.X + dc.GraphicObject.Width, dc.GraphicObject.Y + 0.3 * dc.GraphicObject.Height)
                                Else
                                    pos = New Point(dc.GraphicObject.X, dc.GraphicObject.Y + 0.3 * dc.GraphicObject.Height)
                                End If
                            Case StreamInformation.Behavior.BottomsLiquid
                                If Not dc.GraphicObject.FlippedH Then
                                    pos = New Point(dc.GraphicObject.X + dc.GraphicObject.Width, dc.GraphicObject.Y + 0.98 * dc.GraphicObject.Height)
                                Else
                                    pos = New Point(dc.GraphicObject.X, dc.GraphicObject.Y + 0.98 * dc.GraphicObject.Height)
                                End If
                        End Select
                        .OutputConnectors(.OutputConnectors.Count - 1).Position = pos
                        form.ConnectObject(dc.GraphicObject, FormFlowsheet.SearchSurfaceObjectsByTag(dc.MaterialStreams(id).Tag, form.FormSurface.FlowsheetDesignSurface), fidx, tidx)
                    End With
            End Select
        End If
    End Sub

    Private Sub dgv4_CellValueChanged(ByVal sender As System.Object, ByVal e As System.Windows.Forms.DataGridViewCellEventArgs) Handles dgv4.CellValueChanged
        If loaded Then
            Dim id As String = dgv4.Rows(e.RowIndex).Cells(2).Value
            Select Case e.ColumnIndex
                Case 1
                    If Not dc.EnergyStreams(id).Name = "" Then
                        Try
                            Dim idx As Integer = FormFlowsheet.SearchSurfaceObjectsByTag(dc.EnergyStreams(id).Tag, form.FormSurface.FlowsheetDesignSurface).InputConnectors(0).AttachedConnector.AttachedFromConnectorIndex
                            form.DisconnectObject(dc.GraphicObject, FormFlowsheet.SearchSurfaceObjectsByTag(dc.EnergyStreams(id).Tag, form.FormSurface.FlowsheetDesignSurface))
                            dc.GraphicObject.OutputConnectors.RemoveAt(idx)
                        Catch ex As Exception
                        End Try
                    End If
                    dc.EnergyStreams(id).Tag = dgv4.Rows(e.RowIndex).Cells(e.ColumnIndex).Value
                    dc.EnergyStreams(id).Name = FormFlowsheet.SearchSurfaceObjectsByTag(dc.EnergyStreams(id).Tag, form.FormSurface.FlowsheetDesignSurface).Name
                    Dim fidx, tidx As Integer
                    With dc.GraphicObject
                        Select Case dc.EnergyStreams(id).StreamBehavior
                            Case StreamInformation.Behavior.Distillate
                                .OutputConnectors.Add(New ConnectionPoint())
                                .OutputConnectors(.OutputConnectors.Count - 1).Type = ConType.ConEn
                                fidx = .OutputConnectors.Count - 1
                                tidx = 0
                                If dc.GraphicObject.FlippedH Then
                                    .OutputConnectors(.OutputConnectors.Count - 1).Position = New Point(dc.GraphicObject.X, dc.GraphicObject.Y + 0.175 * dc.GraphicObject.Height)
                                Else
                                    .OutputConnectors(.OutputConnectors.Count - 1).Position = New Point(dc.GraphicObject.X + dc.GraphicObject.Width, dc.GraphicObject.Y + 0.175 * dc.GraphicObject.Height)
                                End If
                                form.ConnectObject(dc.GraphicObject, FormFlowsheet.SearchSurfaceObjectsByTag(dc.EnergyStreams(id).Tag, form.FormSurface.FlowsheetDesignSurface), fidx, tidx)
                            Case StreamInformation.Behavior.BottomsLiquid
                                .OutputConnectors.Add(New ConnectionPoint())
                                .OutputConnectors(.OutputConnectors.Count - 1).Type = ConType.ConEn
                                fidx = .OutputConnectors.Count - 1
                                tidx = 0
                                If dc.GraphicObject.FlippedH Then
                                    .OutputConnectors(.OutputConnectors.Count - 1).Position = New Point(dc.GraphicObject.X, dc.GraphicObject.Y + 0.825 * dc.GraphicObject.Height)
                                Else
                                    .OutputConnectors(.OutputConnectors.Count - 1).Position = New Point(dc.GraphicObject.X + dc.GraphicObject.Width, dc.GraphicObject.Y + 0.825 * dc.GraphicObject.Height)
                                End If
                                form.ConnectObject(dc.GraphicObject, FormFlowsheet.SearchSurfaceObjectsByTag(dc.EnergyStreams(id).Tag, form.FormSurface.FlowsheetDesignSurface), fidx, tidx)
                            Case StreamInformation.Behavior.InterExchanger
                                .InputConnectors.Add(New ConnectionPoint())
                                .InputConnectors(.InputConnectors.Count - 1).Type = ConType.ConEn
                                fidx = 0
                                tidx = .InputConnectors.Count - 1
                                If dc.GraphicObject.FlippedH Then
                                    .InputConnectors(.InputConnectors.Count - 1).Position = New Point(dc.GraphicObject.X, dc.GraphicObject.Y + dc.StageIndex(dc.EnergyStreams(id).AssociatedStage) / dc.NumberOfStages * dc.GraphicObject.Height)
                                Else
                                    .InputConnectors(.InputConnectors.Count - 1).Position = New Point(dc.GraphicObject.X + dc.GraphicObject.Width, dc.GraphicObject.Y + dc.StageIndex(dc.EnergyStreams(id).AssociatedStage) / dc.NumberOfStages * dc.GraphicObject.Height)
                                End If
                                form.ConnectObject(FormFlowsheet.SearchSurfaceObjectsByTag(dc.EnergyStreams(id).Tag, form.FormSurface.FlowsheetDesignSurface), dc.GraphicObject, fidx, tidx)
                        End Select
                    End With
            End Select
        End If
    End Sub

    Private Sub dgv1_DataError(ByVal sender As Object, ByVal e As System.Windows.Forms.DataGridViewDataErrorEventArgs) Handles dgv1.DataError
        e.Cancel = True
    End Sub

    Private Sub dgv2_DataError(ByVal sender As Object, ByVal e As System.Windows.Forms.DataGridViewDataErrorEventArgs) Handles dgv2.DataError
        e.Cancel = True
    End Sub

    Private Sub dgv3_DataError(ByVal sender As Object, ByVal e As System.Windows.Forms.DataGridViewDataErrorEventArgs) Handles dgv3.DataError
        e.Cancel = True
    End Sub

    Private Sub dgv4_DataError(ByVal sender As Object, ByVal e As System.Windows.Forms.DataGridViewDataErrorEventArgs) Handles dgv4.DataError
        e.Cancel = True
    End Sub

    Private Sub ToolStripButton5_Click(ByVal sender As System.Object, ByVal e As System.EventArgs) Handles ToolStripButton5.Click
        With Me.dgv4.Rows
            Dim id As String = Guid.NewGuid.ToString
            .Add(New Object() {"", "", id.ToString})
            dc.EnergyStreams.Add(id.ToString, New StreamInformation(id.ToString, "0", "", "", StreamInformation.Type.Energy, StreamInformation.Behavior.InterExchanger, StreamInformation.Phase.None))
        End With
    End Sub

    Private Sub ToolStripButton6_Click(ByVal sender As System.Object, ByVal e As System.EventArgs) Handles ToolStripButton6.Click
        Dim id As String = dgv4.Rows(dgv4.SelectedCells(0).RowIndex).Cells(2).Value
        With Me.dgv4.Rows
            Dim idx2 As Integer
            Select Case dc.ColumnType
                Case Column.ColType.DistillationColumn
                    idx2 = 2
                Case Column.ColType.AbsorptionColumn
                    idx2 = 0
                Case Column.ColType.ReboiledAbsorber
                    idx2 = 1
                Case Column.ColType.RefluxedAbsorber
                    idx2 = 1
            End Select
            If dgv4.SelectedRows(0).Index >= idx2 Then
                If Not dc.EnergyStreams(id).Name = "" Then
                    Dim idx As Integer = FormFlowsheet.SearchSurfaceObjectsByTag(dc.EnergyStreams(id).Tag, form.FormSurface.FlowsheetDesignSurface).InputConnectors(0).AttachedConnector.AttachedFromConnectorIndex
                    form.DisconnectObject(dc.GraphicObject, FormFlowsheet.SearchSurfaceObjectsByTag(dc.EnergyStreams(id).Tag, form.FormSurface.FlowsheetDesignSurface))
                    dc.GraphicObject.OutputConnectors.RemoveAt(idx)
                End If
                dc.EnergyStreams.Remove(dgv4.Rows(dgv4.SelectedCells(0).RowIndex).Cells(2).Value.ToString)
                .RemoveAt(dgv4.SelectedCells(0).RowIndex)
            End If
        End With

    End Sub

    Function ReturnObjTag(ByVal Id As String) As String

        If form.Collections.ObjectCollection.ContainsKey(Id) Then
            Return form.Collections.ObjectCollection(Id).GraphicObject.Tag
        Else
            Return ""
        End If

    End Function

End Class