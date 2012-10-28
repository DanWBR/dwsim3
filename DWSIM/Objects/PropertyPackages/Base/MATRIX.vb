Module MATRIX
    Function Poly_Roots_QR(ByVal coeff)
        'returns all roots of a polynomial with QR algorithm
        Dim n&, Roots(,), c, Wr#(), Wi#(), Ierr
        Dim i, NM
        'build the companion matrix
        c = MatCmpn(coeff)
        n = UBound(c)
        ReDim Wr(0 To n), Wi(0 To n)
        'find eigenvalues of companion matrix
        NM = 0
        Ierr = 0
        HQR(NM, n, 1, n, c, Wr, Wi, Ierr)

        ReDim Roots(0 To n, 0 To 1)
        For i = 0 To n
            If i > Ierr Then
                Roots(i, 0) = Wr(i)
                Roots(i, 1) = Wi(i)
            Else
                Roots(i, 0) = "?"
                Roots(i, 1) = "?"
            End If
        Next

        Poly_Roots_QR = Roots
    End Function

    Function MatCmpn(ByVal Cf)
        'returns the companion matrix of a monic polynomial
        'mod. 26-7-03
        Dim c(,)
        Dim i
        'Cf = Coeff
        Dim m = UBound(Cf)
        Dim n = m - 1
        ReDim c(0 To n, 0 To n)
        'build subdiagonal (lower)
        For i = 0 To n - 1
            c(i + 1, i) = 1
        Next i
        'insert coefficients into last column
        For i = 0 To n
            c(i, n) = -Cf(i) / Cf(m)
        Next i
        MatCmpn = c
    End Function

    Sub HQR(ByVal NM, ByVal n, ByVal LOW, ByVal IGH, ByVal H, ByVal Wr, ByVal Wi, ByVal Ierr)
        '
        '     THIS SUBROUTINE IS A TRANSLATION OF THE ALGOL PROCEDURE HQR,
        '     NUM. MATH. 14, 219-231(1970) BY MARTIN, PETERS, AND WILKINSON.
        '     HANDBOOK FOR AUTO. COMP., VOL.II-LINEAR ALGEBRA, 359-371(1971).
        '
        '     THIS SUBROUTINE FINDS THE EIGENVALUES OF A REAL
        '     UPPER HESSENBERG MATRIX BY THE QR METHOD.
        '
        '     ON INPUT
        '
        '        NM MUST BE SET TO THE ROW DIMENSION OF TWO-DIMENSIONAL
        '          ARRAY PARAMETERS AS DECLARED IN THE CALLING PROGRAM
        '          DIMENSION STATEMENT.
        '
        '        N IS THE ORDER OF THE MATRIX.
        '
        '        LOW AND IGH ARE INTEGERS DETERMINED BY THE BALANCING
        '          SUBROUTINE  BALANC.  IF  BALANC  HAS NOT BEEN USED,
        '          SET LOW=1, IGH=N.
        '
        '        H CONTAINS THE UPPER HESSENBERG MATRIX.  INFORMATION ABOUT
        '          THE TRANSFORMATIONS USED IN THE REDUCTION TO HESSENBERG
        '          FORM BY  ELMHES  OR  ORTHES, IF PERFORMED, IS STORED
        '          IN THE REMAINING TRIANGLE UNDER THE HESSENBERG MATRIX.
        '
        '     ON OUTPUT
        '
        '        H HAS BEEN DESTROYED.  THEREFORE, IT MUST BE SAVED
        '          BEFORE CALLING  HQR  IF SUBSEQUENT CALCULATION AND
        '          BACK TRANSFORMATION OF EIGENVECTORS IS TO BE PERFORMED.
        '
        '        WR AND WI CONTAIN THE REAL AND IMAGINARY PARTS,
        '          RESPECTIVELY, OF THE EIGENVALUES.  THE EIGENVALUES
        '          ARE UNORDERED EXCEPT THAT COMPLEX CONJUGATE PAIRS
        '          OF VALUES APPEAR CONSECUTIVELY WITH THE EIGENVALUE
        '          HAVING THE POSITIVE IMAGINARY PART FIRST.  IF AN
        '          ERROR EXIT IS MADE, THE EIGENVALUES SHOULD BE CORRECT
        '          FOR INDICES IERR+1,...,N.
        '
        '        IERR IS SET TO
        '          ZERO       FOR NORMAL RETURN,
        '          J          IF THE LIMIT OF 30*N ITERATIONS IS EXHAUSTED
        '                     WHILE THE J-TH EIGENVALUE IS BEING SOUGHT.
        '
        '
        '     THIS VERSION DATED APRIL 1983.
        '
        '     ------------------------------------------------------------------
        Dim i&, j&, k&, L&, m&, EN&, LL&, mm&, na&, ITN&, ITS&, MP2&, ENM2&
        Dim P#, Q#, R#, s#, T#, W#, x#, y#, ZZ#, NORM#, TST1#, TST2#
        Dim NOTLAS As Boolean
        '
        Ierr = 0
        NORM = 0.0#
        k = 1
        '     .......... STORE ROOTS ISOLATED BY BALANC
        '                AND COMPUTE MATRIX NORM ..........
        For i = 0 To n
            '
            For j = k To n
                NORM = NORM + Math.Abs(H(i, j))
            Next j
            '
            k = i
            If ((i >= LOW) And (i <= IGH)) Then GoTo Lab50
            Wr(i) = H(i, i)
            Wi(i) = 0.0#
        Next i
