'    Copyright 2009 Daniel Wagner O. de Medeiros
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

Imports DWSIM.DWSIM.ClassesBasicasTermodinamica

Imports DWSIM.DWSIM.Utilities.Hypos.Methods

Public Class FormCompCreator

    Dim frm As FormFlowsheet
    Public jb As DWSIM.Utilities.Hypos.Methods.Joback
    Public cp As ConstantProperties

    Private Sub FormCompCreator_Load(ByVal sender As System.Object, ByVal e As System.EventArgs) Handles MyBase.Load

        frm = My.Application.ActiveSimulation

        cp = New ConstantProperties

        'Grid UNIFAC/MODFAC
        jb = New DWSIM.Utilities.Hypos.Methods.Joback
        With Me.GridUNIFAC.Rows
            .Clear()
            For Each jg As JobackGroup In jb.Groups.Values
                .Add(New Object() {CInt(0)})
                .Item(.Count - 1).HeaderCell.Value = jg.Group
                .Item(.Count - 1).HeaderCell.Tag = jg.ID
            Next
        End With

    End Sub

    Private Sub Button2_Click(ByVal sender As System.Object, ByVal e As System.EventArgs) Handles Button2.Click

        Dim msgres As DialogResult = MessageBox.Show(DWSIM.App.GetLocalString("AreYouSure"), "DWSIM", MessageBoxButtons.YesNo)

        If msgres = Windows.Forms.DialogResult.Yes Then
            Me.Close()
        End If

    End Sub

    Private Sub Button1_Click(ByVal sender As System.Object, ByVal e As System.EventArgs) Handles Button1.Click
        Try

            With cp

                .Name = Me.TextBoxName.Text
                .CAS_Number = Me.TextBoxCAS.Text
                .Formula = Me.TextBoxFormula.Text
                .Molar_Weight = Me.TextBoxMW.Text

                .Critical_Temperature = Me.TextBoxTc.Text
                .Critical_Pressure = Me.TextBoxPc.Text
                .Critical_Compressibility = Me.TextBoxZc.Text
                .Critical_Volume = 8.314 * .Critical_Compressibility * .Critical_Temperature / .Critical_Pressure
                .Z_Rackett = Me.TextBoxZRa.Text
                .Acentric_Factor = Me.TextBoxAF.Text

                .Vapor_Pressure_Constant_A = Me.TextBoxVPA.Text
                .Vapor_Pressure_Constant_B = Me.TextBoxVPB.Text
                .Vapor_Pressure_Constant_C = Me.TextBoxVPC.Text
                .Vapor_Pressure_Constant_D = Me.TextBoxVPD.Text
                .Vapor_Pressure_Constant_E = Me.TextBoxVPE.Text

                .VaporPressureEquation = Me.ComboBox1.SelectedItem.ToString.Split(":")(0)

                .Ideal_Gas_Heat_Capacity_Const_A = Me.TextBoxHCA.Text
                .Ideal_Gas_Heat_Capacity_Const_B = Me.TextBoxHCB.Text
                .Ideal_Gas_Heat_Capacity_Const_C = Me.TextBoxHCC.Text
                .Ideal_Gas_Heat_Capacity_Const_D = Me.TextBoxHCD.Text
                .Ideal_Gas_Heat_Capacity_Const_E = Me.TextBoxHCE.Text

                .IdealgasCpEquation = Me.ComboBox2.SelectedItem.ToString.Split(":")(0)

                .Liquid_Viscosity_Const_A = Me.TextBoxLVA.Text
                .Liquid_Viscosity_Const_B = Me.TextBoxLVB.Text
                .Liquid_Viscosity_Const_C = Me.TextBoxLVC.Text
                .Liquid_Viscosity_Const_D = Me.TextBoxLVD.Text
                .Liquid_Viscosity_Const_E = Me.TextBoxLVE.Text

                .LiquidViscosityEquation = Me.ComboBox3.SelectedItem.ToString.Split(":")(0)

                .IG_Enthalpy_of_Formation_25C = Me.TextBoxDHF.Text
                .IG_Gibbs_Energy_of_Formation_25C = Me.TextBoxDGF.Text
                .IG_Entropy_of_Formation_25C = (.IG_Enthalpy_of_Formation_25C - .IG_Gibbs_Energy_of_Formation_25C) / 298.15

                .NBP = Me.TextBoxNBP.Text
                .Normal_Boiling_Point = .NBP

                .UNIQUAC_R = CInt(Me.TextBoxUQR.Text)
                .UNIQUAC_Q = CInt(Me.TextBoxUQQ.Text)

                .Chao_Seader_Acentricity = CDbl(Me.TextBoxCSAF.Text)
                .Chao_Seader_Liquid_Molar_Volume = CDbl(Me.TextBoxCSLV.Text)
                .Chao_Seader_Solubility_Parameter = CDbl(Me.TextBoxCSSP.Text)

                .PR_Volume_Translation_Coefficient = CDbl(Me.TextBoxVTCPR.Text)
                .SRK_Volume_Translation_Coefficient = CDbl(Me.TextBoxVTCSRK.Text)

            End With

            If Not frm.Options.NotSelectedComponents.ContainsKey(cp.Name) Then
                frm.Options.NotSelectedComponents.Add(cp.Name, cp)
                frm.FrmStSim1.AddCompToGrid(cp)
            Else
                MessageBox.Show(cp.Name & ": " & DWSIM.App.GetLocalString("CompAlrPresent"))
            End If

        Catch ex As Exception
            MessageBox.Show(DWSIM.App.GetLocalString("ErrorAddingComponent") & vbCrLf & ex.Message)
        End Try

    End Sub

    Private Sub GridUNIFAC_CellValueChanged(ByVal sender As System.Object, ByVal e As System.Windows.Forms.DataGridViewCellEventArgs)
        'get group amounts
        With Me.cp
            For Each r As DataGridViewRow In GridUNIFAC.Rows
                If Not r.Cells(0).Value Is Nothing Then
                    If r.Cells(0).Value <> 0 Then
                        If .UNIFACGroups.Collection.ContainsKey(r.HeaderCell.Value) Then
                            .UNIFACGroups.Collection(r.HeaderCell.Value) = r.Cells(0).Value
                            .MODFACGroups.Collection(r.HeaderCell.Value) = r.Cells(0).Value
                        Else
                            .UNIFACGroups.Collection.Add(r.HeaderCell.Value, r.Cells(0).Value)
                            .MODFACGroups.Collection(r.HeaderCell.Value) = r.Cells(0).Value
                        End If
                        If r.Cells(0).Value = 1 Then
                            .Formula += r.HeaderCell.Value
                        Else
                            .Formula += "(" & r.HeaderCell.Value & ")" & r.Cells(0).Value
                        End If
                    End If
                End If
            Next
        End With
    End Sub

End Class