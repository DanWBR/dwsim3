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

Public Class FormCheckHypData

    Inherits System.Windows.Forms.Form

    Public uni As DWSIM.SimulationObjects.PropertyPackages.Auxiliary.Unifac
    Public frmchild As FormFlowsheet
    Public frm As FormHypGen
    Public frmview As FormPureComp
    Public AddOnly As Boolean = False

    Private Sub FormCheckHypData_Load(ByVal sender As System.Object, ByVal e As System.EventArgs) Handles MyBase.Load

        frm = Me.Owner
        frm.constprop = New DWSIM.ClassesBasicasTermodinamica.ConstantProperties()
        frmchild = My.Application.ActiveSimulation
        uni = New DWSIM.SimulationObjects.PropertyPackages.Auxiliary.Unifac

        'unifac
        If frm.ValUNIFACOK = False Then
            Me.ListView1.Items(0).SubItems(1).Text = DWSIM.App.GetLocalString("Erro")
            Me.ListView1.Items(0).SubItems(2).Text = DWSIM.App.GetLocalString("ErronosgruposUNIFAC")
            'Me.KryptonButton2.Text = "Voltar ao Gerador de Hipotéticos"
            Me.KryptonButton2.Enabled = True
            Me.KryptonButton3.Enabled = False
            Exit Sub
        Else
            Me.ListView1.Items(0).SubItems(1).Text = "OK"
            Me.ListView1.Items(0).SubItems(2).Text = DWSIM.App.GetLocalString("GruposUNIFACOK")
        End If

        'props
        If frm.ValPropsOK = False Then
            Me.ListView1.Items(1).SubItems(1).Text = DWSIM.App.GetLocalString("Erro")
            Me.ListView1.Items(1).SubItems(2).Text = DWSIM.App.GetLocalString("Erronaspropriedades")
            'Me.KryptonButton2.Text = "Voltar ao Gerador de Hipotéticos"
            Me.KryptonButton2.Enabled = True
            Me.KryptonButton3.Enabled = False
            Exit Sub
        Else
            With frm.constprop
                'get group amounts
                For Each r As DataGridViewRow In frm.GridUNIFAC.Rows
                    If Not r.Cells(0).Value Is Nothing Then
                        If r.Cells(0).Value <> 0 Then
                            If .UNIFACGroups.Collection.ContainsKey(r.HeaderCell.Value) Then
                                .UNIFACGroups.Collection(r.HeaderCell.Value) = r.Cells(0).Value
                            Else
                                .UNIFACGroups.Collection.Add(r.HeaderCell.Value, r.Cells(0).Value)
                            End If
                            If .MODFACGroups.Collection.ContainsKey(r.HeaderCell.Value) Then
                                .MODFACGroups.Collection(r.HeaderCell.Value) = r.Cells(0).Value
                            Else
                                .MODFACGroups.Collection.Add(r.HeaderCell.Value, r.Cells(0).Value)
                            End If
                            If r.Cells(0).Value = 1 Then
                                .Formula += r.HeaderCell.Value
                            Else
                                .Formula += "(" & r.HeaderCell.Value & ")" & r.Cells(0).Value
                            End If
                        End If
                    End If
                Next
                .Acentric_Factor = frm.w
                .Critical_Compressibility = frm.Zc
                .Critical_Pressure = frm.Pc
                .Critical_Temperature = frm.Tc
                .Critical_Volume = frm.Vc
                .Dipole_Moment = frm.TextBox2.Text
                Dim Rnd As New Random
                .ID = -1 * Rnd.Next(1000, 9999)
                .Name = frm.TextBox1.Text
                .IG_Enthalpy_of_Formation_25C = frm.Hf
                .IG_Gibbs_Energy_of_Formation_25C = frm.Gf
                .IsHYPO = 1
                .IsPF = 0
                .Molar_Weight = frm.MM
                .Normal_Boiling_Point = frm.Tb
                .IG_Entropy_of_Formation_25C = (.IG_Enthalpy_of_Formation_25C - .IG_Gibbs_Energy_of_Formation_25C) / 298.15
                .Z_Rackett = 0.29056 - 0.08775 * frm.w
                .Chao_Seader_Acentricity = frm.CSw
                .Chao_Seader_Liquid_Molar_Volume = frm.CSlv
                .Chao_Seader_Solubility_Parameter = frm.CSsp
            End With
            Me.ListView1.Items(1).SubItems(1).Text = "OK"
            Me.ListView1.Items(1).SubItems(2).Text = DWSIM.App.GetLocalString("PropriedadesOK")
            'Me.KryptonButton2.Text = "Voltar ao Gerador de Hipotéticos"
            Me.KryptonButton3.Enabled = False
            Me.KryptonButton2.Enabled = True
        End If

        'pvap
        Try
            With frm
                Dim Parameters(4) As Double
                If .RadioButton5.Checked = True Then
                    Dim obj As Object = frm.RegressData(frm.Tc, frm.Pc, frm.w, frm.MM, frm.Hvb, frm.Tb, 0, False)
                    Parameters = obj(0)
                Else
                    Dim obj As Object = frm.RegressData(frm.Tc, frm.Pc, frm.w, frm.MM, frm.Hvb, frm.Tb, 0, True)
                    Parameters = obj(0)
                End If
                With frm.constprop
                    .Vapor_Pressure_Constant_A = Parameters(0)
                    .Vapor_Pressure_Constant_B = Parameters(1)
                    .Vapor_Pressure_Constant_C = Parameters(2)
                    .Vapor_Pressure_Constant_D = Parameters(3)
                    .Vapor_Pressure_Constant_E = Parameters(4)
                    .Vapor_Pressure_TMIN = 0.2 * frm.Tc
                    .Vapor_Pressure_TMAX = frm.Tc
                End With
            End With
            Me.ListView1.Items(2).SubItems(1).Text = "OK"
            Me.ListView1.Items(2).SubItems(2).Text = DWSIM.App.GetLocalString("Dadosverificados")
        Catch ex As Exception
            Me.TextBox1.Text = ex.Message.ToString
            Me.ListView1.Items(2).SubItems(1).Text = DWSIM.App.GetLocalString("Erro")
            Me.ListView1.Items(2).SubItems(2).Text = ex.Message.ToString
            'Me.KryptonButton2.Text = "Voltar ao Gerador de Hipotéticos"
            Me.KryptonButton3.Enabled = False
            Me.KryptonButton2.Enabled = True
            Exit Sub
        Finally

        End Try

        'cp
        Try
            With frm
                Dim Parameters(4) As Double
                If .RadioButton2.Checked = True Then
                    Dim obj As Object = frm.RegressData(frm.Tc, frm.Pc, frm.w, frm.MM, frm.Hvb, frm.Tb, 1, False)
                    Parameters = obj(0)
                Else
                    Dim obj As Object = frm.RegressData(frm.Tc, frm.Pc, frm.w, frm.MM, frm.Hvb, frm.Tb, 1, True)
                    Parameters = obj(0)
                End If
                With frm.constprop
                    .Ideal_Gas_Heat_Capacity_Const_A = Parameters(0)
                    .Ideal_Gas_Heat_Capacity_Const_B = Parameters(1)
                    .Ideal_Gas_Heat_Capacity_Const_C = Parameters(2)
                    .Ideal_Gas_Heat_Capacity_Const_D = Parameters(3)
                    .Ideal_Gas_Heat_Capacity_Const_E = Parameters(4)
                End With
            End With
            Me.ListView1.Items(3).SubItems(1).Text = "OK"
            Me.ListView1.Items(3).SubItems(2).Text = DWSIM.App.GetLocalString("Dadosverificadoscoms")
        Catch ex As Exception
            Me.TextBox1.Text = ex.Message.ToString
            Me.ListView1.Items(3).SubItems(1).Text = DWSIM.App.GetLocalString("Erro")
            Me.ListView1.Items(3).SubItems(2).Text = ex.Message.ToString
            'Me.KryptonButton2.Text = "Voltar ao Gerador de Hipotéticos"
            Me.KryptonButton3.Enabled = False
            Me.KryptonButton2.Enabled = True
            Exit Sub
        Finally

        End Try

        'visc
        Try
            With frm
                Dim Parameters(4) As Double
                If .RadioButton7.Checked = True Then
                    Dim obj As Object = .RegressData(frm.Tc, frm.Pc, frm.w, frm.MM, frm.Hvb, frm.Tb, 2, False)
                    Parameters = obj(0)
                Else
                    Dim obj As Object = .RegressData(frm.Tc, frm.Pc, frm.w, frm.MM, frm.Hvb, frm.Tb, 2, True)
                    Parameters = obj(0)
                End If
                With frm.constprop
                    .Liquid_Viscosity_Const_A = Parameters(0)
                    .Liquid_Viscosity_Const_B = Parameters(1)
                    .Liquid_Viscosity_Const_C = Parameters(2)
                    .Liquid_Viscosity_Const_D = Parameters(3)
                    .Liquid_Viscosity_Const_E = Parameters(4)
                End With
            End With
            Me.ListView1.Items(4).SubItems(1).Text = "OK"
            Me.ListView1.Items(4).SubItems(2).Text = DWSIM.App.GetLocalString("Dadosverificadoscoms")
        Catch ex As Exception
            Me.TextBox1.Text = ex.Message.ToString
            Me.ListView1.Items(4).SubItems(1).Text = DWSIM.App.GetLocalString("Erro")
            Me.ListView1.Items(4).SubItems(2).Text = ex.Message.ToString
            'Me.KryptonButton2.Text = "Voltar ao Gerador de Hipotéticos"
            Me.KryptonButton3.Enabled = False
            Me.KryptonButton2.Enabled = True
            Exit Sub
        Finally

        End Try

        'hvap
        Try
            With frm.constprop
                .HVap_A = frm.Hvb
                .HVap_TMAX = frm.Tc
                .HVap_TMIN = 0.6 * frm.Tc
            End With
        Catch ex As Exception
            Me.TextBox1.Text = ex.Message.ToString
            Exit Sub
        Finally

        End Try

        'Me.TextBox1.Text = "Hipotético criado com sucesso."
        Me.KryptonButton3.Enabled = True
        'Me.KryptonButton2.Text = "Adicionar Hipotético"
        Me.KryptonButton2.Enabled = True
        frm.HypError = False

        If frmchild.Collections.MaterialStreamCollection.Count > 0 Then
            Me.KryptonButton3.Enabled = True
        Else
            Me.KryptonButton3.Enabled = False
        End If

    End Sub

    Private Sub KryptonButton3_Click(ByVal sender As System.Object, ByVal e As System.EventArgs) Handles KryptonButton3.Click
        frmview = New FormPureComp
        With frmview
            '.Text = "Visualizar propriedades do hipotético"
            .ComboBox1.Items.Add(frm.TextBox1.Text)
            .ComboBox1.Enabled = False
            .constprop = frm.constprop
        End With
        frmview.ShowDialog(Me)
    End Sub

    Private Sub KryptonButton2_Click(ByVal sender As System.Object, ByVal e As System.EventArgs) Handles KryptonButton2.Click

        If frm.HypError = False Then
            If AddOnly Then
                Dim tmpcomp As New DWSIM.ClassesBasicasTermodinamica.ConstantProperties
                tmpcomp = frm.constprop
                frmchild.Options.NotSelectedComponents.Add(tmpcomp.Name, tmpcomp)
                frmchild.FrmStSim1.AddCompToGrid(tmpcomp)
            Else
                Dim tmpcomp As New DWSIM.ClassesBasicasTermodinamica.ConstantProperties
                tmpcomp = frm.constprop
                frmchild.Options.NotSelectedComponents.Add(tmpcomp.Name, tmpcomp)
                frmchild.FrmStSim1.AddCompToSimulation(frmchild.FrmStSim1.ogc1.Rows(frmchild.FrmStSim1.AddCompToGrid(tmpcomp)).Index)
            End If
        End If
        Me.Close()

    End Sub

End Class