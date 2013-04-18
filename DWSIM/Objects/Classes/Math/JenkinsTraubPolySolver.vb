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

Imports System.Numerics

Namespace DWSIM.MathEx.RootFinders

    Public Class JenkinsTraubPolySolver

        'Global variables that assist the computation, taken from the Visual Studio C++ compiler class float
        Dim DBL_EPSILON As Double = 0.00000000000000022204460492503131 ' smallest such that 1.0+DBL_EPSILON != 1.0 
        Dim DBL_MAX As Double = 1.7976931348623157E+308 ' max value 
        Dim DBL_MIN As Double = 2.2250738585072014E-308 ' min positive value 

        'If needed, set the maximum allowed degree for the polynomial here:
        Dim Max_Degree_Polynomial As Integer = 100

        'It is done to allocate memory for the computation arrays, so be careful to not set i too high, though in practice it should not be a problem as it is now.

        ''' <summary>
        ''' The Jenkins–Traub algorithm for polynomial zeros translated into pure VB.NET. It is a translation of the C++ algorithm, which in turn is a translation of the FORTRAN code by Jenkins. See Wikipedia for referances: http://en.wikipedia.org/wiki/Jenkins%E2%80%93Traub_algorithm 
        ''' </summary>
        ''' <param name="Input">The coefficients for the polynomial starting with the highest degree and ends on the constant, missing degree must be implemented as a 0.</param>
        ''' <returns>All the real and complex roots that are found is returned in a list of complex numbers.</returns>
        ''' <remarks>The maximum alloed degree polynomial for this implementation is set to 100. It can only take real coefficients.</remarks>
        Public Function FindRoots(ByVal ParamArray Input As Double()) As List(Of Complex)

            Dim result As New List(Of Complex)

            Dim i, j, jj, l, N, NM1, NN, NZ, zerok As Integer

            'Helper variable that indicates the maximum length of the polynomial array
            Dim Max_Degree_Helper As Integer = Max_Degree_Polynomial + 1

            'Actual degree calculated from the imtems in the Input ParamArray
            Dim Degree As Integer = Input.Length - 1

            Dim op(Max_Degree_Helper), K(Max_Degree_Helper), p(Max_Degree_Helper), pt(Max_Degree_Helper), qp(Max_Degree_Helper), temp(Max_Degree_Helper) As Double
            Dim zeror(Max_Degree_Polynomial), zeroi(Max_Degree_Polynomial) As Double
            Dim bnd, df, dx, ff, moduli_max, moduli_min, x, xm, aa, bb, cc, lzi, lzr, sr, szi, szr, t, u, xx, xxx, yy As Double

            ' These are used to scale the polynomial for more accurecy
            Dim factor, sc As Double

            Dim RADFAC As Double = 3.1415926535897931 / 180 ' Degrees-to-radians conversion factor = pi/180
            Dim lb2 As Double = Math.Log(2.0) ' Dummy variable to avoid re-calculating this value in loop below
            Dim lo As Double = DBL_MIN / DBL_EPSILON  'Double.MinValue / Double.Epsilon
            Dim cosr As Double = Math.Cos(94.0 * RADFAC) ' = -0.069756474
            Dim sinr As Double = Math.Sin(94.0 * RADFAC) ' = 0.99756405

            'Are the polynomial larger that the maximum allowed?
            If Degree > Max_Degree_Polynomial Then
                Throw New Exception("The entered Degree is greater than MAXDEGREE. Exiting root finding algorithm. No further action taken.")
            End If

            'Check if the leading coefficient is zero
            If Input(0) <> 0 Then

                For i = 0 To Input.Length - 1
                    op(i) = Input(i)
                Next

                N = Degree
                xx = Math.Sqrt(0.5) '= 0.70710678
                yy = -xx

                ' Remove zeros at the origin, if any
                j = 0
                While (op(N) = 0)
                    zeror(j) = 0
                    zeroi(j) = 0.0
                    N -= 1
                    j += 1
                End While

                NN = N + 1

                For i = 0 To NN - 1
                    p(i) = op(i)
                Next

                While N >= 1
                    'Start the algorithm for one zero
                    If N <= 2 Then
                        If N < 2 Then
                            '1st degree polynomial
                            zeror((Degree) - 1) = -(p(1) / p(0))
                            zeroi((Degree) - 1) = 0.0
                        Else
                            '2nd degree polynomial
                            Quad_ak1(p(0), p(1), p(2), zeror(((Degree) - 2)), zeroi(((Degree) - 2)), zeror(((Degree) - 1)), zeroi((Degree) - 1))
                        End If
                        'Solutions have been calculated, so exit the loop
                        Exit While
                    End If

                    moduli_max = 0.0
                    moduli_min = DBL_MAX

                    For i = 0 To NN - 1
                        x = Math.Abs(p(i))
                        If (x > moduli_max) Then moduli_max = x
                        If ((x <> 0) And (x < moduli_min)) Then moduli_min = x
                    Next

                    ' Scale if there are large or very small coefficients
                    ' Computes a scale factor to multiply the coefficients of the polynomial. The scaling
                    ' is done to avoid overflow and to avoid undetected underflow interfering with the
                    ' convergence criterion.
                    ' The factor is a power of the base.

                    '  Scaling the polynomial
                    sc = lo / moduli_min

                    If (((sc <= 1.0) And (moduli_max >= 10)) Or ((sc > 1.0) And (DBL_MAX / sc >= moduli_max))) Then
                        If sc = 0 Then
                            sc = DBL_MIN
                        End If

                        l = CInt(Math.Log(sc) / lb2 + 0.5)
                        factor = Math.Pow(2.0, l)
                        If (factor <> 1.0) Then
                            For i = 0 To NN
                                p(i) *= factor
                            Next
                        End If
                    End If

                    'Compute lower bound on moduli of zeros
                    For i = 0 To NN - 1
                        pt(i) = Math.Abs(p(i))
                    Next
                    pt(N) = -(pt(N))

                    NM1 = N - 1

                    ' Compute upper estimate of bound
                    x = Math.Exp((Math.Log(-pt(N)) - Math.Log(pt(0))) / CDbl(N))

                    If (pt(NM1) <> 0) Then
                        ' If Newton step at the origin is better, use it
                        xm = -pt(N) / pt(NM1)
                        If xm < x Then
                            x = xm
                        End If
                    End If

                    ' Chop the interval (0, x) until ff <= 0
                    xm = x

                    Do
                        x = xm
                        xm = 0.1 * x
                        ff = pt(0)
                        For i = 1 To NN - 1
                            ff = ff * xm + pt(i)
                        Next
                    Loop While (ff > 0)

                    dx = x

                    Do
                        df = pt(0)
                        ff = pt(0)
                        For i = 1 To N - 1
                            ff = x * ff + pt(i)
                            df = x * df + ff
                        Next
                        ff = x * ff + pt(N)
                        dx = ff / df
                        x -= dx
                    Loop While (Math.Abs(dx / x) > 0.005)

                    bnd = x

                    ' Compute the derivative as the initial K polynomial and do 5 steps with no shift
                    For i = 1 To N - 1
                        K(i) = CDbl(N - i) * p(i) / (CDbl(N))
                    Next
                    K(0) = p(0)

                    aa = p(N)
                    bb = p(NM1)
                    If (K(NM1) = 0) Then
                        zerok = 1
                    Else
                        zerok = 0
                    End If

                    For jj = 0 To 4
                        cc = K(NM1)
                        If (zerok) Then
                            ' Use unscaled form of recurrence
                            For i = 0 To NM1 - 1
                                j = NM1 - i
                                K(j) = K(j - 1)
                            Next
                            K(0) = 0
                            If (K(NM1) = 0) Then
                                zerok = 1
                            Else
                                zerok = 0
                            End If
                        Else
                            ' Used scaled form of recurrence if value of K at 0 is nonzero
                            t = -aa / cc
                            For i = 0 To NM1 - 1
                                j = NM1 - i
                                K(j) = t * K(j - 1) + p(j)
                            Next
                            K(0) = p(0)
                            If (Math.Abs(K(NM1)) <= Math.Abs(bb) * DBL_EPSILON * 10.0) Then
                                zerok = 1
                            Else
                                zerok = 0
                            End If
                        End If
                    Next

                    ' Save K for restarts with new shifts
                    For i = 0 To N - 1
                        temp(i) = K(i)
                    Next

                    For jj = 1 To 20

                        ' Quadratic corresponds to a double shift to a non-real point and its
                        ' complex conjugate. The point has modulus BND and amplitude rotated
                        ' by 94 degrees from the previous shift.

                        xxx = -(sinr * yy) + cosr * xx
                        yy = sinr * xx + cosr * yy
                        xx = xxx
                        sr = bnd * xx
                        u = -(2.0 * sr)

                        ' Second stage calculation, fixed quadratic
                        Fxshfr_ak1(20 * jj, NZ, sr, bnd, K, N, p, NN, qp, u, lzi, lzr, szi, szr)

                        If (NZ <> 0) Then

                            ' The second stage jumps directly to one of the third stage iterations and
                            ' returns here if successful. Deflate the polynomial, store the zero or
                            ' zeros, and return to the main algorithm.

                            j = (Degree) - N
                            zeror(j) = szr
                            zeroi(j) = szi
                            NN = NN - NZ
                            N = NN - 1
                            For i = 0 To NN - 1
                                p(i) = qp(i)
                            Next
                            If (NZ <> 1) Then
                                zeror(j + 1) = lzr
                                zeroi(j + 1) = lzi
                            End If

                            'Found roots start all calulations again, with a lower order polynomial
                            Exit For
                        Else
                            ' If the iteration is unsuccessful, another quadratic is chosen after restoring K
                            For i = 0 To N - 1
                                K(i) = temp(i)
                            Next
                        End If
                        If (jj >= 20) Then
                            Throw New Exception("Failure. No convergence after 20 shifts. Program terminated.")
                        End If
                    Next
                End While

            Else
                Throw New Exception("The leading coefficient is zero. No further action taken. Program terminated.")
            End If

            For i = 0 To Degree - 1
                result.Add(New Complex(zeror(Degree - 1 - i), zeroi(Degree - 1 - i)))
            Next

            Return result
        End Function

        Private Sub Fxshfr_ak1(ByVal L2 As Integer, ByRef NZ As Integer, ByVal sr As Double, ByVal v As Double, ByVal K() As Double, ByVal N As Integer, ByVal p() As Double, ByVal NN As Integer, ByVal qp() As Double, ByVal u As Double, ByRef lzi As Double, ByRef lzr As Double, ByRef szi As Double, ByRef szr As Double)

            ' Computes up to L2 fixed shift K-polynomials, testing for convergence in the linear or
            ' quadratic case. Initiates one of the variable shift iterations and returns with the
            ' number of zeros found.

            ' L2 limit of fixed shift steps
            ' NZ number of zeros found

            Dim fflag, i, iFlag, j, spass, stry, tFlag, vpass, vtry As Integer
            iFlag = 1
            Dim a, a1, a3, a7, b, betas, betav, c, d, e, f, g, h, oss, ots, otv, ovv, s, ss, ts, tss, tv, tvv, ui, vi, vv As Double
            Dim qk(100 + 1), svk(100 + 1) As Double

            NZ = 0
            betav = 0.25
            betas = 0.25
            oss = sr
            ovv = v

            ' Evaluate polynomial by synthetic division
            QuadSD_ak1(NN, u, v, p, qp, a, b)

            tFlag = calcSC_ak1(N, a, b, a1, a3, a7, c, d, e, f, g, h, K, u, v, qk)

            For j = 0 To L2 - 1

                fflag = 1
                ' Calculate next K polynomial and estimate v
                nextK_ak1(N, tFlag, a, b, a1, a3, a7, K, qk, qp)
                tFlag = calcSC_ak1(N, a, b, a1, a3, a7, c, d, e, f, g, h, K, u, v, qk)
                newest_ak1(tFlag, ui, vi, a, a1, a3, a7, b, c, d, f, g, h, u, v, K, N, p)

                vv = vi

                ' Estimate s
                If K(N - 1) <> 0 Then
                    ss = -(p(N) / K(N - 1))
                Else
                    ss = 0
                End If

                ts = 1
                tv = 1.0

                If ((j <> 0) And (tFlag <> 3)) Then

                    ' Compute relative measures of convergence of s and v sequences
                    If vv <> 0 Then
                        tv = Math.Abs((vv - ovv) / vv)
                    End If

                    If ss <> 0 Then
                        ts = Math.Abs((ss - oss) / ss)
                    End If


                    ' If decreasing, multiply the two most recent convergence measures

                    If tv < otv Then
                        tvv = tv * otv
                    Else
                        tvv = 1
                    End If


                    If ts < ots Then
                        tss = ts * ots
                    Else
                        tss = 1
                    End If

                    ' Compare with convergence criteria

                    If tvv < betav Then
                        vpass = 1
                    Else
                        vpass = 0
                    End If

                    If tss < betas Then
                        spass = 1
                    Else
                        spass = 0
                    End If


                    If ((spass) Or (vpass)) Then

                        ' At least one sequence has passed the convergence test.
                        ' Store variables before iterating

                        For i = 0 To N - 1
                            svk(i) = K(i)
                        Next

                        s = ss

                        ' Choose iteration according to the fastest converging sequence
                        stry = 0
                        vtry = 0

                        Do

                            If ((fflag And ((fflag = 0) = 0)) And ((spass) And (Not vpass Or (tss < tvv)))) Then
                                ' Do nothing. Provides a quick "short circuit".
                            Else
                                QuadIT_ak1(N, NZ, ui, vi, szr, szi, lzr, lzi, qp, NN, a, b, p, qk, a1, a3, a7, c, d, e, f, g, h, K)

                                If ((NZ) > 0) Then Exit Sub

                                ' Quadratic iteration has failed. Flag that it has been tried and decrease the
                                ' convergence criterion

                                iFlag = 1
                                vtry = 1
                                betav *= 0.25

                                ' Try linear iteration if it has not been tried and the s sequence is converging
                                If (stry Or (Not spass)) Then
                                    iFlag = 0
                                Else
                                    For i = 0 To N - 1
                                        K(i) = svk(i)
                                    Next
                                End If

                            End If

                            If (iFlag <> 0) Then
                                RealIT_ak1(iFlag, NZ, s, N, p, NN, qp, szr, szi, K, qk)

                                If ((NZ) > 0) Then Exit Sub

                                ' Linear iteration has failed. Flag that it has been tried and decrease the
                                ' convergence criterion
                                stry = 1
                                betas *= 0.25

                                If (iFlag <> 0) Then

                                    ' If linear iteration signals an almost double real zero, attempt quadratic iteration
                                    ui = -(s + s)
                                    vi = s * s
                                End If
                            End If

                            ' Restore variables
                            For i = 0 To N - 1
                                K(i) = svk(i)
                            Next


                            ' Try quadratic iteration if it has not been tried and the v sequence is converging
                            If (Not vpass Or vtry) Then
                                ' Break out of infinite for loop
                                Exit Do
                            End If


                        Loop While True

                        ' Re-compute qp and scalar values to continue the second stage
                        QuadSD_ak1(NN, u, v, p, qp, a, b)
                        tFlag = calcSC_ak1(N, a, b, a1, a3, a7, c, d, e, f, g, h, K, u, v, qk)

                    End If
                End If

                ovv = vv
                oss = ss
                otv = tv
                ots = ts
            Next

        End Sub

        Private Sub QuadSD_ak1(ByVal NN As Integer, ByVal u As Double, ByVal v As Double, ByVal p() As Double, ByVal q() As Double, ByRef a As Double, ByRef b As Double)

            ' Divides p by the quadratic 1, u, v placing the quotient in q and the remainder in a, b

            Dim i As Integer

            b = p(0)
            q(0) = p(0)

            a = -((b) * u) + p(1)
            q(1) = -((b) * u) + p(1)

            For i = 2 To NN - 1
                q(i) = -((a) * u + (b) * v) + p(i)
                b = (a)
                a = q(i)
            Next

        End Sub

        Private Function calcSC_ak1(ByVal N As Integer, ByVal a As Double, ByVal b As Double, ByRef a1 As Double, ByRef a3 As Double, ByRef a7 As Double, ByRef c As Double, ByRef d As Double, ByRef e As Double, ByRef f As Double, ByRef g As Double, ByRef h As Double, ByVal K() As Double, ByVal u As Double, ByVal v As Double, ByVal qk() As Double) As Integer

            ' This routine calculates scalar quantities used to compute the next K polynomial and
            ' new estimates of the quadratic coefficients.

            ' calcSC - integer variable set here indicating how the calculations are normalized
            ' to avoid overflow.

            Dim dumFlag As Integer = 3 ' TYPE = 3 indicates the quadratic is almost a factor of K

            ' Synthetic division of K by the quadratic 1, u, v
            QuadSD_ak1(N, u, v, K, qk, c, d)

            If (Math.Abs((c)) <= (100.0 * DBL_EPSILON * Math.Abs(K(N - 1)))) Then
                If (Math.Abs((d)) <= (100.0 * DBL_EPSILON * Math.Abs(K(N - 2)))) Then
                    Return dumFlag
                End If
            End If

            h = v * b
            If (Math.Abs((d)) >= Math.Abs((c))) Then
                dumFlag = 2 ' TYPE = 2 indicates that all formulas are divided by d
                e = a / (d)
                f = (c) / (d)
                g = u * b
                a3 = (e) * ((g) + a) + (h) * (b / (d))
                a1 = -a + (f) * b
                a7 = (h) + ((f) + u) * a
            Else
                dumFlag = 1 ' TYPE = 1 indicates that all formulas are divided by c
                e = a / (c)
                f = (d) / (c)
                g = (e) * u
                a3 = (e) * a + ((g) + (h) / (c)) * b
                a1 = -(a * ((d) / (c))) + b
                a7 = (g) * (d) + (h) * (f) + a
            End If

            Return dumFlag
        End Function

        Private Sub nextK_ak1(ByVal N As Integer, ByVal tFlag As Integer, ByVal a As Double, ByVal b As Double, ByVal a1 As Double, ByRef a3 As Double, ByRef a7 As Double, ByVal K() As Double, ByVal qk() As Double, ByVal qp() As Double)

            ' Computes the next K polynomials using the scalars computed in calcSC_ak1

            Dim i As Integer
            Dim temp As Double

            If (tFlag = 3) Then ' Use unscaled form of the recurrence
                K(1) = 0
                K(0) = 0.0

                For i = 2 To N - 1
                    K(i) = qk(i - 2)
                Next

                Exit Sub
            End If

            If tFlag = 1 Then
                temp = b
            Else
                temp = a
            End If


            If (Math.Abs(a1) > (10.0 * DBL_EPSILON * Math.Abs(temp))) Then
                ' Use scaled form of the recurrence

                a7 = a7 / a1
                a3 = a3 / a1
                K(0) = qp(0)
                K(1) = -((a7) * qp(0)) + qp(1)

                For i = 2 To N - 1
                    K(i) = -((a7) * qp(i - 1)) + (a3) * qk(i - 2) + qp(i)
                Next
            Else
                ' If a1 is nearly zero, then use a special form of the recurrence

                K(0) = 0.0
                K(1) = -(a7) * qp(0)

                For i = 2 To N - 1
                    K(i) = -((a7) * qp(i - 1)) + (a3) * qk(i - 2)
                Next
            End If
        End Sub

        Private Sub newest_ak1(ByVal tFlag As Integer, ByRef uu As Double, ByRef vv As Double, ByVal a As Double, ByVal a1 As Double, ByVal a3 As Double, ByVal a7 As Double, ByVal b As Double, ByVal c As Double, ByVal d As Double, ByVal f As Double, ByVal g As Double, ByVal h As Double, ByVal u As Double, ByVal v As Double, ByVal K() As Double, ByVal N As Integer, ByVal p() As Double)
            ' Compute new estimates of the quadratic coefficients using the scalars computed in calcSC_ak1

            Dim a4, a5, b1, b2, c1, c2, c3, c4, temp As Double

            vv = 0 'The quadratic is zeroed
            uu = 0.0 'The quadratic is zeroed

            If (tFlag <> 3) Then

                If (tFlag <> 2) Then
                    a4 = a + u * b + h * f
                    a5 = c + (u + v * f) * d

                Else
                    a4 = (a + g) * f + h
                    a5 = (f + u) * c + v * d
                End If

                ' Evaluate new quadratic coefficients
                b1 = -K(N - 1) / p(N)
                b2 = -(K(N - 2) + b1 * p(N - 1)) / p(N)
                c1 = v * b2 * a1
                c2 = b1 * a7
                c3 = b1 * b1 * a3
                c4 = -(c2 + c3) + c1
                temp = -c4 + a5 + b1 * a4
                If (temp <> 0.0) Then
                    uu = -((u * (c3 + c2) + v * (b1 * a1 + b2 * a7)) / temp) + u
                    vv = v * (1.0 + c4 / temp)
                End If

            End If
        End Sub

        Private Sub QuadIT_ak1(ByVal N As Integer, ByRef NZ As Integer, ByVal uu As Double, ByVal vv As Double, ByRef szr As Double, ByRef szi As Double, ByRef lzr As Double, ByRef lzi As Double, ByVal qp() As Double, ByVal NN As Integer, ByRef a As Double, ByRef b As Double, ByVal p() As Double, ByVal qk() As Double, ByRef a1 As Double, ByRef a3 As Double, ByRef a7 As Double, ByRef c As Double, ByRef d As Double, ByRef e As Double, ByRef f As Double, ByRef g As Double, ByRef h As Double, ByVal K() As Double)

            ' Variable-shift K-polynomial iteration for a quadratic factor converges only if the
            ' zeros are equimodular or nearly so.

            Dim i, j, tFlag, triedFlag As Integer
            j = 0
            triedFlag = 0

            Dim ee, mp, omp, relstp, t, u, ui, v, vi, zm As Double

            NZ = 0  'Number of zeros found
            u = uu 'uu and vv are coefficients of the starting quadratic
            v = vv

            Do
                Quad_ak1(1.0, u, v, szr, szi, lzr, lzi)

                ' Return if roots of the quadratic are real and not close to multiple or nearly
                ' equal and of opposite sign.
                If (Math.Abs(Math.Abs(szr) - Math.Abs(lzr)) > 0.01 * Math.Abs(lzr)) Then
                    Exit Do
                End If

                ' Evaluate polynomial by quadratic synthetic division
                QuadSD_ak1(NN, u, v, p, qp, a, b)

                mp = Math.Abs(-((szr) * (b)) + (a)) + Math.Abs((szi) * (b))

                ' Compute a rigorous bound on the rounding error in evaluating p
                zm = Math.Sqrt(Math.Abs(v))
                ee = 2.0 * Math.Abs(qp(0))
                t = -((szr) * (b))

                For i = 1 To N - 1
                    ee = ee * zm + Math.Abs(qp(i))
                Next

                ee = ee * zm + Math.Abs((a) + t)
                ee = (9.0 * ee + 2.0 * Math.Abs(t) - 7.0 * (Math.Abs((a) + t) + zm * Math.Abs((b)))) * DBL_EPSILON

                ' Iteration has converged sufficiently if the polynomial value is less than 20 times this bound
                If (mp <= 20.0 * ee) Then
                    NZ = 2
                    Exit Do
                End If

                j += 1

                ' Stop iteration after 20 steps
                If (j > 20) Then Exit Do

                If (j >= 2) Then
                    If ((relstp <= 0.01) And (mp >= omp) And (Not triedFlag)) Then
                        ' A cluster appears to be stalling the convergence. Five fixed shift
                        ' steps are taken with a u, v close to the cluster.
                        If relstp < DBL_EPSILON Then
                            relstp = Math.Sqrt(DBL_EPSILON)
                        Else
                            relstp = Math.Sqrt(relstp)
                        End If

                        u -= u * relstp
                        v += v * relstp

                        QuadSD_ak1(NN, u, v, p, qp, a, b)

                        For i = 0 To 4
                            tFlag = calcSC_ak1(N, a, b, a1, a3, a7, c, d, e, f, g, h, K, u, v, qk)
                            nextK_ak1(N, tFlag, a, b, a1, a3, a7, K, qk, qp)
                        Next

                        triedFlag = 1
                        j = 0

                    End If

                End If

                omp = mp

                ' Calculate next K polynomial and new u and v
                tFlag = calcSC_ak1(N, a, b, a1, a3, a7, c, d, e, f, g, h, K, u, v, qk)
                nextK_ak1(N, tFlag, a, b, a1, a3, a7, K, qk, qp)
                tFlag = calcSC_ak1(N, a, b, a1, a3, a7, c, d, e, f, g, h, K, u, v, qk)
                newest_ak1(tFlag, ui, vi, a, a1, a3, a7, b, c, d, f, g, h, u, v, K, N, p)

                ' If vi is zero, the iteration is not converging
                If (vi <> 0) Then
                    relstp = Math.Abs((-v + vi) / vi)
                    u = ui
                    v = vi
                End If
            Loop While (vi <> 0)
        End Sub

        Private Sub RealIT_ak1(ByRef iFlag As Integer, ByRef NZ As Integer, ByRef sss As Double, ByVal N As Integer, ByVal p() As Double, ByVal NN As Integer, ByVal qp() As Double, ByRef szr As Double, ByRef szi As Double, ByVal K() As Double, ByVal qk() As Double)

            ' Variable-shift H-polynomial iteration for a real zero

            ' sss - starting iterate
            ' NZ - number of zeros found
            ' iFlag - flag to indicate a pair of zeros near real axis
            Dim i, j, nm1 As Integer
            j = 0
            nm1 = N - 1
            Dim ee, kv, mp, ms, omp, pv, s, t As Double

            iFlag = 0
            NZ = 0
            s = sss

            Do
                pv = p(0)

                ' Evaluate p at s
                qp(0) = pv
                For i = 1 To NN - 1
                    qp(i) = pv * s + p(i)
                    pv = pv * s + p(i)
                Next
                mp = Math.Abs(pv)

                ' Compute a rigorous bound on the error in evaluating p
                ms = Math.Abs(s)
                ee = 0.5 * Math.Abs(qp(0))
                For i = 1 To NN - 1
                    ee = ee * ms + Math.Abs(qp(i))
                Next

                ' Iteration has converged sufficiently if the polynomial value is less than
                ' 20 times this bound
                If (mp <= 20.0 * DBL_EPSILON * (2.0 * ee - mp)) Then
                    NZ = 1
                    szr = s
                    szi = 0.0
                    Exit Do
                End If

                j += 1

                ' Stop iteration after 10 steps
                If (j > 10) Then Exit Do

                If (j >= 2) Then
                    If ((Math.Abs(t) <= 0.001 * Math.Abs(-t + s)) And (mp > omp)) Then
                        ' A cluster of zeros near the real axis has been encountered                    ' Return with iFlag set to initiate a quadratic iteration

                        iFlag = 1
                        sss = s
                        Exit Do
                    End If

                End If

                ' Return if the polynomial value has increased significantly
                omp = mp

                ' Compute t, the next polynomial and the new iterate
                qk(0) = K(0)
                kv = K(0)
                For i = 1 To N - 1
                    kv = kv * s + K(i)
                    qk(i) = kv
                Next
                If (Math.Abs(kv) > Math.Abs(K(nm1)) * 10.0 * DBL_EPSILON) Then
                    ' Use the scaled form of the recurrence if the value of K at s is non-zero
                    t = -(pv / kv)
                    K(0) = qp(0)
                    For i = 1 To N - 1
                        K(i) = t * qk(i - 1) + qp(i)
                    Next
                Else
                    ' Use unscaled form
                    K(0) = 0.0
                    For i = 1 To N - 1
                        K(i) = qk(i - 1)
                    Next
                End If

                kv = K(0)
                For i = 1 To N - 1
                    kv = kv * s + K(i)
                Next

                If (Math.Abs(kv) > (Math.Abs(K(nm1)) * 10.0 * DBL_EPSILON)) Then
                    t = -(pv / kv)
                Else
                    t = 0.0
                End If

                s += t

            Loop While True
        End Sub

        Private Sub Quad_ak1(ByVal a As Double, ByVal b1 As Double, ByVal c As Double, ByRef sr As Double, ByRef si As Double, ByRef lr As Double, ByRef li As Double)
            ' Calculates the zeros of the quadratic a*Z^2 + b1*Z + c
            ' The quadratic formula, modified to avoid overflow, is used to find the larger zero if the
            ' zeros are real and both zeros are complex. The smaller real zero is found directly from
            ' the product of the zeros c/a.

            Dim b, d, e As Double

            sr = 0
            si = 0
            lr = 0
            li = 0.0

            If a = 0 Then
                If b1 = 0 Then
                    sr = -c / b1
                End If
            End If

            If c = 0 Then
                lr = -b1 / a
            End If

            'Compute discriminant avoiding overflow
            b = b1 / 2.0

            If Math.Abs(b) < Math.Abs(c) Then
                If c >= 0 Then
                    e = a
                Else
                    e = -a
                End If

                e = -e + b * (b / Math.Abs(c))
                d = Math.Sqrt(Math.Abs(e)) * Math.Sqrt(Math.Abs(c))
            Else
                e = -((a / b) * (c / b)) + 1.0
                d = Math.Sqrt(Math.Abs(e)) * (Math.Abs(b))
            End If



            If (e >= 0) Then
                ' Real zero
                If b >= 0 Then
                    d *= -1
                End If
                lr = (-b + d) / a

                If lr <> 0 Then
                    sr = (c / (lr)) / a
                End If
            Else
                ' Complex conjugate zeros
                lr = -(b / a)
                sr = -(b / a)
                si = Math.Abs(d / a)
                li = -(si)
            End If

        End Sub

    End Class

    Public Class ComplexPolynomialRootFinder

        Dim sr, si, tr, ti, pvr, pvi, are, mre, eta, infin As Double
        Dim nn As Integer

        'Global variables that assist the computation, taken from the Visual Studio C++ compiler class float
        Dim DBL_EPSILON As Double = 0.00000000000000022204460492503131 ' smallest such that 1.0+DBL_EPSILON != 1.0 
        Dim DBL_MAX As Double = 1.7976931348623157E+308 ' max value 
        Dim DBL_MIN As Double = 2.2250738585072014E-308 ' min positive value 
        Dim DBL_RADIX As Double = 2                       ' exponent radix 

        'If needed, set the maximum allowed degree for the polynomial here:
        Dim Max_Degree_Polynomial As Integer = 100

        'It is done to allocate memory for the computation arrays, so be careful to not set i too high, though in practice it should not be a problem as it is now.
        Dim Degree As Integer

        ' Allocate arrays
        Dim pr(Max_Degree_Polynomial + 1) As Double
        Dim pi(Max_Degree_Polynomial + 1) As Double
        Dim hr(Max_Degree_Polynomial + 1) As Double
        Dim hi(Max_Degree_Polynomial + 1) As Double
        Dim qpr(Max_Degree_Polynomial + 1) As Double
        Dim qpi(Max_Degree_Polynomial + 1) As Double
        Dim qhr(Max_Degree_Polynomial + 1) As Double
        Dim qhi(Max_Degree_Polynomial + 1) As Double
        Dim shr(Max_Degree_Polynomial + 1) As Double
        Dim shi(Max_Degree_Polynomial + 1) As Double

        Public Function FindRoots(ByVal ParamArray Input() As Complex) As List(Of Complex)

            Dim result As New List(Of Complex)

            Dim idnn2, conv As Integer
            Dim xx, yy, cosr, sinr, smalno, base, xxx, zr, zi, bnd As Double

            '     const double *opr, const double *opi, int degree, double *zeror, double *zeroi
            'Helper variable that indicates the maximum length of the polynomial array
            Dim Max_Degree_Helper As Integer = Max_Degree_Polynomial + 1

            'Actual degree calculated from the items in the Input ParamArray
            Dim Degree As Integer = Input.Length - 1

            'Are the polynomial larger that the maximum allowed?
            If Degree > Max_Degree_Polynomial Then
                Throw New Exception("The entered Degree is greater than MAXDEGREE. Exiting root finding algorithm. No further action taken.")
            End If

            Dim opr(Degree + 1) As Double
            Dim opi(Degree + 1) As Double
            Dim zeror(Degree + 1) As Double
            Dim zeroi(Degree + 1) As Double

            For i As Integer = 0 To Input.Length - 1
                opr(i) = Input(i).Real
                opi(i) = Input(i).Imaginary
            Next

            mcon(eta, infin, smalno, base)
            are = eta
            mre = 2.0 * Math.Sqrt(2.0) * eta
            xx = 0.70710678
            yy = -xx
            cosr = -0.060756474
            sinr = -0.99756405
            nn = Degree

            ' Algorithm fails if the leading coefficient is zero
            If (opr(0) = 0 And opi(0) = 0) Then
                Throw New Exception("The leading coefficient is zero. No further action taken. Program terminated.")
            End If


            ' Remove the zeros at the origin if any
            While (opr(nn) = 0 And opi(nn) = 0)
                idnn2 = Degree - nn
                zeror(idnn2) = 0
                zeroi(idnn2) = 0
                nn -= 1
            End While

            ' Make a copy of the coefficients
            For i As Integer = 0 To nn
                pr(i) = opr(i)
                pi(i) = opi(i)
                shr(i) = cmod(pr(i), pi(i))
            Next

            ' Scale the polynomial
            bnd = scale(nn, shr, eta, infin, smalno, base)
            If (bnd <> 1) Then
                For i As Integer = 0 To nn
                    pr(i) *= bnd
                    pi(i) *= bnd
                Next
            End If

