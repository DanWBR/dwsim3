'    Flowsheet Object Base Classes 
'    Copyright 2008-2014 Daniel Wagner O. de Medeiros
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
Imports System.Runtime.Serialization.Formatters.Binary
Imports System.Runtime.Serialization
Imports System.IO
Imports System.Linq
Imports DWSIM.Interfaces2
Imports DWSIM.DWSIM.Flowsheet.FlowsheetSolver
Imports Microsoft.Scripting.Hosting
Imports CapeOpen
Imports System.Runtime.Serialization.Formatters
Imports System.Runtime.InteropServices.Marshal
Imports System.Runtime.InteropServices
Imports DWSIM.DWSIM.SimulationObjects
Imports System.Text

<System.Serializable()> <ComVisible(True)> Public MustInherit Class SimulationObjects_BaseClass

    Implements ICloneable, IDisposable, XMLSerializer.Interfaces.ICustomXMLSerialization

    Protected m_ComponentDescription As String = ""
    Protected m_ComponentName As String = ""

    Public Const ClassId As String = ""

    Protected m_nodeitems As System.Collections.Generic.Dictionary(Of Integer, DWSIM.Outros.NodeItem)
    Protected m_qtnodeitems As System.Collections.Generic.Dictionary(Of Integer, DWSIM.Outros.NodeItem)
    Protected m_table As DWSIM.GraphicObjects.TableGraphic
    Protected m_qtable As DWSIM.GraphicObjects.QuickTableGraphic

    Protected m_IsAdjustAttached As Boolean = False
    Protected m_AdjustId As String = ""
    Protected m_AdjustVarType As DWSIM.SimulationObjects.SpecialOps.Helpers.Adjust.TipoVar = DWSIM.SimulationObjects.SpecialOps.Helpers.Adjust.TipoVar.Nenhum

    Protected m_IsSpecAttached As Boolean = False
    Protected m_SpecId As String = ""
    Protected m_SpecVarType As DWSIM.SimulationObjects.SpecialOps.Helpers.Spec.TipoVar = DWSIM.SimulationObjects.SpecialOps.Helpers.Spec.TipoVar.Nenhum

    <System.NonSerialized()> Protected Friend m_flowsheet As FormFlowsheet

    Protected m_showqtable As Boolean = True

    Public MustOverride Sub UpdatePropertyNodes(ByVal su As DWSIM.SistemasDeUnidades.Unidades, ByVal nf As String)

    Public MustOverride Sub PopulatePropertyGrid(ByRef pgrid As PropertyGridEx.PropertyGridEx, ByVal su As DWSIM.SistemasDeUnidades.Unidades)

    Public Sub New()

    End Sub

    Public Sub FillNodeItems(Optional ByVal NoPropVals As Boolean = False)

        If Me.NodeTableItems Is Nothing Then Me.NodeTableItems = New Dictionary(Of Integer, DWSIM.Outros.NodeItem)

        With Me.NodeTableItems

            .Clear()

            Dim props As String() = Me.GetProperties(PropertyType.ALL)
            Dim key As Integer = 0

            Dim rnd As New Random

            For Each prop As String In props
                key = rnd.Next()
                If Not NoPropVals Then
                    .Add(key, New DWSIM.Outros.NodeItem(prop, GetPropertyValue(prop, FlowSheet.Options.SelectedUnitSystem), GetPropertyUnit(prop, FlowSheet.Options.SelectedUnitSystem), key, 0, ""))
                Else
                    .Add(key, New DWSIM.Outros.NodeItem(prop, "", "", key, 0, ""))
                End If
            Next

        End With


    End Sub

    Public MustOverride Sub QTFillNodeItems()

    Protected m_graphicobject As GraphicObject = Nothing

    Protected m_annotation As DWSIM.Outros.Annotation

    Public Enum PropertyType
        RO = 0
        RW = 1
        WR = 2
        ALL = 3
    End Enum

    Public MustOverride Function GetProperties(ByVal proptype As PropertyType) As String()

    Public MustOverride Function GetPropertyValue(ByVal prop As String, Optional ByVal su As DWSIM.SistemasDeUnidades.Unidades = Nothing)
    Public MustOverride Function GetPropertyUnit(ByVal prop As String, Optional ByVal su As DWSIM.SistemasDeUnidades.Unidades = Nothing)
    Public MustOverride Function SetPropertyValue(ByVal prop As String, ByVal propval As Object, Optional ByVal su As DWSIM.SistemasDeUnidades.Unidades = Nothing)

    Public Overridable Sub PropertyValueChanged(ByVal s As Object, ByVal e As System.Windows.Forms.PropertyValueChangedEventArgs)

        Dim ChildParent As FormFlowsheet = FlowSheet
        Dim sobj As Microsoft.Msdn.Samples.GraphicObjects.GraphicObject = ChildParent.FormSurface.FlowsheetDesignSurface.SelectedObject

        If Not sobj Is Nothing Then

            'connections
            If sobj.TipoObjeto = TipoObjeto.Cooler Or sobj.TipoObjeto = TipoObjeto.Pipe Or sobj.TipoObjeto = TipoObjeto.Expander Then
                If e.ChangedItem.Label.Equals(DWSIM.App.GetLocalString("Correntedeentrada")) Then
                    If e.ChangedItem.Value <> "" Then
                        If FormFlowsheet.SearchSurfaceObjectsByTag(e.ChangedItem.Value, ChildParent.FormSurface.FlowsheetDesignSurface) Is Nothing Then
                            Dim oguid As String = ChildParent.FormSurface.AddObjectToSurface(TipoObjeto.MaterialStream, sobj.X - 40, sobj.Y, e.ChangedItem.Value)
                        ElseIf CType(FormFlowsheet.SearchSurfaceObjectsByTag(e.ChangedItem.Value, ChildParent.FormSurface.FlowsheetDesignSurface), GraphicObject).OutputConnectors(0).IsAttached Then
                            MessageBox.Show(DWSIM.App.GetLocalString("Todasasconexespossve"), DWSIM.App.GetLocalString("Erro"), MessageBoxButtons.OK, MessageBoxIcon.Error)
                            Exit Sub
                        End If
                        If Not sobj.InputConnectors(0).IsAttached Then
                            ChildParent.ConnectObject(FormFlowsheet.SearchSurfaceObjectsByTag(e.ChangedItem.Value, ChildParent.FormSurface.FlowsheetDesignSurface), sobj)
                        Else
                            ChildParent.DisconnectObject(sobj.InputConnectors(0).AttachedConnector.AttachedFrom, sobj)
                            ChildParent.ConnectObject(FormFlowsheet.SearchSurfaceObjectsByTag(e.ChangedItem.Value, ChildParent.FormSurface.FlowsheetDesignSurface), sobj)
                        End If
                    Else
                        If e.OldValue.ToString <> "" Then
                            ChildParent.DisconnectObject(sobj.InputConnectors(0).AttachedConnector.AttachedFrom, sobj)
                        End If
                    End If
                ElseIf e.ChangedItem.Label.Equals(DWSIM.App.GetLocalString("Correntedesada")) Then
                    If e.ChangedItem.Value <> "" Then
                        If FormFlowsheet.SearchSurfaceObjectsByTag(e.ChangedItem.Value, ChildParent.FormSurface.FlowsheetDesignSurface) Is Nothing Then
                            Dim oguid As String = ChildParent.FormSurface.AddObjectToSurface(TipoObjeto.MaterialStream, sobj.X + sobj.Width + 40, sobj.Y, e.ChangedItem.Value)
                        ElseIf CType(FormFlowsheet.SearchSurfaceObjectsByTag(e.ChangedItem.Value, ChildParent.FormSurface.FlowsheetDesignSurface), GraphicObject).InputConnectors(0).IsAttached Then
                            MessageBox.Show(DWSIM.App.GetLocalString("Todasasconexespossve"), DWSIM.App.GetLocalString("Erro"), MessageBoxButtons.OK, MessageBoxIcon.Error)
                            Exit Sub
                        End If
                        If Not sobj.OutputConnectors(0).IsAttached Then
                            ChildParent.ConnectObject(sobj, FormFlowsheet.SearchSurfaceObjectsByTag(e.ChangedItem.Value, ChildParent.FormSurface.FlowsheetDesignSurface))
                        Else
                            ChildParent.DisconnectObject(sobj, sobj.OutputConnectors(0).AttachedConnector.AttachedTo)
                            ChildParent.ConnectObject(sobj, FormFlowsheet.SearchSurfaceObjectsByTag(e.ChangedItem.Value, ChildParent.FormSurface.FlowsheetDesignSurface))
                        End If
                    Else
                        If e.OldValue.ToString <> "" Then
                            ChildParent.DisconnectObject(sobj, sobj.OutputConnectors(0).AttachedConnector.AttachedTo)
                        End If
                    End If
                ElseIf e.ChangedItem.Label.Equals(DWSIM.App.GetLocalString("Correntedeenergia")) Then
                    If e.ChangedItem.Value <> "" Then
                        If FormFlowsheet.SearchSurfaceObjectsByTag(e.ChangedItem.Value, ChildParent.FormSurface.FlowsheetDesignSurface) Is Nothing Then
                            Dim oguid As String = ChildParent.FormSurface.AddObjectToSurface(TipoObjeto.EnergyStream, sobj.X + sobj.Width * 1.2 / 2, sobj.Y + sobj.Height + 20, e.ChangedItem.Value)
                        ElseIf CType(FormFlowsheet.SearchSurfaceObjectsByTag(e.ChangedItem.Value, ChildParent.FormSurface.FlowsheetDesignSurface), GraphicObject).InputConnectors(0).IsAttached Then
                            MessageBox.Show(DWSIM.App.GetLocalString("Todasasconexespossve"), DWSIM.App.GetLocalString("Erro"), MessageBoxButtons.OK, MessageBoxIcon.Error)
                            Exit Sub
                        End If
                        If Not sobj.EnergyConnector.IsAttached Then
                            ChildParent.ConnectObject(sobj, FormFlowsheet.SearchSurfaceObjectsByTag(e.ChangedItem.Value, ChildParent.FormSurface.FlowsheetDesignSurface))
                        Else
                            ChildParent.DisconnectObject(sobj, sobj.OutputConnectors(0).AttachedConnector.AttachedTo)
                            ChildParent.ConnectObject(sobj, FormFlowsheet.SearchSurfaceObjectsByTag(e.ChangedItem.Value, ChildParent.FormSurface.FlowsheetDesignSurface))
                        End If
                    Else
                        If e.OldValue.ToString <> "" Then
                            ChildParent.DisconnectObject(sobj, sobj.OutputConnectors(0).AttachedConnector.AttachedTo)
                        End If
                    End If
                End If
            ElseIf sobj.TipoObjeto = TipoObjeto.Compressor Or sobj.TipoObjeto = TipoObjeto.Heater Or sobj.TipoObjeto = TipoObjeto.Pump Or
                sobj.TipoObjeto = TipoObjeto.RCT_PFR Or sobj.TipoObjeto = TipoObjeto.RCT_CSTR Or sobj.TipoObjeto = TipoObjeto.ExcelUO Then
                If e.ChangedItem.Label.Equals(DWSIM.App.GetLocalString("Correntedeentrada")) Then
                    If e.ChangedItem.Value <> "" Then
                        If FormFlowsheet.SearchSurfaceObjectsByTag(e.ChangedItem.Value, ChildParent.FormSurface.FlowsheetDesignSurface) Is Nothing Then
                            Dim oguid As String = ChildParent.FormSurface.AddObjectToSurface(TipoObjeto.MaterialStream, sobj.X - 40, sobj.Y, e.ChangedItem.Value)
                        ElseIf CType(FormFlowsheet.SearchSurfaceObjectsByTag(e.ChangedItem.Value, ChildParent.FormSurface.FlowsheetDesignSurface), GraphicObject).OutputConnectors(0).IsAttached Then
                            MessageBox.Show(DWSIM.App.GetLocalString("Todasasconexespossve"), DWSIM.App.GetLocalString("Erro"), MessageBoxButtons.OK, MessageBoxIcon.Error)
                            Exit Sub
                        End If
                        If Not sobj.InputConnectors(0).IsAttached Then
                            ChildParent.ConnectObject(FormFlowsheet.SearchSurfaceObjectsByTag(e.ChangedItem.Value, ChildParent.FormSurface.FlowsheetDesignSurface), sobj)
                        Else
                            ChildParent.DisconnectObject(sobj.InputConnectors(0).AttachedConnector.AttachedFrom, sobj)
                            ChildParent.ConnectObject(FormFlowsheet.SearchSurfaceObjectsByTag(e.ChangedItem.Value, ChildParent.FormSurface.FlowsheetDesignSurface), sobj)
                        End If
                    Else
                        If e.OldValue.ToString <> "" Then
                            ChildParent.DisconnectObject(sobj.InputConnectors(0).AttachedConnector.AttachedFrom, sobj)
                        End If
                    End If
                ElseIf e.ChangedItem.Label.Equals(DWSIM.App.GetLocalString("Correntedesada")) Then
                    If e.ChangedItem.Value <> "" Then
                        If FormFlowsheet.SearchSurfaceObjectsByTag(e.ChangedItem.Value, ChildParent.FormSurface.FlowsheetDesignSurface) Is Nothing Then
                            Dim oguid As String = ChildParent.FormSurface.AddObjectToSurface(TipoObjeto.MaterialStream, sobj.X + sobj.Width + 40, sobj.Y, e.ChangedItem.Value)
                        ElseIf CType(FormFlowsheet.SearchSurfaceObjectsByTag(e.ChangedItem.Value, ChildParent.FormSurface.FlowsheetDesignSurface), GraphicObject).InputConnectors(0).IsAttached Then
                            MessageBox.Show(DWSIM.App.GetLocalString("Todasasconexespossve"), DWSIM.App.GetLocalString("Erro"), MessageBoxButtons.OK, MessageBoxIcon.Error)
                            Exit Sub
                        End If
                        If Not sobj.OutputConnectors(0).IsAttached Then
                            ChildParent.ConnectObject(sobj, FormFlowsheet.SearchSurfaceObjectsByTag(e.ChangedItem.Value, ChildParent.FormSurface.FlowsheetDesignSurface))
                        Else
                            ChildParent.DisconnectObject(sobj, sobj.OutputConnectors(0).AttachedConnector.AttachedTo)
                            ChildParent.ConnectObject(sobj, FormFlowsheet.SearchSurfaceObjectsByTag(e.ChangedItem.Value, ChildParent.FormSurface.FlowsheetDesignSurface))
                        End If
                    Else
                        If e.OldValue.ToString <> "" Then
                            ChildParent.DisconnectObject(sobj, sobj.OutputConnectors(0).AttachedConnector.AttachedTo)
                        End If
                    End If
                ElseIf e.ChangedItem.Label.Equals(DWSIM.App.GetLocalString("Correntedeenergia")) Then
                    If e.ChangedItem.Value <> "" Then
                        If FormFlowsheet.SearchSurfaceObjectsByTag(e.ChangedItem.Value, ChildParent.FormSurface.FlowsheetDesignSurface) Is Nothing Then
                            Dim oguid As String = ChildParent.FormSurface.AddObjectToSurface(TipoObjeto.EnergyStream, sobj.X - 20, sobj.Y + sobj.Height + 20, e.ChangedItem.Value)
                        ElseIf CType(FormFlowsheet.SearchSurfaceObjectsByTag(e.ChangedItem.Value, ChildParent.FormSurface.FlowsheetDesignSurface), GraphicObject).OutputConnectors(0).IsAttached Then
                            MessageBox.Show(DWSIM.App.GetLocalString("Todasasconexespossve"), DWSIM.App.GetLocalString("Erro"), MessageBoxButtons.OK, MessageBoxIcon.Error)
                            Exit Sub
                        End If
                        If Not sobj.InputConnectors(1).IsAttached Then
                            ChildParent.ConnectObject(FormFlowsheet.SearchSurfaceObjectsByTag(e.ChangedItem.Value, ChildParent.FormSurface.FlowsheetDesignSurface), sobj)
                        Else
                            ChildParent.DisconnectObject(sobj.InputConnectors(1).AttachedConnector.AttachedFrom, sobj)
                            ChildParent.ConnectObject(FormFlowsheet.SearchSurfaceObjectsByTag(e.ChangedItem.Value, ChildParent.FormSurface.FlowsheetDesignSurface), sobj)
                        End If
                    Else
                        If e.OldValue.ToString <> "" Then
                            ChildParent.DisconnectObject(sobj.InputConnectors(1).AttachedConnector.AttachedFrom, sobj)
                        End If
                    End If
                End If
            ElseIf sobj.TipoObjeto = TipoObjeto.Valve Or sobj.TipoObjeto = TipoObjeto.OrificePlate Or sobj.TipoObjeto = TipoObjeto.OT_Reciclo Or sobj.TipoObjeto = TipoObjeto.Tank Then
                If e.ChangedItem.Label.Equals(DWSIM.App.GetLocalString("Correntedeentrada")) Then
                    If e.ChangedItem.Value <> "" Then
                        If FormFlowsheet.SearchSurfaceObjectsByTag(e.ChangedItem.Value, ChildParent.FormSurface.FlowsheetDesignSurface) Is Nothing Then
                            Dim oguid As String = ChildParent.FormSurface.AddObjectToSurface(TipoObjeto.MaterialStream, sobj.X - 40, sobj.Y, e.ChangedItem.Value)
                        ElseIf CType(FormFlowsheet.SearchSurfaceObjectsByTag(e.ChangedItem.Value, ChildParent.FormSurface.FlowsheetDesignSurface), GraphicObject).OutputConnectors(0).IsAttached Then
                            MessageBox.Show(DWSIM.App.GetLocalString("Todasasconexespossve"), DWSIM.App.GetLocalString("Erro"), MessageBoxButtons.OK, MessageBoxIcon.Error)
                            Exit Sub
                        End If
                        If Not sobj.InputConnectors(0).IsAttached Then
                            ChildParent.ConnectObject(FormFlowsheet.SearchSurfaceObjectsByTag(e.ChangedItem.Value, ChildParent.FormSurface.FlowsheetDesignSurface), sobj)
                        Else
                            ChildParent.DisconnectObject(sobj.InputConnectors(0).AttachedConnector.AttachedFrom, sobj)
                            ChildParent.ConnectObject(FormFlowsheet.SearchSurfaceObjectsByTag(e.ChangedItem.Value, ChildParent.FormSurface.FlowsheetDesignSurface), sobj)
                        End If
                    Else
                        If e.OldValue.ToString <> "" Then
                            ChildParent.DisconnectObject(sobj.InputConnectors(0).AttachedConnector.AttachedFrom, sobj)
                        End If
                    End If
                ElseIf e.ChangedItem.Label.Equals(DWSIM.App.GetLocalString("Correntedesada")) Then
                    If e.ChangedItem.Value <> "" Then
                        If FormFlowsheet.SearchSurfaceObjectsByTag(e.ChangedItem.Value, ChildParent.FormSurface.FlowsheetDesignSurface) Is Nothing Then
                            Dim oguid As String = ChildParent.FormSurface.AddObjectToSurface(TipoObjeto.MaterialStream, sobj.X + sobj.Width + 40, sobj.Y, e.ChangedItem.Value)
                        ElseIf CType(FormFlowsheet.SearchSurfaceObjectsByTag(e.ChangedItem.Value, ChildParent.FormSurface.FlowsheetDesignSurface), GraphicObject).InputConnectors(0).IsAttached Then
                            MessageBox.Show(DWSIM.App.GetLocalString("Todasasconexespossve"), DWSIM.App.GetLocalString("Erro"), MessageBoxButtons.OK, MessageBoxIcon.Error)
                            Exit Sub
                        End If
                        If Not sobj.OutputConnectors(0).IsAttached Then
                            ChildParent.ConnectObject(sobj, FormFlowsheet.SearchSurfaceObjectsByTag(e.ChangedItem.Value, ChildParent.FormSurface.FlowsheetDesignSurface))
                        Else
                            ChildParent.DisconnectObject(sobj, sobj.OutputConnectors(0).AttachedConnector.AttachedTo)
                            ChildParent.ConnectObject(sobj, FormFlowsheet.SearchSurfaceObjectsByTag(e.ChangedItem.Value, ChildParent.FormSurface.FlowsheetDesignSurface))
                        End If
                    Else
                        If e.OldValue.ToString <> "" Then
                            ChildParent.DisconnectObject(sobj, sobj.OutputConnectors(0).AttachedConnector.AttachedTo)
                        End If
                    End If
                End If
            ElseIf sobj.TipoObjeto = TipoObjeto.Vessel Then
                If e.ChangedItem.Label.Equals(DWSIM.App.GetLocalString("Correntedeentrada1")) Then
                    If e.ChangedItem.Value <> "" Then
                        If FormFlowsheet.SearchSurfaceObjectsByTag(e.ChangedItem.Value, ChildParent.FormSurface.FlowsheetDesignSurface) Is Nothing Then
                            Dim oguid As String = ChildParent.FormSurface.AddObjectToSurface(TipoObjeto.MaterialStream, sobj.X - 40, sobj.Y - 20, e.ChangedItem.Value)
                        ElseIf CType(FormFlowsheet.SearchSurfaceObjectsByTag(e.ChangedItem.Value, ChildParent.FormSurface.FlowsheetDesignSurface), GraphicObject).OutputConnectors(0).IsAttached Then
                            MessageBox.Show(DWSIM.App.GetLocalString("Todasasconexespossve"), DWSIM.App.GetLocalString("Erro"), MessageBoxButtons.OK, MessageBoxIcon.Error)
                            Exit Sub
                        End If
                        If Not sobj.InputConnectors(0).IsAttached Then
                            ChildParent.ConnectObject(FormFlowsheet.SearchSurfaceObjectsByTag(e.ChangedItem.Value, ChildParent.FormSurface.FlowsheetDesignSurface), sobj, 0, 0)
                        Else
                            ChildParent.DisconnectObject(sobj.InputConnectors(0).AttachedConnector.AttachedFrom, sobj)
                            ChildParent.ConnectObject(FormFlowsheet.SearchSurfaceObjectsByTag(e.ChangedItem.Value, ChildParent.FormSurface.FlowsheetDesignSurface), sobj, 0, 0)
                        End If
                    Else
                        If e.OldValue.ToString <> "" Then
                            ChildParent.DisconnectObject(sobj.InputConnectors(0).AttachedConnector.AttachedFrom, sobj)
                        End If
                    End If
                ElseIf e.ChangedItem.Label.Equals(DWSIM.App.GetLocalString("Correntedeentrada2")) Then
                    If e.ChangedItem.Value <> "" Then
                        If FormFlowsheet.SearchSurfaceObjectsByTag(e.ChangedItem.Value, ChildParent.FormSurface.FlowsheetDesignSurface) Is Nothing Then
                            Dim oguid As String = ChildParent.FormSurface.AddObjectToSurface(TipoObjeto.MaterialStream, sobj.X - 40, sobj.Y, e.ChangedItem.Value)
                        ElseIf CType(FormFlowsheet.SearchSurfaceObjectsByTag(e.ChangedItem.Value, ChildParent.FormSurface.FlowsheetDesignSurface), GraphicObject).OutputConnectors(0).IsAttached Then
                            MessageBox.Show(DWSIM.App.GetLocalString("Todasasconexespossve"), DWSIM.App.GetLocalString("Erro"), MessageBoxButtons.OK, MessageBoxIcon.Error)
                            Exit Sub
                        End If
                        If Not sobj.InputConnectors(1).IsAttached Then
                            ChildParent.ConnectObject(FormFlowsheet.SearchSurfaceObjectsByTag(e.ChangedItem.Value, ChildParent.FormSurface.FlowsheetDesignSurface), sobj, 0, 1)
                        Else
                            ChildParent.DisconnectObject(sobj.InputConnectors(1).AttachedConnector.AttachedFrom, sobj)
                            ChildParent.ConnectObject(FormFlowsheet.SearchSurfaceObjectsByTag(e.ChangedItem.Value, ChildParent.FormSurface.FlowsheetDesignSurface), sobj, 0, 1)
                        End If
                    Else
                        If e.OldValue.ToString <> "" Then
                            ChildParent.DisconnectObject(sobj.InputConnectors(1).AttachedConnector.AttachedFrom, sobj)
                        End If
                    End If
                ElseIf e.ChangedItem.Label.Equals(DWSIM.App.GetLocalString("Correntedeentrada3")) Then
                    If e.ChangedItem.Value <> "" Then
                        If FormFlowsheet.SearchSurfaceObjectsByTag(e.ChangedItem.Value, ChildParent.FormSurface.FlowsheetDesignSurface) Is Nothing Then
                            Dim oguid As String = ChildParent.FormSurface.AddObjectToSurface(TipoObjeto.MaterialStream, sobj.X - 40, sobj.Y + 20, e.ChangedItem.Value)
                        ElseIf CType(FormFlowsheet.SearchSurfaceObjectsByTag(e.ChangedItem.Value, ChildParent.FormSurface.FlowsheetDesignSurface), GraphicObject).OutputConnectors(0).IsAttached Then
                            MessageBox.Show(DWSIM.App.GetLocalString("Todasasconexespossve"), DWSIM.App.GetLocalString("Erro"), MessageBoxButtons.OK, MessageBoxIcon.Error)
                            Exit Sub
                        End If
                        If Not sobj.InputConnectors(2).IsAttached Then
                            ChildParent.ConnectObject(FormFlowsheet.SearchSurfaceObjectsByTag(e.ChangedItem.Value, ChildParent.FormSurface.FlowsheetDesignSurface), sobj, 0, 2)
                        Else
                            ChildParent.DisconnectObject(sobj.InputConnectors(2).AttachedConnector.AttachedFrom, sobj)
                            ChildParent.ConnectObject(FormFlowsheet.SearchSurfaceObjectsByTag(e.ChangedItem.Value, ChildParent.FormSurface.FlowsheetDesignSurface), sobj, 0, 2)
                        End If
                    Else
                        If e.OldValue.ToString <> "" Then
                            ChildParent.DisconnectObject(sobj.InputConnectors(2).AttachedConnector.AttachedFrom, sobj)
                        End If
                    End If
                ElseIf e.ChangedItem.Label.Equals(DWSIM.App.GetLocalString("Correntedeentrada4")) Then
                    If e.ChangedItem.Value <> "" Then
                        If FormFlowsheet.SearchSurfaceObjectsByTag(e.ChangedItem.Value, ChildParent.FormSurface.FlowsheetDesignSurface) Is Nothing Then
                            Dim oguid As String = ChildParent.FormSurface.AddObjectToSurface(TipoObjeto.MaterialStream, sobj.X - 40, sobj.Y + 30, e.ChangedItem.Value)
                        ElseIf CType(FormFlowsheet.SearchSurfaceObjectsByTag(e.ChangedItem.Value, ChildParent.FormSurface.FlowsheetDesignSurface), GraphicObject).OutputConnectors(0).IsAttached Then
                            MessageBox.Show(DWSIM.App.GetLocalString("Todasasconexespossve"), DWSIM.App.GetLocalString("Erro"), MessageBoxButtons.OK, MessageBoxIcon.Error)
                            Exit Sub
                        End If
                        If Not sobj.InputConnectors(3).IsAttached Then
                            ChildParent.ConnectObject(FormFlowsheet.SearchSurfaceObjectsByTag(e.ChangedItem.Value, ChildParent.FormSurface.FlowsheetDesignSurface), sobj, 0, 3)
                        Else
                            ChildParent.DisconnectObject(sobj.InputConnectors(3).AttachedConnector.AttachedFrom, sobj)
                            ChildParent.ConnectObject(FormFlowsheet.SearchSurfaceObjectsByTag(e.ChangedItem.Value, ChildParent.FormSurface.FlowsheetDesignSurface), sobj, 0, 3)
                        End If
                    Else
                        If e.OldValue.ToString <> "" Then
                            ChildParent.DisconnectObject(sobj.InputConnectors(3).AttachedConnector.AttachedFrom, sobj)
                        End If
                    End If
                ElseIf e.ChangedItem.Label.Equals(DWSIM.App.GetLocalString("Correntedeentrada5")) Then
                    If e.ChangedItem.Value <> "" Then
                        If FormFlowsheet.SearchSurfaceObjectsByTag(e.ChangedItem.Value, ChildParent.FormSurface.FlowsheetDesignSurface) Is Nothing Then
                            Dim oguid As String = ChildParent.FormSurface.AddObjectToSurface(TipoObjeto.MaterialStream, sobj.X - 40, sobj.Y + 40, e.ChangedItem.Value)
                        ElseIf CType(FormFlowsheet.SearchSurfaceObjectsByTag(e.ChangedItem.Value, ChildParent.FormSurface.FlowsheetDesignSurface), GraphicObject).OutputConnectors(0).IsAttached Then
                            MessageBox.Show(DWSIM.App.GetLocalString("Todasasconexespossve"), DWSIM.App.GetLocalString("Erro"), MessageBoxButtons.OK, MessageBoxIcon.Error)
                            Exit Sub
                        End If
                        If Not sobj.InputConnectors(4).IsAttached Then
                            ChildParent.ConnectObject(FormFlowsheet.SearchSurfaceObjectsByTag(e.ChangedItem.Value, ChildParent.FormSurface.FlowsheetDesignSurface), sobj, 0, 4)
                        Else
                            ChildParent.DisconnectObject(sobj.InputConnectors(4).AttachedConnector.AttachedFrom, sobj)
                            ChildParent.ConnectObject(FormFlowsheet.SearchSurfaceObjectsByTag(e.ChangedItem.Value, ChildParent.FormSurface.FlowsheetDesignSurface), sobj, 0, 4)
                        End If
                    Else
                        If e.OldValue.ToString <> "" Then
                            ChildParent.DisconnectObject(sobj.InputConnectors(4).AttachedConnector.AttachedFrom, sobj)
                        End If
                    End If
                ElseIf e.ChangedItem.Label.Equals(DWSIM.App.GetLocalString("Correntedeentrada6")) Then
                    If e.ChangedItem.Value <> "" Then
                        If FormFlowsheet.SearchSurfaceObjectsByTag(e.ChangedItem.Value, ChildParent.FormSurface.FlowsheetDesignSurface) Is Nothing Then
                            Dim oguid As String = ChildParent.FormSurface.AddObjectToSurface(TipoObjeto.MaterialStream, sobj.X - 40, sobj.Y + 50, e.ChangedItem.Value)
                        ElseIf CType(FormFlowsheet.SearchSurfaceObjectsByTag(e.ChangedItem.Value, ChildParent.FormSurface.FlowsheetDesignSurface), GraphicObject).OutputConnectors(0).IsAttached Then
                            MessageBox.Show(DWSIM.App.GetLocalString("Todasasconexespossve"), DWSIM.App.GetLocalString("Erro"), MessageBoxButtons.OK, MessageBoxIcon.Error)
                            Exit Sub
                        End If
                        If Not sobj.InputConnectors(5).IsAttached Then
                            ChildParent.ConnectObject(FormFlowsheet.SearchSurfaceObjectsByTag(e.ChangedItem.Value, ChildParent.FormSurface.FlowsheetDesignSurface), sobj, 0, 5)
                        Else
                            ChildParent.DisconnectObject(sobj.InputConnectors(5).AttachedConnector.AttachedFrom, sobj)
                            ChildParent.ConnectObject(FormFlowsheet.SearchSurfaceObjectsByTag(e.ChangedItem.Value, ChildParent.FormSurface.FlowsheetDesignSurface), sobj, 0, 5)
                        End If
                    Else
                        If e.OldValue.ToString <> "" Then
                            ChildParent.DisconnectObject(sobj.InputConnectors(5).AttachedConnector.AttachedFrom, sobj)
                        End If
                    End If
                ElseIf e.ChangedItem.Label = (DWSIM.App.GetLocalString("Saidadevapor")) Then
                    If e.ChangedItem.Value <> "" Then
                        If FormFlowsheet.SearchSurfaceObjectsByTag(e.ChangedItem.Value, ChildParent.FormSurface.FlowsheetDesignSurface) Is Nothing Then
                            Dim oguid As String = ChildParent.FormSurface.AddObjectToSurface(TipoObjeto.MaterialStream, sobj.X + sobj.Width + 40, sobj.Y, e.ChangedItem.Value)
                        ElseIf CType(FormFlowsheet.SearchSurfaceObjectsByTag(e.ChangedItem.Value, ChildParent.FormSurface.FlowsheetDesignSurface), GraphicObject).InputConnectors(0).IsAttached Then
                            MessageBox.Show(DWSIM.App.GetLocalString("Todasasconexespossve"), DWSIM.App.GetLocalString("Erro"), MessageBoxButtons.OK, MessageBoxIcon.Error)
                            Exit Sub
                        End If
                        If Not sobj.OutputConnectors(0).IsAttached Then
                            ChildParent.ConnectObject(sobj, FormFlowsheet.SearchSurfaceObjectsByTag(e.ChangedItem.Value, ChildParent.FormSurface.FlowsheetDesignSurface), 0, 0)
                        Else
                            ChildParent.DisconnectObject(sobj, sobj.OutputConnectors(0).AttachedConnector.AttachedTo)
                            ChildParent.ConnectObject(sobj, FormFlowsheet.SearchSurfaceObjectsByTag(e.ChangedItem.Value, ChildParent.FormSurface.FlowsheetDesignSurface), 0, 0)
                        End If
                    Else
                        If e.OldValue.ToString <> "" Then
                            ChildParent.DisconnectObject(sobj, sobj.OutputConnectors(0).AttachedConnector.AttachedTo)
                        End If
                    End If
                ElseIf e.ChangedItem.Label = (DWSIM.App.GetLocalString("Saidadelquido")) Then
                    If e.ChangedItem.Value <> "" Then
                        If FormFlowsheet.SearchSurfaceObjectsByTag(e.ChangedItem.Value, ChildParent.FormSurface.FlowsheetDesignSurface) Is Nothing Then
                            Dim oguid As String = ChildParent.FormSurface.AddObjectToSurface(TipoObjeto.MaterialStream, sobj.X + sobj.Width + 40, sobj.Y + sobj.Height * 0.8, e.ChangedItem.Value)
                        ElseIf CType(FormFlowsheet.SearchSurfaceObjectsByTag(e.ChangedItem.Value, ChildParent.FormSurface.FlowsheetDesignSurface), GraphicObject).InputConnectors(0).IsAttached Then
                            MessageBox.Show(DWSIM.App.GetLocalString("Todasasconexespossve"), DWSIM.App.GetLocalString("Erro"), MessageBoxButtons.OK, MessageBoxIcon.Error)
                            Exit Sub
                        End If
                        If Not sobj.OutputConnectors(1).IsAttached Then
                            ChildParent.ConnectObject(sobj, FormFlowsheet.SearchSurfaceObjectsByTag(e.ChangedItem.Value, ChildParent.FormSurface.FlowsheetDesignSurface), 1, 0)
                        Else
                            ChildParent.DisconnectObject(sobj, sobj.OutputConnectors(0).AttachedConnector.AttachedTo)
                            ChildParent.ConnectObject(sobj, FormFlowsheet.SearchSurfaceObjectsByTag(e.ChangedItem.Value, ChildParent.FormSurface.FlowsheetDesignSurface), 1, 0)
                        End If
                    Else
                        If e.OldValue.ToString <> "" Then
                            ChildParent.DisconnectObject(sobj, sobj.OutputConnectors(0).AttachedConnector.AttachedTo)
                        End If
                    End If
                ElseIf e.ChangedItem.Label = (DWSIM.App.GetLocalString("Saidadelquido") & " (2)") Then
                    If e.ChangedItem.Value <> "" Then
                        If FormFlowsheet.SearchSurfaceObjectsByTag(e.ChangedItem.Value, ChildParent.FormSurface.FlowsheetDesignSurface) Is Nothing Then
                            Dim oguid As String = ChildParent.FormSurface.AddObjectToSurface(TipoObjeto.MaterialStream, sobj.X + sobj.Width + 40, sobj.Y + sobj.Height * 1.2, e.ChangedItem.Value)
                        ElseIf CType(FormFlowsheet.SearchSurfaceObjectsByTag(e.ChangedItem.Value, ChildParent.FormSurface.FlowsheetDesignSurface), GraphicObject).InputConnectors(0).IsAttached Then
                            MessageBox.Show(DWSIM.App.GetLocalString("Todasasconexespossve"), DWSIM.App.GetLocalString("Erro"), MessageBoxButtons.OK, MessageBoxIcon.Error)
                            Exit Sub
                        End If
                        If Not sobj.OutputConnectors(2).IsAttached Then
                            ChildParent.ConnectObject(sobj, FormFlowsheet.SearchSurfaceObjectsByTag(e.ChangedItem.Value, ChildParent.FormSurface.FlowsheetDesignSurface), 2, 0)
                        Else
                            ChildParent.DisconnectObject(sobj, sobj.OutputConnectors(0).AttachedConnector.AttachedTo)
                            ChildParent.ConnectObject(sobj, FormFlowsheet.SearchSurfaceObjectsByTag(e.ChangedItem.Value, ChildParent.FormSurface.FlowsheetDesignSurface), 2, 0)
                        End If
                    Else
                        If e.OldValue.ToString <> "" Then
                            ChildParent.DisconnectObject(sobj, sobj.OutputConnectors(0).AttachedConnector.AttachedTo)
                        End If
                    End If
                ElseIf e.ChangedItem.Label.Equals(DWSIM.App.GetLocalString("Correntedeenergia")) Then
                    If e.ChangedItem.Value <> "" Then
                        If FormFlowsheet.SearchSurfaceObjectsByTag(e.ChangedItem.Value, ChildParent.FormSurface.FlowsheetDesignSurface) Is Nothing Then
                            Dim oguid As String = ChildParent.FormSurface.AddObjectToSurface(TipoObjeto.EnergyStream, sobj.X - 20, sobj.Y + sobj.Height + 20, e.ChangedItem.Value)
                        ElseIf CType(FormFlowsheet.SearchSurfaceObjectsByTag(e.ChangedItem.Value, ChildParent.FormSurface.FlowsheetDesignSurface), GraphicObject).OutputConnectors(0).IsAttached Then
                            MessageBox.Show(DWSIM.App.GetLocalString("Todasasconexespossve"), DWSIM.App.GetLocalString("Erro"), MessageBoxButtons.OK, MessageBoxIcon.Error)
                            Exit Sub
                        End If
                        If Not sobj.InputConnectors(1).IsAttached Then
                            ChildParent.ConnectObject(FormFlowsheet.SearchSurfaceObjectsByTag(e.ChangedItem.Value, ChildParent.FormSurface.FlowsheetDesignSurface), sobj)
                        Else
                            ChildParent.DisconnectObject(sobj.InputConnectors(1).AttachedConnector.AttachedFrom, sobj)
                            ChildParent.ConnectObject(FormFlowsheet.SearchSurfaceObjectsByTag(e.ChangedItem.Value, ChildParent.FormSurface.FlowsheetDesignSurface), sobj)
                        End If
                    Else
                        If e.OldValue.ToString <> "" Then
                            ChildParent.DisconnectObject(sobj.InputConnectors(1).AttachedConnector.AttachedFrom, sobj)
                        End If
                    End If
                End If
            ElseIf sobj.TipoObjeto = TipoObjeto.RCT_Conversion Or sobj.TipoObjeto = TipoObjeto.RCT_Equilibrium Or sobj.TipoObjeto = TipoObjeto.RCT_Gibbs Then
                If e.ChangedItem.Label.Equals(DWSIM.App.GetLocalString("Correntedeentrada")) Then
                    If e.ChangedItem.Value <> "" Then
                        If FormFlowsheet.SearchSurfaceObjectsByTag(e.ChangedItem.Value, ChildParent.FormSurface.FlowsheetDesignSurface) Is Nothing Then
                            Dim oguid As String = ChildParent.FormSurface.AddObjectToSurface(TipoObjeto.MaterialStream, sobj.X - 40, sobj.Y, e.ChangedItem.Value)
                        ElseIf CType(FormFlowsheet.SearchSurfaceObjectsByTag(e.ChangedItem.Value, ChildParent.FormSurface.FlowsheetDesignSurface), GraphicObject).OutputConnectors(0).IsAttached Then
                            MessageBox.Show(DWSIM.App.GetLocalString("Todasasconexespossve"), DWSIM.App.GetLocalString("Erro"), MessageBoxButtons.OK, MessageBoxIcon.Error)
                            Exit Sub
                        End If
                        If Not sobj.InputConnectors(0).IsAttached Then
                            ChildParent.ConnectObject(FormFlowsheet.SearchSurfaceObjectsByTag(e.ChangedItem.Value, ChildParent.FormSurface.FlowsheetDesignSurface), sobj)
                        Else
                            ChildParent.DisconnectObject(sobj.InputConnectors(0).AttachedConnector.AttachedFrom, sobj)
                            ChildParent.ConnectObject(FormFlowsheet.SearchSurfaceObjectsByTag(e.ChangedItem.Value, ChildParent.FormSurface.FlowsheetDesignSurface), sobj)
                        End If
                    Else
                        If e.OldValue.ToString <> "" Then
                            ChildParent.DisconnectObject(sobj.InputConnectors(0).AttachedConnector.AttachedFrom, sobj)
                        End If
                    End If
                ElseIf e.ChangedItem.Label = (DWSIM.App.GetLocalString("Saidadevapor")) Then
                    If e.ChangedItem.Value <> "" Then
                        If FormFlowsheet.SearchSurfaceObjectsByTag(e.ChangedItem.Value, ChildParent.FormSurface.FlowsheetDesignSurface) Is Nothing Then
                            Dim oguid As String = ChildParent.FormSurface.AddObjectToSurface(TipoObjeto.MaterialStream, sobj.X + sobj.Width + 40, sobj.Y, e.ChangedItem.Value)
                        ElseIf CType(FormFlowsheet.SearchSurfaceObjectsByTag(e.ChangedItem.Value, ChildParent.FormSurface.FlowsheetDesignSurface), GraphicObject).InputConnectors(0).IsAttached Then
                            MessageBox.Show(DWSIM.App.GetLocalString("Todasasconexespossve"), DWSIM.App.GetLocalString("Erro"), MessageBoxButtons.OK, MessageBoxIcon.Error)
                            Exit Sub
                        End If
                        If Not sobj.OutputConnectors(0).IsAttached Then
                            ChildParent.ConnectObject(sobj, FormFlowsheet.SearchSurfaceObjectsByTag(e.ChangedItem.Value, ChildParent.FormSurface.FlowsheetDesignSurface), 0, 0)
                        Else
                            ChildParent.DisconnectObject(sobj, sobj.OutputConnectors(0).AttachedConnector.AttachedTo)
                            ChildParent.ConnectObject(sobj, FormFlowsheet.SearchSurfaceObjectsByTag(e.ChangedItem.Value, ChildParent.FormSurface.FlowsheetDesignSurface), 0, 0)
                        End If
                    Else
                        If e.OldValue.ToString <> "" Then
                            ChildParent.DisconnectObject(sobj, sobj.OutputConnectors(0).AttachedConnector.AttachedTo)
                        End If
                    End If
                ElseIf e.ChangedItem.Label = (DWSIM.App.GetLocalString("Saidadelquido")) Then
                    If e.ChangedItem.Value <> "" Then
                        If FormFlowsheet.SearchSurfaceObjectsByTag(e.ChangedItem.Value, ChildParent.FormSurface.FlowsheetDesignSurface) Is Nothing Then
                            Dim oguid As String = ChildParent.FormSurface.AddObjectToSurface(TipoObjeto.MaterialStream, sobj.X + sobj.Width + 40, sobj.Y + sobj.Height * 0.8, e.ChangedItem.Value)
                        ElseIf CType(FormFlowsheet.SearchSurfaceObjectsByTag(e.ChangedItem.Value, ChildParent.FormSurface.FlowsheetDesignSurface), GraphicObject).InputConnectors(0).IsAttached Then
                            MessageBox.Show(DWSIM.App.GetLocalString("Todasasconexespossve"), DWSIM.App.GetLocalString("Erro"), MessageBoxButtons.OK, MessageBoxIcon.Error)
                            Exit Sub
                        End If
                        If Not sobj.OutputConnectors(1).IsAttached Then
                            ChildParent.ConnectObject(sobj, FormFlowsheet.SearchSurfaceObjectsByTag(e.ChangedItem.Value, ChildParent.FormSurface.FlowsheetDesignSurface), 1, 0)
                        Else
                            ChildParent.DisconnectObject(sobj, sobj.OutputConnectors(0).AttachedConnector.AttachedTo)
                            ChildParent.ConnectObject(sobj, FormFlowsheet.SearchSurfaceObjectsByTag(e.ChangedItem.Value, ChildParent.FormSurface.FlowsheetDesignSurface), 1, 0)
                        End If
                    Else
                        If e.OldValue.ToString <> "" Then
                            ChildParent.DisconnectObject(sobj, sobj.OutputConnectors(0).AttachedConnector.AttachedTo)
                        End If
                    End If
                ElseIf e.ChangedItem.Label.Equals(DWSIM.App.GetLocalString("Correntedeenergia")) Then
                    If e.ChangedItem.Value <> "" Then
                        If FormFlowsheet.SearchSurfaceObjectsByTag(e.ChangedItem.Value, ChildParent.FormSurface.FlowsheetDesignSurface) Is Nothing Then
                            Dim oguid As String = ChildParent.FormSurface.AddObjectToSurface(TipoObjeto.EnergyStream, sobj.X - 20, sobj.Y + sobj.Height + 20, e.ChangedItem.Value)
                        ElseIf CType(FormFlowsheet.SearchSurfaceObjectsByTag(e.ChangedItem.Value, ChildParent.FormSurface.FlowsheetDesignSurface), GraphicObject).OutputConnectors(0).IsAttached Then
                            MessageBox.Show(DWSIM.App.GetLocalString("Todasasconexespossve"), DWSIM.App.GetLocalString("Erro"), MessageBoxButtons.OK, MessageBoxIcon.Error)
                            Exit Sub
                        End If
                        If Not sobj.InputConnectors(1).IsAttached Then
                            ChildParent.ConnectObject(FormFlowsheet.SearchSurfaceObjectsByTag(e.ChangedItem.Value, ChildParent.FormSurface.FlowsheetDesignSurface), sobj)
                        Else
                            ChildParent.DisconnectObject(sobj.InputConnectors(1).AttachedConnector.AttachedFrom, sobj)
                            ChildParent.ConnectObject(FormFlowsheet.SearchSurfaceObjectsByTag(e.ChangedItem.Value, ChildParent.FormSurface.FlowsheetDesignSurface), sobj)
                        End If
                    Else
                        If e.OldValue.ToString <> "" Then
                            ChildParent.DisconnectObject(sobj.InputConnectors(1).AttachedConnector.AttachedFrom, sobj)
                        End If
                    End If
                End If
            ElseIf sobj.TipoObjeto = TipoObjeto.NodeIn Then
                If e.ChangedItem.Label.Equals(DWSIM.App.GetLocalString("Correntedeentrada1")) Then
                    If e.ChangedItem.Value <> "" Then
                        If FormFlowsheet.SearchSurfaceObjectsByTag(e.ChangedItem.Value, ChildParent.FormSurface.FlowsheetDesignSurface) Is Nothing Then
                            Dim oguid As String = ChildParent.FormSurface.AddObjectToSurface(TipoObjeto.MaterialStream, sobj.X - 40, sobj.Y - 20, e.ChangedItem.Value)
                        ElseIf CType(FormFlowsheet.SearchSurfaceObjectsByTag(e.ChangedItem.Value, ChildParent.FormSurface.FlowsheetDesignSurface), GraphicObject).OutputConnectors(0).IsAttached Then
                            MessageBox.Show(DWSIM.App.GetLocalString("Todasasconexespossve"), DWSIM.App.GetLocalString("Erro"), MessageBoxButtons.OK, MessageBoxIcon.Error)
                            Exit Sub
                        End If
                        If Not sobj.InputConnectors(0).IsAttached Then
                            ChildParent.ConnectObject(FormFlowsheet.SearchSurfaceObjectsByTag(e.ChangedItem.Value, ChildParent.FormSurface.FlowsheetDesignSurface), sobj, 0, 0)
                        Else
                            ChildParent.DisconnectObject(sobj.InputConnectors(0).AttachedConnector.AttachedFrom, sobj)
                            ChildParent.ConnectObject(FormFlowsheet.SearchSurfaceObjectsByTag(e.ChangedItem.Value, ChildParent.FormSurface.FlowsheetDesignSurface), sobj, 0, 0)
                        End If
                    Else
                        If e.OldValue.ToString <> "" Then
                            ChildParent.DisconnectObject(sobj.InputConnectors(0).AttachedConnector.AttachedFrom, sobj)
                        End If
                    End If
                ElseIf e.ChangedItem.Label.Equals(DWSIM.App.GetLocalString("Correntedeentrada2")) Then
                    If e.ChangedItem.Value <> "" Then
                        If FormFlowsheet.SearchSurfaceObjectsByTag(e.ChangedItem.Value, ChildParent.FormSurface.FlowsheetDesignSurface) Is Nothing Then
                            Dim oguid As String = ChildParent.FormSurface.AddObjectToSurface(TipoObjeto.MaterialStream, sobj.X - 40, sobj.Y, e.ChangedItem.Value)
                        ElseIf CType(FormFlowsheet.SearchSurfaceObjectsByTag(e.ChangedItem.Value, ChildParent.FormSurface.FlowsheetDesignSurface), GraphicObject).OutputConnectors(0).IsAttached Then
                            MessageBox.Show(DWSIM.App.GetLocalString("Todasasconexespossve"), DWSIM.App.GetLocalString("Erro"), MessageBoxButtons.OK, MessageBoxIcon.Error)
                            Exit Sub
                        End If
                        If Not sobj.InputConnectors(1).IsAttached Then
                            ChildParent.ConnectObject(FormFlowsheet.SearchSurfaceObjectsByTag(e.ChangedItem.Value, ChildParent.FormSurface.FlowsheetDesignSurface), sobj, 0, 1)
                        Else
                            ChildParent.DisconnectObject(sobj.InputConnectors(1).AttachedConnector.AttachedFrom, sobj)
                            ChildParent.ConnectObject(FormFlowsheet.SearchSurfaceObjectsByTag(e.ChangedItem.Value, ChildParent.FormSurface.FlowsheetDesignSurface), sobj, 0, 1)
                        End If
                    Else
                        If e.OldValue.ToString <> "" Then
                            ChildParent.DisconnectObject(sobj.InputConnectors(1).AttachedConnector.AttachedFrom, sobj)
                        End If
                    End If
                ElseIf e.ChangedItem.Label.Equals(DWSIM.App.GetLocalString("Correntedeentrada3")) Then
                    If e.ChangedItem.Value <> "" Then
                        If FormFlowsheet.SearchSurfaceObjectsByTag(e.ChangedItem.Value, ChildParent.FormSurface.FlowsheetDesignSurface) Is Nothing Then
                            Dim oguid As String = ChildParent.FormSurface.AddObjectToSurface(TipoObjeto.MaterialStream, sobj.X - 40, sobj.Y + 20, e.ChangedItem.Value)
                        ElseIf CType(FormFlowsheet.SearchSurfaceObjectsByTag(e.ChangedItem.Value, ChildParent.FormSurface.FlowsheetDesignSurface), GraphicObject).OutputConnectors(0).IsAttached Then
                            MessageBox.Show(DWSIM.App.GetLocalString("Todasasconexespossve"), DWSIM.App.GetLocalString("Erro"), MessageBoxButtons.OK, MessageBoxIcon.Error)
                            Exit Sub
                        End If
                        If Not sobj.InputConnectors(2).IsAttached Then
                            ChildParent.ConnectObject(FormFlowsheet.SearchSurfaceObjectsByTag(e.ChangedItem.Value, ChildParent.FormSurface.FlowsheetDesignSurface), sobj, 0, 2)
                        Else
                            ChildParent.DisconnectObject(sobj.InputConnectors(2).AttachedConnector.AttachedFrom, sobj)
                            ChildParent.ConnectObject(FormFlowsheet.SearchSurfaceObjectsByTag(e.ChangedItem.Value, ChildParent.FormSurface.FlowsheetDesignSurface), sobj, 0, 2)
                        End If
                    Else
                        If e.OldValue.ToString <> "" Then
                            ChildParent.DisconnectObject(sobj.InputConnectors(2).AttachedConnector.AttachedFrom, sobj)
                        End If
                    End If
                ElseIf e.ChangedItem.Label.Equals(DWSIM.App.GetLocalString("Correntedeentrada4")) Then
                    If e.ChangedItem.Value <> "" Then
                        If FormFlowsheet.SearchSurfaceObjectsByTag(e.ChangedItem.Value, ChildParent.FormSurface.FlowsheetDesignSurface) Is Nothing Then
                            Dim oguid As String = ChildParent.FormSurface.AddObjectToSurface(TipoObjeto.MaterialStream, sobj.X - 40, sobj.Y + 30, e.ChangedItem.Value)
                        ElseIf CType(FormFlowsheet.SearchSurfaceObjectsByTag(e.ChangedItem.Value, ChildParent.FormSurface.FlowsheetDesignSurface), GraphicObject).OutputConnectors(0).IsAttached Then
                            MessageBox.Show(DWSIM.App.GetLocalString("Todasasconexespossve"), DWSIM.App.GetLocalString("Erro"), MessageBoxButtons.OK, MessageBoxIcon.Error)
                            Exit Sub
                        End If
                        If Not sobj.InputConnectors(3).IsAttached Then
                            ChildParent.ConnectObject(FormFlowsheet.SearchSurfaceObjectsByTag(e.ChangedItem.Value, ChildParent.FormSurface.FlowsheetDesignSurface), sobj, 0, 3)
                        Else
                            ChildParent.DisconnectObject(sobj.InputConnectors(3).AttachedConnector.AttachedFrom, sobj)
                            ChildParent.ConnectObject(FormFlowsheet.SearchSurfaceObjectsByTag(e.ChangedItem.Value, ChildParent.FormSurface.FlowsheetDesignSurface), sobj, 0, 3)
                        End If
                    Else
                        If e.OldValue.ToString <> "" Then
                            ChildParent.DisconnectObject(sobj.InputConnectors(3).AttachedConnector.AttachedFrom, sobj)
                        End If
                    End If
                ElseIf e.ChangedItem.Label.Equals(DWSIM.App.GetLocalString("Correntedeentrada5")) Then
                    If e.ChangedItem.Value <> "" Then
                        If FormFlowsheet.SearchSurfaceObjectsByTag(e.ChangedItem.Value, ChildParent.FormSurface.FlowsheetDesignSurface) Is Nothing Then
                            Dim oguid As String = ChildParent.FormSurface.AddObjectToSurface(TipoObjeto.MaterialStream, sobj.X - 40, sobj.Y + 40, e.ChangedItem.Value)
                        ElseIf CType(FormFlowsheet.SearchSurfaceObjectsByTag(e.ChangedItem.Value, ChildParent.FormSurface.FlowsheetDesignSurface), GraphicObject).OutputConnectors(0).IsAttached Then
                            MessageBox.Show(DWSIM.App.GetLocalString("Todasasconexespossve"), DWSIM.App.GetLocalString("Erro"), MessageBoxButtons.OK, MessageBoxIcon.Error)
                            Exit Sub
                        End If
                        If Not sobj.InputConnectors(4).IsAttached Then
                            ChildParent.ConnectObject(FormFlowsheet.SearchSurfaceObjectsByTag(e.ChangedItem.Value, ChildParent.FormSurface.FlowsheetDesignSurface), sobj, 0, 4)
                        Else
                            ChildParent.DisconnectObject(sobj.InputConnectors(4).AttachedConnector.AttachedFrom, sobj)
                            ChildParent.ConnectObject(FormFlowsheet.SearchSurfaceObjectsByTag(e.ChangedItem.Value, ChildParent.FormSurface.FlowsheetDesignSurface), sobj, 0, 4)
                        End If
                    Else
                        If e.OldValue.ToString <> "" Then
                            ChildParent.DisconnectObject(sobj.InputConnectors(4).AttachedConnector.AttachedFrom, sobj)
                        End If
                    End If
                ElseIf e.ChangedItem.Label.Equals(DWSIM.App.GetLocalString("Correntedeentrada6")) Then
                    If e.ChangedItem.Value <> "" Then
                        If FormFlowsheet.SearchSurfaceObjectsByTag(e.ChangedItem.Value, ChildParent.FormSurface.FlowsheetDesignSurface) Is Nothing Then
                            Dim oguid As String = ChildParent.FormSurface.AddObjectToSurface(TipoObjeto.MaterialStream, sobj.X - 40, sobj.Y + 50, e.ChangedItem.Value)
                        ElseIf CType(FormFlowsheet.SearchSurfaceObjectsByTag(e.ChangedItem.Value, ChildParent.FormSurface.FlowsheetDesignSurface), GraphicObject).OutputConnectors(0).IsAttached Then
                            MessageBox.Show(DWSIM.App.GetLocalString("Todasasconexespossve"), DWSIM.App.GetLocalString("Erro"), MessageBoxButtons.OK, MessageBoxIcon.Error)
                            Exit Sub
                        End If
                        If Not sobj.InputConnectors(5).IsAttached Then
                            ChildParent.ConnectObject(FormFlowsheet.SearchSurfaceObjectsByTag(e.ChangedItem.Value, ChildParent.FormSurface.FlowsheetDesignSurface), sobj, 0, 5)
                        Else
                            ChildParent.DisconnectObject(sobj.InputConnectors(5).AttachedConnector.AttachedFrom, sobj)
                            ChildParent.ConnectObject(FormFlowsheet.SearchSurfaceObjectsByTag(e.ChangedItem.Value, ChildParent.FormSurface.FlowsheetDesignSurface), sobj, 0, 5)
                        End If
                    Else
                        If e.OldValue.ToString <> "" Then
                            ChildParent.DisconnectObject(sobj.InputConnectors(5).AttachedConnector.AttachedFrom, sobj)
                        End If
                    End If
                ElseIf e.ChangedItem.Label.Equals(DWSIM.App.GetLocalString("Conectadoasada")) Then
                    If e.ChangedItem.Value <> "" Then
                        If FormFlowsheet.SearchSurfaceObjectsByTag(e.ChangedItem.Value, ChildParent.FormSurface.FlowsheetDesignSurface) Is Nothing Then
                            Dim oguid As String = ChildParent.FormSurface.AddObjectToSurface(TipoObjeto.MaterialStream, sobj.X + sobj.Width + 40, sobj.Y, e.ChangedItem.Value)
                        ElseIf CType(FormFlowsheet.SearchSurfaceObjectsByTag(e.ChangedItem.Value, ChildParent.FormSurface.FlowsheetDesignSurface), GraphicObject).InputConnectors(0).IsAttached Then
                            MessageBox.Show(DWSIM.App.GetLocalString("Todasasconexespossve"), DWSIM.App.GetLocalString("Erro"), MessageBoxButtons.OK, MessageBoxIcon.Error)
                            Exit Sub
                        End If
                        If Not sobj.OutputConnectors(0).IsAttached Then
                            ChildParent.ConnectObject(sobj, FormFlowsheet.SearchSurfaceObjectsByTag(e.ChangedItem.Value, ChildParent.FormSurface.FlowsheetDesignSurface))
                        Else
                            ChildParent.DisconnectObject(sobj, sobj.OutputConnectors(0).AttachedConnector.AttachedTo)
                            ChildParent.ConnectObject(sobj, FormFlowsheet.SearchSurfaceObjectsByTag(e.ChangedItem.Value, ChildParent.FormSurface.FlowsheetDesignSurface))
                        End If
                    Else
                        If e.OldValue.ToString <> "" Then
                            ChildParent.DisconnectObject(sobj, sobj.OutputConnectors(0).AttachedConnector.AttachedTo)
                        End If
                    End If
                End If
            ElseIf sobj.TipoObjeto = TipoObjeto.NodeOut Then
                If e.ChangedItem.Label.Equals(DWSIM.App.GetLocalString("Correntedeentrada")) Then
                    If e.ChangedItem.Value <> "" Then
                        If FormFlowsheet.SearchSurfaceObjectsByTag(e.ChangedItem.Value, ChildParent.FormSurface.FlowsheetDesignSurface) Is Nothing Then
                            Dim oguid As String = ChildParent.FormSurface.AddObjectToSurface(TipoObjeto.MaterialStream, sobj.X - 40, sobj.Y, e.ChangedItem.Value)
                        ElseIf CType(FormFlowsheet.SearchSurfaceObjectsByTag(e.ChangedItem.Value, ChildParent.FormSurface.FlowsheetDesignSurface), GraphicObject).OutputConnectors(0).IsAttached Then
                            MessageBox.Show(DWSIM.App.GetLocalString("Todasasconexespossve"), DWSIM.App.GetLocalString("Erro"), MessageBoxButtons.OK, MessageBoxIcon.Error)
                            Exit Sub
                        End If
                        If Not sobj.InputConnectors(0).IsAttached Then
                            ChildParent.ConnectObject(FormFlowsheet.SearchSurfaceObjectsByTag(e.ChangedItem.Value, ChildParent.FormSurface.FlowsheetDesignSurface), sobj)
                        Else
                            ChildParent.DisconnectObject(sobj.InputConnectors(0).AttachedConnector.AttachedFrom, sobj)
                            ChildParent.ConnectObject(FormFlowsheet.SearchSurfaceObjectsByTag(e.ChangedItem.Value, ChildParent.FormSurface.FlowsheetDesignSurface), sobj)
                        End If
                    Else
                        If e.OldValue.ToString <> "" Then
                            ChildParent.DisconnectObject(sobj.InputConnectors(0).AttachedConnector.AttachedFrom, sobj)
                        End If
                    End If
                ElseIf e.ChangedItem.Label.Equals(DWSIM.App.GetLocalString("Correntedesaida1")) Then
                    If e.ChangedItem.Value <> "" Then
                        If FormFlowsheet.SearchSurfaceObjectsByTag(e.ChangedItem.Value, ChildParent.FormSurface.FlowsheetDesignSurface) Is Nothing Then
                            Dim oguid As String = ChildParent.FormSurface.AddObjectToSurface(TipoObjeto.MaterialStream, sobj.X + sobj.Width + 40, sobj.Y + 20, e.ChangedItem.Value)
                        ElseIf CType(FormFlowsheet.SearchSurfaceObjectsByTag(e.ChangedItem.Value, ChildParent.FormSurface.FlowsheetDesignSurface), GraphicObject).InputConnectors(0).IsAttached Then
                            MessageBox.Show(DWSIM.App.GetLocalString("Todasasconexespossve"), DWSIM.App.GetLocalString("Erro"), MessageBoxButtons.OK, MessageBoxIcon.Error)
                            Exit Sub
                        End If
                        If Not sobj.OutputConnectors(0).IsAttached Then
                            ChildParent.ConnectObject(sobj, FormFlowsheet.SearchSurfaceObjectsByTag(e.ChangedItem.Value, ChildParent.FormSurface.FlowsheetDesignSurface), 0, 0)
                        Else
                            ChildParent.DisconnectObject(sobj, sobj.OutputConnectors(0).AttachedConnector.AttachedTo)
                            ChildParent.ConnectObject(sobj, FormFlowsheet.SearchSurfaceObjectsByTag(e.ChangedItem.Value, ChildParent.FormSurface.FlowsheetDesignSurface), 0, 0)
                        End If
                    Else
                        If e.OldValue.ToString <> "" Then
                            ChildParent.DisconnectObject(sobj, sobj.OutputConnectors(0).AttachedConnector.AttachedTo)
                        End If
                    End If
                ElseIf e.ChangedItem.Label.Equals(DWSIM.App.GetLocalString("Correntedesaida2")) Then
                    If e.ChangedItem.Value <> "" Then
                        If FormFlowsheet.SearchSurfaceObjectsByTag(e.ChangedItem.Value, ChildParent.FormSurface.FlowsheetDesignSurface) Is Nothing Then
                            Dim oguid As String = ChildParent.FormSurface.AddObjectToSurface(TipoObjeto.MaterialStream, sobj.X + sobj.Width + 40, sobj.Y, e.ChangedItem.Value)
                        ElseIf CType(FormFlowsheet.SearchSurfaceObjectsByTag(e.ChangedItem.Value, ChildParent.FormSurface.FlowsheetDesignSurface), GraphicObject).InputConnectors(0).IsAttached Then
                            MessageBox.Show(DWSIM.App.GetLocalString("Todasasconexespossve"), DWSIM.App.GetLocalString("Erro"), MessageBoxButtons.OK, MessageBoxIcon.Error)
                            Exit Sub
                        End If
                        If Not sobj.OutputConnectors(1).IsAttached Then
                            ChildParent.ConnectObject(sobj, FormFlowsheet.SearchSurfaceObjectsByTag(e.ChangedItem.Value, ChildParent.FormSurface.FlowsheetDesignSurface), 1, 0)
                        Else
                            ChildParent.DisconnectObject(sobj, sobj.OutputConnectors(1).AttachedConnector.AttachedTo)
                            ChildParent.ConnectObject(sobj, FormFlowsheet.SearchSurfaceObjectsByTag(e.ChangedItem.Value, ChildParent.FormSurface.FlowsheetDesignSurface), 1, 0)
                        End If
                    Else
                        If e.OldValue.ToString <> "" Then
                            ChildParent.DisconnectObject(sobj, sobj.OutputConnectors(1).AttachedConnector.AttachedTo)
                        End If
                    End If
                ElseIf e.ChangedItem.Label.Equals(DWSIM.App.GetLocalString("Correntedesaida3")) Then
                    If e.ChangedItem.Value <> "" Then
                        If FormFlowsheet.SearchSurfaceObjectsByTag(e.ChangedItem.Value, ChildParent.FormSurface.FlowsheetDesignSurface) Is Nothing Then
                            Dim oguid As String = ChildParent.FormSurface.AddObjectToSurface(TipoObjeto.MaterialStream, sobj.X + sobj.Width + 40, sobj.Y - 20, e.ChangedItem.Value)
                        ElseIf CType(FormFlowsheet.SearchSurfaceObjectsByTag(e.ChangedItem.Value, ChildParent.FormSurface.FlowsheetDesignSurface), GraphicObject).InputConnectors(0).IsAttached Then
                            MessageBox.Show(DWSIM.App.GetLocalString("Todasasconexespossve"), DWSIM.App.GetLocalString("Erro"), MessageBoxButtons.OK, MessageBoxIcon.Error)
                            Exit Sub
                        End If
                        If Not sobj.OutputConnectors(2).IsAttached Then
                            ChildParent.ConnectObject(sobj, FormFlowsheet.SearchSurfaceObjectsByTag(e.ChangedItem.Value, ChildParent.FormSurface.FlowsheetDesignSurface), 2, 0)
                        Else
                            ChildParent.DisconnectObject(sobj, sobj.OutputConnectors(2).AttachedConnector.AttachedTo)
                            ChildParent.ConnectObject(sobj, FormFlowsheet.SearchSurfaceObjectsByTag(e.ChangedItem.Value, ChildParent.FormSurface.FlowsheetDesignSurface), 2, 0)
                        End If
                    Else
                        If e.OldValue.ToString <> "" Then
                            ChildParent.DisconnectObject(sobj, sobj.OutputConnectors(2).AttachedConnector.AttachedTo)
                        End If
                    End If
                End If
            ElseIf sobj.TipoObjeto = TipoObjeto.ShortcutColumn Then
                If e.ChangedItem.Label.Equals(DWSIM.App.GetLocalString("SCFeed")) Then
                    If e.ChangedItem.Value <> "" Then
                        If FormFlowsheet.SearchSurfaceObjectsByTag(e.ChangedItem.Value, ChildParent.FormSurface.FlowsheetDesignSurface) Is Nothing Then
                            Dim oguid As String = ChildParent.FormSurface.AddObjectToSurface(TipoObjeto.MaterialStream, sobj.X - 40, sobj.Y, e.ChangedItem.Value)
                        ElseIf CType(FormFlowsheet.SearchSurfaceObjectsByTag(e.ChangedItem.Value, ChildParent.FormSurface.FlowsheetDesignSurface), GraphicObject).OutputConnectors(0).IsAttached Then
                            MessageBox.Show(DWSIM.App.GetLocalString("Todasasconexespossve"), DWSIM.App.GetLocalString("Erro"), MessageBoxButtons.OK, MessageBoxIcon.Error)
                            Exit Sub
                        End If
                        If Not sobj.InputConnectors(0).IsAttached Then
                            ChildParent.ConnectObject(FormFlowsheet.SearchSurfaceObjectsByTag(e.ChangedItem.Value, ChildParent.FormSurface.FlowsheetDesignSurface), sobj)
                        Else
                            ChildParent.DisconnectObject(sobj.InputConnectors(0).AttachedConnector.AttachedFrom, sobj)
                            ChildParent.ConnectObject(FormFlowsheet.SearchSurfaceObjectsByTag(e.ChangedItem.Value, ChildParent.FormSurface.FlowsheetDesignSurface), sobj)
                        End If
                    Else
                        If e.OldValue.ToString <> "" Then
                            ChildParent.DisconnectObject(sobj.InputConnectors(0).AttachedConnector.AttachedFrom, sobj)
                        End If
                    End If
                ElseIf e.ChangedItem.Label.Equals(DWSIM.App.GetLocalString("SCReboilerDuty")) Then
                    If e.ChangedItem.Value <> "" Then
                        If FormFlowsheet.SearchSurfaceObjectsByTag(e.ChangedItem.Value, ChildParent.FormSurface.FlowsheetDesignSurface) Is Nothing Then
                            Dim oguid As String = ChildParent.FormSurface.AddObjectToSurface(TipoObjeto.EnergyStream, sobj.X + sobj.Width + 30, sobj.Y + sobj.Height - 30, e.ChangedItem.Value)
                        ElseIf CType(FormFlowsheet.SearchSurfaceObjectsByTag(e.ChangedItem.Value, ChildParent.FormSurface.FlowsheetDesignSurface), GraphicObject).OutputConnectors(0).IsAttached Then
                            MessageBox.Show(DWSIM.App.GetLocalString("Todasasconexespossve"), DWSIM.App.GetLocalString("Erro"), MessageBoxButtons.OK, MessageBoxIcon.Error)
                            Exit Sub
                        End If
                        If Not sobj.InputConnectors(1).IsAttached Then
                            ChildParent.ConnectObject(FormFlowsheet.SearchSurfaceObjectsByTag(e.ChangedItem.Value, ChildParent.FormSurface.FlowsheetDesignSurface), sobj, 0, 1)
                        Else
                            ChildParent.DisconnectObject(sobj.InputConnectors(1).AttachedConnector.AttachedFrom, sobj)
                            ChildParent.ConnectObject(FormFlowsheet.SearchSurfaceObjectsByTag(e.ChangedItem.Value, ChildParent.FormSurface.FlowsheetDesignSurface), sobj, 0, 1)
                        End If
                    Else
                        If e.OldValue.ToString <> "" Then
                            ChildParent.DisconnectObject(sobj.InputConnectors(1).AttachedConnector.AttachedFrom, sobj)
                        End If
                    End If
                ElseIf e.ChangedItem.Label.Equals(DWSIM.App.GetLocalString("SCDistillate")) Then
                    If e.ChangedItem.Value <> "" Then
                        If FormFlowsheet.SearchSurfaceObjectsByTag(e.ChangedItem.Value, ChildParent.FormSurface.FlowsheetDesignSurface) Is Nothing Then
                            Dim oguid As String = ChildParent.FormSurface.AddObjectToSurface(TipoObjeto.MaterialStream, sobj.X + sobj.Width + 40, sobj.Y + 40, e.ChangedItem.Value)
                        ElseIf CType(FormFlowsheet.SearchSurfaceObjectsByTag(e.ChangedItem.Value, ChildParent.FormSurface.FlowsheetDesignSurface), GraphicObject).InputConnectors(0).IsAttached Then
                            MessageBox.Show(DWSIM.App.GetLocalString("Todasasconexespossve"), DWSIM.App.GetLocalString("Erro"), MessageBoxButtons.OK, MessageBoxIcon.Error)
                            Exit Sub
                        End If
                        If Not sobj.OutputConnectors(0).IsAttached Then
                            ChildParent.ConnectObject(sobj, FormFlowsheet.SearchSurfaceObjectsByTag(e.ChangedItem.Value, ChildParent.FormSurface.FlowsheetDesignSurface), 0, 0)
                        Else
                            ChildParent.DisconnectObject(sobj, sobj.OutputConnectors(0).AttachedConnector.AttachedTo)
                            ChildParent.ConnectObject(sobj, FormFlowsheet.SearchSurfaceObjectsByTag(e.ChangedItem.Value, ChildParent.FormSurface.FlowsheetDesignSurface), 0, 0)
                        End If
                    Else
                        If e.OldValue.ToString <> "" Then
                            ChildParent.DisconnectObject(sobj, sobj.OutputConnectors(0).AttachedConnector.AttachedTo)
                        End If
                    End If
                ElseIf e.ChangedItem.Label.Equals(DWSIM.App.GetLocalString("SCBottoms")) Then
                    If e.ChangedItem.Value <> "" Then
                        If FormFlowsheet.SearchSurfaceObjectsByTag(e.ChangedItem.Value, ChildParent.FormSurface.FlowsheetDesignSurface) Is Nothing Then
                            Dim oguid As String = ChildParent.FormSurface.AddObjectToSurface(TipoObjeto.MaterialStream, sobj.X + sobj.Width + 40, sobj.Y + sobj.Height, e.ChangedItem.Value)
                        ElseIf CType(FormFlowsheet.SearchSurfaceObjectsByTag(e.ChangedItem.Value, ChildParent.FormSurface.FlowsheetDesignSurface), GraphicObject).InputConnectors(0).IsAttached Then
                            MessageBox.Show(DWSIM.App.GetLocalString("Todasasconexespossve"), DWSIM.App.GetLocalString("Erro"), MessageBoxButtons.OK, MessageBoxIcon.Error)
                            Exit Sub
                        End If
                        If Not sobj.OutputConnectors(1).IsAttached Then
                            ChildParent.ConnectObject(sobj, FormFlowsheet.SearchSurfaceObjectsByTag(e.ChangedItem.Value, ChildParent.FormSurface.FlowsheetDesignSurface), 1, 0)
                        Else
                            ChildParent.DisconnectObject(sobj, sobj.OutputConnectors(1).AttachedConnector.AttachedTo)
                            ChildParent.ConnectObject(sobj, FormFlowsheet.SearchSurfaceObjectsByTag(e.ChangedItem.Value, ChildParent.FormSurface.FlowsheetDesignSurface), 1, 0)
                        End If
                    Else
                        If e.OldValue.ToString <> "" Then
                            ChildParent.DisconnectObject(sobj, sobj.OutputConnectors(1).AttachedConnector.AttachedTo)
                        End If
                    End If
                ElseIf e.ChangedItem.Label.Equals(DWSIM.App.GetLocalString("SCCondenserDuty")) Then
                    If e.ChangedItem.Value <> "" Then
                        If FormFlowsheet.SearchSurfaceObjectsByTag(e.ChangedItem.Value, ChildParent.FormSurface.FlowsheetDesignSurface) Is Nothing Then
                            Dim oguid As String = ChildParent.FormSurface.AddObjectToSurface(TipoObjeto.EnergyStream, sobj.X + sobj.Width + 30, sobj.Y + 30, e.ChangedItem.Value)
                        ElseIf CType(FormFlowsheet.SearchSurfaceObjectsByTag(e.ChangedItem.Value, ChildParent.FormSurface.FlowsheetDesignSurface), GraphicObject).InputConnectors(0).IsAttached Then
                            MessageBox.Show(DWSIM.App.GetLocalString("Todasasconexespossve"), DWSIM.App.GetLocalString("Erro"), MessageBoxButtons.OK, MessageBoxIcon.Error)
                            Exit Sub
                        End If
                        If Not sobj.EnergyConnector.IsAttached Then
                            ChildParent.ConnectObject(sobj, FormFlowsheet.SearchSurfaceObjectsByTag(e.ChangedItem.Value, ChildParent.FormSurface.FlowsheetDesignSurface))
                        Else
                            ChildParent.DisconnectObject(sobj, sobj.EnergyConnector.AttachedConnector.AttachedTo)
                            ChildParent.ConnectObject(sobj, FormFlowsheet.SearchSurfaceObjectsByTag(e.ChangedItem.Value, ChildParent.FormSurface.FlowsheetDesignSurface))
                        End If
                    Else
                        If e.OldValue.ToString <> "" Then
                            ChildParent.DisconnectObject(sobj, sobj.EnergyConnector.AttachedConnector.AttachedTo)
                        End If
                    End If
                End If
            ElseIf sobj.TipoObjeto = TipoObjeto.HeatExchanger Then
                If e.ChangedItem.Label.Equals(DWSIM.App.GetLocalString("Correntedeentrada1")) Then
                    If e.ChangedItem.Value <> "" Then
                        If FormFlowsheet.SearchSurfaceObjectsByTag(e.ChangedItem.Value, ChildParent.FormSurface.FlowsheetDesignSurface) Is Nothing Then
                            Dim oguid As String = ChildParent.FormSurface.AddObjectToSurface(TipoObjeto.MaterialStream, sobj.X - 40, sobj.Y, e.ChangedItem.Value)
                        ElseIf CType(FormFlowsheet.SearchSurfaceObjectsByTag(e.ChangedItem.Value, ChildParent.FormSurface.FlowsheetDesignSurface), GraphicObject).OutputConnectors(0).IsAttached Then
                            MessageBox.Show(DWSIM.App.GetLocalString("Todasasconexespossve"), DWSIM.App.GetLocalString("Erro"), MessageBoxButtons.OK, MessageBoxIcon.Error)
                            Exit Sub
                        End If
                        If Not sobj.InputConnectors(0).IsAttached Then
                            ChildParent.ConnectObject(FormFlowsheet.SearchSurfaceObjectsByTag(e.ChangedItem.Value, ChildParent.FormSurface.FlowsheetDesignSurface), sobj)
                        Else
                            ChildParent.DisconnectObject(sobj.InputConnectors(0).AttachedConnector.AttachedFrom, sobj)
                            ChildParent.ConnectObject(FormFlowsheet.SearchSurfaceObjectsByTag(e.ChangedItem.Value, ChildParent.FormSurface.FlowsheetDesignSurface), sobj)
                        End If
                    Else
                        If e.OldValue.ToString <> "" Then
                            ChildParent.DisconnectObject(sobj.InputConnectors(0).AttachedConnector.AttachedFrom, sobj)
                        End If
                    End If
                ElseIf e.ChangedItem.Label.Equals(DWSIM.App.GetLocalString("Correntedeentrada2")) Then
                    If e.ChangedItem.Value <> "" Then
                        If FormFlowsheet.SearchSurfaceObjectsByTag(e.ChangedItem.Value, ChildParent.FormSurface.FlowsheetDesignSurface) Is Nothing Then
                            Dim oguid As String = ChildParent.FormSurface.AddObjectToSurface(TipoObjeto.MaterialStream, sobj.X - 40, sobj.Y, e.ChangedItem.Value)
                        ElseIf CType(FormFlowsheet.SearchSurfaceObjectsByTag(e.ChangedItem.Value, ChildParent.FormSurface.FlowsheetDesignSurface), GraphicObject).OutputConnectors(0).IsAttached Then
                            MessageBox.Show(DWSIM.App.GetLocalString("Todasasconexespossve"), DWSIM.App.GetLocalString("Erro"), MessageBoxButtons.OK, MessageBoxIcon.Error)
                            Exit Sub
                        End If
                        If Not sobj.InputConnectors(1).IsAttached Then
                            ChildParent.ConnectObject(FormFlowsheet.SearchSurfaceObjectsByTag(e.ChangedItem.Value, ChildParent.FormSurface.FlowsheetDesignSurface), sobj)
                        Else
                            ChildParent.DisconnectObject(sobj.InputConnectors(1).AttachedConnector.AttachedFrom, sobj)
                            ChildParent.ConnectObject(FormFlowsheet.SearchSurfaceObjectsByTag(e.ChangedItem.Value, ChildParent.FormSurface.FlowsheetDesignSurface), sobj)
                        End If
                    Else
                        If e.OldValue.ToString <> "" Then
                            ChildParent.DisconnectObject(sobj.InputConnectors(1).AttachedConnector.AttachedFrom, sobj)
                        End If
                    End If
                ElseIf e.ChangedItem.Label.Equals(DWSIM.App.GetLocalString("Correntedesaida1")) Then
                    If e.ChangedItem.Value <> "" Then
                        If FormFlowsheet.SearchSurfaceObjectsByTag(e.ChangedItem.Value, ChildParent.FormSurface.FlowsheetDesignSurface) Is Nothing Then
                            Dim oguid As String = ChildParent.FormSurface.AddObjectToSurface(TipoObjeto.MaterialStream, sobj.X + sobj.Width + 40, sobj.Y + 20, e.ChangedItem.Value)
                        ElseIf CType(FormFlowsheet.SearchSurfaceObjectsByTag(e.ChangedItem.Value, ChildParent.FormSurface.FlowsheetDesignSurface), GraphicObject).InputConnectors(0).IsAttached Then
                            MessageBox.Show(DWSIM.App.GetLocalString("Todasasconexespossve"), DWSIM.App.GetLocalString("Erro"), MessageBoxButtons.OK, MessageBoxIcon.Error)
                            Exit Sub
                        End If
                        If Not sobj.OutputConnectors(0).IsAttached Then
                            ChildParent.ConnectObject(sobj, FormFlowsheet.SearchSurfaceObjectsByTag(e.ChangedItem.Value, ChildParent.FormSurface.FlowsheetDesignSurface), 0, 0)
                        Else
                            ChildParent.DisconnectObject(sobj, sobj.OutputConnectors(0).AttachedConnector.AttachedTo)
                            ChildParent.ConnectObject(sobj, FormFlowsheet.SearchSurfaceObjectsByTag(e.ChangedItem.Value, ChildParent.FormSurface.FlowsheetDesignSurface), 0, 0)
                        End If
                    Else
                        If e.OldValue.ToString <> "" Then
                            ChildParent.DisconnectObject(sobj, sobj.OutputConnectors(0).AttachedConnector.AttachedTo)
                        End If
                    End If
                ElseIf e.ChangedItem.Label.Equals(DWSIM.App.GetLocalString("Correntedesaida2")) Then
                    If e.ChangedItem.Value <> "" Then
                        If FormFlowsheet.SearchSurfaceObjectsByTag(e.ChangedItem.Value, ChildParent.FormSurface.FlowsheetDesignSurface) Is Nothing Then
                            Dim oguid As String = ChildParent.FormSurface.AddObjectToSurface(TipoObjeto.MaterialStream, sobj.X + sobj.Width + 40, sobj.Y, e.ChangedItem.Value)
                        ElseIf CType(FormFlowsheet.SearchSurfaceObjectsByTag(e.ChangedItem.Value, ChildParent.FormSurface.FlowsheetDesignSurface), GraphicObject).InputConnectors(0).IsAttached Then
                            MessageBox.Show(DWSIM.App.GetLocalString("Todasasconexespossve"), DWSIM.App.GetLocalString("Erro"), MessageBoxButtons.OK, MessageBoxIcon.Error)
                            Exit Sub
                        End If
                        If Not sobj.OutputConnectors(1).IsAttached Then
                            ChildParent.ConnectObject(sobj, FormFlowsheet.SearchSurfaceObjectsByTag(e.ChangedItem.Value, ChildParent.FormSurface.FlowsheetDesignSurface), 1, 0)
                        Else
                            ChildParent.DisconnectObject(sobj, sobj.OutputConnectors(1).AttachedConnector.AttachedTo)
                            ChildParent.ConnectObject(sobj, FormFlowsheet.SearchSurfaceObjectsByTag(e.ChangedItem.Value, ChildParent.FormSurface.FlowsheetDesignSurface), 1, 0)
                        End If
                    Else
                        If e.OldValue.ToString <> "" Then
                            ChildParent.DisconnectObject(sobj, sobj.OutputConnectors(1).AttachedConnector.AttachedTo)
                        End If
                    End If
                End If
            ElseIf sobj.TipoObjeto = TipoObjeto.OT_EnergyRecycle Then
                If e.ChangedItem.Label.Equals(DWSIM.App.GetLocalString("Correntedeentrada")) Then
                    If e.ChangedItem.Value <> "" Then
                        If FormFlowsheet.SearchSurfaceObjectsByTag(e.ChangedItem.Value, ChildParent.FormSurface.FlowsheetDesignSurface) Is Nothing Then
                            Dim oguid As String = ChildParent.FormSurface.AddObjectToSurface(TipoObjeto.EnergyStream, sobj.X - 40, sobj.Y, e.ChangedItem.Value)
                        ElseIf CType(FormFlowsheet.SearchSurfaceObjectsByTag(e.ChangedItem.Value, ChildParent.FormSurface.FlowsheetDesignSurface), GraphicObject).OutputConnectors(0).IsAttached Then
                            MessageBox.Show(DWSIM.App.GetLocalString("Todasasconexespossve"), DWSIM.App.GetLocalString("Erro"), MessageBoxButtons.OK, MessageBoxIcon.Error)
                            Exit Sub
                        End If
                        If Not sobj.InputConnectors(0).IsAttached Then
                            ChildParent.ConnectObject(FormFlowsheet.SearchSurfaceObjectsByTag(e.ChangedItem.Value, ChildParent.FormSurface.FlowsheetDesignSurface), sobj)
                        Else
                            ChildParent.DisconnectObject(sobj.InputConnectors(0).AttachedConnector.AttachedFrom, sobj)
                            ChildParent.ConnectObject(FormFlowsheet.SearchSurfaceObjectsByTag(e.ChangedItem.Value, ChildParent.FormSurface.FlowsheetDesignSurface), sobj)
                        End If
                    Else
                        If e.OldValue.ToString <> "" Then
                            ChildParent.DisconnectObject(sobj.InputConnectors(0).AttachedConnector.AttachedFrom, sobj)
                        End If
                    End If
                ElseIf e.ChangedItem.Label.Equals(DWSIM.App.GetLocalString("Correntedesada")) Then
                    If e.ChangedItem.Value <> "" Then
                        If FormFlowsheet.SearchSurfaceObjectsByTag(e.ChangedItem.Value, ChildParent.FormSurface.FlowsheetDesignSurface) Is Nothing Then
                            Dim oguid As String = ChildParent.FormSurface.AddObjectToSurface(TipoObjeto.EnergyStream, sobj.X + sobj.Width + 40, sobj.Y, e.ChangedItem.Value)
                        ElseIf CType(FormFlowsheet.SearchSurfaceObjectsByTag(e.ChangedItem.Value, ChildParent.FormSurface.FlowsheetDesignSurface), GraphicObject).InputConnectors(0).IsAttached Then
                            MessageBox.Show(DWSIM.App.GetLocalString("Todasasconexespossve"), DWSIM.App.GetLocalString("Erro"), MessageBoxButtons.OK, MessageBoxIcon.Error)
                            Exit Sub
                        End If
                        If Not sobj.OutputConnectors(0).IsAttached Then
                            ChildParent.ConnectObject(sobj, FormFlowsheet.SearchSurfaceObjectsByTag(e.ChangedItem.Value, ChildParent.FormSurface.FlowsheetDesignSurface))
                        Else
                            ChildParent.DisconnectObject(sobj, sobj.OutputConnectors(0).AttachedConnector.AttachedTo)
                            ChildParent.ConnectObject(sobj, FormFlowsheet.SearchSurfaceObjectsByTag(e.ChangedItem.Value, ChildParent.FormSurface.FlowsheetDesignSurface))
                        End If
                    Else
                        If e.OldValue.ToString <> "" Then
                            ChildParent.DisconnectObject(sobj, sobj.OutputConnectors(0).AttachedConnector.AttachedTo)
                        End If
                    End If
                End If
            ElseIf sobj.TipoObjeto = TipoObjeto.ComponentSeparator Or sobj.TipoObjeto = TipoObjeto.SolidSeparator Or sobj.TipoObjeto = TipoObjeto.Filter Then
                If e.ChangedItem.Label.Equals(DWSIM.App.GetLocalString("Correntedeentrada")) Then
                    If e.ChangedItem.Value <> "" Then
                        If FormFlowsheet.SearchSurfaceObjectsByTag(e.ChangedItem.Value, ChildParent.FormSurface.FlowsheetDesignSurface) Is Nothing Then
                            Dim oguid As String = ChildParent.FormSurface.AddObjectToSurface(TipoObjeto.MaterialStream, sobj.X - 40, sobj.Y, e.ChangedItem.Value)
                        ElseIf CType(FormFlowsheet.SearchSurfaceObjectsByTag(e.ChangedItem.Value, ChildParent.FormSurface.FlowsheetDesignSurface), GraphicObject).OutputConnectors(0).IsAttached Then
                            MessageBox.Show(DWSIM.App.GetLocalString("Todasasconexespossve"), DWSIM.App.GetLocalString("Erro"), MessageBoxButtons.OK, MessageBoxIcon.Error)
                            Exit Sub
                        End If
                        If Not sobj.InputConnectors(0).IsAttached Then
                            ChildParent.ConnectObject(FormFlowsheet.SearchSurfaceObjectsByTag(e.ChangedItem.Value, ChildParent.FormSurface.FlowsheetDesignSurface), sobj)
                        Else
                            ChildParent.DisconnectObject(sobj.InputConnectors(0).AttachedConnector.AttachedFrom, sobj)
                            ChildParent.ConnectObject(FormFlowsheet.SearchSurfaceObjectsByTag(e.ChangedItem.Value, ChildParent.FormSurface.FlowsheetDesignSurface), sobj)
                        End If
                    Else
                        If e.OldValue.ToString <> "" Then
                            ChildParent.DisconnectObject(sobj.InputConnectors(0).AttachedConnector.AttachedFrom, sobj)
                        End If
                    End If
                ElseIf e.ChangedItem.Label.Equals(DWSIM.App.GetLocalString("OutletStream1")) Then
                    If e.ChangedItem.Value <> "" Then
                        If FormFlowsheet.SearchSurfaceObjectsByTag(e.ChangedItem.Value, ChildParent.FormSurface.FlowsheetDesignSurface) Is Nothing Then
                            Dim oguid As String = ChildParent.FormSurface.AddObjectToSurface(TipoObjeto.MaterialStream, sobj.X + sobj.Width + 40, sobj.Y, e.ChangedItem.Value)
                        ElseIf CType(FormFlowsheet.SearchSurfaceObjectsByTag(e.ChangedItem.Value, ChildParent.FormSurface.FlowsheetDesignSurface), GraphicObject).InputConnectors(0).IsAttached Then
                            MessageBox.Show(DWSIM.App.GetLocalString("Todasasconexespossve"), DWSIM.App.GetLocalString("Erro"), MessageBoxButtons.OK, MessageBoxIcon.Error)
                            Exit Sub
                        End If
                        If Not sobj.OutputConnectors(0).IsAttached Then
                            ChildParent.ConnectObject(sobj, FormFlowsheet.SearchSurfaceObjectsByTag(e.ChangedItem.Value, ChildParent.FormSurface.FlowsheetDesignSurface), 0, 0)
                        Else
                            ChildParent.DisconnectObject(sobj, sobj.OutputConnectors(0).AttachedConnector.AttachedTo)
                            ChildParent.ConnectObject(sobj, FormFlowsheet.SearchSurfaceObjectsByTag(e.ChangedItem.Value, ChildParent.FormSurface.FlowsheetDesignSurface), 0, 0)
                        End If
                    Else
                        If e.OldValue.ToString <> "" Then
                            ChildParent.DisconnectObject(sobj, sobj.OutputConnectors(0).AttachedConnector.AttachedTo)
                        End If
                    End If
                ElseIf e.ChangedItem.Label.Equals(DWSIM.App.GetLocalString("OutletStream2")) Then
                    If e.ChangedItem.Value <> "" Then
                        If FormFlowsheet.SearchSurfaceObjectsByTag(e.ChangedItem.Value, ChildParent.FormSurface.FlowsheetDesignSurface) Is Nothing Then
                            Dim oguid As String = ChildParent.FormSurface.AddObjectToSurface(TipoObjeto.MaterialStream, sobj.X + sobj.Width + 40, sobj.Y + sobj.Height * 0.8, e.ChangedItem.Value)
                        ElseIf CType(FormFlowsheet.SearchSurfaceObjectsByTag(e.ChangedItem.Value, ChildParent.FormSurface.FlowsheetDesignSurface), GraphicObject).InputConnectors(0).IsAttached Then
                            MessageBox.Show(DWSIM.App.GetLocalString("Todasasconexespossve"), DWSIM.App.GetLocalString("Erro"), MessageBoxButtons.OK, MessageBoxIcon.Error)
                            Exit Sub
                        End If
                        If Not sobj.OutputConnectors(1).IsAttached Then
                            ChildParent.ConnectObject(sobj, FormFlowsheet.SearchSurfaceObjectsByTag(e.ChangedItem.Value, ChildParent.FormSurface.FlowsheetDesignSurface), 1, 0)
                        Else
                            ChildParent.DisconnectObject(sobj, sobj.OutputConnectors(0).AttachedConnector.AttachedTo)
                            ChildParent.ConnectObject(sobj, FormFlowsheet.SearchSurfaceObjectsByTag(e.ChangedItem.Value, ChildParent.FormSurface.FlowsheetDesignSurface), 1, 0)
                        End If
                    Else
                        If e.OldValue.ToString <> "" Then
                            ChildParent.DisconnectObject(sobj, sobj.OutputConnectors(0).AttachedConnector.AttachedTo)
                        End If
                    End If
                ElseIf e.ChangedItem.Label.Equals(DWSIM.App.GetLocalString("Correntedeenergia")) Then
                    If e.ChangedItem.Value <> "" Then
                        If FormFlowsheet.SearchSurfaceObjectsByTag(e.ChangedItem.Value, ChildParent.FormSurface.FlowsheetDesignSurface) Is Nothing Then
                            Dim oguid As String = ChildParent.FormSurface.AddObjectToSurface(TipoObjeto.EnergyStream, sobj.X - 20, sobj.Y + sobj.Height + 20, e.ChangedItem.Value)
                        ElseIf CType(FormFlowsheet.SearchSurfaceObjectsByTag(e.ChangedItem.Value, ChildParent.FormSurface.FlowsheetDesignSurface), GraphicObject).InputConnectors(0).IsAttached Then
                            MessageBox.Show(DWSIM.App.GetLocalString("Todasasconexespossve"), DWSIM.App.GetLocalString("Erro"), MessageBoxButtons.OK, MessageBoxIcon.Error)
                            Exit Sub
                        End If
                        If Not sobj.EnergyConnector.IsAttached Then
                            ChildParent.ConnectObject(sobj, FormFlowsheet.SearchSurfaceObjectsByTag(e.ChangedItem.Value, ChildParent.FormSurface.FlowsheetDesignSurface))
                        Else
                            ChildParent.DisconnectObject(sobj, sobj.EnergyConnector.AttachedConnector.AttachedTo)
                            ChildParent.ConnectObject(sobj, FormFlowsheet.SearchSurfaceObjectsByTag(e.ChangedItem.Value, ChildParent.FormSurface.FlowsheetDesignSurface))
                        End If
                    Else
                        If e.OldValue.ToString <> "" Then
                            ChildParent.DisconnectObject(sobj, sobj.EnergyConnector.AttachedConnector.AttachedTo)
                        End If
                    End If
                End If
            ElseIf sobj.TipoObjeto = TipoObjeto.CustomUO Then
                If e.ChangedItem.Label.Equals(DWSIM.App.GetLocalString("Correntedeentrada1")) Then
                    If e.ChangedItem.Value <> "" Then
                        If FormFlowsheet.SearchSurfaceObjectsByTag(e.ChangedItem.Value, ChildParent.FormSurface.FlowsheetDesignSurface) Is Nothing Then
                            Dim oguid As String = ChildParent.FormSurface.AddObjectToSurface(TipoObjeto.MaterialStream, sobj.X - 40, sobj.Y - 20, e.ChangedItem.Value)
                        ElseIf CType(FormFlowsheet.SearchSurfaceObjectsByTag(e.ChangedItem.Value, ChildParent.FormSurface.FlowsheetDesignSurface), GraphicObject).OutputConnectors(0).IsAttached Then
                            MessageBox.Show(DWSIM.App.GetLocalString("Todasasconexespossve"), DWSIM.App.GetLocalString("Erro"), MessageBoxButtons.OK, MessageBoxIcon.Error)
                            Exit Sub
                        End If
                        If Not sobj.InputConnectors(0).IsAttached Then
                            ChildParent.ConnectObject(FormFlowsheet.SearchSurfaceObjectsByTag(e.ChangedItem.Value, ChildParent.FormSurface.FlowsheetDesignSurface), sobj, 0, 0)
                        Else
                            ChildParent.DisconnectObject(sobj.InputConnectors(0).AttachedConnector.AttachedFrom, sobj)
                            ChildParent.ConnectObject(FormFlowsheet.SearchSurfaceObjectsByTag(e.ChangedItem.Value, ChildParent.FormSurface.FlowsheetDesignSurface), sobj, 0, 0)
                        End If
                    Else
                        If e.OldValue.ToString <> "" Then
                            ChildParent.DisconnectObject(sobj.InputConnectors(0).AttachedConnector.AttachedFrom, sobj)
                        End If
                    End If
                ElseIf e.ChangedItem.Label.Equals(DWSIM.App.GetLocalString("Correntedeentrada2")) Then
                    If e.ChangedItem.Value <> "" Then
                        If FormFlowsheet.SearchSurfaceObjectsByTag(e.ChangedItem.Value, ChildParent.FormSurface.FlowsheetDesignSurface) Is Nothing Then
                            Dim oguid As String = ChildParent.FormSurface.AddObjectToSurface(TipoObjeto.MaterialStream, sobj.X - 40, sobj.Y, e.ChangedItem.Value)
                        ElseIf CType(FormFlowsheet.SearchSurfaceObjectsByTag(e.ChangedItem.Value, ChildParent.FormSurface.FlowsheetDesignSurface), GraphicObject).OutputConnectors(0).IsAttached Then
                            MessageBox.Show(DWSIM.App.GetLocalString("Todasasconexespossve"), DWSIM.App.GetLocalString("Erro"), MessageBoxButtons.OK, MessageBoxIcon.Error)
                            Exit Sub
                        End If
                        If Not sobj.InputConnectors(1).IsAttached Then
                            ChildParent.ConnectObject(FormFlowsheet.SearchSurfaceObjectsByTag(e.ChangedItem.Value, ChildParent.FormSurface.FlowsheetDesignSurface), sobj, 0, 1)
                        Else
                            ChildParent.DisconnectObject(sobj.InputConnectors(1).AttachedConnector.AttachedFrom, sobj)
                            ChildParent.ConnectObject(FormFlowsheet.SearchSurfaceObjectsByTag(e.ChangedItem.Value, ChildParent.FormSurface.FlowsheetDesignSurface), sobj, 0, 1)
                        End If
                    Else
                        If e.OldValue.ToString <> "" Then
                            ChildParent.DisconnectObject(sobj.InputConnectors(1).AttachedConnector.AttachedFrom, sobj)
                        End If
                    End If
                ElseIf e.ChangedItem.Label.Equals(DWSIM.App.GetLocalString("Correntedeentrada3")) Then
                    If e.ChangedItem.Value <> "" Then
                        If FormFlowsheet.SearchSurfaceObjectsByTag(e.ChangedItem.Value, ChildParent.FormSurface.FlowsheetDesignSurface) Is Nothing Then
                            Dim oguid As String = ChildParent.FormSurface.AddObjectToSurface(TipoObjeto.MaterialStream, sobj.X - 40, sobj.Y + 20, e.ChangedItem.Value)
                        ElseIf CType(FormFlowsheet.SearchSurfaceObjectsByTag(e.ChangedItem.Value, ChildParent.FormSurface.FlowsheetDesignSurface), GraphicObject).OutputConnectors(0).IsAttached Then
                            MessageBox.Show(DWSIM.App.GetLocalString("Todasasconexespossve"), DWSIM.App.GetLocalString("Erro"), MessageBoxButtons.OK, MessageBoxIcon.Error)
                            Exit Sub
                        End If
                        If Not sobj.InputConnectors(2).IsAttached Then
                            ChildParent.ConnectObject(FormFlowsheet.SearchSurfaceObjectsByTag(e.ChangedItem.Value, ChildParent.FormSurface.FlowsheetDesignSurface), sobj, 0, 2)
                        Else
                            ChildParent.DisconnectObject(sobj.InputConnectors(2).AttachedConnector.AttachedFrom, sobj)
                            ChildParent.ConnectObject(FormFlowsheet.SearchSurfaceObjectsByTag(e.ChangedItem.Value, ChildParent.FormSurface.FlowsheetDesignSurface), sobj, 0, 2)
                        End If
                    Else
                        If e.OldValue.ToString <> "" Then
                            ChildParent.DisconnectObject(sobj.InputConnectors(2).AttachedConnector.AttachedFrom, sobj)
                        End If
                    End If
                ElseIf e.ChangedItem.Label.Equals(DWSIM.App.GetLocalString("CorrentedeenergiaE")) Then
                    If e.ChangedItem.Value <> "" Then
                        If FormFlowsheet.SearchSurfaceObjectsByTag(e.ChangedItem.Value, ChildParent.FormSurface.FlowsheetDesignSurface) Is Nothing Then
                            Dim oguid As String = ChildParent.FormSurface.AddObjectToSurface(TipoObjeto.EnergyStream, sobj.X - sobj.Width * 1.5, sobj.Y - sobj.Height - 40, e.ChangedItem.Value)
                        ElseIf CType(FormFlowsheet.SearchSurfaceObjectsByTag(e.ChangedItem.Value, ChildParent.FormSurface.FlowsheetDesignSurface), GraphicObject).OutputConnectors(0).IsAttached Then
                            MessageBox.Show(DWSIM.App.GetLocalString("Todasasconexespossve"), DWSIM.App.GetLocalString("Erro"), MessageBoxButtons.OK, MessageBoxIcon.Error)
                            Exit Sub
                        End If
                        If Not sobj.InputConnectors(3).IsAttached Then
                            ChildParent.ConnectObject(FormFlowsheet.SearchSurfaceObjectsByTag(e.ChangedItem.Value, ChildParent.FormSurface.FlowsheetDesignSurface), sobj)
                        Else
                            ChildParent.DisconnectObject(sobj.InputConnectors(3).AttachedConnector.AttachedFrom, sobj)
                            ChildParent.ConnectObject(FormFlowsheet.SearchSurfaceObjectsByTag(e.ChangedItem.Value, ChildParent.FormSurface.FlowsheetDesignSurface), sobj)
                        End If
                    Else
                        If e.OldValue.ToString <> "" Then
                            ChildParent.DisconnectObject(sobj.InputConnectors(3).AttachedConnector.AttachedFrom, sobj)
                        End If
                    End If
                ElseIf e.ChangedItem.Label.Equals(DWSIM.App.GetLocalString("Correntedesaida1")) Then
                    If e.ChangedItem.Value <> "" Then
                        If FormFlowsheet.SearchSurfaceObjectsByTag(e.ChangedItem.Value, ChildParent.FormSurface.FlowsheetDesignSurface) Is Nothing Then
                            Dim oguid As String = ChildParent.FormSurface.AddObjectToSurface(TipoObjeto.MaterialStream, sobj.X + sobj.Width + 40, sobj.Y + 20, e.ChangedItem.Value)
                        ElseIf CType(FormFlowsheet.SearchSurfaceObjectsByTag(e.ChangedItem.Value, ChildParent.FormSurface.FlowsheetDesignSurface), GraphicObject).InputConnectors(0).IsAttached Then
                            MessageBox.Show(DWSIM.App.GetLocalString("Todasasconexespossve"), DWSIM.App.GetLocalString("Erro"), MessageBoxButtons.OK, MessageBoxIcon.Error)
                            Exit Sub
                        End If
                        If Not sobj.OutputConnectors(0).IsAttached Then
                            ChildParent.ConnectObject(sobj, FormFlowsheet.SearchSurfaceObjectsByTag(e.ChangedItem.Value, ChildParent.FormSurface.FlowsheetDesignSurface), 0, 0)
                        Else
                            ChildParent.DisconnectObject(sobj, sobj.OutputConnectors(0).AttachedConnector.AttachedTo)
                            ChildParent.ConnectObject(sobj, FormFlowsheet.SearchSurfaceObjectsByTag(e.ChangedItem.Value, ChildParent.FormSurface.FlowsheetDesignSurface), 0, 0)
                        End If
                    Else
                        If e.OldValue.ToString <> "" Then
                            ChildParent.DisconnectObject(sobj, sobj.OutputConnectors(0).AttachedConnector.AttachedTo)
                        End If
                    End If
                ElseIf e.ChangedItem.Label.Equals(DWSIM.App.GetLocalString("Correntedesaida2")) Then
                    If e.ChangedItem.Value <> "" Then
                        If FormFlowsheet.SearchSurfaceObjectsByTag(e.ChangedItem.Value, ChildParent.FormSurface.FlowsheetDesignSurface) Is Nothing Then
                            Dim oguid As String = ChildParent.FormSurface.AddObjectToSurface(TipoObjeto.MaterialStream, sobj.X + sobj.Width + 40, sobj.Y, e.ChangedItem.Value)
                        ElseIf CType(FormFlowsheet.SearchSurfaceObjectsByTag(e.ChangedItem.Value, ChildParent.FormSurface.FlowsheetDesignSurface), GraphicObject).InputConnectors(0).IsAttached Then
                            MessageBox.Show(DWSIM.App.GetLocalString("Todasasconexespossve"), DWSIM.App.GetLocalString("Erro"), MessageBoxButtons.OK, MessageBoxIcon.Error)
                            Exit Sub
                        End If
                        If Not sobj.OutputConnectors(1).IsAttached Then
                            ChildParent.ConnectObject(sobj, FormFlowsheet.SearchSurfaceObjectsByTag(e.ChangedItem.Value, ChildParent.FormSurface.FlowsheetDesignSurface), 1, 0)
                        Else
                            ChildParent.DisconnectObject(sobj, sobj.OutputConnectors(1).AttachedConnector.AttachedTo)
                            ChildParent.ConnectObject(sobj, FormFlowsheet.SearchSurfaceObjectsByTag(e.ChangedItem.Value, ChildParent.FormSurface.FlowsheetDesignSurface), 1, 0)
                        End If
                    Else
                        If e.OldValue.ToString <> "" Then
                            ChildParent.DisconnectObject(sobj, sobj.OutputConnectors(1).AttachedConnector.AttachedTo)
                        End If
                    End If
                ElseIf e.ChangedItem.Label.Equals(DWSIM.App.GetLocalString("Correntedesaida3")) Then
                    If e.ChangedItem.Value <> "" Then
                        If FormFlowsheet.SearchSurfaceObjectsByTag(e.ChangedItem.Value, ChildParent.FormSurface.FlowsheetDesignSurface) Is Nothing Then
                            Dim oguid As String = ChildParent.FormSurface.AddObjectToSurface(TipoObjeto.MaterialStream, sobj.X + sobj.Width + 40, sobj.Y - 20, e.ChangedItem.Value)
                        ElseIf CType(FormFlowsheet.SearchSurfaceObjectsByTag(e.ChangedItem.Value, ChildParent.FormSurface.FlowsheetDesignSurface), GraphicObject).InputConnectors(0).IsAttached Then
                            MessageBox.Show(DWSIM.App.GetLocalString("Todasasconexespossve"), DWSIM.App.GetLocalString("Erro"), MessageBoxButtons.OK, MessageBoxIcon.Error)
                            Exit Sub
                        End If
                        If Not sobj.OutputConnectors(2).IsAttached Then
                            ChildParent.ConnectObject(sobj, FormFlowsheet.SearchSurfaceObjectsByTag(e.ChangedItem.Value, ChildParent.FormSurface.FlowsheetDesignSurface), 2, 0)
                        Else
                            ChildParent.DisconnectObject(sobj, sobj.OutputConnectors(2).AttachedConnector.AttachedTo)
                            ChildParent.ConnectObject(sobj, FormFlowsheet.SearchSurfaceObjectsByTag(e.ChangedItem.Value, ChildParent.FormSurface.FlowsheetDesignSurface), 2, 0)
                        End If
                    Else
                        If e.OldValue.ToString <> "" Then
                            ChildParent.DisconnectObject(sobj, sobj.OutputConnectors(2).AttachedConnector.AttachedTo)
                        End If
                    End If
                ElseIf e.ChangedItem.Label.Equals(DWSIM.App.GetLocalString("CorrentedeenergiaS")) Then
                    If e.ChangedItem.Value <> "" Then
                        If FormFlowsheet.SearchSurfaceObjectsByTag(e.ChangedItem.Value, ChildParent.FormSurface.FlowsheetDesignSurface) Is Nothing Then
                            Dim oguid As String = ChildParent.FormSurface.AddObjectToSurface(TipoObjeto.EnergyStream, sobj.X + sobj.Width * 1.5, sobj.Y + sobj.Height + 40, e.ChangedItem.Value)
                        ElseIf CType(FormFlowsheet.SearchSurfaceObjectsByTag(e.ChangedItem.Value, ChildParent.FormSurface.FlowsheetDesignSurface), GraphicObject).InputConnectors(0).IsAttached Then
                            MessageBox.Show(DWSIM.App.GetLocalString("Todasasconexespossve"), DWSIM.App.GetLocalString("Erro"), MessageBoxButtons.OK, MessageBoxIcon.Error)
                            Exit Sub
                        End If
                        If Not sobj.OutputConnectors(3).IsAttached Then
                            ChildParent.ConnectObject(sobj, FormFlowsheet.SearchSurfaceObjectsByTag(e.ChangedItem.Value, ChildParent.FormSurface.FlowsheetDesignSurface))
                        Else
                            ChildParent.DisconnectObject(sobj, sobj.OutputConnectors(3).AttachedConnector.AttachedTo)
                            ChildParent.ConnectObject(sobj, FormFlowsheet.SearchSurfaceObjectsByTag(e.ChangedItem.Value, ChildParent.FormSurface.FlowsheetDesignSurface))
                        End If
                    Else
                        If e.OldValue.ToString <> "" Then
                            ChildParent.DisconnectObject(sobj, sobj.OutputConnectors(3).AttachedConnector.AttachedTo)
                        End If
                    End If
                End If
            End If
        End If

    End Sub

    Public Function FT(ByRef prop As String, ByVal unit As String)
        Return prop & " (" & unit & ")"
    End Function

    Public Overridable ReadOnly Property FlowSheet() As Global.DWSIM.FormFlowsheet
        Get
            Dim frm As FormFlowsheet = My.Application.ActiveSimulation
            If Not frm Is Nothing Then Return frm
            If Not FormMain.ActiveMdiChild Is Nothing Then
                If TypeOf FormMain.ActiveMdiChild Is FormFlowsheet Then
                    If frm Is Nothing Then frm = FormMain.ActiveMdiChild Else m_flowsheet = frm
                    If frm Is Nothing Then frm = m_flowsheet
                    If Not frm Is Nothing Then Return frm Else Return Nothing
                Else
                    If FormMain.ActiveMdiChild IsNot Nothing Then
                        If TypeOf FormMain.ActiveMdiChild Is FormFlowsheet Then Return FormMain.ActiveMdiChild Else Return Nothing
                    Else
                        Return Nothing
                    End If
                End If
            Else
                If frm Is Nothing Then frm = m_flowsheet
                If Not frm Is Nothing Then Return frm Else Return Nothing
            End If
        End Get
    End Property

    Public Property Annotation() As DWSIM.Outros.Annotation
        Get
            If Me.m_annotation Is Nothing Then Me.m_annotation = New DWSIM.Outros.Annotation
            Return Me.m_annotation
        End Get
        Set(ByVal value As DWSIM.Outros.Annotation)
            Me.m_annotation = value
        End Set
    End Property

    Public Property IsAdjustAttached() As Boolean
        Get
            Return Me.m_IsAdjustAttached
        End Get
        Set(ByVal value As Boolean)
            Me.m_IsAdjustAttached = value
        End Set
    End Property

    Public Property AttachedAdjustId() As String
        Get
            Return Me.m_AdjustId
        End Get
        Set(ByVal value As String)
            Me.m_AdjustId = value
        End Set
    End Property

    Public Property AdjustVarType() As DWSIM.SimulationObjects.SpecialOps.Helpers.Adjust.TipoVar
        Get
            Return Me.m_AdjustVarType
        End Get
        Set(ByVal value As DWSIM.SimulationObjects.SpecialOps.Helpers.Adjust.TipoVar)
            Me.m_AdjustVarType = value
        End Set
    End Property

    Public Property IsSpecAttached() As Boolean
        Get
            Return Me.m_IsSpecAttached
        End Get
        Set(ByVal value As Boolean)
            Me.m_IsSpecAttached = value
        End Set
    End Property

    Public Property AttachedSpecId() As String
        Get
            Return Me.m_SpecId
        End Get
        Set(ByVal value As String)
            Me.m_SpecId = value
        End Set
    End Property

    Public Property SpecVarType() As DWSIM.SimulationObjects.SpecialOps.Helpers.Spec.TipoVar
        Get
            Return Me.m_SpecVarType
        End Get
        Set(ByVal value As DWSIM.SimulationObjects.SpecialOps.Helpers.Spec.TipoVar)
            Me.m_SpecVarType = value
        End Set
    End Property

    Public Property GraphicObject() As GraphicObject
        Get
            Return m_graphicobject
        End Get
        Set(ByVal gObj As GraphicObject)
            m_graphicobject = gObj
        End Set
    End Property

    Public Property NodeTableItems() As System.Collections.Generic.Dictionary(Of Integer, DWSIM.Outros.NodeItem)
        Get
            If m_nodeitems Is Nothing Then m_nodeitems = New Generic.Dictionary(Of Integer, DWSIM.Outros.NodeItem)()
            Return m_nodeitems
        End Get
        Set(ByVal value As System.Collections.Generic.Dictionary(Of Integer, DWSIM.Outros.NodeItem))
            m_nodeitems = value
        End Set
    End Property

    Public Property QTNodeTableItems() As System.Collections.Generic.Dictionary(Of Integer, DWSIM.Outros.NodeItem)
        Get
            If m_qtnodeitems Is Nothing Then m_qtnodeitems = New Generic.Dictionary(Of Integer, DWSIM.Outros.NodeItem)()
            Return m_qtnodeitems
        End Get
        Set(ByVal value As System.Collections.Generic.Dictionary(Of Integer, DWSIM.Outros.NodeItem))
            m_qtnodeitems = value
        End Set
    End Property

    Public Property Descricao() As String
        Get
            Return m_ComponentDescription
        End Get
        Set(ByVal value As String)
            m_ComponentDescription = value
        End Set
    End Property

    Public Property Nome() As String
        Get
            Return m_ComponentName
        End Get
        Set(ByVal value As String)
            m_ComponentName = value
        End Set
    End Property

    Public Property Tabela() As DWSIM.GraphicObjects.TableGraphic
        Get
            Return m_table
        End Get
        Set(ByVal value As DWSIM.GraphicObjects.TableGraphic)
            m_table = value
        End Set
    End Property

    Public Property TabelaRapida() As DWSIM.GraphicObjects.QuickTableGraphic
        Get
            Return m_qtable
        End Get
        Set(ByVal value As DWSIM.GraphicObjects.QuickTableGraphic)
            m_qtable = value
        End Set
    End Property

    Public Property ShowQuickTable() As Boolean
        Get
            Return Me.m_showqtable
        End Get
        Set(ByVal value As Boolean)
            Me.m_showqtable = value
        End Set
    End Property

    Sub CreateNew()
        Me.m_annotation = New DWSIM.Outros.Annotation
        Me.m_nodeitems = New System.Collections.Generic.Dictionary(Of Integer, DWSIM.Outros.NodeItem)
        Me.m_qtnodeitems = New System.Collections.Generic.Dictionary(Of Integer, DWSIM.Outros.NodeItem)
    End Sub

    Public Function Clone() As Object Implements System.ICloneable.Clone

        Return ObjectCopy(Me)

    End Function

    Function ObjectCopy(ByVal obj As SimulationObjects_BaseClass) As SimulationObjects_BaseClass

        Dim objMemStream As New MemoryStream(50000)
        Dim objBinaryFormatter As New BinaryFormatter(Nothing, New StreamingContext(StreamingContextStates.Clone))

        objBinaryFormatter.Serialize(objMemStream, obj)

        objMemStream.Seek(0, SeekOrigin.Begin)

        ObjectCopy = objBinaryFormatter.Deserialize(objMemStream)

        objMemStream.Close()

    End Function

    Public disposedValue As Boolean = False        ' To detect redundant calls

    ' IDisposable
    Protected Overridable Sub Dispose(ByVal disposing As Boolean)
        If Not Me.disposedValue Then
            If disposing Then
                ' TODO: free other state (managed objects).
            End If

            ' TODO: free your own state (unmanaged objects).
            ' TODO: set large fields to null.
        End If
        Me.disposedValue = True
    End Sub

