'    Electrolyte Property Package Base Class 
'    Copyright 2013-2014 Daniel Wagner O. de Medeiros
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
Imports System.Xml.Linq
Imports System.Linq
Imports DWSIM.DWSIM.ClassesBasicasTermodinamica
Imports DWSIM.DWSIM.MathEx
Imports DWSIM.DWSIM.MathEx.Common
Imports Ciloci.Flee

Namespace DWSIM.SimulationObjects.PropertyPackages

    <System.Serializable()> Public MustInherit Class ElectrolyteBasePropertyPackage

        Inherits DWSIM.SimulationObjects.PropertyPackages.PropertyPackage

        Private m_props As New DWSIM.SimulationObjects.PropertyPackages.Auxiliary.PROPS
        Public m_elec As New DWSIM.SimulationObjects.PropertyPackages.Auxiliary.Electrolyte
        Public Property ElectrolyteFlash As DWSIM.SimulationObjects.PropertyPackages.Auxiliary.FlashAlgorithms.ElectrolyteSVLE2
        Private m_id As New DWSIM.SimulationObjects.PropertyPackages.Auxiliary.Ideal

        Public Sub New(ByVal comode As Boolean)

            MyBase.New(comode)

            Me.IsConfigurable = True
            Me._packagetype = PropertyPackages.PackageType.ActivityCoefficient
            Me.IsElectrolytePP = True

            Me.ElectrolyteFlash = New Auxiliary.FlashAlgorithms.ElectrolyteSVLE2

        End Sub

        Public Sub New()

            MyBase.New()

            Me.IsConfigurable = True
            Me._packagetype = PropertyPackages.PackageType.ActivityCoefficient
            Me.IsElectrolytePP = True

            Me.ElectrolyteFlash = New Auxiliary.FlashAlgorithms.ElectrolyteSVLE2

        End Sub


