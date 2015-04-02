'    Custom (Scripting) Unit Operation Calculation Routines 
'    Copyright 2010-2011 Daniel Wagner O. de Medeiros
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
Imports System.Runtime.InteropServices
Imports CapeOpen
Imports System.Runtime.Serialization.Formatters
Imports LuaInterface

Namespace DWSIM.SimulationObjects.UnitOps

    <Guid(CustomUO.ClassId)> <System.Serializable()> <ComVisible(True)> Public Class CustomUO

        Inherits SimulationObjects_UnitOpBaseClass

        Private _scripttext As String = ""
        Private _scriptlanguage As scriptlanguage = scriptlanguage.IronPython
        Private _includes() As String
        Private _fontname As String = "Courier New"
        Private _fontsize As Integer = 10

        Public Shadows Const ClassId As String = "1FD2DC53-DC7B-4c4d-BBEE-F37F4E5ADDFB"

#Region "   DWSIM Methods"

        Public Property FontName() As String
            Get
                Return _fontname
            End Get
            Set(ByVal value As String)
                _fontname = value
            End Set
        End Property

        Public Property FontSize() As Integer
            Get
                Return _fontsize
            End Get
            Set(ByVal value As Integer)
                _fontsize = value
            End Set
        End Property

        Public Property Includes() As String()
            Get
                Return _includes
            End Get
            Set(ByVal value As String())
                _includes = value
            End Set
        End Property

        Public Property ScriptText() As String
            Get
                Return _scripttext
            End Get
            Set(ByVal value As String)
                _scripttext = value
            End Set
        End Property

        Public Property Language() As scriptlanguage
            Get
                Return _scriptlanguage
            End Get
            Set(ByVal value As scriptlanguage)
                _scriptlanguage = value
            End Set
        End Property

        Public Sub New()
            MyBase.New()
        End Sub

        Public Sub New(ByVal nome As String, ByVal descricao As String)
            MyBase.CreateNew()
            Me.m_ComponentName = nome
            Me.m_ComponentDescription = descricao
            Me.FillNodeItems()
            Me.QTFillNodeItems()
        End Sub

        Public Overrides Function Calculate(Optional ByVal args As Object = Nothing) As Integer

            Dim ims1, ims2, ims3, oms1, oms2, oms3 As SimulationObjects.Streams.MaterialStream
            If Me.GraphicObject.InputConnectors(0).IsAttached Then
                ims1 = FlowSheet.Collections.CLCS_MaterialStreamCollection(Me.GraphicObject.InputConnectors(0).AttachedConnector.AttachedFrom.Name)
            Else
                ims1 = Nothing
            End If
            If Me.GraphicObject.InputConnectors(1).IsAttached Then
                ims2 = FlowSheet.Collections.CLCS_MaterialStreamCollection(Me.GraphicObject.InputConnectors(1).AttachedConnector.AttachedFrom.Name)
            Else
                ims2 = Nothing
            End If
            If Me.GraphicObject.InputConnectors(2).IsAttached Then
                ims3 = FlowSheet.Collections.CLCS_MaterialStreamCollection(Me.GraphicObject.InputConnectors(2).AttachedConnector.AttachedFrom.Name)
            Else
                ims3 = Nothing
            End If
            If Me.GraphicObject.OutputConnectors(0).IsAttached Then
                oms1 = FlowSheet.Collections.CLCS_MaterialStreamCollection(Me.GraphicObject.OutputConnectors(0).AttachedConnector.AttachedTo.Name)
            Else
                oms1 = Nothing
            End If
            If Me.GraphicObject.OutputConnectors(1).IsAttached Then
                oms2 = FlowSheet.Collections.CLCS_MaterialStreamCollection(Me.GraphicObject.OutputConnectors(1).AttachedConnector.AttachedTo.Name)
            Else
                oms2 = Nothing
            End If
            If Me.GraphicObject.OutputConnectors(2).IsAttached Then
                oms3 = FlowSheet.Collections.CLCS_MaterialStreamCollection(Me.GraphicObject.OutputConnectors(2).AttachedConnector.AttachedTo.Name)
            Else
                oms3 = Nothing
            End If

            Dim ies1, oes1 As SimulationObjects.Streams.EnergyStream
            If Me.GraphicObject.InputConnectors(3).IsAttached Then
                ies1 = FlowSheet.Collections.CLCS_EnergyStreamCollection(Me.GraphicObject.InputConnectors(3).AttachedConnector.AttachedFrom.Name)
            Else
                ies1 = Nothing
            End If
            If Me.GraphicObject.OutputConnectors(3).IsAttached Then
                oes1 = FlowSheet.Collections.CLCS_EnergyStreamCollection(Me.GraphicObject.OutputConnectors(3).AttachedConnector.AttachedTo.Name)
            Else
                oes1 = Nothing
            End If

            Select Case Language
                Case 4
                    Dim lscript As New Lua
                    lscript("Flowsheet") = FlowSheet
                    lscript("Spreadsheet") = FlowSheet.FormSpreadsheet
                    lscript("Plugins") = FormMain.UtilityPlugins
                    Dim Solver As New DWSIM.Flowsheet.FlowsheetSolver
                    lscript("Solver") = Solver
                    lscript("Me") = Me
                    If Me.GraphicObject.InputConnectors(0).IsAttached Then lscript("ims1") = ims1
                    If Me.GraphicObject.InputConnectors(1).IsAttached Then lscript("ims2") = ims2
                    If Me.GraphicObject.InputConnectors(2).IsAttached Then lscript("ims3") = ims3
                    If Me.GraphicObject.OutputConnectors(0).IsAttached Then lscript("oms1") = oms1
                    If Me.GraphicObject.OutputConnectors(1).IsAttached Then lscript("oms2") = oms2
                    If Me.GraphicObject.OutputConnectors(2).IsAttached Then lscript("oms3") = oms3
                    If Me.GraphicObject.InputConnectors(3).IsAttached Then lscript("ies1") = ies1
                    If Me.GraphicObject.OutputConnectors(3).IsAttached Then lscript("oes1") = oes1
                    lscript("DWSIM") = GetType(DWSIM.ClassesBasicasTermodinamica.Fase).Assembly
                    Try
                        Dim txtcode As String = ""
                        If Not Includes Is Nothing Then
                            For Each fname As String In Me.Includes
                                txtcode += File.ReadAllText(fname) + vbCrLf
                            Next
                        End If
                        txtcode += Me.ScriptText
                        lscript.DoString(txtcode)
                    Catch ex As Exception
                        Me.ErrorMessage = ex.ToString
                        Me.DeCalculate()
                        lscript = Nothing
                        Throw ex
                    End Try
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
                    scope.SetVariable("Plugins", FormMain.UtilityPlugins)
                    scope.SetVariable("Me", Me)
                    If Me.GraphicObject.InputConnectors(0).IsAttached Then scope.SetVariable("ims1", ims1)
                    If Me.GraphicObject.InputConnectors(1).IsAttached Then scope.SetVariable("ims2", ims2)
                    If Me.GraphicObject.InputConnectors(2).IsAttached Then scope.SetVariable("ims3", ims3)
                    If Me.GraphicObject.OutputConnectors(0).IsAttached Then scope.SetVariable("oms1", oms1)
                    If Me.GraphicObject.OutputConnectors(1).IsAttached Then scope.SetVariable("oms2", oms2)
                    If Me.GraphicObject.OutputConnectors(2).IsAttached Then scope.SetVariable("oms3", oms3)
                    If Me.GraphicObject.InputConnectors(3).IsAttached Then scope.SetVariable("ies1", ies1)
                    If Me.GraphicObject.OutputConnectors(3).IsAttached Then scope.SetVariable("oes1", oes1)
                    Dim Solver As New DWSIM.Flowsheet.FlowsheetSolver
                    scope.SetVariable("Solver", Solver)
                    Dim txtcode As String = ""
                    If Not Includes Is Nothing Then
                        For Each fname As String In Me.Includes
                            txtcode += File.ReadAllText(fname) + vbCrLf
                        Next
                    End If
                    txtcode += Me.ScriptText
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

            If Not oes1 Is Nothing Then
                oes1.GraphicObject.Calculated = True
            End If

            'Call function to calculate flowsheet
            Dim objargs As New DWSIM.Outros.StatusChangeEventArgs
            With objargs
                .Calculado = True
                .Nome = Me.Nome
                .Tag = Me.GraphicObject.Tag
                .Tipo = TipoObjeto.CustomUO
            End With

            FlowSheet.CalculationQueue.Enqueue(objargs)

        End Function

        Public Overrides Function DeCalculate() As Integer

            'Call function to calculate flowsheet
            Dim objargs As New DWSIM.Outros.StatusChangeEventArgs
            With objargs
                .Calculado = False
                .Nome = Me.Nome
                .Tag = Me.GraphicObject.Tag
                .Tipo = TipoObjeto.CustomUO
            End With

            FlowSheet.CalculationQueue.Enqueue(objargs)

        End Function

        Public Overrides Sub Validate()
            MyBase.Validate()
        End Sub

        Public Overrides Sub PopulatePropertyGrid(ByRef pgrid As PropertyGridEx.PropertyGridEx, ByVal su As SistemasDeUnidades.Unidades)

            Dim Conversor As New DWSIM.SistemasDeUnidades.Conversor

            With pgrid

                .PropertySort = PropertySort.Categorized
                .ShowCustomProperties = True
                .Item.Clear()

                MyBase.PopulatePropertyGrid(pgrid, su)

                Dim ent1, ent2, ent3, ent4 As String

                If Me.GraphicObject.InputConnectors(0).IsAttached = True Then
                    ent1 = Me.GraphicObject.InputConnectors(0).AttachedConnector.AttachedFrom.Tag
                Else
                    ent1 = ""
                End If
                If Me.GraphicObject.InputConnectors(1).IsAttached = True Then
                    ent2 = Me.GraphicObject.InputConnectors(1).AttachedConnector.AttachedFrom.Tag
                Else
                    ent2 = ""
                End If
                If Me.GraphicObject.InputConnectors(2).IsAttached = True Then
                    ent3 = Me.GraphicObject.InputConnectors(2).AttachedConnector.AttachedFrom.Tag
                Else
                    ent3 = ""
                End If
                If Me.GraphicObject.InputConnectors(3).IsAttached = True Then
                    ent4 = Me.GraphicObject.InputConnectors(3).AttachedConnector.AttachedFrom.Tag
                Else
                    ent4 = ""
                End If

                Dim saida1, saida2, saida3, saida4 As String

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
                If Me.GraphicObject.OutputConnectors(3).IsAttached = True Then
                    saida4 = Me.GraphicObject.OutputConnectors(3).AttachedConnector.AttachedTo.Tag
                Else
                    saida4 = ""
                End If

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
                .Item.Add(DWSIM.App.GetLocalString("CorrentedeenergiaE"), ent4, False, DWSIM.App.GetLocalString("Conexes1"), "", True)
                With .Item(.Item.Count - 1)
                    .DefaultValue = Nothing
                    .CustomEditor = New DWSIM.Editors.Streams.UIInputESSelector
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
                .Item.Add(DWSIM.App.GetLocalString("CorrentedeenergiaS"), saida4, False, DWSIM.App.GetLocalString("Conexes1"), "", True)
                With .Item(.Item.Count - 1)
                    .DefaultValue = Nothing
                    .CustomEditor = New DWSIM.Editors.Streams.UIOutputESSelector
                End With

                .Item.Add(DWSIM.App.GetLocalString("CUO_ScriptLanguage"), Me, "Language", False, DWSIM.App.GetLocalString("Parmetrosdeclculo2"), "", True)

                .Item.Add(DWSIM.App.GetLocalString("CUO_ScriptText"), Me, "ScriptText", False, DWSIM.App.GetLocalString("Parmetrosdeclculo2"), DWSIM.App.GetLocalString("Cliquenobotocomretic"), True)
                With .Item(.Item.Count - 1)
                    .CustomEditor = New DWSIM.Editors.CustomUO.UIScriptEditor
                End With

                If Me.GraphicObject.Calculated = False Then
                    .Item.Add(DWSIM.App.GetLocalString("Mensagemdeerro"), Me, "ErrorMessage", True, DWSIM.App.GetLocalString("Miscelnea5"), DWSIM.App.GetLocalString("Mensagemretornadaqua"), True)
                    With .Item(.Item.Count - 1)
                        .DefaultType = GetType(System.String)
                    End With
                End If

            End With


        End Sub

        Public Overrides Sub PropertyValueChanged(ByVal s As Object, ByVal e As System.Windows.Forms.PropertyValueChangedEventArgs)
            MyBase.PropertyValueChanged(s, e)
        End Sub

        Public Overrides Function GetProperties(ByVal proptype As SimulationObjects_BaseClass.PropertyType) As String()
            Return New String() {}
        End Function

        Public Overrides Function GetPropertyUnit(ByVal prop As String, Optional ByVal su As SistemasDeUnidades.Unidades = Nothing) As Object
            Return Nothing
        End Function

        Public Overrides Function GetPropertyValue(ByVal prop As String, Optional ByVal su As SistemasDeUnidades.Unidades = Nothing) As Object
            Return Nothing
        End Function

        Public Overrides Sub QTFillNodeItems()

        End Sub

        Public Overrides Function SetPropertyValue(ByVal prop As String, ByVal propval As Object, Optional ByVal su As SistemasDeUnidades.Unidades = Nothing) As Object
            Return Nothing
        End Function

        Public Overrides Sub UpdatePropertyNodes(ByVal su As SistemasDeUnidades.Unidades, ByVal nf As String)

        End Sub

