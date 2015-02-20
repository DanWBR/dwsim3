'    Flash Algorithm for Electrolyte solutions
'    Copyright 2013-2014 Daniel Wagner O. de Medeiros
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

Namespace DWSIM.SimulationObjects.PropertyPackages.Auxiliary.FlashAlgorithms

    <System.Serializable()> Public Class ElectrolyteSVLE

        Public proppack As PropertyPackage

        Dim tmpx As Double(), tmpdx As Double()

        Dim N0 As New Dictionary(Of String, Double)
        Dim DN As New Dictionary(Of String, Double)
        Dim N As New Dictionary(Of String, Double)
        Dim Hf, Hl, Hv, Hs, T, P, P0, Ninerts, Winerts, E(,) As Double
        Dim r, c, els, comps As Integer

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

        Public Function Flash_PT(Vx As Array, T As Double, P As Double) As Dictionary(Of String, Object)

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
            Dim Vnf(n), Vnl(n), Vnl_ant(n), Vxl(n), Vns(n), Vxs(n), Vnv(n), Vxv(n), Vf(n), V, S, L, Vp(n), err As Double
            Dim sumN As Double = 0

            'get water index in the array.

            Vnf = Vx.Clone

            Dim wid As Integer = CompoundProperties.IndexOf((From c As ConstantProperties In CompoundProperties Select c Where c.Name = "Water").SingleOrDefault)

            'calculate water vapor pressure.

            Dim Psat = proppack.AUX_PVAPi(wid, T)

            For i = 0 To n
                If i <> wid Then
                    Vxv(i) = 0.0#
                Else
                    Vxv(i) = 1.0#
                End If
            Next

            If Vnf(wid) = 0.0# Then

                'only solids in the stream (no liquid water).

                V = 0.0#
                L = 0.0#
                S = 1.0#
                For i = 0 To n
                    Vxs(i) = Vnf(i)
                Next
                Vf = Vnf
                sumN = 1.0#

            Else

                V = 0.0#
                L = 1.0#
                S = 0.0#

                'calculate SLE.

                Dim ids As New List(Of String)
                For i = 0 To n
                    ids.Add(CompoundProperties(i).Name)
                    Vp(i) = proppack.AUX_PVAPi(i, T)
                Next

                'get the default reaction set.

                Me.Vx0 = Vx.Clone

                Vnl = Vx.Clone
                Vf = Vx.Clone

                Dim int_count As Integer = 0
                Dim L_ant As Double = 0.0#

                Dim rext As Double() = Nothing
                Dim result As Object

                Do

                    'calculate chemical equilibria between ions, salts and water. 
                    ''SolveChemicalEquilibria' returns the equilibrium molar amounts in the liquid phase, including precipitates.

                    If CalculateChemicalEquilibria Then
                        result = SolveChemicalEquilibria(Vf, T, P, ids, rext)
                        Vf = result(0).clone
                        rext = result(1)
                    End If

                    For i = 0 To n
                        Vxl(i) = Vf(i) / Vf.Sum()
                    Next

                    'calculate activity coefficients.

                    If TypeOf proppack Is ExUNIQUACPropertyPackage Then
                        activcoeff = CType(proppack, ExUNIQUACPropertyPackage).m_uni.GAMMA_MR(T, Vxl, CompoundProperties)
                    ElseIf TypeOf proppack Is LIQUAC2PropertyPackage Then
                        activcoeff = CType(proppack, LIQUAC2PropertyPackage).m_uni.GAMMA_MR(T, Vxl, CompoundProperties)
                    End If

                    If activcoeff(wid) = 0.0# Then
                        'possibly only solids in this stream. will make arrangements so the maximum solubility is very small.
                        For i = 0 To n
                            activcoeff(i) = 10000000000.0
                        Next
                    End If

                    Dim Vxlmax(n) As Double

                    'calculate maximum solubilities for solids/precipitates.

                    For i = 0 To n
                        If CompoundProperties(i).TemperatureOfFusion <> 0.0# And i <> wid Then
                            Vxlmax(i) = (1 / activcoeff(i)) * Exp(-CompoundProperties(i).EnthalpyOfFusionAtTf / (0.00831447 * T) * (1 - T / CompoundProperties(i).TemperatureOfFusion))
                            If Vxlmax(i) > 1 Then Vxlmax(i) = 1.0#
                        Else
                            If CompoundProperties(i).IsHydratedSalt Then
                                Vxlmax(i) = 0.0# 'in the absence of enthalpy/temperature of fusion, I'll assume that the hydrated salt will always precipitate, if present.
                            Else
                                Vxlmax(i) = 1.0#
                            End If
                        End If
                    Next

                    'mass balance.

                    For i = 0 To n
                        Vnl_ant(i) = Vnl(i)
                        If Vxl(i) > Vxlmax(i) Then
                            Vxl(i) = Vxlmax(i)
                            Vnl(i) = Vxl(i) * L
                            Vns(i) = Vf(i) - Vnl(i)
                        Else
                            Vnl(i) = Vf(i)
                            Vns(i) = 0
                        End If
                    Next

                    'check for vapors
                    'For i = 0 To n
                    '    If P < Vp(i) Then
                    '        Vxl(i) = 0
                    '        Vnl(i) = 0
                    '        Vnv(i) = Vf(i)
                    '    End If
                    'Next

                    'liquid mole amounts

                    L_ant = L
                    L = Vnl.Sum()
                    S = Vns.Sum()
                    V = Vnv.Sum()

                    For i = 0 To n
                        If Sum(Vnl) <> 0.0# Then Vxl(i) = Vnl(i) / Sum(Vnl) Else Vxl(i) = 0.0#
                        If Sum(Vns) <> 0.0# Then Vxs(i) = Vns(i) / Sum(Vns) Else Vxs(i) = 0.0#
                        If Sum(Vnv) <> 0.0# Then Vxv(i) = Vnv(i) / Sum(Vnv) Else Vxv(i) = 0.0#
                    Next

                    sumN = 0
                    For i = 0 To n
                        Vf(i) = Vnv(i) + Vnl(i) + Vns(i)
                        sumN += Vf(i)
                    Next

                    err = 0
                    For i = 0 To n
                        err += Abs(Vnl(i) - Vnl_ant(i)) ^ 2
                    Next
                    err += (L - L_ant) ^ 2

                    If err < Tolerance And int_count > 0 Then Exit Do

                    int_count += 1

                    CheckCalculatorStatus()

                Loop Until int_count > MaximumIterations

                If int_count > MaximumIterations Then
                    Throw New Exception("Chemical Equilibrium Solver error: Reached the maximum number of external iterations without converging.")
                End If

            End If

            L = L / sumN
            S = S / sumN
            V = V / sumN

            'return flash calculation results.

            Dim results As New Dictionary(Of String, Object)

            results.Add("MixtureMoleFlows", Vf)
            results.Add("VaporPhaseMoleFraction", V)
            results.Add("LiquidPhaseMoleFraction", L)
            results.Add("SolidPhaseMoleFraction", S)
            results.Add("VaporPhaseMolarComposition", Vxv)
            results.Add("LiquidPhaseMolarComposition", Vxl)
            results.Add("SolidPhaseMolarComposition", Vxs)
            results.Add("LiquidPhaseActivityCoefficients", activcoeff)
            results.Add("MoleSum", sumN)

            Return results

        End Function

        Private Function SolveChemicalEquilibria(ByVal Vx As Array, ByVal T As Double, ByVal P As Double, ByVal ids As List(Of String), Optional ByVal prevx As Double() = Nothing) As Array

            Dim i, j As Integer

            'solves the chemical equilibria for the liquid phase.

            If Me.ReactionExtents Is Nothing Then Me.ReactionExtents = New Dictionary(Of String, Double)
            If Me.Reactions Is Nothing Then Me.Reactions = New List(Of String)
            If Me.ComponentConversions Is Nothing Then Me.ComponentConversions = New Dictionary(Of String, Double)
            If Me.ComponentIDs Is Nothing Then Me.ComponentIDs = New List(Of String)

            Dim form As FormFlowsheet = proppack.CurrentMaterialStream.Flowsheet

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

            If Me.Reactions.Count > 0 Then

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

                'upper and lower bounds for reaction extents (just an estimate, will be different when a single compound appears in multiple reactions)

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
                            lbound(i) = 0.0#
                            ubound(i) = var1
                        Else
                            If var1 < lbound(i) Then lbound(i) = 0.0#
                            If var1 > ubound(i) Then ubound(i) = var1
                        End If
                        j += 1
                    Next
                    i += 1
                Next

                'initial estimates for the reaction extents

                Dim REx(r) As Double

                If Not prevx Is Nothing Then
                    For i = 0 To r
                        REx(i) = prevx(i) * 0.9
                    Next
                Else
                    i = 0
                    For Each rxnsb As ReactionSetBase In form.Options.ReactionSets(Me.ReactionSet).Reactions.Values
                        If form.Options.Reactions(rxnsb.ReactionID).ReactionType = ReactionType.Equilibrium And rxnsb.IsActive Then
                            rxn = form.Options.Reactions(rxnsb.ReactionID)
                            If rxn.ConstantKeqValue < 1 Then
                                REx(i) = 0.0#
                            ElseIf rxn.ConstantKeqValue > 10 Then
                                REx(i) = ubound(i)
                            Else
                                REx(i) = ubound(i) * 0.5
                            End If
                            i += 1
                        End If
                    Next
                End If

                Dim REx0(REx.Length - 1) As Double

                'solve using newton's method

                Dim fx(r), dfdx(r, r), dfdx_ant(r, r), dx(r), x(r), df, fval As Double
                Dim brentsolver As New BrentOpt.BrentMinimize
                brentsolver.DefineFuncDelegate(AddressOf MinimizeError)

                Dim niter As Integer, allok As Boolean

                Me.T = T
                Me.P = P

                x = REx
                niter = 0
                allok = True
                Do

                    fx = Me.FunctionValue2N(x)

                    For i = 0 To r
                        If x(i) < 0.5 And Abs(x(i)) ^ 2 > Tolerance Then
                            allok = False
                        ElseIf x(i) >= 0.5 And Abs(x(i) - 1.0#) ^ 2 > Tolerance Then
                            allok = False
                        End If
                    Next

                    If SumSqr(fx) < Tolerance Or allok Then Exit Do

                    dfdx_ant = dfdx.Clone
                    dfdx = Me.FunctionGradient2N(x)

                    Dim success As Boolean
                    success = MathEx.SysLin.rsolve.rmatrixsolve(dfdx, fx, r + 1, dx)

                    If Not success Then
                        dfdx = dfdx_ant.Clone
                        MathEx.SysLin.rsolve.rmatrixsolve(dfdx, fx, r + 1, dx)
                    End If

                    tmpx = x
                    tmpdx = dx
                    df = 1
                    fval = brentsolver.brentoptimize(0.00001, 2.0#, 0.00000001, df)

                    For i = 0 To r
                        x(i) -= dx(i) * df
                    Next

                    niter += 1

                    If AbsSum(dx) = 0.0# Then
                        Throw New Exception("Chemical Equilibrium Solver error: No solution found - reached a stationary point of the objective function (singular gradient matrix).")
                    End If

                    If niter > MaximumIterations Then
                        Throw New Exception("Chemical Equilibrium Solver error: Reached the maximum number of internal iterations without converging.")
                    End If

                    CheckCalculatorStatus()

                Loop

                fx = Me.FunctionValue2N(x)

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

                ' return equilibrium molar amounts in the liquid phase.

                For Each s As String In N.Keys
                    Vx(ids.IndexOf(s)) = Abs(N(s))
                Next

                Dim nc As Integer = Vx.Length - 1

                Dim mtot As Double = 0
                For i = 0 To nc
                    mtot += Vx(i)
                Next

                For i = 0 To nc
                    Vx(i) = Vx(i) '/ mtot
                Next


                Return New Object() {Vx, x}

            Else

                Return New Object() {Vx, Nothing}

                End If

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
                'If N(s) < 0 Then N(s) = 0
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
                        Vx(j) = N(ComponentIDs(i))
                        Exit For
                    End If
                Next
            Next

            i = 0
            Do
                If CompoundProperties(i).Name = "Water" Then
                    wtotal += Vx(i) * CompoundProperties(i).Molar_Weight / 1000
                End If
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

            If TypeOf proppack Is ExUNIQUACPropertyPackage Then
                activcoeff = CType(proppack, ExUNIQUACPropertyPackage).m_uni.GAMMA_MR(T, Vx, CompoundProperties)
            ElseIf TypeOf proppack Is LIQUAC2PropertyPackage Then
                activcoeff = CType(proppack, LIQUAC2PropertyPackage).m_uni.GAMMA_MR(T, Vx, CompoundProperties)
            End If

            Dim CP(nc) As Double
            Dim f(x.Length - 1) As Double
            Dim prod(x.Length - 1) As Double

            For i = 0 To nc
                If CompoundProperties(i).IsIon Then
                    CP(i) = molality(i) * activcoeff(i)
                ElseIf CompoundProperties(i).IsSalt Then
                    CP(i) = 1.0#
                Else
                    CP(i) = Vx(i) * activcoeff(i)
                End If
            Next

            For i = 0 To Me.Reactions.Count - 1
                prod(i) = 1
                For Each s As String In Me.ComponentIDs
                    With proppack.CurrentMaterialStream.Flowsheet.Options.Reactions(Me.Reactions(i))
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

            Dim pen_val As Double = ReturnPenaltyValue()

            For i = 0 To Me.Reactions.Count - 1
                With proppack.CurrentMaterialStream.Flowsheet.Options.Reactions(Me.Reactions(i))
                    f(i) = (prod(i) - .ConstantKeqValue)
                    If Double.IsNaN(f(i)) Or Double.IsInfinity(f(i)) Or pen_val <> 0.0# Then
                        f(i) = pen_val ^ 2
                    End If
                End With
            Next

            Return f

        End Function

        Private Function FunctionGradient2N(ByVal x() As Double) As Double(,)

            Dim epsilon As Double = 0.000001

            Dim f1(), f2() As Double
            Dim g(x.Length - 1, x.Length - 1), x2(x.Length - 1) As Double
            Dim i, j, k As Integer

            f1 = FunctionValue2N(x)
            For i = 0 To x.Length - 1
                For j = 0 To x.Length - 1
                    If i <> j Then
                        x2(j) = x(j)
                    Else
                        If x(j) = 0.0# Then
                            x2(j) = x(j) + epsilon
                        Else
                            x2(j) = x(j) * (1 + epsilon)
                        End If
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

        Public Function Flash_PH(ByVal Vz As Array, ByVal P As Double, ByVal H As Double, ByVal Tref As Double) As Dictionary(Of String, Object)

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

        Function OBJ_FUNC_PH_FLASH(ByVal T As Double, ByVal H As Double, ByVal P As Double, ByVal Vz As Object) As Object

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
            If V > 0 Then _Hv = proppack.DW_CalcEnthalpy(Vy, T, P, State.Vapor)
            If L > 0 Then _Hl = proppack.DW_CalcEnthalpy(Vx, T, P, State.Liquid)
            If S > 0 Then _Hs = proppack.DW_CalcSolidEnthalpy(T, Vs, CompoundProperties)
            mmg = proppack.AUX_MMM(Vy)
            mml = proppack.AUX_MMM(Vx)
            mms = proppack.AUX_MMM(Vs)

            Dim herr As Double = Hf - ((mmg * V / (mmg * V + mml * L + mms * S)) * _Hv + (mml * L / (mmg * V + mml * L + mms * S)) * _Hl + (mms * S / (mmg * V + mml * L + mms * S)) * _Hs)
            OBJ_FUNC_PH_FLASH = herr

            WriteDebugInfo("PH Flash [Electrolyte]: Current T = " & T & ", Current H Error = " & herr)

        End Function

        Function Herror(ByVal Tt As Double, ByVal otherargs As Object) As Double
            Return OBJ_FUNC_PH_FLASH(Tt, Hf, otherargs(0), otherargs(1))
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

    End Class

End Namespace