#Region "    DWSIM Functions"

        Public Overrides Function AUX_LIQDENS(T As Double, Optional P As Double = 0.0, Optional Pvp As Double = 0.0, Optional phaseid As Integer = 3, Optional FORCE_EOS As Boolean = False) As Double

            Dim phase As Fase

            Select Case phaseid
                Case 1
                    phase = Fase.Liquid
                Case 3
                    phase = Fase.Liquid1
                Case 4
                    phase = Fase.Liquid2
                Case 5
                    phase = Fase.Liquid3
                Case 6
                    phase = Fase.Aqueous
            End Select

            Dim constprops As New List(Of ConstantProperties)
            For Each su As Substancia In Me.CurrentMaterialStream.Fases(0).Componentes.Values
                constprops.Add(su.ConstantProperties)
            Next

            Return Me.m_elec.LiquidDensity(RET_VMOL(phase), T, constprops)

        End Function

        Public Function RET_KIJ(ByVal id1 As String, ByVal id2 As String) As Double
            Return 0
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

            Return 0

        End Function

        Public Overrides Function DW_CalcK_ISOL(ByVal fase1 As DWSIM.SimulationObjects.PropertyPackages.Fase, ByVal T As Double, ByVal P As Double) As Double
            If fase1 = Fase.Liquid Then
                Return Me.AUX_CONDTL(T)
            ElseIf fase1 = Fase.Vapor Then
                Return Me.AUX_CONDTG(T, P)
            End If
        End Function

        Public Overrides Function DW_CalcMassaEspecifica_ISOL(ByVal fase1 As DWSIM.SimulationObjects.PropertyPackages.Fase, ByVal T As Double, ByVal P As Double, Optional ByVal pvp As Double = 0) As Double
            If fase1 = Fase.Liquid Then
                Return Me.AUX_LIQDENS(T)
            ElseIf fase1 = Fase.Vapor Then
                Return Me.AUX_VAPDENS(T, P)
            ElseIf fase1 = Fase.Mixture Then
                Return Me.CurrentMaterialStream.Fases(1).SPMProperties.volumetric_flow.GetValueOrDefault * Me.AUX_LIQDENS(T) / Me.CurrentMaterialStream.Fases(0).SPMProperties.volumetric_flow.GetValueOrDefault + Me.CurrentMaterialStream.Fases(2).SPMProperties.volumetric_flow.GetValueOrDefault * Me.AUX_VAPDENS(T, P) / Me.CurrentMaterialStream.Fases(0).SPMProperties.volumetric_flow.GetValueOrDefault
            End If
        End Function

        Public Overrides Function DW_CalcMM_ISOL(ByVal fase1 As DWSIM.SimulationObjects.PropertyPackages.Fase, ByVal T As Double, ByVal P As Double) As Double
            Return Me.AUX_MMM(fase1)
        End Function

        Public Overrides Sub DW_CalcCompFugCoeff(ByVal f As Fase)

            Dim fc As Object
            Dim vmol As Object = Me.RET_VMOL(f)
            Dim P, T As Double
            P = Me.CurrentMaterialStream.Fases(0).SPMProperties.pressure.GetValueOrDefault
            T = Me.CurrentMaterialStream.Fases(0).SPMProperties.temperature.GetValueOrDefault
            Select Case f
                Case Fase.Vapor
                    fc = Me.DW_CalcFugCoeff(vmol, T, P, State.Vapor)
                Case Else
                    fc = Me.DW_CalcFugCoeff(vmol, T, P, State.Liquid)
            End Select
            Dim i As Integer = 0
            For Each subs As Substancia In Me.CurrentMaterialStream.Fases(Me.RET_PHASEID(f)).Componentes.Values
                subs.FugacityCoeff = fc(i)
                i += 1
            Next

        End Sub

        Public Overrides Sub DW_CalcOverallProps()
            MyBase.DW_CalcOverallProps()
        End Sub

        Public Overrides Function DW_CalcPVAP_ISOL(ByVal T As Double) As Double
            Return Me.m_props.Pvp_leekesler(T, Me.RET_VTC(Fase.Liquid), Me.RET_VPC(Fase.Liquid), Me.RET_VW(Fase.Liquid))
        End Function

        Public Overrides Function DW_CalcTensaoSuperficial_ISOL(ByVal fase1 As DWSIM.SimulationObjects.PropertyPackages.Fase, ByVal T As Double, ByVal P As Double) As Double
            Return Me.AUX_SURFTM(T)
        End Function

        Public Overrides Sub DW_CalcTwoPhaseProps(ByVal fase1 As DWSIM.SimulationObjects.PropertyPackages.Fase, ByVal fase2 As DWSIM.SimulationObjects.PropertyPackages.Fase)

            Dim T As Double

            T = Me.CurrentMaterialStream.Fases(0).SPMProperties.temperature.GetValueOrDefault
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

            If Me.SupportedComponents.Contains(comp.ID) Then
                Return True
            ElseIf comp.IsHYPO = 1 Then
                Return False
            Else
                Return False
            End If

        End Function

        Function dfidRbb_H(ByVal Rbb, ByVal Kb0, ByVal Vz, ByVal Vu, ByVal sum_Hvi0, ByVal DHv, ByVal DHl, ByVal HT) As Double

            Dim i As Integer = 0
            Dim n = UBound(Vz)

            Dim Vpbb2(n), L2, V2, Kb2 As Double

            i = 0
            Dim sum_pi2 = 0.0#
            Dim sum_eui_pi2 = 0.0#
            Do
                Vpbb2(i) = Vz(i) / (1 - Rbb + Kb0 * Rbb * Exp(Vu(i)))
                sum_pi2 += Vpbb2(i)
                sum_eui_pi2 += Exp(Vu(i)) * Vpbb2(i)
                i = i + 1
            Loop Until i = n + 1
            Kb2 = sum_pi2 / sum_eui_pi2

            L2 = (1 - Rbb) * sum_pi2
            V2 = 1 - L2

            Return L2 * (DHv - DHl) - sum_Hvi0 - DHv + HT * Me.AUX_MMM(Vz)

        End Function

        Function dfidRbb_S(ByVal Rbb, ByVal Kb0, ByVal Vz, ByVal Vu, ByVal sum_Hvi0, ByVal DHv, ByVal DHl, ByVal ST) As Double

            Dim i As Integer = 0
            Dim n = UBound(Vz)

            Dim Vpbb2(n), L, V As Double

            i = 0
            Dim sum_pi2 = 0.0#
            Dim sum_eui_pi2 = 0.0#
            Do
                Vpbb2(i) = Vz(i) / (1 - Rbb + Kb0 * Rbb * Exp(Vu(i)))
                sum_pi2 += Vpbb2(i)
                sum_eui_pi2 += Exp(Vu(i)) * Vpbb2(i)
                i = i + 1
            Loop Until i = n + 1

            L = (1 - Rbb) * sum_pi2
            V = 1 - L

            Return L * (DHv - DHl) - sum_Hvi0 - DHv + ST * Me.AUX_MMM(Vz)

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

            Select Case phase
                Case Fase.Liquid
                    For Each subst As ClassesBasicasTermodinamica.Substancia In Me.CurrentMaterialStream.Fases(1).Componentes.Values
                        subst.PartialVolume = 1 / 1000 * subst.ConstantProperties.Molar_Weight / Me.m_props.liq_dens_rackett(T, subst.ConstantProperties.Critical_Temperature, subst.ConstantProperties.Critical_Pressure, subst.ConstantProperties.Acentric_Factor, subst.ConstantProperties.Molar_Weight, subst.ConstantProperties.Z_Rackett, P, Me.AUX_PVAPi(subst.Nome, T))
                    Next
                Case Fase.Aqueous
                    For Each subst As ClassesBasicasTermodinamica.Substancia In Me.CurrentMaterialStream.Fases(6).Componentes.Values
                        subst.PartialVolume = 1 / 1000 * subst.ConstantProperties.Molar_Weight / Me.m_props.liq_dens_rackett(T, subst.ConstantProperties.Critical_Temperature, subst.ConstantProperties.Critical_Pressure, subst.ConstantProperties.Acentric_Factor, subst.ConstantProperties.Molar_Weight, subst.ConstantProperties.Z_Rackett, P, Me.AUX_PVAPi(subst.Nome, T))
                    Next
                Case Fase.Liquid1
                    For Each subst As ClassesBasicasTermodinamica.Substancia In Me.CurrentMaterialStream.Fases(3).Componentes.Values
                        subst.PartialVolume = 1 / 1000 * subst.ConstantProperties.Molar_Weight / Me.m_props.liq_dens_rackett(T, subst.ConstantProperties.Critical_Temperature, subst.ConstantProperties.Critical_Pressure, subst.ConstantProperties.Acentric_Factor, subst.ConstantProperties.Molar_Weight, subst.ConstantProperties.Z_Rackett, P, Me.AUX_PVAPi(subst.Nome, T))
                    Next
                Case Fase.Liquid2
                    For Each subst As ClassesBasicasTermodinamica.Substancia In Me.CurrentMaterialStream.Fases(4).Componentes.Values
                        subst.PartialVolume = 1 / 1000 * subst.ConstantProperties.Molar_Weight / Me.m_props.liq_dens_rackett(T, subst.ConstantProperties.Critical_Temperature, subst.ConstantProperties.Critical_Pressure, subst.ConstantProperties.Acentric_Factor, subst.ConstantProperties.Molar_Weight, subst.ConstantProperties.Z_Rackett, P, Me.AUX_PVAPi(subst.Nome, T))
                    Next
                Case Fase.Liquid3
                    For Each subst As ClassesBasicasTermodinamica.Substancia In Me.CurrentMaterialStream.Fases(5).Componentes.Values
                        subst.PartialVolume = 1 / 1000 * subst.ConstantProperties.Molar_Weight / Me.m_props.liq_dens_rackett(T, subst.ConstantProperties.Critical_Temperature, subst.ConstantProperties.Critical_Pressure, subst.ConstantProperties.Acentric_Factor, subst.ConstantProperties.Molar_Weight, subst.ConstantProperties.Z_Rackett, P, Me.AUX_PVAPi(subst.Nome, T))
                    Next
                Case Fase.Vapor
                    For Each subst As ClassesBasicasTermodinamica.Substancia In Me.CurrentMaterialStream.Fases(2).Componentes.Values
                        subst.PartialVolume = subst.FracaoMolar.GetValueOrDefault * 8.314 * T / P
                    Next
            End Select

        End Sub

        Public Overrides Function AUX_VAPDENS(ByVal T As Double, ByVal P As Double) As Double
            Dim val As Double
            Dim Z As Double = 1.0#
            val = P / (Z * 8.314 * T) / 1000 * AUX_MMM(Fase.Vapor)
            Return val
        End Function

        Public Overrides Function DW_CalcEntropy(ByVal Vx As System.Array, ByVal T As Double, ByVal P As Double, ByVal st As State) As Double

            Return 0.0#

        End Function

        Public Overrides Function DW_CalcEntropyDeparture(ByVal Vx As System.Array, ByVal T As Double, ByVal P As Double, ByVal st As State) As Double

            Return 0.0#

        End Function

