'    Peng-Robinson (with Volume Translation and Immiscible Water) Property Package 
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

Imports DWSIM.DWSIM.SimulationObjects.PropertyPackages
Imports System.Math
Imports DWSIM.DWSIM.Flowsheet.FlowSheetSolver

Namespace DWSIM.SimulationObjects.PropertyPackages

    <System.Serializable()> Public Class PengRobinsonIWVTPropertyPackage

        Inherits DWSIM.SimulationObjects.PropertyPackages.PropertyPackage

        Public MAT_KIJ(38, 38) As Object

        Private m_props As New DWSIM.SimulationObjects.PropertyPackages.Auxiliary.PROPS
        Public m_pr As New DWSIM.SimulationObjects.PropertyPackages.Auxiliary.PengRobinson

        Public Sub New(ByVal comode As Boolean)
            MyBase.New(comode)
        End Sub

        Public Sub New()

            MyBase.New()

            With Me.Parameters
                .Add("PP_USE_EOS_LIQDENS", 1)
            End With

            Me.IsConfigurable = True
            Me.ConfigForm = New FormConfigPP
            Me._packagetype = PropertyPackages.PackageType.EOS

        End Sub

        Public Overrides Sub ReconfigureConfigForm()
            MyBase.ReconfigureConfigForm()
            Me.ConfigForm = New FormConfigPP
        End Sub

        Private Function GetWaterIndex(ByVal Vx As Object) As Integer

            Dim i As Integer
            Dim n As Integer = UBound(Vx)
            Dim Vmw, VTc, VTb As Double()

            Vmw = Me.RET_VMM()
            VTc = Me.RET_VTC()
            VTb = Me.RET_VTB()

            For i = 0 To n
                If CInt(Vmw(i)) = 18 And CInt(VTc(i)) = 647 And CInt(VTb(i)) = 373 Then
                    Return i
                End If
            Next

            Return -1

        End Function

#Region "    DWSIM Functions"

        Public Overrides Function DW_CalcCp_ISOL(ByVal fase1 As DWSIM.SimulationObjects.PropertyPackages.Fase, ByVal T As Double, ByVal P As Double) As Double
            Select Case fase1
                Case Fase.Liquid
                    Return Me.m_props.CpCvR("L", T, P, RET_VMOL(fase1), RET_VKij(), RET_VMAS(fase1), RET_VTC(), RET_VPC(), RET_VCP(T), RET_VMM(), RET_VW(), RET_VZRa())(1)
                Case Fase.Aqueous
                    Return Me.m_props.CpCvR("L", T, P, RET_VMOL(fase1), RET_VKij(), RET_VMAS(fase1), RET_VTC(), RET_VPC(), RET_VCP(T), RET_VMM(), RET_VW(), RET_VZRa())(1)
                Case Fase.Liquid1
                    Return Me.m_props.CpCvR("L", T, P, RET_VMOL(fase1), RET_VKij(), RET_VMAS(fase1), RET_VTC(), RET_VPC(), RET_VCP(T), RET_VMM(), RET_VW(), RET_VZRa())(1)
                Case Fase.Liquid2
                    Return Me.m_props.CpCvR("L", T, P, RET_VMOL(fase1), RET_VKij(), RET_VMAS(fase1), RET_VTC(), RET_VPC(), RET_VCP(T), RET_VMM(), RET_VW(), RET_VZRa())(1)
                Case Fase.Liquid3
                    Return Me.m_props.CpCvR("L", T, P, RET_VMOL(fase1), RET_VKij(), RET_VMAS(fase1), RET_VTC(), RET_VPC(), RET_VCP(T), RET_VMM(), RET_VW(), RET_VZRa())(1)
                Case Fase.Vapor
                    Return Me.m_props.CpCvR("V", T, P, RET_VMOL(fase1), RET_VKij(), RET_VMAS(fase1), RET_VTC(), RET_VPC(), RET_VCP(T), RET_VMM(), RET_VW(), RET_VZRa())(1)
            End Select
        End Function

        Public Overrides Function DW_CalcEnergiaMistura_ISOL(ByVal T As Double, ByVal P As Double) As Double

            Dim HM, HV, HL1, HL2 As Double

            HL1 = Me.m_pr.H_PR_MIX("L", T, P, RET_VMOL(Fase.Liquid1), RET_VKij(), RET_VTC, RET_VPC, RET_VW, RET_VMM, Me.RET_Hid(298.15, T, Fase.Liquid1))
            HL2 = Me.m_pr.H_PR_MIX("L", T, P, RET_VMOL(Fase.Aqueous), RET_VKij(), RET_VTC, RET_VPC, RET_VW, RET_VMM, Me.RET_Hid(298.15, T, Fase.Aqueous))
            HV = Me.m_pr.H_PR_MIX("V", T, P, RET_VMOL(Fase.Vapor), RET_VKij(), RET_VTC, RET_VPC, RET_VW, RET_VMM, Me.RET_Hid(298.15, T, Fase.Vapor))
            HM = Me.CurrentMaterialStream.Fases(Me.RET_PHASEID(Fase.Liquid1)).SPMProperties.massfraction.GetValueOrDefault * HL1 + _
                Me.CurrentMaterialStream.Fases(Me.RET_PHASEID(Fase.Aqueous)).SPMProperties.massfraction.GetValueOrDefault * HL2 + _
                Me.CurrentMaterialStream.Fases(2).SPMProperties.massfraction.GetValueOrDefault * HV

            Dim ent_massica = HM
            Dim flow = Me.CurrentMaterialStream.Fases(0).SPMProperties.massflow
            Return ent_massica * flow

        End Function

        Public Overrides Sub DW_CalcEquilibrium(ByVal spec1 As DWSIM.SimulationObjects.PropertyPackages.FlashSpec, ByVal spec2 As DWSIM.SimulationObjects.PropertyPackages.FlashSpec)

            Dim P, T, H, S, xv, xl As Double
            Dim result As Object = Nothing
            Dim subst As DWSIM.ClassesBasicasTermodinamica.Substancia
            Dim n As Integer = Me.CurrentMaterialStream.Fases(0).Componentes.Count - 1
            Dim i As Integer = 0
            Dim Vx(n), Vy(n), Vx1(n), Vx2(n) As Double

            Select Case spec1

                Case FlashSpec.T

                    Select Case spec2

                        Case FlashSpec.P

                            T = Me.CurrentMaterialStream.Fases(0).SPMProperties.temperature.GetValueOrDefault
                            P = Me.CurrentMaterialStream.Fases(0).SPMProperties.pressure.GetValueOrDefault

                            result = Me.FLASH_TP(T, P, RET_VMOL(Fase.Mixture), RET_VKij, RET_VTC, RET_VPC, RET_VTB, RET_VW, RET_VC)

                            xl = result(0, 0)
                            xv = result(1, 0)

                            i = 0
                            Do
                                Vx(i) = result(0, i + 1)
                                Vy(i) = result(1, i + 1)
                                i = i + 1
                            Loop Until i = n + 1

                            i = 1
                            For Each subst In Me.CurrentMaterialStream.Fases(1).Componentes.Values
                                subst.FracaoMolar = result(0, i)
                                subst.FugacityCoeff = result(4, i)
                                subst.ActivityCoeff = result(6, i)
                                subst.PartialVolume = result(8, i)
                                subst.PartialPressure = result(10, i)
                                i += 1
                            Next
                            For Each subst In Me.CurrentMaterialStream.Fases(1).Componentes.Values
                                subst.FracaoMassica = Me.AUX_CONVERT_MOL_TO_MASS(subst.Nome, 1)
                            Next
                            i = 1
                            For Each subst In Me.CurrentMaterialStream.Fases(2).Componentes.Values
                                subst.FracaoMolar = result(1, i)
                                subst.FugacityCoeff = result(5, i)
                                subst.ActivityCoeff = result(7, i)
                                subst.PartialVolume = result(9, i)
                                subst.PartialPressure = result(11, i)
                                i += 1
                            Next
                            For Each subst In Me.CurrentMaterialStream.Fases(2).Componentes.Values
                                subst.FracaoMassica = Me.AUX_CONVERT_MOL_TO_MASS(subst.Nome, 2)
                            Next
                            Me.CurrentMaterialStream.Fases(1).SPMProperties.molarfraction = xl
                            Me.CurrentMaterialStream.Fases(2).SPMProperties.molarfraction = xv
                            Me.CurrentMaterialStream.Fases(1).SPMProperties.massfraction = xl * Me.AUX_MMM(Fase.Liquid) / (xl * Me.AUX_MMM(Fase.Liquid) + xv * Me.AUX_MMM(Fase.Vapor))
                            Me.CurrentMaterialStream.Fases(2).SPMProperties.massfraction = xv * Me.AUX_MMM(Fase.Vapor) / (xl * Me.AUX_MMM(Fase.Liquid) + xv * Me.AUX_MMM(Fase.Vapor))

                            Dim HM, HV, HL, HL1, HL2, xl1, xl2 As Double
                            Dim wi As Integer
                            If xl <> 0 Then
                                wi = Me.GetWaterIndex(Vx)
                                If wi <> -1 Then
                                    'there is water in the mixture
                                    xl1 = xl * (1 - Vx(wi))
                                    xl2 = xl - xl1
                                    Vx1 = Vx.Clone
                                    Vx2 = Vx.Clone
                                    Vx1(wi) = 0
                                    Vx1 = Me.AUX_NORMALIZE(Vx1)
                                    Vx2 = Me.AUX_ERASE(Vx)
                                    Vx2(wi) = 1
                                    HL1 = Me.m_pr.H_PR_MIX("L", T, P, Vx1, RET_VKij(), RET_VTC, RET_VPC, RET_VW, RET_VMM, Me.RET_Hid(298.15, T, Vx1)) * Me.AUX_MMM(Vx1)
                                    HL2 = Me.m_pr.H_PR_MIX("L", T, P, Vx2, RET_VKij(), RET_VTC, RET_VPC, RET_VW, RET_VMM, Me.RET_Hid(298.15, T, Vx2)) * Me.AUX_MMM(Vx2)
                                Else
                                    'there is NO water in the mixture
                                    xl1 = xl
                                    xl2 = 0
                                    Vx1 = Vx.Clone
                                    Vx2 = Me.AUX_ERASE(Vx)
                                    HL1 = Me.m_pr.H_PR_MIX("L", T, P, Vx1, RET_VKij(), RET_VTC, RET_VPC, RET_VW, RET_VMM, Me.RET_Hid(298.15, T, Vx1)) * Me.AUX_MMM(Vx1)
                                    HL2 = 0
                                End If
                            End If

                            If Double.IsNaN(HL1) Then HL1 = 0.0#
                            If Double.IsNaN(HL2) Then HL2 = 0.0#

                            HL = xl1 * HL1 / Me.AUX_MMM(Vx1) + xl2 * HL2 / Me.AUX_MMM(Vx2)
                            If xv <> 0 Then HV = Me.m_pr.H_PR_MIX("V", T, P, Vy, RET_VKij(), RET_VTC, RET_VPC, RET_VW, RET_VMM, Me.RET_Hid(298.15, T, Vy)) * Me.AUX_MMM(Vy)
                            HM = (xl1 * HL1 + xl2 * HL2 + xv * HV) / Me.AUX_MMM(Fase.Mixture)

                            H = HM

                            Dim SM, SV, SL, SL1, SL2 As Double

                            If xl <> 0 Then
                                wi = Me.GetWaterIndex(Vx)
                                If wi <> -1 Then
                                    'there is water in the mixture
                                    xl1 = xl * (1 - Vx(wi))
                                    xl2 = xl - xl1
                                    Vx1 = Vx.Clone
                                    Vx2 = Vx.Clone
                                    Vx1(wi) = 0
                                    Vx1 = Me.AUX_NORMALIZE(Vx1)
                                    Vx2 = Me.AUX_ERASE(Vx)
                                    Vx2(wi) = 1
                                    SL1 = Me.m_pr.S_PR_MIX("L", T, P, Vx1, RET_VKij, RET_VTC, RET_VPC, RET_VW, RET_VMM, Me.RET_Sid(298.15, T, P, Vx1)) * Me.AUX_MMM(Vx1)
                                    SL2 = Me.m_pr.S_PR_MIX("L", T, P, Vx2, RET_VKij, RET_VTC, RET_VPC, RET_VW, RET_VMM, Me.RET_Sid(298.15, T, P, Vx2)) * Me.AUX_MMM(Vx2)
                                Else
                                    'there is NO water in the mixture
                                    xl1 = xl
                                    xl2 = 0
                                    Vx1 = Vx.Clone
                                    Vx2 = Me.AUX_ERASE(Vx)
                                    SL1 = Me.m_pr.S_PR_MIX("L", T, P, Vx1, RET_VKij, RET_VTC, RET_VPC, RET_VW, RET_VMM, Me.RET_Sid(298.15, T, P, Vx1)) * Me.AUX_MMM(Vx1)
                                    SL2 = 0
                                End If
                            End If

                            If Double.IsNaN(SL1) Then SL1 = 0.0#
                            If Double.IsNaN(SL2) Then SL2 = 0.0#

                            SL = xl1 * SL1 / Me.AUX_MMM(Vx1) + xl2 * SL2 / Me.AUX_MMM(Vx2)
                            If xv <> 0 Then SV = Me.m_pr.S_PR_MIX("V", T, P, Vy, RET_VKij, RET_VTC, RET_VPC, RET_VW, RET_VMM, Me.RET_Sid(298.15, T, P, Vy)) * Me.AUX_MMM(Vy)
                            SM = (xl1 * SL1 + xl2 * SL2 + xv * SV) / Me.AUX_MMM(Fase.Mixture)

                            S = SM

                            i = 0
                            For Each subst In Me.CurrentMaterialStream.Fases(Me.RET_PHASEID(Fase.Liquid1)).Componentes.Values
                                subst.FracaoMolar = Vx1(i)
                                i += 1
                            Next
                            For Each subst In Me.CurrentMaterialStream.Fases(Me.RET_PHASEID(Fase.Liquid1)).Componentes.Values
                                subst.FracaoMassica = Me.AUX_CONVERT_MOL_TO_MASS(subst.Nome, Me.RET_PHASEID(Fase.Liquid1))
                            Next
                            i = 0
                            For Each subst In Me.CurrentMaterialStream.Fases(Me.RET_PHASEID(Fase.Aqueous)).Componentes.Values
                                subst.FracaoMolar = Vx2(i)
                                i += 1
                            Next
                            For Each subst In Me.CurrentMaterialStream.Fases(Me.RET_PHASEID(Fase.Aqueous)).Componentes.Values
                                subst.FracaoMassica = Me.AUX_CONVERT_MOL_TO_MASS(subst.Nome, Me.RET_PHASEID(Fase.Aqueous))
                            Next
                            Me.CurrentMaterialStream.Fases(Me.RET_PHASEID(Fase.Liquid1)).SPMProperties.molarfraction = xl1
                            Me.CurrentMaterialStream.Fases(Me.RET_PHASEID(Fase.Aqueous)).SPMProperties.molarfraction = xl2
                            Me.CurrentMaterialStream.Fases(Me.RET_PHASEID(Fase.Liquid1)).SPMProperties.massfraction = xl1 * Me.AUX_MMM(Vx1) / (xl1 * Me.AUX_MMM(Vx1) + xl2 * Me.AUX_MMM(Vx2) + xv * Me.AUX_MMM(Fase.Vapor))
                            Me.CurrentMaterialStream.Fases(Me.RET_PHASEID(Fase.Aqueous)).SPMProperties.massfraction = xl2 * Me.AUX_MMM(Vx2) / (xl1 * Me.AUX_MMM(Vx1) + xl2 * Me.AUX_MMM(Vx2) + xv * Me.AUX_MMM(Fase.Vapor))

                        Case FlashSpec.H

                            Throw New Exception(DWSIM.App.GetLocalString("PropPack_FlashTHNotSupported"))

                        Case FlashSpec.S

                            Throw New Exception(DWSIM.App.GetLocalString("PropPack_FlashTSNotSupported"))

                    End Select

                Case FlashSpec.P

                    Select Case spec2

                        Case FlashSpec.H

                            T = Me.CurrentMaterialStream.Fases(0).SPMProperties.temperature.GetValueOrDefault
                            P = Me.CurrentMaterialStream.Fases(0).SPMProperties.pressure.GetValueOrDefault
                            H = Me.CurrentMaterialStream.Fases(0).SPMProperties.enthalpy.GetValueOrDefault

                            If Me.AUX_IS_SINGLECOMP(Fase.Mixture) Then

                                Dim hl, hv, sl, sv As Double
                                Dim vz As Object = Me.RET_VMOL(Fase.Mixture)
                                T = Me.AUX_TSATi(P, Me.AUX_SINGLECOMPIDX(Fase.Mixture))

                                hl = Me.m_pr.H_PR_MIX("L", T, P, vz, RET_VKij, RET_VTC, RET_VPC, RET_VW, RET_VMM, Me.RET_Hid(298.15, T, vz))
                                hv = Me.m_pr.H_PR_MIX("V", T, P, vz, RET_VKij, RET_VTC, RET_VPC, RET_VW, RET_VMM, Me.RET_Hid(298.15, T, vz))
                                sl = Me.m_pr.S_PR_MIX("L", T, P, vz, RET_VKij, RET_VTC, RET_VPC, RET_VW, RET_VMM, Me.RET_Sid(298.15, T, P, vz))
                                sv = Me.m_pr.S_PR_MIX("V", T, P, vz, RET_VKij, RET_VTC, RET_VPC, RET_VW, RET_VMM, Me.RET_Sid(298.15, T, P, vz))
                                If H < hl Then
                                    xv = 0
                                    GoTo redirect
                                ElseIf H > hv Then
                                    xv = 1
                                    GoTo redirect
                                Else
                                    xv = (H - hl) / (hv - hl)
                                End If
                                S = xv * sv + (1 - xv) * sl
                                xl = 1 - xv
                                T = Me.CurrentMaterialStream.Fases(0).SPMProperties.temperature.GetValueOrDefault

                                Me.CurrentMaterialStream.Fases(1).SPMProperties.molarfraction = xl
                                Me.CurrentMaterialStream.Fases(2).SPMProperties.molarfraction = xv
                                Me.CurrentMaterialStream.Fases(1).SPMProperties.massfraction = xl
                                Me.CurrentMaterialStream.Fases(2).SPMProperties.massfraction = xv

                                i = 0
                                For Each subst In Me.CurrentMaterialStream.Fases(1).Componentes.Values
                                    subst.FracaoMolar = vz(i)
                                    subst.FugacityCoeff = 1
                                    subst.ActivityCoeff = 1
                                    subst.PartialVolume = 0
                                    subst.PartialPressure = P
                                    subst.FracaoMassica = vz(i)
                                    i += 1
                                Next
                                i = 0
                                For Each subst In Me.CurrentMaterialStream.Fases(2).Componentes.Values
                                    subst.FracaoMolar = vz(i)
                                    subst.FugacityCoeff = 1
                                    subst.ActivityCoeff = 1
                                    subst.PartialVolume = 0
                                    subst.PartialPressure = P
                                    subst.FracaoMassica = vz(i)
                                    i += 1
                                Next

                            Else

redirect:                       result = Me.FLASH_PH(T, H, P, RET_VMOL(Fase.Mixture), RET_VKij, RET_VTC, RET_VPC, RET_VTB, RET_VW)

                                T = result(3, 0)

                                xl = result(0, 0)
                                xv = result(1, 0)

                                i = 0
                                Do
                                    Vx(i) = result(0, i + 1)
                                    Vy(i) = result(1, i + 1)
                                    i = i + 1
                                Loop Until i = n + 1

                                Me.CurrentMaterialStream.Fases(1).SPMProperties.molarfraction = xl
                                Me.CurrentMaterialStream.Fases(2).SPMProperties.molarfraction = xv

                                i = 1
                                For Each subst In Me.CurrentMaterialStream.Fases(1).Componentes.Values
                                    subst.FracaoMolar = result(0, i)
                                    subst.FugacityCoeff = result(4, i)
                                    subst.ActivityCoeff = result(6, i)
                                    subst.PartialVolume = result(8, i)
                                    subst.PartialPressure = result(10, i)
                                    i += 1
                                Next
                                i = 1
                                For Each subst In Me.CurrentMaterialStream.Fases(1).Componentes.Values
                                    subst.FracaoMassica = Me.AUX_CONVERT_MOL_TO_MASS(subst.Nome, 1)
                                    i += 1
                                Next
                                i = 1
                                For Each subst In Me.CurrentMaterialStream.Fases(2).Componentes.Values
                                    subst.FracaoMolar = result(1, i)
                                    subst.FugacityCoeff = result(5, i)
                                    subst.ActivityCoeff = result(7, i)
                                    subst.PartialVolume = result(9, i)
                                    subst.PartialPressure = result(11, i)
                                    i += 1
                                Next

                                i = 1
                                For Each subst In Me.CurrentMaterialStream.Fases(2).Componentes.Values
                                    subst.FracaoMassica = Me.AUX_CONVERT_MOL_TO_MASS(subst.Nome, 2)
                                    i += 1
                                Next

                                Me.CurrentMaterialStream.Fases(1).SPMProperties.massfraction = xl * Me.AUX_MMM(Fase.Liquid) / (xl * Me.AUX_MMM(Fase.Liquid) + xv * Me.AUX_MMM(Fase.Vapor))
                                Me.CurrentMaterialStream.Fases(2).SPMProperties.massfraction = xv * Me.AUX_MMM(Fase.Vapor) / (xl * Me.AUX_MMM(Fase.Liquid) + xv * Me.AUX_MMM(Fase.Vapor))

                                Dim wi As Integer

                                Dim SM, SV, SL, SL1, SL2, xl1, xl2 As Double

                                If xl <> 0 Then
                                    wi = Me.GetWaterIndex(Vx)
                                    If wi <> -1 Then
                                        'there is water in the mixture
                                        xl1 = xl * (1 - Vx(wi))
                                        xl2 = xl - xl1
                                        Vx1 = Vx.Clone
                                        Vx2 = Vx.Clone
                                        Vx1(wi) = 0
                                        Vx1 = Me.AUX_NORMALIZE(Vx1)
                                        Vx2 = Me.AUX_ERASE(Vx)
                                        Vx2(wi) = 1
                                        SL1 = Me.m_pr.S_PR_MIX("L", T, P, Vx1, RET_VKij, RET_VTC, RET_VPC, RET_VW, RET_VMM, Me.RET_Sid(298.15, T, P, Vx1)) * Me.AUX_MMM(Vx1)
                                        SL2 = Me.m_pr.S_PR_MIX("L", T, P, Vx2, RET_VKij, RET_VTC, RET_VPC, RET_VW, RET_VMM, Me.RET_Sid(298.15, T, P, Vx2)) * Me.AUX_MMM(Vx2)
                                    Else
                                        'there is NO water in the mixture
                                        xl1 = xl
                                        xl2 = 0
                                        Vx1 = Vx.Clone
                                        Vx2 = Me.AUX_ERASE(Vx)
                                        SL1 = Me.m_pr.S_PR_MIX("L", T, P, Vx1, RET_VKij, RET_VTC, RET_VPC, RET_VW, RET_VMM, Me.RET_Sid(298.15, T, P, Vx1)) * Me.AUX_MMM(Vx1)
                                        SL2 = 0
                                    End If
                                End If

                                If Double.IsNaN(SL1) Then SL1 = 0.0#
                                If Double.IsNaN(SL2) Then SL2 = 0.0#

                                SL = xl1 * SL1 / Me.AUX_MMM(Vx1) + xl2 * SL2 / Me.AUX_MMM(Vx2)
                                If xv <> 0 Then SV = Me.m_pr.S_PR_MIX("V", T, P, Vy, RET_VKij, RET_VTC, RET_VPC, RET_VW, RET_VMM, Me.RET_Sid(298.15, T, P, Vy)) * Me.AUX_MMM(Vy)
                                SM = (xl1 * SL1 + xl2 * SL2 + xv * SV) / Me.AUX_MMM(Fase.Mixture)

                                S = SM

                                i = 0
                                For Each subst In Me.CurrentMaterialStream.Fases(Me.RET_PHASEID(Fase.Liquid1)).Componentes.Values
                                    subst.FracaoMolar = Vx1(i)
                                    i += 1
                                Next
                                For Each subst In Me.CurrentMaterialStream.Fases(Me.RET_PHASEID(Fase.Liquid1)).Componentes.Values
                                    subst.FracaoMassica = Me.AUX_CONVERT_MOL_TO_MASS(subst.Nome, Me.RET_PHASEID(Fase.Liquid1))
                                Next
                                i = 0
                                For Each subst In Me.CurrentMaterialStream.Fases(Me.RET_PHASEID(Fase.Aqueous)).Componentes.Values
                                    subst.FracaoMolar = Vx2(i)
                                    i += 1
                                Next
                                For Each subst In Me.CurrentMaterialStream.Fases(Me.RET_PHASEID(Fase.Aqueous)).Componentes.Values
                                    subst.FracaoMassica = Me.AUX_CONVERT_MOL_TO_MASS(subst.Nome, Me.RET_PHASEID(Fase.Aqueous))
                                Next
                                Me.CurrentMaterialStream.Fases(Me.RET_PHASEID(Fase.Liquid1)).SPMProperties.molarfraction = xl1
                                Me.CurrentMaterialStream.Fases(Me.RET_PHASEID(Fase.Aqueous)).SPMProperties.molarfraction = xl2
                                Me.CurrentMaterialStream.Fases(Me.RET_PHASEID(Fase.Liquid1)).SPMProperties.massfraction = xl1 * Me.AUX_MMM(Vx1) / (xl1 * Me.AUX_MMM(Vx1) + xl2 * Me.AUX_MMM(Vx2) + xv * Me.AUX_MMM(Fase.Vapor))
                                Me.CurrentMaterialStream.Fases(Me.RET_PHASEID(Fase.Aqueous)).SPMProperties.massfraction = xl2 * Me.AUX_MMM(Vx2) / (xl1 * Me.AUX_MMM(Vx1) + xl2 * Me.AUX_MMM(Vx2) + xv * Me.AUX_MMM(Fase.Vapor))

                            End If

                        Case FlashSpec.S

                            T = Me.CurrentMaterialStream.Fases(0).SPMProperties.temperature.GetValueOrDefault
                            P = Me.CurrentMaterialStream.Fases(0).SPMProperties.pressure.GetValueOrDefault
                            S = Me.CurrentMaterialStream.Fases(0).SPMProperties.entropy.GetValueOrDefault

                            If Me.AUX_IS_SINGLECOMP(Fase.Mixture) Then

                                Dim hl, hv, sl, sv As Double
                                Dim vz As Object = Me.RET_VMOL(Fase.Mixture)
                                T = Me.AUX_TSATi(P, Me.AUX_SINGLECOMPIDX(Fase.Mixture))

                                hl = Me.m_pr.H_PR_MIX("L", T, P, vz, RET_VKij, RET_VTC, RET_VPC, RET_VW, RET_VMM, Me.RET_Hid(298.15, T, vz))
                                hv = Me.m_pr.H_PR_MIX("V", T, P, vz, RET_VKij, RET_VTC, RET_VPC, RET_VW, RET_VMM, Me.RET_Hid(298.15, T, vz))
                                sl = Me.m_pr.S_PR_MIX("L", T, P, vz, RET_VKij, RET_VTC, RET_VPC, RET_VW, RET_VMM, Me.RET_Sid(298.15, T, P, vz))
                                sv = Me.m_pr.S_PR_MIX("V", T, P, vz, RET_VKij, RET_VTC, RET_VPC, RET_VW, RET_VMM, Me.RET_Sid(298.15, T, P, vz))
                                If H < hl Then
                                    xv = 0
                                    GoTo redirect2
                                ElseIf H > hv Then
                                    xv = 1
                                    GoTo redirect2
                                Else
                                    xv = (S - sl) / (sv - sl)
                                End If
                                H = xv * hv + (1 - xv) * hl
                                xl = 1 - xv
                                T = Me.CurrentMaterialStream.Fases(0).SPMProperties.temperature.GetValueOrDefault

                                Me.CurrentMaterialStream.Fases(1).SPMProperties.molarfraction = xl
                                Me.CurrentMaterialStream.Fases(2).SPMProperties.molarfraction = xv
                                Me.CurrentMaterialStream.Fases(1).SPMProperties.massfraction = xl
                                Me.CurrentMaterialStream.Fases(2).SPMProperties.massfraction = xv

                                i = 0
                                For Each subst In Me.CurrentMaterialStream.Fases(1).Componentes.Values
                                    subst.FracaoMolar = vz(i)
                                    subst.FugacityCoeff = 1
                                    subst.ActivityCoeff = 1
                                    subst.PartialVolume = 0
                                    subst.PartialPressure = P
                                    subst.FracaoMassica = vz(i)
                                    i += 1
                                Next
                                i = 0
                                For Each subst In Me.CurrentMaterialStream.Fases(2).Componentes.Values
                                    subst.FracaoMolar = vz(i)
                                    subst.FugacityCoeff = 1
                                    subst.ActivityCoeff = 1
                                    subst.PartialVolume = 0
                                    subst.PartialPressure = P
                                    subst.FracaoMassica = vz(i)
                                    i += 1
                                Next

                            Else

