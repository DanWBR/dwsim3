'    CoolProp Property Package
'    Copyright 2014 Daniel Wagner O. de Medeiros
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
Imports DWSIM.DWSIM.MathEx
Imports DWSIM.DWSIM.ClassesBasicasTermodinamica
Imports System.Runtime.InteropServices
Imports CoolPropInterface
Imports System.Linq

Namespace DWSIM.SimulationObjects.PropertyPackages


    <System.Runtime.InteropServices.Guid(CoolPropPropertyPackage.ClassId)> _
    <System.Serializable()> Public Class CoolPropPropertyPackage

        Inherits DWSIM.SimulationObjects.PropertyPackages.PropertyPackage

        Public Shadows Const ClassId As String = "1F5B0263-E936-40d5-BA5B-FFAB11595E43"

        Public Sub New(ByVal comode As Boolean)
            MyBase.New(comode)
        End Sub

        Public Sub New()

            MyBase.New()

            With Me.Parameters
                .Item("PP_IDEAL_MIXRULE_LIQDENS") = 1
                .Item("PP_USEEXPLIQDENS") = 1
            End With

            Me.IsConfigurable = True
            Me.ConfigForm = New FormConfigPP
            Me._packagetype = PropertyPackages.PackageType.Miscelaneous

        End Sub

        Public Overrides Sub ReconfigureConfigForm()
            MyBase.ReconfigureConfigForm()
            Me.ConfigForm = New FormConfigPP
        End Sub

