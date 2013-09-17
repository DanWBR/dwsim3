'    Copyright 2008-2013 Daniel Wagner O. de Medeiros
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

'Imports DWSIM.SimulationObjects
Imports System.ComponentModel
Imports FileHelpers
Imports DWSIM.DWSIM.ClassesBasicasTermodinamica
Imports System.Runtime.Serialization.Formatters
Imports System.Runtime.Serialization
Imports System.IO
Imports System.Linq
Imports ICSharpCode.SharpZipLib.Core
Imports ICSharpCode.SharpZipLib.Zip
Imports WeifenLuo.WinFormsUI.Docking
Imports WeifenLuo.WinFormsUI
Imports DWSIM.DWSIM.SimulationObjects.PropertyPackages
Imports System.Runtime.Serialization.Formatters.Binary
Imports Microsoft.Msdn.Samples
Imports Infralution.Localization
Imports System.Globalization
Imports DWSIM.DWSIM.FormClasses
Imports System.Threading.Tasks
Imports System.Xml.Serialization
Imports System.Xml
Imports System.Reflection
Imports Microsoft.Win32
Imports DWSIM.DWSIM.SimulationObjects
Imports System.Text
Imports System.Xml.Linq
Imports Microsoft.Msdn.Samples.GraphicObjects

Public Class FormMain

    Inherits Form

    Public Shared m_childcount As Integer = 1
    Public filename As String
    Public sairdevez As Boolean = False
    Public SairDiretoERRO As Boolean = False
    Public loadedCSDB As Boolean = False
    Public pathsep As Char

    Public WithEvents FrmLoadSave As New FormLS
    Public FrmOptions As FormOptions
    Public FrmRec As FormRecoverFiles

    Private dropdownlist As ArrayList

    Private dlok As Boolean = False

    Private tmpform2 As FormFlowsheet

    Public AvailableComponents As New Dictionary(Of String, DWSIM.ClassesBasicasTermodinamica.ConstantProperties)
    Public AvailableUnitSystems As New Dictionary(Of String, DWSIM.SistemasDeUnidades.Unidades)
    Public PropertyPackages As New Dictionary(Of String, DWSIM.SimulationObjects.PropertyPackages.PropertyPackage)

    Public UtilityPlugins As New Dictionary(Of String, Interfaces.IUtilityPlugin)
    Public COMonitoringObjects As New Dictionary(Of String, DWSIM.SimulationObjects.UnitOps.Auxiliary.CapeOpen.CapeOpenUnitOpInfo)

#Region "    Form Events"

    Private Sub FormMain_DragDrop(ByVal sender As Object, ByVal e As System.Windows.Forms.DragEventArgs) Handles Me.DragDrop
        If e.Data.GetDataPresent(DataFormats.FileDrop) Then
            Dim MyFiles() As String
            Dim i As Integer
            ' Assign the files to an array.
            MyFiles = e.Data.GetData(DataFormats.FileDrop)
            ' Loop through the array and add the files to the list.
            For i = 0 To MyFiles.Length - 1
                Select Case Path.GetExtension(MyFiles(i)).ToLower
                    Case ".dwxml"
                        Me.ToolStripStatusLabel1.Text = DWSIM.App.GetLocalString("Abrindosimulao") + " " + MyFiles(i) + "..."
                        Application.DoEvents()
                        Me.LoadXML(MyFiles(i))
                    Case ".dwxmz"
                        Me.ToolStripStatusLabel1.Text = DWSIM.App.GetLocalString("Abrindosimulao") + " " + MyFiles(i) + "..."
                        Application.DoEvents()
                        Me.LoadAndExtractXMLZIP(MyFiles(i))
                    Case ".dwsim"
                        Me.ToolStripStatusLabel1.Text = DWSIM.App.GetLocalString("Abrindosimulao") + " " + MyFiles(i) + "..."
                        Application.DoEvents()
                        Me.LoadF(MyFiles(i))
                    Case ".dwcsd"
                        Dim NewMDIChild As New FormCompoundCreator()
                        NewMDIChild.MdiParent = Me
                        NewMDIChild.Show()
                        Dim objStreamReader As New FileStream(MyFiles(i), FileMode.Open)
                        Dim x As New BinaryFormatter()
                        NewMDIChild.mycase = x.Deserialize(objStreamReader)
                        objStreamReader.Close()
                        NewMDIChild.WriteData()
                        If Not My.Settings.MostRecentFiles.Contains(MyFiles(i)) Then
                            My.Settings.MostRecentFiles.Add(MyFiles(i))
                            Me.UpdateMRUList()
                        End If
                    Case ".dwrsd"
                        Dim NewMDIChild As New FormDataRegression()
                        NewMDIChild.MdiParent = Me
                        NewMDIChild.Show()
                        Dim objStreamReader As New FileStream(MyFiles(i), FileMode.Open)
                        Dim x As New BinaryFormatter()
                        NewMDIChild.currcase = x.Deserialize(objStreamReader)
                        objStreamReader.Close()
                        NewMDIChild.LoadCase(NewMDIChild.currcase, False)
                        If Not My.Settings.MostRecentFiles.Contains(MyFiles(i)) Then
                            My.Settings.MostRecentFiles.Add(MyFiles(i))
                            Me.UpdateMRUList()
                        End If
                End Select
            Next
        End If
    End Sub

    Private Sub FormMain_DragEnter(ByVal sender As Object, ByVal e As System.Windows.Forms.DragEventArgs) Handles Me.DragEnter
        If e.Data.GetDataPresent(DataFormats.FileDrop) Then
            e.Effect = DragDropEffects.All
        End If
    End Sub

    Private Sub FormParent_FormClosing(ByVal sender As Object, ByVal e As System.Windows.Forms.FormClosingEventArgs) Handles Me.FormClosing

        If Not Me.SairDiretoERRO Then
            If Me.MdiChildren.Length > 0 Then
                Dim ms As MsgBoxResult = MessageBox.Show(DWSIM.App.GetLocalString("Existemsimulaesabert"), DWSIM.App.GetLocalString("Ateno"), MessageBoxButtons.YesNo, MessageBoxIcon.Question)
                If ms = MsgBoxResult.No Then e.Cancel = True
            End If
        End If

        'Check if DWSIM is running in Portable/Mono mode, then save settings to file.
        If File.Exists(My.Application.Info.DirectoryPath & Path.DirectorySeparatorChar & "default.ini") Or DWSIM.App.IsRunningOnMono Then
            DWSIM.App.SaveSettings()
        End If
        My.Application.SaveMySettingsOnExit = True
        My.Settings.Save()

    End Sub

    Private Sub MyApplication_UnhandledException(ByVal sender As Object, ByVal e As System.Threading.ThreadExceptionEventArgs)
        Try
            Dim frmEx As New FormUnhandledException
            frmEx.TextBox1.Text = e.Exception.ToString
            frmEx.ex = e.Exception
            frmEx.ShowDialog()
        Finally

        End Try
    End Sub

    Private Sub MyApplication_UnhandledException2(ByVal sender As Object, ByVal e As System.UnhandledExceptionEventArgs)
        Try
            Dim frmEx As New FormUnhandledException
            frmEx.TextBox1.Text = e.ExceptionObject.ToString
            frmEx.ex = e.ExceptionObject
            frmEx.ShowDialog()
        Catch ex As Exception

        End Try
    End Sub

    Private Sub Form1_Load(ByVal sender As System.Object, ByVal e As System.EventArgs) Handles MyBase.Load

        If My.Settings.BackupFolder = "" Then My.Settings.BackupFolder = My.Computer.FileSystem.SpecialDirectories.Temp & "\DWSIM"
        If My.Settings.BackupActivated Then
            Me.TimerBackup.Interval = My.Settings.BackupInterval * 60000
            Me.TimerBackup.Enabled = True
        End If

        Me.Text = DWSIM.App.GetLocalString("FormParent_FormText")

        Global.EWSoftware.StatusBarText.StatusBarTextProvider.ApplicationStatusBar = Me.ToolStripStatusLabel1

        Me.StatusBarTextProvider1.InstanceStatusBar = Me.ToolStripStatusLabel1

        Me.dropdownlist = New ArrayList
        Me.UpdateMRUList()

        'load plugins from 'Plugins' folder

        Dim pluginlist As List(Of Interfaces.IUtilityPlugin) = GetPlugins(LoadPluginAssemblies())

        For Each ip As Interfaces.IUtilityPlugin In pluginlist
            Me.UtilityPlugins.Add(ip.UniqueID, ip)
        Next

        'load external property packages from 'propertypackages' folder, if there is any
        Dim epplist As List(Of PropertyPackage) = GetExternalPPs(LoadExternalPPs())

        For Each pp As PropertyPackage In epplist
            PropertyPackages.Add(pp.ComponentName, pp)
        Next

        'Search and populate CAPE-OPEN Flowsheet Monitoring Object collection
        'SearchCOMOs() 'doing this only when the user hovers the mouse over the plugins toolstrip menu item

        If My.Settings.ScriptPaths Is Nothing Then My.Settings.ScriptPaths = New Collections.Specialized.StringCollection()

    End Sub

    Sub SearchCOMOs()

        Dim keys As String() = My.Computer.Registry.ClassesRoot.OpenSubKey("CLSID", False).GetSubKeyNames()

        For Each k In keys
            Dim mykey As RegistryKey = My.Computer.Registry.ClassesRoot.OpenSubKey("CLSID", False).OpenSubKey(k, False)
            Dim mykeys As String() = mykey.GetSubKeyNames()
            For Each s As String In mykeys
                If s = "Implemented Categories" Then
                    Dim arr As Array = mykey.OpenSubKey("Implemented Categories").GetSubKeyNames()
                    For Each s2 As String In arr
                        If s2.ToLower = "{7ba1af89-b2e4-493d-bd80-2970bf4cbe99}" Then
                            'this is a CAPE-OPEN MO
                            Dim myuo As New DWSIM.SimulationObjects.UnitOps.Auxiliary.CapeOpen.CapeOpenUnitOpInfo
                            With myuo
                                .AboutInfo = mykey.OpenSubKey("CapeDescription").GetValue("About")
                                .CapeVersion = mykey.OpenSubKey("CapeDescription").GetValue("CapeVersion")
                                .Description = mykey.OpenSubKey("CapeDescription").GetValue("Description")
                                .HelpURL = mykey.OpenSubKey("CapeDescription").GetValue("HelpURL")
                                .Name = mykey.OpenSubKey("CapeDescription").GetValue("Name")
                                .VendorURL = mykey.OpenSubKey("CapeDescription").GetValue("VendorURL")
                                .Version = mykey.OpenSubKey("CapeDescription").GetValue("ComponentVersion")
                                Try
                                    .TypeName = mykey.OpenSubKey("ProgID").GetValue("")
                                Catch ex As Exception
                                End Try
                                Try
                                    .Location = mykey.OpenSubKey("InProcServer32").GetValue("")
                                Catch ex As Exception
                                    .Location = mykey.OpenSubKey("LocalServer32").GetValue("")
                                End Try
                            End With
                            Me.COMonitoringObjects.Add(myuo.TypeName, myuo)
                        End If
                    Next
                End If
            Next
            mykey.Close()
        Next

    End Sub

    Private Function LoadPluginAssemblies() As List(Of Assembly)

        Dim pluginassemblylist As List(Of Assembly) = New List(Of Assembly)

        If Directory.Exists(Path.Combine(Environment.CurrentDirectory, "plugins")) Then

            Dim dinfo As New DirectoryInfo(Path.Combine(Environment.CurrentDirectory, "plugins"))

            Dim files() As FileInfo = dinfo.GetFiles("*.*", SearchOption.TopDirectoryOnly)

            If Not files Is Nothing Then
                For Each fi As FileInfo In files
                    Try
                        pluginassemblylist.Add(Assembly.LoadFile(fi.FullName))
                    Catch ex As Exception

                    End Try
                Next
            End If

        End If

        Return pluginassemblylist

    End Function

    Function GetPlugins(ByVal alist As List(Of Assembly)) As List(Of Interfaces.IUtilityPlugin)

        Dim availableTypes As New List(Of Type)()

        For Each currentAssembly As Assembly In alist
            Try
                availableTypes.AddRange(currentAssembly.GetTypes())
            Catch ex As ReflectionTypeLoadException
                Dim errstr As New StringBuilder()
                For Each lex As Exception In ex.LoaderExceptions
                    errstr.AppendLine(lex.ToString)
                Next
                MessageBox.Show(errstr.ToString, "Error loading plugin", MessageBoxButtons.OK, MessageBoxIcon.Error)
            End Try
        Next

        Dim pluginlist As List(Of Type) = availableTypes.FindAll(AddressOf isPlugin)

        Return pluginlist.ConvertAll(Of Interfaces.IUtilityPlugin)(Function(t As Type) TryCast(Activator.CreateInstance(t), Interfaces.IUtilityPlugin))

    End Function

    Function isPlugin(ByVal t As Type)
        Dim interfaceTypes As New List(Of Type)(t.GetInterfaces())
        Return (interfaceTypes.Contains(GetType(Interfaces.IUtilityPlugin)))
    End Function

    Private Function LoadExternalPPs() As List(Of Assembly)

        Dim pluginassemblylist As List(Of Assembly) = New List(Of Assembly)

        If Directory.Exists(Path.Combine(Environment.CurrentDirectory, "propertypackages")) Then

            Dim dinfo As New DirectoryInfo(Path.Combine(Environment.CurrentDirectory, "propertypackages"))

            Dim files() As FileInfo = dinfo.GetFiles("*.dll")

            If Not files Is Nothing Then
                For Each fi As FileInfo In files
                    pluginassemblylist.Add(Assembly.LoadFile(fi.FullName))
                Next
            End If

        End If

        Return pluginassemblylist

    End Function

    Function GetExternalPPs(ByVal alist As List(Of Assembly)) As List(Of PropertyPackage)

        Dim availableTypes As New List(Of Type)()

        For Each currentAssembly As Assembly In alist
            Try
                availableTypes.AddRange(currentAssembly.GetTypes())
            Catch ex As Exception
                MessageBox.Show(ex.Message.ToCharArray, "Error loading plugin", MessageBoxButtons.OK, MessageBoxIcon.Error)
            End Try
        Next

        Dim ppList As List(Of Type) = availableTypes.FindAll(AddressOf isPP)

        Return ppList.ConvertAll(Of PropertyPackage)(Function(t As Type) TryCast(Activator.CreateInstance(t), PropertyPackage))

    End Function

    Function isPP(ByVal t As Type)
        Return (t Is GetType(PropertyPackage))
    End Function

    Private Sub UpdateMRUList()

        'process MRU file list

        If My.Settings.MostRecentFiles.Count > 10 Then
            My.Settings.MostRecentFiles.RemoveAt(0)
        End If

        Dim j As Integer = 0
        For Each k As String In Me.dropdownlist
            Dim tsmi As ToolStripItem = Me.FileToolStripMenuItem.DropDownItems(CInt(k - j))
            If tsmi.DisplayStyle = ToolStripItemDisplayStyle.Text Or TypeOf tsmi Is ToolStripSeparator Then
                Me.FileToolStripMenuItem.DropDownItems.Remove(tsmi)
                j = j + 1
            End If
        Next

        Me.dropdownlist.Clear()

        Dim toremove As New ArrayList

        If Not My.Settings.MostRecentFiles Is Nothing Then
            For Each str As String In My.Settings.MostRecentFiles
                If File.Exists(str) Then
                    Dim tsmi As New ToolStripMenuItem
                    With tsmi
                        .Text = str
                        .Tag = str
                        .DisplayStyle = ToolStripItemDisplayStyle.Text
                    End With
                    Me.FileToolStripMenuItem.DropDownItems.Insert(Me.FileToolStripMenuItem.DropDownItems.Count - 1, tsmi)
                    Me.dropdownlist.Add(Me.FileToolStripMenuItem.DropDownItems.Count - 2)
                    AddHandler tsmi.Click, AddressOf Me.OpenRecent_click
                Else
                    toremove.Add(str)
                End If
            Next
            For Each s As String In toremove
                My.Settings.MostRecentFiles.Remove(s)
            Next
            If My.Settings.MostRecentFiles.Count > 0 Then
                Me.FileToolStripMenuItem.DropDownItems.Insert(Me.FileToolStripMenuItem.DropDownItems.Count - 1, New ToolStripSeparator())
                Me.dropdownlist.Add(Me.FileToolStripMenuItem.DropDownItems.Count - 2)
            End If
        Else
            My.Settings.MostRecentFiles = New System.Collections.Specialized.StringCollection
        End If

    End Sub

    Sub AddPropPacks()

        Dim FPP As FPROPSPropertyPackage = New FPROPSPropertyPackage()
        FPP.ComponentName = DWSIM.App.GetLocalString("FPP")
        FPP.ComponentDescription = DWSIM.App.GetLocalString("DescFPP")
        PropertyPackages.Add(FPP.ComponentName.ToString, FPP)

        Dim STPP As SteamTablesPropertyPackage = New SteamTablesPropertyPackage()
        STPP.ComponentName = DWSIM.App.GetLocalString("TabelasdeVaporSteamT")
        STPP.ComponentDescription = DWSIM.App.GetLocalString("DescSteamTablesPP")
        PropertyPackages.Add(STPP.ComponentName.ToString, STPP)

        Dim PCSAFTPP As PCSAFTPropertyPackage = New PCSAFTPropertyPackage()
        PCSAFTPP.ComponentName = "PC-SAFT"
        PCSAFTPP.ComponentDescription = DWSIM.App.GetLocalString("DescPCSAFTPP")
        PropertyPackages.Add(PCSAFTPP.ComponentName.ToString, PCSAFTPP)

        Dim PRPP As PengRobinsonPropertyPackage = New PengRobinsonPropertyPackage()
        PRPP.ComponentName = "Peng-Robinson (PR)"
        PRPP.ComponentDescription = DWSIM.App.GetLocalString("DescPengRobinsonPP")
        PropertyPackages.Add(PRPP.ComponentName.ToString, PRPP)

        Dim PRSV2PP As PRSV2PropertyPackage = New PRSV2PropertyPackage()
        PRSV2PP.ComponentName = "Peng-Robinson-Stryjek-Vera 2 (PRSV2-M)"
        PRSV2PP.ComponentDescription = DWSIM.App.GetLocalString("DescPRSV2PP")
        PropertyPackages.Add(PRSV2PP.ComponentName.ToString, PRSV2PP)

        Dim PRSV2PPVL As PRSV2VLPropertyPackage = New PRSV2VLPropertyPackage()
        PRSV2PPVL.ComponentName = "Peng-Robinson-Stryjek-Vera 2 (PRSV2-VL)"
        PRSV2PPVL.ComponentDescription = DWSIM.App.GetLocalString("DescPRSV2VLPP")
        PropertyPackages.Add(PRSV2PPVL.ComponentName.ToString, PRSV2PPVL)

        Dim SRKPP As SRKPropertyPackage = New SRKPropertyPackage()
        SRKPP.ComponentName = "Soave-Redlich-Kwong (SRK)"
        SRKPP.ComponentDescription = DWSIM.App.GetLocalString("DescSoaveRedlichKwongSRK")
        PropertyPackages.Add(SRKPP.ComponentName.ToString, SRKPP)

        Dim PRLKPP As PengRobinsonLKPropertyPackage = New PengRobinsonLKPropertyPackage()
        PRLKPP.ComponentName = "Peng-Robinson / Lee-Kesler (PR/LK)"
        PRLKPP.ComponentDescription = DWSIM.App.GetLocalString("DescPRLK")

        PropertyPackages.Add(PRLKPP.ComponentName.ToString, PRLKPP)

        Dim UPP As UNIFACPropertyPackage = New UNIFACPropertyPackage()
        UPP.ComponentName = "UNIFAC"
        UPP.ComponentDescription = DWSIM.App.GetLocalString("DescUPP")

        PropertyPackages.Add(UPP.ComponentName.ToString, UPP)

        Dim ULLPP As UNIFACLLPropertyPackage = New UNIFACLLPropertyPackage()
        ULLPP.ComponentName = "UNIFAC-LL"
        ULLPP.ComponentDescription = DWSIM.App.GetLocalString("DescUPP")

        PropertyPackages.Add(ULLPP.ComponentName.ToString, ULLPP)

        Dim MUPP As MODFACPropertyPackage = New MODFACPropertyPackage()
        MUPP.ComponentName = "Modified UNIFAC (Dortmund)"
        MUPP.ComponentDescription = DWSIM.App.GetLocalString("DescMUPP")

        PropertyPackages.Add(MUPP.ComponentName.ToString, MUPP)

        Dim NRTLPP As NRTLPropertyPackage = New NRTLPropertyPackage()
        NRTLPP.ComponentName = "NRTL"
        NRTLPP.ComponentDescription = DWSIM.App.GetLocalString("DescNRTLPP")

        PropertyPackages.Add(NRTLPP.ComponentName.ToString, NRTLPP)

        Dim UQPP As UNIQUACPropertyPackage = New UNIQUACPropertyPackage()
        UQPP.ComponentName = "UNIQUAC"
        UQPP.ComponentDescription = DWSIM.App.GetLocalString("DescUNIQUACPP")

        PropertyPackages.Add(UQPP.ComponentName.ToString, UQPP)

        Dim CSLKPP As ChaoSeaderPropertyPackage = New ChaoSeaderPropertyPackage()
        CSLKPP.ComponentName = "Chao-Seader"
        CSLKPP.ComponentDescription = DWSIM.App.GetLocalString("DescCSLKPP")

        PropertyPackages.Add(CSLKPP.ComponentName.ToString, CSLKPP)

        Dim GSLKPP As GraysonStreedPropertyPackage = New GraysonStreedPropertyPackage()
        GSLKPP.ComponentName = "Grayson-Streed"
        GSLKPP.ComponentDescription = DWSIM.App.GetLocalString("DescGSLKPP")

        PropertyPackages.Add(GSLKPP.ComponentName.ToString, GSLKPP)

        Dim RPP As RaoultPropertyPackage = New RaoultPropertyPackage()
        RPP.ComponentName = DWSIM.App.GetLocalString("LeideRaoultGsSoluoId")
        RPP.ComponentDescription = DWSIM.App.GetLocalString("DescRPP")

        PropertyPackages.Add(RPP.ComponentName.ToString, RPP)

        Dim LKPPP As LKPPropertyPackage = New LKPPropertyPackage()
        LKPPP.ComponentName = "Lee-Kesler-Plöcker"
        LKPPP.ComponentDescription = DWSIM.App.GetLocalString("DescLKPPP")

        PropertyPackages.Add(LKPPP.ComponentName.ToString, LKPPP)

        'Check if DWSIM is running in Portable/Mono mode, if not then load the COSMO-SAC Property Package.
        If Not File.Exists(My.Application.Info.DirectoryPath & Path.DirectorySeparatorChar & "default.ini") Or Not DWSIM.App.IsRunningOnMono Then

            Dim CSPP As COSMOSACPropertyPackage = New COSMOSACPropertyPackage()
            CSPP.ComponentName = "COSMO-SAC (JCOSMO)"
            CSPP.ComponentDescription = DWSIM.App.GetLocalString("DescCSPP")

            PropertyPackages.Add(CSPP.ComponentName.ToString, CSPP)

        End If

        If Not DWSIM.App.IsRunningOnMono Then

            Dim COPP As CAPEOPENPropertyPackage = New CAPEOPENPropertyPackage()
            COPP.ComponentName = "CAPE-OPEN"
            COPP.ComponentDescription = DWSIM.App.GetLocalString("DescCOPP")

            PropertyPackages.Add(COPP.ComponentName.ToString, COPP)

        End If

        Dim LQPP As LIQUAC2PropertyPackage = New LIQUAC2PropertyPackage()
        LQPP.ComponentName = "LIQUAC2 (Aqueous Electrolytes)"
        LQPP.ComponentDescription = DWSIM.App.GetLocalString("DescLQPP")

        PropertyPackages.Add(LQPP.ComponentName.ToString, LQPP)

        Dim EUQPP As ExUNIQUACPropertyPackage = New ExUNIQUACPropertyPackage()
        EUQPP.ComponentName = "Extended UNIQUAC (Aqueous Electrolytes)"
        EUQPP.ComponentDescription = DWSIM.App.GetLocalString("DescEUPP")

        PropertyPackages.Add(EUQPP.ComponentName.ToString, EUQPP)

    End Sub

    Private Sub FormParent_MdiChildActivate(ByVal sender As Object, ByVal e As System.EventArgs) Handles Me.MdiChildActivate
        If Me.MdiChildren.Length >= 1 Then
            Me.ToolStripButton1.Enabled = True
            Me.SaveAllToolStripButton.Enabled = True
            Me.SaveToolStripButton.Enabled = True
            Me.SaveToolStripMenuItem.Enabled = True
            Me.SaveAllToolStripMenuItem.Enabled = True
            Me.SaveAsToolStripMenuItem.Enabled = True
            Me.ToolStripButton1.Enabled = True
            Me.CloseAllToolstripMenuItem.Enabled = True
            If Not Me.ActiveMdiChild Is Nothing Then
                If TypeOf Me.ActiveMdiChild Is FormFlowsheet Then
                    My.Application.ActiveSimulation = Me.ActiveMdiChild
               End If
            End If

        End If
    End Sub

    Private Sub FormParent_Shown(ByVal sender As Object, ByVal e As System.EventArgs) Handles Me.Shown

        Dim cmdLine() As String = System.Environment.GetCommandLineArgs()
        If UBound(cmdLine) = 1 And Not cmdLine(0).StartsWith("-") Then
            Try
                Me.filename = cmdLine(1)
                Try
                    Me.ToolStripStatusLabel1.Text = DWSIM.App.GetLocalString("Abrindosimulao") + " (" + Me.filename + ")"
                    Application.DoEvents()
                    Select Case Path.GetExtension(Me.filename).ToLower()
                        Case ".dwsim"
                            Me.LoadF(Me.filename)
                        Case ".dwxml"
                            Me.LoadXML(Me.filename)
                        Case ".dwxmz"
                            Me.LoadAndExtractXMLZIP(Me.filename)
                        Case ".dwcsd"
                            Dim NewMDIChild As New FormCompoundCreator()
                            NewMDIChild.MdiParent = Me
                            NewMDIChild.Show()
                            Dim objStreamReader As New FileStream(Me.filename, FileMode.Open)
                            Dim x As New BinaryFormatter()
                            NewMDIChild.mycase = x.Deserialize(objStreamReader)
                            objStreamReader.Close()
                            NewMDIChild.WriteData()
                        Case ".dwrsd"
                            Dim NewMDIChild As New FormDataRegression()
                            NewMDIChild.MdiParent = Me
                            NewMDIChild.Show()
                            Dim objStreamReader As New FileStream(Me.filename, FileMode.Open)
                            Dim x As New BinaryFormatter()
                            NewMDIChild.currcase = x.Deserialize(objStreamReader)
                            objStreamReader.Close()
                            NewMDIChild.LoadCase(NewMDIChild.currcase, False)
                    End Select
                Catch ex As Exception
                    MessageBox.Show(DWSIM.App.GetLocalString("Erroaoabrirarquivo") & " " & ex.Message, DWSIM.App.GetLocalString("Erro"), MessageBoxButtons.OK, MessageBoxIcon.Error)
                Finally
                    Me.ToolStripStatusLabel1.Text = ""
                End Try
            Catch ex As Exception
                MessageBox.Show(ex.Message, DWSIM.App.GetLocalString("Erro"), MessageBoxButtons.OK, MessageBoxIcon.Error)
            End Try
        Else
            If My.Settings.BackupFiles.Count > 0 Then
                Me.FrmRec = New FormRecoverFiles
                Me.FrmRec.ShowDialog(Me)
            Else
                If My.Settings.ShowTips Then
                    Dim frmw As New FormWelcome
                    frmw.ShowDialog(Me)
                End If
            End If

        End If

        'check for updates
        If My.Settings.CheckForUpdates Then Me.bgUpdater.RunWorkerAsync()

    End Sub

