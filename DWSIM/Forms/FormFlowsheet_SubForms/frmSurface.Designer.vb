<Global.Microsoft.VisualBasic.CompilerServices.DesignerGenerated()> _
Partial Class frmSurface
    Inherits WeifenLuo.WinFormsUI.Docking.DockContent

    'Form overrides dispose to clean up the component list.
    <System.Diagnostics.DebuggerNonUserCode()> _
    Protected Overrides Sub Dispose(ByVal disposing As Boolean)
        If disposing AndAlso components IsNot Nothing Then
            components.Dispose()
        End If
        MyBase.Dispose(disposing)
    End Sub

    'Required by the Windows Form Designer
    Private components As System.ComponentModel.IContainer

    'NOTE: The following procedure is required by the Windows Form Designer
    'It can be modified using the Windows Form Designer.  
    'Do not modify it using the code editor.
    <System.Diagnostics.DebuggerStepThrough()> _
    Private Sub InitializeComponent()
        Me.components = New System.ComponentModel.Container()
        Dim resources As System.ComponentModel.ComponentResourceManager = New System.ComponentModel.ComponentResourceManager(GetType(frmSurface))
        Me.TableLayoutPanel1 = New System.Windows.Forms.TableLayoutPanel()
        Me.FlowsheetDesignSurface = New Microsoft.Msdn.Samples.DesignSurface.GraphicsSurface()
        Me.TableLayoutPanel2 = New System.Windows.Forms.TableLayoutPanel()
        Me.Panel2 = New System.Windows.Forms.Panel()
        Me.PictureBox3 = New System.Windows.Forms.PictureBox()
        Me.PictureBox4 = New System.Windows.Forms.PictureBox()
        Me.LabelCalculator = New System.Windows.Forms.Label()
        Me.LabelTime = New System.Windows.Forms.Label()
        Me.PanelSimultAdjust = New System.Windows.Forms.Panel()
        Me.LabelSimultAdjInfo = New System.Windows.Forms.Label()
        Me.PicSimultAdjust = New System.Windows.Forms.PictureBox()
        Me.LabelSimultAdjustStatus = New System.Windows.Forms.Label()
        Me.CMS_NoSel = New System.Windows.Forms.ContextMenuStrip(Me.components)
        Me.ToolStripMenuItem3 = New System.Windows.Forms.ToolStripMenuItem()
        Me.ToolStripSeparator1 = New System.Windows.Forms.ToolStripSeparator()
        Me.ToolStripMenuItem2 = New System.Windows.Forms.ToolStripMenuItem()
        Me.ToolStripMenuItem5 = New System.Windows.Forms.ToolStripMenuItem()
        Me.CopiarParaAÁreaDeTransferênciaToolStripMenuItem = New System.Windows.Forms.ToolStripMenuItem()
        Me.ToolStripMenuItem1 = New System.Windows.Forms.ToolStripMenuItem()
        Me.ToolStripMenuItem4 = New System.Windows.Forms.ToolStripMenuItem()
        Me.ToolStripMenuItem8 = New System.Windows.Forms.ToolStripMenuItem()
        Me.ToolStripMenuItem10 = New System.Windows.Forms.ToolStripMenuItem()
        Me.ExibirTudoToolStripMenuItem = New System.Windows.Forms.ToolStripMenuItem()
        Me.ZoomPadrão100ToolStripMenuItem = New System.Windows.Forms.ToolStripMenuItem()
        Me.CentralizarToolStripMenuItem = New System.Windows.Forms.ToolStripMenuItem()
        Me.CMS_Sel = New System.Windows.Forms.ContextMenuStrip(Me.components)
        Me.TSMI_Label = New System.Windows.Forms.ToolStripMenuItem()
        Me.ToolStripSeparator3 = New System.Windows.Forms.ToolStripSeparator()
        Me.RecalcularToolStripMenuItem = New System.Windows.Forms.ToolStripMenuItem()
        Me.EditCompTSMI = New System.Windows.Forms.ToolStripMenuItem()
        Me.CopyFromTSMI = New System.Windows.Forms.ToolStripMenuItem()
        Me.RestoreTSMI = New System.Windows.Forms.ToolStripMenuItem()
        Me.ToolStripSeparator6 = New System.Windows.Forms.ToolStripSeparator()
        Me.ConectarAToolStripMenuItem = New System.Windows.Forms.ToolStripMenuItem()
        Me.DesconectarDeToolStripMenuItem = New System.Windows.Forms.ToolStripMenuItem()
        Me.ToolStripSeparator4 = New System.Windows.Forms.ToolStripSeparator()
        Me.TSMI_Girar = New System.Windows.Forms.ToolStripMenuItem()
        Me.ToolStripMenuItem6 = New System.Windows.Forms.ToolStripMenuItem()
        Me.BToolStripMenuItem = New System.Windows.Forms.ToolStripMenuItem()
        Me.ToolStripMenuItem7 = New System.Windows.Forms.ToolStripMenuItem()
        Me.HorizontalmenteToolStripMenuItem = New System.Windows.Forms.ToolStripMenuItem()
        Me.ToolStripSeparator2 = New System.Windows.Forms.ToolStripSeparator()
        Me.ClonarToolStripMenuItem = New System.Windows.Forms.ToolStripMenuItem()
        Me.ExcluirToolStripMenuItem = New System.Windows.Forms.ToolStripMenuItem()
        Me.ToolStripSeparator5 = New System.Windows.Forms.ToolStripSeparator()
        Me.TabelaToolStripMenuItem = New System.Windows.Forms.ToolStripMenuItem()
        Me.MostrarToolStripMenuItem = New System.Windows.Forms.ToolStripMenuItem()
        Me.ConfigurarToolStripMenuItem = New System.Windows.Forms.ToolStripMenuItem()
        Me.ToolStripSeparator7 = New System.Windows.Forms.ToolStripSeparator()
        Me.CopiarDadosParaÁreaDeTransferênciaToolStripMenuItem = New System.Windows.Forms.ToolStripMenuItem()
        Me.PreviewDialog = New System.Windows.Forms.PrintPreviewDialog()
        Me.designSurfacePrintDocument = New System.Drawing.Printing.PrintDocument()
        Me.CMS_ItemsToConnect = New System.Windows.Forms.ContextMenuStrip(Me.components)
        Me.CMS_ItemsToDisconnect = New System.Windows.Forms.ContextMenuStrip(Me.components)
        Me.pageSetup = New System.Windows.Forms.PageSetupDialog()
        Me.setupPrint = New System.Windows.Forms.PrintDialog()
        Me.Timer1 = New System.Windows.Forms.Timer(Me.components)
        Me.Timer2 = New System.Windows.Forms.Timer(Me.components)
        Me.TableLayoutPanel1.SuspendLayout()
        Me.TableLayoutPanel2.SuspendLayout()
        Me.Panel2.SuspendLayout()
        CType(Me.PictureBox3, System.ComponentModel.ISupportInitialize).BeginInit()
        CType(Me.PictureBox4, System.ComponentModel.ISupportInitialize).BeginInit()
        Me.PanelSimultAdjust.SuspendLayout()
        CType(Me.PicSimultAdjust, System.ComponentModel.ISupportInitialize).BeginInit()
        Me.CMS_NoSel.SuspendLayout()
        Me.CMS_Sel.SuspendLayout()
        Me.SuspendLayout()
        '
        'TableLayoutPanel1
        '
        Try
            resources.ApplyResources(Me.TableLayoutPanel1, "TableLayoutPanel1")
        Catch ex As Exception

        End Try
        Me.TableLayoutPanel1.Controls.Add(Me.FlowsheetDesignSurface, 0, 0)
        Me.TableLayoutPanel1.Controls.Add(Me.TableLayoutPanel2, 0, 1)
        Me.TableLayoutPanel1.Name = "TableLayoutPanel1"
        '
        'FlowsheetDesignSurface
        '
        resources.ApplyResources(Me.FlowsheetDesignSurface, "FlowsheetDesignSurface")
        Me.FlowsheetDesignSurface.AllowDrop = True
        Me.FlowsheetDesignSurface.BackColor = System.Drawing.Color.White
        Me.FlowsheetDesignSurface.GridColor = System.Drawing.Color.GhostWhite
        Me.FlowsheetDesignSurface.GridLineWidth = 1
        Me.FlowsheetDesignSurface.GridSize = 25.0!
        Me.FlowsheetDesignSurface.MarginColor = System.Drawing.SystemColors.Control
        Me.FlowsheetDesignSurface.MarginLineWidth = 1
        Me.FlowsheetDesignSurface.MouseHoverSelect = False
        Me.FlowsheetDesignSurface.Name = "FlowsheetDesignSurface"
        Me.FlowsheetDesignSurface.NonPrintingAreaColor = System.Drawing.Color.LightGray
        Me.FlowsheetDesignSurface.QuickConnect = False
        Me.FlowsheetDesignSurface.SelectedObject = Nothing
        Me.FlowsheetDesignSurface.SelectRectangle = True
        Me.FlowsheetDesignSurface.ShowGrid = False
        Me.FlowsheetDesignSurface.SnapToGrid = False
        Me.FlowsheetDesignSurface.SurfaceBounds = New System.Drawing.Rectangle(0, 0, 10000, 7000)
        Me.FlowsheetDesignSurface.SurfaceMargins = New System.Drawing.Rectangle(0, 0, 10000, 7000)
        Me.FlowsheetDesignSurface.Zoom = 1.0!
        '
        'TableLayoutPanel2
        '
        Try
            resources.ApplyResources(Me.TableLayoutPanel2, "TableLayoutPanel2")
        Catch ex As Exception

        End Try
        Me.TableLayoutPanel2.Controls.Add(Me.Panel2, 0, 0)
        Me.TableLayoutPanel2.Controls.Add(Me.PanelSimultAdjust, 1, 0)
        Me.TableLayoutPanel2.Name = "TableLayoutPanel2"
        '
        'Panel2
        '
        resources.ApplyResources(Me.Panel2, "Panel2")
        Me.Panel2.Controls.Add(Me.PictureBox3)
        Me.Panel2.Controls.Add(Me.PictureBox4)
        Me.Panel2.Controls.Add(Me.LabelCalculator)
        Me.Panel2.Controls.Add(Me.LabelTime)
        Me.Panel2.Name = "Panel2"
        '
        'PictureBox3
        '
        resources.ApplyResources(Me.PictureBox3, "PictureBox3")
        Me.PictureBox3.Image = Global.DWSIM.My.Resources.Resources.tick
        Me.PictureBox3.Name = "PictureBox3"
        Me.PictureBox3.TabStop = False
        '
        'PictureBox4
        '
        resources.ApplyResources(Me.PictureBox4, "PictureBox4")
        Me.PictureBox4.Image = Global.DWSIM.My.Resources.Resources.clock
        Me.PictureBox4.Name = "PictureBox4"
        Me.PictureBox4.TabStop = False
        '
        'LabelCalculator
        '
        resources.ApplyResources(Me.LabelCalculator, "LabelCalculator")
        Me.LabelCalculator.ForeColor = System.Drawing.Color.DimGray
        Me.LabelCalculator.Name = "LabelCalculator"
        '
        'LabelTime
        '
        resources.ApplyResources(Me.LabelTime, "LabelTime")
        Me.LabelTime.ForeColor = System.Drawing.Color.DimGray
        Me.LabelTime.Name = "LabelTime"
        '
        'PanelSimultAdjust
        '
        resources.ApplyResources(Me.PanelSimultAdjust, "PanelSimultAdjust")
        Me.PanelSimultAdjust.Controls.Add(Me.LabelSimultAdjInfo)
        Me.PanelSimultAdjust.Controls.Add(Me.PicSimultAdjust)
        Me.PanelSimultAdjust.Controls.Add(Me.LabelSimultAdjustStatus)
        Me.PanelSimultAdjust.Name = "PanelSimultAdjust"
        '
        'LabelSimultAdjInfo
        '
        resources.ApplyResources(Me.LabelSimultAdjInfo, "LabelSimultAdjInfo")
        Me.LabelSimultAdjInfo.ForeColor = System.Drawing.Color.OrangeRed
        Me.LabelSimultAdjInfo.Name = "LabelSimultAdjInfo"
        '
        'PicSimultAdjust
        '
        resources.ApplyResources(Me.PicSimultAdjust, "PicSimultAdjust")
        Me.PicSimultAdjust.Image = Global.DWSIM.My.Resources.Resources.lightning1
        Me.PicSimultAdjust.Name = "PicSimultAdjust"
        Me.PicSimultAdjust.TabStop = False
        '
        'LabelSimultAdjustStatus
        '
        resources.ApplyResources(Me.LabelSimultAdjustStatus, "LabelSimultAdjustStatus")
        Me.LabelSimultAdjustStatus.ForeColor = System.Drawing.Color.OrangeRed
        Me.LabelSimultAdjustStatus.Name = "LabelSimultAdjustStatus"
        '
        'CMS_NoSel
        '
        resources.ApplyResources(Me.CMS_NoSel, "CMS_NoSel")
        Me.CMS_NoSel.Items.AddRange(New System.Windows.Forms.ToolStripItem() {Me.ToolStripMenuItem3, Me.ToolStripSeparator1, Me.ToolStripMenuItem2, Me.ToolStripMenuItem5, Me.CopiarParaAÁreaDeTransferênciaToolStripMenuItem, Me.ExibirTudoToolStripMenuItem, Me.ZoomPadrão100ToolStripMenuItem, Me.CentralizarToolStripMenuItem})
        Me.CMS_NoSel.Name = "ContextMenuStrip1"
        '
        'ToolStripMenuItem3
        '
        resources.ApplyResources(Me.ToolStripMenuItem3, "ToolStripMenuItem3")
        Me.ToolStripMenuItem3.Name = "ToolStripMenuItem3"
        '
        'ToolStripSeparator1
        '
        resources.ApplyResources(Me.ToolStripSeparator1, "ToolStripSeparator1")
        Me.ToolStripSeparator1.Name = "ToolStripSeparator1"
        '
        'ToolStripMenuItem2
        '
        resources.ApplyResources(Me.ToolStripMenuItem2, "ToolStripMenuItem2")
        Me.ToolStripMenuItem2.Image = Global.DWSIM.My.Resources.Resources.page_white_paint
        Me.ToolStripMenuItem2.Name = "ToolStripMenuItem2"
        '
        'ToolStripMenuItem5
        '
        resources.ApplyResources(Me.ToolStripMenuItem5, "ToolStripMenuItem5")
        Me.ToolStripMenuItem5.Image = Global.DWSIM.My.Resources.Resources.printer
        Me.ToolStripMenuItem5.Name = "ToolStripMenuItem5"
        '
        'CopiarParaAÁreaDeTransferênciaToolStripMenuItem
        '
        resources.ApplyResources(Me.CopiarParaAÁreaDeTransferênciaToolStripMenuItem, "CopiarParaAÁreaDeTransferênciaToolStripMenuItem")
        Me.CopiarParaAÁreaDeTransferênciaToolStripMenuItem.DropDownItems.AddRange(New System.Windows.Forms.ToolStripItem() {Me.ToolStripMenuItem1, Me.ToolStripMenuItem4, Me.ToolStripMenuItem8, Me.ToolStripMenuItem10})
        Me.CopiarParaAÁreaDeTransferênciaToolStripMenuItem.Image = Global.DWSIM.My.Resources.Resources.images
        Me.CopiarParaAÁreaDeTransferênciaToolStripMenuItem.Name = "CopiarParaAÁreaDeTransferênciaToolStripMenuItem"
        '
        'ToolStripMenuItem1
        '
        resources.ApplyResources(Me.ToolStripMenuItem1, "ToolStripMenuItem1")
        Me.ToolStripMenuItem1.Image = Global.DWSIM.My.Resources.Resources.zoom
        Me.ToolStripMenuItem1.Name = "ToolStripMenuItem1"
        '
        'ToolStripMenuItem4
        '
        resources.ApplyResources(Me.ToolStripMenuItem4, "ToolStripMenuItem4")
        Me.ToolStripMenuItem4.Image = Global.DWSIM.My.Resources.Resources.zoom
        Me.ToolStripMenuItem4.Name = "ToolStripMenuItem4"
        '
        'ToolStripMenuItem8
        '
        resources.ApplyResources(Me.ToolStripMenuItem8, "ToolStripMenuItem8")
        Me.ToolStripMenuItem8.Image = Global.DWSIM.My.Resources.Resources.zoom
        Me.ToolStripMenuItem8.Name = "ToolStripMenuItem8"
        '
        'ToolStripMenuItem10
        '
        resources.ApplyResources(Me.ToolStripMenuItem10, "ToolStripMenuItem10")
        Me.ToolStripMenuItem10.Image = Global.DWSIM.My.Resources.Resources.zoom
        Me.ToolStripMenuItem10.Name = "ToolStripMenuItem10"
        '
        'ExibirTudoToolStripMenuItem
        '
        resources.ApplyResources(Me.ExibirTudoToolStripMenuItem, "ExibirTudoToolStripMenuItem")
        Me.ExibirTudoToolStripMenuItem.Image = Global.DWSIM.My.Resources.Resources.zoom_extend
        Me.ExibirTudoToolStripMenuItem.Name = "ExibirTudoToolStripMenuItem"
        '
        'ZoomPadrão100ToolStripMenuItem
        '
        resources.ApplyResources(Me.ZoomPadrão100ToolStripMenuItem, "ZoomPadrão100ToolStripMenuItem")
        Me.ZoomPadrão100ToolStripMenuItem.Image = Global.DWSIM.My.Resources.Resources.zoom_refresh
        Me.ZoomPadrão100ToolStripMenuItem.Name = "ZoomPadrão100ToolStripMenuItem"
        '
        'CentralizarToolStripMenuItem
        '
        resources.ApplyResources(Me.CentralizarToolStripMenuItem, "CentralizarToolStripMenuItem")
        Me.CentralizarToolStripMenuItem.Image = Global.DWSIM.My.Resources.Resources.zoom
        Me.CentralizarToolStripMenuItem.Name = "CentralizarToolStripMenuItem"
        '
        'CMS_Sel
        '
        resources.ApplyResources(Me.CMS_Sel, "CMS_Sel")
        Me.CMS_Sel.Items.AddRange(New System.Windows.Forms.ToolStripItem() {Me.TSMI_Label, Me.ToolStripSeparator3, Me.RecalcularToolStripMenuItem, Me.EditCompTSMI, Me.CopyFromTSMI, Me.RestoreTSMI, Me.ToolStripSeparator6, Me.ConectarAToolStripMenuItem, Me.DesconectarDeToolStripMenuItem, Me.ToolStripSeparator4, Me.TSMI_Girar, Me.HorizontalmenteToolStripMenuItem, Me.ToolStripSeparator2, Me.ClonarToolStripMenuItem, Me.ExcluirToolStripMenuItem, Me.ToolStripSeparator5, Me.TabelaToolStripMenuItem, Me.ToolStripSeparator7, Me.CopiarDadosParaÁreaDeTransferênciaToolStripMenuItem})
        Me.CMS_Sel.Name = "CMS_Sel"
        '
        'TSMI_Label
        '
        resources.ApplyResources(Me.TSMI_Label, "TSMI_Label")
        Me.TSMI_Label.Name = "TSMI_Label"
        '
        'ToolStripSeparator3
        '
        resources.ApplyResources(Me.ToolStripSeparator3, "ToolStripSeparator3")
        Me.ToolStripSeparator3.Name = "ToolStripSeparator3"
        '
        'RecalcularToolStripMenuItem
        '
        resources.ApplyResources(Me.RecalcularToolStripMenuItem, "RecalcularToolStripMenuItem")
        Me.RecalcularToolStripMenuItem.Image = Global.DWSIM.My.Resources.Resources.arrow_refresh
        Me.RecalcularToolStripMenuItem.Name = "RecalcularToolStripMenuItem"
        '
        'EditCompTSMI
        '
        resources.ApplyResources(Me.EditCompTSMI, "EditCompTSMI")
        Me.EditCompTSMI.Image = Global.DWSIM.My.Resources.Resources.Lab_icon
        Me.EditCompTSMI.Name = "EditCompTSMI"
        '
        'CopyFromTSMI
        '
        resources.ApplyResources(Me.CopyFromTSMI, "CopyFromTSMI")
        Me.CopyFromTSMI.Image = Global.DWSIM.My.Resources.Resources.table_row_insert1
        Me.CopyFromTSMI.Name = "CopyFromTSMI"
        '
        'RestoreTSMI
        '
        resources.ApplyResources(Me.RestoreTSMI, "RestoreTSMI")
        Me.RestoreTSMI.Image = Global.DWSIM.My.Resources.Resources.undo_16
        Me.RestoreTSMI.Name = "RestoreTSMI"
        '
        'ToolStripSeparator6
        '
        resources.ApplyResources(Me.ToolStripSeparator6, "ToolStripSeparator6")
        Me.ToolStripSeparator6.Name = "ToolStripSeparator6"
        '
        'ConectarAToolStripMenuItem
        '
        resources.ApplyResources(Me.ConectarAToolStripMenuItem, "ConectarAToolStripMenuItem")
        Me.ConectarAToolStripMenuItem.Image = Global.DWSIM.My.Resources.Resources.connect
        Me.ConectarAToolStripMenuItem.Name = "ConectarAToolStripMenuItem"
        '
        'DesconectarDeToolStripMenuItem
        '
        resources.ApplyResources(Me.DesconectarDeToolStripMenuItem, "DesconectarDeToolStripMenuItem")
        Me.DesconectarDeToolStripMenuItem.Image = Global.DWSIM.My.Resources.Resources.disconnect
        Me.DesconectarDeToolStripMenuItem.Name = "DesconectarDeToolStripMenuItem"
        '
        'ToolStripSeparator4
        '
        resources.ApplyResources(Me.ToolStripSeparator4, "ToolStripSeparator4")
        Me.ToolStripSeparator4.Name = "ToolStripSeparator4"
        '
        'TSMI_Girar
        '
        resources.ApplyResources(Me.TSMI_Girar, "TSMI_Girar")
        Me.TSMI_Girar.DropDownItems.AddRange(New System.Windows.Forms.ToolStripItem() {Me.ToolStripMenuItem6, Me.BToolStripMenuItem, Me.ToolStripMenuItem7})
        Me.TSMI_Girar.Image = Global.DWSIM.My.Resources.Resources.arrow_rotate_clockwise
        Me.TSMI_Girar.Name = "TSMI_Girar"
        '
        'ToolStripMenuItem6
        '
        resources.ApplyResources(Me.ToolStripMenuItem6, "ToolStripMenuItem6")
        Me.ToolStripMenuItem6.DisplayStyle = System.Windows.Forms.ToolStripItemDisplayStyle.Text
        Me.ToolStripMenuItem6.Name = "ToolStripMenuItem6"
        '
        'BToolStripMenuItem
        '
        resources.ApplyResources(Me.BToolStripMenuItem, "BToolStripMenuItem")
        Me.BToolStripMenuItem.DisplayStyle = System.Windows.Forms.ToolStripItemDisplayStyle.Text
        Me.BToolStripMenuItem.Name = "BToolStripMenuItem"
        '
        'ToolStripMenuItem7
        '
        resources.ApplyResources(Me.ToolStripMenuItem7, "ToolStripMenuItem7")
        Me.ToolStripMenuItem7.DisplayStyle = System.Windows.Forms.ToolStripItemDisplayStyle.Text
        Me.ToolStripMenuItem7.Name = "ToolStripMenuItem7"
        '
        'HorizontalmenteToolStripMenuItem
        '
        resources.ApplyResources(Me.HorizontalmenteToolStripMenuItem, "HorizontalmenteToolStripMenuItem")
        Me.HorizontalmenteToolStripMenuItem.CheckOnClick = True
        Me.HorizontalmenteToolStripMenuItem.Image = Global.DWSIM.My.Resources.Resources.shape_flip_horizontal
        Me.HorizontalmenteToolStripMenuItem.Name = "HorizontalmenteToolStripMenuItem"
        '
        'ToolStripSeparator2
        '
        resources.ApplyResources(Me.ToolStripSeparator2, "ToolStripSeparator2")
        Me.ToolStripSeparator2.Name = "ToolStripSeparator2"
        '
        'ClonarToolStripMenuItem
        '
        resources.ApplyResources(Me.ClonarToolStripMenuItem, "ClonarToolStripMenuItem")
        Me.ClonarToolStripMenuItem.Image = Global.DWSIM.My.Resources.Resources.sheep
        Me.ClonarToolStripMenuItem.Name = "ClonarToolStripMenuItem"
        '
        'ExcluirToolStripMenuItem
        '
        resources.ApplyResources(Me.ExcluirToolStripMenuItem, "ExcluirToolStripMenuItem")
        Me.ExcluirToolStripMenuItem.Image = Global.DWSIM.My.Resources.Resources.cross
        Me.ExcluirToolStripMenuItem.Name = "ExcluirToolStripMenuItem"
        '
        'ToolStripSeparator5
        '
        resources.ApplyResources(Me.ToolStripSeparator5, "ToolStripSeparator5")
        Me.ToolStripSeparator5.Name = "ToolStripSeparator5"
        '
        'TabelaToolStripMenuItem
        '
        resources.ApplyResources(Me.TabelaToolStripMenuItem, "TabelaToolStripMenuItem")
        Me.TabelaToolStripMenuItem.DropDownItems.AddRange(New System.Windows.Forms.ToolStripItem() {Me.MostrarToolStripMenuItem, Me.ConfigurarToolStripMenuItem})
        Me.TabelaToolStripMenuItem.Image = Global.DWSIM.My.Resources.Resources.table
        Me.TabelaToolStripMenuItem.Name = "TabelaToolStripMenuItem"
        '
        'MostrarToolStripMenuItem
        '
        resources.ApplyResources(Me.MostrarToolStripMenuItem, "MostrarToolStripMenuItem")
        Me.MostrarToolStripMenuItem.CheckOnClick = True
        Me.MostrarToolStripMenuItem.Image = Global.DWSIM.My.Resources.Resources.table_go
        Me.MostrarToolStripMenuItem.Name = "MostrarToolStripMenuItem"
        '
        'ConfigurarToolStripMenuItem
        '
        resources.ApplyResources(Me.ConfigurarToolStripMenuItem, "ConfigurarToolStripMenuItem")
        Me.ConfigurarToolStripMenuItem.Image = Global.DWSIM.My.Resources.Resources.cog
        Me.ConfigurarToolStripMenuItem.Name = "ConfigurarToolStripMenuItem"
        '
        'ToolStripSeparator7
        '
        resources.ApplyResources(Me.ToolStripSeparator7, "ToolStripSeparator7")
        Me.ToolStripSeparator7.Name = "ToolStripSeparator7"
        '
        'CopiarDadosParaÁreaDeTransferênciaToolStripMenuItem
        '
        resources.ApplyResources(Me.CopiarDadosParaÁreaDeTransferênciaToolStripMenuItem, "CopiarDadosParaÁreaDeTransferênciaToolStripMenuItem")
        Me.CopiarDadosParaÁreaDeTransferênciaToolStripMenuItem.Image = Global.DWSIM.My.Resources.Resources.clipboard_sign
        Me.CopiarDadosParaÁreaDeTransferênciaToolStripMenuItem.Name = "CopiarDadosParaÁreaDeTransferênciaToolStripMenuItem"
        '
        'PreviewDialog
        '
        resources.ApplyResources(Me.PreviewDialog, "PreviewDialog")
        Me.PreviewDialog.Document = Me.designSurfacePrintDocument
        Me.PreviewDialog.Name = "PrintPreviewDialog1"
        Me.PreviewDialog.UseAntiAlias = True
        '
        'designSurfacePrintDocument
        '
        Me.designSurfacePrintDocument.DocumentName = "documento"
        '
        'CMS_ItemsToConnect
        '
        resources.ApplyResources(Me.CMS_ItemsToConnect, "CMS_ItemsToConnect")
        Me.CMS_ItemsToConnect.Name = "CMS_ItemsToConnect"
        '
        'CMS_ItemsToDisconnect
        '
        resources.ApplyResources(Me.CMS_ItemsToDisconnect, "CMS_ItemsToDisconnect")
        Me.CMS_ItemsToDisconnect.Name = "CMS_ItemsToConnect"
        '
        'pageSetup
        '
        Me.pageSetup.Document = Me.designSurfacePrintDocument
        '
        'setupPrint
        '
        Me.setupPrint.AllowCurrentPage = True
        Me.setupPrint.Document = Me.designSurfacePrintDocument
        Me.setupPrint.UseEXDialog = True
        '
        'Timer1
        '
        Me.Timer1.Enabled = True
        '
        'Timer2
        '
        '
        'frmSurface
        '
        resources.ApplyResources(Me, "$this")
        Me.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font
        Me.CloseButton = False
        Me.Controls.Add(Me.TableLayoutPanel1)
        Me.DoubleBuffered = True
        Me.Name = "frmSurface"
        Me.ShowHint = WeifenLuo.WinFormsUI.Docking.DockState.Document
        Me.TabText = Me.Text
        Me.TableLayoutPanel1.ResumeLayout(False)
        Me.TableLayoutPanel2.ResumeLayout(False)
        Me.Panel2.ResumeLayout(False)
        Me.Panel2.PerformLayout()
        CType(Me.PictureBox3, System.ComponentModel.ISupportInitialize).EndInit()
        CType(Me.PictureBox4, System.ComponentModel.ISupportInitialize).EndInit()
        Me.PanelSimultAdjust.ResumeLayout(False)
        CType(Me.PicSimultAdjust, System.ComponentModel.ISupportInitialize).EndInit()
        Me.CMS_NoSel.ResumeLayout(False)
        Me.CMS_Sel.ResumeLayout(False)
        Me.ResumeLayout(False)

    End Sub
    Public WithEvents TableLayoutPanel1 As System.Windows.Forms.TableLayoutPanel
    Public WithEvents FlowsheetDesignSurface As Microsoft.MSDN.Samples.DesignSurface.GraphicsSurface
    Public WithEvents CMS_NoSel As System.Windows.Forms.ContextMenuStrip
    Public WithEvents ToolStripMenuItem2 As System.Windows.Forms.ToolStripMenuItem
    Public WithEvents ToolStripMenuItem3 As System.Windows.Forms.ToolStripMenuItem
    Public WithEvents ToolStripMenuItem5 As System.Windows.Forms.ToolStripMenuItem
    Public WithEvents CMS_Sel As System.Windows.Forms.ContextMenuStrip
    Public WithEvents TSMI_Girar As System.Windows.Forms.ToolStripMenuItem
    Public WithEvents ToolStripMenuItem6 As System.Windows.Forms.ToolStripMenuItem
    Public WithEvents BToolStripMenuItem As System.Windows.Forms.ToolStripMenuItem
    Public WithEvents ToolStripMenuItem7 As System.Windows.Forms.ToolStripMenuItem
    Public WithEvents ToolStripSeparator2 As System.Windows.Forms.ToolStripSeparator
    Public WithEvents TSMI_Label As System.Windows.Forms.ToolStripMenuItem
    Public WithEvents TabelaToolStripMenuItem As System.Windows.Forms.ToolStripMenuItem
    Public WithEvents MostrarToolStripMenuItem As System.Windows.Forms.ToolStripMenuItem
    Public WithEvents ConfigurarToolStripMenuItem As System.Windows.Forms.ToolStripMenuItem
    Public WithEvents PreviewDialog As System.Windows.Forms.PrintPreviewDialog
    Public WithEvents designSurfacePrintDocument As System.Drawing.Printing.PrintDocument
    Public WithEvents pageSetup As System.Windows.Forms.PageSetupDialog
    Public WithEvents setupPrint As System.Windows.Forms.PrintDialog
    Public WithEvents ToolStripSeparator1 As System.Windows.Forms.ToolStripSeparator
    Public WithEvents Timer1 As System.Windows.Forms.Timer
    Public WithEvents TableLayoutPanel2 As System.Windows.Forms.TableLayoutPanel
    Public WithEvents ClonarToolStripMenuItem As System.Windows.Forms.ToolStripMenuItem
    Public WithEvents HorizontalmenteToolStripMenuItem As System.Windows.Forms.ToolStripMenuItem
    Public WithEvents ConectarAToolStripMenuItem As System.Windows.Forms.ToolStripMenuItem
    Public WithEvents DesconectarDeToolStripMenuItem As System.Windows.Forms.ToolStripMenuItem
    Public WithEvents ToolStripSeparator4 As System.Windows.Forms.ToolStripSeparator
    Public WithEvents CMS_ItemsToConnect As System.Windows.Forms.ContextMenuStrip
    Public WithEvents CMS_ItemsToDisconnect As System.Windows.Forms.ContextMenuStrip
    Public WithEvents ExcluirToolStripMenuItem As System.Windows.Forms.ToolStripMenuItem
    Public WithEvents ToolStripSeparator5 As System.Windows.Forms.ToolStripSeparator
    Public WithEvents ToolStripSeparator3 As System.Windows.Forms.ToolStripSeparator
    Public WithEvents Timer2 As System.Windows.Forms.Timer
    Public WithEvents RecalcularToolStripMenuItem As System.Windows.Forms.ToolStripMenuItem
    Public WithEvents ToolStripSeparator6 As System.Windows.Forms.ToolStripSeparator
    Friend WithEvents EditCompTSMI As System.Windows.Forms.ToolStripMenuItem
    Friend WithEvents CopiarParaAÁreaDeTransferênciaToolStripMenuItem As System.Windows.Forms.ToolStripMenuItem
    Friend WithEvents ToolStripMenuItem1 As System.Windows.Forms.ToolStripMenuItem
    Friend WithEvents ToolStripMenuItem4 As System.Windows.Forms.ToolStripMenuItem
    Friend WithEvents ToolStripMenuItem8 As System.Windows.Forms.ToolStripMenuItem
    Friend WithEvents ToolStripMenuItem10 As System.Windows.Forms.ToolStripMenuItem
    Public WithEvents Panel2 As System.Windows.Forms.Panel
    Public WithEvents PictureBox3 As System.Windows.Forms.PictureBox
    Public WithEvents PictureBox4 As System.Windows.Forms.PictureBox
    Public WithEvents LabelTime As System.Windows.Forms.Label
    Public WithEvents LabelCalculator As System.Windows.Forms.Label
    Public WithEvents PanelSimultAdjust As System.Windows.Forms.Panel
    Public WithEvents PicSimultAdjust As System.Windows.Forms.PictureBox
    Public WithEvents LabelSimultAdjustStatus As System.Windows.Forms.Label
    Public WithEvents LabelSimultAdjInfo As System.Windows.Forms.Label
    Friend WithEvents CopiarDadosParaÁreaDeTransferênciaToolStripMenuItem As System.Windows.Forms.ToolStripMenuItem
    Friend WithEvents ToolStripSeparator7 As System.Windows.Forms.ToolStripSeparator
    Friend WithEvents RestoreTSMI As System.Windows.Forms.ToolStripMenuItem
    Friend WithEvents CopyFromTSMI As System.Windows.Forms.ToolStripMenuItem
    Friend WithEvents ExibirTudoToolStripMenuItem As System.Windows.Forms.ToolStripMenuItem
    Friend WithEvents ZoomPadrão100ToolStripMenuItem As System.Windows.Forms.ToolStripMenuItem
    Friend WithEvents CentralizarToolStripMenuItem As System.Windows.Forms.ToolStripMenuItem
End Class
