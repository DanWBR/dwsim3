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

Imports DWSIM.DWSIM.SimulationObjects.PropertyPackages.Auxiliary
Imports DWSIM.DWSIM.Utilities.Hypos.Methods
Imports DWSIM.DWSIM.MathEx.Common
Imports System.IO
Imports System.Runtime.Serialization.Formatters.Binary
Imports System.Math

Public Class FormCompoundCreator

    Public su As New DWSIM.SistemasDeUnidades.Unidades
    Public cv As New DWSIM.SistemasDeUnidades.Conversor
    Public nf As String

    Public methods As DWSIM.Utilities.Hypos.Methods.HYP
    Friend methods2 As DWSIM.SimulationObjects.PropertyPackages.Auxiliary.PROPS
    Public jb As DWSIM.Utilities.Hypos.Methods.Joback
    Friend m_props As PROPS

    Friend mycase As New CompoundGeneratorCase

    Friend loaded As Boolean = False
    Private forceclose As Boolean = False
    Private populating As Boolean = False

    Private Sub FormCompoundCreator_Load(ByVal sender As System.Object, ByVal e As System.EventArgs) Handles MyBase.Load

        jb = New DWSIM.Utilities.Hypos.Methods.Joback
        methods2 = New DWSIM.SimulationObjects.PropertyPackages.Auxiliary.PROPS

        methods = New DWSIM.Utilities.Hypos.Methods.HYP()

        'Grid UNIFAC
        Dim pathsep = System.IO.Path.DirectorySeparatorChar
        Dim picpath As String = My.Application.Info.DirectoryPath & pathsep & "data" & pathsep & "unifac" & pathsep
        Dim filename As String = My.Application.Info.DirectoryPath & pathsep & "data" & pathsep & "unifac.txt"
        Dim lines() As String = IO.File.ReadAllLines(filename)
        Dim i As Integer
        With Me.GridUNIFAC.Rows
            .Clear()
            For i = 2 To lines.Length - 1
                .Add(New Object() {CInt(0), Image.FromFile(picpath & lines(i).Split(",")(7) & ".png")})
                .Item(.Count - 1).HeaderCell.Value = lines(i).Split(",")(3)
                .Item(.Count - 1).HeaderCell.Tag = lines(i).Split(",")(2)
                .Item(.Count - 1).Cells(1).ToolTipText = "Main Group: " & lines(i).Split(",")(2) & vbCrLf & _
                                                        "Subgroup: " & lines(i).Split(",")(3) & vbCrLf & _
                                                        "Rk / Qk: " & lines(i).Split(",")(4) & " / " & lines(i).Split(",")(5) & vbCrLf & _
                                                        "Example Compound: " & lines(i).Split(",")(6)
            Next
        End With

        mycase = New CompoundGeneratorCase
        With mycase
            .cp.VaporPressureEquation = 0
            .cp.IdealgasCpEquation = 0
            .cp.LiquidDensityEquation = 0
            .cp.LiquidViscosityEquation = 0
        End With

        cbEqPVAP.SelectedIndex = 0
        cbEqCPIG.SelectedIndex = 0
        cbEqLIQDENS.SelectedIndex = 0
        cbEqLIQVISC.SelectedIndex = 0

        Me.cbUnits.Items.Clear()

        For Each su In CType(Me.MdiParent, FormMain).AvailableUnitSystems.Values
            Me.cbUnits.Items.Add(su.nome)
        Next

        Me.cbUnits.SelectedIndex = 0

        Me.TextBoxID.Text = New Random().Next(100000)

    End Sub

   

    Private Sub FormCompoundCreator_FormClosing(ByVal sender As Object, ByVal e As System.Windows.Forms.FormClosingEventArgs) Handles Me.FormClosing

        If Not forceclose Then
            Dim x = MessageBox.Show(DWSIM.App.GetLocalString("Desejasalvarasaltera"), DWSIM.App.GetLocalString("Fechando") & " " & Me.Text, MessageBoxButtons.YesNoCancel, MessageBoxIcon.Question)

            If x = MsgBoxResult.Yes Then

                FormMain.SaveFileDialog()
                forceclose = True
                Me.Close()

            ElseIf x = MsgBoxResult.Cancel Then

                e.Cancel = True

            Else

                forceclose = True
                Me.Close()

            End If
        End If


    End Sub

    Sub WriteData()

        Dim i As Integer

        With mycase
            If File.Exists(.database) Then
                Me.tbDBPath.Text = .database
            Else
                Me.tbDBPath.Text = "<file not found>"
            End If
            Me.Text = .Filename
            If .su.spmp_temperature IsNot Nothing Then
                Me.su = .su
                If Not CType(Me.MdiParent, FormMain).AvailableUnitSystems.ContainsKey(.su.nome) Then
                    If Not TypeOf .su Is DWSIM.SistemasDeUnidades.UnidadesSI And Not _
                        TypeOf .su Is DWSIM.SistemasDeUnidades.UnidadesCGS And Not _
                        TypeOf .su Is DWSIM.SistemasDeUnidades.UnidadesINGLES Then
                        CType(Me.MdiParent, FormMain).AvailableUnitSystems.Add(.su.nome, .su)
                    End If
                    Me.cbUnits.Items.Add(mycase.su.nome)
                Else
                    UpdateUnits()
                End If
            Else
                .su = Me.su
            End If
            tbDBPath.Text = .database
            TextBoxAF.Text = .cp.Acentric_Factor
            TextBoxCAS.Text = .cp.CAS_Number
            TextBoxCSAF.Text = .cp.Chao_Seader_Acentricity
            TextBoxCSLV.Text = .cp.Chao_Seader_Liquid_Molar_Volume
            TextBoxCSSP.Text = .cp.Chao_Seader_Solubility_Parameter
            TextBoxDGF.Text = cv.ConverterDoSI(su.spmp_enthalpy, .cp.IG_Gibbs_Energy_of_Formation_25C)
            TextBoxDHF.Text = cv.ConverterDoSI(su.spmp_enthalpy, .cp.IG_Enthalpy_of_Formation_25C)
            TextBoxFormula.Text = .cp.Formula
            TextBoxID.Text = .cp.ID
            TextBoxMW.Text = .cp.Molar_Weight
            TextBoxName.Text = .cp.Name
            TextBoxNBP.Text = cv.ConverterDoSI(su.spmp_temperature, .cp.Normal_Boiling_Point)
            TextBoxPc.Text = cv.ConverterDoSI(su.spmp_pressure, .cp.Critical_Pressure)
            TextBoxPCSAFTEpsilon.Text = .cp.PC_SAFT_epsilon_k
            TextBoxPCSAFTm.Text = .cp.PC_SAFT_m
            TextBoxPCSAFTSigma.Text = .cp.PC_SAFT_sigma
            TextBoxUNIQUAC_Q.Text = .cp.UNIQUAC_Q
            TextBoxUNIQUAC_R.Text = .cp.UNIQUAC_R
            TextBoxTc.Text = cv.ConverterDoSI(su.spmp_temperature, .cp.Critical_Temperature)
            TextBoxVTCPR.Text = .cp.PR_Volume_Translation_Coefficient
            TextBoxVTCSRK.Text = .cp.SRK_Volume_Translation_Coefficient
            TextBoxZc.Text = .cp.Critical_Compressibility
            TextBoxZRa.Text = .cp.Z_Rackett

            If .RegressPVAP Then rbEstimatePVAP.Checked = True
            If .RegressCPIG Then rbRegressCPIG.Checked = True
            If .RegressLDENS Then rbRegressLIQDENS.Checked = True
            If .RegressLVISC Then rbRegressLIQVISC.Checked = True
            If .EqPVAP Then rbCoeffPVAP.Checked = True
            If .EqCPIG Then rbCoeffCPIG.Checked = True
            If .EqLDENS Then rbCoeffLIQDENS.Checked = True
            If .EqLVISC Then rbCoeffLIQVISC.Checked = True

            CheckBoxMW.Checked = .CalcMW
            CheckBoxNBP.Checked = .CalcNBP
            CheckBoxAF.Checked = .CalcAF
            CheckBoxCSAF.Checked = .CalcCSAF
            CheckBoxCSLV.Checked = .CalcCSMV
            CheckBoxCSSP.Checked = .CalcCSSP
            CheckBoxTc.Checked = .CalcTC
            CheckBoxPc.Checked = .CalcPC
            CheckBoxZc.Checked = .CalcZC
            CheckBoxZRa.Checked = .CalcZRA
            CheckBoxDHF.Checked = .CalcHF
            CheckBoxDGF.Checked = .CalcGF

            For Each it As Object In cbEqPVAP.Items
                If it.ToString.Split(":")(0) = .cp.VaporPressureEquation Then
                    cbEqPVAP.SelectedIndex = cbEqPVAP.Items.IndexOf(it)
                    Exit For
                End If
            Next

            For Each it As Object In cbEqCPIG.Items
                If it.ToString.Split(":")(0) = .cp.IdealgasCpEquation Then
                    cbEqCPIG.SelectedIndex = cbEqCPIG.Items.IndexOf(it)
                    Exit For
                End If
            Next

            For Each it As Object In cbEqLIQDENS.Items
                If it.ToString.Split(":")(0) = .cp.LiquidDensityEquation Then
                    cbEqLIQDENS.SelectedIndex = cbEqLIQDENS.Items.IndexOf(it)
                    Exit For
                End If
            Next

            For Each it As Object In cbEqLIQVISC.Items
                If it.ToString.Split(":")(0) = .cp.LiquidViscosityEquation Then
                    cbEqLIQVISC.SelectedIndex = cbEqLIQVISC.Items.IndexOf(it)
                    Exit For
                End If
            Next

            tbPVAP_A.Text = .cp.Vapor_Pressure_Constant_A
            tbPVAP_B.Text = .cp.Vapor_Pressure_Constant_B
            tbPVAP_C.Text = .cp.Vapor_Pressure_Constant_C
            tbPVAP_D.Text = .cp.Vapor_Pressure_Constant_D
            tbPVAP_E.Text = .cp.Vapor_Pressure_Constant_E

            tbCPIG_A.Text = .cp.Ideal_Gas_Heat_Capacity_Const_A
            tbCPIG_B.Text = .cp.Ideal_Gas_Heat_Capacity_Const_B
            tbCPIG_C.Text = .cp.Ideal_Gas_Heat_Capacity_Const_C
            tbCPIG_D.Text = .cp.Ideal_Gas_Heat_Capacity_Const_D
            tbCPIG_E.Text = .cp.Ideal_Gas_Heat_Capacity_Const_E

            tbLIQDENS_A.Text = .cp.Liquid_Density_Const_A
            tbLIQDENS_B.Text = .cp.Liquid_Density_Const_B
            tbLIQDENS_C.Text = .cp.Liquid_Density_Const_C
            tbLIQDENS_D.Text = .cp.Liquid_Density_Const_D
            tbLIQDENS_E.Text = .cp.Liquid_Density_Const_E

            tbLIQVISC_A.Text = .cp.Liquid_Viscosity_Const_A
            tbLIQVISC_B.Text = .cp.Liquid_Viscosity_Const_B
            tbLIQVISC_C.Text = .cp.Liquid_Viscosity_Const_C
            tbLIQVISC_D.Text = .cp.Liquid_Viscosity_Const_D
            tbLIQVISC_E.Text = .cp.Liquid_Viscosity_Const_E

            populating = True
            For Each r As DataGridViewRow In Me.GridUNIFAC.Rows
                r.Cells(0).Value = .cp.UNIFACGroups.Collection(r.HeaderCell.Value)
            Next
            populating = False
            Me.GridExpDataPVAP.Rows.Clear()
            For i = 0 To .DataPVAP.Count - 1
                Me.GridExpDataPVAP.Rows.Add(New Object() {cv.ConverterDoSI(su.spmp_temperature, .DataPVAP(i)(0)), cv.ConverterDoSI(su.spmp_pressure, .DataPVAP(i)(1))})
            Next
            Me.GridExpDataCPIG.Rows.Clear()
            For i = 0 To .DataCPIG.Count - 1
                Me.GridExpDataCPIG.Rows.Add(New Object() {cv.ConverterDoSI(su.spmp_temperature, .DataCPIG(i)(0)), cv.ConverterDoSI(su.spmp_heatCapacityCp, .DataCPIG(i)(1))})
            Next
            Me.GridExpDataLIQDENS.Rows.Clear()
            For i = 0 To .DataLDENS.Count - 1
                Me.GridExpDataLIQDENS.Rows.Add(New Object() {cv.ConverterDoSI(su.spmp_temperature, .DataLDENS(i)(0)), cv.ConverterDoSI(su.spmp_density, .DataLDENS(i)(1))})
            Next
            Me.GridExpDataLIQVISC.Rows.Clear()
            For i = 0 To .DataLVISC.Count - 1
                Me.GridExpDataLIQVISC.Rows.Add(New Object() {cv.ConverterDoSI(su.spmp_temperature, .DataLVISC(i)(0)), cv.ConverterDoSI(su.spmp_viscosity, .DataLVISC(i)(1))})
            Next
            If .RegressOKPVAP Then tbStatusPVAP.Text = "OK" Else tbStatusPVAP.Text = .ErrorMsgPVAP
            If .RegressOKCPIG Then tbStatusCPIG.Text = "OK" Else tbStatusCPIG.Text = .ErrorMsgCPIG
            If .RegressOKLDENS Then tbStatusLIQDENS.Text = "OK" Else tbStatusLIQDENS.Text = .ErrorMsgLDENS
            If .RegressOKLVISC Then tbStatusLIQVISC.Text = "OK" Else tbStatusLIQVISC.Text = .ErrorMsgLVISC
        End With

    End Sub

    Sub StoreData()

        With mycase
            .su = Me.su
            .database = tbDBPath.Text
            .cp.Acentric_Factor = TextBoxAF.Text
            .cp.CAS_Number = TextBoxCAS.Text
            .cp.Chao_Seader_Acentricity = TextBoxCSAF.Text
            .cp.Chao_Seader_Liquid_Molar_Volume = TextBoxCSLV.Text
            .cp.Chao_Seader_Solubility_Parameter = TextBoxCSSP.Text
            .cp.IG_Gibbs_Energy_of_Formation_25C = cv.ConverterParaSI(su.spmp_enthalpy, TextBoxDGF.Text)
            .cp.IG_Enthalpy_of_Formation_25C = cv.ConverterParaSI(su.spmp_enthalpy, TextBoxDHF.Text)
            .cp.Formula = TextBoxFormula.Text
            .cp.ID = TextBoxID.Text
            .cp.Molar_Weight = TextBoxMW.Text
            .cp.Name = TextBoxName.Text
            .cp.Normal_Boiling_Point = cv.ConverterParaSI(su.spmp_temperature, TextBoxNBP.Text)
            .cp.Critical_Pressure = cv.ConverterParaSI(su.spmp_pressure, TextBoxPc.Text)
            .cp.PC_SAFT_epsilon_k = TextBoxPCSAFTEpsilon.Text
            .cp.PC_SAFT_m = TextBoxPCSAFTm.Text
            .cp.PC_SAFT_sigma = TextBoxPCSAFTSigma.Text
            .cp.UNIQUAC_Q = TextBoxUNIQUAC_Q.Text
            .cp.UNIQUAC_R = TextBoxUNIQUAC_R.Text
            .cp.Critical_Temperature = cv.ConverterParaSI(su.spmp_temperature, TextBoxTc.Text)
            .cp.PR_Volume_Translation_Coefficient = TextBoxVTCPR.Text
            .cp.SRK_Volume_Translation_Coefficient = TextBoxVTCSRK.Text
            .cp.Critical_Compressibility = TextBoxZc.Text
            .cp.Z_Rackett = TextBoxZRa.Text

            .RegressPVAP = rbEstimatePVAP.Checked
            .RegressCPIG = rbRegressCPIG.Checked
            .RegressLDENS = rbRegressLIQDENS.Checked
            .RegressLVISC = rbRegressLIQVISC.Checked
            .EqPVAP = rbCoeffPVAP.Checked
            .EqCPIG = rbCoeffCPIG.Checked
            .EqLDENS = rbCoeffLIQDENS.Checked
            .EqLVISC = rbCoeffLIQVISC.Checked

            .CalcMW = CheckBoxMW.Checked
            .CalcNBP = CheckBoxNBP.Checked
            .CalcAF = CheckBoxAF.Checked
            .CalcCSAF = CheckBoxCSAF.Checked
            .CalcCSMV = CheckBoxCSLV.Checked
            .CalcCSSP = CheckBoxCSSP.Checked
            .CalcTC = CheckBoxTc.Checked
            .CalcPC = CheckBoxPc.Checked
            .CalcZC = CheckBoxZc.Checked
            .CalcZRA = CheckBoxZRa.Checked
            .CalcHF = CheckBoxDHF.Checked
            .CalcGF = CheckBoxDGF.Checked

            .cp.VaporPressureEquation = cbEqPVAP.SelectedItem.ToString.Split(":")(0)
            .cp.LiquidDensityEquation = cbEqLIQDENS.SelectedItem.ToString.Split(":")(0)
            .cp.LiquidViscosityEquation = cbEqLIQVISC.SelectedItem.ToString.Split(":")(0)

            .cp.Vapor_Pressure_Constant_A = tbPVAP_A.Text
            .cp.Vapor_Pressure_Constant_B = tbPVAP_B.Text
            .cp.Vapor_Pressure_Constant_C = tbPVAP_C.Text
            .cp.Vapor_Pressure_Constant_D = tbPVAP_D.Text
            .cp.Vapor_Pressure_Constant_E = tbPVAP_E.Text

            If rbEstimateCPIG.Checked Then
                Dim result As Object = RegressData(1, True)
                .cp.IdealgasCpEquation = 5
                .cp.Ideal_Gas_Heat_Capacity_Const_A = result(0)(0) * 1000
                .cp.Ideal_Gas_Heat_Capacity_Const_B = result(0)(1) * 1000
                .cp.Ideal_Gas_Heat_Capacity_Const_C = result(0)(2) * 1000
                .cp.Ideal_Gas_Heat_Capacity_Const_D = result(0)(3) * 1000
                .cp.Ideal_Gas_Heat_Capacity_Const_E = result(0)(4) * 1000
            Else
                .cp.IdealgasCpEquation = cbEqCPIG.SelectedItem.ToString.Split(":")(0)
                .cp.Ideal_Gas_Heat_Capacity_Const_A = tbCPIG_A.Text
                .cp.Ideal_Gas_Heat_Capacity_Const_B = tbCPIG_B.Text
                .cp.Ideal_Gas_Heat_Capacity_Const_C = tbCPIG_C.Text
                .cp.Ideal_Gas_Heat_Capacity_Const_D = tbCPIG_D.Text
                .cp.Ideal_Gas_Heat_Capacity_Const_E = tbCPIG_E.Text
            End If

            .cp.Liquid_Density_Const_A = tbLIQDENS_A.Text
            .cp.Liquid_Density_Const_B = tbLIQDENS_B.Text
            .cp.Liquid_Density_Const_C = tbLIQDENS_C.Text
            .cp.Liquid_Density_Const_D = tbLIQDENS_D.Text
            .cp.Liquid_Density_Const_E = tbLIQDENS_E.Text

            .cp.Liquid_Viscosity_Const_A = tbLIQVISC_A.Text
            .cp.Liquid_Viscosity_Const_B = tbLIQVISC_B.Text
            .cp.Liquid_Viscosity_Const_C = tbLIQVISC_C.Text
            .cp.Liquid_Viscosity_Const_D = tbLIQVISC_D.Text
            .cp.Liquid_Viscosity_Const_E = tbLIQVISC_E.Text

            For Each r As DataGridViewRow In Me.GridUNIFAC.Rows
                If CInt(r.Cells(0).Value) <> 0 Then
                    .cp.UNIFACGroups.Collection(r.HeaderCell.Value) = r.Cells(0).Value
                    .cp.MODFACGroups.Collection(r.HeaderCell.Value) = r.Cells(0).Value
                Else
                    If .cp.UNIFACGroups.Collection.ContainsKey(r.HeaderCell.Value) Then
                        .cp.UNIFACGroups.Collection.Remove(r.HeaderCell.Value)
                    End If
                    If .cp.MODFACGroups.Collection.ContainsKey(r.HeaderCell.Value) Then
                        .cp.MODFACGroups.Collection.Remove(r.HeaderCell.Value)
                    End If
                End If
            Next

            mycase.DataPVAP.Clear()
            For Each row As DataGridViewRow In Me.GridExpDataPVAP.Rows
                If row.Index < Me.GridExpDataPVAP.Rows.Count - 1 Then mycase.DataPVAP.Add(New Double() {cv.ConverterParaSI(su.spmp_temperature, row.Cells(0).Value), cv.ConverterParaSI(su.spmp_pressure, row.Cells(1).Value)})
            Next

            mycase.DataCPIG.Clear()
            For Each row As DataGridViewRow In Me.GridExpDataCPIG.Rows
                If row.Index < Me.GridExpDataCPIG.Rows.Count - 1 Then mycase.DataCPIG.Add(New Double() {cv.ConverterParaSI(su.spmp_temperature, row.Cells(0).Value), cv.ConverterParaSI(su.spmp_heatCapacityCp, row.Cells(1).Value)})
            Next

            mycase.DataLDENS.Clear()
            For Each row As DataGridViewRow In Me.GridExpDataLIQDENS.Rows
                If row.Index < Me.GridExpDataLIQDENS.Rows.Count - 1 Then mycase.DataLDENS.Add(New Double() {cv.ConverterParaSI(su.spmp_temperature, row.Cells(0).Value), cv.ConverterParaSI(su.spmp_density, row.Cells(1).Value)})
            Next

            mycase.DataLVISC.Clear()
            For Each row As DataGridViewRow In Me.GridExpDataLIQVISC.Rows
                If row.Index < Me.GridExpDataLIQVISC.Rows.Count - 1 Then mycase.DataLVISC.Add(New Double() {cv.ConverterParaSI(su.spmp_temperature, row.Cells(0).Value), cv.ConverterParaSI(su.spmp_viscosity, row.Cells(1).Value)})
            Next

        End With


    End Sub

    Private Sub GridUNIFAC_CellValueChanged(ByVal sender As System.Object, ByVal e As System.Windows.Forms.DataGridViewCellEventArgs) Handles GridUNIFAC.CellValueChanged
        If loaded Then
            'get group amounts
            If Not populating Then
                mycase.cp.UNIFACGroups.Collection.Clear()
                mycase.cp.MODFACGroups.Collection.Clear()
            End If
            For Each r As DataGridViewRow In Me.GridUNIFAC.Rows
                If Not r.Cells(0).Value Is Nothing Then
                    If CInt(r.Cells(0).Value) <> 0 Then
                        If Not populating Then
                            mycase.cp.UNIFACGroups.Collection.Add(r.HeaderCell.Value, r.Cells(0).Value)
                            mycase.cp.MODFACGroups.Collection.Add(r.HeaderCell.Value, r.Cells(0).Value)
                        End If
                    End If
                End If

                'get group amounts
                Dim vn As New ArrayList
                For Each row As DataGridViewRow In Me.GridUNIFAC.Rows
                    If Not row.Cells(0).Value Is Nothing Then
                        vn.Add(Integer.Parse(row.Cells(0).Value))
                    Else
                        vn.Add(0)
                    End If
                Next
                Dim vnd As Int32() = vn.ToArray(Type.GetType("System.Int32"))

                Dim Tb, Tc, Pc, Vc, MM, w, Hvb As Double

                Tb = jb.CalcTb(vnd)
                If CheckBoxNBP.Checked Then Me.TextBoxNBP.Text = cv.ConverterDoSI(su.spmp_temperature, Tb)
                Tb = cv.ConverterParaSI(su.spmp_temperature, Me.TextBoxNBP.Text)

                MM = jb.CalcMW(vnd)
                If CheckBoxMW.Checked Then Me.TextBoxMW.Text = MM
                MM = Me.TextBoxMW.Text

                Tc = jb.CalcTc(Tb, vnd)
                If CheckBoxTc.Checked Then Me.TextBoxTc.Text = cv.ConverterDoSI(su.spmp_temperature, Tc)
                Tc = cv.ConverterParaSI(su.spmp_temperature, Me.TextBoxTc.Text)

                Pc = jb.CalcPc(vnd)
                If CheckBoxPc.Checked Then Me.TextBoxPc.Text = cv.ConverterDoSI(su.spmp_pressure, Pc)
                Pc = cv.ConverterParaSI(su.spmp_pressure, Me.TextBoxPc.Text)

                Vc = jb.CalcVc(vnd)
                If CheckBoxZc.Checked Then Me.TextBoxZc.Text = Pc * Vc / Tc / 8.314 / 1000
                If CheckBoxZRa.Checked Then Me.TextBoxZRa.Text = Pc * Vc / Tc / 8.314 / 1000

                w = (-Math.Log(Pc / 100000) - 5.92714 + 6.09648 / (Tb / Tc) + 1.28862 * Math.Log(Tb / Tc) - 0.169347 * (Tb / Tc) ^ 6) / (15.2518 - 15.6875 / (Tb / Tc) - 13.4721 * Math.Log(Tb / Tc) + 0.43577 * (Tb / Tc) ^ 6)

                If CheckBoxAF.Checked Then Me.TextBoxAF.Text = w
                w = Me.TextBoxAF.Text
                If CheckBoxDHF.Checked Then Me.TextBoxDHF.Text = cv.ConverterDoSI(su.spmp_enthalpy, jb.CalcDHf(vnd) / MM)
                Hvb = methods.DHvb_Vetere(Tc, Pc, Tb) / MM
                If CheckBoxDGF.Checked Then Me.TextBoxDGF.Text = cv.ConverterDoSI(su.spmp_enthalpy, jb.CalcDGf(vnd) / MM)
                If CheckBoxCSAF.Checked Then Me.TextBoxCSAF.Text = w
                If CheckBoxCSSP.Checked Then Me.TextBoxCSSP.Text = ((Hvb * MM - 8.314 * Tb) * 238.846 * methods2.liq_dens_rackett(Tb, Tc, Pc, w, MM) / MM / 1000000.0) ^ 0.5
                If CheckBoxCSLV.Checked Then Me.TextBoxCSLV.Text = 1 / methods2.liq_dens_rackett(Tb, Tc, Pc, w, MM) * MM / 1000 * 1000000.0

                If rbEstimateCPIG.Checked Then
                    Dim result As Object = RegressData(1, True)
                    With mycase.cp
                        .IdealgasCpEquation = 5
                        .Ideal_Gas_Heat_Capacity_Const_A = result(0)(0) * 1000
                        .Ideal_Gas_Heat_Capacity_Const_B = result(0)(1) * 1000
                        .Ideal_Gas_Heat_Capacity_Const_C = result(0)(2) * 1000
                        .Ideal_Gas_Heat_Capacity_Const_D = result(0)(3) * 1000
                        .Ideal_Gas_Heat_Capacity_Const_E = result(0)(4) * 1000
                    End With
                End If

            Next
        End If
    End Sub

    'Private Sub TextBoxTc_TextChanged(ByVal sender As System.Object, ByVal e As System.EventArgs) Handles TextBoxTc.TextChanged
    '    If Double.TryParse(TextBoxTc.Text, New Double()) Then
    '        mycase.cp.Critical_Temperature = cv.ConverterParaSI(su.spmp_temperature, TextBoxTc.Text)
    '    End If
    'End Sub

    'Private Sub TextBoxPc_TextChanged(ByVal sender As System.Object, ByVal e As System.EventArgs) Handles TextBoxPc.TextChanged
    '    If Double.TryParse(TextBoxPc.Text, New Double()) Then
    '        mycase.cp.Critical_Pressure = cv.ConverterParaSI(su.spmp_pressure, TextBoxPc.Text)
    '    End If
    'End Sub

    'Private Sub TextBoxDHF_TextChanged(ByVal sender As System.Object, ByVal e As System.EventArgs) Handles TextBoxDHF.TextChanged
    '    If Double.TryParse(TextBoxDHF.Text, New Double()) Then
    '        mycase.cp.IG_Enthalpy_of_Formation_25C = cv.ConverterParaSI(su.spmp_enthalpy, TextBoxDHF.Text)
    '    End If
    'End Sub

    'Private Sub TextBoxDGF_TextChanged(ByVal sender As System.Object, ByVal e As System.EventArgs) Handles TextBoxDGF.TextChanged
    '    If Double.TryParse(TextBoxDGF.Text, New Double()) Then
    '        mycase.cp.IG_Gibbs_Energy_of_Formation_25C = cv.ConverterParaSI(su.spmp_enthalpy, TextBoxDGF.Text)
    '    End If
    'End Sub

    'Private Sub TextBoxNBP_TextChanged(ByVal sender As System.Object, ByVal e As System.EventArgs) Handles TextBoxNBP.TextChanged
    '    If Double.TryParse(TextBoxNBP.Text, New Double()) Then
    '        mycase.cp.Normal_Boiling_Point = cv.ConverterParaSI(su.spmp_temperature, TextBoxNBP.Text)
    '    End If
    'End Sub

    'Private Sub TextBoxZc_TextChanged(ByVal sender As System.Object, ByVal e As System.EventArgs) Handles TextBoxZc.TextChanged
    '    If Double.TryParse(TextBoxZc.Text, New Double()) Then
    '        mycase.cp.Critical_Compressibility = TextBoxZc.Text
    '    End If
    'End Sub

    'Private Sub TextBoxZRa_TextChanged(ByVal sender As System.Object, ByVal e As System.EventArgs) Handles TextBoxZRa.TextChanged
    '    If Double.TryParse(TextBoxZRa.Text, New Double()) Then
    '        mycase.cp.Z_Rackett = TextBoxZRa.Text
    '    End If
    'End Sub

    'Private Sub TextBoxAF_TextChanged(ByVal sender As System.Object, ByVal e As System.EventArgs) Handles TextBoxAF.TextChanged
    '    If Double.TryParse(TextBoxAF.Text, New Double()) Then
    '        mycase.cp.Acentric_Factor = TextBoxAF.Text
    '    End If
    'End Sub

    'Private Sub TextBoxMW_TextChanged(ByVal sender As System.Object, ByVal e As System.EventArgs) Handles TextBoxMW.TextChanged
    '    If Double.TryParse(TextBoxMW.Text, New Double()) Then
    '        mycase.cp.Molar_Weight = TextBoxMW.Text
    '    End If
    'End Sub

    'Private Sub TextBoxPCSAFTSigma_TextChanged(ByVal sender As System.Object, ByVal e As System.EventArgs) Handles TextBoxPCSAFTSigma.TextChanged
    '    If Double.TryParse(TextBoxPCSAFTSigma.Text, New Double()) Then
    '        mycase.cp.PC_SAFT_sigma = TextBoxPCSAFTSigma.Text
    '    End If
    'End Sub

    'Private Sub TextBoxPCSAFTm_TextChanged(ByVal sender As System.Object, ByVal e As System.EventArgs) Handles TextBoxPCSAFTm.TextChanged
    '    If Double.TryParse(TextBoxPCSAFTm.Text, New Double()) Then
    '        mycase.cp.PC_SAFT_m = TextBoxPCSAFTm.Text
    '    End If
    'End Sub

    'Private Sub TextBoxPCSAFTEpsilon_TextChanged(ByVal sender As System.Object, ByVal e As System.EventArgs) Handles TextBoxPCSAFTEpsilon.TextChanged
    '    If Double.TryParse(TextBoxPCSAFTEpsilon.Text, New Double()) Then
    '        mycase.cp.PC_SAFT_epsilon_k = TextBoxPCSAFTEpsilon.Text
    '    End If
    'End Sub

    'Private Sub TextBoxCSAF_TextChanged(ByVal sender As System.Object, ByVal e As System.EventArgs) Handles TextBoxCSAF.TextChanged
    '    If Double.TryParse(TextBoxCSAF.Text, New Double()) Then
    '        mycase.cp.Chao_Seader_Acentricity = TextBoxCSAF.Text
    '    End If
    'End Sub

    'Private Sub TextBoxCSSP_TextChanged(ByVal sender As System.Object, ByVal e As System.EventArgs) Handles TextBoxCSSP.TextChanged
    '    If Double.TryParse(TextBoxCSSP.Text, New Double()) Then
    '        mycase.cp.Chao_Seader_Solubility_Parameter = TextBoxCSSP.Text
    '    End If
    'End Sub

    'Private Sub TextBoxCSLV_TextChanged(ByVal sender As System.Object, ByVal e As System.EventArgs) Handles TextBoxCSLV.TextChanged
    '    If Double.TryParse(TextBoxCSLV.Text, New Double()) Then
    '        mycase.cp.Chao_Seader_Liquid_Molar_Volume = TextBoxCSLV.Text
    '    End If
    'End Sub

    'Private Sub TextBoxVTCPR_TextChanged(ByVal sender As System.Object, ByVal e As System.EventArgs) Handles TextBoxVTCPR.TextChanged
    '    If Double.TryParse(TextBoxVTCPR.Text, New Double()) Then
    '        mycase.cp.PR_Volume_Translation_Coefficient = TextBoxVTCPR.Text
    '    End If
    'End Sub

    'Private Sub TextBoxVTCSRK_TextChanged(ByVal sender As System.Object, ByVal e As System.EventArgs) Handles TextBoxVTCSRK.TextChanged
    '    If Double.TryParse(TextBoxVTCSRK.Text, New Double()) Then
    '        mycase.cp.SRK_Volume_Translation_Coefficient = TextBoxVTCSRK.Text
    '    End If
    'End Sub

    'Private Sub TextBoxID_TextChanged(ByVal sender As System.Object, ByVal e As System.EventArgs) Handles TextBoxID.TextChanged
    '    mycase.cp.ID = TextBoxID.Text
    'End Sub

    'Private Sub TextBoxName_TextChanged(ByVal sender As System.Object, ByVal e As System.EventArgs) Handles TextBoxName.TextChanged
    '    mycase.cp.Name = TextBoxName.Text
    'End Sub

    'Private Sub TextBoxCAS_TextChanged(ByVal sender As System.Object, ByVal e As System.EventArgs) Handles TextBoxCAS.TextChanged
    '    mycase.cp.CAS_Number = TextBoxCAS.Text
    'End Sub

    'Private Sub TextBoxFormula_TextChanged(ByVal sender As System.Object, ByVal e As System.EventArgs) Handles TextBoxFormula.TextChanged
    '    mycase.cp.Formula = TextBoxFormula.Text
    'End Sub

    Private Sub SalvarNoBancoDeDadosToolStripMenuItem_Click(ByVal sender As System.Object, ByVal e As System.EventArgs)

        Try
            DWSIM.Databases.UserDB.AddCompounds(New DWSIM.ClassesBasicasTermodinamica.ConstantProperties() {mycase.cp}, tbDBPath.Text, chkReplaceComps.Checked)
            MessageBox.Show("Compound added to the database.", "Information", MessageBoxButtons.OK, MessageBoxIcon.Information)
        Catch ex As Exception
            MessageBox.Show("Error adding compound to the database: " & ex.Message.ToString, "Error", MessageBoxButtons.OK, MessageBoxIcon.Error)
        End Try

    End Sub

    Sub UpdateUnits()
        With su
            lblTc.Text = .spmp_temperature
            lblPc.Text = .spmp_pressure
            lblNBP.Text = .spmp_temperature
            lblMW.Text = .spmp_molecularWeight
            lblDHF.Text = .spmp_enthalpy
            lblDGF.Text = .spmp_enthalpy
            Me.GridExpDataPVAP.Columns(0).HeaderText = "T (" & su.spmp_temperature & ")"
            Me.GridExpDataCPIG.Columns(0).HeaderText = "T (" & su.spmp_temperature & ")"
            Me.GridExpDataLIQDENS.Columns(0).HeaderText = "T (" & su.spmp_temperature & ")"
            Me.GridExpDataLIQVISC.Columns(0).HeaderText = "T (" & su.spmp_temperature & ")"
            Me.GridExpDataPVAP.Columns(1).HeaderText = "Pvap (" & su.spmp_pressure & ")"
            Me.GridExpDataCPIG.Columns(1).HeaderText = "Cpig (" & su.spmp_heatCapacityCp & ")"
            Me.GridExpDataLIQDENS.Columns(1).HeaderText = "Dens (" & su.spmp_density & ")"
            Me.GridExpDataLIQVISC.Columns(1).HeaderText = "Visc (" & su.spmp_viscosity & ")"
        End With
    End Sub

    Public Function RegressData(ByVal tipo As Integer, ByVal calcular As Boolean)

        Dim obj As Object = Nothing
        Dim lmfit As New DWSIM.Utilities.PetroleumCharacterization.LMFit

        m_props = New PROPS()

        Dim c_pv(4), c_cp(4), c_vi(4), c_de(4) As Double
        Dim r_cp, r_vi, r_pv, n_cp, n_pv, n_vi, r_de, n_de As Double

        c_pv(0) = 25
        c_pv(1) = 2000
        c_pv(2) = -5.245
        c_pv(3) = 0.0#
        c_pv(4) = 0.0#

        c_cp(0) = 33.7
        c_cp(1) = 0.249
        c_cp(2) = 0.000253
        c_cp(3) = -0.000000384
        c_cp(4) = 0.000000000129

        c_vi(0) = -17.255
        c_vi(1) = 1576
        c_vi(2) = 0.86191
        c_vi(3) = 0
        c_vi(4) = 0

        c_de(3) = 0.15994
        c_de(2) = 647.3
        c_de(1) = 0.14056
        c_de(0) = -141.26

        Select Case tipo
            Case 0
                'regressão dos dados
                obj = lmfit.GetCoeffs(CopyToVector(mycase.DataPVAP, 0), CopyToVector(mycase.DataPVAP, 1), c_pv.Clone, DWSIM.Utilities.PetroleumCharacterization.LMFit.FitType.Pvap, 0.0000000001, 0.0000000001, 0.0000000001, 10000)
                c_pv = obj(0)
                r_pv = obj(2)
                n_pv = obj(3)
            Case 1
                If calcular Then
                    'get group amounts
                    Dim vn As New ArrayList
                    For Each r As DataGridViewRow In Me.GridUNIFAC.Rows
                        If Not r.Cells(0).Value Is Nothing Then
                            vn.Add(Integer.Parse(r.Cells(0).Value))
                        Else
                            vn.Add(0)
                        End If
                    Next
                    Dim vnd As Int32() = vn.ToArray(Type.GetType("System.Int32"))
                    c_cp(0) = Me.jb.CalcCpA(vnd)
                    c_cp(1) = Me.jb.CalcCpB(vnd)
                    c_cp(2) = Me.jb.CalcCpC(vnd)
                    c_cp(3) = Me.jb.CalcCpD(vnd)
                    obj = New Integer() {0, 0, 0, 10}
                Else
                    'regressão dos dados
                    obj = lmfit.GetCoeffs(CopyToVector(mycase.DataCPIG, 0), CopyToVector(mycase.DataCPIG, 1), c_cp, DWSIM.Utilities.PetroleumCharacterization.LMFit.FitType.Cp, 0.0000000001, 0.0000000001, 0.0000000001, 10000)
                    c_cp = obj(0)
                    r_cp = obj(2)
                    n_cp = obj(3)
                End If
            Case 2
                'regressão dos dados
                obj = lmfit.GetCoeffs(CopyToVector(mycase.DataLVISC, 0), CopyToVector(mycase.DataLVISC, 1), c_vi, DWSIM.Utilities.PetroleumCharacterization.LMFit.FitType.LiqVisc, 0.0000000001, 0.0000000001, 0.0000000001, 10000)
                c_vi = obj(0)
                r_vi = obj(2)
                n_vi = obj(3)
            Case 3

                Dim x1, x2, y1, y2, rhoc, al, bl As Double

                rhoc = mycase.cp.Molar_Weight / mycase.cp.Critical_Compressibility * 8.314 * mycase.cp.Critical_Temperature / mycase.cp.Critical_Pressure * 1000

                x1 = Log(1 - mycase.DataLDENS(0)(0) / mycase.cp.Critical_Temperature)
                x2 = Log(1 - mycase.DataLDENS(1)(0) / mycase.cp.Critical_Temperature)
                y1 = Log(Log(mycase.DataLDENS(0)(1) / rhoc))
                y2 = Log(Log(mycase.DataLDENS(1)(1) / rhoc))

                al = (y2 - y1) / (x2 - x1)
                bl = y1 - al * x1

                c_de(3) = al
                c_de(2) = mycase.cp.Critical_Temperature
                c_de(1) = 1 / Exp(Exp(bl))
                c_de(0) = c_de(1) * rhoc

                'regressão dos dados
                obj = lmfit.GetCoeffs(CopyToVector(mycase.DataLDENS, 0), CopyToVector(mycase.DataLDENS, 1), c_de, DWSIM.Utilities.PetroleumCharacterization.LMFit.FitType.LiqDens, 0.00001, 0.00001, 0.00001, 10000)
                c_de = obj(0)
                r_de = obj(2)
                n_de = obj(3)
        End Select

        Select Case tipo
            Case 0
                Return New Object() {c_pv, r_pv, n_pv, obj(1)}
            Case 1
                Return New Object() {c_cp, r_cp, n_cp, obj(1)}
            Case 2
                Return New Object() {c_vi, r_vi, n_vi, obj(1)}
            Case 3
                Return New Object() {c_de, r_de, n_de, obj(1)}
            Case Else
                Return Nothing
        End Select

    End Function

    Private Sub btnRegressPVAP_Click(ByVal sender As System.Object, ByVal e As System.EventArgs) Handles btnRegressPVAP.Click

        mycase.DataPVAP.Clear()
        For Each row As DataGridViewRow In Me.GridExpDataPVAP.Rows
            If row.Index < Me.GridExpDataPVAP.Rows.Count - 1 Then mycase.DataPVAP.Add(New Double() {cv.ConverterParaSI(su.spmp_temperature, row.Cells(0).Value), cv.ConverterParaSI(su.spmp_pressure, row.Cells(1).Value)})
        Next

        Dim result As Object = RegressData(0, False)

        tbStatusPVAP.Text = GetInfo(result(3))

        With mycase.cp
            .VaporPressureEquation = 101
            .Vapor_Pressure_Constant_A = result(0)(0)
            .Vapor_Pressure_Constant_B = result(0)(1)
            .Vapor_Pressure_Constant_C = result(0)(2)
            .Vapor_Pressure_Constant_D = result(0)(3)
            .Vapor_Pressure_Constant_E = result(0)(4)
        End With

    End Sub

    Private Sub btnRegressCPIG_Click(ByVal sender As System.Object, ByVal e As System.EventArgs) Handles btnRegressCPIG.Click

        mycase.DataCPIG.Clear()
        For Each row As DataGridViewRow In Me.GridExpDataCPIG.Rows
            If row.Index < Me.GridExpDataCPIG.Rows.Count - 1 Then mycase.DataCPIG.Add(New Double() {cv.ConverterParaSI(su.spmp_temperature, row.Cells(0).Value), cv.ConverterParaSI(su.spmp_heatCapacityCp, row.Cells(1).Value)})
        Next

        Dim result As Object = RegressData(1, False)

        tbStatusCPIG.Text = GetInfo(result(3))

        With mycase.cp
            .IdealgasCpEquation = 5
            .Ideal_Gas_Heat_Capacity_Const_A = result(0)(0) * 1000
            .Ideal_Gas_Heat_Capacity_Const_B = result(0)(1) * 1000
            .Ideal_Gas_Heat_Capacity_Const_C = result(0)(2) * 1000
            .Ideal_Gas_Heat_Capacity_Const_D = result(0)(3) * 1000
            .Ideal_Gas_Heat_Capacity_Const_E = result(0)(4) * 1000
        End With

    End Sub

    Private Sub btnRegressLIQDENS_Click(ByVal sender As System.Object, ByVal e As System.EventArgs) Handles btnRegressLIQDENS.Click

        mycase.DataLDENS.Clear()
        For Each row As DataGridViewRow In Me.GridExpDataLIQDENS.Rows
            If row.Index < Me.GridExpDataLIQDENS.Rows.Count - 1 Then mycase.DataLDENS.Add(New Double() {cv.ConverterParaSI(su.spmp_temperature, row.Cells(0).Value), cv.ConverterParaSI(su.spmp_density, row.Cells(1).Value)})
        Next

        Dim result As Object = RegressData(3, False)

        tbStatusLIQDENS.Text = GetInfo(result(3))

        With mycase.cp
            .LiquidDensityEquation = 105
            .Liquid_Density_Const_A = result(0)(0)
            .Liquid_Density_Const_B = result(0)(1)
            .Liquid_Density_Const_C = result(0)(2)
            .Liquid_Density_Const_D = result(0)(3)
            .Liquid_Density_Const_E = result(0)(4)
        End With

    End Sub

    Private Sub btnRegressLIQVISC_Click(ByVal sender As System.Object, ByVal e As System.EventArgs) Handles btnRegressLIQVISC.Click

        mycase.DataLVISC.Clear()
        For Each row As DataGridViewRow In Me.GridExpDataLIQVISC.Rows
            If row.Index < Me.GridExpDataLIQVISC.Rows.Count - 1 Then mycase.DataLVISC.Add(New Double() {cv.ConverterParaSI(su.spmp_temperature, row.Cells(0).Value), cv.ConverterParaSI(su.spmp_viscosity, row.Cells(1).Value)})
        Next

        Dim result As Object = RegressData(2, False)

        tbStatusLIQVISC.Text = GetInfo(result(3))

        With mycase.cp
            .LiquidViscosityEquation = 101
            .Liquid_Viscosity_Const_A = result(0)(0)
            .Liquid_Viscosity_Const_B = result(0)(1)
            .Liquid_Viscosity_Const_C = result(0)(2)
            .Liquid_Viscosity_Const_D = result(0)(3)
            .Liquid_Viscosity_Const_E = result(0)(4)
        End With

    End Sub

    Private Sub GridExpData_KeyDown1(ByVal sender As Object, ByVal e As System.Windows.Forms.KeyEventArgs) Handles GridExpDataCPIG.KeyDown, GridExpDataLIQDENS.KeyDown, GridExpDataLIQVISC.KeyDown, GridExpDataPVAP.KeyDown

        If e.KeyCode = Keys.Delete And e.Modifiers = Keys.Shift Then
            Dim toremove As New ArrayList
            For Each c As DataGridViewCell In CType(sender, DataGridView).SelectedCells
                If Not toremove.Contains(c.RowIndex) Then toremove.Add(c.RowIndex)
            Next
            Try
                For Each i As Integer In toremove
                    CType(sender, DataGridView).Rows.RemoveAt(i)
                Next
            Catch ex As Exception

            End Try
        ElseIf e.KeyCode = Keys.V And e.Modifiers = Keys.Control Then
            PasteData(sender)
        ElseIf e.KeyCode = Keys.Delete Then
            For Each c As DataGridViewCell In CType(sender, DataGridView).SelectedCells
                c.Value = ""
            Next
        End If

    End Sub

    Private Sub btnSearch_Click(ByVal sender As System.Object, ByVal e As System.EventArgs) Handles btnSearch.Click
        If Me.DBOpenDlg.ShowDialog(Me) = Windows.Forms.DialogResult.OK Then
            Me.tbDBPath.Text = Me.DBOpenDlg.FileName
        End If
    End Sub

    Private Sub btnCreateNewDB_Click(ByVal sender As System.Object, ByVal e As System.EventArgs) Handles btnCreateNewDB.Click
        If Me.DBOpenDlg.ShowDialog(Me) = Windows.Forms.DialogResult.OK Then
            If Not File.Exists(Me.DBOpenDlg.FileName) Then File.Create(Me.DBOpenDlg.FileName)
            Me.tbDBPath.Text = Me.DBOpenDlg.FileName
        End If
    End Sub

    Private Sub rbEstimatePVAP_CheckedChanged(ByVal sender As System.Object, ByVal e As System.EventArgs) Handles rbEstimatePVAP.CheckedChanged
        If rbEstimatePVAP.Checked Then
            mycase.cp.VaporPressureEquation = 0
            tbStatusPVAP.Text = "OK"
        End If
    End Sub

    Private Sub rbEstimateCPIG_CheckedChanged(ByVal sender As System.Object, ByVal e As System.EventArgs) Handles rbEstimateCPIG.CheckedChanged
        If rbEstimateCPIG.Checked Then
            mycase.cp.IdealgasCpEquation = 0
            tbStatusCPIG.Text = "OK"
        End If
    End Sub

    Private Sub rbEstimateLIQDENS_CheckedChanged(ByVal sender As System.Object, ByVal e As System.EventArgs) Handles rbEstimateLIQDENS.CheckedChanged
        If rbEstimateLIQDENS.Checked Then
            mycase.cp.LiquidDensityEquation = 0
            tbStatusLIQDENS.Text = "OK"
        End If
    End Sub

    Private Sub rbEstimateLIQVISC_CheckedChanged(ByVal sender As System.Object, ByVal e As System.EventArgs) Handles rbEstimateLIQVISC.CheckedChanged
        If rbEstimateLIQVISC.Checked Then
            mycase.cp.LiquidViscosityEquation = 0
            tbStatusLIQVISC.Text = "OK"
        End If
    End Sub

    Private Sub btnViewPVAP_Click(ByVal sender As System.Object, ByVal e As System.EventArgs) Handles btnViewPVAP.Click

        If tbStatusPVAP.Text = "" And rbRegressPVAP.Checked Then
            MessageBox.Show(DWSIM.App.GetLocalString("NoRegressionAvailable"), DWSIM.App.GetLocalString("Erro"), MessageBoxButtons.OK, MessageBoxIcon.Error)
            Exit Sub
        End If

        Dim mytext As New System.Text.StringBuilder
        mytext.AppendLine("Data Points")
        mytext.AppendLine("x" & vbTab & "yEXP" & vbTab & "yCALC")
        Dim px, py1, py2 As New ArrayList, x, y1, y2 As Double
        For Each d As Double() In mycase.DataPVAP
            x = cv.ConverterDoSI(su.spmp_temperature, d(0))
            px.Add(x)
            y1 = cv.ConverterDoSI(su.spmp_pressure, d(1))
            py1.Add(y1)
            Dim pp As New DWSIM.SimulationObjects.PropertyPackages.RaoultPropertyPackage(False)
            With mycase.cp
                y2 = cv.ConverterDoSI(su.spmp_pressure, pp.CalcCSTDepProp(.VaporPressureEquation, .Vapor_Pressure_Constant_A, .Vapor_Pressure_Constant_B, .Vapor_Pressure_Constant_C, .Vapor_Pressure_Constant_D, .Vapor_Pressure_Constant_E, d(0), 0))
                py2.Add(y2)
            End With
            mytext.AppendLine(x & vbTab & y1 & vbTab & y2)
        Next

        Dim frc As New FormChart
        With frc
            .tbtext = mytext.ToString
            .px = px
            .py1 = py1
            .py2 = py2
            .xformat = 1
            .ytitle = "Pvap / " & su.spmp_pressure
            .xtitle = "T / " & su.spmp_temperature
            .ycurvetypes = New ArrayList(New Integer() {1, 3})
            .title = "Vapor Pressure Fitting Results"
            .ShowDialog(Me)
        End With

    End Sub

    Private Sub btnViewCPIG_Click(ByVal sender As System.Object, ByVal e As System.EventArgs) Handles btnViewCPIG.Click

        If tbStatusCPIG.Text = "" And rbRegressCPIG.Checked Then
            MessageBox.Show(DWSIM.App.GetLocalString("NoRegressionAvailable"), DWSIM.App.GetLocalString("Erro"), MessageBoxButtons.OK, MessageBoxIcon.Error)
            Exit Sub
        End If

        Dim mytext As New System.Text.StringBuilder
        mytext.AppendLine("Data Points")
        mytext.AppendLine("x" & vbTab & "yEXP" & vbTab & "yCALC")
        Dim px, py1, py2 As New ArrayList, x, y1, y2 As Double
        For Each d As Double() In mycase.DataCPIG
            x = cv.ConverterDoSI(su.spmp_temperature, d(0))
            px.Add(x)
            y1 = cv.ConverterDoSI(su.spmp_heatCapacityCp, d(1))
            py1.Add(y1)
            Dim pp As New DWSIM.SimulationObjects.PropertyPackages.RaoultPropertyPackage(False)
            With mycase.cp
                y2 = cv.ConverterDoSI(su.spmp_heatCapacityCp, pp.CalcCSTDepProp(.IdealgasCpEquation, .Ideal_Gas_Heat_Capacity_Const_A, .Ideal_Gas_Heat_Capacity_Const_B, .Ideal_Gas_Heat_Capacity_Const_C, .Ideal_Gas_Heat_Capacity_Const_D, .Ideal_Gas_Heat_Capacity_Const_E, d(0), 0))
                py2.Add(y2)
            End With
        Next

        Dim frc As New FormChart
        With frc
            .tbtext = mytext.ToString
            .px = px
            .py1 = py1
            .py2 = py2
            .xformat = 1
            .ytitle = "Cpig / " & su.spmp_heatCapacityCp
            .xtitle = "T / " & su.spmp_temperature
            .ycurvetypes = New ArrayList(New Integer() {1, 3})
            .title = "Ideal Gas Heat Capacity Fitting Results"
            .ShowDialog(Me)
        End With

    End Sub

    Private Sub btnViewLIQDENS_Click(ByVal sender As System.Object, ByVal e As System.EventArgs) Handles btnViewLIQDENS.Click

        If tbStatusLIQDENS.Text = "" And rbRegressLIQDENS.Checked Then
            MessageBox.Show(DWSIM.App.GetLocalString("NoRegressionAvailable"), DWSIM.App.GetLocalString("Erro"), MessageBoxButtons.OK, MessageBoxIcon.Error)
            Exit Sub
        End If

        Dim mytext As New System.Text.StringBuilder
        mytext.AppendLine("Data Points")
        mytext.AppendLine("x" & vbTab & "yEXP" & vbTab & "yCALC")
        Dim px, py1, py2 As New ArrayList, x, y1, y2 As Double
        For Each d As Double() In mycase.DataLDENS
            x = cv.ConverterDoSI(su.spmp_temperature, d(0))
            px.Add(x)
            y1 = cv.ConverterDoSI(su.spmp_density, d(1))
            py1.Add(y1)
            Dim pp As New DWSIM.SimulationObjects.PropertyPackages.RaoultPropertyPackage(False)
            With mycase.cp
                y2 = cv.ConverterDoSI(su.spmp_density, pp.CalcCSTDepProp(.LiquidDensityEquation, .Liquid_Density_Const_A, .Liquid_Density_Const_B, .Liquid_Density_Const_C, .Liquid_Density_Const_D, .Liquid_Density_Const_E, d(0), 0))
                py2.Add(y2)
            End With
        Next

        Dim frc As New FormChart
        With frc
            .tbtext = mytext.ToString
            .px = px
            .py1 = py1
            .py2 = py2
            .xformat = 1
            .ytitle = "Rho / " & su.spmp_density
            .xtitle = "T / " & su.spmp_temperature
            .ycurvetypes = New ArrayList(New Integer() {1, 3})
            .title = "Liquid Density Fitting Results"
            .ShowDialog(Me)
        End With

    End Sub

    Private Sub btnViewLIQVISC_Click(ByVal sender As System.Object, ByVal e As System.EventArgs) Handles btnViewLIQVISC.Click

        If tbStatusLIQVISC.Text = "" And rbRegressLIQVISC.Checked Then
            MessageBox.Show(DWSIM.App.GetLocalString("NoRegressionAvailable"), DWSIM.App.GetLocalString("Erro"), MessageBoxButtons.OK, MessageBoxIcon.Error)
            Exit Sub
        End If

        Dim mytext As New System.Text.StringBuilder
        mytext.AppendLine("Data Points")
        mytext.AppendLine("x" & vbTab & "yEXP" & vbTab & "yCALC")
        Dim px, py1, py2 As New ArrayList, x, y1, y2 As Double
        For Each d As Double() In mycase.DataLVISC
            x = cv.ConverterDoSI(su.spmp_temperature, d(0))
            px.Add(x)
            y1 = cv.ConverterDoSI(su.spmp_viscosity, d(1))
            py1.Add(y1)
            Dim pp As New DWSIM.SimulationObjects.PropertyPackages.RaoultPropertyPackage(False)
            With mycase.cp
                y2 = cv.ConverterDoSI(su.spmp_viscosity, pp.CalcCSTDepProp(.LiquidViscosityEquation, .Liquid_Viscosity_Const_A, .Liquid_Viscosity_Const_B, .Liquid_Viscosity_Const_C, .Liquid_Viscosity_Const_D, .Liquid_Viscosity_Const_E, d(0), 0))
                py2.Add(y2)
            End With
        Next

        Dim frc As New FormChart
        With frc
            .tbtext = mytext.ToString
            .px = px
            .py1 = py1
            .py2 = py2
            .xformat = 1
            .ytitle = "Rho / " & su.spmp_viscosity
            .xtitle = "T / " & su.spmp_temperature
            .ycurvetypes = New ArrayList(New Integer() {1, 3})
            .title = "Liquid Viscosity Fitting Results"
            .ShowDialog(Me)
        End With

    End Sub

    Function GetInfo(ByVal code As Integer) As String

        Select Case code
            Case -1
                Return "Error - Wrong parameters were specified"
            Case 0
                Return "Error - Interrupted by user"
            Case 1
                Return "OK - Relative decrease of sum of function values squares (real and predicted on the base  of extrapolation) is less or equal EpsF."
            Case 2
                Return "OK - Relative change of solution is less or equal EpsX."
            Case 3
                Return "OK - Relative decrease of sum of function values squares (real and predicted on the base  of extrapolation) is less or equal EpsF / Relative change of solution is less or equal EpsX."
            Case 4
                Return "OK - Cosine of the angle between vector of function values and each of the Jacobian columns is less or equal EpsG by absolute value."
            Case 5
                Return "Number of iterations exceeds MaxIts."
            Case 6
                Return "EpsF is too small. It is impossible to get a better result."
            Case 7
                Return "EpsX is too small. It is impossible to get a better result"
            Case 8
                Return "EpsG is too small. Vector of functions is orthogonal to Jacobian columns with near-machine precision."
            Case Else
                Return "OK"
        End Select

    End Function

    Public Sub PasteData(ByRef dgv As DataGridView)
        Dim tArr() As String
        Dim arT() As String
        Dim i, ii As Integer
        Dim c, cc, r As Integer

        tArr = Clipboard.GetText().Split(Environment.NewLine)

        If dgv.SelectedCells.Count > 0 Then
            r = dgv.SelectedCells(0).RowIndex
            c = dgv.SelectedCells(0).ColumnIndex
        Else
            r = 0
            c = 0
        End If
        For i = 0 To tArr.Length - 1
            If tArr(i) <> "" Then
                arT = tArr(i).Split(vbTab)
                For ii = 0 To arT.Length - 1
                    If r > dgv.Rows.Count - 1 Then
                        dgv.Rows.Add()
                        dgv.Rows(0).Cells(0).Selected = True
                    End If
                Next
                r = r + 1
            End If
        Next
        If dgv.SelectedCells.Count > 0 Then
            r = dgv.SelectedCells(0).RowIndex
            c = dgv.SelectedCells(0).ColumnIndex
        Else
            r = 0
            c = 0
        End If
        For i = 0 To tArr.Length - 1
            If tArr(i) <> "" Then
                arT = tArr(i).Split(vbTab)
                cc = c
                For ii = 0 To arT.Length - 1
                    cc = GetNextVisibleCol(dgv, cc)
                    If cc > dgv.ColumnCount - 1 Then Exit For
                    dgv.Item(cc, r).Value = arT(ii).TrimStart
                    cc = cc + 1
                Next
                r = r + 1
            End If
        Next

    End Sub

    Private Function GetNextVisibleCol(ByRef dgv As DataGridView, ByVal stidx As Integer) As Integer

        Dim i As Integer

        For i = stidx To dgv.ColumnCount - 1
            If dgv.Columns(i).Visible Then Return i
        Next

        Return Nothing

    End Function

    Private Sub rbCoeffPVAP_CheckedChanged(sender As System.Object, e As System.EventArgs) Handles rbCoeffPVAP.CheckedChanged
        mycase.EqPVAP = rbCoeffPVAP.Checked
    End Sub

    Private Sub rbCoeffCPIG_CheckedChanged(sender As System.Object, e As System.EventArgs) Handles rbCoeffCPIG.CheckedChanged
        mycase.EqCPIG = rbCoeffCPIG.Checked
    End Sub

    Private Sub rbCoeffLIQDENS_CheckedChanged(sender As System.Object, e As System.EventArgs) Handles rbCoeffLIQDENS.CheckedChanged
        mycase.EqLDENS = rbCoeffLIQDENS.Checked
    End Sub

    Private Sub rbCoeffLIQVISC_CheckedChanged(sender As System.Object, e As System.EventArgs) Handles rbCoeffLIQVISC.CheckedChanged
        mycase.EqLVISC = rbCoeffLIQVISC.Checked
    End Sub

    Private Sub cbEqPVAP_SelectedIndexChanged(sender As System.Object, e As System.EventArgs) Handles cbEqPVAP.SelectedIndexChanged
        If mycase.EqPVAP Then mycase.cp.VaporPressureEquation = cbEqPVAP.SelectedItem.ToString.Split(":")(0)
    End Sub

    Private Sub cbEqCPIG_SelectedIndexChanged(sender As System.Object, e As System.EventArgs) Handles cbEqCPIG.SelectedIndexChanged
        If mycase.EqCPIG Then mycase.cp.IdealgasCpEquation = cbEqCPIG.SelectedItem.ToString.Split(":")(0)
    End Sub

    Private Sub cbEqLIQDENS_SelectedIndexChanged(sender As System.Object, e As System.EventArgs) Handles cbEqLIQDENS.SelectedIndexChanged
        If mycase.EqLDENS Then mycase.cp.LiquidDensityEquation = cbEqLIQDENS.SelectedItem.ToString.Split(":")(0)
    End Sub

    Private Sub cbEqLIQVISC_SelectedIndexChanged(sender As System.Object, e As System.EventArgs) Handles cbEqLIQVISC.SelectedIndexChanged
        If mycase.EqLVISC Then mycase.cp.LiquidViscosityEquation = cbEqLIQVISC.SelectedItem.ToString.Split(":")(0)
    End Sub

    Private Sub tbPVAP_A_TextChanged(sender As System.Object, e As System.EventArgs) Handles tbPVAP_A.TextChanged
        If mycase.EqPVAP Then mycase.cp.Vapor_Pressure_Constant_A = tbPVAP_A.Text
    End Sub

    Private Sub tbPVAP_B_TextChanged(sender As System.Object, e As System.EventArgs) Handles tbPVAP_B.TextChanged
        If mycase.EqPVAP Then mycase.cp.Vapor_Pressure_Constant_B = tbPVAP_B.Text
    End Sub

    Private Sub tbPVAP_C_TextChanged(sender As System.Object, e As System.EventArgs) Handles tbPVAP_C.TextChanged
        If mycase.EqPVAP Then mycase.cp.Vapor_Pressure_Constant_C = tbPVAP_C.Text
    End Sub

    Private Sub tbPVAP_D_TextChanged(sender As System.Object, e As System.EventArgs) Handles tbPVAP_D.TextChanged
        If mycase.EqPVAP Then mycase.cp.Vapor_Pressure_Constant_D = tbPVAP_D.Text
    End Sub

    Private Sub tbPVAP_E_TextChanged(sender As System.Object, e As System.EventArgs) Handles tbPVAP_E.TextChanged
        If mycase.EqPVAP Then mycase.cp.Vapor_Pressure_Constant_E = tbPVAP_E.Text
    End Sub

    Private Sub tbCPIG_A_TextChanged(sender As System.Object, e As System.EventArgs) Handles tbCPIG_A.TextChanged
        If mycase.EqCPIG Then mycase.cp.Ideal_Gas_Heat_Capacity_Const_A = tbCPIG_A.Text
    End Sub

    Private Sub tbCPIG_B_TextChanged(sender As System.Object, e As System.EventArgs) Handles tbCPIG_B.TextChanged
        If mycase.EqCPIG Then mycase.cp.Ideal_Gas_Heat_Capacity_Const_B = tbCPIG_B.Text
    End Sub

    Private Sub tbCPIG_C_TextChanged(sender As System.Object, e As System.EventArgs) Handles tbCPIG_C.TextChanged
        If mycase.EqCPIG Then mycase.cp.Ideal_Gas_Heat_Capacity_Const_C = tbCPIG_C.Text
    End Sub

    Private Sub tbCPIG_D_TextChanged(sender As System.Object, e As System.EventArgs) Handles tbCPIG_D.TextChanged
        If mycase.EqCPIG Then mycase.cp.Ideal_Gas_Heat_Capacity_Const_D = tbCPIG_D.Text
    End Sub

    Private Sub tbCPIG_E_TextChanged(sender As System.Object, e As System.EventArgs) Handles tbCPIG_E.TextChanged
        If mycase.EqCPIG Then mycase.cp.Ideal_Gas_Heat_Capacity_Const_E = tbCPIG_E.Text
    End Sub

    Private Sub tbLIQDENS_A_TextChanged(sender As System.Object, e As System.EventArgs) Handles tbLIQDENS_A.TextChanged
        If mycase.EqLDENS Then mycase.cp.Liquid_Density_Const_A = tbLIQDENS_A.Text
    End Sub

    Private Sub tbLIQDENS_b_TextChanged(sender As System.Object, e As System.EventArgs) Handles tbLIQDENS_B.TextChanged
        If mycase.EqLDENS Then mycase.cp.Liquid_Density_Const_B = tbLIQDENS_B.Text
    End Sub

    Private Sub tbLIQDENS_c_TextChanged(sender As System.Object, e As System.EventArgs) Handles tbLIQDENS_C.TextChanged
        If mycase.EqLDENS Then mycase.cp.Liquid_Density_Const_C = tbLIQDENS_C.Text
    End Sub

    Private Sub tbLIQDENS_d_TextChanged(sender As System.Object, e As System.EventArgs) Handles tbLIQDENS_D.TextChanged
        If mycase.EqLDENS Then mycase.cp.Liquid_Density_Const_D = tbLIQDENS_D.Text
    End Sub

    Private Sub tbLIQDENS_e_TextChanged(sender As System.Object, e As System.EventArgs) Handles tbLIQDENS_E.TextChanged
        If mycase.EqLDENS Then mycase.cp.Liquid_Density_Const_E = tbLIQDENS_E.Text
    End Sub

    Private Sub tbLIQVISC_A_TextChanged(sender As System.Object, e As System.EventArgs) Handles tbLIQVISC_A.TextChanged
        If mycase.EqLVISC Then mycase.cp.Liquid_Viscosity_Const_A = tbLIQVISC_A.Text
    End Sub

    Private Sub tbLIQVISC_B_TextChanged(sender As System.Object, e As System.EventArgs) Handles tbLIQVISC_B.TextChanged
        If mycase.EqLVISC Then mycase.cp.Liquid_Viscosity_Const_B = tbLIQVISC_B.Text
    End Sub

    Private Sub tbLIQVISC_C_TextChanged(sender As System.Object, e As System.EventArgs) Handles tbLIQVISC_C.TextChanged
        If mycase.EqLVISC Then mycase.cp.Liquid_Viscosity_Const_C = tbLIQVISC_C.Text
    End Sub

    Private Sub tbLIQVISC_D_TextChanged(sender As System.Object, e As System.EventArgs) Handles tbLIQVISC_D.TextChanged
        If mycase.EqLVISC Then mycase.cp.Liquid_Viscosity_Const_D = tbLIQVISC_D.Text
    End Sub

    Private Sub tbLIQVISC_E_TextChanged(sender As System.Object, e As System.EventArgs) Handles tbLIQVISC_E.TextChanged
        If mycase.EqLVISC Then mycase.cp.Liquid_Viscosity_Const_E = tbLIQVISC_E.Text
    End Sub

    Private Sub CheckBoxTc_CheckedChanged(sender As System.Object, e As System.EventArgs) Handles CheckBoxTc.CheckedChanged
        mycase.CalcTC = CheckBoxTc.Checked
    End Sub

    Private Sub CheckBoxPc_CheckedChanged(sender As System.Object, e As System.EventArgs) Handles CheckBoxPc.CheckedChanged
        mycase.CalcPC = CheckBoxPc.Checked
    End Sub

    Private Sub CheckBoxZc_CheckedChanged(sender As System.Object, e As System.EventArgs) Handles CheckBoxZc.CheckedChanged
        mycase.CalcZC = CheckBoxZc.Checked
    End Sub

    Private Sub CheckBoxZRa_CheckedChanged(sender As System.Object, e As System.EventArgs) Handles CheckBoxZRa.CheckedChanged
        mycase.CalcZRA = CheckBoxZRa.Checked
    End Sub

    Private Sub CheckBoxAF_CheckedChanged(sender As System.Object, e As System.EventArgs) Handles CheckBoxAF.CheckedChanged
        mycase.CalcAF = CheckBoxAF.Checked
    End Sub

    Private Sub CheckBoxMW_CheckedChanged(sender As System.Object, e As System.EventArgs) Handles CheckBoxMW.CheckedChanged
        mycase.CalcMW = CheckBoxMW.Checked
    End Sub

    Private Sub CheckBoxDHF_CheckedChanged(sender As System.Object, e As System.EventArgs) Handles CheckBoxDHF.CheckedChanged
        mycase.CalcHF = CheckBoxDHF.Checked
    End Sub

    Private Sub CheckBoxDGF_CheckedChanged(sender As System.Object, e As System.EventArgs) Handles CheckBoxDGF.CheckedChanged
        mycase.CalcGF = CheckBoxDGF.Checked
    End Sub

    Private Sub CheckBoxNBP_CheckedChanged(sender As System.Object, e As System.EventArgs) Handles CheckBoxNBP.CheckedChanged
        mycase.CalcNBP = CheckBoxNBP.Checked
    End Sub

    Private Sub CheckBoxCSAF_CheckedChanged(sender As System.Object, e As System.EventArgs) Handles CheckBoxCSAF.CheckedChanged
        mycase.CalcCSAF = CheckBoxCSAF.Checked
    End Sub

    Private Sub CheckBoxCSSP_CheckedChanged(sender As System.Object, e As System.EventArgs) Handles CheckBoxCSSP.CheckedChanged
        mycase.CalcCSSP = CheckBoxCSSP.Checked
    End Sub

    Private Sub CheckBoxCSLV_CheckedChanged(sender As System.Object, e As System.EventArgs) Handles CheckBoxCSLV.CheckedChanged
        mycase.CalcCSMV = CheckBoxCSLV.Checked
    End Sub

    Private Sub TextBoxUNIQUAC_Q_TextChanged(sender As System.Object, e As System.EventArgs) Handles TextBoxUNIQUAC_Q.TextChanged
        If Double.TryParse(TextBoxUNIQUAC_Q.Text, New Double()) Then
            mycase.cp.UNIQUAC_Q = TextBoxUNIQUAC_Q.Text
        End If
    End Sub

    Private Sub TextBoxUNIQUAC_R_TextChanged(sender As System.Object, e As System.EventArgs) Handles TextBoxUNIQUAC_R.TextChanged
        If Double.TryParse(TextBoxUNIQUAC_R.Text, New Double()) Then
            mycase.cp.UNIQUAC_R = TextBoxUNIQUAC_R.Text
        End If
    End Sub

    Private Sub rb_CheckedChanged(sender As System.Object, e As System.EventArgs) Handles rbCoeffCPIG.CheckedChanged, rbCoeffLIQDENS.CheckedChanged, rbCoeffLIQVISC.CheckedChanged, rbCoeffPVAP.CheckedChanged, _
                                                                                        rbEstimateCPIG.CheckedChanged, rbEstimateLIQDENS.CheckedChanged, rbEstimateLIQVISC.CheckedChanged, rbEstimatePVAP.CheckedChanged, _
                                                                                        rbRegressCPIG.CheckedChanged, rbRegressLIQDENS.CheckedChanged, rbRegressLIQVISC.CheckedChanged, rbRegressPVAP.CheckedChanged
        If loaded Then StoreData()

    End Sub

    Private Sub FormCompoundCreator_Shown(sender As Object, e As System.EventArgs) Handles Me.Shown

        loaded = True

    End Sub

    Private Sub Button1_Click(sender As System.Object, e As System.EventArgs) Handles Button1.Click
        Try
            StoreData()
            DWSIM.Databases.UserDB.AddCompounds(New DWSIM.ClassesBasicasTermodinamica.ConstantProperties() {mycase.cp}, tbDBPath.Text, chkReplaceComps.Checked)
            MessageBox.Show("Compound added to the database.", "Information", MessageBoxButtons.OK, MessageBoxIcon.Information)
        Catch ex As Exception
            MessageBox.Show("Error adding compound to the database: " & ex.Message.ToString, "Error", MessageBoxButtons.OK, MessageBoxIcon.Error)
        End Try
    End Sub

    Private Sub cbUnits_SelectedIndexChanged(sender As Object, e As System.EventArgs) Handles cbUnits.SelectedIndexChanged
        If loaded Then
            StoreData()
        End If
        If CType(Me.MdiParent, FormMain).AvailableUnitSystems.ContainsKey(cbUnits.SelectedItem.ToString) Then
            su = CType(Me.MdiParent, FormMain).AvailableUnitSystems(cbUnits.SelectedItem.ToString)
        End If
        If loaded Then
            mycase.su = su
            UpdateUnits()
            WriteData()
        End If
    End Sub

