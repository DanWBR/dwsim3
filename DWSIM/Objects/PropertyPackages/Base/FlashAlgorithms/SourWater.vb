'    Flash Algorithms for Sour Water simulations
'    Copyright 2016 Daniel Wagner O. de Medeiros
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

Imports System.Math
Imports DWSIM.DWSIM.SimulationObjects
Imports DWSIM.DWSIM.MathEx
Imports DWSIM.DWSIM.MathEx.Common
Imports DWSIM.DWSIM.Flowsheet.FlowsheetSolver
Imports System.Threading.Tasks
Imports DWSIM.DWSIM.ClassesBasicasTermodinamica
Imports System.Linq
Imports System.IO

Namespace DWSIM.SimulationObjects.PropertyPackages.Auxiliary.FlashAlgorithms

    <System.Serializable()> Public Class SourWater

        Inherits FlashAlgorithm

        Dim etol As Double = 0.000001
        Dim itol As Double = 0.000001
        Dim maxit_i As Integer = 100
        Dim maxit_e As Integer = 100
        Dim Hv0, Hvid, Hlid, Hf, Hv, Hl, Hs As Double
        Dim Sv0, Svid, Slid, Sf, Sv, Sl, Ss As Double

        Public Property CompoundProperties As List(Of ConstantProperties)

        Public Property Reactions As List(Of Reaction)

        Sub New()

            Reactions = New List(Of Reaction)

            Dim rfile As String = My.Application.Info.DirectoryPath & Path.DirectorySeparatorChar & "reactions" & Path.DirectorySeparatorChar & "Sour Water Reaction Set.dwrxm"

            Dim xdoc As XDocument = XDocument.Load(rfile)
            Dim data As List(Of XElement) = xdoc.Element("DWSIM_Reaction_Data").Elements.ToList
            For Each xel As XElement In data
                Dim obj As New Reaction()
                obj.LoadData(xel.Elements.ToList)
                Reactions.Add(obj)
            Next

            '   i   Name                            Equation
            '
            '   1   CO2 ionization	                CO2 + H2O <--> H+ + HCO3- 
            '   2   Carbonate production	        HCO3- <--> CO3-2 + H+ 
            '   3   Ammonia ionization	            H+ + NH3 <--> NH4+ 
            '   4   Carbamate production	        HCO3- + NH3 <--> H2NCOO- + H2O 
            '   5   H2S ionization	                H2S <--> HS- + H+ 
            '   6   Sulfide production	            HS- <--> S-2 + H+ 
            '   7   Water self-ionization	        H2O <--> OH- + H+ 
            '   8   Sodium Hydroxide dissociation   NaOH <--> OH- + Na+ 

        End Sub

        Sub Setup(conc As Dictionary(Of String, Double), conc0 As Dictionary(Of String, Double), deltaconc As Dictionary(Of String, Double), id As Dictionary(Of String, Integer))

            conc.Clear()

            conc.Add("H2O", 0.0#)
            conc.Add("H+", 0.0#)
            conc.Add("OH-", 0.0#)
            conc.Add("NH3", 0.0#)
            conc.Add("NH4+", 0.0#)
            conc.Add("CO2", 0.0#)
            conc.Add("HCO3-", 0.0#)
            conc.Add("CO3-2", 0.0#)
            conc.Add("H2NCOO-", 0.0#)
            conc.Add("H2S", 0.0#)
            conc.Add("HS-", 0.0#)
            conc.Add("S-2", 0.0#)
            conc.Add("NaOH", 0.0#)
            conc.Add("Na+", 0.0#)

            deltaconc.Clear()

            deltaconc.Add("H2O", 0.0#)
            deltaconc.Add("H+", 0.0#)
            deltaconc.Add("OH-", 0.0#)
            deltaconc.Add("NH3", 0.0#)
            deltaconc.Add("NH4+", 0.0#)
            deltaconc.Add("CO2", 0.0#)
            deltaconc.Add("HCO3-", 0.0#)
            deltaconc.Add("CO3-2", 0.0#)
            deltaconc.Add("H2NCOO-", 0.0#)
            deltaconc.Add("H2S", 0.0#)
            deltaconc.Add("HS-", 0.0#)
            deltaconc.Add("S-2", 0.0#)
            deltaconc.Add("NaOH", 0.0#)
            deltaconc.Add("Na+", 0.0#)

            conc0.Clear()

            conc0.Add("H2O", 0.0#)
            conc0.Add("H+", 0.0#)
            conc0.Add("OH-", 0.0#)
            conc0.Add("NH3", 0.0#)
            conc0.Add("NH4+", 0.0#)
            conc0.Add("CO2", 0.0#)
            conc0.Add("HCO3-", 0.0#)
            conc0.Add("CO3-2", 0.0#)
            conc0.Add("H2NCOO-", 0.0#)
            conc0.Add("H2S", 0.0#)
            conc0.Add("HS-", 0.0#)
            conc0.Add("S-2", 0.0#)
            conc0.Add("NaOH", 0.0#)
            conc0.Add("Na+", 0.0#)

            Dim wid As Integer = CompoundProperties.IndexOf((From c As ConstantProperties In CompoundProperties Select c Where c.CAS_Number = "7732-18-5").FirstOrDefault)
            Dim co2id As Integer = CompoundProperties.IndexOf((From c As ConstantProperties In CompoundProperties Select c Where c.Name = "Carbon dioxide").FirstOrDefault)
            Dim nh3id As Integer = CompoundProperties.IndexOf((From c As ConstantProperties In CompoundProperties Select c Where c.Name = "Ammonia").FirstOrDefault)
            Dim h2sid As Integer = CompoundProperties.IndexOf((From c As ConstantProperties In CompoundProperties Select c Where c.Name = "Hydrogen sulfide").FirstOrDefault)
            Dim naohid As Integer = CompoundProperties.IndexOf((From c As ConstantProperties In CompoundProperties Select c Where c.Formula = "NaOH").FirstOrDefault)
            Dim naid As Integer = CompoundProperties.IndexOf((From c As ConstantProperties In CompoundProperties Select c Where c.Formula = "Na+").FirstOrDefault)
            Dim ohid As Integer = CompoundProperties.IndexOf((From c As ConstantProperties In CompoundProperties Select c Where c.Formula = "OH-").FirstOrDefault)
            Dim hid As Integer = CompoundProperties.IndexOf((From c As ConstantProperties In CompoundProperties Select c Where c.Formula = "H+").FirstOrDefault)
            Dim nh4id As Integer = CompoundProperties.IndexOf((From c As ConstantProperties In CompoundProperties Select c Where c.Formula = "NH4+").FirstOrDefault)
            Dim hcoid As Integer = CompoundProperties.IndexOf((From c As ConstantProperties In CompoundProperties Select c Where c.Formula = "HCO3-").FirstOrDefault)
            Dim co3id As Integer = CompoundProperties.IndexOf((From c As ConstantProperties In CompoundProperties Select c Where c.Formula = "CO3-2").FirstOrDefault)
            Dim h2nid As Integer = CompoundProperties.IndexOf((From c As ConstantProperties In CompoundProperties Select c Where c.Formula = "H2NCOO-").FirstOrDefault)
            Dim hsid As Integer = CompoundProperties.IndexOf((From c As ConstantProperties In CompoundProperties Select c Where c.Formula = "HS-").FirstOrDefault)
            Dim s2id As Integer = CompoundProperties.IndexOf((From c As ConstantProperties In CompoundProperties Select c Where c.Formula = "S-2").FirstOrDefault)

            id.Clear()

            id.Add("H2O", wid)
            id.Add("H+", hid)
            id.Add("OH-", ohid)
            id.Add("NH3", nh3id)
            id.Add("NH4+", nh4id)
            id.Add("CO2", co2id)
            id.Add("HCO3-", hcoid)
            id.Add("CO3-2", co3id)
            id.Add("H2NCOO-", h2nid)
            id.Add("H2S", h2sid)
            id.Add("HS-", hsid)
            id.Add("S-2", s2id)
            id.Add("NaOH", naohid)
            id.Add("Na+", naid)


        End Sub

        Public Overrides Function Flash_PT(ByVal Vz As Double(), ByVal P As Double, ByVal T As Double, ByVal PP As PropertyPackages.PropertyPackage, Optional ByVal ReuseKI As Boolean = False, Optional ByVal PrevKi As Double() = Nothing) As Object

            Return Flash_PT_Internal(Vz, P, T, PP)

        End Function

        Public Function Flash_PT_Internal(ByVal Vz As Double(), ByVal P As Double, ByVal T As Double, ByVal PP As PropertyPackages.PropertyPackage) As Object

            Dim d1, d2 As Date, dt As TimeSpan

            d1 = Date.Now

            etol = CDbl(PP.Parameters("PP_PTFELT"))
            maxit_e = CInt(PP.Parameters("PP_PTFMEI"))
            itol = CDbl(PP.Parameters("PP_PTFILT"))
            maxit_i = CInt(PP.Parameters("PP_PTFMII"))

            Dim n As Integer = CompoundProperties.Count - 1
            Dim activcoeff(n), errfunc, nch, pch, nco2, nco2_old, pH, pH_old, pH_old0 As Double
            Dim icount, icount2, ecount As Integer

            'Vnf = feed molar amounts (considering 1 mol of feed)
            'Vnl = liquid phase molar amounts
            'Vnv = vapor phase molar amounts
            'Vxl = liquid phase molar fractions
            'Vxv = vapor phase molar fractions
            'V, L = phase molar amounts (F = 1 = V + L)

            Dim Vnf(n), Vnl(n), Vxl(n), Vxl_ant(n), Vns(n), Vnv(n), Vxv(n), V, L, L_old, Vp(n), Ki(n), fx, fx_old, fx_old0 As Double
            Dim sumN As Double = 0

            Vnf = Vz.Clone

            'set up concentrations & ids

            Dim conc, conc0, deltaconc As New Dictionary(Of String, Double)
            Dim id As New Dictionary(Of String, Integer)

            Setup(conc, conc0, deltaconc, id)

            'calculate initial solution amounts

            Dim totalkg As Double = PP.AUX_MMM(Vz) / 1000 'kg solution

            'calculate equilibrium constants (f(T))

            Dim kr As New List(Of Double)

            For Each r In Reactions
                kr.Add(r.EvaluateK(T, PP))
            Next

            'calculate NH3-H2S-CO2-H2O VLE

            Dim nl As New DWSIMDefault()

            'loop 1: VLE

            ecount = 0
            L = 0.0#

            Do

                L_old = L

                Dim flashresult = nl.CalculateEquilibrium(FlashSpec.P, FlashSpec.T, P, T, PP, Vnf, Nothing, 0.0#)

                With flashresult
                    L = .GetLiquidPhase1MoleFraction
                    V = .GetVaporPhaseMoleFraction
                    Vxl = .GetLiquidPhase1MoleFractions
                    Vxv = .GetVaporPhaseMoleFractions
                    Vnl = Vxl.MultiplyConstY(L)
                    Vnv = Vxv.MultiplyConstY(V)
                End With

                If Abs(L - L_old) < etol Then Exit Do

                'calculate concentrations

                If id("H2O") > -1 Then conc("H2O") = Vnl(id("H2O")) / totalkg
                If id("H+") > -1 Then conc("H+") = Vnl(id("H+")) / totalkg
                If id("OH-") > -1 Then conc("OH-") = Vnl(id("OH-")) / totalkg
                If id("CO2") > -1 Then conc("CO2") = Vnl(id("CO2")) / totalkg
                If id("HCO3-") > -1 Then conc("HCO3-") = Vnl(id("HCO3-")) / totalkg
                If id("CO3-2") > -1 Then conc("CO3-2") = Vnl(id("CO3-2")) / totalkg
                If id("H2NCOO-") > -1 Then conc("H2NCOO-") = Vnl(id("H2NCOO-")) / totalkg
                If id("NH3") > -1 Then conc("NH3") = Vnl(id("NH3")) / totalkg
                If id("NH4+") > -1 Then conc("NH4+") = Vnl(id("NH4+")) / totalkg
                If id("H2S") > -1 Then conc("H2S") = Vnl(id("H2S")) / totalkg
                If id("HS-") > -1 Then conc("HS-") = Vnl(id("HS-")) / totalkg
                If id("S-2") > -1 Then conc("S-2") = Vnl(id("S-2")) / totalkg
                If id("NaOH") > -1 Then conc("NaOH") = Vnl(id("NaOH")) / totalkg
                If id("Na+") > -1 Then conc("Na+") = Vnl(id("Na+")) / totalkg

                'assume an initial concentration of bicarbonate.

                If conc("HCO3-") = 0.0# Then conc("HCO3-") = 1.0E-40

                conc0("HCO3-") = conc("HCO3-")
                conc0("CO3-2") = conc("CO3-2")
                conc0("NH4+") = conc("NH4+")
                conc0("H2NCOO-") = conc("H2NCOO-")
                conc0("HS-") = conc("HS-")
                conc0("S-2") = conc("S-2")
                conc0("OH-") = conc("OH-")
                conc0("Na+") = conc("Na+")
                conc0("H2O") = conc("H2O")
                conc0("CO2") = conc("CO2")
                conc0("H2S") = conc("H2S")
                conc0("NH3") = conc("NH3")
                conc0("NaOH") = conc("NaOH")

                'loop 2: bicarbonate conc. convergence

                icount = 0

                Do

                    'loop 3: pH convergence

                    If conc("H+") > 0.0# Then
                        pH = -Log10(conc("H+"))
                    Else
                        pH = 7.0#
                        conc("H+") = 10 ^ (-pH)
                    End If

                    icount2 = 0

                    Do

                        'calculate liquid phase chemical equilibrium

                        '   1   CO2 ionization	                CO2 + H2O <--> H+ + HCO3- 

                        conc0("HCO3-") = conc("HCO3-")
                        conc("HCO3-") = kr(0) * conc("CO2") / conc("H+")
                        deltaconc("HCO3-") = conc("HCO3-") - conc0("HCO3-")

                        '   2   Carbonate production	        HCO3- <--> CO3-2 + H+ 

                        conc0("CO3-2") = conc("CO3-2")
                        conc("CO3-2") = kr(1) * conc("HCO3-") / conc("H+")
                        deltaconc("CO3-2") = conc("CO3-2") - conc0("CO3-2")

                        '   3   Ammonia ionization	            H+ + NH3 <--> NH4+ 

                        conc0("NH4+") = conc("NH4+")
                        conc("NH4+") = kr(2) * conc("NH3") * conc("H+")
                        deltaconc("NH4+") = conc("NH4+") - conc0("NH4+")

                        '   4   Carbamate production	        HCO3- + NH3 <--> H2NCOO- + H2O 

                        conc0("H2NCOO-") = conc("H2NCOO-")
                        conc("H2NCOO-") = kr(3) * conc("HCO3-") * conc("NH3")
                        deltaconc("H2NCOO-") = conc("H2NCOO-") - conc0("H2NCOO-")

                        '   5   H2S ionization	                H2S <--> HS- + H+ 

                        conc0("HS-") = conc("HS-")
                        conc("HS-") = kr(4) * conc("H2S") / conc("H+")
                        deltaconc("HS-") = conc("HS-") - conc0("HS-")

                        '   6   Sulfide production	            HS- <--> S-2 + H+ 

                        conc0("S-2") = conc("S-2")
                        conc("S-2") = kr(5) * conc("HS-") / conc("H+")
                        deltaconc("S-2") = conc("S-2") - conc0("S-2")

                        '   7   Water self-ionization	        H2O <--> OH- + H+ 

                        conc0("OH-") = conc("OH-")
                        conc("OH-") = kr(6) / conc("H+")
                        deltaconc("OH-") = conc("OH-") - conc0("OH-")

                        '   8   Sodium Hydroxide dissociation   NaOH <--> OH- + Na+ 

                        conc0("Na+") = conc("Na+")
                        conc("Na+") = kr(7) * conc("NaOH") / conc("OH-")
                        deltaconc("Na+") = conc("Na+") - conc0("Na+")

                        'neutrality check

                        pch = conc("H+") + conc("NH4+") + conc("Na+")
                        nch = conc("OH-") + conc("HCO3-") + conc("H2NCOO-") + conc("HS-") + 2 * conc("S-2") + 2 * conc("CO3-2")

                        If Abs((pch - nch) / pch) < etol Then Exit Do

                        'mass balance

                        conc0("H2O") = conc("H2O")
                        conc("H2O") -= deltaconc("OH-")
                        deltaconc("H2O") = conc("H2O") - conc0("H2O")

                        conc0("CO2") = conc("CO2")
                        conc("CO2") -= deltaconc("HCO3-")
                        If conc("CO2") < 0.0# Then conc("CO2") = 0.0#
                        deltaconc("CO2") = conc("CO2") - conc0("CO2")

                        conc0("H2S") = conc("H2S")
                        conc("H2S") -= deltaconc("HS-")
                        If conc("H2S") < 0.0# Then conc("H2S") = 0.0#
                        deltaconc("H2S") = conc("H2S") - conc0("H2S")

                        conc0("NH3") = conc("NH3")
                        conc("NH3") -= deltaconc("NH4+")
                        If conc("NH3") < 0.0# Then conc("NH3") = 0.0#
                        deltaconc("NH3") = conc("NH3") - conc0("NH3")

                        conc0("NaOH") = conc("NaOH")
                        conc("NaOH") -= deltaconc("Na+")
                        If conc("NaOH") < 0.0# Then conc("NaOH") = 0.0#
                        deltaconc("NaOH") = conc("NaOH") - conc0("NaOH")

                        fx_old0 = fx_old
                        fx_old = fx
                        fx = pch - nch

                        If Abs(fx - fx_old) < itol Then Exit Do

                        pH_old0 = pH_old
                        pH_old = pH

                        If icount2 <= 2 Then
                            pH += 0.1
                        Else
                            pH = pH - 0.1 * fx * (pH - pH_old0) / (fx - fx_old0)
                            If pH < 2.0# Then pH = 2.0#
                            If pH > 14.0# Then pH = 14.0#
                        End If

                        conc("H+") = 10 ^ (-pH)

                        icount2 += 1

                    Loop

                    nco2_old = nco2

                    nco2 = conc("CO2") + conc("HCO3-") + conc("CO3-2") + conc("H2NCOO-")

                    If Abs(nco2 - nco2_old) < etol Then Exit Do

                    conc0("HCO3-") = conc("HCO3-")
                    If icount < 1 Then
                        conc("HCO3-") *= 1.1
                    Else
                        conc("HCO3-") *= nco2 / nco2_old
                    End If
                    deltaconc("HCO3-") = conc("HCO3-") - conc0("HCO3-")

                    icount += 1

                Loop

                ecount += 1

            Loop

            'return flash calculation results.

            d2 = Date.Now

            dt = d2 - d1

            WriteDebugInfo("PT Flash [Seawater]: Converged in " & ecount & " iterations. Time taken: " & dt.TotalMilliseconds & " ms. Error function value: " & errfunc)

out:        Return New Object() {L, V, Vxl, Vxv, ecount, 0.0#, PP.RET_NullVector, 0.0#, PP.RET_NullVector()}

        End Function

        Public Overrides Function Flash_PH(ByVal Vz As Double(), ByVal P As Double, ByVal H As Double, ByVal Tref As Double, ByVal PP As PropertyPackages.PropertyPackage, Optional ByVal ReuseKI As Boolean = False, Optional ByVal PrevKi As Double() = Nothing) As Object

            Dim Vn(1) As String, Vx(1), Vy(1), Vx_ant(1), Vy_ant(1), Vp(1), Ki(1), Ki_ant(1), fi(1), Vs(1) As Double
            Dim i, n, ecount As Integer
            Dim d1, d2 As Date, dt As TimeSpan
            Dim L, V, T, S, Pf As Double

            d1 = Date.Now

            n = UBound(Vz)

            PP = PP
            Hf = H
            Pf = P

            ReDim Vn(n), Vx(n), Vy(n), Vx_ant(n), Vy_ant(n), Vp(n), Ki(n), fi(n), Vs(n)

            Vn = PP.RET_VNAMES()
            fi = Vz.Clone

            Dim maxitINT As Integer = CInt(PP.Parameters("PP_PHFMII"))
            Dim maxitEXT As Integer = CInt(PP.Parameters("PP_PHFMEI"))
            Dim tolINT As Double = CDbl(PP.Parameters("PP_PHFILT"))
            Dim tolEXT As Double = CDbl(PP.Parameters("PP_PHFELT"))

            Dim Tsup, Tinf

            Tinf = 100
            Tsup = 2000

            Dim bo As New BrentOpt.Brent
            bo.DefineFuncDelegate(AddressOf Herror)
            WriteDebugInfo("PH Flash: Starting calculation for " & Tinf & " <= T <= " & Tsup)

            Dim fx, fx2, dfdx, x1 As Double

            Dim cnt As Integer = 0

            Tref = 300.0#
            x1 = Tref
            Do
                fx = Herror(x1, {P, Vz, PP})
                fx2 = Herror(x1 + 1, {P, Vz, PP})
                If Abs(fx) < etol Then Exit Do
                dfdx = (fx2 - fx)
                x1 = x1 - fx / dfdx
                If x1 < 0 Then GoTo alt
                cnt += 1
            Loop Until cnt > 100 Or Double.IsNaN(x1)
            If Double.IsNaN(x1) Then
alt:            T = bo.BrentOpt(Tinf, Tsup, 100, tolEXT, maxitEXT, {P, Vz, PP})
            Else
                T = x1
            End If

            Dim Hs, Hl, Hv, xl, xv, xs As Double

            Dim wid As Integer = CompoundProperties.IndexOf((From c As ConstantProperties In CompoundProperties Select c Where c.CAS_Number = "7732-18-5").SingleOrDefault)

            Hs = PP.DW_CalcSolidEnthalpy(T, Vz, CompoundProperties)
            Hl = PP.DW_CalcEnthalpy(Vz, T, P, State.Liquid)
            Hv = PP.DW_CalcEnthalpy(Vz, T, P, State.Vapor)

            Dim Tsat As Double = PP.AUX_TSATi(P, wid)

            Dim tmp As Object() = Nothing

            If T > Tsat Then
                'vapor only
                tmp = Flash_PT(Vz, P, T, PP)
                L = tmp(0)
                V = tmp(1)
                S = tmp(7)
                Vx = tmp(2)
                Vy = tmp(3)
                Vs = tmp(8)
            ElseIf H <= Hs Then
                'solids only.
                tmp = Flash_PT(Vz, P, T, PP)
                L = 0.0#
                V = 0.0#
                S = 1.0#
                Vx = PP.RET_NullVector()
                Vy = PP.RET_NullVector()
                Vs = Vz
            ElseIf H > Hs And H <= Hl Then
                'partial liquefaction.
                xl = (H - Hs) / (Hl - Hs)
                xs = 1 - xl
                tmp = Flash_PT(Vz, P, T, PP)
                L = xl
                V = 0.0#
                S = xs
                Vx = Vz
                Vy = PP.RET_NullVector()
                Vs = Vz
            ElseIf H > Hl And H <= Hv Then
                'partial vaporization.
                xv = (H - Hl) / (Hv - Hl)
                xl = 1 - xv
                Vz(wid) -= xv
                Vz = Vz.NormalizeY()
                tmp = Flash_PT(Vz, P, T, PP)
                L = tmp(0) * xl
                V = xv
                S = tmp(7) * xl
                Vx = tmp(2)
                Vy = PP.RET_NullVector()
                Vy(wid) = 1.0#
                Vs = tmp(8)
            Else
                'vapor only.
                tmp = Flash_PT(Vz, P, T, PP)
                L = tmp(0)
                V = tmp(1)
                S = tmp(7)
                Vx = tmp(2)
                Vy = tmp(3)
                Vs = tmp(8)
            End If

            ecount = tmp(4)

            For i = 0 To n
                Ki(i) = Vy(i) / Vx(i)
            Next

            d2 = Date.Now

            dt = d2 - d1

            WriteDebugInfo("PH Flash [Seawater]: Converged in " & ecount & " iterations. Time taken: " & dt.TotalMilliseconds & " ms.")

            Return New Object() {L, V, Vx, Vy, T, ecount, Ki, 0.0#, PP.RET_NullVector, S, Vs}

        End Function

        Public Overrides Function Flash_PS(ByVal Vz As Double(), ByVal P As Double, ByVal S As Double, ByVal Tref As Double, ByVal PP As PropertyPackages.PropertyPackage, Optional ByVal ReuseKI As Boolean = False, Optional ByVal PrevKi As Double() = Nothing) As Object

            Dim doparallel As Boolean = My.Settings.EnableParallelProcessing

            Dim Vn(1) As String, Vx(1), Vy(1), Vx_ant(1), Vy_ant(1), Vp(1), Ki(1), Ki_ant(1), fi(1), Vs(1) As Double
            Dim i, n, ecount As Integer
            Dim d1, d2 As Date, dt As TimeSpan
            Dim L, V, Ss, T, Pf As Double

            d1 = Date.Now

            n = UBound(Vz)

            PP = PP
            Sf = S
            Pf = P

            ReDim Vn(n), Vx(n), Vy(n), Vx_ant(n), Vy_ant(n), Vp(n), Ki(n), fi(n), Vs(n)

            Vn = PP.RET_VNAMES()
            fi = Vz.Clone

            Dim maxitINT As Integer = CInt(PP.Parameters("PP_PSFMII"))
            Dim maxitEXT As Integer = CInt(PP.Parameters("PP_PSFMEI"))
            Dim tolINT As Double = CDbl(PP.Parameters("PP_PSFILT"))
            Dim tolEXT As Double = CDbl(PP.Parameters("PP_PSFELT"))

            Dim Tsup, Tinf ', Ssup, Sinf

            If Tref <> 0 Then
                Tinf = Tref - 200
                Tsup = Tref + 200
            Else
                Tinf = 100
                Tsup = 2000
            End If
            If Tinf < 100 Then Tinf = 100
            Dim bo As New BrentOpt.Brent
            bo.DefineFuncDelegate(AddressOf Serror)
            WriteDebugInfo("PS Flash: Starting calculation for " & Tinf & " <= T <= " & Tsup)

            Dim fx, fx2, dfdx, x1 As Double

            Dim cnt As Integer = 0

            If Tref = 0 Then Tref = 298.15
            x1 = Tref
            Do
                fx = Serror(x1, {P, Vz, PP})
                fx2 = Serror(x1 + 1, {P, Vz, PP})
                If Abs(fx) < etol Then Exit Do
                dfdx = (fx2 - fx)
                x1 = x1 - fx / dfdx
                If x1 < 0 Then GoTo alt
                cnt += 1
            Loop Until cnt > 50 Or Double.IsNaN(x1)
            If Double.IsNaN(x1) Then
alt:            T = bo.BrentOpt(Tinf, Tsup, 10, tolEXT, maxitEXT, {P, Vz, PP})
            Else
                T = x1
            End If

            Dim Ss1, Sl, Sv, xl, xv, xs As Double

            Dim wid As Integer = CompoundProperties.IndexOf((From c As ConstantProperties In CompoundProperties Select c Where c.CAS_Number = "7732-18-5").SingleOrDefault)

            Ss1 = PP.DW_CalcSolidEnthalpy(T, Vz, CompoundProperties) / T
            Sl = PP.DW_CalcEntropy(Vz, T, P, State.Liquid)
            Sv = PP.DW_CalcEntropy(Vz, T, P, State.Vapor)

            Dim Tsat As Double = PP.AUX_TSATi(P, wid)

            Dim tmp As Object() = Nothing

            If T > Tsat Then
                'vapor only
                tmp = Flash_PT(Vz, P, T, PP)
                L = tmp(0)
                V = tmp(1)
                Ss = tmp(7)
                Vx = tmp(2)
                Vy = tmp(3)
                Vs = tmp(8)
            ElseIf S <= Ss1 Then
                'solids only.
                tmp = Flash_PT(Vz, P, T, PP)
                L = 0.0#
                V = 0.0#
                Ss = 1.0#
                Vx = PP.RET_NullVector()
                Vy = PP.RET_NullVector()
                Vs = Vz
            ElseIf S > Ss1 And S <= Sl Then
                'partial liquefaction.
                xl = (S - Ss) / (Sl - Ss1)
                xs = 1 - xl
                tmp = Flash_PT(Vz, P, T, PP)
                L = xl
                V = 0.0#
                Ss = xs
                Vx = Vz
                Vy = PP.RET_NullVector()
                Vs = Vz
            ElseIf S > Sl And S <= Sv Then
                'partial vaporization.
                xv = (S - Sl) / (Sv - Sl)
                xl = 1 - xv
                Vz(wid) -= xv
                Vz = Vz.NormalizeY()
                tmp = Flash_PT(Vz, P, T, PP)
                L = tmp(0) * xl
                V = xv
                Ss = tmp(7) * xl
                Vx = tmp(2)
                Vy = PP.RET_NullVector()
                Vy(wid) = 1.0#
                Vs = tmp(8)
            Else
                'vapor only.
                tmp = Flash_PT(Vz, P, T, PP)
                L = tmp(0)
                V = tmp(1)
                Ss = tmp(7)
                Vx = tmp(2)
                Vy = tmp(3)
                Vs = tmp(8)
            End If

            ecount = tmp(4)

            For i = 0 To n
                Ki(i) = Vy(i) / Vx(i)
            Next

            d2 = Date.Now

            dt = d2 - d1

            WriteDebugInfo("PS Flash [Seawater]: Converged in " & ecount & " iterations. Time taken: " & dt.TotalMilliseconds & " ms.")

            Return New Object() {L, V, Vx, Vy, T, ecount, Ki, 0.0#, PP.RET_NullVector, Ss, Vs}

        End Function

        Function OBJ_FUNC_PH_FLASH(ByVal T As Double, ByVal H As Double, ByVal P As Double, ByVal Vz As Object, ByVal pp As PropertyPackage) As Object

            Dim tmp As Object
            tmp = Me.Flash_PT(Vz, P, T, pp)
            Dim L, V, S, Vx(), Vy(), Vs(), _Hv, _Hl, _Hs As Double

            Dim n = UBound(Vz)

            L = tmp(0)
            V = tmp(1)
            S = tmp(7)
            Vx = tmp(2)
            Vy = tmp(3)
            Vs = tmp(8)

            _Hv = 0
            _Hl = 0
            _Hs = 0

            Dim mmg, mml, mms As Double
            If V > 0 Then _Hv = pp.DW_CalcEnthalpy(Vy, T, P, State.Vapor)
            If L > 0 Then _Hl = pp.DW_CalcEnthalpy(Vx, T, P, State.Liquid)
            If S > 0 Then _Hs = pp.DW_CalcSolidEnthalpy(T, Vs, CompoundProperties)
            mmg = pp.AUX_MMM(Vy)
            mml = pp.AUX_MMM(Vx)
            mms = pp.AUX_MMM(Vs)

            Dim herr As Double = Hf - (mmg * V / (mmg * V + mml * L + mms * S)) * _Hv - (mml * L / (mmg * V + mml * L + mms * S)) * _Hl - (mms * S / (mmg * V + mml * L + mms * S)) * _Hs
            OBJ_FUNC_PH_FLASH = herr

            WriteDebugInfo("PH Flash [Seawater]: Current T = " & T & ", Current H Error = " & herr)

        End Function

        Function OBJ_FUNC_PS_FLASH(ByVal T As Double, ByVal S As Double, ByVal P As Double, ByVal Vz As Object, ByVal pp As PropertyPackage) As Object

            Dim tmp As Object
            tmp = Me.Flash_PT(Vz, P, T, pp)
            Dim L, V, Ssf, Vx(), Vy(), Vs(), _Sv, _Sl, _Ss As Double

            Dim n = UBound(Vz)

            L = tmp(0)
            V = tmp(1)
            Ssf = tmp(7)
            Vx = tmp(2)
            Vy = tmp(3)
            Vs = tmp(8)

            _Sv = 0
            _Sl = 0
            _Ss = 0
            Dim mmg, mml, mms As Double

            If V > 0 Then _Sv = pp.DW_CalcEntropy(Vy, T, P, State.Vapor)
            If L > 0 Then _Sl = pp.DW_CalcEntropy(Vx, T, P, State.Liquid)
            If Ssf > 0 Then _Ss = pp.DW_CalcSolidEnthalpy(T, Vs, CompoundProperties) / (T - 298.15)
            mmg = pp.AUX_MMM(Vy)
            mml = pp.AUX_MMM(Vx)
            mms = pp.AUX_MMM(Vs)

            Dim serr As Double = Sf - (mmg * V / (mmg * V + mml * L + mms * Ssf)) * _Sv - (mml * L / (mmg * V + mml * L + mms * Ssf)) * _Sl - (mms * Ssf / (mmg * V + mml * L + mms * Ssf)) * _Ss
            OBJ_FUNC_PS_FLASH = serr

            WriteDebugInfo("PS Flash [Seawater]: Current T = " & T & ", Current S Error = " & serr)

        End Function

        Function Herror(ByVal Tt As Double, ByVal otherargs As Object) As Double
            Return OBJ_FUNC_PH_FLASH(Tt, Hf, otherargs(0), otherargs(1), otherargs(2))
        End Function

        Function Serror(ByVal Tt As Double, ByVal otherargs As Object) As Double
            Return OBJ_FUNC_PS_FLASH(Tt, Sf, otherargs(0), otherargs(1), otherargs(2))
        End Function

        Public Overrides Function Flash_TV(ByVal Vz As Double(), ByVal T As Double, ByVal V As Double, ByVal Pref As Double, ByVal PP As PropertyPackages.PropertyPackage, Optional ByVal ReuseKI As Boolean = False, Optional ByVal PrevKi As Double() = Nothing) As Object

            Dim n, ecount As Integer
            Dim d1, d2 As Date, dt As TimeSpan

            d1 = Date.Now

            etol = CDbl(PP.Parameters("PP_PTFELT"))
            maxit_e = CInt(PP.Parameters("PP_PTFMEI"))
            itol = CDbl(PP.Parameters("PP_PTFILT"))
            maxit_i = CInt(PP.Parameters("PP_PTFMII"))

            n = UBound(Vz)

            Dim Vx(n), Vy(n), Vs(n), xv, xl, L, S, P As Double
            Dim wid As Integer = CompoundProperties.IndexOf((From c As ConstantProperties In CompoundProperties Select c Where c.CAS_Number = "7732-18-5").SingleOrDefault)

            xv = V
            xl = 1 - V



            d2 = Date.Now

            dt = d2 - d1

            If ecount > maxit_e Then Throw New Exception(DWSIM.App.GetLocalString("PropPack_FlashMaxIt2"))

            WriteDebugInfo("TV Flash [Seawater]: Converged in " & ecount & " iterations. Time taken: " & dt.TotalMilliseconds & " ms.")

            Return New Object() {L, V, Vx, Vy, P, ecount, Vy.DivideY(Vx), 0.0#, PP.RET_NullVector, S, Vs}

        End Function

        Public Overrides Function Flash_PV(ByVal Vz As Double(), ByVal P As Double, ByVal V As Double, ByVal Tref As Double, ByVal PP As PropertyPackages.PropertyPackage, Optional ByVal ReuseKI As Boolean = False, Optional ByVal PrevKi As Double() = Nothing) As Object

            Dim n, ecount As Integer
            Dim d1, d2 As Date, dt As TimeSpan

            d1 = Date.Now

            etol = CDbl(PP.Parameters("PP_PTFELT"))
            maxit_e = CInt(PP.Parameters("PP_PTFMEI"))
            itol = CDbl(PP.Parameters("PP_PTFILT"))
            maxit_i = CInt(PP.Parameters("PP_PTFMII"))

            n = UBound(Vz)

            Dim Vx(n), Vy(n), Vs(n), xv, xl, L, S, T As Double
            Dim wid As Integer = CompoundProperties.IndexOf((From c As ConstantProperties In CompoundProperties Select c Where c.CAS_Number = "7732-18-5").SingleOrDefault)

            xv = V
            xl = 1 - V


            d2 = Date.Now

            dt = d2 - d1

            If ecount > maxit_e Then Throw New Exception(DWSIM.App.GetLocalString("PropPack_FlashMaxIt2"))

            WriteDebugInfo("PV Flash [Seawater]: Converged in " & ecount & " iterations. Time taken: " & dt.TotalMilliseconds & " ms.")

            Return New Object() {L, V, Vx, Vy, T, ecount, Vy.DivideY(Vx), 0.0#, PP.RET_NullVector, S, Vs}

        End Function

    End Class

End Namespace

