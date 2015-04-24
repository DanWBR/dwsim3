'    Copyright 2008-2015 Daniel Wagner O. de Medeiros
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

Imports Microsoft.Msdn.Samples.GraphicObjects
Imports System.Collections.Generic
Imports System.ComponentModel
Imports PropertyGridEx
Imports WeifenLuo.WinFormsUI
Imports System.Drawing
Imports System.Linq
Imports System.IO
Imports DWSIM.DWSIM.Flowsheet.FlowsheetSolver
Imports DWSIM.DWSIM.SimulationObjects.PropertyPackages
Imports Microsoft.Win32
Imports DWSIM.DWSIM.SimulationObjects
Imports DWSIM.DWSIM.ClassesBasicasTermodinamica
Imports System.Runtime.Serialization.Formatters.Binary
Imports DWSIM.DWSIM.FormClasses
Imports DWSIM.DWSIM.GraphicObjects
Imports DWSIM.DWSIM.Outros

<System.Serializable()> Public Class FormFlowsheet

    Inherits System.Windows.Forms.Form

    'CAPE-OPEN PME/COSE Interfaces
    Implements CapeOpen.ICapeCOSEUtilities, CapeOpen.ICapeMaterialTemplateSystem, CapeOpen.ICapeDiagnostic,  _
                CapeOpen.ICapeFlowsheetMonitoring, CapeOpen.ICapeSimulationContext, CapeOpen.ICapeIdentification

#Region "    Variable Declarations "

    Public Property MasterFlowsheet As FormFlowsheet = Nothing
    Public Property RedirectMessages As Boolean = False

    Public FrmStSim1 As New FormSimulSettings
    Public FrmPCBulk As New FormPCBulk
    Public FrmReport As New FormReportConfig

    Public m_IsLoadedFromFile As Boolean = False
    Public m_overrideCloseQuestion As Boolean = False
    Public m_simultadjustsolverenabled As Boolean = True

    Public FormSurface As New frmSurface
    Public FormProps As New frmProps
    Public FormObjList As New frmObjList
    Public FormLog As New frmLog
    Public FormMatList As New frmMatList
    Public FormObjListView As New frmObjListView
    Public FormSpreadsheet As New SpreadsheetForm

    Public FormOutput As New frmOutput
    Public FormQueue As New frmQueue
    Public FormCOReports As New frmCOReports
    Public FormWatch As New frmWatch

    Public FormCritPt As New FrmCritpt
    Public FormStabAn As New FrmStabAn
    Public FormHid As New FormHYD
    Public FormPE As New FormPhEnv
    Public FormLLEDiag As New FormLLEDiagram
    Public FormBE As New FormBinEnv
    Public FormPSVS As New FrmPsvSize
    Public FormVS As New FrmDAVP
    Public FormColdP As New FrmColdProperties

    Public FormSensAnalysis0 As New FormSensAnalysis
    Public FormOptimization0 As New FormOptimization

    Public FormCL As FormCLM

    Public WithEvents Options As New DWSIM.FormClasses.ClsFormOptions

    Public Conversor As New DWSIM.SistemasDeUnidades.Conversor

    Public CalculationQueue As Generic.Queue(Of DWSIM.Outros.StatusChangeEventArgs)

    Public FlowsheetStates As Dictionary(Of Date, FlowsheetState)

    Public PreviousSolutions As Dictionary(Of String, FlowsheetSolution)

    Public ScriptCollection As Dictionary(Of String, Script)

    Public CheckedToolstripButton As ToolStripButton
    Public ClickedToolStripMenuItem As ToolStripMenuItem
    Public InsertingObjectToPFD As Boolean = False

    Public prevcolor1, prevcolor2 As Color

    Public Collections As New DWSIM.FormClasses.ClsObjectCollections

    Public ID As String

#End Region

#Region "    Form Event Handlers "

    Public Sub New()

        ' This call is required by the Windows Form Designer.
        InitializeComponent()

        ' Add any initialization after the InitializeComponent() call.
        ID = Guid.NewGuid().ToString

    End Sub

    Private Sub FormChild_Activated(ByVal sender As Object, ByVal e As System.EventArgs) Handles Me.Activated
        My.Application.ActiveSimulation = Me
    End Sub

    Private Sub FormChild_Load(ByVal sender As System.Object, ByVal e As System.EventArgs) Handles MyBase.Load

        If DWSIM.App.IsRunningOnMono Then
            Me.FlowLayoutPanel1.AutoSize = False
            Me.FlowLayoutPanel1.Height = 50
            Me.MenuStrip1.Visible = False
        Else
            Me.MenuStrip1.Visible = False
        End If

        Dim rand As New Random
        Dim str As String = rand.Next(10000000, 99999999)

        Me.Options.BackupFileName = str & ".dwbcs"

        Me.CalculationQueue = New Generic.Queue(Of DWSIM.Outros.StatusChangeEventArgs)

        Me.StatusBarTextProvider1.InstanceStatusBar = My.Forms.FormMain.ToolStripStatusLabel1

        Me.TSTBZoom.Text = Format(Me.FormSurface.FlowsheetDesignSurface.Zoom, "#%")

        If Me.Options.CalculatorActivated Then
            Me.tsbAtivar.Checked = True
            Me.tsbDesat.Checked = False
        Else
            Me.tsbAtivar.Checked = False
            Me.tsbDesat.Checked = True
        End If

        If Me.ScriptCollection Is Nothing Then Me.ScriptCollection = New Dictionary(Of String, Script)

        If Not Me.m_IsLoadedFromFile Then

            If Not DWSIM.App.IsRunningOnMono Then
                Me.Options.SimAutor = My.User.Name
            Else
                Me.Options.SimAutor = "user"
            End If

            For Each pp As DWSIM.SimulationObjects.PropertyPackages.PropertyPackage In Me.Options.PropertyPackages.Values
                If pp.ConfigForm Is Nothing Then pp.ReconfigureConfigForm()
            Next

            Me.Options.NotSelectedComponents = New Dictionary(Of String, DWSIM.ClassesBasicasTermodinamica.ConstantProperties)

            Dim tmpc As DWSIM.ClassesBasicasTermodinamica.ConstantProperties
            For Each tmpc In FormMain.AvailableComponents.Values
                Dim newc As New DWSIM.ClassesBasicasTermodinamica.ConstantProperties
                newc = tmpc
                Me.Options.NotSelectedComponents.Add(tmpc.Name, newc)
            Next

            Dim Frm = ParentForm

            ' Set DockPanel properties
            dckPanel.ActiveAutoHideContent = Nothing
            dckPanel.Parent = Me

            'FormStatus.Show(dckPanel)
            FormLog.Show(dckPanel)
            FormObjListView.Show(dckPanel)
            FormObjList.Show(dckPanel)
            FormProps.Show(dckPanel)
            FormMatList.Show(dckPanel)
            FormSpreadsheet.Show(dckPanel)
            FormSurface.Show(dckPanel)

            If DWSIM.App.IsRunningOnMono Then
                'FormLog.DockState = Docking.DockState.DockRight
                Me.dckPanel.UpdateDockWindowZOrder(DockStyle.Fill, True)
            End If

            Try
                FormWatch.DockState = Docking.DockState.DockRight
                FormWatch.DockState = Docking.DockState.DockBottom
                FormCOReports.DockState = Docking.DockState.DockLeft
                FormCOReports.DockState = Docking.DockState.DockBottom
                FormOutput.DockState = Docking.DockState.DockLeft
                FormOutput.DockState = Docking.DockState.DockBottom
                FormQueue.DockState = Docking.DockState.DockRight
                FormQueue.DockState = Docking.DockState.DockBottom
            Catch ex As Exception

            End Try

            dckPanel.BringToFront()

            dckPanel.UpdateDockWindowZOrder(DockStyle.Fill, True)

            Me.FormSurface.FlowsheetDesignSurface.Zoom = 1
            Me.FormSurface.FlowsheetDesignSurface.VerticalScroll.Maximum = 7000
            Me.FormSurface.FlowsheetDesignSurface.VerticalScroll.Value = 3500
            Me.FormSurface.FlowsheetDesignSurface.HorizontalScroll.Maximum = 10000
            Me.FormSurface.FlowsheetDesignSurface.HorizontalScroll.Value = 5000

            Me.Invalidate()
            Application.DoEvents()

        Else

            If Me.Collections.AdjustCollection Is Nothing Then
                Me.Collections.AdjustCollection = New Dictionary(Of String, AdjustGraphic)
                Me.Collections.CLCS_AdjustCollection = New Dictionary(Of String, DWSIM.SimulationObjects.SpecialOps.Adjust)
                Me.Collections.SpecCollection = New Dictionary(Of String, SpecGraphic)
                Me.Collections.CLCS_SpecCollection = New Dictionary(Of String, DWSIM.SimulationObjects.SpecialOps.Spec)
                Me.Collections.RecycleCollection = New Dictionary(Of String, RecycleGraphic)
                Me.Collections.CLCS_RecycleCollection = New Dictionary(Of String, DWSIM.SimulationObjects.SpecialOps.Recycle)
            End If

            Dim node, node2 As TreeNode
            For Each node In Me.FormObjList.TreeViewObj.Nodes
                For Each node2 In node.Nodes
                    node2.ContextMenuStrip = Me.FormObjList.ContextMenuStrip1
                Next
            Next

        End If

        Dim array1(FormMain.AvailableUnitSystems.Count - 1) As String
        FormMain.AvailableUnitSystems.Keys.CopyTo(array1, 0)
        Me.ToolStripComboBoxUnitSystem.Items.Clear()
        Me.ToolStripComboBoxUnitSystem.Items.AddRange(array1)

        If Me.Options.SelectedUnitSystem.nome <> "" Then
            Me.ToolStripComboBoxUnitSystem.SelectedItem = Me.Options.SelectedUnitSystem.nome
        Else
            Me.ToolStripComboBoxUnitSystem.SelectedIndex = 0
        End If

        Me.ToolStripComboBoxNumberFormatting.SelectedItem = Me.Options.NumberFormat
        Me.ToolStripComboBoxNumberFractionFormatting.SelectedItem = Me.Options.FractionNumberFormat

        'load plugins
        CreatePluginsList()

    End Sub

    Public Sub FormChild_Shown(ByVal sender As Object, ByVal e As System.EventArgs) Handles Me.Shown

        If Not Me.m_IsLoadedFromFile Then

            Me.WindowState = FormWindowState.Maximized

            Application.DoEvents()

            If Not DWSIM.App.IsRunningOnMono Then
                Dim fw As New FormConfigWizard
                With fw
                    .StartPosition = FormStartPosition.CenterScreen
                    .WindowState = FormWindowState.Normal
                    .ShowDialog(Me)
                    If .switch Then
                        With Me.FrmStSim1
                            .WindowState = FormWindowState.Normal
                            .StartPosition = FormStartPosition.CenterScreen
                            .ShowDialog(Me)
                        End With
                    End If
                End With
            Else
                With Me.FrmStSim1
                    .WindowState = FormWindowState.Normal
                    .StartPosition = FormStartPosition.CenterScreen
                    .ShowDialog(Me)
                End With
            End If
        Else

            Dim array1(FormMain.AvailableUnitSystems.Count - 1) As String
            FormMain.AvailableUnitSystems.Keys.CopyTo(array1, 0)
            Me.ToolStripComboBoxUnitSystem.Items.Clear()
            Me.ToolStripComboBoxUnitSystem.Items.AddRange(array1)

            If Me.ToolStripComboBoxUnitSystem.Items.Contains(Me.Options.SelectedUnitSystem.nome) Then
                Me.ToolStripComboBoxUnitSystem.SelectedItem = Me.Options.SelectedUnitSystem.nome
            Else
                If Me.Options.SelectedUnitSystem.nome <> "" Then
                    If MessageBox.Show(DWSIM.App.GetLocalString("ConfirmAddUnitSystemFromSimulation"), DWSIM.App.GetLocalString("AddUnitSystemFromSimulation"), MessageBoxButtons.YesNo, MessageBoxIcon.Question) = Windows.Forms.DialogResult.Yes Then
                        AddUnitSystem(Me.Options.SelectedUnitSystem)
                    End If
                Else
                    Me.ToolStripComboBoxUnitSystem.SelectedIndex = 0
                    Me.ToolStripComboBoxUnitSystem.SelectedItem = Me.Options.SelectedUnitSystem.nome
                End If
            End If
            Me.ToolStripComboBoxNumberFormatting.SelectedItem = Me.Options.NumberFormat
            Me.ToolStripComboBoxNumberFractionFormatting.SelectedItem = Me.Options.FractionNumberFormat

        End If

        Me.FormLog.Grid1.Sort(Me.FormLog.Grid1.Columns(1), ListSortDirection.Descending)

        If DWSIM.App.IsRunningOnMono Then
            FormMain.ToolStripButton1.Enabled = True
            FormMain.SaveAllToolStripButton.Enabled = True
            FormMain.SaveToolStripButton.Enabled = True
            FormMain.SaveToolStripMenuItem.Enabled = True
            FormMain.SaveAllToolStripMenuItem.Enabled = True
            FormMain.SaveAsToolStripMenuItem.Enabled = True
            FormMain.ToolStripButton1.Enabled = True
            FormMain.CloseAllToolstripMenuItem.Enabled = True
        End If

        My.Application.ActiveSimulation = Me

        Me.ProcessScripts(Script.EventType.SimulationOpened, Script.ObjectType.Simulation)

        WriteToLog(DWSIM.App.GetLocalTipString("FLSH003"), Color.Black, TipoAviso.Dica)
        WriteToLog(DWSIM.App.GetLocalTipString("FLSH001"), Color.Black, TipoAviso.Dica)
        WriteToLog(DWSIM.App.GetLocalTipString("FLSH002"), Color.Black, TipoAviso.Dica)
        WriteToLog(DWSIM.App.GetLocalTipString("FLSH005"), Color.Black, TipoAviso.Dica)

    End Sub

    Private Sub FormChild2_FormClosed(ByVal sender As Object, ByVal e As System.Windows.Forms.FormClosedEventArgs) Handles Me.FormClosed

        Me.ProcessScripts(Script.EventType.SimulationClosed, Script.ObjectType.Simulation)

        If My.Application.ActiveSimulation Is Me Then
            My.Application.ActiveSimulation = Nothing
        End If

        'dispose objects
        For Each obj As SimulationObjects_BaseClass In Me.Collections.ObjectCollection.Values
            If obj.disposedValue = False Then obj.Dispose()
        Next

        Dim path As String = My.Settings.BackupFolder + System.IO.Path.DirectorySeparatorChar + Me.Options.BackupFileName

        If My.Settings.BackupFiles.Contains(path) Then
            My.Settings.BackupFiles.Remove(path)
            My.Settings.Save()
            Try
                If File.Exists(path) Then File.Delete(path)
            Catch ex As Exception
                My.Application.Log.WriteException(ex)
            End Try
        End If

        Dim cnt As Integer = FormMain.MdiChildren.Length

        If cnt = 0 Then

            FormMain.ToolStripButton1.Enabled = False
            FormMain.SaveAllToolStripButton.Enabled = False
            FormMain.SaveToolStripButton.Enabled = False
            FormMain.SaveToolStripMenuItem.Enabled = False
            FormMain.SaveAllToolStripMenuItem.Enabled = False
            FormMain.SaveAsToolStripMenuItem.Enabled = False
            FormMain.ToolStripButton1.Enabled = False

        Else

            FormMain.ToolStripButton1.Enabled = True
            FormMain.SaveAllToolStripButton.Enabled = True
            FormMain.SaveToolStripButton.Enabled = True
            FormMain.SaveToolStripMenuItem.Enabled = True
            FormMain.SaveAllToolStripMenuItem.Enabled = True
            FormMain.SaveAsToolStripMenuItem.Enabled = True
            FormMain.ToolStripButton1.Enabled = True

        End If

    End Sub

    Private Sub FormChild2_FormClosing(ByVal sender As Object, ByVal e As System.Windows.Forms.FormClosingEventArgs) Handles Me.FormClosing

        If FormMain.SairDiretoERRO Then Exit Sub

        If Me.m_overrideCloseQuestion = False Then

            Dim x = MessageBox.Show(DWSIM.App.GetLocalString("Desejasalvarasaltera"), DWSIM.App.GetLocalString("Fechando") & " " & Me.Options.SimNome & " (" & System.IO.Path.GetFileName(Me.Options.FilePath) & ") ...", MessageBoxButtons.YesNoCancel, MessageBoxIcon.Question)

            If x = MsgBoxResult.Yes Then

                FormMain.SaveFile(False)
                Me.m_overrideCloseQuestion = True
                Me.Close()

            ElseIf x = MsgBoxResult.Cancel Then

                e.Cancel = True

            Else

                Me.m_overrideCloseQuestion = True
                Me.Close()

            End If

        End If

    End Sub

#End Region

