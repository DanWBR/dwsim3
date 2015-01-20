'    Flowsheet Unit Operation
'    Copyright 2015 Daniel Wagner O. de Medeiros
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
Imports DWSIM.DWSIM.Flowsheet.FlowsheetSolver
Imports DWSIM.DWSIM.SimulationObjects.UnitOps.Auxiliary
Imports System.Globalization
Imports System.Linq
Imports System.Xml.Linq
Imports DWSIM.DWSIM.SimulationObjects.PropertyPackages
Imports Microsoft.Msdn.Samples
Imports DWSIM.DWSIM.ClassesBasicasTermodinamica
Imports DWSIM.DWSIM.Outros

Namespace DWSIM.SimulationObjects.UnitOps.Auxiliary

    <System.Serializable()> Public Class FlowsheetUOParameter
        Implements XMLSerializer.Interfaces.ICustomXMLSerialization
        Public Property ID As String = ""
        Public Property ObjectID As String = ""
        Public Property ObjectProperty As String = ""
        'Public Property Value As Object = Nothing
        'Public Property Unit As String = ""
        Public Function LoadData(data As List(Of XElement)) As Boolean Implements XMLSerializer.Interfaces.ICustomXMLSerialization.LoadData
            XMLSerializer.XMLSerializer.Deserialize(Me, data)
        End Function
        Public Function SaveData() As List(Of XElement) Implements XMLSerializer.Interfaces.ICustomXMLSerialization.SaveData
            Return XMLSerializer.XMLSerializer.Serialize(Me)
        End Function
    End Class

    Public Enum FlowsheetUOMassTransferMode
        CompoundMassFlows = 0
        CompoundMoleFlows = 1
        CompoundMassFractions = 2
        CompoundMoleFractions = 3
    End Enum

End Namespace

