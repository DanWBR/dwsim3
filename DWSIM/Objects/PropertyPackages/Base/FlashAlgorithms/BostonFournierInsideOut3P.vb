'    Boston-Fournier Inside-Out Three-Phase Flash Algorithm
'    Copyright 2011 Daniel Wagner O. de Medeiros
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

Namespace DWSIM.SimulationObjects.PropertyPackages.Auxiliary.FlashAlgorithms

    <System.Serializable()> Public Class BostonFournierInsideOut3P

        Inherits FlashAlgorithm

        Private _io As New BostonBrittInsideOut

        Dim i, j, k, n, ecount As Integer
        Dim etol As Double = 0.000001
        Dim itol As Double = 0.000001
        Dim maxit_i As Integer = 100
        Dim maxit_e As Integer = 100
        Dim Vn(n) As String
        Dim Vx1(n), Vx2(n), Vy(n), Vp(n), ui1(n), ui2(n), uic1(n), uic2(n), pi(n), Ki1(n), Ki2(n), fi(n), Vt(n), Vpc(n), VTc(n), Vw(n) As Double
        Dim L, L1, L2, beta, Lf, V, Vf, R, Rant, S, Sant, Tant, Pant, T, T_, Tf, P, P_, Pf, T0, P0, A, B, C, D, E, F, Ac, Bc, Cc, Dc, Ec, Fc As Double
        Dim Kb, Kb0, Kb_ As Double
        Dim DHv, DHl, DHv1, DHv2, DHl1, DHl2, Hv0, Hvid, Hlid, Hf, DHlsp, DHvsp As Double
        Dim DSv, DSl, DSv1, DSv2, DSl1, DSl2, Sv0, Svid, Slid, Sf, DSlsp, DSvsp As Double
        Dim Pb, Pd, Pmin, Pmax, Px, soma_x1, soma_x2, soma_y, Tmin, Tmax As Double
        Dim proppack As PropertyPackages.PropertyPackage
        Dim tmpdx, refx, currx As Object

        Public Overrides Function Flash_PT(ByVal Vz() As Double, ByVal P As Double, ByVal T As Double, ByVal PP As PropertyPackage, Optional ByVal ReuseKI As Boolean = False, Optional ByVal PrevKi() As Double = Nothing) As Object

            Dim d1, d2 As Date, dt As TimeSpan

            d1 = Date.Now

            ' try a two-phase flash first.

            Dim result As Object = _io.Flash_PT(Vz, P, T, PP, ReuseKI, PrevKi)

            ' check if there is a liquid phase

            If result(0) > 0 Then ' we have a liquid phase

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
                    Dim m As Double = UBound(stresult(1), 1)
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

                        Dim vx1e(UBound(Vz)), vx2e(UBound(Vz)) As Double

                        Dim maxl As Double = MathEx.Common.Max(vx2est)
                        Dim imaxl As Integer = Array.IndexOf(vx2est, maxl)

                        F = 1
                        V = result(1)
                        L2 = F * result(3)(imaxl)
                        L1 = F - L2 - V

                        If L2 < 0 Then
                            L2 = Abs(L2)
                            L1 = F - L2 - V
                        End If

                        For i = 0 To n
                            If i <> imaxl Then
                                vx1e(i) = Vz(i) / (1 - L2)
                            Else
                                vx1e(i) = Vz(i) * L2
                            End If
                        Next

                        Dim sumvx2 = 0
                        For i = 0 To n
                            sumvx2 += Abs(vx1e(i))
                        Next

                        For i = 0 To n
                            vx1e(i) = Abs(vx1e(i)) / sumvx2
                        Next

                        Try
                            result = Flash_PT_3P(Vz, V, L1, L2, result(3), vx1e, vx2est, P, T, PP)
                        Catch ex As Exception
                            'if there was an error, keep the two-phase result.
                            result = _io.Flash_PT(Vz, P, T, PP, ReuseKI, PrevKi)
                        End Try

                    End If

                End If

            End If

            d2 = Date.Now

            dt = d2 - d1

            Console.WriteLine("PT Flash [IO3P]: Converged in " & ecount & " iterations. Time taken: " & dt.Milliseconds & " ms.")

            Return result

        End Function

        Public Overrides Function Flash_PH(ByVal Vz() As Double, ByVal P As Double, ByVal H As Double, ByVal Tref As Double, ByVal PP As PropertyPackage, Optional ByVal ReuseKI As Boolean = False, Optional ByVal PrevKi() As Double = Nothing) As Object

            Dim d1, d2 As Date, dt As TimeSpan

            d1 = Date.Now

            Dim result As Object = _io.Flash_PH(Vz, P, H, Tref, PP, ReuseKI, PrevKi)

            If result(0) > 0 Then

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

                Dim stresult As Object = StabTest(result(4), P, result(2), PP, Vtrials, Me.StabSearchSeverity)

                If stresult(0) = False Then

                    Dim vx2est(UBound(Vz)) As Double
                    Dim m As Double = UBound(stresult(1), 1)
                    Dim gl, hl, sl, gv, hv, sv, gli As Double

                    If StabSearchSeverity = 2 Then
                        gli = 0
                        For j = 0 To m
                            For i = 0 To UBound(Vz)
                                vx2est(i) = stresult(1)(j, i)
                            Next
                            hl = PP.DW_CalcEnthalpy(vx2est, result(4), P, State.Liquid)
                            sl = PP.DW_CalcEntropy(vx2est, result(4), P, State.Liquid)
                            gl = hl - result(4) * sl
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

                    hl = PP.DW_CalcEnthalpy(vx2est, result(4), P, State.Liquid)
                    sl = PP.DW_CalcEntropy(vx2est, result(4), P, State.Liquid)
                    gl = hl - result(4) * sl

                    hv = PP.DW_CalcEnthalpy(vx2est, result(4), P, State.Vapor)
                    sv = PP.DW_CalcEntropy(vx2est, result(4), P, State.Vapor)
                    gv = hv - result(4) * sv

                    If Abs((gl - gv) / gl) > 0.05 Then 'liquid-like

                        Dim vx1e(UBound(Vz)), vx2e(UBound(Vz)) As Double

                        Dim maxl As Double = MathEx.Common.Max(vx2est)
                        Dim imaxl As Integer = Array.IndexOf(vx2est, maxl)

                        F = 1
                        V = result(1)
                        L1 = (F * Vz(imaxl) - result(3)(imaxl) - F * vx2est(imaxl) + V * vx2est(imaxl)) / (result(2)(imaxl) - vx2est(imaxl))
                        L1 = L1 * (1 - result(2)(imaxl))
                        L2 = F - L1 - V

                        If L2 < 0 Then
                            L2 = Abs(L2)
                            L1 = F - L2 - V
                        End If

                        For i = 0 To n
                            vx1e(i) = (result(2)(i) * L1 - vx2est(i) * L2) / (L1 - L2)
                        Next

                        Dim sumvx2 = 0
                        For i = 0 To n
                            sumvx2 += Abs(vx1e(i))
                        Next

                        For i = 0 To n
                            vx1e(i) = Abs(vx1e(i)) / sumvx2
                        Next

                        result = Flash_PH_3P(Vz, result(1), L1, L2, result(3), vx1e, vx2est, P, H, result(4), PP)

                    End If

                End If

            End If

            d2 = Date.Now

            dt = d2 - d1

            Console.WriteLine("PH Flash [IO3P]: Converged in " & ecount & " iterations. Time taken: " & dt.Milliseconds & " ms.")

            Return result

        End Function

        Public Overrides Function Flash_PS(ByVal Vz() As Double, ByVal P As Double, ByVal S As Double, ByVal Tref As Double, ByVal PP As PropertyPackage, Optional ByVal ReuseKI As Boolean = False, Optional ByVal PrevKi() As Double = Nothing) As Object

            Dim d1, d2 As Date, dt As TimeSpan

            d1 = Date.Now

            Dim result As Object = _io.Flash_PS(Vz, P, S, Tref, PP, ReuseKI, PrevKi)

            If result(0) > 0 Then

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

                Dim stresult As Object = StabTest(result(4), P, result(2), PP, Vtrials, Me.StabSearchSeverity)

                If stresult(0) = False Then

                    Dim vx2est(UBound(Vz)) As Double
                    Dim m As Double = UBound(stresult(1), 1)
                    Dim gl, hl, sl, gv, hv, sv, gli As Double

                    If StabSearchSeverity = 2 Then
                        gli = 0
                        For j = 0 To m
                            For i = 0 To UBound(Vz)
                                vx2est(i) = stresult(1)(j, i)
                            Next
                            hl = PP.DW_CalcEnthalpy(vx2est, result(4), P, State.Liquid)
                            sl = PP.DW_CalcEntropy(vx2est, result(4), P, State.Liquid)
                            gl = hl - result(4) * sl
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

                    hl = PP.DW_CalcEnthalpy(vx2est, result(4), P, State.Liquid)
                    sl = PP.DW_CalcEntropy(vx2est, result(4), P, State.Liquid)
                    gl = hl - result(4) * sl

                    hv = PP.DW_CalcEnthalpy(vx2est, result(4), P, State.Vapor)
                    sv = PP.DW_CalcEntropy(vx2est, result(4), P, State.Vapor)
                    gv = hv - result(4) * sv

                    If Abs((gl - gv) / gl) > 0.05 Then 'liquid-like

                        Dim vx1e(UBound(Vz)), vx2e(UBound(Vz)) As Double

                        Dim maxl As Double = MathEx.Common.Max(vx2est)
                        Dim imaxl As Integer = Array.IndexOf(vx2est, maxl)

                        F = 1
                        V = result(1)
                        L1 = (F * Vz(imaxl) - result(3)(imaxl) - F * vx2est(imaxl) + V * vx2est(imaxl)) / (result(2)(imaxl) - vx2est(imaxl))
                        L1 = L1 * (1 - result(2)(imaxl))
                        L2 = F - L1 - V

                        If L2 < 0 Then
                            L2 = Abs(L2)
                            L1 = F - L2 - V
                        End If

                        For i = 0 To n
                            vx1e(i) = (result(2)(i) * L1 - vx2est(i) * L2) / (L1 - L2)
                        Next

                        Dim sumvx2 = 0
                        For i = 0 To n
                            sumvx2 += Abs(vx1e(i))
                        Next

                        For i = 0 To n
                            vx1e(i) = Abs(vx1e(i)) / sumvx2
                        Next

                        result = Flash_PS_3P(Vz, result(1), L1, L2, result(3), vx1e, vx2est, P, S, result(4), PP)

                    End If

                End If

            End If

            d2 = Date.Now

            dt = d2 - d1

            Console.WriteLine("PS Flash [IO3P]: Converged in " & ecount & " iterations. Time taken: " & dt.Milliseconds & " ms.")

            Return result

        End Function

        Public Overrides Function Flash_TV(ByVal Vz() As Double, ByVal T As Double, ByVal V As Double, ByVal Pref As Double, ByVal PP As PropertyPackage, Optional ByVal ReuseKI As Boolean = False, Optional ByVal PrevKi() As Double = Nothing) As Object

            Dim d1, d2 As Date, dt As TimeSpan

            d1 = Date.Now

            Dim result As Object = _io.Flash_TV(Vz, T, V, Pref, PP, ReuseKI, PrevKi)

            If result(0) > 0 Then

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

                Dim stresult As Object = StabTest(T, P, result(2), PP, Vtrials, Me.StabSearchSeverity)

                If stresult(0) = False Then

                    Dim vx2est(UBound(Vz)) As Double
                    Dim m As Double = UBound(stresult(1), 1)
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

                    If gl < gv Then 'liquid-like
                        result = Flash_TV_3P(Vz, result(1), result(0) / 2, result(0) / 2, result(3), result(2), vx2est, T, V, result(4), PP)
                    End If

                End If

            End If

            d2 = Date.Now

            dt = d2 - d1

            Console.WriteLine("TV Flash [IO3P]: Converged in " & ecount & " iterations. Time taken: " & dt.Milliseconds & " ms.")

            Return result

        End Function

        Public Overrides Function Flash_PV(ByVal Vz() As Double, ByVal P As Double, ByVal V As Double, ByVal Tref As Double, ByVal PP As PropertyPackage, Optional ByVal ReuseKI As Boolean = False, Optional ByVal PrevKi() As Double = Nothing) As Object

            Dim d1, d2 As Date, dt As TimeSpan

            d1 = Date.Now

            Dim result As Object = _io.Flash_PV(Vz, P, V, Tref, PP, ReuseKI, PrevKi)

            If result(0) > 0 Then

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

                Dim stresult As Object = StabTest(T, P, result(2), PP, Vtrials, Me.StabSearchSeverity)

                If stresult(0) = False Then

                    Dim vx2est(UBound(Vz)) As Double
                    Dim m As Double = UBound(stresult(1), 1)
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

                    If gl < gv Then 'liquid-like
                        result = Flash_PV_3P(Vz, result(1), result(0) / 2, result(0) / 2, result(3), result(2), vx2est, P, V, result(4), PP)
                    End If

                End If

            End If

            d2 = Date.Now

            dt = d2 - d1

            Console.WriteLine("TV Flash [IO3P]: Converged in " & ecount & " iterations. Time taken: " & dt.Milliseconds & " ms.")

            Return result

        End Function

        Private Function CalcKbj1(ByVal K() As Double) As Double

            Dim i As Integer
            Dim n As Integer = UBound(K) - 1

            Dim Kbj1 As Object

            Kbj1 = K(0)
            For i = 1 To n
                If Abs(K(i) - 1) < Abs(Kbj1 - 1) Then Kbj1 = K(i)
            Next

            Return Kbj1

        End Function

        Private Function CalcKbjw(ByVal K1() As Double, ByVal K2() As Double, ByVal L1 As Double, ByVal L2 As Double) As Double

            Dim i As Integer
            Dim n As Integer = UBound(K1) - 1

            Dim Kbj1 As Object

            Kbj1 = L1 * K1(0) + L2 * K2(0)
            For i = 1 To n
                If Abs(K1(i) - 1) < Abs(Kbj1 - 1) Then Kbj1 = L1 * K1(i) + L2 * K2(i)
            Next

            Return Kbj1

        End Function

        Private Function EnergyBalance(ByVal R As Double) As Double

            Dim fr, dfr, S0, S1 As Double
            Dim icount As Integer = 0

            S = 1 - R
            Do
                S0 = S
                S1 = S + 0.0001
                fr = Me.SErrorFunc(S0, R)
                dfr = (fr - Me.SErrorFunc(S1, R)) / -0.0001
                S += -0.3 * fr / dfr
                icount += 1
            Loop Until Abs(fr) < itol Or icount > maxit_i

            If S <= -(1 - R) Then S = -(1 - R)
            If S >= (1 - R) Then S = (1 - R)

            For i = 0 To n
                pi(i) = fi(i) / (R + (1 - R + S) / (2 * Kb0 * Exp(ui1(i))) + (1 - R - S) / (2 * Kb0 * Exp(ui2(i))))
            Next

            Dim sumpi As Double = 0
            Dim sumeuipi1 As Double = 0
            Dim sumeuipi2 As Double = 0
            For i = 0 To n
                sumpi += pi(i)
                sumeuipi1 += pi(i) / Exp(ui1(i))
                sumeuipi2 += pi(i) / Exp(ui2(i))
            Next
            For i = 0 To n
                Vx1(i) = (pi(i) / Exp(ui1(i))) / sumeuipi1
                Vx2(i) = (pi(i) / Exp(ui2(i))) / sumeuipi2
                Vy(i) = pi(i) / sumpi
            Next

            If R <> 1.0# Then
                Kb = ((1 - R + S) * sumeuipi1 + (1 - R - S) * sumeuipi2) / (2 * (1 - R) * sumpi)
            Else
                Kb = 1.0#
            End If

            V = R * sumpi
            L1 = 0.5 * (S * V * (Kb / Kb0 - 1) + (1 + S) - V)
            L2 = 1 - L1 - V
            beta = L1 / (L1 + L2)
            L = L1 + L2

            Dim eberror As Double

            T = 1 / T_ + (Log(Kb) - A) / B
            T = 1 / T
            If T < Tmin Then T = Tmin
            If T > Tmax Then T = Tmax

            Dim Hv, Hl1, Hl2 As Double

            Hv = proppack.DW_CalcEnthalpy(Vy, T, Pf, State.Vapor) * proppack.AUX_MMM(Vy)
            Hl1 = proppack.DW_CalcEnthalpy(Vx1, T, Pf, State.Liquid) * proppack.AUX_MMM(Vx1)
            Hl2 = proppack.DW_CalcEnthalpy(Vx2, T, Pf, State.Liquid) * proppack.AUX_MMM(Vx2)
            eberror = Hf - V * Hv - L1 * Hl1 - L2 * Hl2

            CheckCalculatorStatus()

            Return eberror

        End Function

        Private Function EntropyBalance(ByVal R As Double) As Double

            Dim fr, dfr, S0, S1 As Double
            Dim icount As Integer = 0

            S = (1 - R)
            Do
                S0 = S
                S1 = S + 0.001
                fr = Me.SErrorFunc(S0, R)
                dfr = (fr - Me.SErrorFunc(S1, R)) / -0.001
                S += -fr / dfr
                icount += 1
            Loop Until Abs(fr) < itol Or icount > maxit_i

            If S <= -(1 - R) Then S = -(1 - R)
            If S >= (1 - R) Then S = (1 - R)

            For i = 0 To n
                pi(i) = fi(i) / (R + (1 - R + S) / (2 * Kb0 * Exp(ui1(i))) + (1 - R - S) / (2 * Kb0 * Exp(ui2(i))))
            Next

            Dim sumpi As Double = 0
            Dim sumeuipi1 As Double = 0
            Dim sumeuipi2 As Double = 0
            For i = 0 To n
                sumpi += pi(i)
                sumeuipi1 += pi(i) / Exp(ui1(i))
                sumeuipi2 += pi(i) / Exp(ui2(i))
            Next
            For i = 0 To n
                Vx1(i) = (pi(i) / Exp(ui1(i))) / sumeuipi1
                Vx2(i) = (pi(i) / Exp(ui2(i))) / sumeuipi2
                Vy(i) = pi(i) / sumpi
            Next

            If R <> 1 Then
                Kb = ((1 - R + S) * sumeuipi1 + (1 - R - S) * sumeuipi2) / (2 * (1 - R) * sumpi)
            Else
                Kb = 1.0#
            End If

            V = R * sumpi
            L1 = 0.5 * (S * V * (Kb / Kb0 - 1) + (1 + S) - V)
            L2 = 1 - L1 - V
            beta = L1 / (L1 + L2)

            T = 1 / T_ + (Log(Kb) - A) / B
            T = 1 / T

            If T < Tmin Then T = Tmin
            If T > Tmax Then T = Tmax

            Dim Sv, Sl1, Sl2 As Double

            Sv = proppack.DW_CalcEntropy(Vy, T, Pf, State.Vapor) * proppack.AUX_MMM(Vy)
            Sl1 = proppack.DW_CalcEntropy(Vx1, T, Pf, State.Liquid) * proppack.AUX_MMM(Vx1)
            Sl2 = proppack.DW_CalcEntropy(Vx2, T, Pf, State.Liquid) * proppack.AUX_MMM(Vx2)

            Dim eberror As Double = Sf - V * Sv - L1 * Sl1 - L2 * Sl2

            CheckCalculatorStatus()

            Return eberror

        End Function

        Public Function Flash_PH_3P(ByVal Vz() As Double, ByVal Vest As Double, ByVal L1est As Double, ByVal L2est As Double, ByVal VyEST() As Double, ByVal Vx1EST() As Double, ByVal Vx2EST() As Double, ByVal P As Double, ByVal H As Double, ByVal Tref As Double, ByVal PP As PropertyPackage) As Object

            maxit_i = CInt(PP.Parameters("PP_PHFMII"))
            maxit_e = CInt(PP.Parameters("PP_PHFMEI"))
            itol = CDbl(PP.Parameters("PP_PHFILT"))
            etol = CDbl(PP.Parameters("PP_PHFELT"))

            n = UBound(Vz)

            proppack = PP
            Hf = H * PP.AUX_MMM(Vz)
            Pf = P

            ReDim Vn(n), Vx1(n), Vx2(n), Vy(n), Vp(n), ui1(n), uic1(n), ui2(n), uic2(n), pi(n), Ki1(n), Ki2(n), fi(n), Vpc(n), VTc(n), Vw(n)

            Vn = PP.RET_VNAMES()
            fi = Vz.Clone

            Tmin = 0
            Tmax = 0
            If Tref = 0 Or Double.IsNaN(Tref) Then
                i = 0
                Tref = 0
                Do
                    Tref += 0.8 * Vz(i) * VTc(i)
                    Tmin += 0.1 * Vz(i) * VTc(i)
                    Tmax += 2.0 * Vz(i) * VTc(i)
                    i += 1
                Loop Until i = n + 1
            Else
                Tmin = Tref - 200
                Tmax = Tref + 200
            End If
            If Tmin < 100 Then Tmin = 100

            '--------------------------------------
            ' STEP 1 - Assume u, A, B, C, D, E, F 
            '--------------------------------------

            T = Tref + 0.1
            T_ = Tref - 0.1
            T0 = Tref

            '----------------------------------------
            ' STEP 1.1 - Estimate K, Vx, Vy, V and L 
            '----------------------------------------

            'Calculate Ki`s

            Ki1 = PP.DW_CalcKvalue(Vx1EST, VyEST, T, P, "LV")
            Ki2 = PP.DW_CalcKvalue(Vx2EST, VyEST, T, P, "LV")

            'Estimate V

            Kb_ = L1est * CalcKbj1(PP.DW_CalcKvalue(Vx1EST, VyEST, T_, P)) + L2est * CalcKbj1(PP.DW_CalcKvalue(Vx2EST, VyEST, T_, P))
            Kb = L1est * CalcKbj1(PP.DW_CalcKvalue(Vx1EST, VyEST, T, P)) + L2est * CalcKbj1(PP.DW_CalcKvalue(Vx2EST, VyEST, T, P))
            Kb0 = Kb_

            B = Log(Kb_ / Kb) / (1 / T_ - 1 / T)
            A = Log(Kb) - B * (1 / T - 1 / T_)

            For i = 0 To n
                ui1(i) = Log(Ki1(i) / Kb)
                ui2(i) = Log(Ki2(i) / Kb)
            Next

            Dim fx(2 * n + 3), x(2 * n + 3), dfdx(2 * n + 3, 2 * n + 3), dx(2 * n + 3), xbr(2 * n + 3), fbr(2 * n + 3), fr As Double

            ecount = 0

            V = Vest
            L1 = L1est
            L2 = L2est
            L = L1 + L2
            beta = L1 / L2

            R = Kb * V / (Kb * V + Kb0 * L)