#Region "   IDisposable Support "
    ' This code added by Visual Basic to correctly implement the disposable pattern.
    Public Sub Dispose() Implements IDisposable.Dispose
        ' Do not change this code.  Put cleanup code in Dispose(ByVal disposing As Boolean) above.
        Dispose(True)
        GC.SuppressFinalize(Me)
    End Sub
#End Region

    Public Overridable Function LoadData(data As System.Collections.Generic.List(Of System.Xml.Linq.XElement)) As Boolean Implements XMLSerializer.Interfaces.ICustomXMLSerialization.LoadData

        XMLSerializer.XMLSerializer.Deserialize(Me, data)

        Me.Annotation.annotation = New Object() {"", (From xel As XElement In data Select xel Where xel.Name = "Annotation").SingleOrDefault.Value}

        For Each xel2 As XElement In (From xel As XElement In data Select xel Where xel.Name = "NodeItems").Elements
            Dim text As String = xel2.@Text
            Dim ni2 As DWSIM.Outros.NodeItem = (From ni As DWSIM.Outros.NodeItem In m_nodeitems.Values Select ni Where ni.Text = text).SingleOrDefault
            If Not ni2 Is Nothing Then
                ni2.Checked = True
            End If
        Next

    End Function

    Public Overridable Function SaveData() As System.Collections.Generic.List(Of System.Xml.Linq.XElement) Implements XMLSerializer.Interfaces.ICustomXMLSerialization.SaveData

        Dim elements As System.Collections.Generic.List(Of System.Xml.Linq.XElement) = XMLSerializer.XMLSerializer.Serialize(Me)
        Dim ci As Globalization.CultureInfo = Globalization.CultureInfo.InvariantCulture

        With elements

            .Add(New XElement("Annotation", m_annotation.annotation.ToString))
            .Add(New XElement("NodeItems"))
            For Each kvp As KeyValuePair(Of Integer, DWSIM.Outros.NodeItem) In m_nodeitems
                If kvp.Value.Checked Then .Item(.Count - 1).Add(New XElement("NodeItem", New XAttribute("ID", kvp.Key),
                                                                New XAttribute("Checked", kvp.Value.Checked),
                                                                New XAttribute("Text", kvp.Value.Text),
                                                                New XAttribute("Value", kvp.Value.Value),
                                                                New XAttribute("Unit", kvp.Value.Unit)))
            Next

        End With

        Return elements

    End Function

    Public Sub CopyDataToClipboard(su As DWSIM.SistemasDeUnidades.Unidades, nf As String)

        Dim DT As New DataTable
        DT.Columns.Clear()
        DT.Columns.Add(("Propriedade"), GetType(System.String))
        DT.Columns.Add(("Valor"), GetType(System.String))
        DT.Columns.Add(("Unidade"), GetType(System.String))
        DT.Rows.Clear()

        Dim baseobj As SimulationObjects_BaseClass
        Dim properties() As String
        Dim description As String
        Dim objtype As Microsoft.Msdn.Samples.GraphicObjects.TipoObjeto
        Dim propidx, r1, r2, r3, r4, r5, r6 As Integer
        r1 = 5
        r2 = 12
        r3 = 30
        r4 = 48
        r5 = 66
        r6 = 84

        baseobj = Me
        properties = baseobj.GetProperties(SimulationObjects_BaseClass.PropertyType.ALL)
        objtype = baseobj.GraphicObject.TipoObjeto
        description = DWSIM.App.GetLocalString(baseobj.GraphicObject.Description)
        If objtype = Microsoft.Msdn.Samples.GraphicObjects.TipoObjeto.MaterialStream Then
            Dim value As String
            For propidx = 0 To r1 - 1
                value = baseobj.GetPropertyValue(properties(propidx), su)
                If Double.TryParse(value, New Double) Then
                    DT.Rows.Add(New String() {DWSIM.App.GetPropertyName(properties(propidx)), Format(Double.Parse(value), nf), baseobj.GetPropertyUnit(properties(propidx), su)})
                Else
                    DT.Rows.Add(New String() {DWSIM.App.GetPropertyName(properties(propidx)), value, baseobj.GetPropertyUnit(properties(propidx), su)})
                End If
            Next
            For propidx = r1 To r2 - 1
                value = baseobj.GetPropertyValue(properties(propidx), su)
                If Double.TryParse(value, New Double) Then
                    DT.Rows.Add(New String() {DWSIM.App.GetPropertyName(properties(propidx)), Format(Double.Parse(value), nf), baseobj.GetPropertyUnit(properties(propidx), su)})
                Else
                    DT.Rows.Add(New String() {DWSIM.App.GetPropertyName(properties(propidx)), value, baseobj.GetPropertyUnit(properties(propidx), su)})
                End If
            Next
            DT.Rows.Add(New String() {DWSIM.App.GetLocalString("FraomolarnaMistura"), "", ""})
            For Each subst As DWSIM.ClassesBasicasTermodinamica.Substancia In CType(Me, Streams.MaterialStream).Fases(0).Componentes.Values
                DT.Rows.Add(New String() {DWSIM.App.GetComponentName(subst.Nome), Format(subst.FracaoMolar.GetValueOrDefault, nf), ""})
            Next
            For propidx = r2 To r3 - 1
                value = baseobj.GetPropertyValue(properties(propidx), su)
                If Double.TryParse(value, New Double) Then
                    DT.Rows.Add(New String() {DWSIM.App.GetPropertyName(properties(propidx)), Format(Double.Parse(value), nf), baseobj.GetPropertyUnit(properties(propidx), su)})
                Else
                    DT.Rows.Add(New String() {DWSIM.App.GetPropertyName(properties(propidx)), value, baseobj.GetPropertyUnit(properties(propidx), su)})
                End If
            Next
            DT.Rows.Add(New String() {DWSIM.App.GetLocalString("FraomolarnaFaseVapor"), "", ""})
            For Each subst As DWSIM.ClassesBasicasTermodinamica.Substancia In CType(Me, Streams.MaterialStream).Fases(2).Componentes.Values
                DT.Rows.Add(New String() {DWSIM.App.GetComponentName(subst.Nome), Format(subst.FracaoMolar.GetValueOrDefault, nf), ""})
            Next
            For propidx = r3 To r4 - 1
                value = baseobj.GetPropertyValue(properties(propidx), su)
                If Double.TryParse(value, New Double) Then
                    DT.Rows.Add(New String() {DWSIM.App.GetPropertyName(properties(propidx)), Format(Double.Parse(value), nf), baseobj.GetPropertyUnit(properties(propidx), su)})
                Else
                    DT.Rows.Add(New String() {DWSIM.App.GetPropertyName(properties(propidx)), value, baseobj.GetPropertyUnit(properties(propidx), su)})
                End If
            Next
            DT.Rows.Add(New String() {DWSIM.App.GetLocalString("FraomolarnaFaseLquid"), "", ""})
            For Each subst As DWSIM.ClassesBasicasTermodinamica.Substancia In CType(Me, Streams.MaterialStream).Fases(1).Componentes.Values
                DT.Rows.Add(New String() {DWSIM.App.GetComponentName(subst.Nome), Format(subst.FracaoMolar.GetValueOrDefault, nf), ""})
            Next
            For propidx = r4 To r5 - 1
                value = baseobj.GetPropertyValue(properties(propidx), su)
                If Double.TryParse(value, New Double) Then
                    DT.Rows.Add(New String() {DWSIM.App.GetPropertyName(properties(propidx)), Format(Double.Parse(value), nf), baseobj.GetPropertyUnit(properties(propidx), su)})
                Else
                    DT.Rows.Add(New String() {DWSIM.App.GetPropertyName(properties(propidx)), value, baseobj.GetPropertyUnit(properties(propidx), su)})
                End If
            Next
            DT.Rows.Add(New String() {DWSIM.App.GetLocalString("FraomolarnaFaseLquid"), "", ""})
            For Each subst As DWSIM.ClassesBasicasTermodinamica.Substancia In CType(Me, Streams.MaterialStream).Fases(3).Componentes.Values
                DT.Rows.Add(New String() {DWSIM.App.GetComponentName(subst.Nome), Format(subst.FracaoMolar.GetValueOrDefault, nf), ""})
            Next
            For propidx = r5 To r6 - 1
                value = baseobj.GetPropertyValue(properties(propidx), su)
                If Double.TryParse(value, New Double) Then
                    DT.Rows.Add(New String() {DWSIM.App.GetPropertyName(properties(propidx)), Format(Double.Parse(value), nf), baseobj.GetPropertyUnit(properties(propidx), su)})
                Else
                    DT.Rows.Add(New String() {DWSIM.App.GetPropertyName(properties(propidx)), value, baseobj.GetPropertyUnit(properties(propidx), su)})
                End If
            Next
            DT.Rows.Add(New String() {DWSIM.App.GetLocalString("FraomolarnaFaseLquid"), "", ""})
            For Each subst As DWSIM.ClassesBasicasTermodinamica.Substancia In CType(Me, Streams.MaterialStream).Fases(4).Componentes.Values
                DT.Rows.Add(New String() {DWSIM.App.GetComponentName(subst.Nome), Format(subst.FracaoMolar.GetValueOrDefault, nf), ""})
            Next
            For propidx = r6 To 101
                value = baseobj.GetPropertyValue(properties(propidx), su)
                If Double.TryParse(value, New Double) Then
                    DT.Rows.Add(New String() {DWSIM.App.GetPropertyName(properties(propidx)), Format(Double.Parse(value), nf), baseobj.GetPropertyUnit(properties(propidx), su)})
                Else
                    DT.Rows.Add(New String() {DWSIM.App.GetPropertyName(properties(propidx)), value, baseobj.GetPropertyUnit(properties(propidx), su)})
                End If
            Next
            DT.Rows.Add(New String() {DWSIM.App.GetLocalString("FraomolarnaFaseLquid"), "", ""})
            For Each subst As DWSIM.ClassesBasicasTermodinamica.Substancia In CType(Me, Streams.MaterialStream).Fases(6).Componentes.Values
                DT.Rows.Add(New String() {DWSIM.App.GetComponentName(subst.Nome), Format(subst.FracaoMolar.GetValueOrDefault, nf), ""})
            Next
        Else
            For Each prop As String In properties
                DT.Rows.Add(New String() {DWSIM.App.GetPropertyName(prop), Format(baseobj.GetPropertyValue(prop, su), nf), baseobj.GetPropertyUnit(prop, su)})
            Next
        End If

        Dim st As New StringBuilder(DWSIM.App.GetLocalString(Me.Descricao) & ": " & Me.GraphicObject.Tag & vbCrLf)
        For Each r As DataRow In DT.Rows
            Dim l As String = ""
            For Each o As Object In r.ItemArray
                l += o.ToString() & vbTab
            Next
            st.AppendLine(l)
        Next

        Clipboard.SetText(st.ToString())

        DT.Clear()
        DT.Dispose()
        DT = Nothing

    End Sub


