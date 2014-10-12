Public Class frmObjListView

    Inherits WeifenLuo.WinFormsUI.Docking.DockContent

    Dim formc As FormFlowsheet

    Dim loaded As Boolean = False

    Private Sub frmObjListView_Load(ByVal sender As System.Object, ByVal e As System.EventArgs) Handles MyBase.Load

        With Me.DataGridView1
            .Rows.Clear()
            .Rows.Add(New Object() {"CorrentedeMatria", Me.ImageList2.Images(17), DWSIM.App.GetLocalString("CorrentedeMatria")})
            .Rows.Add(New Object() {"Correntedeenergia", Me.ImageList2.Images(18), DWSIM.App.GetLocalString("Correntedeenergia")})
            .Rows.Add(New Object() {"Misturador", Me.ImageList2.Images(3), DWSIM.App.GetLocalString("Misturador")})
            .Rows.Add(New Object() {"Divisor", Me.ImageList2.Images(4), DWSIM.App.GetLocalString("Divisor")})
            .Rows.Add(New Object() {"Resfriador", Me.ImageList2.Images(1), DWSIM.App.GetLocalString("Resfriador")})
            .Rows.Add(New Object() {"Aquecedor", Me.ImageList2.Images(2), DWSIM.App.GetLocalString("Aquecedor")})
            .Rows.Add(New Object() {"Tubulao", Me.ImageList2.Images(5), DWSIM.App.GetLocalString("Tubulao")})
            .Rows.Add(New Object() {"Vlvula", Me.ImageList2.Images(15), DWSIM.App.GetLocalString("Vlvula")})
            .Rows.Add(New Object() {"Bomba", Me.ImageList2.Images(6), DWSIM.App.GetLocalString("Bomba")})
            .Rows.Add(New Object() {"CompressorAdiabtico", Me.ImageList2.Images(0), DWSIM.App.GetLocalString("CompressorAdiabtico")})
            .Rows.Add(New Object() {"TurbinaAdiabtica", Me.ImageList2.Images(14), DWSIM.App.GetLocalString("TurbinaAdiabtica")})
            .Rows.Add(New Object() {"HeatExchanger", Me.ImageList2.Images(2), DWSIM.App.GetLocalString("HeatExchanger")})
            .Rows.Add(New Object() {"ShortcutColumn", Me.ImageList2.Images(16), DWSIM.App.GetLocalString("ShortcutColumn")})
            .Rows.Add(New Object() {"DistillationColumn", Me.ImageList2.Images(16), DWSIM.App.GetLocalString("DistillationColumn")})
            .Rows.Add(New Object() {"AbsorptionColumn", Me.ImageList2.Images(16), DWSIM.App.GetLocalString("AbsorptionColumn")})
            .Rows.Add(New Object() {"ReboiledAbsorber", Me.ImageList2.Images(16), DWSIM.App.GetLocalString("ReboiledAbsorber")})
            .Rows.Add(New Object() {"RefluxedAbsorber", Me.ImageList2.Images(16), DWSIM.App.GetLocalString("RefluxedAbsorber")})
            .Rows.Add(New Object() {"ComponentSeparator", Me.ImageList2.Images(22), DWSIM.App.GetLocalString("ComponentSeparator")})
            .Rows.Add(New Object() {"Tanque", Me.ImageList2.Images(13), DWSIM.App.GetLocalString("Tanque")})
            .Rows.Add(New Object() {"VasoSeparadorGL", Me.ImageList2.Images(16), DWSIM.App.GetLocalString("VasoSeparadorGL")})
            .Rows.Add(New Object() {"ReatorConversao", Me.ImageList2.Images(7), DWSIM.App.GetLocalString("ReatorConversao")})
            .Rows.Add(New Object() {"ReatorEquilibrio", Me.ImageList2.Images(9), DWSIM.App.GetLocalString("ReatorEquilibrio")})
            .Rows.Add(New Object() {"ReatorGibbs", Me.ImageList2.Images(10), DWSIM.App.GetLocalString("ReatorGibbs")})
            .Rows.Add(New Object() {"ReatorCSTR", Me.ImageList2.Images(8), DWSIM.App.GetLocalString("ReatorCSTR")})
            .Rows.Add(New Object() {"ReatorPFR", Me.ImageList2.Images(11), DWSIM.App.GetLocalString("ReatorPFR")})
            .Rows.Add(New Object() {"OrificePlate", Me.ImageList2.Images(23), DWSIM.App.GetLocalString("OrificePlate")})
            .Rows.Add(New Object() {"Ajuste", Me.ImageList2.Images(19), DWSIM.App.GetLocalString("Ajuste")})
            .Rows.Add(New Object() {"Especificao", Me.ImageList2.Images(20), DWSIM.App.GetLocalString("Especificao")})
            .Rows.Add(New Object() {"Reciclo", Me.ImageList2.Images(12), DWSIM.App.GetLocalString("Reciclo")})
            .Rows.Add(New Object() {"EnergyRecycle", Me.ImageList2.Images(21), DWSIM.App.GetLocalString("EnergyRecycle")})
            .Rows.Add(New Object() {"CustomUnitOp", Me.ImageList2.Images(24), DWSIM.App.GetLocalString("CustomUnitOp")})
            .Rows.Add(New Object() {"ExcelUnitOp", Me.ImageList2.Images(24), "Excel"})
            .Rows.Add(New Object() {"CapeOpenUnitOperation", Me.ImageList2.Images(25), DWSIM.App.GetLocalString("CapeOpenUnitOperation")})
            .Rows.Add(New Object() {"SolidsSeparator", Me.ImageList2.Images(22), DWSIM.App.GetLocalString("SolidsSeparator")})
            .Rows.Add(New Object() {"Filter", Me.ImageList2.Images(22), DWSIM.App.GetLocalString("Filter")})
            '.Sort(.Columns(2), System.ComponentModel.ListSortDirection.Ascending)
        End With

    End Sub

    Private Sub DataGridView1_MouseDown(ByVal sender As Object, ByVal e As System.Windows.Forms.MouseEventArgs) Handles DataGridView1.MouseDown
        Dim hit As DataGridView.HitTestInfo = Me.DataGridView1.HitTest(e.X, e.Y)
        Me.DataGridView1.DoDragDrop(Me.DataGridView1.Rows(hit.RowIndex), DragDropEffects.Copy)
    End Sub

End Class