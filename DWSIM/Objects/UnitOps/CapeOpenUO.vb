'    CAPE-OPEN Unit Operation Wrapper Class
'    Copyright 2011 Daniel Wagner O. de Medeiros
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
Imports Microsoft.Scripting.Hosting
Imports System.IO
Imports CapeOpen
Imports System.Runtime.InteropServices.ComTypes
Imports System.Runtime.Serialization
Imports STATSTG = System.Runtime.InteropServices.ComTypes.STATSTG
Imports System.Runtime.InteropServices

Imports DWSIM.DWSIM.SimulationObjects

Namespace DWSIM.SimulationObjects.UnitOps

    <System.Serializable()> Public Class CapeOpenUO

        Inherits SimulationObjects_UnitOpBaseClass

        <System.NonSerialized()> Private _couo As Object
        <System.NonSerialized()> Private _form As FormCapeOpenUnitSelector

        Private m_reactionSetID As String = "DefaultSet"
        Private m_reactionSetName As String = ""

        Private _seluo As DWSIM.SimulationObjects.UnitOps.Auxiliary.CapeOpen.CapeOpenUnitOpInfo
        Private Shadows _ports As List(Of ICapeUnitPort)
        Private _params As List(Of ICapeParameter)

        Private _istr As DWSIM.SimulationObjects.UnitOps.Auxiliary.CapeOpen.ComIStreamWrapper

        Private _restorefromcollections As Boolean = False
        Private _recalculateoutputstreams As Boolean = True

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

        Public Property RecalcOutputStreams() As Boolean
            Get
                Return _recalculateoutputstreams
            End Get
            Set(ByVal value As Boolean)
                _recalculateoutputstreams = value
            End Set
        End Property

        Public Sub New()
            MyBase.New()
        End Sub

        Public Sub New(ByVal name As String, ByVal desc As String)
            Me.New(name, desc, Nothing)
        End Sub

        Public Sub New(ByVal nome As String, ByVal descricao As String, ByVal gobj As GraphicObject)

            MyBase.CreateNew()

            Me.GraphicObject = gobj

            Me.ComponentName = nome
            Me.ComponentDescription = descricao
            Me.FillNodeItems()
            Me.QTFillNodeItems()

            _ports = New List(Of ICapeUnitPort)
            _params = New List(Of ICapeParameter)

            ShowForm()

            If Not _seluo Is Nothing Then
                Try
                    Dim t As Type = Type.GetTypeFromProgID(_seluo.TypeName)
                    _couo = Activator.CreateInstance(t)
                    InitNew()
                    Init()
                    GetPorts()
                    GetParams()
                    CreateConnectors()
                Catch ex As Exception
                    Me.FlowSheet.WriteToLog("Error creating CAPE-OPEN Unit Operation: " & ex.ToString, Color.Red, FormClasses.TipoAviso.Erro)
                End Try
            End If

        End Sub