#End Region

#Region "    Load Databases / Property Packages"

    Private Function GetComponents()

        'try to find chemsep xml database
        If File.Exists(My.Application.Info.DirectoryPath & Path.DirectorySeparatorChar & "dwsim.ini") Or DWSIM.App.IsRunningOnMono Then
            'PORTABLE/MONO MODE
            My.Settings.ChemSepDatabasePath = My.Application.Info.DirectoryPath & Path.DirectorySeparatorChar & "chemsepdb" & Path.DirectorySeparatorChar & "chemsep1.xml"
        Else
            Try
                Dim cspath As String = My.Computer.Registry.LocalMachine.OpenSubKey("Software").OpenSubKey("ChemSepL6v96").GetValue("")
                cspath += Path.DirectorySeparatorChar + "pcd" + Path.DirectorySeparatorChar + "chemsep1.xml"
                If File.Exists(cspath) Then My.Settings.ChemSepDatabasePath = cspath
            Catch ex As Exception
                Console.WriteLine("Error: Unable to find ChemSep database: " & ex.ToString)
            End Try
        End If

        Try
            'load chempsep database, if existent
            If File.Exists(My.Settings.ChemSepDatabasePath) Then Me.LoadCSDB(My.Settings.ChemSepDatabasePath)
        Catch ex As Exception
            ex.Data.Add("Reason", "Error loading ChemSep database")
            Throw ex
        End Try

        'load DWSIM XML database
        Me.LoadDWSIMDB(My.Application.Info.DirectoryPath & pathsep & "data" & pathsep & "databases" & pathsep & "dwsim.xml")

        'load Biodiesel XML database
        Me.LoadBDDB(My.Application.Info.DirectoryPath & pathsep & "data" & pathsep & "databases" & pathsep & "biod_db.xml")

        'load Electrolyte XML database
        Me.LoadEDB(My.Application.Info.DirectoryPath & pathsep & "data" & pathsep & "databases" & pathsep & "electrolyte.xml")

        Dim invaliddbs As New List(Of String)

        'load user databases
        For Each fpath As String In My.Settings.UserDatabases
            Try
                Dim componentes As ConstantProperties()
                componentes = DWSIM.Databases.UserDB.Read(fpath)
                If componentes.Length > 0 Then
                    If My.Settings.ReplaceComps Then
                        For Each c As ConstantProperties In componentes
                            If Not Me.AvailableComponents.ContainsKey(c.Name) Then
                                Me.AvailableComponents.Add(c.Name, c)
                            Else
                                Me.AvailableComponents(c.Name) = c
                            End If
                        Next
                    Else
                        For Each c As ConstantProperties In componentes
                            If Not Me.AvailableComponents.ContainsKey(c.Name) Then
                                Me.AvailableComponents.Add(c.Name, c)
                            End If
                        Next
                    End If
                End If
            Catch ex As System.Runtime.Serialization.SerializationException
                invaliddbs.Add(fpath)
            Catch ex As FileNotFoundException
                invaliddbs.Add(fpath)
            End Try
        Next

        'remove non-existent or broken user databases from the list
        For Each str As String In invaliddbs
            My.Settings.UserDatabases.Remove(str)
        Next

        Return Nothing

    End Function

    Public Sub LoadCSDB(ByVal filename As String)
        If File.Exists(filename) Then
            Dim csdb As New DWSIM.Databases.ChemSep
            Dim cpa() As DWSIM.ClassesBasicasTermodinamica.ConstantProperties
            csdb.Load(filename)
            cpa = csdb.Transfer()
            For Each cp As DWSIM.ClassesBasicasTermodinamica.ConstantProperties In cpa
                If Not Me.AvailableComponents.ContainsKey(cp.Name) Then Me.AvailableComponents.Add(cp.Name, cp)
            Next
            loadedCSDB = True
        End If
    End Sub

    Public Sub LoadDWSIMDB(ByVal filename As String)
        If File.Exists(filename) Then
            Dim dwdb As New DWSIM.Databases.DWSIM
            Dim cpa() As DWSIM.ClassesBasicasTermodinamica.ConstantProperties
            dwdb.Load(filename)
            cpa = dwdb.Transfer()
            For Each cp As DWSIM.ClassesBasicasTermodinamica.ConstantProperties In cpa
                If Not Me.AvailableComponents.ContainsKey(cp.Name) Then Me.AvailableComponents.Add(cp.Name, cp)
            Next
        End If
    End Sub

    Public Sub LoadBDDB(ByVal filename As String)
        If File.Exists(filename) Then
            Dim bddb As New DWSIM.Databases.Biodiesel
            Dim cpa() As DWSIM.ClassesBasicasTermodinamica.ConstantProperties
            bddb.Load(filename)
            cpa = bddb.Transfer()
            For Each cp As DWSIM.ClassesBasicasTermodinamica.ConstantProperties In cpa
                If Not Me.AvailableComponents.ContainsKey(cp.Name) Then Me.AvailableComponents.Add(cp.Name, cp)
            Next
        End If
    End Sub

    Public Sub LoadEDB(ByVal filename As String)
        If File.Exists(filename) Then
            Dim edb As New DWSIM.Databases.Electrolyte
            Dim cpa() As DWSIM.ClassesBasicasTermodinamica.ConstantProperties
            edb.Load(filename)
            cpa = edb.Transfer()
            For Each cp As DWSIM.ClassesBasicasTermodinamica.ConstantProperties In cpa
                If Not Me.AvailableComponents.ContainsKey(cp.Name) Then Me.AvailableComponents.Add(cp.Name, cp)
            Next
        End If
    End Sub

    Public Function CopyPropertyPackages() As Object

        Dim col As New System.Collections.Generic.Dictionary(Of String, DWSIM.SimulationObjects.PropertyPackages.PropertyPackage)

        For Each pp As PropertyPackage In Me.PropertyPackages.Values
            col.Add(pp.ComponentName, CType(pp.Clone, PropertyPackage))
        Next

        Return col

    End Function

#End Region