search:
            If (nn <= 1) Then
                cdivid(-pr(1), -pi(1), pr(0), pi(0), zeror(Degree - 1), zeroi(Degree - 1))

                For i As Integer = 0 To Degree - 1
                    result.Add(New Complex(zeror(i), zeroi(i)))
                Next
                Return result

            End If

            ' Calculate bnd, alower bound on the modulus of the zeros
            For i As Integer = 0 To nn
                shr(i) = cmod(pr(i), pi(i))
            Next

            cauchy(nn, shr, shi, bnd)


            ' Outer loop to control 2 Major passes with different sequences of shifts
            For cnt1 As Integer = 1 To 2
                ' First stage  calculation , no shift
                noshft(5)

                ' Inner loop to select a shift
                For cnt2 As Integer = 1 To 9
                    ' Shift is chosen with modulus bnd and amplitude rotated by 94 degree from the previous shif
                    xxx = cosr * xx - sinr * yy
                    yy = sinr * xx + cosr * yy
                    xx = xxx
                    sr = bnd * xx
                    si = bnd * yy

                    ' Second stage calculation, fixed shift
                    fxshft(10 * cnt2, zr, zi, conv)
                    If (conv) Then
                        ' The second stage jumps directly to the third stage ieration
                        ' If successful the zero is stored and the polynomial deflated
                        idnn2 = Degree - nn
                        zeror(idnn2) = zr
                        zeroi(idnn2) = zi
                        nn -= 1
                        For i As Integer = 0 To nn
                            pr(i) = qpr(i)
                            pi(i) = qpi(i)
                        Next

                        GoTo search
                    End If
                    ' If the iteration is unsuccessful another shift is chosen
                Next
                ' if 9 shifts fail, the outer loop is repeated with another sequence of shifts
            Next

            ' The zerofinder has failed on two major passes
            ' return empty handed with the number of roots found (less than the original degree)
            Degree -= nn


            For i As Integer = 0 To Degree - 1
                result.Add(New Complex(zeror(i), zeroi(i)))
            Next

            Throw New Exception("The program could not converge to find all the zeroes, but a prelimenary result with the ones that are found is returned.")

            Return result

        End Function


        ' COMPUTES  THE DERIVATIVE  POLYNOMIAL AS THE INITIAL H
        ' POLYNOMIAL AND COMPUTES L1 NO-SHIFT H POLYNOMIALS.
        '
        Private Sub noshft(ByVal l1 As Integer)
            Dim j, n, nm1 As Integer
            Dim xni, t1, t2 As Double

            n = nn
            nm1 = n - 1
            For i As Integer = 0 To n
                xni = nn - i
                hr(i) = xni * pr(i) / n
                hi(i) = xni * pi(i) / n
            Next
            For jj As Integer = 1 To l1
                If (cmod(hr(n - 1), hi(n - 1)) > eta * 10 * cmod(pr(n - 1), pi(n - 1))) Then
                    cdivid(-pr(nn), -pi(nn), hr(n - 1), hi(n - 1), tr, ti)
                    For i As Integer = 0 To nm1 - 1
                        j = nn - i - 1
                        t1 = hr(j - 1)
                        t2 = hi(j - 1)
                        hr(j) = tr * t1 - ti * t2 + pr(j)
                        hi(j) = tr * t2 + ti * t1 + pi(j)
                    Next
                    hr(0) = pr(0)
                    hi(0) = pi(0)

                Else
                    ' If the constant term is essentially zero, shift H coefficients
                    For i As Integer = 0 To nm1 - 1
                        j = nn - i - 1
                        hr(j) = hr(j - 1)
                        hi(j) = hi(j - 1)
                    Next
                    hr(0) = 0
                    hi(0) = 0
                End If
            Next
        End Sub

        ' COMPUTES L2 FIXED-SHIFT H POLYNOMIALS AND TESTS FOR CONVERGENCE.
        ' INITIATES A VARIABLE-SHIFT ITERATION AND RETURNS WITH THE
        ' APPROXIMATE ZERO IF SUCCESSFUL.
        ' L2 - LIMIT OF FIXED SHIFT STEPS
        ' ZR,ZI - APPROXIMATE ZERO IF CONV IS .TRUE.
        ' CONV  - LOGICAL INDICATING CONVERGENCE OF STAGE 3 ITERATION
        '
        Private Sub fxshft(ByVal l2 As Integer, ByRef zr As Double, ByRef zi As Double, ByRef conv As Integer)
            Dim n As Integer
            Dim test, pasd, bol As Integer
            Dim otr, oti, svsr, svsi As Double

            n = nn
            polyev(nn, sr, si, pr, pi, qpr, qpi, pvr, pvi)
            test = 1
            pasd = 0

            ' Calculate first T = -P(S)/H(S)
            calct(bol)

            ' Main loop for second stage
            For j As Integer = 1 To l2
                otr = tr
                oti = ti

                ' Compute the next H Polynomial and new t
                nexth(bol)
                calct(bol)
                zr = sr + tr
                zi = si + ti

                ' Test for convergence unless stage 3 has failed once or this
                ' is the last H Polynomial
                If (Not (bol Or Not test Or j = 12)) Then
                    If (cmod(tr - otr, ti - oti) < 0.5 * cmod(zr, zi)) Then
                        If (pasd) Then

                            ' The weak convergence test has been passwed twice, start the third stage
                            ' Iteration, after saving the current H polynomial and shift
                            For i As Integer = 0 To n - 1
                                shr(i) = hr(i)
                                shi(i) = hi(i)
                            Next
                            svsr = sr
                            svsi = si
                            vrshft(10, zr, zi, conv)
                            If (conv) Then Return

                            'The iteration failed to converge. Turn off testing and restore h,s,pv and T
                            test = 0
                            For i As Integer = 0 To n - 1
                                hr(i) = shr(i)
                                hi(i) = shi(i)
                            Next
                            sr = svsr
                            si = svsi
                            polyev(nn, sr, si, pr, pi, qpr, qpi, pvr, pvi)
                            calct(bol)
                            Continue For
                        End If
                        pasd = 1
                    End If
                Else
                    pasd = 0
                End If
            Next

            ' Attempt an iteration with final H polynomial from second stage
            vrshft(10, zr, zi, conv)
        End Sub

        ' CARRIES OUT THE THIRD STAGE ITERATION.
        ' L3 - LIMIT OF STEPS IN STAGE 3.
        ' ZR,ZI   - ON ENTRY CONTAINS THE INITIAL ITERATE, IF THE
        '           ITERATION CONVERGES IT CONTAINS THE FINAL ITERATE ON EXIT.
        ' CONV    -  .TRUE. IF ITERATION CONVERGES
        '
        Private Sub vrshft(ByVal l3 As Integer, ByRef zr As Double, ByRef zi As Double, ByRef conv As Integer)
            Dim b, bol As Integer
            ' Int(i, j)

            Dim mp, ms, omp, relstp, r1, r2, tp As Double

            conv = 0
            b = 0
            sr = zr
            si = zi

            ' Main loop for stage three
            For i As Integer = 1 To l3

                ' Evaluate P at S and test for convergence
                polyev(nn, sr, si, pr, pi, qpr, qpi, pvr, pvi)
                mp = cmod(pvr, pvi)
                ms = cmod(sr, si)
                If (mp <= 20 * errev(nn, qpr, qpi, ms, mp, are, mre)) Then
                    ' Polynomial value is smaller in value than a bound onthe error
                    ' in evaluationg P, terminate the ietartion
                    conv = 1
                    zr = sr
                    zi = si
                    Return
                End If
                If (i <> 1) Then
                    If (Not (b Or mp < omp Or relstp >= 0.05)) Then

                        ' Iteration has stalled. Probably a cluster of zeros. Do 5 fixed 
                        ' shift steps into the cluster to force one zero to dominate
                        tp = relstp
                        b = 1
                        If (relstp < eta) Then tp = eta
                        r1 = Math.Sqrt(tp)
                        r2 = sr * (1 + r1) - si * r1
                        si = sr * r1 + si * (1 + r1)
                        sr = r2
                        polyev(nn, sr, si, pr, pi, qpr, qpi, pvr, pvi)
                        For j As Integer = 1 To 5
                            calct(bol)
                            nexth(bol)
                        Next

                        omp = infin
                        GoTo _20
                    End If

                    ' Exit if polynomial value increase significantly
                    If (mp * 0.1 > omp) Then Return
                End If

                omp = mp

                ' Calculate next iterate