Lab50:
        '
        EN = IGH
        T = 0.0#
        ITN = 30 * n
        '     .......... SEARCH FOR NEXT EIGENVALUES ..........
Lab60:
        If (EN < LOW) Then GoTo Lab1001
        ITS = 0
        na = EN - 1
        ENM2 = na - 1
        '     .......... LOOK FOR SINGLE SMALL SUB-DIAGONAL ELEMENT
        '                FOR L=EN STEP -1 UNTIL LOW DO -- ..........
Lab70:
        For LL = LOW To EN
            L = EN + LOW - LL
            If (L = LOW) Then GoTo Lab100
            s = Math.Abs(H(L - 1, L - 1)) + Math.Abs(H(L, L))
            If (s = 0) Then s = NORM
            TST1 = s
            TST2 = TST1 + Math.Abs(H(L, L - 1))
            If (TST2 = TST1) Then GoTo Lab100
        Next LL
        '     .......... FORM SHIFT ..........
Lab100:
        x = H(EN, EN)
        If (L = EN) Then GoTo Lab270
        y = H(na, na)
        W = H(EN, na) * H(na, EN)
        If (L = na) Then GoTo Lab280
        If (ITN = 0) Then GoTo Lab1000
        If ((ITS <> 10) And (ITS <> 20)) Then GoTo Lab130
        '     .......... FORM EXCEPTIONAL SHIFT ..........
        T = T + x
        '
        For i = LOW To EN
            H(i, i) = H(i, i) - x
        Next i
        '
        s = Math.Abs(H(EN, na)) + Math.Abs(H(na, ENM2))
        x = 0.75 * s
        y = x
        W = -0.4375 * s * s
Lab130:
        ITS = ITS + 1
        ITN = ITN - 1
        '     .......... LOOK FOR TWO CONSECUTIVE SMALL
        '                SUB-DIAGONAL ELEMENTS.
        '                FOR M=EN-2 STEP -1 UNTIL L DO -- ..........
        For mm = L To ENM2
            m = ENM2 + L - mm
            ZZ = H(m, m)
            R = x - ZZ
            s = y - ZZ
            P = (R * s - W) / H(m + 1, m) + H(m, m + 1)
            Q = H(m + 1, m + 1) - ZZ - R - s
            R = H(m + 2, m + 1)
            s = Math.Abs(P) + Math.Abs(Q) + Math.Abs(R)
            P = P / s
            Q = Q / s
            R = R / s
            If (m = L) Then GoTo Lab150
            TST1 = Math.Abs(P) * (Math.Abs(H(m - 1, m - 1)) + Math.Abs(ZZ) + Math.Abs(H(m + 1, m + 1)))
            TST2 = TST1 + Math.Abs(H(m, m - 1)) * (Math.Abs(Q) + Math.Abs(R))
            If (TST2 = TST1) Then GoTo Lab150
        Next mm
        '