#Region "    Functions "

    Public Sub ProcessScripts(ByVal sourceevent As DWSIM.Outros.Script.EventType, ByVal sourceobj As DWSIM.Outros.Script.ObjectType, Optional ByVal sourceobjname As String = "")

        For Each scr As Script In Me.ScriptCollection.Values
            If scr.Linked And scr.LinkedEventType = sourceevent And scr.LinkedObjectType = sourceobj And scr.LinkedObjectName = sourceobjname Then
                If My.MyApplication.CommandLineMode Then
                    Console.WriteLine()
                    Console.WriteLine("Running script '" & scr.Title & "' for event '" & scr.LinkedEventType.ToString & "', linked to '" & scr.LinkedObjectName & "'...")
                    Console.WriteLine()
                Else
                    If scr.LinkedObjectName <> "" Then
                        Me.WriteToLog("Running script '" & scr.Title & "' for event '" & scr.LinkedEventType.ToString & "', linked to '" & scr.LinkedObjectName & "'...", Color.Blue, TipoAviso.Informacao)
                    Else
                        Me.WriteToLog("Running script '" & scr.Title & "' for event '" & scr.LinkedEventType.ToString & "'", Color.Blue, TipoAviso.Informacao)
                    End If
                End If
                FormScript.RunScript(scr.ScriptText, Me)
            End If
        Next

    End Sub

    Public Sub AddUnitSystem(ByVal su As DWSIM.SistemasDeUnidades.Unidades)

        If Not My.MyApplication.UserUnitSystems.ContainsKey(su.nome) Then
            My.MyApplication.UserUnitSystems.Add(su.nome, su)
            FormMain.AvailableUnitSystems.Add(su.nome, su)
            Me.FrmStSim1.ComboBox2.Items.Add(su.nome)
            Me.ToolStripComboBoxUnitSystem.Items.Add(su.nome)
        Else
            MessageBox.Show("Please input a different name for the unit system.")
        End If

    End Sub

    Public Sub AddComponentsRows(ByRef MaterialStream As DWSIM.SimulationObjects.Streams.MaterialStream)
        If Me.Options.SelectedComponents.Count = 0 Then
            MessageBox.Show(DWSIM.App.GetLocalString("Nohcomponentesaadici"))
        Else
            Dim comp As DWSIM.ClassesBasicasTermodinamica.ConstantProperties
            For Each phase As DWSIM.ClassesBasicasTermodinamica.Fase In MaterialStream.Fases.Values
                For Each comp In Me.Options.SelectedComponents.Values
                    With phase
                        .Componentes.Add(comp.Name, New DWSIM.ClassesBasicasTermodinamica.Substancia(comp.Name, ""))
                        .Componentes(comp.Name).ConstantProperties = comp
                    End With
                Next
            Next
        End If
    End Sub

    Public Function FT(ByRef prop As String, ByVal unit As String)
        Return prop & " (" & unit & ")"
    End Function

    Public Enum ID_Type
        Name
        Tag
    End Enum

    Public Shared Function SearchSurfaceObjectsByName(ByVal Name As String, ByVal Surface As Microsoft.Msdn.Samples.DesignSurface.GraphicsSurface) As GraphicObject

        Dim gObj As GraphicObject = Nothing
        Dim gObj2 As GraphicObject = Nothing
        For Each gObj In Surface.drawingObjects
            If gObj.Name.ToString = Name Then
                gObj2 = gObj
                Exit For
            End If
        Next
        Return gObj2

    End Function

    Public Shared Function SearchSurfaceObjectsByTag(ByVal Name As String, ByVal Surface As Microsoft.Msdn.Samples.DesignSurface.GraphicsSurface) As GraphicObject

        Dim gObj As GraphicObject = Nothing
        Dim gObj2 As GraphicObject = Nothing
        For Each gObj In Surface.drawingObjects
            If gObj.Tag.ToString = Name Then
                gObj2 = gObj
                Exit For
            End If
        Next
        Return gObj2

    End Function

    Public Function GetFlowsheetGraphicObject(ByVal tag As String) As GraphicObject

        Dim gObj As GraphicObject = Nothing
        Dim gObj2 As GraphicObject = Nothing
        For Each gObj In Me.FormSurface.FlowsheetDesignSurface.drawingObjects
            If gObj.Tag.ToString = tag Then
                gObj2 = gObj
                Exit For
            End If
        Next

        Return gObj2

    End Function

    Public Function GetFlowsheetSimulationObject(ByVal tag As String) As SimulationObjects_BaseClass

        For Each obj As SimulationObjects_BaseClass In Me.Collections.ObjectCollection.Values
            If obj.GraphicObject.Tag = tag Then
                Return obj
            End If
        Next

        Return Nothing

    End Function

    Public Function gscTogoc(ByVal X As Integer, ByVal Y As Integer) As Point
        Dim myNewPoint As Point
        myNewPoint.X = CInt((X - Me.FormSurface.FlowsheetDesignSurface.AutoScrollPosition.X) / Me.FormSurface.FlowsheetDesignSurface.Zoom)
        myNewPoint.Y = CInt((Y - Me.FormSurface.FlowsheetDesignSurface.AutoScrollPosition.Y) / Me.FormSurface.FlowsheetDesignSurface.Zoom)
        Return myNewPoint
    End Function

    Public Sub WriteToLog(ByVal texto As String, ByVal cor As Color, ByVal tipo As DWSIM.FormClasses.TipoAviso)

        Dim frsht As FormFlowsheet
        If Not Me.MasterFlowsheet Is Nothing And Me.RedirectMessages Then
            frsht = Me.MasterFlowsheet
        Else
            frsht = Me
        End If

        If frsht.Visible Then

            Me.UIThread(New System.Action(Sub()

                                              If Not My.MyApplication.CommandLineMode Then

                                                  Dim frlog = frsht.FormLog

                                                  Dim img As Bitmap
                                                  Dim strtipo As String
                                                  Select Case tipo
                                                      Case DWSIM.FormClasses.TipoAviso.Aviso
                                                          img = My.Resources._error
                                                          strtipo = DWSIM.App.GetLocalString("Aviso")
                                                      Case DWSIM.FormClasses.TipoAviso.Erro
                                                          img = My.Resources.exclamation
                                                          strtipo = DWSIM.App.GetLocalString("Erro")
                                                      Case DWSIM.FormClasses.TipoAviso.Dica
                                                          If Not My.Settings.ShowTips Then Exit Sub
                                                          img = My.Resources.lightbulb
                                                          strtipo = DWSIM.App.GetLocalString("Dica")
                                                      Case Else
                                                          img = My.Resources.information
                                                          strtipo = DWSIM.App.GetLocalString("Mensagem")
                                                  End Select

                                                  If frlog.GridDT.Columns.Count < 4 Then
                                                      frlog.GridDT.Columns.Add("Imagem", GetType(Bitmap))
                                                      frlog.GridDT.Columns.Add("Data")
                                                      frlog.GridDT.Columns.Add("Tipo")
                                                      frlog.GridDT.Columns.Add("Mensagem")
                                                      frlog.GridDT.Columns.Add("Cor", GetType(Color))
                                                      frlog.GridDT.Columns.Add("Indice")
                                                  ElseIf frlog.GridDT.Columns.Count = 4 Then
                                                      frlog.GridDT.Columns.Add("Cor", GetType(Color))
                                                      frlog.GridDT.Columns.Add("Indice")
                                                  ElseIf frlog.GridDT.Columns.Count = 5 Then
                                                      frlog.GridDT.Columns.Add("Indice")
                                                  End If
                                                  frlog.GridDT.PrimaryKey = New DataColumn() {frlog.GridDT.Columns("Indice")}
                                                  With frlog.GridDT.Columns("Indice")
                                                      .AutoIncrement = True
                                                      .AutoIncrementSeed = 1
                                                      .AutoIncrementStep = 1
                                                      .Unique = True
                                                  End With

                                                  frlog.GridDT.Rows.Add(New Object() {img, Date.Now, strtipo, texto, cor, frlog.GridDT.Rows.Count})

                                                  If DWSIM.App.IsRunningOnMono Then
                                                      frlog.Grid1.Rows.Add(New Object() {img, frlog.GridDT.Rows.Count, Date.Now, strtipo, texto})
                                                  End If

                                                  frlog.Grid1.Sort(frlog.Grid1.Columns(1), ListSortDirection.Descending)

                                              Else

                                                  If Not Me.FormCL Is Nothing Then
                                                      Me.FormCL.LBLogMsg.Items.Insert(0, Date.Now.ToString & " " & texto)
                                                  End If

                                              End If

                                          End Sub))

        End If

    End Sub

    Public Sub WriteMessage(ByVal message As String)
        WriteToLog(message, Color.Black, DWSIM.FormClasses.TipoAviso.Informacao)
    End Sub

    Public Sub UpdateStatusLabel(ByVal message As String)
        Me.FormSurface.LabelCalculator.Text = message
    End Sub

    Public Sub CheckCollections()

        'Creates all the graphic collections.
        If Collections.MixerCollection Is Nothing Then Collections.MixerCollection = New Dictionary(Of String, NodeInGraphic)
        If Collections.SplitterCollection Is Nothing Then Collections.SplitterCollection = New Dictionary(Of String, NodeOutGraphic)
        If Collections.MaterialStreamCollection Is Nothing Then Collections.MaterialStreamCollection = New Dictionary(Of String, MaterialStreamGraphic)
        If Collections.EnergyStreamCollection Is Nothing Then Collections.EnergyStreamCollection = New Dictionary(Of String, EnergyStreamGraphic)
        If Collections.PumpCollection Is Nothing Then Collections.PumpCollection = New Dictionary(Of String, PumpGraphic)
        If Collections.SeparatorCollection Is Nothing Then Collections.SeparatorCollection = New Dictionary(Of String, VesselGraphic)
        If Collections.CompressorCollection Is Nothing Then Collections.CompressorCollection = New Dictionary(Of String, CompressorGraphic)
        If Collections.PipeCollection Is Nothing Then Collections.PipeCollection = New Dictionary(Of String, PipeGraphic)
        If Collections.ValveCollection Is Nothing Then Collections.ValveCollection = New Dictionary(Of String, ValveGraphic)
        If Collections.CoolerCollection Is Nothing Then Collections.CoolerCollection = New Dictionary(Of String, CoolerGraphic)
        If Collections.HeaterCollection Is Nothing Then Collections.HeaterCollection = New Dictionary(Of String, HeaterGraphic)
        If Collections.TankCollection Is Nothing Then Collections.TankCollection = New Dictionary(Of String, TankGraphic)
        If Collections.ConnectorCollection Is Nothing Then Collections.ConnectorCollection = New Dictionary(Of String, ConnectorGraphic)
        If Collections.TPSeparatorCollection Is Nothing Then Collections.TPSeparatorCollection = New Dictionary(Of String, TPVesselGraphic)
        If Collections.TurbineCollection Is Nothing Then Collections.TurbineCollection = New Dictionary(Of String, TurbineGraphic)
        If Collections.MixerENCollection Is Nothing Then Collections.MixerENCollection = New Dictionary(Of String, NodeEnGraphic)
        If Collections.AdjustCollection Is Nothing Then Collections.AdjustCollection = New Dictionary(Of String, AdjustGraphic)
        If Collections.SpecCollection Is Nothing Then Collections.SpecCollection = New Dictionary(Of String, SpecGraphic)
        If Collections.RecycleCollection Is Nothing Then Collections.RecycleCollection = New Dictionary(Of String, RecycleGraphic)
        If Collections.ReactorConversionCollection Is Nothing Then Collections.ReactorConversionCollection = New Dictionary(Of String, ReactorConversionGraphic)
        If Collections.ReactorEquilibriumCollection Is Nothing Then Collections.ReactorEquilibriumCollection = New Dictionary(Of String, ReactorEquilibriumGraphic)
        If Collections.ReactorGibbsCollection Is Nothing Then Collections.ReactorGibbsCollection = New Dictionary(Of String, ReactorGibbsGraphic)
        If Collections.ReactorCSTRCollection Is Nothing Then Collections.ReactorCSTRCollection = New Dictionary(Of String, ReactorCSTRGraphic)
        If Collections.ReactorPFRCollection Is Nothing Then Collections.ReactorPFRCollection = New Dictionary(Of String, ReactorPFRGraphic)
        If Collections.HeatExchangerCollection Is Nothing Then Collections.HeatExchangerCollection = New Dictionary(Of String, HeatExchangerGraphic)
        If Collections.ShortcutColumnCollection Is Nothing Then Collections.ShortcutColumnCollection = New Dictionary(Of String, ShorcutColumnGraphic)
        If Collections.DistillationColumnCollection Is Nothing Then Collections.DistillationColumnCollection = New Dictionary(Of String, DistillationColumnGraphic)
        If Collections.AbsorptionColumnCollection Is Nothing Then Collections.AbsorptionColumnCollection = New Dictionary(Of String, AbsorptionColumnGraphic)
        If Collections.RefluxedAbsorberCollection Is Nothing Then Collections.RefluxedAbsorberCollection = New Dictionary(Of String, RefluxedAbsorberGraphic)
        If Collections.ReboiledAbsorberCollection Is Nothing Then Collections.ReboiledAbsorberCollection = New Dictionary(Of String, ReboiledAbsorberGraphic)
        If Collections.EnergyRecycleCollection Is Nothing Then Collections.EnergyRecycleCollection = New Dictionary(Of String, EnergyRecycleGraphic)
        If Collections.ComponentSeparatorCollection Is Nothing Then Collections.ComponentSeparatorCollection = New Dictionary(Of String, ComponentSeparatorGraphic)
        If Collections.OrificePlateCollection Is Nothing Then Collections.OrificePlateCollection = New Dictionary(Of String, OrificePlateGraphic)
        If Collections.CustomUOCollection Is Nothing Then Collections.CustomUOCollection = New Dictionary(Of String, CustomUOGraphic)
        If Collections.ExcelUOCollection Is Nothing Then Collections.ExcelUOCollection = New Dictionary(Of String, ExcelUOGraphic)
        If Collections.FlowsheetUOCollection Is Nothing Then Collections.FlowsheetUOCollection = New Dictionary(Of String, FlowsheetUOGraphic)
        If Collections.SolidsSeparatorCollection Is Nothing Then Collections.SolidsSeparatorCollection = New Dictionary(Of String, SolidSeparatorGraphic)
        If Collections.FilterCollection Is Nothing Then Collections.FilterCollection = New Dictionary(Of String, FilterGraphic)

        If Collections.ObjectCollection Is Nothing Then Collections.ObjectCollection = New Dictionary(Of String, SimulationObjects_BaseClass)

        'Creates all the actual unit operations collections.
        If Collections.CLCS_MaterialStreamCollection Is Nothing Then Collections.CLCS_MaterialStreamCollection = New Dictionary(Of String, DWSIM.SimulationObjects.Streams.MaterialStream)
        If Collections.CLCS_EnergyStreamCollection Is Nothing Then Collections.CLCS_EnergyStreamCollection = New Dictionary(Of String, DWSIM.SimulationObjects.Streams.EnergyStream)
        If Collections.CLCS_PipeCollection Is Nothing Then Collections.CLCS_PipeCollection = New Dictionary(Of String, DWSIM.SimulationObjects.UnitOps.Pipe)
        If Collections.CLCS_MixerCollection Is Nothing Then Collections.CLCS_MixerCollection = New Dictionary(Of String, DWSIM.SimulationObjects.UnitOps.Mixer)
        If Collections.CLCS_SplitterCollection Is Nothing Then Collections.CLCS_SplitterCollection = New Dictionary(Of String, DWSIM.SimulationObjects.UnitOps.Splitter)
        If Collections.CLCS_PumpCollection Is Nothing Then Collections.CLCS_PumpCollection = New Dictionary(Of String, DWSIM.SimulationObjects.UnitOps.Pump)
        If Collections.CLCS_CompressorCollection Is Nothing Then Collections.CLCS_CompressorCollection = New Dictionary(Of String, DWSIM.SimulationObjects.UnitOps.Compressor)
        If Collections.CLCS_ValveCollection Is Nothing Then Collections.CLCS_ValveCollection = New Dictionary(Of String, DWSIM.SimulationObjects.UnitOps.Valve)
        If Collections.CLCS_VesselCollection Is Nothing Then Collections.CLCS_VesselCollection = New Dictionary(Of String, DWSIM.SimulationObjects.UnitOps.Vessel)
        If Collections.CLCS_TurbineCollection Is Nothing Then Collections.CLCS_TurbineCollection = New Dictionary(Of String, DWSIM.SimulationObjects.UnitOps.Expander)
        If Collections.CLCS_EnergyMixerCollection Is Nothing Then Collections.CLCS_EnergyMixerCollection = New Dictionary(Of String, DWSIM.SimulationObjects.UnitOps.EnergyMixer)
        If Collections.CLCS_HeaterCollection Is Nothing Then Collections.CLCS_HeaterCollection = New Dictionary(Of String, DWSIM.SimulationObjects.UnitOps.Heater)
        If Collections.CLCS_CoolerCollection Is Nothing Then Collections.CLCS_CoolerCollection = New Dictionary(Of String, DWSIM.SimulationObjects.UnitOps.Cooler)
        If Collections.CLCS_TankCollection Is Nothing Then Collections.CLCS_TankCollection = New Dictionary(Of String, DWSIM.SimulationObjects.UnitOps.Tank)
        If Collections.CLCS_AdjustCollection Is Nothing Then Collections.CLCS_AdjustCollection = New Dictionary(Of String, DWSIM.SimulationObjects.SpecialOps.Adjust)
        If Collections.CLCS_SpecCollection Is Nothing Then Collections.CLCS_SpecCollection = New Dictionary(Of String, DWSIM.SimulationObjects.SpecialOps.Spec)
        If Collections.CLCS_RecycleCollection Is Nothing Then Collections.CLCS_RecycleCollection = New Dictionary(Of String, DWSIM.SimulationObjects.SpecialOps.Recycle)
        If Collections.CLCS_ReactorConversionCollection Is Nothing Then Collections.CLCS_ReactorConversionCollection = New Dictionary(Of String, DWSIM.SimulationObjects.Reactors.Reactor_Conversion)
        If Collections.CLCS_ReactorEquilibriumCollection Is Nothing Then Collections.CLCS_ReactorEquilibriumCollection = New Dictionary(Of String, DWSIM.SimulationObjects.Reactors.Reactor_Equilibrium)
        If Collections.CLCS_ReactorGibbsCollection Is Nothing Then Collections.CLCS_ReactorGibbsCollection = New Dictionary(Of String, DWSIM.SimulationObjects.Reactors.Reactor_Gibbs)
        If Collections.CLCS_ReactorCSTRCollection Is Nothing Then Collections.CLCS_ReactorCSTRCollection = New Dictionary(Of String, DWSIM.SimulationObjects.Reactors.Reactor_CSTR)
        If Collections.CLCS_ReactorPFRCollection Is Nothing Then Collections.CLCS_ReactorPFRCollection = New Dictionary(Of String, DWSIM.SimulationObjects.Reactors.Reactor_PFR)
        If Collections.CLCS_HeatExchangerCollection Is Nothing Then Collections.CLCS_HeatExchangerCollection = New Dictionary(Of String, DWSIM.SimulationObjects.UnitOps.HeatExchanger)
        If Collections.CLCS_ShortcutColumnCollection Is Nothing Then Collections.CLCS_ShortcutColumnCollection = New Dictionary(Of String, DWSIM.SimulationObjects.UnitOps.ShortcutColumn)
        If Collections.CLCS_DistillationColumnCollection Is Nothing Then Collections.CLCS_DistillationColumnCollection = New Dictionary(Of String, DWSIM.SimulationObjects.UnitOps.DistillationColumn)
        If Collections.CLCS_AbsorptionColumnCollection Is Nothing Then Collections.CLCS_AbsorptionColumnCollection = New Dictionary(Of String, DWSIM.SimulationObjects.UnitOps.AbsorptionColumn)
        If Collections.CLCS_ReboiledAbsorberCollection Is Nothing Then Collections.CLCS_ReboiledAbsorberCollection = New Dictionary(Of String, DWSIM.SimulationObjects.UnitOps.ReboiledAbsorber)
        If Collections.CLCS_RefluxedAbsorberCollection Is Nothing Then Collections.CLCS_RefluxedAbsorberCollection = New Dictionary(Of String, DWSIM.SimulationObjects.UnitOps.RefluxedAbsorber)
        If Collections.CLCS_EnergyRecycleCollection Is Nothing Then Collections.CLCS_EnergyRecycleCollection = New Dictionary(Of String, DWSIM.SimulationObjects.SpecialOps.EnergyRecycle)
        If Collections.CLCS_ComponentSeparatorCollection Is Nothing Then Collections.CLCS_ComponentSeparatorCollection = New Dictionary(Of String, DWSIM.SimulationObjects.UnitOps.ComponentSeparator)
        If Collections.CLCS_OrificePlateCollection Is Nothing Then Collections.CLCS_OrificePlateCollection = New Dictionary(Of String, DWSIM.SimulationObjects.UnitOps.OrificePlate)
        If Collections.CLCS_CustomUOCollection Is Nothing Then Collections.CLCS_CustomUOCollection = New Dictionary(Of String, DWSIM.SimulationObjects.UnitOps.CustomUO)
        If Collections.CLCS_ExcelUOCollection Is Nothing Then Collections.CLCS_ExcelUOCollection = New Dictionary(Of String, DWSIM.SimulationObjects.UnitOps.ExcelUO)
        If Collections.CLCS_SolidsSeparatorCollection Is Nothing Then Collections.CLCS_SolidsSeparatorCollection = New Dictionary(Of String, DWSIM.SimulationObjects.UnitOps.SolidsSeparator)
        If Collections.CLCS_FilterCollection Is Nothing Then Collections.CLCS_FilterCollection = New Dictionary(Of String, DWSIM.SimulationObjects.UnitOps.Filter)
        If Collections.CLCS_FlowsheetUOCollection Is Nothing Then Collections.CLCS_FlowsheetUOCollection = New Dictionary(Of String, DWSIM.SimulationObjects.UnitOps.Flowsheet)

        If Collections.OPT_SensAnalysisCollection Is Nothing Then Collections.OPT_SensAnalysisCollection = New List(Of DWSIM.Optimization.SensitivityAnalysisCase)
        If Collections.OPT_OptimizationCollection Is Nothing Then Collections.OPT_OptimizationCollection = New List(Of DWSIM.Optimization.OptimizationCase)

    End Sub