redirect2:                      result = Me.FLASH_PS(T, S, P, RET_VMOL(Fase.Mixture), RET_VKij, RET_VTC, RET_VPC, RET_VTB, RET_VW)

                                T = result(3, 0)

                                xl = result(0, 0)
                                xv = result(1, 0)

                                i = 0
                                Do
                                    Vx(i) = result(0, i + 1)
                                    Vy(i) = result(1, i + 1)
                                    i = i + 1
                                Loop Until i = n + 1

                                Me.CurrentMaterialStream.Fases(1).SPMProperties.molarfraction = xl
                                Me.CurrentMaterialStream.Fases(2).SPMProperties.molarfraction = xv

                                i = 1
                                For Each subst In Me.CurrentMaterialStream.Fases(1).Componentes.Values
                                    subst.FracaoMolar = result(0, i)
                                    subst.FugacityCoeff = result(4, i)
                                    subst.ActivityCoeff = result(6, i)
                                    subst.PartialVolume = result(8, i)
                                    subst.PartialPressure = result(10, i)
                                    i += 1
                                Next
                                i = 1
                                For Each subst In Me.CurrentMaterialStream.Fases(1).Componentes.Values
                                    subst.FracaoMassica = Me.AUX_CONVERT_MOL_TO_MASS(subst.Nome, 1)
                                    i += 1
                                Next
                                i = 1
                                For Each subst In Me.CurrentMaterialStream.Fases(2).Componentes.Values
                                    subst.FracaoMolar = result(1, i)
                                    subst.FugacityCoeff = result(5, i)
                                    subst.ActivityCoeff = result(7, i)
                                    subst.PartialVolume = result(9, i)
                                    subst.PartialPressure = result(11, i)
                                    i += 1
                                Next

                                i = 1
                                For Each subst In Me.CurrentMaterialStream.Fases(2).Componentes.Values
                                    subst.FracaoMassica = Me.AUX_CONVERT_MOL_TO_MASS(subst.Nome, 2)
                                    i += 1
                                Next

                                Me.CurrentMaterialStream.Fases(1).SPMProperties.massfraction = xl * Me.AUX_MMM(Fase.Liquid) / (xl * Me.AUX_MMM(Fase.Liquid) + xv * Me.AUX_MMM(Fase.Vapor))
                                Me.CurrentMaterialStream.Fases(2).SPMProperties.massfraction = xv * Me.AUX_MMM(Fase.Vapor) / (xl * Me.AUX_MMM(Fase.Liquid) + xv * Me.AUX_MMM(Fase.Vapor))

                                Dim HM, HV, HL, HL1, HL2, xl1, xl2 As Double
                                Dim wi As Integer
                                If xl <> 0 Then
                                    wi = Me.GetWaterIndex(Vx)
                                    If wi <> -1 Then
                                        'there is water in the mixture
                                        xl1 = xl * (1 - Vx(wi))
                                        xl2 = xl - xl1
                                        Vx1 = Vx.Clone
                                        Vx2 = Vx.Clone
                                        Vx1(wi) = 0
                                        Vx1 = Me.AUX_NORMALIZE(Vx1)
                                        Vx2 = Me.AUX_ERASE(Vx)
                                        Vx2(wi) = 1
                                        HL1 = Me.m_pr.H_PR_MIX("L", T, P, Vx1, RET_VKij(), RET_VTC, RET_VPC, RET_VW, RET_VMM, Me.RET_Hid(298.15, T, Vx1)) * Me.AUX_MMM(Vx1)
                                        HL2 = Me.m_pr.H_PR_MIX("L", T, P, Vx2, RET_VKij(), RET_VTC, RET_VPC, RET_VW, RET_VMM, Me.RET_Hid(298.15, T, Vx2)) * Me.AUX_MMM(Vx2)
                                    Else
                                        'there is NO water in the mixture
                                        xl1 = xl
                                        xl2 = 0
                                        Vx1 = Vx.Clone
                                        Vx2 = Me.AUX_ERASE(Vx)
                                        HL1 = Me.m_pr.H_PR_MIX("L", T, P, Vx1, RET_VKij(), RET_VTC, RET_VPC, RET_VW, RET_VMM, Me.RET_Hid(298.15, T, Vx1)) * Me.AUX_MMM(Vx1)
                                        HL2 = 0
                                    End If
                                End If

                                If Double.IsNaN(HL1) Then HL1 = 0.0#
                                If Double.IsNaN(HL2) Then HL2 = 0.0#

                                HL = xl1 * HL1 / Me.AUX_MMM(Vx1) + xl2 * HL2 / Me.AUX_MMM(Vx2)
                                If xv <> 0 Then HV = Me.m_pr.H_PR_MIX("V", T, P, Vy, RET_VKij(), RET_VTC, RET_VPC, RET_VW, RET_VMM, Me.RET_Hid(298.15, T, Vy)) * Me.AUX_MMM(Vy)
                                HM = (xl1 * HL1 + xl2 * HL2 + xv * HV) / Me.AUX_MMM(Fase.Mixture)

                                H = HM

                                i = 0
                                For Each subst In Me.CurrentMaterialStream.Fases(Me.RET_PHASEID(Fase.Liquid1)).Componentes.Values
                                    subst.FracaoMolar = Vx1(i)
                                    i += 1
                                Next
                                For Each subst In Me.CurrentMaterialStream.Fases(Me.RET_PHASEID(Fase.Liquid1)).Componentes.Values
                                    subst.FracaoMassica = Me.AUX_CONVERT_MOL_TO_MASS(subst.Nome, Me.RET_PHASEID(Fase.Liquid1))
                                Next
                                i = 0
                                For Each subst In Me.CurrentMaterialStream.Fases(Me.RET_PHASEID(Fase.Aqueous)).Componentes.Values
                                    subst.FracaoMolar = Vx2(i)
                                    i += 1
                                Next
                                For Each subst In Me.CurrentMaterialStream.Fases(Me.RET_PHASEID(Fase.Aqueous)).Componentes.Values
                                    subst.FracaoMassica = Me.AUX_CONVERT_MOL_TO_MASS(subst.Nome, Me.RET_PHASEID(Fase.Aqueous))
                                Next
                                Me.CurrentMaterialStream.Fases(Me.RET_PHASEID(Fase.Liquid1)).SPMProperties.molarfraction = xl1
                                Me.CurrentMaterialStream.Fases(Me.RET_PHASEID(Fase.Aqueous)).SPMProperties.molarfraction = xl2
                                Me.CurrentMaterialStream.Fases(Me.RET_PHASEID(Fase.Liquid1)).SPMProperties.massfraction = xl1 * Me.AUX_MMM(Vx1) / (xl1 * Me.AUX_MMM(Vx1) + xl2 * Me.AUX_MMM(Vx2) + xv * Me.AUX_MMM(Fase.Vapor))
                                Me.CurrentMaterialStream.Fases(Me.RET_PHASEID(Fase.Aqueous)).SPMProperties.massfraction = xl2 * Me.AUX_MMM(Vx2) / (xl1 * Me.AUX_MMM(Vx1) + xl2 * Me.AUX_MMM(Vx2) + xv * Me.AUX_MMM(Fase.Vapor))

                            End If

                    End Select

            End Select

            With Me.CurrentMaterialStream
                .Fases(0).SPMProperties.temperature = T
                .Fases(0).SPMProperties.pressure = P
                .Fases(0).SPMProperties.enthalpy = H
                .Fases(0).SPMProperties.entropy = S

                If xl <= 0.00001 Then
                    xl = 0
                    With .Fases(1).SPMProperties
                        .activity = 0
                        .activityCoefficient = 0
                        .compressibility = 0
                        .compressibilityFactor = 0
                        .density = 0
                        .enthalpy = 0
                        .entropy = 0
                        .excessEnthalpy = 0
                        .excessEntropy = 0
                        .fugacity = 0
                        .fugacityCoefficient = 0
                        .heatCapacityCp = 0
                        .heatCapacityCv = 0
                        .jouleThomsonCoefficient = 0
                        .kinematic_viscosity = 0
                        .logFugacityCoefficient = 0
                        .massflow = 0
                        .massfraction = 0
                        .molarflow = 0
                        .molarfraction = 0
                        .molecularWeight = 0
                        .pressure = 0
                        .speedOfSound = 0
                        .temperature = 0
                        .thermalConductivity = 0
                        .viscosity = 0
                        .volumetric_flow = 0
                    End With
                ElseIf xv <= 0.00001 Then
                    xv = 0
                    With .Fases(2).SPMProperties
                        .activity = 0
                        .activityCoefficient = 0
                        .compressibility = 0
                        .compressibilityFactor = 0
                        .density = 0
                        .enthalpy = 0
                        .entropy = 0
                        .excessEnthalpy = 0
                        .excessEntropy = 0
                        .fugacity = 0
                        .fugacityCoefficient = 0
                        .heatCapacityCp = 0
                        .heatCapacityCv = 0
                        .jouleThomsonCoefficient = 0
                        .kinematic_viscosity = 0
                        .logFugacityCoefficient = 0
                        .massflow = 0
                        .massfraction = 0
                        .molarflow = 0
                        .molarfraction = 0
                        .molecularWeight = 0
                        .pressure = 0
                        .speedOfSound = 0
                        .temperature = 0
                        .thermalConductivity = 0
                        .viscosity = 0
                        .volumetric_flow = 0
                    End With
                End If

            End With

        End Sub

        Public Overrides Function DW_CalcK_ISOL(ByVal fase1 As DWSIM.SimulationObjects.PropertyPackages.Fase, ByVal T As Double, ByVal P As Double) As Double
            If fase1 = Fase.Liquid Then
                Return Me.AUX_CONDTL(T)
            ElseIf fase1 = Fase.Liquid1 Then
                Return Me.AUX_CONDTG(T, Me.RET_PHASEID(Fase.Liquid1))
            ElseIf fase1 = Fase.Aqueous Then
                Return Me.AUX_CONDTL(T, Me.RET_PHASEID(Fase.Aqueous))
            ElseIf fase1 = Fase.Vapor Then
                Return Me.AUX_CONDTG(T, P)
            End If
        End Function

        Public Overrides Function DW_CalcMassaEspecifica_ISOL(ByVal fase1 As DWSIM.SimulationObjects.PropertyPackages.Fase, ByVal T As Double, ByVal P As Double, Optional ByVal pvp As Double = 0) As Double
            If fase1 = Fase.Liquid Then
                Return Me.AUX_LIQDENS(T)
            ElseIf fase1 = Fase.Liquid1 Then
                Return Me.AUX_LIQDENS(T, 0, 0, Me.RET_PHASEID(Fase.Liquid1), False)
            ElseIf fase1 = Fase.Aqueous Then
                Return Me.AUX_LIQDENS(T, 0, 0, Me.RET_PHASEID(Fase.Aqueous), False)
            ElseIf fase1 = Fase.Vapor Then
                Return Me.AUX_VAPDENS(T, P)
            ElseIf fase1 = Fase.Mixture Then
                Return Me.CurrentMaterialStream.Fases(1).SPMProperties.volumetric_flow.GetValueOrDefault * Me.AUX_LIQDENS(T) / Me.CurrentMaterialStream.Fases(0).SPMProperties.volumetric_flow.GetValueOrDefault + Me.CurrentMaterialStream.Fases(2).SPMProperties.volumetric_flow.GetValueOrDefault * Me.AUX_VAPDENS(T, P) / Me.CurrentMaterialStream.Fases(0).SPMProperties.volumetric_flow.GetValueOrDefault
            End If
        End Function

        Public Overrides Function DW_CalcMM_ISOL(ByVal fase1 As DWSIM.SimulationObjects.PropertyPackages.Fase, ByVal T As Double, ByVal P As Double) As Double
            Return Me.AUX_MMM(fase1)
        End Function

        Public Overrides Sub DW_CalcOverallProps()
            MyBase.DW_CalcOverallProps()
        End Sub

        Public Function AUX_CM(ByVal Vx As Object) As Double

            Dim val As Double
            Dim subst As DWSIM.ClassesBasicasTermodinamica.Substancia

            Dim i As Integer = 0
            For Each subst In Me.CurrentMaterialStream.Fases(0).Componentes.Values
                val += Vx(i) * subst.ConstantProperties.PR_Volume_Translation_Coefficient * Me.m_pr.bi(0.0778, subst.ConstantProperties.Critical_Temperature, subst.ConstantProperties.Critical_Pressure)
                i += 1
            Next

            Return val

        End Function

        Public Function AUX_CM(ByVal fase As Fase) As Double

            Dim val As Double
            Dim subst As DWSIM.ClassesBasicasTermodinamica.Substancia

            For Each subst In Me.CurrentMaterialStream.Fases(Me.RET_PHASEID(fase)).Componentes.Values
                val += subst.FracaoMolar.GetValueOrDefault * subst.ConstantProperties.PR_Volume_Translation_Coefficient * Me.m_pr.bi(0.0778, subst.ConstantProperties.Critical_Temperature, subst.ConstantProperties.Critical_Pressure)
            Next

            Return val

        End Function

        Public Function RET_VS() As Double()

            Dim val(Me.CurrentMaterialStream.Fases(0).Componentes.Count - 1) As Double
            Dim subst As DWSIM.ClassesBasicasTermodinamica.Substancia
            Dim i As Integer = 0

            For Each subst In Me.CurrentMaterialStream.Fases(0).Componentes.Values
                val(i) = subst.ConstantProperties.PR_Volume_Translation_Coefficient
                i += 1
            Next

            Return val

        End Function

        Public Function RET_VC() As Double()

            Dim val(Me.CurrentMaterialStream.Fases(0).Componentes.Count - 1) As Double
            Dim subst As DWSIM.ClassesBasicasTermodinamica.Substancia
            Dim i As Integer = 0

            For Each subst In Me.CurrentMaterialStream.Fases(0).Componentes.Values
                val(i) = subst.ConstantProperties.PR_Volume_Translation_Coefficient * Me.m_pr.bi(0.0778, subst.ConstantProperties.Critical_Temperature, subst.ConstantProperties.Critical_Pressure)
                i += 1
            Next

            Return val

        End Function

        Public Overrides Sub DW_CalcPhaseProps(ByVal fase As DWSIM.SimulationObjects.PropertyPackages.Fase)

            Dim result As Double
            Dim resultObj As Object

            Dim T, P As Double
            Dim phasemolarfrac As Double = Nothing
            Dim overallmolarflow As Double = Nothing

            Dim phaseID As Integer
            T = Me.CurrentMaterialStream.Fases(0).SPMProperties.temperature
            P = Me.CurrentMaterialStream.Fases(0).SPMProperties.pressure

            phaseID = Me.RET_PHASEID(fase)

            If phaseID > 0 Then
                overallmolarflow = Me.CurrentMaterialStream.Fases(0).SPMProperties.molarflow.GetValueOrDefault
                phasemolarfrac = Me.CurrentMaterialStream.Fases(phaseID).SPMProperties.molarfraction.GetValueOrDefault
                result = overallmolarflow * phasemolarfrac
                Me.CurrentMaterialStream.Fases(phaseID).SPMProperties.molarflow = result
                result = result * Me.AUX_MMM(fase) / 1000
                Me.CurrentMaterialStream.Fases(phaseID).SPMProperties.massflow = result
                result = phasemolarfrac * overallmolarflow * Me.AUX_MMM(fase) / 1000 / Me.CurrentMaterialStream.Fases(0).SPMProperties.massflow.GetValueOrDefault
                Me.CurrentMaterialStream.Fases(phaseID).SPMProperties.massfraction = result
                Me.DW_CalcCompVolFlow(phaseID)
                Me.DW_CalcCompFugCoeff(fase)
            End If

            If phaseID > 0 Then

                If fase = PropertyPackages.Fase.Liquid Or fase = PropertyPackages.Fase.Liquid1 Or fase = PropertyPackages.Fase.Aqueous Then

                    If CInt(Me.Parameters("PP_USE_EOS_LIQDENS")) = 1 Then
                        Dim val As Double
                        val = m_pr.Z_PR(T, P, RET_VMOL(fase), RET_VKij(), RET_VTC, RET_VPC, RET_VW, "L")
                        val = 8.314 * val * T / P
                        val = val - Me.AUX_CM(RET_VMOL(fase))
                        val = 1 / val * Me.AUX_MMM(fase) / 1000
                        result = val
                    Else
                        result = Me.AUX_LIQDENS(T, P, , phaseID, False)
                    End If
                    Me.CurrentMaterialStream.Fases(phaseID).SPMProperties.density = result
                    result = Me.m_pr.H_PR_MIX("L", T, P, RET_VMOL(fase), RET_VKij(), RET_VTC(), RET_VPC(), RET_VW(), RET_VMM(), Me.RET_Hid(298.15, T, fase))
                    Me.CurrentMaterialStream.Fases(phaseID).SPMProperties.enthalpy = result
                    result = Me.m_pr.S_PR_MIX("L", T, P, RET_VMOL(fase), RET_VKij(), RET_VTC(), RET_VPC(), RET_VW(), RET_VMM(), Me.RET_Sid(298.15, T, P, fase))
                    Me.CurrentMaterialStream.Fases(phaseID).SPMProperties.entropy = result
                    result = Me.m_pr.Z_PR(T, P, RET_VMOL(fase), RET_VKij(), RET_VTC, RET_VPC, RET_VW, "L")
                    Me.CurrentMaterialStream.Fases(phaseID).SPMProperties.compressibilityFactor = result
                    resultObj = Me.m_props.CpCvR("L", T, P, RET_VMOL(fase), RET_VKij(), RET_VMAS(fase), RET_VTC(), RET_VPC(), RET_VCP(T), RET_VMM(), RET_VW(), RET_VZRa())
                    Me.CurrentMaterialStream.Fases(phaseID).SPMProperties.heatCapacityCp = resultObj(1)
                    Me.CurrentMaterialStream.Fases(phaseID).SPMProperties.heatCapacityCv = resultObj(2)
                    result = Me.AUX_MMM(fase)
                    Me.CurrentMaterialStream.Fases(phaseID).SPMProperties.molecularWeight = result
                    result = Me.CurrentMaterialStream.Fases(phaseID).SPMProperties.enthalpy.GetValueOrDefault * Me.CurrentMaterialStream.Fases(phaseID).SPMProperties.molecularWeight.GetValueOrDefault
                    Me.CurrentMaterialStream.Fases(phaseID).SPMProperties.molar_enthalpy = result
                    result = Me.CurrentMaterialStream.Fases(phaseID).SPMProperties.entropy.GetValueOrDefault * Me.CurrentMaterialStream.Fases(phaseID).SPMProperties.molecularWeight.GetValueOrDefault
                    Me.CurrentMaterialStream.Fases(phaseID).SPMProperties.molar_entropy = result
                    result = Me.AUX_CONDTL(T, phaseID)
                    Me.CurrentMaterialStream.Fases(phaseID).SPMProperties.thermalConductivity = result
                    result = Me.AUX_LIQVISCm(T, phaseID)
                    Me.CurrentMaterialStream.Fases(phaseID).SPMProperties.viscosity = result
                    Me.CurrentMaterialStream.Fases(phaseID).SPMProperties.kinematic_viscosity = result / Me.CurrentMaterialStream.Fases(phaseID).SPMProperties.density.Value

                ElseIf phaseID = 2 Then

                    result = Me.m_pr.Z_PR(T, P, RET_VMOL(fase.Vapor), RET_VKij, RET_VTC, RET_VPC, RET_VW, "V")
                    Me.CurrentMaterialStream.Fases(phaseID).SPMProperties.compressibilityFactor = result
                    result = Me.AUX_VAPDENS(T, P)
                    Me.CurrentMaterialStream.Fases(phaseID).SPMProperties.density = result
                    result = Me.m_pr.H_PR_MIX("V", T, P, RET_VMOL(fase.Vapor), RET_VKij(), RET_VTC(), RET_VPC(), RET_VW(), RET_VMM(), Me.RET_Hid(298.15, T, fase.Vapor))
                    Me.CurrentMaterialStream.Fases(phaseID).SPMProperties.enthalpy = result
                    result = Me.m_pr.S_PR_MIX("V", T, P, RET_VMOL(fase.Vapor), RET_VKij(), RET_VTC(), RET_VPC(), RET_VW(), RET_VMM(), Me.RET_Sid(298.15, T, P, fase.Vapor))
                    Me.CurrentMaterialStream.Fases(phaseID).SPMProperties.entropy = result
                    result = Me.AUX_CPm(PropertyPackages.Fase.Vapor, T)
                    resultObj = Me.m_props.CpCvR("V", T, P, RET_VMOL(PropertyPackages.Fase.Vapor), RET_VKij(), RET_VMAS(PropertyPackages.Fase.Vapor), RET_VTC(), RET_VPC(), RET_VCP(T), RET_VMM(), RET_VW(), RET_VZRa())
                    Me.CurrentMaterialStream.Fases(phaseID).SPMProperties.heatCapacityCp = resultObj(1)
                    Me.CurrentMaterialStream.Fases(phaseID).SPMProperties.heatCapacityCv = resultObj(2)
                    result = Me.AUX_MMM(fase)
                    Me.CurrentMaterialStream.Fases(phaseID).SPMProperties.molecularWeight = result
                    result = Me.CurrentMaterialStream.Fases(phaseID).SPMProperties.enthalpy.GetValueOrDefault * Me.CurrentMaterialStream.Fases(phaseID).SPMProperties.molecularWeight.GetValueOrDefault
                    Me.CurrentMaterialStream.Fases(phaseID).SPMProperties.molar_enthalpy = result
                    result = Me.CurrentMaterialStream.Fases(phaseID).SPMProperties.entropy.GetValueOrDefault * Me.CurrentMaterialStream.Fases(phaseID).SPMProperties.molecularWeight.GetValueOrDefault
                    Me.CurrentMaterialStream.Fases(phaseID).SPMProperties.molar_entropy = result
                    result = Me.AUX_CONDTG(T, P)
                    Me.CurrentMaterialStream.Fases(phaseID).SPMProperties.thermalConductivity = result
                    result = Me.AUX_VAPVISCm(T, Me.CurrentMaterialStream.Fases(phaseID).SPMProperties.density.GetValueOrDefault, Me.AUX_MMM(fase))
                    Me.CurrentMaterialStream.Fases(phaseID).SPMProperties.viscosity = result
                    Me.CurrentMaterialStream.Fases(phaseID).SPMProperties.kinematic_viscosity = result / Me.CurrentMaterialStream.Fases(phaseID).SPMProperties.density.Value

                End If

            Else

                Dim HL, HL1, HL2, HV, SL, SL1, SL2, SV, DL, DL1, DL2, DV, CPL, CPL1, CPL2, CPV, KL, KL1, KL2, KV, CVL, CVL1, CVL2, CVV As Nullable(Of Double)
                Dim xl, xl1, xl2, xv, wl, wl1, wl2, wv, vl, vv, dl0, dv0 As Double

                xl = Me.CurrentMaterialStream.Fases(1).SPMProperties.molarfraction.GetValueOrDefault
                xl1 = Me.CurrentMaterialStream.Fases(Me.RET_PHASEID(PropertyPackages.Fase.Liquid1)).SPMProperties.molarfraction.GetValueOrDefault
                xl2 = Me.CurrentMaterialStream.Fases(Me.RET_PHASEID(PropertyPackages.Fase.Aqueous)).SPMProperties.molarfraction.GetValueOrDefault
                xv = Me.CurrentMaterialStream.Fases(2).SPMProperties.molarfraction.GetValueOrDefault

                wl = Me.CurrentMaterialStream.Fases(1).SPMProperties.massfraction.GetValueOrDefault
                wl1 = Me.CurrentMaterialStream.Fases(Me.RET_PHASEID(PropertyPackages.Fase.Liquid1)).SPMProperties.massfraction.GetValueOrDefault
                wl2 = Me.CurrentMaterialStream.Fases(Me.RET_PHASEID(PropertyPackages.Fase.Aqueous)).SPMProperties.massfraction.GetValueOrDefault
                wv = Me.CurrentMaterialStream.Fases(2).SPMProperties.massfraction.GetValueOrDefault

                DL = Me.CurrentMaterialStream.Fases(1).SPMProperties.density
                DL1 = Me.CurrentMaterialStream.Fases(Me.RET_PHASEID(PropertyPackages.Fase.Liquid1)).SPMProperties.density
                DL2 = Me.CurrentMaterialStream.Fases(Me.RET_PHASEID(PropertyPackages.Fase.Aqueous)).SPMProperties.density
                DV = Me.CurrentMaterialStream.Fases(2).SPMProperties.density

                vl = wl / DL.GetValueOrDefault / (wl / DL.GetValueOrDefault + wv / DV.GetValueOrDefault)
                vv = wv / DV.GetValueOrDefault / (wl / DL.GetValueOrDefault + wv / DV.GetValueOrDefault)

                If xl = 1 Then
                    vl = 1
                    vv = 0
                ElseIf xl = 0 Then
                    vl = 0
                    vv = 1
                End If

                result = vl * DL.GetValueOrDefault + vv * DV.GetValueOrDefault
                If Double.IsNaN(result) Then
                    If Double.TryParse(DL.ToString, dl0) = True And Double.TryParse(DV.ToString, dv0) = False Then
                        result = dl0
                    ElseIf Double.TryParse(DL.ToString, dl0) = False And Double.TryParse(DV.ToString, dv0) = True Then
                        result = dv0
                    Else
                        result = 0
                    End If
                End If
                Me.CurrentMaterialStream.Fases(phaseID).SPMProperties.density = result

                HL1 = Me.CurrentMaterialStream.Fases(Me.RET_PHASEID(fase.Liquid1)).SPMProperties.enthalpy
                HL2 = Me.CurrentMaterialStream.Fases(Me.RET_PHASEID(fase.Aqueous)).SPMProperties.enthalpy
                HL = wl1 * HL1.GetValueOrDefault + wl2 * HL2.GetValueOrDefault / (wl1 + wl2)
                HV = Me.CurrentMaterialStream.Fases(2).SPMProperties.enthalpy

                result = wl1 * HL1.GetValueOrDefault + wl2 * HL2.GetValueOrDefault + wv * HV.GetValueOrDefault
                Me.CurrentMaterialStream.Fases(phaseID).SPMProperties.enthalpy = result

                SL1 = Me.CurrentMaterialStream.Fases(Me.RET_PHASEID(fase.Liquid1)).SPMProperties.entropy
                SL2 = Me.CurrentMaterialStream.Fases(Me.RET_PHASEID(fase.Aqueous)).SPMProperties.entropy
                SL = wl1 * SL1.GetValueOrDefault + wl2 * SL2.GetValueOrDefault / (wl1 + wl2)
                SV = Me.CurrentMaterialStream.Fases(2).SPMProperties.entropy

                result = wl1 * SL1.GetValueOrDefault + wl2 * SL2.GetValueOrDefault + wv * HV.GetValueOrDefault
                Me.CurrentMaterialStream.Fases(phaseID).SPMProperties.entropy = result

                Me.CurrentMaterialStream.Fases(phaseID).SPMProperties.compressibilityFactor = Nothing

                CPL = Me.CurrentMaterialStream.Fases(1).SPMProperties.heatCapacityCp
                CPL1 = Me.CurrentMaterialStream.Fases(Me.RET_PHASEID(fase.Liquid1)).SPMProperties.heatCapacityCp
                CPL2 = Me.CurrentMaterialStream.Fases(Me.RET_PHASEID(fase.Aqueous)).SPMProperties.heatCapacityCp
                CPV = Me.CurrentMaterialStream.Fases(2).SPMProperties.heatCapacityCp

                result = wl1 * CPL1.GetValueOrDefault + wl2 * CPL2.GetValueOrDefault + wv * CPV.GetValueOrDefault
                Me.CurrentMaterialStream.Fases(phaseID).SPMProperties.heatCapacityCp = result

                CVL = Me.CurrentMaterialStream.Fases(1).SPMProperties.heatCapacityCv
                CVL1 = Me.CurrentMaterialStream.Fases(Me.RET_PHASEID(fase.Liquid1)).SPMProperties.heatCapacityCv
                CVL2 = Me.CurrentMaterialStream.Fases(Me.RET_PHASEID(fase.Aqueous)).SPMProperties.heatCapacityCv
                CVV = Me.CurrentMaterialStream.Fases(2).SPMProperties.heatCapacityCv

                result = wl1 * CVL1.GetValueOrDefault + wl2 * CVL2.GetValueOrDefault + wv * CVV.GetValueOrDefault
                Me.CurrentMaterialStream.Fases(phaseID).SPMProperties.heatCapacityCv = result

                result = Me.AUX_MMM(fase)
                Me.CurrentMaterialStream.Fases(phaseID).SPMProperties.molecularWeight = result

                result = Me.CurrentMaterialStream.Fases(phaseID).SPMProperties.enthalpy.GetValueOrDefault * Me.CurrentMaterialStream.Fases(phaseID).SPMProperties.molecularWeight.GetValueOrDefault
                Me.CurrentMaterialStream.Fases(phaseID).SPMProperties.molar_enthalpy = result
                result = Me.CurrentMaterialStream.Fases(phaseID).SPMProperties.entropy.GetValueOrDefault * Me.CurrentMaterialStream.Fases(phaseID).SPMProperties.molecularWeight.GetValueOrDefault
                Me.CurrentMaterialStream.Fases(phaseID).SPMProperties.molar_entropy = result

                KL = Me.CurrentMaterialStream.Fases(1).SPMProperties.thermalConductivity
                KL1 = Me.CurrentMaterialStream.Fases(Me.RET_PHASEID(fase.Liquid1)).SPMProperties.thermalConductivity
                KL2 = Me.CurrentMaterialStream.Fases(Me.RET_PHASEID(fase.Aqueous)).SPMProperties.thermalConductivity
                KV = Me.CurrentMaterialStream.Fases(2).SPMProperties.thermalConductivity

                result = xl1 * KL1.GetValueOrDefault + xl2 * KL2.GetValueOrDefault + xv * KV.GetValueOrDefault
                Me.CurrentMaterialStream.Fases(phaseID).SPMProperties.thermalConductivity = result

                Me.CurrentMaterialStream.Fases(phaseID).SPMProperties.viscosity = Nothing

                Me.CurrentMaterialStream.Fases(phaseID).SPMProperties.kinematic_viscosity = Nothing

            End If

            If phaseID > 0 Then
                result = overallmolarflow * phasemolarfrac * Me.AUX_MMM(fase) / 1000 / Me.CurrentMaterialStream.Fases(phaseID).SPMProperties.density.GetValueOrDefault
                Me.CurrentMaterialStream.Fases(phaseID).SPMProperties.volumetric_flow = result
            Else
                'result = Me.CurrentMaterialStream.Fases(phaseID).SPMProperties.massflow.GetValueOrDefault / Me.CurrentMaterialStream.Fases(phaseID).SPMProperties.density.GetValueOrDefault
                'Me.CurrentMaterialStream.Fases(phaseID).SPMProperties.volumetric_flow = result
            End If


        End Sub

        Public Overrides Function DW_CalcPVAP_ISOL(ByVal T As Double) As Double
            Return Me.m_props.Pvp_leekesler(T, Me.RET_VTC(Fase.Liquid), Me.RET_VPC(Fase.Liquid), Me.RET_VW(Fase.Liquid))
        End Function

        Public Overrides Function DW_CalcTensaoSuperficial_ISOL(ByVal fase1 As DWSIM.SimulationObjects.PropertyPackages.Fase, ByVal T As Double, ByVal P As Double) As Double
            Return Me.AUX_SURFTM(T)
        End Function

        Public Overrides Sub DW_CalcTwoPhaseProps(ByVal fase1 As DWSIM.SimulationObjects.PropertyPackages.Fase, ByVal fase2 As DWSIM.SimulationObjects.PropertyPackages.Fase)

            Dim T As Double

            T = Me.CurrentMaterialStream.Fases(0).SPMProperties.temperature
            Me.CurrentMaterialStream.Fases(0).TPMProperties.surfaceTension = Me.AUX_SURFTM(T)

        End Sub

        Public Overrides Sub DW_CalcVazaoMassica()
            With Me.CurrentMaterialStream
                .Fases(0).SPMProperties.massflow = .Fases(0).SPMProperties.molarflow.GetValueOrDefault * Me.AUX_MMM(Fase.Mixture) / 1000
            End With
        End Sub

        Public Overrides Sub DW_CalcVazaoMolar()
            With Me.CurrentMaterialStream
                .Fases(0).SPMProperties.molarflow = .Fases(0).SPMProperties.massflow.GetValueOrDefault / Me.AUX_MMM(Fase.Mixture) * 1000
            End With
        End Sub

        Public Overrides Sub DW_CalcVazaoVolumetrica()
            With Me.CurrentMaterialStream
                .Fases(0).SPMProperties.volumetric_flow = .Fases(0).SPMProperties.massflow.GetValueOrDefault / .Fases(0).SPMProperties.density.GetValueOrDefault
            End With
        End Sub

        Public Overrides Function DW_CalcViscosidadeDinamica_ISOL(ByVal fase1 As DWSIM.SimulationObjects.PropertyPackages.Fase, ByVal T As Double, ByVal P As Double) As Double
            If fase1 = Fase.Liquid Then
                Return Me.AUX_LIQVISCm(T)
            ElseIf fase1 = Fase.Vapor Then
                Return Me.AUX_VAPVISCm(T, Me.AUX_VAPDENS(T, P), Me.AUX_MMM(Fase.Vapor))
            End If
        End Function

        Public Overrides Function SupportsComponent(ByVal comp As ClassesBasicasTermodinamica.ConstantProperties) As Boolean

            Return True

            'If Me.SupportedComponents.Contains(comp.ID) Then
            '    Return True
            'ElseIf comp.IsPF = 1 Then
            '    Return True
            'ElseIf comp.IsHYPO = 1 Then
            '    Return True
            'Else
            '    Return False
            'End If

        End Function


        Public Overrides Function DW_ReturnPhaseEnvelope(ByVal parameters As Object) As Object

            Dim cpc As New DWSIM.Utilities.TCP.Methods

            Dim i As Integer

            Dim n As Integer = Me.CurrentMaterialStream.Fases(0).Componentes.Count - 1

            Dim Vz(n) As Double
            Dim comp As DWSIM.ClassesBasicasTermodinamica.Substancia

            i = 0
            For Each comp In Me.CurrentMaterialStream.Fases(0).Componentes.Values
                Vz(i) += comp.FracaoMolar.GetValueOrDefault
                i += 1
            Next

            Dim j, k, l As Integer
            i = 0
            Do
                If Vz(i) = 0 Then j += 1
                i = i + 1
            Loop Until i = n + 1

            Dim VTc(n), Vpc(n), Vw(n), VVc(n), VKij(n, n) As Double
            Dim Vm2(UBound(Vz) - j), VPc2(UBound(Vz) - j), VTc2(UBound(Vz) - j), VVc2(UBound(Vz) - j), Vw2(UBound(Vz) - j), VKij2(UBound(Vz) - j, UBound(Vz) - j)

            VTc = Me.RET_VTC()
            Vpc = Me.RET_VPC()
            VVc = Me.RET_VVC()
            Vw = Me.RET_VW()
            VKij = Me.RET_VKij

            i = 0
            k = 0
            Do
                If Vz(i) <> 0 Then
                    Vm2(k) = Vz(i)
                    VTc2(k) = VTc(i)
                    VPc2(k) = Vpc(i)
                    VVc2(k) = VVc(i)
                    Vw2(k) = Vw(i)
                    j = 0
                    l = 0
                    Do
                        If Vz(l) <> 0 Then
                            VKij2(k, j) = VKij(i, l)
                            j = j + 1
                        End If
                        l = l + 1
                    Loop Until l = n + 1
                    k = k + 1
                End If
                i = i + 1
            Loop Until i = n + 1

            Dim Pmin, Tmin, dP, dT, T, P As Double
            Dim PB, PO, TVB, TVD, HB, HO, SB, SO, VB, VO, TE, PE, TH, PHsI, PHsII, TQ, PQ As New ArrayList
            Dim TCR, PCR, VCR As Double

            Dim CP As New ArrayList
            If n > 0 Then
                CP = cpc.CRITPT_PR(Vm2, VTc2, VPc2, VVc2, Vw2, VKij2)
                If CP.Count > 0 Then
                    Dim cp0 = CP(0)
                    TCR = cp0(0)
                    PCR = cp0(1)
                    VCR = cp0(2)
                Else
                    TCR = 0
                    PCR = 0
                    VCR = 0
                End If
            Else
                TCR = Me.AUX_TCM(Fase.Mixture)
                PCR = Me.AUX_PCM(Fase.Mixture)
                VCR = Me.AUX_VCM(Fase.Mixture)
                CP.Add(New Object() {TCR, PCR, VCR})
            End If

            Pmin = 101325
            Tmin = 0.3 * TCR

            dP = 2 * 101325 '(PCR - Pmin) / 7
            dT = 2 '(Tmax - Tmin) / 7

            Dim beta As Double = 10

            Dim tmp2 As Object
            Dim KI(n) As Double

            j = 0
            Do
                KI(j) = 0
                j = j + 1
            Loop Until j = n + 1

            i = 0
            P = Pmin
            T = Tmin
            Do
                If i < 2 Then
                    tmp2 = BUBP_PR_M2(T, Me.RET_VMOL(Fase.Mixture), Me.RET_VKij, Me.RET_VTC, Me.RET_VPC, Me.RET_VTB, Me.RET_VW, KI, RET_VC)
                    TVB.Add(T)
                    PB.Add(tmp2(0))
                    P = PB(i)
                    HB.Add(Me.m_pr.H_PR_MIX("L", T, P, RET_VMOL(Fase.Mixture), RET_VKij(), RET_VTC, RET_VPC, RET_VW, RET_VMM, Me.RET_Hid(298.15, T, Fase.Mixture)))
                    SB.Add(Me.m_pr.S_PR_MIX("L", T, P, RET_VMOL(Fase.Mixture), RET_VKij(), RET_VTC, RET_VPC, RET_VW, RET_VMM, Me.RET_Sid(298.15, T, P, Fase.Mixture)))
                    VB.Add(Me.m_pr.Z_PR(T, P, RET_VMOL(Fase.Mixture), RET_VKij(), RET_VTC, RET_VPC, RET_VW, "L") * 8.314 * T / P)
                    T = T + dT
                    j = 0
                    Do
                        KI(j) = tmp2(j + 1)
                        j = j + 1
                    Loop Until j = n + 1
                Else
                    If beta < 20 Then
                        tmp2 = BUBP_PR_M2(T, Me.RET_VMOL(Fase.Mixture), Me.RET_VKij, Me.RET_VTC, Me.RET_VPC, Me.RET_VTB, Me.RET_VW, KI, RET_VC, PB(i - 1))
                        TVB.Add(T)
                        PB.Add(tmp2(0))
                        P = PB(i)
                        HB.Add(Me.m_pr.H_PR_MIX("L", T, P, RET_VMOL(Fase.Mixture), RET_VKij(), RET_VTC, RET_VPC, RET_VW, RET_VMM, Me.RET_Hid(298.15, T, Fase.Mixture)))
                        SB.Add(Me.m_pr.S_PR_MIX("L", T, P, RET_VMOL(Fase.Mixture), RET_VKij(), RET_VTC, RET_VPC, RET_VW, RET_VMM, Me.RET_Sid(298.15, T, P, Fase.Mixture)))
                        VB.Add(Me.m_pr.Z_PR(T, P, RET_VMOL(Fase.Mixture), RET_VKij(), RET_VTC, RET_VPC, RET_VW, "L") * 8.314 * T / P)
                        If Math.Abs(T - TCR) / TCR < 0.1 And Math.Abs(P - PCR) / PCR < 0.2 Then
                            T = T + dT * 0.25
                        Else
                            T = T + dT
                        End If
                        j = 0
                        Do
                            KI(j) = tmp2(j + 1)
                            j = j + 1
                        Loop Until j = n + 1
                    Else
                        tmp2 = BUBT_PR_M2(P, Me.RET_VMOL(Fase.Mixture), Me.RET_VKij, Me.RET_VTC, Me.RET_VPC, Me.RET_VTB, Me.RET_VW, KI, RET_VC, TVB(i - 1))
                        TVB.Add(tmp2(0))
                        PB.Add(P)
                        T = TVB(i)
                        HB.Add(Me.m_pr.H_PR_MIX("L", T, P, RET_VMOL(Fase.Mixture), RET_VKij(), RET_VTC, RET_VPC, RET_VW, RET_VMM, Me.RET_Hid(298.15, T, Fase.Mixture)))
                        SB.Add(Me.m_pr.S_PR_MIX("L", T, P, RET_VMOL(Fase.Mixture), RET_VKij(), RET_VTC, RET_VPC, RET_VW, RET_VMM, Me.RET_Sid(298.15, T, P, Fase.Mixture)))
                        VB.Add(Me.m_pr.Z_PR(T, P, RET_VMOL(Fase.Mixture), RET_VKij(), RET_VTC, RET_VPC, RET_VW, "L") * 8.314 * T / P)
                        If Math.Abs(T - TCR) / TCR < 0.1 And Math.Abs(P - PCR) / PCR < 0.1 Then
                            P = P + dP * 0.1
                        Else
                            P = P + dP
                        End If
                        j = 0
                        Do
                            KI(j) = tmp2(j + 1)
                            j = j + 1
                        Loop Until j = n + 1
                    End If
                    beta = (Math.Log(PB(i) / 101325) - Math.Log(PB(i - 1) / 101325)) / (Math.Log(TVB(i)) - Math.Log(TVB(i - 1)))
                End If
                i = i + 1
                'If i > 2 Then
                '    If TVB(i - 1) = TVB(i - 2) Then Exit Do
                'End If
            Loop Until i >= 200 Or PB(i - 1) = 0 Or PB(i - 1) < 0 Or TVB(i - 1) < 0 Or _
                        T >= TCR Or Double.IsNaN(PB(i - 1)) = True Or _
                        Double.IsNaN(TVB(i - 1)) = True Or Math.Abs(T - TCR) / TCR < 0.02 And _
                        Math.Abs(P - PCR) / PCR < 0.02

            Dim Switch = False

            beta = 10

            j = 0
            Do
                KI(j) = 0
                j = j + 1
            Loop Until j = n + 1

            i = 0
            P = Pmin
            Do
                If i < 2 Then
                    tmp2 = DEWT_PR_M2(P, Me.RET_VMOL(Fase.Mixture), Me.RET_VKij, Me.RET_VTC, Me.RET_VPC, Me.RET_VTB, Me.RET_VW, KI, RET_VC)
                    TVD.Add(tmp2(0))
                    PO.Add(P)
                    T = TVD(i)
                    HO.Add(Me.m_pr.H_PR_MIX("V", T, P, RET_VMOL(Fase.Mixture), RET_VKij(), RET_VTC, RET_VPC, RET_VW, RET_VMM, Me.RET_Hid(298.15, T, Fase.Mixture)))
                    SO.Add(Me.m_pr.S_PR_MIX("V", T, P, RET_VMOL(Fase.Mixture), RET_VKij(), RET_VTC, RET_VPC, RET_VW, RET_VMM, Me.RET_Sid(298.15, T, P, Fase.Mixture)))
                    VO.Add(Me.m_pr.Z_PR(T, P, RET_VMOL(Fase.Mixture), RET_VKij(), RET_VTC, RET_VPC, RET_VW, "V") * 8.314 * T / P)
                    If Math.Abs(T - TCR) / TCR < 0.1 And Math.Abs(P - PCR) / PCR < 0.1 Then
                        P = P + dP * 0.08
                    Else
                        P = P + dP
                    End If
                    j = 0
                    Do
                        KI(j) = tmp2(j + 1)
                        j = j + 1
                    Loop Until j = n + 1
                Else
                    If Abs(beta) < 2 Then
                        tmp2 = DEWP_PR_M2(T, Me.RET_VMOL(Fase.Mixture), Me.RET_VKij, Me.RET_VTC, Me.RET_VPC, Me.RET_VTB, Me.RET_VW, KI, RET_VC, PO(i - 1))
                        TVD.Add(T)
                        PO.Add(tmp2(0))
                        P = PO(i)
                        HO.Add(Me.m_pr.H_PR_MIX("V", T, P, RET_VMOL(Fase.Mixture), RET_VKij(), RET_VTC, RET_VPC, RET_VW, RET_VMM, Me.RET_Hid(298.15, T, Fase.Mixture)))
                        SO.Add(Me.m_pr.S_PR_MIX("V", T, P, RET_VMOL(Fase.Mixture), RET_VKij(), RET_VTC, RET_VPC, RET_VW, RET_VMM, Me.RET_Sid(298.15, T, P, Fase.Mixture)))
                        VO.Add(Me.m_pr.Z_PR(T, P, RET_VMOL(Fase.Mixture), RET_VKij(), RET_VTC, RET_VPC, RET_VW, "V") * 8.314 * T / P)
                        If Double.IsInfinity(beta) = True Then
                            T = T - dT
                        ElseIf T > TCR Then
                            If Math.Abs(T - TCR) / TCR < 0.2 And Math.Abs(P - PCR) / PCR < 0.2 Then
                                T = T - dT * 0.08
                            Else
                                T = T - dT
                            End If
                        Else
                            If Math.Abs(T - TCR) / TCR < 0.2 And Math.Abs(P - PCR) / PCR < 0.2 Then
                                T = T + dT * 0.08
                            Else
                                T = T + dT
                            End If
                        End If
                        j = 0
                        Do
                            KI(j) = tmp2(j + 1)
                            j = j + 1
                        Loop Until j = n + 1
                    Else
                        tmp2 = DEWT_PR_M2(P, Me.RET_VMOL(Fase.Mixture), Me.RET_VKij, Me.RET_VTC, Me.RET_VPC, Me.RET_VTB, Me.RET_VW, KI, RET_VC, TVD(i - 1)) 'BOLP_PR2(T, Vz2, Vids2)
                        TVD.Add(tmp2(0))
                        PO.Add(P)
                        T = TVD(i)
                        HO.Add(Me.m_pr.H_PR_MIX("V", T, P, RET_VMOL(Fase.Mixture), RET_VKij(), RET_VTC, RET_VPC, RET_VW, RET_VMM, Me.RET_Hid(298.15, T, Fase.Mixture)))
                        SO.Add(Me.m_pr.S_PR_MIX("V", T, P, RET_VMOL(Fase.Mixture), RET_VKij(), RET_VTC, RET_VPC, RET_VW, RET_VMM, Me.RET_Sid(298.15, T, P, Fase.Mixture)))
                        VO.Add(Me.m_pr.Z_PR(T, P, RET_VMOL(Fase.Mixture), RET_VKij(), RET_VTC, RET_VPC, RET_VW, "V") * 8.314 * T / P)
                        If Math.Abs(T - TCR) / TCR < 0.1 And Math.Abs(P - PCR) / PCR < 0.1 Then
                            P = P + dP * 0.1
                        Else
                            P = P + dP
                        End If
                        j = 0
                        Do
                            KI(j) = tmp2(j + 1)
                            j = j + 1
                        Loop Until j = n + 1
                    End If
                    If i >= PO.Count Then
                        i = i - 1
                    End If
                    beta = (Math.Log(PO(i) / 101325) - Math.Log(PO(i - 1) / 101325)) / (Math.Log(TVD(i)) - Math.Log(TVD(i - 1)))
                End If
                i = i + 1
                'If i > 2 Then
                '    If PO(i - 1) = PO(i - 2) Then Exit Do
                'End If
            Loop Until i >= 200 Or PO(i - 1) = 0 Or PO(i - 1) < 0 Or TVD(i - 1) < 0 Or _
                        Double.IsNaN(PO(i - 1)) = True Or Double.IsNaN(TVD(i - 1)) = True Or _
                        Math.Abs(T - TCR) / TCR < 0.02 And Math.Abs(P - PCR) / PCR < 0.02

            beta = 10

            If CBool(parameters(2)) = True Then

                j = 0
                Do
                    KI(j) = 0
                    j = j + 1
                Loop Until j = n + 1

                i = 0
                P = 400000
                T = TVD(0)
                Do
                    If i < 2 Then
                        tmp2 = FLASH_PV(P, parameters(1), Me.RET_VMOL(Fase.Mixture), Me.RET_VKij, Me.RET_VTC, Me.RET_VPC, Me.RET_VTB, Me.RET_VW, KI, RET_VC)
                        TQ.Add(tmp2(0))
                        PQ.Add(P)
                        T = TQ(i)
                        P = P + dP
                        j = 0
                        Do
                            KI(j) = tmp2(j + 1)
                            j = j + 1
                        Loop Until j = n + 1
                    Else
                        If beta < 2 Then
                            tmp2 = FLASH_TV(T, parameters(1), Me.RET_VMOL(Fase.Mixture), Me.RET_VKij, Me.RET_VTC, Me.RET_VPC, Me.RET_VTB, Me.RET_VW, KI, RET_VC, PQ(i - 1))
                            TQ.Add(T)
                            PQ.Add(tmp2(0))
                            P = PQ(i)
                            If Math.Abs(T - TCR) / TCR < 0.1 And Math.Abs(P - PCR) / PCR < 0.2 Then
                                T = T + dT * 0.25
                            Else
                                T = T + dT
                            End If
                            j = 0
                            Do
                                KI(j) = tmp2(j + 1)
                                j = j + 1
                            Loop Until j = n + 1
                        Else
                            tmp2 = FLASH_PV(P, parameters(1), Me.RET_VMOL(Fase.Mixture), Me.RET_VKij, Me.RET_VTC, Me.RET_VPC, Me.RET_VTB, Me.RET_VW, KI, RET_VC, TQ(i - 1))
                            TQ.Add(tmp2(0))
                            PQ.Add(P)
                            T = TQ(i)
                            If Math.Abs(T - TCR) / TCR < 0.1 And Math.Abs(P - PCR) / PCR < 0.1 Then
                                P = P + dP * 0.1
                            Else
                                P = P + dP
                            End If
                            j = 0
                            Do
                                KI(j) = tmp2(j + 1)
                                j = j + 1
                            Loop Until j = n + 1
                        End If
                        beta = (Math.Log(PQ(i) / 101325) - Math.Log(PQ(i - 1) / 101325)) / (Math.Log(TQ(i)) - Math.Log(TQ(i - 1)))
                    End If
                    i = i + 1
                    If i > 2 Then
                        If PQ(i - 1) = PQ(i - 2) Or TQ(i - 1) = TQ(i - 2) Then Exit Do
                    End If
                Loop Until i >= 200 Or PQ(i - 1) = 0 Or PQ(i - 1) < 0 Or TQ(i - 1) < 0 Or _
                            Double.IsNaN(PQ(i - 1)) = True Or Double.IsNaN(TQ(i - 1)) = True Or _
                            Math.Abs(T - TCR) / TCR < 0.02 And Math.Abs(P - PCR) / PCR < 0.02
            Else
                TQ.Add(0)
                PQ.Add(0)
            End If

            If n > 0 And CBool(parameters(3)) = True Then
                Dim res As ArrayList = cpc.STABILITY_CURVE(Vm2, VTc2, VPc2, VVc2, Vw2, VKij2)
                i = 0
                Do
                    TE.Add(res(i)(0))
                    PE.Add(res(i)(1))
                    i += 1
                Loop Until i = res.Count
                'TE.Add(0)
                'PE.Add(0)
            Else
                TE.Add(0)
                PE.Add(0)
            End If

            If TVB.Count > 1 Then TVB.RemoveAt(TVB.Count - 1)
            If PB.Count > 1 Then PB.RemoveAt(PB.Count - 1)
            If HB.Count > 1 Then HB.RemoveAt(HB.Count - 1)
            If SB.Count > 1 Then SB.RemoveAt(SB.Count - 1)
            If VB.Count > 1 Then VB.RemoveAt(VB.Count - 1)
            If TVB.Count > 1 Then TVB.RemoveAt(TVB.Count - 1)
            If PB.Count > 1 Then PB.RemoveAt(PB.Count - 1)
            If HB.Count > 1 Then HB.RemoveAt(HB.Count - 1)
            If SB.Count > 1 Then SB.RemoveAt(SB.Count - 1)
            If VB.Count > 1 Then VB.RemoveAt(VB.Count - 1)

            If TVD.Count > 1 Then TVD.RemoveAt(TVD.Count - 1)
            If PO.Count > 1 Then PO.RemoveAt(PO.Count - 1)
            If HO.Count > 1 Then HO.RemoveAt(HO.Count - 1)
            If SO.Count > 1 Then SO.RemoveAt(SO.Count - 1)
            If VO.Count > 1 Then VO.RemoveAt(VO.Count - 1)
            If TVD.Count > 1 Then TVD.RemoveAt(TVD.Count - 1)
            If PO.Count > 1 Then PO.RemoveAt(PO.Count - 1)
            If HO.Count > 1 Then HO.RemoveAt(HO.Count - 1)
            If SO.Count > 1 Then SO.RemoveAt(SO.Count - 1)
            If VO.Count > 1 Then VO.RemoveAt(VO.Count - 1)

            If TQ.Count > 1 Then TQ.RemoveAt(TQ.Count - 1)
            If PQ.Count > 1 Then PQ.RemoveAt(PQ.Count - 1)
            If TQ.Count > 1 Then TQ.RemoveAt(TQ.Count - 1)
            If PQ.Count > 1 Then PQ.RemoveAt(PQ.Count - 1)

            Return New Object() {TVB, PB, HB, SB, VB, TVD, PO, HO, SO, VO, TE, PE, TH, PHsI, PHsII, CP, TQ, PQ}

        End Function

        Public Overrides Function DW_ReturnBinaryEnvelope(ByVal parameters As Object) As Object
            Dim n, i As Integer

            n = Me.CurrentMaterialStream.Fases(0).Componentes.Count - 1

            Dim dx As Double = 0.05

            Dim tipocalc As String
            Dim P, T As Double

            tipocalc = parameters(0)
            P = parameters(1)
            T = parameters(2)

            Select Case tipocalc

                Case "T-x-y"

                    Dim px, py1, py2 As New ArrayList

                    i = 0
                    Do
                        px.Add(i * dx)
                        py1.Add(Me.BUBT_PR_M2(P, New Object() {i * dx, 1 - i * dx}, RET_VKij, RET_VTC, RET_VPC, RET_VTB, RET_VW, New Object() {0, 0}, RET_VC)(0))
                        py2.Add(Me.DEWT_PR_M2(P, New Object() {i * dx, 1 - i * dx}, RET_VKij, RET_VTC, RET_VPC, RET_VTB, RET_VW, New Object() {0, 0}, RET_VC)(0))
                        i = i + 1
                    Loop Until (i - 1) * dx >= 1

                    Return New Object() {px, py1, py2}

                Case "P-x-y"

                    Dim px, py1, py2 As New ArrayList

                    i = 0
                    Do
                        px.Add(i * dx)
                        py1.Add(Me.BUBP_PR_M2(T, New Object() {i * dx, 1 - i * dx}, RET_VKij, RET_VTC, RET_VPC, RET_VTB, RET_VW, New Object() {0, 0}, RET_VC)(0))
                        py2.Add(Me.DEWP_PR_M2(T, New Object() {i * dx, 1 - i * dx}, RET_VKij, RET_VTC, RET_VPC, RET_VTB, RET_VW, New Object() {0, 0}, RET_VC)(0))
                        i = i + 1
                    Loop Until (i - 1) * dx >= 1

                    Return New Object() {px, py1, py2}

                Case "(T)y-x"

                    Dim px, py As New ArrayList

                    i = 0
                    Do
                        px.Add(i * dx)
                        py.Add(Me.BUBT_PR_M2(P, New Object() {i * dx, 1 - i * dx}, RET_VKij, RET_VTC, RET_VPC, RET_VTB, RET_VW, New Object() {0, 0}, RET_VC)(1) * i * dx)
                        i = i + 1
                    Loop Until (i - 1) * dx >= 1

                    Return New Object() {px, py}

                Case Else

                    Dim px, py As New ArrayList

                    i = 0
                    Do
                        px.Add(i * dx)
                        py.Add(Me.BUBP_PR_M2(T, New Object() {i * dx, 1 - i * dx}, RET_VKij, RET_VTC, RET_VPC, RET_VTB, RET_VW, New Object() {0, 0}, RET_VC)(1) * i * dx)
                        i = i + 1
                    Loop Until (i - 1) * dx >= 1

                    Return New Object() {px, py}

            End Select
        End Function

        Public Function RET_KIJ(ByVal id1 As String, ByVal id2 As String) As Double
            If Me.m_pr.InteractionParameters.ContainsKey(id1) Then
                If Me.m_pr.InteractionParameters(id1).ContainsKey(id2) Then
                    Return m_pr.InteractionParameters(id1)(id2).kij
                Else
                    If Me.m_pr.InteractionParameters.ContainsKey(id2) Then
                        If Me.m_pr.InteractionParameters(id2).ContainsKey(id1) Then
                            Return m_pr.InteractionParameters(id2)(id1).kij
                        Else
                            Return 0
                        End If
                    Else
                        Return 0
                    End If
                End If
            Else
                Return 0
            End If
        End Function

        Public Overrides Function RET_VKij() As Double(,)

            Dim val(Me.CurrentMaterialStream.Fases(0).Componentes.Count - 1, Me.CurrentMaterialStream.Fases(0).Componentes.Count - 1) As Double
            Dim i As Integer = 0
            Dim l As Integer = 0

            i = 0
            For Each cp As ClassesBasicasTermodinamica.Substancia In Me.CurrentMaterialStream.Fases(0).Componentes.Values
                l = 0
                For Each cp2 As ClassesBasicasTermodinamica.Substancia In Me.CurrentMaterialStream.Fases(0).Componentes.Values
                    val(i, l) = Me.RET_KIJ(cp.Nome, cp2.Nome)
                    l = l + 1
                Next
                i = i + 1
            Next

            Return val

        End Function

