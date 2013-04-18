'    Extended UNIQUAC Property Package 
'    Copyright 2013 Daniel Wagner O. de Medeiros
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
<System.Serializable()> Public Class ExUNIQUACPropertyPackage

        Inherits DWSIM.SimulationObjects.PropertyPackages.PropertyPackage

        Public Shadows Const ClassId As String = "f3eeff51-eccd-4c15-b4b0-1eb4d13c61f3"

        Private m_props As New DWSIM.SimulationObjects.PropertyPackages.Auxiliary.PROPS
        Public m_uni As New DWSIM.SimulationObjects.PropertyPackages.Auxiliary.ExUNIQUAC
        Public m_elec As New DWSIM.SimulationObjects.PropertyPackages.Auxiliary.Electrolyte
        Public m_elecflash As New DWSIM.SimulationObjects.PropertyPackages.Auxiliary.FlashAlgorithms.ElectrolyteSVLE
        Private m_id As New DWSIM.SimulationObjects.PropertyPackages.Auxiliary.Ideal

        Public Sub New(ByVal comode As Boolean)
            MyBase.New(comode)
        End Sub

        Public Sub New()

            MyBase.New()

            Me.IsConfigurable = True
            Me.ConfigForm = New FormConfigExUNIQUAC
            Me._packagetype = PropertyPackages.PackageType.ActivityCoefficient
            Me.IsElectrolytePP = True

        End Sub

        Public Overrides Sub ReconfigureConfigForm()
            MyBase.ReconfigureConfigForm()
            Me.ConfigForm = New FormConfigExUNIQUAC
        End Sub