#Region "    CAPE-OPEN Specifics"

        <OnDeserialized()> Sub PersistLoad(ByVal context As System.Runtime.Serialization.StreamingContext)

            If Not _seluo Is Nothing Then
                Dim t As Type = Type.GetTypeFromProgID(_seluo.TypeName)
                Try
                    _couo = Activator.CreateInstance(t)
                Catch ex As Exception
                    MessageBox.Show("Error creating CAPE-OPEN Unit Operation instance." & vbCrLf & ex.ToString, "Error", MessageBoxButtons.OK, MessageBoxIcon.Error)
                End Try
            End If

            If My.Settings.UseCOPersistenceSupport Then
                If _istr IsNot Nothing Then
                    Dim myuo As Interfaces.IPersistStreamInit = TryCast(_couo, Interfaces.IPersistStreamInit)
                    If Not myuo Is Nothing Then
                        Try
                            _istr.baseStream.Position = 0
                            myuo.Load(_istr)
                            _restorefromcollections = False
                        Catch ex As Exception
                            'couldn't restore data from IStream. Will restore using port and parameter collections instead.
                            MessageBox.Show(Me.GraphicObject.Tag + ": Error restoring persisted data from CAPE-OPEN Object - " + ex.Message.ToString(), "Error", MessageBoxButtons.OK, MessageBoxIcon.Error)
                            _restorefromcollections = True
                        End Try
                    Else
                        Dim myuo2 As Interfaces.IPersistStream = TryCast(_couo, Interfaces.IPersistStream)
                        If myuo2 IsNot Nothing Then
                            Try
                                _istr.baseStream.Position = 0
                                myuo2.Load(_istr)
                                _restorefromcollections = False
                            Catch ex As Exception
                                MessageBox.Show(Me.GraphicObject.Tag + ": Error restoring persisted data from CAPE-OPEN Object - " + ex.Message.ToString(), "Error", MessageBoxButtons.OK, MessageBoxIcon.Error)
                                _restorefromcollections = True
                            End Try
                        End If
                    End If
                Else
                    'the CAPE-OPEN object doesn't support Persistence, restore parameters and ports info from internal collections.
                    _restorefromcollections = True
                End If
            Else
                _restorefromcollections = True
            End If

            Init()

            If _restorefromcollections Then
                RestoreParams()
            End If

        End Sub

        <OnSerializing()> Sub PersistSave(ByVal context As System.Runtime.Serialization.StreamingContext)

            'If the Unit Operation doesn't implement any of the IPersist interfaces, the _istr variable will be null.
            'The Object will have to be restored using the parameters and ports stored information only.

            If My.Settings.UseCOPersistenceSupport Then
                If Not _couo Is Nothing Then
                    Dim myuo As Interfaces.IPersistStream = TryCast(_couo, Interfaces.IPersistStream)
                    If myuo IsNot Nothing Then
                        _istr = New DWSIM.SimulationObjects.UnitOps.Auxiliary.CapeOpen.ComIStreamWrapper(New MemoryStream())
                        Try
                            myuo.Save(_istr, True)
                        Catch ex As Exception
                            Me.FlowSheet.WriteToLog(Me.GraphicObject.Tag + ": Error saving data from CAPE-OPEN Object - " + ex.Message.ToString(), Color.Red, FormClasses.TipoAviso.Erro)
                        End Try
                    Else
                        Dim myuo2 As Interfaces.IPersistStreamInit = TryCast(_couo, Interfaces.IPersistStreamInit)
                        If myuo2 IsNot Nothing Then
                            _istr = New DWSIM.SimulationObjects.UnitOps.Auxiliary.CapeOpen.ComIStreamWrapper(New MemoryStream())
                            Try
                                myuo2.Save(_istr, True)
                            Catch ex As Exception
                                Me.FlowSheet.WriteToLog(Me.GraphicObject.Tag + ": Error saving data from CAPE-OPEN Object - " + ex.Message.ToString(), Color.Red, FormClasses.TipoAviso.Erro)
                            End Try
                        End If
                    End If
                End If
            End If

        End Sub

        Sub ShowForm()

            _form = New FormCapeOpenUnitSelector
            _form.ShowDialog(Me.FlowSheet)
            Me._seluo = _form._seluo

        End Sub

        Overloads Sub InitNew()

            Dim myuo As Interfaces.IPersistStreamInit = TryCast(_couo, Interfaces.IPersistStreamInit)
            If Not myuo Is Nothing Then
                Try
                    myuo.InitNew()
                Catch ex As Exception
                End Try
            End If

        End Sub

        Sub Init()

            If Not _couo Is Nothing Then
                Dim myuo As CapeOpen.ICapeUtilities = TryCast(_couo, CapeOpen.ICapeUtilities)
                If Not myuo Is Nothing Then myuo.Initialize()
                Dim myuo2 As CapeOpen.ICapeIdentification = TryCast(_couo, CapeOpen.ICapeIdentification)
                If Not myuo2 Is Nothing Then
                    myuo2.ComponentName = Me.GraphicObject.Tag
                    myuo2.ComponentDescription = Me.GraphicObject.Name
                End If
                If My.Settings.SetCOSimulationContext Then
                    myuo.simulationContext = Me.FlowSheet
                End If
            End If

        End Sub

        Sub Terminate()

            If Not _couo Is Nothing Then
                Dim myuo As CapeOpen.ICapeUtilities = TryCast(_couo, CapeOpen.ICapeUtilities)
                If Not myuo Is Nothing Then myuo.Terminate()
            End If

        End Sub

        Sub GetPorts()
            If Not _couo Is Nothing Then
                Dim myuo As CapeOpen.ICapeUnit = _couo
                Dim myports As ICapeCollection = myuo.ports
                Dim i As Integer = 0
                Dim numports As Integer = myports.Count
                If numports > 0 Then
                    For i = 1 To numports
                        Dim id As ICapeIdentification = myports.Item(i)
                        Dim myport As ICapeUnitPort = myports.Item(i)
                        _ports.Add(New UnitPort(id.ComponentName, id.ComponentDescription, myport.direction, myport.portType))
                    Next
                End If
            End If
        End Sub

        Sub GetParams()
            If Not _couo Is Nothing Then
                Dim myuo As CapeOpen.ICapeUtilities = _couo
                Dim myparms As ICapeCollection = myuo.parameters
                Dim paramcount As Integer = myparms.Count
                If paramcount > 0 Then
                    Dim i As Integer = 0
                    For i = 1 To paramcount
                        Dim id As ICapeIdentification = myparms.Item(i)
                        Dim myparam As ICapeParameterSpec = myparms.Item(i)
                        Select Case myparam.Type
                            Case CapeParamType.CAPE_REAL
                                Dim ips As ICapeRealParameterSpec = CType(myparam, ICapeRealParameterSpec)
                                Dim ip As ICapeParameter = CType(myparam, ICapeParameter)
                                Dim p As New RealParameter(id.ComponentName, id.ComponentDescription, ip.value, ips.DefaultValue, ips.LowerBound, ips.UpperBound, ip.Mode, "")
                                _params.Add(p)
                            Case CapeParamType.CAPE_INT
                                Dim ips As ICapeIntegerParameterSpec = CType(myparam, ICapeIntegerParameterSpec)
                                Dim ip As ICapeParameter = CType(myparam, ICapeParameter)
                                Dim p As New IntegerParameter(id.ComponentName, id.ComponentDescription, ip.value, ips.DefaultValue, ips.LowerBound, ips.UpperBound, ip.Mode)
                                _params.Add(p)
                            Case CapeParamType.CAPE_BOOLEAN
                                Dim ips As ICapeBooleanParameterSpec = CType(myparam, ICapeBooleanParameterSpec)
                                Dim ip As ICapeParameter = CType(myparam, ICapeParameter)
                                Dim p As New BooleanParameter(id.ComponentName, id.ComponentDescription, ip.value, ips.DefaultValue, ip.Mode)
                                _params.Add(p)
                            Case CapeParamType.CAPE_OPTION
                                Dim ips As ICapeOptionParameterSpec = CType(myparam, ICapeOptionParameterSpec)
                                Dim ip As ICapeParameter = CType(myparam, ICapeParameter)
                                Dim p As New OptionParameter(id.ComponentName, id.ComponentDescription, ip.value, ips.DefaultValue, ips.OptionList, ips.RestrictedToList, ip.Mode)
                                _params.Add(p)
                            Case CapeParamType.CAPE_ARRAY
                                'Dim ip As ICapeParameter = CType(myparam, ICapeParameter)
                                '_params.Add(ip)
                        End Select
                    Next
                End If
            End If
        End Sub

        Sub RestorePorts()
            If Not _couo Is Nothing Then
                Dim myuo As ICapeUnit = _couo
                Dim myports As ICapeCollection = myuo.ports
                Dim i As Integer = 0
                Dim numports As Integer = myports.Count
                If numports > 0 Then
                    For i = 1 To numports
                        Dim id As ICapeIdentification = myports.Item(i)
                        Dim myport As ICapeUnitPort = myports.Item(i)
                        For Each p As UnitPort In _ports
                            If id.ComponentName = p.ComponentName Then
                                If p.connectedObject IsNot Nothing Then
                                    If Not My.Application.ActiveSimulation Is Nothing Then
                                        Dim mystr As SimulationObjects_BaseClass = My.Application.ActiveSimulation.Collections.ObjectCollection(CType(p.connectedObject, ICapeIdentification).ComponentDescription)
                                        If Not myport.connectedObject Is Nothing Then myport.Disconnect()
                                        myport.Connect(mystr)
                                    End If
                                End If
                            End If
                        Next
                    Next
                End If
            End If
        End Sub

        Sub RestoreParams()
            If Not _couo Is Nothing Then
                Dim myuo As CapeOpen.ICapeUtilities = _couo
                Dim myparms As ICapeCollection = myuo.parameters
                Dim paramcount As Integer = myparms.Count
                If paramcount > 0 Then
                    Dim i As Integer = 0
                    For i = 1 To paramcount
                        Dim myparam As ICapeParameterSpec = myparms.Item(i)
                        Dim ip As ICapeParameter = CType(myparam, ICapeParameter)
                        Try
                            ip.value = _params(i - 1).value
                        Catch ex As Exception

                        End Try
                    Next
                End If
            End If
        End Sub

        Sub UpdateParams()
            If Not _couo Is Nothing Then
                Dim myuo As CapeOpen.ICapeUtilities = _couo
                Dim myparms As ICapeCollection = myuo.parameters
                Dim paramcount As Integer = myparms.Count
                If paramcount > 0 Then
                    If CInt(paramcount) <> _params.Count Then
                        _params = New List(Of ICapeParameter)
                        GetParams()
                    End If
                    Dim i As Integer = 0
                    For i = 1 To paramcount
                        Dim myparam As ICapeParameterSpec = myparms.Item(i)
                        Dim ip As ICapeParameter = CType(myparam, ICapeParameter)
                        Try
                            _params(i - 1).value = ip.value
                        Catch ex As Exception
                            Console.WriteLine(ex.ToString)
                        End Try
                    Next
                End If
            End If
        End Sub

        Sub CreateConnectors()
            Me.GraphicObject.InputConnectors = New List(Of ConnectionPoint)
            Me.GraphicObject.OutputConnectors = New List(Of ConnectionPoint)
            Dim nip As Integer = 1
            Dim nop As Integer = 1
            Dim objid As String = ""
            Dim i As Integer = 0
            For Each p As UnitPort In _ports
                Select Case p.direction
                    Case CapePortDirection.CAPE_INLET
                        nip += 1
                    Case CapePortDirection.CAPE_OUTLET
                        nop += 1
                End Select
            Next
            For Each p As UnitPort In _ports
                Select Case p.direction
                    Case CapePortDirection.CAPE_INLET
                        Me.GraphicObject.InputConnectors.Add(New ConnectionPoint())
                        With Me.GraphicObject.InputConnectors(Me.GraphicObject.InputConnectors.Count - 1)
                            Select Case p.portType
                                Case CapePortType.CAPE_ENERGY
                                    .Type = ConType.ConEn
                                Case CapePortType.CAPE_MATERIAL
                                    .Type = ConType.ConIn
                            End Select
                            .Position = New Point(Me.GraphicObject.X, Me.GraphicObject.Y + (Me.GraphicObject.InputConnectors.Count) / (nip - 1) * Me.GraphicObject.Height / 2)
                            .ConnectorName = p.ComponentName
                        End With
                    Case CapePortDirection.CAPE_OUTLET
                        Me.GraphicObject.OutputConnectors.Add(New ConnectionPoint())
                        With Me.GraphicObject.OutputConnectors(Me.GraphicObject.OutputConnectors.Count - 1)
                            Select Case p.portType
                                Case CapePortType.CAPE_ENERGY
                                    .Type = ConType.ConEn
                                Case CapePortType.CAPE_MATERIAL
                                    .Type = ConType.ConOut
                            End Select
                            .Position = New Point(Me.GraphicObject.X + Me.GraphicObject.Width, Me.GraphicObject.Y + +(Me.GraphicObject.InputConnectors.Count) / (nip - 1) * Me.GraphicObject.Height / 2)
                            .ConnectorName = p.ComponentName
                        End With
                End Select
            Next
            UpdateConnectorPositions()
        End Sub

        Sub UpdateConnectors()

            ' disconnect existing connections
            For Each c As ConnectionPoint In Me.GraphicObject.InputConnectors
                If c.IsAttached Then
                    Me.FlowSheet.DisconnectObject(Me.FlowSheet.GetFlowsheetGraphicObject(c.AttachedConnector.AttachedFrom.Tag), Me.GraphicObject)
                End If
            Next
            For Each c As ConnectionPoint In Me.GraphicObject.OutputConnectors
                If c.IsAttached Then
                    Me.FlowSheet.DisconnectObject(Me.GraphicObject, Me.FlowSheet.GetFlowsheetGraphicObject(c.AttachedConnector.AttachedTo.Tag))
                End If
            Next

            Me.GraphicObject.InputConnectors = New List(Of ConnectionPoint)
            Me.GraphicObject.OutputConnectors = New List(Of ConnectionPoint)

            If Not _couo Is Nothing Then
                Dim myuo As CapeOpen.ICapeUnit = _couo
                Dim myports As ICapeCollection = myuo.ports
                Dim nip As Integer = 1
                Dim nop As Integer = 1
                Dim objid As String
                Dim i As Integer = 0
                Dim numports As Integer = myports.Count
                If numports > 0 Then
                    For i = 1 To numports
                        Dim id As ICapeIdentification = myports.Item(i)
                        Dim myport As ICapeUnitPort = myports.Item(i)
                        Select Case myport.direction
                            Case CapePortDirection.CAPE_INLET
                                nip += 1
                            Case CapePortDirection.CAPE_OUTLET
                                nop += 1
                        End Select
                    Next
                    For i = 1 To numports
                        Dim id As ICapeIdentification = myports.Item(i)
                        Dim myport As ICapeUnitPort = myports.Item(i)
                        Select Case myport.direction
                            Case CapePortDirection.CAPE_INLET
                                Me.GraphicObject.InputConnectors.Add(New ConnectionPoint())
                                With Me.GraphicObject.InputConnectors(Me.GraphicObject.InputConnectors.Count - 1)
                                    Select Case myport.portType
                                        Case CapePortType.CAPE_ENERGY
                                            .Type = ConType.ConEn
                                        Case CapePortType.CAPE_MATERIAL
                                            .Type = ConType.ConIn
                                    End Select
                                    .Position = New Point(Me.GraphicObject.X, Me.GraphicObject.Y + (Me.GraphicObject.InputConnectors.Count) / (nip - 1) * Me.GraphicObject.Height / 2)
                                    .ConnectorName = id.ComponentName
                                End With
                                If myport.connectedObject IsNot Nothing Then
                                    objid = CType(myport.connectedObject, ICapeIdentification).ComponentDescription
                                    myport.Disconnect()
                                    Dim gobj As GraphicObject = FormFlowsheet.SearchSurfaceObjectsByName(objid, Me.FlowSheet.FormSurface.FlowsheetDesignSurface)
                                    Select Case myport.portType
                                        Case CapePortType.CAPE_MATERIAL
                                            Me.FlowSheet.ConnectObject(gobj, Me.GraphicObject, 0, Me.GraphicObject.InputConnectors.Count - 1)
                                        Case CapePortType.CAPE_ENERGY
                                            Me.FlowSheet.ConnectObject(gobj, Me.GraphicObject, 0, Me.GraphicObject.InputConnectors.Count - 1)
                                    End Select
                                    myport.Connect(Me.FlowSheet.GetFlowsheetSimulationObject(gobj.Tag))
                                End If
                            Case CapePortDirection.CAPE_OUTLET
                                Me.GraphicObject.OutputConnectors.Add(New ConnectionPoint())
                                With Me.GraphicObject.OutputConnectors(Me.GraphicObject.OutputConnectors.Count - 1)
                                    Select Case myport.portType
                                        Case CapePortType.CAPE_ENERGY
                                            .Type = ConType.ConEn
                                        Case CapePortType.CAPE_MATERIAL
                                            .Type = ConType.ConOut
                                    End Select
                                    .Position = New Point(Me.GraphicObject.X + Me.GraphicObject.Width, Me.GraphicObject.Y + (Me.GraphicObject.OutputConnectors.Count) / (nop - 1) * Me.GraphicObject.Height / 2)
                                    .ConnectorName = id.ComponentName
                                End With
                                If myport.connectedObject IsNot Nothing Then
                                    objid = CType(myport.connectedObject, ICapeIdentification).ComponentDescription
                                    myport.Disconnect()
                                    Dim gobj As GraphicObject = FormFlowsheet.SearchSurfaceObjectsByName(objid, Me.FlowSheet.FormSurface.FlowsheetDesignSurface)
                                    Select Case myport.portType
                                        Case CapePortType.CAPE_MATERIAL
                                            Me.FlowSheet.ConnectObject(Me.GraphicObject, gobj, Me.GraphicObject.OutputConnectors.Count - 1, 0)
                                        Case CapePortType.CAPE_ENERGY
                                            Me.FlowSheet.ConnectObject(Me.GraphicObject, gobj, Me.GraphicObject.OutputConnectors.Count - 1, 0)
                                    End Select
                                    myport.Connect(Me.FlowSheet.GetFlowsheetSimulationObject(gobj.Tag))
                                End If
                        End Select
                    Next
                End If
                UpdateConnectorPositions()
            End If
        End Sub

        Sub UpdateConnectorPositions()
            Dim i As Integer = 0
            Dim obj1(Me.GraphicObject.InputConnectors.Count), obj2(Me.GraphicObject.InputConnectors.Count) As Double
            Dim obj3(Me.GraphicObject.OutputConnectors.Count), obj4(Me.GraphicObject.OutputConnectors.Count) As Double
            For Each ic As ConnectionPoint In Me.GraphicObject.InputConnectors
                obj1(i) = -Me.GraphicObject.X + ic.Position.X
                obj2(i) = -Me.GraphicObject.Y + ic.Position.Y
                i = i + 1
            Next
            i = 0
            For Each oc As ConnectionPoint In Me.GraphicObject.OutputConnectors
                obj3(i) = -Me.GraphicObject.X + oc.Position.X
                obj4(i) = -Me.GraphicObject.Y + oc.Position.Y
                i = i + 1
            Next
            Me.GraphicObject.AdditionalInfo = New Object() {obj1, obj2, obj3, obj4}
        End Sub

        Sub UpdatePorts()
            If Not _couo Is Nothing Then
                Dim myuo As CapeOpen.ICapeUnit = _couo
                Dim myports As ICapeCollection = myuo.ports
                Dim i As Integer = 0
                Dim numports As Integer = myports.Count
                _ports.Clear()
                If numports > 0 Then
                    For i = 1 To numports
                        Dim id As ICapeIdentification = myports.Item(i)
                        Dim myport As ICapeUnitPort = myports.Item(i)
                        _ports.Add(New UnitPort(id.ComponentName, id.ComponentDescription, myport.direction, myport.portType))
                        Try
                            If Not myport.connectedObject Is Nothing Then
                                _ports(_ports.Count - 1).Connect(Me.FlowSheet.Collections.ObjectCollection(CType(myport.connectedObject, ICapeIdentification).ComponentDescription))
                            End If
                        Catch ex As Exception
                            Dim ecu As CapeOpen.ECapeUser = myuo
                            Me.FlowSheet.WriteToLog(Me.GraphicObject.Tag & ": CAPE-OPEN Exception: " & ecu.code & " at " & ecu.interfaceName & ". Reason: " & ecu.description, Color.DarkGray, FormClasses.TipoAviso.Aviso)
                        End Try
                    Next
                End If
            End If
        End Sub

        Sub UpdatePortsFromConnectors()

            For Each c As ConnectionPoint In Me.GraphicObject.InputConnectors
                For Each p As UnitPort In _ports
                    If c.ConnectorName = p.ComponentName Then
                        If Not c.IsAttached Then
                            p.Disconnect()
                        ElseIf c.IsAttached And p.connectedObject Is Nothing Then
                            p.Connect(Me.FlowSheet.Collections.ObjectCollection(c.AttachedConnector.AttachedFrom.Name))
                        End If
                    End If
                Next
            Next

            For Each c As ConnectionPoint In Me.GraphicObject.OutputConnectors
                For Each p As UnitPort In _ports
                    If c.ConnectorName = p.ComponentName Then
                        If Not c.IsAttached Then
                            p.Disconnect()
                        ElseIf c.IsAttached And p.connectedObject Is Nothing Then
                            p.Connect(Me.FlowSheet.Collections.ObjectCollection(c.AttachedConnector.AttachedTo.Name))
                        End If
                    End If
                Next
            Next

            RestorePorts()

        End Sub

        Function Edit(ByVal sender As Object, ByVal e As System.EventArgs) As Object
            If Not _couo Is Nothing Then
                Dim myuo As CapeOpen.ICapeUtilities = _couo
                RestorePorts()
                Try
                    'set reaction set, if supported
                    If Not TryCast(_couo, ICapeKineticReactionContext) Is Nothing Then
                        Me.FlowSheet.Options.ReactionSets(Me.ReactionSetID).simulationContext = Me.FlowSheet
                        Dim myset As Object = CType(Me.FlowSheet.Options.ReactionSets(Me.ReactionSetID), Object)
                        Dim myruo As CAPEOPEN110.ICapeKineticReactionContext = _couo
                        myruo.SetReactionObject(myset)
                    End If
                    myuo.Edit()
                Catch ex As Exception
                    Dim ecu As CapeOpen.ECapeUser = myuo
                    Me.FlowSheet.WriteToLog(Me.GraphicObject.Tag & ": CAPE-OPEN Exception: " & ecu.code & " at " & ecu.interfaceName & ". Reason: " & ecu.description, Color.DarkGray, FormClasses.TipoAviso.Aviso)
                    Throw
                End Try
                UpdateParams()
                UpdatePorts()
                UpdateConnectors()
            End If
            Return "click to show ->"
        End Function

