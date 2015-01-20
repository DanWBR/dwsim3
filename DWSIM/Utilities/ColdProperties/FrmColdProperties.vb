'    Petroleum Cold Flow Properties Utility
'    Copyright 2009-2014 Daniel Wagner O. de Medeiros
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
Imports DWSIM.DWSIM.SimulationObjects
Imports System.Math

Public Class FrmColdProperties

    Dim mat As DWSIM.SimulationObjects.Streams.MaterialStream
    Dim frm As FormFlowsheet

    Public su As New DWSIM.SistemasDeUnidades.Unidades
    Public cv As New DWSIM.SistemasDeUnidades.Conversor
    Public nf As String

    Private Sub FrmColdProperties_Load(ByVal sender As System.Object, ByVal e As System.EventArgs) Handles MyBase.Load

        frm = My.Application.ActiveSimulation

        Me.ComboBox3.Items.Clear()
        For Each mat2 In Me.frm.Collections.CLCS_MaterialStreamCollection.Values
            If mat2.GraphicObject.Calculated Then Me.ComboBox3.Items.Add(mat2.GraphicObject.Tag.ToString)
        Next

        Me.ComboBox3.SelectedIndex = 0

        Me.frm = My.Application.ActiveSimulation

        Me.su = frm.Options.SelectedUnitSystem
        Me.nf = frm.Options.NumberFormat

        Me.LabelU1.Text = su.spmp_pressure
        Me.LabelU2.Text = su.spmp_pressure
        Me.LabelU3.Text = su.spmp_viscosity
        Me.LabelU4.Text = su.spmp_viscosity
        Me.LabelU5.Text = su.spmp_temperature
        Me.LabelU6.Text = su.spmp_temperature
        Me.LabelU7.Text = su.spmp_temperature
        Me.LabelU8.Text = su.spmp_temperature

    End Sub


    Private Sub Button1_Click(ByVal sender As System.Object, ByVal e As System.EventArgs) Handles Button1.Click

        If Not Me.ComboBox3.SelectedItem Is Nothing Then

            Dim gobj As Microsoft.Msdn.Samples.GraphicObjects.GraphicObject = FormFlowsheet.SearchSurfaceObjectsByTag(Me.ComboBox3.SelectedItem, frm.FormSurface.FlowsheetDesignSurface)
            Me.mat = frm.Collections.CLCS_MaterialStreamCollection(gobj.Name).Clone

            Dim pp As PropertyPackages.PropertyPackage = frm.Options.SelectedPropertyPackage
            Dim MABP, CABP, MeABP, K, SG, API As Double

            pp.CurrentMaterialStream = mat

            Dim ppi As New PropertyPackages.RaoultPropertyPackage
            ppi.CurrentMaterialStream = mat

            MABP = 0
            CABP = 0

            Dim i As Integer = 0
            Dim Vx(mat.Fases(0).Componentes.Count - 1) As Double
            For Each subst As Substancia In mat.Fases(0).Componentes.Values
                MABP += subst.FracaoMolar.GetValueOrDefault * subst.ConstantProperties.Normal_Boiling_Point
                CABP += subst.FracaoMolar.GetValueOrDefault * subst.ConstantProperties.Normal_Boiling_Point ^ (1 / 3)
                Vx(i) = subst.FracaoMolar
                i = i + 1
            Next
            CABP = CABP ^ 3
            MeABP = (MABP + CABP) / 2

            SG = pp.DW_CalcMassaEspecifica_ISOL(PropertyPackages.Fase.Liquid, 288.706, 101325) / 999
            K = (1.8 * MeABP) ^ (1 / 3) / SG
            API = 141.5 / SG - 131.5

            Dim FlashPoint, PourPoint, CloudPoint, FreezingPoint, RVP, TVP, RefractionIndex, Huang_I, CetaneIndex, v1, v2, kv1 As Double

            TVP = pp.DW_CalcBubP(Vx, 310.95, 101325)(4)
            v1 = pp.DW_CalcViscosidadeDinamica_ISOL(PropertyPackages.Fase.Liquid, 310.95, 101325)
            kv1 = v1 / pp.DW_CalcMassaEspecifica_ISOL(PropertyPackages.Fase.Liquid, 310.95, 101325)
            v2 = pp.DW_CalcViscosidadeDinamica_ISOL(PropertyPackages.Fase.Liquid, 372.05, 101325)

            Dim t10ASTM, t10TBP, bt, dp As Double
            Try
                bt = pp.DW_CalcBubT(Vx, 101325)(4)
            Catch ex As Exception
                bt = ppi.DW_CalcBubT(Vx, 101325)(4)
            End Try
            Try
                dp = pp.DW_CalcDewP(Vx, 310.95)(4)
            Catch ex As Exception
                dp = ppi.DW_CalcDewP(Vx, 310.95)(4)
            End Try

            If dp < 0 Or Double.IsNaN(dp) Or Double.IsInfinity(dp) Then
                dp = ppi.DW_CalcDewP(Vx, 310.95)(4)
            End If

            Dim tmp, vv, vl, dv, dl, mwv, mwl As Object
            Dim vwl(mat.Fases(0).Componentes.Count - 1), vwv(mat.Fases(0).Componentes.Count - 1) As Double

            Dim t, t_ant, t_ant2, ft, ft_ant, ft_ant2, v As Double, j As Integer

            t = bt + 2
            i = 0
            Do
                ft_ant2 = ft_ant
                ft_ant = ft
                Try
                    tmp = pp.FlashBase.Flash_PT(pp.RET_VMOL(PropertyPackages.Fase.Mixture), 101325, t, pp)
                    v = tmp(1)
                    vv = tmp(3)
                    vl = tmp(2)
                Catch ex As Exception
                    tmp = ppi.FlashBase.Flash_PT(pp.RET_VMOL(PropertyPackages.Fase.Mixture), 101325, t, pp)
                    v = tmp(1)
                    vv = tmp(3)
                    vl = tmp(2)
                End Try

                mwv = pp.AUX_MMM(vv)
                mwl = pp.AUX_MMM(vl)

                'j = 0
                'sum1 = 0
                'sum2 = 0
                'For Each subst As Substancia In mat.Fases(0).Componentes.Values
                '    sum1 += vl(j) * subst.ConstantProperties.Molar_Weight / subst.ConstantProperties.PF_SG.GetValueOrDefault
                '    sum2 += vv(j) * subst.ConstantProperties.Molar_Weight / subst.ConstantProperties.PF_SG.GetValueOrDefault
                '    j = j + 1
                'Next
                'j = 0
                'For Each subst As Substancia In mat.Fases(0).Componentes.Values
                '    vwl(j) = vl(j) * subst.ConstantProperties.Molar_Weight / subst.ConstantProperties.PF_SG.GetValueOrDefault / sum1
                '    vwv(j) = vv(j) * subst.ConstantProperties.Molar_Weight / subst.ConstantProperties.PF_SG.GetValueOrDefault / sum1
                '    j = j + 1
                'Next
                'j = 0
                'dl = 0
                'dv = 0
                'For Each subst As Substancia In mat.Fases(0).Componentes.Values
                '    dl += vwl(j) / subst.ConstantProperties.PF_SG.GetValueOrDefault
                '    dv += vwv(j) / subst.ConstantProperties.PF_SG.GetValueOrDefault
                '    j = j + 1
                'Next

                'dl = 1 / dl
                'dv = 1 / dv

                mat.Fases(0).SPMProperties.temperature = t
                mat.Fases(0).SPMProperties.pressure = 101325
                j = 0
                For Each subst As Substancia In mat.Fases(1).Componentes.Values
                    subst.FracaoMolar = vl(j)
                    j += 1
                Next
                pp.DW_CalcProp("density", PropertyPackages.Fase.Liquid)
                dl = mat.Fases(1).SPMProperties.density.GetValueOrDefault
                j = 0
                For Each subst As Substancia In mat.Fases(1).Componentes.Values
                    subst.FracaoMolar = vv(j)
                    j += 1
                Next
                pp.DW_CalcProp("density", PropertyPackages.Fase.Liquid)
                dv = mat.Fases(1).SPMProperties.density.GetValueOrDefault

                If v = 0 Then v = i * 0.0001

                ft = v - (0.1 / dv) / ((0.1 / dv) + (0.9 / dl))

                t_ant2 = t_ant
                t_ant = t
                If i > 2 Then
                    If ft <> ft_ant2 Then
                        t = t - ft * (t - t_ant2) / (ft - ft_ant2)
                    Else
                        t = t
                    End If
                Else
                    t = t - 1
                End If
                i = i + 1
            Loop Until Abs(ft) < 0.001 Or t < 0 Or Double.IsNaN(t) Or Double.IsInfinity(t) Or i > 100
            t10TBP = t

            t10ASTM = (t10TBP / 0.5564) ^ (1 / 1.09)

            'API Procedure 2B5.1
            Huang_I = 0.02266 * Exp(0.0003905 * (1.8 * MeABP) + 2.468 * SG - 0.0005704 * (1.8 * MeABP) * SG) * (1.8 * MeABP) ^ 0.0572 * SG ^ -0.72
            RefractionIndex = ((1 + 2 * Huang_I) / (1 - Huang_I)) ^ 0.5

            'API Procedure 2B7.1 (Pensky-Martens Closed Cup - ASTM D93)
            FlashPoint = 0.69 * ((t10ASTM - 273.15) * 9 / 5 + 32) - 118.2
            FlashPoint = (FlashPoint - 32) * 5 / 9 + 273.15

            'API Procedure 2B8.1
            PourPoint = 753 + 136 * (1 - Exp(-0.15 * kv1 * 1000000.0)) - 572 * SG + 0.0512 * kv1 * 1000000.0 + 0.139 * (MeABP * 1.8)
            PourPoint /= 1.8

            'API Procedure 2B11.1
            FreezingPoint = -2390.42 + 1826 * SG + 122.49 * K - 0.135 * 1.8 * MeABP
            FreezingPoint /= 1.8

            'API Procedure 2B12.1
            CloudPoint = 10 ^ (-7.41 + 5.49 * Log10(MeABP * 1.8) - 0.712 * (1.8 * MeABP) ^ 0.315 - 0.133 * SG)
            CloudPoint /= 1.8

            'API Procedure 2B13.1
            CetaneIndex = 415.26 - 7.673 * API + 0.186 * (MeABP * 1.8 - 458.67) + 3.503 * API * Log10(1.8 * MeABP - 458.67) - 193.816 * Log10(MeABP * 1.8 - 458.67)

            Dim p, p_ant, p_ant2, fp, fp_ant, fp_ant2 As Double

            p = dp * 1.1
            i = 0
            Do
                fp_ant2 = fp_ant
                fp_ant = fp
                Try
                    tmp = pp.DW_CalcEquilibrio_ISOL(PropertyPackages.FlashSpec.T, PropertyPackages.FlashSpec.P, 310.95, p, 0)
                    v = tmp(1)
                    vv = tmp(9)
                    vl = tmp(8)
                Catch ex As Exception
                    tmp = ppi.DW_CalcEquilibrio_ISOL(PropertyPackages.FlashSpec.T, PropertyPackages.FlashSpec.P, 310.95, p, 0)
                    v = tmp(1)
                    vv = tmp(9)
                    vl = tmp(8)
                End Try

                mwv = pp.AUX_MMM(vv)
                mwl = pp.AUX_MMM(vl)

                'j = 0
                'sum1 = 0
                'sum2 = 0
                'For Each subst As Substancia In mat.Fases(0).Componentes.Values
                '    sum1 += vl(j) * subst.ConstantProperties.Molar_Weight / subst.ConstantProperties.PF_SG.GetValueOrDefault
                '    sum2 += vv(j) * subst.ConstantProperties.Molar_Weight / subst.ConstantProperties.PF_SG.GetValueOrDefault
                '    j = j + 1
                'Next
                'j = 0
                'For Each subst As Substancia In mat.Fases(0).Componentes.Values
                '    vwl(j) = vl(j) * subst.ConstantProperties.Molar_Weight / subst.ConstantProperties.PF_SG.GetValueOrDefault / sum1
                '    vwv(j) = vv(j) * subst.ConstantProperties.Molar_Weight / subst.ConstantProperties.PF_SG.GetValueOrDefault / sum1
                '    j = j + 1
                'Next
                'j = 0
                'dl = 0
                'dv = 0
                'For Each subst As Substancia In mat.Fases(0).Componentes.Values
                '    dl += vwl(j) / subst.ConstantProperties.PF_SG.GetValueOrDefault
                '    dv += vwv(j) / subst.ConstantProperties.PF_SG.GetValueOrDefault
                '    j = j + 1
                'Next

                'dl = 1 / dl
                'dv = 1 / dv

                mat.Fases(0).SPMProperties.temperature = t
                mat.Fases(0).SPMProperties.pressure = 101325
                j = 0
                For Each subst As Substancia In mat.Fases(1).Componentes.Values
                    subst.FracaoMolar = vl(j)
                    j += 1
                Next
                pp.DW_CalcProp("density", PropertyPackages.Fase.Liquid)
                dl = mat.Fases(1).SPMProperties.density.GetValueOrDefault
                j = 0
                For Each subst As Substancia In mat.Fases(1).Componentes.Values
                    subst.FracaoMolar = vv(j)
                    j += 1
                Next
                pp.DW_CalcProp("density", PropertyPackages.Fase.Liquid)
                dv = mat.Fases(1).SPMProperties.density.GetValueOrDefault

                If v = 1 Then v = 1 - i * 0.0001

                fp = v - (0.8 / dv) / ((0.8 / dv) + (0.2 / dl))

                p_ant2 = p_ant
                p_ant = p
                If i > 2 Then
                    If ft <> ft_ant2 Then
                        p = p - fp * (p - p_ant2) / (fp - fp_ant2)
                    Else
                        p = p
                    End If
                Else
                    p = p * 0.9999
                End If
                i = i + 1
            Loop Until Abs(fp) < 0.001 Or p <= 0 Or Double.IsNaN(p) Or Double.IsInfinity(p) Or i > 100

            RVP = p

            Me.TextBox1.Text = Format(cv.ConverterDoSI(su.spmp_pressure, TVP), nf)
            Me.TextBox2.Text = Format(cv.ConverterDoSI(su.spmp_pressure, RVP), nf)
            Me.TextBox3.Text = Format(cv.ConverterDoSI(su.spmp_viscosity, v1), nf)
            Me.TextBox4.Text = Format(cv.ConverterDoSI(su.spmp_viscosity, v2), nf)
            Me.TextBox5.Text = Format(cv.ConverterDoSI(su.spmp_temperature, FlashPoint), nf)
            Me.TextBox6.Text = Format(cv.ConverterDoSI(su.spmp_temperature, PourPoint), nf)
            Me.TextBox7.Text = Format(cv.ConverterDoSI(su.spmp_temperature, CloudPoint), nf)
            Me.TextBox8.Text = Format(cv.ConverterDoSI(su.spmp_temperature, FreezingPoint), nf)
            Me.TextBox9.Text = Format(RefractionIndex, nf)
            Me.TextBox10.Text = Format(CetaneIndex, nf)

        End If

    End Sub

End Class