Lab150:
        MP2 = m + 2
        '
        For i = MP2 To EN
            H(i, i - 2) = 0.0#
            If (i <> MP2) Then H(i, i - 3) = 0.0#
        Next i
        '     .......... DOUBLE QR STEP INVOLVING ROWS L TO EN AND
        '                COLUMNS M TO EN ..........
        For k = m To na
            NOTLAS = k <> na
            If (k = m) Then GoTo Lab170
            P = H(k, k - 1)
            Q = H(k + 1, k - 1)
            R = 0.0#
            If (NOTLAS) Then R = H(k + 2, k - 1)
            x = Math.Abs(P) + Math.Abs(Q) + Math.Abs(R)
            If (x = 0.0#) Then GoTo Lab260
            P = P / x
            Q = Q / x
            R = R / x
Lab170:
            s = dsign(Math.Sqrt(P * P + Q * Q + R * R), P)
            If (k = m) Then GoTo Lab180
            H(k, k - 1) = -s * x
            GoTo Lab190
Lab180:
            If (L <> m) Then H(k, k - 1) = -H(k, k - 1)
Lab190:
            P = P + s
            x = P / s
            y = Q / s
            ZZ = R / s
            Q = Q / P
            R = R / P
            If (NOTLAS) Then GoTo Lab225
            '     .......... ROW MODIFICATION ..........
            For j = k To n
                P = H(k, j) + Q * H(k + 1, j)
                H(k, j) = H(k, j) - P * x
                H(k + 1, j) = H(k + 1, j) - P * y
            Next j
            '
            j = Min(EN, k + 3)
            '     .......... COLUMN MODIFICATION ..........
            For i = 1 To j
                P = x * H(i, k) + y * H(i, k + 1)
                H(i, k) = H(i, k) - P
                H(i, k + 1) = H(i, k + 1) - P * Q
            Next i
            GoTo Lab255
Lab225:
            '     .......... ROW MODIFICATION ..........
            For j = k To n
                P = H(k, j) + Q * H(k + 1, j) + R * H(k + 2, j)
                H(k, j) = H(k, j) - P * x
                H(k + 1, j) = H(k + 1, j) - P * y
                H(k + 2, j) = H(k + 2, j) - P * ZZ
            Next j
            '
            j = Min(EN, k + 3)
            '     .......... COLUMN MODIFICATION ..........
            For i = 0 To j
                P = x * H(i, k) + y * H(i, k + 1) + ZZ * H(i, k + 2)
                H(i, k) = H(i, k) - P
                H(i, k + 1) = H(i, k + 1) - P * Q
                H(i, k + 2) = H(i, k + 2) - P * R
            Next i
Lab255:
            '
        Next k
Lab260:
        '
        GoTo Lab70
        '     .......... ONE ROOT FOUND ..........
Lab270:
        Wr(EN) = x + T
        Wi(EN) = 0.0#
        EN = na
        GoTo Lab60
        '     .......... TWO ROOTS FOUND ..........
Lab280:
        P = (y - x) / 2.0#
        Q = P * P + W
        ZZ = Math.Sqrt(Math.Abs(Q))
        x = x + T
        If (Q < 0.0#) Then GoTo Lab320
        '     .......... REAL PAIR ..........
        ZZ = P + dsign(ZZ, P)
        Wr(na) = x + ZZ
        Wr(EN) = Wr(na)
        If (ZZ <> 0.0#) Then Wr(EN) = x - W / ZZ
        Wi(na) = 0.0#
        Wi(EN) = 0.0#
        GoTo Lab330
        '     .......... COMPLEX PAIR ..........
Lab320:
        Wr(na) = x + P
        Wr(EN) = x + P
        Wi(na) = ZZ
        Wi(EN) = -ZZ
Lab330:
        EN = ENM2
        GoTo Lab60
        '     .......... SET ERROR -- ALL EIGENVALUES HAVE NOT
        '                CONVERGED AFTER 30*N ITERATIONS ..........
Lab1000:
        Ierr = EN
Lab1001:
    End Sub

    Private Function dsign(ByVal x, ByVal y)
        If y >= 0 Then
            dsign = Math.Abs(x)
        Else
            dsign = -Math.Abs(x)
        End If
    End Function

    Private Function Max(ByVal a, ByVal b)
        If a > b Then Max = a Else Max = b
    End Function

    Private Function Min(ByVal a, ByVal b)
        If a < b Then Min = a Else Min = b
    End Function

    Private Sub MatrixSort(ByVal a(,), ByVal Order)
        '==========================================
        'SORT Routine with Swapping Algorithm
        'by Leonardo Volpi   , Sept. 2000
        'A() may be matrix (N x M) or vector (N)
        'Sort is always based on the first column
        'Order = A (Ascending), D (Descending)
        '==========================================
        Dim flag_exchanged As Boolean
        Dim i_min = LBound(a, 1)
        Dim i_max = UBound(a, 1)
        Dim j_min = LBound(a, 2)
        Dim j_max = UBound(a, 2)

        Dim i, k, j, c

        'Sort algortithm begin
        Do
            flag_exchanged = False
            For i = i_min To i_max Step 2
                k = i + 1
                If k > i_max Then Exit For
                If (a(i, j_min) > a(k, j_min) And Order = "A") Or _
                   (a(i, j_min) < a(k, j_min) And Order = "D") Then
                    'swap rows
                    For j = j_min To j_max
                        c = a(k, j)
                        a(k, j) = a(i, j)
                        a(i, j) = c
                    Next j
                    flag_exchanged = True
                End If
            Next
            If i_min = LBound(a, j_min) Then
                i_min = LBound(a, j_min) + 1
            Else
                i_min = LBound(a, j_min)
            End If
        Loop Until flag_exchanged = False And i_min = LBound(a, j_min)

    End Sub
End Module