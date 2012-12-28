'    LIQUAC2 Property Package 
'    Copyright 2012 Daniel Wagner O. de Medeiros
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

    <System.Runtime.InteropServices.Guid(LIQUAC2PropertyPackage.ClassId)> _
<System.Serializable()> Public Class LIQUAC2PropertyPackage

        Inherits DWSIM.SimulationObjects.PropertyPackages.PropertyPackage

        Public Shadows Const ClassId As String = "f3eeff51-eccc-4c15-b4b0-1eb4d13c61f3"

        Public MAT_KIJ(38, 38)

        Private m_props As New DWSIM.SimulationObjects.PropertyPackages.Auxiliary.PROPS
        Public m_pr As New DWSIM.SimulationObjects.PropertyPackages.Auxiliary.PengRobinson
        Public m_uni As New DWSIM.SimulationObjects.PropertyPackages.Auxiliary.LIQUAC2
        Private m_lk As New DWSIM.SimulationObjects.PropertyPackages.Auxiliary.LeeKesler
        '<System.NonSerialized()> Private m_xn As DLLXnumbers.Xnumbers

        Public Sub New(ByVal comode As Boolean)
            MyBase.New(comode)
        End Sub

        Public Sub New()

            MyBase.New()

            Me.IsConfigurable = True
            Me.ConfigForm = New FormConfigLIQUAC
            Me._packagetype = PropertyPackages.PackageType.ActivityCoefficient

        End Sub

        Public Overrides Sub ReconfigureConfigForm()
            MyBase.ReconfigureConfigForm()
            Me.ConfigForm = New FormConfigLIQUAC
        End Sub

#Region "    DWSIM Functions"

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

            Dim HM, HV, HL As Double

            HL = Me.m_lk.H_LK_MIX("L", T, P, RET_VMOL(Fase.Liquid), RET_VKij(), RET_VTC, RET_VPC, RET_VW, RET_VMM, Me.RET_Hid(298.15, T, Fase.Liquid))
            HV = Me.m_lk.H_LK_MIX("V", T, P, RET_VMOL(Fase.Vapor), RET_VKij(), RET_VTC, RET_VPC, RET_VW, RET_VMM, Me.RET_Hid(298.15, T, Fase.Vapor))
            HM = Me.CurrentMaterialStream.Fases(1).SPMProperties.massfraction.GetValueOrDefault * HL + Me.CurrentMaterialStream.Fases(2).SPMProperties.massfraction.GetValueOrDefault * HV

            Dim ent_massica = HM
            Dim flow = Me.CurrentMaterialStream.Fases(0).SPMProperties.massflow
            Return ent_massica * flow

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

        Public Overrides Sub DW_CalcOverallProps()
            MyBase.DW_CalcOverallProps()
        End Sub

        Public Overrides Sub DW_CalcProp(ByVal [property] As String, ByVal phase As Fase)

            Dim result As Double = 0.0#
            Dim resultObj As Object = Nothing
            Dim phaseID As Integer = -1
            Dim state As String = ""

            Dim T, P As Double
            T = Me.CurrentMaterialStream.Fases(0).SPMProperties.temperature
            P = Me.CurrentMaterialStream.Fases(0).SPMProperties.pressure

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
                    resultObj = Me.m_lk.CpCvR_LK(state, T, P, RET_VMOL(phase), RET_VKij(), RET_VMAS(phase), RET_VTC(), RET_VPC(), RET_VCP(T), RET_VMM(), RET_VW(), RET_VZRa())
                    Me.CurrentMaterialStream.Fases(phaseID).SPMProperties.heatCapacityCp = resultObj(1)
                Case "heatcapacitycv"
                    resultObj = Me.m_lk.CpCvR_LK(state, T, P, RET_VMOL(phase), RET_VKij(), RET_VMAS(phase), RET_VTC(), RET_VPC(), RET_VCP(T), RET_VMM(), RET_VW(), RET_VZRa())
                    Me.CurrentMaterialStream.Fases(phaseID).SPMProperties.heatCapacityCv = resultObj(2)
                Case "enthalpy", "enthalpynf"
                    result = Me.m_lk.H_LK_MIX(state, T, P, RET_VMOL(phase), RET_VKij, RET_VTC(), RET_VPC(), RET_VW(), RET_VMM(), Me.RET_Hid(298.15, T, phase))
                    Me.CurrentMaterialStream.Fases(phaseID).SPMProperties.enthalpy = result
                    result = Me.CurrentMaterialStream.Fases(phaseID).SPMProperties.enthalpy.GetValueOrDefault * Me.CurrentMaterialStream.Fases(phaseID).SPMProperties.molecularWeight.GetValueOrDefault
                    Me.CurrentMaterialStream.Fases(phaseID).SPMProperties.molar_enthalpy = result
                Case "entropy", "entropynf"
                    result = Me.m_lk.S_LK_MIX(state, T, P, RET_VMOL(phase), RET_VKij, RET_VTC(), RET_VPC(), RET_VW(), RET_VMM(), Me.RET_Sid(298.15, T, P, phase))
                    Me.CurrentMaterialStream.Fases(phaseID).SPMProperties.entropy = result
                    result = Me.CurrentMaterialStream.Fases(phaseID).SPMProperties.entropy.GetValueOrDefault * Me.CurrentMaterialStream.Fases(phaseID).SPMProperties.molecularWeight.GetValueOrDefault
                    Me.CurrentMaterialStream.Fases(phaseID).SPMProperties.molar_entropy = result
                Case "excessenthalpy"
                    result = Me.m_lk.H_LK_MIX(state, T, P, RET_VMOL(phase), RET_VKij, RET_VTC(), RET_VPC(), RET_VW(), RET_VMM(), 0)
                    Me.CurrentMaterialStream.Fases(phaseID).SPMProperties.excessEnthalpy = result
                Case "excessentropy"
                    result = Me.m_lk.S_LK_MIX(state, T, P, RET_VMOL(phase), RET_VKij, RET_VTC(), RET_VPC(), RET_VW(), RET_VMM(), 0)
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
            T = Me.CurrentMaterialStream.Fases(0).SPMProperties.temperature
            P = Me.CurrentMaterialStream.Fases(0).SPMProperties.pressure

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
                result = Me.m_lk.H_LK_MIX("L", T, P, RET_VMOL(dwpl), RET_VKij, RET_VTC(), RET_VPC(), RET_VW(), RET_VMM(), Me.RET_Hid(298.15, T, dwpl))
                Me.CurrentMaterialStream.Fases(phaseID).SPMProperties.enthalpy = result
                result = Me.m_lk.S_LK_MIX("L", T, P, RET_VMOL(dwpl), RET_VKij, RET_VTC(), RET_VPC(), RET_VW(), RET_VMM(), Me.RET_Sid(298.15, T, P, dwpl))
                Me.CurrentMaterialStream.Fases(phaseID).SPMProperties.entropy = result
                'result = Me.m_pr.Z_PR(T, P, RET_VMOL(dwpl), RET_VKij(), RET_VTC, RET_VPC, RET_VW, "L")
                result = Me.m_lk.Z_LK("L", T / Me.AUX_TCM(dwpl), P / Me.AUX_PCM(dwpl), Me.AUX_WM(dwpl))(0)
                Me.CurrentMaterialStream.Fases(phaseID).SPMProperties.compressibilityFactor = result
                resultObj = Me.m_lk.CpCvR_LK("L", T, P, RET_VMOL(dwpl), RET_VKij(), RET_VMAS(dwpl), RET_VTC(), RET_VPC(), RET_VCP(T), RET_VMM(), RET_VW(), RET_VZRa())
                Me.CurrentMaterialStream.Fases(phaseID).SPMProperties.heatCapacityCp = resultObj(1)
                Me.CurrentMaterialStream.Fases(phaseID).SPMProperties.heatCapacityCv = resultObj(2)
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
                'result = Me.m_pr.Z_PR(T, P, RET_VMOL(fase.Vapor), RET_VKij, RET_VTC, RET_VPC, RET_VW, "V")
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
                Me.CurrentMaterialStream.Fases(phaseID).SPMProperties.viscosity = result
                Me.CurrentMaterialStream.Fases(phaseID).SPMProperties.kinematic_viscosity = result / Me.CurrentMaterialStream.Fases(phaseID).SPMProperties.density.Value

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
            Dim sum_pi2 = 0
            Dim sum_eui_pi2 = 0
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
            Dim sum_pi2 = 0
            Dim sum_eui_pi2 = 0
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

        Public Overrides Function DW_CalcEnthalpy(ByVal Vx As System.Array, ByVal T As Double, ByVal P As Double, ByVal st As State) As Double

            Dim H As Double

            If st = State.Liquid Then
                H = Me.m_lk.H_LK_MIX("L", T, P, Vx, RET_VKij(), RET_VTC, RET_VPC, RET_VW, RET_VMM, Me.RET_Hid(298.15, T, Vx))
            Else
                H = Me.m_lk.H_LK_MIX("V", T, P, Vx, RET_VKij(), RET_VTC, RET_VPC, RET_VW, RET_VMM, Me.RET_Hid(298.15, T, Vx))
            End If

            Return H

        End Function

        Public Overrides Function DW_CalcEnthalpyDeparture(ByVal Vx As System.Array, ByVal T As Double, ByVal P As Double, ByVal st As State) As Double
            Dim H As Double

            If st = State.Liquid Then
                H = Me.m_lk.H_LK_MIX("L", T, P, Vx, RET_VKij(), RET_VTC, RET_VPC, RET_VW, RET_VMM, 0)
            Else
                H = Me.m_lk.H_LK_MIX("V", T, P, Vx, RET_VKij(), RET_VTC, RET_VPC, RET_VW, RET_VMM, 0)
            End If

            Return H

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
                    Dim partvol As New Object
                    Dim i As Integer = 0
                    partvol = Me.m_pr.CalcPartialVolume(T, P, RET_VMOL(phase), RET_VKij(), RET_VTC(), RET_VPC(), RET_VW(), RET_VTB(), "V", 0.0001)
                    i = 0
                    For Each subst As ClassesBasicasTermodinamica.Substancia In Me.CurrentMaterialStream.Fases(2).Componentes.Values
                        subst.PartialVolume = partvol(i)
                        i += 1
                    Next
            End Select
        End Sub

        Public Overrides Function AUX_VAPDENS(ByVal T As Double, ByVal P As Double) As Double
            Dim val As Double
            Dim Z As Double = Me.m_pr.Z_PR(T, P, RET_VMOL(Fase.Vapor), RET_VKij, RET_VTC, RET_VPC, RET_VW, "V")
            val = P / (Z * 8.314 * T) / 1000 * AUX_MMM(Fase.Vapor)
            Return val
        End Function

        Public Overrides Function DW_CalcEntropy(ByVal Vx As System.Array, ByVal T As Double, ByVal P As Double, ByVal st As State) As Double

            Dim S As Double

            If st = State.Liquid Then
                S = Me.m_lk.S_LK_MIX("L", T, P, Vx, RET_VKij(), RET_VTC, RET_VPC, RET_VW, RET_VMM, Me.RET_Sid(298.15, T, P, Vx))
            Else
                S = Me.m_lk.S_LK_MIX("V", T, P, Vx, RET_VKij(), RET_VTC, RET_VPC, RET_VW, RET_VMM, Me.RET_Sid(298.15, T, P, Vx))
            End If

            Return S

        End Function

        Public Overrides Function DW_CalcEntropyDeparture(ByVal Vx As System.Array, ByVal T As Double, ByVal P As Double, ByVal st As State) As Double
            Dim S As Double

            If st = State.Liquid Then
                S = Me.m_lk.S_LK_MIX("L", T, P, Vx, RET_VKij(), RET_VTC, RET_VPC, RET_VW, RET_VMM, 0)
            Else
                S = Me.m_lk.S_LK_MIX("V", T, P, Vx, RET_VKij(), RET_VTC, RET_VPC, RET_VW, RET_VMM, 0)
            End If

            Return S

        End Function

        Public Overrides Function DW_CalcFugCoeff(ByVal Vx As System.Array, ByVal T As Double, ByVal P As Double, ByVal st As State) As Object

            Dim prn As New PropertyPackages.ThermoPlugs.PR

            Dim n As Integer = UBound(Vx)
            Dim lnfug(n), ativ(n) As Double
            Dim fugcoeff(n) As Double
            Dim i As Integer

            Dim Tc As Object = Me.RET_VTC()

            Dim constprops As New List(Of ConstantProperties)
            For Each s As Substancia In Me.CurrentMaterialStream.Fases(0).Componentes.Values
                constprops.Add(s.ConstantProperties)
            Next

            If st = State.Liquid Then
                ativ = Me.m_uni.GAMMA_MR(T, Vx, constprops)
                For i = 0 To n
                    If T / Tc(i) >= 1 Then
                        lnfug(i) = Math.Log(Me.AUX_PVAPi(i, T) / P)
                    Else
                        lnfug(i) = Math.Log(ativ(i) * Me.AUX_PVAPi(i, T) / (P))
                    End If
                Next
            ElseIf st = State.Vapor Then
                For i = 0 To n
                    lnfug(i) = 0.0#
                Next
            ElseIf st = State.Solid Then
                For i = 0 To n
                    If constprops(i).TemperatureOfFusion <> 0 Then
                        lnfug(i) = Log(Me.AUX_PVAPi(i, T) * Exp(-constprops(i).EnthalpyOfFusionAtTf / (8.314 * T) * (1 - T / constprops(i).TemperatureOfFusion)))
                    Else
                        lnfug(i) = 0.0#
                    End If
                Next
            End If

            For i = 0 To n
                fugcoeff(i) = Exp(lnfug(i))
            Next

            Return fugcoeff

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

