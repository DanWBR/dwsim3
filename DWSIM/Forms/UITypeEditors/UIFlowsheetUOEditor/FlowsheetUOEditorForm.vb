Imports Microsoft.Msdn.Samples.GraphicObjects
Public Class FlowsheetUOEditorForm

    Public fsuo As DWSIM.SimulationObjects.UnitOps.Flowsheet

    Private loaded As Boolean = True

    Private Sub FlowsheetUOEditorForm_Load(sender As Object, e As EventArgs) Handles MyBase.Load

        If fsuo.Initialized Then
            btnInitialize.Enabled = False
            lblInit.Text = DWSIM.App.GetLocalString("FlowsheetInitialized")
            UpdateLinks()
        Else
            btnInitialize.Enabled = True
            lblInit.Text = DWSIM.App.GetLocalString("FlowsheetNotInitialized")
        End If

      
    End Sub

    Private Sub btnInitialize_Click(sender As Object, e As EventArgs) Handles btnInitialize.Click

        fsuo.InitializeFlowsheet(fsuo.SimulationFile)

        If fsuo.Initialized Then
            btnInitialize.Enabled = False
            lblInit.Text = DWSIM.App.GetLocalString("FlowsheetInitializationSuccess")
        Else
            btnInitialize.Enabled = True
            lblInit.Text = DWSIM.App.GetLocalString("FlowsheetInitializationError")
        End If

        UpdateLinks()

    End Sub

    Sub UpdateLinks()

        Dim i As Integer
        Dim connectedfrom, connectedto As String

        Dim cb As New DataGridViewComboBoxCell

        cb.Items.Add("")
        For Each ms As GraphicObject In fsuo.Fsheet.Collections.MaterialStreamCollection.Values
            cb.Items.Add(ms.Tag)
        Next

        dgvInputLinks.Columns(1).CellTemplate = cb
        dgvOutputLinks.Columns(1).CellTemplate = cb

        With dgvInputLinks.Rows
            .Clear()
            For i = 0 To 9
                If fsuo.GraphicObject.InputConnectors(i).IsAttached Then
                    connectedfrom = " (" & fsuo.GraphicObject.InputConnectors(i).AttachedConnector.AttachedFrom.Tag & ")"
                Else
                    connectedfrom = ""
                End If
                If fsuo.InputConnections(i) <> "" Then
                    Dim obj As GraphicObject = fsuo.FlowSheet.GetFlowsheetGraphicObject(fsuo.InputConnections(i))
                    If Not obj Is Nothing Then connectedto = obj.Tag Else connectedto = ""
                Else
                    connectedto = ""
                End If
                .Add(New Object() {DWSIM.App.GetLocalString("Correntedeentrada" & (i + 1).ToString) & connectedfrom, connectedto})
            Next
        End With

        With dgvOutputLinks.Rows
            .Clear()
            For i = 0 To 9
                If fsuo.GraphicObject.OutputConnectors(i).IsAttached Then
                    connectedto = " (" & fsuo.GraphicObject.OutputConnectors(i).AttachedConnector.AttachedTo.Tag & ")"
                Else
                    connectedto = ""
                End If
                If fsuo.OutputConnections(i) <> "" Then
                    Dim obj As GraphicObject = fsuo.FlowSheet.GetFlowsheetGraphicObject(fsuo.OutputConnections(i))
                    If Not obj Is Nothing Then connectedfrom = obj.Tag Else connectedfrom = ""
                Else
                    connectedfrom = ""
                End If
                .Add(New Object() {DWSIM.App.GetLocalString("Correntedesada" & (i + 1).ToString) & connectedto, connectedfrom})
            Next
        End With


    End Sub

    Private Sub dgv_CellValueChanged(ByVal sender As System.Object, ByVal e As System.Windows.Forms.DataGridViewCellEventArgs) Handles dgvInputLinks.CellValueChanged
        If e.RowIndex >= 0 And e.ColumnIndex = 1 Then
            Dim obj As GraphicObject = fsuo.FlowSheet.GetFlowsheetGraphicObject(dgvInputLinks.Rows(e.RowIndex).Cells(1).Value)
            If Not obj Is Nothing Then fsuo.InputConnections(e.RowIndex) = obj.Tag Else fsuo.InputConnections(e.RowIndex) = ""
        End If
    End Sub

    Private Sub dgv_CellValueChanged2(ByVal sender As System.Object, ByVal e As System.Windows.Forms.DataGridViewCellEventArgs) Handles dgvOutputLinks.CellValueChanged
        If e.RowIndex >= 0 And e.ColumnIndex = 1 Then
            Dim obj As GraphicObject = fsuo.FlowSheet.GetFlowsheetGraphicObject(dgvOutputLinks.Rows(e.RowIndex).Cells(1).Value)
            If Not obj Is Nothing Then fsuo.OutputConnections(e.RowIndex) = obj.Tag Else fsuo.OutputConnections(e.RowIndex) = ""
        End If
    End Sub

    Private Function ReturnProperties(ByVal objectTAG As String, ByVal dependent As Boolean) As String()

        For Each obj As SimulationObjects_BaseClass In fsuo.Fsheet.Collections.ObjectCollection.Values
            If objectTAG = obj.GraphicObject.Tag Then
                If dependent Then
                    Return obj.GetProperties(SimulationObjects_BaseClass.PropertyType.ALL)
                Else
                    Return obj.GetProperties(SimulationObjects_BaseClass.PropertyType.WR)
                End If
                Exit Function
            End If
        Next

        Return Nothing

    End Function

    Private Function ReturnObject(ByVal objectTAG As String) As SimulationObjects_BaseClass

        For Each obj As SimulationObjects_BaseClass In fsuo.Fsheet.Collections.ObjectCollection.Values
            If objectTAG = obj.GraphicObject.Tag Then
                Return obj
                Exit Function
            End If
        Next

        Return Nothing

    End Function

    Private Function ReturnPropertyID(ByVal objectID As String, ByVal propTAG As String) As String

        Dim props As String() = fsuo.Fsheet.Collections.ObjectCollection(objectID).GetProperties(SimulationObjects_BaseClass.PropertyType.ALL)
        For Each prop As String In props
            If DWSIM.App.GetPropertyName(prop) = propTAG Then
                Return prop
            End If
        Next

        Return Nothing

    End Function

End Class