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
Imports DotNumerics.Optimization

Namespace DWSIM.SimulationObjects.PropertyPackages.Auxiliary.FlashAlgorithms

    <System.Serializable()> Public Class ElectrolyteSVLE2

        Public proppack As PropertyPackage

        Dim tmpx As Double(), tmpdx As Double()

        Dim Hf, Hl, Hv, Hs, Tf, Pf, P0, Ninerts, Winerts, E(,) As Double
        Dim r, c, els, comps, wid As Integer

        Dim n, nr, ecount As Integer
        Dim etol As Double = 0.01
        Dim itol As Double = 0.01
        Dim maxit_i As Integer = 1000
        Dim maxit_e As Integer = 1000

        Dim Vxl(n), Vxs(n), Vxv(n), Vv(n), Vl(n), Vs(n), fi(n), Vp(n) As Double, Vn(n) As String

        Dim F, L, S, V As Double

        Dim soma_x, soma_s, soma_y As Double

        Public Property ReactionSet As String = "DefaultSet"
        Public Property Reactions As List(Of String)
        Public Property ReactionExtents As Dictionary(Of String, Double)
        Public Property ComponentIDs As List(Of String)
        Public Property CompoundProperties As List(Of ConstantProperties)
        Public Property ComponentConversions As Dictionary(Of String, Double)

        Public Property MaximumIterations As Integer = 5000
        Public Property Tolerance As Double = 0.001
        Public Property NumericalDerivativePerturbation As Double = 0.0000000001

        Public Property CalculateChemicalEquilibria As Boolean = True

        Private Vx0 As Double()

        Private LoopVarF, LoopVarX As Double, LoopVarVz As Double(), LoopVarState As State

        Public Sub WriteDebugInfo(text As String)

            Select Case My.Settings.DebugLevel
                Case 0
                    'do nothing
                Case Else
                    Console.WriteLine(text)
            End Select

        End Sub

        Public Function Flash_PT(Vz As Array, T As Double, P As Double) As Dictionary(Of String, Object)

            'This flash algorithm is for Electrolye/Salt systems with Water as the single solvent.
            'The vapor and solid phases are considered to be ideal.
            'Chemical equilibria is calculated using the reactions enabled in the default reaction set.

            etol = CDbl(proppack.Parameters("PP_PTFELT"))
            maxit_e = CInt(proppack.Parameters("PP_PTFMEI"))
            itol = CDbl(proppack.Parameters("PP_PTFILT"))
            maxit_i = CInt(proppack.Parameters("PP_PTFMII"))

            Dim d1, d2 As Date, dt As TimeSpan
            d1 = Date.Now

            n = CompoundProperties.Count - 1

            If Me.Reactions Is Nothing Then Me.Reactions = New List(Of String)
         
            Dim form As FormFlowsheet = proppack.CurrentMaterialStream.FlowSheet

            Me.Reactions.Clear()
      
            Dim i, j As Integer

            ReDim Vxv(n), Vxl(n), Vxs(n), Vn(n), Vp(n)

            Dim activcoeff(n), VxVp(n) As Double

            'Vnf = feed molar amounts (considering 1 mol of feed)
            'Vnl = liquid phase molar amounts
            'Vnv = vapor phase molar amounts
            'Vns = solid phase molar amounts
            'Vxl = liquid phase molar fractions
            'Vxv = vapor phase molar fractions
            'Vxs = solid phase molar fractions
            'V, S, L = phase molar amounts (F = 1 = V + S + L)

            Dim sumN As Double = 0

            Vn = proppack.RET_VNAMES

            For i = 0 To n
                Vp(i) = proppack.AUX_PVAPi(i, T)
                If Double.IsNaN(Vp(i)) Or Double.IsInfinity(Vp(i)) Then Vp(i) = 1.0E+20
                VxVp(i) = Vz(i) * Vp(i)
            Next

            'get water index in the array.

            wid = CompoundProperties.IndexOf((From c As ConstantProperties In CompoundProperties Select c Where c.Name = "Water").SingleOrDefault)

            F = 1.0#

            Pf = P
            Tf = T

            fi = Vz.Clone()

            If Vz(wid) = 0.0# Then

                'only solids in the stream (no liquid water).

                V = 0.0#
                L = 0.0#
                S = 1.0#
                For i = 0 To n
                    Vxs(i) = Vz(i)
                Next
                sumN = 1.0#

            ElseIf VxVp.Min > P Then

                'all vapor

                V = 1.0#
                L = 0.0#
                S = 0.0#
                For i = 0 To n
                    Vxv(i) = Vz(i)
                Next
                sumN = 1.0#

            Else

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
                                rxn.ExpContext = New Ciloci.Flee.ExpressionContext
                                rxn.ExpContext.Imports.AddType(GetType(System.Math))
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

                Dim initval2(2 * n + 1) As Double
                Dim lconstr2(2 * n + 1) As Double
                Dim uconstr2(2 * n + 1) As Double
                Dim finalval2(2 * n + 1) As Double

                For i = 0 To n
                    If CompoundProperties(i).IsIon Or CompoundProperties(i).IsSalt Then
                        initval2(i) = Vz(i) * F
                        If ComponentIDs.Contains(Vn(i)) Then
                            uconstr2(i) = F
                        Else
                            uconstr2(i) = Vz(i) * F
                        End If
                    Else
                        If Vp(i) > P Then
                            initval2(i) = 0.0#
                        Else
                            initval2(i) = Vz(i) * F
                        End If
                        uconstr2(i) = Vz(i) * F
                    End If
                    lconstr2(i) = 0.0#
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

                'run a few iterations of the simplex method to refine initial estimates

                ecount = 0

                'Dim variables(initval2.Length - 1) As OptSimplexBoundVariable
                'For i = 0 To initval2.Length - 1
                '    variables(i) = New OptSimplexBoundVariable(initval2(i), lconstr2(i), uconstr2(i))
                'Next
                'Dim solver As New Simplex
                'solver.Tolerance = 1.0E-20
                'solver.MaxFunEvaluations = 10000
                'initval2 = solver.ComputeMin(AddressOf FunctionValue, variables)

                'For i = 0 To 2 * n + 1
                '    If initval2(i) < lconstr2(i) Then initval2(i) = lconstr2(i)
                '    If initval2(i) > uconstr2(i) Then initval2(i) = uconstr2(i)
                'Next

                'find optimim solution with IPOPT solver

                Dim obj As Double
                Dim status As IpoptReturnCode = IpoptReturnCode.Solve_Succeeded

                Using problem As New Ipopt(initval2.Length, lconstr2, uconstr2, 0, Nothing, Nothing, 0, 0, _
                        AddressOf eval_f, AddressOf eval_g, _
                        AddressOf eval_grad_f, AddressOf eval_jac_g, AddressOf eval_h)
                    problem.AddOption("tol", Tolerance)
                    problem.AddOption("max_iter", MaximumIterations)
                    problem.AddOption("acceptable_tol", etol)
                    problem.AddOption("acceptable_iter", maxit_e)
                    problem.AddOption("mu_strategy", "adaptive")
                    'problem.AddOption("mehrotra_algorithm", "no")
                    problem.AddOption("hessian_approximation", "limited-memory")
                    problem.AddOption("print_level", 5)
                    'solve the problem 
                    status = problem.SolveProblem(initval2, obj, Nothing, Nothing, Nothing, Nothing)
                End Using

                'FunctionValue(initval2)

                Dim CE, MB As Double

                CE = CheckEquilibrium().Sum
                MB = CheckMassBalance() ^ 2

                If status = IpoptReturnCode.Solve_Succeeded Or status = IpoptReturnCode.Solved_To_Acceptable_Level Then
                    WriteDebugInfo("PT Flash [Electrolyte]: Converged in " & ecount & " iterations. Status: " & [Enum].GetName(GetType(IpoptReturnCode), status) & ". Time taken: " & dt.TotalMilliseconds & " ms")
                ElseIf CE < Tolerance And MB < etol Then
                    form.WriteToLog("PT Flash [Electrolyte]: Maximum iterations exceeded, although mass balance and chemical equilibrium are satisfied within tolerance. Status: " & [Enum].GetName(GetType(IpoptReturnCode), status), Color.DarkOrange, FormClasses.TipoAviso.Aviso)
                Else
                    Throw New Exception("PT Flash [Electrolyte]: Unable to solve - " & [Enum].GetName(GetType(IpoptReturnCode), status))
                End If

                For i = 0 To n
                    fi(i) = (L * Vxl(i) + V * Vxv(i) + S * Vxs(i)) / F
                Next

            End If

            d2 = Date.Now

            dt = d2 - d1

