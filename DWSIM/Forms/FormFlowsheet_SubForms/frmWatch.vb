Imports DWSIM.DWSIM.Outros

<System.Serializable()> Public Class frmWatch

    Inherits WeifenLuo.WinFormsUI.Docking.DockContent

    Public items As New Dictionary(Of Integer, DWSIM.Outros.WatchItem)
    Private updating As Boolean = False
    Private loaded As Boolean = False

    Private Sub frmWatch_Load(ByVal sender As System.Object, ByVal e As System.EventArgs) Handles MyBase.Load

        If items Is Nothing Then items = New Dictionary(Of Integer, DWSIM.Outros.WatchItem)

    End Sub

    Private Sub frmWatch_Shown(ByVal sender As Object, ByVal e As System.EventArgs) Handles Me.Shown

        loaded = True

    End Sub

    Sub PopulateList()

        updating = True

        Me.dgv.Rows.Clear()

        For Each kvp As KeyValuePair(Of Integer, WatchItem) In items
            Dim newitem As WatchItem = kvp.Value
            Dim myobjname As String = My.Application.ActiveSimulation.Collections.ObjectCollection(newitem.ObjID).GraphicObject.Tag
            Dim propname As String = DWSIM.App.GetPropertyName(newitem.PropID)
            Dim propvalue As Object = My.Application.ActiveSimulation.Collections.ObjectCollection(newitem.ObjID).GetPropertyValue(newitem.PropID, My.Application.ActiveSimulation.Options.SelectedUnitSystem)
            Dim propunit As String = My.Application.ActiveSimulation.Collections.ObjectCollection(newitem.ObjID).GetPropertyUnit(newitem.PropID, My.Application.ActiveSimulation.Options.SelectedUnitSystem)
            Me.dgv.Rows.Add(New Object() {kvp.Key, newitem.ObjID, newitem.PropID, newitem.ROnly, myobjname, propname & " (" & propunit & ")", propvalue})
        Next

        updating = False

    End Sub

    Public Sub UpdateList()

        updating = True

        Dim toremove As New ArrayList

        For Each r As DataGridViewRow In dgv.Rows
            Dim wi As WatchItem = items(r.Cells(0).Value)
            If My.Application.ActiveSimulation.Collections.ObjectCollection.ContainsKey(wi.ObjID) Then
                Dim myobjname As String = My.Application.ActiveSimulation.Collections.ObjectCollection(wi.ObjID).GraphicObject.Tag
                Dim propname As String = DWSIM.App.GetPropertyName(wi.PropID)
                Dim propvalue As Object = My.Application.ActiveSimulation.Collections.ObjectCollection(wi.ObjID).GetPropertyValue(wi.PropID, My.Application.ActiveSimulation.Options.SelectedUnitSystem)
                Dim propunit As String = My.Application.ActiveSimulation.Collections.ObjectCollection(wi.ObjID).GetPropertyUnit(wi.PropID, My.Application.ActiveSimulation.Options.SelectedUnitSystem)
                With r
                    .Cells(4).Value = myobjname
                    .Cells(5).Value = propname & " (" & propunit & ")"
                    .Cells(6).Value = propvalue
                End With
            Else
                toremove.Add(Me.dgv.Rows.IndexOf(r))
                items.Remove(r.Cells(0).Value)
            End If
        Next
        For Each r As Integer In toremove
            Me.dgv.Rows.RemoveAt(r)
        Next

        updating = False

    End Sub

    Private Sub btnAdd_Click(ByVal sender As System.Object, ByVal e As System.EventArgs) Handles btnAdd.Click

        Dim frmprop As New FormPropSelection

        frmprop.ssmode = False
        frmprop.ShowDialog(Me)

        Dim newitem As WatchItem = frmprop.wi

        If Not newitem Is Nothing Then
            Dim id As Integer = Date.Now.Year + Date.Now.Month + Date.Now.Day + Date.Now.Hour + Date.Now.Minute + Date.Now.Second + Date.Now.Millisecond
            Me.items.Add(id, newitem)
            Me.PopulateList()
        End If

    End Sub

    Private Sub dgv_CellValueChanged(ByVal sender As System.Object, ByVal e As System.Windows.Forms.DataGridViewCellEventArgs) Handles dgv.CellValueChanged

        If updating = False And loaded Then

            If e.ColumnIndex = 6 Then

                Dim wi As WatchItem = items(Me.dgv.Rows(e.RowIndex).Cells(0).Value)
                If My.Application.ActiveSimulation.Collections.ObjectCollection.ContainsKey(wi.ObjID) Then
                    Dim myobjname As String = My.Application.ActiveSimulation.Collections.ObjectCollection(wi.ObjID).GraphicObject.Tag
                    Dim propname As String = DWSIM.App.GetPropertyName(wi.PropID)
                    Dim propvalue As Object = Me.dgv.Rows(e.RowIndex).Cells(e.ColumnIndex).Value
                    My.Application.ActiveSimulation.Collections.ObjectCollection(wi.ObjID).SetPropertyValue(wi.PropID, propvalue, My.Application.ActiveSimulation.Options.SelectedUnitSystem)
                    DWSIM.Flowsheet.FlowsheetSolver.CalculateObject(My.Application.ActiveSimulation, wi.ObjID)
                    Me.dgv.Focus()
                End If

            End If

        End If

    End Sub

    Private Sub btnRemove_Click(ByVal sender As System.Object, ByVal e As System.EventArgs) Handles btnRemove.Click

        For i As Integer = 0 To Me.dgv.SelectedRows.Count - 1
            items.Remove(Me.dgv.SelectedRows(i).Cells(0).Value)
            Me.dgv.Rows.Remove(Me.dgv.SelectedRows(i))
        Next

    End Sub

    Private Sub ToolStripButton1_Click(ByVal sender As System.Object, ByVal e As System.EventArgs) Handles ToolStripButton1.Click
        UpdateList()
    End Sub

End Class