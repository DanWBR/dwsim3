Imports com.ggasoftware.indigo
Imports DWSIM.DWSIM.ClassesBasicasTermodinamica
Imports DWSIM.DWSIM.SimulationObjects.Streams

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

Public Class FormPureComp

    Inherits System.Windows.Forms.Form

    Dim MatStream As DWSIM.SimulationObjects.Streams.MaterialStream

    Public ChildParent As FormFlowsheet

    Public OnlyViewing As Boolean = True

    Dim vxCp, vyCp, vxPvap, vyPvap, vxVisc, vyVisc, vxDHvap, vyDHvap As New ArrayList
    Public constprop As DWSIM.ClassesBasicasTermodinamica.ConstantProperties
    Public constprop_orig As DWSIM.ClassesBasicasTermodinamica.ConstantProperties

    Private Sub FormPureComp_Load(ByVal sender As System.Object, ByVal e As System.EventArgs) Handles MyBase.Load

        Me.ChildParent = My.Application.ActiveSimulation

        OnlyViewing = False
        If constprop Is Nothing Then
            OnlyViewing = True

            With Me.ChildParent
                Dim subst As DWSIM.ClassesBasicasTermodinamica.ConstantProperties
                Me.ComboBox1.Items.Clear()
                For Each subst In .Options.SelectedComponents.Values
                    Me.ComboBox1.Items.Add(DWSIM.App.GetComponentName(subst.Name) + " [" + subst.Name + "]")
                Next
            End With
        End If

        If ChildParent.Options.SelectedPropertyPackage Is Nothing Then
            MessageBox.Show(DWSIM.App.GetLocalString("NoPropPackDefined"), DWSIM.App.GetLocalString("Erro"), MessageBoxButtons.OK, MessageBoxIcon.Error)

            Me.Close()
        Else
            Me.ComboBox1.SelectedIndex = 0
        End If

    End Sub

    Sub Populate()

        Dim su As DWSIM.SistemasDeUnidades.Unidades = ChildParent.Options.SelectedUnitSystem
        Dim cv As New DWSIM.SistemasDeUnidades.Conversor
        Dim nf As String = ChildParent.Options.NumberFormat
        Dim pp As DWSIM.SimulationObjects.PropertyPackages.PropertyPackage = ChildParent.Options.SelectedPropertyPackage

        Me.MatStream = New MaterialStream("", "")

        'add simulation compounds to the dummy material stream
        Me.ChildParent.AddComponentsRows(Me.MatStream)

        pp.CurrentMaterialStream = MatStream

        'setting up datatable
        Dim Row As Integer
        Dim TD, VD As Double
        Me.DataTable.Rows.Clear()
        Me.DataTable.Rows.Add(51)
        Me.DataTable.Columns.Item(0).HeaderText = "Temp " & su.spmp_temperature
        Me.DataTable.Columns.Item(2).HeaderText = "Temp " & su.spmp_temperature
        Me.DataTable.Columns.Item(4).HeaderText = "Temp " & su.spmp_temperature
        Me.DataTable.Columns.Item(6).HeaderText = "Temp " & su.spmp_temperature

        Me.DataTable.Columns.Item(1).HeaderText = DWSIM.App.GetLocalString("CapacidadeCalorficaP2") & " " & su.spmp_heatCapacityCp
        Me.DataTable.Columns.Item(3).HeaderText = DWSIM.App.GetLocalString("PressodeVapor") & " " & su.spmp_pressure
        Me.DataTable.Columns.Item(5).HeaderText = DWSIM.App.GetLocalString("ViscosidadeLquido") & " " & su.spmp_viscosity
        Me.DataTable.Columns.Item(7).HeaderText = DWSIM.App.GetLocalString("EntalpiadeVaporizao") & " " & su.spmp_enthalpy

        'setting up curves
        Dim T As Double
        Dim Tmin, Tmax, delta As Double

        'gas heat capacity
        Tmin = 200
        Tmax = 1500
        delta = (Tmax - Tmin) / 50

        T = Tmin
        Row = 0
        vxCp.Clear()
        vyCp.Clear()

        If Not constprop.IsIon Or Not constprop.IsSalt Then
            Do
                TD = cv.ConverterDoSI(su.spmp_temperature, T)
                VD = cv.ConverterDoSI(su.spmp_heatCapacityCp, pp.AUX_CPi(constprop.Name, T))
                vxCp.Add(TD)
                vyCp.Add(VD)
                Me.DataTable.Item(0, Row).Value = Format(TD, nf)
                Me.DataTable.Item(1, Row).Value = Format(VD, nf)
                T += delta
                Row += 1
            Loop Until T > Tmax
        End If

        'pressao de vapor
        With constprop
            Tmin = .Vapor_Pressure_TMIN
            Tmax = .Vapor_Pressure_TMAX
            If Tmin = 0 Then Tmin = 0.4 * .Critical_Temperature
            If Tmax = 0 Then Tmax = .Critical_Temperature
            delta = (Tmax - Tmin) / 50
        End With
        T = Tmin
        Row = 0
        vxPvap.Clear()
        vyPvap.Clear()
        If Not constprop.IsIon And Not constprop.IsSalt Then
            Do
                TD = cv.ConverterDoSI(su.spmp_temperature, T)
                VD = cv.ConverterDoSI(su.spmp_heatCapacityCp, pp.AUX_PVAPi(constprop.Name, T))
                vxPvap.Add(TD)
                vyPvap.Add(VD)
                Me.DataTable.Item(2, Row).Value = Format(TD, nf)
                Me.DataTable.Item(3, Row).Value = Format(VD, nf)
                T += delta
                Row += 1
            Loop Until Row = 51
        End If

        'viscosidade liquido
        With constprop
            Tmin = 0.6 * .Critical_Temperature
            Tmax = .Critical_Temperature
            delta = (Tmax - Tmin) / 50
        End With
        T = Tmin
        Row = 0
        vxVisc.Clear()
        vyVisc.Clear()
        If Not constprop.IsIon And Not constprop.IsSalt Then
            Do
                TD = cv.ConverterDoSI(su.spmp_temperature, T)
                VD = cv.ConverterDoSI(su.spmp_viscosity, pp.AUX_LIQVISCi(constprop.Name, T))
                vxVisc.Add(TD)
                vyVisc.Add(VD)
                Me.DataTable.Item(4, Row).Value = Format(TD, nf)
                Me.DataTable.Item(5, Row).Value = Format(VD, nf)
                T += delta
                Row += 1
            Loop Until Row = 51
        End If

        'entalpia vaporizacao
        With constprop
            Tmin = .HVap_TMIN
            Tmax = .HVap_TMAX
            If Tmin = 0 Then Tmin = 0.6 * .Critical_Temperature
            If Tmax = 0 Then Tmax = .Critical_Temperature
            delta = (Tmax - Tmin) / 50
        End With
        T = Tmin
        Row = 0
        vxDHvap.Clear()
        vyDHvap.Clear()
        If Not constprop.IsIon And Not constprop.IsSalt Then
            Do
                TD = cv.ConverterDoSI(su.spmp_temperature, T)
                VD = cv.ConverterDoSI(su.spmp_enthalpy, pp.AUX_HVAPi(constprop.Name, T))
                vxDHvap.Add(TD)
                vyDHvap.Add(VD)
                Me.DataTable.Item(6, Row).Value = Format(TD, nf)
                Me.DataTable.Item(7, Row).Value = Format(VD, nf)
                T += delta
                Row += 1
            Loop Until Row = 51
        End If

        With Me.GraphCp.GraphPane
            .CurveList.Clear()
            With .AddCurve("", Me.vxCp.ToArray(GetType(Double)), Me.vyCp.ToArray(GetType(Double)), Color.Blue, ZedGraph.SymbolType.Circle)
                .Color = Color.SteelBlue
                .Line.IsSmooth = False
                .Symbol.Fill.Type = ZedGraph.FillType.Solid
            End With
            .Title.Text = DWSIM.App.GetLocalString("CapacidadeCalorficaP2")
            .XAxis.Title.Text = "T / " & su.spmp_temperature
            .YAxis.Title.Text = "Cp / " & su.spmp_heatCapacityCp
            .AxisChange(Me.CreateGraphics)
        End With
        Me.GraphCp.Invalidate()
        With Me.GraphPvap.GraphPane
            .CurveList.Clear()
            With .AddCurve("", Me.vxPvap.ToArray(GetType(Double)), Me.vyPvap.ToArray(GetType(Double)), Color.Blue, ZedGraph.SymbolType.Circle)
                .Color = Color.SteelBlue
                .Line.IsSmooth = False
                .Symbol.Fill.Type = ZedGraph.FillType.Solid
            End With
            .Title.Text = DWSIM.App.GetLocalString("PressodeVapor")
            .XAxis.Title.Text = "T / " & su.spmp_temperature
            .YAxis.Title.Text = "Pvap / " & su.spmp_pressure
            .AxisChange(Me.CreateGraphics)
        End With
        Me.GraphPvap.Invalidate()
        With Me.GraphVisc.GraphPane
            .CurveList.Clear()
            With .AddCurve("", Me.vxVisc.ToArray(GetType(Double)), Me.vyVisc.ToArray(GetType(Double)), Color.Blue, ZedGraph.SymbolType.Circle)
                .Color = Color.SteelBlue
                .Line.IsSmooth = False
                .Symbol.Fill.Type = ZedGraph.FillType.Solid
            End With
            .Title.Text = DWSIM.App.GetLocalString("ViscosidadeLquido")
            .XAxis.Title.Text = "T / " & su.spmp_temperature
            .YAxis.Title.Text = "Visc / " & su.spmp_viscosity
            .AxisChange(Me.CreateGraphics)
        End With
        Me.GraphVisc.Invalidate()
        With Me.GraphDHVAP.GraphPane
            .CurveList.Clear()
            With .AddCurve("", Me.vxDHvap.ToArray(GetType(Double)), Me.vyDHvap.ToArray(GetType(Double)), Color.Blue, ZedGraph.SymbolType.Circle)
                .Color = Color.SteelBlue
                .Line.IsSmooth = False
                .Symbol.Fill.Type = ZedGraph.FillType.Solid
            End With
            .Title.Text = DWSIM.App.GetLocalString("EntalpiadeVaporizao")
            .XAxis.Title.Text = "T / " & su.spmp_temperature
            .YAxis.Title.Text = "DHvap / " & su.spmp_enthalpy
            .AxisChange(Me.CreateGraphics)
        End With
        Me.GraphDHVAP.Invalidate()

        'UNIFAC
        tbUNIFAC.Text = ""
        If Not constprop.UNIFACGroups Is Nothing Then
            For Each s As String In constprop.UNIFACGroups.Collection.Keys
                tbUNIFAC.Text += constprop.UNIFACGroups.Collection(s) & " " & s & ", "
            Next
            tbUNIFAC.Text = tbUNIFAC.Text.TrimEnd(New Char() {",", " "})
        End If

        'MODFAC
        tbMODFAC.Text = ""
        If Not constprop.MODFACGroups Is Nothing Then
            For Each s As String In constprop.MODFACGroups.Collection.Keys
                tbMODFAC.Text += constprop.MODFACGroups.Collection(s) & " " & s & ", "
            Next
            tbMODFAC.Text = tbMODFAC.Text.TrimEnd(New Char() {",", " "})
        End If

        tbFormula.Text = constprop.Formula
        tbSMILES.Text = constprop.SMILES
        tbInChI.Text = constprop.InChI


        'Render molecule / Calculate InChI from SMILES
        If Not constprop.SMILES Is Nothing And Not constprop.SMILES = "" Then

            'definition available, render molecule
            Try
                Dim ind As New Indigo()
                Dim mol As IndigoObject = ind.loadMolecule(constprop.SMILES)
                Dim renderer As New IndigoRenderer(ind)

                If constprop.InChI = "" Then
                    Dim ii As New IndigoInchi(ind)
                    tbInChI.Text = ii.getInchi(mol)
                End If

                With renderer
                    ind.setOption("render-image-size", 733, 222)
                    ind.setOption("render-margins", 15, 15)
                    ind.setOption("render-coloring", True)
                    ind.setOption("render-background-color", Color.White)
                End With

                pbRender.Image = renderer.renderToBitmap(mol)

            Catch ex As Exception
                MessageBox.Show(ex.ToString, DWSIM.App.GetLocalString("Erro"), MessageBoxButtons.OK, MessageBoxIcon.Error)

            End Try
        Else
            'no definition available, delete old picture
            pbRender.Image = Nothing

        End If


        'Grid Propriedades
        With Me.GridProps.Rows
            .Clear()
            .Add(New Object() {DWSIM.App.GetLocalString("Database"), constprop.OriginalDB, ""})
            .Add(New Object() {DWSIM.App.GetLocalString("Type"), DWSIM.App.GetComponentType(constprop), ""})
            .Add(New Object() {"ID", constprop.ID, ""})
            .Add(New Object() {DWSIM.App.GetLocalString("CASNumber"), constprop.CAS_Number, ""})
            .Add(New Object() {DWSIM.App.GetLocalString("Massamolar"), Format(constprop.Molar_Weight, nf), su.spmp_molecularWeight})
            .Add(New Object() {DWSIM.App.GetLocalString("TemperaturaCrtica"), Format(cv.ConverterDoSI(su.spmp_temperature, constprop.Critical_Temperature), nf), su.spmp_temperature})
            .Add(New Object() {DWSIM.App.GetLocalString("PressoCrtica"), Format(cv.ConverterDoSI(su.spmp_pressure, constprop.Critical_Pressure), nf), su.spmp_pressure})
            .Add(New Object() {DWSIM.App.GetLocalString("VolumeCrtico"), Format(cv.ConverterDoSI(su.molar_volume, constprop.Critical_Volume), nf), su.molar_volume})
            .Add(New Object() {DWSIM.App.GetLocalString("CompressibilidadeCrt"), Format(constprop.Critical_Compressibility, nf), ""})
            .Add(New Object() {DWSIM.App.GetLocalString("FatorAcntrico"), Format(constprop.Acentric_Factor, nf), ""})
            .Add(New Object() {DWSIM.App.GetLocalString("EntalpiadeFormaodoGs"), Format(cv.ConverterDoSI(su.spmp_enthalpy, constprop.IG_Enthalpy_of_Formation_25C), nf), su.spmp_enthalpy})
            .Add(New Object() {DWSIM.App.GetLocalString("EnergiadeGibbsdeForm2"), Format(cv.ConverterDoSI(su.spmp_entropy, constprop.IG_Gibbs_Energy_of_Formation_25C), nf), su.spmp_enthalpy})
            .Add(New Object() {DWSIM.App.GetLocalString("PontoNormaldeEbulio"), Format(cv.ConverterDoSI(su.spmp_temperature, constprop.Normal_Boiling_Point), nf), su.spmp_temperature})
            .Add(New Object() {DWSIM.App.GetLocalString("TemperatureOfFusion"), Format(cv.ConverterDoSI(su.spmp_temperature, constprop.TemperatureOfFusion), nf), su.spmp_temperature})
            .Add(New Object() {DWSIM.App.GetLocalString("EnthalpyOfFusionAtTf"), Format(constprop.EnthalpyOfFusionAtTf, nf), "kJ/mol"})
            .Add(New Object() {DWSIM.App.GetLocalString("TemperatureOfSolidDensity_Ts"), Format(cv.ConverterDoSI(su.spmp_temperature, constprop.SolidTs), nf), su.spmp_temperature})
            .Add(New Object() {DWSIM.App.GetLocalString("SolidDensityAtTs"), Format(cv.ConverterDoSI(su.spmp_density, constprop.SolidDensityAtTs), nf), su.spmp_density})
            .Add(New Object() {DWSIM.App.GetLocalString("ChaoSeaderAcentricFactor"), Format(constprop.Chao_Seader_Acentricity, nf), "-"})
            .Add(New Object() {DWSIM.App.GetLocalString("ChaoSeaderSolubilityParameter"), Format(constprop.Chao_Seader_Solubility_Parameter, nf), "(cal/mL)^0.5"})
            .Add(New Object() {DWSIM.App.GetLocalString("ChaoSeaderLiquidMolarVolume"), Format(constprop.Chao_Seader_Liquid_Molar_Volume, nf), "mL/mol"})
            .Add(New Object() {DWSIM.App.GetLocalString("RackettCompressibility"), Format(constprop.Z_Rackett, nf), ""})
            .Add(New Object() {DWSIM.App.GetLocalString("PengRobinsonVolumeTranslationCoefficient"), Format(constprop.PR_Volume_Translation_Coefficient, nf), ""})
            .Add(New Object() {DWSIM.App.GetLocalString("SRKVolumeTranslationCoefficient"), Format(constprop.SRK_Volume_Translation_Coefficient, nf), ""})
            .Add(New Object() {"UNIQUAC R", Format(constprop.UNIQUAC_R, nf), ""})
            .Add(New Object() {"UNIQUAC Q", Format(constprop.UNIQUAC_Q, nf), ""})
            .Add(New Object() {DWSIM.App.GetLocalString("Charge"), Format(constprop.Charge, "+#;-#"), ""})
            .Add(New Object() {DWSIM.App.GetLocalString("HydrationNumber"), constprop.HydrationNumber, ""})
            .Add(New Object() {DWSIM.App.GetLocalString("PositiveIon"), constprop.PositiveIon, ""})
            .Add(New Object() {DWSIM.App.GetLocalString("NegativeIon"), constprop.NegativeIon, ""})

        End With

        chkEnableEdit_CheckedChanged(Me, New EventArgs)

    End Sub

    Private Sub ComboBox1_SelectedIndexChanged(ByVal sender As Object, ByVal e As System.EventArgs) Handles ComboBox1.SelectedIndexChanged
        If OnlyViewing = True Or constprop Is Nothing Then
            Dim name As String = ComboBox1.SelectedItem.ToString.Substring(ComboBox1.SelectedItem.ToString.IndexOf("[") + 1, ComboBox1.SelectedItem.ToString.Length - ComboBox1.SelectedItem.ToString.IndexOf("[") - 2)
            constprop = Nothing
            constprop = Me.ChildParent.Options.SelectedComponents(name)
        End If
        Call Me.Populate()
    End Sub

    Private Sub GridProps_CellEndEdit(ByVal sender As System.Object, ByVal e As System.Windows.Forms.DataGridViewCellEventArgs) Handles GridProps.CellEndEdit

        Dim cv As New DWSIM.SistemasDeUnidades.Conversor
        Dim su As DWSIM.SistemasDeUnidades.Unidades = ChildParent.Options.SelectedUnitSystem
        Select Case e.RowIndex
            Case 0 '.Add(New Object() {DWSIM.App.GetLocalString("Database"), constprop.CurrentDB, ""})
            Case 1 '.Add(New Object() {DWSIM.App.GetLocalString("Type"), DWSIM.App.GetComponentType(constprop), ""})
            Case 2 '.Add(New Object() {"ID", constprop.ID, ""})
            Case 3 '.Add(New Object() {DWSIM.App.GetLocalString("CASNumber"), constprop.CAS_Number, ""})
            Case 4 '.Add(New Object() {DWSIM.App.GetLocalString("Massamolar"), Format(constprop.Molar_Weight, nf), su.spmp_molecularWeight})
                constprop.Molar_Weight = cv.ConverterParaSI(su.spmp_molecularWeight, GridProps.Rows(e.RowIndex).Cells(1).Value)
            Case 5 '.Add(New Object() {DWSIM.App.GetLocalString("TemperaturaCrtica"), Format(cv.ConverterDoSI(su.spmp_temperature, constprop.Critical_Temperature), nf), su.spmp_temperature})
                constprop.Critical_Temperature = cv.ConverterParaSI(su.spmp_temperature, GridProps.Rows(e.RowIndex).Cells(1).Value)
            Case 6 '.Add(New Object() {DWSIM.App.GetLocalString("PressoCrtica"), Format(cv.ConverterDoSI(su.spmp_pressure, constprop.Critical_Pressure), nf), su.spmp_pressure})
                constprop.Critical_Pressure = cv.ConverterParaSI(su.spmp_pressure, GridProps.Rows(e.RowIndex).Cells(1).Value)
            Case 7 '.Add(New Object() {DWSIM.App.GetLocalString("VolumeCrtico"), Format(cv.ConverterDoSI(su.molar_volume, constprop.Critical_Volume), nf), su.molar_volume})
                constprop.Critical_Volume = cv.ConverterParaSI(su.volume, GridProps.Rows(e.RowIndex).Cells(1).Value)
            Case 8 '.Add(New Object() {DWSIM.App.GetLocalString("CompressibilidadeCrt"), Format(constprop.Critical_Compressibility, nf), ""})
                constprop.Critical_Compressibility = GridProps.Rows(e.RowIndex).Cells(1).Value
            Case 9 '.Add(New Object() {DWSIM.App.GetLocalString("FatorAcntrico"), Format(constprop.Acentric_Factor, nf), ""})
                constprop.Acentric_Factor = GridProps.Rows(e.RowIndex).Cells(1).Value
            Case 10 '.Add(New Object() {DWSIM.App.GetLocalString("EntalpiadeFormaodoGs"), Format(cv.ConverterDoSI(su.spmp_enthalpy, constprop.IG_Enthalpy_of_Formation_25C), nf), su.spmp_enthalpy})
                constprop.IG_Enthalpy_of_Formation_25C = cv.ConverterParaSI(su.spmp_enthalpy, GridProps.Rows(e.RowIndex).Cells(1).Value)
            Case 11 '.Add(New Object() {DWSIM.App.GetLocalString("EnergiadeGibbsdeForm2"), Format(cv.ConverterDoSI(su.spmp_entropy, constprop.IG_Gibbs_Energy_of_Formation_25C), nf), su.spmp_enthalpy})
                constprop.IG_Gibbs_Energy_of_Formation_25C = cv.ConverterParaSI(su.spmp_enthalpy, GridProps.Rows(e.RowIndex).Cells(1).Value)
            Case 12 '.Add(New Object() {DWSIM.App.GetLocalString("PontoNormaldeEbulio"), Format(cv.ConverterDoSI(su.spmp_temperature, constprop.Normal_Boiling_Point), nf), su.spmp_temperature})
                constprop.Normal_Boiling_Point = cv.ConverterParaSI(su.spmp_temperature, GridProps.Rows(e.RowIndex).Cells(1).Value)
                constprop.NBP = cv.ConverterParaSI(su.spmp_temperature, GridProps.Rows(e.RowIndex).Cells(1).Value)
            Case 17 '.Add(New Object() {DWSIM.App.GetLocalString("ChaoSeaderAcentricFactor"), Format(constprop.Chao_Seader_Acentricity, nf), "-"})
                constprop.Chao_Seader_Acentricity = GridProps.Rows(e.RowIndex).Cells(1).Value
            Case 18 '.Add(New Object() {DWSIM.App.GetLocalString("ChaoSeaderSolubilityParameter"), Format(constprop.Chao_Seader_Solubility_Parameter, nf), "(cal/mL)^0.5"})
                constprop.Chao_Seader_Solubility_Parameter = GridProps.Rows(e.RowIndex).Cells(1).Value
            Case 19 '.Add(New Object() {DWSIM.App.GetLocalString("ChaoSeaderLiquidMolarVolume"), Format(constprop.Chao_Seader_Liquid_Molar_Volume, nf), "mL/mol"})
                constprop.Chao_Seader_Liquid_Molar_Volume = GridProps.Rows(e.RowIndex).Cells(1).Value
            Case 20 '.Add(New Object() {DWSIM.App.GetLocalString("RackettCompressibility"), Format(constprop.Z_Rackett, nf), ""})
                constprop.Z_Rackett = GridProps.Rows(e.RowIndex).Cells(1).Value
            Case 21 '.Add(New Object() {DWSIM.App.GetLocalString("PengRobinsonVolumeTranslationCoefficient"), Format(constprop.PR_Volume_Translation_Coefficient, nf), ""})
                constprop.PR_Volume_Translation_Coefficient = GridProps.Rows(e.RowIndex).Cells(1).Value
            Case 22 '.Add(New Object() {DWSIM.App.GetLocalString("SRKVolumeTranslationCoefficient"), Format(constprop.SRK_Volume_Translation_Coefficient, nf), ""})
                constprop.SRK_Volume_Translation_Coefficient = GridProps.Rows(e.RowIndex).Cells(1).Value
            Case 23 '.Add(New Object() {"UNIQUAC R", Format(constprop.UNIQUAC_R, nf), ""})
                constprop.UNIQUAC_R = GridProps.Rows(e.RowIndex).Cells(1).Value
            Case 24 '.Add(New Object() {"UNIQUAC Q", Format(constprop.UNIQUAC_Q, nf), ""})
                constprop.UNIQUAC_Q = GridProps.Rows(e.RowIndex).Cells(1).Value
            Case 25 '.Add(New Object() {DWSIM.App.GetLocalString("Charge"), Format(constprop.Charge, "+#;-#"), ""})
                constprop.Charge = GridProps.Rows(e.RowIndex).Cells(1).Value
            Case 26 '.Add(New Object() {DWSIM.App.GetLocalString("HydrationNumber"), constprop.HydrationNumber, ""})
                constprop.HydrationNumber = GridProps.Rows(e.RowIndex).Cells(1).Value
            Case 27 '.Add(New Object() {DWSIM.App.GetLocalString("PositiveIon"), constprop.PositiveIon, ""})
                constprop.PositiveIon = GridProps.Rows(e.RowIndex).Cells(1).Value
            Case 28 '.Add(New Object() {DWSIM.App.GetLocalString("NegativeIon"), constprop.NegativeIon, ""})
                constprop.NegativeIon = GridProps.Rows(e.RowIndex).Cells(1).Value
            Case 13 '.Add(New Object() {DWSIM.App.GetLocalString("TemperatureOfFusion"), Format(cv.ConverterDoSI(su.spmp_temperature, constprop.TemperatureOfFusion), nf), su.spmp_temperature})
                constprop.TemperatureOfFusion = cv.ConverterParaSI(su.spmp_temperature, GridProps.Rows(e.RowIndex).Cells(1).Value)
            Case 14 '.Add(New Object() {DWSIM.App.GetLocalString("EnthalpyOfFusionAtTf"), Format(constprop.EnthalpyOfFusionAtTf, nf), "kJ/mol"})
                constprop.EnthalpyOfFusionAtTf = GridProps.Rows(e.RowIndex).Cells(1).Value
            Case 15 '.Add(New Object() {DWSIM.App.GetLocalString("TemperatureOfSolidDensity_Ts"), Format(cv.ConverterDoSI(su.spmp_temperature, constprop.SolidTs), nf), su.spmp_temperature})
                constprop.SolidTs = cv.ConverterParaSI(su.spmp_temperature, GridProps.Rows(e.RowIndex).Cells(1).Value)
            Case 16 '.Add(New Object() {DWSIM.App.GetLocalString("SolidDensityAtTs"), Format(cv.ConverterDoSI(su.spmp_density, constprop.SolidDensityAtTs), nf), su.spmp_density})
                constprop.SolidDensityAtTs = cv.ConverterParaSI(su.spmp_density, GridProps.Rows(e.RowIndex).Cells(1).Value)
        End Select

        For Each mat As DWSIM.SimulationObjects.Streams.MaterialStream In Me.ChildParent.Collections.CLCS_MaterialStreamCollection.Values
            For Each p As DWSIM.ClassesBasicasTermodinamica.Fase In mat.Fases.Values
                For Each subst As DWSIM.ClassesBasicasTermodinamica.Substancia In p.Componentes.Values
                    If subst.ConstantProperties.Name = constprop.Name Then
                        subst.ConstantProperties = constprop
                        Exit For
                    End If
                Next
            Next
        Next

    End Sub

    Private Sub chkEnableEdit_CheckedChanged(ByVal sender As System.Object, ByVal e As System.EventArgs) Handles chkEnableEdit.CheckedChanged
        If chkEnableEdit.Checked Then
            Me.ComboBox1.Enabled = False
            Me.Button1.Enabled = True
            constprop_orig = constprop.Clone
            Colorize()
        Else
            Me.ComboBox1.Enabled = True
            Me.Button1.Enabled = False
            constprop_orig = Nothing
            For Each r As DataGridViewRow In GridProps.Rows
                r.Cells(1).ReadOnly = True
                r.Cells(1).Style.ForeColor = r.DefaultCellStyle.ForeColor
            Next
        End If
    End Sub

    Private Sub Button1_Click(ByVal sender As System.Object, ByVal e As System.EventArgs) Handles Button1.Click

        constprop = constprop_orig.Clone

        For Each mat As DWSIM.SimulationObjects.Streams.MaterialStream In Me.ChildParent.Collections.CLCS_MaterialStreamCollection.Values
            For Each p As DWSIM.ClassesBasicasTermodinamica.Fase In mat.Fases.Values
                For Each subst As DWSIM.ClassesBasicasTermodinamica.Substancia In p.Componentes.Values
                    subst.ConstantProperties = constprop
                Next
            Next
        Next

        Populate()

        Colorize()

    End Sub

    Sub Colorize()

        For Each r As DataGridViewRow In GridProps.Rows
            If r.Index >= 4 Then
                r.Cells(1).ReadOnly = False
                r.Cells(1).Style.ForeColor = Color.Red
            Else
                r.Cells(1).ReadOnly = True
                r.Cells(1).Style.ForeColor = Color.DarkGray
            End If
        Next

    End Sub
End Class