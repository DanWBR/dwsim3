'    DWSIM Nested Loops Flash Algorithms for Solid-Liquid Equilibria (SLE)
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

Imports System.Math
Imports DWSIM.DWSIM.SimulationObjects
Imports DWSIM.DWSIM.MathEx
Imports DWSIM.DWSIM.MathEx.Common
Imports DWSIM.DWSIM.Flowsheet.FlowsheetSolver
Imports System.Threading.Tasks
Imports DWSIM.DWSIM.ClassesBasicasTermodinamica
Imports System.Linq

Namespace DWSIM.SimulationObjects.PropertyPackages.Auxiliary.FlashAlgorithms

    ''' <summary>
    ''' The Flash algorithms in this class are based on the Nested Loops approach to solve equilibrium calculations.
    ''' </summary>
    ''' <remarks></remarks>
    <System.Serializable()> Public Class NestedLoopsSLE

        Inherits FlashAlgorithm

        Dim etol As Double = 0.000001
        Dim itol As Double = 0.000001
        Dim maxit_i As Integer = 100
        Dim maxit_e As Integer = 100
        Dim Hv0, Hvid, Hlid, Hf, Hv, Hl, Hs As Double
        Dim Sv0, Svid, Slid, Sf, Sv, Sl, Ss As Double

        Public Property CompoundProperties As List(Of ConstantProperties)

        Public Property SolidSolution As Boolean = False

        Public Overrides Function Flash_PT(ByVal Vz As Double(), ByVal P As Double, ByVal T As Double, ByVal PP As PropertyPackages.PropertyPackage, Optional ByVal ReuseKI As Boolean = False, Optional ByVal PrevKi As Double() = Nothing) As Object

            If SolidSolution Then
                Return Flash_PT_SS(Vz, P, T, PP, ReuseKI, PrevKi)
            Else
                Return Flash_PT_E(Vz, P, T, PP, ReuseKI, PrevKi)
            End If

        End Function

        Public Function Flash_PT_SS(ByVal Vz As Double(), ByVal P As Double, ByVal T As Double, ByVal PP As PropertyPackages.PropertyPackage, Optional ByVal ReuseKI As Boolean = False, Optional ByVal PrevKi As Double() = Nothing) As Object

            Dim i, n, ecount As Integer
            Dim soma_x, soma_y, soma_s As Double
            Dim d1, d2 As Date, dt As TimeSpan
            Dim L, S, Lant, V As Double

            Dim ids As New List(Of String)

            d1 = Date.Now

            etol = CDbl(PP.Parameters("PP_PTFELT"))
            maxit_e = CInt(PP.Parameters("PP_PTFMEI"))
            itol = CDbl(PP.Parameters("PP_PTFILT"))
            maxit_i = CInt(PP.Parameters("PP_PTFMII"))

            n = UBound(Vz)

            Dim Vn(n) As String, Vx(n), Vy(n), Vx_ant(n), Vy_ant(n), Vp(n), Ki(n), Ki_ant(n), fi(n), Vs(n), Vs_ant(n), activcoeff(n) As Double

            Vn = PP.RET_VNAMES()
            fi = Vz.Clone

            'Calculate Ki`s

            i = 0
            Do
                ids.Add(CompoundProperties(i).Name)
                Vp(i) = PP.AUX_PVAPi(i, T)
                If CompoundProperties(i).TemperatureOfFusion <> 0.0# Then
                    Ki(i) = Exp(-CompoundProperties(i).EnthalpyOfFusionAtTf / (0.00831447 * T) * (1 - T / CompoundProperties(i).TemperatureOfFusion))
                Else
                    Ki(i) = 1.0E+20
                End If
                i += 1
            Loop Until i = n + 1

            V = 0.0#
            L = 1.0#
            S = 0.0#

            i = 0
            Do
                If Vz(i) <> 0.0# Then
                    Vx(i) = Vz(i) * Ki(i) / ((Ki(i) - 1) * L + 1)
                    If Ki(i) <> 0 Then Vs(i) = Vx(i) / Ki(i) Else Vs(i) = Vz(i)
                    If Vs(i) < 0 Then Vs(i) = 0
                    If Vx(i) < 0 Then Vx(i) = 0
                Else
                    Vs(i) = 0
                    Vx(i) = 0
                End If
                i += 1
            Loop Until i = n + 1

            i = 0
            soma_x = 0
            soma_s = 0
            soma_y = 0.0#
            Do
                soma_x = soma_x + Vx(i)
                soma_s = soma_s + Vs(i)
                i = i + 1
            Loop Until i = n + 1
            i = 0
            Do
                Vx(i) = Vx(i) / soma_x
                Vs(i) = Vs(i) / soma_s
                i = i + 1
            Loop Until i = n + 1

            ecount = 0
            Dim convergiu = 0
            Dim F = 0

            Do

                Ki_ant = Ki.Clone

                activcoeff = PP.DW_CalcFugCoeff(Vx, T, P, State.Liquid)

                For i = 0 To n
                    activcoeff(i) = activcoeff(i) * P / Vp(i)
                    If CompoundProperties(i).TemperatureOfFusion <> 0.0# Then
                        Ki(i) = (1 / activcoeff(i)) * Exp(-CompoundProperties(i).EnthalpyOfFusionAtTf / (0.00831447 * T) * (1 - T / CompoundProperties(i).TemperatureOfFusion))
                    Else
                        Ki(i) = 1.0E+20
                    End If
                Next

                i = 0
                Do
                    If Vz(i) <> 0 Then
                        Vs_ant(i) = Vs(i)
                        Vx_ant(i) = Vx(i)
                        Vx(i) = Vz(i) * Ki(i) / ((Ki(i) - 1) * L + 1)
                        Vs(i) = Vx(i) / Ki(i)
                    Else
                        Vy(i) = 0
                        Vx(i) = 0
                    End If
                    i += 1
                Loop Until i = n + 1

                i = 0
                soma_x = 0
                soma_s = 0
                Do
                    soma_x = soma_x + Vx(i)
                    soma_s = soma_s + Vs(i)
                    i = i + 1
                Loop Until i = n + 1
                i = 0
                Do
                    Vx(i) = Vx(i) / soma_x
                    Vs(i) = Vs(i) / soma_s
                    i = i + 1
                Loop Until i = n + 1

                Dim e1 As Double = 0
                Dim e2 As Double = 0
                Dim e3 As Double = 0
                i = 0
                Do
                    e1 = e1 + (Vx(i) - Vx_ant(i))
                    e2 = e2 + (Vs(i) - Vs_ant(i))
                    i = i + 1
                Loop Until i = n + 1

                e3 = (L - Lant)

                If Double.IsNaN(Math.Abs(e1) + Math.Abs(e2)) Then

                    Throw New Exception(DWSIM.App.GetLocalString("PropPack_FlashError"))

                ElseIf Math.Abs(e3) < 0.0000000001 Then

                    convergiu = 1

                    Exit Do

                Else

                    Lant = L

                    F = 0.0#
                    Dim dF = 0
                    i = 0
                    Do
                        If Vz(i) > 0 Then
                            F = F + Vz(i) * (Ki(i) - 1) / (1 + L * (Ki(i) - 1))
                            dF = dF - Vz(i) * (Ki(i) - 1) ^ 2 / (1 + L * (Ki(i) - 1)) ^ 2
                        End If
                        i = i + 1
                    Loop Until i = n + 1

                    If Abs(F) < 0.000001 Then Exit Do

                    L = -0.4 * F / dF + L

                End If

                S = 1 - L

                If L > 1 Then
                    L = 1
                    S = 0
                    i = 0
                    Do
                        Vx(i) = Vz(i)
                        i = i + 1
                    Loop Until i = n + 1
                ElseIf L < 0 Then
                    L = 0
                    S = 1
                    i = 0
                    Do
                        Vs(i) = Vz(i)
                        i = i + 1
                    Loop Until i = n + 1
                End If

                ecount += 1

                If Double.IsNaN(L) Then Throw New Exception(DWSIM.App.GetLocalString("pp_FlashTPVapFracError"))
                If ecount > maxit_e Then Throw New Exception(DWSIM.App.GetLocalString("pp_FlashMaxIt2"))

                Console.WriteLine("PT Flash [NL-SLE]: Iteration #" & ecount & ", LF = " & L)

                CheckCalculatorStatus()

            Loop Until convergiu = 1

            d2 = Date.Now

            dt = d2 - d1

            Console.WriteLine("PT Flash [NL-SLE]: Converged in " & ecount & " iterations. Time taken: " & dt.TotalMilliseconds & " ms. Error function value: " & F)