restart:    Do

                '--------------------------------------------------------------
                ' STEPS 2, 3, 4, 5, 6, 7 and 8 - Calculate R and Energy Balance
                '--------------------------------------------------------------

                Rant = R
                Tant = T

                Dim dfr, R0, R1 As Double
                Dim icount As Integer = 0

                Do
                    R0 = R
                    If R > 0.99 Then
                        R1 = R - 0.0001
                        fr = Me.EnergyBalance(R0)
                        dfr = (fr - Me.EnergyBalance(R1)) / 0.0001
                    Else
                        R1 = R + 0.0001
                        fr = Me.EnergyBalance(R0)
                        dfr = (fr - Me.EnergyBalance(R1)) / -0.0001
                    End If
                    R0 = R
                    If Abs(fr) < itol Then Exit Do
                    R += -0.3 * fr / dfr
                    If R < 0 Then R = 0.0#
                    If R > 1 Then R = 1.0#
                    icount += 1
                Loop Until icount > maxit_i Or Abs(R - R0) < 0.000001

                'At this point, we have converged T and R for the simplified model. Proceed to step 9.

                '----------------------------------------------------------
                ' STEP 9 - Rigorous model Enthalpy and K-values calculation
                '----------------------------------------------------------

                Me.EnergyBalance(R)

                Ki1 = PP.DW_CalcKvalue(Vx1, Vy, T, P)
                Ki2 = PP.DW_CalcKvalue(Vx2, Vy, T, P)

                For i = 0 To n
                    If Ki1(i) <> 0 Then
                        uic1(i) = Log(Ki1(i) / Kb)
                    Else
                        uic1(i) = ui1(i)
                    End If
                    If Ki2(i) <> 0 Then
                        uic2(i) = Log(Ki2(i) / Kb)
                    Else
                        uic2(i) = ui2(i)
                    End If
                Next

                Bc = Log(Kb_ / Kb) / (1 / T_ - 1 / T)
                Ac = Log(Kb) - Bc * (1 / T - 1 / T_)

                '-------------------------------------------
                ' STEP 10 - Update variables using Broyden
                '-------------------------------------------

                For i = 0 To n
                    fx(i) = (ui1(i) - uic1(i))
                    x(i) = ui1(i)
                Next

                For i = n + 1 To 2 * n + 1
                    fx(i) = (ui2(i - n - 1) - uic2(i - n - 1))
                    x(i) = ui2(i - n - 1)
                Next

                If PP._ioquick Then

                    fx(2 * n + 2) = (A - Ac)
                    fx(2 * n + 3) = (B - Bc)
                    x(2 * n + 2) = A
                    x(2 * n + 3) = B

                    If ecount = 0 Then
                        For i = 0 To 2 * n + 3
                            For j = 0 To 2 * n + 3
                                If i = j Then dfdx(i, j) = 1 Else dfdx(i, j) = 0
                            Next
                        Next
                        broydn(2 * n + 3, x, fx, dx, xbr, fbr, dfdx, 0)
                    Else
                        broydn(2 * n + 3, x, fx, dx, xbr, fbr, dfdx, 1)
                    End If

                    For i = 0 To n
                        ui1(i) += dx(i)
                    Next
                    For i = n + 1 To 2 * n + 1
                        ui2(i - n - 1) += dx(i)
                    Next

                    A += dx(2 * n + 2)
                    B += dx(2 * n + 3)

                Else

                    For i = 0 To n
                        ui1(i) = uic1(i)
                        ui2(i) = uic2(i)
                    Next
                    A = Ac
                    B = Bc

                End If

                ecount += 1

                If ecount > maxit_e Then Throw New Exception(DWSIM.App.GetLocalString("PropPack_FlashMaxIt"))
                If Double.IsNaN(AbsSum(fx)) Then Throw New Exception(DWSIM.App.GetLocalString("PropPack_FlashError"))

                Console.WriteLine("PH Flash 3P [IO]: Iteration #" & ecount & ", T = " & T)
                Console.WriteLine("PH Flash 3P [IO]: Iteration #" & ecount & ", VF = " & V)
                Console.WriteLine("PH Flash 3P [IO]: Iteration #" & ecount & ", L1F = " & L1)
                Console.WriteLine("PH Flash 3P [IO]: Iteration #" & ecount & ", L2F = " & L2)
                Console.WriteLine("PH Flash 3P [IO]: Iteration #" & ecount & ", H error = " & fr)

                CheckCalculatorStatus()

            Loop Until AbsSum(fx) < etol

            If Abs(fr) > itol Then
                If V <= 0.001 Then
                    'single phase solution found (liquid only). Obtain T using single phase calculation.
                    Dim ic As Integer
                    Dim bo As New BrentOpt.Brent
                    bo.DefineFuncDelegate(AddressOf EnergyBalanceSPL)
                    ic = 0
                    Do
                        Tant = T
                        T = bo.BrentOpt(Tmin, Tmax, 5, 0.1, maxit_i, Nothing)
                        Me.Flash_PT_3P(fi, V, L1, L2, Vy, Vx1, Vx2, P, T, PP)
                        ic += 1
                    Loop Until Abs(T - Tant) < 0.1 Or ic > 10
                Else
                    'single phase solution found (vapor only). Obtain T using single phase calculation.
                    Dim x1, fx2, dfdx2 As Double
                    ecount = 0
                    If Tref = 0 Then Tref = 298.15
                    x1 = Tref
                    Do
                        fx2 = EnergyBalanceSPV(x1, Nothing)
                        If Math.Abs(fx2) < etol Then Exit Do
                        dfdx2 = (EnergyBalanceSPV(x1 + 1, Nothing) - fx2)
                        x1 = x1 - fx2 / dfdx2
                        ecount += 1
                    Loop Until ecount > maxit_e Or Double.IsNaN(x1)
                    T = x1
                    Vy = Vz
                End If
            End If

            Return New Object() {L1, V, Vx1, Vy, T, ecount, Ki1, L2, Vx2, Ki2}

        End Function

        Private Function EnergyBalanceSPL(ByVal T As Double, ByVal otherargs As Object) As Double

            CheckCalculatorStatus()

            Dim Hl1, Hl2, balerror As Double

            Hl1 = proppack.DW_CalcEnthalpy(Vx1, T, Pf, State.Liquid) * proppack.AUX_MMM(Vx1)
            Hl2 = proppack.DW_CalcEnthalpy(Vx2, T, Pf, State.Liquid) * proppack.AUX_MMM(Vx2)

            balerror = Hf - L1 * Hl1 - L2 * Hl2

            Console.WriteLine("PH Flash [IO]: Iteration #" & ecount & ", T = " & T)
            Console.WriteLine("PH Flash [IO]: Iteration #" & ecount & ", L = 1 (SP)")
            Console.WriteLine("PH Flash [IO]: Iteration #" & ecount & ", H error = " & balerror)

            Return balerror

        End Function

        Private Function EnergyBalanceSPV(ByVal T As Double, ByVal otherargs As Object) As Double

            CheckCalculatorStatus()

            Dim HV, balerror As Double

            HV = proppack.DW_CalcEnthalpy(fi, T, Pf, PropertyPackages.State.Vapor) * proppack.AUX_MMM(fi)

            balerror = Hf - HV

            Console.WriteLine("PH Flash [IO]: Iteration #" & ecount & ", T = " & T)
            Console.WriteLine("PH Flash [IO]: Iteration #" & ecount & ", L1 = 1 (SP)")
            Console.WriteLine("PH Flash [IO]: Iteration #" & ecount & ", H error = " & balerror)

            Return balerror


        End Function

        Public Function Flash_PS_3P(ByVal Vz() As Double, ByVal Vest As Double, ByVal L1est As Double, ByVal L2est As Double, ByVal VyEST() As Double, ByVal Vx1EST() As Double, ByVal Vx2EST() As Double, ByVal P As Double, ByVal S As Double, ByVal Tref As Double, ByVal PP As PropertyPackage) As Object

            maxit_i = CInt(PP.Parameters("PP_PSFMII"))
            maxit_e = CInt(PP.Parameters("PP_PSFMEI"))
            itol = CDbl(PP.Parameters("PP_PSFILT"))
            etol = CDbl(PP.Parameters("PP_PSFELT"))

            n = UBound(Vz)

            proppack = PP
            Sf = S * PP.AUX_MMM(Vz)
            Pf = P

            ReDim Vn(n), Vx1(n), Vx2(n), Vy(n), Vp(n), ui1(n), uic1(n), ui2(n), uic2(n), pi(n), Ki1(n), Ki2(n), fi(n), Vpc(n), VTc(n), Vw(n)

            Vn = PP.RET_VNAMES()
            fi = Vz.Clone

            Tmin = 0
            Tmax = 0
            If Tref = 0 Or Double.IsNaN(Tref) Then
                i = 0
                Tref = 0
                Do
                    Tref += 0.8 * Vz(i) * VTc(i)
                    Tmin += 0.1 * Vz(i) * VTc(i)
                    Tmax += 2.0 * Vz(i) * VTc(i)
                    i += 1
                Loop Until i = n + 1
            Else
                Tmin = Tref - 200
                Tmax = Tref + 200
            End If
            If Tmin < 100 Then Tmin = 100

            '--------------------------------------
            ' STEP 1 - Assume u, A, B, C, D, E, F 
            '--------------------------------------

            T = Tref + 0.1
            T_ = Tref - 0.1
            T0 = Tref

            '----------------------------------------
            ' STEP 1.1 - Estimate K, Vx, Vy, V and L 
            '----------------------------------------

            'Calculate Ki`s

            Ki1 = PP.DW_CalcKvalue(Vx1EST, VyEST, T, P, "LV")
            Ki2 = PP.DW_CalcKvalue(Vx2EST, VyEST, T, P, "LV")

            'Estimate V

            Kb_ = CalcKbj1(PP.DW_CalcKvalue(Vx1EST, VyEST, T_, P))
            Kb = CalcKbj1(PP.DW_CalcKvalue(Vx1EST, VyEST, T, P))
            Kb0 = Kb_

            B = Log(Kb_ / Kb) / (1 / T_ - 1 / T)
            A = Log(Kb) - B * (1 / T - 1 / T_)

            For i = 0 To n
                ui1(i) = Log(Ki1(i) / Kb)
                ui2(i) = Log(Ki2(i) / Kb)
            Next

            Dim fx(2 * n + 3), x(2 * n + 3), dfdx(2 * n + 3, 2 * n + 3), dx(2 * n + 3), xbr(2 * n + 3), fbr(2 * n + 3), fr As Double

            ecount = 0

            V = Vest
            L1 = L1est
            L2 = L2est
            L = L1 + L2
            beta = L1 / L2

            R = Kb * V / (Kb * V + Kb0 * L)