out:        'return flash calculation results.

            If L > 0.0# Then

                'calculate activity coefficients.

                If TypeOf proppack Is ExUNIQUACPropertyPackage Then
                    activcoeff = CType(proppack, ExUNIQUACPropertyPackage).m_uni.GAMMA_MR(T, Vxl, CompoundProperties)
                ElseIf TypeOf proppack Is LIQUAC2PropertyPackage Then
                    activcoeff = CType(proppack, LIQUAC2PropertyPackage).m_uni.GAMMA_MR(T, Vxl, CompoundProperties)
                End If

            End If

            Dim results As New Dictionary(Of String, Object)

            results.Add("MixtureMoleFlows", fi)
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

            'This flash algorithm is for Electrolye/Salt systems with Water as the single solvent.
            'The vapor and solid phases are considered to be ideal.
            'Chemical equilibria is calculated using the reactions enabled in the default reaction set.

            etol = CDbl(proppack.Parameters("PP_PHFELT"))
            maxit_e = CInt(proppack.Parameters("PP_PHFMEI"))
            itol = CDbl(proppack.Parameters("PP_PHFILT"))
            maxit_i = CInt(proppack.Parameters("PP_PHFMII"))

            Dim d1, d2 As Date, dt As TimeSpan
            d1 = Date.Now

            n = CompoundProperties.Count - 1

            If Me.Reactions Is Nothing Then Me.Reactions = New List(Of String)

            Dim form As FormFlowsheet = proppack.CurrentMaterialStream.FlowSheet

            Me.Reactions.Clear()

            Dim i, j As Integer

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

            wid = CompoundProperties.IndexOf((From c As ConstantProperties In CompoundProperties Select c Where c.Name = "Water").SingleOrDefault)

            F = 1.0#

            Pf = P
            Hf = H

            fi = Vz.Clone()

            If Vz(wid) = 0.0# Then

                'only solids in the stream (no liquid water).

                V = 0.0#
                L = 0.0#
                S = 1.0#
                For i = 0 To n
                    Vxs(i) = Vz(i)
                Next
                sumN = 1.0#

            Else

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
                                rxn.ExpContext = New Ciloci.Flee.ExpressionContext
                                rxn.ExpContext.Imports.AddType(GetType(System.Math))
                                rxn.ExpContext.Variables.Add("T", Tf)
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
                                Dim DelG_RT = proppack.AUX_DELGig_RT(298.15, Tf, id, stcoef, bcidx, True)
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

                Dim initval2(2 * n + 2) As Double
                Dim lconstr2(2 * n + 2) As Double
                Dim uconstr2(2 * n + 2) As Double
                Dim finalval2(2 * n + 2) As Double

                For i = 0 To n
                    If CompoundProperties(i).IsIon Or CompoundProperties(i).IsSalt Then
                        initval2(i) = Vz(i) * F
                        If ComponentIDs.Contains(Vn(i)) Then
                            uconstr2(i) = F
                        Else
                            uconstr2(i) = Vz(i) * F
                        End If
                    Else
                        If Vp(i) > P Then
                            initval2(i) = 0.0#
                        Else
                            initval2(i) = Vz(i) * F
                        End If
                        uconstr2(i) = Vz(i) * F
                    End If
                    lconstr2(i) = 0.0#
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
                initval2(2 * n + 2) = Tref
                lconstr2(2 * n + 2) = 273
                uconstr2(2 * n + 2) = 2000

                ecount = 0

                Dim obj As Double
                Dim status As IpoptReturnCode

                Using problem As New Ipopt(initval2.Length, lconstr2, uconstr2, 0, Nothing, Nothing, 0, 0, _
                        AddressOf eval_fh, AddressOf eval_g, _
                        AddressOf eval_grad_fh, AddressOf eval_jac_g, AddressOf eval_h)
                    problem.AddOption("tol", Tolerance)
                    problem.AddOption("max_iter", MaximumIterations)
                    problem.AddOption("acceptable_tol", etol)
                    problem.AddOption("acceptable_iter", maxit_e)
                    problem.AddOption("mu_strategy", "adaptive")
                    'problem.AddOption("mehrotra_algorithm", "no")
                    problem.AddOption("hessian_approximation", "limited-memory")
                    problem.AddOption("print_level", 5)
                    'solve the problem 
                    status = problem.SolveProblem(initval2, obj, Nothing, Nothing, Nothing, Nothing)
                End Using

                'FunctionValueH(initval2)

                Dim CE, MB As Double

                CE = CheckEquilibrium().Sum
                MB = CheckMassBalance() ^ 2

                If status = IpoptReturnCode.Solve_Succeeded Or status = IpoptReturnCode.Solved_To_Acceptable_Level Then
                    WriteDebugInfo("PH Flash [Electrolyte]: Converged in " & ecount & " iterations. Status: " & [Enum].GetName(GetType(IpoptReturnCode), status) & ". Time taken: " & dt.TotalMilliseconds & " ms")
                ElseIf CE < Tolerance And MB < etol Then
                    form.WriteToLog("PH Flash [Electrolyte]: Maximum iterations exceeded, although mass balance and chemical equilibrium are satisfied within tolerance. Status: " & [Enum].GetName(GetType(IpoptReturnCode), status), Color.DarkOrange, FormClasses.TipoAviso.Aviso)
                Else
                    Throw New Exception("PH Flash [Electrolyte]: Unable to solve - " & [Enum].GetName(GetType(IpoptReturnCode), status))
                End If

                For i = 0 To n
                    fi(i) = (L * Vxl(i) + V * Vxv(i) + S * Vxs(i)) / F
                Next

            End If

            d2 = Date.Now

            dt = d2 - d1

