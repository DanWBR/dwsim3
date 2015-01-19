Imports Microsoft.Msdn.Samples.GraphicObjects
Public Class FlowsheetUOEditorForm

    Public fsuo As DWSIM.SimulationObjects.UnitOps.Flowsheet

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


    End Sub

End Class