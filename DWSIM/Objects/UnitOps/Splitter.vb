'    Splitter Calculation Routines 
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

Imports Microsoft.MSDN.Samples.GraphicObjects
Imports DWSIM.DWSIM.Flowsheet.FlowsheetSolver
Imports System.Linq

Namespace DWSIM.SimulationObjects.UnitOps

    <System.Serializable()> Public Class Splitter

        Inherits SimulationObjects_UnitOpBaseClass

        Public Enum OpMode
            SplitRatios = 0
            StreamMassFlowSpec = 1
            StreamMoleFlowSpec = 2
        End Enum

        Protected m_ratios As New System.Collections.ArrayList(3)

        Public Property StreamFlowSpec As Double = 0.0#
        Public Property Stream2FlowSpec As Double = 0.0#

        Public Property OperationMode As OpMode = OpMode.SplitRatios

        Public Overrides Function LoadData(data As System.Collections.Generic.List(Of System.Xml.Linq.XElement)) As Boolean

            Dim ci As Globalization.CultureInfo = Globalization.CultureInfo.InvariantCulture

            MyBase.LoadData(data)

            Me.m_ratios = New ArrayList

            For Each xel As XElement In (From xel2 As XElement In data Select xel2 Where xel2.Name = "SplitRatios").SingleOrDefault.Elements.ToList
                m_ratios.Add(Double.Parse(xel.Value, ci))
            Next

        End Function

        Public Overrides Function SaveData() As System.Collections.Generic.List(Of System.Xml.Linq.XElement)

            Dim elements As System.Collections.Generic.List(Of System.Xml.Linq.XElement) = MyBase.SaveData()
            Dim ci As Globalization.CultureInfo = Globalization.CultureInfo.InvariantCulture

            With elements
                .Add(New XElement("SplitRatios"))
                For Each d As Double In m_ratios
                    .Item(.Count - 1).Add(New XElement("SplitRatio", d.ToString(ci)))
                Next
            End With

            Return elements

        End Function

        Public ReadOnly Property Ratios() As System.Collections.ArrayList
            Get
                Return Me.m_ratios
            End Get
        End Property

        Public Sub New(ByVal nome As String, ByVal descricao As String)

            MyBase.CreateNew()
            Me.m_ComponentName = nome
            Me.m_ComponentDescription = descricao
            Me.m_ratios.Add(New Double)
            Me.m_ratios.Add(New Double)
            Me.m_ratios.Add(New Double)
            Me.FillNodeItems()
            Me.QTFillNodeItems()
        End Sub

        Public Sub New()
            MyBase.New()
        End Sub

        Public Overrides Function Calculate(Optional ByVal args As Object = Nothing) As Integer

            Dim form As Global.DWSIM.FormFlowsheet = Me.Flowsheet
            Dim objargs As New DWSIM.Outros.StatusChangeEventArgs

            If Not Me.GraphicObject.OutputConnectors(0).IsAttached Then
                'Call function to calculate flowsheet
                With objargs
                    .Calculado = False
                    .Nome = Me.Nome
                    .Tipo = TipoObjeto.NodeOut
                End With
                CalculateFlowsheet(FlowSheet, objargs, Nothing)
                Throw New Exception(DWSIM.App.GetLocalString("Nohcorrentedematriac8"))
            ElseIf Not Me.GraphicObject.InputConnectors(0).IsAttached Then
                'Call function to calculate flowsheet
                With objargs
                    .Calculado = False
                    .Nome = Me.Nome
                    .Tipo = TipoObjeto.NodeOut
                End With
                CalculateFlowsheet(FlowSheet, objargs, Nothing)
                Throw New Exception(DWSIM.App.GetLocalString("Verifiqueasconexesdo"))
            End If

            Dim ems As DWSIM.SimulationObjects.Streams.MaterialStream = form.Collections.CLCS_MaterialStreamCollection(Me.GraphicObject.InputConnectors(0).AttachedConnector.AttachedFrom.Name)
            ems.Validate()
            Dim W As Double = ems.Fases(0).SPMProperties.massflow.GetValueOrDefault
            Dim M As Double = ems.Fases(0).SPMProperties.molarflow.GetValueOrDefault

            Dim i As Integer = 0
            Dim j As Integer = 0

            Dim ms As DWSIM.SimulationObjects.Streams.MaterialStream

            Select Case Me.OperationMode

                Case OpMode.SplitRatios

                    Dim cp As ConnectionPoint
                    For Each cp In Me.GraphicObject.OutputConnectors
                        If cp.IsAttached Then
                            ms = form.Collections.CLCS_MaterialStreamCollection(cp.AttachedConnector.AttachedTo.Name)
                            With ms
                                .Fases(0).SPMProperties.temperature = ems.Fases(0).SPMProperties.temperature
                                .Fases(0).SPMProperties.pressure = ems.Fases(0).SPMProperties.pressure
                                .Fases(0).SPMProperties.enthalpy = ems.Fases(0).SPMProperties.enthalpy
                                Dim comp As DWSIM.ClassesBasicasTermodinamica.Substancia
                                j = 0
                                For Each comp In .Fases(0).Componentes.Values
                                    comp.FracaoMolar = ems.Fases(0).Componentes(comp.Nome).FracaoMolar
                                    comp.FracaoMassica = form.Collections.CLCS_MaterialStreamCollection(Me.GraphicObject.InputConnectors(0).AttachedConnector.AttachedFrom.Name).Fases(0).Componentes(comp.Nome).FracaoMassica
                                    j += 1
                                Next
                                .Fases(0).SPMProperties.massflow = W * Me.Ratios(i)
                                .Fases(0).SPMProperties.massfraction = 1
                                .Fases(0).SPMProperties.molarfraction = 1
                                .SpecType = Streams.MaterialStream.Flashspec.Pressure_and_Enthalpy
                            End With
                        End If
                        i += 1
                    Next

                Case OpMode.StreamMassFlowSpec

                    Dim cp As ConnectionPoint
                    Dim w1, w2 As Double, n As Integer

                    n = 0
                    For Each cp In Me.GraphicObject.OutputConnectors
                        If cp.IsAttached Then
                            n += 1
                        End If
                    Next

                    Dim wn(n) As Double

                    Select Case n
                        Case 1
                            w1 = Me.StreamFlowSpec
                            wn(0) = w1
                        Case 2
                            w1 = Me.StreamFlowSpec
                            wn(0) = w1
                            wn(1) = W - w1
                        Case 3
                            w1 = Me.StreamFlowSpec
                            w2 = Me.Stream2FlowSpec
                            wn(0) = w1
                            wn(1) = w2
                            wn(2) = W - w1 - w2
                    End Select

                    i = 0
                    For Each cp In Me.GraphicObject.OutputConnectors
                        If cp.IsAttached Then
                            ms = form.Collections.CLCS_MaterialStreamCollection(cp.AttachedConnector.AttachedTo.Name)
                            With ms
                                .Fases(0).SPMProperties.temperature = ems.Fases(0).SPMProperties.temperature
                                .Fases(0).SPMProperties.pressure = ems.Fases(0).SPMProperties.pressure
                                .Fases(0).SPMProperties.enthalpy = ems.Fases(0).SPMProperties.enthalpy
                                Dim comp As DWSIM.ClassesBasicasTermodinamica.Substancia
                                j = 0
                                For Each comp In .Fases(0).Componentes.Values
                                    comp.FracaoMolar = ems.Fases(0).Componentes(comp.Nome).FracaoMolar
                                    comp.FracaoMassica = form.Collections.CLCS_MaterialStreamCollection(Me.GraphicObject.InputConnectors(0).AttachedConnector.AttachedFrom.Name).Fases(0).Componentes(comp.Nome).FracaoMassica
                                    j += 1
                                Next
                                .Fases(0).SPMProperties.massflow = wn(i)
                                .Fases(0).SPMProperties.massfraction = 1
                                .Fases(0).SPMProperties.molarfraction = 1
                                .SpecType = Streams.MaterialStream.Flashspec.Pressure_and_Enthalpy
                            End With
                        End If
                        i += 1
                    Next

                Case OpMode.StreamMoleFlowSpec

                    Dim cp As ConnectionPoint
                    Dim m1, m2 As Double, n As Integer

                    n = 0
                    For Each cp In Me.GraphicObject.OutputConnectors
                        If cp.IsAttached Then
                            n += 1
                        End If
                    Next

                    Dim mn(n) As Double

                    Select Case n
                        Case 1
                            m1 = m1
                            mn(0) = m1
                        Case 2
                            m1 = Me.StreamFlowSpec
                            mn(0) = m1
                            mn(1) = M - m1
                        Case 3
                            m1 = Me.StreamFlowSpec
                            m2 = Me.Stream2FlowSpec
                            mn(0) = m1
                            mn(1) = m2
                            mn(2) = M - m1 - m2
                    End Select

                    i = 0
                    For Each cp In Me.GraphicObject.OutputConnectors
                        If cp.IsAttached Then
                            ms = form.Collections.CLCS_MaterialStreamCollection(cp.AttachedConnector.AttachedTo.Name)
                            With ms
                                .Fases(0).SPMProperties.temperature = ems.Fases(0).SPMProperties.temperature
                                .Fases(0).SPMProperties.pressure = ems.Fases(0).SPMProperties.pressure
                                .Fases(0).SPMProperties.enthalpy = ems.Fases(0).SPMProperties.enthalpy
                                Dim comp As DWSIM.ClassesBasicasTermodinamica.Substancia
                                j = 0
                                For Each comp In .Fases(0).Componentes.Values
                                    comp.FracaoMolar = ems.Fases(0).Componentes(comp.Nome).FracaoMolar
                                    comp.FracaoMassica = form.Collections.CLCS_MaterialStreamCollection(Me.GraphicObject.InputConnectors(0).AttachedConnector.AttachedFrom.Name).Fases(0).Componentes(comp.Nome).FracaoMassica
                                    j += 1
                                Next
                                .Fases(0).SPMProperties.massflow = Nothing
                                .Fases(0).SPMProperties.molarflow = mn(i)
                                .Fases(0).SPMProperties.massfraction = 1
                                .Fases(0).SPMProperties.molarfraction = 1
                                .SpecType = Streams.MaterialStream.Flashspec.Pressure_and_Enthalpy
                            End With
                        End If
                        i += 1
                    Next

            End Select


            'Call function to calculate flowsheet
            With objargs
                .Calculado = True
                .Nome = Me.Nome
                .Tag = Me.GraphicObject.Tag
                .Tipo = TipoObjeto.NodeOut
            End With

            form.CalculationQueue.Enqueue(objargs)

        End Function

        Public Overrides Function DeCalculate() As Integer

            Dim form As Global.DWSIM.FormFlowsheet = Me.FlowSheet

            Dim i As Integer = 0
            Dim j As Integer = 0

            Dim ms As DWSIM.SimulationObjects.Streams.MaterialStream
            Dim cp As ConnectionPoint
            For Each cp In Me.GraphicObject.OutputConnectors
                If cp.IsAttached Then
                    ms = form.Collections.CLCS_MaterialStreamCollection(cp.AttachedConnector.AttachedTo.Name)
                    j = 0
                    With ms
                        .Fases(0).SPMProperties.temperature = Nothing
                        .Fases(0).SPMProperties.pressure = Nothing
                        .Fases(0).SPMProperties.enthalpy = Nothing
                        Dim comp As DWSIM.ClassesBasicasTermodinamica.Substancia
                        For Each comp In .Fases(0).Componentes.Values
                            comp.FracaoMolar = 0
                            comp.FracaoMassica = 0
                            j += 1
                        Next
                        .Fases(0).SPMProperties.massflow = Nothing
                        .Fases(0).SPMProperties.massfraction = 1
                        .Fases(0).SPMProperties.molarfraction = 1
                    End With
                End If
                i += 1
            Next

            'Call function to calculate flowsheet
            Dim objargs As New DWSIM.Outros.StatusChangeEventArgs
            With objargs
                .Calculado = False
                .Nome = Me.Nome
                .Tag = Me.GraphicObject.Tag
                .Tipo = TipoObjeto.NodeOut
            End With

            form.CalculationQueue.Enqueue(objargs)

        End Function

        Public Overloads Overrides Sub UpdatePropertyNodes(ByVal su As SistemasDeUnidades.Unidades, ByVal nf As String)

            Dim Conversor As New DWSIM.SistemasDeUnidades.Conversor
            If Me.NodeTableItems Is Nothing Then
                Me.NodeTableItems = New System.Collections.Generic.Dictionary(Of Integer, DWSIM.Outros.NodeItem)
                Me.FillNodeItems()
            End If

            For Each nti As Outros.NodeItem In Me.NodeTableItems.Values
                nti.Value = GetPropertyValue(nti.Text, FlowSheet.Options.SelectedUnitSystem)
                nti.Unit = GetPropertyUnit(nti.Text, FlowSheet.Options.SelectedUnitSystem)
            Next

            If Me.QTNodeTableItems Is Nothing Then
                Me.QTNodeTableItems = New System.Collections.Generic.Dictionary(Of Integer, DWSIM.Outros.NodeItem)
                Me.QTFillNodeItems()
            End If

            With Me.QTNodeTableItems

                .Item(0).Value = Me.Ratios(0)
                .Item(0).Unit = ""
                .Item(1).Value = Me.Ratios(1)
                .Item(1).Unit = ""
                .Item(2).Value = Me.Ratios(2)
                .Item(2).Unit = ""

            End With

        End Sub

        Public Overrides Sub QTFillNodeItems()

            With Me.QTNodeTableItems

                .Clear()

                .Add(0, New DWSIM.Outros.NodeItem("S1", "", "", 0, 0, ""))
                .Add(1, New DWSIM.Outros.NodeItem("S2", "", "", 1, 0, ""))
                .Add(2, New DWSIM.Outros.NodeItem("S2", "", "", 2, 0, ""))

            End With

        End Sub

        Public Overrides Sub PopulatePropertyGrid(ByRef pgrid As PropertyGridEx.PropertyGridEx, ByVal su As SistemasDeUnidades.Unidades)
            Dim Conversor As New DWSIM.SistemasDeUnidades.Conversor

            With pgrid

                .PropertySort = PropertySort.Categorized
                .ShowCustomProperties = True
                .Item.Clear()

                MyBase.PopulatePropertyGrid(pgrid, su)

                Dim saida1, saida2, saida3, ent As String

                If Me.GraphicObject.OutputConnectors(0).IsAttached = True Then
                    saida1 = Me.GraphicObject.OutputConnectors(0).AttachedConnector.AttachedTo.Tag
                Else
                    saida1 = ""
                End If
                If Me.GraphicObject.OutputConnectors(1).IsAttached = True Then
                    saida2 = Me.GraphicObject.OutputConnectors(1).AttachedConnector.AttachedTo.Tag
                Else
                    saida2 = ""
                End If
                If Me.GraphicObject.OutputConnectors(2).IsAttached = True Then
                    saida3 = Me.GraphicObject.OutputConnectors(2).AttachedConnector.AttachedTo.Tag
                Else
                    saida3 = ""
                End If

                If Me.GraphicObject.InputConnectors(0).IsAttached = True Then
                    ent = Me.GraphicObject.InputConnectors(0).AttachedConnector.AttachedFrom.Tag
                Else
                    ent = ""
                End If

                .Item.Add(DWSIM.App.GetLocalString("Correntedeentrada"), ent, False, DWSIM.App.GetLocalString("Conexes1"), "", True)
                With .Item(.Item.Count - 1)
                    .DefaultValue = Nothing
                    .CustomEditor = New DWSIM.Editors.Streams.UIInputMSSelector
                End With

                .Item.Add(DWSIM.App.GetLocalString("Correntedesaida1"), saida1, False, DWSIM.App.GetLocalString("Conexes1"), "", True)
                With .Item(.Item.Count - 1)
                    .DefaultValue = Nothing
                    .CustomEditor = New DWSIM.Editors.Streams.UIOutputMSSelector
                End With
                .Item.Add(DWSIM.App.GetLocalString("Correntedesaida2"), saida2, False, DWSIM.App.GetLocalString("Conexes1"), "", True)
                With .Item(.Item.Count - 1)
                    .DefaultValue = Nothing
                    .CustomEditor = New DWSIM.Editors.Streams.UIOutputMSSelector
                End With
                .Item.Add(DWSIM.App.GetLocalString("Correntedesaida3"), saida3, False, DWSIM.App.GetLocalString("Conexes1"), "", True)
                With .Item(.Item.Count - 1)
                    .DefaultValue = Nothing
                    .CustomEditor = New DWSIM.Editors.Streams.UIOutputMSSelector
                End With

                .Item.Add(DWSIM.App.GetLocalString("SplitterOperationMode"), Me, "OperationMode", False, DWSIM.App.GetLocalString("Parmetros2"), "", True)

                Dim n As Integer = 0
                For Each cp In Me.GraphicObject.OutputConnectors
                    If cp.IsAttached Then
                        n += 1
                    End If
                Next

                Select Case Me.OperationMode
                    Case OpMode.SplitRatios
                        Dim i As Integer = 0
                        Dim cg2 As ConnectionPoint
                        For Each cg2 In Me.GraphicObject.OutputConnectors
                            If cg2.IsAttached = True Then
                                .Item.Add("[Split Ratio] " & cg2.AttachedConnector.AttachedTo.Tag, Me.Ratios.Item(i), False, DWSIM.App.GetLocalString("Parmetros2"), DWSIM.App.GetLocalString("Digiteumvalorentre0e"), True)
                                With .Item(.Item.Count - 1)
                                    .DefaultValue = GetType(System.Double)
                                End With
                            End If
                            i += 1
                        Next
                    Case OpMode.StreamMassFlowSpec
                        Dim valor = Format(Conversor.ConverterDoSI(su.spmp_massflow, Me.StreamFlowSpec), FlowSheet.Options.NumberFormat)
                        .Item.Add(FT(DWSIM.App.GetPropertyName("PROP_SP_1"), su.spmp_massflow), valor, False, DWSIM.App.GetLocalString("Parmetros2"), "", True)
                        With .Item(.Item.Count - 1)
                            .Tag = New Object() {FlowSheet.Options.NumberFormat, su.spmp_massflow, "W"}
                            .CustomEditor = New DWSIM.Editors.Generic.UIUnitConverter
                        End With
                        If n = 3 Then
                            valor = Format(Conversor.ConverterDoSI(su.spmp_massflow, Me.Stream2FlowSpec), FlowSheet.Options.NumberFormat)
                            .Item.Add(FT(DWSIM.App.GetPropertyName("PROP_SP_2"), su.spmp_massflow), valor, False, DWSIM.App.GetLocalString("Parmetros2"), "", True)
                            With .Item(.Item.Count - 1)
                                .Tag = New Object() {FlowSheet.Options.NumberFormat, su.spmp_massflow, "W"}
                                .CustomEditor = New DWSIM.Editors.Generic.UIUnitConverter
                            End With
                        End If
                    Case OpMode.StreamMoleFlowSpec
                        Dim valor = Format(Conversor.ConverterDoSI(su.spmp_molarflow, Me.StreamFlowSpec), FlowSheet.Options.NumberFormat)
                        .Item.Add(FT(DWSIM.App.GetPropertyName("PROP_SP_1"), su.spmp_molarflow), valor, False, DWSIM.App.GetLocalString("Parmetros2"), "", True)
                        With .Item(.Item.Count - 1)
                            .Tag = New Object() {FlowSheet.Options.NumberFormat, su.spmp_molarflow, "M"}
                            .CustomEditor = New DWSIM.Editors.Generic.UIUnitConverter
                        End With
                        If n = 3 Then
                            valor = Format(Conversor.ConverterDoSI(su.spmp_molarflow, Me.Stream2FlowSpec), FlowSheet.Options.NumberFormat)
                            .Item.Add(FT(DWSIM.App.GetPropertyName("PROP_SP_2"), su.spmp_molarflow), valor, False, DWSIM.App.GetLocalString("Parmetros2"), "", True)
                            With .Item(.Item.Count - 1)
                                .Tag = New Object() {FlowSheet.Options.NumberFormat, su.spmp_molarflow, "M"}
                                .CustomEditor = New DWSIM.Editors.Generic.UIUnitConverter
                            End With
                        End If
                End Select

            End With

        End Sub

        Public Overrides Function GetPropertyValue(ByVal prop As String, Optional ByVal su As SistemasDeUnidades.Unidades = Nothing) As Object
            If su Is Nothing Then su = New DWSIM.SistemasDeUnidades.UnidadesSI
            Dim cv As New DWSIM.SistemasDeUnidades.Conversor
            Dim value As Double = 0
            Select Case prop
                Case "PROP_SP_1"
                    If Me.OperationMode = OpMode.StreamMassFlowSpec Then
                        value = cv.ConverterDoSI(su.spmp_massflow, Me.StreamFlowSpec)
                    Else
                        value = cv.ConverterDoSI(su.spmp_molarflow, Me.StreamFlowSpec)
                    End If
                Case "PROP_SP_2"
                    If Me.OperationMode = OpMode.StreamMassFlowSpec Then
                        value = cv.ConverterDoSI(su.spmp_massflow, Me.Stream2FlowSpec)
                    Else
                        value = cv.ConverterDoSI(su.spmp_molarflow, Me.Stream2FlowSpec)
                    End If
                Case "SR1"
                    If Me.Ratios.Count > 0 Then value = Me.Ratios(0)
                Case "SR2"
                    If Me.Ratios.Count > 1 Then value = Me.Ratios(1)
                Case "SR3"
                    If Me.Ratios.Count > 2 Then value = Me.Ratios(2)
            End Select
            Return value
        End Function

        Public Overloads Overrides Function GetProperties(ByVal proptype As SimulationObjects_BaseClass.PropertyType) As String()
            Dim proplist As New ArrayList
            proplist.Add("PROP_SP_1")
            proplist.Add("PROP_SP_2")
            proplist.Add("SR1")
            proplist.Add("SR2")
            proplist.Add("SR3")
            Return proplist.ToArray(GetType(System.String))
        End Function

        Public Overrides Function SetPropertyValue(ByVal prop As String, ByVal propval As Object, Optional ByVal su As DWSIM.SistemasDeUnidades.Unidades = Nothing) As Object
            If su Is Nothing Then su = New DWSIM.SistemasDeUnidades.UnidadesSI
            Dim cv As New DWSIM.SistemasDeUnidades.Conversor
            Select Case prop
                Case "PROP_SP_1"
                    If Me.OperationMode = OpMode.StreamMassFlowSpec Then
                        Me.StreamFlowSpec = cv.ConverterParaSI(su.spmp_massflow, propval)
                    Else
                        Me.StreamFlowSpec = cv.ConverterParaSI(su.spmp_molarflow, propval)
                    End If
                Case "PROP_SP_2"
                    If Me.OperationMode = OpMode.StreamMassFlowSpec Then
                        Me.Stream2FlowSpec = cv.ConverterParaSI(su.spmp_massflow, propval)
                    Else
                        Me.Stream2FlowSpec = cv.ConverterParaSI(su.spmp_molarflow, propval)
                    End If
                Case "SR1"
                    If Me.Ratios.Count > 0 Then Me.Ratios(0) = propval
                Case "SR2"
                    If Me.Ratios.Count > 1 Then Me.Ratios(1) = propval
                Case "SR3"
                    If Me.Ratios.Count > 2 Then Me.Ratios(2) = propval
            End Select
            Return 1
        End Function

        Public Overrides Function GetPropertyUnit(ByVal prop As String, Optional ByVal su As SistemasDeUnidades.Unidades = Nothing) As Object
            If su Is Nothing Then su = New DWSIM.SistemasDeUnidades.UnidadesSI
            Dim value As String = ""
            If prop.StartsWith("P") Then
                Select Case Me.OperationMode
                    Case OpMode.StreamMassFlowSpec
                        value = su.spmp_massflow
                    Case OpMode.StreamMoleFlowSpec
                        value = su.spmp_molarflow
                End Select
            Else
                value = ""
            End If
            Return value
        End Function
    End Class

End Namespace