restart:    Do

                '--------------------------------------------------------------
                ' STEPS 2, 3, 4, 5, 6, 7 and 8 - Calculate R and Energy Balance
                '--------------------------------------------------------------

                Rant = R
                Tant = T

                Dim dfr, R0, R1 As Double
                Dim icount As Integer = 0

                Do
                    R0 = R
                    If R > 0.999 Then
                        R1 = R - 0.0001
                        fr = Me.EntropyBalance(R0)
                        dfr = (fr - Me.EntropyBalance(R1)) / 0.0001
                    Else
                        R1 = R + 0.0001
                        fr = Me.EntropyBalance(R0)
                        dfr = (fr - Me.EntropyBalance(R1)) / -0.0001
                    End If
                    R0 = R
                    If Abs(fr) < itol Then Exit Do
                    R += -0.3 * fr / dfr
                    If R < 0 Then R = 0.0#
                    If R > 1 Then R = 1.0#
                    icount += 1
                Loop Until icount > maxit_i 'Or Abs(R - R0) < 0.000001

                'At this point, we have converged T and R for the simplified model. Proceed to step 9.

                '----------------------------------------------------------
                ' STEP 9 - Rigorous model Enthalpy and K-values calculation
                '----------------------------------------------------------

                Me.EntropyBalance(R)

                Ki1 = PP.DW_CalcKvalue(Vx1, Vy, T, P)
                Ki2 = PP.DW_CalcKvalue(Vx2, Vy, T, P)


                For i = 0 To n
                    If Ki1(i) <> 0 Then
                        uic1(i) = Log(Ki1(i) / Kb)
                    Else
                        uic1(i) = ui1(i)
                    End If
                    If Ki2(i) <> 0 Then
                        uic2(i) = Log(Ki2(i) / Kb)
                    Else
                        uic2(i) = ui2(i)
                    End If
                Next

                Bc = Log(Kb_ / Kb) / (1 / T_ - 1 / T)
                Ac = Log(Kb) - Bc * (1 / T - 1 / T_)

                '-------------------------------------------
                ' STEP 10 - Update variables using Broyden
                '-------------------------------------------

                For i = 0 To n
                    fx(i) = (ui1(i) - uic1(i))
                    x(i) = ui1(i)
                Next

                For i = n + 1 To 2 * n + 1
                    fx(i) = (ui2(i - n - 1) - uic2(i - n - 1))
                    x(i) = ui2(i - n - 1)
                Next

                If PP._ioquick Then

                    fx(2 * n + 2) = (A - Ac)
                    fx(2 * n + 3) = (B - Bc)
                    x(2 * n + 2) = A
                    x(2 * n + 3) = B

                    If ecount = 0 Then
                        For i = 0 To 2 * n + 3
                            For j = 0 To 2 * n + 3
                                If i = j Then dfdx(i, j) = 1 Else dfdx(i, j) = 0
                            Next
                        Next
                        broydn(2 * n + 3, x, fx, dx, xbr, fbr, dfdx, 0)
                    Else
                        broydn(2 * n + 3, x, fx, dx, xbr, fbr, dfdx, 1)
                    End If

                    For i = 0 To n
                        ui1(i) = ui1(i) + dx(i)
                    Next
                    For i = n + 1 To 2 * n + 1
                        ui2(i - n - 1) = ui2(i - n - 1) + dx(i)
                    Next

                    A += dx(2 * n + 2)
                    B += dx(2 * n + 3)

                Else

                    For i = 0 To n
                        ui1(i) = uic1(i)
                        ui2(i) = uic2(i)
                    Next
                    A = Ac
                    B = Bc

                End If

                ecount += 1

                If ecount > maxit_e Then Throw New Exception(DWSIM.App.GetLocalString("PropPack_FlashMaxIt"))
                If Double.IsNaN(AbsSum(fx)) Then Throw New Exception(DWSIM.App.GetLocalString("PropPack_FlashError"))

                Console.WriteLine("PS Flash 3P [IO]: Iteration #" & ecount & ", T = " & T)
                Console.WriteLine("PS Flash 3P [IO]: Iteration #" & ecount & ", VF = " & V)
                Console.WriteLine("PS Flash 3P [IO]: Iteration #" & ecount & ", S error = " & fr)

                CheckCalculatorStatus()

            Loop Until AbsSum(fx) < etol

            If Abs(fr) > itol Then
                If V <= 0.001 Then
                    'single phase solution found (liquid only). Obtain T using single phase calculation.
                    Dim bo As New BrentOpt.Brent
                    Dim ic As Integer
                    bo.DefineFuncDelegate(AddressOf EntropyBalanceSPL)
                    T = bo.BrentOpt(Tmin, Tmax, 5, 0.1, maxit_i, Nothing)
                    ic = 0
                    Do
                        Tant = T
                        T = bo.BrentOpt(Tmin, Tmax, 5, 0.1, maxit_i, Nothing)
                        Me.Flash_PT_3P(fi, V, L1, L2, Vy, Vx1, Vx2, P, T, PP)
                        ic += 1
                    Loop Until Abs(T - Tant) < 0.01 Or ic > 10
                Else
                    'single phase solution found (vapor only). Obtain T using single phase calculation.
                    Dim x1, fx2, dfdx2 As Double
                    ecount = 0
                    If Tref = 0 Then Tref = 298.15
                    x1 = Tref
                    Do
                        fx2 = EntropyBalanceSPV(x1, Nothing)
                        If Math.Abs(fx2) < etol Then Exit Do
                        dfdx2 = (EntropyBalanceSPV(x1 + 1, Nothing) - fx2)
                        x1 = x1 - fx2 / dfdx2
                        ecount += 1
                    Loop Until ecount > maxit_e Or Double.IsNaN(x1)
                    T = x1
                    Vy = Vz
                End If
                'confirm single-phase solution with a PT Flash.
                Dim res As Object = Me.Flash_PT(Vz, P, T, PP, False, Nothing)
                If Abs(L - res(0)) > 0.0001 And Abs(V - res(1)) > 0.0001 Then
                    'NOT SP solution. go back to 2-phase loop.
                    GoTo restart
                End If
            End If

            Return New Object() {L1, V, Vx1, Vy, T, ecount, Ki1, L2, Vx2, Ki2}

        End Function

        Private Function EntropyBalanceSPL(ByVal T As Double, ByVal otherargs As Object) As Double

            CheckCalculatorStatus()

            Dim Sl1, Sl2, balerror As Double

            Sl1 = proppack.DW_CalcEntropy(Vx1, T, Pf, State.Liquid) * proppack.AUX_MMM(Vx1)
            Sl2 = proppack.DW_CalcEntropy(Vx2, T, Pf, State.Liquid) * proppack.AUX_MMM(Vx2)

            balerror = Sf - L1 * Sl1 - L2 * Sl2

            Console.WriteLine("PS 3P Flash [IO]: Iteration #" & ecount & ", T = " & T)
            Console.WriteLine("PS 3P Flash [IO]: Iteration #" & ecount & ", L = 1 (SP)")
            Console.WriteLine("PS 3P Flash [IO]: Iteration #" & ecount & ", S error = " & balerror)

            Return balerror

        End Function

        Private Function EntropyBalanceSPV(ByVal T As Double, ByVal otherargs As Object) As Double

            CheckCalculatorStatus()

            Dim SV, balerror As Double

            SV = proppack.DW_CalcEnthalpy(fi, T, Pf, PropertyPackages.State.Vapor) * proppack.AUX_MMM(fi)

            balerror = Sf - SV

            Console.WriteLine("PS Flash [IO]: Iteration #" & ecount & ", T = " & T)
            Console.WriteLine("PS Flash [IO]: Iteration #" & ecount & ", V = 1 (SP)")
            Console.WriteLine("PS Flash [IO]: Iteration #" & ecount & ", S error = " & balerror)

            Return balerror


        End Function

        Public Function Flash_PT_3P(ByVal Vz As Double(), ByVal Vest As Double, ByVal L1est As Double, ByVal L2est As Double, ByVal VyEST As Double(), ByVal Vx1EST As Double(), ByVal Vx2EST As Double(), ByVal P As Double, ByVal T As Double, ByVal PP As PropertyPackages.PropertyPackage) As Object

            etol = CDbl(PP.Parameters("PP_PTFELT"))
            maxit_e = CInt(PP.Parameters("PP_PTFMEI"))
            itol = CDbl(PP.Parameters("PP_PTFILT"))
            maxit_i = CInt(PP.Parameters("PP_PTFMII"))

            n = UBound(Vz)

            proppack = PP

            ReDim Vn(n), Vx1(n), Vx2(n), Vy(n), Vp(n), ui1(n), ui2(n), uic1(n), uic2(n), pi(n), Ki1(n), Ki2(n), fi(n)

            Vn = PP.RET_VNAMES()
            fi = Vz.Clone

            '--------------------------------------
            ' STEP 1 - Assume u, A, B, C, D, E, F 
            '--------------------------------------


            '----------------------------------------
            ' STEP 1.1 - Estimate K, Vx, Vy, V and L 
            '----------------------------------------

            'Calculate Ki`s

            Ki1 = PP.DW_CalcKvalue(Vx1EST, VyEST, T, P)
            Ki2 = PP.DW_CalcKvalue(Vx2EST, VyEST, T, P)

            If n = 0 Then
                If Vp(0) <= P Then
                    L = 1
                    V = 0
                    Vx1 = Vz
                    GoTo out
                Else
                    L = 0
                    V = 1
                    Vy = Vz
                    GoTo out
                End If
            End If

            i = 0
            Do
                If Vz(i) <> 0 Then
                    Vy(i) = VyEST(i)
                    Vx1(i) = Vx1EST(i)
                    Vx2(i) = Vx2EST(i)
                Else
                    Vy(i) = 0
                    Vx1(i) = 0
                    Vx2(i) = 0
                End If
                i += 1
            Loop Until i = n + 1

            i = 0
            soma_x1 = 0
            soma_x2 = 0
            soma_y = 0
            Do
                soma_x1 = soma_x1 + Vx1(i)
                soma_x2 = soma_x2 + Vx2(i)
                soma_y = soma_y + Vy(i)
                i = i + 1
            Loop Until i = n + 1
            i = 0
            Do
                Vx1(i) = Vx1(i) / soma_x1
                Vx2(i) = Vx2(i) / soma_x2
                Vy(i) = Vy(i) / soma_y
                i = i + 1
            Loop Until i = n + 1

            Kb = CalcKbjw(Ki1, Ki2, L1est, L2est)
            Kb0 = Kb

            For i = 0 To n
                ui1(i) = Log(Ki1(i))
                ui2(i) = Log(Ki2(i))
            Next

            Dim fx(2 * n + 1), x(2 * n + 1), dfdx(2 * n + 1, 2 * n + 1), dx(2 * n + 1), xbr(2 * n + 1), fbr(2 * n + 1) As Double
            Dim bo As New BrentOpt.Brent
            Dim bo2 As New BrentOpt.BrentMinimize

            ecount = 0

            V = Vest
            L1 = L1est
            L2 = L2est
            L = L1 + L2
            beta = L1 / L2

            R = Kb * V / (Kb * V + Kb0 * L)

            Do

                '--------------------------------------------------------------
                ' STEPS 2, 3, 4, 5, 6, 7 and 8 - Calculate R and Energy Balance
                '--------------------------------------------------------------

                Rant = R
                R = Kb * V / (Kb * V + Kb0 * L)

                Dim fr, dfr, R0, R1 As Double
                Dim icount As Integer = 0

                Do
                    R0 = R
                    If R > 0.999 Then
                        R1 = R - 0.0001
                        fr = Me.TPErrorFunc(R0)
                        dfr = (fr - Me.TPErrorFunc(R1)) / 0.0001
                    Else
                        R1 = R + 0.0001
                        fr = Me.TPErrorFunc(R0)
                        dfr = (fr - Me.TPErrorFunc(R1)) / (-0.0001)
                    End If
                    R0 = R
                    R += -fr / dfr
                    If R < 0 Then R = 0
                    If R > 1 Then R = 1
                    icount += 1
                Loop Until Abs(fr) < itol Or icount > maxit_i Or R = 0 Or R = 1

                Me.TPErrorFunc(R)

                'At this point, we have converged R for the simplified model. Proceed to step 9.

                '----------------------------------------------------------
                ' STEP 9 - Rigorous model Enthalpy and K-values calculation
                '----------------------------------------------------------

                Ki1 = PP.DW_CalcKvalue(Vx1, Vy, T, P)
                Ki2 = PP.DW_CalcKvalue(Vx2, Vy, T, P)
                Kb = CalcKbjw(Ki1, Ki2, L1, L2)

                For i = 0 To n
                    uic1(i) = Log(Ki1(i))
                    uic2(i) = Log(Ki2(i))
                Next

                '-------------------------------------------
                ' STEP 10 - Update variables using Broyden
                '-------------------------------------------

                For i = 0 To n
                    fx(i) = (ui1(i) - uic1(i))
                    x(i) = ui1(i)
                Next

                For i = n + 1 To 2 * n + 1
                    fx(i) = (ui2(i - n - 1) - uic2(i - n - 1))
                    x(i) = ui2(i - n - 1)
                Next

                If PP._ioquick Then

                    If ecount = 0 Then
                        For i = 0 To 2 * n + 1
                            For j = 0 To 2 * n + 1
                                If i = j Then dfdx(i, j) = 1 Else dfdx(i, j) = 0
                            Next
                        Next
                        broydn(2 * n + 1, x, fx, dx, xbr, fbr, dfdx, 0)
                    Else
                        broydn(2 * n + 1, x, fx, dx, xbr, fbr, dfdx, 1)
                    End If

                    For i = 0 To n
                        ui1(i) = ui1(i) + dx(i)
                    Next
                    For i = n + 1 To 2 * n + 1
                        ui2(i - n - 1) = ui2(i - n - 1) + dx(i)
                    Next

                Else

                    For i = 0 To n
                        ui1(i) = uic1(i)
                        ui2(i) = uic2(i)
                    Next

                End If

                ecount += 1

                If Double.IsNaN(V) Then Throw New Exception(DWSIM.App.GetLocalString("PropPack_FlashTPVapFracError"))
                If ecount > maxit_e Then Throw New Exception(DWSIM.App.GetLocalString("PropPack_FlashMaxIt2"))

                Console.WriteLine("PT Flash [IO]: Iteration #" & ecount & ", VF = " & V)

                CheckCalculatorStatus()

            Loop Until AbsSum(fx) < etol

