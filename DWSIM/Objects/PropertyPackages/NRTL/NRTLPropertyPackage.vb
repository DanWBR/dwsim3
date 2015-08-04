'    NRTL Property Package 
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

Imports DWSIM.DWSIM.SimulationObjects.PropertyPackages
Imports System.Math
Imports DWSIM.DWSIM.ClassesBasicasTermodinamica

Namespace DWSIM.SimulationObjects.PropertyPackages

    <System.Runtime.InteropServices.Guid(NRTLPropertyPackage.ClassId)> _
   <System.Serializable()> Public Class NRTLPropertyPackage

        Inherits DWSIM.SimulationObjects.PropertyPackages.ActivityCoefficientPropertyPackage

        Public Shadows Const ClassId As String = "D42F0157-5750-4c89-A94E-634A04701568"

        Public m_uni As New DWSIM.SimulationObjects.PropertyPackages.Auxiliary.NRTL

        Public Sub New(ByVal comode As Boolean)
            MyBase.New(comode)
        End Sub

        Public Sub New()

            MyBase.New(False)

            Me.IsConfigurable = True
            Me.ConfigForm = New FormConfigNRTL
            Me._packagetype = PropertyPackages.PackageType.ActivityCoefficient

        End Sub

        Public Overrides Sub ReconfigureConfigForm()
            MyBase.ReconfigureConfigForm()
            Me.ConfigForm = New FormConfigNRTL
        End Sub