#End Region

#Region "    Equilibrium Server"

        Function FLASH_TP(ByVal T As Double, ByVal P As Double, ByVal Vz As Object, ByVal VKij As Object, ByVal VTc As Object, ByVal VPc As Object, ByVal VTb As Object, ByVal Vw As Object, ByVal VCT As Object) As Object

            Dim tol As Double = CDbl(Me.Parameters("PP_PTFELT"))
            Dim maxit As Integer = CInt(Me.Parameters("PP_PTFMEI"))

            'm_xn = New DLLXnumbers.Xnumbers

            'T,Tc em K
            'P,Pc em Pa

            Dim n As Integer = UBound(Vz)
            Dim i, cnt As Integer

            Dim nwm As Double = 0
            Dim wid As Integer = 0

            For i = 0 To n
                If CInt(VTc(i)) = 647 And CInt(VPc(i)) = 22055000 And Vz(i) <> 1 Then
                    wid = i
                    nwm = Vz(i)
                    Vz(i) = 0
                End If
            Next

            If nwm <> 0 Then
                For i = 0 To n
                    If i <> wid Then Vz(i) = Vz(i) / (1 - nwm)
                Next
            End If

            Dim Vx(n), Vy(n), Vx_ant(n), Vy_ant(n), KI(n), KI_ant(n), PHIV(n), PHIL(n), LN_CFL(n), LN_CFV(n) As Double
            Dim ai(n), bi(n), aml2(n), amv2(n) As Double
            Dim Vp(n), R, coeff(3), tmp(11, n + 1) As Double
            Dim Tc(n), Pc(n), Vc(n), W(n), Zc(n), alpha(n), m(n), a(n, n), b(n, n), ZL, ZV, Tr(n) As Double
            Dim fug_vap(n), fug_ratio(n), H(n) As Double
            Dim j, Vant, xiVpi As Double
            Dim t1, t2, t3, t4, t5 As Double
            Dim count As Integer = 1
            Dim soma_x As Double
            Dim soma_y As Double
            Dim mpl As Double = 1

            Dim Tref, Pref, L, V, err As Double

            Tref = 298.15
            Pref = 101325
            err = 0.000001

            i = 0
            xiVpi = 0
            Do
                Tc(i) = VTc(i)
                Tr(i) = T / Tc(i)
                Pc(i) = VPc(i)
                W(i) = Vw(i)
                Vp(i) = Me.AUX_PVAPi(i, T)
                xiVpi += Vz(i) * Vp(i)
                i = i + 1
            Loop Until i = n + 1

            '===============
            ' PASSO 0 - Verificação Inicial
            '===============

            If T > DWSIM.MathEx.Common.Max(Tc, Vz) Then
                Vy = Vz
                V = 1
                L = 0
                GoTo 100
            End If

            '===============
            ' PASSO 1
            '===============

            'Calcular Ki`s

            i = 0
            Do
                KI(i) = Vp(i) / P
                KI_ant(i) = 0
                i += 1
            Loop Until i = n + 1

            'Estimate V

            Dim Pb, Pd As Double

            Dim Pmin, Pmax, Px As Double
            i = 0
            Px = 0
            Do
                Px = Px + (Vz(i) / Vp(i))
                i = i + 1
            Loop Until i = n + 1
            Px = 1 / Px
            Pmin = Px
            i = 0
            Px = 0
            Do
                Px = Px + Vz(i) * Vp(i)
                i = i + 1
            Loop Until i = n + 1
            Pmax = Px
            Pb = Pmax
            Pd = Pmin

            'check for existence of new parameter
            If Not Me.Parameters.ContainsKey("PP_RIG_BUB_DEW_FLASH_INIT") Then
                Me.Parameters.Add("PP_RIG_BUB_DEW_FLASH_INIT", 0)
            End If

            If CInt(Me.Parameters("PP_RIG_BUB_DEW_FLASH_INIT")) = 1 Or Double.IsNaN(Pb) Or Double.IsNaN(Pd) Or Double.IsInfinity(Pb) Or Double.IsInfinity(Pd) Then
                Pb = Me.DW_CalcBubP(Vz, T)(0)
                Pd = Me.DW_CalcDewP(Vz, T)(0)
            End If

            If Abs(Pb - Pd) / Pb < 0.01 Then
                'one comp only
                If xiVpi <= P Then
                    L = 1
                    V = 0
                Else
                    L = 0
                    V = 1
                End If
            ElseIf P <= Pd Then
                'vapor only
                L = 0
                V = 1
            ElseIf P >= Pb Then
                'liquid only
                L = 1
                V = 0
            Else
                'VLE
                V = 1 - (P - Pd) / (Pb - Pd)
                L = 1 - V
            End If


            If n = 0 Then
                If Vp(0) <= P Then
                    L = 1
                    V = 0
                Else
                    L = 0
                    V = 1
                End If
            End If

            i = 0
            Do
                Vy(i) = Vz(i) * KI(i) / ((KI(i) - 1) * V + 1)
                Vx(i) = Vy(i) / KI(i)
                i += 1
            Loop Until i = n + 1
            i = 0
            soma_x = 0
            soma_y = 0
            Do
                soma_x = soma_x + Vx(i)
                soma_y = soma_y + Vy(i)
                i = i + 1
            Loop Until i = n + 1
            i = 0
            Do
                Vx(i) = Vx(i) / soma_x
                Vy(i) = Vy(i) / soma_y
                i = i + 1
            Loop Until i = n + 1

            R = 8.314

            i = 0
            Do
                Tc(i) = VTc(i)
                Tr(i) = T / Tc(i)
                Pc(i) = VPc(i)
                W(i) = Vw(i)
                i = i + 1
            Loop Until i = n + 1

            Dim ai_(n)

            i = 0
            Do
                If Vz(i) <> 0 Then
                    alpha(i) = (1 + (0.37464 + 1.54226 * W(i) - 0.26992 * W(i) ^ 2) * (1 - (T / Tc(i)) ^ 0.5)) ^ 2
                    ai(i) = 0.45724 * alpha(i) * R ^ 2 * Tc(i) ^ 2 / Pc(i)
                    ai_(i) = ai(i) ^ 0.5
                    bi(i) = 0.0778 * R * Tc(i) / Pc(i)
                End If
                i = i + 1
            Loop Until i = n + 1

            i = 0
            Do
                j = 0
                Do
                    a(i, j) = (ai(i) * ai(j)) ^ 0.5 * (1 - VKij(i, j))
                    j = j + 1
                Loop Until j = n + 1
                i = i + 1
            Loop Until i = n + 1

            i = 0
            Do
                aml2(i) = 0
                i = i + 1
            Loop Until i = n + 1

            cnt = 1
            Dim convergiu = 0

            Do

                Dim ssubc = 0
                Dim ssupc = 0
                i = 0
                Do
                    If Tr(i) < 1 Then
                        ssubc = ssubc + Vx(i)
                    Else
                        ssupc = ssupc + Vx(i)
                    End If
                    i = i + 1
                Loop Until i = n + 1

                i = 0
                Do
                    aml2(i) = 0
                    i = i + 1
                Loop Until i = n + 1

                i = 0
                Dim aml = 0
                Do
                    j = 0
                    Do
                        aml = aml + Vx(i) * Vx(j) * a(i, j)
                        aml2(i) = aml2(i) + Vx(j) * a(j, i)
                        j = j + 1
                    Loop Until j = n + 1
                    i = i + 1
                Loop Until i = n + 1

                i = 0
                Dim bml = 0
                Do
                    bml = bml + Vx(i) * bi(i)
                    i = i + 1
                Loop Until i = n + 1

                Dim AG = aml * P / (R * T) ^ 2
                Dim BG = bml * P / (R * T)

                coeff(0) = -AG * BG + BG ^ 2 + BG ^ 3
                coeff(1) = AG - 3 * BG ^ 2 - 2 * BG
                coeff(2) = BG - 1
                coeff(3) = 1

                Dim temp1 = Poly_Roots(coeff)
                Dim tv
                Dim tv2
                Try

                    If temp1(0, 0) > temp1(1, 0) Then
                        tv = temp1(1, 0)
                        tv2 = temp1(1, 1)
                        temp1(1, 0) = temp1(0, 0)
                        temp1(0, 0) = tv
                        temp1(1, 1) = temp1(0, 1)
                        temp1(0, 1) = tv2
                    End If
                    If temp1(0, 0) > temp1(2, 0) Then
                        tv = temp1(2, 0)
                        temp1(2, 0) = temp1(0, 0)
                        temp1(0, 0) = tv
                        tv2 = temp1(2, 1)
                        temp1(2, 1) = temp1(0, 1)
                        temp1(0, 1) = tv2
                    End If
                    If temp1(1, 0) > temp1(2, 0) Then
                        tv = temp1(2, 0)
                        temp1(2, 0) = temp1(1, 0)
                        temp1(1, 0) = tv
                        tv2 = temp1(2, 1)
                        temp1(2, 1) = temp1(1, 1)
                        temp1(1, 1) = tv2
                    End If

                    ZL = temp1(0, 0)
                    If temp1(0, 1) <> 0 Then
                        ZL = temp1(1, 0)
                        If temp1(1, 1) <> 0 Then
                            ZL = temp1(2, 0)
                        End If
                    End If

                Catch

                    Dim findZL
                    ZL = 0
                    Do
                        findZL = coeff(3) * ZL ^ 3 + coeff(2) * ZL ^ 2 + coeff(1) * ZL + coeff(0)
                        ZL += 0.00001
                        If ZL > 1 Then Throw New Exception(DWSIM.App.GetLocalString("PropPack_ZError"))
                    Loop Until Math.Abs(findZL) < 0.0001

                End Try

                Dim sum_ci As Double = 0
                For i = 0 To n
                    sum_ci += VCT(i)
                Next

                i = 0
                Do
                    t1 = bi(i) * (ZL - 1) / bml
                    t2 = -Math.Log(ZL - BG)
                    t3 = AG * (2 * aml2(i) / aml - bi(i) / bml)
                    t4 = Math.Log((ZL + (1 + 2 ^ 0.5) * BG) / (ZL + (1 - 2 ^ 0.5) * BG))
                    t5 = 2 * 2 ^ 0.5 * BG
                    LN_CFL(i) = t1 + t2 - (t3 * t4 / t5)
                    PHIL(i) = Math.Exp(LN_CFL(i)) * Vx(i) * P
                    i = i + 1
                Loop Until i = n + 1

                ' CALCULO DAS RAIZES PARA A FASE VAPOR

                i = 0
                Do
                    amv2(i) = 0
                    i = i + 1
                Loop Until i = n + 1

                i = 0
                Dim amv = 0
                Do
                    j = 0
                    Do
                        amv = amv + Vy(i) * Vy(j) * a(i, j)
                        amv2(i) = amv2(i) + Vy(j) * a(j, i)
                        j = j + 1
                    Loop Until j = n + 1
                    i = i + 1
                Loop Until i = n + 1

                i = 0
                Dim bmv = 0
                Do
                    bmv = bmv + Vy(i) * bi(i)
                    i = i + 1
                Loop Until i = n + 1

                AG = amv * P / (R * T) ^ 2
                BG = bmv * P / (R * T)

                coeff(0) = -AG * BG + BG ^ 2 + BG ^ 3
                coeff(1) = AG - 3 * BG ^ 2 - 2 * BG
                coeff(2) = BG - 1
                coeff(3) = 1

                temp1 = Poly_Roots(coeff)
                tv = 0

                Try

                    If temp1(0, 0) > temp1(1, 0) Then
                        tv = temp1(1, 0)
                        temp1(1, 0) = temp1(0, 0)
                        temp1(0, 0) = tv
                        tv2 = temp1(1, 1)
                        temp1(1, 1) = temp1(0, 1)
                        temp1(0, 1) = tv2
                    End If
                    If temp1(0, 0) > temp1(2, 0) Then
                        tv = temp1(2, 0)
                        temp1(2, 0) = temp1(0, 0)
                        temp1(0, 0) = tv
                        tv2 = temp1(2, 1)
                        temp1(2, 1) = temp1(0, 1)
                        temp1(0, 1) = tv2
                    End If
                    If temp1(1, 0) > temp1(2, 0) Then
                        tv = temp1(2, 0)
                        temp1(2, 0) = temp1(1, 0)
                        temp1(1, 0) = tv
                        tv2 = temp1(2, 1)
                        temp1(2, 1) = temp1(1, 1)
                        temp1(1, 1) = tv2
                    End If

                    ZV = temp1(2, 0)
                    If temp1(2, 1) <> 0 Then
                        ZV = temp1(1, 0)
                        If temp1(1, 1) <> 0 Then
                            ZV = temp1(0, 0)
                        End If
                    End If

                Catch

                    Dim findZV
                    ZV = 1
                    Do
                        findZV = coeff(3) * ZV ^ 3 + coeff(2) * ZV ^ 2 + coeff(1) * ZV + coeff(0)
                        ZV -= 0.00001
                        If ZV < 0 Then Throw New Exception(DWSIM.App.GetLocalString("PropPack_ZError"))
                    Loop Until Math.Abs(findZV) < 0.0001

                End Try

                i = 0
                Do
                    t1 = bi(i) * (ZV - 1) / bmv
                    t2 = -Math.Log(ZV - BG)
                    t3 = AG * (2 * amv2(i) / amv - bi(i) / bmv)
                    t4 = Math.Log(ZV + (1 + 2 ^ 0.5) * BG) - Math.Log(ZV + ((1 - 2 ^ 0.5) * BG))
                    t5 = 8 ^ 0.5 * BG
                    LN_CFV(i) = t1 + t2 - (t3 * t4 / t5)
                    PHIV(i) = Math.Exp(LN_CFV(i)) * Vy(i) * P
                    i = i + 1
                Loop Until i = n + 1

                i = 0
                Do
                    If Vz(i) <> 0 Then
                        KI_ant(i) = KI(i)
                        KI(i) = (Math.Exp(LN_CFL(i)) / (Math.Exp(LN_CFV(i))))
                    End If
                    i = i + 1
                Loop Until i = n + 1

                'Cálculo HL e HV

                '===============
                ' PASSO 10/11
                '===============

                i = 0
                Do
                    If Vz(i) <> 0 Then
                        Vy_ant(i) = Vy(i)
                        Vx_ant(i) = Vx(i)
                        Vy(i) = Vz(i) * KI(i) / ((KI(i) - 1) * V + 1)
                        Vx(i) = Vy(i) / KI(i)
                    Else
                        Vy(i) = 0
                        Vx(i) = 0
                    End If
                    i += 1
                Loop Until i = n + 1
                i = 0
                soma_x = 0
                soma_y = 0
                Do
                    soma_x = soma_x + Vx(i)
                    soma_y = soma_y + Vy(i)
                    i = i + 1
                Loop Until i = n + 1
                i = 0
                Do
                    Vx(i) = Vx(i) / soma_x
                    Vy(i) = Vy(i) / soma_y
                    i = i + 1
                Loop Until i = n + 1

                Dim e1 = 0
                Dim e2 = 0
                Dim e3 = 0
                i = 0
                Do
                    e1 = e1 + (Vx(i) - Vx_ant(i))
                    e2 = e2 + (Vy(i) - Vy_ant(i))
                    i = i + 1
                Loop Until i = n + 1

                e3 = (V - Vant)

                If Double.IsNaN(Math.Abs(e1) + Math.Abs(e2)) Then

                    Throw New Exception(DWSIM.App.GetLocalString("PropPack_FlashError"))

                ElseIf (Math.Abs(e1) + Math.Abs(e2) + Math.Abs(e3)) < tol Then

                    convergiu = 1

                    Exit Do

                Else

                    Vant = V

                    Dim F = 0
                    Dim dF = 0
                    i = 0
                    Do
                        If Vz(i) > 0 Then
                            F = F + Vz(i) * (KI(i) - 1) / (1 + V * (KI(i) - 1))
                            dF = dF - Vz(i) * (KI(i) - 1) ^ 2 / (1 + V * (KI(i) - 1)) ^ 2
                        End If
                        i = i + 1
                    Loop Until i = n + 1

                    If Abs(F) < 0.000001 Then Exit Do

                    If Math.Abs(F / dF) > 1 Then
                        V = -F / dF * 0.5 * V + V
                    Else
                        V = -F / dF * mpl + V
                    End If

                    If V > 1 Then
                        V = 1
                        L = 0
                        i = 0
                        Do
                            Vy(i) = Vz(i)
                            Vx(i) = 0
                            i = i + 1
                        Loop Until i = n + 1
                        Exit Do
                    ElseIf V < 0 Then
                        V = 0
                        L = 1
                        i = 0
                        Do
                            Vx(i) = Vz(i)
                            Vy(i) = 0
                            i = i + 1
                        Loop Until i = n + 1
                        Exit Do
                    End If

                End If

                L = 1 - V

                cnt += 1

                If Double.IsNaN(V) Then Throw New Exception(DWSIM.App.GetLocalString("PropPack_FlashTPVapFracError"))
                If cnt > maxit Then Throw New Exception(DWSIM.App.GetLocalString("PropPack_FlashMaxIt2"))

                CheckCalculatorStatus()

            Loop Until convergiu = 1

            If Me.AUX_CheckTrivial(KI) And V <> 0 And V <> 1 Then
                'trivial solution...
                Throw New Exception(DWSIM.App.GetLocalString("PropPack_FlashTrivialSolution"))
            End If

100:        If nwm <> 0 Then

                Vz(wid) = nwm

                Dim nm, nwv, nwl, nml, nmv As Double

                nml = L * (1 - nwm)
                nmv = V * (1 - nwm)
                nm = nml + nmv

                If V > 0 Then

                    Vy(wid) = Vp(wid) / P
                    nwv = nmv * Vy(wid) / (1 - Vy(wid))
                    If nwv > nwm Then
                        nwl = 0
                        Vy(wid) = nwm / (nwm + nmv)
                        Vx(wid) = 0
                    Else
                        nwl = nwm - nwv
                        Vx(wid) = nwl / (nml + nwl)
                    End If

                    L = nml + nwl
                    V = 1 - L

                    For i = 0 To n
                        If i <> wid Then
                            Vx(i) = Vx(i) * (1 - Vx(wid))
                            Vy(i) = Vy(i) * (1 - Vy(wid))
                            Vz(i) = Vz(i) * (1 - nwm)
                        End If
                    Next

                ElseIf V = 0 And L <> 0 Then

                    nwl = nwm
                    Vx(wid) = nwl / (nwl + nml)

                    For i = 0 To n
                        If i <> wid Then
                            Vx(i) = Vx(i) * (1 - Vx(wid))
                            Vz(i) = Vz(i) * (1 - nwm)
                        End If
                    Next

                ElseIf V <> 0 And L = 0 Then

                    nwv = nwm
                    Vy(wid) = nwv / (nwv + nmv)

                    For i = 0 To n
                        If i <> wid Then
                            Vy(i) = Vy(i) * (1 - Vy(wid))
                            Vz(i) = Vz(i) * (1 - nwm)
                        End If
                    Next

                End If

            End If

            tmp(0, 0) = L
            tmp(1, 0) = V

            Dim tmp2 As Object = Nothing
            If V = 1 Or L = 1 Then
                If V = 1 Then tmp2 = Me.m_pr.CalcLnFug(T, P, Vy, VKij, VTc, VPc, Vw, VTb, "V")
                If L = 1 Then tmp2 = Me.m_pr.CalcLnFug(T, P, Vx, VKij, VTc, VPc, Vw, VTb, "L")
            End If

            i = 1
            Do
                tmp(0, i) = Vx(i - 1)
                tmp(1, i) = Vy(i - 1)
                If V = 1 Then
                    tmp(5, i) = Exp(tmp2(i - 1))
                Else
                    tmp(5, i) = Exp(LN_CFV(i - 1))
                End If
                If L = 1 Then
                    tmp(4, i) = Exp(tmp2(i - 1))
                    tmp(6, i) = Exp(tmp2(i - 1)) * P / Vp(i - 1)
                Else
                    tmp(4, i) = Exp(LN_CFV(i - 1))
                    tmp(6, i) = Exp(LN_CFL(i - 1)) * P / Vp(i - 1)
                End If
                tmp(7, i) = 0
                tmp(8, i) = 0
                tmp(9, i) = 0
                tmp(10, i) = 0
                tmp(11, i) = Vy(i - 1) * P
                i = i + 1
            Loop Until i = n + 2
            tmp(2, 0) = ZL
            tmp(2, 1) = ZV
            tmp(3, 0) = 0

            'm_xn = Nothing

            FLASH_TP = tmp

        End Function

#End Region

#Region "    Loops para correção de H/S"

        Function ESTIMAR_T_S2(ByVal ST As Double, ByVal Tref As Double, ByVal TIPO As String, ByVal P As Double, ByVal Vz As Array, ByVal VKij As Object, ByVal VTc As Array, ByVal VPc As Array, ByVal Vw As Array, ByVal VMM As Array) As Double

            Dim n = UBound(Vz)

            Dim i As Integer

            Dim Tinf, Tsup As Double

            Dim fT, fT_inf, nsub, delta_T As Double

            Tinf = Tref * 0.2
            Tsup = Tref * 5

            nsub = 3

            delta_T = (Tsup - Tinf) / nsub

            i = 0
            Do
                i = i + 1
                fT = ST - Me.m_pr.S_PR_MIX(TIPO, Tinf, P, Vz, VKij, VTc, VPc, Vw, VMM, 0) * Me.AUX_MMM(Vz) - Me.RET_Sid(298.15, Tinf, P, Vz) * Me.AUX_MMM(Vz)
                Tinf = Tinf + delta_T
                fT_inf = ST - Me.m_pr.S_PR_MIX(TIPO, Tinf, P, Vz, VKij, VTc, VPc, Vw, VMM, 0) * Me.AUX_MMM(Vz) - Me.RET_Sid(298.15, Tinf, P, Vz) * Me.AUX_MMM(Vz)
            Loop Until fT * fT_inf < 0 Or fT_inf > fT Or i >= 4
            Tsup = Tinf
            Tinf = Tinf - delta_T

            'método de Brent para encontrar Vc

            Dim aaa, bbb, ccc, ddd, eee, min11, min22, faa, fbb, fcc, ppp, qqq, rrr, sss, tol11, xmm As Double
            Dim ITMAX2 As Integer = 100
            Dim iter2 As Integer

            aaa = Tinf
            bbb = Tsup
            ccc = Tsup

            faa = ST - Me.m_pr.S_PR_MIX(TIPO, aaa, P, Vz, VKij, VTc, VPc, Vw, VMM, 0) * Me.AUX_MMM(Vz) - Me.RET_Sid(298.15, aaa, P, Vz) * Me.AUX_MMM(Vz)
            fbb = ST - Me.m_pr.S_PR_MIX(TIPO, bbb, P, Vz, VKij, VTc, VPc, Vw, VMM, 0) * Me.AUX_MMM(Vz) - Me.RET_Sid(298.15, bbb, P, Vz) * Me.AUX_MMM(Vz)
            fcc = fbb

            iter2 = 0
            Do
                If (fbb > 0 And fcc > 0) Or (fbb < 0 And fcc < 0) Then
                    ccc = aaa
                    fcc = faa
                    ddd = bbb - aaa
                    eee = ddd
                End If
                If Math.Abs(fcc) < Math.Abs(fbb) Then
                    aaa = bbb
                    bbb = ccc
                    ccc = aaa
                    faa = fbb
                    fbb = fcc
                    fcc = faa
                End If
                tol11 = 0.0001
                xmm = 0.5 * (ccc - bbb)
                If (Math.Abs(xmm) <= tol11) Or (fbb = 0) Then GoTo Final3
                If (Math.Abs(eee) >= tol11) And (Math.Abs(faa) > Math.Abs(fbb)) Then
                    sss = fbb / faa
                    If aaa = ccc Then
                        ppp = 2 * xmm * sss
                        qqq = 1 - sss
                    Else
                        qqq = faa / fcc
                        rrr = fbb / fcc
                        ppp = sss * (2 * xmm * qqq * (qqq - rrr) - (bbb - aaa) * (rrr - 1))
                        qqq = (qqq - 1) * (rrr - 1) * (sss - 1)
                    End If
                    If ppp > 0 Then qqq = -qqq
                    ppp = Math.Abs(ppp)
                    min11 = 3 * xmm * qqq - Math.Abs(tol11 * qqq)
                    min22 = Math.Abs(eee * qqq)
                    Dim tvar2 As Double
                    If min11 < min22 Then tvar2 = min11
                    If min11 > min22 Then tvar2 = min22
                    If 2 * ppp < tvar2 Then
                        eee = ddd
                        ddd = ppp / qqq
                    Else
                        ddd = xmm
                        eee = ddd
                    End If
                Else
                    ddd = xmm
                    eee = ddd
                End If
                aaa = bbb
                faa = fbb
                If (Math.Abs(ddd) > tol11) Then
                    bbb += ddd
                Else
                    bbb += Math.Sign(xmm) * tol11
                End If
                fbb = ST - Me.m_pr.S_PR_MIX(TIPO, bbb, P, Vz, VKij, VTc, VPc, Vw, VMM, 0) * Me.AUX_MMM(Vz) - Me.RET_Sid(298.15, bbb, P, Vz) * Me.AUX_MMM(Vz)
                iter2 += 1
            Loop Until iter2 = ITMAX2

