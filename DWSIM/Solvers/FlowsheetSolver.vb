'    DWSIM Flowsheet Solver & Auxiliary Functions
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
Imports System.IO
Imports DWSIM.DWSIM.SimulationObjects
Imports System.Threading.Tasks
Imports DWSIM.DWSIM.Outros
Imports Microsoft.Scripting.Hosting

Namespace DWSIM.Flowsheet

    <System.Serializable()> Public Class FlowsheetSolver

        'events for plugins
        Public Shared Event UnitOpCalculationStarted As CustomEvent
        Public Shared Event UnitOpCalculationFinished As CustomEvent
        Public Shared Event FlowsheetCalculationStarted As CustomEvent
        Public Shared Event FlowsheetCalculationFinished As CustomEvent
        Public Shared Event MaterialStreamCalculationStarted As CustomEvent
        Public Shared Event MaterialStreamCalculationFinished As CustomEvent

        ''' <summary>
        ''' Flowsheet calculation routine 1. Calculates the object sent by the queue and updates the flowsheet.
        ''' </summary>
        ''' <param name="form">Flowsheet to calculate (FormChild object).</param>
        ''' <param name="objArgs">A StatusChangeEventArgs object containing information about the object to be calculated and its current status.</param>
        ''' <param name="sender"></param>
        ''' <remarks></remarks>
        Public Shared Sub CalculateFlowsheet(ByVal form As FormFlowsheet, ByVal objArgs As DWSIM.Outros.StatusChangeEventArgs, ByVal sender As Object, Optional ByVal OnlyMe As Boolean = False)

            Dim preLab As String = form.FormSurface.LabelCalculator.Text

            If form.Options.CalculatorActivated Then

                RaiseEvent UnitOpCalculationStarted(form, New System.EventArgs(), objArgs)

                form.ProcessScripts(Script.EventType.ObjectCalculationStarted, Script.ObjectType.FlowsheetObject, objArgs.Nome)

                Select Case objArgs.Tipo
                    Case TipoObjeto.MaterialStream
                        Dim myObj As DWSIM.SimulationObjects.Streams.MaterialStream = form.Collections.CLCS_MaterialStreamCollection(objArgs.Nome)
                        Dim gobj As GraphicObject = myObj.GraphicObject
                        If Not gobj Is Nothing Then
                            If gobj.OutputConnectors(0).IsAttached = True Then
                                Dim myUnitOp As SimulationObjects_UnitOpBaseClass
                                myUnitOp = form.Collections.ObjectCollection(myObj.GraphicObject.OutputConnectors(0).AttachedConnector.AttachedTo.Name)
                                If objArgs.Emissor = "Spec" Or objArgs.Emissor = "FlowsheetSolver" Then
                                    CalculateMaterialStream(form, myObj, , OnlyMe)
                                    myObj.UpdatePropertyNodes(form.Options.SelectedUnitSystem, form.Options.NumberFormat)
                                Else
                                    If objArgs.Calculado = True Then
                                        Dim gObjA As GraphicObject = Nothing
                                        Try
                                            gobj = myUnitOp.GraphicObject
                                            gobj.Calculated = True
                                            preLab = form.FormSurface.LabelCalculator.Text
                                            form.UpdateStatusLabel(DWSIM.App.GetLocalString("Calculando") & " " & gobj.Tag & "... (PP: " & myObj.PropertyPackage.Tag & " [" & myObj.PropertyPackage.ComponentName & "])")
                                            Dim imgA = My.Resources.green_down
                                            If Not imgA Is Nothing Then
                                                Dim myEmbeddedImage As New EmbeddedImageGraphic(gobj.X + gobj.Width / 2 - 8, gobj.Y - 18, imgA)
                                                gObjA = myEmbeddedImage
                                                gObjA.AutoSize = False
                                                gObjA.Height = 16
                                                gObjA.Width = 16
                                                gobj.Status = Status.Calculating
                                            End If
                                            form.FormSurface.FlowsheetDesignSurface.drawingObjects.Add(gObjA)
                                            form.FormSurface.FlowsheetDesignSurface.Invalidate()
                                            myUnitOp.Solve()
                                            gobj.Status = Status.Calculated
                                            If myUnitOp.IsSpecAttached = True And myUnitOp.SpecVarType = DWSIM.SimulationObjects.SpecialOps.Helpers.Spec.TipoVar.Fonte Then form.Collections.CLCS_SpecCollection(myUnitOp.AttachedSpecId).Calculate()
                                            form.WriteToLog(gobj.Tag & ": " & DWSIM.App.GetLocalString("Calculadocomsucesso"), Color.DarkGreen, DWSIM.FormClasses.TipoAviso.Informacao)
                                            form.UpdateStatusLabel(preLab)
                                        Catch ex As Exception
                                            form.ProcessScripts(Script.EventType.ObjectCalculationError, Script.ObjectType.FlowsheetObject, objArgs.Nome)
                                            form.UpdateStatusLabel(preLab)
                                            myUnitOp.ErrorMessage = ex.Message
                                            Dim st As New StackTrace(ex, True)
                                            If st.FrameCount > 0 Then
                                                form.WriteToLog(gobj.Tag & ": " & ex.Message & " (" & Path.GetFileName(st.GetFrame(0).GetFileName) & ", " & st.GetFrame(0).GetFileLineNumber & ")", Color.Red, FormClasses.TipoAviso.Erro)
                                            Else
                                                form.WriteToLog(gobj.Tag & ": " & ex.Message.ToString, Color.Red, FormClasses.TipoAviso.Erro)
                                            End If
                                            gobj = myUnitOp.GraphicObject
                                            gobj.Calculated = False
                                            myUnitOp.Unsolve()
                                        Finally
                                            form.FormSurface.FlowsheetDesignSurface.drawingObjects.Remove(gObjA)
                                            form.FormSurface.FlowsheetDesignSurface.Invalidate()
                                        End Try
                                    Else
                                        myUnitOp.Unsolve()
                                        gobj = myUnitOp.GraphicObject
                                        gobj.Calculated = False
                                    End If
                                End If
                            End If
                            form.FormSurface.Refresh()
                        End If
                    Case TipoObjeto.EnergyStream
                        Dim myObj As DWSIM.SimulationObjects.Streams.EnergyStream = form.Collections.CLCS_EnergyStreamCollection(objArgs.Nome)
                        Dim gobj As GraphicObject = myObj.GraphicObject
                        If Not gobj Is Nothing Then
                            If gobj.OutputConnectors(0).IsAttached = True And Not OnlyMe Then
                                Dim myUnitOp As SimulationObjects_UnitOpBaseClass
                                myUnitOp = form.Collections.ObjectCollection(myObj.GraphicObject.OutputConnectors(0).AttachedConnector.AttachedTo.Name)
                                If objArgs.Calculado = True Then
                                    Try
                                        preLab = form.FormSurface.LabelCalculator.Text
                                        myUnitOp.GraphicObject.Calculated = False
                                        form.UpdateStatusLabel(DWSIM.App.GetLocalString("Calculando") & " " & gobj.Tag & "... (PP: " & myUnitOp.PropertyPackage.Tag & " [" & myUnitOp.PropertyPackage.ComponentName & "])")
                                        myUnitOp.Solve()
                                        myUnitOp.UpdatePropertyNodes(form.Options.SelectedUnitSystem, form.Options.NumberFormat)
                                        form.WriteToLog(gobj.Tag & ": " & DWSIM.App.GetLocalString("Calculadocomsucesso"), Color.DarkGreen, DWSIM.FormClasses.TipoAviso.Informacao)
                                        myUnitOp.GraphicObject.Calculated = True
                                        If myUnitOp.IsSpecAttached = True And myUnitOp.SpecVarType = DWSIM.SimulationObjects.SpecialOps.Helpers.Spec.TipoVar.Fonte Then form.Collections.CLCS_SpecCollection(myUnitOp.AttachedSpecId).Calculate()
                                        form.UpdateStatusLabel(preLab)
                                        gobj = myUnitOp.GraphicObject
                                        gobj.Calculated = True
                                    Catch ex As Exception
                                        form.ProcessScripts(Script.EventType.ObjectCalculationError, Script.ObjectType.FlowsheetObject, objArgs.Nome)
                                        form.UpdateStatusLabel(preLab)
                                        myUnitOp.GraphicObject.Calculated = False
                                        myUnitOp.ErrorMessage = ex.Message
                                        Dim st As New StackTrace(ex, True)
                                        If st.FrameCount > 0 Then
                                            form.WriteToLog(gobj.Tag & ": " & ex.Message & " (" & Path.GetFileName(st.GetFrame(0).GetFileName) & ", " & st.GetFrame(0).GetFileLineNumber & ")", Color.Red, FormClasses.TipoAviso.Erro)
                                        Else
                                            form.WriteToLog(gobj.Tag & ": " & ex.Message.ToString, Color.Red, FormClasses.TipoAviso.Erro)
                                        End If
                                        gobj = myUnitOp.GraphicObject
                                        gobj.Calculated = False
                                        myUnitOp.Unsolve()
                                        myUnitOp.UpdatePropertyNodes(form.Options.SelectedUnitSystem, form.Options.NumberFormat)
                                    Finally
                                    End Try
                                Else
                                    myUnitOp.Unsolve()
                                    myUnitOp.GraphicObject.Calculated = False
                                End If
                                myObj.UpdatePropertyNodes(form.Options.SelectedUnitSystem, form.Options.NumberFormat)
                                form.FormSurface.Refresh()
                            End If
                        End If
                    Case Else
                        If objArgs.Emissor = "PropertyGrid" Or objArgs.Emissor = "Adjust" Or objArgs.Emissor = "FlowsheetSolver" Then
                            Dim myObj As SimulationObjects_UnitOpBaseClass = form.Collections.ObjectCollection(objArgs.Nome)
                            myObj.GraphicObject.Calculated = False
                            form.UpdateStatusLabel(DWSIM.App.GetLocalString("Calculando") & " " & myObj.GraphicObject.Tag & "... (PP: " & myObj.PropertyPackage.Tag & " [" & myObj.PropertyPackage.ComponentName & "])")
                            Try
                                myObj.Solve()
                                form.WriteToLog(objArgs.Tag & ": " & DWSIM.App.GetLocalString("Calculadocomsucesso"), Color.DarkGreen, DWSIM.FormClasses.TipoAviso.Informacao)
                                myObj.GraphicObject.Calculated = True
                                If myObj.IsSpecAttached = True And myObj.SpecVarType = DWSIM.SimulationObjects.SpecialOps.Helpers.Spec.TipoVar.Fonte Then form.Collections.CLCS_SpecCollection(myObj.AttachedSpecId).Calculate()
                                form.FormProps.PGEx1.Refresh()
                                myObj.UpdatePropertyNodes(form.Options.SelectedUnitSystem, form.Options.NumberFormat)
                            Catch ex As Exception
                                form.ProcessScripts(Script.EventType.ObjectCalculationError, Script.ObjectType.FlowsheetObject, objArgs.Nome)
                                Dim st As New StackTrace(ex, True)
                                If st.FrameCount > 0 Then
                                    form.WriteToLog(myObj.GraphicObject.Tag & ": " & ex.Message & " (" & Path.GetFileName(st.GetFrame(0).GetFileName) & ", " & st.GetFrame(0).GetFileLineNumber & ")", Color.Red, FormClasses.TipoAviso.Erro)
                                Else
                                    form.WriteToLog(myObj.GraphicObject.Tag & ": " & ex.Message.ToString, Color.Red, FormClasses.TipoAviso.Erro)
                                End If
                           End Try
                        Else
                            Dim myObj As SimulationObjects_UnitOpBaseClass = form.Collections.ObjectCollection(objArgs.Nome)
                            Dim gobj As GraphicObject = FormFlowsheet.SearchSurfaceObjectsByName(objArgs.Nome, form.FormSurface.FlowsheetDesignSurface)
                            If Not OnlyMe Then
                                For Each cp As ConnectionPoint In gobj.OutputConnectors
                                    If cp.IsAttached And cp.Type = ConType.ConOut Then
                                        Dim obj As SimulationObjects_BaseClass = form.Collections.ObjectCollection(cp.AttachedConnector.AttachedTo.Name)
                                        If TypeOf obj Is Streams.MaterialStream Then
                                            Dim ms As Streams.MaterialStream = CType(obj, Streams.MaterialStream)
                                            Try
                                                ms.GraphicObject.Calculated = False
                                                form.UpdateStatusLabel(DWSIM.App.GetLocalString("Calculando") & " " & ms.GraphicObject.Tag & "... (PP: " & ms.PropertyPackage.Tag & " [" & ms.PropertyPackage.ComponentName & "])")
                                                CalculateMaterialStream(form, ms)
                                                ms.GraphicObject.Calculated = True
                                            Catch ex As Exception
                                                form.ProcessScripts(Script.EventType.ObjectCalculationError, Script.ObjectType.FlowsheetObject, ms.Name)
                                                ms.GraphicObject.Calculated = False
                                                ms.Clear()
                                                ms.ClearAllProps()
                                                ms.UpdatePropertyNodes(form.Options.SelectedUnitSystem, form.Options.NumberFormat)
                                                Dim st As New StackTrace(ex, True)
                                                If st.FrameCount > 0 Then
                                                    form.WriteToLog(gobj.Tag & ": " & ex.Message & " (" & Path.GetFileName(st.GetFrame(0).GetFileName) & ", " & st.GetFrame(0).GetFileLineNumber & ")", Color.Red, FormClasses.TipoAviso.Erro)
                                                Else
                                                    form.WriteToLog(gobj.Tag & ": " & ex.Message.ToString, Color.Red, FormClasses.TipoAviso.Erro)
                                                End If
                                            End Try
                                        End If
                                    End If
                                Next
                            End If
                            myObj.UpdatePropertyNodes(form.Options.SelectedUnitSystem, form.Options.NumberFormat)
                        End If
                End Select

                form.ProcessScripts(Script.EventType.ObjectCalculationFinished, Script.ObjectType.FlowsheetObject, objArgs.Nome)

                RaiseEvent UnitOpCalculationFinished(form, New System.EventArgs(), objArgs)

            End If

            Application.DoEvents()

            form.FormSurface.LabelCalculator.Text = preLab

        End Sub

        Public Shared Sub CalculateFlowsheetAsync(ByVal form As FormFlowsheet, ByVal objArgs As DWSIM.Outros.StatusChangeEventArgs, ByVal sender As Object, Optional ByVal OnlyMe As Boolean = False)

            If objArgs.Emissor = "FlowsheetSolver" Then
                form.ProcessScripts(Script.EventType.ObjectCalculationStarted, Script.ObjectType.FlowsheetObject, objArgs.Nome)
                Select Case objArgs.Tipo
                    Case TipoObjeto.MaterialStream
                        Dim myObj As DWSIM.SimulationObjects.Streams.MaterialStream = form.Collections.CLCS_MaterialStreamCollection(objArgs.Nome)
                        RaiseEvent MaterialStreamCalculationStarted(form, New System.EventArgs(), myObj)
                        CalculateMaterialStreamAsync(form, myObj, , OnlyMe)
                        RaiseEvent MaterialStreamCalculationFinished(form, New System.EventArgs(), myObj)
                    Case TipoObjeto.EnergyStream
                    Case Else
                        RaiseEvent UnitOpCalculationStarted(form, New System.EventArgs(), objArgs)
                        Dim myObj As SimulationObjects_UnitOpBaseClass = form.Collections.ObjectCollection(objArgs.Nome)
                        myObj.Solve()
                        RaiseEvent UnitOpCalculationFinished(form, New System.EventArgs(), objArgs)
                End Select
                form.ProcessScripts(Script.EventType.ObjectCalculationFinished, Script.ObjectType.FlowsheetObject, objArgs.Nome)
            End If

        End Sub

        ''' <summary>
        ''' Material Stream calculation routine 1. This routine check all input values and calculates all remaining properties of the stream.
        ''' </summary>
        ''' <param name="form">Flowsheet to what the stream belongs to.</param>
        ''' <param name="ms">Material Stream object to be calculated.</param>
        ''' <param name="DoNotCalcFlash">Tells the calculator whether to do flash calculations or not.</param>
        ''' <remarks></remarks>
        Public Shared Sub CalculateMaterialStream(ByVal form As FormFlowsheet, ByVal ms As DWSIM.SimulationObjects.Streams.MaterialStream, Optional ByVal DoNotCalcFlash As Boolean = False, Optional ByVal OnlyMe As Boolean = False)

            ms.Calculated = False

            RaiseEvent MaterialStreamCalculationStarted(form, New System.EventArgs(), ms)

            form.ProcessScripts(Script.EventType.ObjectCalculationStarted, Script.ObjectType.FlowsheetObject, ms.Nome)

            If My.Settings.EnableGPUProcessing Then DWSIM.App.InitComputeDevice()

            Dim doparallel As Boolean = My.Settings.EnableParallelProcessing

            Dim preLab As String = form.FormSurface.LabelCalculator.Text

            Try
                form.UpdateStatusLabel(DWSIM.App.GetLocalString("Calculando") & " " & ms.GraphicObject.Tag & "... (PP: " & ms.PropertyPackage.Tag & " [" & ms.PropertyPackage.ComponentName & "])")
            Catch ex As Exception
                form.UpdateStatusLabel(DWSIM.App.GetLocalString("Calculando") & "...")
            End Try

            Dim sobj As Microsoft.Msdn.Samples.GraphicObjects.GraphicObject = form.Collections.MaterialStreamCollection(ms.Nome)

            Dim T As Double = ms.Fases(0).SPMProperties.temperature.GetValueOrDefault
            Dim P As Double = ms.Fases(0).SPMProperties.pressure.GetValueOrDefault
            Dim W As Nullable(Of Double) = ms.Fases(0).SPMProperties.massflow
            Dim Q As Nullable(Of Double) = ms.Fases(0).SPMProperties.molarflow
            Dim QV As Nullable(Of Double) = ms.Fases(0).SPMProperties.volumetric_flow
            Dim H As Double = ms.Fases(0).SPMProperties.enthalpy.GetValueOrDefault

            Dim subs As DWSIM.ClassesBasicasTermodinamica.Substancia
            Dim comp As Double = 0
            For Each subs In ms.Fases(0).Componentes.Values
                comp += subs.FracaoMolar.GetValueOrDefault
            Next

            Dim calculated As Boolean

            If W.HasValue And comp >= 0 Then
                With ms.PropertyPackage
                    .CurrentMaterialStream = ms
                    .DW_CalcVazaoMolar()
                    If DoNotCalcFlash Then
                        'do not do a flash calculation...
                    ElseIf form.Options.SempreCalcularFlashPH And H <> 0 Then
                        Try
                            .DW_CalcEquilibrium(DWSIM.SimulationObjects.PropertyPackages.FlashSpec.P, DWSIM.SimulationObjects.PropertyPackages.FlashSpec.H)
                        Catch ex As Exception
                            Dim st As New StackTrace(ex, True)
                            If st.FrameCount > 0 Then
                                form.WriteToLog(ms.GraphicObject.Tag & ": " & ex.Message & " (" & Path.GetFileName(st.GetFrame(0).GetFileName) & ", " & st.GetFrame(0).GetFileLineNumber & ")", Color.Red, FormClasses.TipoAviso.Erro)
                            Else
                                form.WriteToLog(ms.GraphicObject.Tag & ": " & ex.Message.ToString, Color.Red, FormClasses.TipoAviso.Erro)
                            End If
                        End Try
                    Else
                        If .AUX_IS_SINGLECOMP(PropertyPackages.Fase.Mixture) Or .IsElectrolytePP Then
                            If ms.GraphicObject.InputConnectors(0).IsAttached Then
                                Try
                                    .DW_CalcEquilibrium(DWSIM.SimulationObjects.PropertyPackages.FlashSpec.P, DWSIM.SimulationObjects.PropertyPackages.FlashSpec.H)
                                Catch ex As Exception
                                    form.ProcessScripts(Script.EventType.ObjectCalculationError, Script.ObjectType.FlowsheetObject, ms.Name)
                                    form.WriteToLog(ms.GraphicObject.Tag & ": " & ex.ToString, Color.Red, FormClasses.TipoAviso.Erro)
                                End Try
                            Else
                                Try
                                    Select Case ms.SpecType
                                        Case SimulationObjects.Streams.MaterialStream.Flashspec.Temperature_and_Pressure
                                            .DW_CalcEquilibrium(DWSIM.SimulationObjects.PropertyPackages.FlashSpec.T, DWSIM.SimulationObjects.PropertyPackages.FlashSpec.P)
                                        Case SimulationObjects.Streams.MaterialStream.Flashspec.Pressure_and_Enthalpy
                                            .DW_CalcEquilibrium(DWSIM.SimulationObjects.PropertyPackages.FlashSpec.P, DWSIM.SimulationObjects.PropertyPackages.FlashSpec.H)
                                        Case SimulationObjects.Streams.MaterialStream.Flashspec.Pressure_and_Entropy
                                            .DW_CalcEquilibrium(DWSIM.SimulationObjects.PropertyPackages.FlashSpec.P, DWSIM.SimulationObjects.PropertyPackages.FlashSpec.S)
                                        Case SimulationObjects.Streams.MaterialStream.Flashspec.Pressure_and_VaporFraction
                                            .DW_CalcEquilibrium(DWSIM.SimulationObjects.PropertyPackages.FlashSpec.P, DWSIM.SimulationObjects.PropertyPackages.FlashSpec.VAP)
                                        Case SimulationObjects.Streams.MaterialStream.Flashspec.Temperature_and_VaporFraction
                                            .DW_CalcEquilibrium(DWSIM.SimulationObjects.PropertyPackages.FlashSpec.T, DWSIM.SimulationObjects.PropertyPackages.FlashSpec.VAP)
                                    End Select
                                Catch ex As Exception
                                    form.ProcessScripts(Script.EventType.ObjectCalculationError, Script.ObjectType.FlowsheetObject, ms.Name)
                                    Dim st As New StackTrace(ex, True)
                                    If st.FrameCount > 0 Then
                                        form.WriteToLog(ms.GraphicObject.Tag & ": " & ex.Message & " (" & Path.GetFileName(st.GetFrame(0).GetFileName) & ", " & st.GetFrame(0).GetFileLineNumber & ")", Color.Red, FormClasses.TipoAviso.Erro)
                                    Else
                                        form.WriteToLog(ms.GraphicObject.Tag & ": " & ex.Message.ToString, Color.Red, FormClasses.TipoAviso.Erro)
                                    End If
                                End Try
                            End If
                        Else
                            Try
                                Select Case ms.SpecType
                                    Case SimulationObjects.Streams.MaterialStream.Flashspec.Temperature_and_Pressure
                                        .DW_CalcEquilibrium(DWSIM.SimulationObjects.PropertyPackages.FlashSpec.T, DWSIM.SimulationObjects.PropertyPackages.FlashSpec.P)
                                    Case SimulationObjects.Streams.MaterialStream.Flashspec.Pressure_and_Enthalpy
                                        .DW_CalcEquilibrium(DWSIM.SimulationObjects.PropertyPackages.FlashSpec.P, DWSIM.SimulationObjects.PropertyPackages.FlashSpec.H)
                                    Case SimulationObjects.Streams.MaterialStream.Flashspec.Pressure_and_Entropy
                                        .DW_CalcEquilibrium(DWSIM.SimulationObjects.PropertyPackages.FlashSpec.P, DWSIM.SimulationObjects.PropertyPackages.FlashSpec.S)
                                    Case SimulationObjects.Streams.MaterialStream.Flashspec.Pressure_and_VaporFraction
                                        .DW_CalcEquilibrium(DWSIM.SimulationObjects.PropertyPackages.FlashSpec.P, DWSIM.SimulationObjects.PropertyPackages.FlashSpec.VAP)
                                    Case SimulationObjects.Streams.MaterialStream.Flashspec.Temperature_and_VaporFraction
                                        .DW_CalcEquilibrium(DWSIM.SimulationObjects.PropertyPackages.FlashSpec.T, DWSIM.SimulationObjects.PropertyPackages.FlashSpec.VAP)
                                End Select
                            Catch ex As Exception
                                form.ProcessScripts(Script.EventType.ObjectCalculationError, Script.ObjectType.FlowsheetObject, ms.Name)
                                Dim st As New StackTrace(ex, True)
                                If st.FrameCount > 0 Then
                                    form.WriteToLog(ms.GraphicObject.Tag & ": " & ex.Message & " (" & Path.GetFileName(st.GetFrame(0).GetFileName) & ", " & st.GetFrame(0).GetFileLineNumber & ")", Color.Red, FormClasses.TipoAviso.Erro)
                                Else
                                    form.WriteToLog(ms.GraphicObject.Tag & ": " & ex.Message.ToString, Color.Red, FormClasses.TipoAviso.Erro)
                                End If
                            End Try
                        End If
                    End If
                    If doparallel Then
                        My.MyApplication.IsRunningParallelTasks = True
                        If My.Settings.EnableGPUProcessing Then
                            My.MyApplication.gpu.EnableMultithreading()
                        End If
                        Try
                            Dim task1 As Task = New Task(Sub()
                                                             If ms.Fases(3).SPMProperties.molarfraction.GetValueOrDefault > 0 Then
                                                                 .DW_CalcPhaseProps(DWSIM.SimulationObjects.PropertyPackages.Fase.Liquid1)
                                                             Else
                                                                 .DW_ZerarPhaseProps(DWSIM.SimulationObjects.PropertyPackages.Fase.Liquid1)
                                                             End If
                                                         End Sub)
                            Dim task2 As Task = New Task(Sub()
                                                             If ms.Fases(4).SPMProperties.molarfraction.GetValueOrDefault > 0 Then
                                                                 .DW_CalcPhaseProps(DWSIM.SimulationObjects.PropertyPackages.Fase.Liquid2)
                                                             Else
                                                                 .DW_ZerarPhaseProps(DWSIM.SimulationObjects.PropertyPackages.Fase.Liquid2)
                                                             End If
                                                         End Sub)
                            Dim task3 As Task = New Task(Sub()
                                                             If ms.Fases(5).SPMProperties.molarfraction.GetValueOrDefault > 0 Then
                                                                 .DW_CalcPhaseProps(DWSIM.SimulationObjects.PropertyPackages.Fase.Liquid3)
                                                             Else
                                                                 .DW_ZerarPhaseProps(DWSIM.SimulationObjects.PropertyPackages.Fase.Liquid3)
                                                             End If
                                                         End Sub)
                            Dim task4 As Task = New Task(Sub()
                                                             If ms.Fases(6).SPMProperties.molarfraction.GetValueOrDefault > 0 Then
                                                                 .DW_CalcPhaseProps(DWSIM.SimulationObjects.PropertyPackages.Fase.Aqueous)
                                                             Else
                                                                 .DW_ZerarPhaseProps(DWSIM.SimulationObjects.PropertyPackages.Fase.Aqueous)
                                                             End If
                                                         End Sub)
                            Dim task5 As Task = New Task(Sub()
                                                             If ms.Fases(7).SPMProperties.molarfraction.GetValueOrDefault > 0 Then
                                                                 .DW_CalcSolidPhaseProps()
                                                             Else
                                                                 .DW_ZerarPhaseProps(DWSIM.SimulationObjects.PropertyPackages.Fase.Solid)
                                                             End If
                                                         End Sub)
                            Dim task6 As Task = New Task(Sub()
                                                             If ms.Fases(2).SPMProperties.molarfraction.GetValueOrDefault > 0 Then
                                                                 .DW_CalcPhaseProps(DWSIM.SimulationObjects.PropertyPackages.Fase.Vapor)
                                                             Else
                                                                 .DW_ZerarPhaseProps(DWSIM.SimulationObjects.PropertyPackages.Fase.Vapor)
                                                             End If
                                                         End Sub)
                            Select Case My.Settings.MaxDegreeOfParallelism
                                Case 1
                                    task1.Start()
                                    task1.Wait()
                                    task2.Start()
                                    task2.Wait()
                                    task3.Start()
                                    task3.Wait()
                                    task4.Start()
                                    task4.Wait()
                                    task5.Start()
                                    task5.Wait()
                                    task6.Start()
                                    task6.Wait()
                                Case 2
                                    task1.Start()
                                    task2.Start()
                                    Task.WaitAll(task1, task2)
                                    task3.Start()
                                    task4.Start()
                                    Task.WaitAll(task3, task4)
                                    task5.Start()
                                    task6.Start()
                                    Task.WaitAll(task5, task6)
                                Case 3
                                    task1.Start()
                                    task2.Start()
                                    task3.Start()
                                    Task.WaitAll(task1, task2, task3)
                                    task4.Start()
                                    task5.Start()
                                    task6.Start()
                                    Task.WaitAll(task4, task5, task6)
                                Case Else
                                    task1.Start()
                                    task2.Start()
                                    task3.Start()
                                    task4.Start()
                                    task5.Start()
                                    task6.Start()
                                    Task.WaitAll(task1, task2, task3, task4, task5, task6)
                            End Select
                        Catch ae As AggregateException
                            form.ProcessScripts(Script.EventType.ObjectCalculationError, Script.ObjectType.FlowsheetObject, ms.Name)
                            Throw ae.Flatten().InnerException
                        Finally
                            If My.Settings.EnableGPUProcessing Then
                                My.MyApplication.gpu.DisableMultithreading()
                                My.MyApplication.gpu.FreeAll()
                            End If
                        End Try
                        My.MyApplication.IsRunningParallelTasks = False
                    Else
                        If ms.Fases(3).SPMProperties.molarfraction.GetValueOrDefault > 0 Then
                            .DW_CalcPhaseProps(DWSIM.SimulationObjects.PropertyPackages.Fase.Liquid1)
                        Else
                            .DW_ZerarPhaseProps(DWSIM.SimulationObjects.PropertyPackages.Fase.Liquid1)
                        End If
                        If ms.Fases(4).SPMProperties.molarfraction.GetValueOrDefault > 0 Then
                            .DW_CalcPhaseProps(DWSIM.SimulationObjects.PropertyPackages.Fase.Liquid2)
                        Else
                            .DW_ZerarPhaseProps(DWSIM.SimulationObjects.PropertyPackages.Fase.Liquid2)
                        End If
                        If ms.Fases(5).SPMProperties.molarfraction.GetValueOrDefault > 0 Then
                            .DW_CalcPhaseProps(DWSIM.SimulationObjects.PropertyPackages.Fase.Liquid3)
                        Else
                            .DW_ZerarPhaseProps(DWSIM.SimulationObjects.PropertyPackages.Fase.Liquid3)
                        End If
                        If ms.Fases(6).SPMProperties.molarfraction.GetValueOrDefault > 0 Then
                            .DW_CalcPhaseProps(DWSIM.SimulationObjects.PropertyPackages.Fase.Aqueous)
                        Else
                            .DW_ZerarPhaseProps(DWSIM.SimulationObjects.PropertyPackages.Fase.Aqueous)
                        End If
                        If ms.Fases(7).SPMProperties.molarfraction.GetValueOrDefault > 0 Then
                            .DW_CalcSolidPhaseProps()
                        Else
                            .DW_ZerarPhaseProps(DWSIM.SimulationObjects.PropertyPackages.Fase.Solid)
                        End If
                        If ms.Fases(2).SPMProperties.molarfraction.GetValueOrDefault > 0 Then
                            .DW_CalcPhaseProps(DWSIM.SimulationObjects.PropertyPackages.Fase.Vapor)
                        Else
                            .DW_ZerarPhaseProps(DWSIM.SimulationObjects.PropertyPackages.Fase.Vapor)
                        End If
                    End If
                    If ms.Fases(2).SPMProperties.molarfraction.GetValueOrDefault >= 0 And ms.Fases(2).SPMProperties.molarfraction.GetValueOrDefault <= 1 Then
                        .DW_CalcPhaseProps(DWSIM.SimulationObjects.PropertyPackages.Fase.Liquid)
                    Else
                        .DW_ZerarPhaseProps(DWSIM.SimulationObjects.PropertyPackages.Fase.Liquid)
                    End If
                    .DW_CalcCompMolarFlow(-1)
                    .DW_CalcCompMassFlow(-1)
                    .DW_CalcCompVolFlow(-1)
                    .DW_CalcOverallProps()
                    .DW_CalcTwoPhaseProps(DWSIM.SimulationObjects.PropertyPackages.Fase.Liquid, DWSIM.SimulationObjects.PropertyPackages.Fase.Vapor)
                    .DW_CalcVazaoVolumetrica()
                    .DW_CalcKvalue()
                    .CurrentMaterialStream = Nothing
                    ms.UpdatePropertyNodes(form.Options.SelectedUnitSystem, form.Options.NumberFormat)
                    If ms.IsSpecAttached = True And ms.SpecVarType = DWSIM.SimulationObjects.SpecialOps.Helpers.Spec.TipoVar.Fonte Then form.Collections.CLCS_SpecCollection(ms.AttachedSpecId).Calculate()
                End With
                sobj.Calculated = True
                form.UpdateStatusLabel(preLab)
                calculated = True
            ElseIf Q.HasValue And comp >= 0 Then
                With ms.PropertyPackage
                    .CurrentMaterialStream = ms
                    .DW_CalcVazaoMassica()
                    If DoNotCalcFlash Then
                        'do not do a flash calculation...
                    ElseIf form.Options.SempreCalcularFlashPH And H <> 0 Then
                        Try
                            .DW_CalcEquilibrium(DWSIM.SimulationObjects.PropertyPackages.FlashSpec.P, DWSIM.SimulationObjects.PropertyPackages.FlashSpec.H)
                        Catch ex As Exception
                            form.ProcessScripts(Script.EventType.ObjectCalculationError, Script.ObjectType.FlowsheetObject, ms.Name)
                            Dim st As New StackTrace(ex, True)
                            If st.FrameCount > 0 Then
                                form.WriteToLog(ms.GraphicObject.Tag & ": " & ex.Message & " (" & Path.GetFileName(st.GetFrame(0).GetFileName) & ", " & st.GetFrame(0).GetFileLineNumber & ")", Color.Red, FormClasses.TipoAviso.Erro)
                            Else
                                form.WriteToLog(ms.GraphicObject.Tag & ": " & ex.Message.ToString, Color.Red, FormClasses.TipoAviso.Erro)
                            End If
                        End Try
                    Else
                        If .AUX_IS_SINGLECOMP(PropertyPackages.Fase.Mixture) Or .IsElectrolytePP Then
                            If ms.GraphicObject.InputConnectors(0).IsAttached Then
                                Try
                                    .DW_CalcEquilibrium(DWSIM.SimulationObjects.PropertyPackages.FlashSpec.P, DWSIM.SimulationObjects.PropertyPackages.FlashSpec.H)
                                Catch ex As Exception
                                    form.ProcessScripts(Script.EventType.ObjectCalculationError, Script.ObjectType.FlowsheetObject, ms.Name)
                                    Dim st As New StackTrace(ex, True)
                                    If st.FrameCount > 0 Then
                                        form.WriteToLog(ms.GraphicObject.Tag & ": " & ex.Message & " (" & Path.GetFileName(st.GetFrame(0).GetFileName) & ", " & st.GetFrame(0).GetFileLineNumber & ")", Color.Red, FormClasses.TipoAviso.Erro)
                                    Else
                                        form.WriteToLog(ms.GraphicObject.Tag & ": " & ex.Message.ToString, Color.Red, FormClasses.TipoAviso.Erro)
                                    End If
                                End Try
                            Else
                                Try
                                    .DW_CalcEquilibrium(DWSIM.SimulationObjects.PropertyPackages.FlashSpec.T, DWSIM.SimulationObjects.PropertyPackages.FlashSpec.P)
                                Catch ex As Exception
                                    form.ProcessScripts(Script.EventType.ObjectCalculationError, Script.ObjectType.FlowsheetObject, ms.Name)
                                    Dim st As New StackTrace(ex, True)
                                    If st.FrameCount > 0 Then
                                        form.WriteToLog(ms.GraphicObject.Tag & ": " & ex.Message & " (" & Path.GetFileName(st.GetFrame(0).GetFileName) & ", " & st.GetFrame(0).GetFileLineNumber & ")", Color.Red, FormClasses.TipoAviso.Erro)
                                    Else
                                        form.WriteToLog(ms.GraphicObject.Tag & ": " & ex.Message.ToString, Color.Red, FormClasses.TipoAviso.Erro)
                                    End If
                                End Try
                            End If
                        Else
                            Try
                                Select Case ms.SpecType
                                    Case SimulationObjects.Streams.MaterialStream.Flashspec.Temperature_and_Pressure
                                        .DW_CalcEquilibrium(DWSIM.SimulationObjects.PropertyPackages.FlashSpec.T, DWSIM.SimulationObjects.PropertyPackages.FlashSpec.P)
                                    Case SimulationObjects.Streams.MaterialStream.Flashspec.Pressure_and_Enthalpy
                                        .DW_CalcEquilibrium(DWSIM.SimulationObjects.PropertyPackages.FlashSpec.P, DWSIM.SimulationObjects.PropertyPackages.FlashSpec.H)
                                    Case SimulationObjects.Streams.MaterialStream.Flashspec.Pressure_and_Entropy
                                        .DW_CalcEquilibrium(DWSIM.SimulationObjects.PropertyPackages.FlashSpec.P, DWSIM.SimulationObjects.PropertyPackages.FlashSpec.S)
                                    Case SimulationObjects.Streams.MaterialStream.Flashspec.Pressure_and_VaporFraction
                                        .DW_CalcEquilibrium(DWSIM.SimulationObjects.PropertyPackages.FlashSpec.P, DWSIM.SimulationObjects.PropertyPackages.FlashSpec.VAP)
                                    Case SimulationObjects.Streams.MaterialStream.Flashspec.Temperature_and_VaporFraction
                                        .DW_CalcEquilibrium(DWSIM.SimulationObjects.PropertyPackages.FlashSpec.T, DWSIM.SimulationObjects.PropertyPackages.FlashSpec.VAP)
                                End Select
                            Catch ex As Exception
                                form.ProcessScripts(Script.EventType.ObjectCalculationError, Script.ObjectType.FlowsheetObject, ms.Name)
                                Dim st As New StackTrace(ex, True)
                                If st.FrameCount > 0 Then
                                    form.WriteToLog(ms.GraphicObject.Tag & ": " & ex.Message & " (" & Path.GetFileName(st.GetFrame(0).GetFileName) & ", " & st.GetFrame(0).GetFileLineNumber & ")", Color.Red, FormClasses.TipoAviso.Erro)
                                Else
                                    form.WriteToLog(ms.GraphicObject.Tag & ": " & ex.Message.ToString, Color.Red, FormClasses.TipoAviso.Erro)
                                End If
                            End Try
                        End If
                    End If
                    If doparallel Then
                        My.MyApplication.IsRunningParallelTasks = True
                        If My.Settings.EnableGPUProcessing Then
                            My.MyApplication.gpu.EnableMultithreading()
                        End If
                        Try
                            Dim task1 As Task = New Task(Sub()
                                                             If ms.Fases(3).SPMProperties.molarfraction.GetValueOrDefault > 0 Then
                                                                 .DW_CalcPhaseProps(DWSIM.SimulationObjects.PropertyPackages.Fase.Liquid1)
                                                             Else
                                                                 .DW_ZerarPhaseProps(DWSIM.SimulationObjects.PropertyPackages.Fase.Liquid1)
                                                             End If
                                                         End Sub)
                            Dim task2 As Task = New Task(Sub()
                                                             If ms.Fases(4).SPMProperties.molarfraction.GetValueOrDefault > 0 Then
                                                                 .DW_CalcPhaseProps(DWSIM.SimulationObjects.PropertyPackages.Fase.Liquid2)
                                                             Else
                                                                 .DW_ZerarPhaseProps(DWSIM.SimulationObjects.PropertyPackages.Fase.Liquid2)
                                                             End If
                                                         End Sub)
                            Dim task3 As Task = New Task(Sub()
                                                             If ms.Fases(5).SPMProperties.molarfraction.GetValueOrDefault > 0 Then
                                                                 .DW_CalcPhaseProps(DWSIM.SimulationObjects.PropertyPackages.Fase.Liquid3)
                                                             Else
                                                                 .DW_ZerarPhaseProps(DWSIM.SimulationObjects.PropertyPackages.Fase.Liquid3)
                                                             End If
                                                         End Sub)
                            Dim task4 As Task = New Task(Sub()
                                                             If ms.Fases(6).SPMProperties.molarfraction.GetValueOrDefault > 0 Then
                                                                 .DW_CalcPhaseProps(DWSIM.SimulationObjects.PropertyPackages.Fase.Aqueous)
                                                             Else
                                                                 .DW_ZerarPhaseProps(DWSIM.SimulationObjects.PropertyPackages.Fase.Aqueous)
                                                             End If
                                                         End Sub)
                            Dim task5 As Task = New Task(Sub()
                                                             If ms.Fases(7).SPMProperties.molarfraction.GetValueOrDefault > 0 Then
                                                                 .DW_CalcSolidPhaseProps()
                                                             Else
                                                                 .DW_ZerarPhaseProps(DWSIM.SimulationObjects.PropertyPackages.Fase.Solid)
                                                             End If
                                                         End Sub)
                            Dim task6 As Task = New Task(Sub()
                                                             If ms.Fases(2).SPMProperties.molarfraction.GetValueOrDefault > 0 Then
                                                                 .DW_CalcPhaseProps(DWSIM.SimulationObjects.PropertyPackages.Fase.Vapor)
                                                             Else
                                                                 .DW_ZerarPhaseProps(DWSIM.SimulationObjects.PropertyPackages.Fase.Vapor)
                                                             End If
                                                         End Sub)
                            Select Case My.Settings.MaxDegreeOfParallelism
                                Case 1
                                    task1.Start()
                                    task1.Wait()
                                    task2.Start()
                                    task2.Wait()
                                    task3.Start()
                                    task3.Wait()
                                    task4.Start()
                                    task4.Wait()
                                    task5.Start()
                                    task5.Wait()
                                    task6.Start()
                                    task6.Wait()
                                Case 2
                                    task1.Start()
                                    task2.Start()
                                    Task.WaitAll(task1, task2)
                                    task3.Start()
                                    task4.Start()
                                    Task.WaitAll(task3, task4)
                                    task5.Start()
                                    task6.Start()
                                    Task.WaitAll(task5, task6)
                                Case 3
                                    task1.Start()
                                    task2.Start()
                                    task3.Start()
                                    Task.WaitAll(task1, task2, task3)
                                    task4.Start()
                                    task5.Start()
                                    task6.Start()
                                    Task.WaitAll(task4, task5, task6)
                                Case Else
                                    task1.Start()
                                    task2.Start()
                                    task3.Start()
                                    task4.Start()
                                    task5.Start()
                                    task6.Start()
                                    Task.WaitAll(task1, task2, task3, task4, task5, task6)
                            End Select
                        Catch ae As AggregateException
                            form.ProcessScripts(Script.EventType.ObjectCalculationError, Script.ObjectType.FlowsheetObject, ms.Name)
                            Throw ae.Flatten().InnerException
                        Finally
                            If My.Settings.EnableGPUProcessing Then
                                My.MyApplication.gpu.DisableMultithreading()
                                My.MyApplication.gpu.FreeAll()
                            End If
                        End Try
                        My.MyApplication.IsRunningParallelTasks = False
                    Else
                        If ms.Fases(3).SPMProperties.molarfraction.GetValueOrDefault > 0 Then
                            .DW_CalcPhaseProps(DWSIM.SimulationObjects.PropertyPackages.Fase.Liquid1)
                        Else
                            .DW_ZerarPhaseProps(DWSIM.SimulationObjects.PropertyPackages.Fase.Liquid1)
                        End If
                        If ms.Fases(4).SPMProperties.molarfraction.GetValueOrDefault > 0 Then
                            .DW_CalcPhaseProps(DWSIM.SimulationObjects.PropertyPackages.Fase.Liquid2)
                        Else
                            .DW_ZerarPhaseProps(DWSIM.SimulationObjects.PropertyPackages.Fase.Liquid2)
                        End If
                        If ms.Fases(5).SPMProperties.molarfraction.GetValueOrDefault > 0 Then
                            .DW_CalcPhaseProps(DWSIM.SimulationObjects.PropertyPackages.Fase.Liquid3)
                        Else
                            .DW_ZerarPhaseProps(DWSIM.SimulationObjects.PropertyPackages.Fase.Liquid3)
                        End If
                        If ms.Fases(6).SPMProperties.molarfraction.GetValueOrDefault > 0 Then
                            .DW_CalcPhaseProps(DWSIM.SimulationObjects.PropertyPackages.Fase.Aqueous)
                        Else
                            .DW_ZerarPhaseProps(DWSIM.SimulationObjects.PropertyPackages.Fase.Aqueous)
                        End If
                        If ms.Fases(7).SPMProperties.molarfraction.GetValueOrDefault > 0 Then
                            .DW_CalcSolidPhaseProps()
                        Else
                            .DW_ZerarPhaseProps(DWSIM.SimulationObjects.PropertyPackages.Fase.Solid)
                        End If
                        If ms.Fases(2).SPMProperties.molarfraction.GetValueOrDefault > 0 Then
                            .DW_CalcPhaseProps(DWSIM.SimulationObjects.PropertyPackages.Fase.Vapor)
                        Else
                            .DW_ZerarPhaseProps(DWSIM.SimulationObjects.PropertyPackages.Fase.Vapor)
                        End If
                    End If
                    If ms.Fases(2).SPMProperties.molarfraction.GetValueOrDefault >= 0 And ms.Fases(2).SPMProperties.molarfraction.GetValueOrDefault <= 1 Then
                        .DW_CalcPhaseProps(DWSIM.SimulationObjects.PropertyPackages.Fase.Liquid)
                    Else
                        .DW_ZerarPhaseProps(DWSIM.SimulationObjects.PropertyPackages.Fase.Liquid)
                    End If
                    .DW_CalcCompMolarFlow(-1)
                    .DW_CalcCompMassFlow(-1)
                    .DW_CalcCompVolFlow(-1)
                    .DW_CalcOverallProps()
                    .DW_CalcTwoPhaseProps(DWSIM.SimulationObjects.PropertyPackages.Fase.Liquid, DWSIM.SimulationObjects.PropertyPackages.Fase.Vapor)
                    .DW_CalcVazaoVolumetrica()
                    .DW_CalcKvalue()
                    .CurrentMaterialStream = Nothing
                    ms.UpdatePropertyNodes(form.Options.SelectedUnitSystem, form.Options.NumberFormat)
                    If ms.IsSpecAttached = True And ms.SpecVarType = DWSIM.SimulationObjects.SpecialOps.Helpers.Spec.TipoVar.Fonte Then form.Collections.CLCS_SpecCollection(ms.AttachedSpecId).Calculate()
                End With
                sobj.Calculated = True
                form.UpdateStatusLabel(preLab)
                calculated = True
            ElseIf QV.HasValue And comp >= 0 Then
                With ms.PropertyPackage
                    .CurrentMaterialStream = ms

                    ms.Fases(0).SPMProperties.molarflow = 1.0#
                    ms.Fases(0).SPMProperties.massflow = 1.0#

                    If DoNotCalcFlash Then
                        'do not do a flash calculation...
                    ElseIf form.Options.SempreCalcularFlashPH And H <> 0 Then
                        Try
                            .DW_CalcEquilibrium(DWSIM.SimulationObjects.PropertyPackages.FlashSpec.P, DWSIM.SimulationObjects.PropertyPackages.FlashSpec.H)
                        Catch ex As Exception
                            form.ProcessScripts(Script.EventType.ObjectCalculationError, Script.ObjectType.FlowsheetObject, ms.Name)
                            Dim st As New StackTrace(ex, True)
                            If st.FrameCount > 0 Then
                                form.WriteToLog(ms.GraphicObject.Tag & ": " & ex.Message & " (" & Path.GetFileName(st.GetFrame(0).GetFileName) & ", " & st.GetFrame(0).GetFileLineNumber & ")", Color.Red, FormClasses.TipoAviso.Erro)
                            Else
                                form.WriteToLog(ms.GraphicObject.Tag & ": " & ex.Message.ToString, Color.Red, FormClasses.TipoAviso.Erro)
                            End If
                        End Try
                    Else
                        If .AUX_IS_SINGLECOMP(PropertyPackages.Fase.Mixture) Or .IsElectrolytePP Then
                            If ms.GraphicObject.InputConnectors(0).IsAttached Then
                                Try
                                    .DW_CalcEquilibrium(DWSIM.SimulationObjects.PropertyPackages.FlashSpec.P, DWSIM.SimulationObjects.PropertyPackages.FlashSpec.H)
                                Catch ex As Exception
                                    form.ProcessScripts(Script.EventType.ObjectCalculationError, Script.ObjectType.FlowsheetObject, ms.Name)
                                    form.WriteToLog(ms.GraphicObject.Tag & ": " & ex.ToString, Color.Red, FormClasses.TipoAviso.Erro)
                                End Try
                            Else
                                Try
                                    .DW_CalcEquilibrium(DWSIM.SimulationObjects.PropertyPackages.FlashSpec.T, DWSIM.SimulationObjects.PropertyPackages.FlashSpec.P)
                                Catch ex As Exception
                                    form.ProcessScripts(Script.EventType.ObjectCalculationError, Script.ObjectType.FlowsheetObject, ms.Name)
                                    Dim st As New StackTrace(ex, True)
                                    If st.FrameCount > 0 Then
                                        form.WriteToLog(ms.GraphicObject.Tag & ": " & ex.Message & " (" & Path.GetFileName(st.GetFrame(0).GetFileName) & ", " & st.GetFrame(0).GetFileLineNumber & ")", Color.Red, FormClasses.TipoAviso.Erro)
                                    Else
                                        form.WriteToLog(ms.GraphicObject.Tag & ": " & ex.Message.ToString, Color.Red, FormClasses.TipoAviso.Erro)
                                    End If
                                End Try
                            End If
                        Else
                            Try
                                Select Case ms.SpecType
                                    Case SimulationObjects.Streams.MaterialStream.Flashspec.Temperature_and_Pressure
                                        .DW_CalcEquilibrium(DWSIM.SimulationObjects.PropertyPackages.FlashSpec.T, DWSIM.SimulationObjects.PropertyPackages.FlashSpec.P)
                                    Case SimulationObjects.Streams.MaterialStream.Flashspec.Pressure_and_Enthalpy
                                        .DW_CalcEquilibrium(DWSIM.SimulationObjects.PropertyPackages.FlashSpec.P, DWSIM.SimulationObjects.PropertyPackages.FlashSpec.H)
                                    Case SimulationObjects.Streams.MaterialStream.Flashspec.Pressure_and_Entropy
                                        .DW_CalcEquilibrium(DWSIM.SimulationObjects.PropertyPackages.FlashSpec.P, DWSIM.SimulationObjects.PropertyPackages.FlashSpec.S)
                                    Case SimulationObjects.Streams.MaterialStream.Flashspec.Pressure_and_VaporFraction
                                        .DW_CalcEquilibrium(DWSIM.SimulationObjects.PropertyPackages.FlashSpec.P, DWSIM.SimulationObjects.PropertyPackages.FlashSpec.VAP)
                                    Case SimulationObjects.Streams.MaterialStream.Flashspec.Temperature_and_VaporFraction
                                        .DW_CalcEquilibrium(DWSIM.SimulationObjects.PropertyPackages.FlashSpec.T, DWSIM.SimulationObjects.PropertyPackages.FlashSpec.VAP)
                                End Select
                            Catch ex As Exception
                                form.ProcessScripts(Script.EventType.ObjectCalculationError, Script.ObjectType.FlowsheetObject, ms.Name)
                                Dim st As New StackTrace(ex, True)
                                If st.FrameCount > 0 Then
                                    form.WriteToLog(ms.GraphicObject.Tag & ": " & ex.Message & " (" & Path.GetFileName(st.GetFrame(0).GetFileName) & ", " & st.GetFrame(0).GetFileLineNumber & ")", Color.Red, FormClasses.TipoAviso.Erro)
                                Else
                                    form.WriteToLog(ms.GraphicObject.Tag & ": " & ex.Message.ToString, Color.Red, FormClasses.TipoAviso.Erro)
                                End If
                            End Try
                        End If
                    End If
                    If doparallel Then
                        My.MyApplication.IsRunningParallelTasks = True
                        If My.Settings.EnableGPUProcessing Then
                            My.MyApplication.gpu.EnableMultithreading()
                        End If
                        Try
                            Dim task1 As Task = New Task(Sub()
                                                             If ms.Fases(3).SPMProperties.molarfraction.GetValueOrDefault > 0 Then
                                                                 .DW_CalcPhaseProps(DWSIM.SimulationObjects.PropertyPackages.Fase.Liquid1)
                                                             Else
                                                                 .DW_ZerarPhaseProps(DWSIM.SimulationObjects.PropertyPackages.Fase.Liquid1)
                                                             End If
                                                         End Sub)
                            Dim task2 As Task = New Task(Sub()
                                                             If ms.Fases(4).SPMProperties.molarfraction.GetValueOrDefault > 0 Then
                                                                 .DW_CalcPhaseProps(DWSIM.SimulationObjects.PropertyPackages.Fase.Liquid2)
                                                             Else
                                                                 .DW_ZerarPhaseProps(DWSIM.SimulationObjects.PropertyPackages.Fase.Liquid2)
                                                             End If
                                                         End Sub)
                            Dim task3 As Task = New Task(Sub()
                                                             If ms.Fases(5).SPMProperties.molarfraction.GetValueOrDefault > 0 Then
                                                                 .DW_CalcPhaseProps(DWSIM.SimulationObjects.PropertyPackages.Fase.Liquid3)
                                                             Else
                                                                 .DW_ZerarPhaseProps(DWSIM.SimulationObjects.PropertyPackages.Fase.Liquid3)
                                                             End If
                                                         End Sub)
                            Dim task4 As Task = New Task(Sub()
                                                             If ms.Fases(6).SPMProperties.molarfraction.GetValueOrDefault > 0 Then
                                                                 .DW_CalcPhaseProps(DWSIM.SimulationObjects.PropertyPackages.Fase.Aqueous)
                                                             Else
                                                                 .DW_ZerarPhaseProps(DWSIM.SimulationObjects.PropertyPackages.Fase.Aqueous)
                                                             End If
                                                         End Sub)
                            Dim task5 As Task = New Task(Sub()
                                                             If ms.Fases(7).SPMProperties.molarfraction.GetValueOrDefault > 0 Then
                                                                 .DW_CalcSolidPhaseProps()
                                                             Else
                                                                 .DW_ZerarPhaseProps(DWSIM.SimulationObjects.PropertyPackages.Fase.Solid)
                                                             End If
                                                         End Sub)
                            Dim task6 As Task = New Task(Sub()
                                                             If ms.Fases(2).SPMProperties.molarfraction.GetValueOrDefault > 0 Then
                                                                 .DW_CalcPhaseProps(DWSIM.SimulationObjects.PropertyPackages.Fase.Vapor)
                                                             Else
                                                                 .DW_ZerarPhaseProps(DWSIM.SimulationObjects.PropertyPackages.Fase.Vapor)
                                                             End If
                                                         End Sub)
                            Select Case My.Settings.MaxDegreeOfParallelism
                                Case 1
                                    task1.Start()
                                    task1.Wait()
                                    task2.Start()
                                    task2.Wait()
                                    task3.Start()
                                    task3.Wait()
                                    task4.Start()
                                    task4.Wait()
                                    task5.Start()
                                    task5.Wait()
                                    task6.Start()
                                    task6.Wait()
                                Case 2
                                    task1.Start()
                                    task2.Start()
                                    Task.WaitAll(task1, task2)
                                    task3.Start()
                                    task4.Start()
                                    Task.WaitAll(task3, task4)
                                    task5.Start()
                                    task6.Start()
                                    Task.WaitAll(task5, task6)
                                Case 3
                                    task1.Start()
                                    task2.Start()
                                    task3.Start()
                                    Task.WaitAll(task1, task2, task3)
                                    task4.Start()
                                    task5.Start()
                                    task6.Start()
                                    Task.WaitAll(task4, task5, task6)
                                Case Else
                                    task1.Start()
                                    task2.Start()
                                    task3.Start()
                                    task4.Start()
                                    task5.Start()
                                    task6.Start()
                                    Task.WaitAll(task1, task2, task3, task4, task5, task6)
                            End Select
                        Catch ae As AggregateException
                            form.ProcessScripts(Script.EventType.ObjectCalculationError, Script.ObjectType.FlowsheetObject, ms.Name)
                            Throw ae.Flatten().InnerException
                        Finally
                            If My.Settings.EnableGPUProcessing Then
                                My.MyApplication.gpu.DisableMultithreading()
                                My.MyApplication.gpu.FreeAll()
                            End If
                        End Try
                        My.MyApplication.IsRunningParallelTasks = False
                    Else
                        If ms.Fases(3).SPMProperties.molarfraction.GetValueOrDefault > 0 Then
                            .DW_CalcPhaseProps(DWSIM.SimulationObjects.PropertyPackages.Fase.Liquid1)
                        Else
                            .DW_ZerarPhaseProps(DWSIM.SimulationObjects.PropertyPackages.Fase.Liquid1)
                        End If
                        If ms.Fases(4).SPMProperties.molarfraction.GetValueOrDefault > 0 Then
                            .DW_CalcPhaseProps(DWSIM.SimulationObjects.PropertyPackages.Fase.Liquid2)
                        Else
                            .DW_ZerarPhaseProps(DWSIM.SimulationObjects.PropertyPackages.Fase.Liquid2)
                        End If
                        If ms.Fases(5).SPMProperties.molarfraction.GetValueOrDefault > 0 Then
                            .DW_CalcPhaseProps(DWSIM.SimulationObjects.PropertyPackages.Fase.Liquid3)
                        Else
                            .DW_ZerarPhaseProps(DWSIM.SimulationObjects.PropertyPackages.Fase.Liquid3)
                        End If
                        If ms.Fases(6).SPMProperties.molarfraction.GetValueOrDefault > 0 Then
                            .DW_CalcPhaseProps(DWSIM.SimulationObjects.PropertyPackages.Fase.Aqueous)
                        Else
                            .DW_ZerarPhaseProps(DWSIM.SimulationObjects.PropertyPackages.Fase.Aqueous)
                        End If
                        If ms.Fases(7).SPMProperties.molarfraction.GetValueOrDefault > 0 Then
                            .DW_CalcSolidPhaseProps()
                        Else
                            .DW_ZerarPhaseProps(DWSIM.SimulationObjects.PropertyPackages.Fase.Solid)
                        End If
                        If ms.Fases(2).SPMProperties.molarfraction.GetValueOrDefault > 0 Then
                            .DW_CalcPhaseProps(DWSIM.SimulationObjects.PropertyPackages.Fase.Vapor)
                        Else
                            .DW_ZerarPhaseProps(DWSIM.SimulationObjects.PropertyPackages.Fase.Vapor)
                        End If
                    End If

                    If ms.Fases(2).SPMProperties.molarfraction.GetValueOrDefault >= 0 And ms.Fases(2).SPMProperties.molarfraction.GetValueOrDefault <= 1 Then
                        .DW_CalcPhaseProps(DWSIM.SimulationObjects.PropertyPackages.Fase.Liquid)
                    Else
                        .DW_ZerarPhaseProps(DWSIM.SimulationObjects.PropertyPackages.Fase.Liquid)
                    End If

                    .DW_CalcOverallProps()

                    ms.Fases(0).SPMProperties.massflow = QV * ms.Fases(0).SPMProperties.density.GetValueOrDefault

                    .DW_CalcVazaoMolar()

                    .DW_CalcCompMolarFlow(-1)
                    .DW_CalcCompMassFlow(-1)
                    .DW_CalcCompVolFlow(-1)
                    .DW_CalcTwoPhaseProps(DWSIM.SimulationObjects.PropertyPackages.Fase.Liquid, DWSIM.SimulationObjects.PropertyPackages.Fase.Vapor)
                    .DW_CalcKvalue()
                    .CurrentMaterialStream = Nothing
                    ms.UpdatePropertyNodes(form.Options.SelectedUnitSystem, form.Options.NumberFormat)
                    If ms.IsSpecAttached = True And ms.SpecVarType = DWSIM.SimulationObjects.SpecialOps.Helpers.Spec.TipoVar.Fonte Then form.Collections.CLCS_SpecCollection(ms.AttachedSpecId).Calculate()
                End With
                sobj.Calculated = True
                form.UpdateStatusLabel(preLab)
                calculated = True
            Else
                With ms.PropertyPackage
                    .CurrentMaterialStream = ms
                    .DW_ZerarVazaoMolar()
                    .DW_ZerarTwoPhaseProps(DWSIM.SimulationObjects.PropertyPackages.Fase.Liquid, DWSIM.SimulationObjects.PropertyPackages.Fase.Vapor)
                    .DW_ZerarPhaseProps(DWSIM.SimulationObjects.PropertyPackages.Fase.Liquid)
                    .DW_ZerarPhaseProps(DWSIM.SimulationObjects.PropertyPackages.Fase.Liquid1)
                    .DW_ZerarPhaseProps(DWSIM.SimulationObjects.PropertyPackages.Fase.Liquid2)
                    .DW_ZerarPhaseProps(DWSIM.SimulationObjects.PropertyPackages.Fase.Liquid3)
                    .DW_ZerarPhaseProps(DWSIM.SimulationObjects.PropertyPackages.Fase.Aqueous)
                    .DW_ZerarPhaseProps(DWSIM.SimulationObjects.PropertyPackages.Fase.Solid)
                    .DW_ZerarPhaseProps(DWSIM.SimulationObjects.PropertyPackages.Fase.Vapor)
                    .DW_ZerarComposicoes(DWSIM.SimulationObjects.PropertyPackages.Fase.Liquid)
                    .DW_ZerarComposicoes(DWSIM.SimulationObjects.PropertyPackages.Fase.Liquid1)
                    .DW_ZerarComposicoes(DWSIM.SimulationObjects.PropertyPackages.Fase.Liquid2)
                    .DW_ZerarComposicoes(DWSIM.SimulationObjects.PropertyPackages.Fase.Liquid3)
                    .DW_ZerarComposicoes(DWSIM.SimulationObjects.PropertyPackages.Fase.Aqueous)
                    .DW_ZerarComposicoes(DWSIM.SimulationObjects.PropertyPackages.Fase.Solid)
                    .DW_ZerarComposicoes(DWSIM.SimulationObjects.PropertyPackages.Fase.Vapor)
                    .DW_ZerarOverallProps()
                    .CurrentMaterialStream = Nothing
                End With
                ms.UpdatePropertyNodes(form.Options.SelectedUnitSystem, form.Options.NumberFormat)
                sobj.Calculated = False
                form.UpdateStatusLabel(preLab)
                calculated = False
            End If

            form.ProcessScripts(Script.EventType.ObjectCalculationFinished, Script.ObjectType.FlowsheetObject, ms.Nome)

            RaiseEvent MaterialStreamCalculationFinished(form, New System.EventArgs(), ms)

            ms.Calculated = calculated

            If Not OnlyMe Then
                Dim objargs As New DWSIM.Outros.StatusChangeEventArgs
                With objargs
                    .Calculado = calculated
                    .Nome = ms.Nome
                    .Tipo = TipoObjeto.MaterialStream
                End With
                CalculateFlowsheet(form, objargs, Nothing)
            End If

        End Sub

        Public Shared Sub CalculateMaterialStreamAsync(ByVal form As FormFlowsheet, ByVal ms As DWSIM.SimulationObjects.Streams.MaterialStream, Optional ByVal DoNotCalcFlash As Boolean = False, Optional ByVal OnlyMe As Boolean = False)

            ms.Calculated = False

            If My.Settings.EnableGPUProcessing Then DWSIM.App.InitComputeDevice()

            Dim doparallel As Boolean = My.Settings.EnableParallelProcessing

            Dim T As Double = ms.Fases(0).SPMProperties.temperature.GetValueOrDefault
            Dim P As Double = ms.Fases(0).SPMProperties.pressure.GetValueOrDefault
            Dim W As Nullable(Of Double) = ms.Fases(0).SPMProperties.massflow
            Dim Q As Nullable(Of Double) = ms.Fases(0).SPMProperties.molarflow
            Dim QV As Nullable(Of Double) = ms.Fases(0).SPMProperties.volumetric_flow
            Dim H As Double = ms.Fases(0).SPMProperties.enthalpy.GetValueOrDefault

            Dim subs As DWSIM.ClassesBasicasTermodinamica.Substancia
            Dim comp As Double = 0
            For Each subs In ms.Fases(0).Componentes.Values
                comp += subs.FracaoMolar.GetValueOrDefault
            Next

            Dim foption As Integer

            With ms.PropertyPackage

                .CurrentMaterialStream = ms

                If W.HasValue And comp >= 0 Then
                    foption = 0
                    .DW_CalcVazaoMolar()
                ElseIf Q.HasValue And comp >= 0 Then
                    foption = 1
                    .DW_CalcVazaoMassica()
                ElseIf QV.HasValue And comp >= 0 Then
                    foption = 2
                    ms.Fases(0).SPMProperties.molarflow = 1.0#
                    ms.Fases(0).SPMProperties.massflow = 1.0#
                End If

                If DoNotCalcFlash Then
                    'do not do a flash calculation...
                ElseIf form.Options.SempreCalcularFlashPH And H <> 0 Then
                    .DW_CalcEquilibrium(DWSIM.SimulationObjects.PropertyPackages.FlashSpec.P, DWSIM.SimulationObjects.PropertyPackages.FlashSpec.H)
                Else
                    If .AUX_IS_SINGLECOMP(PropertyPackages.Fase.Mixture) Or .IsElectrolytePP Then
                        If ms.GraphicObject.InputConnectors(0).IsAttached Then
                            .DW_CalcEquilibrium(DWSIM.SimulationObjects.PropertyPackages.FlashSpec.P, DWSIM.SimulationObjects.PropertyPackages.FlashSpec.H)
                        Else
                            Select Case ms.SpecType
                                Case SimulationObjects.Streams.MaterialStream.Flashspec.Temperature_and_Pressure
                                    .DW_CalcEquilibrium(DWSIM.SimulationObjects.PropertyPackages.FlashSpec.T, DWSIM.SimulationObjects.PropertyPackages.FlashSpec.P)
                                Case SimulationObjects.Streams.MaterialStream.Flashspec.Pressure_and_Enthalpy
                                    .DW_CalcEquilibrium(DWSIM.SimulationObjects.PropertyPackages.FlashSpec.P, DWSIM.SimulationObjects.PropertyPackages.FlashSpec.H)
                                Case SimulationObjects.Streams.MaterialStream.Flashspec.Pressure_and_Entropy
                                    .DW_CalcEquilibrium(DWSIM.SimulationObjects.PropertyPackages.FlashSpec.P, DWSIM.SimulationObjects.PropertyPackages.FlashSpec.S)
                                Case SimulationObjects.Streams.MaterialStream.Flashspec.Pressure_and_VaporFraction
                                    .DW_CalcEquilibrium(DWSIM.SimulationObjects.PropertyPackages.FlashSpec.P, DWSIM.SimulationObjects.PropertyPackages.FlashSpec.VAP)
                                Case SimulationObjects.Streams.MaterialStream.Flashspec.Temperature_and_VaporFraction
                                    .DW_CalcEquilibrium(DWSIM.SimulationObjects.PropertyPackages.FlashSpec.T, DWSIM.SimulationObjects.PropertyPackages.FlashSpec.VAP)
                            End Select
                        End If
                    Else
                        Select Case ms.SpecType
                            Case SimulationObjects.Streams.MaterialStream.Flashspec.Temperature_and_Pressure
                                .DW_CalcEquilibrium(DWSIM.SimulationObjects.PropertyPackages.FlashSpec.T, DWSIM.SimulationObjects.PropertyPackages.FlashSpec.P)
                            Case SimulationObjects.Streams.MaterialStream.Flashspec.Pressure_and_Enthalpy
                                .DW_CalcEquilibrium(DWSIM.SimulationObjects.PropertyPackages.FlashSpec.P, DWSIM.SimulationObjects.PropertyPackages.FlashSpec.H)
                            Case SimulationObjects.Streams.MaterialStream.Flashspec.Pressure_and_Entropy
                                .DW_CalcEquilibrium(DWSIM.SimulationObjects.PropertyPackages.FlashSpec.P, DWSIM.SimulationObjects.PropertyPackages.FlashSpec.S)
                            Case SimulationObjects.Streams.MaterialStream.Flashspec.Pressure_and_VaporFraction
                                .DW_CalcEquilibrium(DWSIM.SimulationObjects.PropertyPackages.FlashSpec.P, DWSIM.SimulationObjects.PropertyPackages.FlashSpec.VAP)
                            Case SimulationObjects.Streams.MaterialStream.Flashspec.Temperature_and_VaporFraction
                                .DW_CalcEquilibrium(DWSIM.SimulationObjects.PropertyPackages.FlashSpec.T, DWSIM.SimulationObjects.PropertyPackages.FlashSpec.VAP)
                        End Select
                    End If
                End If
                If doparallel Then
                    My.MyApplication.IsRunningParallelTasks = True
                    If My.Settings.EnableGPUProcessing Then
                        My.MyApplication.gpu.EnableMultithreading()
                    End If
                    Try
                        Dim task1 As Task = New Task(Sub()
                                                         If ms.Fases(3).SPMProperties.molarfraction.GetValueOrDefault > 0 Then
                                                             .DW_CalcPhaseProps(DWSIM.SimulationObjects.PropertyPackages.Fase.Liquid1)
                                                         Else
                                                             .DW_ZerarPhaseProps(DWSIM.SimulationObjects.PropertyPackages.Fase.Liquid1)
                                                         End If
                                                     End Sub)
                        Dim task2 As Task = New Task(Sub()
                                                         If ms.Fases(4).SPMProperties.molarfraction.GetValueOrDefault > 0 Then
                                                             .DW_CalcPhaseProps(DWSIM.SimulationObjects.PropertyPackages.Fase.Liquid2)
                                                         Else
                                                             .DW_ZerarPhaseProps(DWSIM.SimulationObjects.PropertyPackages.Fase.Liquid2)
                                                         End If
                                                     End Sub)
                        Dim task3 As Task = New Task(Sub()
                                                         If ms.Fases(5).SPMProperties.molarfraction.GetValueOrDefault > 0 Then
                                                             .DW_CalcPhaseProps(DWSIM.SimulationObjects.PropertyPackages.Fase.Liquid3)
                                                         Else
                                                             .DW_ZerarPhaseProps(DWSIM.SimulationObjects.PropertyPackages.Fase.Liquid3)
                                                         End If
                                                     End Sub)
                        Dim task4 As Task = New Task(Sub()
                                                         If ms.Fases(6).SPMProperties.molarfraction.GetValueOrDefault > 0 Then
                                                             .DW_CalcPhaseProps(DWSIM.SimulationObjects.PropertyPackages.Fase.Aqueous)
                                                         Else
                                                             .DW_ZerarPhaseProps(DWSIM.SimulationObjects.PropertyPackages.Fase.Aqueous)
                                                         End If
                                                     End Sub)
                        Dim task5 As Task = New Task(Sub()
                                                         If ms.Fases(7).SPMProperties.molarfraction.GetValueOrDefault > 0 Then
                                                             .DW_CalcSolidPhaseProps()
                                                         Else
                                                             .DW_ZerarPhaseProps(DWSIM.SimulationObjects.PropertyPackages.Fase.Solid)
                                                         End If
                                                     End Sub)
                        Dim task6 As Task = New Task(Sub()
                                                         If ms.Fases(2).SPMProperties.molarfraction.GetValueOrDefault > 0 Then
                                                             .DW_CalcPhaseProps(DWSIM.SimulationObjects.PropertyPackages.Fase.Vapor)
                                                         Else
                                                             .DW_ZerarPhaseProps(DWSIM.SimulationObjects.PropertyPackages.Fase.Vapor)
                                                         End If
                                                     End Sub)
                        Select Case My.Settings.MaxDegreeOfParallelism
                            Case 1
                                task1.Start()
                                task1.Wait()
                                task2.Start()
                                task2.Wait()
                                task3.Start()
                                task3.Wait()
                                task4.Start()
                                task4.Wait()
                                task5.Start()
                                task5.Wait()
                                task6.Start()
                                task6.Wait()
                            Case 2
                                task1.Start()
                                task2.Start()
                                Task.WaitAll(task1, task2)
                                task3.Start()
                                task4.Start()
                                Task.WaitAll(task3, task4)
                                task5.Start()
                                task6.Start()
                                Task.WaitAll(task5, task6)
                            Case 3
                                task1.Start()
                                task2.Start()
                                task3.Start()
                                Task.WaitAll(task1, task2, task3)
                                task4.Start()
                                task5.Start()
                                task6.Start()
                                Task.WaitAll(task4, task5, task6)
                            Case Else
                                task1.Start()
                                task2.Start()
                                task3.Start()
                                task4.Start()
                                task5.Start()
                                task6.Start()
                                Task.WaitAll(task1, task2, task3, task4, task5, task6)
                        End Select
                    Catch ae As AggregateException
                        form.ProcessScripts(Script.EventType.ObjectCalculationError, Script.ObjectType.FlowsheetObject, ms.Name)
                        Throw ae.Flatten().InnerException
                    Finally
                        If My.Settings.EnableGPUProcessing Then
                            My.MyApplication.gpu.DisableMultithreading()
                            My.MyApplication.gpu.FreeAll()
                        End If
                    End Try
                    My.MyApplication.IsRunningParallelTasks = False
                Else
                    If ms.Fases(3).SPMProperties.molarfraction.GetValueOrDefault > 0 Then
                        .DW_CalcPhaseProps(DWSIM.SimulationObjects.PropertyPackages.Fase.Liquid1)
                    Else
                        .DW_ZerarPhaseProps(DWSIM.SimulationObjects.PropertyPackages.Fase.Liquid1)
                    End If
                    If ms.Fases(4).SPMProperties.molarfraction.GetValueOrDefault > 0 Then
                        .DW_CalcPhaseProps(DWSIM.SimulationObjects.PropertyPackages.Fase.Liquid2)
                    Else
                        .DW_ZerarPhaseProps(DWSIM.SimulationObjects.PropertyPackages.Fase.Liquid2)
                    End If
                    If ms.Fases(5).SPMProperties.molarfraction.GetValueOrDefault > 0 Then
                        .DW_CalcPhaseProps(DWSIM.SimulationObjects.PropertyPackages.Fase.Liquid3)
                    Else
                        .DW_ZerarPhaseProps(DWSIM.SimulationObjects.PropertyPackages.Fase.Liquid3)
                    End If
                    If ms.Fases(6).SPMProperties.molarfraction.GetValueOrDefault > 0 Then
                        .DW_CalcPhaseProps(DWSIM.SimulationObjects.PropertyPackages.Fase.Aqueous)
                    Else
                        .DW_ZerarPhaseProps(DWSIM.SimulationObjects.PropertyPackages.Fase.Aqueous)
                    End If
                    If ms.Fases(7).SPMProperties.molarfraction.GetValueOrDefault > 0 Then
                        .DW_CalcSolidPhaseProps()
                    Else
                        .DW_ZerarPhaseProps(DWSIM.SimulationObjects.PropertyPackages.Fase.Solid)
                    End If
                    If ms.Fases(2).SPMProperties.molarfraction.GetValueOrDefault > 0 Then
                        .DW_CalcPhaseProps(DWSIM.SimulationObjects.PropertyPackages.Fase.Vapor)
                    Else
                        .DW_ZerarPhaseProps(DWSIM.SimulationObjects.PropertyPackages.Fase.Vapor)
                    End If
                End If
                If ms.Fases(2).SPMProperties.molarfraction.GetValueOrDefault >= 0 And ms.Fases(2).SPMProperties.molarfraction.GetValueOrDefault <= 1 Then
                    .DW_CalcPhaseProps(DWSIM.SimulationObjects.PropertyPackages.Fase.Liquid)
                Else
                    .DW_ZerarPhaseProps(DWSIM.SimulationObjects.PropertyPackages.Fase.Liquid)
                End If

                Select Case foption
                    Case 0, 1
                        .DW_CalcCompMolarFlow(-1)
                        .DW_CalcCompMassFlow(-1)
                        .DW_CalcCompVolFlow(-1)
                        .DW_CalcOverallProps()
                        .DW_CalcTwoPhaseProps(DWSIM.SimulationObjects.PropertyPackages.Fase.Liquid, DWSIM.SimulationObjects.PropertyPackages.Fase.Vapor)
                        .DW_CalcVazaoVolumetrica()
                        .DW_CalcKvalue()
                    Case 2
                        .DW_CalcOverallProps()
                        ms.Fases(0).SPMProperties.massflow = QV * ms.Fases(0).SPMProperties.density.GetValueOrDefault
                        .DW_CalcVazaoMolar()
                        .DW_CalcCompMolarFlow(-1)
                        .DW_CalcCompMassFlow(-1)
                        .DW_CalcCompVolFlow(-1)
                        .DW_CalcTwoPhaseProps(DWSIM.SimulationObjects.PropertyPackages.Fase.Liquid, DWSIM.SimulationObjects.PropertyPackages.Fase.Vapor)
                        .DW_CalcKvalue()
                End Select

                .CurrentMaterialStream = Nothing

                If ms.IsSpecAttached = True And ms.SpecVarType = DWSIM.SimulationObjects.SpecialOps.Helpers.Spec.TipoVar.Fonte Then form.Collections.CLCS_SpecCollection(ms.AttachedSpecId).Calculate()

            End With

            ms.Calculated = True

        End Sub

        Public Shared Sub DeCalculateObject(ByVal form As FormFlowsheet, ByVal obj As GraphicObject)

            If obj.TipoObjeto = TipoObjeto.MaterialStream Then

                Dim con As ConnectionPoint
                For Each con In obj.InputConnectors
                    If con.IsAttached Then
                        Dim UnitOp As Object = form.Collections.ObjectCollection(con.AttachedConnector.AttachedFrom.Name)
                        UnitOp.UpdatePropertyNodes(form.Options.SelectedUnitSystem, form.Options.NumberFormat)
                        UnitOp.DeCalculate()
                    End If
                Next
                For Each con In obj.OutputConnectors
                    If con.IsAttached Then
                        Dim UnitOp As Object = form.Collections.ObjectCollection(con.AttachedConnector.AttachedTo.Name)
                        UnitOp.UpdatePropertyNodes(form.Options.SelectedUnitSystem, form.Options.NumberFormat)
                        UnitOp.DeCalculate()
                    End If
                Next

            ElseIf obj.TipoObjeto = TipoObjeto.EnergyStream Then

                Dim con As ConnectionPoint
                For Each con In obj.InputConnectors
                    If con.IsAttached Then
                        Dim UnitOp As Object = form.Collections.ObjectCollection(con.AttachedConnector.AttachedFrom.Name)
                        UnitOp.UpdatePropertyNodes(form.Options.SelectedUnitSystem, form.Options.NumberFormat)
                        UnitOp.DeCalculate()
                    End If
                Next
                For Each con In obj.OutputConnectors
                    If con.IsAttached Then
                        Dim UnitOp As Object = form.Collections.ObjectCollection(con.AttachedConnector.AttachedTo.Name)
                        UnitOp.UpdatePropertyNodes(form.Options.SelectedUnitSystem, form.Options.NumberFormat)
                        UnitOp.DeCalculate()
                    End If
                Next

            Else

                Dim UnitOp As Object = form.Collections.ObjectCollection(obj.Name)
                UnitOp.UpdatePropertyNodes(form.Options.SelectedUnitSystem, form.Options.NumberFormat)
                UnitOp.DeCalculate()

            End If

        End Sub

        Public Shared Sub DeCalculateDisconnectedObject(ByVal form As FormFlowsheet, ByVal obj As GraphicObject, ByVal side As String)

            If obj.TipoObjeto = TipoObjeto.MaterialStream Then

                Dim con As ConnectionPoint

                If side = "In" Then

                    For Each con In obj.InputConnectors
                        If con.IsAttached Then
                            Try
                                Dim UnitOp As Object = form.Collections.ObjectCollection(con.AttachedConnector.AttachedFrom.Name)
                                UnitOp.UpdatePropertyNodes(form.Options.SelectedUnitSystem, form.Options.NumberFormat)
                                UnitOp.DeCalculate()
                            Catch ex As Exception

                            End Try
                        End If
                    Next

                Else

                    For Each con In obj.OutputConnectors
                        If con.IsAttached Then
                            Try
                                Dim UnitOp As Object = form.Collections.ObjectCollection(con.AttachedConnector.AttachedTo.Name)
                                UnitOp.UpdatePropertyNodes(form.Options.SelectedUnitSystem, form.Options.NumberFormat)
                                UnitOp.DeCalculate()
                            Catch ex As Exception

                            End Try
                        End If
                    Next

                End If

            ElseIf obj.TipoObjeto = TipoObjeto.EnergyStream Then

                Dim con As ConnectionPoint
                If side = "In" Then

                    For Each con In obj.InputConnectors
                        If con.IsAttached Then
                            Dim UnitOp As Object = form.Collections.ObjectCollection(con.AttachedConnector.AttachedFrom.Name)
                            UnitOp.UpdatePropertyNodes(form.Options.SelectedUnitSystem, form.Options.NumberFormat)
                            UnitOp.DeCalculate()
                        End If
                    Next

                Else

                    For Each con In obj.OutputConnectors
                        If con.IsAttached Then
                            Dim UnitOp As Object = form.Collections.ObjectCollection(con.AttachedConnector.AttachedTo.Name)
                            UnitOp.UpdatePropertyNodes(form.Options.SelectedUnitSystem, form.Options.NumberFormat)
                            UnitOp.DeCalculate()
                        End If
                    Next

                End If

            Else

                If side = "In" Then

                    Try
                        Dim UnitOp As Object = form.Collections.ObjectCollection(obj.Name)
                        UnitOp.UpdatePropertyNodes(form.Options.SelectedUnitSystem, form.Options.NumberFormat)
                        UnitOp.DeCalculate()
                    Catch ex As Exception

                    End Try

                Else

                    Try
                        Dim UnitOp As Object = form.Collections.ObjectCollection(obj.Name)
                        UnitOp.UpdatePropertyNodes(form.Options.SelectedUnitSystem, form.Options.NumberFormat)
                        UnitOp.DeCalculate()
                    Catch ex As Exception

                    End Try

                End If

            End If

        End Sub

        ''' <summary>
        ''' Process the calculation queue of the Flowsheet passed as an argument. Checks all elements in the queue and calculates them.
        ''' </summary>
        ''' <param name="form">Flowsheet to be calculated (FormChild object)</param>
        ''' <remarks></remarks>

        Public Shared Sub ProcessCalculationQueue(ByVal form As FormFlowsheet, Optional ByVal Isolated As Boolean = False, Optional ByVal FlowsheetSolverMode As Boolean = False, Optional ByVal mode As Integer = 0, Optional orderedlist As Object = Nothing)

            If mode = 0 Then
                ProcessQueueInternal(form, Isolated, FlowsheetSolverMode)
                SolveSimultaneousAdjusts(form)
            ElseIf mode = 1 Then
                If orderedlist Is Nothing Then
                    ProcessQueueInternalAsync(form, Isolated)
                Else
                    ProcessQueueInternalAsyncParallel(form, orderedlist)
                End If
                SolveSimultaneousAdjustsAsync(form)
            End If

        End Sub

        Private Shared Sub ProcessQueueInternal(ByVal form As FormFlowsheet, Optional ByVal Isolated As Boolean = False, Optional ByVal FlowsheetSolverMode As Boolean = False)

            form.FormSurface.LabelTime.Text = ""
            form.FormSurface.calcstart = Date.Now
            form.FormSurface.PictureBox3.Image = My.Resources.weather_lightning
            form.FormSurface.PictureBox4.Visible = True

            While form.CalculationQueue.Count >= 1

                My.MyApplication.CalculatorStopRequested = False

                If form.FormSurface.Timer2.Enabled = False Then form.FormSurface.Timer2.Start()

                Dim myinfo As DWSIM.Outros.StatusChangeEventArgs = form.CalculationQueue.Peek()

                Try
                    form.FormQueue.TextBox1.Clear()
                    For Each c As DWSIM.Outros.StatusChangeEventArgs In form.CalculationQueue
                        form.FormQueue.TextBox1.AppendText(form.Collections.ObjectCollection(c.Nome).GraphicObject.Tag & vbTab & vbTab & vbTab & "[" & DWSIM.App.GetLocalString(form.Collections.ObjectCollection(c.Nome).Descricao) & "]" & vbCrLf)
                    Next
                Catch ex As Exception
                    'form.WriteToLog(ex.Message, Color.Black, DWSIM.FormClasses.TipoAviso.Erro)
                End Try

                Try
                    If FlowsheetSolverMode Then
                        If myinfo.Emissor = "FlowsheetSolver" Then
                            If myinfo.Tipo = TipoObjeto.MaterialStream Then
                                CalculateMaterialStream(form, form.Collections.CLCS_MaterialStreamCollection(myinfo.Nome), , Isolated)
                            Else
                                If My.Settings.EnableGPUProcessing Then DWSIM.App.InitComputeDevice()
                                CalculateFlowsheet(form, myinfo, Nothing, Isolated)
                            End If
                        End If
                    Else
                        If myinfo.Tipo = TipoObjeto.MaterialStream Then
                            CalculateMaterialStream(form, form.Collections.CLCS_MaterialStreamCollection(myinfo.Nome), , Isolated)
                        Else
                            If My.Settings.EnableGPUProcessing Then DWSIM.App.InitComputeDevice()
                            CalculateFlowsheet(form, myinfo, Nothing, Isolated)
                        End If
                    End If
                Catch ex As Exception
                    Dim st As New StackTrace(ex, True)
                    If st.FrameCount > 0 Then
                        form.WriteToLog(myinfo.Tag & ": " & ex.Message.ToString & " (" & Path.GetFileName(st.GetFrame(0).GetFileName) & ", " & st.GetFrame(0).GetFileLineNumber & ")", Color.Red, FormClasses.TipoAviso.Erro)
                    Else
                        form.WriteToLog(myinfo.Tag & ": " & ex.Message.ToString, Color.Red, FormClasses.TipoAviso.Erro)
                    End If
                End Try

                form.FormWatch.UpdateList()

                For Each g As GraphicObject In form.FormSurface.FlowsheetDesignSurface.drawingObjects
                    If g.TipoObjeto = TipoObjeto.GO_MasterTable Then
                        CType(g, DWSIM.GraphicObjects.MasterTableGraphic).Update(form)
                    End If
                Next

                If form.CalculationQueue.Count = 1 Then form.FormSpreadsheet.InternalCounter = 0
                If form.CalculationQueue.Count > 0 Then form.CalculationQueue.Dequeue()

                CheckCalculatorStatus()

                Application.DoEvents()

            End While

            form.FormQueue.TextBox1.Clear()

            If Not form.FormSpreadsheet Is Nothing Then
                If form.FormSpreadsheet.chkUpdate.Checked Then form.FormSpreadsheet.EvaluateAll()
            End If

            If form.FormSurface.LabelTime.Text <> "" Then
                form.WriteToLog(DWSIM.App.GetLocalString("Runtime") & ": " & form.FormSurface.LabelTime.Text, Color.MediumBlue, DWSIM.FormClasses.TipoAviso.Informacao)
            End If

            If form.FormSurface.Timer2.Enabled = True Then form.FormSurface.Timer2.Stop()
            form.FormSurface.PictureBox3.Image = My.Resources.tick
            form.FormSurface.PictureBox4.Visible = False
            form.FormSurface.LabelTime.Text = ""

            If Not form.FormSurface.FlowsheetDesignSurface.SelectedObject Is Nothing Then Call form.FormSurface.UpdateSelectedObject()

            form.FormSurface.LabelCalculator.Text = DWSIM.App.GetLocalString("CalculadorOcioso")

            Application.DoEvents()

        End Sub

        Private Shared Sub ProcessQueueInternalAsync(ByVal form As FormFlowsheet, Optional ByVal Isolated As Boolean = False)

            If My.Settings.EnableGPUProcessing Then DWSIM.App.InitComputeDevice()

            Dim allex As New List(Of Exception)

            While form.CalculationQueue.Count >= 1

                Dim myinfo As DWSIM.Outros.StatusChangeEventArgs = form.CalculationQueue.Peek()
                Try
                    If myinfo.Tipo = TipoObjeto.MaterialStream Then
                        CalculateMaterialStreamAsync(form, form.Collections.CLCS_MaterialStreamCollection(myinfo.Nome), , Isolated)
                    Else
                        CalculateFlowsheetAsync(form, myinfo, Nothing, Isolated)
                    End If
                Catch ex As Exception
                    allex.Add(New Exception(myinfo.Tag & ": " & ex.Message.ToString, ex))
                End Try

                If form.CalculationQueue.Count = 1 Then form.FormSpreadsheet.InternalCounter = 0
                If form.CalculationQueue.Count > 0 Then form.CalculationQueue.Dequeue()

            End While

            If allex.Count > 0 Then Throw New AggregateException(allex)

        End Sub

        Private Shared Sub ProcessQueueInternalAsyncParallel(ByVal form As FormFlowsheet, ByVal orderedlist As Dictionary(Of Integer, List(Of StatusChangeEventArgs)))

            If My.Settings.EnableGPUProcessing Then DWSIM.App.InitComputeDevice()

            Dim allex As New List(Of Exception)

            For Each obj In form.Collections.ObjectCollection.Values
                If TypeOf obj Is SimulationObjects_UnitOpBaseClass Then
                    DirectCast(obj, SimulationObjects_UnitOpBaseClass).PropertyPackage = Nothing
                    DirectCast(obj, SimulationObjects_UnitOpBaseClass).PropertyPackage = DirectCast(obj, SimulationObjects_UnitOpBaseClass).PropertyPackage.Clone
                    DirectCast(obj, SimulationObjects_UnitOpBaseClass).PropertyPackage.ForceNewFlashAlgorithmInstance = True
                ElseIf TypeOf obj Is Streams.MaterialStream Then
                    DirectCast(obj, Streams.MaterialStream).PropertyPackage = Nothing
                    DirectCast(obj, Streams.MaterialStream).PropertyPackage = DirectCast(obj, Streams.MaterialStream).PropertyPackage.Clone
                    DirectCast(obj, Streams.MaterialStream).PropertyPackage.ForceNewFlashAlgorithmInstance = True
                    DirectCast(obj, Streams.MaterialStream).PropertyPackage.CurrentMaterialStream = DirectCast(obj, Streams.MaterialStream)
                End If
            Next

            For Each li In orderedlist
                Parallel.ForEach(li.Value, Sub(myinfo)
                                               Try
                                                   If myinfo.Tipo = TipoObjeto.MaterialStream Then
                                                       CalculateMaterialStreamAsync(form, form.Collections.CLCS_MaterialStreamCollection(myinfo.Nome), , True)
                                                   Else
                                                       CalculateFlowsheetAsync(form, myinfo, Nothing, True)
                                                   End If
                                               Catch ex As Exception
                                                   allex.Add(New Exception(myinfo.Tag & ": " & ex.Message.ToString, ex))
                                               End Try
                                           End Sub)
            Next

            For Each obj In form.Collections.ObjectCollection.Values
                If TypeOf obj Is SimulationObjects_UnitOpBaseClass Then
                    DirectCast(obj, SimulationObjects_UnitOpBaseClass).PropertyPackage = Nothing
                ElseIf TypeOf obj Is Streams.MaterialStream Then
                    DirectCast(obj, Streams.MaterialStream).PropertyPackage = Nothing
                End If
            Next

            form.CalculationQueue.Clear()

            If allex.Count > 0 Then Throw New AggregateException(allex)

        End Sub

        ''' <summary>
        ''' Checks the calculator status to see if the user did any stop/abort request, and throws an exception to force aborting, if necessary.
        ''' </summary>
        ''' <remarks></remarks>
        Public Shared Sub CheckCalculatorStatus()

            If Not My.MyApplication.IsRunningParallelTasks Then
                If Not My.Application.CAPEOPENMode Then
                    If My.MyApplication.CalculatorStopRequested = True Then
                        My.MyApplication.MasterCalculatorStopRequested = True
                        My.MyApplication.CalculatorStopRequested = False
                        Throw New Exception(DWSIM.App.GetLocalString("CalculationAborted"))
                    End If
                End If
                'Application.DoEvents()
            End If

        End Sub

        ''' <summary>
        ''' Calculate all objects in the Flowsheet using a ordering method.
        ''' </summary>
        ''' <param name="form">Flowsheet to be calculated (FormChild object)</param>
        ''' <remarks></remarks>
        Public Shared Sub CalculateAll2(ByVal form As FormFlowsheet, mode As Integer)

            'mode:
            '0 = Synchronous (main thread)
            '1 = Asynchronous (background thread)
            '2 = Azure VM

            Dim lists As New Dictionary(Of Integer, List(Of String))
            Dim filteredlist As New Dictionary(Of Integer, List(Of String))
            Dim filteredlist2 As New Dictionary(Of Integer, List(Of StatusChangeEventArgs))
            Dim objstack As New List(Of String)
            Dim recycles As New List(Of String)

            Dim obj As SimulationObjects_BaseClass

            form.ProcessScripts(Script.EventType.SolverStarted, Script.ObjectType.Solver)

            RaiseEvent FlowsheetCalculationStarted(form, New System.EventArgs(), Nothing)

            'find recycles.

            For Each baseobj As SimulationObjects_BaseClass In form.Collections.ObjectCollection.Values
                If baseobj.GraphicObject.TipoObjeto = TipoObjeto.OT_Reciclo Then
                    recycles.Add(baseobj.Nome)
                End If
            Next

            'add endpoint material streams and recycle ops to the list, they will be the last objects to be calculated.

            lists.Add(0, New List(Of String))

            For Each baseobj As SimulationObjects_BaseClass In form.Collections.ObjectCollection.Values
                If baseobj.GraphicObject.TipoObjeto = TipoObjeto.MaterialStream Then
                    Dim ms As Streams.MaterialStream = baseobj
                    If ms.GraphicObject.OutputConnectors(0).IsAttached = False Then
                        lists(0).Add(baseobj.Nome)
                    End If
                ElseIf baseobj.GraphicObject.TipoObjeto = TipoObjeto.OT_Reciclo Then
                    lists(0).Add(baseobj.Nome)
                End If
            Next

            'now start processing the list at each level, until it reaches the beginning of the flowsheet.

            Dim listidx As Integer = 0
            Dim maxidx As Integer = 0

            Do
                listidx += 1
                If lists(listidx - 1).Count > 0 Then
                    lists.Add(listidx, New List(Of String))
                    maxidx = listidx
                    For Each o As String In lists(listidx - 1)
                        obj = form.Collections.ObjectCollection(o)
                        For Each c As ConnectionPoint In obj.GraphicObject.InputConnectors
                            If c.IsAttached Then
                                If c.AttachedConnector.AttachedFrom.TipoObjeto <> TipoObjeto.OT_Reciclo Then
                                    lists(listidx).Add(c.AttachedConnector.AttachedFrom.Name)
                                End If
                            End If
                        Next
                    Next
                Else
                    Exit Do
                End If
            Loop

            'process the lists backwards, adding objects to the stack, discarding duplicate entries.

            listidx = maxidx

            Do
                If lists.ContainsKey(listidx) Then
                    filteredlist.Add(maxidx - listidx, New List(Of String)(lists(listidx).ToArray))
                    For Each o As String In lists(listidx)
                        If Not objstack.Contains(o) Then
                            objstack.Add(o)
                        Else
                            filteredlist(maxidx - listidx).Remove(o)
                        End If
                    Next
                Else
                    Exit Do
                End If
                listidx -= 1
            Loop

            'define variable to check for flowsheet convergence if there are recycle ops

            Dim converged As Boolean = False

            'process/calculate the stack.

            If form.CalculationQueue Is Nothing Then form.CalculationQueue = New Queue(Of DWSIM.Outros.StatusChangeEventArgs)

            My.MyApplication.MasterCalculatorStopRequested = False

            Dim d1 As Date

            d1 = Date.Now

            form.WriteToLog(DWSIM.App.GetLocalString("FSstartedsolving"), Color.Blue, FormClasses.TipoAviso.Informacao)

            Dim age As AggregateException = Nothing

            Dim objargs As DWSIM.Outros.StatusChangeEventArgs = Nothing

            Dim preLab As String = form.FormSurface.LabelCalculator.Text

            While Not converged

                If My.MyApplication.MasterCalculatorStopRequested Then Exit While

                For Each o As String In objstack
                    obj = form.Collections.ObjectCollection(o)
                    objargs = New DWSIM.Outros.StatusChangeEventArgs
                    With objargs
                        .Emissor = "FlowsheetSolver"
                        .Calculado = True
                        .Nome = obj.Nome
                        .Tipo = obj.GraphicObject.TipoObjeto
                        .Tag = obj.GraphicObject.Tag
                        form.CalculationQueue.Enqueue(objargs)
                    End With
                Next

                For Each obj In form.Collections.ObjectCollection.Values
                    obj.SetFlowsheet(form)
                Next

                If mode = 0 Then
                    ProcessCalculationQueue(form, True, True, 0)
                    CheckCalculatorStatus()
                ElseIf mode = 1 Then
                    filteredlist2.Clear()
                    For Each li In filteredlist
                        Dim objcalclist As New List(Of StatusChangeEventArgs)
                        For Each o In li.Value
                            obj = form.Collections.ObjectCollection(o)
                            objcalclist.Add(New StatusChangeEventArgs() With {.Emissor = "FlowsheetSolver", .Nome = obj.Nome, .Tipo = obj.GraphicObject.TipoObjeto, .Tag = obj.GraphicObject.Tag})
                        Next
                        filteredlist2.Add(li.Key, objcalclist)
                    Next
                    Try
                        form.UpdateStatusLabel(DWSIM.App.GetLocalString("Calculando") & " " & DWSIM.App.GetLocalString("Fluxograma") & "...")
                        Dim t As New Task(Sub()
                                              If My.Settings.EnableParallelProcessing Then
                                                  ProcessCalculationQueue(form, True, True, 1, filteredlist2)
                                              Else
                                                  ProcessCalculationQueue(form, True, True, 1)
                                              End If
                                          End Sub)
                        t.Start()
                        If Not t.Wait(My.Settings.SolverTimeoutSeconds * 1000, My.MyApplication.SolverCancellationToken) Then
                            Throw New TimeoutException(DWSIM.App.GetLocalString("SolverTimeout"))
                        End If
                    Catch agex As AggregateException
                        age = agex
                        Exit While
                    Catch ex As TimeoutException
                        age = New AggregateException(ex.Message.ToString, ex)
                        Exit While
                    End Try
                End If

                converged = True

                For Each r As String In recycles
                    obj = form.Collections.CLCS_RecycleCollection(r)
                    converged = DirectCast(obj, SpecialOps.Recycle).Converged
                    If Not converged Then Exit For
                Next

                form.ProcessScripts(Script.EventType.SolverRecycleLoop, Script.ObjectType.Solver)

            End While

            If mode = 1 Then
                form.UpdateStatusLabel(prelab)
                If age Is Nothing Then
                    form.WriteToLog(DWSIM.App.GetLocalString("FSfinishedsolvingok"), Color.Blue, FormClasses.TipoAviso.Informacao)
                    form.WriteToLog(DWSIM.App.GetLocalString("Runtime") & ": " & Format((Date.Now - d1).TotalSeconds, "0.##") & "s", Color.MediumBlue, DWSIM.FormClasses.TipoAviso.Informacao)
                Else
                    age = age.Flatten()
                    form.WriteToLog(DWSIM.App.GetLocalString("FSfinishedsolvingerror"), Color.Red, FormClasses.TipoAviso.Erro)
                    For Each ex In age.InnerExceptions
                        Dim st As New StackTrace(ex.InnerException, True)
                        If st.FrameCount > 0 Then
                            form.WriteToLog(ex.Message & " (" & Path.GetFileName(st.GetFrame(0).GetFileName) & ", " & st.GetFrame(0).GetFileLineNumber & ")", Color.Red, FormClasses.TipoAviso.Erro)
                        Else
                            form.WriteToLog(ex.Message.ToString, Color.Red, FormClasses.TipoAviso.Erro)
                        End If
                    Next
                    age = Nothing
                End If
            End If

            objstack.Clear()
            lists.Clear()
            recycles.Clear()

            form.ProcessScripts(Script.EventType.SolverFinished, Script.ObjectType.Solver)

            RaiseEvent FlowsheetCalculationFinished(form, New System.EventArgs(), Nothing)

        End Sub

        ''' <summary>
        ''' Calculate all objects in the Flowsheet.
        ''' </summary>
        ''' <param name="form">Flowsheet to be calculated (FormChild object)</param>
        ''' <remarks></remarks>
        Public Shared Sub CalculateAll(ByVal form As FormFlowsheet)

            CalculateAll2(form, My.Settings.SolverMode)

        End Sub

        ''' <summary>
        ''' Calculates a single object in the Flowsheet.
        ''' </summary>
        ''' <param name="form">Flowsheet where the object belongs to.</param>
        ''' <param name="ObjID">Unique Id of the object ("Name" or "GraphicObject.Name" properties). This is not the object's Flowsheet display name ("Tag" property or its GraphicObject object).</param>
        ''' <remarks></remarks>

        Public Shared Sub CalculateObjectAsync(ByVal form As FormFlowsheet, ByVal ObjID As String)

            If form.Collections.ObjectCollection.ContainsKey(ObjID) Then

                Dim baseobj As SimulationObjects_BaseClass = form.Collections.ObjectCollection(ObjID)

                If baseobj.GraphicObject.TipoObjeto = TipoObjeto.MaterialStream Then
                    Dim ms As Streams.MaterialStream = baseobj
                    If ms.GraphicObject.InputConnectors(0).IsAttached = False Then
                        'add this stream to the calculator queue list
                        Dim objargs As New DWSIM.Outros.StatusChangeEventArgs
                        With objargs
                            .Calculado = True
                            .Nome = ms.Nome
                            .Tipo = TipoObjeto.MaterialStream
                            .Tag = ms.GraphicObject.Tag
                        End With
                        If ms.IsSpecAttached = True And ms.SpecVarType = DWSIM.SimulationObjects.SpecialOps.Helpers.Spec.TipoVar.Fonte Then
                            form.Collections.CLCS_SpecCollection(ms.AttachedSpecId).Calculate()
                        End If
                        form.CalculationQueue.Enqueue(objargs)
                        ProcessQueueInternal(form)
                    Else
                        If ms.GraphicObject.InputConnectors(0).AttachedConnector.AttachedFrom.TipoObjeto = TipoObjeto.OT_Reciclo Then
                            'add this stream to the calculator queue list
                            Dim objargs As New DWSIM.Outros.StatusChangeEventArgs
                            With objargs
                                .Calculado = True
                                .Nome = ms.Nome
                                .Tipo = TipoObjeto.MaterialStream
                                .Tag = ms.GraphicObject.Tag
                            End With
                            If ms.IsSpecAttached = True And ms.SpecVarType = DWSIM.SimulationObjects.SpecialOps.Helpers.Spec.TipoVar.Fonte Then
                                form.Collections.CLCS_SpecCollection(ms.AttachedSpecId).Calculate()
                            End If
                            form.CalculationQueue.Enqueue(objargs)
                            ProcessQueueInternal(form)
                        End If
                    End If
                Else
                    Dim unit As SimulationObjects_UnitOpBaseClass = baseobj
                    Dim objargs As New DWSIM.Outros.StatusChangeEventArgs
                    With objargs
                        .Emissor = "PropertyGrid"
                        .Calculado = True
                        .Nome = unit.Nome
                        .Tipo = unit.GraphicObject.TipoObjeto
                        .Tag = unit.GraphicObject.Tag
                    End With
                    form.CalculationQueue.Enqueue(objargs)
                    ProcessQueueInternal(form)
                End If

            End If

        End Sub

        Public Shared Sub CalculateObject(ByVal form As FormFlowsheet, ByVal ObjID As String)

            If form.Collections.ObjectCollection.ContainsKey(ObjID) Then

                Dim baseobj As SimulationObjects_BaseClass = form.Collections.ObjectCollection(ObjID)

                If baseobj.GraphicObject.TipoObjeto = TipoObjeto.MaterialStream Then
                    Dim ms As Streams.MaterialStream = baseobj
                    If ms.GraphicObject.InputConnectors(0).IsAttached = False Then
                        'add this stream to the calculator queue list
                        Dim objargs As New DWSIM.Outros.StatusChangeEventArgs
                        With objargs
                            .Calculado = True
                            .Nome = ms.Nome
                            .Tipo = TipoObjeto.MaterialStream
                            .Tag = ms.GraphicObject.Tag
                        End With
                        If ms.IsSpecAttached = True And ms.SpecVarType = DWSIM.SimulationObjects.SpecialOps.Helpers.Spec.TipoVar.Fonte Then
                            form.Collections.CLCS_SpecCollection(ms.AttachedSpecId).Calculate()
                        End If
                        form.CalculationQueue.Enqueue(objargs)
                        ProcessQueueInternal(form)
                    Else
                        If ms.GraphicObject.InputConnectors(0).AttachedConnector.AttachedFrom.TipoObjeto = TipoObjeto.OT_Reciclo Then
                            'add this stream to the calculator queue list
                            Dim objargs As New DWSIM.Outros.StatusChangeEventArgs
                            With objargs
                                .Calculado = True
                                .Nome = ms.Nome
                                .Tipo = TipoObjeto.MaterialStream
                                .Tag = ms.GraphicObject.Tag
                            End With
                            If ms.IsSpecAttached = True And ms.SpecVarType = DWSIM.SimulationObjects.SpecialOps.Helpers.Spec.TipoVar.Fonte Then
                                form.Collections.CLCS_SpecCollection(ms.AttachedSpecId).Calculate()
                            End If
                            form.CalculationQueue.Enqueue(objargs)
                            ProcessQueueInternal(form)
                        End If
                    End If
                Else
                    Dim unit As SimulationObjects_UnitOpBaseClass = baseobj
                    Dim objargs As New DWSIM.Outros.StatusChangeEventArgs
                    With objargs
                        .Emissor = "PropertyGrid"
                        .Calculado = True
                        .Nome = unit.Nome
                        .Tipo = unit.GraphicObject.TipoObjeto
                        .Tag = unit.GraphicObject.Tag
                    End With
                    form.CalculationQueue.Enqueue(objargs)
                    ProcessQueueInternal(form)
                End If

            End If

        End Sub

        'Simultaneous Adjust Solver
        Private Shared Sub SolveSimultaneousAdjusts(ByVal form As FormFlowsheet)

            If form.m_simultadjustsolverenabled Then

                form.FormSurface.LabelSimultAdjInfo.Text = ""
                form.FormSurface.PanelSimultAdjust.Visible = True

                Try
                    Dim n As Integer = 0

                    For Each adj As SimulationObjects.SpecialOps.Adjust In form.Collections.CLCS_AdjustCollection.Values
                        If adj.SimultaneousAdjust Then n += 1
                    Next

                    n -= 1

                    Dim i As Integer = 0
                    Dim dfdx(n, n), dx(n), fx(n), x(n) As Double
                    Dim il_err_ant As Double = 10000000000.0
                    Dim il_err As Double = 10000000000.0
                    Dim ic As Integer

                    i = 0
                    For Each adj As SimulationObjects.SpecialOps.Adjust In form.Collections.CLCS_AdjustCollection.Values
                        If adj.SimultaneousAdjust Then
                            x(i) = GetMnpVarValue(form, adj)
                            i += 1
                        End If
                    Next

                    ic = 0
                    Do

                        fx = FunctionValue(form, x)

                        il_err_ant = il_err
                        il_err = 0
                        For i = 0 To x.Length - 1
                            il_err += (fx(i) / x(i)) ^ 2
                        Next

                        form.FormSurface.LabelSimultAdjInfo.Text = "Iteration #" & ic + 1 & ", NSSE: " & il_err

                        Application.DoEvents()

                        If il_err < 0.0000000001 Then Exit Do

                        dfdx = FunctionGradient(form, x)

                        Dim success As Boolean
                        success = MathEx.SysLin.rsolve.rmatrixsolve(dfdx, fx, x.Length, dx)
                        If success Then
                            For i = 0 To x.Length - 1
                                dx(i) = -dx(i)
                                x(i) += dx(i)
                            Next
                        End If

                        ic += 1

                        If ic >= 100 Then Throw New Exception(DWSIM.App.GetLocalString("SADJMaxIterationsReached"))
                        If Double.IsNaN(il_err) Then Throw New Exception(DWSIM.App.GetLocalString("SADJGeneralError"))
                        If Math.Abs(MathEx.Common.AbsSum(dx)) < 0.000001 Then Exit Do

                    Loop
                Catch ex As Exception
                    form.WriteToLog(DWSIM.App.GetLocalString("SADJGeneralError") & ": " & ex.Message.ToString, Color.Red, FormClasses.TipoAviso.Erro)
                Finally
                    form.FormSurface.LabelSimultAdjInfo.Text = ""
                    form.FormSurface.PanelSimultAdjust.Visible = False
                End Try


            End If

        End Sub

        Private Shared Sub SolveSimultaneousAdjustsAsync(ByVal form As FormFlowsheet)

            If form.m_simultadjustsolverenabled Then

                Dim n As Integer = 0

                For Each adj As SimulationObjects.SpecialOps.Adjust In form.Collections.CLCS_AdjustCollection.Values
                    If adj.SimultaneousAdjust Then n += 1
                Next

                n -= 1

                Dim i As Integer = 0
                Dim dfdx(n, n), dx(n), fx(n), x(n) As Double
                Dim il_err_ant As Double = 10000000000.0
                Dim il_err As Double = 10000000000.0
                Dim ic As Integer

                i = 0
                For Each adj As SimulationObjects.SpecialOps.Adjust In form.Collections.CLCS_AdjustCollection.Values
                    If adj.SimultaneousAdjust Then
                        x(i) = GetMnpVarValue(form, adj)
                        i += 1
                    End If
                Next

                ic = 0
                Do

                    fx = FunctionValue(form, x)

                    il_err_ant = il_err
                    il_err = 0
                    For i = 0 To x.Length - 1
                        il_err += (fx(i) / x(i)) ^ 2
                    Next

                    If il_err < 0.0000000001 Then Exit Do

                    dfdx = FunctionGradient(form, x)

                    Dim success As Boolean
                    success = MathEx.SysLin.rsolve.rmatrixsolve(dfdx, fx, x.Length, dx)
                    If success Then
                        For i = 0 To x.Length - 1
                            dx(i) = -dx(i)
                            x(i) += dx(i)
                        Next
                    End If

                    ic += 1

                    If ic >= 100 Then Throw New Exception(DWSIM.App.GetLocalString("SADJMaxIterationsReached"))
                    If Double.IsNaN(il_err) Then Throw New Exception(DWSIM.App.GetLocalString("SADJGeneralError"))
                    If Math.Abs(MathEx.Common.AbsSum(dx)) < 0.000001 Then Exit Do

                Loop

            End If

        End Sub

        Private Shared Function FunctionValue(ByVal form As FormFlowsheet, ByVal x() As Double) As Double()

            Dim i As Integer = 0
            For Each adj As SimulationObjects.SpecialOps.Adjust In form.Collections.CLCS_AdjustCollection.Values
                If adj.SimultaneousAdjust Then
                    SetMnpVarValue(x(i), form, adj)
                    i += 1
                End If
            Next

            For Each adj As SimulationObjects.SpecialOps.Adjust In form.Collections.CLCS_AdjustCollection.Values
                If adj.SimultaneousAdjust Then
                    CalculateObject(form, adj.ManipulatedObject.Nome)
                End If
            Next

            Dim fx(x.Length - 1) As Double
            i = 0
            For Each adj As SimulationObjects.SpecialOps.Adjust In form.Collections.CLCS_AdjustCollection.Values
                If adj.SimultaneousAdjust Then
                    If adj.Referenced Then
                        fx(i) = adj.AdjustValue + GetRefVarValue(form, adj) - GetCtlVarValue(form, adj)
                    Else
                        fx(i) = adj.AdjustValue - GetCtlVarValue(form, adj)
                    End If
                    i += 1
                End If
            Next

            Return fx

        End Function

        Private Shared Function FunctionGradient(ByVal form As FormFlowsheet, ByVal x() As Double) As Double(,)

            Dim epsilon As Double = 0.01

            Dim f2(), f3() As Double
            Dim g(x.Length - 1, x.Length - 1), x1(x.Length - 1), x2(x.Length - 1), x3(x.Length - 1), x4(x.Length - 1) As Double
            Dim i, j, k As Integer

            For i = 0 To x.Length - 1
                For j = 0 To x.Length - 1
                    If i <> j Then
                        x2(j) = x(j)
                        x3(j) = x(j)
                    Else
                        If x(j) <> 0.0# Then
                            x2(j) = x(j) * (1 + epsilon)
                            x3(j) = x(j) * (1 - epsilon)
                        Else
                            x2(j) = x(j) + epsilon
                            x3(j) = x(j)
                        End If
                    End If
                Next
                f2 = FunctionValue(form, x2)
                f3 = FunctionValue(form, x3)
                For k = 0 To x.Length - 1
                    g(k, i) = (f2(k) - f3(k)) / (x2(i) - x3(i))
                Next
            Next

            Return g

        End Function

        Private Shared Function GetCtlVarValue(ByVal form As FormFlowsheet, ByVal adj As SimulationObjects.SpecialOps.Adjust)

            With adj.ControlledObjectData
                Return form.Collections.ObjectCollection(.m_ID).GetPropertyValue(.m_Property)
            End With

        End Function

        Private Shared Function GetMnpVarValue(ByVal form As FormFlowsheet, ByVal adj As SimulationObjects.SpecialOps.Adjust)

            With adj.ManipulatedObjectData()
                Return form.Collections.ObjectCollection(.m_ID).GetPropertyValue(.m_Property)
            End With

        End Function

        Private Shared Function SetMnpVarValue(ByVal val As Nullable(Of Double), ByVal form As FormFlowsheet, ByVal adj As SimulationObjects.SpecialOps.Adjust)

            With adj.ManipulatedObjectData()
                form.Collections.ObjectCollection(.m_ID).SetPropertyValue(.m_Property, val)
            End With

            Return 1

        End Function

        Private Shared Function GetRefVarValue(ByVal form As FormFlowsheet, ByVal adj As SimulationObjects.SpecialOps.Adjust)

            With adj.ManipulatedObjectData
                With adj.ControlledObjectData()
                    Return form.Collections.ObjectCollection(.m_ID).GetPropertyValue(.m_Name, form.Options.SelectedUnitSystem)
                End With
            End With

        End Function

    End Class

    <System.Serializable()> Public Class COMSolver

        Public Sub CalculateFlowsheet(ByVal form As FormFlowsheet, ByVal objArgs As DWSIM.Outros.StatusChangeEventArgs, ByVal sender As Object)
            FlowsheetSolver.CalculateFlowsheet(form, objArgs, sender)
        End Sub

        Public Sub CalculateMaterialStream(ByVal form As FormFlowsheet, ByVal ms As DWSIM.SimulationObjects.Streams.MaterialStream, Optional ByVal DoNotCalcFlash As Boolean = False)
            FlowsheetSolver.CalculateMaterialStream(form, ms, DoNotCalcFlash)
        End Sub

        Public Sub DeCalculateObject(ByVal form As FormFlowsheet, ByVal obj As GraphicObject)
            FlowsheetSolver.DeCalculateObject(form, obj)
        End Sub

        Public Sub DeCalculateDisconnectedObject(ByVal form As FormFlowsheet, ByVal obj As GraphicObject, ByVal side As String)
            FlowsheetSolver.DeCalculateDisconnectedObject(form, obj, side)
        End Sub

        Public Sub ProcessCalculationQueue(ByVal form As FormFlowsheet)
            FlowsheetSolver.ProcessCalculationQueue(form)
        End Sub

        Public Sub CheckCalculatorStatus()
            FlowsheetSolver.CheckCalculatorStatus()
        End Sub

        Public Sub CalculateAll(ByVal form As FormFlowsheet)
            FlowsheetSolver.CalculateAll(form)
        End Sub

        Public Sub CalculateObject(ByVal form As FormFlowsheet, ByVal ObjID As String)
            FlowsheetSolver.CalculateObject(form, ObjID)
        End Sub

    End Class

End Namespace
