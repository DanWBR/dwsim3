Public Class FlowsheetUOViewerForm

    Public fsuo As DWSIM.SimulationObjects.UnitOps.Flowsheet

    Private Sub FlowsheetUOViewerForm_Load(sender As Object, e As EventArgs) Handles Me.Load

        fsuo.Fsheet.FormSurface.ChildParent = fsuo.Fsheet
        Me.Controls.Add(fsuo.Fsheet.dckPanel)
        fsuo.Fsheet.dckPanel.Invalidate()
        fsuo.Fsheet.FormSurface.FlowsheetDesignSurface.ZoomAll()
        fsuo.Fsheet.FormSurface.CMS_Sel.Enabled = False
        fsuo.Fsheet.FormSurface.TableLayoutPanel1.RowStyles(1).Height = 0
        Me.Invalidate()

    End Sub

End Class