#End Region

#Region "    Click Event Handlers "
    Private Sub FormFlowsheet_HelpRequested(sender As System.Object, hlpevent As System.Windows.Forms.HelpEventArgs) Handles MyBase.HelpRequested

        Dim obj As GraphicObject = Me.FormSurface.FlowsheetDesignSurface.SelectedObject

        If obj Is Nothing Then
            DWSIM.App.HelpRequested("frame.htm")
        Else
            Select Case obj.TipoObjeto
                Case TipoObjeto.MaterialStream
                    DWSIM.App.HelpRequested("SO_Material_Stream.htm")
                Case TipoObjeto.EnergyStream
                    DWSIM.App.HelpRequested("SO_Energy_Stream.htm")
                Case TipoObjeto.NodeIn
                    DWSIM.App.HelpRequested("SO_Mixer.htm")
                Case TipoObjeto.NodeOut
                    DWSIM.App.HelpRequested("SO_Splitter.htm")
                Case TipoObjeto.Vessel
                    DWSIM.App.HelpRequested("SO_Separator.htm")
                Case TipoObjeto.Tank
                    DWSIM.App.HelpRequested("SO_Tank.htm")
                Case TipoObjeto.Pipe
                    DWSIM.App.HelpRequested("SO_Pipe_Segment.htm")
                Case TipoObjeto.Valve
                    DWSIM.App.HelpRequested("SO_Valve.htm")
                Case TipoObjeto.Pump
                    DWSIM.App.HelpRequested("SO_Pump.htm")
                Case TipoObjeto.Compressor
                    DWSIM.App.HelpRequested("SO_Compressor.htm")
                Case TipoObjeto.Expander
                    DWSIM.App.HelpRequested("SO_Expander.htm")
                Case TipoObjeto.Heater
                    DWSIM.App.HelpRequested("SO_Heater.htm")
                Case TipoObjeto.Cooler
                    DWSIM.App.HelpRequested("SO_Cooler.htm")
                Case TipoObjeto.HeatExchanger
                    DWSIM.App.HelpRequested("SO_Heatexchanger.htm")
                Case TipoObjeto.ShortcutColumn
                    DWSIM.App.HelpRequested("SO_Shortcut_Column.htm")
                Case TipoObjeto.DistillationColumn
                    DWSIM.App.HelpRequested("SO_Rigorous_Column.htm")
                Case TipoObjeto.AbsorptionColumn
                    DWSIM.App.HelpRequested("NoHelp.htm") 'no topic yet
                Case TipoObjeto.ReboiledAbsorber
                    DWSIM.App.HelpRequested("NoHelp.htm") 'no topic yet
                Case TipoObjeto.RefluxedAbsorber
                    DWSIM.App.HelpRequested("NoHelp.htm") 'no topic yet
                Case TipoObjeto.ComponentSeparator
                    DWSIM.App.HelpRequested("SO_CompSep.htm")
                Case TipoObjeto.OrificePlate
                    DWSIM.App.HelpRequested("SO_OrificePlate.htm")
                Case TipoObjeto.CustomUO
                    DWSIM.App.HelpRequested("SO_CustomUO.htm")
                Case TipoObjeto.ExcelUO
                    DWSIM.App.HelpRequested("SO_ExcelUO.htm")
                Case TipoObjeto.FlowsheetUO
                    DWSIM.App.HelpRequested("SO_FlowsheetUO.htm")
                Case TipoObjeto.SolidSeparator
                    DWSIM.App.HelpRequested("SO_SolidSeparator.htm")
                Case TipoObjeto.Filter
                    DWSIM.App.HelpRequested("SO_CakeFilter.htm")
                Case TipoObjeto.RCT_Conversion, TipoObjeto.RCT_CSTR, TipoObjeto.RCT_Equilibrium, TipoObjeto.RCT_Gibbs, TipoObjeto.RCT_PFR
                    DWSIM.App.HelpRequested("SO_Reactor.htm")
                Case TipoObjeto.OT_Reciclo, TipoObjeto.OT_EnergyRecycle
                    DWSIM.App.HelpRequested("SO_Recycle.htm")
                Case TipoObjeto.OT_Ajuste
                    DWSIM.App.HelpRequested("SO_Adjust.htm")
                Case TipoObjeto.OT_Especificacao
                    DWSIM.App.HelpRequested("SO_Specification.htm")
                Case Else
                    DWSIM.App.HelpRequested("frame.htm")
            End Select
        End If

    End Sub
    Private Sub InserObjectTSMIClick(ByVal sender As System.Object, ByVal e As EventArgs) Handles _
    TSMIAdjust.Click, TSMIColAbs.Click, TSMIColAbsCond.Click, TSMIColAbsReb.Click, TSMIColDist.Click, _
     TSMIColShortcut.Click, TSMIComponentSeparator.Click, TSMICompressor.Click, TSMICooler.Click, _
     TSMIEnergyRecycle.Click, TSMIEnergyStream.Click, TSMIExpander.Click, TSMIHeater.Click, _
     TSMIHeatExchanger.Click, TSMIMaterialStream.Click, TSMIMixer.Click, TSMIOrificePlate.Click, _
     TSMIPipe.Click, TSMIPump.Click, TSMIReactorConv.Click, TSMIReactorCSTR.Click, TSMIReactorEquilibrium.Click, _
     TSMIReactorGibbs.Click, TSMIReactorPFR.Click, TSMIRecycle.Click, TSMISeparator.Click, _
     TSMISpecification.Click, TSMISplitter.Click, TSMITank.Click, TSMIValve.Click, TSMICUO.Click, TSMICOUO.Click, _
     TSMISolidsSeparator.Click, TSMIFilter.Click, TSMIExcelUO.Click, TSMIFlowsheet.Click

        Me.InsertingObjectToPFD = True
        Me.FormSurface.FlowsheetDesignSurface.Cursor = Cursors.Hand

        Me.ClickedToolStripMenuItem = sender

    End Sub

    Private Sub InsertObjectButtonClick(ByVal sender As System.Object, ByVal e As System.EventArgs)

        If Not Me.CheckedToolstripButton Is Nothing Then
            Try
                Me.CheckedToolstripButton.Checked = False
            Catch ex As Exception

            End Try
        End If

        Me.CheckedToolstripButton = sender

        If Me.CheckedToolstripButton.Name = "TSBSelect" Then
            Me.InsertingObjectToPFD = False
            Me.FormSurface.FlowsheetDesignSurface.Cursor = Cursors.Default
        ElseIf Me.CheckedToolstripButton.Checked = True Then
            Me.InsertingObjectToPFD = True
            Me.FormSurface.FlowsheetDesignSurface.Cursor = Cursors.Hand
        Else
            Me.InsertingObjectToPFD = False
            Me.FormSurface.FlowsheetDesignSurface.Cursor = Cursors.Default
        End If

    End Sub

    Private Sub ToolStripButton6_Click_1(ByVal sender As System.Object, ByVal e As System.EventArgs) Handles ToolStripButton6.Click
        Me.FormSurface.pageSetup.ShowDialog()
    End Sub

    Private Sub ToolStripButton10_Click(ByVal sender As System.Object, ByVal e As System.EventArgs) Handles ToolStripButton10.Click
        Me.FormSurface.PreviewDialog.ShowDialog()
    End Sub

    Private Sub ToolStripButton11_Click(ByVal sender As System.Object, ByVal e As System.EventArgs) Handles ToolStripButton11.Click
        Me.FormSurface.setupPrint.ShowDialog()
    End Sub

    Private Sub TSBTexto_Click(ByVal sender As System.Object, ByVal e As System.EventArgs) Handles TSBTexto.Click
        Dim myTextObject As New TextGraphic(-Me.FormSurface.FlowsheetDesignSurface.AutoScrollPosition.X / Me.FormSurface.FlowsheetDesignSurface.Zoom + 30, _
            -Me.FormSurface.FlowsheetDesignSurface.AutoScrollPosition.Y / Me.FormSurface.FlowsheetDesignSurface.Zoom + 30, _
            DWSIM.App.GetLocalString("caixa_de_texto"), _
            System.Drawing.SystemFonts.DefaultFont, _
            Color.Black)
        Dim gObj As GraphicObject = Nothing
        gObj = myTextObject
        gObj.Name = "TEXT-" & Guid.NewGuid.ToString
        gObj.Tag = "TEXT" & ((From t As GraphicObject In Me.FormSurface.FlowsheetDesignSurface.drawingObjects Select t Where t.TipoObjeto = TipoObjeto.GO_Texto).Count + 1).ToString
        gObj.AutoSize = True
        gObj.TipoObjeto = TipoObjeto.GO_Texto
        Me.FormSurface.FlowsheetDesignSurface.drawingObjects.Add(gObj)
        Me.FormSurface.FlowsheetDesignSurface.Invalidate()

    End Sub

    Private Sub ToolStripButton19_Click(ByVal sender As System.Object, ByVal e As System.EventArgs) Handles ToolStripButton19.Click
        Dim myMasterTable As New DWSIM.GraphicObjects.MasterTableGraphic(-Me.FormSurface.FlowsheetDesignSurface.AutoScrollPosition.X / Me.FormSurface.FlowsheetDesignSurface.Zoom + 30, _
           -Me.FormSurface.FlowsheetDesignSurface.AutoScrollPosition.Y / Me.FormSurface.FlowsheetDesignSurface.Zoom + 30)
        Dim gObj As GraphicObject = Nothing
        gObj = myMasterTable
        gObj.Tag = "MASTERTABLE-" & Guid.NewGuid.ToString
        gObj.AutoSize = True
        gObj.TipoObjeto = TipoObjeto.GO_MasterTable
        Me.FormSurface.FlowsheetDesignSurface.drawingObjects.Add(gObj)
        Me.FormSurface.FlowsheetDesignSurface.Invalidate()
    End Sub

    Private Sub ToolStripButton4_Click(sender As Object, e As EventArgs) Handles ToolStripButton4.Click
        Dim mySpreadsheetTable As New DWSIM.GraphicObjects.SpreadsheetTableGraphic(
            Me.FormSpreadsheet,
            -Me.FormSurface.FlowsheetDesignSurface.AutoScrollPosition.X / Me.FormSurface.FlowsheetDesignSurface.Zoom + 30,
            -Me.FormSurface.FlowsheetDesignSurface.AutoScrollPosition.Y / Me.FormSurface.FlowsheetDesignSurface.Zoom + 30)
        Dim gObj As GraphicObject = Nothing
        gObj = mySpreadsheetTable
        gObj.Tag = "SHEETTABLE-" & Guid.NewGuid.ToString
        gObj.AutoSize = True
        gObj.TipoObjeto = TipoObjeto.GO_SpreadsheetTable
        Me.FormSurface.FlowsheetDesignSurface.drawingObjects.Add(gObj)
        Me.FormSurface.FlowsheetDesignSurface.Invalidate()
    End Sub

    Private Sub TSBtabela_Click(ByVal sender As System.Object, ByVal e As System.EventArgs) Handles TSBtabela.Click
        With Me.OpenFileName
            .CheckFileExists = True
            .CheckPathExists = True
            .Title = DWSIM.App.GetLocalString("Adicionarfigura")
            .Filter = "Images|*.bmp;*.jpg;*.png;*.gif"
            .AddExtension = True
            .Multiselect = False
            .RestoreDirectory = True
            Dim res As DialogResult = .ShowDialog
            If res = Windows.Forms.DialogResult.OK Then
                Dim img = System.Drawing.Image.FromFile(.FileName)
                Dim gObj As GraphicObject = Nothing
                If Not img Is Nothing Then
                    Dim myEmbeddedImage As New EmbeddedImageGraphic(-Me.FormSurface.FlowsheetDesignSurface.AutoScrollPosition.X / Me.FormSurface.FlowsheetDesignSurface.Zoom, _
                                    -Me.FormSurface.FlowsheetDesignSurface.AutoScrollPosition.Y / Me.FormSurface.FlowsheetDesignSurface.Zoom, img)
                    gObj = myEmbeddedImage
                    gObj.Tag = DWSIM.App.GetLocalString("FIGURA") & Guid.NewGuid.ToString
                    gObj.AutoSize = True
                End If
                Me.FormSurface.FlowsheetDesignSurface.drawingObjects.Add(gObj)
                Me.FormSurface.FlowsheetDesignSurface.Invalidate()
            End If
        End With
        Me.TSBtabela.Checked = False
    End Sub

    Private Sub PropriedadesDosComponentesToolStripMenuItem_Click(ByVal sender As System.Object, ByVal e As System.EventArgs) Handles PropriedadesDosComponentesToolStripMenuItem.Click
        Dim frmpc As New FormPureComp
        frmpc.ShowDialog(Me)
    End Sub

    Private Sub PontoCríticoRealToolStripMenuItem_Click(ByVal sender As System.Object, ByVal e As System.EventArgs) Handles PontoCríticoRealToolStripMenuItem.Click
        Me.FormCritPt.ShowDialog(Me)
    End Sub

    Private Sub DiagramaDeFasesToolStripMenuItem_Click(ByVal sender As System.Object, ByVal e As System.EventArgs) Handles DiagramaDeFasesToolStripMenuItem.Click
        Me.FormStabAn.ShowDialog(Me)
    End Sub

    Private Sub tsbAtivar_Click(ByVal sender As Object, ByVal e As System.EventArgs) Handles tsbAtivar.Click
       Me.tsbDesat.Checked = False
        Me.tsbAtivar.Checked = True
        Me.Options.CalculatorActivated = True
        Me.FormSurface.LabelCalculator.Text = DWSIM.App.GetLocalString("CalculadorOcioso")
        Me.WriteToLog(DWSIM.App.GetLocalString("Calculadorativado"), Color.DimGray, DWSIM.FormClasses.TipoAviso.Informacao)
        If Not Me.CalculationQueue Is Nothing Then
            Me.CalculationQueue.Clear()
            'If Me.CalculationQueue.Count >= 1 Then
            'Dim msgres As MsgBoxResult = MessageBox.Show(DWSIM.App.GetLocalString("Existemobjetosespera"), _
            'DWSIM.App.GetLocalString("Objetosnafiladeclcul"), _
            'MessageBoxButtons.YesNo, MessageBoxIcon.Question)
            'If msgres = MsgBoxResult.Yes Then
            '    ProcessCalculationQueue(Me)
            'End If
            'End If
        End If
    End Sub

    Private Sub tsbDesat_Click(ByVal sender As System.Object, ByVal e As System.EventArgs) Handles tsbDesat.Click
        'If Me.tsbDesat.Checked = True Then
        Me.tsbAtivar.Checked = False
        Me.tsbDesat.Checked = True
        Me.Options.CalculatorActivated = False
        Me.FormSurface.LabelCalculator.Text = DWSIM.App.GetLocalString("CalculadorDesativado1")
        'Else
        'Me.tsbAtivar.Checked = True
        'Me.Options.CalculatorActivated = True
        'Me.FormSurface.LabelSimMode.Text = DWSIM.App.GetLocalString("CalculadorOcioso")
        'End If
    End Sub

    Private Sub HYDVerificaçãoDasCondiçõesDeFormaçãoDeHidratosToolStripMenuItem_Click(ByVal sender As System.Object, ByVal e As System.EventArgs) Handles HYDVerificaçãoDasCondiçõesDeFormaçãoDeHidratosToolStripMenuItem.Click
        Me.FormHid = New FormHYD
        Me.FormHid.Show(Me.dckPanel)
    End Sub

    Private Sub DiagramaDeFasesToolStripMenuItem1_Click(ByVal sender As System.Object, ByVal e As System.EventArgs) Handles DiagramaDeFasesToolStripMenuItem1.Click
        Me.FormPE = New FormPhEnv
        Me.FormPE.Show(Me.dckPanel)
    End Sub
    Private Sub LLEDiagramToolStripMenuItem_Click(sender As System.Object, e As System.EventArgs) Handles LLEDiagramToolStripMenuItem.Click
        Me.FormLLEDiag = New FormLLEDiagram
        Me.FormLLEDiag.Show(Me.dckPanel)
    End Sub
    Private Sub DiagramaBinárioToolStripMenuItem_Click(ByVal sender As System.Object, ByVal e As System.EventArgs) Handles DiagramaBinárioToolStripMenuItem.Click
        Me.FormBE = New FormBinEnv
        Me.FormBE.Show(Me.dckPanel)
    End Sub
    Private Sub FecharSimulaçãoAtualToolStripMenuItem_Click(ByVal sender As System.Object, ByVal e As System.EventArgs) Handles CloseToolStripMenuItem.Click
        Me.Close()
    End Sub

    Private Sub PSVSizingToolStripMenuItem_Click(ByVal sender As System.Object, ByVal e As System.EventArgs) Handles PSVSizingToolStripMenuItem.Click
        Me.FormPSVS.ShowDialog(Me)
    End Sub

    Private Sub FlashVesselSizingToolStripMenuItem_Click(ByVal sender As System.Object, ByVal e As System.EventArgs) Handles FlashVesselSizingToolStripMenuItem.Click
        Me.FormVS.ShowDialog(Me)
    End Sub

    Private Sub PropriedadesDePetróleosToolStripMenuItem_Click(ByVal sender As System.Object, ByVal e As System.EventArgs) Handles PropriedadesDePetróleosToolStripMenuItem.Click
        Me.FormColdP = New FrmColdProperties
        Me.FormColdP.Show(Me.dckPanel)
    End Sub

    Private Sub ToolStripButton14_Click(ByVal sender As System.Object, ByVal e As System.EventArgs) Handles ToolStripButton14.Click
        My.MyApplication.CalculatorStopRequested = True
        If My.MyApplication.TaskCancellationTokenSource IsNot Nothing Then
            My.MyApplication.TaskCancellationTokenSource.Cancel()
        End If
    End Sub

    Private Sub ToolStripButton13_Click(ByVal sender As System.Object, ByVal e As System.EventArgs) Handles ToolStripButton13.Click
        If My.Computer.Keyboard.ShiftKeyDown Then
            CalculateAll(Me)
        Else
            CalculateAll2(Me, My.Settings.SolverMode)
        End If
    End Sub

    Private Sub ToolStripButton15_Click(ByVal sender As System.Object, ByVal e As System.EventArgs) Handles ToolStripButton15.Click
        Me.CalculationQueue.Clear()
    End Sub

    Private Sub AnáliseDeSensibilidadeToolStripMenuItem_Click(ByVal sender As System.Object, ByVal e As System.EventArgs) Handles AnáliseDeSensibilidadeToolStripMenuItem.Click
        Me.FormSensAnalysis0 = New FormSensAnalysis
        Me.FormSensAnalysis0.Show(Me.dckPanel)
    End Sub

    Private Sub MultivariateOptimizerToolStripMenuItem_Click(ByVal sender As System.Object, ByVal e As System.EventArgs) Handles MultivariateOptimizerToolStripMenuItem.Click
        Me.FormOptimization0 = New FormOptimization
        Me.FormOptimization0.Show(Me.dckPanel)
    End Sub

    Private Sub GerenciadorDeReaçõesToolStripMenuItem_Click(ByVal sender As System.Object, ByVal e As System.EventArgs) Handles GerenciadorDeReaçõesToolStripMenuItem.Click
        Dim rm As New FormReacManager
        rm.ShowDialog()
    End Sub

    Private Sub CaracterizaçãoDePetróleosFraçõesC7ToolStripMenuItem_Click(ByVal sender As System.Object, ByVal e As System.EventArgs) Handles CaracterizaçãoDePetróleosFraçõesC7ToolStripMenuItem.Click
        Me.FrmPCBulk.ShowDialog(Me)
    End Sub

    Private Sub CaracterizaçãoDePetróleosCurvasDeDestilaçãoToolStripMenuItem_Click(ByVal sender As System.Object, ByVal e As System.EventArgs) Handles CaracterizaçãoDePetróleosCurvasDeDestilaçãoToolStripMenuItem.Click
        Dim frmdc As New DCCharacterizationWizard
        frmdc.ShowDialog(Me)
    End Sub

    Private Sub ToolStripComboBoxNumberFormatting_SelectedIndexChanged(ByVal sender As Object, ByVal e As System.EventArgs) Handles ToolStripComboBoxNumberFormatting.SelectedIndexChanged
        Me.Options.NumberFormat = Me.ToolStripComboBoxNumberFormatting.SelectedItem
        Try
            Me.FormSurface.UpdateSelectedObject()
        Catch ex As Exception

        End Try
    End Sub

    Private Sub ToolStripComboBoxNumberFractionFormatting_Click(ByVal sender As System.Object, ByVal e As System.EventArgs) Handles ToolStripComboBoxNumberFractionFormatting.SelectedIndexChanged
        Me.Options.FractionNumberFormat = Me.ToolStripComboBoxNumberFractionFormatting.SelectedItem
        Try
            Me.FormSurface.UpdateSelectedObject()
        Catch ex As Exception

        End Try
    End Sub

    Private Sub ToolStripComboBoxUnitSystem_Click(ByVal sender As System.Object, ByVal e As System.EventArgs) Handles ToolStripComboBoxUnitSystem.SelectedIndexChanged

        Try

            If FormMain.AvailableUnitSystems.ContainsKey(Me.ToolStripComboBoxUnitSystem.SelectedItem.ToString) Then
                Me.Options.SelectedUnitSystem = FormMain.AvailableUnitSystems.Item(Me.ToolStripComboBoxUnitSystem.SelectedItem.ToString)
            End If

            Me.FormSurface.UpdateSelectedObject()

            For Each o In Collections.ObjectCollection.Values
                o.UpdatePropertyNodes(Me.Options.SelectedUnitSystem, Me.Options.NumberFormat)
            Next

        Catch ex As Exception

        End Try

    End Sub

    Private Sub ToolStripButton7_Click(ByVal sender As System.Object, ByVal e As System.EventArgs) Handles ToolStripButton7.Click
        Dim frmUnit As New FormUnitGen
        frmUnit.ShowDialog(Me)
    End Sub

    Private Sub IronRubyToolStripMenuItem_Click(ByVal sender As System.Object, ByVal e As System.EventArgs) Handles IronRubyToolStripMenuItem.Click
        Dim fs As New FormScript
        fs.fc = Me
        fs.Show(Me.dckPanel)
    End Sub

    Private Sub ExibirSaídaDoConsoleToolStripMenuItem_CheckedChanged(ByVal sender As Object, ByVal e As System.EventArgs) Handles ExibirSaídaDoConsoleToolStripMenuItem.CheckedChanged
        If ExibirSaídaDoConsoleToolStripMenuItem.Checked Then
            FormOutput.Show(dckPanel)
        Else
            FormOutput.Hide()
        End If
    End Sub

    Private Sub ExibirListaDeItensACalcularToolStripMenuItem_CheckedChanged(ByVal sender As Object, ByVal e As System.EventArgs) Handles ExibirListaDeItensACalcularToolStripMenuItem.CheckedChanged
        If ExibirListaDeItensACalcularToolStripMenuItem.Checked Then
            FormQueue.Show(dckPanel)
        Else
            FormQueue.Hide()
        End If
    End Sub

    Private Sub ExibirRelatóriosDosObjetosCAPEOPENToolStripMenuItem_Click(ByVal sender As System.Object, ByVal e As System.EventArgs) Handles ExibirRelatóriosDosObjetosCAPEOPENToolStripMenuItem.CheckedChanged
        If ExibirRelatóriosDosObjetosCAPEOPENToolStripMenuItem.Checked Then
            FormCOReports.Show(dckPanel)
        Else
            FormCOReports.Hide()
        End If
    End Sub

    Private Sub PainelDeVariáveisToolStripMenuItem_CheckedChanged(ByVal sender As System.Object, ByVal e As System.EventArgs) Handles PainelDeVariáveisToolStripMenuItem.CheckedChanged
        If PainelDeVariáveisToolStripMenuItem.Checked Then
            FormWatch.Show(dckPanel)
        Else
            FormWatch.Hide()
        End If
    End Sub

    Private Sub ToolStripButton16_Click(ByVal sender As System.Object, ByVal e As System.EventArgs) Handles ToolStripButton16.CheckStateChanged
        If ToolStripButton16.Checked Then
            Me.FormSurface.FlowsheetDesignSurface.SnapToGrid = True
        Else
            Me.FormSurface.FlowsheetDesignSurface.SnapToGrid = False
        End If
    End Sub

    Private Sub ToolStripButton17_Click(ByVal sender As System.Object, ByVal e As System.EventArgs) Handles ToolStripButton17.Click
        If ToolStripButton17.Checked Then
            Me.FormSurface.FlowsheetDesignSurface.QuickConnect = True
        Else
            Me.FormSurface.FlowsheetDesignSurface.QuickConnect = False
        End If
    End Sub
    Private Sub ToolStripButton2_Click(ByVal sender As System.Object, ByVal e As System.EventArgs) Handles ToolStripButton2.Click
        Me.FormSurface.FlowsheetDesignSurface.Zoom += 0.05
        Me.TSTBZoom.Text = Format(Me.FormSurface.FlowsheetDesignSurface.Zoom, "#%")
        Me.FormSurface.FlowsheetDesignSurface.Invalidate()
    End Sub

    Private Sub ToolStripButton1_Click(ByVal sender As System.Object, ByVal e As System.EventArgs) Handles ToolStripButton1.Click
        Me.FormSurface.FlowsheetDesignSurface.Zoom -= 0.05
        Me.TSTBZoom.Text = Format(Me.FormSurface.FlowsheetDesignSurface.Zoom, "#%")
        Me.FormSurface.FlowsheetDesignSurface.Invalidate()
    End Sub

    Private Sub ComponentesToolStripMenuItem_Click(ByVal sender As System.Object, ByVal e As System.EventArgs) Handles ComponentesToolStripMenuItem.Click
        If DWSIM.App.IsRunningOnMono Then
            Me.FrmStSim1.Show(Me)
        Else
            Me.FrmStSim1.ShowDialog(Me)
        End If
    End Sub

    Private Sub ToolStripButton5_Click(ByVal sender As System.Object, ByVal e As System.EventArgs) Handles ToolStripButton5.Click
        Call Me.ComponentesToolStripMenuItem_Click(sender, e)
    End Sub

    Private Sub ToolStripButton8_Click(ByVal sender As System.Object, ByVal e As System.EventArgs) Handles ToolStripButton8.Click
        Call Me.GerarRelatórioToolStripMenuItem_Click(sender, e)
    End Sub

    Private Sub GerarRelatórioToolStripMenuItem_Click(ByVal sender As System.Object, ByVal e As System.EventArgs) Handles GerarRelatórioToolStripMenuItem.Click
        Me.FrmReport.ShowDialog(Me)
    End Sub


    Private Sub CAPEOPENFlowsheetMonitoringObjectsMOsToolStripMenuItem_MouseHover(ByVal sender As Object, ByVal e As System.EventArgs) Handles CAPEOPENFlowsheetMonitoringObjectsMOsToolStripMenuItem.MouseHover

        If Me.CAPEOPENFlowsheetMonitoringObjectsMOsToolStripMenuItem.DropDownItems.Count = 0 Then

            Dim tsmi As New ToolStripMenuItem
            With tsmi
                .Text = "Please wait..."
                .DisplayStyle = ToolStripItemDisplayStyle.Text
                .AutoToolTip = False
            End With
            Me.CAPEOPENFlowsheetMonitoringObjectsMOsToolStripMenuItem.DropDownItems.Add(tsmi)

            Application.DoEvents()

            If FormMain.COMonitoringObjects.Count = 0 Then
                FormMain.SearchCOMOs()
            End If

            Me.CAPEOPENFlowsheetMonitoringObjectsMOsToolStripMenuItem.DropDownItems.Clear()

            tsmi = Nothing

            Application.DoEvents()

            'load CAPE-OPEN Flowsheet Monitoring Objects
            CreateCOMOList()

        End If


    End Sub

    Private Sub ToolStripButton18_Click(ByVal sender As System.Object, ByVal e As System.EventArgs) Handles ToolStripButton18.Click

        If Me.SaveFileDialog1.ShowDialog = Windows.Forms.DialogResult.OK Then
            Dim rect As Rectangle = New Rectangle(0, 0, Me.FormSurface.FlowsheetDesignSurface.Width - 14, Me.FormSurface.FlowsheetDesignSurface.Height - 14)
            Dim img As Image = New Bitmap(rect.Width, rect.Height)
            Me.FormSurface.FlowsheetDesignSurface.DrawToBitmap(img, Me.FormSurface.FlowsheetDesignSurface.Bounds)
            img.Save(Me.SaveFileDialog1.FileName, Imaging.ImageFormat.Png)
            img.Dispose()
        End If

    End Sub

    Private Sub ToolStripButton20_Click(ByVal sender As System.Object, ByVal e As System.EventArgs) Handles ToolStripButton20.Click
        Me.FormSurface.FlowsheetDesignSurface.ZoomAll()
        Application.DoEvents()
        Me.FormSurface.FlowsheetDesignSurface.ZoomAll()
        Application.DoEvents()
        Me.TSTBZoom.Text = Format(Me.FormSurface.FlowsheetDesignSurface.Zoom, "#%")
    End Sub

    Private Sub ToolStripButton3_Click(sender As System.Object, e As System.EventArgs) Handles ToolStripButton3.Click
        Me.FormSurface.FlowsheetDesignSurface.Zoom = 1
        Me.TSTBZoom.Text = Format(Me.FormSurface.FlowsheetDesignSurface.Zoom, "#%")
        Me.FormSurface.FlowsheetDesignSurface.Invalidate()
    End Sub

    Private Sub TSTBZoom_KeyPress(ByVal sender As System.Object, ByVal e As System.Windows.Forms.KeyEventArgs) Handles TSTBZoom.KeyDown
        If e.KeyCode = Keys.Enter Then
            Me.FormSurface.FlowsheetDesignSurface.Zoom = CInt(Me.TSTBZoom.Text.Replace("%", "")) / 100
            Me.TSTBZoom.Text = Format(Me.FormSurface.FlowsheetDesignSurface.Zoom, "#%")
            Me.FormSurface.FlowsheetDesignSurface.Invalidate()
        End If
    End Sub


    Private Sub GerenciadorDeAmostrasDePetróleoToolStripMenuItem_Click(ByVal sender As System.Object, ByVal e As System.EventArgs) Handles GerenciadorDeAmostrasDePetróleoToolStripMenuItem.Click
        Dim frmam As New FormAssayManager
        frmam.ShowDialog(Me)
        Try
            frmam.Close()
        Catch ex As Exception

        End Try
    End Sub

    Private Sub ToolStripButton21_Click(ByVal sender As System.Object, ByVal e As System.EventArgs) Handles tsbSaveState.Click

        FormMain.SaveState(Me)

    End Sub

    Private Sub RestoreState_ItemClick(ByVal sender As System.Object, ByVal e As System.EventArgs)

        Dim tsmi As ToolStripMenuItem = sender

        FormMain.RestoreState(Me, FlowsheetStates(tsmi.Tag))

    End Sub

    Private Sub RemoveState_ItemClick(ByVal sender As System.Object, ByVal e As System.EventArgs)

        Dim tsmi As ToolStripMenuItem = sender

        FlowsheetStates.Remove(tsmi.Tag)

        UpdateStateList()

    End Sub

    Private Sub RestoreSolution_ItemClick(ByVal sender As System.Object, ByVal e As System.EventArgs)

        Dim tsmi As ToolStripMenuItem = sender

        Dim solutionkey As String = tsmi.Tag

        Using ms As New MemoryStream(Me.PreviousSolutions(solutionkey).Solution)
            Using decompressedstream As New IO.MemoryStream
                Using gzs As New IO.BufferedStream(New Compression.GZipStream(ms, Compression.CompressionMode.Decompress, True), 64 * 1024)
                    gzs.CopyTo(decompressedstream)
                    gzs.Close()
                    Me.WriteToLog(DWSIM.App.GetLocalString("ClientUpdatingData") & " " & Math.Round(decompressedstream.Length / 1024).ToString & " KB", Color.Brown, TipoAviso.Informacao)
                    decompressedstream.Position = 0
                    Dim xdoc As XDocument = XDocument.Load(decompressedstream)
                    DWSIM.SimulationObjects.UnitOps.Flowsheet.UpdateProcessData(Me, xdoc)
                    DWSIM.Flowsheet.FlowsheetSolver.UpdateDisplayStatus(Me)
                    Me.WriteToLog(DWSIM.App.GetLocalString("ClientUpdatedDataOK"), Color.Brown, TipoAviso.Informacao)
                End Using
            End Using
        End Using

    End Sub

    Sub UpdateSolutionsList()

        With Me.tsbRestoreSolutions.DropDownItems

            .Clear()

            While Me.PreviousSolutions.Count > 15
                Dim idtoremove As String = ""
                For Each s In Me.PreviousSolutions.Values
                    idtoremove = s.ID
                    Exit For
                Next
                If Me.PreviousSolutions.ContainsKey(idtoremove) Then Me.PreviousSolutions.Remove(idtoremove)
            End While

            For Each k As Long In Me.PreviousSolutions.Keys

                Dim tsmi As ToolStripMenuItem = .Add(Me.PreviousSolutions(k).SaveDate.ToString)
                tsmi.Tag = k
                AddHandler tsmi.Click, AddressOf RestoreSolution_ItemClick

            Next

        End With

    End Sub

    Sub UpdateStateList()

        With Me.tsbRestoreStates.DropDownItems

            .Clear()

            For Each k As Date In Me.FlowsheetStates.Keys

                Dim tsmi As ToolStripMenuItem = .Add(Me.FlowsheetStates(k).Description & " (" & k.ToString & ")")
                tsmi.Tag = k
                tsmi.Image = Me.FlowsheetStates(k).Snapshot
                AddHandler tsmi.Click, AddressOf RestoreState_ItemClick

                Dim tsmiR As ToolStripMenuItem = tsmi.DropDownItems.Add(DWSIM.App.GetLocalString("RestoreState"))
                tsmiR.Tag = k
                tsmiR.Image = My.Resources.arrow_in
                AddHandler tsmiR.Click, AddressOf RestoreState_ItemClick

                Dim tsmiE As ToolStripMenuItem = tsmi.DropDownItems.Add(DWSIM.App.GetLocalString("Excluir"))
                tsmiE.Tag = k
                tsmiE.Image = My.Resources.cross
                AddHandler tsmiE.Click, AddressOf RemoveState_ItemClick

            Next

        End With

    End Sub

    Private Sub ToolStripButton21_Click_1(ByVal sender As System.Object, ByVal e As System.EventArgs) Handles tsbClearStates.Click

        If Not Me.FlowsheetStates Is Nothing Then
            Me.FlowsheetStates.Clear()
            UpdateStateList()
        End If

    End Sub

    Private Sub tsbSimultAdjustSolver_CheckedChanged(ByVal sender As System.Object, ByVal e As System.EventArgs) Handles tsbSimultAdjustSolver.CheckedChanged
        m_simultadjustsolverenabled = tsbSimultAdjustSolver.Checked
    End Sub