out:        Return New Object() {L1, V, Vx1, Vy, ecount, L2, Vx2}

        End Function

        Private Function TPErrorFunc(ByVal Rt As Double) As Double

            Dim fr, dfr, S0, S1 As Double
            Dim icount As Integer = 0

            S = 1 - Rt
            Do
                S0 = S
                S1 = S + 0.001
                fr = Me.SErrorFunc(S0, Rt)
                dfr = (fr - Me.SErrorFunc(S1, Rt)) / -0.001
                S += -fr / dfr
                If S < -(1 - Rt) Then S = -(1 - Rt) + 0.001
                If S > (1 - Rt) Then S = (1 - Rt) - 0.001
                icount += 1
            Loop Until Abs(fr) < itol Or icount > maxit_i

            If S <= -(1 - Rt) Then S = -(1 - Rt)
            If S >= (1 - Rt) Then S = (1 - Rt)

            For i = 0 To n
                pi(i) = fi(i) / (Rt + (1 - Rt + S) / (2 * Kb0 * Exp(ui1(i))) + (1 - Rt - S) / (2 * Kb0 * Exp(ui2(i))))
            Next

            Dim sumpi As Double = 0
            Dim sumeuipi1 As Double = 0
            Dim sumeuipi2 As Double = 0
            For i = 0 To n
                sumpi += pi(i)
                sumeuipi1 += pi(i) / Exp(ui1(i))
                sumeuipi2 += pi(i) / Exp(ui2(i))
            Next
            For i = 0 To n
                Vx1(i) = (pi(i) / Exp(ui1(i))) / sumeuipi1
                Vx2(i) = (pi(i) / Exp(ui2(i))) / sumeuipi2
                Vy(i) = pi(i) / sumpi
            Next

            If Rt <> 1 Then
                Kb = ((1 - Rt + S) * sumeuipi1 + (1 - Rt - S) * sumeuipi2) / (2 * (1 - Rt) * sumpi)
            Else
                Kb = 1.0#
            End If

            V = Rt * sumpi
            L1 = 0.5 * (S * V * (Kb / Kb0 - 1) + (1 + S) - V)
            L2 = 1 - L1 - V
            beta = L1 / (L1 + L2)

            Dim err1 As Double = Kb - 1

            CheckCalculatorStatus()

            Return err1

        End Function

        Private Function SErrorFunc(ByVal S0 As Double, ByVal Rt As Double)

            Dim errfunc As Double = 0
            For i = 0 To n
                errfunc += fi(i) * (1 / Exp(ui1(i)) - 1 / Exp(ui2(i))) / (Rt + (1 - Rt + S0) / (2 * Kb0 * Exp(ui1(i))) + (1 - Rt - S0) / (2 * Kb0 * Exp(ui2(i))))
            Next
            Return errfunc

        End Function

        Public Function Flash_PV_3P(ByVal Vz() As Double, ByVal Vest As Double, ByVal L1est As Double, ByVal L2est As Double, ByVal VyEST As Double(), ByVal Vx1EST As Double(), ByVal Vx2EST As Double(), ByVal P As Double, ByVal V As Double, ByVal Tref As Double, ByVal PP As PropertyPackage, Optional ByVal ReuseKI As Boolean = False, Optional ByVal PrevKi() As Double = Nothing) As Object

            etol = CDbl(PP.Parameters("PP_PTFELT"))
            maxit_e = CInt(PP.Parameters("PP_PTFMEI"))
            itol = CDbl(PP.Parameters("PP_PTFILT"))
            maxit_i = CInt(PP.Parameters("PP_PTFMII"))

            n = UBound(Vz)

            proppack = PP
            Vf = V
            L = 1 - V
            Lf = 1 - Vf
            Pf = P

            ReDim Vn(n), Vx1(n), Vx2(n), Vy(n), Vp(n), ui1(n), uic1(n), ui2(n), uic2(n), pi(n), Ki1(n), Ki2(n), fi(n)
            Dim Vt(n), VTc(n), Tmin, Tmax As Double

            Vn = PP.RET_VNAMES()
            VTc = PP.RET_VTC
            fi = Vz.Clone

            '--------------------------------------
            ' STEP 1 - Assume u, A, B, C, D, E, F 
            '--------------------------------------

            Tmin = 0
            Tmax = 0
            If Tref = 0 Then

                i = 0
                Tref = 0
                Do
                    Tref += 0.8 * Vz(i) * VTc(i)
                    Tmin += 0.1 * Vz(i) * VTc(i)
                    Tmax += 2.0 * Vz(i) * VTc(i)
                    i += 1
                Loop Until i = n + 1

            Else

                Tmin = Tref - 50
                Tmax = Tref + 50

            End If

            T = Tref
            T_ = T - 0.1
            T0 = T - 0.2

            '----------------------------------------
            ' STEP 1.1 - Estimate K, Vx, Vy, V and L 
            '----------------------------------------

            'Calculate Ki`s

            Ki1 = PP.DW_CalcKvalue(Vx1EST, VyEST, T, P, "LV")
            Ki2 = PP.DW_CalcKvalue(Vx2EST, VyEST, T, P, "LV")

            'Estimate V

            Kb_ = CalcKbj1(PP.DW_CalcKvalue(Vx1EST, VyEST, T_, P))
            Kb = CalcKbj1(PP.DW_CalcKvalue(Vx1EST, VyEST, T, P))
            Kb0 = Kb_

            B = Log(Kb_ / Kb) / (1 / T_ - 1 / T)
            A = Log(Kb) - B * (1 / T - 1 / T_)

            For i = 0 To n
                ui1(i) = Log(Ki1(i) / Kb)
                ui2(i) = Log(Ki2(i) / Kb)
            Next

            Dim fx(2 * n + 3), x(2 * n + 3), dfdx(2 * n + 3, 2 * n + 3), dx(2 * n + 3), xbr(2 * n + 3), fbr(2 * n + 3), fr As Double

            ecount = 0

            V = Vest
            L1 = L1est
            L2 = L2est
            L = L1 + L2
            beta = L1 / L2

            R = Kb * V / (Kb * V + Kb0 * L)
            Dim RLoop As Boolean = True

            If V = 0.0# Or V = 1.0# Then RLoop = False

            ecount = 0

            Do

                '--------------------------------------------------------------
                ' STEPS 2, 3, 4, 5, 6, 7 and 8 - Calculate R and Energy Balance
                '--------------------------------------------------------------

                R = Kb * V / (Kb * V + Kb0 * L)

                If RLoop Then

                    Dim dfr, R0, R1 As Double
                    Dim icount As Integer = 0

                    Do
                        R1 = R + 0.001
                        fr = Me.LiquidFractionBalance(R)
                        dfr = (fr - Me.LiquidFractionBalance(R1)) / -0.001
                        R0 = R
                        R += -fr / dfr
                        If R < 0 Then R = 0
                        If R > 1 Then R = 1
                        icount += 1
                    Loop Until Abs(fr) < itol Or icount > maxit_i Or Abs(R - R0) < 0.000001

                Else

                    Me.LiquidFractionBalance(R)

                End If

                'At this point, we have converged T for the simplified model. Proceed to step 9.

                '----------------------------------------------------------
                ' STEP 9 - Rigorous model Enthalpy and K-values calculation
                '----------------------------------------------------------

                Ki1 = PP.DW_CalcKvalue(Vx1, Vy, T, P)
                Ki2 = PP.DW_CalcKvalue(Vx2, Vy, T, P)

                For i = 0 To n
                    uic1(i) = Log(Ki1(i) / Kb)
                    uic2(i) = Log(Ki2(i) / Kb)
                Next

                Bc = Log(Kb_ / Kb) / (1 / T_ - 1 / T)
                Ac = Log(Kb) - Bc * (1 / T - 1 / T0)

                '-------------------------------------------
                ' STEP 10 - Update variables using Broyden
                '-------------------------------------------

                For i = 0 To n
                    fx(i) = (ui1(i) - uic1(i))
                    x(i) = ui1(i)
                Next

                For i = n + 1 To 2 * n + 1
                    fx(i) = (ui2(i - n - 1) - uic2(i - n - 1))
                    x(i) = ui2(i - n - 1)
                Next

                fx(2 * n + 2) = (A - Ac)
                fx(2 * n + 3) = (B - Bc)
                x(2 * n + 2) = A
                x(2 * n + 3) = B

                If ecount = 0 Then
                    For i = 0 To 2 * n + 1
                        For j = 0 To 2 * n + 1
                            If i = j Then dfdx(i, j) = 1 Else dfdx(i, j) = 0
                        Next
                    Next
                    broydn(2 * n + 3, x, fx, dx, xbr, fbr, dfdx, 0)
                Else
                    broydn(2 * n + 3, x, fx, dx, xbr, fbr, dfdx, 1)
                End If

                For i = 0 To n
                    ui1(i) = ui1(i) + dx(i)
                Next
                For i = n + 1 To 2 * n + 1
                    ui2(i - n - 1) = ui2(i - n - 1) + dx(i)
                Next
                A += dx(2 * n + 2)
                B += dx(2 * n + 3)

                ecount += 1

                If ecount > maxit_e Then
                    Throw New Exception(DWSIM.App.GetLocalString("PropPack_FlashMaxIt"))
                End If
                If Double.IsNaN(AbsSum(fx)) Then
                    Throw New Exception(DWSIM.App.GetLocalString("PropPack_FlashError"))
                End If

                Console.WriteLine("PV Flash 3P [IO]: Iteration #" & ecount & ", T = " & T & ", VF = " & V)

                CheckCalculatorStatus()

            Loop Until AbsSum(fx) < etol * (n + 2)

            Return New Object() {L1, V, Vx1, Vy, T, ecount, Ki1, L2, Vx2, Ki2}

        End Function

        Public Function Flash_TV_3P(ByVal Vz() As Double, ByVal Vest As Double, ByVal L1est As Double, ByVal L2est As Double, ByVal VyEST As Double(), ByVal Vx1EST As Double(), ByVal Vx2EST As Double(), ByVal T As Double, ByVal V As Double, ByVal Pref As Double, ByVal PP As PropertyPackage) As Object

            etol = CDbl(PP.Parameters("PP_PTFELT"))
            maxit_e = CInt(PP.Parameters("PP_PTFMEI"))

            n = UBound(Vz)

            proppack = PP
            Vf = V
            L = 1 - V
            Lf = 1 - Vf
            Tf = T

            ReDim Vn(n), Vx1(n), Vx2(n), Vy(n), Vp(n), ui1(n), uic1(n), ui2(n), uic2(n), pi(n), Ki1(n), Ki2(n), fi(n)

            Dim VTc = PP.RET_VTC()

            Vn = PP.RET_VNAMES()
            fi = Vz.Clone

            If Pref = 0 Then

                i = 0
                Do
                    If T / VTc(i) <= 0.9 Then
                        Vp(i) = PP.AUX_PVAPi(Vn(i), T)
                    End If
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
            P_ = Pref * 1.05
            P0 = Pref * 0.95

            '----------------------------------------
            ' STEP 1.1 - Estimate K, Vx, Vy, V and L 
            '----------------------------------------

            'Calculate Ki`s

            Ki1 = PP.DW_CalcKvalue(Vx1EST, VyEST, T, P, "LV")
            Ki2 = PP.DW_CalcKvalue(Vx2EST, VyEST, T, P, "LV")

            'Estimate V

            Kb_ = CalcKbj1(PP.DW_CalcKvalue(Vx1EST, VyEST, T_, P))
            Kb = CalcKbj1(PP.DW_CalcKvalue(Vx1EST, VyEST, T, P))
            Kb0 = Kb_

            B = Log(Kb_ / Kb) / (1 / T_ - 1 / T)
            A = Log(Kb) - B * (1 / T - 1 / T_)

            For i = 0 To n
                ui1(i) = Log(Ki1(i) / Kb)
                ui2(i) = Log(Ki2(i) / Kb)
            Next

            Dim fx(2 * n + 3), x(2 * n + 3), dfdx(2 * n + 3, 2 * n + 3), dx(2 * n + 3), xbr(2 * n + 3), fbr(2 * n + 3), fr As Double

            ecount = 0

            V = Vest
            L1 = L1est
            L2 = L2est
            L = L1 + L2
            beta = L1 / L2

            R = Kb * V / (Kb * V + Kb0 * L)
            Dim RLoop As Boolean = True

            If V = 0.0# Or V = 1.0# Then RLoop = False

            ecount = 0

            Do

                '--------------------------------------------------------------
                ' STEPS 2, 3, 4, 5, 6, 7 and 8 - Calculate R and Energy Balance
                '--------------------------------------------------------------

                R = Kb * V / (Kb * V + Kb0 * L)

                If RLoop Then

                    Dim dfr, R0, R1 As Double
                    Dim icount As Integer = 0

                    Do
                        R1 = R + 0.001
                        fr = Me.LiquidFractionBalanceP(R)
                        dfr = (fr - Me.LiquidFractionBalanceP(R1)) / -0.001
                        R0 = R
                        R += -fr / dfr
                        If R < 0 Then R = 0
                        If R > 1 Then R = 1
                        icount += 1
                    Loop Until Abs(fr) < itol Or icount > maxit_i

                Else

                    Me.LiquidFractionBalanceP(R)

                End If

                'At this point, we have converged T for the simplified model. Proceed to step 9.

                '----------------------------------------------------------
                ' STEP 9 - Rigorous model Enthalpy and K-values calculation
                '----------------------------------------------------------

                Ki1 = PP.DW_CalcKvalue(Vx1, Vy, T, P)
                Ki2 = PP.DW_CalcKvalue(Vx2, Vy, T, P)

                For i = 0 To n
                    uic1(i) = Log(Ki1(i) / Kb)
                    uic2(i) = Log(Ki2(i) / Kb)
                Next

                Bc = Log(Kb_ * P_ / (Kb0 * P0)) / Log(P_ / P0)
                Ac = Log(Kb * P) - Bc * Log(P / P0)

                '-------------------------------------------
                ' STEP 10 - Update variables using Broyden
                '-------------------------------------------

                For i = 0 To n
                    fx(i) = (ui1(i) - uic1(i))
                    x(i) = ui1(i)
                Next

                For i = n + 1 To 2 * n + 1
                    fx(i) = (ui2(i - n - 1) - uic2(i - n - 1))
                    x(i) = ui2(i - n - 1)
                Next

                fx(2 * n + 2) = (A - Ac)
                fx(2 * n + 3) = (B - Bc)
                x(2 * n + 2) = A
                x(2 * n + 3) = B

                If ecount = 0 Then
                    For i = 0 To 2 * n + 1
                        For j = 0 To 2 * n + 1
                            If i = j Then dfdx(i, j) = 1 Else dfdx(i, j) = 0
                        Next
                    Next
                    broydn(2 * n + 3, x, fx, dx, xbr, fbr, dfdx, 0)
                Else
                    broydn(2 * n + 3, x, fx, dx, xbr, fbr, dfdx, 1)
                End If

                For i = 0 To n
                    ui1(i) = ui1(i) + dx(i)
                Next
                For i = n + 1 To 2 * n + 1
                    ui2(i - n - 1) = ui2(i - n - 1) + dx(i)
                Next
                A += dx(2 * n + 2)
                B += dx(2 * n + 3)

                ecount += 1

                If ecount > maxit_e Then Throw New Exception(DWSIM.App.GetLocalString("PropPack_FlashMaxIt"))
                If Double.IsNaN(AbsSum(fx)) Then Throw New Exception(DWSIM.App.GetLocalString("PropPack_FlashError"))

                Console.WriteLine("TV Flash 3P [IO]: Iteration #" & ecount & ", P = " & P & ", VF = " & V)

                CheckCalculatorStatus()

            Loop Until AbsSum(fx) < etol * (n + 2)

            Return New Object() {L1, V, Vx1, Vy, P, ecount, Ki1, L2, Vx2, Ki2}

        End Function

        Public Function StabTest(ByVal T As Double, ByVal P As Double, ByVal Vz As Array, ByVal pp As PropertyPackage, Optional ByVal VzArray(,) As Double = Nothing, Optional ByVal searchseverity As Integer = 0)

            Dim i, j, c, n, o, l, nt, maxits As Integer
            n = UBound(Vz)
            nt = UBound(VzArray, 1)

            Dim Y, K As Double(,), tol As Double

            Select Case searchseverity
                Case 0
                    ReDim Y(nt, n)
                    tol = 0.01
                    maxits = 25
                Case 1
                    ReDim Y(nt + 1, n)
                    tol = 0.001
                    maxits = 100
                Case Else
                    ReDim Y(nt + 2, n)
                    tol = 0.0001
                    maxits = 200
            End Select

            For i = 0 To nt
                For j = 0 To n
                    Y(i, j) = VzArray(i, j)
                Next
            Next

            ReDim K(0, n)

            Dim m As Integer = UBound(Y, 1)

            Dim h(n), lnfi_z(n), Y_ant(m, n) As Double

            Dim gl, hl, sl, gv, hv, sv As Double

            hl = pp.DW_CalcEnthalpy(Vz, T, P, State.Liquid)
            sl = pp.DW_CalcEntropy(Vz, T, P, State.Liquid)
            gl = hl - T * sl

            hv = pp.DW_CalcEnthalpy(Vz, T, P, State.Vapor)
            sv = pp.DW_CalcEntropy(Vz, T, P, State.Vapor)
            gv = hv - T * sv

            If gl <= gv Then
                lnfi_z = pp.DW_CalcFugCoeff(Vz, T, P, State.Liquid)
            Else
                lnfi_z = pp.DW_CalcFugCoeff(Vz, T, P, State.Vapor)
            End If

            For i = 0 To n
                lnfi_z(i) = Log(lnfi_z(i))
            Next

            i = 0
            Do
                h(i) = Log(Vz(i)) + lnfi_z(i)
                i = i + 1
            Loop Until i = n + 1

            If Not VzArray Is Nothing Then
                If searchseverity = 1 Then
                    Dim sum0(n) As Double
                    i = 0
                    Do
                        sum0(i) = 0
                        j = 0
                        Do
                            sum0(i) += VzArray(j, i)
                            j = j + 1
                        Loop Until j = UBound(VzArray, 1) + 1
                        i = i + 1
                    Loop Until i = n + 1
                    i = 0
                    Do
                        Y(nt + 1, i) = sum0(i) / UBound(VzArray, 1)
                        i = i + 1
                    Loop Until i = n + 1
                End If
                If searchseverity = 2 Then
                    Dim sum0(n) As Double
                    i = 0
                    Do
                        sum0(i) = 0
                        j = 0
                        Do
                            sum0(i) += VzArray(j, i)
                            j = j + 1
                        Loop Until j = UBound(VzArray, 1) + 1
                        i = i + 1
                    Loop Until i = n + 1
                    i = 0
                    Do
                        Y(nt + 1, i) = sum0(i) / UBound(VzArray, 1)
                        Y(nt + 2, i) = Exp(h(i))
                        i = i + 1
                    Loop Until i = n + 1
                End If
            Else
                i = 0
                Do
                    Y(n + 1, i) = Exp(h(i))
                    i = i + 1
                Loop Until i = n + 1
            End If

            Dim lnfi(m, n), beta(m), r(m), r_ant(m) As Double
            Dim currcomp(n) As Double
            Dim dgdY(m, n), g_(m), tmpfug(n), dY(m, n), sum3 As Double
            Dim excidx As New ArrayList
            Dim finish As Boolean = True

            c = 0
            Do

                'start stability test for each one of the initial estimate vectors
                i = 0
                Do
                    If Not excidx.Contains(i) Then
                        j = 0
                        sum3 = 0
                        Do
                            If Y(i, j) > 0 Then sum3 += Y(i, j)
                            j = j + 1
                        Loop Until j = n + 1
                        j = 0
                        Do
                            If Y(i, j) > 0 Then currcomp(j) = Y(i, j) / sum3 Else currcomp(j) = 0
                            j = j + 1
                        Loop Until j = n + 1

                        hl = pp.DW_CalcEnthalpy(currcomp, T, P, State.Liquid)
                        sl = pp.DW_CalcEntropy(currcomp, T, P, State.Liquid)
                        gl = hl - T * sl

                        hv = pp.DW_CalcEnthalpy(currcomp, T, P, State.Vapor)
                        sv = pp.DW_CalcEntropy(currcomp, T, P, State.Vapor)
                        gv = hv - T * sv

                        If gl <= gv Then
                            tmpfug = pp.DW_CalcFugCoeff(currcomp, T, P, State.Liquid)
                        Else
                            tmpfug = pp.DW_CalcFugCoeff(currcomp, T, P, State.Vapor)
                        End If

                        j = 0
                        Do
                            lnfi(i, j) = Log(tmpfug(j))
                            j = j + 1
                        Loop Until j = n + 1
                        j = 0
                        Do
                            dgdY(i, j) = Log(Y(i, j)) + lnfi(i, j) - h(j)
                            j = j + 1
                        Loop Until j = n + 1
                        j = 0
                        beta(i) = 0
                        Do
                            beta(i) += (Y(i, j) - Vz(j)) * dgdY(i, j)
                            j = j + 1
                        Loop Until j = n + 1
                        g_(i) = 1
                        j = 0
                        Do
                            g_(i) += Y(i, j) * (Log(Y(i, j)) + lnfi(i, j) - h(j) - 1)
                            j = j + 1
                        Loop Until j = n + 1
                        If i > 0 Then r_ant(i) = r(i) Else r_ant(i) = 0
                        r(i) = 2 * g_(i) / beta(i)
                    End If

                    i = i + 1

                    CheckCalculatorStatus()

                Loop Until i = m + 1

                i = 0
                Do
                    If (Abs(g_(i)) < 0.0000000001 And r(i) > 0.9 And r(i) < 1.1) Then
                        If Not excidx.Contains(i) Then excidx.Add(i)
                        'ElseIf c > 4 And r(i) > r_ant(i) Then
                        '    If Not excidx.Contains(i) Then excidx.Add(i)
                    End If
                    i = i + 1
                Loop Until i = m + 1

                i = 0
                Do
                    If Not excidx.Contains(i) Then
                        j = 0
                        Do
                            Y_ant(i, j) = Y(i, j)
                            Y(i, j) = Exp(h(j) - lnfi(i, j))
                            dY(i, j) = Y(i, j) - Y_ant(i, j)
                            If Y(i, j) < 0 Then Y(i, j) = 0
                            j = j + 1
                        Loop Until j = n + 1
                    End If
                    i = i + 1
                Loop Until i = m + 1

                'check convergence

                finish = True
                i = 0
                Do
                    If Not excidx.Contains(i) Then
                        j = 0
                        Do
                            If Abs(dY(i, j)) > tol Then finish = False
                            j = j + 1
                        Loop Until j = n + 1
                    End If
                    i = i + 1
                Loop Until i = m + 1

                c = c + 1

                If c > maxits Then Throw New Exception("Stability Test: Maximum Iterations Reached.")

            Loop Until finish = True

            ' search for trivial solutions

            Dim sum As Double
            i = 0
            Do
                If Not excidx.Contains(i) Then
                    j = 0
                    sum = 0
                    Do
                        sum += Abs(Y(i, j) - Vz(j))
                        j = j + 1
                    Loop Until j = n + 1
                    If sum < 0.001 Then
                        If Not excidx.Contains(i) Then excidx.Add(i)
                    End If
                End If
                i = i + 1
            Loop Until i = m + 1

            ' search for trivial solutions

            Dim sum5 As Double
            i = 0
            Do
                If Not excidx.Contains(i) Then
                    j = 0
                    sum5 = 0
                    Do
                        sum5 += Y(i, j)
                        j = j + 1
                    Loop Until j = n + 1
                    If sum5 < 1 Then
                        'phase is stable
                        If Not excidx.Contains(i) Then excidx.Add(i)
                    End If
                End If
                i = i + 1
            Loop Until i = m + 1

            ' join similar solutions

            Dim similar As Boolean

            i = 0
            Do
                If Not excidx.Contains(i) Then
                    o = 0
                    Do
                        If Not excidx.Contains(o) And i <> o Then
                            similar = True
                            j = 0
                            Do
                                If Abs(Y(i, j) - Y(o, j)) > 0.00001 Then
                                    similar = False
                                End If
                                j = j + 1
                            Loop Until j = n + 1
                            If similar Then
                                excidx.Add(o)
                                Exit Do
                            End If
                        End If
                        o = o + 1
                    Loop Until o = m + 1
                End If
                i = i + 1
            Loop Until i = m + 1

            l = excidx.Count
            Dim sum2 As Double
            Dim isStable As Boolean

            If m + 1 - l > 0 Then

                'the phase is unstable

                isStable = False

                'normalize initial estimates

                Dim inest(m - l, n) As Double
                i = 0
                l = 0
                Do
                    If Not excidx.Contains(i) Then
                        j = 0
                        sum2 = 0
                        Do
                            sum2 += Y(i, j)
                            j = j + 1
                        Loop Until j = n + 1
                        j = 0
                        Do
                            inest(l, j) = Y(i, j) / sum2
                            j = j + 1
                        Loop Until j = n + 1
                        l = l + 1
                    End If
                    i = i + 1
                Loop Until i = m + 1
                Return New Object() {isStable, inest}
            Else

                'the phase is stable

                isStable = True
                Return New Object() {isStable, Nothing}
            End If

        End Function

        Private Sub broydn(ByVal N As Object, ByVal X As Object, ByVal F As Object, ByVal P As Object, ByVal XB As Object, ByVal FB As Object, ByVal H As Object, ByVal IFLAG As Integer)
            '
            '**********************************************************************
            '
            '       N = NUMBER OF EQUATIONS
            '       X(N) = CURRENT VALUE OF X, INITAL GUESS X0 ON FIRST CALL
            '              THE VALUE OF X IS NOT UPDATED AND MUST BE UPDATED IN
            '              CALLING PROGRAM
            '       F(N) = VALUE OF F(X) MUST BE PROVIDED ON ALL CALLS
            '       P(N) = STEP PREDICTED BY BROYDN (USED TO UPDATE X)
            '              THE NEW VALUE OF X IS X+P
            '       XB(N) = RETENTION FOR X VECTOR
            '       FB(N) = RETENTION FOR F VECTOR
            '       H(N,N) = BROYDEN H MATRIX IT MUST BE INITIALIZED TO A CLOSE
            '                J(X0)**-1 OR IDENTITY MATRIX
            '       IFLAG = CALCULATION CONTROL FLAG
            '               0 INITIAL CALL, NO H UPDATE
            '               1 UPDATE CALL, NO H DAMPING
            '
            Dim I As Short
            Dim J As Short
            Dim PTP As Double
            Dim PTH As Double
            Dim THETA As Double
            Dim PTHY As Double
            Dim PTHF As Double
            Dim HY As Double
            Dim DENOM As Double
            '
            '      INITIAL CALL
            '
            If (IFLAG <> 0) Then
                PTP = 0.0#
                '
                For I = 0 To N 'do 30 I=1,N
                    P(I) = X(I) - XB(I)
                    PTP = PTP + P(I) * P(I)
                    HY = 0.0#
                    For J = 0 To N '  DO 20 J=1,N
                        HY = HY + H(I, J) * (F(J) - FB(J))