End Class

<System.Serializable()> <ComVisible(True)> Public MustInherit Class SimulationObjects_UnitOpBaseClass

    Inherits SimulationObjects_BaseClass

    'CAPE-OPEN Unit Operation Support
    Implements ICapeIdentification, ICapeUnit, ICapeUtilities, ICapeUnitReport

    'CAPE-OPEN Persistence Interface
    Implements IPersistStreamInit

    'CAPE-OPEN Error Interfaces
    Implements ECapeUser, ECapeUnknown, ECapeRoot

    Public ObjectType As Integer
    Private _pp As DWSIM.SimulationObjects.PropertyPackages.PropertyPackage
    Private _ppid As String = ""

    Friend _capeopenmode As Boolean = False

    Private _scripttextEB As String = ""
    Private _scripttextEA As String = ""
    Private _scriptlanguageE As scriptlanguage = scriptlanguage.IronPython
    Private _includesE() As String
    Private _fontnameE As String = "Courier New"
    Private _fontsizeE As Integer = 10
    Protected m_errormessage As String = ""

    <System.NonSerialized()> Public scope As Microsoft.Scripting.Hosting.ScriptScope
    <System.NonSerialized()> Public engine As Microsoft.Scripting.Hosting.ScriptEngine

    Public Sub New()
        MyBase.CreateNew()
    End Sub

    Public Overrides Function LoadData(data As System.Collections.Generic.List(Of System.Xml.Linq.XElement)) As Boolean

        MyBase.LoadData(data)

        Try
            Me._ppid = (From xel As XElement In data Select xel Where xel.Name = "PropertyPackage").SingleOrDefault.Value
        Catch

        End Try

        Return True

    End Function

    Public Overrides Function SaveData() As System.Collections.Generic.List(Of System.Xml.Linq.XElement)

        Dim elements As List(Of XElement) = MyBase.SaveData()

        Dim ppid As String = ""
        If _ppid <> "" Then
            ppid = _ppid
        ElseIf Not _pp Is Nothing Then
            ppid = _pp.Name
        Else
            ppid = ""
        End If
        elements.Add(New XElement("PropertyPackage", ppid))

        Return elements

    End Function

