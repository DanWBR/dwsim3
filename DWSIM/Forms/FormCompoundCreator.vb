Imports com.ggasoftware.indigo
'http://www.ggasoftware.com/opensource/indigo

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
    Friend PureUNIFACCompound As Boolean = True
    Friend isDWSimSaved As Boolean = True
    Friend isUserDBSaved As Boolean = True
    Private forceclose As Boolean = False
    Private populating As Boolean = False
    Private UNIFAClines(), JOBACKlines(), ElementLines() As String

    Private Sub FormCompoundCreator_Load(ByVal sender As System.Object, ByVal e As System.EventArgs) Handles MyBase.Load

        'Grid UNIFAC
        Dim pathsep = System.IO.Path.DirectorySeparatorChar
        Dim picpath As String = My.Application.Info.DirectoryPath & pathsep & "data" & pathsep & "unifac" & pathsep
        Dim filename As String = My.Application.Info.DirectoryPath & pathsep & "data" & pathsep & "unifac.txt"

        Dim i As Integer

        UNIFAClines = IO.File.ReadAllLines(filename)
        With Me.GridUNIFAC.Rows
            .Clear()
            For i = 2 To UNIFAClines.Length - 1

                .Add(New Object() {CInt(0), Image.FromFile(picpath & UNIFAClines(i).Split(",")(7) & ".png")})
                .Item(.Count - 1).HeaderCell.Value = UNIFAClines(i).Split(",")(3)
                .Item(.Count - 1).HeaderCell.Tag = UNIFAClines(i).Split(",")(2)
                .Item(.Count - 1).Cells(1).ToolTipText = "Main Group: " & UNIFAClines(i).Split(",")(2) & vbCrLf & _
                                                         "Subgroup: " & UNIFAClines(i).Split(",")(3) & vbCrLf & _
                                                         "Rk / Qk: " & UNIFAClines(i).Split(",")(4) & " / " & UNIFAClines(i).Split(",")(5) & vbCrLf & _
                                                         "Example Compound: " & UNIFAClines(i).Split(",")(6) & vbCrLf & _
                                                         "Joback subgroups: " & UNIFAClines(i).Split(",")(8)

                
            Next
        End With

        'Grid Joback
        filename = My.Application.Info.DirectoryPath & pathsep & "data" & pathsep & "JobackGroups.txt"
        JOBACKlines = IO.File.ReadAllLines(filename)

        Dim ID, GroupType, GroupName As String
        Dim L As Boolean = True

        GroupType = ""

        With Me.GridJoback.Rows
            .Clear()
            For i = 1 To JOBACKlines.Length - 1

                ID = JOBACKlines(i).Split(";")(0)

                If Not ID = "X" Then
                    .Add(New Object())
                    .Item(.Count - 1).HeaderCell.Value = "ID " & ID
                    .Item(.Count - 1).Cells(0).Value = GroupType
                    GroupName = JOBACKlines(i).Split(";")(1)
                    .Item(.Count - 1).Cells(1).Value = GroupName

                    If L Then
                        .Item(.Count - 1).Cells(0).Style.BackColor = Color.CadetBlue
                    Else
                        .Item(.Count - 1).Cells(0).Style.BackColor = Color.CornflowerBlue
                    End If
                Else
                    GroupType = JOBACKlines(i).Split(";")(1)
                    L = Not L
                End If
            Next

        End With

        'Grid addition Elements
        filename = My.Application.Info.DirectoryPath & pathsep & "data" & pathsep & "Elements.txt"
        ElementLines = IO.File.ReadAllLines(filename)
        With Me.AddAtomDataGrid.Rows
            .Clear()
            For i = 1 To ElementLines.Length - 1
                .Add(New Object())
                .Item(.Count - 1).Cells(0).Value = ElementLines(i).Split(";")(2)
                .Item(.Count - 1).Cells(0).ToolTipText = "Element # " & ElementLines(i).Split(";")(0) & vbCrLf & _
                                    ElementLines(i).Split(";")(1) & vbCrLf & "MW: " & ElementLines(i).Split(";")(3)
            Next
        End With


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
        cbEqSolidDENS.SelectedIndex = 0
        cbEqCpS.SelectedIndex = 0

        Me.cbUnits.Items.Clear()

        For Each su In CType(Me.MdiParent, FormMain).AvailableUnitSystems.Values
            Me.cbUnits.Items.Add(su.nome)
        Next

        Me.cbUnits.SelectedIndex = 0

        Me.TextBoxID.Text = New Random().Next(100000)

        SetCompCreatorSaveStatus(True)
        SetUserDBSaveStatus(True)

    End Sub

    Private Sub FormCompoundCreator_FormClosing(ByVal sender As Object, ByVal e As System.Windows.Forms.FormClosingEventArgs) Handles MyBase.FormClosing
        If Not forceclose Then
            'CompoundCeator case file
            If Not isDWSimSaved Then
                Dim x = MessageBox.Show(DWSIM.App.GetLocalString("Desejasalvarasaltera") & Chr(13) & " " & Me.Text, DWSIM.App.GetLocalString("Fechando"), MessageBoxButtons.YesNoCancel, MessageBoxIcon.Question)
                If x = MsgBoxResult.Yes Then
                    FormMain.SaveStudyDlg.FileName = mycase.Filename
                    FormMain.SaveFileDialog()
                ElseIf x = MsgBoxResult.Cancel Then
                    e.Cancel = True
                End If
            End If

            'User database
            If Not isUserDBSaved Then
                Dim y = MessageBox.Show(DWSIM.App.GetLocalString("DesejaSalvaroUserDB"), DWSIM.App.GetLocalString("Fechando") & " " & DWSIM.App.GetLocalString("BancodeDados"), MessageBoxButtons.YesNoCancel, MessageBoxIcon.Question)
                If y = MsgBoxResult.Yes Then
                    btnSaveToDB_Click(sender, e)
                ElseIf y = MsgBoxResult.Cancel Then
                    e.Cancel = True
                End If
            End If

            If Not e.Cancel Then
                forceclose = True
                Me.Close()
            End If
        End If
    End Sub
    Sub SetCompCreatorSaveStatus(ByVal Status As Boolean)
        isDWSimSaved = Status
        If isDWSimSaved Then
            ToolStripStatusDWSIM.Text = DWSIM.App.GetLocalString("Saved")
            ToolStripStatusDWSIM.BackColor = Color.Green

        Else
            ToolStripStatusDWSIM.Text = DWSIM.App.GetLocalString("Modified")
            ToolStripStatusDWSIM.BackColor = Color.Red
        End If
    End Sub
    Sub SetUserDBSaveStatus(ByVal Status As Boolean)
        isUserDBSaved = Status
        If isUserDBSaved Then
            ToolStripStatusUserDB.Text = DWSIM.App.GetLocalString("Saved")
            ToolStripStatusUserDB.BackColor = Color.Green
        Else
            ToolStripStatusUserDB.Text = DWSIM.App.GetLocalString("Modified")
            ToolStripStatusUserDB.BackColor = Color.Red
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
            If cbUnits.Items.Contains(.su.nome) Then
                cbUnits.SelectedIndex = cbUnits.Items.IndexOf(.su.nome)
            Else : cbUnits.SelectedIndex = 0
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
            TextBoxMeltingTemp.Text = cv.ConverterDoSI(su.spmp_temperature, .cp.TemperatureOfFusion)
            TextBoxEnthOfFusion.Text = cv.ConverterDoSI(su.spmp_enthalpy, .cp.EnthalpyOfFusionAtTf)
            TextBoxSMILES.Text = .cp.SMILES
            If Not .cp.SMILES = "" Then
                RenderSMILES()
            End If



            If .RegressPVAP Then rbRegressPVAP.Checked = True
            If .RegressCPIG Then rbRegressCPIG.Checked = True
            If .RegressLDENS Then rbRegressLIQDENS.Checked = True
            If .RegressLVISC Then rbRegressLIQVISC.Checked = True
            If .RegressCpS Then rbRegressSolidCp.Checked = True
            If .RegressRoS Then rbRegressSolidDens.Checked = True

            If .EqPVAP Then rbCoeffPVAP.Checked = True
            If .EqCPIG Then rbCoeffCPIG.Checked = True
            If .EqLDENS Then rbCoeffLIQDENS.Checked = True
            If .EqLVISC Then rbCoeffLIQVISC.Checked = True
            If .EqCpS Then rbCoeffSolidCp.Checked = True
            If .EqSDens Then rbCoeffSolidDens.Checked = True

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
            CheckBoxMeltingTemp.Checked = .CalcMP
            CheckBoxEnthOfFusion.Checked = .CalcEM

            AtomDataGrid.Rows.Clear()
            For i = 0 To .cp.Elements.Collection.Count - 1
                AtomDataGrid.Rows.Add(New Object() {.cp.Elements.Collection.GetKey(i), .cp.Elements.Collection.GetByIndex(i)})
            Next

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

            For Each it As Object In cbEqCpS.Items
                If it.ToString.Split(":")(0) = .cp.SolidHeatCapacityEquation Then
                    cbEqCpS.SelectedIndex = cbEqCpS.Items.IndexOf(it)
                    Exit For
                End If
            Next

            For Each it As Object In cbEqSolidDENS.Items
                If it.ToString.Split(":")(0) = .cp.SolidDensityEquation Then
                    cbEqSolidDENS.SelectedIndex = cbEqSolidDENS.Items.IndexOf(it)
                    Exit For
                End If
            Next

            tbCpS_A.Text = .cp.Solid_Heat_Capacity_Const_A
            tbCpS_B.Text = .cp.Solid_Heat_Capacity_Const_B
            tbCpS_C.Text = .cp.Solid_Heat_Capacity_Const_C
            tbCpS_D.Text = .cp.Solid_Heat_Capacity_Const_D
            tbCpS_E.Text = .cp.Solid_Heat_Capacity_Const_E

            tbRoS_A.Text = .cp.Solid_Density_Const_A
            tbRoS_B.Text = .cp.Solid_Density_Const_B
            tbRoS_C.Text = .cp.Solid_Density_Const_C
            tbRoS_D.Text = .cp.Solid_Density_Const_D
            tbRoS_E.Text = .cp.Solid_Density_Const_E

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

            FillUnifacSubGroups()

            'check for updated elements when loading older files and create them if not existing
            If .JobackGroups Is Nothing Then .JobackGroups = New ArrayList()
            If .DataRoS Is Nothing Then .DataRoS = New ArrayList
            If .DataCpS Is Nothing Then .DataCpS = New ArrayList


            'populating Joback Grid with additional Joback groups
            For i = 0 To .JobackGroups.Count - 1
                Me.GridJoback.Rows.Item(.JobackGroups.Item(i)(0)).Cells(3).Value = .JobackGroups.Item(i)(1)
                If .JobackGroups.Item(i)(1) > 0 Then PureUNIFACCompound = False
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
            Me.GridExpDataRoS.Rows.Clear()
            For i = 0 To .DataRoS.Count - 1
                Me.GridExpDataRoS.Rows.Add(New Object() {cv.ConverterDoSI(su.spmp_temperature, .DataRoS(i)(0)), cv.ConverterDoSI(su.spmp_density, .DataRoS(i)(1))})
            Next
            Me.GridExpDataCpS.Rows.Clear()
            For i = 0 To .DataCpS.Count - 1
                Me.GridExpDataCpS.Rows.Add(New Object() {cv.ConverterDoSI(su.spmp_temperature, .DataCpS(i)(0)), cv.ConverterDoSI(su.spmp_heatCapacityCp, .DataCpS(i)(1))})
            Next
            If .RegressOKPVAP Then tbStatusPVAP.Text = "OK" Else tbStatusPVAP.Text = .ErrorMsgPVAP
            If .RegressOKCPIG Then tbStatusCPIG.Text = "OK" Else tbStatusCPIG.Text = .ErrorMsgCPIG
            If .RegressOKLDENS Then tbStatusLIQDENS.Text = "OK" Else tbStatusLIQDENS.Text = .ErrorMsgLDENS
            If .RegressOKLVISC Then tbStatusLIQVISC.Text = "OK" Else tbStatusLIQVISC.Text = .ErrorMsgLVISC
            If .RegressOKRoS Then tbStatusSolidDens.Text = "OK" Else tbStatusSolidDens.Text = .ErrorMsgRoS
            If .RegressOKCpS Then tbStatusSolidCp.Text = "OK" Else tbStatusSolidCp.Text = .ErrorMsgCpS

        End With

    End Sub

    Sub FillUnifacSubGroups()

        'fill Joback groups table with UNIFAC subgoups
        Dim k, ugc, usgc, usgid, oc As Integer
        Dim JG, JSG As String

        For Each r As DataGridViewRow In Me.GridJoback.Rows
            r.Cells(2).Value = Nothing
        Next
        For Each r As DataGridViewRow In Me.GridUNIFAC.Rows
            ugc = mycase.cp.UNIFACGroups.Collection(r.HeaderCell.Value)

            'Joback groups from UNIFAC subgroups
            JG = UNIFAClines(r.Index + 2).Split(",")(8) 'Joback Subgroup List
            If r.Cells(0).Value > 0 Then
                For k = 0 To 3
                    JSG = JG.Split("/")(k)
                    If Not JSG = "" Then
                        usgc = JSG.Split(":")(0) 'Joback subgroup count
                        usgid = JSG.Split(":")(1) 'Joback subgroup ID
                        oc = Me.GridJoback.Rows.Item(usgid - 1).Cells(2).Value
                        Me.GridJoback.Rows.Item(usgid - 1).Cells(2).Value = oc + usgc * ugc
                    End If
                Next
            End If
        Next
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
            .cp.SMILES = TextBoxSMILES.Text
            .cp.TemperatureOfFusion = cv.ConverterParaSI(su.spmp_temperature, TextBoxMeltingTemp.Text)
            .cp.EnthalpyOfFusionAtTf = cv.ConverterParaSI(su.spmp_enthalpy, TextBoxEnthOfFusion.Text)

            .RegressPVAP = rbRegressPVAP.Checked
            .RegressCPIG = rbRegressCPIG.Checked
            .RegressLDENS = rbRegressLIQDENS.Checked
            .RegressLVISC = rbRegressLIQVISC.Checked
            .RegressRoS = rbRegressSolidDens.Checked
            .RegressCpS = rbRegressSolidCp.Checked

            .EqPVAP = rbCoeffPVAP.Checked
            .EqCPIG = rbCoeffCPIG.Checked
            .EqLDENS = rbCoeffLIQDENS.Checked
            .EqLVISC = rbCoeffLIQVISC.Checked
            .EqSDens = rbCoeffSolidDens.Checked
            .EqCpS = rbCoeffSolidCp.Checked

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
            .CalcMP = CheckBoxMeltingTemp.Checked
            .CalcEM = CheckBoxEnthOfFusion.Checked

            .cp.VaporPressureEquation = cbEqPVAP.SelectedItem.ToString.Split(":")(0)
            .cp.LiquidDensityEquation = cbEqLIQDENS.SelectedItem.ToString.Split(":")(0)
            .cp.LiquidViscosityEquation = cbEqLIQVISC.SelectedItem.ToString.Split(":")(0)
            .cp.SolidHeatCapacityEquation = cbEqCpS.SelectedIndex.ToString.Split(":")(0)
            .cp.SolidDensityEquation = cbEqSolidDENS.SelectedIndex.ToString.Split(":")(0)

            .cp.Solid_Heat_Capacity_Const_A = CheckEmptyCell(tbCpS_A.Text)
            .cp.Solid_Heat_Capacity_Const_B = CheckEmptyCell(tbCpS_B.Text)
            .cp.Solid_Heat_Capacity_Const_C = CheckEmptyCell(tbCpS_C.Text)
            .cp.Solid_Heat_Capacity_Const_D = CheckEmptyCell(tbCpS_D.Text)
            .cp.Solid_Heat_Capacity_Const_E = CheckEmptyCell(tbCpS_E.Text)
            .cp.Solid_Heat_Capacity_Tmin = 0
            .cp.Solid_Heat_Capacity_Tmax = .cp.TemperatureOfFusion

            .cp.Solid_Density_Const_A = CheckEmptyCell(tbRoS_A.Text)
            .cp.Solid_Density_Const_B = CheckEmptyCell(tbRoS_B.Text)
            .cp.Solid_Density_Const_C = CheckEmptyCell(tbRoS_C.Text)
            .cp.Solid_Density_Const_D = CheckEmptyCell(tbRoS_D.Text)
            .cp.Solid_Density_Const_E = CheckEmptyCell(tbRoS_E.Text)
            .cp.Solid_Density_Tmin = 0
            .cp.Solid_Density_Tmax = .cp.TemperatureOfFusion

            .cp.Vapor_Pressure_Constant_A = CheckEmptyCell(tbPVAP_A.Text)
            .cp.Vapor_Pressure_Constant_B = CheckEmptyCell(tbPVAP_B.Text)
            .cp.Vapor_Pressure_Constant_C = CheckEmptyCell(tbPVAP_C.Text)
            .cp.Vapor_Pressure_Constant_D = CheckEmptyCell(tbPVAP_D.Text)
            .cp.Vapor_Pressure_Constant_E = CheckEmptyCell(tbPVAP_E.Text)

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
                .cp.Ideal_Gas_Heat_Capacity_Const_A = CheckEmptyCell(tbCPIG_A.Text)
                .cp.Ideal_Gas_Heat_Capacity_Const_B = CheckEmptyCell(tbCPIG_B.Text)
                .cp.Ideal_Gas_Heat_Capacity_Const_C = CheckEmptyCell(tbCPIG_C.Text)
                .cp.Ideal_Gas_Heat_Capacity_Const_D = CheckEmptyCell(tbCPIG_D.Text)
                .cp.Ideal_Gas_Heat_Capacity_Const_E = CheckEmptyCell(tbCPIG_E.Text)
            End If

            .cp.Liquid_Density_Const_A = CheckEmptyCell(tbLIQDENS_A.Text)
            .cp.Liquid_Density_Const_B = CheckEmptyCell(tbLIQDENS_B.Text)
            .cp.Liquid_Density_Const_C = CheckEmptyCell(tbLIQDENS_C.Text)
            .cp.Liquid_Density_Const_D = CheckEmptyCell(tbLIQDENS_D.Text)
            .cp.Liquid_Density_Const_E = CheckEmptyCell(tbLIQDENS_E.Text)

            .cp.Liquid_Viscosity_Const_A = CheckEmptyCell(tbLIQVISC_A.Text)
            .cp.Liquid_Viscosity_Const_B = CheckEmptyCell(tbLIQVISC_B.Text)
            .cp.Liquid_Viscosity_Const_C = CheckEmptyCell(tbLIQVISC_C.Text)
            .cp.Liquid_Viscosity_Const_D = CheckEmptyCell(tbLIQVISC_D.Text)
            .cp.Liquid_Viscosity_Const_E = CheckEmptyCell(tbLIQVISC_E.Text)

            For Each r As DataGridViewRow In Me.GridUNIFAC.Rows
                If CInt(r.Cells(0).Value) <> 0 Then
                    .cp.UNIFACGroups.Collection(r.HeaderCell.Value) = r.Cells(0).Value
                    '.cp.MODFACGroups.Collection(r.HeaderCell.Value) = r.Cells(0).Value
                Else
                    If .cp.UNIFACGroups.Collection.ContainsKey(r.HeaderCell.Value) Then
                        .cp.UNIFACGroups.Collection.Remove(r.HeaderCell.Value)
                    End If
                    'MODFAC groups need to be defined separately! Not yet implemented!
                    'If .cp.MODFACGroups.Collection.ContainsKey(r.HeaderCell.Value) Then
                    '    .cp.MODFACGroups.Collection.Remove(r.HeaderCell.Value)
                    'End If
                End If
            Next

            Dim JC As Integer
            If mycase.JobackGroups Is Nothing Then .JobackGroups = New ArrayList 'for old file versions where field was not defined
            .JobackGroups.Clear()
            For Each r As DataGridViewRow In Me.GridJoback.Rows
                JC = r.Cells(3).Value
                If JC > 0 Then .JobackGroups.Add(New Integer() {r.Index, JC})
            Next

            .cp.Elements.Collection.Clear()
            For Each r As DataGridViewRow In Me.AtomDataGrid.Rows
                .cp.Elements.Collection.Add(r.Cells(0).Value, r.Cells(1).Value)
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

            mycase.DataRoS.Clear()
            For Each row As DataGridViewRow In Me.GridExpDataRoS.Rows
                If row.Index < Me.GridExpDataRoS.Rows.Count - 1 Then mycase.DataRoS.Add(New Double() {cv.ConverterParaSI(su.spmp_temperature, row.Cells(0).Value), cv.ConverterParaSI(su.spmp_density, row.Cells(1).Value)})
            Next

            mycase.DataCpS.Clear()
            For Each row As DataGridViewRow In Me.GridExpDataCpS.Rows
                If row.Index < Me.GridExpDataCpS.Rows.Count - 1 Then mycase.DataCpS.Add(New Double() {cv.ConverterParaSI(su.spmp_temperature, row.Cells(0).Value), cv.ConverterParaSI(su.spmp_heatCapacityCp, row.Cells(1).Value)})
            Next

        End With


    End Sub

    Function CheckEmptyCell(ByVal val As String) As Double
        Try
            CheckEmptyCell = val
        Catch ex As Exception
            CheckEmptyCell = Nothing
        End Try
    End Function
    Function CheckValidDF(ByVal val As String) As Boolean
        Dim DN As Double
        Try
            DN = val
            CheckValidDF = True
        Catch ex As Exception
            CheckValidDF = False
        End Try
    End Function

    Private Sub CalcJobackParams()

        If loaded Then

            loaded = False 'prevent recalculation due to edit field event procedures

            jb = New DWSIM.Utilities.Hypos.Methods.Joback
            methods2 = New DWSIM.SimulationObjects.PropertyPackages.Auxiliary.PROPS
            methods = New DWSIM.Utilities.Hypos.Methods.HYP()

            'get UNIFAC group amounts
            Dim vn As New ArrayList
            For Each row As DataGridViewRow In GridUNIFAC.Rows
                If Not row.Cells(0).Value Is Nothing Then
                    vn.Add(Integer.Parse(row.Cells(0).Value))
                Else
                    vn.Add(0)
                End If
            Next
            Dim vnd As Int32() = vn.ToArray(Type.GetType("System.Int32"))

            'get Joback group amounts
            Dim JC, UC, GC As Integer 'JC=UnifacGroupCount; UC=UnifacGroupCount; GC=GlobalGroupCount
            Dim JG As New ArrayList


            PureUNIFACCompound = True
            For Each r As DataGridViewRow In Me.GridJoback.Rows
                JC = r.Cells(3).Value 'additional Joback groups
                UC = r.Cells(2).Value 'Joback groups from UNIFAC subgoups
                JG.Add(JC + UC)
                GC += JC + UC
                If JC > 0 Then PureUNIFACCompound = False
            Next
            Dim JGD As Int32() = JG.ToArray(Type.GetType("System.Int32"))

            'Calculate atoms list
            Dim ACL As New System.Collections.Generic.Dictionary(Of String, Integer) ' => Atom Count List
            Dim Sy As String 'Element Symbol
            Dim AC As Integer 'Atom count
            Dim Formula As String
            Dim SpecialDefinition As Boolean = False 'special definition -> no joback calculation possible

            ACL = jb.GetAtomCountList(JGD) 'Atoms from Joback groups

            'add additional atoms from datatable
            For Each r As DataGridViewRow In Me.AddAtomDataGrid.Rows
                Sy = r.Cells(0).Value
                AC = r.Cells(1).Value
                If AC > 0 Then
                    PureUNIFACCompound = False
                    SpecialDefinition = True
                    If Not ACL.ContainsKey(Sy) Then
                        ACL.Add(Sy, AC)
                    Else
                        ACL.Item(Sy) = ACL.Item(Sy) + AC
                    End If
                End If
            Next

            'add atoms from special UNIFAC groups to list where no Joback subgroup is defined -> e.g. silanes
            Dim i, n, k, AtomCount As Integer
            Dim s, AtomTypeCount, AtomName As String
            For i = 2 To UNIFAClines.Length - 1
                s = UNIFAClines(i).Split(",")(9)
                If Not s = "" And vnd(i - 2) > 0 Then
                    SpecialDefinition = True
                    n = 0
                    For k = 0 To s.Length - 1 'count atom types in group
                        If s.Chars(k) = "/" Then n += 1
                    Next
                    For k = 0 To n 'insert atoms in list
                        AtomTypeCount = s.Split("/")(k) 'get type and count of Atom
                        AtomName = AtomTypeCount.Split(":")(0)
                        AtomCount = AtomTypeCount.Split(":")(1) * vnd(i - 2)

                        If Not ACL.ContainsKey(AtomName) Then
                            ACL.Add(AtomName, AtomCount)
                        Else
                            ACL.Item(AtomName) = ACL.Item(AtomName) + AtomCount
                        End If
                    Next
                End If
            Next

            'Calculation of chemical formula
            Me.AtomDataGrid.Rows.Clear()
            Formula = ""

            For Each A As String In ACL.Keys
                Formula = Formula & A
                AC = ACL.Item(A)
                If AC > 1 Then Formula = Formula & AC
                Me.AtomDataGrid.Rows.Add(New Object() {A, AC})
            Next

            Me.TextBoxFormula.Text = Formula

            Dim Tb, Tc, Pc, Vc, MM, w, Hvb As Double

            If ACL.Count > 0 Then
                MM = jb.CalcMW(ACL)
                If CheckBoxMW.Checked Then Me.TextBoxMW.Text = MM
                MM = Me.TextBoxMW.Text
            Else
                If CheckBoxMW.Checked Then Me.TextBoxMW.Text = ""
            End If

            If GC > 0 And Not SpecialDefinition Then
                'boiling point
                Tb = jb.CalcTb(JGD)
                If CheckBoxNBP.Checked Then Me.TextBoxNBP.Text = cv.ConverterDoSI(su.spmp_temperature, Tb)
                If CheckValidDF(Me.TextBoxNBP.Text) Then
                    Tb = cv.ConverterParaSI(su.spmp_temperature, Me.TextBoxNBP.Text)
                Else : Tb = -1
                End If

                'critical temperature
                If Tb > 0 Then
                    Tc = jb.CalcTc(Tb, JGD)
                    If CheckBoxTc.Checked Then Me.TextBoxTc.Text = cv.ConverterDoSI(su.spmp_temperature, Tc)
                    If CheckValidDF(Me.TextBoxTc.Text) Then
                        Tc = cv.ConverterParaSI(su.spmp_temperature, Me.TextBoxTc.Text)
                    Else : Tc = -1
                    End If
                Else
                    Tc = -1
                    If CheckBoxTc.Checked Then Me.TextBoxTc.Text = ""
                End If

                'critical pressure
                Pc = jb.CalcPc(JGD)
                If CheckBoxPc.Checked Then Me.TextBoxPc.Text = cv.ConverterDoSI(su.spmp_pressure, Pc)
                If CheckValidDF(Me.TextBoxPc.Text) Then
                    Pc = cv.ConverterParaSI(su.spmp_pressure, Me.TextBoxPc.Text)
                Else : Pc = -1
                End If

                'critical compressibility
                If Tc > 0 And Pc > 0 Then
                    Vc = jb.CalcVc(JGD)
                    If CheckBoxZc.Checked Then Me.TextBoxZc.Text = Pc * Vc / Tc / 8.314 / 1000
                    If CheckBoxZRa.Checked Then Me.TextBoxZRa.Text = Pc * Vc / Tc / 8.314 / 1000
                Else
                    If CheckBoxZc.Checked Then Me.TextBoxZc.Text = ""
                    If CheckBoxZRa.Checked Then Me.TextBoxZRa.Text = ""
                End If

                'acentric factor
                If Tb > 0 And Tc > 0 And Pc > 0 Then
                    w = (-Math.Log(Pc / 100000) - 5.92714 + 6.09648 / (Tb / Tc) + 1.28862 * Math.Log(Tb / Tc) - 0.169347 * (Tb / Tc) ^ 6) / (15.2518 - 15.6875 / (Tb / Tc) - 13.4721 * Math.Log(Tb / Tc) + 0.43577 * (Tb / Tc) ^ 6)
                    If CheckBoxAF.Checked Then Me.TextBoxAF.Text = w
                    If CheckValidDF(Me.TextBoxAF.Text) Then
                        w = Me.TextBoxAF.Text
                    Else
                        w = -1
                    End If
                Else
                    w = -1
                    If CheckBoxAF.Checked Then Me.TextBoxAF.Text = ""
                End If
                

                If CheckBoxDHF.Checked Then Me.TextBoxDHF.Text = cv.ConverterDoSI(su.spmp_enthalpy, jb.CalcDHf(JGD) / MM)
                If CheckBoxDGF.Checked Then Me.TextBoxDGF.Text = cv.ConverterDoSI(su.spmp_enthalpy, jb.CalcDGf(JGD) / MM)
                If CheckBoxCSAF.Checked Then
                    If w > 0 Then
                        Me.TextBoxCSAF.Text = w
                    Else
                        Me.TextBoxCSAF.Text = ""
                    End If
                End If

                If Tc > 0 And Pc > 0 And Tb > 0 And MM > 0 And w > 0 Then
                    Hvb = methods.DHvb_Vetere(Tc, Pc, Tb) / MM
                    If CheckBoxCSSP.Checked Then Me.TextBoxCSSP.Text = ((Hvb * MM - 8.314 * Tb) * 238.846 * methods2.liq_dens_rackett(Tb, Tc, Pc, w, MM) / MM / 1000000.0) ^ 0.5
                    If CheckBoxCSLV.Checked Then Me.TextBoxCSLV.Text = 1 / methods2.liq_dens_rackett(Tb, Tc, Pc, w, MM) * MM / 1000 * 1000000.0
                Else
                    If CheckBoxCSSP.Checked Then Me.TextBoxCSSP.Text = ""
                    If CheckBoxCSLV.Checked Then Me.TextBoxCSLV.Text = ""
                End If

                If CheckBoxMeltingTemp.Checked Then Me.TextBoxMeltingTemp.Text = cv.ConverterDoSI(su.spmp_temperature, jb.CalcTf(JGD)) 'melting temperature - temperature of fusion
                If CheckBoxEnthOfFusion.Checked Then Me.TextBoxEnthOfFusion.Text = cv.ConverterDoSI(su.spmp_enthalpy, jb.CalcHf(JGD) / MM) 'enthalpy of fusion

                If rbEstimateCPIG.Checked Then
                    Dim result As Object = RegressData(1, True)

                    For Each it As Object In cbEqCPIG.Items
                        If it.ToString.Split(":")(0) = 5 Then
                            cbEqCPIG.SelectedIndex = cbEqCPIG.Items.IndexOf(it)
                            Exit For
                        End If
                    Next

                    tbCPIG_A.Text = result(0)(0) * 1000
                    tbCPIG_B.Text = result(0)(1) * 1000
                    tbCPIG_C.Text = result(0)(2) * 1000
                    tbCPIG_D.Text = result(0)(3) * 1000
                    tbCPIG_E.Text = result(0)(4) * 1000
                End If
            Else
                If CheckBoxNBP.Checked Then Me.TextBoxNBP.Text = ""
                If CheckBoxTc.Checked Then Me.TextBoxTc.Text = ""
                If CheckBoxPc.Checked Then Me.TextBoxPc.Text = ""
                If CheckBoxZc.Checked Then Me.TextBoxZc.Text = ""
                If CheckBoxZRa.Checked Then Me.TextBoxZRa.Text = ""
                If CheckBoxAF.Checked Then Me.TextBoxAF.Text = ""
                If CheckBoxDHF.Checked Then Me.TextBoxDHF.Text = ""
                If CheckBoxDGF.Checked Then Me.TextBoxDGF.Text = ""
                If CheckBoxCSAF.Checked Then Me.TextBoxCSAF.Text = ""
                If CheckBoxCSSP.Checked Then Me.TextBoxCSSP.Text = ""
                If CheckBoxCSLV.Checked Then Me.TextBoxCSLV.Text = ""
                If CheckBoxMeltingTemp.Checked Then Me.TextBoxMeltingTemp.Text = ""
                If CheckBoxEnthOfFusion.Checked Then Me.TextBoxEnthOfFusion.Text = ""
                If rbEstimateCPIG.Checked Then
                    tbCPIG_A.Text = ""
                    tbCPIG_B.Text = ""
                    tbCPIG_C.Text = ""
                    tbCPIG_D.Text = ""
                    tbCPIG_E.Text = ""
                End If
            End If

            loaded = True 'reset old status
        End If
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
            Next
            loaded = False
            FillUnifacSubGroups()
            loaded = True
            CalcJobackParams()
        End If
        BothSaveStatusModified(sender, e)
    End Sub

    Private Sub Grid_CellValueChanged(ByVal sender As System.Object, ByVal e As System.Windows.Forms.DataGridViewCellEventArgs) Handles GridJoback.CellValueChanged, AddAtomDataGrid.CellValueChanged
        If loaded Then
            CalcJobackParams()
            BothSaveStatusModified(sender, e)
        End If
    End Sub

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
            lblMeltingTemp.Text = .spmp_temperature
            lblEnthOfFusion.Text = .spmp_enthalpy
            Me.GridExpDataPVAP.Columns(0).HeaderText = "T [" & su.spmp_temperature & "]"
            Me.GridExpDataCPIG.Columns(0).HeaderText = "T [" & su.spmp_temperature & "]"
            Me.GridExpDataLIQDENS.Columns(0).HeaderText = "T [" & su.spmp_temperature & "]"
            Me.GridExpDataLIQVISC.Columns(0).HeaderText = "T [" & su.spmp_temperature & "]"
            Me.GridExpDataCpS.Columns(0).HeaderText = "T [" & su.spmp_temperature & "]"
            Me.GridExpDataRoS.Columns(0).HeaderText = "T [" & su.spmp_temperature & "]"

            Me.GridExpDataPVAP.Columns(1).HeaderText = "Pvap [" & su.spmp_pressure & "]"
            Me.GridExpDataCPIG.Columns(1).HeaderText = "Cpig [" & su.spmp_heatCapacityCp & "]"
            Me.GridExpDataLIQDENS.Columns(1).HeaderText = "Dens [" & su.spmp_density & "]"
            Me.GridExpDataLIQVISC.Columns(1).HeaderText = "Visc [" & su.spmp_viscosity & "]"
            Me.GridExpDataCpS.Columns(1).HeaderText = "CpS [" & su.spmp_heatCapacityCp & "]"
            Me.GridExpDataRoS.Columns(1).HeaderText = "DensS [" & su.spmp_density & "]"
        End With
    End Sub

    Public Function RegressData(ByVal tipo As Integer, ByVal calcular As Boolean)

        Dim obj As Object = Nothing
        Dim lmfit As New DWSIM.Utilities.PetroleumCharacterization.LMFit

        jb = New DWSIM.Utilities.Hypos.Methods.Joback

        m_props = New PROPS()

        Dim c_pv(4), c_cp(4), c_vi(4), c_de(4), c_sd(4), c_scp(4) As Double
        Dim r_cp, r_vi, r_pv, r_de, r_sd, r_scp, n_cp, n_pv, n_de, n_vi, n_sd, n_scp As Double

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

        c_sd(0) = 11.5
        c_sd(1) = -0.01
        c_sd(2) = 0.0#
        c_sd(3) = 0.0#
        c_sd(4) = 0.0#

        c_scp(0) = 0
        c_scp(1) = 0.1
        c_scp(2) = 0
        c_scp(3) = 0
        c_scp(4) = 0

        Select Case tipo
            Case 0
                'regressão dos dados
                obj = lmfit.GetCoeffs(CopyToVector(mycase.DataPVAP, 0), CopyToVector(mycase.DataPVAP, 1), c_pv.Clone, DWSIM.Utilities.PetroleumCharacterization.LMFit.FitType.Pvap, 0.0000000001, 0.0000000001, 0.0000000001, 10000)
                c_pv = obj(0)
                r_pv = obj(2)
                n_pv = obj(3)
            Case 1
                If calcular Then
                    'get Joback group amounts
                    Dim JC, UC As Integer
                    Dim JG As New ArrayList

                    For Each r As DataGridViewRow In Me.GridJoback.Rows
                        JC = r.Cells(3).Value 'additional Joback groups
                        UC = r.Cells(2).Value 'Joback groups from UNIFAC subgoups
                        JG.Add(JC + UC)
                    Next
                    Dim JGD As Int32() = JG.ToArray(Type.GetType("System.Int32"))

                    c_cp(0) = Me.jb.CalcCpA(JGD)
                    c_cp(1) = Me.jb.CalcCpB(JGD)
                    c_cp(2) = Me.jb.CalcCpC(JGD)
                    c_cp(3) = Me.jb.CalcCpD(JGD)
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

            Case 4
                'regressão dos dados
                obj = lmfit.GetCoeffs(CopyToVector(mycase.DataRoS, 0), CopyToVector(mycase.DataRoS, 1), c_sd, DWSIM.Utilities.PetroleumCharacterization.LMFit.FitType.Cp, 0.0000000000001, 0.0000000000001, 0.0000000000001, 10000)
                c_sd = obj(0)
                r_sd = obj(2)
                n_sd = obj(3)

            Case 5
                'regressão dos dados
                obj = lmfit.GetCoeffs(CopyToVector(mycase.DataCpS, 0), CopyToVector(mycase.DataCpS, 1), c_scp, DWSIM.Utilities.PetroleumCharacterization.LMFit.FitType.Cp, 0.0000000001, 0.0000000001, 0.0000000001, 10000)
                c_scp = obj(0)
                r_scp = obj(2)
                n_scp = obj(3)
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
            Case 4
                Return New Object() {c_sd, r_sd, n_sd, obj(1)}
            Case 5
                Return New Object() {c_scp, r_scp, n_scp, obj(1)}
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

            For Each it As Object In cbEqPVAP.Items
                If it.ToString.Split(":")(0) = .VaporPressureEquation Then
                    cbEqPVAP.SelectedIndex = cbEqPVAP.Items.IndexOf(it)
                    Exit For
                End If
            Next

            .Vapor_Pressure_Constant_A = result(0)(0)
            .Vapor_Pressure_Constant_B = result(0)(1)
            .Vapor_Pressure_Constant_C = result(0)(2)
            .Vapor_Pressure_Constant_D = result(0)(3)
            .Vapor_Pressure_Constant_E = result(0)(4)

            tbPVAP_A.Text = .Vapor_Pressure_Constant_A
            tbPVAP_B.Text = .Vapor_Pressure_Constant_B
            tbPVAP_C.Text = .Vapor_Pressure_Constant_C
            tbPVAP_D.Text = .Vapor_Pressure_Constant_D
            tbPVAP_E.Text = .Vapor_Pressure_Constant_E
        End With
    End Sub

    Private Sub btnRegressSolidCp_Click(sender As System.Object, e As System.EventArgs) Handles btnRegressSolidCp.Click
        Dim MW As Double = Me.TextBoxMW.Text
        mycase.DataCpS.Clear()
        For Each row As DataGridViewRow In Me.GridExpDataCpS.Rows
            If row.Index < Me.GridExpDataCpS.Rows.Count - 1 Then mycase.DataCpS.Add(New Double() {cv.ConverterParaSI(su.spmp_temperature, row.Cells(0).Value), cv.ConverterParaSI(su.spmp_heatCapacityCp, row.Cells(1).Value) * MW / 1000})
        Next
        Dim result As Object = RegressData(5, False)
        tbStatusSolidCp.Text = GetInfo(result(3))

        With mycase.cp
            .SolidHeatCapacityEquation = 5

            For Each it As Object In cbEqCpS.Items
                If it.ToString.Split(":")(0) = .SolidHeatCapacityEquation Then
                    cbEqCpS.SelectedIndex = cbEqCpS.Items.IndexOf(it)
                    Exit For
                End If
            Next

            .Solid_Heat_Capacity_Const_A = result(0)(0)
            .Solid_Heat_Capacity_Const_B = result(0)(1)
            .Solid_Heat_Capacity_Const_C = result(0)(2)
            .Solid_Heat_Capacity_Const_D = result(0)(3)
            .Solid_Heat_Capacity_Const_E = result(0)(4)

            tbCpS_A.Text = .Solid_Heat_Capacity_Const_A
            tbCpS_B.Text = .Solid_Heat_Capacity_Const_B
            tbCpS_C.Text = .Solid_Heat_Capacity_Const_C
            tbCpS_D.Text = .Solid_Heat_Capacity_Const_D
            tbCpS_E.Text = .Solid_Heat_Capacity_Const_E

        End With
    End Sub
    Private Sub btnRegressSolidDens_Click(ByVal sender As System.Object, ByVal e As System.EventArgs) Handles btnRegressSolidDens.Click
        Dim MW As Double = Me.TextBoxMW.Text
        mycase.DataRoS.Clear()
        For Each row As DataGridViewRow In Me.GridExpDataRoS.Rows
            If row.Index < Me.GridExpDataRoS.Rows.Count - 1 Then mycase.DataRoS.Add(New Double() {cv.ConverterParaSI(su.spmp_temperature, row.Cells(0).Value), cv.ConverterParaSI(su.spmp_density, row.Cells(1).Value) / MW})
        Next
        Dim result As Object = RegressData(4, False)
        tbStatusSolidDens.Text = GetInfo(result(3))

        With mycase.cp
            .SolidDensityEquation = 5

            For Each it As Object In cbEqSolidDENS.Items
                If it.ToString.Split(":")(0) = .SolidDensityEquation Then
                    cbEqSolidDENS.SelectedIndex = cbEqSolidDENS.Items.IndexOf(it)
                    Exit For
                End If
            Next

            .Solid_Density_Const_A = result(0)(0)
            .Solid_Density_Const_B = result(0)(1)
            .Solid_Density_Const_C = result(0)(2)
            .Solid_Density_Const_D = result(0)(3)
            .Solid_Density_Const_E = result(0)(4)

            tbRoS_A.Text = .Solid_Density_Const_A
            tbRoS_B.Text = .Solid_Density_Const_B
            tbRoS_C.Text = .Solid_Density_Const_C
            tbRoS_D.Text = .Solid_Density_Const_D
            tbRoS_E.Text = .Solid_Density_Const_E

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

            For Each it As Object In cbEqCPIG.Items
                If it.ToString.Split(":")(0) = .IdealgasCpEquation Then
                    cbEqCPIG.SelectedIndex = cbEqCPIG.Items.IndexOf(it)
                    Exit For
                End If
            Next

            .Ideal_Gas_Heat_Capacity_Const_A = result(0)(0) * 1000
            .Ideal_Gas_Heat_Capacity_Const_B = result(0)(1) * 1000
            .Ideal_Gas_Heat_Capacity_Const_C = result(0)(2) * 1000
            .Ideal_Gas_Heat_Capacity_Const_D = result(0)(3) * 1000
            .Ideal_Gas_Heat_Capacity_Const_E = result(0)(4) * 1000

            tbCPIG_A.Text = .Ideal_Gas_Heat_Capacity_Const_A
            tbCPIG_B.Text = .Ideal_Gas_Heat_Capacity_Const_B
            tbCPIG_C.Text = .Ideal_Gas_Heat_Capacity_Const_C
            tbCPIG_D.Text = .Ideal_Gas_Heat_Capacity_Const_D
            tbCPIG_E.Text = .Ideal_Gas_Heat_Capacity_Const_E

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

            For Each it As Object In cbEqLIQDENS.Items
                If it.ToString.Split(":")(0) = .LiquidDensityEquation Then
                    cbEqLIQDENS.SelectedIndex = cbEqLIQDENS.Items.IndexOf(it)
                    Exit For
                End If
            Next

            .Liquid_Density_Const_A = result(0)(0)
            .Liquid_Density_Const_B = result(0)(1)
            .Liquid_Density_Const_C = result(0)(2)
            .Liquid_Density_Const_D = result(0)(3)
            .Liquid_Density_Const_E = result(0)(4)

            tbLIQDENS_A.Text = .Liquid_Density_Const_A
            tbLIQDENS_B.Text = .Liquid_Density_Const_B
            tbLIQDENS_C.Text = .Liquid_Density_Const_C
            tbLIQDENS_D.Text = .Liquid_Density_Const_D
            tbLIQDENS_E.Text = .Liquid_Density_Const_E

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

            For Each it As Object In cbEqLIQVISC.Items
                If it.ToString.Split(":")(0) = .LiquidViscosityEquation Then
                    cbEqLIQVISC.SelectedIndex = cbEqLIQVISC.Items.IndexOf(it)
                    Exit For
                End If
            Next

            .Liquid_Viscosity_Const_A = result(0)(0)
            .Liquid_Viscosity_Const_B = result(0)(1)
            .Liquid_Viscosity_Const_C = result(0)(2)
            .Liquid_Viscosity_Const_D = result(0)(3)
            .Liquid_Viscosity_Const_E = result(0)(4)

            tbLIQVISC_A.Text = .Liquid_Viscosity_Const_A
            tbLIQVISC_B.Text = .Liquid_Viscosity_Const_B
            tbLIQVISC_C.Text = .Liquid_Viscosity_Const_C
            tbLIQVISC_D.Text = .Liquid_Viscosity_Const_D
            tbLIQVISC_E.Text = .Liquid_Viscosity_Const_E

        End With
    End Sub

  
    Private Sub GridExpData_KeyDown1(ByVal sender As Object, ByVal e As System.Windows.Forms.KeyEventArgs) Handles GridExpDataRoS.KeyDown, GridExpDataCpS.KeyDown

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
            SetCompCreatorSaveStatus(False)
        End If
    End Sub

    Private Sub btnCreateNewDB_Click(ByVal sender As System.Object, ByVal e As System.EventArgs) Handles btnCreateNewDB.Click
        If Me.DBOpenDlg.ShowDialog(Me) = Windows.Forms.DialogResult.OK Then
            If Not File.Exists(Me.DBOpenDlg.FileName) Then File.Create(Me.DBOpenDlg.FileName)
            Me.tbDBPath.Text = Me.DBOpenDlg.FileName
            SetCompCreatorSaveStatus(False)
        End If
    End Sub

    Private Sub rbEstimatePVAP_CheckedChanged(ByVal sender As System.Object, ByVal e As System.EventArgs)
        If rbEstimatePVAP.Checked Then
            mycase.cp.VaporPressureEquation = 0
            tbStatusPVAP.Text = "OK"
        End If
    End Sub

    Private Sub rbEstimateCPIG_CheckedChanged(ByVal sender As System.Object, ByVal e As System.EventArgs)
        If rbEstimateCPIG.Checked Then
            mycase.cp.IdealgasCpEquation = 0
            tbStatusCPIG.Text = "OK"
        End If
    End Sub

    Private Sub rbEstimateLIQDENS_CheckedChanged(ByVal sender As System.Object, ByVal e As System.EventArgs)
        If rbEstimateLIQDENS.Checked Then
            mycase.cp.LiquidDensityEquation = 0
            tbStatusLIQDENS.Text = "OK"
        End If
    End Sub

    Private Sub rbEstimateLIQVISC_CheckedChanged(ByVal sender As System.Object, ByVal e As System.EventArgs)
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

    Private Sub btnViewSolidCp_Click(sender As System.Object, e As System.EventArgs) Handles btnViewSolidCp.Click
        If tbStatusSolidCp.Text = "" And rbRegressSolidCp.Checked Then
            MessageBox.Show(DWSIM.App.GetLocalString("NoRegressionAvailable"), DWSIM.App.GetLocalString("Erro"), MessageBoxButtons.OK, MessageBoxIcon.Error)
            Exit Sub
        End If

        Dim mytext As New System.Text.StringBuilder
        mytext.AppendLine("Data Points")
        mytext.AppendLine("x" & vbTab & "yEXP" & vbTab & "yCALC")
        Dim px, py1, py2 As New ArrayList, x, y1, y2 As Double
        For Each d As Double() In mycase.DataCpS
            x = cv.ConverterDoSI(su.spmp_temperature, d(0))
            px.Add(x)
            y1 = cv.ConverterDoSI(su.spmp_heatCapacityCp, d(1))
            py1.Add(y1)
            Dim pp As New DWSIM.SimulationObjects.PropertyPackages.RaoultPropertyPackage(False)
            With mycase.cp
                y2 = cv.ConverterDoSI(su.spmp_heatCapacityCp, pp.CalcCSTDepProp(.SolidHeatCapacityEquation, .Solid_Heat_Capacity_Const_A, .Solid_Heat_Capacity_Const_B, .Solid_Heat_Capacity_Const_C, .Solid_Heat_Capacity_Const_D, .Solid_Heat_Capacity_Const_E, d(0), 0))
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
            .ytitle = "Cp [ " & su.spmp_heatCapacityCp & " ]"
            .xtitle = "T [ " & su.spmp_temperature & " ]"
            .ycurvetypes = New ArrayList(New Integer() {1, 3})
            .title = "Solid Heat Capacity Fitting Results"
            .ShowDialog(Me)
        End With
    End Sub
    Private Sub btnViewSolidDens_Click(sender As System.Object, e As System.EventArgs) Handles btnViewSolidDens.Click

        If tbStatusSolidDens.Text = "" And rbRegressSolidDens.Checked Then
            MessageBox.Show(DWSIM.App.GetLocalString("NoRegressionAvailable"), DWSIM.App.GetLocalString("Erro"), MessageBoxButtons.OK, MessageBoxIcon.Error)
            Exit Sub
        End If

        Dim mytext As New System.Text.StringBuilder
        mytext.AppendLine("Data Points")
        mytext.AppendLine("x" & vbTab & "yEXP" & vbTab & "yCALC")
        Dim px, py1, py2 As New ArrayList, x, y1, y2 As Double
        For Each d As Double() In mycase.DataRoS
            x = cv.ConverterDoSI(su.spmp_temperature, d(0))
            px.Add(x)
            y1 = cv.ConverterDoSI(su.spmp_density, d(1)) * mycase.cp.Molar_Weight
            py1.Add(y1)
            Dim pp As New DWSIM.SimulationObjects.PropertyPackages.RaoultPropertyPackage(False)
            With mycase.cp
                y2 = cv.ConverterDoSI(su.spmp_density, pp.CalcCSTDepProp(.SolidDensityEquation, .Solid_Density_Const_A, .Solid_Density_Const_B, .Solid_Density_Const_C, .Solid_Density_Const_D, .Solid_Density_Const_E, d(0), 0) * .Molar_Weight)
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
            .ytitle = "Rho / " & su.spmp_density
            .xtitle = "T / " & su.spmp_temperature
            .ycurvetypes = New ArrayList(New Integer() {1, 3})
            .title = "Solid Density Fitting Results"
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

    Private Sub rbCoeffPVAP_CheckedChanged(ByVal sender As System.Object, ByVal e As System.EventArgs)
        mycase.EqPVAP = rbCoeffPVAP.Checked
    End Sub

    Private Sub rbCoeffCPIG_CheckedChanged(ByVal sender As System.Object, ByVal e As System.EventArgs)
        mycase.EqCPIG = rbCoeffCPIG.Checked
    End Sub

    Private Sub rbCoeffLIQDENS_CheckedChanged(ByVal sender As System.Object, ByVal e As System.EventArgs)
        mycase.EqLDENS = rbCoeffLIQDENS.Checked
    End Sub

    Private Sub rbCoeffLIQVISC_CheckedChanged(ByVal sender As System.Object, ByVal e As System.EventArgs)
        mycase.EqLVISC = rbCoeffLIQVISC.Checked
    End Sub
    Private Sub cbEqCpS_SelectedIndexChanged(ByVal sender As System.Object, ByVal e As System.EventArgs)
        If mycase.EqCpS Then mycase.cp.SolidHeatCapacityEquation = cbEqCpS.SelectedItem.ToString.Split(":")(0)
        If loaded Then
            SetCompCreatorSaveStatus(False)
            SetUserDBSaveStatus(False)
        End If
    End Sub
    Private Sub cbEqSolidDENS_SelectedIndexChanged(ByVal sender As System.Object, ByVal e As System.EventArgs)
        If mycase.EqSDens Then mycase.cp.SolidDensityEquation = cbEqSolidDENS.SelectedItem.ToString.Split(":")(0)
        If loaded Then
            SetCompCreatorSaveStatus(False)
            SetUserDBSaveStatus(False)
        End If
    End Sub
    Private Sub cbEqPVAP_SelectedIndexChanged(ByVal sender As System.Object, ByVal e As System.EventArgs) Handles cbEqPVAP.SelectedIndexChanged
        If mycase.EqPVAP Then mycase.cp.VaporPressureEquation = cbEqPVAP.SelectedItem.ToString.Split(":")(0)
        If loaded Then
            SetCompCreatorSaveStatus(False)
            SetUserDBSaveStatus(False)
        End If
    End Sub

    Private Sub cbEqCPIG_SelectedIndexChanged(ByVal sender As System.Object, ByVal e As System.EventArgs) Handles cbEqCPIG.SelectedIndexChanged
        If mycase.EqCPIG Then mycase.cp.IdealgasCpEquation = cbEqCPIG.SelectedItem.ToString.Split(":")(0)
        If loaded Then
            SetCompCreatorSaveStatus(False)
            SetUserDBSaveStatus(False)
        End If
    End Sub

    Private Sub cbEqLIQDENS_SelectedIndexChanged(ByVal sender As System.Object, ByVal e As System.EventArgs) Handles cbEqLIQDENS.SelectedIndexChanged
        If mycase.EqLDENS Then mycase.cp.LiquidDensityEquation = cbEqLIQDENS.SelectedItem.ToString.Split(":")(0)
        If loaded Then
            SetCompCreatorSaveStatus(False)
            SetUserDBSaveStatus(False)
        End If
    End Sub

    Private Sub cbEqLIQVISC_SelectedIndexChanged(ByVal sender As System.Object, ByVal e As System.EventArgs) Handles cbEqLIQVISC.SelectedIndexChanged
        If mycase.EqLVISC Then mycase.cp.LiquidViscosityEquation = cbEqLIQVISC.SelectedItem.ToString.Split(":")(0)
        If loaded Then
            SetCompCreatorSaveStatus(False)
            SetUserDBSaveStatus(False)
        End If
    End Sub

    Private Sub CheckBoxTc_CheckedChanged(ByVal sender As System.Object, ByVal e As System.EventArgs) Handles CheckBoxTc.CheckedChanged
        If CheckBoxTc.Checked = True Then
            TextBoxTc.Enabled = False
            CalcJobackParams()
        Else
            TextBoxTc.Enabled = True
        End If
        If loaded Then
            SetCompCreatorSaveStatus(False)
        End If
    End Sub

    Private Sub CheckBoxPc_CheckedChanged(ByVal sender As System.Object, ByVal e As System.EventArgs) Handles CheckBoxPc.CheckedChanged
        If CheckBoxPc.Checked = True Then
            TextBoxPc.Enabled = False
            CalcJobackParams()
        Else
            TextBoxPc.Enabled = True
        End If
        If loaded Then
            SetCompCreatorSaveStatus(False)
        End If
    End Sub

    Private Sub CheckBoxZc_CheckedChanged(ByVal sender As System.Object, ByVal e As System.EventArgs) Handles CheckBoxZc.CheckedChanged
        If CheckBoxZc.Checked = True Then
            TextBoxZc.Enabled = False
            CalcJobackParams()
        Else
            TextBoxZc.Enabled = True
        End If
        If loaded Then
            SetCompCreatorSaveStatus(False)
        End If
    End Sub

    Private Sub CheckBoxZRa_CheckedChanged(ByVal sender As System.Object, ByVal e As System.EventArgs) Handles CheckBoxZRa.CheckedChanged
        If CheckBoxZRa.Checked = True Then
            TextBoxZRa.Enabled = False
            CalcJobackParams()
        Else
            TextBoxZRa.Enabled = True
        End If
        If loaded Then
            SetCompCreatorSaveStatus(False)
        End If
    End Sub

    Private Sub CheckBoxAF_CheckedChanged(ByVal sender As System.Object, ByVal e As System.EventArgs) Handles CheckBoxAF.CheckedChanged
        If CheckBoxAF.Checked = True Then
            TextBoxAF.Enabled = False
            CalcJobackParams()
        Else
            TextBoxAF.Enabled = True
        End If
        If loaded Then
            SetCompCreatorSaveStatus(False)
        End If
    End Sub

    Private Sub CheckBoxMW_CheckedChanged(ByVal sender As System.Object, ByVal e As System.EventArgs) Handles CheckBoxMW.CheckedChanged
        If CheckBoxMW.Checked = True Then
            TextBoxMW.Enabled = False
            CalcJobackParams()
        Else
            TextBoxMW.Enabled = True
        End If
        If loaded Then
            SetCompCreatorSaveStatus(False)
        End If
    End Sub

    Private Sub CheckBoxDHF_CheckedChanged(ByVal sender As System.Object, ByVal e As System.EventArgs) Handles CheckBoxDHF.CheckedChanged
        If CheckBoxDHF.Checked = True Then
            TextBoxDHF.Enabled = False
            CalcJobackParams()
        Else
            TextBoxDHF.Enabled = True
        End If
        If loaded Then
            SetCompCreatorSaveStatus(False)
        End If
    End Sub

    Private Sub CheckBoxDGF_CheckedChanged(ByVal sender As System.Object, ByVal e As System.EventArgs) Handles CheckBoxDGF.CheckedChanged
        If CheckBoxDGF.Checked = True Then
            TextBoxDGF.Enabled = False
            CalcJobackParams()
        Else
            TextBoxDGF.Enabled = True
        End If
        If loaded Then
            SetCompCreatorSaveStatus(False)
        End If
    End Sub

    Private Sub CheckBoxNBP_CheckedChanged(ByVal sender As System.Object, ByVal e As System.EventArgs) Handles CheckBoxNBP.CheckedChanged
        If CheckBoxNBP.Checked = True Then
            TextBoxNBP.Enabled = False
            CalcJobackParams()
        Else
            TextBoxNBP.Enabled = True
        End If
        If loaded Then
            SetCompCreatorSaveStatus(False)
        End If
    End Sub

    Private Sub CheckBoxCSAF_CheckedChanged(ByVal sender As System.Object, ByVal e As System.EventArgs) Handles CheckBoxCSAF.CheckedChanged
        If CheckBoxCSAF.Checked = True Then
            TextBoxCSAF.Enabled = False
            CalcJobackParams()
        Else
            TextBoxCSAF.Enabled = True
        End If
        If loaded Then
            SetCompCreatorSaveStatus(False)
        End If
    End Sub

    Private Sub CheckBoxCSSP_CheckedChanged(ByVal sender As System.Object, ByVal e As System.EventArgs) Handles CheckBoxCSSP.CheckedChanged
        If CheckBoxCSSP.Checked = True Then
            TextBoxCSSP.Enabled = False
            CalcJobackParams()
        Else
            TextBoxCSSP.Enabled = True
        End If
        If loaded Then
            SetCompCreatorSaveStatus(False)
        End If
    End Sub

    Private Sub CheckBoxCSLV_CheckedChanged(ByVal sender As System.Object, ByVal e As System.EventArgs) Handles CheckBoxCSLV.CheckedChanged
        If CheckBoxCSLV.Checked = True Then
            TextBoxCSLV.Enabled = False
            CalcJobackParams()
        Else
            TextBoxCSLV.Enabled = True
        End If
        If loaded Then
            SetCompCreatorSaveStatus(False)
        End If
    End Sub

    Private Sub CheckBoxMeltingTemp_CheckedChanged(ByVal sender As System.Object, ByVal e As System.EventArgs) Handles CheckBoxMeltingTemp.CheckedChanged
        If CheckBoxMeltingTemp.Checked = True Then
            TextBoxMeltingTemp.Enabled = False
            CalcJobackParams()
        Else
            TextBoxMeltingTemp.Enabled = True
        End If
        If loaded Then
            SetCompCreatorSaveStatus(False)
        End If
    End Sub
    Private Sub CheckBoxEnthOfFusion_CheckedChanged(ByVal sender As System.Object, ByVal e As System.EventArgs) Handles CheckBoxEnthOfFusion.CheckedChanged
        If CheckBoxEnthOfFusion.Checked = True Then
            TextBoxEnthOfFusion.Enabled = False
            CalcJobackParams()
        Else
            TextBoxEnthOfFusion.Enabled = True
        End If
        If loaded Then
            SetCompCreatorSaveStatus(False)
        End If
    End Sub

    Private Sub rb_CheckedChanged(ByVal sender As System.Object, ByVal e As System.EventArgs) Handles rbCoeffCPIG.CheckedChanged, rbCoeffLIQDENS.CheckedChanged, _
                                    rbRegressLIQVISC.CheckedChanged, rbRegressLIQDENS.CheckedChanged, rbEstimateLIQVISC.CheckedChanged, rbEstimateLIQDENS.CheckedChanged, _
                                    rbCoeffLIQVISC.CheckedChanged, rbRegressPVAP.CheckedChanged, rbRegressCPIG.CheckedChanged, rbEstimatePVAP.CheckedChanged, _
                                    rbEstimateCPIG.CheckedChanged, rbCoeffPVAP.CheckedChanged

        If loaded Then
            StoreData()
            SetCompCreatorSaveStatus(False)
            SetUserDBSaveStatus(False)
        End If
    End Sub

    Private Sub FormCompoundCreator_Shown(ByVal sender As Object, ByVal e As System.EventArgs) Handles MyBase.Shown
        loaded = True
    End Sub

    Private Sub cbUnits_SelectedIndexChanged(ByVal sender As Object, ByVal e As System.EventArgs) Handles cbUnits.SelectedIndexChanged
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
            SetCompCreatorSaveStatus(False)
        End If
    End Sub

    Sub RenderSMILES()
        'definition available, render molecule
        Try
            Dim ind As New Indigo()
            Dim mol As IndigoObject = ind.loadMolecule(TextBoxSMILES.Text)
            Dim renderer As New IndigoRenderer(ind)

            With renderer
                ind.setOption("render-image-size", pbRender.Size.Width, pbRender.Size.Height)
                ind.setOption("render-margins", 15, 15)
                ind.setOption("render-coloring", True)
                ind.setOption("render-background-color", Color.White)
            End With

            pbRender.Image = renderer.renderToBitmap(mol)
            btnRenderSMILES.Enabled = False

        Catch ex As Exception
            MessageBox.Show(DWSIM.App.GetLocalString("SMILESRenderError"), DWSIM.App.GetLocalString("Erro"), MessageBoxButtons.OK, MessageBoxIcon.Error)
        End Try
    End Sub

    Private Sub TextBoxSMILES_TextChanged(ByVal sender As System.Object, ByVal e As System.EventArgs) Handles TextBoxSMILES.TextChanged
        btnRenderSMILES.Enabled = True
        If loaded Then
            SetCompCreatorSaveStatus(False)
            SetUserDBSaveStatus(False)
        End If
    End Sub

    Private Sub btnRenderSMILES_Click(ByVal sender As System.Object, ByVal e As System.EventArgs) Handles btnRenderSMILES.Click
        RenderSMILES()
    End Sub

    Private Sub btnSaveToDB_Click(ByVal sender As System.Object, ByVal e As System.EventArgs) Handles btnSaveToDB.Click
        Try
            StoreData()

            'In case of additionalt Joback groups no UNIFAC calculation is possible anymore.
            'Delete groups to prevent wrong calculations.
            If Not PureUNIFACCompound Then
                mycase.cp.UNIFACGroups.Collection.Clear()
                mycase.cp.MODFACGroups.Collection.Clear()
            End If

            DWSIM.Databases.UserDB.AddCompounds(New DWSIM.ClassesBasicasTermodinamica.ConstantProperties() {mycase.cp}, tbDBPath.Text, chkReplaceComps.Checked)
            SetUserDBSaveStatus(True)
        Catch ex As Exception
            MessageBox.Show("Error adding compound to the database: " & ex.Message.ToString, "Error", MessageBoxButtons.OK, MessageBoxIcon.Error)
        End Try
    End Sub
    Private Sub chkReplaceComps_CheckStateChanged(ByVal sender As System.Object, ByVal e As System.EventArgs) Handles chkReplaceComps.CheckStateChanged
        If loaded Then
            SetCompCreatorSaveStatus(False)
        End If
    End Sub
    Private Sub BothSaveStatusModified(ByVal sender As System.Object, ByVal e As System.EventArgs) Handles TextBoxMeltingTemp.TextChanged, _
                TextBoxUNIQUAC_R.TextChanged, TextBoxUNIQUAC_Q.TextChanged, TextBoxZRa.TextChanged, TextBoxZc.TextChanged, TextBoxPCSAFTSigma.TextChanged, _
                TextBoxPCSAFTm.TextChanged, TextBoxPCSAFTEpsilon.TextChanged, TextBoxDHF.TextChanged, TextBoxDGF.TextChanged, _
                TextBoxVTCSRK.TextChanged, TextBoxVTCPR.TextChanged, TextBoxCSSP.TextChanged, TextBoxCSLV.TextChanged, TextBoxCSAF.TextChanged, TextBoxName.TextChanged, _
                 TextBoxID.TextChanged, TextBoxFormula.TextChanged, TextBoxCAS.TextChanged, tbPVAP_D.TextChanged, tbPVAP_C.TextChanged, _
                tbPVAP_B.TextChanged, tbPVAP_A.TextChanged, tbPVAP_E.TextChanged, tbCPIG_E.TextChanged, tbCPIG_D.TextChanged, tbCPIG_C.TextChanged, tbCPIG_B.TextChanged, _
                tbCPIG_A.TextChanged, tbLIQVISC_E.TextChanged, tbLIQVISC_D.TextChanged, tbLIQVISC_C.TextChanged, tbLIQVISC_B.TextChanged, tbLIQVISC_A.TextChanged, _
                tbLIQDENS_E.TextChanged, tbLIQDENS_D.TextChanged, tbLIQDENS_C.TextChanged, tbLIQDENS_B.TextChanged, tbLIQDENS_A.TextChanged
        If loaded Then
            SetCompCreatorSaveStatus(False)
            SetUserDBSaveStatus(False)
        End If
    End Sub

    Private Sub GridExpData_CellValueChanged(ByVal sender As System.Object, ByVal e As System.Windows.Forms.DataGridViewCellEventArgs) Handles GridExpDataPVAP.CellValueChanged, _
                GridExpDataLIQVISC.CellValueChanged, GridExpDataLIQDENS.CellValueChanged, GridExpDataCPIG.CellValueChanged, GridExpDataRoS.CellValueChanged
        If loaded Then
            SetCompCreatorSaveStatus(False)
        End If
    End Sub

    Private Sub TextBoxEnthOfFusion_TextChanged(ByVal sender As System.Object, ByVal e As System.EventArgs) Handles TextBoxEnthOfFusion.TextChanged
        Try
            TextBoxEnthOfFusion2.Text = cv.ConverterParaSI(su.spmp_enthalpy, TextBoxEnthOfFusion.Text) * Me.TextBoxMW.Text / 1000
        Catch ex As Exception
            TextBoxEnthOfFusion2.Text = ""
        End Try
        BothSaveStatusModified(sender, e)
    End Sub

    Private Sub TextBoxChanged_recalc(ByVal sender As System.Object, ByVal e As System.EventArgs) Handles TextBoxNBP.TextChanged, TextBoxMW.TextChanged, TextBoxTc.TextChanged, TextBoxPc.TextChanged, TextBoxAF.TextChanged
        CalcJobackParams()
        BothSaveStatusModified(sender, e)
    End Sub

    Private Sub LinkPubChem_LinkClicked(ByVal sender As System.Object, ByVal e As System.Windows.Forms.LinkLabelLinkClickedEventArgs) Handles LinkPubChem.LinkClicked
        System.Diagnostics.Process.Start("http://pubchem.ncbi.nlm.nih.gov/edit2/index.html")
    End Sub

    Private Sub LinkLabel1_LinkClicked(ByVal sender As System.Object, ByVal e As System.Windows.Forms.LinkLabelLinkClickedEventArgs) Handles LinkLabel1.LinkClicked
        System.Diagnostics.Process.Start("http://webbook.nist.gov/cgi/cbook.cgi?ID=" & TextBoxCAS.Text)
    End Sub

    Private Sub LinkLabel2_LinkClicked(ByVal sender As System.Object, ByVal e As System.Windows.Forms.LinkLabelLinkClickedEventArgs) Handles LinkLabel2.LinkClicked
        System.Diagnostics.Process.Start("http://www.ddbst.com/unifacga.html")
    End Sub
    Private Sub LinkLabel3_LinkClicked(ByVal sender As System.Object, ByVal e As System.Windows.Forms.LinkLabelLinkClickedEventArgs) Handles LinkLabel3.LinkClicked
        System.Diagnostics.Process.Start("http://chemeo.com/")
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
    Public CalcMP As Boolean = True
    Public CalcEM As Boolean = True

    Public RegressPVAP As Boolean = False
    Public RegressCPIG As Boolean = False
    Public RegressLVISC As Boolean = False
    Public RegressLDENS As Boolean = False
    Public RegressCpS As Boolean = False
    Public RegressRoS As Boolean = False

    Public EqPVAP As Boolean = False
    Public EqCPIG As Boolean = False
    Public EqLVISC As Boolean = False
    Public EqLDENS As Boolean = False
    Public EqCpS As Boolean = False
    Public EqSDens As Boolean = False

    Public RegressOKPVAP As Boolean = False
    Public RegressOKCPIG As Boolean = False
    Public RegressOKLVISC As Boolean = False
    Public RegressOKLDENS As Boolean = False
    Public RegressOKCpS As Boolean = False
    Public RegressOKRoS As Boolean = False

    Public ErrorMsgPVAP As String = ""
    Public ErrorMsgCPIG As String = ""
    Public ErrorMsgLVISC As String = ""
    Public ErrorMsgLDENS As String = ""
    Public ErrorMsgCpS As String = ""
    Public ErrorMsgRoS As String = ""

    Public JobackGroups As New ArrayList

    Public DataPVAP As New ArrayList
    Public DataCPIG As New ArrayList
    Public DataLVISC As New ArrayList
    Public DataLDENS As New ArrayList
    Public DataCpS As New ArrayList
    Public DataRoS As New ArrayList
End Class