#End Region

#Region "    Connect/Disconnect Objects "

    Public Sub DeleteSelectedObject(ByVal sender As System.Object, ByVal e As System.EventArgs, gobj As GraphicObject, Optional ByVal confirmation As Boolean = True)

        If Not gobj Is Nothing Then
            Dim SelectedObj As GraphicObject = gobj
            Dim namesel As String = SelectedObj.Name
            If Not gobj.IsConnector Then
                Dim msgresult As MsgBoxResult
                If confirmation Then
                    If SelectedObj.TipoObjeto = TipoObjeto.GO_Figura Then
                        msgresult = MessageBox.Show(DWSIM.App.GetLocalString("Excluirafiguraseleci"), DWSIM.App.GetLocalString("Excluirobjeto"), MessageBoxButtons.YesNo, MessageBoxIcon.Question)
                    ElseIf SelectedObj.TipoObjeto = TipoObjeto.GO_Tabela Then
                        MessageBox.Show(DWSIM.App.GetLocalString("Atabelapodeseroculta") & vbCrLf & DWSIM.App.GetLocalString("doobjetoqualelaperte"), DWSIM.App.GetLocalString("Nopossvelexcluirtabe"), MessageBoxButtons.OK, MessageBoxIcon.Information)
                    ElseIf SelectedObj.TipoObjeto = TipoObjeto.GO_Texto Then
                        msgresult = MessageBox.Show(DWSIM.App.GetLocalString("Excluiracaixadetexto"), DWSIM.App.GetLocalString("Excluirobjeto"), MessageBoxButtons.YesNo, MessageBoxIcon.Question)
                    Else
                        msgresult = MessageBox.Show(DWSIM.App.GetLocalString("Excluir") & gobj.Tag & "?", DWSIM.App.GetLocalString("Excluirobjeto"), MessageBoxButtons.YesNo, MessageBoxIcon.Question)
                    End If
                Else
                    msgresult = MsgBoxResult.Yes
                End If
                If msgresult = MsgBoxResult.Yes Then

                    'remove object property table, if it exists
                    Dim tables As List(Of GraphicObject) = (From t As GraphicObject In Me.FormSurface.FlowsheetDesignSurface.drawingObjects
                                                                      Select t Where t.TipoObjeto = TipoObjeto.GO_Tabela).ToList
                    Dim tablelist As List(Of TableGraphic) = (From t As TableGraphic In tables
                                                                      Select t Where t.BaseOwner.Nome = gobj.Name).ToList
                    If Not tablelist Is Nothing Then
                        For Each table As TableGraphic In tablelist
                            Try
                                Me.FormSurface.FlowsheetDesignSurface.DeleteSelectedObject(table)
                            Catch ex As Exception

                            End Try
                        Next
                    End If

                    If SelectedObj.IsEnergyStream Then

                        DeCalculateObject(Me, SelectedObj)
                        Dim InCon, OutCon As ConnectionPoint
                        For Each InCon In gobj.InputConnectors
                            If InCon.IsAttached = True Then
                                If InCon.AttachedConnector.AttachedFrom.EnergyConnector.IsAttached Then
                                    With InCon.AttachedConnector.AttachedFrom.EnergyConnector
                                        .IsAttached = False
                                        gobj = .AttachedConnector
                                        Me.FormSurface.FlowsheetDesignSurface.DeleteSelectedObject(gobj)
                                        .AttachedConnector = Nothing
                                    End With
                                Else
                                    With InCon.AttachedConnector.AttachedFrom.OutputConnectors(InCon.AttachedConnector.AttachedFromConnectorIndex)
                                        .IsAttached = False
                                        gobj = .AttachedConnector
                                        Me.FormSurface.FlowsheetDesignSurface.DeleteSelectedObject(gobj)
                                        .AttachedConnector = Nothing
                                    End With
                                End If
                            End If
                        Next
                        gobj = SelectedObj
                        For Each OutCon In gobj.OutputConnectors
                            If OutCon.IsAttached = True Then
                                With OutCon.AttachedConnector.AttachedTo.InputConnectors(OutCon.AttachedConnector.AttachedToConnectorIndex)
                                    .IsAttached = False
                                    gobj = .AttachedConnector
                                    Me.FormSurface.FlowsheetDesignSurface.DeleteSelectedObject(gobj)
                                    .AttachedConnector = Nothing
                                End With
                            End If
                        Next
                        gobj = SelectedObj

                        Me.Collections.EnergyStreamCollection.Remove(namesel)
                        If Not DWSIM.App.IsRunningOnMono Then Me.FormObjList.TreeViewObj.Nodes("NodeEN").Nodes.RemoveByKey(namesel)
                        'DWSIM
                        Me.Collections.CLCS_EnergyStreamCollection(namesel).Dispose()
                        Me.Collections.CLCS_EnergyStreamCollection.Remove(namesel)
                        Me.Collections.ObjectCollection.Remove(namesel)
                        Me.Collections.ObjectCollection.Remove(namesel)
                        Me.FormSurface.FlowsheetDesignSurface.DeleteSelectedObject(gobj)
                    Else

                        If SelectedObj.TipoObjeto = TipoObjeto.GO_Figura Then
                            Me.FormSurface.FlowsheetDesignSurface.DeleteSelectedObject(gobj)
                        ElseIf SelectedObj.TipoObjeto = TipoObjeto.GO_Tabela Then
                            'Me.FormSurface.FlowsheetDesignSurface.DeleteSelectedObject(gobj)
                        ElseIf SelectedObj.TipoObjeto = TipoObjeto.GO_MasterTable Then
                            Me.FormSurface.FlowsheetDesignSurface.DeleteSelectedObject(gobj)
                        ElseIf SelectedObj.TipoObjeto = TipoObjeto.GO_Texto Then
                            Me.FormSurface.FlowsheetDesignSurface.DeleteSelectedObject(gobj)
                        ElseIf SelectedObj.TipoObjeto = TipoObjeto.GO_TabelaRapida Then
                            Me.FormSurface.FlowsheetDesignSurface.DeleteSelectedObject(gobj)
                        Else
                            Dim obj As SimulationObjects_BaseClass = Me.Collections.ObjectCollection(SelectedObj.Name)
                            DeCalculateObject(Me, SelectedObj)
                            If gobj.EnergyConnector.IsAttached = True Then
                                With gobj.EnergyConnector.AttachedConnector.AttachedTo.InputConnectors(0)
                                    .IsAttached = False
                                    gobj = .AttachedConnector
                                    Me.FormSurface.FlowsheetDesignSurface.DeleteSelectedObject(gobj)
                                    .AttachedConnector = Nothing
                                End With
                            End If
                            gobj = SelectedObj
                            Dim InCon, OutCon As ConnectionPoint
                            For Each InCon In gobj.InputConnectors
                                Try
                                    If InCon.IsAttached = True Then
                                        With InCon.AttachedConnector.AttachedFrom.OutputConnectors(InCon.AttachedConnector.AttachedFromConnectorIndex)
                                            .IsAttached = False
                                            gobj = .AttachedConnector
                                            Me.FormSurface.FlowsheetDesignSurface.DeleteSelectedObject(gobj)
                                            .AttachedConnector = Nothing
                                        End With
                                    End If
                                Catch ex As Exception

                                End Try
                            Next
                            gobj = SelectedObj
                            For Each OutCon In gobj.OutputConnectors
                                Try
                                    If OutCon.IsAttached = True Then
                                        With OutCon.AttachedConnector.AttachedTo.InputConnectors(OutCon.AttachedConnector.AttachedToConnectorIndex)
                                            .IsAttached = False
                                            gobj = .AttachedConnector
                                            Me.FormSurface.FlowsheetDesignSurface.DeleteSelectedObject(gobj)
                                            .AttachedConnector = Nothing
                                        End With
                                    End If
                                Catch ex As Exception

                                End Try
                            Next

                            gobj = SelectedObj

                            'dispose object
                            Me.Collections.ObjectCollection(namesel).Dispose()

                            Select Case SelectedObj.TipoObjeto
                                Case TipoObjeto.NodeIn
                                    Me.Collections.MixerCollection.Remove(namesel)
                                    If Not DWSIM.App.IsRunningOnMono Then Me.FormObjList.TreeViewObj.Nodes("NodeMX").Nodes.RemoveByKey(namesel)
                                    'DWSIM
                                    Me.Collections.CLCS_MixerCollection.Remove(namesel)
                                    Me.Collections.ObjectCollection.Remove(namesel)
                                Case TipoObjeto.NodeOut
                                    Me.Collections.SplitterCollection.Remove(namesel)
                                    If Not DWSIM.App.IsRunningOnMono Then Me.FormObjList.TreeViewObj.Nodes("NodeSP").Nodes.RemoveByKey(namesel)
                                    'DWSIM
                                    Me.Collections.CLCS_SplitterCollection.Remove(namesel)
                                    Me.Collections.ObjectCollection.Remove(namesel)
                                Case TipoObjeto.NodeEn
                                    Me.Collections.MixerENCollection.Remove(namesel)
                                    If Not DWSIM.App.IsRunningOnMono Then Me.FormObjList.TreeViewObj.Nodes("NodeME").Nodes.RemoveByKey(namesel)
                                    'DWSIM
                                    Me.Collections.CLCS_EnergyMixerCollection.Remove(namesel)
                                    Me.Collections.ObjectCollection.Remove(namesel)
                                Case TipoObjeto.Pump
                                    Me.Collections.PumpCollection.Remove(namesel)
                                    If Not DWSIM.App.IsRunningOnMono Then Me.FormObjList.TreeViewObj.Nodes("NodePU").Nodes.RemoveByKey(namesel)
                                    'DWSIM
                                    Me.Collections.CLCS_PumpCollection.Remove(namesel)
                                    Me.Collections.ObjectCollection.Remove(namesel)
                                Case TipoObjeto.Tank
                                    Me.Collections.TankCollection.Remove(namesel)
                                    If Not DWSIM.App.IsRunningOnMono Then Me.FormObjList.TreeViewObj.Nodes("NodeTQ").Nodes.RemoveByKey(namesel)
                                    'DWSIM
                                    Me.Collections.CLCS_TankCollection.Remove(namesel)
                                    Me.Collections.ObjectCollection.Remove(namesel)
                                Case TipoObjeto.Vessel
                                    Me.Collections.SeparatorCollection.Remove(namesel)
                                    If Not DWSIM.App.IsRunningOnMono Then Me.FormObjList.TreeViewObj.Nodes("NodeSE").Nodes.RemoveByKey(namesel)
                                    'DWSIM
                                    Me.Collections.CLCS_VesselCollection.Remove(namesel)
                                    Me.Collections.ObjectCollection.Remove(namesel)
                                Case TipoObjeto.MaterialStream
                                    Me.Collections.MaterialStreamCollection.Remove(namesel)
                                    If Not DWSIM.App.IsRunningOnMono Then Me.FormObjList.TreeViewObj.Nodes("NodeMS").Nodes.RemoveByKey(namesel)
                                    'DWSIM
                                    Me.Collections.CLCS_MaterialStreamCollection.Remove(namesel)
                                    Me.Collections.ObjectCollection.Remove(namesel)
                                Case TipoObjeto.Compressor
                                    Me.Collections.CompressorCollection.Remove(namesel)
                                    If Not DWSIM.App.IsRunningOnMono Then Me.FormObjList.TreeViewObj.Nodes("NodeCO").Nodes.RemoveByKey(namesel)
                                    'DWSIM
                                    Me.Collections.CLCS_CompressorCollection.Remove(namesel)
                                    Me.Collections.ObjectCollection.Remove(namesel)
                                Case TipoObjeto.Expander
                                    Me.Collections.TurbineCollection.Remove(namesel)
                                    If Not DWSIM.App.IsRunningOnMono Then Me.FormObjList.TreeViewObj.Nodes("NodeTU").Nodes.RemoveByKey(namesel)
                                    'DWSIM
                                    Me.Collections.CLCS_TurbineCollection.Remove(namesel)
                                    Me.Collections.ObjectCollection.Remove(namesel)
                                Case TipoObjeto.TPVessel
                                    Me.Collections.TPSeparatorCollection.Remove(namesel)
                                    If Not DWSIM.App.IsRunningOnMono Then Me.FormObjList.TreeViewObj.Nodes("NodeTP").Nodes.RemoveByKey(namesel)
                                Case TipoObjeto.Cooler
                                    Me.Collections.CoolerCollection.Remove(namesel)
                                    If Not DWSIM.App.IsRunningOnMono Then Me.FormObjList.TreeViewObj.Nodes("NodeCL").Nodes.RemoveByKey(namesel)
                                    'DWSIM
                                    Me.Collections.CLCS_CoolerCollection.Remove(namesel)
                                    Me.Collections.ObjectCollection.Remove(namesel)
                                Case TipoObjeto.Heater
                                    Me.Collections.HeaterCollection.Remove(namesel)
                                    If Not DWSIM.App.IsRunningOnMono Then Me.FormObjList.TreeViewObj.Nodes("NodeHT").Nodes.RemoveByKey(namesel)
                                    'DWSIM
                                    Me.Collections.CLCS_HeaterCollection.Remove(namesel)
                                    Me.Collections.ObjectCollection.Remove(namesel)
                                Case TipoObjeto.Pipe
                                    Me.Collections.CLCS_PipeCollection.Remove(namesel)
                                    If Not DWSIM.App.IsRunningOnMono Then Me.FormObjList.TreeViewObj.Nodes("NodePI").Nodes.RemoveByKey(namesel)
                                    'DWSIM
                                    Me.Collections.PipeCollection.Remove(namesel)
                                    Me.Collections.ObjectCollection.Remove(namesel)
                                Case TipoObjeto.Valve
                                    Me.Collections.ValveCollection.Remove(namesel)
                                    If Not DWSIM.App.IsRunningOnMono Then Me.FormObjList.TreeViewObj.Nodes("NodeVA").Nodes.RemoveByKey(namesel)
                                    'DWSIM
                                    Me.Collections.CLCS_ValveCollection.Remove(namesel)
                                    Me.Collections.ObjectCollection.Remove(namesel)
                                Case TipoObjeto.OT_Ajuste
                                    Me.Collections.AdjustCollection.Remove(namesel)
                                    'DWSIM
                                    Me.Collections.CLCS_AdjustCollection.Remove(namesel)
                                    Me.Collections.ObjectCollection.Remove(namesel)
                                Case TipoObjeto.OT_Especificacao
                                    If Me.Collections.ObjectCollection.ContainsKey(Me.Collections.CLCS_SpecCollection(namesel).TargetObjectData.m_ID) Then
                                        Me.Collections.ObjectCollection(Me.Collections.CLCS_SpecCollection(namesel).TargetObjectData.m_ID).IsSpecAttached = False
                                        Me.Collections.ObjectCollection(Me.Collections.CLCS_SpecCollection(namesel).TargetObjectData.m_ID).AttachedSpecId = ""
                                    End If
                                    If Me.Collections.ObjectCollection.ContainsKey(Me.Collections.CLCS_SpecCollection(namesel).SourceObjectData.m_ID) Then
                                        Me.Collections.ObjectCollection(Me.Collections.CLCS_SpecCollection(namesel).SourceObjectData.m_ID).IsSpecAttached = False
                                        Me.Collections.ObjectCollection(Me.Collections.CLCS_SpecCollection(namesel).SourceObjectData.m_ID).AttachedSpecId = ""
                                    End If
                                    Me.Collections.SpecCollection.Remove(namesel)
                                    'DWSIM
                                    Me.Collections.CLCS_SpecCollection.Remove(namesel)
                                    Me.Collections.ObjectCollection.Remove(namesel)
                                Case TipoObjeto.OT_Reciclo
                                    Me.Collections.RecycleCollection.Remove(namesel)
                                    'DWSIM
                                    Me.Collections.CLCS_RecycleCollection.Remove(namesel)
                                    Me.Collections.ObjectCollection.Remove(namesel)
                                Case TipoObjeto.HeatExchanger
                                    Me.Collections.HeatExchangerCollection.Remove(namesel)
                                    If Not DWSIM.App.IsRunningOnMono Then Me.FormObjList.TreeViewObj.Nodes("NodeHE").Nodes.RemoveByKey(namesel)
                                    'DWSIM
                                    Me.Collections.CLCS_HeatExchangerCollection.Remove(namesel)
                                    Me.Collections.ObjectCollection.Remove(namesel)
                                Case TipoObjeto.ShortcutColumn
                                    Me.Collections.HeatExchangerCollection.Remove(namesel)
                                    If Not DWSIM.App.IsRunningOnMono Then Me.FormObjList.TreeViewObj.Nodes("NodeSC").Nodes.RemoveByKey(namesel)
                                    'DWSIM
                                    Me.Collections.CLCS_HeatExchangerCollection.Remove(namesel)
                                    Me.Collections.ObjectCollection.Remove(namesel)
                                Case TipoObjeto.OrificePlate
                                    Me.Collections.OrificePlateCollection.Remove(namesel)
                                    If Not DWSIM.App.IsRunningOnMono Then Me.FormObjList.TreeViewObj.Nodes("NodeOPL").Nodes.RemoveByKey(namesel)
                                    'DWSIM
                                    Me.Collections.CLCS_OrificePlateCollection.Remove(namesel)
                                    Me.Collections.ObjectCollection.Remove(namesel)
                                Case TipoObjeto.ComponentSeparator
                                    Me.Collections.ComponentSeparatorCollection.Remove(namesel)
                                    If Not DWSIM.App.IsRunningOnMono Then Me.FormObjList.TreeViewObj.Nodes("NodeCSEP").Nodes.RemoveByKey(namesel)
                                    'DWSIM
                                    Me.Collections.CLCS_ComponentSeparatorCollection.Remove(namesel)
                                    Me.Collections.ObjectCollection.Remove(namesel)
                                Case TipoObjeto.CustomUO
                                    Me.Collections.CustomUOCollection.Remove(namesel)
                                    If Not DWSIM.App.IsRunningOnMono Then Me.FormObjList.TreeViewObj.Nodes("NodeUO").Nodes.RemoveByKey(namesel)
                                    'DWSIM
                                    Me.Collections.CLCS_CustomUOCollection.Remove(namesel)
                                    Me.Collections.ObjectCollection.Remove(namesel)
                                Case TipoObjeto.ExcelUO
                                    Me.Collections.ExcelUOCollection.Remove(namesel)
                                    If Not DWSIM.App.IsRunningOnMono Then Me.FormObjList.TreeViewObj.Nodes("NodeExcel").Nodes.RemoveByKey(namesel)
                                    'DWSIM
                                    Me.Collections.CLCS_ExcelUOCollection.Remove(namesel)
                                    Me.Collections.ObjectCollection.Remove(namesel)
                                    Me.Collections.ObjectCollection.Remove(namesel)
                                Case TipoObjeto.FlowsheetUO
                                    Me.Collections.FlowsheetUOCollection.Remove(namesel)
                                    If Not DWSIM.App.IsRunningOnMono Then Me.FormObjList.TreeViewObj.Nodes("NodeFS").Nodes.RemoveByKey(namesel)
                                    'DWSIM
                                    Me.Collections.CLCS_FlowsheetUOCollection.Remove(namesel)
                                    Me.Collections.ObjectCollection.Remove(namesel)
                                Case TipoObjeto.CapeOpenUO
                                    Me.Collections.CapeOpenUOCollection.Remove(namesel)
                                    If Not DWSIM.App.IsRunningOnMono Then Me.FormObjList.TreeViewObj.Nodes("NodeCOUO").Nodes.RemoveByKey(namesel)
                                    'DWSIM
                                    Me.Collections.CLCS_CapeOpenUOCollection.Remove(namesel)
                                    Me.Collections.ObjectCollection.Remove(namesel)
                                Case TipoObjeto.SolidSeparator
                                    Me.Collections.SolidsSeparatorCollection.Remove(namesel)
                                    If Not DWSIM.App.IsRunningOnMono Then Me.FormObjList.TreeViewObj.Nodes("NodeSS").Nodes.RemoveByKey(namesel)
                                    'DWSIM
                                    Me.Collections.CLCS_SolidsSeparatorCollection.Remove(namesel)
                                    Me.Collections.ObjectCollection.Remove(namesel)
                                Case TipoObjeto.Filter
                                    Me.Collections.FilterCollection.Remove(namesel)
                                    If Not DWSIM.App.IsRunningOnMono Then Me.FormObjList.TreeViewObj.Nodes("NodeFT").Nodes.RemoveByKey(namesel)
                                    'DWSIM
                                    Me.Collections.CLCS_FilterCollection.Remove(namesel)
                                    Me.Collections.ObjectCollection.Remove(namesel)
                                Case TipoObjeto.RCT_Conversion
                                    Me.Collections.ReactorConversionCollection.Remove(namesel)
                                    If Not DWSIM.App.IsRunningOnMono Then Me.FormObjList.TreeViewObj.Nodes("NodeRCONV").Nodes.RemoveByKey(namesel)
                                    'DWSIM
                                    Me.Collections.CLCS_ReactorConversionCollection.Remove(namesel)
                                    Me.Collections.ObjectCollection.Remove(namesel)
                                Case TipoObjeto.RCT_CSTR
                                    Me.Collections.ReactorCSTRCollection.Remove(namesel)
                                    If Not DWSIM.App.IsRunningOnMono Then Me.FormObjList.TreeViewObj.Nodes("NodeRCSTR").Nodes.RemoveByKey(namesel)
                                    'DWSIM
                                    Me.Collections.CLCS_ReactorCSTRCollection.Remove(namesel)
                                    Me.Collections.ObjectCollection.Remove(namesel)
                                Case TipoObjeto.RCT_Equilibrium
                                    Me.Collections.ReactorEquilibriumCollection.Remove(namesel)
                                    If Not DWSIM.App.IsRunningOnMono Then Me.FormObjList.TreeViewObj.Nodes("NodeREQ").Nodes.RemoveByKey(namesel)
                                    'DWSIM
                                    Me.Collections.CLCS_ReactorEquilibriumCollection.Remove(namesel)
                                    Me.Collections.ObjectCollection.Remove(namesel)
                                Case TipoObjeto.RCT_Gibbs
                                    Me.Collections.ReactorGibbsCollection.Remove(namesel)
                                    If Not DWSIM.App.IsRunningOnMono Then Me.FormObjList.TreeViewObj.Nodes("NodeRGIB").Nodes.RemoveByKey(namesel)
                                    'DWSIM
                                    Me.Collections.CLCS_ReactorGibbsCollection.Remove(namesel)
                                    Me.Collections.ObjectCollection.Remove(namesel)
                                Case TipoObjeto.RCT_PFR
                                    Me.Collections.ReactorPFRCollection.Remove(namesel)
                                    If Not DWSIM.App.IsRunningOnMono Then Me.FormObjList.TreeViewObj.Nodes("NodeRPFR").Nodes.RemoveByKey(namesel)
                                    'DWSIM
                                    Me.Collections.CLCS_ReactorPFRCollection.Remove(namesel)
                                    Me.Collections.ObjectCollection.Remove(namesel)
                                Case TipoObjeto.DistillationColumn
                                    Me.Collections.DistillationColumnCollection.Remove(namesel)
                                    If Not DWSIM.App.IsRunningOnMono Then Me.FormObjList.TreeViewObj.Nodes("NodeDC").Nodes.RemoveByKey(namesel)
                                    'DWSIM
                                    Me.Collections.CLCS_DistillationColumnCollection.Remove(namesel)
                                    Me.Collections.ObjectCollection.Remove(namesel)
                                Case TipoObjeto.AbsorptionColumn
                                    Me.Collections.AbsorptionColumnCollection.Remove(namesel)
                                    If Not DWSIM.App.IsRunningOnMono Then Me.FormObjList.TreeViewObj.Nodes("NodeAC").Nodes.RemoveByKey(namesel)
                                    'DWSIM
                                    Me.Collections.CLCS_AbsorptionColumnCollection.Remove(namesel)
                                    Me.Collections.ObjectCollection.Remove(namesel)
                                Case TipoObjeto.RefluxedAbsorber
                                    Me.Collections.RefluxedAbsorberCollection.Remove(namesel)
                                    If Not DWSIM.App.IsRunningOnMono Then Me.FormObjList.TreeViewObj.Nodes("NodeRFA").Nodes.RemoveByKey(namesel)
                                    'DWSIM
                                    Me.Collections.CLCS_RefluxedAbsorberCollection.Remove(namesel)
                                    Me.Collections.ObjectCollection.Remove(namesel)
                                Case TipoObjeto.ReboiledAbsorber
                                    Me.Collections.ReboiledAbsorberCollection.Remove(namesel)
                                    If Not DWSIM.App.IsRunningOnMono Then Me.FormObjList.TreeViewObj.Nodes("NodeRBA").Nodes.RemoveByKey(namesel)
                                    'DWSIM
                                    Me.Collections.CLCS_ReboiledAbsorberCollection.Remove(namesel)
                                    Me.Collections.ObjectCollection.Remove(namesel)
                            End Select

                            Me.FormSurface.FlowsheetDesignSurface.DeleteSelectedObject(gobj)

                        End If

                        Dim arrays(Me.Collections.ObjectCollection.Count - 1) As String
                        Dim aNode, aNode2 As TreeNode
                        Dim i As Integer = 0
                        For Each aNode In Me.FormObjList.TreeViewObj.Nodes
                            For Each aNode2 In aNode.Nodes
                                Try
                                    arrays(i) = aNode2.Text
                                Catch ex As Exception
                                End Try
                                i += 1
                            Next
                        Next
                        Me.FormObjList.ACSC.Clear()
                        Me.FormObjList.ACSC.AddRange(arrays)
                        Me.FormObjList.TBSearch.AutoCompleteCustomSource = Me.FormObjList.ACSC

                    End If
                End If
            End If
        End If
    End Sub

    Public Sub DeleteObject(ByVal tag As String, Optional ByVal confirmation As Boolean = True)

        Dim gobj As GraphicObject = Me.GetFlowsheetGraphicObject(tag)

        If Not gobj Is Nothing Then
            Me.FormSurface.FlowsheetDesignSurface.SelectedObject = gobj
            Me.DeleteSelectedObject(Me, New EventArgs(), gobj, confirmation)
        End If

    End Sub

    Public Sub DisconnectObject(ByRef gObjFrom As GraphicObject, ByRef gObjTo As GraphicObject)

        Dim conObj As ConnectorGraphic = Nothing
        Dim SelObj As GraphicObject = gObjFrom
        Dim ObjToDisconnect As GraphicObject = Nothing
        ObjToDisconnect = gObjTo
        If Not ObjToDisconnect Is Nothing Then
            Dim conptObj As ConnectionPoint = Nothing
            For Each conptObj In SelObj.InputConnectors
                If conptObj.IsAttached = True Then
                    If Not conptObj.AttachedConnector Is Nothing Then
                        If conptObj.AttachedConnector.AttachedFrom.Name.ToString = ObjToDisconnect.Name.ToString Then
                            DeCalculateDisconnectedObject(Me, SelObj, "In")
                            conptObj.AttachedConnector.AttachedFrom.OutputConnectors(conptObj.AttachedConnector.AttachedFromConnectorIndex).IsAttached = False
                            conptObj.AttachedConnector.AttachedFrom.OutputConnectors(conptObj.AttachedConnector.AttachedFromConnectorIndex).AttachedConnector = Nothing
                            Me.FormSurface.FlowsheetDesignSurface.SelectedObjects.Clear()
                            conptObj.IsAttached = False
                            Me.FormSurface.FlowsheetDesignSurface.DeleteSelectedObject(conptObj.AttachedConnector)
                            Exit Sub
                        End If
                    End If
                End If
            Next
            For Each conptObj In SelObj.OutputConnectors
                If conptObj.IsAttached = True Then
                    If Not conptObj.AttachedConnector Is Nothing Then
                        If conptObj.AttachedConnector.AttachedTo.Name.ToString = ObjToDisconnect.Name.ToString Then
                            DeCalculateDisconnectedObject(Me, SelObj, "Out")
                            conptObj.AttachedConnector.AttachedTo.InputConnectors(conptObj.AttachedConnector.AttachedToConnectorIndex).IsAttached = False
                            conptObj.AttachedConnector.AttachedTo.InputConnectors(conptObj.AttachedConnector.AttachedToConnectorIndex).AttachedConnector = Nothing
                            conptObj.IsAttached = False
                            Me.FormSurface.FlowsheetDesignSurface.DeleteSelectedObject(conptObj.AttachedConnector)
                            Exit Sub
                        End If
                    End If
                End If
            Next
            If SelObj.EnergyConnector.IsAttached = True Then
                If SelObj.EnergyConnector.AttachedConnector.AttachedFrom.Name.ToString = ObjToDisconnect.Name.ToString Then
                    DeCalculateDisconnectedObject(Me, SelObj, "Out")
                    SelObj.EnergyConnector.AttachedConnector.AttachedFrom.OutputConnectors(SelObj.EnergyConnector.AttachedConnector.AttachedFromConnectorIndex).IsAttached = False
                    SelObj.EnergyConnector.AttachedConnector.AttachedFrom.OutputConnectors(SelObj.EnergyConnector.AttachedConnector.AttachedFromConnectorIndex).AttachedConnector = Nothing
                    SelObj.EnergyConnector.IsAttached = False
                    Me.FormSurface.FlowsheetDesignSurface.DeleteSelectedObject(SelObj.EnergyConnector.AttachedConnector)
                    Exit Sub
                End If
            End If
        End If

    End Sub

    Public Sub ConnectObject(ByRef gObjFrom As GraphicObject, ByRef gObjTo As GraphicObject, Optional ByVal fidx As Integer = -1, Optional ByVal tidx As Integer = -1)

        If gObjFrom.TipoObjeto <> TipoObjeto.GO_Figura And gObjFrom.TipoObjeto <> TipoObjeto.GO_Tabela And _
        gObjFrom.TipoObjeto <> TipoObjeto.GO_Tabela And gObjFrom.TipoObjeto <> TipoObjeto.GO_TabelaRapida And _
        gObjFrom.TipoObjeto <> TipoObjeto.Nenhum And _
        gObjTo.TipoObjeto <> TipoObjeto.GO_Figura And gObjTo.TipoObjeto <> TipoObjeto.GO_Tabela And _
        gObjTo.TipoObjeto <> TipoObjeto.GO_Tabela And gObjTo.TipoObjeto <> TipoObjeto.GO_TabelaRapida And _
        gObjTo.TipoObjeto <> TipoObjeto.Nenhum And gObjTo.TipoObjeto <> TipoObjeto.GO_MasterTable Then

            Dim con1OK As Boolean = False
            Dim con2OK As Boolean = False

            'For Each gObj In Me.FormSurface.FlowsheetDesignSurface.drawingObjects

            '    If gObjConnectFrom_Tag = gObj.Tag.ToString Then gObjFrom = gObj
            '    If gObjConnectTo_Tag = gObj.Tag.ToString Then gObjTo = gObj

            'Next

            'posicionar pontos nos primeiros slots livres
            Dim StartPos, EndPos As New Point
            Dim InConSlot, OutConSlot As New ConnectionPoint
            If Not gObjFrom Is Nothing Then
                If Not gObjTo Is Nothing Then
                    If gObjFrom.TipoObjeto = TipoObjeto.MaterialStream And gObjTo.TipoObjeto = TipoObjeto.MaterialStream Then
                        Throw New Exception(DWSIM.App.GetLocalString("Nopossvelrealizaress"))
                     ElseIf gObjFrom.TipoObjeto = TipoObjeto.EnergyStream And gObjTo.TipoObjeto = TipoObjeto.EnergyStream Then
                        Throw New Exception(DWSIM.App.GetLocalString("Nopossvelrealizaress"))
                    ElseIf Not gObjFrom.TipoObjeto = TipoObjeto.MaterialStream And Not gObjFrom.TipoObjeto = TipoObjeto.EnergyStream Then
                        If Not gObjTo.TipoObjeto = TipoObjeto.EnergyStream And Not gObjTo.TipoObjeto = TipoObjeto.MaterialStream Then
                            Throw New Exception(DWSIM.App.GetLocalString("Nopossvelrealizaress"))
                        End If
                    ElseIf gObjFrom.TipoObjeto = TipoObjeto.MaterialStream And gObjTo.TipoObjeto = TipoObjeto.EnergyStream Then
                        Throw New Exception(DWSIM.App.GetLocalString("Nopossvelrealizaress"))
                    ElseIf gObjFrom.TipoObjeto = TipoObjeto.EnergyStream And gObjTo.TipoObjeto = TipoObjeto.MaterialStream Then
                        Throw New Exception(DWSIM.App.GetLocalString("Nopossvelrealizaress"))
                    End If
                    If gObjTo.IsEnergyStream = False Then
                        If Not gObjFrom.IsEnergyStream Then
                            If tidx = -1 Then
                                For Each InConSlot In gObjTo.InputConnectors
                                    If Not InConSlot.IsAttached And InConSlot.Type = ConType.ConIn Then
                                        EndPos = InConSlot.Position
                                        InConSlot.IsAttached = True
                                        con2OK = True
                                        Exit For
                                    End If
                                Next
                            Else
                                If Not gObjTo.InputConnectors(tidx).IsAttached And gObjTo.InputConnectors(tidx).Type = ConType.ConIn Then
                                    InConSlot = gObjTo.InputConnectors(tidx)
                                    EndPos = InConSlot.Position
                                    InConSlot.IsAttached = True
                                    con2OK = True
                                End If
                            End If
                        Else
                            If tidx = -1 Then
                                For Each InConSlot In gObjTo.InputConnectors
                                    If Not InConSlot.IsAttached And InConSlot.Type = ConType.ConEn Then
                                        EndPos = InConSlot.Position
                                        InConSlot.IsAttached = True
                                        con2OK = True
                                        Exit For
                                    End If
                                Next
                            Else
                                If Not gObjTo.InputConnectors(tidx).IsAttached And gObjTo.InputConnectors(tidx).Type = ConType.ConEn Then
                                    InConSlot = gObjTo.InputConnectors(tidx)
                                    EndPos = InConSlot.Position
                                    InConSlot.IsAttached = True
                                    con2OK = True
                                End If
                            End If
                            If Not con2OK Then
                                Throw New Exception(DWSIM.App.GetLocalString("Correntesdeenergiasp"))
                                Exit Sub
                            End If
                        End If
                        If fidx = -1 Then
                            For Each OutConSlot In gObjFrom.OutputConnectors
                                If Not OutConSlot.IsAttached Then
                                    StartPos = OutConSlot.Position
                                    OutConSlot.IsAttached = True
                                    If con2OK Then con1OK = True
                                    Exit For
                                End If
                            Next
                        Else
                            If Not gObjFrom.OutputConnectors(fidx).IsAttached Then
                                OutConSlot = gObjFrom.OutputConnectors(fidx)
                                StartPos = OutConSlot.Position
                                OutConSlot.IsAttached = True
                                If con2OK Then con1OK = True
                            End If
                        End If
                    Else
                        Select Case gObjFrom.TipoObjeto
                            Case TipoObjeto.Cooler, TipoObjeto.Pipe, TipoObjeto.Expander, TipoObjeto.ShortcutColumn, TipoObjeto.DistillationColumn, TipoObjeto.AbsorptionColumn,
                                TipoObjeto.ReboiledAbsorber, TipoObjeto.RefluxedAbsorber, TipoObjeto.OT_EnergyRecycle, TipoObjeto.ComponentSeparator, TipoObjeto.SolidSeparator,
                                TipoObjeto.Filter, TipoObjeto.CustomUO, TipoObjeto.CapeOpenUO, TipoObjeto.FlowsheetUO
                                GoTo 100
                            Case Else
                                Throw New Exception(DWSIM.App.GetLocalString("Correntesdeenergiasp2") & DWSIM.App.GetLocalString("TubulaesTurbinaseRes"))
                         End Select
