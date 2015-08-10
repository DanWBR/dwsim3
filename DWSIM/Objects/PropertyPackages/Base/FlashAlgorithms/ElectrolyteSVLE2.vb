'    Flash Algorithm for Electrolyte solutions (Gibbs-min based)
'    Copyright 2013-2015 Daniel Wagner O. de Medeiros
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
Imports DWSIM.DWSIM.Flowsheet.FlowsheetSolver
Imports Cureos.Numerics

Namespace DWSIM.SimulationObjects.PropertyPackages.Auxiliary.FlashAlgorithms

    <System.Serializable()> Public Class ElectrolyteSVLE2

        Public proppack As PropertyPackage

        Dim tmpx As Double(), tmpdx As Double()

        Dim Hf, Hl, Hv, Hs, Tf, Pf, P0, Ninerts, Winerts, E(,) As Double
        Dim r, c, els, comps As Integer

        Dim n, nr, ecount As Integer
        Dim etol As Double = 0.01
        Dim itol As Double = 0.01
        Dim maxit_i As Integer = 1000
        Dim maxit_e As Integer = 1000

        Dim Vxl(n), Vxs(n), Vxv(n), Vv(n), Vl(n), Vs(n), fi(n), Vp(n) As Double

        Dim F, L, S, V As Double

        Dim soma_x, soma_s, soma_y As Double

        Public Property ReactionSet As String = "DefaultSet"
        Public Property Reactions As List(Of String)
        Public Property ReactionExtents As Dictionary(Of String, Double)
        Public Property ComponentIDs As List(Of String)
        Public Property CompoundProperties As List(Of ConstantProperties)
        Public Property ComponentConversions As Dictionary(Of String, Double)

        Public Property MaximumIterations As Integer = 1000
        Public Property Tolerance As Double = 1.0E-20

        Public Property CalculateChemicalEquilibria As Boolean = True

        Private Vx0 As Double()

        Private LoopVarF, LoopVarX As Double, LoopVarVz As Double(), LoopVarState As State

        Public Sub WriteDebugInfo(text As String)

            Select Case My.Settings.DebugLevel
                Case 0
                    'do nothing
                Case 1
                    Console.WriteLine(text)
                Case 2
            End Select

        End Sub

        Public Function Flash_PT(Vz As Array, T As Double, P As Double) As Dictionary(Of String, Object)

            'This flash algorithm is for Electrolye/Salt systems with Water as the single solvent.
            'The vapor and solid phases are considered to be ideal.
            'Chemical equilibria is calculated using the reactions enabled in the default reaction set.

            Dim d1, d2 As Date, dt As TimeSpan
            d1 = Date.Now

            n = CompoundProperties.Count - 1

            If Me.Reactions Is Nothing Then Me.Reactions = New List(Of String)
         
            Dim form As FormFlowsheet = proppack.CurrentMaterialStream.FlowSheet

            Me.Reactions.Clear()
      
            Dim i, j As Integer

            P0 = 101325

            Dim rxn As Reaction

            'check active reactions (equilibrium only) in the reaction set
            For Each rxnsb As ReactionSetBase In form.Options.ReactionSets(Me.ReactionSet).Reactions.Values
                If form.Options.Reactions(rxnsb.ReactionID).ReactionType = ReactionType.Equilibrium And rxnsb.IsActive Then
                    Me.Reactions.Add(rxnsb.ReactionID)
                    rxn = form.Options.Reactions(rxnsb.ReactionID)
                    'equilibrium constant calculation
                    Select Case rxn.KExprType
                        Case Reaction.KOpt.Constant
                            'rxn.ConstantKeqValue = rxn.ConstantKeqValue
                        Case Reaction.KOpt.Expression
                            If rxn.ExpContext Is Nothing Then
                                rxn.ExpContext = New Ciloci.Flee.ExpressionContext
                                With rxn.ExpContext
                                    .Imports.AddType(GetType(System.Math))
                                End With
                            End If
                            rxn.ExpContext.Variables.Add("T", T)
                            rxn.Expr = rxn.ExpContext.CompileGeneric(Of Double)(rxn.Expression)
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
                            Dim DelG_RT = proppack.AUX_DELGig_RT(298.15, T, id, stcoef, bcidx, True)
                            rxn.ConstantKeqValue = Exp(-DelG_RT)
                    End Select
                End If
            Next

            nr = Me.Reactions.Count

            Dim rx As Reaction

            If Me.ComponentConversions Is Nothing Then Me.ComponentConversions = New Dictionary(Of String, Double)
            If Me.ComponentIDs Is Nothing Then Me.ComponentIDs = New List(Of String)

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

            ReDim Vxv(n), Vxl(n), Vxs(n)

            Dim activcoeff(n) As Double
            
            'Vnf = feed molar amounts (considering 1 mol of feed)
            'Vnl = liquid phase molar amounts
            'Vnv = vapor phase molar amounts
            'Vns = solid phase molar amounts
            'Vxl = liquid phase molar fractions
            'Vxv = vapor phase molar fractions
            'Vxs = solid phase molar fractions
            'V, S, L = phase molar amounts (F = 1 = V + S + L)
            Dim sumN As Double = 0

            'get water index in the array.

            Dim wid As Integer = CompoundProperties.IndexOf((From c As ConstantProperties In CompoundProperties Select c Where c.Name = "Water").SingleOrDefault)

            Dim initval(n) As Double
            Dim lconstr(n) As Double
            Dim uconstr(n) As Double
            Dim finalval(n) As Double

            F = 1000.0#

            Pf = P
            Tf = T

            fi = Vz.Clone()

            Dim initval2(2 * n + 1) As Double
            Dim lconstr2(2 * n + 1) As Double
            Dim uconstr2(2 * n + 1) As Double
            Dim finalval2(2 * n + 1) As Double
            Dim glow(n + 1), gup(n + 1), g(n + 1) As Double

            For i = 0 To n
                If CompoundProperties(i).IsIon Then
                    initval2(i) = Vz(i) * F
                    uconstr2(i) = F
                Else
                    initval2(i) = Vz(i) * F
                    uconstr2(i) = Vz(i) * F
                End If
                lconstr2(i) = 0.0#
                glow(i) = 0.0#
                gup(i) = F
            Next
            For i = n + 1 To 2 * n + 1
                lconstr2(i) = 0.0#
                If CompoundProperties(i - n - 1).IsSalt Then
                    initval2(i) = 0.0#
                    uconstr2(i) = Vz(i - n - 1) * F
                Else
                    initval2(i) = 0.0#
                    uconstr2(i) = 0.0#
                End If
            Next
            glow(n + 1) = 0.0#
            gup(n + 1) = F

            ecount = 0

            Dim obj As Double
            Dim status As IpoptReturnCode

            Using problem As New Ipopt(initval2.Length, lconstr2, uconstr2, n + 2, glow, gup, (n + 1) * 2, (n + 1) * 2 + 1, _
                    AddressOf eval_f, AddressOf eval_g, _
                    AddressOf eval_grad_f, AddressOf eval_jac_g, AddressOf eval_h)
                problem.AddOption("tol", Tolerance)
                problem.AddOption("max_iter", MaximumIterations)
                problem.AddOption("mu_strategy", "adaptive")
                'problem.AddOption("mehrotra_algorithm", "yes")
                problem.AddOption("hessian_approximation", "limited-memory")
                'problem.SetIntermediateCallback(AddressOf intermediate)
                'solve the problem 
                status = problem.SolveProblem(initval2, obj, g, Nothing, Nothing, Nothing)
            End Using

            FunctionValue(initval2)

            'check if maximum iterations exceeded.
            If status = IpoptReturnCode.Maximum_Iterations_Exceeded Then
                WriteDebugInfo("PT Flash [GM]: Maximum iterations exceeded. Recalculating with Nested-Loops PT-Flash...")
            End If

            d2 = Date.Now

            dt = d2 - d1

            WriteDebugInfo("PT Flash [GM]: Converged in " & ecount & " iterations. Status: " & [Enum].GetName(GetType(IpoptReturnCode), status) & ". Time taken: " & dt.TotalMilliseconds & " ms")

