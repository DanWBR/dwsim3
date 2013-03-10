Imports Cudafy
Imports Cudafy.Translator
Imports Cudafy.Host

Public Class Broyden

    Shared Sub broydn(ByVal N As Object, ByVal X As Object, ByVal F As Object, ByRef P As Object, ByRef XB As Object, ByRef FB As Object, ByRef H As Object, ByRef IFLAG As Integer)
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
20:             Next J
                XB(I) = HY - P(I)
30:         Next I
            PTHY = 0.0#
            PTHF = 0.0#
            '
            For I = 0 To N ' DO 40 I=1,N
                PTH = 0.0#
                For J = 0 To N '  DO 35 J=1,N
                    PTH = PTH + P(J) * H(J, I)
35:             Next J
                PTHY = PTHY + PTH * (F(I) - FB(I))
                PTHF = PTHF + PTH * F(I)
                FB(I) = PTH
40:         Next I
            THETA = 1.0#
            '
            DENOM = (1.0# - THETA) * PTP + THETA * PTHY
            '
            For I = 0 To N ' DO 50 I=1,N
                For J = 0 To N ' DO 50 J=1,N
                    H(I, J) = H(I, J) - THETA * XB(I) * FB(J) / DENOM
                Next J
50:         Next I
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
70:     Next I
        ''
    End Sub

    Shared Sub broydnCUDA(ByVal N As Object, ByVal X As Double(), ByVal F As Double(), ByRef P As Double(), ByRef XB As Double(), ByRef FB As Double(), ByRef H As Double(,), ByRef IFLAG As Integer)

        Dim km As CudafyModule = CudafyTranslator.Cudafy()

        Dim gpu As GPGPU = CudafyHost.GetDevice(CudafyModes.Target)
        gpu.LoadModule(km)

        ' allocate the memory on the GPU
        Dim dev_x As Double() = gpu.Allocate(Of Double)(X)
        Dim dev_f As Double() = gpu.Allocate(Of Double)(F)
        Dim dev_p As Double() = gpu.Allocate(Of Double)(P)
        Dim dev_xb As Double() = gpu.Allocate(Of Double)(XB)
        Dim dev_fb As Double() = gpu.Allocate(Of Double)(FB)
        Dim dev_h As Double(,) = gpu.Allocate(Of Double)(H)

        ' copy the arrays to the GPU
        gpu.CopyToDevice(X, dev_x)
        gpu.CopyToDevice(F, dev_f)
        gpu.CopyToDevice(P, dev_p)
        gpu.CopyToDevice(XB, dev_xb)
        gpu.CopyToDevice(FB, dev_fb)
        gpu.CopyToDevice(H, dev_h)

        ' launch sub
        gpu.Launch().broydn2(N, dev_x, dev_f, dev_p, dev_xb, dev_fb, dev_h, IFLAG)

        ' copy the array back from the GPU to the CPU
        gpu.CopyFromDevice(dev_p, P)
        gpu.CopyFromDevice(dev_xb, XB)
        gpu.CopyFromDevice(dev_fb, FB)
        gpu.CopyFromDevice(dev_h, H)

        ' free the memory allocated on the GPU
        gpu.Free(dev_x)
        gpu.Free(dev_f)
        gpu.Free(dev_p)
        gpu.Free(dev_xb)
        gpu.Free(dev_fb)
        gpu.Free(dev_h)

    End Sub

    <Cudafy.Cudafy()> Public Shared Sub broydn2(ByVal N As Integer, ByVal X As Double(), ByVal F As Double(), ByVal P As Double(), ByVal XB As Double(), ByVal FB As Double(), ByVal H As Double(,), ByVal IFLAG As Integer)
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
                Next J
                XB(I) = HY - P(I)
            Next I
            PTHY = 0.0#
            PTHF = 0.0#
            '
            For I = 0 To N ' DO 40 I=1,N
                PTH = 0.0#
                For J = 0 To N '  DO 35 J=1,N
                    PTH = PTH + P(J) * H(J, I)
                Next J
                PTHY = PTHY + PTH * (F(I) - FB(I))
                PTHF = PTHF + PTH * F(I)
                FB(I) = PTH
            Next I
            THETA = 1.0#
            '
            DENOM = (1.0# - THETA) * PTP + THETA * PTHY
            '
            For I = 0 To N ' DO 50 I=1,N
                For J = 0 To N ' DO 50 J=1,N
                    H(I, J) = H(I, J) - THETA * XB(I) * FB(J) / DENOM
                Next J
            Next I
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
        Next I

    End Sub

End Class
