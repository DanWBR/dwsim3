'    DWSIM Flowsheet Solver & Auxiliary Functions
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

Imports Microsoft.Msdn.Samples.GraphicObjects
Imports System.Collections.Generic
Imports System.ComponentModel
Imports PropertyGridEx
Imports WeifenLuo.WinFormsUI
Imports System.Drawing
Imports System.IO
Imports DWSIM.DWSIM.SimulationObjects

Namespace DWSIM.Flowsheet

    <System.Serializable()> Public Class FlowsheetSolver

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

                Select Case objArgs.Tipo
                    Case TipoObjeto.MaterialStream
                        Dim myObj As DWSIM.SimulationObjects.Streams.MaterialStream = form.Collections.CLCS_MaterialStreamCollection(objArgs.Nome)
                        Dim gobj As GraphicObject = myObj.GraphicObject
                        If Not gobj Is Nothing Then
                            If gobj.OutputConnectors(0).IsAttached = True Then
                                Dim myUnitOp As SimulationObjects_UnitOpBaseClass
                                myUnitOp = form.Collections.ObjectCollection(myObj.GraphicObject.OutputConnectors(0).AttachedConnector.AttachedTo.Name)
                                If objArgs.Emissor = "Spec" Then
                                    CalculateMaterialStream(form, myObj)
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
                                            form.UpdateStatusLabel(preLab)
                                            myUnitOp.ErrorMessage = ex.Message
                                            form.WriteToLog(gobj.Tag & ": " & ex.Message, Color.Red, DWSIM.FormClasses.TipoAviso.Erro)
                                            gobj = myUnitOp.GraphicObject
                                            gobj.Calculated = False
                                            myUnitOp.DeCalculate()
                                        Finally
                                            form.FormSurface.FlowsheetDesignSurface.drawingObjects.Remove(gObjA)
                                            form.FormSurface.FlowsheetDesignSurface.Invalidate()
                                        End Try
                                    Else
                                        myUnitOp.DeCalculate()
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
                            If gobj.OutputConnectors(0).IsAttached = True Then
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
                                        form.UpdateStatusLabel(preLab)
                                        myUnitOp.GraphicObject.Calculated = False
                                        myUnitOp.ErrorMessage = ex.Message
                                        form.WriteToLog(gobj.Tag & ": " & ex.Message, Color.Red, DWSIM.FormClasses.TipoAviso.Erro)
                                        gobj = myUnitOp.GraphicObject
                                        gobj.Calculated = False
                                        myUnitOp.DeCalculate()
                                        myUnitOp.UpdatePropertyNodes(form.Options.SelectedUnitSystem, form.Options.NumberFormat)
                                    End Try
                                Else
                                    myUnitOp.DeCalculate()
                                    myUnitOp.GraphicObject.Calculated = False
                                End If
                                myObj.UpdatePropertyNodes(form.Options.SelectedUnitSystem, form.Options.NumberFormat)
                                form.FormSurface.Refresh()
                            End If
                        End If
                    Case Else
                        If objArgs.Emissor = "PropertyGrid" Or objArgs.Emissor = "Adjust" Or OnlyMe Then
                            Dim myObj As SimulationObjects_UnitOpBaseClass = form.Collections.ObjectCollection(objArgs.Nome)
                            myObj.GraphicObject.Calculated = False
                            form.UpdateStatusLabel(DWSIM.App.GetLocalString("Calculando") & " " & myObj.GraphicObject.Tag & "... (PP: " & myObj.PropertyPackage.Tag & " [" & myObj.PropertyPackage.ComponentName & "])")
                            myObj.Solve()
                            form.WriteToLog(objArgs.Tag & ": " & DWSIM.App.GetLocalString("Calculadocomsucesso"), Color.DarkGreen, DWSIM.FormClasses.TipoAviso.Informacao)
                            myObj.GraphicObject.Calculated = True
                            If myObj.IsSpecAttached = True And myObj.SpecVarType = DWSIM.SimulationObjects.SpecialOps.Helpers.Spec.TipoVar.Fonte Then form.Collections.CLCS_SpecCollection(myObj.AttachedSpecId).Calculate()
                            form.FormProps.PGEx1.Refresh()
                            myObj.UpdatePropertyNodes(form.Options.SelectedUnitSystem, form.Options.NumberFormat)
                        Else
                            Dim myObj As SimulationObjects_UnitOpBaseClass = form.Collections.ObjectCollection(objArgs.Nome)
                            Dim gobj As GraphicObject = FormFlowsheet.SearchSurfaceObjectsByName(objArgs.Nome, form.FormSurface.FlowsheetDesignSurface)
                            For Each cp As ConnectionPoint In gobj.OutputConnectors
                                If cp.IsAttached And cp.Type = ConType.ConOut Then
                                    Dim ms As SimulationObjects.Streams.MaterialStream = form.Collections.CLCS_MaterialStreamCollection(cp.AttachedConnector.AttachedTo.Name)
                                    Try
                                        ms.GraphicObject.Calculated = False
                                        form.UpdateStatusLabel(DWSIM.App.GetLocalString("Calculando") & " " & ms.GraphicObject.Tag & "... (PP: " & ms.PropertyPackage.Tag & " [" & ms.PropertyPackage.ComponentName & "])")
                                        CalculateMaterialStream(form, ms)
                                        ms.GraphicObject.Calculated = True
                                    Catch ex As Exception
                                        ms.GraphicObject.Calculated = False
                                        ms.Clear()
                                        ms.ClearAllProps()
                                        ms.UpdatePropertyNodes(form.Options.SelectedUnitSystem, form.Options.NumberFormat)
                                        form.WriteToLog(gobj.Tag & ": " & ex.Message, Color.Red, DWSIM.FormClasses.TipoAviso.Erro)
                                    End Try
                                End If
                            Next
                            myObj.UpdatePropertyNodes(form.Options.SelectedUnitSystem, form.Options.NumberFormat)
                        End If
                End Select

            End If

            Application.DoEvents()
            form.FormSurface.LabelCalculator.Text = preLab

        End Sub

        ''' <summary>
        ''' Material Stream calculation routine 1. This routine check all input values and calculates all remaining properties of the stream.
        ''' </summary>
        ''' <param name="form">Flowsheet to what the stream belongs to.</param>
        ''' <param name="ms">Material Stream object to be calculated.</param>
        ''' <param name="DoNotCalcFlash">Tells the calculator whether to do flash calculations or not.</param>
        ''' <remarks></remarks>
        Public Shared Sub CalculateMaterialStream(ByVal form As FormFlowsheet, ByVal ms As DWSIM.SimulationObjects.Streams.MaterialStream, Optional ByVal DoNotCalcFlash As Boolean = False, Optional ByVal OnlyMe As Boolean = False)

            Dim preLab As String = form.FormSurface.LabelCalculator.Text
            form.UpdateStatusLabel(DWSIM.App.GetLocalString("Calculando") & " " & ms.GraphicObject.Tag & "... (PP: " & ms.PropertyPackage.Tag & " [" & ms.PropertyPackage.ComponentName & "])")

            If ms.Fases.Count <= 3 Then
                ms.Fases.Add("3", New DWSIM.ClassesBasicasTermodinamica.Fase(DWSIM.App.GetLocalString("Liquid1"), ""))
                ms.Fases.Add("4", New DWSIM.ClassesBasicasTermodinamica.Fase(DWSIM.App.GetLocalString("Liquid2"), ""))
                ms.Fases.Add("5", New DWSIM.ClassesBasicasTermodinamica.Fase(DWSIM.App.GetLocalString("Liquid3"), ""))
                ms.Fases.Add("6", New DWSIM.ClassesBasicasTermodinamica.Fase(DWSIM.App.GetLocalString("Aqueous"), ""))
                ms.Fases.Add("7", New DWSIM.ClassesBasicasTermodinamica.Fase(DWSIM.App.GetLocalString("Solid"), ""))
                If form.Options.SelectedComponents.Count = 0 Then
                    MessageBox.Show(DWSIM.App.GetLocalString("Nohcomponentesaadici"))
                Else
                    Dim comp2 As DWSIM.ClassesBasicasTermodinamica.ConstantProperties
                    For Each comp2 In form.Options.SelectedComponents.Values
                        ms.Fases(3).Componentes.Add(comp2.Name, New DWSIM.ClassesBasicasTermodinamica.Substancia(comp2.Name, ""))
                        ms.Fases(3).Componentes(comp2.Name).ConstantProperties = comp2
                        ms.Fases(4).Componentes.Add(comp2.Name, New DWSIM.ClassesBasicasTermodinamica.Substancia(comp2.Name, ""))
                        ms.Fases(4).Componentes(comp2.Name).ConstantProperties = comp2
                        ms.Fases(5).Componentes.Add(comp2.Name, New DWSIM.ClassesBasicasTermodinamica.Substancia(comp2.Name, ""))
                        ms.Fases(5).Componentes(comp2.Name).ConstantProperties = comp2
                        ms.Fases(6).Componentes.Add(comp2.Name, New DWSIM.ClassesBasicasTermodinamica.Substancia(comp2.Name, ""))
                        ms.Fases(6).Componentes(comp2.Name).ConstantProperties = comp2
                    Next
                End If
            ElseIf ms.Fases.Count <= 6 Then
                ms.Fases.Add("6", New DWSIM.ClassesBasicasTermodinamica.Fase(DWSIM.App.GetLocalString("Aqueous"), ""))
                If form.Options.SelectedComponents.Count = 0 Then
                    MessageBox.Show(DWSIM.App.GetLocalString("Nohcomponentesaadici"))
                Else
                    Dim comp2 As DWSIM.ClassesBasicasTermodinamica.ConstantProperties
                    For Each comp2 In form.Options.SelectedComponents.Values
                        ms.Fases(6).Componentes.Add(comp2.Name, New DWSIM.ClassesBasicasTermodinamica.Substancia(comp2.Name, ""))
                        ms.Fases(6).Componentes(comp2.Name).ConstantProperties = comp2
                    Next
                End If
            ElseIf ms.Fases.Count <= 7 Then
                ms.Fases.Add("7", New DWSIM.ClassesBasicasTermodinamica.Fase(DWSIM.App.GetLocalString("Solid"), ""))
                If form.Options.SelectedComponents.Count = 0 Then
                    MessageBox.Show(DWSIM.App.GetLocalString("Nohcomponentesaadici"))
                Else
                    Dim comp2 As DWSIM.ClassesBasicasTermodinamica.ConstantProperties
                    For Each comp2 In form.Options.SelectedComponents.Values
                        ms.Fases(7).Componentes.Add(comp2.Name, New DWSIM.ClassesBasicasTermodinamica.Substancia(comp2.Name, ""))
                        ms.Fases(7).Componentes(comp2.Name).ConstantProperties = comp2
                    Next
                End If
            End If

            Dim sobj As Microsoft.MSDN.Samples.GraphicObjects.GraphicObject = form.Collections.MaterialStreamCollection(ms.Nome)

            Dim T As Double = ms.Fases(0).SPMProperties.temperature.GetValueOrDefault
            Dim P As Double = ms.Fases(0).SPMProperties.pressure.GetValueOrDefault
            Dim W As Double = ms.Fases(0).SPMProperties.massflow.GetValueOrDefault
            Dim Q As Double = ms.Fases(0).SPMProperties.molarflow.GetValueOrDefault
            Dim QV As Double = ms.Fases(0).SPMProperties.volumetric_flow.GetValueOrDefault
            Dim H As Double = ms.Fases(0).SPMProperties.enthalpy.GetValueOrDefault

            Dim subs As DWSIM.ClassesBasicasTermodinamica.Substancia
            Dim comp As Double = 0
            For Each subs In ms.Fases(0).Componentes.Values
                comp += subs.FracaoMolar.GetValueOrDefault
            Next

            If W >= 0 And T > 0 And P > 0 And comp >= 0 Then
                With ms.PropertyPackage
                    .CurrentMaterialStream = ms
                    .DW_CalcVazaoMolar()
                    If DoNotCalcFlash Then
                        '.DW_CalcEquilibrium(DWSIM.SimulationObjects.PropertyPackages.FlashSpec.P, DWSIM.SimulationObjects.PropertyPackages.FlashSpec.H)
                    ElseIf form.Options.SempreCalcularFlashPH And H <> 0 Then
                        .DW_CalcEquilibrium(DWSIM.SimulationObjects.PropertyPackages.FlashSpec.P, DWSIM.SimulationObjects.PropertyPackages.FlashSpec.H)
                    Else
                        If .AUX_IS_SINGLECOMP(PropertyPackages.Fase.Mixture) Then
                            If ms.GraphicObject.InputConnectors(0).IsAttached Then
                                .DW_CalcEquilibrium(DWSIM.SimulationObjects.PropertyPackages.FlashSpec.P, DWSIM.SimulationObjects.PropertyPackages.FlashSpec.H)
                            Else
                                .DW_CalcEquilibrium(DWSIM.SimulationObjects.PropertyPackages.FlashSpec.T, DWSIM.SimulationObjects.PropertyPackages.FlashSpec.P)
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
                        .DW_CalcPhaseProps(DWSIM.SimulationObjects.PropertyPackages.Fase.Solid)
                    Else
                        .DW_ZerarPhaseProps(DWSIM.SimulationObjects.PropertyPackages.Fase.Solid)
                    End If
                    If ms.Fases(2).SPMProperties.molarfraction.GetValueOrDefault > 0 Then
                        .DW_CalcPhaseProps(DWSIM.SimulationObjects.PropertyPackages.Fase.Vapor)
                    Else
                        .DW_ZerarPhaseProps(DWSIM.SimulationObjects.PropertyPackages.Fase.Vapor)
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
                If Not OnlyMe Then
                    Dim objargs As New DWSIM.Outros.StatusChangeEventArgs
                    With objargs
                        .Calculado = True
                        .Nome = ms.Nome
                        .Tipo = TipoObjeto.MaterialStream
                    End With
                    CalculateFlowsheet(form, objargs, Nothing)
                End If
            ElseIf Q >= 0 And T > 0 And P > 0 And comp >= 0 Then
                With ms.PropertyPackage
                    .CurrentMaterialStream = ms
                    .DW_CalcVazaoMassica()
                    If DoNotCalcFlash Then
                        '.DW_CalcEquilibrium(DWSIM.SimulationObjects.PropertyPackages.FlashSpec.P, DWSIM.SimulationObjects.PropertyPackages.FlashSpec.H)
                    ElseIf form.Options.SempreCalcularFlashPH And H <> 0 Then
                        .DW_CalcEquilibrium(DWSIM.SimulationObjects.PropertyPackages.FlashSpec.P, DWSIM.SimulationObjects.PropertyPackages.FlashSpec.H)
                    Else
                        If .AUX_IS_SINGLECOMP(PropertyPackages.Fase.Mixture) Then
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
                    End If
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
                        .DW_CalcPhaseProps(DWSIM.SimulationObjects.PropertyPackages.Fase.Solid)
                    Else
                        .DW_ZerarPhaseProps(DWSIM.SimulationObjects.PropertyPackages.Fase.Solid)
                    End If
                    If ms.Fases(2).SPMProperties.molarfraction.GetValueOrDefault > 0 Then
                        .DW_CalcPhaseProps(DWSIM.SimulationObjects.PropertyPackages.Fase.Vapor)
                    Else
                        .DW_ZerarPhaseProps(DWSIM.SimulationObjects.PropertyPackages.Fase.Vapor)
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
                If Not OnlyMe Then
                    Dim objargs As New DWSIM.Outros.StatusChangeEventArgs
                    With objargs
                        .Calculado = True
                        .Nome = ms.Nome
                        .Tipo = TipoObjeto.MaterialStream
                    End With
                    CalculateFlowsheet(form, objargs, Nothing)
                End If
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
                    '.DW_ZerarComposicoes(DWSIM.SimulationObjects.PropertyPackages.Fase.Mixture)
                    .DW_ZerarOverallProps()
                    .CurrentMaterialStream = Nothing
                End With
                ms.UpdatePropertyNodes(form.Options.SelectedUnitSystem, form.Options.NumberFormat)
                sobj.Calculated = False
                form.UpdateStatusLabel(preLab)
                If Not OnlyMe Then
                    Dim objargs As New DWSIM.Outros.StatusChangeEventArgs
                    With objargs
                        .Calculado = False
                        .Nome = ms.Nome
                        .Tipo = TipoObjeto.MaterialStream
                    End With
                    CalculateFlowsheet(form, objargs, Nothing)
                End If
            End If

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
        Public Shared Sub ProcessCalculationQueue(ByVal form As FormFlowsheet)

            ProcessQueueInternal(form)

            SolveSimultaneousAdjusts(form)

        End Sub

        Private Shared Sub ProcessQueueInternal(ByVal form As FormFlowsheet)

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
                    If myinfo.Tipo = TipoObjeto.MaterialStream Then
                        CalculateMaterialStream(form, form.Collections.CLCS_MaterialStreamCollection(myinfo.Nome))
                    Else
                        CalculateFlowsheet(form, myinfo, Nothing)
                    End If
                Catch ex As Exception
                    form.WriteToLog(myinfo.Tag & ": " & ex.Message.ToString, Color.Red, FormClasses.TipoAviso.Erro)
                End Try

                form.FormWatch.UpdateList()

                For Each g As GraphicObject In form.FormSurface.FlowsheetDesignSurface.drawingObjects
                    If g.TipoObjeto = TipoObjeto.GO_MasterTable Then
                        CType(g, DWSIM.GraphicObjects.MasterTableGraphic).Update(form)
                    End If
                Next

                If form.CalculationQueue.Count = 1 Then form.FormSpreadsheet.InternalCounter = 0
                If form.CalculationQueue.Count > 0 Then form.CalculationQueue.Dequeue()

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

        Private Shared Sub ProcessQueue(ByVal form As FormFlowsheet, ByVal objqueue As Queue(Of Outros.StatusChangeEventArgs))

            form.FormSurface.LabelTime.Text = ""
            form.FormSurface.calcstart = Date.Now
            form.FormSurface.PictureBox3.Image = My.Resources.weather_lightning
            form.FormSurface.PictureBox4.Visible = True

            While objqueue.Count >= 1

                My.MyApplication.CalculatorStopRequested = False

                If form.FormSurface.Timer2.Enabled = False Then form.FormSurface.Timer2.Start()

                Dim myinfo As DWSIM.Outros.StatusChangeEventArgs = objqueue.Peek()

                Try
                    form.FormQueue.TextBox1.Clear()
                    For Each c As DWSIM.Outros.StatusChangeEventArgs In objqueue
                        form.FormQueue.TextBox1.AppendText(form.Collections.ObjectCollection(c.Nome).GraphicObject.Tag & vbTab & vbTab & vbTab & "[" & DWSIM.App.GetLocalString(form.Collections.ObjectCollection(c.Nome).Descricao) & "]" & vbCrLf)
                    Next
                Catch ex As Exception
                    'form.WriteToLog(ex.Message, Color.Black, DWSIM.FormClasses.TipoAviso.Erro)
                End Try

                Try
                    If myinfo.Tipo = TipoObjeto.MaterialStream Then
                        CalculateMaterialStream(form, form.Collections.CLCS_MaterialStreamCollection(myinfo.Nome), False, True)
                    Else
                        CalculateFlowsheet(form, myinfo, Nothing)
                    End If
                Catch ex As Exception
                    form.WriteToLog(myinfo.Tag & ": " & ex.Message.ToString, Color.Red, FormClasses.TipoAviso.Erro)
                End Try

                form.FormWatch.UpdateList()

                For Each g As GraphicObject In form.FormSurface.FlowsheetDesignSurface.drawingObjects
                    If g.TipoObjeto = TipoObjeto.GO_MasterTable Then
                        CType(g, DWSIM.GraphicObjects.MasterTableGraphic).Update(form)
                    End If
                Next

                If objqueue.Count = 1 Then form.FormSpreadsheet.InternalCounter = 0
                If objqueue.Count > 0 Then objqueue.Dequeue()

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


        ''' <summary>
        ''' Checks the calculator status to see if the user did any stop/abort request, and throws an exception to force aborting, if necessary.
        ''' </summary>
        ''' <remarks></remarks>
        Public Shared Sub CheckCalculatorStatus()

            If Not My.Application.IsRunningParallelTasks Then
                Application.DoEvents()
                If Not My.Application.CAPEOPENMode Then
                    If My.MyApplication.CalculatorStopRequested = True Then
                        My.MyApplication.CalculatorStopRequested = False
                        Throw New Exception(DWSIM.App.GetLocalString("CalculationAborted"))
                    End If
                End If
            End If

        End Sub

        ''' <summary>
        ''' Calculate all objects in the Flowsheet.
        ''' </summary>
        ''' <param name="form">Flowsheet to be calculated (FormChild object)</param>
        ''' <remarks></remarks>
        Public Shared Sub CalculateAll(ByVal form As FormFlowsheet)

            For Each baseobj As SimulationObjects_BaseClass In form.Collections.ObjectCollection.Values
                If baseobj.GraphicObject.TipoObjeto = TipoObjeto.MaterialStream And baseobj.GraphicObject.Calculated Then
                    Dim ms As Streams.MaterialStream = baseobj
                    If ms.GraphicObject.InputConnectors(0).IsAttached = False Then
                        'add this stream to the calculator queue list
                        Dim objargs As New DWSIM.Outros.StatusChangeEventArgs
                        With objargs
                            .Calculado = True
                            .Nome = ms.Nome
                            .Tipo = TipoObjeto.MaterialStream
                        End With
                        If ms.IsSpecAttached = True And ms.SpecVarType = DWSIM.SimulationObjects.SpecialOps.Helpers.Spec.TipoVar.Fonte Then
                            form.Collections.CLCS_SpecCollection(ms.AttachedSpecId).Calculate()
                        End If
                        If form.CalculationQueue Is Nothing Then form.CalculationQueue = New Queue(Of DWSIM.Outros.StatusChangeEventArgs)
                        form.CalculationQueue.Enqueue(objargs)
                    End If
                End If
            Next
            ProcessCalculationQueue(form)

        End Sub

        ''' <summary>
        ''' Calculates a single object in the Flowsheet.
        ''' </summary>
        ''' <param name="form">Flowsheet where the object belongs to.</param>
        ''' <param name="ObjID">Unique Id of the object ("Name" or "GraphicObject.Name" properties). This is not the object's Flowsheet display name ("Tag" property or its GraphicObject object).</param>
        ''' <remarks></remarks>
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