#Region "    DWSIM Functions"

        Public Overrides Sub DW_CalcProp(ByVal [property] As String, ByVal phase As Fase)

            Dim result As Double = 0.0#
            Dim resultObj As Object = Nothing
            Dim phaseID As Integer = -1
            Dim state As String = ""

            Dim T, P As Double
            T = Me.CurrentMaterialStream.Fases(0).SPMProperties.temperature.GetValueOrDefault
            P = Me.CurrentMaterialStream.Fases(0).SPMProperties.pressure.GetValueOrDefault

            Select Case phase
                Case Fase.Vapor
                    state = "V"
                Case Fase.Liquid, Fase.Liquid1, Fase.Liquid2, Fase.Liquid3
                    state = "L"
            End Select

            Select Case phase
                Case PropertyPackages.Fase.Mixture
                    phaseID = 0
                Case PropertyPackages.Fase.Vapor
                    phaseID = 2
                Case PropertyPackages.Fase.Liquid1
                    phaseID = 3
                Case PropertyPackages.Fase.Liquid2
                    phaseID = 4
                Case PropertyPackages.Fase.Liquid3
                    phaseID = 5
                Case PropertyPackages.Fase.Liquid
                    phaseID = 1
                Case PropertyPackages.Fase.Aqueous
                    phaseID = 6
                Case PropertyPackages.Fase.Solid
                    phaseID = 7
            End Select

            Me.CurrentMaterialStream.Fases(phaseID).SPMProperties.molecularWeight = Me.AUX_MMM(phase)

            Select Case [property].ToLower
                Case "compressibilityfactor"
                    result = Me.m_lk.Z_LK(state, T / Me.AUX_TCM(phase), P / Me.AUX_PCM(phase), Me.AUX_WM(phase))(0)
                    Me.CurrentMaterialStream.Fases(phaseID).SPMProperties.compressibilityFactor = result
                Case "heatcapacity", "heatcapacitycp"
                    If state = "V" Then
                        resultObj = Me.m_lk.CpCvR_LK(state, T, P, RET_VMOL(phase), RET_VKij(), RET_VMAS(phase), RET_VTC(), RET_VPC(), RET_VCP(T), RET_VMM(), RET_VW(), RET_VZRa())
                        result = resultObj(1)
                    Else
                        Select Case Me.Parameters("PP_ENTH_CP_CALC_METHOD")
                            Case 0 'LK
                                resultObj = Me.m_lk.CpCvR_LK(state, T, P, RET_VMOL(phase), RET_VKij(), RET_VMAS(phase), RET_VTC(), RET_VPC(), RET_VCP(T), RET_VMM(), RET_VW(), RET_VZRa())
                                result = resultObj(1)
                            Case 1 'Ideal
                                result = Me.AUX_LIQCPm(T, phaseID)
                            Case 2 'Excess
                                result = Me.AUX_LIQCPm(T, phaseID) + Me.m_uni.CPEX_MIX(T, RET_VMOL(phase), Me.RET_VNAMES) / Me.AUX_MMM(phase)
                        End Select
                    End If
                    Me.CurrentMaterialStream.Fases(phaseID).SPMProperties.heatCapacityCp = result
                Case "heatcapacitycv"
                    If state = "V" Then
                        resultObj = Me.m_lk.CpCvR_LK(state, T, P, RET_VMOL(phase), RET_VKij(), RET_VMAS(phase), RET_VTC(), RET_VPC(), RET_VCP(T), RET_VMM(), RET_VW(), RET_VZRa())
                        result = resultObj(2)
                    Else
                        Select Case Me.Parameters("PP_ENTH_CP_CALC_METHOD")
                            Case 0 'LK
                                resultObj = Me.m_lk.CpCvR_LK(state, T, P, RET_VMOL(phase), RET_VKij(), RET_VMAS(phase), RET_VTC(), RET_VPC(), RET_VCP(T), RET_VMM(), RET_VW(), RET_VZRa())
                                result = resultObj(2)
                            Case 1 'Ideal
                                result = Me.AUX_LIQCPm(T, phaseID)
                            Case 2 'Excess
                                result = Me.AUX_LIQCPm(T, phaseID) + Me.m_uni.CPEX_MIX(T, RET_VMOL(phase), Me.RET_VNAMES) / Me.AUX_MMM(phase)
                        End Select
                    End If
                    Me.CurrentMaterialStream.Fases(phaseID).SPMProperties.heatCapacityCv = result
                Case "enthalpy", "enthalpynf"
                    If state = "V" Then
                        result = Me.m_lk.H_LK_MIX(state, T, P, RET_VMOL(phase), RET_VKij, RET_VTC(), RET_VPC(), RET_VW(), RET_VMM(), Me.RET_Hid(298.15, T, phase))
                    Else
                        Select Case Me.Parameters("PP_ENTH_CP_CALC_METHOD")
                            Case 0 'LK
                                result = Me.m_lk.H_LK_MIX(state, T, P, RET_VMOL(phase), RET_VKij, RET_VTC(), RET_VPC(), RET_VW(), RET_VMM(), Me.RET_Hid(298.15, T, phase))
                            Case 1 'Ideal
                                result = Me.RET_Hid_L(298.15, T, RET_VMOL(phase)) - Me.RET_HVAPM(RET_VMAS(phase), T)
                            Case 2 'Excess
                                result = Me.RET_Hid_L(298.15, T, RET_VMOL(phase)) + Me.m_uni.HEX_MIX(T, RET_VMOL(phase), Me.RET_VNAMES) / Me.AUX_MMM(phase) - Me.RET_HVAPM(RET_VMAS(phase), T)
                        End Select
                    End If
                    Me.CurrentMaterialStream.Fases(phaseID).SPMProperties.enthalpy = result
                    result = Me.CurrentMaterialStream.Fases(phaseID).SPMProperties.enthalpy.GetValueOrDefault * Me.CurrentMaterialStream.Fases(phaseID).SPMProperties.molecularWeight.GetValueOrDefault
                    Me.CurrentMaterialStream.Fases(phaseID).SPMProperties.molar_enthalpy = result
                Case "entropy", "entropynf"
                    If state = "V" Then
                        result = Me.m_lk.S_LK_MIX(state, T, P, RET_VMOL(phase), RET_VKij, RET_VTC(), RET_VPC(), RET_VW(), RET_VMM(), Me.RET_Sid(298.15, T, P, phase))
                    Else
                        Select Case Me.Parameters("PP_ENTH_CP_CALC_METHOD")
                            Case 0 'LK
                                result = Me.m_lk.S_LK_MIX(state, T, P, RET_VMOL(phase), RET_VKij, RET_VTC(), RET_VPC(), RET_VW(), RET_VMM(), Me.RET_Sid(298.15, T, P, phase))
                            Case 1 'Ideal
                                result = Me.RET_Hid_L(298.15, T, RET_VMOL(phase)) / T - Me.RET_HVAPM(RET_VMAS(phase), T) / T
                            Case 2 'Excess
                                result = (Me.RET_Hid_L(298.15, T, RET_VMOL(phase)) + Me.m_uni.HEX_MIX(T, RET_VMOL(phase), Me.RET_VNAMES)) / Me.AUX_MMM(phase) / T - Me.RET_HVAPM(RET_VMAS(phase), T) / T
                        End Select
                    End If
                    Me.CurrentMaterialStream.Fases(phaseID).SPMProperties.entropy = result
                    result = Me.CurrentMaterialStream.Fases(phaseID).SPMProperties.entropy.GetValueOrDefault * Me.CurrentMaterialStream.Fases(phaseID).SPMProperties.molecularWeight.GetValueOrDefault
                    Me.CurrentMaterialStream.Fases(phaseID).SPMProperties.molar_entropy = result
                Case "excessenthalpy"
                    If state = "V" Then
                        result = Me.m_lk.H_LK_MIX(state, T, P, RET_VMOL(phase), RET_VKij, RET_VTC(), RET_VPC(), RET_VW(), RET_VMM(), 0)
                    Else
                        Select Case Me.Parameters("PP_ENTH_CP_CALC_METHOD")
                            Case 0 'LK
                                result = Me.m_lk.H_LK_MIX(state, T, P, RET_VMOL(phase), RET_VKij, RET_VTC(), RET_VPC(), RET_VW(), RET_VMM(), 0)
                            Case 1 'Ideal
                                result = 0.0#
                            Case 2 'Excess
                                result = Me.m_uni.HEX_MIX(T, RET_VMOL(phase), Me.RET_VNAMES) / Me.AUX_MMM(phase)
                        End Select
                    End If
                    Me.CurrentMaterialStream.Fases(phaseID).SPMProperties.excessEnthalpy = result
                Case "excessentropy"
                    If state = "V" Then
                        result = Me.m_lk.S_LK_MIX(state, T, P, RET_VMOL(phase), RET_VKij, RET_VTC(), RET_VPC(), RET_VW(), RET_VMM(), 0)
                    Else
                        Select Case Me.Parameters("PP_ENTH_CP_CALC_METHOD")
                            Case 0 'LK
                                result = Me.m_lk.S_LK_MIX(state, T, P, RET_VMOL(phase), RET_VKij, RET_VTC(), RET_VPC(), RET_VW(), RET_VMM(), 0)
                            Case 1 'Ideal
                                result = 0.0#
                            Case 2 'Excess
                                result = Me.m_uni.HEX_MIX(T, RET_VMOL(phase), Me.RET_VNAMES) / Me.AUX_MMM(phase) / T
                        End Select
                    End If
                    Me.CurrentMaterialStream.Fases(phaseID).SPMProperties.excessEntropy = result
                Case "enthalpyf"
                    Dim entF As Double = Me.AUX_HFm25(phase)
                    result = Me.m_lk.H_LK_MIX(state, T, P, RET_VMOL(phase), RET_VKij, RET_VTC(), RET_VPC(), RET_VW(), RET_VMM(), Me.RET_Hid(298.15, T, phase))
                    Me.CurrentMaterialStream.Fases(phaseID).SPMProperties.enthalpyF = result + entF
                    result = Me.CurrentMaterialStream.Fases(phaseID).SPMProperties.enthalpyF.GetValueOrDefault * Me.CurrentMaterialStream.Fases(phaseID).SPMProperties.molecularWeight.GetValueOrDefault
                    Me.CurrentMaterialStream.Fases(phaseID).SPMProperties.molar_enthalpyF = result
                Case "entropyf"
                    Dim entF As Double = Me.AUX_SFm25(phase)
                    result = Me.m_lk.S_LK_MIX(state, T, P, RET_VMOL(phase), RET_VKij, RET_VTC(), RET_VPC(), RET_VW(), RET_VMM(), Me.RET_Sid(298.15, T, P, phase))
                    Me.CurrentMaterialStream.Fases(phaseID).SPMProperties.entropyF = result + entF
                    result = Me.CurrentMaterialStream.Fases(phaseID).SPMProperties.entropyF.GetValueOrDefault * Me.CurrentMaterialStream.Fases(phaseID).SPMProperties.molecularWeight.GetValueOrDefault
                    Me.CurrentMaterialStream.Fases(phaseID).SPMProperties.molar_entropyF = result
                Case "viscosity"
                    If state = "L" Then
                        result = Me.AUX_LIQVISCm(T)
                    Else
                        result = Me.AUX_VAPVISCm(T, Me.CurrentMaterialStream.Fases(phaseID).SPMProperties.density.GetValueOrDefault, Me.AUX_MMM(phase))
                    End If
                    Me.CurrentMaterialStream.Fases(phaseID).SPMProperties.viscosity = result
                Case "thermalconductivity"
                    If state = "L" Then
                        result = Me.AUX_CONDTL(T)
                    Else
                        result = Me.AUX_CONDTG(T, P)
                    End If
                    Me.CurrentMaterialStream.Fases(phaseID).SPMProperties.thermalConductivity = result
                Case "fugacity", "fugacitycoefficient", "logfugacitycoefficient", "activity", "activitycoefficient"
                    Me.DW_CalcCompFugCoeff(phase)
                Case "volume", "density"
                    If state = "L" Then
                        result = Me.AUX_LIQDENS(T, P, 0.0#, phaseID, False)
                    Else
                        result = Me.AUX_VAPDENS(T, P)
                    End If
                    Me.CurrentMaterialStream.Fases(phaseID).SPMProperties.density = result
                Case "surfacetension"
                    Me.CurrentMaterialStream.Fases(0).TPMProperties.surfaceTension = Me.AUX_SURFTM(T)
                Case Else
                    Dim ex As Exception = New CapeOpen.CapeThrmPropertyNotAvailableException
                    ThrowCAPEException(ex, "Error", ex.Message, "ICapeThermoMaterial", ex.Source, ex.StackTrace, "CalcSinglePhaseProp/CalcTwoPhaseProp/CalcProp", ex.GetHashCode)
            End Select

        End Sub

        Public Overrides Sub DW_CalcPhaseProps(ByVal fase As DWSIM.SimulationObjects.PropertyPackages.Fase)

            Dim result As Double
            Dim resultObj As Object
            Dim dwpl As Fase

            Dim T, P As Double
            Dim phasemolarfrac As Double = Nothing
            Dim overallmolarflow As Double = Nothing

            Dim phaseID As Integer
            T = Me.CurrentMaterialStream.Fases(0).SPMProperties.temperature.GetValueOrDefault
            P = Me.CurrentMaterialStream.Fases(0).SPMProperties.pressure.GetValueOrDefault

            Select Case fase
                Case PropertyPackages.Fase.Mixture
                    phaseID = 0
                    dwpl = PropertyPackages.Fase.Mixture
                Case PropertyPackages.Fase.Vapor
                    phaseID = 2
                    dwpl = PropertyPackages.Fase.Vapor
                Case PropertyPackages.Fase.Liquid1
                    phaseID = 3
                    dwpl = PropertyPackages.Fase.Liquid1
                Case PropertyPackages.Fase.Liquid2
                    phaseID = 4
                    dwpl = PropertyPackages.Fase.Liquid2
                Case PropertyPackages.Fase.Liquid3
                    phaseID = 5
                    dwpl = PropertyPackages.Fase.Liquid3
                Case PropertyPackages.Fase.Liquid
                    phaseID = 1
                    dwpl = PropertyPackages.Fase.Liquid
                Case PropertyPackages.Fase.Aqueous
                    phaseID = 6
                    dwpl = PropertyPackages.Fase.Aqueous
                Case PropertyPackages.Fase.Solid
                    phaseID = 7
                    dwpl = PropertyPackages.Fase.Solid
            End Select

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

            If phaseID = 3 Or phaseID = 4 Or phaseID = 5 Or phaseID = 6 Then

                result = Me.AUX_LIQDENS(T, P, 0.0#, phaseID, False)
                Me.CurrentMaterialStream.Fases(phaseID).SPMProperties.density = result
                Me.CurrentMaterialStream.Fases(phaseID).SPMProperties.enthalpy = Me.DW_CalcEnthalpy(RET_VMOL(dwpl), T, P, State.Liquid)
                Me.CurrentMaterialStream.Fases(phaseID).SPMProperties.entropy = Me.DW_CalcEntropy(RET_VMOL(dwpl), T, P, State.Liquid)
                result = Me.m_lk.Z_LK("L", T / Me.AUX_TCM(dwpl), P / Me.AUX_PCM(dwpl), Me.AUX_WM(dwpl))(0)
                Me.CurrentMaterialStream.Fases(phaseID).SPMProperties.compressibilityFactor = result
                Select Case Me.Parameters("PP_ENTH_CP_CALC_METHOD")
                    Case 0 'LK
                        resultObj = Me.m_lk.CpCvR_LK("L", T, P, RET_VMOL(dwpl), RET_VKij(), RET_VMAS(dwpl), RET_VTC(), RET_VPC(), RET_VCP(T), RET_VMM(), RET_VW(), RET_VZRa())
                        Me.CurrentMaterialStream.Fases(phaseID).SPMProperties.heatCapacityCp = resultObj(1)
                        Me.CurrentMaterialStream.Fases(phaseID).SPMProperties.heatCapacityCv = resultObj(2)
                    Case 1 'Ideal
                        result = Me.AUX_LIQCPm(T, phaseID)
                        Me.CurrentMaterialStream.Fases(phaseID).SPMProperties.heatCapacityCp = result
                        Me.CurrentMaterialStream.Fases(phaseID).SPMProperties.heatCapacityCv = result
                    Case 2 'Excess
                        result = Me.AUX_LIQCPm(T, phaseID) + Me.m_uni.CPEX_MIX(T, RET_VMOL(dwpl), Me.RET_VNAMES) / Me.AUX_MMM(dwpl)
                        Me.CurrentMaterialStream.Fases(phaseID).SPMProperties.heatCapacityCp = result
                        Me.CurrentMaterialStream.Fases(phaseID).SPMProperties.heatCapacityCv = result
                End Select
                result = Me.AUX_MMM(fase)
                Me.CurrentMaterialStream.Fases(phaseID).SPMProperties.molecularWeight = result
                result = Me.CurrentMaterialStream.Fases(phaseID).SPMProperties.enthalpy.GetValueOrDefault * Me.CurrentMaterialStream.Fases(phaseID).SPMProperties.molecularWeight.GetValueOrDefault
                Me.CurrentMaterialStream.Fases(phaseID).SPMProperties.molar_enthalpy = result
                result = Me.CurrentMaterialStream.Fases(phaseID).SPMProperties.entropy.GetValueOrDefault * Me.CurrentMaterialStream.Fases(phaseID).SPMProperties.molecularWeight.GetValueOrDefault
                Me.CurrentMaterialStream.Fases(phaseID).SPMProperties.molar_entropy = result
                result = Me.AUX_CONDTL(T)
                Me.CurrentMaterialStream.Fases(phaseID).SPMProperties.thermalConductivity = result
                result = Me.AUX_LIQVISCm(T)
                Me.CurrentMaterialStream.Fases(phaseID).SPMProperties.viscosity = result
                Me.CurrentMaterialStream.Fases(phaseID).SPMProperties.kinematic_viscosity = result / Me.CurrentMaterialStream.Fases(phaseID).SPMProperties.density.Value

            ElseIf phaseID = 2 Then

                result = Me.AUX_VAPDENS(T, P)
                Me.CurrentMaterialStream.Fases(phaseID).SPMProperties.density = result
                result = Me.m_lk.H_LK_MIX("V", T, P, RET_VMOL(fase.Vapor), RET_VKij, RET_VTC(), RET_VPC(), RET_VW(), RET_VMM(), Me.RET_Hid(298.15, T, fase.Vapor))
                Me.CurrentMaterialStream.Fases(phaseID).SPMProperties.enthalpy = result
                result = Me.m_lk.S_LK_MIX("V", T, P, RET_VMOL(fase.Vapor), RET_VKij, RET_VTC(), RET_VPC(), RET_VW(), RET_VMM(), Me.RET_Sid(298.15, T, P, fase.Vapor))
                Me.CurrentMaterialStream.Fases(phaseID).SPMProperties.entropy = result
                result = Me.m_lk.Z_LK("V", T / Me.AUX_TCM(PropertyPackages.Fase.Vapor), P / Me.AUX_PCM(PropertyPackages.Fase.Vapor), Me.AUX_WM(PropertyPackages.Fase.Vapor))(0)
                Me.CurrentMaterialStream.Fases(phaseID).SPMProperties.compressibilityFactor = result
                result = Me.AUX_CPm(PropertyPackages.Fase.Vapor, T)
                resultObj = Me.m_lk.CpCvR_LK("V", T, P, RET_VMOL(PropertyPackages.Fase.Vapor), RET_VKij(), RET_VMAS(PropertyPackages.Fase.Vapor), RET_VTC(), RET_VPC(), RET_VCP(T), RET_VMM(), RET_VW(), RET_VZRa())
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

            ElseIf phaseID = 7 Then

                result = Me.AUX_SOLIDDENS
                Me.CurrentMaterialStream.Fases(phaseID).SPMProperties.density = result
                Dim constprops As New List(Of ConstantProperties)
                For Each su As Substancia In Me.CurrentMaterialStream.Fases(0).Componentes.Values
                    constprops.Add(su.ConstantProperties)
                Next
                result = Me.DW_CalcSolidEnthalpy(T, RET_VMOL(PropertyPackages.Fase.Solid), constprops)
                Me.CurrentMaterialStream.Fases(phaseID).SPMProperties.enthalpy = result
                Me.CurrentMaterialStream.Fases(phaseID).SPMProperties.entropy = result / T
                Me.CurrentMaterialStream.Fases(phaseID).SPMProperties.compressibilityFactor = 0.0# 'result
                result = Me.DW_CalcSolidHeatCapacityCp(T, RET_VMOL(PropertyPackages.Fase.Solid), constprops)
                Me.CurrentMaterialStream.Fases(phaseID).SPMProperties.heatCapacityCp = result
                Me.CurrentMaterialStream.Fases(phaseID).SPMProperties.heatCapacityCv = result
                result = Me.AUX_MMM(fase)
                Me.CurrentMaterialStream.Fases(phaseID).SPMProperties.molecularWeight = result
                result = Me.CurrentMaterialStream.Fases(phaseID).SPMProperties.enthalpy.GetValueOrDefault * Me.CurrentMaterialStream.Fases(phaseID).SPMProperties.molecularWeight.GetValueOrDefault
                Me.CurrentMaterialStream.Fases(phaseID).SPMProperties.molar_enthalpy = result
                result = Me.CurrentMaterialStream.Fases(phaseID).SPMProperties.entropy.GetValueOrDefault * Me.CurrentMaterialStream.Fases(phaseID).SPMProperties.molecularWeight.GetValueOrDefault
                Me.CurrentMaterialStream.Fases(phaseID).SPMProperties.molar_entropy = result
                result = Me.AUX_CONDTG(T, P)
                Me.CurrentMaterialStream.Fases(phaseID).SPMProperties.thermalConductivity = 0.0# 'result
                Me.CurrentMaterialStream.Fases(phaseID).SPMProperties.viscosity = 1.0E+20
                Me.CurrentMaterialStream.Fases(phaseID).SPMProperties.kinematic_viscosity = 1.0E+20

            ElseIf phaseID = 1 Then

                DW_CalcLiqMixtureProps()

            Else

                DW_CalcOverallProps()


            End If

            If phaseID > 0 Then
                result = overallmolarflow * phasemolarfrac * Me.AUX_MMM(fase) / 1000 / Me.CurrentMaterialStream.Fases(phaseID).SPMProperties.density.GetValueOrDefault
                Me.CurrentMaterialStream.Fases(phaseID).SPMProperties.volumetric_flow = result
            Else
                'result = Me.CurrentMaterialStream.Fases(phaseID).SPMProperties.massflow.GetValueOrDefault / Me.CurrentMaterialStream.Fases(phaseID).SPMProperties.density.GetValueOrDefault
                'Me.CurrentMaterialStream.Fases(phaseID).SPMProperties.volumetric_flow = result
            End If

        End Sub

#End Region

        Public Overrides Function DW_CalcEnthalpy(ByVal Vx As System.Array, ByVal T As Double, ByVal P As Double, ByVal st As State) As Double

            Dim H As Double

            If st = State.Liquid Then
                Select Case Me.Parameters("PP_ENTH_CP_CALC_METHOD")
                    Case 0 'LK
                        H = Me.m_lk.H_LK_MIX("L", T, P, Vx, RET_VKij(), RET_VTC, RET_VPC, RET_VW, RET_VMM, Me.RET_Hid(298.15, T, Vx))
                    Case 1 'Ideal
                        H = Me.RET_Hid_L(298.15, T, Vx)
                    Case 2 'Excess
                        H = Me.RET_Hid_L(298.15, T, Vx) + Me.m_uni.HEX_MIX(T, Vx, Me.RET_VNAMES) / Me.AUX_MMM(Vx)
                End Select
            Else
                Select Case Me.Parameters("PP_ENTH_CP_CALC_METHOD")
                    Case 0 'LK
                        H = Me.m_lk.H_LK_MIX("V", T, P, Vx, RET_VKij(), RET_VTC, RET_VPC, RET_VW, RET_VMM, Me.RET_Hid(298.15, T, Vx))
                    Case 1 'Ideal
                        H = Me.RET_Hid_L(298.15, T, Vx) + Me.RET_HVAPM(Me.AUX_CONVERT_MOL_TO_MASS(Vx), T)
                    Case 2 'Excess
                        H = Me.RET_Hid_L(298.15, T, Vx) + Me.m_uni.HEX_MIX(T, Vx, Me.RET_VNAMES) / Me.AUX_MMM(Vx) + Me.RET_HVAPM(Me.AUX_CONVERT_MOL_TO_MASS(Vx), T)
                End Select
            End If

            Return H

        End Function

        Public Overrides Function DW_CalcEnthalpyDeparture(ByVal Vx As System.Array, ByVal T As Double, ByVal P As Double, ByVal st As State) As Double
            Dim H As Double

            If st = State.Liquid Then
                Select Case Me.Parameters("PP_ENTH_CP_CALC_METHOD")
                    Case 0 'LK
                        H = Me.m_lk.H_LK_MIX("L", T, P, Vx, RET_VKij(), RET_VTC, RET_VPC, RET_VW, RET_VMM, 0)
                    Case 1 'Ideal
                        H = 0.0#
                    Case 2 'Excess
                        H = Me.m_uni.HEX_MIX(T, Vx, Me.RET_VNAMES) / Me.AUX_MMM(Vx)
                End Select
            Else
                Select Case Me.Parameters("PP_ENTH_CP_CALC_METHOD")
                    Case 0 'LK
                        H = Me.m_lk.H_LK_MIX("V", T, P, Vx, RET_VKij(), RET_VTC, RET_VPC, RET_VW, RET_VMM, 0)
                    Case 1 'Ideal
                        H = Me.RET_HVAPM(Me.AUX_CONVERT_MOL_TO_MASS(Vx), T)
                    Case 2 'Excess
                        H = Me.m_uni.HEX_MIX(T, Vx, Me.RET_VNAMES) / Me.AUX_MMM(Vx) + Me.RET_HVAPM(Me.AUX_CONVERT_MOL_TO_MASS(Vx), T)
                End Select
            End If

            Return H

        End Function

        Public Overrides Function DW_CalcEntropy(ByVal Vx As System.Array, ByVal T As Double, ByVal P As Double, ByVal st As State) As Double

            Dim S As Double

            If st = State.Liquid Then
                Select Case Me.Parameters("PP_ENTH_CP_CALC_METHOD")
                    Case 0 'LK
                        S = Me.m_lk.S_LK_MIX("L", T, P, Vx, RET_VKij(), RET_VTC, RET_VPC, RET_VW, RET_VMM, Me.RET_Sid(298.15, T, P, Vx))
                    Case 1 'Ideal
                        S = Me.RET_Hid_L(298.15, T, Vx) / T
                    Case 2 'Excess
                        S = (Me.RET_Hid_L(298.15, T, Vx) + Me.m_uni.HEX_MIX(T, Vx, Me.RET_VNAMES) / Me.AUX_MMM(Vx)) / T
                End Select
            Else
                Select Case Me.Parameters("PP_ENTH_CP_CALC_METHOD")
                    Case 0 'LK
                        S = Me.m_lk.S_LK_MIX("V", T, P, Vx, RET_VKij(), RET_VTC, RET_VPC, RET_VW, RET_VMM, Me.RET_Sid(298.15, T, P, Vx))
                    Case 1 'Ideal
                        S = Me.RET_Hid_L(298.15, T, Vx) / T + Me.RET_HVAPM(Me.AUX_CONVERT_MOL_TO_MASS(Vx), T) / T
                    Case 2 'Excess
                        S = (Me.RET_Hid_L(298.15, T, Vx) + Me.m_uni.HEX_MIX(T, Vx, Me.RET_VNAMES) / Me.AUX_MMM(Vx)) / T + Me.RET_HVAPM(Me.AUX_CONVERT_MOL_TO_MASS(Vx), T) / T
                End Select
            End If

            Return S

        End Function

        Public Overrides Function DW_CalcEntropyDeparture(ByVal Vx As System.Array, ByVal T As Double, ByVal P As Double, ByVal st As State) As Double
            Dim S As Double

            If st = State.Liquid Then
                Select Case Me.Parameters("PP_ENTH_CP_CALC_METHOD")
                    Case 0 'LK
                        S = Me.m_lk.S_LK_MIX("L", T, P, Vx, RET_VKij(), RET_VTC, RET_VPC, RET_VW, RET_VMM, 0)
                    Case 1 'Ideal
                        S = 0.0# '-Me.RET_HVAPM(Me.AUX_CONVERT_MOL_TO_MASS(Vx), T) / T
                    Case 2 'Excess
                        S = (Me.m_uni.HEX_MIX(T, Vx, Me.RET_VNAMES) / Me.AUX_MMM(Vx)) / T '- Me.RET_HVAPM(Me.AUX_CONVERT_MOL_TO_MASS(Vx), T) / T
                End Select
            Else
                S = Me.m_lk.S_LK_MIX("V", T, P, Vx, RET_VKij(), RET_VTC, RET_VPC, RET_VW, RET_VMM, 0)
            End If

            Return S

        End Function

        Public Overrides Function DW_CalcFugCoeff(ByVal Vx As System.Array, ByVal T As Double, ByVal P As Double, ByVal st As State) As Double()

            DWSIM.App.WriteToConsole(Me.ComponentName & " fugacity coefficient calculation for phase '" & st.ToString & "' requested at T = " & T & " K and P = " & P & " Pa.", 2)
            DWSIM.App.WriteToConsole("Compounds: " & Me.RET_VNAMES.ToArrayString, 2)
            DWSIM.App.WriteToConsole("Mole fractions: " & Vx.ToArrayString(), 2)

            Dim prn As New PropertyPackages.ThermoPlugs.PR

            Dim n As Integer = UBound(Vx)
            Dim lnfug(n), ativ(n) As Double
            Dim fugcoeff(n) As Double
            Dim i As Integer

            Dim Tc As Object = Me.RET_VTC()
            Dim Tr As Double
            If st = State.Liquid Then
                ativ = Me.m_uni.GAMMA_MR(T, Vx, Me.RET_VNAMES)
                For i = 0 To n
                    Tr = T / Tc(i)
                    If Tr >= 1.02 Then
                        lnfug(i) = Math.Log(AUX_KHenry(Me.RET_VNAMES(i), T) / P)
                    ElseIf Tr < 0.98 Then
                        lnfug(i) = Math.Log(ativ(i) * Me.AUX_PVAPi(i, T) / (P))
                    Else 'do interpolation at proximity of critical point
                        Dim a2 As Double = AUX_KHenry(Me.RET_VNAMES(i), 1.02 * Tc(i))
                        Dim a1 As Double = ativ(i) * Me.AUX_PVAPi(i, 0.98 * Tc(i))
                        lnfug(i) = Math.Log(((Tr - 0.98) / (1.02 - 0.98) * (a2 - a1) + a1) / P)
                    End If
                Next
            Else
                If Not Me.Parameters.ContainsKey("PP_IDEAL_VAPOR_PHASE_FUG") Then Me.Parameters.Add("PP_IDEAL_VAPOR_PHASE_FUG", 0)
                If Me.Parameters("PP_IDEAL_VAPOR_PHASE_FUG") = 1 Then
                    For i = 0 To n
                        lnfug(i) = 0.0#
                    Next
                Else
                    lnfug = prn.CalcLnFug(T, P, Vx, Me.RET_VKij, Me.RET_VTC, Me.RET_VPC, Me.RET_VW, Nothing, "V")
                End If
            End If

            For i = 0 To n
                fugcoeff(i) = Exp(lnfug(i))
            Next

            DWSIM.App.WriteToConsole("Result: " & fugcoeff.ToArrayString(), 2)

            Return fugcoeff

        End Function

    End Class

End Namespace


