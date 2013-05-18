'    Flash Algorithm for Electrolyte solutions
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

Namespace DWSIM.SimulationObjects.PropertyPackages.Auxiliary.FlashAlgorithms

    <System.Serializable()> Public Class ElectrolyteSVLE

        Public proppack As PropertyPackage

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
            Dim Vnf(n), Vnl(n), Vxl(n), Vns(n), Vxs(n), Vnv(n), Vxv(n), Vf(n), V, S, L As Double
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

            If Vnf(wid) = 1.0# Then

                'only water in the stream. calculate vapor pressure and compare it with system pressure to determine the phase.

                If P > Psat Then

                    'liquid only.

                    V = 0.0#
                    L = 1.0#
                    S = 0.0#
                    Vxl(wid) = 1.0#

                Else

                    'vapor only. 

                    V = 1.0#
                    L = 0.0#
                    S = 0.0#
                    Vxv(wid) = 1.0#

                End If

            ElseIf Vnf(wid) = 0.0# Then

                'only solids in the stream (no liquid water).

                V = 0.0#
                L = 0.0#
                S = 1.0#
                For i = 0 To n
                    Vxs(i) = Vnf(i)
                Next

            Else

                'calculate SLE.

                Dim ids As New List(Of String)
                For i = 0 To n
                    ids.Add(CompoundProperties(i).Name)
                Next

                'get the default reaction set.

                Me.ReactionSet = proppack.CurrentMaterialStream.Flowsheet.Options.ReactionSets.First.Key

                Me.Vx0 = Vx.Clone

                Dim int_count As Integer = 0
                Dim L_ant As Double = 0.0#

                Do

                    'calculate chemical equilibria between ions, salts and water. 
                    ''SolveChemicalEquilibria' returns the equilibrium molar amounts in the liquid phase, including precipitates.

                    Vnf = SolveChemicalEquilibria(Vx, T, P, ids).Clone

                    'calculate activity coefficients.

                    If TypeOf proppack Is ExUNIQUACPropertyPackage Then
                        activcoeff = CType(proppack, ExUNIQUACPropertyPackage).m_uni.GAMMA_MR(T, Vnf, CompoundProperties)
                    ElseIf TypeOf proppack Is LIQUAC2PropertyPackage Then
                        activcoeff = CType(proppack, LIQUAC2PropertyPackage).m_uni.GAMMA_MR(T, Vnf, CompoundProperties)
                    End If

                    Dim Vxlmax(n) As Double

                    If P > Vnf(wid) * activcoeff(wid) * Psat Then

                        'water is still on liquid phase. proceed.

                        'calculate maximum solubilities for solids/precipitates.

                        For i = 0 To n
                            If CompoundProperties(i).TemperatureOfFusion <> 0.0# Then
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

                        Dim sumfis, sumlis As Double
                        Dim hassolids As Boolean = False

                        sumfis = 0
                        sumlis = 0
                        For i = 0 To n
                            If Vnf(i) / Sum(Vnf) > Vxlmax(i) Then
                                hassolids = True
                                Vxl(i) = Vxlmax(i)
                                sumfis += Vnf(i)
                                sumlis += Vxl(i)
                            End If
                        Next

                        L_ant = L
                        If hassolids Then L = (1 - sumfis) / (1 - sumlis) Else L = 1
                        S = 1 - L

                        For i = 0 To n
                            If Vnf(i) > Vxlmax(i) Then
                                Vns(i) = Vnf(i) - Vxl(i) * L
                            End If
                            If Vxl(i) <> 0.0# Then
                                Vnl(i) = Vxl(i) * L
                            Else
                                Vnl(i) = Vnf(i)
                            End If
                        Next

                        For i = 0 To n
                            Vxl(i) = Vnl(i) / Sum(Vnl)
                            Vxs(i) = Vns(i) / Sum(Vns)
                        Next


                    Else

                        'water is in vapor phase. all remaining compounds will be treated as solids.
                        'ions???

                        L_ant = L
                        L = 0
                        V = Vnf(wid)
                        Vxl(wid) = 0.0#
                        Vxv(wid) = 1.0#

                        For i = 0 To n
                            If i <> wid Then
                                Vxlmax(i) = 0.0#
                            End If
                        Next

                        'mass balance.

                        Dim sumfis, sumlis As Double
                        Dim hassolids As Boolean = False

                        sumfis = 0
                        sumlis = 0
                        For i = 0 To n
                            If Vnf(i) > Vxlmax(i) And i <> wid Then
                                hassolids = True
                                Vxl(i) = Vxlmax(i)
                                sumfis += Vnf(i)
                                sumlis += Vxl(i)
                            End If
                        Next

                        S = 1 - V

                        For i = 0 To n
                            If Vnf(i) > Vxlmax(i) Then
                                Vns(i) = Vnf(i)
                            End If
                        Next

                        For i = 0 To n
                            Vxs(i) = Vns(i) / Sum(Vns)
                        Next

                    End If

                    If Math.Abs(L - L_ant) < 0.001 Then Exit Do

                    sumN = 0
                    For i = 0 To n
                        Vf(i) = Vnv(i) + Vnl(i) + Vns(i)
                        sumN += Vf(i)
                    Next

                    For i = 0 To n
                        Vx(i) = Vf(i) / sumN
                    Next

                Loop Until int_count > 50

                If int_count > 50 Then
                    Throw New Exception("Chemical Equilibrium Solver error: Reached the maximum number of external iterations without converging.")
                End If

            End If

            

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

        Private Function SolveChemicalEquilibria(ByVal Vx As Array, ByVal T As Double, ByVal P As Double, ByVal ids As List(Of String)) As Array

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

                'initial estimates for the reaction extents

                Dim REx(r) As Double

                For i = 0 To r
                    REx(i) = ubound(i) * 0.5
                Next

                Dim REx0(REx.Length - 1) As Double

                'solve using newton's method

                Dim fx(r), dfdx(r, r), dfdx_ant(r, r), dx(r), x(r), df, fval As Double
                Dim brentsolver As New BrentOpt.BrentMinimize
                brentsolver.DefineFuncDelegate(AddressOf MinimizeError)

                Dim niter As Integer

                Me.T = T
                Me.P = P

                x = REx
                niter = 0
                Do

                    fx = Me.FunctionValue2N(x)

                    If AbsSum(fx) < 0.00001 Then Exit Do

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
                    fval = brentsolver.brentoptimize(0.01, 1.0#, 0.0001, df)

                    For i = 0 To r
                        x(i) -= dx(i) * df
                    Next

                    niter += 1

                    If AbsSum(dx) = 0.0# Then
                        Throw New Exception("Chemical Equilibrium Solver error: No solution found - reached a stationary point of the objective function (singular gradient matrix).")
                    End If

                    If niter > 250 Then
                        Throw New Exception("Chemical Equilibrium Solver error: Reached the maximum number of internal iterations without converging.")
                    End If

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


                Return Vx

            Else

                Return Vx

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

            If TypeOf proppack Is ExUNIQUACPropertyPackage Then
                activcoeff = CType(proppack, ExUNIQUACPropertyPackage).m_uni.GAMMA_MR(T, Vx, CompoundProperties)
            ElseIf TypeOf proppack Is LIQUAC2PropertyPackage Then
                activcoeff = CType(proppack, LIQUAC2PropertyPackage).m_uni.GAMMA_MR(T, Vx, CompoundProperties)
            End If

            Dim CP(nc) As Double
            Dim f(x.Length - 1) As Double
            Dim prod(x.Length - 1) As Double

            For i = 0 To nc
                If CompoundProperties(i).IsIon Or CompoundProperties(i).IsSalt Then
                    CP(i) = molality(i) * activcoeff(i)
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

            Dim pen_val As Double = 0 'ReturnPenaltyValue()

            For i = 0 To Me.Reactions.Count - 1
                With proppack.CurrentMaterialStream.Flowsheet.Options.Reactions(Me.Reactions(i))
                    f(i) = prod(i) - .ConstantKeqValue + pen_val
                    If Double.IsNaN(f(i)) Or Double.IsInfinity(f(i)) Then
                        f(i) = -.ConstantKeqValue + pen_val
                    End If
                End With
            Next

            Return f

        End Function

        Private Function FunctionGradient2N(ByVal x() As Double) As Double(,)

            Dim epsilon As Double = 0.001

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

    End Class

End Namespace