100:                    If gObjFrom.TipoObjeto <> TipoObjeto.CapeOpenUO And gObjFrom.TipoObjeto <> TipoObjeto.CustomUO And gObjFrom.TipoObjeto <> TipoObjeto.DistillationColumn _
                            And gObjFrom.TipoObjeto <> TipoObjeto.AbsorptionColumn And gObjFrom.TipoObjeto <> TipoObjeto.OT_EnergyRecycle _
                            And gObjFrom.TipoObjeto <> TipoObjeto.RefluxedAbsorber And gObjFrom.TipoObjeto <> TipoObjeto.ReboiledAbsorber Then
                            If Not gObjFrom.EnergyConnector.IsAttached Then
                                StartPos = gObjFrom.EnergyConnector.Position
                                gObjFrom.EnergyConnector.IsAttached = True
                                con1OK = True
                                OutConSlot = gObjFrom.EnergyConnector
                                EndPos = gObjTo.InputConnectors(0).Position
                                gObjTo.InputConnectors(0).IsAttached = True
                                con2OK = True
                                InConSlot = gObjTo.InputConnectors(0)
                            End If
                        Else
                            If tidx = -1 Then
                                For Each InConSlot In gObjTo.InputConnectors
                                    If Not InConSlot.IsAttached And InConSlot.Type = ConType.ConIn Then
                                        EndPos = InConSlot.Position
                                        InConSlot.IsAttached = True
                                        con2OK = True
                                        Exit For
                                    End If
                                Next
                            Else
                                If Not gObjTo.InputConnectors(tidx).IsAttached And gObjTo.InputConnectors(tidx).Type = ConType.ConIn Then
                                    InConSlot = gObjTo.InputConnectors(tidx)
                                    EndPos = InConSlot.Position
                                    InConSlot.IsAttached = True
                                    con2OK = True
                                End If
                            End If
                            If fidx = -1 Then
                                For Each OutConSlot In gObjFrom.OutputConnectors
                                    If Not OutConSlot.IsAttached And OutConSlot.Type = ConType.ConEn Then
                                        StartPos = OutConSlot.Position
                                        OutConSlot.IsAttached = True
                                        If con2OK Then con1OK = True
                                        Exit For
                                    End If
                                Next
                            Else
                                If Not gObjFrom.OutputConnectors(fidx).IsAttached Then
                                    OutConSlot = gObjFrom.OutputConnectors(fidx)
                                    StartPos = OutConSlot.Position
                                    OutConSlot.IsAttached = True
                                    If con2OK Then con1OK = True
                                End If
                            End If
                        End If
                    End If
                Else
                    Me.WriteToLog(DWSIM.App.GetLocalString("Nohobjetosaseremcone"), Color.Blue, TipoAviso.Informacao)
                    Exit Sub
                End If
            Else
                Me.WriteToLog(DWSIM.App.GetLocalString("Nohobjetosaseremcone"), Color.Blue, TipoAviso.Informacao)
                Exit Sub
            End If
            If con1OK = True And con2OK = True Then
                'desenhar conector
                Dim myCon As New ConnectorGraphic(StartPos, EndPos, 1, Color.DarkRed)
                OutConSlot.AttachedConnector = myCon
                InConSlot.AttachedConnector = myCon
                With myCon
                    .IsConnector = True
                    .AttachedFrom = gObjFrom
                    If gObjFrom.IsEnergyStream Then
                        .AttachedFromEnergy = True
                    End If
                    .AttachedFromConnectorIndex = gObjFrom.OutputConnectors.IndexOf(OutConSlot)
                    .AttachedTo = gObjTo
                    If gObjTo.IsEnergyStream Then
                        .AttachedToEnergy = True
                    End If
                    .AttachedToConnectorIndex = gObjTo.InputConnectors.IndexOf(InConSlot)
                    If Not myCon Is Nothing Then
                        Me.FormSurface.FlowsheetDesignSurface.drawingObjects.Add(myCon)
                        Me.FormSurface.FlowsheetDesignSurface.Invalidate()
                    End If
                End With
            Else
                Throw New Exception(DWSIM.App.GetLocalString("Todasasconexespossve"))
            End If

        Else


        End If

    End Sub