#End Region

#Region "    Auxiliary Functions"

        Function RET_VQ() As Object

            Dim subst As DWSIM.ClassesBasicasTermodinamica.Substancia
            Dim VQ(Me.CurrentMaterialStream.Fases(0).Componentes.Count - 1) As Double
            Dim i As Integer = 0

            For Each subst In Me.CurrentMaterialStream.Fases(0).Componentes.Values
                VQ(i) = subst.ConstantProperties.UNIQUAC_Q
                i += 1
            Next

            Return VQ

        End Function

        Function RET_VR() As Object

            Dim subst As DWSIM.ClassesBasicasTermodinamica.Substancia
            Dim VR(Me.CurrentMaterialStream.Fases(0).Componentes.Count - 1) As Double
            Dim i As Integer = 0

            For Each subst In Me.CurrentMaterialStream.Fases(0).Componentes.Values
                VR(i) = subst.ConstantProperties.UNIQUAC_R
                i += 1
            Next

            Return VR

        End Function

#End Region

#Region "    CalcEquilibrium Override"

        Public Overrides Function DW_CalcEquilibrio_ISOL(spec1 As FlashSpec, spec2 As FlashSpec, val1 As Double, val2 As Double, estimate As Double) As Object

            Dim P, T, H, S, xv, xl, xs, M, W As Double
            Dim result As Dictionary(Of String, Object)
            Dim n As Integer = Me.CurrentMaterialStream.Fases(0).Componentes.Count
            Dim Vx(n - 1), Vy(n - 1) As Double
            Dim i As Integer = 0

            'for TVF/PVF/PH/PS flashes
            xv = Me.CurrentMaterialStream.Fases(2).SPMProperties.molarfraction.GetValueOrDefault
            H = Me.CurrentMaterialStream.Fases(0).SPMProperties.enthalpy.GetValueOrDefault
            S = Me.CurrentMaterialStream.Fases(0).SPMProperties.entropy.GetValueOrDefault

            Dim constprops As New List(Of ConstantProperties)
            For Each su As Substancia In Me.CurrentMaterialStream.Fases(0).Componentes.Values
                constprops.Add(su.ConstantProperties)
            Next

            Me.m_elec = New Auxiliary.Electrolyte
            If Me.ElectrolyteFlash Is Nothing Then Me.ElectrolyteFlash = New Auxiliary.FlashAlgorithms.ElectrolyteSVLE2
            Me.ElectrolyteFlash.CompoundProperties = constprops
            Me.ElectrolyteFlash.proppack = Me

            Select Case spec1

                Case FlashSpec.T

                    Select Case spec2

                        Case FlashSpec.P

                            T = val1
                            P = val2

                            result = Me.ElectrolyteFlash.Flash_PT(RET_VMOL(Fase.Mixture), T, P)

                            xl = result("LiquidPhaseMoleFraction")
                            xv = result("VaporPhaseMoleFraction")
                            xs = result("SolidPhaseMoleFraction")

                            M = result("MoleSum")
                            W = Me.CurrentMaterialStream.Fases(0).SPMProperties.massflow.GetValueOrDefault

                            Dim Vnf = result("MixtureMoleFlows")

                            Vx = result("LiquidPhaseMolarComposition")
                            Vy = result("VaporPhaseMolarComposition")
                            Dim Vs = result("SolidPhaseMolarComposition")

                            Dim HM, HV, HL, HS As Double

                            If xl <> 0 Then HL = Me.DW_CalcEnthalpy(Vx, T, P, State.Liquid)
                            If xs <> 0 Then HS = Me.DW_CalcEnthalpy(Vs, T, P, State.Solid)
                            If xv <> 0 Then HV = Me.DW_CalcEnthalpy(Vy, T, P, State.Vapor)
                            HM = xl * HL + xs * HS + xv * HV

                            H = HM

                            Dim SM, SV, SL, SS As Double

                            If xl <> 0 Then SL = Me.DW_CalcEntropy(Vx, T, P, State.Liquid)
                            If xs <> 0 Then SS = Me.DW_CalcEntropy(Vs, T, P, State.Solid)
                            If xv <> 0 Then SV = Me.DW_CalcEntropy(Vy, T, P, State.Vapor)
                            SM = xl * SL + xs * SS + xv * SV

                            S = SM

                        Case FlashSpec.H

                            Throw New Exception(DWSIM.App.GetLocalString("PropPack_FlashTHNotSupported"))

                        Case FlashSpec.S

                            Throw New Exception(DWSIM.App.GetLocalString("PropPack_FlashTSNotSupported"))

                        Case FlashSpec.VAP

                            Throw New Exception(DWSIM.App.GetLocalString("PropPack_FlashTVNotSupported"))

                    End Select

                Case FlashSpec.P

                    Select Case spec2

                        Case FlashSpec.H

                            If estimate <> 0 Then
                                T = estimate
                            Else
                                T = Me.CurrentMaterialStream.Fases(0).SPMProperties.temperature.GetValueOrDefault
                            End If
                            P = val1
                            H = val2

                            result = Me.ElectrolyteFlash.Flash_PH(RET_VMOL(Fase.Mixture), P, H, T)

                            T = result("Temperature")

                            xl = result("LiquidPhaseMoleFraction")
                            xv = result("VaporPhaseMoleFraction")
                            xs = result("SolidPhaseMoleFraction")

                            M = result("MoleSum")
                            W = Me.CurrentMaterialStream.Fases(0).SPMProperties.massflow.GetValueOrDefault

                            Dim Vnf = result("MixtureMoleFlows")

                            Vx = result("LiquidPhaseMolarComposition")
                            Vy = result("VaporPhaseMolarComposition")
                            Dim Vs = result("SolidPhaseMolarComposition")

                            Dim SM, SV, SL, SS As Double

                            If xl <> 0 Then SL = Me.DW_CalcEntropy(Vx, T, P, State.Liquid)
                            If xs <> 0 Then SS = Me.DW_CalcEntropy(Vs, T, P, State.Solid)
                            If xv <> 0 Then SV = Me.DW_CalcEntropy(Vy, T, P, State.Vapor)
                            SM = xl * SL + xs * SS + xv * SV

                            S = SM

                        Case FlashSpec.S

                            Throw New Exception(DWSIM.App.GetLocalString("PropPack_FlashPSNotSupported"))

                        Case FlashSpec.VAP

                            If estimate <> 0 Then
                                T = estimate
                            Else
                                T = Me.CurrentMaterialStream.Fases(0).SPMProperties.temperature.GetValueOrDefault
                            End If

                            P = val1
                            xv = val2

                            result = Me.ElectrolyteFlash.Flash_PV(RET_VMOL(Fase.Mixture), P, xv, T)

                            T = result("Temperature")

                            xl = result("LiquidPhaseMoleFraction")
                            xv = result("VaporPhaseMoleFraction")
                            xs = result("SolidPhaseMoleFraction")

                            M = result("MoleSum")
                            W = Me.CurrentMaterialStream.Fases(0).SPMProperties.massflow.GetValueOrDefault

                            Dim Vnf = result("MixtureMoleFlows")

                            Vx = result("LiquidPhaseMolarComposition")
                            Vy = result("VaporPhaseMolarComposition")
                            Dim Vs = result("SolidPhaseMolarComposition")

                            Dim HM, HV, HL, HS As Double

                            If xl <> 0 Then HL = Me.DW_CalcEnthalpy(Vx, T, P, State.Liquid)
                            If xs <> 0 Then HS = Me.DW_CalcEnthalpy(Vs, T, P, State.Solid)
                            If xv <> 0 Then HV = Me.DW_CalcEnthalpy(Vy, T, P, State.Vapor)
                            HM = xl * HL + xs * HS + xv * HV

                            H = HM

                            Dim SM, SV, SL, SS As Double

                            If xl <> 0 Then SL = Me.DW_CalcEntropy(Vx, T, P, State.Liquid)
                            If xs <> 0 Then SS = Me.DW_CalcEntropy(Vs, T, P, State.Solid)
                            If xv <> 0 Then SV = Me.DW_CalcEntropy(Vy, T, P, State.Vapor)
                            SM = xl * SL + xs * SS + xv * SV

                            S = SM

                    End Select

            End Select

            Return New Object() {xl, xv, T, P, H, S, 1, 1, Vx, Vy, Nothing}

        End Function

        Public Overrides Sub DW_CalcEquilibrium(spec1 As FlashSpec, spec2 As FlashSpec)

            Me.CurrentMaterialStream.AtEquilibrium = False

            Dim P, T, H, S, xv, xl, xs, M, W, MW As Double
            Dim result As Dictionary(Of String, Object)
            Dim subst As DWSIM.ClassesBasicasTermodinamica.Substancia
            Dim n As Integer = Me.CurrentMaterialStream.Fases(0).Componentes.Count
            Dim i As Integer = 0

            'for TVF/PVF/PH/PS flashes
            xv = Me.CurrentMaterialStream.Fases(2).SPMProperties.molarfraction.GetValueOrDefault
            H = Me.CurrentMaterialStream.Fases(0).SPMProperties.enthalpy.GetValueOrDefault
            S = Me.CurrentMaterialStream.Fases(0).SPMProperties.entropy.GetValueOrDefault

            Me.DW_ZerarPhaseProps(Fase.Vapor)
            Me.DW_ZerarPhaseProps(Fase.Liquid)
            Me.DW_ZerarPhaseProps(Fase.Liquid1)
            Me.DW_ZerarPhaseProps(Fase.Liquid2)
            Me.DW_ZerarPhaseProps(Fase.Liquid3)
            Me.DW_ZerarPhaseProps(Fase.Aqueous)
            Me.DW_ZerarPhaseProps(Fase.Solid)
            Me.DW_ZerarComposicoes(Fase.Vapor)
            Me.DW_ZerarComposicoes(Fase.Liquid)
            Me.DW_ZerarComposicoes(Fase.Liquid1)
            Me.DW_ZerarComposicoes(Fase.Liquid2)
            Me.DW_ZerarComposicoes(Fase.Liquid3)
            Me.DW_ZerarComposicoes(Fase.Aqueous)
            Me.DW_ZerarComposicoes(Fase.Solid)

            Dim constprops As New List(Of ConstantProperties)
            For Each su As Substancia In Me.CurrentMaterialStream.Fases(0).Componentes.Values
                constprops.Add(su.ConstantProperties)
            Next

            Me.m_elec = New Auxiliary.Electrolyte
            If Me.ElectrolyteFlash Is Nothing Then Me.ElectrolyteFlash = New Auxiliary.FlashAlgorithms.ElectrolyteSVLE2
            Me.ElectrolyteFlash.CompoundProperties = constprops
            Me.ElectrolyteFlash.proppack = Me

            Select Case spec1

                Case FlashSpec.T

                    Select Case spec2

                        Case FlashSpec.P

                            T = Me.CurrentMaterialStream.Fases(0).SPMProperties.temperature.GetValueOrDefault
                            P = Me.CurrentMaterialStream.Fases(0).SPMProperties.pressure.GetValueOrDefault

                            result = Me.ElectrolyteFlash.Flash_PT(RET_VMOL(Fase.Mixture), T, P)

                            xl = result("LiquidPhaseMoleFraction")
                            xv = result("VaporPhaseMoleFraction")
                            xs = result("SolidPhaseMoleFraction")

                            Me.CurrentMaterialStream.Fases(3).SPMProperties.molarfraction = xl
                            Me.CurrentMaterialStream.Fases(4).SPMProperties.molarfraction = 0.0#
                            Me.CurrentMaterialStream.Fases(2).SPMProperties.molarfraction = xv
                            Me.CurrentMaterialStream.Fases(7).SPMProperties.molarfraction = xs

                            M = result("MoleSum")
                            W = Me.CurrentMaterialStream.Fases(0).SPMProperties.massflow.GetValueOrDefault

                            Dim Vnf = result("MixtureMoleFlows")

                            'MW = Me.AUX_MMM(Vnf)

                            Me.CurrentMaterialStream.Fases(0).SPMProperties.molarflow *= M '* W / MW * 1000

                            i = 0
                            For Each subst In Me.CurrentMaterialStream.Fases(0).Componentes.Values
                                subst.FracaoMolar = Vnf(i) / M
                                i += 1
                            Next
                            For Each subst In Me.CurrentMaterialStream.Fases(0).Componentes.Values
                                subst.FracaoMassica = Me.AUX_CONVERT_MOL_TO_MASS(subst.Nome, 0)
                            Next

                            Dim Vx = result("LiquidPhaseMolarComposition")
                            Dim Vy = result("VaporPhaseMolarComposition")
                            Dim Vs = result("SolidPhaseMolarComposition")

                            Dim ACL = result("LiquidPhaseActivityCoefficients")

                            i = 0
                            For Each subst In Me.CurrentMaterialStream.Fases(3).Componentes.Values
                                subst.FracaoMolar = Vx(i)
                                subst.FugacityCoeff = 0
                                subst.ActivityCoeff = ACL(i)
                                subst.PartialVolume = 0
                                subst.PartialPressure = 0
                                i += 1
                            Next
                            For Each subst In Me.CurrentMaterialStream.Fases(3).Componentes.Values
                                subst.FracaoMassica = Me.AUX_CONVERT_MOL_TO_MASS(subst.Nome, 3)
                            Next
                            i = 0
                            For Each subst In Me.CurrentMaterialStream.Fases(7).Componentes.Values
                                subst.FracaoMolar = Vs(i)
                                subst.FugacityCoeff = 0
                                subst.ActivityCoeff = 0
                                subst.PartialVolume = 0
                                subst.PartialPressure = 0
                                i += 1
                            Next
                            For Each subst In Me.CurrentMaterialStream.Fases(7).Componentes.Values
                                subst.FracaoMassica = Me.AUX_CONVERT_MOL_TO_MASS(subst.Nome, 7)
                            Next
                            i = 0
                            For Each subst In Me.CurrentMaterialStream.Fases(2).Componentes.Values
                                subst.FracaoMolar = Vy(i)
                                subst.FugacityCoeff = 1.0#
                                subst.ActivityCoeff = 0
                                subst.PartialVolume = 0
                                subst.PartialPressure = 0
                                i += 1
                            Next
                            For Each subst In Me.CurrentMaterialStream.Fases(2).Componentes.Values
                                subst.FracaoMassica = Me.AUX_CONVERT_MOL_TO_MASS(subst.Nome, 2)
                            Next

                            Me.CurrentMaterialStream.Fases(3).SPMProperties.massfraction = xl * Me.AUX_MMM(Fase.Liquid1) / (xl * Me.AUX_MMM(Fase.Liquid1) + xs * Me.AUX_MMM(Fase.Solid) + xv * Me.AUX_MMM(Fase.Vapor))
                            Me.CurrentMaterialStream.Fases(4).SPMProperties.massfraction = 0.0#
                            Me.CurrentMaterialStream.Fases(7).SPMProperties.massfraction = xs * Me.AUX_MMM(Fase.Solid) / (xl * Me.AUX_MMM(Fase.Liquid1) + xs * Me.AUX_MMM(Fase.Solid) + xv * Me.AUX_MMM(Fase.Vapor))
                            Me.CurrentMaterialStream.Fases(2).SPMProperties.massfraction = xv * Me.AUX_MMM(Fase.Vapor) / (xl * Me.AUX_MMM(Fase.Liquid1) + xs * Me.AUX_MMM(Fase.Solid) + xv * Me.AUX_MMM(Fase.Vapor))

                            Dim HM, HV, HL, HS As Double

                            If xl <> 0 Then HL = Me.DW_CalcEnthalpy(Vx, T, P, State.Liquid)
                            If xs <> 0 Then HS = Me.DW_CalcEnthalpy(Vs, T, P, State.Solid)
                            If xv <> 0 Then HV = Me.DW_CalcEnthalpy(Vy, T, P, State.Vapor)
                            HM = Me.CurrentMaterialStream.Fases(3).SPMProperties.massfraction.GetValueOrDefault * HL + Me.CurrentMaterialStream.Fases(7).SPMProperties.massfraction.GetValueOrDefault * HS + Me.CurrentMaterialStream.Fases(2).SPMProperties.massfraction.GetValueOrDefault * HV

                            H = HM

                            Dim SM, SV, SL, SS As Double

                            If xl <> 0 Then SL = Me.DW_CalcEntropy(Vx, T, P, State.Liquid)
                            If xs <> 0 Then SS = Me.DW_CalcEntropy(Vs, T, P, State.Solid)
                            If xv <> 0 Then SV = Me.DW_CalcEntropy(Vy, T, P, State.Vapor)
                            SM = Me.CurrentMaterialStream.Fases(3).SPMProperties.massfraction.GetValueOrDefault * SL + Me.CurrentMaterialStream.Fases(7).SPMProperties.massfraction.GetValueOrDefault * SS + Me.CurrentMaterialStream.Fases(2).SPMProperties.massfraction.GetValueOrDefault * SV

                            S = SM

                        Case FlashSpec.H

                            Throw New Exception(DWSIM.App.GetLocalString("PropPack_FlashTHNotSupported"))

                        Case FlashSpec.S

                            Throw New Exception(DWSIM.App.GetLocalString("PropPack_FlashTSNotSupported"))

                        Case FlashSpec.VAP

                            Throw New Exception(DWSIM.App.GetLocalString("PropPack_FlashTVNotSupported"))

                    End Select

                Case FlashSpec.P

                    Select Case spec2

                        Case FlashSpec.H

                            T = Me.CurrentMaterialStream.Fases(0).SPMProperties.temperature.GetValueOrDefault
                            H = Me.CurrentMaterialStream.Fases(0).SPMProperties.enthalpy.GetValueOrDefault
                            P = Me.CurrentMaterialStream.Fases(0).SPMProperties.pressure.GetValueOrDefault

                            result = Me.ElectrolyteFlash.Flash_PH(RET_VMOL(Fase.Mixture), P, H, T)

                            T = result("Temperature")

                            xl = result("LiquidPhaseMoleFraction")
                            xv = result("VaporPhaseMoleFraction")
                            xs = result("SolidPhaseMoleFraction")

                            Me.CurrentMaterialStream.Fases(3).SPMProperties.molarfraction = xl
                            Me.CurrentMaterialStream.Fases(4).SPMProperties.molarfraction = 0.0#
                            Me.CurrentMaterialStream.Fases(2).SPMProperties.molarfraction = xv
                            Me.CurrentMaterialStream.Fases(7).SPMProperties.molarfraction = xs

                            M = result("MoleSum")
                            W = Me.CurrentMaterialStream.Fases(0).SPMProperties.massflow.GetValueOrDefault

                            Dim Vnf = result("MixtureMoleFlows")

                            MW = Me.AUX_MMM(Vnf)

                            Me.CurrentMaterialStream.Fases(0).SPMProperties.molarflow = M * W / MW * 1000

                            i = 0
                            For Each subst In Me.CurrentMaterialStream.Fases(0).Componentes.Values
                                subst.FracaoMolar = Vnf(i) / M
                                i += 1
                            Next
                            For Each subst In Me.CurrentMaterialStream.Fases(7).Componentes.Values
                                subst.FracaoMassica = Me.AUX_CONVERT_MOL_TO_MASS(subst.Nome, 0)
                            Next

                            Dim Vx = result("LiquidPhaseMolarComposition")
                            Dim Vy = result("VaporPhaseMolarComposition")
                            Dim Vs = result("SolidPhaseMolarComposition")

                            Dim ACL = result("LiquidPhaseActivityCoefficients")

                            i = 0
                            For Each subst In Me.CurrentMaterialStream.Fases(3).Componentes.Values
                                subst.FracaoMolar = Vx(i)
                                subst.FugacityCoeff = 0
                                subst.ActivityCoeff = ACL(i)
                                subst.PartialVolume = 0
                                subst.PartialPressure = 0
                                i += 1
                            Next
                            For Each subst In Me.CurrentMaterialStream.Fases(3).Componentes.Values
                                subst.FracaoMassica = Me.AUX_CONVERT_MOL_TO_MASS(subst.Nome, 3)
                            Next
                            i = 0
                            For Each subst In Me.CurrentMaterialStream.Fases(7).Componentes.Values
                                subst.FracaoMolar = Vs(i)
                                subst.FugacityCoeff = 0
                                subst.ActivityCoeff = 0
                                subst.PartialVolume = 0
                                subst.PartialPressure = 0
                                i += 1
                            Next
                            For Each subst In Me.CurrentMaterialStream.Fases(7).Componentes.Values
                                subst.FracaoMassica = Me.AUX_CONVERT_MOL_TO_MASS(subst.Nome, 7)
                            Next
                            i = 0
                            For Each subst In Me.CurrentMaterialStream.Fases(2).Componentes.Values
                                subst.FracaoMolar = Vy(i)
                                subst.FugacityCoeff = 1.0#
                                subst.ActivityCoeff = 0
                                subst.PartialVolume = 0
                                subst.PartialPressure = 0
                                i += 1
                            Next
                            For Each subst In Me.CurrentMaterialStream.Fases(2).Componentes.Values
                                subst.FracaoMassica = Me.AUX_CONVERT_MOL_TO_MASS(subst.Nome, 2)
                            Next

                            Me.CurrentMaterialStream.Fases(3).SPMProperties.massfraction = xl * Me.AUX_MMM(Fase.Liquid1) / (xl * Me.AUX_MMM(Fase.Liquid1) + xs * Me.AUX_MMM(Fase.Solid) + xv * Me.AUX_MMM(Fase.Vapor))
                            Me.CurrentMaterialStream.Fases(4).SPMProperties.massfraction = 0.0#
                            Me.CurrentMaterialStream.Fases(7).SPMProperties.massfraction = xs * Me.AUX_MMM(Fase.Solid) / (xl * Me.AUX_MMM(Fase.Liquid1) + xs * Me.AUX_MMM(Fase.Solid) + xv * Me.AUX_MMM(Fase.Vapor))
                            Me.CurrentMaterialStream.Fases(2).SPMProperties.massfraction = xv * Me.AUX_MMM(Fase.Vapor) / (xl * Me.AUX_MMM(Fase.Liquid1) + xs * Me.AUX_MMM(Fase.Solid) + xv * Me.AUX_MMM(Fase.Vapor))

                            Dim SM, SV, SL, SS As Double

                            If xl <> 0 Then SL = Me.DW_CalcEntropy(Vx, T, P, State.Liquid)
                            If xs <> 0 Then SS = Me.DW_CalcEntropy(Vs, T, P, State.Solid)
                            If xv <> 0 Then SV = Me.DW_CalcEntropy(Vy, T, P, State.Vapor)
                            SM = Me.CurrentMaterialStream.Fases(3).SPMProperties.massfraction.GetValueOrDefault * SL + Me.CurrentMaterialStream.Fases(7).SPMProperties.massfraction.GetValueOrDefault * SS + Me.CurrentMaterialStream.Fases(2).SPMProperties.massfraction.GetValueOrDefault * SV

                            S = SM

                        Case FlashSpec.S

                            Throw New Exception(DWSIM.App.GetLocalString("PropPack_FlashPSNotSupported"))

                        Case FlashSpec.VAP

                            result = Me.ElectrolyteFlash.Flash_PV(RET_VMOL(Fase.Mixture), P, xv, T)

                            T = result("Temperature")

                            xl = result("LiquidPhaseMoleFraction")
                            xv = result("VaporPhaseMoleFraction")
                            xs = result("SolidPhaseMoleFraction")

                            Me.CurrentMaterialStream.Fases(3).SPMProperties.molarfraction = xl
                            Me.CurrentMaterialStream.Fases(4).SPMProperties.molarfraction = 0.0#
                            Me.CurrentMaterialStream.Fases(2).SPMProperties.molarfraction = xv
                            Me.CurrentMaterialStream.Fases(7).SPMProperties.molarfraction = xs

                            M = result("MoleSum")
                            W = Me.CurrentMaterialStream.Fases(0).SPMProperties.massflow.GetValueOrDefault

                            Dim Vnf = result("MixtureMoleFlows")

                            MW = Me.AUX_MMM(Vnf)

                            Me.CurrentMaterialStream.Fases(0).SPMProperties.molarflow = M * W / MW * 1000

                            i = 0
                            For Each subst In Me.CurrentMaterialStream.Fases(0).Componentes.Values
                                subst.FracaoMolar = Vnf(i) / M
                                i += 1
                            Next
                            For Each subst In Me.CurrentMaterialStream.Fases(7).Componentes.Values
                                subst.FracaoMassica = Me.AUX_CONVERT_MOL_TO_MASS(subst.Nome, 0)
                            Next

                            Dim Vx = result("LiquidPhaseMolarComposition")
                            Dim Vy = result("VaporPhaseMolarComposition")
                            Dim Vs = result("SolidPhaseMolarComposition")

                            Dim ACL = result("LiquidPhaseActivityCoefficients")

                            i = 0
                            For Each subst In Me.CurrentMaterialStream.Fases(3).Componentes.Values
                                subst.FracaoMolar = Vx(i)
                                subst.FugacityCoeff = 0
                                subst.ActivityCoeff = ACL(i)
                                subst.PartialVolume = 0
                                subst.PartialPressure = 0
                                i += 1
                            Next
                            For Each subst In Me.CurrentMaterialStream.Fases(3).Componentes.Values
                                subst.FracaoMassica = Me.AUX_CONVERT_MOL_TO_MASS(subst.Nome, 3)
                            Next
                            i = 0
                            For Each subst In Me.CurrentMaterialStream.Fases(7).Componentes.Values
                                subst.FracaoMolar = Vs(i)
                                subst.FugacityCoeff = 0
                                subst.ActivityCoeff = 0
                                subst.PartialVolume = 0
                                subst.PartialPressure = 0
                                i += 1
                            Next
                            For Each subst In Me.CurrentMaterialStream.Fases(7).Componentes.Values
                                subst.FracaoMassica = Me.AUX_CONVERT_MOL_TO_MASS(subst.Nome, 7)
                            Next
                            i = 0
                            For Each subst In Me.CurrentMaterialStream.Fases(2).Componentes.Values
                                subst.FracaoMolar = Vy(i)
                                subst.FugacityCoeff = 1.0#
                                subst.ActivityCoeff = 0
                                subst.PartialVolume = 0
                                subst.PartialPressure = 0
                                i += 1
                            Next
                            For Each subst In Me.CurrentMaterialStream.Fases(2).Componentes.Values
                                subst.FracaoMassica = Me.AUX_CONVERT_MOL_TO_MASS(subst.Nome, 2)
                            Next

                            Me.CurrentMaterialStream.Fases(3).SPMProperties.massfraction = xl * Me.AUX_MMM(Fase.Liquid1) / (xl * Me.AUX_MMM(Fase.Liquid1) + xs * Me.AUX_MMM(Fase.Solid) + xv * Me.AUX_MMM(Fase.Vapor))
                            Me.CurrentMaterialStream.Fases(4).SPMProperties.massfraction = 0.0#
                            Me.CurrentMaterialStream.Fases(7).SPMProperties.massfraction = xs * Me.AUX_MMM(Fase.Solid) / (xl * Me.AUX_MMM(Fase.Liquid1) + xs * Me.AUX_MMM(Fase.Solid) + xv * Me.AUX_MMM(Fase.Vapor))
                            Me.CurrentMaterialStream.Fases(2).SPMProperties.massfraction = xv * Me.AUX_MMM(Fase.Vapor) / (xl * Me.AUX_MMM(Fase.Liquid1) + xs * Me.AUX_MMM(Fase.Solid) + xv * Me.AUX_MMM(Fase.Vapor))

                            Dim SM, SV, SL, SS As Double

                            If xl <> 0 Then SL = Me.DW_CalcEntropy(Vx, T, P, State.Liquid)
                            If xs <> 0 Then SS = Me.DW_CalcEntropy(Vs, T, P, State.Solid)
                            If xv <> 0 Then SV = Me.DW_CalcEntropy(Vy, T, P, State.Vapor)
                            SM = Me.CurrentMaterialStream.Fases(3).SPMProperties.massfraction.GetValueOrDefault * SL + Me.CurrentMaterialStream.Fases(7).SPMProperties.massfraction.GetValueOrDefault * SS + Me.CurrentMaterialStream.Fases(2).SPMProperties.massfraction.GetValueOrDefault * SV

                            S = SM

                            Dim HM, HV, HL, HS As Double

                            If xl <> 0 Then HL = Me.DW_CalcEnthalpy(Vx, T, P, State.Liquid)
                            If xs <> 0 Then HS = Me.DW_CalcEnthalpy(Vs, T, P, State.Solid)
                            If xv <> 0 Then HV = Me.DW_CalcEnthalpy(Vy, T, P, State.Vapor)
                            HM = Me.CurrentMaterialStream.Fases(3).SPMProperties.massfraction.GetValueOrDefault * HL + Me.CurrentMaterialStream.Fases(7).SPMProperties.massfraction.GetValueOrDefault * HS + Me.CurrentMaterialStream.Fases(2).SPMProperties.massfraction.GetValueOrDefault * HV

                            H = HM

                    End Select

            End Select

            Dim summf As Double = 0, sumwf As Double = 0
            For Each pi As PhaseInfo In Me.PhaseMappings.Values
                If Not pi.PhaseLabel = "Disabled" Then
                    summf += Me.CurrentMaterialStream.Fases(pi.DWPhaseIndex).SPMProperties.molarfraction.GetValueOrDefault
                    sumwf += Me.CurrentMaterialStream.Fases(pi.DWPhaseIndex).SPMProperties.massfraction.GetValueOrDefault
                End If
            Next
            If Abs(summf - 1) > 0.000001 Then
                For Each pi As PhaseInfo In Me.PhaseMappings.Values
                    If Not pi.PhaseLabel = "Disabled" Then
                        If Not Me.CurrentMaterialStream.Fases(pi.DWPhaseIndex).SPMProperties.molarfraction.HasValue Then
                            Me.CurrentMaterialStream.Fases(pi.DWPhaseIndex).SPMProperties.molarfraction = 1 - summf
                            Me.CurrentMaterialStream.Fases(pi.DWPhaseIndex).SPMProperties.massfraction = 1 - sumwf
                        End If
                    End If
                Next
            End If

            With Me.CurrentMaterialStream

                .Fases(0).SPMProperties.temperature = T
                .Fases(0).SPMProperties.pressure = P
                .Fases(0).SPMProperties.enthalpy = H
                .Fases(0).SPMProperties.entropy = S

            End With

            Me.CurrentMaterialStream.AtEquilibrium = True


        End Sub