#Region "    Flash Algorithms for Chemical and Physical Equilibria"

        Dim tmpx As Double(), tmpdx As Double()

        Dim N0 As New Dictionary(Of String, Double)
        Dim DN As New Dictionary(Of String, Double)
        Dim N As New Dictionary(Of String, Double)
        Dim T, P, P0, Ninerts, Winerts, E(,) As Double
        Dim r, c, els, comps, i, j As Integer

        Public Property ReactionSet As String = ""
        Public Property Reactions As List(Of String)
        Public Property ReactionExtents As Dictionary(Of String, Double)
        Public Property ComponentIDs As List(Of String)
        Public Property CompoundProperties As List(Of ConstantProperties)
        Public Property ComponentConversions As Dictionary(Of String, Double)

        Private Vx0 As Double()

        Public Function Flash_PT(Vx As Array, T As Double, P As Double) As Dictionary(Of String, Object)

            'This flash algorithm is for Electrolye/Salt systems with Water as the only solvent.
            'The vapor and solid phases are considered to be ideal.
            'Chemical equilibria is calculated using the reactions referenced by the reaction set of this property package.

            Dim n As Integer = CompoundProperties.Count - 1
            Dim activcoeff(n) As Double
            Dim i As Integer

            Dim Vxl(n), Vxs(n), Vxv(n), V, S, L As Double

            Dim wid As Integer = CompoundProperties.IndexOf((From c As ConstantProperties In CompoundProperties Select c Where c.Name = "Water").SingleOrDefault)

            For i = 0 To n
                If i <> wid Then
                    Vxv(i) = 0.0#
                Else
                    Vxv(i) = 1.0#
                End If
            Next

            Vxl = Vx

            Dim ids As New List(Of String)
            For i = 0 To n
                ids.Add(CompoundProperties(i).Name)
            Next

            Me.ReactionSet = Me.CurrentMaterialStream.Flowsheet.Options.ReactionSets.First.Key

            Me.Vx0 = Vxl.Clone

            Vxl = SolveChemicalEquilibria(Vxl, T, P, ids)

            activcoeff = Me.m_uni.GAMMA_MR(T, Vxl, CompoundProperties)

            Dim Vxlmax(n) As Double

            'solubilities 

            For i = 0 To n
                If CompoundProperties(i).TemperatureOfFusion <> 0.0# Then
                    Vxlmax(i) = (1 / activcoeff(i)) * Exp(-CompoundProperties(i).EnthalpyOfFusionAtTf / (0.00831447 * T) * (1 - T / CompoundProperties(i).TemperatureOfFusion))
                Else
                    Vxlmax(i) = 1.0#
                End If
            Next

            For i = 0 To n
                If Vxl(i) > Vxlmax(i) Then
                    Vxs(i) = Vxl(i) - Vxlmax(i)
                    Vxl(i) = Vxlmax(i)
                End If
            Next

            Dim results As New Dictionary(Of String, Object)

            Return results

        End Function

        Private Function SolveChemicalEquilibria(ByVal Vx As Array, ByVal T As Double, ByVal P As Double, ByVal ids As List(Of String)) As Array

            'solves the chemical equilibria for the liquid phase

            If Me.ReactionExtents Is Nothing Then Me.ReactionExtents = New Dictionary(Of String, Double)
            If Me.Reactions Is Nothing Then Me.Reactions = New List(Of String)
            If Me.ComponentConversions Is Nothing Then Me.ComponentConversions = New Dictionary(Of String, Double)
            If Me.ComponentIDs Is Nothing Then Me.ComponentIDs = New List(Of String)

            Dim form As FormFlowsheet = Me.CurrentMaterialStream.Flowsheet

            Dim objargs As New DWSIM.Outros.StatusChangeEventArgs

            Me.Reactions.Clear()
            Me.ReactionExtents.Clear()

            Dim rx As Reaction

            P0 = 101325

            Dim rxn As Reaction

            'check active reactions (equilibrium only) in the reaction set
            For Each rxnsb As ReactionSetBase In form.Options.ReactionSets(Me.ReactionSet).Reactions.Values
                If form.Options.Reactions(rxnsb.ReactionID).ReactionType = ReactionType.Equilibrium And rxnsb.IsActive Then
                    Me.Reactions.Add(rxnsb.ReactionID)
                    Me.ReactionExtents.Add(rxnsb.ReactionID, 0)
                    rxn = form.Options.Reactions(rxnsb.ReactionID)
                    'equilibrium constant calculation
                    Select Case rxn.KExprType
                        Case Reaction.KOpt.Constant
                            'rxn.ConstantKeqValue = rxn.ConstantKeqValue
                        Case Reaction.KOpt.Expression
                            If rxn.ExpContext Is Nothing Then
                                rxn.ExpContext = New Ciloci.Flee.ExpressionContext
                                With rxn.ExpContext
                                    .Imports.ImportStaticMembers(GetType(System.Math))
                                    .Variables.DefineVariable("T", GetType(Double))
                                End With
                            End If
                            rxn.ExpContext.Variables.SetVariableValue("T", T)
                            rxn.Expr = ExpressionFactory.CreateGeneric(Of Double)(rxn.Expression, rxn.ExpContext)
                            rxn.ConstantKeqValue = Exp(rxn.Expr.Evaluate)
                        Case Reaction.KOpt.Gibbs
                            Dim id(rxn.Components.Count - 1) As String
                            Dim stcoef(rxn.Components.Count - 1) As Double
                            Dim bcidx As Integer = 0
                            j = 0
                            For Each sb As ReactionStoichBase In rxn.Components.Values
                                id(j) = sb.CompName
                                stcoef(j) = sb.StoichCoeff
                                If sb.IsBaseReactant Then bcidx = j
                                j += 1
                            Next
                            Dim DelG_RT = Me.AUX_DELGig_RT(298.15, T, id, stcoef, bcidx)
                            rxn.ConstantKeqValue = Exp(-DelG_RT)
                    End Select
                End If
            Next

            'initial estimates for reaction extents

            Me.ComponentConversions.Clear()
            Me.ComponentIDs.Clear()

            'r: number of reactions
            'c: number of components
            'i,j: iterators

            i = 0
            For Each rxid As String In Me.Reactions
                rx = form.Options.Reactions(rxid)
                j = 0
                For Each comp As ReactionStoichBase In rx.Components.Values
                    If Not Me.ComponentIDs.Contains(comp.CompName) Then
                        Me.ComponentIDs.Add(comp.CompName)
                        Me.ComponentConversions.Add(comp.CompName, 0)
                    End If
                    j += 1
                Next
                i += 1
            Next

            r = Me.Reactions.Count - 1
            c = Me.ComponentIDs.Count - 1

            ReDim E(c, r)

            'E: matrix of stoichometric coefficients
            i = 0
            For Each rxid As String In Me.Reactions
                rx = form.Options.Reactions(rxid)
                j = 0
                For Each cname As String In Me.ComponentIDs
                    If rx.Components.ContainsKey(cname) Then
                        E(j, i) = rx.Components(cname).StoichCoeff
                    Else
                        E(j, i) = 0
                    End If
                    j += 1
                Next
                i += 1
            Next

            Dim fm0(c), N0tot As Double

            N0.Clear()
            DN.Clear()
            N.Clear()

            For Each cname As String In Me.ComponentIDs
                N0.Add(cname, Vx(ids.IndexOf(cname)))
                DN.Add(cname, 0)
                N.Add(cname, Vx(ids.IndexOf(cname)))
            Next

            N0.Values.CopyTo(fm0, 0)

            N0tot = 1.0#
            Ninerts = N0tot - Sum(fm0)

            Dim lbound(Me.ReactionExtents.Count - 1) As Double
            Dim ubound(Me.ReactionExtents.Count - 1) As Double
            Dim var1 As Double

            i = 0
            For Each rxid As String In Me.Reactions
                rx = form.Options.Reactions(rxid)
                j = 0
                For Each comp As ReactionStoichBase In rx.Components.Values
                    var1 = -N0(comp.CompName) / comp.StoichCoeff
                    If j = 0 Then
                        lbound(i) = var1
                        ubound(i) = var1
                    Else
                        If var1 < lbound(i) Then lbound(i) = var1
                        If var1 > ubound(i) Then ubound(i) = var1
                    End If
                    j += 1
                Next
                i += 1
            Next

            Dim REx(r) As Double

            For i = 0 To r
                REx(i) = 0.0# '(lbound(i) + ubound(i)) / 2
            Next

            Dim REx0(REx.Length - 1) As Double

            'solve using newton's method

            Dim fx(r), dfdx(r, r), dx(r), x(r), df, fval As Double
            Dim brentsolver As New BrentOpt.BrentMinimize
            brentsolver.DefineFuncDelegate(AddressOf MinimizeError)

            Dim niter As Integer

            Me.T = T
            Me.P = P

            x = REx
            niter = 0
            Do

                fx = Me.FunctionValue2N(x)
                dfdx = Me.FunctionGradient2N(x)

                Dim success As Boolean
                success = MathEx.SysLin.rsolve.rmatrixsolve(dfdx, fx, r + 1, dx)

                tmpx = x
                tmpdx = dx
                df = 1
                fval = brentsolver.brentoptimize(0.1, 2, 0.0001, df)

                For i = 0 To r
                    x(i) -= dx(i) * df
                    'If x(i) > ubound(i) Then x(i) = ubound(i)
                    'If x(i) < lbound(i) Then x(i) = lbound(i)
                Next

                niter += 1

                If AbsSum(dx) = 0.0# Then
                    Throw New Exception("No solution found - reached a stationary point of the objective function (singular gradient matrix).")
                End If

            Loop Until AbsSum(fx) < 0.00001 Or niter > 999

            If niter > 999 Then
                Throw New Exception("Reached the maximum number of iterations without converging.")
            End If

            i = 0
            For Each r As String In Me.Reactions
                Me.ReactionExtents(r) = REx(i)
                i += 1
            Next

            ' comp. conversions
            For Each sb As String In ids
                If Me.ComponentConversions.ContainsKey(sb) Then
                    Me.ComponentConversions(sb) = -DN(sb) / N0(sb)
                End If
            Next

            Dim mtotal As Double = 0
            For Each s As String In N.Keys
                mtotal += Abs(N(s))
            Next

            For Each s As String In N.Keys
                Vx(ids.IndexOf(s)) = Abs(N(s)) / mtotal
            Next

            Return Vx

        End Function

        Private Function FunctionValue2N(ByVal x() As Double) As Double()

            Dim i, j, nc As Integer

            nc = Me.CompoundProperties.Count - 1

            i = 0
            For Each s As String In N.Keys
                DN(s) = 0
                For j = 0 To r
                    DN(s) += E(i, j) * x(j)
                Next
                i += 1
            Next

            For Each s As String In DN.Keys
                N(s) = N0(s) + DN(s)
            Next

            Dim Vx(nc) As Double

            'calculate molality considering 1 mol of mixture.

            Dim wtotal As Double = 0
            Dim mtotal As Double = 0
            Dim molality(nc) As Double

            For i = 0 To nc
                Vx(i) = Vx0(i)
            Next

            For i = 0 To N.Count - 1
                For j = 0 To nc
                    If CompoundProperties(j).Name = ComponentIDs(i) Then
                        Vx(j) = Abs(N(ComponentIDs(i)))
                        Exit For
                    End If
                Next
            Next

            i = 0
            Do
                wtotal += Vx(i) * CompoundProperties(i).Molar_Weight / 1000
                mtotal += Vx(i)
                i += 1
            Loop Until i = nc + 1

            Dim Xsolv As Double = 1

            i = 0
            Do
                Vx(i) /= mtotal
                molality(i) = Vx(i) / wtotal
                i += 1
            Loop Until i = nc + 1

            Dim activcoeff(nc) As Double

            activcoeff = Me.m_uni.GAMMA_MR(T, Vx, CompoundProperties)

            Dim CP(nc) As Double
            Dim f(x.Length - 1) As Double
            Dim prod(x.Length - 1) As Double

            For i = 0 To nc
                If CompoundProperties(i).IsIon Then
                    CP(i) = molality(i) * activcoeff(i)
                Else
                    CP(i) = Vx(i) * activcoeff(i)
                End If
            Next

            For i = 0 To Me.Reactions.Count - 1
                prod(i) = 1
                j = 0
                For Each s As String In Me.ComponentIDs
                    With Me.CurrentMaterialStream.Flowsheet.Options.Reactions(Me.Reactions(i))
                        If .Components.ContainsKey(s) Then
                            prod(i) += .Components(s).StoichCoeff * Log(CP(j))
                        End If
                    End With
                    j += 1
                Next
            Next

            Dim pen_val As Double = ReturnPenaltyValue()

            For i = 0 To Me.Reactions.Count - 1
                With Me.CurrentMaterialStream.Flowsheet.Options.Reactions(Me.Reactions(i))
                    f(i) = prod(i) - Log(.ConstantKeqValue) + pen_val
                    If Double.IsNaN(f(i)) Or Double.IsInfinity(f(i)) Then
                        f(i) = pen_val - .ConstantKeqValue
                    End If
                End With
            Next

            Return f

        End Function

        Private Function FunctionGradient2N(ByVal x() As Double) As Double(,)

            Dim epsilon As Double = 0.0000000001

            Dim f1(), f2() As Double
            Dim g(x.Length - 1, x.Length - 1), x2(x.Length - 1) As Double
            Dim i, j, k As Integer

            f1 = FunctionValue2N(x)
            For i = 0 To x.Length - 1
                For j = 0 To x.Length - 1
                    If i <> j Then
                        x2(j) = x(j)
                    Else
                        x2(j) = x(j) + epsilon
                    End If
                Next
                f2 = FunctionValue2N(x2)
                For k = 0 To x.Length - 1
                    g(k, i) = (f2(k) - f1(k)) / (x2(i) - x(i))
                Next
            Next

            Return g

        End Function

        Public Function MinimizeError(ByVal t As Double) As Double

            Dim tmpx0 As Double() = tmpx.Clone

            For i = 0 To r
                tmpx0(i) -= tmpdx(i) * t
            Next

            Dim abssum0 = AbsSum(FunctionValue2N(tmpx0))
            Return abssum0

        End Function

        Private Function ReturnPenaltyValue() As Double

            'calculate penalty functions for constraint variables

            Dim i As Integer
            Dim nc As Integer = Me.CompoundProperties.Count - 1

            Dim con_lc(nc), con_uc(nc), con_val(nc) As Double
            Dim pen_val As Double = 0
            Dim delta1, delta2 As Double

            i = 0
            For Each comp As String In Me.ComponentIDs
                con_lc(i) = 0
                con_uc(i) = 1
                con_val(i) = N(comp)
                i += 1
            Next

            pen_val = 0
            For i = 0 To nc
                delta1 = con_val(i) - con_lc(i)
                delta2 = con_val(i) - con_uc(i)
                If delta1 < 0 Then
                    pen_val += -delta1 * 100000
                ElseIf delta2 > 1 Then
                    pen_val += -delta2 * 100000
                Else
                    pen_val += 0
                End If
            Next

            If Double.IsNaN(pen_val) Then pen_val = 0

            Return pen_val

        End Function