#End Region

#Region "    Property Grid 2 Populate Functions "

    Public Function PopulatePGEx2(ByRef gobj As GraphicObject)

        If gobj.TipoObjeto = TipoObjeto.GO_Tabela Then

            Dim gobj2 As DWSIM.GraphicObjects.TableGraphic = CType(gobj, DWSIM.GraphicObjects.TableGraphic)

            With Me.FormProps.PGEx2

                .PropertySort = PropertySort.Categorized
                .ShowCustomProperties = True
                .Item.Clear()

                .Item.Add(DWSIM.App.GetLocalString("Cor"), gobj2, "LineColor", False, DWSIM.App.GetLocalString("Formataodotexto1"), DWSIM.App.GetLocalString("Cordotextodatabela"), True)
                With .Item(.Item.Count - 1)
                    .DefaultType = GetType(System.Drawing.Color)
                End With
                .Item.Add(DWSIM.App.GetLocalString("Cabealho"), gobj2, "HeaderFont", False, DWSIM.App.GetLocalString("Formataodotexto1"), DWSIM.App.GetLocalString("Fontedotextodocabeal"), True)
                With .Item(.Item.Count - 1)
                    .DefaultType = GetType(System.Drawing.Font)
                End With
                .Item.Add(DWSIM.App.GetLocalString("Coluna1Fonte"), gobj2, "FontCol1", False, DWSIM.App.GetLocalString("Formataodotexto1"), DWSIM.App.GetLocalString("Fontedotextodacoluna"), True)
                With .Item(.Item.Count - 1)
                    .DefaultType = GetType(System.Drawing.Font)
                End With
                .Item.Add(DWSIM.App.GetLocalString("Coluna2Fonte"), gobj2, "FontCol2", False, DWSIM.App.GetLocalString("Formataodotexto1"), DWSIM.App.GetLocalString("Fontedotextodacoluna2"), True)
                With .Item(.Item.Count - 1)
                    .DefaultType = GetType(System.Drawing.Font)
                End With
                .Item.Add(DWSIM.App.GetLocalString("Coluna3Fonte"), gobj2, "FontCol3", False, DWSIM.App.GetLocalString("Formataodotexto1"), DWSIM.App.GetLocalString("Fontedotextodacoluna3"), True)
                With .Item(.Item.Count - 1)
                    .DefaultType = GetType(System.Drawing.Font)
                End With

                .Item.Add(DWSIM.App.GetLocalString("Tratamentodotexto"), gobj2, "TextRenderStyle", False, DWSIM.App.GetLocalString("Aparncia2"), DWSIM.App.GetLocalString("Tipodesuavizaoaplica"), True)
                .Item.Add(DWSIM.App.GetLocalString("Estilodaborda"), gobj2, "BorderStyle", False, DWSIM.App.GetLocalString("Aparncia2"), DWSIM.App.GetLocalString("Estilodabordatraceja"), True)
                .Item.Add(DWSIM.App.GetLocalString("Cordaborda"), gobj2, "BorderColor", False, DWSIM.App.GetLocalString("Aparncia2"), "", True)
                With .Item(.Item.Count - 1)
                    .DefaultType = GetType(System.Drawing.Color)
                End With
                .Item.Add(DWSIM.App.GetLocalString("Espaamento"), gobj2, "Padding", False, DWSIM.App.GetLocalString("Aparncia2"), DWSIM.App.GetLocalString("Espaamentoentreotext"), True)
                With .Item(.Item.Count - 1)
                    .DefaultType = GetType(Integer)
                End With
                .Item.Add(DWSIM.App.GetLocalString("Rotao"), gobj2, "Rotation", False, DWSIM.App.GetLocalString("Aparncia2"), DWSIM.App.GetLocalString("Inclinaodatabelaemre"), True)
                With .Item(.Item.Count - 1)
                    .DefaultType = GetType(System.Int32)
                End With

                .Item.Add(DWSIM.App.GetLocalString("Gradiente2"), gobj2, "IsGradientBackground", False, DWSIM.App.GetLocalString("Fundo"), "Selecione se deve ser utilizado um gradiente no fundo da tabela", True)
                With .Item(.Item.Count - 1)
                    .DefaultType = GetType(Boolean)
                End With
                .Item.Add(DWSIM.App.GetLocalString("Corsemgradiente"), gobj2, "FillColor", False, DWSIM.App.GetLocalString("Fundo"), DWSIM.App.GetLocalString("Corsemgradiente"), True)
                With .Item(.Item.Count - 1)
                    .DefaultType = GetType(System.Drawing.Color)
                End With
                .Item.Add(DWSIM.App.GetLocalString("Cor1gradiente"), gobj2, "BackgroundGradientColor1", False, DWSIM.App.GetLocalString("Fundo"), DWSIM.App.GetLocalString("Cor1dogradientecasoa"), True)
                With .Item(.Item.Count - 1)
                    .DefaultType = GetType(System.Drawing.Color)
                End With
                .Item.Add(DWSIM.App.GetLocalString("Cor2gradiente"), gobj2, "BackgroundGradientColor2", False, DWSIM.App.GetLocalString("Fundo"), DWSIM.App.GetLocalString("Cor2dogradientecasoa"), True)
                With .Item(.Item.Count - 1)
                    .DefaultType = GetType(System.Drawing.Color)
                End With
                .Item.Add(DWSIM.App.GetLocalString("Opacidade0255"), gobj2, "Opacity", False, DWSIM.App.GetLocalString("Fundo"), DWSIM.App.GetLocalString("Nveldetransparnciada"), True)
                With .Item(.Item.Count - 1)
                    .DefaultType = GetType(Integer)
                End With

            End With

            gobj2 = Nothing
            FormProps.FTSProps.SelectedItem = FormProps.TSProps

        ElseIf gobj.TipoObjeto = TipoObjeto.GO_MasterTable Then

            Dim gobj2 As DWSIM.GraphicObjects.MasterTableGraphic = CType(gobj, DWSIM.GraphicObjects.MasterTableGraphic)

            With Me.FormProps.PGEx2

                .PropertySort = PropertySort.Categorized
                .ShowCustomProperties = True
                .Item.Clear()

                .Item.Add(DWSIM.App.GetLocalString("Cor"), gobj2, "LineColor", False, DWSIM.App.GetLocalString("Formataodotexto1"), DWSIM.App.GetLocalString("Cordotextodatabela"), True)
                With .Item(.Item.Count - 1)
                    .DefaultType = GetType(System.Drawing.Color)
                End With
                .Item.Add(DWSIM.App.GetLocalString("Cabealho"), gobj2, "HeaderFont", False, DWSIM.App.GetLocalString("Formataodotexto1"), DWSIM.App.GetLocalString("Fontedotextodocabeal"), True)
                With .Item(.Item.Count - 1)
                    .DefaultType = GetType(System.Drawing.Font)
                End With
                .Item.Add(DWSIM.App.GetLocalString("Coluna1Fonte"), gobj2, "FontCol1", False, DWSIM.App.GetLocalString("Formataodotexto1"), DWSIM.App.GetLocalString("Fontedotextodacoluna"), True)
                With .Item(.Item.Count - 1)
                    .DefaultType = GetType(System.Drawing.Font)
                End With
                .Item.Add(DWSIM.App.GetLocalString("Coluna2Fonte"), gobj2, "FontCol2", False, DWSIM.App.GetLocalString("Formataodotexto1"), DWSIM.App.GetLocalString("Fontedotextodacoluna2"), True)
                With .Item(.Item.Count - 1)
                    .DefaultType = GetType(System.Drawing.Font)
                End With
                .Item.Add(DWSIM.App.GetLocalString("Coluna3Fonte"), gobj2, "FontCol3", False, DWSIM.App.GetLocalString("Formataodotexto1"), DWSIM.App.GetLocalString("Fontedotextodacoluna3"), True)
                With .Item(.Item.Count - 1)
                    .DefaultType = GetType(System.Drawing.Font)
                End With

                .Item.Add(DWSIM.App.GetLocalString("HeaderText"), gobj2, "HeaderText", False, DWSIM.App.GetLocalString("Aparncia2"), DWSIM.App.GetLocalString(""), True)
                .Item.Add(DWSIM.App.GetLocalString("Tratamentodotexto"), gobj2, "TextRenderStyle", False, DWSIM.App.GetLocalString("Aparncia2"), DWSIM.App.GetLocalString("Tipodesuavizaoaplica"), True)
                .Item.Add(DWSIM.App.GetLocalString("Estilodaborda"), gobj2, "BorderStyle", False, DWSIM.App.GetLocalString("Aparncia2"), DWSIM.App.GetLocalString("Estilodabordatraceja"), True)
                .Item.Add(DWSIM.App.GetLocalString("Cordaborda"), gobj2, "BorderColor", False, DWSIM.App.GetLocalString("Aparncia2"), "", True)
                With .Item(.Item.Count - 1)
                    .DefaultType = GetType(System.Drawing.Color)
                End With
                .Item.Add(DWSIM.App.GetLocalString("Espaamento"), gobj2, "Padding", False, DWSIM.App.GetLocalString("Aparncia2"), DWSIM.App.GetLocalString("Espaamentoentreotext"), True)
                With .Item(.Item.Count - 1)
                    .DefaultType = GetType(Integer)
                End With
                .Item.Add(DWSIM.App.GetLocalString("Rotao"), gobj2, "Rotation", False, DWSIM.App.GetLocalString("Aparncia2"), DWSIM.App.GetLocalString("Inclinaodatabelaemre"), True)
                With .Item(.Item.Count - 1)
                    .DefaultType = GetType(System.Int32)
                End With

                .Item.Add(DWSIM.App.GetLocalString("Gradiente2"), gobj2, "IsGradientBackground", False, DWSIM.App.GetLocalString("Fundo"), "Selecione se deve ser utilizado um gradiente no fundo da tabela", True)
                With .Item(.Item.Count - 1)
                    .DefaultType = GetType(Boolean)
                End With
                .Item.Add(DWSIM.App.GetLocalString("Corsemgradiente"), gobj2, "FillColor", False, DWSIM.App.GetLocalString("Fundo"), DWSIM.App.GetLocalString("Corsemgradiente"), True)
                With .Item(.Item.Count - 1)
                    .DefaultType = GetType(System.Drawing.Color)
                End With
                .Item.Add(DWSIM.App.GetLocalString("Cor1gradiente"), gobj2, "BackgroundGradientColor1", False, DWSIM.App.GetLocalString("Fundo"), DWSIM.App.GetLocalString("Cor1dogradientecasoa"), True)
                With .Item(.Item.Count - 1)
                    .DefaultType = GetType(System.Drawing.Color)
                End With
                .Item.Add(DWSIM.App.GetLocalString("Cor2gradiente"), gobj2, "BackgroundGradientColor2", False, DWSIM.App.GetLocalString("Fundo"), DWSIM.App.GetLocalString("Cor2dogradientecasoa"), True)
                With .Item(.Item.Count - 1)
                    .DefaultType = GetType(System.Drawing.Color)
                End With
                .Item.Add(DWSIM.App.GetLocalString("Opacidade0255"), gobj2, "Opacity", False, DWSIM.App.GetLocalString("Fundo"), DWSIM.App.GetLocalString("Nveldetransparnciada"), True)
                With .Item(.Item.Count - 1)
                    .DefaultType = GetType(Integer)
                End With

            End With

            gobj2 = Nothing
            FormProps.FTSProps.SelectedItem = FormProps.TSProps

        ElseIf gobj.TipoObjeto = TipoObjeto.GO_SpreadsheetTable Then

            Dim gobj2 As DWSIM.GraphicObjects.SpreadsheetTableGraphic = CType(gobj, DWSIM.GraphicObjects.SpreadsheetTableGraphic)

            With Me.FormProps.PGEx2

                .PropertySort = PropertySort.Categorized
                .ShowCustomProperties = True
                .Item.Clear()

                .Item.Add(DWSIM.App.GetLocalString("Cor"), gobj2, "LineColor", False, DWSIM.App.GetLocalString("Formataodotexto1"), DWSIM.App.GetLocalString("Cordotextodatabela"), True)
                With .Item(.Item.Count - 1)
                    .DefaultType = GetType(System.Drawing.Color)
                End With
                .Item.Add(DWSIM.App.GetLocalString("Coluna1Fonte"), gobj2, "FontCol1", False, DWSIM.App.GetLocalString("Formataodotexto1"), DWSIM.App.GetLocalString("Fontedotextodacoluna"), True)
                With .Item(.Item.Count - 1)
                    .DefaultType = GetType(System.Drawing.Font)
                End With
                
                .Item.Add(DWSIM.App.GetLocalString("Tratamentodotexto"), gobj2, "TextRenderStyle", False, DWSIM.App.GetLocalString("Aparncia2"), DWSIM.App.GetLocalString("Tipodesuavizaoaplica"), True)
                .Item.Add(DWSIM.App.GetLocalString("Estilodaborda"), gobj2, "BorderStyle", False, DWSIM.App.GetLocalString("Aparncia2"), DWSIM.App.GetLocalString("Estilodabordatraceja"), True)
                .Item.Add(DWSIM.App.GetLocalString("Cordaborda"), gobj2, "BorderColor", False, DWSIM.App.GetLocalString("Aparncia2"), "", True)
                With .Item(.Item.Count - 1)
                    .DefaultType = GetType(System.Drawing.Color)
                End With
                .Item.Add(DWSIM.App.GetLocalString("Espaamento"), gobj2, "Padding", False, DWSIM.App.GetLocalString("Aparncia2"), DWSIM.App.GetLocalString("Espaamentoentreotext"), True)
                With .Item(.Item.Count - 1)
                    .DefaultType = GetType(Integer)
                End With
                .Item.Add(DWSIM.App.GetLocalString("Rotao"), gobj2, "Rotation", False, DWSIM.App.GetLocalString("Aparncia2"), DWSIM.App.GetLocalString("Inclinaodatabelaemre"), True)
                With .Item(.Item.Count - 1)
                    .DefaultType = GetType(System.Int32)
                End With

                .Item.Add(DWSIM.App.GetLocalString("Gradiente2"), gobj2, "IsGradientBackground", False, DWSIM.App.GetLocalString("Fundo"), "Selecione se deve ser utilizado um gradiente no fundo da tabela", True)
                With .Item(.Item.Count - 1)
                    .DefaultType = GetType(Boolean)
                End With
                .Item.Add(DWSIM.App.GetLocalString("Corsemgradiente"), gobj2, "FillColor", False, DWSIM.App.GetLocalString("Fundo"), DWSIM.App.GetLocalString("Corsemgradiente"), True)
                With .Item(.Item.Count - 1)
                    .DefaultType = GetType(System.Drawing.Color)
                End With
                .Item.Add(DWSIM.App.GetLocalString("Cor1gradiente"), gobj2, "BackgroundGradientColor1", False, DWSIM.App.GetLocalString("Fundo"), DWSIM.App.GetLocalString("Cor1dogradientecasoa"), True)
                With .Item(.Item.Count - 1)
                    .DefaultType = GetType(System.Drawing.Color)
                End With
                .Item.Add(DWSIM.App.GetLocalString("Cor2gradiente"), gobj2, "BackgroundGradientColor2", False, DWSIM.App.GetLocalString("Fundo"), DWSIM.App.GetLocalString("Cor2dogradientecasoa"), True)
                With .Item(.Item.Count - 1)
                    .DefaultType = GetType(System.Drawing.Color)
                End With
                .Item.Add(DWSIM.App.GetLocalString("Opacidade0255"), gobj2, "Opacity", False, DWSIM.App.GetLocalString("Fundo"), DWSIM.App.GetLocalString("Nveldetransparnciada"), True)
                With .Item(.Item.Count - 1)
                    .DefaultType = GetType(Integer)
                End With

            End With

            gobj2 = Nothing
            FormProps.FTSProps.SelectedItem = FormProps.TSProps

        ElseIf gobj.TipoObjeto = TipoObjeto.GO_Texto Then

            Dim gobj2 As TextGraphic = CType(gobj, TextGraphic)

            With Me.FormProps.PGEx2

                .PropertySort = PropertySort.Categorized
                .ShowCustomProperties = True
                .Item.Clear()

                .Item.Add(DWSIM.App.GetLocalString("Nome"), gobj.Tag, False, DWSIM.App.GetLocalString("Descrio1"), DWSIM.App.GetLocalString("Nomedoobjeto"), True)
                With .Item(.Item.Count - 1)
                    .DefaultType = GetType(System.Boolean)
                End With

                .Item.Add(DWSIM.App.GetLocalString("Texto"), gobj2, "Text", False, "", DWSIM.App.GetLocalString("Textoaserexibidonaca"), True)
                With .Item(.Item.Count - 1)
                    .CustomEditor = New System.ComponentModel.Design.MultilineStringEditor
                    .DefaultType = GetType(String)
                End With
                .Item.Add(DWSIM.App.GetLocalString("Tratamentodotexto"), gobj2, "TextRenderStyle", False, DWSIM.App.GetLocalString("Aparncia2"), DWSIM.App.GetLocalString("Tipodesuavizaoaplica"), True)
                .Item.Add(DWSIM.App.GetLocalString("Cor"), gobj2, "Color", False, "", DWSIM.App.GetLocalString("Cordotexto"), True)
                With .Item(.Item.Count - 1)
                    .DefaultType = GetType(System.Drawing.Color)
                End With
                .Item.Add(DWSIM.App.GetLocalString("Fonte"), gobj2, "Font", False, "", DWSIM.App.GetLocalString("Fontedotexto"), True)
                With .Item(.Item.Count - 1)
                    .DefaultType = GetType(System.Drawing.Font)
                End With

            End With

            gobj2 = Nothing
            FormProps.FTSProps.SelectedItem = FormProps.TSObj

        ElseIf gobj.TipoObjeto = TipoObjeto.GO_Figura Then

            Dim gobj2 As EmbeddedImageGraphic = CType(gobj, EmbeddedImageGraphic)

            With Me.FormProps.PGEx2

                .PropertySort = PropertySort.Categorized
                .ShowCustomProperties = True
                .Item.Clear()

                .Item.Add(DWSIM.App.GetLocalString("Autodimensionar"), gobj2, "AutoSize", False, "", DWSIM.App.GetLocalString("SelecioLiquidrueparaque"), True)
                .Item.Add(DWSIM.App.GetLocalString("Altura"), gobj2, "Height", False, "", DWSIM.App.GetLocalString("Alturadafiguraempixe"), True)
                .Item.Add(DWSIM.App.GetLocalString("Largura"), gobj2, "Width", False, "", DWSIM.App.GetLocalString("Larguradafiguraempix"), True)
                .Item.Add(DWSIM.App.GetLocalString("Rotao"), gobj2, "Rotation", False, "", DWSIM.App.GetLocalString("Rotaodafigurade0a360"), True)

            End With

            gobj2 = Nothing
            FormProps.FTSProps.SelectedItem = FormProps.TSObj

        ElseIf gobj.TipoObjeto = TipoObjeto.GO_Animation Then

            Dim gobj2 As EmbeddedAnimationGraphic = CType(gobj, EmbeddedAnimationGraphic)

            With Me.FormProps.PGEx2

                .PropertySort = PropertySort.Categorized
                .ShowCustomProperties = True
                .Item.Clear()

                .Item.Add(DWSIM.App.GetLocalString("Autodimensionar"), gobj2, DWSIM.App.GetLocalString("AutoSize"), False, "", DWSIM.App.GetLocalString("SelecioLiquidrueparaque"), True)
                .Item.Add(DWSIM.App.GetLocalString("Altura"), gobj2, "Height", False, "", DWSIM.App.GetLocalString("Alturadafiguraempixe"), True)
                .Item.Add(DWSIM.App.GetLocalString("Largura"), gobj2, "Width", False, "", DWSIM.App.GetLocalString("Larguradafiguraempix"), True)
                .Item.Add(DWSIM.App.GetLocalString("Rotao"), gobj2, "Rotation", False, "", DWSIM.App.GetLocalString("Rotaodafigurade0a360"), True)

            End With

            gobj2 = Nothing
            FormProps.FTSProps.SelectedItem = FormProps.TSObj

        Else

            With Me.FormProps.PGEx2

                .PropertySort = PropertySort.Categorized
                .ShowCustomProperties = True
                .Item.Clear()

                .Item.Add(DWSIM.App.GetLocalString("Nome"), gobj, "Tag", False, DWSIM.App.GetLocalString("Descrio1"), DWSIM.App.GetLocalString("Nomedoobjeto"), True)
                With .Item(.Item.Count - 1)
                    .DefaultType = GetType(System.Boolean)
                End With

                .Item.Add(DWSIM.App.GetLocalString("Gradiente2"), gobj, "GradientMode", False, DWSIM.App.GetLocalString("Aparncia2"), DWSIM.App.GetLocalString("SelecioLiquidrueparaapl"), True)
                With .Item(.Item.Count - 1)
                    .DefaultType = GetType(System.Boolean)
                End With
                .Item.Add("Gradiente_Cor1", gobj, "GradientColor1", False, DWSIM.App.GetLocalString("Aparncia2"), DWSIM.App.GetLocalString("Cor1dogradienteseapl"), True)
                With .Item(.Item.Count - 1)
                    .DefaultType = GetType(System.Drawing.Color)
                End With
                .Item.Add("Gradiente_Cor2", gobj, "GradientColor2", False, DWSIM.App.GetLocalString("Aparncia2"), DWSIM.App.GetLocalString("Cor2dogradienteseapl"), True)
                With .Item(.Item.Count - 1)
                    .DefaultType = GetType(System.Drawing.Color)
                End With
                .Item.Add(DWSIM.App.GetLocalString("Cor"), gobj, "FillColor", False, DWSIM.App.GetLocalString("Aparncia2"), "Cor de fundo, caso o modo de gradiente não esteja ativado", True)
                With .Item(.Item.Count - 1)
                    .DefaultType = GetType(System.Drawing.Color)
                End With
                .Item.Add(DWSIM.App.GetLocalString("EspessuradaBorda"), gobj, "LineWidth", False, DWSIM.App.GetLocalString("Aparncia2"), DWSIM.App.GetLocalString("Espessuradabordadoob"), True)
                With .Item(.Item.Count - 1)
                    .DefaultType = GetType(System.Int32)
                End With

                .Item.Add(DWSIM.App.GetLocalString("Comprimento"), gobj, "Width", False, DWSIM.App.GetLocalString("Tamanho3"), DWSIM.App.GetLocalString("Comprimentodoobjetoe"), True)
                With .Item(.Item.Count - 1)
                    .DefaultType = GetType(System.Int32)
                End With
                .Item.Add(DWSIM.App.GetLocalString("Altura"), gobj, "Height", False, DWSIM.App.GetLocalString("Tamanho3"), DWSIM.App.GetLocalString("Alturadoobjetoempixe"), True)
                With .Item(.Item.Count - 1)
                    .DefaultType = GetType(System.Int32)
                End With
                .Item.Add(DWSIM.App.GetLocalString("Rotao"), gobj, "Rotation", False, DWSIM.App.GetLocalString("Tamanho3"), DWSIM.App.GetLocalString("Rotaodoobjetode0a360"), True)
                With .Item(.Item.Count - 1)
                    .DefaultType = GetType(System.Int32)
                End With

                .Item.Add("X", gobj, "X", False, DWSIM.App.GetLocalString("Coordenadas4"), DWSIM.App.GetLocalString("Coordenadahorizontal"), True)
                With .Item(.Item.Count - 1)
                    .DefaultType = GetType(System.Int32)
                End With
                .Item.Add("Y", gobj, "Y", False, DWSIM.App.GetLocalString("Coordenadas4"), DWSIM.App.GetLocalString("Coordenadaverticaldo"), True)
                With .Item(.Item.Count - 1)
                    .DefaultType = GetType(System.Int32)
                End With

            End With

            FormProps.FTSProps.SelectedItem = FormProps.TSProps

        End If

        Return 1

    End Function