Final3:

            Return bbb

        End Function

        Function ESTIMAR_T_H2(ByVal HT As Double, ByVal Tref As Double, ByVal TIPO As String, ByVal P As Double, ByVal Vz As Array, ByVal VKij As Object, ByVal VTc As Array, ByVal VPc As Array, ByVal Vw As Array, ByVal VMM As Array) As Double

            Dim n = UBound(Vz)

            Dim i As Integer
            Dim t1, t2, t3 As Double

            Dim Tinf, Tsup As Double

            Dim fT, fT_inf, nsub, delta_T As Double

            Tinf = Tref * 0.2
            Tsup = Tref * 5

            nsub = 3

            delta_T = (Tsup - Tinf) / nsub

            i = 0
            Do
                i = i + 1
                t1 = HT
                t2 = Me.m_pr.H_PR_MIX(TIPO, Tinf, P, Vz, VKij, VTc, VPc, Vw, VMM, 0)
                t3 = Me.RET_Hid(298.15, Tinf, Vz)
                fT = t1 - t2 - t3
                Tinf = Tinf + delta_T
                t1 = HT
                t2 = Me.m_pr.H_PR_MIX(TIPO, Tinf, P, Vz, VKij, VTc, VPc, Vw, VMM, 0)
                t3 = Me.RET_Hid(298.15, Tinf, Vz)
                fT_inf = t1 - t2 - t3
            Loop Until fT * fT_inf < 0 Or i >= 3
            Tsup = Tinf
            Tinf = Tinf - delta_T

            'método de Brent para encontrar Vc

            Dim aaa, bbb, ccc, ddd, eee, min11, min22, faa, fbb, fcc, ppp, qqq, rrr, sss, tol11, xmm As Double
            Dim ITMAX2 As Integer = 100
            Dim iter2 As Integer

            aaa = Tinf
            bbb = Tsup
            ccc = Tsup

            faa = HT - Me.m_pr.H_PR_MIX(TIPO, aaa, P, Vz, VKij, VTc, VPc, Vw, VMM, 0) - Me.RET_Hid(298.15, aaa, Vz)
            fbb = HT - Me.m_pr.H_PR_MIX(TIPO, bbb, P, Vz, VKij, VTc, VPc, Vw, VMM, 0) - Me.RET_Hid(298.15, bbb, Vz)
            fcc = fbb

            iter2 = 0
            Do
                If (fbb > 0 And fcc > 0) Or (fbb < 0 And fcc < 0) Then
                    ccc = aaa
                    fcc = faa
                    ddd = bbb - aaa
                    eee = ddd
                End If
                If Math.Abs(fcc) < Math.Abs(fbb) Then
                    aaa = bbb
                    bbb = ccc
                    ccc = aaa
                    faa = fbb
                    fbb = fcc
                    fcc = faa
                End If
                tol11 = 0.0000001
                xmm = 0.5 * (ccc - bbb)
                If (Math.Abs(xmm) <= tol11) Or (fbb = 0) Then GoTo Final3
                'If Math.Abs(fbb) < 0.1 Then GoTo Final3
                If (Math.Abs(eee) >= tol11) And (Math.Abs(faa) > Math.Abs(fbb)) Then
                    sss = fbb / faa
                    If aaa = ccc Then
                        ppp = 2 * xmm * sss
                        qqq = 1 - sss
                    Else
                        qqq = faa / fcc
                        rrr = fbb / fcc
                        ppp = sss * (2 * xmm * qqq * (qqq - rrr) - (bbb - aaa) * (rrr - 1))
                        qqq = (qqq - 1) * (rrr - 1) * (sss - 1)
                    End If
                    If ppp > 0 Then qqq = -qqq
                    ppp = Math.Abs(ppp)
                    min11 = 3 * xmm * qqq - Math.Abs(tol11 * qqq)
                    min22 = Math.Abs(eee * qqq)
                    Dim tvar2 As Double
                    If min11 < min22 Then tvar2 = min11
                    If min11 > min22 Then tvar2 = min22
                    If 2 * ppp < tvar2 Then
                        eee = ddd
                        ddd = ppp / qqq
                    Else
                        ddd = xmm
                        eee = ddd
                    End If
                Else
                    ddd = xmm
                    eee = ddd
                End If
                aaa = bbb
                faa = fbb
                If (Math.Abs(ddd) > tol11) Then
                    bbb += ddd
                Else
                    bbb += Math.Sign(xmm) * tol11
                End If
                fbb = HT - Me.m_pr.H_PR_MIX(TIPO, bbb, P, Vz, VKij, VTc, VPc, Vw, VMM, 0) - Me.RET_Hid(298.15, bbb, Vz)
                iter2 += 1
            Loop Until iter2 = ITMAX2

Final3:

            Return bbb

        End Function

        Function ESTIMAR_T_H(ByVal HT As Double, ByVal Tref As Double, ByVal TIPO As String, ByVal P As Double, ByVal Vz As Array, ByVal VKij As Object, ByVal VTc As Array, ByVal VPc As Array, ByVal Vw As Array, ByVal VMM As Array) As Double

            Dim maxit As Integer = CInt(Me.Parameters("PP_PHFMII"))
            Dim tol As Double = CDbl(Me.Parameters("PP_PHFILT"))

            Dim n = UBound(Vz)

            Dim cnt As Integer = 0
            Dim T, Tant, Tant2, fi, fi_ant, fi_ant2 As Double

            T = Tref
            Do
                fi_ant2 = fi_ant
                fi_ant = fi
                fi = HT - Me.m_pr.H_PR_MIX(TIPO, T, P, Vz, VKij, VTc, VPc, Vw, VMM, 0) - Me.RET_Hid(298.15, T, Vz)
                If cnt <= 1 Then
                    Tant2 = Tant
                    Tant = T
                    T = T * 1.1
                Else
                    Tant2 = Tant
                    Tant = T
                    T = T - fi / -Me.m_props.CpCvR(TIPO, T, P, Vz, VKij, Me.AUX_CONVERT_MOL_TO_MASS(Vz), VTc, VPc, Me.RET_VCP(T), VMM, Vw, Me.RET_VZRa)(1)
                End If
                If T < 0 Then
                    T = Tant * 1.01
                End If
                cnt += 1
                If cnt >= maxit Then Throw New Exception("MÁX. ITERAÇÕES ATINGIDO: Flash PH - Loop Temperatura (Monofásico)")
            Loop Until Math.Abs(fi / HT) < tol Or Double.IsNaN(T)

            Return T

        End Function

        Function ESTIMAR_T_S(ByVal ST As Double, ByVal Tref As Double, ByVal TIPO As String, ByVal P As Double, ByVal Vz As Array, ByVal VKij As Object, ByVal VTc As Array, ByVal VPc As Array, ByVal Vw As Array, ByVal VMM As Array) As Double

            Dim maxit As Integer = CInt(Me.Parameters("PP_PSFMII"))
            Dim tol As Double = CDbl(Me.Parameters("PP_PSFILT"))

            Dim n = UBound(Vz)

            Dim cnt As Integer = 0
            Dim T, Tant, Tant2, fi, fi_ant, fi_ant2 As Double

            T = Tref
            Do
                fi_ant2 = fi_ant
                fi_ant = fi
                fi = ST - Me.m_pr.S_PR_MIX(TIPO, T, P, Vz, VKij, VTc, VPc, Vw, VMM, 0) - Me.RET_Sid(298.15, T, P, Vz)
                If cnt <= 1 Then
                    Tant2 = Tant
                    Tant = T
                    T = T * 1.001
                Else
                    Tant2 = Tant
                    Tant = T
                    T = T - fi * (T - Tant2) / (fi - fi_ant2)
                End If
                If T < 0 Then
                    T = Tant * 1.1

                End If
                cnt += 1
                If cnt >= maxit Then Throw New Exception("MÁX. ITERAÇÕES ATINGIDO: Flash PS - Loop Temperatura (Monofásico)")
            Loop Until Math.Abs(fi / ST) < tol Or Double.IsNaN(T) Or Math.Abs(fi - fi_ant2) < tol

            Return T

        End Function


#End Region

#Region "    Extras"

        Function Calc_Kb(ByVal T, ByVal P, ByVal Vz, ByVal VTc, ByVal VPc, ByVal Vw, ByVal Vp, ByVal V)

            'm_xn = New DLLXnumbers.Xnumbers

            Dim i, n As Integer
            n = UBound(Vz)
            Dim soma_x, soma_y, Vx(n), Vy(n), Vt(n), Vwbb(n), KI(n), Kb As Double

            'Calcular Ki`s

            i = 0
            Do
                If Vz(i) <> 0 Then
                    KI(i) = VPc(i) / P * Math.Exp(5.373 * (1 + Vw(i)) * (1 - VTc(i) / T))
                ElseIf Vz(i) = 0 Then
                    KI(i) = 0
                End If
                i += 1
            Loop Until i = n + 1

            i = 0
            Do
                If Vz(i) <> 0 Then
                    Vy(i) = Vz(i) * KI(i) / ((KI(i) - 1) * V + 1)
                    Vx(i) = Vy(i) / KI(i)
                Else
                    Vy(i) = 0
                    Vx(i) = 0
                End If
                i += 1
            Loop Until i = n + 1
            i = 0
            soma_x = 0
            soma_y = 0
            Do
                soma_x = soma_x + Vx(i)
                soma_y = soma_y + Vy(i)
                i = i + 1
            Loop Until i = n + 1
            i = 0
            Do
                Vx(i) = Vx(i) / soma_x
                Vy(i) = Vy(i) / soma_y
                i = i + 1
            Loop Until i = n + 1

            'Calculo dos wi`s, ui`s

            i = 0
            Dim sum_vti = 0
            Do
                If Vz(i) <> 0 Then
                    Vt(i) = Vy(i) * 5.373 * (1 - Vw(i)) * (VTc(i) / T ^ 2) / (1 - V + V * KI(i))
                Else
                    Vt(i) = 0
                End If
                sum_vti += Vt(i)
                i = i + 1
            Loop Until i = n + 1
            i = 0
            Do
                Vwbb(i) = Vt(i) / sum_vti
                i = i + 1
            Loop Until i = n + 1
            i = 0
            Kb = 0
            Do
                If KI(i) <> 0 Then Kb += Vwbb(i) * Math.Log(KI(i))
                i = i + 1
            Loop Until i = n + 1

            Return Exp(Kb)

        End Function

        Function Calc_Kb_TP(ByVal T, ByVal P, ByVal Vz, ByVal VTc, ByVal VPc, ByVal Vw, ByVal Vp, ByVal V)

            'm_xn = New DLLXnumbers.Xnumbers

            Dim i, n As Integer
            n = UBound(Vz)
            Dim soma_x, soma_y, Vx(n), Vy(n), Vt(n), Vwbb(n), KI(n), Kb As Double

            'Calcular Ki`s

            i = 0
            Do
                If Vz(i) <> 0 Then
                    KI(i) = VPc(i) / P * Math.Exp(5.373 * (1 + Vw(i)) * (1 - VTc(i) / T))
                ElseIf Vz(i) = 0 Then
                    KI(i) = 0
                End If
                i += 1
            Loop Until i = n + 1

            'Estimar V

            'Estimar V

            i = 0
            Do
                If Vz(i) <> 0 Then
                    Vy(i) = Vz(i) * KI(i) / ((KI(i) - 1) * V + 1)
                    Vx(i) = Vy(i) / KI(i)
                Else
                    Vy(i) = 0
                    Vx(i) = 0
                End If
                i += 1
            Loop Until i = n + 1
            i = 0
            soma_x = 0
            soma_y = 0
            Do
                soma_x = soma_x + Vx(i)
                soma_y = soma_y + Vy(i)
                i = i + 1
            Loop Until i = n + 1
            i = 0
            Do
                Vx(i) = Vx(i) / soma_x
                Vy(i) = Vy(i) / soma_y
                i = i + 1
            Loop Until i = n + 1

            'Calculo dos wi`s, ui`s

            i = 0
            Dim sum_vti = 0
            Do
                If Vz(i) <> 0 Then
                    Vt(i) = Vy(i) * 5.373 * (1 + Vw(i)) * (VTc(i) / T ^ 2) / (1 - V + V * KI(i))
                Else
                    Vt(i) = 0
                End If
                sum_vti += Vt(i)
                i = i + 1
            Loop Until i = n + 1
            i = 0
            Do
                Vwbb(i) = Vt(i) / sum_vti
                i = i + 1
            Loop Until i = n + 1
            i = 0
            Kb = 0
            Do
                If KI(i) <> 0 Then Kb += Vwbb(i) * Math.Log(KI(i))
                i = i + 1
            Loop Until i = n + 1

            Return Exp(Kb)

        End Function

        Function OBJ_Loop_R(ByVal Rbb As Double, ByVal Vu As Object, ByVal Vz As Object, ByVal Kb0 As Double, ByVal HT As Double, _
                            ByVal Tref As Double, ByVal AA As Double, ByVal BB As Double, ByVal CC As Double, ByVal DD As Double, _
                            ByVal EE As Double, ByVal FF As Double)

            Dim i As Integer
            Dim n As Integer = UBound(Vz)

            Dim Vt(n), Vpbb(n), Vwbb(n), DHv, DHl, Kb, _
                 L, V, fi, T, Vu_ant(n) As Double

            Dim sum_vti As Double = 0
            Dim sum_pi, sum_eui_pi As Double
            Dim sum_Hvi0 As Double

            i = 0
            sum_pi = 0
            sum_eui_pi = 0
            Do
                Vpbb(i) = Vz(i) / (1 - Rbb + Kb0 * Rbb * Exp(Vu(i)))
                sum_pi += Vpbb(i)
                sum_eui_pi += Exp(Vu(i)) * Vpbb(i)
                i = i + 1
            Loop Until i = n + 1
            Kb = sum_pi / sum_eui_pi
            T = 1 / Tref + (Log(Kb) - AA) / BB
            T = 1 / T

            L = (1 - Rbb) * sum_pi
            V = 1 - L

            '===============
            ' PASSO 4
            '===============

            DHv = CC + DD * (T - Tref)
            DHl = EE + FF * (T - Tref)

            '===============
            ' PASSO 5
            '===============

            sum_Hvi0 = Me.RET_Hid(Tref, T, Vz) * Me.AUX_MMM(Vz)

            '===============
            ' PASSO 6
            '===============

            fi = L * (DHv - DHl) - sum_Hvi0 - DHv + HT * Me.AUX_MMM(Vz)

            '===============
            ' PASSO 7
            '===============

            Return fi

        End Function

        Function ESTIMAR_R(ByVal Vu As Object, ByVal Vz As Object, ByVal Kb0 As Double, ByVal HT As Double, _
                            ByVal Tref As Double, ByVal AA As Double, ByVal BB As Double, ByVal CC As Double, ByVal DD As Double, _
                            ByVal EE As Double, ByVal FF As Double)

            Dim n = UBound(Vz)

            Dim i As Integer

            Dim Vinf, Vsup As Double

            Dim fV, fV_inf, nsub, delta_V As Double

            Vinf = 0
            Vsup = 1

            nsub = 20

            delta_V = (Vsup - Vinf) / nsub

            i = 0
            Do
                i = i + 1
                fV = OBJ_Loop_R(Vinf, Vu, Vz, Kb0, HT, Tref, AA, BB, CC, DD, EE, FF)
                Vinf = Vinf + delta_V
                fV_inf = OBJ_Loop_R(Vinf, Vu, Vz, Kb0, HT, Tref, AA, BB, CC, DD, EE, FF)
            Loop Until fV * fV_inf < 0 Or Vinf >= Vsup
            Vsup = Vinf
            Vinf = Vinf - delta_V

            'método de Brent para encontrar Vc

            Dim aaa, bbb, ccc, ddd, eee, min11, min22, faa, fbb, fcc, ppp, qqq, rrr, sss, tol11, xmm As Double
            Dim ITMAX2 As Integer = 100
            Dim iter2 As Integer

            aaa = Vinf
            bbb = Vsup
            ccc = Vsup

            faa = OBJ_Loop_R(aaa, Vu, Vz, Kb0, HT, Tref, AA, BB, CC, DD, EE, FF)
            fbb = OBJ_Loop_R(bbb, Vu, Vz, Kb0, HT, Tref, AA, BB, CC, DD, EE, FF)
            fcc = fbb

            iter2 = 0
            Do
                If (fbb > 0 And fcc > 0) Or (fbb < 0 And fcc < 0) Then
                    ccc = aaa
                    fcc = faa
                    ddd = bbb - aaa
                    eee = ddd
                End If
                If Math.Abs(fcc) < Math.Abs(fbb) Then
                    aaa = bbb
                    bbb = ccc
                    ccc = aaa
                    faa = fbb
                    fbb = fcc
                    fcc = faa
                End If
                tol11 = 0.000001
                xmm = 0.5 * (ccc - bbb)
                If (Math.Abs(xmm) <= tol11) Or (fbb = 0) Then GoTo Final3
                If (Math.Abs(eee) >= tol11) And (Math.Abs(faa) > Math.Abs(fbb)) Then
                    sss = fbb / faa
                    If aaa = ccc Then
                        ppp = 2 * xmm * sss
                        qqq = 1 - sss
                    Else
                        qqq = faa / fcc
                        rrr = fbb / fcc
                        ppp = sss * (2 * xmm * qqq * (qqq - rrr) - (bbb - aaa) * (rrr - 1))
                        qqq = (qqq - 1) * (rrr - 1) * (sss - 1)
                    End If
                    If ppp > 0 Then qqq = -qqq
                    ppp = Math.Abs(ppp)
                    min11 = 3 * xmm * qqq - Math.Abs(tol11 * qqq)
                    min22 = Math.Abs(eee * qqq)
                    Dim tvar2 As Double
                    If min11 < min22 Then tvar2 = min11
                    If min11 > min22 Then tvar2 = min22
                    If 2 * ppp < tvar2 Then
                        eee = ddd
                        ddd = ppp / qqq
                    Else
                        ddd = xmm
                        eee = ddd
                    End If
                Else
                    ddd = xmm
                    eee = ddd
                End If
                aaa = bbb
                faa = fbb
                If (Math.Abs(ddd) > tol11) Then
                    bbb += ddd
                Else
                    bbb += Math.Sign(xmm) * tol11
                End If
                fbb = OBJ_Loop_R(bbb, Vu, Vz, Kb0, HT, Tref, AA, BB, CC, DD, EE, FF)
                iter2 += 1
            Loop Until iter2 = ITMAX2

Final3:

            Dim flag As Integer
            If i > 19 Then
                flag = 1
                fV = OBJ_Loop_R(0, Vu, Vz, Kb0, HT, Tref, AA, BB, CC, DD, EE, FF)
                fV_inf = OBJ_Loop_R(1, Vu, Vz, Kb0, HT, Tref, AA, BB, CC, DD, EE, FF)
                If Math.Abs(fV) < Math.Abs(fV_inf) Then
                    bbb = 0
                Else
                    bbb = 1
                End If
            End If

            Return bbb 'Return_Results_Loop_R(bbb, Vu, Vz, Kb0, HT, Tref, AA, BB, CC, DD, EE, FF, flag)

        End Function

        Function Return_Results_Loop_R(ByVal Rbb As Double, ByVal Vu As Object, ByVal Vz As Object, ByVal Kb0 As Double, ByVal HT As Double, _
                            ByVal Tref As Double, ByVal AA As Double, ByVal BB As Double, ByVal CC As Double, ByVal DD As Double, _
                            ByVal EE As Double, ByVal FF As Double, ByVal flag As Integer)

            Dim i As Integer
            Dim n As Integer = UBound(Vz)

            Dim Vt(n), Vx(n), Vy(n), Vpbb(n), Vwbb(n), Kb, T, Vu_ant(n) As Double

            Dim sum_vti As Double = 0
            Dim sum_pi, sum_eui_pi As Double

            Dim sum_Hvi0 As Double = 0

            i = 0
            sum_pi = 0
            sum_eui_pi = 0
            Do
                Vpbb(i) = Vz(i) / (1 - Rbb + Kb0 * Rbb * Exp(Vu(i)))
                sum_pi += Vpbb(i)
                sum_eui_pi += Exp(Vu(i)) * Vpbb(i)
                i = i + 1
            Loop Until i = n + 1
            Kb = sum_pi / sum_eui_pi
            T = 1 / Tref + (Log(Kb) - AA) / BB
            T = 1 / T
            If BB = 0 Then T = Tref
            i = 0
            Do
                Vx(i) = Vpbb(i) / sum_pi
                If Vx(i) < 0 Then Vx(i) = 0
                If Vx(i) > 1 Then Vx(i) = 1
                Vy(i) = Exp(Vu(i)) * Vpbb(i) / sum_eui_pi
                If Vy(i) < 0 Then Vy(i) = 0
                If Vy(i) > 1 Then Vy(i) = 1
                i = i + 1
            Loop Until i = n + 1

            Return New Object() {Rbb, Kb, sum_pi, T, Vx, Vy, flag}

        End Function

        Function ESTIMAR_RS(ByVal P As Double, ByVal Vu As Object, ByVal Vz As Object, ByVal Kb0 As Double, ByVal ST As Double, _
                            ByVal Tref As Double, ByVal AA As Double, ByVal BB As Double, ByVal CC As Double, ByVal DD As Double, _
                            ByVal EE As Double, ByVal FF As Double)

            Dim n = UBound(Vz)

            Dim i As Integer

            Dim Vinf, Vsup As Double

            Dim fV, fV_inf, nsub, delta_V As Double

            Vinf = 0
            Vsup = 1

            nsub = 10

            delta_V = (Vsup - Vinf) / nsub

            i = 0
            Do
                i = i + 1
                fV = OBJ_Loop_RS(P, Vinf, Vu, Vz, Kb0, ST, Tref, AA, BB, CC, DD, EE, FF)
                Vinf = Vinf + delta_V
                fV_inf = OBJ_Loop_RS(P, Vinf, Vu, Vz, Kb0, ST, Tref, AA, BB, CC, DD, EE, FF)
            Loop Until fV * fV_inf < 0 Or i > 9
            Vsup = Vinf
            Vinf = Vinf - delta_V

            'método de Brent para encontrar Vc

            Dim aaa, bbb, ccc, ddd, eee, min11, min22, faa, fbb, fcc, ppp, qqq, rrr, sss, tol11, xmm As Double
            Dim ITMAX2 As Integer = 100
            Dim iter2 As Integer

            aaa = Vinf
            bbb = Vsup
            ccc = Vsup

            faa = OBJ_Loop_RS(P, aaa, Vu, Vz, Kb0, ST, Tref, AA, BB, CC, DD, EE, FF)
            fbb = OBJ_Loop_RS(P, bbb, Vu, Vz, Kb0, ST, Tref, AA, BB, CC, DD, EE, FF)
            fcc = fbb

            iter2 = 0
            Do
                If (fbb > 0 And fcc > 0) Or (fbb < 0 And fcc < 0) Then
                    ccc = aaa
                    fcc = faa
                    ddd = bbb - aaa
                    eee = ddd
                End If
                If Math.Abs(fcc) < Math.Abs(fbb) Then
                    aaa = bbb
                    bbb = ccc
                    ccc = aaa
                    faa = fbb
                    fbb = fcc
                    fcc = faa
                End If
                tol11 = 0.0000000001
                xmm = 0.5 * (ccc - bbb)
                If (Math.Abs(xmm) <= tol11) Or (fbb = 0) Then GoTo Final3
                If (Math.Abs(eee) >= tol11) And (Math.Abs(faa) > Math.Abs(fbb)) Then
                    sss = fbb / faa
                    If aaa = ccc Then
                        ppp = 2 * xmm * sss
                        qqq = 1 - sss
                    Else
                        qqq = faa / fcc
                        rrr = fbb / fcc
                        ppp = sss * (2 * xmm * qqq * (qqq - rrr) - (bbb - aaa) * (rrr - 1))
                        qqq = (qqq - 1) * (rrr - 1) * (sss - 1)
                    End If
                    If ppp > 0 Then qqq = -qqq
                    ppp = Math.Abs(ppp)
                    min11 = 3 * xmm * qqq - Math.Abs(tol11 * qqq)
                    min22 = Math.Abs(eee * qqq)
                    Dim tvar2 As Double
                    If min11 < min22 Then tvar2 = min11
                    If min11 > min22 Then tvar2 = min22
                    If 2 * ppp < tvar2 Then
                        eee = ddd
                        ddd = ppp / qqq
                    Else
                        ddd = xmm
                        eee = ddd
                    End If
                Else
                    ddd = xmm
                    eee = ddd
                End If
                aaa = bbb
                faa = fbb
                If (Math.Abs(ddd) > tol11) Then
                    bbb += ddd
                Else
                    bbb += Math.Sign(xmm) * tol11
                End If
                fbb = OBJ_Loop_RS(P, bbb, Vu, Vz, Kb0, ST, Tref, AA, BB, CC, DD, EE, FF)
                iter2 += 1
            Loop Until iter2 = ITMAX2

Final3:
            Dim flag As Integer
            If i > 9 Then
                flag = 1
                fV = OBJ_Loop_RS(P, 0, Vu, Vz, Kb0, ST, Tref, AA, BB, CC, DD, EE, FF)
                fV_inf = OBJ_Loop_RS(P, 1, Vu, Vz, Kb0, ST, Tref, AA, BB, CC, DD, EE, FF)
                If Math.Abs(fV) > Math.Abs(fV_inf) Then
                    bbb = 0
                Else
                    bbb = 1
                End If
            End If

            Return bbb

        End Function

        Function OBJ_Loop_RS(ByVal P As Double, ByVal Rbb As Double, ByVal Vu As Object, ByVal Vz As Object, ByVal Kb0 As Double, ByVal ST As Double, _
                            ByVal Tref As Double, ByVal AA As Double, ByVal BB As Double, ByVal CC As Double, ByVal DD As Double, _
                            ByVal EE As Double, ByVal FF As Double)

            Dim i As Integer
            Dim n As Integer = UBound(Vz)

            Dim Vt(n), Vx(n), Vy(n), Vpbb(n), Vwbb(n), DSv, DSl, Kb, _
                 L, V, fi, fi_ant, fi_ant2, T, Vu_ant(n) As Double

            Dim sum_vti As Double = 0
            Dim sum_pi, sum_eui_pi As Double
            Dim sum_Svi0 As Double = 0
            Dim sum_Sli0 As Double = 0

            i = 0
            sum_pi = 0
            sum_eui_pi = 0
            Do
                Vpbb(i) = Vz(i) / (1 - Rbb + Kb0 * Rbb * Exp(Vu(i)))
                sum_pi += Vpbb(i)
                sum_eui_pi += Exp(Vu(i)) * Vpbb(i)
                i = i + 1
            Loop Until i = n + 1
            Kb = sum_pi / sum_eui_pi
            T = 1 / Tref + (Log(Kb) - AA) / BB
            T = 1 / T
            i = 0
            Do
                Vx(i) = Vpbb(i) / sum_pi
                Vy(i) = Exp(Vu(i)) * Vpbb(i) / sum_eui_pi
                i = i + 1
            Loop Until i = n + 1

            L = (1 - Rbb) * sum_pi
            V = 1 - L

            '===============
            ' PASSO 4
            '===============

            DSv = CC + DD * (T - Tref)
            DSl = EE + FF * (T - Tref)

            '===============
            ' PASSO 5
            '===============

            sum_Svi0 = Me.RET_Sid(Tref, T, P, Vy) * Me.AUX_MMM(Vy)
            sum_Sli0 = Me.RET_Sid(Tref, T, P, Vx) * Me.AUX_MMM(Vx)

            '===============
            ' PASSO 6
            '===============

            fi_ant2 = fi_ant
            fi_ant = fi
            fi = L * (DSl + sum_Sli0) + V * (DSv + sum_Svi0) - ST * Me.AUX_MMM(Vz)

            '===============
            ' PASSO 7
            '===============

            Return fi

        End Function

        Function Return_Results_Loop_RS(ByVal P As Double, ByVal Rbb As Double, ByVal Vu As Object, ByVal Vz As Object, ByVal Kb0 As Double, ByVal HT As Double, _
                            ByVal Tref As Double, ByVal AA As Double, ByVal BB As Double, ByVal CC As Double, ByVal DD As Double, _
                            ByVal EE As Double, ByVal FF As Double, ByVal flag As Integer)

            Dim i As Integer
            Dim n As Integer = UBound(Vz)

            Dim Vt(n), Vx(n), Vy(n), Vpbb(n), Vwbb(n), Kb, T, Vu_ant(n) As Double

            Dim sum_vti As Double = 0
            Dim sum_pi, sum_eui_pi As Double

            i = 0
            sum_pi = 0
            sum_eui_pi = 0
            Do
                Vpbb(i) = Vz(i) / (1 - Rbb + Kb0 * Rbb * Exp(Vu(i)))
                sum_pi += Vpbb(i)
                sum_eui_pi += Exp(Vu(i)) * Vpbb(i)
                i = i + 1
            Loop Until i = n + 1
            Kb = sum_pi / sum_eui_pi
            T = 1 / Tref + (Log(Kb) - AA) / BB
            T = 1 / T
            If BB = 0 Then T = Tref
            i = 0
            Do
                Vx(i) = Vpbb(i) / sum_pi
                If Vx(i) < 0 Then Vx(i) = 0
                If Vx(i) > 1 Then Vx(i) = 1
                Vy(i) = Exp(Vu(i)) * Vpbb(i) / sum_eui_pi
                If Vy(i) < 0 Then Vy(i) = 0
                If Vy(i) > 1 Then Vy(i) = 1
                i = i + 1
            Loop Until i = n + 1

            Return New Object() {Rbb, Kb, sum_pi, T, Vx, Vy, flag}

        End Function

        Public Sub broydn(ByRef N As Object, ByRef X As Object, ByRef F As Object, ByRef P As Object, ByRef XB As Object, ByRef FB As Object, ByRef H As Object, ByRef IFLAG As Object)
            '
            '**********************************************************************
            '
            '       N = NUMBER OF EQUATIONS
            '       X(N) = CURRENT VALUE OF X, INITAL GUESS X0 ON FIRST CALL
            '              THE VALUE OF X IS NOT UPDATED AND MUST BE UPDATED IN
            '              CALLING PROGRAM
            '       F(N) = VALUE OF F(X) MUST BE PROVIDED ON ALL CALLS
            '       P(N) = STEP PREDICTED BY BROYDN (USED TO UPDATE X)
            '              THE NEW VALUE OF X IS X+P
            '       XB(N) = RETENTION FOR X VECTOR
            '       FB(N) = RETENTION FOR F VECTOR
            '       H(N,N) = BROYDEN H MATRIX IT MUST BE INITIALIZED TO A CLOSE
            '                J(X0)**-1 OR IDENTITY MATRIX
            '       IFLAG = CALCULATION CONTROL FLAG
            '               0 INITIAL CALL, NO H UPDATE
            '               1 UPDATE CALL, NO H DAMPING
            '
            Dim I As Short
            Dim J As Short
            Dim PTP As Double
            Dim PTH As Double
            Dim THETA As Double
            Dim PTHY As Double
            Dim PTHF As Double
            Dim HY As Double
            Dim DENOM As Double
            '
            '      INITIAL CALL
            '
            If (IFLAG <> 0) Then
                PTP = 0.0#
                '
                For I = 0 To N + 1 'do 30 I=1,N
                    P(I) = X(I) - XB(I)
                    PTP = PTP + P(I) * P(I)
                    HY = 0.0#
                    For J = 0 To N + 1 '  DO 20 J=1,N
                        HY = HY + H(I, J) * (F(J) - FB(J))
20:                 Next J
                    XB(I) = HY - P(I)
30:             Next I
                PTHY = 0.0#
                PTHF = 0.0#
                '
                For I = 0 To N + 1 ' DO 40 I=1,N
                    PTH = 0.0#
                    For J = 0 To N + 1 '  DO 35 J=1,N
                        PTH = PTH + P(J) * H(J, I)
35:                 Next J
                    PTHY = PTHY + PTH * (F(I) - FB(I))
                    PTHF = PTHF + PTH * F(I)
                    FB(I) = PTH
