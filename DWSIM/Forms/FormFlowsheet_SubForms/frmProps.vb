Imports WeifenLuo.WinFormsUI.Docking
Imports Microsoft.Msdn.Samples.GraphicObjects

Imports DWSIM.DWSIM.Flowsheet.FlowsheetSolver
Imports DWSIM.DWSIM
Imports System.Text
Imports PropertyGridEx

Public Class frmProps
    Inherits DockContent

#Region "    Declaração de eventos "

    Private Event ObjectStatusChanged(ByVal obj As Microsoft.MSDN.Samples.GraphicObjects.GraphicObject)

#End Region

    Private ChildParent As FormFlowsheet
    Private Conversor As New DWSIM.SistemasDeUnidades.Conversor

    Private Sub _Load(ByVal sender As System.Object, ByVal e As System.EventArgs) Handles MyBase.Load

        ChildParent = My.Application.ActiveSimulation

    End Sub

    Public Function ReturnForm(ByVal str As String) As IDockContent

        If str = Me.ToString Then
            Return Me
        Else
            Return Nothing
        End If

    End Function

    Public Sub PGEx1_PropertyValueChanged(ByVal s As Object, ByVal e As System.Windows.Forms.PropertyValueChangedEventArgs) Handles PGEx1.PropertyValueChanged

        ChildParent = Me.ParentForm

        Dim sobj As Microsoft.MSDN.Samples.GraphicObjects.GraphicObject = ChildParent.FormSurface.FlowsheetDesignSurface.SelectedObject

        'handle changes internally
        If Not sobj Is Nothing Then

            If sobj.TipoObjeto <> TipoObjeto.GO_Tabela And sobj.TipoObjeto <> TipoObjeto.GO_MasterTable And sobj.TipoObjeto <> TipoObjeto.GO_SpreadsheetTable Then

                ChildParent.Collections.ObjectCollection(sobj.Name).PropertyValueChanged(s, e)

            End If

        End If

        'more...

        ChildParent.FormSurface.FlowsheetDesignSurface.SelectedObject = sobj

        If Not sobj Is Nothing Then

            If sobj.TipoObjeto = TipoObjeto.FlowsheetUO Then

                Dim fs As DWSIM.SimulationObjects.UnitOps.Flowsheet = ChildParent.Collections.CLCS_FlowsheetUOCollection.Item(sobj.Name)

                If e.ChangedItem.PropertyDescriptor.Category.Equals(DWSIM.App.GetLocalString("LinkedInputParms")) Then

                    Dim pkey As String = CType(e.ChangedItem.PropertyDescriptor, CustomProperty.CustomPropertyDescriptor).CustomProperty.Tag

                    fs.Fsheet.Collections.ObjectCollection(fs.InputParams(pkey).ObjectID).SetPropertyValue(fs.InputParams(pkey).ObjectProperty, e.ChangedItem.Value, ChildParent.Options.SelectedUnitSystem)

                    If ChildParent.Options.CalculatorActivated Then

                        sobj.Calculated = True
                        RaiseEvent ObjectStatusChanged(sobj)

                        'Call function to calculate flowsheet
                        Dim objargs As New DWSIM.Outros.StatusChangeEventArgs
                        With objargs
                            .Calculado = True
                            .Nome = sobj.Name
                            .Tag = sobj.Tag
                            .Tipo = TipoObjeto.FlowsheetUO
                            .Emissor = "PropertyGrid"
                        End With

                        If fs.IsSpecAttached = True And fs.SpecVarType = DWSIM.SimulationObjects.SpecialOps.Helpers.Spec.TipoVar.Fonte Then ChildParent.Collections.CLCS_SpecCollection(fs.AttachedSpecId).Calculate()
                        ChildParent.CalculationQueue.Enqueue(objargs)

                    End If

                End If

            ElseIf sobj.TipoObjeto = TipoObjeto.GO_MasterTable Then

                Dim mt As DWSIM.GraphicObjects.MasterTableGraphic = sobj

                If e.ChangedItem.PropertyDescriptor.Category.Contains(DWSIM.App.GetLocalString("MT_PropertiesToShow")) Then
                    Dim pkey As String = CType(e.ChangedItem.PropertyDescriptor, CustomProperty.CustomPropertyDescriptor).CustomProperty.Tag
                    If Not mt.PropertyList.ContainsKey(pkey) Then
                        mt.PropertyList.Add(pkey, e.ChangedItem.Value)
                    Else
                        mt.PropertyList(pkey) = e.ChangedItem.Value
                    End If
                ElseIf e.ChangedItem.PropertyDescriptor.Category.Contains(DWSIM.App.GetLocalString("MT_ObjectsToShow")) Then
                    If Not mt.ObjectList.ContainsKey(e.ChangedItem.Label) Then
                        mt.ObjectList.Add(e.ChangedItem.Label, e.ChangedItem.Value)
                    Else
                        mt.ObjectList(e.ChangedItem.Label) = e.ChangedItem.Value
                    End If
                End If

                mt.Update(ChildParent)

            ElseIf sobj.TipoObjeto = TipoObjeto.MaterialStream Then

                Dim ms As DWSIM.SimulationObjects.Streams.MaterialStream = ChildParent.Collections.CLCS_MaterialStreamCollection.Item(sobj.Name)

                If Not e.ChangedItem.Label.Contains(DWSIM.App.GetLocalString("Base")) Then

                    Dim T, P, W, Q, QV, HM, SM, VF As Double

                    If e.ChangedItem.Label.Contains(DWSIM.App.GetLocalString("Temperatura")) Then
                        T = Conversor.ConverterParaSI(ChildParent.Options.SelectedUnitSystem.spmp_temperature, e.ChangedItem.Value)
                        ms.Fases(0).SPMProperties.temperature = T
                    ElseIf e.ChangedItem.Label.Contains(DWSIM.App.GetLocalString("Presso")) Then
                        P = Conversor.ConverterParaSI(ChildParent.Options.SelectedUnitSystem.spmp_pressure, e.ChangedItem.Value)
                        ms.Fases(0).SPMProperties.pressure = P
                    ElseIf e.ChangedItem.Label.Contains(DWSIM.App.GetLocalString("Vazomssica")) Then
                        W = Conversor.ConverterParaSI(ChildParent.Options.SelectedUnitSystem.spmp_massflow, e.ChangedItem.Value)
                        ms.Fases(0).SPMProperties.massflow = W
                        ms.Fases(0).SPMProperties.molarflow = Nothing
                        ms.Fases(0).SPMProperties.volumetric_flow = Nothing
                    ElseIf e.ChangedItem.Label.Contains(DWSIM.App.GetLocalString("Vazomolar")) Then
                        Q = Conversor.ConverterParaSI(ChildParent.Options.SelectedUnitSystem.spmp_molarflow, e.ChangedItem.Value)
                        ms.Fases(0).SPMProperties.molarflow = Q
                        ms.Fases(0).SPMProperties.massflow = Nothing
                        ms.Fases(0).SPMProperties.volumetric_flow = Nothing
                    ElseIf e.ChangedItem.Label.Contains(DWSIM.App.GetLocalString("Vazovolumtrica")) Then
                        QV = Conversor.ConverterParaSI(ChildParent.Options.SelectedUnitSystem.spmp_volumetricFlow, e.ChangedItem.Value)
                        ms.Fases(0).SPMProperties.volumetric_flow = QV
                        ms.Fases(0).SPMProperties.massflow = Nothing
                        ms.Fases(0).SPMProperties.molarflow = Nothing
                    ElseIf e.ChangedItem.Label.Contains(DWSIM.App.GetLocalString("EntalpiaEspecfica")) Then
                        HM = Conversor.ConverterParaSI(ChildParent.Options.SelectedUnitSystem.spmp_enthalpy, e.ChangedItem.Value)
                        ms.Fases(0).SPMProperties.enthalpy = HM
                    ElseIf e.ChangedItem.Label.Contains(DWSIM.App.GetLocalString("EntropiaEspecfica")) Then
                        SM = Conversor.ConverterParaSI(ChildParent.Options.SelectedUnitSystem.spmp_entropy, e.ChangedItem.Value)
                        ms.Fases(0).SPMProperties.entropy = SM
                    ElseIf e.ChangedItem.Label.Contains(DWSIM.App.GetPropertyName("PROP_MS_106")) Then
                        VF = e.ChangedItem.Value
                        ms.Fases(2).SPMProperties.molarfraction = VF
                    End If

                    'Call function to calculate flowsheet
                    Dim objargs As New DWSIM.Outros.StatusChangeEventArgs
                    With objargs
                        .Calculado = False
                        .Nome = sobj.Name
                        .Tag = sobj.Tag
                        .Tipo = TipoObjeto.MaterialStream
                        .Emissor = "PropertyGrid"
                    End With

                    ChildParent.CalculationQueue.Enqueue(objargs)

                End If

            ElseIf sobj.TipoObjeto = TipoObjeto.EnergyStream Then

                Dim es As DWSIM.SimulationObjects.Streams.EnergyStream = ChildParent.Collections.CLCS_EnergyStreamCollection.Item(sobj.Name)

                If e.ChangedItem.Label.Contains(DWSIM.App.GetLocalString("Energia")) Then

                    es.Energia = Conversor.ConverterParaSI(ChildParent.Options.SelectedUnitSystem.spmp_heatflow, e.ChangedItem.Value)

                End If

                If ChildParent.Options.CalculatorActivated Then

                    sobj.Calculated = True
                    RaiseEvent ObjectStatusChanged(sobj)

                    'Call function to calculate flowsheet
                    Dim objargs As New DWSIM.Outros.StatusChangeEventArgs
                    With objargs
                        .Calculado = True
                        .Nome = sobj.Name
                        .Tag = sobj.Tag
                        .Tipo = TipoObjeto.EnergyStream
                        .Emissor = "PropertyGrid"
                    End With

                    If es.IsSpecAttached = True And es.SpecVarType = DWSIM.SimulationObjects.SpecialOps.Helpers.Spec.TipoVar.Fonte Then ChildParent.Collections.CLCS_SpecCollection(es.AttachedSpecId).Calculate()
                    ChildParent.CalculationQueue.Enqueue(objargs)

                End If


            ElseIf sobj.TipoObjeto = TipoObjeto.NodeOut Then

                Dim sp As DWSIM.SimulationObjects.UnitOps.Splitter = ChildParent.Collections.CLCS_SplitterCollection.Item(sobj.Name)

                If e.ChangedItem.Label.Contains("[Split Ratio] ") Then

                    If e.ChangedItem.Value < 0.0# Or e.ChangedItem.Value > 1.0# Then Throw New InvalidCastException(DWSIM.App.GetLocalString("Ovalorinformadonovli"))

                    Dim i As Integer = 0
                    Dim j As Integer = 0

                    Dim cp As ConnectionPoint
                    For Each cp In sp.GraphicObject.OutputConnectors
                        If cp.IsAttached Then
                            If e.ChangedItem.Label.Contains(cp.AttachedConnector.AttachedTo.Tag) Then
                                sp.Ratios(i) = e.ChangedItem.Value
                            End If
                        End If
                        i += 1
                    Next

                ElseIf e.ChangedItem.Label.Contains(DWSIM.App.GetPropertyName("PROP_SP_1")) Then

                    Select Case sp.OperationMode
                        Case SimulationObjects.UnitOps.Splitter.OpMode.StreamMassFlowSpec
                            sp.StreamFlowSpec = Conversor.ConverterParaSI(ChildParent.Options.SelectedUnitSystem.spmp_massflow, e.ChangedItem.Value)
                        Case SimulationObjects.UnitOps.Splitter.OpMode.StreamMoleFlowSpec
                            sp.StreamFlowSpec = Conversor.ConverterParaSI(ChildParent.Options.SelectedUnitSystem.spmp_molarflow, e.ChangedItem.Value)
                    End Select

                End If

                If ChildParent.Options.CalculatorActivated Then

                    sobj.Calculated = True

                    RaiseEvent ObjectStatusChanged(sobj)

                    'Call function to calculate flowsheet
                    Dim objargs As New DWSIM.Outros.StatusChangeEventArgs
                    With objargs
                        .Calculado = False
                        .Nome = sobj.Name
                        .Tag = sobj.Tag
                        .Tipo = TipoObjeto.NodeOut
                        .Emissor = "PropertyGrid"
                    End With

                    If sp.IsSpecAttached = True And sp.SpecVarType = DWSIM.SimulationObjects.SpecialOps.Helpers.Spec.TipoVar.Fonte Then ChildParent.Collections.CLCS_SpecCollection(sp.AttachedSpecId).Calculate()
                    ChildParent.CalculationQueue.Enqueue(objargs)

                End If

            ElseIf sobj.TipoObjeto = TipoObjeto.Pump Then

                Dim bb As DWSIM.SimulationObjects.UnitOps.Pump = ChildParent.Collections.CLCS_PumpCollection.Item(sobj.Name)

                If e.ChangedItem.Label.Contains("Delta P") Then

                    If e.ChangedItem.Value < 0 Then Throw New InvalidCastException(DWSIM.App.GetLocalString("Ovalorinformadonovli"))
                    bb.DeltaP = Conversor.ConverterParaSI(ChildParent.Options.SelectedUnitSystem.spmp_deltaP, e.ChangedItem.Value)

                ElseIf e.ChangedItem.Label.Contains(DWSIM.App.GetLocalString("Pressoajusante")) Then

                    If e.ChangedItem.Value < 0 Then Throw New InvalidCastException(DWSIM.App.GetLocalString("Ovalorinformadonovli"))
                    bb.Pout = Conversor.ConverterParaSI(ChildParent.Options.SelectedUnitSystem.spmp_pressure, e.ChangedItem.Value)

                ElseIf e.ChangedItem.Label.Contains(DWSIM.App.GetLocalString("Eficincia")) Then

                    If e.ChangedItem.Value <= 20 Or e.ChangedItem.Value > 100 Then Throw New InvalidCastException(DWSIM.App.GetLocalString("Ovalorinformadonovli"))

                End If

                If ChildParent.Options.CalculatorActivated Then

                    'Call function to calculate flowsheet
                    Dim objargs As New DWSIM.Outros.StatusChangeEventArgs
                    With objargs
                        .Calculado = False
                        .Tag = sobj.Tag
                        .Nome = sobj.Name
                        .Tipo = TipoObjeto.Pump
                        .Emissor = "PropertyGrid"
                    End With

                    If bb.IsSpecAttached = True And bb.SpecVarType = DWSIM.SimulationObjects.SpecialOps.Helpers.Spec.TipoVar.Fonte Then ChildParent.Collections.CLCS_SpecCollection(bb.AttachedSpecId).Calculate()
                    ChildParent.CalculationQueue.Enqueue(objargs)

                End If

            ElseIf sobj.TipoObjeto = TipoObjeto.Valve Then

                Dim bb As DWSIM.SimulationObjects.UnitOps.Valve = ChildParent.Collections.CLCS_ValveCollection.Item(sobj.Name)

                If e.ChangedItem.Label.Contains(DWSIM.App.GetLocalString("Quedadepresso")) Then

                    bb.DeltaP = Conversor.ConverterParaSI(ChildParent.Options.SelectedUnitSystem.spmp_deltaP, e.ChangedItem.Value)

                    'If e.ChangedItem.Value < 0 Then Throw New InvalidCastException(DWSIM.App.GetLocalString("Ovalorinformadonovli"))

                ElseIf e.ChangedItem.Label.Contains(DWSIM.App.GetLocalString("ValveOutletPressure")) Then

                    bb.OutletPressure = Conversor.ConverterParaSI(ChildParent.Options.SelectedUnitSystem.spmp_pressure, e.ChangedItem.Value)

                End If

                If ChildParent.Options.CalculatorActivated Then

                    'Call function to calculate flowsheet
                    Dim objargs As New DWSIM.Outros.StatusChangeEventArgs
                    With objargs
                        .Calculado = False
                        .Tag = sobj.Tag
                        .Nome = sobj.Name
                        .Tipo = TipoObjeto.Valve
                        .Emissor = "PropertyGrid"
                    End With

                    If bb.IsSpecAttached = True And bb.SpecVarType = DWSIM.SimulationObjects.SpecialOps.Helpers.Spec.TipoVar.Fonte Then ChildParent.Collections.CLCS_SpecCollection(bb.AttachedSpecId).Calculate()
                    ChildParent.CalculationQueue.Enqueue(objargs)

                End If

            ElseIf sobj.TipoObjeto = TipoObjeto.Filter Then

                Dim ft As DWSIM.SimulationObjects.UnitOps.Filter = ChildParent.Collections.CLCS_FilterCollection.Item(sobj.Name)

                If e.ChangedItem.Label.Contains(DWSIM.App.GetLocalString("FilterMediumResistance")) Then
                    ft.FilterMediumResistance = Conversor.ConverterParaSI(ChildParent.Options.SelectedUnitSystem.mediumresistance, e.ChangedItem.Value)
                ElseIf e.ChangedItem.Label.Contains(DWSIM.App.GetLocalString("FilterSpecificCakeResistance")) Then
                    ft.SpecificCakeResistance = Conversor.ConverterParaSI(ChildParent.Options.SelectedUnitSystem.cakeresistance, e.ChangedItem.Value)
                ElseIf e.ChangedItem.Label.Contains(DWSIM.App.GetLocalString("FilterCycleTime")) Then
                    ft.FilterCycleTime = Conversor.ConverterParaSI(ChildParent.Options.SelectedUnitSystem.time, e.ChangedItem.Value)
                ElseIf e.ChangedItem.Label.Contains(DWSIM.App.GetLocalString("FilterPressureDrop")) Then
                    ft.PressureDrop = Conversor.ConverterParaSI(ChildParent.Options.SelectedUnitSystem.spmp_deltaP, e.ChangedItem.Value)
                ElseIf e.ChangedItem.Label.Contains(DWSIM.App.GetLocalString("FilterArea")) Then
                    ft.TotalFilterArea = Conversor.ConverterParaSI(ChildParent.Options.SelectedUnitSystem.area, e.ChangedItem.Value)
                End If

                If ChildParent.Options.CalculatorActivated Then

                    'Call function to calculate flowsheet
                    Dim objargs As New DWSIM.Outros.StatusChangeEventArgs
                    With objargs
                        .Calculado = False
                        .Tag = sobj.Tag
                        .Nome = sobj.Name
                        .Tipo = sobj.TipoObjeto
                        .Emissor = "PropertyGrid"
                    End With

                    If ft.IsSpecAttached = True And ft.SpecVarType = DWSIM.SimulationObjects.SpecialOps.Helpers.Spec.TipoVar.Fonte Then ChildParent.Collections.CLCS_SpecCollection(ft.AttachedSpecId).Calculate()
                    ChildParent.CalculationQueue.Enqueue(objargs)

                End If

            ElseIf sobj.TipoObjeto = TipoObjeto.Compressor Then

                Dim bb As DWSIM.SimulationObjects.UnitOps.Compressor = ChildParent.Collections.CLCS_CompressorCollection.Item(sobj.Name)

                If e.ChangedItem.Label.Contains("Delta P") Then

                    If e.ChangedItem.Value < 0 Then Throw New InvalidCastException(DWSIM.App.GetLocalString("Ovalorinformadonovli"))
                    bb.DeltaP = Conversor.ConverterParaSI(ChildParent.Options.SelectedUnitSystem.spmp_deltaP, e.ChangedItem.Value)

                ElseIf e.ChangedItem.Label.Contains(DWSIM.App.GetLocalString("Presso")) Then
                    If e.ChangedItem.Value < 0 Then Throw New InvalidCastException(DWSIM.App.GetLocalString("Ovalorinformadonovli"))
                    bb.POut = Conversor.ConverterParaSI(ChildParent.Options.SelectedUnitSystem.spmp_deltaP, e.ChangedItem.Value)

                ElseIf e.ChangedItem.Label.Contains(DWSIM.App.GetLocalString("Eficincia")) Then
                    If e.ChangedItem.Value <= 20 Or e.ChangedItem.Value > 100 Then Throw New InvalidCastException(DWSIM.App.GetLocalString("Ovalorinformadonovli"))

                End If

                If ChildParent.Options.CalculatorActivated Then

                    'Call function to calculate flowsheet
                    Dim objargs As New DWSIM.Outros.StatusChangeEventArgs
                    With objargs
                        .Calculado = False
                        .Tag = sobj.Tag
                        .Nome = sobj.Name
                        .Tipo = TipoObjeto.Compressor
                        .Emissor = "PropertyGrid"
                    End With

                    If bb.IsSpecAttached = True And bb.SpecVarType = DWSIM.SimulationObjects.SpecialOps.Helpers.Spec.TipoVar.Fonte Then ChildParent.Collections.CLCS_SpecCollection(bb.AttachedSpecId).Calculate()
                    ChildParent.CalculationQueue.Enqueue(objargs)

                End If

            ElseIf sobj.TipoObjeto = TipoObjeto.Expander Then

                Dim bb As DWSIM.SimulationObjects.UnitOps.Expander = ChildParent.Collections.CLCS_TurbineCollection.Item(sobj.Name)

                If e.ChangedItem.Label.Contains(DWSIM.App.GetLocalString("Quedadepresso")) Then

                    If e.ChangedItem.Value < 0 Then Throw New InvalidCastException(DWSIM.App.GetLocalString("Ovalorinformadonovli"))
                    bb.DeltaP = Conversor.ConverterParaSI(ChildParent.Options.SelectedUnitSystem.spmp_deltaP, e.ChangedItem.Value)

                ElseIf e.ChangedItem.Label.Contains(DWSIM.App.GetLocalString("Eficincia")) Then

                    If e.ChangedItem.Value <= 20 Or e.ChangedItem.Value > 100 Then Throw New InvalidCastException(DWSIM.App.GetLocalString("Ovalorinformadonovli"))

                End If

                If ChildParent.Options.CalculatorActivated Then

                    'Call function to calculate flowsheet
                    Dim objargs As New DWSIM.Outros.StatusChangeEventArgs
                    With objargs
                        .Calculado = False
                        .Tag = sobj.Tag
                        .Nome = sobj.Name
                        .Tipo = TipoObjeto.Expander
                        .Emissor = "PropertyGrid"
                    End With

                    If bb.IsSpecAttached = True And bb.SpecVarType = DWSIM.SimulationObjects.SpecialOps.Helpers.Spec.TipoVar.Fonte Then ChildParent.Collections.CLCS_SpecCollection(bb.AttachedSpecId).Calculate()
                    ChildParent.CalculationQueue.Enqueue(objargs)

                End If

            ElseIf sobj.TipoObjeto = TipoObjeto.Pipe Then

                Dim bb As DWSIM.SimulationObjects.UnitOps.Pipe = ChildParent.Collections.CLCS_PipeCollection.Item(sobj.Name)

                If ChildParent.Options.CalculatorActivated Then

                    'Call function to calculate flowsheet
                    Dim objargs As New DWSIM.Outros.StatusChangeEventArgs
                    With objargs
                        .Calculado = False
                        .Tag = sobj.Tag
                        .Nome = sobj.Name
                        .Tipo = TipoObjeto.Pipe
                        .Emissor = "PropertyGrid"
                    End With

                    If bb.IsSpecAttached = True And bb.SpecVarType = DWSIM.SimulationObjects.SpecialOps.Helpers.Spec.TipoVar.Fonte Then ChildParent.Collections.CLCS_SpecCollection(bb.AttachedSpecId).Calculate()
                    ChildParent.CalculationQueue.Enqueue(objargs)

                End If

            ElseIf sobj.TipoObjeto = TipoObjeto.Heater Then

                Dim bb As DWSIM.SimulationObjects.UnitOps.Heater = ChildParent.Collections.CLCS_HeaterCollection.Item(sobj.Name)

                If e.ChangedItem.Label.Contains(DWSIM.App.GetLocalString("Eficincia")) Then

                    If e.ChangedItem.Value <= 20 Or e.ChangedItem.Value > 100 Then Throw New InvalidCastException(DWSIM.App.GetLocalString("Ovalorinformadonovli"))

                ElseIf e.ChangedItem.Label.Contains(DWSIM.App.GetLocalString("Calor")) Then

                    bb.DeltaQ = Conversor.ConverterParaSI(ChildParent.Options.SelectedUnitSystem.spmp_heatflow, e.ChangedItem.Value)

                ElseIf e.ChangedItem.Label.Contains(DWSIM.App.GetLocalString("Quedadepresso")) Then

                    If e.ChangedItem.Value < 0 Then Throw New InvalidCastException(DWSIM.App.GetLocalString("Ovalorinformadonovli"))
                    bb.DeltaP = Conversor.ConverterParaSI(ChildParent.Options.SelectedUnitSystem.spmp_deltaP, e.ChangedItem.Value)

                ElseIf e.ChangedItem.Label.Contains(DWSIM.App.GetLocalString("HeaterCoolerOutletTemperature")) Then

                    bb.OutletTemperature = Conversor.ConverterParaSI(ChildParent.Options.SelectedUnitSystem.spmp_temperature, e.ChangedItem.Value)

                ElseIf e.ChangedItem.Label.Contains(DWSIM.App.GetLocalString("FraomolardafaseFaseV")) Then

                    bb.OutletVaporFraction = Double.Parse(e.ChangedItem.Value)

                End If

                If ChildParent.Options.CalculatorActivated Then

                    'Call function to calculate flowsheet
                    Dim objargs As New DWSIM.Outros.StatusChangeEventArgs
                    With objargs
                        .Calculado = False
                        .Tag = sobj.Tag
                        .Nome = sobj.Name
                        .Tipo = TipoObjeto.Heater
                        .Emissor = "PropertyGrid"
                    End With

                    If bb.IsSpecAttached = True And bb.SpecVarType = DWSIM.SimulationObjects.SpecialOps.Helpers.Spec.TipoVar.Fonte Then ChildParent.Collections.CLCS_SpecCollection(bb.AttachedSpecId).Calculate()
                    ChildParent.CalculationQueue.Enqueue(objargs)

                End If

            ElseIf sobj.TipoObjeto = TipoObjeto.Cooler Then

                Dim bb As DWSIM.SimulationObjects.UnitOps.Cooler = ChildParent.Collections.CLCS_CoolerCollection.Item(sobj.Name)

                If e.ChangedItem.Label.Contains(DWSIM.App.GetLocalString("Eficincia")) Then

                    If e.ChangedItem.Value <= 20 Or e.ChangedItem.Value > 100 Then Throw New InvalidCastException(DWSIM.App.GetLocalString("Ovalorinformadonovli"))

                ElseIf e.ChangedItem.Label.Contains(DWSIM.App.GetLocalString("Calor")) Then

                    bb.DeltaQ = Conversor.ConverterParaSI(ChildParent.Options.SelectedUnitSystem.spmp_heatflow, e.ChangedItem.Value)

                ElseIf e.ChangedItem.Label.Contains(DWSIM.App.GetLocalString("Quedadepresso")) Then

                    If e.ChangedItem.Value < 0 Then Throw New InvalidCastException(DWSIM.App.GetLocalString("Ovalorinformadonovli"))
                    bb.DeltaP = Conversor.ConverterParaSI(ChildParent.Options.SelectedUnitSystem.spmp_deltaP, e.ChangedItem.Value)

                ElseIf e.ChangedItem.Label.Contains(DWSIM.App.GetLocalString("HeaterCoolerOutletTemperature")) Then

                    bb.OutletTemperature = Conversor.ConverterParaSI(ChildParent.Options.SelectedUnitSystem.spmp_temperature, e.ChangedItem.Value)

                ElseIf e.ChangedItem.Label.Contains(DWSIM.App.GetLocalString("FraomolardafaseFaseV")) Then

                    bb.OutletVaporFraction = Double.Parse(e.ChangedItem.Value)

                End If

                If ChildParent.Options.CalculatorActivated Then

                    'Call function to calculate flowsheet
                    Dim objargs As New DWSIM.Outros.StatusChangeEventArgs
                    With objargs
                        .Calculado = False
                        .Tag = sobj.Tag
                        .Nome = sobj.Name
                        .Tipo = TipoObjeto.Cooler
                        .Emissor = "PropertyGrid"
                    End With

                    If bb.IsSpecAttached = True And bb.SpecVarType = DWSIM.SimulationObjects.SpecialOps.Helpers.Spec.TipoVar.Fonte Then ChildParent.Collections.CLCS_SpecCollection(bb.AttachedSpecId).Calculate()
                    ChildParent.CalculationQueue.Enqueue(objargs)

                End If

            ElseIf sobj.TipoObjeto = TipoObjeto.Tank Then

                Dim bb As DWSIM.SimulationObjects.UnitOps.Tank = ChildParent.Collections.CLCS_TankCollection.Item(sobj.Name)

                If e.ChangedItem.Label.Contains(DWSIM.App.GetLocalString("Quedadepresso")) Then

                    If e.ChangedItem.Value < 0 Then Throw New InvalidCastException(DWSIM.App.GetLocalString("Ovalorinformadonovli"))
                    bb.DeltaP = Conversor.ConverterParaSI(ChildParent.Options.SelectedUnitSystem.spmp_deltaP, e.ChangedItem.Value)

                ElseIf e.ChangedItem.Label.Contains(DWSIM.App.GetLocalString("AquecimentoResfriame")) Then

                    bb.DeltaQ = Conversor.ConverterParaSI(ChildParent.Options.SelectedUnitSystem.spmp_heatflow, e.ChangedItem.Value)

                ElseIf e.ChangedItem.Label.Contains(DWSIM.App.GetLocalString("TKVol")) Then

                    If e.ChangedItem.Value < 0 Then Throw New InvalidCastException(DWSIM.App.GetLocalString("Ovalorinformadonovli"))
                    bb.Volume = Conversor.ConverterParaSI(ChildParent.Options.SelectedUnitSystem.volume, e.ChangedItem.Value)

                End If

                If ChildParent.Options.CalculatorActivated Then

                    'Call function to calculate flowsheet
                    Dim objargs As New DWSIM.Outros.StatusChangeEventArgs
                    With objargs
                        .Tag = sobj.Tag
                        .Calculado = False
                        .Nome = sobj.Name
                        .Tipo = TipoObjeto.Tank
                        .Emissor = "PropertyGrid"
                    End With

                    If bb.IsSpecAttached = True And bb.SpecVarType = DWSIM.SimulationObjects.SpecialOps.Helpers.Spec.TipoVar.Fonte Then ChildParent.Collections.CLCS_SpecCollection(bb.AttachedSpecId).Calculate()
                    ChildParent.CalculationQueue.Enqueue(objargs)

                End If

            ElseIf sobj.TipoObjeto = TipoObjeto.OT_Ajuste Then

                Dim adj As DWSIM.SimulationObjects.SpecialOps.Adjust = ChildParent.Collections.CLCS_AdjustCollection.Item(sobj.Name)

                With adj
                    If e.ChangedItem.Label.Contains(DWSIM.App.GetLocalString("Controlada")) Then
                        .ControlledObject = ChildParent.Collections.ObjectCollection(.ControlledObjectData.m_ID)
                        .ControlledVariable = .ControlledObjectData.m_Property
                        CType(ChildParent.Collections.AdjustCollection(adj.Nome), AdjustGraphic).ConnectedToCv = .ControlledObject.GraphicObject
                        .ReferenceObject = Nothing
                        .ReferenceVariable = Nothing
                        With .ReferencedObjectData
                            .m_ID = ""
                            .m_Name = ""
                            .m_Property = ""
                            .m_Type = ""
                        End With
                    ElseIf e.ChangedItem.Label.Contains(DWSIM.App.GetLocalString("Manipulada")) Then
                        .ManipulatedObject = ChildParent.Collections.ObjectCollection(.ManipulatedObjectData.m_ID)
                        Dim gr As AdjustGraphic = ChildParent.Collections.AdjustCollection(adj.Nome)
                        gr.ConnectedToMv = .ManipulatedObject.GraphicObject
                        .ManipulatedVariable = .ManipulatedObjectData.m_Property
                    ElseIf e.ChangedItem.Label.Contains(DWSIM.App.GetLocalString("ObjetoVariveldeRefer")) Then
                        .ReferenceObject = ChildParent.Collections.ObjectCollection(.ReferencedObjectData.m_ID)
                        .ReferenceVariable = .ReferencedObjectData.m_Property
                        Dim gr As AdjustGraphic = ChildParent.Collections.AdjustCollection(adj.Nome)
                        gr.ConnectedToRv = .ReferenceObject.GraphicObject
                    ElseIf e.ChangedItem.Label.Contains(DWSIM.App.GetLocalString("Valormnimoopcional")) Then
                        adj.MinVal = Conversor.ConverterParaSI(adj.ManipulatedObject.GetPropertyUnit(adj.ManipulatedObjectData.m_Property, ChildParent.Options.SelectedUnitSystem), e.ChangedItem.Value)
                    ElseIf e.ChangedItem.Label.Contains(DWSIM.App.GetLocalString("Valormximoopcional")) Then
                        adj.MaxVal = Conversor.ConverterParaSI(adj.ManipulatedObject.GetPropertyUnit(adj.ManipulatedObjectData.m_Property, ChildParent.Options.SelectedUnitSystem), e.ChangedItem.Value)
                    ElseIf e.ChangedItem.Label.Contains(DWSIM.App.GetLocalString("ValordeAjusteouOffse")) Then
                        adj.AdjustValue = Conversor.ConverterParaSI(adj.ControlledObject.GetPropertyUnit(adj.ControlledObjectData.m_Property, ChildParent.Options.SelectedUnitSystem), e.ChangedItem.Value)
                    End If
                End With

            ElseIf sobj.TipoObjeto = TipoObjeto.OT_Especificacao Then

                Dim spec As DWSIM.SimulationObjects.SpecialOps.Spec = ChildParent.Collections.CLCS_SpecCollection.Item(sobj.Name)

                With spec
                    If e.ChangedItem.Label.Contains(DWSIM.App.GetLocalString("Destino")) Then
                        .TargetObject = ChildParent.Collections.ObjectCollection(.TargetObjectData.m_ID)
                        .TargetVariable = .TargetObjectData.m_Property
                        CType(ChildParent.Collections.SpecCollection(spec.Nome), SpecGraphic).ConnectedToTv = .TargetObject.GraphicObject
                    ElseIf e.ChangedItem.Label.Contains(DWSIM.App.GetLocalString("Fonte")) Then
                        .SourceObject = ChildParent.Collections.ObjectCollection(.SourceObjectData.m_ID)
                        Dim gr As SpecGraphic = ChildParent.Collections.SpecCollection(spec.Nome)
                        gr.ConnectedToSv = .SourceObject.GraphicObject
                        .SourceVariable = .SourceObjectData.m_Property
                    End If
                End With

            ElseIf sobj.TipoObjeto = TipoObjeto.Vessel Then

                Dim vessel As DWSIM.SimulationObjects.UnitOps.Vessel = ChildParent.Collections.CLCS_VesselCollection.Item(sobj.Name)

                Dim T, P As Double
                If e.ChangedItem.Label.Contains(DWSIM.App.GetLocalString("Temperatura")) Then
                    T = Conversor.ConverterParaSI(ChildParent.Options.SelectedUnitSystem.spmp_temperature, e.ChangedItem.Value)
                    vessel.FlashTemperature = T
                ElseIf e.ChangedItem.Label.Contains(DWSIM.App.GetLocalString("Presso")) Then
                    P = Conversor.ConverterParaSI(ChildParent.Options.SelectedUnitSystem.spmp_pressure, e.ChangedItem.Value)
                    vessel.FlashPressure = P
                End If

                If ChildParent.Options.CalculatorActivated Then

                    'Call function to calculate flowsheet
                    Dim objargs As New DWSIM.Outros.StatusChangeEventArgs
                    With objargs
                        .Tag = sobj.Tag
                        .Calculado = False
                        .Nome = sobj.Name
                        .Tipo = TipoObjeto.Vessel
                        .Emissor = "PropertyGrid"
                    End With

                    If vessel.IsSpecAttached = True And vessel.SpecVarType = DWSIM.SimulationObjects.SpecialOps.Helpers.Spec.TipoVar.Fonte Then ChildParent.Collections.CLCS_SpecCollection(vessel.AttachedSpecId).Calculate()
                    ChildParent.CalculationQueue.Enqueue(objargs)

                End If

            ElseIf sobj.TipoObjeto = TipoObjeto.OT_Reciclo Then

                Dim rec As DWSIM.SimulationObjects.SpecialOps.Recycle = ChildParent.Collections.CLCS_RecycleCollection.Item(sobj.Name)

                Dim T, P, W As Double
                If e.ChangedItem.Label.Contains(DWSIM.App.GetLocalString("Temperatura")) Then
                    T = Conversor.ConverterParaSI(ChildParent.Options.SelectedUnitSystem.spmp_deltaT, e.ChangedItem.Value)
                    rec.ConvergenceParameters.Temperatura = T
                ElseIf e.ChangedItem.Label.Contains(DWSIM.App.GetLocalString("Presso")) Then
                    P = Conversor.ConverterParaSI(ChildParent.Options.SelectedUnitSystem.spmp_deltaP, e.ChangedItem.Value)
                    rec.ConvergenceParameters.Pressao = P
                ElseIf e.ChangedItem.Label.Contains(DWSIM.App.GetLocalString("mssica")) Then
                    W = Conversor.ConverterParaSI(ChildParent.Options.SelectedUnitSystem.spmp_massflow, e.ChangedItem.Value)
                    rec.ConvergenceParameters.VazaoMassica = W
                End If

            ElseIf sobj.TipoObjeto = TipoObjeto.RCT_Conversion Then

                Dim bb As DWSIM.SimulationObjects.Reactors.Reactor_Conversion = ChildParent.Collections.CLCS_ReactorConversionCollection.Item(sobj.Name)

                If e.ChangedItem.Label.Contains(DWSIM.App.GetLocalString("Quedadepresso")) Then

                    If e.ChangedItem.Value < 0 Then Throw New InvalidCastException(DWSIM.App.GetLocalString("Ovalorinformadonovli"))
                    bb.DeltaP = Conversor.ConverterParaSI(ChildParent.Options.SelectedUnitSystem.spmp_deltaP, e.ChangedItem.Value)

                ElseIf e.ChangedItem.Label.Contains(DWSIM.App.GetLocalString("HeaterCoolerOutletTemperature")) Then

                    bb.OutletTemperature = Conversor.ConverterParaSI(ChildParent.Options.SelectedUnitSystem.spmp_temperature, e.ChangedItem.Value)

                End If

                If ChildParent.Options.CalculatorActivated Then

                    'Call function to calculate flowsheet
                    Dim objargs As New DWSIM.Outros.StatusChangeEventArgs
                    With objargs
                        .Calculado = False
                        .Tag = sobj.Tag
                        .Nome = sobj.Name
                        .Tipo = TipoObjeto.RCT_Conversion
                        .Emissor = "PropertyGrid"
                    End With

                    If bb.IsSpecAttached = True And bb.SpecVarType = DWSIM.SimulationObjects.SpecialOps.Helpers.Spec.TipoVar.Fonte Then ChildParent.Collections.CLCS_SpecCollection(bb.AttachedSpecId).Calculate()
                    ChildParent.CalculationQueue.Enqueue(objargs)

                End If

            ElseIf sobj.TipoObjeto = TipoObjeto.RCT_Equilibrium Then

                Dim bb As DWSIM.SimulationObjects.Reactors.Reactor_Equilibrium = ChildParent.Collections.CLCS_ReactorEquilibriumCollection.Item(sobj.Name)

                If e.ChangedItem.Label.Contains(DWSIM.App.GetLocalString("Quedadepresso")) Then

                    If e.ChangedItem.Value < 0 Then Throw New InvalidCastException(DWSIM.App.GetLocalString("Ovalorinformadonovli"))
                    bb.DeltaP = Conversor.ConverterParaSI(ChildParent.Options.SelectedUnitSystem.spmp_deltaP, e.ChangedItem.Value)

                ElseIf e.ChangedItem.Label.Contains(DWSIM.App.GetLocalString("HeaterCoolerOutletTemperature")) Then

                    bb.OutletTemperature = Conversor.ConverterParaSI(ChildParent.Options.SelectedUnitSystem.spmp_temperature, e.ChangedItem.Value)

                End If

                If ChildParent.Options.CalculatorActivated Then

                    'Call function to calculate flowsheet
                    Dim objargs As New DWSIM.Outros.StatusChangeEventArgs
                    With objargs
                        .Calculado = False
                        .Tag = sobj.Tag
                        .Nome = sobj.Name
                        .Tipo = TipoObjeto.RCT_Equilibrium
                        .Emissor = "PropertyGrid"
                    End With

                    If bb.IsSpecAttached = True And bb.SpecVarType = DWSIM.SimulationObjects.SpecialOps.Helpers.Spec.TipoVar.Fonte Then ChildParent.Collections.CLCS_SpecCollection(bb.AttachedSpecId).Calculate()
                    ChildParent.CalculationQueue.Enqueue(objargs)

                End If

            ElseIf sobj.TipoObjeto = TipoObjeto.RCT_Gibbs Then

                Dim bb As DWSIM.SimulationObjects.Reactors.Reactor_Gibbs = ChildParent.Collections.CLCS_ReactorGibbsCollection.Item(sobj.Name)

                If e.ChangedItem.Label.Contains(DWSIM.App.GetLocalString("Quedadepresso")) Then

                    If e.ChangedItem.Value < 0 Then Throw New InvalidCastException(DWSIM.App.GetLocalString("Ovalorinformadonovli"))
                    bb.DeltaP = Conversor.ConverterParaSI(ChildParent.Options.SelectedUnitSystem.spmp_deltaP, e.ChangedItem.Value)

                ElseIf e.ChangedItem.Label.Contains(DWSIM.App.GetLocalString("HeaterCoolerOutletTemperature")) Then

                    bb.OutletTemperature = Conversor.ConverterParaSI(ChildParent.Options.SelectedUnitSystem.spmp_temperature, e.ChangedItem.Value)

                End If

                If ChildParent.Options.CalculatorActivated Then

                    'Call function to calculate flowsheet
                    Dim objargs As New DWSIM.Outros.StatusChangeEventArgs
                    With objargs
                        .Calculado = False
                        .Tag = sobj.Tag
                        .Nome = sobj.Name
                        .Tipo = TipoObjeto.RCT_Gibbs
                        .Emissor = "PropertyGrid"
                    End With

                    If bb.IsSpecAttached = True And bb.SpecVarType = DWSIM.SimulationObjects.SpecialOps.Helpers.Spec.TipoVar.Fonte Then ChildParent.Collections.CLCS_SpecCollection(bb.AttachedSpecId).Calculate()
                    ChildParent.CalculationQueue.Enqueue(objargs)

                End If

            ElseIf sobj.TipoObjeto = TipoObjeto.RCT_CSTR Then

                Dim bb As DWSIM.SimulationObjects.Reactors.Reactor_CSTR = ChildParent.Collections.CLCS_ReactorCSTRCollection.Item(sobj.Name)

                If e.ChangedItem.Label.Contains(DWSIM.App.GetLocalString("RSCTRIsothermalTemperature")) Then

                    bb.IsothermalTemperature = Conversor.ConverterParaSI(ChildParent.Options.SelectedUnitSystem.spmp_temperature, e.ChangedItem.Value)

                ElseIf e.ChangedItem.Label.Contains(DWSIM.App.GetLocalString("HeaterCoolerOutletTemperature")) Then

                    bb.OutletTemperature = Conversor.ConverterParaSI(ChildParent.Options.SelectedUnitSystem.spmp_temperature, e.ChangedItem.Value)

                ElseIf e.ChangedItem.Label.Contains(DWSIM.App.GetLocalString("Quedadepresso")) Then

                    If e.ChangedItem.Value < 0 Then Throw New InvalidCastException(DWSIM.App.GetLocalString("Ovalorinformadonovli"))
                    bb.DeltaP = Conversor.ConverterParaSI(ChildParent.Options.SelectedUnitSystem.spmp_deltaP, e.ChangedItem.Value)

                ElseIf e.ChangedItem.Label.Contains(DWSIM.App.GetLocalString("RCSTRPGridItem1")) Then

                    If e.ChangedItem.Value < 0 Then Throw New InvalidCastException(DWSIM.App.GetLocalString("Ovalorinformadonovli"))
                    bb.Volume = Conversor.ConverterParaSI(ChildParent.Options.SelectedUnitSystem.volume, e.ChangedItem.Value)

                End If

                If ChildParent.Options.CalculatorActivated Then

                    'Call function to calculate flowsheet
                    Dim objargs As New DWSIM.Outros.StatusChangeEventArgs
                    With objargs
                        .Calculado = False
                        .Tag = sobj.Tag
                        .Nome = sobj.Name
                        .Tipo = TipoObjeto.RCT_CSTR
                        .Emissor = "PropertyGrid"
                    End With

                    If bb.IsSpecAttached = True And bb.SpecVarType = DWSIM.SimulationObjects.SpecialOps.Helpers.Spec.TipoVar.Fonte Then ChildParent.Collections.CLCS_SpecCollection(bb.AttachedSpecId).Calculate()
                    ChildParent.CalculationQueue.Enqueue(objargs)

                End If

            ElseIf sobj.TipoObjeto = TipoObjeto.RCT_PFR Then

                Dim bb As DWSIM.SimulationObjects.Reactors.Reactor_PFR = ChildParent.Collections.CLCS_ReactorPFRCollection.Item(sobj.Name)

                If e.ChangedItem.Label.Contains(DWSIM.App.GetLocalString("Quedadepresso")) Then

                    If e.ChangedItem.Value < 0 Then Throw New InvalidCastException(DWSIM.App.GetLocalString("Ovalorinformadonovli"))
                    bb.DeltaP = Conversor.ConverterParaSI(ChildParent.Options.SelectedUnitSystem.spmp_deltaP, e.ChangedItem.Value)

                ElseIf e.ChangedItem.Label.Contains(DWSIM.App.GetLocalString("HeaterCoolerOutletTemperature")) Then

                    bb.OutletTemperature = Conversor.ConverterParaSI(ChildParent.Options.SelectedUnitSystem.spmp_temperature, e.ChangedItem.Value)

                ElseIf e.ChangedItem.Label.Contains(DWSIM.App.GetLocalString("RCSTRPGridItem1")) Then

                    If e.ChangedItem.Value < 0 Then Throw New InvalidCastException(DWSIM.App.GetLocalString("Ovalorinformadonovli"))
                    bb.Volume = Conversor.ConverterParaSI(ChildParent.Options.SelectedUnitSystem.volume, e.ChangedItem.Value)

                End If

                If ChildParent.Options.CalculatorActivated Then

                    'Call function to calculate flowsheet
                    Dim objargs As New DWSIM.Outros.StatusChangeEventArgs
                    With objargs
                        .Calculado = False
                        .Tag = sobj.Tag
                        .Nome = sobj.Name
                        .Tipo = TipoObjeto.RCT_PFR
                        .Emissor = "PropertyGrid"
                    End With

                    If bb.IsSpecAttached = True And bb.SpecVarType = DWSIM.SimulationObjects.SpecialOps.Helpers.Spec.TipoVar.Fonte Then ChildParent.Collections.CLCS_SpecCollection(bb.AttachedSpecId).Calculate()
                    ChildParent.CalculationQueue.Enqueue(objargs)

                End If

            ElseIf sobj.TipoObjeto = TipoObjeto.HeatExchanger Then

                Dim bb As DWSIM.SimulationObjects.UnitOps.HeatExchanger = ChildParent.Collections.CLCS_HeatExchangerCollection.Item(sobj.Name)

                If e.ChangedItem.Label.Contains(DWSIM.App.GetLocalString("OverallHeatTranferCoefficient")) Then

                    If e.ChangedItem.Value < 0 Then Throw New InvalidCastException(DWSIM.App.GetLocalString("Ovalorinformadonovli"))
                    bb.OverallCoefficient = Conversor.ConverterParaSI(ChildParent.Options.SelectedUnitSystem.heat_transf_coeff, e.ChangedItem.Value)

                ElseIf e.ChangedItem.Label.Contains(DWSIM.App.GetLocalString("Area")) Then

                    bb.Area = Conversor.ConverterParaSI(ChildParent.Options.SelectedUnitSystem.area, e.ChangedItem.Value)

                ElseIf e.ChangedItem.Label.Contains(DWSIM.App.GetLocalString("HeatLoad")) Then

                    bb.Q = Conversor.ConverterParaSI(ChildParent.Options.SelectedUnitSystem.spmp_heatflow, e.ChangedItem.Value)

                ElseIf e.ChangedItem.Label.Contains(DWSIM.App.GetLocalString("HXHotSidePressureDrop")) Then

                    bb.HotSidePressureDrop = Conversor.ConverterParaSI(ChildParent.Options.SelectedUnitSystem.spmp_deltaP, e.ChangedItem.Value)

                ElseIf e.ChangedItem.Label.Contains(DWSIM.App.GetLocalString("HXColdSidePressureDrop")) Then

                    bb.ColdSidePressureDrop = Conversor.ConverterParaSI(ChildParent.Options.SelectedUnitSystem.spmp_deltaP, e.ChangedItem.Value)

                ElseIf e.ChangedItem.Label.Contains(DWSIM.App.GetLocalString("HXTempHotOut")) Then

                    bb.HotSideOutletTemperature = Conversor.ConverterParaSI(ChildParent.Options.SelectedUnitSystem.spmp_temperature, e.ChangedItem.Value)

                ElseIf e.ChangedItem.Label.Contains(DWSIM.App.GetLocalString("HXTempColdOut")) Then

                    bb.ColdSideOutletTemperature = Conversor.ConverterParaSI(ChildParent.Options.SelectedUnitSystem.spmp_temperature, e.ChangedItem.Value)

                End If

                If ChildParent.Options.CalculatorActivated Then

                    'Call function to calculate flowsheet
                    Dim objargs As New DWSIM.Outros.StatusChangeEventArgs
                    With objargs
                        .Tag = sobj.Tag
                        .Calculado = False
                        .Nome = sobj.Name
                        .Tipo = TipoObjeto.HeatExchanger
                        .Emissor = "PropertyGrid"
                    End With

                    If bb.IsSpecAttached = True And bb.SpecVarType = DWSIM.SimulationObjects.SpecialOps.Helpers.Spec.TipoVar.Fonte Then ChildParent.Collections.CLCS_SpecCollection(bb.AttachedSpecId).Calculate()
                    ChildParent.CalculationQueue.Enqueue(objargs)

                End If

            ElseIf sobj.TipoObjeto = TipoObjeto.ShortcutColumn Then

                Dim sc As DWSIM.SimulationObjects.UnitOps.ShortcutColumn = ChildParent.Collections.CLCS_ShortcutColumnCollection.Item(sobj.Name)
                Dim Pr, Pc As Double

                If e.ChangedItem.Label.Contains(DWSIM.App.GetLocalString("SCCondenserType")) Then
                    sc.GraphicObject.Shape = sc.condtype
                ElseIf e.ChangedItem.Label.Contains(DWSIM.App.GetLocalString("SCCondenserPressure")) Then
                    Pc = Conversor.ConverterParaSI(ChildParent.Options.SelectedUnitSystem.spmp_pressure, e.ChangedItem.Value)
                    sc.m_condenserpressure = Pc
                ElseIf e.ChangedItem.Label.Contains(DWSIM.App.GetLocalString("SCReboilerPressure")) Then
                    Pr = Conversor.ConverterParaSI(ChildParent.Options.SelectedUnitSystem.spmp_pressure, e.ChangedItem.Value)
                    sc.m_boilerpressure = Pr
                ElseIf e.ChangedItem.Label.Equals(DWSIM.App.GetLocalString("SCLightKey")) Then
                    sc.m_lightkey = e.ChangedItem.Value
                ElseIf e.ChangedItem.Label.Equals(DWSIM.App.GetLocalString("SCHeavyKey")) Then
                    sc.m_heavykey = e.ChangedItem.Value
                End If

                If ChildParent.Options.CalculatorActivated Then

                    'Call function to calculate flowsheet
                    Dim objargs As New DWSIM.Outros.StatusChangeEventArgs
                    With objargs
                        .Tag = sobj.Tag
                        .Calculado = False
                        .Nome = sobj.Name
                        .Tipo = TipoObjeto.ShortcutColumn
                        .Emissor = "PropertyGrid"
                    End With

                    If sc.IsSpecAttached = True And sc.SpecVarType = DWSIM.SimulationObjects.SpecialOps.Helpers.Spec.TipoVar.Fonte Then ChildParent.Collections.CLCS_SpecCollection(sc.AttachedSpecId).Calculate()
                    ChildParent.CalculationQueue.Enqueue(objargs)

                End If

            ElseIf sobj.TipoObjeto = TipoObjeto.OrificePlate Then

                Dim op As DWSIM.SimulationObjects.UnitOps.OrificePlate = ChildParent.Collections.CLCS_OrificePlateCollection.Item(sobj.Name)

                If e.ChangedItem.Label.Contains(DWSIM.App.GetLocalString("OPOrificeDiameter")) Then
                    op.OrificeDiameter = Conversor.ConverterParaSI(ChildParent.Options.SelectedUnitSystem.diameter, e.ChangedItem.Value)
                ElseIf e.ChangedItem.Label.Contains(DWSIM.App.GetLocalString("OPBeta")) Then
                    op.Beta = e.ChangedItem.Value
                ElseIf e.ChangedItem.Label.Contains(DWSIM.App.GetLocalString("OPCorrectionFactor")) Then
                    op.CorrectionFactor = e.ChangedItem.Value
                End If

                If ChildParent.Options.CalculatorActivated Then

                    'Call function to calculate flowsheet
                    Dim objargs As New DWSIM.Outros.StatusChangeEventArgs
                    With objargs
                        .Calculado = False
                        .Tag = sobj.Tag
                        .Nome = sobj.Name
                        .Tipo = TipoObjeto.OrificePlate
                        .Emissor = "PropertyGrid"
                    End With

                    If op.IsSpecAttached = True And op.SpecVarType = DWSIM.SimulationObjects.SpecialOps.Helpers.Spec.TipoVar.Fonte Then ChildParent.Collections.CLCS_SpecCollection(op.AttachedSpecId).Calculate()
                    ChildParent.CalculationQueue.Enqueue(objargs)

                End If


            ElseIf sobj.TipoObjeto = TipoObjeto.ExcelUO Then

                Dim eo As DWSIM.SimulationObjects.UnitOps.ExcelUO = ChildParent.Collections.CLCS_ExcelUOCollection.Item(sobj.Name)
                Dim P1 As Integer
                Dim L As String
                P1 = InStr(1, e.ChangedItem.Label, "(") - 2
                If P1 > 0 Then
                    L = Strings.Left(e.ChangedItem.Label, P1)
                    If eo.InputParams.ContainsKey(L) Then
                        eo.InputParams(L).Value = e.ChangedItem.Value
                    End If
                End If

                If ChildParent.Options.CalculatorActivated Then

                    'Call function to calculate flowsheet
                    Dim objargs As New DWSIM.Outros.StatusChangeEventArgs
                    With objargs
                        .Calculado = False
                        .Tag = sobj.Tag
                        .Nome = sobj.Name
                        .Tipo = TipoObjeto.ExcelUO
                        .Emissor = "PropertyGrid"
                    End With

                    If eo.IsSpecAttached = True And eo.SpecVarType = DWSIM.SimulationObjects.SpecialOps.Helpers.Spec.TipoVar.Fonte Then ChildParent.Collections.CLCS_SpecCollection(eo.AttachedSpecId).Calculate()
                    ChildParent.CalculationQueue.Enqueue(objargs)

                End If

            ElseIf sobj.TipoObjeto = TipoObjeto.DistillationColumn Or sobj.TipoObjeto = TipoObjeto.AbsorptionColumn Or sobj.TipoObjeto = TipoObjeto.ReboiledAbsorber Or
                sobj.TipoObjeto = TipoObjeto.RefluxedAbsorber Or sobj.TipoObjeto = TipoObjeto.CapeOpenUO Then


                If ChildParent.Options.CalculatorActivated Then

                    sobj.Calculated = True
                    RaiseEvent ObjectStatusChanged(sobj)

                    'Call function to calculate flowsheet
                    Dim objargs As New DWSIM.Outros.StatusChangeEventArgs
                    With objargs
                        .Calculado = True
                        .Nome = sobj.Name
                        .Tag = sobj.Tag
                        .Tipo = sobj.TipoObjeto
                        .Emissor = "PropertyGrid"
                    End With

                    Dim obj = ChildParent.Collections.ObjectCollection.Item(sobj.Name)

                    If obj.IsSpecAttached = True And obj.SpecVarType = DWSIM.SimulationObjects.SpecialOps.Helpers.Spec.TipoVar.Fonte Then ChildParent.Collections.CLCS_SpecCollection(obj.AttachedSpecId).Calculate()
                    ChildParent.CalculationQueue.Enqueue(objargs)

                End If

            End If

        End If

        Call ChildParent.FormSurface.UpdateSelectedObject()
        Call ChildParent.FormSurface.FlowsheetDesignSurface.Invalidate()

        CalculateAll2(ChildParent, My.Settings.SolverMode, , True)

    End Sub

    Private Sub FormChild_ObjectStatusChanged(ByVal obj As Microsoft.MSDN.Samples.GraphicObjects.GraphicObject) Handles Me.ObjectStatusChanged

        If obj.Active = False Then
            LblStatusObj.Text = DWSIM.App.GetLocalString("Inativo")
            LblStatusObj.ForeColor = Color.DimGray
        ElseIf obj.Calculated = False Then
            LblStatusObj.Text = DWSIM.App.GetLocalString("NoCalculado")
            LblStatusObj.ForeColor = Color.Red
        Else
            LblStatusObj.Text = DWSIM.App.GetLocalString("Calculado")
            LblStatusObj.ForeColor = Color.DarkGreen
        End If

    End Sub


    Private Sub PGEx2_PropertyValueChanged(ByVal s As Object, ByVal e As System.Windows.Forms.PropertyValueChangedEventArgs) Handles PGEx2.PropertyValueChanged
        ChildParent = Me.ParentForm
        If e.ChangedItem.Label.Contains(DWSIM.App.GetLocalString("Nome")) Then
            Try
                If Not ChildParent.Collections.ObjectCollection(ChildParent.FormSurface.FlowsheetDesignSurface.SelectedObject.Name).Tabela Is Nothing Then
                    ChildParent.Collections.ObjectCollection(ChildParent.FormSurface.FlowsheetDesignSurface.SelectedObject.Name).Tabela.HeaderText = ChildParent.FormSurface.FlowsheetDesignSurface.SelectedObject.Tag
                End If
                ChildParent.FormObjList.TreeViewObj.Nodes.Find(ChildParent.FormSurface.FlowsheetDesignSurface.SelectedObject.Name, True)(0).Text = e.ChangedItem.Value
            Catch ex As Exception
                'ChildParent.WriteToLog(ex.ToString, Color.Red, FormClasses.TipoAviso.Erro)
            Finally
                'CType(FormFlowsheet.SearchSurfaceObjectsByTag(e.OldValue, ChildParent.FormSurface.FlowsheetDesignSurface), GraphicObject).Tag = e.ChangedItem.Value
                For Each g As GraphicObject In ChildParent.FormSurface.FlowsheetDesignSurface.drawingObjects
                    If g.TipoObjeto = TipoObjeto.GO_MasterTable Then
                        CType(g, DWSIM.GraphicObjects.MasterTableGraphic).Update(ChildParent)
                    End If
                Next
            End Try
            ChildParent.FormSurface.FlowsheetDesignSurface.Invalidate()
        End If
    End Sub

    Private Sub ToolStripButton1_Click(ByVal sender As Object, ByVal e As System.EventArgs) Handles ToolStripButton1.Click

        If sfdxml1.ShowDialog = Windows.Forms.DialogResult.OK Then
            Dim str As New StringBuilder
            str.AppendLine(Label1.Text & vbTab & LblNomeObj.Text)
            str.AppendLine(Label3.Text & vbTab & LblTipoObj.Text)
            str.AppendLine(Label4.Text & vbTab & LblStatusObj.Text)
            str.AppendLine(DWSIM.App.GetLocalString("Propriedades") & ":")
            For Each item As CustomProperty In PGEx1.Item
                If TypeOf item.Value Is CustomPropertyCollection Then
                    For Each item2 As CustomProperty In item.Value
                        str.AppendLine("[" & item.Category.ToString & "] " & vbTab & item.Name & vbTab & item2.Name & vbTab & item2.Value)
                    Next
                Else
                    str.AppendLine("[" & item.Category.ToString & "] " & vbTab & item.Name & vbTab & item.Value.ToString)
                End If
            Next
            IO.File.WriteAllText(sfdxml1.FileName, str.ToString)
        End If

    End Sub

    Private Sub FloatToolStripMenuItem_Click(sender As Object, e As EventArgs) Handles FloatToolStripMenuItem.Click, DocumentToolStripMenuItem.Click,
                                                                         DockLeftToolStripMenuItem.Click, DockLeftAutoHideToolStripMenuItem.Click,
                                                                         DockRightAutoHideToolStripMenuItem.Click, DockRightToolStripMenuItem.Click,
                                                                         DockTopAutoHideToolStripMenuItem.Click, DockTopToolStripMenuItem.Click,
                                                                         DockBottomAutoHideToolStripMenuItem.Click, DockBottomToolStripMenuItem.Click

        For Each ts As ToolStripMenuItem In dckMenu.Items
            ts.Checked = False
        Next

        sender.Checked = True

        Select Case sender.Name
            Case "FloatToolStripMenuItem"
                Me.DockState = WeifenLuo.WinFormsUI.Docking.DockState.Float
            Case "DocumentToolStripMenuItem"
                Me.DockState = WeifenLuo.WinFormsUI.Docking.DockState.Document
            Case "DockLeftToolStripMenuItem"
                Me.DockState = WeifenLuo.WinFormsUI.Docking.DockState.DockLeft
            Case "DockLeftAutoHideToolStripMenuItem"
                Me.DockState = WeifenLuo.WinFormsUI.Docking.DockState.DockLeftAutoHide
            Case "DockRightAutoHideToolStripMenuItem"
                Me.DockState = WeifenLuo.WinFormsUI.Docking.DockState.DockRightAutoHide
            Case "DockRightToolStripMenuItem"
                Me.DockState = WeifenLuo.WinFormsUI.Docking.DockState.DockRight
            Case "DockBottomAutoHideToolStripMenuItem"
                Me.DockState = WeifenLuo.WinFormsUI.Docking.DockState.DockBottomAutoHide
            Case "DockBottomToolStripMenuItem"
                Me.DockState = WeifenLuo.WinFormsUI.Docking.DockState.DockBottom
            Case "DockTopAutoHideToolStripMenuItem"
                Me.DockState = WeifenLuo.WinFormsUI.Docking.DockState.DockTopAutoHide
            Case "DockTopToolStripMenuItem"
                Me.DockState = WeifenLuo.WinFormsUI.Docking.DockState.DockTop
            Case "HiddenToolStripMenuItem"
                Me.DockState = WeifenLuo.WinFormsUI.Docking.DockState.Hidden
        End Select

    End Sub

End Class