#Region "    Open/Save Files"

    Shared Sub SaveState(ByRef flowsheet As FormFlowsheet)

        Dim st As New FlowsheetState()

        Dim rect As Rectangle = New Rectangle(0, 0, flowsheet.FormSurface.FlowsheetDesignSurface.Width - 14, flowsheet.FormSurface.FlowsheetDesignSurface.Height - 14)
        st.Snapshot = New Bitmap(rect.Width, rect.Height)
        flowsheet.FormSurface.FlowsheetDesignSurface.DrawToBitmap(st.Snapshot, flowsheet.FormSurface.FlowsheetDesignSurface.Bounds)

        Dim fs As New FormState
        fs.TextBox2.Text = Date.Now.ToString
        fs.TextBox1.Text = "state_" & Date.Now.ToString
        fs.PictureBox1.Image = st.Snapshot

        fs.btnOK.Text = DWSIM.App.GetLocalString("SaveState")

        If fs.ShowDialog(flowsheet) = Windows.Forms.DialogResult.OK Then

            Dim frmwait As New FormLS

            Try

                frmwait.Show(flowsheet)

                Application.DoEvents()

                Dim mySerializer As Binary.BinaryFormatter = New Binary.BinaryFormatter(Nothing, New System.Runtime.Serialization.StreamingContext())

                Using stream As New MemoryStream()
                    mySerializer.Serialize(stream, flowsheet.Collections)
                    st.Collections = stream.ToArray()
                End Using

                Application.DoEvents()

                Using stream As New MemoryStream()
                    mySerializer.Serialize(stream, flowsheet.FormSurface.FlowsheetDesignSurface.drawingObjects)
                    st.GraphicObjects = stream.ToArray()
                End Using

                Application.DoEvents()

                Using stream As New MemoryStream()
                    mySerializer.Serialize(stream, flowsheet.Options)
                    st.Options = stream.ToArray()
                End Using

                Using stream As New MemoryStream()
                    TreeViewDataAccess.SaveTreeViewData(flowsheet.FormObjList.TreeViewObj, stream)
                    st.TreeViewObjects = stream.ToArray()
                End Using

                Application.DoEvents()

                flowsheet.FormSpreadsheet.CopyToDT()

                Using stream As New MemoryStream()
                    mySerializer.Serialize(stream, flowsheet.FormSpreadsheet.dt1)
                    st.SpreadsheetDT1 = stream.ToArray()
                    mySerializer.Serialize(stream, flowsheet.FormSpreadsheet.dt2)
                    st.SpreadsheetDT1 = stream.ToArray()
                End Using

                Application.DoEvents()

                Using stream As New MemoryStream()
                    mySerializer.Serialize(stream, flowsheet.FormSpreadsheet.dt2)
                    st.SpreadsheetDT1 = stream.ToArray()
                End Using

                Application.DoEvents()

                Using stream As New MemoryStream()
                    mySerializer.Serialize(stream, flowsheet.FormWatch.items)
                    st.WatchItems = stream.ToArray()
                End Using

                Application.DoEvents()

                If flowsheet.FlowsheetStates Is Nothing Then
                    flowsheet.FlowsheetStates = New Dictionary(Of Date, FlowsheetState)
                End If

                st.Description = fs.TextBox1.Text

                st.SaveDate = Date.Now

                flowsheet.FlowsheetStates.Add(st.SaveDate, st)

                flowsheet.UpdateStateList()

                flowsheet.WriteToLog(DWSIM.App.GetLocalString("SaveStateOK"), Color.Blue, TipoAviso.Informacao)

            Catch ex As Exception

                flowsheet.WriteToLog(DWSIM.App.GetLocalString("SaveStateError") & " " & ex.Message.ToString, Color.Red, TipoAviso.Erro)

                st = Nothing

            Finally

                frmwait.Close()

            End Try

        End If

    End Sub

    Shared Sub RestoreState(ByRef flowsheet As FormFlowsheet, ByVal st As FlowsheetState)

        Dim fs As New FormState
        fs.TextBox1.Enabled = False
        fs.TextBox2.Enabled = False
        fs.TextBox2.Text = st.SaveDate
        fs.TextBox1.Text = st.Description
        fs.PictureBox1.Image = st.Snapshot

        fs.btnOK.Text = DWSIM.App.GetLocalString("RestoreState")

        If fs.ShowDialog(flowsheet) = Windows.Forms.DialogResult.OK Then

            Dim frmwait As New FormLS

            Try

                frmwait.Show(flowsheet)

                Application.DoEvents()

                flowsheet.SuspendLayout()

                Dim mySerializer As Binary.BinaryFormatter = New Binary.BinaryFormatter(Nothing, New System.Runtime.Serialization.StreamingContext())

                Using stream As New MemoryStream(st.GraphicObjects)
                    flowsheet.FormSurface.FlowsheetDesignSurface.m_drawingObjects = Nothing
                    flowsheet.FormSurface.FlowsheetDesignSurface.m_drawingObjects = DirectCast(mySerializer.Deserialize(stream), Microsoft.Msdn.Samples.GraphicObjects.GraphicObjectCollection)
                End Using

                Application.DoEvents()

                Using stream As New MemoryStream(st.Collections)
                    flowsheet.Collections = Nothing
                    flowsheet.Collections = DirectCast(mySerializer.Deserialize(stream), DWSIM.FormClasses.ClsObjectCollections)
                End Using

                Application.DoEvents()

                Using stream As New MemoryStream(st.Options)
                    flowsheet.Options = Nothing
                    flowsheet.Options = DirectCast(mySerializer.Deserialize(stream), DWSIM.FormClasses.ClsFormOptions)
                End Using

                Application.DoEvents()

                Using stream As New MemoryStream(st.TreeViewObjects)
                    flowsheet.FormObjList.TreeViewObj.Nodes.Clear()
                    TreeViewDataAccess.LoadTreeViewData(flowsheet.FormObjList.TreeViewObj, stream)
                End Using

                Application.DoEvents()

                If Not st.SpreadsheetDT1 Is Nothing Then
                    Using stream As New MemoryStream(st.SpreadsheetDT1)
                        flowsheet.FormSpreadsheet.dt1 = DirectCast(mySerializer.Deserialize(stream), Object(,))
                    End Using
                End If

                Application.DoEvents()

                If Not st.SpreadsheetDT2 Is Nothing Then
                    Using stream As New MemoryStream(st.SpreadsheetDT2)
                        flowsheet.FormSpreadsheet.dt2 = DirectCast(mySerializer.Deserialize(stream), Object(,))
                    End Using
                End If

                Application.DoEvents()

                Using stream As New MemoryStream(st.WatchItems)
                    flowsheet.FormWatch.items = DirectCast(mySerializer.Deserialize(stream), Dictionary(Of Integer, DWSIM.Outros.WatchItem))
                    flowsheet.FormWatch.PopulateList()
                End Using

                Application.DoEvents()

                flowsheet.CheckCollections()

                Application.DoEvents()

                With flowsheet.Collections
                    Dim gObj As Microsoft.Msdn.Samples.GraphicObjects.GraphicObject
                    For Each gObj In flowsheet.FormSurface.FlowsheetDesignSurface.drawingObjects
                        Select Case gObj.TipoObjeto
                            Case Microsoft.Msdn.Samples.GraphicObjects.TipoObjeto.Compressor
                                .CLCS_CompressorCollection(gObj.Name).GraphicObject = gObj
                                .CompressorCollection(gObj.Name) = gObj
                            Case Microsoft.Msdn.Samples.GraphicObjects.TipoObjeto.Cooler
                                .CLCS_CoolerCollection(gObj.Name).GraphicObject = gObj
                                .CoolerCollection(gObj.Name) = gObj
                            Case Microsoft.Msdn.Samples.GraphicObjects.TipoObjeto.EnergyStream
                                .CLCS_EnergyStreamCollection(gObj.Name).GraphicObject = gObj
                                .EnergyStreamCollection(gObj.Name) = gObj
                            Case Microsoft.Msdn.Samples.GraphicObjects.TipoObjeto.Heater
                                .CLCS_HeaterCollection(gObj.Name).GraphicObject = gObj
                                .HeaterCollection(gObj.Name) = gObj
                            Case Microsoft.Msdn.Samples.GraphicObjects.TipoObjeto.MaterialStream
                                .CLCS_MaterialStreamCollection(gObj.Name).GraphicObject = gObj
                                .MaterialStreamCollection(gObj.Name) = gObj
                            Case Microsoft.Msdn.Samples.GraphicObjects.TipoObjeto.NodeEn
                                .CLCS_EnergyMixerCollection(gObj.Name).GraphicObject = gObj
                                .MixerENCollection(gObj.Name) = gObj
                            Case Microsoft.Msdn.Samples.GraphicObjects.TipoObjeto.NodeIn
                                .CLCS_MixerCollection(gObj.Name).GraphicObject = gObj
                                .MixerCollection(gObj.Name) = gObj
                            Case Microsoft.Msdn.Samples.GraphicObjects.TipoObjeto.NodeOut
                                .CLCS_SplitterCollection(gObj.Name).GraphicObject = gObj
                                .SplitterCollection(gObj.Name) = gObj
                            Case Microsoft.Msdn.Samples.GraphicObjects.TipoObjeto.Pipe
                                .CLCS_PipeCollection(gObj.Name).GraphicObject = gObj
                                .PipeCollection(gObj.Name) = gObj
                            Case Microsoft.Msdn.Samples.GraphicObjects.TipoObjeto.Pump
                                .CLCS_PumpCollection(gObj.Name).GraphicObject = gObj
                                .PumpCollection(gObj.Name) = gObj
                            Case Microsoft.Msdn.Samples.GraphicObjects.TipoObjeto.Tank
                                .CLCS_TankCollection(gObj.Name).GraphicObject = gObj
                                .TankCollection(gObj.Name) = gObj
                            Case Microsoft.Msdn.Samples.GraphicObjects.TipoObjeto.Expander
                                .CLCS_TurbineCollection(gObj.Name).GraphicObject = gObj
                                .TurbineCollection(gObj.Name) = gObj
                            Case Microsoft.Msdn.Samples.GraphicObjects.TipoObjeto.Valve
                                .CLCS_ValveCollection(gObj.Name).GraphicObject = gObj
                                .ValveCollection(gObj.Name) = gObj
                            Case Microsoft.Msdn.Samples.GraphicObjects.TipoObjeto.Vessel
                                .CLCS_VesselCollection(gObj.Name).GraphicObject = gObj
                                .SeparatorCollection(gObj.Name) = gObj
                            Case Microsoft.Msdn.Samples.GraphicObjects.TipoObjeto.GO_Tabela
                                .ObjectCollection(gObj.Tag).Tabela = gObj
                                CType(gObj, DWSIM.GraphicObjects.TableGraphic).BaseOwner = .ObjectCollection(gObj.Tag)
                            Case Microsoft.Msdn.Samples.GraphicObjects.TipoObjeto.Expander
                                .CLCS_TurbineCollection(gObj.Name).GraphicObject = gObj
                                .TurbineCollection(gObj.Name) = gObj
                            Case Microsoft.Msdn.Samples.GraphicObjects.TipoObjeto.OT_Ajuste
                                .CLCS_AdjustCollection(gObj.Name).GraphicObject = gObj
                                .AdjustCollection(gObj.Name) = gObj
                            Case Microsoft.Msdn.Samples.GraphicObjects.TipoObjeto.OT_Reciclo
                                .CLCS_RecycleCollection(gObj.Name).GraphicObject = gObj
                                .RecycleCollection(gObj.Name) = gObj
                            Case Microsoft.Msdn.Samples.GraphicObjects.TipoObjeto.OT_Especificacao
                                .CLCS_SpecCollection(gObj.Name).GraphicObject = gObj
                                .SpecCollection(gObj.Name) = gObj
                            Case Microsoft.Msdn.Samples.GraphicObjects.TipoObjeto.RCT_Conversion
                                .CLCS_ReactorConversionCollection(gObj.Name).GraphicObject = gObj
                                .ReactorConversionCollection(gObj.Name) = gObj
                            Case Microsoft.Msdn.Samples.GraphicObjects.TipoObjeto.RCT_Equilibrium
                                .CLCS_ReactorEquilibriumCollection(gObj.Name).GraphicObject = gObj
                                .ReactorEquilibriumCollection(gObj.Name) = gObj
                            Case Microsoft.Msdn.Samples.GraphicObjects.TipoObjeto.RCT_Gibbs
                                .CLCS_ReactorGibbsCollection(gObj.Name).GraphicObject = gObj
                                .ReactorGibbsCollection(gObj.Name) = gObj
                            Case Microsoft.Msdn.Samples.GraphicObjects.TipoObjeto.RCT_CSTR
                                .CLCS_ReactorCSTRCollection(gObj.Name).GraphicObject = gObj
                                .ReactorCSTRCollection(gObj.Name) = gObj
                            Case Microsoft.Msdn.Samples.GraphicObjects.TipoObjeto.RCT_PFR
                                .CLCS_ReactorPFRCollection(gObj.Name).GraphicObject = gObj
                                .ReactorPFRCollection(gObj.Name) = gObj
                            Case Microsoft.Msdn.Samples.GraphicObjects.TipoObjeto.HeatExchanger
                                .CLCS_HeatExchangerCollection(gObj.Name).GraphicObject = gObj
                                .HeatExchangerCollection(gObj.Name) = gObj
                            Case Microsoft.Msdn.Samples.GraphicObjects.TipoObjeto.ShortcutColumn
                                .CLCS_ShortcutColumnCollection(gObj.Name).GraphicObject = gObj
                                .ShortcutColumnCollection(gObj.Name) = gObj
                            Case Microsoft.Msdn.Samples.GraphicObjects.TipoObjeto.DistillationColumn
                                .CLCS_DistillationColumnCollection(gObj.Name).GraphicObject = gObj
                                .DistillationColumnCollection(gObj.Name) = gObj
                            Case Microsoft.Msdn.Samples.GraphicObjects.TipoObjeto.AbsorptionColumn
                                .CLCS_AbsorptionColumnCollection(gObj.Name).GraphicObject = gObj
                                .AbsorptionColumnCollection(gObj.Name) = gObj
                            Case Microsoft.Msdn.Samples.GraphicObjects.TipoObjeto.RefluxedAbsorber
                                .CLCS_RefluxedAbsorberCollection(gObj.Name).GraphicObject = gObj
                                .RefluxedAbsorberCollection(gObj.Name) = gObj
                            Case Microsoft.Msdn.Samples.GraphicObjects.TipoObjeto.ReboiledAbsorber
                                .CLCS_ReboiledAbsorberCollection(gObj.Name).GraphicObject = gObj
                                .ReboiledAbsorberCollection(gObj.Name) = gObj
                            Case Microsoft.Msdn.Samples.GraphicObjects.TipoObjeto.OT_EnergyRecycle
                                .CLCS_EnergyRecycleCollection(gObj.Name).GraphicObject = gObj
                                .EnergyRecycleCollection(gObj.Name) = gObj
                            Case Microsoft.Msdn.Samples.GraphicObjects.TipoObjeto.GO_TabelaRapida
                                .ObjectCollection(CType(gObj, DWSIM.GraphicObjects.QuickTableGraphic).BaseOwner.Nome).TabelaRapida = gObj
                            Case Microsoft.Msdn.Samples.GraphicObjects.TipoObjeto.ComponentSeparator
                                .CLCS_ComponentSeparatorCollection(gObj.Name).GraphicObject = gObj
                                .ComponentSeparatorCollection(gObj.Name) = gObj
                            Case Microsoft.Msdn.Samples.GraphicObjects.TipoObjeto.OrificePlate
                                .CLCS_OrificePlateCollection(gObj.Name).GraphicObject = gObj
                                .OrificePlateCollection(gObj.Name) = gObj
                            Case Microsoft.Msdn.Samples.GraphicObjects.TipoObjeto.CustomUO
                                .CLCS_CustomUOCollection(gObj.Name).GraphicObject = gObj
                                .CustomUOCollection(gObj.Name) = gObj
                            Case Microsoft.Msdn.Samples.GraphicObjects.TipoObjeto.CapeOpenUO
                                .CLCS_CapeOpenUOCollection(gObj.Name).GraphicObject = gObj
                                .CapeOpenUOCollection(gObj.Name) = gObj
                            Case Microsoft.MSDN.Samples.GraphicObjects.TipoObjeto.SolidSeparator
                                .CLCS_SolidsSeparatorCollection(gObj.Name).GraphicObject = gObj
                                .SolidsSeparatorCollection(gObj.Name) = gObj
                            Case Microsoft.MSDN.Samples.GraphicObjects.TipoObjeto.Filter
                                .CLCS_FilterCollection(gObj.Name).GraphicObject = gObj
                                .FilterCollection(gObj.Name) = gObj
                        End Select
                    Next
                End With

                Application.DoEvents()

                Dim refill As Boolean = False

                'refill (quick)table items for backwards compatibility
                For Each obj As SimulationObjects_BaseClass In flowsheet.Collections.ObjectCollection.Values
                    With obj
                        If .NodeTableItems.Count > 0 Then
                            For Each nvi As DWSIM.Outros.NodeItem In .NodeTableItems.Values
                                Try
                                    If Not nvi.Text.Contains("PROP_") Then
                                        refill = True
                                        Exit For
                                    End If
                                Catch ex As Exception

                                End Try
                            Next
                        End If
                        If refill Then
                            .NodeTableItems.Clear()
                            .QTNodeTableItems.Clear()
                            .FillNodeItems()
                            .QTFillNodeItems()
                        End If
                    End With
                Next

                Application.DoEvents()

                flowsheet.FrmStSim1.Init(True)

                Application.DoEvents()

                flowsheet.ResumeLayout()

                flowsheet.FormSurface.FlowsheetDesignSurface.Invalidate()

                flowsheet.WriteToLog(DWSIM.App.GetLocalString("RestoreStateOK"), Color.Blue, TipoAviso.Informacao)

            Catch ex As Exception

                flowsheet.WriteToLog(DWSIM.App.GetLocalString("RestoreStateError") & " " & ex.Message.ToString, Color.Red, TipoAviso.Erro)

                st = Nothing

            Finally

                frmwait.Close()

            End Try

        End If

    End Sub

    Function ReturnForm(ByVal str As String) As IDockContent
        Select Case str
            Case "DWSIM.frmProps"
                Return Me.tmpform2.FormProps
            Case "DWSIM.frmObjList"
                Return Me.tmpform2.FormObjList
            Case "DWSIM.frmLog"
                Return Me.tmpform2.FormLog
            Case "DWSIM.frmMatList"
                Return Me.tmpform2.FormMatList
            Case "DWSIM.frmSurface"
                Return Me.tmpform2.FormSurface
            Case "DWSIM.SpreadsheetForm"
                Return Me.tmpform2.FormSpreadsheet
            Case "DWSIM.frmObjListView"
                Return Me.tmpform2.FormObjListView
            Case "DWSIM.frmWatch"
                Return Me.tmpform2.FormWatch
        End Select
        Return Nothing
    End Function

    Sub LoadF(ByVal caminho As String)

        If System.IO.File.Exists(caminho) Then

            Dim rnd As New Random()
            Dim fn As String = rnd.Next(10000, 99999)

            Dim diretorio As String = Path.GetDirectoryName(caminho)
            Dim arquivo As String = Path.GetFileName(caminho)
            Dim arquivoCAB As String = "dwsim" + fn

            Dim ziperror As Boolean = False
            Dim loadedok As Boolean = False
            Try
                Dim zp As New ZipFile(caminho)
                zp = Nothing
                'is a zip file
            Catch ex As Exception
                ziperror = True
            End Try

            loadedok = Me.LoadAndExtractZIP(caminho)

            If Not loadedok Then Exit Sub

            Me.SuspendLayout()
            m_childcount += 1

            Dim form As FormFlowsheet = New FormFlowsheet()
            My.Application.CAPEOPENMode = False
            My.Application.ActiveSimulation = form

            'is not a zip file
            If ziperror Then
                Try
                    If Not DWSIM.App.IsRunningOnMono() Then
                        'Call Me.LoadAndExtractCAB(caminho)
                    Else
                        MsgBox("This file is not loadable when running DWSIM on Mono.", MsgBoxStyle.Critical, "Error!")
                        Exit Sub
                    End If
                Catch ex As Exception
                    MessageBox.Show(ex.Message, DWSIM.App.GetLocalString("Erroaoabrirarquivo"), MessageBoxButtons.OK, MessageBoxIcon.Error)
                    form = Nothing
                End Try
            End If

            Me.filename = caminho

            Dim mySerializer As Binary.BinaryFormatter = New Binary.BinaryFormatter(Nothing, New System.Runtime.Serialization.StreamingContext())

            Dim fs3 As New FileStream(My.Computer.FileSystem.SpecialDirectories.Temp & "\3.bin", FileMode.Open)

            Using fs3
                form.FormSurface.FlowsheetDesignSurface.m_drawingObjects = Nothing
                form.FormSurface.FlowsheetDesignSurface.m_drawingObjects = DirectCast(mySerializer.Deserialize(fs3), Microsoft.Msdn.Samples.GraphicObjects.GraphicObjectCollection)
            End Using

            Dim fs As New FileStream(My.Computer.FileSystem.SpecialDirectories.Temp & "\1.bin", FileMode.Open)

            Using fs
                form.Collections = Nothing
                form.Collections = DirectCast(mySerializer.Deserialize(fs), DWSIM.FormClasses.ClsObjectCollections)
            End Using

            Dim fs2 As New FileStream(My.Computer.FileSystem.SpecialDirectories.Temp & "\2.bin", FileMode.Open)

            Using fs2
                form.Options = Nothing
                form.Options = DirectCast(mySerializer.Deserialize(fs2), DWSIM.FormClasses.ClsFormOptions)
                If form.Options.PropertyPackages.Count = 0 Then form.Options.PropertyPackages = Me.PropertyPackages
            End Using

            If Not My.Settings.ReplaceCompoundConstantProperties Then
                For Each c As ConstantProperties In form.Options.SelectedComponents.Values
                    If Me.AvailableComponents.ContainsKey(c.Name) Then
                        c = Me.AvailableComponents(c.Name)
                    End If
                Next
            End If

            If Not My.Settings.ReplaceCompoundConstantProperties Then
                For Each ms As Streams.MaterialStream In form.Collections.CLCS_MaterialStreamCollection.Values
                    For Each phase As DWSIM.ClassesBasicasTermodinamica.Fase In ms.Fases.Values
                        For Each c As ConstantProperties In form.Options.SelectedComponents.Values
                            If Me.AvailableComponents.ContainsKey(c.Name) Then
                                phase.Componentes(c.Name).ConstantProperties = Me.AvailableComponents(c.Name)
                            End If
                        Next
                    Next
                Next
            End If

            form.FormObjList.TreeViewObj.Nodes.Clear()
            TreeViewDataAccess.LoadTreeViewData(form.FormObjList.TreeViewObj, My.Computer.FileSystem.SpecialDirectories.Temp & "\5.bin")

            Dim fs7 As New FileStream(My.Computer.FileSystem.SpecialDirectories.Temp & "\7.bin", FileMode.Open)

            Using fs7
                form.Text = DirectCast(mySerializer.Deserialize(fs7), String)
            End Using

            If File.Exists(My.Computer.FileSystem.SpecialDirectories.Temp & "\8.bin") Then
                Dim fs8 As New FileStream(My.Computer.FileSystem.SpecialDirectories.Temp & "\8.bin", FileMode.Open)
                Using fs8
                    form.FormLog.GridDT.Rows.Clear()
                    form.FormLog.GridDT = DirectCast(mySerializer.Deserialize(fs8), DataTable)
                End Using
            End If

            If File.Exists(My.Computer.FileSystem.SpecialDirectories.Temp & "\9.bin") Then
                Dim fs9 As New FileStream(My.Computer.FileSystem.SpecialDirectories.Temp & "\9.bin", FileMode.Open)
                Using fs9
                    form.FormSpreadsheet.dt1 = DirectCast(mySerializer.Deserialize(fs9), Object(,))
                End Using
            End If

            If File.Exists(My.Computer.FileSystem.SpecialDirectories.Temp & "\10.bin") Then
                Dim fs10 As New FileStream(My.Computer.FileSystem.SpecialDirectories.Temp & "\10.bin", FileMode.Open)
                Using fs10
                    form.FormSpreadsheet.dt2 = DirectCast(mySerializer.Deserialize(fs10), Object(,))
                End Using
            End If

            If File.Exists(My.Computer.FileSystem.SpecialDirectories.Temp & "\11.bin") Then
                Dim fs11 As New FileStream(My.Computer.FileSystem.SpecialDirectories.Temp & "\11.bin", FileMode.Open)
                Using fs11
                    form.FormWatch.items = DirectCast(mySerializer.Deserialize(fs11), Dictionary(Of Integer, DWSIM.Outros.WatchItem))
                    form.FormWatch.PopulateList()
                End Using
            End If

            If File.Exists(My.Computer.FileSystem.SpecialDirectories.Temp & "\13.bin") Then
                Dim fs13 As New FileStream(My.Computer.FileSystem.SpecialDirectories.Temp & "\13.bin", FileMode.Open)
                Using fs13
                    form.FlowsheetStates = DirectCast(mySerializer.Deserialize(fs13), Dictionary(Of Date, FlowsheetState))
                    form.UpdateStateList()
                End Using
            End If

            form.CheckCollections()

            With form.Collections
                Dim gObj As Microsoft.Msdn.Samples.GraphicObjects.GraphicObject
                For Each gObj In form.FormSurface.FlowsheetDesignSurface.drawingObjects
                    Select Case gObj.TipoObjeto
                        Case Microsoft.Msdn.Samples.GraphicObjects.TipoObjeto.Compressor
                            .CLCS_CompressorCollection(gObj.Name).GraphicObject = gObj
                            .CompressorCollection(gObj.Name) = gObj
                        Case Microsoft.Msdn.Samples.GraphicObjects.TipoObjeto.Cooler
                            .CLCS_CoolerCollection(gObj.Name).GraphicObject = gObj
                            .CoolerCollection(gObj.Name) = gObj
                        Case Microsoft.Msdn.Samples.GraphicObjects.TipoObjeto.EnergyStream
                            .CLCS_EnergyStreamCollection(gObj.Name).GraphicObject = gObj
                            .EnergyStreamCollection(gObj.Name) = gObj
                        Case Microsoft.Msdn.Samples.GraphicObjects.TipoObjeto.Heater
                            .CLCS_HeaterCollection(gObj.Name).GraphicObject = gObj
                            .HeaterCollection(gObj.Name) = gObj
                        Case Microsoft.Msdn.Samples.GraphicObjects.TipoObjeto.MaterialStream
                            .CLCS_MaterialStreamCollection(gObj.Name).GraphicObject = gObj
                            .MaterialStreamCollection(gObj.Name) = gObj
                        Case Microsoft.Msdn.Samples.GraphicObjects.TipoObjeto.NodeEn
                            .CLCS_EnergyMixerCollection(gObj.Name).GraphicObject = gObj
                            .MixerENCollection(gObj.Name) = gObj
                        Case Microsoft.Msdn.Samples.GraphicObjects.TipoObjeto.NodeIn
                            .CLCS_MixerCollection(gObj.Name).GraphicObject = gObj
                            .MixerCollection(gObj.Name) = gObj
                        Case Microsoft.Msdn.Samples.GraphicObjects.TipoObjeto.NodeOut
                            .CLCS_SplitterCollection(gObj.Name).GraphicObject = gObj
                            .SplitterCollection(gObj.Name) = gObj
                        Case Microsoft.Msdn.Samples.GraphicObjects.TipoObjeto.Pipe
                            .CLCS_PipeCollection(gObj.Name).GraphicObject = gObj
                            .PipeCollection(gObj.Name) = gObj
                        Case Microsoft.Msdn.Samples.GraphicObjects.TipoObjeto.Pump
                            .CLCS_PumpCollection(gObj.Name).GraphicObject = gObj
                            .PumpCollection(gObj.Name) = gObj
                        Case Microsoft.Msdn.Samples.GraphicObjects.TipoObjeto.Tank
                            .CLCS_TankCollection(gObj.Name).GraphicObject = gObj
                            .TankCollection(gObj.Name) = gObj
                        Case Microsoft.Msdn.Samples.GraphicObjects.TipoObjeto.Expander
                            .CLCS_TurbineCollection(gObj.Name).GraphicObject = gObj
                            .TurbineCollection(gObj.Name) = gObj
                        Case Microsoft.Msdn.Samples.GraphicObjects.TipoObjeto.Valve
                            .CLCS_ValveCollection(gObj.Name).GraphicObject = gObj
                            .ValveCollection(gObj.Name) = gObj
                        Case Microsoft.Msdn.Samples.GraphicObjects.TipoObjeto.Vessel
                            .CLCS_VesselCollection(gObj.Name).GraphicObject = gObj
                            .SeparatorCollection(gObj.Name) = gObj
                        Case Microsoft.Msdn.Samples.GraphicObjects.TipoObjeto.GO_Tabela
                            .ObjectCollection(gObj.Tag).Tabela = gObj
                            CType(gObj, DWSIM.GraphicObjects.TableGraphic).BaseOwner = .ObjectCollection(gObj.Tag)
                        Case Microsoft.Msdn.Samples.GraphicObjects.TipoObjeto.Expander
                            .CLCS_TurbineCollection(gObj.Name).GraphicObject = gObj
                            .TurbineCollection(gObj.Name) = gObj
                        Case Microsoft.Msdn.Samples.GraphicObjects.TipoObjeto.OT_Ajuste
                            .CLCS_AdjustCollection(gObj.Name).GraphicObject = gObj
                            .AdjustCollection(gObj.Name) = gObj
                        Case Microsoft.Msdn.Samples.GraphicObjects.TipoObjeto.OT_Reciclo
                            .CLCS_RecycleCollection(gObj.Name).GraphicObject = gObj
                            .RecycleCollection(gObj.Name) = gObj
                        Case Microsoft.Msdn.Samples.GraphicObjects.TipoObjeto.OT_Especificacao
                            .CLCS_SpecCollection(gObj.Name).GraphicObject = gObj
                            .SpecCollection(gObj.Name) = gObj
                        Case Microsoft.Msdn.Samples.GraphicObjects.TipoObjeto.RCT_Conversion
                            .CLCS_ReactorConversionCollection(gObj.Name).GraphicObject = gObj
                            .ReactorConversionCollection(gObj.Name) = gObj
                        Case Microsoft.Msdn.Samples.GraphicObjects.TipoObjeto.RCT_Equilibrium
                            .CLCS_ReactorEquilibriumCollection(gObj.Name).GraphicObject = gObj
                            .ReactorEquilibriumCollection(gObj.Name) = gObj
                        Case Microsoft.Msdn.Samples.GraphicObjects.TipoObjeto.RCT_Gibbs
                            .CLCS_ReactorGibbsCollection(gObj.Name).GraphicObject = gObj
                            .ReactorGibbsCollection(gObj.Name) = gObj
                        Case Microsoft.Msdn.Samples.GraphicObjects.TipoObjeto.RCT_CSTR
                            .CLCS_ReactorCSTRCollection(gObj.Name).GraphicObject = gObj
                            .ReactorCSTRCollection(gObj.Name) = gObj
                        Case Microsoft.Msdn.Samples.GraphicObjects.TipoObjeto.RCT_PFR
                            .CLCS_ReactorPFRCollection(gObj.Name).GraphicObject = gObj
                            .ReactorPFRCollection(gObj.Name) = gObj
                        Case Microsoft.Msdn.Samples.GraphicObjects.TipoObjeto.HeatExchanger
                            .CLCS_HeatExchangerCollection(gObj.Name).GraphicObject = gObj
                            .HeatExchangerCollection(gObj.Name) = gObj
                        Case Microsoft.Msdn.Samples.GraphicObjects.TipoObjeto.ShortcutColumn
                            .CLCS_ShortcutColumnCollection(gObj.Name).GraphicObject = gObj
                            .ShortcutColumnCollection(gObj.Name) = gObj
                        Case Microsoft.Msdn.Samples.GraphicObjects.TipoObjeto.DistillationColumn
                            .CLCS_DistillationColumnCollection(gObj.Name).GraphicObject = gObj
                            .DistillationColumnCollection(gObj.Name) = gObj
                        Case Microsoft.Msdn.Samples.GraphicObjects.TipoObjeto.AbsorptionColumn
                            .CLCS_AbsorptionColumnCollection(gObj.Name).GraphicObject = gObj
                            .AbsorptionColumnCollection(gObj.Name) = gObj
                        Case Microsoft.Msdn.Samples.GraphicObjects.TipoObjeto.RefluxedAbsorber
                            .CLCS_RefluxedAbsorberCollection(gObj.Name).GraphicObject = gObj
                            .RefluxedAbsorberCollection(gObj.Name) = gObj
                        Case Microsoft.Msdn.Samples.GraphicObjects.TipoObjeto.ReboiledAbsorber
                            .CLCS_ReboiledAbsorberCollection(gObj.Name).GraphicObject = gObj
                            .ReboiledAbsorberCollection(gObj.Name) = gObj
                        Case Microsoft.Msdn.Samples.GraphicObjects.TipoObjeto.OT_EnergyRecycle
                            .CLCS_EnergyRecycleCollection(gObj.Name).GraphicObject = gObj
                            .EnergyRecycleCollection(gObj.Name) = gObj
                        Case Microsoft.Msdn.Samples.GraphicObjects.TipoObjeto.GO_TabelaRapida
                            .ObjectCollection(CType(gObj, DWSIM.GraphicObjects.QuickTableGraphic).BaseOwner.Nome).TabelaRapida = gObj
                        Case Microsoft.Msdn.Samples.GraphicObjects.TipoObjeto.ComponentSeparator
                            .CLCS_ComponentSeparatorCollection(gObj.Name).GraphicObject = gObj
                            .ComponentSeparatorCollection(gObj.Name) = gObj
                        Case Microsoft.Msdn.Samples.GraphicObjects.TipoObjeto.OrificePlate
                            .CLCS_OrificePlateCollection(gObj.Name).GraphicObject = gObj
                            .OrificePlateCollection(gObj.Name) = gObj
                        Case Microsoft.Msdn.Samples.GraphicObjects.TipoObjeto.CustomUO
                            .CLCS_CustomUOCollection(gObj.Name).GraphicObject = gObj
                            .CustomUOCollection(gObj.Name) = gObj
                        Case Microsoft.Msdn.Samples.GraphicObjects.TipoObjeto.CapeOpenUO
                            .CLCS_CapeOpenUOCollection(gObj.Name).GraphicObject = gObj
                            .CapeOpenUOCollection(gObj.Name) = gObj
                        Case Microsoft.Msdn.Samples.GraphicObjects.TipoObjeto.SolidSeparator
                            .CLCS_SolidsSeparatorCollection(gObj.Name).GraphicObject = gObj
                            .SolidsSeparatorCollection(gObj.Name) = gObj
                        Case Microsoft.Msdn.Samples.GraphicObjects.TipoObjeto.Filter
                            .CLCS_FilterCollection(gObj.Name).GraphicObject = gObj
                            .FilterCollection(gObj.Name) = gObj
                    End Select
                Next
            End With

            My.Application.ActiveSimulation = form

            Dim refill As Boolean = False

            'refill (quick)table items for backwards compatibility
            For Each obj As SimulationObjects_BaseClass In form.Collections.ObjectCollection.Values
                With obj
                    If .NodeTableItems.Count > 0 Then
                        For Each nvi As DWSIM.Outros.NodeItem In .NodeTableItems.Values
                            Try
                                If Not nvi.Text.Contains("PROP_") Then
                                    refill = True
                                    Exit For
                                End If
                            Catch ex As Exception

                            End Try
                        Next
                    End If
                    If refill Then
                        .NodeTableItems.Clear()
                        .QTNodeTableItems.Clear()
                        .FillNodeItems()
                        .QTFillNodeItems()
                    End If
                End With
            Next

            form.MdiParent = Me
            form.m_IsLoadedFromFile = True

            Me.tmpform2 = form
            form.dckPanel.SuspendLayout(True)
            form.FormLog.DockPanel = Nothing
            form.FormObjList.DockPanel = Nothing
            form.FormProps.DockPanel = Nothing
            form.FormMatList.DockPanel = Nothing
            form.FormSpreadsheet.DockPanel = Nothing
            form.FormWatch.DockPanel = Nothing
            form.FormSurface.DockPanel = Nothing

            If File.Exists(My.Computer.FileSystem.SpecialDirectories.Temp & "\4.xml") Then form.dckPanel.LoadFromXml(My.Computer.FileSystem.SpecialDirectories.Temp & "\4.xml", New DeserializeDockContent(AddressOf Me.ReturnForm))

            ''Set DockPanel and Form  properties

            form.dckPanel.ActiveAutoHideContent = Nothing
            form.dckPanel.Parent = form

            form.Options.FilePath = Me.filename
            form.WriteToLog(DWSIM.App.GetLocalString("Arquivo") & Me.filename & DWSIM.App.GetLocalString("carregadocomsucesso"), Color.Blue, DWSIM.FormClasses.TipoAviso.Informacao)
            form.Text += " (" + Me.filename + ")"

            form.FormObjListView.Show(form.dckPanel)
            form.FormLog.Show(form.dckPanel)
            form.FormMatList.Show(form.dckPanel)
            form.FormSpreadsheet.Show(form.dckPanel)
            form.FormSurface.Show(form.dckPanel)
            form.FormObjList.Show(form.dckPanel)
            form.FormProps.Show(form.dckPanel)

            form.FormOutput.Show(form.dckPanel)
            form.FormOutput.DockState = WeifenLuo.WinFormsUI.Docking.DockState.DockLeft
            form.FormOutput.DockState = WeifenLuo.WinFormsUI.Docking.DockState.DockBottom
            form.FormOutput.Hide()
            form.FormQueue.Show(form.dckPanel)
            form.FormQueue.DockState = WeifenLuo.WinFormsUI.Docking.DockState.DockRight
            form.FormQueue.DockState = WeifenLuo.WinFormsUI.Docking.DockState.DockBottom
            form.FormQueue.Hide()
            form.FormWatch.Show(form.dckPanel)
            form.FormWatch.DockState = WeifenLuo.WinFormsUI.Docking.DockState.DockRight
            form.FormWatch.DockState = WeifenLuo.WinFormsUI.Docking.DockState.DockBottom
            form.FormWatch.Hide()

            form.dckPanel.ResumeLayout(True, True)
            form.dckPanel.BringToFront()

            Me.ResumeLayout()

            Dim repositionpfd As Boolean = True

            If File.Exists(My.Computer.FileSystem.SpecialDirectories.Temp & "\12.bin") Then
                repositionpfd = False
            End If

            If Not My.Settings.MostRecentFiles.Contains(caminho) And Path.GetExtension(caminho).ToLower <> ".dwbcs" Then
                My.Settings.MostRecentFiles.Add(caminho)
                Me.UpdateMRUList()
            End If

            form.MdiParent = Me
            form.Show()
            form.MdiParent = Me

            My.Application.ActiveSimulation = form

            If Not repositionpfd Then
                Dim text As String() = File.ReadAllLines(My.Computer.FileSystem.SpecialDirectories.Temp & "\12.bin")
                form.FormSurface.FlowsheetDesignSurface.Zoom = text(0)
                'form.TSTBZoom.Text = CStr(CInt(text(0) * 100)) & "%"
                form.FormSurface.FlowsheetDesignSurface.VerticalScroll.Maximum = 7000
                form.FormSurface.FlowsheetDesignSurface.VerticalScroll.Value = CInt(text(1))
                form.FormSurface.FlowsheetDesignSurface.HorizontalScroll.Maximum = 10000
                form.FormSurface.FlowsheetDesignSurface.HorizontalScroll.Value = CInt(text(2))
            Else
                form.FormSurface.FlowsheetDesignSurface.Zoom = 1
                form.FormSurface.FlowsheetDesignSurface.VerticalScroll.Maximum = 7000
                form.FormSurface.FlowsheetDesignSurface.VerticalScroll.Value = 3500
                form.FormSurface.FlowsheetDesignSurface.HorizontalScroll.Maximum = 10000
                form.FormSurface.FlowsheetDesignSurface.HorizontalScroll.Value = 5000
                For Each obj As Microsoft.Msdn.Samples.GraphicObjects.GraphicObject In form.FormSurface.FlowsheetDesignSurface.drawingObjects
                    obj.X += 5000
                    obj.Y += 3500
                Next
            End If

            form.Invalidate()
            Application.DoEvents()

            'form = Nothing
            Me.ToolStripStatusLabel1.Text = ""

            'delete files

            Dim filespath As String = My.Computer.FileSystem.SpecialDirectories.Temp & pathsep

            If File.Exists(filespath & "1.bin") Then File.Delete(filespath & "1.bin")
            If File.Exists(filespath & "2.bin") Then File.Delete(filespath & "2.bin")
            If File.Exists(filespath & "3.bin") Then File.Delete(filespath & "3.bin")
            If File.Exists(filespath & "4.xml") Then File.Delete(filespath & "4.xml")
            If File.Exists(filespath & "5.bin") Then File.Delete(filespath & "5.bin")
            If File.Exists(filespath & "7.bin") Then File.Delete(filespath & "7.bin")
            If File.Exists(filespath & "8.bin") Then File.Delete(filespath & "8.bin")
            If File.Exists(filespath & "9.bin") Then File.Delete(filespath & "9.bin")
            If File.Exists(filespath & "10.bin") Then File.Delete(filespath & "10.bin")
            If File.Exists(filespath & "11.bin") Then File.Delete(filespath & "11.bin")
            If File.Exists(filespath & "12.bin") Then File.Delete(filespath & "12.bin")
            If File.Exists(filespath & "13.bin") Then File.Delete(filespath & "13.bin")

        Else

            MessageBox.Show(DWSIM.App.GetLocalString("Oarquivonoexisteoufo"), DWSIM.App.GetLocalString("Erro"), MessageBoxButtons.OK, MessageBoxIcon.Error)

        End If


    End Sub

    Function LoadFileForCommandLine(ByVal caminho As String) As FormFlowsheet

        If System.IO.File.Exists(caminho) Then

            Dim rnd As New Random()
            Dim fn As String = rnd.Next(10000, 99999)

            Dim diretorio As String = Path.GetDirectoryName(caminho)
            Dim arquivo As String = Path.GetFileName(caminho)
            Dim arquivoCAB As String = "dwsim" + fn

            Dim formc As FormFlowsheet = New FormFlowsheet()

            Dim ziperror As Boolean = False
            Try
                Dim zp As New ZipFile(caminho)
                'is a zip file
                zp = Nothing
                Call Me.LoadAndExtractZIP(caminho)
            Catch ex As Exception
                ziperror = True
            End Try

            'is not a zip file
            If ziperror Then
                Try
                    If Not DWSIM.App.IsRunningOnMono() Then
                        'Call Me.LoadAndExtractCAB(caminho)
                    Else
                        MsgBox("This file is not loadable when running DWSIM on Mono.", MsgBoxStyle.Critical, "Error!")
                        Return Nothing
                        Exit Function
                    End If
                Catch ex As Exception
                    MessageBox.Show(ex.Message, DWSIM.App.GetLocalString("Erroaoabrirarquivo"), MessageBoxButtons.OK, MessageBoxIcon.Error)
                    formc = Nothing
                End Try
            End If

            Dim mySerializer As Binary.BinaryFormatter = New Binary.BinaryFormatter(Nothing, New System.Runtime.Serialization.StreamingContext())
            Dim fs3 As New FileStream(My.Computer.FileSystem.SpecialDirectories.Temp & "\3.bin", FileMode.Open)
            Try
                formc.FormSurface.FlowsheetDesignSurface.m_drawingObjects = Nothing
                formc.FormSurface.FlowsheetDesignSurface.m_drawingObjects = DirectCast(mySerializer.Deserialize(fs3), Microsoft.Msdn.Samples.GraphicObjects.GraphicObjectCollection)
            Catch ex As System.Runtime.Serialization.SerializationException
                Console.WriteLine("Failed to serialize. Reason: " & ex.Message)
                MessageBox.Show(ex.Message)
            Finally
                fs3.Close()
            End Try
            Dim fs As New FileStream(My.Computer.FileSystem.SpecialDirectories.Temp & "\1.bin", FileMode.Open)
            Try
                formc.Collections = Nothing
                formc.Collections = DirectCast(mySerializer.Deserialize(fs), DWSIM.FormClasses.ClsObjectCollections)
            Catch ex As System.Runtime.Serialization.SerializationException
                Console.WriteLine("Failed to serialize. Reason: " & ex.Message)
                MessageBox.Show(ex.Message)
            Finally
                fs.Close()
            End Try
            Dim fs2 As New FileStream(My.Computer.FileSystem.SpecialDirectories.Temp & "\2.bin", FileMode.Open)
            Try
                formc.Options = Nothing
                formc.Options = DirectCast(mySerializer.Deserialize(fs2), DWSIM.FormClasses.ClsFormOptions)
                If formc.Options.PropertyPackages.Count = 0 Then formc.Options.PropertyPackages = Me.PropertyPackages
            Catch ex As System.Runtime.Serialization.SerializationException
                Console.WriteLine("Failed to serialize. Reason: " & ex.Message)
                MessageBox.Show(ex.Message)
            Finally
                fs2.Close()
            End Try
            If File.Exists(My.Computer.FileSystem.SpecialDirectories.Temp & "\9.bin") Then
                Dim fs9 As New FileStream(My.Computer.FileSystem.SpecialDirectories.Temp & "\9.bin", FileMode.Open)
                Try
                    formc.FormSpreadsheet.dt1 = DirectCast(mySerializer.Deserialize(fs9), Object(,))
                Catch ex As System.Runtime.Serialization.SerializationException
                    Console.WriteLine("Failed to serialize. Reason: " & ex.Message)
                    MessageBox.Show(ex.Message)
                Finally
                    fs9.Close()
                End Try
            End If
            If File.Exists(My.Computer.FileSystem.SpecialDirectories.Temp & "\10.bin") Then
                Dim fs10 As New FileStream(My.Computer.FileSystem.SpecialDirectories.Temp & "\10.bin", FileMode.Open)
                Try
                    formc.FormSpreadsheet.dt2 = DirectCast(mySerializer.Deserialize(fs10), Object(,))
                Catch ex As System.Runtime.Serialization.SerializationException
                    Console.WriteLine("Failed to serialize. Reason: " & ex.Message)
                    MessageBox.Show(ex.Message)
                Finally
                    fs10.Close()
                End Try
            End If

            With formc.Collections
                Dim gObj As Microsoft.Msdn.Samples.GraphicObjects.GraphicObject
                For Each gObj In formc.FormSurface.FlowsheetDesignSurface.drawingObjects
                    Select Case gObj.TipoObjeto
                        Case Microsoft.Msdn.Samples.GraphicObjects.TipoObjeto.Compressor
                            .CLCS_CompressorCollection(gObj.Name).GraphicObject = gObj
                            .CompressorCollection(gObj.Name) = gObj
                        Case Microsoft.Msdn.Samples.GraphicObjects.TipoObjeto.Cooler
                            .CLCS_CoolerCollection(gObj.Name).GraphicObject = gObj
                            .CoolerCollection(gObj.Name) = gObj
                        Case Microsoft.Msdn.Samples.GraphicObjects.TipoObjeto.EnergyStream
                            .CLCS_EnergyStreamCollection(gObj.Name).GraphicObject = gObj
                            .EnergyStreamCollection(gObj.Name) = gObj
                        Case Microsoft.Msdn.Samples.GraphicObjects.TipoObjeto.Heater
                            .CLCS_HeaterCollection(gObj.Name).GraphicObject = gObj
                            .HeaterCollection(gObj.Name) = gObj
                        Case Microsoft.Msdn.Samples.GraphicObjects.TipoObjeto.MaterialStream
                            .CLCS_MaterialStreamCollection(gObj.Name).GraphicObject = gObj
                            .MaterialStreamCollection(gObj.Name) = gObj
                        Case Microsoft.Msdn.Samples.GraphicObjects.TipoObjeto.NodeEn
                            .CLCS_EnergyMixerCollection(gObj.Name).GraphicObject = gObj
                            .MixerENCollection(gObj.Name) = gObj
                        Case Microsoft.Msdn.Samples.GraphicObjects.TipoObjeto.NodeIn
                            .CLCS_MixerCollection(gObj.Name).GraphicObject = gObj
                            .MixerCollection(gObj.Name) = gObj
                        Case Microsoft.Msdn.Samples.GraphicObjects.TipoObjeto.NodeOut
                            .CLCS_SplitterCollection(gObj.Name).GraphicObject = gObj
                            .SplitterCollection(gObj.Name) = gObj
                        Case Microsoft.Msdn.Samples.GraphicObjects.TipoObjeto.Pipe
                            .CLCS_PipeCollection(gObj.Name).GraphicObject = gObj
                            .PipeCollection(gObj.Name) = gObj
                        Case Microsoft.Msdn.Samples.GraphicObjects.TipoObjeto.Pump
                            .CLCS_PumpCollection(gObj.Name).GraphicObject = gObj
                            .PumpCollection(gObj.Name) = gObj
                        Case Microsoft.Msdn.Samples.GraphicObjects.TipoObjeto.Tank
                            .CLCS_TankCollection(gObj.Name).GraphicObject = gObj
                            .TankCollection(gObj.Name) = gObj
                        Case Microsoft.Msdn.Samples.GraphicObjects.TipoObjeto.Expander
                            .CLCS_TurbineCollection(gObj.Name).GraphicObject = gObj
                            .TurbineCollection(gObj.Name) = gObj
                        Case Microsoft.Msdn.Samples.GraphicObjects.TipoObjeto.Valve
                            .CLCS_ValveCollection(gObj.Name).GraphicObject = gObj
                            .ValveCollection(gObj.Name) = gObj
                        Case Microsoft.Msdn.Samples.GraphicObjects.TipoObjeto.Vessel
                            .CLCS_VesselCollection(gObj.Name).GraphicObject = gObj
                            .SeparatorCollection(gObj.Name) = gObj
                        Case Microsoft.Msdn.Samples.GraphicObjects.TipoObjeto.GO_Tabela
                            .ObjectCollection(gObj.Tag).Tabela = gObj
                            CType(gObj, DWSIM.GraphicObjects.TableGraphic).BaseOwner = .ObjectCollection(gObj.Tag)
                        Case Microsoft.Msdn.Samples.GraphicObjects.TipoObjeto.Expander
                            .CLCS_TurbineCollection(gObj.Name).GraphicObject = gObj
                            .TurbineCollection(gObj.Name) = gObj
                        Case Microsoft.Msdn.Samples.GraphicObjects.TipoObjeto.OT_Ajuste
                            .CLCS_AdjustCollection(gObj.Name).GraphicObject = gObj
                            .AdjustCollection(gObj.Name) = gObj
                        Case Microsoft.Msdn.Samples.GraphicObjects.TipoObjeto.OT_Reciclo
                            .CLCS_RecycleCollection(gObj.Name).GraphicObject = gObj
                            .RecycleCollection(gObj.Name) = gObj
                        Case Microsoft.Msdn.Samples.GraphicObjects.TipoObjeto.OT_Especificacao
                            .CLCS_SpecCollection(gObj.Name).GraphicObject = gObj
                            .SpecCollection(gObj.Name) = gObj
                        Case Microsoft.Msdn.Samples.GraphicObjects.TipoObjeto.RCT_Conversion
                            .CLCS_ReactorConversionCollection(gObj.Name).GraphicObject = gObj
                            .ReactorConversionCollection(gObj.Name) = gObj
                        Case Microsoft.Msdn.Samples.GraphicObjects.TipoObjeto.RCT_Equilibrium
                            .CLCS_ReactorEquilibriumCollection(gObj.Name).GraphicObject = gObj
                            .ReactorEquilibriumCollection(gObj.Name) = gObj
                        Case Microsoft.Msdn.Samples.GraphicObjects.TipoObjeto.RCT_Gibbs
                            .CLCS_ReactorGibbsCollection(gObj.Name).GraphicObject = gObj
                            .ReactorGibbsCollection(gObj.Name) = gObj
                        Case Microsoft.Msdn.Samples.GraphicObjects.TipoObjeto.RCT_CSTR
                            .CLCS_ReactorCSTRCollection(gObj.Name).GraphicObject = gObj
                            .ReactorCSTRCollection(gObj.Name) = gObj
                        Case Microsoft.Msdn.Samples.GraphicObjects.TipoObjeto.RCT_PFR
                            .CLCS_ReactorPFRCollection(gObj.Name).GraphicObject = gObj
                            .ReactorPFRCollection(gObj.Name) = gObj
                        Case Microsoft.Msdn.Samples.GraphicObjects.TipoObjeto.HeatExchanger
                            .CLCS_HeatExchangerCollection(gObj.Name).GraphicObject = gObj
                            .HeatExchangerCollection(gObj.Name) = gObj
                        Case Microsoft.Msdn.Samples.GraphicObjects.TipoObjeto.ShortcutColumn
                            .CLCS_ShortcutColumnCollection(gObj.Name).GraphicObject = gObj
                            .ShortcutColumnCollection(gObj.Name) = gObj
                        Case Microsoft.Msdn.Samples.GraphicObjects.TipoObjeto.DistillationColumn
                            .CLCS_DistillationColumnCollection(gObj.Name).GraphicObject = gObj
                            .DistillationColumnCollection(gObj.Name) = gObj
                        Case Microsoft.Msdn.Samples.GraphicObjects.TipoObjeto.AbsorptionColumn
                            .CLCS_AbsorptionColumnCollection(gObj.Name).GraphicObject = gObj
                            .AbsorptionColumnCollection(gObj.Name) = gObj
                        Case Microsoft.Msdn.Samples.GraphicObjects.TipoObjeto.RefluxedAbsorber
                            .CLCS_RefluxedAbsorberCollection(gObj.Name).GraphicObject = gObj
                            .RefluxedAbsorberCollection(gObj.Name) = gObj
                        Case Microsoft.Msdn.Samples.GraphicObjects.TipoObjeto.ReboiledAbsorber
                            .CLCS_ReboiledAbsorberCollection(gObj.Name).GraphicObject = gObj
                            .ReboiledAbsorberCollection(gObj.Name) = gObj
                        Case Microsoft.Msdn.Samples.GraphicObjects.TipoObjeto.OT_EnergyRecycle
                            .CLCS_EnergyRecycleCollection(gObj.Name).GraphicObject = gObj
                            .EnergyRecycleCollection(gObj.Name) = gObj
                        Case Microsoft.MSDN.Samples.GraphicObjects.TipoObjeto.SolidSeparator
                            .CLCS_SolidsSeparatorCollection(gObj.Name).GraphicObject = gObj
                            .SolidsSeparatorCollection(gObj.Name) = gObj
                        Case Microsoft.MSDN.Samples.GraphicObjects.TipoObjeto.Filter
                            .CLCS_FilterCollection(gObj.Name).GraphicObject = gObj
                            .FilterCollection(gObj.Name) = gObj
                    End Select
                Next
            End With

            My.Application.ActiveSimulation = formc

            Dim refill As Boolean = False

            'refill (quick)table items for backwards compatibility
            For Each obj As SimulationObjects_BaseClass In formc.Collections.ObjectCollection.Values
                With obj
                    If .NodeTableItems.Count > 0 Then
                        For Each nvi As DWSIM.Outros.NodeItem In .NodeTableItems.Values
                            If Not nvi.Text.Contains("PROP_") Then
                                refill = True
                                Exit For
                            End If
                        Next
                    End If
                    If refill Then
                        .NodeTableItems.Clear()
                        .QTNodeTableItems.Clear()
                        .FillNodeItems()
                        .QTFillNodeItems()
                    End If
                End With
            Next

            formc.m_IsLoadedFromFile = True

            Return formc

        Else

            Return Nothing

        End If

        Return Nothing

    End Function

    '/ '/ Generates a random string with the given length
    '/ '/ Size of the string '/ If true, generate lowercase string
    '/ Random string
    Private Function RandomString(ByVal size As Integer, ByVal lowerCase As Boolean) As String
        Dim builder As New StringBuilder()
        Dim random As New Random()
        Dim ch As Char
        Dim i As Integer
        For i = 0 To size - 1
            ch = Convert.ToChar(Convert.ToInt32((26 * random.NextDouble() + 65)))
            builder.Append(ch)
        Next
        If lowerCase Then
            Return builder.ToString().ToLower()
        End If
        Return builder.ToString()
    End Function 'RandomString 

    Sub LoadXML(ByVal path As String, Optional ByVal simulationfilename As String = "")

        Dim fls As New FormLS
        Dim ci As CultureInfo = CultureInfo.InvariantCulture

        fls.Show(Me)
        fls.Label1.Text = "Restoring Simulation from XML file"
        fls.Label2.Text = "Loading XML document..."
        Application.DoEvents()

        Try

            Dim xdoc As XDocument = XDocument.Load(path)

            Me.SuspendLayout()

            Dim form As FormFlowsheet = New FormFlowsheet()
            My.Application.CAPEOPENMode = False
            My.Application.ActiveSimulation = form

            fls.Label2.Text = "Loading Flowsheet Settings..."
            Application.DoEvents()

            Dim data As List(Of XElement) = xdoc.Element("DWSIM_Simulation_Data").Element("Settings").Elements.ToList

            form.Options.LoadData(data)

            If simulationfilename <> "" Then Me.filename = simulationfilename Else Me.filename = path

            fls.Label2.Text = "Loading Flowsheet Graphic Objects..."
            Application.DoEvents()

            data = xdoc.Element("DWSIM_Simulation_Data").Element("GraphicObjects").Elements.ToList

            For Each xel As XElement In data
                Dim obj As GraphicObjects.GraphicObject = Nothing
                Dim t As Type = Type.GetType(xel.Element("Type").Value, False)
                If Not t Is Nothing Then obj = Activator.CreateInstance(t)
                If obj Is Nothing Then
                    obj = GraphicObjects.GraphicObject.ReturnInstance(xel.Element("Type").Value)
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
                                Case TipoObjeto.CapeOpenUO
                                    form.FormObjList.TreeViewObj.Nodes("NodeCOUO").Nodes.Add(obj.Name, obj.Tag).Name = obj.Name
                                    form.FormObjList.TreeViewObj.Nodes("NodeCOUO").Nodes(obj.Name).ContextMenuStrip = form.FormObjList.ContextMenuStrip1
                                Case TipoObjeto.SolidSeparator
                                    form.FormObjList.TreeViewObj.Nodes("NodeSS").Nodes.Add(obj.Name, obj.Tag).Name = obj.Name
                                    form.FormObjList.TreeViewObj.Nodes("NodeSS").Nodes(obj.Name).ContextMenuStrip = form.FormObjList.ContextMenuStrip1
                                Case TipoObjeto.Filter
                                    form.FormObjList.TreeViewObj.Nodes("NodeFT").Nodes.Add(obj.Name, obj.Tag).Name = obj.Name
                                    form.FormObjList.TreeViewObj.Nodes("NodeFT").Nodes(obj.Name).ContextMenuStrip = form.FormObjList.ContextMenuStrip1
                            End Select
                        End If
                    End With
                End If
            Next


            For Each xel As XElement In data
                Dim id As String = xel.Element("Name").Value
                If id <> "" Then
                    Dim obj As GraphicObjects.GraphicObject = (From go As GraphicObjects.GraphicObject In
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
            Next

            For Each xel As XElement In data
                Dim id As String = xel.Element("Name").Value
                If id <> "" Then
                    Dim obj As GraphicObjects.GraphicObject = (From go As GraphicObjects.GraphicObject In
                                                           form.FormSurface.FlowsheetDesignSurface.drawingObjects Where go.Name = id).SingleOrDefault
                    If Not obj Is Nothing Then
                        For Each xel2 As XElement In xel.Element("OutputConnectors").Elements
                            If xel2.@IsAttached = True Then
                                Dim objToID = xel2.@AttachedToObjID
                                If objToID <> "" Then
                                    Dim objTo As GraphicObjects.GraphicObject = (From go As GraphicObjects.GraphicObject In
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
                                    Dim objTo As GraphicObjects.GraphicObject = (From go As GraphicObjects.GraphicObject In
                                                                                 form.FormSurface.FlowsheetDesignSurface.drawingObjects Where go.Name = objToID).SingleOrDefault
                                    form.ConnectObject(obj, objTo, -1, xel2.@AttachedToConnIndex)
                                End If
                            End If
                        Next
                    End If
                End If
            Next

            fls.Label2.Text = "Loading Compounds..."
            Application.DoEvents()

            data = xdoc.Element("DWSIM_Simulation_Data").Element("Compounds").Elements.ToList

            For Each xel As XElement In data
                Dim obj As New ConstantProperties
                obj.LoadData(xel.Elements.ToList)
                If Not My.Settings.ReplaceCompoundConstantProperties Then
                    If Me.AvailableComponents.ContainsKey(obj.Name) Then
                        obj = Me.AvailableComponents(obj.Name)
                    End If
                End If
                form.Options.SelectedComponents.Add(obj.Name, obj)
            Next

            fls.Label2.Text = "Loading Property Packages..."
            Application.DoEvents()

            data = xdoc.Element("DWSIM_Simulation_Data").Element("PropertyPackages").Elements.ToList

            For Each xel As XElement In data
                Dim t As Type = Type.GetType(xel.Element("Type").Value, False)
                Dim obj As PropertyPackage = Activator.CreateInstance(t)
                obj.LoadData(xel.Elements.ToList)
                Dim uniqueID As String = Guid.NewGuid.ToString
                obj.UniqueID = uniqueID
                form.Options.PropertyPackages.Add(uniqueID, obj)
            Next

            My.Application.ActiveSimulation = form

            fls.Label2.Text = "Loading Flowsheet Unit Operations and Streams..."
            Application.DoEvents()

            data = xdoc.Element("DWSIM_Simulation_Data").Element("SimulationObjects").Elements.ToList

            For Each xel As XElement In data
                Dim id As String = xel.<Nome>.Value
                Dim t As Type = Type.GetType(xel.Element("Type").Value, False)
                Dim obj As SimulationObjects_BaseClass = Activator.CreateInstance(t)
                obj.FillNodeItems(True)
                obj.QTFillNodeItems()
                Dim gobj As GraphicObjects.GraphicObject = (From go As GraphicObjects.GraphicObject In
                                    form.FormSurface.FlowsheetDesignSurface.drawingObjects Where go.Name = id).SingleOrDefault
                obj.GraphicObject = gobj
                If Not gobj Is Nothing Then
                    form.Collections.ObjectCollection.Add(id, obj)
                    obj.LoadData(xel.Elements.ToList)
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
                            Case Microsoft.Msdn.Samples.GraphicObjects.TipoObjeto.CapeOpenUO
                                .CLCS_CapeOpenUOCollection.Add(id, obj)
                            Case Microsoft.Msdn.Samples.GraphicObjects.TipoObjeto.SolidSeparator
                                .CLCS_SolidsSeparatorCollection.Add(id, obj)
                            Case Microsoft.Msdn.Samples.GraphicObjects.TipoObjeto.Filter
                                .CLCS_FilterCollection.Add(id, obj)
                        End Select
                    End With
                End If
            Next

            For Each so As SimulationObjects_BaseClass In form.Collections.ObjectCollection.Values
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
                        DirectCast(so2.GraphicObject, SpecGraphic).ConnectedToSv = so2.SourceObject.GraphicObject
                    End If
                    If form.Collections.ObjectCollection.ContainsKey(so2.SourceObjectData.m_ID) Then
                        so2.SourceObject = form.Collections.ObjectCollection(so2.SourceObjectData.m_ID)
                        DirectCast(so2.GraphicObject, SpecGraphic).ConnectedToTv = so2.TargetObject.GraphicObject
                    End If
                End If
                If TryCast(so, DWSIM.SimulationObjects.UnitOps.CapeOpenUO) IsNot Nothing Then
                    DirectCast(so, DWSIM.SimulationObjects.UnitOps.CapeOpenUO).UpdateConnectors2()
                    DirectCast(so, DWSIM.SimulationObjects.UnitOps.CapeOpenUO).UpdatePortsFromConnectors()
                End If
            Next

            If Not DWSIM.App.IsRunningOnMono Then
                Dim arrays As New ArrayList
                Dim aNode, aNode2 As TreeNode
                Dim i As Integer = 0
                For Each aNode In form.FormObjList.TreeViewObj.Nodes
                    For Each aNode2 In aNode.Nodes
                        arrays.Add(aNode2.Text)
                        i += 1
                    Next
                Next
                form.FormObjList.ACSC.Clear()
                form.FormObjList.ACSC.AddRange(arrays.ToArray(Type.GetType("System.String")))
                form.FormObjList.TBSearch.AutoCompleteCustomSource = form.FormObjList.ACSC
            End If

            data = xdoc.Element("DWSIM_Simulation_Data").Element("GraphicObjects").Elements.ToList

            For Each xel2 As XElement In (From xel As XElement In data Select xel Where xel.<Type>.Value.Equals("DWSIM.DWSIM.GraphicObjects.TableGraphic")).ToList

                Dim obj As GraphicObjects.GraphicObject = Nothing
                Dim t As Type = Type.GetType(xel2.Element("Type").Value, False)
                If Not t Is Nothing Then obj = Activator.CreateInstance(t)
                If obj Is Nothing Then
                    obj = GraphicObjects.GraphicObject.ReturnInstance(xel2.Element("Type").Value)
                End If
                obj.LoadData(xel2.Elements.ToList)
                DirectCast(obj, DWSIM.GraphicObjects.TableGraphic).BaseOwner = form.Collections.ObjectCollection(xel2.<Owner>.Value)
                form.FormSurface.FlowsheetDesignSurface.drawingObjects.Add(obj)
            Next

            fls.Label2.Text = "Loading Reaction Sets..."
            Application.DoEvents()

            data = xdoc.Element("DWSIM_Simulation_Data").Element("ReactionSets").Elements.ToList

            form.Options.ReactionSets.Clear()

            For Each xel As XElement In data
                Dim obj As New ReactionSet()
                obj.LoadData(xel.Elements.ToList)
                form.Options.ReactionSets.Add(obj.ID, obj)
            Next

            data = xdoc.Element("DWSIM_Simulation_Data").Element("Reactions").Elements.ToList

            For Each xel As XElement In data
                Dim obj As New Reaction()
                obj.LoadData(xel.Elements.ToList)
                form.Options.Reactions.Add(obj.ID, obj)
            Next

            fls.Label2.Text = "Loading Optimization Cases..."
            Application.DoEvents()

            data = xdoc.Element("DWSIM_Simulation_Data").Element("OptimizationCases").Elements.ToList

            For Each xel As XElement In data
                Dim obj As New DWSIM.Optimization.OptimizationCase
                obj.LoadData(xel.Elements.ToList)
                form.Collections.OPT_OptimizationCollection.Add(obj)
            Next

            fls.Label2.Text = "Loading Sensitivity Analysis Cases..."
            Application.DoEvents()

            data = xdoc.Element("DWSIM_Simulation_Data").Element("SensitivityAnalysis").Elements.ToList

            For Each xel As XElement In data
                Dim obj As New DWSIM.Optimization.SensitivityAnalysisCase
                obj.LoadData(xel.Elements.ToList)
                form.Collections.OPT_SensAnalysisCollection.Add(obj)
            Next

            fls.Label2.Text = "Loading Petroleum Assays..."
            Application.DoEvents()

            data = xdoc.Element("DWSIM_Simulation_Data").Element("PetroleumAssays").Elements.ToList

            For Each xel As XElement In data
                Dim obj As New DWSIM.Utilities.PetroleumCharacterization.Assay.Assay()
                obj.LoadData(xel.Elements.ToList)
                form.Options.PetroleumAssays.Add(obj.Name, obj)
            Next

            fls.Label2.Text = "Loading Spreadsheet Data..."
            Application.DoEvents()

            Dim data1 As String = xdoc.Element("DWSIM_Simulation_Data").Element("Spreadsheet").Element("Data1").Value
            Dim data2 As String = xdoc.Element("DWSIM_Simulation_Data").Element("Spreadsheet").Element("Data2").Value

            If data1 <> "" Then form.FormSpreadsheet.CopyDT1FromString(data1)
            If data2 <> "" Then form.FormSpreadsheet.CopyDT2FromString(data2)

            For Each pp As DWSIM.SimulationObjects.PropertyPackages.PropertyPackage In form.Options.PropertyPackages.Values
                If pp.ConfigForm Is Nothing Then pp.ReconfigureConfigForm()
            Next

            form.Options.NotSelectedComponents = New Dictionary(Of String, DWSIM.ClassesBasicasTermodinamica.ConstantProperties)

            Dim tmpc As DWSIM.ClassesBasicasTermodinamica.ConstantProperties
            For Each tmpc In Me.AvailableComponents.Values
                Dim newc As New DWSIM.ClassesBasicasTermodinamica.ConstantProperties
                newc = tmpc
                If Not form.Options.SelectedComponents.ContainsKey(tmpc.Name) Then
                    form.Options.NotSelectedComponents.Add(tmpc.Name, newc)
                End If
            Next

            fls.Label2.Text = "Restoring Window Layout..."
            Application.DoEvents()

            My.Application.ActiveSimulation = form

            Me.ResumeLayout()
            m_childcount += 1

            form.MdiParent = Me
            form.m_IsLoadedFromFile = True

            form.Options.FilePath = Me.filename
            form.WriteToLog(DWSIM.App.GetLocalString("Arquivo") & Me.filename & DWSIM.App.GetLocalString("carregadocomsucesso"), Color.Blue, DWSIM.FormClasses.TipoAviso.Informacao)
            form.Text += " (" + Me.filename + ")"

            ' Set DockPanel properties
            form.dckPanel.ActiveAutoHideContent = Nothing
            form.dckPanel.Parent = form

            Me.tmpform2 = form
            'form.dckPanel.SuspendLayout(True)
            form.FormLog.DockPanel = Nothing
            form.FormObjList.DockPanel = Nothing
            form.FormProps.DockPanel = Nothing
            form.FormMatList.DockPanel = Nothing
            form.FormSpreadsheet.DockPanel = Nothing
            form.FormWatch.DockPanel = Nothing
            form.FormSurface.DockPanel = Nothing

            Dim pnl As String = xdoc.Element("DWSIM_Simulation_Data").Element("PanelLayout").Value

            Dim myfile As String = My.Computer.FileSystem.GetTempFileName()
            File.WriteAllText(myfile, pnl)
            form.dckPanel.LoadFromXml(myfile, New DeserializeDockContent(AddressOf Me.ReturnForm))
            File.Delete(myfile)

            form.FormLog.Show(form.dckPanel)
            form.FormObjListView.Show(form.dckPanel)
            form.FormObjList.Show(form.dckPanel)
            form.FormProps.Show(form.dckPanel)
            'form.FormMatList.Show(form.dckPanel)
            form.FormSpreadsheet.Show(form.dckPanel)
            form.FormSurface.Show(form.dckPanel)

            form.dckPanel.BringToFront()

            form.dckPanel.UpdateDockWindowZOrder(DockStyle.Fill, True)

            fls.Label2.Text = "Done!"
            Application.DoEvents()

            Me.Invalidate()
            Application.DoEvents()

            Dim mypath As String = simulationfilename
            If mypath = "" Then mypath = [path]
            If Not My.Settings.MostRecentFiles.Contains(mypath) And IO.Path.GetExtension(mypath).ToLower <> ".dwbcs" Then
                My.Settings.MostRecentFiles.Add(mypath)
                Me.UpdateMRUList()
            End If

            form.MdiParent = Me
            form.Show()
            form.MdiParent = Me

            My.Application.ActiveSimulation = form

            'form.FrmStSim1.Init()

            form.Invalidate()

            'form = Nothing

            If xdoc.Element("DWSIM_Simulation_Data").Element("FlowsheetView") IsNot Nothing Then
                Dim flsconfig As String = xdoc.Element("DWSIM_Simulation_Data").Element("FlowsheetView").Value
                If flsconfig <> "" Then
                    form.FormSurface.FlowsheetDesignSurface.Zoom = Single.Parse(flsconfig.Split(";")(0), ci)
                    form.FormSurface.FlowsheetDesignSurface.VerticalScroll.Value = Integer.Parse(flsconfig.Split(";")(1))
                    form.FormSurface.FlowsheetDesignSurface.HorizontalScroll.Value = Integer.Parse(flsconfig.Split(";")(2))
                End If
            End If

            form.TSTBZoom.Text = Format(form.FormSurface.FlowsheetDesignSurface.Zoom, "#%")

            Application.DoEvents()

        Catch ex As Exception

            MessageBox.Show(ex.ToString, "Error loading XML file", MessageBoxButtons.OK, MessageBoxIcon.Error)

        Finally

            fls.Close()
            fls = Nothing

            Me.ResumeLayout()
            Me.ToolStripStatusLabel1.Text = ""
            Application.DoEvents()

        End Try

    End Sub

    Sub SaveXML(ByVal path As String, ByVal form As FormFlowsheet, Optional ByVal simulationfilename As String = "")

        Dim xdoc As New XDocument()
        Dim xel As XElement

        Dim ci As CultureInfo = CultureInfo.InvariantCulture

        xdoc.Add(New XElement("DWSIM_Simulation_Data"))

        xdoc.Element("DWSIM_Simulation_Data").Add(New XElement("Settings"))
        xel = xdoc.Element("DWSIM_Simulation_Data").Element("Settings")

        xel.Add(form.Options.SaveData().ToArray())

        xdoc.Element("DWSIM_Simulation_Data").Add(New XElement("SimulationObjects"))
        xel = xdoc.Element("DWSIM_Simulation_Data").Element("SimulationObjects")

        For Each so As SimulationObjects_BaseClass In form.Collections.ObjectCollection.Values
            xel.Add(New XElement("SimulationObject", {so.SaveData().ToArray()}))
        Next

        xdoc.Element("DWSIM_Simulation_Data").Add(New XElement("GraphicObjects"))
        xel = xdoc.Element("DWSIM_Simulation_Data").Element("GraphicObjects")

        For Each go As Microsoft.Msdn.Samples.GraphicObjects.GraphicObject In form.FormSurface.FlowsheetDesignSurface.drawingObjects
            If Not go.IsConnector Then xel.Add(New XElement("GraphicObject", go.SaveData().ToArray()))
        Next

        xdoc.Element("DWSIM_Simulation_Data").Add(New XElement("PropertyPackages"))
        xel = xdoc.Element("DWSIM_Simulation_Data").Element("PropertyPackages")

        For Each pp As KeyValuePair(Of String, PropertyPackage) In form.Options.PropertyPackages
            Dim createdms As Boolean = False
            If pp.Value.CurrentMaterialStream Is Nothing Then
                Dim ms As New Streams.MaterialStream("", "", form, pp.Value)
                form.AddComponentsRows(ms)
                pp.Value.CurrentMaterialStream = ms
                createdms = True
            End If
            xel.Add(New XElement("PropertyPackage", {New XElement("ID", pp.Key),
                                                     pp.Value.SaveData().ToArray()}))
            If createdms Then pp.Value.CurrentMaterialStream = Nothing
        Next

        xdoc.Element("DWSIM_Simulation_Data").Add(New XElement("Compounds"))
        xel = xdoc.Element("DWSIM_Simulation_Data").Element("Compounds")

        For Each cp As ConstantProperties In form.Options.SelectedComponents.Values
            xel.Add(New XElement("Compound", cp.SaveData().ToArray()))
        Next

        xdoc.Element("DWSIM_Simulation_Data").Add(New XElement("ReactionSets"))
        xel = xdoc.Element("DWSIM_Simulation_Data").Element("ReactionSets")

        For Each pp As KeyValuePair(Of String, ReactionSet) In form.Options.ReactionSets
            xel.Add(New XElement("ReactionSet", pp.Value.SaveData().ToArray()))
        Next

        xdoc.Element("DWSIM_Simulation_Data").Add(New XElement("Reactions"))
        xel = xdoc.Element("DWSIM_Simulation_Data").Element("Reactions")

        For Each pp As KeyValuePair(Of String, Reaction) In form.Options.Reactions
            xel.Add(New XElement("Reaction", {pp.Value.SaveData().ToArray()}))
        Next

        xdoc.Element("DWSIM_Simulation_Data").Add(New XElement("OptimizationCases"))
        xel = xdoc.Element("DWSIM_Simulation_Data").Element("OptimizationCases")

        For Each pp As DWSIM.Optimization.OptimizationCase In form.Collections.OPT_OptimizationCollection
            xel.Add(New XElement("OptimizationCase", {pp.SaveData().ToArray()}))
        Next

        xdoc.Element("DWSIM_Simulation_Data").Add(New XElement("SensitivityAnalysis"))
        xel = xdoc.Element("DWSIM_Simulation_Data").Element("SensitivityAnalysis")

        For Each pp As DWSIM.Optimization.SensitivityAnalysisCase In form.Collections.OPT_SensAnalysisCollection
            xel.Add(New XElement("SensitivityAnalysisCase", {pp.SaveData().ToArray()}))
        Next

        xdoc.Element("DWSIM_Simulation_Data").Add(New XElement("PetroleumAssays"))
        xel = xdoc.Element("DWSIM_Simulation_Data").Element("PetroleumAssays")

        If form.Options.PetroleumAssays Is Nothing Then form.Options.PetroleumAssays = New Dictionary(Of String, DWSIM.Utilities.PetroleumCharacterization.Assay.Assay)

        For Each pp As KeyValuePair(Of String, DWSIM.Utilities.PetroleumCharacterization.Assay.Assay) In form.Options.PetroleumAssays
            xel.Add(New XElement("Assay", pp.Value.SaveData().ToArray()))
        Next

        xdoc.Element("DWSIM_Simulation_Data").Add(New XElement("Spreadsheet"))
        xdoc.Element("DWSIM_Simulation_Data").Element("Spreadsheet").Add(New XElement("Data1"))
        xdoc.Element("DWSIM_Simulation_Data").Element("Spreadsheet").Add(New XElement("Data2"))
        xdoc.Element("DWSIM_Simulation_Data").Element("Spreadsheet").Element("Data1").Value = form.FormSpreadsheet.CopyDT1ToString()
        xdoc.Element("DWSIM_Simulation_Data").Element("Spreadsheet").Element("Data2").Value = form.FormSpreadsheet.CopyDT2ToString()

        Dim flsconfig As New StringBuilder()

        With flsconfig
            .Append(form.FormSurface.FlowsheetDesignSurface.Zoom.ToString(ci) & ";")
            .Append(form.FormSurface.FlowsheetDesignSurface.VerticalScroll.Value & ";")
            .Append(form.FormSurface.FlowsheetDesignSurface.HorizontalScroll.Value)
        End With

        xdoc.Element("DWSIM_Simulation_Data").Add(New XElement("FlowsheetView"))
        xel = xdoc.Element("DWSIM_Simulation_Data").Element("FlowsheetView")

        xel.Add(flsconfig.ToString)

        xdoc.Element("DWSIM_Simulation_Data").Add(New XElement("PanelLayout"))
        xel = xdoc.Element("DWSIM_Simulation_Data").Element("PanelLayout")

        Dim myfile As String = My.Computer.FileSystem.GetTempFileName()
        form.dckPanel.SaveAsXml(myfile, Encoding.UTF8)
        xel.Add(File.ReadAllText(myfile).ToString)
        File.Delete(myfile)

        xdoc.Save(path)

        Me.UIThread(New Action(Sub()
                                   Dim mypath As String = simulationfilename
                                   If mypath = "" Then mypath = [path]
                                   'process recent files list
                                   If Not My.Settings.MostRecentFiles.Contains(mypath) Then
                                       My.Settings.MostRecentFiles.Add(mypath)
                                       If Not My.Application.CommandLineArgs.Count > 1 Then Me.UpdateMRUList()
                                   End If
                                   form.Options.FilePath = Me.filename
                                   form.Text = form.Options.SimNome + " (" + form.Options.FilePath + ")"
                                   form.WriteToLog(DWSIM.App.GetLocalString("Arquivo") & Me.filename & DWSIM.App.GetLocalString("salvocomsucesso"), Color.Blue, DWSIM.FormClasses.TipoAviso.Informacao)
                                   Me.ToolStripStatusLabel1.Text = ""
                               End Sub))

        Application.DoEvents()

    End Sub

    Sub SaveF(ByVal caminho As String, ByVal form As FormFlowsheet)

        Dim rndfolder As String = My.Computer.FileSystem.SpecialDirectories.Temp & pathsep & RandomString(8, True) & pathsep

        Directory.CreateDirectory(rndfolder)

        Dim mySerializer As Binary.BinaryFormatter = New Binary.BinaryFormatter(Nothing, New System.Runtime.Serialization.StreamingContext())
        Dim fs As New FileStream(rndfolder & "1.bin", FileMode.Create)
        Try
            mySerializer.Serialize(fs, form.Collections)
        Catch ex As Exception
            Console.WriteLine("Failed to serialize. Reason: " & ex.Message)
            MessageBox.Show(ex.Message)
        Finally
            fs.Close()
        End Try
        Dim fs2 As New FileStream(rndfolder & "2.bin", FileMode.Create)
        Try
            mySerializer.Serialize(fs2, form.Options)
        Catch ex As System.Runtime.Serialization.SerializationException
            Console.WriteLine("Failed to serialize. Reason: " & ex.Message)
            MessageBox.Show(ex.Message)
        Finally
            fs2.Close()
        End Try
        Dim fs3 As New FileStream(rndfolder & "3.bin", FileMode.Create)
        Try
            mySerializer.Serialize(fs3, form.FormSurface.FlowsheetDesignSurface.drawingObjects)
        Catch ex As System.Runtime.Serialization.SerializationException
            Console.WriteLine("Failed to serialize. Reason: " & ex.Message)
            MessageBox.Show(ex.Message)
        Finally
            fs3.Close()
        End Try
        Try
            form.dckPanel.SaveAsXml(rndfolder & "4.xml")
        Catch ex As Exception
            Console.WriteLine("Failed to serialize. Reason: " & ex.Message)
            MessageBox.Show(ex.Message)
        Finally

        End Try
        Try
            TreeViewDataAccess.SaveTreeViewData(form.FormObjList.TreeViewObj, rndfolder & "5.bin")
        Catch ex As System.Runtime.Serialization.SerializationException
            Console.WriteLine("Failed to serialize. Reason: " & ex.Message)
            MessageBox.Show(ex.Message)
        Finally

        End Try
        Dim fs7 As New FileStream(rndfolder & "7.bin", FileMode.Create)
        Try
            mySerializer.Serialize(fs7, form.Options.SimNome)
        Catch ex As System.Runtime.Serialization.SerializationException
            Console.WriteLine("Failed to serialize. Reason: " & ex.Message)
            MessageBox.Show(ex.Message)
        Finally
            fs7.Close()
        End Try
        Dim fs8 As New FileStream(rndfolder & "8.bin", FileMode.Create)
        Try
            mySerializer.Serialize(fs8, form.FormLog.GridDT)
        Catch ex As System.Runtime.Serialization.SerializationException
            Console.WriteLine("Failed to serialize. Reason: " & ex.Message)
            MessageBox.Show(ex.Message)
        Finally
            fs8.Close()
        End Try
        form.FormSpreadsheet.CopyToDT()
        Dim fs9 As New FileStream(rndfolder & "9.bin", FileMode.Create)
        Try
            mySerializer.Serialize(fs9, form.FormSpreadsheet.dt1)
        Catch ex As System.Runtime.Serialization.SerializationException
            Console.WriteLine("Failed to serialize. Reason: " & ex.Message)
            MessageBox.Show(ex.Message)
        Finally
            fs9.Close()
        End Try
        Dim fs10 As New FileStream(rndfolder & "10.bin", FileMode.Create)
        Try
            mySerializer.Serialize(fs10, form.FormSpreadsheet.dt2)
        Catch ex As System.Runtime.Serialization.SerializationException
            Console.WriteLine("Failed to serialize. Reason: " & ex.Message)
            MessageBox.Show(ex.Message)
        Finally
            fs10.Close()
        End Try
        Dim fs11 As New FileStream(rndfolder & "11.bin", FileMode.Create)
        Try
            mySerializer.Serialize(fs11, form.FormWatch.items)
        Catch ex As System.Runtime.Serialization.SerializationException
            Console.WriteLine("Failed to serialize. Reason: " & ex.Message)
            MessageBox.Show(ex.Message)
        Finally
            fs11.Close()
        End Try

        Dim flsconfig As New StringBuilder()

        With flsconfig
            .AppendLine(form.FormSurface.FlowsheetDesignSurface.Zoom)
            .AppendLine(form.FormSurface.FlowsheetDesignSurface.VerticalScroll.Value)
            .AppendLine(form.FormSurface.FlowsheetDesignSurface.HorizontalScroll.Value)
        End With

        File.WriteAllText(rndfolder & "12.bin", flsconfig.ToString)

        Dim fs13 As New FileStream(rndfolder & "13.bin", FileMode.Create)
        Try
            If form.FlowsheetStates Is Nothing Then form.FlowsheetStates = New Dictionary(Of Date, FlowsheetState)
            mySerializer.Serialize(fs13, form.FlowsheetStates)
        Catch ex As System.Runtime.Serialization.SerializationException
            Console.WriteLine("Failed to serialize. Reason: " & ex.Message)
            MessageBox.Show(ex.Message)
        Finally
            fs13.Close()
        End Try

        Dim pwd As String = Nothing
        If form.Options.UsePassword Then pwd = form.Options.Password

        Try
            Call Me.SaveZIP(caminho, rndfolder, pwd)
        Catch ex As Exception
            form.WriteToLog("Error saving file: " & ex.Message, Color.Red, DWSIM.FormClasses.TipoAviso.Erro)
        End Try

        Dim ext As String = Path.GetExtension(caminho)

        Me.UIThread(New System.Action(Sub()
                                          If ext <> ".dwbcs" Then
                                              'lista dos mais recentes, modificar
                                              With My.Settings.MostRecentFiles
                                                  If Not .Contains(Me.filename) Then
                                                      If My.Settings.MostRecentFiles.Count = 3 Then
                                                          .Item(2) = .Item(1)
                                                          .Item(1) = .Item(0)
                                                          .Item(0) = Me.filename
                                                      ElseIf My.Settings.MostRecentFiles.Count = 2 Then
                                                          .Add(.Item(1))
                                                          .Item(1) = .Item(0)
                                                          .Item(0) = Me.filename
                                                      ElseIf My.Settings.MostRecentFiles.Count = 1 Then
                                                          .Add(.Item(0))
                                                          .Item(0) = Me.filename
                                                      ElseIf My.Settings.MostRecentFiles.Count = 0 Then
                                                          .Add(Me.filename)
                                                      End If
                                                  End If
                                              End With
                                              'processar lista de arquivos recentes
                                              If Not My.Settings.MostRecentFiles.Contains(caminho) Then
                                                  My.Settings.MostRecentFiles.Add(caminho)
                                                  If Not My.Application.CommandLineArgs.Count > 1 Then Me.UpdateMRUList()
                                              End If
                                              form.Options.FilePath = Me.filename
                                              form.Text = form.Options.SimNome + " (" + form.Options.FilePath + ")"
                                              form.WriteToLog(DWSIM.App.GetLocalString("Arquivo") & Me.filename & DWSIM.App.GetLocalString("salvocomsucesso"), Color.Blue, DWSIM.FormClasses.TipoAviso.Informacao)
                                          End If
                                      End Sub))

    End Sub

    Private Function IsZipFilePasswordProtected(ByVal ZipFile As String) As Boolean
        Using fsIn As New FileStream(ZipFile, FileMode.Open, FileAccess.Read)
            Using zipInStream As New ZipInputStream(fsIn)
                Dim zEntry As ZipEntry = zipInStream.GetNextEntry()
                Return zEntry.IsCrypted
            End Using
        End Using
    End Function

    Function LoadAndExtractZIP(ByVal caminho As String) As Boolean

        Dim pathtosave As String = My.Computer.FileSystem.SpecialDirectories.Temp + Path.DirectorySeparatorChar

        If (caminho.Length < 1) Then
            MsgBox("Usage UnzipFile NameOfFile")
            Return False
        ElseIf Not File.Exists(caminho) Then
            MsgBox("Cannot find file '{0}'", caminho)
            Return False
        Else
            Dim pwd As String = Nothing
            If IsZipFilePasswordProtected(caminho) Then
                Dim fp As New FormPassword
                If fp.ShowDialog() = Windows.Forms.DialogResult.OK Then
                    pwd = fp.tbPassword.Text
                End If
            End If

            Try
                Using stream As ZipInputStream = New ZipInputStream(File.OpenRead(caminho))
                    stream.Password = pwd
                    Dim entry As ZipEntry
Label_00CC:
                    entry = stream.GetNextEntry()
                    Do While (Not entry Is Nothing)
                        Dim fileName As String = Path.GetFileName(entry.Name)
                        If (fileName <> String.Empty) Then
                            Using stream2 As FileStream = File.Create(pathtosave + Path.GetFileName(entry.Name))
                                Dim count As Integer = 2048
                                Dim buffer As Byte() = New Byte(2048) {}
                                Do While True
                                    count = stream.Read(buffer, 0, buffer.Length)
                                    If (count <= 0) Then
                                        GoTo Label_00CC
                                    End If
                                    stream2.Write(buffer, 0, count)
                                Loop
                            End Using
                        End If
                        entry = stream.GetNextEntry
                    Loop
                End Using
                Return True
            Catch ex As Exception
                MessageBox.Show(ex.Message, DWSIM.App.GetLocalString("Erroaoabrirarquivo"), MessageBoxButtons.OK, MessageBoxIcon.Error)
                Return False
            End Try
        End If

    End Function

    Function LoadAndExtractXMLZIP(ByVal caminho As String) As Boolean

        Dim pathtosave As String = My.Computer.FileSystem.SpecialDirectories.Temp + Path.DirectorySeparatorChar
        Dim fullname As String = ""

        Dim pwd As String = Nothing
        If IsZipFilePasswordProtected(caminho) Then
            Dim fp As New FormPassword
            If fp.ShowDialog() = Windows.Forms.DialogResult.OK Then
                pwd = fp.tbPassword.Text
            End If
        End If

        Try
            Using stream As ZipInputStream = New ZipInputStream(File.OpenRead(caminho))
                stream.Password = pwd
                Dim entry As ZipEntry
Label_00CC:
                entry = stream.GetNextEntry()
                Do While (Not entry Is Nothing)
                    Dim fileName As String = Path.GetFileName(entry.Name)
                    If (fileName <> String.Empty) Then
                        Using stream2 As FileStream = File.Create(pathtosave + Path.GetFileName(entry.Name))
                            Dim count As Integer = 2048
                            Dim buffer As Byte() = New Byte(2048) {}
                            Do While True
                                count = stream.Read(buffer, 0, buffer.Length)
                                If (count <= 0) Then
                                    fullname = pathtosave + Path.GetFileName(entry.Name)
                                    GoTo Label_00CC
                                End If
                                stream2.Write(buffer, 0, count)
                            Loop
                        End Using
                    End If
                    entry = stream.GetNextEntry
                Loop
            End Using
            LoadXML(fullname, caminho)
            File.Delete(fullname)
            Return True
        Catch ex As Exception
            MessageBox.Show(ex.Message, DWSIM.App.GetLocalString("Erroaoabrirarquivo"), MessageBoxButtons.OK, MessageBoxIcon.Error)
            Return False
        End Try

    End Function

    Sub SaveZIP(ByVal caminho As String, ByVal filespath As String, ByVal password As String)

        Dim i_Files As ArrayList = New ArrayList()
        If File.Exists(filespath & "1.bin") Then i_Files.Add(filespath & "1.bin")
        If File.Exists(filespath & "2.bin") Then i_Files.Add(filespath & "2.bin")
        If File.Exists(filespath & "3.bin") Then i_Files.Add(filespath & "3.bin")
        If File.Exists(filespath & "4.xml") Then i_Files.Add(filespath & "4.xml")
        If File.Exists(filespath & "5.bin") Then i_Files.Add(filespath & "5.bin")
        If File.Exists(filespath & "7.bin") Then i_Files.Add(filespath & "7.bin")
        If File.Exists(filespath & "8.bin") Then i_Files.Add(filespath & "8.bin")
        If File.Exists(filespath & "9.bin") Then i_Files.Add(filespath & "9.bin")
        If File.Exists(filespath & "10.bin") Then i_Files.Add(filespath & "10.bin")
        If File.Exists(filespath & "11.bin") Then i_Files.Add(filespath & "11.bin")
        If File.Exists(filespath & "12.bin") Then i_Files.Add(filespath & "12.bin")
        If File.Exists(filespath & "13.bin") Then i_Files.Add(filespath & "13.bin")

        Dim astrFileNames() As String = i_Files.ToArray(GetType(String))
        Dim strmZipOutputStream As ZipOutputStream

        strmZipOutputStream = New ZipOutputStream(File.Create(caminho))

        ' Compression Level: 0-9
        ' 0: no(Compression)
        ' 9: maximum compression
        strmZipOutputStream.SetLevel(9)

        'save with password, if set
        strmZipOutputStream.Password = password

        Dim strFile As String

        For Each strFile In astrFileNames

            Dim strmFile As FileStream = File.OpenRead(strFile)
            Dim abyBuffer(strmFile.Length - 1) As Byte

            strmFile.Read(abyBuffer, 0, abyBuffer.Length)
            Dim objZipEntry As ZipEntry = New ZipEntry(strFile)

            objZipEntry.DateTime = DateTime.Now
            objZipEntry.Size = strmFile.Length
            strmFile.Close()
            strmZipOutputStream.PutNextEntry(objZipEntry)
            strmZipOutputStream.Write(abyBuffer, 0, abyBuffer.Length)

        Next

        strmZipOutputStream.Finish()
        strmZipOutputStream.Close()

        Dim ext As String = Path.GetExtension(caminho)
        Dim diretorio As String = Path.GetDirectoryName(caminho)

        If ext <> ".dwbcs" Then
            If File.Exists(caminho) Then
                File.Copy(caminho, diretorio + Path.DirectorySeparatorChar + Path.GetFileNameWithoutExtension(caminho) + ".dwbak", True)
            End If
        End If

        Directory.Delete(filespath, True)

    End Sub

    Sub SaveXMLZIP(ByVal zipfilename As String, ByVal form As FormFlowsheet)

        Dim xmlfile As String = My.Computer.FileSystem.GetTempFileName
        Me.SaveXML(xmlfile, form, zipfilename)

        Dim i_Files As ArrayList = New ArrayList()
        If File.Exists(xmlfile) Then i_Files.Add(xmlfile)

        Dim astrFileNames() As String = i_Files.ToArray(GetType(String))
        Dim strmZipOutputStream As ZipOutputStream

        strmZipOutputStream = New ZipOutputStream(File.Create(zipfilename))

        ' Compression Level: 0-9
        ' 0: no(Compression)
        ' 9: maximum compression
        strmZipOutputStream.SetLevel(9)

        'save with password, if set
        If form.Options.UsePassword Then strmZipOutputStream.Password = form.Options.Password

        Dim strFile As String

        For Each strFile In astrFileNames

            Dim strmFile As FileStream = File.OpenRead(strFile)
            Dim abyBuffer(strmFile.Length - 1) As Byte

            strmFile.Read(abyBuffer, 0, abyBuffer.Length)
            Dim objZipEntry As ZipEntry = New ZipEntry(strFile)

            objZipEntry.DateTime = DateTime.Now
            objZipEntry.Size = strmFile.Length
            strmFile.Close()
            strmZipOutputStream.PutNextEntry(objZipEntry)
            strmZipOutputStream.Write(abyBuffer, 0, abyBuffer.Length)

        Next

        strmZipOutputStream.Finish()
        strmZipOutputStream.Close()

        File.Delete(xmlfile)

    End Sub

    Sub LoadFileDialog()

        If Me.OpenFileDialog1.ShowDialog() = Windows.Forms.DialogResult.OK Then
            Select Case Me.OpenFileDialog1.FilterIndex
                Case 1
simx:               Dim myStream As System.IO.FileStream
                    myStream = Me.OpenFileDialog1.OpenFile()
                    If Not (myStream Is Nothing) Then
                        Dim nome = myStream.Name
                        myStream.Close()
                        Me.filename = nome
                        Me.ToolStripStatusLabel1.Text = DWSIM.App.GetLocalString("Abrindosimulao") + " " + nome + "..."
                        Application.DoEvents()
                        Me.LoadXML(Me.filename)
                    End If
                Case 2
simx2:              Dim myStream As System.IO.FileStream
                    myStream = Me.OpenFileDialog1.OpenFile()
                    If Not (myStream Is Nothing) Then
                        Dim nome = myStream.Name
                        myStream.Close()
                        Me.filename = nome
                        Me.ToolStripStatusLabel1.Text = DWSIM.App.GetLocalString("Abrindosimulao") + " " + nome + "..."
                        Application.DoEvents()
                        Me.LoadAndExtractXMLZIP(Me.filename)
                    End If
                Case 3
sim:                Dim myStream As System.IO.FileStream
                    myStream = Me.OpenFileDialog1.OpenFile()
                    If Not (myStream Is Nothing) Then
                        Dim nome = myStream.Name
                        myStream.Close()
                        Me.filename = nome
                        Me.ToolStripStatusLabel1.Text = DWSIM.App.GetLocalString("Abrindosimulao") + " " + nome + "..."
                        Application.DoEvents()
                        Me.LoadF(Me.filename)
                    End If
                Case 4
csd:                Dim NewMDIChild As New FormCompoundCreator()
                    NewMDIChild.MdiParent = Me
                    NewMDIChild.Show()
                    Dim objStreamReader As New FileStream(Me.OpenFileDialog1.FileName, FileMode.Open)
                    Dim x As New BinaryFormatter()
                    NewMDIChild.mycase = x.Deserialize(objStreamReader)
                    NewMDIChild.mycase.Filename = Me.OpenFileDialog1.FileName
                    objStreamReader.Close()
                    NewMDIChild.WriteData()
                    If Not My.Settings.MostRecentFiles.Contains(Me.OpenFileDialog1.FileName) Then
                        My.Settings.MostRecentFiles.Add(Me.OpenFileDialog1.FileName)
                        Me.UpdateMRUList()
                    End If
                Case 5
rsd:                Dim NewMDIChild As New FormDataRegression()
                    NewMDIChild.MdiParent = Me
                    NewMDIChild.Show()
                    Dim objStreamReader As New FileStream(Me.OpenFileDialog1.FileName, FileMode.Open)
                    Dim x As New BinaryFormatter()
                    NewMDIChild.currcase = x.Deserialize(objStreamReader)
                    objStreamReader.Close()
                    NewMDIChild.LoadCase(NewMDIChild.currcase, False)
                    If Not My.Settings.MostRecentFiles.Contains(Me.OpenFileDialog1.FileName) Then
                        My.Settings.MostRecentFiles.Add(Me.OpenFileDialog1.FileName)
                        Me.UpdateMRUList()
                    End If
                Case 6
                    Select Case Path.GetExtension(Me.OpenFileDialog1.FileName).ToLower()
                        Case ".dwxml"
                            GoTo simx
                        Case ".dwxmz"
                            GoTo simx2
                        Case ".dwsim"
                            GoTo sim
                        Case ".dwcsd"
                            GoTo csd
                        Case ".dwrsd"
                            GoTo rsd
                    End Select
            End Select
        End If

    End Sub

    Sub SaveFileDialog()

        If TypeOf Me.ActiveMdiChild Is FormFlowsheet Then
            Dim myStream As System.IO.FileStream
            If Me.SaveFileDialog1.ShowDialog() = Windows.Forms.DialogResult.OK Then
                myStream = Me.SaveFileDialog1.OpenFile()
                Me.filename = myStream.Name
                myStream.Close()
                If Not (myStream Is Nothing) Then
                    Me.ToolStripStatusLabel1.Text = DWSIM.App.GetLocalString("Salvandosimulao") + " (" + Me.filename + ")"
                    Application.DoEvents()
                    If Path.GetExtension(Me.filename).ToLower = ".dwxml" Then
                        Dim t As Task = Task.Factory.StartNew(Sub()
                                                                  Me.SaveXML(Me.filename, Me.ActiveMdiChild)
                                                              End Sub)
                        While Not t.IsCompleted
                            Application.DoEvents()
                        End While
                    ElseIf Path.GetExtension(Me.filename).ToLower = ".dwxmz" Then
                        Dim t As Task = Task.Factory.StartNew(Sub()
                                                                  Me.SaveXMLZIP(Me.filename, Me.ActiveMdiChild)
                                                              End Sub)
                        While Not t.IsCompleted
                            Application.DoEvents()
                        End While
                    Else
                        Me.bgSaveFile.RunWorkerAsync()
                    End If
                End If
            End If
        ElseIf TypeOf Me.ActiveMdiChild Is FormCompoundCreator Then
            If Me.SaveStudyDlg.ShowDialog(Me) = Windows.Forms.DialogResult.OK Then
                CType(Me.ActiveMdiChild, FormCompoundCreator).StoreData()
                Dim objStreamWriter As New FileStream(Me.SaveStudyDlg.FileName, FileMode.OpenOrCreate)
                Dim x As New BinaryFormatter
                x.Serialize(objStreamWriter, CType(Me.ActiveMdiChild, FormCompoundCreator).mycase)
                objStreamWriter.Close()
                CType(Me.ActiveMdiChild, FormCompoundCreator).SetCompCreatorSaveStatus(True)
                Me.filename = Me.SaveStudyDlg.FileName
                Me.ActiveMdiChild.Text = Me.filename
            End If
        ElseIf TypeOf Me.ActiveMdiChild Is FormDataRegression Then
            If Me.SaveRegStudyDlg.ShowDialog(Me) = Windows.Forms.DialogResult.OK Then
                Dim objStreamWriter As New FileStream(Me.SaveRegStudyDlg.FileName, FileMode.OpenOrCreate)
                Dim x As New BinaryFormatter
                x.Serialize(objStreamWriter, CType(Me.ActiveMdiChild, FormDataRegression).StoreCase())
                objStreamWriter.Close()
                Me.filename = Me.SaveRegStudyDlg.FileName
                Me.ActiveMdiChild.Text = Me.filename
            End If
        End If



    End Sub

    Sub SaveFileDialog_NoBG()

        Dim myStream As System.IO.FileStream

        Me.SaveFileDialog1.Filter = DWSIM.App.GetLocalString("SimulaesdoDWSIMdwsim")
        Me.SaveFileDialog1.AddExtension = True
        If Me.SaveFileDialog1.FilterIndex = 1 Then
            If Me.SaveFileDialog1.ShowDialog() = Windows.Forms.DialogResult.OK Then
                myStream = Me.SaveFileDialog1.OpenFile()
                Me.filename = myStream.Name
                myStream.Close()
                If Not (myStream Is Nothing) Then
                    Me.ToolStripStatusLabel1.Text = DWSIM.App.GetLocalString("Salvandosimulao") + " (" + Me.filename + ")"
                    Application.DoEvents()
                    Me.SaveF(Me.filename, Me.ActiveMdiChild)
                End If
            End If
        End If

    End Sub

    Private Sub OpenToolStripButton_Click(ByVal sender As System.Object, ByVal e As System.EventArgs) Handles OpenToolStripButton.Click, OpenToolStripMenuItem.Click

        Call Me.LoadFileDialog()

    End Sub

    Private Sub bgLoadFile_DoWork(ByVal sender As System.Object, ByVal e As System.ComponentModel.DoWorkEventArgs) Handles bgLoadFile.DoWork
        Dim bw As BackgroundWorker = CType(sender, BackgroundWorker)
        ' Start the time-consuming operation.
        Me.LoadF(Me.filename)
    End Sub

    Private Sub bgSaveFile_DoWork(ByVal sender As System.Object, ByVal e As System.ComponentModel.DoWorkEventArgs) Handles bgSaveFile.DoWork
        Dim bw As BackgroundWorker = CType(sender, BackgroundWorker)
        ' Start the time-consuming operation.
        Me.SaveF(Me.filename, Me.ActiveMdiChild)
    End Sub

    Private Sub bgLoadFile_RunWorkerCompleted(ByVal sender As Object, ByVal e As System.ComponentModel.RunWorkerCompletedEventArgs) Handles bgLoadFile.RunWorkerCompleted
        If Not (e.Error Is Nothing) Then
            ' There was an error during the operation.
            'FrmLoadSave.Hide()
            MessageBox.Show("Erro ao carregar arquivo: " & e.Error.Message, DWSIM.App.GetLocalString("Erro"), MessageBoxButtons.OK, MessageBoxIcon.Error)
            'Me.Close()
        Else
            'Me.FrmLoadSave.Close()
        End If
        Me.ToolStripStatusLabel1.Text = ""
    End Sub

    Private Sub bgSaveFile_RunWorkerCompleted(ByVal sender As Object, ByVal e As System.ComponentModel.RunWorkerCompletedEventArgs) Handles bgSaveFile.RunWorkerCompleted
        If Not (e.Error Is Nothing) Then
            ' There was an error during the operation.
            MessageBox.Show("Erro ao salvar arquivo: " & e.Error.Message, DWSIM.App.GetLocalString("Erro"), MessageBoxButtons.OK, MessageBoxIcon.Error)
        Else

            'lista dos mais recentes, modificar

            With My.Settings.MostRecentFiles
                If Not .Contains(Me.filename) Then
                    .Item(2) = .Item(1)
                    .Item(1) = .Item(0)
                    .Item(0) = Me.filename
                End If
            End With

            'processar lista de arquivos recentes


        End If
        Me.ToolStripStatusLabel1.Text = ""
    End Sub

#End Region

#Region "    Click Handlers"

    Private Sub LR1_LinkClicked(ByVal sender As System.Object, ByVal e As System.Windows.Forms.LinkLabelLinkClickedEventArgs)

        Dim myLink As LinkLabel = CType(sender, LinkLabel)

        If myLink.Text <> DWSIM.App.GetLocalString("vazio") Then
            Dim nome = myLink.Tag.ToString
            Me.filename = nome
            Me.ToolStripStatusLabel1.Text = DWSIM.App.GetLocalString("Abrindosimulao") + " (" + nome + ")"
            Application.DoEvents()
            Try
                Select Case Path.GetExtension(Me.filename).ToLower
                    Case ".dwsim"
                        Me.LoadF(Me.filename)
                    Case ".dwcsd"
                        Dim NewMDIChild As New FormCompoundCreator()
                        NewMDIChild.MdiParent = Me
                        NewMDIChild.Show()
                        Dim objStreamReader As New FileStream(Me.filename, FileMode.Open)
                        Dim x As New BinaryFormatter()
                        NewMDIChild.mycase = x.Deserialize(objStreamReader)
                        objStreamReader.Close()
                        NewMDIChild.WriteData()
                    Case ".dwrsd"
                        Dim NewMDIChild As New FormDataRegression()
                        NewMDIChild.MdiParent = Me
                        NewMDIChild.Show()
                        Dim objStreamReader As New FileStream(Me.filename, FileMode.Open)
                        Dim x As New BinaryFormatter()
                        NewMDIChild.currcase = x.Deserialize(objStreamReader)
                        objStreamReader.Close()
                        NewMDIChild.LoadCase(NewMDIChild.currcase, False)
                End Select
            Catch ex As Exception
                MessageBox.Show("Erro ao carregar arquivo: " & ex.Message, DWSIM.App.GetLocalString("Erro"), MessageBoxButtons.OK, MessageBoxIcon.Error)
            Finally
                Me.ToolStripStatusLabel1.Text = ""
            End Try
            'Me.bgLoadFile.RunWorkerAsync()
        End If
    End Sub

    Private Sub NewToolStripButton_Click(ByVal sender As System.Object, ByVal e As System.EventArgs) Handles NewToolStripButton.Click, NewToolStripMenuItem.Click

        Dim NewMDIChild As New FormFlowsheet()

        'Set the Parent Form of the Child window.
        NewMDIChild.MdiParent = Me
        'Display the new form.
        NewMDIChild.Text = "Simulation" & m_childcount
        NewMDIChild.Show()
        Application.DoEvents()
        Me.ActivateMdiChild(NewMDIChild)
        m_childcount += 1

    End Sub

    Private Sub CascadeToolStripMenuItem_Click(ByVal sender As Object, ByVal e As EventArgs) Handles CascadeToolStripMenuItem.Click
        Me.LayoutMdi(MdiLayout.Cascade)
        If Me.CascadeToolStripMenuItem.Checked = True Then
            Me.TileVerticalToolStripMenuItem.Checked = False
            Me.TileHorizontalToolStripMenuItem.Checked = False
        Else
            Me.TileHorizontalToolStripMenuItem.Checked = False
            Me.TileVerticalToolStripMenuItem.Checked = False
            Me.CascadeToolStripMenuItem.Checked = True
        End If
    End Sub

    Private Sub TileVerticleToolStripMenuItem_Click(ByVal sender As Object, ByVal e As EventArgs) Handles TileVerticalToolStripMenuItem.Click
        Me.LayoutMdi(MdiLayout.TileVertical)
        If Me.TileVerticalToolStripMenuItem.Checked = True Then
            Me.TileHorizontalToolStripMenuItem.Checked = False
            Me.CascadeToolStripMenuItem.Checked = False
        Else
            Me.TileHorizontalToolStripMenuItem.Checked = False
            Me.TileVerticalToolStripMenuItem.Checked = True
            Me.CascadeToolStripMenuItem.Checked = False
        End If
    End Sub

    Private Sub TileHorizontalToolStripMenuItem_Click(ByVal sender As Object, ByVal e As EventArgs) Handles TileHorizontalToolStripMenuItem.Click
        Me.LayoutMdi(MdiLayout.TileHorizontal)
        If Me.TileHorizontalToolStripMenuItem.Checked = True Then
            Me.TileVerticalToolStripMenuItem.Checked = False
            Me.CascadeToolStripMenuItem.Checked = False
        Else
            Me.TileHorizontalToolStripMenuItem.Checked = True
            Me.TileVerticalToolStripMenuItem.Checked = False
            Me.CascadeToolStripMenuItem.Checked = False
        End If
    End Sub

    Private Sub ExitToolStripMenuItem_Click(ByVal sender As System.Object, ByVal e As System.EventArgs) Handles ExitToolStripMenuItem.Click
        Me.Close()
    End Sub

    Private Sub AboutToolStripMenuItem_Click(ByVal sender As System.Object, ByVal e As System.EventArgs) Handles AboutToolStripMenuItem.Click
        If Not DWSIM.App.IsRunningOnMono Then
            Dim frmAbout As New AboutBoxNET
            frmAbout.ShowDialog(Me)
        Else
            Dim frmAbout As New AboutBoxMONO
            frmAbout.ShowDialog(Me)
        End If
    End Sub

    Private Sub LinkLabel7_LinkClicked_1(ByVal sender As System.Object, ByVal e As System.Windows.Forms.LinkLabelLinkClickedEventArgs)
        Call Me.OpenToolStripButton_Click(sender, e)
    End Sub

    Private Sub LinkLabel5_LinkClicked_1(ByVal sender As System.Object, ByVal e As System.Windows.Forms.LinkLabelLinkClickedEventArgs)
        Call Me.NewToolStripButton_Click(sender, e)
    End Sub

    Private Sub OpenRecent_click(ByVal sender As System.Object, ByVal e As System.EventArgs)

        Dim myLink As ToolStripMenuItem = CType(sender, ToolStripMenuItem)
        If myLink.Text <> DWSIM.App.GetLocalString("vazio") Then
            If File.Exists(myLink.Tag.ToString) Then
                Dim nome = myLink.Tag.ToString
                Me.ToolStripStatusLabel1.Text = DWSIM.App.GetLocalString("Abrindosimulao") + " (" + nome + ")"
                Me.filename = nome
                Application.DoEvents()
                Dim objStreamReader As FileStream = Nothing
                Try
                    Select Case Path.GetExtension(nome).ToLower()
                        Case ".dwxml"
                            Me.LoadXML(nome)
                        Case ".dwsim"
                            Me.LoadF(nome)
                        Case ".dwcsd"
                            Dim NewMDIChild As New FormCompoundCreator()
                            NewMDIChild.MdiParent = Me
                            NewMDIChild.Show()
                            objStreamReader = New FileStream(nome, FileMode.Open)
                            Dim x As New BinaryFormatter()
                            NewMDIChild.mycase = x.Deserialize(objStreamReader)
                            objStreamReader.Close()
                            NewMDIChild.WriteData()
                        Case ".dwrsd"
                            Dim NewMDIChild As New FormDataRegression()
                            NewMDIChild.MdiParent = Me
                            NewMDIChild.Show()
                            objStreamReader = New FileStream(nome, FileMode.Open)
                            Dim x As New BinaryFormatter()
                            NewMDIChild.currcase = x.Deserialize(objStreamReader)
                            objStreamReader.Close()
                            NewMDIChild.LoadCase(NewMDIChild.currcase, False)
                    End Select
                Catch ex As Exception
                    MessageBox.Show("Erro ao carregar arquivo: " & ex.Message, DWSIM.App.GetLocalString("Erro"), MessageBoxButtons.OK, MessageBoxIcon.Error)
                Finally
                    Me.ToolStripStatusLabel1.Text = ""
                    If objStreamReader IsNot Nothing Then objStreamReader.Close()
                End Try
            End If
        End If
    End Sub

    Private Sub SaveAllToolStripButton_Click(ByVal sender As System.Object, ByVal e As System.EventArgs) Handles SaveAllToolStripButton.Click, SaveAllToolStripMenuItem.Click
        If Me.MdiChildren.Length > 0 Then
            Dim result As MsgBoxResult = MessageBox.Show(DWSIM.App.GetLocalString("Istoirsalvartodasass"), DWSIM.App.GetLocalString("Ateno2"), MessageBoxButtons.YesNo, MessageBoxIcon.Question)
            If result = MsgBoxResult.Yes Then
                For Each form0 As Form In Me.MdiChildren
                    If TypeOf form0 Is FormFlowsheet Then
                        Dim form2 As FormFlowsheet = form0
                        If form2.Options.FilePath <> "" Then
                            Me.ToolStripStatusLabel1.Text = DWSIM.App.GetLocalString("Salvandosimulao") + " (" + Me.filename + ")"
                            Try
                                If Path.GetExtension(form2.Options.FilePath).ToLower = ".dwsim" Then
                                    SaveF(form2.Options.FilePath, form2)
                                Else
                                    SaveXML(form2.Options.FilePath, form2)
                                End If
                            Catch ex As Exception
                                MessageBox.Show(ex.Message, DWSIM.App.GetLocalString("Erro"), MessageBoxButtons.OK, MessageBoxIcon.Error)
                            Finally
                                Me.ToolStripStatusLabel1.Text = ""
                            End Try
                        Else
                            Dim myStream As System.IO.FileStream
                            If Me.SaveFileDialog1.ShowDialog() = Windows.Forms.DialogResult.OK Then
                                myStream = Me.SaveFileDialog1.OpenFile()
                                myStream.Close()
                                If Not (myStream Is Nothing) Then
                                    Me.ToolStripStatusLabel1.Text = DWSIM.App.GetLocalString("Salvandosimulao") + " (" + Me.filename + ")"
                                    Try
                                        If Path.GetExtension(myStream.Name).ToLower = ".dwsim" Then
                                            SaveF(myStream.Name, form2)
                                        Else
                                            SaveXML(myStream.Name, form2)
                                        End If
                                    Catch ex As Exception
                                        MessageBox.Show(ex.Message, DWSIM.App.GetLocalString("Erro"), MessageBoxButtons.OK, MessageBoxIcon.Error)
                                    Finally
                                        Me.ToolStripStatusLabel1.Text = ""
                                    End Try
                                End If
                            End If
                        End If
                    ElseIf TypeOf form0 Is FormCompoundCreator Then
                        Dim filename As String = CType(Me.ActiveMdiChild, FormCompoundCreator).mycase.Filename
                        If filename = "" Then
                            If Me.SaveStudyDlg.ShowDialog(Me) = Windows.Forms.DialogResult.OK Then
                                CType(Me.ActiveMdiChild, FormCompoundCreator).mycase.Filename = Me.SaveStudyDlg.FileName
                                CType(Me.ActiveMdiChild, FormCompoundCreator).StoreData()
                                Dim objStreamWriter As New FileStream(Me.SaveStudyDlg.FileName, FileMode.OpenOrCreate)
                                Dim x As New BinaryFormatter
                                x.Serialize(objStreamWriter, CType(Me.ActiveMdiChild, FormCompoundCreator).mycase)
                                objStreamWriter.Close()
                                CType(Me.ActiveMdiChild, FormCompoundCreator).SetCompCreatorSaveStatus(True)
                            End If
                        Else
                            CType(Me.ActiveMdiChild, FormCompoundCreator).StoreData()
                            Dim objStreamWriter As New FileStream(filename, FileMode.OpenOrCreate)
                            Dim x As New BinaryFormatter
                            x.Serialize(objStreamWriter, CType(Me.ActiveMdiChild, FormCompoundCreator).mycase)
                            objStreamWriter.Close()
                            CType(Me.ActiveMdiChild, FormCompoundCreator).SetCompCreatorSaveStatus(True)
                        End If
                    ElseIf TypeOf form0 Is FormDataRegression Then
                        If Me.SaveRegStudyDlg.ShowDialog(Me) = Windows.Forms.DialogResult.OK Then
                            Dim objStreamWriter As New FileStream(Me.SaveRegStudyDlg.FileName, FileMode.OpenOrCreate)
                            Dim x As New BinaryFormatter
                            x.Serialize(objStreamWriter, CType(form0, FormDataRegression).StoreCase())
                            objStreamWriter.Close()
                        End If
                    End If
                Next
            End If
        Else
            MessageBox.Show(DWSIM.App.GetLocalString("Noexistemsimulaesase"), DWSIM.App.GetLocalString("Informao"), MessageBoxButtons.OK, MessageBoxIcon.Information)
        End If
    End Sub

    Public Sub SaveToolStripButton_Click(ByVal sender As System.Object, ByVal e As System.EventArgs) Handles SaveToolStripButton.Click, SaveToolStripMenuItem.Click

        If Not Me.ActiveMdiChild Is Nothing Then
            If TypeOf Me.ActiveMdiChild Is FormFlowsheet Then
                Dim form2 As FormFlowsheet = Me.ActiveMdiChild
                If form2.Options.FilePath <> "" Then
                    Me.ToolStripStatusLabel1.Text = DWSIM.App.GetLocalString("Salvandosimulao") + " (" + Me.filename + ")"
                    Application.DoEvents()
                    Try
                        Me.filename = form2.Options.FilePath
                        If Path.GetExtension(Me.filename).ToLower = ".dwsim" Then
                            SaveF(form2.Options.FilePath, form2)
                        Else
                            SaveXML(form2.Options.FilePath, form2)
                        End If
                    Catch ex As Exception
                        MessageBox.Show(ex.Message, DWSIM.App.GetLocalString("Erro"), MessageBoxButtons.OK, MessageBoxIcon.Error)
                    Finally
                        Me.ToolStripStatusLabel1.Text = ""
                    End Try
                Else
                    Dim myStream As System.IO.FileStream
                    If Me.SaveFileDialog1.ShowDialog() = Windows.Forms.DialogResult.OK Then
                        myStream = Me.SaveFileDialog1.OpenFile()
                        Me.filename = myStream.Name
                        myStream.Close()
                        If Not (myStream Is Nothing) Then
                            Me.ToolStripStatusLabel1.Text = DWSIM.App.GetLocalString("Salvandosimulao") + " (" + Me.filename + ")"
                            Application.DoEvents()
                            Try
                                If Path.GetExtension(Me.filename).ToLower = ".dwsim" Then
                                    SaveF(myStream.Name, form2)
                                Else
                                    SaveXML(myStream.Name, form2)
                                End If
                            Catch ex As Exception
                                MessageBox.Show(ex.Message, DWSIM.App.GetLocalString("Erro"), MessageBoxButtons.OK, MessageBoxIcon.Error)
                            Finally
                                Me.ToolStripStatusLabel1.Text = ""
                            End Try
                        End If
                    End If
                End If
            ElseIf TypeOf Me.ActiveMdiChild Is FormCompoundCreator Then
                Dim filename As String = CType(Me.ActiveMdiChild, FormCompoundCreator).mycase.Filename
                If filename = "" Then
                    If Me.SaveStudyDlg.ShowDialog(Me) = Windows.Forms.DialogResult.OK Then
                        CType(Me.ActiveMdiChild, FormCompoundCreator).mycase.Filename = Me.SaveStudyDlg.FileName
                        CType(Me.ActiveMdiChild, FormCompoundCreator).StoreData()
                        Dim objStreamWriter As New FileStream(Me.SaveStudyDlg.FileName, FileMode.OpenOrCreate)
                        Dim x As New BinaryFormatter
                        x.Serialize(objStreamWriter, CType(Me.ActiveMdiChild, FormCompoundCreator).mycase)
                        objStreamWriter.Close()
                        CType(Me.ActiveMdiChild, FormCompoundCreator).SetCompCreatorSaveStatus(True)
                        Me.filename = Me.SaveStudyDlg.FileName
                        Me.ActiveMdiChild.Text = Me.filename
                    End If
                Else
                    CType(Me.ActiveMdiChild, FormCompoundCreator).StoreData()
                    Dim objStreamWriter As New FileStream(filename, FileMode.OpenOrCreate)
                    Dim x As New BinaryFormatter
                    x.Serialize(objStreamWriter, CType(Me.ActiveMdiChild, FormCompoundCreator).mycase)
                    objStreamWriter.Close()
                    CType(Me.ActiveMdiChild, FormCompoundCreator).SetCompCreatorSaveStatus(True)
                    Me.filename = filename
                    Me.ActiveMdiChild.Text = filename
                End If
            ElseIf TypeOf Me.ActiveMdiChild Is FormDataRegression Then
                If Me.SaveRegStudyDlg.ShowDialog(Me) = Windows.Forms.DialogResult.OK Then
                    Dim objStreamWriter As New FileStream(Me.SaveRegStudyDlg.FileName, FileMode.OpenOrCreate)
                    Dim x As New BinaryFormatter
                    x.Serialize(objStreamWriter, CType(Me.ActiveMdiChild, FormDataRegression).StoreCase())
                    objStreamWriter.Close()
                    Me.filename = Me.SaveRegStudyDlg.FileName
                    Me.ActiveMdiChild.Text = Me.filename
                End If
            End If
        Else
            MessageBox.Show(DWSIM.App.GetLocalString("Noexistemsimulaesati"), DWSIM.App.GetLocalString("Erro"), MessageBoxButtons.OK, MessageBoxIcon.Error)
        End If

    End Sub

    Private Sub ToolStripButton1_Click(ByVal sender As System.Object, ByVal e As System.EventArgs) Handles ToolStripButton1.Click, SaveAsToolStripMenuItem.Click
        Call Me.SaveFileDialog()
    End Sub

    Private Sub FecharTodasAsSimulaçõesAbertasToolStripMenuItem_Click(ByVal sender As System.Object, ByVal e As System.EventArgs) Handles CloseAllToolstripMenuItem.Click
        If Me.MdiChildren.Length > 0 Then
            Dim form2 As Form
            For Each form2 In Me.MdiChildren
                Application.DoEvents()
                Try
                    form2.Close()
                Catch ex As Exception
                    Console.WriteLine(ex.ToString)
                End Try
                Application.DoEvents()
            Next
        Else
            MessageBox.Show(DWSIM.App.GetLocalString("Noexistemsimulaesase"), DWSIM.App.GetLocalString("Informao"), MessageBoxButtons.OK, MessageBoxIcon.Information)
        End If
    End Sub

    Private Sub BlogDeDesenvolvimentoToolStripMenuItem_Click(ByVal sender As System.Object, ByVal e As System.EventArgs) Handles BlogDeDesenvolvimentoToolStripMenuItem.Click
        System.Diagnostics.Process.Start("http://dwsim.inforside.com.br/blog")
    End Sub

    Private Sub DownloadsToolStripMenuItem_Click_1(ByVal sender As System.Object, ByVal e As System.EventArgs) Handles DownloadsToolStripMenuItem.Click
        System.Diagnostics.Process.Start("http://sourceforge.net/projects/dwsim/files/")
    End Sub

    Private Sub WikiToolStripMenuItem_Click(ByVal sender As System.Object, ByVal e As System.EventArgs) Handles WikiToolStripMenuItem.Click
        System.Diagnostics.Process.Start("http://dwsim.inforside.com.br")
    End Sub

    Private Sub FórumToolStripMenuItem_Click(ByVal sender As System.Object, ByVal e As System.EventArgs) Handles FórumToolStripMenuItem.Click
        System.Diagnostics.Process.Start("http://sourceforge.net/p/dwsim/discussion/")
    End Sub

    Private Sub RastreamentoDeBugsToolStripMenuItem_Click(ByVal sender As System.Object, ByVal e As System.EventArgs) Handles RastreamentoDeBugsToolStripMenuItem.Click
        System.Diagnostics.Process.Start("http://sourceforge.net/apps/mantisbt/dwsim/")
    End Sub

    Private Sub DonateToolStripMenuItem_Click(ByVal sender As System.Object, ByVal e As System.EventArgs) Handles DonateToolStripMenuItem.Click
        System.Diagnostics.Process.Start("http://sourceforge.net/p/dwsim/donate/")
    End Sub

    Private Sub MostrarBarraDeFerramentasToolStripMenuItem_Click(ByVal sender As System.Object, ByVal e As System.EventArgs) Handles MostrarBarraDeFerramentasToolStripMenuItem.Click
        If Me.MostrarBarraDeFerramentasToolStripMenuItem.Checked Then
            Me.ToolStrip1.Visible = True
        Else
            Me.ToolStrip1.Visible = False
        End If
    End Sub

    Private Sub ToolStripButton2_Click(ByVal sender As System.Object, ByVal e As System.EventArgs) Handles ToolStripButton2.Click
        Me.PreferênciasDoDWSIMToolStripMenuItem_Click(sender, e)
    End Sub

    Private Sub ToolStripButton3_Click(ByVal sender As System.Object, ByVal e As System.EventArgs) Handles ToolStripButton3.Click
        Me.CascadeToolStripMenuItem_Click(sender, e)
    End Sub

    Private Sub ToolStripButton5_Click(ByVal sender As System.Object, ByVal e As System.EventArgs) Handles ToolStripButton5.Click
        Me.TileVerticleToolStripMenuItem_Click(sender, e)
    End Sub

    Private Sub ToolStripButton4_Click(ByVal sender As System.Object, ByVal e As System.EventArgs) Handles ToolStripButton4.Click
        Me.TileHorizontalToolStripMenuItem_Click(sender, e)
    End Sub

    Private Sub ToolStripButton7_Click(ByVal sender As System.Object, ByVal e As System.EventArgs) Handles ToolStripButton7.Click
        Me.DonateToolStripMenuItem_Click(sender, e)
    End Sub

    Private Sub ToolStripButton8_Click(ByVal sender As System.Object, ByVal e As System.EventArgs) Handles ToolStripButton8.Click
        Me.AboutToolStripMenuItem_Click(sender, e)
    End Sub

    Private Sub ContentsToolStripMenuItem_Click(ByVal sender As System.Object, ByVal e As System.EventArgs) Handles ContentsToolStripMenuItem.Click
        My.Computer.Keyboard.SendKeys("{F1}", True)
    End Sub

    Private Sub RegistrarTiposCOMToolStripMenuItem_Click(ByVal sender As System.Object, ByVal e As System.EventArgs) Handles RegistrarTiposCOMToolStripMenuItem.Click

        Dim windir As String = Environment.GetEnvironmentVariable("SystemRoot")
        Process.Start(windir & "\Microsoft.NET\Framework\v4.0.30319\RegAsm.exe", "/codebase /silent " & Chr(34) & My.Application.Info.DirectoryPath & " \DWSIM.exe" & Chr(34))

    End Sub

    Private Sub DeToolStripMenuItem_Click(ByVal sender As System.Object, ByVal e As System.EventArgs) Handles DeToolStripMenuItem.Click

        Dim windir As String = Environment.GetEnvironmentVariable("SystemRoot")
        Process.Start(windir & "\Microsoft.NET\Framework\v4.0.30319\RegAsm.exe", "/u " & Chr(34) & My.Application.Info.DirectoryPath & " \DWSIM.exe" & Chr(34))

    End Sub

    Private Sub tslupd_Click(ByVal sender As Object, ByVal e As System.EventArgs) Handles tslupd.Click
        Dim myfile As String = My.Computer.FileSystem.SpecialDirectories.Temp & Path.DirectorySeparatorChar & "DWSIM" & Path.DirectorySeparatorChar & "dwsim.txt"
        Dim txt() As String = File.ReadAllLines(myfile)
        Dim build As Integer, bdate As Date, fname As String, dlpath As String, changelog As String = ""
        build = txt(0)
        bdate = Date.Parse(txt(1), New CultureInfo("en-US"))
        dlpath = txt(2)
        fname = txt(3)
        For i As Integer = 4 To txt.Length - 1
            changelog += txt(i) + vbCrLf
        Next
        Dim strb As New StringBuilder()
        With strb
            .AppendLine(DWSIM.App.GetLocalString("BuildNumber") & ": " & build & vbCrLf)
            .AppendLine(DWSIM.App.GetLocalString("BuildDate") & ": " & bdate.ToString(My.Application.Culture.DateTimeFormat.ShortDatePattern, My.Application.Culture) & vbCrLf)
            .AppendLine(DWSIM.App.GetLocalString("Changes") & ": " & vbCrLf & changelog & vbCrLf)
            .AppendLine(DWSIM.App.GetLocalString("DownloadQuestion"))
        End With
        Dim msgresult As MsgBoxResult = MessageBox.Show(strb.ToString, DWSIM.App.GetLocalString("NewVersionAvailable"), MessageBoxButtons.YesNo, MessageBoxIcon.Information)
        If msgresult = MsgBoxResult.Yes Then
            'Me.sfdUpdater.FileName = fname
            'If Me.sfdUpdater.ShowDialog(Me) = Windows.Forms.DialogResult.OK Then
            'Try
            'My.Computer.Network.DownloadFile(dlpath, Me.sfdUpdater.FileName, "", "", True, 100000, True, FileIO.UICancelOption.DoNothing)
            Process.Start(dlpath)
            tslupd.Visible = False
            'Catch ex As Exception
            'MessageBox.Show(ex.ToString, "Error", MessageBoxButtons.OK, MessageBoxIcon.Error)
            'End Try
            'End If
        End If
    End Sub

    Private Sub NovoEstudoDoCriadorDeComponentesToolStripMenuItem_Click(ByVal sender As System.Object, ByVal e As System.EventArgs) Handles NovoEstudoDoCriadorDeComponentesToolStripMenuItem.Click
        Dim NewMDIChild As New FormCompoundCreator()
        'Set the Parent Form of the Child window.
        NewMDIChild.MdiParent = Me
        'Display the new form.
        NewMDIChild.Text = "CompCreator" & m_childcount
        Me.ActivateMdiChild(NewMDIChild)
        NewMDIChild.Show()
        m_childcount += 1
    End Sub

    Private Sub NovoEstudoDeRegressãoDeDadosToolStripMenuItem_Click(ByVal sender As System.Object, ByVal e As System.EventArgs) Handles NovoEstudoDeRegressãoDeDadosToolStripMenuItem.Click
        Dim NewMDIChild As New FormDataRegression()
        'Set the Parent Form of the Child window.
        NewMDIChild.MdiParent = Me
        'Display the new form.
        NewMDIChild.Text = "DataRegression" & m_childcount
        Me.ActivateMdiChild(NewMDIChild)
        NewMDIChild.Show()
        m_childcount += 1
    End Sub

    Private Sub PreferênciasDoDWSIMToolStripMenuItem_Click(ByVal sender As System.Object, ByVal e As System.EventArgs) Handles PreferênciasDoDWSIMToolStripMenuItem.Click
        Me.FrmOptions = New FormOptions
        Me.FrmOptions.ShowDialog(Me)
    End Sub

#End Region

#Region "    Backup/Update"

    Private Sub TimerBackup_Tick(ByVal sender As Object, ByVal e As System.EventArgs) Handles TimerBackup.Tick

        Dim folder As String = My.Settings.BackupFolder
        If Not Directory.Exists(folder) And folder <> "" Then
            Try
                Directory.CreateDirectory(folder)
            Catch ex As Exception
                MessageBox.Show(DWSIM.App.GetLocalString("Erroaocriardiretriop") & vbCrLf & DWSIM.App.GetLocalString("Verifiquesevoctemonv"), _
                             DWSIM.App.GetLocalString("Cpiasdesegurana"), MessageBoxButtons.OK, MessageBoxIcon.Error)
            End Try
        End If

        If Directory.Exists(folder) Then
            If Not Me.bgSaveBackup.IsBusy Then Me.bgSaveBackup.RunWorkerAsync()
        End If

    End Sub

    Private Sub bgSaveBackup_DoWork(ByVal sender As Object, ByVal e As System.ComponentModel.DoWorkEventArgs) Handles bgSaveBackup.DoWork
        Dim bw As BackgroundWorker = CType(sender, BackgroundWorker)
        ' Start the time-consuming operation.
        Dim folder As String = My.Settings.BackupFolder
        Dim path As String = ""
        For Each form0 As Form In Me.MdiChildren
            If TypeOf form0 Is FormFlowsheet Then
                path = folder + "\" + CType(form0, FormFlowsheet).Options.BackupFileName
                Me.SaveF(path, form0)
                If Not My.Settings.BackupFiles.Contains(path) Then
                    My.Settings.BackupFiles.Add(path)
                    My.Settings.Save()
                End If
            End If
        Next
    End Sub

    Private Sub bgSaveBackup_RunWorkerCompleted(ByVal sender As Object, ByVal e As System.ComponentModel.RunWorkerCompletedEventArgs) Handles bgSaveBackup.RunWorkerCompleted
        If Not (e.Error Is Nothing) Then
            ' There was an error during the operation.
            MessageBox.Show("Erro ao salvar cópias de segurança: " & e.Error.Message & vbCrLf & DWSIM.App.GetLocalString("Paranovermaisesseavi"), DWSIM.App.GetLocalString("Erro"), MessageBoxButtons.OK, MessageBoxIcon.Error)
            My.Application.Log.WriteException(e.Error)
        End If
    End Sub

    Private Sub bgUpdater_DoWork(ByVal sender As Object, ByVal e As System.ComponentModel.DoWorkEventArgs) Handles bgUpdater.DoWork

        Dim myfile As String = My.Computer.FileSystem.SpecialDirectories.Temp & Path.DirectorySeparatorChar & "DWSIM" & Path.DirectorySeparatorChar & "dwsim.txt"
        Dim webp As New System.Net.WebProxy
        webp.UseDefaultCredentials = True
        Dim webcl As New System.Net.WebClient()
        webcl.Proxy = webp
        webcl.DownloadFile("http://dwsim.inforside.com.br/dwsim.txt", myfile)
        dlok = True

    End Sub

    Private Sub bgUpdater_RunWorkerCompleted(ByVal sender As Object, ByVal e As System.ComponentModel.RunWorkerCompletedEventArgs) Handles bgUpdater.RunWorkerCompleted
        If dlok Then
            Dim myfile As String = My.Computer.FileSystem.SpecialDirectories.Temp & Path.DirectorySeparatorChar & "DWSIM" & Path.DirectorySeparatorChar & "dwsim.txt"
            If File.Exists(myfile) Then
                Dim txt() As String = File.ReadAllLines(myfile)
                Dim build As Integer
                build = txt(0)
                If My.Application.Info.Version.Build < CInt(build) Then
                    tslupd.Visible = True
                    tslupd.Text = DWSIM.App.GetLocalString("NewVersionAvailable")
                End If
            End If
        End If
    End Sub

#End Region

End Class
