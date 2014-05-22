'    Copyright 2014 Daniel Wagner O. de Medeiros
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

Imports OutlookStyleControls
Imports DWSIM.DWSIM.ClassesBasicasTermodinamica
Imports System.IO
Imports DWSIM.DWSIM.Outros
Imports DWSIM.DWSIM.Flowsheet.FlowsheetSolver

Public Class FormConfigWizard

    Private FrmChild As FormFlowsheet
    Dim loaded As Boolean = False

    Private prevsort As System.ComponentModel.ListSortDirection = System.ComponentModel.ListSortDirection.Ascending
    Private prevcol As Integer = 1
    Private prevgroup As IOutlookGridGroup

    Public switch As Boolean = False

    Dim ACSC1 As AutoCompleteStringCollection

    Private Sub FormConfigWizard_Load(sender As Object, e As System.EventArgs) Handles Me.Load

        If DWSIM.App.IsRunningOnMono Then
            Me.ListViewPP.View = View.List
        Else
            Me.ListViewPP.View = View.Tile
        End If

        Me.StepWizardControl1.FinishButtonText = DWSIM.App.GetLocalString("FinishText")
        Me.StepWizardControl1.CancelButtonText = DWSIM.App.GetLocalString("CancelText")
        Me.StepWizardControl1.NextButtonText = DWSIM.App.GetLocalString("NextText") & " >"

        Init()

    End Sub

    Sub Init(Optional ByVal reset As Boolean = False)

        Dim pathsep As Char = Path.DirectorySeparatorChar

        FrmChild = My.Application.ActiveSimulation

        Dim comp As DWSIM.ClassesBasicasTermodinamica.ConstantProperties
        If Not loaded Or reset Then

            ACSC1 = New AutoCompleteStringCollection

            Me.ListViewA.Items.Clear()
            For Each comp In Me.FrmChild.Options.SelectedComponents.Values
                Me.ListViewA.Items.Add(comp.Name, DWSIM.App.GetComponentName(comp.Name), 0).Tag = comp.Name
            Next
            For Each comp In Me.FrmChild.Options.NotSelectedComponents.Values
                Dim idx As Integer = Me.AddCompToGrid(comp)
                If Not idx = -1 Then
                    For Each c As DataGridViewCell In Me.ogc1.Rows(idx).Cells
                        If comp.OriginalDB <> "Electrolytes" Then
                            If comp.Acentric_Factor = 0.0# Or comp.Critical_Compressibility = 0.0# Then
                                c.Style.ForeColor = Color.Red
                                c.ToolTipText = DWSIM.App.GetLocalString("CompMissingData")
                            End If
                        End If
                    Next
                End If
            Next

            'property packages
            Me.ListViewPP.Items.Clear()
            For Each pp2 As DWSIM.SimulationObjects.PropertyPackages.PropertyPackage In FormMain.PropertyPackages.Values
                Select Case pp2.PackageType
                    Case DWSIM.SimulationObjects.PropertyPackages.PackageType.EOS
                        With Me.ListViewPP.Items.Add(pp2.ComponentName)
                            .Group = Me.ListViewPP.Groups("EOS")
                            .ToolTipText = pp2.ComponentDescription
                        End With
                    Case DWSIM.SimulationObjects.PropertyPackages.PackageType.ActivityCoefficient
                        With Me.ListViewPP.Items.Add(pp2.ComponentName)
                            .Group = Me.ListViewPP.Groups("ACT")
                            .ToolTipText = pp2.ComponentDescription
                        End With
                    Case DWSIM.SimulationObjects.PropertyPackages.PackageType.ChaoSeader
                        With Me.ListViewPP.Items.Add(pp2.ComponentName)
                            .Group = Me.ListViewPP.Groups("CS")
                            .ToolTipText = pp2.ComponentDescription
                        End With
                    Case DWSIM.SimulationObjects.PropertyPackages.PackageType.VaporPressure
                        With Me.ListViewPP.Items.Add(pp2.ComponentName)
                            .Group = Me.ListViewPP.Groups("VAP")
                            .ToolTipText = pp2.ComponentDescription
                        End With
                    Case DWSIM.SimulationObjects.PropertyPackages.PackageType.Miscelaneous
                        With Me.ListViewPP.Items.Add(pp2.ComponentName)
                            .Group = Me.ListViewPP.Groups("MISC")
                            .ToolTipText = pp2.ComponentDescription
                        End With
                    Case DWSIM.SimulationObjects.PropertyPackages.PackageType.CorrespondingStates
                        With Me.ListViewPP.Items.Add(pp2.ComponentName)
                            .Group = Me.ListViewPP.Groups("CST")
                            .ToolTipText = pp2.ComponentDescription
                        End With
                    Case DWSIM.SimulationObjects.PropertyPackages.PackageType.CAPEOPEN
                        With Me.ListViewPP.Items.Add(pp2.ComponentName)
                            .Group = Me.ListViewPP.Groups("CAP")
                            .ToolTipText = pp2.ComponentDescription
                        End With
                End Select
            Next

        Else

            For Each r As DataGridViewRow In ogc1.Rows
                If FrmChild.Options.NotSelectedComponents.ContainsKey(r.Cells(0).Value) Then
                    comp = FrmChild.Options.NotSelectedComponents(r.Cells(0).Value)
                    For Each c As DataGridViewCell In r.Cells
                        If comp.Acentric_Factor = 0.0# Or comp.Critical_Compressibility = 0.0# Then
                            c.Style.ForeColor = Color.Red
                            c.ToolTipText = DWSIM.App.GetLocalString("CompMissingData")
                        End If
                    Next
                End If
            Next

        End If

        With Me.dgvpp.Rows
            .Clear()
            For Each pp2 As DWSIM.SimulationObjects.PropertyPackages.PropertyPackage In FrmChild.Options.PropertyPackages.Values
                .Add(New Object() {pp2.UniqueID, pp2.Tag, pp2.ComponentName})
            Next
        End With

        Dim array1(FormMain.AvailableUnitSystems.Count - 1) As String
        FormMain.AvailableUnitSystems.Keys.CopyTo(array1, 0)
        Me.ComboBox2.Items.Clear()
        Me.ComboBox2.Items.AddRange(array1)
        FrmChild.ToolStripComboBoxUnitSystem.Items.Clear()
        FrmChild.ToolStripComboBoxUnitSystem.Items.AddRange(array1)

        FrmChild.ToolStripComboBoxNumberFormatting.SelectedItem = Me.FrmChild.Options.NumberFormat
        FrmChild.ToolStripComboBoxNumberFractionFormatting.SelectedItem = Me.FrmChild.Options.FractionNumberFormat

        ComboBox2.SelectedIndex = 0
        FrmChild.ToolStripComboBoxUnitSystem.SelectedIndex = 0

        ListBoxPP.SelectedIndex = 0

        Me.loaded = True

    End Sub

    Public Function AddCompToGrid(ByRef comp As DWSIM.ClassesBasicasTermodinamica.ConstantProperties) As Integer

        'If Not initialized Then
        '    Me.Visible = False
        '    Me.Show()
        '    Me.Visible = False
        'End If

        Dim contains As Boolean = False
        Dim index As Integer = -1
        For Each r As OutlookGridRow In ogc1.Rows
            If r.Cells(0).Value = comp.Name Then
                contains = True
                index = r.Index
            End If
        Next

        Dim translatedname As String = ""

        If Not contains Then
            Try
                Dim r As New OutlookGridRow
                translatedname = DWSIM.App.GetComponentName(comp.Name)
                r.CreateCells(ogc1, New Object() {comp.Name, translatedname, comp.CAS_Number, DWSIM.App.GetComponentType(comp), comp.Formula, comp.OriginalDB})
                ogc1.Rows.Add(r)
                Return ogc1.Rows.Count - 1
            Catch ex As Exception
                Console.WriteLine(ex.ToString)
                Return -1
            Finally
                ACSC1.Add(translatedname)
                ACSC1.Add(comp.CAS_Number)
                ACSC1.Add(comp.Formula)
                Me.TextBox1.AutoCompleteCustomSource = ACSC1
            End Try
        Else
            Return index
        End If

    End Function

    Private Sub LinkLabelPropertyMethods_LinkClicked(sender As System.Object, e As System.Windows.Forms.LinkLabelLinkClickedEventArgs) Handles LinkLabelPropertyMethods.LinkClicked
        Process.Start("http://dwsim.inforside.com.br/wiki/index.php?title=Property_Methods_and_Correlation_Profiles")
    End Sub

    Private Sub LinkLabel1_LinkClicked(sender As System.Object, e As System.Windows.Forms.LinkLabelLinkClickedEventArgs) Handles LinkLabel1.LinkClicked
        Process.Start("http://dwsim.inforside.com.br/wiki/index.php?title=Excel_Add-In_for_Thermodynamic_Calculations#Flash_Algorithms_and_Results_Validation")
    End Sub

    Private Sub TextBox1_KeyDown(sender As Object, e As System.Windows.Forms.KeyEventArgs) Handles TextBox1.KeyDown
        If e.KeyCode = Keys.Enter Then
            Call Button7_Click(sender, e)
            Me.TextBox1.Text = ""
        End If
    End Sub

    Private Sub TextBox1_TextChanged(ByVal sender As System.Object, ByVal e As System.EventArgs) Handles TextBox1.TextChanged
        For Each r As DataGridViewRow In ogc1.Rows
            If Not r.Cells(1).Value Is Nothing Then
                If r.Cells(1).Value.ToString.Contains(Me.TextBox1.Text) Or
                   r.Cells(2).Value.ToString.Contains(Me.TextBox1.Text) Or
                   r.Cells(4).Value.ToString.Contains(Me.TextBox1.Text) Then
                    r.Visible = True
                    If r.Cells(1).Value.ToString.Equals(Me.TextBox1.Text) Or
                                       r.Cells(2).Value.ToString.Equals(Me.TextBox1.Text) Or
                                       r.Cells(4).Value.ToString.Equals(Me.TextBox1.Text) Then
                        r.Selected = True
                    End If
                Else
                    r.Selected = False
                    r.Visible = False
                End If
            End If
        Next
        If TextBox1.Text = "" Then
            ogc1.FirstDisplayedScrollingRowIndex = 0
            For Each r As DataGridViewRow In ogc1.Rows
                r.Selected = False
                r.Visible = True
            Next
        End If
    End Sub

    Private Sub Button7_Click(sender As System.Object, e As System.EventArgs) Handles Button7.Click
        If Me.ogc1.SelectedRows.Count > 0 Then
            Me.AddCompToSimulation(Me.ogc1.SelectedRows(0).Index)
        End If
    End Sub

    Sub AddCompToSimulation(ByVal index As Integer)
        ' TODO Add code to check that index is within range. If it is out of range, don't do anything.
        If Me.loaded Then
            If Not Me.FrmChild.Options.SelectedComponents.ContainsKey(ogc1.Rows(index).Cells(0).Value) Then
                Dim tmpcomp As New DWSIM.ClassesBasicasTermodinamica.ConstantProperties
                tmpcomp = Me.FrmChild.Options.NotSelectedComponents(ogc1.Rows(index).Cells(0).Value)
                Me.FrmChild.Options.SelectedComponents.Add(tmpcomp.Name, tmpcomp)
                Me.FrmChild.Options.NotSelectedComponents.Remove(tmpcomp.Name)
                Dim ms As DWSIM.SimulationObjects.Streams.MaterialStream

                Dim proplist As New ArrayList
                For Each ms In FrmChild.Collections.CLCS_MaterialStreamCollection.Values
                    For Each phase As DWSIM.ClassesBasicasTermodinamica.Fase In ms.Fases.Values
                        phase.Componentes.Add(tmpcomp.Name, New DWSIM.ClassesBasicasTermodinamica.Substancia(tmpcomp.Name, ""))
                        phase.Componentes(tmpcomp.Name).ConstantProperties = tmpcomp
                    Next

                    proplist.Clear()
                    For Each pi As DWSIM.Outros.NodeItem In ms.NodeTableItems.Values
                        If pi.Checked Then
                            proplist.Add(pi.Text)
                        End If
                    Next
                    ms.FillNodeItems()
                    For Each pi As DWSIM.Outros.NodeItem In ms.NodeTableItems.Values
                        If proplist.Contains(pi.Text) Then
                            pi.Checked = True
                        End If
                    Next
                Next

                Me.ListViewA.Items.Add(tmpcomp.Name, DWSIM.App.GetComponentName(tmpcomp.Name), 0).Tag = tmpcomp.Name
                Me.ogc1.Rows.RemoveAt(index)
            End If
        End If
        UpdateKeyCompounds()
    End Sub

    Private Sub Button10_Click(sender As System.Object, e As System.EventArgs) Handles Button10.Click
        If Me.ListViewA.SelectedItems.Count > 0 Then
            For Each lvi As ListViewItem In Me.ListViewA.SelectedItems
                Me.RemoveCompFromSimulation(lvi.Tag)
            Next
        End If
    End Sub

    Sub RemoveCompFromSimulation(ByVal compid As String)

        Dim tmpcomp As New DWSIM.ClassesBasicasTermodinamica.ConstantProperties
        Dim nm As String = compid
        tmpcomp = Me.FrmChild.Options.SelectedComponents(nm)
        Me.FrmChild.Options.SelectedComponents.Remove(tmpcomp.Name)
        Me.ListViewA.Items.RemoveByKey(tmpcomp.Name)
        Me.FrmChild.Options.NotSelectedComponents.Add(tmpcomp.Name, tmpcomp)
        Me.AddCompToGrid(tmpcomp)
        Dim ms As DWSIM.SimulationObjects.Streams.MaterialStream
        Dim proplist As New ArrayList

        For Each ms In FrmChild.Collections.CLCS_MaterialStreamCollection.Values
            For Each phase As DWSIM.ClassesBasicasTermodinamica.Fase In ms.Fases.Values
                phase.Componentes.Remove(tmpcomp.Name)
            Next

            proplist.Clear()
            For Each pi As DWSIM.Outros.NodeItem In ms.NodeTableItems.Values
                If pi.Checked Then
                    proplist.Add(pi.Text)
                End If
            Next
            ms.FillNodeItems()
            For Each pi As DWSIM.Outros.NodeItem In ms.NodeTableItems.Values
                If proplist.Contains(pi.Text) Then
                    pi.Checked = True
                End If
            Next
        Next
        UpdateKeyCompounds()
    End Sub

    Private Sub Button11_Click(sender As System.Object, e As System.EventArgs) Handles Button11.Click
        For Each lvi As ListViewItem In Me.ListViewA.Items
            Me.RemoveCompFromSimulation(lvi.Tag)
        Next
    End Sub

    Private Sub Button6_Click(sender As System.Object, e As System.EventArgs)

    End Sub

    Private Sub Button8_Click(sender As System.Object, e As System.EventArgs) Handles Button8.Click
        Dim pp As DWSIM.SimulationObjects.PropertyPackages.PropertyPackage
        pp = FormMain.PropertyPackages(ListViewPP.SelectedItems(0).Text).Clone
        With pp
            pp.Tag = "PP_" & CStr(Me.dgvpp.Rows.Count + 1)
            pp.UniqueID = "PP-" & Guid.NewGuid.ToString
        End With
        FrmChild.Options.PropertyPackages.Add(pp.UniqueID, pp)
        Me.dgvpp.Rows.Add(New Object() {pp.UniqueID, pp.Tag, pp.ComponentName})
    End Sub

    Private Sub ComboBox2_SelectedIndexChanged(ByVal sender As System.Object, ByVal e As System.EventArgs) Handles ComboBox2.SelectedIndexChanged

        FrmChild.Options.SelectedUnitSystem = FormMain.AvailableUnitSystems.Item(ComboBox2.SelectedItem.ToString)
        Dim su As DWSIM.SistemasDeUnidades.Unidades = FrmChild.Options.SelectedUnitSystem

        With Me.DataGridView1.Rows
            .Clear()
            .Add(New Object() {DWSIM.App.GetLocalString("Temperatura"), su.spmp_temperature, DWSIM.App.GetLocalString("Presso"), su.spmp_pressure})
            .Add(New Object() {DWSIM.App.GetLocalString("Vazomssica"), su.spmp_massflow, DWSIM.App.GetLocalString("Vazomolar"), su.spmp_molarflow})
            .Add(New Object() {DWSIM.App.GetLocalString("Vazovolumtrica"), su.spmp_volumetricFlow, DWSIM.App.GetLocalString("EntalpiaEspecfica"), su.spmp_enthalpy})
            .Add(New Object() {DWSIM.App.GetLocalString("EntropiaEspecfica"), su.spmp_entropy, DWSIM.App.GetLocalString("Massamolar"), su.spmp_molecularWeight})
            .Add(New Object() {DWSIM.App.GetLocalString("Massaespecfica"), su.spmp_density, DWSIM.App.GetLocalString("Tensosuperficial"), su.tdp_surfaceTension})
            .Add(New Object() {DWSIM.App.GetLocalString("CapacidadeCalorfica"), su.spmp_heatCapacityCp, DWSIM.App.GetLocalString("Condutividadetrmica"), su.spmp_thermalConductivity})
            .Add(New Object() {DWSIM.App.GetLocalString("Viscosidadecinemtica"), su.spmp_cinematic_viscosity, DWSIM.App.GetLocalString("Viscosidadedinmica"), su.spmp_viscosity})
            .Add(New Object() {DWSIM.App.GetLocalString("DeltaT2"), su.spmp_deltaT, DWSIM.App.GetLocalString("DeltaP"), su.spmp_deltaP})
            .Add(New Object() {DWSIM.App.GetLocalString("ComprimentoHead"), su.spmp_head, DWSIM.App.GetLocalString("FluxodeEnergia"), su.spmp_heatflow})
            .Add(New Object() {DWSIM.App.GetLocalString("Tempo"), su.time, DWSIM.App.GetLocalString("Volume"), su.volume})
            .Add(New Object() {DWSIM.App.GetLocalString("VolumeMolar"), su.molar_volume, DWSIM.App.GetLocalString("rea"), su.area})
            .Add(New Object() {DWSIM.App.GetLocalString("DimetroEspessura"), su.diameter, DWSIM.App.GetLocalString("Fora"), su.force})
            .Add(New Object() {DWSIM.App.GetLocalString("Aceleracao"), su.accel, DWSIM.App.GetLocalString("CoefdeTransfdeCalor"), su.heat_transf_coeff})
            .Add(New Object() {DWSIM.App.GetLocalString("ConcMolar"), su.molar_conc, DWSIM.App.GetLocalString("ConcMssica"), su.mass_conc})
            .Add(New Object() {DWSIM.App.GetLocalString("TaxadeReao"), su.reac_rate, DWSIM.App.GetLocalString("VolEspecfico"), su.spec_vol})
            .Add(New Object() {DWSIM.App.GetLocalString("MolarEnthalpy"), su.molar_enthalpy, DWSIM.App.GetLocalString("MolarEntropy"), su.molar_entropy})
            .Add(New Object() {DWSIM.App.GetLocalString("Velocity"), su.velocity, DWSIM.App.GetLocalString("HXFoulingFactor"), su.foulingfactor})
            .Add(New Object() {DWSIM.App.GetLocalString("FilterSpecificCakeResistance"), su.cakeresistance, DWSIM.App.GetLocalString("FilterMediumResistance"), su.mediumresistance})
        End With

        If ComboBox2.SelectedIndex <= 2 Then
            Me.DataGridView1.Columns(1).ReadOnly = True
            Me.DataGridView1.Columns(3).ReadOnly = True
        Else
            Me.DataGridView1.Columns(1).ReadOnly = False
            Me.DataGridView1.Columns(3).ReadOnly = False
        End If

        Dim cb As DataGridViewComboBoxCell

        With Me.DataGridView1.Rows

            cb = New DataGridViewComboBoxCell
            cb.Items.AddRange(New String() {"K", "R", "C", "F"})
            cb.Value = su.spmp_temperature
            cb.Style.Tag = 1
            '.Add(New Object() {DWSIM.App.GetLocalString("Temperatura")})
            .Item(0).Cells(1) = cb

            cb = New DataGridViewComboBoxCell
            cb.Items.AddRange(New Object() {"Pa", "atm", "kgf/cm2", "kgf/cm2g", "lbf/ft2", "kPa", "kPag", "bar", "barg", "ftH2O", "inH2O", "inHg", "mbar", "mH2O", "mmH2O", "mmHg", "MPa", "psi", "psig"})
            cb.Value = su.spmp_pressure
            cb.Style.Tag = 2
            '.Add(New Object() {DWSIM.App.GetLocalString("Presso")})
            .Item(0).Cells(3) = cb

            cb = New DataGridViewComboBoxCell
            cb.Items.AddRange(New Object() {"g/s", "lbm/h", "kg/s", "kg/h", "kg/d", "kg/min", "lb/min", "lb/s"})
            cb.Value = su.spmp_massflow
            cb.Style.Tag = 3
            '.Add(New Object() {DWSIM.App.GetLocalString("Vazomssica")})
            .Item(1).Cells(1) = cb

            cb = New DataGridViewComboBoxCell
            cb.Items.AddRange(New Object() {"mol/s", "lbmol/h", "mol/h", "mol/d", "kmol/s", "kmol/h", "kmol/d", "m3/d @ BR", "m3/d @ NC", "m3/d @ CNTP", "m3/d @ SC"})
            cb.Value = su.spmp_molarflow
            cb.Style.Tag = 4
            '.Add(New Object() {DWSIM.App.GetLocalString("Vazomolar")})
            .Item(1).Cells(3) = cb

            cb = New DataGridViewComboBoxCell
            cb.Items.AddRange(New Object() {"m3/s", "ft3/s", "cm3/s", "m3/h", "m3/d", "bbl/h", "bbl/d", "ft3/min", "gal[UK]/h", "gal[UK]/s", "gal[US]/h", "gal[US]/min", "L/h", "L/min", "L/s"})
            cb.Value = su.spmp_volumetricFlow
            cb.Style.Tag = 5
            '.Add(New Object() {DWSIM.App.GetLocalString("Vazovolumtrica")})
            .Item(2).Cells(1) = cb

            cb = New DataGridViewComboBoxCell
            cb.Items.AddRange(New Object() {"kJ/kg", "cal/g", "BTU/lbm", "kcal/kg"})
            cb.Value = su.spmp_enthalpy
            cb.Style.Tag = 6
            '.Add(New Object() {DWSIM.App.GetLocalString("EntalpiaEspecfica")})
            .Item(2).Cells(3) = cb

            cb = New DataGridViewComboBoxCell
            cb.Items.AddRange(New Object() {"kJ/[kg.K]", "cal/[g.C]", "BTU/[lbm.R]"})
            cb.Value = su.spmp_entropy
            cb.Style.Tag = 7
            '.Add(New Object() {DWSIM.App.GetLocalString("EntropiaEspecfica")})
            .Item(3).Cells(1) = cb

            cb = New DataGridViewComboBoxCell
            cb.Items.AddRange(New Object() {"kg/kmol", "g/mol", "lbm/lbmol"})
            cb.Value = su.spmp_molecularWeight
            cb.Style.Tag = 8
            '.Add(New Object() {DWSIM.App.GetLocalString("Massamolar")})
            .Item(3).Cells(3) = cb

            cb = New DataGridViewComboBoxCell
            cb.Items.AddRange(New Object() {"N/m", "dyn/cm", "lbf/in"})
            cb.Value = su.tdp_surfaceTension
            cb.Style.Tag = 9
            '.Add(New Object() {DWSIM.App.GetLocalString("Tensosuperficial")})
            .Item(4).Cells(3) = cb

            cb = New DataGridViewComboBoxCell
            cb.Items.AddRange(New Object() {"kg/m3", "g/cm3", "lbm/ft3"})
            cb.Value = su.spmp_density
            cb.Style.Tag = 10
            '.Add(New Object() {DWSIM.App.GetLocalString("Massaespecfica")})
            .Item(4).Cells(1) = cb

            cb = New DataGridViewComboBoxCell
            cb.Items.AddRange(New Object() {"kJ/[kg.K]", "cal/[g.C]", "BTU/[lbm.R]"})
            cb.Value = su.spmp_heatCapacityCp
            cb.Style.Tag = 11
            '.Add(New Object() {DWSIM.App.GetLocalString("CapacidadeCalorfica")})
            .Item(5).Cells(1) = cb

            cb = New DataGridViewComboBoxCell
            cb.Items.AddRange(New Object() {"W/[m.K]", "cal/[cm.s.C]", "BTU/[ft.h.R]"})
            cb.Value = su.spmp_thermalConductivity
            cb.Style.Tag = 12
            '.Add(New Object() {DWSIM.App.GetLocalString("Condutividadetrmica")})
            .Item(5).Cells(3) = cb

            cb = New DataGridViewComboBoxCell
            cb.Items.AddRange(New Object() {"m2/s", "cSt", "ft2/s", "mm2/s"})
            cb.Value = su.spmp_cinematic_viscosity
            cb.Style.Tag = 13
            '.Add(New Object() {DWSIM.App.GetLocalString("Viscosidadecinemtica")})
            .Item(6).Cells(1) = cb

            cb = New DataGridViewComboBoxCell
            cb.Items.AddRange(New Object() {"kg/[m.s]", "Pa.s", "cP", "lbm/[ft.h]"})
            cb.Value = su.spmp_viscosity
            cb.Style.Tag = 14
            '.Add(New Object() {DWSIM.App.GetLocalString("ViscosidadeDinmica1")})
            .Item(6).Cells(3) = cb

            cb = New DataGridViewComboBoxCell
            cb.Items.AddRange(New Object() {"Pa", "atm", "lbf/ft2", "kgf/cm2", "kPa", "bar", "ftH2O", "inH2O", "inHg", "mbar", "mH2O", "mmH2O", "mmHg", "MPa", "psi"})
            cb.Value = su.spmp_deltaP
            cb.Style.Tag = 15
            '.Add(New Object() {DWSIM.App.GetLocalString("DeltaP")})
            .Item(7).Cells(3) = cb

            cb = New DataGridViewComboBoxCell
            cb.Items.AddRange(New Object() {"C.", "K.", "F.", "R."})
            cb.Value = su.spmp_deltaT
            cb.Style.Tag = 16
            '.Add(New Object() {DWSIM.App.GetLocalString("DeltaT2")})
            .Item(7).Cells(1) = cb

            cb = New DataGridViewComboBoxCell
            cb.Items.AddRange(New Object() {"m", "ft", "cm"})
            cb.Value = su.spmp_head
            cb.Style.Tag = 17
            '.Add(New Object() {DWSIM.App.GetLocalString("ComprimentoHead")})
            .Item(8).Cells(1) = cb

            cb = New DataGridViewComboBoxCell
            cb.Items.AddRange(New Object() {"kW", "kcal/h", "BTU/h", "BTU/s", "cal/s", "HP", "kJ/h", "kJ/d", "MW", "W"})
            cb.Value = su.spmp_heatflow
            cb.Style.Tag = 18
            '.Add(New Object() {DWSIM.App.GetLocalString("FluxodeEnergia")})
            .Item(8).Cells(3) = cb

            cb = New DataGridViewComboBoxCell
            cb.Items.AddRange(New String() {"s", "min.", "h"})
            cb.Value = su.time
            cb.Style.Tag = 19
            '.Add(New Object() {DWSIM.App.GetLocalString("Tempo")})
            .Item(9).Cells(1) = cb

            cb = New DataGridViewComboBoxCell
            cb.Items.AddRange(New String() {"m3", "cm3", "L", "ft3"})
            cb.Value = su.volume
            cb.Style.Tag = 20
            '.Add(New Object() {DWSIM.App.GetLocalString("Volume")})
            .Item(9).Cells(3) = cb

            cb = New DataGridViewComboBoxCell
            cb.Items.AddRange(New String() {DWSIM.App.GetLocalString("m3kmol"), "cm3/mmol", "ft3/lbmol"})
            cb.Value = su.molar_volume
            cb.Style.Tag = 21
            '.Add(New Object() {DWSIM.App.GetLocalString("VolumeMolar")})
            .Item(10).Cells(1) = cb

            cb = New DataGridViewComboBoxCell
            cb.Items.AddRange(New String() {"m2", "cm2", "ft2"})
            cb.Value = su.area
            cb.Style.Tag = 22
            '.Add(New Object() {DWSIM.App.GetLocalString("rea")})
            .Item(10).Cells(3) = cb

            cb = New DataGridViewComboBoxCell
            cb.Items.AddRange(New String() {"m", "mm", "cm", "in", "ft"})
            cb.Value = su.diameter
            cb.Style.Tag = 23
            '.Add(New Object() {DWSIM.App.GetLocalString("DimetroEspessura")})
            .Item(11).Cells(1) = cb

            cb = New DataGridViewComboBoxCell
            cb.Items.AddRange(New String() {DWSIM.App.GetLocalString("N"), "dyn", "kgf", "lbf"})
            cb.Value = su.force
            cb.Style.Tag = 24
            '.Add(New Object() {DWSIM.App.GetLocalString("Fora")})
            .Item(11).Cells(3) = cb

            cb = New DataGridViewComboBoxCell
            cb.Items.AddRange(New String() {"W/[m2.K]", "cal/[cm2.s.C]", "BTU/[ft2.h.R]"})
            cb.Value = su.heat_transf_coeff
            cb.Style.Tag = 25
            '.Add(New Object() {DWSIM.App.GetLocalString("CoefdeTransfdeCalor")})
            .Item(12).Cells(3) = cb

            cb = New DataGridViewComboBoxCell
            cb.Items.AddRange(New String() {"m/s2", "cm/s2", "ft/s2"})
            cb.Value = su.accel
            cb.Style.Tag = 26
            '.Add(New Object() {DWSIM.App.GetLocalString("Aceleracao")})
            .Item(12).Cells(1) = cb

            cb = New DataGridViewComboBoxCell
            cb.Items.AddRange(New String() {"kmol/m3", "mol/m3", "mol/L", "mol/cm3", "mol/mL", "lbmol/ft3"})
            cb.Value = su.molar_conc
            cb.Style.Tag = 28
            '.Add(New Object() {DWSIM.App.GetLocalString("ConcentraoMolar")})
            .Item(13).Cells(1) = cb

            cb = New DataGridViewComboBoxCell
            cb.Items.AddRange(New String() {"kg/m3", "g/L", "g/cm3", "g/mL", "lbm/ft3"})
            cb.Value = su.mass_conc
            cb.Style.Tag = 29
            '.Add(New Object() {DWSIM.App.GetLocalString("ConcentraoMssica")})
            .Item(13).Cells(3) = cb

            cb = New DataGridViewComboBoxCell
            cb.Items.AddRange(New String() {"kmol/[m3.s]", "kmol/[m3.min.]", "kmol/[m3.h]", "mol/[m3.s]", "mol/[m3.min.]", "mol/[m3.h]", "mol/[L.s]", "mol/[L.min.]", "mol/[L.h]", "mol/[cm3.s]", "mol/[cm3.min.]", "mol/[cm3.h]", "lbmol.[ft3.h]"})
            cb.Value = su.reac_rate
            cb.Style.Tag = 30
            '.Add(New Object() {DWSIM.App.GetLocalString("TaxadeReao")})
            .Item(14).Cells(1) = cb

            cb = New DataGridViewComboBoxCell
            cb.Items.AddRange(New String() {"m3/kg", "cm3/g", "ft3/lbm"})
            cb.Value = su.spec_vol
            cb.Style.Tag = 27
            '.Add(New Object() {DWSIM.App.GetLocalString("VolumeEspecfico")})
            .Item(14).Cells(3) = cb

            cb = New DataGridViewComboBoxCell
            cb.Items.AddRange(New String() {"kJ/kmol", "cal/mol", "BTU/lbmol"})
            cb.Value = su.molar_enthalpy
            cb.Style.Tag = 31
            '.Add(New Object() {DWSIM.App.GetLocalString("MolarEnthalpy")})
            .Item(15).Cells(1) = cb

            cb = New DataGridViewComboBoxCell
            cb.Items.AddRange(New String() {"kJ/[kmol.K]", "cal/[mol.C]", "BTU/[lbmol.R]"})
            cb.Value = su.molar_entropy
            cb.Style.Tag = 32
            '.Add(New Object() {DWSIM.App.GetLocalString("MolarEntropy")})
            .Item(15).Cells(3) = cb

            cb = New DataGridViewComboBoxCell
            cb.Items.AddRange(New String() {"m/s", "cm/s", "mm/s", "km/h", "ft/h", "ft/min", "ft/s", "in/s"})
            cb.Value = su.velocity
            cb.Style.Tag = 33
            '.Add(New Object() {DWSIM.App.GetLocalString("Velocity")})
            .Item(16).Cells(1) = cb

            cb = New DataGridViewComboBoxCell
            cb.Items.AddRange(New String() {"K.m2/W", "C.cm2.s/cal", "ft2.h.F/BTU"})
            cb.Value = su.foulingfactor
            cb.Style.Tag = 34
            '.Add(New Object() {DWSIM.App.GetLocalString("HXFoulingFactor")})
            .Item(16).Cells(3) = cb

            cb = New DataGridViewComboBoxCell
            cb.Items.AddRange(New String() {"m/kg", "ft/lbm", "cm/g"})
            cb.Value = su.cakeresistance
            cb.Style.Tag = 35
            '.Add(New Object() {DWSIM.App.GetLocalString("FilterSpecificCakeResistance")})
            .Item(17).Cells(1) = cb

            cb = New DataGridViewComboBoxCell
            cb.Items.AddRange(New String() {"m-1", "cm-1", "ft-1"})
            cb.Value = su.mediumresistance
            cb.Style.Tag = 36
            '.Add(New Object() {DWSIM.App.GetLocalString("FilterMediumResistance")})
            .Item(17).Cells(3) = cb

        End With

        FrmChild.ToolStripComboBoxUnitSystem.SelectedItem = ComboBox2.SelectedItem

    End Sub

    Private Sub ListViewPP_DoubleClick(sender As Object, e As EventArgs) Handles ListViewPP.DoubleClick
        If ListViewPP.SelectedItems.Count = 1 Then
            Button8.PerformClick()
        End If
    End Sub

    Private Sub ListViewPP_SelectedIndexChanged(ByVal sender As System.Object, ByVal e As System.EventArgs) Handles ListViewPP.SelectedIndexChanged
        If Me.ListViewPP.SelectedItems.Count > 0 Then
            Me.Button8.Enabled = True
        Else
            Me.Button8.Enabled = False
        End If
    End Sub

    Private Sub ListBoxPP_SelectedIndexChanged(sender As System.Object, e As System.EventArgs) Handles ListBoxPP.SelectedIndexChanged
        Select Case ListBoxPP.SelectedIndex
            Case 0
                Me.FrmChild.Options.PropertyPackageFlashAlgorithm = DWSIM.SimulationObjects.PropertyPackages.FlashMethod.DWSIMDefault
            Case 1
                Me.FrmChild.Options.PropertyPackageFlashAlgorithm = DWSIM.SimulationObjects.PropertyPackages.FlashMethod.NestedLoops3PV2
            Case 2
                Me.FrmChild.Options.PropertyPackageFlashAlgorithm = DWSIM.SimulationObjects.PropertyPackages.FlashMethod.InsideOut
            Case 3
                Me.FrmChild.Options.PropertyPackageFlashAlgorithm = DWSIM.SimulationObjects.PropertyPackages.FlashMethod.InsideOut3P
            Case 4
                Me.FrmChild.Options.PropertyPackageFlashAlgorithm = DWSIM.SimulationObjects.PropertyPackages.FlashMethod.GibbsMin2P
            Case 5
                Me.FrmChild.Options.PropertyPackageFlashAlgorithm = DWSIM.SimulationObjects.PropertyPackages.FlashMethod.GibbsMin3P
            Case 6
                Me.FrmChild.Options.PropertyPackageFlashAlgorithm = DWSIM.SimulationObjects.PropertyPackages.FlashMethod.NestedLoops3P
            Case 7
                Me.FrmChild.Options.PropertyPackageFlashAlgorithm = DWSIM.SimulationObjects.PropertyPackages.FlashMethod.NestedLoopsSLE
            Case 8
                Me.FrmChild.Options.PropertyPackageFlashAlgorithm = DWSIM.SimulationObjects.PropertyPackages.FlashMethod.NestedLoopsSLE_SS
            Case 9
                Me.FrmChild.Options.PropertyPackageFlashAlgorithm = DWSIM.SimulationObjects.PropertyPackages.FlashMethod.NestedLoopsImmiscible
        End Select
    End Sub

    Private Sub DataGridView1_DataError(ByVal sender As Object, ByVal e As System.Windows.Forms.DataGridViewDataErrorEventArgs) Handles DataGridView1.DataError

    End Sub

    Private Sub ogc1_CellDoubleClick(sender As Object, e As System.Windows.Forms.DataGridViewCellEventArgs) Handles ogc1.CellDoubleClick
        If e.RowIndex > -1 Then
            AddCompToSimulation(e.RowIndex)
        End If
    End Sub

    Sub UpdateKeyCompounds()

        Try
            Dim i As Integer = 0
            Dim sel As New ArrayList
            Dim lvi2 As ListViewItem
            For Each lvi2 In Me.ListViewA.Items
                If Not lvi2 Is Nothing Then sel.Add(lvi2.Tag)
            Next
            FrmChild.Options.ThreePhaseFlashStabTestCompIds = sel.ToArray(Type.GetType("System.String"))
        Catch ex As Exception

        End Try

    End Sub

    Private Sub Button1_Click(sender As System.Object, e As System.EventArgs) Handles Button1.Click

        switch = True

        Me.Close()

    End Sub

End Class