#End Region

#Region "   CAPE-OPEN Methods"

        Protected WithEvents m_sl As OptionParameter

        Private Sub m_sl_OnParameterValueChanged(ByVal sender As Object, ByVal args As System.EventArgs) Handles m_sl.ParameterValueChanged
            Select Case m_sl.Value
                Case "IronPython"
                    Me._scriptlanguage = scriptlanguage.IronPython
                Case "Lua"
                    Me._scriptlanguage = scriptlanguage.Lua
            End Select
        End Sub

        Public Overrides Sub Initialize()

            My.Application.ChangeUICulture("en-US")

            m_sl = New OptionParameter("Script Language", "Select the scripting language for this Unit Operation.", Me.Language.ToString, "IronPython", New String() {"IronPython", "Lua"}, True, CapeParamMode.CAPE_INPUT)

            'set CAPE-OPEN Mode 
            _capeopenmode = True

            'create port collection
            _ports = New PortCollection()

            ' create ports
            With _ports
                .Add(New UnitPort("Inlet_Port_1", "Material Object Inlet Port 1", CapePortDirection.CAPE_INLET, CapePortType.CAPE_MATERIAL))
                .Add(New UnitPort("Inlet_Port_2", "Material Object Inlet Port 2", CapePortDirection.CAPE_INLET, CapePortType.CAPE_MATERIAL))
                .Add(New UnitPort("Inlet_Port_3", "Material Object Inlet Port 3", CapePortDirection.CAPE_INLET, CapePortType.CAPE_MATERIAL))
                .Add(New UnitPort("Outlet_Port_1", "Material Object Outlet Port 1", CapePortDirection.CAPE_OUTLET, CapePortType.CAPE_MATERIAL))
                .Add(New UnitPort("Outlet_Port_2", "Material Object Outlet Port 2", CapePortDirection.CAPE_OUTLET, CapePortType.CAPE_MATERIAL))
                .Add(New UnitPort("Outlet_Port_3", "Material Object Outlet Port 3", CapePortDirection.CAPE_OUTLET, CapePortType.CAPE_MATERIAL))
                .Add(New UnitPort("Energy_Inlet_Port_1", "Energy Stream Inlet Port", CapePortDirection.CAPE_INLET, CapePortType.CAPE_ENERGY))
                .Add(New UnitPort("Energy_Outlet_Port_1", "Energy Stream Outlet Port", CapePortDirection.CAPE_OUTLET, CapePortType.CAPE_ENERGY))
            End With

            _parameters = New ParameterCollection()

            ' create parameters
            With _parameters
                .Add(m_sl)
            End With

        End Sub

        Public Overrides Sub Edit1()

            Dim edform As New ScriptEditorForm
            With edform
                .language = Me.Language
                .fontname = Me.FontName
                .fontsize = Me.FontSize
                .includes = Me.Includes
                .scripttext = Me.ScriptText
                .ShowDialog()
                Me.FontName = .fontname
                Me.FontSize = .fontsize
                Me.Includes = .includes
                Me.ScriptText = .txtScript.Document.Text
            End With
            edform.Dispose()
            edform = Nothing

        End Sub

        Public Overrides ReadOnly Property ValStatus() As CapeOpen.CapeValidationStatus
            Get
                _valres = "Unit validated successfully."
                Return CapeOpen.CapeValidationStatus.CAPE_VALID
            End Get
        End Property

        Public Overrides Sub Calculate1()

            Dim ims1, ims2, ims3, oms1, oms2, oms3 As ICapeThermoMaterialObject

            ims1 = TryCast(Me._ports(0).connectedObject, ICapeThermoMaterialObject)
            ims2 = TryCast(Me._ports(1).connectedObject, ICapeThermoMaterialObject)
            ims3 = TryCast(Me._ports(2).connectedObject, ICapeThermoMaterialObject)
            oms1 = TryCast(Me._ports(3).connectedObject, ICapeThermoMaterialObject)
            oms2 = TryCast(Me._ports(4).connectedObject, ICapeThermoMaterialObject)
            oms3 = TryCast(Me._ports(5).connectedObject, ICapeThermoMaterialObject)

            Dim ies1, oes1 As ICapeCollection

            ies1 = TryCast(Me._ports(6).connectedObject, ICapeCollection)
            oes1 = TryCast(Me._ports(7).connectedObject, ICapeCollection)

            Select Case Language
                Case 4
                    Dim lscript As New Lua
                    Try
                        lscript("pme") = Me._simcontext
                        lscript("dwsim") = GetType(DWSIM.ClassesBasicasTermodinamica.Fase).Assembly
                        lscript("capeopen") = GetType(ICapeIdentification).Assembly
                        lscript("ims1") = ims1
                        lscript("ims2") = ims2
                        lscript("ims3") = ims3
                        lscript("oms1") = oms1
                        lscript("oms2") = oms2
                        lscript("oms3") = oms3
                        lscript("ies1") = ies1
                        lscript("oes1") = oes1
                        Dim txtcode As String = ""
                        If Not Includes Is Nothing Then
                            For Each fname As String In Me.Includes
                                txtcode += File.ReadAllText(fname) + vbCrLf
                            Next
                        End If
                        txtcode += Me.ScriptText
                        lscript.DoString(txtcode)
                        _lastrun = "script executed succesfully."
                    Catch ex As Exception
                        Me.ErrorMessage = ex.ToString
                        CType(Me._simcontext, ICapeDiagnostic).LogMessage(Me.ErrorMessage)
                        Throw ex
                    Finally
                        Me._calclog = Me.ErrorMessage
                        _lastrun = "error executing script: " & _calclog
                        lscript = Nothing
                    End Try
                Case 2
                    Dim source As Microsoft.Scripting.Hosting.ScriptSource
                    Try
                        engine = IronPython.Hosting.Python.CreateEngine()
                        engine.Runtime.LoadAssembly(GetType(System.String).Assembly)
                        engine.Runtime.LoadAssembly(GetType(CAPEOPEN110.ICapeIdentification).Assembly)
                        engine.Runtime.LoadAssembly(GetType(CapeOpen.ICapeIdentification).Assembly)
                        engine.Runtime.LoadAssembly(GetType(DWSIM.ClassesBasicasTermodinamica.ConstantProperties).Assembly)
                        scope = engine.CreateScope()
                        scope.SetVariable("pme", Me._simcontext)
                        scope.SetVariable("this", Me)
                        scope.SetVariable("ims1", ims1)
                        scope.SetVariable("ims2", ims2)
                        scope.SetVariable("ims3", ims3)
                        scope.SetVariable("oms1", oms1)
                        scope.SetVariable("oms2", oms2)
                        scope.SetVariable("oms3", oms3)
                        scope.SetVariable("ies1", ies1)
                        scope.SetVariable("oes1", oes1)
                        Dim txtcode As String = ""
                        If Not Includes Is Nothing Then
                            For Each fname As String In Me.Includes
                                txtcode += File.ReadAllText(fname) + vbCrLf
                            Next
                        End If
                        txtcode += Me.ScriptText
                        source = Me.engine.CreateScriptSourceFromString(txtcode, Microsoft.Scripting.SourceCodeKind.Statements)
                        Me.ErrorMessage = ""
                        source.Execute(Me.scope)
                        _lastrun = "script executed succesfully."
                    Catch ex As Exception
                        Dim ops As ExceptionOperations = engine.GetService(Of ExceptionOperations)()
                        Me.ErrorMessage = ops.FormatException(ex).ToString
                        CType(Me._simcontext, ICapeDiagnostic).LogMessage(Me.ErrorMessage)
                        engine = Nothing
                        scope = Nothing
                        source = Nothing
                        Throw ex
                    Finally
                        engine = Nothing
                        scope = Nothing
                        source = Nothing
                        Me._calclog = Me.ErrorMessage
                        _lastrun = "error executing script: " & _calclog
                    End Try
            End Select

        End Sub

        Public Overrides Sub Terminate1()
            _ports.Clear()
            _parameters.Clear()
            MyBase.Terminate1()
        End Sub

        Public Overrides Sub Load(ByVal pStm As System.Runtime.InteropServices.ComTypes.IStream)

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

                Me.Language = myarr(0)
                'Me._ports = myarr(1)
                Me.ScriptText = myarr(1)
                Me.FontName = myarr(2)
                Me.FontSize = myarr(3)

                myarr = Nothing
                mySerializer = Nothing

                RemoveHandler domain.AssemblyResolve, New ResolveEventHandler(AddressOf MyResolveEventHandler)

            Catch p_Ex As System.Exception

                System.Windows.Forms.MessageBox.Show(p_Ex.ToString())

            End Try

            memoryStream.Close()

        End Sub

        Public Overrides Sub Save(ByVal pStm As System.Runtime.InteropServices.ComTypes.IStream, ByVal fClearDirty As Boolean)

            Dim props As New ArrayList

            With props

                .Add(Me.Language)
                .Add(Me.ScriptText)
                .Add(Me.FontName)
                .Add(Me.FontSize)

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

