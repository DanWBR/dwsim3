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
            '   1   CO2 ionization	                OCO + HOH <--> H+ + HCO3- 
            '   2   Carbonate production	        HCO3- <--> CO3-2 + H+ 
            '   3   Ammonia ionization	            H+ + NH3 <--> NH4+ 
            '   4   Carbamate production	        HCO3- + NH3 <--> H2NCOO- + HOH 
            '   5   H2S ionization	                HSH <--> HS- + H+ 
            '   6   Sulfide production	            HS- <--> S-2 + H+ 
            '   7   Water self-ionization	        HOH <--> OH- + H+ 
            '   8   Sodium Hydroxide dissociation   NaOH <--> OH- + Na+ 

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
            Dim activcoeff(n), errfunc As Double
            Dim ecount As Integer

            'Vnf = feed molar amounts (considering 1 mol of feed)
            'Vnl = liquid phase molar amounts
            'Vnv = vapor phase molar amounts
            'Vns = solid phase molar amounts
            'Vxl = liquid phase molar fractions
            'Vxv = vapor phase molar fractions
            'Vxs = solid phase molar fractions
            'V, S, L = phase molar amounts (F = 1 = V + S + L)
            Dim Vnf(n), Vnl(n), Vxl(n), Vxl_ant(n), Vns(n), Vxs(n), Vnv(n), Vxv(n), V, S, L, Vp(n) As Double
            Dim sumN As Double = 0

            Vnf = Vz.Clone

            Dim wid As Integer = CompoundProperties.IndexOf((From c As ConstantProperties In CompoundProperties Select c Where c.CAS_Number = "7732-18-5").SingleOrDefault)

            'return flash calculation results.

            d2 = Date.Now

            dt = d2 - d1

            WriteDebugInfo("PT Flash [Seawater]: Converged in " & ecount & " iterations. Time taken: " & dt.TotalMilliseconds & " ms. Error function value: " & errfunc)

out:        Return New Object() {L, V, Vxl, Vxv, ecount, 0.0#, PP.RET_NullVector, S, Vxs}

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