20:                 Next J
                    XB(I) = HY - P(I)
30:             Next I
                PTHY = 0.0#
                PTHF = 0.0#
                '
                For I = 0 To N ' DO 40 I=1,N
                    PTH = 0.0#
                    For J = 0 To N '  DO 35 J=1,N
                        PTH = PTH + P(J) * H(J, I)
35:                 Next J
                    PTHY = PTHY + PTH * (F(I) - FB(I))
                    PTHF = PTHF + PTH * F(I)
                    FB(I) = PTH
40:             Next I
                THETA = 1.0#
                '
                DENOM = (1.0# - THETA) * PTP + THETA * PTHY
                '
                For I = 0 To N ' DO 50 I=1,N
                    For J = 0 To N ' DO 50 J=1,N
                        H(I, J) = H(I, J) - THETA * XB(I) * FB(J) / DENOM
                    Next J
50:             Next I
                '
            End If
            For I = 0 To N '  DO 70 I=1,N
                XB(I) = X(I)
                FB(I) = F(I)
                P(I) = 0.0#
                '
                For J = 0 To N '  DO 70 J=1,N
                    P(I) = P(I) - H(I, J) * F(J)
                Next J
70:         Next I
            ''
        End Sub

        Private Function LiquidFractionBalance(ByVal R As Double) As Double

            Dim fr, dfr, S0, S1 As Double
            Dim icount As Integer = 0

            S = 1 - R
            Do
                S0 = S
                S1 = S + 0.001
                fr = Me.SErrorFunc(S0, R)
                dfr = (fr - Me.SErrorFunc(S1, R)) / -0.001
                S += -fr / dfr
                icount += 1
            Loop Until Abs(fr) < itol Or icount > maxit_i

            If S <= -(1 - R) Then S = -(1 - R)
            If S >= (1 - R) Then S = (1 - R)

            For i = 0 To n
                pi(i) = fi(i) / (R + (1 - R + S) / (2 * Kb0 * Exp(ui1(i))) + (1 - R - S) / (2 * Kb0 * Exp(ui2(i))))
            Next

            Dim sumpi As Double = 0
            Dim sumeuipi1 As Double = 0
            Dim sumeuipi2 As Double = 0
            For i = 0 To n
                sumpi += pi(i)
                sumeuipi1 += pi(i) / Exp(ui1(i))
                sumeuipi2 += pi(i) / Exp(ui2(i))
            Next
            For i = 0 To n
                Vx1(i) = (pi(i) / Exp(ui1(i))) / sumeuipi1
                Vx2(i) = (pi(i) / Exp(ui2(i))) / sumeuipi2
                Vy(i) = pi(i) / sumpi
            Next

            If R <> 1 Then
                Kb = ((1 - R + S) * sumeuipi1 + (1 - R - S) * sumeuipi2) / (2 * (1 - R) * sumpi)
            Else
                Kb = 1.0#
            End If

            V = R * sumpi
            L1 = 0.5 * (S * V * (Kb / Kb0 - 1) + (1 + S) - V)
            L2 = 1 - L1 - V
            beta = L1 / (L1 + L2)

            T = 1 / T_ + (Log(Kb) - A) / B
            T = 1 / T

            Dim eberror As Double = (L1 + L2) - Lf

            CheckCalculatorStatus()

            Return eberror

        End Function

        Private Function LiquidFractionBalanceP(ByVal R As Double) As Double

            Dim fr, dfr, S0, S1 As Double
            Dim icount As Integer = 0

            S = ((-(1 - R)) + (1 - R)) / 2
            Do
                S0 = S
                S1 = S + 0.001
                fr = Me.SErrorFunc(S0, R)
                dfr = (fr - Me.SErrorFunc(S1, R)) / -0.001
                S += -fr / dfr
                icount += 1
            Loop Until Abs(fr) < itol Or icount > maxit_i

            For i = 0 To n
                pi(i) = fi(i) / (R + (1 - R + S) / (2 * Kb0 * Exp(ui1(i))) + (1 - R - S) / (2 * Kb0 * Exp(ui2(i))))
            Next

            Dim sumpi As Double = 0
            Dim sumeuipi1 As Double = 0
            Dim sumeuipi2 As Double = 0
            For i = 0 To n
                sumpi += pi(i)
                sumeuipi1 += pi(i) / Exp(ui1(i))
                sumeuipi2 += pi(i) / Exp(ui2(i))
            Next
            For i = 0 To n
                Vx1(i) = (pi(i) / Exp(ui1(i))) / sumeuipi1
                Vx2(i) = (pi(i) / Exp(ui2(i))) / sumeuipi2
                Vy(i) = pi(i) / sumpi
            Next

            If R <> 1 Then
                Kb = ((1 - R + S) * sumeuipi1 + (1 - R - S) * sumeuipi2) / (2 * (1 - R) * sumpi)
            Else
                Kb = 1.0#
            End If

            V = R * sumpi
            L1 = 0.5 * (S * V * (Kb / Kb0 - 1) + (1 + S) - V)
            L2 = 1 - L1 - V
            beta = L1 / (L1 + L2)

            P = Exp((A - Log(Kb) - B * Log(P0)) / (1 - B))

            Dim eberror As Double = (L1 + L2) - Lf

            CheckCalculatorStatus()

            Return eberror

        End Function

        Private Function MinimizeError(ByVal alpha As Double) As Double

            Dim n As Integer = UBound(tmpdx)
            Dim i As Integer
            Dim errors(n) As Double

            For i = 0 To n
                errors(i) = (refx(i) - (currx(i) + alpha * tmpdx(i))) ^ 2
            Next

            Return Common.Sum(errors)

        End Function

        Public Sub New()



        End Sub

    End Class

End Namespace