out:        'return flash calculation results.

            Dim results As New Dictionary(Of String, Object)

            results.Add("MixtureMoleFlows", Vz)
            results.Add("VaporPhaseMoleFraction", V / F)
            results.Add("LiquidPhaseMoleFraction", L / F)
            results.Add("SolidPhaseMoleFraction", S / F)
            results.Add("VaporPhaseMolarComposition", Vxv)
            results.Add("LiquidPhaseMolarComposition", Vxl)
            results.Add("SolidPhaseMolarComposition", Vxs)
            results.Add("LiquidPhaseActivityCoefficients", activcoeff)
            results.Add("MoleSum", 1.0#)

            Return results

        End Function

        Public Function Flash_PH(ByVal Vz As Double(), ByVal P As Double, ByVal H As Double, ByVal Tref As Double, Optional ByVal ReuseKI As Boolean = False, Optional ByVal PrevKi As Double() = Nothing) As Object

            Dim doparallel As Boolean = My.Settings.EnableParallelProcessing

            Dim Vn(1) As String, Vx(1), Vy(1), Vx_ant(1), Vy_ant(1), Vp(1), Ki(1), Ki_ant(1), fi(1) As Double
            Dim j, n, ecount As Integer
            Dim d1, d2 As Date, dt As TimeSpan
            Dim L, V, T, Pf As Double

            d1 = Date.Now

            n = UBound(Vz)

            Hf = H
            Pf = P

            ReDim Vn(n), Vx(n), Vy(n), Vx_ant(n), Vy_ant(n), Vp(n), Ki(n), fi(n)

            fi = Vz.Clone

            Dim maxitINT As Integer = CInt(proppack.Parameters("PP_PHFMII"))
            Dim maxitEXT As Integer = CInt(proppack.Parameters("PP_PHFMEI"))
            Dim tolINT As Double = CDbl(proppack.Parameters("PP_PHFILT"))
            Dim tolEXT As Double = CDbl(proppack.Parameters("PP_PHFELT"))

            Dim Tmin, Tmax, epsilon(4), maxDT As Double

            Tmax = 2000.0#
            Tmin = 50.0#
            maxDT = 5.0#

            epsilon(0) = 0.1
            epsilon(1) = 0.01
            epsilon(2) = 0.001
            epsilon(3) = 0.0001
            epsilon(4) = 0.00001

            Dim fx, fx2, dfdx, x1, dx As Double

            Dim cnt As Integer

            If Tref = 0.0# Then Tref = 298.15

            For j = 0 To 4

                cnt = 0
                x1 = Tref

                Do

                    fx = Herror(x1, P, Vz)
                    fx2 = Herror(x1 + epsilon(j), P, Vz)

                    If Abs(fx) <= tolEXT Then Exit Do

                    dfdx = (fx2 - fx) / epsilon(j)

                    If dfdx = 0.0# Then
                        fx = 0.0#
                        Exit Do
                    End If

                    dx = fx / dfdx

                    If Abs(dx) > maxDT Then dx = maxDT * Sign(dx)

                    x1 = x1 - dx

                    cnt += 1

                Loop Until cnt > maxitEXT Or Double.IsNaN(x1) Or x1 < 0.0#

                T = x1

                If Not Double.IsNaN(T) And Not Double.IsInfinity(T) And Not cnt > maxitEXT Then
                    If T > Tmin And T < Tmax Then Exit For
                End If

            Next

            If Double.IsNaN(T) Or T <= Tmin Or T >= Tmax Or cnt > maxitEXT Or Abs(fx) > tolEXT Then Throw New Exception("PH Flash [Electrolyte]: Invalid result: Temperature did not converge.")

            Dim tmp As Object = Flash_PT(Vz, T, P)

            Dim S, Vs(), sumN As Double

            sumN = tmp("MoleSum")
            L = tmp("LiquidPhaseMoleFraction")
            V = tmp("VaporPhaseMoleFraction")
            S = tmp("SolidPhaseMoleFraction")
            Vx = tmp("LiquidPhaseMolarComposition")
            Vy = tmp("VaporPhaseMolarComposition")
            Vs = tmp("SolidPhaseMolarComposition")

            d2 = Date.Now

            dt = d2 - d1

            WriteDebugInfo("PH Flash [Electrolyte]: Converged in " & ecount & " iterations. Time taken: " & dt.TotalMilliseconds & " ms.")

            'return flash calculation results.

            Dim results As New Dictionary(Of String, Object)

            results.Add("MixtureMoleFlows", Vz)
            results.Add("VaporPhaseMoleFraction", V)
            results.Add("LiquidPhaseMoleFraction", L)
            results.Add("SolidPhaseMoleFraction", S)
            results.Add("VaporPhaseMolarComposition", Vy)
            results.Add("LiquidPhaseMolarComposition", Vx)
            results.Add("SolidPhaseMolarComposition", Vs)
            results.Add("MoleSum", sumN)
            results.Add("Temperature", T)
            results.Add("LiquidPhaseActivityCoefficients", tmp("LiquidPhaseActivityCoefficients"))

            Return results

        End Function

        Function Herror(ByVal X As Double, ByVal P As Double, ByVal Vz() As Double) As Double
            Return OBJ_FUNC_PH_FLASH(X, P, Vz)
        End Function

        Public Function Flash_PH0(ByVal Vz As Array, ByVal P As Double, ByVal H As Double, ByVal Tref As Double) As Dictionary(Of String, Object)

            Dim n As Integer
            Dim d1, d2 As Date, dt As TimeSpan
            Dim L, V, T, S, Pf, sumN As Double

            d1 = Date.Now

            n = UBound(Vz)

            Hf = H
            Pf = P

            Dim Vx(n), Vx2(n), Vy(n), Vs(n), Vz0(n) As Double

            Dim maxitINT As Integer = Me.MaximumIterations
            Dim maxitEXT As Integer = Me.MaximumIterations
            Dim tolINT As Double = 0.0001
            Dim tolEXT As Double = 0.0001

            Dim hl, hv, Tsat, Tsat_ant, Psat, xv, xv_ant, xl, wac, wx, deltaT, sumnw As Double

            Dim wid As Integer = CompoundProperties.IndexOf((From c As ConstantProperties In CompoundProperties Select c Where c.Name = "Water").SingleOrDefault)

            Dim tmp As Dictionary(Of String, Object) = Nothing

            'calculate water saturation temperature.

            Tsat = proppack.AUX_TSATi(P, wid)

            'calculate activity coefficient

            deltaT = 1

            If Vz(wid) <> 0.0# Then
                'Do
                '    Tsat = Tsat - deltaT
                '    Try
                '        tmp = Flash_PT(Vz, Tsat, P)
                '        wac = tmp("LiquidPhaseActivityCoefficients")(wid)
                '        wx = tmp("LiquidPhaseMolarComposition")(wid)
                '    Catch ex As Exception
                '        wx = 0.0#
                '    End Try
                'Loop Until wx > 0.0#

                Psat = P '/ (wx * wac)
                Tsat = 300 'proppack.AUX_TSATi(Psat, wid)
            End If

            Dim icount As Integer = 0

            Do

                hl = proppack.DW_CalcEnthalpy(Vz, Tsat, P, State.Liquid)
                hv = proppack.DW_CalcEnthalpy(Vz, Tsat, P, State.Vapor)

                xv_ant = xv

                If H <= hl Then
                    xv = 0
                    LoopVarState = State.Liquid
                ElseIf H >= hv Then
                    xv = 1
                    LoopVarState = State.Vapor
                Else
                    xv = (H - hl) / (hv - hl)
                End If
                xl = 1 - xv

                If xv <> 0.0# And xv <> 1.0# Then
                    T = Tsat
                Else
                    LoopVarF = H
                    LoopVarX = P
                    LoopVarVz = Vz
                    Dim x0, x, fx, dfdx As Double, k As Integer
                    x = Tsat - 5
                    k = 0
                    Do
                        fx = EnthalpyTx(x, Nothing)
                        If Abs(fx) < 0.0001 Then Exit Do
                        dfdx = (EnthalpyTx(x + 0.1, Nothing) - EnthalpyTx(x, Nothing)) / 0.1
                        x0 = x
                        x -= fx / dfdx
                        k += 1
                    Loop Until k >= 100 Or Double.IsNaN(x)
                    If Double.IsNaN(x) Then Throw New Exception("Temperature loop did not converge. Calculation aborted.")
                    T = x
                End If

                Vz0 = Vz.Clone

                V = Vz0(wid) * xv
                Vz(wid) = Vz0(wid) * (1 - xv)
                sumnw = 0.0#
                For i = 0 To n
                    If i <> wid Then
                        sumnw += Vz0(i)
                    End If
                Next
                For i = 0 To n
                    If i <> wid Then
                        Vz(i) = Vz0(i) / sumnw * (1 - Vz(wid))
                    End If
                Next

                tmp = Flash_PT(Vz, T, P)

                L = tmp("LiquidPhaseMoleFraction") * (1 - V)
                S = tmp("SolidPhaseMoleFraction") * (1 - V)
                Vx = tmp("LiquidPhaseMolarComposition")
                Vy = proppack.RET_NullVector()
                Vy(wid) = 1.0#
                Vs = tmp("SolidPhaseMolarComposition")
                sumN = tmp("MoleSum")
                Vz = Vz0

                If Vz(wid) <> 0.0# And xv <> 0.0# And xv <> 1.0# Then
                    wac = tmp("LiquidPhaseActivityCoefficients")(wid)
                    wx = tmp("LiquidPhaseMolarComposition")(wid)
                    'If wx < 0.6 Then Throw New Exception("Water mole fraction in liquid phase is less than 0.6. Calculation aborted.")
                    Psat = P / (wx * wac)
                    Tsat_ant = Tsat
                    Tsat = proppack.AUX_TSATi(Psat, wid)
                End If

                'If wx < 0.6 Then Throw New Exception("Water mole fraction in liquid phase is less than 0.6. Calculation aborted.")

                If Double.IsNaN(Tsat) Then Throw New Exception("Temperature loop did not converge. Calculation aborted.")

                'If Tsat < 273.15 Then Throw New Exception("Temperature loop did not converge. Calculation aborted.")

                If icount > maxitINT Then Throw New Exception("Temperature loop did not converge. Maximum iterations reached.")

                icount += 1

            Loop Until Abs(xv - xv_ant) < 0.001

            d2 = Date.Now

            dt = d2 - d1

            WriteDebugInfo("PH Flash [Electrolyte]: Converged successfully. Time taken: " & dt.TotalMilliseconds & " ms.")

            'return flash calculation results.

            Dim results As New Dictionary(Of String, Object)

            results.Add("MixtureMoleFlows", Vz)
            results.Add("VaporPhaseMoleFraction", V)
            results.Add("LiquidPhaseMoleFraction", L)
            results.Add("SolidPhaseMoleFraction", S)
            results.Add("VaporPhaseMolarComposition", Vy)
            results.Add("LiquidPhaseMolarComposition", Vx)
            results.Add("SolidPhaseMolarComposition", Vs)
            results.Add("MoleSum", sumN)
            results.Add("Temperature", T)
            results.Add("LiquidPhaseActivityCoefficients", tmp("LiquidPhaseActivityCoefficients"))

            Return results

        End Function

        Function OBJ_FUNC_PH_FLASH(ByVal T As Double, ByVal P As Double, ByVal Vz As Object) As Double

            Dim tmp As Dictionary(Of String, Object) = Flash_PT(Vz, T, P)

            Dim L, V, S, Vx(), Vy(), Vs(), sumN, _Hv, _Hl, _Hs As Double

            Dim n = UBound(Vz)

            sumN = tmp("MoleSum")
            L = tmp("LiquidPhaseMoleFraction")
            V = tmp("VaporPhaseMoleFraction")
            S = tmp("SolidPhaseMoleFraction")
            Vx = tmp("LiquidPhaseMolarComposition")
            Vy = tmp("VaporPhaseMolarComposition")
            Vs = tmp("SolidPhaseMolarComposition")
            'Vz = tmp("MixtureMoleFlows")

            _Hv = 0
            _Hl = 0
            _Hs = 0

            Dim mmg, mml, mms As Double
            If V > 0.0# Then _Hv = proppack.DW_CalcEnthalpy(Vy, T, P, State.Vapor)
            If L > 0.0# Then _Hl = proppack.DW_CalcEnthalpy(Vx, T, P, State.Liquid)
            If S > 0.0# Then _Hs = proppack.DW_CalcSolidEnthalpy(T, Vs, CompoundProperties)
            mmg = proppack.AUX_MMM(Vy)
            mml = proppack.AUX_MMM(Vx)
            mms = proppack.AUX_MMM(Vs)

            Dim herr As Double = Hf - ((mmg * V / (mmg * V + mml * L + mms * S)) * _Hv + (mml * L / (mmg * V + mml * L + mms * S)) * _Hl + (mms * S / (mmg * V + mml * L + mms * S)) * _Hs)
            OBJ_FUNC_PH_FLASH = herr

            WriteDebugInfo("PH Flash [Electrolyte]: Current T = " & T & ", Current H Error = " & herr)

        End Function

        Private Function EnthalpyTx(ByVal x As Double, ByVal otherargs As Object) As Double

            Dim er As Double = LoopVarF - proppack.DW_CalcEnthalpy(LoopVarVz, x, LoopVarX, LoopVarState)
            Return er

        End Function

        Public Function Flash_PV(ByVal Vz As Array, ByVal P As Double, ByVal V As Double, ByVal Tref As Double) As Dictionary(Of String, Object)

            Dim n As Integer
            Dim d1, d2 As Date, dt As TimeSpan
            Dim L, T, S, Pf, sumN As Double

            d1 = Date.Now

            n = UBound(Vz)

            Pf = P

            Dim Vx(n), Vx2(n), Vy(n), Vs(n), Vz0(n) As Double

            Dim maxitINT As Integer = Me.MaximumIterations
            Dim maxitEXT As Integer = Me.MaximumIterations
            Dim tolINT As Double = 0.0001
            Dim tolEXT As Double = 0.0001

            Dim brentsolverT As New BrentOpt.Brent
            brentsolverT.DefineFuncDelegate(AddressOf EnthalpyTx)

            Dim Tsat, Tsat_ant, Psat, xv, xl, wac, wx, deltaT, sumnw As Double

            Dim wid As Integer = CompoundProperties.IndexOf((From c As ConstantProperties In CompoundProperties Select c Where c.Name = "Water").SingleOrDefault)

            Dim tmp As Dictionary(Of String, Object) = Nothing

            'calculate water saturation temperature.

            Tsat = proppack.AUX_TSATi(P, wid)

            'calculate activity coefficient

            deltaT = 1

            xv = V / Vz0(wid)
            xl = 1 - xv

            Dim icount As Integer = 0

            If Vz(wid) <> 0.0# Then
                Do
                    Tsat = Tsat - deltaT
                    tmp = Flash_PT(Vz, Tsat, P)
                    wac = tmp("LiquidPhaseActivityCoefficients")(wid)
                    wx = tmp("LiquidPhaseMolarComposition")(wid)
                Loop Until wx > 0.0#

                Psat = P / (wx * wac)
                Tsat = proppack.AUX_TSATi(Psat, wid)
            End If

            Do

                T = Tsat

                Vz0 = Vz.Clone

                Vz(wid) = Vz0(wid) * (1 - xv)
                sumnw = 0.0#
                For i = 0 To n
                    If i <> wid Then
                        sumnw += Vz0(i)
                    End If
                Next
                For i = 0 To n
                    If i <> wid Then
                        Vz(i) = Vz0(i) / sumnw * (1 - Vz(wid))
                    End If
                Next

                tmp = Flash_PT(Vz, T, P)

                L = tmp("LiquidPhaseMoleFraction") * (1 - V)
                S = tmp("SolidPhaseMoleFraction") * (1 - V)
                Vx = tmp("LiquidPhaseMolarComposition")
                Vy = proppack.RET_NullVector()
                Vy(wid) = 1.0#
                Vs = tmp("SolidPhaseMolarComposition")
                sumN = tmp("MoleSum")
                Vz = Vz0

                If Vz(wid) <> 0.0# And xv <> 0.0# And xv <> 1.0# Then

                    wac = tmp("LiquidPhaseActivityCoefficients")(wid)
                    wx = tmp("LiquidPhaseMolarComposition")(wid)

                    If wx < 0.6 Then Throw New Exception("Water mole fraction in liquid phase is less than 0.6. Calculation aborted.")

                    Psat = P / (wx * wac)
                    Tsat_ant = Tsat
                    Tsat = proppack.AUX_TSATi(Psat, wid)

                End If

                If wx < 0.6 Then Throw New Exception("Water mole fraction in liquid phase is less than 0.6. Calculation aborted.")

                If Double.IsNaN(Tsat) Then Throw New Exception("Temperature loop did not converge. Calculation aborted.")

                If Tsat < 273.15 Then Throw New Exception("Temperature loop did not converge. Calculation aborted.")

            Loop Until Abs(Tsat - Tsat_ant) < 0.01

            d2 = Date.Now

            dt = d2 - d1

            WriteDebugInfo("PH Flash [Electrolyte]: Converged successfully. Time taken: " & dt.TotalMilliseconds & " ms.")

            'return flash calculation results.

            Dim results As New Dictionary(Of String, Object)

            results.Add("MixtureMoleFlows", Vz)
            results.Add("VaporPhaseMoleFraction", V)
            results.Add("LiquidPhaseMoleFraction", L)
            results.Add("SolidPhaseMoleFraction", S)
            results.Add("VaporPhaseMolarComposition", Vy)
            results.Add("LiquidPhaseMolarComposition", Vx)
            results.Add("SolidPhaseMolarComposition", Vs)
            results.Add("MoleSum", sumN)
            results.Add("Temperature", T)
            results.Add("LiquidPhaseActivityCoefficients", tmp("LiquidPhaseActivityCoefficients"))

            Return results

        End Function

        'Function Values

        Private Function FunctionValue(ByVal x() As Double) As Double

            CheckCalculatorStatus()

            Dim fcv(n), fcl(n), fcs(n) As Double

            soma_x = 0
            For i = 0 To x.Length - n - 2
                soma_x += x(i)
            Next
            soma_s = 0
            For i = x.Length - n - 1 To x.Length - 1
                soma_s += x(i)
            Next
            L = soma_x
            S = soma_s
            V = F - L - S

            For i = 0 To n
                If CompoundProperties(i).IsIon Then
                    Vxs(i) = 0.0#
                    Vxv(i) = 0.0#
                    If L <> 0.0# Then Vxl(i) = (x(i) / L) Else Vxl(i) = 0.0#
                ElseIf CompoundProperties(i).IsSalt Then
                    Vxv(i) = 0.0#
                    If L <> 0.0# Then Vxl(i) = (x(i) / L) Else Vxl(i) = 0.0#
                    If S <> 0.0# Then Vxs(i) = (x(i + n + 1) / S) Else Vxs(i) = 0.0#
                Else
                    If L <> 0.0# Then Vxl(i) = (x(i) / L) Else Vxl(i) = 0.0#
                    If S <> 0.0# Then Vxs(i) = (x(i + n + 1) / S) Else Vxs(i) = 0.0#
                    If V <> 0.0# Then Vxv(i) = ((fi(i) * F - Vxl(i) * L - Vxs(i) * S) / V) Else Vxv(i) = 0.0#
                End If
                If Vxv(i) < 0.0# Then Vxv(i) = 1.0E-20
                If Vxl(i) < 0.0# Then Vxl(i) = 1.0E-20
                If Vxs(i) < 0.0# Then Vxs(i) = 1.0E-20
            Next

            If V > 0.0# And Vxv.Sum <> 0.0# Then
                soma_y = 0
                For i = 0 To n
                    soma_y += Vxv(i)
                Next
                For i = 0 To n
                    Vxv(i) /= soma_y
                Next
            End If

            fcv = proppack.DW_CalcFugCoeff(Vxv, Tf, Pf, State.Vapor)
            fcl = proppack.DW_CalcFugCoeff(Vxl, Tf, Pf, State.Liquid)
            fcs = proppack.DW_CalcFugCoeff(Vxs, Tf, Pf, State.Solid)

            Dim Gv, Gl, Gs, Gm, Rx As Double

            Gv = 0.0#
            Gl = 0.0#
            Gs = 0.0#
            For i = 0 To n
                If Vxv(i) <> 0.0# Then Gv += Vxv(i) * V * Log(fcv(i) * Vxv(i))
                If Vxl(i) <> 0.0# Then Gl += Vxl(i) * L * Log(fcl(i) * Vxl(i))
                If Vxs(i) <> 0.0# Then Gs += Vxs(i) * S * Log(fcs(i) * Vxs(i))
            Next

            Gm = Gv + Gl + Gs

            Rx = CheckEquilibrium().Sum

            WriteDebugInfo("[GM] V = " & Format(V / 1000, "N4") & ", L = " & Format(L / 1000, "N4") & ", S= " & Format(S / 1000, "N4") & " / GE = " & Format(Gm * 8.314 * Tf / 1000, "N2") & " kJ/kmol")

            ecount += 1

            Return Gm + Rx

        End Function

      Private Function FunctionGradient(ByVal x() As Double) As Double()

            Dim g(x.Length - 1) As Double

            Dim epsilon As Double = 0.1

            Dim f2(x.Length - 1), f3(x.Length - 1) As Double
            Dim x2(x.Length - 1), x3(x.Length - 1) As Double
            Dim i, j As Integer

            For i = 0 To x.Length - 1
                For j = 0 To x.Length - 1
                    x2(j) = x(j)
                    x3(j) = x(j)
                Next
                x2(i) = x(i) + epsilon
                x3(i) = x(i) - epsilon
                f2(i) = FunctionValue(x2)
                f3(i) = FunctionValue(x3)
                g(i) = (f2(i) - f3(i)) / (x2(i) - x3(i))
                If Double.IsNaN(g(i)) Then g(i) = 0.0#
            Next

            Return g

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
                f2 = FunctionGradient(x2)
                f3 = FunctionGradient(x3)
                For k2 = 0 To x.Length - 1
                    h(m) = (f2(k2) - f3(k)) / (x2(i) - x3(i))
                    If Double.IsNaN(h(m)) Then h(m) = 0.0#
                    m += 1
                Next
            Next

            Return h

        End Function

        Private Function CheckEquilibrium() As Double()

            Dim i, nc As Integer

            nc = Me.CompoundProperties.Count - 1

            'calculate molality considering 1 mol of mixture.

            Dim wtotal As Double = 0
            Dim mtotal As Double = 0
            Dim molality(nc) As Double

            i = 0
            Do
                If CompoundProperties(i).Name = "Water" Then
                    wtotal += Vxl(i) * CompoundProperties(i).Molar_Weight / 1000
                End If
                mtotal += Vxl(i)
                i += 1
            Loop Until i = nc + 1

            Dim Xsolv As Double = 1

            i = 0
            Do
                molality(i) = Vxl(i) / wtotal
                i += 1
            Loop Until i = nc + 1

            Dim activcoeff(nc) As Double

            If TypeOf proppack Is ExUNIQUACPropertyPackage Then
                activcoeff = CType(proppack, ExUNIQUACPropertyPackage).m_uni.GAMMA_MR(Tf, Vxl, CompoundProperties)
            ElseIf TypeOf proppack Is LIQUAC2PropertyPackage Then
                activcoeff = CType(proppack, LIQUAC2PropertyPackage).m_uni.GAMMA_MR(Tf, Vxl, CompoundProperties)
            End If

            Dim CP(nc) As Double
            Dim f(nr - 1) As Double
            Dim prod(nr - 1) As Double

            For i = 0 To nc
                If CompoundProperties(i).IsIon Then
                    CP(i) = molality(i) * activcoeff(i)
                ElseIf CompoundProperties(i).IsSalt Then
                    CP(i) = 1.0#
                Else
                    CP(i) = Vxl(i) * activcoeff(i)
                End If
            Next

            For i = 0 To Me.Reactions.Count - 1
                prod(i) = 1
                For Each s As String In Me.ComponentIDs
                    With proppack.CurrentMaterialStream.FlowSheet.Options.Reactions(Me.Reactions(i))
                        If .Components.ContainsKey(s) Then
                            If .Components(s).StoichCoeff > 0 Then
                                For j = 0 To nc
                                    If CompoundProperties(j).Name = s Then
                                        prod(i) *= CP(j) ^ .Components(s).StoichCoeff
                                        Exit For
                                    End If
                                Next
                            End If
                        End If
                    End With
                Next
            Next

            For i = 0 To Me.Reactions.Count - 1
                With proppack.CurrentMaterialStream.FlowSheet.Options.Reactions(Me.Reactions(i))
                    f(i) = Abs(prod(i) - .ConstantKeqValue) ^ 2
                End With
            Next

            Return f

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
            For i = 0 To m - 1 - 1
                If CompoundProperties(i).IsIon Then
                    g(i) = x(i)
                ElseIf CompoundProperties(i).IsSalt Then
                    g(i) = x(i + m - 1)
                Else
                    g(i) = fi(i) * F - (x(i) + x(i + m - 1))
                End If
            Next
            g(m - 1) = V
            Return True
        End Function

        Public Function eval_jac_g(ByVal n As Integer, ByVal x As Double(), ByVal new_x As Boolean, ByVal m As Integer, ByVal nele_jac As Integer, ByRef iRow As Integer(), _
         ByRef jCol As Integer(), ByRef values As Double()) As Boolean

            If values Is Nothing Then

                Dim row(nele_jac - 1), col(nele_jac - 1) As Integer

                k = 0
                For i = 0 To m - 1 - 1
                    row(i) = i
                    row(i + m - 1) = i
                    col(i) = i
                    col(i + m - 1) = i + m - 1
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

            Return True

        End Function


    End Class

End Namespace