End Class

<System.Serializable()> Public Class CompoundGeneratorCase

    Sub New()
        cp = New DWSIM.ClassesBasicasTermodinamica.ConstantProperties
        su = New DWSIM.SistemasDeUnidades.UnidadesSI
    End Sub

    Public Filename As String = ""

    Public cp As DWSIM.ClassesBasicasTermodinamica.ConstantProperties
    Public database As String = ""
    Public su As DWSIM.SistemasDeUnidades.Unidades
    Public nf As String = My.Computer.Info.InstalledUICulture.NumberFormat.ToString

    Public CalcMW As Boolean = True
    Public CalcTC As Boolean = True
    Public CalcPC As Boolean = True
    Public CalcZC As Boolean = True
    Public CalcZRA As Boolean = True
    Public CalcAF As Boolean = True
    Public CalcHF As Boolean = True
    Public CalcGF As Boolean = True
    Public CalcNBP As Boolean = True
    Public CalcCSAF As Boolean = True
    Public CalcCSSP As Boolean = True
    Public CalcCSMV As Boolean = True

    Public RegressPVAP As Boolean = False
    Public RegressCPIG As Boolean = False
    Public RegressLVISC As Boolean = False
    Public RegressLDENS As Boolean = False

    Public EqPVAP As Boolean = False
    Public EqCPIG As Boolean = False
    Public EqLVISC As Boolean = False
    Public EqLDENS As Boolean = False

    Public RegressOKPVAP As Boolean = False
    Public RegressOKCPIG As Boolean = False
    Public RegressOKLVISC As Boolean = False
    Public RegressOKLDENS As Boolean = False

    Public ErrorMsgPVAP As String = ""
    Public ErrorMsgCPIG As String = ""
    Public ErrorMsgLVISC As String = ""
    Public ErrorMsgLDENS As String = ""

    Public DataPVAP As New ArrayList
    Public DataCPIG As New ArrayList
    Public DataLVISC As New ArrayList
    Public DataLDENS As New ArrayList

End Class