#End Region

#Region "    DWSIM Specifics"

        Public Overrides Function Calculate(Optional ByVal args As Object = Nothing) As Integer

            UpdatePortsFromConnectors()

            Dim objargs As New DWSIM.Outros.StatusChangeEventArgs

            If Not _couo Is Nothing Then
                For Each c As ConnectionPoint In Me.GraphicObject.InputConnectors
                    If c.IsAttached And c.Type = ConType.ConIn Then
                        Dim mat As Streams.MaterialStream = Me.FlowSheet.Collections.ObjectCollection(c.AttachedConnector.AttachedFrom.Name)
                        mat.SetFlowsheet(Me.FlowSheet)
                    End If
                Next
                Dim myuo As CapeOpen.ICapeUnit = _couo
                Dim msg As String = ""
                Try
                    'set reaction set, if supported
                    If Not TryCast(_couo, ICapeKineticReactionContext) Is Nothing Then
                        Me.FlowSheet.Options.ReactionSets(Me.ReactionSetID).simulationContext = Me.FlowSheet
                        Dim myset As Object = CType(Me.FlowSheet.Options.ReactionSets(Me.ReactionSetID), Object)
                        Dim myruo As CAPEOPEN110.ICapeKineticReactionContext = _couo
                        myruo.SetReactionObject(myset)
                    End If
                Catch ex As Exception
                    With objargs
                        .Calculado = False
                        .Nome = Me.Nome
                        .Tag = Me.GraphicObject.Tag
                        .Tipo = TipoObjeto.CapeOpenUO
                    End With
                    For Each c As ConnectionPoint In Me.GraphicObject.OutputConnectors
                        If c.Type = ConType.ConEn Then
                            If c.IsAttached Then c.AttachedConnector.AttachedTo.Calculated = False
                        End If
                    Next
                    Dim ecu As CapeOpen.ECapeUser = myuo
                    Me.FlowSheet.WriteToLog(Me.GraphicObject.Tag & ": CAPE-OPEN Exception " & ecu.code & " at " & ecu.interfaceName & ":" & ecu.scope & ". Reason: " & ecu.description, Color.Red, FormClasses.TipoAviso.Erro)
                End Try
                My.Application.ActiveSimulation = Me.FlowSheet
                myuo.Validate(msg)
                If myuo.ValStatus = CapeValidationStatus.CAPE_VALID Then
                    Try
                        For Each c As ConnectionPoint In Me.GraphicObject.OutputConnectors
                            If c.IsAttached And c.Type = ConType.ConOut Then
                                Dim mat As Streams.MaterialStream = Me.FlowSheet.Collections.ObjectCollection(c.AttachedConnector.AttachedTo.Name)
                                mat.ClearAllProps()
                            End If
                        Next
                        RestorePorts()
                        myuo.Calculate()
                        UpdateParams()
                        'Call function to calculate flowsheet
                        With objargs
                            .Calculado = True
                            .Nome = Me.Nome
                            .Tag = Me.GraphicObject.Tag
                            .Tipo = TipoObjeto.CapeOpenUO
                        End With
                        For Each c As ConnectionPoint In Me.GraphicObject.OutputConnectors
                            If c.Type = ConType.ConEn And c.IsAttached Then
                                c.AttachedConnector.AttachedTo.Calculated = True
                            End If
                        Next
                    Catch ex As Exception
                        With objargs
                            .Calculado = False
                            .Nome = Me.Nome
                            .Tag = Me.GraphicObject.Tag
                            .Tipo = TipoObjeto.CapeOpenUO
                        End With
                        For Each c As ConnectionPoint In Me.GraphicObject.OutputConnectors
                            If c.Type = ConType.ConEn Then
                                If c.IsAttached Then c.AttachedConnector.AttachedTo.Calculated = False
                            End If
                        Next
                        Dim ecu As CapeOpen.ECapeUser = myuo
                        Me.FlowSheet.WriteToLog(Me.GraphicObject.Tag & ": CAPE-OPEN Exception " & ecu.code & " at " & ecu.interfaceName & ":" & ecu.scope & ". Reason: " & ecu.description, Color.Red, FormClasses.TipoAviso.Erro)
                    Finally
                        Dim ur As CapeOpen.ICapeUnitReport = _couo
                        If Not ur Is Nothing Then
                            Dim reps As String() = ur.reports
                            For Each r As String In reps
                                ur.selectedReport = r
                                Dim msg2 As String = ""
                                ur.ProduceReport(msg2)
                                Me.FlowSheet.FormCOReports.TextBox1.AppendText(Date.Now.ToString + ", " + Me.GraphicObject.Tag + " (" + r + "):" + vbCrLf + vbCrLf + msg2 + vbCrLf + vbCrLf)
                            Next
                        End If
                    End Try
                Else
                    Me.FlowSheet.WriteToLog(Me.GraphicObject.Tag + ": CO Unit not validated. Reason: " + msg, Color.Red, FormClasses.TipoAviso.Erro)
                    'Call function to calculate flowsheet
                    With objargs
                        .Calculado = False
                        .Nome = Me.Nome
                        .Tag = Me.GraphicObject.Tag
                        .Tipo = TipoObjeto.CapeOpenUO
                    End With
                    For Each c As ConnectionPoint In Me.GraphicObject.OutputConnectors
                        If c.Type = ConType.ConEn Then
                            If c.IsAttached Then c.AttachedConnector.AttachedTo.Calculated = False
                        End If
                    Next
                End If
            End If

            FlowSheet.CalculationQueue.Enqueue(objargs)

        End Function

        Public Overrides Function DeCalculate() As Integer

            'Call function to calculate flowsheet
            Dim objargs As New DWSIM.Outros.StatusChangeEventArgs
            With objargs
                .Calculado = False
                .Nome = Me.Nome
                .Tag = Me.GraphicObject.Tag
                .Tipo = TipoObjeto.CapeOpenUO
            End With

            FlowSheet.CalculationQueue.Enqueue(objargs)

        End Function

        Public Overrides Sub Validate()
            MyBase.Validate()
        End Sub

        Public Overrides Sub PopulatePropertyGrid(ByRef pgrid As PropertyGridEx.PropertyGridEx, ByVal su As SistemasDeUnidades.Unidades)

            UpdatePortsFromConnectors()

            Dim Conversor As New DWSIM.SistemasDeUnidades.Conversor

            With pgrid

                .PropertySort = PropertySort.Categorized
                .Item.Clear()
                .ShowCustomProperties = True

                MyBase.PopulatePropertyGrid(pgrid, su)

                'identify
                .Item.Add("Name", _seluo.Name, True, "1. CAPE-OPEN Object Info", "", True)
                .Item.Add("Description", _seluo.Description, True, "1. CAPE-OPEN Object Info", "", True)
                .Item.Add("ProgID", _seluo.TypeName, True, "1. CAPE-OPEN Object Info", "", True)
                .Item.Add("Version", _seluo.Version, True, "1. CAPE-OPEN Object Info", "", True)
                .Item.Add("CAPE-OPEN Version", _seluo.CapeVersion, True, "1. CAPE-OPEN Object Info", "", True)
                .Item.Add("File Location", _seluo.Location, True, "1. CAPE-OPEN Object Info", "", True)
                .Item.Add("Vendor URL", _seluo.VendorURL, True, "1. CAPE-OPEN Object Info", "", True)
                '.Item.Add("Help URL", _seluo.HelpURL, True, "1. CAPE-OPEN Object Info", "", True)

                'show edit form if available
                .Item.Add("Editing Form", "click to show ->", False, "2. Editing Form", "", True)
                .Item(.Item.Count - 1).OnClick = AddressOf Me.Edit

                'populate ports
                For Each p As UnitPort In _ports
                    If p.portType = CapePortType.CAPE_MATERIAL Then
                        Dim tag As String = ""
                        Dim conobj As DWSIM.SimulationObjects.Streams.MaterialStream = p.connectedObject
                        If Not conobj Is Nothing Then tag = conobj.GraphicObject.Tag
                        .Item.Add(p.ComponentName + " [" + p.direction.ToString + ", " + p.portType.ToString() + "]", tag, False, "3. Ports", p.ComponentDescription, True)
                        With .Item(.Item.Count - 1)
                            .Tag = _ports.IndexOf(p)
                            If p.direction = CapePortDirection.CAPE_INLET Then
                                .CustomEditor = New DWSIM.Editors.Streams.UIInputMSSelector
                            Else
                                .CustomEditor = New DWSIM.Editors.Streams.UIOutputMSSelector
                            End If
                        End With
                    ElseIf p.portType = CapePortType.CAPE_ENERGY Then
                        Dim tag As String = ""
                        Dim conobj As DWSIM.SimulationObjects.Streams.EnergyStream = p.connectedObject
                        If Not conobj Is Nothing Then tag = conobj.GraphicObject.Tag
                        .Item.Add(p.ComponentName + " [" + p.direction.ToString + ", " + p.portType.ToString() + "]", tag, False, "3. Ports", p.ComponentDescription, True)
                        With .Item(.Item.Count - 1)
                            .Tag = _ports.IndexOf(p)
                            If p.direction = CapePortDirection.CAPE_INLET Then
                                .CustomEditor = New DWSIM.Editors.Streams.UIInputESSelector
                            Else
                                .CustomEditor = New DWSIM.Editors.Streams.UIOutputESSelector
                            End If
                        End With
                    End If
                Next

                .Item.Add("Shape Override", Me.GraphicObject, "ShapeOverride", False, "4. Parameters", "Overrides the graphical representation of the object in the Flowsheet.", True)
                .Item.Add("Recalculate Output Streams", Me, "RecalcOutputStreams", False, "4. Parameters", "Recalculate output streams using the selected property package.", True)

                If Not TryCast(_couo, ICapeKineticReactionContext) Is Nothing Then
                    .Item.Add(DWSIM.App.GetLocalString("RConvPGridItem1"), FlowSheet.Options.ReactionSets(Me.ReactionSetID).Name, False, "4. Parameters", DWSIM.App.GetLocalString("RConvPGridItem1Help"), True)
                    With .Item(.Item.Count - 1)
                        .CustomEditor = New DWSIM.Editors.Reactors.UIReactionSetSelector
                        .IsDropdownResizable = True
                    End With
                End If

                'populate parameters
                For Each p As Object In _params
                    Dim id As String = ""
                    Dim desc As String = ""
                    id = CType(p, ICapeIdentification).ComponentName
                    desc = CType(p, ICapeIdentification).ComponentDescription
                    'find parameter type
                    Dim myp As ICapeParameterSpec = TryCast(p, ICapeParameterSpec)
                    Select Case myp.Type
                        Case CapeParamType.CAPE_ARRAY
                            Dim par As ICapeParameter = p
                            .Item.Add(id, par.value, If(par.Mode = CapeParamMode.CAPE_OUTPUT, True, False), "4. Parameters", desc, True)
                        Case CapeParamType.CAPE_BOOLEAN
                            Dim par As BooleanParameter = TryCast(p, BooleanParameter)
                            .Item.Add(id, par, "Value", If(par.Mode = CapeParamMode.CAPE_OUTPUT, True, False), "4. Parameters", desc, True)
                        Case CapeParamType.CAPE_INT
                            Dim par As IntegerParameter = TryCast(p, IntegerParameter)
                            .Item.Add(id, par, "Value", If(par.Mode = CapeParamMode.CAPE_OUTPUT, True, False), "4. Parameters", desc, True)
                        Case CapeParamType.CAPE_OPTION
                            Dim par As OptionParameter = TryCast(p, OptionParameter)
                            .Item.Add(id, par, "Value", If(par.Mode = CapeParamMode.CAPE_OUTPUT, True, False), "4. Parameters", desc, True)
                            With .Item(.Item.Count - 1)
                                If Not par.OptionList Is Nothing Then .Choices = New PropertyGridEx.CustomChoices(par.OptionList, False)
                            End With
                        Case CapeParamType.CAPE_REAL
                            Dim par As RealParameter = TryCast(p, RealParameter)
                            .Item.Add(id, par, "SIValue", If(par.Mode = CapeParamMode.CAPE_OUTPUT, True, False), "4. Parameters", desc, True)
                    End Select
                Next

                If Me.GraphicObject.Calculated = False Then
                    .Item.Add(DWSIM.App.GetLocalString("Mensagemdeerro"), Me, "ErrorMessage", True, DWSIM.App.GetLocalString("Miscelnea5"), DWSIM.App.GetLocalString("Mensagemretornadaqua"), True)
                    With .Item(.Item.Count - 1)
                        .DefaultType = GetType(System.String)
                    End With
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

            If e.ChangedItem.Parent.Label.Contains("Parameters") Then
                RestoreParams()
                If Me.FlowSheet.Options.CalculatorActivated Then

                    Me.GraphicObject.Calculated = True

                    'Call function to calculate flowsheet
                    Dim objargs As New DWSIM.Outros.StatusChangeEventArgs
                    With objargs
                        .Calculado = False
                        .Nome = Me.GraphicObject.Name
                        .Tag = Me.GraphicObject.Tag
                        .Tipo = TipoObjeto.CapeOpenUO
                        .Emissor = "PropertyGrid"
                    End With

                    Me.FlowSheet.CalculationQueue.Enqueue(objargs)

                    Me.FlowSheet.FormSurface.UpdateSelectedObject()
                    Me.FlowSheet.FormSurface.FlowsheetDesignSurface.Invalidate()
                    Application.DoEvents()
                    If Me.FlowSheet.Options.CalculatorActivated Then ProcessCalculationQueue(Me.FlowSheet)

                End If
            ElseIf e.ChangedItem.Parent.Label.Contains("Ports") Then
                Dim index, indexc, i As Integer
                i = 0
                For Each gi As GridItem In e.ChangedItem.Parent.GridItems
                    If gi.Label = e.ChangedItem.Label Then
                        index = i
                        Exit For
                    End If
                    i += 1
                Next
                If e.ChangedItem.Label.Contains("[CAPE_INLET, CAPE_MATERIAL]") Then
                    For Each p As UnitPort In _ports
                        i = 0
                        If e.ChangedItem.Label.Contains(p.ComponentName) Then
                            For Each c As ConnectionPoint In Me.GraphicObject.InputConnectors
                                If p.ComponentName = c.ConnectorName And _
                                p.direction = CapePortDirection.CAPE_INLET And _
                                p.portType = CapePortType.CAPE_MATERIAL Then
                                    indexc = i
                                    Exit For
                                End If
                                i += 1
                            Next
                        End If
                    Next
                    If e.ChangedItem.Value <> "" Then
                        If FormFlowsheet.SearchSurfaceObjectsByTag(e.ChangedItem.Value, Me.FlowSheet.FormSurface.FlowsheetDesignSurface) Is Nothing Then
                            Dim oguid As String = Me.FlowSheet.FormSurface.AddObjectToSurface(TipoObjeto.MaterialStream, Me.GraphicObject.X - 40, Me.GraphicObject.Y, e.ChangedItem.Value)
                        ElseIf CType(FormFlowsheet.SearchSurfaceObjectsByTag(e.ChangedItem.Value, Me.FlowSheet.FormSurface.FlowsheetDesignSurface), GraphicObject).OutputConnectors(0).IsAttached Then
                            MessageBox.Show(DWSIM.App.GetLocalString("Todasasconexespossve"), DWSIM.App.GetLocalString("Erro"), MessageBoxButtons.OK, MessageBoxIcon.Error)
                            Exit Sub
                        End If
                        If Me.GraphicObject.InputConnectors(indexc).IsAttached Then
                            Me.FlowSheet.DisconnectObject(Me.GraphicObject.InputConnectors(indexc).AttachedConnector.AttachedFrom, Me.GraphicObject)
                            _ports(index).Disconnect()
                        End If
                        Me.FlowSheet.ConnectObject(FormFlowsheet.SearchSurfaceObjectsByTag(e.ChangedItem.Value, Me.FlowSheet.FormSurface.FlowsheetDesignSurface), Me.GraphicObject, 0, indexc)
                        _ports(index).Connect(Me.FlowSheet.GetFlowsheetSimulationObject(e.ChangedItem.Value))
                    Else
                        If e.OldValue.ToString <> "" Then
                            Me.FlowSheet.DisconnectObject(Me.GraphicObject.InputConnectors(indexc).AttachedConnector.AttachedFrom, Me.GraphicObject)
                            _ports(index).Disconnect()
                        End If
                    End If
                ElseIf e.ChangedItem.Label.Contains("[CAPE_OUTLET, CAPE_MATERIAL]") Then
                    For Each p As UnitPort In _ports
                        i = 0
                        If e.ChangedItem.Label.Contains(p.ComponentName) Then
                            For Each c As ConnectionPoint In Me.GraphicObject.OutputConnectors
                                If p.ComponentName = c.ConnectorName And _
                                p.direction = CapePortDirection.CAPE_OUTLET And _
                                p.portType = CapePortType.CAPE_MATERIAL Then
                                    indexc = i
                                    Exit For
                                End If
                                i += 1
                            Next
                        End If
                    Next
                    If e.ChangedItem.Value <> "" Then
                        If FormFlowsheet.SearchSurfaceObjectsByTag(e.ChangedItem.Value, Me.FlowSheet.FormSurface.FlowsheetDesignSurface) Is Nothing Then
                            Dim oguid As String = Me.FlowSheet.FormSurface.AddObjectToSurface(TipoObjeto.MaterialStream, Me.GraphicObject.X + Me.GraphicObject.Width + 40, Me.GraphicObject.Y, e.ChangedItem.Value)
                        ElseIf CType(FormFlowsheet.SearchSurfaceObjectsByTag(e.ChangedItem.Value, Me.FlowSheet.FormSurface.FlowsheetDesignSurface), GraphicObject).InputConnectors(0).IsAttached Then
                            MessageBox.Show(DWSIM.App.GetLocalString("Todasasconexespossve"), DWSIM.App.GetLocalString("Erro"), MessageBoxButtons.OK, MessageBoxIcon.Error)
                            Exit Sub
                        End If
                        If Me.GraphicObject.OutputConnectors(indexc).IsAttached Then
                            Me.FlowSheet.DisconnectObject(Me.GraphicObject, Me.GraphicObject.OutputConnectors(indexc).AttachedConnector.AttachedTo)
                            _ports(index).Disconnect()
                        End If
                        Me.FlowSheet.ConnectObject(Me.GraphicObject, FormFlowsheet.SearchSurfaceObjectsByTag(e.ChangedItem.Value, Me.FlowSheet.FormSurface.FlowsheetDesignSurface), indexc, 0)
                        _ports(index).Connect(Me.FlowSheet.GetFlowsheetSimulationObject(e.ChangedItem.Value))
                    Else
                        If e.OldValue.ToString <> "" Then
                            Me.FlowSheet.DisconnectObject(Me.GraphicObject, Me.GraphicObject.OutputConnectors(indexc).AttachedConnector.AttachedTo)
                            _ports(index).Disconnect()
                        End If
                    End If
                ElseIf e.ChangedItem.Label.Contains("[CAPE_INLET, CAPE_ENERGY]") Then
                    For Each p As UnitPort In _ports
                        i = 0
                        If e.ChangedItem.Label.Contains(p.ComponentName) Then
                            For Each c As ConnectionPoint In Me.GraphicObject.InputConnectors
                                If p.ComponentName = c.ConnectorName And _
                                p.direction = CapePortDirection.CAPE_INLET And _
                                p.portType = CapePortType.CAPE_ENERGY Then
                                    indexc = i
                                    Exit For
                                End If
                                i += 1
                            Next
                        End If
                    Next
                    If e.ChangedItem.Value <> "" Then
                        If FormFlowsheet.SearchSurfaceObjectsByTag(e.ChangedItem.Value, Me.FlowSheet.FormSurface.FlowsheetDesignSurface) Is Nothing Then
                            Dim oguid As String = Me.FlowSheet.FormSurface.AddObjectToSurface(TipoObjeto.EnergyStream, Me.GraphicObject.X - 40, Me.GraphicObject.Y, e.ChangedItem.Value)
                        ElseIf CType(FormFlowsheet.SearchSurfaceObjectsByTag(e.ChangedItem.Value, Me.FlowSheet.FormSurface.FlowsheetDesignSurface), GraphicObject).OutputConnectors(0).IsAttached Then
                            MessageBox.Show(DWSIM.App.GetLocalString("Todasasconexespossve"), DWSIM.App.GetLocalString("Erro"), MessageBoxButtons.OK, MessageBoxIcon.Error)
                            Exit Sub
                        End If
                        If Me.GraphicObject.InputConnectors(indexc).IsAttached Then
                            Me.FlowSheet.DisconnectObject(Me.GraphicObject.InputConnectors(indexc).AttachedConnector.AttachedFrom, Me.GraphicObject)
                            _ports(index).Disconnect()
                        End If
                        Me.FlowSheet.ConnectObject(FormFlowsheet.SearchSurfaceObjectsByTag(e.ChangedItem.Value, Me.FlowSheet.FormSurface.FlowsheetDesignSurface), Me.GraphicObject, 0, indexc)
                        _ports(index).Connect(Me.FlowSheet.GetFlowsheetSimulationObject(e.ChangedItem.Value))
                    Else
                        If e.OldValue.ToString <> "" Then
                            Me.FlowSheet.DisconnectObject(Me.GraphicObject.InputConnectors(indexc).AttachedConnector.AttachedFrom, Me.GraphicObject)
                            _ports(index).Disconnect()
                        End If
                    End If
                ElseIf e.ChangedItem.Label.Contains("[CAPE_OUTLET, CAPE_ENERGY]") Then
                    For Each p As UnitPort In _ports
                        i = 0
                        If e.ChangedItem.Label.Contains(p.ComponentName) Then
                            For Each c As ConnectionPoint In Me.GraphicObject.OutputConnectors
                                If p.ComponentName = c.ConnectorName And _
                                p.direction = CapePortDirection.CAPE_OUTLET And _
                                p.portType = CapePortType.CAPE_ENERGY Then
                                    indexc = i
                                    Exit For
                                End If
                                i += 1
                            Next
                        End If
                    Next
                    If e.ChangedItem.Value <> "" Then
                        If FormFlowsheet.SearchSurfaceObjectsByTag(e.ChangedItem.Value, Me.FlowSheet.FormSurface.FlowsheetDesignSurface) Is Nothing Then
                            Dim oguid As String = Me.FlowSheet.FormSurface.AddObjectToSurface(TipoObjeto.EnergyStream, Me.GraphicObject.X + Me.GraphicObject.Width + 40, Me.GraphicObject.Y, e.ChangedItem.Value)
                        ElseIf CType(FormFlowsheet.SearchSurfaceObjectsByTag(e.ChangedItem.Value, Me.FlowSheet.FormSurface.FlowsheetDesignSurface), GraphicObject).InputConnectors(0).IsAttached Then
                            MessageBox.Show(DWSIM.App.GetLocalString("Todasasconexespossve"), DWSIM.App.GetLocalString("Erro"), MessageBoxButtons.OK, MessageBoxIcon.Error)
                            Exit Sub
                        End If
                        If Me.GraphicObject.OutputConnectors(indexc).IsAttached Then
                            Me.FlowSheet.DisconnectObject(Me.GraphicObject, Me.GraphicObject.OutputConnectors(indexc).AttachedConnector.AttachedTo)
                            _ports(index).Disconnect()
                        End If
                        Me.FlowSheet.ConnectObject(Me.GraphicObject, FormFlowsheet.SearchSurfaceObjectsByTag(e.ChangedItem.Value, Me.FlowSheet.FormSurface.FlowsheetDesignSurface), indexc, 0)
                        _ports(index).Connect(Me.FlowSheet.GetFlowsheetSimulationObject(e.ChangedItem.Value))
                    Else
                        If e.OldValue.ToString <> "" Then
                            Me.FlowSheet.DisconnectObject(Me.GraphicObject, Me.GraphicObject.OutputConnectors(indexc).AttachedConnector.AttachedTo)
                            _ports(index).Disconnect()
                        End If
                    End If
                End If
                UpdateConnectorPositions()
                RestorePorts()
                Calculate()
            End If
        End Sub

        Public Overrides Function GetProperties(ByVal proptype As SimulationObjects_BaseClass.PropertyType) As String()
            If Not Me._params Is Nothing Then
                Dim props As New ArrayList
                For Each p As ICapeIdentification In Me._params
                    props.Add(p.ComponentName)
                Next
                Return props.ToArray(Type.GetType("System.String"))
            Else
                Return New String() {Nothing}
            End If
        End Function

        Public Overrides Function GetPropertyUnit(ByVal prop As String, Optional ByVal su As SistemasDeUnidades.Unidades = Nothing) As Object
            Return ""
        End Function

        Public Overrides Function GetPropertyValue(ByVal prop As String, Optional ByVal su As SistemasDeUnidades.Unidades = Nothing) As Object
            If Not Me._params Is Nothing Then
                For Each p As ICapeIdentification In Me._params
                    If p.ComponentName = prop Then
                        Return CType(p, ICapeParameter).value
                        Exit Function
                    End If
                Next
                Return Nothing
            Else
                Return Nothing
            End If
        End Function

        Public Overrides Sub QTFillNodeItems()
            Me.QTNodeTableItems = New System.Collections.Generic.Dictionary(Of Integer, DWSIM.Outros.NodeItem)
            If Not _seluo Is Nothing Then
                With _seluo
                    Me.QTNodeTableItems.Add(0, New Outros.NodeItem("Name", .Name, "", 0, 0, Nothing))
                    Me.QTNodeTableItems.Add(1, New Outros.NodeItem("Type Name", .TypeName, "", 1, 0, Nothing))
                    Me.QTNodeTableItems.Add(2, New Outros.NodeItem("Version", .Version, "", 2, 0, Nothing))
                End With
            End If
        End Sub

        Public Overrides Function SetPropertyValue(ByVal prop As String, ByVal propval As Object, Optional ByVal su As SistemasDeUnidades.Unidades = Nothing) As Object
            For Each p As ICapeIdentification In Me._params
                If p.ComponentName = prop Then
                    CType(p, ICapeParameter).value = propval
                    RestoreParams()
                    Return 1
                End If
            Next
            Return 0
        End Function

        Public Overrides Sub UpdatePropertyNodes(ByVal su As SistemasDeUnidades.Unidades, ByVal nf As String)

            Me.QTFillNodeItems()

        End Sub

