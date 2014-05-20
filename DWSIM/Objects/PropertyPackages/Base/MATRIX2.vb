Module Matrix2

    Public Roots(,)
    Public k

    '  Function PolyReg(ByVal xv, ByVal y, ByVal Degree)

    ''*** Polynomial Regression **
    ''Dim MP As New DLLXnumbers.Xnumbers
    'Dim x(,), c, r, N, i, j
    ''load data
    ''set the polynomial degree
    '    N = UBound(xv) + 1
    ''compute the x's powers
    '    ReDim x(0 To N - 1, 0 To Degree - 1)
    '    For j = 1 To Degree
    '        For i = 0 To N - 1
    '            x(i, j - 1) = xv(i) ^ j
    '    Next i, j
    '    With MP
    '        c = .xRegLinCoeff(y, x)
    '        r = .xRegLinStat(y, x, c)
    '    End With

    '    MP = Nothing
    '    PolyReg = c

    'End Function

    Function Poly_Roots(ByVal Coeff)
        'returns all roots of a polynomial
        'Dim tiny#, ErrMsg
        'Const Itermax = 1000, TRIALS = 5

        'Dim ErrMax = 10 ^ -14
        'tiny = ErrMax
        'ErrMsg = ""

        'If Not My.Application.CAPEOPENMode Then
        '    Poly_Roots = RootFinder_LB(Coeff, tiny, Itermax, TRIALS, ErrMsg)
        'Else
        Return CalcCubicRoots(Coeff(3), Coeff(2), Coeff(1), Coeff(0))
        'End If

    End Function

    ''' <summary>
    ''' Calculate roots of cubic equation, given cofficients.
    ''' </summary>
    ''' <param name="a"></param>
    ''' <param name="b"></param>
    ''' <param name="c"></param>
    ''' <param name="d"></param>
    ''' <returns>(2,1) array containing roots in first column and either -1 or 0 in second column depending on 
    ''' whether a root is found or not.</returns>
    ''' <remarks></remarks>
    Function CalcCubicRoots(ByVal a As Double, ByVal b As Double, ByVal c As Double, ByVal d As Double) As Double(,)
        Dim r1, i1, r2, i2, r3, i3 As Double

        r1 = FindCubicRoot(a, b, c, d, 0.01)
        If IsNothing(r1) Then : i1 = -1
        Else : i1 = 0
        End If

        r2 = FindCubicRoot(a, b, c, d, 0.5)
        If IsNothing(r2) Then : i2 = -1
        Else : i2 = 0
        End If

        r3 = FindCubicRoot(a, b, c, d, 0.99999999)
        If IsNothing(r3) Then : i3 = -1
        Else : i3 = 0
        End If

        Dim roots(2, 1) As Double
        roots(0, 0) = r1
        roots(0, 1) = i1
        roots(1, 0) = r2
        roots(1, 1) = i2
        roots(2, 0) = r3
        roots(2, 1) = i3
        Return roots
    End Function

    ''' <summary>
    ''' Finds one root of cubic equation, given cofficients and starting guess.
    ''' </summary>
    ''' <returns></returns>
    ''' <remarks></remarks>
    Function FindCubicRoot(ByVal a As Double, ByVal b As Double, ByVal c As Double, ByVal d As Double, ByVal guess As Double, Optional ByVal tol As Double = 0.00000001, Optional ByVal max_iterations As Integer = 1000) As Double
        Dim i As Integer
        Dim previous_guess As Double
        For i = 1 To max_iterations
            previous_guess = guess
            guess = guess - (a * guess ^ 3 + b * guess ^ 2 + c * guess + d) / (3 * a * guess ^ 2 + 2 * b * guess + c)
            If Math.Abs(guess - previous_guess) < tol Then Return guess
        Next
        Return Nothing
    End Function

    ''' <summary>
    ''' Sorts the roots given by Poly_Roots or CalcCubicRoots functions above, in ascending order.
    ''' </summary>
    ''' <param name="temp1"></param>
    ''' <returns></returns>
    ''' <remarks></remarks>
    Function SortRoots(ByVal temp1 As Double(,)) As Double(,)
        Dim tv As Double
        Dim tv2 As Double

        If temp1(0, 0) > temp1(1, 0) Then
            tv = temp1(1, 0)
            tv2 = temp1(1, 1)
            temp1(1, 0) = temp1(0, 0)
            temp1(0, 0) = tv
            temp1(1, 1) = temp1(0, 1)
            temp1(0, 1) = tv2
        End If
        If temp1(0, 0) > temp1(2, 0) Then
            tv = temp1(2, 0)
            temp1(2, 0) = temp1(0, 0)
            temp1(0, 0) = tv
            tv2 = temp1(2, 1)
            temp1(2, 1) = temp1(0, 1)
            temp1(0, 1) = tv2
        End If
        If temp1(1, 0) > temp1(2, 0) Then
            tv = temp1(2, 0)
            temp1(2, 0) = temp1(1, 0)
            temp1(1, 0) = tv
            tv2 = temp1(2, 1)
            temp1(2, 1) = temp1(1, 1)
            temp1(1, 1) = tv2
        End If
        Return temp1
    End Function

    Public Function RootFinder_LB(ByVal coeff, ByVal ErrMax, ByVal Itermax, ByVal TRIALS, Optional ByVal ErrMsg = "")
        'Coeff() = polynomial coefficients: coeff(0)=a0,coeff(1)=a1,...
        Dim coeffA#() 'coefficenti del polinomio dato
        Dim CoeffB#() 'coefficenti del polinomio ridotto
        Dim CoeffC#() 'coefficenti delle derivate parziali
        Dim i, ErrLoop, m
        Dim U#, V#, d#, Du#, Dv#, n&, k&, CountIter&
        Dim Start As Boolean
        n = UBound(coeff) 'polynomial degree
        ReDim coeffA(0 To n), CoeffB(0 To n), CoeffC(0 To n), Roots(0 To n - 1, 0 To 1)
        'Load and normalize coefficients -------
        ErrMsg = ""
        For i = 0 To n
            coeffA(n - i) = coeff(i) / coeff(n)
        Next
        CountIter = 0   'iterations counter
        k = 1           'roots counter
        Start = True
        Do While n > 2  'degree > 2
            Do
                If Start Then  'choose starting values u, v
                    V = Rnd() : U = coeffA(1) / n
                    Start = False
                End If
                'Generate coefficients of reduced polynomial
                CoeffB(0) = coeffA(0)
                CoeffB(1) = coeffA(1) + CoeffB(0) * U
                For i = 2 To n
                    CoeffB(i) = coeffA(i) + CoeffB(i - 1) * U + CoeffB(i - 2) * V
                Next
                'Generate coefficients of derivative polynomial
                CoeffC(0) = CoeffB(0)
                CoeffC(1) = CoeffB(1) + CoeffC(0) * U
                For i = 2 To n
                    CoeffC(i) = CoeffB(i) + CoeffC(i - 1) * U + CoeffC(i - 2) * V
                Next

                d = CoeffC(n - 2) ^ 2 - CoeffC(n - 1) * CoeffC(n - 3)
                Du = CoeffB(n) * CoeffC(n - 3) - CoeffB(n - 1) * CoeffC(n - 2)
                Dv = CoeffB(n - 1) * CoeffC(n - 1) - CoeffB(n) * CoeffC(n - 2)
                If d <> 0 Then
                    Du = Du / d
                    Dv = Dv / d
                End If
                U = Du + U
                V = Dv + V
                'check increment
                ErrLoop = (Math.Abs(Du) + Math.Abs(Dv)) / 2
                m = Math.Abs(U) + Math.Abs(V)
                If m > 1 Then ErrLoop = ErrLoop / m
                CountIter = CountIter + 1
                If CountIter > Itermax Then
                    Start = True            'try another starting point
                    TRIALS = TRIALS - 1     '
                    CountIter = 0             'reset counter
                End If
            Loop Until ErrLoop <= ErrMax Or TRIALS < 1

            If ErrLoop > 100 * ErrMax Then
                ErrMsg = "dubious accuracy"
                Roots(k - 1, 0) = "?" : Roots(k - 1, 1) = "?"
                RootFinder_LB = 0
                Exit Function
            End If

            Call SolvePoly2(U, V, k)
            k = k + 2
            n = n - 2
            For i = 0 To n
                coeffA(i) = CoeffB(i)
            Next
        Loop

        If n = 2 Then
            U = -coeffA(1) / coeffA(0)
            V = -coeffA(2) / coeffA(0)
            Call SolvePoly2(U, V, k)
        ElseIf n = 1 Then
            Roots(k - 1, 0) = -coeffA(1)
            Roots(k - 1, 1) = 0
        End If

        RootFinder_LB = Roots

    End Function

    Public Sub SolvePoly2(ByVal U, ByVal V, ByVal k)
        'Roots of 2° degree normalized polynomial  P(x)= x^2-u*x-v ,
        Dim delta = U ^ 2 + 4 * V
        Dim X1, Y1, X2, Y2
        If delta < 0 Then
            X1 = U / 2
            X2 = X1
            Y1 = Math.Sqrt(-delta) / 2
            Y2 = -Y1
        Else
            X1 = U / 2 - Math.Sqrt(delta) / 2
            X2 = U / 2 + Math.Sqrt(delta) / 2
            Y1 = 0
            Y2 = 0
        End If

        Dim tmp(1, 1)
        Roots(k - 1, 0) = X1
        Roots(k - 1, 1) = Y1
        Roots(k, 0) = X2
        Roots(k, 1) = Y2

    End Sub

End Module