Namespace DWSIM.SimulationObjects.UnitOps
    <System.Serializable()> Public Class Flowsheet

        Inherits SimulationObjects_UnitOpBaseClass

        Public Property SimulationFile As String = ""
        <System.Xml.Serialization.XmlIgnore> Public Property Initialized As Boolean = False
        Public Property InitializeOnLoad As Boolean = False
        Public Property MassTransferMode As FlowsheetUOMassTransferMode = FlowsheetUOMassTransferMode.CompoundMassFlows
        Public Property InputParams As Dictionary(Of String, FlowsheetUOParameter)
        Public Property OutputParams As Dictionary(Of String, FlowsheetUOParameter)
        <System.Xml.Serialization.XmlIgnore> <System.NonSerialized> Public Fsheet As FormFlowsheet = Nothing
        Public Property InputConnections As List(Of String)
        Public Property OutputConnections As List(Of String)

        Public Sub New(ByVal nome As String, ByVal descricao As String)

            MyBase.CreateNew()
            Me.m_ComponentName = nome
            Me.m_ComponentDescription = descricao
            Me.FillNodeItems()
            Me.QTFillNodeItems()

            InputParams = New Dictionary(Of String, FlowsheetUOParameter)
            OutputParams = New Dictionary(Of String, FlowsheetUOParameter)

            InputConnections = New List(Of String) From {"", "", "", "", "", "", "", "", "", ""}
            OutputConnections = New List(Of String) From {"", "", "", "", "", "", "", "", "", ""}

        End Sub

        Public Sub New()

            MyBase.New()

            InputParams = New Dictionary(Of String, FlowsheetUOParameter)
            OutputParams = New Dictionary(Of String, FlowsheetUOParameter)

            InputConnections = New List(Of String) From {"", "", "", "", "", "", "", "", "", ""}
            OutputConnections = New List(Of String) From {"", "", "", "", "", "", "", "", "", ""}

        End Sub

        Public Sub InitializeFlowsheet(path As String)

            If Not fsheet Is Nothing Then fsheet.Dispose()

            Dim ci As CultureInfo = CultureInfo.InvariantCulture

            Dim excs As New List(Of Exception)

            Dim xdoc As XDocument = XDocument.Load(path)

            Dim form As FormFlowsheet = New FormFlowsheet()

            Dim data As List(Of XElement) = xdoc.Element("DWSIM_Simulation_Data").Element("Settings").Elements.ToList

            Try
                form.Options.LoadData(data)
            Catch ex As Exception
                excs.Add(New Exception("Error Loading Flowsheet Settings", ex))
            End Try

            data = xdoc.Element("DWSIM_Simulation_Data").Element("GraphicObjects").Elements.ToList

            For Each xel As XElement In data
                Try
                    Dim obj As GraphicObject = Nothing
                    Dim t As Type = Type.GetType(xel.Element("Type").Value, False)
                    If Not t Is Nothing Then obj = Activator.CreateInstance(t)
                    If obj Is Nothing Then
                        obj = GraphicObject.ReturnInstance(xel.Element("Type").Value)
                    End If
                    obj.LoadData(xel.Elements.ToList)
                    If Not TypeOf obj Is DWSIM.GraphicObjects.TableGraphic Then
                        form.FormSurface.FlowsheetDesignSurface.drawingObjects.Add(obj)
                        obj.CreateConnectors(0, 0)
                        With form.Collections
                            Select Case obj.TipoObjeto
                                Case Microsoft.Msdn.Samples.GraphicObjects.TipoObjeto.Compressor
                                    .CompressorCollection.Add(obj.Name, obj)
                                Case Microsoft.Msdn.Samples.GraphicObjects.TipoObjeto.Cooler
                                    .CoolerCollection.Add(obj.Name, obj)
                                Case Microsoft.Msdn.Samples.GraphicObjects.TipoObjeto.EnergyStream
                                    .EnergyStreamCollection.Add(obj.Name, obj)
                                Case Microsoft.Msdn.Samples.GraphicObjects.TipoObjeto.Heater
                                    .HeaterCollection.Add(obj.Name, obj)
                                Case Microsoft.Msdn.Samples.GraphicObjects.TipoObjeto.MaterialStream
                                    .MaterialStreamCollection.Add(obj.Name, obj)
                                Case Microsoft.Msdn.Samples.GraphicObjects.TipoObjeto.NodeEn
                                    .MixerENCollection.Add(obj.Name, obj)
                                Case Microsoft.Msdn.Samples.GraphicObjects.TipoObjeto.NodeIn
                                    .MixerCollection.Add(obj.Name, obj)
                                Case Microsoft.Msdn.Samples.GraphicObjects.TipoObjeto.NodeOut
                                    .SplitterCollection.Add(obj.Name, obj)
                                Case Microsoft.Msdn.Samples.GraphicObjects.TipoObjeto.Pipe
                                    .PipeCollection.Add(obj.Name, obj)
                                Case Microsoft.Msdn.Samples.GraphicObjects.TipoObjeto.Pump
                                    .PumpCollection.Add(obj.Name, obj)
                                Case Microsoft.Msdn.Samples.GraphicObjects.TipoObjeto.Tank
                                    .TankCollection.Add(obj.Name, obj)
                                Case Microsoft.Msdn.Samples.GraphicObjects.TipoObjeto.Expander
                                    .TurbineCollection.Add(obj.Name, obj)
                                Case Microsoft.Msdn.Samples.GraphicObjects.TipoObjeto.Valve
                                    .ValveCollection.Add(obj.Name, obj)
                                Case Microsoft.Msdn.Samples.GraphicObjects.TipoObjeto.Vessel
                                    .SeparatorCollection.Add(obj.Name, obj)
                                Case Microsoft.Msdn.Samples.GraphicObjects.TipoObjeto.Expander
                                    .TurbineCollection.Add(obj.Name, obj)
                                Case Microsoft.Msdn.Samples.GraphicObjects.TipoObjeto.OT_Ajuste
                                    .AdjustCollection.Add(obj.Name, obj)
                                Case Microsoft.Msdn.Samples.GraphicObjects.TipoObjeto.OT_Reciclo
                                    .RecycleCollection.Add(obj.Name, obj)
                                Case Microsoft.Msdn.Samples.GraphicObjects.TipoObjeto.OT_Especificacao
                                    .SpecCollection.Add(obj.Name, obj)
                                Case Microsoft.Msdn.Samples.GraphicObjects.TipoObjeto.RCT_Conversion
                                    .ReactorConversionCollection.Add(obj.Name, obj)
                                Case Microsoft.Msdn.Samples.GraphicObjects.TipoObjeto.RCT_Equilibrium
                                    .ReactorEquilibriumCollection.Add(obj.Name, obj)
                                Case Microsoft.Msdn.Samples.GraphicObjects.TipoObjeto.RCT_Gibbs
                                    .ReactorGibbsCollection.Add(obj.Name, obj)
                                Case Microsoft.Msdn.Samples.GraphicObjects.TipoObjeto.RCT_CSTR
                                    .ReactorCSTRCollection.Add(obj.Name, obj)
                                Case Microsoft.Msdn.Samples.GraphicObjects.TipoObjeto.RCT_PFR
                                    .ReactorPFRCollection.Add(obj.Name, obj)
                                Case Microsoft.Msdn.Samples.GraphicObjects.TipoObjeto.HeatExchanger
                                    .HeatExchangerCollection.Add(obj.Name, obj)
                                Case Microsoft.Msdn.Samples.GraphicObjects.TipoObjeto.ShortcutColumn
                                    .ShortcutColumnCollection.Add(obj.Name, obj)
                                Case Microsoft.Msdn.Samples.GraphicObjects.TipoObjeto.DistillationColumn
                                    obj.CreateConnectors(xel.Element("InputConnectors").Elements.Count, xel.Element("OutputConnectors").Elements.Count)
                                    .DistillationColumnCollection.Add(obj.Name, obj)
                                Case Microsoft.Msdn.Samples.GraphicObjects.TipoObjeto.AbsorptionColumn
                                    obj.CreateConnectors(xel.Element("InputConnectors").Elements.Count, xel.Element("OutputConnectors").Elements.Count)
                                    .AbsorptionColumnCollection.Add(obj.Name, obj)
                                Case Microsoft.Msdn.Samples.GraphicObjects.TipoObjeto.RefluxedAbsorber
                                    obj.CreateConnectors(xel.Element("InputConnectors").Elements.Count, xel.Element("OutputConnectors").Elements.Count)
                                    .RefluxedAbsorberCollection.Add(obj.Name, obj)
                                Case Microsoft.Msdn.Samples.GraphicObjects.TipoObjeto.ReboiledAbsorber
                                    obj.CreateConnectors(xel.Element("InputConnectors").Elements.Count, xel.Element("OutputConnectors").Elements.Count)
                                    .ReboiledAbsorberCollection.Add(obj.Name, obj)
                                Case Microsoft.Msdn.Samples.GraphicObjects.TipoObjeto.OT_EnergyRecycle
                                    .EnergyRecycleCollection.Add(obj.Name, obj)
                                Case Microsoft.Msdn.Samples.GraphicObjects.TipoObjeto.ComponentSeparator
                                    .ComponentSeparatorCollection.Add(obj.Name, obj)
                                Case Microsoft.Msdn.Samples.GraphicObjects.TipoObjeto.OrificePlate
                                    .OrificePlateCollection.Add(obj.Name, obj)
                                Case Microsoft.Msdn.Samples.GraphicObjects.TipoObjeto.CustomUO
                                    .CustomUOCollection.Add(obj.Name, obj)
                                Case Microsoft.Msdn.Samples.GraphicObjects.TipoObjeto.ExcelUO
                                    .ExcelUOCollection.Add(obj.Name, obj)
                                Case Microsoft.Msdn.Samples.GraphicObjects.TipoObjeto.FlowsheetUO
                                    .FlowsheetUOCollection.Add(obj.Name, obj)
                                Case Microsoft.Msdn.Samples.GraphicObjects.TipoObjeto.CapeOpenUO
                                    obj.CreateConnectors(xel.Element("InputConnectors").Elements.Count, xel.Element("OutputConnectors").Elements.Count)
                                    .CapeOpenUOCollection.Add(obj.Name, obj)
                                Case Microsoft.Msdn.Samples.GraphicObjects.TipoObjeto.SolidSeparator
                                    .SolidsSeparatorCollection.Add(obj.Name, obj)
                                Case Microsoft.Msdn.Samples.GraphicObjects.TipoObjeto.Filter
                                    .FilterCollection.Add(obj.Name, obj)
                            End Select
                            If Not DWSIM.App.IsRunningOnMono Then
                                Select Case obj.TipoObjeto
                                    Case TipoObjeto.NodeIn
                                        form.FormObjList.TreeViewObj.Nodes("NodeMX").Nodes.Add(obj.Name, obj.Tag).Name = obj.Name
                                        form.FormObjList.TreeViewObj.Nodes("NodeMX").Nodes(obj.Name).ContextMenuStrip = form.FormObjList.ContextMenuStrip1
                                    Case TipoObjeto.NodeEn
                                        form.FormObjList.TreeViewObj.Nodes("NodeME").Nodes.Add(obj.Name, obj.Tag).Name = obj.Name
                                        form.FormObjList.TreeViewObj.Nodes("NodeME").Nodes(obj.Name).ContextMenuStrip = form.FormObjList.ContextMenuStrip1
                                    Case TipoObjeto.NodeOut
                                        form.FormObjList.TreeViewObj.Nodes("NodeSP").Nodes.Add(obj.Name, obj.Tag).Name = obj.Name
                                        form.FormObjList.TreeViewObj.Nodes("NodeSP").Nodes(obj.Name).ContextMenuStrip = form.FormObjList.ContextMenuStrip1
                                    Case TipoObjeto.Pump
                                        form.FormObjList.TreeViewObj.Nodes("NodePU").Nodes.Add(obj.Name, obj.Tag).Name = obj.Name
                                        form.FormObjList.TreeViewObj.Nodes("NodePU").Nodes(obj.Name).ContextMenuStrip = form.FormObjList.ContextMenuStrip1
                                    Case TipoObjeto.Tank
                                        form.FormObjList.TreeViewObj.Nodes("NodeTQ").Nodes.Add(obj.Name, obj.Tag).Name = obj.Name
                                        form.FormObjList.TreeViewObj.Nodes("NodeTQ").Nodes(obj.Name).ContextMenuStrip = form.FormObjList.ContextMenuStrip1
                                    Case TipoObjeto.Vessel
                                        form.FormObjList.TreeViewObj.Nodes("NodeSE").Nodes.Add(obj.Name, obj.Tag).Name = obj.Name
                                        form.FormObjList.TreeViewObj.Nodes("NodeSE").Nodes(obj.Name).ContextMenuStrip = form.FormObjList.ContextMenuStrip1
                                    Case TipoObjeto.TPVessel
                                        form.FormObjList.TreeViewObj.Nodes("NodeTP").Nodes.Add(obj.Name, obj.Tag).Name = obj.Name
                                        form.FormObjList.TreeViewObj.Nodes("NodeTP").Nodes(obj.Name).ContextMenuStrip = form.FormObjList.ContextMenuStrip1
                                    Case TipoObjeto.MaterialStream
                                        form.FormObjList.TreeViewObj.Nodes("NodeMS").Nodes.Add(obj.Name, obj.Tag).Name = obj.Name
                                        form.FormObjList.TreeViewObj.Nodes("NodeMS").Nodes(obj.Name).ContextMenuStrip = form.FormObjList.ContextMenuStrip1
                                    Case TipoObjeto.EnergyStream
                                        form.FormObjList.TreeViewObj.Nodes("NodeEN").Nodes.Add(obj.Name, obj.Tag).Name = obj.Name
                                        form.FormObjList.TreeViewObj.Nodes("NodeEN").Nodes(obj.Name).ContextMenuStrip = form.FormObjList.ContextMenuStrip1
                                    Case TipoObjeto.Compressor
                                        form.FormObjList.TreeViewObj.Nodes("NodeCO").Nodes.Add(obj.Name, obj.Tag).Name = obj.Name
                                        form.FormObjList.TreeViewObj.Nodes("NodeCO").Nodes(obj.Name).ContextMenuStrip = form.FormObjList.ContextMenuStrip1
                                    Case TipoObjeto.Expander
                                        form.FormObjList.TreeViewObj.Nodes("NodeTU").Nodes.Add(obj.Name, obj.Tag).Name = obj.Name
                                        form.FormObjList.TreeViewObj.Nodes("NodeTU").Nodes(obj.Name).ContextMenuStrip = form.FormObjList.ContextMenuStrip1
                                    Case TipoObjeto.Cooler
                                        form.FormObjList.TreeViewObj.Nodes("NodeCL").Nodes.Add(obj.Name, obj.Tag).Name = obj.Name
                                        form.FormObjList.TreeViewObj.Nodes("NodeCL").Nodes(obj.Name).ContextMenuStrip = form.FormObjList.ContextMenuStrip1
                                    Case TipoObjeto.Heater
                                        form.FormObjList.TreeViewObj.Nodes("NodeHT").Nodes.Add(obj.Name, obj.Tag).Name = obj.Name
                                        form.FormObjList.TreeViewObj.Nodes("NodeHT").Nodes(obj.Name).ContextMenuStrip = form.FormObjList.ContextMenuStrip1
                                    Case TipoObjeto.Pipe
                                        form.FormObjList.TreeViewObj.Nodes("NodePI").Nodes.Add(obj.Name, obj.Tag).Name = obj.Name
                                        form.FormObjList.TreeViewObj.Nodes("NodePI").Nodes(obj.Name).ContextMenuStrip = form.FormObjList.ContextMenuStrip1
                                    Case TipoObjeto.Valve
                                        form.FormObjList.TreeViewObj.Nodes("NodeVA").Nodes.Add(obj.Name, obj.Tag).Name = obj.Name
                                        form.FormObjList.TreeViewObj.Nodes("NodeVA").Nodes(obj.Name).ContextMenuStrip = form.FormObjList.ContextMenuStrip1
                                    Case TipoObjeto.RCT_Conversion
                                        form.FormObjList.TreeViewObj.Nodes("NodeRCONV").Nodes.Add(obj.Name, obj.Tag).Name = obj.Name
                                        form.FormObjList.TreeViewObj.Nodes("NodeRCONV").Nodes(obj.Name).ContextMenuStrip = form.FormObjList.ContextMenuStrip1
                                    Case TipoObjeto.RCT_Equilibrium
                                        form.FormObjList.TreeViewObj.Nodes("NodeREQ").Nodes.Add(obj.Name, obj.Tag).Name = obj.Name
                                        form.FormObjList.TreeViewObj.Nodes("NodeREQ").Nodes(obj.Name).ContextMenuStrip = form.FormObjList.ContextMenuStrip1
                                    Case TipoObjeto.RCT_Gibbs
                                        form.FormObjList.TreeViewObj.Nodes("NodeRGIB").Nodes.Add(obj.Name, obj.Tag).Name = obj.Name
                                        form.FormObjList.TreeViewObj.Nodes("NodeRGIB").Nodes(obj.Name).ContextMenuStrip = form.FormObjList.ContextMenuStrip1
                                    Case TipoObjeto.RCT_CSTR
                                        form.FormObjList.TreeViewObj.Nodes("NodeRCSTR").Nodes.Add(obj.Name, obj.Tag).Name = obj.Name
                                        form.FormObjList.TreeViewObj.Nodes("NodeRCSTR").Nodes(obj.Name).ContextMenuStrip = form.FormObjList.ContextMenuStrip1
                                    Case TipoObjeto.RCT_PFR
                                        form.FormObjList.TreeViewObj.Nodes("NodeRPFR").Nodes.Add(obj.Name, obj.Tag).Name = obj.Name
                                        form.FormObjList.TreeViewObj.Nodes("NodeRPFR").Nodes(obj.Name).ContextMenuStrip = form.FormObjList.ContextMenuStrip1
                                    Case TipoObjeto.HeatExchanger
                                        form.FormObjList.TreeViewObj.Nodes("NodeHE").Nodes.Add(obj.Name, obj.Tag).Name = obj.Name
                                        form.FormObjList.TreeViewObj.Nodes("NodeHE").Nodes(obj.Name).ContextMenuStrip = form.FormObjList.ContextMenuStrip1
                                    Case TipoObjeto.ShortcutColumn
                                        form.FormObjList.TreeViewObj.Nodes("NodeSC").Nodes.Add(obj.Name, obj.Tag).Name = obj.Name
                                        form.FormObjList.TreeViewObj.Nodes("NodeSC").Nodes(obj.Name).ContextMenuStrip = form.FormObjList.ContextMenuStrip1
                                    Case TipoObjeto.DistillationColumn
                                        form.FormObjList.TreeViewObj.Nodes("NodeDC").Nodes.Add(obj.Name, obj.Tag).Name = obj.Name
                                        form.FormObjList.TreeViewObj.Nodes("NodeDC").Nodes(obj.Name).ContextMenuStrip = form.FormObjList.ContextMenuStrip1
                                    Case TipoObjeto.AbsorptionColumn
                                        form.FormObjList.TreeViewObj.Nodes("NodeAC").Nodes.Add(obj.Name, obj.Tag).Name = obj.Name
                                        form.FormObjList.TreeViewObj.Nodes("NodeAC").Nodes(obj.Name).ContextMenuStrip = form.FormObjList.ContextMenuStrip1
                                    Case TipoObjeto.ReboiledAbsorber
                                        form.FormObjList.TreeViewObj.Nodes("NodeRBA").Nodes.Add(obj.Name, obj.Tag).Name = obj.Name
                                        form.FormObjList.TreeViewObj.Nodes("NodeRBA").Nodes(obj.Name).ContextMenuStrip = form.FormObjList.ContextMenuStrip1
                                    Case TipoObjeto.RefluxedAbsorber
                                        form.FormObjList.TreeViewObj.Nodes("NodeRFA").Nodes.Add(obj.Name, obj.Tag).Name = obj.Name
                                        form.FormObjList.TreeViewObj.Nodes("NodeRFA").Nodes(obj.Name).ContextMenuStrip = form.FormObjList.ContextMenuStrip1
                                    Case TipoObjeto.ComponentSeparator
                                        form.FormObjList.TreeViewObj.Nodes("NodeCSEP").Nodes.Add(obj.Name, obj.Tag).Name = obj.Name
                                        form.FormObjList.TreeViewObj.Nodes("NodeCSEP").Nodes(obj.Name).ContextMenuStrip = form.FormObjList.ContextMenuStrip1
                                    Case TipoObjeto.OrificePlate
                                        form.FormObjList.TreeViewObj.Nodes("NodeOPL").Nodes.Add(obj.Name, obj.Tag).Name = obj.Name
                                        form.FormObjList.TreeViewObj.Nodes("NodeOPL").Nodes(obj.Name).ContextMenuStrip = form.FormObjList.ContextMenuStrip1
                                    Case TipoObjeto.CustomUO
                                        form.FormObjList.TreeViewObj.Nodes("NodeUO").Nodes.Add(obj.Name, obj.Tag).Name = obj.Name
                                        form.FormObjList.TreeViewObj.Nodes("NodeUO").Nodes(obj.Name).ContextMenuStrip = form.FormObjList.ContextMenuStrip1
                                    Case TipoObjeto.ExcelUO
                                        form.FormObjList.TreeViewObj.Nodes("NodeExcel").Nodes.Add(obj.Name, obj.Tag).Name = obj.Name
                                        form.FormObjList.TreeViewObj.Nodes("NodeExcel").Nodes(obj.Name).ContextMenuStrip = form.FormObjList.ContextMenuStrip1
                                    Case TipoObjeto.CapeOpenUO
                                        form.FormObjList.TreeViewObj.Nodes("NodeCOUO").Nodes.Add(obj.Name, obj.Tag).Name = obj.Name
                                        form.FormObjList.TreeViewObj.Nodes("NodeCOUO").Nodes(obj.Name).ContextMenuStrip = form.FormObjList.ContextMenuStrip1
                                    Case TipoObjeto.SolidSeparator
                                        form.FormObjList.TreeViewObj.Nodes("NodeSS").Nodes.Add(obj.Name, obj.Tag).Name = obj.Name
                                        form.FormObjList.TreeViewObj.Nodes("NodeSS").Nodes(obj.Name).ContextMenuStrip = form.FormObjList.ContextMenuStrip1
                                    Case TipoObjeto.Filter
                                        form.FormObjList.TreeViewObj.Nodes("NodeFT").Nodes.Add(obj.Name, obj.Tag).Name = obj.Name
                                        form.FormObjList.TreeViewObj.Nodes("NodeFT").Nodes(obj.Name).ContextMenuStrip = form.FormObjList.ContextMenuStrip1
                                    Case TipoObjeto.FlowsheetUO
                                        form.FormObjList.TreeViewObj.Nodes("NodeFS").Nodes.Add(obj.Name, obj.Tag).Name = obj.Name
                                        form.FormObjList.TreeViewObj.Nodes("NodeFS").Nodes(obj.Name).ContextMenuStrip = form.FormObjList.ContextMenuStrip1
                                End Select
                            End If
                        End With
                    End If
                Catch ex As Exception
                    excs.Add(New Exception("Error Loading Flowsheet Graphic Objects", ex))
                End Try
            Next

            For Each xel As XElement In data
                Try
                    Dim id As String = xel.Element("Name").Value
                    If id <> "" Then
                        Dim obj As GraphicObject = (From go As GraphicObject In
                                                                form.FormSurface.FlowsheetDesignSurface.drawingObjects Where go.Name = id).SingleOrDefault
                        If Not obj Is Nothing Then
                            Dim i As Integer = 0
                            For Each xel2 As XElement In xel.Element("InputConnectors").Elements
                                If xel2.@IsAttached = True Then
                                    obj.InputConnectors(i).ConnectorName = xel2.@AttachedFromObjID & "|" & xel2.@AttachedFromConnIndex
                                    obj.InputConnectors(i).Type = [Enum].Parse(obj.InputConnectors(i).Type.GetType, xel2.@ConnType)
                                End If
                                i += 1
                            Next
                        End If
                    End If
                Catch ex As Exception
                    excs.Add(New Exception("Error Loading Flowsheet Object Connection Information", ex))
                End Try
            Next

            For Each xel As XElement In data
                Try
                    Dim id As String = xel.Element("Name").Value
                    If id <> "" Then
                        Dim obj As GraphicObject = (From go As GraphicObject In
                                                                form.FormSurface.FlowsheetDesignSurface.drawingObjects Where go.Name = id).SingleOrDefault
                        If Not obj Is Nothing Then
                            For Each xel2 As XElement In xel.Element("OutputConnectors").Elements
                                If xel2.@IsAttached = True Then
                                    Dim objToID = xel2.@AttachedToObjID
                                    If objToID <> "" Then
                                        Dim objTo As GraphicObject = (From go As GraphicObject In
                                                                                        form.FormSurface.FlowsheetDesignSurface.drawingObjects Where go.Name = objToID).SingleOrDefault
                                        Dim fromidx As Integer = -1
                                        Dim cp As ConnectionPoint = (From cp2 As ConnectionPoint In objTo.InputConnectors Select cp2 Where cp2.ConnectorName.Split("|")(0) = obj.Name).SingleOrDefault
                                        If Not cp Is Nothing Then
                                            fromidx = cp.ConnectorName.Split("|")(1)
                                        End If
                                        form.ConnectObject(obj, objTo, fromidx, xel2.@AttachedToConnIndex)
                                    End If
                                End If
                            Next
                            For Each xel2 As XElement In xel.Element("EnergyConnector").Elements
                                If xel2.@IsAttached = True Then
                                    Dim objToID = xel2.@AttachedToObjID
                                    If objToID <> "" Then
                                        Dim objTo As GraphicObject = (From go As GraphicObject In
                                                                                        form.FormSurface.FlowsheetDesignSurface.drawingObjects Where go.Name = objToID).SingleOrDefault
                                        form.ConnectObject(obj, objTo, -1, xel2.@AttachedToConnIndex)
                                    End If
                                End If
                            Next
                        End If
                    End If
                Catch ex As Exception
                    excs.Add(New Exception("Error Loading Flowsheet Object Connection Information", ex))
                End Try
            Next

            data = xdoc.Element("DWSIM_Simulation_Data").Element("Compounds").Elements.ToList

            For Each xel As XElement In data
                Try
                    Dim obj As New ConstantProperties
                    obj.LoadData(xel.Elements.ToList)
                    form.Options.SelectedComponents.Add(obj.Name, obj)
                Catch ex As Exception
                    excs.Add(New Exception("Error Loading Compound Information", ex))
                End Try
            Next

            data = xdoc.Element("DWSIM_Simulation_Data").Element("PropertyPackages").Elements.ToList

            For Each xel As XElement In data
                Try
                    Dim t As Type = Type.GetType(xel.Element("Type").Value, False)
                    Dim obj As PropertyPackage = Activator.CreateInstance(t)
                    obj.LoadData(xel.Elements.ToList)
                    Dim newID As String = Guid.NewGuid.ToString
                    If form.Options.PropertyPackages.ContainsKey(obj.UniqueID) Then obj.UniqueID = newID
                    form.Options.PropertyPackages.Add(obj.UniqueID, obj)
                Catch ex As Exception
                    excs.Add(New Exception("Error Loading Property Package Information", ex))
                End Try
            Next

            data = xdoc.Element("DWSIM_Simulation_Data").Element("SimulationObjects").Elements.ToList

            For Each xel As XElement In data
                Try
                    Dim id As String = xel.<Nome>.Value
                    Dim t As Type = Type.GetType(xel.Element("Type").Value, False)
                    Dim obj As SimulationObjects_BaseClass = Activator.CreateInstance(t)
                    Dim gobj As GraphicObject = (From go As GraphicObject In
                                        form.FormSurface.FlowsheetDesignSurface.drawingObjects Where go.Name = id).SingleOrDefault
                    obj.GraphicObject = gobj
                    obj.FillNodeItems(True)
                    obj.QTFillNodeItems()
                    If Not gobj Is Nothing Then
                        form.Collections.ObjectCollection.Add(id, obj)
                        obj.LoadData(xel.Elements.ToList)
                        obj.SetFlowsheet(form)
                        If TypeOf obj Is Streams.MaterialStream Then
                            For Each phase As DWSIM.ClassesBasicasTermodinamica.Fase In DirectCast(obj, Streams.MaterialStream).Fases.Values
                                For Each c As ConstantProperties In form.Options.SelectedComponents.Values
                                    phase.Componentes(c.Name).ConstantProperties = c
                                Next
                            Next
                        End If
                        With form.Collections
                            Select Case gobj.TipoObjeto
                                Case Microsoft.Msdn.Samples.GraphicObjects.TipoObjeto.Compressor
                                    .CLCS_CompressorCollection.Add(id, obj)
                                Case Microsoft.Msdn.Samples.GraphicObjects.TipoObjeto.Cooler
                                    .CLCS_CoolerCollection.Add(id, obj)
                                Case Microsoft.Msdn.Samples.GraphicObjects.TipoObjeto.EnergyStream
                                    .CLCS_EnergyStreamCollection.Add(id, obj)
                                Case Microsoft.Msdn.Samples.GraphicObjects.TipoObjeto.Heater
                                    .CLCS_HeaterCollection.Add(id, obj)
                                Case Microsoft.Msdn.Samples.GraphicObjects.TipoObjeto.MaterialStream
                                    .CLCS_MaterialStreamCollection.Add(id, obj)
                                Case Microsoft.Msdn.Samples.GraphicObjects.TipoObjeto.NodeEn
                                    .CLCS_EnergyMixerCollection.Add(id, obj)
                                Case Microsoft.Msdn.Samples.GraphicObjects.TipoObjeto.NodeIn
                                    .CLCS_MixerCollection.Add(id, obj)
                                Case Microsoft.Msdn.Samples.GraphicObjects.TipoObjeto.NodeOut
                                    .CLCS_SplitterCollection.Add(id, obj)
                                Case Microsoft.Msdn.Samples.GraphicObjects.TipoObjeto.Pipe
                                    .CLCS_PipeCollection.Add(id, obj)
                                Case Microsoft.Msdn.Samples.GraphicObjects.TipoObjeto.Pump
                                    .CLCS_PumpCollection.Add(id, obj)
                                Case Microsoft.Msdn.Samples.GraphicObjects.TipoObjeto.Tank
                                    .CLCS_TankCollection.Add(id, obj)
                                Case Microsoft.Msdn.Samples.GraphicObjects.TipoObjeto.Expander
                                    .CLCS_TurbineCollection.Add(id, obj)
                                Case Microsoft.Msdn.Samples.GraphicObjects.TipoObjeto.Valve
                                    .CLCS_ValveCollection.Add(id, obj)
                                Case Microsoft.Msdn.Samples.GraphicObjects.TipoObjeto.Vessel
                                    .CLCS_VesselCollection.Add(id, obj)
                                Case Microsoft.Msdn.Samples.GraphicObjects.TipoObjeto.GO_Tabela
                                    .ObjectCollection(gobj.Tag).Tabela = gobj
                                Case Microsoft.Msdn.Samples.GraphicObjects.TipoObjeto.Expander
                                    .CLCS_TurbineCollection.Add(id, obj)
                                Case Microsoft.Msdn.Samples.GraphicObjects.TipoObjeto.OT_Ajuste
                                    .CLCS_AdjustCollection.Add(id, obj)
                                Case Microsoft.Msdn.Samples.GraphicObjects.TipoObjeto.OT_Reciclo
                                    .CLCS_RecycleCollection.Add(id, obj)
                                Case Microsoft.Msdn.Samples.GraphicObjects.TipoObjeto.OT_Especificacao
                                    .CLCS_SpecCollection.Add(id, obj)
                                Case Microsoft.Msdn.Samples.GraphicObjects.TipoObjeto.RCT_Conversion
                                    .CLCS_ReactorConversionCollection.Add(id, obj)
                                Case Microsoft.Msdn.Samples.GraphicObjects.TipoObjeto.RCT_Equilibrium
                                    .CLCS_ReactorEquilibriumCollection.Add(id, obj)
                                    .ReactorEquilibriumCollection(gobj.Name) = gobj
                                Case Microsoft.Msdn.Samples.GraphicObjects.TipoObjeto.RCT_Gibbs
                                    .CLCS_ReactorGibbsCollection.Add(id, obj)
                                Case Microsoft.Msdn.Samples.GraphicObjects.TipoObjeto.RCT_CSTR
                                    .CLCS_ReactorCSTRCollection.Add(id, obj)
                                Case Microsoft.Msdn.Samples.GraphicObjects.TipoObjeto.RCT_PFR
                                    .CLCS_ReactorPFRCollection.Add(id, obj)
                                Case Microsoft.Msdn.Samples.GraphicObjects.TipoObjeto.HeatExchanger
                                    .CLCS_HeatExchangerCollection.Add(id, obj)
                                Case Microsoft.Msdn.Samples.GraphicObjects.TipoObjeto.ShortcutColumn
                                    .CLCS_ShortcutColumnCollection.Add(id, obj)
                                Case Microsoft.Msdn.Samples.GraphicObjects.TipoObjeto.DistillationColumn
                                    .CLCS_DistillationColumnCollection.Add(id, obj)
                                Case Microsoft.Msdn.Samples.GraphicObjects.TipoObjeto.AbsorptionColumn
                                    .CLCS_AbsorptionColumnCollection.Add(id, obj)
                                Case Microsoft.Msdn.Samples.GraphicObjects.TipoObjeto.RefluxedAbsorber
                                    .CLCS_RefluxedAbsorberCollection.Add(id, obj)
                                Case Microsoft.Msdn.Samples.GraphicObjects.TipoObjeto.ReboiledAbsorber
                                    .CLCS_ReboiledAbsorberCollection.Add(id, obj)
                                Case Microsoft.Msdn.Samples.GraphicObjects.TipoObjeto.OT_EnergyRecycle
                                    .CLCS_EnergyRecycleCollection.Add(id, obj)
                                Case Microsoft.Msdn.Samples.GraphicObjects.TipoObjeto.GO_TabelaRapida
                                    .ObjectCollection(CType(gobj, DWSIM.GraphicObjects.QuickTableGraphic).BaseOwner.Nome).TabelaRapida = gobj
                                Case Microsoft.Msdn.Samples.GraphicObjects.TipoObjeto.ComponentSeparator
                                    .CLCS_ComponentSeparatorCollection.Add(id, obj)
                                Case Microsoft.Msdn.Samples.GraphicObjects.TipoObjeto.OrificePlate
                                    .CLCS_OrificePlateCollection.Add(id, obj)
                                Case Microsoft.Msdn.Samples.GraphicObjects.TipoObjeto.CustomUO
                                    .CLCS_CustomUOCollection.Add(id, obj)
                                Case Microsoft.Msdn.Samples.GraphicObjects.TipoObjeto.ExcelUO
                                    .CLCS_ExcelUOCollection.Add(id, obj)
                                Case Microsoft.Msdn.Samples.GraphicObjects.TipoObjeto.CapeOpenUO
                                    .CLCS_CapeOpenUOCollection.Add(id, obj)
                                Case Microsoft.Msdn.Samples.GraphicObjects.TipoObjeto.SolidSeparator
                                    .CLCS_SolidsSeparatorCollection.Add(id, obj)
                                Case Microsoft.Msdn.Samples.GraphicObjects.TipoObjeto.Filter
                                    .CLCS_FilterCollection.Add(id, obj)
                                Case Microsoft.Msdn.Samples.GraphicObjects.TipoObjeto.FlowsheetUO
                                    .CLCS_FlowsheetUOCollection.Add(id, obj)
                            End Select
                        End With
                        obj.UpdatePropertyNodes(form.Options.SelectedUnitSystem, form.Options.NumberFormat)
                    End If
                Catch ex As Exception
                    excs.Add(New Exception("Error Loading Unit Operation Information", ex))
                End Try
            Next

            For Each so As SimulationObjects_BaseClass In form.Collections.ObjectCollection.Values
                Try
                    If TryCast(so, DWSIM.SimulationObjects.SpecialOps.Adjust) IsNot Nothing Then
                        Dim so2 As DWSIM.SimulationObjects.SpecialOps.Adjust = so
                        If form.Collections.ObjectCollection.ContainsKey(so2.ManipulatedObjectData.m_ID) Then
                            so2.ManipulatedObject = form.Collections.ObjectCollection(so2.ManipulatedObjectData.m_ID)
                            DirectCast(so2.GraphicObject, AdjustGraphic).ConnectedToMv = so2.ManipulatedObject.GraphicObject
                        End If
                        If form.Collections.ObjectCollection.ContainsKey(so2.ControlledObjectData.m_ID) Then
                            so2.ControlledObject = form.Collections.ObjectCollection(so2.ControlledObjectData.m_ID)
                            DirectCast(so2.GraphicObject, AdjustGraphic).ConnectedToCv = so2.ControlledObject.GraphicObject
                        End If
                        If form.Collections.ObjectCollection.ContainsKey(so2.ReferencedObjectData.m_ID) Then
                            so2.ReferenceObject = form.Collections.ObjectCollection(so2.ReferencedObjectData.m_ID)
                            DirectCast(so2.GraphicObject, AdjustGraphic).ConnectedToRv = so2.ReferenceObject.GraphicObject
                        End If
                    End If
                    If TryCast(so, DWSIM.SimulationObjects.SpecialOps.Spec) IsNot Nothing Then
                        Dim so2 As DWSIM.SimulationObjects.SpecialOps.Spec = so
                        If form.Collections.ObjectCollection.ContainsKey(so2.TargetObjectData.m_ID) Then
                            so2.TargetObject = form.Collections.ObjectCollection(so2.TargetObjectData.m_ID)
                            DirectCast(so2.GraphicObject, SpecGraphic).ConnectedToTv = so2.TargetObject.GraphicObject
                        End If
                        If form.Collections.ObjectCollection.ContainsKey(so2.SourceObjectData.m_ID) Then
                            so2.SourceObject = form.Collections.ObjectCollection(so2.SourceObjectData.m_ID)
                            DirectCast(so2.GraphicObject, SpecGraphic).ConnectedToSv = so2.SourceObject.GraphicObject
                        End If
                    End If
                    If TryCast(so, DWSIM.SimulationObjects.UnitOps.CapeOpenUO) IsNot Nothing Then
                        DirectCast(so, DWSIM.SimulationObjects.UnitOps.CapeOpenUO).UpdateConnectors2()
                        DirectCast(so, DWSIM.SimulationObjects.UnitOps.CapeOpenUO).UpdatePortsFromConnectors()
                    End If
                Catch ex As Exception
                    excs.Add(New Exception("Error Loading Unit Operation Connection Information", ex))
                End Try
            Next

            data = xdoc.Element("DWSIM_Simulation_Data").Element("GraphicObjects").Elements.ToList

            For Each xel2 As XElement In (From xel As XElement In data Select xel Where xel.<Type>.Value.Equals("DWSIM.DWSIM.GraphicObjects.TableGraphic")).ToList
                Try
                    Dim obj As GraphicObject = Nothing
                    Dim t As Type = Type.GetType(xel2.Element("Type").Value, False)
                    If Not t Is Nothing Then obj = Activator.CreateInstance(t)
                    If obj Is Nothing Then
                        obj = GraphicObject.ReturnInstance(xel2.Element("Type").Value)
                    End If
                    obj.LoadData(xel2.Elements.ToList)
                    DirectCast(obj, DWSIM.GraphicObjects.TableGraphic).BaseOwner = form.Collections.ObjectCollection(xel2.<Owner>.Value)
                    form.Collections.ObjectCollection(xel2.<Owner>.Value).Tabela = obj
                    form.FormSurface.FlowsheetDesignSurface.drawingObjects.Add(obj)
                Catch ex As Exception
                    excs.Add(New Exception("Error Loading Flowsheet Table Information", ex))
                End Try
            Next

            data = xdoc.Element("DWSIM_Simulation_Data").Element("ReactionSets").Elements.ToList

            form.Options.ReactionSets.Clear()

            For Each xel As XElement In data
                Try
                    Dim obj As New ReactionSet()
                    obj.LoadData(xel.Elements.ToList)
                    form.Options.ReactionSets.Add(obj.ID, obj)
                Catch ex As Exception
                    excs.Add(New Exception("Error Loading Reaction Set Information", ex))
                End Try
            Next

            data = xdoc.Element("DWSIM_Simulation_Data").Element("Reactions").Elements.ToList

            For Each xel As XElement In data
                Try
                    Dim obj As New Reaction()
                    obj.LoadData(xel.Elements.ToList)
                    form.Options.Reactions.Add(obj.ID, obj)
                Catch ex As Exception
                    excs.Add(New Exception("Error Loading Reaction Information", ex))
                End Try
            Next

            form.ScriptCollection = New Dictionary(Of String, Script)

            If xdoc.Element("DWSIM_Simulation_Data").Element("ScriptItems") IsNot Nothing Then

                data = xdoc.Element("DWSIM_Simulation_Data").Element("ScriptItems").Elements.ToList

                Dim i As Integer = 0
                For Each xel As XElement In data
                    Try
                        Dim obj As New DWSIM.Outros.Script()
                        obj.LoadData(xel.Elements.ToList)
                        form.ScriptCollection.Add(obj.ID, obj)
                    Catch ex As Exception
                        excs.Add(New Exception("Error Loading Script Item Information", ex))
                    End Try
                    i += 1
                Next

            End If

            Try
                Dim data1 As String = xdoc.Element("DWSIM_Simulation_Data").Element("Spreadsheet").Element("Data1").Value
                Dim data2 As String = xdoc.Element("DWSIM_Simulation_Data").Element("Spreadsheet").Element("Data2").Value
                If data1 <> "" Then form.FormSpreadsheet.CopyDT1FromString(data1)
                If data2 <> "" Then form.FormSpreadsheet.CopyDT2FromString(data2)
            Catch ex As Exception
                excs.Add(New Exception("Error Loading Spreadsheet Information", ex))
            End Try

            For Each pp As DWSIM.SimulationObjects.PropertyPackages.PropertyPackage In form.Options.PropertyPackages.Values
                Try
                    If pp.ConfigForm Is Nothing Then pp.ReconfigureConfigForm()
                Catch ex As Exception
                    excs.Add(New Exception("Error Reconfiguring Property Package", ex))
                End Try
            Next

            form.Options.NotSelectedComponents = New Dictionary(Of String, DWSIM.ClassesBasicasTermodinamica.ConstantProperties)

            Dim tmpc As DWSIM.ClassesBasicasTermodinamica.ConstantProperties
            For Each tmpc In FormMain.AvailableComponents.Values
                Dim newc As New DWSIM.ClassesBasicasTermodinamica.ConstantProperties
                newc = tmpc
                If Not form.Options.SelectedComponents.ContainsKey(tmpc.Name) Then
                    form.Options.NotSelectedComponents.Add(tmpc.Name, newc)
                End If
            Next

            form.FormProps.DockPanel = Nothing
            form.FormSurface.DockPanel = Nothing

            Try
                form.FormProps.Show(form.dckPanel)
                form.FormSurface.Show(form.dckPanel)
                form.dckPanel.BringToFront()
                form.dckPanel.UpdateDockWindowZOrder(DockStyle.Fill, True)
            Catch ex As Exception
                excs.Add(New Exception("Error Restoring Window Layout", ex))
            End Try

            If excs.Count > 0 Then
                Me.FlowSheet.WriteToLog("Some errors where found while parsing the XML file. The simulation might not work as expected. Please read the subsequent messages for more details.", Color.DarkRed, FormClasses.TipoAviso.Erro)
                For Each ex As Exception In excs
                    Me.FlowSheet.WriteToLog(ex.Message.ToString & ": " & ex.InnerException.ToString, Color.DarkRed, FormClasses.TipoAviso.Erro)
                Next
                form.Dispose()
                Initialized = False
            Else
                fsheet = form
                Initialized = True
            End If

        End Sub

        Public Overrides Function GetPropertyValue(ByVal prop As String, Optional ByVal su As SistemasDeUnidades.Unidades = Nothing) As Object

            If su Is Nothing Then su = New DWSIM.SistemasDeUnidades.UnidadesSI
            Dim cv As New DWSIM.SistemasDeUnidades.Conversor
            Dim value As Double = 0
            Dim propidx As Integer = CInt(prop.Split("_")(2))

            Select Case propidx

            End Select

            Return value

        End Function

        Public Overloads Overrides Function GetProperties(ByVal proptype As SimulationObjects_BaseClass.PropertyType) As String()
            Dim i As Integer = 0
            Dim proplist As New ArrayList
            Select Case proptype
                Case PropertyType.RW
                    For i = 2 To 2
                        proplist.Add("PROP_TK_" + CStr(i))
                    Next
                Case PropertyType.RW
                    For i = 0 To 2
                        proplist.Add("PROP_TK_" + CStr(i))
                    Next
                Case PropertyType.WR
                    For i = 0 To 1
                        proplist.Add("PROP_TK_" + CStr(i))
                    Next
                Case PropertyType.ALL
                    For i = 0 To 2
                        proplist.Add("PROP_TK_" + CStr(i))
                    Next
            End Select
            Return proplist.ToArray(GetType(System.String))
            proplist = Nothing
        End Function

        Public Overrides Function SetPropertyValue(ByVal prop As String, ByVal propval As Object, Optional ByVal su As DWSIM.SistemasDeUnidades.Unidades = Nothing) As Object
            If su Is Nothing Then su = New DWSIM.SistemasDeUnidades.UnidadesSI
            Dim cv As New DWSIM.SistemasDeUnidades.Conversor
            Dim propidx As Integer = CInt(prop.Split("_")(2))

            Select Case propidx
                Case 0
                    'PROP_TK_0	Pressure Drop
                Case 1
                    'PROP_TK_1	Volume
            End Select
            Return 1
        End Function

        Public Overrides Function GetPropertyUnit(ByVal prop As String, Optional ByVal su As SistemasDeUnidades.Unidades = Nothing) As Object
            If su Is Nothing Then su = New DWSIM.SistemasDeUnidades.UnidadesSI
            Dim cv As New DWSIM.SistemasDeUnidades.Conversor
            Dim value As String = ""
            Dim propidx As Integer = CInt(prop.Split("_")(2))

            Select Case propidx

                Case 0
                    'PROP_TK_0	Pressure Drop
                    value = su.spmp_deltaP

                Case 1
                    'PROP_TK_1	Volume
                    value = su.volume

                Case 2
                    'PROP_TK_2	Residence Time
                    value = su.time

            End Select

            Return value
        End Function

        Public Overrides Sub QTFillNodeItems()

        End Sub

        Public Overrides Sub UpdatePropertyNodes(su As SistemasDeUnidades.Unidades, nf As String)

        End Sub

        Public Overrides Sub PopulatePropertyGrid(ByRef pgrid As PropertyGridEx.PropertyGridEx, ByVal su As SistemasDeUnidades.Unidades)

            Dim Conversor As New DWSIM.SistemasDeUnidades.Conversor

            With pgrid

                .PropertySort = PropertySort.Categorized
                .ShowCustomProperties = True
                .Item.Clear()

                MyBase.PopulatePropertyGrid(pgrid, su)

                Dim ent1, ent2, ent3, ent4, ent5, ent6, ent7, ent8, ent9, ent10, saida1, saida2, saida3, saida4, saida5, saida6, saida7, saida8, saida9, saida10 As String

                If Me.GraphicObject.InputConnectors(0).IsAttached = True Then ent1 = Me.GraphicObject.InputConnectors(0).AttachedConnector.AttachedFrom.Tag Else ent1 = ""
                If Me.GraphicObject.InputConnectors(1).IsAttached = True Then ent2 = Me.GraphicObject.InputConnectors(1).AttachedConnector.AttachedFrom.Tag Else ent2 = ""
                If Me.GraphicObject.InputConnectors(2).IsAttached = True Then ent3 = Me.GraphicObject.InputConnectors(2).AttachedConnector.AttachedFrom.Tag Else ent3 = ""
                If Me.GraphicObject.InputConnectors(3).IsAttached = True Then ent4 = Me.GraphicObject.InputConnectors(3).AttachedConnector.AttachedFrom.Tag Else ent4 = ""
                If Me.GraphicObject.InputConnectors(4).IsAttached = True Then ent5 = Me.GraphicObject.InputConnectors(4).AttachedConnector.AttachedFrom.Tag Else ent5 = ""
                If Me.GraphicObject.InputConnectors(5).IsAttached = True Then ent6 = Me.GraphicObject.InputConnectors(5).AttachedConnector.AttachedFrom.Tag Else ent6 = ""
                If Me.GraphicObject.InputConnectors(6).IsAttached = True Then ent7 = Me.GraphicObject.InputConnectors(6).AttachedConnector.AttachedFrom.Tag Else ent7 = ""
                If Me.GraphicObject.InputConnectors(7).IsAttached = True Then ent8 = Me.GraphicObject.InputConnectors(7).AttachedConnector.AttachedFrom.Tag Else ent8 = ""
                If Me.GraphicObject.InputConnectors(8).IsAttached = True Then ent9 = Me.GraphicObject.InputConnectors(8).AttachedConnector.AttachedFrom.Tag Else ent9 = ""
                If Me.GraphicObject.InputConnectors(9).IsAttached = True Then ent10 = Me.GraphicObject.InputConnectors(9).AttachedConnector.AttachedFrom.Tag Else ent10 = ""

                If Me.GraphicObject.OutputConnectors(0).IsAttached = True Then saida1 = Me.GraphicObject.OutputConnectors(0).AttachedConnector.AttachedTo.Tag Else saida1 = ""
                If Me.GraphicObject.OutputConnectors(1).IsAttached = True Then saida2 = Me.GraphicObject.OutputConnectors(1).AttachedConnector.AttachedTo.Tag Else saida2 = ""
                If Me.GraphicObject.OutputConnectors(2).IsAttached = True Then saida3 = Me.GraphicObject.OutputConnectors(2).AttachedConnector.AttachedTo.Tag Else saida3 = ""
                If Me.GraphicObject.OutputConnectors(3).IsAttached = True Then saida4 = Me.GraphicObject.OutputConnectors(3).AttachedConnector.AttachedTo.Tag Else saida4 = ""
                If Me.GraphicObject.OutputConnectors(4).IsAttached = True Then saida5 = Me.GraphicObject.OutputConnectors(4).AttachedConnector.AttachedTo.Tag Else saida5 = ""
                If Me.GraphicObject.OutputConnectors(5).IsAttached = True Then saida6 = Me.GraphicObject.OutputConnectors(5).AttachedConnector.AttachedTo.Tag Else saida6 = ""
                If Me.GraphicObject.OutputConnectors(6).IsAttached = True Then saida7 = Me.GraphicObject.OutputConnectors(6).AttachedConnector.AttachedTo.Tag Else saida7 = ""
                If Me.GraphicObject.OutputConnectors(7).IsAttached = True Then saida8 = Me.GraphicObject.OutputConnectors(7).AttachedConnector.AttachedTo.Tag Else saida8 = ""
                If Me.GraphicObject.OutputConnectors(8).IsAttached = True Then saida9 = Me.GraphicObject.OutputConnectors(8).AttachedConnector.AttachedTo.Tag Else saida9 = ""
                If Me.GraphicObject.OutputConnectors(9).IsAttached = True Then saida10 = Me.GraphicObject.OutputConnectors(9).AttachedConnector.AttachedTo.Tag Else saida10 = ""

                '==== Streams (1) =======================
                '==== Input streams ===
                .Item.Add(DWSIM.App.GetLocalString("Correntedeentrada1"), ent1, False, DWSIM.App.GetLocalString("Conexes1"), "", True)
                With .Item(.Item.Count - 1)
                    .DefaultValue = Nothing
                    .CustomEditor = New DWSIM.Editors.Streams.UIInputMSSelector
                End With
                .Item.Add(DWSIM.App.GetLocalString("Correntedeentrada2"), ent2, False, DWSIM.App.GetLocalString("Conexes1"), "", True)
                With .Item(.Item.Count - 1)
                    .DefaultValue = Nothing
                    .CustomEditor = New DWSIM.Editors.Streams.UIInputMSSelector
                End With
                .Item.Add(DWSIM.App.GetLocalString("Correntedeentrada3"), ent3, False, DWSIM.App.GetLocalString("Conexes1"), "", True)
                With .Item(.Item.Count - 1)
                    .DefaultValue = Nothing
                    .CustomEditor = New DWSIM.Editors.Streams.UIInputMSSelector
                End With
                .Item.Add(DWSIM.App.GetLocalString("Correntedeentrada4"), ent4, False, DWSIM.App.GetLocalString("Conexes1"), "", True)
                With .Item(.Item.Count - 1)
                    .DefaultValue = Nothing
                    .CustomEditor = New DWSIM.Editors.Streams.UIInputMSSelector
                End With
                .Item.Add(DWSIM.App.GetLocalString("Correntedeentrada5"), ent5, False, DWSIM.App.GetLocalString("Conexes1"), "", True)
                With .Item(.Item.Count - 1)
                    .DefaultValue = Nothing
                    .CustomEditor = New DWSIM.Editors.Streams.UIInputMSSelector
                End With
                .Item.Add(DWSIM.App.GetLocalString("Correntedeentrada6"), ent6, False, DWSIM.App.GetLocalString("Conexes1"), "", True)
                With .Item(.Item.Count - 1)
                    .DefaultValue = Nothing
                    .CustomEditor = New DWSIM.Editors.Streams.UIInputMSSelector
                End With
                .Item.Add(DWSIM.App.GetLocalString("Correntedeentrada7"), ent7, False, DWSIM.App.GetLocalString("Conexes1"), "", True)
                With .Item(.Item.Count - 1)
                    .DefaultValue = Nothing
                    .CustomEditor = New DWSIM.Editors.Streams.UIInputMSSelector
                End With
                .Item.Add(DWSIM.App.GetLocalString("Correntedeentrada8"), ent8, False, DWSIM.App.GetLocalString("Conexes1"), "", True)
                With .Item(.Item.Count - 1)
                    .DefaultValue = Nothing
                    .CustomEditor = New DWSIM.Editors.Streams.UIInputMSSelector
                End With
                .Item.Add(DWSIM.App.GetLocalString("Correntedeentrada9"), ent9, False, DWSIM.App.GetLocalString("Conexes1"), "", True)
                With .Item(.Item.Count - 1)
                    .DefaultValue = Nothing
                    .CustomEditor = New DWSIM.Editors.Streams.UIInputMSSelector
                End With
                .Item.Add(DWSIM.App.GetLocalString("Correntedeentrada10"), ent10, False, DWSIM.App.GetLocalString("Conexes1"), "", True)
                With .Item(.Item.Count - 1)
                    .DefaultValue = Nothing
                    .CustomEditor = New DWSIM.Editors.Streams.UIInputMSSelector
                End With

                '==== Output streams ===
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
                .Item.Add(DWSIM.App.GetLocalString("Correntedesaida4"), saida4, False, DWSIM.App.GetLocalString("Conexes1"), "", True)
                With .Item(.Item.Count - 1)
                    .DefaultValue = Nothing
                    .CustomEditor = New DWSIM.Editors.Streams.UIOutputMSSelector
                End With
                .Item.Add(DWSIM.App.GetLocalString("Correntedesaida5"), saida5, False, DWSIM.App.GetLocalString("Conexes1"), "", True)
                With .Item(.Item.Count - 1)
                    .DefaultValue = Nothing
                    .CustomEditor = New DWSIM.Editors.Streams.UIOutputMSSelector
                End With
                .Item.Add(DWSIM.App.GetLocalString("Correntedesaida6"), saida6, False, DWSIM.App.GetLocalString("Conexes1"), "", True)
                With .Item(.Item.Count - 1)
                    .DefaultValue = Nothing
                    .CustomEditor = New DWSIM.Editors.Streams.UIOutputMSSelector
                End With
                .Item.Add(DWSIM.App.GetLocalString("Correntedesaida7"), saida7, False, DWSIM.App.GetLocalString("Conexes1"), "", True)
                With .Item(.Item.Count - 1)
                    .DefaultValue = Nothing
                    .CustomEditor = New DWSIM.Editors.Streams.UIOutputMSSelector
                End With
                .Item.Add(DWSIM.App.GetLocalString("Correntedesaida8"), saida8, False, DWSIM.App.GetLocalString("Conexes1"), "", True)
                With .Item(.Item.Count - 1)
                    .DefaultValue = Nothing
                    .CustomEditor = New DWSIM.Editors.Streams.UIOutputMSSelector
                End With
                .Item.Add(DWSIM.App.GetLocalString("Correntedesaida9"), saida9, False, DWSIM.App.GetLocalString("Conexes1"), "", True)
                With .Item(.Item.Count - 1)
                    .DefaultValue = Nothing
                    .CustomEditor = New DWSIM.Editors.Streams.UIOutputMSSelector
                End With
                .Item.Add(DWSIM.App.GetLocalString("Correntedesaida10"), saida10, False, DWSIM.App.GetLocalString("Conexes1"), "", True)
                With .Item(.Item.Count - 1)
                    .DefaultValue = Nothing
                    .CustomEditor = New DWSIM.Editors.Streams.UIOutputMSSelector
                End With

                .Item.Add(DWSIM.App.GetLocalString("SimulationFile"), Me, "SimulationFile", False, DWSIM.App.GetLocalString("Parmetrosdeclculo2"), DWSIM.App.GetLocalString("SimulationFileDesc"), True)
                .Item(.Item.Count - 1).CustomEditor = New PropertyGridEx.UIFilenameEditor

                If IO.File.Exists(SimulationFile) Then
                    .Item.Add(DWSIM.App.GetLocalString("FlowsheetUOEditor"), "", False, DWSIM.App.GetLocalString("Parmetrosdeclculo2"), DWSIM.App.GetLocalString("FlowsheetUOEditorDesc"), True)
                    .Item(.Item.Count - 1).DefaultValue = Nothing
                    .Item(.Item.Count - 1).CustomEditor = New DWSIM.Editors.FlowsheetUO.UIFlowsheetUOEditor
                End If

                If Initialized Then
                    .Item.Add(DWSIM.App.GetLocalString("FlowsheetUOViewer"), "", False, DWSIM.App.GetLocalString("Parmetrosdeclculo2"), DWSIM.App.GetLocalString("FlowsheetUOViewerDesc"), True)
                    .Item(.Item.Count - 1).DefaultValue = Nothing
                    .Item(.Item.Count - 1).CustomEditor = New DWSIM.Editors.FlowsheetUO.UIFlowsheetUOViewer
                End If

                .Item.Add(DWSIM.App.GetLocalString("InitializeOnLoad"), Me, "InitializeOnLoad", False, DWSIM.App.GetLocalString("Parmetrosdeclculo2"), DWSIM.App.GetLocalString("InitializeOnLoadDesc"), True)

            End With

        End Sub

        Public Overrides Function LoadData(data As List(Of XElement)) As Boolean

            XMLSerializer.XMLSerializer.Deserialize(Me, data)

            Dim i As Integer

            i = 0
            For Each xel As XElement In (From xel2 As XElement In data Select xel2 Where xel2.Name = "InputConnections").Elements.ToList
                Me.InputConnections(i) = xel.Value
                i += 1
            Next

            i = 0
            For Each xel As XElement In (From xel2 As XElement In data Select xel2 Where xel2.Name = "OutputConnections").Elements.ToList
                Me.OutputConnections(i) = xel.Value
                i += 1
            Next

            For Each xel As XElement In (From xel2 As XElement In data Select xel2 Where xel2.Name = "InputParameters").Elements.ToList
                Dim fp As New FlowsheetUOParameter()
                fp.LoadData(xel.Elements.ToList)
                Me.InputParams.Add(fp.ID, fp)
            Next

            For Each xel As XElement In (From xel2 As XElement In data Select xel2 Where xel2.Name = "OutputParameters").Elements.ToList
                Dim fp As New FlowsheetUOParameter()
                fp.LoadData(xel.Elements.ToList)
                Me.OutputParams.Add(fp.ID, fp)
            Next

            If InitializeOnLoad Then
                If IO.File.Exists(SimulationFile) Then InitializeFlowsheet(SimulationFile)
            End If

        End Function

        Public Overrides Function SaveData() As List(Of XElement)

            Dim elements As List(Of System.Xml.Linq.XElement) = MyBase.SaveData()
            Dim ci As Globalization.CultureInfo = Globalization.CultureInfo.InvariantCulture

            With elements
                .Add(New XElement("InputConnections"))
                For Each s In InputConnections
                    .Item(.Count - 1).Add(New XElement("InputConnection", s))
                Next
                .Add(New XElement("OutputConnections"))
                For Each s In OutputConnections
                    .Item(.Count - 1).Add(New XElement("OutputConnection", s))
                Next
                .Add(New XElement("InputParameters"))
                For Each p In InputParams.Values
                    .Item(.Count - 1).Add(New XElement("FlowsheetUOParameter", p.SaveData.ToArray))
                Next
                .Add(New XElement("OutputParameters"))
                For Each p In OutputParams.Values
                    .Item(.Count - 1).Add(New XElement("FlowsheetUOParameter", p.SaveData.ToArray))
                Next
            End With

            Return elements

        End Function

    End Class

End Namespace


