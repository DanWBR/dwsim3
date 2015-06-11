'    Reactor Base Class
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
Imports System.Collections.Generic
Imports DWSIM.DWSIM.ClassesBasicasTermodinamica
Imports System.Linq

Namespace DWSIM.SimulationObjects.Reactors

    Public Enum OperationMode
        Isothermic = 0
        Adiabatic = 1
        OutletTemperature = 2
    End Enum

    <System.Serializable()> Public MustInherit Class Reactor

        Inherits SimulationObjects_UnitOpBaseClass

        Protected m_reactionSequence As SortedList(Of Integer, ArrayList)
        Protected m_reactions As List(Of String)
        Protected m_conversions As Dictionary(Of String, Double)
        Protected m_componentconversions As Dictionary(Of String, Double)
        Protected m_reactionSetID As String = "DefaultSet"
        Protected m_reactionSetName As String = ""
        Protected m_opmode As OperationMode = OperationMode.Adiabatic
        Protected m_dp As Nullable(Of Double)
        Protected m_dt As Nullable(Of Double)
        Protected m_DQ As Nullable(Of Double)

        Public Overrides Function LoadData(data As System.Collections.Generic.List(Of System.Xml.Linq.XElement)) As Boolean

            MyBase.LoadData(data)
            Dim ci As Globalization.CultureInfo = Globalization.CultureInfo.InvariantCulture

            For Each xel2 As XElement In (From xel As XElement In data Select xel Where xel.Name = "ReactionSequence").Elements
                m_reactionSequence.Add(xel2.@Index, XMLSerializer.XMLSerializer.StringToArray(xel2.Value, ci))
            Next

            For Each xel2 As XElement In (From xel As XElement In data Select xel Where xel.Name = "Reaction").Elements
                m_reactions.Add(xel2.@ID)
            Next

            For Each xel2 As XElement In (From xel As XElement In data Select xel Where xel.Name = "ReactionConversions").Elements
                m_conversions.Add(xel2.@ID, Double.Parse(xel2.Value, ci))
            Next

            For Each xel2 As XElement In (From xel As XElement In data Select xel Where xel.Name = "CompoundConversions").Elements
                m_componentconversions.Add(xel2.@ID, Double.Parse(xel2.Value, ci))
            Next

        End Function

        Public Overrides Function SaveData() As System.Collections.Generic.List(Of System.Xml.Linq.XElement)

            Dim elements As System.Collections.Generic.List(Of System.Xml.Linq.XElement) = MyBase.SaveData()
            Dim ci As Globalization.CultureInfo = Globalization.CultureInfo.InvariantCulture

            With elements
                .Add(New XElement("ReactionSequence"))
                For Each kvp As KeyValuePair(Of Integer, ArrayList) In m_reactionSequence
                    .Item(.Count - 1).Add(New XElement("Item", New XAttribute("Index", kvp.Key), XMLSerializer.XMLSerializer.ArrayToString(kvp.Value, ci)))
                Next
                .Add(New XElement("Reactions"))
                For Each s As String In m_reactions
                    .Item(.Count - 1).Add(New XElement("Reaction", New XAttribute("ID", s)))
                Next
                .Add(New XElement("ReactionConversions"))
                For Each kvp As KeyValuePair(Of String, Double) In m_conversions
                    .Item(.Count - 1).Add(New XElement("Reaction", New XAttribute("ID", kvp.Key), kvp.Value.ToString(ci)))
                Next
                .Add(New XElement("CompoundConversions"))
                For Each kvp As KeyValuePair(Of String, Double) In m_componentconversions
                    .Item(.Count - 1).Add(New XElement("Compound", New XAttribute("ID", kvp.Key), kvp.Value.ToString(ci)))
                Next
            End With

            Return elements

        End Function

        Function GetAmounts(rxn As Reaction, ims As SimulationObjects.Streams.MaterialStream) As Dictionary(Of String, Double)

            Dim conv As New SistemasDeUnidades.Conversor

            Dim Pin As Double = ims.Fases(0).SPMProperties.pressure.GetValueOrDefault
            Dim amounts As New Dictionary(Of String, Double)

            For Each sb As ReactionStoichBase In rxn.Components.Values

                If Not amounts.ContainsKey(sb.CompName) Then amounts.Add(sb.CompName, 0.0#)

                Select Case rxn.ReactionBasis
                    Case ReactionBasis.Activity
                        amounts(sb.CompName) = ims.Fases(3).Componentes(sb.CompName).FracaoMolar.GetValueOrDefault * ims.Fases(3).Componentes(sb.CompName).ActivityCoeff.GetValueOrDefault
                    Case ReactionBasis.Fugacity
                        Select Case rxn.ReactionPhase
                            Case PhaseName.Vapor
                                amounts(sb.CompName) = ims.Fases(2).Componentes(sb.CompName).FracaoMolar.GetValueOrDefault * ims.Fases(2).Componentes(sb.CompName).FugacityCoeff.GetValueOrDefault * Pin
                            Case PhaseName.Liquid
                                amounts(sb.CompName) = ims.Fases(3).Componentes(sb.CompName).FracaoMolar.GetValueOrDefault * ims.Fases(3).Componentes(sb.CompName).FugacityCoeff.GetValueOrDefault * Pin
                            Case PhaseName.Mixture
                                amounts(sb.CompName) = ims.Fases(0).Componentes(sb.CompName).FracaoMolar.GetValueOrDefault * ims.Fases(0).Componentes(sb.CompName).FugacityCoeff.GetValueOrDefault * Pin
                        End Select
                    Case ReactionBasis.MassConc
                        Select Case rxn.ReactionPhase
                            Case PhaseName.Vapor
                                amounts(sb.CompName) = ims.Fases(2).Componentes(sb.CompName).MassFlow.GetValueOrDefault / ims.Fases(2).SPMProperties.volumetric_flow.GetValueOrDefault
                            Case PhaseName.Liquid
                                amounts(sb.CompName) = ims.Fases(3).Componentes(sb.CompName).MassFlow.GetValueOrDefault / ims.Fases(3).SPMProperties.volumetric_flow.GetValueOrDefault
                            Case PhaseName.Mixture
                                amounts(sb.CompName) = ims.Fases(0).Componentes(sb.CompName).MassFlow.GetValueOrDefault / ims.Fases(0).SPMProperties.volumetric_flow.GetValueOrDefault
                        End Select
                    Case ReactionBasis.MassFrac
                        Select Case rxn.ReactionPhase
                            Case PhaseName.Vapor
                                amounts(sb.CompName) = ims.Fases(2).Componentes(sb.CompName).FracaoMassica.GetValueOrDefault
                            Case PhaseName.Liquid
                                amounts(sb.CompName) = ims.Fases(3).Componentes(sb.CompName).FracaoMassica.GetValueOrDefault
                            Case PhaseName.Mixture
                                amounts(sb.CompName) = ims.Fases(0).Componentes(sb.CompName).FracaoMassica.GetValueOrDefault
                        End Select
                    Case ReactionBasis.MolarConc
                        Select Case rxn.ReactionPhase
                            Case PhaseName.Vapor
                                amounts(sb.CompName) = ims.Fases(2).Componentes(sb.CompName).MolarFlow.GetValueOrDefault / ims.Fases(2).SPMProperties.volumetric_flow.GetValueOrDefault
                            Case PhaseName.Liquid
                                amounts(sb.CompName) = ims.Fases(3).Componentes(sb.CompName).MolarFlow.GetValueOrDefault / ims.Fases(3).SPMProperties.volumetric_flow.GetValueOrDefault
                            Case PhaseName.Mixture
                                amounts(sb.CompName) = ims.Fases(0).Componentes(sb.CompName).MolarFlow.GetValueOrDefault / ims.Fases(0).SPMProperties.volumetric_flow.GetValueOrDefault
                        End Select
                    Case ReactionBasis.MolarFrac
                        Select Case rxn.ReactionPhase
                            Case PhaseName.Vapor
                                amounts(sb.CompName) = ims.Fases(2).Componentes(sb.CompName).FracaoMolar.GetValueOrDefault
                            Case PhaseName.Liquid
                                amounts(sb.CompName) = ims.Fases(3).Componentes(sb.CompName).FracaoMolar.GetValueOrDefault
                            Case PhaseName.Mixture
                                amounts(sb.CompName) = ims.Fases(0).Componentes(sb.CompName).FracaoMolar.GetValueOrDefault
                        End Select
                    Case ReactionBasis.PartialPress
                        amounts(sb.CompName) = ims.Fases(2).Componentes(sb.CompName).FracaoMolar.GetValueOrDefault * Pin
                End Select

                amounts(sb.CompName) = conv.ConverterParaSI(rxn.ConcUnit, amounts(sb.CompName))

            Next

            Return amounts

        End Function

        Sub New()
            MyBase.CreateNew()
            Me.m_reactionSequence = New SortedList(Of Integer, ArrayList)
            Me.m_reactions = New List(Of String)
            Me.m_conversions = New Dictionary(Of String, Double)
            Me.m_componentconversions = New Dictionary(Of String, Double)
        End Sub

        Public Property OutletTemperature As Double = 298.15#

        <Xml.Serialization.XmlIgnore()> Public Property ReactionsSequence() As SortedList(Of Integer, ArrayList)
            Get
                Return m_reactionSequence
            End Get
            Set(ByVal value As SortedList(Of Integer, ArrayList))
                m_reactionSequence = value
            End Set
        End Property

        <Xml.Serialization.XmlIgnore()> Public ReadOnly Property Conversions() As Dictionary(Of String, Double)
            Get
                Return Me.m_conversions
            End Get
        End Property

        <Xml.Serialization.XmlIgnore()> Public ReadOnly Property ComponentConversions() As Dictionary(Of String, Double)
            Get
                Return Me.m_componentconversions
            End Get
        End Property

        <Xml.Serialization.XmlIgnore()> Public Property Reactions() As List(Of String)
            Get
                Return m_reactions
            End Get
            Set(ByVal value As List(Of String))
                m_reactions = value
            End Set
        End Property

        Public Property ReactorOperationMode() As OperationMode
            Get
                Return Me.m_opmode
            End Get
            Set(ByVal value As OperationMode)
                Me.m_opmode = value
            End Set
        End Property

        Public Property ReactionSetID() As String
            Get
                Return Me.m_reactionSetID
            End Get
            Set(ByVal value As String)
                Me.m_reactionSetID = value
            End Set
        End Property

        Public Property ReactionSetName() As String
            Get
                Return Me.m_reactionSetName
            End Get
            Set(ByVal value As String)
                Me.m_reactionSetName = value
            End Set
        End Property

        Public Property DeltaP() As Nullable(Of Double)
            Get
                Return m_dp
            End Get
            Set(ByVal value As Nullable(Of Double))
                m_dp = value
            End Set
        End Property

        Public Property DeltaT() As Nullable(Of Double)
            Get
                Return m_dt
            End Get
            Set(ByVal value As Nullable(Of Double))
                m_dt = value
            End Set
        End Property

        Public Property DeltaQ() As Nullable(Of Double)
            Get
                Return m_DQ
            End Get
            Set(ByVal value As Nullable(Of Double))
                m_DQ = value
            End Set
        End Property

#Region "    Table Functions"

        Public Overrides Sub QTFillNodeItems()

        End Sub

        Public Overrides Sub UpdatePropertyNodes(ByVal su As SistemasDeUnidades.Unidades, ByVal nf As String)

        End Sub

#End Region

    End Class

End Namespace