#Region "   DWSIM Specific"

    Public Enum scriptlanguage
        IronPython = 2
        Lua = 4
    End Enum

    Public Property ScriptExt_FontName() As String
        Get
            Return _fontnameE
        End Get
        Set(ByVal value As String)
            _fontnameE = value
        End Set
    End Property

    Public Property ScriptExt_FontSize() As Integer
        Get
            Return _fontsizeE
        End Get
        Set(ByVal value As Integer)
            _fontsizeE = value
        End Set
    End Property

    Public Property ScriptExt_Includes() As String()
        Get
            Return _includesE
        End Get
        Set(ByVal value As String())
            _includesE = value
        End Set
    End Property

    Public Property ScriptExt_ScriptTextB() As String
        Get
            If _scripttextEB IsNot Nothing Then Return _scripttextEB Else Return ""
        End Get
        Set(ByVal value As String)
            _scripttextEB = value
        End Set
    End Property

    Public Property ScriptExt_ScriptTextA() As String
        Get
            If _scripttextEA IsNot Nothing Then Return _scripttextEA Else Return ""
        End Get
        Set(ByVal value As String)
            _scripttextEA = value
        End Set
    End Property

    Public Property ScriptExt_Language() As scriptlanguage
        Get
            Return _scriptlanguageE
        End Get
        Set(ByVal value As scriptlanguage)
            _scriptlanguageE = value
        End Set
    End Property

    <Xml.Serialization.XmlIgnore()> Property PropertyPackage() As DWSIM.SimulationObjects.PropertyPackages.PropertyPackage
        Get
            If _ppid Is Nothing Then _ppid = ""
            If FlowSheet.Options.PropertyPackages.ContainsKey(_ppid) Then
                Return FlowSheet.Options.PropertyPackages(_ppid)
            Else
                Try
                    For Each pp As DWSIM.SimulationObjects.PropertyPackages.PropertyPackage In Me.FlowSheet.Options.PropertyPackages.Values
                        Return pp
                        Exit For
                    Next
                Catch ex As Exception
                    FlowSheet.WriteToLog(ex.Message, Color.Red, DWSIM.FormClasses.TipoAviso.Erro)
                End Try
            End If
            Return Nothing
        End Get
        Set(ByVal value As DWSIM.SimulationObjects.PropertyPackages.PropertyPackage)
            If value IsNot Nothing Then _ppid = value.UniqueID
        End Set
    End Property

    Public Overridable Function Calculate(Optional ByVal args As Object = Nothing) As Integer
        Return Nothing
    End Function

    Public Function Solve(Optional ByVal args As Object = Nothing) As Integer
        If ScriptExt_ScriptTextB <> "" Then RunScript_Before()
        Dim res As Integer = Calculate(args)
        If ScriptExt_ScriptTextA <> "" Then RunScript_After()
        Return res
    End Function

    Public Overridable Function DeCalculate() As Integer
        Return Nothing
    End Function

    Public Overridable Sub Validate()

        Dim vForm As Global.DWSIM.FormFlowsheet = FlowSheet
        Dim vEventArgs As New DWSIM.Outros.StatusChangeEventArgs
        Dim vCon As ConnectionPoint

        With vEventArgs
            .Calculado = False
            .Nome = Me.Nome
            .Tipo = Me.ObjectType
        End With

        'Validate input connections.
        For Each vCon In Me.GraphicObject.InputConnectors
            If Not vCon.IsAttached Then
                CalculateFlowsheet(FlowSheet, vEventArgs, Nothing)
                Throw New Exception(DWSIM.App.GetLocalString("Verifiqueasconexesdo"))
            End If
        Next

        'Validate output connections.
        For Each vCon In Me.GraphicObject.OutputConnectors
            If Not vCon.IsAttached Then
                CalculateFlowsheet(vForm, vEventArgs, Nothing)
                Throw New Exception(DWSIM.App.GetLocalString("Verifiqueasconexesdo"))
            End If
        Next

    End Sub

    Sub RunScript_Before()

        Select Case ScriptExt_Language
            Case 2
                engine = IronPython.Hosting.Python.CreateEngine()
                Dim paths(My.Settings.ScriptPaths.Count - 1) As String
                My.Settings.ScriptPaths.CopyTo(paths, 0)
                Try
                    engine.SetSearchPaths(paths)
                Catch ex As Exception
                End Try
                engine.Runtime.LoadAssembly(GetType(System.String).Assembly)
                engine.Runtime.LoadAssembly(GetType(DWSIM.ClassesBasicasTermodinamica.ConstantProperties).Assembly)
                engine.Runtime.LoadAssembly(GetType(Microsoft.Msdn.Samples.GraphicObjects.GraphicObject).Assembly)
                engine.Runtime.LoadAssembly(GetType(Microsoft.Msdn.Samples.DesignSurface.GraphicsSurface).Assembly)
                scope = engine.CreateScope()
                scope.SetVariable("Flowsheet", FlowSheet)
                scope.SetVariable("Spreadsheet", FlowSheet.FormSpreadsheet)
                scope.SetVariable("Me", Me)
                Dim Solver As New DWSIM.Flowsheet.COMSolver
                scope.SetVariable("Solver", Solver)
                Dim txtcode As String = ""
                If Not ScriptExt_Includes Is Nothing Then
                    For Each fname As String In Me.ScriptExt_Includes
                        txtcode += File.ReadAllText(fname) + vbCrLf
                    Next
                End If
                txtcode += Me.ScriptExt_ScriptTextB
                Dim source As Microsoft.Scripting.Hosting.ScriptSource = Me.engine.CreateScriptSourceFromString(txtcode, Microsoft.Scripting.SourceCodeKind.Statements)
                Try
                    Me.ErrorMessage = ""
                    source.Execute(Me.scope)
                Catch ex As Exception
                    Dim ops As ExceptionOperations = engine.GetService(Of ExceptionOperations)()
                    Me.ErrorMessage = ops.FormatException(ex).ToString
                    Me.DeCalculate()
                    engine = Nothing
                    scope = Nothing
                    source = Nothing
                    Throw ex
                Finally
                    engine = Nothing
                    scope = Nothing
                    source = Nothing
                End Try
        End Select

    End Sub

    Sub RunScript_After()

        Select Case ScriptExt_Language
            Case 2
                engine = IronPython.Hosting.Python.CreateEngine()
                Dim paths(My.Settings.ScriptPaths.Count - 1) As String
                My.Settings.ScriptPaths.CopyTo(paths, 0)
                Try
                    engine.SetSearchPaths(paths)
                Catch ex As Exception
                End Try
                engine.Runtime.LoadAssembly(GetType(System.String).Assembly)
                engine.Runtime.LoadAssembly(GetType(DWSIM.ClassesBasicasTermodinamica.ConstantProperties).Assembly)
                engine.Runtime.LoadAssembly(GetType(Microsoft.Msdn.Samples.GraphicObjects.GraphicObject).Assembly)
                engine.Runtime.LoadAssembly(GetType(Microsoft.Msdn.Samples.DesignSurface.GraphicsSurface).Assembly)
                scope = engine.CreateScope()
                scope.SetVariable("Flowsheet", FlowSheet)
                scope.SetVariable("Spreadsheet", FlowSheet.FormSpreadsheet)
                scope.SetVariable("Me", Me)
                Dim Solver As New DWSIM.Flowsheet.COMSolver
                scope.SetVariable("Solver", Solver)
                Dim txtcode As String = ""
                If Not ScriptExt_Includes Is Nothing Then
                    For Each fname As String In Me.ScriptExt_Includes
                        txtcode += File.ReadAllText(fname) + vbCrLf
                    Next
                End If
                txtcode += Me.ScriptExt_ScriptTextA
                Dim source As Microsoft.Scripting.Hosting.ScriptSource = Me.engine.CreateScriptSourceFromString(txtcode, Microsoft.Scripting.SourceCodeKind.Statements)
                Try
                    Me.ErrorMessage = ""
                    source.Execute(Me.scope)
                Catch ex As Exception
                    Dim ops As ExceptionOperations = engine.GetService(Of ExceptionOperations)()
                    Me.ErrorMessage = ops.FormatException(ex).ToString
                    Me.DeCalculate()
                    engine = Nothing
                    scope = Nothing
                    source = Nothing
                    Throw ex
                Finally
                    engine = Nothing
                    scope = Nothing
                    source = Nothing
                End Try
        End Select

    End Sub

    Public Property ErrorMessage() As String
        Get
            Return m_errormessage
        End Get
        Set(ByVal value As String)
            m_errormessage = value
        End Set
    End Property

    Public Overrides Sub PopulatePropertyGrid(ByRef pgrid As PropertyGridEx.PropertyGridEx, ByVal su As DWSIM.SistemasDeUnidades.Unidades)
        With pgrid
            .Item.Add(DWSIM.App.GetLocalString("UO_ScriptLanguage"), Me, "ScriptExt_Language", False, DWSIM.App.GetLocalString("UO_ScriptExtension"), "", True)
            .Item.Add(DWSIM.App.GetLocalString("UO_ScriptText_Before"), Me, "ScriptExt_ScriptTextB", False, DWSIM.App.GetLocalString("UO_ScriptExtension"), DWSIM.App.GetLocalString("Cliquenobotocomretic"), True)
            With .Item(.Item.Count - 1)
                .CustomEditor = New DWSIM.Editors.CustomUO.UIScriptEditor
                .Tag = "B"
            End With
            .Item.Add(DWSIM.App.GetLocalString("UO_ScriptText_After"), Me, "ScriptExt_ScriptTextA", False, DWSIM.App.GetLocalString("UO_ScriptExtension"), DWSIM.App.GetLocalString("Cliquenobotocomretic"), True)
            With .Item(.Item.Count - 1)
                .CustomEditor = New DWSIM.Editors.CustomUO.UIScriptEditor
                .Tag = "A"
            End With
            .Item.Add(DWSIM.App.GetLocalString("UOPropertyPackage"), Me.PropertyPackage.Tag, False, DWSIM.App.GetLocalString("UOPropertyPackage0"), "", True)
            With .Item(.Item.Count - 1)
                .CustomEditor = New DWSIM.Editors.PropertyPackages.UIPPSelector
            End With
            If Me.IsSpecAttached = True Then
                .Item.Add(DWSIM.App.GetLocalString("ObjetoUtilizadopor"), FlowSheet.Collections.ObjectCollection(Me.AttachedSpecId).GraphicObject.Tag, True, DWSIM.App.GetLocalString("Miscelnea4"), "", True)
                Select Case Me.SpecVarType
                    Case SpecialOps.Helpers.Spec.TipoVar.Destino
                        .Item.Add(DWSIM.App.GetLocalString("Utilizadocomo"), DWSIM.App.GetLocalString(Me.SpecVarType.ToString), True, DWSIM.App.GetLocalString("Miscelnea4"), "", True)
                    Case SpecialOps.Helpers.Spec.TipoVar.Fonte
                        .Item.Add(DWSIM.App.GetLocalString("Utilizadocomo"), DWSIM.App.GetLocalString("SpecSource"), True, DWSIM.App.GetLocalString("Miscelnea4"), "", True)
                End Select
            End If
            If Not Me.Annotation Is Nothing Then
                .Item.Add(DWSIM.App.GetLocalString("Anotaes"), Me, "Annotation", False, DWSIM.App.GetLocalString("Outros"), DWSIM.App.GetLocalString("Cliquenobotocomretic"), True)
                With .Item(.Item.Count - 1)
                    .IsBrowsable = False
                    .CustomEditor = New DWSIM.Editors.Annotation.UIAnnotationEditor
                End With
            End If
        End With
    End Sub

    Public Overrides Sub PropertyValueChanged(ByVal s As Object, ByVal e As System.Windows.Forms.PropertyValueChangedEventArgs)

        MyBase.PropertyValueChanged(s, e)

        If e.ChangedItem.Label.Contains(DWSIM.App.GetLocalString("UOPropertyPackage")) Then
            If e.ChangedItem.Value <> "" Then
                If FlowSheet.Options.PropertyPackages.ContainsKey(e.ChangedItem.Value) Then
                    Me.PropertyPackage = FlowSheet.Options.PropertyPackages(e.ChangedItem.Value)
                End If
            End If
        End If

    End Sub

    Sub CreateCOPorts()
        _ports = New CapeOpen.PortCollection()
        For Each c As ConnectionPoint In Me.GraphicObject.InputConnectors
            Select Case c.Type
                Case ConType.ConIn
                    _ports.Add(New UnitPort("Inlet" + Me.GraphicObject.InputConnectors.IndexOf(c).ToString, "", CapePortDirection.CAPE_INLET, CapePortType.CAPE_MATERIAL))
                    With _ports(_ports.Count - 1)
                        If c.IsAttached Then
                            .Connect(Me.FlowSheet.GetFlowsheetSimulationObject(c.AttachedConnector.AttachedFrom.Tag))
                        End If
                    End With
                Case ConType.ConEn
                    _ports.Add(New UnitPort("Inlet" + Me.GraphicObject.InputConnectors.IndexOf(c).ToString, "", CapePortDirection.CAPE_INLET, CapePortType.CAPE_ENERGY))
                    With _ports(_ports.Count - 1)
                        If c.IsAttached Then
                            .Connect(Me.FlowSheet.GetFlowsheetSimulationObject(c.AttachedConnector.AttachedFrom.Tag))
                        End If
                    End With
            End Select
        Next
        For Each c As ConnectionPoint In Me.GraphicObject.OutputConnectors
            Select Case c.Type
                Case ConType.ConOut
                    _ports.Add(New UnitPort("Outlet" + Me.GraphicObject.OutputConnectors.IndexOf(c).ToString, "", CapePortDirection.CAPE_OUTLET, CapePortType.CAPE_MATERIAL))
                    With _ports(_ports.Count - 1)
                        If c.IsAttached Then
                            .Connect(Me.FlowSheet.GetFlowsheetSimulationObject(c.AttachedConnector.AttachedTo.Tag))
                        End If
                    End With
                Case ConType.ConEn
                    _ports.Add(New UnitPort("Outlet" + Me.GraphicObject.OutputConnectors.IndexOf(c).ToString, "", CapePortDirection.CAPE_OUTLET, CapePortType.CAPE_ENERGY))
                    With _ports(_ports.Count - 1)
                        If c.IsAttached Then
                            .Connect(Me.FlowSheet.GetFlowsheetSimulationObject(c.AttachedConnector.AttachedTo.Tag))
                        End If
                    End With
            End Select
        Next
    End Sub

    Sub CreateCOParameters()
        _parameters = New CapeOpen.ParameterCollection
        Dim props() = Me.GetProperties(PropertyType.ALL)
        For Each s As String In props
            _parameters.Add(New CapeOpen.RealParameter(s, Me.GetPropertyValue(s), 0.0#, Me.GetPropertyUnit(s)))
            With _parameters.Item(_parameters.Count - 1)
                .Mode = CapeParamMode.CAPE_OUTPUT
            End With
        Next
    End Sub

#End Region

#Region "   CAPE-OPEN Unit Operation internal support"

    Friend _ports As CapeOpen.PortCollection
    Friend _parameters As CapeOpen.ParameterCollection
    Friend _simcontext As Object = Nothing

    ''' <summary>
    ''' Gets the description of the component.
    ''' </summary>
    ''' <value></value>
    ''' <returns>CapeString</returns>
    ''' <remarks>Implements CapeOpen.ICapeIdentification.ComponentName</remarks>
    Public Property ComponentDescription() As String Implements CapeOpen.ICapeIdentification.ComponentDescription
        Get
            Return Me.m_ComponentDescription
        End Get
        Set(ByVal value As String)
            Me.m_ComponentDescription = value
        End Set
    End Property

    ''' <summary>
    ''' Gets the name of the component.
    ''' </summary>
    ''' <value></value>
    ''' <returns>CapeString</returns>
    ''' <remarks>Implements CapeOpen.ICapeIdentification.ComponentDescription</remarks>
    Public Property ComponentName() As String Implements CapeOpen.ICapeIdentification.ComponentName
        Get
            Return Me.m_ComponentName
        End Get
        Set(ByVal value As String)
            Me.m_ComponentName = value
        End Set
    End Property

    ''' <summary>
    ''' Calculates the Unit Operation.
    ''' </summary>
    ''' <remarks>The Flowsheet Unit performs its calculation, that is, computes the variables that are missing at
    ''' this stage in the complete description of the input and output streams and computes any public
    ''' parameter value that needs to be displayed. Calculate will be able to do progress monitoring
    ''' and checks for interrupts as required using the simulation context. At present, there are no
    ''' standards agreed for this.
    ''' It is recommended that Flowsheet Units perform a suitable flash calculation on all output
    ''' streams. In some cases a Simulation Executive will be able to perform a flash calculation but
    ''' the writer of a Flowsheet Unit is in the best position to decide the correct flash to use.
    ''' Before performing the calculation, this method should perform any final validation tests that
    ''' are required. For example, at this point the validity of Material Objects connected to ports can
    ''' be checked.
    ''' There are no input or output arguments for this method.</remarks>
    Public Overridable Sub Calculate1() Implements CapeOpen.ICapeUnit.Calculate
        'do CAPE calculation here
    End Sub

    ''' <summary>
    ''' Return an interface to a collection containing the list of unit ports (e.g. ICapeUnitCollection).
    ''' </summary>
    ''' <value></value>
    ''' <returns>A reference to the interface on the collection containing the specified ports</returns>
    ''' <remarks>Return the collection of unit ports (i.e. ICapeUnitCollection). These are delivered as a
    ''' collection of elements exposing the interfaces ICapeUnitPort</remarks>
    <Xml.Serialization.XmlIgnore()> Public ReadOnly Property ports() As Object Implements CapeOpen.ICapeUnit.ports
        Get
            If Not Me._capeopenmode Then
                If Not Me.GraphicObject.TipoObjeto = TipoObjeto.CapeOpenUO Then
                    _ports = New CapeOpen.PortCollection
                    For Each c As ConnectionPoint In Me.GraphicObject.InputConnectors
                        If c.Type = ConType.ConIn Then
                            _ports.Add(New UnitPort(c.ConnectorName, "", CapePortDirection.CAPE_INLET, CapePortType.CAPE_MATERIAL))
                        ElseIf c.Type = ConType.ConEn Then
                            _ports.Add(New UnitPort(c.ConnectorName, "", CapePortDirection.CAPE_INLET, CapePortType.CAPE_ENERGY))
                        End If
                        With _ports(_ports.Count - 1)
                            If c.IsAttached And Not c.AttachedConnector Is Nothing Then .Connect(Me.FlowSheet.Collections.ObjectCollection(c.AttachedConnector.AttachedFrom.Name))
                        End With
                    Next
                    For Each c As ConnectionPoint In Me.GraphicObject.OutputConnectors
                        If c.Type = ConType.ConOut Then
                            _ports.Add(New UnitPort(c.ConnectorName, "", CapePortDirection.CAPE_OUTLET, CapePortType.CAPE_MATERIAL))
                        ElseIf c.Type = ConType.ConEn Then
                            _ports.Add(New UnitPort(c.ConnectorName, "", CapePortDirection.CAPE_OUTLET, CapePortType.CAPE_ENERGY))
                        End If
                        With _ports(_ports.Count - 1)
                            If c.IsAttached And Not c.AttachedConnector Is Nothing Then .Connect(Me.FlowSheet.Collections.ObjectCollection(c.AttachedConnector.AttachedTo.Name))
                        End With
                    Next
                End If
            End If
            Return _ports
        End Get
    End Property

    ''' <summary>Validates the Unit Operation.</summary>
    ''' <param name="message">An optional message describing the cause of the validation failure.</param>
    ''' <returns>TRUE if the Unit is valid</returns>
    ''' <remarks>Sets the flag that indicates whether the Flowsheet Unit is valid by validating the ports and
    ''' parameters of the Flowsheet Unit. For example, this method could check that all mandatory
    ''' ports have connections and that the values of all parameters are within bounds.
    ''' Note that the Simulation Executive can call the Validate routine at any time, in particular it
    ''' may be called before the executive is ready to call the Calculate method. This means that
    ''' Material Objects connected to unit ports may not be correctly configured when Validate is
    ''' called. The recommended approach is for this method to validate parameters and ports but not
    ''' Material Object configuration. A second level of validation to check Material Objects can be
    ''' implemented as part of Calculate, when it is reasonable to expect that the Material Objects
    ''' connected to ports will be correctly configured.</remarks>
    Public Overridable Function Validate1(ByRef message As String) As Boolean Implements CapeOpen.ICapeUnit.Validate
        'do CAPE validation here
        message = "Validation OK"
        Return True
    End Function

    ''' <summary>
    ''' Get the flag that indicates whether the Flowsheet Unit is valid (e.g. some parameter values
    ''' have changed but they have not been validated by using Validate). It has three possible
    ''' values:
    ''' ·  notValidated(CAPE_NOT_VALIDATED): the unit’s validate() method has not been
    ''' called since the last operation that could have changed the validation status of the unit, for
    ''' example an update to a parameter value of a connection to a port.
    ''' ·  invalid(CAPE_INVALID): the last time the unit’s validate() method was called it returned
    ''' false.
    ''' ·  valid(CAPE_VALID): the last time the unit’s validate() method was called it returned true.
    ''' </summary>
    ''' <value>CAPE_VALID meaning the Validate method returned success; CAPE_INVALID meaing the Validate 
    ''' method returned failure; CAPE_NOT_VALIDATED meaning that the Validate method needs to be called 
    ''' to determine whether the unit is valid or not.</value>
    ''' <returns></returns>
    ''' <remarks></remarks>
    Public Overridable ReadOnly Property ValStatus() As CapeOpen.CapeValidationStatus Implements CapeOpen.ICapeUnit.ValStatus
        Get
            Return CapeValidationStatus.CAPE_VALID
        End Get
    End Property

    ''' <summary>
    ''' The PMC displays its user interface and allows the Flowsheet User to interact with it. If no user interface is
    ''' available it returns an error.</summary>
    ''' <remarks></remarks>
    Public Overridable Sub Edit1() Implements CapeOpen.ICapeUtilities.Edit
        Throw New CapeNoImplException("ICapeUtilities.Edit() Method not implemented.")
    End Sub

    ''' <summary>
    ''' Initially, this method was only present in the ICapeUnit interface. Since ICapeUtilities.Initialize is now
    ''' available for any kind of PMC, ICapeUnit. Initialize is deprecated.
    ''' The PME will order the PMC to get initialized through this method. Any initialisation that could fail must be
    ''' placed here. Initialize is guaranteed to be the first method called by the client (except low level methods such
    ''' as class constructors or initialization persistence methods). Initialize has to be called once when the PMC is
    ''' instantiated in a particular flowsheet.
    ''' When the initialization fails, before signalling an error, the PMC must free all the resources that were
    ''' allocated before the failure occurred. When the PME receives this error, it may not use the PMC anymore.
    ''' The method terminate of the current interface must not either be called. Hence, the PME may only release
    ''' the PMC through the middleware native mechanisms.
    ''' </summary>
    ''' <remarks></remarks>
    Public Overridable Sub Initialize() Implements CapeOpen.ICapeUtilities.Initialize
        'do CAPE Initialization here
        CreateCOPorts()
        CreateCOParameters()
    End Sub

    ''' <summary>
    ''' Returns an ICapeCollection interface.
    ''' </summary>
    ''' <value></value>
    ''' <returns>CapeInterface (ICapeCollection)</returns>
    ''' <remarks>This interface will contain a collection of ICapeParameter interfaces.
    ''' This method allows any client to access all the CO Parameters exposed by a PMC. Initially, this method was
    ''' only present in the ICapeUnit interface. Since ICapeUtilities.GetParameters is now available for any kind of
    ''' PMC, ICapeUnit.GetParameters is deprecated. Consult the “Open Interface Specification: Parameter
    ''' Common Interface” document for more information about parameter. Consult the “Open Interface
    ''' Specification: Collection Common Interface” document for more information about collection.
    ''' If the PMC does not support exposing its parameters, it should raise the ECapeNoImpl error, instead of
    ''' returning a NULL reference or an empty Collection. But if the PMC supports parameters but has for this call
    ''' no parameters, it should return a valid ICapeCollection reference exposing zero parameters.</remarks>
    <Xml.Serialization.XmlIgnore()> Public ReadOnly Property parameters() As Object Implements CapeOpen.ICapeUtilities.parameters
        Get
            Return _parameters
        End Get
    End Property

    ''' <summary>
    ''' Allows the PME to convey the PMC a reference to the former’s simulation context. 
    ''' </summary>
    ''' <value>The reference to the PME’s simulation context class. For the PMC to
    ''' use this class, this reference will have to be converted to each of the
    ''' defined CO Simulation Context interfaces.</value>
    ''' <remarks>The simulation context
    ''' will be PME objects which will expose a given set of CO interfaces. Each of these interfaces will allow the
    ''' PMC to call back the PME in order to benefit from its exposed services (such as creation of material
    ''' templates, diagnostics or measurement unit conversion). If the PMC does not support accessing the
    ''' simulation context, it is recommended to raise the ECapeNoImpl error.
    ''' Initially, this method was only present in the ICapeUnit interface. Since ICapeUtilities.SetSimulationContext
    ''' is now available for any kind of PMC, ICapeUnit. SetSimulationContext is deprecated.</remarks>
    <Xml.Serialization.XmlIgnore()> Public WriteOnly Property simulationContext() As Object Implements CapeOpen.ICapeUtilities.simulationContext
        Set(ByVal value As Object)
            _simcontext = value
        End Set
    End Property

    ''' <summary>
    ''' Initially, this method was only present in the ICapeUnit interface. Since ICapeUtilities.Terminate is now
    ''' available for any kind of PMC, ICapeUnit.Terminate is deprecated.
    ''' The PME will order the PMC to get destroyed through this method. Any uninitialization that could fail must
    ''' be placed here. ‘Terminate’ is guaranteed to be the last method called by the client (except low level methods
    ''' such as class destructors). ‘Terminate’ may be called at any time, but may be only called once.
    ''' When this method returns an error, the PME should report the user. However, after that the PME is not
    ''' allowed to use the PMC anymore.
    ''' The Unit specification stated that “Terminate may check if the data has been saved and return an error if
    ''' not.” It is suggested not to follow this recommendation, since it’s the PME responsibility to save the state of
    ''' the PMC before terminating it. In the case that a user wants to close a simulation case without saving it, it’s
    ''' better to leave the PME to handle the situation instead of each PMC providing a different implementation.
    ''' </summary>
    ''' <remarks></remarks>
    Public Overridable Sub Terminate1() Implements CapeOpen.ICapeUtilities.Terminate
        If Not _simcontext Is Nothing Then
            If IsComObject(_simcontext) Then
                ReleaseComObject(_simcontext)
            Else
                _simcontext = Nothing
            End If
        End If
        Me.Dispose()
    End Sub

#End Region

#Region "   CAPE-OPEN Persistence Implementation"

    Friend m_dirty As Boolean = True

    Public Sub GetClassID(ByRef pClassID As System.Guid) Implements IPersistStreamInit.GetClassID
        pClassID = New Guid(SimulationObjects_UnitOpBaseClass.ClassId)
    End Sub

    Public Sub GetSizeMax(ByRef pcbSize As Long) Implements IPersistStreamInit.GetSizeMax
        pcbSize = 1024 * 1024
    End Sub

    Public Sub InitNew() Implements IPersistStreamInit.InitNew
        'do nothing
    End Sub

    Public Function IsDirty() As Integer Implements IPersistStreamInit.IsDirty
        Return m_dirty
    End Function

    Public Overridable Sub Load(ByVal pStm As System.Runtime.InteropServices.ComTypes.IStream) Implements IPersistStreamInit.Load

        ' Read the length of the string  
        Dim arrLen As Byte() = New [Byte](3) {}
        pStm.Read(arrLen, arrLen.Length, IntPtr.Zero)

        ' Calculate the length  
        Dim cb As Integer = BitConverter.ToInt32(arrLen, 0)

        ' Read the stream to get the string    
        Dim bytes As Byte() = New Byte(cb - 1) {}
        Dim pcb As New IntPtr()
        pStm.Read(bytes, bytes.Length, pcb)
        If System.Runtime.InteropServices.Marshal.IsComObject(pStm) Then System.Runtime.InteropServices.Marshal.ReleaseComObject(pStm)

        ' Deserialize byte array    

        Dim memoryStream As New System.IO.MemoryStream(bytes)

        Try

            Dim domain As AppDomain = AppDomain.CurrentDomain
            AddHandler domain.AssemblyResolve, New ResolveEventHandler(AddressOf MyResolveEventHandler)

            Dim myarr As ArrayList

            Dim mySerializer As Binary.BinaryFormatter = New Binary.BinaryFormatter(Nothing, New System.Runtime.Serialization.StreamingContext())
            myarr = mySerializer.Deserialize(memoryStream)

            Me._parameters = myarr(0)
            Me._ports = myarr(1)

            myarr = Nothing
            mySerializer = Nothing

            RemoveHandler domain.AssemblyResolve, New ResolveEventHandler(AddressOf MyResolveEventHandler)

        Catch p_Ex As System.Exception

            System.Windows.Forms.MessageBox.Show(p_Ex.ToString())

        End Try

        memoryStream.Close()

    End Sub

    Public Overridable Sub Save(ByVal pStm As System.Runtime.InteropServices.ComTypes.IStream, ByVal fClearDirty As Boolean) Implements IPersistStreamInit.Save

        Dim props As New ArrayList

        With props

            .Add(_parameters)
            .Add(_ports)

        End With

        Dim mySerializer As Binary.BinaryFormatter = New Binary.BinaryFormatter(Nothing, New System.Runtime.Serialization.StreamingContext())
        Dim mstr As New MemoryStream
        mySerializer.Serialize(mstr, props)
        Dim bytes As Byte() = mstr.ToArray()
        mstr.Close()

        ' construct length (separate into two separate bytes)    

        Dim arrLen As Byte() = BitConverter.GetBytes(bytes.Length)
        Try

            ' Save the array in the stream    
            pStm.Write(arrLen, arrLen.Length, IntPtr.Zero)
            pStm.Write(bytes, bytes.Length, IntPtr.Zero)
            If System.Runtime.InteropServices.Marshal.IsComObject(pStm) Then System.Runtime.InteropServices.Marshal.ReleaseComObject(pStm)

        Catch p_Ex As System.Exception

            System.Windows.Forms.MessageBox.Show(p_Ex.ToString())

        End Try

        If fClearDirty Then
            m_dirty = False
        End If

    End Sub

    Friend Function MyResolveEventHandler(ByVal sender As Object, ByVal args As ResolveEventArgs) As System.Reflection.Assembly
        Return Me.[GetType]().Assembly
    End Function

#End Region

#Region "   CAPE-OPEN Error Interfaces"

    Sub ThrowCAPEException(ByRef ex As Exception, ByVal name As String, ByVal description As String, ByVal interf As String, ByVal moreinfo As String, ByVal operation As String, ByVal scope As String, ByVal code As Integer)

        _name = name
        _code = code
        _description = description
        _interfacename = interf
        _moreinfo = moreinfo
        _operation = operation
        _scope = scope

        Throw ex

    End Sub

    Private _name, _description, _interfacename, _moreinfo, _operation, _scope As String, _code As Integer

    Public ReadOnly Property Name() As String Implements CapeOpen.ECapeRoot.Name
        Get
            Return _name
        End Get
    End Property

    Public ReadOnly Property code() As Integer Implements CapeOpen.ECapeUser.code
        Get
            Return _code
        End Get
    End Property

    Public ReadOnly Property description() As String Implements CapeOpen.ECapeUser.description
        Get
            Return _description
        End Get
    End Property

    Public ReadOnly Property interfaceName() As String Implements CapeOpen.ECapeUser.interfaceName
        Get
            Return _interfacename
        End Get
    End Property

    Public ReadOnly Property moreInfo() As String Implements CapeOpen.ECapeUser.moreInfo
        Get
            Return _moreinfo
        End Get
    End Property

    Public ReadOnly Property operation() As String Implements CapeOpen.ECapeUser.operation
        Get
            Return _operation
        End Get
    End Property

    Public ReadOnly Property scope1() As String Implements CapeOpen.ECapeUser.scope
        Get
            Return _scope
        End Get
    End Property

#End Region

#Region "   CAPE-OPEN Reports"

    Friend _reports As String() = New String() {"log", "last run", "validation results"}
    Friend _selreport As String = ""
    Friend _calclog As String = ""
    Friend _lastrun As String = ""
    Friend _valres As String = ""

    Public Sub ProduceReport(ByRef message As String) Implements CapeOpen.ICapeUnitReport.ProduceReport
        Select Case _selreport
            Case "log"
                message = _calclog
            Case "last run"
                message = _lastrun
            Case "validation results"
                message = _valres
        End Select
    End Sub

    Public ReadOnly Property reports() As Object Implements CapeOpen.ICapeUnitReport.reports
        Get
            Return _reports
        End Get
    End Property

    Public Property selectedReport() As String Implements CapeOpen.ICapeUnitReport.selectedReport
        Get
            Return _selreport
        End Get
        Set(ByVal value As String)
            _selreport = value
        End Set
    End Property

#End Region

End Class

<System.Serializable()> Public MustInherit Class SimulationObjects_SpecialOpBaseClass

    Inherits SimulationObjects_BaseClass

    Public Overridable Function Calculate() As Integer
        Return Nothing
    End Function

    Public Overridable Function DeCalculate() As Integer
        Return Nothing
    End Function

    Protected m_errormessage As String = ""

    Public Sub New()
        MyBase.CreateNew()
    End Sub

    Public Property ErrorMessage() As String
        Get
            Return m_errormessage
        End Get
        Set(ByVal value As String)
            m_errormessage = value
        End Set
    End Property

End Class
