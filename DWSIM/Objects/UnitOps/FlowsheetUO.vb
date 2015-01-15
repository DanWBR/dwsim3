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

Imports Microsoft.MSDN.Samples.GraphicObjects
Imports DWSIM.DWSIM.Flowsheet.FlowsheetSolver

Namespace DWSIM.SimulationObjects.UnitOps

    <System.Serializable()> Public Class FlowsheetUOParameter
        Implements XMLSerializer.Interfaces.ICustomXMLSerialization
        Public Property ObjectID As String = ""
        Public Property ObjectProperty As String = ""
        Public Property Value As Object = Nothing
        Public Property Unit As String = ""
        Public Function LoadData(data As List(Of XElement)) As Boolean Implements XMLSerializer.Interfaces.ICustomXMLSerialization.LoadData
            XMLSerializer.XMLSerializer.Deserialize(Me, data)
        End Function
        Public Function SaveData() As List(Of XElement) Implements XMLSerializer.Interfaces.ICustomXMLSerialization.SaveData
            Return XMLSerializer.XMLSerializer.Serialize(Me)
        End Function
    End Class

    <System.Serializable()> Public Class Flowsheet

        Inherits SimulationObjects_UnitOpBaseClass

        Public Property SimulationFile As String = ""
        <System.Xml.Serialization.XmlIgnore> Public Property Initialized As Boolean = False
        Public Property InputParams As Dictionary(Of String, ExcelParameter)
        Public Property OutputParams As Dictionary(Of String, ExcelParameter)
        <System.Xml.Serialization.XmlIgnore> Private Property fsheet As FormFlowsheet = Nothing

        Public Sub New(ByVal nome As String, ByVal descricao As String)

            MyBase.CreateNew()
            Me.m_ComponentName = nome
            Me.m_ComponentDescription = descricao
            Me.FillNodeItems()
            Me.QTFillNodeItems()

            InputParams = New Dictionary(Of String, ExcelParameter)
            OutputParams = New Dictionary(Of String, ExcelParameter)

        End Sub

        Public Sub New()

            MyBase.New()

            InputParams = New Dictionary(Of String, ExcelParameter)
            OutputParams = New Dictionary(Of String, ExcelParameter)

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

                .Item.Add(DWSIM.App.GetLocalString("FlowsheetUOEditor"), "", False, DWSIM.App.GetLocalString("Parmetrosdeclculo2"), DWSIM.App.GetLocalString("FlowsheetUOEditor"), True)
                .Item(.Item.Count - 1).DefaultValue = Nothing
                .Item(.Item.Count - 1).CustomEditor = New DWSIM.Editors.FlowsheetUO.UIFlowsheetUOEditor

            End With

        End Sub



    End Class

End Namespace