40:             Next I
                THETA = 1.0#
                '
                DENOM = (1.0# - THETA) * PTP + THETA * PTHY
                '
                For I = 0 To N + 1 ' DO 50 I=1,N
                    For J = 0 To N + 1 ' DO 50 J=1,N
                        H(I, J) = H(I, J) - THETA * XB(I) * FB(J) / DENOM
                    Next J
50:             Next I
                '
            End If
            For I = 0 To N + 1 '  DO 70 I=1,N
                XB(I) = X(I)
                FB(I) = F(I)
                P(I) = 0.0#
                '
                For J = 0 To N + 1 '  DO 70 J=1,N
                    P(I) = P(I) - H(I, J) * F(J)
                Next J
70:         Next I
            ''
        End Sub

#End Region

#Region "    Métodos Numéricos"

        Public Function IntegralSimpsonCp(ByVal a As Double, _
                 ByVal b As Double, _
                 ByVal Epsilon As Double, ByVal subst As String) As Double

            Dim Result As Double
            Dim switch As Boolean = False
            Dim h As Double
            Dim s As Double
            Dim s1 As Double
            Dim s2 As Double
            Dim s3 As Double
            Dim x As Double
            Dim tm As Double

            If a > b Then
                switch = True
                tm = a
                a = b
                b = tm
            ElseIf Abs(a - b) < 0.01 Then
                Return 0
            End If

            s2 = 1.0#
            h = b - a
            s = Me.AUX_CPi(subst, a) + Me.AUX_CPi(subst, b)
            Do
                s3 = s2
                h = h / 2.0#
                s1 = 0.0#
                x = a + h
                Do
                    s1 = s1 + 2.0# * Me.AUX_CPi(subst, x)
                    x = x + 2.0# * h
                Loop Until Not x < b
                s = s + s1
                s2 = (s + s1) * h / 3.0#
                x = Abs(s3 - s2) / 15.0#
            Loop Until Not x > Epsilon
            Result = s2

            If switch Then Result = -Result

            IntegralSimpsonCp = Result

        End Function

        Public Function IntegralSimpsonCp_T(ByVal a As Double, _
         ByVal b As Double, _
         ByVal Epsilon As Double, ByVal subst As String) As Double

            'Cp = A + B*T + C*T^2 + D*T^3 + E*T^4 where Cp in kJ/kg-mol , T in K 


            Dim Result As Double
            Dim h As Double
            Dim s As Double
            Dim s1 As Double
            Dim s2 As Double
            Dim s3 As Double
            Dim x As Double
            Dim tm As Double
            Dim switch As Boolean = False

            If a > b Then
                switch = True
                tm = a
                a = b
                b = tm
            ElseIf Abs(a - b) < 0.01 Then
                Return 0
            End If

            s2 = 1.0#
            h = b - a
            s = Me.AUX_CPi(subst, a) / a + Me.AUX_CPi(subst, b) / b
            Do
                s3 = s2
                h = h / 2.0#
                s1 = 0.0#
                x = a + h
                Do
                    s1 = s1 + 2.0# * Me.AUX_CPi(subst, x) / x
                    x = x + 2.0# * h
                Loop Until Not x < b
                s = s + s1
                s2 = (s + s1) * h / 3.0#
                x = Abs(s3 - s2) / 15.0#
            Loop Until Not x > Epsilon
            Result = s2

            If switch Then Result = -Result

            IntegralSimpsonCp_T = Result

        End Function

#End Region

#Region "    Pontos de Bolha e de orvalho, DEWP/T, BUBP/T"

        Function DKDT_PR(ByVal T As Double, ByVal P As Double, ByVal Vx As Object, ByVal Vy As Object, ByVal VKij As Object, ByVal VTc As Object, ByVal VPc As Object, ByVal VTb As Object, ByVal Vw As Object, ByVal VCT As Object) As Object

            'T,Tc em K
            'P,Pc em Pa
            Dim n = UBound(Vx)

            Dim PHIV(n), PHIL(n), LN_CFL(n), LN_CFV(n) As Double
            Dim ai(n), bi(n), aml2(n), amv2(n) As Double
            Dim Vp(n), R, coeff(3), tmp(2, 2) As Double
            Dim Tc(n), Pc(n), Vc(n), W(n), Zc(n), alpha(n), m(n), a(n, n), b(n, n), ZL, ZV, Tr(n) As Double
            Dim i, j
            Dim t1, t2, t3, t4, t5 As Double
            Dim h = 0.0001
            Dim stmp4_ant, stmp4 As Double
            stmp4_ant = 0
            stmp4 = 0

            Dim KI(n), KI2(n), KI_ant(n), dKdT(n)

            R = 8.314

            Dim chk As Boolean = False

Start:

            i = 0
            Do
                Tc(i) = VTc(i)
                Tr(i) = T / Tc(i)
                Pc(i) = VPc(i)
                W(i) = Vw(i)
                Vp(i) = Me.AUX_PVAPi(i, T)
                KI_ant(i) = Vy(i) / Vx(i)
                i = i + 1
            Loop Until i = n + 1

            Dim ai_(n)

            i = 0
            Do
                If T / Tc(i) <= 1 Then
                    alpha(i) = (1 + (0.37464 + 1.54226 * W(i) - 0.26992 * W(i) ^ 2) * (1 - (T / Tc(i)) ^ 0.5)) ^ 2
                Else
                    alpha(i) = (1 + 1.21 * (0.37464 + 1.54226 * W(i) - 0.26992 * W(i) ^ 2) * (1 - (T / Tc(i)) ^ 0.5)) ^ 2
                End If
                ai(i) = 0.45724 * alpha(i) * R ^ 2 * Tc(i) ^ 2 / Pc(i)
                ai_(i) = ai(i) ^ 0.5
                bi(i) = 0.0778 * R * Tc(i) / Pc(i)
                i = i + 1
            Loop Until i = n + 1

            i = 0
            Do
                j = 0
                Do
                    a(i, j) = (ai(i) * ai(j)) ^ 0.5 * (1 - VKij(i, j))
                    j = j + 1
                Loop Until j = n + 1
                i = i + 1
            Loop Until i = n + 1

            ' CALCULO DAS RAIZES PARA A FASE LIQUIDA
            i = 0
            Do
                aml2(i) = 0
                i = i + 1
            Loop Until i = n + 1

            i = 0
            Dim aml = 0
            Do
                j = 0
                Do
                    aml = aml + Vx(i) * Vx(j) * a(i, j)
                    aml2(i) = aml2(i) + Vx(j) * a(j, i)
                    j = j + 1
                Loop Until j = n + 1
                i = i + 1
            Loop Until i = n + 1

            i = 0
            Dim bml = 0
            Do
                bml = bml + Vx(i) * bi(i)
                i = i + 1
            Loop Until i = n + 1

            Dim AG = aml * P / (R * T) ^ 2
            Dim BG = bml * P / (R * T)

            coeff(0) = -AG * BG + BG ^ 2 + BG ^ 3
            coeff(1) = AG - 3 * BG ^ 2 - 2 * BG
            coeff(2) = BG - 1
            coeff(3) = 1

            Dim temp1 = Poly_Roots(coeff)
            Dim tv = 0
            Dim tv2 = 0

            Try

                If temp1(0, 0) > temp1(1, 0) Then
                    tv = temp1(1, 0)
                    tv2 = temp1(1, 1)
                    temp1(1, 0) = temp1(0, 0)
                    temp1(0, 0) = tv
                    temp1(1, 1) = temp1(0, 1)
                    temp1(0, 1) = tv2
                End If
                If temp1(0, 0) > temp1(2, 0) Then
                    tv = temp1(2, 0)
                    temp1(2, 0) = temp1(0, 0)
                    temp1(0, 0) = tv
                    tv2 = temp1(2, 1)
                    temp1(2, 1) = temp1(0, 1)
                    temp1(0, 1) = tv2
                End If
                If temp1(1, 0) > temp1(2, 0) Then
                    tv = temp1(2, 0)
                    temp1(2, 0) = temp1(1, 0)
                    temp1(1, 0) = tv
                    tv2 = temp1(2, 1)
                    temp1(2, 1) = temp1(1, 1)
                    temp1(1, 1) = tv2
                End If

                ZL = temp1(0, 0)
                If temp1(0, 1) <> 0 Then
                    ZL = temp1(1, 0)
                    If temp1(1, 1) <> 0 Then
                        ZL = temp1(2, 0)
                    End If
                End If

                If ZL < 0 Then
                    ZL = temp1(0, 0)
                    If temp1(0, 0) < 0 Then
                        ZL = temp1(1, 0)
                        If temp1(1, 0) < 0 Then
                            ZL = temp1(2, 0)
                        End If
                    End If
                End If

            Catch

                Dim findZL
                ZL = 0
                Do
                    findZL = coeff(3) * ZL ^ 3 + coeff(2) * ZL ^ 2 + coeff(1) * ZL + coeff(0)
                    ZL += 0.000001
                    If ZL > 1 Then ZL = 0
                Loop Until Math.Abs(findZL) < 0.0001

            End Try

            Dim sum_ci As Double = 0
            For i = 0 To n
                sum_ci += Vc(i)
            Next

            ZL = ZL - P * sum_ci / (R * T)

            'Dim betaL = 1 / P * (1 - (BG * ZL ^ 2 + AG * ZL - 6 * BG ^ 2 * ZL - 2 * BG * ZL - 2 * AG * BG + 2 * BG ^ 2 + 2 * BG) / (ZL * (3 * ZL ^ 2 - 2 * ZL + 2 * BG * ZL + AG - 3 * BG ^ 2 - 2 * BG)))

            'Dim res As Object
            'If betaL > 0.005 / 101325 Then
            '    res = Me.m_pr.GeneratePseudoRoot(T, P, Vx, VKij, VTc, VPc, Vw, VTb, "L")
            '    If Not res Is Nothing Then
            '        ZL = res(0)
            '        AG = res(1)
            '        BG = res(2)
            '        'amv = res(3)
            '        'bmv = res(4)
            '    End If
            'End If

            ' CALCULO DO COEFICIENTE DE FUGACIDADE DA FASE LIQUIDA

            i = 0
            Do
                'If Tr(i) >= 1 Then
                '    LN_CFL(i) = Math.Log(Vp(i))
                '    PHIL(i) = Math.Exp(Vx(i) * LN_CFL(i))
                'Else
                t1 = bi(i) * (ZL - 1) / bml
                t2 = -Math.Log(ZL - BG)
                t3 = aml * (2 * aml2(i) / aml - bi(i) / bml)
                t4 = Math.Log((ZL + (1 + 2 ^ 0.5) * BG) / (ZL + (1 - 2 ^ 0.5) * BG))
                t5 = 2 * 2 ^ 0.5 * bml * R * T
                LN_CFL(i) = t1 + t2 - (t3 * t4 / t5)
                PHIL(i) = Math.Exp(LN_CFL(i)) * Vx(i) * P
                'End If
                i = i + 1
            Loop Until i = n + 1

            ' CALCULO DAS RAIZES PARA A FASE VAPOR

            i = 0
            Do
                amv2(i) = 0
                i = i + 1
            Loop Until i = n + 1

            i = 0
            Dim amv = 0
            Do
                j = 0
                Do
                    amv = amv + Vy(i) * Vy(j) * a(i, j)
                    amv2(i) = amv2(i) + Vy(j) * a(j, i)
                    j = j + 1
                Loop Until j = n + 1
                i = i + 1
            Loop Until i = n + 1

            i = 0
            Dim bmv = 0
            Do
                bmv = bmv + Vy(i) * bi(i)
                i = i + 1
            Loop Until i = n + 1

            AG = amv * P / (R * T) ^ 2
            BG = bmv * P / (R * T)

            coeff(0) = -AG * BG + BG ^ 2 + BG ^ 3
            coeff(1) = AG - 3 * BG ^ 2 - 2 * BG
            coeff(2) = BG - 1
            coeff(3) = 1

            temp1 = Poly_Roots(coeff)
            tv = 0

            Try

                If temp1(0, 0) > temp1(1, 0) Then
                    tv = temp1(1, 0)
                    temp1(1, 0) = temp1(0, 0)
                    temp1(0, 0) = tv
                    tv2 = temp1(1, 1)
                    temp1(1, 1) = temp1(0, 1)
                    temp1(0, 1) = tv2
                End If
                If temp1(0, 0) > temp1(2, 0) Then
                    tv = temp1(2, 0)
                    temp1(2, 0) = temp1(0, 0)
                    temp1(0, 0) = tv
                    tv2 = temp1(2, 1)
                    temp1(2, 1) = temp1(0, 1)
                    temp1(0, 1) = tv2
                End If
                If temp1(1, 0) > temp1(2, 0) Then
                    tv = temp1(2, 0)
                    temp1(2, 0) = temp1(1, 0)
                    temp1(1, 0) = tv
                    tv2 = temp1(2, 1)
                    temp1(2, 1) = temp1(1, 1)
                    temp1(1, 1) = tv2
                End If

                ZV = temp1(2, 0)
                If temp1(2, 1) <> 0 Then
                    ZV = temp1(1, 0)
                    If temp1(1, 1) <> 0 Then
                        ZV = temp1(0, 0)
                    End If
                End If

            Catch

                Dim findZV
                ZV = 1
                Do
                    findZV = coeff(3) * ZV ^ 3 + coeff(2) * ZV ^ 2 + coeff(1) * ZV + coeff(0)
                    ZV -= 0.00001
                    If ZV < 0 Then ZV = 1
                Loop Until Math.Abs(findZV) < 0.0001

            End Try

            ZV = ZV - P * sum_ci / (R * T)

            'Dim betaV = 1 / P * (1 - (BG * ZV ^ 2 + AG * ZV - 6 * BG ^ 2 * ZV - 2 * BG * ZV - 2 * AG * BG + 2 * BG ^ 2 + 2 * BG) / (ZV * (3 * ZV ^ 2 - 2 * ZV + 2 * BG * ZV + AG - 3 * BG ^ 2 - 2 * BG)))

            'If betaV > 3 / (P / 101325) Or betaV < 0.9 / (P / 101325) Then
            '    res = Me.m_pr.GeneratePseudoRoot(T, P, Vy, VKij, VTc, VPc, Vw, VTb, "V")
            '    If Not res Is Nothing Then
            '        ZV = res(0)
            '        AG = res(1)
            '        BG = res(2)
            '        'amv = res(3)
            '        'bmv = res(4)
            '    End If
            'End If

            ' CALCULO DO COEFICIENTE DE FUGACIDADE DA FASE VAPOR

            i = 0
            Do
                t1 = bi(i) * (ZV - 1) / bmv
                t2 = -Math.Log(ZV - BG)
                t3 = amv * (2 * amv2(i) / amv - bi(i) / bmv)
                t4 = Math.Log(ZV + (1 + 2 ^ 0.5) * BG) - Math.Log(ZV + ((1 - 2 ^ 0.5) * BG))
                t5 = 8 ^ 0.5 * bmv * R * T
                LN_CFV(i) = t1 + t2 - (t3 * t4 / t5)
                PHIV(i) = Math.Exp(LN_CFV(i)) * Vy(i) * P
                i = i + 1
            Loop Until i = n + 1

            i = 0
            Do
                KI(i) = (Math.Exp(LN_CFL(i)) / (Math.Exp(LN_CFV(i))))
                If chk = False Then KI2(i) = (Math.Exp(LN_CFL(i)) / (Math.Exp(LN_CFV(i))))
                i = i + 1
            Loop Until i = n + 1

            If chk = True Then GoTo Final

            T = T + h

            'i = 0
            'Do
            '    Vx(i) = Vx(i) * KI(i) / KI_ant(i)
            '    Vy(i) = Vy(i) / KI(i) * KI_ant(i)
            '    i += 1
            'Loop Until i = n + 1

            'i = 0
            'Dim soma_x = 0
            'Dim soma_y = 0
            'Do
            '    soma_x = soma_x + Vx(i)
            '    soma_y = soma_y + Vy(i)
            '    i = i + 1
            'Loop Until i = n + 1

            'i = 0
            'Do
            '    Vx(i) = Vx(i) / soma_x
            '    Vy(i) = Vy(i) / soma_y
            '    i = i + 1
            'Loop Until i = n + 1

            chk = True

            GoTo Start

Final:

            i = 0
            Do
                dKdT(i) = (KI(i) - KI2(i)) / h
                i = i + 1
            Loop Until i = n + 1

            Return dKdT

        End Function

        Function DKDP_PR(ByVal T As Double, ByVal P As Double, ByVal Vx As Object, ByVal Vy As Object, ByVal VKij As Object, ByVal VTc As Object, ByVal VPc As Object, ByVal VTb As Object, ByVal Vw As Object, ByVal VCT As Object) As Object

            'T,Tc em K
            'P,Pc em Pa
            Dim n = UBound(Vx)

            Dim PHIV(n), PHIL(n), LN_CFL(n), LN_CFV(n) As Double
            Dim ai(n), bi(n), aml2(n), amv2(n) As Double
            Dim Vp(n), R, coeff(3), tmp(2, 2) As Double
            Dim Tc(n), Pc(n), Vc(n), W(n), Zc(n), alpha(n), m(n), a(n, n), b(n, n), ZL, ZV, Tr(n) As Double
            Dim i, j
            Dim t1, t2, t3, t4, t5 As Double
            Dim h = 0.00001
            Dim stmp4_ant, stmp4 As Double
            stmp4_ant = 0
            stmp4 = 0

            Dim KI(n), KI2(n), KI_ant(n), dKdP(n)

            R = 8.314

            Dim chk As Boolean = False

Start:

            i = 0
            Do
                Tc(i) = VTc(i)
                Tr(i) = T / Tc(i)
                Pc(i) = VPc(i)
                W(i) = Vw(i)
                Vp(i) = Me.AUX_PVAPi(i, T)
                KI_ant(i) = Vy(i) / Vx(i)
                i = i + 1
            Loop Until i = n + 1

            Dim ai_(n)

            i = 0
            Do
                If T / Tc(i) <= 1 Then
                    alpha(i) = (1 + (0.37464 + 1.54226 * W(i) - 0.26992 * W(i) ^ 2) * (1 - (T / Tc(i)) ^ 0.5)) ^ 2
                Else
                    alpha(i) = (1 + 1.21 * (0.37464 + 1.54226 * W(i) - 0.26992 * W(i) ^ 2) * (1 - (T / Tc(i)) ^ 0.5)) ^ 2
                End If
                ai(i) = 0.45724 * alpha(i) * R ^ 2 * Tc(i) ^ 2 / Pc(i)
                ai_(i) = ai(i) ^ 0.5
                bi(i) = 0.0778 * R * Tc(i) / Pc(i)
                i = i + 1
            Loop Until i = n + 1

            i = 0
            Do
                j = 0
                Do
                    a(i, j) = (ai(i) * ai(j)) ^ 0.5 * (1 - VKij(i, j))
                    j = j + 1
                Loop Until j = n + 1
                i = i + 1
            Loop Until i = n + 1

            ' CALCULO DAS RAIZES PARA A FASE LIQUIDA

            i = 0
            Do
                aml2(i) = 0
                i = i + 1
            Loop Until i = n + 1

            i = 0
            Dim aml = 0
            Do
                j = 0
                Do
                    aml = aml + Vx(i) * Vx(j) * a(i, j)
                    aml2(i) = aml2(i) + Vx(j) * a(j, i)
                    j = j + 1
                Loop Until j = n + 1
                i = i + 1
            Loop Until i = n + 1

            i = 0
            Dim bml = 0
            Do
                bml = bml + Vx(i) * bi(i)
                i = i + 1
            Loop Until i = n + 1

            Dim AG = aml * P / (R * T) ^ 2
            Dim BG = bml * P / (R * T)

            coeff(0) = -AG * BG + BG ^ 2 + BG ^ 3
            coeff(1) = AG - 3 * BG ^ 2 - 2 * BG
            coeff(2) = BG - 1
            coeff(3) = 1

            Dim temp1 = Poly_Roots(coeff)
            Dim tv = 0
            Dim tv2 = 0

            Try

                If temp1(0, 0) > temp1(1, 0) Then
                    tv = temp1(1, 0)
                    tv2 = temp1(1, 1)
                    temp1(1, 0) = temp1(0, 0)
                    temp1(0, 0) = tv
                    temp1(1, 1) = temp1(0, 1)
                    temp1(0, 1) = tv2
                End If
                If temp1(0, 0) > temp1(2, 0) Then
                    tv = temp1(2, 0)
                    temp1(2, 0) = temp1(0, 0)
                    temp1(0, 0) = tv
                    tv2 = temp1(2, 1)
                    temp1(2, 1) = temp1(0, 1)
                    temp1(0, 1) = tv2
                End If
                If temp1(1, 0) > temp1(2, 0) Then
                    tv = temp1(2, 0)
                    temp1(2, 0) = temp1(1, 0)
                    temp1(1, 0) = tv
                    tv2 = temp1(2, 1)
                    temp1(2, 1) = temp1(1, 1)
                    temp1(1, 1) = tv2
                End If

                ZL = temp1(0, 0)
                If temp1(0, 1) <> 0 Then
                    ZL = temp1(1, 0)
                    If temp1(1, 1) <> 0 Then
                        ZL = temp1(2, 0)
                    End If
                End If

                If ZL < 0 Then
                    ZL = temp1(0, 0)
                    If temp1(0, 0) < 0 Then
                        ZL = temp1(1, 0)
                        If temp1(1, 0) < 0 Then
                            ZL = temp1(2, 0)
                        End If
                    End If
                End If

            Catch

                Dim findZL
                ZL = 0
                Do
                    findZL = coeff(3) * ZL ^ 3 + coeff(2) * ZL ^ 2 + coeff(1) * ZL + coeff(0)
                    ZL += 0.000001
                    If ZL > 1 Then ZL = 0
                Loop Until Math.Abs(findZL) < 0.0001

            End Try

            Dim sum_ci As Double = 0
            For i = 0 To n
                sum_ci += VCT(i)
            Next

            ZL = ZL - P * sum_ci / (R * T)

            'Dim betaL = 1 / P * (1 - (BG * ZL ^ 2 + AG * ZL - 6 * BG ^ 2 * ZL - 2 * BG * ZL - 2 * AG * BG + 2 * BG ^ 2 + 2 * BG) / (ZL * (3 * ZL ^ 2 - 2 * ZL + 2 * BG * ZL + AG - 3 * BG ^ 2 - 2 * BG)))

            'Dim res As Object
            'If betaL > 0.005 / 101325 Then
            '    res = Me.m_pr.GeneratePseudoRoot(T, P, Vx, VKij, VTc, VPc, Vw, VTb, "L")
            '    If Not res Is Nothing Then
            '        ZL = res(0)
            '        AG = res(1)
            '        BG = res(2)
            '        'amv = res(3)
            '        'bmv = res(4)
            '    End If
            'End If

            ' CALCULO DO COEFICIENTE DE FUGACIDADE DA FASE LIQUIDA

            i = 0
            Do
                'If Tr(i) >= 1 Then
                '    LN_CFL(i) = Math.Log(Vp(i))
                '    PHIL(i) = Math.Exp(Vx(i) * LN_CFL(i))
                'Else
                t1 = bi(i) * (ZL - 1) / bml
                t2 = -Math.Log(ZL - BG)
                t3 = aml * (2 * aml2(i) / aml - bi(i) / bml)
                t4 = Math.Log((ZL + (1 + 2 ^ 0.5) * BG) / (ZL + (1 - 2 ^ 0.5) * BG))
                t5 = 2 * 2 ^ 0.5 * bml * R * T
                LN_CFL(i) = t1 + t2 - (t3 * t4 / t5)
                PHIL(i) = Math.Exp(LN_CFL(i)) * Vx(i) * P
                'End If
                i = i + 1
            Loop Until i = n + 1

            ' CALCULO DAS RAIZES PARA A FASE VAPOR

            i = 0
            Do
                amv2(i) = 0
                i = i + 1
            Loop Until i = n + 1

            i = 0
            Dim amv = 0
            Do
                j = 0
                Do
                    amv = amv + Vy(i) * Vy(j) * a(i, j)
                    amv2(i) = amv2(i) + Vy(j) * a(j, i)
                    j = j + 1
                Loop Until j = n + 1
                i = i + 1
            Loop Until i = n + 1

            i = 0
            Dim bmv = 0
            Do
                bmv = bmv + Vy(i) * bi(i)
                i = i + 1
            Loop Until i = n + 1

            AG = amv * P / (R * T) ^ 2
            BG = bmv * P / (R * T)

            coeff(0) = -AG * BG + BG ^ 2 + BG ^ 3
            coeff(1) = AG - 3 * BG ^ 2 - 2 * BG
            coeff(2) = BG - 1
            coeff(3) = 1

            temp1 = Poly_Roots(coeff)
            tv = 0

            Try

                If temp1(0, 0) > temp1(1, 0) Then
                    tv = temp1(1, 0)
                    temp1(1, 0) = temp1(0, 0)
                    temp1(0, 0) = tv
                    tv2 = temp1(1, 1)
                    temp1(1, 1) = temp1(0, 1)
                    temp1(0, 1) = tv2
                End If
                If temp1(0, 0) > temp1(2, 0) Then
                    tv = temp1(2, 0)
                    temp1(2, 0) = temp1(0, 0)
                    temp1(0, 0) = tv
                    tv2 = temp1(2, 1)
                    temp1(2, 1) = temp1(0, 1)
                    temp1(0, 1) = tv2
                End If
                If temp1(1, 0) > temp1(2, 0) Then
                    tv = temp1(2, 0)
                    temp1(2, 0) = temp1(1, 0)
                    temp1(1, 0) = tv
                    tv2 = temp1(2, 1)
                    temp1(2, 1) = temp1(1, 1)
                    temp1(1, 1) = tv2
                End If

                ZV = temp1(2, 0)
                If temp1(2, 1) <> 0 Then
                    ZV = temp1(1, 0)
                    If temp1(1, 1) <> 0 Then
                        ZV = temp1(0, 0)
                    End If
                End If

            Catch

                Dim findZV
                ZV = 1
                Do
                    findZV = coeff(3) * ZV ^ 3 + coeff(2) * ZV ^ 2 + coeff(1) * ZV + coeff(0)
                    ZV -= 0.00001
                    If ZV < 0 Then ZV = 1
                Loop Until Math.Abs(findZV) < 0.0001

            End Try

            ZV = ZV - P * sum_ci / (R * T)

            'Dim betaV = 1 / P * (1 - (BG * ZV ^ 2 + AG * ZV - 6 * BG ^ 2 * ZV - 2 * BG * ZV - 2 * AG * BG + 2 * BG ^ 2 + 2 * BG) / (ZV * (3 * ZV ^ 2 - 2 * ZV + 2 * BG * ZV + AG - 3 * BG ^ 2 - 2 * BG)))

            'If betaV > 3 / (P / 101325) Or betaV < 0.9 / (P / 101325) Then
            '    res = Me.m_pr.GeneratePseudoRoot(T, P, Vy, VKij, VTc, VPc, Vw, VTb, "V")
            '    If Not res Is Nothing Then
            '        ZV = res(0)
            '        AG = res(1)
            '        BG = res(2)
            '        'amv = res(3)
            '        'bmv = res(4)
            '    End If
            'End If

            ' CALCULO DO COEFICIENTE DE FUGACIDADE DA FASE VAPOR

            i = 0
            Do
                t1 = bi(i) * (ZV - 1) / bmv
                t2 = -Math.Log(ZV - BG)
                t3 = amv * (2 * amv2(i) / amv - bi(i) / bmv)
                t4 = Math.Log(ZV + (1 + 2 ^ 0.5) * BG) - Math.Log(ZV + ((1 - 2 ^ 0.5) * BG))
                t5 = 8 ^ 0.5 * bmv * R * T
                LN_CFV(i) = t1 + t2 - (t3 * t4 / t5)
                PHIV(i) = Math.Exp(LN_CFV(i)) * Vy(i) * P
                i = i + 1
            Loop Until i = n + 1

            i = 0
            Do
                KI(i) = (Math.Exp(LN_CFL(i)) / (Math.Exp(LN_CFV(i))))
                If chk = False Then KI2(i) = (Math.Exp(LN_CFL(i)) / (Math.Exp(LN_CFV(i))))
                i = i + 1
            Loop Until i = n + 1

            If chk = True Then GoTo Final

            P = P + h

            'i = 0
            'Do
            '    Vx(i) = Vx(i) * KI(i) / KI_ant(i)
            '    Vy(i) = Vy(i) / KI(i) * KI_ant(i)
            '    i += 1
            'Loop Until i = n + 1

            'i = 0
            'Dim soma_x = 0
            'Dim soma_y = 0
            'Do
            '    soma_x = soma_x + Vx(i)
            '    soma_y = soma_y + Vy(i)
            '    i = i + 1
            'Loop Until i = n + 1

            'i = 0
            'Do
            '    Vx(i) = Vx(i) / soma_x
            '    Vy(i) = Vy(i) / soma_y
            '    i = i + 1
            'Loop Until i = n + 1

            chk = True

            GoTo Start

Final:

            i = 0
            Do
                dKdP(i) = (KI(i) - KI2(i)) / h
                i = i + 1
            Loop Until i = n + 1

            Return dKdP

        End Function

        Function DEWP_PR_M2(ByVal T As Double, ByVal Vy As Object, ByVal VKij As Object, ByVal VTc As Object, ByVal VPc As Object, ByVal VTb As Object, ByVal Vw As Object, ByVal KI As Object, ByVal VCT As Object, Optional ByVal P As Double = 0) As Object

            Dim dFdP As Double
            Dim cnt As Integer = 0

            'T,Tc em K
            'P,Pc em Pa
            Dim n = UBound(Vy)

            Dim chk As Boolean = False
            Dim marcador2 As Integer

            Dim F As Double
            Dim dKdP(n), dKdPi(n) As Object
            Dim Vx(n), PHIV(n), PHIL(n), LN_CFL(n), LN_CFV(n) As Double
            Dim ai(n), bi(n), aml2(n), amv2(n) As Double
            Dim Vp(n), R, coeff(3) As Double
            Dim Tc(n), Pc(n), W(n), alpha(n), a(n, n), ZL, ZV, Tr(n) As Double
            Dim i, j
            Dim t1, t2, t3, t4, t5 As Double
            Dim soma_x As Double
            Dim marcador
            Dim stmp4_ant, stmp4, Pant As Double
            stmp4_ant = 0
            stmp4 = 0
            Pant = 0

            R = 8.314

            i = 0
            Do
                Tc(i) = VTc(i)
                Tr(i) = T / Tc(i)
                Pc(i) = VPc(i)
                W(i) = Vw(i)
                Vp(i) = Me.AUX_PVAPi(i, T)
                i = i + 1
            Loop Until i = n + 1

            If P = 0 Then
                'generate first estimate for P
                i = 0
                Do
                    P = P + (Vy(i) / Vp(i))
                    i = i + 1
                Loop Until i = n + 1
                P = 1 / P
            End If

            Dim ai_(n)

            i = 0
            Do
                alpha(i) = (1 + (0.37464 + 1.54226 * W(i) - 0.26992 * W(i) ^ 2) * (1 - (T / Tc(i)) ^ 0.5)) ^ 2
                ai(i) = 0.45724 * alpha(i) * R ^ 2 * Tc(i) ^ 2 / Pc(i)
                ai_(i) = ai(i) ^ 0.5
                bi(i) = 0.0778 * R * Tc(i) / Pc(i)
                i = i + 1
            Loop Until i = n + 1

            i = 0
            Do
                j = 0
                Do
                    a(i, j) = (ai(i) * ai(j)) ^ 0.5 * (1 - VKij(i, j))
                    j = j + 1
                Loop Until j = n + 1
                i = i + 1
            Loop Until i = n + 1

            i = 0
            Do
                If KI(i) = 0 Then KI(i) = Vp(i) / P
                dKdPi(i) = -Vp(i) / P ^ 2
                Vx(i) = Vy(i) / KI(i)
                i = i + 1
            Loop Until i = n + 1

            i = 0
            soma_x = 0
            Do
                soma_x = soma_x + Vx(i)
                i = i + 1
            Loop Until i = n + 1

            i = 0
            Do
                Vx(i) = Vx(i) / soma_x
                i = i + 1
            Loop Until i = n + 1

            ' CALCULO DAS RAIZES PARA A FASE VAPOR

            i = 0
            Do
                amv2(i) = 0
                i = i + 1
            Loop Until i = n + 1

            i = 0
            Dim amv = 0
            Do
                j = 0
                Do
                    amv = amv + Vy(i) * Vy(j) * a(i, j)
                    amv2(i) = amv2(i) + Vy(j) * a(j, i)
                    j = j + 1
                Loop Until j = n + 1
                i = i + 1
            Loop Until i = n + 1

            i = 0
            Dim bmv = 0
            Do
                bmv = bmv + Vy(i) * bi(i)
                i = i + 1
            Loop Until i = n + 1

            'loop externo
            Do

                Dim AG = amv * P / (R * T) ^ 2
                Dim BG = bmv * P / (R * T)

                coeff(0) = -AG * BG + BG ^ 2 + BG ^ 3
                coeff(1) = AG - 3 * BG ^ 2 - 2 * BG
                coeff(2) = BG - 1
                coeff(3) = 1

                Dim temp1 = Poly_Roots(coeff)
                Dim tv = 0
                Dim tv2 = 0
                Try

                    If temp1(0, 0) > temp1(1, 0) Then
                        tv = temp1(1, 0)
                        temp1(1, 0) = temp1(0, 0)
                        temp1(0, 0) = tv
                        tv2 = temp1(1, 1)
                        temp1(1, 1) = temp1(0, 1)
                        temp1(0, 1) = tv2
                    End If
                    If temp1(0, 0) > temp1(2, 0) Then
                        tv = temp1(2, 0)
                        temp1(2, 0) = temp1(0, 0)
                        temp1(0, 0) = tv
                        tv2 = temp1(2, 1)
                        temp1(2, 1) = temp1(0, 1)
                        temp1(0, 1) = tv2
                    End If
                    If temp1(1, 0) > temp1(2, 0) Then
                        tv = temp1(2, 0)
                        temp1(2, 0) = temp1(1, 0)
                        temp1(1, 0) = tv
                        tv2 = temp1(2, 1)
                        temp1(2, 1) = temp1(1, 1)
                        temp1(1, 1) = tv2
                    End If

                    ZV = temp1(2, 0)
                    If temp1(2, 1) <> 0 Then
                        ZV = temp1(1, 0)
                        If temp1(1, 1) <> 0 Then
                            ZV = temp1(0, 0)
                        End If
                    End If

                Catch

                    Dim findZV
                    ZV = 1
                    Do
                        findZV = coeff(3) * ZV ^ 3 + coeff(2) * ZV ^ 2 + coeff(1) * ZV + coeff(0)
                        ZV -= 0.00001
                        If ZV < 0 Then ZV = 1
                    Loop Until Math.Abs(findZV) < 0.0001

                End Try

                Dim sum_ci As Double = 0
                For i = 0 To n
                    sum_ci += VCT(i)
                Next

                ZV = ZV - P * sum_ci / (R * T)

                'Dim betaV = 1 / P * (1 - (BG * ZV ^ 2 + AG * ZV - 6 * BG ^ 2 * ZV - 2 * BG * ZV - 2 * AG * BG + 2 * BG ^ 2 + 2 * BG) / (ZV * (3 * ZV ^ 2 - 2 * ZV + 2 * BG * ZV + AG - 3 * BG ^ 2 - 2 * BG)))
                'Dim res As Object

                'If betaV > 3 / (P / 101325) Or betaV < 0.9 / (P / 101325) Then
                '    res = Me.m_pr.GeneratePseudoRoot(T, P, Vy, VKij, VTc, VPc, Vw, VTb, "V")
                '    If Not res Is Nothing Then
                '        ZV = res(0)
                '        AG = res(1)
                '        BG = res(2)
                '        'amv = res(3)
                '        'bmv = res(4)
                '    End If
                'End If

                ' CALCULO DO COEFICIENTE DE FUGACIDADE DA FASE VAPOR

                i = 0
                Do
                    t1 = bi(i) * (ZV - 1) / bmv
                    t2 = -Math.Log(ZV - BG)
                    t3 = amv * (2 * amv2(i) / amv - bi(i) / bmv)
                    t4 = Math.Log(ZV + (1 + 2 ^ 0.5) * BG) - Math.Log(ZV + ((1 - 2 ^ 0.5) * BG))
                    t5 = 8 ^ 0.5 * bmv * R * T
                    LN_CFV(i) = t1 + t2 - (t3 * t4 / t5)
                    PHIV(i) = Math.Exp(LN_CFV(i)) * Vy(i) * P
                    i = i + 1
                Loop Until i = n + 1

                '' INICIO DO LOOP INTERNO
                Dim cont_int = 1
                Do

                    ' CALCULO DAS RAIZES PARA A FASE LIQUIDA

                    i = 0
                    Do
                        aml2(i) = 0
                        i = i + 1
                    Loop Until i = n + 1

                    i = 0
                    Dim aml = 0
                    Do
                        j = 0
                        Do
                            aml = aml + Vx(i) * Vx(j) * a(i, j)
                            aml2(i) = aml2(i) + Vx(j) * a(j, i)
                            j = j + 1
                        Loop Until j = n + 1
                        i = i + 1
                    Loop Until i = n + 1

                    i = 0
                    Dim bml = 0
                    Do
                        bml = bml + Vx(i) * bi(i)
                        i = i + 1
                    Loop Until i = n + 1

                    AG = aml * P / (R * T) ^ 2
                    BG = bml * P / (R * T)

                    coeff(0) = -AG * BG + BG ^ 2 + BG ^ 3
                    coeff(1) = AG - 3 * BG ^ 2 - 2 * BG
                    coeff(2) = BG - 1
                    coeff(3) = 1

                    temp1 = Poly_Roots(coeff)
                    tv = 0
                    tv2 = 0

                    Try

                        If temp1(0, 0) > temp1(1, 0) Then
                            tv = temp1(1, 0)
                            tv2 = temp1(1, 1)
                            temp1(1, 0) = temp1(0, 0)
                            temp1(0, 0) = tv
                            temp1(1, 1) = temp1(0, 1)
                            temp1(0, 1) = tv2
                        End If
                        If temp1(0, 0) > temp1(2, 0) Then
                            tv = temp1(2, 0)
                            temp1(2, 0) = temp1(0, 0)
                            temp1(0, 0) = tv
                            tv2 = temp1(2, 1)
                            temp1(2, 1) = temp1(0, 1)
                            temp1(0, 1) = tv2
                        End If
                        If temp1(1, 0) > temp1(2, 0) Then
                            tv = temp1(2, 0)
                            temp1(2, 0) = temp1(1, 0)
                            temp1(1, 0) = tv
                            tv2 = temp1(2, 1)
                            temp1(2, 1) = temp1(1, 1)
                            temp1(1, 1) = tv2
                        End If

                        ZL = temp1(0, 0)
                        If temp1(0, 1) <> 0 Then
                            ZL = temp1(1, 0)
                            If temp1(1, 1) <> 0 Then
                                ZL = temp1(2, 0)
                            End If
                        End If

                    Catch

                        Dim findZL
                        ZL = 0
                        Do
                            findZL = coeff(3) * ZL ^ 3 + coeff(2) * ZL ^ 2 + coeff(1) * ZL + coeff(0)
                            ZL += 0.00001
                            If ZL > 1 Then ZL = 0
                        Loop Until Math.Abs(findZL) < 0.0001

                    End Try

                    ZL = ZL - P * sum_ci / (R * T)

                    'Dim betaL = 1 / P * (1 - (BG * ZL ^ 2 + AG * ZL - 6 * BG ^ 2 * ZL - 2 * BG * ZL - 2 * AG * BG + 2 * BG ^ 2 + 2 * BG) / (ZL * (3 * ZL ^ 2 - 2 * ZL + 2 * BG * ZL + AG - 3 * BG ^ 2 - 2 * BG)))

                    'If betaL > 0.005 / 101325 Then
                    '    res = Me.m_pr.GeneratePseudoRoot(T, P, Vx, VKij, VTc, VPc, Vw, VTb, "L")
                    '    If Not res Is Nothing Then
                    '        ZL = res(0)
                    '        AG = res(1)
                    '        BG = res(2)
                    '        'amv = res(3)
                    '        'bmv = res(4)
                    '    End If
                    'End If

                    ' CALCULO DO COEFICIENTE DE FUGACIDADE DA FASE LIQUIDA

                    i = 0
                    Do
                        'If Tr(i) >= 1 Then
                        '    LN_CFL(i) = Math.Log(Vp(i))
                        '    PHIL(i) = Math.Exp(Vx(i) * LN_CFL(i))
                        'Else
                        t1 = bi(i) * (ZL - 1) / bml
                        t2 = -Math.Log(ZL - BG)
                        t3 = aml * (2 * aml2(i) / aml - bi(i) / bml)
                        t4 = Math.Log((ZL + (1 + 2 ^ 0.5) * BG) / (ZL + (1 - 2 ^ 0.5) * BG))
                        t5 = 2 * 2 ^ 0.5 * bml * R * T
                        LN_CFL(i) = t1 + t2 - (t3 * t4 / t5)
                        PHIL(i) = Math.Exp(LN_CFL(i)) * Vx(i) * P
                        'End If
                        i = i + 1
                    Loop Until i = n + 1

                    ' CALCULO DA COMPOSICAO DA FASE LIQUIDA

                    i = 0
                    Do
                        KI(i) = (Math.Exp(LN_CFL(i)) / (Math.Exp(LN_CFV(i))))
                        i = i + 1
                    Loop Until i = n + 1

                    marcador = 0
                    If stmp4_ant <> 0 Then
                        marcador = 1
                    End If
                    stmp4_ant = stmp4

                    i = 0
                    stmp4 = 0
                    Do
                        stmp4 = stmp4 + Vy(i) / KI(i)
                        i = i + 1
                    Loop Until i = n + 1

                    i = 0
                    Do
                        Vx(i) = (Vy(i) / KI(i)) / stmp4
                        i = i + 1
                    Loop Until i = n + 1

                    marcador2 = 0
                    If marcador = 1 Then
                        If Math.Abs(stmp4_ant - stmp4) < 0.001 Then
                            marcador2 = 1
                        End If
                    End If

                    ' FIM DO LOOP INTERNO
                    cont_int = cont_int + 1

                Loop Until marcador2 = 1 Or Double.IsNaN(stmp4)

                dKdP = DKDP_PR(T, P, Vx, Vy, VKij, VTc, VPc, VTb, Vw, RET_VC)

                F = stmp4 - 1

                cnt += 1

                i = 0
                dFdP = 0
                Do
                    dFdP = dFdP - Vy(i) / (KI(i) ^ 2) * dKdP(i)
                    i = i + 1
                Loop Until i = n + 1

                Pant = P
                P = P - F / dFdP
                'If P < 0 Then P = -P

                CheckCalculatorStatus()

            Loop Until Math.Abs(F) < 0.001 Or Double.IsNaN(P) = True Or cnt > 50

            Dim tmp2(n + 1)
            tmp2(0) = Pant
            i = 0
            Do
                tmp2(i + 1) = KI(i)
                i = i + 1
            Loop Until i = n + 1

            Return tmp2

        End Function

        Function BUBP_PR_M2(ByVal T As Double, ByVal Vx As Object, ByVal VKij As Object, ByVal VTc As Object, ByVal VPc As Object, ByVal VTb As Object, ByVal Vw As Object, ByVal KI As Object, ByVal VCT As Object, Optional ByVal P As Double = 0) As Object

            Dim dFdP As Double
            Dim cnt As Integer = 0

            'T,Tc em K
            'P,Pc em Pa
            Dim n = UBound(Vx)

            Dim chk As Boolean = False
            Dim marcador2 As Integer

            'Dim betaL, betaV As Double
            Dim F As Double
            Dim dKdP(n), dKdPi(n) As Object
            Dim Vy(n), Vx_ant(n), Vy_ant(n), PHIV(n), PHIL(n), LN_CFL(n), LN_CFV(n) As Double
            Dim ai(n), bi(n), aml2(n), amv2(n) As Double
            Dim Vp(n), R, coeff(3), tmp(2, 2) As Double
            Dim Tc(n), Pc(n), Vc(n), W(n), Zc(n), alpha(n), m(n), a(n, n), b(n, n), ZL, ZV, Tr(n) As Double
            Dim i, j
            Dim t1, t2, t3, t4, t5 As Double
            Dim soma_y As Double
            Dim marcador3, marcador
            Dim stmp4_ant, stmp4, Pant As Double
            stmp4_ant = 0
            stmp4 = 0
            Pant = 0

            R = 8.314

            i = 0
            Do
                Tc(i) = VTc(i)
                Tr(i) = T / Tc(i)
                Pc(i) = VPc(i)
                W(i) = Vw(i)
                Vp(i) = Me.AUX_PVAPi(i, T)
                i = i + 1
            Loop Until i = n + 1

            If P = 0 Then
                'generate first estimate for T
                i = 0
                Do
                    P = P + Vx(i) * Vp(i)
                    i = i + 1
                Loop Until i = n + 1
            End If

            Dim ai_(n)

            i = 0
            Do
                If T / Tc(i) <= 1 Then
                    alpha(i) = (1 + (0.37464 + 1.54226 * W(i) - 0.26992 * W(i) ^ 2) * (1 - (T / Tc(i)) ^ 0.5)) ^ 2
                Else
                    alpha(i) = (1 + 1.21 * (0.37464 + 1.54226 * W(i) - 0.26992 * W(i) ^ 2) * (1 - (T / Tc(i)) ^ 0.5)) ^ 2
                End If
                ai(i) = 0.45724 * alpha(i) * R ^ 2 * Tc(i) ^ 2 / Pc(i)
                ai_(i) = ai(i) ^ 0.5
                bi(i) = 0.0778 * R * Tc(i) / Pc(i)
                i = i + 1
            Loop Until i = n + 1

            i = 0
            Do
                j = 0
                Do
                    a(i, j) = (ai(i) * ai(j)) ^ 0.5 * (1 - VKij(i, j))
                    j = j + 1
                Loop Until j = n + 1
                i = i + 1
            Loop Until i = n + 1

            i = 0
            Do
                If KI(i) = 0 Then KI(i) = Vp(i) / P
                dKdPi(i) = -Vp(i) / P ^ 2
                Vy(i) = Vx(i) * KI(i)
                i = i + 1
            Loop Until i = n + 1

            i = 0
            soma_y = 0
            Do
                soma_y = soma_y + Vy(i)
                i = i + 1
            Loop Until i = n + 1

            i = 0
            Do
                Vy(i) = Vy(i) / soma_y
                i = i + 1
            Loop Until i = n + 1

            ' CALCULO DAS RAIZES PARA A FASE LIQUIDA

            i = 0
            Do
                aml2(i) = 0
                i = i + 1
            Loop Until i = n + 1

            i = 0
            Dim aml = 0
            Do
                j = 0
                Do
                    aml = aml + Vx(i) * Vx(j) * a(i, j)
                    aml2(i) = aml2(i) + Vx(j) * a(j, i)
                    j = j + 1
                Loop Until j = n + 1
                i = i + 1
            Loop Until i = n + 1

            i = 0
            Dim bml = 0
            Do
                bml = bml + Vx(i) * bi(i)
                i = i + 1
            Loop Until i = n + 1

            'loop externo
            Do

                Dim AG = aml * P / (R * T) ^ 2
                Dim BG = bml * P / (R * T)

                coeff(0) = -AG * BG + BG ^ 2 + BG ^ 3
                coeff(1) = AG - 3 * BG ^ 2 - 2 * BG
                coeff(2) = BG - 1
                coeff(3) = 1

                Dim temp1 = Poly_Roots(coeff)
                Dim tv = 0
                Dim tv2 = 0

                Try

                    If temp1(0, 0) > temp1(1, 0) Then
                        tv = temp1(1, 0)
                        tv2 = temp1(1, 1)
                        temp1(1, 0) = temp1(0, 0)
                        temp1(0, 0) = tv
                        temp1(1, 1) = temp1(0, 1)
                        temp1(0, 1) = tv2
                    End If
                    If temp1(0, 0) > temp1(2, 0) Then
                        tv = temp1(2, 0)
                        temp1(2, 0) = temp1(0, 0)
                        temp1(0, 0) = tv
                        tv2 = temp1(2, 1)
                        temp1(2, 1) = temp1(0, 1)
                        temp1(0, 1) = tv2
                    End If
                    If temp1(1, 0) > temp1(2, 0) Then
                        tv = temp1(2, 0)
                        temp1(2, 0) = temp1(1, 0)
                        temp1(1, 0) = tv
                        tv2 = temp1(2, 1)
                        temp1(2, 1) = temp1(1, 1)
                        temp1(1, 1) = tv2
                    End If

                    ZL = temp1(0, 0)
                    If temp1(0, 1) <> 0 Then
                        ZL = temp1(1, 0)
                        If temp1(1, 1) <> 0 Then
                            ZL = temp1(2, 0)
                        End If
                    End If

                    If ZL < 0 Then
                        ZL = temp1(0, 0)
                        If temp1(0, 0) < 0 Then
                            ZL = temp1(1, 0)
                            If temp1(1, 0) < 0 Then
                                ZL = temp1(2, 0)
                            End If
                        End If
                    End If

                Catch

                    Dim findZL
                    ZL = 0
                    Do
                        findZL = coeff(3) * ZL ^ 3 + coeff(2) * ZL ^ 2 + coeff(1) * ZL + coeff(0)
                        ZL += 0.000001
                        If ZL > 1 Then ZL = 0
                    Loop Until Math.Abs(findZL) < 0.0001

                End Try

                Dim sum_ci As Double = 0
                For i = 0 To n
                    sum_ci += Vc(i)
                Next

                ZL = ZL - P * sum_ci / (R * T)

                'betaL = 1 / P * (1 - (BG * ZL ^ 2 + AG * ZL - 6 * BG ^ 2 * ZL - 2 * BG * ZL - 2 * AG * BG + 2 * BG ^ 2 + 2 * BG) / (ZL * (3 * ZL ^ 2 - 2 * ZL + 2 * BG * ZL + AG - 3 * BG ^ 2 - 2 * BG)))

                'Dim res As Object
                'If betaL > 0.005 / 101325 Then
                '    res = Me.m_pr.GeneratePseudoRoot(T, P, Vx, VKij, VTc, VPc, Vw, VTb, "L")
                '    If Not res Is Nothing Then
                '        ZL = res(0)
                '        AG = res(1)
                '        BG = res(2)
                '        'amv = res(3)
                '        'bmv = res(4)
                '    End If
                'End If

                ' CALCULO DO COEFICIENTE DE FUGACIDADE DA FASE LIQUIDA

                i = 0
                Do
                    'If Tr(i) >= 1 Then
                    '    LN_CFL(i) = Math.Log(Vp(i))
                    '    PHIL(i) = Math.Exp(Vx(i) * LN_CFL(i))
                    'Else
                    t1 = bi(i) * (ZL - 1) / bml
                    t2 = -Math.Log(ZL - BG)
                    t3 = aml * (2 * aml2(i) / aml - bi(i) / bml)
                    t4 = Math.Log((ZL + (1 + 2 ^ 0.5) * BG) / (ZL + (1 - 2 ^ 0.5) * BG))
                    t5 = 2 * 2 ^ 0.5 * bml * R * T
                    LN_CFL(i) = t1 + t2 - (t3 * t4 / t5)
                    PHIL(i) = Math.Exp(LN_CFL(i)) * Vx(i) * P
                    'End If
                    i = i + 1
                Loop Until i = n + 1

                marcador3 = 0

                '' INICIO DO LOOP INTERNO
                Dim cont_int = 0
                Do

                    ' CALCULO DAS RAIZES PARA A FASE VAPOR

                    i = 0
                    Do
                        amv2(i) = 0
                        i = i + 1
                    Loop Until i = n + 1

                    i = 0
                    Dim amv = 0
                    Do
                        j = 0
                        Do
                            amv = amv + Vy(i) * Vy(j) * a(i, j)
                            amv2(i) = amv2(i) + Vy(j) * a(j, i)
                            j = j + 1
                        Loop Until j = n + 1
                        i = i + 1
                    Loop Until i = n + 1

                    i = 0
                    Dim bmv = 0
                    Do
                        bmv = bmv + Vy(i) * bi(i)
                        i = i + 1
                    Loop Until i = n + 1

                    AG = amv * P / (R * T) ^ 2
                    BG = bmv * P / (R * T)

                    coeff(0) = -AG * BG + BG ^ 2 + BG ^ 3
                    coeff(1) = AG - 3 * BG ^ 2 - 2 * BG
                    coeff(2) = BG - 1
                    coeff(3) = 1

                    temp1 = Poly_Roots(coeff)
                    tv = 0

                    Try

                        If temp1(0, 0) > temp1(1, 0) Then
                            tv = temp1(1, 0)
                            temp1(1, 0) = temp1(0, 0)
                            temp1(0, 0) = tv
                            tv2 = temp1(1, 1)
                            temp1(1, 1) = temp1(0, 1)
                            temp1(0, 1) = tv2
                        End If
                        If temp1(0, 0) > temp1(2, 0) Then
                            tv = temp1(2, 0)
                            temp1(2, 0) = temp1(0, 0)
                            temp1(0, 0) = tv
                            tv2 = temp1(2, 1)
                            temp1(2, 1) = temp1(0, 1)
                            temp1(0, 1) = tv2
                        End If
                        If temp1(1, 0) > temp1(2, 0) Then
                            tv = temp1(2, 0)
                            temp1(2, 0) = temp1(1, 0)
                            temp1(1, 0) = tv
                            tv2 = temp1(2, 1)
                            temp1(2, 1) = temp1(1, 1)
                            temp1(1, 1) = tv2
                        End If

                        ZV = temp1(2, 0)
                        If temp1(2, 1) <> 0 Then
                            ZV = temp1(1, 0)
                            If temp1(1, 1) <> 0 Then
                                ZV = temp1(0, 0)
                            End If
                        End If

                    Catch

                        Dim findZV
                        ZV = 1
                        Do
                            findZV = coeff(3) * ZV ^ 3 + coeff(2) * ZV ^ 2 + coeff(1) * ZV + coeff(0)
                            ZV -= 0.00001
                            If ZV < 0 Then ZV = 1
                        Loop Until Math.Abs(findZV) < 0.0001

                    End Try

                    ZV = ZV - P * sum_ci / (R * T)

                    'betaV = 1 / P * (1 - (BG * ZV ^ 2 + AG * ZV - 6 * BG ^ 2 * ZV - 2 * BG * ZV - 2 * AG * BG + 2 * BG ^ 2 + 2 * BG) / (ZV * (3 * ZV ^ 2 - 2 * ZV + 2 * BG * ZV + AG - 3 * BG ^ 2 - 2 * BG)))

                    'If betaV > 3 / (P / 101325) Or betaV < 0.9 / (P / 101325) Then
                    '    res = Me.m_pr.GeneratePseudoRoot(T, P, Vy, VKij, VTc, VPc, Vw, VTb, "V")
                    '    If Not res Is Nothing Then
                    '        ZV = res(0)
                    '        AG = res(1)
                    '        BG = res(2)
                    '        'amv = res(3)
                    '        'bmv = res(4)
                    '    End If
                    'End If

                    ' CALCULO DO COEFICIENTE DE FUGACIDADE DA FASE VAPOR

                    i = 0
                    Do
                        t1 = bi(i) * (ZV - 1) / bmv
                        t2 = -Math.Log(ZV - BG)
                        t3 = amv * (2 * amv2(i) / amv - bi(i) / bmv)
                        t4 = Math.Log(ZV + (1 + 2 ^ 0.5) * BG) - Math.Log(ZV + ((1 - 2 ^ 0.5) * BG))
                        t5 = 8 ^ 0.5 * bmv * R * T
                        LN_CFV(i) = t1 + t2 - (t3 * t4 / t5)
                        PHIV(i) = Math.Exp(LN_CFV(i)) * Vy(i) * P
                        i = i + 1
                    Loop Until i = n + 1

                    ' CALCULO DA COMPOSICAO DA FASE VAPOR

                    i = 0
                    Do
                        KI(i) = (Math.Exp(LN_CFL(i) - LN_CFV(i)))
                        i = i + 1
                    Loop Until i = n + 1

                    marcador = 0
                    If stmp4_ant <> 0 Then
                        marcador = 1
                    End If
                    stmp4_ant = stmp4

                    i = 0
                    stmp4 = 0
                    Do
                        stmp4 = stmp4 + KI(i) * Vx(i)
                        i = i + 1
                    Loop Until i = n + 1

                    i = 0
                    Do
                        Vy_ant(i) = Vy(i)
                        Vy(i) = KI(i) * Vx(i) / stmp4
                        i = i + 1
                    Loop Until i = n + 1

                    marcador2 = 0
                    If marcador = 1 Then
                        If Math.Abs(Vy(0) - Vy_ant(0)) < 0.001 Then
                            marcador2 = 1
                        End If
                    End If

                    ' FIM DO LOOP INTERNO
                    cont_int = cont_int + 1

                Loop Until marcador2 = 1 Or Double.IsNaN(Vy(0)) Or Double.IsNaN(stmp4) Or cont_int > 50

                dKdP = DKDP_PR(T, P, Vx, Vy, VKij, VTc, VPc, VTb, Vw, RET_VC)

                F = stmp4 - 1

                cnt += 1

                i = 0
                dFdP = 0
                Do
                    dFdP = dFdP + Vx(i) * dKdP(i)
                    i = i + 1
                Loop Until i = n + 1

                Pant = P
                P = P - F / dFdP

                CheckCalculatorStatus()

            Loop Until Math.Abs(P - Pant) / Pant < 0.001 Or Double.IsNaN(P) = True Or cnt > 50

            Dim tmp2(n + 1)
            tmp2(0) = Pant
            i = 0
            Do
                tmp2(i + 1) = KI(i)
                i = i + 1
            Loop Until i = n + 1

            Return tmp2

        End Function

        Function DEWT_PR_M2(ByVal P As Double, ByVal Vy As Object, ByVal VKij As Object, ByVal VTc As Object, ByVal VPc As Object, ByVal VTb As Object, ByVal Vw As Object, ByVal KI As Object, ByVal VCT As Object, Optional ByVal T As Double = 0) As Object

            Dim dFdT As Double
            Dim cnt As Integer = 0

            'T,Tc em K
            'P,Pc em Pa
            Dim n = UBound(Vy)

            Dim chk As Boolean = False
            Dim marcador2 As Integer

            Dim F As Double
            Dim dKdT(n), dKdTi(n) As Object
            Dim Vx(n), PHIV(n), PHIL(n), LN_CFL(n), LN_CFV(n) As Double
            Dim ai(n), bi(n), aml2(n), amv2(n) As Double
            Dim Vp(n), R, coeff(3) As Double
            Dim Tc(n), Pc(n), W(n), alpha(n), a(n, n), ZL, ZV, Tr(n) As Double
            Dim i, j
            Dim t1, t2, t3, t4, t5 As Double
            Dim soma_x As Double
            Dim marcador
            Dim stmp4_ant, stmp4, Tant As Double
            stmp4_ant = 0
            stmp4 = 0
            Tant = 0

            R = 8.314

            If T = 0 Then
                'generate first estimate for T
                i = 0
                T = 0
                Do
                    'T = T + 0.7 * Vy(i) * VTc(i)
                    T = T + Vy(i) * Me.AUX_TSATi(P, i)
                    i = i + 1
                Loop Until i = n + 1
            End If

            i = 0
            Do
                Tc(i) = VTc(i)
                Tr(i) = T / Tc(i)
                Pc(i) = VPc(i)
                W(i) = Vw(i)
                Vp(i) = Me.AUX_PVAPi(i, T)
                i = i + 1
            Loop Until i = n + 1

            i = 0
            Do
                KI(i) = VPc(i) / P * Exp(5.42 * (1 - VTc(i) / T))
                dKdTi(i) = 5.42 * KI(i) * Tc(i) / T ^ 2
                Vx(i) = Vy(i) / KI(i)
                i = i + 1
            Loop Until i = n + 1

            i = 0
            soma_x = 0
            Do
                soma_x = soma_x + Vx(i)
                i = i + 1
            Loop Until i = n + 1

            i = 0
            Do
                Vx(i) = Vx(i) / soma_x
                i = i + 1
            Loop Until i = n + 1

            'loop externo
            Do

                i = 0
                Do
                    Tr(i) = T / Tc(i)
                    Vp(i) = Me.AUX_PVAPi(i, T)
                    i = i + 1
                Loop Until i = n + 1

                Dim ai_(n)

                i = 0
                Do
                    alpha(i) = (1 + (0.37464 + 1.54226 * W(i) - 0.26992 * W(i) ^ 2) * (1 - (T / Tc(i)) ^ 0.5)) ^ 2
                    ai(i) = 0.45724 * alpha(i) * R ^ 2 * Tc(i) ^ 2 / Pc(i)
                    ai_(i) = ai(i) ^ 0.5
                    bi(i) = 0.0778 * R * Tc(i) / Pc(i)
                    i = i + 1
                Loop Until i = n + 1

                i = 0
                Do
                    j = 0
                    Do
                        a(i, j) = (ai(i) * ai(j)) ^ 0.5 * (1 - VKij(i, j))
                        j = j + 1
                    Loop Until j = n + 1
                    i = i + 1
                Loop Until i = n + 1

                ' CALCULO DAS RAIZES PARA A FASE VAPOR

                i = 0
                Do
                    amv2(i) = 0
                    i = i + 1
                Loop Until i = n + 1

                i = 0
                Dim amv = 0
                Do
                    j = 0
                    Do
                        amv = amv + Vy(i) * Vy(j) * a(i, j)
                        amv2(i) = amv2(i) + Vy(j) * a(j, i)
                        j = j + 1
                    Loop Until j = n + 1
                    i = i + 1
                Loop Until i = n + 1

                i = 0
                Dim bmv = 0
                Do
                    bmv = bmv + Vy(i) * bi(i)
                    i = i + 1
                Loop Until i = n + 1

                Dim AG = amv * P / (R * T) ^ 2
                Dim BG = bmv * P / (R * T)

                coeff(0) = -AG * BG + BG ^ 2 + BG ^ 3
                coeff(1) = AG - 3 * BG ^ 2 - 2 * BG
                coeff(2) = BG - 1
                coeff(3) = 1

                Dim temp1 = Poly_Roots(coeff)
                Dim tv = 0
                Dim tv2 = 0
                Try

                    If temp1(0, 0) > temp1(1, 0) Then
                        tv = temp1(1, 0)
                        temp1(1, 0) = temp1(0, 0)
                        temp1(0, 0) = tv
                        tv2 = temp1(1, 1)
                        temp1(1, 1) = temp1(0, 1)
                        temp1(0, 1) = tv2
                    End If
                    If temp1(0, 0) > temp1(2, 0) Then
                        tv = temp1(2, 0)
                        temp1(2, 0) = temp1(0, 0)
                        temp1(0, 0) = tv
                        tv2 = temp1(2, 1)
                        temp1(2, 1) = temp1(0, 1)
                        temp1(0, 1) = tv2
                    End If
                    If temp1(1, 0) > temp1(2, 0) Then
                        tv = temp1(2, 0)
                        temp1(2, 0) = temp1(1, 0)
                        temp1(1, 0) = tv
                        tv2 = temp1(2, 1)
                        temp1(2, 1) = temp1(1, 1)
                        temp1(1, 1) = tv2
                    End If

                    ZV = temp1(2, 0)
                    If temp1(2, 1) <> 0 Then
                        ZV = temp1(1, 0)
                        If temp1(1, 1) <> 0 Then
                            ZV = temp1(0, 0)
                        End If
                    End If

                Catch

                    Dim findZV
                    ZV = 1
                    Do
                        findZV = coeff(3) * ZV ^ 3 + coeff(2) * ZV ^ 2 + coeff(1) * ZV + coeff(0)
                        ZV -= 0.00001
                        If ZV < 0 Then ZV = 1
                    Loop Until Math.Abs(findZV) < 0.0001

                End Try

                Dim sum_ci As Double = 0
                For i = 0 To n
                    sum_ci += VCT(i)
                Next

                ZV = ZV - P * sum_ci / (R * T)

                'Dim betaV = 1 / P * (1 - (BG * ZV ^ 2 + AG * ZV - 6 * BG ^ 2 * ZV - 2 * BG * ZV - 2 * AG * BG + 2 * BG ^ 2 + 2 * BG) / (ZV * (3 * ZV ^ 2 - 2 * ZV + 2 * BG * ZV + AG - 3 * BG ^ 2 - 2 * BG)))
                'Dim res As Object

                'If betaV > 3 / (P / 101325) Or betaV < 0.9 / (P / 101325) Then
                '    res = Me.m_pr.GeneratePseudoRoot(T, P, Vy, VKij, VTc, VPc, Vw, VTb, "V")
                '    If Not res Is Nothing Then
                '        ZV = res(0)
                '        AG = res(1)
                '        BG = res(2)
                '        'amv = res(3)
                '        'bmv = res(4)
                '    End If
                'End If

                ' CALCULO DO COEFICIENTE DE FUGACIDADE DA FASE VAPOR

                i = 0
                Do
                    t1 = bi(i) * (ZV - 1) / bmv
                    t2 = -Math.Log(ZV - BG)
                    t3 = amv * (2 * amv2(i) / amv - bi(i) / bmv)
                    t4 = Math.Log(ZV + (1 + 2 ^ 0.5) * BG) - Math.Log(ZV + ((1 - 2 ^ 0.5) * BG))
                    t5 = 8 ^ 0.5 * bmv * R * T
                    LN_CFV(i) = t1 + t2 - (t3 * t4 / t5)
                    PHIV(i) = Math.Exp(LN_CFV(i)) * Vy(i) * P
                    i = i + 1
                Loop Until i = n + 1

                '' INICIO DO LOOP INTERNO
                Dim cont_int = 1
                Do

                    ' CALCULO DAS RAIZES PARA A FASE LIQUIDA

                    i = 0
                    Do
                        aml2(i) = 0
                        i = i + 1
                    Loop Until i = n + 1

                    i = 0
                    Dim aml = 0
                    Do
                        j = 0
                        Do
                            aml = aml + Vx(i) * Vx(j) * a(i, j)
                            aml2(i) = aml2(i) + Vx(j) * a(j, i)
                            j = j + 1
                        Loop Until j = n + 1
                        i = i + 1
                    Loop Until i = n + 1

                    i = 0
                    Dim bml = 0
                    Do
                        bml = bml + Vx(i) * bi(i)
                        i = i + 1
                    Loop Until i = n + 1

                    AG = aml * P / (R * T) ^ 2
                    BG = bml * P / (R * T)

                    coeff(0) = -AG * BG + BG ^ 2 + BG ^ 3
                    coeff(1) = AG - 3 * BG ^ 2 - 2 * BG
                    coeff(2) = BG - 1
                    coeff(3) = 1

                    temp1 = Poly_Roots(coeff)
                    tv = 0
                    tv2 = 0

                    Try

                        If temp1(0, 0) > temp1(1, 0) Then
                            tv = temp1(1, 0)
                            tv2 = temp1(1, 1)
                            temp1(1, 0) = temp1(0, 0)
                            temp1(0, 0) = tv
                            temp1(1, 1) = temp1(0, 1)
                            temp1(0, 1) = tv2
                        End If
                        If temp1(0, 0) > temp1(2, 0) Then
                            tv = temp1(2, 0)
                            temp1(2, 0) = temp1(0, 0)
                            temp1(0, 0) = tv
                            tv2 = temp1(2, 1)
                            temp1(2, 1) = temp1(0, 1)
                            temp1(0, 1) = tv2
                        End If
                        If temp1(1, 0) > temp1(2, 0) Then
                            tv = temp1(2, 0)
                            temp1(2, 0) = temp1(1, 0)
                            temp1(1, 0) = tv
                            tv2 = temp1(2, 1)
                            temp1(2, 1) = temp1(1, 1)
                            temp1(1, 1) = tv2
                        End If

                        ZL = temp1(0, 0)
                        If temp1(0, 1) <> 0 Then
                            ZL = temp1(1, 0)
                            If temp1(1, 1) <> 0 Then
                                ZL = temp1(2, 0)
                            End If
                        End If

                    Catch

                        Dim findZL
                        ZL = 0
                        Do
                            findZL = coeff(3) * ZL ^ 3 + coeff(2) * ZL ^ 2 + coeff(1) * ZL + coeff(0)
                            ZL += 0.00001
                            If ZL > 1 Then ZL = 0
                        Loop Until Math.Abs(findZL) < 0.0001

                    End Try

                    ZL = ZL - P * sum_ci / (R * T)

                    'Dim betaL = 1 / P * (1 - (BG * ZL ^ 2 + AG * ZL - 6 * BG ^ 2 * ZL - 2 * BG * ZL - 2 * AG * BG + 2 * BG ^ 2 + 2 * BG) / (ZL * (3 * ZL ^ 2 - 2 * ZL + 2 * BG * ZL + AG - 3 * BG ^ 2 - 2 * BG)))

                    'If betaL > 0.005 / 101325 Then
                    '    res = Me.m_pr.GeneratePseudoRoot(T, P, Vx, VKij, VTc, VPc, Vw, VTb, "L")
                    '    If Not res Is Nothing Then
                    '        ZL = res(0)
                    '        AG = res(1)
                    '        BG = res(2)
                    '        'amv = res(3)
                    '        'bmv = res(4)
                    '    End If
                    'End If

                    ' CALCULO DO COEFICIENTE DE FUGACIDADE DA FASE LIQUIDA

                    i = 0
                    Do
                        'If Tr(i) >= 1 Then
                        '    LN_CFL(i) = Math.Log(Vp(i))
                        '    PHIL(i) = Math.Exp(Vx(i) * LN_CFL(i))
                        'Else
                        t1 = bi(i) * (ZL - 1) / bml
                        t2 = -Math.Log(ZL - BG)
                        t3 = aml * (2 * aml2(i) / aml - bi(i) / bml)
                        t4 = Math.Log((ZL + (1 + 2 ^ 0.5) * BG) / (ZL + (1 - 2 ^ 0.5) * BG))
                        t5 = 2 * 2 ^ 0.5 * bml * R * T
                        LN_CFL(i) = t1 + t2 - (t3 * t4 / t5)
                        PHIL(i) = Math.Exp(LN_CFL(i)) * Vx(i) * P
                        'End If
                        i = i + 1
                    Loop Until i = n + 1

                    ' CALCULO DA COMPOSICAO DA FASE LIQUIDA

                    i = 0
                    Do
                        KI(i) = (Math.Exp(LN_CFL(i)) / (Math.Exp(LN_CFV(i))))
                        i = i + 1
                    Loop Until i = n + 1

                    marcador = 0
                    If stmp4_ant <> 0 Then
                        marcador = 1
                    End If
                    stmp4_ant = stmp4

                    i = 0
                    stmp4 = 0
                    Do
                        stmp4 = stmp4 + Vy(i) / KI(i)
                        i = i + 1
                    Loop Until i = n + 1

                    i = 0
                    Do
                        Vx(i) = (Vy(i) / KI(i)) / stmp4
                        i = i + 1
                    Loop Until i = n + 1

                    marcador2 = 0
                    If marcador = 1 Then
                        If Math.Abs(stmp4_ant - stmp4) < 0.001 Then
                            marcador2 = 1
                        End If
                    End If

                    ' FIM DO LOOP INTERNO
                    cont_int = cont_int + 1

                Loop Until marcador2 = 1 Or Double.IsNaN(stmp4)

                dKdT = DKDT_PR(T, P, Vx, Vy, VKij, VTc, VPc, VTb, Vw, RET_VC)

                F = stmp4 - 1

                i = 0
                dFdT = 0
                Do
                    dFdT = dFdT - Vy(i) / (KI(i) ^ 2) * dKdT(i)
                    i = i + 1
                Loop Until i = n + 1

                Tant = T
                T = T - F / dFdT

                cnt += 1

                CheckCalculatorStatus()

            Loop Until Math.Abs(F) < 0.001 Or Double.IsNaN(T) = True

            'check for trivial solution

            Dim sumk As Double = 0
            i = 0
            Do
                sumk += KI(i) / n
                i = i + 1
            Loop Until i = n + 1

            If Abs(sumk - 1) < 0.1 Then

                i = 0
                T = 0
                Do
                    'T = T + 0.7 * Vy(i) * VTc(i)
                    T = T + Vy(i) * Me.AUX_TSATi(P, i)
                    i = i + 1
                Loop Until i = n + 1

            End If

            Dim tmp2(n + 1)
            tmp2(0) = T
            i = 0
            Do
                tmp2(i + 1) = KI(i)
                i = i + 1
            Loop Until i = n + 1

            Return tmp2

        End Function

        Function BUBT_PR_M2(ByVal P As Double, ByVal Vx As Object, ByVal VKij As Object, ByVal VTc As Object, ByVal VPc As Object, ByVal VTb As Object, ByVal Vw As Object, ByVal KI As Object, ByVal VCT As Object, Optional ByVal T As Double = 0) As Object

            Dim dFdT As Double
            Dim cnt As Integer = 0

            'T,Tc em K
            'P,Pc em Pa
            Dim n = UBound(Vx)

            Dim chk As Boolean = False
            Dim marcador2 As Integer

            'Dim betaL, betaV As Double
            Dim F As Double
            Dim dKdT(n) As Object
            Dim dKdTi(n) As Object
            Dim Vy(n), Vx_ant(n), Vy_ant(n), PHIV(n), PHIL(n), LN_CFL(n), LN_CFV(n) As Double
            Dim ai(n), bi(n), aml2(n), amv2(n) As Double
            Dim Vp(n), R, coeff(3), tmp(2, 2) As Double
            Dim Pc(n), Vc(n), W(n), Zc(n), alpha(n), m(n), a(n, n), b(n, n), ZL, ZV, Tr(n) As Double
            Dim i, j
            Dim t1, t2, t3, t4, t5 As Double
            Dim soma_y As Double
            Dim marcador3, marcador
            Dim stmp4_ant, stmp4, Tant As Double
            stmp4_ant = 0
            stmp4 = 0
            Tant = 0

            R = 8.314

            If T = 0 Then
                'generate first estimate for T
                i = 0
                Do
                    If T <= VTc(i) Then T = T + Vx(i) * Me.AUX_TSATi(P, i)
                    i = i + 1
                Loop Until i = n + 1
            End If

            i = 0
            Do
                Tr(i) = T / VTc(i)
                Pc(i) = VPc(i)
                W(i) = Vw(i)
                Vp(i) = Me.AUX_PVAPi(i, T)
                i = i + 1
            Loop Until i = n + 1

            i = 0
            Do
                If KI(i) = 0 Then KI(i) = Vp(i) / P
                dKdTi(i) = 5.42 * KI(i) * VTc(i) / T ^ 2
                Vy(i) = Vx(i) * KI(i)
                i = i + 1
            Loop Until i = n + 1

            i = 0
            soma_y = 0
            Do
                soma_y = soma_y + Vy(i)
                i = i + 1
            Loop Until i = n + 1

            i = 0
            Do
                Vy(i) = Vy(i) / soma_y
                i = i + 1
            Loop Until i = n + 1

            'loop externo
            Do

                i = 0
                Do
                    Tr(i) = T / VTc(i)
                    Vp(i) = Me.AUX_PVAPi(i, T)
                    i = i + 1
                Loop Until i = n + 1

                Dim ai_(n)

                i = 0
                Do
                    alpha(i) = (1 + (0.37464 + 1.54226 * W(i) - 0.26992 * W(i) ^ 2) * (1 - (T / VTc(i)) ^ 0.5)) ^ 2
                    ai(i) = 0.45724 * alpha(i) * R ^ 2 * VTc(i) ^ 2 / Pc(i)
                    ai_(i) = ai(i) ^ 0.5
                    bi(i) = 0.0778 * R * VTc(i) / Pc(i)
                    i = i + 1
                Loop Until i = n + 1

                i = 0
                Do
                    j = 0
                    Do
                        a(i, j) = (ai(i) * ai(j)) ^ 0.5 * (1 - VKij(i, j))
                        j = j + 1
                    Loop Until j = n + 1
                    i = i + 1
                Loop Until i = n + 1

                ' CALCULO DAS RAIZES PARA A FASE LIQUIDA

                i = 0
                Do
                    aml2(i) = 0
                    i = i + 1
                Loop Until i = n + 1

                i = 0
                Dim aml = 0
                Do
                    j = 0
                    Do
                        aml = aml + Vx(i) * Vx(j) * a(i, j)
                        aml2(i) = aml2(i) + Vx(j) * a(j, i)
                        j = j + 1
                    Loop Until j = n + 1
                    i = i + 1
                Loop Until i = n + 1

                i = 0
                Dim bml = 0
                Do
                    bml = bml + Vx(i) * bi(i)
                    i = i + 1
                Loop Until i = n + 1

                Dim AG = aml * P / (R * T) ^ 2
                Dim BG = bml * P / (R * T)

                coeff(0) = -AG * BG + BG ^ 2 + BG ^ 3
                coeff(1) = AG - 3 * BG ^ 2 - 2 * BG
                coeff(2) = BG - 1
                coeff(3) = 1

                Dim temp1 = Poly_Roots(coeff)
                Dim tv = 0
                Dim tv2 = 0

                Try

                    If temp1(0, 0) > temp1(1, 0) Then
                        tv = temp1(1, 0)
                        tv2 = temp1(1, 1)
                        temp1(1, 0) = temp1(0, 0)
                        temp1(0, 0) = tv
                        temp1(1, 1) = temp1(0, 1)
                        temp1(0, 1) = tv2
                    End If
                    If temp1(0, 0) > temp1(2, 0) Then
                        tv = temp1(2, 0)
                        temp1(2, 0) = temp1(0, 0)
                        temp1(0, 0) = tv
                        tv2 = temp1(2, 1)
                        temp1(2, 1) = temp1(0, 1)
                        temp1(0, 1) = tv2
                    End If
                    If temp1(1, 0) > temp1(2, 0) Then
                        tv = temp1(2, 0)
                        temp1(2, 0) = temp1(1, 0)
                        temp1(1, 0) = tv
                        tv2 = temp1(2, 1)
                        temp1(2, 1) = temp1(1, 1)
                        temp1(1, 1) = tv2
                    End If

                    ZL = temp1(0, 0)
                    If temp1(0, 1) <> 0 Then
                        ZL = temp1(1, 0)
                        If temp1(1, 1) <> 0 Then
                            ZL = temp1(2, 0)
                        End If
                    End If

                Catch

                    Dim findZL
                    ZL = 0
                    Do
                        findZL = coeff(3) * ZL ^ 3 + coeff(2) * ZL ^ 2 + coeff(1) * ZL + coeff(0)
                        ZL += 0.000001
                        If ZL > 1 Then ZL = 0
                    Loop Until Math.Abs(findZL) < 0.0001

                End Try

                Dim sum_ci As Double = 0
                For i = 0 To n
                    sum_ci += Vc(i)
                Next

                ZL = ZL - P * sum_ci / (R * T)

                'betaL = 1 / P * (1 - (BG * ZL ^ 2 + AG * ZL - 6 * BG ^ 2 * ZL - 2 * BG * ZL - 2 * AG * BG + 2 * BG ^ 2 + 2 * BG) / (ZL * (3 * ZL ^ 2 - 2 * ZL + 2 * BG * ZL + AG - 3 * BG ^ 2 - 2 * BG)))

                'Dim res As Object
                'If betaL > 0.005 / 101325 Then
                '    res = Me.m_pr.GeneratePseudoRoot(T, P, Vx, VKij, VTc, VPc, Vw, VTb, "L")
                '    If Not res Is Nothing Then
                '        ZL = res(0)
                '        AG = res(1)
                '        BG = res(2)
                '        'amv = res(3)
                '        'bmv = res(4)
                '    End If
                'End If

                ' CALCULO DO COEFICIENTE DE FUGACIDADE DA FASE LIQUIDA

                i = 0
                Do
                    'If Tr(i) >= 1 Then
                    '    LN_CFL(i) = Math.Log(Vp(i))
                    '    PHIL(i) = Math.Exp(Vx(i) * LN_CFL(i))
                    'Else
                    t1 = bi(i) * (ZL - 1) / bml
                    t2 = -Math.Log(ZL - BG)
                    t3 = aml * (2 * aml2(i) / aml - bi(i) / bml)
                    t4 = Math.Log((ZL + (1 + 2 ^ 0.5) * BG) / (ZL + (1 - 2 ^ 0.5) * BG))
                    t5 = 2 * 2 ^ 0.5 * bml * R * T
                    LN_CFL(i) = t1 + t2 - (t3 * t4 / t5)
                    PHIL(i) = Math.Exp(LN_CFL(i)) * Vx(i) * P
                    'End If
                    i = i + 1
                Loop Until i = n + 1

                marcador3 = 0

                '' INICIO DO LOOP INTERNO
                Dim cont_int = 1
                Do

                    ' CALCULO DAS RAIZES PARA A FASE VAPOR

                    i = 0
                    Do
                        amv2(i) = 0
                        i = i + 1
                    Loop Until i = n + 1

                    i = 0
                    Dim amv = 0
                    Do
                        j = 0
                        Do
                            amv = amv + Vy(i) * Vy(j) * a(i, j)
                            amv2(i) = amv2(i) + Vy(j) * a(j, i)
                            j = j + 1
                        Loop Until j = n + 1
                        i = i + 1
                    Loop Until i = n + 1

                    i = 0
                    Dim bmv = 0
                    Do
                        bmv = bmv + Vy(i) * bi(i)
                        i = i + 1
                    Loop Until i = n + 1

                    AG = amv * P / (R * T) ^ 2
                    BG = bmv * P / (R * T)

                    coeff(0) = -AG * BG + BG ^ 2 + BG ^ 3
                    coeff(1) = AG - 3 * BG ^ 2 - 2 * BG
                    coeff(2) = BG - 1
                    coeff(3) = 1

                    temp1 = Poly_Roots(coeff)
                    tv = 0

                    Try

                        If temp1(0, 0) > temp1(1, 0) Then
                            tv = temp1(1, 0)
                            temp1(1, 0) = temp1(0, 0)
                            temp1(0, 0) = tv
                            tv2 = temp1(1, 1)
                            temp1(1, 1) = temp1(0, 1)
                            temp1(0, 1) = tv2
                        End If
                        If temp1(0, 0) > temp1(2, 0) Then
                            tv = temp1(2, 0)
                            temp1(2, 0) = temp1(0, 0)
                            temp1(0, 0) = tv
                            tv2 = temp1(2, 1)
                            temp1(2, 1) = temp1(0, 1)
                            temp1(0, 1) = tv2
                        End If
                        If temp1(1, 0) > temp1(2, 0) Then
                            tv = temp1(2, 0)
                            temp1(2, 0) = temp1(1, 0)
                            temp1(1, 0) = tv
                            tv2 = temp1(2, 1)
                            temp1(2, 1) = temp1(1, 1)
                            temp1(1, 1) = tv2
                        End If

                        ZV = temp1(2, 0)
                        If temp1(2, 1) <> 0 Then
                            ZV = temp1(1, 0)
                            If temp1(1, 1) <> 0 Then
                                ZV = temp1(0, 0)
                            End If
                        End If

                    Catch

                        Dim findZV
                        ZV = 1
                        Do
                            findZV = coeff(3) * ZV ^ 3 + coeff(2) * ZV ^ 2 + coeff(1) * ZV + coeff(0)
                            ZV -= 0.00001
                            If ZV < 0 Then ZV = 1
                        Loop Until Math.Abs(findZV) < 0.0001

                    End Try

                    ZV = ZV - P * sum_ci / (R * T)

                    'betaV = 1 / P * (1 - (BG * ZV ^ 2 + AG * ZV - 6 * BG ^ 2 * ZV - 2 * BG * ZV - 2 * AG * BG + 2 * BG ^ 2 + 2 * BG) / (ZV * (3 * ZV ^ 2 - 2 * ZV + 2 * BG * ZV + AG - 3 * BG ^ 2 - 2 * BG)))

                    'If betaV > 3 / (P / 101325) Or betaV < 0.9 / (P / 101325) Then
                    '    res = Me.m_pr.GeneratePseudoRoot(T, P, Vy, VKij, VTc, VPc, Vw, VTb, "V")
                    '    If Not res Is Nothing Then
                    '        ZV = res(0)
                    '        AG = res(1)
                    '        BG = res(2)
                    '        'amv = res(3)
                    '        'bmv = res(4)
                    '    End If
                    'End If

                    ' CALCULO DO COEFICIENTE DE FUGACIDADE DA FASE VAPOR

                    i = 0
                    Do
                        t1 = bi(i) * (ZV - 1) / bmv
                        t2 = -Math.Log(ZV - BG)
                        t3 = amv * (2 * amv2(i) / amv - bi(i) / bmv)
                        t4 = Math.Log(ZV + (1 + 2 ^ 0.5) * BG) - Math.Log(ZV + ((1 - 2 ^ 0.5) * BG))
                        t5 = 8 ^ 0.5 * bmv * R * T
                        LN_CFV(i) = t1 + t2 - (t3 * t4 / t5)
                        PHIV(i) = Math.Exp(LN_CFV(i)) * Vy(i) * P
                        i = i + 1
                    Loop Until i = n + 1

                    ' CALCULO DA COMPOSICAO DA FASE VAPOR

                    i = 0
                    Do
                        KI(i) = (Math.Exp(LN_CFL(i)) / (Math.Exp(LN_CFV(i))))
                        i = i + 1
                    Loop Until i = n + 1

                    marcador = 0
                    If stmp4_ant <> 0 Then
                        marcador = 1
                    End If
                    stmp4_ant = stmp4

                    i = 0
                    stmp4 = 0
                    Do
                        stmp4 = stmp4 + KI(i) * Vx(i)
                        i = i + 1
                    Loop Until i = n + 1

                    i = 0
                    Do
                        Vy(i) = KI(i) * Vx(i) / stmp4
                        i = i + 1
                    Loop Until i = n + 1

                    marcador2 = 0
                    If marcador = 1 Then
                        If Math.Abs(stmp4_ant - stmp4) < 0.001 Then
                            marcador2 = 1
                        End If
                    End If

                    cont_int += 1

                Loop Until marcador2 = 1 Or Double.IsNaN(stmp4)

                dKdT = DKDT_PR(T, P, Vx, Vy, VKij, VTc, VPc, VTb, Vw, RET_VC)

                F = stmp4 - 1

                i = 0
                dFdT = 0
                Do
                    dFdT = dFdT + Vx(i) * dKdT(i)
                    i = i + 1
                Loop Until i = n + 1

                Tant = T
                T = T - F / dFdT

                cnt += 1

                CheckCalculatorStatus()

            Loop Until Math.Abs(F) < 0.001 Or Double.IsNaN(T) = True

            'check for trivial solution

            Dim sumk As Double = 0
            i = 0
            Do
                sumk += KI(i) / n
                i = i + 1
            Loop Until i = n + 1

            If Abs(sumk - 1) < 0.1 Then

                i = 0
                T = 0
                Do
                    'T = T + 0.7 * Vy(i) * VTc(i)
                    T = T + Vx(i) * Me.AUX_TSATi(P, i)
                    i = i + 1
                Loop Until i = n + 1

            End If

            Dim tmp2(n + 1)
            tmp2(0) = Tant
            i = 0
            Do
                tmp2(i + 1) = KI(i)
                i = i + 1
            Loop Until i = n + 1

            Return tmp2

        End Function

        Function FLASH_TV(ByVal T As Double, ByVal V As Double, ByVal Vz As Object, ByVal VKij As Object, ByVal VTc As Object, ByVal VPc As Object, ByVal VTb As Object, ByVal Vw As Object, ByVal KI As Object, ByVal VCT As Object, Optional ByVal P As Double = 0) As Object

            Dim dFdP As Double
            Dim cnt As Integer = 0

            'T,Tc em K
            'P,Pc em Pa
            Dim n = UBound(Vz)

            Dim chk As Boolean = False

            'Dim betaL, betaV As Double
            Dim F As Double
            Dim dKdP(n), dKdPi(n) As Object
            Dim Vx(n), Vy(n), Vx_ant(n), Vy_ant(n), KI_ant(n), PHIV(n), PHIL(n), LN_CFL(n), LN_CFV(n) As Double
            Dim ai(n), bi(n), aml2(n), amv2(n) As Double
            Dim Vp(n), R, coeff(3), tmp(2, 2) As Double
            Dim Tc(n), Pc(n), Vc(n), W(n), Zc(n), alpha(n), m(n), a(n, n), b(n, n), ZL, ZV, Tr(n) As Double
            Dim i, j
            Dim t1, t2, t3, t4, t5 As Double
            Dim marcador3
            Dim stmp4_ant, stmp4, Pant As Double
            stmp4_ant = 0
            stmp4 = 0
            Pant = 0

            R = 8.314

            i = 0
            Do
                Tc(i) = VTc(i)
                Tr(i) = T / Tc(i)
                Pc(i) = VPc(i)
                W(i) = Vw(i)
                Vp(i) = Me.AUX_PVAPi(i, T)
                i = i + 1
            Loop Until i = n + 1

            If Double.IsNaN(P) Or P = 0 Then

                'Estimar P
                Dim Pbol, Porv As Double

                Pbol = Me.BUBP_PR_M2(T, Vz, VKij, VTc, VPc, VTb, Vw, KI_ant, RET_VC)(0)
                Porv = Me.DEWP_PR_M2(T, Vz, VKij, VTc, VPc, VTb, Vw, KI_ant, RET_VC)(0)

                P = Pbol - V * (Pbol - Porv)

            End If

            'Calcular Ki`s

            i = 0
            Do
                If Vz(i) <> 0 Then
                    If KI(i) = 0 Or Double.IsNaN(KI(i)) Then KI(i) = Vp(i) / P
                    KI_ant(i) = 0
                ElseIf Vz(i) = 0 Then
                    KI(i) = 0
                    KI_ant(i) = 0
                End If
                i += 1
            Loop Until i = n + 1

            If Double.IsNaN(P) Then
                Dim tmp0(n + 1)
                tmp0(0) = Double.NaN
                i = 0
                Do
                    tmp0(i + 1) = KI(i)
                    i = i + 1
                Loop Until i = n + 1
                Return tmp0
            End If
            Dim L = 1 - V

            i = 0
            Do
                If Vz(i) <> 0 Then
                    Vy(i) = Vz(i) * KI(i) / ((KI(i) - 1) * V + 1)
                    Vx(i) = Vy(i) / KI(i)
                Else
                    Vy(i) = 0
                    Vx(i) = 0
                End If
                i += 1
            Loop Until i = n + 1
            i = 0
            Dim soma_x = 0
            Dim soma_y = 0
            Do
                soma_x = soma_x + Vx(i)
                soma_y = soma_y + Vy(i)
                i = i + 1
            Loop Until i = n + 1
            i = 0
            Do
                Vx(i) = Vx(i) / soma_x
                Vy(i) = Vy(i) / soma_y
                i = i + 1
            Loop Until i = n + 1

            Dim ai_(n)

            i = 0
            Do
                If T / Tc(i) <= 1 Then
                    alpha(i) = (1 + (0.37464 + 1.54226 * W(i) - 0.26992 * W(i) ^ 2) * (1 - (T / Tc(i)) ^ 0.5)) ^ 2
                Else
                    alpha(i) = (1 + 1.21 * (0.37464 + 1.54226 * W(i) - 0.26992 * W(i) ^ 2) * (1 - (T / Tc(i)) ^ 0.5)) ^ 2
                End If
                ai(i) = 0.45724 * alpha(i) * R ^ 2 * Tc(i) ^ 2 / Pc(i)
                ai_(i) = ai(i) ^ 0.5
                bi(i) = 0.0778 * R * Tc(i) / Pc(i)
                i = i + 1
            Loop Until i = n + 1

            i = 0
            Do
                j = 0
                Do
                    a(i, j) = (ai(i) * ai(j)) ^ 0.5 * (1 - VKij(i, j))
                    j = j + 1
                Loop Until j = n + 1
                i = i + 1
            Loop Until i = n + 1

            ' CALCULO DAS RAIZES PARA A FASE LIQUIDA

            i = 0
            Do
                aml2(i) = 0
                i = i + 1
            Loop Until i = n + 1

            i = 0
            Dim aml = 0
            Do
                j = 0
                Do
                    aml = aml + Vx(i) * Vx(j) * a(i, j)
                    aml2(i) = aml2(i) + Vx(j) * a(j, i)
                    j = j + 1
                Loop Until j = n + 1
                i = i + 1
            Loop Until i = n + 1

            i = 0
            Dim bml = 0
            Do
                bml = bml + Vx(i) * bi(i)
                i = i + 1
            Loop Until i = n + 1

            'loop externo
            Do

                Dim AG = aml * P / (R * T) ^ 2
                Dim BG = bml * P / (R * T)

                coeff(0) = -AG * BG + BG ^ 2 + BG ^ 3
                coeff(1) = AG - 3 * BG ^ 2 - 2 * BG
                coeff(2) = BG - 1
                coeff(3) = 1

                Dim temp1 = Poly_Roots(coeff)
                Dim tv = 0
                Dim tv2 = 0

                Try

                    If temp1(0, 0) > temp1(1, 0) Then
                        tv = temp1(1, 0)
                        tv2 = temp1(1, 1)
                        temp1(1, 0) = temp1(0, 0)
                        temp1(0, 0) = tv
                        temp1(1, 1) = temp1(0, 1)
                        temp1(0, 1) = tv2
                    End If
                    If temp1(0, 0) > temp1(2, 0) Then
                        tv = temp1(2, 0)
                        temp1(2, 0) = temp1(0, 0)
                        temp1(0, 0) = tv
                        tv2 = temp1(2, 1)
                        temp1(2, 1) = temp1(0, 1)
                        temp1(0, 1) = tv2
                    End If
                    If temp1(1, 0) > temp1(2, 0) Then
                        tv = temp1(2, 0)
                        temp1(2, 0) = temp1(1, 0)
                        temp1(1, 0) = tv
                        tv2 = temp1(2, 1)
                        temp1(2, 1) = temp1(1, 1)
                        temp1(1, 1) = tv2
                    End If

                    ZL = temp1(0, 0)
                    If temp1(0, 1) <> 0 Then
                        ZL = temp1(1, 0)
                        If temp1(1, 1) <> 0 Then
                            ZL = temp1(2, 0)
                        End If
                    End If

                    If ZL < 0 Then
                        ZL = temp1(0, 0)
                        If temp1(0, 0) < 0 Then
                            ZL = temp1(1, 0)
                            If temp1(1, 0) < 0 Then
                                ZL = temp1(2, 0)
                            End If
                        End If
                    End If

                Catch

                    Dim findZL
                    ZL = 0
                    Do
                        findZL = coeff(3) * ZL ^ 3 + coeff(2) * ZL ^ 2 + coeff(1) * ZL + coeff(0)
                        ZL += 0.000001
                        If ZL > 1 Then ZL = 0
                    Loop Until Math.Abs(findZL) < 0.0001

                End Try

                Dim sum_ci As Double = 0
                For i = 0 To n
                    sum_ci += VCT(i)
                Next

                ' CALCULO DO COEFICIENTE DE FUGACIDADE DA FASE LIQUIDA

                i = 0
                Do
                    t1 = bi(i) * (ZL - 1) / bml
                    t2 = -Math.Log(ZL - BG)
                    t3 = aml * (2 * aml2(i) / aml - bi(i) / bml)
                    t4 = Math.Log((ZL + (1 + 2 ^ 0.5) * BG) / (ZL + (1 - 2 ^ 0.5) * BG))
                    t5 = 2 * 2 ^ 0.5 * bml * R * T
                    LN_CFL(i) = t1 + t2 - (t3 * t4 / t5)
                    PHIL(i) = Math.Exp(LN_CFL(i)) * Vx(i) * P
                    i = i + 1
                Loop Until i = n + 1

                marcador3 = 0

                ' CALCULO DAS RAIZES PARA A FASE VAPOR

                i = 0
                Do
                    amv2(i) = 0
                    i = i + 1
                Loop Until i = n + 1

                i = 0
                Dim amv = 0
                Do
                    j = 0
                    Do
                        amv = amv + Vy(i) * Vy(j) * a(i, j)
                        amv2(i) = amv2(i) + Vy(j) * a(j, i)
                        j = j + 1
                    Loop Until j = n + 1
                    i = i + 1
                Loop Until i = n + 1

                i = 0
                Dim bmv = 0
                Do
                    bmv = bmv + Vy(i) * bi(i)
                    i = i + 1
                Loop Until i = n + 1

                AG = amv * P / (R * T) ^ 2
                BG = bmv * P / (R * T)

                coeff(0) = -AG * BG + BG ^ 2 + BG ^ 3
                coeff(1) = AG - 3 * BG ^ 2 - 2 * BG
                coeff(2) = BG - 1
                coeff(3) = 1

                temp1 = Poly_Roots(coeff)
                tv = 0

                Try

                    If temp1(0, 0) > temp1(1, 0) Then
                        tv = temp1(1, 0)
                        temp1(1, 0) = temp1(0, 0)
                        temp1(0, 0) = tv
                        tv2 = temp1(1, 1)
                        temp1(1, 1) = temp1(0, 1)
                        temp1(0, 1) = tv2
                    End If
                    If temp1(0, 0) > temp1(2, 0) Then
                        tv = temp1(2, 0)
                        temp1(2, 0) = temp1(0, 0)
                        temp1(0, 0) = tv
                        tv2 = temp1(2, 1)
                        temp1(2, 1) = temp1(0, 1)
                        temp1(0, 1) = tv2
                    End If
                    If temp1(1, 0) > temp1(2, 0) Then
                        tv = temp1(2, 0)
                        temp1(2, 0) = temp1(1, 0)
                        temp1(1, 0) = tv
                        tv2 = temp1(2, 1)
                        temp1(2, 1) = temp1(1, 1)
                        temp1(1, 1) = tv2
                    End If

                    ZV = temp1(2, 0)
                    If temp1(2, 1) <> 0 Then
                        ZV = temp1(1, 0)
                        If temp1(1, 1) <> 0 Then
                            ZV = temp1(0, 0)
                        End If
                    End If

                Catch

                    Dim findZV
                    ZV = 1
                    Do
                        findZV = coeff(3) * ZV ^ 3 + coeff(2) * ZV ^ 2 + coeff(1) * ZV + coeff(0)
                        ZV -= 0.00001
                        If ZV < 0 Then ZV = 1
                    Loop Until Math.Abs(findZV) < 0.0001

                End Try


                ' CALCULO DO COEFICIENTE DE FUGACIDADE DA FASE VAPOR

                i = 0
                Do
                    t1 = bi(i) * (ZV - 1) / bmv
                    t2 = -Math.Log(ZV - BG)
                    t3 = amv * (2 * amv2(i) / amv - bi(i) / bmv)
                    t4 = Math.Log(ZV + (1 + 2 ^ 0.5) * BG) - Math.Log(ZV + ((1 - 2 ^ 0.5) * BG))
                    t5 = 8 ^ 0.5 * bmv * R * T
                    LN_CFV(i) = t1 + t2 - (t3 * t4 / t5)
                    PHIV(i) = Math.Exp(LN_CFV(i)) * Vy(i) * P
                    i = i + 1
                Loop Until i = n + 1

                ' CALCULO DA COMPOSICAO DA FASE VAPOR

                i = 0
                Do
                    KI(i) = (Math.Exp(LN_CFL(i) - LN_CFV(i)))
                    i = i + 1
                Loop Until i = n + 1

                i = 0
                Do
                    If Vz(i) <> 0 Then
                        Vy_ant(i) = Vy(i)
                        Vx_ant(i) = Vx(i)
                        Vy(i) = Vz(i) * KI(i) / ((KI(i) - 1) * V + 1)
                        Vx(i) = Vy(i) / KI(i)
                    Else
                        Vy(i) = 0
                        Vx(i) = 0
                    End If
                    i += 1
                Loop Until i = n + 1
                i = 0
                soma_x = 0
                soma_y = 0
                Do
                    soma_x = soma_x + Vx(i)
                    soma_y = soma_y + Vy(i)
                    i = i + 1
                Loop Until i = n + 1
                i = 0
                Do
                    Vx(i) = Vx(i) / soma_x
                    Vy(i) = Vy(i) / soma_y
                    i = i + 1
                Loop Until i = n + 1

                If V <= 0.5 Then
                    i = 0
                    stmp4 = 0
                    Do
                        stmp4 = stmp4 + KI(i) * Vx(i)
                        i = i + 1
                    Loop Until i = n + 1

                    F = stmp4 - 1

                    dKdP = DKDP_PR(T, P, Vx, Vy, VKij, VTc, VPc, VTb, Vw, RET_VC)

                    i = 0
                    dFdP = 0
                    Do
                        dFdP = dFdP + Vx(i) * dKdP(i)
                        i = i + 1
                    Loop Until i = n + 1
                Else
                    i = 0
                    stmp4 = 0
                    Do
                        stmp4 = stmp4 + Vy(i) / KI(i)
                        i = i + 1
                    Loop Until i = n + 1

                    F = stmp4 - 1

                    dKdP = DKDP_PR(T, P, Vx, Vy, VKij, VTc, VPc, VTb, Vw, RET_VC)

                    i = 0
                    dFdP = 0
                    Do
                        dFdP = dFdP - Vy(i) / (KI(i) ^ 2) * dKdP(i)
                        'dFdP = dFdP + Vx(i) * dKdP(i)
                        i = i + 1
                    Loop Until i = n + 1
                End If

                cnt += 1

                Pant = P
                P = P - F / dFdP

            Loop Until Math.Abs(F) < 0.01 Or Double.IsNaN(P) = True Or cnt > 50

            Dim tmp2(n + 1)
            tmp2(0) = Pant
            i = 0
            Do
                tmp2(i + 1) = KI(i)
                i = i + 1
            Loop Until i = n + 1

            Return tmp2

        End Function

        Function FLASH_PV(ByVal P As Double, ByVal V As Double, ByVal Vz As Object, ByVal VKij As Object, ByVal VTc As Object, ByVal VPc As Object, ByVal VTb As Object, ByVal Vw As Object, ByVal KI As Object, ByVal VCT As Object, Optional ByVal T As Double = 0) As Object

            Dim dFdT As Double
            Dim cnt As Integer = 0

            'T,Tc em K
            'P,Pc em Pa
            Dim n = UBound(Vz)

            Dim chk As Boolean = False

            'Dim betaL, betaV As Double
            Dim F As Double
            Dim dKdT(n) As Object
            Dim dKdTi(n) As Object
            Dim Vx(n), Vy(n), Vx_ant(n), Vy_ant(n), KI_ant(n), PHIV(n), PHIL(n), LN_CFL(n), LN_CFV(n) As Double
            Dim ai(n), bi(n), aml2(n), amv2(n) As Double
            Dim Vp(n), R, coeff(3), tmp(2, 2) As Double
            Dim Tc(n), Pc(n), Vc(n), W(n), Zc(n), alpha(n), m(n), a(n, n), b(n, n), ZL, ZV, Tr(n) As Double
            Dim i, j
            Dim t1, t2, t3, t4, t5 As Double
            Dim marcador3
            Dim stmp4_ant, stmp4, Tant As Double
            stmp4_ant = 0
            stmp4 = 0
            Tant = 0

            R = 8.314

            i = 0
            Do
                KI_ant(i) = 0
                i = i + 1
            Loop Until i = n + 1

            'Estimar T
            If Double.IsNaN(T) Or T = 0 Then
                Dim Tbol, Torv As Double

                Tbol = Me.BUBT_PR_M2(P, Vz, VKij, VTc, VPc, VTb, Vw, KI_ant, RET_VC)(0)
                Torv = Me.DEWT_PR_M2(P, Vz, VKij, VTc, VPc, VTb, Vw, KI_ant, RET_VC)(0)

                T = V * (Torv - Tbol) + Tbol

            End If

            i = 0
            Do
                Tc(i) = VTc(i)
                Tr(i) = T / Tc(i)
                Pc(i) = VPc(i)
                W(i) = Vw(i)
                Vp(i) = Me.AUX_PVAPi(i, T)
                i = i + 1
            Loop Until i = n + 1

            'Calcular Ki`s

            i = 0
            Do
                If Vz(i) <> 0 Then
                    KI(i) = Vp(i) / P
                    KI_ant(i) = 0
                ElseIf Vz(i) = 0 Then
                    KI(i) = 0
                    KI_ant(i) = 0
                End If
                i += 1
            Loop Until i = n + 1

            If Double.IsNaN(T) Then
                Dim tmp0(n + 1)
                tmp0(0) = Double.NaN
                i = 0
                Do
                    tmp0(i + 1) = KI(i)
                    i = i + 1
                Loop Until i = n + 1
                Return tmp0
            End If
            Dim L = 1 - V

            i = 0
            Do
                If Vz(i) <> 0 Then
                    Vy(i) = Vz(i) * KI(i) / ((KI(i) - 1) * V + 1)
                    Vx(i) = Vy(i) / KI(i)
                Else
                    Vy(i) = 0
                    Vx(i) = 0
                End If
                i += 1
            Loop Until i = n + 1
            i = 0
            Dim soma_x = 0
            Dim soma_y = 0
            Do
                soma_x = soma_x + Vx(i)
                soma_y = soma_y + Vy(i)
                i = i + 1
            Loop Until i = n + 1
            i = 0
            Do
                Vx(i) = Vx(i) / soma_x
                Vy(i) = Vy(i) / soma_y
                i = i + 1
            Loop Until i = n + 1

            'loop externo
            Do

                i = 0
                Do
                    Tr(i) = T / Tc(i)
                    Vp(i) = Me.AUX_PVAPi(i, T)
                    i = i + 1
                Loop Until i = n + 1

                Dim ai_(n)

                i = 0
                Do
                    If T / Tc(i) <= 1 Then
                        alpha(i) = (1 + (0.37464 + 1.54226 * W(i) - 0.26992 * W(i) ^ 2) * (1 - (T / Tc(i)) ^ 0.5)) ^ 2
                    Else
                        alpha(i) = (1 + 1.21 * (0.37464 + 1.54226 * W(i) - 0.26992 * W(i) ^ 2) * (1 - (T / Tc(i)) ^ 0.5)) ^ 2
                    End If
                    ai(i) = 0.45724 * alpha(i) * R ^ 2 * Tc(i) ^ 2 / Pc(i)
                    ai_(i) = ai(i) ^ 0.5
                    bi(i) = 0.0778 * R * Tc(i) / Pc(i)
                    i = i + 1
                Loop Until i = n + 1

                i = 0
                Do
                    j = 0
                    Do
                        a(i, j) = (ai(i) * ai(j)) ^ 0.5 * (1 - VKij(i, j))
                        j = j + 1
                    Loop Until j = n + 1
                    i = i + 1
                Loop Until i = n + 1

                ' CALCULO DAS RAIZES PARA A FASE LIQUIDA

                i = 0
                Do
                    aml2(i) = 0
                    i = i + 1
                Loop Until i = n + 1

                i = 0
                Dim aml = 0
                Do
                    j = 0
                    Do
                        aml = aml + Vx(i) * Vx(j) * a(i, j)
                        aml2(i) = aml2(i) + Vx(j) * a(j, i)
                        j = j + 1
                    Loop Until j = n + 1
                    i = i + 1
                Loop Until i = n + 1

                i = 0
                Dim bml = 0
                Do
                    bml = bml + Vx(i) * bi(i)
                    i = i + 1
                Loop Until i = n + 1

                Dim AG = aml * P / (R * T) ^ 2
                Dim BG = bml * P / (R * T)

                coeff(0) = -AG * BG + BG ^ 2 + BG ^ 3
                coeff(1) = AG - 3 * BG ^ 2 - 2 * BG
                coeff(2) = BG - 1
                coeff(3) = 1

                Dim temp1 = Poly_Roots(coeff)
                Dim tv = 0
                Dim tv2 = 0

                Try

                    If temp1(0, 0) > temp1(1, 0) Then
                        tv = temp1(1, 0)
                        tv2 = temp1(1, 1)
                        temp1(1, 0) = temp1(0, 0)
                        temp1(0, 0) = tv
                        temp1(1, 1) = temp1(0, 1)
                        temp1(0, 1) = tv2
                    End If
                    If temp1(0, 0) > temp1(2, 0) Then
                        tv = temp1(2, 0)
                        temp1(2, 0) = temp1(0, 0)
                        temp1(0, 0) = tv
                        tv2 = temp1(2, 1)
                        temp1(2, 1) = temp1(0, 1)
                        temp1(0, 1) = tv2
                    End If
                    If temp1(1, 0) > temp1(2, 0) Then
                        tv = temp1(2, 0)
                        temp1(2, 0) = temp1(1, 0)
                        temp1(1, 0) = tv
                        tv2 = temp1(2, 1)
                        temp1(2, 1) = temp1(1, 1)
                        temp1(1, 1) = tv2
                    End If

                    ZL = temp1(0, 0)
                    If temp1(0, 1) <> 0 Then
                        ZL = temp1(1, 0)
                        If temp1(1, 1) <> 0 Then
                            ZL = temp1(2, 0)
                        End If
                    End If

                Catch

                    Dim findZL
                    ZL = 0
                    Do
                        findZL = coeff(3) * ZL ^ 3 + coeff(2) * ZL ^ 2 + coeff(1) * ZL + coeff(0)
                        ZL += 0.000001
                        If ZL > 1 Then ZL = 0
                    Loop Until Math.Abs(findZL) < 0.0001

                End Try


                Dim sum_ci As Double = 0
                For i = 0 To n
                    sum_ci += VCT(i)
                Next

                ZL = ZL - P * sum_ci / (R * T)

                'betaL = 1 / P * (1 - (BG * ZL ^ 2 + AG * ZL - 6 * BG ^ 2 * ZL - 2 * BG * ZL - 2 * AG * BG + 2 * BG ^ 2 + 2 * BG) / (ZL * (3 * ZL ^ 2 - 2 * ZL + 2 * BG * ZL + AG - 3 * BG ^ 2 - 2 * BG)))

                'Dim res As Object
                'If betaL > 0.005 / 101325 Then
                '    res = Me.m_pr.GeneratePseudoRoot(T, P, Vx, VKij, VTc, VPc, Vw, VTb, "L")
                '    If Not res Is Nothing Then
                '        ZL = res(0)
                '        AG = res(1)
                '        BG = res(2)
                '        'amv = res(3)
                '        'bmv = res(4)
                '    End If
                'End If

                ' CALCULO DO COEFICIENTE DE FUGACIDADE DA FASE LIQUIDA

                i = 0
                Do
                    'If Tr(i) >= 1 Then
                    '    LN_CFL(i) = Math.Log(Vp(i))
                    '    PHIL(i) = Math.Exp(Vx(i) * LN_CFL(i))
                    'Else
                    t1 = bi(i) * (ZL - 1) / bml
                    t2 = -Math.Log(ZL - BG)
                    t3 = aml * (2 * aml2(i) / aml - bi(i) / bml)
                    t4 = Math.Log((ZL + (1 + 2 ^ 0.5) * BG) / (ZL + (1 - 2 ^ 0.5) * BG))
                    t5 = 2 * 2 ^ 0.5 * bml * R * T
                    LN_CFL(i) = t1 + t2 - (t3 * t4 / t5)
                    PHIL(i) = Math.Exp(LN_CFL(i)) * Vx(i) * P
                    'End If
                    i = i + 1
                Loop Until i = n + 1

                marcador3 = 0


                ' CALCULO DAS RAIZES PARA A FASE VAPOR

                i = 0
                Do
                    amv2(i) = 0
                    i = i + 1
                Loop Until i = n + 1

                i = 0
                Dim amv = 0
                Do
                    j = 0
                    Do
                        amv = amv + Vy(i) * Vy(j) * a(i, j)
                        amv2(i) = amv2(i) + Vy(j) * a(j, i)
                        j = j + 1
                    Loop Until j = n + 1
                    i = i + 1
                Loop Until i = n + 1

                i = 0
                Dim bmv = 0
                Do
                    bmv = bmv + Vy(i) * bi(i)
                    i = i + 1
                Loop Until i = n + 1

                AG = amv * P / (R * T) ^ 2
                BG = bmv * P / (R * T)

                coeff(0) = -AG * BG + BG ^ 2 + BG ^ 3
                coeff(1) = AG - 3 * BG ^ 2 - 2 * BG
                coeff(2) = BG - 1
                coeff(3) = 1

                temp1 = Poly_Roots(coeff)
                tv = 0

                Try

                    If temp1(0, 0) > temp1(1, 0) Then
                        tv = temp1(1, 0)
                        temp1(1, 0) = temp1(0, 0)
                        temp1(0, 0) = tv
                        tv2 = temp1(1, 1)
                        temp1(1, 1) = temp1(0, 1)
                        temp1(0, 1) = tv2
                    End If
                    If temp1(0, 0) > temp1(2, 0) Then
                        tv = temp1(2, 0)
                        temp1(2, 0) = temp1(0, 0)
                        temp1(0, 0) = tv
                        tv2 = temp1(2, 1)
                        temp1(2, 1) = temp1(0, 1)
                        temp1(0, 1) = tv2
                    End If
                    If temp1(1, 0) > temp1(2, 0) Then
                        tv = temp1(2, 0)
                        temp1(2, 0) = temp1(1, 0)
                        temp1(1, 0) = tv
                        tv2 = temp1(2, 1)
                        temp1(2, 1) = temp1(1, 1)
                        temp1(1, 1) = tv2
                    End If

                    ZV = temp1(2, 0)
                    If temp1(2, 1) <> 0 Then
                        ZV = temp1(1, 0)
                        If temp1(1, 1) <> 0 Then
                            ZV = temp1(0, 0)
                        End If
                    End If

                Catch

                    Dim findZV
                    ZV = 1
                    Do
                        findZV = coeff(3) * ZV ^ 3 + coeff(2) * ZV ^ 2 + coeff(1) * ZV + coeff(0)
                        ZV -= 0.00001
                        If ZV < 0 Then ZV = 1
                    Loop Until Math.Abs(findZV) < 0.0001

                End Try

                ZV = ZV - P * sum_ci / (R * T)

                'betaV = 1 / P * (1 - (BG * ZV ^ 2 + AG * ZV - 6 * BG ^ 2 * ZV - 2 * BG * ZV - 2 * AG * BG + 2 * BG ^ 2 + 2 * BG) / (ZV * (3 * ZV ^ 2 - 2 * ZV + 2 * BG * ZV + AG - 3 * BG ^ 2 - 2 * BG)))

                'If betaV > 3 / (P / 101325) Or betaV < 0.9 / (P / 101325) Then
                '    res = Me.m_pr.GeneratePseudoRoot(T, P, Vy, VKij, VTc, VPc, Vw, VTb, "V")
                '    If Not res Is Nothing Then
                '        ZV = res(0)
                '        AG = res(1)
                '        BG = res(2)
                '        'amv = res(3)
                '        'bmv = res(4)
                '    End If
                'End If

                ' CALCULO DO COEFICIENTE DE FUGACIDADE DA FASE VAPOR

                i = 0
                Do
                    t1 = bi(i) * (ZV - 1) / bmv
                    t2 = -Math.Log(ZV - BG)
                    t3 = amv * (2 * amv2(i) / amv - bi(i) / bmv)
                    t4 = Math.Log(ZV + (1 + 2 ^ 0.5) * BG) - Math.Log(ZV + ((1 - 2 ^ 0.5) * BG))
                    t5 = 8 ^ 0.5 * bmv * R * T
                    LN_CFV(i) = t1 + t2 - (t3 * t4 / t5)
                    PHIV(i) = Math.Exp(LN_CFV(i)) * Vy(i) * P
                    i = i + 1
                Loop Until i = n + 1

                ' CALCULO DA COMPOSICAO DA FASE VAPOR

                i = 0
                Do
                    KI(i) = (Math.Exp(LN_CFL(i)) / (Math.Exp(LN_CFV(i))))
                    i = i + 1
                Loop Until i = n + 1

                i = 0
                Do
                    If Vz(i) <> 0 Then
                        Vy_ant(i) = Vy(i)
                        Vx_ant(i) = Vx(i)
                        Vy(i) = Vz(i) * KI(i) / ((KI(i) - 1) * V + 1)
                        Vx(i) = Vy(i) / KI(i)
                    Else
                        Vy(i) = 0
                        Vx(i) = 0
                    End If
                    i += 1
                Loop Until i = n + 1
                i = 0
                soma_x = 0
                soma_y = 0
                Do
                    soma_x = soma_x + Vx(i)
                    soma_y = soma_y + Vy(i)
                    i = i + 1
                Loop Until i = n + 1
                i = 0
                Do
                    Vx(i) = Vx(i) / soma_x
                    Vy(i) = Vy(i) / soma_y
                    i = i + 1
                Loop Until i = n + 1

                If V <= 0.5 Then
                    i = 0
                    stmp4 = 0
                    Do
                        stmp4 = stmp4 + KI(i) * Vx(i)
                        i = i + 1
                    Loop Until i = n + 1
                    dKdT = DKDT_PR(T, P, Vx, Vy, VKij, VTc, VPc, VTb, Vw, RET_VC)
                    F = stmp4 - 1
                    i = 0
                    dFdT = 0
                    Do
                        dFdT = dFdT + Vx(i) * dKdT(i)
                        i = i + 1
                    Loop Until i = n + 1
                Else
                    i = 0
                    stmp4 = 0
                    Do
                        stmp4 = stmp4 + Vy(i) / KI(i)
                        i = i + 1
                    Loop Until i = n + 1
                    dKdT = DKDT_PR(T, P, Vx, Vy, VKij, VTc, VPc, VTb, Vw, RET_VC)
                    F = stmp4 - 1
                    i = 0
                    dFdT = 0
                    Do
                        dFdT = dFdT - Vy(i) / (KI(i) ^ 2) * dKdT(i)
                        i = i + 1
                    Loop Until i = n + 1
                End If

                Tant = T
                T = T - F / dFdT

                cnt += 1

            Loop Until Math.Abs(F) < 0.000001 Or Double.IsNaN(T) = True Or cnt > 50

            Dim tmp2(n + 1)
            tmp2(0) = Tant
            i = 0
            Do
                tmp2(i + 1) = KI(i)
                i = i + 1
            Loop Until i = n + 1

            Return tmp2

        End Function

#End Region

#Region "    Flashes BRENT"

        Function FLASH_PH(ByVal Test As Double, ByVal HT As Double, ByVal P As Double, ByVal Vz As Object, ByVal VKij As Object, ByVal VTc As Object, ByVal VPc As Object, ByVal VTb As Object, ByVal Vw As Object) As Object

            Dim maxitINT As Integer = CInt(Me.Parameters("PP_PHFMII"))
            Dim maxitEXT As Integer = CInt(Me.Parameters("PP_PHFMEI"))
            Dim tolINT As Double = CDbl(Me.Parameters("PP_PHFILT"))
            Dim tolEXT As Double = CDbl(Me.Parameters("PP_PHFELT"))

            'T,Tc em K
            'P,Pc em Pa

            Dim T, Tsup, Tinf, Vx(UBound(Vz)), Vy(UBound(Vz)), x(UBound(Vz)), Hsup, Hinf
            Dim l As Integer = 0
            Dim i As Integer = 0

            If Test <> 0 Then
                Tinf = Test - 100
                Tsup = Test + 100
            Else
                Tinf = 100
                Tsup = 2000
            End If
            If Tinf < 100 Then Tinf = 100

            Hinf = Me.m_pr.H_PR_MIX("L", Tinf, P, Vz, VKij, VTc, VPc, Vw, Me.RET_VMM, Me.RET_Hid(298.15, Tinf, Vz))
            Hsup = Me.m_pr.H_PR_MIX("V", Tsup, P, Vz, VKij, VTc, VPc, Vw, Me.RET_VMM, Me.RET_Hid(298.15, Tsup, Vz))

            If HT >= Hsup Then
                T = Me.ESTIMAR_T_H(HT, Test, "V", P, Vz, VKij, VTc, VPc, Vw, Me.RET_VMM())
                GoTo Final4
            ElseIf HT <= Hinf Then
                T = Me.ESTIMAR_T_H(HT, Test, "L", P, Vz, VKij, VTc, VPc, Vw, Me.RET_VMM())
                GoTo Final4
            End If

            Dim fT, fT_inf, nsub, delta_T As Double

            nsub = 20

            delta_T = (Tsup - Tinf) / nsub

            Do
                fT = OBJ_FUNC_PH_FLASH(Tinf, HT, P, Vz, VKij, VTc, VPc, VTb, Vw, Me.RET_VMM)
                Tinf = Tinf + delta_T
                fT_inf = OBJ_FUNC_PH_FLASH(Tinf, HT, P, Vz, VKij, VTc, VPc, VTb, Vw, Me.RET_VMM)
                l += 1
                If l > 100 Then Throw New Exception(DWSIM.App.GetLocalString("PropPack_FlashMaxIt"))
            Loop Until fT * fT_inf < 0
            Tsup = Tinf
            Tinf = Tinf - delta_T

            'método de Brent para encontrar Vc

            Dim aaa, bbb, ccc, ddd, eee, min11, min22, faa, fbb, fcc, ppp, qqq, rrr, sss, tol11, xmm As Double
            Dim ITMAX2 As Integer = 10000
            Dim iter2 As Integer

            aaa = Tinf
            bbb = Tsup
            ccc = Tsup

            faa = OBJ_FUNC_PH_FLASH(Tinf, HT, P, Vz, VKij, VTc, VPc, VTb, Vw, Me.RET_VMM)
            fbb = OBJ_FUNC_PH_FLASH(Tsup, HT, P, Vz, VKij, VTc, VPc, VTb, Vw, Me.RET_VMM)
            fcc = fbb

            iter2 = 0
            Do
                If (fbb > 0 And fcc > 0) Or (fbb < 0 And fcc < 0) Then
                    ccc = aaa
                    fcc = faa
                    ddd = bbb - aaa
                    eee = ddd
                End If
                If Math.Abs(fcc) < Math.Abs(fbb) Then
                    aaa = bbb
                    bbb = ccc
                    ccc = aaa
                    faa = fbb
                    fbb = fcc
                    fcc = faa
                End If
                tol11 = tolEXT
                xmm = 0.5 * (ccc - bbb)
                If (Math.Abs(xmm) <= tol11) Or (fbb = 0) Then GoTo Final3
                If (Math.Abs(eee) >= tol11) And (Math.Abs(faa) > Math.Abs(fbb)) Then
                    sss = fbb / faa
                    If aaa = ccc Then
                        ppp = 2 * xmm * sss
                        qqq = 1 - sss
                    Else
                        qqq = faa / fcc
                        rrr = fbb / fcc
                        ppp = sss * (2 * xmm * qqq * (qqq - rrr) - (bbb - aaa) * (rrr - 1))
                        qqq = (qqq - 1) * (rrr - 1) * (sss - 1)
                    End If
                    If ppp > 0 Then qqq = -qqq
                    ppp = Math.Abs(ppp)
                    min11 = 3 * xmm * qqq - Math.Abs(tol11 * qqq)
                    min22 = Math.Abs(eee * qqq)
                    Dim tvar2 As Double
                    If min11 < min22 Then tvar2 = min11
                    If min11 > min22 Then tvar2 = min22
                    If 2 * ppp < tvar2 Then
                        eee = ddd
                        ddd = ppp / qqq
                    Else
                        ddd = xmm
                        eee = ddd
                    End If
                Else
                    ddd = xmm
                    eee = ddd
                End If
                aaa = bbb
                faa = fbb
                If (Math.Abs(ddd) > tol11) Then
                    bbb += ddd
                Else
                    bbb += Math.Sign(xmm) * tol11
                End If
                fbb = OBJ_FUNC_PH_FLASH(bbb, HT, P, Vz, VKij, VTc, VPc, VTb, Vw, Me.RET_VMM)
                iter2 += 1
            Loop Until iter2 = ITMAX2

Final3:     T = bbb
Final4:

            Dim tmp2 = FLASH_TP(T, P, Vz, VKij, VTc, VPc, VTb, Vw, RET_VC)
            tmp2(3, 0) = T

            Return tmp2

        End Function

        Function OBJ_FUNC_PH_FLASH(ByVal T As Double, ByVal H As Double, ByVal P As Double, ByVal Vz As Object, ByVal VKij As Object, ByVal VTc As Object, ByVal VPc As Object, ByVal VTb As Object, ByVal Vw As Object, ByVal VMM As Object, Optional ByVal V As Double = 0, Optional ByVal KI As Object = Nothing) As Object

            Dim tmp = Me.FLASH_TP(T, P, Vz, VKij, VTc, VPc, VTb, Vw, RET_VC)

            Dim Vx(UBound(Vz)), Vy(UBound(Vz)), L, HV As Double

            Dim n = UBound(Vz)
            Dim i As Integer

            L = tmp(0, 0)
            V = tmp(1, 0)

            i = 0
            Do
                Vx(i) = tmp(0, i + 1)
                Vy(i) = tmp(1, i + 1)
                i = i + 1
            Loop Until i = n + 1

            Dim HL1, HL2, xl1, xl2 As Double
            Dim wi As Integer
            Dim Vx1(n), Vx2(n) As Double
            If L <> 0 Then
                wi = Me.GetWaterIndex(Vx)
                If wi <> -1 Then
                    'there is water in the mixture
                    xl1 = L * (1 - Vx(wi))
                    xl2 = L - xl1
                    Vx1 = Vx.Clone
                    Vx2 = Vx.Clone
                    Vx1(wi) = 0
                    Vx1 = Me.AUX_NORMALIZE(Vx1)
                    Vx2 = Me.AUX_ERASE(Vx)
                    Vx2(wi) = 1
                    HL1 = Me.m_pr.H_PR_MIX("L", T, P, Vx1, RET_VKij(), RET_VTC, RET_VPC, RET_VW, RET_VMM, Me.RET_Hid(298.15, T, Vx1))
                    HL2 = Me.m_pr.H_PR_MIX("L", T, P, Vx2, RET_VKij(), RET_VTC, RET_VPC, RET_VW, RET_VMM, Me.RET_Hid(298.15, T, Vx2))
                Else
                    'there is NO water in the mixture
                    xl1 = L
                    xl2 = 0
                    Vx1 = Vx.Clone
                    Vx2 = Me.AUX_ERASE(Vx)
                    HL1 = Me.m_pr.H_PR_MIX("L", T, P, Vx1, RET_VKij(), RET_VTC, RET_VPC, RET_VW, RET_VMM, Me.RET_Hid(298.15, T, Vx1))
                    HL2 = 0
                End If
            End If
            If V > 0 Then HV = Me.m_pr.H_PR_MIX("V", T, P, Vy, VKij, VTc, VPc, Vw, VMM, Me.RET_Hid(298.15, T, Vy))

            Dim mmg, mml1, mml2
            mmg = 0
            mml1 = 0
            mml2 = 0
            i = 0
            Do
                mmg += Vy(i) * VMM(i)
                mml1 += Vx1(i) * VMM(i)
                mml2 += Vx2(i) * VMM(i)
                i = i + 1
            Loop Until i = n + 1

            If Double.IsNaN(HL1) Then HL1 = 0.0#
            If Double.IsNaN(HL2) Then HL2 = 0.0#

            OBJ_FUNC_PH_FLASH = H - (mmg * V / (mmg * V + mml1 * xl1 + mml2 * xl2)) * HV - (mml1 * xl1 / (mmg * V + mml1 * xl1 + mml2 * xl2)) * HL1 - (mml2 * xl2 / (mmg * V + mml1 * xl1 + mml2 * xl2)) * HL2

        End Function

        Function FLASH_PS(ByVal Test As Double, ByVal ST As Double, ByVal P As Double, ByVal Vz As Object, ByVal VKij As Object, ByVal VTc As Object, ByVal VPc As Object, ByVal VTb As Object, ByVal Vw As Object) As Object

            Dim maxitINT As Integer = CInt(Me.Parameters("PP_PSFMII"))
            Dim maxitEXT As Integer = CInt(Me.Parameters("PP_PSFMEI"))
            Dim tolINT As Double = CDbl(Me.Parameters("PP_PSFILT"))
            Dim tolEXT As Double = CDbl(Me.Parameters("PP_PSFELT"))

            'T,Tc em K
            'P,Pc em Pa

            Dim T, Tsup, Tinf, Vx(UBound(Vz)), Vy(UBound(Vz)), x(UBound(Vz)), Ssup, Sinf
            Dim l As Integer = 0
            Dim i As Integer = 0

            If Test <> 0 Then
                Tinf = Test - 100
                Tsup = Test + 100
            Else
                Tinf = 100
                Tsup = 2000
            End If

            If Tinf < 100 Then Tinf = 100

            Sinf = Me.m_pr.S_PR_MIX("L", Tinf, P, Vz, VKij, VTc, VPc, Vw, Me.RET_VMM, Me.RET_Sid(298.15, Tinf, P, Vz))
            Ssup = Me.m_pr.S_PR_MIX("V", Tsup, P, Vz, VKij, VTc, VPc, Vw, Me.RET_VMM, Me.RET_Sid(298.15, Tsup, P, Vz))

            If ST >= Ssup Then
                T = Me.ESTIMAR_T_S(ST, Test, "V", P, Vz, VKij, VTc, VPc, Vw, Me.RET_VMM())
                GoTo Final4
            ElseIf ST <= Sinf Then
                T = Me.ESTIMAR_T_S(ST, Test, "L", P, Vz, VKij, VTc, VPc, Vw, Me.RET_VMM())
                GoTo Final4
            End If

            Dim fT, fT_inf, nsub, delta_T As Double

            nsub = 5

            delta_T = (Tsup - Tinf) / nsub

            Do
                fT = OBJ_FUNC_PS_FLASH(Tinf, ST, P, Vz, VKij, VTc, VPc, VTb, Vw, Me.RET_VMM)
                Tinf = Tinf + delta_T
                fT_inf = OBJ_FUNC_PS_FLASH(Tinf, ST, P, Vz, VKij, VTc, VPc, VTb, Vw, Me.RET_VMM)
                l += 1
                If l > 20 Then Throw New Exception(DWSIM.App.GetLocalString("PropPack_FlashMaxIt"))
            Loop Until fT * fT_inf < 0
            Tsup = Tinf
            Tinf = Tinf - delta_T

            'método de Brent para encontrar Vc

            Dim aaa, bbb, ccc, ddd, eee, min11, min22, faa, fbb, fcc, ppp, qqq, rrr, sss, tol11, xmm As Double
            Dim ITMAX2 As Integer = 10000
            Dim iter2 As Integer

            aaa = Tinf
            bbb = Tsup
            ccc = Tsup

            faa = OBJ_FUNC_PS_FLASH(Tinf, ST, P, Vz, VKij, VTc, VPc, VTb, Vw, Me.RET_VMM)
            fbb = OBJ_FUNC_PS_FLASH(Tsup, ST, P, Vz, VKij, VTc, VPc, VTb, Vw, Me.RET_VMM)
            fcc = fbb

            iter2 = 0
            Do
                If (fbb > 0 And fcc > 0) Or (fbb < 0 And fcc < 0) Then
                    ccc = aaa
                    fcc = faa
                    ddd = bbb - aaa
                    eee = ddd
                End If
                If Math.Abs(fcc) < Math.Abs(fbb) Then
                    aaa = bbb
                    bbb = ccc
                    ccc = aaa
                    faa = fbb
                    fbb = fcc
                    fcc = faa
                End If
                tol11 = tolEXT
                xmm = 0.5 * (ccc - bbb)
                If (Math.Abs(xmm) <= tol11) Or (fbb = 0) Then GoTo Final3
                If (Math.Abs(eee) >= tol11) And (Math.Abs(faa) > Math.Abs(fbb)) Then
                    sss = fbb / faa
                    If aaa = ccc Then
                        ppp = 2 * xmm * sss
                        qqq = 1 - sss
                    Else
                        qqq = faa / fcc
                        rrr = fbb / fcc
                        ppp = sss * (2 * xmm * qqq * (qqq - rrr) - (bbb - aaa) * (rrr - 1))
                        qqq = (qqq - 1) * (rrr - 1) * (sss - 1)
                    End If
                    If ppp > 0 Then qqq = -qqq
                    ppp = Math.Abs(ppp)
                    min11 = 3 * xmm * qqq - Math.Abs(tol11 * qqq)
                    min22 = Math.Abs(eee * qqq)
                    Dim tvar2 As Double
                    If min11 < min22 Then tvar2 = min11
                    If min11 > min22 Then tvar2 = min22
                    If 2 * ppp < tvar2 Then
                        eee = ddd
                        ddd = ppp / qqq
                    Else
                        ddd = xmm
                        eee = ddd
                    End If
                Else
                    ddd = xmm
                    eee = ddd
                End If
                aaa = bbb
                faa = fbb
                If (Math.Abs(ddd) > tol11) Then
                    bbb += ddd
                Else
                    bbb += Math.Sign(xmm) * tol11
                End If
                fbb = OBJ_FUNC_PS_FLASH(bbb, ST, P, Vz, VKij, VTc, VPc, VTb, Vw, Me.RET_VMM)
                iter2 += 1
            Loop Until iter2 = ITMAX2

Final3:     T = bbb
Final4:

            Dim tmp2 = FLASH_TP(T, P, Vz, VKij, VTc, VPc, VTb, Vw, RET_VC)
            tmp2(3, 0) = T

            Return tmp2

        End Function

        Function OBJ_FUNC_PS_FLASH(ByVal T As Double, ByVal S As Double, ByVal P As Double, ByVal Vz As Object, ByVal VKij As Object, ByVal VTc As Object, ByVal VPc As Object, ByVal VTb As Object, ByVal Vw As Object, ByVal VMM As Object, Optional ByVal V As Double = 0, Optional ByVal KI As Object = Nothing) As Object

            Dim tmp = Me.FLASH_TP(T, P, Vz, VKij, VTc, VPc, VTb, Vw, RET_VC)

            Dim Vx(UBound(Vz)), Vy(UBound(Vz)), L, SV As Double

            Dim n = UBound(Vz)
            Dim i As Integer

            L = tmp(0, 0)
            V = tmp(1, 0)
            i = 0
            Do
                Vx(i) = tmp(0, i + 1)
                Vy(i) = tmp(1, i + 1)
                i = i + 1
            Loop Until i = n + 1

            Dim SL1, SL2, xl1, xl2 As Double
            Dim wi As Integer
            Dim Vx1(n), Vx2(n) As Double
            If L <> 0 Then
                wi = Me.GetWaterIndex(Vx)
                If wi <> -1 Then
                    'there is water in the mixture
                    xl1 = L * (1 - Vx(wi))
                    xl2 = L - xl1
                    Vx1 = Vx.Clone
                    Vx2 = Vx.Clone
                    Vx1(wi) = 0
                    Vx1 = Me.AUX_NORMALIZE(Vx1)
                    Vx2 = Me.AUX_ERASE(Vx)
                    Vx2(wi) = 1
                    SL1 = Me.m_pr.S_PR_MIX("L", T, P, Vx1, RET_VKij, RET_VTC, RET_VPC, RET_VW, RET_VMM, Me.RET_Sid(298.15, T, P, Vx1))
                    SL2 = Me.m_pr.S_PR_MIX("L", T, P, Vx2, RET_VKij, RET_VTC, RET_VPC, RET_VW, RET_VMM, Me.RET_Sid(298.15, T, P, Vx2))
                Else
                    'there is NO water in the mixture
                    xl1 = L
                    xl2 = 0
                    Vx1 = Vx.Clone
                    Vx2 = Me.AUX_ERASE(Vx)
                    SL1 = Me.m_pr.S_PR_MIX("L", T, P, Vx1, RET_VKij, RET_VTC, RET_VPC, RET_VW, RET_VMM, Me.RET_Sid(298.15, T, P, Vx1))
                    SL2 = 0
                End If
            End If
            If V <> 0 Then SV = Me.m_pr.S_PR_MIX("V", T, P, Vy, RET_VKij, RET_VTC, RET_VPC, RET_VW, RET_VMM, Me.RET_Sid(298.15, T, P, Vy))

            Dim mmg, mml1, mml2
            mmg = 0
            mml1 = 0
            mml2 = 0
            i = 0
            Do
                mmg += Vy(i) * VMM(i)
                mml1 += Vx1(i) * VMM(i)
                mml2 += Vx2(i) * VMM(i)
                i = i + 1
            Loop Until i = n + 1

            If Double.IsNaN(SL1) Then SL1 = 0.0#
            If Double.IsNaN(SL2) Then SL2 = 0.0#

            OBJ_FUNC_PS_FLASH = S - (mmg * V / (mmg * V + mml1 * xl1 + mml2 * xl2)) * SV - (mml1 * xl1 / (mmg * V + mml1 * xl1 + mml2 * xl2)) * SL1 - (mml2 * xl2 / (mmg * V + mml1 * xl1 + mml2 * xl2)) * SL2

        End Function

#End Region

        Public Overrides Function DW_CalcEnthalpy(ByVal Vx As System.Array, ByVal T As Double, ByVal P As Double, ByVal st As State) As Double

            Dim H As Double

            If st = State.Liquid Then
                H = Me.m_pr.H_PR_MIX("L", T, P, Vx, RET_VKij(), RET_VTC, RET_VPC, RET_VW, RET_VMM, Me.RET_Hid(298.15, T, Vx))
            Else
                H = Me.m_pr.H_PR_MIX("V", T, P, Vx, RET_VKij(), RET_VTC, RET_VPC, RET_VW, RET_VMM, Me.RET_Hid(298.15, T, Vx))
            End If

            Return H

        End Function

        Public Overrides Function DW_CalcKvalue(ByVal Vx As System.Array, ByVal T As Double, ByVal P As Double) As [Object]

            Dim i As Integer
            Dim result = Me.FLASH_TP(T, P, Vx, RET_VKij, RET_VTC, RET_VPC, RET_VTB, RET_VW, RET_VC)
            Dim K As New ArrayList

            i = 1
            For Each subst As ClassesBasicasTermodinamica.Substancia In Me.CurrentMaterialStream.Fases(1).Componentes.Values
                K.Add(result(1, i) / result(0, i))
                i += 1
            Next

            i = 0
            For Each subst As ClassesBasicasTermodinamica.Substancia In Me.CurrentMaterialStream.Fases(1).Componentes.Values
                If K(i) = 0 Then K(i) = Me.AUX_PVAPi(subst.Nome, T) / P
                If Double.IsInfinity(K(i)) Or Double.IsNaN(K(i)) Then
                    Dim Pc, Tc, w As Double
                    Pc = subst.ConstantProperties.Critical_Pressure
                    Tc = subst.ConstantProperties.Critical_Temperature
                    w = subst.ConstantProperties.Acentric_Factor
                    K(i) = Pc / P * Math.Exp(5.373 * (1 + w) * (1 - Tc / T))
                End If
                i += 1
            Next

            Return K.ToArray

        End Function

        Public Overrides Function DW_CalcEnthalpyDeparture(ByVal Vx As System.Array, ByVal T As Double, ByVal P As Double, ByVal st As State) As Double
            Dim H As Double

            If st = State.Liquid Then
                H = Me.m_pr.H_PR_MIX("L", T, P, Vx, RET_VKij(), RET_VTC, RET_VPC, RET_VW, RET_VMM, 0)
            Else
                H = Me.m_pr.H_PR_MIX("V", T, P, Vx, RET_VKij(), RET_VTC, RET_VPC, RET_VW, RET_VMM, 0)
            End If

            Return H

        End Function

        Public Overrides Function DW_CalcBubP(ByVal Vx As System.Array, ByVal T As Double, Optional ByVal Pref As Double = 0, Optional ByVal K As System.Array = Nothing, Optional ByVal ReuseK As Boolean = False) As Object
            Dim j, n As Integer
            n = UBound(Vx)
            Dim KI(n) As Double
            j = 0
            Do
                KI(j) = 0
                j = j + 1
            Loop Until j = n + 1
            Return Me.BUBP_PR_M2(T, Vx, RET_VKij, RET_VTC, RET_VPC, RET_VTB, RET_VW, KI, RET_VC)
        End Function

        Public Overrides Function DW_CalcBubT(ByVal Vx As System.Array, ByVal P As Double, Optional ByVal Tref As Double = 0, Optional ByVal K As System.Array = Nothing, Optional ByVal ReuseK As Boolean = False) As Object
            Dim j, n As Integer
            n = UBound(Vx)
            Dim KI(n) As Double
            j = 0
            Do
                KI(j) = 0
                j = j + 1
            Loop Until j = n + 1
            Return Me.BUBT_PR_M2(P, Vx, RET_VKij, RET_VTC, RET_VPC, RET_VTB, RET_VW, KI, RET_VC)
        End Function

        Public Overrides Function DW_CalcDewP(ByVal Vx As System.Array, ByVal T As Double, Optional ByVal Pref As Double = 0, Optional ByVal K As System.Array = Nothing, Optional ByVal ReuseK As Boolean = False) As Object
            Dim j, n As Integer
            n = UBound(Vx)
            Dim KI(n) As Double
            j = 0
            Do
                KI(j) = 0
                j = j + 1
            Loop Until j = n + 1
            Return Me.DEWP_PR_M2(T, Vx, RET_VKij, RET_VTC, RET_VPC, RET_VTB, RET_VW, KI, RET_VC)
        End Function

        Public Overrides Function DW_CalcDewT(ByVal Vx As System.Array, ByVal P As Double, Optional ByVal Tref As Double = 0, Optional ByVal K As System.Array = Nothing, Optional ByVal ReuseK As Boolean = False) As Object
            Dim j, n As Integer
            n = UBound(Vx)
            Dim KI(n) As Double
            j = 0
            Do
                KI(j) = 0
                j = j + 1
            Loop Until j = n + 1
            Return Me.DEWT_PR_M2(P, Vx, RET_VKij, RET_VTC, RET_VPC, RET_VTB, RET_VW, KI, RET_VC)
        End Function

        Public Overrides Function DW_CalcCv_ISOL(ByVal fase1 As Fase, ByVal T As Double, ByVal P As Double) As Double
            Select Case fase1
                Case Fase.Liquid
                    Return Me.m_props.CpCvR("L", T, P, RET_VMOL(fase1), RET_VKij(), RET_VMAS(fase1), RET_VTC(), RET_VPC(), RET_VCP(T), RET_VMM(), RET_VW(), RET_VZRa())(2)
                Case Fase.Aqueous
                    Return Me.m_props.CpCvR("L", T, P, RET_VMOL(fase1), RET_VKij(), RET_VMAS(fase1), RET_VTC(), RET_VPC(), RET_VCP(T), RET_VMM(), RET_VW(), RET_VZRa())(2)
                Case Fase.Liquid1
                    Return Me.m_props.CpCvR("L", T, P, RET_VMOL(fase1), RET_VKij(), RET_VMAS(fase1), RET_VTC(), RET_VPC(), RET_VCP(T), RET_VMM(), RET_VW(), RET_VZRa())(2)
                Case Fase.Liquid2
                    Return Me.m_props.CpCvR("L", T, P, RET_VMOL(fase1), RET_VKij(), RET_VMAS(fase1), RET_VTC(), RET_VPC(), RET_VCP(T), RET_VMM(), RET_VW(), RET_VZRa())(2)
                Case Fase.Liquid3
                    Return Me.m_props.CpCvR("L", T, P, RET_VMOL(fase1), RET_VKij(), RET_VMAS(fase1), RET_VTC(), RET_VPC(), RET_VCP(T), RET_VMM(), RET_VW(), RET_VZRa())(2)
                Case Fase.Vapor
                    Return Me.m_props.CpCvR("V", T, P, RET_VMOL(fase1), RET_VKij(), RET_VMAS(fase1), RET_VTC(), RET_VPC(), RET_VCP(T), RET_VMM(), RET_VW(), RET_VZRa())(2)
            End Select
        End Function

        Public Overrides Sub DW_CalcCompPartialVolume(ByVal phase As Fase, ByVal T As Double, ByVal P As Double)

            Dim partvol As New Object
            Dim key As String = "0"
            Dim i As Integer = 0

            If Not Me.Parameters.ContainsKey("PP_USE_EOS_LIQDENS") Then Me.Parameters.Add("PP_USE_EOS_LIQDENS", 0)

            Select Case phase
                Case Fase.Liquid
                    key = "1"
                    If CInt(Me.Parameters("PP_USE_EOS_LIQDENS")) = 1 Then
                        partvol = Me.m_pr.CalcPartialVolume(T, P, RET_VMOL(phase), RET_VKij(), RET_VTC(), RET_VPC(), RET_VW(), RET_VTB(), "L", 0.01)
                    Else
                        partvol = New ArrayList
                        For Each subst As ClassesBasicasTermodinamica.Substancia In Me.CurrentMaterialStream.Fases(key).Componentes.Values
                            partvol.Add(1 / 1000 * subst.ConstantProperties.Molar_Weight / Me.m_props.liq_dens_rackett(T, subst.ConstantProperties.Critical_Temperature, subst.ConstantProperties.Critical_Pressure, subst.ConstantProperties.Acentric_Factor, subst.ConstantProperties.Molar_Weight, subst.ConstantProperties.Z_Rackett, P, Me.AUX_PVAPi(subst.Nome, T)))
                        Next
                    End If
                Case Fase.Aqueous
                    key = "6"
                    If CInt(Me.Parameters("PP_USE_EOS_LIQDENS")) = 1 Then
                        partvol = Me.m_pr.CalcPartialVolume(T, P, RET_VMOL(phase), RET_VKij(), RET_VTC(), RET_VPC(), RET_VW(), RET_VTB(), "L", 0.01)
                    Else
                        partvol = New ArrayList
                        For Each subst As ClassesBasicasTermodinamica.Substancia In Me.CurrentMaterialStream.Fases(key).Componentes.Values
                            partvol.Add(1 / 1000 * subst.ConstantProperties.Molar_Weight / Me.m_props.liq_dens_rackett(T, subst.ConstantProperties.Critical_Temperature, subst.ConstantProperties.Critical_Pressure, subst.ConstantProperties.Acentric_Factor, subst.ConstantProperties.Molar_Weight, subst.ConstantProperties.Z_Rackett, P, Me.AUX_PVAPi(subst.Nome, T)))
                        Next
                    End If
                Case Fase.Liquid1
                    key = "3"
                    If CInt(Me.Parameters("PP_USE_EOS_LIQDENS")) = 1 Then
                        partvol = Me.m_pr.CalcPartialVolume(T, P, RET_VMOL(phase), RET_VKij(), RET_VTC(), RET_VPC(), RET_VW(), RET_VTB(), "L", 0.01)
                    Else
                        partvol = New ArrayList
                        For Each subst As ClassesBasicasTermodinamica.Substancia In Me.CurrentMaterialStream.Fases(key).Componentes.Values
                            partvol.Add(1 / 1000 * subst.ConstantProperties.Molar_Weight / Me.m_props.liq_dens_rackett(T, subst.ConstantProperties.Critical_Temperature, subst.ConstantProperties.Critical_Pressure, subst.ConstantProperties.Acentric_Factor, subst.ConstantProperties.Molar_Weight, subst.ConstantProperties.Z_Rackett, P, Me.AUX_PVAPi(subst.Nome, T)))
                        Next
                    End If
                Case Fase.Liquid2
                    key = "4"
                    If CInt(Me.Parameters("PP_USE_EOS_LIQDENS")) = 1 Then
                        partvol = Me.m_pr.CalcPartialVolume(T, P, RET_VMOL(phase), RET_VKij(), RET_VTC(), RET_VPC(), RET_VW(), RET_VTB(), "L", 0.01)
                    Else
                        partvol = New ArrayList
                        For Each subst As ClassesBasicasTermodinamica.Substancia In Me.CurrentMaterialStream.Fases(key).Componentes.Values
                            partvol.Add(1 / 1000 * subst.ConstantProperties.Molar_Weight / Me.m_props.liq_dens_rackett(T, subst.ConstantProperties.Critical_Temperature, subst.ConstantProperties.Critical_Pressure, subst.ConstantProperties.Acentric_Factor, subst.ConstantProperties.Molar_Weight, subst.ConstantProperties.Z_Rackett, P, Me.AUX_PVAPi(subst.Nome, T)))
                        Next
                    End If
                Case Fase.Liquid3
                    key = "5"
                    If CInt(Me.Parameters("PP_USE_EOS_LIQDENS")) = 1 Then
                        partvol = Me.m_pr.CalcPartialVolume(T, P, RET_VMOL(phase), RET_VKij(), RET_VTC(), RET_VPC(), RET_VW(), RET_VTB(), "L", 0.01)
                    Else
                        partvol = New ArrayList
                        For Each subst As ClassesBasicasTermodinamica.Substancia In Me.CurrentMaterialStream.Fases(key).Componentes.Values
                            partvol.Add(1 / 1000 * subst.ConstantProperties.Molar_Weight / Me.m_props.liq_dens_rackett(T, subst.ConstantProperties.Critical_Temperature, subst.ConstantProperties.Critical_Pressure, subst.ConstantProperties.Acentric_Factor, subst.ConstantProperties.Molar_Weight, subst.ConstantProperties.Z_Rackett, P, Me.AUX_PVAPi(subst.Nome, T)))
                        Next
                    End If
                Case Fase.Vapor
                    partvol = Me.m_pr.CalcPartialVolume(T, P, RET_VMOL(phase), RET_VKij(), RET_VTC(), RET_VPC(), RET_VW(), RET_VTB(), "V", 0.01)
                    key = "2"
            End Select

            i = 0
            For Each subst As ClassesBasicasTermodinamica.Substancia In Me.CurrentMaterialStream.Fases(key).Componentes.Values
                subst.PartialVolume = partvol(i)
                i += 1
            Next

        End Sub

        Public Overrides Function AUX_VAPDENS(ByVal T As Double, ByVal P As Double) As Double
            Dim val As Double
            Dim Z As Double = Me.m_pr.Z_PR(T, P, RET_VMOL(Fase.Vapor), RET_VKij, RET_VTC, RET_VPC, RET_VW, "V")
            val = (Z * 8.314 * T) / P - Me.AUX_CM(Fase.Vapor)
            val = AUX_MMM(Fase.Vapor) / val
            Return val / 1000
        End Function


        Public Overrides Function DW_CalcEntropy(ByVal Vx As System.Array, ByVal T As Double, ByVal P As Double, ByVal st As State) As Double

            Dim S As Double

            If st = State.Liquid Then
                S = Me.m_pr.S_PR_MIX("L", T, P, Vx, RET_VKij(), RET_VTC, RET_VPC, RET_VW, RET_VMM, Me.RET_Sid(298.15, T, P, Vx))
            Else
                S = Me.m_pr.S_PR_MIX("V", T, P, Vx, RET_VKij(), RET_VTC, RET_VPC, RET_VW, RET_VMM, Me.RET_Sid(298.15, T, P, Vx))
            End If

            Return S

        End Function

        Public Overrides Function DW_CalcEntropyDeparture(ByVal Vx As System.Array, ByVal T As Double, ByVal P As Double, ByVal st As State) As Double
            Dim S As Double

            If st = State.Liquid Then
                S = Me.m_pr.S_PR_MIX("L", T, P, Vx, RET_VKij(), RET_VTC, RET_VPC, RET_VW, RET_VMM, 0)
            Else
                S = Me.m_pr.S_PR_MIX("V", T, P, Vx, RET_VKij(), RET_VTC, RET_VPC, RET_VW, RET_VMM, 0)
            End If

            Return S

        End Function

        Public Overrides Function DW_CalcFugCoeff(ByVal Vx As System.Array, ByVal T As Double, ByVal P As Double, ByVal st As State) As Object

            Dim prn As New PropertyPackages.ThermoPlugs.PR

            Dim lnfug As Object

            If st = State.Liquid Then
                lnfug = prn.CalcLnFug(T, P, Vx, Me.RET_VKij, Me.RET_VTC, Me.RET_VPC, Me.RET_VW, Nothing, "L")
            Else
                lnfug = prn.CalcLnFug(T, P, Vx, Me.RET_VKij, Me.RET_VTC, Me.RET_VPC, Me.RET_VW, Nothing, "V")
            End If

            Dim n As Integer = UBound(lnfug)
            Dim i As Integer
            Dim fugcoeff(n) As Double

            For i = 0 To n
                fugcoeff(i) = Exp(lnfug(i))
            Next

            Return fugcoeff

        End Function

        Public Overrides Sub DW_CalcProp(ByVal [property] As String, ByVal phase As Fase)

        End Sub
    End Class

End Namespace