out:        'return flash calculation results.

            If L > 0.0# Then

                'calculate activity coefficients.

                If TypeOf proppack Is ExUNIQUACPropertyPackage Then
                    activcoeff = CType(proppack, ExUNIQUACPropertyPackage).m_uni.GAMMA_MR(Tf, Vxl, CompoundProperties)
                ElseIf TypeOf proppack Is LIQUAC2PropertyPackage Then
                    activcoeff = CType(proppack, LIQUAC2PropertyPackage).m_uni.GAMMA_MR(Tf, Vxl, CompoundProperties)
                End If

            End If

            Dim results As New Dictionary(Of String, Object)

            results.Add("MixtureMoleFlows", fi)
            results.Add("VaporPhaseMoleFraction", V / F)
            results.Add("LiquidPhaseMoleFraction", L / F)
            results.Add("SolidPhaseMoleFraction", S / F)
            results.Add("VaporPhaseMolarComposition", Vxv)
            results.Add("LiquidPhaseMolarComposition", Vxl)
            results.Add("SolidPhaseMolarComposition", Vxs)
            results.Add("LiquidPhaseActivityCoefficients", activcoeff)
            results.Add("MoleSum", 1.0#)
            results.Add("Temperature", Tf)

            Return results

        End Function

        Function Herror(ByVal X As Double, ByVal P As Double, ByVal Vz() As Double) As Double
            Return OBJ_FUNC_PH_FLASH(X, P, Vz)
        End Function

        Function EnthalpyError(T As Double) As Double

            Dim _Hv, _Hl, _Hs As Double
            Dim mmg, mml, mms As Double

            If V > 0.0# Then _Hv = proppack.DW_CalcEnthalpy(Vxv, T, Pf, State.Vapor)
            If L > 0.0# Then _Hl = proppack.DW_CalcEnthalpy(Vxl, T, Pf, State.Liquid)
            If S > 0.0# Then _Hs = proppack.DW_CalcSolidEnthalpy(T, Vxs, CompoundProperties)

            mmg = proppack.AUX_MMM(Vxv)
            mml = proppack.AUX_MMM(Vxl)
            mms = proppack.AUX_MMM(Vxs)

            Dim herr As Double = Hf - ((mmg * V / (mmg * V + mml * L + mms * S)) * _Hv + (mml * L / (mmg * V + mml * L + mms * S)) * _Hl + (mms * S / (mmg * V + mml * L + mms * S)) * _Hs)

            Return herr

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
                If Vxv(i) < 0.0# Then Vxv(i) = 0.0#
                If Vxl(i) < 0.0# Then Vxl(i) = 0.0#
                If Vxs(i) < 0.0# Then Vxs(i) = 0.0#
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

            Dim Gv, Gl, Gs, Gm, Rx, Pv As Double

            Gv = 0.0#
            Gl = 0.0#
            Gs = 0.0#
            For i = 0 To n
                If fcl(i) = 0.0# Or Double.IsInfinity(fcl(i)) Or Double.IsNaN(fcl(i)) Then fcl(i) = 1.0E+20
                If Vxv(i) <> 0.0# Then Gv += Vxv(i) * V * Log(fcv(i) * Vxv(i))
                If Vxl(i) <> 0.0# Then Gl += Vxl(i) * L * Log(fcl(i) * Vxl(i))
                If Vxs(i) <> 0.0# Then Gs += Vxs(i) * S * Log(fcs(i) * Vxs(i))
            Next

            ecount += 1

            Gm = Gv + Gl + Gs

            Rx = CheckEquilibrium().Sum

            If Double.IsNaN(Rx) Then Rx = 0.0#

            Pv = CheckMassBalance() ^ 2

            Return Gm + Rx + Pv

        End Function

        Private Function FunctionGradient(ByVal x() As Double) As Double()

            Dim epsilon As Double = NumericalDerivativePerturbation

            Dim f1, f2 As Double
            Dim g(x.Length - 1), x2(x.Length - 1) As Double
            Dim i, j As Integer

            For i = 0 To x.Length - 1
                f1 = FunctionValue(x)
                For j = 0 To x.Length - 1
                    If x(j) = 0.0# Then
                        If i <> j Then
                            x2(j) = x(j)
                        Else
                            x2(j) = x(j) + epsilon
                        End If
                    Else
                        If i <> j Then
                            x2(j) = x(j)
                        Else
                            x2(j) = x(j) * (1 + epsilon)
                        End If
                    End If
                Next
                f2 = FunctionValue(x2)
                g(i) = (f2 - f1) / (x2(i) - x(i))
            Next

            Return g

        End Function

        Private Function FunctionValueH(ByVal x() As Double) As Double

            CheckCalculatorStatus()

            Dim fcv(n), fcl(n), fcs(n) As Double

            soma_x = 0
            For i = 0 To x.Length - n - 3
                soma_x += x(i)
            Next
            soma_s = 0
            For i = x.Length - n - 1 To x.Length - 2
                soma_s += x(i)
            Next
            L = soma_x
            S = soma_s
            V = F - L - S

            Tf = x(x.Length - 1)

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
                If Vxv(i) < 0.0# Then Vxv(i) = 0.0#
                If Vxl(i) < 0.0# Then Vxl(i) = 0.0#
                If Vxs(i) < 0.0# Then Vxs(i) = 0.0#
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

            Dim Gv, Gl, Gs, Gm, Rx, Pv, Herr As Double

            Gv = 0.0#
            Gl = 0.0#
            Gs = 0.0#
            For i = 0 To n
                If fcl(i) = 0.0# Or Double.IsInfinity(fcl(i)) Or Double.IsNaN(fcl(i)) Then fcl(i) = 1.0E+20
                If Vxv(i) <> 0.0# Then Gv += Vxv(i) * V * Log(fcv(i) * Vxv(i))
                If Vxl(i) <> 0.0# Then Gl += Vxl(i) * L * Log(fcl(i) * Vxl(i))
                If Vxs(i) <> 0.0# Then Gs += Vxs(i) * S * Log(fcs(i) * Vxs(i))
            Next

            ecount += 1

            Gm = Gv + Gl + Gs

            Rx = CheckEquilibrium().Sum

            If Double.IsNaN(Rx) Then Rx = 0.0#

            Pv = CheckMassBalance() ^ 2

            Herr = EnthalpyError(Tf) ^ 2

            Return Gm + Rx + Pv + Herr

        End Function

        Private Function FunctionGradientH(ByVal x() As Double) As Double()

            Dim epsilon As Double = NumericalDerivativePerturbation

            Dim f1, f2 As Double
            Dim g(x.Length - 1), x2(x.Length - 1) As Double
            Dim i, j As Integer

            For i = 0 To x.Length - 1
                f1 = FunctionValueH(x)
                For j = 0 To x.Length - 1
                    If x(j) = 0.0# Then
                        If i <> j Then
                            x2(j) = x(j)
                        Else
                            x2(j) = x(j) + epsilon
                        End If
                    Else
                        If i <> j Then
                            x2(j) = x(j)
                        Else
                            x2(j) = x(j) * (1 + epsilon)
                        End If
                    End If
                Next
                f2 = FunctionValueH(x2)
                g(i) = (f2 - f1) / (x2(i) - x(i))
            Next

            Return g

        End Function

        Private Function CheckMassBalance() As Double

            Dim Mb As Double
            With proppack
                Mb = F * .AUX_MMM(fi) - (Abs(L * .AUX_MMM(Vxl)) + Abs(S * .AUX_MMM(Vxs)) + Abs(V * .AUX_MMM(Vxv)))
            End With
            Return Abs(Mb) * 1000

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

        Public Function eval_fh(ByVal n As Integer, ByVal x As Double(), ByVal new_x As Boolean, ByRef obj_value As Double) As Boolean
            Dim fval As Double = FunctionValueH(x)
            obj_value = fval
            Return True
        End Function

        Public Function eval_grad_f(ByVal n As Integer, ByVal x As Double(), ByVal new_x As Boolean, ByRef grad_f As Double()) As Boolean
            Dim g As Double() = FunctionGradient(x)
            grad_f = g
            Return True
        End Function

        Public Function eval_grad_fh(ByVal n As Integer, ByVal x As Double(), ByVal new_x As Boolean, ByRef grad_f As Double()) As Boolean
            Dim g As Double() = FunctionGradientH(x)
            grad_f = g
            Return True
        End Function

        Public Function eval_g(ByVal n As Integer, ByVal x As Double(), ByVal new_x As Boolean, ByVal m As Integer, ByRef g As Double()) As Boolean
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

                'values = FunctionHessian(x)

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