#Region "    DWSIM Functions"

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
                Case Fase.Solid
                    state = "S"
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
                    result = 0.0#
                    Me.CurrentMaterialStream.Fases(phaseID).SPMProperties.compressibilityFactor = result
                Case "heatcapacity", "heatcapacitycp"
                    If phase = Fase.Solid Then
                        Dim constprops As New List(Of ConstantProperties)
                        For Each su As Substancia In Me.CurrentMaterialStream.Fases(0).Componentes.Values
                            constprops.Add(su.ConstantProperties)
                        Next
                        result = Me.m_elec.HeatCapacityCp(T, RET_VMOL(phase), constprops)
                        Me.CurrentMaterialStream.Fases(phaseID).SPMProperties.heatCapacityCp = result
                    Else
                        resultObj = m_id.CpCv(state, T, P, RET_VMOL(phase), RET_VKij(), RET_VMAS(phase), RET_VTC(), RET_VPC(), RET_VCP(T), RET_VMM(), RET_VW(), RET_VZRa())
                        Me.CurrentMaterialStream.Fases(phaseID).SPMProperties.heatCapacityCp = resultObj(1)
                    End If
                Case "heatcapacitycv"
                    If phase = Fase.Solid Then
                        Dim constprops As New List(Of ConstantProperties)
                        For Each su As Substancia In Me.CurrentMaterialStream.Fases(0).Componentes.Values
                            constprops.Add(su.ConstantProperties)
                        Next
                        result = Me.m_elec.HeatCapacityCp(T, RET_VMOL(phase), constprops)
                        Me.CurrentMaterialStream.Fases(phaseID).SPMProperties.heatCapacityCv = result
                    Else
                        resultObj = m_id.CpCv(state, T, P, RET_VMOL(phase), RET_VKij(), RET_VMAS(phase), RET_VTC(), RET_VPC(), RET_VCP(T), RET_VMM(), RET_VW(), RET_VZRa())
                        Me.CurrentMaterialStream.Fases(phaseID).SPMProperties.heatCapacityCv = resultObj(2)
                    End If
                Case "enthalpy", "enthalpynf"
                    If phase = Fase.Solid Then
                        Dim constprops As New List(Of ConstantProperties)
                        For Each su As Substancia In Me.CurrentMaterialStream.Fases(0).Componentes.Values
                            constprops.Add(su.ConstantProperties)
                        Next
                        result = Me.m_elec.SolidEnthalpy(T, RET_VMOL(phase), constprops)
                        Me.CurrentMaterialStream.Fases(phaseID).SPMProperties.enthalpy = result
                        result = Me.CurrentMaterialStream.Fases(phaseID).SPMProperties.enthalpy.GetValueOrDefault * Me.CurrentMaterialStream.Fases(phaseID).SPMProperties.molecularWeight.GetValueOrDefault
                        Me.CurrentMaterialStream.Fases(phaseID).SPMProperties.molar_enthalpy = result
                    Else
                        result = m_id.H_RA_MIX(state, T, P, RET_VMOL(phase), RET_VKij, RET_VTC(), RET_VPC(), RET_VW(), RET_VMM(), Me.RET_Hid(298.15, T, phase), Me.RET_VHVAP(T))
                        Me.CurrentMaterialStream.Fases(phaseID).SPMProperties.enthalpy = result
                        result = Me.CurrentMaterialStream.Fases(phaseID).SPMProperties.enthalpy.GetValueOrDefault * Me.CurrentMaterialStream.Fases(phaseID).SPMProperties.molecularWeight.GetValueOrDefault
                        Me.CurrentMaterialStream.Fases(phaseID).SPMProperties.molar_enthalpy = result
                    End If
                Case "entropy", "entropynf"
                    If phase = Fase.Solid Then
                        result = 0.0#
                        Me.CurrentMaterialStream.Fases(phaseID).SPMProperties.entropy = result
                        result = Me.CurrentMaterialStream.Fases(phaseID).SPMProperties.entropy.GetValueOrDefault * Me.CurrentMaterialStream.Fases(phaseID).SPMProperties.molecularWeight.GetValueOrDefault
                        Me.CurrentMaterialStream.Fases(phaseID).SPMProperties.molar_entropy = result
                    Else
                        result = m_id.S_RA_MIX(state, T, P, RET_VMOL(phase), RET_VKij, RET_VTC(), RET_VPC(), RET_VW(), RET_VMM(), Me.RET_Sid(298.15, T, P, phase), Me.RET_VHVAP(T), Me.RET_Hid(298.15, T, phase))
                        Me.CurrentMaterialStream.Fases(phaseID).SPMProperties.entropy = result
                        result = Me.CurrentMaterialStream.Fases(phaseID).SPMProperties.entropy.GetValueOrDefault * Me.CurrentMaterialStream.Fases(phaseID).SPMProperties.molecularWeight.GetValueOrDefault
                        Me.CurrentMaterialStream.Fases(phaseID).SPMProperties.molar_entropy = result
                    End If
                Case "excessenthalpy"
                    result = m_id.H_RA_MIX(state, T, P, RET_VMOL(phase), RET_VKij, RET_VTC(), RET_VPC(), RET_VW(), RET_VMM(), 0, Me.RET_VHVAP(T))
                    Me.CurrentMaterialStream.Fases(phaseID).SPMProperties.excessEnthalpy = result
                Case "excessentropy"
                    result = m_id.S_RA_MIX(state, T, P, RET_VMOL(phase), RET_VKij, RET_VTC(), RET_VPC(), RET_VW(), RET_VMM(), 0, Me.RET_VHVAP(T), Me.RET_Hid(298.15, T, phase))
                    Me.CurrentMaterialStream.Fases(phaseID).SPMProperties.excessEntropy = result
                Case "enthalpyf"
                    Dim entF As Double = Me.AUX_HFm25(phase)
                    result = m_id.H_RA_MIX(state, T, P, RET_VMOL(phase), RET_VKij, RET_VTC(), RET_VPC(), RET_VW(), RET_VMM(), Me.RET_Hid(298.15, T, phase), Me.RET_VHVAP(T))
                    Me.CurrentMaterialStream.Fases(phaseID).SPMProperties.enthalpyF = result + entF
                    result = Me.CurrentMaterialStream.Fases(phaseID).SPMProperties.enthalpyF.GetValueOrDefault * Me.CurrentMaterialStream.Fases(phaseID).SPMProperties.molecularWeight.GetValueOrDefault
                    Me.CurrentMaterialStream.Fases(phaseID).SPMProperties.molar_enthalpyF = result
                Case "entropyf"
                    Dim entF As Double = Me.AUX_SFm25(phase)
                    result = m_id.S_RA_MIX(state, T, P, RET_VMOL(phase), RET_VKij, RET_VTC(), RET_VPC(), RET_VW(), RET_VMM(), Me.RET_Sid(298.15, T, P, phase), Me.RET_VHVAP(T), Me.RET_Hid(298.15, T, phase))
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
                Case "osmoticcoefficient"
                    Dim constprops As New List(Of ConstantProperties)
                    For Each su As Substancia In Me.CurrentMaterialStream.Fases(0).Componentes.Values
                        constprops.Add(su.ConstantProperties)
                    Next
                    Me.CurrentMaterialStream.Fases(phaseID).SPMProperties.osmoticCoefficient = Me.m_elec.OsmoticCoeff(RET_VMOL(phase), Me.m_uni.GAMMA_MR(T, RET_VMOL(phase), constprops), constprops)
                Case "freezingpoint"
                    Dim constprops As New List(Of ConstantProperties)
                    For Each su As Substancia In Me.CurrentMaterialStream.Fases(0).Componentes.Values
                        constprops.Add(su.ConstantProperties)
                    Next
                    Me.CurrentMaterialStream.Fases(phaseID).SPMProperties.freezingPoint = Me.m_elec.FreezingPointDepression(RET_VMOL(phase), Me.m_uni.GAMMA_MR(T, RET_VMOL(phase), constprops), constprops)(0)
                Case "freezingpointdepression"
                    Dim constprops As New List(Of ConstantProperties)
                    For Each su As Substancia In Me.CurrentMaterialStream.Fases(0).Componentes.Values
                        constprops.Add(su.ConstantProperties)
                    Next
                    Me.CurrentMaterialStream.Fases(phaseID).SPMProperties.freezingPointDepression = Me.m_elec.FreezingPointDepression(RET_VMOL(phase), Me.m_uni.GAMMA_MR(T, RET_VMOL(phase), constprops), constprops)(1)
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

            If phaseID = 3 Then

                Me.DW_CalcProp("osmoticcoefficient", PropertyPackages.Fase.Liquid1)
                Me.DW_CalcProp("freezingpoint", PropertyPackages.Fase.Liquid1)
                Me.DW_CalcProp("freezingpointdepression", PropertyPackages.Fase.Liquid1)

            End If

            If phaseID = 3 Or phaseID = 4 Or phaseID = 5 Or phaseID = 6 Then

                result = Me.AUX_LIQDENS(T, P, 0.0#, phaseID, False)
                Me.CurrentMaterialStream.Fases(phaseID).SPMProperties.density = result
                result = Me.m_id.H_RA_MIX("L", T, P, RET_VMOL(dwpl), RET_VKij, RET_VTC(), RET_VPC(), RET_VW(), RET_VMM(), Me.RET_Hid(298.15, T, dwpl), Me.RET_VHVAP(T))
                Me.CurrentMaterialStream.Fases(phaseID).SPMProperties.enthalpy = result
                result = Me.m_id.S_RA_MIX("L", T, P, RET_VMOL(dwpl), RET_VKij, RET_VTC(), RET_VPC(), RET_VW(), RET_VMM(), Me.RET_Sid(298.15, T, P, dwpl), Me.RET_VHVAP(T), Me.RET_Hid(298.15, T, dwpl))
                Me.CurrentMaterialStream.Fases(phaseID).SPMProperties.entropy = result
                result = 0
                Me.CurrentMaterialStream.Fases(phaseID).SPMProperties.compressibilityFactor = result
                resultObj = Me.m_id.CpCv("L", T, P, RET_VMOL(dwpl), RET_VKij(), RET_VMAS(dwpl), RET_VTC(), RET_VPC(), RET_VCP(T), RET_VMM(), RET_VW(), RET_VZRa())
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
                result = Me.m_id.H_RA_MIX("V", T, P, RET_VMOL(fase.Vapor), RET_VKij, RET_VTC(), RET_VPC(), RET_VW(), RET_VMM(), Me.RET_Hid(298.15, T, fase.Vapor), Me.RET_VHVAP(T))
                Me.CurrentMaterialStream.Fases(phaseID).SPMProperties.enthalpy = result
                result = Me.m_id.S_RA_MIX("V", T, P, RET_VMOL(fase.Vapor), RET_VKij, RET_VTC(), RET_VPC(), RET_VW(), RET_VMM(), Me.RET_Sid(298.15, T, P, fase.Vapor), Me.RET_VHVAP(T), Me.RET_Hid(298.15, T, fase.Vapor))
                Me.CurrentMaterialStream.Fases(phaseID).SPMProperties.entropy = result
                result = 1
                Me.CurrentMaterialStream.Fases(phaseID).SPMProperties.compressibilityFactor = result
                result = Me.AUX_CPm(PropertyPackages.Fase.Vapor, T)
                resultObj = Me.m_id.CpCv("V", T, P, RET_VMOL(PropertyPackages.Fase.Vapor), RET_VKij(), RET_VMAS(PropertyPackages.Fase.Vapor), RET_VTC(), RET_VPC(), RET_VCP(T), RET_VMM(), RET_VW(), RET_VZRa())
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
                result = Me.m_elec.SolidEnthalpy(T, RET_VMOL(PropertyPackages.Fase.Solid), constprops)
                Me.CurrentMaterialStream.Fases(phaseID).SPMProperties.enthalpy = result
                Me.CurrentMaterialStream.Fases(phaseID).SPMProperties.entropy = 0.0# 'result
                result = 1
                Me.CurrentMaterialStream.Fases(phaseID).SPMProperties.compressibilityFactor = 0.0# 'result
                result = Me.m_elec.HeatCapacityCp(T, RET_VMOL(PropertyPackages.Fase.Solid), constprops)
                Me.CurrentMaterialStream.Fases(phaseID).SPMProperties.heatCapacityCp = result
                Me.CurrentMaterialStream.Fases(phaseID).SPMProperties.heatCapacityCv = result
                result = Me.AUX_MMM(fase)
                Me.CurrentMaterialStream.Fases(phaseID).SPMProperties.molecularWeight = result
                result = Me.CurrentMaterialStream.Fases(phaseID).SPMProperties.enthalpy.GetValueOrDefault * Me.CurrentMaterialStream.Fases(phaseID).SPMProperties.molecularWeight.GetValueOrDefault
                Me.CurrentMaterialStream.Fases(phaseID).SPMProperties.molar_enthalpy = 0.0# 'result
                result = Me.CurrentMaterialStream.Fases(phaseID).SPMProperties.entropy.GetValueOrDefault * Me.CurrentMaterialStream.Fases(phaseID).SPMProperties.molecularWeight.GetValueOrDefault
                Me.CurrentMaterialStream.Fases(phaseID).SPMProperties.molar_entropy = 0.0# 'result
                result = Me.AUX_CONDTG(T, P)
                Me.CurrentMaterialStream.Fases(phaseID).SPMProperties.thermalConductivity = 0.0# 'result
                result = Me.AUX_VAPVISCm(T, Me.CurrentMaterialStream.Fases(phaseID).SPMProperties.density.GetValueOrDefault, Me.AUX_MMM(fase))
                Me.CurrentMaterialStream.Fases(phaseID).SPMProperties.viscosity = 0.0# 'result
                Me.CurrentMaterialStream.Fases(phaseID).SPMProperties.kinematic_viscosity = 0.0# 'result / Me.CurrentMaterialStream.Fases(phaseID).SPMProperties.density.Value

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
                H = Me.m_id.H_RA_MIX("L", T, P, Vx, RET_VKij(), RET_VTC, RET_VPC, RET_VW, RET_VMM, Me.RET_Hid(298.15, T, Vx), Me.RET_VHVAP(T))
            Else
                H = Me.m_id.H_RA_MIX("V", T, P, Vx, RET_VKij(), RET_VTC, RET_VPC, RET_VW, RET_VMM, Me.RET_Hid(298.15, T, Vx), Me.RET_VHVAP(T))
            End If

            Return H

        End Function

        Public Overrides Function DW_CalcEnthalpyDeparture(ByVal Vx As System.Array, ByVal T As Double, ByVal P As Double, ByVal st As State) As Double
            Dim H As Double

            If st = State.Liquid Then
                H = Me.m_id.H_RA_MIX("L", T, P, Vx, RET_VKij(), RET_VTC, RET_VPC, RET_VW, RET_VMM, 0, Me.RET_VHVAP(T))
            Else
                H = Me.m_id.H_RA_MIX("V", T, P, Vx, RET_VKij(), RET_VTC, RET_VPC, RET_VW, RET_VMM, 0, Me.RET_VHVAP(T))
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

            Dim S As Double

            If st = State.Liquid Then
                S = Me.m_id.S_RA_MIX("L", T, P, Vx, RET_VKij(), RET_VTC, RET_VPC, RET_VW, RET_VMM, Me.RET_Sid(298.15, T, P, Vx), Me.RET_VHVAP(T), Me.RET_Hid(298.15, T, Vx))
            Else
                S = Me.m_id.S_RA_MIX("V", T, P, Vx, RET_VKij(), RET_VTC, RET_VPC, RET_VW, RET_VMM, Me.RET_Sid(298.15, T, P, Vx), Me.RET_VHVAP(T), Me.RET_Hid(298.15, T, Vx))
            End If

            Return S

        End Function

        Public Overrides Function DW_CalcEntropyDeparture(ByVal Vx As System.Array, ByVal T As Double, ByVal P As Double, ByVal st As State) As Double
            Dim S As Double

            If st = State.Liquid Then
                S = Me.m_id.S_RA_MIX("L", T, P, Vx, RET_VKij(), RET_VTC, RET_VPC, RET_VW, RET_VMM, 0, Me.RET_VHVAP(T), Me.RET_Hid(298.15, T, Vx))
            Else
                S = Me.m_id.S_RA_MIX("V", T, P, Vx, RET_VKij(), RET_VTC, RET_VPC, RET_VW, RET_VMM, 0, Me.RET_VHVAP(T), Me.RET_Hid(298.15, T, Vx))
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

#Region "    CalcEquilibrium Override"

        Public Overrides Sub DW_CalcEquilibrium(spec1 As FlashSpec, spec2 As FlashSpec)

            Me.CurrentMaterialStream.AtEquilibrium = False

            Dim P, T, H, S, xv, xl, xs, M As Double
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
            Me.m_elecflash = New Auxiliary.FlashAlgorithms.ElectrolyteSVLE
            Me.m_elecflash.CompoundProperties = constprops
            Me.m_elecflash.proppack = Me

            Select Case spec1

                Case FlashSpec.T

                    Select Case spec2

                        Case FlashSpec.P

                            T = Me.CurrentMaterialStream.Fases(0).SPMProperties.temperature.GetValueOrDefault
                            P = Me.CurrentMaterialStream.Fases(0).SPMProperties.pressure.GetValueOrDefault

                            result = Me.m_elecflash.Flash_PT(RET_VMOL(Fase.Mixture), T, P)

                            xl = result("LiquidPhaseMoleFraction")
                            xv = result("VaporPhaseMoleFraction")
                            xs = result("SolidPhaseMoleFraction")

                            Me.CurrentMaterialStream.Fases(3).SPMProperties.molarfraction = xl
                            Me.CurrentMaterialStream.Fases(4).SPMProperties.molarfraction = 0.0#
                            Me.CurrentMaterialStream.Fases(2).SPMProperties.molarfraction = xv
                            Me.CurrentMaterialStream.Fases(7).SPMProperties.molarfraction = xs

                            Dim Vz = result("LiquidPhaseMolarComposition")

                            i = 0
                            For Each subst In Me.CurrentMaterialStream.Fases(0).Componentes.Values
                                subst.MolarFlow = Vz(i) * Me.CurrentMaterialStream.Fases(0).SPMProperties.molarflow
                                i += 1
                            Next

                            For Each subst In Me.CurrentMaterialStream.Fases(0).Componentes.Values
                                subst.MassFlow = subst.MolarFlow * subst.ConstantProperties.Molar_Weight / 1000
                            Next

                            M = result("MoleSum")

                            Me.CurrentMaterialStream.Fases(0).SPMProperties.molarflow *= M

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
                            Me.CurrentMaterialStream.Fases(3).SPMProperties.massfraction = 0.0#
                            Me.CurrentMaterialStream.Fases(7).SPMProperties.massfraction = xs * Me.AUX_MMM(Fase.Solid) / (xl * Me.AUX_MMM(Fase.Liquid1) + xs * Me.AUX_MMM(Fase.Solid) + xv * Me.AUX_MMM(Fase.Vapor))
                            Me.CurrentMaterialStream.Fases(2).SPMProperties.massfraction = xv * Me.AUX_MMM(Fase.Vapor) / (xl * Me.AUX_MMM(Fase.Liquid1) + xs * Me.AUX_MMM(Fase.Solid) + xv * Me.AUX_MMM(Fase.Vapor))

                            Dim HM, HV, HL, HS As Double

                            If xl <> 0 Then HL = Me.DW_CalcEnthalpy(Vx, T, P, State.Liquid)
                            If xs <> 0 Then HS = Me.DW_CalcEnthalpy(Vs, T, P, State.Solid)
                            If xv <> 0 Then HV = Me.DW_CalcEnthalpy(Vy, T, P, State.Vapor)
                            HM = Me.CurrentMaterialStream.Fases(4).SPMProperties.massfraction.GetValueOrDefault * HS + Me.CurrentMaterialStream.Fases(7).SPMProperties.massfraction.GetValueOrDefault * HS + Me.CurrentMaterialStream.Fases(2).SPMProperties.massfraction.GetValueOrDefault * HV

                            H = HM

                            Dim SM, SV, SL, SS As Double

                            If xl <> 0 Then SL = Me.DW_CalcEntropy(Vx, T, P, State.Liquid)
                            If xs <> 0 Then SS = Me.DW_CalcEntropy(Vs, T, P, State.Solid)
                            If xv <> 0 Then SV = Me.DW_CalcEntropy(Vy, T, P, State.Vapor)
                            SM = Me.CurrentMaterialStream.Fases(4).SPMProperties.massfraction.GetValueOrDefault * SS + Me.CurrentMaterialStream.Fases(7).SPMProperties.massfraction.GetValueOrDefault * SS + Me.CurrentMaterialStream.Fases(2).SPMProperties.massfraction.GetValueOrDefault * SV

                            S = SM

                        Case FlashSpec.H

                            Throw New Exception(DWSIM.App.GetLocalString("PropPack_FlashTHNotSupported"))

                        Case FlashSpec.S

                            Throw New Exception(DWSIM.App.GetLocalString("PropPack_FlashTSNotSupported"))

                        Case FlashSpec.VAP

                            'Dim KI(n) As Double

                            'i = 0
                            'Do
                            '    KI(i) = 0
                            '    i = i + 1
                            'Loop Until i = n + 1

                            'T = Me.CurrentMaterialStream.Fases(0).SPMProperties.temperature.GetValueOrDefault
                            'P = Me.CurrentMaterialStream.Fases(0).SPMProperties.pressure.GetValueOrDefault

                            'result = Me.FlashBase.Flash_TV(RET_VMOL(Fase.Mixture), T, xv, 0, Me)

                            'P = result(4)

                            'xl = result(0)
                            'xv = result(1)
                            'xl2 = result(7)

                            'Me.CurrentMaterialStream.Fases(3).SPMProperties.molarfraction = xl
                            'Me.CurrentMaterialStream.Fases(4).SPMProperties.molarfraction = xl2
                            'Me.CurrentMaterialStream.Fases(2).SPMProperties.molarfraction = xv

                            'Dim Vx = result(2)
                            'Dim Vy = result(3)
                            'Dim Vx2 = result(8)

                            'Dim FCL = Me.DW_CalcFugCoeff(Vx, T, P, State.Liquid)
                            'Dim FCL2 = Me.DW_CalcFugCoeff(Vx2, T, P, State.Liquid)
                            'Dim FCV = Me.DW_CalcFugCoeff(Vy, T, P, State.Vapor)

                            'i = 0
                            'For Each subst In Me.CurrentMaterialStream.Fases(3).Componentes.Values
                            '    subst.FracaoMolar = Vx(i)
                            '    subst.FugacityCoeff = FCL(i)
                            '    subst.ActivityCoeff = 0
                            '    subst.PartialVolume = 0
                            '    subst.PartialPressure = 0
                            '    i += 1
                            'Next
                            'For Each subst In Me.CurrentMaterialStream.Fases(3).Componentes.Values
                            '    subst.FracaoMassica = Me.AUX_CONVERT_MOL_TO_MASS(subst.Nome, 3)
                            'Next
                            'i = 0
                            'For Each subst In Me.CurrentMaterialStream.Fases(4).Componentes.Values
                            '    subst.FracaoMolar = Vx2(i)
                            '    subst.FugacityCoeff = FCL2(i)
                            '    subst.ActivityCoeff = 0
                            '    subst.PartialVolume = 0
                            '    subst.PartialPressure = 0
                            '    i += 1
                            'Next
                            'For Each subst In Me.CurrentMaterialStream.Fases(4).Componentes.Values
                            '    subst.FracaoMassica = Me.AUX_CONVERT_MOL_TO_MASS(subst.Nome, 4)
                            'Next
                            'i = 0
                            'For Each subst In Me.CurrentMaterialStream.Fases(2).Componentes.Values
                            '    subst.FracaoMolar = Vy(i)
                            '    subst.FugacityCoeff = FCV(i)
                            '    subst.ActivityCoeff = 0
                            '    subst.PartialVolume = 0
                            '    subst.PartialPressure = 0
                            '    i += 1
                            'Next
                            'For Each subst In Me.CurrentMaterialStream.Fases(2).Componentes.Values
                            '    subst.FracaoMassica = Me.AUX_CONVERT_MOL_TO_MASS(subst.Nome, 2)
                            'Next

                            'Me.CurrentMaterialStream.Fases(3).SPMProperties.massfraction = xl * Me.AUX_MMM(Fase.Liquid1) / (xl * Me.AUX_MMM(Fase.Liquid1) + xl2 * Me.AUX_MMM(Fase.Liquid2) + xv * Me.AUX_MMM(Fase.Vapor))
                            'Me.CurrentMaterialStream.Fases(4).SPMProperties.massfraction = xl2 * Me.AUX_MMM(Fase.Liquid2) / (xl * Me.AUX_MMM(Fase.Liquid1) + xl2 * Me.AUX_MMM(Fase.Liquid2) + xv * Me.AUX_MMM(Fase.Vapor))
                            'Me.CurrentMaterialStream.Fases(2).SPMProperties.massfraction = xv * Me.AUX_MMM(Fase.Vapor) / (xl * Me.AUX_MMM(Fase.Liquid1) + xl2 * Me.AUX_MMM(Fase.Liquid2) + xv * Me.AUX_MMM(Fase.Vapor))

                            'Dim HM, HV, HL, HL2 As Double

                            'If xl <> 0 Then HL = Me.DW_CalcEnthalpy(Vx, T, P, State.Liquid)
                            'If xl2 <> 0 Then HL2 = Me.DW_CalcEnthalpy(Vx2, T, P, State.Liquid)
                            'If xv <> 0 Then HV = Me.DW_CalcEnthalpy(Vy, T, P, State.Vapor)
                            'HM = Me.CurrentMaterialStream.Fases(4).SPMProperties.massfraction.GetValueOrDefault * HL2 + Me.CurrentMaterialStream.Fases(3).SPMProperties.massfraction.GetValueOrDefault * HL + Me.CurrentMaterialStream.Fases(2).SPMProperties.massfraction.GetValueOrDefault * HV

                            'H = HM

                            'Dim SM, SV, SL, SL2 As Double

                            'If xl <> 0 Then SL = Me.DW_CalcEntropy(Vx, T, P, State.Liquid)
                            'If xl2 <> 0 Then SL2 = Me.DW_CalcEntropy(Vx2, T, P, State.Liquid)
                            'If xv <> 0 Then SV = Me.DW_CalcEntropy(Vy, T, P, State.Vapor)
                            'SM = Me.CurrentMaterialStream.Fases(4).SPMProperties.massfraction.GetValueOrDefault * SL2 + Me.CurrentMaterialStream.Fases(3).SPMProperties.massfraction.GetValueOrDefault * SL + Me.CurrentMaterialStream.Fases(2).SPMProperties.massfraction.GetValueOrDefault * SV

                            'S = SM

                    End Select

                    '                Case FlashSpec.P

                    '                    Select Case spec2

                    '                        Case FlashSpec.H

                    '                            T = Me.CurrentMaterialStream.Fases(0).SPMProperties.temperature.GetValueOrDefault
                    '                            P = Me.CurrentMaterialStream.Fases(0).SPMProperties.pressure.GetValueOrDefault

                    '                            If Double.IsNaN(H) Or Double.IsInfinity(H) Then H = Me.CurrentMaterialStream.Fases(0).SPMProperties.molar_enthalpy.GetValueOrDefault / Me.CurrentMaterialStream.Fases(0).SPMProperties.molecularWeight.GetValueOrDefault

                    '                            If Me.AUX_IS_SINGLECOMP(Fase.Mixture) Then

                    '                                Dim hl, hv, sl, sv As Double
                    '                                Dim vz As Object = Me.RET_VMOL(Fase.Mixture)
                    '                                result = Me.FlashBase.Flash_PH(RET_VMOL(Fase.Mixture), P, H, 0.0#, Me)

                    '                                T = result(4)

                    '                                hl = Me.DW_CalcEnthalpy(vz, T, P, State.Liquid)
                    '                                hv = Me.DW_CalcEnthalpy(vz, T, P, State.Vapor)
                    '                                sl = Me.DW_CalcEntropy(vz, T, P, State.Liquid)
                    '                                sv = Me.DW_CalcEntropy(vz, T, P, State.Vapor)

                    '                                If Double.IsNaN(hl) Then hl = Double.NegativeInfinity
                    '                                If Double.IsNaN(sl) Then sl = Double.NegativeInfinity

                    '                                If H < hl Then
                    '                                    xv = 0
                    '                                    GoTo redirect
                    '                                ElseIf H >= hv Then
                    '                                    xv = 1
                    '                                    GoTo redirect
                    '                                Else
                    '                                    If hl = Double.NegativeInfinity Then
                    '                                        xv = 1.0#
                    '                                        S = sv
                    '                                    Else
                    '                                        xv = (H - hl) / (hv - hl)
                    '                                        S = xv * sv + (1 - xv) * sl
                    '                                    End If
                    '                                End If
                    '                                xl = 1 - xv

                    '                                'T = Me.CurrentMaterialStream.Fases(0).SPMProperties.temperature.GetValueOrDefault

                    '                                Me.CurrentMaterialStream.Fases(3).SPMProperties.molarfraction = xl
                    '                                Me.CurrentMaterialStream.Fases(2).SPMProperties.molarfraction = xv
                    '                                Me.CurrentMaterialStream.Fases(3).SPMProperties.massfraction = xl
                    '                                Me.CurrentMaterialStream.Fases(2).SPMProperties.massfraction = xv

                    '                                i = 0
                    '                                For Each subst In Me.CurrentMaterialStream.Fases(3).Componentes.Values
                    '                                    subst.FracaoMolar = vz(i)
                    '                                    subst.FugacityCoeff = 1
                    '                                    subst.ActivityCoeff = 1
                    '                                    subst.PartialVolume = 0
                    '                                    subst.PartialPressure = P
                    '                                    subst.FracaoMassica = vz(i)
                    '                                    i += 1
                    '                                Next
                    '                                i = 0
                    '                                For Each subst In Me.CurrentMaterialStream.Fases(2).Componentes.Values
                    '                                    subst.FracaoMolar = vz(i)
                    '                                    subst.FugacityCoeff = 1
                    '                                    subst.ActivityCoeff = 1
                    '                                    subst.PartialVolume = 0
                    '                                    subst.PartialPressure = P
                    '                                    subst.FracaoMassica = vz(i)
                    '                                    i += 1
                    '                                Next

                    '                            Else

                    'redirect:                       result = Me.FlashBase.Flash_PH(RET_VMOL(Fase.Mixture), P, H, 0.0#, Me)

                    '                                T = result(4)

                    '                                xl = result(0)
                    '                                xv = result(1)
                    '                                xl2 = result(7)

                    '                                Me.CurrentMaterialStream.Fases(3).SPMProperties.molarfraction = xl
                    '                                Me.CurrentMaterialStream.Fases(4).SPMProperties.molarfraction = xl2
                    '                                Me.CurrentMaterialStream.Fases(2).SPMProperties.molarfraction = xv

                    '                                Dim Vx = result(2)
                    '                                Dim Vy = result(3)
                    '                                Dim Vx2 = result(8)

                    '                                Dim FCL = Me.DW_CalcFugCoeff(Vx, T, P, State.Liquid)
                    '                                Dim FCL2 = Me.DW_CalcFugCoeff(Vx2, T, P, State.Liquid)
                    '                                Dim FCV = Me.DW_CalcFugCoeff(Vy, T, P, State.Vapor)

                    '                                i = 0
                    '                                For Each subst In Me.CurrentMaterialStream.Fases(3).Componentes.Values
                    '                                    subst.FracaoMolar = Vx(i)
                    '                                    subst.FugacityCoeff = FCL(i)
                    '                                    subst.ActivityCoeff = 0
                    '                                    subst.PartialVolume = 0
                    '                                    subst.PartialPressure = 0
                    '                                    i += 1
                    '                                Next
                    '                                i = 1
                    '                                For Each subst In Me.CurrentMaterialStream.Fases(3).Componentes.Values
                    '                                    subst.FracaoMassica = Me.AUX_CONVERT_MOL_TO_MASS(subst.Nome, 3)
                    '                                    i += 1
                    '                                Next
                    '                                i = 0
                    '                                For Each subst In Me.CurrentMaterialStream.Fases(4).Componentes.Values
                    '                                    subst.FracaoMolar = Vx2(i)
                    '                                    subst.FugacityCoeff = FCL2(i)
                    '                                    subst.ActivityCoeff = 0
                    '                                    subst.PartialVolume = 0
                    '                                    subst.PartialPressure = 0
                    '                                    i += 1
                    '                                Next
                    '                                i = 1
                    '                                For Each subst In Me.CurrentMaterialStream.Fases(4).Componentes.Values
                    '                                    subst.FracaoMassica = Me.AUX_CONVERT_MOL_TO_MASS(subst.Nome, 4)
                    '                                    i += 1
                    '                                Next
                    '                                i = 0
                    '                                For Each subst In Me.CurrentMaterialStream.Fases(2).Componentes.Values
                    '                                    subst.FracaoMolar = Vy(i)
                    '                                    subst.FugacityCoeff = FCV(i)
                    '                                    subst.ActivityCoeff = 0
                    '                                    subst.PartialVolume = 0
                    '                                    subst.PartialPressure = 0
                    '                                    i += 1
                    '                                Next

                    '                                i = 1
                    '                                For Each subst In Me.CurrentMaterialStream.Fases(2).Componentes.Values
                    '                                    subst.FracaoMassica = Me.AUX_CONVERT_MOL_TO_MASS(subst.Nome, 2)
                    '                                    i += 1
                    '                                Next

                    '                                Me.CurrentMaterialStream.Fases(3).SPMProperties.massfraction = xl * Me.AUX_MMM(Fase.Liquid1) / (xl * Me.AUX_MMM(Fase.Liquid1) + xl2 * Me.AUX_MMM(Fase.Liquid2) + xv * Me.AUX_MMM(Fase.Vapor))
                    '                                Me.CurrentMaterialStream.Fases(4).SPMProperties.massfraction = xl2 * Me.AUX_MMM(Fase.Liquid2) / (xl * Me.AUX_MMM(Fase.Liquid1) + xl2 * Me.AUX_MMM(Fase.Liquid2) + xv * Me.AUX_MMM(Fase.Vapor))
                    '                                Me.CurrentMaterialStream.Fases(2).SPMProperties.massfraction = xv * Me.AUX_MMM(Fase.Vapor) / (xl * Me.AUX_MMM(Fase.Liquid1) + xl2 * Me.AUX_MMM(Fase.Liquid2) + xv * Me.AUX_MMM(Fase.Vapor))

                    '                                Dim SM, SV, SL, SL2 As Double

                    '                                If xl <> 0 Then SL = Me.DW_CalcEntropy(Vx, T, P, State.Liquid)
                    '                                If xl2 <> 0 Then SL2 = Me.DW_CalcEntropy(Vx2, T, P, State.Liquid)
                    '                                If xv <> 0 Then SV = Me.DW_CalcEntropy(Vy, T, P, State.Vapor)
                    '                                SM = Me.CurrentMaterialStream.Fases(4).SPMProperties.massfraction.GetValueOrDefault * SL2 + Me.CurrentMaterialStream.Fases(3).SPMProperties.massfraction.GetValueOrDefault * SL + Me.CurrentMaterialStream.Fases(2).SPMProperties.massfraction.GetValueOrDefault * SV

                    '                                S = SM

                    '                            End If

                    '                        Case FlashSpec.S

                    '                            T = Me.CurrentMaterialStream.Fases(0).SPMProperties.temperature.GetValueOrDefault
                    '                            P = Me.CurrentMaterialStream.Fases(0).SPMProperties.pressure.GetValueOrDefault

                    '                            If Double.IsNaN(S) Or Double.IsInfinity(S) Then S = Me.CurrentMaterialStream.Fases(0).SPMProperties.molar_entropy.GetValueOrDefault / Me.CurrentMaterialStream.Fases(0).SPMProperties.molecularWeight.GetValueOrDefault

                    '                            If Me.AUX_IS_SINGLECOMP(Fase.Mixture) And Me.ComponentName <> "FPROPS" Then

                    '                                Dim hl, hv, sl, sv As Double
                    '                                Dim vz As Object = Me.RET_VMOL(Fase.Mixture)

                    '                                result = Me.FlashBase.Flash_PS(RET_VMOL(Fase.Mixture), P, S, 0.0#, Me)

                    '                                T = result(4)

                    '                                hl = Me.DW_CalcEnthalpy(vz, T, P, State.Liquid)
                    '                                hv = Me.DW_CalcEnthalpy(vz, T, P, State.Vapor)
                    '                                sl = Me.DW_CalcEntropy(vz, T, P, State.Liquid)
                    '                                sv = Me.DW_CalcEntropy(vz, T, P, State.Vapor)

                    '                                If Double.IsNaN(hl) Then hl = Double.NegativeInfinity
                    '                                If Double.IsNaN(sl) Then sl = Double.NegativeInfinity

                    '                                If S < sl Then
                    '                                    xv = 0
                    '                                    GoTo redirect2
                    '                                ElseIf S >= sv Then
                    '                                    xv = 1
                    '                                    GoTo redirect2
                    '                                Else
                    '                                    If sl = Double.NegativeInfinity Then
                    '                                        xv = 1.0#
                    '                                        H = hv
                    '                                    Else
                    '                                        xv = (S - sl) / (sv - sl)
                    '                                        H = xv * hv + (1 - xv) * hl
                    '                                    End If
                    '                                End If
                    '                                xl = 1 - xv

                    '                                'T = Me.CurrentMaterialStream.Fases(0).SPMProperties.temperature.GetValueOrDefault

                    '                                Me.CurrentMaterialStream.Fases(3).SPMProperties.molarfraction = xl
                    '                                Me.CurrentMaterialStream.Fases(2).SPMProperties.molarfraction = xv
                    '                                Me.CurrentMaterialStream.Fases(3).SPMProperties.massfraction = xl
                    '                                Me.CurrentMaterialStream.Fases(2).SPMProperties.massfraction = xv

                    '                                i = 0
                    '                                For Each subst In Me.CurrentMaterialStream.Fases(3).Componentes.Values
                    '                                    subst.FracaoMolar = vz(i)
                    '                                    subst.FugacityCoeff = 1
                    '                                    subst.ActivityCoeff = 1
                    '                                    subst.PartialVolume = 0
                    '                                    subst.PartialPressure = P
                    '                                    subst.FracaoMassica = vz(i)
                    '                                    i += 1
                    '                                Next
                    '                                i = 0
                    '                                For Each subst In Me.CurrentMaterialStream.Fases(2).Componentes.Values
                    '                                    subst.FracaoMolar = vz(i)
                    '                                    subst.FugacityCoeff = 1
                    '                                    subst.ActivityCoeff = 1
                    '                                    subst.PartialVolume = 0
                    '                                    subst.PartialPressure = P
                    '                                    subst.FracaoMassica = vz(i)
                    '                                    i += 1
                    '                                Next

                    '                            Else

                    'redirect2:                      result = Me.FlashBase.Flash_PS(RET_VMOL(Fase.Mixture), P, S, 0.0#, Me)

                    '                                T = result(4)

                    '                                xl = result(0)
                    '                                xv = result(1)
                    '                                xl2 = result(7)

                    '                                Me.CurrentMaterialStream.Fases(3).SPMProperties.molarfraction = xl
                    '                                Me.CurrentMaterialStream.Fases(4).SPMProperties.molarfraction = xl2
                    '                                Me.CurrentMaterialStream.Fases(2).SPMProperties.molarfraction = xv

                    '                                Dim Vx = result(2)
                    '                                Dim Vy = result(3)
                    '                                Dim Vx2 = result(8)

                    '                                Dim FCL = Me.DW_CalcFugCoeff(Vx, T, P, State.Liquid)
                    '                                Dim FCL2 = Me.DW_CalcFugCoeff(Vx2, T, P, State.Liquid)
                    '                                Dim FCV = Me.DW_CalcFugCoeff(Vy, T, P, State.Vapor)

                    '                                i = 0
                    '                                For Each subst In Me.CurrentMaterialStream.Fases(3).Componentes.Values
                    '                                    subst.FracaoMolar = Vx(i)
                    '                                    subst.FugacityCoeff = FCL(i)
                    '                                    subst.ActivityCoeff = 0
                    '                                    subst.PartialVolume = 0
                    '                                    subst.PartialPressure = 0
                    '                                    i += 1
                    '                                Next
                    '                                i = 1
                    '                                For Each subst In Me.CurrentMaterialStream.Fases(3).Componentes.Values
                    '                                    subst.FracaoMassica = Me.AUX_CONVERT_MOL_TO_MASS(subst.Nome, 3)
                    '                                    i += 1
                    '                                Next
                    '                                i = 0
                    '                                For Each subst In Me.CurrentMaterialStream.Fases(4).Componentes.Values
                    '                                    subst.FracaoMolar = Vx2(i)
                    '                                    subst.FugacityCoeff = FCL2(i)
                    '                                    subst.ActivityCoeff = 0
                    '                                    subst.PartialVolume = 0
                    '                                    subst.PartialPressure = 0
                    '                                    i += 1
                    '                                Next
                    '                                i = 1
                    '                                For Each subst In Me.CurrentMaterialStream.Fases(4).Componentes.Values
                    '                                    subst.FracaoMassica = Me.AUX_CONVERT_MOL_TO_MASS(subst.Nome, 4)
                    '                                    i += 1
                    '                                Next
                    '                                i = 0
                    '                                For Each subst In Me.CurrentMaterialStream.Fases(2).Componentes.Values
                    '                                    subst.FracaoMolar = Vy(i)
                    '                                    subst.FugacityCoeff = FCV(i)
                    '                                    subst.ActivityCoeff = 0
                    '                                    subst.PartialVolume = 0
                    '                                    subst.PartialPressure = 0
                    '                                    i += 1
                    '                                Next

                    '                                i = 1
                    '                                For Each subst In Me.CurrentMaterialStream.Fases(2).Componentes.Values
                    '                                    subst.FracaoMassica = Me.AUX_CONVERT_MOL_TO_MASS(subst.Nome, 2)
                    '                                    i += 1
                    '                                Next

                    '                                Me.CurrentMaterialStream.Fases(3).SPMProperties.massfraction = xl * Me.AUX_MMM(Fase.Liquid1) / (xl * Me.AUX_MMM(Fase.Liquid1) + xl2 * Me.AUX_MMM(Fase.Liquid2) + xv * Me.AUX_MMM(Fase.Vapor))
                    '                                Me.CurrentMaterialStream.Fases(4).SPMProperties.massfraction = xl2 * Me.AUX_MMM(Fase.Liquid2) / (xl * Me.AUX_MMM(Fase.Liquid1) + xl2 * Me.AUX_MMM(Fase.Liquid2) + xv * Me.AUX_MMM(Fase.Vapor))
                    '                                Me.CurrentMaterialStream.Fases(2).SPMProperties.massfraction = xv * Me.AUX_MMM(Fase.Vapor) / (xl * Me.AUX_MMM(Fase.Liquid1) + xl2 * Me.AUX_MMM(Fase.Liquid2) + xv * Me.AUX_MMM(Fase.Vapor))

                    '                                Dim HM, HV, HL, HL2 As Double

                    '                                If xl <> 0 Then HL = Me.DW_CalcEnthalpy(Vx, T, P, State.Liquid)
                    '                                If xl2 <> 0 Then HL2 = Me.DW_CalcEnthalpy(Vx2, T, P, State.Liquid)
                    '                                If xv <> 0 Then HV = Me.DW_CalcEnthalpy(Vy, T, P, State.Vapor)
                    '                                HM = Me.CurrentMaterialStream.Fases(4).SPMProperties.massfraction.GetValueOrDefault * HL2 + Me.CurrentMaterialStream.Fases(3).SPMProperties.massfraction.GetValueOrDefault * HL + Me.CurrentMaterialStream.Fases(2).SPMProperties.massfraction.GetValueOrDefault * HV

                    '                                H = HM

                    '                            End If

                    '                        Case FlashSpec.VAP

                    '                            Dim KI(n) As Double

                    '                            i = 0
                    '                            Do
                    '                                KI(i) = 0
                    '                                i = i + 1
                    '                            Loop Until i = n + 1

                    '                            T = Me.CurrentMaterialStream.Fases(0).SPMProperties.temperature.GetValueOrDefault
                    '                            P = Me.CurrentMaterialStream.Fases(0).SPMProperties.pressure.GetValueOrDefault

                    '                            result = Me.FlashBase.Flash_PV(RET_VMOL(Fase.Mixture), P, xv, 0, Me)

                    '                            T = result(4)

                    '                            xl = result(0)
                    '                            xv = result(1)
                    '                            xl2 = result(7)

                    '                            Me.CurrentMaterialStream.Fases(3).SPMProperties.molarfraction = xl
                    '                            Me.CurrentMaterialStream.Fases(4).SPMProperties.molarfraction = xl2
                    '                            Me.CurrentMaterialStream.Fases(2).SPMProperties.molarfraction = xv

                    '                            Dim Vx = result(2)
                    '                            Dim Vy = result(3)
                    '                            Dim Vx2 = result(8)

                    '                            Dim FCL = Me.DW_CalcFugCoeff(Vx, T, P, State.Liquid)
                    '                            Dim FCL2 = Me.DW_CalcFugCoeff(Vx2, T, P, State.Liquid)
                    '                            Dim FCV = Me.DW_CalcFugCoeff(Vy, T, P, State.Vapor)

                    '                            i = 0
                    '                            For Each subst In Me.CurrentMaterialStream.Fases(3).Componentes.Values
                    '                                subst.FracaoMolar = Vx(i)
                    '                                subst.FugacityCoeff = FCL(i)
                    '                                subst.ActivityCoeff = 0
                    '                                subst.PartialVolume = 0
                    '                                subst.PartialPressure = 0
                    '                                i += 1
                    '                            Next
                    '                            For Each subst In Me.CurrentMaterialStream.Fases(3).Componentes.Values
                    '                                subst.FracaoMassica = Me.AUX_CONVERT_MOL_TO_MASS(subst.Nome, 3)
                    '                            Next
                    '                            i = 0
                    '                            For Each subst In Me.CurrentMaterialStream.Fases(4).Componentes.Values
                    '                                subst.FracaoMolar = Vx2(i)
                    '                                subst.FugacityCoeff = FCL2(i)
                    '                                subst.ActivityCoeff = 0
                    '                                subst.PartialVolume = 0
                    '                                subst.PartialPressure = 0
                    '                                i += 1
                    '                            Next
                    '                            For Each subst In Me.CurrentMaterialStream.Fases(4).Componentes.Values
                    '                                subst.FracaoMassica = Me.AUX_CONVERT_MOL_TO_MASS(subst.Nome, 4)
                    '                            Next
                    '                            i = 0
                    '                            For Each subst In Me.CurrentMaterialStream.Fases(2).Componentes.Values
                    '                                subst.FracaoMolar = Vy(i)
                    '                                subst.FugacityCoeff = FCV(i)
                    '                                subst.ActivityCoeff = 0
                    '                                subst.PartialVolume = 0
                    '                                subst.PartialPressure = 0
                    '                                i += 1
                    '                            Next
                    '                            For Each subst In Me.CurrentMaterialStream.Fases(2).Componentes.Values
                    '                                subst.FracaoMassica = Me.AUX_CONVERT_MOL_TO_MASS(subst.Nome, 2)
                    '                            Next

                    '                            Me.CurrentMaterialStream.Fases(3).SPMProperties.massfraction = xl * Me.AUX_MMM(Fase.Liquid1) / (xl * Me.AUX_MMM(Fase.Liquid1) + xl2 * Me.AUX_MMM(Fase.Liquid2) + xv * Me.AUX_MMM(Fase.Vapor))
                    '                            Me.CurrentMaterialStream.Fases(4).SPMProperties.massfraction = xl2 * Me.AUX_MMM(Fase.Liquid2) / (xl * Me.AUX_MMM(Fase.Liquid1) + xl2 * Me.AUX_MMM(Fase.Liquid2) + xv * Me.AUX_MMM(Fase.Vapor))
                    '                            Me.CurrentMaterialStream.Fases(2).SPMProperties.massfraction = xv * Me.AUX_MMM(Fase.Vapor) / (xl * Me.AUX_MMM(Fase.Liquid1) + xl2 * Me.AUX_MMM(Fase.Liquid2) + xv * Me.AUX_MMM(Fase.Vapor))

                    '                            Dim HM, HV, HL, HL2 As Double

                    '                            If xl <> 0 Then HL = Me.DW_CalcEnthalpy(Vx, T, P, State.Liquid)
                    '                            If xl2 <> 0 Then HL2 = Me.DW_CalcEnthalpy(Vx2, T, P, State.Liquid)
                    '                            If xv <> 0 Then HV = Me.DW_CalcEnthalpy(Vy, T, P, State.Vapor)
                    '                            HM = Me.CurrentMaterialStream.Fases(4).SPMProperties.massfraction.GetValueOrDefault * HL2 + Me.CurrentMaterialStream.Fases(3).SPMProperties.massfraction.GetValueOrDefault * HL + Me.CurrentMaterialStream.Fases(2).SPMProperties.massfraction.GetValueOrDefault * HV

                    '                            H = HM

                    '                            Dim SM, SV, SL, SL2 As Double

                    '                            If xl <> 0 Then SL = Me.DW_CalcEntropy(Vx, T, P, State.Liquid)
                    '                            If xl2 <> 0 Then SL2 = Me.DW_CalcEntropy(Vx2, T, P, State.Liquid)
                    '                            If xv <> 0 Then SV = Me.DW_CalcEntropy(Vy, T, P, State.Vapor)
                    '                            SM = Me.CurrentMaterialStream.Fases(4).SPMProperties.massfraction.GetValueOrDefault * SL2 + Me.CurrentMaterialStream.Fases(3).SPMProperties.massfraction.GetValueOrDefault * SL + Me.CurrentMaterialStream.Fases(2).SPMProperties.massfraction.GetValueOrDefault * SV

                    '                            S = SM

                    '                    End Select

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

    End Class

End Namespace