_20:            calct(bol)
                nexth(bol)
                calct(bol)
                If (Not bol) Then
                    relstp = cmod(tr, ti) / cmod(sr, si)
                    sr += tr
                    si += ti
                End If
            Next
        End Sub

        ' COMPUTES  T = -P(S)/H(S).
        ' BOOL   - LOGICAL, SET TRUE IF H(S) IS ESSENTIALLY ZERO.
        Private Sub calct(ByRef bol As Integer)
            ' Int(n)
            Dim n As Integer
            Dim hvr, hvi As Double

            n = nn

            ' evaluate h(s)
            polyev(n - 1, sr, si, hr, hi, qhr, qhi, hvr, hvi)

            If cmod(hvr, hvi) <= are * 10 * cmod(hr(n - 1), hi(n - 1)) Then
                bol = 1
            Else
                bol = 0
            End If

            If (Not bol) Then
                cdivid(-pvr, -pvi, hvr, hvi, tr, ti)
                Exit Sub
            End If

            tr = 0
            ti = 0
        End Sub

        ' CALCULATES THE NEXT SHIFTED H POLYNOMIAL.
        ' BOOL   -  LOGICAL, IF .TRUE. H(S) IS ESSENTIALLY ZERO
        '
        Private Sub nexth(ByVal bol As Integer)

            Dim n As Integer
            Dim t1, t2 As Double

            n = nn
            If (Not bol) Then
                For j As Integer = 1 To n - 1
                    t1 = qhr(j - 1)
                    t2 = qhi(j - 1)
                    hr(j) = tr * t1 - ti * t2 + qpr(j)
                    hi(j) = tr * t2 + ti * t1 + qpi(j)
                Next
                hr(0) = qpr(0)
                hi(0) = qpi(0)
                Return
            End If

            ' If h(s) is zero replace H with qh
            For j As Integer = 1 To n - 1

                hr(j) = qhr(j - 1)
                hi(j) = qhi(j - 1)
            Next
            hr(0) = 0
            hi(0) = 0
        End Sub

        ' EVALUATES A POLYNOMIAL  P  AT  S  BY THE HORNER RECURRENCE
        ' PLACING THE PARTIAL SUMS IN Q AND THE COMPUTED VALUE IN PV.
        '  
        Private Sub polyev(ByVal nn As Integer, ByVal sr As Double, ByVal si As Double, ByVal pr() As Double, ByVal pi() As Double, ByVal qr() As Double, ByVal qi() As Double, ByRef pvr As Double, ByRef pvi As Double)
            '{
            '     Int(i)
            Dim t As Double

            qr(0) = pr(0)
            qi(0) = pi(0)
            pvr = qr(0)
            pvi = qi(0)

            For i As Integer = 1 To nn
                t = (pvr) * sr - (pvi) * si + pr(i)
                pvi = (pvr) * si + (pvi) * sr + pi(i)
                pvr = t
                qr(i) = pvr
                qi(i) = pvi
            Next
        End Sub

        ' BOUNDS THE ERROR IN EVALUATING THE POLYNOMIAL BY THE HORNER RECURRENCE.
        ' QR,QI - THE PARTIAL SUMS
        ' MS    -MODULUS OF THE POINT
        ' MP    -MODULUS OF POLYNOMIAL VALUE
        ' ARE, MRE -ERROR BOUNDS ON COMPLEX ADDITION AND MULTIPLICATION
        '
        Private Function errev(ByVal nn As Integer, ByVal qr() As Double, ByVal qi() As Double, ByVal ms As Double, ByVal mp As Double, ByVal are As Double, ByVal mre As Double) As Double
            '{
            '     Int(i)
            Dim e As Double

            e = cmod(qr(0), qi(0)) * mre / (are + mre)
            For i As Integer = 0 To nn
                e = e * ms + cmod(qr(i), qi(i))
            Next

            Return e * (are + mre) - mp * mre
        End Function

        ' CAUCHY COMPUTES A LOWER BOUND ON THE MODULI OF THE ZEROS OF A
        ' POLYNOMIAL - PT IS THE MODULUS OF THE COEFFICIENTS.
        '
        Private Sub cauchy(ByVal nn As Integer, ByVal pt() As Double, ByVal q() As Double, ByRef fn_val As Double)
            Dim n As Integer
            Dim x, xm, f, dx, df As Double

            pt(nn) = -pt(nn)

            ' Compute upper estimate bound
            n = nn
            x = Math.Exp(Math.Log(-pt(nn)) - Math.Log(pt(0))) / n
            If (pt(n - 1) <> 0) Then
                '// Newton step at the origin is better, use it
                xm = -pt(nn) / pt(n - 1)
                If (xm < x) Then x = xm
            End If

            ' Chop the interval (0,x) until f < 0
            While (1)

                xm = x * 0.1
                f = pt(0)
                For i As Integer = 1 To nn
                    f = f * xm + pt(i)
                Next
                If (f <= 0) Then Exit While
                x = xm
            End While
            dx = x

            ' Do Newton iteration until x converges to two decimal places
            While (Math.Abs(dx / x) > 0.005)
                q(0) = pt(0)
                For i As Integer = 1 To nn
                    q(i) = q(i - 1) * x + pt(i)
                Next
                f = q(nn)
                df = q(0)
                For i As Integer = 1 To n - 1
                    df = df * x + q(i)
                Next
                dx = f / df
                x -= dx
            End While

            fn_val = x
        End Sub

        ' RETURNS A SCALE FACTOR TO MULTIPLY THE COEFFICIENTS OF THE POLYNOMIAL.
        ' THE SCALING IS DONE TO AVOID OVERFLOW AND TO AVOID UNDETECTED UNDERFLOW
        ' INTERFERING WITH THE CONVERGENCE CRITERION.  THE FACTOR IS A POWER OF THE
        ' BASE.
        ' PT - MODULUS OF COEFFICIENTS OF P
        ' ETA, INFIN, SMALNO, BASE - CONSTANTS DESCRIBING THE FLOATING POINT ARITHMETIC.
        '
        Private Function scale(ByVal nn As Integer, ByVal pt() As Double, ByVal eta As Double, ByVal infin As Double, ByVal smalno As Double, ByVal base As Double) As Double
            '{
            '     Int(i, l)
            Dim l As Integer
            Dim hi, lo, max, min, x, sc As Double
            Dim fn_val As Double

            ' Find largest and smallest moduli of coefficients
            hi = Math.Sqrt(infin)
            lo = smalno / eta
            max = 0
            min = infin

            For i As Integer = 0 To nn
                x = pt(i)
                If (x > max) Then max = x
                If (x <> 0 And x < min) Then min = x
            Next

            ' Scale only if there are very large or very small components
            fn_val = 1
            If (min >= lo And max <= hi) Then Return fn_val
            x = lo / min
            If (x <= 1) Then
                sc = 1 / (Math.Sqrt(max) * Math.Sqrt(min))
            Else
                sc = x
                If (infin / sc > max) Then sc = 1
            End If
            l = CInt(Math.Log(sc) / Math.Log(base) + 0.5)
            fn_val = Math.Pow(base, l)
            Return fn_val
        End Function

        ' COMPLEX DIVISION C = A/B, AVOIDING OVERFLOW.
        '
        Private Sub cdivid(ByVal ar As Double, ByVal ai As Double, ByVal br As Double, ByVal bi As Double, ByRef cr As Double, ByRef ci As Double)
            Dim r, d, t, infin As Double

            If (br = 0 And bi = 0) Then
                ' Division by zero, c = infinity
                mcon(t, infin, t, t)
                cr = infin
                ci = infin
                Return
            End If

            If (Math.Abs(br) < Math.Abs(bi)) Then
                r = br / bi
                d = bi + r * br
                cr = (ar * r + ai) / d
                ci = (ai * r - ar) / d
                Return
            End If

            r = bi / br
            d = br + r * bi
            cr = (ar + ai * r) / d
            ci = (ai - ar * r) / d
        End Sub

        ' MODULUS OF A COMPLEX NUMBER AVOIDING OVERFLOW.
        '
        Private Function cmod(ByVal r As Double, ByVal i As Double) As Double
            Dim ar, ai As Double

            ar = Math.Abs(r)
            ai = Math.Abs(i)
            If (ar < ai) Then
                Return ai * Math.Sqrt(1.0 + Math.Pow((ar / ai), 2.0))

            ElseIf (ar > ai) Then
                Return ar * Math.Sqrt(1.0 + Math.Pow((ai / ar), 2.0))
            Else
                Return ar * Math.Sqrt(2.0)
            End If
        End Function
        ' MCON PROVIDES MACHINE CONSTANTS USED IN VARIOUS PARTS OF THE PROGRAM.
        ' THE USER MAY EITHER SET THEM DIRECTLY OR USE THE STATEMENTS BELOW TO
        ' COMPUTE THEM. THE MEANING OF THE FOUR CONSTANTS ARE -
        ' ETA       THE MAXIMUM RELATIVE REPRESENTATION ERROR WHICH CAN BE DESCRIBED
        '           AS THE SMALLEST POSITIVE FLOATING-POINT NUMBER SUCH THAT
        '           1.0_dp + ETA &gt; 1.0.
        ' INFINY    THE LARGEST FLOATING-POINT NUMBER
        ' SMALNO    THE SMALLEST POSITIVE FLOATING-POINT NUMBER
        ' BASE      THE BASE OF THE FLOATING-POINT NUMBER SYSTEM USED
        '
        Private Sub mcon(ByRef eta As Double, ByRef infiny As Double, ByRef smalno As Double, ByRef base As Double)

            base = DBL_RADIX
            eta = DBL_EPSILON
            infiny = DBL_MAX
            smalno = DBL_MIN
        End Sub

    End Class

End Namespace
