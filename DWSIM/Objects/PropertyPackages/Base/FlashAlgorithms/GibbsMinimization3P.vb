'    Michelsen's Three-Phase Gibbs Minimization w/ Nested Loops Flash Algorithms
'    Copyright 2012-2014 Daniel Wagner O. de Medeiros
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
Imports Cureos.Numerics
Imports DotNumerics.Optimization
Imports System.Threading.Tasks
Imports DotNumerics

Namespace DWSIM.SimulationObjects.PropertyPackages.Auxiliary.FlashAlgorithms

    <System.Serializable()> Public Class GibbsMinimization3P

        Inherits FlashAlgorithm

        Private _io As New BostonBrittInsideOut

        Public ForceTwoPhaseOnly As Boolean = False
        Public L1sat As Double = 0.0#
        Dim ThreePhase As Boolean = False
        Dim i, j, k, n, ecount As Integer
        Dim etol As Double = 0.01
        Dim itol As Double = 0.01
        Dim maxit_i As Integer = 1000
        Dim maxit_e As Integer = 1000
        Dim Vn(n) As String
        Dim Vx1(n), Vx2(n), Vy(n), Vv(n), Vl(n), fi(n), Vp(n), Ki(n) As Double
        Dim Vx(n), Vx_ant(n), Vy_ant(n), Ki_ant(n) As Double
        Dim F, L, L1, L2, V, Tf, Pf, Hf, Sf As Double
        Dim DHv, DHl, DHl1, DHl2, Hv0, Hvid, Hlid1, Hlid2, Hm, Hv, Hl1, Hl2 As Double
        Dim DSv, DSl, DSl1, DSl2, Sv0, Svid, Slid1, Slid2, Sm, Sv, Sl1, Sl2 As Double
        Dim DGv, DGl, DGl1, DGl2, Gv0, Gvid, Glid1, Glid2, Gm, Gv, Gl1, Gl2 As Double
        Dim MMv, MMl1, MMl2 As Double
        Dim Pb, Pd, Pmin, Pmax, Px, soma_x1, soma_x2, soma_y, soma_x, Tmin, Tmax As Double
        Dim proppack As PropertyPackages.PropertyPackage
        Dim objval, objval0 As Double

        Public Enum numsolver
            Limited_Memory_BGFS = 0
            Truncated_Newton = 1
            Simplex = 2
            IPOPT = 3
        End Enum

        Public Property Solver As numsolver = numsolver.IPOPT

        Public Enum ObjFuncType As Integer
            MinGibbs = 0
            BubblePointT = 1
            BubblePointP = 2
            DewPointT = 3
            DewPointP = 4
        End Enum

        Dim objfunc As ObjFuncType = ObjFuncType.MinGibbs

        Public Overrides Function Flash_PT(ByVal Vz() As Double, ByVal P As Double, ByVal T As Double, ByVal PP As PropertyPackage, Optional ByVal ReuseKI As Boolean = False, Optional ByVal PrevKi() As Double = Nothing) As Object

            Dim d1, d2 As Date, dt As TimeSpan

            d1 = Date.Now

            etol = CDbl(PP.Parameters("PP_PTFELT"))
            maxit_e = CInt(PP.Parameters("PP_PTFMEI"))
            itol = CDbl(PP.Parameters("PP_PTFILT"))
            maxit_i = CInt(PP.Parameters("PP_PTFMII"))

            n = UBound(Vz)

            proppack = PP

            ReDim Vn(n), Vx1(n), Vx2(n), Vy(n), Vp(n), Ki(n), fi(n)

            Dim result As Object = Nothing

            Vn = PP.RET_VNAMES()

            Tf = T
            Pf = P

            fi = Vz.Clone

            'Calculate Ki`s

            If Not ReuseKI Then
                i = 0
                Do
                    Vp(i) = PP.AUX_PVAPi(Vn(i), T)
                    Ki(i) = Vp(i) / P
                    i += 1
                Loop Until i = n + 1
            Else
                For i = 0 To n
                    Vp(i) = PP.AUX_PVAPi(Vn(i), T)
                    Ki(i) = PrevKi(i)
                Next
            End If

            'Estimate V

            If T > DWSIM.MathEx.Common.Max(proppack.RET_VTC, Vz) Then
                Vy = Vz
                V = 1
                L = 0
                result = New Object() {L, V, Vx1, Vy, ecount, 0.0#, PP.RET_NullVector, 0.0#, PP.RET_NullVector}
                GoTo out
            End If

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

            If Abs(Pb - Pd) / Pb < 0.0000001 Then
                'one comp only
                If Px <= P Then
                    L = 1
                    V = 0
                    Vx1 = Vz
                    result = New Object() {L, V, Vx1, Vy, ecount, 0.0#, PP.RET_NullVector, 0.0#, PP.RET_NullVector}
                    GoTo out
                Else
                    L = 0
                    V = 1
                    Vy = Vz
                    result = New Object() {L, V, Vx1, Vy, ecount, 0.0#, PP.RET_NullVector, 0.0#, PP.RET_NullVector}
                    GoTo out
                End If
            ElseIf P <= Pd Then
                'vapor only
                L = 0.0#
                V = 1.0#
            ElseIf P >= Pb Then
                'liquid only
                L = 1.0#
                V = 0.0#
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
                If Vz(i) <> 0 Then
                    Vy(i) = Vz(i) * Ki(i) / ((Ki(i) - 1) * V + 1)
                    Vx1(i) = Vy(i) / Ki(i)
                    If Vy(i) < 0 Then Vy(i) = 0
                    If Vx1(i) < 0 Then Vx1(i) = 0
                Else
                    Vy(i) = 0
                    Vx1(i) = 0
                End If
                i += 1
            Loop Until i = n + 1

            i = 0
            soma_x1 = 0
            soma_y = 0
            Do
                soma_x1 = soma_x1 + Vx1(i)
                soma_y = soma_y + Vy(i)
                i = i + 1
            Loop Until i = n + 1
            i = 0
            Do
                Vx1(i) = Vx1(i) / soma_x1
                Vy(i) = Vy(i) / soma_y
                i = i + 1
            Loop Until i = n + 1

            Dim initval(n) As Double
            Dim lconstr(n) As Double
            Dim uconstr(n) As Double
            Dim finalval(n) As Double

            'F = 1000.0#

            Dim maxy As Double = MathEx.Common.Max(Vy)
            Dim imaxy As Integer = Array.IndexOf(Vy, maxy)

            If maxy * V > Vz(imaxy) Then
                V = Vz(imaxy) / maxy * 0.8
            End If

            'V = V * F

            If V <= 0.0# Then V = 0.2

            For i = 0 To n
                initval(i) = Vy(i) * V
                lconstr(i) = 0.0#
                uconstr(i) = Vz(i)
            Next

            ecount = 0

            ThreePhase = False

            objfunc = ObjFuncType.MinGibbs

            objval = 0.0#
            objval0 = 0.0#

            Dim obj As Double
            Dim status As IpoptReturnCode
            Using problem As New Ipopt(initval.Length, lconstr, uconstr, 0, Nothing, Nothing, _
             0, 0, AddressOf eval_f, AddressOf eval_g, _
             AddressOf eval_grad_f, AddressOf eval_jac_g, AddressOf eval_h)
                problem.AddOption("tol", etol)
                problem.AddOption("max_iter", maxit_e)
                problem.AddOption("mu_strategy", "adaptive")
                problem.AddOption("hessian_approximation", "limited-memory")
                'problem.AddOption("hessian_approximation", "exact")
                problem.SetIntermediateCallback(AddressOf intermediate)
                'solve the problem 
                status = problem.SolveProblem(initval, obj, Nothing, Nothing, Nothing, Nothing)
            End Using

            For i = 0 To initval.Length - 1
                If Double.IsNaN(initval(i)) Then initval(i) = 0.0#
            Next

            For i = 0 To n
                Ki(i) = Vy(i) / Vx1(i)
            Next

            'check if the algorithm converged to the trivial solution.
            If PP.AUX_CheckTrivial(Ki) Then
                'rollback to inside-out PT flash.
                Console.WriteLine("PT Flash [GM]: Converged to the trivial solution at specified conditions. Rolling back to Inside-Out PT-Flash...")
                result = _io.Flash_PT(Vz, P, T, PP, ReuseKI, PrevKi)
            ElseIf status = IpoptReturnCode.Maximum_Iterations_Exceeded Then
                'retry with inside-out PT flash.
                Console.WriteLine("PT Flash [GM]: Maximum iterations exceeded. Recalculating with Inside-Out PT-Flash...")
                result = _io.Flash_PT(Vz, P, T, PP, ReuseKI, PrevKi)
            Else
                FunctionValue(initval)
                result = New Object() {L, V, Vx1, Vy, ecount, 0.0#, PP.RET_NullVector, 0.0#, PP.RET_NullVector}
            End If

            'if two-phase only, no need to do stability check on the liquid phase.

            If ForceTwoPhaseOnly = False Then

                ' check if there is a liquid phase

                If result(0) > 0 Then ' we have a liquid phase

                    If result(1) > 0.01 And n = 1 Then
                        'the liquid phase cannot be unstable when there's also vapor and only two compounds in the system.
                        Return result
                    End If

                    Dim nt As Integer = Me.StabSearchCompIDs.Length - 1
                    Dim nc As Integer = UBound(Vz)

                    If nt = -1 Then nt = nc

                    Dim Vtrials(nt, nc) As Double
                    Dim idx(nt) As Integer

                    For i = 0 To nt
                        If Me.StabSearchCompIDs.Length = 0 Then
                            idx(i) = i
                        Else
                            j = 0
                            For Each subst As DWSIM.ClassesBasicasTermodinamica.Substancia In PP.CurrentMaterialStream.Fases(0).Componentes.Values
                                If subst.Nome = Me.StabSearchCompIDs(i) Then
                                    idx(i) = j
                                    Exit For
                                End If
                                j += 1
                            Next
                        End If
                    Next

                    For i = 0 To nt
                        For j = 0 To nc
                            Vtrials(i, j) = 0.00001
                        Next
                    Next
                    For j = 0 To nt
                        Vtrials(j, idx(j)) = 1
                    Next

                    ' do a stability test in the liquid phase

                    Dim stresult As Object = StabTest(T, P, result(2), PP, Vtrials, Me.StabSearchSeverity)

                    If stresult(0) = False Then

                        ' liquid phase NOT stable. proceed to three-phase flash.

                        Dim vx2est(UBound(Vz)) As Double
                        Dim m As Double = LBound(stresult(1), 1)
                        Dim gl, hl, sl, gv, hv, sv, gli As Double

                        If StabSearchSeverity = 2 Then
                            gli = 0
                            For j = 0 To m
                                For i = 0 To UBound(Vz)
                                    vx2est(i) = stresult(1)(j, i)
                                Next
                                hl = PP.DW_CalcEnthalpy(vx2est, T, P, State.Liquid)
                                sl = PP.DW_CalcEntropy(vx2est, T, P, State.Liquid)
                                gl = hl - T * sl
                                If gl <= gli Then
                                    gli = gl
                                    k = j
                                End If
                            Next
                            For i = 0 To UBound(Vz)
                                vx2est(i) = stresult(1)(k, i)
                            Next
                        Else
                            For i = 0 To UBound(Vz)
                                vx2est(i) = stresult(1)(m, i)
                            Next
                        End If

                        hl = PP.DW_CalcEnthalpy(vx2est, T, P, State.Liquid)
                        sl = PP.DW_CalcEntropy(vx2est, T, P, State.Liquid)
                        gl = hl - T * sl

                        hv = PP.DW_CalcEnthalpy(vx2est, T, P, State.Vapor)
                        sv = PP.DW_CalcEntropy(vx2est, T, P, State.Vapor)
                        gv = hv - T * sv

                        If Abs((gl - gv) / gl) > 0.05 Then 'test phase is liquid-like.

                            Dim initval2(2 * n + 1) As Double
                            Dim lconstr2(2 * n + 1) As Double
                            Dim uconstr2(2 * n + 1) As Double
                            Dim finalval2(2 * n + 1) As Double
                            Dim glow(n), gup(n), g(n) As Double

                            Dim maxl As Double = MathEx.Common.Max(vx2est)
                            Dim imaxl As Integer = Array.IndexOf(vx2est, maxl)

                            F = 1000.0#
                            V = result(1) * F
                            L2 = F * fi(imaxl)
                            L1 = F - L2 - V

                            If L1 < 0.0# Then
                                L1 = Abs(L1)
                                L2 = F - L1 - V
                            End If

                            If L2 < 0.0# Then
                                V += L2
                                L2 = Abs(L2)
                            End If

                            For i = 0 To n
                                If Vz(i) <> 0 Then
                                    initval2(i) = Vy(i) * V - vx2est(i) * L2
                                    If initval2(i) < 0.0# Then initval2(i) = 0.0#
                                Else
                                    initval2(i) = 0.0#
                                End If
                                lconstr2(i) = 0.0#
                                uconstr2(i) = fi(i) * F
                                glow(i) = 0.0#
                                gup(i) = 1000.0#
                            Next
                            For i = n + 1 To 2 * n + 1
                                If Vz(i - n - 1) <> 0 Then
                                    initval2(i) = (vx2est(i - n - 1) * L2)
                                    If initval2(i) < 0 Then initval2(i) = 0
                                Else
                                    initval2(i) = 0.0#
                                End If
                                lconstr2(i) = 0.0#
                                uconstr2(i) = fi(i - n - 1) * F
                            Next

                            ecount = 0

                            ThreePhase = True

                            objval = 0.0#
                            objval0 = 0.0#

                            Solver = numsolver.IPOPT

                            Select Case Me.Solver
                                Case numsolver.Limited_Memory_BGFS
                                    Dim variables(2 * n + 1) As OptBoundVariable
                                    For i = 0 To 2 * n + 1
                                        variables(i) = New OptBoundVariable("x" & CStr(i + 1), initval2(i), False, lconstr2(i), uconstr2(i))
                                    Next
                                    Dim solver As New L_BFGS_B
                                    solver.Tolerance = etol
                                    solver.MaxFunEvaluations = maxit_e
                                    initval2 = solver.ComputeMin(AddressOf FunctionValue, AddressOf FunctionGradient, variables)
                                    solver = Nothing
                                Case numsolver.Truncated_Newton
                                    Dim variables(2 * n + 1) As OptBoundVariable
                                    For i = 0 To 2 * n + 1
                                        variables(i) = New OptBoundVariable("x" & CStr(i + 1), initval2(i), False, lconstr2(i), uconstr2(i))
                                    Next
                                    Dim solver As New TruncatedNewton
                                    solver.Tolerance = etol
                                    solver.MaxFunEvaluations = maxit_e
                                    initval2 = solver.ComputeMin(AddressOf FunctionValue, AddressOf FunctionGradient, variables)
                                    solver = Nothing
                                Case numsolver.Simplex
                                    Dim variables(2 * n + 1) As OptBoundVariable
                                    For i = 0 To 2 * n + 1
                                        variables(i) = New OptBoundVariable("x" & CStr(i + 1), initval2(i), False, lconstr2(i), uconstr2(i))
                                    Next
                                    Dim solver As New Simplex
                                    solver.Tolerance = etol
                                    solver.MaxFunEvaluations = maxit_e
                                    initval2 = solver.ComputeMin(AddressOf FunctionValue, variables)
                                    solver = Nothing
                                Case numsolver.IPOPT
                                    Using problem As New Ipopt(initval2.Length, lconstr2, uconstr2, n + 1, glow, gup, (n + 1) * 2, 0, _
                                            AddressOf eval_f, AddressOf eval_g, _
                                            AddressOf eval_grad_f, AddressOf eval_jac_g, AddressOf eval_h)
                                        problem.AddOption("tol", etol)
                                        problem.AddOption("max_iter", maxit_e)
                                        problem.AddOption("mu_strategy", "adaptive")
                                        problem.AddOption("hessian_approximation", "limited-memory")
                                        problem.SetIntermediateCallback(AddressOf intermediate)
                                        'solve the problem 
                                        status = problem.SolveProblem(initval2, obj, g, Nothing, Nothing, Nothing)
                                    End Using
                            End Select

                            For i = 0 To initval2.Length - 1
                                If Double.IsNaN(initval2(i)) Then initval2(i) = 0.0#
                            Next

                            FunctionValue(initval2)

                            'order liquid phases by mixture NBP
                            Dim VNBP = PP.RET_VTB()
                            Dim nbp1 As Double = 0
                            Dim nbp2 As Double = 0

                            For i = 0 To n
                                nbp1 += Vx1(i) * VNBP(i)
                                nbp2 += Vx2(i) * VNBP(i)
                            Next

                            If nbp1 >= nbp2 Then
                                result = New Object() {L1 / F, V / F, Vx1, Vy, ecount, L2 / F, Vx2, 0.0#, PP.RET_NullVector}
                            Else
                                result = New Object() {L2 / F, V / F, Vx2, Vy, ecount, L1 / F, Vx1, 0.0#, PP.RET_NullVector}
                            End If


                        End If

                    End If

                End If

            End If

            d2 = Date.Now

            dt = d2 - d1

            Console.WriteLine("PT Flash [GM]: Converged in " & ecount & " iterations. Status: " & [Enum].GetName(GetType(IpoptReturnCode), status) & ". Time taken: " & dt.TotalMilliseconds & " ms")

out:        Return result

        End Function

        Public Overrides Function Flash_PH(ByVal Vz As Double(), ByVal P As Double, ByVal H As Double, ByVal Tref As Double, ByVal PP As PropertyPackages.PropertyPackage, Optional ByVal ReuseKI As Boolean = False, Optional ByVal PrevKi As Double() = Nothing) As Object

            Dim d1, d2 As Date, dt As TimeSpan

            d1 = Date.Now

            n = UBound(Vz)

            proppack = PP
            Hf = H
            Pf = P

            ReDim Vn(n), Vx1(n), Vx2(n), Vy(n), Vp(n), Ki(n), fi(n)

            Vn = PP.RET_VNAMES()
            fi = Vz.Clone

            Dim maxitINT As Integer = CInt(PP.Parameters("PP_PHFMII"))
            Dim maxitEXT As Integer = CInt(PP.Parameters("PP_PHFMEI"))
            Dim tolINT As Double = CDbl(PP.Parameters("PP_PHFILT"))
            Dim tolEXT As Double = CDbl(PP.Parameters("PP_PHFELT"))

            Dim Tsup, Tinf, Hsup, Hinf

            If Tref <> 0 Then
                Tinf = Tref - 250
                Tsup = Tref + 250
            Else
                Tinf = 100
                Tsup = 2000
            End If
            If Tinf < 100 Then Tinf = 100

            Hinf = PP.DW_CalcEnthalpy(Vz, Tinf, P, State.Liquid)
            Hsup = PP.DW_CalcEnthalpy(Vz, Tsup, P, State.Vapor)

            If H >= Hsup Then
                Tf = Me.ESTIMAR_T_H(H, Tref, "V", P, Vz)
            ElseIf H <= Hinf Then
                Tf = Me.ESTIMAR_T_H(H, Tref, "L", P, Vz)
            Else
                Dim bo As New BrentOpt.Brent
                bo.DefineFuncDelegate(AddressOf Herror)
                Console.WriteLine("PH Flash: Starting calculation for " & Tinf & " <= T <= " & Tsup)

                Dim fx, dfdx, x1 As Double

                Dim cnt As Integer = 0

                If Tref = 0 Then Tref = 298.15
                x1 = Tref
                Do
                    fx = Herror(x1, Nothing)
                    If Abs(fx) < etol Then Exit Do
                    dfdx = (Herror(x1 + 1, Nothing) - fx)
                    x1 = x1 - fx / dfdx
                    cnt += 1
                Loop Until cnt > maxit_e Or Double.IsNaN(x1)
                If Double.IsNaN(x1) Then
                    Tf = bo.BrentOpt(Tinf, Tsup, 4, tolEXT, maxitEXT, Nothing)
                Else
                    Tf = x1
                End If

            End If

            Dim tmp As Object = Flash_PT(Vz, P, Tf, PP)

            L1 = tmp(0)
            V = tmp(1)
            Vx1 = tmp(2)
            Vy = tmp(3)
            ecount = tmp(4)
            L2 = tmp(5)
            Vx2 = tmp(6)

            For i = 0 To n
                Ki(i) = Vy(i) / Vx1(i)
            Next

            d2 = Date.Now

            dt = d2 - d1

            Console.WriteLine("PH Flash [GM]: Converged in " & ecount & " iterations. Time taken: " & dt.TotalMilliseconds & " ms")

            Return New Object() {L, V, Vx1, Vy, Tf, ecount, Ki, L2, Vx2, 0.0#, PP.RET_NullVector}

        End Function

        Public Overrides Function Flash_PS(ByVal Vz As Double(), ByVal P As Double, ByVal S As Double, ByVal Tref As Double, ByVal PP As PropertyPackages.PropertyPackage, Optional ByVal ReuseKI As Boolean = False, Optional ByVal PrevKi As Double() = Nothing) As Object

            Dim d1, d2 As Date, dt As TimeSpan

            d1 = Date.Now

            n = UBound(Vz)

            proppack = PP
            Sf = S
            Pf = P

            ReDim Vn(n), Vx1(n), Vx2(n), Vy(n), Vp(n), Ki(n), fi(n)

            Vn = PP.RET_VNAMES()
            fi = Vz.Clone

            Dim maxitINT As Integer = CInt(PP.Parameters("PP_PSFMII"))
            Dim maxitEXT As Integer = CInt(PP.Parameters("PP_PSFMEI"))
            Dim tolINT As Double = CDbl(PP.Parameters("PP_PSFILT"))
            Dim tolEXT As Double = CDbl(PP.Parameters("PP_PSFELT"))

            Dim Tsup, Tinf, Ssup, Sinf

            If Tref <> 0 Then
                Tinf = Tref - 200
                Tsup = Tref + 200
            Else
                Tinf = 100
                Tsup = 2000
            End If
            If Tinf < 100 Then Tinf = 100

            Sinf = PP.DW_CalcEntropy(Vz, Tinf, P, State.Liquid)
            Ssup = PP.DW_CalcEntropy(Vz, Tsup, P, State.Vapor)

            If S >= Ssup Then
                Tf = Me.ESTIMAR_T_S(S, Tref, "V", P, Vz)
            ElseIf S <= Sinf Then
                Tf = Me.ESTIMAR_T_S(S, Tref, "L", P, Vz)
            Else
                Dim bo As New BrentOpt.Brent
                bo.DefineFuncDelegate(AddressOf Serror)
                Console.WriteLine("PS Flash: Starting calculation for " & Tinf & " <= T <= " & Tsup)

                Dim fx, dfdx, x1 As Double

                ecount = 0

                If Tref = 0 Then Tref = 298.15
                x1 = Tref
                Do
                    fx = Serror(x1, Nothing)
                    If Abs(fx) < etol Then Exit Do
                    dfdx = (Serror(x1 + 1, Nothing) - fx)
                    x1 = x1 - fx / dfdx
                    ecount += 1
                Loop Until ecount > maxit_e Or Double.IsNaN(x1)
                If Double.IsNaN(x1) Then
                    Tf = bo.BrentOpt(Tinf, Tsup, 4, tolEXT, maxitEXT, Nothing)
                Else
                    Tf = x1
                End If

            End If

            Dim tmp As Object = Flash_PT(Vz, P, Tf, PP)

            L1 = tmp(0)
            V = tmp(1)
            Vx1 = tmp(2)
            Vy = tmp(3)
            ecount = tmp(4)
            L2 = tmp(5)
            Vx2 = tmp(6)

            For i = 0 To n
                Ki(i) = Vy(i) / Vx1(i)
            Next

            d2 = Date.Now

            dt = d2 - d1

            Console.WriteLine("PS Flash [GM]: Converged in " & ecount & " iterations. Time taken: " & dt.TotalMilliseconds & " ms")

            Return New Object() {L, V, Vx1, Vy, Tf, ecount, Ki, L2, Vx2, 0.0#, PP.RET_NullVector}

        End Function

        Function OBJ_FUNC_PH_FLASH(ByVal T As Double, ByVal H As Double, ByVal P As Double, ByVal Vz As Object) As Object

            Dim tmp = Me.Flash_PT(Vz, Pf, T, proppack)

            Dim n = UBound(Vz)

            L1 = tmp(0)
            V = tmp(1)
            Vx1 = tmp(2)
            Vy = tmp(3)
            L2 = tmp(5)
            Vx2 = tmp(6)

            Hv = 0
            Hl1 = 0
            Hl2 = 0

            If V > 0.00000001 Then Hv = proppack.DW_CalcEnthalpy(Vy, T, Pf, State.Vapor)
            If L1 > 0.00000001 Then Hl1 = proppack.DW_CalcEnthalpy(Vx1, T, Pf, State.Liquid)
            If L2 > 0.00000001 Then Hl2 = proppack.DW_CalcEnthalpy(Vx2, T, Pf, State.Liquid)

            Dim mmg, mml, mml2
            mmg = proppack.AUX_MMM(Vy)
            mml = proppack.AUX_MMM(Vx1)
            mml2 = proppack.AUX_MMM(Vx2)

            Dim herr As Double = Hf - (mmg * V / (mmg * V + mml * L1 + mml2 * L2)) * Hv - (mml * L1 / (mmg * V + mml * L1 + mml2 * L2)) * Hl1 - (mml2 * L2 / (mmg * V + mml * L1 + mml2 * L2)) * Hl2
            OBJ_FUNC_PH_FLASH = herr

            Console.WriteLine("PH Flash [GM]: Current T = " & T & ", Current H Error = " & herr)

        End Function

        Function OBJ_FUNC_PS_FLASH(ByVal T As Double, ByVal S As Double, ByVal P As Double, ByVal Vz As Object) As Object

            Dim tmp = Me.Flash_PT(Vz, Pf, T, proppack)

            Dim n = UBound(Vz)

            L1 = tmp(0)
            V = tmp(1)
            Vx1 = tmp(2)
            Vy = tmp(3)
            L2 = tmp(5)
            Vx2 = tmp(6)

            Sv = 0
            Sl1 = 0
            Sl2 = 0

            If V > 0.00000001 Then Sv = proppack.DW_CalcEntropy(Vy, T, Pf, State.Vapor)
            If L1 > 0.00000001 Then Sl1 = proppack.DW_CalcEntropy(Vx1, T, Pf, State.Liquid)
            If L2 > 0.00000001 Then Sl2 = proppack.DW_CalcEntropy(Vx2, T, Pf, State.Liquid)

            Dim mmg, mml, mml2
            mmg = proppack.AUX_MMM(Vy)
            mml = proppack.AUX_MMM(Vx1)
            mml2 = proppack.AUX_MMM(Vx2)

            Dim serr As Double = Sf - (mmg * V / (mmg * V + mml * L1 + mml2 * L2)) * Sv - (mml * L1 / (mmg * V + mml * L1 + mml2 * L2)) * Sl1 - (mml2 * L2 / (mmg * V + mml * L1 + mml2 * L2)) * Sl2
            OBJ_FUNC_PS_FLASH = serr

            Console.WriteLine("PS Flash [GM]: Current T = " & T & ", Current S Error = " & serr)

        End Function

        Function ESTIMAR_T_H(ByVal HT As Double, ByVal Tref As Double, ByVal TIPO As String, ByVal P As Double, ByVal Vz As Array) As Double

            Dim maxit As Integer = CInt(proppack.Parameters("PP_PHFMII"))
            Dim tol As Double = CDbl(proppack.Parameters("PP_PHFILT"))

            Dim cnt As Integer = 0
            Dim Tant, Tant2, fi_, fip_, dfdT, fi_ant, fi_ant2 As Double

            Tf = Tref
            Do
                fi_ant2 = fi_ant
                fi_ant = fi_
                If TIPO = "L" Then
                    fi_ = HT - proppack.DW_CalcEnthalpy(Vz, Tf, Pf, State.Liquid)
                Else
                    fi_ = HT - proppack.DW_CalcEnthalpy(Vz, Tf, Pf, State.Vapor)
                End If
                If cnt <= 1 Then
                    Tant2 = Tant
                    Tant = Tf
                    Tf = Tf * 1.1
                Else
                    Tant2 = Tant
                    Tant = Tf
                    fip_ = HT - proppack.DW_CalcEnthalpy(Vz, Tf + 0.01, Pf, State.Vapor)
                    dfdT = (fip_ - fi_) / 0.01
                    Tf = Tf - fi_ / dfdT
                End If
                If Tf < 0 Then
                    Tf = Tant * 1.01
                End If
                cnt += 1
                If cnt >= maxit Then Throw New Exception(DWSIM.App.GetLocalString("PropPack_FlashMaxIt2"))
            Loop Until Math.Abs(fi_ / HT) < tol Or Double.IsNaN(Tf) Or Abs(Tf - Tant) < tol

            Return Tf

        End Function

        Function ESTIMAR_T_S(ByVal ST As Double, ByVal Tref As Double, ByVal TIPO As String, ByVal P As Double, ByVal Vz As Array) As Double

            Dim maxit As Integer = CInt(proppack.Parameters("PP_PSFMII"))
            Dim tol As Double = CDbl(proppack.Parameters("PP_PSFILT"))

            Dim cnt As Integer = 0
            Dim Tant, Tant2, fi_, fip_, dfdT, fi_ant, fi_ant2 As Double

            Tf = Tref
            Do
                fi_ant2 = fi_ant
                fi_ant = fi_
                If TIPO = "L" Then
                    fi_ = ST - proppack.DW_CalcEntropy(Vz, Tf, Pf, State.Liquid)
                Else
                    fi_ = ST - proppack.DW_CalcEntropy(Vz, Tf, Pf, State.Vapor)
                End If
                If cnt <= 1 Then
                    Tant2 = Tant
                    Tant = Tf
                    Tf = Tf * 1.1
                Else
                    Tant2 = Tant
                    Tant = Tf
                    fip_ = ST - proppack.DW_CalcEntropy(Vz, Tf + 0.01, Pf, State.Vapor)
                    dfdT = (fip_ - fi_) / 0.01
                    Tf = Tf - fi_ / dfdT
                End If
                If Tf < 0 Then
                    Tf = Tant * 1.01
                End If
                cnt += 1
                If cnt >= maxit Then Throw New Exception(DWSIM.App.GetLocalString("PropPack_FlashMaxIt2"))
            Loop Until Math.Abs(fi_ / ST) < tol Or Double.IsNaN(Tf) Or Abs(Tf - Tant) < tol

            Return Tf

        End Function

        Function Herror(ByVal Tt As Double, ByVal otherargs As Object) As Double
            Return OBJ_FUNC_PH_FLASH(Tt, Sf, Pf, fi)
        End Function

        Function Serror(ByVal Tt As Double, ByVal otherargs As Object) As Double
            Return OBJ_FUNC_PS_FLASH(Tt, Sf, Pf, fi)
        End Function


        Public Overrides Function Flash_TV(ByVal Vz As Double(), ByVal T As Double, ByVal V As Double, ByVal Pref As Double, ByVal PP As PropertyPackages.PropertyPackage, Optional ByVal ReuseKI As Boolean = False, Optional ByVal PrevKi As Double() = Nothing) As Object

            Dim Vn(1) As String, Vx(1), Vy(1), Vx_ant(1), Vy_ant(1), Vp(1), Ki(1), Ki_ant(1), fi(1) As Double
            Dim i, n, ecount As Integer
            Dim d1, d2 As Date, dt As TimeSpan
            Dim Pmin, Pmax, soma_x, soma_y As Double
            Dim L, Lf, Vf, P, Pf As Double

            d1 = Date.Now

            etol = CDbl(PP.Parameters("PP_PTFELT"))
            maxit_e = CInt(PP.Parameters("PP_PTFMEI"))
            itol = CDbl(PP.Parameters("PP_PTFILT"))
            maxit_i = CInt(PP.Parameters("PP_PTFMII"))

            n = UBound(Vz)

            PP = PP
            Vf = V
            L = 1 - V
            Lf = 1 - Vf
            Pf = P

            ReDim Vn(n), Vx(n), Vy(n), Vx_ant(n), Vy_ant(n), Vp(n), Ki(n), fi(n)
            Dim dFdP As Double

            Dim VTc = PP.RET_VTC()

            Vn = PP.RET_VNAMES()
            fi = Vz.Clone

            If Pref = 0 Then

                i = 0
                Do
                    Vp(i) = PP.AUX_PVAPi(Vn(i), T)
                    i += 1
                Loop Until i = n + 1

                Pmin = Common.Min(Vp)
                Pmax = Common.Max(Vp)

                Pref = Pmin + (1 - V) * (Pmax - Pmin)

            Else

                Pmin = Pref * 0.8
                Pmax = Pref * 1.2

            End If

            P = Pref

            'Calculate Ki`s

            If Not ReuseKI Then
                i = 0
                Do
                    Vp(i) = PP.AUX_PVAPi(Vn(i), T)
                    Ki(i) = Vp(i) / P
                    i += 1
                Loop Until i = n + 1
            Else
                If Not PP.AUX_CheckTrivial(PrevKi) Then
                    For i = 0 To n
                        Vp(i) = PP.AUX_PVAPi(Vn(i), T)
                        Ki(i) = PrevKi(i)
                    Next
                Else
                    i = 0
                    Do
                        Vp(i) = PP.AUX_PVAPi(Vn(i), T)
                        Ki(i) = Vp(i) / P
                        i += 1
                    Loop Until i = n + 1
                End If
            End If

            i = 0
            Do
                If Vz(i) <> 0 Then
                    Vy(i) = Vz(i) * Ki(i) / ((Ki(i) - 1) * V + 1)
                    Vx(i) = Vy(i) / Ki(i)
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

            Dim marcador3, marcador2, marcador As Integer
            Dim stmp4_ant, stmp4, Pant, fval As Double
            Dim chk As Boolean = False

            If V = 1.0# Or V = 0.0# Then

                ecount = 0
                Do

                    marcador3 = 0

                    Dim cont_int = 0
                    Do


                        Ki = PP.DW_CalcKvalue(Vx, Vy, T, P)

                        marcador = 0
                        If stmp4_ant <> 0 Then
                            marcador = 1
                        End If
                        stmp4_ant = stmp4

                        If V = 0 Then
                            i = 0
                            stmp4 = 0
                            Do
                                stmp4 = stmp4 + Ki(i) * Vx(i)
                                i = i + 1
                            Loop Until i = n + 1
                        Else
                            i = 0
                            stmp4 = 0
                            Do
                                stmp4 = stmp4 + Vy(i) / Ki(i)
                                i = i + 1
                            Loop Until i = n + 1
                        End If

                        If V = 0 Then
                            i = 0
                            Do
                                Vy_ant(i) = Vy(i)
                                Vy(i) = Ki(i) * Vx(i) / stmp4
                                i = i + 1
                            Loop Until i = n + 1
                        Else
                            i = 0
                            Do
                                Vx_ant(i) = Vx(i)
                                Vx(i) = (Vy(i) / Ki(i)) / stmp4
                                i = i + 1
                            Loop Until i = n + 1
                        End If

                        marcador2 = 0
                        If marcador = 1 Then
                            If V = 0 Then
                                If Math.Abs(Vy(0) - Vy_ant(0)) < itol Then
                                    marcador2 = 1
                                End If
                            Else
                                If Math.Abs(Vx(0) - Vx_ant(0)) < itol Then
                                    marcador2 = 1
                                End If
                            End If
                        End If

                        cont_int = cont_int + 1

                    Loop Until marcador2 = 1 Or Double.IsNaN(stmp4) Or cont_int > maxit_i

                    Dim K1(n), K2(n), dKdP(n) As Double

                    K1 = PP.DW_CalcKvalue(Vx, Vy, T, P)
                    K2 = PP.DW_CalcKvalue(Vx, Vy, T, P * 1.001)

                    For i = 0 To n
                        dKdP(i) = (K2(i) - K1(i)) / (0.001 * P)
                    Next

                    fval = stmp4 - 1

                    ecount += 1

                    i = 0
                    dFdP = 0
                    Do
                        If V = 0 Then
                            dFdP = dFdP + Vx(i) * dKdP(i)
                        Else
                            dFdP = dFdP - Vy(i) / (Ki(i) ^ 2) * dKdP(i)
                        End If
                        i = i + 1
                    Loop Until i = n + 1

                    If (P - fval / dFdP) < 0 Then
                        P = (P + Pant) / 2
                    Else
                        Pant = P
                        P = P - fval / dFdP
                    End If

                    Console.WriteLine("TV Flash [GM]: Iteration #" & ecount & ", P = " & P & ", VF = " & V)

                    CheckCalculatorStatus()

                Loop Until Math.Abs(P - Pant) < 1 Or Double.IsNaN(P) = True Or ecount > maxit_e Or Double.IsNaN(P) Or Double.IsInfinity(P)

            Else

                ecount = 0

                Do

                    Ki = PP.DW_CalcKvalue(Vx, Vy, T, P)

                    i = 0
                    Do
                        If Vz(i) <> 0 Then
                            Vy_ant(i) = Vy(i)
                            Vx_ant(i) = Vx(i)
                            Vy(i) = Vz(i) * Ki(i) / ((Ki(i) - 1) * V + 1)
                            Vx(i) = Vy(i) / Ki(i)
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
                            stmp4 = stmp4 + Ki(i) * Vx(i)
                            i = i + 1
                        Loop Until i = n + 1

                        Dim K1(n), K2(n), dKdP(n) As Double

                        K1 = PP.DW_CalcKvalue(Vx, Vy, T, P)
                        K2 = PP.DW_CalcKvalue(Vx, Vy, T, P * 1.001)

                        For i = 0 To n
                            dKdP(i) = (K2(i) - K1(i)) / (0.001 * P)
                        Next

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
                            stmp4 = stmp4 + Vy(i) / Ki(i)
                            i = i + 1
                        Loop Until i = n + 1

                        Dim K1(n), K2(n), dKdP(n) As Double

                        K1 = PP.DW_CalcKvalue(Vx, Vy, T, P)
                        K2 = PP.DW_CalcKvalue(Vx, Vy, T, P * 1.001)

                        For i = 0 To n
                            dKdP(i) = (K2(i) - K1(i)) / (0.001 * P)
                        Next

                        i = 0
                        dFdP = 0
                        Do
                            dFdP = dFdP - Vy(i) / (Ki(i) ^ 2) * dKdP(i)
                            i = i + 1
                        Loop Until i = n + 1
                    End If

                    ecount += 1

                    fval = stmp4 - 1

                    If (P - fval / dFdP) < 0 Then
                        P = (P + Pant) / 2
                    Else
                        Pant = P
                        P = P - fval / dFdP
                    End If

                    Console.WriteLine("TV Flash [GM]: Iteration #" & ecount & ", P = " & P & ", VF = " & V)

                    CheckCalculatorStatus()

                Loop Until Math.Abs(fval) < etol Or Double.IsNaN(P) = True Or ecount > maxit_e

            End If

            d2 = Date.Now

            dt = d2 - d1

            Console.WriteLine("TV Flash [GM]: Converged in " & ecount & " iterations. Time taken: " & dt.TotalMilliseconds & " ms.")

            Return New Object() {L, V, Vx, Vy, P, ecount, Ki, 0.0#, PP.RET_NullVector, 0.0#, PP.RET_NullVector}

        End Function

        Public Overrides Function Flash_PV(ByVal Vz As Double(), ByVal P As Double, ByVal V As Double, ByVal Tref As Double, ByVal PP As PropertyPackages.PropertyPackage, Optional ByVal ReuseKI As Boolean = False, Optional ByVal PrevKi As Double() = Nothing) As Object

            Dim Vn(1) As String, Vx(1), Vy(1), Vx_ant(1), Vy_ant(1), Vp(1), Ki(1), Ki_ant(1), fi(1) As Double
            Dim i, n, ecount As Integer
            Dim d1, d2 As Date, dt As TimeSpan
            Dim soma_x, soma_y As Double
            Dim L, Lf, Vf, T, Tf As Double

            d1 = Date.Now

            etol = CDbl(PP.Parameters("PP_PTFELT"))
            maxit_e = CInt(PP.Parameters("PP_PTFMEI"))
            itol = CDbl(PP.Parameters("PP_PTFILT"))
            maxit_i = CInt(PP.Parameters("PP_PTFMII"))

            n = UBound(Vz)

            PP = PP
            Vf = V
            L = 1 - V
            Lf = 1 - Vf
            Tf = T

            ReDim Vn(n), Vx(n), Vy(n), Vx_ant(n), Vy_ant(n), Vp(n), Ki(n), fi(n)
            Dim Vt(n), VTc(n), Tmin, Tmax, dFdT As Double

            Vn = PP.RET_VNAMES()
            VTc = PP.RET_VTC()
            fi = Vz.Clone

            If Tref = 0.0# Then

                i = 0
                Tref = 0
                Do
                    Tref += 0.7 * Vz(i) * VTc(i)
                    Tmin += 0.1 * Vz(i) * VTc(i)
                    Tmax += 2.0 * Vz(i) * VTc(i)
                    i += 1
                Loop Until i = n + 1

            Else

                Tmin = Tref - 50
                Tmax = Tref + 50

            End If

            T = Tref

            'Calculate Ki`s

            If Not ReuseKI Then
                i = 0
                Do
                    Vp(i) = PP.AUX_PVAPi(Vn(i), T)
                    Ki(i) = Vp(i) / P
                    i += 1
                Loop Until i = n + 1
            Else
                If Not PP.AUX_CheckTrivial(PrevKi) And Not Double.IsNaN(PrevKi(0)) Then
                    For i = 0 To n
                        Vp(i) = PP.AUX_PVAPi(Vn(i), T)
                        Ki(i) = PrevKi(i)
                    Next
                Else
                    i = 0
                    Do
                        Vp(i) = PP.AUX_PVAPi(Vn(i), T)
                        Ki(i) = Vp(i) / P
                        i += 1
                    Loop Until i = n + 1
                End If
            End If

            i = 0
            Do
                If Vz(i) <> 0 Then
                    Vy(i) = Vz(i) * Ki(i) / ((Ki(i) - 1) * V + 1)
                    Vx(i) = Vy(i) / Ki(i)
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

            Dim marcador3, marcador2, marcador As Integer
            Dim stmp4_ant, stmp4, Tant, fval As Double
            Dim chk As Boolean = False

            If V = 1.0# Or V = 0.0# Then

                ecount = 0
                Do

                    marcador3 = 0

                    Dim cont_int = 0
                    Do


                        Ki = PP.DW_CalcKvalue(Vx, Vy, T, P)

                        marcador = 0
                        If stmp4_ant <> 0 Then
                            marcador = 1
                        End If
                        stmp4_ant = stmp4

                        If V = 0 Then
                            i = 0
                            stmp4 = 0
                            Do
                                stmp4 = stmp4 + Ki(i) * Vx(i)
                                i = i + 1
                            Loop Until i = n + 1
                        Else
                            i = 0
                            stmp4 = 0
                            Do
                                stmp4 = stmp4 + Vy(i) / Ki(i)
                                i = i + 1
                            Loop Until i = n + 1
                        End If

                        If V = 0 Then
                            i = 0
                            Do
                                Vy_ant(i) = Vy(i)
                                Vy(i) = Ki(i) * Vx(i) / stmp4
                                i = i + 1
                            Loop Until i = n + 1
                        Else
                            i = 0
                            Do
                                Vx_ant(i) = Vx(i)
                                Vx(i) = (Vy(i) / Ki(i)) / stmp4
                                i = i + 1
                            Loop Until i = n + 1
                        End If

                        marcador2 = 0
                        If marcador = 1 Then
                            If V = 0 Then
                                If Math.Abs(Vy(0) - Vy_ant(0)) < itol Then
                                    marcador2 = 1
                                End If
                            Else
                                If Math.Abs(Vx(0) - Vx_ant(0)) < itol Then
                                    marcador2 = 1
                                End If
                            End If
                        End If

                        cont_int = cont_int + 1

                    Loop Until marcador2 = 1 Or Double.IsNaN(stmp4) Or cont_int > maxit_i

                    Dim K1(n), K2(n), dKdT(n) As Double

                    K1 = PP.DW_CalcKvalue(Vx, Vy, T, P)
                    K2 = PP.DW_CalcKvalue(Vx, Vy, T + 0.1, P)

                    For i = 0 To n
                        dKdT(i) = (K2(i) - K1(i)) / (0.1)
                    Next

                    fval = stmp4 - 1

                    ecount += 1

                    i = 0
                    dFdT = 0
                    Do
                        If V = 0 Then
                            dFdT = dFdT + Vx(i) * dKdT(i)
                        Else
                            dFdT = dFdT - Vy(i) / (Ki(i) ^ 2) * dKdT(i)
                        End If
                        i = i + 1
                    Loop Until i = n + 1

                    Tant = T
                    T = T - fval / dFdT
                    If T < Tmin Then T = Tmin
                    If T > Tmax Then T = Tmax

                    Console.WriteLine("PV Flash [GM]: Iteration #" & ecount & ", T = " & T & ", VF = " & V)

                    CheckCalculatorStatus()

                Loop Until Math.Abs(T - Tant) < 0.1 Or Double.IsNaN(T) = True Or ecount > maxit_e Or Double.IsNaN(T) Or Double.IsInfinity(T)

            Else

                ecount = 0

                Do

                    Ki = PP.DW_CalcKvalue(Vx, Vy, T, P)

                    i = 0
                    Do
                        If Vz(i) <> 0 Then
                            Vy_ant(i) = Vy(i)
                            Vx_ant(i) = Vx(i)
                            Vy(i) = Vz(i) * Ki(i) / ((Ki(i) - 1) * V + 1)
                            Vx(i) = Vy(i) / Ki(i)
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
                            stmp4 = stmp4 + Ki(i) * Vx(i)
                            i = i + 1
                        Loop Until i = n + 1

                        Dim K1(n), K2(n), dKdT(n) As Double

                        K1 = PP.DW_CalcKvalue(Vx, Vy, T, P)
                        K2 = PP.DW_CalcKvalue(Vx, Vy, T + 0.1, P)

                        For i = 0 To n
                            dKdT(i) = (K2(i) - K1(i)) / (0.1)
                        Next

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
                            stmp4 = stmp4 + Vy(i) / Ki(i)
                            i = i + 1
                        Loop Until i = n + 1

                        Dim K1(n), K2(n), dKdT(n) As Double

                        K1 = PP.DW_CalcKvalue(Vx, Vy, T, P)
                        K2 = PP.DW_CalcKvalue(Vx, Vy, T + 0.1, P)

                        For i = 0 To n
                            dKdT(i) = (K2(i) - K1(i)) / (0.1)
                        Next

                        i = 0
                        dFdT = 0
                        Do
                            dFdT = dFdT - Vy(i) / (Ki(i) ^ 2) * dKdT(i)
                            i = i + 1
                        Loop Until i = n + 1
                    End If

                    ecount += 1

                    fval = stmp4 - 1

                    Tant = T
                    T = T - fval / dFdT
                    If T < Tmin Then T = Tmin
                    If T > Tmax Then T = Tmax

                    Console.WriteLine("PV Flash [GM]: Iteration #" & ecount & ", T = " & T & ", VF = " & V)

                    CheckCalculatorStatus()

                Loop Until Math.Abs(fval) < etol Or Double.IsNaN(T) = True Or ecount > maxit_e

            End If

            d2 = Date.Now

            dt = d2 - d1

            Console.WriteLine("PV Flash [GM]: Converged in " & ecount & " iterations. Time taken: " & dt.TotalMilliseconds & " ms.")

            Return New Object() {L, V, Vx, Vy, T, ecount, Ki, 0.0#, PP.RET_NullVector, 0.0#, PP.RET_NullVector}

        End Function

        'Function Values

        Private Function FunctionValue(ByVal x() As Double) As Double

            CheckCalculatorStatus()

            Dim pval As Double = 0.0#
            Dim fcv(n), fcl(n), fcl2(n) As Double

            Select Case objfunc

                Case ObjFuncType.MinGibbs

                    If Not ThreePhase Then

                        soma_y = MathEx.Common.Sum(x)
                        V = soma_y
                        L = 1 - soma_y

                        For i = 0 To x.Length - 1
                            If V <> 0.0# Then Vy(i) = Abs(x(i) / V) Else Vy(i) = 0.0#
                            If L <> 0.0# Then Vx1(i) = Abs((fi(i) - x(i)) / L) Else Vx1(i) = 0.0#
                        Next

                        If My.Settings.EnableParallelProcessing Then
                            My.MyApplication.IsRunningParallelTasks = True
                            If My.Settings.EnableGPUProcessing Then
                                My.MyApplication.gpu.EnableMultithreading()
                            End If
                            Try
                                Dim task1 As Task = New Task(Sub()
                                                                 fcv = proppack.DW_CalcFugCoeff(Vy, Tf, Pf, State.Vapor)
                                                             End Sub)
                                Dim task2 As Task = New Task(Sub()
                                                                 fcl = proppack.DW_CalcFugCoeff(Vx1, Tf, Pf, State.Liquid)
                                                             End Sub)
                                task1.Start()
                                task2.Start()
                                Task.WaitAll(task1, task2)
                            Catch ae As AggregateException
                                For Each ex As Exception In ae.InnerExceptions
                                    Throw ex
                                Next
                            Finally
                                If My.Settings.EnableGPUProcessing Then
                                    My.MyApplication.gpu.DisableMultithreading()
                                    My.MyApplication.gpu.FreeAll()
                                End If
                            End Try
                            My.MyApplication.IsRunningParallelTasks = False
                        Else
                            fcv = proppack.DW_CalcFugCoeff(Vy, Tf, Pf, State.Vapor)
                            fcl = proppack.DW_CalcFugCoeff(Vx1, Tf, Pf, State.Liquid)
                        End If

                        Gv = 0
                        Gl1 = 0
                        For i = 0 To x.Length - 1
                            If Vy(i) <> 0.0# Then Gv += Vy(i) * V * Log(fcv(i) * Vy(i))
                            If Vx1(i) <> 0.0# Then Gl1 += Vx1(i) * L * Log(fcl(i) * Vx1(i))
                        Next

                        Gm = Gv + Gl1

                        Console.WriteLine("[GM] V = " & Format(V, "N4") & ", L = " & Format(L, "N4") & " / GE = " & Format(Gm * 8.314 * Tf, "N2") & " kJ/kmol")

                    Else

                        soma_y = 0
                        For i = 0 To x.Length - n - 2
                            soma_y += x(i)
                        Next
                        soma_x2 = 0
                        For i = x.Length - n - 1 To x.Length - 1
                            soma_x2 += x(i)
                        Next
                        V = soma_y
                        L = F - soma_y
                        L2 = soma_x2
                        L1 = F - V - L2

                        pval = 0.0#
                        For i = 0 To n
                            If V <> 0.0# Then Vy(i) = (x(i) / V) Else Vy(i) = 0.0#
                            If L2 <> 0.0# Then Vx2(i) = (x(i + n + 1) / L2) Else Vx2(i) = 0.0#
                            If L1 <> 0.0# Then Vx1(i) = ((fi(i) * F - Vy(i) * V - Vx2(i) * L2) / L1) Else Vx1(i) = 0.0#
                            If Vy(i) < 0.0# Then Vy(i) = 1.0E-20
                            If Vx1(i) < 0.0# Then Vx1(i) = 1.0E-20
                            If Vx2(i) < 0.0# Then Vx2(i) = 1.0E-20
                        Next

                        soma_x1 = 0
                        For i = 0 To n
                            soma_x1 += Vx1(i)
                        Next
                        For i = 0 To n
                            If soma_x1 <> 0.0# Then Vx1(i) /= soma_x1
                        Next

                        If My.Settings.EnableParallelProcessing Then
                            My.MyApplication.IsRunningParallelTasks = True
                            If My.Settings.EnableGPUProcessing Then
                                My.MyApplication.gpu.EnableMultithreading()
                            End If
                            Try
                                Dim task1 As Task = New Task(Sub()
                                                                 fcv = proppack.DW_CalcFugCoeff(Vy, Tf, Pf, State.Vapor)
                                                             End Sub)
                                Dim task2 As Task = New Task(Sub()
                                                                 fcl = proppack.DW_CalcFugCoeff(Vx1, Tf, Pf, State.Liquid)
                                                             End Sub)
                                Dim task3 As Task = New Task(Sub()
                                                                 fcl2 = proppack.DW_CalcFugCoeff(Vx2, Tf, Pf, State.Liquid)
                                                             End Sub)
                                task1.Start()
                                task2.Start()
                                task3.Start()
                                Task.WaitAll(task1, task2, task3)
                            Catch ae As AggregateException
                                For Each ex As Exception In ae.InnerExceptions
                                    Throw ex
                                Next
                            Finally
                                If My.Settings.EnableGPUProcessing Then
                                    My.MyApplication.gpu.DisableMultithreading()
                                    My.MyApplication.gpu.FreeAll()
                                End If
                            End Try
                            My.MyApplication.IsRunningParallelTasks = False
                        Else
                            fcv = proppack.DW_CalcFugCoeff(Vy, Tf, Pf, State.Vapor)
                            fcl = proppack.DW_CalcFugCoeff(Vx1, Tf, Pf, State.Liquid)
                            fcl2 = proppack.DW_CalcFugCoeff(Vx2, Tf, Pf, State.Liquid)
                        End If

                        Gv = 0
                        Gl1 = 0
                        Gl2 = 0
                        For i = 0 To n
                            If Vy(i) <> 0 Then Gv += Vy(i) * V * Log(fcv(i) * Vy(i))
                            If Vx1(i) <> 0 Then Gl1 += Vx1(i) * L1 * Log(fcl(i) * Vx1(i))
                            If Vx2(i) <> 0 Then Gl2 += Vx2(i) * L2 * Log(fcl2(i) * Vx2(i))
                        Next

                        Gm = Gv + Gl1 + Gl2 + pval

                        Console.WriteLine("[GM] V = " & Format(V / 1000, "N4") & ", L1 = " & Format(L1 / 1000, "N4") & ", L2 = " & Format(L2 / 1000, "N4") & " / GE = " & Format(Gm * 8.314 * Tf / 1000, "N2") & " kJ/kmol")

                    End If

                    ecount += 1

                    Return Gm

                Case Else

                    L1 = L1sat
                    L2 = 1 - V - L1

                    Dim sumx As Double = 0
                    For i = 0 To n
                        sumx += x(i)
                    Next

                    Select Case objfunc
                        Case ObjFuncType.BubblePointP, ObjFuncType.DewPointP
                            Pf = x(n + 1)
                        Case ObjFuncType.BubblePointT, ObjFuncType.DewPointT
                            Tf = x(n + 1)
                    End Select

                    Select Case objfunc
                        Case ObjFuncType.BubblePointP, ObjFuncType.BubblePointT
                            For i = 0 To n
                                Vy(i) = x(i) / sumx
                                Vx1(i) = fi(i)
                                Vx2(i) = ((L1 + 0.0000000001) * Vx1(i) - (V + 0.0000000001) * Vy(i)) / L2
                                If L2 = 0.0# Then Vx2(i) = 0
                            Next
                        Case ObjFuncType.DewPointP, ObjFuncType.DewPointT
                            For i = 0 To n
                                Vy(i) = fi(i)
                                Vx1(i) = x(i) / sumx
                                Vx2(i) = ((L1 + 0.0000000001) * Vx1(i) - (V + 0.0000000001) * Vy(i)) / L2
                                If L2 = 0.0# Then Vx2(i) = 0
                            Next
                    End Select

                    For i = 0 To n
                        If Vx2(i) <= 0 Then Vx2(i) = 1.0E-20
                    Next

                    soma_x2 = 0
                    For i = 0 To n
                        soma_x2 += Vx2(i)
                    Next
                    For i = 0 To n
                        Vx2(i) /= soma_x2
                    Next

                    If My.Settings.EnableParallelProcessing Then
                        My.MyApplication.IsRunningParallelTasks = True
                        If My.Settings.EnableGPUProcessing Then
                            My.MyApplication.gpu.EnableMultithreading()
                        End If
                        Try
                            Dim task1 As Task = New Task(Sub()
                                                             fcv = proppack.DW_CalcFugCoeff(Vy, Tf, Pf, State.Vapor)
                                                         End Sub)
                            Dim task2 As Task = New Task(Sub()
                                                             fcl = proppack.DW_CalcFugCoeff(Vx1, Tf, Pf, State.Liquid)
                                                         End Sub)
                            Dim task3 As Task = New Task(Sub()
                                                             fcl2 = proppack.DW_CalcFugCoeff(Vx2, Tf, Pf, State.Liquid)
                                                         End Sub)
                            task1.Start()
                            task2.Start()
                            task3.Start()
                            Task.WaitAll(task1, task2, task3)
                        Catch ae As AggregateException
                            For Each ex As Exception In ae.InnerExceptions
                                Throw ex
                            Next
                        Finally
                            If My.Settings.EnableGPUProcessing Then
                                My.MyApplication.gpu.DisableMultithreading()
                                My.MyApplication.gpu.FreeAll()
                            End If
                        End Try
                        My.MyApplication.IsRunningParallelTasks = False
                    Else
                        fcv = proppack.DW_CalcFugCoeff(Vy, Tf, Pf, State.Vapor)
                        fcl = proppack.DW_CalcFugCoeff(Vx1, Tf, Pf, State.Liquid)
                        fcl2 = proppack.DW_CalcFugCoeff(Vx2, Tf, Pf, State.Liquid)
                    End If

                    Gv = 0
                    Gl1 = 0
                    Gl2 = 0
                    For i = 0 To n
                        If Vy(i) <> 0 Then Gv += Vy(i) * V * Log(fcv(i) * Vy(i))
                        If Vx1(i) <> 0 Then Gl1 += Vx1(i) * L1 * Log(fcl(i) * Vx1(i))
                        If Vx2(i) <> 0 Then Gl2 += Vx2(i) * L2 * Log(fcl2(i) * Vx2(i))
                    Next

                    pval = 0.0#

                    Gm = Gv + Gl1 + Gl2 + pval

                    ecount += 1

                    Return Gm

            End Select

        End Function

        Private Function FunctionGradient(ByVal x() As Double) As Double()

            Dim g(x.Length - 1) As Double
            Dim epsilon As Double = 0.000001
            Dim fcv(x.Length - 1), fcl(x.Length - 1), fcl2(x.Length - 1) As Double
            Dim i As Integer

            Select Case objfunc

                Case ObjFuncType.MinGibbs

                    If Not ThreePhase Then

                        soma_y = MathEx.Common.Sum(x)
                        V = soma_y
                        L = 1 - soma_y

                        For i = 0 To x.Length - 1
                            Vy(i) = Abs(x(i) / V)
                            Vx1(i) = Abs((fi(i) - x(i)) / L)
                        Next

                        If My.Settings.EnableParallelProcessing Then
                            My.MyApplication.IsRunningParallelTasks = True
                            If My.Settings.EnableGPUProcessing Then
                                My.MyApplication.gpu.EnableMultithreading()
                            End If
                            Try
                                Dim task1 As Task = New Task(Sub()
                                                                 fcv = proppack.DW_CalcFugCoeff(Vy, Tf, Pf, State.Vapor)
                                                             End Sub)
                                Dim task2 As Task = New Task(Sub()
                                                                 fcl = proppack.DW_CalcFugCoeff(Vx1, Tf, Pf, State.Liquid)
                                                             End Sub)
                                task1.Start()
                                task2.Start()
                                Task.WaitAll(task1, task2)
                            Catch ae As AggregateException
                                For Each ex As Exception In ae.InnerExceptions
                                    Throw ex
                                Next
                            Finally
                                If My.Settings.EnableGPUProcessing Then
                                    My.MyApplication.gpu.DisableMultithreading()
                                    My.MyApplication.gpu.FreeAll()
                                End If
                            End Try
                            My.MyApplication.IsRunningParallelTasks = False
                        Else
                            fcv = proppack.DW_CalcFugCoeff(Vy, Tf, Pf, State.Vapor)
                            fcl = proppack.DW_CalcFugCoeff(Vx1, Tf, Pf, State.Liquid)
                        End If

                        For i = 0 To x.Length - 1
                            If Vy(i) <> 0 And Vx1(i) <> 0 Then g(i) = Log(fcv(i) * Vy(i) / (fcl(i) * Vx1(i)))
                        Next

                    Else

                        soma_y = 0
                        For i = 0 To x.Length - n - 2
                            soma_y += x(i)
                        Next
                        soma_x2 = 0
                        For i = x.Length - n - 1 To x.Length - 1
                            soma_x2 += x(i)
                        Next
                        V = soma_y
                        L = F - soma_y
                        L2 = soma_x2
                        L1 = F - V - L2

                        For i = 0 To n
                            If V <> 0.0# Then Vy(i) = (x(i) / V) Else Vy(i) = 0.0#
                            If L2 <> 0.0# Then Vx2(i) = (x(i + n + 1) / L2) Else Vx2(i) = 0.0#
                            If L1 <> 0.0# Then Vx1(i) = ((fi(i) * F - Vy(i) * V - Vx2(i) * L2) / L1) Else Vx1(i) = 0.0#
                            If Vy(i) < 0.0# Then Vy(i) = 1.0E-20
                            If Vx1(i) < 0.0# Then Vx1(i) = 1.0E-20
                            If Vx2(i) < 0.0# Then Vx2(i) = 1.0E-20
                        Next

                        soma_x1 = 0
                        For i = 0 To n
                            soma_x1 += Vx1(i)
                        Next
                        For i = 0 To n
                            Vx1(i) /= soma_x1
                        Next

                        If My.Settings.EnableParallelProcessing Then
                            My.MyApplication.IsRunningParallelTasks = True
                            If My.Settings.EnableGPUProcessing Then
                                My.MyApplication.gpu.EnableMultithreading()
                            End If
                            Try
                                Dim task1 As Task = New Task(Sub()
                                                                 fcv = proppack.DW_CalcFugCoeff(Vy, Tf, Pf, State.Vapor)
                                                             End Sub)
                                Dim task2 As Task = New Task(Sub()
                                                                 fcl = proppack.DW_CalcFugCoeff(Vx1, Tf, Pf, State.Liquid)
                                                             End Sub)
                                Dim task3 As Task = New Task(Sub()
                                                                 fcl2 = proppack.DW_CalcFugCoeff(Vx2, Tf, Pf, State.Liquid)
                                                             End Sub)
                                task1.Start()
                                task2.Start()
                                task3.Start()
                                Task.WaitAll(task1, task2, task3)
                            Catch ae As AggregateException
                                For Each ex As Exception In ae.InnerExceptions
                                    Throw ex
                                Next
                            Finally
                                If My.Settings.EnableGPUProcessing Then
                                    My.MyApplication.gpu.DisableMultithreading()
                                    My.MyApplication.gpu.FreeAll()
                                End If
                            End Try
                            My.MyApplication.IsRunningParallelTasks = False
                        Else
                            fcv = proppack.DW_CalcFugCoeff(Vy, Tf, Pf, State.Vapor)
                            fcl = proppack.DW_CalcFugCoeff(Vx1, Tf, Pf, State.Liquid)
                            fcl2 = proppack.DW_CalcFugCoeff(Vx2, Tf, Pf, State.Liquid)
                        End If

                        For i = 0 To x.Length - n - 2
                            If Vy(i) <> 0 And Vx2(i) <> 0 Then g(i) = Log(fcv(i) * Vy(i)) - Log(fcl(i) * Vx1(i))
                        Next
                        For i = x.Length - n - 1 To (x.Length - 1)
                            If Vx1(i - (x.Length - n - 1)) <> 0 And Vx2(i - (x.Length - n - 1)) <> 0 Then g(i) = Log(fcl2(i - (x.Length - n - 1)) * Vx2(i - (x.Length - n - 1))) - Log(fcl(i - (x.Length - n - 1)) * Vx1(i - (x.Length - n - 1)))
                        Next

                    End If

                    Return g

                Case Else

                    L1 = L1sat
                    L2 = 1 - V - L1

                    Dim sumx As Double = 0
                    For i = 0 To n
                        sumx += x(i)
                    Next

                    Select Case objfunc
                        Case ObjFuncType.BubblePointP, ObjFuncType.DewPointP
                            Pf = x(n + 1)
                        Case ObjFuncType.BubblePointT, ObjFuncType.DewPointT
                            Tf = x(n + 1)
                    End Select

                    Select Case objfunc
                        Case ObjFuncType.BubblePointP, ObjFuncType.BubblePointT
                            For i = 0 To n
                                Vy(i) = x(i) / sumx
                                Vx1(i) = fi(i)
                                Vx2(i) = ((L1 + 0.0000000001) * Vx1(i) - (V + 0.0000000001) * Vy(i)) / L2
                                If L2 = 0.0# Then Vx2(i) = 0
                            Next
                        Case ObjFuncType.DewPointP, ObjFuncType.DewPointT
                            For i = 0 To n
                                Vy(i) = fi(i)
                                Vx1(i) = x(i) / sumx
                                Vx2(i) = ((L1 + 0.0000000001) * Vx1(i) - (V + 0.0000000001) * Vy(i)) / L2
                                If L2 = 0.0# Then Vx2(i) = 0
                            Next
                    End Select

                    For i = 0 To n
                        If Vx2(i) <= 0 Then Vx2(i) = 1.0E-20
                    Next

                    soma_x2 = 0
                    For i = 0 To n
                        soma_x2 += Vx2(i)
                    Next
                    For i = 0 To n
                        Vx2(i) /= soma_x2
                    Next

                    If My.Settings.EnableParallelProcessing Then
                        My.MyApplication.IsRunningParallelTasks = True
                        If My.Settings.EnableGPUProcessing Then
                            My.MyApplication.gpu.EnableMultithreading()
                        End If
                        Try
                            Dim task1 As Task = New Task(Sub()
                                                             fcv = proppack.DW_CalcFugCoeff(Vy, Tf, Pf, State.Vapor)
                                                         End Sub)
                            Dim task2 As Task = New Task(Sub()
                                                             fcl = proppack.DW_CalcFugCoeff(Vx1, Tf, Pf, State.Liquid)
                                                         End Sub)
                            Dim task3 As Task = New Task(Sub()
                                                             fcl2 = proppack.DW_CalcFugCoeff(Vx2, Tf, Pf, State.Liquid)
                                                         End Sub)
                            task1.Start()
                            task2.Start()
                            task3.Start()
                            Task.WaitAll(task1, task2, task3)
                        Catch ae As AggregateException
                            For Each ex As Exception In ae.InnerExceptions
                                Throw ex
                            Next
                        Finally
                            If My.Settings.EnableGPUProcessing Then
                                My.MyApplication.gpu.DisableMultithreading()
                                My.MyApplication.gpu.FreeAll()
                            End If
                        End Try
                        My.MyApplication.IsRunningParallelTasks = False
                    Else
                        fcv = proppack.DW_CalcFugCoeff(Vy, Tf, Pf, State.Vapor)
                        fcl = proppack.DW_CalcFugCoeff(Vx1, Tf, Pf, State.Liquid)
                        fcl2 = proppack.DW_CalcFugCoeff(Vx2, Tf, Pf, State.Liquid)
                    End If

                    Select Case objfunc
                        Case ObjFuncType.BubblePointP, ObjFuncType.BubblePointT
                            For i = 0 To n
                                If Vy(i) <> 0 And Vx1(i) <> 0 Then g(i) = Log(fcl(i) * Vx1(i)) - Log(fcv(i) * Vy(i))
                            Next
                        Case ObjFuncType.DewPointP, ObjFuncType.DewPointT
                            For i = 0 To n
                                If Vy(i) <> 0 And Vx1(i) <> 0 Then g(i) = Log(fcv(i) * Vy(i)) - Log(fcl(i) * Vx1(i))
                            Next
                    End Select
                    Dim xg1, xg2 As Double()
                    xg1 = x.Clone
                    xg2 = x.Clone
                    xg2(n + 1) *= 1.01
                    g(n + 1) = (FunctionValue(xg2) - FunctionValue(xg1)) / (0.01 * xg1(n + 1))

                    Return g

            End Select


        End Function

        Private Function FunctionHessian(ByVal x() As Double) As Double()

            Dim epsilon As Double = 0.01

            Dim f2() As Double = Nothing
            Dim f3() As Double = Nothing
            Dim h((x.Length) * (x.Length) - 1), x2(x.Length - 1), x3(x.Length - 1) As Double
            Dim m As Integer

            m = 0
            For i = 0 To x.Length - 1
                For j = 0 To x.Length - 1
                    If i <> j Then
                        x2(j) = x(j)
                        x3(j) = x(j)
                    Else
                        x2(j) = x(j) * (1 + epsilon)
                        x3(j) = x(j) * (1 - epsilon)
                    End If
                Next
                If My.Settings.EnableParallelProcessing Then
                    My.MyApplication.IsRunningParallelTasks = True
                    If My.Settings.EnableGPUProcessing Then
                        My.MyApplication.gpu.EnableMultithreading()
                    End If
                    Try
                        Dim task1 As Task = New Task(Sub()
                                                         f2 = FunctionGradient(x2)
                                                     End Sub)
                        Dim task2 As Task = New Task(Sub()
                                                         f3 = FunctionGradient(x3)
                                                     End Sub)
                        task1.Start()
                        task2.Start()
                        Task.WaitAll(task1, task2)
                    Catch ae As AggregateException
                        For Each ex As Exception In ae.InnerExceptions
                            Throw ex
                        Next
                    Finally
                        If My.Settings.EnableGPUProcessing Then
                            My.MyApplication.gpu.DisableMultithreading()
                            My.MyApplication.gpu.FreeAll()
                        End If
                    End Try
                    My.MyApplication.IsRunningParallelTasks = False
                Else
                    f2 = FunctionGradient(x2)
                    f3 = FunctionGradient(x3)
                End If
                For k = 0 To x.Length - 1
                    h(m) = (f2(k) - f3(k)) / (x2(i) - x3(i))
                    If Double.IsNaN(h(m)) Then h(m) = 0.0#
                    m += 1
                Next
            Next

            Return h

        End Function

        'IPOPT

        Public Function eval_f(ByVal n As Integer, ByVal x As Double(), ByVal new_x As Boolean, ByRef obj_value As Double) As Boolean
            Dim fval As Double = FunctionValue(x)
            obj_value = fval
            Return True
        End Function

        Public Function eval_grad_f(ByVal n As Integer, ByVal x As Double(), ByVal new_x As Boolean, ByRef grad_f As Double()) As Boolean
            Dim g As Double() = FunctionGradient(x)
            grad_f = g
            Return True
        End Function

        Public Function eval_g(ByVal n As Integer, ByVal x As Double(), ByVal new_x As Boolean, ByVal m As Integer, ByRef g As Double()) As Boolean
            For i = 0 To m - 1
                g(i) = fi(i) * F - x(i) - x(i + m)
            Next
            Return True
        End Function

        Public Function eval_jac_g(ByVal n As Integer, ByVal x As Double(), ByVal new_x As Boolean, ByVal m As Integer, ByVal nele_jac As Integer, ByRef iRow As Integer(), _
         ByRef jCol As Integer(), ByRef values As Double()) As Boolean

            If values Is Nothing Then

                Dim row(nele_jac - 1), col(nele_jac - 1) As Integer

                k = 0
                For i = 0 To m - 1
                    row(i) = i
                    row(i + m) = i
                    col(i) = i
                    col(i + m) = i + m
                Next

                iRow = row
                jCol = col

            Else

                Dim res(nele_jac - 1) As Double

                For i = 0 To nele_jac - 1
                    res(i) = -1
                Next

                values = res

            End If
            Return True
        End Function

        Public Function eval_h(ByVal n As Integer, ByVal x As Double(), ByVal new_x As Boolean, ByVal obj_factor As Double, ByVal m As Integer, ByVal lambda As Double(), _
         ByVal new_lambda As Boolean, ByVal nele_hess As Integer, ByRef iRow As Integer(), ByRef jCol As Integer(), ByRef values As Double()) As Boolean

            If values Is Nothing Then

                Dim row(nele_hess - 1), col(nele_hess - 1) As Integer

                'k = 0
                'For i = 0 To n - 1
                '    For j = 0 To n - 1
                '        row(k) = i
                '        col(k) = j
                '        k += 1
                '    Next
                'Next

                iRow = row
                jCol = col

            Else

                values = FunctionHessian(x)

            End If

            Return True

        End Function

        Public Function intermediate(ByVal alg_mod As IpoptAlgorithmMode, ByVal iter_count As Integer, ByVal obj_value As Double, _
                                     ByVal inf_pr As Double, ByVal inf_du As Double, ByVal mu As Double, _
                                     ByVal d_norm As Double, ByVal regularization_size As Double, ByVal alpha_du As Double, _
                                     ByVal alpha_pr As Double, ByVal ls_trials As Integer) As Boolean
            objval0 = objval
            objval = obj_value
            Return True
        End Function

    End Class

End Namespace