#End Region

#Region "    CalcEquilibrium Override"

        Public Overrides Sub DW_CalcEquilibrium(spec1 As FlashSpec, spec2 As FlashSpec)

            Me.CurrentMaterialStream.AtEquilibrium = False

            Dim P, T, H, S, xv, xl, xl2 As Double
            Dim result As Object = Nothing
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

            Me.CompoundProperties = constprops

            Select Case spec1

                Case FlashSpec.T

                    Select Case spec2

                        Case FlashSpec.P

                            T = Me.CurrentMaterialStream.Fases(0).SPMProperties.temperature.GetValueOrDefault
                            P = Me.CurrentMaterialStream.Fases(0).SPMProperties.pressure.GetValueOrDefault

                            result = Me.Flash_PT(RET_VMOL(Fase.Mixture), T, P)

                            xl = result(0)
                            xv = result(1)
                            xl2 = result(5)

                            Me.CurrentMaterialStream.Fases(3).SPMProperties.molarfraction = xl
                            Me.CurrentMaterialStream.Fases(4).SPMProperties.molarfraction = xl2
                            Me.CurrentMaterialStream.Fases(2).SPMProperties.molarfraction = xv

                            Dim Vx = result(2)
                            Dim Vy = result(3)
                            Dim Vx2 = result(6)

                            Dim FCL = Me.DW_CalcFugCoeff(Vx, T, P, State.Liquid)
                            Dim FCL2 = Me.DW_CalcFugCoeff(Vx2, T, P, State.Liquid)
                            Dim FCV = Me.DW_CalcFugCoeff(Vy, T, P, State.Vapor)

                            i = 0
                            For Each subst In Me.CurrentMaterialStream.Fases(3).Componentes.Values
                                subst.FracaoMolar = Vx(i)
                                subst.FugacityCoeff = FCL(i)
                                subst.ActivityCoeff = 0
                                subst.PartialVolume = 0
                                subst.PartialPressure = 0
                                i += 1
                            Next
                            For Each subst In Me.CurrentMaterialStream.Fases(3).Componentes.Values
                                subst.FracaoMassica = Me.AUX_CONVERT_MOL_TO_MASS(subst.Nome, 3)
                            Next
                            i = 0
                            For Each subst In Me.CurrentMaterialStream.Fases(4).Componentes.Values
                                subst.FracaoMolar = Vx2(i)
                                subst.FugacityCoeff = FCL2(i)
                                subst.ActivityCoeff = 0
                                subst.PartialVolume = 0
                                subst.PartialPressure = 0
                                i += 1
                            Next
                            For Each subst In Me.CurrentMaterialStream.Fases(4).Componentes.Values
                                subst.FracaoMassica = Me.AUX_CONVERT_MOL_TO_MASS(subst.Nome, 4)
                            Next
                            i = 0
                            For Each subst In Me.CurrentMaterialStream.Fases(2).Componentes.Values
                                subst.FracaoMolar = Vy(i)
                                subst.FugacityCoeff = FCV(i)
                                subst.ActivityCoeff = 0
                                subst.PartialVolume = 0
                                subst.PartialPressure = 0
                                i += 1
                            Next
                            For Each subst In Me.CurrentMaterialStream.Fases(2).Componentes.Values
                                subst.FracaoMassica = Me.AUX_CONVERT_MOL_TO_MASS(subst.Nome, 2)
                            Next

                            Me.CurrentMaterialStream.Fases(3).SPMProperties.massfraction = xl * Me.AUX_MMM(Fase.Liquid1) / (xl * Me.AUX_MMM(Fase.Liquid1) + xl2 * Me.AUX_MMM(Fase.Liquid2) + xv * Me.AUX_MMM(Fase.Vapor))
                            Me.CurrentMaterialStream.Fases(4).SPMProperties.massfraction = xl2 * Me.AUX_MMM(Fase.Liquid2) / (xl * Me.AUX_MMM(Fase.Liquid1) + xl2 * Me.AUX_MMM(Fase.Liquid2) + xv * Me.AUX_MMM(Fase.Vapor))
                            Me.CurrentMaterialStream.Fases(2).SPMProperties.massfraction = xv * Me.AUX_MMM(Fase.Vapor) / (xl * Me.AUX_MMM(Fase.Liquid1) + xl2 * Me.AUX_MMM(Fase.Liquid2) + xv * Me.AUX_MMM(Fase.Vapor))

                            Dim HM, HV, HL, HL2 As Double

                            If xl <> 0 Then HL = Me.DW_CalcEnthalpy(Vx, T, P, State.Liquid)
                            If xl2 <> 0 Then HL2 = Me.DW_CalcEnthalpy(Vx2, T, P, State.Liquid)
                            If xv <> 0 Then HV = Me.DW_CalcEnthalpy(Vy, T, P, State.Vapor)
                            HM = Me.CurrentMaterialStream.Fases(4).SPMProperties.massfraction.GetValueOrDefault * HL2 + Me.CurrentMaterialStream.Fases(3).SPMProperties.massfraction.GetValueOrDefault * HL + Me.CurrentMaterialStream.Fases(2).SPMProperties.massfraction.GetValueOrDefault * HV

                            H = HM

                            Dim SM, SV, SL, SL2 As Double

                            If xl <> 0 Then SL = Me.DW_CalcEntropy(Vx, T, P, State.Liquid)
                            If xl2 <> 0 Then SL2 = Me.DW_CalcEntropy(Vx2, T, P, State.Liquid)
                            If xv <> 0 Then SV = Me.DW_CalcEntropy(Vy, T, P, State.Vapor)
                            SM = Me.CurrentMaterialStream.Fases(4).SPMProperties.massfraction.GetValueOrDefault * SL2 + Me.CurrentMaterialStream.Fases(3).SPMProperties.massfraction.GetValueOrDefault * SL + Me.CurrentMaterialStream.Fases(2).SPMProperties.massfraction.GetValueOrDefault * SV

                            S = SM

                        Case FlashSpec.H

                            Throw New Exception(DWSIM.App.GetLocalString("PropPack_FlashTHNotSupported"))

                        Case FlashSpec.S

                            Throw New Exception(DWSIM.App.GetLocalString("PropPack_FlashTSNotSupported"))

                        Case FlashSpec.VAP

                            Dim KI(n) As Double

                            i = 0
                            Do
                                KI(i) = 0
                                i = i + 1
                            Loop Until i = n + 1

                            T = Me.CurrentMaterialStream.Fases(0).SPMProperties.temperature.GetValueOrDefault
                            P = Me.CurrentMaterialStream.Fases(0).SPMProperties.pressure.GetValueOrDefault

                            result = Me.FlashBase.Flash_TV(RET_VMOL(Fase.Mixture), T, xv, 0, Me)

                            P = result(4)

                            xl = result(0)
                            xv = result(1)
                            xl2 = result(7)

                            Me.CurrentMaterialStream.Fases(3).SPMProperties.molarfraction = xl
                            Me.CurrentMaterialStream.Fases(4).SPMProperties.molarfraction = xl2
                            Me.CurrentMaterialStream.Fases(2).SPMProperties.molarfraction = xv

                            Dim Vx = result(2)
                            Dim Vy = result(3)
                            Dim Vx2 = result(8)

                            Dim FCL = Me.DW_CalcFugCoeff(Vx, T, P, State.Liquid)
                            Dim FCL2 = Me.DW_CalcFugCoeff(Vx2, T, P, State.Liquid)
                            Dim FCV = Me.DW_CalcFugCoeff(Vy, T, P, State.Vapor)

                            i = 0
                            For Each subst In Me.CurrentMaterialStream.Fases(3).Componentes.Values
                                subst.FracaoMolar = Vx(i)
                                subst.FugacityCoeff = FCL(i)
                                subst.ActivityCoeff = 0
                                subst.PartialVolume = 0
                                subst.PartialPressure = 0
                                i += 1
                            Next
                            For Each subst In Me.CurrentMaterialStream.Fases(3).Componentes.Values
                                subst.FracaoMassica = Me.AUX_CONVERT_MOL_TO_MASS(subst.Nome, 3)
                            Next
                            i = 0
                            For Each subst In Me.CurrentMaterialStream.Fases(4).Componentes.Values
                                subst.FracaoMolar = Vx2(i)
                                subst.FugacityCoeff = FCL2(i)
                                subst.ActivityCoeff = 0
                                subst.PartialVolume = 0
                                subst.PartialPressure = 0
                                i += 1
                            Next
                            For Each subst In Me.CurrentMaterialStream.Fases(4).Componentes.Values
                                subst.FracaoMassica = Me.AUX_CONVERT_MOL_TO_MASS(subst.Nome, 4)
                            Next
                            i = 0
                            For Each subst In Me.CurrentMaterialStream.Fases(2).Componentes.Values
                                subst.FracaoMolar = Vy(i)
                                subst.FugacityCoeff = FCV(i)
                                subst.ActivityCoeff = 0
                                subst.PartialVolume = 0
                                subst.PartialPressure = 0
                                i += 1
                            Next
                            For Each subst In Me.CurrentMaterialStream.Fases(2).Componentes.Values
                                subst.FracaoMassica = Me.AUX_CONVERT_MOL_TO_MASS(subst.Nome, 2)
                            Next

                            Me.CurrentMaterialStream.Fases(3).SPMProperties.massfraction = xl * Me.AUX_MMM(Fase.Liquid1) / (xl * Me.AUX_MMM(Fase.Liquid1) + xl2 * Me.AUX_MMM(Fase.Liquid2) + xv * Me.AUX_MMM(Fase.Vapor))
                            Me.CurrentMaterialStream.Fases(4).SPMProperties.massfraction = xl2 * Me.AUX_MMM(Fase.Liquid2) / (xl * Me.AUX_MMM(Fase.Liquid1) + xl2 * Me.AUX_MMM(Fase.Liquid2) + xv * Me.AUX_MMM(Fase.Vapor))
                            Me.CurrentMaterialStream.Fases(2).SPMProperties.massfraction = xv * Me.AUX_MMM(Fase.Vapor) / (xl * Me.AUX_MMM(Fase.Liquid1) + xl2 * Me.AUX_MMM(Fase.Liquid2) + xv * Me.AUX_MMM(Fase.Vapor))

                            Dim HM, HV, HL, HL2 As Double

                            If xl <> 0 Then HL = Me.DW_CalcEnthalpy(Vx, T, P, State.Liquid)
                            If xl2 <> 0 Then HL2 = Me.DW_CalcEnthalpy(Vx2, T, P, State.Liquid)
                            If xv <> 0 Then HV = Me.DW_CalcEnthalpy(Vy, T, P, State.Vapor)
                            HM = Me.CurrentMaterialStream.Fases(4).SPMProperties.massfraction.GetValueOrDefault * HL2 + Me.CurrentMaterialStream.Fases(3).SPMProperties.massfraction.GetValueOrDefault * HL + Me.CurrentMaterialStream.Fases(2).SPMProperties.massfraction.GetValueOrDefault * HV

                            H = HM

                            Dim SM, SV, SL, SL2 As Double

                            If xl <> 0 Then SL = Me.DW_CalcEntropy(Vx, T, P, State.Liquid)
                            If xl2 <> 0 Then SL2 = Me.DW_CalcEntropy(Vx2, T, P, State.Liquid)
                            If xv <> 0 Then SV = Me.DW_CalcEntropy(Vy, T, P, State.Vapor)
                            SM = Me.CurrentMaterialStream.Fases(4).SPMProperties.massfraction.GetValueOrDefault * SL2 + Me.CurrentMaterialStream.Fases(3).SPMProperties.massfraction.GetValueOrDefault * SL + Me.CurrentMaterialStream.Fases(2).SPMProperties.massfraction.GetValueOrDefault * SV

                            S = SM

                    End Select

                Case FlashSpec.P

                    Select Case spec2

                        Case FlashSpec.H

                            T = Me.CurrentMaterialStream.Fases(0).SPMProperties.temperature.GetValueOrDefault
                            P = Me.CurrentMaterialStream.Fases(0).SPMProperties.pressure.GetValueOrDefault

                            If Double.IsNaN(H) Or Double.IsInfinity(H) Then H = Me.CurrentMaterialStream.Fases(0).SPMProperties.molar_enthalpy.GetValueOrDefault / Me.CurrentMaterialStream.Fases(0).SPMProperties.molecularWeight.GetValueOrDefault

                            If Me.AUX_IS_SINGLECOMP(Fase.Mixture) Then

                                Dim hl, hv, sl, sv As Double
                                Dim vz As Object = Me.RET_VMOL(Fase.Mixture)
                                result = Me.FlashBase.Flash_PH(RET_VMOL(Fase.Mixture), P, H, 0.0#, Me)

                                T = result(4)

                                hl = Me.DW_CalcEnthalpy(vz, T, P, State.Liquid)
                                hv = Me.DW_CalcEnthalpy(vz, T, P, State.Vapor)
                                sl = Me.DW_CalcEntropy(vz, T, P, State.Liquid)
                                sv = Me.DW_CalcEntropy(vz, T, P, State.Vapor)

                                If Double.IsNaN(hl) Then hl = Double.NegativeInfinity
                                If Double.IsNaN(sl) Then sl = Double.NegativeInfinity

                                If H < hl Then
                                    xv = 0
                                    GoTo redirect
                                ElseIf H >= hv Then
                                    xv = 1
                                    GoTo redirect
                                Else
                                    If hl = Double.NegativeInfinity Then
                                        xv = 1.0#
                                        S = sv
                                    Else
                                        xv = (H - hl) / (hv - hl)
                                        S = xv * sv + (1 - xv) * sl
                                    End If
                                End If
                                xl = 1 - xv

                                'T = Me.CurrentMaterialStream.Fases(0).SPMProperties.temperature.GetValueOrDefault

                                Me.CurrentMaterialStream.Fases(3).SPMProperties.molarfraction = xl
                                Me.CurrentMaterialStream.Fases(2).SPMProperties.molarfraction = xv
                                Me.CurrentMaterialStream.Fases(3).SPMProperties.massfraction = xl
                                Me.CurrentMaterialStream.Fases(2).SPMProperties.massfraction = xv

                                i = 0
                                For Each subst In Me.CurrentMaterialStream.Fases(3).Componentes.Values
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

redirect:                       result = Me.FlashBase.Flash_PH(RET_VMOL(Fase.Mixture), P, H, 0.0#, Me)

                                T = result(4)

                                xl = result(0)
                                xv = result(1)
                                xl2 = result(7)

                                Me.CurrentMaterialStream.Fases(3).SPMProperties.molarfraction = xl
                                Me.CurrentMaterialStream.Fases(4).SPMProperties.molarfraction = xl2
                                Me.CurrentMaterialStream.Fases(2).SPMProperties.molarfraction = xv

                                Dim Vx = result(2)
                                Dim Vy = result(3)
                                Dim Vx2 = result(8)

                                Dim FCL = Me.DW_CalcFugCoeff(Vx, T, P, State.Liquid)
                                Dim FCL2 = Me.DW_CalcFugCoeff(Vx2, T, P, State.Liquid)
                                Dim FCV = Me.DW_CalcFugCoeff(Vy, T, P, State.Vapor)

                                i = 0
                                For Each subst In Me.CurrentMaterialStream.Fases(3).Componentes.Values
                                    subst.FracaoMolar = Vx(i)
                                    subst.FugacityCoeff = FCL(i)
                                    subst.ActivityCoeff = 0
                                    subst.PartialVolume = 0
                                    subst.PartialPressure = 0
                                    i += 1
                                Next
                                i = 1
                                For Each subst In Me.CurrentMaterialStream.Fases(3).Componentes.Values
                                    subst.FracaoMassica = Me.AUX_CONVERT_MOL_TO_MASS(subst.Nome, 3)
                                    i += 1
                                Next
                                i = 0
                                For Each subst In Me.CurrentMaterialStream.Fases(4).Componentes.Values
                                    subst.FracaoMolar = Vx2(i)
                                    subst.FugacityCoeff = FCL2(i)
                                    subst.ActivityCoeff = 0
                                    subst.PartialVolume = 0
                                    subst.PartialPressure = 0
                                    i += 1
                                Next
                                i = 1
                                For Each subst In Me.CurrentMaterialStream.Fases(4).Componentes.Values
                                    subst.FracaoMassica = Me.AUX_CONVERT_MOL_TO_MASS(subst.Nome, 4)
                                    i += 1
                                Next
                                i = 0
                                For Each subst In Me.CurrentMaterialStream.Fases(2).Componentes.Values
                                    subst.FracaoMolar = Vy(i)
                                    subst.FugacityCoeff = FCV(i)
                                    subst.ActivityCoeff = 0
                                    subst.PartialVolume = 0
                                    subst.PartialPressure = 0
                                    i += 1
                                Next

                                i = 1
                                For Each subst In Me.CurrentMaterialStream.Fases(2).Componentes.Values
                                    subst.FracaoMassica = Me.AUX_CONVERT_MOL_TO_MASS(subst.Nome, 2)
                                    i += 1
                                Next

                                Me.CurrentMaterialStream.Fases(3).SPMProperties.massfraction = xl * Me.AUX_MMM(Fase.Liquid1) / (xl * Me.AUX_MMM(Fase.Liquid1) + xl2 * Me.AUX_MMM(Fase.Liquid2) + xv * Me.AUX_MMM(Fase.Vapor))
                                Me.CurrentMaterialStream.Fases(4).SPMProperties.massfraction = xl2 * Me.AUX_MMM(Fase.Liquid2) / (xl * Me.AUX_MMM(Fase.Liquid1) + xl2 * Me.AUX_MMM(Fase.Liquid2) + xv * Me.AUX_MMM(Fase.Vapor))
                                Me.CurrentMaterialStream.Fases(2).SPMProperties.massfraction = xv * Me.AUX_MMM(Fase.Vapor) / (xl * Me.AUX_MMM(Fase.Liquid1) + xl2 * Me.AUX_MMM(Fase.Liquid2) + xv * Me.AUX_MMM(Fase.Vapor))

                                Dim SM, SV, SL, SL2 As Double

                                If xl <> 0 Then SL = Me.DW_CalcEntropy(Vx, T, P, State.Liquid)
                                If xl2 <> 0 Then SL2 = Me.DW_CalcEntropy(Vx2, T, P, State.Liquid)
                                If xv <> 0 Then SV = Me.DW_CalcEntropy(Vy, T, P, State.Vapor)
                                SM = Me.CurrentMaterialStream.Fases(4).SPMProperties.massfraction.GetValueOrDefault * SL2 + Me.CurrentMaterialStream.Fases(3).SPMProperties.massfraction.GetValueOrDefault * SL + Me.CurrentMaterialStream.Fases(2).SPMProperties.massfraction.GetValueOrDefault * SV

                                S = SM

                            End If

                        Case FlashSpec.S

                            T = Me.CurrentMaterialStream.Fases(0).SPMProperties.temperature.GetValueOrDefault
                            P = Me.CurrentMaterialStream.Fases(0).SPMProperties.pressure.GetValueOrDefault

                            If Double.IsNaN(S) Or Double.IsInfinity(S) Then S = Me.CurrentMaterialStream.Fases(0).SPMProperties.molar_entropy.GetValueOrDefault / Me.CurrentMaterialStream.Fases(0).SPMProperties.molecularWeight.GetValueOrDefault

                            If Me.AUX_IS_SINGLECOMP(Fase.Mixture) And Me.ComponentName <> "FPROPS" Then

                                Dim hl, hv, sl, sv As Double
                                Dim vz As Object = Me.RET_VMOL(Fase.Mixture)

                                result = Me.FlashBase.Flash_PS(RET_VMOL(Fase.Mixture), P, S, 0.0#, Me)

                                T = result(4)

                                hl = Me.DW_CalcEnthalpy(vz, T, P, State.Liquid)
                                hv = Me.DW_CalcEnthalpy(vz, T, P, State.Vapor)
                                sl = Me.DW_CalcEntropy(vz, T, P, State.Liquid)
                                sv = Me.DW_CalcEntropy(vz, T, P, State.Vapor)

                                If Double.IsNaN(hl) Then hl = Double.NegativeInfinity
                                If Double.IsNaN(sl) Then sl = Double.NegativeInfinity

                                If S < sl Then
                                    xv = 0
                                    GoTo redirect2
                                ElseIf S >= sv Then
                                    xv = 1
                                    GoTo redirect2
                                Else
                                    If sl = Double.NegativeInfinity Then
                                        xv = 1.0#
                                        H = hv
                                    Else
                                        xv = (S - sl) / (sv - sl)
                                        H = xv * hv + (1 - xv) * hl
                                    End If
                                End If
                                xl = 1 - xv

                                'T = Me.CurrentMaterialStream.Fases(0).SPMProperties.temperature.GetValueOrDefault

                                Me.CurrentMaterialStream.Fases(3).SPMProperties.molarfraction = xl
                                Me.CurrentMaterialStream.Fases(2).SPMProperties.molarfraction = xv
                                Me.CurrentMaterialStream.Fases(3).SPMProperties.massfraction = xl
                                Me.CurrentMaterialStream.Fases(2).SPMProperties.massfraction = xv

                                i = 0
                                For Each subst In Me.CurrentMaterialStream.Fases(3).Componentes.Values
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

redirect2:                      result = Me.FlashBase.Flash_PS(RET_VMOL(Fase.Mixture), P, S, 0.0#, Me)

                                T = result(4)

                                xl = result(0)
                                xv = result(1)
                                xl2 = result(7)

                                Me.CurrentMaterialStream.Fases(3).SPMProperties.molarfraction = xl
                                Me.CurrentMaterialStream.Fases(4).SPMProperties.molarfraction = xl2
                                Me.CurrentMaterialStream.Fases(2).SPMProperties.molarfraction = xv

                                Dim Vx = result(2)
                                Dim Vy = result(3)
                                Dim Vx2 = result(8)

                                Dim FCL = Me.DW_CalcFugCoeff(Vx, T, P, State.Liquid)
                                Dim FCL2 = Me.DW_CalcFugCoeff(Vx2, T, P, State.Liquid)
                                Dim FCV = Me.DW_CalcFugCoeff(Vy, T, P, State.Vapor)

                                i = 0
                                For Each subst In Me.CurrentMaterialStream.Fases(3).Componentes.Values
                                    subst.FracaoMolar = Vx(i)
                                    subst.FugacityCoeff = FCL(i)
                                    subst.ActivityCoeff = 0
                                    subst.PartialVolume = 0
                                    subst.PartialPressure = 0
                                    i += 1
                                Next
                                i = 1
                                For Each subst In Me.CurrentMaterialStream.Fases(3).Componentes.Values
                                    subst.FracaoMassica = Me.AUX_CONVERT_MOL_TO_MASS(subst.Nome, 3)
                                    i += 1
                                Next
                                i = 0
                                For Each subst In Me.CurrentMaterialStream.Fases(4).Componentes.Values
                                    subst.FracaoMolar = Vx2(i)
                                    subst.FugacityCoeff = FCL2(i)
                                    subst.ActivityCoeff = 0
                                    subst.PartialVolume = 0
                                    subst.PartialPressure = 0
                                    i += 1
                                Next
                                i = 1
                                For Each subst In Me.CurrentMaterialStream.Fases(4).Componentes.Values
                                    subst.FracaoMassica = Me.AUX_CONVERT_MOL_TO_MASS(subst.Nome, 4)
                                    i += 1
                                Next
                                i = 0
                                For Each subst In Me.CurrentMaterialStream.Fases(2).Componentes.Values
                                    subst.FracaoMolar = Vy(i)
                                    subst.FugacityCoeff = FCV(i)
                                    subst.ActivityCoeff = 0
                                    subst.PartialVolume = 0
                                    subst.PartialPressure = 0
                                    i += 1
                                Next

                                i = 1
                                For Each subst In Me.CurrentMaterialStream.Fases(2).Componentes.Values
                                    subst.FracaoMassica = Me.AUX_CONVERT_MOL_TO_MASS(subst.Nome, 2)
                                    i += 1
                                Next

                                Me.CurrentMaterialStream.Fases(3).SPMProperties.massfraction = xl * Me.AUX_MMM(Fase.Liquid1) / (xl * Me.AUX_MMM(Fase.Liquid1) + xl2 * Me.AUX_MMM(Fase.Liquid2) + xv * Me.AUX_MMM(Fase.Vapor))
                                Me.CurrentMaterialStream.Fases(4).SPMProperties.massfraction = xl2 * Me.AUX_MMM(Fase.Liquid2) / (xl * Me.AUX_MMM(Fase.Liquid1) + xl2 * Me.AUX_MMM(Fase.Liquid2) + xv * Me.AUX_MMM(Fase.Vapor))
                                Me.CurrentMaterialStream.Fases(2).SPMProperties.massfraction = xv * Me.AUX_MMM(Fase.Vapor) / (xl * Me.AUX_MMM(Fase.Liquid1) + xl2 * Me.AUX_MMM(Fase.Liquid2) + xv * Me.AUX_MMM(Fase.Vapor))

                                Dim HM, HV, HL, HL2 As Double

                                If xl <> 0 Then HL = Me.DW_CalcEnthalpy(Vx, T, P, State.Liquid)
                                If xl2 <> 0 Then HL2 = Me.DW_CalcEnthalpy(Vx2, T, P, State.Liquid)
                                If xv <> 0 Then HV = Me.DW_CalcEnthalpy(Vy, T, P, State.Vapor)
                                HM = Me.CurrentMaterialStream.Fases(4).SPMProperties.massfraction.GetValueOrDefault * HL2 + Me.CurrentMaterialStream.Fases(3).SPMProperties.massfraction.GetValueOrDefault * HL + Me.CurrentMaterialStream.Fases(2).SPMProperties.massfraction.GetValueOrDefault * HV

                                H = HM

                            End If

                        Case FlashSpec.VAP

                            Dim KI(n) As Double

                            i = 0
                            Do
                                KI(i) = 0
                                i = i + 1
                            Loop Until i = n + 1

                            T = Me.CurrentMaterialStream.Fases(0).SPMProperties.temperature.GetValueOrDefault
                            P = Me.CurrentMaterialStream.Fases(0).SPMProperties.pressure.GetValueOrDefault

                            result = Me.FlashBase.Flash_PV(RET_VMOL(Fase.Mixture), P, xv, 0, Me)

                            T = result(4)

                            xl = result(0)
                            xv = result(1)
                            xl2 = result(7)

                            Me.CurrentMaterialStream.Fases(3).SPMProperties.molarfraction = xl
                            Me.CurrentMaterialStream.Fases(4).SPMProperties.molarfraction = xl2
                            Me.CurrentMaterialStream.Fases(2).SPMProperties.molarfraction = xv

                            Dim Vx = result(2)
                            Dim Vy = result(3)
                            Dim Vx2 = result(8)

                            Dim FCL = Me.DW_CalcFugCoeff(Vx, T, P, State.Liquid)
                            Dim FCL2 = Me.DW_CalcFugCoeff(Vx2, T, P, State.Liquid)
                            Dim FCV = Me.DW_CalcFugCoeff(Vy, T, P, State.Vapor)

                            i = 0
                            For Each subst In Me.CurrentMaterialStream.Fases(3).Componentes.Values
                                subst.FracaoMolar = Vx(i)
                                subst.FugacityCoeff = FCL(i)
                                subst.ActivityCoeff = 0
                                subst.PartialVolume = 0
                                subst.PartialPressure = 0
                                i += 1
                            Next
                            For Each subst In Me.CurrentMaterialStream.Fases(3).Componentes.Values
                                subst.FracaoMassica = Me.AUX_CONVERT_MOL_TO_MASS(subst.Nome, 3)
                            Next
                            i = 0
                            For Each subst In Me.CurrentMaterialStream.Fases(4).Componentes.Values
                                subst.FracaoMolar = Vx2(i)
                                subst.FugacityCoeff = FCL2(i)
                                subst.ActivityCoeff = 0
                                subst.PartialVolume = 0
                                subst.PartialPressure = 0
                                i += 1
                            Next
                            For Each subst In Me.CurrentMaterialStream.Fases(4).Componentes.Values
                                subst.FracaoMassica = Me.AUX_CONVERT_MOL_TO_MASS(subst.Nome, 4)
                            Next
                            i = 0
                            For Each subst In Me.CurrentMaterialStream.Fases(2).Componentes.Values
                                subst.FracaoMolar = Vy(i)
                                subst.FugacityCoeff = FCV(i)
                                subst.ActivityCoeff = 0
                                subst.PartialVolume = 0
                                subst.PartialPressure = 0
                                i += 1
                            Next
                            For Each subst In Me.CurrentMaterialStream.Fases(2).Componentes.Values
                                subst.FracaoMassica = Me.AUX_CONVERT_MOL_TO_MASS(subst.Nome, 2)
                            Next

                            Me.CurrentMaterialStream.Fases(3).SPMProperties.massfraction = xl * Me.AUX_MMM(Fase.Liquid1) / (xl * Me.AUX_MMM(Fase.Liquid1) + xl2 * Me.AUX_MMM(Fase.Liquid2) + xv * Me.AUX_MMM(Fase.Vapor))
                            Me.CurrentMaterialStream.Fases(4).SPMProperties.massfraction = xl2 * Me.AUX_MMM(Fase.Liquid2) / (xl * Me.AUX_MMM(Fase.Liquid1) + xl2 * Me.AUX_MMM(Fase.Liquid2) + xv * Me.AUX_MMM(Fase.Vapor))
                            Me.CurrentMaterialStream.Fases(2).SPMProperties.massfraction = xv * Me.AUX_MMM(Fase.Vapor) / (xl * Me.AUX_MMM(Fase.Liquid1) + xl2 * Me.AUX_MMM(Fase.Liquid2) + xv * Me.AUX_MMM(Fase.Vapor))

                            Dim HM, HV, HL, HL2 As Double

                            If xl <> 0 Then HL = Me.DW_CalcEnthalpy(Vx, T, P, State.Liquid)
                            If xl2 <> 0 Then HL2 = Me.DW_CalcEnthalpy(Vx2, T, P, State.Liquid)
                            If xv <> 0 Then HV = Me.DW_CalcEnthalpy(Vy, T, P, State.Vapor)
                            HM = Me.CurrentMaterialStream.Fases(4).SPMProperties.massfraction.GetValueOrDefault * HL2 + Me.CurrentMaterialStream.Fases(3).SPMProperties.massfraction.GetValueOrDefault * HL + Me.CurrentMaterialStream.Fases(2).SPMProperties.massfraction.GetValueOrDefault * HV

                            H = HM

                            Dim SM, SV, SL, SL2 As Double

                            If xl <> 0 Then SL = Me.DW_CalcEntropy(Vx, T, P, State.Liquid)
                            If xl2 <> 0 Then SL2 = Me.DW_CalcEntropy(Vx2, T, P, State.Liquid)
                            If xv <> 0 Then SV = Me.DW_CalcEntropy(Vy, T, P, State.Vapor)
                            SM = Me.CurrentMaterialStream.Fases(4).SPMProperties.massfraction.GetValueOrDefault * SL2 + Me.CurrentMaterialStream.Fases(3).SPMProperties.massfraction.GetValueOrDefault * SL + Me.CurrentMaterialStream.Fases(2).SPMProperties.massfraction.GetValueOrDefault * SV

                            S = SM

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

                'If xl <= 0.00001 Then
                '    xl = 0
                '    With .Fases(3).SPMProperties
                '        .activity = 0
                '        .activityCoefficient = 0
                '        .compressibility = 0
                '        .compressibilityFactor = 0
                '        .density = 0
                '        .enthalpy = 0
                '        .enthalpyF = 0
                '        .entropy = 0
                '        .entropyF = 0
                '        .molar_enthalpy = 0
                '        .molar_enthalpyF = 0
                '        .molar_entropy = 0
                '        .molar_entropyF = 0
                '        .excessEnthalpy = 0
                '        .excessEntropy = 0
                '        .fugacity = 0
                '        .fugacityCoefficient = 0
                '        .heatCapacityCp = 0
                '        .heatCapacityCv = 0
                '        .jouleThomsonCoefficient = 0
                '        .kinematic_viscosity = 0
                '        .logFugacityCoefficient = 0
                '        .massflow = 0
                '        .massfraction = 0
                '        .molarflow = 0
                '        .molarfraction = 0
                '        .molecularWeight = 0
                '        .pressure = 0
                '        .speedOfSound = 0
                '        .temperature = 0
                '        .thermalConductivity = 0
                '        .viscosity = 0
                '        .volumetric_flow = 0
                '    End With
                'End If
                'If xl2 <= 0.00001 Then
                '    xl2 = 0
                '    With .Fases(4).SPMProperties
                '        .activity = 0
                '        .activityCoefficient = 0
                '        .compressibility = 0
                '        .compressibilityFactor = 0
                '        .density = 0
                '        .enthalpy = 0
                '        .enthalpyF = 0
                '        .entropy = 0
                '        .entropyF = 0
                '        .molar_enthalpy = 0
                '        .molar_enthalpyF = 0
                '        .molar_entropy = 0
                '        .molar_entropyF = 0
                '        .excessEnthalpy = 0
                '        .excessEntropy = 0
                '        .fugacity = 0
                '        .fugacityCoefficient = 0
                '        .heatCapacityCp = 0
                '        .heatCapacityCv = 0
                '        .jouleThomsonCoefficient = 0
                '        .kinematic_viscosity = 0
                '        .logFugacityCoefficient = 0
                '        '.massflow = 0
                '        '.massfraction = 0
                '        '.molarflow = 0
                '        '.molarfraction = 0
                '        .molecularWeight = 0
                '        .pressure = 0
                '        .speedOfSound = 0
                '        .temperature = 0
                '        .thermalConductivity = 0
                '        .viscosity = 0
                '        .volumetric_flow = 0
                '    End With
                'End If
                'If xv <= 0.00001 Then
                '    xv = 0
                '    With .Fases(2).SPMProperties
                '        .activity = 0
                '        .activityCoefficient = 0
                '        .compressibility = 0
                '        .compressibilityFactor = 0
                '        .density = 0
                '        .enthalpy = 0
                '        .enthalpyF = 0
                '        .entropy = 0
                '        .entropyF = 0
                '        .molar_enthalpy = 0
                '        .molar_enthalpyF = 0
                '        .molar_entropy = 0
                '        .molar_entropyF = 0
                '        .excessEnthalpy = 0
                '        .excessEntropy = 0
                '        .fugacity = 0
                '        .fugacityCoefficient = 0
                '        .heatCapacityCp = 0
                '        .heatCapacityCv = 0
                '        .jouleThomsonCoefficient = 0
                '        .kinematic_viscosity = 0
                '        .logFugacityCoefficient = 0
                '        '.massflow = 0
                '        '.massfraction = 0
                '        '.molarflow = 0
                '        '.molarfraction = 0
                '        .molecularWeight = 0
                '        .pressure = 0
                '        .speedOfSound = 0
                '        .temperature = 0
                '        .thermalConductivity = 0
                '        .viscosity = 0
                '        .volumetric_flow = 0
                '    End With
                'End If

            End With

            Me.CurrentMaterialStream.AtEquilibrium = True


        End Sub


#End Region

    End Class

End Namespace