#End Region

#Region "    IDisposable Overload"

        Protected Overrides Sub Dispose(ByVal disposing As Boolean)
            ' Check to see if Dispose has already been called.
            If Not Me.disposedValue Then
                ' If disposing equals true, dispose all managed 
                ' and unmanaged resources.
                If disposing Then
                    ' Dispose managed resources.
                End If

                ' Call the appropriate methods to clean up 
                ' unmanaged resources here.
                ' If disposing is false, 
                ' only the following code is executed.
                If Not _couo Is Nothing Then
                    Terminate()
                    If Marshal.IsComObject(_couo) Then Marshal.ReleaseComObject(_couo)
                End If

                ' Note disposing has been done.
                disposedValue = True

            End If
        End Sub

#End Region

    End Class

End Namespace

Namespace DWSIM.SimulationObjects.UnitOps.Auxiliary.CapeOpen

    <System.Serializable()> Public Class CapeOpenUnitOpInfo
        Public TypeName As String = ""
        Public Version As String = ""
        Public Description As String = ""
        Public HelpURL As String = ""
        Public VendorURL As String = ""
        Public AboutInfo As String = ""
        Public CapeVersion As String = ""
        Public Location As String = ""
        Public Name As String = ""
        Public ImplementedCategory As String = ""
    End Class

    ' System.Drawing.ComIStreamWrapper.cs
    '
    ' Author:
    ' Kornél Pál <http://www.kornelpal.hu/>
    '
    ' Copyright (C) 2005-2008 Kornél Pál
    '

    '
    ' Permission is hereby granted, free of charge, to any person obtaining
    ' a copy of this software and associated documentation files (the
    ' "Software"), to deal in the Software without restriction, including
    ' without limitation the rights to use, copy, modify, merge, publish,
    ' distribute, sublicense, and/or sell copies of the Software, and to
    ' permit persons to whom the Software is furnished to do so, subject to
    ' the following conditions:
    '
    ' The above copyright notice and this permission notice shall be
    ' included in all copies or substantial portions of the Software.
    '
    ' THE SOFTWARE IS PROVIDED "AS IS", WITHOUT WARRANTY OF ANY KIND,
    ' EXPRESS OR IMPLIED, INCLUDING BUT NOT LIMITED TO THE WARRANTIES OF
    ' MERCHANTABILITY, FITNESS FOR A PARTICULAR PURPOSE AND
    ' NONINFRINGEMENT. IN NO EVENT SHALL THE AUTHORS OR COPYRIGHT HOLDERS BE
    ' LIABLE FOR ANY CLAIM, DAMAGES OR OTHER LIABILITY, WHETHER IN AN ACTION
    ' OF CONTRACT, TORT OR OTHERWISE, ARISING FROM, OUT OF OR IN CONNECTION
    ' WITH THE SOFTWARE OR THE USE OR OTHER DEALINGS IN THE SOFTWARE.
    '
    <System.Serializable()> Friend NotInheritable Class ComIStreamWrapper

        Implements IStream

        Private Const STG_E_INVALIDFUNCTION As Integer = CInt(&H80030001)

        Public ReadOnly baseStream As Stream
        Private position As Long = -1

        Friend Sub New(ByVal stream As Stream)
            baseStream = stream
        End Sub

        Private Sub SetSizeToPosition()
            If position <> -1 Then
                If position > baseStream.Length Then
                    baseStream.SetLength(position)
                End If
                baseStream.Position = position
                position = -1
            End If
        End Sub

        Public Sub Read(ByVal pv As Byte(), ByVal cb As Integer, ByVal pcbRead As IntPtr) Implements IStream.Read
            Dim read__1 As Integer = 0

            If cb <> 0 Then
                SetSizeToPosition()
                read__1 = baseStream.Read(pv, 0, cb)
            End If

            If pcbRead <> IntPtr.Zero Then
                Marshal.WriteInt32(pcbRead, read__1)
            End If
        End Sub

        Public Sub Write(ByVal pv As Byte(), ByVal cb As Integer, ByVal pcbWritten As IntPtr) Implements IStream.Write
            If cb <> 0 Then
                SetSizeToPosition()
                baseStream.Write(pv, 0, cb)
            End If

            If pcbWritten <> IntPtr.Zero Then
                Marshal.WriteInt32(pcbWritten, cb)
            End If
        End Sub

        Public Sub Seek(ByVal dlibMove As Long, ByVal dwOrigin As Integer, ByVal plibNewPosition As IntPtr) Implements IStream.Seek
            Dim length As Long = baseStream.Length
            Dim newPosition As Long

            Select Case CType(dwOrigin, SeekOrigin)
                Case SeekOrigin.Begin
                    newPosition = dlibMove
                    Exit Select
                Case SeekOrigin.Current
                    If position = -1 Then
                        newPosition = baseStream.Position + dlibMove
                    Else
                        newPosition = position + dlibMove
                    End If
                    Exit Select
                Case SeekOrigin.[End]
                    newPosition = length + dlibMove
                    Exit Select
                Case Else
                    Throw New ExternalException(Nothing, STG_E_INVALIDFUNCTION)
            End Select

            If newPosition > length Then
                position = newPosition
            Else
                baseStream.Position = newPosition
                position = -1
            End If

            If plibNewPosition <> IntPtr.Zero Then
                Marshal.WriteInt64(plibNewPosition, newPosition)
            End If
        End Sub

        Public Sub SetSize(ByVal libNewSize As Long) Implements IStream.SetSize
            baseStream.SetLength(libNewSize)
        End Sub

        Public Sub CopyTo(ByVal pstm As IStream, ByVal cb As Long, ByVal pcbRead As IntPtr, ByVal pcbWritten As IntPtr) Implements IStream.CopyTo
            Dim buffer As Byte()
            Dim written As Long = 0
            Dim read As Integer
            Dim count As Integer

            If cb <> 0 Then
                If cb < 4096 Then
                    count = CInt(cb)
                Else
                    count = 4096
                End If
                buffer = New Byte(count - 1) {}
                SetSizeToPosition()
                While True
                    If (InlineAssignHelper(read, baseStream.Read(buffer, 0, count))) = 0 Then
                        Exit While
                    End If
                    pstm.Write(buffer, read, IntPtr.Zero)
                    written += read
                    If written >= cb Then
                        Exit While
                    End If
                    If cb - written < 4096 Then
                        count = CInt(cb - written)
                    End If
                End While
            End If

            If pcbRead <> IntPtr.Zero Then
                Marshal.WriteInt64(pcbRead, written)
            End If
            If pcbWritten <> IntPtr.Zero Then
                Marshal.WriteInt64(pcbWritten, written)
            End If
        End Sub

        Public Sub Commit(ByVal grfCommitFlags As Integer) Implements IStream.Commit
            baseStream.Flush()
            SetSizeToPosition()
        End Sub

        Public Sub Revert() Implements IStream.Revert
            Throw New Runtime.InteropServices.ExternalException(Nothing, STG_E_INVALIDFUNCTION)
        End Sub

        Public Sub LockRegion(ByVal libOffset As Long, ByVal cb As Long, ByVal dwLockType As Integer) Implements IStream.LockRegion
            Throw New Runtime.InteropServices.ExternalException(Nothing, STG_E_INVALIDFUNCTION)
        End Sub

        Public Sub UnlockRegion(ByVal libOffset As Long, ByVal cb As Long, ByVal dwLockType As Integer) Implements IStream.UnlockRegion
            Throw New Runtime.InteropServices.ExternalException(Nothing, STG_E_INVALIDFUNCTION)
        End Sub

        Public Sub Stat(ByRef pstatstg As System.Runtime.InteropServices.ComTypes.STATSTG, ByVal grfStatFlag As Integer) Implements IStream.Stat
            pstatstg = New System.Runtime.InteropServices.ComTypes.STATSTG()
            pstatstg.cbSize = baseStream.Length
        End Sub

        Public Sub Clone(ByRef ppstm As IStream) Implements IStream.Clone
            ppstm = Nothing
            Throw New Runtime.InteropServices.ExternalException(Nothing, STG_E_INVALIDFUNCTION)
        End Sub

        Private Shared Function InlineAssignHelper(Of T)(ByRef target As T, ByVal value As T) As T
            target = value
            Return value
        End Function

    End Class

    <System.Serializable()> Public Class RealParameter

        Implements ICapeIdentification, ICapeParameter, ICapeParameterSpec, ICapeRealParameterSpec

        Dim _par As Global.CapeOpen.RealParameter

        Public Event OnParameterValueChanged(ByVal sender As Object, ByVal args As System.EventArgs)

        Sub New(ByVal name As String, ByVal value As Double, ByVal defaultvalue As Double, ByVal unit As String)
            _par = New Global.CapeOpen.RealParameter(name, value, defaultvalue, unit)
        End Sub

        Public Property ComponentDescription() As String Implements Global.CapeOpen.ICapeIdentification.ComponentDescription
            Get
                Return _par.ComponentDescription
            End Get
            Set(ByVal value As String)
                _par.ComponentDescription = value
            End Set
        End Property

        Public Property ComponentName() As String Implements Global.CapeOpen.ICapeIdentification.ComponentName
            Get
                Return _par.ComponentName
            End Get
            Set(ByVal value As String)
                _par.ComponentName = value
            End Set
        End Property

        Public Property Mode() As Global.CapeOpen.CapeParamMode Implements Global.CapeOpen.ICapeParameter.Mode
            Get
                Return _par.Mode
            End Get
            Set(ByVal value As Global.CapeOpen.CapeParamMode)
                _par.Mode = value
            End Set
        End Property

        Public Sub Reset() Implements Global.CapeOpen.ICapeParameter.Reset
            _par.Reset()
        End Sub

        Public ReadOnly Property Specification() As Object Implements Global.CapeOpen.ICapeParameter.Specification
            Get
                Return Me
            End Get
        End Property

        Public Function Validate(ByRef message As String) As Boolean Implements Global.CapeOpen.ICapeParameter.Validate
            Return _par.Validate(message)
        End Function

        Public ReadOnly Property ValStatus() As Global.CapeOpen.CapeValidationStatus Implements Global.CapeOpen.ICapeParameter.ValStatus
            Get
                Return _par.ValStatus
            End Get
        End Property

        Public Property value() As Object Implements Global.CapeOpen.ICapeParameter.value
            Get
                Return _par.SIValue
            End Get
            Set(ByVal value As Object)
                _par.SIValue = value
                RaiseEvent OnParameterValueChanged(Me, New System.EventArgs())
            End Set
        End Property

        Public ReadOnly Property Dimensionality() As Object Implements Global.CapeOpen.ICapeParameterSpec.Dimensionality
            Get
                Dim myd As ICapeParameterSpec = _par
                Return New Double() {myd.Dimensionality(0), myd.Dimensionality(1), myd.Dimensionality(2), myd.Dimensionality(3), myd.Dimensionality(4), myd.Dimensionality(5), myd.Dimensionality(6), myd.Dimensionality(7)}
            End Get
        End Property

        Public ReadOnly Property Type() As Global.CapeOpen.CapeParamType Implements Global.CapeOpen.ICapeParameterSpec.Type
            Get
                Return _par.Type
            End Get
        End Property

        Public ReadOnly Property DefaultValue() As Double Implements Global.CapeOpen.ICapeRealParameterSpec.DefaultValue
            Get
                Return _par.DefaultValue
            End Get
        End Property

        Public ReadOnly Property LowerBound() As Double Implements Global.CapeOpen.ICapeRealParameterSpec.LowerBound
            Get
                Return _par.LowerBound
            End Get
        End Property

        Public ReadOnly Property UpperBound() As Double Implements Global.CapeOpen.ICapeRealParameterSpec.UpperBound
            Get
                Return _par.UpperBound
            End Get
        End Property

        Public Function Validate1(ByVal value As Double, ByRef message As String) As Boolean Implements Global.CapeOpen.ICapeRealParameterSpec.Validate
            Return _par.Validate(value, message)
        End Function
    End Class

    <System.Serializable()> Public Class CCapeCollection

        Implements ICapeCollection

        Public _icol As List(Of ICapeIdentification)

        Sub New()
            _icol = New List(Of ICapeIdentification)
        End Sub

        Public Function Count() As Integer Implements Global.CapeOpen.ICapeCollection.Count
            Return _icol.Count
        End Function

        Public Function Item(ByVal index As Object) As Object Implements Global.CapeOpen.ICapeCollection.Item
            If Integer.TryParse(index, New Integer) Then
                Return _icol(index - 1)
            Else
                For Each p As ICapeIdentification In _icol
                    If p.ComponentName = index Then Return p Else Return Nothing
                Next
                Return Nothing
            End If
        End Function
    End Class

End Namespace