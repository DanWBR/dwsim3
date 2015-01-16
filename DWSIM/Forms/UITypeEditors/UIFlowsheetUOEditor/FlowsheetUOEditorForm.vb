Public Class FlowsheetUOEditorForm

    Public fsuo As DWSIM.SimulationObjects.UnitOps.Flowsheet

    Private Sub FlowsheetUOEditorForm_Load(sender As Object, e As EventArgs) Handles MyBase.Load

        If fsuo.Initialized Then
            btnInitialize.Enabled = False
            lblInit.Text = DWSIM.App.GetLocalString("FlowsheetInitialized")
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

    End Sub

End Class