#End Region

#Region "    Plugin/CAPE-OPEN MO Management "

    Private Sub CreatePluginsList()

        'process plugin list

        For Each iplugin As Interfaces.IUtilityPlugin In My.MyApplication.UtilityPlugins.Values

            Dim tsmi As New ToolStripMenuItem
            With tsmi
                .Text = iplugin.Name
                .Tag = iplugin.UniqueID
                .Image = My.Resources.plugin
                .DisplayStyle = ToolStripItemDisplayStyle.ImageAndText
            End With
            Me.PluginsToolStripMenuItem.DropDownItems.Add(tsmi)
            AddHandler tsmi.Click, AddressOf Me.PluginClick
        Next

    End Sub

    Private Sub PluginClick(ByVal sender As System.Object, ByVal e As System.EventArgs)

        Dim tsmi As ToolStripMenuItem = CType(sender, ToolStripMenuItem)

        Dim myUPlugin As Interfaces.IUtilityPlugin = My.MyApplication.UtilityPlugins.Item(tsmi.Tag)

        myUPlugin.SetFlowsheet(Me)
        Select Case myUPlugin.DisplayMode
            Case Interfaces.IUtilityPlugin.DispMode.Normal
                myUPlugin.UtilityForm.Show(Me)
            Case Interfaces.IUtilityPlugin.DispMode.Modal
                myUPlugin.UtilityForm.ShowDialog(Me)
            Case Interfaces.IUtilityPlugin.DispMode.Dockable
                CType(myUPlugin.UtilityForm, Docking.DockContent).Show(Me.dckPanel)
        End Select

    End Sub

    Private Sub CreateCOMOList()

        'process plugin list

        For Each icomo As DWSIM.SimulationObjects.UnitOps.Auxiliary.CapeOpen.CapeOpenUnitOpInfo In FormMain.COMonitoringObjects.Values

            Dim tsmi As New ToolStripMenuItem
            With tsmi
                .Text = icomo.Name
                .Tag = icomo.TypeName
                .Image = My.Resources.colan2
                .DisplayStyle = ToolStripItemDisplayStyle.ImageAndText
                .AutoToolTip = False
            End With
            With icomo
                tsmi.ToolTipText = "TypeName: " & vbTab & .TypeName & vbCrLf & _
                                    "Version: " & vbTab & vbTab & .Version & vbCrLf & _
                                    "Vendor URL: " & vbTab & .VendorURL & vbCrLf & _
                                    "About: " & vbTab & vbTab & .AboutInfo
            End With
            Me.CAPEOPENFlowsheetMonitoringObjectsMOsToolStripMenuItem.DropDownItems.Add(tsmi)
            AddHandler tsmi.Click, AddressOf Me.COMOClick
        Next

    End Sub

    Private Sub COMOClick(ByVal sender As System.Object, ByVal e As System.EventArgs)

        Dim tsmi As ToolStripMenuItem = CType(sender, ToolStripMenuItem)

        Dim myCOMO As DWSIM.SimulationObjects.UnitOps.Auxiliary.CapeOpen.CapeOpenUnitOpInfo = FormMain.COMonitoringObjects.Item(tsmi.Tag)

        Dim _como As Object = Nothing
        Try
            Dim t As Type = Type.GetTypeFromProgID(myCOMO.TypeName)
            _como = Activator.CreateInstance(t)
            If TryCast(_como, CapeOpen.ICapeUtilities) IsNot Nothing Then
                If TryCast(_como, Interfaces.IPersistStreamInit) IsNot Nothing Then
                    CType(_como, Interfaces.IPersistStreamInit).InitNew()
                End If
                With CType(_como, CapeOpen.ICapeUtilities)
                    .Initialize()
                    .simulationContext = Me
                    .Edit()
                End With
            End If
        Catch ex As Exception
            Me.WriteToLog("Error creating CAPE-OPEN Flowsheet Monitoring Object: " & ex.ToString, Color.Red, DWSIM.FormClasses.TipoAviso.Erro)
        Finally
            If TryCast(_como, CapeOpen.ICapeUtilities) IsNot Nothing Then
                With CType(_como, CapeOpen.ICapeUtilities)
                    .Terminate()
                End With
            End If
        End Try

    End Sub