#Region "    DWSIM Functions"

        Public Overrides Function AUX_CPi(sub1 As String, T As Double) As Object
            CheckIfCompoundIsSupported(sub1)
            Return CoolProp.Props("C0", "T", T, "Q", 1, sub1)
        End Function

        Public Overrides Function AUX_PVAPi(index As Integer, T As Double) As Object
            CheckIfCompoundIsSupported(RET_VNAMES()(index))
            Return CoolProp.Props("P", "T", T, "Q", 0, RET_VNAMES()(index)) * 1000
        End Function

        Public Overrides Function AUX_PVAPi(sub1 As String, T As Double) As Object
            CheckIfCompoundIsSupported(sub1)
            Return CoolProp.Props("P", "T", T, "Q", 0, sub1) * 1000
        End Function

        Public Overrides Function AUX_LIQDENSi(cprop As ClassesBasicasTermodinamica.ConstantProperties, T As Double) As Double
            CheckIfCompoundIsSupported(cprop.Name)
            Return CoolProp.Props("D", "T", T, "Q", 0, cprop.Name)
        End Function

        Public Overrides Function AUX_LIQ_Cpi(cprop As ClassesBasicasTermodinamica.ConstantProperties, T As Double) As Double
            CheckIfCompoundIsSupported(cprop.Name)
            Return CoolProp.Props("C", "T", T, "Q", 0, cprop.Name)
        End Function

        Public Overrides Function AUX_CONDTG(T As Double, P As Double) As Double
            Dim val As Double
            Dim i As Integer
            Dim vk(Me.CurrentMaterialStream.Fases(0).Componentes.Count - 1) As Double
            i = 0
            For Each subst As Substancia In Me.CurrentMaterialStream.Fases(2).Componentes.Values
                CheckIfCompoundIsSupported(subst.ConstantProperties.Name)
                If CoolProp.Props("P", "T", T, "Q", 1, subst.ConstantProperties.Name) > P Then
                    vk(i) = CoolProp.Props("L", "T", T, "P", P / 1000, subst.ConstantProperties.Name) * 1000
                Else
                    vk(i) = CoolProp.Props("L", "T", T, "Q", 1, subst.ConstantProperties.Name) * 1000
                End If
                vk(i) = subst.FracaoMassica * vk(i)
                i = i + 1
            Next
            val = MathEx.Common.Sum(vk)

            Return val
        End Function

        Public Overrides Function AUX_CONDTL(T As Double, Optional phaseid As Integer = 3) As Double

            Dim val As Double
            Dim i As Integer
            Dim vk(Me.CurrentMaterialStream.Fases(0).Componentes.Count - 1) As Double
            i = 0
            For Each subst As Substancia In Me.CurrentMaterialStream.Fases(phaseid).Componentes.Values
                CheckIfCompoundIsSupported(subst.ConstantProperties.Name)
                vk(i) = CoolProp.Props("L", "T", T, "Q", 0, subst.ConstantProperties.Name) * 1000
                vk(i) = subst.FracaoMassica * vk(i)
                i = i + 1
            Next
            val = MathEx.Common.Sum(vk)
            Return val

        End Function

        Public Overrides Function AUX_LIQDENS(T As Double, Vx As System.Array, Optional P As Double = 0.0, Optional Pvp As Double = 0.0, Optional FORCE_EOS As Boolean = False) As Double
            Dim val As Double
            Dim i As Integer
            Dim vk(Me.CurrentMaterialStream.Fases(0).Componentes.Count - 1) As Double
            Dim vn As String() = Me.RET_VNAMES
            Dim n As Integer = Vx.Length - 1
            For i = 0 To n
                CheckIfCompoundIsSupported(vn(i))
                vk(i) = CoolProp.Props("D", "T", T, "P", P / 1000, vn(i))
                If vn(i) <> 0.0# Then vk(i) = vn(i) / vk(i)
            Next
            val = 1 / MathEx.Common.Sum(vk)
            Return val
        End Function

        Public Overrides Function AUX_LIQDENSi(subst As ClassesBasicasTermodinamica.Substancia, T As Double) As Double
            CheckIfCompoundIsSupported(subst.ConstantProperties.Name)
            Return CoolProp.Props("L", "T", T, "Q", 0, subst.ConstantProperties.Name)
        End Function

        Public Overrides Function AUX_LIQTHERMCONDi(cprop As ClassesBasicasTermodinamica.ConstantProperties, T As Double) As Double
            CheckIfCompoundIsSupported(cprop.Name)
            Return CoolProp.Props("L", "T", T, "Q", 0, cprop.Name) * 1000
        End Function

        Public Overrides Function AUX_LIQVISCi(sub1 As String, T As Double) As Object
            CheckIfCompoundIsSupported(sub1)
            Return CoolProp.Props("V", "T", T, "Q", 0, sub1)
        End Function

        Public Overrides Function AUX_SURFTi(constprop As ClassesBasicasTermodinamica.ConstantProperties, T As Double) As Double
            CheckIfCompoundIsSupported(constprop.Name)
            Return CoolProp.Props("I", "T", T, "Q", 0, constprop.Name)
        End Function

        Public Overrides Function AUX_SURFTM(T As Double) As Double

            Dim subst As DWSIM.ClassesBasicasTermodinamica.Substancia
            Dim val As Double = 0
            For Each subst In Me.CurrentMaterialStream.Fases(1).Componentes.Values
                val += subst.FracaoMolar.GetValueOrDefault * Me.AUX_SURFTi(subst.ConstantProperties, T)
            Next
            Return val

        End Function

        Public Overrides Function AUX_VAPTHERMCONDi(cprop As ClassesBasicasTermodinamica.ConstantProperties, T As Double, P As Double) As Double
            CheckIfCompoundIsSupported(cprop.Name)
            Dim val As Double
            If CoolProp.Props("P", "T", T, "Q", 1, cprop.Name) * 1000 > P Then
                val = CoolProp.Props("L", "T", T, "P", P / 1000, cprop.Name) * 1000
            Else
                val = CoolProp.Props("L", "T", T, "Q", 1, cprop.Name) * 1000
            End If
            Return val
        End Function

        Public Overrides Function AUX_VAPVISCi(cprop As ClassesBasicasTermodinamica.ConstantProperties, T As Double) As Double
            CheckIfCompoundIsSupported(cprop.Name)
            Return CoolProp.Props("V", "T", T, "Q", 1, cprop.Name)
        End Function

        Public Function AUX_VAPVISCMIX(T As Double, P As Double, MM As Double) As Double
            Dim val As Double = 0.0#
            For Each subst As Substancia In Me.CurrentMaterialStream.Fases(2).Componentes.Values
                CheckIfCompoundIsSupported(subst.ConstantProperties.Name)
                If CoolProp.Props("P", "T", T, "Q", 1, subst.ConstantProperties.Name) * 1000 > P Then
                    val = subst.FracaoMolar.GetValueOrDefault * CoolProp.Props("V", "T", T, "P", P / 1000, subst.ConstantProperties.Name)
                Else
                    val = subst.FracaoMolar.GetValueOrDefault * CoolProp.Props("V", "T", T, "Q", 1, subst.ConstantProperties.Name)
                End If
            Next
            Return val
        End Function

        Public Overrides Function AUX_LIQDENS(ByVal T As Double, Optional ByVal P As Double = 0.0, Optional ByVal Pvp As Double = 0.0, Optional ByVal phaseid As Integer = 3, Optional ByVal FORCE_EOS As Boolean = False) As Double

            Dim val As Double
            Dim i As Integer
            Dim vk(Me.CurrentMaterialStream.Fases(0).Componentes.Count - 1) As Double
            i = 0
            For Each subst As Substancia In Me.CurrentMaterialStream.Fases(phaseid).Componentes.Values
                CheckIfCompoundIsSupported(subst.ConstantProperties.Name)
                If CoolProp.Props("P", "T", T, "Q", 0, subst.ConstantProperties.Name) * 1000 < P Then
                    vk(i) = CoolProp.Props("D", "T", T, "P", P / 1000, subst.ConstantProperties.Name)
                Else
                    vk(i) = CoolProp.Props("D", "T", T, "Q", 0, subst.ConstantProperties.Name)
                End If
                vk(i) = subst.FracaoMassica / vk(i)
                i = i + 1
            Next
            val = 1 / MathEx.Common.Sum(vk)

            Return val

        End Function

        Public Overrides Function AUX_VAPDENS(ByVal T As Double, ByVal P As Double) As Double

            Dim val As Double
            Dim i As Integer
            Dim vk(Me.CurrentMaterialStream.Fases(0).Componentes.Count - 1) As Double
            i = 0
            For Each subst As Substancia In Me.CurrentMaterialStream.Fases(2).Componentes.Values
                CheckIfCompoundIsSupported(subst.ConstantProperties.Name)
                If CoolProp.Props("P", "T", T, "Q", 1, subst.ConstantProperties.Name) * 1000 > P Then
                    vk(i) = CoolProp.Props("D", "T", T, "P", P / 1000, subst.ConstantProperties.Name)
                Else
                    vk(i) = CoolProp.Props("D", "T", T, "Q", 1, subst.ConstantProperties.Name)
                End If
                vk(i) = subst.FracaoMassica / vk(i)
                i = i + 1
            Next
            val = 1 / MathEx.Common.Sum(vk)

            Return val

        End Function

        Public Overrides Sub DW_CalcCompPartialVolume(ByVal phase As Fase, ByVal T As Double, ByVal P As Double)

            For Each subst As ClassesBasicasTermodinamica.Substancia In Me.CurrentMaterialStream.Fases(0).Componentes.Values
                subst.PartialVolume = 0.0#
            Next

        End Sub

        Public Overrides Function DW_CalcCp_ISOL(ByVal fase1 As Fase, ByVal T As Double, ByVal P As Double) As Double

            Dim phaseID As Integer
            Select Case fase1
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

            Dim val As Double
            Dim i As Integer
            Dim vk(Me.CurrentMaterialStream.Fases(0).Componentes.Count - 1) As Double
            Select Case fase1
                Case Fase.Aqueous, Fase.Liquid, Fase.Liquid1, Fase.Liquid2, Fase.Liquid3
                    i = 0
                    For Each subst As Substancia In Me.CurrentMaterialStream.Fases(phaseID).Componentes.Values
                        CheckIfCompoundIsSupported(subst.ConstantProperties.Name)
                        Dim psat As Double = CoolProp.Props("P", "T", T, "Q", 0, subst.ConstantProperties.Name) * 1000
                        If psat < P Then
                            vk(i) = CoolProp.Props("C", "T", T, "P", P / 1000, subst.ConstantProperties.Name)
                        Else
                            vk(i) = CoolProp.Props("C", "T", T, "Q", 0, subst.ConstantProperties.Name)
                        End If
                        vk(i) = subst.FracaoMassica * vk(i)
                        i = i + 1
                    Next
                Case Fase.Vapor
                    i = 0
                    For Each subst As Substancia In Me.CurrentMaterialStream.Fases(phaseID).Componentes.Values
                        CheckIfCompoundIsSupported(subst.ConstantProperties.Name)
                        Dim psat As Double = CoolProp.Props("P", "T", T, "Q", 1, subst.ConstantProperties.Name) * 1000
                        If psat > P Then
                            vk(i) = CoolProp.Props("C", "T", T, "P", P / 1000, subst.ConstantProperties.Name)
                        Else
                            vk(i) = CoolProp.Props("C", "T", T, "Q", 1, subst.ConstantProperties.Name)
                        End If
                        vk(i) = subst.FracaoMassica * vk(i)
                        i = i + 1
                    Next
            End Select

            val = MathEx.Common.Sum(vk)

            Return val

        End Function

        Public Overrides Function DW_CalcCv_ISOL(ByVal fase1 As Fase, ByVal T As Double, ByVal P As Double) As Double

            Dim phaseID As Integer
            Select Case fase1
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

            Dim val As Double
            Dim i As Integer
            Dim vk(Me.CurrentMaterialStream.Fases(0).Componentes.Count - 1) As Double
            Select Case fase1
                Case Fase.Aqueous, Fase.Liquid, Fase.Liquid1, Fase.Liquid2, Fase.Liquid3
                    i = 0
                    For Each subst As Substancia In Me.CurrentMaterialStream.Fases(phaseID).Componentes.Values
                        CheckIfCompoundIsSupported(subst.ConstantProperties.Name)
                        Dim psat As Double = CoolProp.Props("P", "T", T, "Q", 0, subst.ConstantProperties.Name) * 1000
                        If psat < P Then
                            vk(i) = CoolProp.Props("O", "T", T, "P", P / 1000, subst.ConstantProperties.Name)
                        Else
                            vk(i) = CoolProp.Props("O", "T", T, "Q", 0, subst.ConstantProperties.Name)
                        End If
                        vk(i) = subst.FracaoMassica * vk(i)
                        i = i + 1
                    Next
                Case Fase.Vapor
                    i = 0
                    For Each subst As Substancia In Me.CurrentMaterialStream.Fases(phaseID).Componentes.Values
                        CheckIfCompoundIsSupported(subst.ConstantProperties.Name)
                        Dim psat As Double = CoolProp.Props("P", "T", T, "Q", 1, subst.ConstantProperties.Name) * 1000
                        If psat > P Then
                            vk(i) = CoolProp.Props("O", "T", T, "P", P / 1000, subst.ConstantProperties.Name)
                        Else
                            vk(i) = CoolProp.Props("O", "T", T, "Q", 1, subst.ConstantProperties.Name)
                        End If
                        vk(i) = subst.FracaoMassica * vk(i)
                        i = i + 1
                    Next
            End Select

            val = MathEx.Common.Sum(vk)

            Return val

        End Function

        Public Overrides Function DW_CalcEnthalpy(ByVal Vx As System.Array, ByVal T As Double, ByVal P As Double, ByVal st As State) As Double

            Dim val As Double
            Dim i As Integer
            Dim vk(Me.CurrentMaterialStream.Fases(0).Componentes.Count - 1) As Double
            Dim vn As String() = Me.RET_VNAMES
            Dim n As Integer = Vx.Length - 1
            Select Case st
                Case State.Liquid
                    For i = 0 To n
                        CheckIfCompoundIsSupported(vn(i))
                        If CoolProp.Props("P", "T", T, "Q", 0, vn(i)) < P Then
                            vk(i) = CoolProp.Props("H", "T", T, "P", P / 1000, vn(i))
                        Else
                            vk(i) = CoolProp.Props("H", "T", T, "Q", 0, vn(i))
                        End If
                        vk(i) = Vx(i) * vk(i)
                    Next
                Case State.Vapor
                    For i = 0 To n
                        CheckIfCompoundIsSupported(vn(i))
                        If CoolProp.Props("P", "T", T, "Q", 1, vn(i)) > P Then
                            vk(i) = CoolProp.Props("H", "T", T, "P", P / 1000, vn(i))
                        Else
                            vk(i) = CoolProp.Props("H", "T", T, "Q", 1, vn(i))
                        End If
                        vk(i) = Vx(i) * vk(i)
                    Next
            End Select

            val = MathEx.Common.Sum(vk)

            Return val

        End Function

        Public Overrides Function DW_CalcEnthalpyDeparture(ByVal Vx As System.Array, ByVal T As Double, ByVal P As Double, ByVal st As State) As Double

            Return DW_CalcEnthalpy(Vx, T, P, st)

        End Function

        Public Overrides Function DW_CalcEntropy(ByVal Vx As System.Array, ByVal T As Double, ByVal P As Double, ByVal st As State) As Double

            Dim val As Double
            Dim i As Integer
            Dim vk(Me.CurrentMaterialStream.Fases(0).Componentes.Count - 1) As Double
            Dim vn As String() = Me.RET_VNAMES
            Dim n As Integer = Vx.Length - 1
            Select Case st
                Case State.Liquid
                    For i = 0 To n
                        CheckIfCompoundIsSupported(vn(i))
                        If CoolProp.Props("P", "T", T, "Q", 0, vn(i)) < P Then
                            vk(i) = CoolProp.Props("S", "T", T, "P", P / 1000, vn(i))
                        Else
                            vk(i) = CoolProp.Props("S", "T", T, "Q", 0, vn(i))
                        End If
                        vk(i) = Vx(i) * vk(i)
                    Next
                Case State.Vapor
                    For i = 0 To n
                        CheckIfCompoundIsSupported(vn(i))
                        If CoolProp.Props("P", "T", T, "Q", 1, vn(i)) > P Then
                            vk(i) = CoolProp.Props("S", "T", T, "P", P / 1000, vn(i))
                        Else
                            vk(i) = CoolProp.Props("S", "T", T, "Q", 1, vn(i))
                        End If
                        vk(i) = Vx(i) * vk(i)
                    Next
            End Select

            val = MathEx.Common.Sum(vk)

            Return val

        End Function

        Public Overrides Function DW_CalcEntropyDeparture(ByVal Vx As System.Array, ByVal T As Double, ByVal P As Double, ByVal st As State) As Double

            Return DW_CalcEntropy(Vx, T, P, st)

        End Function

        Public Overrides Function DW_CalcFugCoeff(ByVal Vx As System.Array, ByVal T As Double, ByVal P As Double, ByVal st As State) As Object

            Dim n As Integer = UBound(Vx)
            Dim lnfug(n) As Double
            Dim fugcoeff(n) As Double
            Dim i As Integer

            Dim Tc As Object = Me.RET_VTC()

            If st = State.Liquid Then
                For i = 0 To n
                    lnfug(i) = Math.Log(Me.AUX_PVAPi(i, T) / P)
                Next
            Else
                For i = 0 To n
                    lnfug(i) = 0.0#
                Next
            End If

            For i = 0 To n
                fugcoeff(i) = Exp(lnfug(i))
            Next

            Return fugcoeff

        End Function

        Public Overrides Function DW_CalcK_ISOL(ByVal fase1 As Fase, ByVal T As Double, ByVal P As Double) As Double

            Return 0.0#

        End Function

        Public Overrides Function DW_CalcMassaEspecifica_ISOL(ByVal fase1 As Fase, ByVal T As Double, ByVal P As Double, Optional ByVal Pvp As Double = 0.0) As Double

            Return 0.0#

        End Function

        Public Overrides Function DW_CalcMM_ISOL(ByVal fase1 As Fase, ByVal T As Double, ByVal P As Double) As Double

            Return Me.AUX_MMM(fase1)

        End Function

        Public Overrides Sub DW_CalcPhaseProps(ByVal fase As Fase)

            Dim result As Double
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


                Me.CurrentMaterialStream.Fases(phaseID).SPMProperties.density = AUX_LIQDENS(T, P, 0.0#, phaseID)

                result = Me.DW_CalcEnthalpy(RET_VMAS(dwpl), T, P, State.Liquid)
                Me.CurrentMaterialStream.Fases(phaseID).SPMProperties.enthalpy = result

                result = Me.DW_CalcEntropy(RET_VMAS(dwpl), T, P, State.Liquid)
                Me.CurrentMaterialStream.Fases(phaseID).SPMProperties.entropy = result

                result = P / (Me.AUX_LIQDENS(T, P, 0, phaseID) * 8.314 * T) / 1000 * AUX_MMM(fase)
                Me.CurrentMaterialStream.Fases(phaseID).SPMProperties.compressibilityFactor = result

                result = Me.DW_CalcCp_ISOL(fase, T, P)
                Me.CurrentMaterialStream.Fases(phaseID).SPMProperties.heatCapacityCp = result

                result = Me.DW_CalcCv_ISOL(fase, T, P)
                Me.CurrentMaterialStream.Fases(phaseID).SPMProperties.heatCapacityCv = result

                result = Me.AUX_MMM(fase)
                Me.CurrentMaterialStream.Fases(phaseID).SPMProperties.molecularWeight = result

                result = Me.DW_CalcEnthalpy(RET_VMAS(dwpl), T, P, State.Liquid) * Me.CurrentMaterialStream.Fases(phaseID).SPMProperties.molecularWeight.GetValueOrDefault
                Me.CurrentMaterialStream.Fases(phaseID).SPMProperties.molar_enthalpy = result

                result = Me.DW_CalcEntropy(RET_VMAS(dwpl), T, P, State.Liquid) * Me.CurrentMaterialStream.Fases(phaseID).SPMProperties.molecularWeight.GetValueOrDefault
                Me.CurrentMaterialStream.Fases(phaseID).SPMProperties.molar_entropy = result

                result = Me.AUX_CONDTL(T)
                Me.CurrentMaterialStream.Fases(phaseID).SPMProperties.thermalConductivity = result

                result = Me.AUX_LIQVISCm(T)
                Me.CurrentMaterialStream.Fases(phaseID).SPMProperties.viscosity = result

                Me.CurrentMaterialStream.Fases(phaseID).SPMProperties.kinematic_viscosity = result / Me.CurrentMaterialStream.Fases(phaseID).SPMProperties.density.Value

            ElseIf phaseID = 2 Then

                result = Me.AUX_VAPDENS(T, P)
                Me.CurrentMaterialStream.Fases(phaseID).SPMProperties.density = result

                result = Me.DW_CalcEnthalpy(RET_VMAS(dwpl), T, P, State.Vapor)
                Me.CurrentMaterialStream.Fases(phaseID).SPMProperties.enthalpy = result

                result = Me.DW_CalcEntropy(RET_VMAS(dwpl), T, P, State.Vapor)
                Me.CurrentMaterialStream.Fases(phaseID).SPMProperties.entropy = result

                result = P / (Me.AUX_VAPDENS(T, P) * 8.314 * T) / 1000 * AUX_MMM(fase)
                Me.CurrentMaterialStream.Fases(phaseID).SPMProperties.compressibilityFactor = result

                result = Me.DW_CalcCp_ISOL(fase, T, P)
                Me.CurrentMaterialStream.Fases(phaseID).SPMProperties.heatCapacityCp = result

                result = Me.DW_CalcCv_ISOL(fase, T, P)
                Me.CurrentMaterialStream.Fases(phaseID).SPMProperties.heatCapacityCv = result

                result = Me.AUX_MMM(fase)
                Me.CurrentMaterialStream.Fases(phaseID).SPMProperties.molecularWeight = result

                result = Me.DW_CalcEnthalpy(RET_VMAS(dwpl), T, P, State.Vapor) * Me.CurrentMaterialStream.Fases(phaseID).SPMProperties.molecularWeight.GetValueOrDefault
                Me.CurrentMaterialStream.Fases(phaseID).SPMProperties.molar_enthalpy = result

                result = Me.DW_CalcEntropy(RET_VMAS(dwpl), T, P, State.Vapor) * Me.CurrentMaterialStream.Fases(phaseID).SPMProperties.molecularWeight.GetValueOrDefault
                Me.CurrentMaterialStream.Fases(phaseID).SPMProperties.molar_entropy = result

                result = Me.AUX_CONDTG(T, P)
                Me.CurrentMaterialStream.Fases(phaseID).SPMProperties.thermalConductivity = result

                result = Me.AUX_VAPVISCMIX(T, P, Me.AUX_MMM(fase))
                Me.CurrentMaterialStream.Fases(phaseID).SPMProperties.viscosity = result
                Me.CurrentMaterialStream.Fases(phaseID).SPMProperties.kinematic_viscosity = result / Me.CurrentMaterialStream.Fases(phaseID).SPMProperties.density.GetValueOrDefault

            ElseIf phaseID = 1 Then

                DW_CalcLiqMixtureProps()


            Else

                DW_CalcOverallProps()

            End If


            If phaseID > 0 Then
                result = overallmolarflow * phasemolarfrac * Me.AUX_MMM(fase) / 1000 / Me.CurrentMaterialStream.Fases(phaseID).SPMProperties.density.GetValueOrDefault
                Me.CurrentMaterialStream.Fases(phaseID).SPMProperties.volumetric_flow = result
            End If


        End Sub

        Public Overrides Sub DW_CalcProp(ByVal [property] As String, ByVal phase As Fase)

            Dim result As Double = 0.0#
            Dim resultObj As Object = Nothing
            Dim phaseID As Integer = -1
            Dim state As String = ""
            Dim fstate As State = PropertyPackages.State.Solid

            Dim T, P As Double
            T = Me.CurrentMaterialStream.Fases(0).SPMProperties.temperature
            P = Me.CurrentMaterialStream.Fases(0).SPMProperties.pressure

            Select Case phase
                Case Fase.Vapor
                    state = "V"
                    fstate = PropertyPackages.State.Vapor
                Case Fase.Liquid, Fase.Liquid1, Fase.Liquid2, Fase.Liquid3
                    state = "L"
                    fstate = PropertyPackages.State.Liquid
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
                    Me.CurrentMaterialStream.Fases(phaseID).SPMProperties.compressibilityFactor = result
                Case "heatcapacity", "heatcapacitycp"
                    Me.CurrentMaterialStream.Fases(phaseID).SPMProperties.heatCapacityCp = result
                Case "heatcapacitycv"
                    Me.CurrentMaterialStream.Fases(phaseID).SPMProperties.heatCapacityCv = result
                Case "enthalpy", "enthalpynf"
                    result = Me.DW_CalcEnthalpy(RET_VMOL(phase), T, P, phase)
                    Me.CurrentMaterialStream.Fases(phaseID).SPMProperties.enthalpy = result
                    result = result * Me.CurrentMaterialStream.Fases(phaseID).SPMProperties.molecularWeight.GetValueOrDefault
                    Me.CurrentMaterialStream.Fases(phaseID).SPMProperties.molar_enthalpy = result
                Case "entropy", "entropynf"
                    result = Me.DW_CalcEntropy(RET_VMOL(phase), T, P, phase)
                    Me.CurrentMaterialStream.Fases(phaseID).SPMProperties.entropy = result
                    result = result * Me.CurrentMaterialStream.Fases(phaseID).SPMProperties.molecularWeight.GetValueOrDefault
                    Me.CurrentMaterialStream.Fases(phaseID).SPMProperties.molar_entropy = result
                Case "excessenthalpy"
                    result = Me.DW_CalcEnthalpyDeparture(RET_VMOL(phase), T, P, phase)
                    Me.CurrentMaterialStream.Fases(phaseID).SPMProperties.excessEnthalpy = result
                Case "excessentropy"
                    result = Me.DW_CalcEntropyDeparture(RET_VMOL(phase), T, P, phase)
                    Me.CurrentMaterialStream.Fases(phaseID).SPMProperties.excessEntropy = result
                Case "enthalpyf"
                    Dim entF As Double = 0.0#
                    result = Me.DW_CalcEnthalpy(RET_VMOL(phase), T, P, phase)
                    Me.CurrentMaterialStream.Fases(phaseID).SPMProperties.enthalpyF = result + entF
                    result = result * Me.CurrentMaterialStream.Fases(phaseID).SPMProperties.molecularWeight.GetValueOrDefault
                    Me.CurrentMaterialStream.Fases(phaseID).SPMProperties.molar_enthalpyF = result
                Case "entropyf"
                    Dim entF As Double = 0.0#
                    result = Me.DW_CalcEntropy(RET_VMOL(phase), T, P, phase)
                    Me.CurrentMaterialStream.Fases(phaseID).SPMProperties.entropyF = result + entF
                    result = Me.CurrentMaterialStream.Fases(phaseID).SPMProperties.entropyF.GetValueOrDefault * Me.CurrentMaterialStream.Fases(phaseID).SPMProperties.molecularWeight.GetValueOrDefault
                    Me.CurrentMaterialStream.Fases(phaseID).SPMProperties.molar_entropyF = result
                Case "viscosity"
                    If state = "L" Then
                        result = Me.AUX_LIQVISCm(T)
                    Else
                        result = Me.AUX_VAPVISCMIX(T, P, Me.AUX_MMM(phase))
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

        Public Overrides Function DW_CalcPVAP_ISOL(ByVal T As Double) As Double

            Return 0.0#

        End Function

        Public Overrides Function DW_CalcTensaoSuperficial_ISOL(ByVal fase1 As Fase, ByVal T As Double, ByVal P As Double) As Double

            Return Me.AUX_SURFTM(T)

        End Function

        Public Overrides Sub DW_CalcTwoPhaseProps(ByVal fase1 As Fase, ByVal fase2 As Fase)

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

        Public Overrides Function DW_CalcViscosidadeDinamica_ISOL(ByVal fase1 As Fase, ByVal T As Double, ByVal P As Double) As Double

            If fase1 = Fase.Liquid Then
                Return Me.AUX_LIQVISCm(T)
            ElseIf fase1 = Fase.Vapor Then
                Return Me.AUX_VAPVISCm(T, Me.AUX_VAPDENS(T, P), Me.AUX_MMM(Fase.Vapor))
            End If

        End Function

        Public Overrides Function SupportsComponent(ByVal comp As ClassesBasicasTermodinamica.ConstantProperties) As Boolean

            CheckIfCompoundIsSupported(comp.Name)

            Return True

        End Function

        Public Overrides Function DW_CalcEnergiaMistura_ISOL(T As Double, P As Double) As Double

        End Function

#End Region

#Region "    Auxiliary Functions"

        Sub CheckIfCompoundIsSupported(compname As String)

            Dim comps() As String = CoolPropInterface.CoolProp.get_global_param_string("FluidsList").Split(",")

            If Not comps.Contains(compname) Then
                Throw New ArgumentOutOfRangeException(compname, "Error: compound '" & compname & "' is not supported by this version of CoolProp.")
            End If

        End Sub

#End Region

    End Class

End Namespace