#End Region

#Region "   Register/Unregister Procedures"

        <System.Runtime.InteropServices.ComRegisterFunction()> _
       Private Shared Sub RegisterFunction(ByVal t As Type)

            Dim keyname As String = String.Concat("CLSID\\{", t.GUID.ToString, "}\\Implemented Categories")
            Dim key As Microsoft.Win32.RegistryKey = Microsoft.Win32.Registry.ClassesRoot.OpenSubKey(keyname, True)
            If key Is Nothing Then
                key = Microsoft.Win32.Registry.ClassesRoot.CreateSubKey(keyname)
            End If
            key.CreateSubKey("{678C09A5-7D66-11D2-A67D-00105A42887F}") ' CAPE-OPEN Unit Operation
            key.CreateSubKey("{678C09A1-7D66-11D2-A67D-00105A42887F}") ' CAPE-OPEN Object 
            keyname = String.Concat("CLSID\\{", t.GUID.ToString, "}\\CapeDescription")
            key = Microsoft.Win32.Registry.ClassesRoot.CreateSubKey(keyname)
            key.SetValue("Name", "IronPython/Lua Scripting Unit Operation")
            key.SetValue("Description", "DWSIM IronPython/Lua Scripting Unit Operation CAPE-OPEN Wrapper")
            key.SetValue("CapeVersion", "1.0")
            key.SetValue("ComponentVersion", My.Application.Info.Version.ToString)
            key.SetValue("VendorURL", "http://dwsim.inforside.com.br")
            key.SetValue("HelpURL", "http://dwsim.inforside.com.br")
            key.SetValue("About", "DWSIM is open-source software, released under the GPL v3 license. (c) 2011-2014 Daniel Medeiros.")
            key.Close()

        End Sub

        <System.Runtime.InteropServices.ComUnregisterFunction()> _
        Private Shared Sub UnregisterFunction(ByVal t As Type)

            Try

                Dim keyname As String = String.Concat("CLSID\\{", t.GUID.ToString, "}")
                Dim key As Microsoft.Win32.RegistryKey = Microsoft.Win32.Registry.ClassesRoot.OpenSubKey(keyname, True)
                Dim keyNames() As String = key.GetSubKeyNames
                For Each kn As String In keyNames
                    key.DeleteSubKeyTree(kn)
                Next
                Dim valueNames() As String = key.GetValueNames
                For Each valueName As String In valueNames
                    key.DeleteValue(valueName)
                Next

            Catch ex As Exception

            End Try

        End Sub

#End Region

    End Class

End Namespace