#End Region

#Region "    CAPE-OPEN COSE/PME Methods and Properties "

    Public Function NamedValue(ByVal value As String) As Object Implements CapeOpen.ICapeCOSEUtilities.NamedValue

        Return NamedValueList()

    End Function
    Public ReadOnly Property NamedValueList() As Object Implements CapeOpen.ICapeCOSEUtilities.NamedValueList
        Get
            Return New String() {Nothing}
        End Get
    End Property

    Public Sub LogMessage(ByVal message As String) Implements CapeOpen.ICapeDiagnostic.LogMessage
        Me.WriteMessage(message)
    End Sub

    Public Sub PopUpMessage(ByVal message As String) Implements CapeOpen.ICapeDiagnostic.PopUpMessage
        MessageBox.Show(message)
    End Sub

    Public Function CreateMaterialTemplate(ByVal materialTemplateName As String) As Object Implements CapeOpen.ICapeMaterialTemplateSystem.CreateMaterialTemplate
        For Each pp As PropertyPackage In Me.Options.PropertyPackages.Values
            If materialTemplateName = pp.ComponentName Then
                Dim mat As New Streams.MaterialStream("temporary stream", "temporary stream", Me, pp)
                Me.AddComponentsRows(mat)
                Return mat
                Exit For
            Else
                Return Nothing
            End If
        Next
        Return Nothing
    End Function

    Public ReadOnly Property MaterialTemplates() As Object Implements CapeOpen.ICapeMaterialTemplateSystem.MaterialTemplates
        Get
            Dim pps As New ArrayList
            For Each p As PropertyPackage In Me.Options.PropertyPackages.Values
                pps.Add(p.ComponentName)
            Next
            Dim arr2(pps.Count - 1) As String
            Array.Copy(pps.ToArray, arr2, pps.Count)
            Return arr2
        End Get
    End Property

    Public Function GetStreamCollection() As Object Implements CapeOpen.ICapeFlowsheetMonitoring.GetStreamCollection
        Dim _col As New DWSIM.SimulationObjects.UnitOps.Auxiliary.CapeOpen.CCapeCollection
        For Each o As SimulationObjects_BaseClass In Me.Collections.ObjectCollection.Values
            If TryCast(o, CapeOpen.ICapeThermoMaterialObject) IsNot Nothing Then
                'object is a CAPE-OPEN Material Object
                _col._icol.Add(o)
            ElseIf TryCast(o, CapeOpen.ICapeCollection) IsNot Nothing Then
                'object is a CAPE-OPEN Energy Object
                _col._icol.Add(o)
            End If
        Next
        Return _col
    End Function

    Public Function GetUnitOperationCollection() As Object Implements CapeOpen.ICapeFlowsheetMonitoring.GetUnitOperationCollection
        Dim _col As New DWSIM.SimulationObjects.UnitOps.Auxiliary.CapeOpen.CCapeCollection
        For Each o As SimulationObjects_BaseClass In Me.Collections.ObjectCollection.Values
            If TryCast(o, CapeOpen.ICapeUnit) IsNot Nothing Then
                'object is a CAPE-OPEN Unit Operation
                _col._icol.Add(o)
            End If
        Next
        Return _col
    End Function

    Public ReadOnly Property SolutionStatus() As CapeOpen.CapeSolutionStatus Implements CapeOpen.ICapeFlowsheetMonitoring.SolutionStatus
        Get
            Return CapeOpen.CapeSolutionStatus.CAPE_SOLVED
        End Get
    End Property

    Public ReadOnly Property ValStatus() As CapeOpen.CapeValidationStatus Implements CapeOpen.ICapeFlowsheetMonitoring.ValStatus
        Get
            Return CapeOpen.CapeValidationStatus.CAPE_VALID
        End Get
    End Property

    Public Property ComponentDescription() As String Implements CapeOpen.ICapeIdentification.ComponentDescription
        Get
            Return Me.Options.SimComentario
        End Get
        Set(ByVal value As String)
            Me.Options.SimComentario = value
        End Set
    End Property

    Public Property ComponentName() As String Implements CapeOpen.ICapeIdentification.ComponentName
        Get
            Return Me.Options.SimNome
        End Get
        Set(ByVal value As String)
            Me.Options.SimNome = value
        End Set
    End Property

#End Region

#Region "    Script Timers"

    Private Sub TimerScripts1_Tick(sender As Object, e As EventArgs) Handles TimerScripts1.Tick
        Me.ProcessScripts(Script.EventType.SimulationTimer1, Script.ObjectType.Simulation)
    End Sub

    Private Sub TimerScripts5_Tick(sender As Object, e As EventArgs) Handles TimerScripts5.Tick
        Me.ProcessScripts(Script.EventType.SimulationTimer5, Script.ObjectType.Simulation)
    End Sub

    Private Sub TimerScripts15_Tick(sender As Object, e As EventArgs) Handles TimerScripts15.Tick
        Me.ProcessScripts(Script.EventType.SimulationTimer15, Script.ObjectType.Simulation)
    End Sub

    Private Sub TimerScripts30_Tick(sender As Object, e As EventArgs) Handles TimerScripts30.Tick
        Me.ProcessScripts(Script.EventType.SimulationTimer30, Script.ObjectType.Simulation)
    End Sub

    Private Sub TimerScripts60_Tick(sender As Object, e As EventArgs) Handles TimerScripts60.Tick
        Me.ProcessScripts(Script.EventType.SimulationTimer60, Script.ObjectType.Simulation)
    End Sub

#End Region

End Class