out:        Return New Object() {L, V, Vx, Vy, ecount, 0.0#, PP.RET_NullVector, S, Vs}

        End Function

        Public Function Flash_PT_E(ByVal Vz As Double(), ByVal P As Double, ByVal T As Double, ByVal PP As PropertyPackages.PropertyPackage, Optional ByVal ReuseKI As Boolean = False, Optional ByVal PrevKi As Double() = Nothing) As Object

            Dim d1, d2 As Date, dt As TimeSpan

            d1 = Date.Now

            etol = CDbl(PP.Parameters("PP_PTFELT"))
            maxit_e = CInt(PP.Parameters("PP_PTFMEI"))
            itol = CDbl(PP.Parameters("PP_PTFILT"))
            maxit_i = CInt(PP.Parameters("PP_PTFMII"))

            'This flash algorithm is for Electrolye/Salt systems with Water as the single solvent.
            'The vapor and solid phases are considered to be ideal.
            'Chemical equilibria is calculated using the reactions enabled in the default reaction set.

            Dim n As Integer = CompoundProperties.Count - 1
            Dim activcoeff(n) As Double
            Dim i As Integer

            'Vnf = feed molar amounts (considering 1 mol of feed)
            'Vnl = liquid phase molar amounts
            'Vnv = vapor phase molar amounts
            'Vns = solid phase molar amounts
            'Vxl = liquid phase molar fractions
            'Vxv = vapor phase molar fractions
            'Vxs = solid phase molar fractions
            'V, S, L = phase molar amounts (F = 1 = V + S + L)
            Dim Vnf(n), Vnl(n), Vxl(n), Vxl_ant(n), Vns(n), Vxs(n), Vnv(n), Vxv(n), V, S, L, L_ant, Vp(n) As Double
            Dim sumN As Double = 0

            Vnf = Vz.Clone

            'calculate SLE.

            Dim ids As New List(Of String)
            For i = 0 To n
                ids.Add(CompoundProperties(i).Name)
                Vp(i) = PP.AUX_PVAPi(i, T)
            Next

            Vxl = Vz.Clone

            'initial estimates for L and S.

            L = 0.0#

            'calculate liquid phase activity coefficients.

            Dim ecount As Integer = 0
            Dim errfunc As Double = 0.0#

            Do

                activcoeff = PP.DW_CalcFugCoeff(Vxl, T, P, State.Liquid)

                For i = 0 To n
                    activcoeff(i) = activcoeff(i) * P / PP.AUX_PVAPi(ids(i), T)
                Next

                Dim Vxlmax(n) As Double

                'calculate maximum solubilities for solids/precipitates.

                For i = 0 To n
                    If CompoundProperties(i).TemperatureOfFusion <> 0.0# Then
                        Vxlmax(i) = (1 / activcoeff(i)) * Exp(-CompoundProperties(i).EnthalpyOfFusionAtTf / (0.00831447 * T) * (1 - T / CompoundProperties(i).TemperatureOfFusion))
                        If Vxlmax(i) > 1 Then Vxlmax(i) = 1.0#
                    Else
                        Vxlmax(i) = 1.0#
                    End If
                Next

                'mass balance.

                Dim hassolids As Boolean = False

                S = 0.0#
                For i = 0 To n
                    If Vnf(i) > Vxlmax(i) Then
                        hassolids = True
                        Vxl(i) = Vxlmax(i)
                        S += Vnf(i) - Vxl(i) * L
                    End If
                Next

                'check for vapors
                V = 0.0#
                For i = 0 To n
                    If P < Vp(i) Then
                        V += Vnf(i)
                        Vxl(i) = 0
                        Vnv(i) = Vnf(i)
                    End If
                Next

                L_ant = L
                If hassolids Then L = 1 - S - V Else L = 1 - V

                For i = 0 To n
                    Vns(i) = Vnf(i) - Vxl(i) * L - Vnv(i)
                    Vnl(i) = Vxl(i) * L
                Next

                For i = 0 To n
                    If Sum(Vnl) <> 0.0# Then Vxl(i) = Vnl(i) / Sum(Vnl) Else Vxl(i) = 0.0#
                    If Sum(Vns) <> 0.0# Then Vxs(i) = Vns(i) / Sum(Vns) Else Vxs(i) = 0.0#
                    If Sum(Vnv) <> 0.0# Then Vxv(i) = Vnv(i) / Sum(Vnv) Else Vxv(i) = 0.0#
                Next

                errfunc = Abs(L - L_ant) ^ 2

                If errfunc <= 0.0000000001 Then Exit Do

                If Double.IsNaN(S) Then Throw New Exception(DWSIM.App.GetLocalString("PP_FlashTPSolidFracError"))
                If ecount > maxit_e Then Throw New Exception(DWSIM.App.GetLocalString("PP_FlashMaxIt2"))

                ecount += 1

            Loop

            'return flash calculation results.

            d2 = Date.Now

            dt = d2 - d1

            Console.WriteLine("PT Flash [NL-SLE]: Converged in " & ecount & " iterations. Time taken: " & dt.TotalMilliseconds & " ms. Error function value: " & errfunc)

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

            If Tref <> 0 Then
                Tinf = Tref - 250
                Tsup = Tref + 250
            Else
                Tinf = 100
                Tsup = 2000
            End If
            If Tinf < 100 Then Tinf = 100

            Dim bo As New BrentOpt.Brent
            bo.DefineFuncDelegate(AddressOf Herror)
            Console.WriteLine("PH Flash: Starting calculation for " & Tinf & " <= T <= " & Tsup)

            Dim fx, fx2, dfdx, x1 As Double

            Dim cnt As Integer = 0

            If Tref = 0 Then Tref = 100.0#
            x1 = Tref
            Do
                fx = Herror(x1, {P, Vz, PP})
                fx2 = Herror(x1 + 1, {P, Vz, PP})
                If Abs(fx) < etol Then Exit Do
                dfdx = (fx2 - fx)
                x1 = x1 - fx / dfdx
                If x1 < 0 Then GoTo alt
                cnt += 1
            Loop Until cnt > 20 Or Double.IsNaN(x1)
            If Double.IsNaN(x1) Then
alt:            T = bo.BrentOpt(Tinf, Tsup, 100, tolEXT, maxitEXT, {P, Vz, PP})
            Else
                T = x1
            End If

            'End If

            Dim tmp As Object = Flash_PT(Vz, P, T, PP)

            L = tmp(0)
            V = tmp(1)
            S = tmp(7)
            Vx = tmp(2)
            Vy = tmp(3)
            Vs = tmp(8)
            ecount = tmp(4)

            For i = 0 To n
                Ki(i) = Vy(i) / Vx(i)
            Next

            d2 = Date.Now

            dt = d2 - d1

            Console.WriteLine("PH Flash [NL-SLE]: Converged in " & ecount & " iterations. Time taken: " & dt.TotalMilliseconds & " ms.")

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
            Console.WriteLine("PS Flash: Starting calculation for " & Tinf & " <= T <= " & Tsup)

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

            Dim tmp As Object = Flash_PT(Vz, P, T, PP)

            L = tmp(0)
            V = tmp(1)
            Ss = tmp(7)
            Vx = tmp(2)
            Vy = tmp(3)
            Vs = tmp(8)
            ecount = tmp(4)

            For i = 0 To n
                Ki(i) = Vy(i) / Vx(i)
            Next

            d2 = Date.Now

            dt = d2 - d1

            Console.WriteLine("PS Flash [NL-SLE]: Converged in " & ecount & " iterations. Time taken: " & dt.TotalMilliseconds & " ms.")

            Return New Object() {L, V, Vx, Vy, T, ecount, Ki, 0.0#, PP.RET_NullVector, Ss, Vs}

        End Function

        Public Overrides Function Flash_TV(ByVal Vz As Double(), ByVal T As Double, ByVal V As Double, ByVal Pref As Double, ByVal PP As PropertyPackages.PropertyPackage, Optional ByVal ReuseKI As Boolean = False, Optional ByVal PrevKi As Double() = Nothing) As Object

            Dim d1, d2 As Date, dt As TimeSpan

            d1 = Date.Now

            d2 = Date.Now

            dt = d2 - d1

            Console.WriteLine("TV Flash [NL-SLE]: Converged in " & 0 & " iterations. Time taken: " & dt.TotalMilliseconds & " ms.")

            Return New Object() {0.0#, 0.0#, PP.RET_NullVector, PP.RET_NullVector, 0, 0, PP.RET_NullVector, 0.0#, PP.RET_NullVector}

        End Function

        Function SolidFractionError(x As Double, otherargs As Object)

            Dim val As Double = (1 - otherargs(0)) - Me.Flash_PT(otherargs(1), otherargs(2), x, otherargs(3))(7)

            Return val

        End Function

        Function OBJ_FUNC_PH_FLASH(ByVal T As Double, ByVal H As Double, ByVal P As Double, ByVal Vz As Object, ByVal pp As PropertyPackage) As Object

            Dim tmp As Object
            tmp = Me.Flash_PT(Vz, P, T, pp)
            Dim L, V, S, Vx(), Vy(), Vs() As Double

            Dim n = UBound(Vz)

            L = tmp(0)
            V = tmp(1)
            S = tmp(7)
            Vx = tmp(2)
            Vy = tmp(3)
            Vs = tmp(8)

            Hv = 0
            Hl = 0
            Hs = 0

            Dim mmg, mml, mms As Double
            If V > 0 Then Hv = pp.DW_CalcEnthalpy(Vy, T, P, State.Vapor)
            If L > 0 Then Hl = pp.DW_CalcEnthalpy(Vx, T, P, State.Liquid)
            If S > 0 Then Hs = pp.DW_CalcSolidEnthalpy(T, Vs, CompoundProperties)
            mmg = pp.AUX_MMM(Vy)
            mml = pp.AUX_MMM(Vx)
            mms = pp.AUX_MMM(Vs)

            Dim herr As Double = Hf - (mmg * V / (mmg * V + mml * L + mms * S)) * Hv - (mml * L / (mmg * V + mml * L + mms * S)) * Hl - (mms * S / (mmg * V + mml * L + mms * S)) * Hs
            OBJ_FUNC_PH_FLASH = herr

            Console.WriteLine("PH Flash [NL]: Current T = " & T & ", Current H Error = " & herr)

        End Function

        Function OBJ_FUNC_PS_FLASH(ByVal T As Double, ByVal S As Double, ByVal P As Double, ByVal Vz As Object, ByVal pp As PropertyPackage) As Object

            Dim tmp As Object
            tmp = Me.Flash_PT(Vz, P, T, pp)
            Dim L, V, Ssf, Vx(), Vy(), Vs() As Double

            Dim n = UBound(Vz)

            L = tmp(0)
            V = tmp(1)
            Ssf = tmp(7)
            Vx = tmp(2)
            Vy = tmp(3)
            Vs = tmp(8)

            Sv = 0
            Sl = 0
            Ss = 0
            Dim mmg, mml, mms As Double

            If V > 0 Then Sv = pp.DW_CalcEntropy(Vy, T, P, State.Vapor)
            If L > 0 Then Sl = pp.DW_CalcEntropy(Vx, T, P, State.Liquid)
            If Ssf > 0 Then Ss = pp.DW_CalcSolidEnthalpy(T, Vs, CompoundProperties) / (T - 298.15)
            mmg = pp.AUX_MMM(Vy)
            mml = pp.AUX_MMM(Vx)
            mms = pp.AUX_MMM(Vs)

            Dim serr As Double = Sf - (mmg * V / (mmg * V + mml * L + mms * Ssf)) * Sv - (mml * L / (mmg * V + mml * L + mms * Ssf)) * Sl - (mms * Ssf / (mmg * V + mml * L + mms * Ssf)) * Ss
            OBJ_FUNC_PS_FLASH = serr

            Console.WriteLine("PS Flash [NL-SLE]: Current T = " & T & ", Current S Error = " & serr)

        End Function

        Function Herror(ByVal Tt As Double, ByVal otherargs As Object) As Double
            Return OBJ_FUNC_PH_FLASH(Tt, Hf, otherargs(0), otherargs(1), otherargs(2))
        End Function

        Function Serror(ByVal Tt As Double, ByVal otherargs As Object) As Double
            Return OBJ_FUNC_PS_FLASH(Tt, Sf, otherargs(0), otherargs(1), otherargs(2))
        End Function

        Public Overrides Function Flash_PV(ByVal Vz As Double(), ByVal P As Double, ByVal V As Double, ByVal Tref As Double, ByVal PP As PropertyPackages.PropertyPackage, Optional ByVal ReuseKI As Boolean = False, Optional ByVal PrevKi As Double() = Nothing) As Object

            If SolidSolution Then
                Return Flash_PV_SS(Vz, P, V, Tref, PP)
            Else
                Return Flash_PV_E(Vz, P, V, Tref, PP)
            End If

        End Function

        Public Function Flash_PV_SS(ByVal Vz As Double(), ByVal P As Double, ByVal V As Double, ByVal Tref As Double, ByVal PP As PropertyPackages.PropertyPackage, Optional ByVal ReuseKI As Boolean = False, Optional ByVal PrevKi As Double() = Nothing) As Object

            Dim i, n, ecount As Integer
            Dim d1, d2 As Date, dt As TimeSpan
            Dim soma_x, soma_s As Double
            Dim L, S, Lf, Sf, T, Tf As Double
            Dim ids As New List(Of String)

            d1 = Date.Now

            etol = CDbl(PP.Parameters("PP_PTFELT"))
            maxit_e = CInt(PP.Parameters("PP_PTFMEI"))
            itol = CDbl(PP.Parameters("PP_PTFILT"))
            maxit_i = CInt(PP.Parameters("PP_PTFMII"))

            n = UBound(Vz)

            PP = PP
            L = V
            Lf = L
            S = 1 - L
            Lf = 1 - Sf
            Tf = T

            Dim Vn(n) As String, Vx(n), Vs(n), Vx_ant(1), Vs_ant(1), Vp(n), Vp2(n), Ki(n), Ki_ant(n), fi(n), activcoeff(n), activcoeff2(n) As Double
            Dim Vt(n), VTF(n), Tmin, Tmax, dFdT As Double

            Vn = PP.RET_VNAMES()
            VTF = PP.RET_VTF()
            fi = Vz.Clone

            If Tref = 0.0# Then

                i = 0
                Tref = 0
                Do
                    If L = 0 Then
                        Tref = MathEx.Common.Min(VTF)
                    Else
                        Tref += Vz(i) * VTF(i)
                    End If
                    Tmin += 0.1 * Vz(i) * VTF(i)
                    Tmax += 2.0 * Vz(i) * VTF(i)
                    i += 1
                Loop Until i = n + 1

            Else

                Tmin = Tref - 50
                Tmax = Tref + 50

            End If

            T = Tref

            'Calculate Ki`s

            i = 0
            Do
                ids.Add(CompoundProperties(i).Name)
                Vp(i) = PP.AUX_PVAPi(i, T)
                If CompoundProperties(i).TemperatureOfFusion <> 0.0# Then
                    Ki(i) = Exp(-CompoundProperties(i).EnthalpyOfFusionAtTf / (0.00831447 * T) * (1 - T / CompoundProperties(i).TemperatureOfFusion))
                Else
                    Ki(i) = 1.0E+20
                End If
                i += 1
            Loop Until i = n + 1

            i = 0
            Do
                If Vz(i) <> 0.0# Then
                    Vx(i) = Vz(i) * Ki(i) / ((Ki(i) - 1) * L + 1)
                    If Ki(i) <> 0 Then Vs(i) = Vx(i) / Ki(i) Else Vs(i) = Vz(i)
                    If Vs(i) < 0 Then Vs(i) = 0
                    If Vx(i) < 0 Then Vx(i) = 0
                Else
                    Vs(i) = 0
                    Vx(i) = 0
                End If
                i += 1
            Loop Until i = n + 1

            i = 0
            soma_x = 0.0#
            soma_s = 0.0#
            Do
                soma_x = soma_x + Vx(i)
                soma_s = soma_s + Vs(i)
                i = i + 1
            Loop Until i = n + 1
            i = 0
            Do
                Vx(i) = Vx(i) / soma_x
                Vs(i) = Vs(i) / soma_s
                i = i + 1
            Loop Until i = n + 1

            Dim marcador3, marcador2, marcador As Integer
            Dim stmp4_ant, stmp4, Tant, fval As Double
            Dim chk As Boolean = False

            ecount = 0
            Do

                marcador3 = 0

                Dim cont_int = 0
                Do

                    Ki_ant = Ki.Clone

                    activcoeff = PP.DW_CalcFugCoeff(Vx, T, P, State.Liquid)

                    For i = 0 To n
                        Vp(i) = PP.AUX_PVAPi(i, T)
                        activcoeff(i) = activcoeff(i) * P / Vp(i)
                        If CompoundProperties(i).TemperatureOfFusion <> 0.0# Then
                            Ki(i) = (1 / activcoeff(i)) * Exp(-CompoundProperties(i).EnthalpyOfFusionAtTf / (0.00831447 * T) * (1 - T / CompoundProperties(i).TemperatureOfFusion))
                        Else
                            Ki(i) = 1.0E+20
                        End If
                    Next

                    marcador = 0
                    If stmp4_ant <> 0 Then
                        marcador = 1
                    End If
                    stmp4_ant = stmp4

                    If L = 0 Then
                        i = 0
                        stmp4 = 0
                        Do
                            stmp4 = stmp4 + Ki(i) * Vs(i)
                            i = i + 1
                        Loop Until i = n + 1
                    Else
                        i = 0
                        stmp4 = 0
                        Do
                            stmp4 = stmp4 + Vx(i) / Ki(i)
                            i = i + 1
                        Loop Until i = n + 1
                    End If

                    If L = 0 Then
                        i = 0
                        Do
                            Vx_ant(i) = Vx(i)
                            Vx(i) = Ki(i) * Vs(i) / stmp4
                            i = i + 1
                        Loop Until i = n + 1
                    Else
                        i = 0
                        Do
                            Vs_ant(i) = Vs(i)
                            Vs(i) = (Vx(i) / Ki(i)) / stmp4
                            i = i + 1
                        Loop Until i = n + 1
                    End If

                    marcador2 = 0
                    If marcador = 1 Then
                        If L = 0 Then
                            If Math.Abs(Vx(0) - Vx_ant(0)) < itol Then
                                marcador2 = 1
                            End If
                        Else
                            If Math.Abs(Vs(0) - Vs_ant(0)) < itol Then
                                marcador2 = 1
                            End If
                        End If
                    End If

                    cont_int = cont_int + 1

                Loop Until marcador2 = 1 Or Double.IsNaN(stmp4) Or cont_int > maxit_i

                Dim K1(n), K2(n), dKdT(n) As Double

                activcoeff = PP.DW_CalcFugCoeff(Vx, T, P, State.Liquid)
                activcoeff2 = PP.DW_CalcFugCoeff(Vx, T + 0.01, P, State.Liquid)

                For i = 0 To n
                    If CompoundProperties(i).TemperatureOfFusion <> 0.0# Then
                        Vp(i) = PP.AUX_PVAPi(i, T)
                        activcoeff(i) = activcoeff(i) * P / Vp(i)
                        K1(i) = (1 / activcoeff(i)) * Exp(-CompoundProperties(i).EnthalpyOfFusionAtTf / (0.00831447 * T) * (1 - T / CompoundProperties(i).TemperatureOfFusion))
                        Vp2(i) = PP.AUX_PVAPi(i, T + 0.01)
                        activcoeff2(i) = activcoeff2(i) * P / Vp2(i)
                        K2(i) = (1 / activcoeff2(i)) * Exp(-CompoundProperties(i).EnthalpyOfFusionAtTf / (0.00831447 * (T + 0.01)) * (1 - (T + 0.01) / CompoundProperties(i).TemperatureOfFusion))
                    Else
                        K1(i) = 1.0E+20
                        K2(i) = 1.0E+20
                    End If
                Next

                For i = 0 To n
                    dKdT(i) = (K2(i) - K1(i)) / 0.01
                Next

                fval = stmp4 - 1

                ecount += 1

                i = 0
                dFdT = 0
                Do
                    If L = 0.0# Then
                        dFdT = dFdT + Vs(i) * dKdT(i)
                    Else
                        dFdT = dFdT - Vx(i) / (Ki(i) ^ 2) * dKdT(i)
                    End If
                    i = i + 1
                Loop Until i = n + 1

                Tant = T
                T = T - fval / dFdT
                'If T < Tmin Then T = Tmin
                'If T > Tmax Then T = Tmax

                Console.WriteLine("PV Flash [NL-SLE]: Iteration #" & ecount & ", T = " & T & ", LF = " & L)

                CheckCalculatorStatus()

            Loop Until Math.Abs(T - Tant) < 0.01 Or Double.IsNaN(T) = True Or ecount > maxit_e Or Double.IsNaN(T) Or Double.IsInfinity(T)

            d2 = Date.Now

            dt = d2 - d1

            Console.WriteLine("PV Flash [NL-SLE]: Converged in " & ecount & " iterations. Time taken: " & dt.TotalMilliseconds & " ms.")

            Return New Object() {L, V, Vx, PP.RET_NullVector, T, ecount, Ki, 0.0#, PP.RET_NullVector, S, Vs}


        End Function

        Public Function Flash_PV_E(ByVal Vz As Double(), ByVal P As Double, ByVal V As Double, ByVal Tref As Double, ByVal PP As PropertyPackages.PropertyPackage, Optional ByVal ReuseKI As Boolean = False, Optional ByVal PrevKi As Double() = Nothing) As Object

            Dim i, n, ecount As Integer
            Dim d1, d2 As Date, dt As TimeSpan
            Dim soma_x, soma_s As Double
            Dim L, S, Lf, Sf, T, Tf As Double
            Dim ids As New List(Of String)

            d1 = Date.Now

            etol = CDbl(PP.Parameters("PP_PTFELT"))
            maxit_e = CInt(PP.Parameters("PP_PTFMEI"))
            itol = CDbl(PP.Parameters("PP_PTFILT"))
            maxit_i = CInt(PP.Parameters("PP_PTFMII"))

            n = UBound(Vz)

            PP = PP
            L = V
            Lf = L
            S = 1 - L
            Lf = 1 - Sf
            Tf = T

            Dim Vn(n) As String, Vx(n), Vs(n), Vx_ant(1), Vs_ant(1), Vp(n), Vp2(n), Ki(n), Ki_ant(n), fi(n), activcoeff(n), activcoeff2(n) As Double
            Dim Vt(n), VTF(n), Tmin, Tmax As Double

            Vn = PP.RET_VNAMES()
            VTF = PP.RET_VTF()
            fi = Vz.Clone

            If Tref = 0.0# Then

                i = 0
                Tref = 0
                Do
                    If L = 0 Then 'L=0
                        Tref = MathEx.Common.Min(VTF)
                    Else
                        Tref += Vz(i) * VTF(i)
                    End If
                    Tmin += 0.1 * Vz(i) * VTF(i)
                    Tmax += 2.0 * Vz(i) * VTF(i)
                    i += 1
                Loop Until i = n + 1

            Else

                Tmin = Tref - 50
                Tmax = Tref + 50

            End If

            T = Tref

            'Calculate Ki`s

            i = 0
            Do
                ids.Add(CompoundProperties(i).Name)
                Vp(i) = PP.AUX_PVAPi(i, T)
                If CompoundProperties(i).TemperatureOfFusion <> 0.0# Then
                    Ki(i) = Exp(-CompoundProperties(i).EnthalpyOfFusionAtTf / (0.00831447 * T) * (1 - T / CompoundProperties(i).TemperatureOfFusion))
                Else
                    Ki(i) = 1.0E+20
                End If
                i += 1
            Loop Until i = n + 1

            i = 0
            Do
                If Vz(i) <> 0.0# Then
                    Vx(i) = Vz(i) * Ki(i) / ((Ki(i) - 1) * L + 1)
                    If Ki(i) <> 0 Then Vs(i) = Vx(i) / Ki(i) Else Vs(i) = Vz(i)
                    If Vs(i) < 0 Then Vs(i) = 0
                    If Vx(i) < 0 Then Vx(i) = 0
                Else
                    Vs(i) = 0
                    Vx(i) = 0
                End If
                i += 1
            Loop Until i = n + 1

            i = 0
            soma_x = 0.0#
            soma_s = 0.0#
            Do
                soma_x = soma_x + Vx(i)
                soma_s = soma_s + Vs(i)
                i = i + 1
            Loop Until i = n + 1
            i = 0
            Do
                Vx(i) = Vx(i) / soma_x
                Vs(i) = Vs(i) / soma_s
                i = i + 1
            Loop Until i = n + 1

            Dim chk As Boolean = False

            Dim result As Object

            If PP.AUX_IS_SINGLECOMP(Vz) Then
                T = 0
                For i = 0 To n
                    T += Vz(i) * Me.CompoundProperties(i).TemperatureOfFusion
                Next
                result = Me.Flash_PT(Vz, P, T, PP)
                Return New Object() {result(0), result(1), result(2), result(3), T, 0, PP.RET_NullVector, 0.0#, PP.RET_NullVector, result(7), result(8)}
            End If

            T = 0
            For i = 0 To n
                T += Vz(i) * Me.CompoundProperties(i).TemperatureOfFusion - 30
                VTF(i) = Me.CompoundProperties(i).TemperatureOfFusion
            Next

            ecount = 0

            Dim bm As New MathEx.BrentOpt.Brent

            bm.DefineFuncDelegate(AddressOf SolidFractionError)

            T = bm.BrentOpt(Common.Min(VTF) - 30, Common.Max(VTF) + 50, 50, etol, 100, New Object() {L, Vz, P, PP})

            result = Me.Flash_PT_E(Vz, P, T, PP)

            d2 = Date.Now

            dt = d2 - d1

            Console.WriteLine("PV Flash [NL-SLE]: Converged in " & ecount & " iterations. Time taken: " & dt.TotalMilliseconds & " ms.")

            Return New Object() {result(0), result(1), result(2), result(3), T, ecount, PP.RET_NullVector, 0.0#, PP.RET_NullVector, result(7), result(8)}

        End Function

    End Class

End Namespace

