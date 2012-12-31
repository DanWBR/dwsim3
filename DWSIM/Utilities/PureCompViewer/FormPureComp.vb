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
        Me.ComboBox1.SelectedIndex = 0

    End Sub

    Sub Populate()

        Dim su As DWSIM.SistemasDeUnidades.Unidades = ChildParent.Options.SelectedUnitSystem
        Dim cv As New DWSIM.SistemasDeUnidades.Conversor
        Dim nf As String = ChildParent.Options.NumberFormat
        Dim pp As DWSIM.SimulationObjects.PropertyPackages.PropertyPackage = ChildParent.Options.SelectedPropertyPackage

        For Each mat As DWSIM.SimulationObjects.Streams.MaterialStream In Me.ChildParent.Collections.CLCS_MaterialStreamCollection.Values
            pp.CurrentMaterialStream = mat
            Exit For
        Next

        'setting up curves
        Dim T As Double
        Dim Tmin, Tmax, delta As Double

        Tmin = 200
        Tmax = 1500
        delta = (Tmax - Tmin) / 50

        T = Tmin
        vxCp.Clear()
        vyCp.Clear()

        If Not constprop.IsIon Or Not constprop.IsSalt Then
            Do
                vxCp.Add(cv.ConverterDoSI(su.spmp_temperature, T))
                vyCp.Add(cv.ConverterDoSI(su.spmp_heatCapacityCp, pp.AUX_CPi(constprop.Name, T)))
                T += delta
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
        vxPvap.Clear()
        vyPvap.Clear()
        If Not constprop.IsIon And Not constprop.IsSalt Then
            Do
                vxPvap.Add(cv.ConverterDoSI(su.spmp_temperature, T))
                vyPvap.Add(cv.ConverterDoSI(su.spmp_pressure, pp.AUX_PVAPi(constprop.Name, T)))
                T += delta
            Loop Until T > Tmax
        End If

        'viscosidade liquido
        With constprop
            Tmin = 0.6 * .Critical_Temperature
            Tmax = .Critical_Temperature
            delta = (Tmax - Tmin) / 50
        End With
        T = Tmin
        vxVisc.Clear()
        vyVisc.Clear()
        If Not constprop.IsIon And Not constprop.IsSalt Then
            Do
                vxVisc.Add(cv.ConverterDoSI(su.spmp_temperature, T))
                vyVisc.Add(cv.ConverterDoSI(su.spmp_viscosity, pp.AUX_LIQVISCi(constprop.Name, T)))
                T += delta
            Loop Until T > Tmax
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
        vxDHvap.Clear()
        vyDHvap.Clear()
        Dim Tr As Double
        If Not constprop.IsIon And Not constprop.IsSalt Then
            Do
                Tr = T / constprop.Critical_Temperature
                vxDHvap.Add(cv.ConverterDoSI(su.spmp_temperature, T))
                vyDHvap.Add(cv.ConverterDoSI(su.spmp_enthalpy, pp.AUX_HVAPi(constprop.Name, T)))
                T += delta
            Loop Until T > Tmax
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

        'Grid UNIFAC
        With Me.GridUNIFAC.Rows
            .Clear()
            If Not constprop.UNIFACGroups Is Nothing Then
                For Each s As String In constprop.UNIFACGroups.Collection.Keys
                    .Add(New Object() {s, constprop.UNIFACGroups.Collection(s)})
                Next
            End If
        End With

        'Grid MODFAC
        With Me.GridMODFAC.Rows
            .Clear()
            If Not constprop.MODFACGroups Is Nothing Then
                For Each s As String In constprop.MODFACGroups.Collection.Keys
                    .Add(New Object() {s, constprop.MODFACGroups.Collection(s)})
                Next
            End If

        End With

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
            .Add(New Object() {DWSIM.App.GetLocalString("ChaoSeaderAcentricFactor"), Format(constprop.Chao_Seader_Acentricity, nf), "-"})
            .Add(New Object() {DWSIM.App.GetLocalString("ChaoSeaderSolubilityParameter"), Format(constprop.Chao_Seader_Solubility_Parameter, nf), "(cal/mL)^0.5"})
            .Add(New Object() {DWSIM.App.GetLocalString("ChaoSeaderLiquidMolarVolume"), Format(constprop.Chao_Seader_Liquid_Molar_Volume, nf), "mL/mol"})
            .Add(New Object() {DWSIM.App.GetLocalString("RackettCompressibility"), Format(constprop.Z_Rackett, nf), ""})
            .Add(New Object() {DWSIM.App.GetLocalString("PengRobinsonVolumeTranslationCoefficient"), Format(constprop.PR_Volume_Translation_Coefficient, nf), ""})
            .Add(New Object() {DWSIM.App.GetLocalString("SRKVolumeTranslationCoefficient"), Format(constprop.SRK_Volume_Translation_Coefficient, nf), ""})
            .Add(New Object() {"UNIQUAC R", Format(constprop.UNIQUAC_R, nf), ""})
            .Add(New Object() {"UNIQUAC Q", Format(constprop.UNIQUAC_Q, nf), ""})
            If constprop.IsIon Then
                .Add(New Object() {"Charge", Format(constprop.Charge, "+#;-#"), ""})
            End If
            If constprop.IsSalt Then
                .Add(New Object() {"HydrationNumber", constprop.HydrationNumber, ""})
                .Add(New Object() {"PositiveIon", constprop.PositiveIon, ""})
                .Add(New Object() {"NegativeIon", constprop.NegativeIon, ""})
                .Add(New Object() {"TemperatureOfFusion", Format(cv.ConverterDoSI(su.spmp_temperature, constprop.TemperatureOfFusion), nf), su.spmp_temperature})
                .Add(New Object() {"EnthalpyOfFusionAtTf", Format(constprop.EnthalpyOfFusionAtTf, nf), "kJ/mol"})
                .Add(New Object() {"TemperatureOfSolidDensity_Ts", Format(cv.ConverterDoSI(su.spmp_temperature, constprop.SolidTs), nf), su.spmp_temperature})
                .Add(New Object() {"SolidDensityAtTs", Format(cv.ConverterDoSI(su.spmp_density, constprop.SolidDensityAtTs), nf), su.spmp_density})
            End If

        End With
    End Sub

    Private Sub ComboBox1_SelectedIndexChanged(ByVal sender As Object, ByVal e As System.EventArgs) Handles ComboBox1.SelectedIndexChanged
        If OnlyViewing = True Or constprop Is Nothing Then
            Dim name As String = ComboBox1.SelectedItem.ToString.Substring(ComboBox1.SelectedItem.ToString.IndexOf("[") + 1, ComboBox1.SelectedItem.ToString.Length - ComboBox1.SelectedItem.ToString.IndexOf("[") - 2)
            constprop = CType(Me.ChildParent.Options.SelectedComponents(name), DWSIM.ClassesBasicasTermodinamica.ConstantProperties)
        End If
        Me.GridUNIFAC.Enabled = True
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
            Case 13 '.Add(New Object() {DWSIM.App.GetLocalString("ChaoSeaderAcentricFactor"), Format(constprop.Chao_Seader_Acentricity, nf), "-"})
                constprop.Chao_Seader_Acentricity = GridProps.Rows(e.RowIndex).Cells(1).Value
            Case 14 '.Add(New Object() {DWSIM.App.GetLocalString("ChaoSeaderSolubilityParameter"), Format(constprop.Chao_Seader_Solubility_Parameter, nf), "(cal/mL)^0.5"})
                constprop.Chao_Seader_Solubility_Parameter = GridProps.Rows(e.RowIndex).Cells(1).Value
            Case 15 '.Add(New Object() {DWSIM.App.GetLocalString("ChaoSeaderLiquidMolarVolume"), Format(constprop.Chao_Seader_Liquid_Molar_Volume, nf), "mL/mol"})
                constprop.Chao_Seader_Liquid_Molar_Volume = GridProps.Rows(e.RowIndex).Cells(1).Value
            Case 16 '.Add(New Object() {DWSIM.App.GetLocalString("RackettCompressibility"), Format(constprop.Z_Rackett, nf), ""})
                constprop.Z_Rackett = GridProps.Rows(e.RowIndex).Cells(1).Value
            Case 17 '.Add(New Object() {DWSIM.App.GetLocalString("PengRobinsonVolumeTranslationCoefficient"), Format(constprop.PR_Volume_Translation_Coefficient, nf), ""})
                constprop.PR_Volume_Translation_Coefficient = GridProps.Rows(e.RowIndex).Cells(1).Value
            Case 18 '.Add(New Object() {DWSIM.App.GetLocalString("SRKVolumeTranslationCoefficient"), Format(constprop.SRK_Volume_Translation_Coefficient, nf), ""})
                constprop.SRK_Volume_Translation_Coefficient = GridProps.Rows(e.RowIndex).Cells(1).Value
            Case 19 '.Add(New Object() {"UNIQUAC R", Format(constprop.UNIQUAC_R, nf), ""})
                constprop.UNIQUAC_R = GridProps.Rows(e.RowIndex).Cells(1).Value
            Case 20 '.Add(New Object() {"UNIQUAC Q", Format(constprop.UNIQUAC_Q, nf), ""})
                constprop.UNIQUAC_Q = GridProps.Rows(e.RowIndex).Cells(1).Value
        End Select

        For Each mat As DWSIM.SimulationObjects.Streams.MaterialStream In Me.ChildParent.Collections.CLCS_MaterialStreamCollection.Values
            For Each p As DWSIM.ClassesBasicasTermodinamica.Fase In mat.Fases.Values
                For Each subst As DWSIM.ClassesBasicasTermodinamica.Substancia In p.Componentes.Values
                    subst.ConstantProperties = constprop
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