#End Region

#Region "    XML Load/Save Override"

        Public Overrides Function LoadData(data As System.Collections.Generic.List(Of System.Xml.Linq.XElement)) As Boolean

            MyBase.LoadData(data)

            Me.ElectrolyteFlash = New DWSIM.SimulationObjects.PropertyPackages.Auxiliary.FlashAlgorithms.ElectrolyteSVLE2

            Dim xel0 As XElement = (From xelv As XElement In data Where xelv.Name = "ElectrolyteFlash_ReactionSetID").SingleOrDefault
            If Not xel0 Is Nothing Then Me.ElectrolyteFlash.ReactionSet = xel0.Value

            Dim xel As XElement = (From xelv As XElement In data Where xelv.Name = "ElectrolyteFlash_CalculateChemicalEquilibria").SingleOrDefault
            If Not xel Is Nothing Then Me.ElectrolyteFlash.CalculateChemicalEquilibria = xel.Value

            Dim xel2 As XElement = (From xelv As XElement In data Where xelv.Name = "ElectrolyteFlash_Tolerance").SingleOrDefault
            If Not xel2 Is Nothing Then Me.ElectrolyteFlash.Tolerance = xel2.Value

            Dim xel3 As XElement = (From xelv As XElement In data Where xelv.Name = "ElectrolyteFlash_MaximumIterations").SingleOrDefault
            If Not xel3 Is Nothing Then Me.ElectrolyteFlash.MaximumIterations = xel3.Value

            xel = (From xelv As XElement In data Where xelv.Name = "ElectrolyteFlash_NumericalDerivativePerturbation").SingleOrDefault
            If Not xel Is Nothing Then Me.ElectrolyteFlash.NumericalDerivativePerturbation = xel.Value

            xel = (From xelv As XElement In data Where xelv.Name = "ElectrolyteFlash_ObjFuncGibbsWeight").SingleOrDefault
            If Not xel Is Nothing Then Me.ElectrolyteFlash.ObjFuncGibbsWeight = xel.Value

            xel = (From xelv As XElement In data Where xelv.Name = "ElectrolyteFlash_ObjFuncChemEqWeight").SingleOrDefault
            If Not xel Is Nothing Then Me.ElectrolyteFlash.ObjFuncChemEqWeight = xel.Value

            xel = (From xelv As XElement In data Where xelv.Name = "ElectrolyteFlash_ObjFuncMassBalWeight").SingleOrDefault
            If Not xel Is Nothing Then Me.ElectrolyteFlash.ObjFuncMassBalWeight = xel.Value

            xel = (From xelv As XElement In data Where xelv.Name = "ElectrolyteFlash_LinearSolver").SingleOrDefault
            If Not xel Is Nothing Then Me.ElectrolyteFlash.LinearSolver = xel.Value

        End Function

        Public Overrides Function SaveData() As System.Collections.Generic.List(Of System.Xml.Linq.XElement)

            Dim elements As System.Collections.Generic.List(Of System.Xml.Linq.XElement) = MyBase.SaveData()
            Dim ci As Globalization.CultureInfo = Globalization.CultureInfo.InvariantCulture

            With elements
                .Add(New XElement("ElectrolyteFlash_ReactionSetID", Me.ElectrolyteFlash.ReactionSet))
                .Add(New XElement("ElectrolyteFlash_CalculateChemicalEquilibria", Me.ElectrolyteFlash.CalculateChemicalEquilibria))
                .Add(New XElement("ElectrolyteFlash_Tolerance", Me.ElectrolyteFlash.Tolerance))
                .Add(New XElement("ElectrolyteFlash_MaximumIterations", Me.ElectrolyteFlash.MaximumIterations))
                .Add(New XElement("ElectrolyteFlash_NumericalDerivativePerturbation", Me.ElectrolyteFlash.NumericalDerivativePerturbation))
                .Add(New XElement("ElectrolyteFlash_ObjFuncGibbsWeight", Me.ElectrolyteFlash.ObjFuncGibbsWeight))
                .Add(New XElement("ElectrolyteFlash_ObjFuncChemEqWeight", Me.ElectrolyteFlash.ObjFuncChemEqWeight))
                .Add(New XElement("ElectrolyteFlash_ObjFuncMassBalWeight", Me.ElectrolyteFlash.ObjFuncMassBalWeight))
                .Add(New XElement("ElectrolyteFlash_LinearSolver", Me.ElectrolyteFlash.LinearSolver))
            End With

            Return elements

        End Function

#End Region

    End Class

End Namespace
